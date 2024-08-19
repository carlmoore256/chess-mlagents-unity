using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using UnityEngine;

// input size for observation is (8x8) square + (16x2) pieces 
public class ChessTeamAgent : Agent
{
    [Observable]
    public Team Team => _team.Team;
    public int maxInferenceAttempts = 10;

    private ChessTeam _team;
    private BehaviorParameters _behaviorParameters;
    private SimpleMultiAgentGroup _agents;
    private List<ChessAgent> _chessAgents = new List<ChessAgent>();
    private ChessGame _game;

    public bool IsReady { get; set; } = false;
    private int _numInitializedAgents = 0;

    public Action<ChessTeamAgent> OnTurnEnded;
    public Action<ChessTeamAgent> OnRequestStep;
    public Action<ChessTeamAgent> OnRequestEpisodeEnd;

    private int _inferenceAttempts = 0;

    private float[] _allPiecesVector;

    private void Awake()
    {
        _team = GetComponent<ChessTeam>();
        _agents = new SimpleMultiAgentGroup();
        _behaviorParameters = GetComponent<BehaviorParameters>();
        // _behaviorParameters.TeamId = (int)_team.Team;
        _game = GetComponentInParent<ChessGame>();
        // GetComponent<BehaviorParameters>().TeamId = gameObject.name == "White" ? 0 : 1;
    }

    public override void Initialize()
    {
        if (!IsReady) return;
        foreach (var piece in _team.AllPieces)
        {
            var chessAgent = piece.GetComponent<ChessAgent>();
            chessAgent.OnActionFailed += OnAgentActionFailed;
            _chessAgents.Add(chessAgent);

            piece.OnCaptured += OnPieceCaptured;
            piece.OnMove += OnPieceMove;

            chessAgent.OnActionSucceeded += OnAgentActionSucceeded;
        }

        RegisterAgents();
        IsReady = true;
        _game = GetComponentInParent<ChessGame>();
        _allPiecesVector = ChessDataHelpers.GetNewVector(32, 1f);
        // if (gameObject.name == "Black")
        // {
        //     _behaviorParameters.TeamId = 1;
        // }
    }

    private void OnAgentActionFailed(ChessAgent agent)
    {
        AddReward(-0.1f); // punish the chess team agent
        _inferenceAttempts++;
        if (_inferenceAttempts >= maxInferenceAttempts)
        {
            // make a random move, since it couldn't decide
            _team.MoveRandomPiece();
            OnTurnEnded?.Invoke(this);
        }
        else
        {
            RequestDecision();
            OnRequestStep?.Invoke(this);
        }
    }

    private void OnAgentActionSucceeded(ChessAgent agent)
    {
        AddReward(1f); // reward the chess team agent
        OnRequestStep?.Invoke(this);
    }


    public override void OnEpisodeBegin()
    {
        _inferenceAttempts = 0;
        _team.ResetCaptures();
    }

    private void RegisterAgents()
    {
        foreach (var agent in _chessAgents)
        {
            _agents.RegisterAgent(agent);
        }
    }

    private void OnPieceMove(Move move)
    {
        if (!IsReady) return;
        _agents.AddGroupReward(0.1f);
        if (move.IsCapture)
        {
            _agents.AddGroupReward(0.1f);
        }
        OnTurnEnded?.Invoke(this);
        _inferenceAttempts = 0;
    }


    public void _EndEpisode(float reward = 0f)
    {
        // get the remaining value of pieces
        float remainingValue = 0f;
        foreach (var piece in _team.ActivePieces)
        {
            remainingValue += piece.Value;
        }

        _agents.AddGroupReward(reward + remainingValue);
        AddReward(reward + remainingValue);
        Debug.Log("End Episode for " + _team.Team + " team | Cumulative Reward: " + GetCumulativeReward());
        EndEpisode();
        _agents.EndGroupEpisode();
    }


    private void OnPieceCaptured(ChessPiece piece)
    {
        if (!IsReady) return;
        // _agents.UnregisterAgent(piece.GetComponent<ChessAgent>());
        _agents.AddGroupReward(-1f);
        AddReward(-1f);
    }

    // returns a float vector of 8x8 containing float values of the value of each piece
    // private float[] GetPiecesBoardVector()
    // {
    //     float[] piecesBoardVector = new float[64];


    //     foreach (ChessPiece piece in _team.ActivePieces)
    //     {
    //         if (piece.IsCaptured)
    //         {
    //             continue;
    //         }
    //         var position = piece.CurrentSquare.Position;
    //         int x = (int)position.x;
    //         int y = (int)position.y;

    //         piecesBoardVector[x + y * 8] = piece.Value / 10f;
    //     }

    //     return piecesBoardVector;
    // }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!IsReady) return;


        var boardVector = ChessDataHelpers.GetBoardVector(_game);
        sensor.AddObservation(boardVector);

        // this makes a one hot vector containing the pieces remaining
        ChessDataHelpers.CategorizeRemainingGamePieces(_allPiecesVector, _game);
        sensor.AddObservation(_allPiecesVector);

        // make a one hot vector representing the existing pieces
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (_team.ActivePieces.Count == 1)
        {
            Debug.Log("Only one active piece remaining, ending episode");
            AddReward(-1f);
            OnRequestEpisodeEnd?.Invoke(this);
            return;
        }


        if (!IsReady) return;
        // here we select which piece to move
        try
        {
            ChessPiece pieceToMove = _team.GetPieceAtIndex(actions.DiscreteActions[0]);
            if (pieceToMove.IsCaptured)
            {
                OnAgentActionFailed(null);
                return;
            }
            pieceToMove.GetComponent<ChessAgent>().RequestDecision();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }

    }
}






// private float[] GetOneHotAvailablePieces()
// {
//     // float[] oneHot = new float[16];
//     // foreach (ChessPiece piece in _team.Pieces)
//     // {
//     //     if (piece.IsCaptured)
//     //     {
//     //         continue;
//     //     }

//     //     oneHot[(int)piece.pieceType] = 1f;
//     // }
// }
