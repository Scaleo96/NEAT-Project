using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;



[System.Serializable, CreateAssetMenu(fileName = "Brain", menuName = "Brain/New Brain", order = 1)]
public class Brain : ScriptableObject {

    //[Header("No touchy touchy")]
    //[SerializeField] private float[] floatNodes;
    //[SerializeField] private float[] floatConnections;
    //[SerializeField] private int innovation;
    //[SerializeField] private int inputs;
    //[SerializeField] private int outputs;

    //[Header("touch and it will brake")]
    //[SerializeField] public int[] gridSize;
    //[SerializeField] public int[] bucketSize;
    //[SerializeField] public int[] offset;

    //each node
    //int order { get; private set; }
    //int nodeID;
    //NodeType nodeType { get; private set; }
    //float activation;
    //float sum;
    //List<Connection> connections { get; private set; }

    //each connections
    //    public int inNode;
    //    public int outNode;
    //    public float weight;
    //    public bool enabled;
    //    public int innovation { get; private set; }

    public SaveBrain GetNEAT() {
        SaveBrain saveBrain;

        // network.SetSize(inputs, outputs);

        //    //create all nodes
        //    for(int i = 0; i < floatNodes.Length; i += 5) {
        //        Node current = new Node((int)floatNodes[i + 1], (int)floatNodes[i], (NodeType)(int)floatNodes[i + 2]);
        //        current.activation = floatNodes[i + 3];
        //        current.sum = floatNodes[i + 4];
        //        network.AddNode(current);
        //    }

        //    //create all connections
        //    for(int i = 0; i < floatConnections.Length; i += 5) {
        //        bool enable = floatConnections[i + 3] == 1 ? true : false;
        //        Connection current = new Connection((int)floatConnections[i], (int)floatConnections[i + 1], floatConnections[i + 2], enable, (int)floatConnections[i + 4]);
        //        Debug.Log(current.inNode + "Innode id, " + current.outNode + "outnode id, " + current.weight + "weight, " + current.innovation + "innovation");
        //        network.AddConnection(current);
        //    }

        //    //add all connections to nodes
        //    foreach(var connection in network.GetConnetionGenom()) {
        //        Node currentNode = network.GetNodeGenom().Find(x => x.nodeID == connection.inNode);
        //        if(currentNode != null) {
        //            currentNode.AddConnection(connection);
        //        }
        //    }

        //    //foreach(var node in nodeGenom) {
        //    //    network.AddNode(new Node(node.nodeID, node.order, node.nodeType));
        //    //    Debug.Log("node type " + node.nodeType);
        //    //}

        //    //foreach(var c in connectionGenom) {
        //    //    Node inNode = network.GetNodeGenom().Find(n => n.nodeID == c.inNode);
        //    //    Node outNode = network.GetNodeGenom().Find(n => n.nodeID == c.outNode);
        //    //    Connection newC = new Connection(inNode.nodeID, outNode.nodeID, c.weight, c.enabled, c.innovation);
        //    //    network.AddConnection(newC);
        //    //    inNode.AddConnection(newC);
        //    //}

        //    network.SetInnovation(innovation);

        if(File.Exists(Application.persistentDataPath + "/" + name + ".save")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + name + ".save", FileMode.Open);
            SaveBrain save = (SaveBrain)bf.Deserialize(file);
            file.Close();

            saveBrain = save;

            Debug.Log("Game Loaded");

        } else {
            Debug.Log("No game saved!");
            saveBrain = null;
        }


       return saveBrain;
    }

    public void SetNEAT(NEAT neat, Vector2Int gridSize, Vector2 bucketSize, Vector2 offset) {

        //this.gridSize = new int[2];
        //this.gridSize[0] = gridSize.x;
        //this.gridSize[1] = gridSize.y;

        //this.bucketSize = new int[2];
        //this.bucketSize[0] = bucketSize.x;
        //this.bucketSize[1] = bucketSize.y;

        //this.offset = new int[2];
        //this.offset[0] = offset.x;
        //this.offset[1] = offset.y;

        //innovation = neat.GetInnovation();
        //inputs = neat.GetInputSize();
        //outputs = neat.GetOutputsSize();

        //List<float> tempFloatsNodes = new List<float>();
        //List<float> tempFloatsConnections = new List<float>();

        //foreach(var node in neat.GetNodeGenom()) {

        //    //node vars
        //    tempFloatsNodes.Add(node.order);
        //    tempFloatsNodes.Add(node.nodeID);
        //    tempFloatsNodes.Add((int)node.nodeType);
        //    tempFloatsNodes.Add(node.activation);
        //    tempFloatsNodes.Add(node.sum);

        //    //nodeGenom.Add(new Node(node.nodeID, node.order, (NodeType)(int)node.nodeType));
        //    //Debug.Log("node type " + node.nodeType);
        //}

        //foreach(var connection in neat.GetConnetionGenom()) {

        //    //connections vars

        //    tempFloatsConnections.Add(connection.inNode);
        //    tempFloatsConnections.Add(connection.outNode);
        //    tempFloatsConnections.Add(connection.weight);

        //    if(connection.enabled)
        //        tempFloatsConnections.Add(1);
        //    else
        //        tempFloatsConnections.Add(0);

        //    tempFloatsConnections.Add(connection.innovation);

        //    //Node inNode = nodeGenom.Find(n => n.nodeID == connection.inNode);
        //    //Node outNode = nodeGenom.Find(n => n.nodeID == connection.outNode);
        //    //Connection newC = new Connection(inNode.nodeID, outNode.nodeID, connection.weight, connection.enabled, connection.innovation);
        //    //connectionGenom.Add(newC);
        //    //inNode.AddConnection(newC);
        //}

        //floatNodes = tempFloatsNodes.ToArray();
        //floatConnections = tempFloatsConnections.ToArray();

        SaveBrain save = new SaveBrain(neat, gridSize, bucketSize, offset);

        // 2
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/" + name + ".save");
        bf.Serialize(file, save);
        file.Close();

        Debug.Log("Game Saved");
    }
}
