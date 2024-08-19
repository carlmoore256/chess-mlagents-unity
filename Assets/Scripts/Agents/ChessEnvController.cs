using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

    private ChessSettings _chessSettings;
    private ChessGame _game;
    public ChessTeamAgent whiteTeamAgent;
    public ChessTeamAgent blackTeamAgent;
    private int _currentStep = 0;
    private Team _currentTeam = Team.White;
    public bool IsTraining { get; private set; } = false;

    private bool _nextStepRequested = false;

    public bool enableTraining = true;

    private bool _hasStepped = false;

    public void RequestNextStep()
    {
        _nextStepRequested = true;
    }

    void Start()
    {
        _game = GetComponent<ChessGame>();
        _chessSettings = FindObjectOfType<ChessSettings>();

        Academy.Instance.AutomaticSteppingEnabled = false;

        whiteTeamAgent = _game.Teams[Team.White].GetComponent<ChessTeamAgent>();
        blackTeamAgent = _game.Teams[Team.Black].GetComponent<ChessTeamAgent>();

        whiteTeamAgent.OnTurnEnded += TeamAgentStep;
        blackTeamAgent.OnTurnEnded += TeamAgentStep;
        whiteTeamAgent.OnRequestStep += (agent) => RequestNextStep();
        blackTeamAgent.OnRequestStep += (agent) => RequestNextStep();
        whiteTeamAgent.OnRequestEpisodeEnd += HandleEndEpisode;
        blackTeamAgent.OnRequestEpisodeEnd += HandleEndEpisode;
    }

    private void HandleEndEpisode(ChessTeamAgent endingAgent)
    {
        blackTeamAgent._EndEpisode(_chessSettings.teamWinReward);
        whiteTeamAgent._EndEpisode(_chessSettings.teamWinReward);
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        _currentStep = 0;
        _currentTeam = Team.White;
        whiteTeamAgent.RequestDecision();
        // Academy.Instance.EnvironmentStep();
    }

    private void TeamAgentStep(ChessTeamAgent teamAgent)
    {
        Debug.Log("Team Agent Step: " + teamAgent.Team);
        _currentTeam = ChessTeam.OpposingTeam(teamAgent.Team);
        // Academy.Instance.EnvironmentStep();
        // teamAgent.RequestDecision();

        if (_currentTeam == Team.White)
        {
            whiteTeamAgent.RequestDecision();
        }
        else
        {
            blackTeamAgent.RequestDecision();
        }

        _currentStep++;

        if (_currentStep >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            ResetScene();
        }
    }

    private void StepForward()
    {
        Debug.Log("Step Forward");
        if (!whiteTeamAgent.IsReady || !blackTeamAgent.IsReady)
        {
            InitializeTeams();
        }

        TeamAgentStep(_currentTeam == Team.White ? whiteTeamAgent : blackTeamAgent);
        // if (_currentTeam == Team.White)
        // {
        //     whiteTeamAgent.RequestDecision();
        // }
        // else
        // {
        //     blackTeamAgent.RequestDecision();
        // }

        _hasStepped = false;

        Academy.Instance.EnvironmentStep();
        _currentTeam = ChessTeam.OpposingTeam(_currentTeam);
    }

    private void InitializeTeams()
    {
        whiteTeamAgent.IsReady = true;
        blackTeamAgent.IsReady = true;
        whiteTeamAgent.Initialize();
        blackTeamAgent.Initialize();
    }

    private void StartTraining()
    {
        Debug.Log("Start Training");
        IsTraining = true;
        InitializeTeams();
        whiteTeamAgent.RequestDecision();
        Academy.Instance.EnvironmentStep();
    }

    private void Update()
    {
        if (!IsTraining && _game.IsInitialized && enableTraining)
        {
            StartTraining();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StepForward();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ResetScene();
        }

        if (_nextStepRequested)
        {
            Academy.Instance.EnvironmentStep();
            // _nextStepRequested = false;
        }
    }

    public void ResetScene()
    {
        Debug.Log("Resetting Scene");
        _currentStep = 0;
        whiteTeamAgent._EndEpisode();
        blackTeamAgent._EndEpisode();
        _currentTeam = Team.White;
    }
}

// void FixedUpdate()
// {
//     m_ResetTimer += 1;
//     if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
//     {
//         _whiteAgents.GroupEpisodeInterrupted();
//         _blackAgents.GroupEpisodeInterrupted();
//         ResetScene();
//     }
// }
