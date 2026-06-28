using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private AttackAction attackAction;

    private void Awake()
    {
        Debug.Log("Relay Awake");
    }

    public void AttackAnimationFinished()
    {
        Debug.Log("呼ばれた");
        attackAction.AttackAnimationFinished();
    }
}
