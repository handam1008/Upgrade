using DevLib.FsmSystem.Runtime;
using SkillSystem;
using UnityEngine;

namespace Agents.Player.FSM
{
    public class PlayerDashState : ActionablePlayerState
    {
        private ISkill _currentSkill;

        public PlayerDashState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
        }

        public override void Enter()
        {
            // 애니메이션은 DashSkill이 직접 재생하므로 base.Enter()를 부르지 않는다.
            // (입력 구독도 하지 않아 대쉬 도중 다른 행동이 끼어들지 않는다)
            _skillModule.UseSkill(_skillModule.RequestedSkillId);
            _currentSkill = _skillModule.CurrentSkill;
        }

        protected override bool OnUpdate()
        {
            // 대쉬가 끝났거나 시작에 실패했으면 상태에 갇히지 않고 빠져나간다
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
                _currentSkill.StopSkill(); // 중간에 다른 상태로 가도 속도/무적이 원복되게

            _currentSkill = null;
            base.Exit();
        }
    }
}
