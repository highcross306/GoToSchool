// ============================================================
// PlayerMover.cs
// 역할: 플레이어 오브젝트를 노드에서 노드로 이동
//       코루틴 기반 이동 애니메이션
//       이동 완료 시 ExecutionManager에 콜백
//       speedMultiplier로 화면 탭 시 배속 이동 지원
//       transport 파라미터로 이동수단별 애니메이션 재생
//
// [수정] PlayerAnimationController를 static Instance가 아니라
//        GetComponent로 같은 오브젝트에서 찾도록 변경.
//        → PlayerMover와 PlayerAnimationController가 서로 다른
//          오브젝트에 붙는 사고를 원천 차단하고, 어긋나면 Awake에서
//          즉시 에러를 띄운다.
// ============================================================

using System;
using System.Collections;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    public static PlayerMover Instance { get; private set; }

    [Header("이동 설정")]
    public float moveSpeed = 3f; // 기본 이동 속도 (유닛/초)

    [Header("배속 설정")]
    public float fastForwardMultiplier = 2f; // 화면 탭 시 배속 배율

    public bool IsFastForward { get; private set; } = false;

    // 같은 오브젝트에 붙어 있는 애니메이션 컨트롤러 (Awake에서 캐싱)
    private PlayerAnimationController anim;

    private void Awake()
    {
        // ---- 진단 1: 씬에 PlayerMover가 여러 개인가? ----
        // 아래 싱글톤 가드는 GameObject 자체를 파괴하기 때문에,
        // 중복이 있으면 제대로 세팅한 Player가 통째로 사라질 수 있다.
        PlayerMover[] all = FindObjectsOfType<PlayerMover>();
        if (all.Length > 1)
        {
            string names = "";
            foreach (PlayerMover m in all) names += m.gameObject.name + " / ";
            Debug.LogError($"[Mover] 씬에 PlayerMover가 {all.Length}개 있습니다: {names}" +
                           "하나만 남기고 나머지는 삭제하세요.", gameObject);
        }

        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // ---- 진단 2: 같은 오브젝트에 애니메이션 컨트롤러가 있는가? ----
        anim = GetComponent<PlayerAnimationController>();
        if (anim == null)
        {
            Debug.LogError(
                $"[Mover] ★이 오브젝트가 진짜 플레이어입니다 → '{gameObject.name}'★\n" +
                "여기에 PlayerAnimationController와 Animator를 붙이세요.\n" +
                "(이 로그를 클릭하면 Hierarchy에서 해당 오브젝트가 선택됩니다)",
                gameObject);
        }
        else
        {
            Debug.Log($"[Mover] 애니메이션 연결 OK — '{gameObject.name}'", gameObject);
        }
    }

    // ExecutionManager가 호출 — 목표 노드로 이동 시작
    // transport: 이동 애니메이션 재생을 위한 이동수단 (없으면 애니메이션 생략)
    public void MoveTo(Vector3 targetPosition, Action onComplete, TransportType? transport = null)
    {
        StartCoroutine(MoveCoroutine(targetPosition, onComplete, transport));
    }

    // 화면 탭 시 호출 — 배속 토글
    public void ToggleFastForward()
    {
        IsFastForward = !IsFastForward;
        MessageSystem.L(IsFastForward ? "이동 배속 ON." : "이동 배속 OFF.");
    }

    // 스테이지 시작/재시작 시 호출 — 배속 상태 원래대로 초기화
    public void ResetSpeed()
    {
        IsFastForward = false;
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition, Action onComplete, TransportType? transport)
    {
        Debug.Log($"[Mover] 이동 시작 → {targetPosition}");

        // 이동수단에 맞는 애니메이션 재생 시작
        if (transport.HasValue && anim != null)
            anim.PlayMoveAnimation(transport.Value);

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            float currentSpeed = IsFastForward ? moveSpeed * fastForwardMultiplier : moveSpeed;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                currentSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPosition;
        Debug.Log("[Mover] 이동 완료");

        // 이동 종료 → Idle 애니메이션으로 복귀
        if (anim != null)
            anim.PlayIdleAnimation();

        onComplete?.Invoke();
    }
}