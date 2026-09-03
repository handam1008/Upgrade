using CombatSystem;
using CombatSystem.Feedback;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Agents
{
    public class AgentDeathFeedback : MonoModule
    {
        [SerializeField] private FeedbackPlayer onDeathFeedback;

        private HealthComponent _health;
        private ActionDataModule _actionData;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _health = owner.GetModule<HealthComponent>();
            _actionData = owner.GetModule<ActionDataModule>();
        }

        private void Start()
        {
            if (_health == null) return;
            _health.OnDead += HandleDead;
        }

        private void OnDestroy()
        {
            if (_health == null) return;
            _health.OnDead -= HandleDead;
        }

        private void HandleDead()
        {
            if (onDeathFeedback == null) return;

            onDeathFeedback.Play(new FeedbackData
            {
                Position = _actionData != null ? _actionData.HitPoint : (Vector2)transform.position,
                Normal = _actionData != null ? _actionData.HitNormal : Vector2.zero,
                IsCritical = false
            });
        }
    }
}
