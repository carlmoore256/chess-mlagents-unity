using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(ChessPiece), typeof(Rigidbody))]
public class ChessAgent : Agent
{
    public ChessPiece Piece { get; private set; }
    public Team Team { get => Piece.team; }

    public Rigidbody agentRb;
    public float rotSign;
    public Vector3 initialPos;
    public Quaternion initialRot;
    public PieceType pieceType;

    private float _forwardSpeed = 0.5f;
    private float _lateralSpeed = 0.5f;

    private float _existential;
    private float _health = 1f;


    private ChessSettings _chessSettings;
    private BehaviorParameters _behaviorParameters;
    private EnvironmentParameters _resetParameters;
    private ChessEnvController _chessEnvController;
    private GameObject _piece;


    private void Awake()
    {
        // Debug.

        initialPos = transform.position;
        initialRot = transform.rotation;
        _chessSettings = FindObjectOfType<ChessSettings>();
        _chessEnvController = GetComponentInParent<ChessEnvController>();
        Piece = GetComponent<ChessPiece>();
        // _piece = _chessSettings.pieces.SpawnPiece(pieceType, Team, transform);
        _health = 1f;
        gameObject.SetActive(true);
        // Piece.OnInitialized += (piece) =>
        // {
        //     Debug.Log("CALLING PIECE ON INIT!!! " + piece.name);
        //     SetInitialTransform(transform.position, transform.rotation);
        // };
    }

    public void SetInitialTransform(Vector3 position, Quaternion rotation)
    {
        initialPos = position;
        initialRot = rotation;
    }

    public void Reset()
    {
        gameObject.SetActive(true);
        transform.position = initialPos;
        transform.rotation = initialRot;
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
        _health = 1f;
        Debug.Log("ChessAgent Initialized");
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

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * _chessSettings.agentMoveSpeed,
            ForceMode.VelocityChange);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        // add the type of piece as one hot encoded vector
        sensor.AddObservation(GetOneHotPieceType());

        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);
        sensor.AddObservation(agentRb.velocity);
        sensor.AddObservation(_health);
        // we can also add observations about what square its on
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (_health <= 0)
        {
            return;
        }

        switch (Piece.pieceType)
        {
            case PieceType.Pawn:
                // PawnAction(actionBuffers);
                break;
            case PieceType.Rook:
                // RookAction(actionBuffers);
                break;
            case PieceType.Knight:
                // KnightAction(actionBuffers);
                break;
            case PieceType.Bishop:
                // BishopAction(actionBuffers);
                break;
            case PieceType.Queen:
                // QueenAction(actionBuffers);
                break;
            case PieceType.King:
                // KingAction(actionBuffers);
                break;
        }

        // punish for not reaching the target
        AddReward(-_existential);
        MoveAgent(actionBuffers.DiscreteActions);
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

        if (collision.gameObject.TryGetComponent<ChessPiece>(out var otherChessPiece))
        {
            if (otherChessPiece.team != Team)
            {
                // determine if agent was moving forward towards enemy
                var forward = transform.forward;
                var direction = collision.transform.position - transform.position;
                var angle = Vector3.Angle(forward, direction);
                var speed = agentRb.velocity.magnitude;

                if (speed > 0.1f && angle < 90)
                {
                    if (otherChessPiece.pieceType == PieceType.King)
                    {
                        Debug.Log("ChessAgent Killed Enemy King");
                        AddReward(ChessPiece.PieceValue(otherChessPiece.pieceType));
                        _chessEnvController.TargetTouched(this);
                        EndEpisode();
                    }
                    else
                    {
                        // otherChessPiece.AddDamage(ChessPiece.PieceValue(otherChessPiece.pieceType));
                        Debug.Log("ChessAgent Hit Enemy");
                        AddReward(ChessPiece.PieceValue(otherChessPiece.pieceType) * 0.1f);
                        otherChessPiece.GetComponent<ChessAgent>().AddDamage(0.1f);
                        // AddReward(0.1f);
                    }
                }
                else
                {
                    AddReward(-0.1f);
                    _health -= _chessSettings.healthDecay;
                    if (_health <= 0)
                    {
                        Debug.Log("ChessAgent Died");
                        SetReward(-1f);
                        EndEpisode();
                    }
                }
                return;
            }
        }
    }
}
