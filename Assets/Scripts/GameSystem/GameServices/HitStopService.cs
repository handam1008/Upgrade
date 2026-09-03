using System.Collections;
using DevLib.ServiceLocator;
using UnityEngine;

namespace GameSystem.GameServices
{
    public class HitStopService : MonoBehaviour, IHitStopService
    {
        [SerializeField] private float maxDuration = 0.2f;

        private float _remaining;
        private bool _running;

        private void Awake()
        {
            ServiceLocator.Register<IHitStopService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IHitStopService>();
        }

        public void Stop(float duration)
        {
            duration = Mathf.Clamp(duration, 0f, maxDuration);
            if (duration <= 0f) return;

            if (_running)
            {
                _remaining = Mathf.Max(_remaining, duration);
                return;
            }

            if (!Mathf.Approximately(Time.timeScale, 1f)) return;

            _remaining = duration;
            StartCoroutine(StopRoutine());
        }

        private IEnumerator StopRoutine()
        {
            _running = true;
            Time.timeScale = 0f;

            while (_remaining > 0f)
            {
                _remaining -= Time.unscaledDeltaTime;
                yield return null;
            }

            Time.timeScale = 1f;
            _running = false;
        }
    }
}
