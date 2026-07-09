// ============================================================
// Route.cs
// 역할: 씬에 배치된 경로 오브젝트
//       NodeData.position으로 직접 위치 계산 (GameObject.Find 불필요)
// ============================================================

using UnityEngine;

public class Route : MonoBehaviour
{
    public RouteData Data { get; private set; }

    [Header("강조 오브젝트 (Route 프리팹 자식으로 배치)")]
    public GameObject highlightObject;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(RouteData data)
    {
        Data = data;
        PlaceSprite();
    }

    // 경로 강조 표시
    public void SetHighlighted(bool highlighted)
    {
        if (highlightObject != null)
            highlightObject.SetActive(highlighted);
    }

    private void PlaceSprite()
    {
        // NodeData.position에서 직접 좌표 가져옴 (탐색 없음)
        Vector3 fromPos = (Vector3)Data.fromNode.position;
        Vector3 toPos = (Vector3)Data.toNode.position;

        // 중간 지점에 배치
        transform.position = (fromPos + toPos) / 2f;

        // 방향에 맞게 회전
        Vector3 direction = toPos - fromPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 거리에 맞게 스케일 조정
        float distance = Vector3.Distance(fromPos, toPos);
        float spriteWidth = spriteRenderer.sprite != null
                               ? spriteRenderer.sprite.bounds.size.x : 1f;
        transform.localScale = new Vector3(distance / spriteWidth, 1f, 1f);
    }
}