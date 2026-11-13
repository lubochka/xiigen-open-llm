// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

// Tests for TD3: ILearningMachine<TDomain> contracts in 00.Core/LearningMachineContracts.cs.
// Governed by: TECHNICAL-DEBT-CLOSURE-PHASE-0.md Gate B TD3.
// These tests verify: interface/record presence, separation rule A-LM-02,
// stub implementation correctness, and OperationResult<T> return-type coverage.

using AgentInfrastructure;         // OperationResult<T>
using AgentInfrastructure.Logical; // ILearningMachine, ILearningMachinePair, record types

public sealed class LearningMachineContractsTests
{
    // -----------------------------------------------------------------------
    // Test 1 — All 7 required type names exist in the Core assembly.
    // Gate B TD3 requires ILearningMachine, ILearningMachinePair, LearningBatch,
    // LearningBatchResult, LearningEvaluationSet, LearningEvaluationResult,
    // LearningMachineExport — 7 names minimum.
    // -----------------------------------------------------------------------
    [Fact]
    public void AllSevenContractTypesExistInCoreAssembly()
    {
        // Obtain the assembly that contains the ILearningMachine type.
        var coreAssembly = typeof(ILearningMachine<>).Assembly;

        // Collect all type names from the Core assembly for assertion.
        var typeNames = coreAssembly.GetTypes()
            .Select(t => t.Name)
            .ToHashSet();

        // Each assertion verifies one required contract name from TD3 spec.
        Assert.Contains("ILearningMachine`1",          typeNames); // ILearningMachine<TDomain>
        Assert.Contains("ILearningMachinePair`2",       typeNames); // ILearningMachinePair<TPersonal,TGlobal>
        Assert.Contains("LearningBatch`1",              typeNames); // LearningBatch<TDomain>
        Assert.Contains("LearningBatchResult",          typeNames); // plain record
        Assert.Contains("LearningEvaluationSet`1",      typeNames); // LearningEvaluationSet<TDomain>
        Assert.Contains("LearningEvaluationResult",     typeNames); // plain record
        Assert.Contains("LearningMachineExport",        typeNames); // plain record
    }

    // -----------------------------------------------------------------------
    // Test 2 — Separation rule A-LM-02: Train() must NOT appear on ILogicalUnit.
    // ILogicalUnit is execution-only. Training is initiated only on ILearningMachine.
    // -----------------------------------------------------------------------
    [Fact]
    public void ILogicalUnitHasNoMethodWithTrainInItsName()
    {
        // Obtain ILogicalUnit<,> from the same Core assembly.
        var ilogicalUnitType = typeof(ILogicalUnit<,>);

        // Check every method name — none should contain "Train" (case-insensitive).
        var trainMethods = ilogicalUnitType.GetMethods()
            .Where(m => m.Name.Contains("Train", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Expect zero matches — Train() belongs to ILearningMachine, not ILogicalUnit.
        Assert.Empty(trainMethods);
    }

    // -----------------------------------------------------------------------
    // Test 3 — A stub implementation satisfies ILearningMachine<TDomain>.
    // Verifies the interface shape is complete and implementable.
    // Also verifies that every return type is wrapped in OperationResult<T>.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task StubMachineImplementsILearningMachineAndReturnsOperationResults()
    {
        // Instantiate the local stub (defined below) and assign to the interface.
        ILearningMachine<StubDomain> machine = new StubLearningMachine();

        // Create minimal input fixtures to exercise each interface method.
        var batch = new LearningBatch<StubDomain>(
            BatchId: "batch-001",
            DomainTag: "stub",
            TrainingItems: [
                new LearningTrainingItem("item-001", "prompt", "chosen", "rejected")
            ],
            SourceIds: ["src-001"]);

        var evalSet = new LearningEvaluationSet<StubDomain>(
            EvaluationId: "eval-001",
            DomainTag: "stub",
            EvaluationItems: [
                new LearningEvaluationItem("ei-001", "eval-prompt", "expected")
            ]);

        var export = new LearningMachineExport(
            MachineId: "machine-001",
            DomainTag: "stub",
            Version: "1.0.0",
            CheckpointRef: "ckpt-ref",
            ManifestRef: "manifest-ref",
            AccuracyAtExport: 0.85,
            ExportedAtUtc: DateTimeOffset.UtcNow);

        // Each call must return IsSuccess = true from the stub implementation.
        var trainResult = await machine.TrainBatchAsync(batch);
        Assert.True(trainResult.IsSuccess);
        Assert.True(trainResult.Value!.AccuracyAfter > 0); // Gate B TD3: AccuracyAfter must be > 0

        var evalResult = await machine.EvaluateAsync(evalSet);
        Assert.True(evalResult.IsSuccess);

        var exportResult = await machine.ExportAsync();
        Assert.True(exportResult.IsSuccess);

        var importResult = await machine.ImportAsync(export);
        Assert.True(importResult.IsSuccess);

        var orchestratorResult = await machine.GetOrchestratorAsync();
        Assert.True(orchestratorResult.IsSuccess);
    }

    // -----------------------------------------------------------------------
    // Test 4 — ILearningMachinePair satisfies the two-machine sync contract.
    // Verifies Personal and Global properties, plus Sync and Pull methods.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task StubMachinePairImplementsILearningMachinePair()
    {
        // Instantiate the stub pair and assign to the interface.
        ILearningMachinePair<StubPersonalDomain, StubGlobalDomain> pair = new StubMachinePair();

        // Personal and Global properties must be non-null and implement their interfaces.
        Assert.NotNull(pair.Personal);
        Assert.NotNull(pair.Global);
        Assert.IsAssignableFrom<ILearningMachine<StubPersonalDomain>>(pair.Personal);
        Assert.IsAssignableFrom<ILearningMachine<StubGlobalDomain>>(pair.Global);

        // SyncPersonalToGlobalAsync must return success — stub sends no real data.
        var syncResult = await pair.SyncPersonalToGlobalAsync();
        Assert.True(syncResult.IsSuccess);

        // PullGlobalToPersonalAsync must return success.
        var pullResult = await pair.PullGlobalToPersonalAsync();
        Assert.True(pullResult.IsSuccess);
    }

    // -----------------------------------------------------------------------
    // Private stub types — used only by these tests, not production artifacts.
    // -----------------------------------------------------------------------

    // StubDomain is the test domain marker for ILearningMachine<TDomain> tests.
    private sealed class StubDomain { }

    // StubPersonalDomain is the test personal domain marker for ILearningMachinePair tests.
    private sealed class StubPersonalDomain { }

    // StubGlobalDomain is the test global domain marker for ILearningMachinePair tests.
    private sealed class StubGlobalDomain { }

    // StubLearningMachine is the minimal implementation that satisfies ILearningMachine<StubDomain>.
    // All methods return OperationResult.Ok(...) with test fixture values.
    private sealed class StubLearningMachine : ILearningMachine<StubDomain>
    {
        // TrainBatchAsync returns a result showing one row trained with AccuracyAfter = 0.9.
        public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
            LearningBatch<StubDomain> batch, CancellationToken ct = default)
            => Task.FromResult(OperationResult<LearningBatchResult>.Ok(
                new LearningBatchResult(
                    BatchId: batch.BatchId,
                    RowsTrained: batch.TrainingItems.Count,
                    AccuracyBefore: 0.7,
                    AccuracyAfter: 0.9,  // > 0 as required by Gate B TD3
                    RewardMargin: 0.2,
                    CpuMs: 100,
                    PeakRamMb: 32,
                    ElapsedMs: 150,
                    PromotedUnitIds: [])));

        // EvaluateAsync returns a result with Accuracy = 0.85 and PassesGate = true.
        public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
            LearningEvaluationSet<StubDomain> evalSet, CancellationToken ct = default)
            => Task.FromResult(OperationResult<LearningEvaluationResult>.Ok(
                new LearningEvaluationResult(
                    EvaluationId: evalSet.EvaluationId,
                    RowsEvaluated: evalSet.EvaluationItems.Count,
                    Accuracy: 0.85,
                    RewardMargin: 0.15,
                    PassesGate: true,
                    GateNote: "stub: threshold=0.75, actual=0.85 — gate passed")));

        // ExportAsync returns a stub export record with a fixed checkpoint reference.
        public Task<OperationResult<LearningMachineExport>> ExportAsync(
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<LearningMachineExport>.Ok(
                new LearningMachineExport(
                    MachineId: "stub-machine-001",
                    DomainTag: "stub",
                    Version: "1.0.0",
                    CheckpointRef: "ckpt-stub-001",
                    ManifestRef: "manifest-stub-001",
                    AccuracyAtExport: 0.85,
                    ExportedAtUtc: DateTimeOffset.UtcNow)));

        // ImportAsync accepts any export record and returns true (stub does not merge weights).
        public Task<OperationResult<bool>> ImportAsync(
            LearningMachineExport export, CancellationToken ct = default)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        // GetOrchestratorAsync returns a stub orchestrator that satisfies the interface contract.
        public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(
                new StubOrchestrator()));
    }

    // StubMachinePair is the minimal implementation that satisfies ILearningMachinePair.
    private sealed class StubMachinePair
        : ILearningMachinePair<StubPersonalDomain, StubGlobalDomain>
    {
        // Personal returns a stub personal machine for test assertions.
        public ILearningMachine<StubPersonalDomain> Personal { get; }
            = new StubTypedMachine<StubPersonalDomain>();

        // Global returns a stub global machine for test assertions.
        public ILearningMachine<StubGlobalDomain> Global { get; }
            = new StubTypedMachine<StubGlobalDomain>();

        // SyncPersonalToGlobalAsync returns success — stub sends no data.
        public Task<OperationResult<bool>> SyncPersonalToGlobalAsync(
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        // PullGlobalToPersonalAsync returns success — stub applies no weights.
        public Task<OperationResult<bool>> PullGlobalToPersonalAsync(
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<bool>.Ok(true));
    }

    // StubTypedMachine<TDomain> is a generic stub machine used by StubMachinePair.
    private sealed class StubTypedMachine<TDomain> : ILearningMachine<TDomain>
    {
        // TrainBatchAsync returns a minimal successful result.
        public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
            LearningBatch<TDomain> batch, CancellationToken ct = default)
            => Task.FromResult(OperationResult<LearningBatchResult>.Ok(
                new LearningBatchResult("b", 0, 0, 0.1, 0, 0, 0, 0, [])));

        // EvaluateAsync returns a minimal successful result.
        public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
            LearningEvaluationSet<TDomain> evalSet, CancellationToken ct = default)
            => Task.FromResult(OperationResult<LearningEvaluationResult>.Ok(
                new LearningEvaluationResult("e", 0, 0, 0, false, "stub")));

        // ExportAsync returns a minimal export record.
        public Task<OperationResult<LearningMachineExport>> ExportAsync(
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<LearningMachineExport>.Ok(
                new LearningMachineExport("m", "d", "1.0.0", "c", "r", 0, DateTimeOffset.UtcNow)));

        // ImportAsync returns true.
        public Task<OperationResult<bool>> ImportAsync(
            LearningMachineExport export, CancellationToken ct = default)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        // GetOrchestratorAsync returns a stub orchestrator.
        public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(
                new StubOrchestrator()));
    }

    // StubOrchestrator satisfies ILogicalOrchestrator — returns an empty successful result for any request.
    private sealed class StubOrchestrator : ILogicalOrchestrator
    {
        // RunAsync returns a minimal successful LogicalOrchestratorResult for any request.
        public Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
            LogicalOrchestratorRequest request,
            CancellationToken cancellationToken = default)
        {
            // Build a minimal LogicalFlowSummary to satisfy the required constructor parameter.
            var summary = new LogicalFlowSummary(
                SummaryId: "stub-summary",
                Tier: 1,
                BaselinePrimary: 0,
                EnhancedPrimary: 0,
                PrimaryMetricName: "stub",
                LowerIsBetter: false,
                Passed: true,
                ShouldScaleUp: false,
                NextAction: "none",
                ScalingSignal: 0,
                ScalingSignalNote: "",
                Notes: "",
                BaselineTimeMs: 0,
                EnhancedTimeMs: 0);

            // Build a minimal LogicalFlowResult carrying the summary.
            var flowResult = new LogicalFlowResult(
                FlowId: "stub-flow",
                ScenarioId: request.ScenarioId,
                OverallVerdict: "PASS",
                FinalHumanResult: "stub",
                Summary: summary,
                Metrics: new Dictionary<string, string>(),
                EvidenceIds: [],
                Warnings: []);

            // Build the LogicalOrchestratorResult that wraps the flow result.
            var result = new LogicalOrchestratorResult(
                ScenarioId: request.ScenarioId,
                SelectedFlowId: "stub-flow",
                OverallVerdict: "PASS",
                FinalHumanResult: "stub",
                AppliedFlags: new LogicalExecutionFlags(),
                FlowResult: flowResult);

            // Return the result wrapped in OperationResult.Ok.
            return Task.FromResult(OperationResult<LogicalOrchestratorResult>.Ok(result));
        }
    }
}
