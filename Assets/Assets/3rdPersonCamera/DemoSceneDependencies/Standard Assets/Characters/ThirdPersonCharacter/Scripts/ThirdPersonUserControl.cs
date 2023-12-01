using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        private ThirdPersonCharacter m_Character; 
        private Transform m_Cam;                  
        private Vector3 m_CamForward;             
        private Vector3 m_Move;
        private bool m_Jump;
        private FloatingJoystick _joystick;
        private bool _isSelect;
        private Button _jumpButton;

        
        private void Start()
        {
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }

            m_Character = GetComponent<ThirdPersonCharacter>();
        }

        public void Initialize(FloatingJoystick joystic, Button jumpButton)
        {
            _jumpButton = jumpButton;
            _jumpButton.onClick.AddListener(Jump);
            _joystick = joystic;
            _isSelect = true;
        }

        public void Disable()
        {
            _jumpButton.onClick.RemoveListener(Jump);
            _joystick = null;
            _isSelect = false;
        }

        private void Jump()
        {
            if (!m_Jump)
            {
                m_Jump = true;
            }
        }

        private void FixedUpdate()
        {
            if (_isSelect)
            {
                float h = _joystick.Horizontal;
                float v = _joystick.Vertical;
                bool crouch = Input.GetKey(KeyCode.C);

                if (m_Cam != null)
                {
                    m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                    m_Move = v * m_CamForward + h * m_Cam.right;
                }
                else
                {
                    m_Move = v * Vector3.forward + h * Vector3.right;
                }

                m_Character.Move(m_Move, crouch, m_Jump);
                m_Jump = false;
            }

        }
    }
}
