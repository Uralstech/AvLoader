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

#if GLTFAST_INSTALLED
using System;
using System.IO;
using System.Threading;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Imports glTF avatars.
    /// </summary>
    public class GLTFAvImporter : IAvImporter
    {
        /// <summary>
        /// Optional factory for custom glTFast GameObjectInstantiators.
        /// </summary>
        public Func<GltfImport, GameObject, Awaitable<GameObjectInstantiator>>? InstantiatorFactory;

        /// <summary>
        /// Optional custom glTFast InstantiationSettings.
        /// </summary>
        public InstantiationSettings? InstantiationSettings;

        /// <summary>
        /// Optional custom glTFast ImportSettings.
        /// </summary>
        public ImportSettings? ImportSettings;

        /// <summary>
        /// Optional custom glTFast defer agent.
        /// </summary>
        public IDeferAgent? DeferAgent;

        /// <summary>
        /// Optional custom glTFast material generator.
        /// </summary>
        public IMaterialGenerator? MaterialGenerator;

        /// <summary>
        /// Optional custom glTFast logger.
        /// </summary>
        public ICodeLogger? Logger;

        /// <inheritdoc/>
        public bool SupportsFormat(AvModelFileExtension format) =>
            format is AvModelFileExtension.GLTF or AvModelFileExtension.GLB or AvModelFileExtension.GLTFAny;

        /// <inheritdoc/>
        public async Awaitable<ILoadedAv?> ImportAvatarAsync(AvDataContainer rawData, bool throwOnFail, CancellationToken token = default)
        {
            GltfImport import = new(
                deferAgent: DeferAgent,
                materialGenerator: MaterialGenerator,
                logger: Logger
            );

            bool success = await import.Load(
                rawData.Model, !string.IsNullOrEmpty(rawData.LocalModelPath) ? new Uri(rawData.LocalModelPath) : null,
                ImportSettings, token
            );

            if (!success)
            {
                import.Dispose();
                if (throwOnFail) throw new InvalidDataException("Could not import glTF avatar.");
                
                Debug.LogWarning($"{nameof(GLTFAvImporter)}: Could not import glTF avatar.");
                return null;
            }

            GameObject gameObject = new($"Avatar ({nameof(GLTFAvImporter)})");
            gameObject.SetActive(false);

            GameObjectInstantiator instantiator;
            if (InstantiatorFactory is not null)
            {
                try
                {
                    instantiator = await InstantiatorFactory(import, gameObject);
                }
                catch (Exception ex)
                {
                    import.Dispose();
                    
                    if (throwOnFail) throw new AggregateException("Could not create instantiator due to exception from user code.", ex);
                    Debug.LogWarning($"{nameof(GLTFAvImporter)}: Could not create instantiator due to exception from user code:\n{ex}");
                    return null;
                }
            }
            else
            {
                instantiator = new GameObjectInstantiator(import, gameObject.transform, Logger, InstantiationSettings);
            }

            success = await import.InstantiateMainSceneAsync(instantiator, token);
            if (!success)
            {
                import.Dispose();
                if (throwOnFail) throw new InvalidDataException("Could not import glTF main scene.");
                
                Debug.LogWarning($"{nameof(GLTFAvImporter)}: Could not import glTF main scene.");
                return null;
            }

            return new LoadedGLTFAv(
                gameObject, import, rawData.Metadata,
                rawData, typeof(GLTFAvImporter),
                rawData.FullRender, rawData.BustRender
            );
        }
    }

    /// <summary>
    /// A loaded glTF avatar.
    /// </summary>
    public class LoadedGLTFAv : ILoadedAv
    {
        /// <inheritdoc/>
        public GameObject GameObject { get; }

        /// <inheritdoc/>
        public AvMetadata Metadata { get; }

        /// <inheritdoc/>
        public Texture2D? FullRender { get; }

        /// <inheritdoc/>
        public Texture2D? BustRender { get; }

        /// <inheritdoc/>
        public AvDataContainer RawData { get; }

        /// <inheritdoc/>
        public Type ImporterType { get; }

        /// <summary>The glTFast import associated with the avatar.</summary>
        public readonly GltfImport Import;

        private bool _disposed = false;

        public LoadedGLTFAv(GameObject gameObject, GltfImport import, AvMetadata metadata, AvDataContainer rawData, Type importerType, Texture2D? fullRender, Texture2D? bustRender)
        {
            GameObject = gameObject;
            Metadata = metadata;
            Import = import;
            RawData = rawData;
            ImporterType = importerType;

            FullRender = fullRender;
            BustRender = bustRender;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            UnityEngine.Object.Destroy(GameObject);
            Import.Dispose();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
#endif
