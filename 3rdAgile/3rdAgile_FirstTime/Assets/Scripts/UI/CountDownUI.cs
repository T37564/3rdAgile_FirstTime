// -----------------------------------------------------------------------------------
// ゲーム開始前のカウントダウンをする処理
// CountDownUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class CountDownUI : MonoBehaviour
{
    // カウントダウンの秒数
    private readonly int COUNTDOWN_START_VALUE = 3;

    // カウントダウンの秒感覚
    private readonly float COUNTDOWN_INTERVAL_SECONDS = 1.0f;
    private readonly float START_MESSAGE_DURATION_SECONDS = 0.3f;

    // 使用するフォントのサイズ
    private readonly float COUNTDOWN_MAX_FONT_SIZE = 380.0f;
    // 使用するフォントのサイズ
    private readonly float START_MESSAGE_FONT_SIZE = 100.0f;


    [Header("カウントダウン時に使用するテキスト")]
    [SerializeField] private TextMeshProUGUI countText = null;

    [Header("カウントダウン時に使用する背景")]
    [SerializeField] private Image countDownBackImage = null;

    /// <summary>
    /// カウントダウン開始処理
    /// </summary>
    private void Start()
    {
        StartCoroutine(PlayCountDown());
    }

    /// <summary>
    /// カウントダウン処理
    /// </summary>
    private IEnumerator PlayCountDown()
    {
        // テキストの初期化
        countText.text = COUNTDOWN_START_VALUE.ToString();
        // サイズの初期化
        countText.fontSize = COUNTDOWN_MAX_FONT_SIZE;

        // カウントダウンスタート
        for (int i = 0; i < COUNTDOWN_START_VALUE; i++)
        {
            countText.text = (COUNTDOWN_START_VALUE - i).ToString();
            countText.DOFade(0.0f, COUNTDOWN_INTERVAL_SECONDS);
            yield return new WaitForSeconds(COUNTDOWN_INTERVAL_SECONDS);
            countText.color = Color.white;
        }

        // 探検開始テキスト表示
        countText.fontSize = START_MESSAGE_FONT_SIZE;
        countText.text = "探検開始!!";
        yield return new WaitForSeconds(START_MESSAGE_DURATION_SECONDS);
        countText.DOFade(0.0f, START_MESSAGE_DURATION_SECONDS);
        countDownBackImage.DOFade(0.0f, START_MESSAGE_DURATION_SECONDS);
    }
}
