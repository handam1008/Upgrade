using System;

namespace GameSystem.GameServices
{
    public interface ILevelService
    {
        public void GetExp(int amount);
        int Level { get; }
        float HaveExp { get; }

        event Action<float> OnLevelChanged;
        event Action<int> OnLevelUp;
    }
}