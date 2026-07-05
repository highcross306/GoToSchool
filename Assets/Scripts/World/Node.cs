// ============================================================
// Node.cs
// 역할: 씬에 배치된 노드 오브젝트
//       NodeData를 참조해 노드 타입별 스프라이트 적용
//       결정 완료된 노드는 시각적으로 비활성화 표시
// ============================================================

using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("노드 타입별 스프라이트")]
    public Sprite startSprite;
    public Sprite checkpointSprite;
    public Sprite endSprite;

    [Header("결정 완료 시 색상 (반투명 처리)")]
    public Color decidedColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("강조 색상")]
    public Color highlightColor = new Color(1f, 0.9f, 0.3f, 1f); // 노란색

    public NodeData Data { get; private set; }

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // StageManager가 생성 직후 호출
    public void Initialize(NodeData data)
    {
        Data = data;
        spriteRenderer.color = Color.white;
        ApplyVisual();
    }

    // 노드 타입에 따라 스프라이트 적용
    private void ApplyVisual()
    {
        spriteRenderer.sprite = Data.nodeType switch
        {
            NodeType.Start => startSprite,
            NodeType.Checkpoint => checkpointSprite,
            NodeType.End => endSprite,
            _ => null
        };
    }

    // PlanningManager가 결정 완료 시 호출 → 반투명 처리
    public void SetDecided()
    {
        spriteRenderer.color = decidedColor;
    }

    // 노드 강조 표시 (노드 클릭 시)
    public void SetHighlighted(bool highlighted)
    {
        spriteRenderer.color = highlighted ? highlightColor : Color.white;
    }

    // ClickableNode가 클릭을 감지하면 이 메서드 호출
    public void OnClicked()
    {
        PlanningManager.Instance.OnNodeClicked(this);
    }
}