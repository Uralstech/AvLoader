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
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// A loaded avatar and its associated data.
    /// </summary>
    public interface ILoadedAv : IDisposable
    {
        /// <summary>The avatar's GameObject.</summary>
        public GameObject GameObject { get; }

        /// <summary>Metadata of the avatar.</summary>
        public AvMetadata Metadata { get; }

        /// <summary>Optional full render of the avatar.</summary>
        public Texture2D? FullRender { get; }

        /// <summary>Optional bust render of the avatar.</summary>
        public Texture2D? BustRender { get; }
        
        /// <summary>The type of the importer which created this <see cref="ILoadedAv"/>.</summary>
        public Type ImporterType { get; }
    }
}