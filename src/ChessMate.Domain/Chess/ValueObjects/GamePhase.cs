namespace ChessMate.Domain.Chess.ValueObjects;

/// <summary>
/// Represents the phase of a chess game
/// </summary>
public sealed record GamePhase
{
    public static readonly GamePhase Opening = new(Phase.Opening, "Opening");
    public static readonly GamePhase Middlegame = new(Phase.Middlegame, "Middlegame");
    public static readonly GamePhase Endgame = new(Phase.Endgame, "Endgame");

    public Phase Phase { get; }
    public string Description { get; }

    private GamePhase(Phase phase, string description)
    {
        Phase = phase;
        Description = description;
    }

    /// <summary>
    /// Determines the game phase based on move number and material on board
    /// </summary>
    /// <param name="moveNumber">Current move number</param>
    /// <param name="fen">Position FEN to analyze material</param>
    /// <returns>The current game phase</returns>
    public static GamePhase DeterminePhase(int moveNumber, string fen)
    {
        // Opening: typically first 3-5 moves
        if (moveNumber <= 3)
            return Opening;

        var material = AnalyzeMaterial(fen);
        var isEndgame = material.Queens <= 1 && 
                        (material.MinorPieces <= 2 || material.TotalPieceValue <= 20);

        return isEndgame ? Endgame : Middlegame;
    }

    private static MaterialCount AnalyzeMaterial(string fen)
    {
        var pieces = fen.Split(' ')[0]; // Get piece placement part of FEN
        
        var queens = CountPieces(pieces, 'q', 'Q');
        var rooks = CountPieces(pieces, 'r', 'R');
        var bishops = CountPieces(pieces, 'b', 'B');
        var knights = CountPieces(pieces, 'n', 'N');
        var minorPieces = bishops + knights;
        var totalValue = queens * 9 + rooks * 5 + minorPieces * 3;

        return new MaterialCount(queens, rooks, minorPieces, totalValue);
    }

    private static int CountPieces(string fen, char blackPiece, char whitePiece)
    {
        return fen.Count(c => c == blackPiece || c == whitePiece);
    }

    public override string ToString() => Description;

    private sealed record MaterialCount(int Queens, int Rooks, int MinorPieces, int TotalPieceValue);
}

/// <summary>
/// Enumeration of chess game phases
/// </summary>
public enum Phase
{
    Opening,
    Middlegame,
    Endgame
}