// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

// Enumerates a small deterministic cross-product of one-node, one-edge plans and four fixed flag profiles.
public sealed class AxisSweepCombinationStrategy : IGraphCombinationStrategy
{
    private const string RequestRequiredCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_REQUEST_REQUIRED";
    private const string RequestInvalidCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_REQUEST_INVALID";
    private const string CountOutOfRangeCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_COUNT_OUT_OF_RANGE";
    private const string BudgetInvalidCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_BUDGET_INVALID";
    private const string AxisInvalidCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_AXIS_INVALID";
    private const string HashCollisionCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_HASH_COLLISION";
    private const string EnumerationExceptionCode = "DYNAMIC_GRAPH_COMBINATION_STRATEGY_ENUMERATION_EXCEPTION";
    private const int MinimumCombinationCount = 2;
    private const int MaximumCombinationCount = 8;
    private readonly bool _useDeterministicFixtureBaseline;

    public AxisSweepCombinationStrategy()
        : this(useDeterministicFixtureBaseline: true)
    {
    }

    public AxisSweepCombinationStrategy(bool useDeterministicFixtureBaseline)
    {
        _useDeterministicFixtureBaseline = useDeterministicFixtureBaseline;
    }

    public OperationResult<IReadOnlyList<CombinationPlan>> Enumerate(
        GraphCandidateRunRequest request)
    {
        var validation = Validate(request);
        if (!validation.IsSuccess)
            return validation.MapError<IReadOnlyList<CombinationPlan>>();

        try
        {
            var nodeIds = request.InputUnitRefs
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var edgeKeys = request.InputGraphRefs
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var flagProfiles = CreateFlagProfiles();
            var tupleSignatures = new HashSet<string>(StringComparer.Ordinal);
            var signaturesById = new Dictionary<string, string>(StringComparer.Ordinal);
            var plans = new List<CombinationPlan>(request.CombinationCount);

            foreach (var nodeId in nodeIds)
            {
                foreach (var edgeKey in edgeKeys)
                {
                    foreach (var flags in flagProfiles)
                    {
                        var signature = CreateCanonicalSignature(nodeId, edgeKey, flags);
                        if (!tupleSignatures.Add(signature))
                            continue;

                        var combinationId = CreateCombinationId(signature);
                        if (signaturesById.TryGetValue(combinationId, out var existingSignature))
                        {
                            if (!StringComparer.Ordinal.Equals(existingSignature, signature))
                            {
                                return OperationResult<IReadOnlyList<CombinationPlan>>.Fail(
                                    "Dynamic graph combination identity collision was detected.",
                                    HashCollisionCode);
                            }

                            continue;
                        }

                        signaturesById.Add(combinationId, signature);
                        plans.Add(new CombinationPlan(
                            Index: plans.Count,
                            CombinationId: combinationId,
                            NodeSetIds: [nodeId],
                            EdgeOrderKey: edgeKey,
                            Flags: flags with { }));

                        if (plans.Count == request.CombinationCount)
                            return Success(plans);
                    }
                }
            }

            return Success(plans);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<IReadOnlyList<CombinationPlan>>.Fail(
                "Dynamic graph combination enumeration failed unexpectedly.",
                EnumerationExceptionCode);
        }
    }

    private static OperationResult<bool> Validate(GraphCandidateRunRequest? request)
    {
        if (request is null)
        {
            return OperationResult<bool>.Fail(
                "Dynamic graph combination request is required.",
                RequestRequiredCode);
        }

        if (string.IsNullOrWhiteSpace(request.ScenarioId)
            || string.IsNullOrWhiteSpace(request.Query)
            || string.IsNullOrWhiteSpace(request.LegalScope)
            || string.IsNullOrWhiteSpace(request.TraceRef)
            || !AreRequiredRefsValid(request.ProvenanceRefs))
        {
            return OperationResult<bool>.Fail(
                "Dynamic graph combination request is invalid.",
                RequestInvalidCode);
        }

        if (request.CombinationCount < MinimumCombinationCount
            || request.CombinationCount > MaximumCombinationCount)
        {
            return OperationResult<bool>.Fail(
                "Dynamic graph combination count must be between two and eight.",
                CountOutOfRangeCode);
        }

        if (request.Budget is null
            || request.Budget.MaxCombinations < request.CombinationCount
            || request.Budget.MaxCombinations > MaximumCombinationCount
            || request.Budget.MaxParallelBranches < 1
            || request.Budget.MaxParallelCombinationRuns < 1
            || request.Budget.MaxParallelCombinationRuns > request.Budget.MaxCombinations
            || request.Budget.Timeout is { } timeout && timeout <= TimeSpan.Zero)
        {
            return OperationResult<bool>.Fail(
                "Dynamic graph combination budget is invalid.",
                BudgetInvalidCode);
        }

        if (!AreRequiredRefsValid(request.InputUnitRefs)
            || !AreRequiredRefsValid(request.InputGraphRefs))
        {
            return OperationResult<bool>.Fail(
                "Dynamic graph combination axes are invalid.",
                AxisInvalidCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool AreRequiredRefsValid(string[]? values)
        => values is { Length: > 0 }
           && values.All(value => !string.IsNullOrWhiteSpace(value));

    private IReadOnlyList<LogicalExecutionFlags> CreateFlagProfiles()
        => Array.AsReadOnly(new[]
        {
            CreateFlags(_useDeterministicFixtureBaseline, useContextCarryForward: false, useE35DependencyDetection: false, useLowConfidenceLessonSignal: false),
            CreateFlags(_useDeterministicFixtureBaseline, useContextCarryForward: true, useE35DependencyDetection: false, useLowConfidenceLessonSignal: false),
            CreateFlags(_useDeterministicFixtureBaseline, useContextCarryForward: true, useE35DependencyDetection: true, useLowConfidenceLessonSignal: false),
            CreateFlags(_useDeterministicFixtureBaseline, useContextCarryForward: true, useE35DependencyDetection: true, useLowConfidenceLessonSignal: true)
        });

    private static LogicalExecutionFlags CreateFlags(
        bool useDeterministicFixtureBaseline,
        bool useContextCarryForward,
        bool useE35DependencyDetection,
        bool useLowConfidenceLessonSignal)
        => new()
        {
            UseDeterministicFixtureBaseline = useDeterministicFixtureBaseline,
            UseContextCarryForward = useContextCarryForward,
            UseE35DependencyDetection = useE35DependencyDetection,
            UseLowConfidenceLessonSignal = useLowConfidenceLessonSignal
        };

    private static string CreateCanonicalSignature(
        string nodeId,
        string edgeKey,
        LogicalExecutionFlags flags)
        => string.Join(
            '\n',
            "axis-v1",
            "nodes:1",
            $"node:{Encoding.UTF8.GetByteCount(nodeId)}:{nodeId}",
            $"edge:{Encoding.UTF8.GetByteCount(edgeKey)}:{edgeKey}",
            $"flags:{CreateFlagKey(flags)}");

    private static string CreateFlagKey(LogicalExecutionFlags flags)
    {
        var bits = new[]
        {
            flags.UseCompiledPredicateRepresentation,
            flags.UseDomainScopedRetrieval,
            flags.UseLowConfidenceLessonSignal,
            flags.UseLearnedUnitWhenAvailable,
            flags.UseTraceEvidence,
            flags.UseContextCarryForward,
            flags.UseIdleLearningScheduling,
            flags.UseE35DependencyDetection,
            flags.UseSkillRetriever,
            flags.UseReplayFusionRanking,
            flags.UseArbiterAuthority,
            flags.UseKnowledgeEdgeDetection,
            flags.UseInternalFirstExecution,
            flags.RequireExternalAiApproval,
            flags.RejectArbiterContradictions,
            flags.UseDeterministicFixtureBaseline,
            flags.WriteEvidenceArtifacts,
            flags.CompareAgainstOracle,
            flags.FailOnMetricDrift
        };

        return new string(bits.Select(value => value ? '1' : '0').ToArray());
    }

    private static string CreateCombinationId(string signature)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(signature));
        return $"combination-{Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    private static bool IsNonFatalException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    private static OperationResult<IReadOnlyList<CombinationPlan>> Success(
        IEnumerable<CombinationPlan> plans)
    {
        var snapshots = plans
            .Select(plan => new CombinationPlan(
                plan.Index,
                plan.CombinationId,
                plan.NodeSetIds.ToArray(),
                plan.EdgeOrderKey,
                plan.Flags with { }))
            .ToArray();
        return OperationResult<IReadOnlyList<CombinationPlan>>.Ok(Array.AsReadOnly(snapshots));
    }
}
