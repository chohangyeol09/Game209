using UnityEngine;

public class Ku_FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    private void Update()
    {
        transform.position = player.position;
    }
}
