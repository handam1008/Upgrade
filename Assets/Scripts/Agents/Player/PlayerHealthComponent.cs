using CombatSystem;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using GameModule.UI;
using UnityEngine;

namespace Agents.Player
{
    public class PlayerHealthComponent : HealthComponent
    {
        [SerializeField] private HealthModelSO healthModel;

        private HealthModelSO _runtimeModel;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _runtimeModel = HealthModelSO.CreateInstanceFromOriginal(healthModel);
            ServiceLocator.Register(_runtimeModel);
        }

        protected override void Start()
        {
            base.Start();
            _runtimeModel.SetHealth(CurrentHealth, MaxHealth);
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            _runtimeModel.SetHealth(CurrentHealth, MaxHealth);
        }
    }
}
