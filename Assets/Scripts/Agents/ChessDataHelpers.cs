using System.Collections.Generic;
using System;

public static class ChessDataHelpers
{
    public static float[] MoveToVector(Move move)
    {
        var normalizedFromPosition = ChessRules.SquareIdToNormalizedPosition(move.FromSquareId);
        var normalizedToPosition = ChessRules.SquareIdToNormalizedPosition(move.ToSquareId);
        return new float[]
        {
            normalizedFromPosition.x,
            normalizedFromPosition.y,
            normalizedToPosition.x,
            normalizedToPosition.y,
            move.IsCapture ? 1f : 0f,
            move.IsPromotion ? 1f : 0f,
            move.CapturedPiece != null ? move.CapturedPiece.Value / 9f : 0f, // 9f expresses max value (queen)
        };
    }

    /// <summary>
    /// Returns a vector of floats sized 7 * fixedSize, where each 7 values represent a move.
    /// </summary>
    /// <param name="moves"></param>
    /// <param name="fixedSize"></param>
    /// <returns></returns>
    public static float[] GetStackedMoveVectors(List<Move> moves, int fixedSize)
    {
        float[] moveVectors = new float[fixedSize * 7]; // 7 values per move
        int i = 0;

        foreach (var move in moves)
        {
            var vector = MoveToVector(move);
            Array.Copy(vector, 0, moveVectors, i * 7, vector.Length);
            i++;
            if (i >= fixedSize)
                break; // Avoid exceeding the fixed size
        }

        // Pad the rest with zeros if fewer moves than fixedSize
        for (int j = i * 7; j < moveVectors.Length; j++)
        {
            moveVectors[j] = 0f;
        }

        return moveVectors;
    }

    public static float[] GetOneHotPieceVector(PieceType pieceType)
    {
        float[] oneHot = new float[6];
        oneHot[(int)pieceType] = 1f;
        return oneHot;
    }

    public static float[] GetBoardVectorForTeam(ChessTeam team)
    {
        float[] piecesBoardVector = new float[64];


        foreach (ChessPiece piece in team.ActivePieces)
        {
            if (piece.IsCaptured)
            {
                continue;
            }
            var position = piece.CurrentSquare.Position;
            int x = (int)position.x;
            int y = (int)position.y;

            piecesBoardVector[x + y * 8] = piece.Value / 10f;
        }

        return piecesBoardVector;
    }

    // returns a float array representing the state of the board
    // encodes white as positive values, and black as negative
    public static float[] GetBoardVector(ChessGame game)
    {
        var whiteVector = GetBoardVectorForTeam(game.WhiteTeam);
        var blackVector = GetBoardVectorForTeam(game.BlackTeam);

        float[] boardVector = new float[whiteVector.Length];
        for (int i = 0; i < whiteVector.Length; i++)
        {
            boardVector[i] = whiteVector[i];
            boardVector[i] += -blackVector[i];
        }

        return boardVector;
    }

    public static float[] GetNewVector(int count, float value = 1f)
    {
        float[] vector = new float[count];
        for (int i = 0; i < count; i++)
        {
            vector[i] = value;
        }
        return vector;
    }

    public static float[] GetGamePiecesVector(ChessGame game)
    {
        return GetNewVector(game.WhiteTeam.AllPieces.Count + game.BlackTeam.AllPieces.Count, 1f);
    }

    public static float[] GetTeamPiecesVector(ChessTeam team)
    {
        return GetNewVector(team.AllPieces.Count, 1f);
    }

    public static void CategorizeRemainingTeamPieces(float[] vector, ChessTeam team)
    {
        for (int i = 0; i < team.AllPieces.Count; i++)
        {
            if (team.AllPieces[i].IsCaptured)
            {
                vector[i] = 0f;
            }
            else
            {
                vector[i] = 1f;
            }
        }
    }

    public static void CategorizeRemainingGamePieces(float[] vector, ChessGame game)
    {
        for (int i = 0; i < game.WhiteTeam.AllPieces.Count; i++)
        {
            if (game.WhiteTeam.AllPieces[i].IsCaptured)
            {
                vector[i] = 0f;
            }
            else
            {
                vector[i] = 1f;
            }
        }
        for (int i = 0; i < game.BlackTeam.AllPieces.Count; i++)
        {
            if (game.BlackTeam.AllPieces[i].IsCaptured)
            {
                vector[i + game.WhiteTeam.AllPieces.Count] = 0f;
            }
            else
            {
                vector[i + game.WhiteTeam.AllPieces.Count] = 1f;
            }
        }
    }

    // float[] remainingPiecesVector = new float[6];
    // foreach (var piece in team.ActivePieces)
    // {
    //     remainingPiecesVector[(int)piece.pieceType] += 1f;
    // }
    // return remainingPiecesVector;
}
