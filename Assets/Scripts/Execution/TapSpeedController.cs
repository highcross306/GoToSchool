// ============================================================
// TapSpeedController.cs
// 역할: 실행 단계에서 화면을 탭하면 이동 속도를 배속으로 전환
//       PC: 마우스 클릭 / 모바일: 터치 모두 지원
// 부착: 씬에 빈 오브젝트(예: [GameManager])에 함께 부착
// ============================================================

using UnityEngine;

public class TapSpeedController : MonoBehaviour
{
    private void Update()
    {
        // 실행 단계가 아니면 무시
        if (GameState.CurrentPhase != Phase.Execution) return;

        bool tapped = false;

        // PC 마우스 클릭
        if (Input.GetMouseButtonDown(0)) tapped = true;

        // 모바일 터치
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            tapped = true;

        if (tapped)
            PlayerMover.Instance.ToggleFastForward();
    }
}