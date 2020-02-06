using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActivator : MonoBehaviour {

    [SerializeField] private LayerMask enemyLayer;

    private void OnTriggerEnter2D(Collider2D other) {
        if(((1 << other.gameObject.layer) & enemyLayer) != 0)
            other.GetComponent<EnemyMovement>().isActive = true;
    }
}
