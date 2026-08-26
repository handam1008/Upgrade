using DevLib.FsmSystem.Runtime;
using DevLib.ModuleSystem;
using SkillSystem;
using UnityEngine;

namespace Agents.Player.FSM
{
    public class PlayerDashState : ActionablePlayerState
    {
        private ISkill _currentSkill;

        public override bool IsInvulnerable => true;


        public PlayerDashState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
            
        }

        public override void Enter()
        {
        
            _skillModule.UseSkill(_skillModule.RequestedSkillId);
            _currentSkill = _skillModule.CurrentSkill;
            


        }

        protected override bool OnUpdate()
        {
           
            if (_currentSkill == null || !_currentSkill.IsUsing)
            {
                bool hasInput = _player.playerInput.InputDirection.sqrMagnitude >= MOVE_THRESHOLD;
                _player.ChangeState(hasInput ? PlayerState.RUN : PlayerState.IDLE);
                return false;
            }

            _currentSkill.OnUpdateSkill();
            return true;
        }

        public override void Exit()
        {
            if (_currentSkill is { IsUsing: true })
                _currentSkill.StopSkill(); 

            _currentSkill = null;
            base.Exit();
        }
    }
}
