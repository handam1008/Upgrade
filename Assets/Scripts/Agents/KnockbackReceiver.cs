using DevLib.ModuleSystem;
using UnityEngine;

namespace Agents
{
    public class KnockbackReceiver : MonoModule
    {
        private AbstractAgent _agent;
        private ActionDataModule _data;
        private ITopDownMover _mover;

        [SerializeField] private float knockbackDuration = 0.12f;
        [SerializeField] private float knockbackSpeed= 5f;

        private float _baseSpeed;
        private bool _isKnockback;
        private float _knockbackEndTime = 2f;
        

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _agent = owner as AbstractAgent;
            _data = owner.GetModule<ActionDataModule>();
            _mover = owner.GetModule<ITopDownMover>();
        }

        private void Start()
        {
            _agent.OnHit.AddListener(HandleHit);
        }

        private void OnDestroy()
        {
            _agent.OnHit.RemoveListener(HandleHit);
        }

        private void HandleHit()
        {
            
            if (!_isKnockback)
            {
                _baseSpeed = _mover.MoveSpeed;
            }
            _isKnockback = true;
            
            _knockbackEndTime = Time.time + knockbackDuration;

           
            _mover.SetMovementSpeed(knockbackSpeed);
            _mover.SetMovement(_data.KnockbackForce);
        }

        private void Update()
        {
            if (!_isKnockback) return;

            if (Time.time >= _knockbackEndTime)
            {
                _mover.SetMovementSpeed(_baseSpeed); 
                _mover.StopImmediately();
                _isKnockback = false;
            }
        }
    }
}