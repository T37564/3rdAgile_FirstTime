// -----------------------------------------------------------------------------------
// NowLoading時のUIを管理するクラス
// NowLoadingUIController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterTextController : MonoBehaviour
{
    // テキストを表示する間隔
    private readonly float INTERVAL = 0.1f;

    // テキストを表示した後の待機時間
    private readonly float WAIT_TIME = 1.0f;

    [Header("Loading時に使用するテキスト")]
    [SerializeField] private TMP_Text loadingText = null;

    private string typewriterText = "";

    /// <summary>
    /// ローディングテキストを表示するコルーチンを開始する
    /// </summary>
    private void OnEnable()
    {
        typewriterText = loadingText.text;
        StartCoroutine(ShowLoadingText());
    }

    /// <summary>
    /// コルーチンを停止し、表示中のテキストをリセットする
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();

        loadingText.text = "";
    }


    /// <summary>
    /// 「Now Loading...」を一文字ずつ繰り返し表示するコルーチン
    /// </summary>
    private IEnumerator ShowLoadingText()
    {
        // 無限ループでテキストを表示し続ける
        while (true)
        {
            loadingText.text = "";

            // テキストを一文字ずつ表示する
            for (int i = 0; i < typewriterText.Length; i++)
            {
                loadingText.text += typewriterText[i];
                yield return new WaitForSeconds(INTERVAL);
            }

            // テキスト表示後に少し待機する
            yield return new WaitForSeconds(WAIT_TIME);
        }
    }
}