using UnityEngine;
using UnityEngine.Tilemaps;

public class ChessBoard : MonoBehaviour
{
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    //Logic
    private ChessPeices[,] chessPeices;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private void Awake()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPeices();
        PositionAllPeices();
    }
    private void Update()
{
    if (!currentCamera)
    {
        currentCamera = Camera.main;
        return;
    }

    RaycastHit info;
    Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

    // --- Start of Raycast Check ---
    if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover")))
    {
        // Get the indexes of the tile i've hit
        Vector2Int hitPosition = LookupTileindex(info.transform.gameObject);

        // CASE 1: Hovering over a tile for the first time
        if (currentHover == -Vector2Int.one)
        {
            currentHover = hitPosition;
            tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
        }

        // CASE 2: Moving from one tile to a DIFFERENT tile
        if (currentHover != hitPosition)
        {
            // Reset the previous tile's layer
            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
            
            // Set the new hover position and layer
            currentHover = hitPosition;
            tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
        }
        
        // NOTE: If currentHover == hitPosition, we do nothing (we're just staying on the same tile)
    }
    // --- End of Raycast Check ---
    
    // --- The essential fix: Mouse is NOT over any tile ---
    else 
    {
        // We were hovering over something previously, so reset it
        if (currentHover != -Vector2Int.one)
        {
            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
            currentHover = -Vector2Int.one;
        }
    }
}
    //Genrate Board
    private void GenerateAllTiles(float tilesize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;
        
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tilesize, x, y);

    }
    private GameObject GenerateSingleTile(float tilesize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tilesize, yOffset, y * tilesize) - bounds;
        vertices[1] = new Vector3(x * tilesize, yOffset, (y + 1) * tilesize) - bounds;
        vertices[2] = new Vector3((x + 1) * tilesize, yOffset, y * tilesize) - bounds;
        vertices[3] = new Vector3((x + 1) * tilesize, yOffset, (y + 1) * tilesize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();


        return tileObject;
    }

    //Spawning og the peices
    private void SpawnAllPeices()
    {
        chessPeices = new ChessPeices[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0, blackTeam = 1;

        // White Team
        chessPeices[0,0] = SpawnSinglePiece(ChessPeicesType.Rook, whiteTeam);
        chessPeices[1,0] = SpawnSinglePiece(ChessPeicesType.Knight, whiteTeam);
        chessPeices[2,0] = SpawnSinglePiece(ChessPeicesType.Bishop, whiteTeam);
        chessPeices[3,0] = SpawnSinglePiece(ChessPeicesType.Queen, whiteTeam);
        chessPeices[4,0] = SpawnSinglePiece(ChessPeicesType.King, whiteTeam);
        chessPeices[5,0] = SpawnSinglePiece(ChessPeicesType.Bishop, whiteTeam);
        chessPeices[6,0] = SpawnSinglePiece(ChessPeicesType.Knight, whiteTeam);
        chessPeices[7,0] = SpawnSinglePiece(ChessPeicesType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPeices[i, 1] = SpawnSinglePiece(ChessPeicesType.Pawn, whiteTeam);

        // Black Team
        chessPeices[0,7] = SpawnSinglePiece(ChessPeicesType.Rook, blackTeam);
        chessPeices[1,7] = SpawnSinglePiece(ChessPeicesType.Knight, blackTeam);
        chessPeices[2,7] = SpawnSinglePiece(ChessPeicesType.Bishop, blackTeam);
        chessPeices[3,7] = SpawnSinglePiece(ChessPeicesType.King, blackTeam);
        chessPeices[4,7] = SpawnSinglePiece(ChessPeicesType.Queen, blackTeam);
        chessPeices[5,7] = SpawnSinglePiece(ChessPeicesType.Bishop, blackTeam);
        chessPeices[6,7] = SpawnSinglePiece(ChessPeicesType.Knight, blackTeam);
        chessPeices[7,7] = SpawnSinglePiece(ChessPeicesType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPeices[i, 6] = SpawnSinglePiece(ChessPeicesType.Pawn, blackTeam);
    }
    private ChessPeices SpawnSinglePiece(ChessPeicesType type, int team)
    {
        ChessPeices cp = Instantiate(prefabs[(int)type -1],transform).GetComponent<ChessPeices>();
        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];
        
        return cp;
    }
    
    // Positioning
    private void PositionAllPeices()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if(chessPeices[x,y] != null)
                    PositionSinglePeice(x,y, true);
    }
    private void PositionSinglePeice(int x, int y, bool force = false)
    {
        chessPeices[x, y].currentX = x;
        chessPeices[x, y].currentY = y;
        chessPeices[x, y].transform.position = GetTileCenter(x, y);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y*tileSize)-bounds + new Vector3(tileSize / 2, 0, tileSize/ 2);
    }
    
    // Operations
    private Vector2Int LookupTileindex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);


        return -Vector2Int.one;
    }
}