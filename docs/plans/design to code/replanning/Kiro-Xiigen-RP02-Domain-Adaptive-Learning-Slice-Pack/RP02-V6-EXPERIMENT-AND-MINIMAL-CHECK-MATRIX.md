# RP02 V6 — Frozen domain-law exam and minimal deterministic checks

**Design, NOT_RUN.** Open examples here are not TRAIN or SEALED. Split independent projects/source families before training; log rights, domain-law-version, Student/checkpoint, vector-coordinate-version, kNN candidate pool, solver/decoder, arbiter, local hardware and author/evaluator boundaries.

| Domain | GOOD and WHY | BAD and WHY | EDGE and handling |
| --- | --- | --- | --- |
| C# AST/nullability | s?.Length can be null; s?.Length ?? 0 applies fallback (different AST/law) | Treating ?. and ?? as lexical equivalents invents non-null value | Unknown C# version/dynamic/nullable context => ask or abstain |
| EN→RU polarity and grammar | Do not send -> Не отправляйте, preserving prohibition; adjective agreement based on same referent | Smooth translation that drops НЕ or blanket masculine ban from one feminine noun | книга на столе; он... can mean стол (different referent); ambiguous POS remains soft |
| SQL NULL/joins | IS NULL vs = NULL; LEFT JOIN ON/WHERE changes unmatched-row outcome | Lexical match overrides three-valued logic | dialect/schema/version unknown => dialect-specific oracle or abstain |

## Preregister comparisons and disproof
- E2 first real learning: parent P, repaired implementation no-update C0, identical implementation and actual persisted weight update T, measured T−C0 on unseen source families. Change neither Student, corpus, candidate IDs, rights, decoder nor hardware between C0 and T. Loss for legal retrieval rank-pair differs from RP01 response-DPO reference-policy objective.
- C01 multi-axis subject/intent/stack/version mapper; C02 law-aware vector vs generic with versioned reindex; C03 fixed-k ranker learning; C04 adaptive-k under equal PC; C05 exact vs ANN; C06 typed multi-evidence only for a genuine reducer. Each isolated, optional based on reachability, never a whole-pack gate.
- Separate 2×2 Student DPO parent/real LoRA update crossed kNN OFF/ON with same hard-law security in all cells; identify interaction separately, no cross-arm candidate leakage.
- First small lawful pilot one domain: PILOT_ONLY; independent C#, EN/RU and SQL families before broad claim, old-domain retention B0→B1→B2 and family-level intervals.

## Output record schema (copy, fill from actual observations only)
experiment_id; P_SHA; K_SHA; task_id; mode_and_rights; domain_law_version; source_family_train_ids; source_family_dev_ids; sealed_cert_hash; ordinary_consumer_method; vector_schema_hash; scorer_or_lora_checkpoint_sha; candidate_pool_sha; arbiter_version; pinned_hardware; preregistered_N; observed_N; changed_factor; P_output; C0_output; T_output; updated_parameter_pre_sha; updated_parameter_post_sha; apply_once_journal_id; law_correct_count; critical_law_errors; false_hard_veto; best_admissible_path_cost_regret; abstain_rate; latency_p50_p95; peak_process_RAM_VRAM; disk_index_rebuild_cost; failed_and_teacher_cost; old_domain_retention; uncertainty; existing_native_checks_reused; new_focused_risk_probes_and_justification; reviewer_receipt_or_NOT_PERFORMED; verdict_or_NOT_MEASURED.

READ_ONLY/no code => 0 tests. Mutated deterministic high-risk boundary => nearest focused invariant probes only, no coverage quota. Nondeterministic quality => heldout law/user-output/time/PC. Missing observed arms => NOT_SUPPORTED, not zero. Unknown domain authority => stop only that data/effect and name owner. Independently invoked Astra must receive raw cases pinned to exact final SHAs; a work order is not an Astra result.
