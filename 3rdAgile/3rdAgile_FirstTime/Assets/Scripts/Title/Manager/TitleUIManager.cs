using System.Collections;
using TMPro;
using UnityEngine;

public class TitleUIManager : SingletonMonobehaviour<TitleUIManager>
{
    [Header("タイトル画面のキャンバス")]
    [Header("タイトルのキャンバス本体")]
    [SerializeField] public GameObject titleCanvas = null;

    [Header("ローディング用のイメージ")]
    [SerializeField] public GameObject nowLoadingImage = null;

    [Header("ルーム名を入力するテキスト")]
    [SerializeField] public TMP_InputField roomNameInput = null;



    [Header("ロビー画面のキャンバス")]
    [Header("ロビーのキャンバス本体")]
    [SerializeField] public GameObject lobbyCanvas = null;



    [Header("システムメッセージのキャンバス")]
    [Header("システムメッセージのキャンバス本体")]
    [SerializeField] public GameObject systemMessageCanvas = null;

    [Header("エラー内容を表示するテキスト")]
    [SerializeField] public TextMeshProUGUI errorText = null;

    /// <summary>
    /// 最初に一度だけ実行
    /// </summary>
    private void Start()
    {
        ResetTitleUI();

        ResetLobbyUI();

        ResetSystemMessageCanvas();
    }

    /// <summary>
    /// タイトルUICanvasを初期状態に戻す処理
    /// </summary>
    public void ResetTitleUI()
    {
        titleCanvas.SetActive(true);
        nowLoadingImage.SetActive(false);
        roomNameInput.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// ロビーUICanvasを初期状態に戻す処理
    /// </summary>
    public void ResetLobbyUI()
    {
        lobbyCanvas.SetActive(false);
    }

    /// <summary>
    /// メッセージ表示用UICanvasを初期状態に戻す処理
    /// </summary>

    public void ResetSystemMessageCanvas()
    {
        systemMessageCanvas.SetActive(false);
        errorText.text = string.Empty;
    }

    public IEnumerator ErrorTextDisplay(string errorMessage)
    {
        systemMessageCanvas.SetActive(true);
        
        errorText.text = errorMessage;
        
        yield return new WaitForSecondsRealtime(3.0f);
       
        ResetTitleUI();
        ResetLobbyUI();
        ResetSystemMessageCanvas();
    }
}
