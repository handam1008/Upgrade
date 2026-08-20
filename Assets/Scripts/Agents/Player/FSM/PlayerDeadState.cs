using DevLib.FsmSystem.Runtime;
using UnityEngine;

namespace Agents.Player.FSM
{
    public class PlayerDeadState : AbstractPlayerState
    {
        public PlayerDeadState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _player.gameObject.layer = LayerMask.NameToLayer("Dead");
        }
    }
}