// ============================================================
// PlayerAnimationController.cs
// 역할: 이동수단(Walk/Bus/Taxi)에 따라 캐릭터 애니메이션 전환
//       Unity Animator Controller + 스프라이트 애니메이션 사용
// 부착: PlayerMover와 "같은" 오브젝트에 부착 (Animator 컴포넌트 필요)
//
// [수정] Awake에서 Animator / Controller 누락을 즉시 에러로 알린다.
//        레거시 Animation 컴포넌트와 혼동하는 사고를 잡기 위함.
// ============================================================

using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public static PlayerAnimationController Instance { get; private set; }

    [Header("Animator 연결 (비워두면 자동으로 찾음)")]
    public Animator animator;

    [Header("Animator Parameter 이름 (Animator 창에서 동일하게 설정)")]
    public string isMovingParam = "IsMoving";          // Bool
    public string transportIntParam = "TransportType"; // Int (0=Walk, 1=Bus, 2=Taxi)

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (animator == null)
            animator = GetComponent<Animator>();

        // ---- 진단 ----
        if (animator == null)
        {
            Debug.LogError(
                $"[Anim] '{gameObject.name}'에 Animator 컴포넌트가 없습니다!\n" +
                "레거시 'Animation'(뒤에 r 없음)과 혼동하지 않았는지 확인하세요. " +
                "필요한 것은 'Animator'입니다.",
                gameObject);
        }
        else if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError(
                $"[Anim] '{gameObject.name}'의 Animator에 Controller가 비어 있습니다!\n" +
                "Assets/Animations/Player.controller 를 할당하세요.",
                gameObject);
        }
        else
        {
            Debug.Log($"[Anim] 준비 완료 — Controller: {animator.runtimeAnimatorController.name}",
                      gameObject);
        }
    }

    // PlayerMover가 이동 시작 시 호출
    public void PlayMoveAnimation(TransportType transport)
    {
        if (animator == null) return;

        animator.SetInteger(transportIntParam, (int)transport);
        animator.SetBool(isMovingParam, true);
    }

    // PlayerMover가 이동 완료 시 호출
    public void PlayIdleAnimation()
    {
        if (animator == null) return;
        animator.SetBool(isMovingParam, false);
    }
}