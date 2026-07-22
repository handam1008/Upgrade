using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Tools > Place Chests : 현재 열려있는 VillageScene에 보물상자를 '추가만' 한다.
// 씬을 새로 만들지 않으므로 기존 수정사항은 그대로 유지된다.
public static class ChestPlacer
{
    private const string ChestPath = "Assets/Asset/The Fan-tasy Tileset (Premium)/Art/Props/Chest_Steel_3.png";

    // 마을 곳곳 (광장 주변 / 상가 거리 / 강변 / 남쪽 골목 / 성벽 근처)
    private static readonly Vector2[] Spots =
    {
        new Vector2(-30, 4),      // 서쪽 대로 끝
        new Vector2(-17, -4),     // 서쪽 중간 지대
        new Vector2(-8, 11.2f),   // 북측 거리 서쪽
        new Vector2(10, 12.2f),   // 북측 거리 동쪽
        new Vector2(19, -2.5f),   // 동쪽 중간 지대
        new Vector2(28.5f, 8.5f), // 강변
        new Vector2(-24, -14),    // 남서 주택가
        new Vector2(8, -14),      // 남쪽 중간
        new Vector2(17, -19.2f),  // 남동 성벽 근처
        new Vector2(2.5f, 6.4f),  // 성당 앞마당 가장자리
    };

    [MenuItem("Tools/Place Chests")]
    public static void Place()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "VillageScene")
        {
            Debug.LogError($"[ChestPlacer] VillageScene이 아닌 씬({scene.name})이 열려있어 중단합니다.");
            return;
        }

        // 임포트 세팅 (PPU 16 / Point / 무압축 / 바닥 피벗)
        var imp = (TextureImporter)AssetImporter.GetAtPath(ChestPath);
        if (imp != null)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 16;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            var ts = new TextureImporterSettings();
            imp.ReadTextureSettings(ts);
            ts.spriteMode = (int)SpriteImportMode.Single;
            ts.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            imp.SetTextureSettings(ts);
            imp.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ChestPath);
        if (sprite == null) { Debug.LogError("[ChestPlacer] 상자 스프라이트 로드 실패"); return; }

        // 부모 오브젝트 (있으면 재사용)
        var parentGo = GameObject.Find("Chests");
        if (parentGo == null) parentGo = new GameObject("Chests");

        int added = 0;
        foreach (var p in Spots)
        {
            var go = new GameObject("Chest_Steel_3", typeof(SpriteRenderer));
            go.transform.SetParent(parentGo.transform, false);
            go.transform.position = new Vector3(p.x, p.y, 0);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = Mathf.RoundToInt(-p.y * 10);
            added++;
        }

        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[ChestPlacer] 보물상자 {added}개 배치 완료 (기존 씬 유지)");
    }
}
