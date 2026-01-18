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
using Uralstech.AvLoader.PostProcessors.Rendering;

#nullable enable
namespace Uralstech.AvLoader.PostProcessors
{
    /// <summary>
    /// Post-processing step that recreates avatar materials using alternative shaders
    /// and copies selected property and keyword values from the original materials.
    /// </summary>
    public class MaterialOverridePostProcessor : IAvPostProcessor
    {
        /// <summary>Configuration which defines the material override rules.</summary>
        public MaterialOverrideConfig Configuration;

        public MaterialOverridePostProcessor(MaterialOverrideConfig configuration)
        {
            Configuration = configuration;
        }

        /// <inheritdoc/>
        public void PostProcess(LoadedAv avatar, AvSourceData rawData)
        {
            IEnumerable<Renderer>? renderers = avatar.TryGetAvatarRenderers();
            renderers ??= avatar.GameObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                int count = materials.Length;
                int updated = 0;

                for (int i = 0; i < count; i++)
                {
                    Material source = materials[i];
                    if (source == null || !Configuration.MaterialOverrides.TryGetValue(source.shader, out RuntimeMaterialOverrideDefinition overrideDefinition))
                        continue;

                    Material target = new(overrideDefinition.Target);
                    avatar.DestroyOnDispose.Add(target);

                    ApplyProperties(overrideDefinition.PropertyMappings, source, target);
                    if (overrideDefinition.KeywordMappings is not null)
                        ApplyKeywords(overrideDefinition.KeywordMappings, source, target);

                    materials[i] = target;
                    updated++;
                }

                if (updated > 0) renderer.sharedMaterials = materials;
            }
        }

        private void ApplyProperties(IReadOnlyList<ShaderPropertyMapping> propertyMappings, Material source, Material target)
        {
            int count = propertyMappings.Count;
            for (int i = 0; i < count; i++)
            {
                ShaderPropertyMapping mapping = propertyMappings[i];
                if (!source.HasProperty(mapping.Source))
                {
                    Debug.LogWarning($"{nameof(MaterialOverridePostProcessor)}: Source material with shader {source.shader.name} does not have property '{mapping.Source}'.");
                    continue;
                }

                if (!target.HasProperty(mapping.Target))
                {
                    Debug.LogWarning($"{nameof(MaterialOverridePostProcessor)}: Target material with shader {target.shader.name} does not have property '{mapping.Target}'.");
                    continue;
                }

                switch (mapping.Type)
                {
                    case ShaderPropertyType.Float:
                        float floatVal = source.GetFloat(mapping.Source);
                        target.SetFloat(mapping.Target, floatVal); break;

                    case ShaderPropertyType.Inverse01RangeFloat:
                        float floatValInv = 1 - source.GetFloat(mapping.Source);
                        target.SetFloat(mapping.Target, floatValInv); break;

                    case ShaderPropertyType.Vector:
                        Vector4 vectorVal = source.GetVector(mapping.Source);
                        target.SetVector(mapping.Target, vectorVal); break;

                    case ShaderPropertyType.Color:
                        Color colorVal = source.GetColor(mapping.Source);
                        target.SetColor(mapping.Target, colorVal); break;

                    case ShaderPropertyType.Texture:
                        Texture textureVal = source.GetTexture(mapping.Source);
                        target.SetTexture(mapping.Target, textureVal); break;

                    case ShaderPropertyType.Matrix:
                        Matrix4x4 matrixVal = source.GetMatrix(mapping.Source);
                        target.SetMatrix(mapping.Target, matrixVal); break;

                    default:
                        throw new NotImplementedException($"Case not defined for type: {mapping.Type}");
                }
            }
        }

        private void ApplyKeywords(IReadOnlyList<ShaderKeywordMapping> keywordMappings, Material source, Material target)
        {
            int count = keywordMappings.Count;
            for (int i = 0; i < count; i++)
            {
                ShaderKeywordMapping mapping = keywordMappings[i];
                UnityEngine.Rendering.LocalKeyword sourceKw = source.shader.keywordSpace.FindKeyword(mapping.Source);
                if (!sourceKw.isValid)
                {
                    Debug.LogWarning($"{nameof(MaterialOverridePostProcessor)}: Source material with shader {source.shader.name} does not have valid keyword '{mapping.Source}'.");
                    continue;
                }

                UnityEngine.Rendering.LocalKeyword targetKw = target.shader.keywordSpace.FindKeyword(mapping.Target);
                if (!targetKw.isValid)
                {
                    Debug.LogWarning($"{nameof(MaterialOverridePostProcessor)}: Target material with shader {target.shader.name} does not have valid keyword '{mapping.Target}'.");
                    continue;
                }

                if (source.IsKeywordEnabled(in sourceKw) == mapping.SourceRequiredState)
                    target.SetKeyword(in targetKw, mapping.TargetResultState);
            }
        }
    }
}