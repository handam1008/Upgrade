using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevLib.TileAstar
{
    [CreateAssetMenu(fileName = "Path bake data", menuName = "Lib/TileAstar/Bake Data", order = 0)]
    public class PathBakeDataSO : ScriptableObject
    {
        public List<NodeData> points = new List<NodeData>();
        private Dictionary<Vector3Int, NodeData> _pointDict; //런타임 전용 딕셔너리
        
        
        public void InitializeBakeData()
        {
            //런타임에서 빠르게 조회가 가능하도록 cellPosition으로 딕셔너리를 만들어준다.
            //개수만 비교하면 맵을 고쳐도 걷는 칸 수가 같을 때 옛 딕셔너리가 그대로 남는다.
            //NodeData는 struct라 딕셔너리에 복사본이 들어가므로, 낡은 복사본은 옛 neighbors를 가리킨다.
            _pointDict = points.ToDictionary(node => node.cellPosition);
        }
        
        public void ClearPoints() => points?.Clear();

        public void AddPoint(Vector3 worldPosition, Vector3Int cellPosition)
        {
            points.Add(new NodeData(worldPosition, cellPosition));
        }
        
        public bool HasNode(Vector3Int cellPosition) 
            => _pointDict != null && _pointDict.ContainsKey(cellPosition);

        public bool GetNodeIfExist(Vector3Int cellPosition, out NodeData node)
        {
            if (HasNode(cellPosition))
            {
                node = _pointDict[cellPosition];
                return true;
            }

            node = default;
            return false;
        }
    }
    
}