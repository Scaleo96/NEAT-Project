using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZoneTrigger : MonoBehaviour {

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask enemyLayer;

    private void OnTriggerEnter2D(Collider2D other) {
        //player collision
        if(((1 << other.gameObject.layer) & playerLayer) != 0)
           other.gameObject.GetComponent<PlayerManager>().PlayerDeath();

        //enemy collision
        if(((1 << other.gameObject.layer) & enemyLayer) != 0) 
            Destroy(other.gameObject, 0.5f);
    }
}
