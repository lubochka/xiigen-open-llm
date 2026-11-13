// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Security.Cryptography;
using System.Reflection;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using TensorCore.Layers;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using TrainingLoop.Optimizers;

public sealed class LlmIngredientLogicalUnitTests
{
    [Fact]
    public void TransformerEmbeddingKnnLogicalUnit_RejectsDuplicateExampleIds()
    {
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "unit-transformer-knn-duplicate-id-v1",
            "domain-splitter",
            "software-development",
            new LlmModel(BuildTinyConfig()),
            BuildTokenizer(),
            [
                new TransformerKnnTrainingExample<string>(
                    "duplicate-example",
                    "jwt oauth token authorization tenant",
                    "SECURITY",
                    ["SECURITY"],
                    ["security"]),
                new TransformerKnnTrainingExample<string>(
                    "duplicate-example",
                    "layout screen typography button design",
                    "UI",
                    ["UI"],
                    ["ui"])
            ]);

        Assert.False(trained.IsSuccess);
        Assert.Equal("INVALID_TRAINING_EXAMPLE", trained.ErrorCode);
    }

    [Fact]
    public async Task TransformerEmbeddingKnnLogicalUnit_UsesTransformerEmbeddingAndHnswInsideExecuteAsync()
    {
        // Build a tiny local tokenizer and transformer so the test stays CPU-light.
        var tokenizer = BuildTokenizer();
        var model = new LlmModel(BuildTinyConfig());

        // Train a transformer-embedding KNN logical unit.
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "unit-transformer-knn-domain-v1",
            "domain-splitter",
            "software-development",
            model,
            tokenizer,
            [
                new TransformerKnnTrainingExample<string>("pair-security", "jwt oauth token authorization tenant", "SECURITY", ["SECURITY"], ["security"]),
                new TransformerKnnTrainingExample<string>("pair-ui", "layout screen typography button design", "UI", ["UI"], ["ui"])
            ]);

        // Training must succeed through the shared result envelope.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // The manifest must prove projects 01, 03, 05, and 06 are on the unit path.
        Assert.Contains("01.Tokenizer:BpeTokenizer", trained.Value!.Manifest.IngredientIds);
        Assert.Contains("03.TransformerModel:LlmModel", trained.Value.Manifest.IngredientIds);
        Assert.Contains("05.Embeddings:EmbeddingModel", trained.Value.Manifest.IngredientIds);
        Assert.Contains("06.VectorSearch:HnswIndex", trained.Value.Manifest.IngredientIds);
        Assert.Contains("E34V2:BM25_KNN_FUSION", trained.Value.Manifest.IngredientIds);

        // Execute through ILogicalUnit.ExecuteAsync.
        var result = await trained.Value.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-transformer-knn",
            "unit-transformer-knn-domain-v1",
            "domain-splitter",
            "software-development",
            "jwt oauth token authorization tenant",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // Retrieval must return the selected stored example output verbatim.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("SECURITY", result.Value!.Output);
        Assert.Contains("pair-security", result.Value.EvidenceIds);
        Assert.Contains("E34V2:BM25_KNN_FUSION", result.Value.EvidenceIds);
    }

    [Fact]
    public async Task TransformerEmbeddingKnnLogicalUnit_FusesBm25WithKnnForExactDeveloperTerms()
    {
        // Build a tiny local tokenizer and transformer.
        var tokenizer = BuildTokenizer();
        var model = new LlmModel(BuildTinyConfig());

        // Train with strongly different exact developer vocabulary.
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "unit-transformer-fusion-v1",
            "domain-splitter",
            "software-development",
            model,
            tokenizer,
            [
                new TransformerKnnTrainingExample<string>("pair-figma", "figma figma export failure frame returned no data", "FIGMA", ["UI", "IMPLEMENT"], ["frontend"]),
                new TransformerKnnTrainingExample<string>("pair-auth", "jwt oauth token tenant authorization validation", "AUTH", ["SECURITY"], ["security"])
            ]);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // Query with the exact lexical signal that E34V2 is meant to preserve.
        var result = await trained.Value!.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-transformer-fusion",
            "unit-transformer-fusion-v1",
            "domain-splitter",
            "software-development",
            "figma export frame returned no data",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // BM25+KNN fusion should select the exact developer-term example.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("FIGMA", result.Value!.Output);
        Assert.Contains("pair-figma", result.Value.EvidenceIds);
        Assert.Contains("E34V2:BM25_KNN_FUSION", result.Value.EvidenceIds);
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_ExecutesLoraAndDpoThroughLogicalUnit()
    {
        // Build a tiny local tokenizer and model pair.
        var tokenizer = BuildTokenizer();
        var policy = new LlmModel(BuildTinyConfig());
        var reference = new LlmModel(BuildTinyConfig());
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"logical-dpo-{Guid.NewGuid():N}");
        IReadOnlyList<LogicalDpoPreparedOptimizerInput>? observedInputs = null;
        IReadOnlyList<DpoVectorBatch>? observedBatches = null;
        var previousSinkObserver =
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting;

        try
        {
            var preferences = new[]
            {
                LogicalDpoPreference(
                    "Classify the prompt.",
                    "SECURITY",
                    "UI")
            };
            var prepared = LogicalDpoTrainingUnit.PrepareExecution(
                preferences,
                tokenizer);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                (inputs, batches) =>
                {
                    observedInputs = inputs;
                    observedBatches = batches;
                };
            // Build a teaching request with one preference pair.
            var request = new LogicalDpoTrainingRequest(
                policy,
                reference,
                tokenizer,
                checkpointDir,
                preferences,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                AdmissionCapabilities: IssueAdmissionCapabilities(static () =>
                    OperationResult<IDisposable>.Ok(new CancellationTokenSource())))
            {
                PreparedExecution = prepared.Value
            };

            // Execute the teaching unit.
            var result = await new LogicalDpoTrainingUnit().ExecuteAsync(new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                "run-logical-dpo",
                LogicalDpoTrainingUnit.Id,
                LogicalDpoTrainingUnit.Role,
                LogicalDpoTrainingUnit.DomainTag,
                request,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            // The teaching pass must succeed through ILogicalUnit.ExecuteAsync.
            Assert.True(result.IsSuccess, result.ErrorMessage);

            // LoRA and DPO must both be visibly connected to the unit result.
            Assert.True(result.Value!.Output.LoraAdapterCount > 0);
            Assert.Equal(1, result.Value.Output.DpoStepCount);
            Assert.Contains("10.LoRA:LoraApplicator", result.Value.Output.IngredientIds);
            Assert.Contains("11.DPO:DpoTrainer", result.Value.Output.IngredientIds);
            Assert.Single(result.Value.Output.MetricFilePaths);
            Assert.True(File.Exists(result.Value.Output.MetricFilePaths[0]));
            var admittedInput = Assert.Single(
                prepared.Value!.OptimizerInputs);
            Assert.Same(admittedInput, Assert.Single(observedInputs!));
            Assert.Same(admittedInput.Batch, Assert.Single(observedBatches!));
        }
        finally
        {
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                previousSinkObserver;
            // Clean test artifacts without touching repository files.
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_BatchesMultiplePreferencesIntoOneOptimizerStep()
    {
        // Build a tiny local tokenizer and model pair.
        var tokenizer = BuildTokenizer();
        var policy = new LlmModel(BuildTinyConfig());
        var reference = new LlmModel(BuildTinyConfig());
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"logical-dpo-batch-{Guid.NewGuid():N}");

        try
        {
            // Build a teaching request with two preferences that should share one optimizer update.
            var preferences = new[]
            {
                LogicalDpoPreference(
                    "Classify jwt auth.", "SECURITY", "UI"),
                LogicalDpoPreference(
                    "Classify figma export failure.", "FIGMA", "AUTH")
            };
            var prepared = LogicalDpoTrainingUnit.PrepareExecution(
                preferences,
                tokenizer);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var request = new LogicalDpoTrainingRequest(
                policy,
                reference,
                tokenizer,
                checkpointDir,
                preferences,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                AdmissionCapabilities: IssueAdmissionCapabilities(static () =>
                    OperationResult<IDisposable>.Ok(new CancellationTokenSource())))
            {
                PreparedExecution = prepared.Value
            };

            // Execute the teaching unit.
            var result = await new LogicalDpoTrainingUnit().ExecuteAsync(new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                "run-logical-dpo-batch",
                LogicalDpoTrainingUnit.Id,
                LogicalDpoTrainingUnit.Role,
                LogicalDpoTrainingUnit.DomainTag,
                request,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            // Both preference examples are consumed, but only one DPO optimizer step and metric file are emitted.
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(2, result.Value!.Output.PreferenceCount);
            Assert.Equal(1, result.Value.Output.DpoStepCount);
            Assert.Single(result.Value.Output.MetricFilePaths);
            Assert.True(File.Exists(result.Value.Output.MetricFilePaths[0]));
        }
        finally
        {
            // Clean test artifacts without touching repository files.
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public void LogicalDpoTrainingUnit_PrepareExecution_MintsAdmissionFromRawPreferenceStrings()
    {
        var preference = LogicalDpoPreference(
            "  classify\tcapability\nbridge  ",
            " chosen  answer\nwith spacing ",
            " rejected answer\twith spacing ",
            [0.125f, -0.25f]);

        var prepared = LogicalDpoTrainingUnit.PrepareExecution(
            [preference],
            BuildTokenizer());

        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var batch = Assert.Single(prepared.Value!.OptimizerInputs).Batch.Batch;
        var admission = Assert.IsType<DpoTrainerPairAdmission>(batch.Admission);
        var expected = DpoTrainerPairCustoms.CreateAdmitted(
            preference.Prompt,
            preference.Chosen,
            preference.Rejected,
            DpoPairSideAuthorKind.DeterministicGenerator,
            DpoPairSideAuthorKind.DeterministicGenerator,
            nameof(LogicalDpoTrainingUnit)).Admission;
        Assert.Equal(expected.ContentFingerprintSha256,
            admission.ContentFingerprintSha256);
        Assert.Equal(nameof(LogicalDpoTrainingUnit), admission.IssuerName);
        Assert.Equal(DpoPairSideAuthorKind.DeterministicGenerator,
            admission.ChosenAuthor);
        Assert.Equal(DpoPairSideAuthorKind.DeterministicGenerator,
            admission.RejectedAuthor);
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RejectsMissingOneSidedAndUndefinedAuthorsBeforeTokenizerOrOptimizer()
    {
        var invalidPreferenceSets = new[]
        {
            new[]
            {
                new LogicalDpoPreferenceExample(
                    "prompt",
                    "chosen",
                    "rejected",
                    [1f])
            },
            new[]
            {
                new LogicalDpoPreferenceExample(
                    "prompt",
                    "chosen",
                    "rejected",
                    [1f])
                {
                    ChosenAuthorKind =
                        DpoPairSideAuthorKind.DeterministicGenerator
                }
            },
            new[]
            {
                new LogicalDpoPreferenceExample(
                    "prompt",
                    "chosen",
                    "rejected",
                    [1f])
                {
                    ChosenAuthorKind = (DpoPairSideAuthorKind)999,
                    RejectedAuthorKind =
                        DpoPairSideAuthorKind.DeterministicGenerator
                }
            }
        };
        var tokenizationObserved = false;
        var optimizerObserved = false;
        var previousTokenizationObserver =
            LogicalDpoTrainingUnit.BatchTokenizationObserverForTesting;
        var previousSinkObserver =
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting;

        try
        {
            LogicalDpoTrainingUnit.BatchTokenizationObserverForTesting =
                _ => tokenizationObserved = true;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                (_, _) => optimizerObserved = true;
            foreach (var preferences in invalidPreferenceSets)
            {
                var prepared = LogicalDpoTrainingUnit.PrepareExecution(
                    preferences,
                    BuildTokenizer());
                Assert.False(prepared.IsSuccess);
                Assert.Equal(
                    LogicalDpoTrainingUnit.PreferenceBoundsInvalidErrorCode,
                    prepared.ErrorCode);
            }

            var checkpointDir = Path.Combine(
                Path.GetTempPath(),
                $"logical-dpo-authorless-{Guid.NewGuid():N}");
            var request = new LogicalDpoTrainingRequest(
                new LlmModel(BuildTinyConfig()),
                new LlmModel(BuildTinyConfig()),
                BuildTokenizer(),
                checkpointDir,
                invalidPreferenceSets[0],
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                AdmissionCapabilities: IssueAdmissionCapabilities(static () =>
                    OperationResult<IDisposable>.Ok(
                        new CancellationTokenSource())));

            var result = await new LogicalDpoTrainingUnit().ExecuteAsync(
                new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                    "logical-dpo-authorless",
                    LogicalDpoTrainingUnit.Id,
                    LogicalDpoTrainingUnit.Role,
                    LogicalDpoTrainingUnit.DomainTag,
                    request,
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()));

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PREPARED_EXECUTION_REQUIRED", result.ErrorCode);
            Assert.False(tokenizationObserved);
            Assert.False(optimizerObserved);
            Assert.False(Directory.Exists(checkpointDir));
        }
        finally
        {
            LogicalDpoTrainingUnit.BatchTokenizationObserverForTesting =
                previousTokenizationObserver;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                previousSinkObserver;
        }
    }

    [Fact]
    public void LogicalDpoPreferenceExample_ProductCreationSiteCensus_StaysAtSeven()
    {
        var root = FindRepositoryRoot();
        // Do not enter generated result/custody directories: this is a product-source census.
        static IEnumerable<string> ProductSources(string directory)
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*.cs")) yield return file;
            foreach (var child in Directory.EnumerateDirectories(directory))
            {
                var childName = Path.GetFileName(child); // Имя последнего каталога решает, входим мы в него или нет.
                if (childName is "Tests" or "TestResults" or ".git" or ".kiro" or "bin" or "obj") continue;
                // Каталог порождённых результатов опытов: обещание в комментарии выше было, а имени "results" в списке не хватало.
                if (string.Equals(childName, "results", StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var file in ProductSources(child)) yield return file;
            }
        }
        var matches = ProductSources(root)
            .Select(path => (Path: path, Relative: Path.GetRelativePath(root, path)
                .Replace('\\', '/')))
            .Where(item => !item.Relative.StartsWith("Tests/", StringComparison.Ordinal))
            .Where(item => !item.Relative.StartsWith(".git/", StringComparison.Ordinal))
            // Песочница experiments-v2 не входит в LlmFromScratch.sln: там лежат копии продуктовых файлов, а не новые места создания пар.
            .Where(item => !item.Relative.StartsWith("experiments-v2/", StringComparison.Ordinal))
            // Копии внутри 35.ExperimentsV2/Experiments/Rp01Micro исключены из сборки самим ExperimentsV2.csproj (Compile Remove).
            // Исключение из исключения — узел NKu01: он действительно собирается, поэтому остаётся под переписью.
            .Where(item => !item.Relative.StartsWith("35.ExperimentsV2/Experiments/Rp01Micro/", StringComparison.Ordinal)
                || item.Relative.StartsWith("35.ExperimentsV2/Experiments/Rp01Micro/NKu01/", StringComparison.Ordinal))
            .SelectMany(item => File.ReadLines(item.Path)
                .Select(line => line.Trim())
                .Where(line => line.Contains(
                    "new LogicalDpoPreferenceExample(",
                    StringComparison.Ordinal))
                .Select(line => $"{item.Relative}::{line}"))
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [
                // Автономный запускатель обучения на симуляции, оценочная ветка: продуктовая сборка ContinuousLearning, вызывается из tools/Rp01SimulationDpo (коммит bc55daea505 от 15.10.2026).
                "14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs::batches = Require(LogicalDpoPreparedBatchMaterializer.Prepare(rows.Select(row => new LogicalDpoPreferenceExample(row.Prompt, row.Chosen, row.Rejected) // Exact same prompt/answer segmentation as the training objective.",
                // Тот же запускатель, основная обучающая ветка: готовит пары предпочтений перед прогоном.
                "14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs::var preferences = rich.Select(row => new LogicalDpoPreferenceExample(row.Prompt, row.Chosen, row.Rejected) // Metadata and labels cannot enter Student tokenization.",
                "14.ContinuousLearning/Training/DpoTrainingExecution.cs::.Select(static pair => new LogicalDpoPreferenceExample(",
                "18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs::branchPlans.Select(plan => new LogicalDpoPreferenceExample(",
                "18.CapabilityTraining/LogicalExperimentOptimizations.cs::preferences.Add(new LogicalDpoPreferenceExample(",
                "18.CapabilityTraining/LogicalExperimentOptimizations.cs::return new LogicalDpoPreferenceExample(",
                "25.ModelServer/DesignConversion/DesignConversionDpoTrainingHandoff.cs::return new LogicalDpoPreferenceExample("
            ],
            matches);
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_NullMutationPermitFailsBeforeModelOrCheckpointMutation()
    {
        var tokenizer = BuildTokenizer();
        var policy = new LlmModel(BuildTinyConfig());
        var reference = new LlmModel(BuildTinyConfig());
        var initialParameterCount = policy.Parameters().Count();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-no-permit-{Guid.NewGuid():N}");
        var request = new LogicalDpoTrainingRequest(
            policy,
            reference,
            tokenizer,
            checkpointDir,
            [new LogicalDpoPreferenceExample("prompt", "chosen", "rejected")],
            new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["q_proj", "v_proj"]
            });

        var result = await new LogicalDpoTrainingUnit().ExecuteAsync(
            new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                "logical-dpo-no-permit",
                LogicalDpoTrainingUnit.Id,
                LogicalDpoTrainingUnit.Role,
                LogicalDpoTrainingUnit.DomainTag,
                request,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LogicalCapabilityTrainingAdmissionCapabilities.RequiredErrorCode,
            result.ErrorCode);
        Assert.Equal(initialParameterCount, policy.Parameters().Count());
        Assert.False(Directory.Exists(checkpointDir));
    }

    [Fact]
    public void LogicalDpoMutationAdmissionCapability_HasNoPublicForgeOrPermitSurface()
    {
        var capabilityType = typeof(LogicalDpoMutationAdmissionCapability);
        var declaredPublicMethods = capabilityType.GetMethods(
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.DeclaredOnly);

        Assert.True(capabilityType.IsSealed);
        Assert.Empty(capabilityType.GetConstructors());
        Assert.DoesNotContain(
            declaredPublicMethods,
            method => method.Name is "Issue" or
                "AcquireMutationPermit" or
                "IssueDpoTrainerMutationAdmissionCapability" or
                "AcquireDpoTrainerMutationPermit");
        Assert.DoesNotContain(
            capabilityType.GetInterfaces(),
            interfaceType => interfaceType.FullName == "DPO.IDpoMutationAdmissionIssuer");
        Assert.DoesNotContain(
            capabilityType.GetMethods(
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.DeclaredOnly),
            method => method.Name.Contains(
                    "AcquireDpoTrainerMutationPermit",
                    StringComparison.Ordinal) ||
                method.ReturnType == typeof(DpoMutationAdmissionCapability));
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RejectsAdversarialRowsBeforePermitOrMutation()
    {
        var oversizedUtf8 = new string('\u20ac',
            (LogicalDpoTrainingUnit.MaximumPreferenceTextUtf8Bytes / 3) + 1);
        var invalidPreferenceSets = new LogicalDpoPreferenceExample[][]
        {
            [null!],
            [LogicalDpoPreference(null!, "chosen", "rejected")],
            [LogicalDpoPreference(oversizedUtf8, "chosen", "rejected")],
            [LogicalDpoPreference("prompt", "chosen", "rejected", [float.NaN])],
            [LogicalDpoPreference("prompt", "chosen", "rejected", [float.PositiveInfinity])],
            [LogicalDpoPreference(
                "prompt",
                "chosen",
                "rejected",
                new float[LogicalDpoTrainingUnit.MaximumTeachingVectorLength + 1])],
            new LogicalDpoPreferenceExample[
                LogicalDpoTrainingUnit.MaximumPreferenceCount + 1]
        };

        foreach (var preferences in invalidPreferenceSets)
        {
            var permitCallCount = 0;
            var policy = new LlmModel(BuildTinyConfig());
            var initialParameterCount = policy.Parameters().Count();
            var tokenizer = BuildTokenizer();
            var preparation = LogicalDpoTrainingUnit.PrepareExecution(
                preferences,
                tokenizer);
            Assert.False(preparation.IsSuccess);
            Assert.Equal(
                LogicalDpoTrainingUnit.PreferenceBoundsInvalidErrorCode,
                preparation.ErrorCode);
            var checkpointDir = Path.Combine(
                Path.GetTempPath(),
                $"logical-dpo-invalid-bounds-{Guid.NewGuid():N}");
            var request = new LogicalDpoTrainingRequest(
                policy,
                new LlmModel(BuildTinyConfig()),
                tokenizer,
                checkpointDir,
                preferences,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                AdmissionCapabilities: IssueAdmissionCapabilities(() =>
                {
                    permitCallCount++;
                    return OperationResult<IDisposable>.Ok(
                        new CancellationTokenSource());
                }));

            var result = await new LogicalDpoTrainingUnit().ExecuteAsync(
                new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                    "logical-dpo-invalid-bounds",
                    LogicalDpoTrainingUnit.Id,
                    LogicalDpoTrainingUnit.Role,
                    LogicalDpoTrainingUnit.DomainTag,
                    request,
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DPO_PREPARED_EXECUTION_REQUIRED",
                result.ErrorCode);
            Assert.Equal(0, permitCallCount);
            Assert.Equal(initialParameterCount, policy.Parameters().Count());
            Assert.False(Directory.Exists(checkpointDir));
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RevocationAndCancellationAfterFirstStepReturnsRevokedBeforeSecondStep()
    {
        using var cancellation = new CancellationTokenSource();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-revoke-after-step-{Guid.NewGuid():N}");
        var stepOnePolicyPath = Path.Combine(
            checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var stepOneMetricsPath = Path.Combine(
            checkpointDir,
            DpoTrainer.BuildMetricsFileName(1));
        var stepTwoPolicyPath = Path.Combine(
            checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(2));
        var stepTwoMetricsPath = Path.Combine(
            checkpointDir,
            DpoTrainer.BuildMetricsFileName(2));
        var permitCallCount = 0;
        var revocationTriggerCount = 0;
        var revoked = false;
        OperationResult<IDisposable> AcquirePermit()
        {
            permitCallCount++;
            if (revoked)
            {
                return OperationResult<IDisposable>.Fail(
                    "The legal teacher training admission was revoked.",
                    "LEGAL_TRAINING_ADMISSION_REVOKED");
            }

            return OperationResult<IDisposable>.Ok(
                new CallbackDisposable(() =>
                {
                    if (!revoked &&
                        File.Exists(stepOnePolicyPath) &&
                        File.Exists(stepOneMetricsPath))
                    {
                        revoked = true;
                        revocationTriggerCount++;
                        cancellation.Cancel();
                    }
                }));
        }

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-revoke-after-step",
                new LlmModel(BuildTinyConfig()),
                new LlmModel(BuildTinyConfig()),
                BuildTokenizer(),
                checkpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                maxStepCount: 2,
                acquireMutationPermit: AcquirePermit,
                cancellationToken: cancellation.Token)
                .WaitAsync(TimeSpan.FromSeconds(10));

            Assert.False(result.IsSuccess);
            Assert.Equal("LEGAL_TRAINING_ADMISSION_REVOKED", result.ErrorCode);
            Assert.True(cancellation.IsCancellationRequested);
            // Preparation, trainer construction, outer step one, inner step one,
            // then the rejected outer step-two acquisition.
            Assert.Equal(5, permitCallCount);
            Assert.Equal(1, revocationTriggerCount);
            Assert.True(File.Exists(stepOnePolicyPath));
            Assert.True(File.Exists(stepOneMetricsPath));
            Assert.False(File.Exists(stepTwoPolicyPath));
            Assert.False(File.Exists(stepTwoMetricsPath));
            Assert.Equal(2, Directory.GetFiles(checkpointDir).Length);
            Assert.Empty(Directory.GetDirectories(checkpointDir));
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RevocationBeforePreparationCreatesNoCheckpointDirectory()
    {
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-revoke-before-preparation-{Guid.NewGuid():N}");

        var result = await ExecuteDpoAsync(
            "run-logical-dpo-revoke-before-preparation",
            new LlmModel(BuildTinyConfig()),
            new LlmModel(BuildTinyConfig()),
            BuildTokenizer(),
            checkpointDir,
            new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["q_proj", "v_proj"]
            },
            acquireMutationPermit: static () =>
                OperationResult<IDisposable>.Fail(
                    "The legal teacher training admission was revoked.",
                    "LEGAL_TRAINING_ADMISSION_REVOKED"));

        Assert.False(result.IsSuccess);
        Assert.Equal("LEGAL_TRAINING_ADMISSION_REVOKED", result.ErrorCode);
        Assert.False(Directory.Exists(checkpointDir));
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_DefaultTestHelperSuppliesMutationPermit()
    {
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-null-permit-seam-{Guid.NewGuid():N}");

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-null-permit-seam",
                new LlmModel(BuildTinyConfig()),
                new LlmModel(BuildTinyConfig()),
                BuildTokenizer(),
                checkpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                });

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(File.Exists(Path.Combine(
                checkpointDir,
                DpoTrainer.BuildPolicyCheckpointFileName(1))));
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_ConvergenceStopsBeforeConfiguredMaximum()
    {
        var config = BuildTinyConfig();
        var tokenizer = BuildTokenizer();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-early-convergence-{Guid.NewGuid():N}");

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-early-convergence",
                new LlmModel(config),
                new LlmModel(config),
                tokenizer,
                checkpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                maxStepCount: LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount,
                convergenceThreshold: float.MaxValue,
                convergenceWindowSteps: 1);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(3, result.Value!.Output.DpoStepCount);
            Assert.True(File.Exists(Path.Combine(
                checkpointDir,
                DpoTrainer.BuildPolicyCheckpointFileName(3))));
            Assert.True(File.Exists(Path.Combine(
                checkpointDir,
                DpoTrainer.BuildMetricsFileName(3))));
            Assert.False(File.Exists(Path.Combine(
                checkpointDir,
                DpoTrainer.BuildPolicyCheckpointFileName(4))));
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RejectsMaximumAboveBoundBeforeCheckpointSideEffects()
    {
        var config = BuildTinyConfig();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-invalid-maximum-{Guid.NewGuid():N}");

        var result = await ExecuteDpoAsync(
            "run-logical-dpo-invalid-maximum",
            new LlmModel(config),
            new LlmModel(config),
            BuildTokenizer(),
            checkpointDir,
            new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["q_proj", "v_proj"]
            },
            maxStepCount: LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount + 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(LogicalDpoTrainingUnit.MaxStepCountOutOfRangeErrorCode, result.ErrorCode);
        Assert.False(Directory.Exists(checkpointDir));
    }

    // Отказ «настройки сходимости недопустимы» объявлен в продукте
    // (18.CapabilityTraining/LlmIngredientLogicalUnits.cs, строки 897-904), но до
    // этих проверок его не подтверждал ни один тест в дереве. Условие состоит из
    // трёх слагаемых, соединённых «или»:
    //   (1) !float.IsFinite(порог) — не-число и обе бесконечности;
    //   (2) порог < 0f             — любое конечное отрицательное значение;
    //   (3) интервал наблюдения < 1 — нуль и любое отрицательное целое.
    // ГРАНИЦА, которую важно назвать прямо: нулевой порог эту проверку проходит
    // НАМЕРЕННО и дефектом не является. При нулевом пороге наблюдатель сходимости
    // просто не создаётся, и прогон идёт на весь отведённый бюджет шагов. Эти
    // проверки допуск нуля не меняют и не оспаривают: ниже он стоит на принимаемой
    // стороне той же самой проверки.
    [Theory]
    // Строка 1: порог не является числом. По стандарту IEEE-754 сравнение
    // NaN < 0f ложно, поэтому эту строку ловит ТОЛЬКО слагаемое (1) — она и
    // доказывает, что без него отказ исчезнет.
    [InlineData(float.NaN, 25, "порог не является числом")]
    // Строка 2: плюс бесконечность. Сравнение с нулём тоже ложно, значит и здесь
    // работает только слагаемое (1).
    [InlineData(float.PositiveInfinity, 25, "порог равен плюс бесконечности")]
    // Строка 3: минус бесконечность. Её ловят сразу (1) и (2), поэтому отдельным
    // доказательством нужности одного слагаемого она служить не может — она здесь
    // ради полноты перечисления трёх нечисловых состояний float.
    [InlineData(float.NegativeInfinity, 25, "порог равен минус бесконечности")]
    // Строка 4: конечное отрицательное значение. Его ловит ТОЛЬКО слагаемое (2).
    [InlineData(-0.001f, 25, "порог конечный и отрицательный")]
    // Строка 5: нулевой интервал наблюдения. Только слагаемое (3).
    [InlineData(0.002f, 0, "интервал наблюдения равен нулю")]
    // Строка 6: отрицательный интервал наблюдения. Тоже только слагаемое (3).
    [InlineData(0.002f, -1, "интервал наблюдения отрицательный")]
    public async Task LogicalDpoTrainingUnit_RefusesEachInvalidConvergenceSettingBeforePermitOrCheckpoint(
        float convergenceThreshold, int convergenceWindowSteps, string branch)
    {
        var config = BuildTinyConfig(); // Крошечная модель: проверяется отказ, а не обучение.
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        var policyBefore = CaptureDpoModelSnapshot(policy); // Состояние до попытки.
        var referenceBefore = CaptureDpoModelSnapshot(reference);
        var permitCallCount = 0; // Сколько раз спросили право на изменение модели.
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-invalid-convergence-{Guid.NewGuid():N}");

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-invalid-convergence",
                policy,
                reference,
                BuildTokenizer(),
                checkpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                convergenceThreshold: convergenceThreshold,
                convergenceWindowSteps: convergenceWindowSteps,
                acquireMutationPermit: () =>
                {
                    permitCallCount++; // Любой вызов означает, что отказ опоздал.
                    return OperationResult<IDisposable>.Ok(
                        new CancellationTokenSource());
                });

            // Отказ объявлен ровно тем кодом и текстом, которые обещает продукт.
            Assert.False(result.IsSuccess, branch);
            Assert.Equal("DPO_CONVERGENCE_SETTINGS_INVALID", result.ErrorCode);
            Assert.Equal(
                "DPO convergence settings are invalid.",
                result.ErrorMessage);
            // Отказ случился раньше, чем взято право на изменение модели: проверка
            // настроек стоит выше выдачи разрешения в том же методе.
            Assert.Equal(0, permitCallCount);
            // И раньше любого следа на диске: каталог снимков даже не создан.
            Assert.False(Directory.Exists(checkpointDir));
            // Обе живые модели остались те же: ни присоединения переходников,
            // ни заморозки, ни изменения чисел.
            AssertDpoModelSnapshot(policyBefore, policy);
            AssertDpoModelSnapshot(referenceBefore, reference);
            AssertFreshUnfrozen(policy);
            AssertFreshUnfrozen(reference);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    // Принимаемая сторона той же проверки: показывает, что она не отказывает
    // лишнего. Первые две строки — это и есть прямое подтверждение границы:
    // нулевой порог остаётся принимаемым значением.
    [Theory]
    [InlineData(0f, 25, "нулевой порог при обычном интервале наблюдения")]
    [InlineData(0f, 1, "нулевой порог при наименьшем допустимом интервале")]
    [InlineData(0.002f, 25, "обычные рабочие настройки")]
    [InlineData(float.MaxValue, 1, "наибольший конечный порог")]
    public async Task LogicalDpoTrainingUnit_AdmitsConvergenceSettingsOnTheAllowedSideOfTheSameCheck(
        float convergenceThreshold, int convergenceWindowSteps, string admitted)
    {
        var config = BuildTinyConfig();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-admitted-convergence-{Guid.NewGuid():N}");

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-admitted-convergence",
                new LlmModel(config),
                new LlmModel(config),
                BuildTokenizer(),
                checkpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                convergenceThreshold: convergenceThreshold,
                convergenceWindowSteps: convergenceWindowSteps);

            // Проверка настроек пропустила запрос, и обучающий проход состоялся.
            Assert.True(result.IsSuccess, $"{admitted}: {result.ErrorMessage}");
            Assert.Equal(1, result.Value!.Output.DpoStepCount);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_NegativeZeroConvergenceThresholdRemainsAdmitted()
    {
        // Отрицательный нуль: знаковый бит установлен, но по стандарту IEEE-754
        // сравнение -0.0f < 0f ложно, а само значение конечно. Значит ни (1), ни (2)
        // его не ловят, и он остаётся принимаемым. Строка существует, чтобы будущий
        // читатель не «починил» проверку в сторону отказа на знаковом бите.
        var negativeZero = BitConverter.Int32BitsToSingle(unchecked((int)0x80000000));
        Assert.True(float.IsNegative(negativeZero), "Знаковый бит должен быть установлен.");
        Assert.True(negativeZero == 0f, "Отрицательный нуль равен нулю.");
        Assert.False(negativeZero < 0f, "Отрицательный нуль не меньше нуля.");
        Assert.True(float.IsFinite(negativeZero), "Отрицательный нуль конечен.");
        var config = BuildTinyConfig();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-negative-zero-convergence-{Guid.NewGuid():N}");

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-negative-zero-convergence",
                new LlmModel(config),
                new LlmModel(config),
                BuildTokenizer(),
                checkpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                convergenceThreshold: negativeZero,
                convergenceWindowSteps: 1);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, result.Value!.Output.DpoStepCount);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public void LogicalDpoTrainingUnit_ConvergenceSettingsRefusalConditionStaysAtThreeCoveredTerms()
    {
        // Перепись самого условия по исходному тексту продукта. Смысл: если кто-то
        // добавит в отказ четвёртое слагаемое, эта проверка упадёт и заставит
        // написать для новой ветви отдельную проверку, а не оставить её без охвата.
        var root = FindRepositoryRoot();
        var productPath = Path.Combine(
            root,
            "18.CapabilityTraining",
            "LlmIngredientLogicalUnits.cs");
        var lines = File.ReadAllLines(productPath);
        var refusalIndexes = Enumerable.Range(0, lines.Length)
            .Where(index => lines[index].Contains(
                "\"DPO_CONVERGENCE_SETTINGS_INVALID\"",
                StringComparison.Ordinal))
            .ToArray();
        // Место отказа в этом файле ровно одно; иначе перепись охватывает не всё.
        var refusalIndex = Assert.Single(refusalIndexes);
        var conditionStart = refusalIndex;
        while (conditionStart > 0 &&
            !lines[conditionStart].TrimStart().StartsWith(
                "if (",
                StringComparison.Ordinal))
        {
            conditionStart--; // Идём вверх до строки, открывающей условие.
        }

        Assert.True(
            conditionStart > 0,
            "Условие отказа не найдено выше строки с кодом отказа.");
        var conditionEnd = conditionStart;
        while (conditionEnd < refusalIndex &&
            !lines[conditionEnd].TrimEnd().EndsWith(")", StringComparison.Ordinal))
        {
            conditionEnd++; // Условие занимает несколько строк: ищем закрывающую.
        }

        var condition = string.Join(
            " ",
            lines[conditionStart..(conditionEnd + 1)].Select(line => line.Trim()));

        Assert.Equal(
            "if (!float.IsFinite(request.Input.ConvergenceThreshold) || "
                + "request.Input.ConvergenceThreshold < 0f || "
                + "request.Input.ConvergenceWindowSteps < 1)",
            condition);
        // Три слагаемых — два разделителя «или». Каждое слагаемое покрыто выше.
        Assert.Equal(2, condition.Split("||").Length - 1);
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_SeededInvalidMaximumDoesNotAttachOrFreeze()
    {
        var config = BuildTinyConfig();
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        var policyBefore = CaptureDpoModelSnapshot(policy);
        var referenceBefore = CaptureDpoModelSnapshot(reference);
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-seeded-invalid-maximum-{Guid.NewGuid():N}");

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-seeded-invalid-maximum",
                policy,
                reference,
                BuildTokenizer(),
                checkpointDir,
                CreateSeededLoraConfig(),
                maxStepCount:
                    LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount + 1,
                adapterSeedAuthority: CreateSeedAuthority());

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LogicalDpoTrainingUnit.MaxStepCountOutOfRangeErrorCode,
                result.ErrorCode);
            AssertDpoModelSnapshot(policyBefore, policy);
            AssertDpoModelSnapshot(referenceBefore, reference);
            AssertFreshUnfrozen(policy);
            AssertFreshUnfrozen(reference);
            Assert.False(Directory.Exists(checkpointDir));
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_SeededEmptyTargetsFailWithoutTopologyMutation()
    {
        var config = BuildTinyConfig();
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        var policyBefore = CaptureDpoModelSnapshot(policy);
        var referenceBefore = CaptureDpoModelSnapshot(reference);
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-seeded-empty-targets-{Guid.NewGuid():N}");

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-seeded-empty-targets",
                policy,
                reference,
                BuildTokenizer(),
                checkpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = []
                },
                adapterSeedAuthority: CreateSeedAuthority());

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_LORA_APPLICATION_FAILED", result.ErrorCode);
            AssertDpoModelSnapshot(policyBefore, policy);
            AssertDpoModelSnapshot(referenceBefore, reference);
            AssertFreshUnfrozen(policy);
            AssertFreshUnfrozen(reference);
            Assert.False(Directory.Exists(checkpointDir));
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_SeededParentInputsFailBeforeArtifactOpenOrTopology()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-seeded-parent-rejection-{Guid.NewGuid():N}");
        var loraConfig = CreateSeededLoraConfig();
        var config = BuildTinyConfig();
        var missingPolicyPath = Path.Combine(root, "missing-policy.bin");
        var provenance = new LogicalDpoParentProvenance(
            config,
            loraConfig,
            Path.Combine(root, "missing-tokenizer.json"),
            new string('0', 64),
            new string('1', 64),
            new string('2', 64));
        var cases = new (string? ParentPath, LogicalDpoParentProvenance? Provenance)[]
        {
            (missingPolicyPath, null),
            (null, provenance),
            (missingPolicyPath, provenance)
        };

        try
        {
            foreach (var (parentPath, parentProvenance) in cases)
            {
                var policy = new LlmModel(config);
                var reference = new LlmModel(config);
                var policyBefore = CaptureDpoModelSnapshot(policy);
                var referenceBefore = CaptureDpoModelSnapshot(reference);
                var checkpointDir = Path.Combine(
                    root,
                    Guid.NewGuid().ToString("N"));

                var result = await ExecuteDpoAsync(
                    "run-logical-dpo-seeded-parent-rejection",
                    policy,
                    reference,
                    BuildTokenizer(),
                    checkpointDir,
                    loraConfig,
                    parentPolicyCheckpointPath: parentPath,
                    parentProvenance: parentProvenance,
                    adapterSeedAuthority: CreateSeedAuthority());

                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "DPO_SEEDED_CONTINUATION_UNSUPPORTED",
                    result.ErrorCode);
                AssertDpoModelSnapshot(policyBefore, policy);
                AssertDpoModelSnapshot(referenceBefore, reference);
                Assert.False(Directory.Exists(checkpointDir));
            }

            Assert.False(File.Exists(missingPolicyPath));
            Assert.False(Directory.Exists(root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_SeededCheckpointFileFailsTrainerInitializationBeforeAttach()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-seeded-checkpoint-file-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var checkpointFile = Path.Combine(root, "checkpoint-target");
        File.WriteAllText(checkpointFile, "not-a-directory");
        var config = BuildTinyConfig();
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        var policyBefore = CaptureDpoModelSnapshot(policy);
        var referenceBefore = CaptureDpoModelSnapshot(reference);

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-seeded-checkpoint-file",
                policy,
                reference,
                BuildTokenizer(),
                checkpointFile,
                CreateSeededLoraConfig(),
                adapterSeedAuthority: CreateSeedAuthority(),
                randomStateColdStartAuthorization: CreateFreshRandomAuthorization());

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_TRAINER_INITIALIZATION_FAILED", result.ErrorCode);
            AssertDpoModelSnapshot(policyBefore, policy);
            AssertDpoModelSnapshot(referenceBefore, reference);
            AssertFreshUnfrozen(policy);
            AssertFreshUnfrozen(reference);
            Assert.True(File.Exists(checkpointFile));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("q_proj", "reference")]
    [InlineData("o_proj", "reference")]
    [InlineData("q_proj", "policy")]
    [InlineData("o_proj", "policy")]
    [InlineData("q_proj", "both")]
    [InlineData("o_proj", "both")]
    public async Task LogicalDpoTrainingUnit_SeededPreoccupiedReferenceLeavesPolicyUntouched(
        string occupiedProjection, string occupiedSide)
    {
        var config = BuildTinyConfig();
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        foreach (var model in occupiedSide == "both" ? new[] { policy, reference }
                     : new[] { occupiedSide == "policy" ? policy : reference })
            Assert.Single(LoraApplicator.Apply(model, new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = [occupiedProjection]
            }));
        var policyBefore = CaptureDpoModelSnapshot(policy);
        var referenceBefore = CaptureDpoModelSnapshot(reference);
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-seeded-preoccupied-{Guid.NewGuid():N}");

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-seeded-preoccupied",
                policy,
                reference,
                BuildTokenizer(),
                checkpointDir,
                CreateSeededLoraConfig(),
                adapterSeedAuthority: CreateSeedAuthority(),
                randomStateColdStartAuthorization: CreateFreshRandomAuthorization());

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_REFERENCE_PROVENANCE_MISMATCH", result.ErrorCode);
            AssertDpoModelSnapshot(policyBefore, policy);
            AssertDpoModelSnapshot(referenceBefore, reference);
            if (occupiedSide == "reference") AssertFreshUnfrozen(policy);
            if (occupiedSide == "policy") AssertFreshUnfrozen(reference);
            Assert.False(Directory.Exists(checkpointDir));
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_SeededSuccessUsesRoleProvenanceAndFreezesReference()
    {
        var config = BuildTinyConfig();
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        var seedAuthority = CreateSeedAuthority();
        var loraConfig = CreateSeededLoraConfig();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-seeded-success-{Guid.NewGuid():N}");

        try
        {
            var result = await ExecuteDpoAsync(
                "run-logical-dpo-seeded-success",
                policy,
                reference,
                BuildTokenizer(),
                checkpointDir,
                loraConfig,
                adapterSeedAuthority: seedAuthority,
                randomStateColdStartAuthorization: CreateFreshRandomAuthorization());

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(result.Value!.Output.LoraAdapterCount > 0);
            Assert.Equal(2, result.Value.Output.LoraAdapterCount);
            foreach (var projection in new[] { "q_proj", "v_proj" })
            {
                var policyAdapter = Assert.IsType<LoraLinear>(
                    policy.GetAttachedProjectionAdapter(0, projection));
                var referenceAdapter = Assert.IsType<LoraLinear>(
                    reference.GetAttachedProjectionAdapter(0, projection));
                var expectedPolicySeeds = LoraAdapterRandomSeedDerivation.Derive(
                    seedAuthority.RootInitializationSeed,
                    0,
                    projection,
                    seedAuthority.PolicyDropoutStreamDomain);
                var expectedReferenceSeeds = LoraAdapterRandomSeedDerivation.Derive(
                    seedAuthority.RootInitializationSeed,
                    0,
                    projection,
                    seedAuthority.ReferenceDropoutStreamDomain);

                Assert.Equal(expectedPolicySeeds, policyAdapter.RandomSeeds);
                Assert.Equal(expectedReferenceSeeds, referenceAdapter.RandomSeeds);
                Assert.Equal(
                    expectedPolicySeeds.InitializationSeed,
                    expectedReferenceSeeds.InitializationSeed);
                Assert.NotEqual(
                    expectedPolicySeeds.DropoutSeed,
                    expectedReferenceSeeds.DropoutSeed);
                Assert.All(
                    referenceAdapter.AdapterParameters,
                    pair =>
                    {
                        Assert.False(pair.Parameter.Value.RequiresGrad);
                        Assert.Null(pair.Parameter.Value.Grad);
                    });
            }

            Assert.All(reference.Parameters(), parameter =>
            {
                Assert.False(parameter.Value.RequiresGrad);
                Assert.Null(parameter.Value.Grad);
            });
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_CapturesFrozenIndependentFullStepZeroPolicyState()
    {
        var config = BuildTinyConfig();
        var tokenizer = BuildTokenizer();
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var adapters = LoraApplicator.Apply(policy, loraConfig);
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-reference-provenance-{Guid.NewGuid():N}");

        try
        {
            FillParametersDeterministically(policy.BaseParameters, 0.125f);
            FillParametersDeterministically(
                adapters.SelectMany(adapter => adapter.Parameters()),
                0.175f);
            var expectedState = SnapshotCheckpointState(policy);
            var expectedBase = SnapshotParameterBits(policy.BaseParameters);
            var adaptersBefore = SnapshotParameterBits(
                adapters.SelectMany(adapter => adapter.Parameters()));

            var result = await ExecuteDpoAsync(
                "run-logical-dpo-reference-provenance",
                policy,
                reference,
                tokenizer,
                checkpointDir,
                loraConfig);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            AssertParameterBitsEqual(expectedBase, policy.BaseParameters);
            AssertCheckpointStateEqual(expectedState, reference);
            Assert.All(reference.Parameters(), parameter =>
            {
                Assert.False(parameter.Value.RequiresGrad);
                Assert.Null(parameter.Value.Grad);
            });

            var policyEntries = policy.GetCheckpointParameterEntries();
            var referenceEntries = reference.GetCheckpointParameterEntries();
            Assert.Equal(policyEntries.Count, referenceEntries.Count);
            for (var index = 0; index < policyEntries.Count; index++)
            {
                Assert.Equal(policyEntries[index].Key, referenceEntries[index].Key);
                Assert.False(ReferenceEquals(
                    policyEntries[index].Parameter,
                    referenceEntries[index].Parameter));
                Assert.False(ReferenceEquals(
                    policyEntries[index].Parameter.Value,
                    referenceEntries[index].Parameter.Value));
                Assert.False(ReferenceEquals(
                    policyEntries[index].Parameter.Value.Storage,
                    referenceEntries[index].Parameter.Value.Storage));
                Assert.False(ReferenceEquals(
                    policyEntries[index].Parameter.Value.Data,
                    referenceEntries[index].Parameter.Value.Data));
            }

            var adaptersAfter = SnapshotParameterBits(
                adapters.SelectMany(adapter => adapter.Parameters()));
            Assert.True(
                adaptersBefore.Zip(adaptersAfter)
                    .Any(pair => !pair.First.SequenceEqual(pair.Second)),
                "At least one live policy adapter parameter must change during the DPO optimizer step.");
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_ReferenceCarriesExactFullParentCheckpointProvenance()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var parent = new LlmModel(config);
        var parentAdapters = LoraApplicator.Apply(parent, loraConfig);
        FillParametersDeterministically(parent.BaseParameters, 0.25f);
        FillParametersDeterministically(
            parentAdapters.SelectMany(adapter => adapter.Parameters()),
            0.375f);
        var expectedParentState = SnapshotCheckpointState(parent);
        var expectedParentBase = SnapshotParameterBits(parent.BaseParameters);
        var expectedParentAdapters = SnapshotParameterBits(
            parentAdapters.SelectMany(adapter => adapter.Parameters()));
        var fixedTokens = new Tensor([1f, 2f, 3f], new TensorShape(1, 3));
        var expectedStepZeroOutput = parent.Forward(fixedTokens, training: false).Data
            .Select(BitConverter.SingleToInt32Bits)
            .ToArray();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-parent-provenance-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(checkpointDir, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(checkpointDir, "parent-tokenizer.json");

        try
        {
            Directory.CreateDirectory(checkpointDir);
            parent.SaveCheckpoint(parentCheckpointPath);
            tokenizer.Save(parentTokenizerPath);
            var policy = new LlmModel(config);
            var reference = new LlmModel(config);
            var parentProvenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);

            var result = await ExecuteDpoAsync(
                "run-logical-dpo-parent-provenance",
                policy,
                reference,
                tokenizer,
                checkpointDir,
                loraConfig,
                parentCheckpointPath,
                parentProvenance);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            AssertParameterBitsEqual(expectedParentBase, policy.BaseParameters);
            AssertCheckpointStateEqual(expectedParentState, reference);
            Assert.All(reference.Parameters(), parameter => Assert.False(parameter.Value.RequiresGrad));
            Assert.Equal(
                expectedStepZeroOutput,
                reference.Forward(fixedTokens, training: false).Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());

            var policyAdapters = policy.Parameters().Skip(policy.BaseParameters.Count);
            var policyAdapterBits = SnapshotParameterBits(policyAdapters);
            Assert.True(
                expectedParentAdapters.Zip(policyAdapterBits)
                    .Any(pair => !pair.First.SequenceEqual(pair.Second)),
                "DPO must update the live policy adapters loaded from the parent checkpoint.");
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_FailsClosedForMissingMismatchedOrSharedReferenceProvenance()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-provenance-fail-closed-{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(checkpointDir);
            var missing = await ExecuteDpoAsync(
                "run-logical-dpo-missing-parent",
                new LlmModel(config),
                new LlmModel(config),
                tokenizer,
                checkpointDir,
                loraConfig,
                Path.Combine(checkpointDir, "missing-parent.bin"));
            Assert.False(missing.IsSuccess);
            Assert.Equal("DPO_PARENT_PROVENANCE_REQUIRED", missing.ErrorCode);
            Assert.Equal(
                "Parent DPO provenance is required for continued training.",
                missing.ErrorMessage);
            Assert.DoesNotContain(checkpointDir, missing.ErrorMessage, StringComparison.OrdinalIgnoreCase);

            var mismatchedParentPath = Path.Combine(checkpointDir, "mismatched-parent.bin");
            var parentTokenizerPath = Path.Combine(checkpointDir, "parent-tokenizer.json");
            tokenizer.Save(parentTokenizerPath);
            new LlmModel(config with { FfnHiddenDim = config.FfnHiddenDim + 16 })
                .SaveCheckpoint(mismatchedParentPath);
            var mismatchedProvenance = CreateParentProvenance(
                config,
                loraConfig,
                mismatchedParentPath,
                parentTokenizerPath,
                tokenizer);
            var mismatched = await ExecuteDpoAsync(
                "run-logical-dpo-mismatched-parent",
                new LlmModel(config),
                new LlmModel(config),
                tokenizer,
                checkpointDir,
                loraConfig,
                mismatchedParentPath,
                mismatchedProvenance);
            Assert.False(mismatched.IsSuccess);
            Assert.Equal("PARENT_POLICY_CHECKPOINT_LOAD_FAILED", mismatched.ErrorCode);
            Assert.Equal(
                "Parent policy checkpoint could not be loaded into the DPO models.",
                mismatched.ErrorMessage);

            var shared = new LlmModel(config);
            var sharedReference = await ExecuteDpoAsync(
                "run-logical-dpo-shared-reference",
                shared,
                shared,
                tokenizer,
                checkpointDir,
                loraConfig);
            Assert.False(sharedReference.IsSuccess);
            Assert.Equal("DPO_REFERENCE_STORAGE_SHARED", sharedReference.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RejectsSameShapeParentBehaviorConfigMismatchBeforeMutation()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-config-provenance-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");

        try
        {
            Directory.CreateDirectory(root);
            var parent = new LlmModel(config);
            _ = LoraApplicator.Apply(parent, loraConfig);
            parent.SaveCheckpoint(parentCheckpointPath);
            tokenizer.Save(parentTokenizerPath);
            var validProvenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);
            var mismatchedConfigs = new[]
            {
                config with { RopeTheta = config.RopeTheta + 1f },
                config with { RmsNormEps = config.RmsNormEps * 2f }
            };

            foreach (var mismatchedConfig in mismatchedConfigs)
            {
                var policy = new LlmModel(mismatchedConfig);
                var reference = new LlmModel(mismatchedConfig);
                var policyBefore = SnapshotCheckpointState(policy);
                var referenceBefore = SnapshotCheckpointState(reference);
                var outputDirectory = Path.Combine(root, Guid.NewGuid().ToString("N"));
                var result = await ExecuteDpoAsync(
                    "run-logical-dpo-config-provenance-mismatch",
                    policy,
                    reference,
                    tokenizer,
                    outputDirectory,
                    loraConfig,
                    parentCheckpointPath,
                    CloneProvenance(
                        validProvenance,
                        parentConfig: mismatchedConfig));

                Assert.False(result.IsSuccess);
                Assert.Equal("DPO_PARENT_PROVENANCE_INVALID", result.ErrorCode);
                Assert.Equal(
                    "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                    result.ErrorMessage);
                AssertCheckpointStateEqual(policyBefore, policy);
                AssertCheckpointStateEqual(referenceBefore, reference);
                Assert.False(Directory.Exists(outputDirectory));
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RejectsParentLoraSemanticTamperingBeforeMutation()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var originalLora = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var tamperedLoraConfigs = new[]
        {
            originalLora with { Alpha = 8 },
            originalLora with { Dropout = 0.25f },
            originalLora with { TargetModules = ["q_proj"] },
            originalLora with { TargetModules = ["v_proj", "q_proj"] }
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-lora-provenance-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");

        try
        {
            Directory.CreateDirectory(root);
            var parent = new LlmModel(config);
            _ = LoraApplicator.Apply(parent, originalLora);
            parent.SaveCheckpoint(parentCheckpointPath);
            tokenizer.Save(parentTokenizerPath);
            var validProvenance = CreateParentProvenance(
                config,
                originalLora,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);

            foreach (var tamperedLora in tamperedLoraConfigs)
            {
                var policy = new LlmModel(config);
                var reference = new LlmModel(config);
                var policyBefore = SnapshotCheckpointState(policy);
                var referenceBefore = SnapshotCheckpointState(reference);
                var policyMutationBefore = CaptureDpoModelSnapshot(policy);
                var referenceMutationBefore = CaptureDpoModelSnapshot(reference);
                var outputDirectory = Path.Combine(root, Guid.NewGuid().ToString("N"));

                var result = await ExecuteDpoAsync(
                    "run-logical-dpo-lora-provenance-mismatch",
                    policy,
                    reference,
                    tokenizer,
                    outputDirectory,
                    // Keep runtime RNG admission valid; tamper the saved provenance,
                    // whose sealed identity must reject every semantic difference.
                    originalLora,
                    parentCheckpointPath,
                    CloneProvenance(
                        validProvenance,
                        parentLoraConfig: tamperedLora));

                Assert.False(result.IsSuccess);
                Assert.Equal("DPO_PARENT_PROVENANCE_INVALID", result.ErrorCode);
                Assert.Equal(
                    "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                    result.ErrorMessage);
                AssertCheckpointStateEqual(policyBefore, policy);
                AssertCheckpointStateEqual(referenceBefore, reference);
                AssertDpoModelSnapshot(policyMutationBefore, policy);
                AssertDpoModelSnapshot(referenceMutationBefore, reference);
                Assert.False(Directory.Exists(outputDirectory));
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_LoadsVerifiedHeldCheckpointWhenPathIsReplacedAfterHash()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-held-checkpoint-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var replacementCheckpointPath = Path.Combine(root, "replacement-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");
        var outputDirectory = Path.Combine(root, "output");

        try
        {
            Directory.CreateDirectory(root);
            var originalParent = new LlmModel(config);
            var originalAdapters = LoraApplicator.Apply(originalParent, loraConfig);
            FillParametersDeterministically(originalParent.BaseParameters, 0.125f);
            FillParametersDeterministically(
                originalAdapters.SelectMany(adapter => adapter.Parameters()),
                0.25f);
            originalParent.SaveCheckpoint(parentCheckpointPath);
            var originalState = SnapshotCheckpointState(originalParent);

            var replacementParent = new LlmModel(config);
            var replacementAdapters = LoraApplicator.Apply(replacementParent, loraConfig);
            FillParametersDeterministically(replacementParent.BaseParameters, 0.625f);
            FillParametersDeterministically(
                replacementAdapters.SelectMany(adapter => adapter.Parameters()),
                0.75f);
            replacementParent.SaveCheckpoint(replacementCheckpointPath);
            Assert.False(CheckpointStatesEqual(
                originalState,
                SnapshotCheckpointState(replacementParent)));

            tokenizer.Save(parentTokenizerPath);
            var provenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);
            var observer = new CheckpointSwapObserver(
                parentCheckpointPath,
                replacementCheckpointPath);
            var policy = new LlmModel(config);
            var reference = new LlmModel(config);

            var result = await ExecuteDpoAsync(
                "run-logical-dpo-held-checkpoint",
                policy,
                reference,
                tokenizer,
                outputDirectory,
                loraConfig,
                parentCheckpointPath,
                provenance,
                unit: new LogicalDpoTrainingUnit(observer));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(observer.WasInvoked);
            if (OperatingSystem.IsWindows())
                Assert.False(observer.ReplacementSucceeded);
            AssertCheckpointStateEqual(originalState, reference);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RejectsCallerLoraMutationAfterParentValidationBeforeModelMutation()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj"]
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-post-validation-lora-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");
        var outputDirectory = Path.Combine(root, "output");

        try
        {
            Directory.CreateDirectory(root);
            var parent = new LlmModel(config);
            _ = LoraApplicator.Apply(parent, loraConfig);
            parent.SaveCheckpoint(parentCheckpointPath);
            tokenizer.Save(parentTokenizerPath);
            var provenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);
            var policy = new LlmModel(config);
            var reference = new LlmModel(config);
            var policyBefore = SnapshotCheckpointState(policy);
            var referenceBefore = SnapshotCheckpointState(reference);
            var observer = new CallbackParentArtifactObserver(
                () => loraConfig.TargetModules[0] = "v_proj");

            var result = await ExecuteDpoAsync(
                "run-logical-dpo-post-validation-lora",
                policy,
                reference,
                tokenizer,
                outputDirectory,
                loraConfig,
                parentCheckpointPath,
                provenance,
                unit: new LogicalDpoTrainingUnit(observer));

            Assert.True(observer.WasInvoked);
            Assert.False(observer.IdentityRevalidatedWasInvoked);
            Assert.Equal("v_proj", Assert.Single(loraConfig.TargetModules));
            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PARENT_PROVENANCE_INVALID", result.ErrorCode);
            Assert.Equal(
                "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                result.ErrorMessage);
            AssertCheckpointStateEqual(policyBefore, policy);
            AssertCheckpointStateEqual(referenceBefore, reference);
            Assert.False(Directory.Exists(outputDirectory));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RejectsLiveTokenizerMutationAfterParentValidationBeforeModelMutation()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-post-validation-tokenizer-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");
        var outputDirectory = Path.Combine(root, "output");

        try
        {
            Directory.CreateDirectory(root);
            var parent = new LlmModel(config);
            _ = LoraApplicator.Apply(parent, loraConfig);
            parent.SaveCheckpoint(parentCheckpointPath);
            tokenizer.Save(parentTokenizerPath);
            var provenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);
            var tokenizerSha256Before = tokenizer.ComputeStateSha256();
            var policy = new LlmModel(config);
            var reference = new LlmModel(config);
            var policyBefore = SnapshotCheckpointState(policy);
            var referenceBefore = SnapshotCheckpointState(reference);
            var observer = new CallbackParentArtifactObserver(
                () => tokenizer.Train(
                    "post validation tokenizer mutation with different repeated pairs "
                    + "omega omega theta theta lambda lambda sigma sigma",
                    tokenizer.VocabSize + 32));

            var result = await ExecuteDpoAsync(
                "run-logical-dpo-post-validation-tokenizer",
                policy,
                reference,
                tokenizer,
                outputDirectory,
                loraConfig,
                parentCheckpointPath,
                provenance,
                unit: new LogicalDpoTrainingUnit(observer));

            Assert.True(observer.WasInvoked);
            Assert.False(observer.IdentityRevalidatedWasInvoked);
            Assert.NotEqual(tokenizerSha256Before, tokenizer.ComputeStateSha256());
            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PARENT_PROVENANCE_INVALID", result.ErrorCode);
            Assert.Equal(
                "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                result.ErrorMessage);
            AssertCheckpointStateEqual(policyBefore, policy);
            AssertCheckpointStateEqual(referenceBefore, reference);
            Assert.False(Directory.Exists(outputDirectory));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_UsesRetainedLoraAfterCallerMutationFollowingRevalidation()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj"]
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-retained-lora-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");
        var outputDirectory = Path.Combine(root, "output");

        try
        {
            Directory.CreateDirectory(root);
            var parent = new LlmModel(config);
            _ = LoraApplicator.Apply(parent, loraConfig);
            parent.SaveCheckpoint(parentCheckpointPath);
            var parentState = SnapshotCheckpointState(parent);
            tokenizer.Save(parentTokenizerPath);
            var provenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);
            var policy = new LlmModel(config);
            var reference = new LlmModel(config);
            var observer = new CallbackParentArtifactObserver(
                onIdentityRevalidated: () => loraConfig.TargetModules[0] = "v_proj");

            var result = await ExecuteDpoAsync(
                "run-logical-dpo-retained-lora",
                policy,
                reference,
                tokenizer,
                outputDirectory,
                loraConfig,
                parentCheckpointPath,
                provenance,
                unit: new LogicalDpoTrainingUnit(observer));

            Assert.True(observer.WasInvoked);
            Assert.True(observer.IdentityRevalidatedWasInvoked);
            Assert.Equal("v_proj", Assert.Single(loraConfig.TargetModules));
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, result.Value!.Output.DpoStepCount);
            AssertCheckpointStateEqual(parentState, reference);
            Assert.Contains(
                reference.GetCheckpointParameterEntries(),
                entry => entry.Key.Contains(".q_proj.lora_", StringComparison.Ordinal));
            Assert.DoesNotContain(
                reference.GetCheckpointParameterEntries(),
                entry => entry.Key.Contains(".v_proj.lora_", StringComparison.Ordinal));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_UsesRetainedTokenizerAfterCallerMutationFollowingRevalidation()
    {
        const string uniquePairPrompt =
            "aabacadaeafagahbbcbdbebfbgbhccdcecfcgchddedfdgdheefegehffgfhgghh";
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with
        {
            VocabSize = tokenizer.VocabSize,
            ContextLen = 128
        };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-retained-tokenizer-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");
        var outputDirectory = Path.Combine(root, "output");

        try
        {
            Directory.CreateDirectory(root);
            var parent = new LlmModel(config);
            _ = LoraApplicator.Apply(parent, loraConfig);
            parent.SaveCheckpoint(parentCheckpointPath);
            var parentState = SnapshotCheckpointState(parent);
            tokenizer.Save(parentTokenizerPath);
            var provenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);
            var policy = new LlmModel(config);
            var reference = new LlmModel(config);
            var observer = new CallbackParentArtifactObserver(
                onIdentityRevalidated: () => tokenizer.Train(
                    uniquePairPrompt,
                    tokenizer.VocabSize + 32));

            var result = await ExecuteDpoAsync(
                "run-logical-dpo-retained-tokenizer",
                policy,
                reference,
                tokenizer,
                outputDirectory,
                loraConfig,
                parentCheckpointPath,
                provenance,
                unit: new LogicalDpoTrainingUnit(observer),
                preferences:
                [
                    LogicalDpoPreference(
                        uniquePairPrompt,
                        "SECURITY",
                        "UI")
                ]);

            Assert.True(observer.WasInvoked);
            Assert.True(observer.IdentityRevalidatedWasInvoked);
            Assert.Contains(
                tokenizer.Encode(uniquePairPrompt),
                tokenId => tokenId >= config.VocabSize);
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, result.Value!.Output.DpoStepCount);
            AssertCheckpointStateEqual(parentState, reference);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_FailsClosedWhenPostRevalidationObserverThrows()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-post-revalidation-observer-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");
        var outputDirectory = Path.Combine(root, "output");

        try
        {
            Directory.CreateDirectory(root);
            var parent = new LlmModel(config);
            _ = LoraApplicator.Apply(parent, loraConfig);
            parent.SaveCheckpoint(parentCheckpointPath);
            tokenizer.Save(parentTokenizerPath);
            var provenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);
            var policy = new LlmModel(config);
            var reference = new LlmModel(config);
            var policyBefore = SnapshotCheckpointState(policy);
            var referenceBefore = SnapshotCheckpointState(reference);
            var observer = new CallbackParentArtifactObserver(
                onIdentityRevalidated: () => throw new InvalidOperationException(
                    "test-owned post-revalidation observer failure"));

            var result = await ExecuteDpoAsync(
                "run-logical-dpo-post-revalidation-observer-failure",
                policy,
                reference,
                tokenizer,
                outputDirectory,
                loraConfig,
                parentCheckpointPath,
                provenance,
                unit: new LogicalDpoTrainingUnit(observer));

            Assert.True(observer.WasInvoked);
            Assert.True(observer.IdentityRevalidatedWasInvoked);
            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PARENT_PROVENANCE_INVALID", result.ErrorCode);
            Assert.Equal(
                "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                result.ErrorMessage);
            AssertCheckpointStateEqual(policyBefore, policy);
            AssertCheckpointStateEqual(referenceBefore, reference);
            Assert.False(Directory.Exists(outputDirectory));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RejectsDifferentTokenizerStateWithSameVocabularyBeforeMutation()
    {
        var parentTokenizer = BuildTokenizer("alpha beta gamma delta epsilon security ui");
        var liveTokenizer = BuildTokenizer("omega theta lambda sigma kappa security ui");
        Assert.Equal(parentTokenizer.VocabSize, liveTokenizer.VocabSize);
        var config = BuildTinyConfig() with { VocabSize = parentTokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-tokenizer-provenance-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");
        var outputDirectory = Path.Combine(root, "output");

        try
        {
            Directory.CreateDirectory(root);
            var parent = new LlmModel(config);
            _ = LoraApplicator.Apply(parent, loraConfig);
            parent.SaveCheckpoint(parentCheckpointPath);
            parentTokenizer.Save(parentTokenizerPath);
            var provenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                parentTokenizer);
            var policy = new LlmModel(config);
            var policyBefore = SnapshotCheckpointState(policy);

            var result = await ExecuteDpoAsync(
                "run-logical-dpo-tokenizer-provenance-mismatch",
                policy,
                new LlmModel(config),
                liveTokenizer,
                outputDirectory,
                loraConfig,
                parentCheckpointPath,
                provenance);

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PARENT_PROVENANCE_INVALID", result.ErrorCode);
            AssertCheckpointStateEqual(policyBefore, policy);
            Assert.False(Directory.Exists(outputDirectory));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_RejectsMalformedOrMismatchedArtifactShaBeforeMutation()
    {
        var tokenizer = BuildTokenizer();
        var config = BuildTinyConfig() with { VocabSize = tokenizer.VocabSize };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-sha-provenance-{Guid.NewGuid():N}");
        var parentCheckpointPath = Path.Combine(root, "parent-policy.bin");
        var parentTokenizerPath = Path.Combine(root, "parent-tokenizer.json");

        try
        {
            Directory.CreateDirectory(root);
            var parent = new LlmModel(config);
            _ = LoraApplicator.Apply(parent, loraConfig);
            parent.SaveCheckpoint(parentCheckpointPath);
            tokenizer.Save(parentTokenizerPath);
            var validProvenance = CreateParentProvenance(
                config,
                loraConfig,
                parentCheckpointPath,
                parentTokenizerPath,
                tokenizer);
            var invalidProvenances = new[]
            {
                CloneProvenance(validProvenance, tokenizerSha256: "not-a-sha256"),
                CloneProvenance(validProvenance, policyCheckpointSha256: new string('0', 64))
            };

            foreach (var invalidProvenance in invalidProvenances)
            {
                var policy = new LlmModel(config);
                var policyBefore = SnapshotCheckpointState(policy);
                var outputDirectory = Path.Combine(root, Guid.NewGuid().ToString("N"));
                var result = await ExecuteDpoAsync(
                    "run-logical-dpo-invalid-sha-provenance",
                    policy,
                    new LlmModel(config),
                    tokenizer,
                    outputDirectory,
                    loraConfig,
                    parentCheckpointPath,
                    invalidProvenance);

                Assert.False(result.IsSuccess);
                Assert.Equal("DPO_PARENT_PROVENANCE_INVALID", result.ErrorCode);
                Assert.Equal(
                    "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                    result.ErrorMessage);
                AssertCheckpointStateEqual(policyBefore, policy);
                Assert.False(Directory.Exists(outputDirectory));
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_LeaseDeterministicallySerializesSamePolicySessions()
    {
        var config = BuildTinyConfig();
        var tokenizer = BuildTokenizer();
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var policy = new LlmModel(config);
        var observer = new DeterministicLeaseObserver();
        var firstCheckpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-exclusive-policy-first-{Guid.NewGuid():N}");
        var secondCheckpointDir = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-exclusive-policy-second-{Guid.NewGuid():N}");

        try
        {
            var firstTask = ExecuteDpoAsync(
                "run-logical-dpo-exclusive-policy-first",
                policy,
                new LlmModel(config),
                tokenizer,
                firstCheckpointDir,
                loraConfig,
                unit: new LogicalDpoTrainingUnit(observer));
            await observer.FirstAcquired.WaitAsync(TimeSpan.FromSeconds(5));

            var secondTask = ExecuteDpoAsync(
                "run-logical-dpo-exclusive-policy-second",
                policy,
                new LlmModel(config),
                tokenizer,
                secondCheckpointDir,
                loraConfig,
                unit: new LogicalDpoTrainingUnit(observer));
            await observer.SecondWaitStarted.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.False(observer.SubsequentAcquired.IsCompleted);
            observer.ReleaseFirst();
            var results = await Task.WhenAll(firstTask, secondTask);

            Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
            await observer.SubsequentAcquired.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            if (Directory.Exists(firstCheckpointDir))
                Directory.Delete(firstCheckpointDir, recursive: true);
            if (Directory.Exists(secondCheckpointDir))
                Directory.Delete(secondCheckpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_CancelledLeaseWaitDoesNotLeakOrOverReleasePermit()
    {
        var config = BuildTinyConfig();
        var tokenizer = BuildTokenizer();
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };
        var policy = new LlmModel(config);
        var observer = new DeterministicLeaseObserver();
        var root = Path.Combine(
            Path.GetTempPath(),
            $"logical-dpo-cancelled-lease-{Guid.NewGuid():N}");
        using var waitingCancellation = new CancellationTokenSource();

        try
        {
            var firstTask = ExecuteDpoAsync(
                "run-logical-dpo-cancelled-lease-holder",
                policy,
                new LlmModel(config),
                tokenizer,
                Path.Combine(root, "first"),
                loraConfig,
                unit: new LogicalDpoTrainingUnit(observer));
            await observer.FirstAcquired.WaitAsync(TimeSpan.FromSeconds(5));

            var cancelledTask = ExecuteDpoAsync(
                "run-logical-dpo-cancelled-lease-waiter",
                policy,
                new LlmModel(config),
                tokenizer,
                Path.Combine(root, "cancelled"),
                loraConfig,
                unit: new LogicalDpoTrainingUnit(observer),
                cancellationToken: waitingCancellation.Token);
            await observer.SecondWaitStarted.WaitAsync(TimeSpan.FromSeconds(5));
            waitingCancellation.Cancel();
            var cancelled = await cancelledTask;
            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);

            var subsequentTask = ExecuteDpoAsync(
                "run-logical-dpo-after-cancelled-lease",
                policy,
                new LlmModel(config),
                tokenizer,
                Path.Combine(root, "subsequent"),
                loraConfig,
                unit: new LogicalDpoTrainingUnit(observer));
            await observer.ThirdWaitStarted.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(observer.SubsequentAcquired.IsCompleted);

            observer.ReleaseFirst();
            var first = await firstTask;
            var subsequent = await subsequentTask;
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(subsequent.IsSuccess, subsequent.ErrorMessage);
            await observer.SubsequentAcquired.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_SeededFreshRequiresExplicitRandomAuthorization()
    {
        var policy = new LlmModel(BuildTinyConfig());
        var reference = new LlmModel(BuildTinyConfig());
        var beforePolicy = CaptureDpoModelSnapshot(policy);
        var beforeReference = CaptureDpoModelSnapshot(reference);
        var root = Path.Combine(Path.GetTempPath(), $"logical-dpo-missing-auth-{Guid.NewGuid():N}");
        try
        {
            var result = await ExecuteDpoAsync("missing-rng-authorization", policy, reference,
                BuildTokenizer(), root, CreateSeededLoraConfig(),
                adapterSeedAuthority: CreateSeedAuthority());
            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_RANDOM_STATE_COLD_START_AUTHORIZATION_REQUIRED", result.ErrorCode);
            AssertDpoModelSnapshot(beforePolicy, policy);
            AssertDpoModelSnapshot(beforeReference, reference);
            Assert.False(Directory.Exists(root));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_PreparedReopenedParentContinuesWithoutReinstallOrColdStart()
    {
        var root = Path.Combine(Path.GetTempPath(), $"logical-dpo-prepared-control-{Guid.NewGuid():N}");
        try
        {
            var fixture = CreateReopenedPreparedFixture(root);
            var policyBefore = CaptureDpoModelSnapshot(fixture.Policy);
            var referenceBefore = CaptureDpoModelSnapshot(fixture.Reference);
            var result = await ExecutePreparedAsync(fixture, Path.Combine(root, "next"));
            Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.Equal(2, result.Value!.Output.PostStepOptimizerState!.Step);
            Assert.Equal(2, result.Value.Output.PostStepRandomState!.RandomStateStep);
            Assert.True(fixture.RandomState.Entries.Zip(result.Value.Output.PostStepRandomState.Entries)
                .Any(pair => pair.Second.ConsumedCursor > pair.First.ConsumedCursor));
            Assert.Equal(1, result.Value.Output.DpoStepCount);
            Assert.Equal(policyBefore.ParameterReferences, fixture.Policy.Parameters().ToArray());
            Assert.Equal(referenceBefore.ParameterReferences, fixture.Reference.Parameters().ToArray());
            foreach (var projection in fixture.LoraConfig.TargetModules)
            {
                Assert.Contains(fixture.Policy.GetAttachedProjectionAdapter(0, projection), policyBefore.AttachedAdapters);
                Assert.Contains(fixture.Reference.GetAttachedProjectionAdapter(0, projection), referenceBefore.AttachedAdapters);
            }
            var referenceParameters = fixture.Reference.Parameters().ToArray();
            for (var index = 0; index < referenceParameters.Length; index++)
            {
                Assert.Equal(referenceBefore.Parameters[index].Values, referenceParameters[index].Value.Data);
                Assert.False(referenceParameters[index].Value.RequiresGrad);
                Assert.Null(referenceParameters[index].Value.Grad);
            }
            Assert.Equal(fixture.ParentSha256, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fixture.ParentPath))));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    [Theory]
    [InlineData("topology", "DPO_REFERENCE_PROVENANCE_MISMATCH")]
    [InlineData("keys", "DPO_REFERENCE_PROVENANCE_MISMATCH")]
    [InlineData("shape", "DPO_REFERENCE_PROVENANCE_MISMATCH")]
    [InlineData("length", "DPO_REFERENCE_PROVENANCE_MISMATCH")]
    [InlineData("parameter-same", "DPO_REFERENCE_STORAGE_SHARED")]
    [InlineData("parameter-cross", "DPO_REFERENCE_STORAGE_SHARED")]
    [InlineData("value-same", "DPO_REFERENCE_STORAGE_SHARED")]
    [InlineData("value-cross", "DPO_REFERENCE_STORAGE_SHARED")]
    [InlineData("storage-same", "DPO_REFERENCE_STORAGE_SHARED")]
    [InlineData("storage-cross", "DPO_REFERENCE_STORAGE_SHARED")]
    [InlineData("data-same", "DPO_REFERENCE_STORAGE_SHARED")]
    [InlineData("data-cross", "DPO_REFERENCE_STORAGE_SHARED")]
    [InlineData("seed", "DPO_LORA_APPLICATION_FAILED")]
    [InlineData("role", "DPO_LORA_APPLICATION_FAILED")]
    [InlineData("config", "DPO_LORA_APPLICATION_FAILED")]
    [InlineData("baseline", "DPO_REFERENCE_PROVENANCE_MISMATCH")]
    public async Task LogicalDpoTrainingUnit_PreparedRejectsMismatchWithoutMutationOrOutput(
        string fault, string expectedCode)
    {
        var root = Path.Combine(Path.GetTempPath(), $"logical-dpo-prepared-{fault}-{Guid.NewGuid():N}");
        try
        {
            var fixture = CreateReopenedPreparedFixture(root);
            var policy = fixture.Policy;
            var reference = fixture.Reference;
            Assert.NotSame(policy, reference);
            if (fault == "topology")
                LoraApplicator.Apply(reference, new LoraConfig
                { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["o_proj"] });
            else if (fault is "keys" or "seed" or "role" or "config")
            {
                reference = new LlmModel(BuildTinyConfig());
                var authority = fixture.Authority;
                var config = fault == "keys" ? fixture.LoraConfig with { TargetModules = ["k_proj", "v_proj"] }
                    : fault == "config" ? fixture.LoraConfig with { Alpha = 8 } : fixture.LoraConfig;
                LoraApplicator.ApplyFreshSeededPair(new LlmModel(BuildTinyConfig()), reference, config,
                    fault == "seed" ? authority.RootInitializationSeed + 1 : authority.RootInitializationSeed,
                    fault == "role" ? authority.ReferenceDropoutStreamDomain : authority.PolicyDropoutStreamDomain,
                    fault == "role" ? authority.PolicyDropoutStreamDomain : authority.ReferenceDropoutStreamDomain);
                if (fault != "keys") reference.LoadCheckpoint(fixture.ParentPath);
                fixture = fixture with { Reference = reference };
            }
            else if (fault == "baseline")
            {
                reference.Parameters().Last().Value.Data[0] += 0.125f;
                foreach (var model in new[] { policy, reference })
                foreach (var parameter in model.Parameters())
                {
                    parameter.Value.RequiresGrad = true;
                    parameter.Value.Grad = new Tensor(parameter.Value.Shape);
                    Array.Fill(parameter.Value.Grad.Data, 0.375f);
                }
            }
            else
            {
                var destination = reference.BaseParameters[0];
                if (fault == "shape")
                    SetFixtureBackingField(destination, "Value", new Tensor(destination.Value.Data,
                        new TensorShape(destination.Value.Data.Length)));
                else if (fault == "length")
                    SetFixtureBackingField(destination.Value.Storage, "Data", destination.Value.Data[..^1]);
                else
                {
                    // Pick distinct keys with equal dimensions: no earlier shape guard can mask sharing.
                    var pair = (from i in Enumerable.Range(0, policy.BaseParameters.Count)
                                from j in Enumerable.Range(0, policy.BaseParameters.Count)
                                where i != j && policy.BaseParameters[i].Value.Shape.Dimensions
                                    .SequenceEqual(policy.BaseParameters[j].Value.Shape.Dimensions)
                                select (i, j)).First();
                    var sourceIndex = fault.EndsWith("cross", StringComparison.Ordinal) ? pair.j : pair.i;
                    var source = policy.BaseParameters[sourceIndex];
                    destination = reference.BaseParameters[pair.i];
                    if (fault.StartsWith("parameter", StringComparison.Ordinal))
                    {
                        var field = typeof(LlmModel).GetField("_baseParameters", BindingFlags.Instance | BindingFlags.NonPublic)!;
                        ((Parameter[])field.GetValue(reference)!)[pair.i] = source;
                    }
                    else if (fault.StartsWith("value", StringComparison.Ordinal)) SetFixtureBackingField(destination, "Value", source.Value);
                    else if (fault.StartsWith("storage", StringComparison.Ordinal)) SetFixtureBackingField(destination.Value, "Storage", source.Value.Storage);
                    else SetFixtureBackingField(destination.Value.Storage, "Data", source.Value.Data);
                    Assert.Equal(source.Value.Shape.Dimensions, reference.BaseParameters[pair.i].Value.Shape.Dimensions);
                }
            }

            var policyBefore = CaptureDpoModelSnapshot(policy);
            var referenceBefore = CaptureDpoModelSnapshot(reference);
            var policyEntries = policy.GetCheckpointParameterEntries().ToArray();
            var referenceEntries = reference.GetCheckpointParameterEntries().ToArray();
            var rootFiles = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .ToDictionary(path => path, path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));
            var output = Path.Combine(root, "rejected-output");
            var result = await ExecutePreparedAsync(fixture, output);
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedCode, result.ErrorCode);
            AssertDpoModelSnapshot(policyBefore, policy);
            AssertDpoModelSnapshot(referenceBefore, reference);
            Assert.Equal(policyEntries, policy.GetCheckpointParameterEntries());
            Assert.Equal(referenceEntries, reference.GetCheckpointParameterEntries());
            Assert.False(Directory.Exists(output));
            Assert.Equal(rootFiles.Keys.Order(), Directory.GetFiles(root, "*", SearchOption.AllDirectories).Order());
            foreach (var file in rootFiles)
                Assert.Equal(file.Value, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file.Key))));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    [Theory]
    [InlineData("permit2", false)]
    [InlineData("permit2", true)]
    [InlineData("import-key", false)]
    [InlineData("import-key", true)]
    [InlineData("import-shape", false)]
    [InlineData("import-shape", true)]
    [InlineData("cancel2", false)]
    [InlineData("cancel2", true)]
    [InlineData("permit3", false)]
    [InlineData("permit3", true)]
    [InlineData("cancel3", false)]
    [InlineData("cancel3", true)]
    [InlineData("post-observer", false)]
    [InlineData("post-observer", true)]
    [InlineData("wrapped-permit2", false)]
    [InlineData("wrapped-permit2", true)]
    [InlineData("wrapped-observer", false)]
    [InlineData("wrapped-observer", true)]
    [InlineData("fatal-permit2", false)]
    [InlineData("fatal-observer", false)]
    public async Task LogicalDpoTrainingUnit_PreparedLateFailureRestoresBothLiveModelsBeforeHandoff(
        string fault, bool preexistingOutput)
    {
        var root = Path.Combine(Path.GetTempPath(), $"logical-dpo-late-{Guid.NewGuid():N}");
        var previousObserver = LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting;
        using var cancellation = new CancellationTokenSource();
        try
        {
            var fixture = CreateReopenedPreparedFixture(root);
            Assert.NotEmpty(fixture.OptimizerState.Entries);
            Assert.True(fixture.OptimizerState.Step > 0);
            Assert.Equal(fixture.OptimizerState.Step, fixture.RandomState.RandomStateStep);
            AssertCheckpointStateEqual(SnapshotCheckpointState(fixture.Policy), fixture.Reference);
            foreach (var model in new[] { fixture.Policy, fixture.Reference })
            {
                var index = 0;
                foreach (var parameter in model.Parameters())
                {
                    parameter.Value.RequiresGrad = index++ % 2 == 0;
                    parameter.Value.Grad = new Tensor(parameter.Value.Shape);
                    Array.Fill(parameter.Value.Grad.Data, index * 0.125f);
                }
            }
            var policyBefore = CaptureDpoModelSnapshot(fixture.Policy);
            var referenceBefore = CaptureDpoModelSnapshot(fixture.Reference);
            var policyRandomBefore = CaptureRandom(fixture.Policy);
            var referenceRandomBefore = CaptureRandom(fixture.Reference);
            Assert.NotEqual(JsonSerializer.Serialize(fixture.RandomState), policyRandomBefore);
            var output = Path.Combine(root, "next");
            if (preexistingOutput)
            {
                Directory.CreateDirectory(output);
                File.WriteAllText(Path.Combine(output, "sentinel.txt"), "preexisting-owner-content");
            }
            var filesBefore = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .ToDictionary(path => path, ComputeFileSha256);
            var optimizer = fixture.OptimizerState;
            if (fault == "import-key")
            {
                var entries = optimizer.Entries.ToArray();
                const string missingKey = "not-a-policy-parameter";
                Assert.DoesNotContain(fixture.Policy.GetCheckpointParameterEntries(), entry => entry.Key == missingKey);
                entries[0] = new AdamWPortableParameterState(missingKey,
                    entries[0].Dimensions.ToArray(),
                    entries[0].FirstMoment.ToArray(), entries[0].SecondMoment.ToArray());
                optimizer = new AdamWPortableState(optimizer.FormatVersion, optimizer.Step, entries);
            }
            else if (fault == "import-shape")
            {
                var entries = optimizer.Entries.ToArray();
                var index = Array.FindIndex(entries, e => e.Dimensions.Count == 2 && e.Dimensions[0] != e.Dimensions[1]);
                Assert.True(index >= 0);
                var original = entries[index];
                Assert.Equal(fixture.Policy.GetCheckpointParameterEntries().Single(e => e.Key == original.Key)
                    .Parameter.Value.Shape.Dimensions, original.Dimensions);
                entries[index] = new AdamWPortableParameterState(original.Key,
                    original.Dimensions.Reverse().ToArray(), original.FirstMoment.ToArray(), original.SecondMoment.ToArray());
                optimizer = AdamWPortableState.FromBytes(new AdamWPortableState(
                    optimizer.FormatVersion, optimizer.Step, entries).ToBytes())!;
                Assert.NotNull(optimizer);
                Assert.Equal(original.FirstMoment.Count, optimizer.Entries[index].FirstMoment.Count);
            }
            var permits = 0;
            var disposed = 0;
            var observerCalls = 0;
            var publications = 0;
            Exception injected = fault.StartsWith("fatal", StringComparison.Ordinal)
                ? new InvalidOperationException("fatal wrapper", new BadImageFormatException("synthetic fatal node"))
                : new AggregateException("wrapped cancellation", new OperationCanceledException(cancellation.Token));
            OperationResult<IDisposable> AcquirePermit()
            {
                permits++;
                Assert.InRange(permits, 1, 3); // A fourth permit would mean StepVector was entered.
                if (permits >= 2)
                    Assert.All(fixture.Reference.Parameters(), parameter =>
                    {
                        Assert.False(parameter.Value.RequiresGrad);
                        Assert.Null(parameter.Value.Grad);
                    });
                if (permits == 3)
                    Assert.Equal(JsonSerializer.Serialize(fixture.RandomState), CaptureRandom(fixture.Policy));
                if (permits == 2 && fault is "wrapped-permit2" or "fatal-permit2")
                    throw injected;
                if ((permits == 2 && fault == "cancel2") || (permits == 3 && fault == "cancel3"))
                {
                    cancellation.Cancel();
                    if (fault == "cancel2") throw new OperationCanceledException(cancellation.Token);
                }
                if ((permits == 2 && fault == "permit2") || (permits == 3 && fault == "permit3"))
                    return OperationResult<IDisposable>.Fail("late permit refusal", "LEGAL_TRAINING_ADMISSION_REVOKED");
                return OperationResult<IDisposable>.Ok(new CallbackDisposable(() => disposed++));
            }
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = (_, _) =>
            {
                observerCalls++;
                Assert.Contains(fault, new[] { "post-observer", "wrapped-observer", "fatal-observer" });
                Assert.Equal(3, permits);
                Assert.Equal(JsonSerializer.Serialize(fixture.RandomState), CaptureRandom(fixture.Policy));
                // Mutate the retained ORIGINAL buffers, including the reference's
                // now-detached gradients; cancelling alone would not detect aliasing.
                foreach (var snapshot in new[] { policyBefore, referenceBefore })
                foreach (var parameter in snapshot.Parameters)
                    Array.Fill(parameter.GradientReference!.Data, -17.25f);
                if (fault != "post-observer")
                {
                    foreach (var model in new[] { fixture.Policy, fixture.Reference })
                    {
                        model.Parameters().First().Value.Data[0] += 0.25f;
                        _ = model.Forward(new Tensor([1f, 2f, 3f], new TensorShape(1, 3)), training: true);
                    }
                    Assert.NotEqual(referenceRandomBefore, CaptureRandom(fixture.Reference));
                    throw injected;
                }
                cancellation.Cancel(); // Returns normally: the final cancellation guard must run.
            };
            Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> Execute() => ExecuteDpoAsync(
                "prepared-late-failure", fixture.Policy, fixture.Reference,
                BuildTokenizer(), output, fixture.LoraConfig, cancellationToken: cancellation.Token,
                acquireMutationPermit: AcquirePermit, adapterSeedAuthority: fixture.Authority,
                seededTopologyPreparedByRepositoryStart: true, optimizerStateToRestore: optimizer,
                randomStateToRestore: fixture.RandomState, persistPublicationBundle: _ => publications++);
            if (fault.StartsWith("wrapped", StringComparison.Ordinal) || fault.StartsWith("fatal", StringComparison.Ordinal))
                Assert.Same(injected, await Record.ExceptionAsync(async () => { await Execute(); }));
            else
            {
                var result = await Execute();
                Assert.False(result.IsSuccess);
                Assert.Equal(fault is "import-key" or "import-shape" ? "DPO_OPTIMIZER_STATE_RESTORE_FAILED"
                    : fault is "permit2" or "permit3" ? "LEGAL_TRAINING_ADMISSION_REVOKED" : "CANCELLED", result.ErrorCode);
            }
            Assert.Equal(fault is "permit2" or "import-key" or "import-shape" or "cancel2" or "wrapped-permit2" or "fatal-permit2" ? 2 : 3, permits);
            Assert.Equal(fault is "permit2" or "permit3" or "cancel2" or "wrapped-permit2" or "fatal-permit2" ? permits - 1 : permits, disposed);
            Assert.Equal(fault.EndsWith("observer", StringComparison.Ordinal) ? 1 : 0, observerCalls);
            Assert.Equal(0, publications);
            AssertDpoModelSnapshot(policyBefore, fixture.Policy);
            AssertDpoModelSnapshot(referenceBefore, fixture.Reference);
            Assert.Equal(policyRandomBefore, CaptureRandom(fixture.Policy));
            Assert.Equal(referenceRandomBefore, CaptureRandom(fixture.Reference));
            Assert.Equal(preexistingOutput, Directory.Exists(output));
            Assert.Equal(filesBefore.Keys.Order(), Directory.GetFiles(root, "*", SearchOption.AllDirectories).Order());
            foreach (var file in filesBefore) Assert.Equal(file.Value, ComputeFileSha256(file.Key));

            string CaptureRandom(LlmModel model) => JsonSerializer.Serialize(model.CaptureRandomState(
                fixture.RandomState.TopologySha256, fixture.RandomState.LoraConfigurationSha256,
                fixture.RandomState.RandomStateStep));
        }
        finally
        {
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = previousObserver;
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task LogicalDpoTrainingUnit_PreparedRollbackFailurePreservesPrimaryCauseAndRestoresOtherModel(
        bool cancel)
        => AssertPreparedRollbackFailureAsync(cancel, fatalCancellation: false);

    [Fact]
    public Task LogicalDpoTrainingUnit_PreparedRollbackFailurePropagatesDirectCancellationWithFatalInnerCause()
        => AssertPreparedRollbackFailureAsync(cancel: false, fatalCancellation: true);

    private static async Task AssertPreparedRollbackFailureAsync(bool cancel, bool fatalCancellation)
    {
        var root = Path.Combine(Path.GetTempPath(), $"logical-dpo-rollback-failure-{Guid.NewGuid():N}");
        var previousObserver = LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting;
        using var cancellation = new CancellationTokenSource();
        try
        {
            var fixture = CreateReopenedPreparedFixture(root);
            foreach (var model in new[] { fixture.Policy, fixture.Reference })
            foreach (var parameter in model.Parameters())
            {
                parameter.Value.RequiresGrad = true;
                parameter.Value.Grad = new Tensor(parameter.Value.Shape);
                Array.Fill(parameter.Value.Grad.Data, 0.375f);
            }
            var policyBefore = CaptureDpoModelSnapshot(fixture.Policy);
            var referenceBefore = CaptureDpoModelSnapshot(fixture.Reference);
            string Random(LlmModel model) => JsonSerializer.Serialize(model.CaptureRandomState(
                fixture.RandomState.TopologySha256, fixture.RandomState.LoraConfigurationSha256, 0));
            var policyRandomBefore = Random(fixture.Policy);
            var referenceRandomBefore = Random(fixture.Reference);
            var filesBefore = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .ToDictionary(path => path, ComputeFileSha256);
            var permits = 0;
            var disposed = 0;
            var observers = 0;
            var publications = 0;
            var output = Path.Combine(root, "next");
            var alteredShape = new TensorShape(fixture.Policy.BaseParameters[0].Value.Data.Length);
            var fatalCause = new BadImageFormatException("primary-fatal-preparation-cause");
            var primaryCancellation = new OperationCanceledException(
                "primary-late-preparation-cancellation", fatalCause, cancellation.Token);
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = (_, _) =>
            {
                observers++;
                Assert.Equal(3, permits);
                foreach (var snapshot in new[] { policyBefore, referenceBefore })
                foreach (var parameter in snapshot.Parameters)
                    Array.Fill(parameter.GradientReference!.Data, -11f);
                // Violate an immutable topology invariant through the existing
                // test-only seam. Production must report it, never rebuild/rebind.
                SetFixtureBackingField(fixture.Policy.BaseParameters[0].Value, "Shape", alteredShape);
                if (fatalCancellation)
                {
                    Assert.False(cancellation.IsCancellationRequested);
                    throw primaryCancellation;
                }
                if (cancel) cancellation.Cancel();
                else throw new IOException("primary-late-preparation-failure");
            };
            Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> Execute() =>
                ExecuteDpoAsync("prepared-rollback-failure", fixture.Policy, fixture.Reference,
                BuildTokenizer(), output, fixture.LoraConfig, cancellationToken: cancellation.Token,
                acquireMutationPermit: () =>
                {
                    Assert.InRange(++permits, 1, 3);
                    return OperationResult<IDisposable>.Ok(new CallbackDisposable(() => disposed++));
                }, adapterSeedAuthority: fixture.Authority, seededTopologyPreparedByRepositoryStart: true,
                optimizerStateToRestore: fixture.OptimizerState, randomStateToRestore: fixture.RandomState,
                persistPublicationBundle: _ => publications++);
            if (fatalCancellation)
            {
                var failure = await Assert.ThrowsAsync<AggregateException>(Execute);
                Assert.Contains("DPO_PREPARATION_ROLLBACK_FAILED", failure.Message);
                Assert.Same(primaryCancellation, failure.InnerExceptions[0]);
                Assert.Same(fatalCause, primaryCancellation.InnerException);
                Assert.Contains(failure.InnerExceptions.Skip(1),
                    rollback => rollback.ToString().Contains("DPO_PREPARATION_TOPOLOGY_ROLLBACK_FAILED", StringComparison.Ordinal));
                Assert.False(cancellation.IsCancellationRequested);
            }
            else
            {
                var result = await Execute();
                Assert.False(result.IsSuccess);
                Assert.Equal("DPO_PREPARATION_ROLLBACK_FAILED", result.ErrorCode);
                Assert.Contains(cancel ? nameof(OperationCanceledException) : "primary-late-preparation-failure", result.ErrorMessage);
                Assert.Contains("DPO_PREPARATION_TOPOLOGY_ROLLBACK_FAILED", result.ErrorMessage);
            }
            Assert.Equal(3, permits);
            Assert.Equal(3, disposed);
            Assert.Equal(1, observers);
            Assert.Equal(0, publications);
            AssertDpoModelSnapshot(referenceBefore, fixture.Reference);
            Assert.Same(alteredShape, fixture.Policy.BaseParameters[0].Value.Shape);
            // Only the deliberate unrecoverable shape violation remains; do not
            // pretend the whole policy topology was restored successfully.
            for (var index = 0; index < policyBefore.Parameters.Length; index++)
            {
                var before = policyBefore.Parameters[index];
                var parameter = fixture.Policy.Parameters().ElementAt(index);
                Assert.Same(policyBefore.ParameterReferences[index], parameter);
                Assert.Same(before.ValueReference, parameter.Value);
                Assert.Same(before.StorageReference, parameter.Value.Storage);
                Assert.Same(before.DataReference, parameter.Value.Data);
                Assert.Equal(before.Values.Select(BitConverter.SingleToInt32Bits), parameter.Value.Data.Select(BitConverter.SingleToInt32Bits));
                Assert.Equal(before.RequiresGrad, parameter.Value.RequiresGrad);
                Assert.Same(before.GradientReference, parameter.Value.Grad);
                Assert.Same(before.GradientStorage, parameter.Value.Grad!.Storage);
                Assert.Same(before.GradientData, parameter.Value.Grad.Data);
                Assert.Equal(before.GradientValues, parameter.Value.Grad.Data);
            }
            Assert.Equal(policyRandomBefore, Random(fixture.Policy));
            Assert.Equal(referenceRandomBefore, Random(fixture.Reference));
            Assert.False(Directory.Exists(output));
            Assert.Equal(filesBefore.Keys.Order(), Directory.GetFiles(root, "*", SearchOption.AllDirectories).Order());
            foreach (var file in filesBefore) Assert.Equal(file.Value, ComputeFileSha256(file.Key));
        }
        finally
        {
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = previousObserver;
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    // Test-only fault injection into immutable storage; no production API is widened.
    private static void SetFixtureBackingField(object target, string property, object value)
    {
        var field = target.GetType().GetField($"<{property}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field.SetValue(target, value);
    }

    private sealed record PreparedDpoFixture(LlmModel Policy, LlmModel Reference, LoraConfig LoraConfig,
        LogicalDpoAdapterSeedAuthority Authority, AdamWPortableState OptimizerState,
        LlmModelRandomStateSnapshot RandomState, string ParentPath, string ParentSha256);

    private sealed record PreparedFixtureIdentity(string PolicySha256, long PolicyLength,
        string OptimizerSha256, string RandomSha256, long RandomLength, string Topology, string Configuration, int Step);

    private static PreparedDpoFixture CreateReopenedPreparedFixture(string root)
    {
        Directory.CreateDirectory(root);
        var config = BuildTinyConfig();
        var lora = CreateSeededLoraConfig();
        var authority = CreateSeedAuthority();
        var pairAuthority = new LoraPairRandomSeedAuthority(authority.RootInitializationSeed,
            authority.PolicyDropoutStreamDomain, authority.ReferenceDropoutStreamDomain);
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(config, lora, pairAuthority);
        var configuration = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(lora, pairAuthority);
        var original = new LlmModel(config);
        var originalReference = new LlmModel(config);
        LoraApplicator.ApplyFreshSeededPair(original, originalReference, lora, authority.RootInitializationSeed,
            authority.PolicyDropoutStreamDomain, authority.ReferenceDropoutStreamDomain);
        var trainer = global::DpoTrainerOptimizerStateHandoffTests.CreateSeededTrainer(
            original, originalReference, root, topology, configuration);
        trainer.Step(global::DpoTrainerOptimizerStateHandoffTests.CreateSinglePairBatch(), 1);
        var parentPath = Path.Combine(root, DpoTrainer.BuildPolicyCheckpointFileName(1));
        var optimizer = RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(root, 1, trainer.ExportOptimizerState());
        Assert.True(optimizer.IsSuccess, optimizer.ErrorMessage);
        var randomPath = Path.Combine(root, RepositoryOwnedDpoRandomStateFile.BuildFileName(1));
        var random = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(randomPath,
            original.CaptureRandomState(topology, configuration, 1));
        Assert.True(random.IsSuccess, random.ErrorMessage);
        var identityPath = Path.Combine(root, "prepared-identities.json");
        File.WriteAllText(identityPath, JsonSerializer.Serialize(new PreparedFixtureIdentity(
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(parentPath))), new FileInfo(parentPath).Length,
            optimizer.Value!.OptimizerStateSha256, random.Value!.Sha256, random.Value.ByteLength,
            topology, configuration, 1)));

        // New model instances and disk-only continuation inputs, including expected identities.
        var identity = JsonSerializer.Deserialize<PreparedFixtureIdentity>(File.ReadAllText(identityPath))!;
        Assert.Equal(identity.PolicySha256, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(parentPath))));
        Assert.Equal(identity.PolicyLength, new FileInfo(parentPath).Length);
        var reopenedOptimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(root, identity.Step, identity.OptimizerSha256);
        var reopenedRandom = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(randomPath, identity.RandomSha256,
            identity.RandomLength, identity.Step, identity.Topology, identity.Configuration);
        Assert.True(reopenedOptimizer.IsSuccess, reopenedOptimizer.ErrorMessage);
        Assert.True(reopenedRandom.IsSuccess, reopenedRandom.ErrorMessage);
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        LoraApplicator.ApplyFreshSeededPair(policy, reference, lora, authority.RootInitializationSeed,
            authority.PolicyDropoutStreamDomain, authority.ReferenceDropoutStreamDomain);
        policy.LoadCheckpoint(parentPath);
        reference.LoadCheckpoint(parentPath);
        Assert.True(policy.Parameters().Count() > policy.BaseParameters.Count);
        Assert.True(reference.Parameters().Count() > reference.BaseParameters.Count);
        Assert.Equal(1, reopenedOptimizer.Value!.State.Step);
        return new PreparedDpoFixture(policy, reference, lora, authority,
            reopenedOptimizer.Value.State, reopenedRandom.Value!, parentPath, identity.PolicySha256);
    }

    private static Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> ExecutePreparedAsync(
        PreparedDpoFixture fixture, string output)
        => ExecuteDpoAsync("prepared-reopened", fixture.Policy, fixture.Reference, BuildTokenizer(), output,
            fixture.LoraConfig, adapterSeedAuthority: fixture.Authority,
            seededTopologyPreparedByRepositoryStart: true, optimizerStateToRestore: fixture.OptimizerState,
            randomStateToRestore: fixture.RandomState,
            persistPublicationBundle: bundle =>
            {
                // Match the repository executor's owning publication route, including
                // the callback that hands the captured cursor back to the logical result.
                Assert.NotNull(bundle.RandomState);
                var saved = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                    Path.Combine(output, RepositoryOwnedDpoRandomStateFile.BuildFileName(bundle.RandomState.RandomStateStep)),
                    bundle.RandomState);
                Assert.True(saved.IsSuccess, saved.ErrorMessage);
            });

    private static DpoRandomStateColdStartAuthorization CreateFreshRandomAuthorization()
        => new("test:explicit-fresh", "controlled initial seeded fixture", CreateSeedAuthority().RootInitializationSeed);

    private static LogicalDpoAdapterSeedAuthority CreateSeedAuthority()
    {
        var context = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentRandomContext();
        return new LogicalDpoAdapterSeedAuthority(
            context.RootInitializationSeed,
            context.PolicyDropoutStreamDomain,
            context.ReferenceDropoutStreamDomain);
    }

    private static LoraConfig CreateSeededLoraConfig()
        => new()
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0.1f,
            TargetModules = ["q_proj", "v_proj"]
        };

    private static DpoModelSnapshot CaptureDpoModelSnapshot(LlmModel model)
        => new(
            model.Parameters().ToArray(),
            model.Parameters()
                .Select(parameter => new DpoParameterSnapshot(
                    parameter.Value.Data.ToArray(),
                    parameter.Value.RequiresGrad,
                    parameter.Value.Grad,
                    parameter.Value.Grad?.Data.ToArray(),
                    parameter.Value, parameter.Value.Storage, parameter.Value.Data,
                    parameter.Value.Shape.Dimensions.ToArray(),
                    parameter.Value.Grad?.Storage, parameter.Value.Grad?.Data))
                .ToArray(),
            Enumerable.Range(0, model.Blocks.Count)
                .SelectMany(block => new[]
                {
                    model.GetAttachedProjectionAdapter(block, "q_proj"),
                    model.GetAttachedProjectionAdapter(block, "k_proj"),
                    model.GetAttachedProjectionAdapter(block, "v_proj"),
                    model.GetAttachedProjectionAdapter(block, "o_proj")
                })
                .Cast<object?>()
                .ToArray());

    private static void AssertDpoModelSnapshot(
        DpoModelSnapshot expected,
        LlmModel actual)
    {
        var actualParameters = actual.Parameters().ToArray();
        Assert.Equal(expected.ParameterReferences.Length, actualParameters.Length);
        for (var index = 0; index < actualParameters.Length; index++)
        {
            var parameter = actualParameters[index];
            var expectedParameter = expected.Parameters[index];
            Assert.Same(expected.ParameterReferences[index], parameter);
            Assert.Same(expectedParameter.ValueReference, parameter.Value);
            Assert.Same(expectedParameter.StorageReference, parameter.Value.Storage);
            Assert.Same(expectedParameter.DataReference, parameter.Value.Data);
            Assert.Equal(expectedParameter.Shape, parameter.Value.Shape.Dimensions);
            Assert.Equal(expectedParameter.Values.Select(BitConverter.SingleToInt32Bits),
                parameter.Value.Data.Select(BitConverter.SingleToInt32Bits));
            Assert.Same(expectedParameter.GradientStorage, parameter.Value.Grad?.Storage);
            Assert.Same(expectedParameter.GradientData, parameter.Value.Grad?.Data);
            Assert.Equal(expectedParameter.GradientValues?.Select(BitConverter.SingleToInt32Bits),
                parameter.Value.Grad?.Data.Select(BitConverter.SingleToInt32Bits));
            Assert.Equal(expectedParameter.Values, parameter.Value.Data);
            Assert.Equal(
                expectedParameter.RequiresGrad,
                parameter.Value.RequiresGrad);
            Assert.Same(expectedParameter.GradientReference, parameter.Value.Grad);
            Assert.Equal(
                expectedParameter.GradientValues,
                parameter.Value.Grad?.Data);
        }

        var actualAdapters = Enumerable.Range(0, actual.Blocks.Count)
            .SelectMany(block => new[]
            {
                actual.GetAttachedProjectionAdapter(block, "q_proj"),
                actual.GetAttachedProjectionAdapter(block, "k_proj"),
                actual.GetAttachedProjectionAdapter(block, "v_proj"),
                actual.GetAttachedProjectionAdapter(block, "o_proj")
            })
            .Cast<object?>()
            .ToArray();
        Assert.Equal(expected.AttachedAdapters.Length, actualAdapters.Length);
        for (var index = 0; index < actualAdapters.Length; index++)
            Assert.Same(expected.AttachedAdapters[index], actualAdapters[index]);
    }

    private static void AssertFreshUnfrozen(LlmModel model)
    {
        Assert.Equal(model.BaseParameters.Count, model.Parameters().Count());
        Assert.All(model.BaseParameters, parameter =>
        {
            Assert.True(parameter.Value.RequiresGrad);
            Assert.Null(parameter.Value.Grad);
        });
        for (var block = 0; block < model.Blocks.Count; block++)
        {
            Assert.Null(model.GetAttachedProjectionAdapter(block, "q_proj"));
            Assert.Null(model.GetAttachedProjectionAdapter(block, "k_proj"));
            Assert.Null(model.GetAttachedProjectionAdapter(block, "v_proj"));
            Assert.Null(model.GetAttachedProjectionAdapter(block, "o_proj"));
        }
    }

    private static Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> ExecuteDpoAsync(
        string runId,
        LlmModel policy,
        LlmModel reference,
        BpeTokenizer tokenizer,
        string checkpointDirectory,
        LoraConfig loraConfig,
        string? parentPolicyCheckpointPath = null,
        LogicalDpoParentProvenance? parentProvenance = null,
        int maxStepCount = 1,
        LogicalDpoTrainingUnit? unit = null,
        CancellationToken cancellationToken = default,
        LogicalDpoPreferenceExample[]? preferences = null,
        float convergenceThreshold = 0.002f,
        int convergenceWindowSteps = 25,
        Func<OperationResult<IDisposable>>? acquireMutationPermit = null,
        LogicalDpoAdapterSeedAuthority? adapterSeedAuthority = null,
        bool seededTopologyPreparedByRepositoryStart = false,
        AdamWPortableState? optimizerStateToRestore = null,
        LlmModelRandomStateSnapshot? randomStateToRestore = null,
        DpoRandomStateColdStartAuthorization? randomStateColdStartAuthorization = null,
        Action<DpoTrainingPublicationBundle>? persistPublicationBundle = null)
    {
        var ownedPreferences = preferences
            ?? [LogicalDpoPreference(
                "Classify the prompt.", "SECURITY", "UI")];
        var prepared = LogicalDpoTrainingUnit.PrepareExecution(
            ownedPreferences,
            tokenizer);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var input = new LogicalDpoTrainingRequest(
            policy,
            reference,
            tokenizer,
            checkpointDirectory,
            ownedPreferences,
            loraConfig,
            parentPolicyCheckpointPath,
            MaxStepCount: maxStepCount,
            ConvergenceThreshold: convergenceThreshold,
            ConvergenceWindowSteps: convergenceWindowSteps,
            ParentProvenance: parentProvenance,
            AdmissionCapabilities: IssueAdmissionCapabilities(
                acquireMutationPermit
                ?? (static () => OperationResult<IDisposable>.Ok(
                    new CancellationTokenSource()))),
            OptimizerStateToRestore: optimizerStateToRestore,
            PersistPublicationBundle: persistPublicationBundle)
        {
            PreparedExecution = prepared.Value,
            AdapterSeedAuthority = adapterSeedAuthority,
            SeededTopologyPreparedByRepositoryStart = seededTopologyPreparedByRepositoryStart,
            RandomStateToRestore = randomStateToRestore,
            RandomStateColdStartAuthorization = randomStateColdStartAuthorization
        };
        return (unit ?? new LogicalDpoTrainingUnit()).ExecuteAsync(
            new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                runId,
                LogicalDpoTrainingUnit.Id,
                LogicalDpoTrainingUnit.Role,
                LogicalDpoTrainingUnit.DomainTag,
                input,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
            cancellationToken);
    }

    private static LogicalDpoPreferenceExample LogicalDpoPreference(
        string prompt,
        string chosen,
        string rejected,
        float[]? teachingVector = null,
        DpoPairSideAuthorKind chosenAuthorKind =
            DpoPairSideAuthorKind.DeterministicGenerator,
        DpoPairSideAuthorKind rejectedAuthorKind =
            DpoPairSideAuthorKind.DeterministicGenerator)
        => new(prompt, chosen, rejected, teachingVector)
        {
            ChosenAuthorKind = chosenAuthorKind,
            RejectedAuthorKind = rejectedAuthorKind
        };

    private static string FindRepositoryRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var current = new DirectoryInfo(start);
            while (current is not null)
            {
                if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                    return current.FullName;
                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException(
            "Repository root with LlmFromScratch.sln was not found.");
    }

    private static LogicalDpoParentProvenance CreateParentProvenance(
        ModelConfig config,
        LoraConfig loraConfig,
        string policyCheckpointPath,
        string tokenizerPath,
        BpeTokenizer tokenizer)
    {
        var tokenizerSha256 = tokenizer.ComputeStateSha256();
        var checkpointSha256 = ComputeFileSha256(policyCheckpointPath);
        var runtimeProvenance = RuntimeProvenance.ComputeSha256(
            config,
            loraConfig,
            tokenizerSha256,
            checkpointSha256);
        Assert.True(runtimeProvenance.IsSuccess, runtimeProvenance.ErrorMessage);
        return new LogicalDpoParentProvenance(
            config,
            loraConfig,
            tokenizerPath,
            tokenizerSha256,
            checkpointSha256,
            runtimeProvenance.Value!);
    }

    private static LogicalDpoParentProvenance CloneProvenance(
        LogicalDpoParentProvenance source,
        ModelConfig? parentConfig = null,
        LoraConfig? parentLoraConfig = null,
        string? tokenizerSha256 = null,
        string? policyCheckpointSha256 = null,
        string? runtimeProvenanceSha256 = null)
        => new(
            parentConfig ?? source.ParentConfig,
            parentLoraConfig ?? source.ParentLoraConfig,
            source.ParentTokenizerPath,
            tokenizerSha256 ?? source.TokenizerSha256,
            policyCheckpointSha256 ?? source.PolicyCheckpointSha256,
            runtimeProvenanceSha256 ?? source.RuntimeProvenanceSha256);

    private static string ComputeFileSha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static int[][] SnapshotParameterBits(IEnumerable<Parameter> parameters)
        => parameters
            .Select(parameter => parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();

    private static CheckpointState SnapshotCheckpointState(LlmModel model)
    {
        var entries = model.GetCheckpointParameterEntries();
        return new CheckpointState(
            entries.Select(entry => entry.Key).ToArray(),
            SnapshotParameterBits(entries.Select(entry => entry.Parameter)));
    }

    private static void AssertCheckpointStateEqual(
        CheckpointState expected,
        LlmModel actual)
    {
        var actualState = SnapshotCheckpointState(actual);
        Assert.Equal(expected.Keys, actualState.Keys);
        Assert.Equal(expected.ParameterBits.Length, actualState.ParameterBits.Length);
        for (var index = 0; index < expected.ParameterBits.Length; index++)
            Assert.Equal(expected.ParameterBits[index], actualState.ParameterBits[index]);
    }

    private static bool CheckpointStatesEqual(
        CheckpointState expected,
        CheckpointState actual)
    {
        if (!expected.Keys.SequenceEqual(actual.Keys)
            || expected.ParameterBits.Length != actual.ParameterBits.Length)
        {
            return false;
        }

        for (var index = 0; index < expected.ParameterBits.Length; index++)
        {
            if (!expected.ParameterBits[index].SequenceEqual(actual.ParameterBits[index]))
                return false;
        }

        return true;
    }

    private static void AssertParameterBitsEqual(
        IReadOnlyList<int[]> expected,
        IEnumerable<Parameter> actual)
    {
        var actualBits = SnapshotParameterBits(actual);
        Assert.Equal(expected.Count, actualBits.Length);
        for (var index = 0; index < expected.Count; index++)
            Assert.Equal(expected[index], actualBits[index]);
    }

    private static void FillParametersDeterministically(
        IEnumerable<Parameter> parameters,
        float seed)
    {
        var parameterIndex = 0;
        foreach (var parameter in parameters)
        {
            for (var valueIndex = 0; valueIndex < parameter.Value.Data.Length; valueIndex++)
                parameter.Value.Data[valueIndex] = seed + (parameterIndex * 0.001f) + (valueIndex * 0.00001f);
            parameterIndex++;
        }
    }

    private sealed record CheckpointState(
        string[] Keys,
        int[][] ParameterBits);

    private sealed record DpoModelSnapshot(
        Parameter[] ParameterReferences,
        DpoParameterSnapshot[] Parameters,
        object?[] AttachedAdapters);

    private sealed record DpoParameterSnapshot(
        float[] Values,
        bool RequiresGrad,
        Tensor? GradientReference,
        float[]? GradientValues,
        Tensor ValueReference, TensorStorage StorageReference, float[] DataReference,
        int[] Shape, TensorStorage? GradientStorage, float[]? GradientData);

    private sealed class CallbackDisposable(Action onDispose) : IDisposable
    {
        private Action? _onDispose = onDispose;

        public void Dispose()
            => Interlocked.Exchange(ref _onDispose, null)?.Invoke();
    }

    private static LogicalCapabilityTrainingAdmissionCapabilities
        IssueAdmissionCapabilities(
            Func<OperationResult<IDisposable>> acquireMutationPermit)
    {
        var lease = new TestDpoMutationAdmissionLease(acquireMutationPermit);
        var logicalMutationAdmission =
            LogicalDpoMutationAdmissionCapability.Issue(lease);
        Assert.True(
            logicalMutationAdmission.IsSuccess,
            logicalMutationAdmission.ErrorMessage);
        var executableManifestAttestation =
            LogicalCapabilityExecutableManifestAttestationCapability.Issue(
                lease);
        Assert.True(
            executableManifestAttestation.IsSuccess,
            executableManifestAttestation.ErrorMessage);
        return new LogicalCapabilityTrainingAdmissionCapabilities(
            logicalMutationAdmission.Value!,
            DpoMutationAdmissionCapability.Issue(lease),
            executableManifestAttestation.Value!);
    }

    private sealed class TestDpoMutationAdmissionLease(
        Func<OperationResult<IDisposable>> acquireMutationPermit)
        : ILogicalCapabilityTrainingAdmissionLease,
          ILogicalDpoMutationAdmissionIssuer,
          IDpoMutationAdmissionIssuer,
          ILogicalCapabilityExecutableManifestAttestationIssuer
    {
        public OperationResult<IDisposable> AcquireMutationPermit()
            => acquireMutationPermit();

        OperationResult<IDisposable>
            ILogicalDpoMutationAdmissionIssuer.AcquireDpoMutationPermit()
            => AcquireMutationPermit();

        IDisposable IDpoMutationAdmissionIssuer
            .AcquireDpoTrainerMutationPermit()
        {
            var permit = AcquireMutationPermit();
            if (!permit.IsSuccess || permit.Value is null)
            {
                throw new DpoMutationAdmissionException(
                    permit.ErrorMessage,
                    permit.ErrorCode);
            }

            return permit.Value;
        }

        OperationResult<LogicalCapabilityExecutableManifestAttestation>
            ILogicalCapabilityExecutableManifestAttestationIssuer
                .SealExecutableManifest(
                    string executableManifestSha256,
                    LogicalCapabilityTrainingReplayAttestation
                        replayAttestation)
            => OperationResult<
                LogicalCapabilityExecutableManifestAttestation>.Fail(
                "Executable manifest sealing is unavailable in this test.",
                LogicalCapabilityExecutableManifestAttestationCapability
                    .RequiredErrorCode);

        public void Dispose()
        {
        }
    }

    private sealed class CheckpointSwapObserver : ILogicalDpoParentArtifactObserver
    {
        private readonly string _targetPath;
        private readonly string _replacementPath;

        internal CheckpointSwapObserver(string targetPath, string replacementPath)
        {
            _targetPath = targetPath;
            _replacementPath = replacementPath;
        }

        internal bool WasInvoked { get; private set; }
        internal bool ReplacementSucceeded { get; private set; }

        public void OnParentArtifactsVerified()
        {
            WasInvoked = true;
            try
            {
                File.Move(_replacementPath, _targetPath, overwrite: true);
                ReplacementSucceeded = true;
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException)
            {
                ReplacementSucceeded = false;
            }
        }

        public void OnParentCallerIdentityRevalidated()
        {
        }
    }

    private sealed class CallbackParentArtifactObserver : ILogicalDpoParentArtifactObserver
    {
        private readonly Action? _onArtifactsVerified;
        private readonly Action? _onIdentityRevalidated;

        internal CallbackParentArtifactObserver(
            Action? onArtifactsVerified = null,
            Action? onIdentityRevalidated = null)
        {
            _onArtifactsVerified = onArtifactsVerified;
            _onIdentityRevalidated = onIdentityRevalidated;
        }

        internal bool WasInvoked { get; private set; }
        internal bool IdentityRevalidatedWasInvoked { get; private set; }

        public void OnParentArtifactsVerified()
        {
            WasInvoked = true;
            _onArtifactsVerified?.Invoke();
        }

        public void OnParentCallerIdentityRevalidated()
        {
            IdentityRevalidatedWasInvoked = true;
            _onIdentityRevalidated?.Invoke();
        }
    }

    private sealed class DeterministicLeaseObserver : ILogicalDpoPolicyLeaseObserver
    {
        private readonly TaskCompletionSource<bool> _firstAcquired =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _secondWaitStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _thirdWaitStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _releaseFirst =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _subsequentAcquired =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _blockedWaitCount;
        private int _acquiredCount;

        public Task FirstAcquired => _firstAcquired.Task;
        public Task SecondWaitStarted => _secondWaitStarted.Task;
        public Task ThirdWaitStarted => _thirdWaitStarted.Task;
        public Task SubsequentAcquired => _subsequentAcquired.Task;

        public void OnWaitStarted(LlmModel policy)
        {
            _ = policy;
        }

        public void OnWaitBlocked(LlmModel policy)
        {
            _ = policy;
            var blockedWaitCount = Interlocked.Increment(ref _blockedWaitCount);
            if (blockedWaitCount == 1)
                _secondWaitStarted.TrySetResult(true);
            else if (blockedWaitCount == 2)
                _thirdWaitStarted.TrySetResult(true);
        }

        public Task OnLeaseAcquiredAsync(
            LlmModel policy,
            CancellationToken cancellationToken)
        {
            _ = policy;
            var acquiredCount = Interlocked.Increment(ref _acquiredCount);
            if (acquiredCount == 1)
            {
                _firstAcquired.TrySetResult(true);
                return _releaseFirst.Task.WaitAsync(cancellationToken);
            }

            _subsequentAcquired.TrySetResult(true);
            return Task.CompletedTask;
        }

        public void ReleaseFirst() => _releaseFirst.TrySetResult(true);
    }

    private static BpeTokenizer BuildTokenizer(string? distinguishingText = null)
    {
        // Train a tiny byte-pair tokenizer with enough vocabulary headroom for merged tokens.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            $"{distinguishingText ?? string.Empty} jwt oauth token authorization tenant layout screen typography button design classify prompt security ui figma export failure frame returned no data auth",
            300);
        return tokenizer;
    }

    private static ModelConfig BuildTinyConfig()
    {
        // Keep the model tiny so the test proves wiring without stressing the PC.
        return new ModelConfig
        {
            VocabSize = 512,
            ContextLen = 64,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };
    }
}
