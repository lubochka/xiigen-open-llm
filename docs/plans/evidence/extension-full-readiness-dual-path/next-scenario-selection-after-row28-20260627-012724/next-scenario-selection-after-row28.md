# Next scenario selection after Row28

claim_scope: next safe planning/orchestration selection after Row28 only
evidence_scope: allowed product readiness board/action plan/dashboard plus recent Row23 and prior Row27 selection/context packets
count_claimed: total scenarios 28; scoped accepted 14; product-ready full pass 0; full joined pass 0; remaining 14
independent_evidence_count: 8
subagent_id_or_NONE: Mendel the 2nd / 019f060b-08ad-7c00-adf2-1dc4a73412db
downgrade_needed: no for this selection packet; yes for any execution/readiness claim

## Решение

`NEEDS_SCENARIO_REPAIR`

Следующий безопасный scenario/subphase: Row1 `frdp-p01-claude-product-readiness-product-workflow`, subphase `split-only scenario repair`.

Это не execution. Это маленькая плановая починка: разложить Claude product workflow на явные non-mutating sub-scenarios, required evidence fields, pass/fail/blocked statuses, dependency gates and no-claim locks. Этот шаг можно делать без свежего approval, потому что board уже говорит для Row1: command evidence insufficient for one complete product workflow, next concrete action can be to split the workflow into explicit sub-scenarios before route repair.

## Счетчики

Done/current/remaining:

- Total scenarios: 28.
- Scoped accepted: 14.
- Product-ready full pass: 0.
- Full joined pass: 0.
- Remaining/open: 14.
- Product readiness claims opened: 0.

## Почему Row1 безопасен

Row1 выбран не как product/runtime/UI work, а как planning-only repair. Он безопасен сейчас, потому что:

- independent from Row23 approved-input blocker;
- does not advance Row24;
- does not require product/runtime/UI/tests/eval/heldout/training;
- does not mutate checkpoint, locator, model, product files, or board counts;
- can be bounded to a new planning packet only;
- produces only a future execution-ready scenario script/decomposition, not readiness.

Row23 не выбран для execution: latest accepted Row23 status is `BLOCKED_NO_APPROVED_INPUT_PATH`, approved input paths found `0`, and the required missing fields are exactly one approved safe Row23 input/path with provenance, approval quote/source, category, evidence refs, forbidden actions, and claim locks.

Row24 не выбран: current instruction says Row24 is likely dependent and must not be used unless independence is explicit. I found no current explicit independence authority in the allowed artifacts.

Row27 and Row28 are scoped accepted only and are not a basis for product readiness.

## Что доказывает выбранный шаг

It can prove only:

- a clear Row1 Claude product-workflow decomposition exists;
- each future Row1 sub-scenario has exact non-mutating evidence requirements;
- missing runtime/UI/route/product evidence is explicitly blocked instead of inferred;
- later sub-agents have a bounded plan that prevents product-readiness overclaim.

## Чего он не доказывает

It does not prove:

- product readiness;
- Claude readiness;
- full extension readiness;
- local host/UI readiness;
- token savings;
- semantic quality;
- training/learnability;
- DPO quality;
- lifecycle readiness;
- checkpoint promotion;
- locator update;
- any runtime/test/eval/heldout result;
- any formal joined pass.

## Exact next sub-agent work order

Role: bounded Row1 split-only scenario-repair sub-agent.

Max active time: 10 minutes.

Current authority quote: "Ты bounded next-scenario selector after Row28 в C:\Projects\llm_mvp_core. Planning/orchestration only, max 10 minutes."

Task: Create a planning-only Row1 decomposition packet for `frdp-p01-claude-product-readiness-product-workflow`. Split the product workflow into explicit non-mutating sub-scenarios and gates covering adapter/core, token, semantic quality, lifecycle, checkpoint, locator, and Claude host route evidence. Do not execute any scenario.

Allowed reads:

- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.md/json`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-action-plan.md/json`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-14-user-visible-status-dashboard.md/json`
- Row1 evidence refs named by the board only if needed for exact blocker wording.

Allowed write scope:

- A fresh Row1 scenario-prep/repair subfolder under `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/`
- One Markdown packet and one JSON packet only.

Required content:

- human-readable Russian scenario prep;
- Row1 purpose and why current evidence is insufficient;
- explicit sub-scenario list for adapter/core, token, semantic quality, lifecycle, checkpoint, locator, and Claude host route;
- exact evidence filenames that a later separately authorized executor would produce;
- exact pass/fail/blocked statuses;
- claim locks: no product readiness, no host readiness, no token savings, no semantic quality, no training, no checkpoint promotion, no locator update, no formal joined pass;
- Row23 blocker and Row24 non-selection note;
- Row27/Row28 scoped-only note;
- `parent_will_perform_commit_push_after_review=true`.

Forbidden:

- product/runtime/UI/tests/eval/heldout/training;
- code edits;
- board/action-plan/dashboard edits;
- AGENTS/CLAUDE/skills edits;
- checkpoint/locator/model mutation;
- approval inference from quoted transcripts or old state;
- Row23 heldout/eval execution;
- Row24 advance;
- stage/commit/push.

Expected decision status from that sub-agent: `NEEDS_SCENARIO_REPAIR`, unless it proves Row1 cannot even be decomposed from current planning artifacts.

Commit/push boundary owner: none in this work order. No commit/push authorized.

## Human decision check

`TRUE_HUMAN_DECISION_REQUIRED` is not the global selection status, because Row1 split-only scenario repair can proceed without fresh Luba approval.

True human decision is required for Row23 semantic-quality execution. Exact missing fields:

- concrete approved safe Row23 input/path: missing;
- provenance: missing for any concrete Row23 input/path;
- approval quote/source: missing for any concrete Row23 input/path;
- category: missing for any concrete Row23 input/path;
- evidence refs: current refs support blocker/status/design/runner-contract only, not approval of a concrete path;
- forbidden actions: known globally but not attached to an approved concrete candidate;
- claim locks: preserved globally but not attached to an approved concrete candidate.

Other remaining categories do not prove product readiness and need separate future authority for execution:

- Claude route/UI categories: require safe host route or exact blocker before joined proof.
- Cursor/Codex UI route categories: require UI/input-surface work, forbidden in this selection task.
- Token/quality/training/lifecycle/checkpoint/locator execution categories: require separate runtime/test/eval/training/mutation-safe authority before execution; planning-only prep remains possible.

No product/runtime/UI/tests/eval/heldout/training was run by this packet. No commit/push was performed.
