// ============================================================
// PlanningUI.cs
// 역할: 선택 단계 전체 UI 컨트롤러
//       RouteData의 allowedTransports 기준으로 카드 표시
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

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        decideButton.onClick.AddListener(OnDecideButtonClicked);
        SetDecideButtonActive(false);
        HideSelectionCards();
    }

    // 경로의 allowedTransports 기준으로 카드 표시
    public void ShowSelectionCards(RouteData route)
    {
        HideSelectionCards();
        selectionPanel.SetActive(true);

        foreach (TransportType type in route.allowedTransports)
        {
            SelectionCardUI card = FindCardByType(type);
            if (card != null) card.Setup(type);
        }
    }

    public void HideSelectionCards()
    {
        selectionPanel.SetActive(false);
        foreach (SelectionCardUI card in selectionCards)
            card.Hide();
    }

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
            if (card.name.Contains(type.ToString())) return card;
        }
        return null;
    }
}