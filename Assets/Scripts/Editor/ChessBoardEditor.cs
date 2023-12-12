using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChessBoard))]
public class ChessBoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ChessBoard board = (ChessBoard)target;
        if (GUILayout.Button("Spawn Squares"))
        {
            board.SpawnSquares();
        }
    }
}