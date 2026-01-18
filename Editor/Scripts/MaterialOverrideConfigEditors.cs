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
using Uralstech.AvLoader.PostProcessors.Rendering;

#nullable enable
namespace Uralstech.AvLoader.Editor
{
    [CustomEditor(typeof(MaterialOverrideConfig))]
    public class MaterialOverrideConfigEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            SerializedProperty property = serializedObject.FindProperty(nameof(MaterialOverrideConfig._materialOverrides));
            property.isExpanded = true;

            root.Add(new PropertyField(property));
            return root;
        }
    }

    [CustomPropertyDrawer(typeof(MaterialOverrideDefinition))]
    public class MaterialOverrideDefinitionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            root.Add(new PropertyField(property.FindPropertyRelative(nameof(MaterialOverrideDefinition.ShaderMapping))));
            root.Add(new PropertyField(property.FindPropertyRelative(nameof(MaterialOverrideDefinition.PropertyMappings))));
            root.Add(new PropertyField(property.FindPropertyRelative(nameof(MaterialOverrideDefinition.KeywordMappings))));
            return root;
        }
    }
    
    [CustomPropertyDrawer(typeof(ShaderPropertyMapping))]
    public class ShaderPropertyMappingPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            root.style.flexDirection = FlexDirection.Row;

            VisualElement key = new();
            key.style.flexBasis = 0;
            key.style.flexGrow = 1;

            key.Add(new Label("Source"));
            key.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderPropertyMapping.Source)), ""));
            root.Add(key);

            VisualElement value = new();
            value.style.flexBasis = 0;
            value.style.flexGrow = 1;

            value.Add(new Label("Target"));
            value.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderPropertyMapping.Target)), ""));
            root.Add(value);

            VisualElement type = new();
            type.style.width = 75;

            type.Add(new Label("Type"));
            type.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderPropertyMapping.Type)), ""));
            root.Add(type);

            return root;
        }
    }
    
    [CustomPropertyDrawer(typeof(ShaderKeywordMapping))]
    public class ShaderKeywordMappingPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            root.style.flexDirection = FlexDirection.Row;

            VisualElement key = new();
            key.style.flexBasis = 0;
            key.style.flexGrow = 1;

            key.Add(new Label("Source"));
            key.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderKeywordMapping.Source)), ""));
            root.Add(key);

            VisualElement sourceStatus = new();
            sourceStatus.style.width = 15;
            sourceStatus.style.marginLeft = 2;

            VisualElement space1 = new();
            space1.style.flexGrow = 1;
            sourceStatus.Add(space1);

            sourceStatus.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderKeywordMapping.SourceRequiredState)), ""));
            root.Add(sourceStatus);

            VisualElement value = new();
            value.style.flexBasis = 0;
            value.style.flexGrow = 1;

            value.Add(new Label("Target"));
            value.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderKeywordMapping.Target)), ""));
            root.Add(value);

            VisualElement targetStatus = new();
            targetStatus.style.width = 15;
            targetStatus.style.marginLeft = 2;

            VisualElement space2 = new();
            space2.style.flexGrow = 1;
            targetStatus.Add(space2);

            targetStatus.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderKeywordMapping.TargetResultState)), ""));
            root.Add(targetStatus);

            return root;
        }
    }
}
