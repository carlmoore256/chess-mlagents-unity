using UnityEngine;

public class ChessSettings : MonoBehaviour
{
    public float agentMoveSpeed;
    public float healthDecay = 0.01f;
    public float hitEnemyReward = 0.1f;
    public ChessPieces pieces;
    public float pieceValueMultiplier = 0.1f;
    public float teamWinReward = 1f;
}
