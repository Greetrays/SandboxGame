using RootMotion.Dynamics;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent (typeof(Collider))]

public class CharacterPart : MonoBehaviour, ISelectableObject, IDestructibleObject
{
    [SerializeField] private LayerMask _mask;
    [SerializeField, Range(0, 3)] private float _radiusCollision;
    [SerializeField, Range(1, 20)] private float _destroyTreshold;

    private PuppetMaster _puppetMaster;
    private float _maxVelocity;
    private Rigidbody _rigidbody;
    private Collider _collider;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void Update()
    {
        float currentVelocity = _rigidbody.velocity.magnitude;

        if (Physics.CheckSphere(transform.position, _radiusCollision, _mask))
        {
            if (currentVelocity > _maxVelocity)
            {
                _maxVelocity = currentVelocity;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        DestroyObject(_maxVelocity);

        if (collision.gameObject.TryGetComponent(out IDestructibleObject destructibleObject))
        {
            destructibleObject.DestroyObject(_maxVelocity);
        }

        _maxVelocity = 0;
    }

    public void Select(bool isSelect)
    {
        _puppetMaster.state = isSelect ? PuppetMaster.State.Alive : PuppetMaster.State.Dead;
    }

    public void DestroyObject(float impactForce)
    {
        if (impactForce > _destroyTreshold)
        {
            var broadcaster = _collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();

            if (broadcaster != null)
            {
                broadcaster.puppetMaster.DisconnectMuscleRecursive(broadcaster.muscleIndex, MuscleDisconnectMode.Sever);
            }
        }
    }

    public void Initialize(PuppetMaster puppetMaster)
    {
        if (puppetMaster == null)
            throw new ArgumentException(nameof(puppetMaster));

        _puppetMaster = puppetMaster;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, _radiusCollision);
    }
}
