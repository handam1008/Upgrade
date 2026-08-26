using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using GameSystem.GameServices;
using UnityEngine;

namespace Agents
{
    public class HitFeedback : MonoModule
    {
        [SerializeField] private ShakeType shakeType = ShakeType.Hit;
        [SerializeField] private float shakeForce = 0.5f;
        
        private AbstractAgent _agent;
        private ActionDataModule  _actionData;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _agent = owner as AbstractAgent;
            _actionData = owner.GetModule<ActionDataModule>();
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
            ServiceLocator.Get<ICameraShakeService>()?.Shake(shakeType, _actionData.HitNormal * shakeForce);
        }
    }
}