// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using ContinuousLearning; // Passive profile values cross the existing lower-assembly bridge.
using System.Globalization; // Describe exact current binary64 metadata without locale aliases.

public sealed class RoutingProfileTrainer
{
    private readonly RoutingUnitMetadataRegistry _metadataRegistry;
    internal const int MinimumKeywordLength = 3; // Normal extraction and pure projection consume the same constant.
    internal const string KeywordSeparators = " \t\r\n.,:;/\\-_()[]{}\"'"; // Preserve all existing separator characters in their original order.
    internal const string CSharpFrom = "C#", CSharpTo = "csharp", DotNetFrom = ".NET", DotNetTo = "dotnet"; // Share the existing language-token replacements between keyword extraction and input description.

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

    internal OperationResult<CodeDocumentationProfileMetadataV2[]> DescribeMetadata(IReadOnlyList<string> unitIds) // Snapshot every actual candidate entry from this trainer injected registry.
    { // Snapshot every actual candidate entry from this trainer injected registry.
        var values = new List<CodeDocumentationProfileMetadataV2>(); // Read this trainer's actual injected registry, never a replacement default.
        foreach (var id in unitIds) // Snapshot every actual candidate entry from this trainer injected registry.
        { // Snapshot every actual candidate entry from this trainer injected registry.
            var found = _metadataRegistry.Require(id); // Snapshot every actual candidate entry from this trainer injected registry.
            if (!found.IsSuccess || found.Value is null) return found.MapError<CodeDocumentationProfileMetadataV2[]>(); // Snapshot every actual candidate entry from this trainer injected registry.
            var m = found.Value; // Snapshot every actual candidate entry from this trainer injected registry.
            if (m.AllowedInputs is null || !double.IsFinite(m.ConfidenceFloor) || !double.IsFinite(m.BaselinePrior)) // Snapshot every actual candidate entry from this trainer injected registry.
                return OperationResult<CodeDocumentationProfileMetadataV2[]>.Fail("Routing metadata is not finite/complete.", CodeDocumentationOneLessonProfileSelection.InvalidErrorCode); // Snapshot every actual candidate entry from this trainer injected registry.
            values.Add(new(m.UnitId, m.Role, m.Domain, m.AllowedInputs.ToArray(), m.ConfidenceFloor.ToString("R", CultureInfo.InvariantCulture), // Snapshot every actual candidate entry from this trainer injected registry.
                m.MetadataSourceRef, m.BaselinePrior.ToString("R", CultureInfo.InvariantCulture))); // Freeze every allowed input before returning the projection.
        } // Snapshot every actual candidate entry from this trainer injected registry.
        return OperationResult<CodeDocumentationProfileMetadataV2[]>.Ok(values.ToArray()); // Snapshot every actual candidate entry from this trainer injected registry.
    } // Snapshot every actual candidate entry from this trainer injected registry.

    internal static string[] DescribeStopWords() => StopWords.Order(StringComparer.Ordinal).ToArray(); // The actual set, projected deterministically without exposing it.

    private static IEnumerable<string> ExtractKeywords(RoutingTrainingExample example)
    {
        var text = $"{example.Prompt} {example.TaskSlice} {example.RequiredDirection} {example.DomainHint}"
            .Replace(CSharpFrom, CSharpTo, StringComparison.OrdinalIgnoreCase) // Normalize C# spelling with the same case-insensitive replacement described in the profile inputs.
            .Replace(DotNetFrom, DotNetTo, StringComparison.OrdinalIgnoreCase) // Normalize .NET spelling before tokenization, preserving the existing replacement order.
            .ToLowerInvariant(); // Preserve culture-independent lowercase keyword extraction.

        foreach (var token in text.Split(KeywordSeparators.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)) // Tokenize with the complete original delimiter set and discard only empty entries.
        {
            if (token.Length < MinimumKeywordLength) // Preserve the existing short-keyword filter using its shared minimum length.
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
