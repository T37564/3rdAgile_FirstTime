using UnityEngine;

public class CorridorSpawnPoints : MonoBehaviour
{
    [SerializeField] private Transform[] roomSpawnPoints;
     public Transform[] RoomSpawnPoints => roomSpawnPoints;
}
