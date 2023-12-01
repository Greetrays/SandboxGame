using UnityEngine;
using UnityEngine.UI;

namespace ThirdPersonCamera.DemoSceneScripts
{
    public class UIHandling : MonoBehaviour
    {
        public Toggle optionCameraEnabled;
        public Toggle optionSmartPivot;
        public Toggle optionOcclusionCheck;
        public Toggle optionThicknessCheck;
        public Toggle optionControllerEnabled;
        public Toggle optionControllerInvertY;
        public Toggle optionMouseInvertY;

        public Dropdown optionCameraMode;

        public InputField inputDesiredDistance;
        public InputField inputMaxThickness;
        public InputField inputZoomOutStepValue;
        public InputField inputCollisionDistance;

        public Slider mouseSliderX;
        public Slider mouseSliderY;
        public Slider controllerSliderX;
        public Slider controllerSliderY;

        private const string ColliderName = "Collider";

        private CameraController cc;
        private FreeForm freeForm;
        private CameraInputSampling_FreeForm cameraInput;

        private bool ignoreChanges;

        void Awake()
        {
            ignoreChanges = true;

            cc = FindObjectOfType<CameraController>();
            cameraInput = FindObjectOfType<CameraInputSampling_FreeForm>();
            freeForm = FindObjectOfType<FreeForm>();

            if (cc != null)
            {
                optionSmartPivot.isOn = cc.smartPivot;
                optionOcclusionCheck.isOn = cc.occlusionCheck;

                inputDesiredDistance.text = cc.desiredDistance.ToString();
                inputMaxThickness.text = cc.maxThickness.ToString();
                inputZoomOutStepValue.text = cc.zoomOutStepValue.ToString();
                inputCollisionDistance.text = cc.collisionDistance.ToString();

                optionThicknessCheck.isOn = cc.thicknessCheck;
            }

            if (cameraInput != null)
            {
                optionCameraEnabled.isOn = freeForm.cameraEnabled;
                optionControllerEnabled.isOn = cameraInput.controllerEnabled;
                optionControllerInvertY.isOn = cameraInput.controllerInvertY;
                optionMouseInvertY.isOn = cameraInput.mouseInvertY;

                if (cameraInput.cameraMode == CameraMode.Input)
                    optionCameraMode.value = 0;
                else
                    optionCameraMode.value = 1;

                mouseSliderX.value = cameraInput.mouseSensitivity.x;
                mouseSliderY.value = cameraInput.mouseSensitivity.y;

                controllerSliderX.value = cameraInput.controllerSensitivity.x;
                controllerSliderY.value = cameraInput.controllerSensitivity.y;
            }

            ignoreChanges = false;
        }

        public void HandleUI()
        {
            if (ignoreChanges)
                return;

            cc.smartPivot = optionSmartPivot.isOn;

            if (!cc.smartPivot)
            {
                cc.cameraNormalMode = true;
            }

            cc.occlusionCheck = optionOcclusionCheck.isOn;
            cc.thicknessCheck = optionThicknessCheck.isOn;

            float newDistance = 0.0f;
            if (float.TryParse(inputDesiredDistance.text, out newDistance))
                cc.desiredDistance = newDistance;

            float newThickness = 0.0f;
            if (float.TryParse(inputMaxThickness.text, out newThickness))
                cc.maxThickness = newThickness;

            float newStep = 0.0f;
            if (float.TryParse(inputZoomOutStepValue.text, out newStep))
                cc.zoomOutStepValue = newStep;

            float newCd = 0.0f;
            if (float.TryParse(inputCollisionDistance.text, out newCd))
                cc.collisionDistance = newCd;

            if (cameraInput != null)
            {
                if (optionCameraMode.value == 0)
                    cameraInput.cameraMode = CameraMode.Input;
                else
                    cameraInput.cameraMode = CameraMode.Always;

                cameraInput.controllerEnabled = optionControllerEnabled.isOn;
                cameraInput.controllerInvertY = optionControllerInvertY.isOn;
                cameraInput.mouseInvertY = optionMouseInvertY.isOn;

                freeForm.cameraEnabled = optionCameraEnabled.isOn;
                cameraInput.mouseSensitivity.x = mouseSliderX.value;
                cameraInput.mouseSensitivity.y = mouseSliderY.value;

                cameraInput.controllerSensitivity.x = controllerSliderX.value;
                cameraInput.controllerSensitivity.y = controllerSliderY.value;
            }
        }

        public void Update()
        {
            if (cameraInput != null)
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(Input.mousePosition.x, Input.mousePosition.y), Vector2.zero);

                if (hit.collider != null && hit.collider.name == ColliderName)
                {
                    freeForm.cameraEnabled = false;
                    optionCameraEnabled.isOn = false;
                }
                else
                {
                    freeForm.cameraEnabled = true;
                    optionCameraEnabled.isOn = true;
                }

                if (Input.GetKey(KeyCode.Escape))
                {
                    if (cameraInput.cameraMode == CameraMode.Always)
                    {
                        cameraInput.cameraMode = CameraMode.Input;
                        optionCameraMode.value = 0;
                    }

                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }

            ignoreChanges = true;
            inputDesiredDistance.text = cc.desiredDistance.ToString();
            ignoreChanges = false;
        }
    }
}