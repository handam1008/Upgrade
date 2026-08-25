using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Tools > Normalize Sprite Sorting
// 씬의 모든 SpriteRenderer 를 같은 Sorting Layer / Order 로 맞춘다.
// Custom Axis 정렬은 Layer 와 Order 가 같을 때만 쓰이므로, 이걸 맞춰야 Y좌표 정렬이 동작한다.
public static class SortingNormalizer
{
    private const string TargetLayer = "Object";
    private const int TargetOrder = 0;

    // 이름이 이걸로 시작하면 건드리지 않는다.
    // Shadow 는 주인보다 항상 뒤에 있어야 해서 Y 정렬에 끼우면 안 된다.
    private static readonly string[] SkipPrefixes = { "Shadow" };

    [MenuItem("Tools/Normalize Sprite Sorting")]
    public static void Normalize()
    {
        if (SortingLayer.layers.All(l => l.name != TargetLayer))
        {
            Debug.LogError($"[Sorting] '{TargetLayer}' Sorting Layer 가 없습니다.");
            return;
        }

        int changed = 0, alreadyOk = 0;
        var skipped = new SortedSet<string>();
        var orderRange = new List<int>();

        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (SkipPrefixes.Any(p => sr.gameObject.name.StartsWith(p)))
            {
                skipped.Add(sr.gameObject.name);
                continue;
            }

            bool needsChange = sr.sortingLayerName != TargetLayer || sr.sortingOrder != TargetOrder;
            if (!needsChange) { alreadyOk++; continue; }

            orderRange.Add(sr.sortingOrder);

            Undo.RecordObject(sr, "Normalize Sprite Sorting");
            sr.sortingLayerName = TargetLayer;
            sr.sortingOrder = TargetOrder;
            EditorUtility.SetDirty(sr);
            changed++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        string before = orderRange.Count > 0
            ? $"(바꾸기 전 Order 범위 {orderRange.Min()} ~ {orderRange.Max()})"
            : "";

        Debug.Log($"[Sorting] {changed}개를 '{TargetLayer}' / Order {TargetOrder} 로 변경 {before}\n" +
                  $"이미 맞던 것 {alreadyOk}개, 건너뛴 것 {skipped.Count}종: {string.Join(", ", skipped)}");
    }
}
