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
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Post-processing step which adds or updates an animation controller in the loaded avatar.
    /// Requries Unity's animation module to be enabled.
    /// </summary>
    public class AddOrUpdateAnimationController : IAvPostProcessor
    {
        /// <summary>Runtime animator controller to assign to the loaded avatar.</summary>
        public RuntimeAnimatorController? AnimatorController;

        /// <summary>
        /// Animation avatar to assign to the loaded avatar.
        /// </summary>
        /// <remarks>
        /// If the avatar already has an animator, this won't be assigned unless <see cref="OverrideAvatar"/> is <see langword="true"/>.
        /// </remarks>
        public Avatar? Avatar;

        /// <summary>
        /// Lookup table for when you need to assign animation avatars
        /// based on the avatar's gender. If not found here, falls back to <see cref="Avatar"/>.
        /// </summary>
        /// <remarks>
        /// If the avatar already has an animator, this won't be assigned unless <see cref="OverrideAvatar"/> is <see langword="true"/>.
        /// </remarks>
        public IReadOnlyDictionary<AvGender, Avatar>? AvGenderToAvatarLookup;

        /// <summary>If the avatar already has an animation avatar assigned, should it be overridden?</summary>
        public bool OverrideAvatar;

        /// <inheritdoc/>
        public void PostProcess(LoadedAv avatar, AvDataContainer _)
        {
            if (!avatar.GameObject.TryGetComponent(out Animator animator))
                animator = avatar.GameObject.AddComponent<Animator>();
            
            animator.runtimeAnimatorController = AnimatorController;        
            if (animator.avatar == null || OverrideAvatar)
                animator.avatar = AvGenderToAvatarLookup?.TryGetValue(avatar.Metadata.Gender, out Avatar animAvatar) == true ? animAvatar : Avatar;
        }
    }
}
#endif
