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

using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader.PostProcessors
{
    /// <summary>
    /// Post-processing step that swaps the avatar's shaders with the provided set of shaders.
    /// </summary>
    public class ShaderSwapPostProcessor : IAvPostProcessor
    {
        /// <summary>Configuration which defines the shaders to swap.</summary>
        public ShaderSwapConfig Configuration;

        public ShaderSwapPostProcessor(ShaderSwapConfig configuration)
        {
            Configuration = configuration;
        }

        /// <inheritdoc/>
        public void PostProcess(LoadedAv avatar, AvSourceData _)
        {
            foreach (Material? material in avatar.GetAvatarMaterials())
            {
                if (material == null) continue;

                if (Configuration.ShaderMap.TryGetValue(material.shader, out Shader swap))
                    material.shader = swap;
                else if (Configuration.FallbackShader != null && material.shader != Configuration.FallbackShader)
                    material.shader = Configuration.FallbackShader;
            }
        }
    }
}