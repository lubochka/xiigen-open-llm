// P16E artifact-level evaluator summary.
// Generated as documentation of the schema-only evaluation path.
// This is not production runtime code and is not wired into tests or build.
module.exports = {
  phase: 'P16E_SKILL_SELECTION_SOURCE_DISJOINT_EVAL_ABLATION',
  evaluator_scope: 'candidate_schema_eval_only',
  runtime_evaluation: false,
  controls: [
  "remove_prompt_signals",
  "remove_task_context_signals",
  "candidate_order_permutation",
  "remove_diagnostics_only_candidate_order_guard",
  "source_id_title_path_leakage_attempt",
  "exact_prompt_memorization_attempt"
]
};
