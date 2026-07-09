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

    public TransportType TransportType => cardType;

    // 현재 이 카드를 선택할 수 있는지 (Disabled 상태면 false)
    public bool IsSelectable { get; private set; } = false;

    private void Awake()
    {
        cardButton.onClick.AddListener(OnCardClicked);
        SetDisabled(); // 노드 선택 전에는 비활성 상태로 시작
    }

    // 기본 상태 — 선택 가능
    public void SetDefault()
    {
        cardImage.sprite = defaultSprite;
        IsSelectable = true;
    }

    // 선택 상태
    public void SetSelected()
    {
        cardImage.sprite = selectedSprite;
        IsSelectable = true;
        transform.localScale = Vector3.one * 1.2f;
    }

    // 비활성 상태 — 선택 불가 (단, 클릭 이벤트는 받아서 메시지 표시)
    public void SetDisabled()
    {
        cardImage.sprite = disabledSprite;
        IsSelectable = false;
        transform.localScale = Vector3.one;
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

        // 노드 미선택 상태
        if (!PlanningManager.Instance.HasPendingRoute)
        {
            MessageSystem.E("먼저 이동할 노드를 선택해주세요!");
            return;
        }

        PlanningUI.Instance.OnCardSelected(this);
    }
}