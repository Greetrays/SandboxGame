using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterPlayer : MonoBehaviour
{
    [SerializeField] private float _delay;
    
    private CinemachineVirtualCamera _camera;
    private ThirdPersonUserControl _userControl;
    private Collider _collider;

    public event UnityAction<ThirdPersonUserControl, CinemachineVirtualCamera> Selected;

    private float _elapsedTime;

    private void OnMouseUp()
    {
        _collider.enabled = true;

        if (_elapsedTime < _delay)
        {
            Selected?.Invoke(_userControl, _camera);
        }

        _elapsedTime = 0;
    }

    private void OnMouseDrag()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime > _delay )
        {
            _collider.enabled = false;
        }
    }

    internal void Initialize(CinemachineVirtualCamera camera, ThirdPersonUserControl userControl, Collider collider)
    {
        _camera = camera;
        _userControl = userControl;
        _collider = collider;
    }
}
