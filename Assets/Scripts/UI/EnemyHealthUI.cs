using System;
using CombatSystem;
using DevLib.ModuleSystem;
using UnityEngine;

namespace UI
{
    public class EnemyHealthUI : MonoModule
    {
        
        [SerializeField] private Transform hpUI;
       
        private IHealth health;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            health = owner.GetModule<IHealth>();
            health.OnHealthChange += HandleHpChange;
            HandleHpChange(health.CurrentHealth,health.CurrentHealth,health.MaxHealth);
        }


        private void OnDestroy()
       {
           if(health != null) health.OnHealthChange -= HandleHpChange;
       }

       private void HandleHpChange(float before, float current, float max)
       {
           hpUI.localScale = new Vector3(current / max, 1f, 1f);
       }
    }
}