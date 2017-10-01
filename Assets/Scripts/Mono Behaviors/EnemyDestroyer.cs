using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyDestroyer : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem hitParticleEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) // ADD TO CONST
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy == null)
            {
                throw new UnityException($"{typeof(EnemyDestroyer)} tried to destroy an object that has \"Enemy\" tag but doesn't have the {typeof(Enemy)} component attached.");
            }

            PlayHitEffect();

            if (!enemy.IsDead)
            {
                enemy.Die(true);
            }
        }
    }
    
    private void PlayHitEffect()
    {
        hitParticleEffect.Play();
    }
}
