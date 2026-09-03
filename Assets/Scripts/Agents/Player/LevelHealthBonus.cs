using CombatSystem;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using GameSystem.GameServices;
using UnityEngine;

namespace Agents.Player
{
    public class LevelHealthBonus : MonoModule
    {
        [SerializeField] private float healthPerLevel = 10f;
        
        private HealthComponent _health;
        private ILevelService _levelService;
        private float _baseMaxHealth;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _health = owner.GetModule<HealthComponent>();
        }

        private void Start()
        {
            _baseMaxHealth = _health.MaxHealth;
            _levelService = ServiceLocator.Get<ILevelService>();
            _levelService.OnLevelUp += HandleLevelUp;
            HandleLevelUp(_levelService.Level);
        }

        private void OnDestroy()
        {
            if (_levelService == null) return;
            
            _levelService.OnLevelUp -= HandleLevelUp;
        }

        private void HandleLevelUp(int level)
        {
            float newMax = _baseMaxHealth + healthPerLevel  * (level - 1 );
            _health.SetMaxHealth(newMax);
        }
    }
}