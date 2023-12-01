//----------------------------------------------
//            3rd Person Camera
// Copyright © 2015-2020 Thomas Enzenebner
//            Version 1.0.7.2
//         t.enzenebner@gmail.com
//----------------------------------------------

using System.Collections;
using UnityEngine;

namespace ThirdPersonCamera
{
    [RequireComponent(typeof(CameraController)), RequireComponent(typeof(Follow)), RequireComponent(typeof(FreeForm))]
    public class DisableFollow : MonoBehaviour
    {
        [Header("Basic settings")]
        [Tooltip("Enables motion check with its motionThreshold")]
        public bool activateMotionCheck = true;
        [Tooltip("Enables timed checks and resets to follow after timeToActivate seconds are elapsed")]
        public bool activateTimeCheck = true;
        [Tooltip("Enables reactivation of follow when mouse buttons are released")]
        public bool activateMouseCheck = true;

        [Tooltip("Time in seconds when follow is reactivated. It will also not reactivate when the user still has input.")]
        public float timeToActivate = 1.0f;
        [Tooltip("Distance which has to be crossed to reactivate follow")]
        public float motionThreshold = 0.05f;

        private CameraController cameraController;
        private FreeForm freeForm;
        private Follow follow;

        private bool followDisabled;
        private Vector3 prevPosition;

        private Coroutine timeCheckRoutine = null;

        void Start()
        {
            cameraController = GetComponent<CameraController>();
            follow = GetComponent<Follow>();
            freeForm = GetComponent<FreeForm>();
            followDisabled = !follow.follow;
        }

        void Update()
        {
            if (freeForm.x != 0 || freeForm.y != 0)
            {
                follow.follow = false;
                followDisabled = true;
            }

            if (followDisabled)
            {
                if (activateMotionCheck)
                {
                    Vector3 motionVector = cameraController.target.transform.position - prevPosition;

                    if (motionVector.magnitude > motionThreshold)
                    {
                        follow.follow = true;
                        followDisabled = false;
                    }
                }

                if (activateTimeCheck && timeCheckRoutine == null)
                {
                    timeCheckRoutine = StartCoroutine(ActivateFollow(timeToActivate));
                }

                if (activateMouseCheck && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
                {                    
                    follow.follow = true;
                    followDisabled = false;
                }
            }

            prevPosition = cameraController.target.transform.position;
        }

        public IEnumerator ActivateFollow(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            if (freeForm.x == 0 && freeForm.y == 0)
            {
                follow.follow = true;
                followDisabled = false;
            }

            timeCheckRoutine = null;
        }
    }
}