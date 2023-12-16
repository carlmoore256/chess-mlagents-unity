using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ChessGame))]
public class ChessEnvController : MonoBehaviour
{
    public UnityEvent<Team> OnTargetTouched;

    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")]
    public int MaxEnvironmentSteps = 25000;

    //List of Agents On Platform
    // public List<ChessAgent> AgentsList;

    private ChessSettings _chessSettings;
    private SimpleMultiAgentGroup _whiteAgents;
    private SimpleMultiAgentGroup _blackAgents;
    public List<ChessAgent> allAgents = new List<ChessAgent>();
    private ChessGame _game;

    public ChessTeamAgent whiteTeamAgent;
    public ChessTeamAgent blackTeamAgent;

    private Team _currentTeam = Team.White;

    private int m_ResetTimer;

    public float DecisionDelay = 0.1f;

    void Start()
    {
        _game = GetComponent<ChessGame>();
        _chessSettings = FindObjectOfType<ChessSettings>();
        _whiteAgents = new SimpleMultiAgentGroup();
        _blackAgents = new SimpleMultiAgentGroup();


        RegisterTeam(Team.White);
        RegisterTeam(Team.Black);


        whiteTeamAgent = _game.Teams[Team.White].GetComponent<ChessTeamAgent>();
        blackTeamAgent = _game.Teams[Team.Black].GetComponent<ChessTeamAgent>();
    }

    private void RegisterTeam(Team team)
    {
        _game
            .Teams[team]
            .ActivePieces
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

        if (!allAgents.Contains(agent))
        {
            allAgents.Add(agent);
        }
    }

    public void NextTurn()
    {        
        Debug.Log("Next Turn" + _currentTeam);
        if (_currentTeam == Team.White)
        {
            whiteTeamAgent.RequestAction();
            _currentTeam = Team.Black;
        }
        else
        {
            blackTeamAgent.RequestAction();
            _currentTeam = Team.White;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextTurn();
        }

        if (Input.GetKeyDown(KeyCode.X))
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
        foreach (var item in allAgents)
        {
            item.Reset();
        }
    }
}
