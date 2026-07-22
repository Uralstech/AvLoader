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

#if ANIMATION_INSTALLED
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader.Capabilities
{
    /// <summary>
    /// Exposes an <see cref="Animator"/> that is created by the avatar importer.
    /// </summary>
    /// <remarks>
    /// This capability represents importer- or runtime-library-defined animation support.
    /// For example, UniVRM generates an <see cref="UnityEngine.Animator"/> as part of the import process.
    ///
    /// Animators added later by post-processors or user code are not considered part of this
    /// capability and should be accessed directly from <see cref="LoadedAv.GameObject"/>.
    /// </remarks>
    public interface IImportedAnimator : ICapability
    {
        /// <summary>The importer-generated animator.</summary>
        public Animator Animator { get; }
    }
}
#endif