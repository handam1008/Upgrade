using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;

namespace CombatSystem.Feedback
{
    public class SoundFeedback : AbstractFeedback
    {
        [SerializeField] private SoundClipSO clip;

        private IAudioService _audio;

        private void Start()
        {
            _audio = ServiceLocator.Get<IAudioService>();
        }

        public override void Play(FeedbackData data)
        {
            if (clip == null) return;

            _audio?.PlaySfx(clip);
        }
    }
}
