using UnityEngine;

namespace CombatSystem
{
    /// <summary>
    /// 공격 예고선을 켜고 끄고, 위치만 잡아주는 컴포넌트.
    /// 색·굵기·재질·정렬 순서는 각 LineRenderer 인스펙터에서 직접 조절한다.
    /// 몇 개를 어디로 그릴지는 쓰는 쪽(스킬)이 정하므로, 돌진 통로든 부챗살이든 그대로 쓸 수 있다.
    /// </summary>
    public class TelegraphLines : MonoBehaviour
    {
        [SerializeField] private LineRenderer[] lines;

        public int Count => lines != null ? lines.Length : 0;

        /// <summary>앞에서부터 count개만 켠다. 나머지는 꺼서 이전 위치가 남지 않게 한다.</summary>
        public void Show(int count)
        {
            for (int i = 0; i < Count; i++)
            {
                if (lines[i] == null) continue;

                lines[i].useWorldSpace = true; //월드 좌표로 위치를 넣으므로 켜져 있어야 한다
                lines[i].positionCount = 2;
                lines[i].enabled = i < count;
            }
        }

        public void Hide()
        {
            for (int i = 0; i < Count; i++)
                if (lines[i] != null)
                    lines[i].enabled = false;
        }

        public void SetLine(int index, Vector3 start, Vector3 end)
        {
            if (index < 0 || index >= Count || lines[index] == null) return;

            lines[index].SetPosition(0, start);
            lines[index].SetPosition(1, end);
        }
    }
}
