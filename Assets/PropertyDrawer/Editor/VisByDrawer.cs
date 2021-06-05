using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

//[CustomPropertyDrawer(typeof(HideByAttribute))]
public class VisByDrawer : PropertyDrawer
{
    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    EditorGUI.PropertyField(position, property, label, true);
    //}
    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //{
    //    return base.GetPropertyHeight(property, label);
    //}
    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    var attr = (HideByAttribute)this.attribute;

    //    EditorGUILayout.PropertyField(property, label);
    //}
    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    EditorGUI.BeginChangeCheck();

    //    _target.EnableShadow = EditorGUILayout.ToggleLeft("EnableShadow", _target.EnableShadow);
    //    if (_target.EnableShadow)
    //    {
    //        PropertyDrawerUtility.DrawDefaultGUI(position, property, label);
    //    }

    //    // GUIÇÃçXêVÇ™Ç†Ç¡ÇΩÇÁé¿çs
    //    if (EditorGUI.EndChangeCheck())
    //    {
    //        EditorUtility.SetDirty(_target);
    //    }
    //}

    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //{
    //    return PropertyDrawerUtility.GetDefaultPropertyHeight(property, label);
    //}
    //public override VisualElement CreatePropertyGUI(SerializedProperty property)
    //{
    //    //EditorGUI.BeginChangeCheck();

    //    var attr = (HideByAttribute)this.attribute;
    //    var isUse = property.serializedObject.FindProperty(attr.fieldname)?.boolValue;
    //    Debug.Log(isUse);

    //    var container = new VisualElement();
    //    var propfield = new PropertyField(property);
    //    container.Add(propfield);
    //    return container;
    //}
}