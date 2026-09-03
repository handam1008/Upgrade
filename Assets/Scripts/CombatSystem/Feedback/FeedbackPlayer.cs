using UnityEngine;

namespace CombatSystem.Feedback
{
    public class FeedbackPlayer : MonoBehaviour
    {
        private AbstractFeedback[] _feedbacks;

        private void Awake()
        {
            _feedbacks = GetComponents<AbstractFeedback>();
        }

        public void Play(FeedbackData data)
        {
            for (int i = 0; i < _feedbacks.Length; i++)
                _feedbacks[i].Play(data);
        }

        public void StopAll()
        {
            for (int i = 0; i < _feedbacks.Length; i++)
                _feedbacks[i].Stop();
        }
    }
}
