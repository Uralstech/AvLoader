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
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using Uralstech.AvLoader.Capabilities;

#nullable enable
namespace Uralstech.AvLoader.Importers
{
    /// <summary>
    /// A loaded UniGLTF glTF avatar.
    /// </summary>
    public class LoadedUniGLTFAv : LoadedAv, IMeshBlendShapeProvider
    {
        /// <summary>The glTF avatar.</summary>
        public readonly RuntimeGltfInstance GLTFInstance;

        /// <inheritdoc/>
        public IReadOnlyCollection<string> ChannelNames => _blendShapeGroups.Keys;

        /// <inheritdoc/>
        public bool HasWeights => _blendShapeGroups.Count > 0;

        private readonly Dictionary<string, List<(SkinnedMeshRenderer renderer, int shapeIndex)>> _blendShapeGroups;

        public LoadedUniGLTFAv(GameObject gameObject, RuntimeGltfInstance gltfInstance, AvMetadata metadata, Texture2D? fullRender, Texture2D? bustRender, Type importerType)
            : base(gameObject, metadata, fullRender, bustRender, importerType)
        {
            GLTFInstance = gltfInstance;

            _blendShapeGroups = new Dictionary<string, List<(SkinnedMeshRenderer, int)>>();
            foreach (SkinnedMeshRenderer renderer in GLTFInstance.SkinnedMeshRenderers)
            {
                if (renderer.sharedMesh == null)
                    continue;

                int shapes = renderer.sharedMesh.blendShapeCount;
                for (int i = 0; i < shapes; i++)
                {
                    string name = renderer.sharedMesh.GetBlendShapeName(i);
                    if (_blendShapeGroups.TryGetValue(name, out List<(SkinnedMeshRenderer, int)> group))
                        group.Add((renderer, i));
                    else
                        _blendShapeGroups[name] = new List<(SkinnedMeshRenderer, int)>() { (renderer, i) };
                }
            }
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Material>? TryGetAvatarMaterials()
        {
            ThrowIfDisposed();
            return GLTFInstance.Materials;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Mesh>? TryGetAvatarMeshes()
        {
            ThrowIfDisposed();
            return GLTFInstance.Meshes;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Renderer>? TryGetAvatarRenderers()
        {
            ThrowIfDisposed();
            return GLTFInstance.Renderers;
        }

        /// <inheritdoc/>
        public float GetWeight(string name)
        {
            ThrowIfDisposed();
            if (!_blendShapeGroups.TryGetValue(name, out List<(SkinnedMeshRenderer, int)>? group))
                throw new ArgumentException($"Blend shape channel '{name}' not defined in avatar!", nameof(name));

            float max = float.MinValue;
            foreach ((SkinnedMeshRenderer renderer, int shapeIndex) in group)
            {
                float weight = renderer.GetBlendShapeWeight(shapeIndex);
                if (weight > max) max = weight;
            }

            return max;
        }

        /// <inheritdoc/>
        public void SetWeight(string name, float weight)
        {
            ThrowIfDisposed();
            if (!_blendShapeGroups.TryGetValue(name, out List<(SkinnedMeshRenderer, int)>? group))
                throw new ArgumentException($"Blend shape channel '{name}' not defined in avatar!", nameof(name));

            foreach ((SkinnedMeshRenderer renderer, int shapeIndex) in group)
                renderer.SetBlendShapeWeight(shapeIndex, weight);
        }

        /// <inheritdoc/>
        public bool HasWeight(string name)
        {
            ThrowIfDisposed();
            return _blendShapeGroups.ContainsKey(name);
        }

        protected override void ImporterSpecificDispose() => GLTFInstance.Dispose();
    }
}
#endif
