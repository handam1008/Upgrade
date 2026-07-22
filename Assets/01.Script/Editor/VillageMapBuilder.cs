using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Tools > Build Village Map — "레드릿지" 성곽도시.
// 바닥/길/물 = 무료팩 오토타일, 건물/성벽/소품 = 프리미엄팩 스프라이트.
public static class VillageMapBuilder
{
    private const string FreeRoot = "Assets/Asset/The Fan-tasy Tileset (Free)/Art/";
    private const string PremRoot = "Assets/Asset/The Fan-tasy Tileset (Premium)/Art/";
    private const string TileDir = "Assets/03.Tiles";
    private const string ScenePath = "Assets/00.Scenes/VillageScene.unity";
    private const string FontPath = "Assets/Font/GFCRedSpirit-Bold SDF.asset";

    // 성벽 링 (내부 = 도시), 강은 동쪽 x 24..26
    private const int WL = -34, ER = 34, SB = -20, NT = 20;
    private const int RiverW = 24, RiverE = 26;

    private static readonly Dictionary<string, int[]> RoadWang = new Dictionary<string, int[]>
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

    private static readonly Dictionary<string, int[]> WaterWang = new Dictionary<string, int[]>
    {
        { "1,1,1,1,1,1,1,1", new[] { 26, 192, 193, 194 } },
        { "0,0,1,1,1,1,1,0", new[] { 2 } },   { "1,1,1,0,0,0,1,1", new[] { 50 } },
        { "1,1,1,1,1,0,0,0", new[] { 25 } },  { "1,0,0,0,1,1,1,1", new[] { 27 } },
        { "0,0,1,1,1,0,0,0", new[] { 1 } },   { "0,0,0,0,1,1,1,0", new[] { 3 } },
        { "1,1,1,0,0,0,0,0", new[] { 49 } },  { "1,0,0,0,0,0,1,1", new[] { 51 } },
        { "1,1,1,0,1,1,1,1", new[] { 96 } },  { "1,1,1,1,1,0,1,1", new[] { 97 } },
        { "1,0,1,1,1,1,1,1", new[] { 120 } }, { "1,1,1,1,1,1,1,0", new[] { 121 } },
        { "1,0,0,0,1,0,0,0", new[] { 24 } },  { "0,0,1,0,0,0,1,0", new[] { 74 } },
        { "0,0,0,0,1,0,0,0", new[] { 0 } },   { "1,0,0,0,0,0,0,0", new[] { 48 } },
        { "0,0,1,0,0,0,0,0", new[] { 73 } },  { "0,0,0,0,0,0,1,0", new[] { 75 } },
        { "0,0,1,0,1,0,0,0", new[] { 98 } },  { "0,0,0,0,1,0,1,0", new[] { 99 } },
        { "1,0,1,0,0,0,0,0", new[] { 122 } }, { "1,0,0,0,0,0,1,0", new[] { 123 } },
        { "1,0,1,0,0,0,1,0", new[] { 146 } }, { "1,0,1,0,1,0,0,0", new[] { 147 } },
        { "1,0,0,0,1,0,1,0", new[] { 170 } }, { "0,0,1,0,1,0,1,0", new[] { 171 } },
        { "1,0,1,0,1,0,1,0", new[] { 172 } },
        { "0,0,1,1,1,0,1,0", new[] { 4 } },   { "1,0,0,0,1,1,1,0", new[] { 5 } },
        { "1,1,1,0,1,0,0,0", new[] { 28 } },  { "1,0,1,0,0,0,1,1", new[] { 29 } },
        { "1,0,1,1,1,0,0,0", new[] { 52 } },  { "0,0,1,0,1,1,1,0", new[] { 53 } },
        { "1,1,1,0,0,0,1,0", new[] { 76 } },  { "1,0,0,0,1,0,1,1", new[] { 77 } },
        { "1,0,1,1,1,1,1,0", new[] { 100 } }, { "1,0,1,0,1,1,1,1", new[] { 101 } },
        { "1,1,1,1,1,0,1,0", new[] { 124 } }, { "1,1,1,0,1,0,1,1", new[] { 125 } },
        { "1,0,1,1,1,0,1,0", new[] { 144 } }, { "1,0,1,0,1,1,1,0", new[] { 145 } },
        { "1,1,1,0,1,1,1,0", new[] { 148 } }, { "1,0,1,1,1,0,1,1", new[] { 149 } },
        { "1,1,1,0,1,0,1,0", new[] { 168 } }, { "1,0,1,0,1,0,1,1", new[] { 169 } },
    };

    private static readonly int[] GrassIds = { 96, 97, 98, 99, 100, 101, 108, 109, 110, 111, 112, 113 };
    private static readonly int[] HouseShapes = { 1, 2, 3, 4, 6, 8 };
    private static readonly int[] ShapeVariants = { 5, 3, 3, 4, 3, 3 };
    private static readonly HashSet<string> _imported = new HashSet<string>();

    // 텃밭: FarmField는 Road와 같은 wang 배치 (54칸뿐이라 50 이하 타일만 사용)
    private static Dictionary<string, int[]> FarmWang =>
        RoadWang.ToDictionary(kv => kv.Key, kv => kv.Value.Where(id => id <= 50).DefaultIfEmpty(kv.Value[0]).ToArray());

    [MenuItem("Tools/Build Village Map")]
    public static void Build()
    {
        MakeTiles("Ground Tileset/Tileset_Ground.png", GrassIds);
        MakeTiles("Ground Tileset/Tileset_Road.png", RoadWang.Values.SelectMany(v => v).Distinct().ToArray());
        MakeTiles("Ground Tilesets/Tileset_FarmField.png", FarmWang.Values.SelectMany(v => v).Distinct().ToArray(), PremRoot);
        MakeWaterTiles();
        // 분수 애니메이션 프레임 (96x96 x 8)
        Slice("Props/Animations/Animation_Fountain_1.png", Enumerable.Range(0, 8).ToArray(),
            PremRoot, 96, 96, SpriteAlignment.BottomCenter);
        BuildScene();
    }

    private static Dictionary<int, TileBase> LoadTiles(string prefix, IEnumerable<int> ids) =>
        ids.ToDictionary(id => id, id => (TileBase)AssetDatabase.LoadAssetAtPath<TileBase>($"{TileDir}/{prefix}_{id}.asset"));

    private static Dictionary<string, Sprite> Slice(string relPath, int[] ids,
        string root = FreeRoot, int tileW = 16, int tileH = 16, SpriteAlignment align = SpriteAlignment.Center)
    {
        string path = root + relPath;
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        string baseName = System.IO.Path.GetFileNameWithoutExtension(path);
        int cols = tex.width / tileW;

        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = 16;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        var tset = new TextureImporterSettings();
        imp.ReadTextureSettings(tset);
        tset.spriteMode = (int)SpriteImportMode.Multiple;
        imp.SetTextureSettings(tset);

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var dp = factories.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();

        var rects = new List<SpriteRect>();
        foreach (int id in ids.OrderBy(i => i))
        {
            int r = id / cols, c = id % cols;
            rects.Add(new SpriteRect
            {
                name = $"{baseName}_{id}",
                spriteID = GUID.Generate(),
                rect = new Rect(c * tileW, tex.height - (r + 1) * tileH, tileW, tileH),
                alignment = align,
                pivot = align == SpriteAlignment.BottomCenter ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f)
            });
        }
        dp.SetSpriteRects(rects.ToArray());
        var nameFileId = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameFileId.SetNameFileIdPairs(rects.Select(x => new SpriteNameFileIdPair(x.name, x.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();

        if (!AssetDatabase.IsValidFolder(TileDir))
            AssetDatabase.CreateFolder("Assets", "03.Tiles");
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s => s.name);
    }

    private static void MakeTiles(string relPath, int[] ids, string root = FreeRoot)
    {
        string baseName = System.IO.Path.GetFileNameWithoutExtension(relPath);
        var sprites = Slice(relPath, ids, root);
        foreach (int id in ids)
        {
            string tilePath = $"{TileDir}/{baseName}_{id}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprites[$"{baseName}_{id}"];
                tile.colliderType = Tile.ColliderType.None;
                AssetDatabase.CreateAsset(tile, tilePath);
            }
            else tile.sprite = sprites[$"{baseName}_{id}"];
        }
        AssetDatabase.SaveAssets();
    }

    private static void MakeWaterTiles()
    {
        int[] baseIds = WaterWang.Values.SelectMany(v => v).Distinct().ToArray();
        int[] allIds = baseIds.SelectMany(id => new[] { id, id + 6, id + 12, id + 18 }).Distinct().ToArray();
        var sprites = Slice("Water and Sand/Tileset_Water.png", allIds);
        foreach (int id in baseIds)
        {
            string tilePath = $"{TileDir}/Tileset_Water_{id}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<AnimatedTile>(tilePath);
            bool isNew = tile == null;
            if (isNew) tile = ScriptableObject.CreateInstance<AnimatedTile>();
            tile.m_AnimatedSprites = new[]
            {
                sprites[$"Tileset_Water_{id}"], sprites[$"Tileset_Water_{id + 6}"],
                sprites[$"Tileset_Water_{id + 12}"], sprites[$"Tileset_Water_{id + 18}"]
            };
            tile.m_MinSpeed = tile.m_MaxSpeed = 3.33f;
            tile.m_TileColliderType = Tile.ColliderType.None;
            if (isNew) AssetDatabase.CreateAsset(tile, tilePath);
            else EditorUtility.SetDirty(tile);
        }
        AssetDatabase.SaveAssets();
    }

    private static Sprite LoadPrem(string rel)
    {
        string path = PremRoot + rel;
        if (!_imported.Contains(path))
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp != null)
            {
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
                    imp.SetTextureSettings(ts); changed = true;
                }
                if (changed) imp.SaveAndReimport();
            }
            _imported.Add(path);
        }
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) Debug.LogWarning("[VillageMap] 스프라이트 로드 실패: " + path);
        return sprite;
    }

    // ── 씬 구성 ─────────────────────────────────────────
    private static void BuildScene()
    {
        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        _imported.Clear();

        var ground = LoadTiles("Tileset_Ground", GrassIds);
        var road = LoadTiles("Tileset_Road", RoadWang.Values.SelectMany(v => v).Distinct());
        var water = LoadTiles("Tileset_Water", WaterWang.Values.SelectMany(v => v).Distinct());
        var farm = LoadTiles("Tileset_FarmField", FarmWang.Values.SelectMany(v => v).Distinct());
        if (ground.Values.Concat(road.Values).Concat(water.Values).Concat(farm.Values).Any(t => t == null))
        {
            Debug.LogError("[VillageMap] Tile 에셋 로드 실패 — Assets/03.Tiles 확인 필요");
            return;
        }
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cam.tag = "MainCamera";
        var camera = cam.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 14f;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(0x22, 0x38, 0x22, 0xFF);

        var lightGo = new GameObject("Global Light 2D", typeof(Light2D));
        lightGo.GetComponent<Light2D>().lightType = Light2D.LightType.Global;

        var gridGo = new GameObject("Grid", typeof(Grid));
        Tilemap groundMap = NewLayer(gridGo, "Ground", -1000);
        Tilemap waterMap = NewLayer(gridGo, "Water", -995);
        Tilemap roadMap = NewLayer(gridGo, "Road", -990);
        Tilemap farmMap = NewLayer(gridGo, "Farm", -985);
        var rand = new System.Random(23);

        int gx0 = WL - 8, gx1 = ER + 8, gy0 = SB - 8, gy1 = NT + 8;
        for (int x = gx0; x <= gx1; x++)
        for (int y = gy0; y <= gy1; y++)
        {
            int id = rand.NextDouble() < 0.85 ? 96 : GrassIds[rand.Next(1, GrassIds.Length)];
            groundMap.SetTile(new Vector3Int(x, y, 0), ground[id]);
        }

        // 강 (동쪽, 성벽 관통)
        var river = new HashSet<Vector2Int>();
        AddRect(river, RiverW, gy0, RiverE, gy1);
        foreach (var c in river)
            waterMap.SetTile(new Vector3Int(c.x, c.y, 0), water[PickWang(WaterWang, river, c, rand)]);

        // ── 길 (모든 건물 줄 아래에 거리가 오도록 4단 구성) ──
        var cells = new HashSet<Vector2Int>();
        AddRect(cells, -6, -3, 6, 3);        // 중앙 광장 (넓게)
        AddRect(cells, -33, 0, 33, 1);       // 동서 대로
        AddRect(cells, -1, SB, 0, 7);        // 남북 대로 (남문 ~ 성당 앞)
        AddRect(cells, -6, 7, 6, 8);         // 성당 앞마당
        AddRect(cells, -30, 8, 21, 9);       // 북측 거리 (윗줄 건물들의 문 앞)
        AddRect(cells, -30, -9, 21, -8);     // 남측 거리 1
        AddRect(cells, -30, -18, 21, -17);   // 남측 거리 2 (최하단 건물들의 문 앞)
        AddRect(cells, -14, -6, 14, -5);     // 시장 거리 (광장 남쪽, 넓게)
        int[] connX = { -28, -16, 12, 18 };
        foreach (int cx in connX) AddRect(cells, cx, -17, cx + 1, 8);
        AddRect(cells, -23, 9, -22, NT);     // 북서 던전문 길
        AddRect(cells, 20, 9, 21, NT);       // 북동 던전문 길
        foreach (var c in cells)
            if (!river.Contains(c))
                roadMap.SetTile(new Vector3Int(c.x, c.y, 0), road[PickWang(RoadWang, cells, c, rand)]);

        var env = new GameObject("Environment").transform;
        var labels = new GameObject("Labels").transform;
        var occ = new HashSet<Vector2Int>(cells);
        occ.UnionWith(river);

        BuildWalls(env, rand, occ);

        // ── 대성당 단지 (북쪽 중앙 랜드마크) ──
        Facility(env, labels, occ, font, "Buildings/House_RedWood_8_3.png", 0, 10, "성당", null, null);
        PutP(env, "Buildings/Tower_RedWood_1_2.png", -8, 10);
        PutP(env, "Buildings/Tower_RedWood_1_2.png", 8, 10);
        MarkArea(occ, -8, 10, 3, 15); MarkArea(occ, 8, 10, 3, 15);
        PutP(env, "Props/Banner_Stick_1_Red.png", -5.4f, 7.4f);
        PutP(env, "Props/Banner_Stick_1_Red.png", 5.4f, 7.4f);

        // ── 텃밭 2곳 (주택 채우기 전에 자리 확보, 세로 연결길 피해서) ──
        MakePlot(farmMap, farm, env, occ, rand, -26, -7, -21, -3);
        MakePlot(farmMap, farm, env, occ, rand, 15, -7, 17, -3);

        // ── 광장 (NPC가 모이는 중심 공간) — 대형 분수가 중심 ──
        var fountainSprites = AssetDatabase.LoadAllAssetsAtPath(
            PremRoot + "Props/Animations/Animation_Fountain_1.png").OfType<Sprite>().OrderBy(s => s.name.Length).ThenBy(s => s.name).ToArray();
        if (fountainSprites.Length > 0)
        {
            var fgo = new GameObject("Fountain", typeof(SpriteRenderer), typeof(SpriteCycler));
            fgo.transform.position = new Vector3(0, -2.6f, 0);
            var fsr = fgo.GetComponent<SpriteRenderer>();
            fsr.sprite = fountainSprites[0];
            fsr.sortingOrder = Mathf.RoundToInt(2.6f * 10);
            var so = new SerializedObject(fgo.GetComponent<SpriteCycler>());
            var arr = so.FindProperty("frames");
            arr.arraySize = fountainSprites.Length;
            for (int i = 0; i < fountainSprites.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = fountainSprites[i];
            so.FindProperty("fps").floatValue = 6f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        PutP(env, "Props/StreetArch_Red.png", 0, 3.6f);           // 광장 북쪽 입구 아치
        PutP(env, "Props/StreetArch_Red.png", 0, -4.4f);          // 광장 남쪽 입구 아치
        PutP(env, "Props/StreetDecoration_1_Red.png", -5.2f, 2.6f);
        PutP(env, "Props/StreetDecoration_1_Red.png", 5.2f, 2.6f);
        PutP(env, "Props/StreetDecoration_2_Red.png", -5.2f, -3.6f);
        PutP(env, "Props/StreetDecoration_2_Red.png", 5.2f, -3.6f);
        PutP(env, "Props/Banner_Stick_1_Red.png", -6.3f, 3.2f);
        PutP(env, "Props/Banner_Stick_1_Red.png", 6.3f, 3.2f);
        PutP(env, "Props/Banner_Stick_1_Red.png", -6.3f, -3.8f);
        PutP(env, "Props/Banner_Stick_1_Red.png", 6.3f, -3.8f);
        PutP(env, "Props/Bench_1.png", -3.4f, -2.6f);
        PutP(env, "Props/Bench_3.png", 3.4f, -2.6f);
        PutP(env, "Props/Bench_1.png", -3.4f, 1.4f);
        PutP(env, "Props/Bench_3.png", 3.4f, 1.4f);
        PutP(env, "Props/BulletinBoard_1.png", -4.8f, 3.4f);
        PutP(env, "Props/Vase_1.png", 4.8f, 3.2f);
        PutP(env, "Props/LampPost_3.png", -6.6f, 0.6f);
        PutP(env, "Props/LampPost_3.png", 6.6f, 0.6f);

        // ── 강변 낚시터 (강 동쪽 죽은 공간 살리기) ──
        PutP(env, "Props/FishRack_1.png", 29, 3);
        PutP(env, "Props/FishRack_2.png", 31.4f, 3.6f);
        PutP(env, "Props/Boat_Sand.png", 28, -3.4f);
        PutP(env, "Props/FishingNet_Exterior.png", 30.4f, -5);
        PutP(env, "Props/Barrel_Small_Fish.png", 28.6f, -6);
        PutP(env, "Props/Crate_Small_Fish.png", 31.6f, -6.6f);
        PutP(env, "Props/Bench_2.png", 30, 6.4f);
        PutP(env, "Props/TreeStump_Lantern_1.png", 29, 10);
        PutP(env, "Props/Lantern_1.png", 28.2f, -0.9f);
        PutP(env, "Props/Sack_1.png", 32.2f, 8);
        float[] catL = { -14, -7, 5, 12, 17, NT + 2, NT + 4.5f, SB - 2, SB - 4.5f };
        float[] catR = { -11, -2, 8, 15, -16, NT + 3.2f, NT + 5.5f, SB - 3.2f, SB - 5.5f };
        for (int i = 0; i < catL.Length; i++)
        {
            PutP(env, $"Props/Cattail_{i % 2 + 1}.png", 23.4f + (float)(rand.NextDouble() * 0.4), catL[i]);
            PutP(env, $"Props/Cattail_{(i + 1) % 2 + 1}.png", 27.5f + (float)(rand.NextDouble() * 0.4), catR[i]);
        }

        // ── 성벽-강 접점 안팎 채우기 ──
        // 성벽 안쪽 (강 서쪽 좁은 띠 + 강 동쪽 북부)
        PutF(env, "Trees and Bushes/Bush_Emerald_2.png", 22.6f, 17.2f);
        PutF(env, "Trees and Bushes/Bush_Emerald_5.png", 22.7f, 13.6f);
        PutF(env, "Trees and Bushes/Bush_Emerald_3.png", 28.2f, 17.6f);
        PutF(env, "Trees and Bushes/Bush_Emerald_1.png", 32.6f, 14);
        PutF(env, "Trees and Bushes/Bush_Emerald_6.png", 28.4f, 12.8f);
        PutP(env, "Rocks/Rock_Brown_2.png", 32.2f, 17.2f);
        PutP(env, "Props/Plant_Sunflower_2.png", 29.6f, 15.6f);
        // 성벽 안쪽 남부 (강-남벽 접점)
        PutF(env, "Trees and Bushes/Bush_Emerald_4.png", 22.6f, -18.6f);
        PutF(env, "Trees and Bushes/Bush_Emerald_2.png", 28.3f, -18.2f);
        PutP(env, "Rocks/Rock_Brown_6.png", 31.8f, -17.6f);
        PutP(env, "Props/TreeStump_Small.png", 29.4f, -16.4f);
        // 성벽 바깥쪽 접점 (숲 사이 빈 틈)
        PutF(env, "Trees and Bushes/Bush_Emerald_3.png", 22.8f, NT + 1.2f);
        PutF(env, "Trees and Bushes/Bush_Emerald_1.png", 28.1f, NT + 1.4f);
        PutF(env, "Trees and Bushes/Bush_Emerald_5.png", 22.8f, SB - 1.2f);
        PutF(env, "Trees and Bushes/Bush_Emerald_7.png", 28.1f, SB - 1.4f);
        PutP(env, "Rocks/Rock_Brown_1.png", 28.6f, NT + 2.6f);
        PutP(env, "Rocks/Rock_Brown_4.png", 22.4f, SB - 3);

        // ── 시장 거리 (가판대 8개 + 상품 소품) ──
        string[] standC = { "Red", "White", "Red", "White", "Red", "White", "Red", "White" };
        for (int i = 0; i < 8; i++)
            PutP(env, $"Buildings/MarketStand_2_{standC[i]}.png", -14 + i * 4, -7.9f);
        PutP(env, "Props/WoodCart_Flowers.png", 16.6f, -6.8f);
        PutP(env, "Props/WoodCart_Hay.png", -16.6f, -6.8f);
        PutP(env, "Props/Crate_Small_Vases_1.png", -15.2f, -4.6f);
        PutP(env, "Props/Basket_Apples.png", -12.4f, -4.4f);
        PutP(env, "Props/Basket_Bread_2.png", -8.4f, -4.4f);
        PutP(env, "Props/DyedCloth_Medium_Red.png", -4.4f, -4.5f);
        PutP(env, "Props/Basket_Vegetables.png", 4.4f, -4.4f);
        PutP(env, "Props/Sack_Cotton_1.png", 8.4f, -4.5f);
        PutP(env, "Props/Basket_Corn.png", 12.4f, -4.4f);
        PutP(env, "Props/Barrel_Medium_Fish.png", 15.4f, -4.6f);

        // ── 북측 상가 ──
        Facility(env, labels, occ, font, "Buildings/House_RedWood_4_2.png", -27, 10, "여관", "Props/Sign_Job_Tavern.png", "Props/Barrel_Horizontal_1.png");
        Facility(env, labels, occ, font, "Buildings/House_RedWood_4_1.png", -15, 10, "대장간", "Props/Sign_Job_Blacksmith.png", "Props/Anvil.png");
        Facility(env, labels, occ, font, "Buildings/House_RedWood_2_1.png", 13, 10, "무기점", "Props/Sign_Job_Swordsmith.png", "Props/WeaponsStand_1.png");
        Facility(env, labels, occ, font, "Buildings/House_RedWood_3_1.png", 20, 10, "도서관", "Props/Sign_Job_Librarian.png", null);

        // ── 남측 상가 (문 앞 = 남측 거리 2) ──
        Facility(env, labels, occ, font, "Buildings/House_RedWood_2_1.png", -26, -16, "방어구점", "Props/Sign_Job_ArmourVendor.png", "Props/ArmorStand_Red.png");
        Facility(env, labels, occ, font, "Buildings/House_RedWood_2_3.png", -13, -16, "병원", "Props/Sign_Job_Empty.png", "Props/Plant_RoseBush_1_Red.png");
        Facility(env, labels, occ, font, "Buildings/House_RedWood_2_1.png", 14, -16, "잡화점", "Props/Sign_Job_Empty.png", "Props/WoodCart_1.png");

        // ── 플레이어 집 (남문 바로 옆) ──
        Facility(env, labels, occ, font, "Buildings/House_RedWood_1_4.png", -6, -16, "내 집", null, "Props/Basket_Apples.png");
        Facility(env, labels, occ, font, "Buildings/House_RedWood_1_1.png", 6, -16, "활방", "Props/Sign_Job_Fletcher.png", "Props/ArcheryTarget_Red.png");

        // ── 다리 (동서 대로가 강을 건너는 곳) ──
        PutP(env, "Buildings/Bridge_Stone_5.png", 25, -0.6f);

        // ── 주택 밴드 채우기: 집 아래가 반드시 길이 되도록 거리 윗줄에만 배치 ──
        FillBand(env, occ, rand, -16, WL + 3, 21, false); // 남측 거리 2 윗줄
        FillBand(env, occ, rand, -7, WL + 3, -16, true);  // 남측 거리 1 윗줄 (서쪽, 작은 집)
        FillBand(env, occ, rand, -7, 16, 21, true);       // 남측 거리 1 윗줄 (동쪽, 작은 집)
        FillBand(env, occ, rand, 2, WL + 3, -9, true);    // 대로 윗줄 (서쪽, 작은 집)
        FillBand(env, occ, rand, 2, 9, 21, true);         // 대로 윗줄 (동쪽, 작은 집)
        FillBand(env, occ, rand, 10, WL + 3, 21, false);  // 북측 거리 윗줄

        // ── 생활 소품 스캐터 ──
        string[] details = {
            "Props/HayStack_1.png", "Props/HayStack_2.png", "Props/Barrel_Small_Empty.png",
            "Props/Crate_Small_Empty.png", "Props/Sack_3.png", "Props/WoodStack_1.png",
            "Props/Plant_Pumpkin_1.png", "Props/Plant_RoseBush_1_Red.png", "Props/Plant_Sunflower_1.png",
            "Props/Vase_2.png", "Props/TreeStump_Small.png", "Props/Woodcutter_Logs_1.png",
            "Props/WoodCart_Hay.png", "Props/Crate_Small_Flowers_Red.png", "Props/Bench_2.png",
            "Props/Lantern_1.png", "Props/Beehive_1.png", "Props/Basket_Bread_1.png" };
        for (int i = 0; i < 130; i++)
        {
            int bx = rand.Next(WL + 2, 23), by = rand.Next(SB + 2, NT - 1);
            if (!AreaClear(occ, bx, by, 1, 1)) continue;
            PutP(env, details[rand.Next(details.Length)], bx + (float)(rand.NextDouble() - 0.5), by);
            occ.Add(new Vector2Int(bx, by));
        }

        // ── 도시 안 나무 ──
        for (int i = 0; i < 26; i++)
        {
            int bx = rand.Next(WL + 2, 23), by = rand.Next(SB + 2, NT - 2);
            if (!AreaClear(occ, bx, by, 1, 2)) continue;
            PutF(env, Tree(rand), bx + (float)(rand.NextDouble() - 0.5), by);
            MarkArea(occ, bx, by, 1, 2);
        }

        // ── 성벽 밖 숲 ──
        for (int layer = 0; layer < 4; layer++)
        {
            float step = 1.6f + layer * 0.2f;
            for (float x = WL - 7; x <= ER + 7; x += step)
            {
                float jx = x + (float)(rand.NextDouble() - 0.5) * 1.2f;
                bool riverGap = jx > RiverW - 0.5f && jx < RiverE + 1.5f; // 강 폭에 딱 맞게
                bool southGate = jx > -3.5f && jx < 3.5f;
                bool nwGate = jx > -25.5f && jx < -19.5f;
                bool neGate = jx > 18.5f && jx < 24.5f;
                if (!riverGap && !nwGate && !neGate)
                    PutF(env, Tree(rand), jx, NT + 2.5f + layer * 1.8f + (float)rand.NextDouble());
                if (!riverGap && !southGate)
                    PutF(env, Tree(rand), jx, SB - 2.5f - layer * 1.8f - (float)rand.NextDouble());
            }
            for (float y = SB - 5; y <= NT + 5; y += step)
            {
                float jy = y + (float)(rand.NextDouble() - 0.5) * 1.2f;
                PutF(env, Tree(rand), WL - 2.5f - layer * 1.8f - (float)rand.NextDouble(), jy);
                PutF(env, Tree(rand), ER + 2.5f + layer * 1.8f + (float)rand.NextDouble(), jy);
            }
        }

        EditorApplication.delayCall += () =>
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            Debug.Log("[VillageMap] 레드릿지 성곽도시 생성 완료 → " + ScenePath);
        };
    }

    // ── 성벽 + 망루 + 게이트 ──────────────────────────
    private static void BuildWalls(Transform env, System.Random rand, HashSet<Vector2Int> occ)
    {
        var wall = new GameObject("Walls").transform;
        wall.SetParent(env, false);

        var gapN = new HashSet<int>();
        for (int x = -25; x <= -20; x++) gapN.Add(x);   // 북서 던전문
        for (int x = 19; x <= 24; x++) gapN.Add(x);     // 북동 던전문
        for (int x = RiverW; x <= RiverE; x++) gapN.Add(x);
        var gapS = new HashSet<int>();
        for (int x = -3; x <= 3; x++) gapS.Add(x);      // 남문(정문)
        for (int x = RiverW; x <= RiverE; x++) gapS.Add(x);

        for (int x = WL + 1; x <= ER - 1; x++)
        {
            if (!gapN.Contains(x)) PutW(wall, $"Fences and Walls/CityWall_Up_{rand.Next(1, 10)}.png", x, NT);
            if (!gapS.Contains(x)) PutW(wall, $"Fences and Walls/CityWall_Down_{rand.Next(1, 10)}.png", x, SB);
        }
        PutW(wall, "Fences and Walls/CityWall_Up_River_1.png", RiverW + 1.5f, NT);
        PutW(wall, "Fences and Walls/CityWall_Down_River_1.png", RiverW + 1.5f, SB);

        for (int y = SB + 1; y <= NT - 1; y++)
        {
            PutW(wall, "Fences and Walls/CityWall_Left_1.png", WL, y);
            PutW(wall, "Fences and Walls/CityWall_Right_1.png", ER, y);
        }
        PutW(wall, "Fences and Walls/CityWall_UpLeft_1.png", WL, NT);
        PutW(wall, "Fences and Walls/CityWall_UpRight_1.png", ER, NT);
        PutW(wall, "Fences and Walls/CityWall_DownLeft_1.png", WL, SB);
        PutW(wall, "Fences and Walls/CityWall_DownRight_1.png", ER, SB);

        // 게이트 아치
        PutW(wall, "Fences and Walls/CityWall_Gate_1.png", -22, NT);
        PutW(wall, "Fences and Walls/CityWall_Gate_1.png", 21, NT);
        PutW(wall, "Fences and Walls/CityWall_Gate_1.png", 0, SB);

        // 모서리 망루
        PutP(env, "Buildings/Watchtower_1_RedWood_Red.png", WL + 3, NT - 3);
        PutP(env, "Buildings/Watchtower_1_RedWood_Red.png", ER - 3, NT - 3);
        PutP(env, "Buildings/Watchtower_1_RedWood_Red.png", WL + 3, SB + 1);
        PutP(env, "Buildings/Watchtower_1_RedWood_Red.png", ER - 3, SB + 1);
        MarkArea(occ, WL + 3, NT - 3, 3, 10);
        MarkArea(occ, ER - 3, NT - 3, 3, 10);
        MarkArea(occ, WL + 3, SB + 1, 3, 10);
        MarkArea(occ, ER - 3, SB + 1, 3, 10);

        // 던전 안내판
        PutP(env, "Props/Sign_1_North.png", -19, NT - 1);
        PutP(env, "Props/Sign_1_North.png", 24, NT - 1);
    }

    private static void Facility(Transform env, Transform labels, HashSet<Vector2Int> occ, TMP_FontAsset font,
        string houseRel, float x, float y, string label, string signRel, string propRel)
    {
        var sprite = PutP(env, houseRel, x, y);
        float h = sprite != null ? sprite.bounds.size.y : 7f;
        if (signRel != null) PutP(env, signRel, x - 3.2f, y + 0.2f);
        if (propRel != null) PutP(env, propRel, x + 3.2f, y + 0.2f);

        var lgo = new GameObject("Label_" + label, typeof(TextMeshPro));
        lgo.transform.SetParent(labels, false);
        lgo.transform.position = new Vector3(x, y + h + 0.5f, 0);
        var tmp = lgo.GetComponent<TextMeshPro>();
        if (font != null) tmp.font = font;
        tmp.text = label;
        tmp.fontSize = 4.5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color32(255, 240, 205, 255);
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.rectTransform.sizeDelta = new Vector2(24, 3);
        lgo.GetComponent<MeshRenderer>().sortingOrder = 6000;

        MarkArea(occ, Mathf.RoundToInt(x), Mathf.RoundToInt(y), 4, Mathf.CeilToInt(h));
    }

    private static string House(System.Random rand, bool shortOnly = false)
    {
        if (shortOnly) return $"Buildings/House_RedWood_1_{rand.Next(1, 6)}.png"; // 낮은 집만
        int si = rand.Next(HouseShapes.Length);
        int shape = HouseShapes[si];
        int variant = rand.Next(1, ShapeVariants[si] + 1);
        return $"Buildings/House_RedWood_{shape}_{variant}.png";
    }

    // 텃밭: 밭 타일 깔고 농작물 심기
    private static void MakePlot(Tilemap farmMap, Dictionary<int, TileBase> farm, Transform env,
        HashSet<Vector2Int> occ, System.Random rand, int x0, int y0, int x1, int y1)
    {
        var plot = new HashSet<Vector2Int>();
        AddRect(plot, x0, y0, x1, y1);
        foreach (var c in plot)
            farmMap.SetTile(new Vector3Int(c.x, c.y, 0), farm[PickWang(FarmWang, plot, c, rand)]);

        string[] crops = { "Props/Plant_Pumpkin_1.png", "Props/Plant_Carrot_1.png",
                           "Props/Plant_Corn_1.png", "Props/Plant_Radish_1.png", "Props/Plant_Cabbage_1.png" };
        int row = 0;
        for (int y = y0; y <= y1; y += 2, row++)
        for (int x = x0; x <= x1; x += 2)
            PutP(env, crops[(row + (x - x0) / 2) % crops.Length], x + 0.5f, y + 0.1f);

        MarkArea(occ, (x0 + x1) / 2, y0 - 1, (x1 - x0) / 2 + 2, y1 - y0 + 3);
    }

    // 거리 윗줄을 실제 스프라이트 폭 기준으로 빈틈없이 채운다 (문 앞은 항상 길)
    private static void FillBand(Transform env, HashSet<Vector2Int> occ, System.Random rand,
        int bottomY, int x0, int x1, bool shortOnly)
    {
        int x = x0;
        int guard = 0;
        while (x <= x1 && guard++ < 500)
        {
            string rel = House(rand, shortOnly);
            var spr = LoadPrem(rel);
            if (spr == null) { x += 2; continue; }
            int w = Mathf.CeilToInt(spr.bounds.size.x);
            int h = Mathf.CeilToInt(spr.bounds.size.y);
            int half = w / 2 + 1;
            int bx = x + half;
            if (bx + half > x1) break;
            if (AreaClear(occ, bx, bottomY, half, h))
            {
                Put(env, spr, rel, bx, bottomY);
                MarkArea(occ, bx, bottomY, half, h);
                x += w + 2;
            }
            else x += 2;
        }
    }

    private static string Tree(System.Random rand) =>
        $"Trees and Bushes/Tree_Emerald_{rand.Next(1, 5)}.png";

    // cy = 건물 바닥, h = 높이. 바닥 아래(cy-1)는 길이므로 검사에서 제외한다.
    private static bool AreaClear(HashSet<Vector2Int> occ, int cx, int cy, int halfW, int h)
    {
        for (int x = cx - halfW; x <= cx + halfW; x++)
        for (int y = cy; y <= cy + h - 1; y++)
            if (occ.Contains(new Vector2Int(x, y))) return false;
        return true;
    }

    private static void MarkArea(HashSet<Vector2Int> occ, int cx, int cy, int halfW, int h)
    {
        for (int x = cx - halfW; x <= cx + halfW; x++)
        for (int y = cy; y <= cy + h - 1; y++)
            occ.Add(new Vector2Int(x, y));
    }

    private static Tilemap NewLayer(GameObject grid, string name, int order)
    {
        var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        go.transform.SetParent(grid.transform, false);
        go.GetComponent<TilemapRenderer>().sortingOrder = order;
        return go.GetComponent<Tilemap>();
    }

    private static void AddRect(HashSet<Vector2Int> set, int x1, int y1, int x2, int y2)
    {
        for (int x = x1; x <= x2; x++)
        for (int y = y1; y <= y2; y++)
            set.Add(new Vector2Int(x, y));
    }

    private static int PickWang(Dictionary<string, int[]> wang, HashSet<Vector2Int> s, Vector2Int c, System.Random rand)
    {
        bool top = s.Contains(c + Vector2Int.up), bot = s.Contains(c + Vector2Int.down);
        bool left = s.Contains(c + Vector2Int.left), right = s.Contains(c + Vector2Int.right);
        bool tr = top && right && s.Contains(c + new Vector2Int(1, 1));
        bool br = bot && right && s.Contains(c + new Vector2Int(1, -1));
        bool bl = bot && left && s.Contains(c + new Vector2Int(-1, -1));
        bool tl = top && left && s.Contains(c + new Vector2Int(-1, 1));
        string key = $"{B(top)},{B(tr)},{B(right)},{B(br)},{B(bot)},{B(bl)},{B(left)},{B(tl)}";
        if (!wang.TryGetValue(key, out int[] options))
        {
            int best = int.MaxValue;
            foreach (var kv in wang)
            {
                int diff = key.Where((ch, i) => ch != kv.Key[i]).Count();
                if (diff < best) { best = diff; options = kv.Value; }
            }
        }
        // 무늬 변형 타일은 낮은 확률로만 사용 → 격자 반복감 감소
        if (options.Length > 1 && rand.NextDouble() < 0.78) return options[0];
        return options[rand.Next(options.Length)];
    }

    private static string B(bool v) => v ? "1" : "0";

    private static Sprite PutP(Transform parent, string premRel, float x, float y) => Put(parent, LoadPrem(premRel), premRel, x, y);
    private static Sprite PutF(Transform parent, string freeRel, float x, float y) => Put(parent, AssetDatabase.LoadAssetAtPath<Sprite>(FreeRoot + freeRel), freeRel, x, y);
    private static Sprite PutW(Transform parent, string premRel, float x, float y) => Put(parent, LoadPrem(premRel), premRel, x, y);

    private static Sprite Put(Transform parent, Sprite sprite, string rel, float x, float y)
    {
        if (sprite == null) { Debug.LogWarning("[VillageMap] 스프라이트 없음: " + rel); return null; }
        var go = new GameObject(sprite.name, typeof(SpriteRenderer));
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = Mathf.RoundToInt(-y * 10);
        return sprite;
    }
}
