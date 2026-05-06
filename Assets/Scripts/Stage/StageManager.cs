// ============================================================
// StageManager.cs
// 역할: StageData를 읽어 Node/Route 프리팹을 씬에 생성
//       씬에 [GameManager] 오브젝트에 부착
// ============================================================

using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("프리팹 연결")]
    public GameObject nodePrefab;
    public GameObject routePrefab;

    [Header("부모 오브젝트")]
    public Transform nodesParent;
    public Transform routesParent;

    public StageData CurrentStageData { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void BuildStage(StageData data)
    {
        CurrentStageData = data;
        ClearStage();

        // 입력 잠금 해제 (스테이지 시작/재시작 시)
        InputLock.Unlock();

        foreach (NodeData nodeData in data.nodes)
            SpawnNode(nodeData);

        foreach (RouteData routeData in data.routes)
            SpawnRoute(routeData);

        // 시작 노드 ID를 PlanningManager에 전달
        string startNodeId = FindStartNodeId(data);
        PlanningManager.Instance.Initialize(startNodeId);
        PlayerBudget.Instance.Initialize(data.initialBudget, data.timeLimitSeconds);
    }

    private string FindStartNodeId(StageData data)
    {
        foreach (NodeData node in data.nodes)
        {
            if (node.nodeType == NodeType.Start) return node.id;
        }
        return "";
    }

    private void SpawnNode(NodeData nodeData)
    {
        GameObject obj = Instantiate(nodePrefab, nodeData.position, Quaternion.identity, nodesParent);
        obj.name = nodeData.id;
        obj.GetComponent<Node>().Initialize(nodeData);
    }

    private void SpawnRoute(RouteData routeData)
    {
        GameObject obj = Instantiate(routePrefab, Vector3.zero, Quaternion.identity, routesParent);
        obj.name = routeData.id;
        obj.GetComponent<Route>().Initialize(routeData);
    }

    private void ClearStage()
    {
        foreach (Transform child in nodesParent) Destroy(child.gameObject);
        foreach (Transform child in routesParent) Destroy(child.gameObject);
    }
}