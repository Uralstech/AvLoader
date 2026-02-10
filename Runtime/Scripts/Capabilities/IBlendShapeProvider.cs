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
using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace Uralstech.AvLoader.Capabilities
{
    /// <summary>Provides access to weighted blend channels.</summary>
    /// <remarks>
    /// This interface does not imply any semantic meaning of the channels.
    /// Channel names and behavior are implementation-defined.
    /// </remarks>
    public interface IBlendShapeProvider : ICapability
    {
        /// <summary>Case-sensitive keys for all available blend weights.</summary>
        public IReadOnlyCollection<string> ChannelNames { get; }

        /// <summary>Gets the weight for the given name.</summary>
        public float GetWeight(string name);

        /// <summary>Sets the weight for the given name.</summary>
        public void SetWeight(string name, float weight);

        /// <summary>Checks if a weight channel with the given name exists.</summary>
        public bool HasWeight(string name);
    }

    /// <summary>
    /// Provides semantically meaningful avatar expressions
    /// (e.g. Smile, Blink, Angry).
    /// </summary>
    /// <remarks>
    /// Expression names are avatar-level concepts and may drive
    /// multiple blendshapes, bones, or other mechanisms.
    /// </remarks>
    public interface IAvatarExpressionProvider : IBlendShapeProvider { }

    /// <summary>Provides access to mesh-level blendshapes.</summary>
    /// <remarks>
    /// Names correspond to mesh blendshape channels and are not
    /// guaranteed to represent avatar expressions.
    /// </remarks>
    public interface IMeshBlendShapeProvider : IBlendShapeProvider { }

    /// <summary>Extensions for <see cref="IBlendShapeProvider"/>s.</summary>
    public static class BlendShapeProviderExtensions
    {
        /// <summary>
        /// Checks if a channel with any one of the given <paramref name="names"/> exists in the current provider.
        /// </summary>
        /// <param name="foundName">The name of the found channel or <see langword="null"/> if not found.</param>
        /// <returns><see langword="true"/> if a channel was found; <see langword="false"/> otherwise.</returns>
        public static bool HasAnyWeight(this IBlendShapeProvider current, string[] names, [NotNullWhen(true)] out string? foundName)
        {
            int count = names.Length;
            for (int i = 0; i < count; i++)
            {
                string name = names[i];
                if (current.HasWeight(name))
                {
                    foundName = name;
                    return true;
                }
            }

            foundName = null;
            return false;
        }
    }
}
