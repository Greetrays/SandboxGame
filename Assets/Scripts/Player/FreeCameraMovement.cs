using Cinemachine;
using UnityEngine;

public class FreeCameraMovement : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField, Range(0, 20)] private float _speed;
    [SerializeField] private CinemachineVirtualCamera _camera;

    private Vector3 _upDirection;

    private void Update()
    {
        Vector3 direction = _camera.transform.forward * _playerInput.InputV + _camera.transform.right * _playerInput.InputH;

        if (_playerInput.PressSpace)
            _upDirection = Vector3.up;
        else if (_playerInput.PressCtrl)
            _upDirection = -Vector3.up;
        else
            _upDirection = Vector3.zero;

        _camera.transform.position += (_upDirection.normalized + direction.normalized) * _speed * Time.deltaTime;
    }
}
