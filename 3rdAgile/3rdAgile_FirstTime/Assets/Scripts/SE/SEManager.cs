using UnityEngine;

/// <summary>
/// SEを管理するクラス
/// </summary>
public class SEManager : MonoBehaviour
{
    [Header("オーディオソース参照用")]
    [SerializeField] private AudioSource audioSource = null;

    [Header("SEリスト参照用")]
    [SerializeField] private SEList list = null;


    public SEList SEList => list;

    /// <summary>
    /// ほかクラスで効果音を鳴らす処理
    /// </summary>
    public void SEPlayOneShot(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }
}
