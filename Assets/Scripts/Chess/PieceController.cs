using System.Linq;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    public Team CurrentTeam { get; set; } = Team.White;
    public ChessPiece CurrentPiece { get; set; }

    private ChessGame _game;
    private int _currentPieceIndex = 0;

    private void Start()
    {
        _game = FindObjectOfType<ChessGame>();
    }

    public void SwitchTeam()
    {
        if (CurrentTeam == Team.White)
        {
            CurrentTeam = Team.Black;
        }
        else
        {
            CurrentTeam = Team.White;
        }
        _currentPieceIndex -= 1;
        CyclePiece();
    }

    public void CyclePiece()
    {
        var pieces = _game.TeamPieces(CurrentTeam).ToList();
        _currentPieceIndex = (_currentPieceIndex + 1) % pieces.Count;
        CurrentPiece = pieces[_currentPieceIndex];

        foreach (var piece in _game.Pieces)
        {
            piece.IsControlling = false;
        }

        Debug.Log($"Current Piece: {CurrentPiece.name}, setting IsControlling to true");

        CurrentPiece.IsControlling = true;

        Debug.Log($"Current Piece: {CurrentPiece.name}, IsControlling: {CurrentPiece.IsControlling}");
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CyclePiece();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            SwitchTeam();
        }
    }
}