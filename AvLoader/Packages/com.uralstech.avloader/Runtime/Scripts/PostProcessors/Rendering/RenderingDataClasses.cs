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
    /// Defines a mapping from one shader to another.
    /// </summary>
    /// <remarks>
    /// This type is shared by two related but distinct operations:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <b>Shader swap operations</b>, where an existing material's shader
    /// is directly replaced with <see cref="Target"/> while keeping the
    /// same material instance.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <b>Material overrides</b>, where a new material is created using
    /// <see cref="Target"/>, and properties and keywords are selectively
    /// transferred from the source material.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [Serializable]
    public class ShaderMapping
    {
        /// <summary>
        /// The shader to match against an existing material.
        /// </summary>
        public Shader? Source;

        /// <summary>
        /// The shader to apply, either by directly swapping the shader
        /// or by creating a new material.
        /// </summary>
        public Shader? Target;
    }

    /// <summary>
    /// Describes a material override operation.
    /// </summary>
    /// <remarks>
    /// A material override does <b>not</b> modify the original material.
    /// Instead, it:
    /// <list type="number">
    /// <item>
    /// <description>
    /// Identifies materials using <see cref="ShaderMapping.Source"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Creates a new material using <see cref="ShaderMapping.Target"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Transfers selected shader properties and configures shader keywords
    /// according to the override configuration.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [Serializable]
    public class MaterialOverrideDefinition
    {
        /// <summary>
        /// Shader mapping that determines which materials are overridden
        /// and which shader is used for the newly created material.
        /// </summary>
        public ShaderMapping? ShaderMapping;

        /// <summary>
        /// Mappings that define how shader property values are copied from
        /// the source material to the new material.
        /// </summary>
        /// <remarks>
        /// Only the properties listed here are transferred. All others
        /// use the target shader's default values.
        /// </remarks>
        [Tooltip("Mappings that define how shader property values are copied from the source material to the new material. " +
                 "Only the properties listed here are transferred. All others use the target shader's default values.")]
        public List<ShaderPropertyMapping>? PropertyMappings;

        /// <summary>
        /// Conditional rules that control shader keyword states on the
        /// newly created material.
        /// </summary>
        [Tooltip("Conditional rules that control shader keyword states on the newly created material.")]
        public List<ShaderKeywordMapping>? KeywordMappings;
    }

    /// <summary>
    /// Declares a mapping between a property on the source shader and
    /// a property on the target shader.
    /// </summary>
    /// <remarks>
    /// Used by material overrides to transfer values from
    /// the original material to the newly created one.
    /// </remarks>
    [Serializable]
    public class ShaderPropertyMapping
    {
        /// <summary>
        /// The name of the property on the source shader.
        /// </summary>
        public string? Source;

        /// <summary>
        /// The name of the corresponding property on the target shader.
        /// </summary>
        public string? Target;

        /// <summary>
        /// The property's data type, which determines how the value
        /// is read from and written to the material.
        /// </summary>
        public ShaderPropertyType Type;
    }

    /// <summary>
    /// Defines a conditional rule for transferring shader keyword states.
    /// </summary>
    /// <remarks>
    /// Keyword overrides are evaluated during material override creation
    /// and allow the target material's keyword configuration to be derived
    /// from the source material.
    /// </remarks>
    [Serializable]
    public class ShaderKeywordMapping
    {
        /// <summary>
        /// The source shader keyword to evaluate.
        /// </summary>
        public string? Source;

        /// <summary>
        /// The required enabled state of <see cref="Source"/> for this rule
        /// to apply.
        /// </summary>
        public bool SourceRequiredState;

        /// <summary>
        /// The target shader keyword to modify.
        /// </summary>
        public string? Target;

        /// <summary>
        /// The enabled state to assign to <see cref="Target"/> when the
        /// condition is met.
        /// </summary>
        public bool TargetResultState;
    }

    /// <summary>
    /// Specifies the data type of a shader property.
    /// </summary>
    /// <remarks>
    /// The values correspond to the <c>Material.GetX</c> and
    /// <c>Material.SetX</c> APIs used during property transfer.
    /// </remarks>
    public enum ShaderPropertyType
    {
        Float,
        Vector,
        Color,
        Texture,
        Matrix
    }
}