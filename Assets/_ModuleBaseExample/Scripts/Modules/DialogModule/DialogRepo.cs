using Ero.Daos.Csv;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    [Injectable(typeof(DialogRepo))]
    public class DialogRepo : MonoBehaviour
    {
        [Inject]
        private IConfigModule _configMod;

        private string GetPath(string chapterName)
        {
            var directory = _configMod.LoadOrDefault<string>("file_dialog_path", "/");
            string path = directory + "/" + chapterName;
            return path;
        }

        public DialogModel GetByChapterName(string chapterName)
        {
            var chats = CSVReader.ReadAll<ChatModel>(GetPath(chapterName));
            DialogModel model = new DialogModel { Chats = chats.ToArray() };
            return model;
        }

    }

    public class DialogModel
    {   
        public ChatModel[] Chats;
    }

    public class ChatModel
    {
        public string Name;
        public string Content;
        public string Padding;
    }
}
