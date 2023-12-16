using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class ChessTeam : MonoBehaviour
{
    public Team Team;
    public List<ChessPiece> ActivePieces = new List<ChessPiece>();
    public List<ChessPiece> AllPieces { get; private set; } = new List<ChessPiece>();
    private ChessRules _rules;
    private ChessBoard _board;
    private GameObject _piecePrefab;
    public GameObject capturedZonePrefab;
    private CaptureZone _captureZone;
    private float _captureZoneOffset = 9f;
    
    public void Initialize(Team team, ChessRules rules, ChessBoard board, GameObject piecePrefab)
    {
        Team = team;
        _rules = rules;
        _board = board;
        _piecePrefab = piecePrefab;

        GameObject captureZoneObj;
        if (capturedZonePrefab != null)
        {
            captureZoneObj = Instantiate(capturedZonePrefab, transform);
            captureZoneObj.transform.position =
                Team.Black == team
                    ? new Vector3(_captureZoneOffset, 0, 0)
                    : new Vector3(-_captureZoneOffset, 0, 0);
        }
        else
        {
            captureZoneObj = new GameObject("Capture Zone");
            captureZoneObj.transform.parent = transform;
            captureZoneObj.transform.localPosition =
                Team.Black == team
                    ? new Vector3(_captureZoneOffset, 0, 0)
                    : new Vector3(-_captureZoneOffset, 0, 0);
        }

        _captureZone = captureZoneObj.GetOrAddComponent<CaptureZone>();
        // _captureZone.transform.position =
        //     Team.Black == team ? new Vector3(0, 0, -4.5f) : new Vector3(0, 0, 4.5f);
    }

    public ChessPiece GetPieceAtIndex(int index)
    {
        return AllPieces.Where(p => p.TeamPieceIndex == index).FirstOrDefault();
    }

    public void ResetCaptures()
    {
        _captureZone.ReturnPieces(
            (piece) =>
            {
                piece.transform.parent = transform;
                piece.IsCaptured = false;
            }
        );
    }

    private void OnDestroy()
    {
        if (_captureZone != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(_captureZone);
#else
            Destroy(_capturedZone);
#endif
        }
    }

    public void ClearPieces()
    {
        var pieceObjects = GetComponentsInChildren<ChessPiece>();
        foreach (var piece in pieceObjects)
        {
#if UNITY_EDITOR
            DestroyImmediate(piece.gameObject);
#else
            Destroy(piece.gameObject);
#endif
        }
        ActivePieces.Clear();
    }

    public void SpawnPiece(PieceType pieceType, string squareId, int pieceIndex = 0)
    {
        var square = _board.Squares[squareId];
        var pieceObject = Instantiate(_piecePrefab, transform);
        var chessPiece =
            pieceObject.GetComponent<ChessPiece>()
            ?? throw new System.Exception("ChessPiece component not found on prefab");
        chessPiece.Initialize(pieceType, Team, _rules, square, null, pieceIndex);
        ActivePieces.Add(chessPiece);
        AllPieces.Add(chessPiece);

        chessPiece.OnCaptured += OnPieceCaptured;
        chessPiece.OnReset += (piece) =>
        {
            _captureZone.ReturnPiece(piece);
            piece.transform.parent = transform;
            ActivePieces.Add(piece);
        };
    }

    private void OnPieceCaptured(ChessPiece piece)
    {
        ActivePieces.Remove(piece);
        _captureZone.AddPiece(piece);
    }

    public void SpawnPieces(List<PiecePlacement> placements)
    {
        int pieceIndex = 0;
        foreach (var placement in placements)
        {
            if (placement.Team == Team)
            {
                SpawnPiece(placement.PieceType, placement.SquareId, pieceIndex);
            }
            pieceIndex++;
        }
    }

    public void MoveRandomPiece()
    {
        List<int> indices = Enumerable.Range(0, ActivePieces.Count).ToList();

        while (indices.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, indices.Count);
            int pieceIndex = indices[randomIndex];
            indices.RemoveAt(randomIndex); // Remove the chosen index

            ChessPiece piece = ActivePieces[pieceIndex];
            var moves = piece.GetValidMoves();
            if (moves.Count > 0)
            {
                var move = moves[UnityEngine.Random.Range(0, moves.Count)];
                piece.MakeMove(move);
                return;
            }
        }

        Debug.Log("No valid moves found");
    }

    public void MoveRandomPiece(PieceType type)
    {
        List<int> indices = Enumerable.Range(0, ActivePieces.Count).ToList();

        while (indices.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, indices.Count);
            int pieceIndex = indices[randomIndex];
            indices.RemoveAt(randomIndex); // Remove the chosen index

            ChessPiece piece = ActivePieces[pieceIndex];
            if (piece.pieceType == type)
            {
                var moves = piece.GetValidMoves();
                if (moves.Count > 0)
                {
                    var move = moves[UnityEngine.Random.Range(0, moves.Count)];
                    piece.MakeMove(move);
                    return;
                }
            }
        }

        Debug.Log("No valid moves found");
    }

    public static Team OpposingTeam(Team team) => team == Team.White ? Team.Black : Team.White;
}
