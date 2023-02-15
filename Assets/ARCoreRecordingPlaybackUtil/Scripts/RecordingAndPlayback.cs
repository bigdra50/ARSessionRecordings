using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

namespace ARCoreRecordingPlaybackUtil.Scripts
{
    public class RecordingAndPlayback : MonoBehaviour
    {
        [SerializeField]
        private Button _playButton;

        [SerializeField]
        private Button _stopButton;

        [SerializeField]
        private Button _recordButton;

        [SerializeField]
        private GameObject _panel;

        [SerializeField]
        private RecordCell _recordCellPrefab;

        [SerializeField]
        private Transform _cellsRoot;

        [SerializeField]
        private Button _decideEditButton;

        [SerializeField]
        private Button _cancelEditButton;


        private ARSession _arSession;
        private List<RecordCell> _recordCells = new();

        private void Start()
        {
            _arSession = FindObjectOfType<ARSession>();
            _playButton.onClick.AddListener(() =>
            {
                _panel.SetActive(!_panel.activeSelf);
                Reload();
            });
            _stopButton.onClick.AddListener(Stop);
            _recordButton.onClick.AddListener(StartRecording);
            Reload();
        }

        private void Reload()
        {
            foreach (var cell in _recordCells)
            {
                Destroy(cell.gameObject);
            }

            _recordCells.Clear();

            var filePaths = Directory.GetFiles(Application.persistentDataPath, "*.mp4", SearchOption.TopDirectoryOnly);
            foreach (var filePath in filePaths)
            {
                var cell = Instantiate(_recordCellPrefab, _cellsRoot);
                cell.Init(filePath, _decideEditButton, _cancelEditButton);
                cell.CellButton.onClick.AddListener(() => StartPlayback(filePath));
                cell.GetComponent<LongPressEventTrigger>().OnLongPressed.AddListener(() =>
                {
                    // Edit Modeのセルはキャンセルさせる
                    _recordCells.ForEach(x => x.CancelFileEdit());
                    cell.EditFileName();
                });
                _recordCells.Add(cell);
            }
        }

        private void StartRecording()
        {
            if (_arSession.subsystem is not ARCoreSessionSubsystem subsystem) return;

            using var config = new ArRecordingConfig(subsystem.session);
            var path = Path.Combine(Application.persistentDataPath, $"arcore-session-{DateTime.Now:yyyyMMddHHmmss}.mp4");
            config.SetMp4DatasetFilePath(subsystem.session, path);
            var screenRotation = Screen.orientation switch
            {
                ScreenOrientation.Portrait => 0,
                ScreenOrientation.LandscapeLeft => 90,
                ScreenOrientation.PortraitUpsideDown => 180,
                ScreenOrientation.LandscapeRight => 270,
                _ => 0
            };
            config.SetRecordingRotation(subsystem.session, screenRotation);

            subsystem.StartRecording(config);
            _panel.SetActive(false);
            _stopButton.gameObject.SetActive(true);
            _recordButton.gameObject.SetActive(false);
        }

        private void Stop()
        {
            if (_arSession.subsystem is not ARCoreSessionSubsystem subsystem) return;
            if (subsystem.recordingStatus.Recording())
            {
                subsystem.StopRecording();
            }
            else
            {
                subsystem.StopPlayback();
            }


            _stopButton.gameObject.SetActive(false);
            _recordButton.gameObject.SetActive(true);
        }

        private void StartPlayback(string path)
        {
            if (_arSession.subsystem is not ARCoreSessionSubsystem subsystem) return;
            if (subsystem.playbackStatus.Playing())
            {
                subsystem.StopPlayback();
                return;
            }

            if (subsystem.playbackStatus == ArPlaybackStatus.Finished)
            {
                subsystem.StopPlayback();
            }

            if (!File.Exists(path))
            {
                return;
            }

            Debug.Log($"Play {path}");
            subsystem.StartPlayback(path);
            _panel.SetActive(false);
            _stopButton.gameObject.SetActive(true);
            _recordButton.gameObject.SetActive(false);
        }
    }
}
