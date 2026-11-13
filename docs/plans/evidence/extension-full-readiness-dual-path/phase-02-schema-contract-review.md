# Phase 02 Schema Contract Review

CURRENT TASK: Review Phase 02 dual-path evidence contract/schema/validator as a bounded review sub-agent. Accept only if the schema and validator enforce the Phase 01 bridge requirements and do not permit overclaiming.

active_task_sentence_source: Luba latest unquoted instruction: "Review Phase 02 dual-path evidence contract/schema/validator as a bounded review sub-agent. Accept only if the schema and validator enforce the Phase 01 bridge requirements and do not permit overclaiming."

NOT THE TASK: host UI/product runtime/training/checkpoint/locator execution, git add/commit/push/checkout/reset/clean, readiness declaration, or substantive implementation edits.

EVIDENCE-ONLY: Phase 01 bridge text, Phase 02 contract/schema/samples/report, validator source, validator output, targeted search output, and the read-only overclaim probe in this review. They support this review only and do not prove readiness.

## Russian-First Status

Фаза 3 из 17: contract/schema для evidence. Смысл фазы: сделать форму доказательств такой, чтобы будущие фазы не могли подменить dual-path evidence произвольными логами.

Шаг 02.02: review schema/validator. Смысл шага: проверить, что контракт реально запрещает подмену evidence и не открывает readiness claims.

Статус: NEEDS_FIX. Контракт и validator покрывают identity fields, разделение Automated boundary / Local desktop, join refs, mismatches, duplicate scenario/marker, typed payload families, честные `NOT_RUN` samples и базовые negative tests. Но validator всё ещё разрешает overclaim: если вручную поставить оба path status в PASS и `join_status=PASS_JOINED`, он принимает joined packet даже когда сами evidence items остаются `NOT_RUN` / `NOT_CAPTURED`, а artifact refs остаются `NONE`. Это нарушает Phase 01 bridge requirement that `NOT_RUN` / `NEEDS_VERIFICATION` must not become readiness pass without evidence.

Сделано: reread the required Phase 02 contract, schema, samples, validation report, validator, and the Phase 01 bridge section; reran the official validator; ran targeted required-term search; ran the requested whitespace check; ran one narrow read-only overclaim probe.

В процессе: no runtime/product/host UI/training/checkpoint/locator work is running.

Осталось: fix the schema/validator so PASS path statuses and `PASS_JOINED` require accepted/collected evidence, non-`NONE` artifact refs where evidence is required, and no `NOT_RUN` / `NOT_CAPTURED` / `NEEDS_VERIFICATION` evidence items can support PASS/readiness. Add a negative test for that exact overclaim probe.

Review нашёл: one blocking overclaim gap. `validate_joined_packet` checks that `PASS_JOINED` has `PASS_AUTOMATED_BOUNDARY`, `PASS_LOCAL_DESKTOP`, and `joined_claim_proven=true`, but it does not require path evidence items to be collected/accepted or backed by artifacts. The schema also enumerates PASS statuses without conditionals tying PASS to non-empty collected evidence.

Что исправлено: no implementation artifacts were changed. This review added only this report under the allowed write scope.

Следующее действие: return Phase 02 to a bounded fix sub-agent. Parent should not launch Phase 03 automated boundary until this review finding is fixed and revalidated.

## Review Decision

Decision: NEEDS_FIX.

Changed file(s): `docs/plans/evidence/extension-full-readiness-dual-path/phase-02-schema-contract-review.md`.

Parent can launch Phase 03 automated boundary: no. The Phase 02 evidence contract is not accepted until PASS/readiness cannot be produced from `NOT_RUN` / `NOT_CAPTURED` / `NONE` evidence.

No git write/ref actions were run.

## Finding

### BLOCKING-01 - Joined PASS can be accepted while evidence remains NOT_RUN / NOT_CAPTURED

Evidence:

- Phase 01 bridge requires non-claim states to stay non-claim until real evidence exists and says tests must verify no schema default can convert `NOT_RUN` or `NEEDS_VERIFICATION` into readiness without evidence: `phase-01-scenario-claim-inventory.md` lines 662-684.
- Schema exposes evidence item status values including `NOT_RUN` and `NEEDS_VERIFICATION`, path PASS statuses, and `PASS_JOINED`, but has no conditional that ties PASS to collected/accepted evidence or non-`NONE` artifact refs: `phase-02-dual-path-evidence-schema.json` lines 103-153, 170-172, 212-214, and 1081-1083.
- Validator path validation requires fields exist but does not reject PASS with `NOT_RUN` / `NOT_CAPTURED` evidence: `validate_phase02_evidence_contract.py` line 263.
- Validator joined PASS logic checks only path status names and `joined_claim_proven`: `validate_phase02_evidence_contract.py` lines 339-341.
- Existing negative tests include "reject PASS_JOINED while both paths are NOT_RUN", but do not cover "both path statuses are PASS while evidence items remain NOT_RUN / NOT_CAPTURED / NONE": `validate_phase02_evidence_contract.py` lines 430-482.

Read-only overclaim probe:

```text
ACCEPTED_OVERCLAIM_PROBE
automated evidence status: NOT_RUN
local evidence status: NOT_CAPTURED
joined evidence status: NOT_RUN
automated artifact: NONE
local artifact: NONE
```

Impact: A future packet can manufacture joined readiness by flipping statuses to PASS without replacing placeholder evidence. That directly violates the "do not permit overclaiming" acceptance condition.

Expected fix: reject any Automated boundary PASS unless its path evidence is collected/accepted and not `NONE` where artifact evidence is required; reject any Local desktop PASS unless activation/evidence are real collected/accepted evidence or exact blocker downgrade is used; reject `PASS_JOINED` unless both paths satisfy those PASS evidence gates and joined/top-level evidence is not `NOT_RUN`, `NOT_CAPTURED`, or `NEEDS_VERIFICATION`. Add a negative test that mutates the sample exactly like this probe and expects failure.

## Criteria Matrix

| Criterion | Result | Evidence |
|---|---:|---|
| Requires identity fields `scenario_id`, `host`, `claim_id`, `marker` | PASS | Schema and validator require these fields; validator negative test rejects missing `scenario_id`. |
| Separates Automated boundary and Local desktop packet fields | PASS | Separate packet kinds and separate `automated_boundary` / `local_desktop` blocks exist. |
| Joined evidence requires both path refs or blocker/downgrade and one path cannot become readiness | PARTIAL | Refs and downgrade record are required, and one-path status mismatch is rejected; however PASS can still be manufactured from placeholder evidence once both path status strings are flipped. |
| Rejects mismatched host/claim_id/marker and duplicate scenario/marker | PASS | Official validator output includes expected failures for host, claim_id, marker, and duplicate scenario+marker. |
| Covers typed payloads for token, semantic quality, learnability/DPO, asset lifecycle, checkpoint, locator, Codex chat UI | PASS | Schema and validator contain typed payload mappings for all required families. |
| Blocks `NOT_RUN` / `NEEDS_VERIFICATION` becoming PASS/readiness | FAIL | Overclaim probe was accepted with `NOT_RUN` / `NOT_CAPTURED` / `NONE` evidence. |
| Samples are honest `NOT_RUN` samples | PASS | All three samples are marked `NOT_RUN` and non-claim. |
| Negative tests catch missing fields, one-path proof, mismatches, typed payload absence, doc/dashboard substitution | PARTIAL | Existing negatives cover listed basics, but miss the PASS-with-placeholder-evidence case. |
| Contract is human-readable and maps back to Phase 01 bridge | PASS | Contract names bridge-derived identity, path, join, typed payload, downgrade, and non-claim rules. |

## Verification Output

Official validator command:

```powershell
python tools\extension-full-readiness-dual-path\validate_phase02_evidence_contract.py
```

Observed output:

```text
VALIDATION PASSED: Phase 02 dual-path evidence contract
positive samples: 3
negative tests: 13
PASS expected failure: reject missing scenario_id
PASS expected failure: reject duplicate scenario_id + marker
PASS expected failure: reject missing path status
PASS expected failure: reject missing path blocker
PASS expected failure: reject missing path downgraded_claim
PASS expected failure: reject joined evidence without both path refs
PASS expected failure: reject mismatched host
PASS expected failure: reject mismatched claim_id
PASS expected failure: reject mismatched marker
PASS expected failure: reject missing token typed payload
PASS expected failure: reject PASS_JOINED while both paths are NOT_RUN
PASS expected failure: reject documentation/dashboard/ledger substitution
PASS expected non-pass: NOT_RUN/NEEDS_VERIFICATION did not become PASS
non_claim: samples are NOT_RUN contract samples only, not readiness evidence
```

Targeted search command result summary:

```text
FOUND scenario_id 76
FOUND host 172
FOUND claim_id 87
FOUND marker 117
FOUND automated_boundary_packet 17
FOUND local_desktop_packet 18
FOUND joined_evidence_packet 8
FOUND Automated boundary 121
FOUND Local desktop 148
FOUND join_keys 11
FOUND automated_boundary_packet_ref 8
FOUND local_desktop_packet_ref 9
FOUND downgrade_record 11
FOUND one_path_cannot_prove_joined_claim 5
FOUND documentation_dashboard_ledger_substitute_allowed 5
FOUND NOT_RUN 105
FOUND NEEDS_VERIFICATION 129
FOUND PASS_JOINED 8
FOUND PASS_AUTOMATED_BOUNDARY 3
FOUND PASS_LOCAL_DESKTOP 3
FOUND token 124
FOUND semantic_quality 6
FOUND learnability_dpo 8
FOUND asset_lifecycle 6
FOUND checkpoint 77
FOUND locator 79
FOUND codex_chat_ui 6
FOUND documentation 13
FOUND dashboard 13
FOUND ledger 16
FOUND duplicate scenario_id + marker 2
FOUND mismatched host 2
FOUND mismatched claim_id 2
FOUND mismatched marker 2
```

Requested whitespace check:

```powershell
git diff --check -- docs/plans/evidence/extension-full-readiness-dual-path tools/extension-full-readiness-dual-path
```

Observed output before writing this review report: no output, exit code 0.

Observed output after writing this review report: no output, exit code 0.

## Non-Claims

I did not run host UI/product runtime/training/checkpoint/locator.
I did not run git add/commit/push/checkout/reset/clean.
I did not declare readiness.
I did not edit implementation artifacts.
This is a review report, not a completed fix cycle.
