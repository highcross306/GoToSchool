// ============================================================
// DisplayNames.cs
// 역할: NodeType / TransportType의 한글 표시 이름 변환.
//       이벤트 미리보기 팝업, 실제 이벤트 메시지 등
//       "스테이지_4_UI_흐름.pdf"에서 요구하는 한글 문구 생성에 사용.
//
//       매핑이 없는 타입은 enum 이름 그대로 반환해 조용히 깨지지 않는다.
// ============================================================

public static class DisplayNames
{
    public static string ToKorean(this NodeType type)
    {
        switch (type)
        {
            case NodeType.Start: return "출발 노드";
            case NodeType.Checkpoint: return "경유 노드";
            case NodeType.End: return "도착 노드";
            case NodeType.Enhanced_BusStop: return "버스 정류장";
            case NodeType.Enhanced_Slum: return "슬럼";
            case NodeType.Enhanced_ConstructionZone: return "공사구간";
            case NodeType.Enhanced_TaxiStand: return "택시 승강장";
            default: return type.ToString();
        }
    }

    public static string ToKorean(this TransportType transport)
    {
        switch (transport)
        {
            case TransportType.Walk: return "걷기";
            case TransportType.Bus: return "버스";
            case TransportType.Taxi: return "택시";
            default: return transport.ToString();
        }
    }
}