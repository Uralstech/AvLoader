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

#if UNIVRM10_INSTALLED
using System;
using System.Collections.Generic;
using System.Threading;
using UniGLTF;
using UnityEngine;
using UniVRM10;

#nullable enable
namespace Uralstech.AvLoader.Importers
{
    /// <summary>
    /// Imports VRM avatars using <a href="https://github.com/vrm-c/UniVRM">UniVRM10</a>.
    /// </summary>
    public class UniVRM10AvImporter : IAvImporter
    {
        /// <summary>
        /// If <see langword="true"/>, VRM-0.x models are converted to VRM-1.0 and loaded.
        /// The components attached to the VRM-0.x model are different. (default: <see langword="true"/>)
        /// </summary>
        public bool CanLoadVrm0X = true;

        /// <summary>ControlRig generation settings. (default: <see cref="ControlRigGenerationOption.Generate"/>)</summary>
        public ControlRigGenerationOption ControlRigGenerationOption = ControlRigGenerationOption.Generate;

        /// <summary>Should all mesh renderers be enabled on load? (default: <see langword="true"/>)</summary>
        public bool ShowMeshes = true;

        /// <summary>Optional texture deserialization.</summary>
        public ITextureDeserializer? TextureDeserializer;

        /// <summary>Optional material generator.</summary>
        public IMaterialDescriptorGenerator? MaterialGenerator;

        /// <summary>Optional callback called on VRM-0x to VRM-1.0 migration.</summary>
        public Vrm10.VrmMetaInformationCallback? VRMMetaInformationCallback;

        /// <summary>Optional importer settings.</summary>
        public ImporterContextSettings? ImporterContextSettings;

        /// <summary>Optional runtime selection for SpringBones.</summary>
        public IVrm10SpringBoneRuntime? SpringboneRuntime;

        /// <inheritdoc/>
        public bool SupportsFormat(AvModelFileExtension format) => format is AvModelFileExtension.VRM;

        /// <inheritdoc/>
        public async Awaitable<LoadedAv?> ImportAvatarAsync(AvSourceData rawData, bool throwOnFail, CancellationToken token = default)
        {
            try
            {
                Vrm10Instance instance = await Vrm10.LoadBytesAsync(
                    rawData.Model, CanLoadVrm0X,
                    ControlRigGenerationOption, ShowMeshes,
                    textureDeserializer: TextureDeserializer,
                    materialGenerator: MaterialGenerator,
                    vrmMetaInformationCallback: VRMMetaInformationCallback,
                    importerContextSettings: ImporterContextSettings,
                    springboneRuntime: SpringboneRuntime
                );

                instance.gameObject.SetActive(false);
                return new LoadedUniVRM10Av(instance.gameObject, instance, rawData.Metadata, rawData.FullRender, rawData.BustRender, typeof(UniVRM10AvImporter));
            }
            catch (Exception ex)
            {
                if (throwOnFail) throw;
                Debug.LogWarning($"{nameof(UniVRM10AvImporter)}: Could not import VRM avatar due to exception:\n{ex}");
                return null;
            }
        }
    }

    /// <summary>
    /// A loaded UniVRM VRM10 avatar.
    /// </summary>
    public class LoadedUniVRM10Av : LoadedAv
    {
        /// <summary>The VRM avatar.</summary>
        public readonly Vrm10Instance VRMInstance;
        
        /// <summary>The <see cref="RuntimeGltfInstance"/> component of the VRM avatar, if it exists.</summary>
        public readonly RuntimeGltfInstance? GLTFInstance;

        private bool _disposed = false;

        public LoadedUniVRM10Av(GameObject gameObject, Vrm10Instance vrmInstance, AvMetadata metadata, Texture2D? fullRender, Texture2D? bustRender, Type importerType)
            : base(gameObject, metadata, fullRender, bustRender, importerType)
        {
            VRMInstance = vrmInstance;
            vrmInstance.TryGetComponent(out GLTFInstance);
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Material>? TryGetAvatarMaterials() => GLTFInstance != null ? GLTFInstance.Materials : null;

        /// <inheritdoc/>
        public override IReadOnlyList<Mesh>? TryGetAvatarMeshes() => GLTFInstance != null ? GLTFInstance.Meshes : null;

        /// <inheritdoc/>
        public override IReadOnlyList<Renderer>? TryGetAvatarRenderers() => GLTFInstance != null ? GLTFInstance.Renderers : null;

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (_disposed)
                return;

            UnityEngine.Object.Destroy(GameObject);
            VRMInstance.DisposeRuntime();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
#endif
