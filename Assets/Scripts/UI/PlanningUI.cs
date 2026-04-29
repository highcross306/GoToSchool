// ============================================================
// PlanningUI.cs
// 역할: 선택 단계 전체 UI 컨트롤러
//       선택지 카드 표시/숨김
//       결정 버튼 활성/비활성 처리
// 부착: Canvas 하위 SelectionPanel 오브젝트에 부착
// ============================================================

using UnityEngine;
using UnityEngine.UI;

public class PlanningUI : MonoBehaviour
{
    public static PlanningUI Instance { get; private set; }

    [Header("UI 연결")]
    public SelectionCardUI[] selectionCards; // 이동수단 카드 배열 (Walk/Bus/Taxi)
    public Button decideButton;   // 결정 버튼
    public GameObject selectionPanel; // 카드 패널 전체

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        decideButton.onClick.AddListener(OnDecideButtonClicked);
        SetDecideButtonActive(false);
        HideSelectionCards();
    }

    // PlanningManager가 노드 클릭 → 경로 확정 후 호출
    // 해당 경로의 이동수단 카드를 화면에 표시
    public void ShowSelectionCards(RouteData route)
    {
        // 모든 카드 숨기기
        HideSelectionCards();

        selectionPanel.SetActive(true);

        // 경로에 있는 이동수단만 카드로 표시
        foreach (TransportCost cost in route.transportCosts)
        {
            SelectionCardUI card = FindCardByType(cost.transportType);
            if (card != null) card.Setup(cost);
        }
    }

    // 카드 패널 전체 숨기기
    public void HideSelectionCards()
    {
        selectionPanel.SetActive(false);
        foreach (SelectionCardUI card in selectionCards)
            card.Hide();
    }

    // 결정 버튼 활성/비활성
    public void SetDecideButtonActive(bool active)
    {
        decideButton.interactable = active;
    }

    private void OnDecideButtonClicked()
    {
        PlanningManager.Instance.OnDecideButtonClicked();
    }

    private SelectionCardUI FindCardByType(TransportType type)
    {
        foreach (SelectionCardUI card in selectionCards)
        {
            // 카드 이름으로 타입 매칭 (Walk/Bus/Taxi)
            if (card.name.Contains(type.ToString())) return card;
        }
        return null;
    }
}