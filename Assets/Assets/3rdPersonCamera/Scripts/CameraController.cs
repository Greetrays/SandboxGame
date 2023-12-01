//----------------------------------------------
//            3rd Person Camera
// Copyright © 2015-2020 Thomas Enzenebner
//            Version 1.0.7.2
//         t.enzenebner@gmail.com
//----------------------------------------------

//#define TPC_DEBUG // uncomment for more options and visualizing debug raycasts

using System;
using UnityEngine;
using System.Collections.Generic;

namespace ThirdPersonCamera
{
    public class CameraController : MonoBehaviour
    {
        #region Public Unity Variables
        [Header("Basic settings")]
        [Tooltip("Set this to the transform the camera should follow, can be changed at runtime")]
        public Transform target;

        [Tooltip("Change this vector to offset the pivot the camera is rotating around")]
        public Vector3 offsetVector = new Vector3(0, 1.0f, 0);
        [Tooltip("Offset the camera in any axis without changing the pivot point")]
        public Vector3 cameraOffsetVector = new Vector3(0, 0, 0);

        [Tooltip("The distance how far away the camera should be away from the target")]
        public float desiredDistance = 5.0f;
        [Tooltip("Offset for the camera to not clip in objects")]
        public float collisionDistance = 0.5f;

        [Tooltip("The distance how fast the player can zoom out with each mousewheel event")]
        public float zoomOutStepValue = 1.0f;
        [Tooltip("The speed, each frame, how fast the camera can zoom out automatically to the desired distance")]
        public float zoomOutStepValuePerFrame = 0.1f;

        [Tooltip("Automatically turns off a targets skinned mesh renderer when the camera collider hits the player collider")]
        public bool hideSkinnedMeshRenderers = false;

        [Header("Collision layer settings")]
        [Tooltip("Set this layermask to specify with which layers the camera should collide")]
        public LayerMask collisionLayer;
        [Tooltip("Set this to your player layer so ground checks will ignore the player collider")]
        public LayerMask playerLayer;

        [Header("Features")]
        [Tooltip("Automatically reposition the camera when the character is blocked by an obstacle.")]
        public bool occlusionCheck = true;
        [Tooltip("Uses a pivot when the camera hits the ground and prevents moving the camera closer to the target when looking up.")]
        public bool smartPivot = true;
        [Tooltip("Thickness checking can be configured to ignore  smaller obstacles like sticks, trees, etc... to not reposition or zoom in the camera.")]
        public bool thicknessCheck = true;

        [Header("Smooth target mode")]
        [Tooltip("Enable to smooth out target following, to hide noisy position changes that should not be picked up by the camera immediately or to enable smooth transitions to other targets that are changed during runtime")]
        public bool smoothTargetMode;
        [Tooltip("The speed at which the camera lerps to the actual target position for each axis. 0 means no smoothing will be applied!")]
        public Vector3 smoothTargetValue = new Vector3(7.0f, 1.0f, 7.0f);

        [Header("Thickness check settings")]
        [Tooltip("Adjusts the thickness check. The higher, the thicker the objects can be and there will be no occlusion check.Warning: Very high values could make Occlusion checking obsolete and as a result the followed target can be occluded")]
        public float maxThickness = 0.3f;
        [Tooltip("The number of iterations with that the thickness is calculated. The higher, the more performance it will take.Consider this as the number of objects the thickness ray will go through.")]
        public int maxThicknessIterations = 5;

        [Header("Smart Pivot settings")]
        [Tooltip("Smart pivot is only activated on stable grounds")]
        public bool smartPivotOnlyGround = true;
        [Tooltip("Default ground up for smart pivot")]
        public Vector3 floorNormalUp = new Vector3(0, 1, 0);
        [Tooltip("Only hit results from maxFloorDot to 1 will be valid")]
        public float maxFloorDot = 0.95f;

        [Header("Static pivot rotation settings")]
        [Tooltip("Enable pivot rotation feature to rotate the camera in any direction without the camera moving or rotation around. Used in LockOn and Follow.")]
        public bool pivotRotationEnabled;
        [Tooltip("When enabled the pivot will smoothly slerp to the new pivot angles")]
        public bool smoothPivot = false;
        [Tooltip("Enable overriding the pivot with the public variable 'pivotAngles'")]
        public bool customPivot = false;
        [Tooltip("Use to override pivot")]
        public Vector3 pivotAngles;

        [Header("Extra settings")]
        [Tooltip("The maximum amount of raycast hits that are available for occlusion hits. Can be tweaked for performance. Don't set amount less than 2 when your geometry has many polygons as this can make repositioning unreliable.")]
        public int maxRaycastHitCount = 6;
        [Tooltip("The maximum amount of raycast hits that are available for thickness hits. Can be tweaked for performance. Use an even amount to account for back-front face and don't use less than 2.")]
        public int maxThicknessRaycastHitCount = 6;

#if TPC_DEBUG
        [Header("Debugging")]
        public Vector3 debugPivotAngles;
        public bool drawDesiredDistance;
        public bool drawFinalRaycastHit;
        public bool drawHitNormal;
        public bool drawRaycastHits;
        public bool drawPrecisionRaycast;
        public bool drawDirToHit;
        public bool drawThicknessCubes;
        public bool showDesiredPosition;
#endif

#if !TPC_DEBUG
        [HideInInspector]
#endif
        public bool playerCollision;
#if !TPC_DEBUG
        [HideInInspector]
#endif
        public bool cameraNormalMode;
#if !TPC_DEBUG
        [HideInInspector]
#endif
        public bool bGroundHit;
#if !TPC_DEBUG
        [HideInInspector]
#endif
        public float startingY;

        [HideInInspector]
        public Quaternion cameraRotation;
        [HideInInspector]
        public Quaternion pivotRotation;
        [HideInInspector]
        public Quaternion smartPivotRotation;
        #endregion

        #region Private Variables
        // usually allocated in update, moved to avoid GC problems
        private bool initDone;
        private Vector3 prevTargetPos;
        private Vector3 prevPosition;

        private SkinnedMeshRenderer[] smrs;

#if TPC_DEBUG
        public float distance;
#else
        private float distance;
#endif
        private float thickness;

        private Vector3 localCameraOffsetVector;
        private Vector3 dirToTarget;

        private RaycastHit? occlusionHit = null;        
        private RaycastHit offsetTest;

        private List<RayCastWithMags> rcms;
        private List<RayCastWithMags> sortList;
        private SortRayCastsTarget sortMethodTarget;
        private SortRayCastsDot sortMethodFinalDistance;
        private SortDistance sortMethodHitDistance;

        private Vector3 targetPosition;
        private Vector3 smoothedTargetPosition;

        private bool offsetTestNeeded;

        private RaycastHit[] startHits;
        private RaycastHit[] endHits;
        private RaycastHit[] hits;
        private Quaternion currentPivotRotation;

#if TPC_DEBUG
        public bool increaseX = false;
        private bool internalIncreaseX = false;
        public Vector3 increaseVector;
        public Vector3 savedVector;
#endif
#endregion

#region Public Get/Set Variables
        public float Distance
        {
            get
            {
                return distance;
            }
        }

#endregion


        void Awake()
        {
            initDone = false;

            startHits = new RaycastHit[maxThicknessRaycastHitCount];
            endHits = new RaycastHit[maxThicknessRaycastHitCount];
            hits = new RaycastHit[maxRaycastHitCount];

            cameraRotation = transform.rotation;
            smartPivotRotation = Quaternion.identity;
            pivotRotation = Quaternion.identity;
            currentPivotRotation = Quaternion.identity;

            sortMethodTarget = new SortRayCastsTarget();
            sortMethodFinalDistance = new SortRayCastsDot();
            sortMethodHitDistance = new SortDistance();

            distance = desiredDistance;

            cameraNormalMode = true;
            playerCollision = false;

            UpdateOffsetVector(offsetVector);
            UpdateCameraOffsetVector(cameraOffsetVector);

            InitFromTarget();
        }

        void InitFromTarget()
        {
            InitFromTarget(target);
        }

        /// <summary>
        /// Use this to re-init a target, for example, when switching through multiple characters
        /// </summary>
        /// <param name="newTarget"></param>
        public void InitFromTarget(Transform newTarget)
        {
            target = newTarget;

            if (target != null)
            {
                cameraRotation = transform.rotation;
                prevTargetPos = target.position;
                prevPosition = transform.position;
                targetPosition = target.position;
                smoothedTargetPosition = target.position;

                if (hideSkinnedMeshRenderers)
                    smrs = target.GetComponentsInChildren<SkinnedMeshRenderer>();
                else
                    smrs = null;

                // get colliders from target
                var colliders = target.GetComponentsInChildren<Collider>();

                foreach (var col in colliders)
                {
                    if (!playerLayer.IsInLayerMask(col.gameObject))
                        Debug.LogWarning("The target \"" + col.gameObject.name + "\" has a collider which is not in the player layer. To fix: Change the layer of " + col.gameObject.name + " to the layer referenced in CameraController->Player layer");
                }

                var selfCollider = GetComponent<Collider>();

                if (selfCollider == null && hideSkinnedMeshRenderers)
                {
                    Debug.LogWarning("HideSkinnedMeshRenderers is activated but there's no collider on the camera. Example to fix: Atach a BoxCollider to the camera and set it to trigger.");
                }

                if (selfCollider && collisionLayer.IsInLayerMask(gameObject))
                {
                    Debug.LogWarning("Camera is colliding with the collision layer. Consider changing the camera to a layer that isn't in the collision layer. Example to fix: Change the camera to the player layer.");
                }

                initDone = true;
            }
        }

        void LateUpdate()
        {
#if TPC_DEBUG
            ResetCubes();
#endif

            if (target == null)
                return;

            if (!initDone)
                return;

            if (distance < 0)
                distance = 0;

            // disable player character when too close
            if (hideSkinnedMeshRenderers && smrs != null)
            {
                if (playerCollision || distance <= collisionDistance)
                {
                    for (int i = 0; i < smrs.Length; i++)
                    {
                        smrs[i].enabled = false;
                    }
                }
                else
                {
                    for (int i = 0; i < smrs.Length; i++)
                    {
                        smrs[i].enabled = true;
                    }
                }
            }

#if TPC_DEBUG
            if (increaseX)
            {
                Vector3 tmp = cameraRotation.eulerAngles;

                if (!internalIncreaseX)
                {
                    savedVector = tmp;
                }

                internalIncreaseX = true;
                tmp += increaseVector;
                cameraRotation = Quaternion.Euler(tmp);
            }
            else if (internalIncreaseX)
            {
                internalIncreaseX = false;
                cameraRotation = Quaternion.Euler(savedVector);
            }
#endif

            Quaternion transformRotation = cameraRotation;

            localCameraOffsetVector = Vector3.Slerp(localCameraOffsetVector, cameraOffsetVector, Time.deltaTime * 3.0f);
            Vector3 offsetVectorTransformed = target.rotation * offsetVector;
            Vector3 cameraOffsetVectorTransformed = transformRotation * localCameraOffsetVector;

            if (smoothTargetMode)
            {
                if (smoothTargetValue.x != 0)
                    smoothedTargetPosition.x = Mathf.Lerp(smoothedTargetPosition.x, target.position.x, smoothTargetValue.x * Time.deltaTime);
                if (smoothTargetValue.y != 0)
                    smoothedTargetPosition.y = Mathf.Lerp(smoothedTargetPosition.y, target.position.y, smoothTargetValue.y * Time.deltaTime);
                if (smoothTargetValue.z != 0)
                    smoothedTargetPosition.z = Mathf.Lerp(smoothedTargetPosition.z, target.position.z, smoothTargetValue.z * Time.deltaTime);

                targetPosition = smoothedTargetPosition;
            }
            else
            {
                targetPosition = target.position;
            }

            Vector3 targetPosWithOffset = (targetPosition + offsetVectorTransformed);
            Vector3 targetPosWithCameraOffset = targetPosition + offsetVectorTransformed + cameraOffsetVectorTransformed;

            Vector3 testDir = targetPosWithCameraOffset - targetPosWithOffset;

            Vector3 desiredPosition = (transformRotation * (new Vector3(0, 0, -desiredDistance) + localCameraOffsetVector) + offsetVectorTransformed + targetPosition);

            if (offsetTestNeeded &&
                Physics.SphereCast(targetPosWithOffset, collisionDistance * 1.01f, testDir, out offsetTest, testDir.magnitude, collisionLayer))
            {
                // offset clips into geometry, move the offset                
                float ratio = 1.0f - ((offsetTest.distance) / testDir.magnitude);
                cameraOffsetVectorTransformed = Vector3.Slerp(cameraOffsetVectorTransformed, Vector3.zero, ratio);

                localCameraOffsetVector = Quaternion.Inverse(transformRotation) * cameraOffsetVectorTransformed;
                targetPosWithCameraOffset = targetPosition + offsetVectorTransformed + cameraOffsetVectorTransformed;
            }

#if TPC_DEBUG
            if (showDesiredPosition)
                ShowThicknessCube(desiredPosition, "desiredPosition");
#endif
            dirToTarget = (desiredPosition - targetPosWithCameraOffset).normalized;
            
            // sweep from target to desiredPosition and check for collisions
            int hitCount = Physics.SphereCastNonAlloc(targetPosWithCameraOffset, collisionDistance, dirToTarget, hits, desiredDistance, collisionLayer);

            Array.Sort(hits, 0, hitCount, sortMethodHitDistance);

            if (hitCount > 0)
            {
                if (Physics.CheckSphere(desiredPosition, collisionDistance, collisionLayer))
                {
                    RaycastHit overlapHit;
                    List<RaycastHit> overlapHits = new List<RaycastHit>(hits);
#if TPC_DEBUG
                    if (drawPrecisionRaycast || drawRaycastHits)
                        Debug.DrawLine(targetPosWithCameraOffset, targetPosWithCameraOffset + (dirToTarget * (desiredDistance + 0.01f)), Color.blue);
#endif
                    if (Physics.Raycast(targetPosWithCameraOffset, dirToTarget, out overlapHit, desiredDistance + collisionDistance, collisionLayer))
                    {
                        overlapHits.Add(overlapHit);
                    }

                    hits = overlapHits.ToArray();
                }
            }
#if TPC_DEBUG
            if(drawDesiredDistance)
                Debug.DrawLine(targetPosWithCameraOffset, desiredPosition, Color.red);
#endif

            if (hitCount > 0)
            {
                rcms = new List<RayCastWithMags>(hitCount);

                for (int i = 0; i < hitCount; i++)
                {
                    var hit = hits[i];

                    if (hit.point != Vector3.zero || hit.normal != -dirToTarget)
                    {
                        RayCastWithMags rcm = new RayCastWithMags();

                        Vector3 finalPosition = hit.point + (hit.normal * collisionDistance);
                        var tmpDirToHit = (finalPosition - targetPosWithCameraOffset);

                        rcm.hit = hit;                        
                        rcm.distanceFromTarget = tmpDirToHit.magnitude;
                        rcm.dot = Vector3.Dot(tmpDirToHit, dirToTarget);                     

                        rcms.Add(rcm);
                    }
#if TPC_DEBUG
                    if (drawRaycastHits)
                        Debug.DrawLine(targetPosWithCameraOffset, hit.point, Color.blue);
#endif
                }

                sortList = new List<RayCastWithMags>(rcms);

                if (sortList.Count > 0)
                {
                    sortList.Sort(sortMethodTarget);
                    float lowestMagn = sortList[0].distanceFromTarget;

                    for (int i = sortList.Count - 1; i >= 0; i--)
                    {
                        if (sortList[i].distanceFromTarget > lowestMagn + collisionDistance * 2)
                            sortList.RemoveAt(i);
                    }

                    if (sortList.Count > 0)
                    {
                        sortList.Sort(sortMethodFinalDistance);
                        occlusionHit = sortList[0].hit;
                    }
                    else
                        occlusionHit = null;

#if TPC_DEBUG
                    if (drawFinalRaycastHit && occlusionHit != null)
                        Debug.DrawLine(targetPosWithCameraOffset, occlusionHit.Value.point, Color.blue);
#endif
                }
                else
                    occlusionHit = null;
            }
            else
            {
                occlusionHit = null;
            }

            // get current thickness between target and camera
            if (thicknessCheck && occlusionHit != null)
            {
                if (!occlusionHit.Value.collider.GetComponent<TerrainCollider>()) // note: has GC allocs in editor
                {
                    thickness = GetThicknessFromCollider(targetPosWithCameraOffset - (dirToTarget * collisionDistance), desiredPosition);
                }
            }
            else
                thickness = float.MaxValue;

            float hitDistance = 0;
            float cameraToHitDistance = 0;

            if (occlusionHit != null)
            {
                hitDistance = occlusionHit.Value.distance;
                cameraToHitDistance = (transform.position - occlusionHit.Value.point).magnitude;
            }
            else
            {
                hitDistance = desiredDistance;
            }

            // Cast ground target to activate smartPivot
            if ((smartPivot && occlusionHit != null) || bGroundHit)
            {
                if (Physics.CheckSphere(prevPosition, collisionDistance, collisionLayer))
                {
                    if (smartPivotOnlyGround)
                    {
                        RaycastHit groundHit;
                        if (Physics.Raycast(prevPosition, -target.up, out groundHit, collisionDistance * 2.0f, collisionLayer))
                        {
                            float dot = Vector3.Dot(floorNormalUp, groundHit.normal);

                            if (dot > maxFloorDot)
                                bGroundHit = true;
                        }
                    }
                    else
                        bGroundHit = true;
                }
                else
                    bGroundHit = false;
            }
            else if (occlusionHit == null && cameraNormalMode)
            {
                bGroundHit = false;
            }

            // Avoid that the character is not visible            
            if (occlusionCheck && occlusionHit != null)
            {
                Vector3 occlusionPoint = occlusionHit.Value.point + (occlusionHit.Value.normal * collisionDistance);

                float newDist = (targetPosWithCameraOffset - occlusionPoint).magnitude;
                float lastDistance = (targetPosWithCameraOffset - prevPosition).magnitude;

                if (thickness > maxThickness)
                {
                    if (newDist > desiredDistance)
                    {
                        AdjustDistance(desiredDistance);
                        transform.position = (transformRotation * (new Vector3(0, 0, -desiredDistance) + localCameraOffsetVector)) + offsetVectorTransformed + targetPosition;
                    }
                    else
                    {
                        // reposition camera to hit point
                        if (desiredDistance > newDist && newDist > distance + collisionDistance)
                        {
                            AdjustDistance(newDist);
                            transform.position = (transformRotation * (new Vector3(0, 0, -distance) + localCameraOffsetVector)) + offsetVectorTransformed + targetPosition; ;
                        }
                        else
                        {
                            if (newDist < distance)
                                distance = newDist;
                            else
                                AdjustDistance(newDist);

                            transform.position = occlusionPoint;
                        }
#if TPC_DEBUG
                        if (drawDirToHit)
                            Debug.DrawLine(desiredPosition, occlusionPoint, Color.green);
                        if (drawHitNormal)
                            Debug.DrawLine(occlusionHit.Value.point, occlusionHit.Value.point + occlusionHit.Value.normal * 10.0f, Color.green);                            
#endif
                    }
                }
                else
                {
                    if (cameraNormalMode)
                        AdjustDistance(desiredDistance);

                    transform.position = (transformRotation * (new Vector3(0, 0, -distance) + localCameraOffsetVector)) + offsetVectorTransformed + targetPosition;
                }

            }
            else if (cameraNormalMode)
            {
                thickness = float.MaxValue;
                
                AdjustDistance(desiredDistance);
                transform.position = transformRotation * (new Vector3(0, 0, -distance) + localCameraOffsetVector) + offsetVectorTransformed + targetPosition;
            }
            else if (!cameraNormalMode)
            {
                transform.position += (targetPosition - prevTargetPos);
            }

            transform.rotation = cameraRotation;

            if (!cameraNormalMode)
            {
                transform.rotation *= smartPivotRotation;
            }

            if (pivotRotationEnabled)
            {
                if (!customPivot)
                {                  
                    if (smoothPivot)
                        currentPivotRotation = Quaternion.Slerp(currentPivotRotation, pivotRotation, Time.deltaTime);
                    else
                        currentPivotRotation = pivotRotation;
                }
                else
                    currentPivotRotation = Quaternion.Slerp(currentPivotRotation, Quaternion.Euler(pivotAngles), Time.deltaTime);

                transform.rotation *= currentPivotRotation;
#if TPC_DEBUG
                debugPivotAngles = currentPivotRotation.eulerAngles;
#endif
            }

            transform.rotation = StabilizeRotation(transform.rotation);

            prevTargetPos = targetPosition;
            prevPosition = transform.position;
        }

        public void RotateTo(Quaternion targetRotation, float timeModifier)
        {
            cameraRotation = StabilizeSlerpRotation(cameraRotation, targetRotation, timeModifier);
        }

        public float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }

        public void InitSmartPivot()
        {
            cameraNormalMode = false;

            smartPivotRotation = Quaternion.identity;

            startingY = cameraRotation.eulerAngles.x;            
        }

        public void DisableSmartPivot()
        {
            cameraNormalMode = true;

            Vector3 tmpEuler = cameraRotation.eulerAngles + smartPivotRotation.eulerAngles;            
            cameraRotation = Quaternion.Euler(tmpEuler);
        }

        public void OnTriggerEnter(Collider c)
        {
            if (c.transform == target || playerLayer.IsInLayerMask(c.gameObject))
            {
                playerCollision = true;
            }
        }

        public void OnTriggerExit(Collider c)
        {
            if (c.transform == target || playerLayer.IsInLayerMask(c.gameObject))
            {
                playerCollision = false;
            }
        }

#if TPC_DEBUG
        public Queue<GameObject> debugThicknessCubes = new Queue<GameObject>();
        public Queue<GameObject> activeDebugThicknessCubes = new Queue<GameObject>();

        public void ShowThicknessCube(Vector3 position, string name)
        {
            GameObject cube = null;

            if (debugThicknessCubes.Count > 0)
            {
                cube = debugThicknessCubes.Dequeue();
            }
            else
            {
                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                Destroy(cube.GetComponent<BoxCollider>());
            }

            cube.SetActive(true);
            cube.transform.position = position;
            cube.name = name;
            activeDebugThicknessCubes.Enqueue(cube);
        }

        public void ResetCubes()
        {
            while (activeDebugThicknessCubes.Count > 0)
            {
                GameObject cube = activeDebugThicknessCubes.Dequeue();

                cube.name = "unused";
                cube.SetActive(false);

                debugThicknessCubes.Enqueue(cube);
            }
        }
#endif

        public class ThicknessCheckList
        {
            public bool marked;
            public Collider collider;
            public Vector3 point;
            public int linkedIndex;
        }

       

        public float GetThicknessFromCollider(Vector3 start, Vector3 end)
        {
            Vector3 dir = (end - start);
            float length = dir.magnitude;
            Vector3 dirToHit = dir.normalized;

            int startHitsCount = Physics.SphereCastNonAlloc(start, collisionDistance, dirToHit, startHits, length, collisionLayer);
            int endHitsCount = Physics.SphereCastNonAlloc(end, collisionDistance, -dirToHit, endHits, length, collisionLayer);

            Array.Sort(startHits, 0, startHitsCount, sortMethodHitDistance);
            Array.Sort(endHits, 0, endHitsCount, sortMethodHitDistance);

            float finalThickness = float.MaxValue;
            bool finalThicknessFound = false;

            if (startHitsCount > 0 && endHitsCount > 0)
            {
                ThicknessCheckList[] startHitList = new ThicknessCheckList[startHitsCount];
                ThicknessCheckList[] endHitList = new ThicknessCheckList[endHitsCount];
                Dictionary<Collider, float> colliderThickness = new Dictionary<Collider, float>();

                for (int i = 0; i < startHitsCount; i++)
                {
                    var tmp = startHits[i];

                    if (!smartPivot || 
                        (smartPivot && (cameraNormalMode || (!cameraNormalMode && Vector3.Dot(tmp.normal, floorNormalUp) < maxFloorDot)))) // filter floor hits when smart pivot is enabled 
                    {
                        if (tmp.normal == dirToHit && tmp.point == Vector3.zero)                        
                            startHitList[i] = new ThicknessCheckList() { collider = tmp.collider, point = start - dirToHit * maxThickness, linkedIndex = -1, marked = false };                        
                        else
                            startHitList[i] = new ThicknessCheckList() { collider = tmp.collider, point = tmp.point , linkedIndex = -1, marked = false };

                        if (!colliderThickness.ContainsKey(tmp.collider))
                            colliderThickness.Add(tmp.collider, 0);
                    }
                }

                for (int i = 0; i < endHitsCount; i++)
                {
                    var tmp = endHits[i];

                    if (tmp.normal == dirToHit && tmp.point == Vector3.zero)
                        endHitList[i] = new ThicknessCheckList() { collider = tmp.collider, point = end + dirToHit * maxThickness, linkedIndex = -1, marked = false };
                    else
                        endHitList[i] = new ThicknessCheckList() { collider = tmp.collider, point = tmp.point, linkedIndex = -1, marked = false };

                    if (!colliderThickness.ContainsKey(tmp.collider))
                        colliderThickness.Add(tmp.collider, 0);
                }                             

                foreach (var entry in colliderThickness)
                {
                    bool thicknessFound = false;
                    float tmpThickness = float.MaxValue;

                    var collider = entry.Key;

                    for (int i = 0; i < startHitList.Length; i++)
                    {
                        var tmpStart = startHitList[i];

                        if (tmpStart == null)
                            continue;

                        if (!tmpStart.marked && tmpStart.collider == collider)
                        {
                            for (int ii = endHitList.Length - 1; ii >= 0; ii--)
                            {
                                var tmpEnd = endHitList[ii];

                                if (tmpEnd == null)
                                    continue;

                                if (!tmpEnd.marked && tmpEnd.collider == collider)
                                {
                                    tmpStart.marked = true;
                                    tmpEnd.marked = true;
                                    tmpStart.linkedIndex = ii;
                                    tmpEnd.linkedIndex = i;
                                }
                            }
                        }
                    }

                    bool endPointFound = false;
                    for (int i = 0; i < startHitList.Length; i++)
                    {
                        var tmpStart = startHitList[i];

                        if (tmpStart == null)
                            continue;

                        if (tmpStart.marked && tmpStart.collider == collider && tmpStart.linkedIndex != -1)
                        {
                            if (!thicknessFound)
                            {
                                thicknessFound = true;
                                tmpThickness = 0;
                            }

                            if (!finalThicknessFound)
                            {
                                finalThicknessFound = true;
                                finalThickness = 0;
                            }

                            endPointFound = true;

                            float thickness = (tmpStart.point - endHitList[tmpStart.linkedIndex].point).magnitude;
                            tmpThickness += thickness;
                        }
                    }

                    if (!endPointFound)
                    {
                        return float.MaxValue;
                    }
                    else
                    {
                        if (tmpThickness > maxThickness)
                            finalThickness += tmpThickness;
                    }
                }                
            }

            return finalThickness;
        }

        public void IncrementDistance(float targetDistance)
        {
            distance += zoomOutStepValuePerFrame;

            if (distance > targetDistance)
                distance = targetDistance;
        }

        public void DecrementDistance(float targetDistance)
        {
            distance -= zoomOutStepValuePerFrame;

            if (distance < targetDistance)
                distance = targetDistance;
        }

        public void AdjustDistance(float targetDistance)
        {
            if (Mathf.Abs(targetDistance - distance) < zoomOutStepValuePerFrame)
            {
                distance = targetDistance;
                return;
            }

            if (targetDistance > distance)
                IncrementDistance(targetDistance);
            else if (targetDistance < distance)
                DecrementDistance(targetDistance);
        }

        /// <summary>
        /// Use this method to update the offset vector during runtime.
        /// In case the offset vector is inside the collision sphere
        /// the offset vector gets moved to a position outside of it
        /// </summary>
        /// <param name="newOffset"></param>
        public void UpdateOffsetVector(Vector3 newOffset)
        {
            if (newOffset.sqrMagnitude > collisionDistance)
                offsetVector = newOffset;
            else
            {
                // when the offset vector ends up inside the collision sphere, add a small vector plus the collision distance to fix collision glitches
                offsetVector = offsetVector + Vector3.up * (collisionDistance + 0.01f);
            }
        }

        /// <summary>
        /// Use this method to update the camera offset vector during runtime.
        /// Updating it like that removes the need to calculate the sqrMagnitude of the vector
        /// every frame.
        /// </summary>
        /// <param name="newOffset"></param>
        public void UpdateCameraOffsetVector(Vector3 newOffset)
        {
            cameraOffsetVector = newOffset;

            if (cameraOffsetVector.sqrMagnitude > 0)
                offsetTestNeeded = true;
        }

        // Slerping to new rotation sometimes leads to wrong rotation in Z-axis        
        // so any Z rotation will be removed
        public static Quaternion StabilizeRotation(Quaternion rotation)
        {
            return Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
        }

        public static Quaternion StabilizeSlerpRotation(Quaternion from, Quaternion to, float timeModifier)
        {
            var slerpedRotation = Quaternion.Slerp(from, to, timeModifier);
            return Quaternion.Euler(slerpedRotation.eulerAngles.x, slerpedRotation.eulerAngles.y, 0);
        }
    }
}