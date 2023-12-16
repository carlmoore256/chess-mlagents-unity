using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ChessGame))]
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
        var pieces = _game.Teams[CurrentTeam].ActivePieces;
        _currentPieceIndex = (_currentPieceIndex + 1) % pieces.Count;
        CurrentPiece = pieces[_currentPieceIndex];

        foreach (var piece in _game.Pieces)
        {
            piece.IsControlling = false;
        }

        Debug.Log($"Current Piece: {CurrentPiece.name}, setting IsControlling to true");

        CurrentPiece.IsControlling = true;

        Debug.Log(
            $"Current Piece: {CurrentPiece.name}, IsControlling: {CurrentPiece.IsControlling}"
        );
    }

    int currentTeamIndex = 0;
    private float lastDispatchTime = 0f;
    private float dispatchInterval = 0.05f; // Interval in seconds

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

        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     var team = currentTeamIndex % 2 == 0 ? Team.White : Team.Black;
        //     currentTeamIndex++;
        //     _game.Teams[team].MoveRandomPiece();
        // }

        if (Input.GetKey(KeyCode.M))
        {
            // convert int into team
            var team = currentTeamIndex % 2 == 0 ? Team.White : Team.Black;
            // _game.Teams[team].MoveRandomPiece();
            currentTeamIndex++;

            if (Time.time - lastDispatchTime >= dispatchInterval)
            {
                _game.Teams[team].MoveRandomPiece();
                lastDispatchTime = Time.time;
            }
            if (currentTeamIndex > 100)
            {
                currentTeamIndex = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            CurrentPiece.IsControlling = false;
            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            var team = currentTeamIndex % 2 == 0 ? Team.White : Team.Black;
            // _game.Teams[team].MoveRandomPiece();
            currentTeamIndex++;

            _game.Teams[team].MoveRandomPiece(PieceType.Bishop);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
