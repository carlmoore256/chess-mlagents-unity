using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;


[RequireComponent(typeof(ChessPiece), typeof(Rigidbody))]
public class ChessAgent : Agent
{
    public ChessPiece Piece { get; private set; }
    public Team Team
    {
        get => Piece.team;
    }

    public bool ShouldRequestDecision { get; set; }

    public Rigidbody agentRb;
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
    private List<Move> _nextMoves = new List<Move>();
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
            // SetInitialTransform(transform.position, transform.rotation);
        };
    }

    // private void Update()
    // {
    //     if (ShouldRequestDecision)
    //     {
    //         RequestDecision();
    //         ShouldRequestDecision = false;
    //     }
    // }

    public void SetInitialTransform(Vector3 position, Quaternion rotation)
    {
        initialPos = position;
        initialRot = rotation;
    }

    public void Reset()
    {
        gameObject.SetActive(true);
        Piece.ResetToStartingPosition();

        agentRb.velocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        _health = 1f;
    }

    private float[] GetOneHotPieceType()
    {
        float[] oneHot = new float[6]; // Adjust the size based on the number of piece types
        oneHot[(int)pieceType] = 1f;
        return oneHot;
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
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;
        _resetParameters = Academy.Instance.EnvironmentParameters;
    }

    public override void OnEpisodeBegin()
    {
        if (gameObject.activeSelf == false && _health <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            Reset();
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var moveForward = act[0] == 1; // Additional action to determine forward movement
        var actionType = act[1]; // Action type specific to the piece

        switch (Piece.pieceType)
        {
            case PieceType.Pawn:
                PawnAction(moveForward, actionType);
                break;
            // case PieceType.Rook:
            //     RookAction(moveForward, actionType);
            //     break;
            // case PieceType.Knight:
            //     KnightAction(moveForward, actionType);
            //     break;
            // case PieceType.Bishop:
            //     BishopAction(moveForward, actionType);
            //     break;
            // case PieceType.Queen:
            //     QueenAction(moveForward, actionType);
            //     break;
            // case PieceType.King:
            //     KingAction(moveForward, actionType);
            //     break;
        }
    }

    private void PawnAction(bool moveForward, int actionType)
    {
        // Implement logic based on actionType for Pawn
        // Example: actionType could define the direction of diagonal capture
        if (moveForward)
        {
            agentRb.MovePosition(
                transform.position + Vector3.forward * _forwardSpeed * Time.fixedDeltaTime
            );
        }
        // Additional logic for diagonal capture or other actions
    }

    public void MoveAgentFree(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * _forwardSpeed;
                break;
            case 2:
                dirToGo = transform.forward * -_forwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * _lateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -_lateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        // quantize rotation to 90 degree increments

        // transform.Rotate(rotateDir, Time.deltaTime * 100f);
        // agentRb.AddForce(dirToGo * _chessSettings.agentMoveSpeed,
        //     ForceMode.VelocityChange);
    }

    // private float[] GetAgentBoardState()
    // {
    //     float[] state = new float[64]; // Adjust the size based on the number of squares
    //     var moves = Piece.GetValidMoves();
    //     state[0] = moves.Count / ChessRules.PiecesMeanMoves[Piece.pieceType];
    //     var captureMoves = moves.FindAll(m => m.IsCapture);
    //     state[1] = captureMoves.Count / ChessRules.PiecesMeanCaptures[Piece.pieceType];
    //     return state;
    // }


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
            // Kill();
            return;
        }

        // add the type of piece as one hot encoded vector
        sensor.AddObservation(GetOneHotPieceType());

        // add the normalized position of the piece on the board
        sensor.AddObservation(ChessRules.SquareIdToNormalizedPosition(Piece.CurrentSquare.Id));

        var moves = Piece.GetValidMoves();
        foreach (var move in moves)
        {
            Debug.Log("Adding move to sensor: " + move);
            // sensor.AddObservation(ChessDataHelpers.MoveToVector(move));
        }

        // record how many moves we have normalized by the mean moves usually available for that piece type
        sensor.AddObservation(moves.Count / ChessRules.PiecesMeanMoves[Piece.pieceType]);

        var captureMoves = moves.FindAll(m => m.IsCapture);
        sensor.AddObservation(captureMoves.Count / ChessRules.PiecesMeanCaptures[Piece.pieceType]);

        // order the moves by the value of captures first
        moves.Sort((a, b) => b.CaptureValue.CompareTo(a.CaptureValue));

        var moveVectors = ChessDataHelpers.GetStackedMoveVectors(moves, numTopVectors);
        sensor.AddObservation(moveVectors);
        _nextMoves.Clear();
        _nextMoves.AddRange(moves.GetRange(0, Mathf.Min(numTopVectors, moves.Count)));

        Debug.Log("Added " + _nextMoves.Count + " moves to _nextMoves for " + Piece.pieceType + " " + Piece.CurrentSquare.Id);

        // sensor.AddObservation(agentRb.velocity);
        // sensor.AddObservation(_health);
        // we can also add observations about what square its on
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        if (!_isInitialized)
        {
            Debug.LogError("[OnActionReceived] ChessAgent not initialized");
            return;
        }

        if (_health <= 0)
        {
            return;
        }

        // model will select a move from the top numTopVector moves, which will be discreteActions[0]
        int selectedMoveIndex = actionBuffers.DiscreteActions[0]; // discrete actions[0] branch size MUST be numTopVectors
        int shouldMove = actionBuffers.DiscreteActions[1]; // discrete actions[1] branch size MUST be 2



        if (shouldMove == 1)
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
            if (move.CapturedPiece)
            {
                // I can actually simply say here, onDestroy, give a negative reward?
                var otherChessAgent = move.CapturedPiece.GetComponent<ChessAgent>();
                if (otherChessAgent)
                {
                    otherChessAgent.Kill();
                }
                else
                {
                    Destroy(move.CapturedPiece.gameObject);
                }
            }

            EventBus.Publish(new MovePieceEvent(Piece, move));
            // Piece.MoveToSquare(move.ToSquare);
        }
        
        // punish for existence
        AddReward(-_existential);

        // switch (Piece.pieceType)
        // {
        //     case PieceType.Pawn:
        //         // PawnAction(actionBuffers);
        //         break;
        //     case PieceType.Rook:
        //         // RookAction(actionBuffers);
        //         break;
        //     case PieceType.Knight:
        //         // KnightAction(actionBuffers);
        //         break;
        //     case PieceType.Bishop:
        //         // BishopAction(actionBuffers);
        //         break;
        //     case PieceType.Queen:
        //         // QueenAction(actionBuffers);
        //         break;
        //     case PieceType.King:
        //         // KingAction(actionBuffers);
        //         break;
        // }
        // MoveAgent(actionBuffers.DiscreteActions);
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

    // private void Update()
    // {
    //     if (transform.position.y < -1)
    //     {
    //         Debug.Log("ChessAgent Fell");
    //         SetReward(-1f);
    //         EndEpisode();
    //     }
    // }

    // private void Update()
    // {
    //     // how to get its own observations
    //     var test = this.GetObservations();
    //     GetStoredActionBuffers();
    // }

    public void Kill()
    {
        _health = 0f;
        SetReward(-1f);
        gameObject.SetActive(false);
        EndEpisode();
    }

    public bool AddDamage(float damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            Kill();
            return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("boundary"))
        {
            if (Piece.pieceType == PieceType.King)
            {
                _chessEnvController.EndEpisode();
            }
            else
            {
                Kill();
            }
            return;
        }

        // if (collision.gameObject.TryGetComponent<ChessPiece>(out var otherChessPiece))
        // {
        //     if (otherChessPiece.team != Team)
        //     {
        //         // determine if agent was moving forward towards enemy
        //         var forward = transform.forward;
        //         var direction = collision.transform.position - transform.position;
        //         var angle = Vector3.Angle(forward, direction);
        //         var speed = agentRb.velocity.magnitude;

        //         if (speed > 0.1f && angle < 90)
        //         {
        //             if (otherChessPiece.pieceType == PieceType.King)
        //             {
        //                 Debug.Log("ChessAgent Killed Enemy King");
        //                 AddReward(ChessPiece.PieceValue(otherChessPiece.pieceType));
        //                 _chessEnvController.TargetTouched(this);
        //                 EndEpisode();
        //             }
        //             else
        //             {
        //                 // otherChessPiece.AddDamage(ChessPiece.PieceValue(otherChessPiece.pieceType));
        //                 Debug.Log("ChessAgent Hit Enemy");
        //                 AddReward(ChessPiece.PieceValue(otherChessPiece.pieceType) * 0.1f);
        //                 otherChessPiece.GetComponent<ChessAgent>().AddDamage(0.1f);
        //                 // AddReward(0.1f);
        //             }
        //         }
        //         else
        //         {
        //             AddReward(-0.1f);
        //             _health -= _chessSettings.healthDecay;
        //             if (_health <= 0)
        //             {
        //                 Debug.Log("ChessAgent Died");
        //                 SetReward(-1f);
        //                 EndEpisode();
        //             }
        //         }
        //         return;
        //     }
        // }
    }
}
