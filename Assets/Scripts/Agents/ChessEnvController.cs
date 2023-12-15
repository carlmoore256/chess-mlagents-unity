using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ChessGame))]
public class ChessEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public ChessAgent Agent;

        [HideInInspector]
        public Vector3 StartingPos;

        [HideInInspector]
        public Quaternion StartingRot;

        [HideInInspector]
        public Rigidbody Rb;
    }

    public UnityEvent<Team> OnTargetTouched;

    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")]
    public int MaxEnvironmentSteps = 25000;

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>


    //List of Agents On Platform
    // public List<ChessAgent> AgentsList;

    private ChessSettings _chessSettings;

    private SimpleMultiAgentGroup _whiteAgents;
    private SimpleMultiAgentGroup _blackAgents;

    private ChessGame _game;

    private int _agentIndex = 0;

    private int m_ResetTimer;

    public float DecisionDelay = 0.1f;

    void Start()
    {
        _game = GetComponent<ChessGame>();
        _chessSettings = FindObjectOfType<ChessSettings>();
        _whiteAgents = new SimpleMultiAgentGroup();
        _blackAgents = new SimpleMultiAgentGroup();

        _game
            .WhiteTeam
            .Pieces
            .ForEach(
                (piece) =>
                {
                    var agent = piece.GetComponent<ChessAgent>();
                    if (agent != null)
                    {
                        _whiteAgents.RegisterAgent(agent);
                    }
                }
            );

        // ResetScene();
    }

    private void RegisterTeam(Team team)
    {
        _game
            .Teams[team]
            .Pieces
            .ForEach(
                (piece) =>
                {
                    RegisterChessPiece(piece);
                }
            );
    }

    public void RegisterChessPiece(ChessPiece piece)
    {
        var agent = piece.GetComponent<ChessAgent>();
        if (agent != null)
        {
            if (piece.team == Team.White)
            {
                _whiteAgents.RegisterAgent(agent);
            }
            else
            {
                _blackAgents.RegisterAgent(agent);
            }
        }

        piece.OnCaptured += (p) =>
        {
            var pieceValue = p.Value * _chessSettings.pieceValueMultiplier;
            if (p.team == Team.White)
            {
                _whiteAgents.AddGroupReward(-pieceValue);
            }
            else
            {
                _blackAgents.AddGroupReward(-pieceValue);
            }
        };
    }

    public void NextTurn()
    {
        // var agents = AgentsList.FindAll(x => x.Piece.pieceType == PieceType.Pawn);
        // AgentsList[_agentIndex].RequestDecision();
        // if (agents.Count() == 0)
        // {
        //     return;
        // }
        // agents.ElementAt(_agentIndex).RequestDecision();
        // _agentIndex++;
        // if (_agentIndex >= agents.Count())
        // {
        //     _agentIndex = 0;
        // }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            NextTurn();
        }

        if (Input.GetKey(KeyCode.X))
        {
            EndEpisode();
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<MovePieceEvent>(OnMovePiece);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<MovePieceEvent>(OnMovePiece);
    }

    private void OnMovePiece(MovePieceEvent e)
    {
        // if (e.Piece.IsCaptured)
        // {
        // if (e.Piece.team == Team.White)
        // {
        //     m_WhiteAgentGroup.AddGroupReward(-0.1f);
        // }
        // else
        // {
        //     m_BlackAgentGroup.AddGroupReward(-0.1f);
        // }
        // }
    }

    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            _whiteAgents.GroupEpisodeInterrupted();
            _blackAgents.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    public void TargetTouched(ChessAgent agent)
    {
        if (agent.Team == Team.White)
        {
            _whiteAgents.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            _blackAgents.AddGroupReward(-1);
        }
        else
        {
            _blackAgents.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            _whiteAgents.AddGroupReward(-1);
        }
        _blackAgents.EndGroupEpisode();
        _whiteAgents.EndGroupEpisode();
        OnTargetTouched?.Invoke(agent.Team);
        ResetScene();
    }

    public void EndEpisode(float whiteReward = -1f, float blackReward = -1f)
    {
        _whiteAgents.AddGroupReward(whiteReward);
        _blackAgents.AddGroupReward(blackReward);
        _whiteAgents.EndGroupEpisode();
        _blackAgents.EndGroupEpisode();
        ResetScene();
    }

    public void ResetScene()
    {
        m_ResetTimer = 0;
        Debug.Log("Resetting Scene");

        // Reset Agents
        // foreach (var item in AgentsList)
        // {
        //     item.Reset();
        // }
    }
}
