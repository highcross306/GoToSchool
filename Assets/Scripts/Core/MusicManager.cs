// ============================================================
// MusicManager.cs
// 역할: 게임플레이 BGM 셔플 재생 총괄 싱글톤
//       - 스테이지 시작마다 새로 셔플해서 처음 곡부터 재생
//       - 곡이 끝나면 셔플된 순서대로 다음 곡 자동 재생
//       - 셔플 리스트를 다 돌면 그때 새로 셔플 (한 바퀴 돌기 전엔 안 깨짐)
//       - 결과창 뜨기 직전에 정지 (ResultUI.ShowPopup에서 호출)
//       - 볼륨은 SettingsPanel의 "BGMVolume" PlayerPrefs 키를 그대로 사용
// 부착: Bootstrap(또는 Managers 프리팹) 오브젝트
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("게임플레이 중 재생할 BGM 목록")]
    public AudioClip[] playlist;

    [Header("메인 메뉴 BGM (단일 트랙, 반복 재생)")]
    public AudioClip menuTrack;

    [Header("엔딩 화면 BGM (단일 트랙, 반복 재생)")]
    public AudioClip endingTrack;

    private AudioSource source;
    private List<AudioClip> shuffledQueue = new();
    private int queueIndex;
    private bool isActive; // true면 Update에서 곡 종료를 감지해 다음 곡 재생

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Managers 프리팹의 자식으로 있으면 DontDestroyOnLoad가 무시되므로
        // 루트로 분리한 뒤 유지시킴
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
    }

    private void Update()
    {
        // 볼륨은 메뉴 BGM/스테이지 BGM 상관없이 항상 실시간 반영
        // (설정 패널에서 조절하면 바로 들리도록)
        if (source != null)
            source.volume = SettingsPanel.GetBGMVolume();

        if (!isActive) return; // 셔플 자동전환은 스테이지 재생 모드에서만 필요

        // 재생 중이던 곡이 끝났으면 다음 곡으로
        if (!source.isPlaying)
            PlayNext();
    }

    // 메인 메뉴에서 호출 — 단일 트랙을 반복 재생한다 (셔플/자동전환 없음)
    public void PlayMenuMusic()
    {
        if (menuTrack == null)
        {
            Debug.LogWarning("[MusicManager] menuTrack이 비어있어 메뉴 BGM을 재생할 수 없습니다.");
            return;
        }

        // 이미 같은 곡이 재생 중이면 다시 틀지 않음 (메인 메뉴 재진입 시 끊김 방지)
        if (isActive == false && source.clip == menuTrack && source.isPlaying) return;

        isActive = false; // 스테이지용 자동전환 로직 끔
        source.loop = true;
        source.volume = SettingsPanel.GetBGMVolume();
        source.clip = menuTrack;
        source.Play();
    }

    // 엔딩 화면에서 호출 — 단일 트랙을 반복 재생한다 (셔플/자동전환 없음)
    public void PlayEndingMusic()
    {
        if (endingTrack == null)
        {
            Debug.LogWarning("[MusicManager] endingTrack이 비어있어 엔딩 BGM을 재생할 수 없습니다.");
            return;
        }

        isActive = false; // 스테이지용 자동전환 로직 끔
        source.loop = true;
        source.volume = SettingsPanel.GetBGMVolume();
        source.clip = endingTrack;
        source.Play();
    }

    // 스테이지가 시작될 때마다 호출 — 완전히 새로운 셔플로 처음부터 재생
    public void StartNewStageMusic()
    {
        if (playlist == null || playlist.Length == 0)
        {
            Debug.LogWarning("[MusicManager] playlist가 비어있어 재생할 BGM이 없습니다.");
            return;
        }

        Reshuffle();
        queueIndex = 0;
        isActive = true;
        source.loop = false; // 메뉴 BGM(loop=true)에서 넘어왔을 경우를 대비해 재설정
        PlayCurrent();
    }

    // 결과창이 뜨기 직전에 호출 — 음악 정지
    public void StopMusic()
    {
        isActive = false;
        source.Stop();
    }

    private void PlayNext()
    {
        queueIndex++;

        // 셔플 리스트를 다 돌았으면 새로 셔플해서 처음부터
        if (queueIndex >= shuffledQueue.Count)
        {
            Reshuffle();
            queueIndex = 0;
        }

        PlayCurrent();
    }

    private void PlayCurrent()
    {
        source.volume = SettingsPanel.GetBGMVolume();
        source.clip = shuffledQueue[queueIndex];
        source.Play();
    }

    // Fisher-Yates 셔플로 새 재생 순서 생성
    private void Reshuffle()
    {
        shuffledQueue = new List<AudioClip>(playlist);

        for (int i = shuffledQueue.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffledQueue[i], shuffledQueue[j]) = (shuffledQueue[j], shuffledQueue[i]);
        }
    }
}