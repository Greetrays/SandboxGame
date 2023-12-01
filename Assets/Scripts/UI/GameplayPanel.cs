using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class GameplayPanel : MonoBehaviour
{
    [SerializeField] private GameObject _playerPanel;
    [SerializeField] private FloatingJoystick _playerJoystic;
    [SerializeField] private Button _playerJumpButton;
    [SerializeField] private GameObject _freeCameraPanel;
    [SerializeField] private ObjectContainer _container;
    [SerializeField] private CinemachineVirtualCamera _mainCamera;
    [SerializeField] private ThirdPersonUserControl _currentPlayer;

    private CinemachineVirtualCamera _thirdPersonCamera;

    private void OnEnable()
    {
        _container.Added += Subscribe;
    }

    private void OnDisable()
    {
        _container.Added -= Subscribe;
    }

    public void ExitToFreeCamera()
    {
        _playerPanel.SetActive(false);
        _freeCameraPanel.SetActive(true);
        _mainCamera.Priority = 1;
        _thirdPersonCamera.Priority = 0;
        _currentPlayer.Disable();
    }

    private void Subscribe(CharacterPlayer characterPlayer)
    {
        characterPlayer.Selected += OnSelected;
    }

    private void OnSelected(ThirdPersonUserControl playerController, CinemachineVirtualCamera camera)
    {
        _thirdPersonCamera = camera;
        _thirdPersonCamera.Priority = 1;
        _playerPanel.SetActive(true);
        _freeCameraPanel.SetActive(false);
        _mainCamera.Priority = 0;
        _currentPlayer = playerController;
        playerController.Initialize(_playerJoystic, _playerJumpButton);
    }
}
