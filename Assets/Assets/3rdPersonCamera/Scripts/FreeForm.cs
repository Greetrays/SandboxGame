//----------------------------------------------
//            3rd Person Camera
// Copyright © 2015-2020 Thomas Enzenebner
//            Version 1.0.7.2
//         t.enzenebner@gmail.com
//----------------------------------------------
using UnityEngine;

namespace ThirdPersonCamera
{
    public enum CameraMode
    {
        Always,
        Input
    }

    public enum StationaryModeType
    {
        Free,
        Fixed,
        Limited,
        RotateWhenLimited
    }

    [RequireComponent(typeof(CameraController))]
    public class FreeForm : MonoBehaviour
    {
        [Header("Basic settings")]
        [Tooltip("Enables/Disables the camera rotation updates. Useful when you want to lock the camera in place or to turn off the camera, for example, when hovering an interface element with the mouse")]
        public bool cameraEnabled = true;        
        [Tooltip("Sets a min distance for zooming in")]
        public float minDistance = 1;
        [Tooltip("Sets a max distance for zooming out")]
        public float maxDistance = 5;
        [Tooltip("The minimum angle you can look down")]
        public float yAngleLimitMin = 0.0f;
        [Tooltip("The maximum angle you can look up")]
        public float yAngleLimitMax = 180.0f;

        [Header("Stationary settings")]
        [Tooltip("Sets a stationary mode for the horizontal axis")]
        public StationaryModeType stationaryModeHorizontal = StationaryModeType.Free;
        [Tooltip("Sets a stationary mode for the vertical axis")]
        public StationaryModeType stationaryModeVertical = StationaryModeType.Free;
        [Tooltip("Maximum angle for the horizontal axis")]
        public float stationaryMaxAngleHorizontal = 30.0f;
        [Tooltip("Maximum angle for the vertical axis")]
        public float stationaryMaxAngleVertical = 30.0f;

        [Header("Extra settings")]
        [Tooltip("Enables smoothing for the camera rotation when looking around - Beware, introduces interpolation lag")]
        public bool smoothing = false;
        [Tooltip("The speed at which the camera will lerp to the final rotation")]
        public float smoothSpeed = 3.0f;
        [Tooltip("Sets the targets y-axis to have the same y rotation as the camera.")]
        public bool forceCharacterDirection = false;
        [Tooltip("In case the rotation starts to drift in the z-axis, enable this to stabilize the rotation.")]
        public bool stabilizeRotation = false;
        [Tooltip("Inform the script of using a custom input script to set the CameraInputFreeForm model")]
        public bool customInput = false;
        [Tooltip("Enables looking backward with pressing middle mouse button")]
        public bool lookingBackwardsEnabled = false;

        [HideInInspector]
        public float x;
        [HideInInspector]
        public float y;
        private float yAngle;
        private float angle;

        private Vector3 upVector;
        private Vector3 downVector;

        private bool smartPivotInit;

        private float smoothHorizontal;
        private float smoothVertical;

        private CameraController cameraController;

        private bool lookingBackwards = false;
        private CameraInputFreeForm inputFreeForm;

        private GameObject dummy;

        public void Start()
        {
            dummy = new GameObject("TPC_Dummy");
            cameraController = GetComponent<CameraController>();

            if (!customInput)
            {
                var lookup = GetComponent<CameraInputSampling_FreeForm>();
                if (lookup == null)
                    Debug.LogError("CameraInputSampling_FreeForm not found on " + transform.name + ". Consider adding it to get input sampling or enable customInput to skip this message");
            }

            x = 0;
            y = 0;

            smartPivotInit = true;

            upVector = Vector3.up;
            downVector = Vector3.down;
        }

        public CameraInputFreeForm GetInput()
        {
            return inputFreeForm;
        }

        public void UpdateInput(CameraInputFreeForm newInput)
        {
            inputFreeForm = newInput;
        }

        public void LateUpdate()
        {
            if (!cameraEnabled)
                return;

            if (cameraController == null || cameraController.target == null)
                return;

            Vector3 cameraAngles = transform.rotation.eulerAngles;
            Vector3 targetAngles = cameraController.target.rotation.eulerAngles;

            if (inputFreeForm.inputFreeLook)
            {
                x = inputFreeForm.mouseX;
                y = inputFreeForm.mouseY;
            }
            else
            {
                x = 0;
                y = 0;
            }

            // sample mouse scrollwheel for zooming in/out
            if (inputFreeForm.mouseWheel < 0) // back
            {
                cameraController.desiredDistance += cameraController.zoomOutStepValue;

                if (cameraController.desiredDistance > maxDistance)
                    cameraController.desiredDistance = maxDistance;
            }
            if (inputFreeForm.mouseWheel > 0) // forward
            {
                cameraController.desiredDistance -= cameraController.zoomOutStepValue;

                if (cameraController.desiredDistance < minDistance)
                    cameraController.desiredDistance = minDistance;
            }

            if (lookingBackwardsEnabled)
            {
                if (inputFreeForm.middleMouseButtonPressed && !lookingBackwards) // flip y-axis when pressing middle mouse button
                {
                    lookingBackwards = true;
                    cameraController.cameraRotation = Quaternion.Euler(cameraAngles.x, cameraAngles.y + 180.0f, cameraAngles.z);
                }
                else if (!inputFreeForm.middleMouseButtonPressed && lookingBackwards)
                {
                    lookingBackwards = false;
                    cameraController.cameraRotation = Quaternion.Euler(cameraAngles.x, cameraAngles.y - 180.0f, cameraAngles.z);
                }
            }

            if (cameraController.desiredDistance < 0)
                cameraController.desiredDistance = 0;

            if (smoothing)
            {
                smoothHorizontal = Mathf.Lerp(smoothHorizontal, x, Time.deltaTime * smoothSpeed);
                smoothVertical = Mathf.Lerp(smoothVertical, y, Time.deltaTime * smoothSpeed);
            }
            else
            {
                smoothHorizontal = x;
                smoothVertical = y;
            }

            // stationary feature
            #region Stationary Feature
            {
                var tmpPivotRotation = cameraController.pivotRotation;
                var tmpPivotRotationEuler = tmpPivotRotation.eulerAngles;

                if (tmpPivotRotationEuler.x > 180)
                    tmpPivotRotationEuler.x -= 360;
                if (tmpPivotRotationEuler.y > 180)
                    tmpPivotRotationEuler.y -= 360;

                if (stationaryModeVertical != StationaryModeType.Free)
                {
                    if (stationaryModeVertical != StationaryModeType.Fixed)
                        tmpPivotRotationEuler.x -= smoothVertical;

                    smoothVertical = 0;

                    if (stationaryModeVertical == StationaryModeType.Limited)
                    {
                        if (tmpPivotRotationEuler.x < -stationaryMaxAngleVertical)
                            tmpPivotRotationEuler.x = -stationaryMaxAngleVertical;
                        if (tmpPivotRotationEuler.x > stationaryMaxAngleVertical)
                            tmpPivotRotationEuler.x = stationaryMaxAngleVertical;
                    }
                    else if (stationaryModeVertical == StationaryModeType.RotateWhenLimited)
                    {
                        if (tmpPivotRotationEuler.x > stationaryMaxAngleVertical)
                        {
                            smoothVertical = tmpPivotRotationEuler.x - stationaryMaxAngleVertical;
                            tmpPivotRotationEuler.x = stationaryMaxAngleVertical;
                        }

                        if (tmpPivotRotationEuler.x < -stationaryMaxAngleVertical)
                        {
                            smoothVertical = tmpPivotRotationEuler.x + stationaryMaxAngleVertical;
                            tmpPivotRotationEuler.x = -stationaryMaxAngleVertical;
                        }
                    }
                }

                if (stationaryModeHorizontal != StationaryModeType.Free)
                {
                    if (stationaryModeHorizontal != StationaryModeType.Fixed)
                        tmpPivotRotationEuler.y += smoothHorizontal;

                    smoothHorizontal = 0;

                    if (stationaryModeHorizontal == StationaryModeType.Limited)
                    {
                        if (tmpPivotRotationEuler.y < -stationaryMaxAngleHorizontal)
                            tmpPivotRotationEuler.y = -stationaryMaxAngleHorizontal;
                        if (tmpPivotRotationEuler.y > stationaryMaxAngleHorizontal)
                            tmpPivotRotationEuler.y = stationaryMaxAngleHorizontal;
                    }
                    else if (stationaryModeHorizontal == StationaryModeType.RotateWhenLimited)
                    {
                        if (tmpPivotRotationEuler.y > stationaryMaxAngleHorizontal)
                        {
                            smoothHorizontal = tmpPivotRotationEuler.y - stationaryMaxAngleHorizontal;
                            tmpPivotRotationEuler.y = stationaryMaxAngleHorizontal;
                        }
                        if (tmpPivotRotationEuler.y < -stationaryMaxAngleHorizontal)                        
                        {
                            smoothHorizontal = tmpPivotRotationEuler.y + stationaryMaxAngleHorizontal;
                            tmpPivotRotationEuler.y = -stationaryMaxAngleHorizontal;
                        }                        
                    }                    
                }

                cameraController.pivotRotation = Quaternion.Euler(tmpPivotRotationEuler);
            }
            #endregion            

            Vector3 offsetVectorTransformed = cameraController.target.rotation * cameraController.offsetVector;

            {
                dummy.transform.position = cameraController.transform.position;
                dummy.transform.rotation = cameraController.cameraRotation;

                dummy.transform.RotateAround(cameraController.target.position + offsetVectorTransformed, cameraController.target.up, smoothHorizontal);

                cameraController.transform.position = dummy.transform.position;
                cameraController.cameraRotation = dummy.transform.rotation;
            }

            yAngle = -smoothVertical;
            // Prevent camera flipping
            angle = Vector3.Angle(transform.forward, upVector);

            if (angle <= yAngleLimitMin && yAngle < 0)
            {
                yAngle = 0;
            }
            if (angle >= yAngleLimitMax && yAngle > 0)
            {
                yAngle = 0;
            }

            if (yAngle > 0)
            {
                if (angle + yAngle > 180.0f)
                {
                    yAngle = Vector3.Angle(transform.forward, upVector) - 180;

                    if (yAngle < 0)
                        yAngle = 0;
                }
            }
            else
            {
                if (angle + yAngle < 0.0f)
                {
                    yAngle = Vector3.Angle(transform.forward, downVector) - 180;
                }
            }

            if (!cameraController.smartPivot ||
                cameraController.cameraNormalMode
                &&
                (!cameraController.bGroundHit ||
                (cameraController.bGroundHit && y < 0) ||
                transform.position.y > (cameraController.target.position.y + cameraController.offsetVector.y)))
            {
                // normal mode     

                {
                    dummy.transform.position = cameraController.transform.position;
                    dummy.transform.rotation = cameraController.cameraRotation;
                    dummy.transform.RotateAround(cameraController.target.position + offsetVectorTransformed, dummy.transform.right, yAngle);

                    cameraController.transform.position = dummy.transform.position;

                    cameraController.cameraRotation = dummy.transform.rotation;
                }
            }
            else
            {
                // smart pivot mode
                if (smartPivotInit)
                {
                    smartPivotInit = false;
                    cameraController.InitSmartPivot();
                }

                cameraController.smartPivotRotation *= Quaternion.AngleAxis(yAngle, Vector3.right);

                if (cameraAngles.x > cameraController.startingY || (cameraAngles.x >= 0 && cameraAngles.x < 90))
                {
                    smartPivotInit = true;

                    cameraController.DisableSmartPivot();
                }
            }

            if (forceCharacterDirection)
            {
                cameraController.target.rotation = Quaternion.Euler(targetAngles.x, cameraAngles.y, targetAngles.z);
            }

            if (stabilizeRotation)
            {
                transform.rotation = CameraController.StabilizeRotation(transform.rotation);
            }
        }

        public struct PositionAndRotation
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        public PositionAndRotation RotateAround(PositionAndRotation posAndRot, Vector3 point, Vector3 axis, float angle)
        {
            Vector3 position = posAndRot.position;
            var rot = Quaternion.AngleAxis(angle, axis);
            Vector3 vector3 = rot * (position - point);
            posAndRot.position = point + vector3;

            posAndRot.rotation *= (Quaternion.Inverse(posAndRot.rotation) * rot);

            return posAndRot;
        }
    }
}

