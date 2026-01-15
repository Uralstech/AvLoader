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

using System.Threading;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Interface for an avatar data loader.
    /// </summary>
    /// <remarks>
    /// The role of the <see cref="IAvDataLoader"/> is to fetch the raw binary and JSON data
    /// associated with the avatar. They can be chained up to create fallbacks, for example,
    /// a <see cref="FileAvDataLoader"/> could check local caches for existing avatars, then
    /// fall back to a <see cref="URIAvDataLoader"/> to download it from remote sources.
    /// 
    /// You could also implement your own data loaders to load avatars using APIs provided
    /// by cloud services, like Firebase.
    /// </remarks>
    public interface IAvDataLoader 
    {
        /// <summary>
        /// Tries loading an avatar into an <see cref="AvDataContainer"/>.
        /// </summary>
        /// <param name="throwOnFail">Should this method throw errors on failures or log them as warnings and return <see langword="null"/>?</param>
        /// <returns>The loaded data if successful; <see langword="null"/> on failure.</returns>
        public Awaitable<AvDataContainer?> LoadAvatarAsync(bool throwOnFail, CancellationToken token = default);
    }

    /// <summary>
    /// Interface for an avatar importer.
    /// </summary>
    /// <remarks>
    /// The role of the <see cref="IAvImporter"/> is to parse data returned by an
    /// <see cref="IAvDataLoader"/> and bring the avatar into Unity-space as a
    /// GameObject. Like <see cref="IAvDataLoader"/>s, they can be chained up to
    /// create fallbacks, for example, a <see cref="GLTFAvImporter"/> for glTF
    /// support, falling back to one for .fbx support, etc.
    /// </remarks>
    public interface IAvImporter
    {
        /// <summary>
        /// Returns <see langword="true"/> if this importer can handle the given format; <see langword="false"/> otherwise.
        /// </summary>
        public bool SupportsFormat(AvModelFileExtension format);

        /// <summary>
        /// Tries to import the avatar into a scene as a disabled GameObject, along with metadata and any renders.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="ILoadedAv"/> may contain code to handle requirements of format-supporting plugins.
        /// For example, <see cref="GLTFAvImporter"/> depends on glTFast, which requires that GltfImport object used
        /// to import the avatar be active alongside the avatar's GameObject, and should be disposed after the avatar
        /// is no longer needed. Thus, <see cref="ILoadedAv"/> implements <see cref="System.IDisposable"/>.
        /// </remarks>
        /// <param name="rawData">The raw avatar data to process.</param>
        /// <param name="throwOnFail">Should this method throw errors on failures or log them as warnings and return <see langword="null"/>?</param>
        /// <returns>The loaded avatar if successful; <see langword="null"/> on failure.</returns>
        public Awaitable<ILoadedAv?> ImportAvatarAsync(AvDataContainer rawData, bool throwOnFail, CancellationToken token = default);
    }

    /// <summary>
    /// A simple script that can perform a post-processing step on an imported avatar.
    /// </summary>
    public interface IAvPostProcessor
    {
        /// <summary>
        /// Runs a post-processing step on the given avatar.
        /// </summary>
        /// <param name="avatar">The loaded avatar to process.</param>
        /// <param name="rawData">The raw data of the loaded avatar.</param>
        public void PostProcess(ILoadedAv avatar, AvDataContainer rawData);
    }

    /// <summary>
    /// A simple script that can perform an asynchronous post-processing step on an imported avatar.
    /// </summary>
    public interface IAvAsyncPostProcessor
    {
        /// <summary>
        /// Runs a post-processing step on the given avatar.
        /// </summary>
        /// <param name="avatar">The loaded avatar to process.</param>
        /// <param name="rawData">The raw data of the loaded avatar.</param>
        public Awaitable PostProcessAsync(ILoadedAv avatar, AvDataContainer rawData, CancellationToken token = default);
    }
}