using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveBrain{
    [SerializeField]
    private float[] floatNodes;
    [SerializeField]
    private float[] floatConnections;
    [SerializeField]
    private int innovation;
    [SerializeField]
    private int inputs;
    [SerializeField]
    private int outputs;
    [SerializeField]
    public int[] gridSize;
    [SerializeField]
    public float[] bucketSize;
    [SerializeField]
    public float[] offset;

    public SaveBrain(NEAT neat, Vector2Int gridSize, Vector2 bucketSize, Vector2 offset) {
        this.gridSize = new int[2];
        this.gridSize[0] = gridSize.x;
        this.gridSize[1] = gridSize.y;

        this.bucketSize = new float[2];
        this.bucketSize[0] = bucketSize.x;
        this.bucketSize[1] = bucketSize.y;

        this.offset = new float[2];
        this.offset[0] = offset.x;
        this.offset[1] = offset.y;

        innovation = neat.GetInnovation();
        inputs = neat.GetInputSize();
        outputs = neat.GetOutputsSize();

        List<float> tempFloatsNodes = new List<float>();
        List<float> tempFloatsConnections = new List<float>();

        foreach(var node in neat.GetNodeGenom()) {

            //node vars
            tempFloatsNodes.Add(node.order);
            tempFloatsNodes.Add(node.nodeID);
            tempFloatsNodes.Add((int)node.nodeType);
            tempFloatsNodes.Add(node.activation);
            tempFloatsNodes.Add(node.sum);
        }

        foreach(var connection in neat.GetConnetionGenom()) {

            //connections vars

            tempFloatsConnections.Add(connection.inNode);
            tempFloatsConnections.Add(connection.outNode);
            tempFloatsConnections.Add(connection.weight);

            if(connection.enabled)
                tempFloatsConnections.Add(1);
            else
                tempFloatsConnections.Add(0);

            tempFloatsConnections.Add(connection.innovation);
        }

        floatNodes = tempFloatsNodes.ToArray();
        floatConnections = tempFloatsConnections.ToArray();
    }

    public NEAT LoadBrain() {
        NEAT network = new NEAT(0, 0);
        network.SetSize(inputs, outputs);

        //create all nodes
        for(int i = 0; i < floatNodes.Length; i += 5) {
            Node current = new Node((int)floatNodes[i + 1], (int)floatNodes[i], (NodeType)(int)floatNodes[i + 2]);
            current.activation = floatNodes[i + 3];
            current.sum = floatNodes[i + 4];
            network.AddNode(current);
        }

        //create all connections
        for(int i = 0; i < floatConnections.Length; i += 5) {
            bool enable = floatConnections[i + 3] == 1 ? true : false;
            Connection current = new Connection((int)floatConnections[i], (int)floatConnections[i + 1], floatConnections[i + 2], enable, (int)floatConnections[i + 4]);
            Debug.Log(current.inNode + "Innode id, " + current.outNode + "outnode id, " + current.weight + "weight, " + current.innovation + "innovation");
            network.AddConnection(current);
        }

        //add all connections to nodes
        foreach(var connection in network.GetConnetionGenom()) {
            Node currentNode = network.GetNodeGenom().Find(x => x.nodeID == connection.inNode);
            if(currentNode != null) {
                currentNode.AddConnection(connection);
            }
        }

        network.SetInnovation(innovation);

        return network;
    }

}
