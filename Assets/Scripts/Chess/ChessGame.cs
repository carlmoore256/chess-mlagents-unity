using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum Team
{
    White = 0,
    Black = 1
}

[System.Serializable]
public struct PiecePlacement
{
    public PieceType PieceType;
    public Team Team;
    public string SquareId;
}


public class ChessGame : MonoBehaviour
{
    public GameObject piecePrefab;
    public GameObject piecesParent;
    public List<ChessPiece> Pieces = new List<ChessPiece>();

    public ChessBoard Board;
    public PGNData PGNData;

    public readonly List<PiecePlacement> StartingPieces = new List<PiecePlacement>() {
        new() { PieceType = PieceType.Pawn, Team = Team.White, SquareId = "a2" },
        new() { PieceType = PieceType.Pawn, Team = Team.White, SquareId = "b2" },
        new() { PieceType = PieceType.Pawn, Team = Team.White, SquareId = "c2" },
        new() { PieceType = PieceType.Pawn, Team = Team.White, SquareId = "d2" },
        new() { PieceType = PieceType.Pawn, Team = Team.White, SquareId = "e2" },
        new() { PieceType = PieceType.Pawn, Team = Team.White, SquareId = "f2" },
        new() { PieceType = PieceType.Pawn, Team = Team.White, SquareId = "g2" },
        new() { PieceType = PieceType.Pawn, Team = Team.White, SquareId = "h2" },
        new() { PieceType = PieceType.Rook, Team = Team.White, SquareId = "a1" },
        new() { PieceType = PieceType.Knight, Team = Team.White, SquareId = "b1" },
        new() { PieceType = PieceType.Bishop, Team = Team.White, SquareId = "c1" },
        new() { PieceType = PieceType.Queen, Team = Team.White, SquareId = "d1" },
        new() { PieceType = PieceType.King, Team = Team.White, SquareId = "e1" },
        new() { PieceType = PieceType.Bishop, Team = Team.White, SquareId = "f1" },
        new() { PieceType = PieceType.Knight, Team = Team.White, SquareId = "g1" },
        new() { PieceType = PieceType.Rook, Team = Team.White, SquareId = "h1" },
        new() { PieceType = PieceType.Pawn, Team = Team.Black, SquareId = "a7" },
        new() { PieceType = PieceType.Pawn, Team = Team.Black, SquareId = "b7" },
        new() { PieceType = PieceType.Pawn, Team = Team.Black, SquareId = "c7" },
        new() { PieceType = PieceType.Pawn, Team = Team.Black, SquareId = "d7" },
        new() { PieceType = PieceType.Pawn, Team = Team.Black, SquareId = "e7" },
        new() { PieceType = PieceType.Pawn, Team = Team.Black, SquareId = "f7" },
        new() { PieceType = PieceType.Pawn, Team = Team.Black, SquareId = "g7" },
        new() { PieceType = PieceType.Pawn, Team = Team.Black, SquareId = "h7" },
        new() { PieceType = PieceType.Rook, Team = Team.Black, SquareId = "a8" },
        new() { PieceType = PieceType.Knight, Team = Team.Black, SquareId = "b8" },
        new() { PieceType = PieceType.Bishop, Team = Team.Black, SquareId = "c8" },
        new() { PieceType = PieceType.Queen, Team = Team.Black, SquareId = "d8" },
        new() { PieceType = PieceType.King, Team = Team.Black, SquareId = "e8" },
        new() { PieceType = PieceType.Bishop, Team = Team.Black, SquareId = "f8" },
        new() { PieceType = PieceType.Knight, Team = Team.Black, SquareId = "g8" },
        new() { PieceType = PieceType.Rook, Team = Team.Black, SquareId = "h8" },
    };

    public void ClearPieces()
    {
        var pieceObjects = piecesParent.GetComponentsInChildren<ChessPiece>();
        foreach (var piece in pieceObjects)
        {
#if UNITY_EDITOR
            DestroyImmediate(piece.gameObject);
#else
            Destroy(piece.gameObject);
#endif
        }
        Pieces.Clear();
    }

    public void SpawnPieces()
    {
        ClearPieces();
        foreach (var piecePlacement in StartingPieces)
        {
            var square = Board.Squares[piecePlacement.SquareId];
            var pieceObject = Instantiate(piecePrefab, piecesParent.transform);
            var chessPiece = pieceObject.GetComponent<ChessPiece>();
            chessPiece.Initialize(piecePlacement.PieceType, piecePlacement.Team, square);
            Pieces.Add(chessPiece);
        }
    }

    public IEnumerable<ChessPiece> TeamPieces(Team team)
    {
        return Pieces.FindAll(p => p.team == team);
    }

    public void LoadFromPGNFile(string filepath)
    {
        // var pgnData = ApplicationData.LoadFromText("PGN", filepath);
    }

    public void FromPGN(string pgn)
    {
        ClearPieces();

        // var pgnData = 
        // var moves = PGNParser.Parse(pgn);
        // foreach (var move in moves)
        // {
        //     var piece = Pieces.Find(p => p.CurrentSquare.Id == move.FromSquareId);
        //     if (piece == null)
        //     {
        //         Debug.LogError($"Piece not found on square {move.FromSquareId}");
        //         continue;
        //     }
        //     piece.MoveToSquare(Board.Squares[move.ToSquareId]);
        // }
    }
}