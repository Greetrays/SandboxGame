﻿//----------------------------------------------
//            3rd Person Camera
// Copyright © 2015-2020 Thomas Enzenebner
//            Version 1.0.7.2
//         t.enzenebner@gmail.com
//----------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThirdPersonCamera
{
    public struct CameraInputFreeForm
    {
        public bool inputFreeLook;        
        public float mouseX;
        public float mouseY;

        public float mouseWheel;
        public bool middleMouseButtonPressed;       
    }

    [RequireComponent(typeof(FreeForm))]
    public class CameraInputSampling_FreeForm : MonoBehaviour
    {
        [Header("Input settings")]
        [Tooltip("Always and hold - Either the camera rotation is always on or you have to use an input to enable")]
        public CameraMode cameraMode = CameraMode.Input;

        [Header("Mouse input settings")]
        [Tooltip("A list of integer mouseButton values to enable freelook, default is left/right mouse button")]
        public List<int> mouseInput = new List<int>() { 0, 1 }; // default input is left and right mouse button  
        [Tooltip("Adjusts the sensitivity of looking around with the mouse")]
        public Vector2 mouseSensitivity = new Vector2(1.5f, 1.0f);
        [Tooltip("Inverts the Y-Axis")]
        public bool mouseInvertY = false;

        [Header("Keyboard input settings")]
        [Tooltip("A list of KeyCodes to enable freelook")]
        public List<KeyCode> keyboardInput = new List<KeyCode>();

        [Header("Controller input settings")]
        [Tooltip("Enables controller support")]
        public bool controllerEnabled = false;
        [Tooltip("A list of button strings to enable freelook")]
        public List<string> buttonInput = new List<string>() { };
        [Tooltip("Inverts the Y-axis")]
        public bool controllerInvertY = true;
        [Tooltip("Adjusts the sensitivity of looking around with the controller")]
        public Vector2 controllerSensitivity = new Vector2(1.0f, 0.7f);
        
        [Header("Lock mouse settings")]
        [Tooltip("When looking around, the mouse cursor will be hidden and locked")]
        public bool lockAndHideMouseCursor = false;
        [Tooltip("A list of KeyCodes to cancel locked mouse cursor")]
        public List<KeyCode> keyboardInputToggle = new List<KeyCode>() { };
        [Tooltip("A list of KeyCodes to cancel locked mouse cursor")]
        public List<KeyCode> keyboardInputCancel = new List<KeyCode>() { KeyCode.Escape };

        [Header("Extra settings")]
        [Tooltip("Enable this feature to automatically handle FreeForm.forceCharacterDirection with given inputs")]
        public bool forceDirectionFeature = false;
        [Tooltip("A list of integer mouseButton values to enable forcing character/target rotation")]
        public List<int> mouseInputForceDirection = new List<int>() { 1 }; // default input to change character rotation is right mouse button

        [HideInInspector]
        public bool inputSamplingEnabled = true;

        private const string CMouseX = "Mouse X";
        private const string CMouseY = "Mouse Y";
        private const string CMouseScrollWheel = "Mouse ScrollWheel";

        private string rightAxisXName;
        private string rightAxisYName;

        private FreeForm freeForm;
        private CameraInputFreeForm inputFreeForm;
        bool waitForRelease = false;
        int mouseButtonId = -1;
        
#if UNITY_STANDALONE_WIN
        private MouseCursorHelper mouseCursorHelper;
#endif

        public void Start()
        {
            freeForm = GetComponent<FreeForm>();
            inputFreeForm = new CameraInputFreeForm();

#if UNITY_STANDALONE_WIN
            mouseCursorHelper = new MouseCursorHelper();
#endif

            string platform = Application.platform.ToString().ToLower();

            if (platform.Contains("windows") || platform.Contains("linux"))
            {
                rightAxisXName = "Right_4";
                rightAxisYName = "Right_5";
            }
            else
            {
                rightAxisXName = "Right_3";
                rightAxisYName = "Right_4";
            }

            // test if the controller axis are setup
            try
            {
                Input.GetAxis(rightAxisXName);
                Input.GetAxis(rightAxisYName);
            }
            catch
            {
                Debug.LogWarning("Controller Error - Right axis not set in InputManager. Controller is disabled!");
                controllerEnabled = false;
            }
        }

        public void Update()
        {
            bool interfaceHovered = false;

            if (EventSystem.current != null)
            {
                interfaceHovered = EventSystem.current.IsPointerOverGameObject();

                if (interfaceHovered && !waitForRelease)
                {
                    for (int i = 0; i < mouseInput.Count && !waitForRelease; i++)
                    {
                        if (Input.GetMouseButtonDown(mouseInput[i]))
                        {
                            waitForRelease = true;
                            mouseButtonId = i;
                        }
                    }
                }

                if (waitForRelease)
                {
                    if (mouseButtonId != -1)
                    {
                        if (Input.GetMouseButtonUp(mouseInput[mouseButtonId]))
                            waitForRelease = false;
                    }
                    else
                        waitForRelease = false;
                }
            }

            if (cameraMode == CameraMode.Input && (!inputSamplingEnabled || interfaceHovered || waitForRelease))
            {
                freeForm.UpdateInput(new CameraInputFreeForm());
                return;
            }

            bool inputFreeLook = false;

            // sample mouse inputs
            if (!inputFreeLook && mouseInput.Count > 0)
            {
                for (int i = 0; i < mouseInput.Count && !inputFreeLook; i++)
                {
                    if (Input.GetMouseButton(mouseInput[i]))
                        inputFreeLook = true;
                }
            }

            // sample keyboard inputs
            if (!inputFreeLook && keyboardInput.Count > 0)
            {
                for (int i = 0; i < keyboardInput.Count && !inputFreeLook; i++)
                {
                    if (Input.GetKey(keyboardInput[i]))
                        inputFreeLook = true;
                }
            }

            if (!inputFreeLook && buttonInput.Count > 0)
            {
                for (int i = 0; i < buttonInput.Count && !inputFreeLook; i++)
                {
                    if (Input.GetButton(buttonInput[i]))
                        inputFreeLook = true;
                }
            }

            float x = 0;
            float y = 0;

            if (inputFreeLook || cameraMode == CameraMode.Always)
            {
                // sample mouse input 
                x = Input.GetAxis(CMouseX) * mouseSensitivity.x;
                y = (mouseInvertY ? (Input.GetAxis(CMouseY) * -1.0f) : Input.GetAxis(CMouseY)) * mouseSensitivity.y;
            }

            if (controllerEnabled)
            {
                // sample controller input
                x += Input.GetAxis(rightAxisXName) * controllerSensitivity.x;
                y += (controllerInvertY ? (Input.GetAxis(rightAxisYName) * -1.0f) : Input.GetAxis(rightAxisYName)) * controllerSensitivity.y;
            }

#if UNITY_STANDALONE_WIN            
            // cancel locked state
            if (keyboardInputToggle.Count > 0)
            {
                for (int i = 0; i < keyboardInputToggle.Count; i++)
                {
                    if (Input.GetKey(keyboardInputToggle[i]))
                        mouseCursorHelper.Toggle();
                }
            }

            if (keyboardInputCancel.Count > 0)
            {
                for (int i = 0; i < keyboardInputCancel.Count; i++)
                {
                    if (Input.GetKey(keyboardInputCancel[i]))
                        mouseCursorHelper.Show();
                }
            }

            if (lockAndHideMouseCursor)
            {
                if (inputFreeLook || cameraMode == CameraMode.Always)
                {
                    mouseCursorHelper.Hide();
                }
                else
                    mouseCursorHelper.Show();
            }

            mouseCursorHelper.Update();
#else
           /* if (lockMouseCursor)
            {
                if (inputFreeLook || cameraMode == CameraMode.Always)
                {
                    HideCursor();
                }
                else
                {
                    ShowCursor();
                }
            }*/
#endif            
            // sample inputs for changing character direction
            if (forceDirectionFeature && mouseInputForceDirection.Count > 0)
            {
                for (int i = 0; i < mouseInputForceDirection.Count; i++)
                {
                    if (Input.GetMouseButton(mouseInputForceDirection[i]))
                        freeForm.forceCharacterDirection = true;
                    else
                        freeForm.forceCharacterDirection = false;
                }
            }
            
            // update values
            inputFreeForm.inputFreeLook = inputFreeLook || cameraMode == CameraMode.Always;
            inputFreeForm.mouseX = x;
            inputFreeForm.mouseY = y;
            inputFreeForm.mouseWheel = Input.GetAxis(CMouseScrollWheel);
            inputFreeForm.middleMouseButtonPressed = Input.GetMouseButton(2);

            // update FreeForm with new values
            freeForm.UpdateInput(inputFreeForm);            
        }

        public static void HideCursor()
        {            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;            
        }

        public static void ShowCursor()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
