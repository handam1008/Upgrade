using System;
using DevLib.ServiceLocator;
using UnityEngine;

namespace GameSystem.GameServices
{
    public class PlayerTrmService : MonoBehaviour , IPlayerTransform
    {
        [field:SerializeField] public Transform Transform { get; private set; }
        
        private void Awake()
        {
            ServiceLocator.Register<IPlayerTransform>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IPlayerTransform>();
        }

    }
}