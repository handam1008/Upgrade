using System;
using System.Collections.Generic;
using DevLib.ServiceLocator;
using Unity.Cinemachine;
using UnityEngine;

namespace GameSystem.GameServices
{
    public class CameraShakeService : MonoBehaviour, ICameraShakeService
    {
        [Serializable]
        private struct ShakePreset
        {
            public ShakeType type;
            public CinemachineImpulseSource source;
        }

        [SerializeField] private ShakePreset[] presets;

        private readonly Dictionary<ShakeType, CinemachineImpulseSource> _sources = new();

        private void Awake()
        {
            foreach (ShakePreset preset in presets)
            {
                if (preset.source == null) continue;
                _sources[preset.type] = preset.source;
            }

            ServiceLocator.Register<ICameraShakeService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<ICameraShakeService>();
        }

        public void Shake(ShakeType type, Vector2 velocity)
        {
            if (!_sources.TryGetValue(type, out CinemachineImpulseSource source))
            {
                Debug.LogWarning($"[CameraShake] {type} 프리셋이 연결되지 않았습니다.");
                return;
            }

            source.GenerateImpulseWithVelocity(velocity);
        }
    }
}
