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
    public GameObject nodePrefab;  // 인스펙터에서 Node 프리팹 연결
    public GameObject routePrefab; // 인스펙터에서 Route 프리팹 연결

    [Header("부모 오브젝트")]
    public Transform nodesParent;  // Hierarchy의 World/Nodes
    public Transform routesParent; // Hierarchy의 World/Routes

    // 현재 스테이지 데이터 (다른 스크립트에서 참조 가능)
    public StageData CurrentStageData { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // StageData를 받아 씬에 오브젝트 생성
    public void BuildStage(StageData data)
    {
        CurrentStageData = data;
        ClearStage();

        foreach (NodeData nodeData in data.nodes)
            SpawnNode(nodeData);

        foreach (RouteData routeData in data.routes)
            SpawnRoute(routeData);
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

    // 기존 오브젝트 전부 제거 (스테이지 재시작 시 사용)
    private void ClearStage()
    {
        foreach (Transform child in nodesParent) Destroy(child.gameObject);
        foreach (Transform child in routesParent) Destroy(child.gameObject);
    }
}