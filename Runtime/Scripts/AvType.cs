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
    /// Describes the type of a loaded model.
    /// </summary>
    public enum AvType
    {
        None = 0,
        
        /// <summary>A humanoid, full-body model.</summary>
        [EnumMember(Value = "fullbody")]
        HumanoidFullBody,
        
        /// <summary>A humanoid, half-body model.</summary>
        [EnumMember(Value = "halfbody")]
        HumanoidHalfBody,
        
        /// <summary>A humanoid, full-body model for XR use.</summary>
        [EnumMember(Value = "fullbody-xr")]
        HumanoidFullBodyXR,
    }
}