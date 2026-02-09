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
using Uralstech.AvLoader.Utils;

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
    /// Defines how a material using a specific shader is recreated
    /// using another shader, with selected properties and keywords copied.
    /// </summary>
    public class RuntimeMaterialOverrideDefinition
    {
        /// <summary>The shader used by the newly created material.</summary>
        public Shader Target;

        /// <summary>
        /// Property mappings used to copy values from the source material
        /// to the newly created material.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if any mapping is invalid or if duplicate source or target properties are found.</exception>
        public IReadOnlyList<ShaderPropertyMapping> PropertyMappings
        {
            get => _propertyMappings;
            set
            {
                ValidatePropertyMappings(value, nameof(value));
                _propertyMappings = value.CreateCopy();
            }
        }

        /// <summary>
        /// Optional keyword mappings that directly copy enabled/disabled states
        /// from source keywords to target keywords.
        /// </summary>
        /// <remarks>For conditional or overridden behavior, use <see cref="KeywordRules"/> instead.</remarks>
        /// <exception cref="ArgumentException">Thrown if any mapping is invalid or if duplicate source or target keywords are found.</exception>
        public IReadOnlyList<ShaderKeywordMapping>? KeywordMappings
        {
            get => _keywordMappings;
            set
            {
                if (!ShaderKeywordMapping.IsValid(value))
                    throw new ArgumentException("Found invalid shader keyword mapping with empty/null source or target, or duplicate entries.", nameof(value));
                _keywordMappings = value?.CreateCopy();
            }
        }

        /// <summary>
        /// Optional rules that conditionally enable or disable shader keywords on the new material.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if any rule has an empty or null source or target keyword.</exception>
        public IReadOnlyList<ShaderKeywordRule>? KeywordRules
        {
            get => _keywordRules;
            set
            {
                if (!ShaderKeywordRule.IsValid(value))
                    throw new ArgumentException("Found invalid shader keyword rule with empty/null source or target.", nameof(value));
                _keywordRules = value?.CreateCopy();
            }
        }

        internal IReadOnlyList<ShaderPropertyMapping> _propertyMappings;
        internal IReadOnlyList<ShaderKeywordMapping>? _keywordMappings;
        internal IReadOnlyList<ShaderKeywordRule>? _keywordRules;

#pragma warning disable CS8618 // Validation-free constructor for internal deserialization methods.
        internal RuntimeMaterialOverrideDefinition() { }
#pragma warning restore CS8618

        public RuntimeMaterialOverrideDefinition(Shader target, IReadOnlyList<ShaderPropertyMapping> propertyMappings,
            IReadOnlyList<ShaderKeywordMapping>? keywordMappings = null, IReadOnlyList<ShaderKeywordRule>? keywordRules = null)
        {
            Target = target;

            ValidatePropertyMappings(propertyMappings, nameof(propertyMappings));
            _propertyMappings = propertyMappings.CreateCopy();

            if (!ShaderKeywordMapping.IsValid(keywordMappings))
                throw new ArgumentException("Found invalid shader keyword mapping with empty/null source or target, or duplicate entries.", nameof(keywordMappings));
            _keywordMappings = keywordMappings?.CreateCopy();

            if (keywordRules is not null && !ShaderKeywordRule.IsValid(keywordRules))
                throw new ArgumentException("Found invalid shader keyword rule with empty/null source or target.", nameof(keywordRules));
            _keywordRules = keywordRules?.CreateCopy();
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
                    : new List<ShaderKeywordMapping>(),
                KeywordRules = _keywordRules is not null
                    ? new List<ShaderKeywordRule>(_keywordRules)
                    : new List<ShaderKeywordRule>()
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

        [Tooltip("Mappings that directly copy the enabled/disabled state of shader keywords from the source material to the target material. " +
                 "Each source and each target keyword must be unique. For conditional behavior, use Keyword Rules instead.")]
        public List<ShaderKeywordMapping> KeywordMappings = new();

        [Tooltip("Conditional rules that control shader keyword states on the newly created material.")]
        public List<ShaderKeywordRule> KeywordRules = new();

        public RuntimeMaterialOverrideDefinition? Deserialize()
        {
            if (ShaderMapping == null
                || !ShaderMapping.IsValid()
                || !ShaderKeywordMapping.IsValid(KeywordMappings)
                || !ShaderKeywordRule.IsValid(KeywordRules))
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
                _propertyMappings = PropertyMappings.CreateCopy(),
                _keywordMappings = KeywordMappings.Count != 0 ? KeywordMappings.CreateCopy() : null,
                _keywordRules = KeywordRules.Count != 0 ? KeywordRules.CreateCopy() : null,
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
    /// Describes how a single shader keyword is copied from a source material to a target material.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ShaderKeywordRule"/> for conditional behavior.
    /// A mapping directly mirrors the enabled state of <see cref="Source"/> onto <see cref="Target"/>.
    /// </remarks>
    [Serializable]
    public class ShaderKeywordMapping
    {
        /// <summary>The name of the keyword on the source shader.</summary>
        public string? Source;

        /// <summary>The name of the corresponding keyword on the target shader.</summary>
        public string? Target;

        /// <summary>Returns <see langword="true"/> if both source and target keyword names are non-empty.</summary>
        public bool IsValid() => !string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(Target);

        /// <summary>Returns <see langword="true"/> if all keyword mappings in the list are valid and contain no duplicates.</summary>
        public static bool IsValid(IReadOnlyList<ShaderKeywordMapping>? mappings)
        {
            if (mappings is null || mappings.Count == 0)
                return true;

            HashSet<string> sourcesSet = new(mappings.Count);
            HashSet<string> targetsSet = new(mappings.Count);
            foreach (ShaderKeywordMapping mapping in mappings)
            {
                if (!mapping.IsValid() || !sourcesSet.Add(mapping.Source!) || !targetsSet.Add(mapping.Target!))
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Defines a conditional rule that enables or disables a target shader keyword based on the state of a source keyword.
    /// </summary>
    /// <remarks>
    /// If you want to copy the enabled/disabled state of a keyword to another one,
    /// use <see cref="ShaderKeywordMapping"/> instead.
    /// </remarks>
    [Serializable]
    public class ShaderKeywordRule
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

        /// <summary>Returns <see langword="true"/> if all keyword rules in the list are valid.</summary>
        public static bool IsValid(IReadOnlyList<ShaderKeywordRule>? rules)
        {
            if (rules is null || rules.Count == 0)
                return true;

            int count = rules.Count;
            for (int i = 0; i < count; i++)
            {
                if (!rules[i].IsValid())
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