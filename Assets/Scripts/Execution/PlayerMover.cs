// ============================================================
// PlayerMover.cs
// 역할: 플레이어 오브젝트를 노드에서 노드로 이동
//       코루틴 기반 이동 애니메이션
//       이동 완료 시 ExecutionManager에 콜백
//       speedMultiplier로 가속 버튼 클릭 시 배속 이동 지원
//       transport 파라미터로 이동수단별 애니메이션 재생
//
// [수정 1] PlayerAnimationController를 static Instance가 아니라
//          GetComponent로 같은 오브젝트에서 찾도록 변경.
// [수정 2] IsMoving 공개 프로퍼티 추가
//          → 가속 버튼이 "이동 중일 때만 클릭 가능"을 판단하는 근거로 사용
// [수정 3] IsFastForward 초기화 시점을 "이동 시작 시"에서
//          "이동 완료 시"로 변경
//          → 이동이 끝나자마자 가속 버튼이 즉시 비활성화 상태가 됨.
//          (이전에는 다음 이동이 시작될 때까지 켜진 상태가 그대로 남아있었음)
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
    public float fastForwardMultiplier = 2f; // 가속 버튼 클릭 시 배속 배율

    public bool IsFastForward { get; private set; } = false;

    // 현재 노드 간 이동 코루틴이 진행 중인지 여부.
    // 가속 버튼(FastForwardButton)이 클릭 허용 여부를 판단하는 데 사용한다.
    public bool IsMoving { get; private set; } = false;

    // 같은 오브젝트에 붙어 있는 애니메이션 컨트롤러 (Awake에서 캐싱)
    private PlayerAnimationController anim;

    private void Awake()
    {
        // ---- 진단 1: 씬에 PlayerMover가 여러 개인가? ----
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

    // 가속 버튼 클릭 시 호출 — 배속 토글
    // 이동 중이 아닐 때 호출하는 것을 막는 책임은 호출부(FastForwardButton)에 있다.
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

        IsMoving = true;

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

        // 이동 종료 → Idle 애니메이션으로 복귀, 이동 상태 해제
        IsMoving = false;

        // 이동이 끝났으므로 가속은 항상 꺼진(비활성화) 상태로 리셋한다.
        // 다음 이동이 시작되기 전, 계획 단계 동안에도 버튼이 켜진 채로
        // 남아있지 않도록 여기서 즉시 초기화한다.
        IsFastForward = false;

        if (anim != null)
            anim.PlayIdleAnimation();

        onComplete?.Invoke();
    }
}