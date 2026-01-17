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
    /// Wrapper for a raw container of avatar data.
    /// </summary>
    public class AvSourceData
    {
        /// <summary>The model as a byte[].</summary>
        public readonly byte[] Model;

        /// <summary>The file format of the model.</summary>
        public readonly AvModelFileExtension ModelFormat;

        /// <summary>The path/URI to model.</summary>
        public readonly string? ModelPath;

        /// <summary>Metadata of the avatar.</summary>
        public readonly AvMetadata Metadata;

        /// <summary>Optional full render of the avatar.</summary>
        public readonly Texture2D? FullRender;

        /// <summary>Optional bust render of the avatar.</summary>
        public readonly Texture2D? BustRender;

        /// <summary>The type of the data loader which created this.</summary>
        public readonly Type DataLoaderType;

        public AvSourceData(byte[] model, AvModelFileExtension modelFormat, string? modelPath, AvMetadata metadata,
            Type dataLoaderType, Texture2D? fullRender = null, Texture2D? bustRender = null)
        {
            Model = model;
            ModelFormat = modelFormat;
            ModelPath = modelPath;
            Metadata = metadata;
            DataLoaderType = dataLoaderType;
            FullRender = fullRender;
            BustRender = bustRender;
        }
    }
}