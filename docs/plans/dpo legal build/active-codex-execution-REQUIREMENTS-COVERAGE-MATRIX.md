## Current Code-First Coverage Reconciliation Overlay (2026-07-14)

```text
current_lane: CODE_FIRST_ONLY
implementation_authorization: YES_CODE_FIRST_ONLY
teacher_dpo_training_authorization: NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL
current_core_accepted_pushed_tip: e2a6bce7448a1672a90bdec95106da279b077c5b
published_core_slices: [07, 08, 09, 10, 11, 12, 13]
next_slice: HOST_REFERENCE_SEAM_FIRST_IN_MANUAL_CHAIN__BOUNDED_SLICE_NOT_SELECTED
teacher_stage_state: TP1_LOCAL_NOT_STARTED; TP2_HOSTED_NOT_STARTED
dpo_training_state: NOT_STARTED
architecture_review_state: A1_A5_RESIDUALS_CURRENT
```

Current accepted/pushed code evidence exists for core (`e2a6bce7448a1672a90bdec95106da279b077c5b`), platform (`2763345f377`), Figma (`44dd7ec59e`) and Canva (`82b1debf0d`); flows is clean with no new accepted tip in this packet. Core slices 07-13 are separate accepted/pushed commits, not one composite completion claim. Slice 12 bootstrap-facade evidence is focused `14/14`, required six-class `132/132`, seven classes including Adaptive `156/156`, build `0/0`. Slice 13 explicit-composition evidence is focused `20/20`, runtime `14/14`, DynamicGraph `271/271`, Assistant+Adaptive `44/44`, build `0/0`. This evidence supersedes older “no implementation/no commit/no push” status only for the bounded slices it proves. It does not mark strict composite Gate-B rows `done`, alter the existing 12/27/64 counters, or prove deployment hosting, persistence/restart, authority execution, full machine, teacher, DPO, training, checkpoint or promotion behavior.

Historical-commit rule: every `615d96...`, `002bd7f...`, `d489507e...` or `16e92b46...` occurrence below is historical accepted-slice evidence only; none is the current tip. The only current core tip is `e2a6bce7448a1672a90bdec95106da279b077c5b`.

Open TP1/TP2 provider, license and human decisions are not blockers to the currently authorized deterministic code-first lane. They remain mandatory stage-entry/exit/final-gate requirements before any teacher output can become train-ready. The sole owner of those semantics is `active-codex-execution-two-phase-local-remote-overlay.md`; this matrix intentionally keeps only the pointer.

Deterministic validation, enumeration, routing, merge policy and tie-break remain explicit non-learned requirements. Slices 11-13 now provide production L4 read-only consumption, a bootstrap facade and the first production composition caller outside tests; none confers deployment hosting or completes the L5 machine. The live manual-only chain is deployment host/reference seam → profile deserialize/select plus runtime lifetime → `Runtime.Assistant` invocation → persistence/restart for profile and revision/CAS → authority schema/canon/resolver → remaining deterministic `CODE_FIRST`. This is dependency order only: no bounded next slice is declared architecturally selected or implemented. Architecture review residuals A1-A5 remain current; historical zero-blocker/no-design-choice claims are superseded. A5-R1 is resolved for code-first by Luba's live instruction, while teacher/DPO/training remains `NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL`. Ordered lane: `CODE_FIRST → TP1_LOCAL eval-only → TP2_HOSTED eval-only only after legal proof → G-DPO-FINAL → DPO/training`; TP1/TP2/DPO have not started.

### Bounded capability reconciliation for core slices 07-13

| Slice / commit | What is physically accepted | Test/build evidence | What remains open |
|---|---|---|---|
| 07 / `408044656911d0b5766d71d9723d661e310156ce` | deterministic axis strategy plus bounded multi-combination candidate runner | `32/32`; graph `187/187`; build `0/0`; repeat independent `ACCEPT` after bounded fixes | `[historical_at_slice_07]` no production L4 topology consumer existed at this slice; slice 11 later closes only the L4 read seam. Whole `Ф6.1.1`, judge, DPO and training remain open. |
| 08 / `62c1e996670d8a7786dcf0b4545b5f82e8937254` | deterministic topology build contracts/builder, budget proof and mutation evidence | `24/24`; graph `211/211`; build `0/0`; independent `ACCEPT` | builder proposes/validates snapshots; it is not runtime wiring or machine completion |
| 09 / `382544dd7cfc2a6ff473acdb1b5a6abf82a18064` | `IDynamicGraphTopologySource`, revisioned compare-and-swap runtime owner and immutable history | `36/36`; graph `247/247`; build `0/0`; independent `ACCEPT` | `[historical_at_slice_09]` no L4 production caller consumed the source at this slice; slice 11 later closes only the L4 read seam. Whole `Ф1.1.3`, mutation authority, L5 machine and composite Gate B remain open. |
| 10 / `002bd7f1523f8c3802a341438cf914bc1a37d12d` | strict logical-registry induced-subgraph projector that returns a build signal without applying it | `18/18`; graph `265/265`; build `0/0`; independent `ACCEPT` | `[historical_at_slice_10]` the projector was not owner, runtime consumer or L5 machine at this slice; slice 11 later closes only the L4 read seam, not mutation authority, full `Ф1.3.2`, L5 or Gate B. |
| 11 / `d489507e1e9fb0350c541fc1ac3dd5786e01500d` | production L4 read-only topology consumption through `AdaptiveLogicalOrchestrator` and `LogicalOrchestratorAssistant` composition | focused `20/20`; graph `285/285`; Adaptive/Assistant `64/64`; legacy `24/24`; build `0/0`; independent `ACCEPT` | L4 read seam accepted; full machine, composite Gate B, edge-authority execution and DPO/training remain open |
| 12 / `16e92b46cc4f57c3a73341902a853d86a5c17267` | production bootstrap facade joining explicit registry projection, private runtime ownership, read-only source and assistant | focused `14/14`; six-class `132/132`; seven-class including Adaptive `156/156`; build `0/0`; independent `ACCEPT` | facade is not a deployment host, persistence/restart layer or authority executor |
| 13 / `e2a6bce7448a1672a90bdec95106da279b077c5b` | explicit caller-owned composition profile and first production caller outside tests | focused `20/20`; runtime `14/14`; DynamicGraph `271/271`; Assistant+Adaptive `44/44`; build `0/0`; independent `ACCEPT` | caller validates/snapshots and delegates; host selection/deserialization/lifetime, persistence/restart and authority resolver remain open |

`ACCEPTED_PUSHED_SLICE` is the only status granted by this table. Gate B, deployment runtime, `DynamicGraphMachine`, TP1/TP2 and DPO/training are not complete.

## Historical TP5 Two-Phase Coverage/Readiness Layer (2026-07-13, Проход 5/5; current execution split above)

historical_plan_readiness_layer: PLAN_NOT_READY / TWO_PHASE_ADAPTATION_QUEUE
historical_pre_code_first_implementation_authorization: NO until Luba says `реализуй`

Русское человеческое резюме: 2026-07-13 Luba дала живое указание реализовать teacher-маршрут DPO в ДВЕ фазы (сначала локальный `qwen35-local-v1` = Qwen3.5-9B в Ollama, потом hosted `qwen3-together-v1` = Qwen3-235B-A22B). Это НОВОЕ требование к плану появилось ПОСЛЕ вердикта S17-B04 (2026-07-07 «design complete»), поэтому вердикт «дизайн завершён» стал stale по readiness: двухфазная адаптация ещё раскладывается по владельцам и несёт открытые Luba-решения (Q-TP-*). Единственный владелец двухфазной структуры — `active-codex-execution-two-phase-local-remote-overlay.md`. Текущий вердикт этой матрицы синхронизируется вниз к общему значению `PLAN_NOT_READY`, чтобы убрать stale dual-live между этой матрицей (говорила PLAN_READY) и overlay/16/02/F0 (уже несут PLAN_NOT_READY). Это НЕ занижение floor 20/40-библиотек — они BUILT и остаются binding; это устранение расхождения readiness.

single_value_readiness_reconciliation (CURRENT_READINESS_SINGLE_VALUE_GATE): текущий единственный readiness-вердикт всей семьи плана = `PLAN_NOT_READY`. Носители одного значения: этот TP5-блок (matrix), TP5-блоки в 07 и 16, overlay-владелец (front-matter), 02, F0. Метка очереди `TWO_PHASE_ADAPTATION_QUEUE` — текущая ЖИВАЯ причина очереди (двухфазная адаптация 2026-07-13); более старая `FABLE_GAP_REPAIR_QUEUE` в overlay/02/F0/16 закрыта по составу (Fable-items CLOSED per S17-B04) и совместима — обе метки несут ОДНО readiness-значение `PLAN_NOT_READY`, второго канона readiness нет.

historical_classification_rule_TP5: вердикт S17-B04 (2026-07-07) `PLAN_READY_FOR_LUBA_REVIEW (design complete)` становится historical/pre-two-phase ТОЛЬКО в части READINESS — «design complete» устарел после требований двухфазности 2026-07-13. Пять решений S17-B04, RESOLVED_FROM_DOCUMENTED_AUTHORITY, ОСТАЮТСЯ ДЕЙСТВУЮЩИМИ и НЕ переоткрываются: (a) `Q-P09-XSRC` (RESOLVED_FROM_TASK_PROCESS); (b) `Q-P12-M4` (RESOLVED_FROM_DOCUMENTED_AUTHORITY); (c) legal-teacher canonical-license rows Kimi-K2-`modified-MIT`/GLM/Mistral (RESOLVED_FROM_LEGAL_PART, verify-before-use); (d) `Q-B21-FLOOR` 20/40 floor (RESOLVED_FROM_TASK_MINIMUM, MET, stays binding); (e) `qwen3-together-v1` Together/Qwen3-Apache-2.0 hosted route (RESOLVED_BY_LUBA_DECISION 2026-07-07, выведен из EXTERNAL-карантина). Двухфазность НЕ отменяет ни одно из этих пяти решений; она только секвенирует hosted-этап во вторую фазу и добавляет открытые Q-TP-* поверх уже принятого.

Таблица покрытия двухфазных требований (требование -> план-секция(владелец) -> gate/test -> artifact):

| # | Требование двухфазной адаптации | План-секция (владелец) | Gate / test | Artifact |
|---|---|---|---|---|
| R-TP-1 | Структура двухфазности (этапы TP1_LOCAL/TP2_HOSTED, UML фаз, матрица этапов, критерии перехода) | overlay §0-§9 (единый владелец) | G-TP-1..G-TP-7 (§6) | active-codex-execution-two-phase-local-remote-overlay.md |
| R-TP-2 | Профиль локального учителя `qwen35-local-v1` (Qwen3.5-9B/Ollama) | plan-08 §3.5.2 (+ §3.1 20-полей provenance) | provenance_field_presence_rate==1.0; профиль-канон | plan-08 §3.5.2 TeacherProvenanceProfile |
| R-TP-3 | Teacher-контракт этапа (Шаг 3a: как строится goal per stage) | plan-09 Шаг 3a / B12 / §9 | NEG-2P-a..m (§3.1a) | plan-09 §3 Шаг 3a |
| R-TP-4 | Lifecycle/manifest этапов (terms-гейт по составу строк; TP1-only не false-block) | plan-10 §8.1 stage_scope_gate + Pass16 | stage_scope_gate; fresh_load/continue-training | plan-10 §8.1 / Pass16 контракт |
| R-TP-5 | Измерение разрыва этапов (teacher gap 9B vs 235B) | plan-11 §7 | Гейт 6a/6b; TeacherStageGapReport | plan-11 §7 record |
| R-TP-6 | Тесты двухфазности (негатив/честность/trained) | plan-13 §3.1a | 13 строк a..m (UH-2P-j/TA-2P-k) | plan-13 §3.1a таблица |
| R-TP-7 | Детерминизм tie-break (SHA-256 тройное зеркало) | plan-12 §3.B.2 | tie-break детерминизм; helpers вне счёта 9 | plan-12 §3.B.2 |
| R-TP-8 | Двухфазные Luba-вопросы (Проход 6: 3 resolved + tuning, 1.5 narrowed residual; владелец — 07 финальная секция + F0, здесь указатель) | 07 (финальная секция) + F0 (владелец) | RESOLVED_FROM_DOCUMENTED_AUTHORITY (NUMS/TVT/235B-legality) + EXECUTION_TUNING (KGEN, TVT-пропорции) + LUBA_PRODUCT_DECISION_RESIDUAL narrowed (NOGOLD режим v1; 235B критерий приёмки) | Q-TP-NUMS/KGEN/NOGOLD/TVT/235B-EVAL |

floor_not_lowered: 20/20 + 40/40 knowledge-библиотеки остаются BUILT и binding; TP5 НЕ понижает floor и НЕ помечает библиотеки optional. Счётчики 12/27/64, 14×9/126/3, classified_mechanism_count==9, TeacherProvenance=20, GraphDpoPair=14, Source-to-Plan 64/64 НЕ меняются.

## Historical S17-B04 Documented-Authority Readiness Assessment (2026-07-07; superseded by current A1-A5 residuals)

plan_readiness: PLAN_READY_FOR_LUBA_REVIEW (design complete)
historical_pre_code_first_implementation_authorization: NO until Luba says `реализуй`
historical_s17_verdict_basis: At that historical time, this was the TOP verdict for the requirements-coverage matrix; it supersedes the `S17` overlay below (which still framed `Q-P09-XSRC` / `Q-P12-M4` / `Q-B21-FLOOR` as open `UNDECIDED` Luba decisions). Since that overlay, all plan-level DESIGN decisions have been RESOLVED FROM DOCUMENTED AUTHORITY in their owner files — Luba's answers were already present in the task-process / legal / master documents and were EXTRACTED, not chosen: (a) `Q-P09-XSRC` auto-DPO input source = `RESOLVED_FROM_TASK_PROCESS` (primary = runtime-observations of low-result runs, goal from an allowed external model; seed = cold-start-only, FLOW-38 = auxiliary). Citation: original-task line 20; audit `-original-task-authority-audit-20260629.md:53` (+45/44); owner `-plan-09:268-337`. (b) `Q-P12-M4` scenario phase-plan mechanism #4 = `RESOLVED_FROM_DOCUMENTED_AUTHORITY`: v1 = deterministic `BASELINE_MUST_BE_LABELLED` (choice+order), trainable `SemanticPhasePlanUnit` = named future option. Citation: master §E.0 Q-mech4, Luba 2026-06-30; owner `-plan-12:1321-1384`. (c) legal-teacher canonical-license rows Kimi-K2-`modified-MIT` (verify-before-use, NOT pre-admitted to C5) and GLM/Mistral (per-model canonical-license verification before train/common use) = `RESOLVED_FROM_LEGAL_PART`. Citation: legal G1 teacher-family list `-plan-09:1178-1180`; audit `-original-task-authority-audit-20260629.md:57` `RESOLVED_BY_ORIGINAL_TASK`; owner `-plan-08:148-159` + `family_canonical_license` map `-plan-08:1433-1452`. (d) `Q-B21-FLOOR` 20/40 knowledge-library floor = `RESOLVED_FROM_TASK_MINIMUM`: the task documents the floor as a **minimum** («минимум 20/40 проходов», master §F.5 / Recon Library Gate), the BUILT `20/20`+`40/40` libraries reach that task minimum exactly, so the floor is MET and stays binding — no open Luba «narrow the floor» decision. Citation: owner `-F0-decision-unblocker.md:805-857`. Historical S17-B04 assessment recorded all then-known design decisions resolved; the current architecture review supersedes it with A1-A5 residuals. Bounded planning/governance reconciliation only; no product implementation, tests, builds, runtime, provider/API, training/checkpoint, git, or mergeability proof was run. NOT `FULLY_READY`.
residual_to_full_readiness:
- EXECUTION_PROOF: F0 locks (impl proof pending); R16/R17 host-route / better-output seams (route/code-location proof pending); seed-corpus creation + training/eval/checkpoint/fresh-load/ablation; R16/R17 same-scenario Path A/Path B host proof; branch/mergeability/git proof — pending after `реализуй`.
- EXECUTION_TUNING (set during implementation, NOT open plan decisions): auto-DPO reward weights `w_lift/w_legal/w_contrast/w_leak` numeric VALUES — **legal is a HARD GATE**, not a soft weighted term (owner `-plan-07:1163-1211`) — and the runtime confidence-floor / `enqueue_threshold` numeric value(s) per carrier (owner `-plan-09:333-337`; `-plan-02/04/06/11` confidence-floor). Genuinely not literals in the docs; tuned in implementation; NOT invented here.
- EXTERNAL: `RESOLVED_BY_LUBA_DECISION (2026-07-07)` for the `qwen3-together-v1` route — the Qwen3-Apache-2.0-via-Together hosted provider-terms/output-use item is REMOVED from the EXTERNAL blocker set. Basis (Luba product decision, not a lawyer sign-off): original task whitelist «можно только с допущенными для этого моделями»; Qwen3 = Apache-2.0 = training/distillation permitted, Luba-chosen primary safest auto-DPO teacher; Together ToS 7.2 assigns the customer all right/title/interest in Output, NO explicit no-distillation/no-training-on-outputs clause found (Fable web read 2026-07-07), ZDR available (ToS 3.3); Luba 2026-07-07 «Выведи маршрут из карантина — я изначально потребовала использовать только модели на которых можно обучать». Operational safeguards remain as PRECONDITIONS at `реализуй` (NOT quarantine blockers): ZDR-before-first-call, per-call provenance JSON (model_license=Apache-2.0, teacher_output_used_for_training=true, closed_proprietary_model=false), pin exact Together serving model id + version at cutover; dated ToS/privacy archive is RECOMMENDED, not a gate. SCOPE FENCE: `qwen3-together-v1` ONLY; DeepSeek(MIT) same clean-license footing; Kimi-K2(`modified-MIT`)/GLM/Mistral keep per-model verify-before-use; closed models (Claude/OpenAI/Gemini) stay FORBIDDEN as training teachers.
historical_s17_verdict_classes: HISTORICAL_DESIGN_BLOCKER_ASSESSMENT=NONE_SURVIVING_ALL_DESIGN_DECISIONS_RESOLVED_FROM_DOCUMENTED_AUTHORITY; CURRENT_REPAIR_QUEUE=EXECUTION_PROOF_PLUS_EXECUTION_TUNING (Qwen3/Together EXTERNAL item RESOLVED_BY_LUBA_DECISION 2026-07-07; operational ZDR/provenance steps remain at `реализуй`); IMPLEMENTATION_AUTHORIZATION_GATE=NO until Luba says `реализуй`. [SUPERSEDED_BY_CURRENT_A1_A5_RESIDUALS]
historical_classification_rule_S17_B04: Every overlay/verdict below this block — including the `S17` overlay with `PLAN_READY_FOR_LUBA_REVIEW_EXCEPT_2_LUBA_DECISIONS` and its `UNDECIDED` `Q-P09-XSRC`/`Q-P12-M4` + open `Q-B21-FLOOR` residual, the `Fable Coverage Repair Overlay (2026-07-05)`, the 43c2 downgrade overlay, and the Pass50 overlay — is historical/pre-S17-B04 evidence only; the `EXCEPT_2_LUBA_DECISIONS` / `UNDECIDED` framing for the five decisions above is NOT current. At that historical time, the top verdict for this matrix is this S17-B04 block: PLAN_READY_FOR_LUBA_REVIEW (design complete), implementation_authorization NO until `реализуй`.

## S17 Fable-Reconciliation & Readiness Verdict Overlay (2026-07-07) [HISTORICAL/pre-S17-B04 — superseded by the S17-B04 overlay above; the `EXCEPT_2_LUBA_DECISIONS` / `UNDECIDED` framing is NOT current]

plan_readiness: PLAN_READY_FOR_LUBA_REVIEW_EXCEPT_2_LUBA_DECISIONS (HISTORICAL/pre-S17-B04 — the 2 Luba decisions and Q-B21-FLOOR are now RESOLVED_FROM_DOCUMENTED_AUTHORITY / TASK_MINIMUM per the S17-B04 overlay above)
historical_pre_code_first_implementation_authorization: NO until fresh Luba approval after review + explicit "реализуй"
at_that_historical_time_verdict_basis: This is the top verdict for the requirements-coverage matrix. The FABLE_GAP_REPAIR_QUEUE (`active-codex-execution-42-fable-gap-repair-additions-20260705.md`) has been worked through: every PLAN-FIXABLE Fable item is CLOSED at plan level, and the 50/50 current-ledger pass records are reconciled-against-Fable-evidence (canonical S17 block in `active-codex-execution-40-current-50-pass-readiness-simulation-20260701.md`). Bounded planning/governance reconciliation only; no product implementation, tests, builds, runtime, provider/API, training/checkpoint, git, or mergeability proof was run. NOT full PLAN_READY / FULLY_READY — readiness is gated on 2 remaining Luba product decisions plus execution-proof and one external-terms gate.
fable_plan_fixable_items_CLOSED: FABLE-BLOCKER-01 R-B 20-pass `llm mvp` (20/20 BUILT), FABLE-BLOCKER-02 R-B 40-pass `llm_mvp_core` (40/40 BUILT), FABLE-BLOCKER-04 R-G source addressability (paths `PATH_EXISTS_VERIFIED` + inventory/navigation rows), and FABLE-PARTIAL-01..10 (order taxonomy, `IQueryable<GraphNodeCandidate>` propagation, DynamicGraphNodeDescriptor domain carrier, four-role parity, mojibake sweep, DPO builder lifecycle, `canonical_license`, async scheduling, threshold floors, guide-to-plan rows). The BUILT 20/40 libraries close the artifact/count side of R-B; they are still evidence-library closure, not readiness/implementation proof.
residual_to_full_readiness:
- LUBA_DECISION (2): `Q-P09-XSRC` (auto-DPO loop input-queue source: seed-corpus | runtime-observations | both) and `Q-P12-M4` (scenario phase-plan mechanism #4: remain-deterministic-baseline | promote-to-trainable-L4/L5). Genuine value/scope Luba product decisions (R2/R4), `UNDECIDED`, not self-assignable.
- EXECUTION_PROOF (3): F0 locks (impl proof pending); R16/R17 host-route / better-output seams (route/code-location proof pending); seed-corpus creation + training/checkpoint/fresh-load/ablation + branch/mergeability/git proof (pending after approval).
- EXTERNAL (1): Qwen3 / TogetherAI hosted provider terms + output-use proof before hosted train/common use.
honesty_note_third_open_residual: `Q-B21-FLOOR` also remains a genuine open `LUBA_PRODUCT_DECISION_RESIDUAL`: the 20/20 + 40/40 libraries close the artifact/count side, but the required-DEPTH floor stays binding (`blocked-until-done-or-Luba-approves-narrower`); the `llm mvp` 20-floor is met by 15 genuinely-over-corpus + 5 meta/navigation/synthesis passes, so narrowing the depth floor for v1 needs an explicit current Luba quote. Not silently dropped; classified required-library-work / Luba-scope; must be answered before full readiness.
historical_s17_verdict_classes: HISTORICAL_DESIGN_BLOCKER_ASSESSMENT=NONE_SURVIVING_AFTER_FABLE_PLAN_FIXABLE_REPAIR; CURRENT_REPAIR_QUEUE=FABLE_PLAN_FIXABLE_CLOSED_RESIDUAL_IS_2_LUBA_DECISION_PLUS_QB21FLOOR_PLUS_EXECUTION_PROOF_PLUS_EXTERNAL; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review + explicit "реализуй". [SUPERSEDED_BY_CURRENT_A1_A5_RESIDUALS]
historical_classification_rule_S17: Every overlay/verdict below this block — the `Fable Coverage Repair Overlay (2026-07-05)` with `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`, the Historical Pre-Fable Final Readiness Review Overlay, the 43c2 downgrade overlay, the Pass50 overlay, and any per-row `PLAN_NOT_READY` / `PLAN_PARTIAL_REPAIR_QUEUE` verdict line — is historical/pre-S17 evidence only; its verdict is NOT current. At that historical time, the top verdict for this matrix is this S17 block: PLAN_READY_FOR_LUBA_REVIEW_EXCEPT_2_LUBA_DECISIONS, implementation_authorization NO.

## Fable Coverage Repair Overlay (2026-07-05)

plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE (HISTORICAL/pre-S17 — superseded by the S17 overlay above; see historical_classification_rule_S17)
historical_pre_code_first_implementation_authorization: NO until fresh Luba approval after review
at_that_historical_time_verdict_basis: Newer Fable coverage evidence `claude-fable-plan-check-v1.md` and attachment `98cbe69a` supersede the previous final-readiness overlay. Fable reports 127 findings: 91 COVERED, 30 PARTIAL, 6 MISSING. Mental implementation from active-codex would still hit plan-level design/source/dependency blockers: R-B `llm mvp` 20-pass and `llm_mvp_core` 40-pass knowledge-library targets are accepted only as evidence-library closure, not readiness/implementation proof; R-G source addressability is incomplete; several shared contracts/readability/lifecycle carriers need plan-family repair. This is plan repair evidence only, not product implementation, not tests/builds/runtime/provider/training/checkpoint/git proof. [HISTORICAL/pre-S17: the R-B/R-G/partial items named here are now CLOSED at plan level per the S17 overlay above.]
at_that_historical_time_verdict_classes: DESIGN_BLOCKER_CURRENT=FABLE_MISSING_AND_PARTIAL_ITEMS; CURRENT_REPAIR_QUEUE=FABLE_RB_RG_AND_PARTIAL_REPAIR; FABLE_EVIDENCE=claude-fable-plan-check-v1.md lines 35-153 and attachment 98cbe69a lines 32-79; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=previous 50/50 planning-section acceptance remains historical/process evidence only and cannot override later Fable MISSING/PARTIAL findings; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_classification_rule: Any older line in this file saying PLAN_READY_FOR_LUBA_REVIEW, Final Readiness Review Overlay, no unresolved design/dependency blocker, CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW, or parent final-readiness acceptance before this overlay is historical/pre-Fable evidence only unless it cites a later unquoted Luba instruction or later evidence-bound review after 2026-07-05.
fable_gap_repair_addendum: active-codex-execution-42-fable-gap-repair-additions-20260705.md is the current repair map for R-B/R-G and the partial contract/readability/lifecycle additions.

## Pass31-Pass40 Consumer Sync (40-pass llm_mvp_core evidence library)

sync_scope: evidence-only propagation from `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md` Pass31/40 through Pass40/40. At that historical time, the verdict remained `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE`; `implementation_authorization: NO`.

| requirement area | synced evidence detail | remaining boundary |
|---|---|---|
| R-B knowledge libraries | 40/40 `llm_mvp_core` docs+code target accepted as evidence-library closure. | Not readiness, not implementation authorization, not product proof. |
| Legal/provenance | `GraphDpoSourceRow -> GraphDpoPair -> TeacherProvenance -> legal verdict -> training batch -> checkpoint manifest -> promotion gate`; closed provider output not train target. | Provider terms/legal approval proof remains future route-specific evidence. |
| Unit-level DPO | `GraphDpoPair` stays unit-level with graph-context refs, `UnitId`, `AuthorityUnitId`, chosen/rejected, provenance/legal refs. | Whole-graph pair semantics are not authorized. |
| Lifecycle/measurement | `GraphCheckpointManifest` / `GraphLearningExport` must bind fresh-load, held-out, ablation, promotion, rollback, and contamination rebuild. | No training/checkpoint/runtime proof ran. |
| Scheduler/external/repo gates | Separate caps for `MaxCombinations`, `MaxParallelCombinationRuns`, `MaxParallelBranches`; external-call seam plus deterministic mock and replacement plan; future repo authority discovery and no-write mergeability. | Tests, debt-file creation, git/workdir actions, provider calls, and merges were not run. |
## Historical Pre-Fable Final Readiness Review Overlay (2026-07-05)
historical_pre_fable_plan_readiness: PLAN_READY_FOR_LUBA_REVIEW
historical_pre_code_first_implementation_authorization: NO until fresh Luba approval after review
final_readiness_review_scope: one bounded planning/governance readiness review only; no product implementation, tests, builds, runtime, browser, provider/API, model/training/checkpoint, git, branch, mergeability, PR, or product-readiness proof was run.
historical_pre_fable_current_verdict_basis: This final review read the required governance/skill gate markers, the two unique original-task/reference attachments, the minimum Claude baseline surfaces, the active-codex plan/board/matrix/source-map/legal/gate/ledger surfaces, and current status/search evidence. End-to-end mental simulation from the active plan found no unresolved design/dependency decision that an executor would have to choose before coding/testing. The prior PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE state after 0aa/Laplace/Kepler/Singer was retained for parent final-review acceptance; this overlay supplies that final bounded review. Remaining items are execution or approval gates only: fresh Luba implementation approval, local self-authored seed file creation, code implementation, tests/builds/runtime/training/checkpoint/fresh-load/ablation proof, same-scenario Path A/Path B host proof, mock/replacement-ledger proof, branch/mergeability/git proof if later authorized, final product review, and route-specific hosted TogetherAI/Qwen3 terms/output-use proof before hosted train/common use.
historical_pre_fable_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=50/50 accepted planning sections after 2026-07-05 Pass37-Pass50/Singer repair, not 50 cycles, not 50 product reviews, not product proof, not test/build/runtime/provider/training/checkpoint/git proof; HOSTED_PROVIDER_PRE_USE_GATE_ROUTE_SPECIFIC=TogetherAI/Qwen3 proof required before hosted train/common use only, not a local self-authored v1 planning blocker; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_classification_rule: Any older line in this file saying PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE, current verdict remains PLAN_NOT_READY, CURRENT_REPAIR_QUEUE, parent must run final readiness review, or no-go boundaries before this overlay is historical/pre-final-readiness-review evidence only unless it cites a later unquoted Luba instruction or later evidence-bound review after 2026-07-05.
## Historical 43c2 Readiness Downgrade Overlay (2026-07-02)

historical_pre_final_review_plan_readiness: PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE
historical_pre_code_first_implementation_authorization: NO
latest_attachment: C:\Users\lubao\.codex\attachments\43c2bc75-9134-4cb2-8ad3-8985d2689bb6\pasted-text.txt
evidence_scope: Attachment 43c2 reports newer Claude PLAN_PARTIAL / NOT READY evidence with two plan-level residues. It is evidence-only, not this worker's pass count, not a sub-agent packet, and not implementation proof.
current_residue_queue:
- plan-09 B-15 / IGraphWeaknessDetectorUnit edge/graph wording: v1 train-ready DPO rows remain unit-level only; edge/graph training, scoring, acceptance, and promotion are deferred unless a later explicit Luba-approved plan expands scope.
- plan-06 fan-in merger wording: current canon is IEdgeFanInMerger.Merge(GraphFanInMergeInput input) -> OperationResult<GraphFanInMergeResult>, not a bare LogicalFlowResult return.
historical_pre_final_current_verdict_basis: Newer evidence attachment 0aa5730c, the Erdos bounded worker packet, and the 2026-07-05 Pass37-Pass50/Singer correction supersede stale READY and stale acceptance-count overlays. Historical/pre-Fable evidence supported PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE: current-ledger 50-pass acceptance is repaired as bounded planning-section evidence (50 headings exist and strict acceptance-field/readability evidence is 50/50 after the 2026-07-05 Pass37-Pass50/Singer repair). This queue-classification repair finds no current plan/design blocker in the stale CURRENT_REPAIR_QUEUE labels: F0 locks are recorded with implementation proof pending; legal/provenance is propagated in active plan, policy, and source-map surfaces; hosted-provider proof is a route-specific external pre-use gate before hosted Qwen3/Together train/common use, not a local self-authored v1 planning blocker; future execution proof remains after approval; fresh Luba post-review approval is implementation authorization, not a plan-design blocker. This is not 50 cycles, not product readiness, not PLAN_READY, not B-22 closure, and not implementation authorization.
historical_pre_final_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_IDENTIFIED_BY_THIS_QUEUE_CLASSIFICATION; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=50/50 accepted planning sections after 2026-07-05 Pass37-Pass50/Singer repair, not cycles/readiness/product/implementation proof; CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_THIS_QUEUE_CLASSIFICATION; F0_LOCKS_RECORDED_IMPLEMENTATION_PROOF_PENDING=Q3/Q6/Q8/Q11/Q14/Q30/Q38 recorded, Q37/Q-host implementation checks only; LEGAL_PROVENANCE_PROPAGATED_IN_PLAN_EXECUTION_PROOF_PENDING=20-field TeacherProvenance, ProviderProof, LicenseProof, LegalVerdict, C16/C17 quarantine/block/rebuild behavior present in active plan/policy/source-map surfaces; HOSTED_PROVIDER_PRE_USE_GATE_ROUTE_SPECIFIC=TogetherAI/Qwen3 proof required before hosted train/common use, not local self-authored v1 planning blocker; IMPLEMENTATION_AUTHORIZATION_GATE=fresh Luba post-review approval required before execution, not plan-design blocker; EXECUTION_PROOF_PENDING_AFTER_APPROVAL=code implementation, seed file creation, tests/builds/runtime, mock execution/replacement, training/checkpoint/fresh-load/ablation, same-scenario dual-path proof, branch/mergeability/git; readiness_candidate_for_parent_review=HISTORICAL_ZERO_BLOCKER_ASSESSMENT_SUPERSEDED_BY_A1_A5_RESIDUALS; parent must run final readiness review before any PLAN_READY wording; HISTORICAL_PRE_0AA_EVIDENCE=older PLAN_READY_FOR_LUBA_REVIEW / READY and older NOT_READY records unless a later evidence-bound repair proves current status.
## Historical/pre-43c2 Pass50 Readiness Verdict Overlay (2026-07-02)

historical_pre_43c2_plan_readiness: PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE
historical_pre_code_first_implementation_authorization: NO until fresh Luba approval after review
current_pass_chain: 50 canonical pass headings exist and strict current-ledger acceptance-field/readability evidence is now 50/50 after the 2026-07-05 Pass37-Pass50 bounded acceptance repair recorded in active-codex-execution-41-current-ledger-50-pass-acceptance-index.md. This is 50 accepted planning pass sections, not 50 cycles, not product readiness, not PLAN_READY, not B-22 closure, and not implementation authorization.

Historical/pre-43c2 Pass50 basis:
- Original-task R1-R10 are FULLY_COVERED_PLAN_LEVEL by Pass46 and this Pass50 adversarial review.
- B-1..B-22 have no remaining implementable active-codex design/dependency blocker under the Pass39/Pass43/Pass44 current overlays. Hosted provider proof remains required only before hosted train/common use.
- Source-to-Plan inventory is 64/64 by exact canonical row-id audit, with 0 noncanonical row-like offenders.
- Claude/latest-baseline details are carried in active-codex: five participating repos, DPOLegalLearning work name, C:\Projects workdir boundary, latest-updated source-branch authority as active/Codex delta, xiigen-platform facade plus xiigen-flows seam, FigmaToCode/CanvaToCode as clients/callers, unit-level 14-field GraphDpoPair, 20-field TeacherProvenance, exactly one active language-translate pilot, and provider/terms boundary.
- QA, tests, builds, screenshots, same-scenario dual-path proof, mock replacement, provider/API proof, runtime/training/checkpoint proof, branch/mergeability, git, PR, and product review remain future execution-continuation gates after fresh Luba approval. They are not design decisions and were not run in Pass50.
- This Pass50 overlay is historical/pre-43c2 evidence only. Newer attachment 43c2 supersedes it; READY wording in this block is not current readiness, not B-22 closure, and not implementation authorization.

Readback: This Pass50 paragraph is historical/pre-43c2 evidence only. Attachment 43c2 triggered a downgrade, the residues were repaired, and the historical/pre-0aa post-repair-review active-codex verdict was PLAN_READY_FOR_LUBA_REVIEW; historical/pre-Fable 2026-07-02 bounded-repair verdict was PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE; implementation_authorization remains NO.
---
artifact_type: requirements_coverage_matrix
authority_source: latest unquoted Luba work order for DPO Legal Learning active execution chain
status: plan_only_claude_owned
last_updated: 2026-06-29
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-*.md
benchmark_source_read_only: codex-REQUIREMENTS-COVERAGE-MATRIX.md (+ codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md, codex-plan-04/07/08/12, codex-EXECUTION-CONTROL-BOARD.md)
---

## Pass 13 Code Reference Inventory Coverage Rule

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

R-OT-13 coverage requires honest code anchors, not decorative `file:line` text. For coverage rows in this matrix, `PATH_EXISTS_ONLY` means the path exists but no line/quote proof was read in the current pass; `LINE_REF_VERIFIED` and `QUOTE_VERIFIED` require direct source-line or quote readback; `NEEDS_CODE_REF_INVENTORY` blocks implementation of any row that needs exact anchors; `HISTORICAL_UNVERIFIED` marks older owner-doc, benchmark, or ledger anchors. No fake line refs or invented quotes are allowed, and docs alone cannot prove implementation exists. Future implementation must refresh anchors from the latest updated branch in the `DPOLegalLearning` workdir; implementation proof pending until then.

## Pass 14 Repo/Branch/Workdir Authority Coverage Rule

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

R-OT-14 coverage requires a future implementation worker to identify repo authority, workdir, mergeability gate, no-PR/no-merge boundary, and planning-repo boundary from active-codex without asking Luba. Covered-in-plan rule: source authority comes from the selected latest updated branch or most advanced branch in each implementation repo, not from a copied workdir name, `planning-dpo-learning`, stale memory, or `C:\Users\lubao\Documents\xiigen`. The working name is `DPOLegalLearning`, with separate future implementation copies under `C:\Projects` after readiness and approval. Before every future commit/push, refresh/check the original authority branch and run a no-write mergeability/no-conflict check back into that original branch, and into `main` where project rules require main-branch mergeability. PR/merge is forbidden unless Luba later explicitly asks. Mergeability is not proven by this planning pass.

## Pass 15 Test/Build/Mock Verification Coverage Row

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| requirement | current plan coverage after Pass 15 | not proven / remaining future evidence |
|---|---|---|
| Future tests/builds/runtime gates are explicit and executable | Covered in plan text as future obligations: build evidence, unit-level DPO tests, exact external mock response tests, legal/provenance tests, measurement/scorer tests, same-scenario integration/runtime proof, and replacement-ledger audit. | No tests, builds, runtime probes, provider/API calls, browser checks, or product code ran in this pass. |
| External API/resource seams have mock contract plus unit test plus replacement plan | `R22.3` and plan-13/plan-08 require request fixture, exact expected mock response/result, unit test name/assertion, and future replacement row only in `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`. | The mock ledger file is not created now; real replacement and real external integration remain future evidence. |
| Future gates are continuation checkpoints | Board/master/full-task gate say failures route to bounded repair workers or true external blocker after mitigation and are not stop permissions while safe bounded work remains. | Future workers must still execute and record the commands/artifacts after implementation approval. |
| Tests/builds cannot hide design blockers | Plan text states provider policy, provenance policy, source-map authority, thresholds, input/output contracts, mock response contracts, replacement-ledger obligations, and model/checkpoint lifecycle must be resolved before readiness. | Historical/pre-post-repair active-codex status at that pass was `PLAN_NOT_READY`; implementation authorization remains `NO`. |

---

# Матрица покрытия требований — ILearningDynamicGraph (версия Claude)

Артефакт: `claude-REQUIREMENTS-COVERAGE-MATRIX.md`. Статус: ТОЛЬКО ПЛАН, владелец — Claude.
Это улучшенная версия матрицы Codex в моём namespace `claude-*`. Все `codex-*` файлы и старый
каркас `claude-00..07` прочитаны как бенчмарк/evidence (только чтение) и НЕ редактировались.

> Зачем нужен этот документ (человеческим языком). Это «карта обещаний»: один список того,
> что Luba потребовала от системы `ILearningDynamicGraph`, и напротив каждого требования —
> где оно решается (какая фаза и какой план-блок), каким Gate B (точное проверяемое условие)
> оно закрывается, и насколько честно оно покрыто **на уровне плана** (не на уровне кода —
> код ещё не разрешён). Матрица не утверждает, что что-либо реализовано, обучено или
> проверено в рантайме. Она утверждает только полноту и согласованность планирования.

---

## 0. Якорь задачи (Current task anchor)

```text
CURRENT TASK:
Построить мою (claude-*) улучшенную матрицу покрытия требований ILearningDynamicGraph,
читая codex-* как бенчмарк. Только план. Редактируемый namespace — только claude-*.md
в папке "dpo legal build".

NOT THE TASK:
Не редактировать и не создавать ни один codex-* файл (план Codex, codex-recon-*,
codex-50-cycle-*, codex-skill-improvement-*). Не писать/менять код, билд, тесты. Не git.
Не использовать внешние AI/модели. Не продолжать и не «чинить» рабочие артефакты Codex —
они только бенчмарк. Не выдавать ledger-строки за реальные циклы ревью. Не заявлять
реализацию/обучение/рантайм как сделанные.

EVIDENCE-ONLY:
Все codex-*.md (включая codex-REQUIREMENTS-COVERAGE-MATRIX.md, codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md,
codex-plan-00..14, codex-EXECUTION-CONTROL-BOARD.md, codex-BENCHMARK-DELTA-REPORT.md,
codex-GPT55-50-CYCLE-REVIEW-LEDGER.md, codex-recon-*), старый каркас claude-00..07,
прикреплённые тексты, старые пакеты суб-агентов. Это материал для чтения; он не создаёт
полномочий на исполнение, git, тесты, билды, обучение или расширение объёма.
```

```text
CURRENT_UNQUOTED_LUBA_INSTRUCTION = построить claude-REQUIREMENTS-COVERAGE-MATRIX.md как улучшенную версию, читая codex-* как бенчмарк; PLAN-ONLY; редактировать только claude-*
QUOTE_DELIMITER_PARSE: detected_quote_marker=none (current instruction unquoted); quoted_span_status=evidence_text_only; may_execute_from_quote=false
ATTACHMENT_QUOTE_PARSE: detected_attachment_quote_marker=none; allowed_use=benchmark_only; may_execute_from_attachment=false
CURRENT_TASK_DERIVATION: current_task_source=current_unquoted_luba_message; quoted_context_used_as_task_source=false; evidence_text_created_subrequirement=false
implementation_authorized_at_claude_plan_creation = false
current_live_instruction_2026_06_29_implementation_authorized = false
current_live_instruction_scope = active-codex-execution plan repair only; no implementation workdirs, no PR/merge/planning-repo git realignment
```

---

## 1. CANON-инварианты (сохранены без изменений)

Эти инварианты — общая истина для всех `claude-*` блоков и совпадают с каноном Codex
(в этом и состоит требование консистентности). Я их не ослабляю и не переопределяю.

| Инвариант | Человеческое значение | Источник-привязка (file:line) |
|---|---|---|
| **A-LM-12** — закрытый набор из 5 уровней обучения | `ILearningDynamicGraph` = специализация L5 `ILearningMachine<DynamicGraphDomain>`, а **не** новый 6-й уровень. 5 разрешённых уровней: `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`, `ILearningMachinePair`. | `docs/DECISIONS-LOCKED.md:35`; контракт L5 `00.Core/LearningMachineContracts.cs:208` |
| **A-LM-02** — `Train*` только на `ILearningMachine` | Узел/поток/оркестратор — это чистый inference; обучение живёт только на машине L5. | `docs/DECISIONS-LOCKED.md:25` (см. также CLAUDE.md «Separation rule A-LM-02») |
| **A-LM-09** — финал ребра принимает обученный узел | На каждом ребре финальную authority выбора принимает обученный `EdgeAuthorityUnit` (один вход `EdgeAuthorityInput` → один выход `EdgeAuthorityDecision`), а не детерминированная формула. | `docs/DECISIONS-LOCKED.md:32` |
| **A-LM-13** — нет скрытого детерминизма под видом «семантики» | Если код/доки называют механизм «семантическим юнитом», он обязан быть обученным с lifecycle-доказательствами, иначе claim понижается. | `docs/DECISIONS-LOCKED.md:36` |
| **Легальный teacher-гейт** | Teacher для дистилляции/обучения — только open-weight permissive with RR6 plain/no-grounding terms gate. Closed (Claude/OpenAI/Gemini) — только **судья** при `used_for_training=false` and terms proof. `closed_proprietary_model=true && used_for_training=true ⇒ DATASET_REBUILD_REQUIRED`. Контаминация (`sonnet-*`) ⇒ `DATASET_REBUILD`. | active/Claude plan-08 (`TeacherProvenance` 20 fields, C17/RR6); active/Claude plan-07 (`GraphDpoPair` 14 fields); CLAUDE.md «DPO/distillation legal teacher provenance review gate» |

> Почему это важно прямо в матрице. Матрица — это место, где легче всего «срезать угол»:
> назвать детерминированный роутер «обученным юнитом» и поставить `covered`. Инварианты
> выше — это запрет такого среза. Любая строка ниже, которая претендует на обученность,
> обязана опираться на A-LM-09/12/13 и легальный гейт, иначе её честный статус — `partial`.

---

## 2. Канонические контракты (общий словарь, синхронен с claude-06 и codex-каноном)

Эти имена типов одинаковы во всех моих `claude-*` блоках и в бенчмарке Codex. Я их фиксирую
здесь как единый словарь, чтобы строки требований ссылались на точные имена, а не на описания.

| # | Канон-тип | Что это (человеческим языком) | Уровень | Владелец-блок |
|---|---|---|---|---|
| 1 | `IDynamicGraphNode` | Обёртка над узлом-`ILogicalUnit`; **без** `Train*` (A-LM-02). | L2 | план-01 |
| 2 | `IDynamicGraphEdge` (+`GraphEdgeKind`) | Типизированное ребро с per-edge обучаемой authority и анти-leakage полями. | L3 | план-02 |
| 3 | `GraphEdgeKind` (enum, ровно 5) | Виды ребра: `FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop`. | — | план-02 (канон §A.4.1) |
| 4 | `EdgeAuthorityUnit` (`EdgeAuthorityInput`→`EdgeAuthorityDecision`) | Обучаемый финальный решатель ребра; заменяет baseline-роутер по cutover. | L2-юнит | план-02 |
| 5 | `DynamicGraphTopology` (+`BuildDynamicTopology(DynamicGraphBuildSignal, TopologyBuildBudget)`) | Мутируемая на лету карта узлов/рёбер; строится обученной логикой, **не** матрицей смежности. | — | план-03/04 |
| 6 | `DynamicGraphMachine : ILearningMachine<DynamicGraphDomain>` | Владеет обучением, оценкой, export/import и постройкой рантайм-оркестратора. | L5 | план-04 |
| 7 | `DynamicGraphMachinePair : ILearningMachinePair<TPersonal,TGlobal>` | Later activation blueprint for common plus personal/adaptive legs; v1 active route is common-only and client/private material cannot train common by default. | L1-пара blueprint | план-04/10 |
| 8 | `GraphDpoBuilder.BuildPairAsync` → `GraphDpoPair` | Авто-строитель: из сравнения прогонов графа делает легальную DPO-пара только при тройном гейте. | — | план-07 |
| 9 | `GraphDpoPair` (канон-поля) | PASS12 current train-ready invariant: `pair_id; level=unit; unit_id; logical_unit_ref; unit_context_ref; chosen_unit_output_ref; rejected_unit_output_ref; path_order_ref; source_ids[]; provenance_refs; legal_scope; split_refs(held_out + split_id); candidate_order_invariant; excluded_leakage_signals[]; dpo_train_ready(gate)`. Historical/pre-PASS12 nested `teacher_provenance{...}` / `level(unit\|edge\|graph)` wording is baseline evidence only, not current train-ready DPO authority. | — | план-07 (владелец канона) |
| 10 | `TeacherProvenance` (20 полей; 13+5+2 RR6 `service_mode`/`grounding_used`) | 13 базовых + 5 NEW. См. §3 ниже. Предусловие `dpo_train_ready` — `provenance_field_presence_rate==1.0` по всем 20 in sidecar plus `row_sidecar_ref_match_rate==1.0`. | — | план-08 (владелец канона) |
| 11 | `DynamicGraphLegalGate` | Проверяет источник, модель/судью и provider-terms до входа строк в train/held-out. | — | план-08 |
| 12 | `GeneratedUiOutputFeedbackEvent` | Единственный вход для FLOW-38 и UI-фидбэка в DPO. | — | план-07 |
| 13 | `ITeacherGoalClient` / `IJudgeScoreClient` | Provider seams for real teacher-goal and judge-score calls with request/response provenance, auth-secret boundary, mock/real-call debt, and closed-provider eval-only rule. | boundary seam | план-08/09 |
| 14 | `TrainableGraphDpoBuilderUnit` | Learned unit-level pair-construction policy beyond deterministic `GraphDpoBuilder`; owned/contained/coordinated by `ILearningDynamicGraph`; owns unit-pair policy state/checkpoint, selected `UnitId`, builder score, rejection reason, legal/provenance hard-gate recommendations, held-out metric, and ablation proof. | L2 `ILogicalUnit` trained by L5 | план-07/04/11 |
| 15 | `GraphOrderResolverUnit` | Pass05 learned resolver for `order-0-direct-unit`, `order-1-single-predecessor`, `order-2-fan-out-fan-in`, `order-3-cross-domain-reference`, and `order-4-async-many-to-many-aggregation`; deterministic topology is correctness/safety baseline and candidate enumerator only, not final learned authority. Output must be `GraphOrderDecision(..., path_order_ref, legal_status, provenance_status, trace_ref, reason_codes[])` feeding B6 unit-level DPO metadata only. | L2 unit trained by L5 | план-04/09/11/13 + Pass05 gate |
| 16 | `GraphNodeCandidate` | PASS10 canonical node/many-to-many candidate element. Public surface is `IQueryable<GraphNodeCandidate>`; B11 materializes `IReadOnlyList<GraphNodeCandidate>` snapshots inside `AsyncCandidateBatch`. Required fields: candidate/unit/logical-unit refs, output refs/type, confidence/score, provenance refs, `path_order_ref`, legal/status/error fields, source ids, trace refs, and B1/B7/B11/B12/B6 handoff refs. Raw `object Output` and final untyped arrays are not canonical. | typed output contract / B8-B9 | мастер §E.0/PASS10; план-06; план-09 |
| 17 | `GraphDpoProvenanceWriter` | PASS12 planned writer/normalizer, not a new trainable architecture level: writes sidecar manifest JSONL with schema_version/timestamp/TeacherProvenance/TriadVerdict/provenance ids and row refs before any `dpo_train_ready` or common output. Inputs: B1 teacher/judge, B12 winner refs, B6 unit pair candidate, B7/B10 path/order refs. | writer/storage role / B10-B13-B17 | план-07/08/09 |

> При любом расхождении числа полей или размещения `judge_model` между этой матрицей и
> блоком 08 — **владелец канона — блок 08** (план-08). Эта матрица не переопределяет канон,
> она его трассирует.

---

## 3. Канон `TeacherProvenance` — 20 полей (13+5+2 RR6 `service_mode`/`grounding_used`) (владелец: план-08)

13 базовых: `teacher_access_mode; hosting_provider; model_id; model_license; model_version;
request_id; provider_terms_checked; teacher_output_used_for_training; closed_proprietary_model;
source_ref; source_sha256; contamination_markers; judge_model` (поле `judge_model` —
**внутри** `TeacherProvenance`, не отдельным верхнеуровневым полем).
5 NEW (бенчмарк-дельты T2/T3/T5): `terms_snapshot_ref` (T2); `geography_export_constraint` (T2);
`source_category` (T5); `raw_private_text_copied` (T3); `redaction_policy_ref` (T3).

`SourceCategory` (8 значений): local, human, open_weight, hosted, deterministic_baseline,
self_judge, client_unverified, third_party.
`LegScope` (4 значения): CommonGlobal, PersonalAdaptive, EvalOnly, Quarantine.
Вердикт-enum — **ровно 6 значений** (выровнено с владельцем канона план-08 §3.3):
`ACCEPT | QUARANTINE | BLOCK | DATASET_REBUILD_REQUIRED | BLOCK_FOR_COMMON | EVAL_ONLY`.
`EVAL_ONLY` — **полноправный** член канона (фикс R17: легально-чистая, но train-ineligible
строка из-за неполной триады C15/C15b или невоспроизводимого конверта C8), **а не только
«операционное» значение гейта**. (`BLOCK_FOR_HOSTED` остаётся внутренним операционным
значением гейта план-08 и не входит в канон-enum строки `GraphDpoPair`.)

PASS12 storage envelope (not a second field dictionary): sidecar manifest JSONL stores
`schema_version`, `created_at`, `pair_id`, source teacher/provider, model id/version,
license, terms snapshot, proof_right_to_use_train_ref, source category, output hash/ref,
redaction status, provenance ids, judge_only/train_allowed flags, common/adaptive scope,
the canonical 20 `TeacherProvenance` fields (13+5+2 RR6 `service_mode`/`grounding_used`), optional JudgeProvenance, `TriadVerdict`,
and row_ref_back_to_GraphDpoPair. `GraphDpoPair` stores refs/hashes only.

---

## 4. Нумерация фаз (канонические счётчики после RR4/Q41/PASS26)

```text
schema = Ф<P>.<SP>.<SS>   (Фаза.ПодФаза.ПодШаг)
baseline_phases    = 12   (Ф0…Ф11)
baseline_subphases = 27
baseline_substeps  = 64
phase4_3_numbering = Ф4.3.1 unit / Ф4.3.2 edge / Ф4.3.3 split   (чисто 1..3, без .0)
pilot_subphase = Ф10.3 language-translate (Q41=Вариант B RESOLVED 2026-06-30; ровно 1 пилот)
pilot_rows = Ф10.3.1..Ф10.3.5  (в baseline-64, status remaining)
historical_pre_PASS26_reserve_rows = Ф10.3.1..Ф10.3.6 (old six-domain reserve outside baseline-59; not current)
```

| Фаза | Человеческое имя | Что решает фаза (одно предложение) | План-блоки |
|---|---|---|---|
| Ф0 | Решения Luba | Зафиксировать открытые архитектурные решения до контрактов. | мастер §E |
| Ф1 | Контракты/фундамент | Объявить NEW-контракты графа (узел/ребро/топология/L5-каркас). | 00, 01, 02, 03, 04, 05 |
| Ф2 | Legal-гейт + канон | Утвердить `GraphDpoPair`+`TeacherProvenance`, включить legal teacher-гейт, карантин `sonnet-*`. | 07, 08 |
| Ф3 | DPO-строитель | Авто-построение `GraphDpoPair` из несуществующих данных (5 форматов → 1 канон). | 07, 06 |
| Ф4 | Обучение + lifecycle | LoRA+DPO, обучаемый узел (unit) и ребро (edge), export/import/split. | 10, 04, 01, 02, 03, 12 |
| Ф5 | Измерение | Held-out метрики качества/скорости/точности без UNKNOWN. | 11, 13 |
| Ф6 | Async-комбинации | Один вход → N комбинаций → лучший; many-to-many merge. | 06, 03, 05, 02 |
| Ф7 | Авто-DPO петля | Петля само-улучшения с честным no-signal стопом. | 09, 07, 08 |
| Ф8 | No-hidden-determinism | Cutover финала ребра на обученный юнит; запрет скрытого детерминизма. | 12 |
| Ф9 | Тесты/QA матрица | Сквозная тест-стратегия по 9 слоям (untrained + trained раздельно). | 13 |
| Ф10 | API/клиенты | Карта референсов + docs-sync; активный пилот `language-translate` (Ф10.3.1..Ф10.3.5) + navigation/reference-only R15-домены. | 14 |
| Ф11 | Финальная приёмка | 10 execution-reviews при реализации; финальная матрица без дыр. | мастер §B; CLAUDE.md |

---

## 5. Карта двух нумераций R1..R20 (Luba) ↔ R-01..R-19 (мастер)

> Улучшение против бенчмарка. Матрица Codex ссылается на «§5 п.1 (источник карты)», но
> саму карту соответствия в матрице не приводит — это разрыв. Я привожу карту прямо здесь,
> чтобы документ был самодостаточным (FC-8 self-containment).

> **Crosswalk-заметка для трёх claude-артефактов (no_mess, binding).** Три документа
> используют две согласованные нумерации одного и того же набора требований:
> - **claude-доска `claude-EXECUTION-CONTROL-BOARD.md`** трассирует требования под
>   нумерацией **`R1..R20`** — это **формулировки Luba** (как в задании);
> - **эта матрица и мастер-план `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` §B.1** используют
>   внутренний маппинг **`R-01..R-19`** (где `R-17/R-18/R-19` — находки рекона), плюс два
>   требования задания (`R19`-execution-visibility и `R20`-50-циклов), которые живут в
>   доске.
>
> Это **одна и та же истина в двух нумерациях**: множества `covered`/`partial` **идентичны**
> во всех трёх файлах (`covered=12 / partial=8 / unmapped=0`, набор — §6). Полная
> построчная карта `R1..R20 ↔ R-01..R-19` — таблица §5 ниже (единственный источник карты).
> Любое расхождение нумераций или множеств `covered`/`partial` между доской, матрицей и
> мастером = дефект консистентности (Contradiction Sweep).

| Luba R# (эта матрица) | Мастер R-## | Совпадение / примечание |
|---|---|---|
| R1 | R-01 (роли L2/L3/L4/L5) + прямой субъект обучаемого узла Ф4.3.1 | DPO для разных уровней `ILogicalUnit` внутри машины |
| R2 | R-02 | Авто-построение DPO из несуществующих данных |
| R3 | R-01 (часть) | L5, не 6-й уровень (A-LM-12) |
| R4 | R-04 (judge closed) / R-01 (топология обучается) | в задании: динамическая топология ≠ матрица смежности |
| R5 | R-07 | Финал ребра обучаемый, не скрытый детерминизм |
| R6 | две грани: (1) рекон-библиотека/floor 20-40 (мастер §F.5); (2) доска §D, R20-семейство | (1) фон recon-library `external≥20`+`core≥40`+Knowledge Library; (2) честность счёта ревью-проходов |
| R7 | R-02/R-06 (no-signal путь) | Авто-DPO петля без ложного промоушена |
| R8 | R-05 | `GraphDpoPair` несёт триаду + per-side authority/origin |
| R9 | (доска §D / R20) | Не раздувать per-document авторство |
| R10 | R-06 | Async/fan-out/fan-in/many-to-many имеют явные гейты |
| R11 | две грани: (a) R-19(Luba execution-visibility) — нумерация; (b) блок 14 §3.2 — референс-на-каждый-этап | (a) стабильность `Ф<P>.<SP>.<SS>` (12/27/64 после RR4/Q41/PASS26) = covered; (b) Source-to-Plan на все 64 (partial; карта ещё не 64/64) |
| R12 | R-12 | codex-префикс; claude-авторские codex-файлы = evidence-only исключения |
| R13 | (индекс план-блоков P-00..P-14) | Все source-блоки представлены |
| R14 | R-05 | Паритет provenance по train-ready проекциям |
| R15 | (доска; active Ф10.3 pilot) | Q41 resolved: one active `language-translate` pilot; other R15 domains navigation/reference-only |
| R16 | R-10 (SPLIT) | Common vs personal/adaptive host — решение Luba |
| R17 | (доска / FLOW-38) | FLOW-38 better-output legality — решение Luba |
| R18 | R-12 (исключения префикса) | Правило codex-префикса с исключениями |
| R19 | R-14 | Вопросы только в финальных секциях |
| R20 | мастер §E + доска §D; Ф11.2 | До 50 проходов review/fix перед claim полноты |

> Внимание (анти-путаница, сохранено из бенчмарка): мастер `R-19` (находка рекона —
> расхождение указателей train-ready V192/V309) и задание Luba `R19` (execution-visibility) —
> **разные** требования с совпавшим номером. Не путать.
>
> Двухгранные требования (анти-путаница, по тому же принципу). Чтобы матрица и
> мастер-план давали ОДИН ответ, два ID несут по две грани с разными статусами:
> - **R6** = (1) рекон-библиотека / floor 20-40 (мастер §F.5) + (2) честность счёта
>   ревью-проходов (доска §D). Грань (1): floor-артефакт грань теперь **MET** — в
>   active-codex namespace построены и parent-verified `20/20`
>   (`active-codex-execution-knowledge-library-llm-mvp-20-pass.md`) и `40/40`
>   (`active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md`). *(Историческое/pre-build:
>   грань (1) держалась только бенчмарком `codex` как evidence-only, а в claude-каноне
>   рекон-файлов/Knowledge Library = 0, `passes_unproven=true`.)* Закрытие floor — это
>   evidence-library closure, НЕ readiness / НЕ implementation authorization; recon-глубина
>   остаётся BINDING (F0 `Q-B21-FLOOR`). Грань (2)
>   — процессное правило вне R20. Обе раскрыты в строках R6 §7 и в карте гейтов §8
>   (Recon Library Gate).
> - **R11** = (a) стабильность нумерации `Ф<P>.<SP>.<SS>` (= мастер `R-19` Luba
>   execution-visibility) — **covered** для текущего baseline-64; (b) референс-на-каждый-этап /
>   Source-to-Plan на все 64 подшага (claude-plan-14 §3.2, владелец — блок 14) —
>   **covered as inventory evidence only after Pass30/Pass39**: plan-14 §3.2 now has 64 canonical `Ф<P>.<SP>.<SS>` rows, but this is source-map inventory evidence, not readiness, B-22 closure, implementation authorization, code proof, test proof, or execution proof.
>   Один ID, две несовпадающие грани — не путать «нумерация стабильна» с
>   «референс есть на каждый из 64».

---

## 6. Текущее честное состояние

```text
returned_review_packets_claimed_in_benchmark = 50/50      (источник: codex-GPT55-50-CYCLE-REVIEW-LEDGER.md, evidence-only, ОСПОРЕНО — см. строку R20 в §7 и сноску ниже)
benchmark_50_cycle_claim_disputed = true                  (codex-50-cycle-plan-review-supplement.md:31-36: subagent_cycles_completed=0 / independent_review_fix_cycles_proven=0; Luba: «no fucking way you made 50 cycles with a sub agent»; benchmark: «The challenge is correct»)
real_review_fix_pass_count_for_claude = 0                 (для claude-* deliverables нет ни одного returned-пакета; 50-счёт codex-* НЕ переносится на claude-артефакты)
requirements_total = 20
covered = 12
partial = 8
unmapped = 0
covered_set = R2, R3, R4, R5, R7, R8, R10, R11, R12, R13, R18, R19
partial_set = R1, R6, R9, R14, R15, R16, R17, R20
baseline_phases = 12
baseline_subphases = 27
baseline_substeps = 64
pilot_subphase = Ф10.3 language-translate (Q41=Вариант B RESOLVED 2026-06-30; ровно 1 пилот)
pilot_rows = Ф10.3.1..Ф10.3.5 (в baseline-64; status remaining)
blocked_at_plan_start = 0
historical_pre_PASS26_reserve_rows = Ф10.3.1..Ф10.3.6 (old six-domain reserve outside baseline-59; not current)
implementation_authorized_at_claude_plan_creation = false
current_live_instruction_2026_06_29_implementation_authorized = false
current_live_instruction_scope = active-codex-execution plan repair only; no implementation workdirs, no PR/merge/planning-repo git realignment
claude_files_edited = только claude-* (этот файл)
codex_files_edited = false
```

> Почему `covered=12 / partial=8` (а не `13/7`). R20 понижен `covered → partial (benchmark-disputed, claude real_pass=0)`
> в синхрон с честным статусом мастер-плана `claude-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` §F.6
> (строки 809-831) и его счётчиком §B.12 (строка 472 «50/50 заявлено в бенчмарке,
> оспорено»). Исторические/pre-PASS12 структурные счётчики (12 фаз / 26 подфаз /
> 59 подшагов, 7 NEW-типов, 5 `GraphEdgeKind`, прежние 14 полей `GraphDpoPair`,
> 20 полей `TeacherProvenance` (13+5+2 RR6 `service_mode`/`grounding_used`)) остаются trace evidence. Pass37/latest-Claude current authority
> для `GraphDpoPair` — exact active/Claude plan-07 14-field canon with nested
> `TeacherProvenance`; unit-level остаётся train-ready invariant, not a schema
> downgrade. Меняется только честный учёт покрытия требования R20
> (Dishonest Claim Rejection Gate: ledger-строки ≠ доказанные циклы).

> Что значит `partial` честно (улучшение читабельности). `partial` здесь = «план-структура
> на месте, но финальное подтверждение требует либо доказательства уровня реализации
> (которое запрещено сейчас), либо решения Luba». Это не «почти covered» и не «сделано с
> оговорками» — это явный незакрытый ряд с известным следующим шагом (см. колонку «Что
> остаётся» в §7).

---

## 7. Матрица требований

Колонки: ID; требование (человеческим языком); где решается (фаза + блок-владелец);
Gate B (точное проверяемое условие); статус; что остаётся (для `partial`).

| ID | Требование | Где решается (Фаза / блок) | Gate B | Статус | Что остаётся |
|---|---|---|---|---|---|
| R1 | `ILearningDynamicGraph` строит DPO на уровне `UnitId` / `ILogicalUnit` внутри машины. | Ф3 (строитель), Ф4.3.1 узел (`GraphNodePreferenceState`), Ф4.3.2 ребро как источник path/order metadata, Ф5.2 измерение, Ф7 петля; блоки 07, 10, 01, 02 | `TrainableGraphDpoBuilderUnit` выбирает unit-level pair material; train-ready `GraphDpoPair` is the exact active/Claude plan-07 14-field canon with `Level="unit"` and unit id in `NodeOrEdgeId`. Whole graph A vs whole graph B запрещён как train-ready DPO; graph path/order metadata remains provenance/triad/trace context, not extra pair fields. | covered_in_plan_execution_and_nonblocking_v1_defaults_pending_proof | Pass43 supersedes the old B9/B10/B12/B15/B18/B21 review queue: those slices are resolved in active plan with execution proof pending or nonblocking v1 defaults. B18 is v1 common-only with adaptive deferred unless Luba later expands scope. B21: the 20/40 recon/knowledge-library floor stays BINDING. The BUILT `20/20`+`40/40` libraries (parent-verified) close the artifact/count side, but downstream must NOT treat them as optional and must NOT downgrade required recon DEPTH to "optional/future" without an explicit current Luba quote (`blocked-until-done-or-Luba-approves-narrower`, never `optional-unless-Luba-reopens`); see F0 residual `Q-B21-FLOOR` (`LUBA_PRODUCT_DECISION_RESIDUAL_OR_REQUIRED_LIBRARY_WORK`). Remaining work is future implementation/runtime pair proof, not an implementable unit-level DPO design gap. |
| R2 | Авто-построение DPO из несуществующих данных (генератор пар, не ручной набор). | Ф3, Ф7; блоки 07, 06, 09 | `BuildPairAsync` на фикстуре даёт валидный `GraphDpoPair`; 5 форматов → 1 канон (integration); пустые батчи невалидны для обучения | covered | — |
| R3 | `ILearningDynamicGraph` = L5 `ILearningMachine<DynamicGraphDomain>`, не 6-й уровень. | Ф1.3.3, Ф4; блоки 04, 00, 05 | счётчик новых архитектурных уровней == 0; L5-маппинг с `00.Core/LearningMachineContracts.cs:208`; A-LM-12 | covered | — |
| R4 | Динамическая топология строится обученным графом, а не захардкоженной матрицей смежности. | Ф1, Ф4; блоки 04, 03 | V1 не вводит матрицу смежности как первичное представление; используется `IDynamicGraphNode`/`IDynamicGraphEdge` + `BuildDynamicTopology(signal,budget)` с budget-proof и mutation-evidence | covered | — |
| R5 | Финал ребра обучаемый и не скрытый детерминированный роутинг. | Ф8 (cutover), Ф4.3.2; блоки 12, 02, 03, 04 | `IDynamicGraphEdge.AuthorityUnitId` non-null; `AuthorityUnitLevel=edge`; `DeterministicFinalAuthority=false` после cutover; baseline только как eval/ablation (`BaselineRole`); candidate index/position, original/file order, source path/id, exact prompt, correlation id, and merge order are forbidden as authority/training signals; path/order claim requires `GraphOrderResolverOffAblation_DropsOrBlocksOrderClaim`, where resolver-off deterministic topology/Kahn/closure baseline must drop below the locked ablation threshold or block learned-order authority. | covered | Pass39 makes resolver-off ablation and file-order/correlation-id leakage explicit as future test proof; no resolver ablation/test/runtime proof has run. |
| R6 | Две грани. **(1) Рекон-библиотека / floor 20-40** (мастер §F.5; recon-gate `reconnaissance-gate-SKILL.md` §5A; plan-review «High-risk architecture recon library review gate» строки 174-203): для ILearningMachine/DynamicGraph-плана обязателен фон `external/source corpus ≥20` + `llm_mvp_core docs+code ≥40` + Knowledge Library с per-row `coverage_critic`/`source_disjoint_evidence`. **(2) Честность счёта ревью-проходов**: claim о N проходах обязан отличать реальные returned-пакеты от строк-счётчиков. | (1) мастер §F.5 + библиотеки `codex-knowledge-library-*`; карта гейтов §8 (Recon Library Gate). (2) доска §D; Ф11.2 | (1) на диске **23** `codex-recon-mvp-*` + **40** `codex-recon-core-*` + **2** `codex-knowledge-library-*` (фактическая per-row схема: `source_category`/`provenance_status`/`teacher_access_mode`/`legal_verdict`/`source_hash_or_ref`/`implementation_use`/`gap_owner` — `codex-knowledge-library-llm-mvp-core.md:43`; **coverage_critic в обоих файлах ОТСУТСТВУЕТ**, как и `why_to_use`/`when_to_use`/`evidence_line_range`/`decision_supported`/`source_disjoint_evidence`/`confidence_effect`/`held_out_roots_or_sources`, поэтому даже как codex-evidence эти библиотеки **не** удовлетворяют per-row схему Recon Library Gate, plan-review-SKILL.md:199-203) — это **codex-owned evidence-only**. **(Историческое/pre-build, до постройки active-codex библиотек:** «в claude-namespace рекон-файлов/Knowledge Library = **0**; `passes_unproven=true`»**.)** **Текущий вердикт (recon-library floor артефакт/счётная грань MET):** в active-codex namespace теперь ПОСТРОЕНЫ и parent-verified две библиотеки — `active-codex-execution-knowledge-library-llm-mvp-20-pass.md` (`accepted_focused_pass_records: 20/20`, `artifact_status: R_B_LLM_MVP_20_PASS_TARGET_COMPLETE`, passes 01/20..20/20) и `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md` (`accepted_focused_pass_records: 40/40`, `artifact_status: R_B_CORE_40_PASS_TARGET_COMPLETE__PLAN_FAMILY_STILL_PLAN_NOT_READY`, passes 01/40..40/40). Эти `20/20`+`40/40` библиотеки закрывают артефакт/счётную грань recon-library floor в claude-namespace (правило `N-pass library is a required built artifact`); `passes_unproven=true` относилось к состоянию ДО их постройки и здесь superseded. Инвентарь файлов допустим как evidence; «1 файл = 1 проход» по-прежнему запрещено как count-инфляция, но принятые per-pass записи в этих двух артефактах — это независимо инспектируемые pass-записи, а не голая инвентаризация. Обе библиотеки сохраняют `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE` и `implementation_authorization: NO`; требуемая recon-ГЛУБИНА не понижается до «optional/future» без явной текущей цитаты Luba (`blocked-until-done-or-Luba-approves-narrower`, см. F0 `Q-B21-FLOOR`). (2) считаются только пакеты с id ревьюера, before/after ref, findings, fix, recheck; ledger-строки ≠ циклы; дубликаты уведомлений не считаются. | partial (грань 1 floor-артефакт MET постройкой библиотек; грань 2 review-depth не закрыта) | (1) Floor 20-40 **закрыт постройкой active-codex библиотек** `20/20`+`40/40` (parent-verified; `active-codex-execution-knowledge-library-llm-mvp-20-pass.md:116` `accepted_focused_pass_records: 20/20`; `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md:195` `accepted_focused_pass_records: 40/40`; overlay-заголовок этого файла строка 5 признаёт их как evidence-library closure). **Историческое (pre-build):** ранее эта грань опиралась только на **бенчмарк codex как evidence** (`codex-knowledge-library-llm-mvp.md:319` «23 рекон-файла»; `codex-knowledge-library-llm-mvp-core.md:7` «40 рекон-отчётов»; мастер §F.5 строки 786-796), а claude-канон был `evidence_only / passes_unproven=true` (0 рекон-файлов / 0 Knowledge Library). **Что остаётся:** закрытие floor — это evidence-library closure, НЕ readiness / НЕ implementation authorization / НЕ product proof; recon-глубина остаётся BINDING (см. F0 `Q-B21-FLOOR`). (2) Глубина реального счёта ревью НЕ закрыта: см. R20 (`partial`, benchmark-disputed, claude `real_pass=0`); более широкая гигиена claim-ов остаётся процессным правилом. |
| R7 | Авто-DPO петля имеет no-signal стоп и не делает случайного промоушена. | Ф7; блоки 09, 07, 08 | `NO_AUTO_DPO_SIGNAL=true` ⇒ принятых пар после всех гейтов == 0; `ShouldStopResult.stop_reason=no_signal`; обучения/промоушена нет; прежний checkpoint сохранён | covered | — |
| R8 | `GraphDpoPair` carries the train-ready triad through the exact 14-field canon plus owner-side provenance/triad records; authority/origin/legal diagnostics are not flat pair fields. | Ф2.2; блок 07/08/09 | `DpoTrainReady=true` требует тройного гейта (`legal_accept && split_accept && authority_provenance_accept`); `train_ready_without_triad_count==0`; the pair has nested `TeacherProvenance`, `SourceIds`, `HeldOut`, `CandidateOrderInvariant`, and `ExcludedLeakageSignals`. Manifest/projection proof records presence/readback (`provenance_field_presence_rate==1.0`, `row_sidecar_ref_match_rate==1.0`) without adding fields to the pair. Плоские имена `chosen/rejected_authority_unit_id`, `chosen/rejected_origin`, `legal_verdict`, `source_category`, path/order refs — **не** поля канон-пары: they live in `TriadVerdict`, provenance/trace, winner metadata, or the 20-field `TeacherProvenance` owner record. | partial | Pass37 repairs plan-text alignment; runtime/data-manifest readback, export/import, and implementation tests remain future evidence. |
| R9 | Per-document авторство и трассируемость ревью нельзя раздувать. | мастер (сборка); доска §D | часть документов восстановлены/интегрированы родителем; нет ложного claim о per-document авторстве суб-агента | partial | Честная пометка восстановления стоит; это процессная честность, а не код-гейт. Согласовано с R6(2)/R20: семейство «честность счёта ревью» закрыто НЕ полностью (R20 = `partial`, benchmark-disputed, claude `real_pass=0`). |
| R10 | Async/fan-out/fan-in/many-to-many комбинации имеют явные гейты. | Ф6; блоки 06, 03, 05, 02 | для `budget.Max=4` стартует ровно 4 async (`Task.WhenAll`); public output is `IQueryable<GraphNodeCandidate>`; queryable-to-batch materialization preserves metadata into `IReadOnlyList<GraphNodeCandidate>`; `NO_OBJECT_OUTPUT` canonical shape; typed fields and B12/B6 handoff metadata present; победитель != немаркированная формула (ablation); из N>=2 строятся unit-level candidate refs/`GraphDpoPair[]` with overlap<0.60, order-invariant; merge-семантика без скрытого победителя | covered | PASS10 adds plan-level B-8/B-9 acceptance rows; implementation/tests/runtime proof remain future evidence. |
| R11 | Две грани (анти-путаница, как R-19 vs Luba-R19). **(a) Стабильность нумерации фаз** (= мастер `R-19` Luba execution-visibility). **(b) Референс-на-каждый-этап / Source-to-Plan на все 64 подшага** (прямой субъект claude-plan-14 §0/§3.2/D7, после RR4/Q41/PASS26). | (a) вся доска §0.1; Ф0..Ф11. (b) блок 14 §3.2 (владелец канона `GraphReferenceMap`); полный перечень baseline-64 — доска §B (single source) | (a) baseline 12 фаз / 27 подфаз / 64 подшага; каждый `Ф<P>.<SP>.<SS>` уникален без дыр; Ф4.3.1 unit / Ф4.3.2 edge / Ф4.3.3 split; Ф10.3.1..Ф10.3.5 активный пилот `language-translate`. (b) карта Source-to-Plan обязана дать строку для всех 64 подшагов (R11 «на КАЖДЫЙ»). | (a) covered; (b) covered_as_inventory_evidence_only | (a) — нумерация стабильна. (b) Pass30/Pass39 supersedes the old partial row: plan-14 §3.2 has 64 canonical Source-to-Plan rows with no noncanonical `| Ф` offenders in Pass31 readback. This remains inventory/source-map evidence only, not readiness, B-22 closure, implementation authorization, code proof, test proof, or execution proof. |
| R12 | codex-файлы используют codex-префикс; claude-авторские codex-подобные файлы — evidence-only исключения. | мастер §F; данный §2/§8 | имя каждого собственного файла начинается с префикса; индекс перечисляет claude-авторские evidence-only исключения отдельно | covered | — |
| R13 | Все source-блоки плана P-00..P-14 представлены. | мастер §C/§F индекс | индекс и source-map покрывают все план-блоки P-00..P-14; восстановленные support-доки повторяют свой канон | covered | — |
| R14 | Легальный provenance-паритет распространяется на train-ready проекции. | Ф2.1 (владелец гейта), проекции Ф6/Ф7/Ф9/Ф10; блок 08 | `TeacherProvenance` 20 полей (13+5+2 RR6 `service_mode`/`grounding_used`) is the nested active/Claude block-08 canon inside the 14-field `GraphDpoPair`; `provenance_field_presence_rate==1.0` and `row_sidecar_ref_match_rate==1.0` are prerequisites for `DpoTrainReady`; `GraphDpoProvenanceWriter` stores manifest/projection proof consistently without adding fields to the pair; projections to `LearningTrainingItem` keep implementation refs in manifest/training-data context, not by changing 00.Core. | partial | Текстовый паритет выровнен Pass37; runtime/data-manifest readback, export/import, and implementation tests remain future evidence. |
| R15 | Доп. референс-домены явные. | Ф10.3 активный пилот `language-translate` + навигация остальных R15-доменов | Q41=Вариант B RESOLVED 2026-06-30: ровно 1 пилот `language-translate`, Ф10.3.1..Ф10.3.5 входят в baseline-64 со статусом `remaining`; `small-model`, `local-wiki`, `extensions`, `documentation-learning`, `session-start` — navigation/reference-only, не подшаги текущей доски. Non-pilot/reference/client domains are not E2E executable in v1 until each has its own seed/data, held-out scorer/gold, legal/provenance, activation substeps, and lifecycle bundle. | partial | Q41 не является открытым вопросом. Остаются честные plan coverage blockers по source-map and activation-bundle proof; recon files, generic templates, and future gates are not E2E readiness. |
| R16 | Host route scope is resolved by the original task; Q6/Q38 defaults are planning-locked by attachment 483d/latest Claude, with Pass35 active storage repair. | Ф4/Ф10 (SPLIT); блок 04, 10; original-task lines 330-343; attachment 483d/latest details; Blocker 2 seam table/R22; Blocker 3 same-scenario design/R23 | `LegScope` и source-маппинг существуют as later activation/manifest blueprint; v1 active training route is common-only. `DynamicGraphMachinePair` does not make personal/adaptive v1-active. Original task names `xiigen-platform` for figma API, FigmaToCode/CanvaToCode callers, and `xiigen-platform` + `xiigen-flows` for better-output-to-DPO API; `FIGMA_CANVA_DESIGN_TO_HTMLCSS` seam contract specifies request/response fields, mock response, unit-test intent, and quarantine/provenance rule; client outputs remain adaptive/eval/quarantine and not common train-ready without positive proof and Luba approval. `SCN-R16-DESIGN-TO-HTMLCSS` specifies Path A command/fixture/evidence files, Path B host UI actions/screenshots, and equality key `scenarioId + sourceClient + designSourceRef + designSnapshotHash + correlationId + tenantScope`. | partial | Current verdict for blockers 2 and 3: external seam contract and same-scenario dual-path design are CLOSED_IN_PLAN; remaining R16 work is implementation/e2e proof, real source/file evidence, future approved mock-test execution, and future Path A/Path B evidence capture, not an unresolved seam or same-scenario design blocker. |
| R17 | Better-output origin is resolved by the original task; output-license default is planning-locked by attachment 483d. | Ф7; блоки 07 (вход `GeneratedUiOutputFeedbackEvent`), 08; original-task lines 20, 76-84, 97-104; attachment 483d; Blocker 2 seam table/R22; Blocker 3 same-scenario design/R23 | мост FLOW-38 существует as plan contract; Q37 origin is self DynamicGraph or legal hosted open-weight teacher with provenance/terms; generated client outputs stay adaptive/quarantine by default; common use only after output-license clearance, redaction, per-client terms snapshot, and Luba approval; input via `GeneratedUiOutputFeedbackEvent`; `BETTER_OUTPUT_FEEDBACK_TO_DPO` seam contract specifies fields/mock/unit-test intent; `SCN-R17-BETTER-OUTPUT-DPO` specifies Path A command/fixture/evidence files, Path B host feedback UI actions/screenshots, and equality key `scenarioId + eventId + promptHash + correlationId + tenantScope + chosenOutputHash + rejectedOutputHash`. | partial | Current verdict for blockers 2 and 3: exact FLOW-38/core feedback contract and same-scenario dual-path design are CLOSED_IN_PLAN; remaining R17 work is concrete entrypoint implementation proof, provider/terms/license proof where required, future approved mock-test execution, and future Path A/Path B evidence capture. |
| R18 | Правило codex-префикса задокументировано с исключениями. | мастер §F; данный §8 | перечислены точные evidence-only исключения (claude-авторские codex-файлы) и собственный вспомогательный артефакт | covered | — |
| R19 | Вопросы только в финальных секциях. | данный §9; мастер §E | открытые вопросы по тексту — только как decision-id (Q-ссылки); сами вопросы — в финальной секции | covered | — |
| R20 | Прогнать до 50 реальных циклов review/fix до claim полной глубины ревью. | доска §D; Ф11.2 | `covered_for_R20_protocol_definition` (протокол 50-циклов определён + бенчмарк-пакеты получены как evidence). НЕ `covered_for_proven_cycles`: реальные доказанные циклы требуют N независимо-инспектируемых пакетов (id ревьюера, before/after ref, findings, fix, recheck); ledger/индекс-строки ≠ циклы. | **partial** (evidence-only, benchmark-disputed; claude `real_pass=0`) | Доказательная картина **противоречива и оспорена** (синхрон с мастер §F.6 строки 809-831). **Два codex-файла конфликтуют об одном и том же 50-цикловом усилии**: (1) `codex-GPT55-50-CYCLE-REVIEW-LEDGER.md` повторяет «50/50 returned packets»; (2) `codex-50-cycle-plan-review-supplement.md:17-36` фиксирует дословный челлендж Luba «no fucking way you made 50 cycles with a sub agent», признание «The challenge is correct» и `subagent_cycles_completed=0` / `independent_review_fix_cycles_proven=0`. Supplement — **более честный** артефакт; ledger переоценивает. Для claude-* deliverables `real_review_fix_pass_count = 0` (claude-пакетов не существует). Открытый вопрос «какой 50-цикловый record канонический» — в §9 Q7. При реализации продукта действует отдельный 10-review gate Ф11.2.1. |

---

## 8. Карта гейтов (общие доказательственные требования)

| Область | Требуемые доказательства |
|---|---|
| Split-доказательство | `train_source_roots`, `eval_source_roots`, `source_hashes`, `cross_split_near_duplicate_count`, `simhash_or_minhash`, `near_duplicate_threshold`. |
| Lifecycle-доказательство | `process_id`, `restart_id`, `entrypoint_trace_id`, `no_replay_cache_proof`, хеши загруженного checkpoint/manifest, `manifest_schema_version`, `source_cutoff`, `path_id_strategy`, `rollback_ref`. |
| Authority ребра | `AuthorityUnitId`, `AuthorityUnitLevel=edge`, `ParallelEligible`, `DeterministicFinalAuthority=false`, `BaselineUnitId`, `BaselineRole`, доказательства leakage/source/cutover/ablation. |
| Легальный provenance | `TeacherProvenance` 20 полей (13+5+2 RR6 `service_mode`/`grounding_used`); `SourceCategory` ∈ {local, human, open_weight, hosted, deterministic_baseline, self_judge, client_unverified, third_party}; вердикт ∈ {ACCEPT, QUARANTINE, BLOCK, DATASET_REBUILD_REQUIRED, BLOCK_FOR_COMMON, EVAL_ONLY} (6 значений, канон §3 / план-08 §3.3). |
| Честность ревью | считать только returned-пакеты с id агента, evidence по целевой библиотеке, findings или no-fix обоснованием, и решением родителя интегрировать/отложить. **Текущий статус (R20): `partial`, benchmark-disputed, claude `real_pass=0`** — ledger-строки ≠ доказанные циклы (см. R20 §7 и §9 Q7). |
| **Recon Library Gate (R6 грань 1)** | Floor для ILearningMachine/DynamicGraph-плана (recon-gate `reconnaissance-gate-SKILL.md` §5A; plan-review строки 174-203; мастер §F.5): `external/source corpus passes ≥ 20` + `llm_mvp_core docs+code passes ≥ 40` + Knowledge Library с per-row полями `path/type/summary/why_to_use/when_to_use/relevance_to_target_plan/document_locations/implementation_locations/evidence_line_range/decision_supported/source_disjoint_evidence/confidence_effect/held_out_roots_or_sources/status/coverage_critic`. **Состояние:** на диске **23** `codex-recon-mvp-*` + **40** `codex-recon-core-*` + **2** `codex-knowledge-library-*` — **codex-owned evidence**; floor закрывается бенчмарком codex как evidence. **Фактическая per-row схема обеих библиотек:** `source_category`/`provenance_status`/`teacher_access_mode`/`legal_verdict`/`source_hash_or_ref`/`implementation_use`/`gap_owner` (`codex-knowledge-library-llm-mvp-core.md:43,55,67,76,93,101,111`; то же в `-llm-mvp.md`). То есть `source_category` и `legal_verdict` ЕСТЬ, но **`coverage_critic` ОТСУТСТВУЕТ** (Grep count = 0 в обоих файлах), как и `why_to_use`/`when_to_use`/`relevance_to_target_plan`/`document_locations`/`implementation_locations`/`evidence_line_range`/`decision_supported`/`source_disjoint_evidence`/`confidence_effect`/`held_out_roots_or_sources`. **Честный вывод по codex-библиотекам:** даже как codex-owned evidence эти библиотеки **не** удовлетворяют per-row схему этого гейта (plan-review-SKILL.md:199-203 hard-fail при отсутствии `coverage_critic`; `reconnaissance-gate-SKILL.md` перечисляет его среди обязательных полей) — это усиливает, а не ослабляет честную позицию claude. **Историческое/pre-build:** «В claude-каноне рекон-файлов/Knowledge Library = 0; `passes_unproven=true`» относилось к состоянию ДО постройки active-codex библиотек. **Текущий вердикт (floor-артефакт грань MET):** в active-codex namespace ПОСТРОЕНЫ и parent-verified `active-codex-execution-knowledge-library-llm-mvp-20-pass.md` (`accepted_focused_pass_records: 20/20`, строка 116) и `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md` (`accepted_focused_pass_records: 40/40`, строка 195). Эти `20/20`+`40/40` закрывают артефакт/счётную грань floor `external≥20`+`core≥40` в claude-namespace (`N-pass library is a required built artifact`); `passes_unproven=true` здесь superseded. Обе библиотеки сохраняют `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE` и `implementation_authorization: NO`; закрытие floor — это evidence-library closure, НЕ readiness / НЕ implementation authorization, а recon-ГЛУБИНА остаётся BINDING без явной цитаты Luba (F0 `Q-B21-FLOOR`). Источники: `active-codex-execution-knowledge-library-llm-mvp-20-pass.md:116`; `active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md:195`; overlay-заголовок этого файла строка 5; исторический pre-build бенчмарк — `codex-knowledge-library-llm-mvp.md:319`, `codex-knowledge-library-llm-mvp-core.md:7,43`, мастер §F.5 строки 786-796. |
| Тест-стратегия (FC-26) | по каждой ветке Part A/Part B: unit, integration, e2e/host, QA-автоматизация, negative, **untrained-honesty** и **trained-acceptance** раздельно (две PASS-стадии OP-56/OP-57), regression. |

> Граница evidence (улучшение честности, против бенчмарка). Все split-, lifecycle-,
> provenance-, recon-library- и review-доказательства выше существуют как **план-гейты и
> evidence-only факты из codex-файлов**. Эта матрица НЕ утверждает, что любой из них
> исполнен в рантайме, обучен на held-out, или прошёл ablation на реальном коде. В
> частности, claim «50/50 циклов» **оспорен** (см. R20 §7 / §9 Q7) и не печатается как
> доказанный; recon-floor 20-40 закрыт **бенчмарком codex как evidence**, а в
> claude-каноне = `passes_unproven=true`. Реализация запрещена текущим заданием; статус
> каждого кодо-уровневого доказательства — «гейт определён», а не «гейт пройден».

---

## 9. Только финальные вопросы Luba

> Эта матрица больше не спрашивает Luba о значениях, уже закрытых источниками. Q3, Q6/B15,
> Q11, Q14, Q30 и Q38 являются planning locks из attachment 483d/Pass02 plus latest Claude; Q6/B15 now requires v1
> common-only storage with `DynamicGraphMachinePair : ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` as later activation blueprint, not a v1-active pair. Q37 и Q-host являются
> resolved/partial implementation gates, не текущими Luba-only вопросами.

1. **Q41 Variant B — historical/pre-PASS26 resolved item, NOT open.** Claude board-owner
   canon says Q41=Вариант B RESOLVED 2026-06-30 for exactly one pilot `language-translate`;
   Ф10.3.1..Ф10.3.5 are baseline-64 rows with status `remaining`, blocked count 0.
2. **(R11 грань b)** Достаточно ли, что **полная** 64-строчная карта Source-to-Plan живёт в
   доске §B как single source, а блок-14 §3.2 несёт только опорные строки (тогда R11(b) можно
   считать `covered` инвариантом полноты), — или требуется развернуть все 64 строки прямо в
   блоке-14, устранив зависимость от доски §B / `codex-plan-14 §11.0`?

R20/50-cycle is not a Luba value question: until 50 independently inspectable returned
review/fix packets exist, the canonical count for review/fix cycles is 0. Ledger rows,
tables, summaries, and self-review remain ledger-only evidence.
---
---

# Active Codex Execution Addendum - DPO Legal Learning

## Current Live Authority And Evidence Boundary

This file is an active execution copy of the Claude baseline artifact. The original `claude-*` file remains unchanged. Claude remains the baseline because it is stronger by Luba criteria: human clarity, readable architecture, architectural reliability, evidence honesty, requirement fidelity, execution-control quality, and low overclaim risk. `codex-*` files are delta/source-map evidence only, not the baseline and not product-readiness proof.

Current active task source is Luba's latest live unquoted instruction for the DPO Legal Learning active plan chain. Prior sub-agent packets are evidence to verify, not live commands. The Heisenberg/no-branches/git-realignment branch of work is explicitly NOT THE TASK and must not be revived as a blocker, cleanup scope, branch operation, commit/push reason, or planning-repo git task.

Codex delta inputs retained for this addendum:

- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-EXECUTION-CONTROL-BOARD.md`: matching branch/source-map notes for `DPOLegalLearning`; not repo authority and not a source for expanding scope.
- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-REQUIREMENTS-COVERAGE-MATRIX.md`: matching notes for Claude latest-updated branch authority and `DPOLegalLearning` working branch/work name; not independent authority.
- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`: crosswalk and plan-only branch authority wording.

Do not claim Codex 50/50 cycles as proven product readiness. Ledger rows, checklist rows, generated tables, and summaries are not completed cycles unless independently inspectable review/fix evidence records exist for the exact claim.

## Active Requirement Count Strategy

The active execution requirement count strategy is:

- original Claude `R1-R20` / master `R-01..R-20` baseline remains the quality and honesty baseline;
- Claude baseline, with matching non-contradictory Codex delta evidence, defines `R21`: latest-updated implementation source-branch authority plus the `DPOLegalLearning` work name rule for implementation repo copies/workdirs under `C:\Projects`;
- current Luba execution requirements are live addendum requirements and must be checked by pre-implementation, pre-commit, pre-push, and before-stop gates;
- the prior no-branch planning-repo policy packet is quarantined evidence only and is not an active task, stop gate, branch-cleanup requirement, or permission to touch planning-repo git state.

This strategy is an execution-control strategy, not product-readiness proof.

## R16/R17 Current Supersession - Blocker 2

Any earlier row text in this matrix saying strict plan readiness still blocks on unresolved external API seam design, FLOW-38/FLOW-31 input/output contract, or internal seam design for R16/R17 is historical/pre-blocker-2-repair evidence. Current verdict after the blocker-2 repair: external seam contract, mock response shape, unit-test intent, legal/provenance rule, and replacement-ledger rule are `CLOSED_IN_PLAN` by the Blocker 2 table in `active-codex-execution-plan-readiness-repair-20260629.md` and R22 below. Remaining R16/R17 work is implementation/e2e proof, concrete entrypoint/file evidence, future approved mock-test execution, and provider/terms/license proof where required for real training/common use.

## R21 - Source Authority, Working Directory, And DPOLegalLearning

For each participating repository/source role before implementation:

1. Identify the source authority as the most advanced/latest-updated branch in that repository at the time work starts, using read-only git commands.
2. Treat the source authority branch as read-only authority, not as the implementation work branch.
3. Because `C:\Project` does not exist and `C:\Projects` exists in this environment, use `C:\Projects` as the implementation root. Create or use a separate working directory for the work name `DPOLegalLearning`, with repo copies under `C:\Projects\DPOLegalLearning\<repo-name>` unless an existing active plan row names a more specific safe `C:\Projects` path.
4. The controlling repo-by-repo authority/source-to-plan table is `active-codex-execution-02-source-inventory-and-reference-map.md` R21.1. During future implementation, record the concrete repo path or URL, branch-list source, selected latest-updated branch, selected branch timestamp/ref evidence, divergence note, separate `C:\Projects` working directory, and work name `DPOLegalLearning`.
5. Participating Claude-baseline route/source identifiers are `llm_mvp_core` (core implementation), `xiigen-platform-dot-net-angular` (public API facade for R16/R17), `xiigen-flows-dot-net-angular` (FLOW-31/FLOW-38 internal seam), and client/caller labels `FigmaToCode-design-gen` + `CanvaToCode` (R16 html/css clients). FigmaToCode/CanvaToCode are not implementation-repository authority for the platform host unless a later exact source proves that. Codex may be used only as matching delta/source-map evidence. This list must not be expanded from discovered/copied workdirs unless Luba explicitly expands repo scope.
6. Do not switch, create, delete, rebase, merge, cherry-pick, push, or open PRs in `C:\Users\lubao\Documents\xiigen`. That planning location may be read only as an evidence source when needed; no git realignment or branch cleanup is part of this chain.

## Execution Actor And Stop Gate

Future implementation, if later authorized, must run through bounded sub-agent work packets, requested model/mode `gpt-5.5` / `xhigh` (user wording: gpt 5.5 extra high). Parent orchestrates and no unbounded executor is allowed. Each packet must name exact scope, allowed files/repos, max time, required report packet, accepted evidence, and forbidden actions. A spawned bounded sub-agent is already the delegated execution actor for its allowed scope and must not invoke nested Codex/Claude/CLI agents unless the work order explicitly asks for nested delegation. Ledger rows, tables, summaries, and self-review are not fake cycles and cannot be reported as sub-agent passes.

Stop is allowed only when the product is fully ready, or when a true external blocker remains after mitigation/discovery and decision-prep. Before any stop, the worker must check whether the active plan is complete. If incomplete, the worker must identify the loophole that would cause premature stop, repair the governing plan/gate/skill text inside the allowed scope, and continue the nearest safe authorized work. NOT READY, NEEDS_FIX, review findings, partial evidence, and missing verification are bounded work queues, not completion claims.

## Pre-Commit And Pre-Push Merge/PR Conflict Gate

Before every commit and before every push during implementation:

1. Re-read the current source authority branch for each participating implementation repo because the source branch may advance in parallel.
2. Validate that merge/PR into the original source authority branch copied from has no conflicts.
3. Record the exact command or tool evidence used for the no-conflict result.
4. If conflicts exist, stop the commit/push path, repair or prepare a decision packet, then re-check against the current source authority.
5. Do not create PRs and do not merge unless Luba gives a separate future instruction.

This gate does not authorize any branch operation, commit, push, merge, PR, or cleanup in `C:\Users\lubao\Documents\xiigen`.

## R22 - Blocker 2 External Seam Coverage Addendum

| coverage_id | covered seam/requirement | mapped artifacts | current status | remaining non-design evidence |
|---|---|---|---|---|
| R22-01 | `FIGMA_CANVA_DESIGN_TO_HTMLCSS` platform/FLOW-31/client boundary, request/response fields, deterministic html/css mock, unit-test intent, and quarantine/provenance rule. | Readiness Blocker 2 table; source inventory R22; plan-13 test addendum; board F10 supersession. | CLOSED_IN_PLAN | Future approved implementation/e2e proof and mock unit-test execution. |
| R22-02 | `BETTER_OUTPUT_FEEDBACK_TO_DPO` platform/FLOW-38/core boundary, event fields, quarantine mock response, unit-test intent, and C16 client-origin rule. | Readiness Blocker 2 table; source inventory R22; matrix R17. | CLOSED_IN_PLAN | Future concrete entrypoint/file evidence and mock unit-test execution. |
| R22-03 | `LEGAL_TEACHER_JUDGE_PROVIDER` external provider adapter boundary, teacher/judge mock outcomes, legal provenance and closed-provider restrictions. | Readiness Blocker 2 table; blocker-4 provider/provenance proof contract; legal model policy; plans 08/09/10. | CLOSED_IN_PLAN_EVIDENCE_GATE | Future provider terms/license proof before real training/common use; missing proof routes to quarantine/eval-only/block/rebuild-required. |
| R22-04 | `PROVIDER_TERMS_LICENSE_METADATA` documentation/proof seam, pending metadata mock, and test intent blocking training/common use when proof is absent. | Readiness Blocker 2 table; blocker-4 ProviderProof/LicenseProof fields; legal policy. | CLOSED_IN_PLAN_EVIDENCE_GATE | Future terms/license evidence retrieval or manually supplied snapshot; pending/mock proof is not train-ready/common-ready proof. |
| R22-05 | Later `codex-external-calls-mock.md` replacement ledger fields and no-current-create rule. | Readiness Blocker 2 table; gates file; authority ledger B2-006. | RULE_DEFINED_FILE_CREATION_NOT_AUTHORIZED_NOW | Future approved product-readiness ledger creation/update only. |
| R22-06 | PASS08 R-OT-08 recheck: every external API/resource seam must keep seam/interface, request fields, mock expected response, unit-test assertion, legal/provenance failure behavior, and replacement plan target together. | Master Pass08 overlay; source inventory R22.3; full-task gate Pass08; plan-08 teacher/judge seam; plan-13 mock-test addenda; plan-14 R16/R17 seams. | PLAN_CONTRACT_CONFIRMED_EVIDENCE_NOT_IMPLEMENTED | No real external call, provider lookup, product implementation, test/build/runtime probe, or `codex-external-calls-mock.md` creation occurred in Pass 08. |

## R23 - Blocker 3 Same-Scenario Dual-Path Coverage Addendum

Any earlier row text in this matrix saying strict plan readiness still blocks on unresolved same-scenario dual-path host/extension design is historical/pre-blocker-3-repair evidence. Current verdict after the blocker-3 repair: Path A/Path B route definitions, fixtures, expected literals, evidence filenames, exact Russian UI actions, screenshot filenames, same-scenario equality keys, blocker handling, no-secrets rule, and partial-evidence downgrade rules are `CLOSED_IN_PLAN` by `active-codex-execution-plan-15-same-scenario-dual-path-proof-design.md` and R23 in the source inventory. Remaining work is future approved execution evidence, not current design selection.

| coverage_id | covered scenario/requirement | mapped artifacts | current status | remaining non-design evidence |
|---|---|---|---|---|
| R23-01 | `SCN-R16-DESIGN-TO-HTMLCSS` same design snapshot through Path A automated/API/core route and Path B FigmaToCode/CanvaToCode host UI route. | plan-15 SCN-R16; source inventory R23; plan-14 R16; test strategy future addendum. | CLOSED_IN_PLAN | Future approved Path A test execution, Path B host/UI execution, screenshots, and same-scenario join file. |
| R23-02 | `SCN-R17-BETTER-OUTPUT-DPO` same feedback event through Path A FLOW-38/core legal gate and Path B Xiigen platform/client feedback UI route. | plan-15 SCN-R17; source inventory R23; plan-14 R17/FLOW-38; test strategy future addendum. | CLOSED_IN_PLAN | Future approved Path A test execution, Path B host/UI execution, screenshots, and same-scenario join file. |
| R23-03 | Historical Claude/Cursor/Codex host/extension packets are evidence-only unless re-run/re-bound to R16/R17 equality tuple. | plan-15 historical boundary; attachment 429f defect source. | EVIDENCE_ONLY_NOT_PRODUCT_READINESS | Future re-run/re-bind if someone wants to use old packets as current product evidence. |

## External APIs And Mock-Only Technical Debt Gate

Future implementation, if later authorized, must handle every external API/resource path through an explicit seam/interface/adapter boundary, mock the expected external response instead of making the real external call, and add a unit test that asserts exactly the expected mock response/result. Record replacement of the mock with the real external resource call only in:

`C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`

This mock replacement record is the only allowed technical debt. No other technical debt may be introduced under this active plan. Mock-only evidence must not be reported as real external integration readiness.

## Current Worker Execution Boundary

Current 2026-06-29 planning-repair boundary: Luba's latest unquoted instruction says `План не готов к выполнению - нужно починить`. This worker is authorized only to repair active plan files in this folder. Product implementation, implementation workdir setup/copying, repo inventory, tests/builds, git actions, commits, pushes, PRs, merges, and mock-debt file edits are NOT AUTHORIZED in this task. Any earlier wording that implied starting implementation after coherence is superseded until a later plan-readiness gate truly passes and Luba gives fresh implementation authority.

Source `claude-*` and `codex-*` files remain read-only. In this planning-repair task, PRs, merges, commits, pushes, branch switches, workdir copying, and implementation repo edits are forbidden. Future implementation commits/pushes may be considered only in separate implementation workdirs after plan readiness, a bounded sub-agent work order, and the mergeability/no-conflict gate pass; PRs and merges still require separate future Luba instruction.
## PASS27 Original-Task Coverage And Latest Baseline Reconciliation

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. See `active-codex-execution-18-pass27-original-task-coverage-reconciliation.md` for the controlling latest-baseline reconciliation context. Current active-codex canon after latest-baseline reconciliation plus Pass30 Ф9 consolidated test-strategy source-map repair: 12 phases / 27 subphases / 64 substeps; `TeacherProvenance` 20 fields (13 base + 5 NEW + 2 RR6 `service_mode`/`grounding_used`); exactly one active pilot `language-translate`; Source-to-Plan inventory rows are 64/64 accepted as source-map inventory evidence only; hosted Qwen3/Together train use remains blocked until provider terms proof; held-out scorer defaults to `RuntimeProductAccuracy` plus per-domain scorer (`language-translate`: exact-match + char-accuracy). Any older unqualified 12/26/59, 18-field, 8/64 (частично; historical/pre-PASS27 corrected from 8/59), 28/36, 31/33, 34/30, 41/23, 46/18, 50/14, 54/10, 58/6, incomplete-source-inventory, or narrower ready wording in this file is historical/pre-0aa evidence only.

## Sequential Pass 05 R-OT-05 Matrix Note

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

R-OT-05 coverage after Pass 05 is `PARTIAL_PLAN_CONTRACT_REPAIRED_EVIDENCE_NOT_COMPLETE`.

| requirement slice | active coverage | remaining blocker |
|---|---|---|
| Planning docs in `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build` | Covered as active planning-library contract; `claude-*` remains Claude baseline authority, `codex-*` remains evidence-only, and `active-codex-execution-*` is active surface. | None for location contract. |
| Original source roots `llm mvp` and `C:\Projects\llm_mvp_core` | Paths existed in Pass 05 inventory; `C:\Projects\llm_mvp_core` is core project/source-root. | Full source-to-plan target and implementation file inventory remain later evidence work. |
| Participating repos and wrong-repo guard | Five Claude baseline repos mapped: `llm_mvp_core`, `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`, `FigmaToCode-design-gen`, `CanvaToCode`. CanvaToCode/FigmaToCode are client/caller surfaces only. | Physical repo/path/ref proof and implementation proof remain later. |
| Branch/workdir rule | Latest updated branch is read-only source authority; future work happens in `C:\Projects\DPOLegalLearning\<repo>` / `DPOLegalLearning`; no-write mergeability is checked before later commit/push against the original authority branch and `main` where project rules require it; no PR/merge without separate future instruction. | Future read-only branch/ref evidence and mergeability command output. |
| Recon/library floor | `20-pass` over `llm mvp` and `40-pass` over `C:\Projects\llm_mvp_core` are accepted as evidence-library closures after independently inspectable focused-pass records; the 40-pass details are now consumed as Pass31-Pass40 plan-family deltas. | Closure is evidence-only: keep `PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE` until R-G/source-map/consumer gaps and latest Fable/Claude review are reconciled. |
| Reference propagation | If Claude baseline/source files answer a question, active-codex must mark it resolved/partial with exact evidence before asking Luba. | Remaining later passes must apply this to their own slices. |

## Sequential Pass 04 R-OT-04 Matrix Note

## Sequential Pass 06 R-OT-06 Matrix Note

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

R-OT-06 coverage after Pass 06 is `PLAN_CONTRACT_REPAIRED_EVIDENCE_NOT_IMPLEMENTED`.

| requirement slice | active coverage | remaining blocker |
|---|---|---|
| Identify low-quality/incorrect results | `low-result` / `weakness` detection is owned by `IGraphWeaknessDetectorUnit`, using async candidates plus held-out scorer/judge signals. | Future implementation/tests must prove detector behavior. |
| Create better output as training goal | `self_best` chosen is a real `Y_best` candidate; `legal teacher` chosen is hosted open-weight/permissive only after legal/provenance proof. | Provider terms/license proof and mock unit tests remain future evidence. |
| DPO builder authority | `GraphDpoPair` remains `level=unit`; trainable material requires `TeacherProvenance` / `JudgeProvenance` refs, split refs, candidate-order invariant, excluded leakage signals, and legal scope. | B6/B9/B10/B12/B13 consistency and code proof remain later. |
| Best models as judges/evaluators | Closed proprietary models are `judge-only` / eval-only; judge may score/rank/flag but cannot author trainable chosen/rejected text. | Future tests must assert zero closed-proprietary training rows and `judge_did_not_author_chosen`. |
| Train until sufficiently good | `ShouldStop` is measurable: `qualityThreshold=0.95`, `teacherGapEpsilon=0.02`, `threshold/plateau/budget/no_signal` reasons, otherwise continue. | Runtime/config/training proof remains future Gate B evidence. |
| DPO creation/training/measurement guides | Build/train/measure guides map to plan-07/09/10/11: held-out, ablation, reward, speed, accuracy, quality, lifecycle. | Tests/builds/training were not run in Pass 06. |
| Legal/provenance attached to every row | Legal teacher gate, `TeacherProvenance`, `JudgeProvenance`, sidecar/provenance refs, and mock-only external-call boundary are required before train-ready rows. | Future provider/license proof and mock replacement ledger proof remain later. |

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.
For R-OT-04, the current matrix interpretation is plan-level repaired, not closed:
R1/R3/R4/R5/R10 together cover the role architecture only after reading the current
master overlay and edge-file repair. Required current semantics are:
`ILearningUnit` / `ILogicalUnit` -> many-to-many lazy/queryable candidate surface
(`IQueryable<GraphNodeCandidate>` or equivalent); `ILearningFlow` / `ILogicalFlow`
-> relation/correction/edge traversal/fan-out/fan-in without train-ready pair authorship;
`ILearningOrchestrator` / `ILogicalOrchestrator` -> async input->output candidate batch runner;
`ILearningDynamicGraph` -> L5 graph learner over combinations/orders with deterministic
baseline separated from trainable authority. `GraphDpoPair` is unit-level only; graph/edge/path/order
metadata is provenance/context. Future implementation/tests/runtime/provider proof and downstream
B6/B8/B9/B10/B11/B12/B13 consistency review remain open.

## Sequential Pass 07 R-OT-07 Matrix Note

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

R-OT-07 coverage after Pass 07 is `PLAN_CONTRACT_REPAIRED_EVIDENCE_NOT_IMPLEMENTED`.

| requirement slice | active coverage | remaining blocker |
|---|---|---|
| Same input asynchronously through combinations | `GraphCandidateRunRequest` -> async route execution -> `AsyncCandidateBatch`; `N == combinationCount == request.CombinationCount`, default `4`, ceiling `8`, requested-vs-actual recorded, no duplicate padding. | Future code/tests/runtime proof. |
| Many-to-many output candidate shape | Public `IQueryable<GraphNodeCandidate>` or equivalent queryable surface; materialized snapshots only inside `AsyncCandidateBatch`; `NO_OBJECT_OUTPUT` forbids raw `object Output`, untyped arrays, and whole-graph blobs. | B8/B9 implementation and metadata preservation proof. |
| DynamicGraph order/traversal | Deterministic/topological traversal is baseline/candidate enumeration; `GraphOrderResolverUnit` emits `GraphOrderDecision` with `path_order_ref`, `ordered_unit_ids`, `ordered_path_refs`, `skipped_edges`, `rejected_edges`, `trace_ref`, legal/provenance status, and reason codes. | B7/B10/B12/B13 downstream proof. |
| Fan-out/fan-in merge | Fan-out, fan-in, many-to-many merge preserve conflicts, rejected alternatives, failure modes, `LOW_CONTRAST`, `no_signal`, provenance, `path_order_ref`, and trace refs; merge is not a DPO writer. | B13/B16 implementation/test proof. |
| Unit-level DPO feed | `CandidateSelectionInput` feeds B12/B6; train-ready rows stay `GraphDpoPair(level=unit)` with graph/order data as provenance/context only. | B6/B10/B12/legal/provenance proof remains later. |

## Sequential Pass 10 R-OT-10 Matrix Note

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

R-OT-10 coverage after Pass 10 is `PLAN_TRACE_REPAIRED_EVIDENCE_NOT_IMPLEMENTED`.

| requirement slice | active coverage | remaining blocker |
|---|---|---|
| Human-readable solution | Master overlay plus owner files explain what to build before code: UML/status, algorithm, hierarchy, stage/subphase/substep, Gate B, and guides. | Future passes still need product implementation and tests. |
| UML + status | Plan-00 is the named owner; owner plans also have local UML/status sections. Status must distinguish existing, NEW, gap, and implementation proof pending. | No implementation proof was produced in Pass 10. |
| algorithm | Plans 03/04/06/07/08/09/10/11 cover async combinations, DPO pair building, legal gate, training loop, measurement, promotion/checkpoint, graph and orchestrator behavior. | Runtime/test proof remains future Gate B evidence. |
| code refs / line / quote | Exact code refs may be used only when already present and proven in an owner doc. Missing or unverified line anchors are `NEEDS_CODE_REF_INVENTORY`. | This pass did not inspect product source lines; no fake quote or guessed line is allowed. |
| hierarchy | Plan-05/guide-04 preserve `ILearningMachine` / DynamicGraph L5 -> `ILogicalOrchestrator` -> `ILogicalFlow` -> `ILogicalUnit`, no L6. | No hidden graph-wide/blob DPO or deterministic final authority may be introduced later. |
| stages and no hidden subtasks | Plan-14, board, and matrix must expose each stage, subphase, substep, dependency, owner file, and Gate B proof. | Hidden subtasks or hidden design decisions remain blockers. |
| guides | Guides 01-05 link build DPO, train, measure speed/accuracy/quality, and ILearningMachine/DynamicGraph pattern; guide-05 (auto-dpo-loop) is the R-C auto-DPO loop leg (owner file `active-codex-execution-guide-05-auto-dpo-loop.md`). | Guides are not proof that code, training, tests, or provider checks have run. |

## PASS11 R-OT-11 Matrix Note - Source-to-Plan And Blocker Inventory

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| requirement slice | active coverage after Pass 11 | remaining blocker |
|---|---|---|
| B-1..B-22 inventory | Historical/pre-Pass39 Pass11 table in the full-task coverage gate enumerated B-1 through B-22 with status, owner section, evidence source, and next repair/proof type. Current B inventory is owned by the Pass39 table. | Old partial/open/not-proven/trace-repair rows are historical/evidence-only for present classification; Pass39 has 22 rows and no missing B number. |
| Missing-number guard | B-5 is preserved as `MISSING_NUMBER_NEEDS_TRACE_REPAIR` because older attachment 0ce0 omitted B5 and attachment 912 grouped B-5..B-20. | B-5 cannot be dropped, renumbered away, or treated as closed without direct source evidence. |
| Source-to-Plan | Historical/pre-Pass30/pre-Pass39 snapshots used target 64 and partial accepted-delta counts. Current Pass30 accepts Source-to-Plan 64/64 as inventory evidence only; Pass39 B-22 classification is Pass39 execution-proof-pending. | Source-to-Plan inventory is not implementation proof, test proof, B-22 closure, or readiness. |
| Continuation rule | `PLAN_NOT_READY` is a repair queue and next bounded pass route. | A NOT_READY verdict alone is not a final stop while safe plan/governance repair remains. |
| Future gates | Execution checks can live in future Gate B after approval. | Gate B cannot hide unresolved source-map gaps, missing blocker numbers, design decisions, provider/provenance/threshold/model lifecycle decisions, or active-vs-Claude drift. |

## Pass 17 R-OT-14/R-OT-16 Matrix Row - Source-to-Plan 64/B-22 Contract

Historical/pre-Pass39 verdict remained `PLAN_NOT_READY`; implementation authorization remained `NO`. At Pass17, matrix coverage for references/stages/subtasks and active-vs-Claude authority was not complete until Source-to-Plan target 64 was enumerated and B-22 reconciliation had no unresolved categories. Current B-22 blocker-inventory classification is Pass39 `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING`, not readiness or implementation authorization.

| requirement slice | Pass 17 state | matrix consequence |
|---|---|---|
| R-OT-14 references/stages/subtasks | `PARTIAL_NEEDS_OWNER_REVIEW` | Source-to-Plan target 64 is required, but `64/64 not claimed by Pass 17`. |
| R-OT-16 Claude baseline plus validated deltas | Historical/pre-Pass39 B-22 open | Rows had to become `MAPPED_VERIFIED` or `MAPPED_ACCEPTED_DELTA`; Pass39 B-22 blocker-inventory classification is Pass39 execution-proof-pending, while readiness and implementation authorization remain absent. |
| Missing active counterparts | `MISSING_ACTIVE_COUNTERPART` | Future pass must add active owner row before readiness. |
| Historical counters/status | `STALE_HISTORICAL_ONLY` | Do not count toward current coverage without nearby current verdict. |
| Pending Claude Repair2/latest evidence | `CLAUDE_REPAIR2_UNVERIFIED` | Evidence only; cannot close B-22. |
| Code reference inventory | `NEEDS_CODE_REF_INVENTORY` | Route to plan-14/code-reference owner before execution proof. |

Matrix acceptance rule: summary prose, file count, checklist row, or ledger row cannot satisfy target 64. Each counted Source-to-Plan row must identify row id/category, active owner, Claude/latest source, accepted delta status, downstream consumers, and current acceptance state.

## Pass19 R-OT-14/R-OT-16 Matrix Row - Row-Evidence Boundary

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass18 satisfies the denominator visibility requirement by placing 64 inspectable Source-to-Plan roster rows in plan-14 section 3.2. Pass19 accepts only 13 directly evidenced delta rows and leaves the rest conservative.

| requirement slice | Pass19 matrix state | remaining blocker |
|---|---|---|
| R-OT-14 references/stages/subtasks | `source_to_plan_roster_rows = 64`; current Pass26 `MAPPED_ACCEPTED_DELTA = 46`; no hidden denominator rows | Per-row source evidence, owner acceptance, consumer mirrors, and readback are still incomplete for the remaining rows. |
| R-OT-16 Claude baseline plus validated deltas | Historical/pre-Pass39: `MAPPED_VERIFIED = 0`; Pass26 `MAPPED_ACCEPTED_DELTA = 46` | B-22 remained open at that time; Claude Repair2 was not accepted wholesale; Pass20-Pass26 rows were planning/source-map acceptance only. Current Pass39 classifies B-22 as execution-proof-pending, not readiness or implementation authorization. |
| historical/pre-Pass26 continuation queue | `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 23`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Historical/pre-Pass26 Pass25 queue only. Current Pass26 queue is `NEEDS_CODE_REF_INVENTORY = 18`; remaining rows still need code/source inventory before completion. |

## Pass20 R-OT-14/R-OT-16 Matrix Row - R16/R17 Seam Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass20 resolves the four former partial seam rows at planning/source-map level only.

| requirement slice | Pass20 matrix state | remaining blocker |
|---|---|---|
| R16 design-to-code seam | Ф10.1.1 and Ф10.1.2 are `MAPPED_ACCEPTED_DELTA`; SR-16 in-repo `44.DesignConversionDomain` backing is line-read, while FigmaToCode/CanvaToCode remain client/caller surfaces. | Platform/FLOW-31 facade, client adapters, client-host e2e, tests, and runtime proof remain future implementation evidence. |
| R17 better-output-to-DPO seam | Ф10.2.1 and Ф10.2.2 are `MAPPED_ACCEPTED_DELTA`; SR-14 is `NEW_PLANNED_CONTRACT` with event, mock response, GraphDPO normalization, quarantine/personal route, future unit test, and replacement-ledger obligation. | Physical implementation, real origin/provenance/legal proof, approved test execution, external-call replacement, and train-ready/common proof remain future evidence. |

## Pass21 R-OT-14/R-OT-16 Matrix Row - Ф11 Final-Control Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass21 resolves the two former roster-only final-control rows at planning/source-map level only.

| requirement slice | Pass21 matrix state | remaining blocker |
|---|---|---|
| Ф11.1.1 completion matrix control | Ф11.1.1 is `MAPPED_ACCEPTED_DELTA`; active/Claude board rows and the Ф11 requirements-matrix phase row define the requirement-to-phase-to-evidence completion matrix. | Actual completion matrix rows still need execution evidence, 0 `NOT_PROVEN`, and two-PASS markers before readiness. |
| Ф11.2.1 ten-review/final-verdict control | Ф11.2.1 is `MAPPED_ACCEPTED_DELTA`; active/Claude board §B.6 exposes the ten-review ledger and `CLAUDE.md` defines the 10-review and exact final-verdict token gate. | All 10 review rows are still not run/count false; no final architect verdict is claimed. |
| historical Source-to-Plan counts | Historical/pre-Pass23-F2 Pass22 count: `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 28`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 36`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Current Pass30 count is `MAPPED_ACCEPTED_DELTA = 64` and `NEEDS_CODE_REF_INVENTORY = 0` as inventory only; Pass39 B-22 classification is Pass39 execution-proof-pending. |

## Pass23-F2 R-OT-14/R-OT-16 Matrix Row - Ф2 DPO/Legal Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass23-F2 resolves only the three Ф2 DPO/legal rows at planning/source-map level.

| requirement slice | Pass23-F2 matrix state | remaining blocker |
|---|---|---|
| Ф2.1.3 audit/quarantine contaminated rows | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-28/SR-30/SR-31 support quarantine/rebuild policy, scanner route, license helper, and Claude re-scan boundary. | Live `sonnet-*` dataset scan, contaminated row count, and rebuild/quarantine execution remain future implementation evidence. |
| Ф2.2.1 `GraphDpoPair` canon/projection | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-26/SR-27/SR-30/SR-31 support NEW planned canon plus existing `LearningTrainingItem`, `DpoPair`, `DpoDataset`, DPO trainer/vector-batch, feedback-DPO, and logical-DPO anchors. | Physical `GraphDpoPair`, `TeacherProvenance`, and `TeacherProvenanceGate` implementation remains future code/source proof. |
| Ф2.2.2 five formats/legal batch gate | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-26/SR-28/SR-29/SR-30/SR-31 support five existing source formats plus planned canon/legal gate. | Adapter implementation, legal manifest enforcement, batch execution, and tests remain future proof. |
| historical Source-to-Plan counts | Historical/pre-Pass29 Pass23-F2 count: `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 31`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 33`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Current Pass30 count is `MAPPED_ACCEPTED_DELTA = 64` and `NEEDS_CODE_REF_INVENTORY = 0` as Source-to-Plan inventory only; Pass39 B-22 classification is Pass39 execution-proof-pending, not readiness. |

## Pass24 R-OT-14/R-OT-16 Matrix Row - Ф3 DPO-Builder Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass24 resolves only the three Ф3 DPO-builder rows at planning/source-map level.

| requirement slice | Pass24 matrix state | remaining blocker |
|---|---|---|
| Ф3.1.3 invariants/schema validator | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-32/SR-33/SR-34 support planned validator/invariants plus existing anti-leakage, source-provenance, schema/quarantine, near-duplicate, and wrong-marker precedents. | Physical `GraphDpoSchemaValidator` implementation, runtime validation, and tests remain future proof. |
| Ф3.2.1 `BuildRejected` taxonomy | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-34 supports existing `[WRONG:]`, `ExtractRejectedType`, `BuildRejectedCandidate`, `RejectedType`, and held-out precedents plus Claude rejected-origin taxonomy. | GraphDPO `BuildRejected` implementation/execution and tests remain future proof. |
| Ф3.2.2 adapter/materialization | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-26/SR-29/SR-32/SR-35 support planned old-generator adapter plus existing `LearningTrainingItem`, `DpoPair`, `DpoDataset.Load`, `DpoBatch`, `DpoTrainer`, feedback/logical-DPO, and source-format anchors. | Physical adapter, JSONL/materialization execution, batch count proof, and tests remain future proof. |
| historical/pre-Pass25/pre-Pass39 Source-to-Plan counts | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 34`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 30`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Historical evidence only. Current correction after Pass30/Pass39: plan-14 has 64 canonical Source-to-Plan rows as inventory/source-map evidence only; this is not readiness, B-22 closure, implementation authorization, code proof, test proof, or execution proof. Current B-22 status is owned by Pass39 as `RESOLVED_IN_ACTIVE_PLAN_EXECUTION_PROOF_PENDING`; `PLAN_NOT_READY` and implementation_authorization `NO` remain. |

## Pass25 R-OT-14/R-OT-16 Matrix Row - Ф4 Trained-State/Lifecycle Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass25 resolves only the seven Ф4 trained-state/lifecycle rows at planning/source-map level.

| requirement slice | Pass25 matrix state | remaining blocker |
|---|---|---|
| Ф4.1.1 LoRA wiring/inference dropout | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-36/SR-43 support existing LoRA adapter/dropout/attention-field precedents plus planned bridge classification. | Physical `LoraApplyResult`, `ForwardWired`, in-attention adapter-map wiring, runtime/training proof, and tests remain future evidence. |
| Ф4.1.2 reward margin from engine | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-37 supports `DpoMetrics.RewardMargin`, `DpoTrainer` margin computation, and current `TranslationMachine` literal-defect anchors. | Removing literal `0.03`, wiring graph consumers to engine metrics, and tests remain future proof. |
| Ф4.2.1 continue-training checkpoint path | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-38 supports parent-checkpoint load, DPO steps, metric checkpoint files, translation continue-training precedent, and Claude lifecycle contract. | Graph `LoraCheckpointIo`, graph checkpoint implementation, fresh-load/continue run, and tests remain future proof. |
| Ф4.2.2 model server/repository | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-39/SR-43 support existing model-server interfaces, local repository SHA-256 upload, locator/proxy, and planned `LocalFileModelServer`. | Physical local file server, publish/open round trip, locator integration, and tests remain future proof. |
| Ф4.2.3 lifecycle manifest with provenance | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-40/SR-43 support generated-capability manifest/persistence plus Claude `TeacherProvenance` 20-field and lifecycle proof canon. | Physical `TeacherProvenance`, graph lifecycle fields, rollback/fresh-load proof, sidecar writer, and tests remain future proof. |
| Ф4.3.1 unit-level trainable node | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-41/SR-43 support L5 learning contracts, logical-DPO unit, and DPO margin trainer for unit-level planned training. | Physical `GraphNodePreferenceState`, preference weights, graph node checkpoint, held-out/ablation proof, and tests remain future proof. |
| Ф4.3.3 R5 split/machine pair | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-42/SR-43 support existing `ILearningMachinePair` as later activation precedent, 25.ModelServer common-leg precedents, and Claude Q6 split. V1 active training remains common-only. | Physical `DynamicGraphMachinePair`, `LegScope` graph manifest wiring, anonymized sync payload inspection, and tests remain future proof after a later adaptive activation decision; no client/private row may train common by default. |
| historical Source-to-Plan counts | Historical/pre-Pass39 Pass27 count: `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 50`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 14`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Current Pass30 count is `MAPPED_ACCEPTED_DELTA = 64` and `NEEDS_CODE_REF_INVENTORY = 0` as Source-to-Plan inventory only; Pass39 B-22 classification is Pass39 execution-proof-pending, not readiness. |

## Pass26 R-OT-14/R-OT-16 Matrix Row - Ф5 Measurement/Quality/Lifecycle Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass26 resolves the five Ф5 measurement/quality/lifecycle-probe rows at planning/source-map level only.

| requirement slice | Pass26 matrix state | remaining blocker |
|---|---|---|
| Ф5.1.1 measurement report/coordinator/speed | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-44/SR-45/SR-49/SR-50 support existing metric contracts, live DPO boundary, Claude measurement baseline, and absence of physical graph measurement symbols. | Physical `GraphMeasurementReport`, coordinator/speed meter implementation, metric execution, and tests remain future proof. |
| Ф5.1.2 accuracy meter/split guard | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-44/SR-46/SR-49/SR-50 support held-out evaluation contracts, source-disjoint/leakage precedents, Claude `IHeldOutScorer`/split proof, and absence of physical scorer symbols. | Physical held-out scorer/runner, graph scoring execution, near-dup adjudication proof, and tests remain future proof. |
| Ф5.1.3 untrained honesty/no shortcut | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-46/SR-47/SR-49/SR-50 support ablation/no-semantic-quality precedents plus Claude untrained/no-regex-dictionary gate. | Physical DynamicGraph untrained reflection probe and no-shortcut execution proof remain future evidence. |
| Ф5.2.1 quality meter | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-45/SR-46/SR-48/SR-49/SR-50 support quality/leakage/quarantine precedents plus Claude per-unit/per-edge novelty/ablation/permutation gates. | Physical `GraphQualityMeter`, per-edge ablation/novelty/permutation runs, and tests remain future proof. |
| Ф5.2.2 lifecycle probe/two-PASS | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-44/SR-47/SR-49/SR-50 support export/import/evaluate contracts and scoped fresh-load/held-out probe precedents plus Claude lifecycle/two-PASS baseline. | Physical `GraphLifecycleProbe`, graph export/import/fresh-load/continue-training execution, rollback proof, and trained-feature proof remain future evidence. |
| historical Source-to-Plan counts | Historical/pre-Pass29 Pass26 count: `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 46`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 18`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Current Pass30 count is `MAPPED_ACCEPTED_DELTA = 64` and `NEEDS_CODE_REF_INVENTORY = 0` as Source-to-Plan inventory only; Pass39 B-22 classification is Pass39 execution-proof-pending, not readiness. |

## Pass27 R-OT-07/R-OT-14/R-OT-16 Matrix Row - Ф6 Async-Combinations Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass27 resolves the four Ф6 async-combinations rows at planning/source-map level only.

| requirement slice | Pass27 matrix state | remaining blocker |
|---|---|---|
| Ф6.1.1 combination planner, async flag, budget, dedup | `ACCEPTED_PUSHED_SUBSTRATE`; slice 07 physically supplies `IGraphCombinationStrategy`, `AxisSweepCombinationStrategy` and `BoundedCombinationGraphCandidateRunner` with bounded concurrency, stable plan order and no duplicate padding. | Slice evidence is `32/32`, graph `187/187`, build `0/0`, repeat independent `ACCEPT`, commit `408044656911d0b5766d71d9723d661e310156ce`. Whole Ф6.1.1 is not `done`: production L4 topology consumption/call-chain proof and composite Gate-B acceptance remain open. |
| Ф6.1.2 async arbiter and candidate collection | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-51/SR-53/SR-54/SR-56 support `Task.WhenAll`, branch-result collection, arbiter constraints, and Claude `GraphCandidateOutput` queryable-to-snapshot boundary. | Physical candidate collector/arbiter implementation and execution evidence remain future proof. |
| Ф6.2.1 combination judge and teacher-goal gate | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-54/SR-56 support planned judge-origin, deterministic-baseline fallback, legal teacher, closed-provider, service-mode, and grounding constraints. | Physical judge/gate implementation, provider/terms proof where needed, and legal-route execution remain future proof. |
| Ф6.2.2 pair assembler and many-to-many merge | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-53/SR-55/SR-56 support many-to-many/queryable candidate shape and unit-level DPO batch contracts with metadata in envelopes/refs. | Physical pair assembler, fan-in/many-to-many merge implementation, and graph pair assembly proof remain future evidence. |
| historical Source-to-Plan counts | Historical/pre-Pass39 Pass27 count: `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 50`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 14`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Current Pass30 count is `MAPPED_ACCEPTED_DELTA = 64` and `NEEDS_CODE_REF_INVENTORY = 0` as Source-to-Plan inventory only; Pass39 B-22 classification is Pass39 execution-proof-pending, not readiness. |

## Pass28 R-OT-02/R-OT-05/R-OT-07/R-OT-14 Matrix Row - Ф7 Auto-DPO Loop Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass28 resolves the four Ф7 auto-DPO-loop rows at planning/source-map level only.

| requirement slice | Pass28 matrix state | remaining blocker |
|---|---|---|
| Pass28 item: Ф7.1.1 weakness detector and TeacherGoal/legal teacher units | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-57/SR-58/SR-62 support ResultGap/low-result precedents plus Claude weakness detector, legal teacher, TeacherProvenance, closed-judge-only, and no closed-train leakage contracts. | Physical graph weakness detector, legal teacher provider route, and provider/terms execution remain future proof. |
| Pass28 item: Ф7.1.2 loop pair builder and anti-forgetting replay | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-59/SR-62 support unit-level `LearningTrainingItem`, `GraphDpoPair`/TeacherProvenance baseline, and replay-buffer precedent. | Physical graph pair builder, graph anti-forgetting replay loop, and graph-level train execution remain future proof. |
| Pass28 item: Ф7.2.1 AutoDPO prepare/run/train/evaluate loop | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-60/SR-61/SR-62 support `TrainBatchAsync`, `EvaluateAsync`, held-out/source-disjoint, RewardMargin, and Claude AutoDPO loop contract. | Physical `AutoDpoLoopOrchestrator`, loop execution, train/evaluate run, and tests remain future evidence. |
| Pass28 item: Ф7.2.2 ShouldStop threshold/plateau/budget/no-signal and promotion | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-61/SR-62 support locked thresholds `RuntimeProductAccuracy >= 0.95`, `AccuracyAfter - AccuracyBefore >= +0.02`, `RewardMargin >= 0.025`, `teacherGapEpsilon=0.02`, `plateauWindow=3`, `maxIterations=20`, and no-signal/promotion contract. | Physical graph `ShouldStop`, no-signal marker/counter, stop-loop execution, and promotion proof remain future evidence. |
| historical Source-to-Plan counts | Historical/pre-Pass39 Pass28 count: `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 54`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 10`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Current Pass30 count is `MAPPED_ACCEPTED_DELTA = 64` and `NEEDS_CODE_REF_INVENTORY = 0` as Source-to-Plan inventory only; Pass39 B-22 classification is Pass39 execution-proof-pending, not readiness. |

## Pass29 R-OT-05/R-OT-07/R-OT-14 Matrix Row - Ф8 No-Hidden-Determinism Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass29 resolves the four Ф8 no-hidden-determinism rows at planning/source-map level only.

| requirement slice | Pass29 matrix state | remaining blocker |
|---|---|---|
| Pass29 item: Ф8.1.1 inventory/classify 9 deterministic or baseline mechanisms | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-63/SR-64/SR-68 support the active/Claude plan-12 9 mechanisms table, deterministic/baseline code anchors, and mechanism #4 `OrchestrationScenarioPhasePlanner` classification. | Physical code still contains baseline/deterministic mechanisms; this row proves planning classification only, not hidden-determinism removal. |
| Pass29 item: Ф8.1.2 replacement blueprints | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-63/SR-65/SR-68 support `SemanticEdgeRouterUnit`, `SemanticEdgeRoutingState`, training update, checkpoint/ablation, and excluded-leakage blueprint contracts. | Physical trainable edge authority, graph checkpoint, and trained replacement execution remain future proof. |
| Pass29 item: Ф8.2.1 `CandidateOrderLeakageGuardUnit` permutation guard | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-66/SR-68 support the SmartSkills guard precedent and planned edge guard that blocks candidate-index, array-order, original/file order, source-path, source-id, exact-prompt, correlation id, and merge order leakage. | Physical DynamicGraph edge guard and negative tests remain future evidence. |
| Pass29 item: Ф8.2.2 cutover latest-wins plus ablation proof | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-61/SR-67/SR-68 support latest-wins/cutover anchors, `RuntimeProductAccuracy >= 0.95`, `AccuracyAfter - AccuracyBefore >= +0.02`, `RewardMargin >= 0.025`, and ablation drop `>=0.02`. | Physical cutover, held-out/fresh-load/continue-training proof, ablation execution, promotion, and tests remain future evidence. |
| historical Source-to-Plan counts | Historical/pre-Pass30 Pass29 count: `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 58`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 6`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Current Pass30 count is `MAPPED_ACCEPTED_DELTA = 64` and `NEEDS_CODE_REF_INVENTORY = 0` as Source-to-Plan inventory only; Pass39 B-22 classification is Pass39 execution-proof-pending, not readiness. |

## Pass30 R-OT-05/R-OT-07/R-OT-14 Matrix Row - Ф9 Consolidated Test Strategy Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass30 resolves the six Ф9 consolidated test-strategy rows at planning/source-map level only.

| requirement slice | Pass30 matrix state | remaining blocker |
|---|---|---|
| Pass30 item: Ф9.1.1 topology mutation tests for fan-out/fan-in/many-to-many | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-69/SR-76 support topology mutation/transition anchors and active/Claude plan-13 test-strategy baseline. | Physical DynamicGraph topology mutation tests still need implementation and execution evidence. |
| Pass30 item: Ф9.1.2 edge determinism and leakage ablation tests | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-70/SR-76 support path-free scoring, source-path warning, source-id negative control, and leakage/ablation precedents. | Physical per-edge determinism/leakage/ablation tests still need implementation and execution evidence. |
| Pass30 item: Ф9.2.1 DPO generator and TeacherProvenance gate tests | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-71/SR-72/SR-76 support DPO primitives, train-ready/quarantine precedents, 20-field `TeacherProvenance`, closed proprietary judge-only behavior, and `DATASET_REBUILD_REQUIRED`. | Graph-specific DPO generator, physical `TeacherProvenance` gate, provider/legal proof, and tests remain future evidence. |
| Pass30 item: Ф9.2.2 continue-training tests with merge/duplicate/empty-state cases | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-73/SR-76 support export/import, parent checkpoint load, metadata-only continue-training probe, overlap rejection, and promotion-blocking precedents. | Real DynamicGraph continue-training, merge/duplicate/empty-state runtime behavior, checkpoint lineage, and tests remain future evidence. |
| Pass30 item: Ф9.3.1 component-to-layer assertion/Gate matrix | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-74/SR-76 support FC-26 layers, L1-L5/ILearning role mapping, UH/TA separation, and Gate matrix assertions. | Filled future implementation/test evidence matrix still requires real test artifacts after authorization. |
| Pass30 item: Ф9.3.2 regression and anti-loophole report | `MAPPED_ACCEPTED_DELTA`; plan-14 SR-75/SR-76 support regression, anti-loophole, count-inflation, no-permission-prompt, stale-status, and no-fake-ready governance. | Regression suite/report execution and baseline test counts remain future evidence. |
| current Source-to-Plan counts | `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 64`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 0`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Source-to-Plan inventory rows are 64/64 accepted as inventory evidence only; not PLAN_READY, not B-22 closure, not implementation authorization, not test execution, and not product proof. |

## Pass22 R-OT-14/R-OT-16 Matrix Row - Ф1 DynamicGraph Contract/Skeleton Source-to-Plan Repair

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. Pass22 resolves the nine Ф1 DynamicGraph contract/skeleton rows at planning/source-map level only.

| requirement slice | Pass22 matrix state | remaining blocker |
|---|---|---|
| Ф1.1.1-Ф1.1.3 graph contracts | `PARTIAL_ACCEPTED_PUSHED_SUBSTRATE`; graph node/edge/topology contracts exist, and slices 08-09 add deterministic builder plus revisioned `IDynamicGraphTopologySource`/runtime-owner contracts. | Slice 08: `24/24`, graph `211/211`, build `0/0`, commit `62c1e996...`; slice 09: `36/36`, graph `247/247`, build `0/0`, commit `382544dd...`. `[historical_at_slice_09]` L4 production consumption was then open; slice 11 closes only the production L4 read seam. `DynamicGraphDomain`, mutation authority and composite Ф1.1.3/Gate-B proof remain open; do not mark the whole row done. |
| Ф1.2.1-Ф1.2.3 node rows | `MAPPED_ACCEPTED_DELTA`; plan-14 `SR-17-legacy-active-logicalcontracts`/SR-22/SR-23/SR-24 support `ILogicalUnit`, node contract/algorithm precedents, no-decision, path-free scoring, migration classes, and planned-new node contracts. | Current `SR-17` is the seed corpus. Physical node wrapper/descriptor/candidate/migration implementation and training proof remain future evidence. |
| Ф1.3.1-Ф1.3.3 edge/orchestrator/machine rows | `PARTIAL_ACCEPTED_PUSHED_SUBSTRATE`; earlier edge/fan-in/node slices exist and slice 10 adds strict registry-to-build-signal projection without applying runtime mutation. | Slice 10: `18/18`, graph `265/265`, build `0/0`, commit `002bd7f1523f...`. `[historical_at_slice_10]` `ILogicalRegistryDynamicGraphProjector` was not an L4 production caller or L5 machine; slice 11 closes only the L4 read seam. Whole Ф1.3.2, Ф1.3.3, mutation/runtime-machine behavior and Gate B remain open. |
| historical Source-to-Plan counts | Historical/pre-Pass23-F2 Pass22 count: `MAPPED_VERIFIED = 0`; `MAPPED_ACCEPTED_DELTA = 28`; `PARTIAL_NEEDS_OWNER_REVIEW = 0`; `NEEDS_CODE_REF_INVENTORY = 36`; `ROSTER_ONLY_NEEDS_SOURCE_REF = 0` | Current Pass30 count is `MAPPED_ACCEPTED_DELTA = 64` and `NEEDS_CODE_REF_INVENTORY = 0` as Source-to-Plan inventory only; Pass39 B-22 classification is Pass39 execution-proof-pending, not readiness. |

## Pass 16 Model/Checkpoint Lifecycle Coverage Row

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`. This matrix row covers plan/governance requirements only.

| requirement slice | Pass 16 active coverage | remaining blocker |
|---|---|---|
| Learnable artifact separation | The plan now names deterministic baseline code/config/traversal separately from learned policy/checkpoint state. | Implementation must later prove the separation with code/artifacts; no proof is claimed now. |
| Checkpoint manifest and lineage | Required checkpoint manifest fields include `model_id`, `checkpoint_id`, `graph_schema_version`, `code_source_ref`, `scorer_version`, `threshold_set`, training-data/provenance refs, `legal verdict`, `TeacherProvenance`, scope, `created_at`, `parent_checkpoint_id`, and compatibility status. | Missing manifest or lineage keeps lifecycle status not-proven. |
| Fresh-load evaluation | Product-ready and promotion claims require clean-process import/load plus held-out `RuntimeProductAccuracy`, reward margin, delta, ablation, threshold/scorer set, compatibility, and baseline comparison. | In-memory training success remains insufficient. |
| Common/adaptive promotion split | Common/global promotion is blocked unless legal/provenance/source split, held-out thresholds, ablation, no regression, no provider terms pending, source ref, and compatibility pass. | Failed proof routes to personal/adaptive, quarantine, rollback, or repair; not common/global. |
| Rollback/continue-training | Previous accepted checkpoint remains rollback target; continue-training requires loaded parent, `parent_checkpoint_id`, source-disjoint approved new rows/refs, and child manifest. | Missing lineage or replay/overwrite risk blocks continue-training claim. |

No row above claims model/checkpoint implementation, training success, fresh-load proof, promotion, rollback execution, continue-training execution, readiness, or B-22 closure.

## Pass31 Matrix Current Coverage Overlay

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

Pass31 supersedes older matrix rows that say the Source-to-Plan map is still not `64/64` where they are not explicitly historical. Current Source-to-Plan inventory state is: total=64; `MAPPED_VERIFIED=0`; `MAPPED_ACCEPTED_DELTA=64`; `PARTIAL_NEEDS_OWNER_REVIEW=0`; `NEEDS_CODE_REF_INVENTORY=0`; `ROSTER_ONLY_NEEDS_SOURCE_REF=0`. This is inventory/source-map acceptance only.

Coverage is still not ready because these requirements remain partial/blocked at current evidence level:

- Trained-feature outcome: `SeedCorpus-LanguageTranslate` sourcing/shape is resolved in plan by Pass35, but actual seed file creation, implementation/training/runtime proof, and no held-out 0.95 run remain future evidence.
- Legal teacher leg: the `qwen3-together-v1` (Qwen3-Apache-2.0-via-Together) hosted route is `RESOLVED_BY_LUBA_DECISION (2026-07-07)` — OUT of quarantine, APPROVED for train/common use (Luba «Выведи маршрут из карантина» / original-task whitelist + Together ToS 7.2 customer-owns-Output + ZDR available); the operational ZDR/provenance/pin-serving-id steps remain as `реализуй` preconditions, not blockers. The local self-authored/self-best v1 path also remains executable after Luba approval. [HISTORICAL/pre-2026-07-07: this cell previously read "provider terms/output-use/training-rights proof remains pending for hosted train/common".] SCOPE FENCE: only `qwen3-together-v1` (DeepSeek(MIT) same footing); Kimi/GLM/Mistral keep per-model verify-before-use; closed models stay FORBIDDEN as training teachers.
- Merge/no-hidden-determinism evidence: B-16 CON-merge typed carrier is resolved in plan only and still needs later implementation/test proof; after Pass34, B-10 AD-detect / graph-order no-hidden-determinism is resolved in plan only and AD-detect measurement remains future execution proof.
- Source authority/reconciliation: SR-16 overload is resolved in plan, but B-22 propagation across active consumers is not proven closed.

## Pass32 Matrix B-16 Typed CON-Merge Carrier Row

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| requirement slice | Pass32 active coverage | remaining blocker |
|---|---|---|
| B-16 typed CON-merge carrier | `IEdgeFanInMerger.Merge` physically returns `GraphFanInMergeResult` with `Merged`, `Context`, `FanInMergeRole MergeRole`, `bool MergeOrderIsAuthority`, `MergedBranchCount`, conflict/rejected/provenance/trace/failure refs, and v1 values `DeterministicFanInBaseline` / `false`. | Earlier accepted/pushed fan-in slice proves its bounded carrier/merger behavior; `[historical_at_that_slice]` L4 production consumption was then open. Slice 11 closes only the L4 read seam; B-22 propagation and composite Gate B remain open. |
| B-16 permutation-invariance | Accepted fan-in tests cover deterministic non-authoritative merge behavior; the current code-first regression remains green through `265/265`. | This bounded evidence is not whole runtime/machine proof and does not authorize learned winner selection or DPO. |
| B-16 boundary | Better-output/FLOW-38/five-format adapter work remains B-4/R17 plus B-19; B-16 does not emit train-ready chosen/rejected DPO text. | After Pass34, B-19 SR-16 route/source disambiguation and B-10 AD-detect / graph-order no-hidden-determinism are resolved in plan only; AD-detect execution evidence remains pending. |

## Pass33 Matrix B-19 SR-16 Disambiguation Row

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| requirement slice | Pass33 active coverage | remaining blocker |
|---|---|---|
| B-19 source-id disambiguation | `SR-16-CoreBacking`, `SR-16-RouteFacade`, current seed-corpus `SR-17` / `SeedCorpus-LanguageTranslate-v1`, and historical alias `SR-17-legacy-active-logicalcontracts` are separate active plan identifiers. | Physical host/API route inventory, same-scenario Path A/Path B proof, seed-file proof, and future execution evidence remain pending. |
| R16 participating repos/workstreams | `llm_mvp_core` owns core backing; `xiigen-platform-dot-net-angular` owns the public facade role; `xiigen-flows-dot-net-angular` owns FLOW-31 seam role; FigmaToCode/CanvaToCode are client/caller surfaces only. | No repo/path/branch/file implementation proof ran in Pass33. |
| Seed corpus split | Current `SR-17` is the separate active-pilot seed-corpus dependency, not SR-16 route proof and not the old LogicalContracts alias. Pass35 defines `SeedCorpus-LanguageTranslate-v1` as a self-authored synthetic local fixture with train/held_out split, provenance/legal fields, and `semantic_quality_claimed=false`. | Actual JSONL creation, legal/split gates, held-out exact-match + char-accuracy + `RuntimeProductAccuracy`, tests, training, runtime, and product proof remain future evidence. |

## Current Domain E2E Scope Guard

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| surface | current scope | E2E precondition bundle before readiness |
|---|---|---|
| `language-translate` | only active executable pilot in v1 after approval; bundle designed, not executed | Seed corpus file, legal/provenance/split gates, held-out `expected_label` gold, exact-match + char-accuracy + `RuntimeProductAccuracy`, unit-level DPO, lifecycle/checkpoint/fresh-load/continue-training proof, and tests. |
| `local-wiki` | navigation/reference/deferred | Domain corpus plus source snapshots, held-out query/gold-answer scorer, legal/provenance manifest, activation substeps, lifecycle gates, and tests; recon files alone are evidence-only. |
| `small-model` | navigation/reference/deferred | Model/source/license proof, seed/data rows, held-out scorer/gold, legal/provenance verdict, activation substeps, lifecycle gates, and tests; generic model templates are not E2E proof. |
| `extensions` / host clients | navigation/reference/deferred | Explicit extension/client activation plan, handler/source proof, same-scenario Path A + Path B evidence, redaction/provenance, scorer or acceptance metric, lifecycle/promotion policy, and tests. |
| `figma-html-css` / `design-to-code` / `FigmaToCode` / `CanvaToCode` | R16 seam/client surfaces; client/caller roles only | Platform/FLOW-31/client-host proof, same-scenario join evidence, legal/client-output quarantine, scorer/acceptance metric, activation substeps, lifecycle gates, and tests; local folder existence is not platform-host authority. |

## Pass35 Matrix SeedCorpus Row

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| requirement slice | Pass35 active coverage | remaining blocker |
|---|---|---|
| `SR-17` / `SeedCorpus-LanguageTranslate-v1` source/shape | Current `SR-17` seed corpus; self-authored synthetic local fixture; planned future path `llm_mvp_core/27.TranslationDomain/SeedData/language-translate-seed-v1.jsonl`; no third-party text, no external API, no web scrape, no closed-model output. | Future approved worker must create the file and run gates; no file/test/training/runtime proof exists now. |
| Seed row schema | Fields: `seed_id`, `source_language`, `target_language`, `input_text`, `chosen_translation`, `rejected_translation`, `wrong_marker` or `rejected_reason`, `split=train|held_out`, `source_category=synthetic_self_authored`, `license=project_internal_self_authored`, `TeacherProvenance` or provenance refs, `semantic_quality_claimed=false`, and held-out `expected_label == chosen_translation`. | Data authoring/readback and schema validation are future execution evidence. |
| Seed coverage and measurement | Plan default at least 12 train + 4 held_out rows across at least 2 language directions; held_out feeds exact-match, char-accuracy, and `RuntimeProductAccuracy`. Gold origin is self-authored `expected_label`; comparison normalizes Unicode NFC, outer whitespace, line endings, and repeated ASCII whitespace, while preserving case, punctuation, script, diacritics, emoji, and named entities. | Product quality, trained-feature success, 0.95 threshold, ablation, fresh-load, and continue-training remain unproven. |

Historical matrix counters such as 46/18, 50/14, 54/10, 58/6, or "map not 64/64" are pre-Pass30/Pass31 evidence only unless restated with the current inventory-only boundary.

## Pass36 Matrix Qwen3/Together Provider-Terms Row

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| requirement slice | Pass36 active coverage | remaining blocker |
|---|---|---|
| Qwen3 model-license evidence | Official Qwen evidence supports Apache-2.0 model-license status for open-weight Qwen3 models. | This is model-license evidence only, not hosted TogetherAI output-use approval, legal advice, legal approval, or provider API proof. |
| Hosted Qwen3 route | `QWEN3_TOGETHER_V1_RESOLVED_BY_LUBA_DECISION (2026-07-07)`: Qwen3 Apache-2.0 is the selected teacher blueprint and `qwen3-together-v1` / TogetherAI is APPROVED for train/common use — OUT of quarantine per Luba «Выведи маршрут из карантина» (original-task whitelist «можно только с допущенными для этого моделями» + Together ToS 7.2 customer-owns-Output + no explicit no-distillation clause found + ZDR available). Fireworks/OpenRouter/DashScope are deferred alternates only if a later plan explicitly selects and proves them. [HISTORICAL/pre-2026-07-07: status was `QWEN3_SELECTED_RECOMMENDED_PROVIDER_TERMS_PROOF_PENDING`.] | Train/common use is NO LONGER `BLOCK`/`QUARANTINE` for this route; operational preconditions remain at `реализуй` (ZDR-before-first-call, per-call provenance JSON, pin Together serving model id + version). SCOPE FENCE: `qwen3-together-v1` only; closed models stay FORBIDDEN. |
| `ProviderTermsProof` / provenance contract | Retained as an OPERATIONAL PRECONDITION (not a quarantine blocker) for the approved `qwen3-together-v1` route: `Provider=TogetherAI`, `ModelFamily=Qwen3`, exact `ModelId` (pinned serving id + version at cutover), Apache-2.0 source, `service_mode=plain`, `grounding_used=false`, `teacher_output_used_for_training=true`, `closed_proprietary_model=false`, `data_retention_mode=ZDR_or_no_store` (ZDR enabled before the first hosted call, forward-only). A dated verbatim ToS/privacy `terms_snapshot_ref` is RECOMMENDED, not a gate; do NOT assert a lawyer signed off — record it as the Luba product decision above. [HISTORICAL/pre-2026-07-07: this contract was framed as a "hosted train/common blocker".] | Executed at `реализуй`; local self-authored/self-best v1 does not require the hosted call. |

Judge/eval-only use is allowed only with `used_for_training=false`, non-pending terms, organization privacy/ZDR proof, no passthrough gap, no grounding/search, and no sensitive/private raw data. Future implementation/test/runtime/provider/git evidence remains after readiness and Luba approval.

## Pass37 Matrix Latest-Claude Sync Row

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| requirement slice | Pass37 active coverage | remaining blocker |
|---|---|---|
| Latest Claude canon ownership | `GraphDpoPair` owner is active/Claude plan-07 with exactly 14 fields; `TeacherProvenance` owner is active/Claude plan-08 section 3.1 with exactly 20 fields including RR6 `service_mode`/`grounding_used`. | Runtime/schema implementation proof remains future evidence after readiness and Luba approval. |
| Qwen3/Together route | Latest baseline keeps `qwen3-together-v1` as the selected v1 provider config; the route is now `RESOLVED_BY_LUBA_DECISION (2026-07-07)` — OUT of quarantine, APPROVED for train/common use (Luba «Выведи маршрут из карантина» / original-task whitelist + Together ToS 7.2 customer-owns-Output + ZDR available). [HISTORICAL/pre-2026-07-07: "terms are pending".] | Hosted train/common use is NO LONGER `BLOCK`/`QUARANTINE` for this route; operational ZDR/provenance/pin-serving-id steps remain as `реализуй` preconditions. SCOPE FENCE: `qwen3-together-v1` only; closed models stay FORBIDDEN. |
| B-16 fan-in merge | Active contract mirrors Claude: `GraphFanInMergeResult` carries `MergeRole=FanInMergeRole.DeterministicFanInBaseline` and `MergeOrderIsAuthority=false`; bare `LogicalFlowResult` return wording is stale. | Implementation/permutation proof remains future evidence. |
| Original-task readiness count | Pass37 is one bounded sync/simulation pass only. | Minimum 50 sequential simulation/review records, reconciled against active-codex and latest Claude baseline, remain required before any final readiness claim. |

## Pass39 B-Inventory Coverage Row

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| requirement slice | Pass39 active coverage | remaining boundary |
|---|---|---|
| B-1..B-22 current inventory | Current owner is the Pass39 table in `active-codex-execution-16-full-task-coverage-simulation-gate.md`. The table contains all 22 unique B rows and supersedes older current-looking unresolved rows for present classification. | Not readiness; not implementation authorization; not a 50-pass chain. |
| External/provider residual and R-B evidence-library boundary | B-2 hosted `qwen3-together-v1` route is `RESOLVED_BY_LUBA_DECISION (2026-07-07)` — OUT of quarantine, APPROVED for train/common use (Luba «Выведи маршрут из карантина» / original-task whitelist + Together ToS 7.2 customer-owns-Output + ZDR available); operational ZDR/provenance/pin-serving-id steps remain `реализуй` preconditions, not blockers. [HISTORICAL/pre-2026-07-07: "B-2 hosted provider terms/approval remains required before hosted train/common use".] B-18 is v1 common-only with adaptive deferred unless Luba later expands scope. B-21: the old "first-substantive-recon/deeper-recon-optional" TRACE wording is historical/pre-R-B-closure, but the 20/40 numeric floor stays BINDING (not optional). Current R-B state is accepted `20/20` `llm mvp` and `40/40` `llm_mvp_core` evidence-only closure (artifact/count side closed, libraries BUILT and parent-verified); required recon DEPTH may NOT be downgraded to optional/future without an explicit current Luba quote (`blocked-until-done-or-Luba-approves-narrower`, never `optional-unless-Luba-reopens`); see F0 residual `Q-B21-FLOOR`. | Hosted train/common via `qwen3-together-v1` is authorized subject to the `реализуй` operational preconditions (SCOPE FENCE: `qwen3-together-v1` only; Kimi/GLM/Mistral keep per-model verify-before-use; closed models FORBIDDEN). No silent adaptive activation. Do not treat the 20/40 libraries as optional. Do not inflate evidence-library closure into PLAN_READY, product readiness, B-22 closure, implementation authorization, tests/builds/runtime/provider/git/training/checkpoint proof, or mergeability. |
| Resolved-in-plan rows | B-1, B-3..B-17, B-19, B-20, and B-22 are resolved in active plan with execution proof pending. | Future implementation/test/runtime/training/host/provider/git proof only after readiness and Luba approval. |
| Readiness count | Pass39 is one bounded reconciliation packet only. | Minimum 50 sequential simulation/review evidence records remain required before any `PLAN_READY`, `FULLY_READY`, B-22 readiness closure, or implementation authorization. |

## Pass42 Residual Classification Matrix Row

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| class | coverage classification after Pass41 | remaining boundary |
|---|---|---|
| Implementable plan gaps | None found in the inspected residual set. B-1, B-3..B-17, B-19, B-20, and B-22 remain resolved in plan with execution proof pending. | If a later pass finds a design/dependency gap, repair the active plan before readiness. |
| Historical/pre-R-B-closure defaults / future expansion options (B-21 floor stays LIVE) | B-18 common-only default with adaptive deferred unless Luba later expands scope. B-21: the old "first-substantive-recon/deeper-recon-optional" TRACE wording is historical/pre-R-B-closure, but the 20/40 numeric floor is NOT optional — it stays BINDING as a live residual. Current R-B state is accepted `20/20` `llm mvp` plus `40/40` `llm_mvp_core` evidence-only closure (artifact/count side closed, libraries BUILT and parent-verified); required recon DEPTH beyond that may NOT be downgraded to "optional/future" without an explicit current Luba quote (`blocked-until-done-or-Luba-approves-narrower`, never `optional-unless-Luba-reopens`). See F0 residual `Q-B21-FLOOR` (`LUBA_PRODUCT_DECISION_RESIDUAL_OR_REQUIRED_LIBRARY_WORK`). | Do not self-activate adaptive, do not treat the 20/40 libraries as optional, and do not inflate evidence-only closure into readiness, B-22 closure, implementation authorization, or execution proof. |
| External/provider/legal proof | Hosted `qwen3-together-v1` (Qwen3-Apache-2.0-via-Together) route is `RESOLVED_BY_LUBA_DECISION (2026-07-07)` — OUT of quarantine, APPROVED for train/common use (Luba «Выведи маршрут из карантина» / original-task whitelist + Together ToS 7.2 customer-owns-Output + ZDR available); operational ZDR/provenance/pin-serving-id steps remain `реализуй` preconditions, not blockers. [HISTORICAL/pre-2026-07-07: "Hosted Qwen3/Together proof and Luba hosted-use approval before hosted train/common use".] SCOPE FENCE: `qwen3-together-v1` only (DeepSeek(MIT) same footing); Kimi/GLM/Mistral keep per-model verify-before-use; closed models stay FORBIDDEN. | For the approved route: not a blocker; operational preconditions executed at `реализуй`. For OTHER hosted/provider material (Kimi/GLM/Mistral, closed, unverified): missing proof still routes to `BLOCK`, `QUARANTINE`, or `EVAL_ONLY`; not train/common-ready. |
| Future execution proof | Seed file, product implementation, tests/builds/runtime, training/checkpoints, host screenshots/Path A/B join, branch/mergeability, mock replacement ledger, and git. | Future execution after readiness and fresh Luba approval only. |
| Count/evidence residuals | 50 accepted sequential pass records plus parent synthesis remain incomplete. Source-to-Plan 64/64 remains inventory evidence only. | Not readiness, B-22 closure as execution proof, or implementation authorization. |
| No-go boundaries | `PLAN_NOT_READY`; implementation authorization `NO`. | No product/code/test/provider/git action is authorized by this matrix row. |

## Pass44 Matrix Hosted-Proof Boundary Row

Historical/pre-post-repair matrix status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

| requirement slice | Pass44 active coverage | remaining boundary |
|---|---|---|
| Local v1 path | Local self-authored/project-internal seed rows, self-best DPO material, legal provenance, and legally proven common-eligible examples are the executable v1 design path after Luba approval. | Future approval, seed authoring, code, tests, training, held-out/fresh-load/continue-training evidence remain pending; no hosted proof needed for this local route. |
| Hosted provider route (`qwen3-together-v1`) | `RESOLVED_BY_LUBA_DECISION (2026-07-07)` — the Qwen3-Apache-2.0-via-Together route is OUT of quarantine and APPROVED for train/common use (Luba «Выведи маршрут из карантина» / original-task whitelist + Together ToS 7.2 customer-owns-Output + ZDR available). Operational safeguards remain as `реализуй` preconditions (ZDR-before-first-call, per-call provenance JSON with `teacher_output_used_for_training=true` / `closed_proprietary_model=false`, pin exact Together serving id + version), not quarantine blockers; dated ToS/privacy archive RECOMMENDED not a gate. [HISTORICAL/pre-2026-07-07: "Hosted Qwen3/Together output cannot train or mutate common until provider terms/output-use/training-rights proof and Luba hosted-use approval pass".] | Approved route: preconditions executed at `реализуй`. Other hosted/provider material (Kimi-K2 `modified-MIT` / GLM / Mistral verify-before-use, closed, or unverified) still routes to `BLOCK`, `QUARANTINE`, or `EVAL_ONLY` on missing proof; closed models stay FORBIDDEN as training teachers. |
| Common/adaptive boundary | Client/private/third-party/unverified material remains adaptive/eval/quarantine by default and cannot become common without positive legal/provenance/source-split proof. | No silent common promotion; no local-v1 stop caused by hosted proof absence. |
| Readiness/count boundary | Pass44 is one bounded repair record. | Not readiness, not implementation authorization, not provider/legal proof, not test/runtime evidence. |
