using System;
using Agents.Player;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using GameSystem.GameServices;
using UnityEngine;

namespace GameSystem
{
    public class Exp : MonoBehaviour
    {
        
        [SerializeField] private float expSpeed = 5f;
        [SerializeField] private int expAmount = 2;
        private const float DetectDistance = 0.2f;

        private Transform _player;
        private ILevelService _levelService;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            IPlayerTransform playerService = ServiceLocator.Get<IPlayerTransform>();
            _levelService = ServiceLocator.Get<ILevelService>();

            if (playerService == null || _levelService == null)
            {
                enabled = false;
                return;
            }

            _player = playerService.Transform;
        }
        

        private void FixedUpdate()
        {
           
            Vector3 dir = _player.position - transform.position;  
            Vector2 normalDir = dir.normalized;
            
            float sqrDistance = dir.sqrMagnitude;
            if (sqrDistance <= DetectDistance * DetectDistance)
            {
                _levelService.GetExp(expAmount);
                gameObject.SetActive(false);
            }
            
            _rb.linearVelocity = normalDir * expSpeed;
        }
        
    }
}