using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARCoreRecordingPlaybackUtil.Scripts
{
    [RequireComponent(typeof(Button))]
    public class RecordCell : MonoBehaviour
    {
        public Button CellButton => _cellButton;
        [SerializeField] private TextMeshProUGUI _tmpro;
        [SerializeField] private Button _cellButton;
        private string _path;

        public void Init(string path)
        {
            _path = path;
            var fileName = Path.GetFileName(path);
            _tmpro.text = fileName;
            name = fileName;
        }
    }
}