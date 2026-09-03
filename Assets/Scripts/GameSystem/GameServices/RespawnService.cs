using System.Collections;
using Agents.Player;
using DevLib.ServiceLocator;
using Unity.Cinemachine;
using UnityEngine;

namespace GameSystem.GameServices
{
    public class RespawnService : MonoBehaviour, IRespawnService
    {
        [SerializeField] private Transform villageSpawnPoint;
        [SerializeField] private float deathDelay = 2f;

        private Transform playerTrm;
        
        
        ITowerService _towerService;
        private IPlayerRevivable _agent;

        private void Awake()
        {
            ServiceLocator.Register<IRespawnService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IRespawnService>();
        }

        private void Start()
        {
            _towerService = ServiceLocator.Get<ITowerService>();
            _agent = ServiceLocator.Get<IPlayerRevivable>();

        }
        

        public void Respawn()
        {
           StopAllCoroutines();
           StartCoroutine(RespawnCoroutine());
        }

        private IEnumerator RespawnCoroutine()
        {
            _towerService?.ExitTower();
            yield return new WaitForSecondsRealtime(deathDelay);
            Transform player = ServiceLocator.Get<IPlayerTransform>()?.Transform;
            if(player == null) yield break;
            
            Vector3 delta = villageSpawnPoint.position - player.position;
            player.position = villageSpawnPoint.position;
            CinemachineCore.OnTargetObjectWarped(player, delta);   
            _agent?.Revive();
        }
    }
}