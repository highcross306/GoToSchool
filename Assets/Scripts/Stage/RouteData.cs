// ============================================================
// RouteData.cs
// 역할: 경로 하나의 데이터 ScriptableObject
//       fromNode, toNode를 문자열이 아닌 객체로 직접 참조
// 생성: Assets/ScriptableObjects/Routes 폴더 우클릭
//       → Create → Game → Route Data
// ============================================================

using UnityEngine;

[CreateAssetMenu(fileName = "RouteData", menuName = "Game/Route Data")]
public class RouteData : ScriptableObject
{
    public NodeData fromNode;
    public NodeData toNode;
    public TransportType[] allowedTransports;
}