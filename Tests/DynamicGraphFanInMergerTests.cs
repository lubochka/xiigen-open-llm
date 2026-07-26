// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;

// These tests pin the typed Plan-06 fan-in contract and its deterministic evidence-preserving baseline.
public sealed class DynamicGraphFanInMergerTests
{
    private const string MergeKey = "merge:invoice-review";
    private const string GroupPathOrderRef = "path-order:invoice-review";

    [Fact]
    public void FanInContracts_ExposeExactTypedOwnerShapeAndOneMergeMethod()
    {
        Assert.Equal(
            [FanInMergeRole.DeterministicFanInBaseline],
            Enum.GetValues<FanInMergeRole>());

        AssertCanonicalRecordShape<GraphAsyncBranchResult>(
            [
                ("CombinationId", typeof(string)),
                ("BranchId", typeof(string)),
                ("OverallVerdict", typeof(string)),
                ("Confidence", typeof(double)),
                ("SourceHashes", typeof(string[])),
                ("Inner", typeof(LogicalFlowResult)),
                ("MergeKey", typeof(string)),
                ("PathOrderRef", typeof(string)),
                ("ProvenanceRefs", typeof(string[])),
                ("TraceRefs", typeof(string[])),
                ("ErrorCodes", typeof(string[]))
            ]);
        AssertCanonicalRecordShape<GraphFanInConflict>(
            [
                ("FieldName", typeof(string)),
                ("ValuesByBranchId", typeof(IReadOnlyDictionary<string, string>))
            ]);
        AssertCanonicalRecordShape<GraphFanInMergeInput>(
            [
                ("RunId", typeof(string)),
                ("Branches", typeof(GraphAsyncBranchResult[])),
                ("MergeKey", typeof(string)),
                ("PathOrderRef", typeof(string)),
                ("LegalScope", typeof(string)),
                ("SourceRefs", typeof(string[])),
                ("ProvenanceRefs", typeof(string[])),
                ("TraceRefs", typeof(string[])),
                ("RejectedAlternatives", typeof(GraphAsyncBranchResult[])),
                ("Conflicts", typeof(GraphFanInConflict[])),
                ("FailureReasonCodes", typeof(string[])),
                ("UseContextCarryForward", typeof(bool))
            ]);
        AssertCanonicalRecordShape<GraphFanInMergeResult>(
            [
                ("Merged", typeof(LogicalFlowResult)),
                ("Context", typeof(LogicalContextPacket)),
                ("MergeRole", typeof(FanInMergeRole)),
                ("MergeOrderIsAuthority", typeof(bool)),
                ("MergedBranchCount", typeof(int)),
                ("RejectedAlternatives", typeof(IReadOnlyList<GraphAsyncBranchResult>)),
                ("Conflicts", typeof(IReadOnlyList<GraphFanInConflict>)),
                ("ProvenanceRefs", typeof(IReadOnlyList<string>)),
                ("TraceRefs", typeof(IReadOnlyList<string>)),
                ("FailureReasonCodes", typeof(IReadOnlyList<string>))
            ]);

        var merge = Assert.Single(typeof(IEdgeFanInMerger).GetMethods());
        Assert.Equal("Merge", merge.Name);
        Assert.Equal(typeof(OperationResult<GraphFanInMergeResult>), merge.ReturnType);
        var parameter = Assert.Single(merge.GetParameters());
        Assert.Equal("input", parameter.Name);
        Assert.Equal(typeof(GraphFanInMergeInput), parameter.ParameterType);

        Assert.Contains(typeof(IEdgeFanInMerger), typeof(DeterministicGraphFanInMerger).GetInterfaces());
        var productionMethods = typeof(DeterministicGraphFanInMerger).GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        Assert.Equal(["Merge"], productionMethods.Select(method => method.Name));
    }

    [Fact]
    public void Merge_FiveBranchPermutations_AreStructurallyIdenticalAndCanonicallyOrdered()
    {
        var branches = new[]
        {
            CreateBranch("branch-a", "path:01", "PASS", 0.91, true, "alpha"),
            CreateBranch("branch-b", "path:02", "PASS_WITH_GAPS", 0.63, false, "beta"),
            CreateBranch("branch-c", "path:03", "REJECT", 0.24, false, "gamma")
        };
        var permutations = new[]
        {
            new[] { 0, 1, 2 },
            new[] { 2, 1, 0 },
            new[] { 1, 2, 0 },
            new[] { 2, 0, 1 },
            new[] { 1, 0, 2 }
        };
        var merger = new DeterministicGraphFanInMerger();

        var results = permutations
            .Select(permutation => merger.Merge(CreateInput(
                permutation.Select(index => branches[index]).ToArray())))
            .ToArray();

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        var expectedMerged = StructuralJson(results[0].Value!.Merged);
        var expectedContext = StructuralJson(results[0].Value.Context);
        var expectedFullResult = StructuralJson(results[0].Value);
        Assert.All(results, result => Assert.Equal(expectedMerged, StructuralJson(result.Value!.Merged)));
        Assert.All(results, result => Assert.Equal(expectedContext, StructuralJson(result.Value!.Context)));
        Assert.All(results, result => Assert.Equal(expectedFullResult, StructuralJson(result.Value)));

        Assert.Equal(
            ["flow-branch-a", "flow-branch-b", "flow-branch-c"],
            results[0].Value!.Context.SourceFlowIds);
        Assert.Equal(DateTimeOffset.UnixEpoch, results[0].Value.Context.CreatedAtUtc);
        Assert.Equal("REJECT", results[0].Value.Merged.OverallVerdict);
    }

    [Fact]
    public void Merge_ConflictingBranchValues_PreservesTypedMapAndDoesNotOverwriteContext()
    {
        var left = CreateBranch(
            "branch-a",
            "path:01",
            metrics: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["decision"] = "left",
                ["shared"] = "same"
            });
        var right = CreateBranch(
            "branch-b",
            "path:02",
            metrics: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["decision"] = "right",
                ["shared"] = "same"
            });
        var conflictValues = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["branch-b"] = "P:5:right",
            ["branch-a"] = "P:4:left"
        };
        var conflict = new GraphFanInConflict("Metrics.decision", conflictValues);

        var result = new DeterministicGraphFanInMerger().Merge(
            CreateInput([right, left], conflicts: [conflict]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var value = result.Value!;
        var preserved = Assert.Single(value.Conflicts);
        Assert.Equal("Metrics.decision", preserved.FieldName);
        Assert.Equal(["branch-a", "branch-b"], preserved.ValuesByBranchId.Keys);
        Assert.Equal("P:4:left", preserved.ValuesByBranchId["branch-a"]);
        Assert.Equal("P:5:right", preserved.ValuesByBranchId["branch-b"]);
        Assert.Equal("same", value.Merged.Metrics["shared"]);
        Assert.Contains("branch-a", value.Merged.Metrics["decision"], StringComparison.Ordinal);
        Assert.Contains("left", value.Merged.Metrics["decision"], StringComparison.Ordinal);
        Assert.Contains("branch-b", value.Merged.Metrics["decision"], StringComparison.Ordinal);
        Assert.Contains("right", value.Merged.Metrics["decision"], StringComparison.Ordinal);
        Assert.NotEqual("left", value.Merged.Metrics["decision"]);
        Assert.NotEqual("right", value.Merged.Metrics["decision"]);
        Assert.Equal("left", value.Context.Fragments.Single(fragment => fragment.FlowId == "flow-branch-a").Metrics["decision"]);
        Assert.Equal("right", value.Context.Fragments.Single(fragment => fragment.FlowId == "flow-branch-b").Metrics["decision"]);
        Assert.Contains("CONFLICT_UNRESOLVED", value.FailureReasonCodes);
    }

    [Fact]
    public void Merge_ConflictEvidenceMustExactlyMatchEveryDerivedActiveBranchMetricConflict()
    {
        var left = CreateBranch(
            "branch-a",
            "path:01",
            metrics: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["decision"] = "left",
                ["left-only"] = "present"
            });
        var right = CreateBranch(
            "branch-b",
            "path:02",
            metrics: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["decision"] = "right"
            });
        var branches = new[] { right, left };
        var exact = CreateDerivedConflicts(branches);
        var merger = new DeterministicGraphFanInMerger();

        Assert.Equal(["Metrics.decision", "Metrics.left-only"], exact.Select(item => item.FieldName));
        var accepted = merger.Merge(CreateInput(branches, conflicts: exact));
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        var presenceConflict = accepted.Value!.Conflicts.Single(
            item => item.FieldName == "Metrics.left-only");
        Assert.Equal("P:7:present", presenceConflict.ValuesByBranchId["branch-a"]);
        Assert.Equal("M:0:", presenceConflict.ValuesByBranchId["branch-b"]);
        Assert.Contains("CONFLICT_UNRESOLVED", accepted.Value.FailureReasonCodes);
        Assert.False(accepted.Value.Merged.Summary.Passed);
        AssertFailure(merger.Merge(CreateInput(branches, conflicts: [])), "CONFLICT_UNRESOLVED");
        AssertFailure(
            merger.Merge(CreateInput(
                branches,
                conflicts:
                [
                    exact[0],
                    new GraphFanInConflict(
                        "Metrics.forged",
                        new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["branch-a"] = "P:4:left",
                            ["branch-b"] = "P:5:right"
                        })
                ])),
            "CONFLICT_UNRESOLVED");
        AssertFailure(
            merger.Merge(CreateInput(
                branches,
                conflicts:
                [
                    new GraphFanInConflict(
                        "Metrics.decision",
                        new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["branch-a"] = "P:4:left"
                        }),
                    exact[1]
                ])),
            "CONFLICT_UNRESOLVED");
        AssertFailure(
            merger.Merge(CreateInput(
                branches,
                conflicts:
                [
                    new GraphFanInConflict(
                        "Metrics.decision",
                        new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["branch-a"] = "P:4:left",
                            ["branch-b"] = "P:4:left"
                        }),
                    exact[1]
                ])),
            "CONFLICT_UNRESOLVED");
        AssertFailure(
            merger.Merge(CreateInput(
                branches,
                conflicts:
                [
                    new GraphFanInConflict(
                        "Metrics.decision",
                        new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["branch-a"] = "P:4:left",
                            ["branch-b"] = "P:7:tampered"
                        }),
                    exact[1]
                ])),
            "CONFLICT_UNRESOLVED");

        var identical = new[]
        {
            CreateBranch(
                "branch-a",
                "path:01",
                metrics: new Dictionary<string, string>(StringComparer.Ordinal) { ["decision"] = "same" }),
            CreateBranch(
                "branch-b",
                "path:02",
                metrics: new Dictionary<string, string>(StringComparer.Ordinal) { ["decision"] = "same" })
        };
        AssertFailure(
            merger.Merge(CreateInput(
                identical,
                conflicts:
                [
                    new GraphFanInConflict(
                        "Metrics.decision",
                        new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["branch-a"] = "P:4:same",
                            ["branch-b"] = "P:4:same"
                        })
                ])),
            "CONFLICT_UNRESOLVED");
    }

    [Fact]
    public void Merge_ErrorsProvenanceTracesAndTransitionSignals_AreAllPreservedStably()
    {
        var signalA = new PhaseTransitionSignal(
            Domain.CODE_Q,
            "Review branch A.",
            "node-a",
            "phase-a",
            ["signal-evidence-a"]);
        var signalB = new PhaseTransitionSignal(
            Domain.PLAN,
            "Repair branch B.",
            "node-b",
            "phase-b",
            ["signal-evidence-b"]);
        var branchA = CreateBranch(
            "branch-a",
            "path:01",
            errorCodes: ["BRANCH_A_ERROR"],
            transitionSignals: [signalA]);
        var branchB = CreateBranch(
            "branch-b",
            "path:02",
            transitionSignals: [signalB]);
        var rejected = CreateBranch(
            "branch-rejected",
            "path:99",
            errorCodes: ["REJECTED_ERROR"],
            sourceHashes: ["source-hash-rejected"],
            provenanceRefs: ["provenance-rejected"],
            traceRefs: ["trace-rejected"]);

        var result = new DeterministicGraphFanInMerger().Merge(CreateInput(
            [branchB, branchA],
            rejectedAlternatives: [rejected],
            sourceRefs: ["source-envelope"],
            provenanceRefs: ["provenance-envelope"],
            traceRefs: ["trace-envelope"],
            failureReasonCodes: ["ENVELOPE_ERROR"]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var value = result.Value!;
        var reversed = new DeterministicGraphFanInMerger().Merge(CreateInput(
            [branchA, branchB],
            rejectedAlternatives: [rejected],
            sourceRefs: ["source-envelope"],
            provenanceRefs: ["provenance-envelope"],
            traceRefs: ["trace-envelope"],
            failureReasonCodes: ["ENVELOPE_ERROR"]));
        Assert.True(reversed.IsSuccess, reversed.ErrorMessage);
        Assert.Equal(StructuralJson(value), StructuralJson(reversed.Value));
        Assert.Equal(
            ["BRANCH_A_ERROR", "CONFLICT_UNRESOLVED", "ENVELOPE_ERROR", "REJECTED_ERROR"],
            value.FailureReasonCodes);
        Assert.Contains("source-envelope", value.ProvenanceRefs);
        Assert.Contains("source-hash-branch-a", value.ProvenanceRefs);
        Assert.Contains("provenance-branch-b", value.ProvenanceRefs);
        Assert.Contains("source-hash-rejected", value.ProvenanceRefs);
        Assert.Contains("provenance-rejected", value.ProvenanceRefs);
        Assert.Contains("trace-envelope", value.TraceRefs);
        Assert.Contains("trace-branch-a", value.TraceRefs);
        Assert.Contains("trace-rejected", value.TraceRefs);
        Assert.Equal(2, value.Merged.TransitionSignals.Length);
        Assert.Contains(value.Merged.TransitionSignals, signal => signal.EvidenceIds.Contains("signal-evidence-a"));
        Assert.Contains(value.Merged.TransitionSignals, signal => signal.EvidenceIds.Contains("signal-evidence-b"));
        Assert.Contains(
            "provenance-branch-a",
            value.Context.Fragments.Single(fragment => fragment.FlowId == "flow-branch-a").EvidenceIds);
        Assert.Contains(
            "trace-branch-b",
            value.Context.Fragments.Single(fragment => fragment.FlowId == "flow-branch-b").EvidenceIds);
        var rejectedSnapshot = Assert.Single(value.RejectedAlternatives);
        Assert.Equal("branch-rejected", rejectedSnapshot.BranchId);
        Assert.Equal(["REJECTED_ERROR"], rejectedSnapshot.ErrorCodes);
    }

    [Fact]
    public void Merge_UsesWorstVerdictAllBranchSuccessAndMinimumConfidence_NotBestOrFirstBranch()
    {
        var bestFirst = CreateBranch("branch-a", "path:01", "PASS", 0.98, true, "best");
        var worstLast = CreateBranch("branch-z", "path:99", "REJECT", 0.17, false, "worst");

        var result = new DeterministicGraphFanInMerger().Merge(CreateInput([bestFirst, worstLast]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("REJECT", result.Value!.Merged.OverallVerdict);
        Assert.False(result.Value.Merged.Summary.Passed);
        Assert.False(result.Value.Merged.Summary.ShouldScaleUp);
        Assert.Equal(0.17, result.Value.Merged.Summary.ScalingSignal, precision: 12);
        Assert.Contains("best", result.Value.Merged.FinalHumanResult, StringComparison.Ordinal);
        Assert.Contains("worst", result.Value.Merged.FinalHumanResult, StringComparison.Ordinal);

        var allPassed = new DeterministicGraphFanInMerger().Merge(CreateInput(
            [
                CreateBranch(
                    "branch-a",
                    "path:01",
                    "PASS",
                    0.82,
                    true,
                    metrics: new Dictionary<string, string>(StringComparer.Ordinal) { ["decision"] = "shared" }),
                CreateBranch(
                    "branch-b",
                    "path:02",
                    "PASS",
                    0.61,
                    true,
                    metrics: new Dictionary<string, string>(StringComparer.Ordinal) { ["decision"] = "shared" })
            ]));
        Assert.True(allPassed.IsSuccess, allPassed.ErrorMessage);
        Assert.True(allPassed.Value!.Merged.Summary.Passed);
        Assert.Equal("PASS", allPassed.Value.Merged.OverallVerdict);
        Assert.Equal(0.61, allPassed.Value.Merged.Summary.ScalingSignal, precision: 12);
    }

    [Fact]
    public void Merge_DuplicateCanonicalKeyEmptyMalformedAndNullInputs_ReturnDeterministicFailures()
    {
        var merger = new DeterministicGraphFanInMerger();
        AssertFailure(merger.Merge(null!), "FAN_IN_INPUT_NULL");
        AssertFailure(merger.Merge(CreateInput([])), "NO_BRANCHES_TO_MERGE");

        var duplicateA = CreateBranch("branch-a", "path:duplicate");
        var duplicateB = CreateBranch("branch-b", "path:duplicate");
        AssertFailure(
            merger.Merge(CreateInput([duplicateB, duplicateA])),
            "AMBIGUOUS_MERGE_PATH_KEY");
        AssertFailure(
            merger.Merge(CreateInput([duplicateA, duplicateB])),
            "AMBIGUOUS_MERGE_PATH_KEY");

        var malformed = CreateBranch("branch-a", "path:01") with { Inner = null! };
        AssertFailure(merger.Merge(CreateInput([malformed])), "FAN_IN_BRANCH_MALFORMED");
    }

    [Fact]
    public void Merge_ActiveAndRejectedBranchIdentitiesMustBeUniqueAcrossTheirUnion()
    {
        var merger = new DeterministicGraphFanInMerger();
        var active = CreateBranch("branch-a", "path:01");
        var repeatedBranchId = CreateBranch("branch-a", "path:99");
        var repeatedCanonicalPath = CreateBranch("branch-rejected", "path:01");

        AssertFailure(
            merger.Merge(CreateInput([active], rejectedAlternatives: [repeatedBranchId])),
            "AMBIGUOUS_MERGE_PATH_KEY");
        AssertFailure(
            merger.Merge(CreateInput([active], rejectedAlternatives: [repeatedCanonicalPath])),
            "AMBIGUOUS_MERGE_PATH_KEY");
    }

    [Fact]
    public void Merge_OwnerFailureBoundaries_UseCanonicalErrorCodes()
    {
        var merger = new DeterministicGraphFanInMerger();
        var branch = CreateBranch("branch-a", "path:01");

        AssertFailure(
            merger.Merge(CreateInput([branch with { MergeKey = "merge:other" }])),
            "MERGE_KEY_MISMATCH");
        AssertFailure(
            merger.Merge(CreateInput([branch with { PathOrderRef = "" }])),
            "PATH_ORDER_REF_MISSING");
        AssertFailure(
            merger.Merge(CreateInput([branch with { ProvenanceRefs = [] }])),
            "PROVENANCE_REF_MISSING");
        AssertFailure(
            merger.Merge(CreateInput([branch], legalScope: "BLOCKED")),
            "LEGAL_SCOPE_BLOCKED");
        AssertFailure(
            merger.Merge(CreateInput([branch], legalScope: "  blocked  ")),
            "LEGAL_SCOPE_BLOCKED");
        AssertFailure(
            merger.Merge(CreateInput([branch], legalScope: "unknown-owner-scope")),
            "LEGAL_SCOPE_BLOCKED");

        var normalizedAllowed = merger.Merge(CreateInput(
            [branch],
            legalScope: "  legal_scope_allowed  "));
        Assert.True(normalizedAllowed.IsSuccess, normalizedAllowed.ErrorMessage);
    }

    [Fact]
    public void Merge_FailureVerdictsCannotRemainPassedAndUnknownVerdictsFailClosed()
    {
        var merger = new DeterministicGraphFanInMerger();
        foreach (var verdict in new[] { "REJECT", "REPROCESS", "KNOWLEDGE_EDGE" })
        {
            var result = merger.Merge(CreateInput(
                [CreateBranch("branch-a", "path:01", verdict: verdict, passed: true)]));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(verdict, result.Value!.Merged.OverallVerdict);
            Assert.False(result.Value.Merged.Summary.Passed);
            Assert.False(result.Value.Merged.Summary.ShouldScaleUp);
        }

        AssertFailure(
            merger.Merge(CreateInput(
                [CreateBranch("branch-a", "path:01", verdict: "UNRECOGNIZED", passed: true)])),
            "FAN_IN_BRANCH_MALFORMED");
    }

    [Fact]
    public void Merge_MalformedSummaryStringsFailClosedInSingletonAndMultiBranchPaths()
    {
        var merger = new DeterministicGraphFanInMerger();
        var branch = CreateBranch("branch-a", "path:01");
        var companion = CreateBranch("branch-b", "path:02");
        var summary = branch.Inner.Summary;
        var malformedSummaries = new[]
        {
            summary with { SummaryId = null! },
            summary with { SummaryId = "   " },
            summary with { PrimaryMetricName = null! },
            summary with { PrimaryMetricName = "   " },
            summary with { NextAction = null! },
            summary with { ScalingSignalNote = null! },
            summary with { Notes = null! }
        };

        foreach (var malformedSummary in malformedSummaries)
        {
            var malformed = branch with
            {
                Inner = branch.Inner with { Summary = malformedSummary }
            };
            AssertFailure(
                merger.Merge(CreateInput([malformed])),
                "FAN_IN_BRANCH_MALFORMED");
            AssertFailure(
                merger.Merge(CreateInput([malformed, companion])),
                "FAN_IN_BRANCH_MALFORMED");
        }

        var blankAdvisoryText = branch with
        {
            Inner = branch.Inner with
            {
                Summary = summary with
                {
                    NextAction = string.Empty,
                    ScalingSignalNote = string.Empty,
                    Notes = string.Empty
                }
            }
        };
        var accepted = merger.Merge(CreateInput([blankAdvisoryText]));
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
    }

    [Fact]
    public void Merge_AllNonFiniteSummaryNumbersFailClosedInSingletonAndMultiBranchPaths()
    {
        var merger = new DeterministicGraphFanInMerger();
        var branch = CreateBranch("branch-a", "path:01");
        var companion = CreateBranch("branch-b", "path:02");
        var mutators = new (string Field, Func<LogicalFlowSummary, double, LogicalFlowSummary> Apply)[]
        {
            ("BaselinePrimary", (summary, value) => summary with { BaselinePrimary = value }),
            ("EnhancedPrimary", (summary, value) => summary with { EnhancedPrimary = value }),
            ("ScalingSignal", (summary, value) => summary with { ScalingSignal = value }),
            ("BaselineTimeMs", (summary, value) => summary with { BaselineTimeMs = value }),
            ("EnhancedTimeMs", (summary, value) => summary with { EnhancedTimeMs = value })
        };
        var nonFiniteValues = new[]
        {
            double.NaN,
            double.PositiveInfinity,
            double.NegativeInfinity
        };

        foreach (var (_, apply) in mutators)
        {
            foreach (var nonFiniteValue in nonFiniteValues)
            {
                var malformed = branch with
                {
                    Inner = branch.Inner with
                    {
                        Summary = apply(branch.Inner.Summary, nonFiniteValue)
                    }
                };
                AssertFailure(
                    merger.Merge(CreateInput([malformed])),
                    "FAN_IN_BRANCH_MALFORMED");
                AssertFailure(
                    merger.Merge(CreateInput([malformed, companion])),
                    "FAN_IN_BRANCH_MALFORMED");
            }
        }
    }

    [Fact]
    public void Merge_FiniteSummaryTimeOverflowFailsClosedForBothFieldsSignsAndCallerPermutations()
    {
        var merger = new DeterministicGraphFanInMerger();
        var mutators = new Func<LogicalFlowSummary, double, LogicalFlowSummary>[]
        {
            (summary, value) => summary with { BaselineTimeMs = value },
            (summary, value) => summary with { EnhancedTimeMs = value }
        };

        foreach (var apply in mutators)
        {
            foreach (var extreme in new[] { double.MaxValue, double.MinValue })
            {
                var left = CreateBranch("branch-a", "path:01");
                var right = CreateBranch("branch-b", "path:02");
                left = left with
                {
                    Inner = left.Inner with
                    {
                        Summary = apply(left.Inner.Summary, extreme)
                    }
                };
                right = right with
                {
                    Inner = right.Inner with
                    {
                        Summary = apply(right.Inner.Summary, extreme)
                    }
                };

                var callerOrderPermutations = new[]
                {
                    new[] { left, right },
                    new[] { right, left }
                };
                foreach (var permutation in callerOrderPermutations)
                {
                    AssertFailure(
                        merger.Merge(CreateInput(permutation)),
                        "FAN_IN_BRANCH_MALFORMED");
                }
            }
        }

        var cancellingLeft = CreateBranch("branch-a", "path:01");
        cancellingLeft = cancellingLeft with
        {
            Inner = cancellingLeft.Inner with
            {
                Summary = cancellingLeft.Inner.Summary with
                {
                    BaselineTimeMs = double.MaxValue,
                    EnhancedTimeMs = double.MinValue
                }
            }
        };
        var cancellingRight = CreateBranch("branch-b", "path:02");
        cancellingRight = cancellingRight with
        {
            Inner = cancellingRight.Inner with
            {
                Summary = cancellingRight.Inner.Summary with
                {
                    BaselineTimeMs = double.MinValue,
                    EnhancedTimeMs = double.MaxValue
                }
            }
        };

        var forward = merger.Merge(CreateInput([cancellingLeft, cancellingRight]));
        var reversed = merger.Merge(CreateInput([cancellingRight, cancellingLeft]));
        Assert.True(forward.IsSuccess, forward.ErrorMessage);
        Assert.True(reversed.IsSuccess, reversed.ErrorMessage);
        Assert.Equal(0d, forward.Value!.Merged.Summary.BaselineTimeMs);
        Assert.Equal(0d, forward.Value.Merged.Summary.EnhancedTimeMs);
        Assert.Equal(StructuralJson(forward.Value), StructuralJson(reversed.Value));
    }

    [Fact]
    public void Merge_DeepSnapshotsEveryReturnedCollectionFromLaterCallerMutation()
    {
        var mutableMetrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["decision"] = "original"
        };
        var signal = new PhaseTransitionSignal(
            Domain.CODE_Q,
            "Original transition.",
            "node-original",
            "phase-original",
            ["signal-original"]);
        var branch = CreateBranch(
            "branch-a",
            "path:01",
            metrics: mutableMetrics,
            sourceHashes: ["source-original"],
            provenanceRefs: ["provenance-original"],
            traceRefs: ["trace-original"],
            errorCodes: ["error-original"],
            transitionSignals: [signal]);
        var otherBranch = CreateBranch(
            "branch-b",
            "path:02",
            metrics: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["decision"] = "other"
            });
        var rejected = CreateBranch("branch-rejected", "path:99");
        var conflictValues = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["branch-b"] = "P:5:other",
            ["branch-a"] = "P:8:original"
        };
        var sourceRefs = new[] { "source-envelope-original" };
        var provenanceRefs = new[] { "provenance-envelope-original" };
        var traceRefs = new[] { "trace-envelope-original" };
        var failureCodes = new[] { "failure-original" };
        var conflicts = new[] { new GraphFanInConflict("Metrics.decision", conflictValues) };
        var rejectedAlternatives = new[] { rejected };
        var input = CreateInput(
            [branch, otherBranch],
            rejectedAlternatives,
            conflicts,
            sourceRefs,
            provenanceRefs,
            traceRefs,
            failureCodes);

        var result = new DeterministicGraphFanInMerger().Merge(input);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var snapshot = StructuralJson(result.Value);

        input.Branches[0] = CreateBranch("branch-mutated", "path:mutated");
        sourceRefs[0] = "source-envelope-mutated";
        provenanceRefs[0] = "provenance-envelope-mutated";
        traceRefs[0] = "trace-envelope-mutated";
        failureCodes[0] = "failure-mutated";
        rejectedAlternatives[0] = CreateBranch("rejected-mutated", "path:98");
        conflicts[0] = new GraphFanInConflict(
            "mutated",
            new Dictionary<string, string> { ["branch-a"] = "mutated" });
        conflictValues["branch-a"] = "P:7:mutated";
        branch.SourceHashes[0] = "source-mutated";
        branch.ProvenanceRefs[0] = "provenance-mutated";
        branch.TraceRefs[0] = "trace-mutated";
        branch.ErrorCodes[0] = "error-mutated";
        mutableMetrics["decision"] = "mutated";
        branch.Inner.EvidenceIds[0] = "evidence-mutated";
        branch.Inner.Warnings[0] = "warning-mutated";
        signal.EvidenceIds[0] = "signal-mutated";

        var exposedMerged = result.Value!.Merged;
        exposedMerged.EvidenceIds[0] = "output-evidence-mutated";
        exposedMerged.Warnings[0] = "output-warning-mutated";
        exposedMerged.TransitionSignals[0].EvidenceIds[0] = "output-signal-mutated";
        var exposedContext = result.Value.Context;
        exposedContext.SourceFlowIds[0] = "output-flow-mutated";
        exposedContext.SourceScenarioIds[0] = "output-scenario-mutated";
        exposedContext.Fragments[0].EvidenceIds[0] = "output-context-evidence-mutated";
        exposedContext.Fragments[0].Warnings[0] = "output-context-warning-mutated";
        var exposedRejected = result.Value.RejectedAlternatives[0];
        exposedRejected.SourceHashes[0] = "output-rejected-source-mutated";
        exposedRejected.Inner.EvidenceIds[0] = "output-rejected-evidence-mutated";

        Assert.Equal(snapshot, StructuralJson(result.Value));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)result.Value!.ProvenanceRefs)[0] = "output-mutation");
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, string>)result.Value!.Conflicts[0].ValuesByBranchId)["branch-a"] = "output-mutation");
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, string>)result.Value!.Merged.Metrics)["decision"] = "output-mutation");
    }

    [Fact]
    public void Merge_EquivalentNestedSetPermutationsAndDuplicates_ProduceIdenticalSnapshots()
    {
        var signalAWithDuplicates = new PhaseTransitionSignal(
            Domain.CODE_Q,
            "Review deterministically.",
            "node-a",
            "phase-a",
            ["signal-z", "signal-a", "signal-z"]);
        var signalACanonical = signalAWithDuplicates with
        {
            EvidenceIds = ["signal-a", "signal-z"]
        };
        var signalB = new PhaseTransitionSignal(
            Domain.PLAN,
            "Repair deterministically.",
            "node-b",
            "phase-b",
            ["signal-b"]);

        var metricsAForward = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["shared"] = "same",
            ["decision"] = "left"
        };
        var metricsAReverse = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["decision"] = "left",
            ["shared"] = "same"
        };
        var metricsBForward = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["decision"] = "right",
            ["shared"] = "same"
        };
        var metricsBReverse = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["shared"] = "same",
            ["decision"] = "right"
        };

        var branchAFirst = CreateBranch(
            "branch-a",
            "path:01",
            metrics: metricsAForward,
            errorCodes: ["error-z", "error-a", "error-z"],
            transitionSignals: [signalB, signalAWithDuplicates, signalAWithDuplicates],
            sourceHashes: ["source-z", "source-a", "source-z"],
            provenanceRefs: ["provenance-z", "provenance-a", "provenance-z"],
            traceRefs: ["trace-z", "trace-a", "trace-z"],
            evidenceIds: ["evidence-z", "evidence-a", "evidence-z"],
            warnings: ["warning-z", "warning-a", "warning-z"]);
        var branchASecond = CreateBranch(
            "branch-a",
            "path:01",
            metrics: metricsAReverse,
            errorCodes: ["error-a", "error-z"],
            transitionSignals: [signalACanonical, signalB],
            sourceHashes: ["source-a", "source-z"],
            provenanceRefs: ["provenance-a", "provenance-z"],
            traceRefs: ["trace-a", "trace-z"],
            evidenceIds: ["evidence-a", "evidence-z"],
            warnings: ["warning-a", "warning-z"]);
        var branchBFirst = CreateBranch(
            "branch-b",
            "path:02",
            metrics: metricsBForward,
            sourceHashes: ["source-b", "source-b"],
            provenanceRefs: ["provenance-b", "provenance-b"],
            traceRefs: ["trace-b", "trace-b"],
            evidenceIds: ["evidence-b", "evidence-b"],
            warnings: ["warning-b", "warning-b"]);
        var branchBSecond = CreateBranch(
            "branch-b",
            "path:02",
            metrics: metricsBReverse,
            sourceHashes: ["source-b"],
            provenanceRefs: ["provenance-b"],
            traceRefs: ["trace-b"],
            evidenceIds: ["evidence-b"],
            warnings: ["warning-b"]);
        var rejectedFirst = CreateBranch(
            "branch-rejected",
            "path:99",
            sourceHashes: ["rejected-z", "rejected-a", "rejected-z"],
            provenanceRefs: ["rejected-provenance-z", "rejected-provenance-a"],
            traceRefs: ["rejected-trace-z", "rejected-trace-a"],
            evidenceIds: ["rejected-evidence-z", "rejected-evidence-a", "rejected-evidence-z"],
            warnings: ["rejected-warning-z", "rejected-warning-a"]);
        var rejectedSecond = CreateBranch(
            "branch-rejected",
            "path:99",
            sourceHashes: ["rejected-a", "rejected-z"],
            provenanceRefs: ["rejected-provenance-a", "rejected-provenance-z"],
            traceRefs: ["rejected-trace-a", "rejected-trace-z"],
            evidenceIds: ["rejected-evidence-a", "rejected-evidence-z"],
            warnings: ["rejected-warning-a", "rejected-warning-z"]);

        var first = new DeterministicGraphFanInMerger().Merge(CreateInput(
            [branchBFirst, branchAFirst],
            rejectedAlternatives: [rejectedFirst],
            sourceRefs: ["source-envelope-z", "source-envelope-a", "source-envelope-z"],
            provenanceRefs: ["provenance-envelope-z", "provenance-envelope-a", "provenance-envelope-z"],
            traceRefs: ["trace-envelope-z", "trace-envelope-a", "trace-envelope-z"],
            failureReasonCodes: ["failure-z", "failure-a", "failure-z"]));
        var second = new DeterministicGraphFanInMerger().Merge(CreateInput(
            [branchASecond, branchBSecond],
            rejectedAlternatives: [rejectedSecond],
            sourceRefs: ["source-envelope-a", "source-envelope-z"],
            provenanceRefs: ["provenance-envelope-a", "provenance-envelope-z"],
            traceRefs: ["trace-envelope-a", "trace-envelope-z"],
            failureReasonCodes: ["failure-a", "failure-z"]));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(StructuralJson(first.Value), StructuralJson(second.Value));
    }

    [Fact]
    public void Merge_AlwaysEmitsBaselineRoleAndFalseOrderAuthority_AndExposesNoTrainingMethods()
    {
        var branch = CreateBranch("branch-a", "path:01");
        var result = new DeterministicGraphFanInMerger().Merge(CreateInput([branch]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(FanInMergeRole.DeterministicFanInBaseline, result.Value!.MergeRole);
        Assert.False(result.Value.MergeOrderIsAuthority);
        Assert.Equal(1, result.Value.MergedBranchCount);
        Assert.Equal(StructuralJson(branch.Inner), StructuralJson(result.Value.Merged));

        var publicSurfaceTypes = new[]
        {
            typeof(FanInMergeRole),
            typeof(GraphAsyncBranchResult),
            typeof(GraphFanInConflict),
            typeof(GraphFanInMergeInput),
            typeof(GraphFanInMergeResult),
            typeof(IEdgeFanInMerger),
            typeof(DeterministicGraphFanInMerger)
        };
        var forbiddenMethods = publicSurfaceTypes
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            .Where(method => !method.IsSpecialName)
            .Where(method => IsTrainingOrTraversalMethod(method.Name))
            .ToArray();
        Assert.Empty(forbiddenMethods);
    }

    private static GraphFanInMergeInput CreateInput(
        GraphAsyncBranchResult[] branches,
        GraphAsyncBranchResult[]? rejectedAlternatives = null,
        GraphFanInConflict[]? conflicts = null,
        string[]? sourceRefs = null,
        string[]? provenanceRefs = null,
        string[]? traceRefs = null,
        string[]? failureReasonCodes = null,
        string legalScope = "LEGAL_SCOPE_ALLOWED",
        bool useContextCarryForward = true)
        => new(
            RunId: "run-fan-in-001",
            Branches: branches,
            MergeKey: MergeKey,
            PathOrderRef: GroupPathOrderRef,
            LegalScope: legalScope,
            SourceRefs: sourceRefs ?? ["source-envelope"],
            ProvenanceRefs: provenanceRefs ?? ["provenance-envelope"],
            TraceRefs: traceRefs ?? ["trace-envelope"],
            RejectedAlternatives: rejectedAlternatives ?? [],
            Conflicts: conflicts ?? CreateDerivedConflicts(branches),
            FailureReasonCodes: failureReasonCodes ?? [],
            UseContextCarryForward: useContextCarryForward);

    private static GraphFanInConflict[] CreateDerivedConflicts(
        IReadOnlyList<GraphAsyncBranchResult> branches)
    {
        if (branches.Any(branch => branch?.Inner?.Metrics is null)
            || branches.Select(branch => branch.BranchId).Distinct(StringComparer.Ordinal).Count() != branches.Count)
        {
            return [];
        }

        var ordered = branches
            .OrderBy(branch => branch.MergeKey, StringComparer.Ordinal)
            .ThenBy(branch => branch.PathOrderRef, StringComparer.Ordinal)
            .ToArray();
        var conflicts = new List<GraphFanInConflict>();
        foreach (var metricKey in ordered
                     .SelectMany(branch => branch.Inner.Metrics.Keys)
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(key => key, StringComparer.Ordinal))
        {
            var states = ordered
                .Select(branch => branch.Inner.Metrics.TryGetValue(metricKey, out var value)
                    ? (IsPresent: true, Value: value)
                    : (IsPresent: false, Value: string.Empty))
                .ToArray();
            if (states.Distinct().Count() == 1)
                continue;

            var valuesByBranch = new SortedDictionary<string, string>(StringComparer.Ordinal);
            for (var index = 0; index < ordered.Length; index++)
            {
                var state = states[index];
                valuesByBranch.Add(
                    ordered[index].BranchId,
                    state.IsPresent ? $"P:{state.Value.Length}:{state.Value}" : "M:0:");
            }

            conflicts.Add(new GraphFanInConflict($"Metrics.{metricKey}", valuesByBranch));
        }

        return conflicts.ToArray();
    }

    private static GraphAsyncBranchResult CreateBranch(
        string branchId,
        string pathOrderRef,
        string verdict = "PASS",
        double confidence = 0.80,
        bool passed = true,
        string? humanResult = null,
        IReadOnlyDictionary<string, string>? metrics = null,
        string[]? errorCodes = null,
        PhaseTransitionSignal[]? transitionSignals = null,
        string[]? sourceHashes = null,
        string[]? provenanceRefs = null,
        string[]? traceRefs = null,
        string[]? evidenceIds = null,
        string[]? warnings = null)
    {
        var summary = new LogicalFlowSummary(
            SummaryId: $"summary-{branchId}",
            Tier: 1,
            BaselinePrimary: confidence - 0.05,
            EnhancedPrimary: confidence,
            PrimaryMetricName: "quality",
            LowerIsBetter: false,
            Passed: passed,
            ShouldScaleUp: passed,
            NextAction: passed ? "continue" : "repair",
            ScalingSignal: confidence,
            ScalingSignalNote: $"signal-{branchId}",
            Notes: $"note-{branchId}",
            BaselineTimeMs: 10,
            EnhancedTimeMs: 12);
        var inner = new LogicalFlowResult(
            FlowId: $"flow-{branchId}",
            ScenarioId: $"scenario-{branchId}",
            OverallVerdict: verdict,
            FinalHumanResult: humanResult ?? $"result-{branchId}",
            Summary: summary,
            Metrics: metrics ?? new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["decision"] = $"decision-{branchId}",
                ["sub_phase_id"] = branchId
            },
            EvidenceIds: evidenceIds ?? [$"evidence-{branchId}"],
            Warnings: warnings ?? [$"warning-{branchId}"])
        {
            TransitionSignals = transitionSignals ?? []
        };

        return new GraphAsyncBranchResult(
            CombinationId: $"combination-{branchId}",
            BranchId: branchId,
            MergeKey: MergeKey,
            PathOrderRef: pathOrderRef,
            OverallVerdict: verdict,
            Confidence: confidence,
            SourceHashes: sourceHashes ?? [$"source-hash-{branchId}"],
            ProvenanceRefs: provenanceRefs ?? [$"provenance-{branchId}"],
            TraceRefs: traceRefs ?? [$"trace-{branchId}"],
            ErrorCodes: errorCodes ?? [],
            Inner: inner);
    }

    private static void AssertCanonicalRecordShape<T>((string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        var parameters = constructor.GetParameters()
            .Select(parameter => (parameter.Name!, parameter.ParameterType))
            .ToArray();
        Assert.Equal(expected, parameters);

        var properties = typeof(T).GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType))
            .ToArray();
        Assert.Equal(expected.OrderBy(item => item.Name, StringComparer.Ordinal), properties);
    }

    private static void AssertFailure(OperationResult<GraphFanInMergeResult> result, string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
    }

    private static string StructuralJson<T>(T value)
        => JsonSerializer.Serialize(value);

    private static bool IsTrainingOrTraversalMethod(string methodName)
        => methodName.Contains("Train", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Evaluate", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Export", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Import", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Provider", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Traverse", StringComparison.OrdinalIgnoreCase)
            || methodName.Contains("Authority", StringComparison.OrdinalIgnoreCase);
}
