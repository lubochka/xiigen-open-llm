# ProjectWikiQueryPlanningFlow Query-Series Phase Report

report_scope: dpo_training_promotion_report
unit: ProjectWikiQueryPlanningFlow
scope: query series before wiki retrieval and answer analysis
claim: PROMOTED_UNIT_SCOPE only
not_claimed: whole_wiki_done, all_project_wikis_covered, all_units_promoted, question_database_done

## Accepted Batch
batch_id: project-wiki-query-planning-query-series-promotion-confirmation-028
heldout_accuracy: 0.9200000166893005
heldout_reward_margin: 0.007554488256573677
heldout_loss: 0.689382016658783
final_accuracy: 0.9200000047683716
final_reward_margin: 0.00773959802754689
cpu_time_ms: 115531.25
average_cpu_percent: 12.71370139505782
peak_ram_mb: 93.859375
elapsed_ms: 160753.3263
query_plan_accuracy: 1
multi_hop_sequence_accuracy: 1
manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-project-wiki-query-planning-query-series-promotion-confirmation-028-dpo\project-wiki-query-planning-flow\project-wiki-query-planning-20260521205436\generated_capability_manifest.json

## Run History
- project-wiki-query-planning-query-series-pre-retrieval-023: status=HELD; heldout_accuracy=0.7599999904632568; heldout_margin=0.0060949367471039295; cpu_ms=109343.75; ram_mb=88.8984375; elapsed_ms=149073.2518; reason=DPO global above 0.75 but answer_without_intermediate_analysis below type gate
- project-wiki-query-planning-answer-analysis-replay-024: status=REJECTED; heldout_accuracy=0; heldout_margin=-0.010172461159527302; cpu_ms=104375; ram_mb=94.16015625; elapsed_ms=143187.6953; reason=focused replay with bad rejected shape collapsed to 0 accuracy
- project-wiki-query-planning-answer-analysis-replay-025: status=REJECTED; heldout_accuracy=0.3799999952316284; heldout_margin=-0.0021638148464262486; cpu_ms=116250; ram_mb=88.2890625; elapsed_ms=164817.4477; reason=near-correct rejected improved the weak type but still below gate
- project-wiki-query-planning-query-series-pre-retrieval-026: status=HELD; heldout_accuracy=0.9399999976158142; heldout_margin=0.009773725643754005; cpu_ms=105953.125; ram_mb=87.125; elapsed_ms=147987.2094; reason=all rejected types passed DPO gate; promotion confirmation still required
- project-wiki-query-planning-query-series-promotion-confirmation-027: status=REJECTED; heldout_accuracy=0.9399999976158142; heldout_margin=0.009933847934007645; cpu_ms=104765.625; ram_mb=99.578125; elapsed_ms=151848.5789; reason=DPO passed but deterministic multiHopSequenceAccuracy was 0.8 due stale wording causing shallow plans
- project-wiki-query-planning-query-series-promotion-confirmation-028: status=PROMOTED_UNIT_SCOPE; heldout_accuracy=0.9200000166893005; heldout_margin=0.007554488256573677; cpu_ms=115531.25; ram_mb=93.859375; elapsed_ms=160753.3263; reason=DPO and deterministic query-planning gate passed; manifest emitted

## Per-Type Accepted Held-Out
- single_retrieval_query_no_plan: count=23; accuracy=0.956521739130435; margin=0.00476925933490629; min_margin=-0.000641822814941406
- answer_without_intermediate_analysis: count=15; accuracy=0.8; margin=0.00394968688488007; min_margin=-0.000733759254217148
- retrieval_before_project_version_checks: count=12; accuracy=1; margin=0.0173988427656392; min_margin=0.0129136089235544

## Accepted Learning
- Correct pattern: decompose developer question into ordered wiki evidence queries before retrieval and answer composition.
- Good rejected pattern: near-correct plan with same query steps, but evidence_analysis_required=false and answer-before-analysis allowed.
- Rejected probe: focused single-type answer-analysis replay collapsed; do not continue from batches 024 or 025.
- Rejected probe: stale wording produced shallow deterministic plans; use current-project/version wording.
