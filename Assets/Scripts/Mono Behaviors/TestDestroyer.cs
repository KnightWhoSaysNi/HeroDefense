using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDestroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().RegisterAttack(19999, DamageType.Normal); // TEST When the enemy finishes a level traps need to know so they wouldn't call it or its navmesh agent
            Destroy(other.gameObject, 0.05f);
        }
    }
}
