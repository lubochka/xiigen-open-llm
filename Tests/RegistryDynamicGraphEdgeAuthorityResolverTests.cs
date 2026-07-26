// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

// These tests call the production resolver directly and pin its fail-closed admission boundary.
public sealed class RegistryDynamicGraphEdgeAuthorityResolverTests
{
    private const string UnitId = "unit-edge-authority";
    private const string UnitRole = "edge-authority";
    private const string DomainTag = "software-development";

    [Fact]
    public void Constructor_MissingRegistry_ThrowsBeforeResolverCanBeUsed()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new RegistryDynamicGraphEdgeAuthorityResolver(null!));

        Assert.Equal("registry", exception.ParamName);
    }

    [Fact]
    public void Resolve_CanonicalProbationaryLearnedTuple_ReturnsExactDescriptorAndUnitWithoutExecution()
    {
        var registry = new InMemoryLogicalUnitRegistry();
        var unit = new NeverExecuteAuthorityUnit();
        var registration = registry.Register(
            UnitId,
            UnitRole,
            DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            isLearned: true,
            unit);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        var result = new RegistryDynamicGraphEdgeAuthorityResolver(registry)
            .Resolve(CreateEdge(), DomainTag);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var resolved = Assert.IsType<ResolvedDynamicGraphEdgeAuthority>(result.Value);
        Assert.Same(registration.Value, resolved.Descriptor);
        Assert.Same(unit, resolved.Unit);
        Assert.Equal(UnitId, resolved.Descriptor.UnitId);
        Assert.Equal(UnitRole, resolved.Descriptor.UnitRole);
        Assert.Equal(DomainTag, resolved.Descriptor.DomainTag);
        Assert.Equal(LogicalUnitTrustState.Probationary, resolved.Descriptor.TrustState);
        Assert.True(resolved.Descriptor.IsLearned);
        Assert.Equal(typeof(EdgeAuthorityInput), resolved.Descriptor.InputType);
        Assert.Equal(typeof(EdgeAuthorityDecision), resolved.Descriptor.OutputType);
        Assert.Single(registry.ListDescriptors());
        Assert.Equal(0, unit.ExecutionCount);
    }

    [Fact]
    public void Resolve_NullEdge_FailsWithoutReadingOrMutatingRegistry()
    {
        var registry = new InMemoryLogicalUnitRegistry();

        var result = new RegistryDynamicGraphEdgeAuthorityResolver(registry)
            .Resolve(null!, DomainTag);

        AssertFailure(
            result,
            "EDGE_AUTHORITY_EDGE_REQUIRED",
            "Dynamic graph edge is required.");
        Assert.Empty(registry.ListDescriptors());
    }

    [Theory]
    [InlineData(EdgeMutation.MissingUnitId, DomainTag, "EDGE_AUTHORITY_UNIT_ID_REQUIRED", "Edge authority unit id is required.")]
    [InlineData(EdgeMutation.NonCanonicalUnitId, DomainTag, "EDGE_AUTHORITY_UNIT_ID_NOT_CANONICAL", "Edge authority unit id must be canonical.")]
    [InlineData(EdgeMutation.MissingLevel, DomainTag, "EDGE_AUTHORITY_UNIT_LEVEL_REQUIRED", "Edge authority unit level is required.")]
    [InlineData(EdgeMutation.WrongLevel, DomainTag, "EDGE_AUTHORITY_UNIT_LEVEL_MISMATCH", "Edge authority unit level must be L2.")]
    [InlineData(EdgeMutation.FinalAuthority, DomainTag, "EDGE_AUTHORITY_FINAL_AUTHORITY_FORBIDDEN", "Learned edge authority cannot claim deterministic final authority.")]
    [InlineData(EdgeMutation.None, "", "EDGE_AUTHORITY_DOMAIN_REQUIRED", "Edge authority domain tag is required.")]
    [InlineData(EdgeMutation.None, " software-development", "EDGE_AUTHORITY_DOMAIN_NOT_CANONICAL", "Edge authority domain tag must be canonical.")]
    public void Resolve_InvalidEdgeOrDomain_FailsWithStableCodeBeforeRegistryLookup(
        EdgeMutation mutation,
        string domainTag,
        string expectedCode,
        string expectedMessage)
    {
        var registry = new InMemoryLogicalUnitRegistry();

        var result = new RegistryDynamicGraphEdgeAuthorityResolver(registry)
            .Resolve(CreateEdge(mutation), domainTag);

        AssertFailure(result, expectedCode, expectedMessage);
        Assert.Empty(registry.ListDescriptors());
    }

    [Fact]
    public void Resolve_ExactUnitIdIsAbsent_PropagatesStableRegistryFailure()
    {
        var registry = new InMemoryLogicalUnitRegistry();

        var result = new RegistryDynamicGraphEdgeAuthorityResolver(registry)
            .Resolve(CreateEdge(), DomainTag);

        AssertFailure(
            result,
            "LOGICAL_UNIT_ID_NOT_FOUND",
            $"No logical unit registered for exact id '{UnitId}'.");
        Assert.Empty(registry.ListDescriptors());
    }

    [Fact]
    public void Resolve_ExactUnitHasWrongExecutableType_PropagatesRegistryTypeFailureWithoutExecution()
    {
        var registry = new InMemoryLogicalUnitRegistry();
        var wrongTypeUnit = new NeverExecuteStringUnit();
        var registration = registry.Register(
            UnitId,
            UnitRole,
            DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            isLearned: true,
            wrongTypeUnit);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        var result = new RegistryDynamicGraphEdgeAuthorityResolver(registry)
            .Resolve(CreateEdge(), DomainTag);

        AssertFailure(
            result,
            "LOGICAL_UNIT_TYPE_MISMATCH",
            $"Latest logical unit '{UnitId}' does not implement the requested typed interface.");
        Assert.Single(registry.ListDescriptors());
        Assert.Equal(0, wrongTypeUnit.ExecutionCount);
    }

    [Fact]
    public void Resolve_NewerExactIdTupleIsInvalid_FailsClosedInsteadOfFallingBackToOlderValidTuple()
    {
        var registry = new InMemoryLogicalUnitRegistry();
        var olderValidUnit = new NeverExecuteAuthorityUnit();
        var newerWrongRoleUnit = new NeverExecuteAuthorityUnit();
        var older = registry.Register(
            UnitId,
            UnitRole,
            DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            isLearned: true,
            olderValidUnit);
        var newer = registry.Register(
            UnitId,
            "not-edge-authority",
            DomainTag,
            "2.0.0",
            LogicalUnitTrustState.Trusted,
            isLearned: true,
            newerWrongRoleUnit);
        Assert.True(older.IsSuccess, older.ErrorMessage);
        Assert.True(newer.IsSuccess, newer.ErrorMessage);

        var result = new RegistryDynamicGraphEdgeAuthorityResolver(registry)
            .Resolve(CreateEdge(), DomainTag);

        AssertFailure(
            result,
            "EDGE_AUTHORITY_ROLE_MISMATCH",
            "Resolved edge authority role is invalid.");
        Assert.Equal(2, registry.ListDescriptors().Count);
        Assert.Equal(0, olderValidUnit.ExecutionCount);
        Assert.Equal(0, newerWrongRoleUnit.ExecutionCount);
    }

    [Theory]
    [InlineData(DescriptorMutation.WrongRole, "EDGE_AUTHORITY_ROLE_MISMATCH", "Resolved edge authority role is invalid.")]
    [InlineData(DescriptorMutation.WrongDomain, "EDGE_AUTHORITY_DOMAIN_MISMATCH", "Resolved edge authority domain does not match the requested domain.")]
    [InlineData(DescriptorMutation.WrongInputType, "EDGE_AUTHORITY_INPUT_TYPE_MISMATCH", "Resolved edge authority input type is invalid.")]
    [InlineData(DescriptorMutation.WrongOutputType, "EDGE_AUTHORITY_OUTPUT_TYPE_MISMATCH", "Resolved edge authority output type is invalid.")]
    [InlineData(DescriptorMutation.NotLearned, "EDGE_AUTHORITY_UNIT_NOT_LEARNED", "Resolved edge authority unit is not learned.")]
    [InlineData(DescriptorMutation.InvalidTrust, "EDGE_AUTHORITY_TRUST_STATE_INVALID", "Resolved edge authority trust state is invalid.")]
    [InlineData(DescriptorMutation.Rejected, "EDGE_AUTHORITY_UNIT_REJECTED", "Resolved edge authority unit is rejected.")]
    [InlineData(DescriptorMutation.Candidate, "EDGE_AUTHORITY_TRUST_TOO_LOW", "Resolved edge authority unit is below the minimum trust state.")]
    public void Resolve_MutatedRegistryDescriptor_FailsClosedWithoutExecutingUnit(
        DescriptorMutation mutation,
        string expectedCode,
        string expectedMessage)
    {
        var unit = new NeverExecuteAuthorityUnit();
        var descriptor = CreateDescriptor(mutation);
        var registry = CreateRegistryWithExactTuple(descriptor, unit);

        var result = new RegistryDynamicGraphEdgeAuthorityResolver(registry)
            .Resolve(CreateEdge(), DomainTag);

        AssertFailure(result, expectedCode, expectedMessage);
        Assert.Equal(descriptor, Assert.Single(registry.ListDescriptors()));
        Assert.Equal(0, unit.ExecutionCount);
    }

    private static TestEdge CreateEdge(EdgeMutation mutation = EdgeMutation.None)
    {
        var edge = new TestEdge(
            "edge-authority-001",
            "node-source",
            ["node-target"],
            GraphEdgeKind.FanOut,
            ParallelEligible: true,
            UnitId,
            "L2",
            DeterministicFinalAuthority: false,
            "baseline-edge-authority",
            "baseline_candidate_generator",
            ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
            "source-proof:resolver-test",
            "cutover-proof:resolver-test",
            "ablation-proof:resolver-test");

        return mutation switch
        {
            EdgeMutation.MissingUnitId => edge with { AuthorityUnitId = "" },
            EdgeMutation.NonCanonicalUnitId => edge with { AuthorityUnitId = $"{UnitId} " },
            EdgeMutation.MissingLevel => edge with { AuthorityUnitLevel = "" },
            EdgeMutation.WrongLevel => edge with { AuthorityUnitLevel = "L3" },
            EdgeMutation.FinalAuthority => edge with { DeterministicFinalAuthority = true },
            _ => edge
        };
    }

    private static LogicalUnitDescriptor CreateDescriptor(DescriptorMutation mutation)
    {
        var descriptor = new LogicalUnitDescriptor(
            UnitId,
            UnitRole,
            DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            IsLearned: true,
            typeof(EdgeAuthorityInput),
            typeof(EdgeAuthorityDecision),
            RegistrationOrder: 7);

        return mutation switch
        {
            DescriptorMutation.WrongRole => descriptor with { UnitRole = "not-edge-authority" },
            DescriptorMutation.WrongDomain => descriptor with { DomainTag = "other-domain" },
            DescriptorMutation.WrongInputType => descriptor with { InputType = typeof(string) },
            DescriptorMutation.WrongOutputType => descriptor with { OutputType = typeof(string) },
            DescriptorMutation.NotLearned => descriptor with { IsLearned = false },
            DescriptorMutation.InvalidTrust => descriptor with { TrustState = (LogicalUnitTrustState)int.MaxValue },
            DescriptorMutation.Rejected => descriptor with { TrustState = LogicalUnitTrustState.Rejected },
            DescriptorMutation.Candidate => descriptor with { TrustState = LogicalUnitTrustState.Candidate },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null)
        };
    }

    private static InMemoryLogicalUnitRegistry CreateRegistryWithExactTuple(
        LogicalUnitDescriptor descriptor,
        NeverExecuteAuthorityUnit unit)
    {
        var registry = new InMemoryLogicalUnitRegistry();
        var canUseValidatedLoad = Enum.IsDefined(descriptor.TrustState)
            && descriptor.InputType == typeof(EdgeAuthorityInput)
            && descriptor.OutputType == typeof(EdgeAuthorityDecision);

        if (canUseValidatedLoad)
        {
            var load = registry.LoadEntries([new LogicalUnitRegistryLoadedEntry(descriptor, unit)]);
            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.Equal(1, load.Value);
            return registry;
        }

        // Corrupt persisted tuples cannot enter through LoadEntries, so inject only to prove the
        // resolver's redundant defense remains fail-closed if registry state is damaged in memory.
        var entriesField = typeof(InMemoryLogicalUnitRegistry).GetField(
            "_entries",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(entriesField);
        var entries = Assert.IsType<List<(LogicalUnitDescriptor Descriptor, object Unit)>>(
            entriesField.GetValue(registry));
        entries.Add((descriptor, unit));
        return registry;
    }

    private static void AssertFailure(
        OperationResult<ResolvedDynamicGraphEdgeAuthority> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    public enum EdgeMutation
    {
        None,
        MissingUnitId,
        NonCanonicalUnitId,
        MissingLevel,
        WrongLevel,
        FinalAuthority
    }

    public enum DescriptorMutation
    {
        WrongRole,
        WrongDomain,
        WrongInputType,
        WrongOutputType,
        NotLearned,
        InvalidTrust,
        Rejected,
        Candidate
    }

    private sealed record TestEdge(
        string EdgeId,
        string FromNodeId,
        IReadOnlyList<string> ToNodeIds,
        GraphEdgeKind Kind,
        bool ParallelEligible,
        string AuthorityUnitId,
        string AuthorityUnitLevel,
        bool DeterministicFinalAuthority,
        string? BaselineUnitId,
        string BaselineRole,
        IReadOnlyList<string> ExcludedLeakageSignals,
        string SourceProof,
        string CutoverProof,
        string AblationProofRef) : IDynamicGraphEdge;

    private sealed class NeverExecuteAuthorityUnit
        : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
    {
        public int ExecutionCount { get; private set; }

        public Task<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>> ExecuteAsync(
            LogicalUnitRequest<EdgeAuthorityInput> request,
            CancellationToken cancellationToken = default)
        {
            ExecutionCount++;
            throw new InvalidOperationException("The resolver must not execute an authority unit.");
        }
    }

    private sealed class NeverExecuteStringUnit : ILogicalUnit<string, string>
    {
        public int ExecutionCount { get; private set; }

        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            ExecutionCount++;
            throw new InvalidOperationException("The resolver must not execute a wrong-type unit.");
        }
    }
}
