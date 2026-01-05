namespace ChessMate.Infrastructure.Stockfish;

public record StockfishRequest(string Tool, Dictionary<string, object> Arguments);

public record StockfishResponse(object Result);
