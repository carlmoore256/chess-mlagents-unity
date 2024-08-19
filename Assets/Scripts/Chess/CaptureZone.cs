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

    public bool isRenderingEnabled = false;

    public List<ChessPiece> Pieces = new List<ChessPiece>();


    private void OnRenderingEnabledChanged(bool enabled)
    {
        if (enabled)
        {
            foreach (var piece in Pieces)
            {
                // piece.gameObject.
            }
        }
    }

    public void AddPiece(ChessPiece piece)
    {
        piece.transform.SetParent(transform);
        piece.transform.localPosition = Vector3.zero;
        if (!isRenderingEnabled)
        {
            TogglePieceRendering(piece, false);
        }
        else
        {
            piece.GetComponent<Animation>().Play();

        }
        Pieces.Add(piece);
    }

    private void TogglePieceRendering(ChessPiece piece, bool enabled)
    {
        var meshRenderers = piece.gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in meshRenderers)
        {
            renderer.enabled = enabled;
        }

        var canvasRenderers = piece.gameObject.GetComponentsInChildren<Canvas>();
        foreach (var renderer in canvasRenderers)
        {
            renderer.enabled = enabled;
        }
    }

    public void ReturnPiece(ChessPiece piece, bool remove = true)
    {
        if (!isRenderingEnabled)
        {
            TogglePieceRendering(piece, true);
        }
        else
        {
            piece.GetComponent<Animation>().Stop();
        }
        if (remove)
        {
            Pieces.Remove(piece);

        }
        piece.transform.SetParent(null);
    }

    public void ReturnPieces(Action<ChessPiece> onReturnPiece = null)
    {
        foreach (var piece in Pieces)
        {
            ReturnPiece(piece, false);
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