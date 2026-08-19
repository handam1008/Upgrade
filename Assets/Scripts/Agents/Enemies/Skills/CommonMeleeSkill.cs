using CombatSystem;
using SkillSystem;
using UnityEngine;

namespace Agents.Enemies.Skills
{
    public class CommonMeleeSkill : AbstractSkill
    {
        private IAnimateRenderer _renderer;
        private IAnimatorTrigger _trigger;
        private ITopDownMover _mover;
        private AbstractDamageCaster _damageCaster;

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _renderer = skillModule.Owner.GetModule<IAnimateRenderer>();
            _trigger = skillModule.Owner.GetModule<IAnimatorTrigger>();
            _mover = skillModule.Owner.GetModule<ITopDownMover>();
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            _damageCaster?.InitCaster(skillModule.Owner);
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            if (target == null) return false; //적의 공격은 타겟이 없다면 수행할 수 없다.
            if (IsUsing || NormalizedCoolTime > 0f) return false;

            //조건이 충족한다면 사거리만 체크하면 됨.
            return Vector2.Distance(transform.position, target.transform.position) <= SkillData.maxRange;
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            _mover.StopImmediately();

            if (target != null && SkillData.directionType == DirectionType.Body)
            {
                Vector2 direction = target.transform.position - transform.position;
                _renderer.SetMovementDirection(direction.normalized);
            }

            _trigger.OnAnimationEnd -= HandleSkillAnimationEnd;
            _trigger.OnAnimationEnd += HandleSkillAnimationEnd;

            _trigger.OnDamageCast -= HandleDamageCast;
            _trigger.OnDamageCast += HandleDamageCast;
            
            if (SkillData.defaultAnimation != null)
                _renderer.RenderClip(SkillData.defaultAnimation.HashValue);
        }

        private void HandleSkillAnimationEnd() => CleanUpSkillData();

        private void HandleDamageCast()
        {
            Vector2 direction = _renderer.FacingDirection;
            _damageCaster.transform.position = transform.position + (Vector3)direction * SkillData.maxRange * 0.5f;
            float damage = _skillModule.GetBaseDamage(SkillData);

            bool hit = _damageCaster.CastDamage(damage, direction, SkillData.kbForce);
            //이제 여기서 hit를 이용해서 추가적인 impact나 sound등을 재생할 수 있다.
        }

        public override void CleanUpSkillData()
        {
            base.CleanUpSkillData();
            _trigger.OnAnimationEnd -= HandleSkillAnimationEnd;
            _trigger.OnDamageCast -= HandleDamageCast;
        }
    }
}