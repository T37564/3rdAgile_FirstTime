// -----------------------------------------------------------------------------------
// タイトル画面・ロビー画面・システムメッセージUIを管理するクラス
// TitleCanvasDisplaySettings.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleCanvasDisplaySettings : SingletonMonobehaviour<TitleCanvasDisplaySettings>
{
    // エラーメッセージを表示する時間
    private readonly float DISPLAY_TIME = 3.0f;


    // Title UI
    public GameObject titleCanvas = null;

    // ローディング用のイメージ
    public GameObject nowLoadingImage = null;

    // ルーム名を入力するテキスト
    public TMP_InputField roomNameInput = null;


    // Lobby UI
    public GameObject lobbyCanvas = null;

    // ゲームスタート用のボタン
    public GameObject gameStartButton = null;

    // ロビーの人数を表示するUI
    public TextMeshProUGUI playerCountDisplayText = null;



    // System Message UI
    public GameObject systemMessageCanvas = null;

    // エラーメッセージを表示する際の背景画像
    public Image systemMessageBackImage = null;

    // エラー内容を表示するテキスト
    public TextMeshProUGUI[] errorText = null;

    /// <summary>
    /// すべてのキャンバスを初期化
    /// </summary>
    private void Start()
    {
        ResetTitleUI();

        ResetLobbyUI();

        ResetSystemMessageCanvas();
    }

    /// <summary>
    /// タイトルUIを初期状態に戻す
    /// </summary>
    public void ResetTitleUI()
    {
        titleCanvas.SetActive(true);
        nowLoadingImage.SetActive(false);
        roomNameInput.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// ロビーUIを初期状態に戻す
    /// </summary>
    public void ResetLobbyUI()
    {
        lobbyCanvas.SetActive(false);
        gameStartButton.SetActive(false);
    }

    /// <summary>
    /// システムメッセージUIを初期状態に戻す
    /// </summary>
    public void ResetSystemMessageCanvas()
    {
        // エラーを伝えるためのキャンバス非表示
        systemMessageCanvas.SetActive(false);

        // テキストの中身を空にする
        foreach (var item in errorText)
        {
            item.text = string.Empty;
        }
    }


    /// <summary>
    /// エラーメッセージを数秒間表示する
    /// </summary>
    public IEnumerator ShowErrorMessage(bool displayBackImage, string errorMessage, int textIndex)
    {
        // エラーを表示するキャンバスを非表示にする
        systemMessageCanvas.SetActive(true);

        // 黒背景の表示/非表示を切り替える
        systemMessageBackImage.enabled = displayBackImage;

        // 指定したインデックスが範囲外の場合
        if (textIndex < 0 || errorText.Length <= textIndex) yield break;

        // 指定したテキスト欄にエラーメッセージを表示
        errorText[textIndex].text = errorMessage;

        yield return new WaitForSecondsRealtime(DISPLAY_TIME);

        // SystemMessageUIを初期化する
        ResetSystemMessageCanvas();
    }
}