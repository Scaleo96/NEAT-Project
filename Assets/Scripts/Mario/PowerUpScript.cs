using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpScript : MonoBehaviour {

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float moveDistance;
    [SerializeField] private float moveSpeed;
    [SerializeField] private int powerUpScore;

    private Vector2 target;
    private PowerUp pUp;

    void Start() {
        target = (Vector2)transform.position + Vector2.up * moveDistance;
        pUp = new PowerUp(GetComponent<SpriteRenderer>().sprite);
    }

    void Update() {
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(((1 << other.gameObject.layer) & playerLayer) != 0) {
            PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
            pm.SetPowerup(pUp);
            pm.addScore(powerUpScore);
            Destroy(gameObject);
        }
    }

}
