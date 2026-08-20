using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Tools > Build Arena Room
//
// 레퍼런스 스크린샷(Cute Fantasy Dungeon 홍보 이미지)과 같은 구조로 방 두 개를 만든다.
//   왼쪽 = 웨이브 아레나 / 가운데 통로 / 오른쪽 = 보스방
//
// 벽은 세 층으로 쌓아 높이가 보이게 한다.
//   ① 윗면(fill)  ② 바깥 테두리(rim)  ③ 방을 향한 벽면(face) + 바닥 그림자(shadow)
public static class ArenaRoomBuilder
{
    private const string Sheet = "Assets/_Graphics/Asset/Cute_Fantasy_Dungeons/Dungeon_2/Dungeon_2.png";
    private const string ArchSheet = "Assets/_Graphics/Asset/Cute_Fantasy_Dungeons/Dungeon_2/Dungeon_2_Arch_small.png";
    private const string PropSheet = "Assets/_Graphics/Asset/Cute_Fantasy_Dungeons/Objects/Dungeon_Objects.png";
    private const string TileDir = "Assets/_Graphics/Tiles/Dungeon";

    private const int Cell = 16;
    private const int PPU = 16;

    // 아레나 복합체의 좌하단 (마을 맵에서 멀리 떨어뜨린다)
    private static readonly Vector2Int Origin = new Vector2Int(200, 0);

    // ── 방 치수 (레퍼런스 비율) ──────────────────────────────
    private const int W = 40, H = 24;              // 전체
    private const int LeftX0 = 3, LeftX1 = 17;     // 아레나 내부
    private const int DivX0 = 18, DivX1 = 21;      // 가운데 벽
    private const int RightX0 = 22, RightX1 = 36;  // 보스방 내부
    private const int InY0 = 3, InY1 = 19;         // 내부 세로
    private const int DoorY0 = 10, DoorY1 = 12;    // 통로

    // ── Dungeon_2 시트 좌표 (좌상단 기준) ────────────────────
    //
    // 시트의 밝기는 세 단계로 나뉜다. 이걸 그대로 역할에 대응시킨다.
    //   밝음(89) 10~12열  → 바닥
    //   중간(67) 4~6열 6~7행 → 벽 몸통 (벽돌)
    //   어두움(58) 4~6열 0~2행 → 벽 가장자리 테두리
    // 바닥과 벽에 같은 타일을 쓰면 대비가 없어서 평면으로 보인다.
    private static readonly Vector2Int[] Fill =
    {
        new Vector2Int(10,0), new Vector2Int(11,0), new Vector2Int(12,0),
        new Vector2Int(10,1), new Vector2Int(11,1), new Vector2Int(12,1),
        new Vector2Int(10,2), new Vector2Int(11,2), new Vector2Int(12,2),
        new Vector2Int(10,5), new Vector2Int(11,5), new Vector2Int(12,5),
        new Vector2Int(10,6), new Vector2Int(11,6), new Vector2Int(12,6),
        new Vector2Int(10,7), new Vector2Int(11,7), new Vector2Int(12,7),
    };
    // 8방향 테두리 (돌 테두리)
    private static readonly Vector2Int RimTL = new Vector2Int(4, 0), RimT = new Vector2Int(5, 0), RimTR = new Vector2Int(6, 0);
    private static readonly Vector2Int RimL = new Vector2Int(4, 1), RimR = new Vector2Int(6, 1);
    private static readonly Vector2Int RimBL = new Vector2Int(4, 2), RimB = new Vector2Int(5, 2), RimBR = new Vector2Int(6, 2);
    // 벽 몸통 (중간 밝기 벽돌). 바닥보다 어두워야 벽으로 읽힌다.
    private static readonly Vector2Int[] WallBrick =
    {
        new Vector2Int(4,6), new Vector2Int(5,6), new Vector2Int(6,6),
        new Vector2Int(4,7), new Vector2Int(5,7), new Vector2Int(6,7),
    };
    // 벽면 아래 바닥에 깔리는 반투명 그림자
    private static readonly Vector2Int[] Shadow =
    { new Vector2Int(10,4), new Vector2Int(11,4), new Vector2Int(12,4) };
    // 장식용 난간 프레임 (가운데가 비어있는 3×3)
    private static readonly Vector2Int RailTL = new Vector2Int(4, 9), RailT = new Vector2Int(5, 9), RailTR = new Vector2Int(6, 9);
    private static readonly Vector2Int RailL = new Vector2Int(4, 10), RailR = new Vector2Int(6, 10);
    private static readonly Vector2Int RailBL = new Vector2Int(4, 11), RailB = new Vector2Int(5, 11), RailBR = new Vector2Int(6, 11);

    // ── 소품 (Dungeon_Objects.png, 좌상단 기준 픽셀 사각형) ──
    // 시트가 격자에 맞지 않아 픽셀로 직접 잘라낸다.
    private struct Prop { public string name; public int x, y, w, h; }
    private static Prop P(string n, int x, int y, int w, int h) => new Prop { name = n, x = x, y = y, w = w, h = h };

    private static readonly Prop[] Props =
    {
        P("Barrel",          19,  3, 11, 12),
        P("Bone_Big",        34,  4, 12,  8),
        P("Sword_Stand",     65,  0, 15, 16),
        P("Crate",            1, 14, 15, 17),
        P("Crate_Tall",      49, 12, 15, 19),
        P("Pot_Brown_Wide",  81, 14, 14, 16),
        P("Table_Stone",     97, 12, 15, 19),
        P("Cobweb_Top",     113,  8, 15, 15),
        P("Table_Stone_2",  130, 12, 13, 19),
        P("Bone_S1",         67, 23,  6,  6),
        P("Bone_S2",         72, 18,  7,  6),
        P("Candle_Tall",     22, 28,  6, 19),
        P("Candelabra",      34, 27, 13, 20),
        P("Candle_Small",     5, 35,  5, 10),
        P("Rock_1",          50, 35, 12, 11),
        P("Rock_2",          64, 35, 16, 11),
        P("Pot_Brown_Small", 83, 33, 10, 13),
        P("Pot_Grey_Wide",   97, 46, 14, 16),
        P("Cobweb_Corner",  112, 40, 12, 18),
        P("Winged_Statue",    6, 48, 34, 48),
        P("Rubble",          35, 53,  9,  8),
        P("Altar_Gem",       54, 48, 21, 16),
        P("Pot_Brown_Tall",  83, 61, 10, 17),
        P("Cross_Grave",     48, 69, 16, 26),
        P("Bench_1",        113, 66, 15, 14),
        P("Tombstone",       65, 74, 15, 21),
        P("Pot_Grey_Tall",   99, 77, 10, 17),
        P("Pot_Grey_Small",  83, 81, 10, 13),
        P("Bench_2",        112, 82, 16, 14),
    };

    // ── 스테인드글라스 (Dungeon_2_Arch_small.png) ────────────
    private static readonly Prop[] ArchProps =
    {
        P("Window_Red",   80,  0, 16, 32),
        P("Window_Blue",  96,  0, 16, 32),
        P("Light_Red",    80, 32, 16, 32),
        P("Light_Blue",   96, 32, 16, 32),
        P("Colonnade",     0,  0, 80, 32),
    };

    [MenuItem("Tools/Build Arena Room")]
    public static void Build()
    {
        var tiles = SliceTiles();
        if (tiles == null) return;
        var props = SliceProps(PropSheet, Props);
        var arch = SliceProps(ArchSheet, ArchProps);

        GameObject level = GameObject.Find("Level") ?? new GameObject("Level");
        Transform grid = level.transform.Find("Grid");
        if (grid == null)
        {
            var g = new GameObject("Grid", typeof(Grid));
            g.transform.SetParent(level.transform, false);
            grid = g.transform;
        }

        // 소품은 타일맵과 "같은" 소팅 레이어에 올려야 order 비교가 통한다.
        // 레이어가 다르면 order 를 아무리 높여도 타일맵 뒤로 묻힌다.
        Tilemap ground = GetOrCreateTilemap(grid, "GroundTile", 0);
        _sortLayerId = ground.GetComponent<TilemapRenderer>().sortingLayerID;

        Tilemap shadow = GetOrCreateTilemap(grid, "ShadowTile", 1);
        Tilemap wall = GetOrCreateTilemap(grid, "Wall Tile", 2);
        SetupWallCollider(wall);
        Debug.Log($"[Arena] 대상 → {Path(ground.transform)} / {Path(wall.transform)} / " +
                  $"소팅 레이어 = \"{SortingLayer.IDToName(_sortLayerId)}\"");

        ground.ClearAllTiles();
        shadow.ClearAllTiles();
        wall.ClearAllTiles();

        bool[,] isWall = BuildMask();
        PaintTiles(ground, shadow, wall, tiles, isWall);
        PlaceProps(level.transform, props, arch);

        EditorApplication.delayCall += () =>
        {
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log($"[Arena] 완료 — {W}x{H} @{Origin} (아레나 + 통로 + 보스방)");
        };
    }

    // ── 벽 마스크 ────────────────────────────────────────────
    private static bool[,] BuildMask()
    {
        var wall = new bool[W, H];
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            bool inLeft = x >= LeftX0 && x <= LeftX1 && y >= InY0 && y <= InY1;
            bool inRight = x >= RightX0 && x <= RightX1 && y >= InY0 && y <= InY1;
            bool inDoor = x >= DivX0 && x <= DivX1 && y >= DoorY0 && y <= DoorY1;
            wall[x, y] = !(inLeft || inRight || inDoor);
        }
        return wall;
    }

    private static void PaintTiles(Tilemap ground, Tilemap shadow, Tilemap wallMap,
        Dictionary<Vector2Int, Tile> t, bool[,] isWall)
    {
        var rand = new System.Random(2026);
        bool IsW(int x, int y) => x >= 0 && x < W && y >= 0 && y < H && isWall[x, y];

        // ① 바닥은 밝은 타일, 벽 몸통은 어두운 벽돌 — 대비가 높이를 만든다
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            if (isWall[x, y]) wallMap.SetTile(Pos(x, y), t[WallBrick[rand.Next(WallBrick.Length)]]);
            else ground.SetTile(Pos(x, y), t[Fill[rand.Next(Fill.Length)]]);
        }

        // ② 벽 덩어리의 경계에 돌 테두리
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            if (!isWall[x, y]) continue;
            bool up = IsW(x, y + 1), dn = IsW(x, y - 1), lf = IsW(x - 1, y), rt = IsW(x + 1, y);
            Vector2Int? rim = null;
            if (!up && !lf) rim = RimTL;
            else if (!up && !rt) rim = RimTR;
            else if (!dn && !lf) rim = RimBL;
            else if (!dn && !rt) rim = RimBR;
            else if (!up) rim = RimT;
            else if (!dn) rim = RimB;
            else if (!lf) rim = RimL;
            else if (!rt) rim = RimR;
            if (rim.HasValue) wallMap.SetTile(Pos(x, y), t[rim.Value]);
        }

        // ③ 벽 아래쪽(방을 향한 면) 바닥에 그림자를 깐다. 벽이 떠 보이게 하는 핵심.
        for (int x = 0; x < W; x++)
        for (int y = 1; y < H; y++)
        {
            if (!isWall[x, y] || IsW(x, y - 1)) continue;

            int side = 1;                       // 0=좌끝 1=중간 2=우끝
            if (!IsW(x - 1, y)) side = 0;
            else if (!IsW(x + 1, y)) side = 2;

            shadow.SetTile(Pos(x, y - 1), t[Shadow[side]]);
        }

        // ④ 보스방 가운데 난간 (장식용)
        PaintRail(ground, t, 26, 9, 32, 13);
    }

    private static void PaintRail(Tilemap map, Dictionary<Vector2Int, Tile> t, int x0, int y0, int x1, int y1)
    {
        for (int x = x0; x <= x1; x++)
        for (int y = y0; y <= y1; y++)
        {
            bool l = x == x0, r = x == x1, b = y == y0, u = y == y1;
            if (!l && !r && !b && !u) continue; // 안쪽은 비워둔다

            Vector2Int c;
            if (u && l) c = RailTL; else if (u && r) c = RailTR;
            else if (b && l) c = RailBL; else if (b && r) c = RailBR;
            else if (u) c = RailT; else if (b) c = RailB;
            else if (l) c = RailL; else c = RailR;
            map.SetTile(Pos(x, y), t[c]);
        }
    }

    private static int _sortLayerId;

    private static Vector3Int Pos(int x, int y) => new Vector3Int(Origin.x + x, Origin.y + y, 0);
    private static Vector3 World(float x, float y) => new Vector3(Origin.x + x, Origin.y + y, 0);

    // ── 소품 배치 ────────────────────────────────────────────
    private static void PlaceProps(Transform levelRoot, Dictionary<string, Sprite> p, Dictionary<string, Sprite> a)
    {
        var old = levelRoot.Find("Arena");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("Arena");
        root.transform.SetParent(levelRoot, false);

        // 스폰 지점 4곳 — 엘프고처럼 사방에서 몰려온다
        var spawns = Child(root.transform, "SpawnPoints");
        Marker(spawns, "Spawn_NW", 6.5f, 16.5f);
        Marker(spawns, "Spawn_NE", 14.5f, 16.5f);
        Marker(spawns, "Spawn_SW", 6.5f, 6.5f);
        Marker(spawns, "Spawn_SE", 14.5f, 6.5f);
        Marker(root.transform, "PlayerStart", 10.5f, 5.5f);
        Marker(root.transform, "BossSpawn", 29.5f, 11.5f);

        var deco = Child(root.transform, "Decorations");

        // ── 아레나(왼쪽): 제단과 스테인드글라스 ──
        Put(deco, a, "Window_Red", 6.5f, 20f);
        Put(deco, a, "Window_Red", 14.5f, 20f);
        Put(deco, a, "Light_Red", 6.5f, 18f);
        Put(deco, a, "Light_Red", 14.5f, 18f);
        Put(deco, p, "Winged_Statue", 10.5f, 18f);
        Put(deco, p, "Bench_1", 10.5f, 16.5f);
        Put(deco, p, "Candelabra", 8.5f, 17f);
        Put(deco, p, "Candelabra", 12.5f, 17f);

        // 구석 잡동사니
        Put(deco, p, "Crate", 4.5f, 5f);
        Put(deco, p, "Crate_Tall", 4.5f, 7f);
        Put(deco, p, "Barrel", 5.8f, 4.2f);
        Put(deco, p, "Pot_Brown_Wide", 7.5f, 4.2f);
        Put(deco, p, "Pot_Brown_Tall", 8.6f, 4.4f);
        Put(deco, p, "Pot_Brown_Small", 9.6f, 4.1f);
        Put(deco, p, "Bone_Big", 11.5f, 4.6f);
        Put(deco, p, "Bone_S1", 12.6f, 4.3f);
        Put(deco, p, "Bone_S2", 13.2f, 5.1f);
        Put(deco, p, "Rock_1", 16f, 8f);
        Put(deco, p, "Rock_2", 4.5f, 14f);
        Put(deco, p, "Candle_Tall", 3.8f, 11f);
        Put(deco, p, "Candle_Tall", 16.8f, 11f);

        // ── 보스방(오른쪽) ──
        Put(deco, a, "Window_Blue", 27.5f, 20f);
        Put(deco, a, "Window_Blue", 31.5f, 20f);
        Put(deco, a, "Light_Blue", 27.5f, 18f);
        Put(deco, a, "Light_Blue", 31.5f, 18f);
        Put(deco, p, "Altar_Gem", 29.5f, 17f);
        Put(deco, p, "Candle_Tall", 26.5f, 17.2f);
        Put(deco, p, "Candle_Tall", 32.5f, 17.2f);
        Put(deco, p, "Cross_Grave", 24f, 16f);
        Put(deco, p, "Tombstone", 35f, 16f);
        Put(deco, p, "Pot_Grey_Wide", 34.5f, 4.5f);
        Put(deco, p, "Pot_Grey_Tall", 35.6f, 5.4f);
        Put(deco, p, "Pot_Grey_Small", 33.6f, 4.2f);
        Put(deco, p, "Table_Stone", 24f, 6f);
        Put(deco, p, "Table_Stone_2", 25.3f, 6f);
        Put(deco, p, "Sword_Stand", 29.5f, 5f);
        Put(deco, p, "Rubble", 31f, 8f);

        // ── 거미줄: 두 방의 네 모서리 ──
        foreach (var (cx, cy, flip) in new (float, float, bool)[]
        {
            (3.5f, 19.5f, false), (17f, 19.5f, true),
            (22.5f, 19.5f, false), (36f, 19.5f, true),
        })
        {
            var web = Put(deco, p, "Cobweb_Corner", cx, cy);
            if (web != null) web.GetComponent<SpriteRenderer>().flipX = flip;
        }
        Put(deco, p, "Cobweb_Top", 10.5f, 19.6f);
        Put(deco, p, "Cobweb_Top", 29.5f, 19.6f);
    }

    private static Transform Child(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void Marker(Transform parent, string name, float x, float y)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = World(x, y);
    }

    private static GameObject Put(Transform parent, Dictionary<string, Sprite> lib, string name, float x, float y)
    {
        if (!lib.TryGetValue(name, out var sprite))
        {
            Debug.LogWarning($"[Arena] 스프라이트 없음: {name}");
            return null;
        }

        var go = new GameObject(name, typeof(SpriteRenderer)) { layer = 0 };
        go.transform.SetParent(parent, false);
        go.transform.position = World(x, y);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerID = _sortLayerId;
        // 벽 타일맵(order 2)보다 무조건 위. 아래에 있는 소품일수록 앞으로 온다.
        sr.sortingOrder = 10 + (H - Mathf.RoundToInt(y));
        return go;
    }

    // ── 유틸 ────────────────────────────────────────────────
    private static string Path(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }

    private static Tilemap GetOrCreateTilemap(Transform parent, string name, int order)
    {
        Transform t = parent.Find(name);
        if (t == null)
        {
            var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(parent, false);
            t = go.transform;
        }
        var tm = t.GetComponent<Tilemap>() ?? t.gameObject.AddComponent<Tilemap>();
        var tr = t.GetComponent<TilemapRenderer>() ?? t.gameObject.AddComponent<TilemapRenderer>();
        if (order > 0) tr.sortingLayerID = _sortLayerId; // GroundTile 기준으로 통일
        tr.sortingOrder = order;
        t.gameObject.layer = 0; // Default. 씬 뷰 레이어 숨김에 걸리지 않게.
        return tm;
    }

    private static void SetupWallCollider(Tilemap wall)
    {
        var go = wall.gameObject;
        // CompositeCollider2D 를 붙이면 Rigidbody2D 가 자동으로 따라붙는다.
        // AddComponent 반환값을 바로 쓰면 그 시점에 아직 유효하지 않으므로 GetComponent 로 다시 받는다.
        if (go.GetComponent<TilemapCollider2D>() == null) go.AddComponent<TilemapCollider2D>();
        if (go.GetComponent<CompositeCollider2D>() == null) go.AddComponent<CompositeCollider2D>();

        var tc = go.GetComponent<TilemapCollider2D>();
        var cc = go.GetComponent<CompositeCollider2D>();
        var rb = go.GetComponent<Rigidbody2D>();

        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        if (cc != null) cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
        if (tc != null) tc.compositeOperation = Collider2D.CompositeOperation.Merge;
    }

    // ── 슬라이스 ────────────────────────────────────────────
    private static Dictionary<Vector2Int, Tile> SliceTiles()
    {
        var needed = Fill
            .Concat(WallBrick)
            .Concat(new[] { RimTL, RimT, RimTR, RimL, RimR, RimBL, RimB, RimBR })
            .Concat(new[] { RailTL, RailT, RailTR, RailL, RailR, RailBL, RailB, RailBR })
            .Concat(Shadow)
            .Distinct().ToList();

        var imp = Prepare(Sheet, out var tex);
        if (imp == null) return null;

        var rects = needed.Select(c => new SpriteRect
        {
            name = $"D2_{c.x}_{c.y}",
            spriteID = GUID.Generate(),
            rect = new Rect(c.x * Cell, tex.height - (c.y + 1) * Cell, Cell, Cell),
            alignment = SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f)
        }).ToList();
        ApplyRects(imp, rects);

        if (!AssetDatabase.IsValidFolder("Assets/_Graphics/Tiles"))
            AssetDatabase.CreateFolder("Assets/_Graphics", "Tiles");
        if (!AssetDatabase.IsValidFolder(TileDir))
            AssetDatabase.CreateFolder("Assets/_Graphics/Tiles", "Dungeon");

        var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>()
            .GroupBy(s => s.name).ToDictionary(g => g.Key, g => g.First());
        var result = new Dictionary<Vector2Int, Tile>();

        foreach (var c in needed)
        {
            string n = $"D2_{c.x}_{c.y}";
            if (!sprites.TryGetValue(n, out var sprite)) continue;

            string path = $"{TileDir}/{n}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            bool isNew = tile == null;
            if (isNew) tile = ScriptableObject.CreateInstance<Tile>();

            tile.sprite = sprite;
            // 충돌은 Wall Tile 타일맵에만 콜라이더 컴포넌트를 달아서 처리한다.
            // 바닥/그림자 타일맵에는 콜라이더가 없으므로 여기서는 전부 Grid 로 둬도 안전하다.
            tile.colliderType = Tile.ColliderType.Grid;

            if (isNew) AssetDatabase.CreateAsset(tile, path);
            else EditorUtility.SetDirty(tile);
            result[c] = tile;
        }

        AssetDatabase.SaveAssets();
        return result;
    }

    private static Dictionary<string, Sprite> SliceProps(string sheetPath, Prop[] table)
    {
        var imp = Prepare(sheetPath, out var tex);
        if (imp == null) return new Dictionary<string, Sprite>();

        var rects = table.Select(pr => new SpriteRect
        {
            name = pr.name,
            spriteID = GUID.Generate(),
            rect = new Rect(pr.x, tex.height - (pr.y + pr.h), pr.w, pr.h),
            alignment = SpriteAlignment.Custom,
            pivot = new Vector2(0.5f, 0f) // 바닥 기준
        }).ToList();
        ApplyRects(imp, rects);

        return AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>()
            .GroupBy(s => s.name).ToDictionary(g => g.Key, g => g.First());
    }

    private static TextureImporter Prepare(string path, out Texture2D tex)
    {
        tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp == null || tex == null)
        {
            Debug.LogError($"[Arena] 시트 없음: {path}");
            return null;
        }

        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = PPU;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        var ts = new TextureImporterSettings();
        imp.ReadTextureSettings(ts);
        ts.spriteMode = (int)SpriteImportMode.Multiple;
        imp.SetTextureSettings(ts);
        return imp;
    }

    private static void ApplyRects(TextureImporter imp, List<SpriteRect> rects)
    {
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var dp = factories.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();
        dp.SetSpriteRects(rects.ToArray());
        dp.GetDataProvider<ISpriteNameFileIdDataProvider>()
          .SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();
    }
}
