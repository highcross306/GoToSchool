// ============================================================
// TransportSettings.cs
// 역할: 이동수단별 고정 비용/시간 전역 설정 ScriptableObject
//       인스펙터에서 수치 조정 가능
// 생성: Assets/ScriptableObjects/ 폴더 우클릭
//       → Create → Game → Transport Settings
// ============================================================

using UnityEngine;

[System.Serializable]
public class TransportSetting
{
    public TransportType transportType;
    public int cost;        // 비용 (원)
    public int timeMinutes; // 소요 시간 (분)
}

[CreateAssetMenu(
    fileName = "TransportSettings",
    menuName = "Game/Transport Settings"
)]
public class TransportSettings : ScriptableObject
{
    public TransportSetting[] settings;

    // 이동수단 타입으로 설정값 반환
    public TransportSetting Get(TransportType type)
    {
        foreach (TransportSetting s in settings)
        {
            if (s.transportType == type) return s;
        }
        Debug.LogWarning($"[TransportSettings] {type} 설정값을 찾을 수 없습니다.");
        return null;
    }
}