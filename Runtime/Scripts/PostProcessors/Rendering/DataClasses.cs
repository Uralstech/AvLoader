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
namespace Uralstech.AvLoader.PostProcessors.Rendering
{
    /// <summary>
    /// Defines a mapping from a source shader to a target shader.
    /// </summary>
    [Serializable]
    public class ShaderMapping
    {
        /// <summary>The original shader to be replaced.</summary>
        public Shader? Source;
        
        /// <summary>The shader that replaces the source shader.</summary>
        public Shader? Target;

        /// <summary>Returns <see langword="true"/> if both source and target shaders are assigned.</summary>
        public bool IsValid() => Source != null && Target != null;
    }

    /// <summary>
    /// Runtime definition describing how a material using a given shader
    /// should be recreated using a different shader and mapped properties.
    /// </summary>
    public class RuntimeMaterialOverrideDefinition
    {
        /// <summary>The shader used by the newly created material.</summary>
        public Shader Target;

        /// <summary>
        /// Defines how shader property values are transferred from the source material
        /// to the newly created material.
        /// </summary>
        /// <remarks>
        /// Each mapping specifies a source property name, a target property name, and the shared property type.
        /// Source and target property names must both be non-empty and unique.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if any source or target property name is <see langword="null"/>/empty, or if duplicates are detected.</exception>
        public IReadOnlyList<ShaderPropertyMapping> PropertyMappings
        {
            get => _propertyMappings;
            set
            {
                ValidatePropertyMappings(value, nameof(value));
                _propertyMappings = value;
            }
        }

        /// <summary>
        /// Optional mappings that control how shader keywords are transferred
        /// or conditionally enabled on the new material.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if any keyword mapping contains an empty or <see langword="null"/> source or target.</exception>
        public IReadOnlyList<ShaderKeywordMapping>? KeywordMappings
        {
            get => _keywordMappings;
            set
            {
                if (value is not null && !ShaderKeywordMapping.IsValid(value))
                    throw new ArgumentException("Found invalid shader keyword mapping with empty/null source or target.", nameof(value));
                _keywordMappings = value;
            }
        }

        internal IReadOnlyList<ShaderKeywordMapping>? _keywordMappings;
        internal IReadOnlyList<ShaderPropertyMapping> _propertyMappings;

#pragma warning disable CS8618
        internal RuntimeMaterialOverrideDefinition() { }
#pragma warning restore CS8618

        public RuntimeMaterialOverrideDefinition(Shader target, IReadOnlyList<ShaderPropertyMapping> propertyMappings, IReadOnlyList<ShaderKeywordMapping>? keywordMappings = null)
        {
            Target = target;
            _keywordMappings = keywordMappings;
            if (_keywordMappings is not null && !ShaderKeywordMapping.IsValid(_keywordMappings))
                throw new ArgumentException("Found invalid shader keyword mapping with empty/null source or target.", nameof(keywordMappings));

            _propertyMappings = propertyMappings;
            ValidatePropertyMappings(_propertyMappings, nameof(propertyMappings));
        }

        private static void ValidatePropertyMappings(IReadOnlyList<ShaderPropertyMapping> mappings, string argName)
        {
            HashSet<string> sourcesSet = new(mappings.Count);
            HashSet<string> targetsSet = new(mappings.Count);
            foreach (ShaderPropertyMapping mapping in mappings)
            {
                if (!mapping.IsValid())
                    throw new ArgumentException("Found null/empty source or target property.", argName);

                if (!sourcesSet.Add(mapping.Source!))
                    throw new ArgumentException($"Expected all sources in property mappings to be unique, found at least one duplicate: '{mapping.Source}'", argName);

                if (!targetsSet.Add(mapping.Target!))
                    throw new ArgumentException($"Expected all targets in property mappings to be unique, found at least one duplicate: '{mapping.Target}'", argName);
            }
        }

        internal EditorMaterialOverrideDefinition Serialize(Shader key)
        {
            return new EditorMaterialOverrideDefinition()
            {
                ShaderMapping = new ShaderMapping() { Source = key, Target = Target },
                PropertyMappings = new List<ShaderPropertyMapping>(_propertyMappings),
                KeywordMappings = _keywordMappings is not null
                    ? new List<ShaderKeywordMapping>(_keywordMappings)
                    : new List<ShaderKeywordMapping>()
            };
        }
    }

    [Serializable]
    internal class EditorMaterialOverrideDefinition
    {
        public ShaderMapping? ShaderMapping;

        [Tooltip("Mappings that define how shader property values are copied from the source material to the new material. " +
                 "Only the properties listed here are transferred. All others use the target shader's default values.")]
        public List<ShaderPropertyMapping> PropertyMappings = new();

        [Tooltip("Conditional rules that control shader keyword states on the newly created material.")]
        public List<ShaderKeywordMapping> KeywordMappings = new();

        public RuntimeMaterialOverrideDefinition? Deserialize()
        {
            if (ShaderMapping == null || !ShaderMapping.IsValid())
                return null;

            HashSet<string> sourcesSet = new(PropertyMappings.Count);
            HashSet<string> targetsSet = new(PropertyMappings.Count);
            foreach (ShaderPropertyMapping mapping in PropertyMappings)
            {
                if (!mapping.IsValid()
                    || !sourcesSet.Add(mapping.Source!)
                    || !targetsSet.Add(mapping.Target!))
                    return null;
            }

            return new RuntimeMaterialOverrideDefinition()
            {
                Target = ShaderMapping!.Target!,
                _propertyMappings = new List<ShaderPropertyMapping>(PropertyMappings),
                _keywordMappings = KeywordMappings.Count != 0 ? new List<ShaderKeywordMapping>(KeywordMappings) : null,
            };
        }
    }

    /// <summary>
    /// Describes how a single shader property value is copied from a source material to a target material.
    /// </summary>
    [Serializable]
    public class ShaderPropertyMapping
    {
        /// <summary>The name of the property on the source shader.</summary>
        public string? Source;

        /// <summary>The name of the corresponding property on the target shader.</summary>
        public string? Target;

        /// <summary>The expected data type of the shader property.</summary>
        public ShaderPropertyType Type;

        /// <summary>Returns <see langword="true"/> if both source and target property names are non-empty.</summary>
        public bool IsValid() => !string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(Target);
    }

    /// <summary>
    /// Defines a conditional rule for transferring or overriding a shader keyword.
    /// </summary>
    [Serializable]
    public class ShaderKeywordMapping
    {
        /// <summary>The source shader keyword to test.</summary>
        public string? Source;

        /// <summary>The required enabled state of the source keyword for this rule to apply.</summary>
        public bool SourceRequiredState;

        /// <summary>The target shader keyword to modify.</summary>
        public string? Target;

        /// <summary>The keyword state applied to the target material when the rule matches.</summary>
        public bool TargetResultState;

        /// <summary>Returns <see langword="true"/> if both source and target keyword names are non-empty.</summary>
        public bool IsValid() => !string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(Target);

        /// <summary>Returns <see langword="true"/> if all keyword mappings in the list are valid.</summary>
        public static bool IsValid(IReadOnlyList<ShaderKeywordMapping> mappings)
        {
            int count = mappings.Count;
            for (int i = 0; i < count; i++)
            {
                if (!mappings[i].IsValid())
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Represents the supported shader property data types that can be transferred
    /// between materials, based on the <see cref="Material"/> <c>.GetX</c> and
    /// <c>.SetX</c> methods.
    /// </summary>
    public enum ShaderPropertyType
    {
        Float,
        Vector,
        Color,
        Texture,
        Matrix,

        /// <summary>Inverts the float value in 0-1 range before assignment.</summary>
        /// <remarks>E.g. assigning a "smoothness" factor to a shader that expects a "roughness" factor.</remarks>
        Inverse01RangeFloat
    }
}