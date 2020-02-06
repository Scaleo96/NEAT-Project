using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BoardMatrix {

    private int[][] board;
    private GameObject[][] objectsOnBoard;

    private Vector2 blockBorders;
    private Vector2 blockBounds;
    private Vector2 position;

    public BoardMatrix(int width, int hight, Vector2 position, Vector2 blockBounds, Vector2 blockBorders) {
        board = new int[width][];
        objectsOnBoard = new GameObject[width][];

        this.blockBorders = blockBorders;
        this.blockBounds = blockBounds;
        this.position = position;

        for(int i = 0; i < board.Length; i++) {
            board[i] = new int[hight];
            objectsOnBoard[i] = new GameObject[hight];

            for(int j = 0; j < board[i].Length; j++) {
                board[i][j] = 0;
                objectsOnBoard[i][j] = null;
            }
        }
    }

    public void ClearBoard(int value = 0) {
        for(int i = 0; i < board.Length; i++) {
            for(int j = 0; j < board[i].Length; j++) {
                board[i][j] = value;
                objectsOnBoard = null;
            }
        }
    }

    private GameObject ClearPoint(int x, int y) {
        board[x][y] = 0;
        GameObject obj = objectsOnBoard[x][y];
        objectsOnBoard[x][y] = null;
        return obj;
    }

    public bool SetPoint(int x, int y, int value, GameObject obj) {
        if(y < 0)
            return false;

        board[x][y] = value;
        objectsOnBoard[x][y] = obj;

        if(obj != null)
            objectsOnBoard[x][y].transform.position = GetUpdatedPosition(x, y);

        return true;
    }

    public bool SetPoints(Vector3Int[] points, GameObject[] objs) {
        if(points.Length != objs.Length)
            Debug.LogError("Points and objects arrays length dont match");

        for(int i = 0; i < points.Length; i++)
            if(!SetPoint(points[i].x, points[i].y, points[i].z, objs[i]))
                return false;

        return true;
    }

    public bool CheckPoints(Vector2Int[] points) {
        foreach(var point in points) {

            if(point.y >= GetBoardHight() || point.x >= GetBoardWidth() || point.x < 0)
                return false;

            if(point.y < 0)
                continue;

            if(board[point.x][point.y] == 1)
                return false;
        }

        return true;
    }

    public int GetPointValue(int x, int y) {
        return board[x][y];
    }

    public GameObject GetPointObject(int x, int y) {
        return objectsOnBoard[x][y];
    }

    public int GetBoardWidth() {
        return board.Length;
    }

    public int GetBoardHight() {
        return board[0].Length;
    }

    public Vector2 GetUpdatedPosition(int x, int y) {
        Vector2 blockBoder = new Vector2(blockBorders.x, blockBorders.y) * 0.5f;
        Vector2 blockOffset = new Vector2(blockBounds.x * -0.5f, blockBounds.y * -0.5f);
        return position + blockBoder - new Vector2Int(x, y) * blockBounds + blockOffset;
    }

    private bool CheckRow(int row) {
        if(row >= board[0].Length)
            return false;

        foreach(var colum in board)
            if(colum[row] == 0)
                return false;

        return true;
    }

    private List<GameObject> ClearRows(List<int> rows) {
        List<GameObject> objs = new List<GameObject>();

        foreach(var row in rows) {
            for(int i = 0; i < GetBoardWidth(); i++) {
                objs.Add(ClearPoint(i, row));
            }
        }

        return objs;
    }

    private void FillSpaceAfterClear(List<int> rows) {

        for(int i = 0; i < GetBoardWidth(); i++) {
            Vector3Int[] points = new Vector3Int[GetBoardHight() - rows.Count];
            GameObject[] objs = new GameObject[GetBoardHight() - rows.Count];

            for(int j = 0; j < rows[0]; j++) {
                points[j] = new Vector3Int(i, j + rows.Count, board[i][j]);
                objs[j] = objectsOnBoard[i][j];
            }

            SetPoints(points, objs);
        }
    }

    public System.Tuple<int, List<GameObject>> CheckForFullRows() {
        int hight = GetBoardHight();
        int counter = 0;
        List<int> rowsToClear = new List<int>();

        for(int i = 0; i < hight; i++) {
            if(!CheckRow(i)) {
                continue;

            } else {
                counter++;
                rowsToClear.Add(i);
                Debug.Log("row: " + i);
            }
        }


        if(counter > 0) {
            var data = System.Tuple.Create(counter, ClearRows(rowsToClear));
            FillSpaceAfterClear(rowsToClear);
            return data;
        }

        return System.Tuple.Create(counter, new List<GameObject>());
    }
}

public class GameBoard : MonoBehaviour {

    [Header("Board size")]
    [SerializeField] private int hight;
    [SerializeField] private int width;

    [Header("Game settings")]
    [SerializeField] private float waitMoveTime;
    [SerializeField] private float timeFractionReducer;
    [SerializeField] private float scoreToIncreaseDificulty;
    [SerializeField] private float framesWaitKeyDown;
    [SerializeField] private float scorePerRow;
    [SerializeField] private bool isAi;
    [SerializeField] private GameObject[] blocks;
    [SerializeField] private Color GameBoardColor;

    [Header("Refs")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text comboText;

    private GameObject[] currentPieces;
    private BoardMatrix gameBoard;
    private Piece currentPiece;
    private NEATAgent agent;
    private List<System.Type> types;
    private Vector2 blockBounds;
    private Vector2 blockBorders;
    private float waitTime;
    private float currentMoveTime;
    private float lastMovetime;
    private float currentScore;
    private bool synkBool;
    private bool comboWhitLast;
    private int combo;

    private int pieces;

    private void Start() {

        agent = GetComponent<NEATAgent>();
        blockBounds = blocks[0].GetComponent<SpriteRenderer>().bounds.size;
        blockBorders = blockBounds * new Vector2(width, hight);
        gameBoard = new BoardMatrix(width, hight, transform.position, blockBounds, blockBorders);

        waitTime = 0;
        currentMoveTime = waitMoveTime;
        currentScore = 0;
        comboWhitLast = false;
        combo = 1;
        pieces = 0;

        scoreText.text = "Score: 0";
        comboText.text = "Combo: 1";

        AddAllPiecesTypes();
        ClearPieces();
        CreateNewPiece();
        StartCoroutine(MoveCurrenPiece());
    }

    private void Update() {
        synkBool = false;

        if(isAi) {
            float[] outputs = agent.GetOutputs();

            //rotate
            if(outputs[0] >= 0.5f)
                Rotate(true);
            else if(outputs[0] <= -0.5f)
                Rotate(false);

            //move
            if(outputs[1] >= 0.5f)
                Move(false);
            else if(outputs[0] <= -0.5f)
                Move(true);

            //place
            if(outputs[2] >= 0.5f) {
                lastMovetime = currentMoveTime;
                currentMoveTime = 0;
            } else if(outputs[2] <= -0.5f)
                currentMoveTime = lastMovetime;

        } else {

            if(Input.GetKeyDown(KeyCode.UpArrow))
                Rotate(true);

            if(Input.GetKeyDown(KeyCode.DownArrow))
                Rotate(false);

            if(Input.GetKeyDown(KeyCode.LeftArrow))
                Move(false);

            if(Input.GetKeyDown(KeyCode.RightArrow))
                Move(true);


            if(Input.GetKey(KeyCode.LeftArrow))
                if(waitTime < framesWaitKeyDown)
                    waitTime++;
                else
                    Move(false);


            if(Input.GetKey(KeyCode.RightArrow))
                if(waitTime < framesWaitKeyDown)
                    waitTime++;
                else
                    Move(true);


            if(Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
                waitTime = 0;

            if(Input.GetKeyDown(KeyCode.Space)) {
                lastMovetime = currentMoveTime;
                currentMoveTime = 0;
            }

            if(Input.GetKeyUp(KeyCode.Space))
                currentMoveTime = lastMovetime;
        }

        synkBool = true;
    }

    private void Move(bool right) {
        if(gameBoard.CheckPoints(currentPiece.GetNextPosition(right))) {
            currentPiece.Move(right);
            UpdatePosistion();
        }
    }

    private void Rotate(bool right) {
        if(gameBoard.CheckPoints(currentPiece.GetNextOriantation(right))) {
            currentPiece.Rotate(right);
            UpdatePosistion();
        }
    }

    private IEnumerator MoveCurrenPiece() {
        bool move = true;

        while(move) {

            Vector2Int[] oriantation = currentPiece.GetOriantation();

            for(int i = 0; i < oriantation.Length; i++)
                oriantation[i] += Vector2Int.up;

            yield return new WaitUntil(() => synkBool == true);

            if(gameBoard.CheckPoints(oriantation)) {
                currentPiece.MoveDown();
                UpdatePosistion();
            } else {
                move = false;
                yield return null;
            }

            yield return new WaitForSeconds(currentMoveTime);
        }

        PlacePieces();
        ClearPieces();
        CheckRows();
        CreateNewPiece();

        StartCoroutine(MoveCurrenPiece());
    }

    private void UpdatePosistion() {
        for(int i = 0; i < 4; i++)
            currentPieces[i].transform.position = GetPiecePosition(i);
    }

    private void ClearPieces() {
        currentPieces = new GameObject[4];
    }

    private void PlacePieces() {
        Vector3Int[] points = new Vector3Int[4];
        Vector2Int[] oriantation = currentPiece.GetOriantation();

        for(int i = 0; i < points.Length; i++)
            points[i] = new Vector3Int(oriantation[i].x, oriantation[i].y, 1);

        if(!gameBoard.SetPoints(points, currentPieces)) {
            agent.SetFitness(pieces + (int)currentScore / 100);
            agent.TrainingOver();
            Debug.Log("Game Over");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        pieces++;
        agent.SetFitness(pieces + (int)currentScore);
    }

    private void CheckRows() {
        var result = gameBoard.CheckForFullRows();

        if(result.Item1 == 0) {
            comboWhitLast = false;
            combo = 1;
            return;
        }

        foreach(var obj in result.Item2)
            Destroy(obj);

        if(comboWhitLast)
            combo += result.Item1;
        else
            combo = result.Item1;

        comboWhitLast = true;

        currentScore += result.Item1 * combo * scorePerRow;

        agent.SetFitness(pieces + (int)currentScore);

        float tempTime = currentMoveTime;
        tempTime -= (currentScore / scoreToIncreaseDificulty) * timeFractionReducer;
        if(tempTime < currentMoveTime && tempTime >= 0)
            currentMoveTime = tempTime;

        scoreText.text = "Score: " + currentScore.ToString();
        comboText.text = "Combo: " + combo.ToString();
    }

    private void CreateNewPiece() {
        var typeSelect = types[Random.Range(0, types.Count)];
        Vector2Int randomPosition = new Vector2Int(Random.Range(1, 11), -2);
        currentPiece = (Piece)System.Activator.CreateInstance(typeSelect, randomPosition, Rotation.Up);
        int blockColor = Random.Range(0, blocks.Length);

        Vector2Int[] oriantation = currentPiece.GetOriantation();

        for(int i = 0; i < 4; i++)
            currentPieces[i] = Instantiate(blocks[blockColor], GetPiecePosition(i), Quaternion.identity);
    }

    private Vector2 GetPiecePosition(int i) {
        Vector2Int[] oriantation = currentPiece.GetOriantation();
        Vector2 blockBoder = new Vector2(blockBorders.x, blockBorders.y) * 0.5f;
        Vector2 blockOffset = new Vector2(blockBounds.x * -0.5f, blockBounds.y * -0.5f);
        return (Vector2)transform.position + blockBoder - oriantation[i] * blockBounds + blockOffset;
    }

    private void AddAllPiecesTypes() {
        var assembly = Assembly.GetExecutingAssembly();
        var currentType = typeof(Piece);
        types = new List<System.Type>();

        foreach(var type in assembly.GetTypes())
            if(currentType.IsAssignableFrom(type))
                types.Add(type);

        types.Remove(typeof(Piece));
    }

    private void OnDrawGizmos() {
        Gizmos.color = GameBoardColor;
        Vector2 bounds = blocks[0].GetComponent<SpriteRenderer>().bounds.size;
        Vector2 borders = bounds * new Vector2(width, hight);

        for(int i = 0; i < hight; i++) {
            float currentY = i * bounds.y;

            for(int j = 0; j < width; j++) {
                float currentX = j * bounds.x;
                Vector2 currenPos = new Vector2(currentX + bounds.x / 2, currentY + bounds.y / 2) + (Vector2)transform.position - borders / 2;

                Gizmos.DrawWireCube(currenPos, bounds);
            }
        }
    }

}
