using DevLib.FsmSystem.Runtime;
using UnityEngine;

namespace Agents.Player.FSM
{
    public class PlayerRunState : ActionablePlayerState
    {
        public PlayerRunState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
        }
        
        protected override bool OnUpdate()
        {
            Debug.Log("런");
            Vector2 inputDirection = _player.playerInput.InputDirection;
            if (inputDirection.sqrMagnitude < MOVE_THRESHOLD)
            {
                
                _player.ChangeState(PlayerState.IDLE);
                return false;
            }
            _player.Mover.SetMovement(inputDirection);
            _player.Renderer.SetMovementDirection(inputDirection);

            return true;
        }

        public override void Exit()
        {
            _player.Mover.StopImmediately();
            base.Exit();
        }
    }
}