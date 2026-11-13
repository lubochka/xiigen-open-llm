// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LlmFromScratch.Tests;

// Этот guard читает живой TSV лестницы и исходники тестовых проектов: второй
// канон в коде не появляется, а любые новые классы проходят через snapshot-review.
internal static class TestSuiteBatchManifest
{
    // Это число является договором с живой TSV-лестницей: когда добавляем
    // человеческую группу, guard обязан требовать новый счёт строк manifest.
    // Текущий счёт: 31 именованная группа и одна остаточная страховочная строка.
    internal const int CanonicalBatchCount = 33;
    // Лестница групп тестов как живой файл и правило классификации
    // (внутренний номер 249) остаётся логическим договором поверх физических
    // строк запускателя: проверка держит оба слоя, чтобы уточнения порций
    // тестов и диагностических групп не стёрли исходную лестницу из
    // шестнадцати плановых групп.
    internal const int PlannedLogicalBatchCount = 16;
    internal const int LogicalRuleCount = 7;
    internal const string LegacyWeakBaselineBodySha256 = "bf61271266a0ba29510763c634fffcc3fa41706b21bdd5d644bcfe97df5fe9bc";
    internal const string BatchHeader =
        "# order\tbatch_id\thuman_name\tdll\tfilter\tmin_rows\tsplit_on_repeat_timeout\tplatform\tplatform_reason";
    internal const string LogicalLadderVersion = "test-suite-batch-ladder-v1";
    internal const string LeafModeOverrideHeader = "# batch_id\tleaf_mode\thuman_reason";
    internal const string SnapshotBodyHeader = "dll\tclass_fqn\tsource_path\tbatch_id";
    private const string SnapshotVersion = "test-suite-batch-legacy-weak-snapshot-v2";
    private const string CanonicalSnapshotNewLine = "\n";
    private const string SemanticCompilerSearchVectorBatchId = "semantic-compiler-search-vector";
    private const string DesignConversionLiveLocalServerMethodDiagnosticBatchId = "design-conversion-live-local-server-method-diagnostic";
    private const string DesignConversionDpoStorageBatchId = "design-conversion-dpo-storage";
    private const string OrchestrationRoutingProductBoundariesBatchId = "orchestration-routing-product-boundaries";
    private const string TestInfrastructureEvidenceMeasurementBatchId = "test-infrastructure-evidence-measurement";
    private const string ServerOwnedReplayManifestAttestationBatchId = "server-owned-replay-manifest-attestation";
    private const string Phase08CommonUserTwoLegLifecycleBatchId = "phase08-common-user-two-leg-lifecycle";
    // Идентификатор закрепляет отдельный маршрут только за singleton-проверками
    // общих объектов потоков и узлов, а не за соседними классами из того же файла.
    private const string SingletonFlowNodeIdentityBatchId = "singleton-flow-node-identity";
    // Эта строка отделяет структуру потоков и иерархию исправления результата:
    // она не смешивает их с обучением модели или большой остаточной корзиной.
    private const string LogicalTopologyAndResultGapBatchId = "logical-topology-and-result-gap";
    // Запись обезличенных обучающих пар и исправлений проверяется отдельно:
    // маршрут не запускает обучение модели и не доказывает качество обучения.
    private const string ResultGapTrainingPairWriterBatchId = "result-gap-training-pair-writer";
    // Выполнение одного варианта графа с отменой и ограничением времени держится
    // отдельно от записи пар, чтобы при падении было понятно, что сломалось
    // выполнение графа, а не запись пары.
    private const string SingleCombinationGraphRunnerBatchId = "single-combination-graph-runner";
    // Защищённый каталог сохраняемых моделей проверяется отдельно: переносимые
    // строки должны запускаться, а Windows-only строки учитываются честно.
    private const string RetainedDomainModelRegistryBatchId = "retained-domain-model-registry";
    private const string ModelActivationBatchId = "model-activation";
    // Эти широкие строки запускалка проверяет по первому совпадению раньше
    // остатка. Две точные группы пары/графа обязаны стоять перед ними, иначе
    // слово "Orchestration" в имени метода будет исключено до точного имени
    // класса.
    private static readonly ImmutableArray<string> PairGraphBroadCollisionBatchIds =
    [
        "dynamic-graph",
        "teaching-translation-dpo",
        "orchestration-skills-wiki"
    ];

    private static readonly string[] AllowedPlatforms = ["any", "windows", "linux", "macos"];
    private static readonly ImmutableArray<string> SemanticCompilerSearchVectorClassFqns =
    [
        "LlmFromScratch.Tests.OssEquivalentsTests",
        "LlmFromScratch.Tests.OssEquivalentsV4Tests",
        "LlmFromScratch.Tests.PatternFusionTests",
        "LlmFromScratch.Tests.RoslynCodeAnalyzerTests",
        "LlmFromScratch.Tests.SearchEngineTests",
        "LlmFromScratch.Tests.SemanticCompiler29Tests",
        "LlmFromScratch.Tests.SemanticCompilerTests",
        "LlmFromScratch.Tests.SemanticNlpTests",
        "LlmFromScratch.Tests.VectorSearchEdgeCaseTests",
        "LlmFromScratch.Tests.VectorSearchTests"
    ];
    // Этот медленный серверный класс без shared fixture вынесен в отдельную
    // строку: один метод = один leaf, а похожие классы остаются вне маршрута.
    private static readonly ImmutableArray<string> DesignConversionLiveLocalServerMethodDiagnosticClassFqns =
    [
        "DesignConversionLiveLocalServerTests"
    ];
    private static readonly ImmutableArray<string> OrchestrationRoutingProductBoundaryClassFqns =
    [
        "LlmFromScratch.Tests.MultiSubPhaseRouterLearnedAuthorityTests",
        "LlmFromScratch.Tests.OrchestratorModelTests",
        "LlmFromScratch.Tests.PluginBoundaryTests",
        "LlmFromScratch.Tests.PromptRoutingInputCompilerUnitTests",
        "LlmFromScratch.Tests.ReflectionWiringTests",
        "LlmFromScratch.Tests.SolutionProductProjectRegistrationTests",
        "ThinClientApiContractTests"
    ];
    private static readonly ImmutableArray<string> TestInfrastructureEvidenceMeasurementClassFqns =
    [
        "LlmFromScratch.Tests.CrossStackFlowSeedHonestyTests",
        "LlmFromScratch.Tests.MeasurementNoisePolicyTests",
        "LlmFromScratch.Tests.TestOwnedOutputRootTests",
        "LlmFromScratch.Tests.TestSuiteBatchInventoryGuardTests",
        "LlmFromScratch.Tests.WorkLeftRagEvidenceIndexTests"
    ];
    private static readonly ImmutableArray<string> ServerOwnedReplayManifestAttestationClassFqns =
    [
        "LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTests"
    ];
    private static readonly ImmutableArray<string> Phase08CommonUserTwoLegLifecycleClassFqns =
    [
        "LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTests"
    ];
    // В этой группе намеренно один класс: он проверяет повторное построение
    // схемы работы и отсутствие повторной регистрации, а не всю соседнюю область.
    private static readonly ImmutableArray<string> SingletonFlowNodeIdentityClassFqns =
    [
        "LlmFromScratch.Tests.SingletonIdentityTests"
    ];
    private static readonly ImmutableArray<string> LogicalTopologyAndResultGapClassFqns =
    [
        "LlmFromScratch.Tests.TopologyMutationFlowTests",
        "ResultGapLogicalHierarchyTests"
    ];
    private static readonly ImmutableArray<string> ResultGapTrainingPairWriterClassFqns =
    [
        "ResultGapPhase07DpoWriterTests"
    ];
    private static readonly ImmutableArray<string> SingleCombinationGraphRunnerClassFqns =
    [
        "SingleCombinationGraphCandidateRunnerTests"
    ];
    private static readonly ImmutableArray<string> RetainedDomainModelRegistryClassFqns =
    [
        "RetainedDomainModelRegistryTests"
    ];
    private static readonly ImmutableArray<string> ModelActivationClassFqns =
    [
        "DesignConversionActivatedModelRepositoryHostRuntimeTests",
        "DesignConversionExternalVisualAiReceiptTrustTests",
        "DesignConversionHumanApprovalTests",
        "DesignConversionImprovementLoopSemanticsTests",
        "DesignConversionModelActivationRegistryTests",
        "DesignConversionModelActivationRouteRuntimeTests"
    ];
    private static readonly ImmutableArray<AtomicBatchGroup> AtomicGroups =
    [
        new(
            "локальный узел конверсии дизайна",
            "design-conversion-local-host",
            [
                "AutonomousPromotionEvaluationComposition",
                "DesignConversionLiveLocalBootstrapCheckpoint",
                "DesignConversionLiveLocalServer",
                "DesignConversionModelServerAspNetHost",
                "DesignConversionModelServerArtifactUploadAspNetEndpointMapper",
                "DesignConversionModelServerDpoUsefulness",
                "DesignConversionModelServerTrainingIntake",
                "DesignConversionModelServerExecutableHost",
                "DesignConversionModelServerFlow38Quarantine",
                "DesignConversionPromotion"
            ]),
        new(
            "порождающие процессы тесты",
            "process-spawning-tests",
            [
                "DesignConversionBrowserQaProvider",
                "DesignConversionBrowserRenderQaProbeIntegration",
                "DesignConversionModelServerImprovementLoopBrowserIntegration",
                "XiigenExtensionApiContracts",
                "ExtensionQuality03ProofContractRunner",
                "ExtensionTokenQualityRunnerHarness"
            ]),
        new(
            "артефакты качества токенов расширения",
            "extension-token-quality",
            [
                "ExtensionAdaptiveEffectGap02",
                "ExtensionQualitySemanticGates",
                "ExtensionToken04QualityScoringArtifacts",
                "SkillSelectionPhase06TrainedSelectorEvidence",
                "SmartSkillsSemanticQualityP17",
                "SmartSkillsSemanticQualityP23",
                "SmartSkillsSemanticQualityP24",
                "SmartSkillsSemanticQualityP25"
            ]),
        new(
            "окружение непрерывного обучения модельного сервера",
            "model-server-continuous-learning",
            [
                "ContinuousLearningBaseBootstrapTool",
                "ContinuousLearningModelServerReadinessEndpoint",
                "TranslationDpoModelServerCoordinator"
            ]),
        new(
            "публикация Project Wiki",
            "single-serial-groups",
            ["DefaultLogicalCapabilityTrainer"]),
        new(
            "snapshot артефактов репозитория",
            "single-serial-groups",
            ["DesignConversionTrainingPipeline"]),
        new(
            "локальный сервер динамического графа",
            "single-serial-groups",
            ["DynamicGraphCompositionLiveLocalServer"]),
        new(
            "окружение ProductHost",
            "single-serial-groups",
            ["ProductHostCapabilityServing"])
    ];

    internal static string FindRepositoryRoot(string? startDirectory = null)
    {
        var current = new DirectoryInfo(startDirectory ?? AppContext.BaseDirectory);
        while (current is not null)
        {
            var hasBatches = File.Exists(Path.Combine(current.FullName, "tools", "test-batches", "batches.tsv"));
            var hasTests = File.Exists(Path.Combine(current.FullName, "Tests", "LlmFromScratch.Tests.csproj"));
            if (hasBatches && hasTests)
                return current.FullName;

            current = current.Parent;
        }

        throw new InvalidOperationException("Repository root with tools/test-batches/batches.tsv was not found.");
    }

    internal static TestSuiteBatchRepositoryInventory CreateRepositoryInventory(string repoRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repoRoot);
        var projects = DiscoverRunnableTestProjects(repoRoot)
            .Select(project => project with { SourceFiles = project.SourceFiles.ToImmutableArray() })
            .ToImmutableArray();
        var classes = DiscoverTestClasses(repoRoot, projects).ToImmutableArray();

        return new TestSuiteBatchRepositoryInventory(projects, classes);
    }

    internal static TestSuiteBatchGuardReport ValidateRepository(
        string repoRoot,
        string? batchesPath = null,
        string? snapshotPath = null,
        TestSuiteBatchGuardOptions? options = null,
        string? legacyWeakBaselinePath = null,
        string? leafModeOverridesPath = null)
        => ValidateRepository(
            repoRoot,
            CreateRepositoryInventory(repoRoot),
            batchesPath,
            snapshotPath,
            options,
            legacyWeakBaselinePath,
            leafModeOverridesPath);

    internal static TestSuiteBatchGuardReport ValidateRepository(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string? batchesPath = null,
        string? snapshotPath = null,
        TestSuiteBatchGuardOptions? options = null,
        string? legacyWeakBaselinePath = null,
        string? leafModeOverridesPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repoRoot);
        ArgumentNullException.ThrowIfNull(inventory);
        options ??= TestSuiteBatchGuardOptions.Canonical;
        batchesPath ??= Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
        snapshotPath ??= Path.Combine(repoRoot, "Tests", "TestSuiteBatchLegacySnapshot.txt");
        legacyWeakBaselinePath ??= Path.Combine(repoRoot, "Tests", "TestSuiteBatchLegacyWeakBaseline.txt");
        leafModeOverridesPath ??= Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv");

        var problems = new List<string>();
        var projects = inventory.Projects;
        var classes = inventory.Classes;
        var manifest = ReadBatchRows(batchesPath, requireCanonicalFilters: true);
        problems.AddRange(manifest.Problems);
        if (options.RequireLogicalLadderMetadata)
        {
            var logicalLadder = ReadLogicalLadderMetadata(batchesPath);
            problems.AddRange(logicalLadder.Problems);
            problems.AddRange(ValidateLogicalLadderMetadata(logicalLadder, manifest.Rows));
        }

        if (options.RequireLeafModeOverrides)
        {
            var overrides = ReadLeafModeOverrides(leafModeOverridesPath, manifest.Rows);
            problems.AddRange(overrides.Problems);
            problems.AddRange(ValidateDesignConversionLiveLocalServerMethodDiagnosticLeafModeOverride(overrides.Rows));
            problems.AddRange(ValidateDesignConversionDpoStorageLeafModeOverride(overrides.Rows));
            problems.AddRange(ValidateTestInfrastructureEvidenceMeasurementLeafModeOverride(overrides.Rows));
        }

        var assignments = ValidateManifestAgainstInventory(
            rows: manifest.Rows,
            projects: projects,
            classes: classes,
            options: options,
            problems: problems);

        if (options.RequireSnapshot)
        {
            // Активный snapshot теперь хранит только слабые классы: те, которые
            // Слабый снимок хранит только классы, которые дошли до
            // wildcard-остатка без явного FullyQualifiedName~ сигнала.
            // Так новый тест не может спрятаться в общей корзине без review.
            var snapshot = ReadLegacySnapshot(snapshotPath);
            problems.AddRange(snapshot.Problems);
            var currentRows = CreateSnapshotRows(assignments);
            var currentWeakRows = CreateWeakSnapshotRows(assignments);
            if (options.RequireWeakBaseline)
            {
                var baseline = ReadLegacySnapshot(legacyWeakBaselinePath);
                problems.AddRange(baseline.Problems);
                problems.AddRange(ValidateLegacyWeakBaselineAnchor(baseline, options.RequiredLegacyWeakBaselineBodySha256));
                problems.AddRange(CompareWeakSnapshot(snapshot.Rows, baseline.Rows, currentWeakRows, currentRows, manifest.Rows));
            }
            else
            {
                problems.AddRange(CompareSnapshot(snapshot.Rows, currentWeakRows));
            }
        }

        return new TestSuiteBatchGuardReport(
            projects,
            classes,
            manifest.Rows,
            assignments,
            problems.OrderBy(problem => problem, StringComparer.Ordinal).ToArray());
    }

    internal static TestSuiteBatchReadResult ReadBatchRows(string path, bool requireCanonicalFilters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var rows = new List<TestSuiteBatchRow>();
        var problems = new List<string>();
        if (!File.Exists(path))
            return new TestSuiteBatchReadResult([], [$"batch_manifest_missing:{NormalizePathForSnapshot(path)}"]);

        var lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length == 0)
            return new TestSuiteBatchReadResult([], ["batch_manifest_empty"]);

        if (TrimUtf8Bom(lines[0]) != BatchHeader)
            problems.Add("batch_manifest_header_mismatch");

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var expectedOrder = 1;
        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var line = TrimUtf8Bom(lines[index]);
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            var parts = line.Split('\t');
            if (parts.Length != 9)
            {
                problems.Add($"batch_manifest_schema_line:{lineNumber}:columns={parts.Length}");
                continue;
            }

            var orderText = parts[0];
            if (!int.TryParse(orderText, out var order) || order != expectedOrder)
                problems.Add($"batch_manifest_order_gap_line:{lineNumber}:expected={expectedOrder:00}:actual={orderText}");

            expectedOrder++;
            var batchId = parts[1];
            if (string.IsNullOrWhiteSpace(batchId))
                problems.Add($"batch_manifest_empty_batch_id_line:{lineNumber}");
            else if (!seenIds.Add(batchId))
                problems.Add($"batch_manifest_duplicate_batch_id:{batchId}");

            if (!int.TryParse(parts[5], out var minRows) || minRows < 1)
                problems.Add($"batch_manifest_min_rows_invalid:{batchId}:{parts[5]}");

            if (!bool.TryParse(parts[6], out _))
                problems.Add($"batch_manifest_split_flag_invalid:{batchId}:{parts[6]}");

            if (!AllowedPlatforms.Contains(parts[7], StringComparer.Ordinal))
                problems.Add($"batch_manifest_platform_invalid:{batchId}:{parts[7]}");

            if (string.IsNullOrWhiteSpace(parts[2]) || string.IsNullOrWhiteSpace(parts[3]) ||
                string.IsNullOrWhiteSpace(parts[4]) || string.IsNullOrWhiteSpace(parts[8]))
                problems.Add($"batch_manifest_required_field_blank:{batchId}");

            if (requireCanonicalFilters && parts[4].Contains("FullyQualifiedName=", StringComparison.Ordinal))
                problems.Add($"canonical_exact_method_filter_rejected:{batchId}");

            problems.AddRange(ValidateRunnerCompatibleFilter(parts[4], batchId));
            rows.Add(new TestSuiteBatchRow(
                Order: orderText,
                BatchId: batchId,
                HumanName: parts[2],
                Dll: NormalizeManifestPath(parts[3]),
                Filter: parts[4],
                MinRows: minRows,
                SplitOnRepeatTimeout: parts[6],
                Platform: parts[7],
                PlatformReason: parts[8],
                LineNumber: lineNumber));
        }

        return new TestSuiteBatchReadResult(rows, problems);
    }

    internal static TestSuiteBatchLogicalLadderReadResult ReadLogicalLadderMetadata(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            return new TestSuiteBatchLogicalLadderReadResult(
                Version: string.Empty,
                DeclaredPhysicalRowCount: null,
                DeclaredLogicalBatchCount: null,
                DeclaredRuleCount: null,
                AssignmentIdentity: string.Empty,
                FirstMatchRule: string.Empty,
                NewTestAuthorObligation: string.Empty,
                Rules: [],
                LogicalBatches: [],
                Problems: [$"logical_ladder_manifest_missing:{NormalizePathForSnapshot(path)}"]);

        var lines = File.ReadAllLines(path, Encoding.UTF8);
        var rules = new List<TestSuiteBatchLogicalRuleRow>();
        var logicalBatches = new List<TestSuiteBatchLogicalBatchRow>();
        var problems = new List<string>();
        string version = string.Empty;
        string assignmentIdentity = string.Empty;
        string firstMatchRule = string.Empty;
        string newTestAuthorObligation = string.Empty;
        int? declaredPhysicalRowCount = null;
        int? declaredLogicalBatchCount = null;
        int? declaredRuleCount = null;

        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var line = TrimUtf8Bom(lines[index]);
            if (!line.StartsWith("# ", StringComparison.Ordinal))
                continue;

            var parts = line.Split('\t');
            switch (parts[0])
            {
                case "# ladder_manifest_version":
                    version = ReadSingleLogicalMetadataValue(parts, lineNumber, "ladder_manifest_version", problems);
                    break;
                case "# ladder_physical_row_count":
                    declaredPhysicalRowCount = ReadNonNegativeLogicalMetadataInt(parts, lineNumber, "ladder_physical_row_count", problems);
                    break;
                case "# ladder_logical_batch_count":
                    declaredLogicalBatchCount = ReadNonNegativeLogicalMetadataInt(parts, lineNumber, "ladder_logical_batch_count", problems);
                    break;
                case "# ladder_rule_count":
                    declaredRuleCount = ReadNonNegativeLogicalMetadataInt(parts, lineNumber, "ladder_rule_count", problems);
                    break;
                case "# ladder_assignment_identity":
                    assignmentIdentity = ReadSingleLogicalMetadataValue(parts, lineNumber, "ladder_assignment_identity", problems);
                    break;
                case "# ladder_first_match_rule":
                    firstMatchRule = ReadSingleLogicalMetadataValue(parts, lineNumber, "ladder_first_match_rule", problems);
                    break;
                case "# new_test_author_obligation":
                    newTestAuthorObligation = ReadSingleLogicalMetadataValue(parts, lineNumber, "new_test_author_obligation", problems);
                    break;
                case "# logical_rule":
                    if (parts.Length != 4)
                    {
                        problems.Add($"logical_ladder_rule_schema_line:{lineNumber}:columns={parts.Length}");
                        break;
                    }

                    // Строки правил хранят порядок семи шагов прямо в живом
                    // файле; код проверяет сам договор, а не скрытую копию
                    // каждой русской формулировки.
                    rules.Add(new TestSuiteBatchLogicalRuleRow(
                        parts[1],
                        parts[2],
                        parts[3],
                        lineNumber));
                    break;
                case "# logical_batch":
                    if (parts.Length != 6)
                    {
                        problems.Add($"logical_ladder_batch_schema_line:{lineNumber}:columns={parts.Length}");
                        break;
                    }

                    var physicalBatchIds = parts[4]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToImmutableArray();
                    logicalBatches.Add(new TestSuiteBatchLogicalBatchRow(
                        parts[1],
                        parts[2],
                        parts[3],
                        physicalBatchIds,
                        parts[5],
                        lineNumber));
                    break;
            }
        }

        return new TestSuiteBatchLogicalLadderReadResult(
            version,
            declaredPhysicalRowCount,
            declaredLogicalBatchCount,
            declaredRuleCount,
            assignmentIdentity,
            firstMatchRule,
            newTestAuthorObligation,
            rules,
            logicalBatches,
            problems);
    }

    private static string ReadSingleLogicalMetadataValue(
        IReadOnlyList<string> parts,
        int lineNumber,
        string key,
        ICollection<string> problems)
    {
        if (parts.Count != 2)
        {
            problems.Add($"logical_ladder_metadata_schema_line:{lineNumber}:key={key}:columns={parts.Count}");
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(parts[1]))
            problems.Add($"logical_ladder_metadata_blank:{key}:line={lineNumber}");

        return parts[1];
    }

    private static int? ReadNonNegativeLogicalMetadataInt(
        IReadOnlyList<string> parts,
        int lineNumber,
        string key,
        ICollection<string> problems)
    {
        var value = ReadSingleLogicalMetadataValue(parts, lineNumber, key, problems);
        if (int.TryParse(value, out var parsed) && parsed >= 0)
            return parsed;

        problems.Add($"logical_ladder_metadata_invalid_int:{key}:line={lineNumber}:actual={value}");
        return null;
    }

    private static IReadOnlyList<string> ValidateLogicalLadderMetadata(
        TestSuiteBatchLogicalLadderReadResult metadata,
        IReadOnlyList<TestSuiteBatchRow> physicalRows)
    {
        var problems = new List<string>();
        if (!string.Equals(metadata.Version, LogicalLadderVersion, StringComparison.Ordinal))
            problems.Add($"logical_ladder_version_mismatch:expected={LogicalLadderVersion}:actual={metadata.Version}");

        if (metadata.DeclaredPhysicalRowCount != physicalRows.Count)
            problems.Add($"logical_ladder_physical_row_count_mismatch:expected={physicalRows.Count}:actual={metadata.DeclaredPhysicalRowCount?.ToString() ?? "missing"}");

        if (metadata.DeclaredLogicalBatchCount != PlannedLogicalBatchCount)
            problems.Add($"logical_ladder_declared_batch_count_mismatch:expected={PlannedLogicalBatchCount}:actual={metadata.DeclaredLogicalBatchCount?.ToString() ?? "missing"}");

        if (metadata.DeclaredRuleCount != LogicalRuleCount)
            problems.Add($"logical_ladder_declared_rule_count_mismatch:expected={LogicalRuleCount}:actual={metadata.DeclaredRuleCount?.ToString() ?? "missing"}");

        if (string.IsNullOrWhiteSpace(metadata.AssignmentIdentity))
            problems.Add("logical_ladder_assignment_identity_missing");

        if (string.IsNullOrWhiteSpace(metadata.FirstMatchRule))
            problems.Add("logical_ladder_first_match_rule_missing");

        if (string.IsNullOrWhiteSpace(metadata.NewTestAuthorObligation))
            problems.Add("logical_ladder_new_test_author_obligation_missing");

        problems.AddRange(ValidateLogicalRuleRows(metadata.Rules));
        problems.AddRange(ValidateLogicalBatchRows(metadata.LogicalBatches, physicalRows));
        return problems;
    }

    private static IReadOnlyList<string> ValidateLogicalRuleRows(
        IReadOnlyList<TestSuiteBatchLogicalRuleRow> rules)
    {
        var problems = new List<string>();
        if (rules.Count != LogicalRuleCount)
            problems.Add($"logical_ladder_rule_count_mismatch:expected={LogicalRuleCount}:actual={rules.Count}");

        var seenRuleIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < rules.Count; index++)
        {
            var rule = rules[index];
            var expectedOrder = (index + 1).ToString("00", System.Globalization.CultureInfo.InvariantCulture);
            if (!string.Equals(rule.Order, expectedOrder, StringComparison.Ordinal))
                problems.Add($"logical_ladder_rule_order_mismatch:line={rule.LineNumber}:expected={expectedOrder}:actual={rule.Order}");

            if (string.IsNullOrWhiteSpace(rule.RuleId) || !seenRuleIds.Add(rule.RuleId))
                problems.Add($"logical_ladder_rule_id_invalid_or_duplicate:line={rule.LineNumber}:actual={rule.RuleId}");

            if (string.IsNullOrWhiteSpace(rule.HumanRule))
                problems.Add($"logical_ladder_rule_text_missing:line={rule.LineNumber}:rule={rule.RuleId}");
        }

        return problems;
    }

    private static IReadOnlyList<string> ValidateLogicalBatchRows(
        IReadOnlyList<TestSuiteBatchLogicalBatchRow> logicalBatches,
        IReadOnlyList<TestSuiteBatchRow> physicalRows)
    {
        var problems = new List<string>();
        if (logicalBatches.Count != PlannedLogicalBatchCount)
            problems.Add($"logical_ladder_batch_count_mismatch:expected={PlannedLogicalBatchCount}:actual={logicalBatches.Count}");

        var seenLogicalIds = new HashSet<string>(StringComparer.Ordinal);
        var knownPhysicalIds = physicalRows
            .Select(row => row.BatchId)
            .ToHashSet(StringComparer.Ordinal);
        var mappedPhysicalIds = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < logicalBatches.Count; index++)
        {
            var logicalBatch = logicalBatches[index];
            var expectedOrder = (index + 1).ToString("00", System.Globalization.CultureInfo.InvariantCulture);
            if (!string.Equals(logicalBatch.Order, expectedOrder, StringComparison.Ordinal))
                problems.Add($"logical_ladder_batch_order_mismatch:line={logicalBatch.LineNumber}:expected={expectedOrder}:actual={logicalBatch.Order}");

            if (string.IsNullOrWhiteSpace(logicalBatch.LogicalBatchId) ||
                !seenLogicalIds.Add(logicalBatch.LogicalBatchId))
                problems.Add($"logical_ladder_batch_id_invalid_or_duplicate:line={logicalBatch.LineNumber}:actual={logicalBatch.LogicalBatchId}");

            if (string.IsNullOrWhiteSpace(logicalBatch.HumanName) ||
                string.IsNullOrWhiteSpace(logicalBatch.HumanStatus))
                problems.Add($"logical_ladder_batch_human_fields_missing:line={logicalBatch.LineNumber}:logical={logicalBatch.LogicalBatchId}");

            if (logicalBatch.PhysicalBatchIds.Count == 0)
                problems.Add($"logical_ladder_batch_has_no_physical_rows:line={logicalBatch.LineNumber}:logical={logicalBatch.LogicalBatchId}");

            foreach (var physicalBatchId in logicalBatch.PhysicalBatchIds)
            {
                if (!knownPhysicalIds.Contains(physicalBatchId))
                {
                    problems.Add($"logical_ladder_unknown_physical_batch:{physicalBatchId}:logical={logicalBatch.LogicalBatchId}:line={logicalBatch.LineNumber}");
                    continue;
                }

                if (mappedPhysicalIds.TryGetValue(physicalBatchId, out var previousLogicalBatch))
                    problems.Add($"logical_ladder_duplicate_physical_batch:{physicalBatchId}:first={previousLogicalBatch}:second={logicalBatch.LogicalBatchId}");
                else
                    mappedPhysicalIds.Add(physicalBatchId, logicalBatch.LogicalBatchId);
            }
        }

        foreach (var physicalRow in physicalRows.Where(row => !mappedPhysicalIds.ContainsKey(row.BatchId)))
            problems.Add($"logical_ladder_unmapped_physical_batch:{physicalRow.BatchId}");

        // Лестница групп тестов как живой файл и правило классификации
        // (внутренний номер 249) явно оставляет настольную Windows-сборку
        // видимой; логический слой обязан сохранить этот человеческий статус,
        // а не спрятать его под общими уточнениями среды выполнения.
        var desktopLogicalRows = logicalBatches
            .Where(batch => batch.PhysicalBatchIds.Contains("api-dashboard-wpf-windows", StringComparer.Ordinal))
            .ToArray();
        if (desktopLogicalRows.Length != 1)
        {
            problems.Add($"logical_ladder_windows_desktop_batch_missing_or_duplicate:count={desktopLogicalRows.Length}");
        }
        else
        {
            var desktopLogical = desktopLogicalRows[0];
            if (!string.Equals(desktopLogical.Order, "16", StringComparison.Ordinal))
                problems.Add($"logical_ladder_windows_desktop_wrong_logical_order:expected=16:actual={desktopLogical.Order}");

            if (!desktopLogical.HumanStatus.Contains("windows", StringComparison.OrdinalIgnoreCase))
                problems.Add("logical_ladder_windows_desktop_status_missing_windows_reason");
        }

        return problems;
    }

    internal static TestSuiteBatchLeafModeOverrideReadResult ReadLeafModeOverrides(
        string path,
        IReadOnlyList<TestSuiteBatchRow> batchRows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var rows = new List<TestSuiteBatchLeafModeOverrideRow>();
        var problems = new List<string>();
        if (!File.Exists(path))
            return new TestSuiteBatchLeafModeOverrideReadResult([], [$"leaf_mode_override_missing:{NormalizePathForSnapshot(path)}"]);

        var lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length == 0)
            return new TestSuiteBatchLeafModeOverrideReadResult([], ["leaf_mode_override_empty"]);

        if (TrimUtf8Bom(lines[0]) != LeafModeOverrideHeader)
            problems.Add("leaf_mode_override_header_mismatch");

        var knownBatchIds = batchRows
            .Select(row => row.BatchId)
            .ToHashSet(StringComparer.Ordinal);
        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 1; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var line = TrimUtf8Bom(lines[index]);
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            var parts = line.Split('\t');
            if (parts.Length != 3)
            {
                problems.Add($"leaf_mode_override_schema_line:{lineNumber}:columns={parts.Length}");
                continue;
            }

            var batchId = parts[0];
            var leafMode = parts[1];
            var humanReason = parts[2];
            if (string.IsNullOrWhiteSpace(batchId) ||
                string.IsNullOrWhiteSpace(leafMode) ||
                string.IsNullOrWhiteSpace(humanReason))
                problems.Add($"leaf_mode_override_required_field_blank_line:{lineNumber}");

            if (!seenIds.Add(batchId))
                problems.Add($"leaf_mode_override_duplicate_batch_id:{batchId}");

            if (!knownBatchIds.Contains(batchId))
                problems.Add($"leaf_mode_override_unknown_batch:{batchId}");

            if (!IsAllowedLeafMode(leafMode))
                problems.Add($"leaf_mode_override_unknown_mode:{batchId}:{leafMode}");
            else if (string.Equals(leafMode, "class-method-pack", StringComparison.Ordinal) &&
                     !string.Equals(batchId, DesignConversionDpoStorageBatchId, StringComparison.Ordinal))
                problems.Add($"leaf_mode_override_class_method_pack_wrong_batch:{batchId}");

            rows.Add(new TestSuiteBatchLeafModeOverrideRow(batchId, leafMode, humanReason, lineNumber));
        }

        return new TestSuiteBatchLeafModeOverrideReadResult(rows, problems);
    }

    internal static IReadOnlyList<TestSuiteBatchProject> DiscoverRunnableTestProjects(string repoRoot)
    {
        var normalizedRoot = Path.GetFullPath(repoRoot);
        // Опись берёт только те файлы проектов, которые входят в область описи:
        // сборку самого хранилища, а не любую копию проекта в рабочем дереве.
        var projects = EnumerateCandidateProjectFiles(normalizedRoot)
            .Where(path => !HasIgnoredPathSegment(path))
            .Select(path => TryReadTestProject(normalizedRoot, path))
            .Where(project => project is not null)
            .Select(project => project!)
            .OrderBy(project => project.Dll, StringComparer.Ordinal)
            .ToArray();

        return projects;
    }

    internal static IReadOnlyList<TestSuiteBatchClass> DiscoverTestClasses(
        string repoRoot,
        IReadOnlyList<TestSuiteBatchProject> projects)
    {
        var classes = new List<TestSuiteBatchClass>();
        foreach (var project in projects)
            classes.AddRange(DiscoverProjectTestClasses(repoRoot, project));

        return classes
            .OrderBy(testClass => testClass.Dll, StringComparer.Ordinal)
            .ThenBy(testClass => testClass.FullyQualifiedName, StringComparer.Ordinal)
            .ThenBy(testClass => testClass.SourcePath, StringComparer.Ordinal)
            .ToArray();
    }

    internal static IReadOnlyList<TestSuiteBatchSnapshotRow> CreateSnapshotRows(
        IEnumerable<TestSuiteBatchAssignment> assignments)
        => assignments
            .Select(assignment => new TestSuiteBatchSnapshotRow(
                assignment.TestClass.Dll,
                assignment.TestClass.FullyQualifiedName,
                assignment.TestClass.SourcePath,
                assignment.BatchId))
            .OrderBy(row => row.Dll, StringComparer.Ordinal)
            .ThenBy(row => row.ClassFqn, StringComparer.Ordinal)
            .ThenBy(row => row.SourcePath, StringComparer.Ordinal)
            .ThenBy(row => row.BatchId, StringComparer.Ordinal)
            .ToArray();

    internal static IReadOnlyList<TestSuiteBatchSnapshotRow> CreateWeakSnapshotRows(
        IEnumerable<TestSuiteBatchAssignment> assignments)
        => assignments
            .Where(assignment => !assignment.HasStrongBatchSignal)
            .Select(assignment => new TestSuiteBatchSnapshotRow(
                assignment.TestClass.Dll,
                assignment.TestClass.FullyQualifiedName,
                assignment.TestClass.SourcePath,
                assignment.BatchId))
            .OrderBy(row => row.Dll, StringComparer.Ordinal)
            .ThenBy(row => row.ClassFqn, StringComparer.Ordinal)
            .ThenBy(row => row.SourcePath, StringComparer.Ordinal)
            .ThenBy(row => row.BatchId, StringComparer.Ordinal)
            .ToArray();

    internal static string CreateSnapshotText(IEnumerable<TestSuiteBatchSnapshotRow> rows)
    {
        var orderedRows = rows
            .OrderBy(row => row.Dll, StringComparer.Ordinal)
            .ThenBy(row => row.ClassFqn, StringComparer.Ordinal)
            .ThenBy(row => row.SourcePath, StringComparer.Ordinal)
            .ThenBy(row => row.BatchId, StringComparer.Ordinal)
            .ToArray();
        var body = CreateSnapshotBody(orderedRows);
        var bodyHash = Sha256Lower(body);
        var builder = new StringBuilder();
        AppendSnapshotLine(builder, "# TestSuiteBatchLegacySnapshot");
        AppendSnapshotLine(builder, "# Назначение: активная опись только слабых тестовых классов без явного FullyQualifiedName~ сигнала.");
        AppendSnapshotLine(builder, "# Обновлять вручную только при сжатии: когда класс получил сильный маршрут, удалить его из активного снимка.");
        AppendSnapshotLine(builder, "# Новый слабый класс сюда не добавляют: сначала добавляют человеческий batch-сигнал в tools/test-batches/batches.tsv.");
        AppendSnapshotLine(builder, $"snapshot_version\t{SnapshotVersion}");
        AppendSnapshotLine(builder, $"total_classes\t{orderedRows.Length}");
        AppendSnapshotLine(builder, $"body_sha256\t{bodyHash}");
        builder.Append(body);
        return builder.ToString();
    }

    internal static TestSuiteBatchSnapshotReadResult ReadLegacySnapshot(string path)
    {
        var problems = new List<string>();
        if (!File.Exists(path))
            return new TestSuiteBatchSnapshotReadResult([], [$"legacy_snapshot_missing:{NormalizePathForSnapshot(path)}"], string.Empty);

        var lines = File.ReadAllLines(path, Encoding.UTF8)
            .Select(TrimUtf8Bom)
            .ToArray();
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var bodyStart = Array.FindIndex(lines, line => line == SnapshotBodyHeader);
        if (bodyStart < 0)
            return new TestSuiteBatchSnapshotReadResult([], ["legacy_snapshot_body_header_missing"], string.Empty);

        foreach (var line in lines.Take(bodyStart))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            var parts = line.Split('\t', 2);
            if (parts.Length == 2)
                values[parts[0]] = parts[1];
        }

        if (!values.TryGetValue("snapshot_version", out var version) || version != SnapshotVersion)
            problems.Add("legacy_snapshot_version_mismatch");

        var rows = new List<TestSuiteBatchSnapshotRow>();
        for (var index = bodyStart + 1; index < lines.Length; index++)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split('\t');
            if (parts.Length != 4)
            {
                problems.Add($"legacy_snapshot_schema_line:{index + 1}:columns={parts.Length}");
                continue;
            }

            rows.Add(new TestSuiteBatchSnapshotRow(
                NormalizeManifestPath(parts[0]),
                parts[1],
                NormalizeManifestPath(parts[2]),
                parts[3]));
        }

        if (!values.TryGetValue("total_classes", out var countText) ||
            !int.TryParse(countText, out var expectedCount) ||
            expectedCount != rows.Count)
            problems.Add($"legacy_snapshot_total_mismatch:header={countText}:body={rows.Count}");

        var actualBody = CreateSnapshotBody(rows);
        var actualHash = Sha256Lower(actualBody);
        if (!values.TryGetValue("body_sha256", out var expectedHash) ||
            !string.Equals(expectedHash, actualHash, StringComparison.Ordinal))
            problems.Add($"legacy_snapshot_hash_mismatch:header={expectedHash}:actual={actualHash}");

        return new TestSuiteBatchSnapshotReadResult(rows, problems, actualHash);
    }

    private static IReadOnlyList<string> ValidateLegacyWeakBaselineAnchor(
        TestSuiteBatchSnapshotReadResult baseline,
        string? requiredBodySha256)
    {
        if (string.IsNullOrWhiteSpace(requiredBodySha256))
            return [];

        if (string.Equals(baseline.BodySha256, requiredBodySha256, StringComparison.Ordinal))
            return [];

        // Baseline слабых классов можно только сжимать через active snapshot:
        // новый слабый класс обязан получить человеческий маршрут, а не запись
        // в "старый" список с пересчитанным внутренним hash.
        return
        [
            "legacy_weak_baseline_anchor_mismatch:" +
            $"expected={requiredBodySha256}:actual={baseline.BodySha256}:reason=baseline_is_immutable_add_new_class_to_batches_tsv_with_strong_signal"
        ];
    }

    private static IReadOnlyList<string> CompareSnapshot(
        IReadOnlyList<TestSuiteBatchSnapshotRow> expectedRows,
        IReadOnlyList<TestSuiteBatchSnapshotRow> actualRows)
    {
        var problems = new List<string>();
        var expected = new HashSet<string>(
            expectedRows.Select(row => row.ToStableLine()),
            StringComparer.Ordinal);
        var actual = new HashSet<string>(
            actualRows.Select(row => row.ToStableLine()),
            StringComparer.Ordinal);

        foreach (var row in actualRows)
        {
            if (!expected.Contains(row.ToStableLine()))
                problems.Add($"legacy_snapshot_missing_current_row:{row.ToStableLine()}");
        }

        foreach (var row in expectedRows)
        {
            if (!actual.Contains(row.ToStableLine()))
                problems.Add($"legacy_snapshot_extra_stale_row:{row.ToStableLine()}");
        }

        return problems;
    }

    private static IReadOnlyList<string> CompareWeakSnapshot(
        IReadOnlyList<TestSuiteBatchSnapshotRow> activeRows,
        IReadOnlyList<TestSuiteBatchSnapshotRow> baselineRows,
        IReadOnlyList<TestSuiteBatchSnapshotRow> currentWeakRows,
        IReadOnlyList<TestSuiteBatchSnapshotRow> currentRows,
        IReadOnlyList<TestSuiteBatchRow> batchRows)
    {
        var problems = new List<string>();
        var active = new HashSet<string>(
            activeRows.Select(row => row.ToStableLine()),
            StringComparer.Ordinal);
        var baseline = new HashSet<string>(
            baselineRows.Select(row => row.ToStableLine()),
            StringComparer.Ordinal);
        var currentWeak = new HashSet<string>(
            currentWeakRows.Select(row => row.ToStableLine()),
            StringComparer.Ordinal);
        var currentAll = new HashSet<string>(
            currentRows.Select(row => row.ToStableLine()),
            StringComparer.Ordinal);

        foreach (var row in activeRows)
        {
            var stableLine = row.ToStableLine();
            if (!baseline.Contains(stableLine))
                problems.Add($"legacy_weak_snapshot_active_row_not_in_baseline:{stableLine}");

            if (!currentAll.Contains(stableLine))
            {
                problems.Add($"legacy_weak_snapshot_active_stale_row_remove_from_snapshot:{stableLine}");
                continue;
            }

            if (!currentWeak.Contains(stableLine))
                problems.Add($"legacy_weak_snapshot_active_row_has_strong_batch_signal_remove_from_snapshot:{stableLine}");
        }

        foreach (var row in currentWeakRows)
        {
            var stableLine = row.ToStableLine();
            if (!baseline.Contains(stableLine))
            {
                problems.Add($"new_test_class_without_strong_batch_signal:{stableLine}:hint={BuildWeakClassRoutingHint(row, batchRows)}");
                continue;
            }

            if (!active.Contains(stableLine))
                problems.Add($"legacy_weak_snapshot_missing_current_weak_row:{stableLine}");
        }

        return problems;
    }

    private static IReadOnlyList<TestSuiteBatchAssignment> ValidateManifestAgainstInventory(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchProject> projects,
        IReadOnlyList<TestSuiteBatchClass> classes,
        TestSuiteBatchGuardOptions options,
        List<string> problems)
    {
        if (options.ExpectedBatchCount is { } expectedBatchCount && rows.Count != expectedBatchCount)
            problems.Add($"batch_manifest_row_count_mismatch:expected={expectedBatchCount}:actual={rows.Count}");

        var projectDlls = projects.Select(project => project.Dll).ToHashSet(StringComparer.Ordinal);
        var manifestDlls = rows.Select(row => row.Dll).ToHashSet(StringComparer.Ordinal);
        foreach (var dll in manifestDlls.Where(dll => !projectDlls.Contains(dll)).OrderBy(dll => dll, StringComparer.Ordinal))
            problems.Add($"batch_manifest_unknown_dll:{dll}");

        foreach (var dll in projectDlls.Where(dll => !manifestDlls.Contains(dll)).OrderBy(dll => dll, StringComparer.Ordinal))
            problems.Add($"batch_manifest_missing_test_assembly:{dll}");

        var effectiveRows = BuildEffectiveRows(rows);
        var assignments = new List<TestSuiteBatchAssignment>();
        var assignmentByIdentity = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var testClass in classes)
        {
            var effectiveMatches = effectiveRows
                .Where(row => row.Dll == testClass.Dll && TestEffectiveFilterForClass(row.EffectiveFilter, testClass.FullyQualifiedName))
                .ToArray();
            if (effectiveMatches.Length != 1)
                problems.Add($"batch_effective_assignment_count:{testClass.Identity}:count={effectiveMatches.Length}");

            var rawMatch = rows.FirstOrDefault(row =>
                row.Dll == testClass.Dll && TestBatchFilter(row.Filter, testClass.FullyQualifiedName));
            if (rawMatch is null)
            {
                problems.Add($"batch_manifest_unassigned_class:{testClass.Identity}");
                continue;
            }

            assignments.Add(new TestSuiteBatchAssignment(
                testClass,
                rawMatch.BatchId,
                HasStrongBatchSignal(rawMatch, testClass.FullyQualifiedName)));
            assignmentByIdentity[testClass.Identity] = rawMatch.BatchId;
        }

        if (options.EnforceAtomicGroups)
            problems.AddRange(ValidateAtomicGroups(classes, assignmentByIdentity, options.RequireAtomicGroupMembers));

        if (rows.Any(row => row.BatchId == SemanticCompilerSearchVectorBatchId) ||
            assignments.Any(IsSemanticCompilerSearchVectorClass))
            problems.AddRange(ValidateSemanticCompilerSearchVectorBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == DesignConversionLiveLocalServerMethodDiagnosticBatchId) ||
            assignments.Any(IsDesignConversionLiveLocalServerMethodDiagnosticClass))
            problems.AddRange(ValidateDesignConversionLiveLocalServerMethodDiagnosticBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == OrchestrationRoutingProductBoundariesBatchId) ||
            assignments.Any(IsOrchestrationRoutingProductBoundaryClass))
            problems.AddRange(ValidateOrchestrationRoutingProductBoundariesBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == TestInfrastructureEvidenceMeasurementBatchId) ||
            assignments.Any(IsTestInfrastructureEvidenceMeasurementClass))
            problems.AddRange(ValidateTestInfrastructureEvidenceMeasurementBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == ServerOwnedReplayManifestAttestationBatchId) ||
            assignments.Any(IsServerOwnedReplayManifestAttestationClass))
            problems.AddRange(ValidateServerOwnedReplayManifestAttestationBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == Phase08CommonUserTwoLegLifecycleBatchId) ||
            assignments.Any(IsPhase08CommonUserTwoLegLifecycleClass))
            problems.AddRange(ValidatePhase08CommonUserTwoLegLifecycleBatch(rows, assignments));

        // Проверяем singleton-маршрут, если строка уже есть в TSV или если класс
        // появился в исходниках: пропавшая строка тоже должна стать явной проблемой.
        if (rows.Any(row => row.BatchId == SingletonFlowNodeIdentityBatchId) ||
            assignments.Any(IsSingletonFlowNodeIdentityClass))
            problems.AddRange(ValidateSingletonFlowNodeIdentityBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == LogicalTopologyAndResultGapBatchId) ||
            assignments.Any(IsLogicalTopologyAndResultGapClass))
            problems.AddRange(ValidateLogicalTopologyAndResultGapBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == ResultGapTrainingPairWriterBatchId) ||
            assignments.Any(IsResultGapTrainingPairWriterClass))
            problems.AddRange(ValidateResultGapTrainingPairWriterBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == SingleCombinationGraphRunnerBatchId) ||
            assignments.Any(IsSingleCombinationGraphRunnerClass))
            problems.AddRange(ValidateSingleCombinationGraphRunnerBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == RetainedDomainModelRegistryBatchId) ||
            assignments.Any(IsRetainedDomainModelRegistryClass))
            problems.AddRange(ValidateRetainedDomainModelRegistryBatch(rows, assignments));

        if (rows.Any(row => row.BatchId == ModelActivationBatchId) ||
            assignments.Any(IsModelActivationClass))
            problems.AddRange(ValidateModelActivationBatch(rows, assignments));

        return assignments
            .OrderBy(assignment => assignment.TestClass.Dll, StringComparer.Ordinal)
            .ThenBy(assignment => assignment.TestClass.FullyQualifiedName, StringComparer.Ordinal)
            .ThenBy(assignment => assignment.TestClass.SourcePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> ValidateSemanticCompilerSearchVectorBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            SemanticCompilerSearchVectorBatchId,
            SemanticCompilerSearchVectorClassFqns,
            "semantic_compiler_search_vector",
            "semantic compiler, search and vector-search classes must stay in their named human batch");

    // Отдельная проверка не запускает сервер: она доказывает, что каноническая
    // лестница выделяет ровно этот класс и сохраняет границу по точке в фильтре.
    private static IReadOnlyList<string> ValidateDesignConversionLiveLocalServerMethodDiagnosticBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            DesignConversionLiveLocalServerMethodDiagnosticBatchId,
            DesignConversionLiveLocalServerMethodDiagnosticClassFqns,
            "design_conversion_live_local_server_method_diagnostic",
            "the slow DesignConversionLiveLocalServerTests class must stay in its one-class method-diagnostic route",
            "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll");

    private static IReadOnlyList<string> ValidateOrchestrationRoutingProductBoundariesBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            OrchestrationRoutingProductBoundariesBatchId,
            OrchestrationRoutingProductBoundaryClassFqns,
            "orchestration_routing_product_boundaries",
            "request compilation, route selection, orchestrator, wiring, plugin and thin-client boundary classes must stay in their named human batch");

    private static IReadOnlyList<string> ValidateTestInfrastructureEvidenceMeasurementBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            TestInfrastructureEvidenceMeasurementBatchId,
            TestInfrastructureEvidenceMeasurementClassFqns,
            "test_infrastructure_evidence_measurement",
            "test infrastructure, RAG evidence index and measurement guard classes must stay in their named human batch");

    private static IReadOnlyList<string> ValidateServerOwnedReplayManifestAttestationBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            ServerOwnedReplayManifestAttestationBatchId,
            ServerOwnedReplayManifestAttestationClassFqns,
            "server_owned_replay_manifest_attestation",
            "server-owned replay verification and executable manifest passport tests must stay in their one-class class-boundary batch",
            "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll");

    private static IReadOnlyList<string> ValidatePhase08CommonUserTwoLegLifecycleBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            Phase08CommonUserTwoLegLifecycleBatchId,
            Phase08CommonUserTwoLegLifecycleClassFqns,
            "phase08_common_user_two_leg_lifecycle",
            "Phase 08 portable profile, approved share, continued training and rollback evidence tests must stay in their one-class class-boundary batch",
            "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll");

    // Эта проверка требует ровно один класс, правильную DLL и фильтр с точкой:
    // так похожий будущий класс не расширит короткий запуск без отдельного решения.
    private static IReadOnlyList<string> ValidateSingletonFlowNodeIdentityBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            SingletonFlowNodeIdentityBatchId,
            SingletonFlowNodeIdentityClassFqns,
            "singleton_flow_node_identity",
            "singleton-проверки общих объектов потоков и узлов должны оставаться в маршруте одного класса без расширения соседними классами",
            "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll");

    // Эти два класса дают короткий диагностический запуск структуры потоков и
    // иерархии исправления результата: проверка не должна принять ни потерю
    // одного класса, ни добавление похожего имени, ни перенос в другую сборку.
    private static IReadOnlyList<string> ValidateLogicalTopologyAndResultGapBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            LogicalTopologyAndResultGapBatchId,
            LogicalTopologyAndResultGapClassFqns,
            "logical_topology_and_result_gap",
            "проверки структуры потоков и иерархии исправления результата должны оставаться в коротком маршруте из двух классов",
            "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll");

    private static IReadOnlyList<string> ValidateResultGapTrainingPairWriterBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            ResultGapTrainingPairWriterBatchId,
            ResultGapTrainingPairWriterClassFqns,
            "result_gap_training_pair_writer",
            "запись обезличенных обучающих пар и исправлений должна оставаться отдельным маршрутом одного класса; этот маршрут не запускает обучение модели и не доказывает качество обучения",
            "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
            PairGraphBroadCollisionBatchIds);

    private static IReadOnlyList<string> ValidateSingleCombinationGraphRunnerBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            SingleCombinationGraphRunnerBatchId,
            SingleCombinationGraphRunnerClassFqns,
            "single_combination_graph_runner",
            "выполнение одного варианта графа с отменой и ограничением времени должно оставаться отдельным маршрутом одного класса; этот маршрут не запускает обучение модели и не доказывает качество обучения",
            "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
            PairGraphBroadCollisionBatchIds);

    private static IReadOnlyList<string> ValidateRetainedDomainModelRegistryBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            RetainedDomainModelRegistryBatchId,
            RetainedDomainModelRegistryClassFqns,
            "retained_domain_model_registry",
            "защищённый каталог сохраняемых моделей должен оставаться отдельным маршрутом одного класса; Windows-only строки учитываются как NotExecuted, а не как пройденные",
            "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll");

    private static IReadOnlyList<string> ValidateModelActivationBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments)
        => ValidateExactHumanBatch(
            rows,
            assignments,
            ModelActivationBatchId,
            ModelActivationClassFqns,
            "model_activation",
            "design conversion model activation classes must stay in the six-class assembly batch without wildcard or lookalike expansion",
            "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll");

    private static IReadOnlyList<string> ValidateTestInfrastructureEvidenceMeasurementLeafModeOverride(
        IReadOnlyList<TestSuiteBatchLeafModeOverrideRow> rows)
    {
        var problems = new List<string>();
        var targetRows = rows
            .Where(row => string.Equals(row.BatchId, TestInfrastructureEvidenceMeasurementBatchId, StringComparison.Ordinal))
            .ToArray();
        if (targetRows.Length != 1)
        {
            problems.Add(
                "test_infrastructure_evidence_measurement_leaf_mode_override_row_count:" +
                $"expected=1:actual={targetRows.Length}:reason=heavy guard methods must be isolated one method per leaf");
            return problems;
        }

        if (!string.Equals(targetRows[0].LeafMode, "method-diagnostic", StringComparison.Ordinal))
        {
            problems.Add(
                "test_infrastructure_evidence_measurement_leaf_mode_override_wrong_mode:" +
                $"actual={targetRows[0].LeafMode}:reason=one hanging inventory guard must not burn sibling RAG and measurement evidence");
        }

        return problems;
    }

    private static IReadOnlyList<string> ValidateDesignConversionLiveLocalServerMethodDiagnosticLeafModeOverride(
        IReadOnlyList<TestSuiteBatchLeafModeOverrideRow> rows)
    {
        var problems = new List<string>();
        var targetRows = rows
            .Where(row => string.Equals(row.BatchId, DesignConversionLiveLocalServerMethodDiagnosticBatchId, StringComparison.Ordinal))
            .ToArray();
        if (targetRows.Length != 1)
        {
            problems.Add(
                "design_conversion_live_local_server_method_diagnostic_leaf_mode_override_row_count:" +
                $"expected=1:actual={targetRows.Length}:reason=slow server methods must be isolated one method per leaf");
            return problems;
        }

        if (!string.Equals(targetRows[0].LeafMode, "method-diagnostic", StringComparison.Ordinal))
        {
            problems.Add(
                "design_conversion_live_local_server_method_diagnostic_leaf_mode_override_wrong_mode:" +
                $"actual={targetRows[0].LeafMode}:reason=method-level results must be written before the next slow method starts");
        }

        return problems;
    }

    private static IReadOnlyList<string> ValidateDesignConversionDpoStorageLeafModeOverride(
        IReadOnlyList<TestSuiteBatchLeafModeOverrideRow> rows)
    {
        var problems = new List<string>();
        // Это намеренно узкая настройка: разбиение по методам допускается только
        // внутри существующей группы хранилища, где проверяется точное объявление
        // класса, его связь с исходниками и изоляция известного медленного метода.
        var targetRows = rows
            .Where(row => string.Equals(row.BatchId, DesignConversionDpoStorageBatchId, StringComparison.Ordinal))
            .ToArray();
        if (targetRows.Length != 1)
        {
            problems.Add(
                "design_conversion_dpo_storage_leaf_mode_override_row_count:" +
                $"expected=1:actual={targetRows.Length}:reason=storage global class must be packed by bounded method groups inside its existing batch");
            return problems;
        }

        if (!string.Equals(targetRows[0].LeafMode, "class-method-pack", StringComparison.Ordinal))
        {
            problems.Add(
                "design_conversion_dpo_storage_leaf_mode_override_wrong_mode:" +
                $"actual={targetRows[0].LeafMode}:reason=storage class-method-pack must isolate the historically slow approval rewrite method without splitting the rest of the batch");
        }

        return problems;
    }

    private static IReadOnlyList<string> ValidateExactHumanBatch(
        IReadOnlyList<TestSuiteBatchRow> rows,
        IReadOnlyList<TestSuiteBatchAssignment> assignments,
        string batchId,
        ImmutableArray<string> expectedClassFqns,
        string problemPrefix,
        string humanReason,
        string? expectedDll = null,
        ImmutableArray<string> mustPrecedeBatchIds = default)
    {
        var problems = new List<string>();
        var expected = expectedClassFqns.ToHashSet(StringComparer.Ordinal);
        var namedRows = rows
            .Where(row => string.Equals(row.BatchId, batchId, StringComparison.Ordinal))
            .ToArray();
        if (namedRows.Length != 1)
            problems.Add($"{problemPrefix}_batch_row_count:expected=1:actual={namedRows.Length}:reason={humanReason}");

        foreach (var row in namedRows)
        {
            if (expectedDll is not null &&
                !string.Equals(row.Dll, expectedDll, StringComparison.Ordinal))
                problems.Add($"{problemPrefix}_wrong_dll:expected={expectedDll}:actual={row.Dll}:reason={humanReason}");

            if (!mustPrecedeBatchIds.IsDefaultOrEmpty)
            {
                foreach (var broadRow in rows.Where(candidate =>
                             mustPrecedeBatchIds.Contains(candidate.BatchId, StringComparer.Ordinal)))
                {
                    if (row.LineNumber >= broadRow.LineNumber)
                        problems.Add(
                            $"{problemPrefix}_priority_after_broad_batch:{row.BatchId}:must_precede={broadRow.BatchId}:" +
                            $"row_line={row.LineNumber}:broad_line={broadRow.LineNumber}:reason={humanReason}");
                }
            }

            var declaredTerms = GetPositiveFilterTerms(row.Filter)
                .Select(NormalizeClassBoundaryTerm)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var classFqn in expected.Where(classFqn => !declaredTerms.Contains(classFqn)).OrderBy(classFqn => classFqn, StringComparer.Ordinal))
                problems.Add($"{problemPrefix}_filter_missing_exact_class:{classFqn}:batch={batchId}:reason=canonical TSV must use the full class name or class-boundary term, not a short method or family term");

            foreach (var term in declaredTerms.Where(term => !expected.Contains(term)).OrderBy(term => term, StringComparer.Ordinal))
                problems.Add($"{problemPrefix}_filter_unexpected_term:{term}:batch={batchId}:reason=small diagnostic batch must not widen into a hidden remainder");
        }

        var relevantAssignments = expectedDll is null
            ? assignments
            : assignments.Where(assignment => string.Equals(assignment.TestClass.Dll, expectedDll, StringComparison.Ordinal)).ToArray();
        var assignmentByClass = relevantAssignments
            .Where(assignment => expected.Contains(assignment.TestClass.FullyQualifiedName))
            .ToDictionary(
                assignment => assignment.TestClass.FullyQualifiedName,
                assignment => assignment.BatchId,
                StringComparer.Ordinal);
        var actual = relevantAssignments
            .Where(assignment => assignment.BatchId == batchId)
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var classFqn in expected.Where(classFqn => !actual.Contains(classFqn)).OrderBy(classFqn => classFqn, StringComparer.Ordinal))
        {
            if (assignmentByClass.TryGetValue(classFqn, out var actualBatchId))
            {
                problems.Add(
                    $"{problemPrefix}_wrong_batch:{classFqn}:expected={batchId}:actual={actualBatchId}:" +
                    $"reason={humanReason}");
                continue;
            }

            problems.Add($"{problemPrefix}_missing_class:{classFqn}:reason={humanReason}");
        }

        foreach (var classFqn in actual.Where(classFqn => !expected.Contains(classFqn)).OrderBy(classFqn => classFqn, StringComparer.Ordinal))
        {
            // Маленькая диагностическая группа не должна становиться второй
            // общей корзиной: один лишний класс уже размывает смысл сигнала.
            problems.Add($"{problemPrefix}_unexpected_class:{classFqn}:reason={humanReason}");
        }

        return problems;
    }

    private static bool IsSemanticCompilerSearchVectorClass(TestSuiteBatchAssignment assignment)
        => SemanticCompilerSearchVectorClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsDesignConversionLiveLocalServerMethodDiagnosticClass(TestSuiteBatchAssignment assignment)
        => DesignConversionLiveLocalServerMethodDiagnosticClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsOrchestrationRoutingProductBoundaryClass(TestSuiteBatchAssignment assignment)
        => OrchestrationRoutingProductBoundaryClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsTestInfrastructureEvidenceMeasurementClass(TestSuiteBatchAssignment assignment)
        => TestInfrastructureEvidenceMeasurementClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsServerOwnedReplayManifestAttestationClass(TestSuiteBatchAssignment assignment)
        => ServerOwnedReplayManifestAttestationClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsPhase08CommonUserTwoLegLifecycleClass(TestSuiteBatchAssignment assignment)
        => Phase08CommonUserTwoLegLifecycleClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    // Этот predicate нужен не для назначения классов, а для раннего сигнала:
    // если singleton-класс уехал из своей строки, guard всё равно запустит проверку.
    private static bool IsSingletonFlowNodeIdentityClass(TestSuiteBatchAssignment assignment)
        => SingletonFlowNodeIdentityClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsLogicalTopologyAndResultGapClass(TestSuiteBatchAssignment assignment)
        => LogicalTopologyAndResultGapClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsResultGapTrainingPairWriterClass(TestSuiteBatchAssignment assignment)
        => ResultGapTrainingPairWriterClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsSingleCombinationGraphRunnerClass(TestSuiteBatchAssignment assignment)
        => SingleCombinationGraphRunnerClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsRetainedDomainModelRegistryClass(TestSuiteBatchAssignment assignment)
        => RetainedDomainModelRegistryClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsModelActivationClass(TestSuiteBatchAssignment assignment)
        => ModelActivationClassFqns.Contains(
            assignment.TestClass.FullyQualifiedName,
            StringComparer.Ordinal);

    private static bool IsAllowedLeafMode(string value)
        => string.Equals(value, "pack", StringComparison.Ordinal) ||
           string.Equals(value, "method-diagnostic", StringComparison.Ordinal) ||
           string.Equals(value, "class-method-pack", StringComparison.Ordinal) ||
           string.Equals(value, "off", StringComparison.Ordinal);

    private static IReadOnlyList<string> ValidateAtomicGroups(
        IReadOnlyList<TestSuiteBatchClass> classes,
        IReadOnlyDictionary<string, string> assignmentByIdentity,
        bool requireMembers)
    {
        var problems = new List<string>();
        foreach (var group in AtomicGroups)
        {
            var members = classes
                .Where(testClass => group.Terms.Any(term =>
                    testClass.FullyQualifiedName.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
            if (members.Length == 0)
            {
                if (requireMembers)
                    problems.Add($"atomic_group_has_no_current_members:{group.Name}");

                continue;
            }

            foreach (var member in members)
            {
                if (!assignmentByIdentity.TryGetValue(member.Identity, out var actualBatchId))
                    continue;

                if (IsDesignConversionLiveLocalServerMethodDiagnosticAtomicException(group, member, actualBatchId))
                    continue;

                if (!string.Equals(actualBatchId, group.IntendedBatchId, StringComparison.Ordinal))
                    problems.Add($"atomic_group_wrong_batch:{group.Name}:{member.Identity}:expected={group.IntendedBatchId}:actual={actualBatchId}");
            }
        }

        return problems;
    }

    private static bool IsDesignConversionLiveLocalServerMethodDiagnosticAtomicException(
        AtomicBatchGroup group,
        TestSuiteBatchClass member,
        string actualBatchId)
    {
        // Старый family-guard всё ещё ловит lookalike и чужие namespace. Исключение
        // разрешено только реальному глобальному классу, который вынесен из pack
        // из-за измеренного размера порции, а не из-за общей смены семейства.
        return string.Equals(group.IntendedBatchId, "design-conversion-local-host", StringComparison.Ordinal) &&
            string.Equals(actualBatchId, DesignConversionLiveLocalServerMethodDiagnosticBatchId, StringComparison.Ordinal) &&
            string.Equals(member.Dll, "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", StringComparison.Ordinal) &&
            string.Equals(member.FullyQualifiedName, "DesignConversionLiveLocalServerTests", StringComparison.Ordinal);
    }

    private static TestSuiteBatchProject? TryReadTestProject(string repoRoot, string projectPath)
    {
        XDocument document;
        try
        {
            document = XDocument.Load(projectPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            throw new InvalidOperationException($"Cannot read project file: {projectPath}", exception);
        }

        var packageNames = document.Descendants()
            .Where(element => element.Name.LocalName == "PackageReference")
            .Select(element => (string?)element.Attribute("Include") ?? (string?)element.Attribute("Update") ?? string.Empty);
        if (!packageNames.Any(name => string.Equals(name, "Microsoft.NET.Test.Sdk", StringComparison.OrdinalIgnoreCase)))
            return null;

        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var targetFramework = document.Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "TargetFramework")?.Value;
        if (string.IsNullOrWhiteSpace(targetFramework))
        {
            targetFramework = document.Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "TargetFrameworks")?.Value
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(targetFramework))
            throw new InvalidOperationException($"Test project has no TargetFramework: {projectPath}");

        var assemblyName = document.Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "AssemblyName")?.Value;
        if (string.IsNullOrWhiteSpace(assemblyName))
            assemblyName = Path.GetFileNameWithoutExtension(projectPath);

        var dllPath = Path.Combine(projectDirectory, "bin", "Release", targetFramework, assemblyName + ".dll");
        return new TestSuiteBatchProject(
            ProjectPath: NormalizeRepoPath(repoRoot, projectPath),
            FullProjectPath: Path.GetFullPath(projectPath),
            Dll: NormalizeRepoPath(repoRoot, dllPath),
            FullProjectDirectory: projectDirectory,
            SourceFiles: DiscoverSourceFiles(projectPath, document));
    }

    private static IReadOnlyList<string> DiscoverSourceFiles(string projectPath, XDocument document)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var files = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in EnumerateFilesWithoutIgnoredPathSegments(projectDirectory, "*.cs"))
        {
            if (!HasIgnoredPathSegment(file))
                files.Add(Path.GetFullPath(file));
        }

        foreach (var include in document.Descendants()
                     .Where(element => element.Name.LocalName == "Compile")
                     .Select(element => (string?)element.Attribute("Include"))
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .SelectMany(value => value!.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))
        {
            // Linked Compile items are outside the project folder; they still
            // compile into this assembly and therefore get their own DLL identity.
            if (include.Contains('*', StringComparison.Ordinal))
                continue;

            var includePath = NormalizeProjectIncludePath(include);
            var fullPath = Path.GetFullPath(Path.Combine(projectDirectory, includePath));
            if (File.Exists(fullPath) && !HasIgnoredPathSegment(fullPath))
                files.Add(fullPath);
        }

        return files.ToArray();
    }

    private static IReadOnlyList<TestSuiteBatchClass> DiscoverProjectTestClasses(
        string repoRoot,
        TestSuiteBatchProject project)
    {
        var trees = project.SourceFiles
            .Select(path => CSharpSyntaxTree.ParseText(
                File.ReadAllText(path, Encoding.UTF8),
                new CSharpParseOptions(LanguageVersion.Preview),
                path: path))
            .ToArray();
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => assembly.Location)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
        var compilation = CSharpCompilation.Create(
            "TestSuiteBatchInventoryScan",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var knownFactAttributeNames = BuildKnownFactAttributeNames(trees);
        var classes = new Dictionary<string, TestClassAccumulator>(StringComparer.Ordinal);

        foreach (var tree in trees)
        {
            var root = tree.GetCompilationUnitRoot();
            var model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!method.AttributeLists.SelectMany(list => list.Attributes)
                        .Any(attribute => IsFactOrTheoryAttribute(attribute, model, knownFactAttributeNames)))
                    continue;

                var containingClass = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (containingClass is null)
                    continue;

                var classFqn = GetNestedClassFqn(containingClass);
                var key = project.Dll + "|" + classFqn;
                if (!classes.TryGetValue(key, out var accumulator))
                {
                    accumulator = new TestClassAccumulator(project.Dll, classFqn);
                    classes[key] = accumulator;
                }

                accumulator.SourcePaths.Add(NormalizeRepoPath(repoRoot, tree.FilePath));
            }
        }

        return classes.Values
            .Select(accumulator => new TestSuiteBatchClass(
                accumulator.Dll,
                accumulator.FullyQualifiedName,
                string.Join(";", accumulator.SourcePaths.OrderBy(path => path, StringComparer.Ordinal))))
            .OrderBy(testClass => testClass.FullyQualifiedName, StringComparer.Ordinal)
            .ToArray();
    }

    private static HashSet<string> BuildKnownFactAttributeNames(IReadOnlyList<SyntaxTree> trees)
    {
        var known = new HashSet<string>(StringComparer.Ordinal)
        {
            "Fact",
            "FactAttribute",
            "Theory",
            "TheoryAttribute"
        };
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var declaration in trees
                         .Select(tree => tree.GetCompilationUnitRoot())
                         .SelectMany(root => root.DescendantNodes().OfType<ClassDeclarationSyntax>()))
            {
                if (declaration.BaseList is null)
                    continue;

                var inheritsKnown = declaration.BaseList.Types
                    .Any(baseType => IsKnownAttributeName(baseType.Type.ToString(), known));
                if (!inheritsKnown)
                    continue;

                if (known.Add(declaration.Identifier.Text))
                    changed = true;

                if (declaration.Identifier.Text.EndsWith("Attribute", StringComparison.Ordinal) &&
                    known.Add(declaration.Identifier.Text[..^"Attribute".Length]))
                    changed = true;
            }
        }

        return known;
    }

    private static bool IsFactOrTheoryAttribute(
        AttributeSyntax attribute,
        SemanticModel semanticModel,
        ISet<string> knownFactAttributeNames)
    {
        var type = semanticModel.GetTypeInfo(attribute).Type;
        while (type is not null)
        {
            if (type.Name is "FactAttribute" or "TheoryAttribute")
                return true;

            type = type.BaseType;
        }

        return IsKnownAttributeName(attribute.Name.ToString(), knownFactAttributeNames);
    }

    private static bool IsKnownAttributeName(string name, ISet<string> knownFactAttributeNames)
    {
        var simpleName = name.Split('.').Last();
        simpleName = simpleName.Split('<').First();
        simpleName = simpleName.Replace("global::", string.Empty, StringComparison.Ordinal);
        if (knownFactAttributeNames.Contains(simpleName))
            return true;

        return simpleName.EndsWith("Attribute", StringComparison.Ordinal) &&
            knownFactAttributeNames.Contains(simpleName[..^"Attribute".Length]);
    }

    private static string GetNestedClassFqn(ClassDeclarationSyntax declaration)
    {
        var typeNames = declaration.AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .Reverse()
            .Select(type => type.Identifier.Text)
            .ToArray();
        var typeName = string.Join("+", typeNames);
        var namespaceName = declaration.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Select(ns => ns.Name.ToString())
            .LastOrDefault();

        return string.IsNullOrWhiteSpace(namespaceName) ? typeName : namespaceName + "." + typeName;
    }

    private static IReadOnlyList<TestSuiteBatchEffectiveRow> BuildEffectiveRows(
        IReadOnlyList<TestSuiteBatchRow> rows)
    {
        var termsByDll = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var effectiveRows = new List<TestSuiteBatchEffectiveRow>();
        foreach (var row in rows)
        {
            if (!termsByDll.TryGetValue(row.Dll, out var priorTerms))
            {
                priorTerms = [];
                termsByDll[row.Dll] = priorTerms;
            }

            var effectiveFilter = BuildEffectiveFilter(row.Filter, priorTerms);
            effectiveRows.Add(new TestSuiteBatchEffectiveRow(row.BatchId, row.Dll, row.Filter, effectiveFilter));
            priorTerms.AddRange(GetPositiveFilterTerms(row.Filter));
        }

        return effectiveRows;
    }

    private static string BuildEffectiveFilter(string rawFilter, IReadOnlyList<string> priorTerms)
    {
        var effective = rawFilter == "*" ? string.Empty : "(" + rawFilter + ")";
        foreach (var term in priorTerms)
        {
            if (string.IsNullOrWhiteSpace(term))
                continue;

            effective = string.IsNullOrWhiteSpace(effective)
                ? "FullyQualifiedName!~" + term
                : effective + "&FullyQualifiedName!~" + term;
        }

        return string.IsNullOrWhiteSpace(effective) ? "*" : effective;
    }

    private static bool TestBatchFilter(string filter, string fullyQualifiedName)
    {
        if (filter == "*")
            return true;

        foreach (var orPart in filter.Split('|'))
        {
            var allMatched = true;
            foreach (var term in orPart.Split('&'))
            {
                if (!TestFilterTerm(term, fullyQualifiedName))
                {
                    allMatched = false;
                    break;
                }
            }

            if (allMatched)
                return true;
        }

        return false;
    }

    private static bool TestEffectiveFilterForClass(string filter, string fullyQualifiedName)
    {
        if (filter == "*")
            return true;

        foreach (var negative in GetNegativeFilterTerms(filter))
        {
            if (ClassOrMethodFilterTermMatches(fullyQualifiedName, negative))
                return false;
        }

        var positives = GetPositiveFilterTerms(filter).ToArray();
        return positives.Length == 0 ||
            positives.Any(positive => ClassOrMethodFilterTermMatches(fullyQualifiedName, positive));
    }

    private static bool TestFilterTerm(string term, string fullyQualifiedName)
    {
        var trimmed = TrimFilterParentheses(term.Trim());
        if (trimmed == "*")
            return true;

        if (trimmed.StartsWith("FullyQualifiedName~", StringComparison.Ordinal))
            return ClassOrMethodFilterTermMatches(fullyQualifiedName, trimmed["FullyQualifiedName~".Length..]);

        if (trimmed.StartsWith("FullyQualifiedName!~", StringComparison.Ordinal))
            return !ClassOrMethodFilterTermMatches(fullyQualifiedName, trimmed["FullyQualifiedName!~".Length..]);

        if (trimmed.StartsWith("FullyQualifiedName=", StringComparison.Ordinal))
            return string.Equals(fullyQualifiedName, trimmed["FullyQualifiedName=".Length..], StringComparison.Ordinal);

        throw new InvalidOperationException("Unsupported test batch filter term: " + trimmed);
    }

    private static bool ClassOrMethodFilterTermMatches(string fullyQualifiedName, string term)
    {
        if (term.EndsWith(".", StringComparison.Ordinal))
        {
            var classFqn = NormalizeClassBoundaryTerm(term);
            return string.Equals(fullyQualifiedName, classFqn, StringComparison.OrdinalIgnoreCase) ||
                fullyQualifiedName.StartsWith(term, StringComparison.OrdinalIgnoreCase);
        }

        return fullyQualifiedName.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeClassBoundaryTerm(string term)
        => term.EndsWith(".", StringComparison.Ordinal) ? term[..^1] : term;

    private static IEnumerable<string> GetPositiveFilterTerms(string filter)
    {
        foreach (var piece in filter.Split(['|', '&']))
        {
            var trimmed = TrimFilterParentheses(piece.Trim());
            if (trimmed.StartsWith("FullyQualifiedName~", StringComparison.Ordinal))
                yield return trimmed["FullyQualifiedName~".Length..];
        }
    }

    private static bool HasStrongBatchSignal(TestSuiteBatchRow row, string fullyQualifiedName)
        => row.Filter != "*" &&
            GetPositiveFilterTerms(row.Filter)
                .Any(term => ClassOrMethodFilterTermMatches(fullyQualifiedName, term));

    private static string BuildWeakClassRoutingHint(
        TestSuiteBatchSnapshotRow row,
        IReadOnlyList<TestSuiteBatchRow> batchRows)
    {
        var declaredPrefixes = batchRows
            .Where(batchRow => string.Equals(batchRow.Dll, row.Dll, StringComparison.Ordinal))
            .SelectMany(batchRow => GetPositiveFilterTerms(batchRow.Filter)
                .Select(term => $"{batchRow.BatchId}:FullyQualifiedName~{term}"))
            .Distinct(StringComparer.Ordinal)
            .Take(6)
            .ToArray();
        var family = GuessHumanClassFamily(row.ClassFqn, row.SourcePath);
        var prefixExamples = declaredPrefixes.Length == 0
            ? "в этой сборке пока нет явных префиксов"
            : string.Join(", ", declaredPrefixes);

        return "Класс попал только в wildcard-остаток. Не добавляйте его в active snapshot; " +
            $"выберите человеческую семью '{family}' и добавьте явный FullyQualifiedName~ префикс в tools/test-batches/batches.tsv. " +
            $"Примеры уже объявленных префиксов: {prefixExamples}.";
    }

    private static string GuessHumanClassFamily(string classFqn, string sourcePath)
    {
        var simpleName = classFqn.Split(['.', '+'], StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? classFqn;
        var trimmed = simpleName.EndsWith("Tests", StringComparison.Ordinal)
            ? simpleName[..^"Tests".Length]
            : simpleName;
        var family = new string(trimmed.TakeWhile(char.IsLetter).ToArray());
        if (string.IsNullOrWhiteSpace(family))
            family = Path.GetFileNameWithoutExtension(sourcePath);

        return string.IsNullOrWhiteSpace(family) ? classFqn : family;
    }

    private static IEnumerable<string> GetNegativeFilterTerms(string filter)
    {
        foreach (var piece in filter.Split(['|', '&']))
        {
            var trimmed = TrimFilterParentheses(piece.Trim());
            if (trimmed.StartsWith("FullyQualifiedName!~", StringComparison.Ordinal))
                yield return trimmed["FullyQualifiedName!~".Length..];
        }
    }

    private static IReadOnlyList<string> ValidateRunnerCompatibleFilter(string filter, string batchId)
    {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(filter))
            return [$"batch_manifest_empty_filter:{batchId}"];

        if (filter == "*")
            return [];

        foreach (var part in filter.Split(['|', '&']))
        {
            var term = TrimFilterParentheses(part.Trim());
            if (term == "*" ||
                term.StartsWith("FullyQualifiedName~", StringComparison.Ordinal) ||
                term.StartsWith("FullyQualifiedName!~", StringComparison.Ordinal) ||
                term.StartsWith("FullyQualifiedName=", StringComparison.Ordinal))
                continue;

            problems.Add($"batch_manifest_unsupported_filter_term:{batchId}:{term}");
        }

        return problems;
    }

    private static string TrimFilterParentheses(string value)
    {
        var trimmed = value.Trim();
        while (trimmed.StartsWith('('))
            trimmed = trimmed[1..].Trim();

        while (trimmed.EndsWith(')'))
            trimmed = trimmed[..^1].Trim();

        return trimmed;
    }

    private static string CreateSnapshotBody(IEnumerable<TestSuiteBatchSnapshotRow> rows)
    {
        var builder = new StringBuilder();
        AppendSnapshotLine(builder, SnapshotBodyHeader);
        foreach (var row in rows)
            AppendSnapshotLine(builder, row.ToStableLine());

        return builder.ToString();
    }

    private static void AppendSnapshotLine(StringBuilder builder, string line)
    {
        // Snapshot hash считается по LF-канону, чтобы CRLF checkout не менял
        // смысл принятой описи между Windows и Linux/WSL.
        builder.Append(line);
        builder.Append(CanonicalSnapshotNewLine);
    }

    private static string Sha256Lower(string text)
    {
        var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static string NormalizeRepoPath(string repoRoot, string path)
    {
        var relative = Path.GetRelativePath(Path.GetFullPath(repoRoot), Path.GetFullPath(path));
        return NormalizeManifestPath(relative);
    }

    private static string NormalizePathForSnapshot(string path)
        => NormalizeManifestPath(Path.GetFullPath(path));

    private static string NormalizeManifestPath(string path)
        => path.Replace('\\', '/').Trim();

    private static string NormalizeProjectIncludePath(string path)
    {
        // MSBuild-проекты в репозитории пишут linked Compile через Windows
        // separators; inventory guard обязан видеть те же файлы и в WSL.
        return path
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }

    private static bool HasIgnoredPathSegment(string path)
    {
        var parts = Path.GetFullPath(path).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(part =>
            string.Equals(part, "bin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(part, "obj", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(part, "TestResults", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(part, "NuGet", StringComparison.OrdinalIgnoreCase));
    }

    // Область описи тестовых проектов — это то, что хранилище САМО объявляет своей
    // сборкой: файл решения в корне обхода. Без этого ограничения тестовым проектом
    // хранилища становился любой найденный на диске файл проекта со ссылкой на пакет
    // тестового набора, включая копии внутри каталога результатов работ владельца и
    // внутри изолированных опытных площадок, которые никогда не были частью сборки.
    // Такое ограничение не делает проверку кольцевой: решение — независимый источник
    // по отношению к лестнице групп, поэтому исчезновение проекта из сборки и
    // появление проекта, не вписанного в лестницу, сторож по-прежнему замечает.
    private static IEnumerable<string> EnumerateCandidateProjectFiles(string normalizedRoot)
    {
        var declaredProjects = ReadSolutionDeclaredProjectFiles(normalizedRoot);

        // Пустой список означает, что в корне обхода решения нет. Так устроены
        // синтетические корни проверок, где файлы проектов создаются прямо на диске:
        // для них сохраняется прежний полный обход дерева.
        return declaredProjects.Count > 0
            ? declaredProjects
            : EnumerateFilesWithoutIgnoredPathSegments(normalizedRoot, "*.csproj");
    }

    // Читает пути файлов проектов, объявленные решениями в корне обхода.
    private static IReadOnlyList<string> ReadSolutionDeclaredProjectFiles(string normalizedRoot)
    {
        // Порядок здесь не важен для результата: вызывающая сторона сортирует проекты
        // по имени сборки, а набор снимает возможные повторы между решениями.
        var declared = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(normalizedRoot))
            return declared.ToArray();

        // Решения ищем только на верхнем уровне корня: вложенные копии файла решения
        // внутри каталогов результатов работ владельца не являются сборкой хранилища.
        foreach (var solutionPath in Directory.EnumerateFiles(normalizedRoot, "*.sln", SearchOption.TopDirectoryOnly))
        {
            foreach (var line in File.ReadLines(solutionPath))
            {
                // Объявление проекта в решении начинается со слова Project с открывающей
                // скобкой; остальные строки файла решения к описи отношения не имеют.
                if (!line.StartsWith("Project(", StringComparison.Ordinal))
                    continue;

                // Относительный путь к проекту хранится в кавычках, поэтому берём каждое
                // значение в кавычках, которое оканчивается расширением проекта C#.
                foreach (var quotedValue in line.Split('"'))
                {
                    if (!quotedValue.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // В файле решения путь записан обратными слешами Windows: приводим его
                    // к разделителю текущей платформы, чтобы опись читалась и в Linux.
                    var relativePath = quotedValue
                        .Replace('\\', Path.DirectorySeparatorChar)
                        .Replace('/', Path.DirectorySeparatorChar);
                    var fullProjectPath = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath));

                    // Запись в решении может указывать на уже удалённый файл: такую строку
                    // пропускаем, иначе опись падала бы на чужой ошибке в файле решения.
                    if (File.Exists(fullProjectPath))
                        declared.Add(fullProjectPath);
                }
            }
        }

        return declared.ToArray();
    }

    private static IEnumerable<string> EnumerateFilesWithoutIgnoredPathSegments(string root, string searchPattern)
    {
        // Инвентаризация должна учитывать зафиксированные в исходниках
        // идентичности, а не артефакты выполнения вроде TestResults. Собственный
        // обход отсекает игнорируемые каталоги до входа в них, поэтому старые
        // результаты с ограниченными правами не ломают проверку раньше фильтра
        // путей.
        var normalizedRoot = Path.GetFullPath(root);
        if (HasIgnoredPathSegment(normalizedRoot))
            yield break;

        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(normalizedRoot);
        while (pendingDirectories.Count > 0)
        {
            var currentDirectory = pendingDirectories.Pop();
            foreach (var file in Directory.EnumerateFiles(currentDirectory, searchPattern, SearchOption.TopDirectoryOnly))
            {
                if (!HasIgnoredPathSegment(file))
                    yield return file;
            }

            foreach (var childDirectory in Directory.EnumerateDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                if (!HasIgnoredPathSegment(childDirectory))
                    pendingDirectories.Push(childDirectory);
            }
        }
    }

    private static string TrimUtf8Bom(string value)
        => value.Length > 0 && value[0] == '\uFEFF' ? value[1..] : value;

    private sealed record AtomicBatchGroup(string Name, string IntendedBatchId, ImmutableArray<string> Terms);

    private sealed class TestClassAccumulator(string dll, string fullyQualifiedName)
    {
        internal string Dll { get; } = dll;
        internal string FullyQualifiedName { get; } = fullyQualifiedName;
        internal SortedSet<string> SourcePaths { get; } = new(StringComparer.Ordinal);
    }
}

internal sealed record TestSuiteBatchGuardOptions(
    int? ExpectedBatchCount,
    bool RequireLogicalLadderMetadata,
    bool RequireSnapshot,
    bool RequireWeakBaseline,
    bool RequireLeafModeOverrides,
    bool EnforceAtomicGroups,
    bool RequireAtomicGroupMembers,
    string? RequiredLegacyWeakBaselineBodySha256)
{
    internal static TestSuiteBatchGuardOptions Canonical { get; } =
        new(
            ExpectedBatchCount: TestSuiteBatchManifest.CanonicalBatchCount,
            RequireLogicalLadderMetadata: true,
            RequireSnapshot: true,
            RequireWeakBaseline: true,
            RequireLeafModeOverrides: true,
            EnforceAtomicGroups: true,
            RequireAtomicGroupMembers: true,
            RequiredLegacyWeakBaselineBodySha256: TestSuiteBatchManifest.LegacyWeakBaselineBodySha256);

    internal static TestSuiteBatchGuardOptions Synthetic { get; } =
        new(
            ExpectedBatchCount: null,
            RequireLogicalLadderMetadata: false,
            RequireSnapshot: false,
            RequireWeakBaseline: false,
            RequireLeafModeOverrides: false,
            EnforceAtomicGroups: false,
            RequireAtomicGroupMembers: false,
            RequiredLegacyWeakBaselineBodySha256: null);
}

internal sealed record TestSuiteBatchRepositoryInventory(
    ImmutableArray<TestSuiteBatchProject> Projects,
    ImmutableArray<TestSuiteBatchClass> Classes);

internal sealed record TestSuiteBatchProject(
    string ProjectPath,
    string FullProjectPath,
    string Dll,
    string FullProjectDirectory,
    IReadOnlyList<string> SourceFiles);

internal sealed record TestSuiteBatchClass(string Dll, string FullyQualifiedName, string SourcePath)
{
    internal string Identity => Dll + "|" + FullyQualifiedName;
}

internal sealed record TestSuiteBatchRow(
    string Order,
    string BatchId,
    string HumanName,
    string Dll,
    string Filter,
    int MinRows,
    string SplitOnRepeatTimeout,
    string Platform,
    string PlatformReason,
    int LineNumber);

internal sealed record TestSuiteBatchEffectiveRow(
    string BatchId,
    string Dll,
    string RawFilter,
    string EffectiveFilter);

internal sealed record TestSuiteBatchAssignment(
    TestSuiteBatchClass TestClass,
    string BatchId,
    bool HasStrongBatchSignal);

internal sealed record TestSuiteBatchSnapshotRow(
    string Dll,
    string ClassFqn,
    string SourcePath,
    string BatchId)
{
    internal string ToStableLine()
        => string.Join('\t', Dll, ClassFqn, SourcePath, BatchId);
}

internal sealed record TestSuiteBatchReadResult(
    IReadOnlyList<TestSuiteBatchRow> Rows,
    IReadOnlyList<string> Problems);

internal sealed record TestSuiteBatchLeafModeOverrideRow(
    string BatchId,
    string LeafMode,
    string HumanReason,
    int LineNumber);

internal sealed record TestSuiteBatchLeafModeOverrideReadResult(
    IReadOnlyList<TestSuiteBatchLeafModeOverrideRow> Rows,
    IReadOnlyList<string> Problems);

internal sealed record TestSuiteBatchLogicalRuleRow(
    string Order,
    string RuleId,
    string HumanRule,
    int LineNumber);

internal sealed record TestSuiteBatchLogicalBatchRow(
    string Order,
    string LogicalBatchId,
    string HumanName,
    IReadOnlyList<string> PhysicalBatchIds,
    string HumanStatus,
    int LineNumber);

internal sealed record TestSuiteBatchLogicalLadderReadResult(
    string Version,
    int? DeclaredPhysicalRowCount,
    int? DeclaredLogicalBatchCount,
    int? DeclaredRuleCount,
    string AssignmentIdentity,
    string FirstMatchRule,
    string NewTestAuthorObligation,
    IReadOnlyList<TestSuiteBatchLogicalRuleRow> Rules,
    IReadOnlyList<TestSuiteBatchLogicalBatchRow> LogicalBatches,
    IReadOnlyList<string> Problems);

internal sealed record TestSuiteBatchSnapshotReadResult(
    IReadOnlyList<TestSuiteBatchSnapshotRow> Rows,
    IReadOnlyList<string> Problems,
    string BodySha256);

internal sealed record TestSuiteBatchGuardReport(
    IReadOnlyList<TestSuiteBatchProject> Projects,
    IReadOnlyList<TestSuiteBatchClass> Classes,
    IReadOnlyList<TestSuiteBatchRow> Rows,
    IReadOnlyList<TestSuiteBatchAssignment> Assignments,
    IReadOnlyList<string> Problems);
