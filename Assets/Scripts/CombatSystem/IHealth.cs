using System;
using UnityEngine;

namespace CombatSystem
{
    public delegate void HealthChange(float before, float current, float max);
    
    
    public interface IHealth
    {
        public float CurrentHealth {get;}
        public float MaxHealth {get;}
        event  HealthChange OnHealthChange;
    }
}