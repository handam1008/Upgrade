using SkillSystem;
using UnityEngine;

namespace Agents.Player.Skills
{
    public class DashSkill : AbstractSkill
    {
        [SerializeField] private float dashSpeed = 18f;
        [SerializeField] private float dashDuration = 0.18f;

        private ITopDownMover _mover;
        private IAnimateRenderer _renderer;

        private float _endTime;
        private float _baseSpeed;
        
        public bool IsInvincible {get; private set;}

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _mover = skillModule.Owner.GetModule<ITopDownMover>();
            _renderer = skillModule.Owner.GetModule<IAnimateRenderer>();
            Debug.Assert(_mover != null, "Mover is null");
            Debug.Assert(_renderer != null, "Renderer is null");
        }


        public override bool CanUseSkill(GameObject target = null)
            => !IsUsing && Time.time - _lastUsedTime >= SkillData.cooldownTime;

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            
            Vector2 dir = _renderer.FacingDirection;

            _baseSpeed = _mover.MoveSpeed;
            _endTime = Time.time + dashDuration;
            IsInvincible = true;

            _mover.SetMovementSpeed(dashSpeed);
            _mover.SetMovement(dir);
            _renderer.RenderClip(SkillData.defaultAnimation.HashValue);
        }

        //대쉬 중에는 방향을 바꾸지 않고 시간만 센다
        public override void OnUpdateSkill()
        {
            if (Time.time >= _endTime)
                StopSkill();
        }

        public override void CleanUpSkillData()
        {
            _mover.SetMovementSpeed(_baseSpeed); //원복하지 않으면 계속 빠른 상태로 남는다
            _mover.StopImmediately();
            IsInvincible = false;
            base.CleanUpSkillData(); //여기서 _lastUsedTime이 기록되어 쿨타임이 시작된다
        }
    }
}