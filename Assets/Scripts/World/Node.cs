// ============================================================
// Node.cs
// 역할: 씬에 배치된 노드 오브젝트
//       NodeData를 참조해 노드 타입별 스프라이트 적용
//       결정 완료된 노드는 시각적으로 비활성화 표시
// ============================================================

using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("기본 노드 스프라이트")]
    public Sprite startSprite;
    public Sprite checkpointSprite;
    public Sprite endSprite;

    [Header("강화 노드 기본 스프라이트 (NodeData.customSprite가 없을 때 사용)")]
    public Sprite enhancedSprite;

    [Header("결정 완료 시 색상 (반투명 처리)")]
    public Color decidedColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("강조 오브젝트 (Node 프리팹 자식으로 배치)")]
    public GameObject highlightObject; // 강조 이미지 자식 오브젝트

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
        // NodeData에 customSprite가 있으면 최우선 적용
        if (Data.customSprite != null)
        {
            spriteRenderer.sprite = Data.customSprite;
            return;
        }

        spriteRenderer.sprite = Data.nodeType switch
        {
            NodeType.Start => startSprite,
            NodeType.Checkpoint => checkpointSprite,
            NodeType.End => endSprite,
            _ => enhancedSprite // 강화 노드 공통 스프라이트
        };
    }

    // PlanningManager가 결정 완료 시 호출 → 반투명 처리
    public void SetDecided()
    {
        spriteRenderer.color = decidedColor;
    }

    // 노드 강조 표시 — 이동 가능한 노드에 강조 이미지 표시
    public void SetHighlighted(bool highlighted)
    {
        if (highlightObject != null)
            highlightObject.SetActive(highlighted);
    }

    // ClickableNode가 클릭을 감지하면 이 메서드 호출
    public void OnClicked()
    {
        PlanningManager.Instance.OnNodeClicked(this);
    }
}