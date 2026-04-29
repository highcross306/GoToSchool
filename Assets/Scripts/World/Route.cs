// ============================================================
// Route.cs
// 역할: 씬에 배치된 경로 오브젝트
//       RouteData를 참조해 두 노드 사이에 선을 그림
//       ClickableRoute로부터 클릭 이벤트를 받아 PlanningManager에 전달
// ============================================================

using UnityEngine;

public class Route : MonoBehaviour
{
    public RouteData Data { get; private set; }

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // StageManager가 생성 직후 호출
    public void Initialize(RouteData data)
    {
        Data = data;
        DrawLine();
    }

    // fromNode ~ toNode 사이에 선을 그림
    private void DrawLine()
    {
        Node fromNode = FindNodeById(Data.fromNodeId);
        Node toNode = FindNodeById(Data.toNodeId);

        if (fromNode == null || toNode == null)
        {
            Debug.LogWarning($"[Route] 노드를 찾을 수 없습니다: {Data.fromNodeId} → {Data.toNodeId}");
            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, fromNode.transform.position);
        lineRenderer.SetPosition(1, toNode.transform.position);
    }

    // ID로 씬 내 Node 검색
    private Node FindNodeById(string id)
    {
        GameObject obj = GameObject.Find(id);
        return obj != null ? obj.GetComponent<Node>() : null;
    }
}