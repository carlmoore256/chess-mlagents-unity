using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChessGame))]
public class ChessGameEditor : Editor
{
    private string squareId = "";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        squareId = EditorGUILayout.TextField("Square Id", squareId);

        ChessGame game = (ChessGame)target;
        if (GUILayout.Button("Load Starting Game"))
        {
            FindObjectOfType<ChessBoard>().SpawnSquares();
            game.LoadStartingGame();
        }

        if (GUILayout.Button("Move Random Piece") && !string.IsNullOrEmpty(squareId))
        {
            // var randomPiece = game.Pieces[Random.Range(0, game.Pieces.Count())];
            // var toSquare = game.Board.Squares[squareId];
            // randomPiece.MoveToSquare(toSquare);
        }

        if(GUILayout.Button("Create teams"))
        {
            game.CreateTeams();
        }
    }
}
