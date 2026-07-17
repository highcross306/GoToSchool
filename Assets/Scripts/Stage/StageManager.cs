// ============================================================
// StageManager.cs
// 역할: 씬 로드 시 자동으로 스테이지 구성
//       stageData는 각 씬의 Inspector에서 직접 연결
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("이 씬의 스테이지 데이터 (Inspector에서 직접 연결)")]
    public StageData stageData;

    [Header("프리팹 연결")]
    public GameObject nodePrefab;
    public GameObject routePrefab;

    [Header("부모 오브젝트")]
    public Transform nodesParent;
    public Transform routesParent;

    public StageData CurrentStageData { get; private set; }

    private Dictionary<NodeData, Node> nodeMap = new();
    private Dictionary<RouteData, Route> routeMap = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 씬 로드 시 자동으로 스테이지 구성
    private void Start()
    {
        if (stageData == null)
        {
            Debug.LogError("[StageManager] stageData가 연결되지 않았습니다. Inspector를 확인하세요.");
            return;
        }
        BuildStage(stageData);
    }

    public void BuildStage(StageData data)
    {
        CurrentStageData = data;
        ClearStage();
        InputLock.Unlock();

        if (MusicManager.Instance != null)
            MusicManager.Instance.StartNewStageMusic();

        foreach (NodeData nodeData in data.allNodes)
            SpawnNode(nodeData);

        foreach (NodeData nodeData in data.allNodes)
            foreach (RouteData routeData in nodeData.outgoingRoutes)
                SpawnRoute(routeData);

        NodeData startNode = FindStartNode(data);
        PlanningManager.Instance.Initialize(startNode);
        PlayerBudget.Instance.Initialize(data.initialBudget, data.timeLimitSeconds);
        PlanningUI.Instance.ResetAllCardsToDefault();

        if (startNode != null && PlayerMover.Instance != null)
        {
            PlayerMover.Instance.transform.position = (Vector3)startNode.position;
            PlayerMover.Instance.ResetSpeed();
        }

        // 스테이지 구성이 끝난 뒤 스토리 인트로 팝업 표시 (없으면 조용히 무시됨)
        if (StoryIntroPanel.Instance != null)
            StoryIntroPanel.Instance.Show(data.storyTitle, data.storyText);
    }

    public Node GetNode(NodeData data)
    {
        nodeMap.TryGetValue(data, out Node node);
        return node;
    }

    private void SpawnNode(NodeData nodeData)
    {
        GameObject obj = Instantiate(nodePrefab, nodeData.position, Quaternion.identity, nodesParent);
        obj.name = nodeData.name;
        Node node = obj.GetComponent<Node>();
        node.Initialize(nodeData);
        nodeMap[nodeData] = node;
    }

    private void SpawnRoute(RouteData routeData)
    {
        GameObject obj = Instantiate(routePrefab, Vector3.zero, Quaternion.identity, routesParent);
        obj.name = routeData.name;
        Route route = obj.GetComponent<Route>();
        route.Initialize(routeData);
        routeMap[routeData] = route;
    }

    private NodeData FindStartNode(StageData data)
    {
        foreach (NodeData node in data.allNodes)
            if (node.nodeType == NodeType.Start) return node;

        Debug.LogError("[StageManager] Start 노드를 찾을 수 없습니다.");
        return null;
    }

    // RouteData로 Route 컴포넌트 조회
    public Route GetRoute(RouteData data)
    {
        routeMap.TryGetValue(data, out Route route);
        return route;
    }

    private void ClearStage()
    {
        nodeMap.Clear();
        routeMap.Clear();
        foreach (Transform child in nodesParent) Destroy(child.gameObject);
        foreach (Transform child in routesParent) Destroy(child.gameObject);
    }
}