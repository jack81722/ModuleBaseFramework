using ModuleBased.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UnityEngine;

namespace ModuleBased.Example.CommandLine
{
    public class CommandOptions : IEnumerable<KeyValuePair<string, object>>
    {
        #region -- Data table --
        public static readonly string ShortNameColumn = "short_name";
        public static readonly string FullNameColumn = "full_name";
        public static readonly string ValueColumn = "value";

        private static DataTable _optionTable;
        public static DataTable OptionTable {
            get
            {
                if (_optionTable == null)
                {
                    _optionTable = new DataTable();
                    _optionTable.Columns.Add(ShortNameColumn, typeof(string));
                    _optionTable.Columns[ShortNameColumn].MaxLength = 1;
                    _optionTable.Columns[ShortNameColumn].AllowDBNull = true;
                    _optionTable.Columns[ShortNameColumn].Unique = true;

                    _optionTable.Columns.Add(FullNameColumn, typeof(string));
                    _optionTable.Columns[FullNameColumn].MaxLength = 20;
                    _optionTable.Columns[FullNameColumn].AllowDBNull = true;
                    _optionTable.Columns[FullNameColumn].Unique = true;

                    _optionTable.Columns.Add(ValueColumn, typeof(object));
                    _optionTable.Columns[ValueColumn].AllowDBNull = true;
                }
                return _optionTable;
            }
        }

        private DataTable _dt;
        #endregion

        public string Name { get; set; }
        public string Verb { get; set; }
        public object Value { get; set; }

        public CommandOptions()
        {
            _dt = OptionTable.Clone();
        }

        public void AddOption(string shortName, string fullName, object value)
        {
            var row = _dt.NewRow();
            row[ShortNameColumn] = shortName;
            row[FullNameColumn] = fullName;
            row[ValueColumn] = value;
            _dt.Rows.Add(row);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            builder.Append($"Name:{Name}");
            if (!string.IsNullOrEmpty(Verb))
                builder.Append($", Verb:{Verb}");
            var array = Value as IEnumerable;
            if (array == null)
                builder.Append($", Value:{Value}");
            else
                builder.Append($", Value:{array.ToArrayString()}");
            foreach (var pair in this)
            {
                builder.Append($", {pair.Key}:{pair.Value}");
            }
            builder.Append(")");
            return builder.ToString();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (DataRow row in _dt.Rows)
            {
                var key = (string)(row[FullNameColumn] != null ? row[FullNameColumn] : row[ShortNameColumn]);
                if (string.IsNullOrEmpty(key))
                    continue;
                var value = row[ValueColumn];
                yield return new KeyValuePair<string, object>(key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
