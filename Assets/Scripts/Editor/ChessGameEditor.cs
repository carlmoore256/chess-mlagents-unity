using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChessGame))]
public class ChessGameEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ChessGame game = (ChessGame)target;
        if (GUILayout.Button("Spawn Pieces"))
        {
            FindObjectOfType<ChessBoard>().SpawnSquares();
            game.SpawnPieces();
        }
    }
}