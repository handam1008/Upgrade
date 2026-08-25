using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Tools > Add Village Colliders
// 마을 스프라이트 오브젝트에 밑동 크기의 BoxCollider2D 를 붙이고 Obstacle 레이어로 바꾼다.
// 강(Water 타일맵)은 타일맵 콜라이더로 한 번에 막는다.
// 이미 콜라이더가 있으면 건너뛰므로 여러 번 실행해도 안전하다.
public static class VillageColliderBuilder
{
    private const string ObstacleLayerName = "Obstacle";

    // 접두어 → (가로 비율, 세로 비율). 스프라이트 하단에 그 비율만큼 박스를 만든다.
    // 탑다운이라 그림 전체를 막으면 뒤로 못 지나가서 답답해진다. 밑동만 막는다.
    private static readonly (string prefix, float w, float h)[] Rules =
    {
        ("TreeStump",     0.5f, 0.30f),
        ("Tree",          0.4f, 0.20f),
        ("Watchtower",    0.8f, 0.50f),
        ("Tower",         0.8f, 0.50f),
        ("House",         0.9f, 0.50f),
        ("Fountain",      1.0f, 0.50f),
        ("MarketStand",   0.9f, 0.40f),
        ("FishRack",      0.8f, 0.40f),
        ("ArcheryTarget", 0.7f, 0.40f),
        ("ArmorStand",    0.6f, 0.40f),
        ("WoodCart",      0.9f, 0.40f),
        ("HayStack",      0.8f, 0.40f),
        ("Bench",         0.9f, 0.40f),
        ("Barrel",        0.7f, 0.50f),
        ("Crate",         0.8f, 0.50f),
        ("Chest",         0.8f, 0.50f),
        ("Anvil",         0.8f, 0.50f),
        ("Boat",          0.9f, 0.50f),
        ("Rock",          0.7f, 0.40f),
        ("Bush",          0.6f, 0.30f),
        ("Sack",          0.7f, 0.40f),
        ("Vase",          0.6f, 0.40f),
        ("LampPost",      0.3f, 0.20f),
        ("Sign",          0.4f, 0.20f),
    };

    // 캐릭터 스프라이트. 여기 콜라이더가 붙으면 자기 몸에 끼인다.
    private static readonly string[] NeverTouch = { "Visual" };

    [MenuItem("Tools/Add Village Colliders")]
    public static void Build()
    {
        int obstacleLayer = LayerMask.NameToLayer(ObstacleLayerName);
        if (obstacleLayer < 0)
        {
            Debug.LogError($"[Village] '{ObstacleLayerName}' 레이어가 없습니다. Project Settings 에서 먼저 만들어 주세요.");
            return;
        }

        int added = 0, already = 0;
        var skipped = new SortedDictionary<string, int>();

        foreach (var renderer in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            GameObject go = renderer.gameObject;
            if (NeverTouch.Contains(go.name)) continue;
            if (renderer.sprite == null) continue;

            if (!TryMatch(go.name, out float widthRatio, out float heightRatio))
            {
                if (!skipped.ContainsKey(go.name)) skipped[go.name] = 0;
                skipped[go.name]++;
                continue;
            }

            //손으로 만든 다른 종류의 콜라이더는 건드리지 않는다.
            var existing = go.GetComponent<Collider2D>();
            if (existing != null && !(existing is BoxCollider2D)) { already++; continue; }

            Bounds b = renderer.sprite.bounds; //로컬 기준. 피벗과 PPU가 반영돼 있다.

            //있으면 크기만 갱신한다. 비율을 바꾸고 다시 돌릴 수 있어야 하기 때문.
            var box = (BoxCollider2D)existing;
            if (box == null) box = Undo.AddComponent<BoxCollider2D>(go);
            else Undo.RecordObject(box, "Resize Village Collider");

            box.size = new Vector2(b.size.x * widthRatio, b.size.y * heightRatio);
            //박스의 아랫변을 스프라이트 아랫변에 맞춘다.
            box.offset = new Vector2(b.center.x, b.min.y + box.size.y * 0.5f);
            EditorUtility.SetDirty(box);

            Undo.RecordObject(go, "Set Obstacle Layer");
            go.layer = obstacleLayer;
            EditorUtility.SetDirty(go);
            added++;
        }

        int water = BuildWaterCollider(obstacleLayer);

        EditorSceneManagerMarkDirty();

        Debug.Log($"[Village] 콜라이더 {added}개 추가, {already}개는 이미 있어 건너뜀, 강 타일맵 {water}개 처리\n" +
                  $"규칙에 없어 건너뛴 이름 {skipped.Count}종: {string.Join(", ", skipped.Keys.Take(40))}");
    }

    // 성벽은 조각마다 막지 않고 큰 콜라이더를 손으로 놓는 편이 낫다. 이미 붙은 것을 걷어낸다.
    [MenuItem("Tools/Remove CityWall Colliders")]
    public static void RemoveCityWallColliders()
    {
        int removed = 0;
        foreach (var renderer in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (!renderer.gameObject.name.StartsWith("CityWall")) continue;

            var box = renderer.GetComponent<BoxCollider2D>();
            if (box == null) continue;

            Undo.DestroyObjectImmediate(box);
            removed++;
        }

        EditorSceneManagerMarkDirty();
        Debug.Log($"[Village] 성벽 콜라이더 {removed}개 제거. 레이어는 Obstacle 그대로 두었다.");
    }

    // 이름 앞부분이 규칙과 맞는지. 긴 접두어를 먼저 보게 정렬해서 TreeStump 가 Tree 보다 우선하게 한다.
    private static bool TryMatch(string goName, out float w, out float h)
    {
        foreach (var rule in Rules.OrderByDescending(r => r.prefix.Length))
        {
            if (!goName.StartsWith(rule.prefix)) continue;
            w = rule.w; h = rule.h;
            return true;
        }
        w = h = 0f;
        return false;
    }

    // 강은 타일 하나하나가 아니라 외곽선 하나로 묶는다.
    private static int BuildWaterCollider(int obstacleLayer)
    {
        int count = 0;
        foreach (var tilemap in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            if (tilemap.gameObject.name != "Water") continue;

            GameObject go = tilemap.gameObject;
            if (go.GetComponent<TilemapCollider2D>() == null)
            {
                var tc = Undo.AddComponent<TilemapCollider2D>(go);
                tc.compositeOperation = Collider2D.CompositeOperation.Merge;

                var rb = Undo.AddComponent<Rigidbody2D>(go);
                rb.bodyType = RigidbodyType2D.Static;

                Undo.AddComponent<CompositeCollider2D>(go);
            }

            Undo.RecordObject(go, "Set Obstacle Layer");
            go.layer = obstacleLayer;
            EditorUtility.SetDirty(go);
            count++;
        }
        return count;
    }

    private static void EditorSceneManagerMarkDirty()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }
}
