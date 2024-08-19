using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class AgentActionEvent
{
    public ChessAgent Agent;
    public Move Move;

    public AgentActionEvent(ChessAgent agent, Move move)
    {
        Agent = agent;
        Move = move;
    }

    public AgentActionEvent(ChessAgent agent)
    {
        Agent = agent;
        Move = null;
    }
}


[RequireComponent(typeof(ChessPiece))]
public class ChessAgent : Agent
{
    public ChessPiece Piece { get; private set; }

    [Observable]
    public Team Team
    {
        get => Piece.team;
    }

    public bool ShouldRequestDecision { get; set; }

    public float rotSign;
    public Vector3 initialPos;
    public Quaternion initialRot;

    [Observable]
    public PieceType PieceType => Piece.pieceType;

    public int numTopVectors = 5;

    private float _forwardSpeed = 0.5f;
    private float _lateralSpeed = 0.5f;
    private float _existential = 0.01f;

    private ChessSettings _chessSettings;
    private BehaviorParameters _behaviorParameters;
    private EnvironmentParameters _resetParameters;
    private ChessEnvController _chessEnvController;
    private List<Move> _nextMoves;
    private bool _isInitialized = false;
    public UnityEvent<float> OnRewardChanged;

    public Action<ChessAgent> OnActionFailed;
    public Action<ChessAgent> OnActionSucceeded;

    public float moveSpeed = 0.5f;

    private void Awake()
    {
        _chessSettings = FindObjectOfType<ChessSettings>();
        _chessEnvController = GetComponentInParent<ChessEnvController>();
        Piece = GetComponent<ChessPiece>();
        // Piece.OnInitialized += (p) =>
        // {
        //     pieceType = p.pieceType;
        //     _isInitialized = true;
        //     Piece.ResetToStartingPosition();
        // };

        _isInitialized = true;

        OnRewardChanged?.Invoke(GetCumulativeReward());

        GetComponent<RayPerceptionSensorComponent3D>().SensorName = Guid.NewGuid().ToString();
        _behaviorParameters = GetComponent<BehaviorParameters>();
        _behaviorParameters.TeamId = Team.White == Team ? 0 : 1;
    }

    public void SetInitialTransform(Vector3 position, Quaternion rotation)
    {
        initialPos = position;
        initialRot = rotation;
    }

    public void Reset()
    {
        // EndEpisode();
        gameObject.SetActive(true);
        Piece.ResetToStartingPosition();
        OnRewardChanged?.Invoke(GetCumulativeReward());
        _behaviorParameters.TeamId = Team.White == Team ? 0 : 1;

    }

    private float[] GetOneHotPieceType()
    {
        return ChessDataHelpers.GetOneHotPieceVector(PieceType);
    }

    public override void Initialize()
    {
        // if (!_isInitialized)
        // {
        //     Debug.LogError("Initialize called by mlagents in ChessAgent, but ChessPiece has not yet initialized", gameObject);
        //     return;
        // }
        OnRewardChanged?.Invoke(GetCumulativeReward());
        Debug.Log("[Initialize] ChessAgent Initialized");
        if (_chessEnvController != null)
        {
            _existential = 1f / _chessEnvController.MaxEnvironmentSteps;
        }
        else
        {
            _existential = 1f / MaxStep;
        }

        _behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (Team == Team.White)
        {
            rotSign = 1f;
            _behaviorParameters.TeamId = 0;
        }
        else
        {
            rotSign = -1f;
            _behaviorParameters.TeamId = 1;
        }
        _resetParameters = Academy.Instance.EnvironmentParameters;


        // Piece.OnCaptured += (p) =>
        // {
        //     Debug.Log("ChessAgent Captured");
        //     Kill();
        // };
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (!_isInitialized)
        {
            Debug.LogError("[CollectObservations] ChessAgent not initialized");
            return;
        }

        if (Piece.CurrentSquare == null)
        {
            Debug.LogError("Piece has no current square");
            return;
        }

        // add the type of piece as one hot encoded vector
        // sensor.AddObservation(GetOneHotPieceType());

        // add the normalized position of the piece on the board
        sensor.AddObservation(ChessRules.SquareIdToNormalizedPosition(Piece.CurrentSquare.Id));

        var moves = Piece.GetValidMoves();

        // record how many moves we have normalized by the mean moves usually available for that piece type
        sensor.AddObservation(moves.Count / ChessRules.PiecesMeanMoves[Piece.pieceType]);

        var captureMoves = moves.FindAll(m => m.IsCapture);
        sensor.AddObservation(captureMoves.Count / ChessRules.PiecesMeanCaptures[Piece.pieceType]);

        // order the moves by the value of captures first
        moves.Sort((a, b) => b.CaptureValue.CompareTo(a.CaptureValue));

        var moveVectors = ChessDataHelpers.GetStackedMoveVectors(moves, numTopVectors);
        sensor.AddObservation(moveVectors);

        // Debug.Log("Current observation size: " + sensor.ObservationSize());
        // Debug.Log("Current observation: " + moveVectors[0]);


        // _nextMoves.Clear();
        // _nextMoves.AddRange(moves.GetRange(0, Mathf.Min(numTopVectors, moves.Count)));

        // Debug.Log();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        if (!_isInitialized)
        {
            Debug.LogError("[OnActionReceived] ChessAgent not initialized");
            return;
        }

        // model will select a move from the top numTopVector moves, which will be discreteActions[0]
        // int selectedMoveIndex = actionBuffers.DiscreteActions[0]; // discrete actions[0] branch size MUST be numTopVectors
        // int shouldMove = actionBuffers.DiscreteActions[1]; // discrete actions[1] branch size MUST be 2

        int x = actionBuffers.DiscreteActions[0];
        int y = actionBuffers.DiscreteActions[1];
        int shouldMove = actionBuffers.DiscreteActions[2];


        if (shouldMove == 0)
        {
            var move = Piece.Rules.CreateMoveToPosition(x, y, Piece);
            if (move == null)
            {
                // Debug.LogError(Piece.pieceType + " " + Piece.CurrentSquare.Id + " could not create move to " + x + ", " + y);
                _AddReward(-0.1f);
                EventBus.Publish(new AgentActionEvent(this));
                OnActionFailed?.Invoke(this);
                return;
            }

            // Debug.Log(Piece.pieceType + " " + Piece.CurrentSquare.Id + " moving to " + x + ", " + y);

            Piece.MakeMove(move, moveSpeed);
            EventBus.Publish(new AgentActionEvent(this, move));
            OnActionSucceeded?.Invoke(this);

            if (move.IsCapture)
            {
                _AddReward(0.5f);
            }
            else if (move.IsPromotion)
            {
                _AddReward(1f);
            }
            else
            {
                _AddReward(0.1f);

            }
        }
        else
        {
            EventBus.Publish(new AgentActionEvent(this));
            OnActionFailed?.Invoke(this);
        }
        _AddReward(_existential); // punish for existence
    }

    public void _AddReward(float reward)
    {
        AddReward(reward);
        OnRewardChanged?.Invoke(GetCumulativeReward());
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (!Piece.IsControlling)
        {
            return;
        }

        var discreteActionsOut = actionsOut.DiscreteActions;

        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }

        // add int for the piece type
        // discreteActionsOut[3] = (int)Piece.pieceType;
    }

    public void Kill()
    {
        SetReward(-1f);
        EndEpisode();
    }

}
