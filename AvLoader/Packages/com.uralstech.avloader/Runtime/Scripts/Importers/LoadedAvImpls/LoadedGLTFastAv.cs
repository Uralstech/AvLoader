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
using System.Collections.Generic;
using GLTFast;
using UnityEngine;
using Uralstech.AvLoader.Capabilities;

#nullable enable
namespace Uralstech.AvLoader.Importers
{
    /// <summary>
    /// A loaded glTFast glTF avatar.
    /// </summary>
    public class LoadedGLTFastAv : LoadedAv, IMeshBlendShapeProvider
    {
        /// <summary>The glTFast import associated with the avatar.</summary>
        public readonly GltfImport Import;

        /// <inheritdoc/>
        public IReadOnlyCollection<string> ChannelNames => _blendShapeGroups.Keys;

        /// <inheritdoc/>
        public bool HasWeights => _blendShapeGroups.Count > 0;

        private readonly Dictionary<string, List<(SkinnedMeshRenderer renderer, int shapeIndex)>> _blendShapeGroups;

        public LoadedGLTFastAv(GameObject gameObject, GltfImport import, AvMetadata metadata, Texture2D? fullRender, Texture2D? bustRender, Type importerType)
            : base(gameObject, metadata, fullRender, bustRender, importerType)
        {
            Import = import;
            
            _blendShapeGroups = new Dictionary<string, List<(SkinnedMeshRenderer, int)>>();
            foreach (Renderer renderer in GetAvatarRenderers())
            {
                if (renderer is not SkinnedMeshRenderer skinnedMeshRenderer || skinnedMeshRenderer.sharedMesh == null)
                    continue;

                int shapes = skinnedMeshRenderer.sharedMesh.blendShapeCount;
                for (int i = 0; i < shapes; i++)
                {
                    string name = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
                    if (_blendShapeGroups.TryGetValue(name, out List<(SkinnedMeshRenderer, int)> group))
                        group.Add((skinnedMeshRenderer, i));
                    else
                        _blendShapeGroups[name] = new List<(SkinnedMeshRenderer, int)>() { (skinnedMeshRenderer, i) };
                }
            }
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Material>? TryGetAvatarMaterials()
        {
            ThrowIfDisposed();
            int count = Import.MaterialCount;
            Material[] materials = new Material[count];
            
            for (int i = 0; i < count; i++)
                materials[i] = Import.GetMaterial(i);

            return materials;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Mesh>? TryGetAvatarMeshes()
        {
            ThrowIfDisposed();
            int count = Import.Meshes.Count;
            Mesh[] meshes = new Mesh[count];

            using IEnumerator<Mesh> meshEnumerator = Import.Meshes.GetEnumerator();
            for (int i = 0; i < count && meshEnumerator.MoveNext(); i++)
                meshes[i] = meshEnumerator.Current;
            
            return meshes;
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

        protected override void ImporterSpecificDispose()
        {
            UnityEngine.Object.Destroy(GameObject);
            Import.Dispose();
        }
    }
}
#endif
