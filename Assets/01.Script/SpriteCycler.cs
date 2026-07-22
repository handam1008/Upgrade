using UnityEngine;

// 스프라이트 프레임을 일정 속도로 순환 (분수 등 환경 애니메이션용)
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteCycler : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 6f;

    private SpriteRenderer sr;
    private float timer;
    private int index;

    private void Awake() => sr = GetComponent<SpriteRenderer>();

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;
        timer += Time.deltaTime;
        if (timer < 1f / fps) return;
        timer -= 1f / fps;
        index = (index + 1) % frames.Length;
        sr.sprite = frames[index];
    }
}
