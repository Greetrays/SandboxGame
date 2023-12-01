//----------------------------------------------
//            3rd Person Camera
// Copyright © 2015-2020 Thomas Enzenebner
//            Version 1.0.7.2
//         t.enzenebner@gmail.com
//----------------------------------------------
using UnityEngine;

namespace ThirdPersonCamera
{
    public class Follow : MonoBehaviour
    {
        [Header("Basic settings")]
        [Tooltip("Enables/Disables the follow mode")]
        public bool follow = true;
        [Tooltip("How fast the camera should align to the transform.forward of the target")]
        public float rotationSpeed = 1.0f;
        [Tooltip("Applies an additional vector to tilt the camera in place. Useful when the offset vector gets too big and leaves the collision box of the model")]
        public Vector3 tiltVector;
        [Tooltip("The default time in seconds the script will be disabled when the player has input in FreeForm")]
        public float disableTime = 1.5f;

        [Header("Slope aligning")]
        [Tooltip("Enables/Disables automatic alignment of the camera when the target is moving on upward or downward slopes")]
        public bool alignOnSlopes = true;
        [Tooltip("The speed at which the camera lerps to the adjusted slope rotation")]
        public float rotationSpeedSlopes = 0.5f;
        [Tooltip("Set the camera to rotate downwards instead of upwards when the target hits a slope")]
        public bool alignDirectionDownwards = false;
        [Tooltip("The layer which should be used for the ground/slope checks. Usually just the Default layer.")]
        public LayerMask layerMask;

        [Header("Backward motion aligning")]
        [Tooltip("Enables/Disables looking backwards")]
        public bool lookBackwards = false;
        [Tooltip("Enables/Disables automatic checking when the camera should look back")]
        public bool checkMotionForBackwards = true;
        [Tooltip("The minimum magnitude of the motion vector when the camera should consider looking back")]
        public float backwardsMotionThreshold = 0.05f;
        [Tooltip("The minimum angle when the camera should consider looking back")]
        public float angleThreshold = 170.0f;

        private float currentDisableTime = 0.0f;
        private Vector3 prevPosition;
        private CameraController cc;
        private FreeForm freeForm;

        void Start()
        {
            cc = GetComponent<CameraController>();
            freeForm = GetComponent<FreeForm>();
        }

        void Update()
        {
            if (cc == null || cc.target == null)
                return;

            if (freeForm != null)
            {
                var input = freeForm.GetInput();
                if (input.inputFreeLook)
                {
                    currentDisableTime = disableTime;
                }
            }

            if (currentDisableTime > 0)
            {
                currentDisableTime -= Time.deltaTime;
                return;
            }

            if (!follow)
            {
                cc.smoothPivot = false;
                return;
            }
            else
                cc.smoothPivot = true;

            var forward = cc.target.transform.forward;

            if (checkMotionForBackwards)
            {
                Vector3 motionVector = cc.target.transform.position - prevPosition;

                if (motionVector.magnitude > backwardsMotionThreshold)
                {
                    float angle = Vector3.Angle(motionVector, forward);

                    if (angle > angleThreshold)
                    {
                        lookBackwards = true;
                    }
                    else
                        lookBackwards = false;
                }

                prevPosition = cc.target.transform.position;
            }

            Quaternion toRotation = Quaternion.LookRotation((!lookBackwards ? forward + tiltVector : -forward + tiltVector), Vector3.up);

            if (alignOnSlopes)
            {
                RaycastHit raycastHit;
                Vector3 upVector = Vector3.up;

                if (Physics.Raycast(cc.target.transform.position, Vector3.down, out raycastHit, 25.0f, layerMask)) // if the range of 15.0 is not enough, increase the value
                {
                    upVector = raycastHit.normal;
                }

                float angle = Vector3.Angle(Vector3.up, upVector);

                if (alignDirectionDownwards)
                    angle = -angle;
                                        
                toRotation = CameraController.StabilizeSlerpRotation(toRotation, toRotation * Quaternion.AngleAxis(angle, Vector3.right), Time.deltaTime * rotationSpeedSlopes);
            }

            //if (cc.pivotAngles.sqrMagnitude < 0.1f)
            //    cc.pivotAngles = Vector3.zero;
            //else
            //    cc.pivotAngles = Vector3.Slerp(cc.pivotAngles, Vector3.zero, Time.deltaTime * rotationSpeed);
            cc.pivotRotation = Quaternion.Slerp(cc.pivotRotation, Quaternion.identity, Time.deltaTime * rotationSpeed);

            cc.cameraRotation = CameraController.StabilizeSlerpRotation(cc.cameraRotation, toRotation, Time.deltaTime * rotationSpeed);            
        }

        public float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }
    }
}