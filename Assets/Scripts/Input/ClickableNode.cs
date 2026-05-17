// ============================================================
// ClickableNode.cs
// 역할: 노드 클릭 감지 + 상태별 메시지 처리
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
        // 이동 중
        if (GameState.CurrentPhase == Phase.Execution)
        {
            MessageSystem.E("이동 중에는 조작할 수 없습니다!");
            return;
        }

        // 결과 팝업
        if (GameState.CurrentPhase == Phase.Result)
        {
            MessageSystem.E("결과 확인 후 진행해주세요!");
            return;
        }

        if (GameState.CurrentPhase != Phase.Planning) return;

        node.OnClicked();
    }
}