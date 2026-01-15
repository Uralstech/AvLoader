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
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Post-processing step which adds an animation controller to the loaded avatar.
    /// Requries Unity's animation module to be enabled.
    /// </summary>
    public class AddAnimationController : IAvPostProcessor
    {
        /// <summary>Runtime animator controller to assign to the loaded avatar.</summary>
        public RuntimeAnimatorController? AnimatorController;

        /// <summary>Animation avatar to assign to the loaded avatar.</summary>
        public Avatar? Avatar;

        /// <inheritdoc/>
        public void PostProcess(ILoadedAv avatar, AvDataContainer _)
        {
            Animator animator = avatar.GameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = AnimatorController;
            animator.avatar = Avatar;
        }
    }
}
#endif
