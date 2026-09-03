using DevLib.ServiceLocator;
using GameSystem.GameServices;
using UnityEngine;

namespace CombatSystem.Feedback
{
    public class CameraShakeFeedback : AbstractFeedback
    {
        [SerializeField] private ShakeType shakeType = ShakeType.Hit;
        [SerializeField] private float force = 0.3f;

        private ICameraShakeService _shake;

        private void Start()
        {
            _shake = ServiceLocator.Get<ICameraShakeService>();
        }

        public override void Play(FeedbackData data)
        {
            _shake?.Shake(shakeType, data.Normal * force);
        }
    }
}
