using System;
using UnityEngine;

namespace GameSystem.GameServices
{
    public interface ITowerService
    {
         int CurrentFloor { get;  }
        int AliveCount { get; }
        int TotalCount { get; }

        void StartTower();

        void ExitTower();
        
        event Action<int> OnFloorChanged; 
        event Action<int, int> OnAliveChanged;
        event Action OnTowerEnded;
    }
}