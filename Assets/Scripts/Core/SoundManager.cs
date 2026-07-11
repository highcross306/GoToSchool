// ============================================================
// SoundManager.cs
// 역할: 게임 전역 효과음(SFX) 재생 총괄 싱글톤
//       "진짜로 비어있는" AudioSource를 찾아서 재생 —
//       재생 중인 소스를 재사용하지 않으므로 pitch/volume 간섭 없이
//       같은 효과음끼리도 완전히 독립적으로 겹쳐서 재생 가능
//       풀이 부족하면 임시 소스를 동적으로 만들고 재생 끝나면 자동 파괴
// 부착: Bootstrap(또는 Managers 프리팹) 오브젝트
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public enum Sfx
    {
        Click,
        Walk,
        Bus,
        Taxi,
        CarMove,
        Confirm,
        Cash,
        EventPositive,
        EventNegative,
        StageFail,
        StageClear
    }

    [Serializable]
    public struct SfxEntry
    {
        public Sfx id;
        public AudioClip clip;
    }

    [Header("SFX 클립 등록 (Inspector에서 11개 연결)")]
    public SfxEntry[] sfxEntries;

    [Header("기본 풀 크기 (동시에 겹칠 것으로 예상되는 최대 개수)")]
    public int poolSize = 8;

    // SettingsPanel.cs와 동일한 키 — 반드시 일치시킬 것
    private const string SFX_KEY = "SFXVolume";

    private Dictionary<Sfx, AudioClip> clipLookup;
    private readonly List<AudioSource> sourcePool = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Managers 프리팹의 자식으로 있으면 DontDestroyOnLoad가 무시되므로
        // 루트로 분리한 뒤 유지시킴
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        BuildLookup();
        for (int i = 0; i < poolSize; i++)
            sourcePool.Add(CreateSource());
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

    private AudioSource CreateSource()
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        return src;
    }

    private float CurrentVolume => PlayerPrefs.GetFloat(SFX_KEY, 1f);

    // 효과음 재생 (겹쳐 재생 가능, 서로 간섭 없음)
    public void Play(Sfx id, float volumeScale = 1f)
    {
        PlayInternal(id, 1f, volumeScale);
    }

    // 발소리처럼 반복 재생 시 단조로움을 줄이기 위한 피치 랜덤 재생
    public void PlayWithPitchVariance(Sfx id, float min = 0.95f, float max = 1.05f, float volumeScale = 1f)
    {
        PlayInternal(id, UnityEngine.Random.Range(min, max), volumeScale);
    }

    private void PlayInternal(Sfx id, float pitch, float volumeScale)
    {
        if (clipLookup == null || !clipLookup.TryGetValue(id, out AudioClip clip))
        {
            Debug.LogWarning($"[SoundManager] {id} 클립을 찾을 수 없습니다.");
            return;
        }

        AudioSource src = GetFreeSource();
        src.pitch = pitch;
        src.PlayOneShot(clip, CurrentVolume * volumeScale);

        if (!sourcePool.Contains(src))
            StartCoroutine(DestroyAfterPlay(src, clip.length));
    }

    // 재생 중이 아닌 소스를 우선 반환. 없으면 임시 소스를 새로 만들어서 반환하고
    // 클립 길이만큼 재생 후 자동 파괴한다.
    private AudioSource GetFreeSource()
    {
        foreach (AudioSource src in sourcePool)
        {
            if (!src.isPlaying)
                return src;
        }

        // 풀이 전부 사용 중 → 임시 소스 생성 (호출부에서 재생 끝나면 자동 정리)
        AudioSource temp = CreateSource();
        return temp;
    }

    private IEnumerator DestroyAfterPlay(AudioSource src, float clipLength)
    {
        yield return new WaitForSeconds(clipLength + 0.1f);
        if (src != null) Destroy(src);
    }
}