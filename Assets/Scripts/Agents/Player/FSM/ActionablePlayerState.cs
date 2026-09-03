using DevLib.FsmSystem.Runtime;
using GameSystem;
using UnityEngine;

namespace Agents.Player.FSM
{
    public abstract class ActionablePlayerState : AbstractPlayerState
    {
        protected PlayerSkillModule _skillModule;

        protected ActionablePlayerState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
            _skillModule = _player.GetModule<PlayerSkillModule>();
            Debug.Assert(_skillModule != null, "SkillModule is null.");
        }

        public override void Enter()
        {
            base.Enter();
            _player.playerInput.OnAttackKeyPress += HandleAttackKey;
            _player.playerInput.OnSkillPerformed += HandleSkillkey;
            _player.playerInput.onDashKeyPress += HandleDashKey;
        }
        

        public override void Exit()
        {                                                                                                                                                                                                                                                                                                                   
            base.Exit();
            _player.playerInput.OnAttackKeyPress -= HandleAttackKey;
            _player.playerInput.OnSkillPerformed -= HandleSkillkey;
            _player.playerInput.onDashKeyPress -= HandleDashKey;

        }

        private void HandleAttackKey(bool isPressed)
        {
             if (isPressed && _skillModule.TryResolveBasicAttack(out int id))
                TryEnterSkill(id, null); //기본공격은 슬롯이 없다
        }
        
        private void HandleDashKey()
        {
            if(_skillModule.TryResolveDash(out int id) && _skillModule.TryRequestSkill(id))
                _player.ChangeState(PlayerState.DASH);
        }
        
        private void HandleSkillkey(SkillSlot slot, bool isPressed)
        {
            if (isPressed && _skillModule.TryResolveSlot(slot, out int id))
                TryEnterSkill(id, slot); //차징 스킬이 어느 키로 시전됐는지 알아야 한다
        }

        private void TryEnterSkill(int skillId, SkillSlot? inputSlot)
        {
            if(_skillModule.TryRequestSkill(skillId, inputSlot))
               _player.ChangeState(PlayerState.SKILL);

        }

    }
}