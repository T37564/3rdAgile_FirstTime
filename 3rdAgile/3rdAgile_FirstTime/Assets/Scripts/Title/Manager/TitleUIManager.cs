using TMPro;
using UnityEngine;

public class TitleUIManager : SingletonMonobehaviour<TitleUIManager>
{
    [Header("タイトル画面のキャンバス")]
    [Header("タイトルのキャンバス本体")]
    [SerializeField] public GameObject titleCanvas = null;

    [Header("ローディング用のイメージ")]
    [SerializeField] public GameObject nowLoadingImage = null;

    [Header("入力するテキスト")]
    [SerializeField] public TMP_InputField roomNameInput = null;



    [Header("ロビー画面のキャンバス")]
    [Header("ロビーのキャンバス本体")]
    [SerializeField] public GameObject lobbyCanvas = null;


    /// <summary>
    /// 最初に一度だけ実行
    /// </summary>
    private void Start()
    {
        ResetTitleUI();

        ResetLobbyUI();
    }

    /// <summary>
    /// タイトルUICanvasを初期に状態に戻す処理
    /// </summary>
    public void ResetTitleUI()
    {
        titleCanvas.SetActive(true);
        nowLoadingImage.SetActive(false);
        roomNameInput.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// ロビーUICanvasを初期に状態に戻す処理
    /// </summary>
    public void ResetLobbyUI()
    {
        lobbyCanvas.SetActive(false);
    }
}
