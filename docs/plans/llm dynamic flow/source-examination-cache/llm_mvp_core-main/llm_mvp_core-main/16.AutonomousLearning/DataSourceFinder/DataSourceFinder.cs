namespace AutonomousLearning;

using AgentInfrastructure;

// DataSourceFinder — discovers configured .NET data sources grouped by trust tier.
// Returns a list of SourceEntry values ready for TrustScorer classification.
// Tier 1 sources (highest trust): dotnet/* GitHub repos + official Microsoft docs.
// Tier 2 sources (medium trust): high-star community repos with CI passing.
// Tier 3 sources (lower trust): repos with 100-500 stars or community blogs.
public sealed class DataSourceFinder : BaseService, IDataSourceFinder
{
    // DefaultSources — Tier 1 sources that are always available without configuration.
    // Sourced from DATASET-COLLECTION-PLAN-DOTNET-v1_0.md R12 analysis:
    // minimum 1,400 total examples required, 500 in CODE_Q domain.
    public static IReadOnlyList<SourceEntry> DefaultSources { get; } = new SourceEntry[]
    {
        // Microsoft's .NET runtime — highest-trust C# reference implementation.
        new("https://github.com/dotnet/runtime",    Tier: 1, Trust: TrustLevel.High),
        // ASP.NET Core — server-side C# framework, well-tested and actively maintained.
        new("https://github.com/dotnet/aspnetcore", Tier: 1, Trust: TrustLevel.High),
        // Entity Framework Core — ORM, rich example of LINQ and async patterns.
        new("https://github.com/dotnet/efcore",     Tier: 1, Trust: TrustLevel.High),
        // Roslyn — compiler-as-a-service, used by 13.CodeOptimizer directly.
        new("https://github.com/dotnet/roslyn",     Tier: 1, Trust: TrustLevel.High),
        // Microsoft Learn .NET docs — canonical reference for API usage patterns.
        new("https://docs.microsoft.com/dotnet",    Tier: 1, Trust: TrustLevel.High),
    };

    // _configured — the source list this instance will return from Find().
    // Set at construction time; cannot be changed after creation (immutable by design).
    private readonly IReadOnlyList<SourceEntry> _configured;

    // Constructor: receives storage and an optional explicit source list.
    // configured = null  → uses DefaultSources (standard production case).
    // configured = []    → Find() returns Fail(EMPTY_CONFIG) (test for missing configuration).
    public DataSourceFinder(ISessionStorage storage, IReadOnlyList<SourceEntry>? configured = null)
        : base(storage)
        => _configured = configured ?? DefaultSources;

    // Find — return all configured .NET data sources.
    // Returns Fail with EMPTY_CONFIG when the source list is empty.
    // An empty list signals a misconfiguration — the caller must supply at least one source.
    public OperationResult<IReadOnlyList<SourceEntry>> Find()
    {
        // Guard: a completely empty source list is a configuration error, not a valid result.
        if (_configured.Count == 0)
            return OperationResult<IReadOnlyList<SourceEntry>>.Fail(
                "No data sources configured. Provide at least one SourceEntry or use DefaultSources.",
                "EMPTY_CONFIG");

        // Return the full configured list — callers filter by Tier and TrustLevel as needed.
        return OperationResult<IReadOnlyList<SourceEntry>>.Ok(_configured);
    }
}
