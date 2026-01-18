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
    /// Configuration data for <see cref="MaterialOverridePostProcessor"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "AvLoader/Material Override Configuration")]
    public class MaterialOverrideConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>Runtime map of source shaders to their material override definitions.</summary>
        public Dictionary<Shader, RuntimeMaterialOverrideDefinition> MaterialOverrides = new();

        [Tooltip("The list of material override definitions.")]
        [SerializeField] internal List<EditorMaterialOverrideDefinition> _serializedMaterialOverrides = new();

        internal bool _isValid;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!_isValid) return;

            _serializedMaterialOverrides.Clear();
            foreach ((Shader key, RuntimeMaterialOverrideDefinition value) in MaterialOverrides)
                _serializedMaterialOverrides.Add(value.Serialize(key));
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() => IsValid();

        internal bool IsValid()
        {
            _isValid = true;
            MaterialOverrides.Clear();

            int count = _serializedMaterialOverrides.Count;
            for (int i = 0; i < count; i++)
            {
                EditorMaterialOverrideDefinition definition = _serializedMaterialOverrides[i];
                if (definition.Deserialize() is not RuntimeMaterialOverrideDefinition runtimeDefinition
                    || !MaterialOverrides.TryAdd(definition.ShaderMapping!.Source!, runtimeDefinition))
                    _isValid = false;
            }

            return _isValid;
        }
    }
}