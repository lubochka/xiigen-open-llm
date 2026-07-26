// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;

// IProjectIndexer — scans a codebase and produces labelled training examples.
// Implementation injects ISemanticCompiler29 from 29.SemanticCompiler — never reimplements it.
// grep -rn "ISemanticCompiler" 14.ContinuousLearning/ must return ≥ 1 line (Principle 4).
public interface IProjectIndexer
{
    // Scan all .cs / .ts / .py files under rootPath.
    // Returns one TrainingExample per file (prompt=symbol, response=body).
    OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath);
}

// IDomainDatasetCache — persists training examples by domain.
// Backed by JSON via ICodeWriter — no SQLite dependency.
public interface IDomainDatasetCache
{
    // Add one example to the in-memory store for its domain.
    OperationResult<bool> Add(TrainingExample example);
    // Retrieve all examples for one canonical domain string.
    OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain);
    // Total example count across all domains.
    OperationResult<int> TotalCount();
    // Write one JSON file per domain to the configured cache directory.
    Task<OperationResult<bool>> FlushAsync();
}

// IFeedbackRouter — converts user feedback events into training signals.
// ACCEPTED → positive example, EDITED/REJECTED → DPO pair.
public interface IFeedbackRouter
{
    // Process one event: route it to cache or create a DPO pair.
    OperationResult<bool> Route(FeedbackEvent feedback);
    // Return all DPO pairs accumulated since last IncrementalTrainer run.
    OperationResult<IReadOnlyList<DpoPair>> GetDpoPairs();
}

// IIncrementalTrainer — runs LoRA fine-tuning on accumulated examples.
// Delegates to LoraApplicator.Apply from 10.LoRA — never reimplements gradient logic.
public interface IIncrementalTrainer
{
    // Run one LoRA training pass on examples for the given domain.
    // Returns the relative path to the saved adapter file.
    Task<OperationResult<string>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair>         dpoPairs);
}

// IDomainModelRegistry — loads and saves LoRA adapters per domain.
public interface IDomainModelRegistry
{
    // Register a newly-trained adapter.
    OperationResult<bool> Register(ModelAdapter adapter);
    // Get the adapter for a domain; Value is null when none trained yet (still IsSuccess).
    OperationResult<ModelAdapter?> GetAdapter(string domain);
    // List all registered adapters.
    OperationResult<IReadOnlyList<ModelAdapter>> ListAll();
}

// IContinuousLearningPipeline — fabric interface for 16.AutonomousLearning.
// 16.AutonomousLearning must inject this, not copy the implementation (Principle 3).
public interface IContinuousLearningPipeline
{
    // Run one full learning cycle: index → cache → train → register → flush.
    Task<OperationResult<IReadOnlyList<ModelAdapter>>> RunCycleAsync(string rootPath);
}
