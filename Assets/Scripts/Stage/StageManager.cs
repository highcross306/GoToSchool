// ============================================================
// StageManager.cs
// 역할: StageData를 읽어 씬에 노드/경로 오브젝트 생성
//       nodeMap으로 NodeData ↔ Node 컴포넌트 빠른 조회 지원
// ============================================================

using System.Collections.Generic;
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

    // NodeData → Node 컴포넌트 빠른 조회
    private Dictionary<NodeData, Node> nodeMap = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void BuildStage(StageData data)
    {
        CurrentStageData = data;
        ClearStage();
        InputLock.Unlock();

        // 노드 생성
        foreach (NodeData nodeData in data.allNodes)
            SpawnNode(nodeData);

        // 각 노드의 outgoingRoutes에서 경로 생성
        foreach (NodeData nodeData in data.allNodes)
            foreach (RouteData routeData in nodeData.outgoingRoutes)
                SpawnRoute(routeData);

        // 시작 노드 탐색 후 PlanningManager 초기화
        NodeData startNode = FindStartNode(data);
        PlanningManager.Instance.Initialize(startNode);
        PlayerBudget.Instance.Initialize(data.initialBudget, data.timeLimitSeconds);

        // PlanningUI도 함께 초기화 — isConfirmed/isLocked 등 잔여 상태 제거
        // (재시작 시 결정 버튼이 영구적으로 잠기는 문제 방지)
        PlanningUI.Instance.ResetAllCardsToDefault();

        // 플레이어 위치를 시작 노드로 리셋
        // (재시작 시 이전 위치에 그대로 남아있는 문제 방지)
        if (startNode != null && PlayerMover.Instance != null)
        {
            PlayerMover.Instance.transform.position = (Vector3)startNode.position;
            PlayerMover.Instance.ResetSpeed(); // 배속 상태도 원래대로 초기화
        }
    }

    // NodeData로 Node 컴포넌트 조회
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
        obj.GetComponent<Route>().Initialize(routeData);
    }

    private NodeData FindStartNode(StageData data)
    {
        foreach (NodeData node in data.allNodes)
            if (node.nodeType == NodeType.Start) return node;
        return null;
    }

    private void ClearStage()
    {
        nodeMap.Clear();
        foreach (Transform child in nodesParent) Destroy(child.gameObject);
        foreach (Transform child in routesParent) Destroy(child.gameObject);
    }
}