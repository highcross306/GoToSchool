// ============================================================
// InputLock.cs
// 역할: 전역 입력 잠금 상태 관리
//       결정 버튼 클릭 후 실행 중 모든 클릭 차단
// ============================================================

public static class InputLock
{
    public static bool IsLocked { get; private set; } = false;

    public static void Lock()
    {
        IsLocked = true;
    }

    public static void Unlock()
    {
        IsLocked = false;
    }
}