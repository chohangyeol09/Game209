using UnityEngine;

public class Ku_PlayerWeaponAttack : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player is Attacking Enemy!");
    }
}
