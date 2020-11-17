using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public GameObject enemy;
    EnemyShooting enemyShooting;

    void Start()
    {
        enemyShooting = enemy.GetComponent<EnemyShooting>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If the entering collider is the player...
        if (collision.collider.tag == "Player")
        {
            // Try and find an EnemyHealth script on the gameobject hit.
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            // If the EnemyHealth component exist...
            if (playerHealth != null)
            {
                // ... the enemy should take damage.
                playerHealth.TakeDamage(enemyShooting.damagePerShot);
            }

            var metaData = new EventMetaData
            {
                damage_amount = enemyShooting.damagePerShot, 
                enemy_character = "tank"
            };
            SCILLManager.Instance.SendEventAsync("receive-damage", "single", metaData);
        }

        Destroy(gameObject);
    }
}