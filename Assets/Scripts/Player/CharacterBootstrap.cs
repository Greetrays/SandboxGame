using Cinemachine;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterBootstrap : MonoBehaviour
{
    [SerializeField] private CharacterPlayer _characterPlayer;
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private ThirdPersonUserControl _userControl;
    [SerializeField] private Collider _collider;

    public CharacterPlayer ChatacterPlayer => _characterPlayer;

    private void OnEnable()
    {
        _characterPlayer.Initialize(_camera, _userControl, _collider);
    }

}
