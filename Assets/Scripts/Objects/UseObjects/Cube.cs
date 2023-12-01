using System.Collections.Generic;
using UnityEngine;

public class Cube : UseObject, ISelectableObject, IDestructibleObject
{
    [SerializeField] private List<Rigidbody> _parts = new List<Rigidbody>();
    [SerializeField] private LayerMask _mask;
    [SerializeField, Range(0, 3)] private float _radiusCollision;
    [SerializeField, Range(1, 20)] private float _destroyTreshold;

    private float _maxVelocity;

    public GameObject GameObject 
    { 
        get => gameObject; 
    }

    public void Select(bool isSelect)
    {
    }

    public void DestroyObject(float impactForce)
    {
        if (impactForce > _destroyTreshold)
        {
            foreach (var part in _parts)
            {
                part.isKinematic = false;
            }
        }
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

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, _radiusCollision);
    }
}
