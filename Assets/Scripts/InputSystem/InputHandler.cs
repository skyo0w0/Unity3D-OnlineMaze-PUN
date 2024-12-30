using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem
{
    public class InputHandler : MonoBehaviour
    {
        #region Input Events
        #region Movement Events
        public event Action OnRight;
        public event Action OnLeft;
        public event Action OnHorizontalReleased;
        public event Action OnUp;
        public event Action OnDown;
        public event Action OnVerticalReleased;
        #endregion
    
        #region Input Attributes
        private Input _input;
        private InputAction _horizontalMovement;
        private InputAction _verticalMovement;
        #endregion
        #endregion
        #region Unity Lifecycle
        private void Awake()
        {
            Debug.Log("Input Handler: Awake");
            _input = new Input();
            _horizontalMovement = _input.Player.HorizontalMovement;
            _verticalMovement = _input.Player.VerticalMovement;
        }

        private void OnEnable()
        {
            _horizontalMovement.performed += HandleHorizontalMovement;
            _horizontalMovement.canceled += HandleHorizontalMovement;
            _verticalMovement.performed += HandleVerticalMovement;
            _verticalMovement.canceled += HandleVerticalMovement;
        
            _input.Enable();
        }

        private void OnDisable()
        {
            _horizontalMovement.performed -= HandleHorizontalMovement;
            _horizontalMovement.canceled -= HandleHorizontalMovement;
            _verticalMovement.performed -= HandleVerticalMovement;
            _verticalMovement.canceled -= HandleVerticalMovement;
        
            _input.Disable();
        }
        #endregion
    
        #region Handle Methods
        private void HandleHorizontalMovement(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<float>();
            if (direction > 0)
            {
                OnRight?.Invoke();
            }
            else if (direction < 0)
            {
                OnLeft?.Invoke();
            }
            else
            {
                OnHorizontalReleased?.Invoke();
            }
        }

        private void HandleVerticalMovement(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<float>();
            if (direction > 0)
            {
                OnUp?.Invoke();
            }
            else if (direction < 0)
            {
                OnDown?.Invoke();
                Debug.Log("ONDOWN");
            }
            else
            {
                OnVerticalReleased?.Invoke();
            }
        }
    
        #endregion
    }
}


