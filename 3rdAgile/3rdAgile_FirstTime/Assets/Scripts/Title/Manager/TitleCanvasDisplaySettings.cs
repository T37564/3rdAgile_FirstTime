// -----------------------------------------------------------------------------------
// プレイヤー参加/退出のUI更新、人数表示、ラベル変更など。
// NetworkGameStarter.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleCanvasDisplaySettings : SingletonMonobehaviour<TitleCanvasDisplaySettings>
{
    private readonly float DISPLASYTIME = 3.0f;


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

    [Header("ゲームスタート用のボタン")]
    [SerializeField] public GameObject gameStartButton = null;

    [Header("ロビーの人数を記入するUI")]
    [SerializeField] public TextMeshProUGUI playerCountDisplayText = null;

    [Header("システムメッセージのキャンバス")]
    [Header("システムメッセージのキャンバス本体")]
    [SerializeField] public GameObject systemMessageCanvas = null;

    [Header("エラーメッセージを表示する際の背景画像")]
    [SerializeField] public Image systemMessageBackImage = null;

    [Header("エラー内容を表示するテキスト")]
    [SerializeField] public TextMeshProUGUI[] errorText = null;

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
        gameStartButton.SetActive(false);
    }

    /// <summary>
    /// メッセージ表示用UICanvasを初期状態に戻す処理
    /// </summary>

    public void ResetSystemMessageCanvas()
    {
        systemMessageCanvas.SetActive(false);

        foreach (var item in errorText)
        {
            item.text = string.Empty;
        }
    }


    /// <summary>
    /// エラーメッセージを数秒間表示する処理
    /// </summary>
    public IEnumerator ErrorTextDisplay(bool displayBackImage,string errorMessage, int textSize)
    {
        systemMessageCanvas.SetActive(true);

        systemMessageBackImage.enabled = displayBackImage;

        errorText[textSize].text = errorMessage;

        yield return new WaitForSecondsRealtime(DISPLASYTIME);

        ResetSystemMessageCanvas();
    }
}
