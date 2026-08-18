using UnityEngine;
namespace GameSystem.GameServices
{
    public interface IMapService
    {
        public Vector3 GetCellCenterToWorld(Vector3Int cellPosition);
        public Vector3Int GetWorldToCell(Vector3 worldPosition);
        public Vector3 GetCellToWorld(Vector3Int cellPosition);
        public void EnterSoundTile(Vector3 worldPosition);
    }
}