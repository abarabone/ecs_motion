using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class VisualElementEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var container = new VisualElement();
        var iterator = this.serializedObject.GetIterator();
        if (!iterator.NextVisible(true)) return container;
        while (iterator.NextVisible(false))
        {
            var propfield = new PropertyField(iterator.Copy())
            {
                name = "PropertyField:" + iterator.propertyPath,
            };
            if (iterator.propertyPath == "m_Script" && this.serializedObject.targetObject != null)
            {
                propfield.SetEnabled(false);
            }
            container.Add(propfield);
        }
        return container;
    }
}
