using ExperimentsV2;
using ExperimentsV2.LogicalFlow;

namespace LlmFromScratch.Tests;

public sealed class LogicalUnitRegistryTests
{
    [Fact]
    public void LogicalUnitRegistry_RegistersAndResolvesT34Splitter()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalUnitRegistry();

        // Register the T34 node-01 logical unit.
        var descriptor = RegisterT34Splitter(registry, "1.0.0", LogicalUnitTrustState.Trusted, isLearned: false);

        // The descriptor must be returned through the shared operation envelope.
        Assert.True(descriptor.IsSuccess, descriptor.ErrorMessage);

        // Resolve the same unit by exact id, role, domain, and generic type.
        var resolved = registry.Resolve<T34PhaseTransitionProof.PhaseContextPacket, T34PhaseTransitionProof.DomainPacket[]>(
            T34TransitionDomainSplitterUnit.Id,
            T34TransitionDomainSplitterUnit.Role,
            T34TransitionDomainSplitterUnit.DomainTag);

        // The registry must find the executable unit.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // The resolved unit must be the T34 transition splitter type.
        Assert.IsType<T34TransitionDomainSplitterUnit>(resolved.Value);
    }

    [Fact]
    public void LogicalUnitRegistry_MissingUnit_FailsCleanly()
    {
        // Create an empty registry.
        var registry = new InMemoryLogicalUnitRegistry();

        // Try to resolve a unit that was never registered.
        var resolved = registry.Resolve<string, string>("missing", "role", "domain");

        // Missing units must fail through the shared operation envelope.
        Assert.False(resolved.IsSuccess);

        // The failure must name the missing unit id.
        Assert.Contains("missing", resolved.ErrorMessage);
    }

    [Fact]
    public void LogicalUnitRegistry_ListDescriptors_ExposesAuditMetadata()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalUnitRegistry();

        // Register the T34 unit as a trusted deterministic unit.
        RegisterT34Splitter(registry, "1.0.0", LogicalUnitTrustState.Trusted, isLearned: false);

        // Read descriptors for audit.
        var descriptors = registry.ListDescriptors();

        // Exactly one descriptor should exist.
        var descriptor = Assert.Single(descriptors);

        // The descriptor must carry unit identity.
        Assert.Equal(T34TransitionDomainSplitterUnit.Id, descriptor.UnitId);

        // The descriptor must carry role identity.
        Assert.Equal(T34TransitionDomainSplitterUnit.Role, descriptor.UnitRole);

        // The descriptor must carry domain identity.
        Assert.Equal(T34TransitionDomainSplitterUnit.DomainTag, descriptor.DomainTag);

        // The descriptor must record that this first unit is not learned.
        Assert.False(descriptor.IsLearned);
    }

    [Fact]
    public void LogicalUnitRegistry_ResolveLatest_ChoosesNewestTrustedVariant()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalUnitRegistry();

        // Register the deterministic first version.
        RegisterT34Splitter(registry, "1.0.0", LogicalUnitTrustState.Trusted, isLearned: false);

        // Register a learned second version with the same slot identity.
        var learned = new T34TransitionDomainSplitterUnit();
        registry.Register(
            "unit-t34-transition-domain-splitter-v2",
            T34TransitionDomainSplitterUnit.Role,
            T34TransitionDomainSplitterUnit.DomainTag,
            "2.0.0",
            LogicalUnitTrustState.Trusted,
            isLearned: true,
            learned);

        // Resolve the latest trusted unit by role and domain.
        var resolved = registry.ResolveLatest<T34PhaseTransitionProof.PhaseContextPacket, T34PhaseTransitionProof.DomainPacket[]>(
            T34TransitionDomainSplitterUnit.Role,
            T34TransitionDomainSplitterUnit.DomainTag,
            LogicalUnitTrustState.Trusted);

        // The latest trusted unit must resolve.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // The latest descriptor must be the learned v2 unit.
        var latestDescriptor = registry.ListDescriptors().OrderByDescending(descriptor => descriptor.RegistrationOrder).First();

        // The latest descriptor must be marked as learned.
        Assert.True(latestDescriptor.IsLearned);

        // The latest descriptor must have the v2 id.
        Assert.Equal("unit-t34-transition-domain-splitter-v2", latestDescriptor.UnitId);
    }

    [Fact]
    public void LogicalUnitRegistry_RejectedUnits_AreNotResolvedAsLatest()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalUnitRegistry();

        // Register a trusted baseline unit.
        RegisterT34Splitter(registry, "1.0.0", LogicalUnitTrustState.Trusted, isLearned: false);

        // Register a newer rejected unit in the same slot.
        registry.Register(
            "unit-t34-transition-domain-splitter-rejected",
            T34TransitionDomainSplitterUnit.Role,
            T34TransitionDomainSplitterUnit.DomainTag,
            "2.0.0",
            LogicalUnitTrustState.Rejected,
            isLearned: true,
            new T34TransitionDomainSplitterUnit());

        // Resolve latest with probationary-or-higher trust.
        var resolved = registry.ResolveLatest<T34PhaseTransitionProof.PhaseContextPacket, T34PhaseTransitionProof.DomainPacket[]>(
            T34TransitionDomainSplitterUnit.Role,
            T34TransitionDomainSplitterUnit.DomainTag);

        // The registry must skip the rejected unit and return the trusted baseline.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // The trusted baseline remains the only usable latest unit.
        Assert.DoesNotContain(registry.ListDescriptors().Where(descriptor => descriptor.TrustState != LogicalUnitTrustState.Rejected), descriptor => descriptor.UnitId.Contains("rejected", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void LogicalUnitRegistry_InvalidRegistration_FailsCleanly()
    {
        // Create the registry for this test.
        var registry = new InMemoryLogicalUnitRegistry();

        // Try to register a unit without a unit id.
        var result = registry.Register(
            "",
            T34TransitionDomainSplitterUnit.Role,
            T34TransitionDomainSplitterUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            isLearned: false,
            new T34TransitionDomainSplitterUnit());

        // Invalid registration must fail through the shared envelope.
        Assert.False(result.IsSuccess);

        // The failure must name the empty unit id.
        Assert.Contains("Unit id", result.ErrorMessage);
    }

    private static OperationResult<LogicalUnitDescriptor> RegisterT34Splitter(InMemoryLogicalUnitRegistry registry, string version, LogicalUnitTrustState trustState, bool isLearned)
    {
        // Register the T34 splitter in the registry slot used by Phase 29.
        return registry.Register(
            T34TransitionDomainSplitterUnit.Id,
            T34TransitionDomainSplitterUnit.Role,
            T34TransitionDomainSplitterUnit.DomainTag,
            version,
            trustState,
            isLearned,
            new T34TransitionDomainSplitterUnit());
    }
}
