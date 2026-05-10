// ============================================================
// Route.cs
// 역할: 씬에 배치된 경로 오브젝트
//       두 노드의 위치를 기반으로 스프라이트를 자동 배치/회전/스케일
// ============================================================

using UnityEngine;

public class Route : MonoBehaviour
{
    public RouteData Data { get; private set; }

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // StageManager가 생성 직후 호출
    public void Initialize(RouteData data)
    {
        Data = data;
        PlaceSprite();
    }

    private void PlaceSprite()
    {
        GameObject fromObj = GameObject.Find(Data.fromNodeId);
        GameObject toObj = GameObject.Find(Data.toNodeId);

        if (fromObj == null || toObj == null)
        {
            Debug.LogWarning($"[Route] 노드를 찾을 수 없습니다: {Data.fromNodeId} → {Data.toNodeId}");
            return;
        }

        Vector3 fromPos = fromObj.transform.position;
        Vector3 toPos = toObj.transform.position;

        // 1. 두 노드의 중간 지점에 배치
        transform.position = (fromPos + toPos) / 2f;

        // 2. 두 노드를 잇는 방향으로 회전
        Vector3 direction = toPos - fromPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 3. 두 노드 사이의 거리에 맞게 X축 스케일 조정
        float distance = Vector3.Distance(fromPos, toPos);
        float spriteWidth = spriteRenderer.sprite != null
                                 ? spriteRenderer.sprite.bounds.size.x
                                 : 1f;
        transform.localScale = new Vector3(distance / spriteWidth, 1f, 1f);
    }
}