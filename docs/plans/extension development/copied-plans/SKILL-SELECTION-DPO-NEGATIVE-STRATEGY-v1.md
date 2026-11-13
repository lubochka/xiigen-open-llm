# SkillSelection DPO Negative Strategy v1

Status: CM-25B strategy artifact. This is a local static strategy document only. It does not train a model and does not claim full SkillSelection semantic quality.

## Purpose

SkillSelectionMachine DPO teaches the universal semantic task:

`structured task/context + available skill inventory -> relevant skill ids, rejected skill ids, reasons, and when-not-to-use warnings`

The trainable target is skill meaning, skill combination, wrong-skill rejection, and user preference semantics. It is not exact repository execution, shell commands, file paths, patch recipes, or host/runtime operation.

## Source Refs

- `docs/plans/ORCHESTRATION-AND-PROMPT-TO-TASK-FULL-EXTENSIONS-PLAN-v3.md`
- `docs/plans/DPO-KNOWLEDGE-MAP-v1.md`
- `artifacts/dpo-universalization/SS-00U.json`
- `training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl#sha256:31bbabca5e4fc20f1f892ca35b25c2a957ff7f772e472f8d02ced39f9321ed0e`
- `<local-ref:skills-inventory-universal-vs-project-specific.md#sha256:905F88B40E89C860AAE158BE6CA6AD6B9A0CE0D53DBB03106A23510A644F58C9>`
- `<style-ref:PROMPT-TO-TASK-DPO-NEGATIVE-STRATEGY-v1.md#sha256:F90505D71200FF10AB74ECBEA6ED00DF7B1F905770E8ED7626F1383107E098B7>`
- `<style-ref:ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md#sha256:A02E6C4FCD8950E428D96B1B7470847578D47D579995D575FD258B9BE8514A5C>`

No raw private prompt text is copied here. Refs are stable local hashes or existing project artifacts.

## Universalization Rule

Common SkillSelection DPO rows may teach:

- skill purpose and trigger semantics;
- when a skill is useful and when it is harmful;
- expected inputs and outputs;
- compatible skill combinations;
- failure modes and rejected skill families;
- user-facing policy boundaries such as external AI, test-only evidence, and human interrupt priority.

Common SkillSelection DPO rows must not teach:

- exact repo paths, commands, patch recipes, or product-specific execution habits;
- host-specific shortcuts;
- deterministic candidate-position or filename matching as final authority;
- raw private prompts or user-local preferences;
- technical execution behavior before `TECH-EXEC-LEARNING-GATE` is accepted.

Adaptive/user-side rows may learn Luba-specific style, depth, sequencing, and correction preferences, but must remain user-local and must not mutate the common checkpoint silently.

## Negative Taxonomy

| Code | Error class | Near-miss shape |
| --- | --- | --- |
| SS-DPO-01 | wrong_skill_family | Selects a skill from the wrong semantic family, for example implementation instead of planning/review. |
| SS-DPO-02 | project_specific_technical_leakage | Selects skills because of exact files, commands, repo names, or host mechanics instead of universal skill semantics. |
| SS-DPO-03 | over_select_all_skills | Selects many or all skills when the task needs a small compatible set. |
| SS-DPO-04 | missing_required_review_or_planning_skill | Omits plan-review, authority, or testability skills when the task is plan/review/governance oriented. |
| SS-DPO-05 | external_ai_tool_policy_miss | Selects web/browser/model-backed tools when the task forbids or does not require them. |
| SS-DPO-06 | test_only_as_implementation_confusion | Treats smoke, handler-level, static, or report-only evidence as implementation/runtime completion. |
| SS-DPO-07 | no_user_adaptive_signal | Ignores available user correction/preference signal when the task is adaptive/user-local. |
| SS-DPO-08 | human_interrupt_priority_miss | Continues execution skills instead of direct answer/authority-check skills after a stop/why/before-continuing instruction. |
| SS-DPO-09 | deterministic_shortcut | Uses candidate position, keyword, filename, regex, or exact lookup as final skill decision authority. |
| SS-DPO-10 | incompatible_skill_combination | Combines mutually confusing skills, such as final completion packaging with known open gaps. |
| SS-DPO-11 | missing_negative_rejection | Chooses the right skill but fails to reject tempting wrong skills with reasons. |
| SS-DPO-12 | adaptive_common_leg_mixing | Places user-local preference/adaptive corrections into common training rows. |

## Positive Examples

Each example uses redacted refs only.

| Row | Source ref | Chosen skill semantics | Rejected near-miss | Error codes |
| --- | --- | --- | --- | --- |
| SS-DPO-SEED-001 | `<skill-selection-corpus:common#sha256:31bbabca...>` + `<inventory-ref:905f88b4...>` | Plan/governance task selects planning, plan-review, authority/self-reflection, and test-strategy skills. | Selects implementation/build/test runner skills because the prompt mentions files or commits. | SS-DPO-01, SS-DPO-04 |
| SS-DPO-SEED-002 | `<skill-selection-corpus:common#sha256:31bbabca...>` + CM-24/B19 evidence refs | Human interrupt task selects answer-first, authority-boundary, and protocol-check skills. | Continues executor/commit/push skills before answering. | SS-DPO-08, SS-DPO-10 |
| SS-DPO-SEED-003 | `<skill-selection-corpus:common#sha256:31bbabca...>` + HOST-08 evidence refs | Handler-level QA task selects test-strategy, evidence-honesty, and host-QA skills with narrow claim scope. | Selects final-completion or full-runtime acceptance skills from handler/static evidence. | SS-DPO-06, SS-DPO-11 |
| SS-DPO-SEED-004 | `<skill-selection-corpus:common#sha256:31bbabca...>` + AD evidence refs | User correction task selects adaptive-signal and user-local DPO skills. | Mutates common checkpoint or ignores adaptive/user-local leg. | SS-DPO-07, SS-DPO-12 |

## Negative Example Requirements

Rejected outputs must be plausible near-misses, not generic bad text. A rejected row should:

- select a tempting but wrong skill family;
- miss a required review/planning/authority skill;
- over-select all skills;
- use an external tool policy incorrectly;
- confuse test-only evidence with implementation acceptance;
- mix common and adaptive/user-local legs.

A rejected row must not be an empty answer, a generic refusal, arbitrary nonsense, or a deterministic keyword mistake without a plausible semantic confusion.

## Row-Level Binding Template

Every approved SkillSelection DPO binding row must include:

```json
{
  "source_row_id": "ss-dpo-ref-<stable>",
  "target_machine": "SkillSelectionMachine",
  "approved_pair_ref": "<local-skill-selection-approved-pair:...>",
  "error_code": "SS-DPO-..",
  "chosen_ref": "<local-skill-selection-chosen:...>",
  "rejected_ref": "<local-skill-selection-rejected:...>",
  "source_ref": "<redacted local ref or corpus hash>",
  "universalization_scope": "universal_skill_semantics_only",
  "raw_private_text_copied": false,
  "invented_approval": false,
  "technical_execution_refs_excluded": true
}
```

Rows with `<operator-approved-...required>` placeholders are not approved. Rows without concrete `chosen_ref`, `rejected_ref`, `error_code`, or `approved_pair_ref` fail CM-25 quality.

## Quality Gates

- Positive and negative rows are close enough to teach a decision boundary.
- Negative row has explicit `SS-DPO-*` error code.
- Chosen and rejected refs are stable local refs or corpus hashes, not raw private text.
- Universal/common rows exclude code-bound technical execution.
- Adaptive rows remain user-local and do not mutate common checkpoint.
- No generic refusal, empty output, absurd bad output, or JSON/report-only substitute is accepted.
- No deterministic shortcut is accepted as learned skill-selection semantics.

## Open Follow-Up

This strategy closes the missing strategy artifact gap. It does not by itself produce a full SkillSelection DPO binding JSONL or train a model. The next CM-25 action should re-audit whether this strategy closes the strategy gap and decide whether row materialization is now the next bounded repair.
