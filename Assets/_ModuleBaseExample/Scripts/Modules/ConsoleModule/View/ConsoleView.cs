using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModuleBased.Example
{
    [Injectable(typeof(ConsoleView))]
    public class ConsoleView : MonoBehaviour
    {
        #region -- Setting --
        public int MaxLine = 30;
        public int MaxHorizontalWord = 100;
        public int FontSize => _txtLog.fontSize;
        #endregion

        #region -- Require --
        [Inject]
        private ConsoleModule _consoleMod;
        #endregion

        #region -- UI --
        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private InputField _inputCmd;

        [SerializeField]
        private Text _txtLog;
        #endregion

        public void Display()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            enabled = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            enabled = false;
        }

        private void OnEnable()
        {
            _inputCmd.onEndEdit.AddListener(Commit);
            Refresh();
            focus();
        }

        private void OnDisable()
        {
            _inputCmd.onEndEdit.RemoveListener(Commit);
        }

        private void Refresh()
        {
            int lineCount = lineOfText(_txtLog.text);
            scaleLogField(lineCount);
        }

        public void TypeCommand(string line)
        {
            _inputCmd.text = line;
        }

        public void Commit(string line)
        {
            // skip empty input
            if (string.IsNullOrEmpty(line))
                return;
            // execute command and get result
            _consoleMod.ExecuteCommand(line);
            // re-focus input
            focus();
        }

        public void Log(object log, bool bold = false, bool italic = false)
        {
            if (log == null)
                return;
            // update log 
            log = insertNewLine(log.ToString());
            string beginBold = bold ? "<b>" : string.Empty;
            string endBold = bold ? "</b>" : string.Empty;
            string beginItalic = italic ? "<i>" : string.Empty;
            string endItalic = italic ? "</i>" : string.Empty;
            log = beginBold + beginItalic + insertNewLine(log.ToString()) + endItalic + endBold;
            _txtLog.text = log + "\n" + _txtLog.text;
            Refresh();
        }

        public void Log(object log, Color color, bool bold = false, bool italic = false)
        {
            if (log == null)
                return;
            // update log 
            string beginColor = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
            string endColor = "</color>";
            string beginBold = bold ? "<b>" : string.Empty;
            string endBold = bold ? "</b>" : string.Empty;
            string beginItalic = italic ? "<i>" : string.Empty;
            string endItalic = italic ? "</i>" : string.Empty;
            log = beginColor + beginBold + beginItalic + insertNewLine(log.ToString()) + endItalic + endBold + endColor;
            _txtLog.text = log + "\n" + _txtLog.text;
            Refresh();

        }

        private string insertNewLine(string result)
        {
            string[] splited = result.Split('\n');
            for (int i = 0; i < splited.Length; i++)
            {
                int count = splited[i].Length / MaxHorizontalWord;
                bool skip = splited[i].Length % MaxHorizontalWord == 0;
                for (int j = 0; j < count; j++)
                {
                    if (j == count - 1 && skip)
                        continue;
                    splited[i] = splited[i].Insert(MaxHorizontalWord * j, "\n");
                }
            }
            return string.Concat(splited);
        }

        private int lineOfText(string text)
        {
            return text.Split('\n').Length;
        }

        private void scaleLogField(int line)
        {
            var size = _txtLog.rectTransform.offsetMin;
            size.y = line * _txtLog.fontSize + (line - 1) * _txtLog.lineSpacing;
            size.y *= -1;
            _txtLog.rectTransform.offsetMin = size;
        }

        private void focus()
        {
            _inputCmd.text = string.Empty;
            _inputCmd.ActivateInputField();
            _inputCmd.Select();
        }
    }
}
