using DevLib.FsmSystem.Runtime;
using DevLib.ServiceLocator;
using GameSystem.GameServices;
using UnityEngine;

namespace Agents.Player.FSM
{
    public class PlayerDeadState : AbstractPlayerState
    {
        IRespawnService _respawnService;
        
        public PlayerDeadState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
        }
        

        public override void Enter()
        {
            base.Enter();
            _player.gameObject.layer = LayerMask.NameToLayer("Dead");
            _respawnService = ServiceLocator.Get<IRespawnService>();
            _respawnService.Respawn();
            
        }

        public override void Exit()
        {
            base.Exit();
            _player.gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }
}