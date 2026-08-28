using UnityEngine;

[CreateAssetMenu(fileName = "SEList", menuName = "Audio/SE List")]
public class SEList : ScriptableObject
{
    [Header("「チームメンバーを募集」か「チームに入る」を押したとき")]
    public AudioClip teamButtonSE = null;
    [Header("数字を消したとき")]
    public AudioClip numberDeleteSE = null;
    [Header("数字を入力したとき")]
    public AudioClip numberInputSE = null;
    [Header("自分以外のプレイヤーが退室したら")]
    public AudioClip playerLeaveSE = null;
    [Header("自分以外のプレイヤーが入室したら")]
    public AudioClip playerJoinSE = null;
    [Header("ゲームスタートを押したら")]
    public AudioClip gameStartSE = null;

    [Header("お宝をつかんだ時")]
    public AudioClip treasurePickupSE = null;
    [Header("お宝を納品箱に入れたとき")]
    public AudioClip treasureDeliverSE = null;
}
