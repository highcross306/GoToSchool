// ============================================================
// FastForwardButton.cs
// 역할: 가속 기능을 화면 탭 대신 버튼 하나로 제공.
//
//       스프라이트는 "가속이 켜져 있는지(on/off)"를 그대로 보여준다.
//         - 꺼짐(disabledSprite): 기본 상태. 새 이동이 시작될 때마다 항상 이 상태.
//         - 켜짐(enabledSprite) : 버튼을 한 번 클릭하면 전환.
//       한 번 더 클릭하면 다시 꺼짐으로 돌아가는 단순 토글이다.
//
//       클릭은 "이동 중일 때만" 유효하다.
//       이동 중이 아닐 때 클릭하면 상태는 바뀌지 않고 에러 메시지만 뜬다.
//
//       주의: Button의 interactable은 계속 true로 둔다.
//       (클릭이 항상 들어와야 "정지 중 클릭 → 에러 메시지"가 동작한다.)
//
// 부착: 가속 버튼 UI 오브젝트 (Image + Button 컴포넌트가 있는 곳)
// 연결: Button의 OnClick() 이벤트에 OnClickFastForward()를 등록
// ============================================================

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FastForwardButton : MonoBehaviour
{
    [Header("상태별 스프라이트")]
    public Sprite enabledSprite;  // 가속 켜짐(ON) 모습
    public Sprite disabledSprite; // 가속 꺼짐(OFF) 모습 — 기본값

    private Image buttonImage;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();

        if (enabledSprite == null || disabledSprite == null)
            Debug.LogWarning("[FastForwardButton] enabledSprite / disabledSprite가 " +
                             "Inspector에 연결되지 않았습니다.", this);
    }

    private void Update()
    {
        if (PlayerMover.Instance == null || buttonImage == null) return;

        // 스프라이트는 가속 on/off 상태를 그대로 반영한다.
        Sprite target = PlayerMover.Instance.IsFastForward ? enabledSprite : disabledSprite;
        if (target != null && buttonImage.sprite != target)
            buttonImage.sprite = target;
    }

    // Button의 OnClick()에 연결
    public void OnClickFastForward()
    {
        if (PlayerMover.Instance == null)
        {
            Debug.LogWarning("[FastForwardButton] PlayerMover.Instance가 null입니다.");
            return;
        }

        // 이동 중이 아니면 상태를 바꾸지 않고 에러만 표시한다.
        if (!PlayerMover.Instance.IsMoving)
        {
            MessageSystem.E("이동 중에만 가속 버튼을 사용할 수 있습니다.");
            return;
        }

        // 이동 중이면 순수 토글: 켜짐 ↔ 꺼짐
        PlayerMover.Instance.ToggleFastForward();
    }
}