// ============================================================
// SelectionCardUI.cs
// 역할: 이동수단 선택 카드 (항상 화면에 표시)
//       텍스트는 이미지에 포함 — 별도 설정 불필요
//       interactable은 항상 true로 유지하고,
//       선택 가능 여부는 IsSelectable 플래그로 판정
//       (비활성 버튼은 onClick이 발생하지 않아 에러 메시지를 띄울 수 없기 때문)
// ============================================================

using UnityEngine;
using UnityEngine.UI;

public class SelectionCardUI : MonoBehaviour
{
    [Header("카드 타입 (Inspector에서 설정)")]
    public TransportType cardType; // Walk / Bus / Taxi

    [Header("UI 연결")]
    public Button cardButton;
    public Image cardImage;

    [Header("카드 상태 스프라이트")]
    public Sprite defaultSprite;
    public Sprite selectedSprite;
    public Sprite disabledSprite;

    [Header("금지 오버레이 (자식 오브젝트)")]
    [Tooltip("노드에서 이 이동수단을 금지했을 때만 표시되는 자식 오브젝트. " +
             "위치/크기는 Awake에서 카드 전체를 덮도록 코드로 강제 고정된다.")]
    public GameObject blockedOverlay;

    public TransportType TransportType => cardType;

    // 현재 이 카드를 선택할 수 있는지 (Disabled 상태면 false)
    public bool IsSelectable { get; private set; } = false;

    // 노드에 의해 아예 선택이 금지된 상태인지 (일반 Disabled와 구분)
    public bool IsBlockedByNode { get; private set; } = false;

    private void Awake()
    {
        cardButton.onClick.AddListener(OnCardClicked);

        if (blockedOverlay != null)
        {
            // 항상 카드 이미지 위(자식 순서상 마지막)에 렌더링되도록 고정
            blockedOverlay.transform.SetAsLastSibling();

            // 위치를 Unity에서 수동으로 맞출 필요 없이, 카드 전체와 정확히
            // 겹치도록 anchor/offset을 강제로 꽉 채움(stretch)으로 고정한다.
            // 카드 크기가 나중에 바뀌어도 항상 카드 전체를 그대로 덮는다.
            RectTransform overlayRect = blockedOverlay.GetComponent<RectTransform>();
            if (overlayRect != null)
            {
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
                overlayRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                Debug.LogWarning($"[SelectionCardUI] '{blockedOverlay.name}'에 " +
                                 "RectTransform이 없어 카드 위치에 맞출 수 없습니다.", this);
            }

            // 카드(200x300)는 정사각형이 아니므로, 위 stretch 설정만 두면
            // 금지 스프라이트가 카드 비율에 맞춰 늘어나 찌그러진다.
            // Preserve Aspect를 켜면 채워지는 영역(카드 전체) 안에서
            // 스프라이트의 원본 비율(1:1)을 유지한 채 중앙에 맞춰 그려진다.
            Image overlayImage = blockedOverlay.GetComponent<Image>();
            if (overlayImage != null)
                overlayImage.preserveAspect = true;

            blockedOverlay.SetActive(false);
        }

        SetDisabled(); // 노드 선택 전에는 비활성 상태로 시작
    }

    // 기본 상태 — 선택 가능
    public void SetDefault()
    {
        cardImage.sprite = defaultSprite;
        IsSelectable = true;
        IsBlockedByNode = false;
        transform.localScale = Vector3.one; // 선택 해제 시 크기 원복
        if (blockedOverlay != null) blockedOverlay.SetActive(false);
    }

    // 선택 상태
    public void SetSelected()
    {
        cardImage.sprite = selectedSprite;
        IsSelectable = true;
        IsBlockedByNode = false;
        transform.localScale = Vector3.one * 1.2f;
        if (blockedOverlay != null) blockedOverlay.SetActive(false);
    }

    // 비활성 상태 — 선택 불가 (단, 클릭 이벤트는 받아서 메시지 표시)
    // 일반 비활성 (경로에서 허용 안 됨 / 아직 노드 선택 전 등)
    public void SetDisabled()
    {
        cardImage.sprite = disabledSprite;
        IsSelectable = false;
        IsBlockedByNode = false;
        transform.localScale = Vector3.one;
        if (blockedOverlay != null) blockedOverlay.SetActive(false);
    }

    // 노드에 의해 금지된 상태 (금지 오버레이 표시)
    public void SetBlockedByNode()
    {
        cardImage.sprite = disabledSprite;
        IsSelectable = false;
        IsBlockedByNode = true;
        transform.localScale = Vector3.one;
        if (blockedOverlay != null) blockedOverlay.SetActive(true);
    }

    // 선택 해제 시 크기만 원복 (스프라이트는 호출부가 결정)
    public void ResetScale()
    {
        transform.localScale = Vector3.one;
    }

    private void OnCardClicked()
    {
        // 이벤트 메시지 표시 중 → 조작 불가
        if (EventMessagePanel.IsOpen) return;

        // 스토리 인트로 표시 중 → 조작 불가
        if (StoryIntroPanel.IsOpen) return;

        // 일시정지 창(ESC) 표시 중 → 조작 불가
        if (PauseMenu.IsOpen) return;

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

        // 노드에 의해 금지된 이동수단
        if (IsBlockedByNode)
        {
            MessageSystem.E("현재 위치에서는 사용할 수 없는 이동수단입니다!");
            return;
        }

        // 노드 미선택 상태
        if (!PlanningManager.Instance.HasPendingRoute)
        {
            MessageSystem.E("먼저 이동할 노드를 선택해주세요!");
            return;
        }

        // 여기까지 통과했으면 유효한 카드 클릭 → 이벤트 정보 팝업 먼저 띄운다
        // (실제 발동/적용과 무관한 미리보기. 아래 선택 로직은 그대로 진행됨)
        if (EventInfoPopup.Instance != null && EventManager.Instance != null &&
            EventManager.Instance.transportEventTable != null)
        {
            GameEvent[] events = EventManager.Instance.transportEventTable.GetEvents(cardType);
            EventInfoPopup.Instance.ShowForCard(cardType, events);
        }

        PlanningUI.Instance.OnCardSelected(this);
    }
}