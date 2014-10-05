#region License (MIT)
/*Copyright (C) 2014, Rasmus Lindén - Brainswitch Machina

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
Except as contained in this notice, the name(s) of the above copyright holders shall not be used in advertising or otherwise to promote the sale, use or other dealings in this Software without prior written authorization.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

// Wish there was a way to check if Unity version is equal to and above...
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5
#define hasSerializedPropertyTypeQuaternion
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// ShowInInspector v.0.5.1
/// Todo: Better support for ExecuteInEditor?
/// </summary>
public static class ShowInInspectorUtilities
{
    static MethodInfo _gradientField = typeof(EditorGUILayout).GetMethod("GradientField", BindingFlags.Static | BindingFlags.NonPublic, null, new System.Type[] { typeof(string), typeof(Gradient), typeof(GUILayoutOption[]) }, null);
    public static Gradient GradientField(string label, Gradient gradient)
    {
        return (Gradient)_gradientField.Invoke(null, new object[] { label, gradient, null });
    }

    private static Dictionary<Editor, EditorData> _editors = new Dictionary<Editor, EditorData>();
    private class EditorData
    {
        ShowInInspectorUtilities.PropertyData[] _properties;
        public PropertyData[] properties { get { return _properties; } }

        ShowInInspectorUtilities.FieldData[] _fields;
        public FieldData[] fields { get { return _fields; } }

        public EditorData(Editor editor)
        {
            _properties = GetProperties(editor.target);
            _fields = GetFields(editor.target);
        }
    }

    public static void DrawShowInInspector(this Editor editor)
    {
        EditorData editorData;
        if (!_editors.ContainsKey(editor))
        {
            editorData = new EditorData(editor);
            _editors.Add(editor, editorData);
        }
        editorData = _editors[editor];

        // Fields first. Todo: Other colour?
        Draw(editorData.fields);

        // Then properties
        if (editorData.properties.Length > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Properties:");
            Draw(editorData.properties);
        }
    }

    private static void Draw(BaseData[] showInInspectorDatas)
    {

        GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

        EditorGUILayout.BeginVertical(emptyOptions);

        foreach (BaseData field in showInInspectorDatas)
        {
            EditorGUILayout.BeginHorizontal(emptyOptions);
            EditorGUI.BeginDisabledGroup(!field.hasSetter);

            switch (field.inspectorType)
            {
                case SerializedPropertyType.Integer:
                    field.SetValue(EditorGUILayout.IntField(field.name, (int)field.GetValue()));
                    break;

                case SerializedPropertyType.Float:
                    field.SetValue(EditorGUILayout.FloatField(field.name, (float)field.GetValue()));
                    break;

                case SerializedPropertyType.Boolean:
                    field.SetValue(EditorGUILayout.Toggle(field.name, (bool)field.GetValue()));
                    break;

                case SerializedPropertyType.String:
                    field.SetValue(EditorGUILayout.TextField(field.name, (String)field.GetValue()));
                    break;

                case SerializedPropertyType.Color:
                    field.SetValue(EditorGUILayout.ColorField(field.name, (Color)field.GetValue()));
                    break;

                case SerializedPropertyType.ObjectReference:
                    var objRef = field.GetValue();
                    field.SetValue(EditorGUILayout.ObjectField(field.name, (Object)objRef, objRef.GetType(), true));
                    break;

                case SerializedPropertyType.LayerMask:
                    field.SetValue(EditorGUILayout.LayerField(field.name, (LayerMask)field.GetValue()));
                    break;

                case SerializedPropertyType.Enum:
                    field.SetValue(EditorGUILayout.EnumPopup(field.name, (Enum)field.GetValue()));
                    break;

                case SerializedPropertyType.Vector2:
                    field.SetValue(EditorGUILayout.Vector2Field(field.name, (Vector2)field.GetValue()));
                    break;

                case SerializedPropertyType.Vector3:
                    field.SetValue(EditorGUILayout.Vector3Field(field.name, (Vector3)field.GetValue()));
                    break;

                case SerializedPropertyType.AnimationCurve:
                    field.SetValue(EditorGUILayout.CurveField(field.name, (AnimationCurve)field.GetValue()));
                    break;

                case SerializedPropertyType.Bounds:
                    field.SetValue(EditorGUILayout.BoundsField(field.name, (Bounds)field.GetValue()));
                    break;

                // Haven't been able to get Gradients to work since the Editor for Gradients rely on SerializedProperty, todo: create my own gradient editor...
                //case SerializedPropertyType.Gradient:
                //    field.SetValue(GradientField(field.name, (Gradient)field.GetValue()));
                //    break;

#if hasSerializedPropertyTypeQuaternion
                case SerializedPropertyType.Quaternion:
                    Vector3 eulerAngles = ((Quaternion)field.GetValue()).eulerAngles;
                    field.SetValue(Quaternion.Euler(EditorGUILayout.Vector3Field(field.name, eulerAngles)));
                    break;
#endif

                case SerializedPropertyType.Rect:
                    field.SetValue(EditorGUILayout.RectField(field.name, (Rect)field.GetValue()));
                    break;


                default:
                    Debug.LogError("ShowInInspector: Failed drawing " + field.name + ", " + field.instance);
                    break;

            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.EndVertical();

    }

    private static PropertyData[] GetProperties(System.Object obj)
    {
        List<PropertyData> properties = new List<PropertyData>();

        PropertyInfo[] typeInfos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (PropertyInfo info in typeInfos)
        {
            Attribute[] attributes = Attribute.GetCustomAttributes(info, typeof(ShowInInspectorAttribute));
            if (!(attributes.Length > 0))
                continue;

            PropertyData data = new PropertyData(obj, info);
            properties.Add(data);
        }

        return properties.ToArray();

    }

    private static FieldData[] GetFields(System.Object obj)
    {
        List<FieldData> fields = new List<FieldData>();
        FieldInfo[] infos = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (FieldInfo info in infos)
        {
            Attribute[] attributes = Attribute.GetCustomAttributes(info, typeof(ShowInInspectorAttribute));
            if (!(attributes.Length > 0))
                continue;

            FieldData data = new FieldData(obj, info);
            fields.Add(data);
        }

        return fields.ToArray();
    }

    public abstract class BaseData
    {
        public System.Object instance { get; protected set; }
        public SerializedPropertyType inspectorType { get; protected set; }
        protected MemberInfo _memberInfo;

        public String name
        {
            get
            {
                return ObjectNames.NicifyVariableName(_memberInfo.Name);
            }
        }

        public abstract bool hasSetter
        {
            get;
        }

        public BaseData(System.Object instance, MemberInfo info)
        {

            this.instance = instance;
            _memberInfo = info;
        }

        public abstract System.Object GetValue();

        public abstract void SetValue(System.Object value);

    }

    public class PropertyData : BaseData
    {
        PropertyInfo _info { get { return _memberInfo as PropertyInfo; } }

        MethodInfo _getMethodInfo;
        MethodInfo _setMethodInfo;

        public override bool hasSetter
        {
            get
            {
                return _setMethodInfo != null;
            }
        }

        public PropertyData(System.Object instance, PropertyInfo info)
            : base(instance, info)
        {
            inspectorType = GetInspectorType(info.PropertyType);

            _getMethodInfo = _info.GetGetMethod();
            _setMethodInfo = _info.GetSetMethod();
        }

        public override System.Object GetValue()
        {
            return _getMethodInfo.Invoke(instance, null);
        }

        public override void SetValue(System.Object value)
        {
            if ((_setMethodInfo == null))
                return;
            _setMethodInfo.Invoke(instance, new System.Object[] { value });
        }

    }

    public class FieldData : BaseData
    {
        FieldInfo _info { get { return _memberInfo as FieldInfo; } }


        public override bool hasSetter
        {
            get
            {
                if (Application.isPlaying)
                    return true;
                return false;
            }
        }

        public FieldData(System.Object instance, FieldInfo info)
            : base(instance, info)
        {
            inspectorType = GetInspectorType(info.FieldType);
        }

        public override System.Object GetValue()
        {
            return _info.GetValue(instance);
        }

        public override void SetValue(System.Object value)
        {
            _info.SetValue(instance, value);
        }

    }

    private static SerializedPropertyType GetInspectorType(Type type)
    {
        if (type == typeof(int))
        {
            return SerializedPropertyType.Integer;
        }

        if (type == typeof(bool))
        {
            return SerializedPropertyType.Boolean;
        }

        if (type == typeof(float))
        {
            return SerializedPropertyType.Float;
        }

        if (type == typeof(string))
        {
            return SerializedPropertyType.String;
        }

        if (type == typeof(Color))
        {
            return SerializedPropertyType.Color;
        }

        if (type == typeof(LayerMask))
        {
            return SerializedPropertyType.LayerMask;
        }

        if (type.IsEnum)
        {
            return SerializedPropertyType.Enum;
        }

        if (type == typeof(Vector2))
        {
            return SerializedPropertyType.Vector2;
        }

        if (type == typeof(Vector3))
        {
            return SerializedPropertyType.Vector3;
        }

        if (type == typeof(Rect))
        {
            return SerializedPropertyType.Rect;
        }

        if (type == typeof(AnimationCurve))
        {
            return SerializedPropertyType.AnimationCurve;
        }

        if (type == typeof(Bounds))
        {
            return SerializedPropertyType.Bounds;
        }

        if (type == typeof(Gradient))
        {
            return SerializedPropertyType.Gradient;
        }

#if hasSerializedPropertyTypeQuaternion
        if (type == typeof(Quaternion))
        {
            return SerializedPropertyType.Quaternion;
        }
#endif

        if (type == typeof(Material))
        {
            return SerializedPropertyType.ObjectReference;
        }
        if (type == typeof(UnityEngine.Object))
        {
            return SerializedPropertyType.ObjectReference;
        }

        return SerializedPropertyType.Generic;

    }
}