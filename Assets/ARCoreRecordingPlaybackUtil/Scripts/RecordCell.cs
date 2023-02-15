using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARCoreRecordingPlaybackUtil.Scripts
{
    [RequireComponent(typeof(Button), typeof(LongPressEventTrigger))]
    public class RecordCell : MonoBehaviour
    {
        public Button CellButton => _cellButton;

        [SerializeField]
        private TextMeshProUGUI _tmpro;

        [SerializeField]
        private TMP_InputField _inputField;

        private Button _cellButton;
        private LongPressEventTrigger _longPressEventTrigger;
        private Button _decideButton;
        private Button _cancelButton;

        private string _path;
        private CellMode _currentMode = CellMode.Normal;
        private List<GameObject> _normalModeOnlyObjects;
        private List<GameObject> _editModeOnlyObjects;


        public void Init(string path, Button decideButton, Button cancelButton)
        {
            _path = path;
            var fileName = Path.GetFileName(path);
            _tmpro.text = fileName;
            name = fileName;
            _currentMode = CellMode.Normal;
            _cellButton = GetComponent<Button>();
            _longPressEventTrigger = GetComponent<LongPressEventTrigger>();
            _decideButton = decideButton;
            _cancelButton = cancelButton;
            _normalModeOnlyObjects = new List<GameObject>
            {
                _tmpro.gameObject,
            };
            _editModeOnlyObjects = new List<GameObject>
            {
                _inputField.gameObject,
                _decideButton.gameObject,
                _cancelButton.gameObject,
            };

            ToNormalMode();
        }


        private void ToEditMode()
        {
            if (_currentMode == CellMode.Edit) return;
            _normalModeOnlyObjects.ForEach(x => x.SetActive(false));
            _editModeOnlyObjects.ForEach(x => x.SetActive(true));
            _inputField.text = _tmpro.text;
            _decideButton.onClick.AddListener(DecideFileName);
            _cancelButton.onClick.AddListener(CancelFileEdit);
            _currentMode = CellMode.Edit;
        }

        private void ToNormalMode()
        {
            if (_currentMode == CellMode.Normal) return;
            _decideButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
            _normalModeOnlyObjects.ForEach(x => x.SetActive(true));
            _editModeOnlyObjects.ForEach(x => x.SetActive(false));
            _currentMode = CellMode.Normal;
        }

        private void DecideFileName()
        {
            if (!ValidateRecordFileName())
            {
                CancelFileEdit();
            }

            RenameRecordFile();
            ToNormalMode();
        }

        public void EditFileName()
        {
            ToEditMode();
        }
        public void CancelFileEdit()
        {
            // TODO: restore file name
            ToNormalMode();
        }

        // TODO:validate file name
        private bool ValidateRecordFileName()
        {
            if (string.IsNullOrEmpty(_inputField.text)) return false;
            // extension is .mp4
            return true;
        }

        private void RenameRecordFile()
        {
            Debug.Log($"{_inputField.text} to filename of {name}");
            var dir = Path.GetDirectoryName(_path);
            var newFileName = _inputField.text;
            var newFilePath = Path.Combine(dir, newFileName);
            File.Move(_path, newFilePath);
            _tmpro.text = newFileName;
            name = newFileName;
        }
    }

    public enum CellMode
    {
        Normal,
        Edit,
    }
}
