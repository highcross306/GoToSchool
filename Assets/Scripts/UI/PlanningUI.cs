// ============================================================
// PlanningUI.cs
// 역할: 선택 단계 UI 관리
//       매 노드 이동 후 다음 선택을 위해 초기화
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanningUI : MonoBehaviour
{
    public static PlanningUI Instance { get; private set; }

    [Header("UI 연결")]
    public SelectionCardUI[] selectionCards;
    public Button decideButton;

    private SelectionCardUI selectedCard = null;
    private bool isConfirmed = false;
    private bool isLocked = false;
    private HashSet<SelectionCardUI> allowedCards = new(); // 현재 선택 가능한 카드 목록

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        decideButton.onClick.AddListener(OnDecideButtonClicked);
        SetDecideButtonActive(false);
        DisableAllCards(); // 노드 선택 전 카드 비활성화
    }

    // 노드 클릭 후 허용된 이동수단 카드 상태 업데이트
    public void ShowSelectionCards(RouteData route)
    {
        selectedCard = null;
        isConfirmed = false;
        isLocked = false;
        SetDecideButtonActive(false);

        allowedCards.Clear();
        HashSet<TransportType> allowed = new HashSet<TransportType>(route.allowedTransports);
        foreach (SelectionCardUI card in selectionCards)
        {
            if (allowed.Contains(card.cardType))
            {
                card.SetDefault();
                allowedCards.Add(card); // 허용된 카드 목록 저장
            }
            else
            {
                card.SetDisabled();
            }
        }
    }

    // 카드 클릭 시 호출
    public void OnCardSelected(SelectionCardUI clickedCard)
    {
        if (isLocked) return;

        if (selectedCard == clickedCard)
        {
            MessageSystem.L("이동수단 선택 해제.");
            selectedCard = null;
            SetDecideButtonActive(false);
            RestoreAllowedCardsToDefault();
            return;
        }

        if (selectedCard != null)
            MessageSystem.L("이동수단 변경.");

        selectedCard = clickedCard;
        UpdateCardStates();
        SetDecideButtonActive(true);
    }

    public void SetDecideButtonActive(bool active)
    {
        decideButton.interactable = active;
    }

    // 이동 후 다음 노드 선택을 위해 초기화
    public void ResetForNextSelection()
    {
        selectedCard = null;
        isConfirmed = false;
        isLocked = false;
        SetDecideButtonActive(false);
        DisableAllCards(); // 다음 노드 선택 전까지 카드 비활성화
    }

    // 스테이지 시작/재시작 시 전체 초기화
    public void ResetAllCardsToDefault()
    {
        selectedCard = null;
        isConfirmed = false;
        isLocked = false;
        allowedCards.Clear();
        SetDecideButtonActive(false);
        DisableAllCards();
    }

    // 모든 카드 비활성화 (노드 선택 전 상태)
    private void DisableAllCards()
    {
        foreach (SelectionCardUI card in selectionCards)
            card.SetDisabled();
    }

    private void OnDecideButtonClicked()
    {
        if (isConfirmed) return;
        if (selectedCard == null) return;

        isConfirmed = true;
        isLocked = true;

        TransportType confirmedTransport = selectedCard.TransportType;
        selectedCard = null;
        SetDecideButtonActive(false);

        PlanningManager.Instance.OnTransportSelected(confirmedTransport);
        PlanningManager.Instance.OnDecideButtonClicked();
    }

    private void UpdateCardStates()
    {
        foreach (SelectionCardUI card in selectionCards)
        {
            if (!card.gameObject.activeSelf) continue;
            if (card == selectedCard) card.SetSelected();
            else card.SetDisabled();
        }
    }

    private void RestoreAllowedCardsToDefault()
    {
        // allowedCards에 저장된 카드만 Default로 복원
        foreach (SelectionCardUI card in allowedCards)
            card.SetDefault();
    }
}