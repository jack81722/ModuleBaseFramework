using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ero.Daos.Csv
{
    /*
     * Refer to below references.
     * The first way is efficient, but cannot handle new line.
     * The second way is complex, but can handle new line.
     * Reference : https://pastebin.com/7XCA2UDD
     * Reference : https://assetstore.unity.com/packages/tools/integration/csv-serialize-135763
     */
    /// <summary>
    /// CSVReader is a tool to read the text or file and convert csv.
    /// </summary>
    public class CSVReader
    {
        /// <summary>
        /// Read all csv entities from file
        /// </summary>
        public static List<CSVEntity> ReadAll(string filePath, char separator = ',')
        {
            string data = File.ReadAllText(filePath);
            return ParseCSV(data, separator);
        }

        /// <summary>
        /// Read all entities and convert to specific type from file
        /// </summary>
        public static List<T> ReadAll<T>(string filePath, char separator = ',')
        {
            string data = File.ReadAllText(filePath);
            return ParseCSV<T>(data, separator);
        }

        public static List<T> ReadAll<T>(string filePath, ICSVConverter<T> converter, char separator = ',')
        {
            string data = File.ReadAllText(filePath);
            return ParseCSV<T>(data, converter, separator);
        }

        /// <summary>
        /// Parse the text string to csv entites
        /// </summary>
        public static List<CSVEntity> ParseCSV(string text, char separator = ',')
        {
            string[] headers = new string[0];
            List<CSVEntity> entities = new List<CSVEntity>();
            List<string> line = new List<string>();
            StringBuilder token = new StringBuilder();
            bool quotes = false;
            bool isHeader = false;

            for (int i = 0; i < text.Length; i++)
            {
                if (quotes == true)
                {
                    if ((text[i] == '\\' && i + 1 < text.Length && text[i + 1] == '\"') || (text[i] == '\"' && i + 1 < text.Length && text[i + 1] == '\"'))
                    {
                        token.Append('\"');
                        i++;
                    }
                    else if (text[i] == '\\' && i + 1 < text.Length && text[i + 1] == 'n')
                    {
                        token.Append('\n');
                        i++;
                    }
                    else if (text[i] == '\"')
                    {
                        line.Add(token.ToString());
                        token = new StringBuilder();
                        quotes = false;
                        if (i + 1 < text.Length && text[i + 1] == separator)
                            i++;
                    }
                    else
                    {
                        token.Append(text[i]);
                    }
                }
                else if (text[i] == '\r' || text[i] == '\n')
                {
                    if (token.Length > 0)
                    {
                        line.Add(token.ToString());
                        token = new StringBuilder();
                    }
                    if (!isHeader)
                    {
                        headers = line.ToArray();
                        line.Clear();
                        isHeader = true;
                    }
                    else
                    {
                        if (line.Count > 0)
                        {   
                            entities.Add(CreateEntity(line, headers));
                            line.Clear();
                        }
                    }
                }
                else if (text[i] == separator)
                {
                    line.Add(token.ToString());
                    token = new StringBuilder();
                }
                else if (text[i] == '\"')
                {
                    quotes = true;
                }
                else
                {
                    token.Append(text[i]);
                }
            }

            if (token.Length > 0)
            {
                line.Add(token.ToString());
            }
            if (line.Count > 0)
            {   
                entities.Add(CreateEntity(line, headers));
            }
            return entities;
        }

        private static CSVEntity CreateEntity(List<string> line, string[] headers)
        {
            CSVEntity entity = new CSVEntity();
            int count = Mathf.Min(line.Count, headers.Length);
            for (int e = 0; e < count; e++)
            {
                entity[headers[e]] = line[e];
            }
            return entity;
        }

        /// <summary>
        /// Parse the text string to the entites of specific type
        /// </summary>
        public static List<T> ParseCSV<T>(string text, char separator = ',')
        {
            string[] headers = new string[0];
            List<T> entities = new List<T>();
            List<string> line = new List<string>();
            StringBuilder token = new StringBuilder();
            bool quotes = false;
            bool isHeader = false;

            for (int i = 0; i < text.Length; i++)
            {
                if (quotes == true)
                {
                    if ((text[i] == '\\' && i + 1 < text.Length && text[i + 1] == '\"') || (text[i] == '\"' && i + 1 < text.Length && text[i + 1] == '\"'))
                    {
                        token.Append('\"');
                        i++;
                    }
                    else if (text[i] == '\\' && i + 1 < text.Length && text[i + 1] == 'n')
                    {
                        token.Append('\n');
                        i++;
                    }
                    else if (text[i] == '\"')
                    {
                        line.Add(token.ToString());
                        token = new StringBuilder();
                        quotes = false;
                        if (i + 1 < text.Length && text[i + 1] == separator)
                            i++;
                    }
                    else
                    {
                        token.Append(text[i]);
                    }
                }
                else if (text[i] == '\r' || text[i] == '\n')
                {
                    if (token.Length > 0)
                    {
                        line.Add(token.ToString());
                        token = new StringBuilder();
                    }
                    if (!isHeader)
                    {
                        headers = line.ToArray();
                        line.Clear();
                        isHeader = true;
                    }
                    else
                    {
                        if (line.Count > 0)
                        {
                            T entity = GenericCSVConvert<T>.Convert(headers, line.ToArray());
                            entities.Add(entity);
                            line.Clear();
                        }
                    }
                }
                else if (text[i] == separator)
                {
                    line.Add(token.ToString());
                    token = new StringBuilder();
                }
                else if (text[i] == '\"')
                {
                    quotes = true;
                }
                else
                {
                    token.Append(text[i]);
                }
            }

            if (token.Length > 0)
            {
                line.Add(token.ToString());
            }
            if (line.Count > 0)
            {
                var entity = GenericCSVConvert<T>.Convert(headers, line.ToArray());
                entities.Add(entity);
            }
            return entities;
        }

        public static List<T> ParseCSV<T>(string text, ICSVConverter<T> converter, char separator = ',')
        {
            string[] headers = new string[0];
            List<T> entities = new List<T>();
            List<string> line = new List<string>();
            StringBuilder token = new StringBuilder();
            bool quotes = false;
            bool isHeader = false;

            for (int i = 0; i < text.Length; i++)
            {
                if (quotes == true)
                {
                    if ((text[i] == '\\' && i + 1 < text.Length && text[i + 1] == '\"') || (text[i] == '\"' && i + 1 < text.Length && text[i + 1] == '\"'))
                    {
                        token.Append('\"');
                        i++;
                    }
                    else if (text[i] == '\\' && i + 1 < text.Length && text[i + 1] == 'n')
                    {
                        token.Append('\n');
                        i++;
                    }
                    else if (text[i] == '\"')
                    {
                        line.Add(token.ToString());
                        token = new StringBuilder();
                        quotes = false;
                        if (i + 1 < text.Length && text[i + 1] == separator)
                            i++;
                    }
                    else
                    {
                        token.Append(text[i]);
                    }
                }
                else if (text[i] == '\r' || text[i] == '\n')
                {
                    if (token.Length > 0)
                    {
                        line.Add(token.ToString());
                        token = new StringBuilder();
                    }
                    if (!isHeader)
                    {
                        headers = line.ToArray();
                        line.Clear();
                        isHeader = true;
                    }
                    else
                    {
                        if (line.Count > 0)
                        {
                            T entity = converter.Convert(headers, line.ToArray());
                            entities.Add(entity);
                            line.Clear();
                        }
                    }
                }
                else if (text[i] == separator)
                {
                    line.Add(token.ToString());
                    token = new StringBuilder();
                }
                else if (text[i] == '\"')
                {
                    quotes = true;
                }
                else
                {
                    token.Append(text[i]);
                }
            }

            if (token.Length > 0)
            {
                line.Add(token.ToString());
            }
            if (line.Count > 0)
            {
                T entity = converter.Convert(headers, line.ToArray());
                entities.Add(entity);
            }
            return entities;
        }

        #region -- Unused --
        //public static List<CSVEntity> ReadAll(string filePath)
        //{
        //    var list = new List<CSVEntity>();
        //    var data = File.ReadAllText(filePath);
        //    var lines = Regex.Split(data, LINE_SPLIT_RE);

        //    if (lines.Length <= 1)
        //        return list;

        //    var header = Regex.Split(lines[0], SPLIT_RE);
        //    for (var i = 1; i < lines.Length; i++)
        //    {
        //        var values = Regex.Split(lines[i], SPLIT_RE);
        //        if (values.Length == 0)
        //            continue;

        //        var entry = new CSVEntity();
        //        for (var j = 0; j < header.Length && j < values.Length; j++)
        //        {
        //            string key = header[j];
        //            string value = values[j];
        //            value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "").Replace("(n)", "\n");
        //            entry[key] = value;
        //        }
        //        list.Add(entry);
        //    }
        //    return list;
        //}
        #endregion
    }
}