using System;
using DevLib.ModuleSystem;
using UnityEngine;

namespace CombatSystem
{
    public class HealthComponent : MonoModule, IHealth
    {
        [field: SerializeField] public float MaxHealth { get; private set; } = 50f;
        public event HealthChange OnHealthChange;


        public event Action OnDead;
        
        [SerializeField] private float currentHealth;

        public float CurrentHealth
        {
            get => currentHealth;
            set
            {
                float before = currentHealth;
                currentHealth = Mathf.Clamp(value, 0f, MaxHealth);
                if (!Mathf.Approximately(before, currentHealth))
                {
                    OnHealthChange?.Invoke(before, currentHealth, MaxHealth);
                }
            }
        }

        protected virtual void Start()
        {
            CurrentHealth = MaxHealth;
        }

        public virtual void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            if(CurrentHealth <= 0)
                OnDead?.Invoke();
        }
        
        public void SetMaxHealth(float value)
        {
            MaxHealth = Mathf.Max(1f, value);
            OnHealthChange?.Invoke(currentHealth, currentHealth, MaxHealth);
        }
    }
}