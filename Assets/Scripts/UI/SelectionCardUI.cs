// ============================================================
// SelectionCardUI.cs
// 역할: 이동수단 선택 카드 (항상 화면에 표시)
//       카드 타입은 Inspector에서 미리 설정
//       텍스트는 이미지에 포함되어 있어 별도 설정 불필요
//       상태 변화만 스프라이트 교체로 처리
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

    private void Awake()
    {
        cardButton.onClick.AddListener(OnCardClicked);
        SetDefault();
    }

    // 기본 상태
    public void SetDefault()
    {
        cardImage.sprite = defaultSprite;
        cardButton.interactable = true;
        transform.localScale = Vector3.one;
    }

    // 선택 상태 — 크기 강조
    // selectedSprite 캔버스가 defaultSprite보다 커서 (256x403 vs 220x360)
    // 캔버스 비율 차이(~1.14)만큼 보정 + 강조 효과(1.05) = 약 1.2배
    public void SetSelected()
    {
        cardImage.sprite = selectedSprite;
        cardButton.interactable = true;
        transform.localScale = Vector3.one * 1.2f;
    }

    // 비활성화 상태
    public void SetDisabled()
    {
        cardImage.sprite = disabledSprite;
        cardButton.interactable = false;
        transform.localScale = Vector3.one;
    }

    private void OnCardClicked()
    {
        if (GameState.CurrentPhase == Phase.Execution)
        {
            MessageSystem.E("이동 중에는 조작할 수 없습니다!");
            return;
        }
        if (GameState.CurrentPhase == Phase.Result)
        {
            MessageSystem.E("결과 확인 후 진행해주세요!");
            return;
        }
        if (!PlanningManager.Instance.HasPendingRoute)
        {
            MessageSystem.E("먼저 이동할 노드를 선택해주세요!");
            return;
        }

        PlanningUI.Instance.OnCardSelected(this);
    }
}