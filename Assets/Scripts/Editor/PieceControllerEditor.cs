using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PieceController))]
public class PieceControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PieceController controller = (PieceController)target;
        if (GUILayout.Button("Cycle Piece"))
        {
            controller.CyclePiece();
        }

        if (GUILayout.Button("Switch Team"))
        {
            controller.SwitchTeam();
        }
    }
}