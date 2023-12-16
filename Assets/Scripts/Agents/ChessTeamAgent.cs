using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ChessTeamAgent : Agent
{
    private ChessTeam _team;

    // returns a float vector of 8x8 containing float values of the value of each piece
    private float[] GetPiecesBoardVector()
    {
        float[] piecesBoardVector = new float[64];

        foreach (ChessPiece piece in _team.ActivePieces)
        {
            if (piece.IsCaptured)
            {
                continue;
            }

            var position = piece.CurrentSquare.Position;
            int x = (int)position.x;
            int y = (int)position.y;

            piecesBoardVector[x + y * 8] = piece.Value / 10f;
        }

        return piecesBoardVector;
    }

    private void Start()
    {
        _team = GetComponent<ChessTeam>();
    }

    public override void OnEpisodeBegin() { }

    public override void CollectObservations(VectorSensor sensor)
    {
        var vector = GetPiecesBoardVector();
        Debug.Log("Collecting Observations for " + _team.Team + " team");
        sensor.AddObservation(vector);
        Debug.Log(vector);
        
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("OnActionReceived for " + _team.Team + " team");

        // here we select which piece to move
        ChessPiece pieceToMove = null;
        try {
            pieceToMove = _team.GetPieceAtIndex(actions.DiscreteActions[0]);
        } catch (System.Exception e) {
            Debug.Log(e);
        }

        pieceToMove.GetComponent<ChessAgent>().RequestAction();
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
