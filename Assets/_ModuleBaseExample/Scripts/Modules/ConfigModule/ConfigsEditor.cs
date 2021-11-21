#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ModuleBased.Example
{
    [CustomEditor(typeof(ConfigScriptableObject))]
    public class ConfigsEditor : Editor
    {
        private static float _lockToggleWidth = 15f;

        private static readonly float SaveDelay = 2.0f;
        private float m_lastDirtyTime = -1.0f;

        private string _filter = string.Empty;
        private ReorderableList _list;

        private void MarkDirty()
        {
            EditorUtility.SetDirty(serializedObject.targetObject);
            m_lastDirtyTime = Time.realtimeSinceStartup;
        }

        private void Save()
        {
            AssetDatabase.SaveAssets();
            m_lastDirtyTime = -1.0f;
        }

        private void TrySave()
        {
            if (m_lastDirtyTime < 0.0f)
                return;

            if (Time.realtimeSinceStartup - m_lastDirtyTime < SaveDelay)
                return;

            Save();
        }

        ReorderableList CreateReorderableList()
        {
            var records = serializedObject.FindProperty("m_records");
            var list = new ReorderableList(serializedObject, records, true, true, true, true);
            list.drawElementCallback = (rect, index, active, focus) => DrawListItems(list, rect, index, active, focus); // Delegate to draw the elements on the list
            list.elementHeightCallback = (index) => ElementHeight(list, index);
            list.drawHeaderCallback = DrawHeader;
            list.onRemoveCallback = (list) =>
            {
                var listProp = list.serializedProperty;
                var element = listProp.GetArrayElementAtIndex(list.index);
                if (element.FindPropertyRelative("Lock").boolValue)
                    return;
                listProp.DeleteArrayElementAtIndex(list.index);
            };
            //list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
            //    var menu = new GenericMenu();
            //    menu.AddItem(new GUIContent("A"), false, clickHandler);

            //    menu.ShowAsContext();
            //};
            return list;
        }

        private void clickHandler()
        {

        }

        private void OnEnable()
        {
            EditorApplication.update += TrySave;
        }

        private void OnDisable()
        {
            EditorApplication.update -= TrySave;
            Save();
        }

        float ElementHeight(ReorderableList list, int index)
        {
            var keyProp = list.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("Key");
            if (!keyProp.stringValue.Contains(_filter))
            {
                return 0;
            }
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }

        void DrawListItems(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index); // The element in the list
            var lockProp = element.FindPropertyRelative("Lock");
            var keyProp = element.FindPropertyRelative("Key");
            if (!keyProp.stringValue.Contains(_filter))
                return;
            bool collide = false;
            for (int i = 0; i < index; i++)
            {
                if (list.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Key").stringValue == keyProp.stringValue)
                {
                    collide = true;
                    break;
                }
            }

            var origin = GUI.backgroundColor;
            if (collide)
            {
                GUI.backgroundColor = Color.red;
            }
            GUI.enabled = !lockProp.boolValue;
            keyProp.stringValue = EditorGUI.TextField(
                new Rect(rect.x, rect.y, rect.width - _lockToggleWidth - 10, EditorGUIUtility.singleLineHeight),
                keyProp.stringValue);
            if (collide)
            {
                GUI.backgroundColor = Color.red;
                GUI.backgroundColor = origin;
            }
            GUI.enabled = true;
            lockProp.boolValue = EditorGUI.Toggle(
                new Rect(rect.x + rect.width - _lockToggleWidth, rect.y, _lockToggleWidth, EditorGUIUtility.singleLineHeight),
                lockProp.boolValue);
            GUI.enabled = !lockProp.boolValue;

            float secondaryRowTypeWidth = 110;
            float secondaryRowValueWidth = rect.width - secondaryRowTypeWidth;
            float secondaryRowY = rect.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            var typeProp = element.FindPropertyRelative("Type");
            typeProp.enumValueIndex = Convert.ToInt32(EditorGUI.EnumPopup(
                new Rect(rect.x, secondaryRowY, 100, EditorGUIUtility.singleLineHeight),
                (Record.TypeEnum)typeProp.enumValueIndex));
            var valueProp = element.FindPropertyRelative("Value");
            switch ((Record.TypeEnum)typeProp.enumValueIndex)
            {
                case Record.TypeEnum.Bool:
                    bool boolValue;
                    if (!bool.TryParse(valueProp.stringValue, out boolValue))
                        boolValue = false;
                    valueProp.stringValue = EditorGUI.Toggle(
                        new Rect(rect.x + secondaryRowTypeWidth, secondaryRowY, secondaryRowValueWidth, EditorGUIUtility.singleLineHeight),
                        boolValue).ToString();
                    break;
                case Record.TypeEnum.Int:
                    int intValue;
                    if (!int.TryParse(valueProp.stringValue, out intValue))
                        intValue = 0;
                    valueProp.stringValue = EditorGUI.IntField(
                        new Rect(rect.x + secondaryRowTypeWidth, secondaryRowY, secondaryRowValueWidth, EditorGUIUtility.singleLineHeight),
                        intValue).ToString();
                    break;
                case Record.TypeEnum.Float:
                    float floatValue;
                    if (!float.TryParse(valueProp.stringValue, out floatValue))
                        floatValue = 0.0f;
                    valueProp.stringValue = EditorGUI.FloatField(
                        new Rect(rect.x + secondaryRowTypeWidth, secondaryRowY, secondaryRowValueWidth, EditorGUIUtility.singleLineHeight),
                        floatValue).ToString();
                    break;
                case Record.TypeEnum.Set:
                case Record.TypeEnum.String:
                    valueProp.stringValue = EditorGUI.TextField(
                        new Rect(rect.x + secondaryRowTypeWidth, secondaryRowY, secondaryRowValueWidth, EditorGUIUtility.singleLineHeight),
                        valueProp.stringValue);
                    break;
            }
            GUI.enabled = true;
        }

        //Draws the header
        void DrawHeader(Rect rect)
        {
            string name = "Record";
            EditorGUI.LabelField(rect, name);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.BeginVertical("groupbox");
            GUILayout.BeginHorizontal();
            var origin = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;
            var filter = EditorGUILayout.TextField("Filter", _filter);
            EditorGUIUtility.labelWidth = origin;
            if (GUILayout.Button("Lock All"))
            {
                var records = serializedObject.FindProperty("m_records");
                for (int i = 0; i < records.arraySize; i++)
                {
                    var element = records.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Lock").boolValue = true;
                }
            }
            if (GUILayout.Button("Unlock All"))
            {
                var records = serializedObject.FindProperty("m_records");
                for (int i = 0; i < records.arraySize; i++)
                {
                    var element = records.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Lock").boolValue = false;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            bool filterChanged = filter != _filter;
            if (_list == null || filterChanged)
            {
                _filter = filter;
                _list = CreateReorderableList();
            }
            _list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif