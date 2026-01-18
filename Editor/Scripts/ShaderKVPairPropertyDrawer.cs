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
using Uralstech.AvLoader.PostProcessors.Rendering;

#nullable enable
namespace Uralstech.AvLoader.Editor
{
    [CustomPropertyDrawer(typeof(ShaderMapping))]
    public class ShaderMappingPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            root.style.flexDirection = FlexDirection.Row;

            VisualElement key = new();
            key.style.flexBasis = 0;
            key.style.flexGrow = 1;

            key.Add(new Label("Source"));
            key.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderMapping.Source)), ""));
            root.Add(key);

            VisualElement value = new();
            value.style.flexBasis = 0;
            value.style.flexGrow = 1;

            value.Add(new Label("Target"));
            value.Add(new PropertyField(property.FindPropertyRelative(nameof(ShaderMapping.Target)), ""));
            root.Add(value);

            return root;
        }
    }
}
