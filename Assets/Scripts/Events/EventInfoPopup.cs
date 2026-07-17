// ============================================================
// EventInfoPopup.cs
// 역할: 강화 노드 또는 이동수단 카드를 클릭했을 때,
//       그곳에서 "발생할 수 있는" 이벤트 목록을 미리 보여주는 팝업.
//       ★ 실제로 확률을 굴리거나 효과를 적용하지 않는다. ★
//       (실제 발동/효과 적용은 EventManager.TryTriggerEventForRoute가 담당)
//
//       평소에는 숨겨져 있다가 노드/카드를 클릭했을 때만 표시된다.
//       사용자가 직접 닫는 버튼은 없다 — Show() 호출로만 나타난다.
//       [수정] 등록된 이벤트가 하나도 없으면 팝업 자체를 띄우지 않는다.
//              (이전엔 "등록된 이벤트가 없습니다"라는 빈 팝업이 떴음)
//
//       문구 형식은 "스테이지_4_UI_흐름.pdf"의 예시를 그대로 따른다:
//         [버스 정류장]
//         이곳에서 이동수단 버스로 결정 시
//         30% 확률로 [지나간 버스] 또는
//         30% 확률로 [난폭 운전] 이벤트 발생
//
//       GameEvent.eventName이 비어있으면 "이벤트 N (미설정)"으로 대체 표시.
//       → 나중에 GameEvent 에셋의 eventName만 채우면 그대로 반영된다.
//
// 부착: Canvas 하위 EventInfoPopup 오브젝트에 부착
// ============================================================

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class EventInfoPopup : MonoBehaviour
{
    public static EventInfoPopup Instance { get; private set; }

    [Header("UI 연결")]
    public GameObject panel;            // 팝업 전체
    public TextMeshProUGUI titleText;   // [노드명] / [이동수단] — 어디서 열린 팝업인지
    public TextMeshProUGUI bodyText;    // 이동수단 제한 안내 + 확률 목록

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (panel != null)
            panel.SetActive(false);
    }

    // 강화 노드 클릭 시 호출
    public void ShowForNode(NodeData node)
    {
        if (node == null) return;

        // 등록된 이벤트가 하나도 없으면 팝업을 띄우지 않는다.
        if (!HasAnyEvent(node.enhancedEvents)) return;

        string title = $"[{node.nodeType.ToKorean()}]";
        string body = BuildNodeBody(node);
        Show(title, body);
    }

    // 이동수단 카드 클릭 시 호출
    public void ShowForCard(TransportType transport, GameEvent[] events)
    {
        // 등록된 이벤트가 하나도 없으면 팝업을 띄우지 않는다.
        if (!HasAnyEvent(events)) return;

        string title = $"[{transport.ToKorean()}]";
        string body = BuildEventListBody(events);
        Show(title, body);
    }

    // 배열이 null이거나, null이 아닌 이벤트가 하나도 없으면 false
    private bool HasAnyEvent(GameEvent[] events)
    {
        if (events == null) return false;
        foreach (GameEvent e in events)
            if (e != null) return true;
        return false;
    }

    private void Show(string title, string body)
    {
        if (panel == null) return;

        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;

        panel.SetActive(true);
    }

    // ---- 노드용 본문: 이동수단 제한 안내 + 이동수단별 이벤트 확률 목록 ----
    private string BuildNodeBody(NodeData node)
    {
        StringBuilder sb = new StringBuilder();

        // "이곳에서 이동수단 택시 선택 불가" 같은 제한 안내를 먼저 표시
        if (node.disabledTransports != null)
        {
            foreach (TransportType t in node.disabledTransports)
                sb.AppendLine($"이곳에서 이동수단 {t.ToKorean()} 선택 불가");
        }

        // requiredTransport 별로 묶어서 "이동수단 X로 결정 시" 블록을 나눈다.
        // restrictToTransport == false인 이벤트는 이동수단 무관 그룹으로 묶는다.
        Dictionary<TransportType?, List<GameEvent>> groups = new Dictionary<TransportType?, List<GameEvent>>();
        foreach (GameEvent e in node.enhancedEvents)
        {
            if (e == null) continue;
            TransportType? key = e.restrictToTransport ? e.requiredTransport : (TransportType?)null;
            if (!groups.TryGetValue(key, out List<GameEvent> list))
            {
                list = new List<GameEvent>();
                groups[key] = list;
            }
            list.Add(e);
        }

        foreach (KeyValuePair<TransportType?, List<GameEvent>> kv in groups)
        {
            sb.AppendLine(kv.Key.HasValue
                ? $"이곳에서 이동수단 {kv.Key.Value.ToKorean()}로 결정 시"
                : "이동수단과 관계없이");

            AppendProbabilityLines(sb, kv.Value);
        }

        return sb.ToString().TrimEnd();
    }

    // ---- 카드용 본문: 해당 이동수단의 이벤트 확률 목록만 ----
    private string BuildEventListBody(GameEvent[] events)
    {
        StringBuilder sb = new StringBuilder();

        List<GameEvent> list = new List<GameEvent>();
        foreach (GameEvent e in events)
            if (e != null) list.Add(e);

        AppendProbabilityLines(sb, list);
        return sb.ToString().TrimEnd();
    }

    // "30% 확률로 [지나간 버스] 또는" / 마지막 줄만 "... 이벤트 발생"으로 마무리
    private void AppendProbabilityLines(StringBuilder sb, List<GameEvent> events)
    {
        for (int i = 0; i < events.Count; i++)
        {
            GameEvent e = events[i];
            string name = string.IsNullOrEmpty(e.eventName)
                ? $"이벤트 {i + 1} (미설정)"
                : e.eventName;

            int percent = Mathf.RoundToInt(e.probability * 100f);
            bool isLast = i == events.Count - 1;
            string suffix = isLast ? " 이벤트 발생" : " 또는";

            sb.AppendLine($"{percent}% 확률로 [{name}]{suffix}");
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}