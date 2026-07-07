// ============================================================
// TransportEventTable.cs
// 역할: 대중교통별 이벤트를 보관하는 전역 ScriptableObject
//       스테이지 2부터 적용되는 공통 이벤트 목록
// 생성: Assets/ScriptableObjects/ 우클릭
//       → Create → Game → Transport Event Table
// ============================================================

using UnityEngine;

[CreateAssetMenu(fileName = "TransportEventTable", menuName = "Game/Transport Event Table")]
public class TransportEventTable : ScriptableObject
{
    [Header("도보 이벤트")]
    public GameEvent[] walkEvents;

    [Header("버스 이벤트")]
    public GameEvent[] busEvents;

    [Header("택시 이벤트")]
    public GameEvent[] taxiEvents;

    // 이동수단에 해당하는 이벤트 목록 반환
    public GameEvent[] GetEvents(TransportType transport)
    {
        return transport switch
        {
            TransportType.Walk => walkEvents,
            TransportType.Bus => busEvents,
            TransportType.Taxi => taxiEvents,
            _ => null
        };
    }
}