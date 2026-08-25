using CombatSystem;
using GameModule.UI;
using UnityEngine;

namespace Agents.Player
{
    public class PlayerHealthComponent : HealthComponent
    {
        [SerializeField] private HealthModelSO healthModel;

        protected override void Start()
        {
            base.Start();
            healthModel.SetHealth(CurrentHealth, MaxHealth);
            
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            healthModel.SetHealth(CurrentHealth, MaxHealth);
        }
    }
}