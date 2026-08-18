using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Tools > Repair Tilesets (Free -> Premium)
// 삭제된 무료팩을 참조하던 타일/스프라이트를 프리미엄팩으로 교체한다.
// 타일 에셋 파일 경로(=GUID)를 유지하므로 씬의 타일맵 데이터는 그대로 살아난다.
public static class TilesetRepair
{
    private const string PremRoot = "Assets/Asset/The Fan-tasy Tileset (Premium)/Art/";
    private const string TsxRoot = "Assets/Asset/The Fan-tasy Tileset (Premium)/Tiled/Tilesets/";
    private const string TileDir = "Assets/03.Tiles";

    private const string GroundPng = PremRoot + "Ground Tilesets/Tileset_Ground.png";
    private const string RoadPng = PremRoot + "Ground Tilesets/Tileset_Road.png";
    private const string WaterPng = PremRoot + "Water and Sand/Tileset_Water.png";

    // 무료팩 길 타일 ID -> 연결규칙(wang) 키. 프리미엄에서 같은 키를 가진 타일로 바꾼다.
    private static readonly Dictionary<string, int[]> FreeRoadWang = new Dictionary<string, int[]>
    {
        { "1,1,1,1,1,1,1,1", new[] { 8, 48, 49, 50, 54, 55, 56 } },
        { "0,0,1,1,1,1,1,0", new[] { 2, 69, 70, 71 } },
        { "1,1,1,0,0,0,1,1", new[] { 14, 60, 61, 62 } },
        { "1,1,1,1,1,0,0,0", new[] { 7, 66, 67, 68 } },
        { "1,0,0,0,1,1,1,1", new[] { 9, 63, 64, 65 } },
        { "0,0,1,1,1,0,0,0", new[] { 1 } },  { "0,0,0,0,1,1,1,0", new[] { 3 } },
        { "1,1,1,0,0,0,0,0", new[] { 13 } }, { "1,0,0,0,0,0,1,1", new[] { 15 } },
        { "1,1,1,0,1,1,1,1", new[] { 24 } }, { "1,1,1,1,1,0,1,1", new[] { 25 } },
        { "1,0,1,1,1,1,1,1", new[] { 30 } }, { "1,1,1,1,1,1,1,0", new[] { 31 } },
        { "0,0,1,0,0,0,1,0", new[] { 20 } }, { "1,0,0,0,1,0,0,0", new[] { 6 } },
        { "0,0,0,0,1,0,0,0", new[] { 0 } },  { "1,0,0,0,0,0,0,0", new[] { 12 } },
        { "0,0,1,0,0,0,0,0", new[] { 19 } }, { "0,0,0,0,0,0,1,0", new[] { 21 } },
        { "0,0,1,0,1,0,0,0", new[] { 26 } }, { "0,0,0,0,1,0,1,0", new[] { 27 } },
        { "1,0,1,0,0,0,0,0", new[] { 32 } }, { "1,0,0,0,0,0,1,0", new[] { 33 } },
        { "1,0,1,0,0,0,1,0", new[] { 38 } }, { "1,0,1,0,1,0,0,0", new[] { 39 } },
        { "1,0,0,0,1,0,1,0", new[] { 44 } }, { "0,0,1,0,1,0,1,0", new[] { 45 } },
        { "1,0,1,0,1,0,1,0", new[] { 46 } },
        { "0,0,1,1,1,0,1,0", new[] { 4 } },  { "1,0,0,0,1,1,1,0", new[] { 5 } },
        { "1,1,1,0,1,0,0,0", new[] { 10 } }, { "1,0,1,0,0,0,1,1", new[] { 11 } },
        { "1,0,1,1,1,0,0,0", new[] { 16 } }, { "0,0,1,0,1,1,1,0", new[] { 17 } },
        { "1,1,1,0,0,0,1,0", new[] { 22 } }, { "1,0,0,0,1,0,1,1", new[] { 23 } },
        { "1,0,1,1,1,0,1,0", new[] { 28 } }, { "1,0,1,0,1,1,1,0", new[] { 29 } },
        { "1,1,1,0,1,0,1,0", new[] { 34 } }, { "1,0,1,0,1,0,1,1", new[] { 35 } },
        { "1,0,1,1,1,1,1,0", new[] { 36 } }, { "1,0,1,0,1,1,1,1", new[] { 37 } },
        { "1,1,1,0,1,1,1,0", new[] { 40 } }, { "1,0,1,1,1,0,1,1", new[] { 41 } },
        { "1,1,1,1,1,0,1,0", new[] { 42 } }, { "1,1,1,0,1,0,1,1", new[] { 43 } },
    };

    // 무료팩 잔디 12종 -> 프리미엄 잔디 12종 (둘 다 6개씩 두 줄)
    private static readonly int[] FreeGrass = { 96, 97, 98, 99, 100, 101, 108, 109, 110, 111, 112, 113 };
    private static readonly int[] PremGrass = { 384, 385, 386, 387, 388, 389, 432, 433, 434, 435, 436, 437 };

    [MenuItem("Tools/Repair Tilesets (Free -> Premium)")]
    public static void Repair()
    {
        // 1) 무료 길 ID -> 프리미엄 길 ID 대응표 만들기
        var premRoadByKey = ParseWang(TsxRoot + "Tileset_Road.tsx");
        var roadMap = new Dictionary<int, int>();
        foreach (var kv in FreeRoadWang)
        {
            if (!premRoadByKey.TryGetValue(kv.Key, out var premIds) || premIds.Count == 0) continue;
            for (int i = 0; i < kv.Value.Length; i++)
                roadMap[kv.Value[i]] = premIds[i % premIds.Count]; // 변형 타일도 골고루
        }

        var grassMap = new Dictionary<int, int>();
        for (int i = 0; i < FreeGrass.Length; i++) grassMap[FreeGrass[i]] = PremGrass[i];

        // 2) 프리미엄 텍스처 슬라이스
        var roadSprites = Slice(RoadPng, roadMap.Values.Distinct().ToArray());
        var grassSprites = Slice(GroundPng, PremGrass);
        var waterIds = CollectWaterIds();
        var waterSprites = Slice(WaterPng, waterIds);

        // 3) 기존 타일 에셋의 스프라이트만 교체 (파일 유지 = GUID 유지 = 씬 무사)
        int fixedRoad = 0, fixedGrass = 0, fixedWater = 0, skipped = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:TileBase", new[] { TileDir }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            int us = file.LastIndexOf('_');
            if (us < 0 || !int.TryParse(file.Substring(us + 1), out int id)) { skipped++; continue; }
            string prefix = file.Substring(0, us);

            if (prefix == "Tileset_Road" && roadMap.TryGetValue(id, out int premRoadId))
            {
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile != null && roadSprites.TryGetValue($"P_Road_{premRoadId}", out var sp))
                { tile.sprite = sp; EditorUtility.SetDirty(tile); fixedRoad++; }
            }
            else if (prefix == "Tileset_Ground" && grassMap.TryGetValue(id, out int premGrassId))
            {
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile != null && grassSprites.TryGetValue($"P_Ground_{premGrassId}", out var sp))
                { tile.sprite = sp; EditorUtility.SetDirty(tile); fixedGrass++; }
            }
            else if (prefix == "Tileset_Water")
            {
                // 프리미엄 물 타일셋은 무료와 배치·애니메이션이 동일 → 같은 ID 그대로
                var anim = AssetDatabase.LoadAssetAtPath<AnimatedTile>(path);
                if (anim != null)
                {
                    var frames = new[] { id, id + 6, id + 12, id + 18 }
                        .Select(f => waterSprites.TryGetValue($"P_Water_{f}", out var s) ? s : null).ToArray();
                    if (frames.All(f => f != null))
                    { anim.m_AnimatedSprites = frames; EditorUtility.SetDirty(anim); fixedWater++; }
                }
            }
            else skipped++; // FarmField 등 이미 프리미엄 기반인 것들
        }
        AssetDatabase.SaveAssets();

        // 4) 씬 안의 깨진 스프라이트(나무/덤불 등) 이름으로 재연결
        int fixedScene = RepairSceneSprites();

        Debug.Log($"[TilesetRepair] 길 {fixedRoad} / 잔디 {fixedGrass} / 물 {fixedWater} 타일 복구, " +
                  $"씬 스프라이트 {fixedScene}개 재연결 (그대로 둔 에셋 {skipped}개)");
    }

    // 씬에서 sprite가 비어버린 SpriteRenderer를 오브젝트 이름으로 프리미엄 스프라이트에 연결
    private static int RepairSceneSprites()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return 0;

        // 프리미엄 팩 전체에서 파일명 -> 경로 색인
        var index = new Dictionary<string, string>();
        foreach (string guid in AssetDatabase.FindAssets("t:texture2D", new[] { PremRoot.TrimEnd('/') }))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            string n = System.IO.Path.GetFileNameWithoutExtension(p);
            if (!index.ContainsKey(n)) index[n] = p;
        }

        int count = 0;
        foreach (var root in scene.GetRootGameObjects())
        foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.sprite != null) continue;
            string name = sr.gameObject.name;
            if (!index.TryGetValue(name, out string assetPath)) continue;

            EnsureSingleSprite(assetPath);
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sp == null) continue;
            sr.sprite = sp;
            EditorUtility.SetDirty(sr);
            count++;
        }
        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        return count;
    }

    private static Dictionary<string, List<int>> ParseWang(string tsxAssetPath)
    {
        var result = new Dictionary<string, List<int>>();
        string full = System.IO.Path.GetFullPath(
            System.IO.Path.Combine(Application.dataPath, "..", tsxAssetPath));
        var root = XDocument.Load(full).Root;
        foreach (var wt in root.Descendants("wangtile"))
        {
            string key = (string)wt.Attribute("wangid");
            if (key.Any(c => c >= '2' && c <= '9')) continue; // 다른 색(벽돌길 등) 제외
            int id = (int)wt.Attribute("tileid");
            if (!result.TryGetValue(key, out var list)) result[key] = list = new List<int>();
            list.Add(id);
        }
        return result;
    }

    private static int[] CollectWaterIds()
    {
        var ids = new HashSet<int>();
        foreach (string guid in AssetDatabase.FindAssets("t:TileBase", new[] { TileDir }))
        {
            string file = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid));
            if (!file.StartsWith("Tileset_Water_")) continue;
            if (!int.TryParse(file.Substring("Tileset_Water_".Length), out int id)) continue;
            foreach (int f in new[] { id, id + 6, id + 12, id + 18 }) ids.Add(f);
        }
        return ids.ToArray();
    }

    // 타일셋 텍스처를 Multiple로 슬라이스. 이름은 P_<종류>_<id>로 고유하게.
    private static Dictionary<string, Sprite> Slice(string assetPath, int[] ids)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (imp == null || tex == null)
        {
            Debug.LogError("[TilesetRepair] 텍스처를 찾을 수 없음: " + assetPath);
            return new Dictionary<string, Sprite>();
        }
        string kind = assetPath.Contains("Road") ? "Road" : assetPath.Contains("Water") ? "Water" : "Ground";
        int cols = tex.width / 16;

        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = 16;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        var ts = new TextureImporterSettings();
        imp.ReadTextureSettings(ts);
        ts.spriteMode = (int)SpriteImportMode.Multiple;
        imp.SetTextureSettings(ts);

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var dp = factories.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();

        // 기존 슬라이스(FarmField 등 다른 용도)를 지우지 않도록 병합
        var existing = dp.GetSpriteRects().ToList();
        var names = new HashSet<string>(existing.Select(r => r.name));
        foreach (int id in ids.OrderBy(i => i))
        {
            string n = $"P_{kind}_{id}";
            if (names.Contains(n)) continue;
            int r = id / cols, c = id % cols;
            existing.Add(new SpriteRect
            {
                name = n,
                spriteID = GUID.Generate(),
                rect = new Rect(c * 16, tex.height - (r + 1) * 16, 16, 16),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });
            names.Add(n);
        }
        dp.SetSpriteRects(existing.ToArray());
        var nameFileId = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameFileId.SetNameFileIdPairs(existing.Select(x => new SpriteNameFileIdPair(x.name, x.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();

        return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>()
            .GroupBy(s => s.name).ToDictionary(g => g.Key, g => g.First());
    }

    private static void EnsureSingleSprite(string assetPath)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        if (imp == null) return;
        bool changed = false;
        if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; changed = true; }
        if (imp.spritePixelsPerUnit != 16) { imp.spritePixelsPerUnit = 16; changed = true; }
        if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; changed = true; }
        if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        var ts = new TextureImporterSettings();
        imp.ReadTextureSettings(ts);
        if (ts.spriteMode != (int)SpriteImportMode.Single || ts.spriteAlignment != (int)SpriteAlignment.BottomCenter)
        {
            ts.spriteMode = (int)SpriteImportMode.Single;
            ts.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            imp.SetTextureSettings(ts);
            changed = true;
        }
        if (changed) imp.SaveAndReimport();
    }
}
