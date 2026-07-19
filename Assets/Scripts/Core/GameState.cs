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

    // 스테이지를 클리어할 때마다 ResultEvaluator에서 누적.
    // 4개 스테이지를 모두 마치면 EndingUI가 이 값을 사용해 별점/코멘트를 표시한다.
    public static int TotalScore { get; set; } = 0;

    // 메인 메뉴에서 "게임 시작"을 누를 때 호출 — 이전 플레이의 누적 상태를 초기화
    public static void ResetForNewGame()
    {
        CurrentStage = 1;
        TotalScore = 0;
    }
}