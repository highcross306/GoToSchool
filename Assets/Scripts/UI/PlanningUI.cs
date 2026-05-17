// ============================================================
// PlanningUI.cs
// 역할: 선택 단계 전체 UI 관리
//       상태별 메시지 처리
// ============================================================

using UnityEngine;
using UnityEngine.UI;

public class PlanningUI : MonoBehaviour
{
    public static PlanningUI Instance { get; private set; }

    [Header("UI 연결")]
    public SelectionCardUI[] selectionCards;
    public Button decideButton;
    public GameObject selectionPanel;

    private SelectionCardUI selectedCard = null;
    private bool isDeciding = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        decideButton.onClick.AddListener(OnDecideButtonClicked);
        HideSelectionCards();
    }

    public void ShowSelectionCards(RouteData route)
    {
        HideSelectionCards();
        selectionPanel.SetActive(true);
        isDeciding = false;

        foreach (TransportType type in route.allowedTransports)
        {
            SelectionCardUI card = FindCardByType(type);
            if (card != null) card.Setup(type);
        }
    }

    public void HideSelectionCards()
    {
        selectionPanel.SetActive(false);
        selectedCard = null;
        foreach (SelectionCardUI card in selectionCards)
            card.Hide();
        SetDecideButtonActive(false);
    }

    public void OnCardSelected(SelectionCardUI clickedCard)
    {
        // 같은 카드 다시 클릭 → 선택 해제
        if (selectedCard == clickedCard)
        {
            MessageSystem.L("이동수단 선택 해제.");
            selectedCard.SetSelected(false);
            selectedCard = null;
            SetDecideButtonActive(false);
            return;
        }

        // 다른 카드 클릭 → 변경
        if (selectedCard != null)
        {
            MessageSystem.L("이동수단 변경.");
            selectedCard.SetSelected(false);
        }

        selectedCard = clickedCard;
        selectedCard.SetSelected(true);
        SetDecideButtonActive(true);
    }

    public void SetDecideButtonActive(bool active)
    {
        decideButton.interactable = active;
    }

    private void OnDecideButtonClicked()
    {
        if (isDeciding) return;

        // 이동 중
        if (GameState.CurrentPhase == Phase.Execution)
        {
            MessageSystem.E("이동 중에는 조작할 수 없습니다!");
            return;
        }

        // 노드 미선택 + 이동수단 미선택
        if (!PlanningManager.Instance.HasPendingRoute && selectedCard == null)
        {
            MessageSystem.E("아직 이동할 노드를 선택하지 않았습니다!");
            return;
        }

        // 노드 선택 + 이동수단 미선택
        if (PlanningManager.Instance.HasPendingRoute && selectedCard == null)
        {
            MessageSystem.E("먼저 이동수단을 선택해주세요!");
            return;
        }

        isDeciding = true;
        decideButton.interactable = false;
        InputLock.Lock();

        PlanningManager.Instance.OnTransportSelected(selectedCard.TransportType);
        PlanningManager.Instance.OnDecideButtonClicked();
    }

    private SelectionCardUI FindCardByType(TransportType type)
    {
        foreach (SelectionCardUI card in selectionCards)
        {
            if (card.name.Contains(type.ToString())) return card;
        }
        return null;
    }
}