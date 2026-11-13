# Real Project Wiki Validation Phase Report

report_scope: real_project_wiki_validation_after_query_intent_fix
report_status: VALIDATION_GATE_PASSED
training_promotion_claim: false
full_wiki_done_claim: false
source_of_truth: persisted runner artifacts only

## Source Artifacts

source_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-real-project-wiki-validation\real-project-validation-after-query-intent-fix-2026-05-22.json
summary_artifact: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-real-project-wiki-validation\REAL-PROJECT-WIKI-VALIDATION-AFTER-QUERY-INTENT-FIX-SUMMARY.json
run_meta: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-real-project-wiki-validation\REAL-PROJECT-WIKI-VALIDATION-AFTER-QUERY-INTENT-FIX-RUN-META.json

## Raw Metrics

project_count: 5
evidence_record_count: 616
total_question_count: 6393
exact_evidence_hit_count: 6393
exact_evidence_accuracy: 1.0
query_planning_invocation_rate: 1.0
entity_impact_trace_pass_rate: 1.0
cpu_ms: 23750
elapsed_ms: 22874.23179999999
peak_ram_mb: 16.28997802734375
is_passed: true

## Per Project Metrics

| Project | Evidence records | Questions | Stored procedure questions | Exact evidence accuracy | Query planning invocation | Entity impact trace pass | CPU ms | Elapsed ms | Peak RAM MB | Passed |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| wizGameserver | 114 | 1185 | 0 | 1.0 | 1.0 | 1.0 | 4312.5 | 3790.3024 | 9.785957336425781 | true |
| GenieDynamicServer | 240 | 2445 | 0 | 1.0 | 1.0 | 1.0 | 14546.875 | 14347.084199999996 | 14.179306030273438 | true |
| geniesoftGameMVPServer | 6 | 105 | 0 | 1.0 | 1.0 | 1.0 | 31.25 | 26.73760000000001 | 13.07672119140625 | true |
| genietasks | 74 | 785 | 0 | 1.0 | 1.0 | 1.0 | 921.875 | 753.4518999999993 | 14.869972229003906 | true |
| askapediaweb | 182 | 1873 | 13 | 1.0 | 1.0 | 1.0 | 3937.5 | 3956.6557 | 16.28997802734375 | true |

## Validation Surface

question_types_validated: meaning, location, parameters, returns, callers, callees, workflow, why, multi_hop, entity_impact_question, flow_trace_question, change_location_question, impact_analysis_question, architecture_boundary_question, missing_evidence_question, query_plan_orchestration_question, stored_procedure_impact_question

stored_procedure_validation: askapediaweb contained SQL stored procedure evidence and produced 13 stored_procedure_impact_question rows. All 13 had exact evidence hits.

## Fixes Required Before Pass

1. Duplicate symbol disambiguation:
   failure: duplicate method names in real projects caused exact evidence misses when queries only named the method.
   fix: validation prompts and evidence retrieval now include source path and source line for ambiguous symbols.

2. Direct evidence ranking:
   failure: source path aliases matched a whole file and could return the wrong method in the same file.
   fix: direct retrieval now ranks evidence id, source path plus exact line, source path plus line, symbol plus line, symbol only, and source path only.

3. Stored procedure intent:
   failure: generic "execute" wording misclassified ordinary methods named Execute as stored procedure impact questions.
   fix: stored procedure intent now requires explicit SQL or procedure evidence terms instead of the generic execute term.

## Verdict Boundary

This report proves the real-project validation gate for query planning, exact evidence retrieval, entity-impact trace, and stored-procedure validation where stored-procedure evidence exists.

This report does not claim that all project-local wiki weights are trained, that every unit is promoted, or that the full local wiki product is done. Those claims still require the relevant DPO runner manifests, held-out metrics, and project adapter publication artifacts.
