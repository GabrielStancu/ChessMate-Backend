# Copilot Instructions for ChessMate Backend

## General Principles
- This project follows a Pragmatic Domain-Driven Design (DDD) approach.
- Prefer clarity and explicit domain language over clever abstractions.
- Avoid overengineering or premature generalization.
- Deterministic logic always precedes AI-generated logic.

## Architectural Rules
- Domain layer MUST NOT depend on:
  - Infrastructure
  - External APIs
  - Stockfish
  - Azure OpenAI
- Application layer orchestrates workflows and use cases.
- Infrastructure layer contains all I/O, external services, and integrations.
- API layer is thin and delegates all logic to Application services.

## Domain Layer Guidelines
- Domain models represent chess concepts:
  - Game, Move, Position, Evaluation, Mistake, GamePhase
- Domain entities:
  - Are persistence-ignorant
  - Contain domain logic when appropriate
  - Avoid getters/setters without behavior
- Do NOT include:
  - Http calls
  - Logging
  - Engine evaluation logic
  - LLM interaction

## Application Layer Guidelines
- Application services:
  - Represent use cases (e.g. AnalyzeGame, GenerateFeedback)
  - Coordinate domain objects and infrastructure services
- Application services MAY:
  - Call infrastructure interfaces
  - Map domain models to DTOs
- Application services MUST NOT:
  - Contain HTTP-specific logic
  - Contain UI logic

## Infrastructure Layer Guidelines
- All external integrations live here:
  - Chess.com API client
  - Stockfish engine client
  - Azure OpenAI client
- Infrastructure classes:
  - Implement interfaces defined in Application layer
  - Are replaceable and testable
- Keep infrastructure logic procedural and simple.

## API Layer Guidelines
- Controllers:
  - Are thin
  - Validate input
  - Call Application services
  - Return DTOs only
- Controllers MUST NOT:
  - Contain business logic
  - Instantiate services directly

## Stockfish Rules
- Stockfish evaluations are authoritative.
- LLMs must never override or contradict Stockfish results.
- Engine evaluation thresholds determine mistake severity.

## Azure OpenAI Rules
- Azure OpenAI is used ONLY for explanation generation.
- Prompts must be structured and deterministic.
- No chess rule evaluation or move legality checks in LLMs.

## Naming Conventions
- Use chess terminology where possible.
- Avoid generic names like "Manager" or "Helper".
- Prefer explicit names like `StockfishAnalysisService`.

## Coding Style
- C# 12 / .NET 8
- Favor immutability where reasonable.
- Prefer explicit types over `var` in public APIs.
- Keep methods short and focused.

## Testing Guidelines
- Domain logic should be unit-testable.
- Infrastructure should be mockable.
- Avoid integration tests in early MVP unless necessary.

## Non-goals (MVP)
- Authentication
- Authorization
- Real-time analysis
- Multi-user persistence
- Training plans

## When in Doubt
- Keep logic close to the domain.
- Ask: "Is this chess knowledge or infrastructure?"
- Choose simplicity over extensibility.
