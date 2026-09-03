using DevLib.ServiceLocator;
using GameSystem.GameServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class TowerHudBinder : MonoBehaviour
    {
        private const string FloorPopClass = "tower-floor--pop";
        private const string CountPopClass = "tower-enemy-count--pop";
        private const long PopHoldMs = 60;

        [SerializeField] private int wavesPerFloor = 3;

        private ITowerService _towerService;
        private VisualElement _root;
        private Label _floor;
        private Label _enemy;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _floor = _root.Q<Label>("floor-label");
            _enemy = _root.Q<Label>("enemy-count");

            if (_floor == null || _enemy == null) return;

            SetVisible(false);

            _towerService = ServiceLocator.Get<ITowerService>();
            if (_towerService == null) return;

            _towerService.OnFloorChanged += HandleFloorChange;
            _towerService.OnAliveChanged += HandleAliveChange;
            _towerService.OnTowerEnded += HandleTowerEnd;
        }

        private void OnDestroy()
        {
            if (_towerService == null) return;

            _towerService.OnFloorChanged -= HandleFloorChange;
            _towerService.OnAliveChanged -= HandleAliveChange;
            _towerService.OnTowerEnded -= HandleTowerEnd;
        }

        private void HandleFloorChange(int wave)
        {
            SetVisible(true);

            int floor = (wave - 1) / wavesPerFloor + 1;
            int waveInFloor = (wave - 1) % wavesPerFloor + 1;

            _floor.text = $"{floor}층  {waveInFloor}/{wavesPerFloor}";
            Pop(_floor, FloorPopClass);
        }

        private void HandleAliveChange(int alive, int total)
        {
            _enemy.text = $"{alive} / {total}";
            Pop(_enemy, CountPopClass);
        }

        private void HandleTowerEnd()
        {
            SetVisible(false);
        }

        private void Pop(VisualElement element, string popClass)
        {
            element.RemoveFromClassList(popClass);
            element.AddToClassList(popClass);
            element.schedule.Execute(() => element.RemoveFromClassList(popClass)).StartingIn(PopHoldMs);
        }

        private void SetVisible(bool visible)
        {
            _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
