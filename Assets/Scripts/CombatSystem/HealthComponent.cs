using System;
using DevLib.ModuleSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace CombatSystem
{
    public class HealthComponent : MonoModule
    {
        [field: SerializeField] public float MaxHealth { get; private set; } = 50f;

        public delegate void HealthChange(float before, float current, float max);
        public event HealthChange OnHealthChanged;
        public event Action OnDead;
        
        [SerializeField] private float currentHealth;

        public float CurrentHealth
        {
            get => currentHealth;
            set
            {
                float before = currentHealth;
                currentHealth = Mathf.Clamp(value, 0f, MaxHealth);
                if(!Mathf.Approximately(before, currentHealth))
                    OnHealthChanged?.Invoke(before, currentHealth, MaxHealth);
            }
        }

        private void Start()
        {
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            if(CurrentHealth <= 0)
                OnDead?.Invoke();
        }
    }
}