using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public GameObject teamPrefab;
    public GameObject piecesParent;
    public GameObject capturedZonePrefab;
    public string startingGamePGN = "PGN/StartingGame";
    public IEnumerable<ChessPiece> Pieces => Teams[Team.White].ActivePieces.Concat(Teams[Team.Black].ActivePieces);
    public ChessTeam WhiteTeam => Teams[Team.White];
    public ChessTeam BlackTeam => Teams[Team.Black];

    public ChessBoard Board;
    public PGNData PGNData;

    public Action<Move> OnMove;

    public ChessRules Rules {
        get {
            if (_rules == null) _rules = new ChessRules(this);
            return _rules;
        }
    }

    private ChessRules _rules;

    public Dictionary<Team, ChessTeam> Teams = new()
    {
        { Team.White, null },
        { Team.Black, null }
    };

    private void Awake()
    {
        LoadStartingGame();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<MovePieceEvent>(OnMovePiece);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<MovePieceEvent>(OnMovePiece);
    }

    public void CreateTeams()
    {
        ClearPieces();
        var captureZones = piecesParent.GetComponentsInChildren<CaptureZone>();
        foreach (var captureZone in captureZones)
        {
#if UNITY_EDITOR
            DestroyImmediate(captureZone.gameObject);
#else
            Destroy(captureZone.gameObject);
#endif
        }
        var teamObjects = piecesParent.GetComponentsInChildren<ChessTeam>();
        foreach (var teamObject in teamObjects)
        {
#if UNITY_EDITOR
            DestroyImmediate(teamObject.gameObject);
#else
            Destroy(teamObject.gameObject);
#endif
        }

        SpawnTeam(Team.White);
        SpawnTeam(Team.Black);
    }

    public void SpawnTeam(Team team)
    {
        GameObject teamObj;
        if (teamPrefab == null) {
            teamObj = new GameObject(team.ToString());
        } else {
            teamObj = Instantiate(teamPrefab);
            teamObj.name = team.ToString();
        }
        teamObj.transform.parent = piecesParent.transform;
        var chessTeam = teamObj.AddComponent<ChessTeam>();
        chessTeam.Initialize(team, Rules, Board, piecePrefab);
        Teams[team] = chessTeam;
    }

    public void ClearPieces()
    {
        foreach (var team in Teams.Values)
        {
            if (team != null) team.ClearPieces();
        }
    }

    public void LoadStartingGame()
    {
        Board.SpawnSquares();
        var pgnString = Resources.Load<TextAsset>(startingGamePGN).text;
        PGNData = PGNParser.Parse(pgnString);
        CreateTeams();
        var placements = PGNData.GetStartingPositions();


        var whitePlacements = placements.Where(p => p.Team == Team.White);
        var blackPlacements = placements.Where(p => p.Team == Team.Black);

        Teams[Team.White].ClearPieces();
        Teams[Team.Black].ClearPieces();

        Teams[Team.White].SpawnPieces(whitePlacements.ToList());
        Teams[Team.Black].SpawnPieces(blackPlacements.ToList());

        // int pieceIndex = 0;
        // foreach (var placement in placements)
        // {
        //     Teams[placement.Team].SpawnPiece(placement.PieceType, placement.SquareId, pieceIndex);
        //     pieceIndex++;
        // }
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

    private void OnMovePiece(MovePieceEvent e)
    {
        Debug.Log(
            "Piece moved! "
                + e.Piece.name
                + " move "
                + e.Move.FromSquareId
                + " -> "
                + e.Move.ToSquareId
        );
        e.Piece.MoveToSquare(Board.Squares[e.Move.ToSquareId], 1f);
        // OnMove?.Invoke(e.Move);
    }
}
