using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Tools > Import Tiled Test Map
// 에셋에 포함된 Tiled 데모 맵(Test map.tmx)을 파싱해 Unity 타일맵 + 스프라이트로 그대로 재현.
public static class TiledMapImporter
{
    private const string TmxAsset = "Assets/Asset/The Fan-tasy Tileset (Premium)/Tiled/Tilemaps/Test map.tmx";
    private const string SpriteOut = "Assets/03.TiledTiles";
    private const string ScenePath = "Assets/00.Scenes/TestMap.unity";
    private const uint FLIP_H = 0x80000000, FLIP_V = 0x40000000, FLIP_D = 0x20000000;
    private const uint GID_MASK = 0x1FFFFFFF;

    private class Tsx
    {
        public int firstgid;
        public string name;
        public bool collection;
        // grid
        public string imageAsset;
        public int cols, tw, th, imgW, imgH;
        // collection
        public Dictionary<int, string> idImage = new Dictionary<int, string>();
    }

    private static List<Tsx> _sets;
    private static readonly Dictionary<uint, Tile> _tileCache = new Dictionary<uint, Tile>();
    private static readonly Dictionary<uint, Sprite> _spriteCache = new Dictionary<uint, Sprite>();
    private static readonly HashSet<string> _imported = new HashSet<string>();

    [MenuItem("Tools/Import Tiled Test Map")]
    public static void Import()
    {
        _sets = null; _tileCache.Clear(); _spriteCache.Clear(); _imported.Clear();
        string tmxFull = ToFull(TmxAsset);
        string tmxDir = Path.GetDirectoryName(tmxFull);
        var map = XDocument.Load(tmxFull).Root;
        int w = (int)map.Attribute("width"), h = (int)map.Attribute("height");
        int mapHpx = h * 16;

        // 타일셋 로드
        _sets = new List<Tsx>();
        foreach (var t in map.Elements("tileset"))
        {
            int fg = (int)t.Attribute("firstgid");
            string tsxFull = Path.GetFullPath(Path.Combine(tmxDir, (string)t.Attribute("source")));
            _sets.Add(ParseTsx(tsxFull, fg));
        }
        _sets.Sort((a, b) => a.firstgid.CompareTo(b.firstgid));

        if (!AssetDatabase.IsValidFolder(SpriteOut))
            AssetDatabase.CreateFolder("Assets", "03.TiledTiles");

        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cam.tag = "MainCamera";
        var camera = cam.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = h / 2f + 1;
        camera.transform.position = new Vector3(w / 2f, h / 2f, -10);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(0x22, 0x38, 0x22, 0xFF);

        var lightGo = new GameObject("Global Light 2D", typeof(Light2D));
        lightGo.GetComponent<Light2D>().lightType = Light2D.LightType.Global;

        var gridGo = new GameObject("Grid", typeof(Grid));

        AssetDatabase.StartAssetEditing();
        try
        {
            int order = -1000;
            foreach (var el in map.Elements())
            {
                if (el.Name == "layer")
                    BuildTileLayer(el, gridGo.transform, order, w, h);
                else if (el.Name == "objectgroup")
                    BuildObjectLayer(el, gridGo.transform, mapHpx);
                order += 5;
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }

        AssetDatabase.SaveAssets();
        EditorApplication.delayCall += () =>
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            Debug.Log("[TiledImport] Test map 재현 완료 → " + ScenePath);
        };
    }

    // ── 타일 레이어 ────────────────────────────────────
    private static void BuildTileLayer(XElement layer, Transform grid, int order, int w, int h)
    {
        var data = layer.Element("data");
        if (data == null) return;
        string name = (string)layer.Attribute("name");
        var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        go.transform.SetParent(grid, false);
        go.GetComponent<TilemapRenderer>().sortingOrder = order;
        var tm = go.GetComponent<Tilemap>();

        string[] tok = data.Value.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        int i = 0;
        for (int row = 0; row < h; row++)
        for (int col = 0; col < w; col++)
        {
            if (i >= tok.Length) break;
            uint raw = uint.Parse(tok[i++].Trim());
            if (raw == 0) continue;
            bool fx = (raw & FLIP_H) != 0, fy = (raw & FLIP_V) != 0, fd = (raw & FLIP_D) != 0;
            uint gid = raw & GID_MASK;
            var tile = GetTile(gid);
            if (tile == null) continue;
            var cell = new Vector3Int(col, h - 1 - row, 0);
            tm.SetTile(cell, tile);
            if (fx || fy || fd)
            {
                Matrix4x4 m = Matrix4x4.TRS(Vector3.zero,
                    fd ? Quaternion.Euler(0, 0, 90) : Quaternion.identity,
                    new Vector3(fx ? -1 : 1, fy ? -1 : 1, 1));
                tm.SetTransformMatrix(cell, m);
            }
        }
    }

    // ── 오브젝트 레이어 ────────────────────────────────
    private static void BuildObjectLayer(XElement group, Transform grid, int mapHpx)
    {
        string name = (string)group.Attribute("name");
        var parent = new GameObject(name).transform;
        parent.SetParent(grid, false);
        bool isShadow = name.ToLower().Contains("shadow");

        foreach (var obj in group.Elements("object"))
        {
            var gidAttr = obj.Attribute("gid");
            if (gidAttr == null) continue;
            uint raw = uint.Parse((string)gidAttr);
            bool fx = (raw & FLIP_H) != 0, fy = (raw & FLIP_V) != 0;
            uint gid = raw & GID_MASK;
            var sprite = GetSprite(gid);
            if (sprite == null) continue;

            float ox = float.Parse((string)obj.Attribute("x"), CultureInfo.InvariantCulture);
            float oy = float.Parse((string)obj.Attribute("y"), CultureInfo.InvariantCulture);
            // Tiled 타일오브젝트: (x,y)=좌하단, y는 위에서부터. Unity(y위+) 좌하단:
            Vector2 bottomLeft = new Vector2(ox / 16f, (mapHpx - oy) / 16f);
            Vector3 pos = (Vector3)(bottomLeft - (Vector2)sprite.bounds.min);

            var go = new GameObject(sprite.name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.flipX = fx; sr.flipY = fy;
            sr.sortingOrder = isShadow ? -800 : Mathf.RoundToInt(-bottomLeft.y * 10);
        }
    }

    // ── GID → Tile / Sprite ───────────────────────────
    private static Tile GetTile(uint gid)
    {
        if (_tileCache.TryGetValue(gid, out var t)) return t;
        var spr = GetSprite(gid);
        if (spr == null) { _tileCache[gid] = null; return null; }
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = spr;
        tile.colliderType = Tile.ColliderType.None;
        AssetDatabase.CreateAsset(tile, $"{SpriteOut}/tile_{gid}.asset");
        _tileCache[gid] = tile;
        return tile;
    }

    private static Sprite GetSprite(uint gid)
    {
        if (_spriteCache.TryGetValue(gid, out var s)) return s;
        Tsx ts = ResolveSet(gid);
        if (ts == null) { _spriteCache[gid] = null; return null; }
        int localId = (int)gid - ts.firstgid;
        Sprite spr = ts.collection ? LoadCollectionSprite(ts, localId) : SliceGridSprite(ts, localId);
        _spriteCache[gid] = spr;
        return spr;
    }

    private static Tsx ResolveSet(uint gid)
    {
        Tsx found = null;
        foreach (var ts in _sets)
            if (ts.firstgid <= gid) found = ts; else break;
        return found;
    }

    private static Sprite LoadCollectionSprite(Tsx ts, int localId)
    {
        if (!ts.idImage.TryGetValue(localId, out string assetPath)) return null;
        EnsureSpriteImport(assetPath);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Sprite SliceGridSprite(Tsx ts, int localId)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ts.imageAsset);
        if (tex == null) return null;
        int col = localId % ts.cols, row = localId / ts.cols;
        var rect = new Rect(col * ts.tw, ts.imgH - (row + 1) * ts.th, ts.tw, ts.th);
        var spr = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
        spr.name = $"{ts.name}_{localId}";
        AssetDatabase.CreateAsset(spr, $"{SpriteOut}/{ts.name}_{localId}.asset");
        return spr;
    }

    // ── .tsx 파싱 ─────────────────────────────────────
    private static Tsx ParseTsx(string tsxFull, int firstgid)
    {
        var root = XDocument.Load(tsxFull).Root;
        var ts = new Tsx { firstgid = firstgid, name = (string)root.Attribute("name") };
        ts.tw = (int)root.Attribute("tilewidth");
        ts.th = (int)root.Attribute("tileheight");
        var img = root.Element("image");
        string tsxDir = Path.GetDirectoryName(tsxFull);
        if (img != null)
        {
            ts.collection = false;
            ts.cols = (int?)root.Attribute("columns") ?? 1;
            ts.imgW = (int)img.Attribute("width");
            ts.imgH = (int)img.Attribute("height");
            ts.imageAsset = ToAsset(Path.GetFullPath(Path.Combine(tsxDir, (string)img.Attribute("source"))));
            EnsureTilesetImport(ts.imageAsset);
        }
        else
        {
            ts.collection = true;
            foreach (var tile in root.Elements("tile"))
            {
                var ti = tile.Element("image");
                if (ti == null) continue;
                int id = (int)tile.Attribute("id");
                ts.idImage[id] = ToAsset(Path.GetFullPath(Path.Combine(tsxDir, (string)ti.Attribute("source"))));
            }
        }
        return ts;
    }

    // ── 임포트 세팅 ────────────────────────────────────
    private static void EnsureSpriteImport(string assetPath)
    {
        if (!_imported.Add(assetPath)) return;
        var imp = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        if (imp == null) return;
        bool changed = false;
        if (imp.spritePixelsPerUnit != 16) { imp.spritePixelsPerUnit = 16; changed = true; }
        if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; changed = true; }
        if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        var s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        if (s.spriteMode != (int)SpriteImportMode.Single) { s.spriteMode = (int)SpriteImportMode.Single; imp.SetTextureSettings(s); changed = true; }
        if (changed) imp.SaveAndReimport();
    }

    private static void EnsureTilesetImport(string assetPath)
    {
        if (!_imported.Add(assetPath)) return;
        var imp = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        if (imp == null) return;
        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = 16;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.isReadable = true; // Sprite.Create 슬라이스용
        var s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        s.spriteMode = (int)SpriteImportMode.Single; // 슬라이스는 Sprite.Create로 직접
        imp.SetTextureSettings(s);
        imp.SaveAndReimport();
    }

    private static string ToFull(string assetPath) =>
        Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));

    private static string ToAsset(string fullPath)
    {
        fullPath = fullPath.Replace('\\', '/');
        int idx = fullPath.IndexOf("/Assets/", StringComparison.Ordinal);
        if (idx >= 0) return fullPath.Substring(idx + 1);
        if (fullPath.StartsWith("Assets/")) return fullPath;
        return fullPath;
    }
}
