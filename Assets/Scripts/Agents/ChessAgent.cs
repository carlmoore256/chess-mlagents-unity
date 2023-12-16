using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;


[RequireComponent(typeof(ChessPiece))]
public class ChessAgent : Agent
{
    public ChessPiece Piece { get; private set; }
    public Team Team
    {
        get => Piece.team;
    }

    public bool ShouldRequestDecision { get; set; }

    public float rotSign;
    public Vector3 initialPos;
    public Quaternion initialRot;
    public PieceType pieceType;

    public int numTopVectors = 5;

    private float _forwardSpeed = 0.5f;
    private float _lateralSpeed = 0.5f;

    private float _existential = 0.01f;
    private float _health = 1f;

    private ChessSettings _chessSettings;
    private BehaviorParameters _behaviorParameters;
    private EnvironmentParameters _resetParameters;
    private ChessEnvController _chessEnvController;
    private List<Move> _nextMoves;
    private bool _isInitialized = false;

    private void Awake()
    {
        // initialPos = transform.position;
        // initialRot = transform.rotation;
        _chessSettings = FindObjectOfType<ChessSettings>();
        _chessEnvController = GetComponentInParent<ChessEnvController>();
        Piece = GetComponent<ChessPiece>();
        _health = 1f;
        gameObject.SetActive(true);
        Debug.Log("ChessAgent Awake");
        Piece.OnInitialized += (p) =>
        {
            pieceType = p.pieceType;
            _isInitialized = true;
            Piece.ResetToStartingPosition();
        };

        // _nextMoves = Piece.GetValidMoves();
    }

    public void SetInitialTransform(Vector3 position, Quaternion rotation)
    {
        initialPos = position;
        initialRot = rotation;
    }

    public void Reset()
    {
        gameObject.SetActive(true);
        Piece.ResetToStartingPosition();
        _health = 1f;
    }

    private float[] GetOneHotPieceType()
    {
        return ChessDataHelpers.GetOneHotPieceVector(pieceType);
    }

    public override void Initialize()
    {
        if (!_isInitialized)
        {
            Debug.LogError("[Initialize] ChessAgent FAILED to initialize!");
            return;
        }
        _health = 1f;
        Debug.Log("[Initialize] ChessAgent Initialized");
        if (_chessEnvController != null)
        {
            _existential = 1f / _chessEnvController.MaxEnvironmentSteps;
        }
        else
        {
            _existential = 1f / MaxStep;
        }

        // initialPos = transform.position;

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


        Piece.OnCaptured += (p) =>
        {
            Debug.Log("ChessAgent Captured");
            Kill();
        };
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
        sensor.AddObservation(GetOneHotPieceType());

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
        
        if (_nextMoves == null)
        {
            _nextMoves = new List<Move>();
        }

        _nextMoves.Clear();
        _nextMoves.AddRange(moves.GetRange(0, Mathf.Min(numTopVectors, moves.Count)));

        Debug.Log("Added " + _nextMoves.Count + " moves to _nextMoves for " + Piece.pieceType + " " + Piece.CurrentSquare.Id);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {   
        Debug.Log("OnActionReceived for " + Piece.pieceType + " " + Piece.CurrentSquare.Id);

        if (!_isInitialized)
        {
            Debug.LogError("[OnActionReceived] ChessAgent not initialized");
            return;
        }

        // model will select a move from the top numTopVector moves, which will be discreteActions[0]
        int selectedMoveIndex = actionBuffers.DiscreteActions[0]; // discrete actions[0] branch size MUST be numTopVectors
        int shouldMove = actionBuffers.DiscreteActions[1]; // discrete actions[1] branch size MUST be 2

        if (_nextMoves == null) {
            _nextMoves = Piece.GetValidMoves();
        }

        if (shouldMove == 0)
        {
            if (_nextMoves.Count == 0)
            {
                Debug.LogError("No moves available, punishing");
                AddReward(-0.1f);
                return;
            }

            if (selectedMoveIndex >= _nextMoves.Count)
            {
                Debug.LogError(
                    $"Selected move index {selectedMoveIndex} is out of range for next moves"
                );
                AddReward(-0.1f);
                return;
            }
            Move move = _nextMoves[selectedMoveIndex];
            Piece.MakeMove(move);
            EventBus.Publish(new MovePieceEvent(Piece, move));
        }
    
        AddReward(-_existential); // punish for existence
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
