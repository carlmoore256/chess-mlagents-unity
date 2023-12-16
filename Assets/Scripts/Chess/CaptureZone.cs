using System;
using System.Collections.Generic;
using UnityEngine;


public class CaptureZone : MonoBehaviour
{
    private List<Vector3> _piecePositions = new List<Vector3>();
    private Vector2 gridSize = new Vector2(10, 10); // Adjust grid size as needed
    private float pieceSize = 0.8f; // Adjust piece size as needed
    private int currentRow = 0;
    private int currentColumn = 0;

    public List<ChessPiece> Pieces = new List<ChessPiece>();

    public void AddPiece(ChessPiece piece)
    {
        piece.transform.SetParent(transform);
        piece.transform.localPosition = Vector3.zero;
        // piece.GetComponent<Rigidbody>().isKinematic = true;
        piece.GetComponent<Animation>().Play();
        // AssignNewPosition(piece);
        Pieces.Add(piece);
    }

    public void ReturnPiece(ChessPiece piece)
    {
        Pieces.Remove(piece);
        piece.transform.SetParent(null);
        piece.GetComponent<Animation>().Stop();
    }

    public void ReturnPieces(Action<ChessPiece> onReturnPiece = null)
    {
        foreach (var piece in Pieces)
        {
            piece.transform.SetParent(null);
            // piece.GetComponent<Rigidbody>().isKinematic = false;
            piece.GetComponent<Animation>().Stop();
            onReturnPiece?.Invoke(piece);
        }
        Pieces.Clear();
    }

    private void AssignNewPosition(ChessPiece piece)
    {
        if (currentColumn >= gridSize.x)
        {
            currentColumn = 0;
            currentRow++;
        }

        if (currentRow >= gridSize.y)
        {
            Debug.LogError("Capture zone is full!");
            return; // or handle the overflow as needed
        }

        Vector3 position = new Vector3(
            currentRow * pieceSize,
            0, // Assuming a flat capture zone; adjust if needed
            currentColumn * pieceSize
        );

        _piecePositions.Add(position);
        piece.transform.localPosition = position;
        // piece.transform.position += new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

        currentColumn++;
    }
}