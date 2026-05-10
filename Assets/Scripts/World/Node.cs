// ============================================================
// Node.cs
// 역할: 씬에 배치된 노드 오브젝트
//       NodeData를 참조해 노드 타입별 스프라이트 적용
//       ClickableNode로부터 클릭 이벤트를 받아 PlanningManager에 전달
// ============================================================

using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("노드 타입별 스프라이트")]
    public Sprite startSprite;      // 출발 노드 스프라이트
    public Sprite checkpointSprite; // 거점 노드 스프라이트
    public Sprite endSprite;        // 도착 노드 스프라이트

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

    // ClickableNode가 클릭을 감지하면 이 메서드 호출
    public void OnClicked()
    {
        PlanningManager.Instance.OnNodeClicked(this);
    }
}