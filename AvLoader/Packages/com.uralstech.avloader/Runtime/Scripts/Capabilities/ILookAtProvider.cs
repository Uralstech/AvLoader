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

using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader.Capabilities
{
    /// <summary>Provides control over avatar look-at behavior.</summary>
    /// <remarks>
    /// The implementation of look-at (e.g. head, eyes, constraints, or animation)
    /// is implementation-defined.
    /// </remarks>
    public interface ILookAtProvider : ICapability
    {
        /// <summary>Sets a transform for the avatar to look at.</summary>
        /// <remarks>Implementations should track the transform over time.</remarks>
        public void SetTarget(Transform transform);

        /// <summary>Sets a fixed world-space position for the avatar to look at.</summary>
        public void SetTarget(Vector3 worldPosition);

        /// <summary>Clears the look-at target.</summary>
        /// <remarks>Implementations should reset the avatar to its default gaze.</remarks>
        public void ClearTarget();
    }
}
