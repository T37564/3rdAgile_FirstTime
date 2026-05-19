// -----------------------------------------------------------------------------------
// プレイヤー参加/退出のUI更新、人数表示、ラベル変更など。
// TitleCanvasDisplaySettings.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleCanvasDisplaySettings : SingletonMonobehaviour<TitleCanvasDisplaySettings>
{
    // 画面に表示する時間
    private readonly float DISPLAY_TIME = 3.0f;


    [Header("Title UI")]
    [SerializeField] public GameObject titleCanvas = null;

    [Header("ローディング用のイメージ")]
    [SerializeField] public GameObject nowLoadingImage = null;

    [Header("ルーム名を入力するテキスト")]
    [SerializeField] public TMP_InputField roomNameInput = null;



    [Header("Lobby UI")]
    [SerializeField] public GameObject lobbyCanvas = null;

    [Header("ゲームスタート用のボタン")]
    [SerializeField] public GameObject gameStartButton = null;

    [Header("ロビーの人数を記入するUI")]
    [SerializeField] public TextMeshProUGUI playerCountDisplayText = null;



    [Header("System Message UI")]
    [SerializeField] public GameObject systemMessageCanvas = null;

    [Header("エラーメッセージを表示する際の背景画像")]
    [SerializeField] public Image systemMessageBackImage = null;

    [Header("エラー内容を表示するテキスト")]
    [SerializeField] public TextMeshProUGUI[] errorText = null;

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
        systemMessageCanvas.SetActive(false);

        // テキストの中身を空にする
        foreach (var item in errorText)
        {
            item.text = string.Empty;
        }
    }


    /// <summary>
    /// エラーメッセージを数秒間表示する処理
    /// </summary>
    public IEnumerator ShowErrorMessage(bool displayBackImage,string errorMessage, int textIndex)
    {
        systemMessageCanvas.SetActive(true);

        // 黒背景表示非表示
        systemMessageBackImage.enabled = displayBackImage;

        if (textIndex < 0 || errorText.Length <= textIndex)
            yield break;

        // 指定したテキスト欄にエラーメッセージを表示
        errorText[textIndex].text = errorMessage;

        yield return new WaitForSecondsRealtime(DISPLAY_TIME);

        // キャンバスの状態を戻す
        ResetSystemMessageCanvas();
    }
}
