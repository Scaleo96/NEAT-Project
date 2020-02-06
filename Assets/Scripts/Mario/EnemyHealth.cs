using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour {

    [SerializeField] private int health;
    [SerializeField] private Vector2 deathForce;
    public int deathScore;

    private CapsuleCollider2D capColl;
    private Rigidbody2D rb2d;

    private void Start() {
        capColl = GetComponent<CapsuleCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    public void DealDamage(int dmg) {
        health -= 1;

        if(health <= 0) {
            health = 0;
            Die();
        }    
    }

    private void Die() {
        capColl.enabled = false;
        rb2d.AddForce(new Vector2(-1, 1) * deathForce, ForceMode2D.Impulse);
    }

}
