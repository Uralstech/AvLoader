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

    [CustomPropertyDrawer(typeof(ShaderSwapConfig.ShaderKVPair))]
    public class ShaderKVPairPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            root.style.flexDirection = FlexDirection.Row;

            VisualElement key = new();
            key.style.flexBasis = 0;
            key.style.flexGrow = 1;

            key.Add(new Label("Source"));
            key.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderSwapConfig.ShaderKVPair._source)), ""));
            root.Add(key);

            VisualElement value = new();
            value.style.flexBasis = 0;
            value.style.flexGrow = 1;

            value.Add(new Label("Target"));
            value.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderSwapConfig.ShaderKVPair._target)), ""));
            root.Add(value);

            return root;
        }
    }
}
