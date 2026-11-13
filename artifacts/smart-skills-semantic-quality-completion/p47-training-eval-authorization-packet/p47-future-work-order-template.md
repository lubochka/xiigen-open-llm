# P47 future work-order template

Status: disabled until fresh explicit Luba approval.

This template is not an active work order. It is a skeleton for a future parent-owned packet after Luba gives a current approval quote. While `exact_luba_approval_quote = "NONE"`, every execution flag remains false.

## Required approval block

```text
exact_luba_approval_quote: <paste fresh current Luba quote>
approval_must_explicitly_accept: P47 training/eval authorization packet
approval_must_explicitly_authorize_now: <training|eval|checkpoint mutation>
training_authorized_now: <true only if quote says training now>
eval_authorized_now: <true only if quote says eval now>
checkpoint_mutation_authorized_now: <true only if quote says checkpoint mutation now>
```

## Required artifact refs

```text
candidate_dataset:
  path: <exact path>
  sha256: <exact checksum>
  row_count: <exact count>
  source_license_gate_status: <passed or user-local/adaptive-only>

heldout_or_eval_fixtures:
  path: <exact path>
  sha256: <exact checksum>
  row_count: <exact count>
  source_disjoint_guard: <exact guard report and result>

target_checkpoint_ref:
  checkpoint_path: <exact path>
  manifest_path: <exact path>
  locator_ref: <exact locator>
  sha256: <exact checksum>

rollback_checkpoint_ref:
  checkpoint_path: <exact path>
  manifest_path: <exact path>
  locator_ref: <exact locator>
  sha256: <exact checksum>
```

## Required metrics and ablations

```text
metrics_required:
  - per_machine_metrics
  - heldout_accuracy_or_pass_rate_by_source_family
  - ablation_delta_exact_prompt_lookup
  - ablation_delta_source_id_path_filename_lookup
  - ablation_delta_title_static_route
  - ablation_delta_nearest_only_baseline
  - ablation_delta_no_trained_state
  - candidate_order_permutation_check_for_skillselection
  - false_positive_claim_scan
  - regression_check_against_previous_checkpoint
  - explicit_failure_examples

ablation_pass_condition:
  <future threshold chosen by parent and approved before execution>
```

## Required forbidden actions unless explicitly approved

- Do not run training unless the approval quote says training now.
- Do not run eval unless the approval quote says eval now.
- Do not mutate checkpoints unless the approval quote says checkpoint mutation now and rollback ref is present.
- Do not claim semantic quality or model readiness from static JSON, dashboard preview, fixture presence, or dry-run denial.
- Do not use external AI/network unless Luba explicitly approves that exact path.

## Stop condition

If any required field is still `<placeholder>` or the approval quote is `NONE`, the future executor must stop before execution and return `FAIL_NEEDS_PARENT_REPAIR`.
