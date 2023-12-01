//----------------------------------------------
//            3rd Person Camera
// Copyright © 2015-2020 Thomas Enzenebner
//            Version 1.0.7.2
//         t.enzenebner@gmail.com
//----------------------------------------------
using UnityEngine;

namespace ThirdPersonCamera
{
    public class Targetable : MonoBehaviour
    {
        [Tooltip("Give the target an offset when the transform.position is not fitting.")]
        public Vector3 offset;
    }
}
