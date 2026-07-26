// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;

public sealed class RoutingProfileTrainer
{
    private readonly RoutingUnitMetadataRegistry _metadataRegistry;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "that", "this", "into", "from", "before",
        "after", "unit", "model", "task", "request", "prompt", "please", "need",
        "should", "which", "will", "can", "must", "how", "what", "when", "where",
        "user", "system", "make", "build", "create", "write", "handle", "route"
    };

    public RoutingProfileTrainer(RoutingUnitMetadataRegistry? metadataRegistry = null)
    {
        _metadataRegistry = metadataRegistry ?? OrchestrationRoutingCurriculum.CreateDefaultUnitMetadataRegistry();
    }

    public OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>> Train(
        IReadOnlyList<RoutingTrainingExample> examples,
        RoutingUnitMetadataRegistry? metadataRegistry = null)
    {
        if (examples is null)
            return OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>>.Fail("Routing examples are required.", "ROUTING_EXAMPLES_REQUIRED");

        if (examples.Count == 0)
            return OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>>.Fail("At least one routing example is required.", "NO_ROUTING_EXAMPLES");

        var activeRegistry = metadataRegistry ?? _metadataRegistry;
        var keywordMap = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var example in examples)
        {
            if (string.IsNullOrWhiteSpace(example.ChosenUnitId))
                return OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>>.Fail("Chosen unit id is required.", "CHOSEN_UNIT_REQUIRED");

            if (!keywordMap.TryGetValue(example.ChosenUnitId, out var keywords))
            {
                keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                keywordMap[example.ChosenUnitId] = keywords;
            }

            foreach (var token in ExtractKeywords(example))
                keywords.Add(token);
        }

        var profiles = new Dictionary<string, UnitScoringProfile>(StringComparer.Ordinal);
        foreach (var pair in keywordMap)
        {
            var metadata = activeRegistry.Require(pair.Key);
            if (!metadata.IsSuccess)
                return metadata.MapError<IReadOnlyDictionary<string, UnitScoringProfile>>();

            profiles[pair.Key] = BuildProfile(
                pair.Key,
                pair.Value.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
                metadata.Value!);
        }

        return OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>>.Ok(profiles);
    }

    private static IEnumerable<string> ExtractKeywords(RoutingTrainingExample example)
    {
        var text = $"{example.Prompt} {example.TaskSlice} {example.RequiredDirection} {example.DomainHint}"
            .Replace("C#", "csharp", StringComparison.OrdinalIgnoreCase)
            .Replace(".NET", "dotnet", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        foreach (var token in text.Split([' ', '\t', '\r', '\n', '.', ',', ':', ';', '/', '\\', '-', '_', '(', ')', '[', ']', '{', '}', '"', '\''], StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Length < 3)
                continue;

            if (StopWords.Contains(token))
                continue;

            yield return token;
        }
    }

    private static UnitScoringProfile BuildProfile(
        string unitId,
        string[] keywords,
        UnitRoutingMetadata metadata)
        => new(
            unitId,
            metadata.Role,
            metadata.Domain,
            keywords,
            metadata.AllowedInputs,
            metadata.ConfidenceFloor,
            metadata.MetadataSourceRef,
            metadata.BaselinePrior);
}

public sealed record UnitRoutingMetadata(
    string UnitId,
    string Role,
    string Domain,
    string[] AllowedInputs,
    double ConfidenceFloor,
    string MetadataSourceRef,
    double BaselinePrior = 0.0);

public sealed class RoutingUnitMetadataRegistry
{
    private readonly IReadOnlyDictionary<string, UnitRoutingMetadata> _entries;

    public RoutingUnitMetadataRegistry(IEnumerable<UnitRoutingMetadata> entries)
    {
        _entries = entries
            .GroupBy(entry => entry.UnitId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.Ordinal);
    }

    public OperationResult<UnitRoutingMetadata> Require(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return OperationResult<UnitRoutingMetadata>.Fail("Unit id is required for routing metadata lookup.", "ROUTING_UNIT_ID_REQUIRED");

        if (!_entries.TryGetValue(unitId, out var metadata))
            return OperationResult<UnitRoutingMetadata>.Fail($"Explicit routing metadata is required for unit '{unitId}'.", "ROUTING_UNIT_METADATA_REQUIRED");

        if (string.IsNullOrWhiteSpace(metadata.Role) || string.IsNullOrWhiteSpace(metadata.Domain) || string.IsNullOrWhiteSpace(metadata.MetadataSourceRef))
            return OperationResult<UnitRoutingMetadata>.Fail($"Routing metadata for unit '{unitId}' must include role, domain, and source reference.", "ROUTING_UNIT_METADATA_INCOMPLETE");

        return OperationResult<UnitRoutingMetadata>.Ok(metadata);
    }
}
