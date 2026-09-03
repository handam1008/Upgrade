using System;
using System.Collections;
using System.Collections.Generic;
using Agents.Enemies;
using DevLib.ServiceLocator;
using UnityEngine;

namespace GameSystem.GameServices
{
    public class TowerControllerService : MonoBehaviour, ITowerService
    {
        [SerializeField] private TowerConfigSO config;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private Transform player;
        [SerializeField] private float floorDelay = 1.5f;
        [SerializeField] private float entryDelay = 2f;

        private readonly List<AbstractEnemy> _spawnedEnemies = new();
        public int CurrentFloor { get; private set; }
        public int AliveCount { get; private set; }
        public int TotalCount { get; private set; }

        
        public event Action<int> OnFloorChanged;
        public event Action<int, int> OnAliveChanged;
        public event Action OnTowerEnded;

        private void Awake()
        {
            ServiceLocator.Register<ITowerService>(this);
        }

        private void Start()
        {
            Debug.Assert(config != null, "TowerConfigSO가 연결되지 않았습니다.");
            Debug.Assert(spawner != null, "EnemySpawner가 연결되지 않았습니다.");

        }
        
        
        private void OnDestroy()
        {
            ServiceLocator.UnRegister<ITowerService>();
            ClearEnemies();
        }
        
        public void StartTower()
        {
            StopAllCoroutines();
            ClearEnemies();
            StartCoroutine(StartTowerRoutine());
        }
        
        public void ExitTower()
        {
            StopAllCoroutines();
            ClearEnemies();
            OnTowerEnded?.Invoke();
        }

        private IEnumerator StartTowerRoutine()
        {
            yield return new WaitForSeconds(entryDelay);
            StartFloor(1);
        }

        private void StartFloor(int floor)
        {
            ClearEnemies();
            CurrentFloor = floor;
        
            OnFloorChanged?.Invoke(CurrentFloor);

            FloorSetup setup = config.GetFloor(floor);
            if (setup.enemies == null) return;

            float healthMultiplier = config.GetHealthMultiplier(floor);
            Vector3 avoidPosition = player != null ? player.position : Vector3.zero;

            foreach (EnemySpawn spawn in setup.enemies)
            {
                for (int i = 0; i < spawn.count; i++)
                {
                    AbstractEnemy enemy = spawner.Spawn(spawn.prefab, healthMultiplier, avoidPosition);
                    if (enemy == null) continue;

                    enemy.Health.OnDead += HandleEnemyDead;
                    _spawnedEnemies.Add(enemy);
                    AliveCount++;
                }
            }
            TotalCount = AliveCount;
            
            OnAliveChanged?.Invoke(AliveCount, TotalCount);
        
        }

        private void HandleEnemyDead()
        {
            AliveCount--;
            OnAliveChanged?.Invoke(AliveCount, TotalCount);
            if (AliveCount > 0) return;

            StartCoroutine(NextFloorRoutine());
        }

        private IEnumerator NextFloorRoutine()
        {
            yield return new WaitForSeconds(floorDelay);
            StartFloor(CurrentFloor + 1);
        }

        private void ClearEnemies()
        {
            foreach (AbstractEnemy enemy in _spawnedEnemies)
            {
                if (enemy == null) continue;

                enemy.Health.OnDead -= HandleEnemyDead;
                Destroy(enemy.gameObject);
            }

            _spawnedEnemies.Clear();
            AliveCount = 0;
        }
    }
}