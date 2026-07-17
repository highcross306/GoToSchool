// ============================================================
// ClickableNode.cs
// 역할: 노드 클릭 감지 + 단계별 에러 메시지
// ============================================================

using UnityEngine;

public class ClickableNode : MonoBehaviour
{
    private Node node;

    private void Awake()
    {
        node = GetComponent<Node>();
    }

    private void OnMouseDown()
    {
        // 이벤트 메시지 표시 중 → 조작 불가 (메시지 패널이 클릭을 처리)
        if (EventMessagePanel.IsOpen) return;

        // 스토리 인트로 표시 중 → 조작 불가
        if (StoryIntroPanel.IsOpen) return;

        // 이동 중
        if (GameState.CurrentPhase == Phase.Execution)
        {
            MessageSystem.E("이동 중에는 조작할 수 없습니다!");
            return;
        }

        // 결과 팝업 중
        if (GameState.CurrentPhase == Phase.Result)
        {
            MessageSystem.E("결과 확인 후 진행해주세요!");
            return;
        }

        if (GameState.CurrentPhase != Phase.Planning) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundManager.Sfx.Click);

        node.OnClicked();
    }
}