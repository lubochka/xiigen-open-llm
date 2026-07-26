// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel;

using AgentInfrastructure;
// SC29 alias prevents OperationResult<T> ambiguity between AgentInfrastructure and SemanticCompiler29.
using SC29 = SemanticCompiler29;

// Classifies a task via the full SemanticCompiler29 pipeline.
// FC-10: uses ISemanticCompiler29.Compile() — never IRouteClassifier29 directly.
// E18: 95% domain accuracy.
public sealed class EpisodeClassifier : BaseService, IEpisodeClassifier
{
    // SemanticCompiler29 pipeline — injected via fabric interface (Fabric First, Rule 2).
    private readonly SC29.ISemanticCompiler29 _compiler;

    // Maps SemanticCompiler29 Domain to OrchestratorModel TaskDomain.
    // Values are identical (D-04), but the types live in different namespaces.
    private static TaskDomain MapDomain(SC29.Domain d) => d switch
    {
        SC29.Domain.BUG      => TaskDomain.BUG,
        SC29.Domain.PLAN     => TaskDomain.PLAN,
        SC29.Domain.ARCH     => TaskDomain.ARCH,
        SC29.Domain.REFACTOR => TaskDomain.REFACTOR,
        _                    => TaskDomain.CODE_Q, // CODE_Q + UNKNOWN → CODE_Q (safe default)
    };

    // FC-12: ISessionStorage first, then domain-specific dependencies.
    public EpisodeClassifier(SC29.ISemanticCompiler29 compiler, ISessionStorage storage)
        : base(storage)
        => _compiler = compiler;

    // Classify without confidence — convenience wrapper.
    public OperationResult<TaskDomain> Classify(string taskDescription)
    {
        var r = ClassifyWithConfidence(taskDescription);
        return r.IsSuccess
            ? OperationResult<TaskDomain>.Ok(r.Value!.Domain)
            : OperationResult<TaskDomain>.Fail(r.ErrorMessage, r.ErrorCode);
    }

    // Classify with confidence score derived from CompressionRatio.
    // confidence = min(1.0, CompressionRatio / 20.0).
    // confidence < 0.6 (ratio < 12) → signal for 17.DynamicDomains (unfamiliar domain).
    public OperationResult<(TaskDomain Domain, double Confidence)> ClassifyWithConfidence(
        string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<(TaskDomain, double)>.Fail(
                "Task description cannot be empty.", "EMPTY_TASK");

        // Run the full pipeline — E20 StackDetector order preserved inside the pipeline.
        var compileResult = _compiler.Compile(
            new SC29.PipelineInput(taskDescription, null));
        if (!compileResult.IsSuccess)
            return OperationResult<(TaskDomain, double)>.Fail(
                compileResult.ErrorMessage, compileResult.ErrorCode);

        var output    = compileResult.Value!;
        TaskDomain domain     = MapDomain(output.Domain);

        // High CompressionRatio = pipeline compressed confidently = well-known domain.
        double confidence = Math.Min(1.0, output.CompressionRatio / 20.0);

        return OperationResult<(TaskDomain, double)>.Ok((domain, confidence));
    }
}
