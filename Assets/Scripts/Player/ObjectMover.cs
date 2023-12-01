using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [SerializeField] private float _forceAmount;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private LayerMask _layerMask;

    private ISelectableObject _selectObject = null;
    private Rigidbody _selectedRigidbody;
    private Vector3 _originalScreenTargetPosition;
    private Vector3 _originalRigidbodyPos;
    private float _selectionDistance;

    private void Update()
    {
        if (_playerInput.MouseButtonDrag && _selectedRigidbody == null)
        {
            _selectedRigidbody = GetRigidbodyFromMouseClick();
        }
        if (_playerInput.MouseButtonUp && _selectedRigidbody != null)
        {
            _selectedRigidbody = null;
            _selectObject.Select(true);
        }
    }

    private void FixedUpdate()
    {
        if (_selectedRigidbody)
        {
            Vector3 mousePositionOffset = _targetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _selectionDistance)) - _originalScreenTargetPosition;
            _selectedRigidbody.velocity = (_originalRigidbodyPos + mousePositionOffset - _selectedRigidbody.transform.position) * _forceAmount * Time.deltaTime;
        }
    }

    private Rigidbody GetRigidbodyFromMouseClick()
    {
        RaycastHit hitInfo = new RaycastHit();
        Ray ray = _targetCamera.ScreenPointToRay(Input.mousePosition);
        bool hit = Physics.Raycast(ray, out hitInfo, _layerMask);

        if (hit)
        {
            if (hitInfo.collider.gameObject.GetComponent<Rigidbody>())
            {
                if (hitInfo.collider.TryGetComponent(out _selectObject))
                {
                    _selectObject.Select(false);
                    _selectionDistance = Vector3.Distance(ray.origin, hitInfo.point);
                    _originalScreenTargetPosition = _targetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _selectionDistance));
                    _originalRigidbodyPos = hitInfo.collider.transform.position;
                    return hitInfo.collider.gameObject.GetComponent<Rigidbody>();
                }
            }
        }
            
        return null;
    }
}
