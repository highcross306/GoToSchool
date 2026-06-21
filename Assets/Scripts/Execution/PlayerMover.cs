// ============================================================
// PlayerMover.cs
// 역할: 플레이어 오브젝트를 노드에서 노드로 이동
//       코루틴 기반 이동 애니메이션
//       이동 완료 시 ExecutionManager에 콜백
//       speedMultiplier로 화면 탭 시 배속 이동 지원
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

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ExecutionManager가 호출 — 목표 노드로 이동 시작
    public void MoveTo(Vector3 targetPosition, Action onComplete)
    {
        StartCoroutine(MoveCoroutine(targetPosition, onComplete));
    }

    // 화면 탭 시 호출 — 배속 토글
    public void ToggleFastForward()
    {
        IsFastForward = !IsFastForward;
        MessageSystem.L(IsFastForward ? "이동 배속 ON." : "이동 배속 OFF.");
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition, Action onComplete)
    {
        Debug.Log($"[Mover] 이동 시작 → {targetPosition}");

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
        Debug.Log($"[Mover] 이동 완료");

        onComplete?.Invoke();
    }
}