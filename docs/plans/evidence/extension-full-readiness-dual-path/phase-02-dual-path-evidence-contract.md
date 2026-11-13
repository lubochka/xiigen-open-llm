# Phase 02 - Dual-Path Evidence Contract

CURRENT TASK: Execute Phase 02 of `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md`: implement the dual-path evidence contract/artifact schema and validation/tests derived from the Phase 01 inventory bridge.

active_task_sentence_source: Luba latest unquoted instruction: "Execute Phase 02 of `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md`: implement the dual-path evidence contract/artifact schema and validation/tests derived from the Phase 01 inventory bridge."

NOT THE TASK: Claude/Cursor/Codex host UI tests, product runtime evidence, token measurements, semantic eval, training, DPO construction, checkpoint mutation/promotion, locator update, browser checks, external AI, web tools, product readiness claims, full extension readiness claims, Codex chat UI readiness claims, git write/ref actions, or production code edits.

EVIDENCE-ONLY: Phase 01 inventory rows, the Phase 01 bridge, the Phase 01 rereview, the Phase 02 plan section, and existing repo validation patterns. They define the contract shape only and do not prove runtime readiness.

## Human-Readable Status

Фаза 3 из 17: contract/schema для evidence. Смысл фазы: сделать форму доказательств такой, чтобы будущие фазы не могли подменить dual-path evidence произвольными логами.

Шаг 02.02: fix validator overclaim. Смысл шага: связать PASS statuses с реальным collected/accepted evidence и запретить placeholder PASS.

Статус: DONE. Validator, schema text, and this contract now state that `PASS_AUTOMATED_BOUNDARY`, `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, and `joined_claim_proven=true` require real `COLLECTED` or `ACCEPTED` evidence with non-`NONE` artifact refs. Shape-only `NOT_RUN` / `NOT_CAPTURED` samples cannot prove readiness and cannot be upgraded to PASS by changing status strings.

Сделано: added a PASS evidence gate for path evidence, joined evidence, artifact refs, activation, provenance timestamps, and blocker/downgrade contradiction checks.

В процессе: no runtime/product/host UI work is running.

Осталось: send Phase 02 back to review; future phases still have to collect real Automated boundary and Local desktop evidence or exact blockers.

Review нашёл: joined PASS could previously be manufactured by setting path statuses and `joined_claim_proven=true` while the evidence still said `NOT_RUN`, `NOT_CAPTURED`, and artifact refs were `NONE`.

Что исправлено: placeholder PASS is now forbidden. PASS evidence must be `COLLECTED` or `ACCEPTED`, artifact refs must point to real collected artifacts, and blocker/downgrade text must not contradict PASS with `NEEDS_VERIFICATION`, `NOT_RUN`, `NOT_CAPTURED`, `NOT_DISCOVERED_YET`, or `NONE_COLLECTED...` values.

Следующее действие: Phase 02 can return to bounded review; do not start Phase 03/runtime work from this contract update.

Historical Step 02.01 baseline status is retained below for context.

Фаза 3 из 17: contract/schema для evidence. Смысл фазы: сделать форму доказательств такой, чтобы будущие фазы не могли подменить dual-path evidence произвольными логами.

Шаг 02.01: schema/validator/tests. Смысл шага: создать проверяемый контракт для Automated boundary packet, Local desktop packet и joined evidence.

Статус: DONE. Контракт, JSON schema, три sample-пакета и локальный validator/test script созданы как Phase 02 schema-only evidence. Они проверяют форму будущих доказательств и правила join/downgrade, но не доказывают readiness, runtime pass, token savings, semantic quality, learnability, DPO quality, checkpoint promotion, locator update или Codex chat UI readiness.

Сделано: определены три packet kind: `automated_boundary_packet`, `local_desktop_packet`, `joined_evidence_packet`; зафиксированы identity keys `scenario_id`, `host`, `claim_id`, `marker`; описаны обязательные path fields, result/downgrade fields, typed payload families, join keys, downgrade rules, non-claims, redaction/provenance fields и negative validation expectations.

В процессе: нет запущенного runtime/product/host UI work. Phase 02 artifacts are static schema/contract files plus a local validator.

Осталось: отдельный review sub-agent должен проверить Phase 02 artifacts. Phase 03+ still must collect real Automated boundary evidence, and Phase 04+ still must collect Local desktop evidence or exact blockers.

Review нашёл: existing repo did not expose a reusable JSON Schema validation dependency for this documentation contract, so Phase 02 uses a small local validator under the allowed `tools/extension-full-readiness-dual-path/` path.

Что исправлено: Phase 01 bridge requirements are now executable checks instead of prose-only requirements: missing identity fields, duplicate scenario/marker pairs, missing path fields, missing typed payloads, mismatched join keys, one-path proof, documentation/dashboard/ledger substitution, and `NOT_RUN` to PASS defaulting are rejected.

Следующее действие: launch a separate Phase 02 review sub-agent; do not start runtime, host UI, token, quality, training, DPO, checkpoint, locator, browser, external AI, or git write/ref work from this packet.

Commit/continuation allowed: git add/commit/push/checkout/reset/clean/branch/tag/merge/rebase are not allowed. Continuation is allowed only to Phase 02 review under a separate bounded work order; implementation/runtime continuation is not opened by this artifact.

## Artifact Set

| Artifact | Purpose |
|---|---|
| `phase-02-dual-path-evidence-contract.md` | Human-readable contract and non-claim boundary. |
| `phase-02-dual-path-evidence-schema.json` | JSON Schema-style contract for the three packet kinds. |
| `phase-02-sample-automated-boundary-packet.json` | Empty Automated boundary packet marked `NOT_RUN`. |
| `phase-02-sample-local-desktop-packet.json` | Empty Local desktop packet marked `NOT_RUN`. |
| `phase-02-sample-joined-evidence.json` | Empty joined evidence packet marked `NOT_RUN`. |
| `tools/extension-full-readiness-dual-path/validate_phase02_evidence_contract.py` | Minimal local validator and negative tests for this contract. |
| `phase-02-schema-validation-report.md` | Command output and verification results. |

## Packet Kinds

### Automated Boundary Packet

The Automated boundary packet records the highest testable boundary before host UI. It cannot prove a local desktop or host UI claim.

Required identity fields:

- `scenario_id`
- `host`
- `claim_id`
- `marker`

Required Automated boundary fields:

- `automated_boundary.status`
- `automated_boundary.evidence`
- `automated_boundary.remaining`
- `automated_boundary.blocker_or_NONE`
- `automated_boundary.downgraded_claim`
- `automated_boundary.provenance`

Required result/downgrade fields:

- `evidence`
- `remaining`
- `downgraded_claim`
- `non_claims`
- `blocker_or_NONE`
- `redaction_status`
- `provenance`

### Local Desktop Packet

The Local desktop packet records real host activation evidence, screenshot/video/log evidence, or exact blocker. It cannot prove the Automated boundary path.

Required identity fields:

- `scenario_id`
- `host`
- `claim_id`
- `marker`

Required Local desktop fields:

- `local_desktop.status`
- `local_desktop.activation`
- `local_desktop.evidence`
- `local_desktop.remaining`
- `local_desktop.blocker_or_NONE`
- `local_desktop.downgraded_claim`
- `local_desktop.provenance`

Required result/downgrade fields:

- `evidence`
- `remaining`
- `downgraded_claim`
- `non_claims`
- `blocker_or_NONE`
- `redaction_status`
- `provenance`

### Joined Evidence Packet

The joined evidence packet joins exactly one Automated boundary packet and exactly one Local desktop packet. It cannot turn one path into a full joined claim.

Required identity fields:

- `scenario_id`
- `host`
- `claim_id`
- `marker`

Required joined fields:

- `join_keys.scenario_id`
- `join_keys.host`
- `join_keys.claim_id`
- `join_keys.marker`
- `automated_boundary_packet_ref`
- `local_desktop_packet_ref`
- `automated_boundary`
- `local_desktop`
- `join_status`
- `downgrade_record`
- `joined_claim_proven`

Required result/downgrade fields:

- `evidence`
- `remaining`
- `downgraded_claim`
- `non_claims`
- `blocker_or_NONE`
- `redaction_status`
- `provenance`

## Typed Payload Families

| Claim family | Required typed payload object | Required fields |
|---|---|---|
| `token-savings` | `typed_payloads.token` | workload identity, tokenizer/version, baseline tokens, candidate tokens, repeat counts, savings calculation, quality guard, evidence, remaining, downgraded_claim |
| `semantic-quality` | `typed_payloads.semantic_quality` | held-out case id, rubric, baseline score, candidate score, leakage checks, answer evidence, remaining, downgraded_claim |
| `learnability-training` | `typed_payloads.learnability_dpo` | dataset provenance, allowed-use status, train/eval split, leakage gates, before/after behavior, checkpoint diff, rollback evidence, remaining, downgraded_claim |
| `dpo-quality` | `typed_payloads.learnability_dpo` | dataset provenance, allowed-use status, train/eval split, leakage gates, before/after behavior, checkpoint diff, rollback evidence, DPO pair evidence, remaining, downgraded_claim |
| `asset-lifecycle` | `typed_payloads.asset_lifecycle` | backup/export/import, share, continue-training, rollback, package manifest, readback evidence, remaining, downgraded_claim |
| `checkpoint-promotion` | `typed_payloads.checkpoint` | candidate hash, compatibility/load result, quality gate, promotion owner, rollback evidence, host reload evidence, remaining, downgraded_claim |
| `locator-update` | `typed_payloads.locator` | locator source of truth, locator diff, fresh-load result, rollback locator, per-host reload evidence, remaining, downgraded_claim |
| `codex-chat-ui` | `typed_payloads.codex_chat_ui` | real Codex app/chat UI activation, visible marker, screenshot/video/log evidence, helper-vs-real-UI separation, exact blocker if unavailable, remaining, downgraded_claim |

## Join Rules

1. Join only by exact match of `scenario_id`, `marker`, `host`, and `claim_id`.
2. Reject mismatched `host`, `claim_id`, or `marker`.
3. Reject joined evidence without both `automated_boundary_packet_ref` and `local_desktop_packet_ref`.
4. One path cannot prove a joined claim.
5. A blocked Local desktop path downgrades only that `host` + `scenario_id` + `claim_id` + `marker` claim and must preserve explicit `remaining` work.
6. Documentation, dashboard, ledger, checklist, screenshot-only, helper-only, or broad log evidence cannot substitute for either required path.
7. `NOT_RUN`, `NEEDS_VERIFICATION`, `NOT_DISCOVERED_YET`, `NOT_CAPTURED`, and exact blockers are non-claim states until future phases collect evidence.

## PASS Evidence Gate

`PASS_AUTOMATED_BOUNDARY`, `PASS_LOCAL_DESKTOP`, `PASS_JOINED`, and `joined_claim_proven=true` are allowed only when the relevant evidence items are real collected evidence. Every path and joined PASS must satisfy all of these rules:

- evidence item `status` is `COLLECTED` or `ACCEPTED`;
- `artifact_ref_or_NONE` points to a real artifact and is not `NONE`, `NONE_COLLECTED_IN_PHASE_01_01`, `NONE_COLLECTED_IN_PHASE_02_SAMPLE`, or any `NONE_COLLECTED...` placeholder;
- path evidence cannot keep placeholder statuses such as `NOT_RUN`, `NOT_CAPTURED`, `NOT_DISCOVERED_YET`, or `NEEDS_VERIFICATION`;
- Local desktop PASS requires real activation evidence, not `NOT_DISCOVERED_YET`;
- provenance must include a real collection timestamp, not `NONE`;
- blocker/downgrade text must not contradict PASS with `NEEDS_VERIFICATION`, `NOT_RUN`, `NOT_CAPTURED`, `NOT_DISCOVERED_YET`, or `NONE_COLLECTED...`;
- a shape-only `NOT_RUN` sample packet cannot prove readiness and cannot become PASS by editing only status fields.

## Validation Expectations

The local validator must reject:

- missing `scenario_id`, `host`, `claim_id`, or `marker`;
- duplicate `scenario_id` + `marker` pairs in a packet-kind scenario collection;
- missing path `status`, `evidence`, `remaining`, `blocker_or_NONE`, or `downgraded_claim`;
- joined evidence without both path packet references;
- joined evidence with mismatched `scenario_id`, `marker`, `host`, or `claim_id`;
- joined `PASS_JOINED` when either path is missing, blocked, `NOT_RUN`, or `NEEDS_VERIFICATION`;
- joined `PASS_JOINED` or `joined_claim_proven=true` when path or joined evidence still contains placeholder `NOT_RUN`, `NOT_CAPTURED`, `NONE`, `NONE_COLLECTED_IN_PHASE_01_01`, or other `NONE_COLLECTED...` values;
- path PASS when path evidence is not `COLLECTED` or `ACCEPTED`;
- path or joined PASS when artifact refs are missing, `NONE`, or placeholder-only;
- PASS when blocker or downgrade information still says `NEEDS_VERIFICATION`, `NOT_RUN`, `NOT_CAPTURED`, `NOT_DISCOVERED_YET`, or `NONE_COLLECTED...`;
- missing typed payload fields for token, semantic quality, learnability/DPO, asset lifecycle, checkpoint, locator, or Codex chat UI claim families;
- any schema `default` key, because defaults must not convert `NOT_RUN` or `NEEDS_VERIFICATION` into PASS;
- documentation/dashboard/ledger evidence marked as a substitute for either path.

## What This Phase Proves

This phase proves only that the Phase 02 evidence contract artifacts exist and that sample empty packets pass shape validation while negative fixtures fail. It proves that future phases have a checkable form for Automated boundary evidence, Local desktop evidence, and joined evidence.

## What This Phase Does Not Prove

This phase does not prove product readiness, full extension readiness, Codex chat UI readiness, token savings, semantic quality improvement, learnability/training, DPO quality, asset lifecycle correctness, checkpoint mutation/promotion, locator update, host runtime behavior, screenshot/video/log evidence, or any Claude/Cursor/Codex UI result.
