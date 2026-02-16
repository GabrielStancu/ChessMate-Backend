namespace ChessMate.Domain.Chess.ValueObjects;

/// <summary>
/// Represents the quality classification of a chess move based on evaluation loss
/// </summary>
public sealed record MoveClassification
{
    public static readonly MoveClassification Brilliant = new(MoveQuality.Brilliant, "Brilliant", "!!");
    public static readonly MoveClassification Great = new(MoveQuality.Great, "Great", "!");
    public static readonly MoveClassification Best = new(MoveQuality.Best, "Best", "");
    public static readonly MoveClassification Excellent = new(MoveQuality.Excellent, "Excellent", "");
    public static readonly MoveClassification Good = new(MoveQuality.Good, "Good", "");
    public static readonly MoveClassification Book = new(MoveQuality.Book, "Book", "");
    public static readonly MoveClassification Inaccuracy = new(MoveQuality.Inaccuracy, "Inaccuracy", "?!");
    public static readonly MoveClassification Mistake = new(MoveQuality.Mistake, "Mistake", "?");
    public static readonly MoveClassification Blunder = new(MoveQuality.Blunder, "Blunder", "??");

    public MoveQuality Quality { get; }
    public string Description { get; }
    public string Symbol { get; }

    private MoveClassification(MoveQuality quality, string description, string symbol)
    {
        Quality = quality;
        Description = description;
        Symbol = symbol;
    }

    /// <summary>
    /// Classifies a move based on centipawn loss and game phase
    /// </summary>
    /// <param name="centipawnLoss">Centipawn loss (negative means improvement)</param>
    /// <param name="gamePhase">Current phase of the game</param>
    /// <returns>Move classification</returns>
    public static MoveClassification FromCentipawnLoss(int centipawnLoss, GamePhase gamePhase)
    {
        return gamePhase.Phase switch
        {
            Phase.Opening => ClassifyOpeningMove(centipawnLoss),
            Phase.Middlegame => ClassifyMiddlegameMove(centipawnLoss),
            Phase.Endgame => ClassifyEndgameMove(centipawnLoss),
            _ => Book
        };
    }

    /// <summary>
    /// Opening phase: Only classify as Book move or significant blunders
    /// No inaccuracy/mistake classification due to expected evaluation swings
    /// </summary>
    private static MoveClassification ClassifyOpeningMove(int centipawnLoss)
    {
        return centipawnLoss switch
        {
            <= 0 => Book,
            <= 150 => Book, // Allow larger swings in opening
            <= 300 => Inaccuracy, // Only flag serious issues
            _ => Blunder // Severe opening mistakes
        };
    }

    /// <summary>
    /// Middlegame phase: Standard classification with normal thresholds
    /// </summary>
    private static MoveClassification ClassifyMiddlegameMove(int centipawnLoss)
    {
        return centipawnLoss switch
        {
            <= -200 => Brilliant,
            <= -100 => Great,
            <= -50 => Best,
            <= 0 => Excellent,
            <= 50 => Good,
            <= 100 => Inaccuracy,
            <= 300 => Mistake,
            _ => Blunder
        };
    }

    /// <summary>
    /// Endgame phase: Stricter thresholds as precision is critical
    /// </summary>
    private static MoveClassification ClassifyEndgameMove(int centipawnLoss)
    {
        return centipawnLoss switch
        {
            <= -150 => Brilliant,
            <= -75 => Great,
            <= -30 => Best,
            <= 0 => Excellent,
            <= 25 => Good,
            <= 60 => Inaccuracy,  // Stricter threshold
            <= 150 => Mistake,    // Stricter threshold
            _ => Blunder
        };
    }

    public bool IsMistake() => Quality is MoveQuality.Inaccuracy or MoveQuality.Mistake or MoveQuality.Blunder;

    public bool IsGreatOrBrilliant() => Quality is MoveQuality.Brilliant or MoveQuality.Great;

    public override string ToString() => Description;
}

/// <summary>
/// Enumeration of move quality levels
/// </summary>
public enum MoveQuality
{
    Brilliant,
    Great,
    Best,
    Excellent,
    Good,
    Book,
    Inaccuracy,
    Mistake,
    Blunder
}