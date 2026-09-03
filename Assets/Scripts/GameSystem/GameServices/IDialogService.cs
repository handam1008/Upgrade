using System;

namespace GameSystem.GameServices
{
    public interface IDialogService
    {
        bool IsOpen { get; }
        void Show(string message, Action onConfirm);
    }
}
