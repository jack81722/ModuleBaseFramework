using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ModuleBased.Example
{
    [Serializable]
    [CreateAssetMenu(fileName = "ConfigFile", menuName = "Config")]
    public class ConfigScriptableObject : ScriptableObject
    {
        [SerializeField] private List<Record> m_records = new List<Record>();
        public List<Record> Records => m_records;


        private static string[] StringToSet(string value)
        {
            return value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string SetToString(string[] set)
        {
            return string.Join(";", set);
        }

        private static string AddToSetString(string setStr, string value)
        {
            var set = StringToSet(setStr);
            if (set.Contains(value))
                return setStr;

            set = set.Append(value).ToArray();
            return SetToString(set);
        }

        private static string RemoveFromSetString(string setStr, string value)
        {
            var set = StringToSet(setStr);
            set = set.Where(x => x != value).ToArray();
            return SetToString(set);
        }

        public bool GetRecord(string key, out Record record)
        {
            var list = Records.FindAll(x => x.Key.Equals(key));
            if (list == null || list.Count == 0)
            {
                record = null;
                return false;
            }

            record = list.First();
            return true;
        }

        public void SetRecord(string key, Record record)
        {
            if (GetRecord(key, out Record existingRecord))
            {
                if (existingRecord.Type == record.Type
                    && existingRecord.Value.Equals(record.Value))
                    return;

                existingRecord.Type = record.Type;
                existingRecord.Value = record.Value;
            }
            else
            {
                record.Key = key;
                Records.Add(record);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        public bool HasKey(string key)
        {
            return Records.Any(x => x.Key.Equals(key));
        }

        public void DeleteKey(string key)
        {
            Records.RemoveAll(x => x.Key.Equals(key));
        }

        public bool GetBool(string key, bool defaultValue)
        {
            if (!GetRecord(key, out Record record))
                return defaultValue;

            if (!bool.TryParse(record.Value, out bool result))
                return defaultValue;

            return result;
        }

        public void SetBool(string key, bool value)
        {
            SetRecord(key, new Record() { Type = Record.TypeEnum.Bool, Value = value.ToString() });
        }

        public int GetInt(string key, int defaultValue)
        {
            if (!GetRecord(key, out Record record))
                return defaultValue;

            if (!int.TryParse(record.Value, out int result))
                return defaultValue;

            return result;
        }

        public void SetInt(string key, int value)
        {
            SetRecord(key, new Record() { Type = Record.TypeEnum.Int, Value = value.ToString() });
        }

        public float GetFloat(string key, float defaultValue)
        {
            if (!GetRecord(key, out Record record))
                return defaultValue;

            if (!float.TryParse(record.Value, out float result))
                return defaultValue;

            return result;
        }

        public void SetFloat(string key, float value)
        {
            SetRecord(key, new Record() { Type = Record.TypeEnum.Float, Value = value.ToString() });
        }

        public string GetString(string key, string defaultValue)
        {
            if (!GetRecord(key, out Record record))
                return defaultValue;

            return record.Value;
        }

        public void SetString(string key, string value)
        {
            SetRecord(key, new Record() { Type = Record.TypeEnum.String, Value = value });
        }

        public string[] GetSet(string key, string[] defaultValue)
        {
            if (!GetRecord(key, out Record record))
                return defaultValue;

            return record.Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool SetContains(string key, string value)
        {
            var set = GetSet(key, null);
            if (set == null)
                return false;

            return set.Contains(value);
        }

        public void AddToSet(string key, string value)
        {
            if (!GetRecord(key, out Record record))
            {
                SetRecord(key, new Record() { Type = Record.TypeEnum.Folder, Value = value });
                return;
            }

            record.Value = AddToSetString(record.Value, value);
        }

        public void RemoveFromSet(string key, string value)
        {
            if (!GetRecord(key, out Record record))
                return;

            record.Value = RemoveFromSetString(record.Value, value);
        }

    }


}

[Serializable]
public class Record
{
    public enum TypeEnum
    {
        Bool,
        Int,
        Float,
        String,
        Folder,
        File
    }

    public string Key = "";
    public bool Lock = false;
    public TypeEnum Type = TypeEnum.Bool;
    public string Value = "";

    public static explicit operator bool(Record record)
    {
        if (bool.TryParse(record.Value, out bool result))
        {
            return result;
        }
        throw new InvalidCastException("Record is not bool type.");
    }

    public static explicit operator int(Record record)
    {
        if (int.TryParse(record.Value, out int result))
        {
            return result;
        }
        throw new InvalidCastException("Record is not int type.");
    }

    public static explicit operator float(Record record)
    {
        if (float.TryParse(record.Value, out float result))
        {
            return result;
        }
        throw new InvalidCastException("Record is not float type.");
    }

    public static explicit operator string(Record record)
    {
        return record.Value;
    }
}