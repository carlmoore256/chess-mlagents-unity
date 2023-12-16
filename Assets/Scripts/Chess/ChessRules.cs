using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum Team
{
    White = 0,
    Black = 1
}

[System.Serializable]
public enum PieceType
{
    Pawn = 0,
    Rook = 1,
    Knight = 2,
    Bishop = 3,
    Queen = 4,
    King = 5
}

public class ChessRules
{
    private ChessGame _game;

    public ChessRules(ChessGame game)
    {
        _game = game;
    }

    // maximum number of moves at the optimal time
    public static readonly Dictionary<PieceType, int> PiecesMaxMoves =
        new()
        {
            { PieceType.Pawn, 4 }, // 1 forward, 2 forward on first move, 2 captures
            { PieceType.Rook, 14 }, // 7 forward + 7 backward, or 7 left + 7 right
            { PieceType.Knight, 8 }, // Maximum 8 L-shaped moves
            { PieceType.Bishop, 13 }, // Up to 13 squares on the longest diagonal
            { PieceType.Queen, 27 }, // 7 in each direction (forward, backward, left, right, diagonal)
            { PieceType.King, 8 } // 1 in each direction around it
        };

    public static readonly Dictionary<PieceType, int> PiecesMaxCaptures = new() { };

    public static readonly Dictionary<PieceType, float> PiecesMeanMoves =
        new()
        {
            { PieceType.Pawn, 1.5f },
            { PieceType.Rook, 7.0f },
            { PieceType.Knight, 3.0f },
            { PieceType.Bishop, 6.0f },
            { PieceType.Queen, 12.0f },
            { PieceType.King, 3.0f }
        };

    // mean number of possible captures available to each piece
    public static readonly Dictionary<PieceType, float> PiecesMeanCaptures =
        new()
        {
            { PieceType.Pawn, 0.5f },
            { PieceType.Rook, 1.0f },
            { PieceType.Knight, 0.8f },
            { PieceType.Bishop, 0.8f },
            { PieceType.Queen, 1.5f },
            { PieceType.King, 0.3f }
        };

    public static string GetSquareId(int x, int y)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7)
            return null;
        return $"{(char)('a' + x)}{y + 1}";
    }

    public static string GetSquareId(Vector2Int position)
    {
        return GetSquareId(position.x, position.y);
    }

    public static Vector2Int GetPosition(string squareId)
    {
        return new Vector2Int(squareId[0] - 'a', squareId[1] - '1');
    }

    public static Vector2 SquareIdToNormalizedPosition(string squareId)
    {
        var position = GetPosition(squareId);
        // Normalizing such that the center of the board (between e4, e5, d4, d5) is (0, 0)
        return new Vector2(((float)position.x - 3.5f) / 4.0f, ((float)position.y - 3.5f) / 4.0f);
    }

    public static string GetRelativeSquareId(string squareId, int x, int y)
    {
        var position = GetPosition(squareId);
        return GetSquareId(position.x + x, position.y + y);
    }

    public static ChessPiece GetPieceAtSquare(string squareId, IEnumerable<ChessPiece> pieces)
    {
        return pieces.FirstOrDefault(p => p.CurrentSquare.Id == squareId);
    }

    public ChessPiece GetEnemyPieceAtSquare(string squareId, Team team)
    {
        return GetPieceAtSquare(squareId, _game.Teams[ChessTeam.OpposingTeam(team)].ActivePieces);
    }

    public ChessPiece GetFriendlyPieceAtSquare(string squareId, Team team)
    {
        return GetPieceAtSquare(squareId, _game.Teams[team].ActivePieces);
    }

    public (ChessPiece enemyOccupier, ChessPiece friendlyOccupier) GetSquareOccupants(
        string squareId,
        Team team
    )
    {
        var enemyOccupier = GetEnemyPieceAtSquare(squareId, team);
        var friendlyOccupier = GetFriendlyPieceAtSquare(squareId, team);
        return (enemyOccupier, friendlyOccupier);
    }

    public List<Move> GetMovesForPiece(ChessPiece piece)
    {
        if (piece.CurrentSquare == null)
            throw new System.Exception("Piece is not on the board");

        switch (piece.pieceType)
        {
            case PieceType.Pawn:
                return PawnMoves(piece);
            case PieceType.Rook:
                return RookMoves(piece);
            case PieceType.Knight:
                return KnightMoves(piece);
            case PieceType.Bishop:
                return BishopMoves(piece);
            case PieceType.Queen:
                return QueenMoves(piece);
            case PieceType.King:
                return KingMoves(piece);
        }

        return new List<Move>();
    }

    private List<Move> DiagonalMoves(ChessPiece piece, int xDir, int yDir)
    {
        var position = GetPosition(piece.CurrentSquare.Id);
        List<Move> moves = new List<Move>();

        int x = position.x;
        int y = position.y;

        while (true)
        {
            x += xDir;
            y += yDir;

            if (x < 0 || x >= 8 || y < 0 || y >= 8)
                break;

            var squareId = GetSquareId(x, y);
            // var occupier = GetPieceAtSquare(squareId, allPieces);

            var (enemyOccupier, friendlyOccupier) = GetSquareOccupants(squareId, piece.team);

            if (enemyOccupier != null)
            {
                moves.Add(new Move(piece, enemyOccupier));
                break;
            }
            else if (friendlyOccupier == null)
            {
                moves.Add(new Move(piece, _game.Board.Squares[squareId]));
            }
        }

        return moves;
    }

    private List<Move> DirectionalMove(ChessPiece piece, Vector2Int direction)
    {
        var position = GetPosition(piece.CurrentSquare.Id);
        List<Move> moves = new List<Move>();

        // Horizontal movement
        if (direction.x != 0)
        {
            int y = position.y;
            for (int x = position.x + direction.x; x >= 0 && x < 8; x += direction.x)
            {
                var squareId = GetSquareId(x, y);
                // var occupier = GetPieceAtSquare(squareId, allPieces);
                var enemyOccupier = GetEnemyPieceAtSquare(squareId, piece.team);
                var friendlyOccupier = GetFriendlyPieceAtSquare(squareId, piece.team);

                if (enemyOccupier != null)
                {
                    moves.Add(new Move(piece, enemyOccupier));
                    break;
                }
                else if (friendlyOccupier == null)
                {
                    moves.Add(new Move(piece, _game.Board.Squares[squareId]));
                }
            }
        }

        // Vertical movement
        if (direction.y != 0)
        {
            int x = position.x;
            for (int y = position.y + direction.y; y >= 0 && y < 8; y += direction.y)
            {
                var squareId = GetSquareId(x, y);
                var (enemyOccupier, friendlyOccupier) = GetSquareOccupants(squareId, piece.team);

                if (enemyOccupier != null)
                {
                    moves.Add(new Move(piece, enemyOccupier));
                    break;
                }
                else if (friendlyOccupier == null)
                {
                    moves.Add(new Move(piece, _game.Board.Squares[squareId]));
                }
            }
        }

        return moves;
    }

    public bool IsSquareUnderThreat(int x, int y, Team team)
    {
        // Implement the logic to check if the square at (x, y) is under threat from any piece of the opposing team
        // This involves checking the potential moves of all opposing pieces to see if they can capture on (x, y)
        return false; //TODO
    }

    public List<Move> KingMoves(ChessPiece piece)
    {
        var position = GetPosition(piece.CurrentSquare.Id);
        List<Move> moves = new List<Move>();

        // All eight possible directions a king can move
        List<Vector2Int> kingDirections = new List<Vector2Int>
        {
            new Vector2Int(1, 0), // Right
            new Vector2Int(-1, 0), // Left
            new Vector2Int(0, 1), // Up
            new Vector2Int(0, -1), // Down
            new Vector2Int(1, 1), // Up-Right
            new Vector2Int(-1, 1), // Up-Left
            new Vector2Int(1, -1), // Down-Right
            new Vector2Int(-1, -1) // Down-Left
        };

        foreach (var direction in kingDirections)
        {
            int x = position.x + direction.x;
            int y = position.y + direction.y;

            // Check if the new position is within the board bounds
            if (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                var squareId = GetSquareId(x, y);
                var (enemyOccupier, friendlyOccupier) = GetSquareOccupants(squareId, piece.team);

                if (enemyOccupier != null && !IsSquareUnderThreat(x, y, piece.team))
                {
                    moves.Add(new Move(piece, enemyOccupier));
                }

                if (friendlyOccupier == null && !IsSquareUnderThreat(x, y, piece.team))
                {
                    moves.Add(new Move(piece, _game.Board.Squares[squareId]));
                }

                if (enemyOccupier == null && !IsSquareUnderThreat(x, y, piece.team))
                {
                    moves.Add(new Move(piece, _game.Board.Squares[squareId]));
                }

                // // If the square is not occupied or is occupied by an opponent's piece, and is not under threat
                // if (
                //     (occupier == null || occupier.team != piece.team)
                //     && !IsSquareUnderThreat(x, y, piece.team, allPieces)
                // )
                // {
                //     moves.Add(new Move(piece, _game.Board.Squares[squareId]));
                // }
            }
        }

        return moves;
    }

    public List<Move> QueenMoves(ChessPiece piece)
    {
        List<Move> moves = new List<Move>();
        moves.AddRange(DirectionalMove(piece, Vector2Int.up));
        moves.AddRange(DirectionalMove(piece, Vector2Int.down));
        moves.AddRange(DirectionalMove(piece, Vector2Int.left));
        moves.AddRange(DirectionalMove(piece, Vector2Int.right));
        moves.AddRange(DiagonalMoves(piece, 1, 1));
        moves.AddRange(DiagonalMoves(piece, 1, -1));
        moves.AddRange(DiagonalMoves(piece, -1, 1));
        moves.AddRange(DiagonalMoves(piece, -1, -1));
        return moves;
    }

    public List<Move> BishopMoves(ChessPiece piece)
    {
        List<Move> moves = new List<Move>();
        moves.AddRange(DiagonalMoves(piece, 1, 1));
        moves.AddRange(DiagonalMoves(piece, 1, -1));
        moves.AddRange(DiagonalMoves(piece, -1, 1));
        moves.AddRange(DiagonalMoves(piece, -1, -1));
        return moves;
    }

    public List<Move> RookMoves(ChessPiece piece)
    {
        List<Move> moves = new List<Move>();
        moves.AddRange(DirectionalMove(piece, Vector2Int.up));
        moves.AddRange(DirectionalMove(piece, Vector2Int.down));
        moves.AddRange(DirectionalMove(piece, Vector2Int.left));
        moves.AddRange(DirectionalMove(piece, Vector2Int.right));
        return moves;
    }

    public List<Move> KnightMoves(ChessPiece piece)
    {
        var position = GetPosition(piece.CurrentSquare.Id);
        List<Move> moves = new List<Move>();

        // Define all possible knight moves as offsets from the current position
        List<Vector2Int> knightOffsets = new List<Vector2Int>
        {
            new Vector2Int(1, 2),
            new Vector2Int(2, 1),
            new Vector2Int(-1, 2),
            new Vector2Int(-2, 1),
            new Vector2Int(1, -2),
            new Vector2Int(2, -1),
            new Vector2Int(-1, -2),
            new Vector2Int(-2, -1)
        };

        foreach (var offset in knightOffsets)
        {
            int x = position.x + offset.x;
            int y = position.y + offset.y;

            // Check if the new position is within the board bounds
            if (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                var squareId = GetSquareId(x, y);
                var (enemyOccupier, friendlyOccupier) = GetSquareOccupants(squareId, piece.team);

                if (enemyOccupier != null)
                {
                    moves.Add(new Move(piece, enemyOccupier));
                    break;
                }
                else if (friendlyOccupier == null)
                {
                    moves.Add(new Move(piece, _game.Board.Squares[squareId]));
                }
            }
        }

        return moves;
    }

    private void PrintOccupiers((ChessPiece enemyOccupier, ChessPiece friendlyOccupier) occupiers, string info)
    {
        Debug.Log(
            info
            + " | Enemy: "
            + (occupiers.enemyOccupier != null ? occupiers.enemyOccupier.CurrentSquare.Id : "null")
            + " | Friendly: "
            + (occupiers.friendlyOccupier != null ? occupiers.friendlyOccupier.CurrentSquare.Id : "null")
        );
    }

    public List<Move> PawnMoves(ChessPiece piece)
    {
        var position = GetPosition(piece.CurrentSquare.Id);
        List<Move> moves = new List<Move>();

        Vector2Int forward = piece.team == Team.White ? Vector2Int.up : Vector2Int.down;
        var forwardSquare = GetSquareId(position + forward);
        if (forwardSquare == null)
            throw new System.Exception(
                "Forward square is null, but pawn is on the board | Pos: " + position
            );

        var forwardOccupiers = GetSquareOccupants(forwardSquare, piece.team);

        if (!forwardOccupiers.enemyOccupier && !forwardOccupiers.friendlyOccupier)
        {
            moves.Add(new Move(piece, _game.Board.Squares[forwardSquare]));
        }

        bool canMove2 = piece.team == Team.White ? position.y == 1 : position.y == 6;

        if (canMove2 && !forwardOccupiers.enemyOccupier && !forwardOccupiers.friendlyOccupier)
        {
            var forward2Square = GetSquareId(position + forward + forward);
            if (forward2Square == null)
                throw new System.Exception("Forward2 square is null, but pawn is on the board");

            var forward2Occupiers = GetSquareOccupants(forward2Square, piece.team);

            if (!forward2Occupiers.enemyOccupier && !forward2Occupiers.friendlyOccupier)
            {
                moves.Add(new Move(piece, _game.Board.Squares[forward2Square]));
            }
        }

        var leftSquare = GetSquareId(position + forward + Vector2Int.left);
        var rightSquare = GetSquareId(position + forward + Vector2Int.right);
        var leftOccupiers = GetSquareOccupants(leftSquare, piece.team);
        var rightOccupiers = GetSquareOccupants(rightSquare, piece.team);

        if (leftOccupiers.enemyOccupier)
        {
            moves.Add(new Move(piece, leftOccupiers.enemyOccupier));
        }
        if (rightOccupiers.enemyOccupier)
        {
            moves.Add(new Move(piece, rightOccupiers.enemyOccupier));
        }

        return moves;
    }
}


// var leftPiece = GetPieceAtSquare(
//     GetSquareId(position + forward + Vector2Int.left),
//     allPieces
// );
// var rightPiece = GetPieceAtSquare(
//     GetSquareId(position + forward + Vector2Int.right),
//     allPieces
// );

// if (leftPiece && leftPiece.team != piece.team)
// {
//     moves.Add(new Move(piece, leftPiece));
// }

// if (rightPiece && rightPiece.team != piece.team)
// {
//     moves.Add(new Move(piece, rightPiece));
// }

//  // can be used for diagonal and straight moves
//     private List<Move> DirectionalMove(
//         ChessPiece piece,
//         IEnumerable<ChessPiece> allPieces,
//         Vector2Int direction
//     )
//     {
//         var position = GetPosition(piece.CurrentSquare.Id);
//         List<Move> moves = new List<Move>();

//         if (direction.x != 0)
//         {
//             for (int x = position.x + direction.x; x < 8 && x >= 0; x += direction.x)
//             {
//                 var squareId = GetSquareId(x, position.y);
//                 var occupier = GetPieceAtSquare(squareId, allPieces);
//                 if (occupier)
//                 {
//                     if (occupier.team != piece.team)
//                     {
//                         moves.Add(new Move(piece, occupier));
//                     }
//                     break;
//                 }
//                 moves.Add(new Move(piece, _game.Board.Squares[squareId]));
//             }
//         }

//         if (direction.y != 0)
//         {
//             for (int y = position.y + direction.y; y < 8 && y >= 0; y += direction.y)
//             {
//                 var squareId = GetSquareId(position.x, y);
//                 var occupier = GetPieceAtSquare(squareId, allPieces);
//                 if (occupier)
//                 {
//                     if (occupier.team != piece.team)
//                     {
//                         moves.Add(new Move(piece, occupier));
//                     }
//                     break;
//                 }
//                 moves.Add(new Move(piece, _game.Board.Squares[squareId]));
//             }
//         }

//         return moves;
//     }


// private List<Move> DiagonalMoves(ChessPiece piece, IEnumerable<ChessPiece> allPieces, int xDir, int yDir)
// {
//     var position = GetPosition(piece.CurrentSquare.Id);
//     List<Move> moves = new List<Move>();

//     for (int x = position.x + xDir, y = position.y + yDir; x < 8 && y < 8; ++x, ++y)
//     {
//         var squareId = GetSquareId(x, y);
//         var occupier = GetPieceAtSquare(squareId, allPieces);
//         if (occupier)
//         {
//             if (occupier.team != piece.team)
//             {
//                 moves.Add(new Move(piece, occupier));
//             }
//             break;
//         }
//         moves.Add(new Move(piece, _game.Board.Squares[squareId]));
//     }

//     for (int x = position.x - xDir, y = position.y - yDir; x >= 0 && y >= 0; --x, --y)
//     {
//         var squareId = GetSquareId(x, y);
//         var occupier = GetPieceAtSquare(squareId, allPieces);
//         if (occupier)
//         {
//             if (occupier.team != piece.team)
//             {
//                 moves.Add(new Move(piece, occupier));
//             }
//             break;
//         }
//         moves.Add(new Move(piece, _game.Board.Squares[squareId]));
//     }

//     return moves;
// }


// public static string[] GetSquaresInRadius(string squareId, PieceType pieceType, Team team)
// {
//     switch (pieceType)
//     {
//         case PieceType.Pawn:
//             return PawnNextSquares(squareId, team);
//     }

//     return new string[0];
// }

// public static string[] PawnNextSquares(string squareId, Team team)
// {
//     var position = GetPosition(squareId);
//     List<string> squares = new List<string>();
//     if (team == Team.White)
//     {
//         var forwardSquare = GetSquareId(position.x, position.y + 1);
//         if (forwardSquare != null)
//         {
//             squares.Add(forwardSquare);
//             if (position.y == 1)
//             {
//                 var forward2Square = GetSquareId(position.x, position.y + 2);
//                 if (forward2Square != null)
//                 {
//                     squares.Add(forward2Square);
//                 }
//             }
//             else
//             {
//                 var capture1Square = GetSquareId(position.x + 1, position.y + 1);
//                 var capture2Square = GetSquareId(position.x - 1, position.y + 1);
//                 if (capture1Square != null)
//                 {
//                     squares.Add(capture1Square);
//                 }
//                 if (capture2Square != null)
//                 {
//                     squares.Add(capture2Square);
//                 }
//             }
//         }
//     }
//     else
//     {
//         var forwardSquare = GetSquareId(position.x, position.y - 1);
//         if (forwardSquare != null)
//         {
//             squares.Add(forwardSquare);
//             if (position.y == 6)
//             {
//                 var forward2Square = GetSquareId(position.x, position.y - 2);
//                 if (forward2Square != null)
//                 {
//                     squares.Add(forward2Square);
//                 }
//             }
//             else
//             {
//                 var capture1Square = GetSquareId(position.x + 1, position.y - 1);
//                 var capture2Square = GetSquareId(position.x - 1, position.y - 1);
//                 if (capture1Square != null)
//                 {
//                     squares.Add(capture1Square);
//                 }
//                 if (capture2Square != null)
//                 {
//                     squares.Add(capture2Square);
//                 }
//             }
//         }
//     }
//     return squares.ToArray();
// }

// public static string[] RookNextSquares(string squareId, Team team)
// {
//     var position = GetPosition(squareId);
//     List<string> squares = new List<string>();
//     for (int x = position.x + 1; x < 8; ++x)
//     {
//         var square = GetSquareId(x, position.y);
//         squares.Add(square);
//         if (Board.Squares[square].CurrentPiece != null)
//             break;
//     }
//     for (int x = position.x - 1; x >= 0; --x)
//     {
//         var square = GetSquareId(x, position.y);
//         squares.Add(square);
//         if (Board.Squares[square].CurrentPiece != null)
//             break;
//     }
//     for (int y = position.y + 1; y < 8; ++y)
//     {
//         var square = GetSquareId(position.x, y);
//         squares.Add(square);
//         if (Board.Squares[square].CurrentPiece != null)
//             break;
//     }
//     for (int y = position.y - 1; y >= 0; --y)
//     {
//         var square = GetSquareId(position.x, y);
//         squares.Add(square);
//         if (Board.Squares[square].CurrentPiece != null)
//             break;
//     }
//     return squares.ToArray();
// }



// if (piece.team == Team.White)
// {
//     var forwardSquare = GetSquareId(position.x, position.y + 1);
//     if (forwardSquare != null)
//     {
//         var occupier = GetPieceAtSquare(forwardSquare, allPieces);
//         if (!occupier)
//         {
//             moves.Add(
//                 new Move()
//                 {
//                     FromSquareId = piece.CurrentSquare.Id,
//                     ToSquareId = forwardSquare,
//                     IsPromotion = position.y == 6
//                 }
//             );
//         }

//         if (position.y == 1)
//         {
//             var forward2Square = GetSquareId(position.x, position.y + 2);
//             if (forward2Square != null)
//             {
//                 moves.Add(
//                     new Move()
//                     {
//                         FromSquareId = piece.CurrentSquare.Id,
//                         ToSquareId = forward2Square
//                     }
//                 );
//             }
//         }
//         else
//         {
//             var capture1Square = GetSquareId(position.x + 1, position.y + 1);
//             var occupier1 = GetPieceAtSquare(capture1Square, allPieces);
//             var capture2Square = GetSquareId(position.x - 1, position.y + 1);
//             var occupier2 = GetPieceAtSquare(capture2Square, allPieces);

//             if (occupier1 && occupier1.team != piece.team)
//             {
//                 moves.Add(
//                     new Move()
//                     {
//                         FromSquareId = piece.CurrentSquare.Id,
//                         ToSquareId = capture1Square,
//                         IsCapture = true,
//                         CapturedPiece = occupier1,
//                         IsPromotion = position.y == 6
//                     }
//                 );
//             }

//             if (occupier2 && occupier2.team != piece.team)
//             {
//                 moves.Add(
//                     new Move()
//                     {
//                         FromSquareId = piece.CurrentSquare.Id,
//                         ToSquareId = capture2Square,
//                         IsCapture = true,
//                         CapturedPiece = occupier2,
//                         IsPromotion = position.y == 6
//                     }
//                 );
//             }
//         }
//     } else {
//         throw new System.Exception("Forward square is null, but pawn is on the board");
//     }
// }
// else
// {
//     var forwardSquare = GetSquareId(position.x, position.y - 1);



//     // if (forwardSquare != null)
//     // {
//     //     squares.Add(forwardSquare);
//     //     if (position.y == 6)
//     //     {
//     //         var forward2Square = GetSquareId(position.x, position.y - 2);
//     //         if (forward2Square != null)
//     //         {
//     //             squares.Add(forward2Square);
//     //         }
//     //     }
//     //     else
//     //     {
//     //         var capture1Square = GetSquareId(position.x + 1, position.y - 1);
//     //         var capture2Square = GetSquareId(position.x - 1, position.y - 1);
//     //         if (capture1Square != null)
//     //         {
//     //             squares.Add(capture1Square);
//     //         }
//     //         if (capture2Square != null)
//     //         {
//     //             squares.Add(capture2Square);
//     //         }
//     //     }
//     // }
// }
// return squares.ToArray();
