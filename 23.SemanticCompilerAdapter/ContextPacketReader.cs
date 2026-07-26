// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompilerAdapter;

using System.Globalization;
using SemanticCompiler29;

public sealed record ContextSignals(
    Domain[] PriorDomains,
    string[] CompletedSubPhases,
    string[] ActiveKnowledgeEdges,
    IReadOnlyDictionary<Domain, double> PriorConfidenceMap)
{
    public static ContextSignals Empty { get; } = new([], [], [], new Dictionary<Domain, double>());

    public bool IsEmpty =>
        PriorDomains.Length == 0 &&
        CompletedSubPhases.Length == 0 &&
        ActiveKnowledgeEdges.Length == 0 &&
        PriorConfidenceMap.Count == 0;
}

public static class ContextPacketReader
{
    public const string ContextPacketMetadataKey = "context.packet";
    public const string ContextSignalsMetadataKey = "context.signals";
    public const string DomainLabelMetadataKey = "domain.label";
    public const string DomainConfidenceMetadataKey = "domain.confidence";

    public static ContextSignals Read(IReadOnlyDictionary<string, object>? metadata)
    {
        if (metadata is null)
            return ContextSignals.Empty;

        if (metadata.TryGetValue(ContextSignalsMetadataKey, out var signals))
            return Read(signals);

        // Transitional compatibility: callers may place pre-extracted signals under
        // the older packet key. Raw LogicalContextPacket is intentionally not read
        // here because project 23 must not reflect over project 21 contracts.
        return metadata.TryGetValue(ContextPacketMetadataKey, out var packet)
            ? Read(packet)
            : ContextSignals.Empty;
    }

    public static ContextSignals Read(object? contextPacket)
    {
        return contextPacket is ContextSignals signals
            ? signals
            : ContextSignals.Empty;
    }

    public static bool TryReadDomain(IReadOnlyDictionary<string, string> metrics, out Domain domain)
    {
        if (metrics.TryGetValue(DomainLabelMetadataKey, out var label) &&
            Enum.TryParse(label, ignoreCase: true, out domain))
            return true;

        domain = Domain.UNKNOWN;
        return false;
    }

    public static bool TryReadConfidence(IReadOnlyDictionary<string, string> metrics, out double confidence)
    {
        if (metrics.TryGetValue(DomainConfidenceMetadataKey, out var text) &&
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out confidence))
            return true;

        confidence = 0.0;
        return false;
    }
}
