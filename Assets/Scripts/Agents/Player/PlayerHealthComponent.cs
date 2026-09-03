using System;
using CombatSystem;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using UISystem;
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
            OnHealthChange += HandleHealthChange;
        }

        private void HandleHealthChange(float before, float current, float max)
        {
            _runtimeModel.SetHealth(current, max);
        }

        protected override void Start()
        {
            base.Start();
            _runtimeModel.SetHealth(CurrentHealth, MaxHealth);
        }

        private void OnDestroy()
        {
            OnHealthChange -= HandleHealthChange;
        }
    }
}
