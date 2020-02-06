using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NEATAgent : MonoBehaviour {


    [Header("Save settings")]
    public bool newNetowrk;
    public Brain brain;
    [SerializeField] private bool saveOnQuit;

    [Header("Collision/view settings")]
    [SerializeField] private LayerMask[] MasksToCheck;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2 bucketSize;
    public Vector2 offset;
    [SerializeField] private int defualtValueInGrid;

    [Header("NEAT settings")]
    [Tooltip("Amount of buttons/inputs requierd for the agent object to function"),
     SerializeField]
    private int outputs;

    [Header("Gizmos settings")]
    [SerializeField] private bool drawGizmos;
    [SerializeField] private Color gizmosColor;

    private NEAT NEATNetowrk;
    private NEATEvolver evolver;
    private LayerMask combinedMask;
    private Dictionary<LayerMask, int> maskTable;
    private Vector2 gridOffset;

    private void Awake() {

        if(newNetowrk || brain == null) {
            int inputSize = gridSize.x * gridSize.y;
            NEATNetowrk = new NEAT(inputSize, outputs);
        } else {
            SaveBrain saveBrain = brain.GetNEAT();
            NEATNetowrk = saveBrain.LoadBrain();
            gridSize = new Vector2Int((int)saveBrain.gridSize[0], (int)saveBrain.gridSize[1]);
            bucketSize = new Vector2Int((int)saveBrain.bucketSize[0], (int)saveBrain.bucketSize[1]);
            offset = new Vector2Int((int)saveBrain.offset[0], (int)saveBrain.offset[1]);
            NEATNetowrk.DenugNEAT();
        }

        ///convert all layers to a singel one
        string[] layerNames = new string[MasksToCheck.Length];
        for(int i = 0; i < layerNames.Length; i++) {
            int counter = 0;
            int currentLayer = MasksToCheck[i];

            while(currentLayer > 0) {
                counter++;
                currentLayer = currentLayer >> 1;
            }

            layerNames[i] = LayerMask.LayerToName(counter - 1);
        }

        combinedMask = LayerMask.GetMask(layerNames);

        maskTable = new Dictionary<LayerMask, int>();
        for(int i = 0; i < MasksToCheck.Length; i++)
            maskTable.Add(MasksToCheck[i], i);
    }

    private void Start() {
        if(!newNetowrk && brain != null) {
            evolver.uiDrawer.ClearUI();
            evolver.uiDrawer.DrawNetwork(NEATNetowrk);
        }
    }

    private void OnApplicationQuit() {
        if(saveOnQuit)
            SaveNeat();
    }

    public void SaveNeat() {
        brain.SetNEAT(evolver.GetBestNetwork(), gridSize, bucketSize, offset);
    }

    public NEAT GetNEAT() {
        return NEATNetowrk;
    }

    public void SetNEAT(NEAT network) {
        NEATNetowrk = network;
    }

    public float[] GetOutputs() {
        return NEATNetowrk.SendInputs(GetView());
    }

    public void PointMutate(float randomStepAmount) {
        NEATNetowrk.PointMutate(randomStepAmount);
    }

    public string LinkMutate() {
        return NEATNetowrk.LinkMutate();
    }

    public void NodeMutate() {
        NEATNetowrk.NodeMutate();
    }

    public void EnableDisableMutate() {
        NEATNetowrk.DisableMutate();
    }

    public void DebugNEAT() {
        NEATNetowrk.DenugNEAT();
    }

    public float[] GetView() {
        float[] input = new float[gridSize.x * gridSize.y];
        //float[] hitCount = new float[gridSize.x * gridSize.y];

        for(int i = 0; i < input.Length; i++)
            input[i] = defualtValueInGrid;

        Vector2 bounds = gridSize * bucketSize;

        for(int i = 0; i < gridSize.y; i++) {
            float currentY = i * bucketSize.y - bounds.y / 2;

            for(int j = 0; j < gridSize.x; j++) {
                float currentX = j * bucketSize.x - bounds.x / 2;
                Vector2 currenPos = new Vector2(currentX + bucketSize.x / 2, currentY + bucketSize.y / 2) + (Vector2)transform.position + offset;

                Collider2D[] collisions = Physics2D.OverlapBoxAll(currenPos, bucketSize, 0, combinedMask);

                if(collisions.Length > 0) {
                    if(collisions.Length > 1) {
                        int counter = 0;

                        foreach(var collision in collisions) {
                            input[i * j] += maskTable[1 << collision.gameObject.layer];
                            counter++;
                        }

                        input[i * j] /= counter;
                        input[i * j] /= MasksToCheck.Length + 1;

                    } else
                        input[i * j] = maskTable[1 << collisions[0].gameObject.layer] + 1;
                }
            }
        }

        return input;
    }

    public int GetNetwortkInputSize() {
        return gridSize.x * gridSize.y;
    }

    public int GetNetworkOutputSize() {
        return outputs;
    }

    public int GetFitness() {
        return NEATNetowrk.GetFitness();
    }

    public void AddFitness(int fitness) {
        NEATNetowrk.AddFitness(fitness);
    }

    public void SetFitness(int fitness) {
        NEATNetowrk.SetFitness(fitness);
        evolver.UpdateFitnessUI(fitness);
        evolver.CheckIfBestNetwork(NEATNetowrk);
    }

    public void TrainingOver() {
        evolver.TrainingOver();
    }

    public void SetEvolver(NEATEvolver evo) {
        evolver = evo;
    }

    private void OnDrawGizmosSelected() {
        if(!Application.IsPlaying(gameObject)) {
            if(drawGizmos) {

                Gizmos.color = gizmosColor;
                Vector2 bounds = gridSize * bucketSize;
                Gizmos.DrawWireCube((Vector2)transform.position + offset, bounds);

                for(int i = 0; i < gridSize.y; i++) {
                    float currentY = i * bucketSize.y - bounds.y / 2;

                    for(int j = 0; j < gridSize.x; j++) {
                        float currentX = j * bucketSize.x - bounds.x / 2;
                        Vector2 currenPos = new Vector2(currentX + bucketSize.x / 2, currentY + bucketSize.y / 2) + (Vector2)transform.position + offset;

                        Gizmos.DrawWireCube(currenPos, bucketSize);
                    }
                }

            }
        }
    }
}
