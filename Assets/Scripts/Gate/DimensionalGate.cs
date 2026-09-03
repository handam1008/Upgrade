using DevLib.ServiceLocator;
using GameSystem;
using GameSystem.GameServices;
using Unity.Cinemachine;
using UnityEngine;

namespace Gate
{
    public class DimensionalGate : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform entryPoint;
        [SerializeField] private string message = "무한의 탑에 입장하시겠습니까?";

        private ITowerService _towerService;
        private IDialogService _dialogService;

        public string Prompt => "무한의 탑 입장";
        public bool CanInteract => _dialogService == null || !_dialogService.IsOpen;

        private void Start()
        {
            _towerService = ServiceLocator.Get<ITowerService>();
            _dialogService = ServiceLocator.Get<IDialogService>();
        }

        public void Interact()
        {
            if (_dialogService == null)
            {
                EnterTower();
                return;
            }

            _dialogService.Show(message, EnterTower);
        }

        private void EnterTower()
        {
            if (entryPoint == null || _towerService == null) return;

            Transform player = ServiceLocator.Get<IPlayerTransform>()?.Transform;
            if (player == null) return;

            Vector3 delta = entryPoint.position - player.position;
            player.position = entryPoint.position;
            CinemachineCore.OnTargetObjectWarped(player, delta);

            _towerService.StartTower();
        }
    }
}
