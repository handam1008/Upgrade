using DevLib.ServiceLocator;
using GameModule.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class PlayerHudBinder : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<UIDocument>().rootVisualElement.dataSource = ServiceLocator.Get<HealthModelSO>();
        }
    }
}
