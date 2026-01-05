using System.Text;
using System.Text.RegularExpressions;
using ChessMate.Application.Interfaces;
using ChessMate.Domain.Chess.Entities;
using ChessMate.Domain.Chess.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ChessMate.Application.Services;

/// <summary>
/// Implementation of PGN parser service
/// </summary>
public sealed partial class PgnParser : IPgnParser
{
    private readonly IChessPositionService _positionService;
    private readonly ILogger<PgnParser> _logger;

    public PgnParser(IChessPositionService positionService, ILogger<PgnParser> logger)
    {
        _positionService = positionService;
        _logger = logger;
    }

    public ParsedGame ParseGame(string pgnContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pgnContent);

        try
        {
            var headers = ParseHeaders(pgnContent);
            var moveText = ExtractMoveText(pgnContent);
            var moves = ParseMoves(moveText, headers.GetValueOrDefault("FEN"));

            return new ParsedGame(
                whitePlayer: headers.GetValueOrDefault("White", "Unknown"),
                blackPlayer: headers.GetValueOrDefault("Black", "Unknown"),
                moves: moves,
                @event: headers.GetValueOrDefault("Event"),
                site: headers.GetValueOrDefault("Site"),
                date: ParseDate(headers.GetValueOrDefault("Date")),
                result: headers.GetValueOrDefault("Result"),
                timeControl: headers.GetValueOrDefault("TimeControl"),
                whiteElo: ParseElo(headers.GetValueOrDefault("WhiteElo")),
                blackElo: ParseElo(headers.GetValueOrDefault("BlackElo")),
                startingPosition: headers.ContainsKey("FEN") 
                    ? new FenPosition(headers["FEN"]) 
                    : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse PGN content");
            throw new InvalidOperationException("Failed to parse PGN content", ex);
        }
    }

    public IEnumerable<ParsedGame> ParseGames(string pgnContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pgnContent);

        var games = SplitPgnIntoGames(pgnContent);
        var parsedGames = new List<ParsedGame>();

        foreach (var gamePgn in games)
        {
            if (string.IsNullOrWhiteSpace(gamePgn))
                continue;

            try
            {
                parsedGames.Add(ParseGame(gamePgn));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping game due to parse error");
            }
        }

        return parsedGames;
    }

    private Dictionary<string, string> ParseHeaders(string pgnContent)
    {
        var headers = new Dictionary<string, string>();
        var headerMatches = HeaderRegex().Matches(pgnContent);

        foreach (Match match in headerMatches)
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            headers[key] = value;
        }

        return headers;
    }

    private string ExtractMoveText(string pgnContent)
    {
        var lines = pgnContent.Split('\n');
        var moveTextBuilder = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('['))
                continue;

            moveTextBuilder.Append(trimmed).Append(' ');
        }

        return moveTextBuilder.ToString();
    }

    private List<ParsedMove> ParseMoves(string moveText, string? startingFen)
    {
        var moves = new List<ParsedMove>();
        var currentPosition = startingFen ?? FenPosition.StartingPosition;
        var cleanedMoveText = CleanMoveText(moveText);
        var moveMatches = MoveRegex().Matches(cleanedMoveText);

        foreach (Match match in moveMatches)
        {
            var moveNumber = int.Parse(match.Groups[1].Value);
            var dots = match.Groups[2].Value;
            var sanMove = match.Groups[3].Value;
            var color = dots == "." ? PieceColor.White : PieceColor.Black;

            currentPosition = _positionService.MakeMove(currentPosition, sanMove);
            moves.Add(new ParsedMove(
                moveNumber,
                color,
                sanMove,
                new FenPosition(currentPosition)
            ));
        }

        return moves;
    }

    private string CleanMoveText(string moveText)
    {
        moveText = CommentBracesRegex().Replace(moveText, string.Empty);
        moveText = CommentParenthesesRegex().Replace(moveText, string.Empty);
        moveText = VariationRegex().Replace(moveText, string.Empty);
        moveText = ResultRegex().Replace(moveText, string.Empty);
        moveText = ClockRegex().Replace(moveText, string.Empty);

        return moveText.Trim();
    }

    private IEnumerable<string> SplitPgnIntoGames(string pgnContent)
    {
        var games = new List<string>();
        var currentGame = new StringBuilder();
        var lines = pgnContent.Split('\n');

        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("[Event ") && currentGame.Length > 0)
            {
                games.Add(currentGame.ToString());
                currentGame.Clear();
            }

            currentGame.AppendLine(line);
        }

        if (currentGame.Length > 0)
        {
            games.Add(currentGame.ToString());
        }

        return games;
    }

    private static DateOnly? ParseDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString) || dateString.Contains("?"))
            return null;

        return DateOnly.TryParse(dateString.Replace(".", "-"), out var date) ? date : null;
    }

    private static int? ParseElo(string? eloString)
    {
        return int.TryParse(eloString, out var elo) ? elo : null;
    }

    [GeneratedRegex(@"\[(\w+)\s+""([^""]*)""\]")]
    private static partial Regex HeaderRegex();

    [GeneratedRegex(@"(\d+)(\.{1,3})\s*([^\s]+)")]
    private static partial Regex MoveRegex();

    [GeneratedRegex(@"\{[^}]*\}")]
    private static partial Regex CommentBracesRegex();

    [GeneratedRegex(@"\([^)]*\)")]
    private static partial Regex CommentParenthesesRegex();

    [GeneratedRegex(@"\[[^\]]*\]")]
    private static partial Regex VariationRegex();

    [GeneratedRegex(@"\s*(1-0|0-1|1/2-1/2|\*)\s*$")]
    private static partial Regex ResultRegex();

    [GeneratedRegex(@"\[\%clk\s+[^\]]*\]")]
    private static partial Regex ClockRegex();
}