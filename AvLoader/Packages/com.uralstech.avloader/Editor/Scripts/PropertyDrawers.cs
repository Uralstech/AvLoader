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
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Uralstech.AvLoader.PostProcessors.Rendering;

#nullable enable
namespace Uralstech.AvLoader.Editor
{
    [CustomPropertyDrawer(typeof(EditorMaterialOverrideDefinition))]
    public class EditorMaterialOverrideDefinitionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            root.Add(new PropertyField(property.FindPropertyRelative(nameof(EditorMaterialOverrideDefinition.ShaderMapping))));
            
            SerializedProperty propertyMappings = property.FindPropertyRelative(nameof(EditorMaterialOverrideDefinition.PropertyMappings));
            root.Add(new PropertyField(propertyMappings));

            HelpBox propertyMappingsHelpBox = new(
                "Invalid property mappings.\n" +
                "- Source and target property names must not be empty.\n" +
                "- Each source and each target property must be unique.",
                HelpBoxMessageType.Error
            );

            ValidatePropertyMappings(propertyMappings, propertyMappingsHelpBox);
            root.TrackPropertyValue(propertyMappings, p => ValidatePropertyMappings(p, propertyMappingsHelpBox));
            root.Add(propertyMappingsHelpBox);

            SerializedProperty keywordMappings = property.FindPropertyRelative(nameof(EditorMaterialOverrideDefinition.KeywordMappings));
            root.Add(new PropertyField(keywordMappings, "Keyword Mappings (Advanced)"));

            HelpBox keywordMappingsHelpBox = new(
                "Invalid keyword mappings.\n" +
                "- Source and target keyword names must not be empty.",
                HelpBoxMessageType.Error
            );

            ValidateKeywordMappings(keywordMappings, keywordMappingsHelpBox);
            root.TrackPropertyValue(keywordMappings, p => ValidateKeywordMappings(p, keywordMappingsHelpBox));
            root.Add(keywordMappingsHelpBox);
            return root;
        }

        private static void ValidatePropertyMappings(SerializedProperty property, HelpBox helpBox)
        {
            helpBox.style.display = DisplayStyle.None;
            if (property.arraySize == 0)
                return;

            int count = property.arraySize;
            HashSet<string> sources = new(count);
            HashSet<string> targets = new(count);
            for (int i = 0; i < count; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                string source = element.FindPropertyRelative(nameof(ShaderPropertyMapping.Source)).stringValue;
                string target = element.FindPropertyRelative(nameof(ShaderPropertyMapping.Target)).stringValue;
                
                if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)
                    || !sources.Add(source) || !targets.Add(target))
                {
                    helpBox.style.display = DisplayStyle.Flex;
                    return;
                }
            }
        }

        private static void ValidateKeywordMappings(SerializedProperty property, HelpBox helpBox)
        {
            helpBox.style.display = DisplayStyle.None;
            if (property.arraySize == 0)
                return;

            int count = property.arraySize;
            for (int i = 0; i < count; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                string source = element.FindPropertyRelative(nameof(ShaderKeywordMapping.Source)).stringValue;
                string target = element.FindPropertyRelative(nameof(ShaderKeywordMapping.Target)).stringValue;

                if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                {
                    helpBox.style.display = DisplayStyle.Flex;
                    return;
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(ShaderPropertyMapping))]
    public class ShaderPropertyMappingPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            root.style.flexDirection = FlexDirection.Row;
            root.AddTwoRowPropertyField(property, nameof(ShaderPropertyMapping.Source), "Source");
            root.AddTwoRowPropertyField(property, nameof(ShaderPropertyMapping.Target), "Target");
            root.AddTwoRowPropertyField(property, nameof(ShaderPropertyMapping.Type), "Type", w: 75);
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

            root.AddTwoRowPropertyField(property, nameof(ShaderKeywordMapping.Source), "Source");
            root.AddTwoRowBoolField(property, nameof(ShaderKeywordMapping.SourceRequiredState));

            root.AddTwoRowPropertyField(property, nameof(ShaderKeywordMapping.Target), "Target");
            root.AddTwoRowBoolField(property, nameof(ShaderKeywordMapping.TargetResultState));
            return root;
        }
    }

    [CustomPropertyDrawer(typeof(ShaderMapping))]
    public class ShaderMappingPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            root.style.flexDirection = FlexDirection.Row;
            root.AddTwoRowPropertyField(property, nameof(ShaderMapping.Source), "Source");
            root.AddTwoRowPropertyField(property, nameof(ShaderMapping.Target), "Target");
            return root;
        }
    }
}