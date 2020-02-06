using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NEATDrawer : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Camera cam;

    [Header("Line color settings")]
    [SerializeField] private Color EnableLineColor;
    [SerializeField] private Color DisableLineColor;

    private bool draw;
    private RectTransform thisRect;
    private Vector2 panelBounds;
    private Vector2 bounds;
    private Vector3[] imagesPoses;
    private Dictionary<LineRenderer, Connection> linePositions;
    private Dictionary<int, RectTransform> nodeToTrans;

    void Awake() {
        thisRect = GetComponent<RectTransform>();
        bounds = nodePrefab.GetComponent<RectTransform>().rect.size;
        panelBounds = thisRect.rect.size;
        linePositions = new Dictionary<LineRenderer, Connection>();
        draw = false;
    }

    void LateUpdate() {
        if(draw) {
            foreach(var index in linePositions) {
                Vector3[] poses = new Vector3[2];
                poses[0] = cam.ScreenToWorldPoint(nodeToTrans[index.Value.inNode].position + new Vector3(bounds.x * 0.5f, -bounds.y * 0.5f, 0));       
                poses[1] = cam.ScreenToWorldPoint(nodeToTrans[index.Value.outNode].position + new Vector3(bounds.x * 0.5f, -bounds.y * 0.5f, 0));

                poses[0].z = 0;
                poses[1].z = 0;
                index.Key.SetPositions(poses);
            }
        }
    }

    public void DrawNetwork(NEAT network) {
        List<Node> nodes = network.GetNodeGenom();
        List<Connection> connections = network.GetConnetionGenom();

        Dictionary<int, List<Node>> table = new Dictionary<int, List<Node>>();
        linePositions = new Dictionary<LineRenderer, Connection>();
        nodeToTrans = new Dictionary<int, RectTransform>();

        int lastOrder = CalculateLargestOrder(nodes);

        for(int i = 0; i <= lastOrder; i++)
            table.Add(i, new List<Node>());

        table.Add(lastOrder + 1, new List<Node>());

        foreach(var node in nodes) {
            if(node.nodeType != NodeType.Output)
                table[node.order].Add(node);
            else
                table[lastOrder + 1].Add(node);
        }

        for(int i = 0; i < table.Count; i++) {
            table[i].Sort(delegate (Node a, Node b) {
                return a.nodeID.CompareTo(b.nodeID);
            });
        }

        for(int i = 0; i <= lastOrder + 1; i++) {
            for(int j = 0; j < table[i].Count; j++) {
                CreateNode(new Vector2(i * ((panelBounds.x - bounds.x * 2)/(lastOrder + 1)), j * ((panelBounds.y - bounds.y * 1.5f)/table[i].Count)), table[i][j]);
            }
        }

        foreach(var connection in connections) {
            Vector3[] poses = new Vector3[2];
            poses[0] = cam.ScreenToWorldPoint(nodeToTrans[connection.inNode].anchoredPosition);
            poses[1] = cam.ScreenToWorldPoint(nodeToTrans[connection.inNode].anchoredPosition);
            CreateConnection(poses, connection);
        }

        draw = true;

    }

    public void ClearUI() {
        foreach(Transform t in transform)
            if(t != transform)
                Destroy(t.gameObject);

        draw = false;
    }

    private int CalculateLargestOrder(List<Node> nodes) {
        int largest = 0;

        foreach(var node in nodes)
            if(node.order > largest)
                largest = node.order;

        return largest;
    }

    private void CreateNode(Vector2 position, Node n) {
        GameObject newRect = Instantiate(nodePrefab, transform);
        RectTransform t = newRect.GetComponent<RectTransform>();
        t.anchoredPosition = new Vector3(position.x + (bounds.x /2), - (bounds.y/2) - position.y, 0);

        nodeToTrans.Add(n.nodeID, t);
    }

    private void CreateConnection(Vector3[] positions, Connection c) {
        GameObject newConnection = Instantiate(linePrefab, transform);
        LineRenderer ln = newConnection.GetComponent<LineRenderer>();
        ln.SetPositions(positions);

        ln.startColor = EnableLineColor;
        ln.endColor = ln.startColor;

        if(!c.enabled) {
            ln.startColor = DisableLineColor;
            ln.endColor = ln.startColor;
        } 

        linePositions.Add(ln, c);
    }

    public void HideShowDrawer() {
        transform.parent.gameObject.SetActive(!gameObject.activeInHierarchy);
    }

}


