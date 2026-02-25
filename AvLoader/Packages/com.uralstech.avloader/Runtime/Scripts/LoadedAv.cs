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
namespace Uralstech.AvLoader
{
    /// <summary>
    /// A loaded avatar and its associated data.
    /// </summary>
    public abstract class LoadedAv : IDisposable
    {
        /// <summary>The avatar's GameObject.</summary>
        public readonly GameObject GameObject;

        /// <summary>Metadata of the avatar.</summary>
        public readonly AvMetadata Metadata;

        /// <summary>Optional full render of the avatar.</summary>
        public readonly Texture2D? FullRender;

        /// <summary>Optional bust render of the avatar.</summary>
        public readonly Texture2D? BustRender;
        
        /// <summary>
        /// Deprecated. Use <see cref="LifetimeObjects"/> and the
        /// <see cref="RegisterLifetimeObject(UnityEngine.Object)"/> APIs instead.
        /// </summary>
        [Obsolete("Use LifetimeObjects/RegisterLifetimeObject/DeregisterLifetimeObject instead.")]
        public readonly List<UnityEngine.Object> DestroyOnDispose;

        /// <summary>Unity objects whose lifetime is owned by this avatar.</summary>
        /// <remarks>
        /// All registered objects are destroyed when the avatar is disposed.
        /// <see cref="FullRender"/> and <see cref="BustRender"/> are registered automatically.
        /// Call <see cref="DeregisterLifetimeObject(UnityEngine.Object)"/> to take ownership
        /// of an object and prevent it from being destroyed.
        /// </remarks>
        public IReadOnlyCollection<UnityEngine.Object> LifetimeObjects => _lifetimeObjects;
        private readonly HashSet<UnityEngine.Object> _lifetimeObjects;

        /// <summary>The type of the importer which created this <see cref="LoadedAv"/>.</summary>
        public readonly Type ImporterType;

        protected LoadedAv(GameObject gameObject, AvMetadata metadata, Texture2D? fullRender, Texture2D? bustRender, Type importerType)
        {
            GameObject = gameObject;
            Metadata = metadata;
            FullRender = fullRender;
            BustRender = bustRender;
            ImporterType = importerType;
            _lifetimeObjects = new HashSet<UnityEngine.Object>();

#pragma warning disable CS0618
            DestroyOnDispose = new List<UnityEngine.Object>();
#pragma warning restore CS0618

            if (FullRender != null) RegisterLifetimeObject(FullRender);
            if (BustRender != null) RegisterLifetimeObject(BustRender);
        }

        /// <summary>Registers a Unity object to be destroyed when the avatar is disposed.</summary>
        /// <remarks>Intended for post-processors and extensions that create temporary runtime objects.</remarks>
        public void RegisterLifetimeObject(UnityEngine.Object unityObject)
        {
            ThrowIfDisposed();
            if (unityObject != null)
            {
                _lifetimeObjects.Add(unityObject);
                
#pragma warning disable CS0618
                DestroyOnDispose.Add(unityObject);
#pragma warning restore CS0618
            }
        }

        /// <summary>Removes a Unity object from the avatar's lifetime ownership.</summary>
        /// <remarks>
        /// After deregistration, the caller becomes responsible for destroying the object.
        /// This is can be used to retain <see cref="FullRender"/> or <see cref="BustRender"/>.
        /// </remarks>
        public void DeregisterLifetimeObject(UnityEngine.Object unityObject)
        {
            ThrowIfDisposed();
            if (unityObject != null)
            {
                _lifetimeObjects.Remove(unityObject);
                
#pragma warning disable CS0618
                DestroyOnDispose.Remove(unityObject);
#pragma warning restore CS0618
            }
        }

        /// <summary>Tries to cast the current avatar object to <typeparamref name="T"/>.</summary>
        /// <remarks>
        /// Ultimately, this is just syntactic sugar for:
        /// <code>
        /// if (avatar is T capability)
        ///     ...
        /// else
        ///     ...
        /// </code>
        /// </remarks>
        /// <typeparam name="T">The capability type to cast to.</typeparam>
        /// <param name="capability">The casted result, or <see langword="null"/> if casting failed.</param>
        /// <returns><see langword="true"/> if casted successfully; <see langword="false"/> otherwise.</returns>
        public bool TryGetCapability<T>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? capability)
            where T : Capabilities.ICapability
        {
            ThrowIfDisposed();
            capability = this is T casted ? casted : default;
            return capability is not null;
        }

        /// <summary>Casts the current avatar object to <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">The capability type to cast to.</typeparam>
        /// <returns>The casted result.</returns>
        public T GetCapability<T>() where T : Capabilities.ICapability
        {
            ThrowIfDisposed();
            return this is not T casted
                ? throw new InvalidOperationException($"Avatar does not support capability {typeof(T).Name}")
                : casted;
        }

        private IReadOnlyList<Material>? _materialsCache;

        /// <summary>Gets all materials used by the avatar.</summary>
        /// <remarks>
        /// Results are cached. Call with <paramref name="refreshCache"/> set to
        /// <see langword="true"/> if renderers or materials change at runtime.
        /// </remarks>
        public IReadOnlyList<Material> GetAvatarMaterials(bool refreshCache = false)
        {
            ThrowIfDisposed();
            if (_materialsCache is not null && !refreshCache)
                return _materialsCache;

            if (TryGetAvatarMaterials() is IReadOnlyList<Material> materials)
                return _materialsCache = materials;

            IReadOnlyList<Renderer> renderers = GetAvatarRenderers();
            HashSet<Material> materialsSet = new(renderers.Count);

            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                    materialsSet.Add(material);
            }
            
            return _materialsCache = ArrayFromHashSet(materialsSet);
        }

        private IReadOnlyList<Renderer>? _renderersCache;

        /// <summary>Gets all renderers used by the avatar.</summary>
        /// <remarks>
        /// Results are cached. Call with <paramref name="refreshCache"/> set to
        /// <see langword="true"/> if renderers change at runtime.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Avoids double conditional, improves readability.")]
        public IReadOnlyList<Renderer> GetAvatarRenderers(bool refreshCache = false)
        {
            ThrowIfDisposed();
            if (_renderersCache is not null && !refreshCache)
                return _renderersCache;

            return _renderersCache = TryGetAvatarRenderers() is IReadOnlyList<Renderer> renderers
                ? renderers : GameObject.GetComponentsInChildren<Renderer>();
        }

        private IReadOnlyList<Mesh>? _meshesCache;

        /// <summary>Gets all meshes used by the avatar.</summary>
        /// <remarks>
        /// Results are cached. Call with <paramref name="refreshCache"/> set to
        /// <see langword="true"/> if renderers or meshes change at runtime.
        /// </remarks>
        public IReadOnlyList<Mesh> GetAvatarMeshes(bool refreshCache = false)
        {
            ThrowIfDisposed();
            if (_meshesCache is not null && !refreshCache)
                return _meshesCache;

            if (TryGetAvatarMeshes() is IReadOnlyList<Mesh> meshes)
                return _meshesCache = meshes;

            IReadOnlyList<Renderer> renderers = GetAvatarRenderers();
            HashSet<Mesh> meshesSet = new(renderers.Count);

            foreach (Renderer renderer in renderers)
            {
                switch (renderer)
                {
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        meshesSet.Add(skinnedMeshRenderer.sharedMesh); break;
                    
                    case MeshRenderer when renderer.TryGetComponent(out MeshFilter meshFilter):
                        meshesSet.Add(meshFilter.sharedMesh); break;
                }
            }
            
            return _meshesCache = ArrayFromHashSet(meshesSet);
        }

        /// <summary>Tries to get all materials of the avatar.</summary>
        public virtual IReadOnlyList<Material>? TryGetAvatarMaterials() => null;

        /// <summary>Tries to get all renderers of the avatar.</summary>
        public virtual IReadOnlyList<Renderer>? TryGetAvatarRenderers() => null;
        
        /// <summary>Tries to get all meshes of the avatar.</summary>
        public virtual IReadOnlyList<Mesh>? TryGetAvatarMeshes() => null;

        private bool _disposed = false;
        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LoadedAv));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            ImporterSpecificDispose();
            foreach (UnityEngine.Object obj in _lifetimeObjects)
                UnityEngine.Object.Destroy(obj);

            _materialsCache = null;
            _renderersCache = null;
            _meshesCache = null;
            
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void ImporterSpecificDispose() { }

        private static T[] ArrayFromHashSet<T>(HashSet<T> set)
        {
            T[] result = new T[set.Count];
            set.CopyTo(result);
            return result;
        }
    }
}