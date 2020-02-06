using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum NodeType { Input, Output, Hidden }

[System.Serializable]
public class NEAT {

    public bool species;

    private Genom genom;
    private int inputLength;
    private int outputsLength;

    public NEAT(int inputs, int outputs) {
        genom = new Genom(inputs, outputs);
        inputLength = inputs;
        outputsLength = outputs;
        species = false;
    }

    public NEAT(NEAT network) {
        genom = new Genom(network.genom);
        inputLength = network.inputLength;
        outputsLength = network.outputsLength;
        species = network.species;
    }

    public void PointMutate(float randomStepAmount) {
        //randomly updates a connections weight
        if(genom.GetConnectionGenesLength() <= 0)
            return;

        int ranIndex = Random.Range(0, genom.GetConnectionGenesLength() - 1);
        genom.ChangeConnectionGeneWeight(ranIndex, Random.Range(-randomStepAmount, randomStepAmount));
    }

    public string LinkMutate() {
        return genom.LinkMutate();
    }

    public void NodeMutate() {
        //adds a new node by disableing a connection. The new node will be connected to the to both the old nodes, there will be a new connection
        //from the old input node and then the new node will copy the old connection and add it as its own connection to the old output node.

        if(genom.GetConnectionGenesLength() <= 0)
            return;

        int index = Random.Range(0, genom.GetConnectionGenesLength() - 1);
        Connection connectionToReplace = genom.GetConnectionGene(index);

        Node oldInput = genom.GetNodeGenom().Find(n => n.nodeID == connectionToReplace.inNode);
        Node oldOutput = genom.GetNodeGenom().Find(n => n.nodeID == connectionToReplace.outNode);

        Node newNode = new Node(genom.counter, oldOutput.order, NodeType.Hidden);

        Connection inNew = new Connection(newNode.nodeID, oldOutput.nodeID, connectionToReplace.weight, true, genom.currentInnovation);
        newNode.AddConnection(inNew);
        genom.AddConnectionGene(inNew);

        Connection inNewOther = new Connection(oldInput.nodeID, newNode.nodeID, 1, true, genom.currentInnovation);

        if(oldInput.nodeType != NodeType.Input)
            oldInput.AddConnection(inNewOther);

        genom.AddConnectionGene(inNewOther);

        genom.AddNodeGene(newNode);
        genom.ChangeConnectionGneneState(index, false);

        List<Node> nodes = genom.GetNodeGenom();
        newNode.AddOrder(ref nodes);
    }

    public void DisableMutate() {
        if(genom.GetConnectionGenesLength() <= 0)
            return;

        int index = Random.Range(0, genom.GetConnectionGenesLength() - 1);
        genom.ChangeConnectionGneneState(index, !genom.GetConnectionGene(index).enabled);
    }

    public float[] SendInputs(float[] inputs) {

        if(inputs.Length == inputLength)
            return genom.FeedForward(inputs, outputsLength);

        Debug.Log("inputs.len: " + inputs.Length + ", real len: " + inputLength);

        return null;
    }

    public List<Node> GetNodeGenom() {
        return genom.GetNodeGenom();
    }

    public List<Connection> GetConnetionGenom() {
        return genom.GetConnetionGenom();
    }

    public void DenugNEAT() {
        genom.DebugNetworkConnetion();
    }

    public int GetInnovation() {
        return genom.currentInnovation;
    }

    public int GetFitness() {
        return genom.genomFitness;
    }

    public void AddFitness(int fitness) {
        genom.genomFitness += fitness;
    }

    public void SetFitness(int fitness) {
        genom.genomFitness = fitness;
    }

    public int GetGlobalRank() {
        return genom.globalRank;
    }

    public void SetGlobalRank(int rank) {
        genom.globalRank = rank;
    }

    public void AddNode(Node node) {
        genom.AddNodeGene(node);
    }

    public void AddConnection(Connection c) {
        genom.AddConnectionGene(c);
    }

    public void RemoveConnection(Connection c) {
        genom.RemoveConnectionGene(c);
    }

    public int GetInputSize() {
        return inputLength;
    }

    public int GetOutputsSize() {
        return outputsLength;
    }

    public void SetInnovation(int innovation) {
        genom.currentInnovation = innovation;
    }

    public void SetSize(int inputs, int outputs) {
        inputLength = inputs;
        outputsLength = outputs;
        genom.inputLength = inputLength;
    }

    public Connection GetConnectionGene(int i) {
        return genom.GetConnectionGene(i);
    }

}

[System.Serializable]
public class Genom {

    private List<Node> nodeGenes;
    private List<Connection> connectionGenes;
    public int inputLength;
    public int currentInnovation;
    public int genomFitness;
    public int globalRank;
    public int counter;

    public Genom(int inputCount, int outputCount) {
        nodeGenes = new List<Node>();
        connectionGenes = new List<Connection>();
        inputLength = inputCount;
        currentInnovation = 1;
        genomFitness = 0;
        counter = 0;

        for(int i = 0; i < inputCount; i++)
            AddNodeGene(new Node(counter, 0, NodeType.Input));

        for(int i = 0; i < outputCount; i++) 
            AddNodeGene(new Node(counter, 1, NodeType.Output));     
    }

    public Genom(Genom genom) {
        nodeGenes = new List<Node>();
        foreach(Node n in genom.nodeGenes) {
            Node newNode = new Node(n.nodeID, n.order, n.nodeType);
            nodeGenes.Add(newNode);
        }

        connectionGenes = new List<Connection>();
        foreach(Connection c in genom.connectionGenes) {
            Connection newC = new Connection(c.inNode, c.outNode, c.weight, c.enabled, c.innovation);
            nodeGenes.Find(n => n.nodeID == newC.inNode).AddConnection(newC);
            connectionGenes.Add(newC);
        }

        inputLength = genom.inputLength;
        currentInnovation = genom.currentInnovation;
        genomFitness = genom.genomFitness;
        counter = genom.counter;
    }

    public string LinkMutate() {
        //connect two new nodes if the connection dosent exsist already

        int randomNode = Random.Range(0, inputLength);
        int randomNode2 = Random.Range(inputLength, nodeGenes.Count);

        while(GetNodeGene(randomNode).nodeType == NodeType.Output)
            randomNode = Random.Range(0, inputLength - 1);

        while(randomNode == randomNode2 || GetNodeGene(randomNode2).nodeType == NodeType.Input)
            randomNode2 = Random.Range(inputLength - 1, nodeGenes.Count);

        Node input = GetNodeGene(randomNode);
        Node output = GetNodeGene(randomNode2);

        int size = connectionGenes.Count;
        for(int i = 0; i < size; i++)
            if(GetConnectionGene(i).CompareConnection(input.nodeID, output.nodeID))
                return "Connection already exsists";

        Connection newConnection = new Connection(input.nodeID, output.nodeID, Random.Range(-2f, 2f), true, currentInnovation);

        if(LoopSearch(newConnection))
            return "loop found connection removed";

        AddConnectionGene(newConnection);
        input.AddConnection(newConnection);
        input.AddOrder(ref nodeGenes);

        return "Succsessfull conncetions";
    }

    public void AddNodeGene(Node node) {
        Node n = nodeGenes.Find(x => x.nodeID == counter);
        while(n != null) {
            counter++;
            n = nodeGenes.Find(x => x.nodeID == counter);
        }

        node.nodeID = counter;
        nodeGenes.Add(node);
        counter++;
    }

    public Node GetNodeGene(int index) {
        if(index < nodeGenes.Count)
            return nodeGenes[index];

        return null;
    }

    public void RemoveNodeGene(Node node) {
        nodeGenes.Remove(node);
    }

    public void RemoveNodeGene(int index) {
        if(index < nodeGenes.Count)
            nodeGenes.RemoveAt(index);
    }

    public int GetNodeGenesLength() {
        return nodeGenes.Count;
    }

    public void AddConnectionGene(Connection connection) {
        connectionGenes.Add(connection);
        currentInnovation++;
    }

    public Connection GetConnectionGene(int index) {;
        if(index < connectionGenes.Count) 
            return connectionGenes[index];

        return null;
    }

    public void ChangeConnectionGeneWeight(int index, float weight) {
        connectionGenes[index].weight += weight;
    }

    public void ChangeConnectionGneneState(int index, bool state) {
        connectionGenes[index].enabled = state;
    }

    public void RemoveConnectionGene(Connection connection) {
        Node inNode = nodeGenes.Find(n => n.nodeID == connection.inNode);
        inNode.RemoveConnetion(connection);
        connectionGenes.Remove(connection);
    }

    public void RemoveConnectionGene(int index) {
        if(index < connectionGenes.Count)
            connectionGenes.RemoveAt(index);
    }

    public int GetConnectionGenesLength() {
        return connectionGenes.Count;
    }

    public List<Node> GetNodeGenom() {
        return new List<Node>(nodeGenes);
    }

    public List<Connection> GetConnetionGenom() {
        return new List<Connection>(connectionGenes);
    }

    public void DebugNetworkConnetion() {

        string output = "";
        foreach(var node in nodeGenes)
            output += "id: " + node.nodeID + ", order: " + node.order + ", type: " + node.nodeType + ", activation: " + node.activation + ", sum: " + node.sum + " \n";

        Debug.Log("nodes: " + output);

        output = "";
        foreach(var connection in connectionGenes)
            output += "in node id: " + connection.inNode + ", out node id: " + connection.outNode + ", weigth: " + connection.weight + ", innovation: " + connection.innovation + ", enable: " + connection.enabled + " \n";

        Debug.Log("nodes: " + output);

    }

    public float[] FeedForward(float[] input, int outputCount) {

        Dictionary<int, List<Node>> hashTable = new Dictionary<int, List<Node>>();

        for(int i = 0; i < nodeGenes.Count; i++) {
            if(i < input.Length)
                nodeGenes[i].activation = input[i];
            else
                nodeGenes[i].sum = 0;

            if(!hashTable.ContainsKey(nodeGenes[i].order))
                hashTable.Add(nodeGenes[i].order, new List<Node>());

            hashTable[nodeGenes[i].order].Add(nodeGenes[i]);       
        }

        foreach(var key in hashTable.Keys) {
            foreach(var node in hashTable[key]) {
                if(key > 0)
                    node.activation = (float)System.Math.Tanh(node.sum);

                foreach(var connection in node.connections)
                    if(connection.enabled)
                        nodeGenes.Find(n => n.nodeID == connection.outNode).sum += node.activation * connection.weight;
            }
        }

        float[] output = new float[outputCount];

        for(int i = input.Length; i < input.Length + outputCount; i++) {
            output[i - input.Length] = nodeGenes[i].activation;
        }

        return output;
    }

    private bool LoopSearch(Connection start) {

        List<int> openSet = new List<int>();
        List<int> closeSet = new List<int>();

        openSet.Add(start.outNode);
        int currenNode = openSet[0];

        while(openSet.Count > 0) {
            currenNode = openSet[0];
            openSet.Remove(currenNode);
            

            if(currenNode == start.inNode)
                return true;

            foreach(var connetion in connectionGenes)
                if(connetion.inNode == currenNode && !openSet.Contains(connetion.outNode) && !closeSet.Contains(connetion.outNode))
                    openSet.Add(connetion.outNode);

            closeSet.Add(currenNode);
        }
        return false;
    }
}

[System.Serializable]
public class Node {

    public int order { get; private set; }
    public int nodeID;
    public NodeType nodeType { get; private set; }
    public float activation;
    public float sum;

    public List<Connection> connections { get; private set; }

    public Node(int id, int order, NodeType type) {
        nodeID = id;
        this.order = order;
        nodeType = type;
        connections = new List<Connection>();
    }

    public void AddConnection(Connection connection) {
        if(!connections.Contains(connection))
            connections.Add(connection);
    }

    public void RemoveConnetion(int index) {
        if(index < connections.Count)
            connections.RemoveAt(index);
    }

    public void RemoveConnetion(Connection connection) {
        connections.Remove(connection);
    }

    public void AddOrder(ref List<Node> nodes) {
        foreach(var connection in connections) {
            Node node = nodes.Find(n => n.nodeID == connection.outNode);

            if(node != null) {
                node.order++;
                node.AddOrder(ref nodes);
            }
        }
    }
}

[System.Serializable]
public class Connection {
    public int inNode;
    public int outNode;
    public float weight;
    public bool enabled;
    public int innovation { get; private set; }

    public Connection(int input, int Output, float weight, bool enabled, int innovation ) {
        inNode = input;
        outNode = Output;
        this.weight = weight;
        this.enabled = enabled;
        this.innovation = innovation;
    }

    public void SetConnection(int input, int Output, float weight, bool enabled) {
        inNode = input;
        outNode = Output;
        this.weight = weight;
        this.enabled = enabled;
    }

    public bool CompareConnection(int input, int output) {
        if(input == inNode && output == outNode)
            return true;

        return false;
    }
}
