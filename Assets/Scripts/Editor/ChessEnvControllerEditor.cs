using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ChessEnvController))]
public class ChessEnvControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ChessEnvController controller = (ChessEnvController)target;
        // if (GUILayout.Button("Reset"))
        // {
        //     controller.Reset();
        // }

        if (GUILayout.Button("Next Turn"))
        {
            controller.NextTurn();
        }
    }
}