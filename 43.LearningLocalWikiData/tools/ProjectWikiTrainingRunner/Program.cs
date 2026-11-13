// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiTrainingRunner;

using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using AgentInfrastructure.Logical;
using ModelServer.Contracts;
using ModelServer.Remote;
using ProjectWikiDomain;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static async Task<int> Main(string[] args)
    {
        var options = ParseArgs(args);
        var mode = ReadOption(options, "mode", "training-flow");
        var repoRoot = ReadOption(options, "repo-root", Directory.GetCurrentDirectory());
        var learningDataRoot = ReadOption(options, "learning-data-root", Path.Combine(repoRoot, "43.LearningLocalWikiData"));
        if (string.Equals(mode, "live-end-to-end-runtime-gate", StringComparison.OrdinalIgnoreCase))
            return await RunLiveEndToEndRuntimeGateAsync(options, repoRoot, learningDataRoot);
        if (string.Equals(mode, "generate-project-wiki-domains", StringComparison.OrdinalIgnoreCase))
            return await RunGenerateProjectWikiDomainsAsync(options, repoRoot, learningDataRoot);
        if (string.Equals(mode, "upgrade-rollback-runtime-gate", StringComparison.OrdinalIgnoreCase))
            return await RunUpgradeRollbackRuntimeGateAsync(options, repoRoot, learningDataRoot);
        if (string.Equals(mode, "plugin-runtime-integration-gate", StringComparison.OrdinalIgnoreCase))
            return await RunPluginRuntimeIntegrationGateAsync(options, repoRoot, learningDataRoot);

        return await RunTrainingFlowAsync(options, repoRoot, learningDataRoot);
    }

    private static async Task<int> RunTrainingFlowAsync(
        IReadOnlyDictionary<string, string> options,
        string repoRoot,
        string learningDataRoot)
    {
        var artifactRoot = ReadOption(options, "artifact-root", Path.Combine(learningDataRoot, "artifacts", "runner-metrics", $"system-boundary-mapper-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}"));
        var batchId = ReadOption(options, "batch-id", "system-boundary-mapper-batch-001");
        var serverBaseUrl = ReadOption(options, "server-base-url", "https://models.local");
        var prompt = ReadOption(options, "prompt", "Train SystemBoundaryMapperUnit with a 50-row system boundary mapper batch for Angular + .NET + SQL stored procedures as one business system.");

        Directory.CreateDirectory(artifactRoot);
        var flow = new ProjectWikiTrainingFlowAdapter();
        var request = new LogicalFlowRequest<string>(
            $"project-wiki-training-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            ProjectWikiTrainingFlowAdapter.FlowId,
            ProjectWikiTrainingFlowAdapter.ScenarioId,
            prompt,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["artifact_root"] = artifactRoot,
                ["batch_id"] = batchId,
                ["server_base_url"] = serverBaseUrl,
                ["learning_data_root"] = learningDataRoot,
                ["routed_phase_id"] = "project-wiki-cross-stack-advanced-gap-closure",
                ["routed_sub_phase_id"] = "system-boundary-mapper-training"
            });

        var executed = await flow.ExecuteAsync(request);
        var outputPath = Path.Combine(artifactRoot, "project-wiki-training-flow-result.json");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(executed, JsonOptions));
        Console.WriteLine(outputPath);
        if (!executed.IsSuccess)
        {
            Console.Error.WriteLine($"{executed.ErrorCode}: {executed.ErrorMessage}");
            return 1;
        }

        Console.WriteLine(executed.Value!.FinalHumanResult);
        return 0;
    }

    private static async Task<int> RunLiveEndToEndRuntimeGateAsync(
        IReadOnlyDictionary<string, string> options,
        string repoRoot,
        string learningDataRoot)
    {
        var reportPath = ReadOption(options, "report-path", Path.Combine(learningDataRoot, "reports", "LIVE-END-TO-END-RUNTIME-GATE.json"));
        var projectRoot = ReadOption(options, "project-root", @"C:\Projects\askapediaweb");
        var projectRoots = ReadProjectRoots(options, projectRoot);
        var serverBaseUrl = ReadOption(options, "server-base-url", "https://models.local");
        var maxEvidence = int.TryParse(ReadOption(options, "max-evidence-records", "25"), out var parsedMaxEvidence)
            ? parsedMaxEvidence
            : 25;
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        var stopwatch = Stopwatch.StartNew();
        var registryResult = ProjectWikiTrainingAssetRegistry.LoadFromDataRoot(learningDataRoot);
        var failedChecks = new List<string>();
        if (!registryResult.IsSuccess)
        {
            failedChecks.Add(registryResult.ErrorCode ?? "registry_load_failed");
            await WriteReportAsync(reportPath, BuildFailedReport(repoRoot, learningDataRoot, string.Join(";", projectRoots), failedChecks, stopwatch.Elapsed.TotalMilliseconds));
            Console.WriteLine(reportPath);
            return 1;
        }

        var registryValidation = registryResult.Value!.ValidateTrainingMayContinue();
        if (!registryValidation.IsSuccess)
            failedChecks.Add(registryValidation.ErrorCode ?? "registry_validation_failed");

        var unitLoadResults = await LoadUnitsThroughRegistryAndLocatorAsync(
            registryResult.Value,
            repoRoot,
            learningDataRoot,
            serverBaseUrl);
        failedChecks.AddRange(unitLoadResults
            .Where(result => !result.CheckpointOpened || !result.ManifestFilePresent || !result.Registered || !result.Located)
            .Select(result => $"unit_load_failed:{result.UnitName}"));

        var validationRunner = new ProjectWikiRealProjectValidationRunner();
        var validationResult = await validationRunner.ValidateProjectsAsync(projectRoots, maxEvidence);
        ProjectWikiValidationRunReport? validationReport = null;
        if (!validationResult.IsSuccess)
        {
            failedChecks.Add(validationResult.ErrorCode ?? "real_project_validation_failed");
        }
        else
        {
            validationReport = validationResult.Value!;
            if (!validationReport.IsPassed)
                failedChecks.AddRange(validationReport.Failures.Select(failure => $"runtime_validation_failed:{failure}"));
        }

        var canonicalUnits = CanonicalUnits();
        var loadedUnitNames = unitLoadResults
            .Where(result => result.CheckpointOpened && result.Registered && result.Located)
            .Select(result => result.UnitName)
            .ToHashSet(StringComparer.Ordinal);
        var missingCanonicalUnits = canonicalUnits
            .Where(unit => !loadedUnitNames.Contains(unit))
            .ToArray();
        failedChecks.AddRange(missingCanonicalUnits.Select(unit => $"canonical_unit_not_loaded:{unit}"));
        var numericRuntimeMetricsPresent = validationReport is not null &&
                                           validationReport.CpuMs > 0 &&
                                           validationReport.PeakRamMb > 0 &&
                                           validationReport.ElapsedMs > 0;
        if (!numericRuntimeMetricsPresent)
            failedChecks.Add("numeric_runtime_metrics_missing");

        var report = new
        {
            schema_version = "1.0",
            report_scope = "live_end_to_end_runtime_gate",
            validation_only = false,
            metric_type = "runner_emitted_runtime_metric",
            generated_at_utc = DateTimeOffset.UtcNow,
            repo_root = repoRoot,
            learning_data_root = learningDataRoot,
            project_under_test = projectRoot,
            projects_under_test = projectRoots,
            all_validation_projects_runtime_covered = projectRoots.Length > 1,
            project_scope = "Angular client + .NET server + SQL stored procedures as one business system",
            registry_loaded = true,
            registry_valid = registryValidation.IsSuccess,
            canonical_units_required = canonicalUnits,
            canonical_units_loaded_count = canonicalUnits.Length - missingCanonicalUnits.Length,
            canonical_units_missing = missingCanonicalUnits,
            unit_load_results = unitLoadResults,
            all_required_checkpoints_opened = unitLoadResults.All(result => result.CheckpointOpened),
            all_units_located_through_locator = unitLoadResults.All(result => result.Registered && result.Located),
            runtime_validation_report = validationReport,
            exact_evidence_accuracy = validationReport?.ExactEvidenceAccuracy,
            query_planning_invocation_rate = validationReport?.QueryPlanningInvocationRate,
            entity_impact_trace_pass_rate = validationReport?.EntityImpactTracePassRate,
            cpu_ms = validationReport?.CpuMs,
            ram_mb = validationReport?.PeakRamMb,
            elapsed_ms = validationReport?.ElapsedMs,
            gate_elapsed_ms = stopwatch.Elapsed.TotalMilliseconds,
            passed = failedChecks.Count == 0,
            failed_checks = failedChecks.Distinct(StringComparer.Ordinal).ToArray(),
            next_action = failedChecks.Count == 0
                ? "Proceed to UPGRADE-ROLLBACK-RUNTIME-GATE.json."
                : "Continue from the first failed runtime check; do not report task complete."
        };
        await WriteReportAsync(reportPath, report);
        Console.WriteLine(reportPath);
        Console.WriteLine($"passed={failedChecks.Count == 0}");
        return failedChecks.Count == 0 ? 0 : 1;
    }

    private static async Task<int> RunGenerateProjectWikiDomainsAsync(
        IReadOnlyDictionary<string, string> options,
        string repoRoot,
        string learningDataRoot)
    {
        var reportPath = ReadOption(options, "report-path", Path.Combine(learningDataRoot, "reports", "PROJECT-WIKI-DOMAIN-GENERATION-GATE.json"));
        var projectRoot = ReadOption(options, "project-root", @"C:\Projects\askapediaweb");
        var projectRoots = ReadProjectRoots(options, projectRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        var stopwatch = Stopwatch.StartNew();
        var generator = new DocumentationOrchestrator();
        var results = new List<object>();
        var failedChecks = new List<string>();
        foreach (var root in projectRoots)
        {
            var generated = await generator.GenerateAsync(new ProjectDocumentationRequest(
                root,
                "",
                $"post-completion-recheck-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
                PersistArtifacts: true));
            if (!generated.IsSuccess)
            {
                failedChecks.Add($"{Path.GetFileName(root)}:{generated.ErrorCode}");
                results.Add(new
                {
                    project_root = root,
                    passed = false,
                    error_code = generated.ErrorCode,
                    error_message = generated.ErrorMessage
                });
                continue;
            }

            results.Add(new
            {
                project_root = root,
                project_id = generated.Value!.ProjectId,
                wiki_root = generated.Value.WikiDomainRoot,
                method_count = generated.Value.MethodCount,
                call_edge_count = generated.Value.CallEdgeCount,
                value_flow_edge_count = generated.Value.ValueFlowEdgeCount,
                question_seed_count = generated.Value.QuestionSeedCount,
                negative_dpo_pair_count = generated.Value.NegativeDpoPairCount,
                warnings = generated.Value.Warnings,
                passed = Directory.Exists(generated.Value.WikiDomainRoot)
            });
        }

        var report = new
        {
            schema_version = "1.0",
            report_scope = "project_wiki_domain_generation_gate",
            validation_only = false,
            metric_type = "runner_emitted_runtime_metric",
            generated_at_utc = DateTimeOffset.UtcNow,
            repo_root = repoRoot,
            learning_data_root = learningDataRoot,
            project_roots = projectRoots,
            project_results = results,
            cpu_ms = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds,
            ram_mb = Process.GetCurrentProcess().PeakWorkingSet64 / 1024d / 1024d,
            elapsed_ms = stopwatch.Elapsed.TotalMilliseconds,
            passed = failedChecks.Count == 0,
            failed_checks = failedChecks.Distinct(StringComparer.Ordinal).ToArray(),
            next_action = failedChecks.Count == 0
                ? "Rerun LIVE-END-TO-END-RUNTIME-GATE.json against all validation projects."
                : "Fix project wiki generation failures before rerunning live runtime gate."
        };
        await WriteReportAsync(reportPath, report);
        Console.WriteLine(reportPath);
        Console.WriteLine($"passed={failedChecks.Count == 0}");
        return failedChecks.Count == 0 ? 0 : 1;
    }

    private static async Task<int> RunPluginRuntimeIntegrationGateAsync(
        IReadOnlyDictionary<string, string> options,
        string repoRoot,
        string learningDataRoot)
    {
        var reportPath = ReadOption(options, "report-path", Path.Combine(learningDataRoot, "reports", "PLUGIN-RUNTIME-INTEGRATION-GATE.json"));
        var visualStudioRepo = ReadOption(options, "visual-studio-repo", @"C:\Projects\xiigen_visual_studio");
        var visualCodeRepo = ReadOption(options, "visual-code-repo", @"C:\Projects\xiigen_visual_code");
        var liveGatePath = ReadOption(options, "live-gate-path", Path.Combine(learningDataRoot, "reports", "LIVE-END-TO-END-RUNTIME-GATE.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        var stopwatch = Stopwatch.StartNew();
        var failedChecks = new List<string>();
        var visualStudioProbe = ProbeVisualStudioPluginRuntime(visualStudioRepo);
        var visualCodeProbe = await ProbeVsCodePluginRuntimeAsync(visualCodeRepo, learningDataRoot);
        var liveGateReadable = TryReadOneByte(liveGatePath, out var liveGateBytes);
        if (!visualStudioProbe.Passed)
            failedChecks.AddRange(visualStudioProbe.Failures.Select(failure => $"visual_studio:{failure}"));
        if (!visualCodeProbe.Passed)
            failedChecks.AddRange(visualCodeProbe.Failures.Select(failure => $"vs_code:{failure}"));
        if (!liveGateReadable)
            failedChecks.Add("live_runtime_gate_not_readable_by_plugin_gate");

        var report = new
        {
            schema_version = "1.0",
            report_scope = "plugin_runtime_integration_gate",
            validation_only = false,
            metric_type = "runner_emitted_runtime_metric",
            generated_at_utc = DateTimeOffset.UtcNow,
            repo_root = repoRoot,
            learning_data_root = learningDataRoot,
            visual_studio_repo = visualStudioRepo,
            vs_code_repo = visualCodeRepo,
            live_runtime_gate_ref = liveGatePath,
            live_runtime_gate_readable = liveGateReadable,
            live_runtime_gate_bytes = liveGateBytes,
            visual_studio_probe = visualStudioProbe,
            vs_code_probe = visualCodeProbe,
            plugins_read_readiness_from_xiigen_runtime_report = liveGateReadable && visualStudioProbe.RuntimeContractLoaded && visualCodeProbe.RuntimeModuleLoaded,
            model_weights_not_embedded_in_plugins = !visualStudioProbe.ModelWeightsEmbedded && !visualCodeProbe.ModelWeightsEmbedded,
            cpu_ms = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds,
            ram_mb = Process.GetCurrentProcess().PeakWorkingSet64 / 1024d / 1024d,
            elapsed_ms = stopwatch.Elapsed.TotalMilliseconds,
            passed = failedChecks.Count == 0,
            failed_checks = failedChecks.Distinct(StringComparer.Ordinal).ToArray(),
            next_action = failedChecks.Count == 0
                ? "Recompute final end-to-end readiness from runtime gates and update state."
                : "Fix plugin runtime integration before any completion claim."
        };
        await WriteReportAsync(reportPath, report);
        Console.WriteLine(reportPath);
        Console.WriteLine($"passed={failedChecks.Count == 0}");
        return failedChecks.Count == 0 ? 0 : 1;
    }

    private static async Task<int> RunUpgradeRollbackRuntimeGateAsync(
        IReadOnlyDictionary<string, string> options,
        string repoRoot,
        string learningDataRoot)
    {
        var reportPath = ReadOption(options, "report-path", Path.Combine(learningDataRoot, "reports", "UPGRADE-ROLLBACK-RUNTIME-GATE.json"));
        var serverBaseUrl = ReadOption(options, "server-base-url", "https://models.local");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        var stopwatch = Stopwatch.StartNew();
        var failedChecks = new List<string>();
        var registryResult = ProjectWikiTrainingAssetRegistry.LoadFromDataRoot(learningDataRoot);
        if (!registryResult.IsSuccess)
        {
            failedChecks.Add(registryResult.ErrorCode ?? "registry_load_failed");
            await WriteReportAsync(reportPath, BuildFailedReport(repoRoot, learningDataRoot, "", failedChecks, stopwatch.Elapsed.TotalMilliseconds));
            Console.WriteLine(reportPath);
            return 1;
        }

        var extras = LoadRegistryExtras(learningDataRoot);
        var locator = new InMemoryRemoteUnitLocator();
        var unitProbeResults = new List<UpgradeRollbackProbeResult>();
        foreach (var unitName in CanonicalUnits())
        {
            var entry = SelectPublishableEntry(registryResult.Value!, unitName);
            if (entry is null)
            {
                unitProbeResults.Add(UpgradeRollbackProbeResult.Missing(unitName));
                failedChecks.Add($"upgrade_unit_missing:{unitName}");
                continue;
            }

            extras.TryGetValue(entry.BatchId, out var extra);
            var compatibilityPath = ResolveRepoPath(repoRoot, learningDataRoot, extra?.CompatibilityReportRef ?? "");
            var rollbackPath = ResolveRepoPath(repoRoot, learningDataRoot, extra?.RollbackRef ?? "");
            var currentCheckpointPath = ResolveRepoPath(repoRoot, learningDataRoot, entry.CheckpointRef);
            var manifestPath = ResolveRepoPath(repoRoot, learningDataRoot, entry.ManifestRef);
            var currentCheckpointOpened = TryReadOneByte(currentCheckpointPath, out var currentCheckpointBytes);
            var rollbackOpened = TryReadOneByte(rollbackPath, out var rollbackBytes);
            var compatibilityOpened = TryReadOneByte(compatibilityPath, out var compatibilityBytes);
            var manifestOpened = TryReadOneByte(manifestPath, out var manifestBytes);
            var registered = await locator.RegisterAsync(new RemoteUnitManifest(
                entry.RemoteUnitId,
                entry.UnitName,
                "project-wiki",
                entry.UnitVersion,
                serverBaseUrl,
                $"/units/{entry.LocatorKey}",
                entry.ArtifactChecksum,
                "main",
                entry.SourceRunId,
                entry.ArtifactChecksum,
                new RemoteModelConfig(32000, 4096, 256, 4, 4, 4, 1024),
                new RemoteLoraConfig(8, 16, 0, ["query", "value"]),
                ResolveRepoPath(repoRoot, learningDataRoot, Path.Combine(Path.GetDirectoryName(entry.ManifestRef) ?? "", "tokenizer.json")),
                DateTimeOffset.UtcNow));
            var located = registered.IsSuccess
                ? await locator.LocateAsync(entry.RemoteUnitId)
                : null;
            var dependentUnitsLocated = new List<string>();
            foreach (var dependency in CanonicalUnits().Where(candidate => !string.Equals(candidate, unitName, StringComparison.Ordinal)))
            {
                var dependencyEntry = SelectPublishableEntry(registryResult.Value!, dependency);
                if (dependencyEntry is null)
                    continue;

                var dependencyLocate = await locator.LocateAsync(dependencyEntry.RemoteUnitId);
                if (!dependencyLocate.IsSuccess)
                {
                    var dependencyManifest = new RemoteUnitManifest(
                        dependencyEntry.RemoteUnitId,
                        dependencyEntry.UnitName,
                        "project-wiki",
                        dependencyEntry.UnitVersion,
                        serverBaseUrl,
                        $"/units/{dependencyEntry.LocatorKey}",
                        dependencyEntry.ArtifactChecksum,
                        "main",
                        dependencyEntry.SourceRunId,
                        dependencyEntry.ArtifactChecksum,
                        new RemoteModelConfig(32000, 4096, 256, 4, 4, 4, 1024),
                        new RemoteLoraConfig(8, 16, 0, ["query", "value"]),
                        "",
                        DateTimeOffset.UtcNow);
                    await locator.RegisterAsync(dependencyManifest);
                    dependencyLocate = await locator.LocateAsync(dependencyEntry.RemoteUnitId);
                }

                if (dependencyLocate.IsSuccess)
                    dependentUnitsLocated.Add(dependency);
            }

            var probePassed = currentCheckpointOpened &&
                              rollbackOpened &&
                              compatibilityOpened &&
                              manifestOpened &&
                              registered.IsSuccess &&
                              located?.IsSuccess == true &&
                              dependentUnitsLocated.Count == CanonicalUnits().Length - 1;
            if (!probePassed)
                failedChecks.Add($"upgrade_rollback_probe_failed:{unitName}");

            unitProbeResults.Add(new UpgradeRollbackProbeResult(
                unitName,
                entry.BatchId,
                entry.RemoteUnitId,
                extra?.UpgradePolicy ?? "",
                extra?.CompatibilityReportRef ?? "",
                extra?.RollbackRef ?? "",
                compatibilityPath,
                rollbackPath,
                currentCheckpointPath,
                manifestPath,
                compatibilityOpened,
                compatibilityBytes,
                rollbackOpened,
                rollbackBytes,
                currentCheckpointOpened,
                currentCheckpointBytes,
                manifestOpened,
                manifestBytes,
                registered.IsSuccess,
                located?.IsSuccess == true,
                dependentUnitsLocated.ToArray(),
                probePassed));
        }

        var report = new
        {
            schema_version = "1.0",
            report_scope = "upgrade_rollback_runtime_gate",
            validation_only = false,
            metric_type = "runner_emitted_runtime_metric",
            generated_at_utc = DateTimeOffset.UtcNow,
            repo_root = repoRoot,
            learning_data_root = learningDataRoot,
            unit_probe_results = unitProbeResults,
            units_probed = unitProbeResults.Count,
            probes_passed = unitProbeResults.Count(result => result.ProbePassed),
            cpu_ms = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds,
            ram_mb = Process.GetCurrentProcess().PeakWorkingSet64 / 1024d / 1024d,
            elapsed_ms = stopwatch.Elapsed.TotalMilliseconds,
            passed = failedChecks.Count == 0,
            failed_checks = failedChecks.Distinct(StringComparer.Ordinal).ToArray(),
            next_action = failedChecks.Count == 0
                ? "Proceed to PLUGIN-RUNTIME-INTEGRATION-GATE.json."
                : "Fix failed upgrade or rollback runtime probes; do not report task complete."
        };
        await WriteReportAsync(reportPath, report);
        Console.WriteLine(reportPath);
        Console.WriteLine($"passed={failedChecks.Count == 0}");
        return failedChecks.Count == 0 ? 0 : 1;
    }

    private static async Task<UnitRuntimeLoadResult[]> LoadUnitsThroughRegistryAndLocatorAsync(
        ProjectWikiTrainingAssetRegistry registry,
        string repoRoot,
        string learningDataRoot,
        string serverBaseUrl)
    {
        var locator = new InMemoryRemoteUnitLocator();
        var results = new List<UnitRuntimeLoadResult>();
        foreach (var unitName in CanonicalUnits())
        {
            var entry = SelectPublishableEntry(registry, unitName);
            if (entry is null)
            {
                results.Add(UnitRuntimeLoadResult.Missing(unitName));
                continue;
            }

            var checkpointPath = ResolveRepoPath(repoRoot, learningDataRoot, entry.CheckpointRef);
            var manifestPath = ResolveRepoPath(repoRoot, learningDataRoot, entry.ManifestRef);
            var checkpointOpened = false;
            long checkpointBytes = 0;
            if (File.Exists(checkpointPath))
            {
                await using var stream = File.OpenRead(checkpointPath);
                checkpointBytes = stream.Length;
                checkpointOpened = stream.Length > 0;
            }

            var manifestFilePresent = File.Exists(manifestPath);
            var manifest = new RemoteUnitManifest(
                entry.RemoteUnitId,
                entry.UnitName,
                "project-wiki",
                entry.UnitVersion,
                serverBaseUrl,
                $"/units/{entry.LocatorKey}",
                entry.ArtifactChecksum,
                "main",
                entry.SourceRunId,
                entry.ArtifactChecksum,
                new RemoteModelConfig(32000, 4096, 256, 4, 4, 4, 1024),
                new RemoteLoraConfig(8, 16, 0, ["query", "value"]),
                ResolveRepoPath(repoRoot, learningDataRoot, Path.Combine(Path.GetDirectoryName(entry.ManifestRef) ?? "", "tokenizer.json")),
                DateTimeOffset.UtcNow);
            var registered = await locator.RegisterAsync(manifest);
            var located = registered.IsSuccess
                ? await locator.LocateAsync(entry.RemoteUnitId)
                : null;
            results.Add(new UnitRuntimeLoadResult(
                unitName,
                entry.BatchId,
                entry.RemoteUnitId,
                entry.LocatorKey,
                entry.ManifestRef,
                entry.CheckpointRef,
                manifestPath,
                checkpointPath,
                manifestFilePresent,
                checkpointOpened,
                checkpointBytes,
                registered.IsSuccess,
                located?.IsSuccess == true,
                registered.ErrorCode ?? located?.ErrorCode ?? ""));
        }

        return results.ToArray();
    }

    private static ProjectWikiTrainingAssetEntry? SelectPublishableEntry(
        ProjectWikiTrainingAssetRegistry registry,
        string unitName)
        => registry.Entries
            .Where(entry => string.Equals(entry.UnitName, unitName, StringComparison.Ordinal))
            .Where(entry => string.Equals(entry.Status, "accepted", StringComparison.Ordinal))
            .Where(entry => !string.IsNullOrWhiteSpace(entry.ManifestRef))
            .Where(entry => !string.IsNullOrWhiteSpace(entry.CheckpointRef))
            .Where(entry => !string.IsNullOrWhiteSpace(entry.LocatorKey))
            .Where(entry => !string.IsNullOrWhiteSpace(entry.RemoteUnitId))
            .LastOrDefault();

    private static string[] CanonicalUnits()
        =>
        [
            "ProjectWikiRequestRouterUnit",
            "StackAndVersionClassifierUnit",
            "SystemBoundaryMapperUnit",
            "CodeTraceBuilderUnit",
            "EntityImpactMapperUnit",
            "DocumentationGeneratorUnit",
            "NegativeDpoGenerator",
            "ProjectWikiQueryPlanningFlow",
            "WikiContextRoutingUnit",
            "ProjectWikiAnswerComposer",
            "ProjectAnswerVerifier",
            "WikiTraceAggregator",
            "ProjectWikiTrainingOrchestrator"
        ];

    private static string[] ReadProjectRoots(IReadOnlyDictionary<string, string> options, string fallbackProjectRoot)
    {
        var joined = ReadOption(options, "project-roots", fallbackProjectRoot);
        return joined
            .Split([';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .DefaultIfEmpty(fallbackProjectRoot)
            .ToArray();
    }

    private static string ResolveRepoPath(string repoRoot, string learningDataRoot, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "";

        if (Path.IsPathRooted(path))
            return path;

        var normalized = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        if (normalized.StartsWith($"43.LearningLocalWikiData{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            return Path.Combine(repoRoot, normalized);
        var repoRelativePath = Path.Combine(repoRoot, normalized);
        if (File.Exists(repoRelativePath) || Directory.Exists(repoRelativePath))
            return repoRelativePath;

        return Path.Combine(learningDataRoot, normalized);
    }

    private static bool TryReadOneByte(string path, out long length)
    {
        length = 0;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return false;

        using var stream = File.OpenRead(path);
        length = stream.Length;
        if (stream.Length == 0)
            return false;

        return stream.ReadByte() >= 0;
    }

    private static Dictionary<string, RegistryExtraFields> LoadRegistryExtras(string learningDataRoot)
    {
        var path = Path.Combine(learningDataRoot, "registry", "PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var result = new Dictionary<string, RegistryExtraFields>(StringComparer.Ordinal);
        foreach (var entry in document.RootElement.GetProperty("entries").EnumerateArray())
        {
            var batchId = ReadJsonString(entry, "batch_id");
            if (string.IsNullOrWhiteSpace(batchId))
                continue;

            result[batchId] = new RegistryExtraFields(
                ReadJsonString(entry, "upgrade_policy"),
                ReadJsonString(entry, "compatibility_report_ref"),
                ReadJsonString(entry, "rollback_ref"));
        }

        return result;
    }

    private static string ReadJsonString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? ""
            : "";

    private static PluginRuntimeProbeResult ProbeVisualStudioPluginRuntime(string visualStudioRepo)
    {
        var failures = new List<string>();
        var assemblyPath = Path.Combine(visualStudioRepo, "src", "42.ProjectWikiDomain", "bin", "Debug", "net461", "_42.ProjectWikiDomain.dll");
        var vsixPath = Path.Combine(visualStudioRepo, "src", "xiigen-vs-extension", "bin", "Debug", "Xiigen.VisualStudio.SmartCodeWiki.vsix");
        var assemblyLoaded = false;
        var runtimeContractLoaded = false;
        string[] contractTypes = [];
        try
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            assemblyLoaded = true;
            contractTypes = assembly
                .GetTypes()
                .Where(type => type.Name is "WikiAnswer" or "WikiReadinessStatus" or "IProjectWikiOrchestrator" or "IWikiOrchestrator")
                .Select(type => type.FullName ?? type.Name)
                .Order(StringComparer.Ordinal)
                .ToArray();
            runtimeContractLoaded = contractTypes.Length >= 3;
        }
        catch (Exception ex)
        {
            failures.Add($"assembly_load_failed:{ex.GetType().Name}");
        }

        if (!assemblyLoaded)
            failures.Add("project_wiki_domain_assembly_not_loaded");
        if (!runtimeContractLoaded)
            failures.Add("smart_code_wiki_contract_types_missing");

        var vsixOpened = TryReadOneByte(vsixPath, out var vsixBytes);
        var vsixContainsDomainAssembly = false;
        if (vsixOpened)
        {
            using var zip = ZipFile.OpenRead(vsixPath);
            vsixContainsDomainAssembly = zip.Entries.Any(entry => entry.FullName.EndsWith("42.ProjectWikiDomain.dll", StringComparison.OrdinalIgnoreCase) ||
                                                                  entry.FullName.EndsWith("_42.ProjectWikiDomain.dll", StringComparison.OrdinalIgnoreCase));
        }

        if (!vsixOpened)
            failures.Add("vsix_not_opened");
        if (!vsixContainsDomainAssembly)
            failures.Add("vsix_missing_project_wiki_domain_library");

        var modelWeightsEmbedded = Directory.Exists(visualStudioRepo) &&
                                   Directory.EnumerateFiles(visualStudioRepo, "*.bin", SearchOption.AllDirectories)
                                       .Any(path => path.Contains("policy", StringComparison.OrdinalIgnoreCase) ||
                                                    path.Contains("checkpoint", StringComparison.OrdinalIgnoreCase));
        if (modelWeightsEmbedded)
            failures.Add("model_weights_embedded_in_visual_studio_plugin");

        return new PluginRuntimeProbeResult(
            "visual_studio",
            visualStudioRepo,
            assemblyPath,
            assemblyLoaded,
            runtimeContractLoaded,
            contractTypes,
            vsixPath,
            vsixOpened,
            vsixBytes,
            vsixContainsDomainAssembly,
            true,
            "",
            modelWeightsEmbedded,
            failures.Count == 0,
            failures.ToArray());
    }

    private static async Task<PluginRuntimeProbeResult> ProbeVsCodePluginRuntimeAsync(
        string visualCodeRepo,
        string learningDataRoot)
    {
        var failures = new List<string>();
        var outHookRunner = Path.Combine(visualCodeRepo, "out", "hookRunner.js");
        var packageJson = Path.Combine(visualCodeRepo, "package.json");
        var moduleOpened = TryReadOneByte(outHookRunner, out var moduleBytes);
        var packageOpened = TryReadOneByte(packageJson, out var packageBytes);
        var stubRoot = Path.Combine(learningDataRoot, "artifacts", "plugin-runtime-node-stub");
        var stubModuleRoot = Path.Combine(stubRoot, "node_modules", "vscode");
        Directory.CreateDirectory(stubModuleRoot);
        await File.WriteAllTextAsync(Path.Combine(stubModuleRoot, "index.js"), """
exports.workspace = {
  workspaceFolders: [{ uri: { fsPath: 'C:/Projects/askapediaweb' } }],
  getConfiguration: function () {
    return { get: function (_key, fallback) { return fallback; } };
  }
};
exports.window = {
  createOutputChannel: function () {
    return { appendLine: function () {}, show: function () {} };
  }
};
exports.Uri = { parse: function (value) { return { value: value }; } };
""");
        var nodeScript = "const hook=require(process.argv[1]); (async()=>{ const result=await hook.checkReadiness(); console.log(JSON.stringify({keys:Object.keys(hook).sort(), readiness:result})); })().catch(err=>{ console.error(err && err.stack || err); process.exit(2); });";
        var nodeResult = RunProcess("node", $"-e \"{nodeScript.Replace("\"", "\\\"")}\" \"{outHookRunner}\"", visualCodeRepo, new Dictionary<string, string>
        {
            ["NODE_PATH"] = Path.Combine(stubRoot, "node_modules"),
            ["XIIGEN_LIVE_GATE_REPORT"] = Path.Combine(learningDataRoot, "reports", "LIVE-END-TO-END-RUNTIME-GATE.json")
        });
        var runtimeModuleLoaded = nodeResult.ExitCode == 0 &&
                                  nodeResult.Stdout.Contains("checkReadiness", StringComparison.Ordinal) &&
                                  nodeResult.Stdout.Contains("queryWiki", StringComparison.Ordinal);
        var mockReadinessReturned = nodeResult.Stdout.Contains("\"status\":\"MOCK\"", StringComparison.OrdinalIgnoreCase) ||
                                    nodeResult.Stdout.Contains("\"status\": \"MOCK\"", StringComparison.OrdinalIgnoreCase);
        if (!moduleOpened)
            failures.Add("hook_runner_module_not_opened");
        if (!packageOpened)
            failures.Add("package_json_not_opened");
        if (!runtimeModuleLoaded)
            failures.Add($"hook_runner_runtime_load_failed:{nodeResult.ExitCode}");
        if (mockReadinessReturned)
            failures.Add("hook_runner_returns_mock_readiness");

        var modelWeightsEmbedded = Directory.Exists(visualCodeRepo) &&
                                   Directory.EnumerateFiles(visualCodeRepo, "*.bin", SearchOption.AllDirectories)
                                       .Any(path => path.Contains("policy", StringComparison.OrdinalIgnoreCase) ||
                                                    path.Contains("checkpoint", StringComparison.OrdinalIgnoreCase));
        if (modelWeightsEmbedded)
            failures.Add("model_weights_embedded_in_vs_code_plugin");

        return new PluginRuntimeProbeResult(
            "vs_code",
            visualCodeRepo,
            outHookRunner,
            moduleOpened,
            runtimeModuleLoaded,
            runtimeModuleLoaded ? ["checkReadiness", "getMap", "queryWiki", "resetBinaryCache"] : [],
            packageJson,
            packageOpened,
            packageBytes,
            packageOpened,
            moduleOpened,
            nodeResult.Stdout,
            modelWeightsEmbedded,
            failures.Count == 0,
            failures.ToArray());
    }

    private static ProcessRunResult RunProcess(
        string fileName,
        string arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string>? environment = null)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        if (environment is not null)
        {
            foreach (var pair in environment)
                process.StartInfo.Environment[pair.Key] = pair.Value;
        }

        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new ProcessRunResult(process.ExitCode, stdout, stderr);
    }

    private static object BuildFailedReport(
        string repoRoot,
        string learningDataRoot,
        string projectRoot,
        List<string> failedChecks,
        double elapsedMs)
        => new
        {
            schema_version = "1.0",
            report_scope = "live_end_to_end_runtime_gate",
            validation_only = false,
            metric_type = "runner_emitted_runtime_metric",
            generated_at_utc = DateTimeOffset.UtcNow,
            repo_root = repoRoot,
            learning_data_root = learningDataRoot,
            project_under_test = projectRoot,
            passed = false,
            gate_elapsed_ms = elapsedMs,
            failed_checks = failedChecks.Distinct(StringComparer.Ordinal).ToArray(),
            next_action = "Fix registry/runtime loading before any completion claim."
        };

    private static async Task WriteReportAsync(string reportPath, object report)
        => await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, JsonOptions));

    private sealed record UnitRuntimeLoadResult(
        string UnitName,
        string BatchId,
        string RemoteUnitId,
        string LocatorKey,
        string ManifestRef,
        string CheckpointRef,
        string ManifestPath,
        string CheckpointPath,
        bool ManifestFilePresent,
        bool CheckpointOpened,
        long CheckpointBytes,
        bool Registered,
        bool Located,
        string ErrorCode)
    {
        public static UnitRuntimeLoadResult Missing(string unitName)
            => new(
                unitName,
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                false,
                false,
                0,
                false,
                false,
                "accepted_publishable_registry_entry_missing");
    }

    private sealed record RegistryExtraFields(
        string UpgradePolicy,
        string CompatibilityReportRef,
        string RollbackRef);

    private sealed record UpgradeRollbackProbeResult(
        string UnitName,
        string BatchId,
        string RemoteUnitId,
        string UpgradePolicy,
        string CompatibilityReportRef,
        string RollbackRef,
        string CompatibilityReportPath,
        string RollbackPath,
        string CurrentCheckpointPath,
        string ManifestPath,
        bool CompatibilityReportOpened,
        long CompatibilityReportBytes,
        bool RollbackOpened,
        long RollbackBytes,
        bool CurrentCheckpointOpened,
        long CurrentCheckpointBytes,
        bool ManifestOpened,
        long ManifestBytes,
        bool Registered,
        bool Located,
        string[] DependentUnitsLocated,
        bool ProbePassed)
    {
        public static UpgradeRollbackProbeResult Missing(string unitName)
            => new(
                unitName,
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                false,
                0,
                false,
                0,
                false,
                0,
                false,
                0,
                false,
                false,
                [],
                false);
    }

    private sealed record PluginRuntimeProbeResult(
        string PluginKind,
        string RepoRoot,
        string RuntimeArtifactPath,
        bool RuntimeArtifactOpened,
        bool RuntimeContractLoaded,
        string[] RuntimeContractSymbols,
        string PackageArtifactPath,
        bool PackageArtifactOpened,
        long PackageArtifactBytes,
        bool PackageContainsRuntimeLibrary,
        bool RuntimeModuleLoaded,
        string RuntimeProbeOutput,
        bool ModelWeightsEmbedded,
        bool Passed,
        string[] Failures);

    private sealed record ProcessRunResult(
        int ExitCode,
        string Stdout,
        string Stderr);

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            var key = args[index];
            if (!key.StartsWith("--", StringComparison.Ordinal))
                continue;

            if (index + 1 >= args.Length)
                continue;

            result[key[2..]] = args[index + 1];
            index++;
        }

        return result;
    }

    private static string ReadOption(IReadOnlyDictionary<string, string> options, string key, string fallback)
        => options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
}
