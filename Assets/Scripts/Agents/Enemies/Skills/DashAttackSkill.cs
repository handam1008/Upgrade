using CombatSystem;
using SkillSystem;
using UnityEngine;

namespace Agents.Enemies.Skills
{
  
    public class DashAttackSkill : TelegraphedAttackSkill
    {
        [SerializeField] private float corridorWidth = 1.6f;

        [Header("돌진")]
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDistance = 5f;

        [Header("벽 검사")]
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private float wallMargin = 0.4f;

        private AbstractDamageCaster _damageCaster;

        private float _dashEndTime;
        private float _baseSpeed;
        private bool _hasHit;

        protected override int TelegraphLineCount => 2; 

        protected override void OnInitialize()
        {
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            _damageCaster?.InitCaster(_skillModule.Owner);
        }

        protected override void DrawTelegraph()
        {
            if (_telegraph == null) return;

            Vector3 origin = transform.position;
            Vector3 tip = (AimDirection * GetClearDashDistance());
            Vector3 halfWidth = new Vector3(-AimDirection.y, AimDirection.x) * (corridorWidth * 0.5f);

            _telegraph.SetLine(0, origin + halfWidth, origin + halfWidth + tip);
            _telegraph.SetLine(1, origin - halfWidth, origin - halfWidth + tip);
        }

        protected override void BeginExecute()
        {
            _hasHit = false;

            float distance = GetClearDashDistance();
            _dashEndTime = Time.time + (dashSpeed <= 0f ? 0f : distance / dashSpeed);

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

        private float GetClearDashDistance()
        {
            bool previous = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = false;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, AimDirection, dashDistance, obstacleMask);
            Physics2D.queriesStartInColliders = previous;

            if (!hit) return dashDistance;

            return Mathf.Max(0f, hit.distance - wallMargin);
        }

        private void CastDamageOnce()
        {
            if (_hasHit || _damageCaster == null) return;

            float damage = _skillModule.GetBaseDamage(SkillData);
            if (_damageCaster.CastDamage(damage, AimDirection, SkillData.kbForce))
                _hasHit = true;
        }

        protected override void OnCleanUp()
        {
            if (CurrentPhase != Phase.Execute) return;

            _mover.SetMovementSpeed(_baseSpeed); 
            _mover.StopImmediately();
            _hasHit = false;
        }
    }
}
