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
        
        public virtual bool IsGuard => false; 
        
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

       
        protected virtual void HandleHit() { }

        protected virtual void HandleDead()
        {
            IsDead = true;
        }

        public virtual void Revive()
        {
            IsDead = false;
            Health.CurrentHealth = Health.MaxHealth;
        }

   
        public void ApplyDamage(DamageData damageData, Vector2 hitPoint, Vector2 hitDirection, Vector2 hitNormal)
        {
           
            if (IsDead) return;
            if (IsGuard) return;

            if (Health == null)
            {
                Debug.LogWarning($"HealthComponent가 없어 데미지를 받을 수 없습니다: {gameObject.name}");
                return;
            }

            ActionData.HitPoint = hitPoint;
            ActionData.HitNormal = hitNormal;
            ActionData.IsLastHitCritical = damageData.IsCritical;
            ActionData.LastDealer = damageData.Dealer?.gameObject; 
            ActionData.KnockbackForce = damageData.DirectedKBForce;

            OnHit?.Invoke();
            Health.TakeDamage(damageData.DamageAmount);

        }
    }
}