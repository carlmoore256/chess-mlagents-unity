using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Events;

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
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>


    //List of Agents On Platform
    public List<ChessAgent> AgentsList = new List<ChessAgent>();

    private ChessSettings m_ChessSettings;


    private SimpleMultiAgentGroup m_WhiteAgentGroup;
    private SimpleMultiAgentGroup m_BlackAgentGroup;

    private int m_ResetTimer;

    void Start()
    {
        m_ChessSettings = FindObjectOfType<ChessSettings>();
        m_WhiteAgentGroup = new SimpleMultiAgentGroup();
        m_BlackAgentGroup = new SimpleMultiAgentGroup();
        AgentsList.AddRange(FindObjectsOfType<ChessAgent>());
        foreach (var item in AgentsList)
        {
            if (item.Team == Team.White)
            {
                m_WhiteAgentGroup.RegisterAgent(item);
            }
            else
            {
                m_BlackAgentGroup.RegisterAgent(item);
            }
        }
        // ResetScene();
    }

    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_WhiteAgentGroup.GroupEpisodeInterrupted();
            m_BlackAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    public void TargetTouched(ChessAgent agent)
    {
        if (agent.Team == Team.White)
        {
            m_WhiteAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_BlackAgentGroup.AddGroupReward(-1);
        }
        else
        {
            m_BlackAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_WhiteAgentGroup.AddGroupReward(-1);
        }
        m_BlackAgentGroup.EndGroupEpisode();
        m_WhiteAgentGroup.EndGroupEpisode();
        OnTargetTouched?.Invoke(agent.Team);
        ResetScene();
    }

    public void EndEpisode(float whiteReward = -1f, float blackReward = -1f)
    {
        m_WhiteAgentGroup.AddGroupReward(whiteReward);
        m_BlackAgentGroup.AddGroupReward(blackReward);
        m_WhiteAgentGroup.EndGroupEpisode();
        m_BlackAgentGroup.EndGroupEpisode();
        ResetScene();
    }


    public void ResetScene()
    {
        m_ResetTimer = 0;
        Debug.Log("Resetting Scene");

        // Reset Agents
        foreach (var item in AgentsList)
        {
            item.Reset();
        }
    }
}
