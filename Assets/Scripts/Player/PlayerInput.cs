using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private FloatingJoystick _joystick;
    [SerializeField] private ButtonClick _upButton;
    [SerializeField] private ButtonClick _downButton;
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _layerMask;

    private RectTransform _joystickRect;
    private RectTransform _upButtonRect;
    private RectTransform _downButtonRect;
    private Vector2 touchStartPos;
    private Vector2 touchDelta;

    public float InputH { get; private set; }
    public float InputV { get; private set; }
    public float InputMouseX { get; private set; }
    public float InputMouseY { get; private set; }
    public bool PressSpace { get; private set; }
    public bool PressCtrl { get; private set; }
    public bool MouseButtonDown { get; private set; }
    public bool MouseButtonUp { get; private set; }
    public bool MouseButtonDrag { get; private set; }

    private void Start()
    {
        Debug.Log(1);
        _joystickRect = _joystick.GetComponent<RectTransform>();
        _upButtonRect = _upButton.GetComponent<RectTransform>();
        _downButtonRect = _downButton.GetComponent<RectTransform>();
    }

    private void Update()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (IsButtonTouch(touch, _joystickRect))
            {
                HandleJoysticTouch(touch);
            }
            else if (IsButtonTouch(touch, _upButtonRect))
            {
                PressSpace = _upButton.IsDown;
            }
            else if (IsButtonTouch(touch, _downButtonRect))
            {
                PressCtrl = _downButton.IsDown;
            }
            else
            {
                HandleCameraTouch(touch);
            }
        }

        MouseButtonDrag = Input.GetMouseButton(0);
        MouseButtonDown = Input.GetMouseButtonDown(0);
        MouseButtonUp = Input.GetMouseButtonUp(0);
    }


    bool IsButtonTouch(Touch touch, RectTransform button)
    {
        Vector2 touchPosition = touch.position;
        RectTransform buttonRect = button.GetComponent<RectTransform>();

        if (RectTransformUtility.RectangleContainsScreenPoint(buttonRect, touchPosition))
        {
            return true;
        }

        return false;
    }

    private void HandleJoysticTouch(Touch touch)
    {
        InputH = _joystick.Horizontal;
        InputV = _joystick.Vertical;
    }

    private void HandleCameraTouch(Touch touch)
    {

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartPos = touch.position;
                break;

            case TouchPhase.Moved:
                touchDelta = touch.position - touchStartPos;

                InputMouseX = touchDelta.x;
                InputMouseY = touchDelta.y;

                touchStartPos = touch.position;
                break;
            default:
                InputMouseX = 0;
                InputMouseY = 0;
                break;

        }
    }
}

