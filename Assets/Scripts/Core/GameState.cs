// ============================================================
// GameState.cs
// 역할: 게임 전체 상태값을 보관하는 정적 클래스
// ============================================================

using System.Collections.Generic;

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

    // 스테이지별 점수를 개별로 기록한다. (키: 스테이지 번호, 값: 그 스테이지 점수)
    // 단순 += 누적이 아니라 스테이지별로 "덮어쓰기" 하는 이유:
    //   재시도(retry)로 같은 스테이지를 여러 번 클리어해도 점수가 중복 합산되지 않도록.
    private static readonly Dictionary<int, int> stageScores = new Dictionary<int, int>();

    // 해당 스테이지의 점수를 기록 (같은 스테이지를 다시 클리어하면 최신 점수로 교체)
    public static void RecordStageScore(int stage, int score)
    {
        stageScores[stage] = score;
    }

    // 기록된 모든 스테이지 점수의 합 — 엔딩 화면에서 별점/코멘트 판정에 사용
    public static int TotalScore
    {
        get
        {
            int sum = 0;
            foreach (int s in stageScores.Values) sum += s;
            return sum;
        }
    }

    // 특정 스테이지의 기록 점수 조회 (기록이 없으면 0)
    public static int GetStageScore(int stage)
    {
        return stageScores.TryGetValue(stage, out int v) ? v : 0;
    }

    // 메인 메뉴에서 "게임 시작"을 누를 때 호출 — 이전 플레이의 누적 상태를 초기화
    public static void ResetForNewGame()
    {
        CurrentStage = 1;
        stageScores.Clear();
    }
}