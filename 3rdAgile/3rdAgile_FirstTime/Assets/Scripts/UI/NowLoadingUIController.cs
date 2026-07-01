// -----------------------------------------------------------------------------------
// NowLoading時のUIを管理するクラス
// NowLoadingUIController.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using TMPro;
using UnityEngine;

public class NowLoadingUIController : MonoBehaviour
{
    // テキストを表示する間隔
    private readonly float INTERVAL = 0.1f;

    // テキストを表示した後の待機時間
    private readonly float WAIT_TIME = 1.0f;

    // 表示するテキスト
    private readonly string LOADING_TEXT = "Now Loading...";


    [Header("Loading時に使用するテキスト")]
    [SerializeField] private TMP_Text loadingText = null;

    /// <summary>
    /// ローディングテキストを表示するコルーチンを開始する
    /// </summary>
    private void OnEnable()
    {
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
            for (int i = 0; i < LOADING_TEXT.Length; i++)
            {
                loadingText.text += LOADING_TEXT[i];
                yield return new WaitForSeconds(INTERVAL);
            }

            // テキスト表示後に少し待機する
            yield return new WaitForSeconds(WAIT_TIME);
        }
    }
}