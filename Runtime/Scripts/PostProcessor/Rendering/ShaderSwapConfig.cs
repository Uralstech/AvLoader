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

#nullable enable
namespace Uralstech.AvLoader.PostProcessors
{        
    /// <summary>
    /// Configuration data for <see cref="ShaderSwapPostProcessor"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "AvLoader/Shader Swap Configuration")]
    public class ShaderSwapConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>Optional fallback shader used when a shader is not defined in the shader map sources.</summary>
        [Tooltip("Optional fallback shader used when a shader is not defined in the shader map sources.")]
        public Shader? FallbackShader;

        /// <summary>Map of shaders to replace, where the key is the original shader and the value is the replacement shader.</summary>
        public Dictionary<Shader, Shader> ShaderMap = new();

        [Tooltip("Map of shaders to replace, where the source is the original shader and the target is the replacement shader.")]
        [SerializeField] internal List<ShaderKVPair>? _serializedShaderMap;

        internal bool _isValid;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!_isValid || _serializedShaderMap == null) return;

            _serializedShaderMap.Clear();
            foreach ((Shader key, Shader value) in ShaderMap)
                _serializedShaderMap.Add(new ShaderKVPair(key, value));
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() => IsValid();

        internal bool IsValid()
        {
            _isValid = true;
            ShaderMap.Clear();
            if (_serializedShaderMap == null) return true;

            int count = _serializedShaderMap.Count;
            for (int i = 0; i < count; i++)
            {
                ShaderKVPair pair = _serializedShaderMap[i];
                if (pair._source == null || pair._target == null || !ShaderMap.TryAdd(pair._source, pair._target))
                    _isValid = false;
            }

            return _isValid;
        }
    }
}