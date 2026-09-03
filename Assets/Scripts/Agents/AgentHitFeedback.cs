using CombatSystem.Feedback;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Agents
{
    public class AgentHitFeedback : MonoModule
    {
        [SerializeField] private FeedbackPlayer onHitFeedback;

        private AbstractAgent _agent;
        private ActionDataModule _actionData;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _agent = owner as AbstractAgent;
            _actionData = owner.GetModule<ActionDataModule>();
        }

        private void Start()
        {
            if (_agent == null) return;
            _agent.OnHit.AddListener(HandleHit);
        }

        private void OnDestroy()
        {
            if (_agent == null) return;
            _agent.OnHit.RemoveListener(HandleHit);
        }

        private void HandleHit()
        {
            if (onHitFeedback == null || _actionData == null) return;

            onHitFeedback.Play(new FeedbackData
            {
                Position = _actionData.HitPoint,
                Normal = _actionData.HitNormal,
                IsCritical = _actionData.IsLastHitCritical
            });
        }
    }
}
