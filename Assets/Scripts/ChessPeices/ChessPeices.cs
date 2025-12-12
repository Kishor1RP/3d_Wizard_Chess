using UnityEngine;

public enum ChessPeicesType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
} 
public class ChessPeices : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public ChessPeicesType type;

    public Vector3 desiredPosition;
    public Vector3 desiredScale;
}
