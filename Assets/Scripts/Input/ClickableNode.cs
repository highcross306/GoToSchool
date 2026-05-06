// ============================================================
// ClickableNode.cs
// 역할: 노드 오브젝트의 클릭/터치 감지 전담
//       감지한 이벤트를 Node.cs에 전달
//       Node 프리팹에 함께 부착
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
        // Planning 단계일 때만 클릭 처리
        if (GameState.CurrentPhase != Phase.Planning) return;

        //입력 잠금 상태이면 무반을
        if (InputLock.IsLocked) return;

        node.OnClicked();
    }
}