using UnityEngine;

namespace GameSystem.GameServices
{
    public interface ICameraShakeService
    {
        void Shake(ShakeType type, Vector2 velocity);
    }
}
