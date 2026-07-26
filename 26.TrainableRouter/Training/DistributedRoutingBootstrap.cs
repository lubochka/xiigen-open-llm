// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using ModelServer.Contracts;
using ModelServer.Remote;
using TrainableRouter.Contracts;

public static class DistributedRoutingBootstrap
{
    public static async Task<OperationResult<DistributedRoutingRuntime>> BuildDefaultAsync(
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(serverBaseUrl))
            return OperationResult<DistributedRoutingRuntime>.Fail("Server base URL is required.", "SERVER_URL_REQUIRED");

        var training = new RoutingProfileTrainer().Train(OrchestrationRoutingCurriculum.CreateV309RouterBatch50());
        if (!training.IsSuccess)
            return training.MapError<DistributedRoutingRuntime>();

        var locator = new InMemoryRemoteUnitLocator();
        foreach (var manifest in BuildDefaultManifests(serverBaseUrl.TrimEnd('/')))
        {
            var registered = await locator.RegisterAsync(manifest, ct);
            if (!registered.IsSuccess)
                return registered.MapError<DistributedRoutingRuntime>();
        }

        var flow = new TrainableRoutingFlow(
            new KeywordUnitScorer(training.Value!),
            new InMemoryRoutingDecisionLog());

        return OperationResult<DistributedRoutingRuntime>.Ok(new DistributedRoutingRuntime(flow, locator, training.Value!));
    }

    public static async Task<OperationResult<RoutedRemoteModelDecision>> RouteAndLocateAsync(
        DistributedRoutingRuntime runtime,
        RoutingRequest request,
        CancellationToken ct = default)
    {
        if (runtime is null)
            return OperationResult<RoutedRemoteModelDecision>.Fail("Distributed routing runtime is required.", "ROUTING_RUNTIME_REQUIRED");

        var routed = await runtime.Flow.RouteAsync(request, ct);
        if (!routed.IsSuccess)
            return routed.MapError<RoutedRemoteModelDecision>();

        var decision = routed.Value!;
        if (decision.KnowledgeEdge || string.IsNullOrWhiteSpace(decision.ChosenUnitId))
            return OperationResult<RoutedRemoteModelDecision>.Fail(decision.Reason, "ROUTING_KNOWLEDGE_EDGE");

        var manifest = await runtime.Locator.LocateAsync(decision.ChosenUnitId, ct);
        if (!manifest.IsSuccess)
            return manifest.MapError<RoutedRemoteModelDecision>();

        return OperationResult<RoutedRemoteModelDecision>.Ok(new RoutedRemoteModelDecision(decision, manifest.Value!));
    }

    private static RemoteUnitManifest[] BuildDefaultManifests(string serverBaseUrl)
        =>
        [
            BuildManifest(
                OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                "orchestration-planning-unit",
                "orchestration",
                "v309",
                serverBaseUrl,
                "/models/unit-orchestration-plan-v309",
                "manifest-orchestration-v309",
                "orchestration/v309-plan-routing",
                OrchestrationRoutingCurriculum.V309TrainingRunId,
                OrchestrationRoutingCurriculum.V309DatasetSha256,
                "server://tokenizers/orchestration-v309/tokenizer.json"),
            BuildManifest(
                OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
                "translation-unit",
                "translation",
                "v1",
                serverBaseUrl,
                "/models/unit-ru-en-general",
                "manifest-translation-ru-en-v1",
                "translation/ru-en-general",
                "translation-placeholder-run",
                "translation-placeholder-dataset",
                "server://tokenizers/translation-general/tokenizer.json"),
            BuildManifest(
                OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
                "documentation-unit",
                "documentation",
                "v1",
                serverBaseUrl,
                "/models/unit-dotnet-legacy-doc-l1",
                "manifest-dotnet-doc-v1",
                "documentation/dotnet-legacy-l1",
                "documentation-placeholder-run",
                "documentation-placeholder-dataset",
                "server://tokenizers/dotnet-doc/tokenizer.json")
        ];

    private static RemoteUnitManifest BuildManifest(
        string unitId,
        string unitRole,
        string domainTag,
        string version,
        string serverBaseUrl,
        string endpoint,
        string manifestSha256,
        string trainingBranch,
        string trainingRunId,
        string sourceDatasetSha256,
        string tokenizerPath)
        => new(
            unitId,
            unitRole,
            domainTag,
            version,
            serverBaseUrl,
            endpoint,
            manifestSha256,
            trainingBranch,
            trainingRunId,
            sourceDatasetSha256,
            new RemoteModelConfig(2048, 96, 64, 2, 4, 4, 128),
            new RemoteLoraConfig(2, 4f, 0.05f, ["q_proj", "v_proj"]),
            tokenizerPath,
            DateTimeOffset.UtcNow);
}

public sealed record DistributedRoutingRuntime(
    ITrainableRoutingFlow Flow,
    IRemoteUnitLocator Locator,
    IReadOnlyDictionary<string, UnitScoringProfile> Profiles);

public sealed record RoutedRemoteModelDecision(
    RoutingDecision Decision,
    RemoteUnitManifest Manifest);
