using System;
using UnityEngine;

[Serializable]
public struct EnemySpawn
{
    public GameObject prefab;
    public int count;
}

[Serializable]
public struct FloorSetup
{
    public EnemySpawn[] enemies;
}

[CreateAssetMenu(fileName = "Tower Config", menuName = "System/Tower Config")]
public class TowerConfigSO : ScriptableObject
{
    public FloorSetup[] floors;

    public float healthPerFloor = 0.25f;
    public float spawnMinDistance = 8f;

    public FloorSetup GetFloor(int floor)
    {
        if (floors == null || floors.Length == 0) return default; 
            

        int lastIndex = floors.Length - 1;
            
        int decideFloor = Mathf.Clamp(floor - 1, 0, lastIndex);
            
        return floors[decideFloor];
    }

    public float GetHealthMultiplier(int floor)
    {
        float multiplier = 1 + (floor - 1) * healthPerFloor;
        return multiplier;
    }
}