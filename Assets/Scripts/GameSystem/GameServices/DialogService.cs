using System;
using DevLib.ServiceLocator;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystem.GameServices
{
    [RequireComponent(typeof(UIDocument))]
    public class DialogService : MonoBehaviour, IDialogService
    {
        private VisualElement _root;
        private Label _message;
        private Button _yesButton;
        private Button _noButton;

        private Action _onConfirm;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            ServiceLocator.Register<IDialogService>(this);
        }

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _message = _root.Q<Label>("dialog-message");
            _yesButton = _root.Q<Button>("yes-button");
            _noButton = _root.Q<Button>("no-button");

            _yesButton.clicked += HandleConfirm;
            _noButton.clicked += HandleCancel;

            _root.style.display = DisplayStyle.None;
            IsOpen = false;
        }

        private void OnDisable()
        {
            if (_yesButton != null) _yesButton.clicked -= HandleConfirm;
            if (_noButton != null) _noButton.clicked -= HandleCancel;
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IDialogService>();
        }

        public void Show(string message, Action onConfirm)
        {
            if (IsOpen) return;

            _onConfirm = onConfirm;
            _message.text = message;

            _root.style.display = DisplayStyle.Flex;
            IsOpen = true;
            Time.timeScale = 0f;
        }

        private void HandleConfirm()
        {
            Action callback = _onConfirm;
            Close();
            callback?.Invoke();
        }

        private void HandleCancel() => Close();

        private void Close()
        {
            _onConfirm = null;
            _root.style.display = DisplayStyle.None;
            IsOpen = false;
            Time.timeScale = 1f;
        }
    }
}
