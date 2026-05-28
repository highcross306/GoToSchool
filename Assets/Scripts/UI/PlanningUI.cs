// ============================================================
// PlanningUI.cs
// 역할: 선택 단계 전체 UI 관리
//       카드는 항상 표시 — 허용/비허용 여부로 상태만 변경
//       결정 버튼은 interactable로만 제어
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanningUI : MonoBehaviour
{
    public static PlanningUI Instance { get; private set; }

    [Header("UI 연결")]
    public SelectionCardUI[] selectionCards; // Card_Walk, Card_Bus, Card_Taxi
    public Button decideButton;

    private SelectionCardUI selectedCard = null;
    private HashSet<TransportType> currentAllowedTypes = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        decideButton.onClick.AddListener(OnDecideButtonClicked);
        SetDecideButtonActive(false);
        ResetAllCardsToDefault(); // 시작 시 모든 카드 기본 상태
    }

    // 노드 클릭 후 경로 확정 시 호출
    // → 허용된 이동수단 카드만 Default, 나머지는 Disabled
    public void ShowSelectionCards(RouteData route)
    {
        selectedCard = null;
        currentAllowedTypes = new HashSet<TransportType>(route.allowedTransports);
        SetDecideButtonActive(false);

        HashSet<TransportType> allowed = new HashSet<TransportType>(route.allowedTransports);

        foreach (SelectionCardUI card in selectionCards)
        {
            if (allowed.Contains(card.cardType))
                card.SetDefault();
            else
                card.SetDisabled();
        }
    }

    // 스테이지 재시작 등 전체 초기화 시 호출
    public void ResetAllCardsToDefault()
    {
        selectedCard = null;
        SetDecideButtonActive(false);
        foreach (SelectionCardUI card in selectionCards)
            card.SetDefault();
    }

    // 카드 클릭 시 호출
    public void OnCardSelected(SelectionCardUI clickedCard)
    {
        // 같은 카드 다시 클릭 → 선택 해제
        if (selectedCard == clickedCard)
        {
            MessageSystem.L("이동수단 선택 해제.");
            selectedCard = null;
            SetDecideButtonActive(false);

            // 허용된 카드들만 다시 Default로 복구
            RestoreAllowedCardsToDefault();
            return;
        }

        // 다른 카드 클릭
        if (selectedCard != null)
            MessageSystem.L("이동수단 변경.");

        selectedCard = clickedCard;
        UpdateCardStates();
        SetDecideButtonActive(true);
    }

    // 결정 버튼 활성/비활성
    public void SetDecideButtonActive(bool active)
    {
        decideButton.interactable = active;
    }

    private void OnDecideButtonClicked()
    {
        if (selectedCard == null) return;

        SetDecideButtonActive(false); // 결정 즉시 비활성화

        PlanningManager.Instance.OnTransportSelected(selectedCard.TransportType);
        PlanningManager.Instance.OnDecideButtonClicked();
    }

    // 선택 카드 → Selected / 나머지 → Disabled
    private void UpdateCardStates()
    {
        foreach (SelectionCardUI card in selectionCards)
        {
            if (card == selectedCard) card.SetSelected();
            else card.SetDisabled();
        }
    }

    // 선택 해제 시 허용된 카드들만 Default로 복구
    private void RestoreAllowedCardsToDefault()
    {
        foreach(SelectionCardUI card in selectionCards)
    {
            if (currentAllowedTypes.Contains(card.cardType))
                card.SetDefault();
            else
                card.SetDisabled();
        }
    }
}