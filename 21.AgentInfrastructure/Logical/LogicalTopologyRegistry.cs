// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This record describes one logical domain available to the orchestrator topology.
public sealed record LogicalDomain(
    string DomainId,
    string Name,
    string Purpose,
    string[] PredicateIds,
    string[] SourceIds);

// This record stores one logical domain with registry audit metadata.
public sealed record LogicalDomainEntry(
    LogicalDomain Domain,
    long Revision,
    DateTimeOffset UpdatedAtUtc);

// This record describes one development or methodology phase in the logical topology.
public sealed record LogicalPhase(
    string PhaseId,
    string Name,
    string Purpose,
    string[] DomainIds,
    string[] PredicateIds,
    string[] SourceIds);

// This record stores one logical phase with registry audit metadata.
public sealed record LogicalPhaseEntry(
    LogicalPhase Phase,
    long Revision,
    DateTimeOffset UpdatedAtUtc);

// This record describes one sub-phase inside a logical phase.
public sealed record LogicalSubPhase(
    string SubPhaseId,
    string PhaseId,
    string Name,
    string Purpose,
    string[] DomainIds,
    string[] PredicateIds,
    string[] SourceIds);

// This record stores one logical sub-phase with registry audit metadata.
public sealed record LogicalSubPhaseEntry(
    LogicalSubPhase SubPhase,
    long Revision,
    DateTimeOffset UpdatedAtUtc);

// This record describes one allowed transition between topology elements.
public sealed record LogicalTransition(
    string TransitionId,
    string FromTopologyKind,
    string FromTopologyId,
    string ToTopologyKind,
    string ToTopologyId,
    string Purpose,
    string[] PredicateIds,
    string[] ConstraintIds,
    string[] SourceIds);

// This record stores one logical transition with registry audit metadata.
public sealed record LogicalTransitionEntry(
    LogicalTransition Transition,
    long Revision,
    DateTimeOffset UpdatedAtUtc);

// This interface is the runtime contract for topology domain storage.
public interface ILogicalTopologyRegistry
{
    // AddOrUpdateDomain stores one domain definition and returns the stored audit entry.
    OperationResult<LogicalDomainEntry> AddOrUpdateDomain(LogicalDomain domain);

    // ResolveDomain returns one domain by its stable id.
    OperationResult<LogicalDomainEntry> ResolveDomain(string domainId);

    // ListDomains returns all stored domains for audit and topology planning.
    OperationResult<IReadOnlyList<LogicalDomainEntry>> ListDomains();

    // AddOrUpdatePhase stores one phase definition and returns the stored audit entry.
    OperationResult<LogicalPhaseEntry> AddOrUpdatePhase(LogicalPhase phase);

    // ResolvePhase returns one phase by its stable id.
    OperationResult<LogicalPhaseEntry> ResolvePhase(string phaseId);

    // ListPhases returns all stored phases for audit and topology planning.
    OperationResult<IReadOnlyList<LogicalPhaseEntry>> ListPhases();

    // AddOrUpdateSubPhase stores one sub-phase definition and returns the stored audit entry.
    OperationResult<LogicalSubPhaseEntry> AddOrUpdateSubPhase(LogicalSubPhase subPhase);

    // ResolveSubPhase returns one sub-phase by its stable id.
    OperationResult<LogicalSubPhaseEntry> ResolveSubPhase(string subPhaseId);

    // ListSubPhases returns all stored sub-phases for audit and topology planning.
    OperationResult<IReadOnlyList<LogicalSubPhaseEntry>> ListSubPhases();

    // AddOrUpdateTransition stores one transition definition and returns the stored audit entry.
    OperationResult<LogicalTransitionEntry> AddOrUpdateTransition(LogicalTransition transition);

    // ResolveTransition returns one transition by its stable id.
    OperationResult<LogicalTransitionEntry> ResolveTransition(string transitionId);

    // ListTransitions returns all stored transitions for audit and topology planning.
    OperationResult<IReadOnlyList<LogicalTransitionEntry>> ListTransitions();
}

// This in-memory registry is the first runtime topology store for logical domains.
public sealed class InMemoryLogicalTopologyRegistry : ILogicalTopologyRegistry
{
    // The domain table is keyed by stable domain id.
    private readonly Dictionary<string, LogicalDomainEntry> _domains = new(StringComparer.Ordinal);

    // The phase table is keyed by stable phase id.
    private readonly Dictionary<string, LogicalPhaseEntry> _phases = new(StringComparer.Ordinal);

    // The sub-phase table is keyed by stable sub-phase id.
    private readonly Dictionary<string, LogicalSubPhaseEntry> _subPhases = new(StringComparer.Ordinal);

    // The transition table is keyed by stable transition id.
    private readonly Dictionary<string, LogicalTransitionEntry> _transitions = new(StringComparer.Ordinal);

    // The revision counter makes every mutation auditable in deterministic order.
    private long _nextRevision;

    // Add or update one logical domain definition.
    public OperationResult<LogicalDomainEntry> AddOrUpdateDomain(LogicalDomain domain)
    {
        // Reject missing domain objects through the shared operation envelope.
        if (domain is null)
            return OperationResult<LogicalDomainEntry>.Fail("Domain is required.", "DOMAIN_REQUIRED");

        // Reject empty domain ids because transitions and units need a stable key.
        if (string.IsNullOrWhiteSpace(domain.DomainId))
            return OperationResult<LogicalDomainEntry>.Fail("Domain id is required.", "EMPTY_DOMAIN_ID");

        // Reject empty names because human review needs a readable domain label.
        if (string.IsNullOrWhiteSpace(domain.Name))
            return OperationResult<LogicalDomainEntry>.Fail("Domain name is required.", "EMPTY_DOMAIN_NAME");

        // Reject empty purpose text because the domain must explain what problem area it owns.
        if (string.IsNullOrWhiteSpace(domain.Purpose))
            return OperationResult<LogicalDomainEntry>.Fail("Domain purpose is required.", "EMPTY_DOMAIN_PURPOSE");

        // Normalize predicate ids so equivalent definitions do not differ by whitespace or casing.
        var predicateIds = NormalizeIdentifiers(domain.PredicateIds);

        // Normalize source ids so audit provenance stays stable.
        var sourceIds = NormalizeIdentifiers(domain.SourceIds);

        // Build the stored domain with normalized identifiers and trimmed text.
        var storedDomain = domain with
        {
            // Keep the domain id trimmed but otherwise caller-owned.
            DomainId = domain.DomainId.Trim(),

            // Keep the display name trimmed for human output.
            Name = domain.Name.Trim(),

            // Keep the purpose trimmed for evidence output.
            Purpose = domain.Purpose.Trim(),

            // Store normalized predicates.
            PredicateIds = predicateIds,

            // Store normalized source ids.
            SourceIds = sourceIds
        };

        // Create a new entry with a fresh revision and timestamp.
        var entry = new LogicalDomainEntry(storedDomain, ++_nextRevision, DateTimeOffset.UtcNow);

        // Store or replace the domain by stable id.
        _domains[storedDomain.DomainId] = entry;

        // Return the stored entry to the caller.
        return OperationResult<LogicalDomainEntry>.Ok(entry);
    }

    // Resolve one domain by stable id.
    public OperationResult<LogicalDomainEntry> ResolveDomain(string domainId)
    {
        // Reject empty lookup keys through the shared operation envelope.
        if (string.IsNullOrWhiteSpace(domainId))
            return OperationResult<LogicalDomainEntry>.Fail("Domain id is required.", "EMPTY_DOMAIN_ID");

        // Trim the lookup id so accidental surrounding whitespace does not miss a domain.
        var normalizedId = domainId.Trim();

        // Return a clean failure when the domain does not exist.
        if (!_domains.TryGetValue(normalizedId, out var entry))
            return OperationResult<LogicalDomainEntry>.Fail($"No logical domain registered for id '{normalizedId}'.", "DOMAIN_NOT_FOUND");

        // Return the stored entry.
        return OperationResult<LogicalDomainEntry>.Ok(entry);
    }

    // Return all domains in revision order.
    public OperationResult<IReadOnlyList<LogicalDomainEntry>> ListDomains()
    {
        // Order entries by revision so callers see the mutation sequence.
        var entries = _domains.Values.OrderBy(entry => entry.Revision).ToArray();

        // Return the read-only entry list.
        return OperationResult<IReadOnlyList<LogicalDomainEntry>>.Ok(entries);
    }

    // Add or update one logical phase definition.
    public OperationResult<LogicalPhaseEntry> AddOrUpdatePhase(LogicalPhase phase)
    {
        // Reject missing phase objects through the shared operation envelope.
        if (phase is null)
            return OperationResult<LogicalPhaseEntry>.Fail("Phase is required.", "PHASE_REQUIRED");

        // Reject empty phase ids because phase transitions need a stable key.
        if (string.IsNullOrWhiteSpace(phase.PhaseId))
            return OperationResult<LogicalPhaseEntry>.Fail("Phase id is required.", "EMPTY_PHASE_ID");

        // Reject empty names because human review needs a readable phase label.
        if (string.IsNullOrWhiteSpace(phase.Name))
            return OperationResult<LogicalPhaseEntry>.Fail("Phase name is required.", "EMPTY_PHASE_NAME");

        // Reject empty purpose text because the phase must explain the lifecycle work it owns.
        if (string.IsNullOrWhiteSpace(phase.Purpose))
            return OperationResult<LogicalPhaseEntry>.Fail("Phase purpose is required.", "EMPTY_PHASE_PURPOSE");

        // Normalize domain ids so phase ownership references stay stable.
        var domainIds = NormalizeStableIdentifiers(phase.DomainIds);

        // Reject phases without domain ownership because a phase with no domains cannot route work.
        if (domainIds.Length == 0)
            return OperationResult<LogicalPhaseEntry>.Fail("At least one phase domain id is required.", "EMPTY_PHASE_DOMAIN_IDS");

        // Normalize predicate ids so equivalent definitions do not differ by whitespace or casing.
        var predicateIds = NormalizeIdentifiers(phase.PredicateIds);

        // Normalize source ids so audit provenance stays stable.
        var sourceIds = NormalizeIdentifiers(phase.SourceIds);

        // Build the stored phase with normalized identifiers and trimmed text.
        var storedPhase = phase with
        {
            // Keep the phase id trimmed but otherwise caller-owned.
            PhaseId = phase.PhaseId.Trim(),

            // Keep the display name trimmed for human output.
            Name = phase.Name.Trim(),

            // Keep the purpose trimmed for evidence output.
            Purpose = phase.Purpose.Trim(),

            // Store normalized domain ids.
            DomainIds = domainIds,

            // Store normalized predicates.
            PredicateIds = predicateIds,

            // Store normalized source ids.
            SourceIds = sourceIds
        };

        // Create a new entry with a fresh topology revision and timestamp.
        var entry = new LogicalPhaseEntry(storedPhase, ++_nextRevision, DateTimeOffset.UtcNow);

        // Store or replace the phase by stable id.
        _phases[storedPhase.PhaseId] = entry;

        // Return the stored entry to the caller.
        return OperationResult<LogicalPhaseEntry>.Ok(entry);
    }

    // Resolve one phase by stable id.
    public OperationResult<LogicalPhaseEntry> ResolvePhase(string phaseId)
    {
        // Reject empty lookup keys through the shared operation envelope.
        if (string.IsNullOrWhiteSpace(phaseId))
            return OperationResult<LogicalPhaseEntry>.Fail("Phase id is required.", "EMPTY_PHASE_ID");

        // Trim the lookup id so accidental surrounding whitespace does not miss a phase.
        var normalizedId = phaseId.Trim();

        // Return a clean failure when the phase does not exist.
        if (!_phases.TryGetValue(normalizedId, out var entry))
            return OperationResult<LogicalPhaseEntry>.Fail($"No logical phase registered for id '{normalizedId}'.", "PHASE_NOT_FOUND");

        // Return the stored entry.
        return OperationResult<LogicalPhaseEntry>.Ok(entry);
    }

    // Return all phases in revision order.
    public OperationResult<IReadOnlyList<LogicalPhaseEntry>> ListPhases()
    {
        // Order entries by revision so callers see the mutation sequence.
        var entries = _phases.Values.OrderBy(entry => entry.Revision).ToArray();

        // Return the read-only phase list.
        return OperationResult<IReadOnlyList<LogicalPhaseEntry>>.Ok(entries);
    }

    // Add or update one logical sub-phase definition.
    public OperationResult<LogicalSubPhaseEntry> AddOrUpdateSubPhase(LogicalSubPhase subPhase)
    {
        // Reject missing sub-phase objects through the shared operation envelope.
        if (subPhase is null)
            return OperationResult<LogicalSubPhaseEntry>.Fail("Sub-phase is required.", "SUB_PHASE_REQUIRED");

        // Reject empty sub-phase ids because sub-phase sequencing needs a stable key.
        if (string.IsNullOrWhiteSpace(subPhase.SubPhaseId))
            return OperationResult<LogicalSubPhaseEntry>.Fail("Sub-phase id is required.", "EMPTY_SUB_PHASE_ID");

        // Reject empty parent phase ids because every sub-phase must belong to a phase.
        if (string.IsNullOrWhiteSpace(subPhase.PhaseId))
            return OperationResult<LogicalSubPhaseEntry>.Fail("Parent phase id is required.", "EMPTY_PARENT_PHASE_ID");

        // Reject empty names because human review needs a readable sub-phase label.
        if (string.IsNullOrWhiteSpace(subPhase.Name))
            return OperationResult<LogicalSubPhaseEntry>.Fail("Sub-phase name is required.", "EMPTY_SUB_PHASE_NAME");

        // Reject empty purpose text because the sub-phase must explain the work slice it owns.
        if (string.IsNullOrWhiteSpace(subPhase.Purpose))
            return OperationResult<LogicalSubPhaseEntry>.Fail("Sub-phase purpose is required.", "EMPTY_SUB_PHASE_PURPOSE");

        // Normalize domain ids so sub-phase ownership references stay stable.
        var domainIds = NormalizeStableIdentifiers(subPhase.DomainIds);

        // Reject sub-phases without domain ownership because they cannot route work.
        if (domainIds.Length == 0)
            return OperationResult<LogicalSubPhaseEntry>.Fail("At least one sub-phase domain id is required.", "EMPTY_SUB_PHASE_DOMAIN_IDS");

        // Normalize predicate ids so equivalent definitions do not differ by whitespace or casing.
        var predicateIds = NormalizeIdentifiers(subPhase.PredicateIds);

        // Normalize source ids so audit provenance stays stable.
        var sourceIds = NormalizeIdentifiers(subPhase.SourceIds);

        // Build the stored sub-phase with normalized identifiers and trimmed text.
        var storedSubPhase = subPhase with
        {
            // Keep the sub-phase id trimmed but otherwise caller-owned.
            SubPhaseId = subPhase.SubPhaseId.Trim(),

            // Keep the parent phase id trimmed but otherwise caller-owned.
            PhaseId = subPhase.PhaseId.Trim(),

            // Keep the display name trimmed for human output.
            Name = subPhase.Name.Trim(),

            // Keep the purpose trimmed for evidence output.
            Purpose = subPhase.Purpose.Trim(),

            // Store normalized domain ids.
            DomainIds = domainIds,

            // Store normalized predicates.
            PredicateIds = predicateIds,

            // Store normalized source ids.
            SourceIds = sourceIds
        };

        // Create a new entry with a fresh topology revision and timestamp.
        var entry = new LogicalSubPhaseEntry(storedSubPhase, ++_nextRevision, DateTimeOffset.UtcNow);

        // Store or replace the sub-phase by stable id.
        _subPhases[storedSubPhase.SubPhaseId] = entry;

        // Return the stored entry to the caller.
        return OperationResult<LogicalSubPhaseEntry>.Ok(entry);
    }

    // Resolve one sub-phase by stable id.
    public OperationResult<LogicalSubPhaseEntry> ResolveSubPhase(string subPhaseId)
    {
        // Reject empty lookup keys through the shared operation envelope.
        if (string.IsNullOrWhiteSpace(subPhaseId))
            return OperationResult<LogicalSubPhaseEntry>.Fail("Sub-phase id is required.", "EMPTY_SUB_PHASE_ID");

        // Trim the lookup id so accidental surrounding whitespace does not miss a sub-phase.
        var normalizedId = subPhaseId.Trim();

        // Return a clean failure when the sub-phase does not exist.
        if (!_subPhases.TryGetValue(normalizedId, out var entry))
            return OperationResult<LogicalSubPhaseEntry>.Fail($"No logical sub-phase registered for id '{normalizedId}'.", "SUB_PHASE_NOT_FOUND");

        // Return the stored entry.
        return OperationResult<LogicalSubPhaseEntry>.Ok(entry);
    }

    // Return all sub-phases in revision order.
    public OperationResult<IReadOnlyList<LogicalSubPhaseEntry>> ListSubPhases()
    {
        // Order entries by revision so callers see the mutation sequence.
        var entries = _subPhases.Values.OrderBy(entry => entry.Revision).ToArray();

        // Return the read-only sub-phase list.
        return OperationResult<IReadOnlyList<LogicalSubPhaseEntry>>.Ok(entries);
    }

    // Add or update one logical transition definition.
    public OperationResult<LogicalTransitionEntry> AddOrUpdateTransition(LogicalTransition transition)
    {
        // Reject missing transition objects through the shared operation envelope.
        if (transition is null)
            return OperationResult<LogicalTransitionEntry>.Fail("Transition is required.", "TRANSITION_REQUIRED");

        // Reject empty transition ids because transition edges need a stable key.
        if (string.IsNullOrWhiteSpace(transition.TransitionId))
            return OperationResult<LogicalTransitionEntry>.Fail("Transition id is required.", "EMPTY_TRANSITION_ID");

        // Reject empty source kinds because transition readers need the source topology type.
        if (string.IsNullOrWhiteSpace(transition.FromTopologyKind))
            return OperationResult<LogicalTransitionEntry>.Fail("Source topology kind is required.", "EMPTY_FROM_TOPOLOGY_KIND");

        // Reject empty source ids because transition edges need a source endpoint.
        if (string.IsNullOrWhiteSpace(transition.FromTopologyId))
            return OperationResult<LogicalTransitionEntry>.Fail("Source topology id is required.", "EMPTY_FROM_TOPOLOGY_ID");

        // Reject empty target kinds because transition readers need the target topology type.
        if (string.IsNullOrWhiteSpace(transition.ToTopologyKind))
            return OperationResult<LogicalTransitionEntry>.Fail("Target topology kind is required.", "EMPTY_TO_TOPOLOGY_KIND");

        // Reject empty target ids because transition edges need a target endpoint.
        if (string.IsNullOrWhiteSpace(transition.ToTopologyId))
            return OperationResult<LogicalTransitionEntry>.Fail("Target topology id is required.", "EMPTY_TO_TOPOLOGY_ID");

        // Reject empty purpose text because transitions must explain why the handoff is valid.
        if (string.IsNullOrWhiteSpace(transition.Purpose))
            return OperationResult<LogicalTransitionEntry>.Fail("Transition purpose is required.", "EMPTY_TRANSITION_PURPOSE");

        // Normalize predicate ids so equivalent definitions do not differ by whitespace or casing.
        var predicateIds = NormalizeIdentifiers(transition.PredicateIds);

        // Normalize constraint ids so gate requirements stay stable.
        var constraintIds = NormalizeIdentifiers(transition.ConstraintIds);

        // Normalize source ids so audit provenance stays stable.
        var sourceIds = NormalizeIdentifiers(transition.SourceIds);

        // Build the stored transition with normalized identifiers and trimmed text.
        var storedTransition = transition with
        {
            // Keep the transition id trimmed but otherwise caller-owned.
            TransitionId = transition.TransitionId.Trim(),

            // Keep the source topology kind trimmed for routing evidence.
            FromTopologyKind = transition.FromTopologyKind.Trim(),

            // Keep the source topology id trimmed for edge lookup.
            FromTopologyId = transition.FromTopologyId.Trim(),

            // Keep the target topology kind trimmed for routing evidence.
            ToTopologyKind = transition.ToTopologyKind.Trim(),

            // Keep the target topology id trimmed for edge lookup.
            ToTopologyId = transition.ToTopologyId.Trim(),

            // Keep the purpose trimmed for evidence output.
            Purpose = transition.Purpose.Trim(),

            // Store normalized predicates.
            PredicateIds = predicateIds,

            // Store normalized constraints.
            ConstraintIds = constraintIds,

            // Store normalized source ids.
            SourceIds = sourceIds
        };

        // Create a new entry with a fresh topology revision and timestamp.
        var entry = new LogicalTransitionEntry(storedTransition, ++_nextRevision, DateTimeOffset.UtcNow);

        // Store or replace the transition by stable id.
        _transitions[storedTransition.TransitionId] = entry;

        // Return the stored entry to the caller.
        return OperationResult<LogicalTransitionEntry>.Ok(entry);
    }

    // Resolve one transition by stable id.
    public OperationResult<LogicalTransitionEntry> ResolveTransition(string transitionId)
    {
        // Reject empty lookup keys through the shared operation envelope.
        if (string.IsNullOrWhiteSpace(transitionId))
            return OperationResult<LogicalTransitionEntry>.Fail("Transition id is required.", "EMPTY_TRANSITION_ID");

        // Trim the lookup id so accidental surrounding whitespace does not miss a transition.
        var normalizedId = transitionId.Trim();

        // Return a clean failure when the transition does not exist.
        if (!_transitions.TryGetValue(normalizedId, out var entry))
            return OperationResult<LogicalTransitionEntry>.Fail($"No logical transition registered for id '{normalizedId}'.", "TRANSITION_NOT_FOUND");

        // Return the stored entry.
        return OperationResult<LogicalTransitionEntry>.Ok(entry);
    }

    // Return all transitions in revision order.
    public OperationResult<IReadOnlyList<LogicalTransitionEntry>> ListTransitions()
    {
        // Order entries by revision so callers see the mutation sequence.
        var entries = _transitions.Values.OrderBy(entry => entry.Revision).ToArray();

        // Return the read-only transition list.
        return OperationResult<IReadOnlyList<LogicalTransitionEntry>>.Ok(entries);
    }

    // Normalize identifier arrays by trimming, uppercasing predicates/source ids, dropping empties, and removing duplicates.
    private static string[] NormalizeIdentifiers(string[]? identifiers)
    {
        // Treat a null array as an empty identifier set.
        var safeIdentifiers = identifiers ?? [];

        // Normalize every identifier to a stable uppercase token.
        return safeIdentifiers
            // Trim whitespace around each identifier.
            .Select(identifier => identifier.Trim())
            // Drop blank identifiers because they cannot contribute to routing.
            .Where(identifier => !string.IsNullOrWhiteSpace(identifier))
            // Normalize casing for stable equality.
            .Select(identifier => identifier.ToUpperInvariant())
            // Remove duplicate identifiers.
            .Distinct(StringComparer.Ordinal)
            // Materialize the normalized identifiers.
            .ToArray();
    }

    // Normalize stable id arrays by trimming, dropping empties, and removing duplicates without changing caller casing.
    private static string[] NormalizeStableIdentifiers(string[]? identifiers)
    {
        // Treat a null array as an empty identifier set.
        var safeIdentifiers = identifiers ?? [];

        // Normalize every identifier to a stable trimmed token.
        return safeIdentifiers
            // Trim whitespace around each identifier.
            .Select(identifier => identifier.Trim())
            // Drop blank identifiers because they cannot contribute to routing.
            .Where(identifier => !string.IsNullOrWhiteSpace(identifier))
            // Remove duplicate identifiers without rewriting the caller's stable id.
            .Distinct(StringComparer.Ordinal)
            // Materialize the normalized identifiers.
            .ToArray();
    }
}
