using DevLib.TileAstar;
using UnityEngine;

namespace Agents.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private PathBakeDataSO bakeData;
        [SerializeField] private float minSpawnDistance = 8f;
        [SerializeField] private int maxTryCount = 30;

        public AbstractEnemy Spawn(GameObject prefab, float healthMultiplier, Vector3 avoidPosition)
        {
            if (prefab == null) return null;
            if (!TryPickSpawnPosition(avoidPosition, out Vector3 spawnPosition)) return null;

            GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
            AbstractEnemy enemy = instance.GetComponent<AbstractEnemy>();

            if (enemy != null && enemy.Health != null)
                enemy.Health.SetMaxHealth(enemy.Health.MaxHealth * healthMultiplier);

            return enemy;
        }

        private bool TryPickSpawnPosition(Vector3 avoidPosition, out Vector3 position)
        {
            if (bakeData != null && bakeData.points.Count > 0)
            {
                float minDistanceSqr = minSpawnDistance * minSpawnDistance;

                for (int i = 0; i < maxTryCount; i++)
                {
                    int randomIndex = Random.Range(0, bakeData.points.Count);
                    Vector3 candidate = bakeData.points[randomIndex].worldPosition;

                    if ((candidate - avoidPosition).sqrMagnitude >= minDistanceSqr)
                    {
                        position = candidate;
                        return true;
                    }
                }
            }

            position = Vector3.zero;
            return false;
        }
    }
}
