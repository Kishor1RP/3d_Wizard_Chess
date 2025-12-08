using UnityEngine;
using UnityEngine.Tilemaps;

public class ChessBoard : MonoBehaviour
{
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    //Logic
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private void Awake()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
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