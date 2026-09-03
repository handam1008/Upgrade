using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;

namespace GameSystem.GameServices
{
    public class BgmDirector : MonoBehaviour
    {
        [SerializeField] private SoundClipSO villageBgm;
        [SerializeField] private SoundClipSO towerEnterBgm;
        [SerializeField] private SoundClipSO towerBattleBgm;
        [SerializeField] private int battleStartsAtWave = 2;

        private IAudioService _audio;
        private ITowerService _tower;
        private SoundClipSO _playing;

        private void Start()
        {
            _audio = ServiceLocator.Get<IAudioService>();
            _tower = ServiceLocator.Get<ITowerService>();

            Play(villageBgm);

            if (_tower == null) return;

            _tower.OnFloorChanged += HandleWaveChanged;
            _tower.OnTowerEnded += HandleTowerEnded;
        }

        private void OnDestroy()
        {
            if (_tower == null) return;

            _tower.OnFloorChanged -= HandleWaveChanged;
            _tower.OnTowerEnded -= HandleTowerEnded;
        }

        private void HandleWaveChanged(int wave)
        {
            Play(wave < battleStartsAtWave ? towerEnterBgm : towerBattleBgm);
        }

        private void HandleTowerEnded()
        {
            Play(villageBgm);
        }

        //같은 곡이면 다시 틀지 않는다. 웨이브마다 처음부터 재생되면 끊겨 들린다.
        private void Play(SoundClipSO clip)
        {
            if (clip == null || clip == _playing) return;

            _playing = clip;
            _audio?.PlayBgm(clip);
        }
    }
}
