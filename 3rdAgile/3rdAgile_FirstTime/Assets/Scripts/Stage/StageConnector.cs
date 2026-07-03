using UnityEngine;

public class StageConnector : MonoBehaviour
{
    [SerializeField] private GridDirection direction;

    public GridDirection Direction => direction;
}
