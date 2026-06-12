using UnityEngine;

public class SimplePlayer : MonoBehaviour
{
    [SerializeField] public int hp = 0;

    /// <summary>
    /// ダメージを喰らう処理
    /// </summary>
    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log(hp);

        if (hp <= 0)
        {
            PlayerDown();
        }
    }

    /// <summary>
    /// プレイヤーがダウンしたときの処理
    /// </summary>
    private void PlayerDown()
    {

    }
}
