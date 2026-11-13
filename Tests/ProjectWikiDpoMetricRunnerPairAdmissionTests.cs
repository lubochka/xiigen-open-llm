// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;
using ContinuousLearning;
using DPO.Models;
using ProjectWikiDomain;

public sealed class ProjectWikiDpoMetricRunnerPairAdmissionTests
    : IDisposable
{
    private readonly string _artifactRoot = Path.Combine(
        Path.GetTempPath(),
        $"project-wiki-pair-admission-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_artifactRoot))
            Directory.Delete(_artifactRoot, recursive: true);
    }

    [Fact]
    public void BuildTrainingBatchPreviewForTesting_CarriesDeterministicGeneratorAdmissionOverRawPair()
    {
        var pair = NewPair(
            "preview",
            1,
            "raw prompt must be fingerprinted without the tokenization answer prefix",
            "chosen raw text with stable wiki evidence",
            "rejected raw text [WRONG: preview_defect - bad wiki signal]");

        var batch = ProjectWikiDpoMetricRunner
            .BuildTrainingBatchPreviewForTesting(pair, 128, 16);

        Assert.NotNull(batch.Admission);
        var admission = batch.Admission!;
        AssertDeterministicProjectWikiAdmission(pair, admission);

        var repeat = ProjectWikiDpoMetricRunner
            .BuildTrainingBatchPreviewForTesting(pair, 128, 16);
        Assert.Equal(
            admission.ContentFingerprintSha256,
            repeat.Admission!.ContentFingerprintSha256);

        var mutated = ProjectWikiDpoMetricRunner
            .BuildTrainingBatchPreviewForTesting(
                pair with { Chosen = pair.Chosen + " mutation" },
                128,
                16);
        Assert.NotEqual(
            admission.ContentFingerprintSha256,
            mutated.Admission!.ContentFingerprintSha256);

        var prefixedAdmission = DpoTrainerPairCustoms.CreateAdmitted(
            $"{pair.Prompt}\n\nAnswer:\n",
            pair.Chosen,
            pair.Rejected,
            DpoPairSideAuthorKind.DeterministicGenerator,
            DpoPairSideAuthorKind.DeterministicGenerator,
            nameof(ProjectWikiDpoMetricRunner)).Admission;
        Assert.NotEqual(
            prefixedAdmission.ContentFingerprintSha256,
            admission.ContentFingerprintSha256);
    }

    [Fact]
    public void BuildEvaluationBatchPreviewForTesting_LeavesAdmissionNullWithoutChangingTokenTensors()
    {
        var pair = NewPair(
            "evaluation-preview",
            1,
            "wiki evaluation prompt",
            "chosen evaluation text",
            "rejected evaluation text [WRONG: evaluation_defect - bad wiki signal]");

        var training = ProjectWikiDpoMetricRunner
            .BuildTrainingBatchPreviewForTesting(pair, 128, 16);
        var evaluation = ProjectWikiDpoMetricRunner
            .BuildEvaluationBatchPreviewForTesting(pair, 128, 16);

        Assert.NotNull(training.Admission);
        Assert.Null(evaluation.Admission);
        Assert.Equal(training.ChosenIds.Data, evaluation.ChosenIds.Data);
        Assert.Equal(training.RejectedIds.Data, evaluation.RejectedIds.Data);
        Assert.Equal(training.ChosenMask.Data, evaluation.ChosenMask.Data);
        Assert.Equal(training.RejectedMask.Data, evaluation.RejectedMask.Data);
    }

    [Fact]
    public async Task TrainAndEvaluateAsync_UsesAdmissionsOnlyForTrainingStepManyBatches()
    {
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();
        var runner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 1,
            learningRate: 5e-2f,
            vocabSize: 128,
            contextLen: 16,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true,
            batchObserverForTesting: observations.Add);

        var result = await runner.TrainAndEvaluateAsync(
            "project-wiki-pair-admission-test-unit",
            "project-wiki-pair-admission-test-batch",
            BuildRows("train"),
            BuildRows("heldout"),
            _artifactRoot);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingRowCount);
        Assert.Equal(50, result.Value.HeldOutRowCount);

        var trainingTokenization = Stage(
            observations,
            ProjectWikiDpoMetricRunner.TrainingTokenizationObservationStage);
        var heldOutTokenization = Stage(
            observations,
            ProjectWikiDpoMetricRunner.HeldOutTokenizationObservationStage);
        var stepMany = Stage(
            observations,
            ProjectWikiDpoMetricRunner.StepManyTrainingObservationStage);
        var diagnostics = Stage(
            observations,
            ProjectWikiDpoMetricRunner.PairDiagnosticEvaluationObservationStage);

        Assert.Equal(50, trainingTokenization.Length);
        Assert.Equal(50, heldOutTokenization.Length);
        Assert.Equal(50, stepMany.Length);
        Assert.Equal(100, diagnostics.Length);

        Assert.All(trainingTokenization, observation =>
        {
            Assert.NotNull(observation.Batch.Admission);
            AssertDeterministicProjectWikiAdmission(
                observation.Pair,
                observation.Batch.Admission!);
        });
        Assert.All(stepMany, observation =>
        {
            Assert.NotNull(observation.Batch.Admission);
            AssertDeterministicProjectWikiAdmission(
                observation.Pair,
                observation.Batch.Admission!);
        });
        Assert.All(heldOutTokenization, observation =>
            Assert.Null(observation.Batch.Admission));
        Assert.All(diagnostics, observation =>
            Assert.Null(observation.Batch.Admission));
    }

    [Fact]
    public async Task TrainAndEvaluateAsync_WritesDpoMetricJsonLinesAsOneObjectPerPhysicalLine()
    {
        var runner = new ProjectWikiDpoMetricRunner(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 1,
            learningRate: 5e-2f,
            vocabSize: 128,
            contextLen: 16,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true);
        var trainingRows = BuildRows("train");
        var heldOutRows = BuildRows("heldout");
        trainingRows[0] = trainingRows[0] with
        {
            Prompt = "wiki training prompt with\nan embedded line break",
            Chosen = "GOOD_PROJECT_WIKI_SIGNAL CHOSEN_IS_CORRECT\nsecond chosen line",
            Rejected = "BAD_PROJECT_WIKI_SIGNAL [WRONG: train_defect - multiline]\nsecond rejected line"
        };
        heldOutRows[0] = heldOutRows[0] with
        {
            Prompt = "wiki heldout prompt with\nan embedded line break",
            Chosen = "GOOD_PROJECT_WIKI_SIGNAL CHOSEN_IS_CORRECT\nheldout chosen line",
            Rejected = "BAD_PROJECT_WIKI_SIGNAL [WRONG: heldout_defect - multiline]\nheldout rejected line"
        };

        var result = await runner.TrainAndEvaluateAsync(
            "project-wiki-jsonl-test-unit",
            "project-wiki-jsonl-test-batch",
            trainingRows,
            heldOutRows,
            _artifactRoot);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var sourceLines = File.ReadAllLines(result.Value!.SourcePath);
        // JSONL читается построчно: обучающая или проверочная пара обязана быть
        // одной физической строкой, иначе конвейер обработки получает только "{" и падает.
        AssertJsonLinesContainOneObjectPerPhysicalLine(
            result.Value.SourcePath,
            expectedRows: 100);
        // Переводы строк внутри пользовательских значений остаются частью
        // данных, но в файле они хранятся в экранированном виде и не разрывают JSONL-строку.
        AssertPersistedJsonLinePair(sourceLines[0], trainingRows[0]);
        AssertPersistedJsonLinePair(sourceLines[50], heldOutRows[0]);
        var diagnostics = AssertDpoPairDiagnosticsJsonLines(
            result.Value.PairDiagnosticsPath,
            expectedRows: 100);
        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.PairId == trainingRows[0].PairId &&
            diagnostic.Split == "train");
        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.PairId == heldOutRows[0].PairId &&
            diagnostic.Split == "heldout");
        Assert.Contains(
            "\\n",
            sourceLines[0]);

        var summaryJson = File.ReadAllText(Path.Combine(
            result.Value.ResultRoot,
            "project-wiki-dpo-metric-summary.json"));
        // Обычные JSON-артефакты не становятся JSONL.
        // Отступы итогового JSON сохраняются, потому что писатель артефактов меняет только копию настроек.
        Assert.Contains(
            "\n  \"runId\"",
            summaryJson,
            StringComparison.Ordinal);
    }

    [Fact]
    public void CurrentProductWikiDpoMetricPairConverters_AreDeterministicGeneratorSources()
    {
        var converters = CurrentProductWikiMetricConverters();

        Assert.Equal(11, converters.Length);
        Assert.Equal(
            11,
            converters
                .Select(converter => converter.PipelineFile)
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.All(converters, converter =>
        {
            Assert.Equal(
                DpoPairSideAuthorKind.DeterministicGenerator,
                converter.ChosenAuthor);
            Assert.Equal(
                DpoPairSideAuthorKind.DeterministicGenerator,
                converter.RejectedAuthor);
            Assert.DoesNotContain("User", converter.SourceSummary, StringComparison.Ordinal);
            Assert.DoesNotContain("LocalModel", converter.SourceSummary, StringComparison.Ordinal);
            Assert.DoesNotContain("OpenTeacher", converter.SourceSummary, StringComparison.Ordinal);
            Assert.DoesNotContain("PaidExternalModel", converter.SourceSummary, StringComparison.Ordinal);
        });
    }

    private static ProjectWikiDpoMetricPair[] BuildRows(string split)
        => Enumerable
            .Range(1, 50)
            .Select(index => NewPair(
                split,
                index,
                $"wiki {split} prompt {index}",
                $"GOOD_PROJECT_WIKI_SIGNAL CHOSEN_IS_CORRECT split {split} row {index}",
                $"BAD_PROJECT_WIKI_SIGNAL [WRONG: {split}_defect - row {index}] REJECTED_IS_WRONG"))
            .ToArray();

    private static ProjectWikiDpoMetricPair NewPair(
        string split,
        int index,
        string prompt,
        string chosen,
        string rejected)
        => new(
            $"project-wiki-{split}-pair-{index:000}",
            prompt,
            chosen,
            rejected);

    private static ProjectWikiDpoMetricBatchObservation[] Stage(
        IEnumerable<ProjectWikiDpoMetricBatchObservation> observations,
        string stage)
        => observations
            .Where(observation => string.Equals(
                observation.Stage,
                stage,
                StringComparison.Ordinal))
            .ToArray();

    private static void AssertPersistedJsonLinePair(
        string line,
        ProjectWikiDpoMetricPair expected)
    {
        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;
        Assert.Equal(expected.PairId, root.GetProperty("id").GetString());
        Assert.Equal(expected.Prompt, root.GetProperty("prompt").GetString());
        Assert.Equal(expected.Chosen, root.GetProperty("chosen").GetString());
        Assert.Equal(expected.Rejected, root.GetProperty("rejected").GetString());
    }

    private static void AssertJsonLinesContainOneObjectPerPhysicalLine(
        string path,
        int expectedRows)
    {
        var lines = File.ReadAllLines(path);
        Assert.Equal(expectedRows, lines.Length);
        Assert.All(lines, line =>
        {
            Assert.False(string.IsNullOrWhiteSpace(line));
            using var document = JsonDocument.Parse(line);
            Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        });
    }

    private static ProjectWikiDpoMetricPairDiagnostic[]
        AssertDpoPairDiagnosticsJsonLines(
            string path,
            int expectedRows)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var lines = File.ReadAllLines(path);
        Assert.Equal(expectedRows, lines.Length);
        var diagnostics = lines
            .Select(line =>
            {
                Assert.Contains(
                    "\"pairId\"",
                    line,
                    StringComparison.Ordinal);
                Assert.DoesNotContain(
                    "\"PairId\"",
                    line,
                    StringComparison.Ordinal);
                return JsonSerializer.Deserialize<ProjectWikiDpoMetricPairDiagnostic>(
                    line,
                    options);
            })
            .ToArray();
        Assert.All(diagnostics, Assert.NotNull);
        return diagnostics!;
    }

    private static void AssertDeterministicProjectWikiAdmission(
        ProjectWikiDpoMetricPair pair,
        DpoTrainerPairAdmission admission)
    {
        Assert.Equal(
            DpoPairSideAuthorKind.DeterministicGenerator,
            admission.ChosenAuthor);
        Assert.Equal(
            DpoPairSideAuthorKind.DeterministicGenerator,
            admission.RejectedAuthor);
        Assert.Equal(
            nameof(ProjectWikiDpoMetricRunner),
            admission.IssuerName);
        Assert.False(admission.HasPaidExternalModelSide);

        var expected = DpoTrainerPairCustoms.CreateAdmitted(
            pair.Prompt,
            pair.Chosen,
            pair.Rejected,
            DpoPairSideAuthorKind.DeterministicGenerator,
            DpoPairSideAuthorKind.DeterministicGenerator,
            nameof(ProjectWikiDpoMetricRunner)).Admission;
        Assert.Equal(
            expected.ContentFingerprintSha256,
            admission.ContentFingerprintSha256);
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateProjectWikiLegalTeacherAdmissionAuthority()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        if (!configuration.IsSuccess || configuration.Value is null)
        {
            throw new InvalidOperationException(
                $"ProjectWiki legal-teacher test configuration admission failed: {configuration.ErrorCode}");
        }

        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration.Value,
                static () => true);
    }

    private static ProjectWikiMetricConverterSource[]
        CurrentProductWikiMetricConverters()
        =>
        [
            Converter(
                "42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs",
                "CodeTraceBuilderDpoGenerator deterministic C# seeds plus BuildChosenSignal/BuildRejectedSignal metric tokens."),
            Converter(
                "42.ProjectWikiDomain/CrossStackBusinessFlowGraphTrainingPipeline.cs",
                "CrossStackBusinessFlowGraphDpoGenerator deterministic system boundary seeds and graph unit output."),
            Converter(
                "42.ProjectWikiDomain/DocumentationGeneratorTrainingPipeline.cs",
                "DocumentationGeneratorDpoGenerator deterministic documentation units and generated/signal documentation text."),
            Converter(
                "42.ProjectWikiDomain/EntityImpactMapperTrainingPipeline.cs",
                "EntityImpactMapperDpoGenerator deterministic entity/evidence seeds and metric signal tokens."),
            Converter(
                "42.ProjectWikiDomain/NegativeDpoGeneratorTrainingPipeline.cs",
                "NegativeDpoGeneratorBatchBuilder deterministic documentation units and negative DTO bundle output."),
            Converter(
                "42.ProjectWikiDomain/ProjectAnswerVerifierTrainingPipeline.cs",
                "ProjectAnswerVerifierDpoGenerator deterministic verifier seeds and metric signal tokens."),
            Converter(
                "42.ProjectWikiDomain/ProjectWikiAnswerComposerTrainingPipeline.cs",
                "ProjectWikiAnswerComposerDpoGenerator deterministic evidence records and missing-evidence templates."),
            Converter(
                "42.ProjectWikiDomain/ProjectWikiQueryPlanningTrainingPipeline.cs",
                "ProjectWikiQueryPlanningDpoGenerator deterministic question seeds and query-plan templates."),
            Converter(
                "42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs",
                "ProjectWikiRequestRoutingDpoGenerator deterministic prompt seeds plus router decision output."),
            Converter(
                "42.ProjectWikiDomain/WikiContextRoutingTrainingPipeline.cs",
                "WikiContextRoutingDpoGenerator deterministic context-routing seeds and route templates."),
            Converter(
                "42.ProjectWikiDomain/WikiTraceAggregatorTrainingPipeline.cs",
                "WikiTraceAggregatorDpoGenerator deterministic query/unit trace seeds and aggregation signal tokens.")
        ];

    private static ProjectWikiMetricConverterSource Converter(
        string pipelineFile,
        string sourceSummary)
        => new(
            pipelineFile,
            DpoPairSideAuthorKind.DeterministicGenerator,
            DpoPairSideAuthorKind.DeterministicGenerator,
            sourceSummary);

    private sealed record ProjectWikiMetricConverterSource(
        string PipelineFile,
        DpoPairSideAuthorKind ChosenAuthor,
        DpoPairSideAuthorKind RejectedAuthor,
        string SourceSummary);
}
