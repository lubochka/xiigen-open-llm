namespace ExperimentsV2.LogicalFlow;

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
        // Reject empty unit ids because they cannot be referenced by flows or evidence.
        if (string.IsNullOrWhiteSpace(unitId))
            return OperationResult<LogicalUnitDescriptor>.Fail("Unit id cannot be empty.");

        // Reject empty roles because role is the slot the unit fills.
        if (string.IsNullOrWhiteSpace(unitRole))
            return OperationResult<LogicalUnitDescriptor>.Fail("Unit role cannot be empty.");

        // Reject empty domain tags because domain is part of unit scoping.
        if (string.IsNullOrWhiteSpace(domainTag))
            return OperationResult<LogicalUnitDescriptor>.Fail("Domain tag cannot be empty.");

        // Reject empty versions because swappability and rollback need version identity.
        if (string.IsNullOrWhiteSpace(version))
            return OperationResult<LogicalUnitDescriptor>.Fail("Version cannot be empty.");

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
            ++_nextRegistrationOrder);

        // Store the descriptor and executable unit together.
        _entries.Add((descriptor, unit));

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
            return OperationResult<ILogicalUnit<TInput, TOutput>>.Fail($"No logical unit registered for role '{unitRole}' and domain '{domainTag}' at trust '{minimumTrustState}' or higher.");

        // Return the executable unit under its typed interface.
        return OperationResult<ILogicalUnit<TInput, TOutput>>.Ok((ILogicalUnit<TInput, TOutput>)entry.Unit);
    }

    // Return descriptors for audit and tests without exposing executable unit instances.
    public IReadOnlyList<LogicalUnitDescriptor> ListDescriptors()
    {
        // Project the stored descriptors into a read-only list.
        return _entries.Select(entry => entry.Descriptor).ToArray();
    }
}
