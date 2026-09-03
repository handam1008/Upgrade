using System;
using DevLib.ServiceLocator;
using GameSystem.GameServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ExpHudBinder : MonoBehaviour
    {
        ILevelService  _levelService;
        VisualElement _expFill;
        Label _expLabel;
        
        
        

        private void Start()
        {
            VisualElement root = this.GetComponent<UIDocument>().rootVisualElement;
            _expFill = root.Q<VisualElement>("exp-fill");
            _expLabel = root.Q<Label>("exp-label");
            
            _levelService = ServiceLocator.Get<ILevelService>();
            _levelService.OnLevelChanged += HandleLevelChange;
            _levelService.OnLevelUp += HandleLevelUpChange;
            _expFill.style.width = Length.Percent(0);
        }

        private void HandleLevelUpChange(int level )
        {
            _expFill.style.width = Length.Percent(0);
            _expLabel.text = "Level " + level;
            
        }

        private void HandleLevelChange(float ratio)
        {
            _expFill.style.width = Length.Percent(ratio * 100);
        }

        private void OnDestroy()
        {
            _levelService.OnLevelChanged -= HandleLevelChange;
            _levelService.OnLevelUp -= HandleLevelUpChange;
        }
    }
}