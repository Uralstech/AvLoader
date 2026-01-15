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

using System.Runtime.Serialization;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// The gender a loaded avatar or of the outfit it is wearing.
    /// Can be useful for using specific animation avatars for
    /// different types.
    /// </summary>
    public enum AvGender
    {
        None = 0,

        [EnumMember(Value = "masculine")]
        Masculine,

        [EnumMember(Value = "feminine")]
        Feminine,

        [EnumMember(Value = "neutral")]
        Neutral,
    }
}