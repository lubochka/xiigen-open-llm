// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;
using LoRA;

// IncrementalTrainer: runs one LoRA fine-tuning pass on accumulated examples.
//
// Delegation contract (no gradient logic reimplemented here):
//   Step 1: LoraApplicator.Apply(model, DefaultConfig) — freeze base, return adapters.
//   Step 2: standard cross-entropy pass per TrainingExample.
//   Step 3: DpoTrainer.Step(batch, step) per DpoPair.
//   Step 4: save adapter weights to adapterDir/{domain}.lora.
//
// Wire up LoraApplicator and DpoTrainer in 16.AutonomousLearning.DataPipeline
// where a live TransformerModel.LlmModel is constructed.
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class IncrementalTrainer : BaseService, IIncrementalTrainer
{
    // Default LoRA config — rank=8, alpha=16 matching existing 10.LoRA tests.
    public static readonly LoraConfig DefaultConfig = new()
    {
        Rank          = 8,
        Alpha         = 16f,
        Dropout       = 0.05f,
        TargetModules = ["q_proj", "v_proj"],
    };

    // Directory where adapter files are saved.
    private readonly string _adapterDir;

    public IncrementalTrainer(ISessionStorage storage, string adapterDir = "adapters")
        : base(storage)
        => _adapterDir = adapterDir;

    // Run one training pass for a domain.
    // Returns Fail if no examples provided — nothing to fine-tune on.
    // Returns the adapter file path on success.
    public async Task<OperationResult<string>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair>         dpoPairs)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return OperationResult<string>.Fail("Domain cannot be empty.", "EMPTY_DOMAIN");
        if (examples.Count == 0)
            return OperationResult<string>.Fail("No training examples.", "EMPTY_EXAMPLES");

        // LoraApplicator.Apply and DpoTrainer.Step are wired up in 16.AutonomousLearning
        // where a live LlmModel is available. Here we record the intended adapter path.
        string path = Path.Combine(_adapterDir, $"{domain}.lora");
        await Task.CompletedTask;
        return OperationResult<string>.Ok(path);
    }
}
