// <copyright file="GeospatialController.cs" company="Google LLC">
//
// Copyright 2022 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARCoreRecordingPlaybackUtil.Sample.Scenes.Geospatial.Scripts
{
#if UNITY_ANDROID
    using UnityEngine.Android;
#endif

    public class GeospatialController : MonoBehaviour
    {
        public AREarthManager EarthManager;
        public ARCoreExtensions ARCoreExtensions;
        public Text InfoText;

        private bool _waitingForLocationService;
        private bool _isLocalizing;
        private bool _enablingGeospatial;
        private float _configurePrepareTime = 3f;
        private IEnumerator _startLocationService;

        public void OnEnable()
        {
            _startLocationService = StartLocationService();
            StartCoroutine(_startLocationService);

            _enablingGeospatial = false;

            _isLocalizing = true;
        }

        public void OnDisable()
        {
            StopCoroutine(_startLocationService);
            _startLocationService = null;
            Debug.Log("Stop location services.");
            Input.location.Stop();
        }

        private void Update()
        {

            if (ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
                return;

            // Check feature support and enable Geospatial API when it's supported.
            var featureSupport = EarthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);
            switch (featureSupport)
            {
                case FeatureSupported.Unknown:
                    return;
                case FeatureSupported.Unsupported:
                    return;
                case FeatureSupported.Supported:
                    if (ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode ==
                        GeospatialMode.Disabled)
                    {
                        Debug.Log("Geospatial sample switched to GeospatialMode.Enabled.");
                        ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode =
                            GeospatialMode.Enabled;
                        _configurePrepareTime = 3.0f;
                        _enablingGeospatial = true;
                        return;
                    }

                    break;
            }

            // Waiting for new configuration taking effect.
            if (_enablingGeospatial)
            {
                _configurePrepareTime -= Time.deltaTime;
                if (_configurePrepareTime < 0)
                    _enablingGeospatial = false;
                else
                    return;
            }

            // Check earth state.
            var earthState = EarthManager.EarthState;
            if (earthState == EarthState.ErrorEarthNotReady) return;

            if (earthState != EarthState.Enabled)
            {
                var errorMessage =
                    "Geospatial sample encountered an EarthState error: " + earthState;
                Debug.LogWarning(errorMessage);
                return;
            }

            // Check earth localization.
            var isSessionReady = ARSession.state == ARSessionState.SessionTracking &&
                                 Input.location.status == LocationServiceStatus.Running;
            var earthTrackingState = EarthManager.EarthTrackingState;
            var pose = earthTrackingState == TrackingState.Tracking ? EarthManager.CameraGeospatialPose : new GeospatialPose();
            if (!isSessionReady || earthTrackingState != TrackingState.Tracking)
            {
                // Lost localization during the session.
                if (!_isLocalizing)
                {
                    _isLocalizing = true;
                }
            }
            else if (_isLocalizing)
            {
                // Finished localization.
                _isLocalizing = false;
            }

            if (earthTrackingState == TrackingState.Tracking)
                InfoText.text = string.Format(
                    "Latitude/Longitude: {1}°, {2}°{0}" +
                    "Horizontal Accuracy: {3}m{0}" +
                    "Altitude: {4}m{0}" +
                    "Vertical Accuracy: {5}m{0}" +
                    "Eun Rotation: {6}{0}" +
                    "Orientation Yaw Accuracy: {7}°",
                    Environment.NewLine,
                    pose.Latitude.ToString("F6"),
                    pose.Longitude.ToString("F6"),
                    pose.HorizontalAccuracy.ToString("F6"),
                    pose.Altitude.ToString("F2"),
                    pose.VerticalAccuracy.ToString("F2"),
                    pose.EunRotation.ToString("F1"),
                    pose.OrientationYawAccuracy.ToString("F1"));
            else
                InfoText.text = "GEOSPATIAL POSE: not tracking";
        }


        private IEnumerator StartLocationService()
        {
            _waitingForLocationService = true;
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.Log("Requesting fine location permission.");
                Permission.RequestUserPermission(Permission.FineLocation);
                yield return new WaitForSeconds(3.0f);
            }
#endif

            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("Location service is disabled by User.");
                _waitingForLocationService = false;
                yield break;
            }

            Debug.Log("Start location service.");
            Input.location.Start();

            while (Input.location.status == LocationServiceStatus.Initializing) yield return null;

            _waitingForLocationService = false;
            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarningFormat(
                    "Location service ends with {0} status.", Input.location.status);
                Input.location.Stop();
            }
        }
    }
}
