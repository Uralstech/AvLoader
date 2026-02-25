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

        /// <summary>Does this avatar have any blendshape weights?</summary>
        public bool HasWeights { get; }

        /// <summary>Gets the weight for the given name.</summary>
        /// <remarks>
        /// If multiple blend shape sources in the avatar have the same
        /// channel name, this gets the maximum weight.
        /// </remarks>
        public float GetWeight(string name);

        /// <summary>Sets the weight for the given name.</summary>
        /// <remarks>
        /// If multiple blend shape sources in the avatar have the same
        /// channel name, this sets the weight for all of them.
        /// </remarks>
        public void SetWeight(string name, float weight);

        /// <summary>Checks if a weight channel with the given name exists.</summary>
        public bool HasWeight(string name);
    }

    /// <summary>Provides allocation-free bulk access to blendshape channels.</summary>
    /// <remarks>
    /// The semantic meaning of channels (e.g. expressions vs mesh blendshapes)
    /// is implementation-defined and should be determined via additional
    /// capability interfaces such as <see cref="IAvatarExpressionProvider"/>
    /// or <see cref="IMeshBlendShapeProvider"/>.
    /// </remarks>
    public interface IBlendShapeProviderBulk : IBlendShapeProvider
    {
        /// <summary>Retrieves the weights of the specified blendshape channels into the provided buffer.</summary>
        /// <remarks>For channel names that do not exist, the corresponding output value is set to zero.</remarks>
        public void GetWeights(ReadOnlySpan<string> names, Span<float> weights);

        /// <summary>Sets the weights of the specified blendshape channels.</summary>
        /// <remarks>Channel names that do not exist are ignored; the return value indicates how many weights were applied.</remarks>
        /// <returns>The number of weights successfully applied.</returns>
        public int SetWeights(ReadOnlySpan<string> names, ReadOnlySpan<float> weights);

        /// <summary>Sets multiple blendshape weights using name–value pairs.</summary>
        /// <remarks>Channel names that do not exist are ignored; the return value indicates how many weights were applied.</remarks>
        /// <returns>The number of weights successfully applied.</returns>
        public int SetWeights(ReadOnlySpan<(string name, float weight)> values);
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

        /// <summary>Retrieves multiple blendshape weights into the provided buffer.</summary>
        /// <remarks>
        /// Uses bulk access when available; otherwise falls back to per-channel access.
        /// Missing channel names result in zero values.
        /// </remarks>
        public static void GetWeights(this IBlendShapeProvider provider, ReadOnlySpan<string> names, Span<float> weights)
        {
            if (provider is IBlendShapeProviderBulk bulk)
            {
                bulk.GetWeights(names, weights);
                return;
            }

            if (weights.Length < names.Length)
                throw new ArgumentException($"{nameof(weights)} must be at least the same size as {nameof(names)}.", nameof(weights));

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                weights[i] = provider.HasWeight(name) ? provider.GetWeight(name) : 0f;
            }
        }

        /// <summary>Sets multiple blendshape weights.</summary>
        /// <remarks>
        /// Uses bulk access when available; otherwise falls back to per-channel access.
        /// Missing channel names are ignored.
        /// </remarks>
        /// <returns>The number of weights successfully applied.</returns>
        public static int SetWeights(this IBlendShapeProvider provider, ReadOnlySpan<string> names, ReadOnlySpan<float> weights)
        {
            if (provider is IBlendShapeProviderBulk bulk)
                return bulk.SetWeights(names, weights);

            if (weights.Length < names.Length)
                throw new ArgumentException($"{nameof(weights)} must be at least the same size as {nameof(names)}.", nameof(weights));

            int set = 0;
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                if (!provider.HasWeight(name))
                    continue;

                provider.SetWeight(name, weights[i]);
                set++;
            }

            return set;
        }

        /// <summary>Sets multiple blendshape weights using name–value pairs.</summary>
        /// <remarks>
        /// Uses bulk access when available; otherwise falls back to per-channel access.
        /// Missing channel names are ignored.
        /// </remarks>
        /// <returns>The number of weights successfully applied.</returns>
        public static int SetWeights(this IBlendShapeProvider provider, ReadOnlySpan<(string name, float weight)> values)
        {
            if (provider is IBlendShapeProviderBulk bulk)
                return bulk.SetWeights(values);

            int set = 0;
            for (int i = 0; i < values.Length; i++)
            {
                (string name, float weight) = values[i];
                if (!provider.HasWeight(name))
                    continue;

                provider.SetWeight(name, weight);
                set++;
            }

            return set;
        }

        /// <summary>Sets multiple blendshape weights from a dictionary.</summary>
        /// <remarks>This is a convenience overload; missing channel names are ignored.</remarks>
        public static int SetWeights(this IBlendShapeProvider provider, IReadOnlyDictionary<string, float> values)
        {
            int set = 0;
            foreach (KeyValuePair<string, float> pair in values)
            {
                if (!provider.HasWeight(pair.Key))
                    continue;

                provider.SetWeight(pair.Key, pair.Value);
                set++;
            }

            return set;
        }
    }
}
