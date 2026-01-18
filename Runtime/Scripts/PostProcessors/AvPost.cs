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
using System.Threading;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader.PostProcessors
{
    /// <summary>
    /// Fluent helper methods for creating common avatar post-processors.
    /// </summary>
    public static class AvPost
    {
        /// <summary>
        /// Creates a post-processor that executes the given action.
        /// </summary>
        /// <param name="action">The action to execute during post-processing.</param>
        public static ActionPostProcessor Do(Action<LoadedAv, AvSourceData> action) => new(action);

        /// <summary>
        /// Creates an asynchronous post-processor that executes the given function.
        /// </summary>
        /// <param name="func">The async function to execute during post-processing.</param>
        public static AsyncFuncPostProcessor DoAsync(Func<LoadedAv, AvSourceData, CancellationToken, Awaitable> func) => new(func);

        /// <summary>
        /// Creates a conditional post-processor that runs only when the given condition is met.
        /// </summary>
        /// <param name="condition">A predicate determining whether the post-processor should run.</param>
        /// <param name="onCondition">The post-processor to execute when the condition is met.</param>
        public static ConditionalPostProcessor When(Func<LoadedAv, AvSourceData, bool> condition, IAvPostProcessor onCondition) => new(condition, onCondition);

        /// <summary>
        /// Creates a conditional asynchronous post-processor that runs only when the given condition is met.
        /// </summary>
        /// <param name="condition">A predicate determining whether the post-processor should run.</param>
        /// <param name="onCondition">The async post-processor to execute when the condition is met.</param>
        public static ConditionalPostProcessor When(Func<LoadedAv, AvSourceData, bool> condition, IAsyncAvPostProcessor onCondition) => new(condition, onCondition);

        /// <summary>
        /// Creates a conditional post-processor that runs only when the avatar is imported using an importer of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The importer type that triggers execution of <paramref name="onCondition"/>.</typeparam>
        /// <param name="onCondition">The post-processor to execute when the condition is met.</param>
        public static ConditionalPostProcessor WhenImportedWith<T>(IAvPostProcessor onCondition) where T: IAvImporter =>
            new(static (av, _) => typeof(T).IsAssignableFrom(av.ImporterType), onCondition);

        /// <summary>
        /// Creates a conditional asynchronous post-processor that runs only when the avatar is imported using an importer of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The importer type that triggers execution of <paramref name="onCondition"/>.</typeparam>
        /// <param name="onCondition">The post-processor to execute when the condition is met.</param>
        public static ConditionalPostProcessor WhenImportedWith<T>(IAsyncAvPostProcessor onCondition) where T: IAvImporter =>
            new(static (av, _) => typeof(T).IsAssignableFrom(av.ImporterType), onCondition);

        /// <summary>
        /// Creates a conditional post-processor that runs only when the avatar data is loaded using a loader of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The loader type that triggers execution of <paramref name="onCondition"/>.</typeparam>
        /// <param name="onCondition">The post-processor to execute when the condition is met.</param>
        public static ConditionalPostProcessor WhenLoadedWith<T>(IAvPostProcessor onCondition) where T: IAvDataLoader =>
            new(static (_, raw) => typeof(T).IsAssignableFrom(raw.DataLoaderType), onCondition);

        /// <summary>
        /// Creates a conditional asynchronous post-processor that runs only when the avatar data is loaded using a loader of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The loader type that triggers execution of <paramref name="onCondition"/>.</typeparam>
        /// <param name="onCondition">The post-processor to execute when the condition is met.</param>
        public static ConditionalPostProcessor WhenLoadedWith<T>(IAsyncAvPostProcessor onCondition) where T: IAvDataLoader =>
            new(static (_, raw) => typeof(T).IsAssignableFrom(raw.DataLoaderType), onCondition);

        /// <summary>
        /// Creates a post-processor that caches the loaded avatar's source data to disk.
        /// </summary>
        /// <remarks>
        /// This only works for formats where the model does not depend on resources
        /// stored separately from the source file (<see cref="AvSourceData.Model"/>).
        /// For example, avatars loaded from OBJ files with external texture files
        /// will not work.
        /// </remarks>
        /// <param name="directory">The base directory to save the cached avatar data to.</param>
        /// <param name="useAvatarIdAsChildDirName">If <see langword="true"/>, the avatar is stored in a child directory named after its ID.</param>
        /// <param name="fullRenderImageFormat">The image format to use when saving the full-body render.</param>
        /// <param name="bustRenderImageFormat">The image format to use when saving the bust render.</param>
        /// <param name="fullRenderJPEGQuality">JPEG quality to use when saving the full-body render.</param>
        /// <param name="bustRenderJPEGQuality">JPEG quality to use when saving the bust render.</param>
        /// <param name="ignoreOriginalLoaderType">If <see langword="true"/>, caching will occur even if the avatar was originally loaded from disk.</param>
        public static CachePostProcessor CacheTo(string directory, bool useAvatarIdAsChildDirName = true,
            AvImageFileExtension fullRenderImageFormat = AvImageFileExtension.JPG,
            AvImageFileExtension bustRenderImageFormat = AvImageFileExtension.JPG,
            int fullRenderJPEGQuality = 100, int bustRenderJPEGQuality = 100,
            bool ignoreOriginalLoaderType = false)
        {
            return new CachePostProcessor(directory, useAvatarIdAsChildDirName)
            {
                FullRenderImageFormat = fullRenderImageFormat,
                BustRenderImageFormat = bustRenderImageFormat,
                FullRenderJPEGQuality = fullRenderJPEGQuality,
                BustRenderJPEGQuality = bustRenderJPEGQuality,
                IgnoreOriginalLoaderType = ignoreOriginalLoaderType,
            };
        }

#if ANIMATION_INSTALLED
        /// <summary>
        /// Creates a post-processor that configures an <see cref="Animator"/> on the loaded avatar.
        /// </summary>
        /// <param name="animatorController">The runtime animator controller to assign to the avatar.</param>
        /// <param name="avatar">The animation avatar to assign if no gender-specific match is found.</param>
        /// <param name="avGenderToAvatarLookup">Optional lookup table mapping avatar gender to animation avatars.</param>
        /// <param name="overrideAvatar">If <see langword="true"/>, any existing animation avatar will be overridden.</param>
        public static AnimatorPostProcessor ConfigureAnimator(RuntimeAnimatorController? animatorController = null, Avatar? avatar = null,
            IReadOnlyDictionary<AvGender, Avatar>? avGenderToAvatarLookup = null, bool overrideAvatar = false)
        {
            return new AnimatorPostProcessor()
            {
                AnimatorController = animatorController,
                Avatar = avatar, AvGenderToAvatarLookup = avGenderToAvatarLookup,
                OverrideAvatar = overrideAvatar,
            };
        }
#endif

        /// <summary>
        /// Creates a post-processor that swaps shaders of the loaded avatar based on the given <paramref name="config"/>.
        /// </summary>
        /// <param name="config">Configuration that includes a map of shaders to be replaced and their replacements.</param>
        public static ShaderSwapPostProcessor SwapShaders(ShaderSwapConfig config) => new(config);

        /// <summary>
        /// Creates a post-processor that recreates avatar materials using alternative shaders and applies configured property and keyword overrides.
        /// </summary>
        /// <param name="config">Configuration that defines material override rules, including source shaders, target shaders, and property and keyword mappings.</param>
        public static MaterialOverridePostProcessor OverrideMaterials(MaterialOverrideConfig config) => new(config);
    }
}