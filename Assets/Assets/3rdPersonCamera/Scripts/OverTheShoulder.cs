//----------------------------------------------
//            3rd Person Camera
// Copyright © 2015-2020 Thomas Enzenebner
//            Version 1.0.7.2
//         t.enzenebner@gmail.com
//----------------------------------------------
using UnityEngine;

namespace ThirdPersonCamera
{
    [RequireComponent(typeof(CameraController))]
    public class OverTheShoulder : MonoBehaviour
    {
        [Header("Basic settings")]
        [Tooltip("The distance the camera moves away from its zero position. 0.5 means it'll set the max camera offset vector to (axis * maxValue), so -0.5f to 0.5f")]
        public float maxValue;
        [Tooltip("How fast it lerps to the max position when starting to aim")]
        public float aimSpeed = 7.0f;
        [Tooltip("How fast it lerps to the zero position when releasing")]
        public float releaseSpeed = 7.0f;
        [Tooltip("When activated the camera will slide to the left")]
        public bool left;
        [Tooltip("The base offset serves as starting and endpoint when releasing.")]
        public Vector3 baseOffset = new Vector3(0, 0, 0);
        [Tooltip("You can tweak the axis on which the camera slides on, usually it will be just operating on the x axis to slide left and right from the targeted character but it can be changed to any direction in case gravity changes for example. The intended design is to use normalized values between - 1 and 1 The difference to the \"Additional Axis Movement\" vector is that the slide axis goes back and forth when aiming / releasing")]
        public Vector3 slideAxis = new Vector3(1, 0, 0);
        [Tooltip("This axis can be used to have additional offsets when aiming. Unlike the slide axis this axis is intended for non - normalized values much like the base offset. It can be used to make the camera zoom high above the character for example when aiming.")]
        public Vector3 additionalAxisMovement = new Vector3(0, 0, 0);

        [Header("Extra settings")]
        [Tooltip("Inform the script of using a custom input method to set the CameraInputShoulder model")]
        public bool customInput = false;

        [Header("Reference settings")]
        [Tooltip("Reference to CameraController, this is set automatically when OverTheShoulder is on the same GameObject as CameraController")]
        public CameraController cc;

        private Vector3 offsetValue;
        private CameraInputShoulder inputShoulder;

        void Start()
        {
            offsetValue = Vector3.zero;

            if (cc == null)
                cc = GetComponent<CameraController>();

            if (!customInput)
            {
                var lookup = GetComponent<CameraInputSampling_Shoulder>();
                if (lookup == null)
                    Debug.LogError("CameraInputSampling_Shoulder not found on " + transform.name + ". Consider adding it to get input sampling or enable customInput to skip this message");
            }
        }

        public CameraInputShoulder GetInput()
        {
            return inputShoulder;
        }

        public void UpdateInput(CameraInputShoulder newInput)
        {
            inputShoulder = newInput;
        }

        void Update()
        {
            bool aiming = inputShoulder.aiming;            

            if (aiming) // aim mode
            {
                bool left = inputShoulder.left;

                Vector3 newOffset = left ? -maxValue * slideAxis + baseOffset + additionalAxisMovement : maxValue * slideAxis + baseOffset + additionalAxisMovement;
                offsetValue = Vector3.Lerp(offsetValue, newOffset, Time.deltaTime * aimSpeed);

                cc.UpdateCameraOffsetVector(offsetValue);
            }
            else if (offsetValue != Vector3.zero) // release mode
            {                
                offsetValue = Vector3.Lerp(offsetValue, baseOffset, Time.deltaTime * releaseSpeed);

                cc.UpdateCameraOffsetVector(offsetValue);
            }
        }
    }
}