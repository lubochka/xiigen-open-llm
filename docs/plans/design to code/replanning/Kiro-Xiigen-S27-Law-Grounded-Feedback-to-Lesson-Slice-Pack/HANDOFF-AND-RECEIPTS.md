# S27 — Required executable per-task evidence packet

Each executor returns both a **short plain Russian human outcome** (what changed for a real user, what is not measured) and this exact machine-style receipt. Missing values = NOT_MEASURED/NOT_AUTHORIZED/UNBOUND, not optimistic PASS.

    task_id: S27.XX
    status: SOURCE_TRACED|CONSUMER_USED|EVIDENCE_ADMITTED|REUSE_NO_CHANGE|
            INTEGRATION_OPEN|NO_TRAIN|PAIR_JUDGED|STATE_CHANGED|
            SCIENCE_EXAMINED|COLD_ORDINARY_ADOPTED|INDEPENDENT_REVIEWED|
            NOT_AUTHORIZED|NOT_MEASURED|REGRESS|INCONCLUSIVE|NEEDS_FIX
    planning_sha: ACTUAL
    core_sha: ACTUAL
    current_unquoted_task: ACTUAL
    not_the_task: ACTUAL
    skills_loaded: [{name,sha,read_status}]
    sources: [{root,ref,blob,lines,READ_FULL|READ_SCOPED|INDEX_ONLY|NOT_READ}]
    workLeft_rag_primary_visited: [...]
    unvisited_sources: [...]
    original_requirements_and_owners: [...]
    protected_domain_laws_and_versions: [...]
    good_bad_edge_falsifier: ...
    real_producer_to_consumer: ...
    deterministic_vs_learned_state_and_recipient: ...
    exact_pair: {codex_case_choice, chinese_both_sides, codex_exact_judge,
                 admitted_unique_pair, actual_optimizer, at_most_once}
    permissions_and_budget: {code,data,teacher,judge,train,exam,promote}
    frozen_panel: {TRAIN,OP_DEV,SEALED_CERT,source_family,law_version}
    actual_arms: {P,C0,E,T1,T2}
    bulk_metrics: {law_accuracy,domain_mapping,route_regret,retention,
                   p95_latency,full_pc_time,peak_ram,peak_vram,
                   failed_attempt_cost,uncertainty}
    changed_paths_and_git_remote_shas: [...]
    tests_only_for_deterministic_properties: ...
    independent_astra_review: ACTUAL_RECEIPT_OR_NOT_PERFORMED
    claim_scope: ...
    evidence_scope: ...
    count_claimed: 0_or_real
    independent_evidence_count: 0_or_real
    subagent_id_or_NONE: NONE_or_real
    downgrade_needed: true_or_false
    next_task_and_owner: ...

When real missing algorithms belong RP01/02/03/04/05/09/10/12/13/18, hand them exact source + acceptance + falsifier instead of creating a duplicate S27 implementation. One-shard user permission never permits rewriting concurrent Rxx results. If a smaller child subtask exists, it must have its own current task, mandatory skills and full source/role/rights schema and is RETURN_ONLY until expressly authorized. Follow the root EXECUTION-BOARD rather than issuing a second global R26/27 research cycle.
