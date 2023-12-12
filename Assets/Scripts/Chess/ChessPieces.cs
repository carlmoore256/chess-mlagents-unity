using UnityEngine;



[CreateAssetMenu(fileName = "ChessPieces", menuName = "ScriptableObjects/ChessPieces", order = 1)]
public class ChessPieces : ScriptableObject
{
    public GameObject whiteKing;
    public GameObject blackKing;

    public GameObject whiteQueen;
    public GameObject blackQueen;

    public GameObject whiteRook;
    public GameObject blackRook;

    public GameObject whiteBishop;
    public GameObject blackBishop;

    public GameObject whiteKnight;
    public GameObject blackKnight;

    public GameObject whitePawn;
    public GameObject blackPawn;

    public GameObject SpawnPieceModel(PieceType pieceType, Team team, Transform parent)
    {
        GameObject pieceToSpawn = null;
        switch (pieceType)
        {
            case PieceType.Pawn:
                pieceToSpawn = team == Team.White ? whitePawn : blackPawn;
                break;
            case PieceType.Rook:
                pieceToSpawn = team == Team.White ? whiteRook : blackRook;
                break;
            case PieceType.Knight:
                pieceToSpawn = team == Team.White ? whiteKnight : blackKnight;
                break;
            case PieceType.Bishop:
                pieceToSpawn = team == Team.White ? whiteBishop : blackBishop;
                break;
            case PieceType.Queen:
                pieceToSpawn = team == Team.White ? whiteQueen : blackQueen;
                break;
            case PieceType.King:
                pieceToSpawn = team == Team.White ? whiteKing : blackKing;
                break;
        }

        GameObject piece = Instantiate(pieceToSpawn, parent.position, parent.rotation, parent);
        piece.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        piece.tag = $"{team.ToString().ToLower()}{pieceType}";
        piece.name = $"{team.ToString().ToLower()}{pieceType}_model";
        return piece;
    }

}