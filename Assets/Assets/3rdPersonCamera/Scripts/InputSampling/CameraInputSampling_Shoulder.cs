//----------------------------------------------
//            3rd Person Camera
// Copyright © 2015-2020 Thomas Enzenebner
//            Version 1.0.7.2
//         t.enzenebner@gmail.com
//----------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonCamera
{
    public struct CameraInputShoulder
    {
        public bool aiming;
        public bool left;
    }

    [RequireComponent(typeof(OverTheShoulder))]
    public class CameraInputSampling_Shoulder : MonoBehaviour
    {
        [Header("Input settings")]
        [Tooltip("A list of Mouse button values to activate aiming - Default is right mouse button")]
        public List<int> inputMouseAimMode = new List<int>() { 1 }; // default is right mouse button
        [Tooltip("A list of KeyCode values to activate aiming")]
        public List<KeyCode> inputKeyboardAimMode = new List<KeyCode>();
        [Tooltip("A list of button strings to activate aiming")]
        public List<string> inputGamepadAimMode = new List<string>();

        [Tooltip("A list of Mouse button values to toggle the side you are leaning to")]
        public List<int> inputMouseChangeLeaning = new List<int>() { };
        [Tooltip("A list of KeyCode values to toggle the side you are leaning to")]
        public List<KeyCode> inputKeyboardChangeLeaning = new List<KeyCode>() { KeyCode.LeftShift };
        [Tooltip("A list of button strings to toggle the side you are leaning to")]
        public List<string> inputGamepadChangeLeaning = new List<string>() { };

        [HideInInspector]
        public bool inputSamplingEnabled = true;

        private CameraInputShoulder cameraInputShoulder;
        private OverTheShoulder shoulder;

        public void Start()
        {
            cameraInputShoulder = new CameraInputShoulder();
            shoulder = GetComponent<OverTheShoulder>();
        }

        public void Update()
        {
            if (!inputSamplingEnabled)
            {
                return;
            }

            bool aiming = false;
            bool left = cameraInputShoulder.left;

            if (inputMouseAimMode.Count > 0) // right mouse click toggles lock on mode
            {
                for (int i = 0; i < inputMouseAimMode.Count; i++)
                {
                    if (Input.GetMouseButton(inputMouseAimMode[i]))
                        aiming = true;
                }
            }

            if (inputKeyboardAimMode.Count > 0) // right mouse click toggles lock on mode
            {
                for (int i = 0; i < inputKeyboardAimMode.Count; i++)
                {
                    if (Input.GetKeyDown(inputKeyboardAimMode[i]))
                        aiming = true;
                }
            }

            if (inputGamepadAimMode.Count > 0) // right mouse click toggles lock on mode
            {
                for (int i = 0; i < inputGamepadAimMode.Count; i++)
                {
                    if (Input.GetButton(inputGamepadAimMode[i]))
                        aiming = true;
                }
            }

            if (inputMouseChangeLeaning.Count > 0) // right mouse click toggles lock on mode
            {
                for (int i = 0; i < inputMouseChangeLeaning.Count; i++)
                {
                    if (Input.GetMouseButton(inputMouseChangeLeaning[i]))
                        left = !left;
                }
            }

            if (inputKeyboardChangeLeaning.Count > 0) // right mouse click toggles lock on mode
            {
                for (int i = 0; i < inputKeyboardChangeLeaning.Count; i++)
                {
                    if (Input.GetKeyDown(inputKeyboardChangeLeaning[i]))
                        left = !left;
                }
            }

            if (inputGamepadChangeLeaning.Count > 0) // right mouse click toggles lock on mode
            {
                for (int i = 0; i < inputGamepadChangeLeaning.Count; i++)
                {
                    if (Input.GetButton(inputGamepadChangeLeaning[i]))
                        left = !left;
                }
            }

            // update input values
            cameraInputShoulder.aiming = aiming;
            cameraInputShoulder.left = left;

            shoulder.UpdateInput(cameraInputShoulder);
        }
    }
}
