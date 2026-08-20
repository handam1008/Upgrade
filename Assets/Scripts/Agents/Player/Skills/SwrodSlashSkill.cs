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

        [Header("전진")]
        [Tooltip("휘두르면서 앞으로 밀리는 속도. 기본 이동속도보다 빨라야 전진이 느껴진다")]
        [SerializeField] private float lungeSpeed = 12f;
        [SerializeField] private float lungeDuration = 0.1f;

        private IAnimateRenderer _renderer;
        private IAnimatorTrigger _trigger;
        private ITopDownMover _mover;
        private AbstractDamageCaster _damageCaster;
        private float _endTime;

        private float _lungeEndTime;
        private float _baseSpeed;
        private bool _isLunging;

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _renderer = skillModule.Owner.GetModule<IAnimateRenderer>();
            _trigger = skillModule.Owner.GetModule<IAnimatorTrigger>();
            _mover = skillModule.Owner.GetModule<ITopDownMover>();
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

            StartLunge();
        }

        //바라보는 방향으로 짧게 밀어준다. 사거리가 늘어나면서 타격감도 살아난다.
        private void StartLunge()
        {
            if (_mover == null || lungeDuration <= 0f) return;

            Vector2 direction = _renderer.FacingDirection;
            if (direction.sqrMagnitude < 0.01f) return;

            _baseSpeed = _mover.MoveSpeed;
            _mover.SetMovementSpeed(lungeSpeed);
            _mover.SetMovement(direction);

            _isLunging = true;
            _lungeEndTime = Time.time + lungeDuration;
        }

        private void StopLunge()
        {
            if (!_isLunging) return;

            _isLunging = false;
            _mover.SetMovementSpeed(_baseSpeed); //원복하지 않으면 계속 빠른 상태로 남는다
            _mover.StopImmediately();
        }

        // 클립에 종료 이벤트가 없어도 스킬 상태에 갇히지 않게 시간으로도 끝낸다
        public override void OnUpdateSkill()
        {
            //전진은 스킬보다 먼저 끝난다. 휘두르는 동안 계속 미끄러지지 않게.
            if (_isLunging && Time.time >= _lungeEndTime)
                StopLunge();

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
            StopLunge(); //중간에 캔슬되어도 속도가 빠른 채로 남지 않게
            _trigger.OnAnimationEnd -= HandleAnimationEnd;
            _trigger.OnDamageCast -= HandleDamageCast;
            base.CleanUpSkillData();
        }
    }
}