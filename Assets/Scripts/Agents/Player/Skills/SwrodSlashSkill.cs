using CombatSystem;
using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using SkillSystem;
using UnityEngine;

namespace Agents.Player.Skills
{
    public class SwrodSlashSkill : AbstractSkill
    {
        [SerializeField] private SoundClipSO slashSound;

        [Tooltip("애니메이션 종료 이벤트가 없을 때 강제로 끝내는 시간 (상태에 갇히는 것 방지)")]
        [SerializeField] private float fallbackDuration = 0.4f;

        private IAnimateRenderer _renderer;
        private IAnimatorTrigger _trigger;
        private AbstractDamageCaster _damageCaster;
        private float _endTime;

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _renderer = skillModule.Owner.GetModule<IAnimateRenderer>();
            _trigger = skillModule.Owner.GetModule<IAnimatorTrigger>();
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            _damageCaster?.InitCaster(skillModule.Owner);
            Debug.Assert(_renderer != null , "Renderer is null");
            Debug.Assert(_trigger != null, "Trigger is null");
            Debug.Assert(_damageCaster != null, "DamageCaster is null");
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            return IsUsing == false;
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            _trigger.OnAnimationEnd += HandleAnimationEnd;
            _trigger.OnDamageCast += HandleDamageCast;
            _renderer.RenderClip(SkillData.defaultAnimation.HashValue);
            ServiceLocator.Get<IAudioService>()?.PlaySfx(slashSound);
            _endTime = Time.time + fallbackDuration;
        }

        // 클립에 종료 이벤트가 없어도 스킬 상태에 갇히지 않게 시간으로도 끝낸다
        public override void OnUpdateSkill()
        {
            if (Time.time >= _endTime)
                StopSkill();
        }

        private void HandleAnimationEnd()
        {
            StopSkill();
        }

        //클립에 박아둔 DamageCastTrigger 시점에 호출된다.
        private void HandleDamageCast()
        {
            if (_damageCaster == null) return;

            //바라보는 방향은 FSM이 시전 직전에 맞춰준다. 히트박스를 그쪽으로 반칸 밀어서 판정한다.
            Vector2 direction = _renderer.FacingDirection;
            _damageCaster.transform.position =
                transform.position + (Vector3)direction * (SkillData.maxRange * 0.5f);

            float damage = _skillModule.GetBaseDamage(SkillData);
            _damageCaster.CastDamage(damage, direction, SkillData.kbForce);
        }

        public override void CleanUpSkillData()
        {
            _trigger.OnAnimationEnd -= HandleAnimationEnd;
            _trigger.OnDamageCast -= HandleDamageCast;
            base.CleanUpSkillData();
        }
    }
}