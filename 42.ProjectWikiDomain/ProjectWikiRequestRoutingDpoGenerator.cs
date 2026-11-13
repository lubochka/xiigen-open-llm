// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectWikiRequestRoutingDpoGenerator
{
    private readonly ProjectWikiRequestRouterUnit _router;

    public ProjectWikiRequestRoutingDpoGenerator(ProjectWikiRequestRouterUnit? router = null)
    {
        _router = router ?? new ProjectWikiRequestRouterUnit();
    }

    public Task<OperationResult<ProjectWikiRequestRoutingDpoBatch>> BuildBatchAsync(
        string batchId = "project-wiki-request-routing-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ProjectWikiRequestRoutingDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var prompts = BuildPrompts(batchId, heldOut);
        var compactOutput = batchId.Contains("compact-output", StringComparison.OrdinalIgnoreCase);
        var signalTokenOutput = batchId.Contains("signal-token", StringComparison.OrdinalIgnoreCase);
        var pairs = prompts.Select((seed, index) =>
        {
            var routed = _router.Route(seed.Prompt);
            var decision = routed.Value!;
            return new ProjectWikiRequestRoutingDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                seed.PromptType,
                seed.Prompt,
                seed.ExpectedRouteKind,
                seed.Prompt,
                BuildChosen(decision, compactOutput, signalTokenOutput),
                BuildRejected(seed.RejectedType, seed.ExpectedRouteKind, signalTokenOutput),
                seed.RejectedType);
        }).ToArray();
        var wrongAnnotationRate = pairs.Length == 0
            ? 0
            : pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");
        foreach (var mismatch in pairs
                     .Select(pair => new { Pair = pair, Actual = _router.Route(pair.Prompt).Value!.RouteKind })
                     .Where(item => item.Pair.ExpectedRouteKind != item.Actual))
            warnings.Add($"router_seed_expectation_mismatch:{mismatch.Pair.PairId}:expected={mismatch.Pair.ExpectedRouteKind}:actual={mismatch.Actual}:prompt={mismatch.Pair.Prompt}");

        return Task.FromResult(OperationResult<ProjectWikiRequestRoutingDpoBatch>.Ok(new ProjectWikiRequestRoutingDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())));
    }

    private static string BuildChosen(ProjectWikiRequestRoutingDecision decision, bool compactOutput, bool signalTokenOutput)
        => signalTokenOutput
            ? $"accepted_route_label: ROUTE_OK_{decision.RouteKind}\nroute_kind: {decision.RouteKind}\ntarget_unit_id: {decision.TargetUnitId}\nroute_quality: correct_trainable_unit_selected\nconfidence: {decision.Confidence:0.######}"
            : compactOutput
            ? $"[CORRECT: route_to_trainable_project_wiki_unit]\nroute_kind: {decision.RouteKind}\ntarget_unit_id: {decision.TargetUnitId}\nconfidence: {decision.Confidence:0.######}"
            : $"[CORRECT: route_to_trainable_project_wiki_unit]\nroute_kind: {decision.RouteKind}\ntarget_unit_id: {decision.TargetUnitId}\nreason: {decision.Reason}\nrequired_inputs: {string.Join(", ", decision.RequiredInputs)}\nconfidence: {decision.Confidence:0.######}";

    private static string BuildRejected(string rejectedType, ProjectWikiRequestRouteKind expected, bool signalTokenOutput)
        => signalTokenOutput
            ? BuildSignalRejected(rejectedType, expected)
            : BuildRejected(rejectedType, expected);

    private static string BuildRejected(string rejectedType, ProjectWikiRequestRouteKind expected)
        => rejectedType switch
        {
            "route_to_static_script" =>
                BuildStaticScriptRejected(expected),
            "wrong_training_unit" =>
                BuildWrongTrainingUnitRejected(expected),
            "route_non_wiki_to_project_wiki" =>
                BuildStructuredRejected(
                    "DocumentationBuild",
                    "project-documentation-pipeline-orchestrator",
                    "Treat the unrelated request as if it were project code documentation.",
                    "project_root",
                    0.42,
                    "route_non_wiki_to_project_wiki",
                    "This sends non-project work into local wiki training and risks invented project citations."),
            "route_validation_to_answer" =>
                BuildStructuredRejected(
                    "AnswerComposerTraining",
                    "project-wiki-answer-composer-unit",
                    "Answer one sample question and mark validation passed.",
                    "retrieved_evidence, answer_contract",
                    0.49,
            "route_validation_to_answer",
                    "Validation must run full project coverage gates, exact evidence checks, and required question variations, not compose one sample answer."),
            "route_to_answer_without_planning" =>
                BuildStructuredRejected(
                    "AnswerComposerTraining",
                    "project-wiki-answer-composer-unit",
                    $"Route {expected} directly to answer composition without query planning.",
                    "retrieved_evidence, answer_contract",
                    0.48,
                    "route_to_answer_without_planning",
                    "It skips the unit that decides which evidence searches are required before answering."),
            "route_training_to_project_repo" =>
                BuildStructuredRejected(
                    expected.ToString(),
                    "project-local-adapter",
                    "Store this general training unit inside the current project repository.",
                    "project_root",
                    0.41,
                    "route_training_to_project_repo",
                    "General algorithm training belongs in llm_mvp_core, not in a project-local adapter."),
            "route_project_unit_to_core" =>
                BuildStructuredRejected(
                    expected.ToString(),
                    "llm_mvp_core-global-unit",
                    "Store this project-specific wiki adapter in llm_mvp_core.",
                    "global_training_root",
                    0.41,
                    "route_project_unit_to_core",
                    "Project-local weights must live with the repository they document."),
            "route_to_non_wiki" =>
                BuildStructuredRejected(
                    "NotProjectWiki",
                    "main-orchestrator",
                    "Classify this project-local wiki task as unrelated general orchestration.",
                    "none",
                    0.43,
                    "route_to_non_wiki",
                    "It loses the documentation/wiki pipeline and its evidence gates."),
            _ =>
                BuildStructuredRejected(
                    expected.ToString(),
                    "unspecified-unit",
                    $"Choose any available project wiki route for {expected}.",
                    "unknown",
                    0.4,
                    "underspecified_request_route",
                    "It makes the executor stop or ask instead of selecting the next trainable unit.")
        };

    private static string BuildSignalRejected(string rejectedType, ProjectWikiRequestRouteKind expected)
        => rejectedType switch
        {
            "wrong_training_unit" =>
                $"rejected_route_label: ROUTE_WRONG_UNIT\nwrong_route_kind: {SelectWrongRouteKind(expected)}\nexpected_route_kind: {expected}\nroute_quality: wrong_trainable_unit_selected\n[WRONG: wrong_training_unit - The route sends the executor to a different wiki unit than the prompt asks to train.]",
            "route_to_static_script" =>
                $"rejected_route_label: ROUTE_STATIC_SCRIPT\nwrong_route_kind: StaticScript\nexpected_route_kind: {expected}\nroute_quality: static_non_trainable_path_selected\n[WRONG: route_to_static_script - The route bypasses trainable project-wiki units and cannot improve model capability.]",
            "route_non_wiki_to_project_wiki" =>
                $"rejected_route_label: ROUTE_FALSE_PROJECT_WIKI\nwrong_route_kind: DocumentationBuild\nexpected_route_kind: {expected}\nroute_quality: non_wiki_prompt_sent_to_project_wiki\n[WRONG: route_non_wiki_to_project_wiki - The route invents a project-local wiki task for a non-wiki prompt.]",
            "route_validation_to_answer" =>
                $"rejected_route_label: ROUTE_VALIDATION_AS_ANSWER\nwrong_route_kind: AnswerComposerTraining\nexpected_route_kind: {expected}\nroute_quality: validation_gate_skipped\n[WRONG: route_validation_to_answer - The route composes an answer instead of running validation gates.]",
            "route_to_answer_without_planning" =>
                $"rejected_route_label: ROUTE_SKIP_QUERY_PLANNING\nwrong_route_kind: AnswerComposerTraining\nexpected_route_kind: {expected}\nroute_quality: query_planning_skipped\n[WRONG: route_to_answer_without_planning - The route answers before planning the wiki evidence searches.]",
            "route_training_to_project_repo" =>
                $"rejected_route_label: ROUTE_WRONG_STORAGE_PROJECT_REPO\nwrong_route_kind: {expected}\nexpected_route_kind: {expected}\nroute_quality: generic_training_stored_in_project_repo\n[WRONG: route_training_to_project_repo - Generic algorithm training belongs in llm_mvp_core with export/import metadata.]",
            "route_project_unit_to_core" =>
                $"rejected_route_label: ROUTE_WRONG_STORAGE_CORE\nwrong_route_kind: {expected}\nexpected_route_kind: {expected}\nroute_quality: project_specific_adapter_stored_in_core\n[WRONG: route_project_unit_to_core - Project-local adapters belong with the repository they document.]",
            "route_to_non_wiki" =>
                $"rejected_route_label: ROUTE_DROP_PROJECT_WIKI\nwrong_route_kind: NotProjectWiki\nexpected_route_kind: {expected}\nroute_quality: wiki_task_dropped\n[WRONG: route_to_non_wiki - The route loses the project wiki training or answering path.]",
            _ =>
                $"rejected_route_label: ROUTE_UNDERSPECIFIED\nwrong_route_kind: UNKNOWN\nexpected_route_kind: {expected}\nroute_quality: underspecified_or_pause_path\n[WRONG: underspecified_request_route - The route fails to choose the exact next trainable unit.]"
        };

    private static ProjectWikiRequestRouteKind SelectWrongRouteKind(ProjectWikiRequestRouteKind expected)
        => expected switch
        {
            ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining => ProjectWikiRequestRouteKind.DocumentationBuild,
            ProjectWikiRequestRouteKind.DocumentationBuild => ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining,
            ProjectWikiRequestRouteKind.ProjectValidation => ProjectWikiRequestRouteKind.AnswerComposerTraining,
            ProjectWikiRequestRouteKind.AnswerComposerTraining => ProjectWikiRequestRouteKind.QueryPlanningTraining,
            ProjectWikiRequestRouteKind.QueryPlanningTraining => ProjectWikiRequestRouteKind.AnswerComposerTraining,
            ProjectWikiRequestRouteKind.NotProjectWiki => ProjectWikiRequestRouteKind.DocumentationBuild,
            _ => ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining
        };

    private static string BuildStaticScriptRejected(ProjectWikiRequestRouteKind expected)
        => expected switch
        {
            ProjectWikiRequestRouteKind.DocumentationBuild =>
                BuildStructuredRejected(
                    "StaticScript",
                    "none",
                    "Run a one-off documentation script and skip trainable wiki traces.",
                    "script_path",
                    0.39,
                    "route_to_static_script_documentation_build",
                    "It bypasses DocumentationOrchestrator and prevents the documentation unit from learning from project evidence."),
            ProjectWikiRequestRouteKind.ProjectValidation =>
                BuildStructuredRejected(
                    "StaticScript",
                    "none",
                    "Use a one-off grep report as validation and skip exact evidence gates.",
                    "script_path",
                    0.38,
                    "route_to_static_script_project_validation",
                    "It turns validation into a file-count check instead of proving wiki answer coverage."),
            _ =>
                BuildStructuredRejected(
                    "StaticScript",
                    "none",
                    $"Route {expected} to a one-off script and skip trainable unit selection.",
                    "script_path",
                    0.39,
                    "route_to_static_script",
                    "It bypasses the learned wiki orchestration units and prevents traceable improvement.")
        };

    private static string BuildWrongTrainingUnitRejected(ProjectWikiRequestRouteKind expected)
        => expected switch
        {
            ProjectWikiRequestRouteKind.StackVersionTraining =>
                BuildStructuredRejected(
                    "DocumentationGeneratorTraining",
                    "project-documentation-generator-unit",
                    "Send stack/version prompts to documentation generation and ignore csproj evidence.",
                    "accepted_documentation",
                    0.46,
                    "wrong_training_unit_stack_version_to_documentation_generator",
                    "It teaches prose generation instead of framework and CSharp version classification."),
            ProjectWikiRequestRouteKind.EntityImpactTraining =>
                BuildStructuredRejected(
                    "DocumentationGeneratorTraining",
                    "project-documentation-generator-unit",
                    "Send entity impact prompts to documentation generation and only summarize the entity name.",
                    "accepted_documentation",
                    0.46,
                    "wrong_training_unit_entity_impact_to_documentation_generator",
                    "It misses readers, writers, validators, persistence, callers, and tests needed for change-impact reasoning."),
            ProjectWikiRequestRouteKind.DocumentationGeneratorTraining =>
                BuildStructuredRejected(
                    "AnswerVerifierTraining",
                    "project-answer-verifier",
                    "Send documentation-generator prompts to answer verification and only check citations.",
                    "answer, evidence_ids",
                    0.45,
                    "wrong_training_unit_documentation_generator_to_answer_verifier",
                    "It verifies answers but does not teach complete meaning, location, parameters, returns, callers, and callees."),
            ProjectWikiRequestRouteKind.AnswerVerifierTraining =>
                BuildStructuredRejected(
                    "AnswerComposerTraining",
                    "project-wiki-answer-composer-unit",
                    "Compose a nicer answer instead of verifying evidence ids.",
                    "candidate_answer",
                    0.31,
                    "wrong_training_unit_answer_verifier_to_answer_composer",
                    "It composes text but does not detect wrong-project, stale, or hallucinated citations."),
            ProjectWikiRequestRouteKind.TraceAggregatorTraining =>
                BuildStructuredRejected(
                    "ProjectWikiRequestRouterTraining",
                    "project-wiki-request-router-unit",
                    "Send trace-aggregation prompts to request routing and only classify the next route.",
                    "executor_prompt",
                    0.45,
                    "wrong_training_unit_trace_aggregator_to_request_router",
                    "It routes the prompt but does not read WikiUnitTrace failures or choose remediation batches."),
            ProjectWikiRequestRouteKind.AnswerComposerTraining =>
                BuildStructuredRejected(
                    "QueryPlanningTraining",
                    "project-wiki-query-planning-flow",
                    "Send answer-composer prompts to query planning and stop after listing searches.",
                    "question, project_scope",
                    0.47,
                    "wrong_training_unit_answer_composer_to_query_planning",
                    "Query planning finds evidence but does not compose a grounded final answer from accepted evidence."),
            _ =>
                BuildStructuredRejected(
                    "OtherWikiTraining",
                    "wrong-trainable-unit",
                    $"Send {expected} prompts to another wiki unit just because it is trainable.",
                    "wrong_unit_inputs",
                    0.44,
                    "wrong_training_unit",
                    "It trains the wrong responsibility boundary and makes later unit metrics meaningless.")
        };

    private static string BuildStructuredRejected(
        string routeKind,
        string targetUnitId,
        string reason,
        string requiredInputs,
        double confidence,
        string wrongType,
        string consequence)
        => $"route_kind: {routeKind}\n" +
           $"target_unit_id: {targetUnitId}\n" +
           $"reason: {reason}\n" +
           $"required_inputs: {requiredInputs}\n" +
           $"confidence: {confidence:0.##}\n" +
           $"[WRONG: {wrongType} - {consequence}]";

    private static RequestRoutingSeed[] BuildPrompts(string batchId, bool heldOut)
    {
        if (batchId.Contains("remediation-002", StringComparison.OrdinalIgnoreCase) ||
            batchId.Contains("weak-route-replay", StringComparison.OrdinalIgnoreCase))
            return BuildFocusedRemediationPrompts(heldOut);
        if (batchId.Contains("weak-target", StringComparison.OrdinalIgnoreCase))
            return BuildWeakTargetPrompts(heldOut);
        if (batchId.Contains("validation-focus", StringComparison.OrdinalIgnoreCase))
            return BuildValidationFocusPrompts(heldOut);
        if (batchId.Contains("precision-patch", StringComparison.OrdinalIgnoreCase))
            return BuildPrecisionPatchPrompts(heldOut);
        if (batchId.Contains("diverse-sibling", StringComparison.OrdinalIgnoreCase))
            return BuildDiverseSiblingPrompts(heldOut);
        if (batchId.Contains("weak-doc-validation-trace", StringComparison.OrdinalIgnoreCase))
            return heldOut ? BuildFocusedRemediationPrompts(true) : BuildWeakDocValidationTracePrompts();
        if (batchId.Contains("docgen-contrast", StringComparison.OrdinalIgnoreCase))
            return heldOut ? BuildFocusedRemediationPrompts(true) : BuildDocumentationGeneratorContrastPrompts();
        if (batchId.Contains("near-miss-contrast", StringComparison.OrdinalIgnoreCase))
            return heldOut ? BuildFocusedRemediationPrompts(true) : BuildNearMissContrastPrompts();
        if (batchId.Contains("weak-train-balanced-heldout", StringComparison.OrdinalIgnoreCase))
            return heldOut ? BuildFocusedRemediationPrompts(true) : BuildWeakTrainBalancedHeldOutPrompts();

        return (heldOut ? HeldOutPrompts : TrainingPrompts).Take(50).ToArray();
    }

    private static RequestRoutingSeed[] BuildPrecisionPatchPrompts(bool heldOut)
    {
        var prefix = heldOut ? "Held-out precision patch: " : "Precision patch: ";
        var rows = new List<RequestRoutingSeed>(50);

        Add(rows, "project_validation", ProjectWikiRequestRouteKind.ProjectValidation, "route_validation_to_answer", prefix,
            "Run the project wiki validation gate over every required question variation, then persist failed categories.",
            "Validate all target repositories with exact evidence accuracy, route accuracy, query plan accuracy, and answer verifier accuracy.",
            "Check the full wiki coverage gate for entity impact, stored procedure impact, stack version, missing evidence, and workflow trace prompts.",
            "Run project wiki validation across askapediaweb, GenieDynamicServer, wizGameserver, geniesoftGameMVPServer, and genietasks before any done claim.",
            "Measure real project wiki validation and list uncovered methods, edges, SQL procedures, and prompt variations.",
            "Validate project wiki coverage so local wiki answers stay inside project evidence and pass all advanced developer question types.",
            "Run a ten-batch project validation checkpoint and update WikiQueryTrace plus WikiUnitTrace.",
            "Verify project-local wiki coverage after training; do not answer one sample question and call validation complete.");

        Add(rows, "answer_composer", ProjectWikiRequestRouteKind.AnswerComposerTraining, "wrong_training_unit", prefix,
            "Train answer composer to turn retrieved entity touchpoints into a developer-facing impact answer.",
            "Improve final wiki answers after query planning retrieves accepted evidence ids.",
            "Teach answer composition for stored procedure impact once SQL evidence has been retrieved.",
            "Train the answer unit to explain callers, callees, parameters, returns, and side effects from accepted evidence.",
            "Improve citation-safe answer prose with abstention when evidence is partial.",
            "Teach answer composer to synthesize workflow trace evidence into a readable project answer.",
            "Train final wiki answer generation after BM25 or semantic context retrieval completes.",
            "Improve answer composer for field-change impact summaries after query planning returns touchpoints.");

        Add(rows, "documentation_generator", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "wrong_training_unit", prefix,
            "Train documentation generator to produce meaning, location, parameters, returns, callers, and callees for each method.",
            "Improve documentation records so callee summaries flow upward to callers.",
            "Teach documentation generator to record stored procedure name, SQL params, result shape, and side effects.",
            "Train bottom-up documentation fields before the wiki uses them as answer evidence.",
            "Improve documentation of repository methods that call SQL stored procedures.",
            "Teach documentation generator to include source location, framework version, and evidence id.",
            "Train complete documentation rows for legacy WebForms, MVC, WPF, and API code.");

        Add(rows, "documentation_build", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script", prefix,
            "Build project-local wiki documentation through DocumentationOrchestrator and keep trainable traces.",
            "Build documentation for askapediaweb through the full code trace, documentation, evidence, and local wiki pipeline.",
            "Run documentation pipeline for legacy .NET repositories instead of a static file-count report.",
            "Create bottom-up project documentation and persist evidence-index plus route and unit traces.",
            "Build local wiki documentation for server code with callers, callees, framework version, and SQL procedure evidence.");

        Add(rows, "stack_version", ProjectWikiRequestRouteKind.StackVersionTraining, "wrong_training_unit", prefix,
            "Train stack/version classifier from csproj target framework, CSharp version, MVC, WPF, WebForms, and API cues.",
            "Improve framework detection for .NET Framework 3.5, 4.5.2, 4.6.1, .NET Core, WebForms, MVC, and WPF.",
            "Teach StackAndVersionClassifierUnit to read project files before documentation generation.",
            "Train stack/version classification so wiki docs know the server/client stack and language level.",
            "Remediate target-framework classification failures using stack and version classifier rows.");

        Add(rows, "entity_impact", ProjectWikiRequestRouteKind.EntityImpactTraining, "wrong_training_unit", prefix,
            "Train entity impact mapper for adding a new field to CampaignDto and following every touchpoint.",
            "Improve touchpoint maps across readers, writers, validators, mappers, persistence, API contracts, UI binding, and tests.",
            "Teach field-change reasoning through DTO, mapper, repository, stored procedure, controller, and caller workflows.",
            "Train entity impact rows that explain what changes when a legacy model property is added.");

        Add(rows, "answer_verifier", ProjectWikiRequestRouteKind.AnswerVerifierTraining, "wrong_training_unit", prefix,
            "Train answer verifier to reject hallucinated citations, stale documentation, and wrong-project evidence ids.",
            "Improve verifier checks for missing evidence abstention and unsupported stored procedure impact claims.",
            "Teach ProjectAnswerVerifier to fail answers that cite evidence outside the project repository.",
            "Train verifier rows where rejected output passes an answer with no exact evidence.");

        Add(rows, "trace_aggregator", ProjectWikiRequestRouteKind.TraceAggregatorTraining, "wrong_training_unit", prefix,
            "Train WikiTraceAggregator to read failed unit traces and choose the next remediation batch.",
            "Improve trace aggregation when ProjectWikiRequestRouter accuracy stalls below the target threshold.",
            "Teach trace aggregator to name the failed wiki unit instead of continuing blind training.",
            "Train WikiUnitTrace summaries that decide whether to source data, validate projects, or run another 50 rows.");

        Add(rows, "not_project_wiki", ProjectWikiRequestRouteKind.NotProjectWiki, "route_non_wiki_to_project_wiki", prefix,
            "Translate this paragraph from Russian to English.",
            "Create a travel itinerary for Rome.",
            "Write a marketing headline for a bakery.",
            "Explain sourdough starter maintenance.",
            "Draft a short thank-you note for a conference speaker.");

        return rows.Take(50).ToArray();
    }

    private static RequestRoutingSeed[] BuildWeakTrainBalancedHeldOutPrompts()
    {
        var rows = new List<RequestRoutingSeed>(50);

        Add(rows, "project_validation", ProjectWikiRequestRouteKind.ProjectValidation, "route_validation_to_answer", "",
            "Validate all five .NET projects with the full wiki question variation suite.",
            "Validate stored procedure wiki coverage on DNN-style SQL evidence.",
            "Run coverage gate and persist WikiQueryTrace plus WikiUnitTrace for each project.",
            "Run exact evidence validation for askapediaweb, GenieDynamicServer, wizGameserver, geniesoftGameMVPServer, and genietasks.",
            "Check whether generated wiki answers all 45 advanced prompt variations with evidence ids.",
            "Measure project wiki coverage for entity impact, workflow trace, stored procedure, stack/version, and missing-evidence questions.",
            "After ten batches, run real project validation and list uncovered methods and edges.",
            "Check that project-local wiki routes every question through query planning before answering.",
            "Run project wiki validation for all repositories and record the failed question classes.",
            "Validate exact evidence coverage for every required project-local wiki prompt variation.",
            "Run the real-project coverage gate and persist WikiQueryTrace plus WikiUnitTrace.",
            "Verify all project wiki answers against evidence ids, project boundaries, and stale-version rules.",
            "Run validation after the latest ten batches and list uncovered methods, edges, and prompt families.",
            "Validate project wiki coverage so local wiki answers stay inside project evidence and pass all advanced developer question types.",
            "Check the full wiki coverage gate for entity impact, stored procedure impact, stack version, missing evidence, and workflow trace prompts.",
            "Run project wiki validation across askapediaweb, GenieDynamicServer, wizGameserver, geniesoftGameMVPServer, and genietasks before any done claim.",
            "Measure real project wiki validation and list uncovered methods, edges, SQL procedures, and prompt variations.",
            "Run a ten-batch project validation checkpoint and update WikiQueryTrace plus WikiUnitTrace.",
            "Verify project-local wiki coverage after training; do not answer one sample question and call validation complete.",
            "Run full project wiki validation across all target repositories before reporting status.");

        Add(rows, "documentation_generator", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "wrong_training_unit", "",
            "Improve bottom-up documentation rows for methods, callers, and higher-level workflow summaries.",
            "Teach DocumentationGeneratorUnit to include stored procedure name, SQL params, result shape, and side effects when present.",
            "Train documentation generator to fill meaning, location, parameters, returns, callers, callees, and stack version.",
            "Train documentation fields so callers and return usage are always included.",
            "Train complete documentation fields before project validation uses the code as test evidence.",
            "Train the documentation generator that creates method records from source code.",
            "Improve documentation generation for callers, callees, parameters, returns, and side effects.",
            "Teach stored procedure documentation fields: procedure name, SQL parameters, result shape, and writes.",
            "Improve documentation generator rows for repository, controller, service, and mapper methods.",
            "Train documentation generator to describe method meaning, location, parameters, returns, callers, and callees.",
            "Improve documentation rows for stored procedures, SQL parameters, result sets, and side effects.",
            "Train complete method documentation before it becomes evidence for wiki answers.",
            "Teach documentation generator to avoid missing return usage and caller context.",
            "Train documentation generator to record code location and evidence id for every summary.",
            "Train bottom-up documentation records for legacy WebForms and MVC projects.");

        Add(rows, "trace_aggregator", ProjectWikiRequestRouteKind.TraceAggregatorTraining, "wrong_training_unit", "",
            "Build rows where the aggregator must stop blind training and name the failed wiki unit.",
            "Train WikiTraceAggregator to read failed unit traces and choose the next remediation batch.",
            "Improve trace aggregation so low ProjectWikiRequestRouter accuracy triggers more routing rows.",
            "Teach WikiUnitTrace summary to decide whether project validation or data sourcing is next.",
            "Train the trace aggregator that reads WikiUnitTrace and selects the next remediation batch.",
            "Improve aggregation of failed ProjectWikiRequestRouter, DocumentationGenerator, and ProjectValidation metrics.",
            "Teach WikiTraceAggregator to identify the weakest unit rather than classify a single prompt.",
            "Build trace aggregation rows that choose sourcing, validation, or another 50-row training step.",
            "Train WikiTraceAggregator to read failed unit traces and choose the next remediation batch.",
            "Improve trace aggregation when ProjectWikiRequestRouter accuracy stalls below the target threshold.");

        Add(rows, "answer_composer", ProjectWikiRequestRouteKind.AnswerComposerTraining, "wrong_training_unit", "",
            "Teach citation-safe answers for stored procedure impact questions.",
            "Improve grounded wiki answers with citations, missing-evidence abstention, and project boundary checks.",
            "Train the answer composer to explain code only from retrieved evidence ids.",
            "Teach answer composition for entity impact questions after query planning retrieves touchpoints.",
            "Improve the unit that turns accepted evidence into a developer-facing answer.");

        return rows.Take(50).ToArray();
    }

    private static RequestRoutingSeed[] BuildNearMissContrastPrompts()
    {
        var rows = new List<RequestRoutingSeed>(50);

        Add(rows, "documentation_build", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script",
            "",
            "Route remediation: build documentation for GenieDynamicServer and persist evidence index, question seeds, and unit traces.",
            "Build documentation for a legacy .NET repository and keep evidence-index, question-seeds, and unit trace artifacts.",
            "Run DocumentationOrchestrator for a project and persist evidence, questions, and unit metrics.",
            "Build documentation for a project with persisted evidence index and training traces.",
            "Build repository documentation through the trainable wiki pipeline, not through a one-off static script.");

        Add(rows, "answer_composer", ProjectWikiRequestRouteKind.AnswerComposerTraining, "wrong_training_unit",
            "",
            "Route remediation: improve grounded wiki answers with citations, missing-evidence abstention, and project boundary checks.",
            "Train answer composer for citation-safe project wiki answers after evidence retrieval.",
            "Improve final local-wiki answer text with project boundary wording and missing evidence abstention.",
            "Teach answer composition to turn retrieved evidence ids into a developer-facing explanation.",
            "Train grounded answer prose after query planning returns accepted evidence.");

        Add(rows, "project_validation", ProjectWikiRequestRouteKind.ProjectValidation, "route_validation_to_answer",
            "",
            "Route remediation: validate all five .NET projects with the complete wiki question variation suite.",
            "Run exact evidence validation across askapediaweb, GenieDynamicServer, wizGameserver, geniesoftGameMVPServer, and genietasks.",
            "Check generated wiki answers against all advanced prompt variations and evidence ids.",
            "Measure wiki coverage for entity impact, workflow trace, SQL procedure, stack/version, and missing-evidence questions.",
            "Run ten-batch project validation and list uncovered methods, edges, SQL procedures, and prompt families.",
            "Validate stored procedure wiki coverage from accepted SQL evidence before any done claim.",
            "Ensure project-local wiki routes every question through query planning before answer composition.",
            "Run coverage gate and persist WikiQueryTrace plus WikiUnitTrace for each target repository.",
            "Validate all required wiki prompts against exact evidence coverage and project boundaries.",
            "Run full project validation rather than answering one representative question.");

        Add(rows, "documentation_generator", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "wrong_training_unit",
            "",
            "Route remediation: improve bottom-up documentation rows for methods, callers, and workflow summaries.",
            "Remediate DocumentationGeneratorUnit so stored procedure name, SQL params, result shape, and side effects are documented.",
            "Teach documentation generator to include stored procedure name, SQL parameters, result shape, and side effects when present.",
            "Improve bottom-up method documentation for callers, callees, parameters, returns, and higher-level workflow summaries.",
            "Train documentation generator rows that explain methods, caller context, and callee meaning before answer verification.",
            "Train documentation generator for stored procedure wrappers with procedure name, SQL params, result shape, writes, and side effects.",
            "Improve documentation generator fields for methods whose callers use returned values in workflow summaries.",
            "Teach DocumentationGeneratorUnit to produce evidence records before any ProjectAnswerVerifier citation check.",
            "Train documentation rows for source location, method meaning, parameters, returns, callers, and callees.",
            "Remediate documentation generation so complete method records become usable wiki evidence.",
            "Improve documentation generator output for bottom-up leaf functions and their direct callers.",
            "Train documentation generator to capture stored procedure access and return-shape meaning.");

        Add(rows, "trace_aggregator", ProjectWikiRequestRouteKind.TraceAggregatorTraining, "wrong_training_unit",
            "",
            "Route remediation: train WikiTraceAggregator to stop blind training and name the failed wiki unit.",
            "Build trace aggregation rows where the aggregator names the failed unit and chooses the next remediation batch.",
            "Teach WikiTraceAggregator to read failed unit traces instead of routing one executor prompt.",
            "Improve trace aggregation so ProjectWikiRequestRouter stalls trigger routing remediation rows.",
            "Train WikiUnitTrace summaries that choose sourcing, validation, or another 50-row training step.",
            "Aggregate WikiQueryTrace and WikiUnitTrace failures before selecting the next wiki unit to train.");

        Add(rows, "not_project_wiki", ProjectWikiRequestRouteKind.NotProjectWiki, "route_non_wiki_to_project_wiki",
            "",
            "Translate this Russian paragraph to English.",
            "Create a marketing homepage for a bakery.",
            "Explain how sourdough starter works.",
            "Make a travel itinerary for Rome.",
            "Write a short birthday greeting.",
            "Generate a generic SQL example without a project repository.",
            "Summarize a public article.",
            "Create a shopping list.",
            "Draft an email reminder.",
            "Explain how rain forms.",
            "Write a product tagline.",
            "Plan a weekend walk in Tel Aviv.");

        return rows.Take(50).ToArray();
    }

    private static RequestRoutingSeed[] BuildDocumentationGeneratorContrastPrompts()
    {
        var rows = new List<RequestRoutingSeed>(50);

        Add(rows, "documentation_generator", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "wrong_training_unit",
            "",
            "Train documentation generator, not answer verifier, to produce meaning, location, parameters, returns, callers, and callees.",
            "Teach DocumentationGeneratorUnit to create method documentation records before any citation verifier runs.",
            "Improve bottom-up documentation generation for methods, callers, callees, parameters, returns, and return usage.",
            "Train the documentation generator to write accepted evidence units, not to validate final answers.",
            "Teach documentation generation for stored procedures: procedure name, SQL parameters, result shape, writes, and side effects.",
            "Train documentation generator rows for repository methods that call SQL and return mapped entities.",
            "Improve documentation generator output for controllers, services, repositories, mappers, validators, and DTO touchpoints.",
            "Teach complete method documentation with source path, symbol name, framework version, evidence id, and caller context.",
            "Train documentation generator so leaf method meaning is available when documenting callers.",
            "Improve documentation rows for legacy WebForms, MVC, WPF, API, and .NET Framework projects.",
            "Teach documentation generator to record parameters sent into callees and returned values consumed by callers.",
            "Train documentation generation for entity touchpoint methods that later answer add-field impact questions.",
            "Improve complete documentation fields before project validation uses them as wiki evidence.",
            "Teach DocumentationGeneratorUnit to describe code behavior instead of checking whether an answer cites evidence.",
            "Train documentation generator to capture callee summaries and propagate them into caller summaries.",
            "Improve method documentation for SQL procedure wrappers with input parameters and result-set meaning.",
            "Teach complete documentation records for repository Save, Load, Map, Validate, and Controller action methods.",
            "Train documentation generator to include callers and callees even when a method is a small wrapper.",
            "Improve documentation generation where a method returns a DTO, collection, bool, Task, or DataProcessResult.",
            "Teach documentation rows to include what the caller does with returned values.",
            "Train documentation generator to produce source-backed records, not a final user answer.",
            "Improve method documentation so ProjectLocalWikiModelUnit can later answer developer questions.",
            "Teach documentation generator records for stored procedure access before answer verification sees citations.",
            "Train documentation generator to document the code unit itself, not judge an answer about the code.",
            "Improve documentation fields that say what the code means, where it lives, what it accepts, who calls it, and what it returns.",
            "Teach documentation generation for legacy DAL methods, provider classes, HTTP handlers, and MVC controllers.",
            "Train documentation generator to write evidence-index-ready units for the project wiki.",
            "Improve documentation rows so every callee has meaning, parameters passed, result shape, and side effects.",
            "Teach DocumentationGeneratorUnit to avoid missing caller context and return-value usage.",
            "Train documentation generator to explain a method before any ProjectAnswerVerifier checks the answer.",
            "Improve code documentation records for field mapping, validation, persistence, and API response paths.",
            "Teach documentation generation to include stored procedure evidence without answering a question directly.",
            "Train documentation generator to produce accepted documentation, not validation verdicts.",
            "Improve method documentation records for multi-step workflows and their lower-level function calls.",
            "Teach documentation generator to include source evidence so later answer verifier can validate citations.",
            "Train documentation generator to describe what a function implements and how its output is used.");

        Add(rows, "answer_verifier", ProjectWikiRequestRouteKind.AnswerVerifierTraining, "wrong_training_unit",
            "",
            "Train answer verifier to check a finished answer only after evidence exists.",
            "Improve ProjectAnswerVerifier to reject unsupported evidence ids, stale documentation, and wrong-project citations.",
            "Teach answer verifier rows where the wrong verifier accepts a hallucinated stored procedure claim.",
            "Train citation verification for entity impact answers that cite missing evidence.");

        Add(rows, "answer_composer", ProjectWikiRequestRouteKind.AnswerComposerTraining, "wrong_training_unit",
            "",
            "Train answer composer to write a final developer answer from already retrieved evidence.",
            "Improve answer composition after query planning returns accepted documentation units.",
            "Teach grounded answer prose with citations and missing-evidence abstention.");

        Add(rows, "project_validation", ProjectWikiRequestRouteKind.ProjectValidation, "route_validation_to_answer",
            "",
            "Run project wiki validation after documentation training and list failed question classes.",
            "Validate exact evidence coverage for documentation-derived wiki answers.",
            "Run project validation across all five repositories before any done claim.");

        Add(rows, "trace_aggregator", ProjectWikiRequestRouteKind.TraceAggregatorTraining, "wrong_training_unit",
            "",
            "Train trace aggregation to identify the weakest unit when row-quality metrics fail.",
            "Teach WikiTraceAggregator to choose another 50-row documentation-generator remediation batch after failed metrics.",
            "Improve trace aggregation from WikiUnitTrace when documentation quality blocks wiki answers.");

        Add(rows, "not_project_wiki", ProjectWikiRequestRouteKind.NotProjectWiki, "route_non_wiki_to_project_wiki",
            "",
            "Translate this short sentence to English.");

        return rows.Take(50).ToArray();
    }

    private static RequestRoutingSeed[] BuildWeakDocValidationTracePrompts()
    {
        var rows = new List<RequestRoutingSeed>(50);

        Add(rows, "project_validation", ProjectWikiRequestRouteKind.ProjectValidation, "route_validation_to_answer",
            "",
            "Run the complete project wiki validation gate; do not answer a sample question.",
            "Validate every required wiki prompt variation and persist failed categories before any done claim.",
            "Run project validation for all five repositories with exact evidence accuracy and question coverage.",
            "Measure validation coverage for entity impact, workflow trace, SQL procedure, stack version, and missing evidence prompts.",
            "Run the ten-batch validation checkpoint and update WikiQueryTrace plus WikiUnitTrace.",
            "Check project wiki validation gates after training and list every uncovered method and edge.",
            "Validate stored procedure impact question coverage against accepted SQL evidence.",
            "Run the real-project validation suite and block promotion if any required prompt variation fails.",
            "Run project validation for local wiki answers against evidence ids, project boundaries, stale-document rules, and missing-evidence abstention.",
            "Run validation over askapediaweb, GenieDynamicServer, wizGameserver, geniesoftGameMVPServer, and genietasks.",
            "Check whether the project-local wiki passes exact evidence gates for every advanced developer question.",
            "Run project validation coverage gate and produce WikiQueryTrace plus WikiUnitTrace metrics.",
            "Validate documentation quality before evidence is allowed into project-local wiki training.",
            "Measure exact_evidence_accuracy, route_accuracy, query_plan_accuracy, and answer_verifier_accuracy.",
            "Run full project wiki validation and report uncovered SQL procedures, methods, graph edges, and prompt families.",
            "Verify the validation suite, not a single answer, before reporting any status.",
            "Run project validation for stale documentation, wrong-project evidence, hallucinated SQL procedure impact, and missing evidence.",
            "Check coverage improvements after the latest 50 training rows.");

        Add(rows, "documentation_generator", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "wrong_training_unit",
            "",
            "Train DocumentationGeneratorUnit to produce meaning, location, parameters, returns, callers, and callees.",
            "Improve bottom-up documentation rows so callee meaning flows upward into caller summaries.",
            "Teach documentation generation for stored procedure calls: procedure name, SQL parameters, result shape, and side effects.",
            "Train documentation fields that record source location, framework version, evidence id, and return usage.",
            "Improve method documentation for controllers, services, repositories, mappers, validators, and SQL callers.",
            "Train documentation generator rows for legacy WebForms, MVC, WPF, API, and .NET Framework projects.",
            "Teach documentation generator to describe what a method does before any answer verifier checks citations.",
            "Train complete documentation records before project validation uses them as wiki evidence.",
            "Improve documentation generation so callers, callees, parameters sent, and returned values used are all recorded.",
            "Teach DocumentationGeneratorUnit to avoid missing return usage and caller context.",
            "Train method documentation for entity touchpoints that will later answer field-change impact questions.",
            "Improve stored procedure documentation fields before local wiki answers cite them.",
            "Teach bottom-up documentation generation for leaf methods and then their callers.",
            "Train documentation generator to produce accepted evidence units, not to judge final answers.",
            "Improve code documentation rows for repository methods that read and write SQL data.",
            "Teach documentation generator to include exact file path and symbol name for every documented method.");

        Add(rows, "trace_aggregator", ProjectWikiRequestRouteKind.TraceAggregatorTraining, "wrong_training_unit",
            "",
            "Train trace aggregation rows that read failed unit results and choose the next remediation batch.",
            "Improve trace aggregation when ProjectWikiRequestRouter accuracy stalls below the 0.92 target.",
            "Teach WikiTraceAggregator to name the weakest wiki unit instead of classifying one prompt.",
            "Build trace aggregation rows that decide between sourcing data, validating projects, or running another 50-row batch.",
            "Train WikiTraceAggregator to read WikiQueryTrace and WikiUnitTrace before selecting the next action.",
            "Improve aggregation of failed ProjectWikiRequestRouter, DocumentationGenerator, ProjectValidation, and AnswerVerifier metrics.",
            "Teach trace aggregation to stop blind training and select the exact failed unit.",
            "Train WikiUnitTrace summaries that choose validation only when unit gates are strong enough.");

        Add(rows, "answer_composer", ProjectWikiRequestRouteKind.AnswerComposerTraining, "wrong_training_unit",
            "",
            "Train answer composer only after query planning and context retrieval return accepted evidence ids.",
            "Improve citation-safe answer text for entity impact questions from retrieved touchpoints.",
            "Teach grounded answer composition for stored procedure impact using accepted SQL evidence.",
            "Train final wiki answer prose with missing-evidence abstention and project boundary wording.");

        Add(rows, "not_project_wiki", ProjectWikiRequestRouteKind.NotProjectWiki, "route_non_wiki_to_project_wiki",
            "",
            "Translate this short Hebrew sentence to English.",
            "Write a friendly thank-you note.",
            "Create a simple travel plan for Prague.",
            "Explain rainfall without using a project repository.");

        return rows.Take(50).ToArray();
    }

    private static RequestRoutingSeed[] BuildDiverseSiblingPrompts(bool heldOut)
    {
        var prefix = heldOut ? "Held-out diverse sibling: " : "Diverse sibling: ";
        var rows = new List<RequestRoutingSeed>(50);

        Add(rows, "documentation_build", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script", prefix,
            "Build a trainable project-local documentation wiki for askapediaweb and preserve evidence traces.",
            "Run the DocumentationOrchestrator for GenieDynamicServer and write traceable wiki evidence.",
            "Create repository documentation for wizGameserver through the learned wiki pipeline.",
            "Produce bottom-up documentation for genietasks with method callers, callees, and return usage.",
            "Document a legacy .NET repository as a local wiki domain with trainable routing and metrics.");

        Add(rows, "answer_composer", ProjectWikiRequestRouteKind.AnswerComposerTraining, "wrong_training_unit", prefix,
            "Train the answer composer that writes the final response after evidence retrieval.",
            "Improve final local-wiki answers using accepted evidence ids and abstention rules.",
            "Teach answer composition for developer questions after query planning has found the relevant records.",
            "Train grounded project answers that explain code behavior without inventing citations.",
            "Improve the unit that turns retrieved documentation units into a readable developer answer.",
            "Teach final answer generation for stored procedure and entity impact wiki questions.");

        Add(rows, "project_validation", ProjectWikiRequestRouteKind.ProjectValidation, "route_validation_to_answer", prefix,
            "Run project wiki validation for all repositories and record the failed question classes.",
            "Validate exact evidence coverage for every required project-local wiki prompt variation.",
            "Measure whether the local wiki passes entity impact, workflow trace, stack/version, SQL procedure, and missing-evidence checks.",
            "Run the real-project coverage gate and persist WikiQueryTrace plus WikiUnitTrace.",
            "Check the wiki validation suite before reporting any training target reached.",
            "Validate stored procedure question coverage against accepted SQL evidence.",
            "Verify all project wiki answers against evidence ids, project boundaries, and stale-version rules.",
            "Run validation after the latest ten batches and list uncovered methods, edges, and prompt families.");

        Add(rows, "not_project_wiki", ProjectWikiRequestRouteKind.NotProjectWiki, "route_non_wiki_to_project_wiki", prefix,
            "Translate a short Hebrew sentence into English.",
            "Write a short product description for a coffee mug.",
            "Give me a weekend itinerary for Barcelona.",
            "Summarize the concept of event sourcing without any project repository.",
            "Create a checklist for packing a suitcase.",
            "Explain how rain forms.",
            "Write a friendly reminder email.",
            "Generate a simple dinner menu.");

        Add(rows, "stack_version", ProjectWikiRequestRouteKind.StackVersionTraining, "wrong_training_unit", prefix,
            "Train stack/version classification from csproj files, target frameworks, and project style cues.",
            "Improve detection of WebForms, MVC, WPF, .NET Framework, .NET Core, and CSharp language versions.",
            "Teach StackAndVersionClassifierUnit to read target framework evidence before documentation starts.",
            "Build framework and language version rows for legacy .NET documentation routing.",
            "Train stack detection so wiki docs know the server, client, storage, and runtime versions.",
            "Remediate stack/version classifier failures found during project wiki validation.");

        Add(rows, "documentation_generator", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "wrong_training_unit", prefix,
            "Train the documentation generator that creates method records from source code.",
            "Improve documentation generation for callers, callees, parameters, returns, and side effects.",
            "Teach stored procedure documentation fields: procedure name, SQL parameters, result shape, and writes.",
            "Train bottom-up documentation so callee meaning is available to caller summaries.",
            "Improve documentation generator rows for repository, controller, service, and mapper methods.");

        Add(rows, "entity_impact", ProjectWikiRequestRouteKind.EntityImpactTraining, "wrong_training_unit", prefix,
            "Train entity impact mapping for adding a new field to a DTO and following every usage.",
            "Improve field-change touchpoint maps across mappers, validators, persistence, UI binding, and tests.",
            "Teach entity impact reasoning for changes that pass through repositories and stored procedures.",
            "Build rows where the chosen answer lists readers, writers, API contracts, and workflow effects.");

        Add(rows, "answer_verifier", ProjectWikiRequestRouteKind.AnswerVerifierTraining, "wrong_training_unit", prefix,
            "Train the verifier to reject wiki answers with unsupported evidence ids.",
            "Improve stale-document and wrong-project citation detection.",
            "Teach answer verification for missing-evidence cases and hallucinated procedure claims.",
            "Train verifier rows that fail unsupported entity impact answers.");

        Add(rows, "trace_aggregator", ProjectWikiRequestRouteKind.TraceAggregatorTraining, "wrong_training_unit", prefix,
            "Train the trace aggregator that reads WikiUnitTrace and selects the next remediation batch.",
            "Improve aggregation of failed ProjectWikiRequestRouter, DocumentationGenerator, and ProjectValidation metrics.",
            "Teach WikiTraceAggregator to identify the weakest unit rather than classify a single prompt.",
            "Build trace aggregation rows that choose sourcing, validation, or another 50-row training step.");

        return rows.Take(50).ToArray();
    }

    private static RequestRoutingSeed[] BuildFocusedRemediationPrompts(bool heldOut)
    {
        var prefix = heldOut ? "Held-out remediation: " : "";
        var rows = new List<RequestRoutingSeed>(50);

        Add(rows, "documentation_build", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script", prefix,
            "Document askapediaweb from source code into a local wiki with trace metrics.",
            "Build documentation for GenieDynamicServer and persist evidence-index, question seeds, and unit traces.",
            "Create bottom-up docs for wizGameserver, including callers, callees, params, returns, and framework version.",
            "Run the full documentation pipeline for genietasks, not just a text summary.",
            "Make a project-local wiki for legacy .NET code and record route/query/documentation traces.");

        Add(rows, "answer_composer", ProjectWikiRequestRouteKind.AnswerComposerTraining, "wrong_training_unit", prefix,
            "Train the answer composer to explain code only from retrieved evidence ids.",
            "Improve grounded wiki answers with citations, missing-evidence abstention, and project boundary checks.",
            "Teach answer composition for entity impact questions after query planning retrieves touchpoints.",
            "Train local wiki answer text to include meaning, location, parameters, returns, callers, and callees.",
            "Improve the unit that turns accepted evidence into a developer-facing answer.",
            "Teach citation-safe answers for stored procedure impact questions.");

        Add(rows, "project_validation", ProjectWikiRequestRouteKind.ProjectValidation, "route_validation_to_answer", prefix,
            "Validate all five .NET projects with the full wiki question variation suite.",
            "Run exact evidence validation for askapediaweb, GenieDynamicServer, wizGameserver, geniesoftGameMVPServer, and genietasks.",
            "Check whether generated wiki answers all 45 advanced prompt variations with evidence ids.",
            "Measure project wiki coverage for entity impact, workflow trace, stored procedure, stack/version, and missing-evidence questions.",
            "After ten batches, run real project validation and list uncovered methods and edges.",
            "Validate stored procedure wiki coverage on DNN-style SQL evidence.",
            "Check that project-local wiki routes every question through query planning before answering.",
            "Run coverage gate and persist WikiQueryTrace plus WikiUnitTrace for each project.");

        Add(rows, "not_project_wiki", ProjectWikiRequestRouteKind.NotProjectWiki, "route_non_wiki_to_project_wiki", prefix,
            "Translate this Russian paragraph to English.",
            "Create a marketing homepage for a bakery.",
            "What is the weather tomorrow in Haifa?",
            "Generate a watercolor image of Jerusalem.",
            "Write a generic SQL query without any project repository context.",
            "Summarize a public news article.",
            "Explain how sourdough starter works.",
            "Make a travel itinerary for Rome.");

        Add(rows, "stack_version", ProjectWikiRequestRouteKind.StackVersionTraining, "wrong_training_unit", prefix,
            "Train the stack and version classifier for csproj target framework, CSharp version, MVC, WPF, and WebForms.",
            "Improve framework/version routing for legacy .NET Framework 3.5, 4.5.2, 4.6.1, and .NET Core projects.",
            "Teach the unit that reads csproj files and decides language version and project style.",
            "Build 50 rows for StackAndVersionClassifierUnit from open legacy .NET examples.",
            "Train framework detection so documentation knows whether code is WebForms, MVC, WPF, or API.",
            "Remediate target-framework classification failures from project wiki validation.");

        Add(rows, "documentation_generator", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "wrong_training_unit", prefix,
            "Train documentation generator to fill meaning, location, parameters, returns, callers, callees, and stack version.",
            "Improve bottom-up documentation rows for methods, callers, and higher-level workflow summaries.",
            "Teach DocumentationGeneratorUnit to include stored procedure name, SQL params, result shape, and side effects when present.",
            "Train documentation fields so callers and return usage are always included.",
            "Train complete documentation fields before project validation uses the code as test evidence.");

        Add(rows, "entity_impact", ProjectWikiRequestRouteKind.EntityImpactTraining, "wrong_training_unit", prefix,
            "Train entity impact mapper for adding field DiscountCode to CampaignDto.",
            "Improve touchpoint maps for readers, writers, validators, mappers, persistence, API contracts, and tests.",
            "Build entity impact rows for adding a property to a legacy WebForms model.",
            "Teach field-change reasoning across DTO, mapper, repository, stored procedure, and UI binding.");

        Add(rows, "answer_verifier", ProjectWikiRequestRouteKind.AnswerVerifierTraining, "wrong_training_unit", prefix,
            "Train answer verifier to reject hallucinated citations and wrong-project evidence ids.",
            "Improve ProjectAnswerVerifier on stale documentation and missing evidence answers.",
            "Build rows where chosen verifier catches unsupported wiki answers and rejected verifier passes them.",
            "Teach citation verification for stored procedure and entity impact answers.");

        Add(rows, "trace_aggregator", ProjectWikiRequestRouteKind.TraceAggregatorTraining, "wrong_training_unit", prefix,
            "Train WikiTraceAggregator to read failed unit traces and choose the next remediation batch.",
            "Improve trace aggregation so low ProjectWikiRequestRouter accuracy triggers more routing rows.",
            "Build rows where the aggregator must stop blind training and name the failed wiki unit.",
            "Teach WikiUnitTrace summary to decide whether project validation or data sourcing is next.");

        return rows.Take(50).ToArray();
    }

    private static RequestRoutingSeed[] BuildWeakTargetPrompts(bool heldOut)
    {
        var prefix = heldOut ? "Held-out: " : "";
        var rows = new List<RequestRoutingSeed>(50);

        Add(rows, "project_validation", ProjectWikiRequestRouteKind.ProjectValidation, "route_validation_to_answer", prefix,
            "Run project validation and check exact evidence accuracy before any wiki answer is accepted.",
            "Validate project wiki coverage for all advanced prompt variations and reject single-answer validation.",
            "Measure project wiki coverage across entity impact, stored procedure impact, stack version, and missing evidence tasks.",
            "Run project validation over all five repositories and persist WikiQueryTrace plus WikiUnitTrace.",
            "Check project wiki validation gates after the latest training batch.",
            "Validate generated wiki answers against source evidence ids and project boundaries.",
            "Run coverage gate for stored procedure impact questions before promotion.",
            "Verify project wiki validation covers every required question class.",
            "Measure exact_evidence_accuracy and question variation pass rate for local wiki answers.",
            "Run full project wiki validation and list uncovered methods, edges, SQL procedures, and prompts.",
            "Validate documentation quality before evidence enters local wiki training.",
            "Check whether all required wiki units pass project validation thresholds.",
            "Run real project validation and block promotion if route accuracy or evidence accuracy fails.",
            "Verify that every developer wiki question passes route, query, retrieval, answer, and verifier gates.",
            "Validate stored procedure documentation supports developer impact questions.");

        Add(rows, "answer_composer", ProjectWikiRequestRouteKind.AnswerComposerTraining, "wrong_training_unit", prefix,
            "Train answer composer to convert retrieved evidence into a final developer answer.",
            "Improve answer composer so it explains entity impact after query planning returns touchpoints.",
            "Teach answer composer to summarize stored procedure impact from accepted SQL evidence.",
            "Train grounded answer text with citations, missing-evidence abstention, and project boundary wording.",
            "Improve local wiki answer prose from accepted documentation records.",
            "Teach answer composition after retrieval, not query planning itself.",
            "Train citation-safe answer text for workflow trace questions.",
            "Improve answer composer for callers, callees, parameters, returns, and side effects.",
            "Teach answer unit to use retrieved evidence ids and never invent missing facts.",
            "Train answer composer to explain code meaning and location from accepted evidence.",
            "Improve final wiki answer generation after BM25 or GraphRAG retrieval.",
            "Teach answer composition for field-change impact summaries.",
            "Train answer text for legacy .NET project wiki questions.",
            "Improve answer composer to include caveats when evidence is partial.",
            "Teach answer composer to present query-planning results as a readable answer.");

        Add(rows, "documentation_generator", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "wrong_training_unit", prefix,
            "Train documentation generator to produce complete method documentation fields.",
            "Improve documentation generator for callers, callees, parameters, returns, and usage.",
            "Teach documentation generator to record stored procedure names, SQL params, result shape, and side effects.",
            "Train documentation rows that carry callee meaning upward to callers.",
            "Improve documentation generator for repository methods that call SQL stored procedures.",
            "Teach documentation records for legacy WebForms, MVC, WPF, and API projects.",
            "Train documentation generator to include source location and evidence id.",
            "Improve documentation evidence for DTO fields, mappers, validators, and persistence.",
            "Teach complete documentation before project validation uses it as wiki evidence.",
            "Train documentation generator to avoid missing caller context or return usage.");

        Add(rows, "documentation_build", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script", prefix,
            "Build local wiki documentation for askapediaweb through trainable orchestration.",
            "Document GenieDynamicServer and persist evidence, route traces, and unit metrics.",
            "Run documentation pipeline for wizGameserver instead of a static summary.",
            "Create project-local wiki docs for genietasks with bottom-up traces.",
            "Build documentation graph for legacy .NET code through DocumentationOrchestrator.");

        Add(rows, "not_project_wiki", ProjectWikiRequestRouteKind.NotProjectWiki, "route_non_wiki_to_project_wiki", prefix,
            "Translate this paragraph to English.",
            "Generate a food shopping list.",
            "Write a birthday greeting.",
            "Explain basic graph theory without project code.",
            "Create a short travel plan for Athens.");

        return rows.Take(50).ToArray();
    }

    private static RequestRoutingSeed[] BuildValidationFocusPrompts(bool heldOut)
    {
        var prefix = heldOut ? "Held-out: " : "";
        var rows = new List<RequestRoutingSeed>(50);

        Add(rows, "project_validation", ProjectWikiRequestRouteKind.ProjectValidation, "route_validation_to_answer", prefix,
            "Run full project wiki validation across all target repositories before reporting status.",
            "Validate exact evidence accuracy for every advanced wiki prompt variation.",
            "Measure whether the wiki covers entity impact, workflow trace, stored procedure, stack version, and missing evidence questions.",
            "Check all generated wiki answers against source evidence ids and project boundaries.",
            "Run the ten-batch validation gate and list uncovered methods, edges, SQL procedures, and question types.",
            "Validate that askapediaweb, GenieDynamicServer, wizGameserver, geniesoftGameMVPServer, and genietasks all pass wiki answer gates.",
            "Evaluate project-local wiki coverage using the stored procedure impact prompt suite.",
            "Run coverage gate after training and persist failed question categories.",
            "Check whether the wiki answers field-change prompts with complete touchpoint maps.",
            "Validate query planning coverage before allowing answer composition metrics.",
            "Run project validation for stale documentation, wrong project evidence, and missing evidence abstention.",
            "Measure exact_evidence_accuracy, route_accuracy, query_plan_accuracy, and answer_verifier_accuracy.",
            "Verify that every question goes through request routing, query planning, context retrieval, and answer verification.",
            "Run all project wiki validation prompts and do not stop after one sample answer.",
            "Check coverage improvements after the latest 50 training rows.",
            "Validate documentation quality before using it as wiki training evidence.",
            "Run project wiki validation and produce WikiQueryTrace plus WikiUnitTrace metrics.",
            "Verify that stored procedure documentation supports developer impact questions.",
            "Check whether all required wiki units pass their acceptance thresholds on real projects.",
            "Run real project validation and block promotion if any required prompt variation fails.");

        Add(rows, "answer_composer", ProjectWikiRequestRouteKind.AnswerComposerTraining, "wrong_training_unit", prefix,
            "Train answer composer after validation proves query planning returns accepted evidence.",
            "Teach answer composition to synthesize retrieved touchpoints into a developer-facing explanation.",
            "Improve grounded answer wording while preserving evidence ids and abstention boundaries.",
            "Train answer composer to explain field-change impact after the query planner finds readers and writers.",
            "Teach the answer unit to summarize stored procedure impact from accepted SQL evidence.",
            "Improve answer composition for callers, callees, parameters, returns, and side effects.",
            "Train citation-safe answer text without changing validation thresholds.",
            "Teach the answer composer to say what evidence is missing instead of inventing it.",
            "Train answer text for workflow trace questions after retrieval is complete.",
            "Improve local wiki answer prose from accepted documentation records.");

        Add(rows, "documentation_generator", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "wrong_training_unit", prefix,
            "Train documentation generator to describe method meaning, location, parameters, returns, callers, and callees.",
            "Improve documentation rows for stored procedures, SQL parameters, result sets, and side effects.",
            "Teach documentation generator to include framework version and project style from stack classifier output.",
            "Train complete method documentation before it becomes evidence for wiki answers.",
            "Build documentation generator rows that explain how callee summaries flow upward to callers.",
            "Improve documentation of repository methods that call SQL stored procedures.",
            "Train bottom-up documentation records for legacy WebForms and MVC projects.",
            "Teach documentation generator to avoid missing return usage and caller context.",
            "Improve documentation evidence for entity mapper and DTO fields.",
            "Train documentation generator to record code location and evidence id for every summary.");

        Add(rows, "answer_verifier", ProjectWikiRequestRouteKind.AnswerVerifierTraining, "wrong_training_unit", prefix,
            "Train answer verifier to fail answers with unsupported citations.",
            "Improve verifier detection of wrong project evidence ids.",
            "Teach ProjectAnswerVerifier to reject stale documentation answers.",
            "Train verifier to require missing-evidence abstention when the wiki cannot prove a claim.",
            "Improve citation verification for stored procedure impact answers.");

        Add(rows, "not_project_wiki", ProjectWikiRequestRouteKind.NotProjectWiki, "route_non_wiki_to_project_wiki", prefix,
            "Write a poem about databases.",
            "Translate a sentence from Hebrew to English.",
            "Create a travel plan for Prague.",
            "Explain what a sourdough starter is.",
            "Generate a logo concept for a bakery.");

        return rows.Take(50).ToArray();
    }

    private static void Add(
        List<RequestRoutingSeed> rows,
        string promptType,
        ProjectWikiRequestRouteKind expectedRouteKind,
        string rejectedType,
        string prefix,
        params string[] prompts)
    {
        rows.AddRange(prompts.Select(prompt => new RequestRoutingSeed(promptType, $"{prefix}{prompt}", expectedRouteKind, rejectedType)));
    }

    private static readonly RequestRoutingSeed[] TrainingPrompts =
    [
        new("documentation_build", "Document this .NET legacy project and build the local wiki evidence graph.", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script"),
        new("documentation_build", "Make documentation for C:\\Projects\\askapediaweb with architecture notes and wiki evidence.", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script"),
        new("documentation_build", "Create bottom-up documentation from methods to callers and persist project wiki artifacts.", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script"),
        new("documentation_build", "Build local wiki for GenieDynamicServer and validate code trace coverage.", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script"),
        new("documentation_build", "Document code and generate evidence-index for wizGameserver.", ProjectWikiRequestRouteKind.DocumentationBuild, "route_to_static_script"),
        new("wiki_question", "Question about code: what does Campaign.GetCampaignName do and who calls it?", ProjectWikiRequestRouteKind.WikiQuestionAnswer, "route_to_answer_without_planning"),
        new("wiki_question", "Ask local wiki where TaskService.AssignTask is implemented.", ProjectWikiRequestRouteKind.WikiQuestionAnswer, "route_to_answer_without_planning"),
        new("wiki_question", "I want to add field DiscountCode to Campaign; what path should the wiki search?", ProjectWikiRequestRouteKind.WikiQuestionAnswer, "route_to_answer_without_planning"),
        new("wiki_question", "What parameters does MatchmakingService.CreateSession accept?", ProjectWikiRequestRouteKind.WikiQuestionAnswer, "route_to_answer_without_planning"),
        new("wiki_question", "Where is DynamicEntityMapper.MapFields and what does it return?", ProjectWikiRequestRouteKind.WikiQuestionAnswer, "route_to_answer_without_planning"),
        new("request_router_training", "Train project wiki request router so executor prompts choose the correct trainable unit.", ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining, "route_to_static_script"),
        new("query_planning", "Train query planning so the wiki asks what would I search before answering.", ProjectWikiRequestRouteKind.QueryPlanningTraining, "route_to_static_script"),
        new("query_planning", "Build 50 rows for route a question into searches over wiki evidence.", ProjectWikiRequestRouteKind.QueryPlanningTraining, "route_to_static_script"),
        new("query_planning", "The user question may need a series of queries; train that planner.", ProjectWikiRequestRouteKind.QueryPlanningTraining, "route_to_static_script"),
        new("query_planning", "Improve ProjectWikiQueryPlanningFlow on entity impact prompts.", ProjectWikiRequestRouteKind.QueryPlanningTraining, "route_training_to_project_repo"),
        new("query_planning", "Plan wiki query before retrieval for code change questions.", ProjectWikiRequestRouteKind.QueryPlanningTraining, "route_to_answer_without_planning"),
        new("answer_composer", "Train answer composer to answer from evidence ids with citations.", ProjectWikiRequestRouteKind.AnswerComposerTraining, "route_to_static_script"),
        new("answer_composer", "Compose wiki answer only from accepted local evidence.", ProjectWikiRequestRouteKind.AnswerComposerTraining, "route_to_static_script"),
        new("answer_composer", "Grounded answer should include meaning, location, params, returns, callers, callees.", ProjectWikiRequestRouteKind.AnswerComposerTraining, "route_to_answer_without_planning"),
        new("answer_composer", "Improve citation answer behavior for local wiki model.", ProjectWikiRequestRouteKind.AnswerComposerTraining, "route_training_to_project_repo"),
        new("answer_composer", "Train the unit that turns retrieved evidence into human answer.", ProjectWikiRequestRouteKind.AnswerComposerTraining, "route_to_static_script"),
        new("context_routing", "Train context routing to choose BM25 or GraphRAG.", ProjectWikiRequestRouteKind.ContextRoutingTraining, "route_to_static_script"),
        new("context_routing", "Which retriever should the wiki use: mode 2 semantic or mode 3 BM25?", ProjectWikiRequestRouteKind.ContextRoutingTraining, "route_to_answer_without_planning"),
        new("context_routing", "Route wiki context retrieval based on query plan complexity.", ProjectWikiRequestRouteKind.ContextRoutingTraining, "route_to_static_script"),
        new("context_routing", "Improve semantic retrieval fallback to lexical evidence.", ProjectWikiRequestRouteKind.ContextRoutingTraining, "route_training_to_project_repo"),
        new("context_routing", "Train Mode 2 and Mode 3 route decisions for project wiki.", ProjectWikiRequestRouteKind.ContextRoutingTraining, "route_to_static_script"),
        new("negative_dpo", "Train negative DPO generator for documentation rejected side.", ProjectWikiRequestRouteKind.NegativeDpoTraining, "route_to_static_script"),
        new("negative_dpo", "Make bad rejected DPO examples with [WRONG:] for docs.", ProjectWikiRequestRouteKind.NegativeDpoTraining, "route_to_static_script"),
        new("negative_dpo", "Negative DTO unit should generate candidates and validate them.", ProjectWikiRequestRouteKind.NegativeDpoTraining, "route_to_static_script"),
        new("negative_dpo", "Improve wrong DPO construction for missing callers and callees.", ProjectWikiRequestRouteKind.NegativeDpoTraining, "route_training_to_project_repo"),
        new("negative_dpo", "Generate rejected side for documentation pairs automatically.", ProjectWikiRequestRouteKind.NegativeDpoTraining, "route_to_static_script"),
        new("project_validation", "Validate wiki coverage across all real .NET projects.", ProjectWikiRequestRouteKind.ProjectValidation, "route_to_static_script"),
        new("project_validation", "Check all projects with 37 variations and exact evidence accuracy.", ProjectWikiRequestRouteKind.ProjectValidation, "route_to_static_script"),
        new("project_validation", "Run real project validation for askapediaweb and GenieDynamicServer.", ProjectWikiRequestRouteKind.ProjectValidation, "route_to_static_script"),
        new("project_validation", "Measure coverage on local wiki question variations.", ProjectWikiRequestRouteKind.ProjectValidation, "route_to_static_script"),
        new("project_validation", "Validate generated wiki and question traces on real project evidence.", ProjectWikiRequestRouteKind.ProjectValidation, "route_to_static_script"),
        new("not_project_wiki", "Summarize today's weather in Tel Aviv.", ProjectWikiRequestRouteKind.NotProjectWiki, "route_to_non_wiki"),
        new("not_project_wiki", "Generate a marketing landing page for a coffee shop.", ProjectWikiRequestRouteKind.NotProjectWiki, "route_to_non_wiki"),
        new("not_project_wiki", "Translate this Hebrew sentence to English.", ProjectWikiRequestRouteKind.NotProjectWiki, "route_to_non_wiki"),
        new("not_project_wiki", "Write a SQL query for a standalone analytics table.", ProjectWikiRequestRouteKind.NotProjectWiki, "route_to_non_wiki"),
        new("not_project_wiki", "Create an image of a city skyline.", ProjectWikiRequestRouteKind.NotProjectWiki, "route_to_non_wiki"),
        new("stack_version", "Train stack and version classifier for target framework and CSharp version.", ProjectWikiRequestRouteKind.StackVersionTraining, "route_to_static_script"),
        new("code_trace", "Train code trace builder for caller callee trace and value-flow training.", ProjectWikiRequestRouteKind.CodeTraceTraining, "route_to_static_script"),
        new("entity_impact", "Train entity impact mapper with touchpoint map for readers writers validators.", ProjectWikiRequestRouteKind.EntityImpactTraining, "route_to_static_script"),
        new("documentation_generator", "Train documentation generator for complete documentation fields.", ProjectWikiRequestRouteKind.DocumentationGeneratorTraining, "route_to_static_script"),
        new("answer_verifier", "Train answer verifier to verify citations and reject wrong project answer.", ProjectWikiRequestRouteKind.AnswerVerifierTraining, "route_to_static_script"),
        new("trace_aggregator", "Train trace aggregator to read WikiUnitTrace and choose remediation batch.", ProjectWikiRequestRouteKind.TraceAggregatorTraining, "route_to_static_script"),
        new("project_validation", "Run coverage after 10 batches and show missing wiki question types.", ProjectWikiRequestRouteKind.ProjectValidation, "route_to_static_script"),
        new("wiki_question", "Add a field to entity Order; ask wiki for readers writers validators and tests.", ProjectWikiRequestRouteKind.WikiQuestionAnswer, "route_to_answer_without_planning"),
        new("not_project_wiki", "Explain how sourdough starter works.", ProjectWikiRequestRouteKind.NotProjectWiki, "route_to_non_wiki")
    ];

    private static readonly RequestRoutingSeed[] HeldOutPrompts = TrainingPrompts
        .Select(seed => seed with { Prompt = $"Held-out: {seed.Prompt}" })
        .ToArray();

    private sealed record RequestRoutingSeed(
        string PromptType,
        string Prompt,
        ProjectWikiRequestRouteKind ExpectedRouteKind,
        string RejectedType);
}
