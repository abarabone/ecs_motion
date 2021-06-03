//using UnityEngine;
//using UnityEditor;
//using UnityEngine.UIElements;
//using UnityEditor.UIElements;

//[CustomPropertyDrawer(typeof(HideByAttribute))]
//public class HideByDrawer : PropertyDrawer
//{
//    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    //{
//    //    var attr = (HideByAttribute)this.attribute;

//    //    EditorGUILayout.PropertyField(property, label);
//    //}
//    public override VisualElement CreatePropertyGUI(SerializedProperty property)
//    {
//        EditorGUI.BeginChangeCheck();

//        var attr = (HideByAttribute)this.attribute;
//        var isUse = property.serializedObject.FindProperty(attr.fieldname)?.boolValue;
//        Debug.LogError(isUse);

//        if (EditorGUI.EndChangeCheck())
//        {
//            EditorUtility.SetDirty(property.serializedObject);
//        }

//        var container = new VisualElement();
//        if (isUse.GetValueOrDefault())
//        {
//            return null;
//            //var propfield = new PropertyField(property.Copy())
//            //{
//            //    name = "PropertyField:" + property.propertyPath,
//            //};
//            //if (property.propertyPath == "m_Script")
//            //{
//            //    propfield.SetEnabled(false);
//            //}
//            //container.Add(propfield);
//        }
//        return container;
//    }
//}