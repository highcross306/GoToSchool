// ============================================================
// SoundManager.cs
// 역할: 게임 전역 효과음(SFX) 재생 총괄 싱글톤
//       AudioSource 풀링으로 효과음이 겹쳐도 잘리지 않음
//       SettingsPanel의 "SFXVolume" PlayerPrefs 키와 연동
// 부착: Bootstrap 씬의 매니저 오브젝트에 부착 (DontDestroyOnLoad)
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public enum Sfx
    {
        Click,          // 빈 공간/노드/다음스테이지/다시하기 클릭
        Walk,           // 걷기 선택 후 이동
        Bus,            // 버스 카드 클릭
        Taxi,           // 택시 카드 클릭
        CarMove,        // 버스/택시 선택 후 이동
        Confirm,        // 결정 버튼 클릭
        Cash,           // 시간/자금 반영
        EventPositive,  // 이득 이벤트
        EventNegative,  // 손해 이벤트
        StageFail,      // 스테이지 실패 (BGM 정지)
        StageClear      // 스테이지 클리어 (BGM 정지)
    }

    [Serializable]
    public struct SfxEntry
    {
        public Sfx id;
        public AudioClip clip;
    }

    [Header("SFX 클립 등록 (Inspector에서 11개 연결)")]
    public SfxEntry[] sfxEntries;

    [Header("동시 재생 채널 수")]
    public int poolSize = 8;

    // SettingsPanel.cs와 동일한 키 — 반드시 일치시킬 것
    private const string SFX_KEY = "SFXVolume";

    private Dictionary<Sfx, AudioClip> clipLookup;
    private List<AudioSource> sourcePool;
    private int nextIndex;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookup();
        BuildPool();
    }

    private void BuildLookup()
    {
        clipLookup = new Dictionary<Sfx, AudioClip>();
        foreach (SfxEntry entry in sfxEntries)
        {
            if (entry.clip == null)
            {
                Debug.LogWarning($"[SoundManager] {entry.id}에 클립이 연결되지 않았습니다.");
                continue;
            }
            clipLookup[entry.id] = entry.clip;
        }
    }

    private void BuildPool()
    {
        sourcePool = new List<AudioSource>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            sourcePool.Add(src);
        }
    }

    // 현재 SFX 볼륨 (SettingsPanel 슬라이더 저장값을 매번 읽어옴)
    private float CurrentVolume => PlayerPrefs.GetFloat(SFX_KEY, 1f);

    // 효과음 재생
    public void Play(Sfx id, float volumeScale = 1f)
    {
        if (clipLookup == null || !clipLookup.TryGetValue(id, out AudioClip clip))
        {
            Debug.LogWarning($"[SoundManager] {id} 클립을 찾을 수 없습니다.");
            return;
        }

        AudioSource src = GetNextSource();
        src.pitch = 1f;
        src.PlayOneShot(clip, CurrentVolume * volumeScale);
    }

    // 발소리처럼 반복되는 소리의 단조로움을 줄이기 위한 피치 랜덤 재생
    public void PlayWithPitchVariance(Sfx id, float min = 0.95f, float max = 1.05f, float volumeScale = 1f)
    {
        if (clipLookup == null || !clipLookup.TryGetValue(id, out AudioClip clip))
        {
            Debug.LogWarning($"[SoundManager] {id} 클립을 찾을 수 없습니다.");
            return;
        }

        AudioSource src = GetNextSource();
        src.pitch = UnityEngine.Random.Range(min, max);
        src.PlayOneShot(clip, CurrentVolume * volumeScale);
    }

    private AudioSource GetNextSource()
    {
        AudioSource src = sourcePool[nextIndex];
        nextIndex = (nextIndex + 1) % sourcePool.Count;
        return src;
    }
}