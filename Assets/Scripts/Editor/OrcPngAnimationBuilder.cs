using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// Tools > Build Orc1 Animation (PNG)
// PNG 시트(_full.png)를 64x64로 잘라 방향별 애니메이션 클립을 만들고,
// 플레이어와 같은 방식(2D Simple Directional 블렌드 트리)의 컨트롤러를 조립한다.
public static class OrcPngAnimationBuilder
{
    private const string PngRoot = "Assets/_Graphics/Asset/GoblinEnemy/PNG/Orc1";
    private const string ClipDir = "Assets/_Graphics/Animations/Orc1Clip";
    private const string ControllerPath = "Assets/_Graphics/Animations/Orc1 Controller.controller";

    private const int FrameSize = 64;
    private const int PixelsPerUnit = 16; // 플레이어/타일맵과 동일하게
    private const float Fps = 12f;

    private const string PMoveX = "MoveX", PMoveY = "MoveY";

    // 시트의 행 순서 = front(정면) → back(뒤) → left → right
    private static readonly (string dir, float x, float y)[] Rows =
    {
        ("front", 0f, -1f),
        ("back",  0f,  1f),
        ("left", -1f,  0f),
        ("right", 1f,  0f),
    };

    // 상태 이름 / 원본 폴더 / 루프 여부 / 노드 위치
    private static readonly (string state, string folder, bool loop, float px, float py)[] Actions =
    {
        ("Idle",       "Orc1_idle",        true,  300f,    0f),
        ("Walk",       "Orc1_walk",        true,  300f,  120f),
        ("Run",        "Orc1_run",         true,  300f,  240f),
        ("Attack",     "Orc1_attack",      false, 650f, -100f),
        ("WalkAttack", "Orc1_walk_attack", false, 650f,   20f),
        ("RunAttack",  "Orc1_run_attack",  false, 650f,  140f),
        ("Hurt",       "Orc1_hurt",        false, 650f,  260f),
        ("Death",      "Orc1_death",       false, 650f,  380f),
    };

    [MenuItem("Tools/Build Orc1 Animation (PNG)")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(ClipDir))
            AssetDatabase.CreateFolder("Assets/_Graphics/Animations", "Orc1Clip");

        var clips = new Dictionary<string, AnimationClip>(); // "Idle_front" -> clip
        int madeClips = 0;

        foreach (var (state, folder, loop, _, _) in Actions)
        {
            string sheetPath = FindFullSheet(folder);
            if (sheetPath == null)
            {
                Debug.LogWarning($"[Orc] 시트를 찾지 못함: {folder}");
                continue;
            }

            Sprite[] sprites = SliceSheet(sheetPath);
            if (sprites == null) continue;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
            int cols = tex.width / FrameSize;

            for (int row = 0; row < Rows.Length; row++)
            {
                // 시트 위쪽이 첫 행이므로 이름 규칙과 맞춰 가져온다
                var frames = new List<Sprite>();
                for (int col = 0; col < cols; col++)
                {
                    string spriteName = $"{state}_{Rows[row].dir}_{col}";
                    Sprite s = sprites.FirstOrDefault(sp => sp.name == spriteName);
                    if (s != null) frames.Add(s);
                }

                if (frames.Count == 0) continue;

                string clipName = $"{state}_{Rows[row].dir}";
                clips[clipName] = CreateClip(clipName, frames, loop);
                madeClips++;
            }
        }

        AssetDatabase.SaveAssets();
        BuildController(clips);

        Debug.Log($"[Orc] 클립 {madeClips}개 생성 → {ClipDir}\n컨트롤러 → {ControllerPath}");
    }

    // 폴더 안에서 합쳐진 시트(_full)를 찾는다. 파일명에 공백 등 표기 흔들림이 있어 패턴으로 찾음
    private static string FindFullSheet(string folder)
    {
        string dir = $"{PngRoot}/{folder}";
        if (!AssetDatabase.IsValidFolder(dir)) return null;

        return AssetDatabase.FindAssets("t:texture2D", new[] { dir })
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p).Replace(" ", "").EndsWith("full"));
    }

    // 시트를 64x64 격자로 잘라 스프라이트 생성 (이름: State_dir_index)
    private static Sprite[] SliceSheet(string path)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (imp == null || tex == null) return null;

        string state = Actions.First(a => path.Contains(a.folder)).state;
        int cols = tex.width / FrameSize;
        int rows = tex.height / FrameSize;

        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = PixelsPerUnit;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;

        var settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        settings.spriteMode = (int)SpriteImportMode.Multiple;
        imp.SetTextureSettings(settings);

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var dp = factories.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();

        var rects = new List<SpriteRect>();
        for (int row = 0; row < rows && row < Rows.Length; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                rects.Add(new SpriteRect
                {
                    name = $"{state}_{Rows[row].dir}_{col}",
                    spriteID = GUID.Generate(),
                    // 텍스처 좌표는 아래가 0이므로 위쪽 행부터 채우려면 뒤집어 계산
                    rect = new Rect(col * FrameSize, tex.height - (row + 1) * FrameSize, FrameSize, FrameSize),
                    alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(0.5f, 0.25f) // 발밑 기준에 가깝게
                });
            }
        }

        dp.SetSpriteRects(rects.ToArray());
        var nameFileId = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameFileId.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();

        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
    }

    // SpriteRenderer.sprite 를 프레임마다 교체하는 클립
    private static AnimationClip CreateClip(string name, List<Sprite> frames, bool loop)
    {
        string path = $"{ClipDir}/{name}.anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        bool isNew = clip == null;
        if (isNew) clip = new AnimationClip();

        clip.frameRate = Fps;

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[frames.Count];
        for (int i = 0; i < frames.Count; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / Fps, value = frames[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (isNew) AssetDatabase.CreateAsset(clip, path);
        else EditorUtility.SetDirty(clip);

        return clip;
    }

    // 플레이어와 동일한 구조: 액션별 4방향 블렌드 트리, 파라미터는 MoveX/MoveY, 전이 없음
    private static void BuildController(Dictionary<string, AnimationClip> clips)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        var sm = controller.layers[0].stateMachine;
        foreach (var t in sm.anyStateTransitions.ToArray()) sm.RemoveAnyStateTransition(t);
        foreach (var s in sm.states.ToArray()) sm.RemoveState(s.state);
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(ControllerPath))
            if (obj is BlendTree bt) Object.DestroyImmediate(bt, true);

        foreach (var p in controller.parameters.ToArray()) controller.RemoveParameter(p);
        controller.AddParameter(PMoveX, AnimatorControllerParameterType.Float);
        controller.AddParameter(PMoveY, AnimatorControllerParameterType.Float);

        var ps = controller.parameters;
        for (int i = 0; i < ps.Length; i++)
            if (ps[i].name == PMoveY) ps[i].defaultFloat = -1f; // 기본은 정면
        controller.parameters = ps;

        AnimatorState first = null;
        foreach (var (state, _, _, px, py) in Actions)
        {
            var st = controller.CreateBlendTreeInController(state, out BlendTree tree, 0);
            tree.name = state;
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = PMoveX;
            tree.blendParameterY = PMoveY;

            foreach (var (dir, x, y) in Rows)
                if (clips.TryGetValue($"{state}_{dir}", out var clip))
                    tree.AddChild(clip, new Vector2(x, y));

            var arr = sm.states;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i].state == st) arr[i].position = new Vector3(px, py, 0);
            sm.states = arr;

            if (first == null) first = st;
        }

        sm.defaultState = first;
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }
}
