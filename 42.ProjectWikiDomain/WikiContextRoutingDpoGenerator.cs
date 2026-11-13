// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text;
using AgentInfrastructure;

public sealed class WikiContextRoutingDpoGenerator
{
    private readonly ProjectWikiQueryPlanningFlow _planner;
    private readonly WikiContextRoutingUnit _routingUnit;

    public WikiContextRoutingDpoGenerator(ProjectWikiQueryPlanningFlow? planner = null, WikiContextRoutingUnit? routingUnit = null)
    {
        _planner = planner ?? new ProjectWikiQueryPlanningFlow();
        _routingUnit = routingUnit ?? new WikiContextRoutingUnit();
    }

    public async Task<OperationResult<ProjectWikiContextRoutingDpoBatch>> BuildBatchAsync(
        string batchId = "wiki-context-routing-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiContextRoutingDpoBatch>.Fail("Cancelled.", "CANCELLED");

        var seeds = BuildSeeds(batchId, heldOut);
        var compact = batchId.Contains("compact", StringComparison.OrdinalIgnoreCase);
        var boundaryStrong = batchId.Contains("boundary-strong", StringComparison.OrdinalIgnoreCase);
        var directStrong = batchId.Contains("direct-strong", StringComparison.OrdinalIgnoreCase);
        var precheckStrong = batchId.Contains("precheck-strong", StringComparison.OrdinalIgnoreCase);
        var precheckPlanStrong = batchId.Contains("precheck-plan-strong", StringComparison.OrdinalIgnoreCase);
        var precheckOnly = batchId.Contains("precheck-only", StringComparison.OrdinalIgnoreCase);
        var precheckClean = batchId.Contains("precheck-clean", StringComparison.OrdinalIgnoreCase);
        var precheckBinary = batchId.Contains("precheck-binary", StringComparison.OrdinalIgnoreCase);
        var precheckProof = batchId.Contains("precheck-proof", StringComparison.OrdinalIgnoreCase);
        var abstainStrong = batchId.Contains("abstain-strong", StringComparison.OrdinalIgnoreCase);
        var pairs = new List<ProjectWikiContextRoutingDpoPair>(seeds.Length);
        for (var index = 0; index < seeds.Length; index++)
        {
            var seed = seeds[index];
            var question = new ProjectWikiUserQuestion(
                $"{batchId}-question-{index + 1:000}",
                seed.ProjectId,
                $@"C:\Projects\{seed.ProjectId}",
                "generic-pretrain-v1",
                seed.Question);
            var planned = await _planner.PlanAsync(question, ct);
            if (!planned.IsSuccess)
                return planned.MapError<ProjectWikiContextRoutingDpoBatch>();

            var plan = planned.Value!;
            var request = new ProjectWikiQuestionRequest(
                seed.ProjectId,
                seed.Question,
                $@"C:\Projects\{seed.ProjectId}\.xiigen\wiki-domain",
                "generic-pretrain-v1",
                TopK: 5,
                AllowSemantic: seed.AllowSemantic);
            var routed = _routingUnit.Route(request, plan);
            if (!routed.IsSuccess)
                return routed.MapError<ProjectWikiContextRoutingDpoBatch>();

            var decision = routed.Value!;
            pairs.Add(new ProjectWikiContextRoutingDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                seed.QuestionType,
                question,
                plan,
                decision.RouteKind,
                seed.Question,
                BuildChosen(decision, plan, compact, boundaryStrong, directStrong, precheckStrong, precheckPlanStrong, precheckOnly, precheckClean, precheckBinary, precheckProof, abstainStrong),
                BuildRejected(seed.RejectedType, decision, compact, boundaryStrong, directStrong, precheckStrong, precheckPlanStrong, precheckOnly, precheckClean, precheckBinary, precheckProof, abstainStrong),
                seed.RejectedType));
        }

        var wrongAnnotationRate = pairs.Count == 0
            ? 0
            : pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Count;
        var developerReasoningRows = pairs.Count(pair => IsDeveloperReasoningType(pair.QuestionType));
        var warnings = new List<string>();
        if (pairs.Count != 50)
            warnings.Add("batch_row_count_not_50");
        if (developerReasoningRows < 20)
            warnings.Add("developer_reasoning_rows_below_40_percent");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return OperationResult<ProjectWikiContextRoutingDpoBatch>.Ok(new ProjectWikiContextRoutingDpoBatch(
            batchId,
            pairs.Count,
            developerReasoningRows,
            wrongAnnotationRate,
            pairs.ToArray(),
            warnings.ToArray()));
    }

    private static string BuildChosen(
        ProjectWikiContextRoutingDecision decision,
        ProjectWikiQueryPlan plan,
        bool compact,
        bool boundaryStrong,
        bool directStrong,
        bool precheckStrong,
        bool precheckPlanStrong,
        bool precheckOnly,
        bool precheckClean,
        bool precheckBinary,
        bool precheckProof,
        bool abstainStrong)
    {
        if (compact)
        {
            if (precheckProof)
            {
                return string.Join("\n", [
                    "context_route_verdict: route_after_query_plan_prechecks",
                    "precheck_source: ProjectWikiQueryPlanningFlow",
                    "query_plan_prechecks_verified: WrongProjectCheck>ConfigurationLookup>StaleVersionCheck>MissingEvidenceCheck",
                    "retriever_selection_after_prechecks: true",
                    $"route_kind: {decision.RouteKind}",
                    "answer_permission: after_evidence_sufficiency"
                ]);
            }

            if (precheckBinary)
            {
                return string.Join("\n", [
                    "context_route_verdict: safe_verified_route",
                    "safe_route: yes",
                    "verified_prechecks: yes",
                    "project_version_evidence_checked: yes",
                    "retriever_after_checks: yes",
                    $"route_kind: {decision.RouteKind}",
                    "answer_permission: verified"
                ]);
            }

            if (precheckClean)
            {
                return string.Join("\n", [
                    "context_route_verdict: verified_prechecked_route",
                    "precheck_status: complete_before_retrieval",
                    "wrong_project_check: complete",
                    "stale_version_check: complete",
                    "missing_evidence_check: complete",
                    "retriever_selection: after_prechecks",
                    $"route_kind: {decision.RouteKind}",
                    "answer_permission: after_evidence_sufficiency"
                ]);
            }

            if (precheckOnly)
            {
                return string.Join("\n", [
                    "context_route_verdict: verified_prechecked_route",
                    "query_plan_order: WrongProjectCheck>StaleVersionCheck>MissingEvidenceCheck>RetrieverSelection>AnswerPermission",
                    $"route_kind: {decision.RouteKind}",
                    "answer_permission: after_evidence_sufficiency"
                ]);
            }

            var lines = new List<string>
            {
                "context_route_verdict: correct_project_wiki_context_route",
                $"route_kind: {decision.RouteKind}",
                $"intent: {plan.Intent}",
                $"requires_semantic_retriever: {decision.RequiresSemanticRetriever}",
                $"allows_lexical_fallback: {decision.AllowsLexicalFallback}",
                "prechecks_complete: true",
                "answer_before_retrieval: false"
            };
            if (boundaryStrong)
            {
                lines.Add("project_boundary: local_project_only");
                lines.Add("cross_project_evidence_allowed: false");
            }
            if (directStrong)
            {
                lines.Add("exact_symbol_lookup: true");
                lines.Add("semantic_expansion_required: false");
            }
            if (precheckStrong)
            {
                lines.Add("route_decision_status: usable_after_prechecks");
                lines.Add("wrong_project_check: complete");
                lines.Add("stale_version_check: complete");
                lines.Add("missing_evidence_check: complete");
                lines.Add("semantic_availability_check: complete");
            }
            if (precheckPlanStrong)
            {
                lines.Add("query_plan_order: WrongProjectCheck>StaleVersionCheck>MissingEvidenceCheck>RetrieverSelection>AnswerPermission");
                lines.Add("answer_permission: after_evidence_sufficiency");
            }
            if (abstainStrong)
            {
                lines.Add("retrieval_required_before_answer: true");
                lines.Add("missing_evidence_policy: return_missing_evidence_not_answer");
            }
            return string.Join("\n", lines);
        }

        var builder = new StringBuilder();
        builder.AppendLine($"route_kind: {decision.RouteKind}");
        builder.AppendLine($"route_reason: {decision.RouteReason}");
        builder.AppendLine($"requires_semantic_retriever: {decision.RequiresSemanticRetriever}");
        builder.AppendLine($"allows_lexical_fallback: {decision.AllowsLexicalFallback}");
        builder.AppendLine($"confidence: {decision.Confidence:0.######}");
        builder.AppendLine($"required_pre_checks: {string.Join(", ", decision.RequiredPreChecks)}");
        builder.AppendLine($"intent: {plan.Intent}");
        builder.AppendLine($"query_steps: {string.Join(", ", plan.RequiredQuerySteps.Select(step => step.StepType))}");
        return builder.ToString().TrimEnd();
    }

    private static string BuildRejected(
        string rejectedType,
        ProjectWikiContextRoutingDecision decision,
        bool compact,
        bool boundaryStrong,
        bool directStrong,
        bool precheckStrong,
        bool precheckPlanStrong,
        bool precheckOnly,
        bool precheckClean,
        bool precheckBinary,
        bool precheckProof,
        bool abstainStrong)
    {
        if (compact)
        {
            var wrong = rejectedType switch
            {
                "always_bm25" =>
                    ("always_bm25", "context_route_verdict: wrong_project_wiki_context_route\nroute_kind: Mode3LexicalEvidence\nrequires_semantic_retriever: false", "misses multi-hop developer reasoning where semantic expansion is required after the query plan identifies many evidence families."),
                "always_semantic" =>
                    ("always_semantic", directStrong
                        ? "context_route_verdict: wrong_oversemantic_route\nroute_kind: Mode2SemanticHybrid\nrequires_semantic_retriever: true\nexact_symbol_lookup: blurred\nsemantic_expansion_required: true"
                        : "context_route_verdict: wrong_project_wiki_context_route\nroute_kind: Mode2SemanticHybrid\nrequires_semantic_retriever: true", "wastes semantic retrieval and can blur exact symbol/location questions that BM25 evidence ids answer directly."),
                "answer_instead_of_abstain" =>
                    ("answer_instead_of_abstain", abstainStrong
                        ? "context_route_verdict: unsafe_answer_route\nanswer_before_retrieval: true\nretrieval_required_before_answer: false\nmissing_evidence_policy: ignored\nanswer_confidence_without_evidence: allowed"
                        : "context_route_verdict: unsafe_answer_route\nanswer_before_retrieval: true\nmissing_evidence_policy: ignored", "teaches the wiki to guess instead of returning a missing-evidence answer with required evidence families."),
                "ignore_wrong_project" =>
                    ("ignore_wrong_project", boundaryStrong
                        ? "context_route_verdict: invalid_answer_source\nroute_kind: Mode3LexicalEvidence\nevidence_scope: non_local_repository_allowed\nproject_boundary: cross_project_evidence_allowed"
                        : "context_route_verdict: unsafe_cross_project_route\nproject_boundary: use_other_project_if_similar", "violates project-local boundaries and produces citations from the wrong repository."),
                "ignore_stale_version" =>
                    ("ignore_stale_version", "context_route_verdict: unsafe_stale_document_route\nversion_policy: use_old_docs_if_symbol_matches", "can answer from stale branch or framework evidence instead of requiring version verification."),
                "no_pre_checks" =>
                    ("no_pre_checks", precheckProof
                        ? $"context_route_verdict: route_without_query_plan_precheck_proof\nprecheck_source: none\nquery_plan_prechecks_verified: not_verified\nretriever_selection_before_plan_checks: true\nroute_kind: {decision.RouteKind}\nanswer_permission: unverified"
                        : precheckBinary
                        ? "context_route_verdict: unsafe_unverified_route\nsafe_route: no\nverified_prechecks: no\nretriever_before_checks: yes\nanswer_permission: unverified"
                        : precheckClean
                        ? "context_route_verdict: unchecked_shortcut_route\nprecheck_status: skipped\nretriever_selection: first_step\nanswer_permission: unverified"
                        : precheckOnly
                        ? "context_route_verdict: invalid_short_circuit_route\nquery_plan_order: RetrieverSelection>AnswerPermission\nskipped_checks: WrongProjectCheck,StaleVersionCheck,MissingEvidenceCheck\nanswer_permission: unverified"
                        : precheckPlanStrong
                        ? "context_route_verdict: invalid_short_circuit_route\nquery_plan_order: RetrieverSelection>AnswerPermission\nskipped_checks: WrongProjectCheck,StaleVersionCheck,MissingEvidenceCheck\nanswer_permission: unverified"
                        : precheckStrong
                        ? "context_route_verdict: invalid_unchecked_route\nroute_decision_status: unusable_without_prechecks\nprecheck_failure: project_version_missing_evidence_semantic_checks_skipped\nanswer_permission: blocked"
                        : $"context_route_verdict: unverifiable_context_route\nroute_kind: {decision.RouteKind}\nprechecks_complete: false", precheckBinary
                            ? "cannot prove the route is safe because project, version, and evidence sufficiency were not checked."
                            : "makes route selection unverifiable and hides the reason a retriever was selected."),
                _ =>
                    ("underspecified_context_route", $"context_route_verdict: underspecified_context_route\nroute_kind: {decision.RouteKind}", "omits required pre-checks and can route complex wiki questions through the wrong retrieval mode.")
            };
            return $"{wrong.Item2}\n[WRONG: {wrong.Item1} {wrong.Item3}]";
        }

        return BuildFullRejected(rejectedType, decision);
    }

    private static string BuildFullRejected(string rejectedType, ProjectWikiContextRoutingDecision decision)
        => rejectedType switch
        {
            "always_bm25" =>
                $"Always route to Mode3 BM25 because lexical lookup is simpler than semantic retrieval.\n[WRONG: always_bm25 misses multi-hop developer reasoning where semantic expansion is required after the query plan identifies many evidence families.]",
            "always_semantic" =>
                $"Always route to Mode2 semantic GraphRAG even for direct symbol lookup.\n[WRONG: always_semantic wastes semantic retrieval and can blur exact symbol/location questions that BM25 evidence ids answer directly.]",
            "answer_instead_of_abstain" =>
                $"Route to {decision.RouteKind} and compose an answer even when required evidence is missing.\n[WRONG: answer_instead_of_abstain teaches the wiki to guess instead of returning a missing-evidence answer with required evidence families.]",
            "ignore_wrong_project" =>
                "Route to local retrieval and use a similar symbol from another project if local evidence is absent.\n[WRONG: ignore_wrong_project violates project-local boundaries and produces citations from the wrong repository.]",
            "ignore_stale_version" =>
                "Route to retrieval using old documentation when the symbol name matches.\n[WRONG: ignore_stale_version can answer from stale branch or framework evidence instead of requiring version verification.]",
            "no_pre_checks" =>
                $"Route to {decision.RouteKind} but skip project, version, missing-evidence, and semantic-availability pre-checks.\n[WRONG: no_pre_checks makes route selection unverifiable and hides the reason a retriever was selected.]",
            _ =>
                $"Use the shortest available retriever for {decision.RouteKind}.\n[WRONG: underspecified_context_route omits required pre-checks and can route complex wiki questions through the wrong retrieval mode.]"
        };

    private static bool IsDeveloperReasoningType(string questionType)
        => questionType is "entity_impact_route" or "field_addition_route" or "workflow_trace_route" or "regression_risk_route" or "architecture_boundary_route";

    private static ContextRoutingSeed[] BuildSeeds(string batchId, bool heldOut)
    {
        if (batchId.Contains("precheck-diverse", StringComparison.OrdinalIgnoreCase))
            return BuildDiverseNoPrecheckSeeds(heldOut);
        if (batchId.Contains("wrong-project-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildFocusedSeeds(heldOut ? HeldOutWrongProjectQuestions : WrongProjectQuestions);
        if (batchId.Contains("always-semantic-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildFocusedSeeds(heldOut ? HeldOutDirectQuestions : DirectQuestions);
        if (batchId.Contains("no-prechecks-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildFocusedSeeds((heldOut ? HeldOutSemanticQuestions : SemanticQuestions)
                .Where(seed => seed.RejectedType == "no_pre_checks")
                .ToArray());
        if (batchId.Contains("answer-abstain-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildFocusedSeeds(heldOut ? HeldOutMissingQuestions : MissingQuestions);

        return BuildBalancedSeeds(heldOut);
    }

    private static ContextRoutingSeed[] BuildDiverseNoPrecheckSeeds(bool heldOut)
    {
        var entities = heldOut
            ? new[] { "InvoiceHeader", "MealPlan", "MatchRequest", "PublishingRequest", "WorkItem", "Shipment", "ClientPreference", "VideoAsset", "ReminderRule", "SurveyResponse" }
            : new[] { "CampaignCreateDto", "ClientProfile", "DietPlan", "MediaAsset", "TaskAssignment", "GameSession", "OrderUpdateDto", "ContentRequest", "InvoiceLineDto", "UserRegistration" };
        var fields = heldOut
            ? new[] { "BillingCountry", "ProteinTarget", "LatencyBucket", "AudienceSegment", "SlaMinutes", "CarrierCode", "NotificationChannel", "ReviewStatus", "ReminderOffset", "ScoreWeight" }
            : new[] { "DiscountCode", "PreferredLanguage", "DailyCalorieTarget", "PublishingChannel", "EscalationPolicy", "MatchmakingRegion", "DeliveryWindow", "PublishingChannel", "TaxRegion", "MarketingConsent" };
        var processes = heldOut
            ? new[] { "invoice recalculation", "meal-plan regeneration", "matchmaking allocation", "publishing dispatch", "task escalation", "shipment return", "preference projection", "video review", "reminder scheduling", "survey scoring" }
            : new[] { "campaign creation", "client onboarding", "diet recalculation", "media processing", "task notification", "server allocation", "order fulfillment", "content publishing", "invoice approval", "registration activation" };
        var procs = heldOut
            ? new[] { "dbo.RepriceInvoice", "dbo.RebuildMealPlan", "dbo.AssignMatchServer", "dbo.DispatchPublishingRequest", "dbo.RecomputeSla" }
            : new[] { "dbo.RecalculateCampaign", "dbo.RefreshClientProfile", "dbo.RebuildDietPlan", "dbo.ProcessMediaAsset", "dbo.EscalateTask" };
        var rows = new List<ContextRoutingSeed>(50);
        for (var index = 0; index < 50; index++)
        {
            var slot = index % entities.Length;
            var project = (index % 5) switch
            {
                0 => "askapediaweb",
                1 => "genietasks",
                2 => "GenieDynamicServer",
                3 => "wizGameserver",
                _ => "geniesoftGameMVPServer"
            };
            var question = (index / 10) switch
            {
                0 => $"Before answering how adding {fields[slot]} to {entities[slot]} affects {processes[slot]}, which project, version, and evidence checks must run first?",
                1 => $"Plan wiki context retrieval for tracing {processes[slot]} from entry point to calculation, branch, persistence, output, and tests; do not answer yet.",
                2 => $"For stored procedure {procs[index % procs.Length]}, what prechecks must happen before selecting BM25, semantic, or abstain route?",
                3 => $"A developer asks whether changing {entities[slot]}.{fields[slot]} can break another project. What must the context router verify before retrieval?",
                _ => $"Choose wiki retrieval strategy for {processes[slot]} when documentation may be stale or missing. Which checks precede answer permission?"
            };
            var questionType = (index / 10) switch
            {
                0 => "entity_impact_route",
                1 => "workflow_trace_route",
                2 => "stored_procedure_route",
                3 => "wrong_project_route",
                _ => "regression_risk_route"
            };
            rows.Add(new ContextRoutingSeed(questionType, question, true, "no_pre_checks")
            {
                ProjectId = project
            });
        }

        return rows.ToArray();
    }

    private static ContextRoutingSeed[] BuildFocusedSeeds(IReadOnlyList<ContextRoutingSeed> source)
        => Enumerable.Range(0, 50)
            .Select(index => source[index % source.Count] with
            {
                ProjectId = (index % 5) switch
                {
                    0 => "askapediaweb",
                    1 => "genietasks",
                    2 => "GenieDynamicServer",
                    3 => "wizGameserver",
                    _ => "geniesoftGameMVPServer"
                }
            })
            .ToArray();

    private static ContextRoutingSeed[] BuildBalancedSeeds(bool heldOut)
    {
        var direct = heldOut ? HeldOutDirectQuestions : DirectQuestions;
        var semantic = heldOut ? HeldOutSemanticQuestions : SemanticQuestions;
        var missing = heldOut ? HeldOutMissingQuestions : MissingQuestions;
        var wrongProject = heldOut ? HeldOutWrongProjectQuestions : WrongProjectQuestions;
        var stale = heldOut ? HeldOutStaleQuestions : StaleQuestions;
        return direct
            .Concat(semantic)
            .Concat(missing)
            .Concat(wrongProject)
            .Concat(stale)
            .Take(50)
            .Select((seed, index) => seed with
            {
                ProjectId = (index % 5) switch
                {
                    0 => "askapediaweb",
                    1 => "genietasks",
                    2 => "GenieDynamicServer",
                    3 => "wizGameserver",
                    _ => "geniesoftGameMVPServer"
                }
            })
            .ToArray();
    }

    private static readonly ContextRoutingSeed[] DirectQuestions =
    [
        new("meaning_route", "What does Campaign.GetCampaignName do?", false, "always_semantic"),
        new("location_route", "Where is TaskService.AssignTask implemented?", false, "always_semantic"),
        new("parameters_route", "What parameters does MatchmakingService.CreateSession accept?", false, "always_semantic"),
        new("returns_route", "What does DynamicEntityMapper.MapFields return?", false, "always_semantic"),
        new("callers_route", "Who calls PlayerProfileService.UpdateRegion?", false, "always_semantic"),
        new("callees_route", "What functions does CampaignApprovalService.Approve call?", false, "always_semantic"),
        new("meaning_route", "Explain ReminderService.ScheduleReminder.", false, "always_semantic"),
        new("location_route", "Where is LobbyService.JoinLobby located?", false, "always_semantic"),
        new("parameters_route", "Which arguments enter SchemaValidator.ValidateEntity?", false, "always_semantic"),
        new("returns_route", "What return value comes from InventoryService.ApplyReward?", false, "always_semantic")
    ];

    private static readonly ContextRoutingSeed[] HeldOutDirectQuestions =
    [
        new("meaning_route", "What does CampaignRepository.Save do?", false, "always_semantic"),
        new("location_route", "Where is NotificationQueue.Enqueue implemented?", false, "always_semantic"),
        new("parameters_route", "What parameters does ServerAllocator.Allocate accept?", false, "always_semantic"),
        new("returns_route", "What does SchemaRepository.Load return?", false, "always_semantic"),
        new("callers_route", "Who calls InventoryRepository.Save?", false, "always_semantic"),
        new("callees_route", "What functions does PublicationQueue.Enqueue call?", false, "always_semantic"),
        new("meaning_route", "Explain TaskRepository.Save.", false, "always_semantic"),
        new("location_route", "Where is LobbyRepository.Save located?", false, "always_semantic"),
        new("parameters_route", "Which arguments enter ValueConverter.Convert?", false, "always_semantic"),
        new("returns_route", "What return value comes from RewardController.Claim?", false, "always_semantic")
    ];

    private static readonly ContextRoutingSeed[] SemanticQuestions =
    [
        new("entity_impact_route", "I want to add DiscountCode to CampaignCreateDto. Where does it travel and what can break?", true, "always_bm25"),
        new("field_addition_route", "Add PreferredLanguage to ClientProfile. Which mapper, validator, persistence path, callers, and tests should be checked?", true, "always_bm25"),
        new("workflow_trace_route", "Trace campaign creation from controller to repository and response projection.", true, "always_bm25"),
        new("regression_risk_route", "What queries estimate regression risk for changing DailyCalorieTarget calculation?", true, "always_bm25"),
        new("architecture_boundary_route", "Investigate the media processing boundary between upload API and queue worker.", true, "always_bm25"),
        new("entity_impact_route", "If I rename MediaAsset.Status, how should the wiki gather the full impact path?", true, "always_bm25"),
        new("workflow_trace_route", "Trace task escalation from assignment to notification and explain branch decisions.", true, "always_bm25"),
        new("field_addition_route", "Add DeliveryWindow to OrderUpdateDto and find every downstream process.", true, "always_bm25"),
        new("regression_risk_route", "How should the wiki inspect hidden breakage after adding EscalationPolicy?", true, "always_bm25"),
        new("architecture_boundary_route", "Explain the campaign module boundary before recommending a code change.", true, "always_bm25"),
        new("entity_impact_route", "Before changing ContentRequest.PublishingChannel, what should be retrieved first?", true, "always_bm25"),
        new("workflow_trace_route", "Follow invoice approval from line update to total calculation.", true, "always_bm25"),
        new("field_addition_route", "Add MarketingConsent to user registration and inspect readers and writers.", true, "always_bm25"),
        new("regression_risk_route", "Which code paths can break if GameSession.MatchmakingRegion changes?", true, "always_bm25"),
        new("architecture_boundary_route", "Inspect task orchestration boundary between scheduler and notification provider.", true, "always_bm25"),
        new("entity_impact_route", "What should the wiki search before changing DietPlan.DailyCalorieTarget?", true, "always_bm25"),
        new("workflow_trace_route", "Trace content publishing from draft save to external provider.", true, "always_bm25"),
        new("field_addition_route", "Add TaxRegion to InvoiceLineDto and identify persistence and projections.", true, "always_bm25"),
        new("regression_risk_route", "Estimate risk for renaming Campaign.DisplayName.", true, "always_bm25"),
        new("architecture_boundary_route", "Investigate game server boundary between matchmaking and allocation.", true, "always_bm25"),
        new("entity_impact_route", "Before changing TaskAssignment.EscalationPolicy, which wiki contexts should be gathered?", true, "no_pre_checks"),
        new("workflow_trace_route", "Trace user registration from form post to welcome notification.", true, "no_pre_checks"),
        new("field_addition_route", "Add MatchmakingRegion to GameSession and decide retrieval mode.", true, "no_pre_checks"),
        new("regression_risk_route", "Find route strategy for adding PublishingChannel to content workflow.", true, "no_pre_checks"),
        new("architecture_boundary_route", "Choose context route for diet planner boundary before answering.", true, "no_pre_checks")
    ];

    private static readonly ContextRoutingSeed[] HeldOutSemanticQuestions =
    [
        new("entity_impact_route", "I want to add ReferralCode to CampaignUpdateDto. Where does it travel and what can break?", true, "always_bm25"),
        new("field_addition_route", "Add NotificationChannel to ClientPreference. Which mapper, validator, persistence path, callers, and tests should be checked?", true, "always_bm25"),
        new("workflow_trace_route", "Trace meal-plan regeneration after questionnaire edit.", true, "always_bm25"),
        new("regression_risk_route", "What queries estimate regression risk for changing ProteinTarget?", true, "always_bm25"),
        new("architecture_boundary_route", "Investigate video transcoding from upload API to worker.", true, "always_bm25"),
        new("entity_impact_route", "If I rename Order.CarrierCode, how should the wiki gather the full impact path?", true, "always_bm25"),
        new("workflow_trace_route", "Trace work item reassignment from queue to notification.", true, "always_bm25"),
        new("field_addition_route", "Add AudienceSegment to PublishingRequest and find every downstream process.", true, "always_bm25"),
        new("regression_risk_route", "How should the wiki inspect hidden breakage after adding SlaMinutes?", true, "always_bm25"),
        new("architecture_boundary_route", "Explain the campaign approval boundary before recommending a code change.", true, "always_bm25"),
        new("entity_impact_route", "Before changing InvoiceHeader.BillingCountry, what should be retrieved first?", true, "always_bm25"),
        new("workflow_trace_route", "Follow shipment return from API request to stock adjustment.", true, "always_bm25"),
        new("field_addition_route", "Add ConsentSource to registration form and inspect readers and writers.", true, "always_bm25"),
        new("regression_risk_route", "Which code paths can break if MatchRequest.LatencyBucket changes?", true, "always_bm25"),
        new("architecture_boundary_route", "Inspect task boundary between project board and reminder service.", true, "always_bm25"),
        new("entity_impact_route", "What should the wiki search before changing MealPlan.ProteinTarget?", true, "always_bm25"),
        new("workflow_trace_route", "Trace article scheduling from draft publish time to provider dispatch.", true, "always_bm25"),
        new("field_addition_route", "Add BillingCountry to InvoiceHeaderDto and identify persistence and projections.", true, "always_bm25"),
        new("regression_risk_route", "Estimate risk for renaming ClientProfile.Locale.", true, "always_bm25"),
        new("architecture_boundary_route", "Investigate game lobby boundary between queue and server allocator.", true, "always_bm25"),
        new("entity_impact_route", "Before changing WorkItem.SlaMinutes, which wiki contexts should be gathered?", true, "no_pre_checks"),
        new("workflow_trace_route", "Trace account activation from email confirmation to session creation.", true, "no_pre_checks"),
        new("field_addition_route", "Add LatencyBucket to MatchRequest and decide retrieval mode.", true, "no_pre_checks"),
        new("regression_risk_route", "Find route strategy for adding AudienceSegment to publishing workflow.", true, "no_pre_checks"),
        new("architecture_boundary_route", "Choose context route for video boundary before answering.", true, "no_pre_checks")
    ];

    private static readonly ContextRoutingSeed[] MissingQuestions =
    [
        new("missing_evidence_route", "Do not answer yet: what evidence is missing for UnknownCampaignSync?", false, "answer_instead_of_abstain"),
        new("missing_evidence_route", "What evidence is needed for undocumented field LegacyDeliveryWindow?", false, "answer_instead_of_abstain"),
        new("missing_evidence_route", "Which evidence is missing before answering GhostPublishingChannel?", false, "answer_instead_of_abstain"),
        new("missing_evidence_route", "UnknownDietProjection is not documented. What should the wiki do?", false, "answer_instead_of_abstain"),
        new("missing_evidence_route", "No accepted documentation exists for OldTaskEscalation. Should the wiki answer?", false, "answer_instead_of_abstain")
    ];

    private static readonly ContextRoutingSeed[] HeldOutMissingQuestions =
    [
        new("missing_evidence_route", "Do not answer yet: what evidence is missing for UnknownInvoiceProjection?", false, "answer_instead_of_abstain"),
        new("missing_evidence_route", "What evidence is needed for undocumented field LegacyProteinTarget?", false, "answer_instead_of_abstain"),
        new("missing_evidence_route", "Which evidence is missing before answering GhostReferralCode?", false, "answer_instead_of_abstain"),
        new("missing_evidence_route", "MissingCampaignWorkflow is not documented. What should the wiki do?", false, "answer_instead_of_abstain"),
        new("missing_evidence_route", "No accepted documentation exists for OldCampaignApproval. Should the wiki answer?", false, "answer_instead_of_abstain")
    ];

    private static readonly ContextRoutingSeed[] WrongProjectQuestions =
    [
        new("wrong_project_route", "This looks like a GenieDynamicServer entity but I am asking askapediaweb. Can the wiki use it?", false, "ignore_wrong_project"),
        new("wrong_project_route", "Question names a wizGameserver symbol while the project is genietasks. Should we answer?", false, "ignore_wrong_project"),
        new("wrong_project_route", "Use a different project entity if Campaign is missing locally.", false, "ignore_wrong_project"),
        new("wrong_project_route", "Can this wrong project service answer my local code question?", false, "ignore_wrong_project"),
        new("wrong_project_route", "If local evidence is absent, borrow similar code from another repository.", false, "ignore_wrong_project")
    ];

    private static readonly ContextRoutingSeed[] HeldOutWrongProjectQuestions =
    [
        new("wrong_project_route", "This looks like an askapediaweb entity but I am asking wizGameserver. Can the wiki use it?", false, "ignore_wrong_project"),
        new("wrong_project_route", "Question names a geniesoftGameMVPServer symbol while the project is GenieDynamicServer. Should we answer?", false, "ignore_wrong_project"),
        new("wrong_project_route", "Use a different project entity if TaskAssignment is missing locally.", false, "ignore_wrong_project"),
        new("wrong_project_route", "Can this wrong project controller answer my local code question?", false, "ignore_wrong_project"),
        new("wrong_project_route", "If local evidence is absent, borrow similar docs from another repository.", false, "ignore_wrong_project")
    ];

    private static readonly ContextRoutingSeed[] StaleQuestions =
    [
        new("stale_documentation_route", "Before trusting stale documentation for Campaign.DisplayName, what should happen?", false, "ignore_stale_version"),
        new("stale_documentation_route", "This branch has old documentation for ClientProfile.Language. Should we answer from it?", false, "ignore_stale_version"),
        new("stale_documentation_route", "Outdated docs mention MediaAsset.Status. Can the wiki use them directly?", false, "ignore_stale_version"),
        new("stale_documentation_route", "Old documentation says Order.DeliveryDate exists. Should retrieval continue?", false, "ignore_stale_version"),
        new("stale_documentation_route", "Stale Task.Priority docs are the only hit. What route is correct?", false, "ignore_stale_version")
    ];

    private static readonly ContextRoutingSeed[] HeldOutStaleQuestions =
    [
        new("stale_documentation_route", "Before trusting stale documentation for Campaign.InternalName, what should happen?", false, "ignore_stale_version"),
        new("stale_documentation_route", "This branch has old documentation for ClientProfile.Locale. Should we answer from it?", false, "ignore_stale_version"),
        new("stale_documentation_route", "Outdated docs mention VideoAsset.ReviewStatus. Can the wiki use them directly?", false, "ignore_stale_version"),
        new("stale_documentation_route", "Old documentation says Shipment.CarrierCode exists. Should retrieval continue?", false, "ignore_stale_version"),
        new("stale_documentation_route", "Stale WorkItem.Urgency docs are the only hit. What route is correct?", false, "ignore_stale_version")
    ];

    private sealed record ContextRoutingSeed(
        string QuestionType,
        string Question,
        bool AllowSemantic,
        string RejectedType)
    {
        public string ProjectId { get; init; } = "askapediaweb";
    }
}
