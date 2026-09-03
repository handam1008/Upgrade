using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameSystem.GameServices
{
    public class MapService : MonoBehaviour, IMapService
    {
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap soundTile;

        private void Awake()
        {
            ServiceLocator.Register<IMapService>(this);
            soundTile.GetComponent<TilemapRenderer>().enabled = false;
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IMapService>();
        }

        public Vector3 GetCellCenterToWorld(Vector3Int cellPosition)
            => grid.GetCellCenterWorld(cellPosition);
        public Vector3Int GetWorldToCell(Vector3 worldPosition)
            => grid.WorldToCell(worldPosition);
        public Vector3 GetCellToWorld(Vector3Int cellPosition)
            => grid.CellToWorld(cellPosition);

        public void EnterSoundTile(Vector3 worldPosition)
        {
            Vector3Int cellPosition = GetWorldToCell(worldPosition);

            SoundClipSO targetClip = null;

            if (soundTile.GetTile(cellPosition) is FootstepTile footstepTile)
            {
                targetClip = footstepTile.GetRandomClip();
            }

            if (targetClip != null)
            {
                ServiceLocator.Get<IAudioService>().PlaySfx(targetClip);
            }
        }
    }
}