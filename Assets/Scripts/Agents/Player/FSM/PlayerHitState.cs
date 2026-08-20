using System.Collections;
using DevLib.FsmSystem.Runtime;
using UnityEngine;

namespace Agents.Player.FSM
{
    public class PlayerHitState : AbstractPlayerState
    {

        private float hitTime = 0.5f;
        private float time;
        public PlayerHitState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
        }

        public override void Enter()
        {
            base.Enter();
            time = 0;
        }

        protected override bool OnUpdate()
        {
            time += Time.deltaTime;
            if (time > hitTime)
            {
                bool hasInput =  _player.playerInput.InputDirection.sqrMagnitude >= MOVE_THRESHOLD;
                _player.ChangeState(hasInput ? PlayerState.RUN : PlayerState.IDLE);
                return false;
            }

            return true;

        }

        
        
    }
}