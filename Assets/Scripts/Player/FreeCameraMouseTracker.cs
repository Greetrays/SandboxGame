using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class FreeCameraMouseTracker : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField, Range(0f, 100f)] private float _elastic;
    [SerializeField, Range(0f, 1f)] private float _sencetivity;
    [SerializeField] private CinemachineVirtualCamera _camera;

    private const int _minRotation = -50;
    private const int _maxRotation = 70;

    private float _mouseX;
    private float _mouseY;

    private void Update()
    {
        _mouseX += _playerInput.InputMouseX * _sencetivity;
        _mouseY -= _playerInput.InputMouseY * _sencetivity;
        _mouseY = Mathf.Clamp(_mouseY, _minRotation, _maxRotation);

        Vector3 newRotation = new Vector3(_mouseY , _mouseX , 0);

        _camera.transform.rotation = Quaternion.Lerp(_camera.transform.rotation, Quaternion.Euler(newRotation), _elastic * Time.deltaTime);
    }
}
