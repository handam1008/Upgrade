using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Tools > Setup Player Animator (Blend Trees)
// FSM에서 상태 전환을 직접 관리하는 구조에 맞춰,
// 액션별 4방향 블렌드 트리(2D Simple Directional)만 구성한다.
// 전이/트리거는 만들지 않고 파라미터는 MoveX, MoveY만 둔다.
public static class PlayerAnimatorSetup
{
    private const string ControllerPath = "Assets/_Graphics/Animations/Player Cotroller.controller";
    private const string ClipDir = "Assets/_Graphics/Animations/PlayerClip";

    private const string PMoveX = "MoveX", PMoveY = "MoveY";

    private static readonly (string dir, float x, float y)[] Dirs =
    {
        ("down", 0f, -1f), ("up", 0f, 1f), ("left", -1f, 0f), ("right", 1f, 0f)
    };

    private static readonly (string state, string action, float x, float y)[] Actions =
    {
        ("Idle",   "idle",   300f,   0f),
        ("Run",    "run",    300f, 120f),
        ("Attack", "attack", 650f, -100f),
        ("Dash",   "dash",   650f,  20f),
        ("Heal",   "heal",   650f, 140f),
        ("Hurt",   "hurt",   650f, 260f),
        ("Dead",   "dead",   650f, 380f),
    };

    [MenuItem("Tools/Setup Player Animator (Blend Trees)")]
    public static void Setup()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError("[AnimSetup] 컨트롤러를 찾을 수 없음: " + ControllerPath);
            return;
        }

        var clips = new Dictionary<string, AnimationClip>();
        foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { ClipDir }))
        {
            var c = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid));
            if (c != null) clips[c.name] = c;
        }
        if (clips.Count == 0) { Debug.LogError("[AnimSetup] 클립을 찾을 수 없음: " + ClipDir); return; }

        var sm = controller.layers[0].stateMachine;

        // 기존 상태/전이 정리
        foreach (var t in sm.anyStateTransitions.ToArray()) sm.RemoveAnyStateTransition(t);
        foreach (var t in sm.entryTransitions.ToArray()) sm.RemoveEntryTransition(t);
        foreach (var s in sm.states.ToArray())
        {
            foreach (var t in s.state.transitions.ToArray()) s.state.RemoveTransition(t);
            sm.RemoveState(s.state);
        }
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(ControllerPath))
            if (obj is BlendTree bt) Object.DestroyImmediate(bt, true);

        // 파라미터: 방향 2개만
        foreach (var p in controller.parameters.ToArray()) controller.RemoveParameter(p);
        controller.AddParameter(PMoveX, AnimatorControllerParameterType.Float);
        controller.AddParameter(PMoveY, AnimatorControllerParameterType.Float);
        SetDefaultFloat(controller, PMoveY, -1f); // 아래를 보고 시작

        // 액션별 블렌드 트리 (전이 없음 — FSM이 Play/CrossFade로 직접 진입)
        AnimatorState first = null;
        int found = 0;
        foreach (var (stateName, action, px, py) in Actions)
        {
            var state = controller.CreateBlendTreeInController(stateName, out BlendTree tree, 0);
            tree.name = stateName;
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = PMoveX;
            tree.blendParameterY = PMoveY;

            foreach (var (dir, x, y) in Dirs)
            {
                if (!clips.TryGetValue($"{action}_{dir}", out var clip))
                {
                    Debug.LogWarning($"[AnimSetup] 클립 없음: {action}_{dir}");
                    continue;
                }
                tree.AddChild(clip, new Vector2(x, y));
                found++;
            }

            var arr = sm.states;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i].state == state) arr[i].position = new Vector3(px, py, 0);
            sm.states = arr;

            if (first == null) first = state;
        }
        sm.defaultState = first; // Idle

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ControllerPath, ImportAssetOptions.ForceUpdate);

        Debug.Log($"[AnimSetup] 블렌드 트리 {Actions.Length}개 구성 완료 (클립 {found}/28, " +
                  $"파라미터 MoveX/MoveY, 전이 없음) → {ControllerPath}");
    }

    private static void SetDefaultFloat(AnimatorController controller, string name, float value)
    {
        var ps = controller.parameters;
        for (int i = 0; i < ps.Length; i++)
            if (ps[i].name == name) ps[i].defaultFloat = value;
        controller.parameters = ps;
    }
}
