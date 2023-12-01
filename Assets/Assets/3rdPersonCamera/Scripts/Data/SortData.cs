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
    public struct RayCastWithMags
    {
        public RaycastHit hit;        
        public float distanceFromTarget;
        public float dot;
        public float finalDistance;
    }

    public class TransformWithTime
    {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
        public float time;
    }

    public class SortRayCastsTarget : IComparer<RayCastWithMags>
    {
        public int Compare(RayCastWithMags a, RayCastWithMags b)
        {
            if (a.distanceFromTarget > b.distanceFromTarget) return 1;
            else if (a.distanceFromTarget < b.distanceFromTarget) return -1;
            else return 0;
        }
    }
    public class SortDistance : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b)
        {
            if (a.distance > b.distance) return 1;
            else if (a.distance < b.distance) return -1;
            else return 0;
        }
    }

    public class SortRayCastsDot : IComparer<RayCastWithMags>
    {
        public int Compare(RayCastWithMags a, RayCastWithMags b)
        {
            if (a.dot > b.dot) return 1;
            else if (a.dot < b.dot) return -1;
            else return 0;
        }
    }

    public class SortRayCastsFinalDistance : IComparer<RayCastWithMags>
    {
        public int Compare(RayCastWithMags a, RayCastWithMags b)
        {
            if (a.finalDistance > b.finalDistance) return 1;
            else if (a.finalDistance < b.finalDistance) return -1;
            else return 0;
        }
    }

    // Data for LockOnTarget

    public struct TargetableWithDistance
    {
        public Targetable target { get; set; }
        public float distance { get; set; }
        public float angle { get; set; }

        public float score;
    }

    public class SortTargetables : IComparer<TargetableWithDistance>
    {
        public int Compare(TargetableWithDistance a, TargetableWithDistance b)
        {
            if (a.score > b.score)
                return 1;
            else if (a.score < b.score)
                return -1;
            else
                return 0;
        }
    }
}
