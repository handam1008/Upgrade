using System.Collections.Generic;
using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        private const string BreatheClass = "title-main--breathe";
        private const string PulseClass = "glow--core--pulse";
        private const string FloatClass = "ember--float";
        private const string LitClass = "tower__win--lit";
        private const string BeaconClass = "tower__beacon--pulse";
        private const long BreatheIntervalMs = 2200;
        private const long PulseIntervalMs = 3000;

        [SerializeField] private string gameSceneName = "VillageScene";
        [SerializeField] private SoundClipSO menuBgm;
        [SerializeField] private SoundClipSO clickSfx;

        private Label _title;
        private Button _startButton;
        private Button _quitButton;
        private Button _tutorialButton;
        private Button _tutorialClose;
        private VisualElement _tutorialPanel;
        private VisualElement _glowCore;
        private bool _breatheOn;
        private bool _pulseOn;

        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;

            _title = root.Q<Label>("title-main");
            _startButton = root.Q<Button>("start-button");
            _quitButton = root.Q<Button>("quit-button");

            _glowCore = root.Q<VisualElement>(className: "glow--core");

            _tutorialButton = root.Q<Button>("tutorial-button");
            _tutorialClose = root.Q<Button>("tutorial-close");
            _tutorialPanel = root.Q<VisualElement>("tutorial-panel");

            _startButton.clicked += HandleStart;
            _quitButton.clicked += HandleQuit;
            _tutorialButton.clicked += OpenTutorial;
            _tutorialClose.clicked += CloseTutorial;

            root.schedule.Execute(Breathe).Every(BreatheIntervalMs);
            root.schedule.Execute(Pulse).Every(PulseIntervalMs);

            StartEmbers(root);
            StartWindows(root);
            StartBeacon(root);

        }

        //AudioService.Awake 보다 OnEnable 이 먼저 돌 수 있어서 Start 에서 튼다.
        private void Start()
        {
            if (menuBgm != null) ServiceLocator.Get<IAudioService>()?.PlayBgm(menuBgm);
        }

        private void PlayClick()
        {
            if (clickSfx == null) return;
            ServiceLocator.Get<IAudioService>()?.PlaySfx(clickSfx);
        }

        private void StartWindows(VisualElement root)
        {
            List<VisualElement> windows = root.Query<VisualElement>(className: "tower__win").ToList();

            for (int i = 0; i < windows.Count; i++)
            {
                VisualElement window = windows[i];
                long cycle = Random.Range(1800, 4200);
                long delay = Random.Range(0, 3000);

                window.schedule.Execute(() => window.ToggleInClassList(LitClass)).Every(cycle).StartingIn(delay);
            }
        }

        private void StartBeacon(VisualElement root)
        {
            VisualElement beacon = root.Q<VisualElement>(className: "tower__beacon");
            if (beacon == null) return;

            beacon.schedule.Execute(() => beacon.ToggleInClassList(BeaconClass)).Every(1400);
        }

        //불티마다 주기와 시작 시각을 다르게 줘야 한 덩어리로 움직이지 않는다.
        private void StartEmbers(VisualElement root)
        {
            List<VisualElement> embers = root.Query<VisualElement>(className: "ember").ToList();

            for (int i = 0; i < embers.Count; i++)
            {
                VisualElement ember = embers[i];
                long cycle = Random.Range(2600, 5200);
                long delay = Random.Range(0, 2600);

                ember.style.transitionDuration = new List<TimeValue> { new TimeValue(cycle / 1000f) };
                ember.schedule.Execute(() => ember.ToggleInClassList(FloatClass)).Every(cycle).StartingIn(delay);
            }
        }

        private void Pulse()
        {
            if (_glowCore == null) return;

            _pulseOn = !_pulseOn;

            if (_pulseOn) _glowCore.AddToClassList(PulseClass);
            else _glowCore.RemoveFromClassList(PulseClass);
        }

        private void OnDisable()
        {
            if (_startButton != null) _startButton.clicked -= HandleStart;
            if (_quitButton != null) _quitButton.clicked -= HandleQuit;
            if (_tutorialButton != null) _tutorialButton.clicked -= OpenTutorial;
            if (_tutorialClose != null) _tutorialClose.clicked -= CloseTutorial;
        }

        private void Breathe()
        {
            if (_title == null) return;

            _breatheOn = !_breatheOn;

            if (_breatheOn) _title.AddToClassList(BreatheClass);
            else _title.RemoveFromClassList(BreatheClass);
        }

        private void OpenTutorial()
        {
            PlayClick();
            if (_tutorialPanel != null) _tutorialPanel.style.display = DisplayStyle.Flex;
        }

        private void CloseTutorial()
        {
            PlayClick();
            if (_tutorialPanel != null) _tutorialPanel.style.display = DisplayStyle.None;
        }

        private void HandleStart()
        {
            PlayClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        private void HandleQuit()
        {
            PlayClick();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
