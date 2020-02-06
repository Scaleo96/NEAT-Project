using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour {

    [SerializeField] private GameObject pipe;
    [SerializeField] private Transform desotryPos;
    [SerializeField] private float spawnTime;
    [SerializeField] private float ySpawnRange;

    private void Start() {
        StartCoroutine(SpawnPipe());
    }

    private IEnumerator SpawnPipe() {
        while(true) {
            GameObject instance = Instantiate(pipe, new Vector2(transform.position.x, transform.position.y + Random.Range(-ySpawnRange, ySpawnRange)), Quaternion.identity);
            instance.GetComponent<PipeMover>().destoryPos = desotryPos;
            yield return new WaitForSeconds(spawnTime);
        }
    }

}
