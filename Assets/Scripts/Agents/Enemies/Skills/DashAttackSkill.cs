using CombatSystem;
using SkillSystem;
using UnityEngine;

namespace Agents.Enemies.Skills
{
    /// <summary>
    /// 경고 선을 띄운 뒤 고정된 방향으로 돌진하는 공격.
    /// 경고 중에 피격되면 취소되고, 돌진이 시작된 뒤에는 끝까지 진행된다.
    /// </summary>
    public class DashAttackSkill : TelegraphedAttackSkill
    {
        //두 경계선 사이 간격. 돌진 히트박스 폭과 같게 맞춰야 경고가 정직해진다.
        [SerializeField] private float corridorWidth = 1.6f;

        [Header("돌진")]
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDistance = 5f;

        private AbstractDamageCaster _damageCaster;

        private float _dashEndTime;
        private float _baseSpeed;
        private bool _hasHit;

        protected override int TelegraphLineCount => 2; //돌진 통로의 좌우 경계

        protected override void OnInitialize()
        {
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            _damageCaster?.InitCaster(_skillModule.Owner);
        }

        //통로 모양은 여기서 만든다. 다른 스킬은 같은 컴포넌트에 부챗살 같은 다른 모양을 그리면 된다.
        protected override void DrawTelegraph()
        {
            if (_telegraph == null) return;

            Vector3 origin = transform.position;
            Vector3 tip = (AimDirection * dashDistance);
            //돌진 방향의 수직 벡터로 좌우로 벌려 통로의 양쪽 경계를 만든다.
            Vector3 halfWidth = new Vector3(-AimDirection.y, AimDirection.x) * (corridorWidth * 0.5f);

            _telegraph.SetLine(0, origin + halfWidth, origin + halfWidth + tip);
            _telegraph.SetLine(1, origin - halfWidth, origin - halfWidth + tip);
        }

        protected override void BeginExecute()
        {
            _hasHit = false;
            _dashEndTime = Time.time + (dashSpeed <= 0f ? 0f : dashDistance / dashSpeed);

            _baseSpeed = _mover.MoveSpeed;
            _mover.SetMovementSpeed(dashSpeed);
            _mover.SetMovement(AimDirection);
        }

        protected override void OnExecuteUpdate()
        {
            CastDamageOnce();

            if (Time.time >= _dashEndTime)
                StopSkill();
        }

        //경로에서 한 번만 피해를 주고, 맞아도 멈추지 않고 그대로 지나간다.
        private void CastDamageOnce()
        {
            if (_hasHit || _damageCaster == null) return;

            float damage = _skillModule.GetBaseDamage(SkillData);
            if (_damageCaster.CastDamage(damage, AimDirection, SkillData.kbForce))
                _hasHit = true;
        }

        protected override void OnCleanUp()
        {
            //경고 단계에서 취소된 경우엔 속도를 건드린 적이 없으므로 원복할 것도 없다.
            if (CurrentPhase != Phase.Execute) return;

            _mover.SetMovementSpeed(_baseSpeed); //원복하지 않으면 계속 빠른 상태로 남는다
            _mover.StopImmediately();
            _hasHit = false;
        }
    }
}
