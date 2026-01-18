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

using System.Collections.Generic;
using UnityEngine;
using Uralstech.AvLoader.PostProcessors.Rendering;

#nullable enable
namespace Uralstech.AvLoader.PostProcessors
{
    /// <summary>
    /// Configuration data for <see cref="ShaderSwapPostProcessor"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "AvLoader/Shader Swap Configuration")]
    public class ShaderSwapConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>Optional shader used when no entry exists in <see cref="ShaderMap"/>.</summary>
        [Tooltip("Optional shader used when no entry exists in the shader map")]
        public Shader? FallbackShader;

        /// <summary>Maps source shaders to their replacement shaders.</summary>
        public Dictionary<Shader, Shader> ShaderMap = new();

        [Tooltip("Map of shaders to replace, where the source is the original shader and the target is the replacement shader.")]
        [SerializeField] internal List<ShaderMapping> _serializedShaderMap = new();

        internal bool _isValid;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!_isValid) return;

            _serializedShaderMap.Clear();
            foreach ((Shader key, Shader value) in ShaderMap)
            {
                _serializedShaderMap.Add(new ShaderMapping()
                {
                    Source = key,
                    Target = value,
                });
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() => IsValid();

        internal bool IsValid()
        {
            _isValid = true;
            ShaderMap.Clear();

            int count = _serializedShaderMap.Count;
            for (int i = 0; i < count; i++)
            {
                ShaderMapping pair = _serializedShaderMap[i];
                if (!pair.IsValid() || !ShaderMap.TryAdd(pair.Source!, pair.Target!))
                    _isValid = false;
            }

            return _isValid;
        }
    }
}