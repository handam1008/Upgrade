using DevLib.ServiceLocator;
using GameSystem.GameServices;
using UnityEngine;

namespace CombatSystem.Feedback
{
    public class HitStopFeedback : AbstractFeedback
    {
        [SerializeField] private float duration = 0.05f;

        private IHitStopService _hitStop;

        private void Start()
        {
            _hitStop = ServiceLocator.Get<IHitStopService>();
        }

        public override void Play(FeedbackData data)
        {
            _hitStop?.Stop(duration);
        }
    }
}
