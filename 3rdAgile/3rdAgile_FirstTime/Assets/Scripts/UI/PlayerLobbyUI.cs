using UnityEngine;
using Fusion;

public class PlayerLobbyUI : NetworkBehaviour
{
    [Header("u‚ ‚È‚½v‚Æ‘‚©‚ê‚½UI")]
    [SerializeField] private GameObject playerMarker = null;

    public override void Spawned()
    {
        playerMarker.SetActive(Object.HasInputAuthority);
    }
}
