using System;
using CombatSystem;
using DevLib.ServiceLocator;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class PlayerHudBinder : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
    
        
        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var hpFill = root.Q<VisualElement>("hp-fill");
            var hpLabel = root.Q<Label>("hp-label");
            
            
            
            
        }
    }
}