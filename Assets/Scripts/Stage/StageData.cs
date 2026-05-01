// ============================================================
// StageData.cs
// 역할: 스테이지 하나의 전체 설정을 담는 ScriptableObject
// 생성: Assets/ScriptableObjects/Stages 폴더 우클릭
//       → Create → Game → Stage Data
// ============================================================

using UnityEngine;

[CreateAssetMenu(
    fileName = "StageData",
    menuName = "Game/Stage Data"
)]
public class StageData : ScriptableObject
{
    [Header("스테이지 기본 설정")]
    public int stageIndex;       // 스테이지 번호 (1~4)
    public int timeLimitSeconds; // 제한 시간 (초) 예: 3600 = 60분
    public int initialBudget;    // 초기 자금 (원) 예: 2000

    [Header("노드 & 경로")]
    public NodeData[] nodes;
    public RouteData[] routes;
}