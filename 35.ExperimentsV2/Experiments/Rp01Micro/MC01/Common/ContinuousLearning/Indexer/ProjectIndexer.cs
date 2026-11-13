// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;
using SC29 = SemanticCompiler29;

// ProjectIndexer: scans a codebase and produces labelled training examples.
// Uses ISemanticCompiler29 from 29.SemanticCompiler — does NOT reimplement
// StackDetector, SemanticNormalizer, FamilyCollapser, or LogicExtractor (Principle 4).
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class ProjectIndexer : BaseService, IProjectIndexer
{
    // Full pipeline from 29.SemanticCompiler — injected, never constructed here (Principle 3).
    private readonly SC29.ISemanticCompiler29 _compiler;

    // File reader from 21.AgentInfrastructure — lists and reads source files.
    private readonly ICodeReader _reader;

    public ProjectIndexer(
        ISessionStorage        storage,
        SC29.ISemanticCompiler29 compiler,
        ICodeReader            reader)
        : base(storage)
    {
        _compiler = compiler;
        _reader   = reader;
    }

    // Scan all .cs files under rootPath using the injected ICodeReader.
    // For each file: run through ISemanticCompiler29.Compile → produce TrainingExample.
    // Weight is always 1.0 here — FeedbackRouter adjusts weights per domain imbalance.
    public OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            return OperationResult<IReadOnlyList<TrainingExample>>.Fail(
                "Root path cannot be empty.", "EMPTY_PATH");

        // List all .cs source files under rootPath.
        var listResult = _reader.ListFilesAsync(rootPath, "*.cs").GetAwaiter().GetResult();
        if (!listResult.IsSuccess)
            return OperationResult<IReadOnlyList<TrainingExample>>.Fail(
                listResult.ErrorMessage, listResult.ErrorCode);

        var files = listResult.Value!;
        if (files.Count == 0)
            return OperationResult<IReadOnlyList<TrainingExample>>.Ok(
                Array.Empty<TrainingExample>());

        var examples = new List<TrainingExample>(files.Count);
        foreach (var file in files)
        {
            // Read the source file content.
            var readResult = _reader.ReadFileAsync(file).GetAwaiter().GetResult();
            if (!readResult.IsSuccess) continue;

            // Run the full SemanticCompiler29 pipeline — StackDetector fires before normalisation.
            var compileResult = _compiler.Compile(
                new SC29.PipelineInput(readResult.Value!, file));

            string domain = compileResult.IsSuccess
                ? MapDomain(compileResult.Value!.Domain)
                : "CODE_Q";

            examples.Add(new TrainingExample(
                Prompt:     $"// file: {Path.GetFileName(file)}",
                Response:   readResult.Value!,
                Domain:     domain,
                Weight:     1.0,
                CapturedAt: DateTimeOffset.UtcNow,
                SourceId:   TrainingSourceProvenance.CreateSourceId(
                    file,
                    readResult.Value!)));
        }

        return OperationResult<IReadOnlyList<TrainingExample>>.Ok(examples);
    }

    // Map SemanticCompiler29.Domain to a canonical D-04 domain string.
    private static string MapDomain(SC29.Domain d) => d switch
    {
        SC29.Domain.BUG      => "BUG",
        SC29.Domain.PLAN     => "PLAN",
        SC29.Domain.ARCH     => "ARCH",
        SC29.Domain.REFACTOR => "REFACTOR",
        _                    => "CODE_Q",
    };
}
