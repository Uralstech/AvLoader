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

#if UNIGLTF_INSTALLED
using System;
using System.Threading;
using UniGLTF;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Imports glTF avatars using <a href="https://github.com/vrm-c/UniVRM">UniGLTF</a>.
    /// </summary>
    public class UniGLTFAvImporter : IAvImporter
    {
        /// <summary>Should all mesh renderers be enabled on load? (default: <see langword="true"/>)</summary>
        public bool ShowMeshes = true;

        /// <summary>Should the avatar's <see cref="SkinnedMeshRenderer"/>s be updated when off-screen?</summary>
        public bool EnableUpdateWhenOffscreen;

        /// <summary>Optional texture deserialization.</summary>
        public ITextureDeserializer? TextureDeserializer;

        /// <summary>Optional material generator.</summary>
        public IMaterialDescriptorGenerator? MaterialGenerator;

        /// <summary>Optional importer settings.</summary>
        public ImporterContextSettings? ImporterContextSettings;

        /// <inheritdoc/>
        public bool SupportsFormat(AvModelFileExtension format) =>
            format is AvModelFileExtension.GLTF or AvModelFileExtension.GLB or AvModelFileExtension.GLTFAny;

        /// <inheritdoc/>
        public async Awaitable<LoadedAv?> ImportAvatarAsync(AvDataContainer rawData, bool throwOnFail, CancellationToken token = default)
        {
            try
            {
                using GltfData data = new GlbBinaryParser(rawData.Model, rawData.ModelPath).Parse();
                using ImporterContext loader = new(data, null, TextureDeserializer, MaterialGenerator, ImporterContextSettings);
                RuntimeGltfInstance instance = await loader.LoadAsync(new RuntimeOnlyAwaitCaller());

                if (ShowMeshes) instance.ShowMeshes();
                if (EnableUpdateWhenOffscreen) instance.EnableUpdateWhenOffscreen();
                instance.gameObject.SetActive(false);

                return new LoadedUniGLTFAv(instance.gameObject, instance, rawData.Metadata, rawData.FullRender, rawData.BustRender, typeof(UniGLTFAvImporter));
            }
            catch (Exception ex)
            {
                if (throwOnFail) throw;
                Debug.LogWarning($"{nameof(UniGLTFAvImporter)}: Could not import glTF avatar due to exception:\n{ex}");
                return null;
            }
        }
    }

    /// <summary>
    /// A loaded UniGLTF glTF avatar.
    /// </summary>
    public class LoadedUniGLTFAv : LoadedAv
    {
        /// <summary>The glTF avatar.</summary>
        public readonly RuntimeGltfInstance GLTFInstance;
        private bool _disposed = false;

        public LoadedUniGLTFAv(GameObject gameObject, RuntimeGltfInstance gltfInstance, AvMetadata metadata, Texture2D? fullRender, Texture2D? bustRender, Type importerType)
            : base(gameObject, metadata, fullRender, bustRender, importerType)
        {
            GLTFInstance = gltfInstance;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (_disposed)
                return;

            GLTFInstance.Dispose();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
#endif
