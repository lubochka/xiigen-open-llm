// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AutonomousLearning;

using AgentInfrastructure;

// SystemStatus — snapshot of machine hardware state consumed by the E41 IdleDetector.
// All four fields must meet their E41 thresholds before autonomous training may begin.
public sealed record SystemStatus(
    double CpuPercent,        // CPU utilisation percentage, 0-100
    double InputIdleMinutes,  // Minutes since the last keyboard or mouse event
    bool   IsCharging,        // true when AC power is connected
    double BatteryPercent,    // Battery level percentage, 0-100
    double DiskFreeGb);       // Free disk space in gigabytes on the training volume

// TrustLevel — E40V2 trust classification bucket assigned to each data source.
// HIGH: score 0.7-1.0, MEDIUM: 0.3-0.7, LOW: 0.0-0.3
public enum TrustLevel { Low, Medium, High }

// SourceEntry — one discovered .NET data source with its tier and trust classification.
// Created by DataSourceFinder; consumed by TrustScorer and DatasetPreprocessor.
public sealed record SourceEntry(
    string     Url,   // Full URL of the source (GitHub, docs.microsoft.com, etc.)
    int        Tier,  // Collection tier: 1=highest trust, 2=medium, 3=lower
    TrustLevel Trust  // E40V2 trust level assigned after K-NN scoring
);

// ISystemMonitor — samples the current machine hardware state for IdleDetector.
// Real implementation reads OS metrics; test stub returns configured values.
public interface ISystemMonitor
{
    // Return a single point-in-time snapshot of the machine state.
    OperationResult<SystemStatus> Sample();
}

// IAuLearningStorage — persists autonomous-learning cycle state across runs.
// Backed by ISessionStorage from 21.AgentInfrastructure — no direct disk access.
public interface IAuLearningStorage
{
    // Persist a JSON-serialised cycle state blob under the given key.
    Task<OperationResult<bool>> SaveCycleStateAsync(string key, string json);
    // Retrieve a previously-saved cycle state blob by key.
    Task<OperationResult<string>> LoadCycleStateAsync(string key);
}

// IDataSourceFinder — discovers configured .NET data sources by tier.
// Returns a list of SourceEntry values ready for TrustScorer classification.
public interface IDataSourceFinder
{
    // Return all configured data sources; fail with EMPTY_CONFIG when none exist.
    OperationResult<IReadOnlyList<SourceEntry>> Find();
}

// ITrainingDataProvider — collects raw labeled training examples from discovered sources.
// Wraps the DataSourceFinder → TrustScorer → DatasetPreprocessor chain into one injectable step.
// Test stubs return a pre-configured list; the real implementation fetches and labels remote content.
public interface ITrainingDataProvider
{
    // Collect and label examples for the given domain from all configured sources.
    // Returns Fail when collection produces zero usable examples.
    Task<OperationResult<IReadOnlyList<ContinuousLearning.TrainingExample>>> CollectAsync(
        string domain, CancellationToken ct);
}

public enum AccuracyEvaluationPhase
{
    Baseline,
    Candidate,
}

public sealed record AccuracyEvaluationArtifactIdentity(
    string BaseCheckpointSha256,
    string TokenizerSha256,
    string ModelConfigurationSha256);

public sealed record AccuracyEvaluationRequest(
    string Domain,
    ContinuousLearning.ModelAdapter? TargetAdapter,
    ContinuousLearning.TrainingSourceManifest PlannedTrainingSources,
    AccuracyEvaluationPhase Phase,
    AccuracyEvaluationArtifactIdentity? ExpectedArtifacts);

public sealed record AccuracyEvaluationResult(
    double Accuracy,
    AccuracyEvaluationArtifactIdentity Artifacts,
    string? AdapterSha256,
    string TrainingSourceManifestSha256);

// IAccuracyEvaluator — target-bound evaluation seam. A concrete evaluator is
// available, but no non-test composition root wires it yet.
// E28-E31 lesson: an implementation must score the requested model/adapter target, never a proxy.
public interface IAccuracyEvaluator
{
    OperationResult<AccuracyEvaluationResult> Evaluate(
        AccuracyEvaluationRequest request,
        CancellationToken cancellationToken = default);
}

// CycleResult — outcome of one complete autonomous learning cycle.
// Returned by AutonomousLearningPipeline.RunAsync on success.
public sealed record CycleResult(
    double BaselineAccuracy,  // Model accuracy measured BEFORE this training cycle
    double EnhancedAccuracy,  // Model accuracy measured AFTER this training cycle
    string AdapterPath,       // Fully-qualified absolute path to the saved LoRA artifact
    int    ExamplesProcessed  // Count of examples used in the training batch
);
