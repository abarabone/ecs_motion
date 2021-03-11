
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof (CompactAttribute))]
internal class CompactDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //EditorGUIUtility.LookLikeControls();

        position.xMin += 4;
        position.xMax -= 4;

        switch (property.propertyType)
        {
			case SerializedPropertyType.Quaternion:
            case SerializedPropertyType.Vector4:
                property.vector4Value = EditorGUI.Vector4Field(position, label.text, property.vector4Value);
                break;

            case SerializedPropertyType.Vector3:
                property.vector3Value = EditorGUI.Vector3Field(position, label.text, property.vector3Value);
                break;

            case SerializedPropertyType.Vector2:
                property.vector2Value = EditorGUI.Vector2Field(position, label.text, property.vector2Value);
                break;

            case SerializedPropertyType.Rect:
                property.rectValue = EditorGUI.RectField(position, label.text, property.rectValue);
                break;

            case SerializedPropertyType.Bounds:
                EditorGUI.LabelField(position, label.text);
                position.y += 20;
                property.boundsValue = EditorGUI.BoundsField(position, property.boundsValue);
                break;
        }
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        var extraHeight = 0f;
        switch (prop.propertyType)
        {
			case SerializedPropertyType.Quaternion:
            case SerializedPropertyType.Vector4:
                //extraHeight = 20;
                break;

            case SerializedPropertyType.Vector3:
                //extraHeight = 20;
                break;

            case SerializedPropertyType.Vector2:
                //extraHeight = 20;
                break;

            case SerializedPropertyType.Rect:
                //extraHeight = 40;
                break;

            case SerializedPropertyType.Bounds:
                //extraHeight = 40;
                break;
        }
        return base.GetPropertyHeight(prop, label) + extraHeight;
    }
}