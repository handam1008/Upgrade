using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;

// Tools > Build Sprite Libraries
//
// 이미 슬라이스된 Cute Fantasy Characters 스프라이트로
//   ① 캐릭터별 .spriteLib (Sprite Library Asset)
//   ② 기존 클립을 SpriteResolver 의 m_SpriteHash 를 돌리도록 변환
// 두 가지를 만든다. 플레이어(PlayerLibrary + PlayerClip)와 같은 파이프라인이 된다.
//
// 카테고리/라벨 규칙은 플레이어와 동일하게 소문자로 맞춘다.
//   category = idle_down          label = idle_down_0, idle_down_1, ...
public static class SpriteLibraryBuilder
{
    private const string SheetRoot = "Assets/_Graphics/Asset/Cute_Fantasy_Characters";
    private const string ClipRoot = "Assets/_Graphics/Animations/Enemies";
    private const string LibDir = "Assets/_Graphics/Animations/SpritePack";
    private const float Fps = 10f;

    // SpriteLibrarySourceAsset 의 스크립트 GUID. PlayerLibrary.spriteLib 에서 그대로 가져온 값.
    private const string SourceAssetScriptGuid = "a5e6fedc2472449cead18ef23b5cb30d";

    [MenuItem("Tools/Build Sprite Libraries")]
    public static void Build()
    {
        string[] sheets = Directory.GetFiles(SheetRoot, "*.png", SearchOption.AllDirectories)
            .Select(p => p.Replace('\\', '/'))
            .OrderBy(p => p)
            .ToArray();

        if (sheets.Length == 0)
        {
            Debug.LogError($"[SpriteLib] 캐릭터 시트를 찾지 못했습니다: {SheetRoot}");
            return;
        }

        int libCount = 0, clipCount = 0;
        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (string sheetPath in sheets)
            {
                string charName = Path.GetFileNameWithoutExtension(sheetPath);

                // 시트에서 잘려나온 스프라이트를 행 이름별로 묶는다.
                // 스프라이트 이름 규칙: {charName}_{Row}_{index}
                var byCategory = new SortedDictionary<string, List<Sprite>>();
                foreach (Sprite sprite in AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>())
                {
                    if (!TryParse(sprite.name, charName, out string row, out int index)) continue;

                    string category = row.ToLowerInvariant();
                    if (!byCategory.TryGetValue(category, out var list))
                        byCategory[category] = list = new List<Sprite>();

                    while (list.Count <= index) list.Add(null);
                    list[index] = sprite;
                }

                foreach (var list in byCategory.Values) list.RemoveAll(s => s == null);
                var categories = byCategory.Where(kv => kv.Value.Count > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                if (categories.Count == 0)
                {
                    Debug.LogWarning($"[SpriteLib] {charName}: 슬라이스된 스프라이트가 없습니다. " +
                                     "먼저 Tools > Build Cute Fantasy Characters 를 실행하세요.");
                    continue;
                }

                WriteLibrary(charName, categories);
                libCount++;
                clipCount += ConvertClips(charName, categories);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        Debug.Log($"[SpriteLib] 라이브러리 {libCount}개, 클립 {clipCount}개 변환 완료 → {LibDir}");
    }

    // "Angel_1_Idle_Down_3" → row="Idle_Down", index=3
    private static bool TryParse(string spriteName, string charName, out string row, out int index)
    {
        row = null;
        index = 0;

        string prefix = charName + "_";
        if (!spriteName.StartsWith(prefix, StringComparison.Ordinal)) return false;

        string rest = spriteName.Substring(prefix.Length);
        int underscore = rest.LastIndexOf('_');
        if (underscore <= 0) return false;

        if (!int.TryParse(rest.Substring(underscore + 1), out index)) return false;
        row = rest.Substring(0, underscore);
        return true;
    }

    // ── .spriteLib 작성 ──────────────────────────────────────
    //
    // SpriteLibrarySourceAsset 을 만드는 공개 API 가 없어서 YAML 을 직접 쓴다.
    // 형식은 PlayerLibrary.spriteLib 과 동일하다.
    private static void WriteLibrary(string charName, Dictionary<string, List<Sprite>> categories)
    {
        var sb = new StringBuilder();
        sb.AppendLine("%YAML 1.1");
        sb.AppendLine("%TAG !u! tag:unity3d.com,2011:");
        sb.AppendLine("--- !u!114 &1");
        sb.AppendLine("MonoBehaviour:");
        sb.AppendLine("  m_ObjectHideFlags: 0");
        sb.AppendLine("  m_CorrespondingSourceObject: {fileID: 0}");
        sb.AppendLine("  m_PrefabInstance: {fileID: 0}");
        sb.AppendLine("  m_PrefabAsset: {fileID: 0}");
        sb.AppendLine("  m_GameObject: {fileID: 0}");
        sb.AppendLine("  m_Enabled: 1");
        sb.AppendLine("  m_EditorHideFlags: 0");
        sb.AppendLine($"  m_Script: {{fileID: 11500000, guid: {SourceAssetScriptGuid}, type: 3}}");
        sb.AppendLine("  m_Name: ");
        sb.AppendLine("  m_EditorClassIdentifier: Unity.2D.Animation.Runtime::UnityEngine.U2D.Animation.SpriteLibrarySourceAsset");
        sb.AppendLine("  m_Library:");

        foreach (var (category, sprites) in categories.Select(kv => (kv.Key, kv.Value)))
        {
            sb.AppendLine($"  - m_Name: {category}");
            sb.AppendLine($"    m_Hash: {StringHash(category)}");
            sb.AppendLine("    m_CategoryList: []");
            sb.AppendLine("    m_OverrideEntries:");

            for (int i = 0; i < sprites.Count; i++)
            {
                string label = $"{category}_{i}";
                string reference = SpriteReference(sprites[i]);

                sb.AppendLine($"    - m_Name: {label}");
                sb.AppendLine($"      m_Hash: {StringHash(label)}");
                sb.AppendLine($"      m_Sprite: {reference}");
                sb.AppendLine("      m_FromMain: 0");
                sb.AppendLine($"      m_SpriteOverride: {reference}");
            }

            sb.AppendLine("    m_FromMain: 0");
            sb.AppendLine($"    m_EntryOverrideCount: {sprites.Count}");
        }

        sb.AppendLine("  m_PrimaryLibraryGUID: ");
        sb.AppendLine($"  m_ModificationHash: {DateTime.Now.Ticks}");
        sb.AppendLine("  m_Version: 1");

        if (!AssetDatabase.IsValidFolder(LibDir))
            AssetDatabase.CreateFolder("Assets/_Graphics/Animations", "SpritePack");

        File.WriteAllText($"{LibDir}/{charName}Library.spriteLib", sb.ToString(), new UTF8Encoding(false));
    }

    private static string SpriteReference(Sprite sprite)
    {
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long localId))
            return $"{{fileID: {localId}, guid: {guid}, type: 3}}";

        Debug.LogWarning($"[SpriteLib] 스프라이트 참조를 얻지 못했습니다: {sprite.name}");
        return "{fileID: 0}";
    }

    // ── 클립 변환 ────────────────────────────────────────────
    //
    // SpriteRenderer.m_Sprite (PPtr 커브) → SpriteResolver.m_SpriteHash (이산 int 커브)
    // 값은 Unity 내부 규칙과 같게 "{category}_{label}" 의 30비트 해시를 float 비트로 담는다.
    private static int ConvertClips(string charName, Dictionary<string, List<Sprite>> categories)
    {
        string dir = $"{ClipRoot}/{charName}";
        if (!AssetDatabase.IsValidFolder(dir)) return 0;

        var oldBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var newBinding = EditorCurveBinding.DiscreteCurve("", typeof(SpriteResolver), "m_SpriteHash");

        int converted = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { dir }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;

            string category = Path.GetFileNameWithoutExtension(path)
                .Substring(charName.Length + 1).ToLowerInvariant();
            if (!categories.TryGetValue(category, out var sprites)) continue;

            var curve = new AnimationCurve();
            for (int i = 0; i < sprites.Count; i++)
            {
                int hash = StringHash($"{category}_{category}_{i}");
                curve.AddKey(new Keyframe(i / Fps, AsFloat(hash))
                {
                    inTangent = float.PositiveInfinity,   // 계단식 — 중간값 보간 금지
                    outTangent = float.PositiveInfinity,
                });
            }

            AnimationUtility.SetObjectReferenceCurve(clip, oldBinding, null); // 옛 커브 제거
            AnimationUtility.SetEditorCurve(clip, newBinding, curve);
            EditorUtility.SetDirty(clip);
            converted++;
        }

        return converted;
    }

    // SpriteLibraryUtility.GetStringHash 과 동일: Animator.StringToHash 의 하위 30비트
    private static int StringHash(string value) => Animator.StringToHash(value) & 0x3FFFFFFF;

    // int 를 그대로 float 비트에 담는다. Unity 가 m_SpriteHash 커브를 저장하는 방식.
    private static float AsFloat(int value) => BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
}
