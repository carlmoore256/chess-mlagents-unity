using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    public GameObject squarePrefab;
    public Vector3 positionOffset = new Vector3(0, -0.1f, 0);
    public Dictionary<string, ChessSquare> Squares = new Dictionary<string, ChessSquare>();

    private void SpawnSquare(int x, int y)
    {
        GameObject square = Instantiate(squarePrefab, transform);
        float tileSize = square.transform.localScale.x;
        var chessSquare = square.GetComponent<ChessSquare>();
        chessSquare.Initialize(
            $"{(char)('a' + x)}{y + 1}",
            new Vector3(x * tileSize, 0, y * tileSize),
            (x + y) % 2 == 1 ? Team.White : Team.Black
        );
        Squares.Add(chessSquare.Id, chessSquare);
    }

    public void SpawnSquares()
    {
        ClearSquares();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; ++y)
            {
                SpawnSquare(x, y);
            }
        }
        // find the center
        float center =
            (Squares["a1"].transform.localPosition.x + Squares["h8"].transform.localPosition.x)
            / 2f;
        // move the board so that the center is at the origin
        transform.localPosition = new Vector3(-center, 0, -center) + positionOffset;
        // spawn a cube that is the same height as a square, but the same width as all squares,
        // which will be the physics collider for the board
        var boardCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boardCollider.transform.SetParent(transform);

        boardCollider.transform.localScale = new Vector3(
            Squares["h8"].transform.localPosition.x
                - Squares["a1"].transform.localPosition.x
                + Squares["h8"].transform.localScale.x,
            Squares["h8"].transform.localScale.y,
            Squares["h8"].transform.localPosition.z
                - Squares["a1"].transform.localPosition.z
                + Squares["h8"].transform.localScale.z
        );

        boardCollider.transform.localPosition = new Vector3(
            Squares["a1"].transform.localPosition.x
                + boardCollider.transform.localScale.x / 2f
                - Squares["a1"].transform.localScale.x / 2f,
            Squares["a1"].transform.localPosition.y,
            Squares["a1"].transform.localPosition.z
                + boardCollider.transform.localScale.z / 2f
                - Squares["a1"].transform.localScale.z / 2f
        );

        boardCollider.GetComponent<MeshRenderer>().enabled = false;
        boardCollider.name = "BoardCollider";
    }

    private void ClearSquares()
    {
        foreach (var square in Squares.Values)
        {
#if UNITY_EDITOR
            try
            {
                DestroyImmediate(square.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
#else
            Destroy(square.gameObject);
#endif
        }
        Squares.Clear();

        var squareObjects = GetComponentsInChildren<ChessSquare>();
        foreach (var square in squareObjects)
        {
#if UNITY_EDITOR
            DestroyImmediate(square.gameObject);
#else
            Destroy(square.gameObject);
#endif
        }

        var boardCollider = transform.Find("BoardCollider");
        if (boardCollider != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(boardCollider.gameObject);
#else
            Destroy(boardCollider.gameObject);
#endif
        }
    }

    // public ChessPiece SpawnPieceAtSquare(
    //     GameObject piecePrefab,
    //     string squareId,
    //     PieceType pieceType,
    //     Team team,
    //     Transform parent = null
    // )
    // {
    //     var square = Squares[squareId];
    //     var pieceObject = Instantiate(piecePrefab, parent);
    //     var chessPiece = pieceObject.GetComponent<ChessPiece>();
    //     chessPiece.Initialize(pieceType, team, null, square);
    //     return chessPiece;
    // }
}
