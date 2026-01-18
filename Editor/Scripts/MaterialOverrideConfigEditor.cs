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
    [CustomEditor(typeof(MaterialOverrideConfig))]
    public class MaterialOverrideConfigEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            MaterialOverrideConfig config = (MaterialOverrideConfig)target;

            SerializedProperty property = serializedObject.FindProperty(nameof(MaterialOverrideConfig._serializedMaterialOverrides));
            property.isExpanded = true;
            root.Add(new PropertyField(property, "Material Overrides"));

            HelpBox helpBox = new(
                "Invalid material override configuration.\n" + 
                "- Each source shader must be unique.\n" +
                "- Source and target shaders must be assigned.\n" +
                "- All property and keyword mappings and rules must be valid.",
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