using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int damage = 3;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !Movement.instance.knockback)
        {
            Movement.instance.DamageAndKnockback(transform, damage);
        }
    }
}
