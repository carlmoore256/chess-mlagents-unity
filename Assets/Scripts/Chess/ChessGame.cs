using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    public GameObject whiteTeamPrefab;
    public GameObject blackTeamPrefab;
    public GameObject piecesParent;
    public GameObject capturedZonePrefab;
    public string startingGamePGN = "PGN/StartingGame";
    public IEnumerable<ChessPiece> Pieces => Teams[Team.White].ActivePieces.Concat(Teams[Team.Black].ActivePieces);
    public ChessTeam WhiteTeam => Teams[Team.White];
    public ChessTeam BlackTeam => Teams[Team.Black];

    public ChessBoard Board;
    public PGNData PGNData;

    public Action<Move> OnMove;
    public bool IsInitialized { get; private set; } = false;

    public ChessRules Rules
    {
        get
        {
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

    public void CreateTeams()
    {
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
        if (whiteTeamPrefab == null)
        {
            teamObj = new GameObject(team.ToString());
        }
        else
        {
            if (team == Team.Black)
            {
                teamObj = Instantiate(blackTeamPrefab);
            }
            else
            {
                teamObj = Instantiate(whiteTeamPrefab);
            }
            teamObj.name = team.ToString();
        }
        teamObj.transform.parent = piecesParent.transform;
        var chessTeam = teamObj.GetOrAddComponent<ChessTeam>();
        chessTeam.Initialize(team, Rules, Board, piecePrefab);
        Teams[team] = chessTeam;
    }

    public void LoadStartingGame()
    {
        Board.SpawnSquares();
        var pgnString = Resources.Load<TextAsset>(startingGamePGN).text;
        PGNData = PGNParser.Parse(pgnString);
        CreateTeams();
        // Debug.Log("YOOO black " + Teams[Team.Black].name);
        var placements = PGNData.GetStartingPositions();
        var whitePlacements = placements.Where(p => p.Team == Team.White);
        var blackPlacements = placements.Where(p => p.Team == Team.Black);
        Teams[Team.White].SpawnPieces(whitePlacements.ToList());
        Teams[Team.Black].SpawnPieces(blackPlacements.ToList());
        IsInitialized = true;
        Debug.Log("Chess Game Initialized");
    }
}
