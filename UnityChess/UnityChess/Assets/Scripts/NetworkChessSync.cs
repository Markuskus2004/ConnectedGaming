using Unity.Netcode;
using UnityEngine;
using UnityChess;

public class NetworkChessSync : NetworkBehaviour
{
    // Network Variables
    public NetworkVariable<bool> IsWhiteTurn = new NetworkVariable<bool>(true);
    public NetworkVariable<ulong> WhitePlayerClientId = new NetworkVariable<ulong>();

    // Game Manager Reference
    private GameManager gameManager;
    private BoardManager boardManager;

    private void Start()
    {
        // Find game managers
        gameManager = GameManager.Instance;
        boardManager = BoardManager.Instance;

        // Subscribe to piece movement events
        VisualPiece.VisualPieceMoved += HandleVisualPieceMoved;

        // Subscribe to game events
        GameManager.MoveExecutedEvent += OnMoveExecuted;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        VisualPiece.VisualPieceMoved -= HandleVisualPieceMoved;
        GameManager.MoveExecutedEvent -= OnMoveExecuted;
    }

    private void HandleVisualPieceMoved(Square startSquare, Transform pieceTransform, Transform endSquareTransform, Piece promotionPiece = null)
    {
        // Only relay moves if we're networked
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            // Send the move to the server
            string startPos = startSquare.ToString();
            string endPos = endSquareTransform.name;

            MovePieceServerRpc(startPos, endPos);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MovePieceServerRpc(string startPos, string endPos)
    {
        Debug.Log($"Server received move from {startPos} to {endPos}");

        // Execute the move on the server
        // (Actual move logic handled by GameManager)

        // Relay to all clients
        MovePieceClientRpc(startPos, endPos);
    }

    [ClientRpc]
    private void MovePieceClientRpc(string startPos, string endPos)
    {
        Debug.Log($"Client received move from {startPos} to {endPos}");

        // Skip if this is the originating client
        if (IsServer && IsOwner)
            return;

        // Here we would trigger the move on the client side
        // Simulate a visual piece move

        // Find the piece at the start position
        Square startSquare = new Square(startPos);
        GameObject pieceGO = BoardManager.Instance.GetPieceGOAtPosition(startSquare);

        if (pieceGO != null)
        {
            // Find the end square transform
            GameObject endSquareGO = BoardManager.Instance.GetSquareGOByPosition(new Square(endPos));

            if (endSquareGO != null)
            {
                // Move the piece
                pieceGO.transform.parent = endSquareGO.transform;
                pieceGO.transform.position = endSquareGO.transform.position;
            }
        }
    }

    // Game event handlers
    private void OnMoveExecuted()
    {
        // If we're the server, toggle the turn
        if (IsServer)
        {
            IsWhiteTurn.Value = !IsWhiteTurn.Value;
            Debug.Log($"Turn changed to {(IsWhiteTurn.Value ? "White" : "Black")}");
        }
    }

    private void Update()
    {
        // Only run if we're connected
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient && boardManager != null)
        {
            // Only enable pieces if it's the local player's turn
            bool isLocalPlayerTurn = IsWhiteTurn.Value
                ? NetworkManager.Singleton.LocalClientId == WhitePlayerClientId.Value
                : NetworkManager.Singleton.LocalClientId != WhitePlayerClientId.Value;

            if (isLocalPlayerTurn)
            {
                Side sideToEnable = IsWhiteTurn.Value ? Side.White : Side.Black;
                boardManager.EnsureOnlyPiecesOfSideAreEnabled(sideToEnable);
            }
            else
            {
                boardManager.SetActiveAllPieces(false);
            }
        }
    }
}