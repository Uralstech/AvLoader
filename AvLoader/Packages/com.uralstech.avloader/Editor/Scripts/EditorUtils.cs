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

#nullable enable
namespace Uralstech.AvLoader.Editor
{
    public static class EditorUtils
    {
        public static void AddTwoRowPropertyField(this VisualElement current, SerializedProperty drawing, string relativePropertyName, string label, int? w = null)
        {
            VisualElement baseElement = new();
            baseElement.style.flexBasis = 0;
            baseElement.style.flexGrow = 1;

            if (w is int width)
                baseElement.style.width = width;

            baseElement.Add(new Label(label));
            baseElement.Add(new PropertyField(drawing.FindPropertyRelative(relativePropertyName), ""));
            current.Add(baseElement);
        }

        public static void AddTwoRowBoolField(this VisualElement current, SerializedProperty drawing, string relativePropertyName)
        {
            VisualElement baseElement = new();
            baseElement.style.width = 15;
            baseElement.style.marginLeft = 2;

            VisualElement space = new();
            space.style.flexGrow = 1;
            baseElement.Add(space);

            baseElement.Add(new PropertyField(drawing.FindPropertyRelative(relativePropertyName), ""));
            current.Add(baseElement);
        }
    }
}