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
using GLTFast.Loading;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader.Importers
{
    /// <summary>
    /// Imports glTF avatars using <a href="https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.15/manual/index.html">glTFast</a>.
    /// </summary>
    public class GLTFastAvImporter : IAvImporter
    {
        /// <summary>
        /// Optional factory for custom glTFast GameObjectInstantiators.
        /// </summary>
        public Func<GltfImport, GameObject, Awaitable<GameObjectInstantiator>>? InstantiatorFactory;

        /// <summary>Optional custom glTFast InstantiationSettings.</summary>
        public InstantiationSettings? InstantiationSettings;

        /// <summary>Optional custom glTFast ImportSettings.</summary>
        public ImportSettings? ImportSettings;

        /// <summary>Optional custom glTFast download provider.</summary>
        public IDownloadProvider? DownloadProvider;

        /// <summary>Optional custom glTFast defer agent.</summary>
        public IDeferAgent? DeferAgent;

        /// <summary>Optional custom glTFast material generator.</summary>
        public IMaterialGenerator? MaterialGenerator;

        /// <summary>Optional custom glTFast logger.</summary>
        public ICodeLogger? Logger;

        /// <inheritdoc/>
        public bool SupportsFormat(AvModelFileExtension format) =>
            format is AvModelFileExtension.GLTF or AvModelFileExtension.GLB or AvModelFileExtension.GLTFAny;

        /// <inheritdoc/>
        public async Awaitable<LoadedAv?> ImportAvatarAsync(AvSourceData rawData, bool throwOnFail, CancellationToken token = default)
        {
            GltfImport import = new(DownloadProvider, DeferAgent, MaterialGenerator, Logger);
            Uri? modelUri = !string.IsNullOrEmpty(rawData.ModelPath) ? new Uri(rawData.ModelPath) : null;

            bool success = await import.Load(rawData.Model, modelUri, ImportSettings, token);
            if (!success)
            {
                import.Dispose();
                if (throwOnFail) throw new InvalidDataException("Could not import glTF avatar.");
                
                Debug.LogWarning($"{nameof(GLTFastAvImporter)}: Could not import glTF avatar.");
                return null;
            }

            GameObject gameObject = new($"Avatar ({nameof(GLTFastAvImporter)})");
            gameObject.SetActive(false);

            if (await GetGameObjectInstantiatorAndDisposeOnFail(import, gameObject, throwOnFail) is not GameObjectInstantiator instantiator)
                return null;

            success = await import.InstantiateMainSceneAsync(instantiator, token);
            if (!success)
            {
                import.Dispose();
                UnityEngine.Object.Destroy(gameObject);

                if (throwOnFail) throw new InvalidDataException("Could not import glTF main scene.");
                Debug.LogWarning($"{nameof(GLTFastAvImporter)}: Could not import glTF main scene.");
                return null;
            }

            return new LoadedGLTFastAv(
                gameObject, import, rawData.Metadata,
                rawData.FullRender, rawData.BustRender,
                typeof(GLTFastAvImporter)
            );
        }

        private async Awaitable<GameObjectInstantiator?> GetGameObjectInstantiatorAndDisposeOnFail(GltfImport import, GameObject gameObject, bool throwOnFail)
        {
            if (InstantiatorFactory is null)
                return new GameObjectInstantiator(import, gameObject.transform, Logger, InstantiationSettings);
            
            try
            {
                return await InstantiatorFactory(import, gameObject);
            }
            catch (Exception ex)
            {
                import.Dispose();
                UnityEngine.Object.Destroy(gameObject);

                if (throwOnFail) throw new AggregateException("Could not create instantiator due to exception from user code.", ex);
                Debug.LogWarning($"{nameof(GLTFastAvImporter)}: Could not create instantiator due to exception from user code:\n{ex}");
                return null;
            }
        }
    }

    /// <summary>
    /// A loaded glTFast glTF avatar.
    /// </summary>
    public class LoadedGLTFastAv : LoadedAv
    {
        /// <summary>The glTFast import associated with the avatar.</summary>
        public readonly GltfImport Import;
        private bool _disposed = false;

        public LoadedGLTFastAv(GameObject gameObject, GltfImport import, AvMetadata metadata, Texture2D? fullRender, Texture2D? bustRender, Type importerType)
            : base(gameObject, metadata, fullRender, bustRender, importerType)
        {
            Import = import;
        }

        /// <inheritdoc/>
        public override void Dispose()
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
