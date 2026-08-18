using DevLib.SoundSystem.Runtime;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameSystem
{
    [CreateAssetMenu(fileName = "FootstepTile", menuName = "System/Footstep Tile", order = 5)]
    public class FootstepTile : Tile
    {
        [SerializeField] private SoundClipSO[] clips;
        
        public SoundClipSO GetRandomClip()
        {
            if (clips == null || clips.Length == 0) return null;
            
            return clips[Random.Range(0, clips.Length)];
        }
    }
}