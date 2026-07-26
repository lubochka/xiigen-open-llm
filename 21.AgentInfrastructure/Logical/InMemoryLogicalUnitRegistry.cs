// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This enum records whether a logical unit is safe to use as a replacement.
public enum LogicalUnitTrustState
{
    Candidate,
    Probationary,
    Trusted,
    Rejected
}

// This record describes one registered logical unit without exposing its generic type at lookup time.
public sealed record LogicalUnitDescriptor(
    string UnitId,
    string UnitRole,
    string DomainTag,
    string Version,
    LogicalUnitTrustState TrustState,
    bool IsLearned,
    Type InputType,
    Type OutputType,
    long RegistrationOrder);

// This record carries one already-loaded logical unit plus its persisted descriptor.
public sealed record LogicalUnitRegistryLoadedEntry(LogicalUnitDescriptor Descriptor, object Unit);

// This carrier preserves descriptor/executable identity from one exact stored registry tuple.
public sealed record ResolvedLogicalUnitRegistryEntry<TInput, TOutput>(
    LogicalUnitDescriptor Descriptor,
    ILogicalUnit<TInput, TOutput> Unit);

// This registry is the first local storage surface for logical units.
public sealed class InMemoryLogicalUnitRegistry
{
    // The unit table stores descriptors and executable unit objects together.
    private readonly List<(LogicalUnitDescriptor Descriptor, object Unit)> _entries = [];

    // This counter preserves registration order so later versions can be selected deterministically.
    private long _nextRegistrationOrder;

    // Register one logical unit instance with role, domain, version, and trust metadata.
    public OperationResult<LogicalUnitDescriptor> Register<TInput, TOutput>(
        string unitId,
        string unitRole,
        string domainTag,
        string version,
        LogicalUnitTrustState trustState,
        bool isLearned,
        ILogicalUnit<TInput, TOutput> unit)
    {
        // Reject non-canonical unit ids because they cannot be referenced safely by flows or evidence.
        if (!IsCanonicalIdentity(unitId))
            return OperationResult<LogicalUnitDescriptor>.Fail(
                "Logical unit id is invalid.",
                "LOGICAL_UNIT_REGISTRATION_UNIT_ID_INVALID");

        // Reject non-canonical roles because role is the slot the unit fills.
        if (!IsCanonicalIdentity(unitRole))
            return OperationResult<LogicalUnitDescriptor>.Fail(
                "Logical unit role is invalid.",
                "LOGICAL_UNIT_REGISTRATION_UNIT_ROLE_INVALID");

        // Reject non-canonical domain tags because domain is part of unit scoping.
        if (!IsCanonicalIdentity(domainTag))
            return OperationResult<LogicalUnitDescriptor>.Fail(
                "Logical unit domain tag is invalid.",
                "LOGICAL_UNIT_REGISTRATION_DOMAIN_TAG_INVALID");

        // Reject non-canonical versions because swappability and rollback need version identity.
        if (!IsCanonicalIdentity(version))
            return OperationResult<LogicalUnitDescriptor>.Fail(
                "Logical unit version is invalid.",
                "LOGICAL_UNIT_REGISTRATION_VERSION_INVALID");

        // Reject undefined trust values before accepting an executable unit.
        if (!Enum.IsDefined(typeof(LogicalUnitTrustState), trustState))
            return OperationResult<LogicalUnitDescriptor>.Fail(
                "Logical unit trust state is invalid.",
                "LOGICAL_UNIT_REGISTRATION_TRUST_STATE_INVALID");

        // A missing executable unit is never a valid registry entry.
        if (unit is null)
            return OperationResult<LogicalUnitDescriptor>.Fail(
                "Logical unit is required.",
                "LOGICAL_UNIT_REGISTRATION_UNIT_REQUIRED");

        // Reserve long.MaxValue and reject counters that cannot allocate another positive order safely.
        if (_nextRegistrationOrder >= long.MaxValue - 1)
            return OperationResult<LogicalUnitDescriptor>.Fail(
                "Logical unit registration order is exhausted.",
                "LOGICAL_UNIT_REGISTRATION_ORDER_EXHAUSTED");

        // Calculate the next identity locally so a failed list insertion cannot advance the counter.
        var nextRegistrationOrder = _nextRegistrationOrder + 1;

        // Build a descriptor that records both identity and generic type boundaries.
        var descriptor = new LogicalUnitDescriptor(
            unitId,
            unitRole,
            domainTag,
            version,
            trustState,
            isLearned,
            typeof(TInput),
            typeof(TOutput),
            nextRegistrationOrder);

        // Store the descriptor and executable unit together.
        _entries.Add((descriptor, unit));

        // Publish the matching counter only after the tuple has been stored successfully.
        _nextRegistrationOrder = nextRegistrationOrder;

        // Return the descriptor for tests and future evidence.
        return OperationResult<LogicalUnitDescriptor>.Ok(descriptor);
    }

    // Resolve a specific unit by id, role, domain, and generic type.
    public OperationResult<ILogicalUnit<TInput, TOutput>> Resolve<TInput, TOutput>(string unitId, string unitRole, string domainTag)
    {
        // Find the matching entry with exact identity, role, domain, and type boundaries.
        var entry = _entries.LastOrDefault(candidate =>
            candidate.Descriptor.UnitId == unitId &&
            candidate.Descriptor.UnitRole == unitRole &&
            candidate.Descriptor.DomainTag == domainTag &&
            candidate.Descriptor.InputType == typeof(TInput) &&
            candidate.Descriptor.OutputType == typeof(TOutput));

        // Return a clean failure when no unit matches.
        if (entry.Unit is null)
            return OperationResult<ILogicalUnit<TInput, TOutput>>.Fail($"No logical unit registered for id '{unitId}', role '{unitRole}', and domain '{domainTag}'.");

        // Return the executable unit under its typed interface.
        return OperationResult<ILogicalUnit<TInput, TOutput>>.Ok((ILogicalUnit<TInput, TOutput>)entry.Unit);
    }

    // Resolve the latest usable unit by role, domain, and generic type.
    public OperationResult<ILogicalUnit<TInput, TOutput>> ResolveLatest<TInput, TOutput>(string unitRole, string domainTag, LogicalUnitTrustState minimumTrustState = LogicalUnitTrustState.Probationary)
    {
        var resolved = ResolveLatestEntry<TInput, TOutput>(unitRole, domainTag, minimumTrustState);
        return resolved.IsSuccess
            ? OperationResult<ILogicalUnit<TInput, TOutput>>.Ok(resolved.Value!.Unit)
            : resolved.MapError<ILogicalUnit<TInput, TOutput>>();
    }

    // Resolve the latest usable descriptor and executable together from one exact stored tuple.
    public OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>> ResolveLatestEntry<TInput, TOutput>(
        string unitRole,
        string domainTag,
        LogicalUnitTrustState minimumTrustState = LogicalUnitTrustState.Probationary)
    {
        // Only executable trust floors may participate in registry selection.
        if (!IsExecutableTrustFloor(minimumTrustState))
        {
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                "Logical unit minimum trust state is invalid.",
                "LOGICAL_UNIT_MINIMUM_TRUST_STATE_INVALID");
        }

        // Find the newest unit whose trust state meets the caller's minimum.
        var entry = _entries
            .Where(candidate =>
                candidate.Descriptor.UnitRole == unitRole &&
                candidate.Descriptor.DomainTag == domainTag &&
                candidate.Descriptor.InputType == typeof(TInput) &&
                candidate.Descriptor.OutputType == typeof(TOutput) &&
                candidate.Descriptor.TrustState >= minimumTrustState &&
                candidate.Descriptor.TrustState != LogicalUnitTrustState.Rejected)
            .OrderByDescending(candidate => candidate.Descriptor.RegistrationOrder)
            .FirstOrDefault();

        // Return a clean failure when no trusted-enough unit is available.
        if (entry.Unit is null)
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail($"No logical unit registered for role '{unitRole}' and domain '{domainTag}' at trust '{minimumTrustState}' or higher.");

        // Return descriptor and executable from the same selection so identity cannot drift from execution.
        return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Ok(
            new ResolvedLogicalUnitRegistryEntry<TInput, TOutput>(
                entry.Descriptor,
                (ILogicalUnit<TInput, TOutput>)entry.Unit));
    }

    // Resolve the latest learned descriptor and executable atomically from one exact stored tuple.
    public OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>> ResolveLatestLearnedEntry<TInput, TOutput>(
        string unitRole,
        string domainTag,
        LogicalUnitTrustState minimumTrustState = LogicalUnitTrustState.Probationary)
    {
        // Only executable trust floors may participate in learned-unit selection.
        if (!IsExecutableTrustFloor(minimumTrustState))
        {
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                "Logical unit minimum trust state is invalid.",
                "LOGICAL_UNIT_MINIMUM_TRUST_STATE_INVALID");
        }

        // Materialize every usable learned tuple before selecting its sole newest registration order.
        var usableEntries = _entries
            .Where(candidate =>
                StringComparer.Ordinal.Equals(candidate.Descriptor.UnitRole, unitRole) &&
                StringComparer.Ordinal.Equals(candidate.Descriptor.DomainTag, domainTag) &&
                candidate.Descriptor.InputType == typeof(TInput) &&
                candidate.Descriptor.OutputType == typeof(TOutput) &&
                candidate.Descriptor.IsLearned &&
                candidate.Descriptor.TrustState >= minimumTrustState &&
                candidate.Descriptor.TrustState != LogicalUnitTrustState.Rejected)
            .ToArray();

        if (usableEntries.Length == 0)
        {
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                "No usable learned logical unit is registered for the requested slot.",
                "LOGICAL_UNIT_LEARNED_ENTRY_NOT_FOUND");
        }

        // RegistrationOrder is the sole newest-version authority for the learned slot.
        var latestOrder = usableEntries.Max(candidate => candidate.Descriptor.RegistrationOrder);
        var latestEntries = usableEntries
            .Where(candidate => candidate.Descriptor.RegistrationOrder == latestOrder)
            .ToArray();

        // Never use list insertion order to resolve an ambiguous maximum.
        if (latestEntries.Length != 1)
        {
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                "The newest learned logical unit registration is ambiguous.",
                "LOGICAL_UNIT_REGISTRATION_ORDER_AMBIGUOUS");
        }

        // Preserve descriptor/executable identity and verify the executable boundary before returning it.
        var selected = latestEntries[0];
        if (selected.Unit is not ILogicalUnit<TInput, TOutput> typedUnit)
        {
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                "The selected learned logical unit does not implement the requested typed interface.",
                "LOGICAL_UNIT_TYPE_MISMATCH");
        }

        return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Ok(
            new ResolvedLogicalUnitRegistryEntry<TInput, TOutput>(selected.Descriptor, typedUnit));
    }

    // Resolve the newest registration for one exact id before applying any type or metadata filter.
    public OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>> ResolveLatestById<TInput, TOutput>(string unitId)
    {
        // Empty identity has no exact registry key. Whitespace is deliberately not trimmed or normalized.
        if (string.IsNullOrEmpty(unitId))
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                "Logical unit id is required.",
                "LOGICAL_UNIT_ID_REQUIRED");

        // Materialize every exact-id tuple before considering role, domain, type, trust, or learned state.
        var exactIdEntries = _entries
            .Where(candidate => StringComparer.Ordinal.Equals(candidate.Descriptor.UnitId, unitId))
            .ToArray();

        if (exactIdEntries.Length == 0)
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                $"No logical unit registered for exact id '{unitId}'.",
                "LOGICAL_UNIT_ID_NOT_FOUND");

        // RegistrationOrder is the sole newest-version authority for this exact identity.
        var latestOrder = exactIdEntries.Max(candidate => candidate.Descriptor.RegistrationOrder);
        // A duplicate maximum is ambiguous and must never be resolved by list insertion order.
        var latestEntries = exactIdEntries
            .Where(candidate => candidate.Descriptor.RegistrationOrder == latestOrder)
            .ToArray();

        if (latestEntries.Length != 1)
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                $"Logical unit '{unitId}' has an ambiguous latest registration order.",
                "LOGICAL_UNIT_REGISTRATION_ORDER_AMBIGUOUS");

        if (latestOrder <= 0)
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                $"Logical unit '{unitId}' has an invalid latest registration order.",
                "LOGICAL_UNIT_REGISTRATION_ORDER_INVALID");

        // Descriptor and executable are selected once from the same stored tuple.
        var selected = latestEntries[0];
        if (selected.Unit is not ILogicalUnit<TInput, TOutput> typedUnit)
            return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Fail(
                $"Latest logical unit '{unitId}' does not implement the requested typed interface.",
                "LOGICAL_UNIT_TYPE_MISMATCH");

        return OperationResult<ResolvedLogicalUnitRegistryEntry<TInput, TOutput>>.Ok(
            new ResolvedLogicalUnitRegistryEntry<TInput, TOutput>(selected.Descriptor, typedUnit));
    }

    // Return descriptors for audit and tests without exposing executable unit instances.
    public IReadOnlyList<LogicalUnitDescriptor> ListDescriptors()
    {
        // Project the stored descriptors into a read-only list.
        return _entries.Select(entry => entry.Descriptor).ToArray();
    }

    // Load persisted registry entries directly without re-registering or changing their registration order.
    public OperationResult<int> LoadEntries(
        IReadOnlyList<LogicalUnitRegistryLoadedEntry> entries,
        bool replaceExisting = true)
    {
        // Reject a missing load list before touching current state.
        if (entries is null)
            return OperationResult<int>.Fail("Loaded unit entries are required.", "LOADED_UNIT_ENTRIES_REQUIRED");

        LogicalUnitRegistryLoadedEntry[] ownedSnapshot;
        try
        {
            // Own the enumerable boundary once so validation and insertion observe the same immutable sequence.
            ownedSnapshot = entries.ToArray();
        }
        catch (Exception exception) when (!IsFatalLoadException(exception))
        {
            return OperationResult<int>.Fail(
                "Loaded unit entries could not be materialized.",
                "INVALID_LOADED_UNIT_ENTRY");
        }

        var acceptedRegistrationOrders = new HashSet<long>();
        HashSet<long>? existingRegistrationOrders = null;
        if (!replaceExisting)
        {
            existingRegistrationOrders = new HashSet<long>(
                _entries.Select(candidate => candidate.Descriptor.RegistrationOrder));
        }

        var acceptedMaximumRegistrationOrder = 0L;

        // Validate the complete owned snapshot before the first registry or counter mutation.
        foreach (var entry in ownedSnapshot)
        {
            if (entry?.Descriptor is null || entry.Unit is null)
                return OperationResult<int>.Fail(
                    "Loaded unit entries require descriptor and unit.",
                    "INVALID_LOADED_UNIT_ENTRY");

            var descriptor = entry.Descriptor;

            if (!IsCanonicalIdentity(descriptor.UnitId) ||
                !IsCanonicalIdentity(descriptor.UnitRole) ||
                !IsCanonicalIdentity(descriptor.DomainTag) ||
                !IsCanonicalIdentity(descriptor.Version))
            {
                return OperationResult<int>.Fail(
                    "Loaded unit identity fields must be canonical.",
                    "LOADED_UNIT_IDENTITY_INVALID");
            }

            if (!Enum.IsDefined(typeof(LogicalUnitTrustState), descriptor.TrustState))
            {
                return OperationResult<int>.Fail(
                    "Loaded unit trust state is invalid.",
                    "LOADED_UNIT_TRUST_STATE_INVALID");
            }

            if (descriptor.RegistrationOrder <= 0 ||
                descriptor.RegistrationOrder == long.MaxValue)
            {
                return OperationResult<int>.Fail(
                    "Loaded unit registration order is invalid.",
                    "LOADED_UNIT_REGISTRATION_ORDER_INVALID");
            }

            if (!acceptedRegistrationOrders.Add(descriptor.RegistrationOrder) ||
                existingRegistrationOrders?.Contains(descriptor.RegistrationOrder) == true)
            {
                return OperationResult<int>.Fail(
                    "Loaded unit registration order conflicts with another registry tuple.",
                    "LOADED_UNIT_REGISTRATION_ORDER_CONFLICT");
            }

            if (!TryConstructLoadedUnitInterface(descriptor.InputType, descriptor.OutputType, out var interfaceType))
            {
                return OperationResult<int>.Fail(
                    "Loaded unit type boundary is invalid.",
                    "LOADED_UNIT_TYPE_BOUNDARY_INVALID");
            }

            if (!interfaceType.IsInstanceOfType(entry.Unit))
                return OperationResult<int>.Fail(
                    "Loaded unit does not implement its declared logical-unit interface.",
                    "INCOMPATIBLE_LOADED_UNIT");

            acceptedMaximumRegistrationOrder = Math.Max(
                acceptedMaximumRegistrationOrder,
                descriptor.RegistrationOrder);
        }

        if (!replaceExisting &&
            ownedSnapshot.Length > int.MaxValue - _entries.Count)
        {
            return OperationResult<int>.Fail(
                "Loaded unit entries exceed registry capacity.",
                "LOADED_UNIT_CAPACITY_EXCEEDED");
        }

        // Reserve all required capacity before changing logical registry state.
        var requiredCapacity = replaceExisting
            ? ownedSnapshot.Length
            : _entries.Count + ownedSnapshot.Length;
        _entries.EnsureCapacity(requiredCapacity);

        // Replace or append entries exactly as loaded; do not allocate new registration identities.
        if (replaceExisting)
            _entries.Clear();

        foreach (var entry in ownedSnapshot)
            _entries.Add((entry.Descriptor, entry.Unit));

        // Continue future Register calls after the largest accepted registration order without ever decreasing.
        _nextRegistrationOrder = Math.Max(_nextRegistrationOrder, acceptedMaximumRegistrationOrder);

        return OperationResult<int>.Ok(ownedSnapshot.Length);
    }

    private static bool IsCanonicalIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !StringComparer.Ordinal.Equals(value, value.Trim()))
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character) ||
                character is '\u0085' or '\u2028' or '\u2029')
            {
                return false;
            }

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length ||
                    !char.IsLowSurrogate(value[index + 1]))
                {
                    return false;
                }

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    private static bool IsExecutableTrustFloor(LogicalUnitTrustState trustState)
    {
        return trustState is LogicalUnitTrustState.Candidate
            or LogicalUnitTrustState.Probationary
            or LogicalUnitTrustState.Trusted;
    }

    private static bool TryConstructLoadedUnitInterface(
        Type? inputType,
        Type? outputType,
        out Type interfaceType)
    {
        interfaceType = null!;
        if (inputType is null || outputType is null)
            return false;

        try
        {
            interfaceType = typeof(ILogicalUnit<,>).MakeGenericType(inputType, outputType);
            return !interfaceType.ContainsGenericParameters;
        }
        catch (Exception exception) when (!IsFatalLoadException(exception))
        {
            interfaceType = null!;
            return false;
        }
    }

    private static bool IsFatalLoadException(Exception exception)
    {
        return exception is OutOfMemoryException or
                   StackOverflowException or
                   AccessViolationException or
                   AppDomainUnloadedException or
                   BadImageFormatException ||
               StringComparer.Ordinal.Equals(
                   exception.GetType().FullName,
                   "System.Threading.ThreadAbortException");
    }
}
