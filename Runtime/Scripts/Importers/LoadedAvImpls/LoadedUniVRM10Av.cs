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
using UniGLTF;
using UnityEngine;
using UniVRM10;
using Uralstech.AvLoader.Capabilities;

#nullable enable
namespace Uralstech.AvLoader.Importers
{
    /// <summary>
    /// A loaded UniVRM VRM10 avatar.
    /// </summary>
    public class LoadedUniVRM10Av : LoadedAv, IImportedAnimator, IAvatarExpressionProvider, IBlendShapeProviderBulk
    {
        /// <summary>The VRM avatar.</summary>
        public readonly Vrm10Instance VRMInstance;
        
        /// <summary>The <see cref="RuntimeGltfInstance"/> component of the VRM avatar, if it exists.</summary>
        public readonly RuntimeGltfInstance? GLTFInstance;

        /// <inheritdoc/>
        public Animator Animator { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<string> ChannelNames => _expressionKeys.Keys;

        /// <inheritdoc/>
        public bool HasWeights => _expressionKeys.Count > 0;

        private readonly Dictionary<string, ExpressionKey> _expressionKeys;

        public LoadedUniVRM10Av(GameObject gameObject, Vrm10Instance vrmInstance, AvMetadata metadata, Texture2D? fullRender, Texture2D? bustRender, Type importerType)
            : base(gameObject, metadata, fullRender, bustRender, importerType)
        {
            VRMInstance = vrmInstance;
            vrmInstance.TryGetComponent(out GLTFInstance);

            Animator? controlRigAnim = vrmInstance.Runtime.ControlRig?.ControlRigAnimator;
            Animator = controlRigAnim != null ? controlRigAnim : VRMInstance.GetComponent<Animator>();

            IReadOnlyList<ExpressionKey> expressionKeys = VRMInstance.Runtime.Expression.ExpressionKeys;
            _expressionKeys = new Dictionary<string, ExpressionKey>(expressionKeys.Count);

            foreach (ExpressionKey key in expressionKeys)
                _expressionKeys.Add(key.Name, key);
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Material>? TryGetAvatarMaterials()
        {
            ThrowIfDisposed();
            return GLTFInstance != null ? GLTFInstance.Materials : null;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Mesh>? TryGetAvatarMeshes()
        {
            ThrowIfDisposed();
            return GLTFInstance != null ? GLTFInstance.Meshes : null;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Renderer>? TryGetAvatarRenderers()
        {
            ThrowIfDisposed();
            return GLTFInstance != null ? GLTFInstance.Renderers : null;
        }

        /// <inheritdoc/>
        public float GetWeight(string name)
        {
            ThrowIfDisposed();
            return _expressionKeys.TryGetValue(name, out ExpressionKey key)
                ? VRMInstance.Runtime.Expression.GetWeight(key)
                : throw new ArgumentException($"Expression '{name}' not defined in avatar!", nameof(name));
        }

        /// <inheritdoc/>
        public void SetWeight(string name, float weight)
        {
            ThrowIfDisposed();
            if (!_expressionKeys.TryGetValue(name, out ExpressionKey key))
                throw new ArgumentException($"Expression '{name}' not defined in avatar!", nameof(name));
    
            VRMInstance.Runtime.Expression.SetWeight(key, weight);
        }
        
        /// <inheritdoc/>
        public bool HasWeight(string name)
        {
            ThrowIfDisposed();
            return _expressionKeys.ContainsKey(name);
        }

        /// <inheritdoc/>
        public void GetWeights(ReadOnlySpan<string> names, Span<float> weights)
        {
            ThrowIfDisposed();
            if (weights.Length < names.Length)
                throw new ArgumentException($"{nameof(weights)} must be at least the same size as {nameof(names)}.", nameof(weights));

            int count = names.Length;
            for (int i = 0; i < count; i++)
            {
                weights[i] = _expressionKeys.TryGetValue(names[i], out ExpressionKey key)
                    ? VRMInstance.Runtime.Expression.GetWeight(key) : 0f;
            }
        }

        /// <inheritdoc/>
        public int SetWeights(ReadOnlySpan<string> names, ReadOnlySpan<float> weights)
        {
            ThrowIfDisposed();
            if (weights.Length < names.Length)
                throw new ArgumentException($"{nameof(weights)} must be at least the same size as {nameof(names)}.", nameof(weights));

            int set = 0;
            int count = names.Length;
            for (int i = 0; i < count; i++)
            {
                if (!_expressionKeys.TryGetValue(names[i], out ExpressionKey key))
                    continue;

                VRMInstance.Runtime.Expression.SetWeight(key, weights[i]);
                set++;
            }

            return set;
        }

        /// <inheritdoc/>
        public int SetWeights(ReadOnlySpan<(string name, float weight)> values)
        {
            ThrowIfDisposed();

            int set = 0;
            int count = values.Length;
            for (int i = 0; i < count; i++)
            {
                (string name, float weight) = values[i];
                if (!_expressionKeys.TryGetValue(name, out ExpressionKey key))
                    continue;
                
                VRMInstance.Runtime.Expression.SetWeight(key, weight);
                set++;
            }

            return set;
        }

        protected override void ImporterSpecificDispose()
        {
            UnityEngine.Object.Destroy(GameObject);
            VRMInstance.DisposeRuntime();
        }
    }
}
#endif
