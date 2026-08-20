using CombatSystem;
using SkillSystem;
using UnityEngine;

namespace Agents.Enemies.Skills
{
    /// <summary>
    /// 경고 선을 띄운 뒤 고정된 방향으로 돌진하는 공격.
    /// 경고 중에 피격되면 취소되고, 돌진이 시작된 뒤에는 끝까지 진행된다.
    /// </summary>
    public class DashAttackSkill : AbstractSkill
    {
        private enum Phase { None, Telegraph, Dash }

        [Header("경고 표시")]
        [SerializeField] private float telegraphTime = 0.6f;
        //두 경계선 사이 간격. 돌진 히트박스 폭과 같게 맞춰야 경고가 정직해진다.
        [SerializeField] private float corridorWidth = 1.6f;

        [Header("돌진")]
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDistance = 5f;

        private ITopDownMover _mover;
        private IAnimateRenderer _renderer;
        private AbstractDamageCaster _damageCaster;
        private TelegraphLines _telegraph;
        private NavModule _nav;

        private Phase _phase;
        private Transform _target;
        private Vector2 _dashDir;
        private float _phaseEndTime;
        private float _baseSpeed;
        private bool _hasHit;

        //경고 단계에서만 피격으로 끊을 수 있다. 돌진에 들어가면 확정 공격이다.
        public override bool CanInterrupt => _phase == Phase.Telegraph;

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _mover = skillModule.Owner.GetModule<ITopDownMover>();
            _renderer = skillModule.Owner.GetModule<IAnimateRenderer>();
            _nav = skillModule.Owner.GetModule<NavModule>();
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            _damageCaster?.InitCaster(skillModule.Owner);
            _telegraph = GetComponentInChildren<TelegraphLines>();
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            if (target == null) return false; //적의 공격은 타겟이 없다면 수행할 수 없다.
            if (IsUsing || NormalizedCoolTime > 0f) return false;

            return Vector2.Distance(transform.position, target.transform.position) <= SkillData.maxRange;
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);

            //경고 중에는 계속 조준을 갱신하고, 돌진이 시작되는 순간에 방향이 확정된다.
            _target = target.transform;
            AimAtTarget();

            //길찾기가 살아 있으면 매 프레임 SetMovement를 덮어써서 고정 방향 돌진이 깨진다.
            _nav?.Stop();
            _mover.StopImmediately();

            _hasHit = false;
            _phase = Phase.Telegraph;
            _phaseEndTime = Time.time + telegraphTime;

            _telegraph?.Show(2); //돌진 통로의 좌우 경계 두 줄
            UpdateTelegraphLine();
        }

        public override void OnUpdateSkill()
        {
            switch (_phase)
            {
                case Phase.Telegraph:
                    AimAtTarget(); //조준은 돌진 직전까지 플레이어를 따라간다
                    if (Time.time >= _phaseEndTime)
                    {
                        BeginDash();
                        return;
                    }
                    UpdateTelegraphLine();
                    break;

                case Phase.Dash:
                    CastDamageOnce();
                    if (Time.time >= _phaseEndTime)
                        StopSkill();
                    break;
            }
        }

        //현재 타겟 위치로 조준을 갱신한다. 타겟이 사라졌다면 마지막 방향을 유지한다.
        private void AimAtTarget()
        {
            if (_target == null) return;

            Vector2 toTarget = (Vector2)_target.position - (Vector2)transform.position;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            _dashDir = toTarget.normalized;
            _renderer.SetMovementDirection(_dashDir); //애니메이션 방향과 flipX도 같이 따라간다
        }

        private void BeginDash()
        {
            //여기서부터 _dashDir은 더 갱신되지 않는다. 돌진 방향이 확정되는 지점.
            _telegraph?.Hide();

            _phase = Phase.Dash;
            _phaseEndTime = Time.time + (dashSpeed <= 0f ? 0f : dashDistance / dashSpeed);

            _baseSpeed = _mover.MoveSpeed;
            _mover.SetMovementSpeed(dashSpeed);
            _mover.SetMovement(_dashDir);

            if (SkillData.defaultAnimation != null)
                _renderer.RenderClip(SkillData.defaultAnimation.HashValue);
        }

        //경로에서 한 번만 피해를 주고, 맞아도 멈추지 않고 그대로 지나간다.
        private void CastDamageOnce()
        {
            if (_hasHit || _damageCaster == null) return;

            float damage = _skillModule.GetBaseDamage(SkillData);
            if (_damageCaster.CastDamage(damage, _dashDir, SkillData.kbForce))
                _hasHit = true;
        }

        //통로 모양은 여기서 만든다. 다른 스킬은 같은 컴포넌트에 부챗살 같은 다른 모양을 그리면 된다.
        private void UpdateTelegraphLine()
        {
            if (_telegraph == null) return;

            Vector3 origin = transform.position;
            Vector3 tip = (Vector3)(_dashDir * dashDistance);
            //돌진 방향의 수직 벡터로 좌우로 벌려 통로의 양쪽 경계를 만든다.
            Vector3 halfWidth = new Vector3(-_dashDir.y, _dashDir.x) * (corridorWidth * 0.5f);

            _telegraph.SetLine(0, origin + halfWidth, origin + halfWidth + tip);
            _telegraph.SetLine(1, origin - halfWidth, origin - halfWidth + tip);
        }

        //정상 종료와 취소가 모두 여기로 모인다. 상태 원복은 이 한 곳에서만 한다.
        public override void CleanUpSkillData()
        {
            _telegraph?.Hide();

            //경고 단계에서 취소된 경우엔 속도를 건드린 적이 없으므로 원복할 것도 없다.
            if (_phase == Phase.Dash)
            {
                _mover.SetMovementSpeed(_baseSpeed); //원복하지 않으면 계속 빠른 상태로 남는다
                _mover.StopImmediately();
            }

            _phase = Phase.None;
            _target = null;
            _hasHit = false;
            base.CleanUpSkillData(); //여기서 _lastUsedTime이 기록되어 쿨타임이 시작된다
        }
    }
}
