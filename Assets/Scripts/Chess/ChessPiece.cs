using System;
using Unity.MLAgents.Actuators;
using UnityEngine;

[System.Serializable]
public enum PieceType
{
    Pawn = 0,
    Rook = 1,
    Knight = 2,
    Bishop = 3,
    Queen = 4,
    King = 5
} 

[RequireComponent(typeof(MeshCollider))]
public class ChessPiece : MonoBehaviour
{
    public PieceType pieceType;
    public Team team;
    public ChessSquare CurrentSquare { get; private set; }
    public bool HasMoved { get; private set; }
    public bool IsPromoted { get; private set; }
    public bool IsCaptured { get; set; }
    public bool IsControlling
    {
        get => _isControlling; set
        {
            if (povCamera != null)
            {
                povCamera.gameObject.SetActive(value);
            }
            _isControlling = value;
        }
    }

    public ChessPieces chessPieces;
    public Vector3 pieceOffset = new Vector3(0, 0.1f, 0);

    private bool _isControlling = false;
    private Coroutine _moveCoroutine;
    private Transform _pieceModel;
    public Camera povCamera;
    public Action<ChessPiece> OnInitialized;

    private void OnEnable()
    {
        if (povCamera == null)
        {
            povCamera = GetComponentInChildren<Camera>();
            if (povCamera == null)
            {
                return;
            }
        }
        povCamera.gameObject.SetActive(IsControlling);
    }

    public void Initialize(PieceType pieceType, Team team, ChessSquare square = null)
    {
        this.pieceType = pieceType;
        this.team = team;
        IsPromoted = false;
        IsCaptured = false;
        HasMoved = false;

        // spawn the piece model
        if (_pieceModel != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(_pieceModel.gameObject);
#else
            Destroy(_pieceModel.gameObject);
#endif
        }

        gameObject.name = $"{team.ToString().ToLower()}{pieceType}";

        _pieceModel = chessPieces.SpawnPieceModel(pieceType, team, transform).transform;

        GetComponent<MeshCollider>().sharedMesh = _pieceModel.GetComponentInChildren<MeshFilter>().sharedMesh;
        if (team == Team.Black)
        {
            // we have to double rotate (because its already rotated)
            _pieceModel.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            // transform.Rotate(0f, 180f, 0f);
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if (square != null)
        {
            MoveToSquare(square, 0f);
        }

        OnInitialized?.Invoke(this);
    }

    public void MoveToSquare(ChessSquare square, float duration = 0.5f)
    {
        CurrentSquare = square;
        if (duration <= 0f)
        {
            transform.position = square.transform.position + pieceOffset;
            return;
        }
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        _moveCoroutine = this.LerpAction((t) =>
        {
            transform.position = Vector3.Lerp(transform.position, square.transform.position + pieceOffset, t);
        },
        duration,
        () => transform.position = square.transform.position + pieceOffset);
        HasMoved = true;
    }

    public static int PieceValue(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Pawn:
                return 1;
            case PieceType.Knight:
            case PieceType.Bishop:
                return 3;
            case PieceType.Rook:
                return 5;
            case PieceType.Queen:
                return 9;
            case PieceType.King:
                return 10;
            default:
                return 0;
        }
    }
}