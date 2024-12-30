using Photon.Pun; // Подключаем Photon
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class MazeGenerator : MonoBehaviourPun
{
    [SerializeField] private int _rows = 64;
    [SerializeField] private int _cols = 36;
    [SerializeField] private float _cellSize = 1.0f;

    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _floorPrefab;

    [SerializeField] private Material _matQuadrant0;
    [SerializeField] private Material _matQuadrant1;
    [SerializeField] private Material _matQuadrant2;
    [SerializeField] private Material _matQuadrant3;
    
    private int[,] _maze;
    private int[,] _cellType;
    private bool isSynchronized = false;
    private System.Random _random = new System.Random();
    private List<GameObject> _floors = new List<GameObject>();
    private GameManager _gameManager => FindObjectOfType<GameManager>();
    
    public Vector3[] PlayerSpawnPoints = new Vector3[4];
    
    [PunRPC]
    private void SyncMaze(int[] serializedMaze, int[] serializedCellType,Vector3[] playerSpawnPoints)
    {
        _maze = DeserializeMaze(serializedMaze, _rows, _cols);
        _cellType = DeserializeMaze(serializedCellType, _rows, _cols);
        PlayerSpawnPoints = playerSpawnPoints;
        BuildMaze();
    }

    private void InitializeMaze()
    {
        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                _maze[r, c] = 1;
                _cellType[r, c] = 0;
            }
        }

        _maze[1, 1] = 0;
        _cellType[1, 1] = 0;
    }

    private void CarveTopLeftQuadrant()
    {
        int midRow = _rows / 2;
        int midCol = _cols / 2;

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(1, 1));

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            List<Vector2Int> neighbors = GetValidNeighbors(current, 2, 1, midRow, midCol);

            if (neighbors.Count > 0)
            {
                stack.Push(current);

                Vector2Int chosen = neighbors[_random.Next(neighbors.Count)];
                int wallX = (current.x + chosen.x) / 2;
                int wallY = (current.y + chosen.y) / 2;

                _maze[chosen.x, chosen.y] = 0;
                _maze[wallX, wallY] = 0;

                _cellType[chosen.x, chosen.y] = 0;
                _cellType[wallX, wallY] = 0;

                stack.Push(chosen);
            }
        }
    }

    private List<Vector2Int> GetValidNeighbors(Vector2Int cell, int step, int minVal, int maxRow, int maxCol)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        int[] dx = { -step, step, 0, 0 };
        int[] dy = { 0, 0, -step, step };

        for (int i = 0; i < 4; i++)
        {
            int nx = cell.x + dx[i];
            int ny = cell.y + dy[i];

            if (nx >= minVal && nx < maxRow && ny >= minVal && ny < maxCol)
            {
                if (_maze[nx, ny] == 1)
                {
                    result.Add(new Vector2Int(nx, ny));
                }
            }
        }

        Shuffle(result);
        return result;
    }

    private void MirrorLeftToRight()
    {
        int midCol = _cols / 2;

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < midCol; c++)
            {
                int mirrorC = _cols - 1 - c;
                _maze[r, mirrorC] = _maze[r, c];
                _cellType[r, mirrorC] = _cellType[r, c] == 0 ? 1 : _cellType[r, c];
            }
        }
    }

    private void MirrorTopToBottom()
    {
        int midRow = _rows / 2;

        for (int r = 0; r < midRow; r++)
        {
            int mirrorR = _rows - 1 - r;

            for (int c = 0; c < _cols; c++)
            {
                _maze[mirrorR, c] = _maze[r, c];
                _cellType[mirrorR, c] = _cellType[r, c] switch
                {
                    0 => 2,
                    1 => 3,
                    _ => _cellType[r, c]
                };
            }
        }
    }

    private void BuildMaze()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                Vector3 pos = new Vector3(r * _cellSize, 0, c * _cellSize);
                bool isWall = (_maze[r, c] == 1);
                GameObject go;
                
                if (isWall)
                {
                    go = Instantiate(_wallPrefab, pos, Quaternion.identity, transform);
                }
                else
                {
                    go = Instantiate(_floorPrefab, pos, Quaternion.identity, transform);
                    _floors.Add(go);
                }
                go.GetComponent<Renderer>().material = _cellType[r, c] switch
                {
                    0 => _matQuadrant0,
                    1 => _matQuadrant1,
                    2 => _matQuadrant2,
                    3 => _matQuadrant3,
                    _ => null
                };
            }
        }
    }

    private int[] SerializeMaze()
    {
        int[] flatMaze = new int[_rows * _cols];
        int index = 0;

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                flatMaze[index++] = _maze[r, c];
            }
        }

        return flatMaze;
    }

    private int[] SerializeCellType()
    {
        int[] flatCellType = new int[_rows * _cols];
        int index = 0;

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                flatCellType[index++] = _cellType[r, c];
            }
        }

        return flatCellType;
    }

    private int[,] DeserializeMaze(int[] flatMaze, int rows, int cols)
    {
        int[,] maze = new int[rows, cols];
        int index = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                maze[r, c] = flatMaze[index++];
            }
        }

        return maze;
    }
    
    private Vector3[] InitializePlayerSpawnPoints()
    {
        Vector3[] spawnPoints = new Vector3[4];

        float offset = 1.5f;
        
        Debug.Log("Initializing Player spawn points");
        spawnPoints[0] = new Vector3(offset, 1, offset); // Верхний левый угол
        Debug.Log($"Player spawn points: {spawnPoints[0]}");
        spawnPoints[1] = new Vector3((_rows - 1) * _cellSize , 1, offset); // Верхний правый угол
        Debug.Log($"Player spawn points: {spawnPoints[1]}");
        spawnPoints[2] = new Vector3(offset, 1, (_cols - offset) * _cellSize); // Нижний левый угол
        Debug.Log($"Player spawn points: {spawnPoints[2]}");
        spawnPoints[3] = new Vector3((_rows - offset) * _cellSize , 1, (_cols - offset) * _cellSize); // Нижний правый угол
        Debug.Log($"Player spawn points: {spawnPoints[3]}");
        
        return spawnPoints;
    }
    
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    [ItemCanBeNull]
    public List<GameObject> GetFloors()
    {
        return _floors;
    }

    public Vector2 GetSize()
    {
        return new Vector2(_rows, _cols);
    }
    public void GenerateMaze()
    {
        _maze = new int[_rows, _cols];
        _cellType = new int[_rows, _cols];

        InitializeMaze();
        CarveTopLeftQuadrant();
        MirrorLeftToRight();
        MirrorTopToBottom();
        PlayerSpawnPoints = InitializePlayerSpawnPoints();
        // Сериализация и отправка данных через RPC
        photonView.RPC("SyncMaze", RpcTarget.AllBuffered, SerializeMaze(), SerializeCellType(), PlayerSpawnPoints);
        
        
    }
}
