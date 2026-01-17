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
        [Serializable]
        internal class ShaderKVPair
        {
            [SerializeField, Tooltip("The shader to be replaced by \"Target\".")] internal Shader? _source;
            [SerializeField, Tooltip("The shader that will replace \"Source\".")] internal Shader? _target;

            public ShaderKVPair() { }
            public ShaderKVPair(Shader source, Shader target)
            {
                _source = source;
                _target = target;
            }
        }

        /// <summary>Map of shaders with Key: Shader to replace, Value: New shader.</summary>
        public Dictionary<Shader, Shader> ShaderMap = new();
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