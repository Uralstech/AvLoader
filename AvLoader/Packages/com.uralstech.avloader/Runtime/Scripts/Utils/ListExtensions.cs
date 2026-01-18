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

using System.Collections.Generic;

#nullable enable
namespace Uralstech.AvLoader.Utils
{
    /// <summary>
    /// Utility extensions for lists.
    /// </summary>
    /// <remarks>
    /// This class is <see langword="public"/> to allow package users to reuse these extensions if useful.
    /// However, it should not be considered stable and is not part of the supported public API.
    /// It exists solely as an internal utility and may change or be removed at any time.
    /// </remarks>
    public static class ListExtensions
    {
        /// <summary>Creates a shallow copy of the current list.</summary>
        public static T[] CreateCopy<T>(this IReadOnlyList<T> current)
        {
            int count = current.Count;
            T[] copy = new T[count];

            for (int i = 0; i < count; i++)
                copy[i] = current[i];
            return copy;
        }
    }
}