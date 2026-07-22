using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools > Arrange Upgrade UI 실행 시 기존 UI를 PixelArtGUI 스프라이트로 재배치한다.
// 오브젝트를 삭제하지 않고 이동/스킨만 하므로 GoldManager 참조는 유지된다.
public static class UpgradeUIArranger
{
    private const string Tex = "Assets/Asset/PixelArtGUI/Textures/";
    private static readonly Color DarkLabel = new Color32(0x33, 0x24, 0x1A, 0xFF);

    [MenuItem("Tools/Arrange Upgrade UI")]
    public static void Arrange()
    {
        // UI 버튼이 실제로 들어있는 캔버스를 찾는다 (피드백용 캔버스 제외)
        Canvas canvas = null;
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.transform.Find("UpgradeBuuton") != null) { canvas = c; break; }
        }
        if (canvas == null) { Debug.LogError("[Arranger] UpgradeBuuton이 있는 Canvas를 찾지 못함"); return; }
        Transform root = canvas.transform;

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f; // 가로형: 높이 기준

        // ── 좌상단: 코인 패널 ──────────────────────────
        RectTransform coinPanel = Panel(root, "CoinPanel", "Panels/TitleBarGold.png",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -40), new Vector2(420, 90));
        Icon(coinPanel, "CoinIcon", "Icons/32/PouchGreenCoinsGold.png", new Vector2(24, 0), 52);
        PutText(root, "Coin", coinPanel, 90, 20, TextAlignmentOptions.Left);

        // ── 중앙 상단: 현재 아이템 + 확률 ──────────────
        RectTransform titlePanel = Panel(root, "TitlePanel", "Panels/TitleBarGoldBig.png",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(700, 100));
        PutText(root, "CurrentSquare", titlePanel, 40, 40, TextAlignmentOptions.Center);
        Place(root, "Percent", new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -150), new Vector2(300, 60), TextAlignmentOptions.Center);

        // ── 우상단: 퀘스트 패널 ────────────────────────
        RectTransform questPanel = Panel(root, "QuestPanel", "Panels/UniversalPanel1.png",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -40), new Vector2(460, 90));
        PutText(root, "Quest", questPanel, 24, 24, TextAlignmentOptions.Center);

        // ── 하단 중앙: 강화 / 판매 ─────────────────────
        Skin(root, "UpgradeBuuton", "Buttons/Green1", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(-190, 50), new Vector2(340, 110));
        Place(root, "UpgradePrice", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(-190, 175), new Vector2(340, 50), TextAlignmentOptions.Center);
        Skin(root, "SellButton", "Buttons/Gold1", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(190, 50), new Vector2(340, 110));
        Place(root, "Sell Price", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(190, 175), new Vector2(340, 50), TextAlignmentOptions.Center);

        // ── 우하단: 전투 ───────────────────────────────
        RectTransform fight = Skin(root, "FightButton", "Buttons/GoldOutlined", new Vector2(1, 0),
            new Vector2(1, 0), new Vector2(-40, 50), new Vector2(320, 120), labelLeftPad: 90);
        if (fight != null) Icon(fight, "SwordIcon", "Icons/32/Sword01.png", new Vector2(28, 0), 56);

        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Arranger] UI 재배치 완료");
    }

    private static Sprite Load(string relPath) =>
        AssetDatabase.LoadAssetAtPath<Sprite>(Tex + relPath);

    // 텍스트가 rect 밖으로 넘치지 않도록: 자동 크기 + 줄바꿈 금지
    private static void Fit(TextMeshProUGUI tmp, float min, float max)
    {
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = min;
        tmp.fontSizeMax = max;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private static RectTransform SetRect(Transform t, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var rt = (RectTransform)t;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    // 배경 패널 생성 (이미 있으면 재사용)
    private static RectTransform Panel(Transform root, string name, string sprite,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        Transform t = root.Find(name);
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.layer = 5;
            t = go.transform;
            t.SetParent(root, false);
        }
        var img = t.GetComponent<Image>();
        img.sprite = Load(sprite);
        img.type = Image.Type.Sliced;
        return SetRect(t, anchor, pivot, pos, size);
    }

    // 패널 안에 아이콘 생성 (좌측 기준)
    private static void Icon(RectTransform parent, string name, string sprite, Vector2 pos, float size)
    {
        Transform t = parent.Find(name);
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.layer = 5;
            t = go.transform;
            t.SetParent(parent, false);
        }
        var img = t.GetComponent<Image>();
        img.sprite = Load(sprite);
        img.raycastTarget = false;
        SetRect(t, new Vector2(0, 0.5f), new Vector2(0, 0.5f), pos, new Vector2(size, size));
    }

    // 기존 텍스트를 패널 안으로 옮기고 좌우 여백을 두고 스트레치
    private static void PutText(Transform root, string name, RectTransform panel,
        float left, float right, TextAlignmentOptions align)
    {
        Transform t = root.Find(name);
        if (t == null) t = panel.Find(name); // 재실행 시 이미 패널 안에 있음
        if (t == null) { Debug.LogWarning($"[Arranger] '{name}' 없음"); return; }

        t.SetParent(panel, false);
        var rt = (RectTransform)t;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(left, 8);
        rt.offsetMax = new Vector2(-right, -8);
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.alignment = align;
            Fit(tmp, 18, 42);
        }
    }

    // 단독 텍스트 배치
    private static void Place(Transform root, string name, Vector2 anchor, Vector2 pivot,
        Vector2 pos, Vector2 size, TextAlignmentOptions align)
    {
        Transform t = root.Find(name);
        if (t == null) { Debug.LogWarning($"[Arranger] '{name}' 없음"); return; }
        SetRect(t, anchor, pivot, pos, size);
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.alignment = align;
            Fit(tmp, 18, 38);
        }
    }

    // 버튼 스킨: 스프라이트 + 눌림/호버 상태 + 라벨 색
    private static RectTransform Skin(Transform root, string name, string spriteBase,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, float labelLeftPad = 24)
    {
        Transform t = root.Find(name);
        if (t == null) { Debug.LogWarning($"[Arranger] '{name}' 없음"); return null; }

        var img = t.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = Load(spriteBase + ".png");
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        var btn = t.GetComponent<Button>();
        if (btn != null)
        {
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = new SpriteState
            {
                highlightedSprite = Load(spriteBase + "Hover.png"),
                pressedSprite = Load(spriteBase + "Down.png"),
                selectedSprite = Load(spriteBase + "Hover.png")
            };
        }

        var label = t.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.color = DarkLabel;
            label.alignment = TextAlignmentOptions.Center;
            Fit(label, 18, 44);
            // 라벨을 버튼 크기에 맞춰 스트레치 (기존 고정 크기 때문에 줄바꿈되던 문제 해결)
            var lrt = (RectTransform)label.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.offsetMin = new Vector2(labelLeftPad, 10);
            lrt.offsetMax = new Vector2(-24, -10);
        }

        return SetRect(t, anchor, pivot, pos, size);
    }
}
