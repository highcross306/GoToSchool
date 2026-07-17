// ============================================================
// PlanningUI.cs
// 역할: 선택 단계 UI 관리
//       결정 버튼도 interactable을 끄지 않고 스프라이트로만 비활성 표현
//       (비활성 버튼은 onClick이 발생하지 않아 에러 메시지를 띄울 수 없기 때문)
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
    public Image decideButtonImage; // 결정 버튼 Image 컴포넌트

    [Header("결정 버튼 스프라이트")]
    public Sprite decideNormalSprite;   // 활성 상태
    public Sprite decideDisabledSprite; // 비활성 상태
    public Sprite decidePressedSprite;  // 눌린 상태

    private bool isPressed = false; // 현재 버튼을 누르고 있는 중인지

    private SelectionCardUI selectedCard = null;
    private bool isConfirmed = false; // 결정 후 재클릭 차단
    private HashSet<SelectionCardUI> allowedCards = new(); // 현재 선택 가능한 카드

    // 결정 버튼이 눌릴 수 있는 상태인지
    private bool IsDecideReady => selectedCard != null && !isConfirmed;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        decideButton.onClick.AddListener(OnDecideButtonClicked);
        ResetAllCardsToDefault();
    }

    // 노드 클릭 후 허용된 이동수단만 활성화
    // fromNode: 현재 캐릭터가 서 있는 노드 (이동수단 제한 확인용)
    public void ShowSelectionCards(RouteData route, NodeData fromNode)
    {
        selectedCard = null;
        isConfirmed = false;
        allowedCards.Clear();

        HashSet<TransportType> allowed = new(route.allowedTransports);
        foreach (SelectionCardUI card in selectionCards)
        {
            bool routeAllows = allowed.Contains(card.cardType);
            bool nodeBlocks = fromNode != null && fromNode.IsTransportDisabled(card.cardType);

            if (routeAllows && !nodeBlocks)
            {
                card.SetDefault();
                allowedCards.Add(card);
            }
            else if (nodeBlocks)
            {
                // 노드가 금지한 이동수단 → 금지 오버레이 표시
                card.SetBlockedByNode();
            }
            else
            {
                card.SetDisabled();
            }
        }
        RefreshDecideButtonVisual();
    }

    // 카드 클릭 시 호출
    public void OnCardSelected(SelectionCardUI clickedCard)
    {
        // 결정 완료 후 → 다음 노드 선택 전까지 차단
        if (isConfirmed)
        {
            MessageSystem.E("이미 이동수단을 결정했습니다!");
            return;
        }

        // 이 경로에서 허용되지 않는 이동수단
        if (!allowedCards.Contains(clickedCard))
        {
            MessageSystem.E("이 경로에서는 사용할 수 없는 이동수단입니다!");
            return;
        }

        // 같은 카드 재클릭 → 선택 해제
        if (selectedCard == clickedCard)
        {
            MessageSystem.L("이동수단 선택 해제.");
            selectedCard = null;
            RestoreAllowedCardsToDefault();
            RefreshDecideButtonVisual();

            // 카드 선택을 취소했으므로 이벤트 미리보기 팝업도 닫는다.
            if (EventInfoPopup.Instance != null)
                EventInfoPopup.Instance.Hide();

            return;
        }

        if (selectedCard != null)
            MessageSystem.L("이동수단 변경.");

        // 새로 선택하거나 다른 카드로 변경할 때만 카드별 효과음 재생
        PlayCardSelectSfx(clickedCard.TransportType);

        selectedCard = clickedCard;
        UpdateCardStates();
        RefreshDecideButtonVisual();
    }

    // 카드 종류별 선택 효과음
    private void PlayCardSelectSfx(TransportType type)
    {
        if (SoundManager.Instance == null) return;

        switch (type)
        {
            case TransportType.Walk:
                SoundManager.Instance.Play(SoundManager.Sfx.Walk);
                break;
            case TransportType.Bus:
                SoundManager.Instance.Play(SoundManager.Sfx.Bus);
                break;
            case TransportType.Taxi:
                SoundManager.Instance.Play(SoundManager.Sfx.Taxi);
                break;
        }
    }

    // 결정 버튼 클릭 시
    private void OnDecideButtonClicked()
    {
        // 이벤트 메시지 표시 중 → 조작 불가
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

        // 이미 결정 완료 (다음 노드 선택 전)
        if (isConfirmed)
        {
            MessageSystem.E("이미 이동수단을 결정했습니다!");
            return;
        }

        // 노드 미선택
        if (!PlanningManager.Instance.HasPendingRoute)
        {
            MessageSystem.E("아직 이동할 노드를 선택하지 않았습니다!");
            return;
        }

        // 노드는 선택했으나 이동수단 미선택
        if (selectedCard == null)
        {
            MessageSystem.E("먼저 이동수단을 선택해주세요!");
            return;
        }

        // 정상 확정
        isConfirmed = true;
        TransportType confirmed = selectedCard.TransportType;
        selectedCard = null;
        DisableAllCards();
        RefreshDecideButtonVisual();

        // 결정 버튼을 눌렀으므로 이벤트 미리보기 팝업을 닫는다.
        if (EventInfoPopup.Instance != null)
            EventInfoPopup.Instance.Hide();

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundManager.Sfx.Confirm);

        PlanningManager.Instance.OnTransportSelected(confirmed);
        PlanningManager.Instance.OnDecideButtonClicked();
    }

    // 이동 후 다음 노드 선택을 위해 초기화
    public void ResetForNextSelection()
    {
        selectedCard = null;
        isConfirmed = false;
        allowedCards.Clear();
        DisableAllCards();
        RefreshDecideButtonVisual();
    }

    // 스테이지 시작/재시작 시 전체 초기화
    public void ResetAllCardsToDefault()
    {
        selectedCard = null;
        isConfirmed = false;
        allowedCards.Clear();
        DisableAllCards();
        RefreshDecideButtonVisual();
    }

    // 모든 카드 비활성화 (노드 선택 전 상태)
    private void DisableAllCards()
    {
        foreach (SelectionCardUI card in selectionCards)
            card.SetDisabled();
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

    // 선택 해제 시 허용된 카드만 Default로 복원
    private void RestoreAllowedCardsToDefault()
    {
        foreach (SelectionCardUI card in selectionCards)
        {
            if (allowedCards.Contains(card)) card.SetDefault();
            else card.SetDisabled();
        }
    }

    // 결정 버튼 스프라이트 갱신 (interactable은 항상 true 유지)
    private void RefreshDecideButtonVisual()
    {
        if (decideButtonImage == null)
        {
            Debug.LogWarning("[PlanningUI] decideButtonImage가 연결되지 않았습니다. " +
                              "Inspector에서 Decide Button Image 슬롯을 연결하세요.");
            return;
        }

        // 비활성 상태가 최우선, 그 다음 눌림 상태, 마지막 기본 상태
        Sprite target;
        if (!IsDecideReady)
            target = decideDisabledSprite;
        else if (isPressed)
            target = decidePressedSprite != null ? decidePressedSprite : decideNormalSprite;
        else
            target = decideNormalSprite;

        if (target == null)
        {
            Debug.LogWarning("[PlanningUI] 결정 버튼 스프라이트가 연결되지 않았습니다.");
            return;
        }

        decideButtonImage.sprite = target;
    }

    // DecideButtonPressHandler가 호출 — 버튼을 누르는 순간
    public void SetPressed(bool pressed)
    {
        if (pressed && !IsDecideReady) return; // 선택 불가 상태면 Pressed 무시
        isPressed = pressed;
        RefreshDecideButtonVisual();
    }
}