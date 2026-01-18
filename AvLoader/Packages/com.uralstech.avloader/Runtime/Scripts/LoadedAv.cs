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
        
        /// <summary>A list of Unity objects to be destroyed when the avatar is disposed, e.g. post-processor created <see cref="Material"/>s.</summary>
        /// <remarks><see cref="FullRender"/> and <see cref="BustRender"/> are automatically added to this.</remarks>
        public readonly List<UnityEngine.Object> DestroyOnDispose;

        /// <summary>The type of the importer which created this <see cref="LoadedAv"/>.</summary>
        public readonly Type ImporterType;

        protected LoadedAv(GameObject gameObject, AvMetadata metadata, Texture2D? fullRender, Texture2D? bustRender, Type importerType)
        {
            GameObject = gameObject;
            Metadata = metadata;
            FullRender = fullRender;
            BustRender = bustRender;
            ImporterType = importerType;
            DestroyOnDispose = new List<UnityEngine.Object>();

            if (FullRender != null) DestroyOnDispose.Add(FullRender);
            if (BustRender != null) DestroyOnDispose.Add(BustRender);
        }

        /// <summary>Tries to get all materials of an avatar.</summary>
        public virtual IReadOnlyList<Material>? TryGetAvatarMaterials() => null;

        /// <summary>Tries to get all renderers of an avatar.</summary>
        public virtual IReadOnlyList<Renderer>? TryGetAvatarRenderers() => null;
        
        /// <summary>Tries to get all meshes of an avatar.</summary>
        public virtual IReadOnlyList<Mesh>? TryGetAvatarMeshes() => null;

        private bool _disposed = false;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            ImporterSpecificDispose();
            foreach (UnityEngine.Object obj in DestroyOnDispose)
                UnityEngine.Object.Destroy(obj);
            
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void ImporterSpecificDispose() { }
    }
}