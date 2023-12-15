using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct Move
{
    public string FromSquareId;
    public string ToSquareId;

    public ChessPiece Piece;
    public ChessSquare FromSquare;
    public ChessSquare ToSquare;

    public ChessPiece CapturedPiece;
    public bool IsCapture;
    public bool IsPromotion => _IsPromotion();
    public float CaptureValue => CapturedPiece != null ? CapturedPiece.Value : 0f;

    public Move(ChessPiece piece, ChessPiece capturedPiece)
    {
        FromSquareId = piece.CurrentSquare.Id;
        ToSquareId = capturedPiece.CurrentSquare.Id;
        Piece = piece;

        FromSquare = piece.CurrentSquare;
        ToSquare = capturedPiece.CurrentSquare;

        CapturedPiece = capturedPiece;
        IsCapture = true;
    }

    public Move(ChessPiece piece, ChessSquare toSquare)
    {
        FromSquareId = piece.CurrentSquare.Id;
        ToSquareId = toSquare.Id;
        Piece = piece;

        FromSquare = piece.CurrentSquare;
        ToSquare = toSquare;

        CapturedPiece = null;
        IsCapture = false;
    }

    public bool _IsPromotion()
    {
        if (Piece.pieceType != PieceType.Pawn)
            return false;
        if (Piece.team == Team.White)
            return ToSquareId[1] == '8';
        else
            return ToSquareId[1] == '1';
    }

    public override string ToString()
    {
        return $"{Piece.team} {Piece.pieceType}";
        // return $"{Piece.team} {Piece.pieceType} from {FromSquareId} to {ToSquareId}";
    }
}
