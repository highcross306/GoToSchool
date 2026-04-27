// ============================================================
// PlayerMover.cs
// 역할: 플레이어 오브젝트를 노드에서 노드로 이동
//       코루틴 기반 이동 애니메이션
//       이동 완료 시 ExecutionManager에 콜백
// ============================================================

using System;
using System.Collections;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    public static PlayerMover Instance { get; private set; }

    [Header("이동 설정")]
    public float moveSpeed = 3f; // 이동 속도 (유닛/초)

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ExecutionManager가 호출 — 목표 노드로 이동 시작
    // onComplete: 이동 완료 시 호출할 콜백
    public void MoveTo(Vector3 targetPosition, Action onComplete)
    {
        StartCoroutine(MoveCoroutine(targetPosition, onComplete));
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition, Action onComplete)
    {
        Debug.Log($"[Mover] 이동 시작 → {targetPosition}");

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 목표 위치에 정확히 고정
        transform.position = targetPosition;
        Debug.Log($"[Mover] 이동 완료");

        onComplete?.Invoke();
    }
}