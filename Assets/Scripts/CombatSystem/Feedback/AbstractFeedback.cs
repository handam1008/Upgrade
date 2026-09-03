using UnityEngine;

namespace CombatSystem.Feedback
{
    public abstract class AbstractFeedback : MonoBehaviour
    {
        public abstract void Play(FeedbackData data);

        public virtual void Stop() { }
    }
}
