// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using AgentInfrastructure;
using ContinuousLearning;
using Inference;

namespace TranslationDomain;

public sealed record ResearchEnRuGenerationTrace(string ContractVersion, string RawInputSha256,
    string FramedInputSha256, string TokenizerSha256, string SettingsSha256,
    SimulationDpoStudentSource Source, TextGenerationReceipt Generation, string QualityVerdict);

/// <summary>Explicit research composition for the normal typed translation flow; no production auto-adoption.</summary>
public sealed class ResearchCheckpointEnRuAdapter : ITranslationDirectionAdapter
{
    private readonly SimulationDpoReadOnlyStudent _student;
    private readonly CodeDocumentationStudentGenerationSettings _settings;
    public ResearchCheckpointEnRuAdapter(SimulationDpoReadOnlyStudent student, CodeDocumentationStudentGenerationSettings settings)
    { _student = student ?? throw new ArgumentNullException(nameof(student)); _settings = settings ?? throw new ArgumentNullException(nameof(settings)); }
    public TranslationDirection Direction => TranslationDirection.EN_RU;
    public string AdapterId => "research-checkpoint-en-ru-" + _student.Source.WeightsSha256;
    public ResearchEnRuGenerationTrace? LastGeneration { get; private set; }

    public Task<OperationResult<TranslationRoutingResult>> TranslateAsync(TranslationRoutingRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastGeneration = null;
        var prepared = PreparePrompt(request);
        if (!prepared.IsSuccess) return Task.FromResult(prepared.MapError<TranslationRoutingResult>());
        var prompt = prepared.Value!;
        var generated = _student.Generate(prompt, _settings, cancellationToken);
        LastGeneration = new(SimulationEnRuPrompt.Version, CodeDocumentationStudentGeneration.InputSha256(request.InputText),
            CodeDocumentationStudentGeneration.InputSha256(prompt), _student.TokenizerSha256,
            CodeDocumentationStudentGeneration.SettingsSha256(_settings), _student.Source, generated, "UNKNOWN_NOT_GRADED");
        // Mechanical generation establishes no semantic confidence. Preserve even invalid raw output for blind grading.
        return Task.FromResult(OperationResult<TranslationRoutingResult>.Ok(new(Direction, generated.Text, 0, AdapterId)));
    }

    public static OperationResult<string> PreparePrompt(TranslationRoutingRequest request)
    {
        if (request is null || !string.Equals(request.SourceLanguage?.Trim(), "en", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(request.TargetLanguage?.Trim(), "ru", StringComparison.OrdinalIgnoreCase))
            return OperationResult<string>.Fail("This research adapter requires raw English and the en→ru direction.", "RESEARCH_ENRU_DIRECTION_INVALID");
        try { return OperationResult<string>.Ok(SimulationEnRuPrompt.Build(request.InputText)); }
        catch (ArgumentException) { return OperationResult<string>.Fail("Raw English must be nonempty, valid Unicode and have no translation prefix.", "RESEARCH_ENRU_INPUT_INVALID"); }
    }
}
