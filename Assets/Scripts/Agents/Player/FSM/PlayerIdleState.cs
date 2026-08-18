using DevLib.FsmSystem.Runtime;
using UnityEngine;

namespace Agents.Player.FSM
{
    public class PlayerIdleState : ActionablePlayerState
    {
        public PlayerIdleState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
        }

        protected override bool OnUpdate()
        {
            Vector2 inputDirection = _player.playerInput.InputDirection;

            if (inputDirection.sqrMagnitude >= MOVE_THRESHOLD)
            {
                _player.ChangeState(PlayerState.RUN);
                return false;
            }
            return true;
        }
    }
}