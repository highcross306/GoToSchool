// ============================================================
// GameState.cs
// 역할: 게임 전체 상태값을 보관하는 정적 클래스
// ============================================================

public enum Phase
{
    Planning,   // 선택 단계
    Execution,  // 실행 단계
    Result      // 결과 단계
}

public static class GameState
{
    public static Phase CurrentPhase { get; set; } = Phase.Planning;
    public static int CurrentStage { get; set; } = 1;
}