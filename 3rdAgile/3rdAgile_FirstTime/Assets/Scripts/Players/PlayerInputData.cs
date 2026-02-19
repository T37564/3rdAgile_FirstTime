using UnityEngine;
using Fusion;

namespace Network.Player
{
    public struct PlayerInputData : INetworkInput
    {
        public Vector2 move;
        public NetworkBool jump;
    }
}
