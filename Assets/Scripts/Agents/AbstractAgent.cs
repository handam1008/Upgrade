using CombatSystem;
using DevLib.ModuleSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Agents
{
    public abstract class AbstractAgent : ModuleOwner, IDamageable
    {
        public UnityEvent OnHit;

        public IAnimateRenderer Renderer { get; private set; }
        public ITopDownMover Mover { get; private set; }
        public HealthComponent Health { get; private set; }
        
        public ActionDataModule ActionData { get; private set; }
        public bool IsDead { get; private set; }
        
        public Vector2 FacingDirection => Renderer?.FacingDirection ?? Vector2.zero;

        protected override void InitializeModules()
        {
            base.InitializeModules();
            Renderer = GetModule<IAnimateRenderer>();
            Mover = GetModule<ITopDownMover>();
            Health = GetModule<HealthComponent>();
            ActionData = GetModule<ActionDataModule>();
            Debug.Assert(Mover != null, "Mover != null");
            Debug.Assert(Renderer != null, "Renderer != null");
            Debug.Assert(ActionData != null, "ActionData != null");
            
        }

        protected override void AfterInitializeModules()
        {
            base.AfterInitializeModules();
            OnHit ??= new UnityEvent();
            OnHit.AddListener(HandleHit);

            if (Health != null) Health.OnDead += HandleDead;
        }

        private void OnDestroy()
        {
            OnHit?.RemoveListener(HandleHit);
            if (Health != null) Health.OnDead -= HandleDead;
        }

        //피격 반응. 기본은 아무것도 하지 않고, 필요한 에이전트만 재정의한다.
        protected virtual void HandleHit() { }

        protected virtual void HandleDead()
        {
            IsDead = true;
        }

   
        public void ApplyDamage(DamageData damageData, Vector2 hitPoint, Vector2 hitDirection, Vector2 hitNormal)
        {
            // HealthComponent는 체력이 0 이하가 된 뒤에도 맞을 때마다 OnDead를 다시 발동시킨다.
            // 광역·다단히트가 시체를 스치면 사망 처리가 중복되므로 여기서 끊는다.
            if (IsDead) return;

            if (Health == null)
            {
                Debug.LogWarning($"HealthComponent가 없어 데미지를 받을 수 없습니다: {gameObject.name}");
                return;
            }

            ActionData.HitPoint = hitPoint;
            ActionData.HitNormal = hitNormal;
            ActionData.IsLastHitCritical = damageData.IsCritical;
            ActionData.LastDealer = damageData.Dealer?.gameObject; //함정 같은 환경 피해는 딜러가 없다
            ActionData.KnockbackForce = damageData.DirectedKBForce;

            OnHit?.Invoke();
            Health.TakeDamage(damageData.DamageAmount);

        }
    }
}