// ============================================================
// DecideButtonPressHandler.cs
// 역할: DecideButton 오브젝트에 직접 부착
//       포인터 다운/업 감지 → PlanningUI.SetPressed() 호출
//       (PlanningUI가 다른 오브젝트에 있어도 동작하도록 분리)
// 부착: DecideButton 오브젝트에 직접 부착
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;

public class DecideButtonPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        PlanningUI.Instance?.SetPressed(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlanningUI.Instance?.SetPressed(false);
    }

    // 버튼 누른 채로 밖으로 드래그해서 나가는 경우도 원복
    public void OnPointerExit(PointerEventData eventData)
    {
        PlanningUI.Instance?.SetPressed(false);
    }
}