# Phase 02 Schema Contract Rereview

CURRENT TASK:
Re-review Phase 02 schema/validator after the overclaim fix. Accept only if the exact placeholder-PASS hole is closed and Phase 02 can proceed to Phase 03 review/launch without readiness overclaim.

active_task_sentence_source: Luba latest unquoted instruction: "Re-review Phase 02 schema/validator after the overclaim fix. Accept only if the exact placeholder-PASS hole is closed and Phase 02 can proceed to Phase 03 review/launch without readiness overclaim."

NOT THE TASK:
Do not run host UI/product runtime/training/checkpoint/locator. Do not git add/commit/push/checkout/reset/clean. Do not declare readiness. Do not edit implementation artifacts unless typo/whitespace only.

EVIDENCE-ONLY:
Phase 02 validator, contract, schema, validation report, prior review, sample JSON files, validator output, targeted search output, and diff-check output. They support this rereview only and do not prove product readiness.

## Russian-First Status

Фаза 3 из 17: contract/schema для evidence. Смысл фазы: сделать форму доказательств такой, чтобы будущие фазы не могли подменить dual-path evidence произвольными логами.

Шаг 02.03: повторный review overclaim fix. Смысл шага: убедиться, что PASS невозможен без реального collected/accepted evidence.

Статус: PASS. Exact placeholder-PASS hole is closed in the official validator: the current negative test suite includes the prior overclaim shape, mutates a joined packet to `PASS_JOINED` with `joined_claim_proven=true`, leaves Automated evidence `NOT_RUN`, Local evidence `NOT_CAPTURED`, joined evidence `NOT_RUN`, and artifact refs `NONE`, and rejects it. Phase 02 can proceed to Phase 03 automated boundary evidence gathering only as a bounded non-readiness work order. This is not a readiness declaration.

Сделано:
- Read the required validator, contract, schema, validation report, prior review, and all three Phase 02 sample JSON packets.
- Ran the official validator.
- Ran targeted search for `placeholder`, `overclaim`, `collected`, `accepted`, `NOT_CAPTURED`, `NONE_COLLECTED_IN_PHASE_01_01`, and `joined_claim_proven`.
- Ran the requested `git diff --check` scope.
- Wrote this rereview report under the allowed write scope.

В процессе:
- No runtime/product/host UI/training/checkpoint/locator work is running.
- No git write/ref action is running.

Осталось:
- Launch Phase 03 only as bounded automated boundary evidence gathering, with explicit non-readiness language.
- Future phases still must collect real Automated boundary evidence and Local desktop evidence or exact blockers before any joined claim can pass.

Review нашёл:
- No blocking finding remains for the exact placeholder-PASS hole.
- Residual boundary: the JSON schema artifact explains the PASS evidence rule in descriptions; the executable enforcement is in `tools/extension-full-readiness-dual-path/validate_phase02_evidence_contract.py`.

Что исправлено:
- The prior review hole is now covered by a validator PASS evidence gate and a dedicated negative overclaim probe.
- `PASS_AUTOMATED_BOUNDARY`, `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, and `joined_claim_proven=true` now require real `COLLECTED` or `ACCEPTED` evidence, non-placeholder artifact refs, non-placeholder collection provenance, and no blocker/downgrade contradiction text.

Следующее действие:
- Parent can launch Phase 03 automated boundary as a bounded non-readiness work order.
- Parent must not call this readiness, must not launch host UI/product runtime/training/checkpoint/locator, and must not use Phase 02 samples as readiness evidence.

## Changed Files

- `docs/plans/evidence/extension-full-readiness-dual-path/phase-02-schema-contract-rereview.md`

No implementation artifacts were edited in this rereview.

## Review Findings

Decision: PASS.

Blocking findings: none.

Exact findings:
- Official validator passed with `negative tests: 14`.
- The updated negative suite includes `reject placeholder PASS overclaim probe with NOT_RUN/NOT_CAPTURED/NONE evidence`.
- Validator logic contains `validate_pass_evidence_list` and `validate_pass_path_block`, and `validate_joined_packet` calls PASS evidence validation before accepting joined PASS.
- The placeholder probe explicitly sets `join_status=PASS_JOINED`, `joined_claim_proven=true`, both path statuses to PASS, Automated evidence to `NOT_RUN`, Local evidence to `NOT_CAPTURED`, joined evidence to `NOT_RUN`, and artifact refs to `NONE`; the validator rejects it.
- Positive samples remain honest shape-only samples: `NOT_RUN` / `NOT_CAPTURED`, `joined_claim_proven=false`, `NONE_COLLECTED_IN_PHASE_02_SAMPLE`, and explicit non-claim text.
- Contract, schema descriptions, and validation report explain that PASS requires real collected/accepted evidence and cannot be made from placeholder refs such as `NONE` or `NONE_COLLECTED_IN_PHASE_01_01`.

## Criteria Matrix

| Criterion | Result | Evidence |
|---|---:|---|
| Official validator includes placeholder-PASS overclaim case | PASS | Validator output shows `negative tests: 14` and the placeholder PASS expected failure. |
| Joined PASS / `joined_claim_proven=true` rejects placeholder evidence/artifact refs | PASS | Validator lines for `validate_pass_evidence_list`, `validate_pass_path_block`, and joined PASS logic enforce collected/accepted evidence and non-placeholder refs. |
| Positive samples remain honest NOT_RUN shape-only samples | PASS | Samples keep `NOT_RUN` / `NOT_CAPTURED`, `joined_claim_proven=false`, and non-claim text. |
| Contract/schema/report explain PASS requires real collected/accepted evidence | PASS | Contract PASS Evidence Gate, schema descriptions, and validation report all state that PASS requires real collected/accepted evidence and non-placeholder refs. |
| Phase 03 should not be launched as readiness | PASS | Phase 03 may start only as bounded automated boundary evidence gathering, not readiness/release/runtime launch. |

## Verification Output

Official validator command:

```powershell
python tools\extension-full-readiness-dual-path\validate_phase02_evidence_contract.py
```

Observed output:

```text
VALIDATION PASSED: Phase 02 dual-path evidence contract
schema: docs\plans\evidence\extension-full-readiness-dual-path\phase-02-dual-path-evidence-schema.json
sample automated boundary packet: docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-automated-boundary-packet.json
sample local desktop packet: docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-local-desktop-packet.json
sample joined evidence packet: docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-joined-evidence.json
positive samples: 3
negative tests: 14
PASS expected failure: reject missing scenario_id: $.scenario_id is missing
PASS expected failure: reject duplicate scenario_id + marker: Duplicate scenario_id + marker pair: frdp-p01-codex-token-savings-baseline / FRDP-P01-frdp-p01-codex-token-savings-baseline
PASS expected failure: reject missing path status: $.automated_boundary.status is missing
PASS expected failure: reject missing path blocker: $.local_desktop.blocker_or_NONE is missing
PASS expected failure: reject missing path downgraded_claim: $.automated_boundary.downgraded_claim is missing
PASS expected failure: reject joined evidence without both path refs: $.local_desktop_packet_ref must reference a packet
PASS expected failure: reject mismatched host: Automated/local host mismatch
PASS expected failure: reject mismatched claim_id: Automated/local claim_id mismatch
PASS expected failure: reject mismatched marker: Automated/local marker mismatch
PASS expected failure: reject missing token typed payload: $.typed_payloads.token is required for token-savings
PASS expected failure: reject PASS_JOINED while both paths are NOT_RUN: PASS_JOINED requires PASS_AUTOMATED_BOUNDARY
PASS expected failure: reject placeholder PASS overclaim probe with NOT_RUN/NOT_CAPTURED/NONE evidence: $.automated_boundary.evidence[0].status must be COLLECTED or ACCEPTED for PASS
PASS expected failure: reject documentation/dashboard/ledger substitution: $.evidence[0] attempts to substitute ledger for a required path
PASS expected non-pass: NOT_RUN/NEEDS_VERIFICATION did not become PASS
non_claim: samples are NOT_RUN contract samples only, not readiness evidence
```

Targeted search command:

```powershell
rg -n "placeholder|overclaim|collected|accepted|NOT_CAPTURED|NONE_COLLECTED_IN_PHASE_01_01|joined_claim_proven" tools\extension-full-readiness-dual-path docs\plans\evidence\extension-full-readiness-dual-path
```

Observed relevant hits:

```text
tools\extension-full-readiness-dual-path\validate_phase02_evidence_contract.py:290:def validate_pass_evidence_list(value: Any, path: str) -> None:
tools\extension-full-readiness-dual-path\validate_phase02_evidence_contract.py:320:def validate_pass_path_block(obj: dict[str, Any], path: str, pass_status: str) -> None:
tools\extension-full-readiness-dual-path\validate_phase02_evidence_contract.py:429:        require(packet["joined_claim_proven"] is True, "PASS_JOINED requires joined_claim_proven true")
tools\extension-full-readiness-dual-path\validate_phase02_evidence_contract.py:571:    placeholder_pass_probe = copy.deepcopy(joined)
tools\extension-full-readiness-dual-path\validate_phase02_evidence_contract.py:576:    placeholder_pass_probe["automated_boundary"]["evidence"][0]["status"] = "NOT_RUN"
tools\extension-full-readiness-dual-path\validate_phase02_evidence_contract.py:578:    placeholder_pass_probe["local_desktop"]["evidence"][0]["status"] = "NOT_CAPTURED"
tools\extension-full-readiness-dual-path\validate_phase02_evidence_contract.py:584:            "reject placeholder PASS overclaim probe with NOT_RUN/NOT_CAPTURED/NONE evidence",
docs\plans\evidence\extension-full-readiness-dual-path\phase-02-dual-path-evidence-contract.md:188:`PASS_AUTOMATED_BOUNDARY`, `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, and `joined_claim_proven=true` are allowed only when the relevant evidence items are real collected evidence.
docs\plans\evidence\extension-full-readiness-dual-path\phase-02-dual-path-evidence-contract.md:190:- evidence item `status` is `COLLECTED` or `ACCEPTED`;
docs\plans\evidence\extension-full-readiness-dual-path\phase-02-dual-path-evidence-contract.md:191:- `artifact_ref_or_NONE` points to a real artifact and is not `NONE`, `NONE_COLLECTED_IN_PHASE_01_01`, `NONE_COLLECTED_IN_PHASE_02_SAMPLE`, or any `NONE_COLLECTED...` placeholder;
docs\plans\evidence\extension-full-readiness-dual-path\phase-02-dual-path-evidence-schema.json:119:          "description": "For any path PASS, PASS_JOINED, or joined_claim_proven=true, evidence status must be COLLECTED or ACCEPTED..."
docs\plans\evidence\extension-full-readiness-dual-path\phase-02-dual-path-evidence-schema.json:1083:              "description": "PASS_JOINED requires both path statuses to be PASS and all path plus joined evidence to be real COLLECTED or ACCEPTED evidence..."
```

Sample honesty search:

```powershell
rg -n "NOT_RUN|NOT_CAPTURED|joined_claim_proven" docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-automated-boundary-packet.json docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-local-desktop-packet.json docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-joined-evidence.json
```

Observed relevant hits:

```text
docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-joined-evidence.json:92:  "join_status": "NOT_RUN",
docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-joined-evidence.json:93:  "joined_claim_proven": false,
docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-local-desktop-packet.json:15:        "status": "NOT_CAPTURED",
docs\plans\evidence\extension-full-readiness-dual-path\phase-02-sample-automated-boundary-packet.json:10:    "status": "NOT_RUN",
```

Diff check command:

```powershell
git diff --check -- docs/plans/evidence/extension-full-readiness-dual-path tools/extension-full-readiness-dual-path
```

Observed output before writing this rereview report: no output, exit code 0.

## Parent Phase 03 Boundary

Parent can launch Phase 03 automated boundary only as a bounded non-readiness work order. The work order must gather automated boundary evidence or blockers and must not claim product readiness, full extension readiness, Codex chat UI readiness, Local desktop proof, training, checkpoint, locator, or release readiness.

## Non-Claims

I did not run host UI/product runtime/training/checkpoint/locator.
I did not run git add/commit/push/checkout/reset/clean.
I did not declare readiness.
I did not edit implementation artifacts.
This is a rereview report, not a readiness artifact.
