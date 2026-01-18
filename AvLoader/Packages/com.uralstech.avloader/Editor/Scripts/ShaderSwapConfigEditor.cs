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

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Uralstech.AvLoader.PostProcessors;

#nullable enable
namespace Uralstech.AvLoader.Editor
{
    [CustomEditor(typeof(ShaderSwapConfig))]
    public class ShaderSwapConfigEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            ShaderSwapConfig config = (ShaderSwapConfig)target;

            root.Add(new PropertyField(serializedObject.FindProperty(nameof(ShaderSwapConfig.FallbackShader))));

            SerializedProperty property = serializedObject.FindProperty(nameof(ShaderSwapConfig._serializedShaderMap));
            property.isExpanded = true;
            root.Add(new PropertyField(property, "Shader Map"));

            HelpBox helpBox = new(
                "Invalid configuration. Ensure that there are no duplicate sources and that no value is null.",
                HelpBoxMessageType.Error
            );

            helpBox.style.display = config.IsValid() ? DisplayStyle.None : DisplayStyle.Flex;
            root.Add(helpBox);

            root.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                helpBox.style.display = config.IsValid() ? DisplayStyle.None : DisplayStyle.Flex;
            });
            
            return root;
        }
    }
}
