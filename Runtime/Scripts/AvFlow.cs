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
    public class AvFlow
    {
        /// <summary>
        /// Fluent builder for <see cref="AvFlow"/>.
        /// </summary>
        public sealed class Builder
        {
            private readonly List<IAvDataLoader> _loaders = new();
            private readonly List<IAvImporter> _importers = new();

            private readonly List<IAvPostProcessor> _postProcessors = new();
            private readonly List<IAsyncAvPostProcessor> _asyncPostProcessors = new();

            /// <summary>Adds multiple avatar data loaders to the flow.</summary>
            /// <remarks>Loaders are tried in the order they are added until one succeeds.</remarks>
            public Builder From(params IAvDataLoader[] loaders)
            {
                _loaders.AddRange(loaders);
                return this;
            }

            /// <summary>Adds an avatar data loader to the flow.</summary>
            /// <remarks>Loaders are tried in the order they are added until one succeeds.</remarks>
            public Builder From(IAvDataLoader loader)
            {
                _loaders.Add(loader);
                return this;
            }

            /// <summary>Adds multiple avatar importers to the flow.</summary>
            /// <remarks>Importers are evaluated based on format support, then tried in the order they are added until one succeeds.</remarks>
            public Builder Using(params IAvImporter[] importers)
            {
                _importers.AddRange(importers);
                return this;
            }

            /// <summary>Adds an avatar importer to the flow.</summary>
            /// <remarks>Importers are evaluated based on format support, then tried in the order they are added until one succeeds.</remarks>
            public Builder Using(IAvImporter importer)
            {
                _importers.Add(importer);
                return this;
            }

            /// <summary>Adds multiple post-processors to the flow.</summary>
            /// <remarks>
            /// Note that <b>all</b> sync post-processors run <i>before</i> <b>all</b> async
            /// post-processors, regardless of order of addition in the builder.
            /// </remarks>
            public Builder ProcessWith(params IAvPostProcessor[] postProcessors)
            {
                _postProcessors.AddRange(postProcessors);
                return this;
            }

            /// <summary>Adds a post-processor to the flow.</summary>
            /// <remarks>
            /// Note that <b>all</b> sync post-processors run <i>before</i> <b>all</b> async
            /// post-processors, regardless of order of addition in the builder.
            /// </remarks>
            public Builder ProcessWith(IAvPostProcessor postProcessor)
            {
                _postProcessors.Add(postProcessor);
                return this;
            }

            /// <summary>Adds multiple asynchronous post-processors to the flow.</summary>
            /// <remarks>
            /// Note that <b>all</b> async post-processors run <i>after</i> <b>all</b> sync
            /// post-processors, regardless of order of addition in the builder.
            /// </remarks>
            public Builder ProcessWithAsync(params IAsyncAvPostProcessor[] asyncPostProcessors)
            {
                _asyncPostProcessors.AddRange(asyncPostProcessors);
                return this;
            }

            /// <summary>Adds an asynchronous post-processor to the flow.</summary>
            /// <remarks>
            /// Note that <b>all</b> async post-processors run <i>after</i> <b>all</b> sync
            /// post-processors, regardless of order of addition in the builder.
            /// </remarks>
            public Builder ProcessWithAsync(IAsyncAvPostProcessor asyncPostProcessor)
            {
                _asyncPostProcessors.Add(asyncPostProcessor);
                return this;
            }

            /// <summary>Builds the <see cref="AvFlow"/>.</summary>
            /// <remarks>
            /// The returned object is independent of the builder and will not
            /// reflect subsequent changes made to this builder instance.
            /// </remarks>
            public AvFlow Build()
            {
                return new AvFlow(_loaders.ToArray(), _importers.ToArray())
                {
                    PostProcessors = _postProcessors.ToArray(),
                    AsyncPostProcessors = _asyncPostProcessors.ToArray(),
                };
            }

            /// <summary>Creates a copy of the builder.</summary>
            /// <remarks>
            /// The returned object is independent of the builder and will not
            /// reflect subsequent changes made to this builder instance.
            /// </remarks>
            public Builder Copy()
            {
                Builder copy = new();
                copy._loaders.AddRange(_loaders);
                copy._importers.AddRange(_importers);
                copy._postProcessors.AddRange(_postProcessors);
                copy._asyncPostProcessors.AddRange(_asyncPostProcessors);
                return copy;
            }
        }

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
        public IEnumerable<IAvPostProcessor> PostProcessors = Array.Empty<IAvPostProcessor>();
        
        /// <summary>
        /// Async post-processors to run after loading the avatar.
        /// </summary>
        public IEnumerable<IAsyncAvPostProcessor> AsyncPostProcessors = Array.Empty<IAsyncAvPostProcessor>();

        public AvFlow(IReadOnlyList<IAvDataLoader> dataLoaders, IReadOnlyList<IAvImporter> importers)
        {
            if (dataLoaders.Count == 0) throw new ArgumentException("No data loaders defined for flow.", nameof(dataLoaders));
            if (importers.Count == 0) throw new ArgumentException("No importers defined for flow.", nameof(importers));

            DataLoaders = dataLoaders;
            Importers = importers;
        }

        /// <summary>
        /// Runs the loader flow completely.
        /// </summary>
        /// <returns>The loaded avatar.</returns>
        public async Awaitable<LoadedAv> RunAsync(CancellationToken token = default)
        {
            AvSourceData rawData = (await LoadAvatarDataAsync(true, token))!;
            LoadedAv avatar = (await ImportAvatarAsync(rawData, true, token))!;
            await RunPostProcessorsAndDisposeAvOnFailAsync(avatar, rawData, true, token);

            avatar.GameObject.SetActive(true);
            return avatar;
        }

        /// <summary>
        /// Tries to run the loader flow completely.
        /// </summary>
        /// <returns>The loaded avatar if the entire flow completed successfully; <see langword="null"/> otherwise.</returns>
        public async Awaitable<LoadedAv?> TryRunAsync(CancellationToken token = default)
        {
            AvSourceData? rawData = await LoadAvatarDataAsync(false, token);
            if (rawData is null) return null;

            LoadedAv? avatar = await ImportAvatarAsync(rawData, false, token);
            if (avatar is null) return null;

            if (!await RunPostProcessorsAndDisposeAvOnFailAsync(avatar, rawData, false, token))
                return null;

            avatar.GameObject.SetActive(true);
            return avatar;
        }

        private async Awaitable<AvSourceData?> LoadAvatarDataAsync(bool throwOnFinalFail, CancellationToken token)
        {
            int counter = 0;
            int failAtIdx = DataLoaders.Count - 1;
            foreach (IAvDataLoader loader in DataLoaders)
            {
                if (await loader.LoadAvatarAsync(throwOnFinalFail && counter == failAtIdx, token) is AvSourceData avData)
                    return avData;
                counter++;
            }

            return null;
        }

        private async Awaitable<LoadedAv?> ImportAvatarAsync(AvSourceData rawData, bool throwOnFinalFail, CancellationToken token)
        {
            int counter = 0;
            int failAtIdx = Importers.Count - 1;
            foreach (IAvImporter importer in Importers)
            {
                if (importer.SupportsFormat(rawData.ModelFormat)
                    && await importer.ImportAvatarAsync(rawData, throwOnFinalFail && counter == failAtIdx, token) is LoadedAv avatar)
                    return avatar;
                counter++;
            }

            if (throwOnFinalFail) throw new NotSupportedException($"No importer defined that supports format '{rawData.ModelFormat}'.");
            Debug.LogWarning($"{nameof(AvFlow)}: No importer defined that supports format '{rawData.ModelFormat}'.");
            return null;
        }


        private async Awaitable<bool> RunPostProcessorsAndDisposeAvOnFailAsync(LoadedAv avatar, AvSourceData rawData, bool throwOnFail, CancellationToken token)
        {
            try
            {
                foreach (IAvPostProcessor postProcessor in PostProcessors)
                    postProcessor.PostProcess(avatar, rawData);

                foreach (IAsyncAvPostProcessor asyncPostProcessor in AsyncPostProcessors)
                    await asyncPostProcessor.PostProcessAsync(avatar, rawData, token);
                
                return true;
            }
            catch (Exception ex)
            {
                avatar.Dispose();
                if (throwOnFail) throw;

                Debug.LogWarning($"{nameof(AvFlow)}: Could not load avatar due to post-processor exception:\n{ex}");
                return false;
            }
        }
    }
}