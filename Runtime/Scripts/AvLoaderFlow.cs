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
        public readonly IReadOnlyList<IAvDataLoader> DataLoaders;

        /// <summary>
        /// The importers to use in the flow, in priority order.
        /// </summary>
        /// <remarks>
        /// Index 0 is used first; if it cannot handle the model format,
        /// the flow falls back to index 1, then 2, and so on.
        /// </remarks>
        public readonly IReadOnlyList<IAvImporter> Importers;

        /// <summary>
        /// Post-processors to run after loading the avatar.
        /// </summary>
        public IReadOnlyList<IAvPostProcessor> PostProcessors = Array.Empty<IAvPostProcessor>();
        
        /// <summary>
        /// Async post-processors to run after loading the avatar.
        /// </summary>
        public IReadOnlyList<IAvAsyncPostProcessor> AsyncPostProcessors = Array.Empty<IAvAsyncPostProcessor>();

        public AvLoaderFlow(IReadOnlyList<IAvDataLoader> dataLoaders, IReadOnlyList<IAvImporter> importers)
        {
            if (dataLoaders.Count == 0) throw new ArgumentException("Zero data loaders defined for flow.", nameof(dataLoaders));
            if (importers.Count == 0) throw new ArgumentException("Zero importers defined for flow.", nameof(importers));

            DataLoaders = dataLoaders;
            Importers = importers;
        }

        public async Awaitable<ILoadedAv> RunFlowAsync(CancellationToken token = default)
        {
            AvDataContainer? container = null;
            int loadersCount = DataLoaders.Count;
            for (int i = 0; i < loadersCount; i++)
            {
                IAvDataLoader loader = DataLoaders[i];
                container = await loader.LoadAvatarAsync(i == loadersCount - 1, token);
                if (container is not null)
                {
                Debug.Log($"LOADED USING: {loader.GetType()}");
                    break;
                }
            }

            ILoadedAv? loadedAvatar = null;
            int importersCount = Importers.Count;
            for (int i = 0; i < importersCount; i++)
            {
                IAvImporter importer = Importers[i];
                loadedAvatar = await importer.ImportAvatarAsync(container!, i == importersCount - 1, token);
                if (loadedAvatar is not null)
                    break;
            }

            foreach (IAvPostProcessor postProcessor in PostProcessors)
                postProcessor.PostProcess(loadedAvatar!);

            foreach (IAvAsyncPostProcessor asyncPostProcessor in AsyncPostProcessors)
                await asyncPostProcessor.PostProcessAsync(loadedAvatar!);

            loadedAvatar!.GameObject.SetActive(true);
            return loadedAvatar;
        }
    }
}