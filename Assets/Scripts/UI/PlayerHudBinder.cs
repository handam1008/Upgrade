using DevLib.ServiceLocator;
using UISystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class PlayerHudBinder : MonoBehaviour
    {
        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            VisualElement vitals = root.Q<VisualElement>("vitals");

            if (vitals == null) return;

            vitals.dataSource = ServiceLocator.Get<HealthModelSO>();
        }
    }
}
