using DevLib.FsmSystem.Runtime;
using GameSystem;
using SkillSystem;
using UnityEngine;

namespace Agents.Player.FSM
{
    public class PlayerSkillState : AbstractPlayerState
    {
        private PlayerSkillModule _skillModule;
        private ISkill _currentSkill;
        private bool _isSkillEnd;
        private SkillSlot? _castInputSlot;

        public PlayerSkillState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
            _skillModule = _player.GetModule<PlayerSkillModule>();
        }

        public override void Enter()
        {
            //base.Enter(); 애니메이션은 스킬마다 다르기 때문에 여기서는 Enter를 수행안한다.
            _isSkillEnd = false;
            _skillModule.OnSkillEnd += HandleSkillEnd;

            _castInputSlot = _skillModule.RequestedInputSlot;

            ApplyAimFacing(_skillModule.RequestedSkillId);

            //사전에 CanUseSkill은 처리되어있을 테니 그냥 수행하면 된다.
            _skillModule.UseSkill(_skillModule.RequestedSkillId);
            _currentSkill = _skillModule.CurrentSkill; //현재 스킬을 가져오고

            //캔슬 판정 용으로 입력을 가져와야 한다.
            _player.playerInput.OnAttackKeyPress += HandleAttackDuringSkill;
            _player.playerInput.OnSkillPerformed += HandleSkillDuringSkill;
        }

        protected override bool OnUpdate()
        {
            _currentSkill?.OnUpdateSkill();

            //시전에 실패했거나(스킬 없음) 끝났으면 상태에 갇히지 않게 빠져나간다
            if (_isSkillEnd || _currentSkill == null)
            {
                _player.ChangeState(PlayerState.IDLE);
                return false;
            }

            return true;
        }

        public override void Exit()
        {
            _player.playerInput.OnAttackKeyPress -= HandleAttackDuringSkill;
            _player.playerInput.OnSkillPerformed -= HandleSkillDuringSkill;

            _skillModule.OnSkillEnd -= HandleSkillEnd;

            //현재 스킬이 캔슬되어 나가진다면 현재스킬을 정리해라.
            if (_currentSkill is { IsUsing: true })
                _currentSkill.StopSkill();

            _currentSkill = null;
            base.Exit();
        }

        private void HandleSkillEnd(int skillId) => _isSkillEnd = true;

        private void ApplyAimFacing(int skillId)
        {
            SkillDataSO skillData = _skillModule.GetSkillData(skillId);
            if (skillData == null || skillData.directionType != DirectionType.Pointer) return;

            Vector3 mouseWorldPosition = _player.playerInput.GetWorldMousePosition();
            Vector2 direction = ((Vector2)(mouseWorldPosition - _player.transform.position)).normalized;
            _player.Renderer.SetMovementDirection(direction); //바라보는 방향 정렬
        }

        #region 시전 중 입력 처리 핸들러

        private void HandleAttackDuringSkill(bool isPressed)
        {
            if (!isPressed) //키가 떼진거니까 차징 종료
            {
                if (_castInputSlot == null)
                    _currentSkill?.ReleaseInput();
                return;
            }

            if (_skillModule.TryResolveBasicAttack(out int id))
                TryCancelInto(id, null);
        }

        private void HandleSkillDuringSkill(SkillSlot slot, bool isPressed)
        {
            if (!isPressed) //키가 떼진거니까 차징 종료
            {
                if (_castInputSlot == slot)
                    _currentSkill?.ReleaseInput();
                return;
            }

            if (_skillModule.TryResolveSlot(slot, out int id))
                TryCancelInto(id, slot);
        }

        private void TryCancelInto(int skillId, SkillSlot? inputSlot)
        {
            if (_currentSkill is { CanInterrupt: true } && _skillModule.TryRequestSkill(skillId, inputSlot))
                _player.ChangeState(PlayerState.SKILL);
        }

        #endregion
    }
}
