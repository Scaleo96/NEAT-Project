using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public static class NeatEvolverData {
    public static List<NEAT> NEATpool;
    public static NEAT currentBest;
    public static List<Species> species;
    public static int currentGeneration;
    public static int bestFitness;
    public static int currentSpecies;
    public static int currenGene;
    public static int timescale;
    public static int currentBestGen;
    public static bool nextNeat;
    public static bool hasLoaded = false;

    public static int population;
    public static float speciesSepartor;
    public static float excessFactor;
    public static float disjointFactor;
    public static float weigthFactor;
    public static int timeScale;
    public static int maxTraningSteps;
    public static float staleSpeciesCounter;

    public static float pointMutateRate;
    public static float pointMutateAmount;
    public static float linkMutateRate;
    public static float nodeMutateRate;
    public static float disableEnableMutateRate;

    public static bool saveEachGen;
}

public class Species {
    public List<NEAT> networks;
    public int speciesRank;
    public int speciesTopFitness;
    public int lastSpeciesTopFitness;
    public int staleCounter;

    public Species() {
        speciesTopFitness = 0;
        lastSpeciesTopFitness = 0;
        staleCounter = 0;
        speciesRank = 0;
    }
}

public class NEATEvolver : MonoBehaviour {

    [Header("References")]
    public NEATDrawer uiDrawer;
    [SerializeField] private NEATDataUI uiData;
    [SerializeField] private NEATAgent currentAgent;

    [Header("General settings")]
    [SerializeField] private int population;
    [SerializeField] private float speciesSepartor;
    [SerializeField] private float excessFactor;
    [SerializeField] private float disjointFactor;
    [SerializeField] private float weigthFactor;
    [SerializeField, Range(1, 100)] private int timeScale;
    [Tooltip("steps the traning loop will wait untill it restes, 0 == unlimited traning time"),
    SerializeField]
    private int maxTraningSteps;
    [SerializeField] private int staleSpeciesCounter;
    [SerializeField] private int minimalChampionPopulation;
    [SerializeField] private int popDivider;

    [Header("Mutation settings")]
    [Tooltip("its not x% as the other values, to get the chance factor calculate the sum from 1 to value and devide by 100"), SerializeField, Range(0, 100)] private float pointMutateRate;
    [SerializeField, Range(0.1f, 2)] private float pointMutateAmount;
    [SerializeField, Range(0, 100)] private float linkMutateRate;
    [SerializeField, Range(0, 100)] private float nodeMutateRate;
    [SerializeField, Range(0, 100)] private float disableEnableMutateRate;

    [Header("Save settings")]
    [SerializeField] private bool saveEachGen;

    private int currentSteps;

    private void Awake() {
        FindAgent();
    }

    private void Start() {

        if(!currentAgent.newNetowrk) {
            gameObject.SetActive(false);
            return;
        }

        if(!NeatEvolverData.hasLoaded) {

            NeatEvolverData.NEATpool = new List<NEAT>();
            NeatEvolverData.species = new List<Species>();
            NeatEvolverData.bestFitness = 0;
            NeatEvolverData.currentGeneration = 1;
            NeatEvolverData.timescale = timeScale;

            for(int i = 0; i < population; i++) {
                NEAT newNeat = new NEAT(currentAgent.GetNetwortkInputSize(), currentAgent.GetNetworkOutputSize());
                MutateGenom(newNeat);
                NeatEvolverData.NEATpool.Add(newNeat);
            }

            NeatEvolverData.currentSpecies = 0;
            NeatEvolverData.currenGene = 0;
            NeatEvolverData.currentBest = new NEAT(NeatEvolverData.NEATpool[0]);
            NeatEvolverData.hasLoaded = true;

            AddSpecies();
        }

        uiData.UpdateGen(NeatEvolverData.currentGeneration);
        uiData.UpdateBestFit(NeatEvolverData.bestFitness);

        currentSteps = 0;
        StartCoroutine(Train());
    }

    private void FixedUpdate() {
        if(maxTraningSteps > 0) {
            if(currentSteps < maxTraningSteps) {
                currentSteps++;
            } else {
                TrainingOver();
            }
        }
    }

    private IEnumerator Train() {

        for(int i = NeatEvolverData.currentSpecies; i < NeatEvolverData.species.Count; i++) {
            NeatEvolverData.currentSpecies = i;

            for(int j = NeatEvolverData.currenGene; j < NeatEvolverData.species[i].networks.Count; j++) {
                NeatEvolverData.currenGene = j + 1;

                SetAgent(NeatEvolverData.species[i].networks[j]);
                NeatEvolverData.nextNeat = false;
                yield return new WaitUntil(() => NeatEvolverData.nextNeat == true);

                uiDrawer.ClearUI();
            }
            NeatEvolverData.currenGene = 0;
        }

        NeatEvolverData.currentSpecies = 0;

        CreateNewGeneration();
    }

    private void CreateNewGeneration() {

        if(saveEachGen)
            SaveBest();

        CullGenes();
        RankGlobal();
        RankCurrentGeneration();
        RemoveStaleSpecies();
        RankGlobal();
        CalculateAvrageSpeciesRank();
        BreedNewChilderen();

        NeatEvolverData.currentGeneration++;
        uiData.UpdateGen(NeatEvolverData.currentGeneration);
    }

    private void SaveBest() {
        currentAgent.SaveNeat();
    }

    public NEAT GetBestNetwork() {
        if(NeatEvolverData.currentBest == null) {
            int fitness = 0;
            NEAT neat = null;

            foreach(NEAT n in NeatEvolverData.NEATpool) {
                if(n.GetFitness() >= fitness) {
                    fitness = n.GetFitness();
                    neat = new NEAT(n);
                }
            }

            return neat;
        }

        return NeatEvolverData.currentBest;
    }

    public void UpdateFitnessUI(int fitness) {
        uiData.UpdateFit(fitness);
    }

    public void CheckIfBestNetwork(NEAT network) {
        if(network.GetFitness() > NeatEvolverData.bestFitness) {
            NeatEvolverData.bestFitness = network.GetFitness();
            NeatEvolverData.currentBest = new NEAT(network);
            uiData.UpdateBestFit(NeatEvolverData.bestFitness);
        }
    }

    public void TrainingOver() {
        NeatEvolverData.nextNeat = true;
        StopAllCoroutines();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void AddSpecies() {

        for(int i = 0; i < NeatEvolverData.NEATpool.Count; i++) {
            NEAT current = NeatEvolverData.NEATpool[i];

            if(current.species)
                continue;

            int speciesMatch = CompareGenomToSpecies(current);

            if(speciesMatch == -1) {
                NEAT match = CompareToGenes(current);

                if(match != null) {
                    Species s1 = new Species();
                    s1.networks = new List<NEAT> { current, match };
                    NeatEvolverData.species.Add(s1);
                    current.species = true;
                    match.species = true;
                    continue;
                }

                Species s = new Species();
                s.networks = new List<NEAT> { current };
                NeatEvolverData.species.Add(s);
                current.species = true;
                continue;
            }

            NeatEvolverData.species[speciesMatch].networks.Add(current);
            current.species = true;
        }
    }

    private void FindAgent() {
        if(currentAgent == null)
            currentAgent = FindObjectOfType<NEATAgent>();

        //beacuse fuck you (aka read the manual)
        if(currentAgent == null) {
            Debug.LogError("NEAT agent was not found, have you tried getting your shit together (aka, add a NEAT agent component on your training object)");
            foreach(var obj in FindObjectsOfType<GameObject>()) {
                if(obj != gameObject)
                    Destroy(obj);
            }

            Destroy(gameObject);
            return;
        }

        currentAgent.SetEvolver(this);
    }

    private void SetAgent(NEAT network) {
        currentAgent.SetNEAT(network);
        uiDrawer.DrawNetwork(network);
        uiData.UpdateSpec(NeatEvolverData.currentSpecies + 1);
        uiData.UpdateGenom(NeatEvolverData.currenGene);
        uiData.UpdateFit(network.GetFitness());
    }

    private void MutateGenom(NEAT network) {
        float currentRate = linkMutateRate;
        if(Random.Range(0, 100) < currentRate)
            network.LinkMutate();

        currentRate = nodeMutateRate;
        if(Random.Range(0, 100) < currentRate)
            network.NodeMutate();

        currentRate = disableEnableMutateRate;
        if(Random.Range(0, 100) < currentRate)
            network.DisableMutate();

        currentRate = pointMutateAmount;
        while(currentRate > 0) {
            if(Random.Range(0, 100) < currentRate)
                network.PointMutate(pointMutateAmount);

            currentRate--;
        }
    }

    private int CompareGenomToSpecies(NEAT network) {

        foreach(var specie in NeatEvolverData.species)
            if(CompareGenes(specie.networks[0], network) <= speciesSepartor)
                return NeatEvolverData.species.FindIndex(x => x == specie);

        return -1;
    }

    private NEAT CompareToGenes(NEAT network) {

        for(int i = 0; i < NeatEvolverData.NEATpool.Count; i++)
            if(!NeatEvolverData.NEATpool[i].species && NeatEvolverData.NEATpool[i] != network)
                if(CompareGenes(network, NeatEvolverData.NEATpool[i]) <= speciesSepartor)
                    return NeatEvolverData.NEATpool[i];

        return null;
    }


    private float CompareGenes(NEAT networkA, NEAT networkB) {

        if(networkA.GetConnetionGenom().Count == 0 && networkB.GetConnetionGenom().Count == 0)
            return 0;

        List<Connection> connectionsA = networkA.GetConnetionGenom();
        List<Connection> connectionsB = networkB.GetConnetionGenom();

        connectionsA.Sort(delegate (Connection a, Connection b) {
            return a.innovation.CompareTo(b.innovation);
        });

        connectionsB.Sort(delegate (Connection a, Connection b) {
            return a.innovation.CompareTo(b.innovation);
        });

        int diff = 0;
        int counterMax = 0;

        #region
        if(connectionsA.Count == 0 && connectionsB.Count != 0) {
            diff = connectionsB.Count;
            counterMax = connectionsB[connectionsB.Count - 1].innovation;
        } else if(connectionsB.Count == 0 && connectionsA.Count != 0) {
            diff = connectionsA.Count;
            counterMax = connectionsA[connectionsA.Count - 1].innovation;
        } else if(connectionsA.Count > 0 && connectionsB.Count > 0) {
            diff = connectionsA[connectionsA.Count - 1].innovation - connectionsB[connectionsB.Count - 1].innovation;
            counterMax = diff < 0 ? connectionsA[connectionsA.Count - 1].innovation : connectionsB[connectionsB.Count - 1].innovation;
        }

        #endregion 

        int[] result = new int[counterMax];

        for(int i = 1; i < counterMax; i++) {
            Connection A = connectionsA.Find(x => x.innovation == i);
            Connection B = connectionsB.Find(x => x.innovation == i);
            if(A != null && B != null) {
                result[i] = 1;
                continue;
            }
            result[i] = 0;
        }

        int disjoints = 0;
        foreach(int i in result)
            if(i == 0)
                disjoints++;

        float weigthSum = 0;
        counterMax = diff > 0 ? connectionsA.Count : connectionsB.Count;
        for(int i = 0; i < counterMax; i++) {

            float w1 = 0;
            float w2 = 0;

            if(i < connectionsA.Count) {
                w1 = connectionsA[i].weight;
            }

            if(i < connectionsB.Count) {
                w2 = connectionsB[i].weight;
            }

            weigthSum += Mathf.Abs(w1 - w1);
        }

        weigthSum /= counterMax;

        diff = Mathf.Abs(diff);

        int genomSumA = connectionsA.Count + networkA.GetNodeGenom().Count;
        int genomSumB = connectionsB.Count + networkB.GetNodeGenom().Count;

        if(genomSumA > genomSumB)
            return ((excessFactor * diff) / genomSumA) + ((disjointFactor * disjoints) / genomSumA) + (weigthFactor * weigthSum);

        return ((excessFactor * diff) / genomSumB) + ((disjointFactor * disjoints) / genomSumB) + (weigthFactor * weigthSum);
    }

    private void CullGenes() {
        foreach(var species in NeatEvolverData.species) {

            species.networks.Sort(delegate (NEAT a, NEAT b) {
                return b.GetFitness().CompareTo(a.GetFitness());
            });

            List<NEAT> removeList = new List<NEAT>();
            int size = Mathf.CeilToInt(species.networks.Count / popDivider);
            if(size > 0) {
                for(int i = size; i < species.networks.Count; i++)
                    removeList.Add(species.networks[i]);

                foreach(var neat in removeList) {
                    species.networks.Remove(neat);
                    NeatEvolverData.NEATpool.Remove(neat);
                }
            }
        }
    }

    private void RankGlobal() {
        List<NEAT> copyPool = new List<NEAT>(NeatEvolverData.NEATpool);

        copyPool.Sort(delegate (NEAT a, NEAT b) {
            return a.GetFitness().CompareTo(b.GetFitness());
        });

        for(int i = 0; i < copyPool.Count; i++)
            copyPool[i].SetGlobalRank(i);

    }

    private void RankCurrentGeneration() {
        int current = 0;
        foreach(var spice in NeatEvolverData.species)
            foreach(var neat in spice.networks)
                if(neat.GetFitness() > current)
                    current = neat.GetFitness();

        NeatEvolverData.currentBestGen = current;
    }

    private void RemoveStaleSpecies() {
        List<Species> weakSpecies = new List<Species>();

        for(int i = 0; i < NeatEvolverData.species.Count; i++) {
            int topFitness = 0;
            foreach(NEAT n in NeatEvolverData.species[i].networks)
                if(topFitness < n.GetFitness()) {
                    topFitness = n.GetFitness();
                    if(NeatEvolverData.species[i].speciesTopFitness < topFitness) {
                        NeatEvolverData.species[i].speciesTopFitness = topFitness;
                        NeatEvolverData.species[i].staleCounter = 0;
                    }
                }

            if(topFitness <= NeatEvolverData.species[i].lastSpeciesTopFitness)
                NeatEvolverData.species[i].staleCounter++;

            if(NeatEvolverData.species[i].staleCounter >= staleSpeciesCounter && topFitness < NeatEvolverData.bestFitness && topFitness < NeatEvolverData.currentBestGen)
                weakSpecies.Add(NeatEvolverData.species[i]);

            NeatEvolverData.species[i].lastSpeciesTopFitness = NeatEvolverData.species[i].speciesTopFitness;
        }

        foreach(Species species in weakSpecies) {
            foreach(NEAT neat in species.networks)
                NeatEvolverData.NEATpool.Remove(neat);
            NeatEvolverData.species.Remove(species);
        }
    }

    private void CalculateAvrageSpeciesRank() {
        foreach(var species in NeatEvolverData.species) {
            int totalRank = 0;
            foreach(var genom in species.networks) {
                totalRank += genom.GetGlobalRank();
            }
            species.speciesRank = totalRank / species.networks.Count;
        }
    }

    private void BreedNewChilderen() {
        int sum = CalculateTotalSpeciesRank();
        List<NEAT> children = new List<NEAT>();
        List<NEAT> champions = new List<NEAT>();

        foreach(var species in NeatEvolverData.species)
            if(species.networks.Count >= minimalChampionPopulation)
                champions.Add(species.networks[0]);

        int popDiff = population - champions.Count;

        //evolve and breed children up to population size
        foreach(var species in NeatEvolverData.species) {
            int breedAmount = Mathf.FloorToInt((species.speciesRank / sum) * popDiff);
            for(int i = 0; i < breedAmount; i++) {
                if(children.Count + champions.Count >= population)
                    break;

                children.Add(BreedChild(NeatEvolverData.species.FindIndex(x => x == species)));
            }
        }

        while(children.Count + champions.Count < population)
            children.Add(BreedChild(Random.Range(0, NeatEvolverData.species.Count)));

        //find all networks that are not the chapion
        List<NEAT> removeList = new List<NEAT>();
        foreach(var species in NeatEvolverData.species) {

            List<NEAT> innerRemoveList = new List<NEAT>();
            foreach(var network in species.networks) {
                if(!champions.Contains(network)) {
                    innerRemoveList.Add(network);
                    removeList.Add(network);
                }
            }

            foreach(var innerNetwork in innerRemoveList)
                species.networks.Remove(innerNetwork);
        }


        //remove all networkes exept best one, aka the chapion
        foreach(var network in removeList)
            NeatEvolverData.NEATpool.Remove(network);

        //remove empty species
        List<Species> removeSpecis = new List<Species>();
        foreach(var species in NeatEvolverData.species)
            if(species.networks.Count <= 0)
                removeSpecis.Add(species);

        foreach(var species in removeSpecis)
            NeatEvolverData.species.Remove(species);

        //add all children back to puplation
        for(int i = 0; i < children.Count; i++) {
            MutateGenom(children[i]);
            NeatEvolverData.NEATpool.Add(children[i]);
            children[i].species = false;
        }

        AddSpecies();
    }

    private int CalculateTotalSpeciesRank() {
        int total = 0;
        foreach(var species in NeatEvolverData.species)
            total += species.speciesRank;

        return total;
    }

    private NEAT BreedChild(int speciesIndex) {

        if(NeatEvolverData.species[speciesIndex].networks.Count <= 1)
            return new NEAT(NeatEvolverData.species[speciesIndex].networks[0]);

        int rand1 = Random.Range(0, NeatEvolverData.species[speciesIndex].networks.Count);
        int rand2 = Random.Range(0, NeatEvolverData.species[speciesIndex].networks.Count);

        while(rand1 == rand2 && NeatEvolverData.species[speciesIndex].networks.Count > 1)
            rand2 = Random.Range(0, NeatEvolverData.species[speciesIndex].networks.Count);

        NEAT parentA = NeatEvolverData.species[speciesIndex].networks[rand1];
        NEAT parentB = NeatEvolverData.species[speciesIndex].networks[rand2];

        if(parentA.GetFitness() >= parentB.GetFitness())
            return Crossover(parentA, parentB);

        return Crossover(parentB, parentA);
    }

    private NEAT Crossover(NEAT dominant, NEAT submissive) {
        NEAT child = new NEAT(dominant);

        List<Connection> newConnections = new List<Connection>();

        foreach(var connection in child.GetConnetionGenom()) {
            Connection c = submissive.GetConnetionGenom().Find(x => x.innovation == connection.innovation);

            if(c != null && Random.Range(0f, 1f) > 0.5f) {
                Node inNode = child.GetNodeGenom().Find(n => n.nodeID == c.inNode);
                Node outNode = child.GetNodeGenom().Find(n => n.nodeID == c.outNode);

                if(outNode == null && inNode != null) {
                    outNode = new Node(c.outNode, inNode.order + 1, NodeType.Hidden);
                    child.AddNode(outNode);

                } else if(outNode == null && inNode == null) {
                    outNode = new Node(c.outNode, 1, NodeType.Hidden);
                    child.AddNode(outNode);
                }

                if(inNode == null) {
                    inNode = new Node(c.inNode, 0, NodeType.Hidden);
                    child.AddNode(inNode);
                }

                Connection cNew = new Connection(inNode.nodeID, outNode.nodeID, c.weight, c.enabled, c.innovation);

                newConnections.Add(cNew);
            }
        }

        foreach(var connection in newConnections) {
            if(!CompareConnection(connection, child)) {
                if(!LoopSearch(connection, child.GetConnetionGenom())) {
                    child.RemoveConnection(child.GetConnetionGenom().Find(n => n.innovation == connection.innovation));
                    child.AddConnection(connection);
                    child.GetNodeGenom().Find(n => n.nodeID == connection.inNode).AddConnection(connection);
                }
            }
        }
        return child;
    }

    private bool CompareConnection(Connection connection, NEAT child) {
        int size = child.GetConnetionGenom().Count;
        for(int i = 0; i < size; i++)
            if(child.GetConnectionGene(i).CompareConnection(connection.inNode, connection.outNode))
                return true;

        return false;
    }

    private bool LoopSearch(Connection start, List<Connection> connections) {

        List<int> openSet = new List<int>();
        List<int> closeSet = new List<int>();

        openSet.Add(start.outNode);
        int currenNode = openSet[0];

        while(openSet.Count > 0) {
            currenNode = openSet[0];
            openSet.Remove(currenNode);

            if(currenNode == start.inNode)
                return true;

            foreach(var connetion in connections)
                if(connetion.inNode == currenNode && !openSet.Contains(connetion.outNode) && !closeSet.Contains(connetion.outNode))
                    openSet.Add(connetion.outNode);

            closeSet.Add(currenNode);
        }
        return false;
    }

    private void OnValidate() {
        Time.timeScale = timeScale;
    }
}
