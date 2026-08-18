using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameSystem
{
    public enum SkillSlot
    {
        Q = 0, E = 1 , R = 2 , F = 3
    }
    [CreateAssetMenu(fileName = "PlayerInput", menuName = "System/PlayerInput", order = 0)]
    public class PlayerInputSO : ScriptableObject, Controls.IPlayerActions
    {
        private Controls _controls;
        
        public Vector2 InputDirection {get; private set;}

        public event Action<bool> OnAttackKeyPress;
        public event Action onInteractKeyPress;
        public event Action onDashKeyPress;
        public event Action<SkillSlot, bool> OnSkillPerformed;

        private Vector2 _mousePosition;
        private Camera _mainCamera;

        public void SetEnable()
        {
            Clear();

            bool created = false;
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
                created = true;
            }
            _controls.Player.Enable();
            
        }

        private void Clear()
        {
            OnAttackKeyPress = null;
            OnSkillPerformed = null;
            onInteractKeyPress = null;
            onDashKeyPress = null;
        }

        public void SetDisable() => _controls?.Player.Disable();

        public void OnMove(InputAction.CallbackContext context)
        {
            InputDirection = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if(context.performed)
                OnAttackKeyPress?.Invoke(true);
            if(context.canceled)
                OnAttackKeyPress?.Invoke(false);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if(context.performed)
                onInteractKeyPress?.Invoke();
            
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if(context.performed)
                onDashKeyPress?.Invoke();
        }

        public void OnPointer(InputAction.CallbackContext context)
        {
            _mousePosition = context.ReadValue<Vector2>();
        }

        public Vector3 GetWorldMousePosition()
        {
            if(_mainCamera == null)
                _mainCamera = Camera.main;
            
            Vector3 worldPosition = _mainCamera!.ScreenToWorldPoint(_mousePosition);
            worldPosition.z = 0;
            return worldPosition;
        }
        
        private void HandleSkill(InputAction.CallbackContext context, SkillSlot slot)
        {
            if(context.performed)
                OnSkillPerformed?.Invoke(slot, true);
            if(context.canceled)
                OnSkillPerformed?.Invoke(slot, false);
        }

        public void OnSkill0(InputAction.CallbackContext context)
            => HandleSkill(context, SkillSlot.Q);

        public void OnSkill1(InputAction.CallbackContext context)
            => HandleSkill(context, SkillSlot.E);

       
    }
}