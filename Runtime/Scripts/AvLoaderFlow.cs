// Copyright 2026 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Class supporting a complete avatar loader flow with fallbacks for each step.
    /// </summary>
    public class AvLoaderFlow
    {
        /// <summary>
        /// The loaders to use in the flow, in priority order.
        /// </summary>
        /// <remarks>
        /// Index 0 is used first; if it cannot handle the data,
        /// the flow falls back to index 1, then 2, and so on.
        /// </remarks>
        public readonly IReadOnlyCollection<IAvDataLoader> DataLoaders;

        /// <summary>
        /// The importers to use in the flow, in priority order.
        /// </summary>
        /// <remarks>
        /// Index 0 is used first; if it cannot handle the model format,
        /// the flow falls back to index 1, then 2, and so on.
        /// </remarks>
        public readonly IReadOnlyCollection<IAvImporter> Importers;

        /// <summary>
        /// Post-processors to run after loading the avatar.
        /// </summary>
        public IEnumerable<IAvPostProcessor> PostProcessors = Array.Empty<IAvPostProcessor>();
        
        /// <summary>
        /// Async post-processors to run after loading the avatar.
        /// </summary>
        public IEnumerable<IAvAsyncPostProcessor> AsyncPostProcessors = Array.Empty<IAvAsyncPostProcessor>();

        public AvLoaderFlow(IReadOnlyCollection<IAvDataLoader> dataLoaders, IReadOnlyCollection<IAvImporter> importers)
        {
            if (dataLoaders.Count == 0) throw new ArgumentException("Zero data loaders defined for flow.", nameof(dataLoaders));
            if (importers.Count == 0) throw new ArgumentException("Zero importers defined for flow.", nameof(importers));

            DataLoaders = dataLoaders;
            Importers = importers;
        }

        /// <summary>
        /// Runs the loader flow completely.
        /// </summary>
        /// <returns>The loaded avatar.</returns>
        public async Awaitable<LoadedAv> RunFlowAsync(CancellationToken token = default)
        {
            AvDataContainer rawData = (await LoadAvatarDataAsync(true, token))!;
            LoadedAv avatar = (await ImportAvatarAsync(rawData, true, token))!;
            await RunPostProcessorsAndDisposeAvOnFailAsync(avatar, rawData, true, token);

            avatar.GameObject.SetActive(true);
            return avatar;
        }

        /// <summary>
        /// Tries to run the loader flow completely.
        /// </summary>
        /// <returns>The loaded avatar if the entire flow completed successfully; <see langword="null"/> otherwise.</returns>
        public async Awaitable<LoadedAv?> TryRunFlowAsync(CancellationToken token = default)
        {
            AvDataContainer? rawData = await LoadAvatarDataAsync(false, token);
            if (rawData is null) return null;

            LoadedAv? avatar = await ImportAvatarAsync(rawData, true, token);
            if (avatar is null) return null;

            if (!await RunPostProcessorsAndDisposeAvOnFailAsync(avatar, rawData, true, token))
                return null;

            avatar.GameObject.SetActive(true);
            return avatar;
        }

        private async Awaitable<AvDataContainer?> LoadAvatarDataAsync(bool throwOnFinalFail, CancellationToken token)
        {
            int counter = 0;
            int failAtIdx = DataLoaders.Count - 1;
            foreach (IAvDataLoader loader in DataLoaders)
            {
                if (await loader.LoadAvatarAsync(throwOnFinalFail && counter == failAtIdx, token) is AvDataContainer avData)
                    return avData;
            }

            return null;
        }

        private async Awaitable<LoadedAv?> ImportAvatarAsync(AvDataContainer rawData, bool throwOnFinalFail, CancellationToken token)
        {
            int counter = 0;
            int failAtIdx = Importers.Count - 1;
            foreach (IAvImporter importer in Importers)
            {
                if (importer.SupportsFormat(rawData.ModelFormat)
                    && await importer.ImportAvatarAsync(rawData, throwOnFinalFail && counter == failAtIdx, token) is LoadedAv avatar)
                    return avatar;
            }

            return throwOnFinalFail
                ? throw new NotSupportedException($"No importer defined that supports format '{rawData.ModelFormat}'.")
                : null;
        }


        private async Awaitable<bool> RunPostProcessorsAndDisposeAvOnFailAsync(LoadedAv avatar, AvDataContainer rawData, bool throwOnFail, CancellationToken token)
        {
            try
            {
                foreach (IAvPostProcessor postProcessor in PostProcessors)
                    postProcessor.PostProcess(avatar, rawData);

                foreach (IAvAsyncPostProcessor asyncPostProcessor in AsyncPostProcessors)
                    await asyncPostProcessor.PostProcessAsync(avatar, rawData, token);
                
                return true;
            }
            catch (Exception ex)
            {
                avatar.Dispose();
                if (throwOnFail) throw;

                Debug.LogWarning($"{nameof(AvLoaderFlow)}: Could not load avatar due to post-processor exception:\n{ex}");
                return false;
            }
        }
    }
}