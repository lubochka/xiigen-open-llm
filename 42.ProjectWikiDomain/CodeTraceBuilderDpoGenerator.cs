// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class CodeTraceBuilderDpoGenerator
{
    private readonly CodeTraceBuilderUnit _builder;

    public CodeTraceBuilderDpoGenerator(CodeTraceBuilderUnit? builder = null)
    {
        _builder = builder ?? new CodeTraceBuilderUnit();
    }

    public Task<OperationResult<CodeTraceBuilderDpoBatch>> BuildBatchAsync(
        string batchId = "code-trace-builder-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<CodeTraceBuilderDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var seeds = BuildSeeds(batchId, heldOut);
        var compact = batchId.Contains("compact", StringComparison.OrdinalIgnoreCase);
        var ultraCompact = batchId.Contains("ultra-compact", StringComparison.OrdinalIgnoreCase);
        var wrongFirst = batchId.Contains("wrong-first", StringComparison.OrdinalIgnoreCase);
        var callerAbstract = batchId.Contains("caller-abstract", StringComparison.OrdinalIgnoreCase);
        var labelOnly = batchId.Contains("label-only", StringComparison.OrdinalIgnoreCase);
        var binaryLabels = batchId.Contains("binary-labels", StringComparison.OrdinalIgnoreCase);
        var evidenceRich = batchId.Contains("evidence-rich", StringComparison.OrdinalIgnoreCase);
        var terminalChain = batchId.Contains("terminal-chain", StringComparison.OrdinalIgnoreCase);
        var highContrast = batchId.Contains("high-contrast", StringComparison.OrdinalIgnoreCase);
        var pairs = seeds.Select((seed, index) =>
        {
            var analysis = _builder.Analyze(seed.ProjectId, seed.SourceId, seed.SourceText).Value!;
            return new CodeTraceBuilderDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                seed.ProjectId,
                seed.SourceId,
                seed.SourceText,
                analysis.Methods,
                analysis.CallEdges,
                analysis.ValueFlows,
                BuildPrompt(seed.SourceText),
                BuildChosen(seed.RejectedType, analysis, compact, ultraCompact, binaryLabels, evidenceRich, terminalChain, highContrast),
                BuildRejected(seed.RejectedType, analysis, compact, wrongFirst, callerAbstract, labelOnly, binaryLabels, highContrast),
                seed.RejectedType);
        }).ToArray();
        var wrongAnnotationRate = pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return Task.FromResult(OperationResult<CodeTraceBuilderDpoBatch>.Ok(new CodeTraceBuilderDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())));
    }

    private static string BuildPrompt(string sourceText)
        => $$"""
           Task: build source-grounded code trace for bottom-up wiki documentation.
           Required: methods, call_edges, value_flows, no invented callers, no invented callees.
           Check: include every direct call, every argument handoff, and terminal return path.
           Source:
           {{sourceText}}
           """;

    private static string BuildChosen(
        string rejectedType,
        CodeTraceBuilderAnalysis analysis,
        bool compact,
        bool ultraCompact,
        bool binaryLabels,
        bool evidenceRich,
        bool terminalChain,
        bool highContrast)
    {
        if (ultraCompact)
            return "trace_verdict: complete_source_grounded_trace\nall_methods_call_edges_value_flows_present: true\ninvented_symbols: false";

        if (highContrast)
            return BuildHighContrastChosen(analysis);

        if (compact)
        {
            var defectVector = binaryLabels
                ? "missing_callee: false\nmissing_value_flow: false\nwrong_caller: false\ninvented_callee: false\nincomplete_trace: false\n"
                : "";
            var core = $"trace_verdict: source_grounded_complete_trace\n{defectVector}methods_count: {analysis.Methods.Length}\ncall_edges_count: {analysis.CallEdges.Length}\nvalue_flows_count: {analysis.ValueFlows.Length}\ncall_edge_status: all_source_visible_callees_present\nvalue_flow_status: arguments_and_return_usage_present\ncaller_status: no_invented_callers\nterminal_return_status: repository_result_returns_to_caller";
            if (evidenceRich)
                return $"{core}\ncall_edges: {string.Join("; ", analysis.CallEdges)}\nvalue_flows: {string.Join("; ", analysis.ValueFlows)}\nterminal_return_chain: {BuildTerminalReturnChain(analysis)}\nside_effect_status: non_returning_audit_or_event_calls_preserved";

            if (terminalChain)
                return $"{core}\nkey_value_flows: {string.Join("; ", analysis.ValueFlows.Take(3))}\nterminal_return_chain: {BuildTerminalReturnChain(analysis)}";

            return core;
        }

        var body = $"methods: {string.Join(", ", analysis.Methods)}\ncall_edges: {string.Join(", ", analysis.CallEdges)}\nvalue_flows: {string.Join(", ", analysis.ValueFlows)}";
        return rejectedType switch
        {
            "missing_callee" => $"trace_verdict: complete_callee_trace\ncallee_coverage: all_source_visible_calls_present\ncaller_to_callee_contract: every direct method invocation must appear in call_edges\n{body}",
            "missing_value_flow" => $"trace_verdict: complete_value_flow_trace\nvalue_flow_coverage: arguments_and_return_usage_present\nargument_and_return_contract: every argument passed and returned value used must appear in value_flows\n{body}",
            "wrong_caller" => $"trace_verdict: source_grounded_caller_trace\ncaller_evidence_check: source_declared_callers_only\ncaller_contract: do not invent external entry callers absent from source\n{body}",
            "incomplete_trace" => BuildCompleteTerminalTrace(analysis),
            "invented_callee" => $"source_evidence_check: source_visible_callees_only\ncallee_evidence_contract: no_extra_dependency\n{body}",
            _ => body
        };
    }

    private static string BuildRejected(
        string rejectedType,
        CodeTraceBuilderAnalysis analysis,
        bool compact,
        bool wrongFirst,
        bool callerAbstract,
        bool labelOnly,
        bool binaryLabels,
        bool highContrast)
    {
        if (compact)
        {
            if (highContrast)
                return BuildHighContrastRejected(rejectedType, analysis, wrongFirst);

            var wrong = rejectedType switch
            {
                "missing_callee" =>
                    ("missing_callee", "omits caller-to-callee evidence, so higher-level documentation cannot propagate inner function meaning.", "trace_verdict: incomplete_trace\ncall_edge_status: missing_source_visible_callees\nvalue_flow_status: unknown"),
                "missing_value_flow" =>
                    ("missing_value_flow", "hides arguments and return usage, so entity-impact answers cannot explain what travels through the call.", "trace_verdict: incomplete_trace\ncall_edge_status: present\nvalue_flow_status: missing_arguments_and_return_usage"),
                "wrong_caller" =>
                    ("wrong_caller", "invents or accepts an entry caller absent from source, so the wiki routes impact analysis to the wrong workflow.", BuildWrongCallerCompactRejected(analysis, callerAbstract, labelOnly)),
                "invented_callee" =>
                    ("invented_callee", "adds a plausible dependency that is not present in source, so the wiki routes field-impact work through a fake service.", "trace_verdict: source_conflicting_trace\ncall_edge_status: invented_policy_dependency"),
                _ =>
                    ("incomplete_trace", "stops before the repository result returns through Persist and Save, so the wiki cannot explain the saved value returned to the caller.", "trace_verdict: shallow_incomplete_trace\nterminal_return_status: missing_repository_result_to_caller")
            };
            var marker = $"[WRONG: {wrong.Item1} {wrong.Item2}]";
            var body = binaryLabels ? $"{BuildDefectVector(wrong.Item1)}\n{wrong.Item3}" : wrong.Item3;
            return wrongFirst ? $"{marker}\n{body}" : $"{body}\n{marker}";
        }

        return rejectedType switch
        {
            "missing_callee" =>
                $"methods: {string.Join(", ", analysis.Methods)}\ncall_edges: none\n[WRONG: missing_callee omits caller to callee evidence, so higher-level documentation cannot propagate inner function meaning.]",
            "missing_value_flow" =>
                $"methods: {string.Join(", ", analysis.Methods)}\ncall_edges: {string.Join(", ", analysis.CallEdges)}\nvalue_flows: none\n[WRONG: missing_value_flow hides arguments and return usage, so entity-impact answers cannot explain what travels through the call.]",
            "wrong_caller" =>
                BuildWrongCallerRejected(analysis),
            "invented_callee" =>
                BuildInventedCalleeRejected(analysis),
            _ =>
                BuildIncompleteTraceRejected(analysis)
        };
    }

    private static string BuildHighContrastChosen(CodeTraceBuilderAnalysis analysis)
    {
        var firstEdge = analysis.CallEdges.FirstOrDefault("none");
        var lastFlow = analysis.ValueFlows.LastOrDefault("none");
        return $$"""
               PASS_CODE_TRACE
               source_grounded: yes
               valid_trace: yes
               failure_code: none
               direct_calls_complete: yes
               argument_flow_complete: yes
               return_flow_complete: yes
               extra_symbols_present: no
               terminal_result_path: repository_to_service_to_controller
               evidence_anchor_call: {{firstEdge}}
               evidence_anchor_value: {{lastFlow}}
               """;
    }

    private static string BuildHighContrastRejected(
        string rejectedType,
        CodeTraceBuilderAnalysis analysis,
        bool wrongFirst)
    {
        var failure = rejectedType switch
        {
            "missing_callee" => ("MISSING_CALLEE", "direct_calls_complete: no", "drops a source-visible method call, so bottom-up documentation loses callee meaning."),
            "missing_value_flow" => ("MISSING_VALUE_FLOW", "argument_flow_complete: no\nreturn_flow_complete: no", "drops argument or return transit, so entity-impact answers cannot say what value travels where."),
            "wrong_caller" => ("WRONG_CALLER", $"fake_entry: {BuildInventedCaller(analysis.Methods.FirstOrDefault("Unknown.Target"))}", "adds a caller absent from source, so workflow routing starts from the wrong entry point."),
            "invented_callee" => ("INVENTED_CALLEE", $"fake_dependency: {ExtractDomainName(analysis.Methods.FirstOrDefault("Unknown.Save"))}PolicyService", "adds a plausible dependency absent from source, so the wiki routes through fake code."),
            _ => ("INCOMPLETE_TRACE", "terminal_result_path: missing\nreturn_flow_complete: no", "stops before repository result returns through service and controller, so the wiki cannot answer what the caller receives.")
        };
        var body = $$"""
                   FAIL_CODE_TRACE
                   source_grounded: no
                   valid_trace: no
                   failure_code: {{failure.Item1}}
                   {{failure.Item2}}
                   evidence_status: insufficient_for_wiki_answer
                   """;
        var marker = $"[WRONG: {rejectedType} {failure.Item3}]";
        return wrongFirst ? $"{marker}\n{body}" : $"{body}\n{marker}";
    }

    private static string BuildDefectVector(string rejectedType)
        => string.Join("\n", new[]
        {
            $"missing_callee: {FormatBool(rejectedType == "missing_callee")}",
            $"missing_value_flow: {FormatBool(rejectedType == "missing_value_flow")}",
            $"wrong_caller: {FormatBool(rejectedType == "wrong_caller")}",
            $"invented_callee: {FormatBool(rejectedType == "invented_callee")}",
            $"incomplete_trace: {FormatBool(rejectedType == "incomplete_trace")}"
        });

    private static string FormatBool(bool value)
        => value ? "true" : "false";

    private static string BuildWrongCallerCompactRejected(CodeTraceBuilderAnalysis analysis, bool callerAbstract, bool labelOnly)
    {
        if (labelOnly)
            return "trace_verdict: wrong_trace\ntrace_defect: wrong_caller";

        if (callerAbstract)
            return "trace_verdict: source_conflicting_trace\ncaller_evidence_status: invented_caller_not_in_source\nentry_caller_policy: reject_external_entry_without_source_evidence";

        return $"trace_verdict: source_conflicting_trace\ncaller_status: {BuildInventedCallerStatus(analysis)}";
    }

    private static string BuildWrongCallerRejected(CodeTraceBuilderAnalysis analysis)
    {
        var target = analysis.Methods.FirstOrDefault("Unknown.Target");
        var inventedCaller = BuildInventedCaller(target);
        return $"methods: {string.Join(", ", analysis.Methods)}\ncall_edges: {inventedCaller}->{target}, {string.Join(", ", analysis.CallEdges.Skip(1))}\nvalue_flows: {string.Join(", ", analysis.ValueFlows)}\n[WRONG: wrong_caller invents an entry caller that is not present in the source trace, so the wiki routes impact analysis to the wrong workflow.]";
    }

    private static string BuildInventedCallerStatus(CodeTraceBuilderAnalysis analysis)
        => BuildInventedCaller(analysis.Methods.FirstOrDefault("Unknown.Target")).Replace('.', '_').ToLowerInvariant();

    private static string BuildInventedCaller(string symbol)
    {
        var domain = ExtractDomainName(symbol);
        var variants = new[]
        {
            $"Admin{domain}Controller.Submit",
            $"{domain}BackgroundJob.Run",
            $"{domain}ApiController.Post",
            $"{domain}Page.Save_Click",
            $"{domain}Workflow.Start"
        };
        var index = Math.Abs(symbol.Aggregate(0, (current, character) => current + character)) % variants.Length;
        return variants[index];
    }

    private static string BuildInventedCalleeRejected(CodeTraceBuilderAnalysis analysis)
    {
        var entry = analysis.Methods.FirstOrDefault(method => method.Contains(".Save", StringComparison.Ordinal), analysis.Methods.FirstOrDefault("Unknown.Source"));
        var domain = ExtractDomainName(entry);
        var invented = $"{domain}PolicyService.Validate{domain}";
        return $"source_evidence_check: invented_callee_absent_from_source\ninvented_dependency_contract: fake_policy_dependency_inserted\nmethods: {string.Join(", ", analysis.Methods)}, {invented}\ncall_edges: {string.Join(", ", analysis.CallEdges)}, {entry}->{invented}\nvalue_flows: {string.Join(", ", analysis.ValueFlows)}, {entry}->{invented}(id|normalized)->assigned\n[WRONG: invented_callee adds a plausible policy-validation dependency that is not present in the source trace; teaching it would make the wiki route field-impact work through a fake service.]";
    }

    private static string BuildIncompleteTraceRejected(CodeTraceBuilderAnalysis analysis)
    {
        var repositoryEdge = analysis.CallEdges.FirstOrDefault(edge => edge.Contains("Repository", StringComparison.Ordinal), "");
        var saveMethod = analysis.Methods.FirstOrDefault(method => method.Contains(".Save", StringComparison.Ordinal), "Unknown.Save");
        var normalizeMethod = analysis.Methods.FirstOrDefault(method => method.Contains(".Normalize", StringComparison.Ordinal), "Unknown.Normalize");
        var persistMethod = analysis.Methods.FirstOrDefault(method => method.Contains(".Persist", StringComparison.Ordinal), "Unknown.Persist");
        return $"trace_verdict: shallow_incomplete_trace\ntrace_depth: local_chain_only\nmethods: {string.Join(", ", analysis.Methods)}\npartial_call_chain: {saveMethod}->{normalizeMethod}; {saveMethod}->{persistMethod}\nargument_transit: value->{normalizeMethod}->normalized\nreturn_transit: missing_repository_result_to_save_caller\nomitted_terminal_return_edge: {repositoryEdge}\n[WRONG: incomplete_trace records the local call chain but stops before the repository result returns through Persist and Save, so the wiki cannot explain the saved value returned to the caller.]";
    }

    private static string BuildCompleteTerminalTrace(CodeTraceBuilderAnalysis analysis)
    {
        var saveMethod = analysis.Methods.FirstOrDefault(method => method.Contains(".Save", StringComparison.Ordinal), "Unknown.Save");
        var normalizeMethod = analysis.Methods.FirstOrDefault(method => method.Contains(".Normalize", StringComparison.Ordinal), "Unknown.Normalize");
        var persistMethod = analysis.Methods.FirstOrDefault(method => method.Contains(".Persist", StringComparison.Ordinal), "Unknown.Persist");
        var repositoryMethod = analysis.Methods.FirstOrDefault(method => method.Contains("Repository", StringComparison.Ordinal), "Unknown.Repository.Store");
        return $"trace_verdict: complete_terminal_return_trace\ntrace_depth: full_chain_to_repository_return\nmethods: {string.Join(", ", analysis.Methods)}\ncomplete_call_chain: {saveMethod}->{normalizeMethod}; {saveMethod}->{persistMethod}; {persistMethod}->{repositoryMethod}\nargument_transit: value->{normalizeMethod}->normalized; id+normalized->{persistMethod}->{repositoryMethod}\nreturn_transit: {repositoryMethod} result->{persistMethod}->{saveMethod}->caller\nvalue_flows: {string.Join(", ", analysis.ValueFlows)}";
    }

    private static string BuildTerminalReturnChain(CodeTraceBuilderAnalysis analysis)
    {
        var entryMethod = analysis.Methods.FirstOrDefault(method => method.Contains("Controller", StringComparison.Ordinal), analysis.Methods.FirstOrDefault("Unknown.Entry"));
        var saveMethod = analysis.Methods.FirstOrDefault(method => method.Contains("Service", StringComparison.Ordinal) && method.Contains(".Save", StringComparison.Ordinal), analysis.Methods.FirstOrDefault(method => method.Contains(".Save", StringComparison.Ordinal), "Unknown.Save"));
        var persistMethod = analysis.Methods.FirstOrDefault(method => method.Contains(".Persist", StringComparison.Ordinal), "Unknown.Persist");
        var repositoryMethod = analysis.Methods.FirstOrDefault(method => method.Contains("Repository", StringComparison.Ordinal), "Unknown.Repository.Store");

        if (!string.Equals(entryMethod, saveMethod, StringComparison.Ordinal) &&
            !string.Equals(saveMethod, "Unknown.Save", StringComparison.Ordinal))
            return $"{repositoryMethod} result->{persistMethod}->{saveMethod}->{entryMethod}->caller";

        return $"{repositoryMethod} result->{persistMethod}->{entryMethod}->caller";
    }

    private static string ExtractDomainName(string symbol)
    {
        var method = symbol.Split('.').LastOrDefault() ?? "Domain";
        if (method.StartsWith("Save", StringComparison.Ordinal) && method.Length > "Save".Length)
            return method["Save".Length..];

        return "Domain";
    }

    private static CodeTraceSeed[] BuildSeeds(string batchId, bool heldOut)
    {
        if (batchId.Contains("realistic-multihop", StringComparison.OrdinalIgnoreCase) &&
            batchId.Contains("invented-callee-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildRealisticMultihopSeeds(heldOut, _ => "invented_callee");
        if (batchId.Contains("realistic-multihop", StringComparison.OrdinalIgnoreCase) &&
            batchId.Contains("wrong-caller-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildRealisticMultihopSeeds(heldOut, _ => "wrong_caller");
        if (batchId.Contains("realistic-multihop", StringComparison.OrdinalIgnoreCase) &&
            batchId.Contains("missing-value-flow-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildRealisticMultihopSeeds(heldOut, _ => "missing_value_flow");
        if (batchId.Contains("realistic-multihop", StringComparison.OrdinalIgnoreCase) &&
            batchId.Contains("incomplete-trace-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildRealisticMultihopSeeds(heldOut, _ => "incomplete_trace");
        if (batchId.Contains("realistic-multihop", StringComparison.OrdinalIgnoreCase) &&
            batchId.Contains("missing-callee-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildRealisticMultihopSeeds(heldOut, _ => "missing_callee");
        if (batchId.Contains("incomplete-trace-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, _ => "incomplete_trace");
        if (batchId.Contains("missing-value-flow-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, _ => "missing_value_flow");
        if (batchId.Contains("wrong-caller-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, _ => "wrong_caller");
        if (batchId.Contains("invented-callee-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, _ => "invented_callee");
        if (batchId.Contains("missing-callee-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, _ => "missing_callee");
        if (batchId.Contains("realistic-multihop", StringComparison.OrdinalIgnoreCase))
            return BuildRealisticMultihopSeeds(heldOut, index => (index % 5) switch
            {
                0 => "missing_callee",
                1 => "missing_value_flow",
                2 => "wrong_caller",
                3 => "invented_callee",
                _ => "incomplete_trace"
            });

        return BuildSeeds(heldOut, index => (index % 5) switch
        {
            0 => "missing_callee",
            1 => "missing_value_flow",
            2 => "wrong_caller",
            3 => "invented_callee",
            _ => "incomplete_trace"
        });
    }

    private static CodeTraceSeed[] BuildSeeds(bool heldOut, Func<int, string> rejectedTypeForIndex)
    {
        var domains = heldOut
            ? new[] { "Invoice", "Profile", "Message", "Shipment", "Ticket", "Payment", "Subscription", "Report", "Alert", "Session" }
            : new[] { "Campaign", "Order", "Client", "DietPlan", "MediaAsset", "Task", "Game", "Article", "Booking", "Survey" };
        var rows = new List<CodeTraceSeed>(50);
        for (var repeat = 0; repeat < 5; repeat++)
        {
            foreach (var domain in domains)
            {
                var service = $"{domain}Service{repeat}";
                var repo = $"{domain}Repository{repeat}";
                var controller = $"{domain}Controller{repeat}";
                var rejected = rejectedTypeForIndex(rows.Count);
                var source = $$"""
                    public class {{controller}}
                    {
                        public string Save{{domain}}(int id, string value)
                        {
                            var normalized = Normalize{{domain}}(value);
                            return Persist{{domain}}(id, normalized);
                        }

                        public string Normalize{{domain}}(string value)
                        {
                            return value.Trim();
                        }

                        public string Persist{{domain}}(int id, string normalized)
                        {
                            return {{repo}}.Store{{domain}}(id, normalized);
                        }
                    }

                    public class {{repo}}
                    {
                        public static string Store{{domain}}(int id, string normalized)
                        {
                            return normalized + id.ToString();
                        }
                    }
                    """;
                rows.Add(new CodeTraceSeed(
                    "generic-dotnet-docs",
                    $"{domain.ToLowerInvariant()}-{repeat}.cs",
                    source,
                    rejected));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static CodeTraceSeed[] BuildRealisticMultihopSeeds(bool heldOut, Func<int, string> rejectedTypeForIndex)
    {
        var domains = heldOut
            ? new[] { "Invoice", "Profile", "Message", "Shipment", "Ticket", "Payment", "Subscription", "Report", "Alert", "Session" }
            : new[] { "Campaign", "Order", "Client", "DietPlan", "MediaAsset", "Task", "Game", "Article", "Booking", "Survey" };
        var rows = new List<CodeTraceSeed>(50);
        for (var repeat = 0; repeat < 5; repeat++)
        {
            foreach (var domain in domains)
            {
                var controller = $"{domain}Controller{repeat}";
                var service = $"{domain}Service{repeat}";
                var validator = $"{domain}Validator{repeat}";
                var repository = $"{domain}Repository{repeat}";
                var audit = $"{domain}AuditService{repeat}";
                var rejected = rejectedTypeForIndex(rows.Count);
                var source = $$"""
                    public class {{controller}}
                    {
                        public string Save{{domain}}(int id, string value, string actor)
                        {
                            return {{service}}.Save{{domain}}(id, value, actor);
                        }
                    }

                    public class {{service}}
                    {
                        public static string Save{{domain}}(int id, string value, string actor)
                        {
                            var valid = {{validator}}.Validate{{domain}}(value);
                            var normalized = Normalize{{domain}}(value);
                            var saved = Persist{{domain}}(id, normalized);
                            {{audit}}.Record{{domain}}Audit(id, actor, saved);
                            return saved;
                        }

                        public static string Normalize{{domain}}(string value)
                        {
                            return value.Trim().ToUpperInvariant();
                        }

                        public static string Persist{{domain}}(int id, string normalized)
                        {
                            return {{repository}}.Store{{domain}}(id, normalized);
                        }
                    }

                    public class {{validator}}
                    {
                        public static string Validate{{domain}}(string value)
                        {
                            return value;
                        }
                    }

                    public class {{repository}}
                    {
                        public static string Store{{domain}}(int id, string normalized)
                        {
                            return normalized + id.ToString();
                        }
                    }

                    public class {{audit}}
                    {
                        public static string Record{{domain}}Audit(int id, string actor, string saved)
                        {
                            return actor + saved + id.ToString();
                        }
                    }
                    """;
                rows.Add(new CodeTraceSeed(
                    "generic-dotnet-docs",
                    $"{domain.ToLowerInvariant()}-realistic-{repeat}.cs",
                    source,
                    rejected));
            }
        }

        return rows.Take(50).ToArray();
    }

    private sealed record CodeTraceSeed(
        string ProjectId,
        string SourceId,
        string SourceText,
        string RejectedType);
}
