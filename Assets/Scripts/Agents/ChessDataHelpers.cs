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
}
