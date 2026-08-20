using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// Tools > Build Cute Fantasy Characters
//
// Cute Fantasy Characters 팩의 시트를 잘라 애니메이션 클립과 애니메이터를 자동 생성한다.
// 시트마다 셀 크기가 달라서(32/48/64px) 격자를 가정하지 않고 픽셀로 프레임을 검출한다.
//
// ⚠ 행 순서(RowNames)는 추정값이다. 결과가 어긋나면 이 표만 고치고 다시 실행하면 된다.
public static class CuteFantasyCharacterBuilder
{
    private const string CharRoot = "Assets/_Graphics/Asset/Cute_Fantasy_Characters";
    private const string OutRoot = "Assets/_Graphics/Animations/Enemies";
    private const int PixelsPerUnit = 16;
    private const float Fps = 10f;
    private const byte AlphaThreshold = 30;

    // 공격 클립에서 타격 판정이 들어가는 지점 (클립 길이 기준 비율)
    private const float DamageCastRatio = 0.55f;

    private const string PMoveX = "MoveX", PMoveY = "MoveY";

    // 시트의 행 순서. 14종 시트를 전부 눈으로 확인해 확정했다.
    //   방향은 Down → Side → Up 순서고, Death 가 Hurt 보다 먼저 온다.
    //   (Death = 쓰러지는 4프레임, Hurt = 흰색 피격 플래시)
    private static readonly string[] RowNames =
    {
        "Idle_Down",   "Idle_Side",   "Idle_Up",
        "Run_Down",    "Run_Side",    "Run_Up",
        "Attack_Down", "Attack_Side", "Attack_Up",
        "Death",
        "Hurt_Down",   "Hurt_Side",   "Hurt_Up",
    };

    // 같은 액션의 세 방향은 프레임 수가 항상 같다. 밴드 하나가 튀면 이걸로 바로잡는다.
    // (시작 인덱스, 길이)
    private static readonly (int start, int length)[] ActionGroups =
    {
        (0, 3), (3, 3), (6, 3), (9, 1), (10, 3),
    };

    // 블렌드 트리로 묶을 액션 (Death는 방향이 없어 단일 클립 상태로 만든다)
    private static readonly string[] DirectionalActions = { "Idle", "Run", "Attack", "Hurt" };

    [MenuItem("Tools/Build Cute Fantasy Characters")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(OutRoot))
            AssetDatabase.CreateFolder("Assets/_Graphics/Animations", "Enemies");

        string[] sheets = AssetDatabase.FindAssets("t:texture2D", new[] { CharRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".png"))
            .OrderBy(p => p)
            .ToArray();

        int okCount = 0;
        foreach (string sheetPath in sheets)
        {
            if (BuildCharacter(sheetPath)) okCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CuteFantasy] 캐릭터 {okCount}/{sheets.Length}종 완료 → {OutRoot}");
    }

    private static bool BuildCharacter(string sheetPath)
    {
        string charName = Path.GetFileNameWithoutExtension(sheetPath);

        // 1) 읽기 가능하게 임포트 설정
        var imp = (TextureImporter)AssetImporter.GetAtPath(sheetPath);
        if (imp == null) return false;

        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = PixelsPerUnit;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.isReadable = true;

        var ts = new TextureImporterSettings();
        imp.ReadTextureSettings(ts);
        ts.spriteMode = (int)SpriteImportMode.Multiple;
        imp.SetTextureSettings(ts);
        imp.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
        if (tex == null) return false;

        // 2) 픽셀로 행(밴드)과 각 행의 프레임을 검출
        List<(int yTop, int yBottom)> bands = FindBands(tex);
        if (bands.Count == 0)
        {
            Debug.LogWarning($"[CuteFantasy] 내용을 찾지 못함: {charName}");
            return false;
        }

        // 밴드마다 열 개수를 구한 뒤, 액션 그룹별로 중앙값을 씌워 튀는 값을 눌러준다.
        Color32[] pixels = tex.GetPixels32();
        var columnCounts = new int[bands.Count];
        for (int r = 0; r < bands.Count; r++)
            columnCounts[r] = CountColumns(pixels, tex.width, bands[r].yTop, bands[r].yBottom);

        if (bands.Count == RowNames.Length)
        {
            foreach (var (start, length) in ActionGroups)
            {
                var slice = new List<int>();
                for (int i = start; i < start + length; i++) slice.Add(columnCounts[i]);
                slice.Sort();
                int median = slice[slice.Count / 2];
                for (int i = start; i < start + length; i++) columnCounts[i] = median;
            }
        }

        var rects = new List<SpriteRect>();
        var rowFrameNames = new List<List<string>>();

        for (int r = 0; r < bands.Count; r++)
        {
            string rowName = r < RowNames.Length ? RowNames[r] : $"Extra{r + 1}";
            var (yTop, yBottom) = bands[r];

            int columns = Mathf.Max(1, columnCounts[r]);
            int height = yBottom - yTop + 1;
            int yFromBottom = tex.height - yBottom - 1;

            var names = new List<string>();
            for (int k = 0; k < columns; k++)
            {
                int xLeft = k * tex.width / columns;
                int xRight = (k + 1) * tex.width / columns - 1;
                if (!HasContent(pixels, tex.width, xLeft, xRight, yTop, yBottom)) continue;

                string spriteName = $"{charName}_{rowName}_{names.Count}";
                rects.Add(new SpriteRect
                {
                    name = spriteName,
                    spriteID = GUID.Generate(),
                    // 칸 전체를 쓴다. 내용에 딱 맞춰 자르면 프레임마다 폭이 달라져 그림이 떨린다.
                    rect = new Rect(xLeft, yFromBottom, xRight - xLeft + 1, height),
                    alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(0.5f, 0.1f) // 발밑 기준
                });
                names.Add(spriteName);
            }
            rowFrameNames.Add(names);
        }

        // 3) 슬라이스 반영
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var dp = factories.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();
        dp.SetSpriteRects(rects.ToArray());
        dp.GetDataProvider<ISpriteNameFileIdDataProvider>()
          .SetNameFileIdPairs(rects.Select(x => new SpriteNameFileIdPair(x.name, x.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();

        var sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
            .OfType<Sprite>().ToDictionary(s => s.name);

        // 4) 행마다 클립 생성
        string charDir = $"{OutRoot}/{charName}";
        if (!AssetDatabase.IsValidFolder(charDir))
            AssetDatabase.CreateFolder(OutRoot, charName);

        var clips = new Dictionary<string, AnimationClip>();
        for (int r = 0; r < rowFrameNames.Count; r++)
        {
            string rowName = r < RowNames.Length ? RowNames[r] : $"Extra{r + 1}";
            var frames = rowFrameNames[r]
                .Where(sprites.ContainsKey).Select(n => sprites[n]).ToList();
            if (frames.Count == 0) continue;

            bool loop = rowName.StartsWith("Idle") || rowName.StartsWith("Run");
            clips[rowName] = CreateClip($"{charDir}/{charName}_{rowName}.anim", frames, loop, rowName);
        }

        // 옛 슬라이스가 남긴 Extra 클립을 치운다 (밴드 병합 전에는 오크가 14~15행으로 잡혔다)
        foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { charDir }))
        {
            string clipPath = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(clipPath).Contains("_Extra"))
                AssetDatabase.DeleteAsset(clipPath);
        }

        // 5) 애니메이터 컨트롤러
        BuildController($"{charDir}/{charName}.controller", clips);
        return true;
    }

    // 완전히 비어있는 가로줄로 행을 나눈다
    private static List<(int, int)> FindBands(Texture2D tex)
    {
        var bands = new List<(int, int)>();
        Color32[] px = tex.GetPixels32();
        bool inBand = false;
        int start = 0;

        for (int y = 0; y < tex.height; y++)
        {
            int texY = tex.height - 1 - y; // 위에서부터 훑는다
            bool has = false;
            for (int x = 0; x < tex.width; x++)
            {
                if (px[texY * tex.width + x].a > AlphaThreshold) { has = true; break; }
            }

            if (has && !inBand) { inBand = true; start = y; }
            else if (!has && inBand) { inBand = false; bands.Add((start, y - 1)); }
        }
        if (inBand) bands.Add((start, tex.height - 1));
        if (bands.Count == 0) return bands;

        // 공격 프레임처럼 무기가 머리 위로 뻗은 행은 무기 끝이 몸통과 투명한 줄로 끊겨서
        // 1~3px짜리 조각 밴드가 생긴다. 조각은 바로 아래 밴드에 흡수시킨다.
        var heights = bands.Select(b => b.Item2 - b.Item1 + 1).OrderBy(h => h).ToList();
        int medianHeight = heights[heights.Count / 2];

        var merged = new List<(int, int)>();
        for (int i = 0; i < bands.Count; i++)
        {
            var (top, bottom) = bands[i];
            if (bottom - top + 1 < medianHeight * 0.4f && i + 1 < bands.Count)
            {
                bands[i + 1] = (top, bands[i + 1].Item2);
                continue;
            }
            merged.Add(bands[i]);
        }
        return merged;
    }

    // 한 밴드의 열 개수를 구한다.
    //
    // 투명한 세로줄만 보고 자르면 두 방향으로 다 틀린다.
    //   · 스프라이트가 서로 닿아 있으면 두 프레임이 하나로 붙고
    //   · 베기 이펙트가 몸통에서 떨어져 있으면 한 프레임이 둘로 갈라진다.
    // 그래서 덩어리 사이 간격(피치)의 중앙값으로 균일 격자를 역산한다.
    private static int CountColumns(Color32[] px, int width, int yTop, int yBottom)
    {
        var starts = new List<int>();
        bool inChunk = false;
        int lastEnd = -99;

        for (int x = 0; x < width; x++)
        {
            bool has = HasContent(px, width, x, x, yTop, yBottom);

            if (has && !inChunk)
            {
                // 3px 미만으로 떨어진 건 같은 프레임의 이펙트로 본다
                if (x - lastEnd - 1 >= 3) starts.Add(x);
                inChunk = true;
            }
            else if (!has && inChunk)
            {
                inChunk = false;
                lastEnd = x - 1;
            }
        }

        if (starts.Count < 2) return Mathf.Max(1, starts.Count);

        var gaps = new List<int>();
        for (int i = 1; i < starts.Count; i++) gaps.Add(starts[i] - starts[i - 1]);
        gaps.Sort();
        int pitch = gaps[gaps.Count / 2];

        return pitch <= 0 ? starts.Count : Mathf.Max(1, Mathf.RoundToInt(width / (float)pitch));
    }

    private static bool HasContent(Color32[] px, int width, int xLeft, int xRight, int yTop, int yBottom)
    {
        for (int y = yTop; y <= yBottom; y++)
        {
            int rowStart = (px.Length / width - 1 - y) * width; // 텍스처는 아래가 0이라 뒤집는다
            for (int x = xLeft; x <= xRight; x++)
                if (px[rowStart + x].a > AlphaThreshold) return true;
        }
        return false;
    }


    // CommonMeleeSkill 은 AgentRenderer 가 애니메이션 이벤트로 쏘는 두 신호에 의존한다.
    //   DamageCastTrigger  → 실제 타격 판정
    //   AnimationEndTrigger → 스킬 종료 (없으면 IsUsing 이 영원히 true 로 남는다)
    // 그래서 클립을 만들 때 함께 박아준다. 함수 이름은 AgentRenderer 의 메서드명과 같아야 한다.
    private static AnimationEvent[] BuildEvents(string rowName, int frameCount)
    {
        // 루프하는 Idle/Run 에는 넣지 않는다
        bool isAttack = rowName.StartsWith("Attack");
        bool needsEnd = isAttack || rowName.StartsWith("Hurt") || rowName == "Death";
        if (!needsEnd || frameCount == 0) return new AnimationEvent[0];

        var events = new List<AnimationEvent>();
        int lastFrame = frameCount - 1;

        if (isAttack)
        {
            // 타격 프레임은 클립의 일정 비율 지점으로 고정한다. 체감이 어색하면 이 값만 조정.
            int castFrame = Mathf.Clamp(Mathf.RoundToInt(lastFrame * DamageCastRatio), 0, lastFrame);
            events.Add(new AnimationEvent
            {
                time = castFrame / Fps,
                functionName = "DamageCastTrigger",
            });
        }

        events.Add(new AnimationEvent
        {
            time = lastFrame / Fps,
            functionName = "AnimationEndTrigger",
        });

        return events.ToArray(); // 시간 오름차순이어야 한다
    }

    private static AnimationClip CreateClip(string path, List<Sprite> frames, bool loop, string rowName)
    {
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

        AnimationUtility.SetAnimationEvents(clip, BuildEvents(rowName, frames.Count));

        if (isNew) AssetDatabase.CreateAsset(clip, path);
        else EditorUtility.SetDirty(clip);
        return clip;
    }

    // 액션별 블렌드 트리. 옆모습은 좌우 양쪽에 같은 클립을 넣고,
    // 실제 좌우 반전은 런타임에 SpriteRenderer.flipX 로 처리한다.
    private static void BuildController(string path, Dictionary<string, AnimationClip> clips)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        var sm = controller.layers[0].stateMachine;
        foreach (var t in sm.anyStateTransitions.ToArray()) sm.RemoveAnyStateTransition(t);
        foreach (var s in sm.states.ToArray()) sm.RemoveState(s.state);
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            if (obj is BlendTree bt) Object.DestroyImmediate(bt, true);

        foreach (var p in controller.parameters.ToArray()) controller.RemoveParameter(p);
        controller.AddParameter(PMoveX, AnimatorControllerParameterType.Float);
        controller.AddParameter(PMoveY, AnimatorControllerParameterType.Float);
        var ps = controller.parameters;
        for (int i = 0; i < ps.Length; i++)
            if (ps[i].name == PMoveY) ps[i].defaultFloat = -1f;
        controller.parameters = ps;

        AnimatorState first = null;
        float y = 0f;

        foreach (string action in DirectionalActions)
        {
            bool hasAny = clips.Keys.Any(k => k.StartsWith(action + "_"));
            if (!hasAny) continue;

            var state = controller.CreateBlendTreeInController(action, out BlendTree tree, 0);
            tree.name = action;
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = PMoveX;
            tree.blendParameterY = PMoveY;

            if (clips.TryGetValue($"{action}_Down", out var down)) tree.AddChild(down, new Vector2(0, -1));
            if (clips.TryGetValue($"{action}_Up", out var up)) tree.AddChild(up, new Vector2(0, 1));
            if (clips.TryGetValue($"{action}_Side", out var side))
            {
                tree.AddChild(side, new Vector2(1, 0));
                tree.AddChild(side, new Vector2(-1, 0)); // 좌측은 flipX로 처리
            }

            PlaceState(sm, state, new Vector3(320, y, 0));
            y += 110;
            if (first == null) first = state;
        }

        if (clips.TryGetValue("Death", out var deathClip))
        {
            // 상태 이름은 플레이어와 같은 "Dead" 로 맞춘다.
            // HashData 에셋(Dead Hash, HashName="Dead")을 적에게도 그대로 쓸 수 있어야 한다.
            var deathState = sm.AddState("Dead");
            deathState.motion = deathClip;
            PlaceState(sm, deathState, new Vector3(640, 0, 0));
        }

        if (first != null) sm.defaultState = first;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    private static void PlaceState(AnimatorStateMachine sm, AnimatorState state, Vector3 pos)
    {
        var arr = sm.states;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].state == state) arr[i].position = pos;
        sm.states = arr;
    }
}
