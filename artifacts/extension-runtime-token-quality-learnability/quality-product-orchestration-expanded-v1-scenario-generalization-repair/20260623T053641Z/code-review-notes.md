# Code Review Notes - QUALITY-PRODUCT-ORCHESTRATION-EXPANDED-V1-SCENARIO-GENERALIZATION-REPAIR-03

## Cycle 1
Checked: Authority, scope, write boundaries, and prior leaf failure mode.
Findings: Old held-out gate measured exact unseen PatternId/ScenarioRef, which is invalid for source-disjoint train-only material.
Fixes: Added transferable_decision_structure_baseline evaluation mode with explicit no-overclaim notes.
Remaining risk: This is still a transparent baseline, not semantic quality.

## Cycle 2
Checked: Plan-output safety versus weak structural matches.
Findings: One-token generalized overlap could make a plan output look ready.
Fixes: Kept one-token overlap measurable in held-out structure eval, but required at least two decision-token anchors before plan output can be ready.
Remaining risk: Structure eval can show transfer evidence while final scenario planning remains conservative.

## Cycle 3
Checked: Hidden deterministic shortcut and leakage controls.
Findings: Project/scope/order/prerequisite tokens could let an older P9B ablation control pass.
Fixes: Excluded those broad ablation tokens from transfer authority and reran P9B ablation regression successfully. Added source-id/exact-scenario-ref-only ablation with 0/4 pass.
Remaining risk: Future token lists need the same ablation review before widening.

## Cycle 4
Checked: Lifecycle, dashboard, evidence packaging, and no-overclaim state.
Findings: This phase did not modify checkpoint lifecycle APIs or dashboard/API behavior.
Fixes: Packaged lifecycle-not-touched evidence, command logs, manifest hashes, Russian report, JSON report, state entry, and next work order.
Remaining risk: Host/dashboard readiness remains unclaimed and should require a separate bounded phase.

## Cycle 5
Checked: Commit-blocking review findings for one-anchor readiness and label-boilerplate leakage.
Findings: Runtime guard coverage did not directly prove that one transferable decision-token can count as eval evidence while Decide remains not ready. Transferable eval rows exposed label-like boilerplate tokens such as decision-token:chosen and decision-token:plan.
Fixes: Added a direct one-anchor guard test that passes the eval counter but asserts Decide returns IsReady=false with the two decision-token anchor block reason. Filtered chosen/plan/token boilerplate, sanitized chosen/rejected quality-reason prefixes, and added a boilerplate-only ablation row with 0 passing controls.
Remaining risk: This still proves only transparent deterministic structure overlap; SemanticQualityClaimed and HostRuntimeClaimed remain false.
