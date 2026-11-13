# Active Codex Execution 42 - Fable Gap Repair Additions

Date: 2026-07-05

## Current execution pointer (2026-07-14)

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

Этот файл больше не является current work queue: `FABLE_GAP_REPAIR_QUEUE` ниже — историческая карта закрытых plan-fixable находок. Текущий порядок — `CODE_FIRST → TP1_LOCAL eval-only → TP2_HOSTED eval-only only after legal proof → G-DPO-FINAL → DPO/training`; TP1/TP2/DPO ещё не начинались. Slice 12 bootstrap facade принят (`14/14`, six-class `132/132`, seven-class including Adaptive `156/156`, build `0/0`); slice 13 explicit composition profile и первый production caller вне тестов приняты (`20/20`, runtime `14/14`, DynamicGraph `271/271`, Assistant+Adaptive `44/44`, build `0/0`). Это не deployment host и не full readiness. Manual-only цепочка: deployment host/reference seam → profile deserialize/select/runtime lifetime → `Runtime.Assistant` → persistence/restart revision/CAS → authority schema/canon/resolver → remaining `CODE_FIRST`; bounded next slice не объявлен выбранным. Full machine, composite Gate B, edge-authority execution и DPO остаются открытыми. Архитектурные остатки A1-A5 текущие; прежние zero-blocker/no-design-choice выводы не текущие. A5-R1 разрешён для code-first живым указанием Luba; teacher/DPO/training остаётся запрещён до `G-DPO-FINAL`, а closed-proprietary запрет не меняется.

A5 historical-status note (2026-07-13): этот файл-42 заголовок (`FABLE_GAP_REPAIR_QUEUE` / `DESIGN_BLOCKER_CURRENT=FABLE_MISSING_AND_PARTIAL_ITEMS` / «6 MISSING + 30 PARTIAL, downgraded until this file's blocking repairs completed») — historical/pre-S17-B04. Fable-очередь ЗАКРЫТА по составу: каждый PLAN-FIXABLE Fable-item CLOSED at plan level per S17-B04 overlay (полные владельцы: `active-codex-execution-40-current-50-pass-readiness-simulation-20260701.md`, `active-codex-execution-17-50-pass-readiness-repair-ledger.md`, `active-codex-execution-07-execution-gates-open-questions.md`). Текущий единственный readiness семьи = `PLAN_NOT_READY / TWO_PHASE_ADAPTATION_QUEUE` (CURRENT_READINESS_SINGLE_VALUE_GATE); значение PLAN_NOT_READY совпадает, меняется только метка очереди (двухфазная адаптация 2026-07-13). Каноническая teacher/DPO/training-авторизация = `NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL`; code-first lane отдельно authorized `YES_CODE_FIRST_ONLY` владельцем `active-codex-execution-EXECUTION-CONTROL-BOARD.md`, а порядок фаз принадлежит `active-codex-execution-two-phase-local-remote-overlay.md`. Ниже — historical repair-map, а не текущий блокер-список.

historical_plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE [READINESS historical/pre-S17-B04 — Fable-очередь CLOSED по составу; текущий = PLAN_NOT_READY / TWO_PHASE_ADAPTATION_QUEUE]
historical_pre_code_first_implementation_authorization: NO until fresh Luba approval after review
historical_pre_s17_b04_verdict_basis: Newer Fable coverage evidence found 6 MISSING and 30 PARTIAL plan-coverage items after the earlier readiness overlay; therefore, at_that_historical_time, readiness was downgraded until this file's blocking repairs were completed and re-reviewed. [HISTORICAL/pre-S17-B04: every PLAN-FIXABLE Fable item is now CLOSED at plan level per the S17-B04 overlay; this basis is no longer the current blocker set.]
historical_pre_s17_b04_verdict_classes: AT_THAT_HISTORICAL_TIME_DESIGN_BLOCKER=FABLE_MISSING_AND_PARTIAL_ITEMS; AT_THAT_HISTORICAL_TIME_REPAIR_QUEUE=FABLE_RB_RG_AND_PARTIAL_REPAIR; HISTORICAL_PRE_CODE_FIRST_IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review. [HISTORICAL/pre-S17-B04 — Fable-items CLOSED per S17-B04; current architecture review carries A1-A5 residuals and the current execution pointer is above.]

## Evidence Used

- `claude-fable-plan-check-v1.md`: Fable 5.0 max-effort coverage check, 11 sequential passes, 127 findings: 91 COVERED, 30 PARTIAL, 6 MISSING.
- Attachment `98cbe69a`: Claude summary of the Fable run and requested handoff to Codex.
- Original task attachment `8528da06`: ILearningDynamicGraph DPO planning task.
- Attachment `b09e3858` / current Luba-provided task transcript: newer Claude-side evidence reports a later `PLAN_PARTIAL` state after >=50 sequential coverage simulation and micro-fixes. This is evidence text only, not a pass run by Codex in this turn, but it confirms that active-codex must keep readiness downgraded until R-B/R-G and latest Claude deltas are reconciled.

This file is an active-codex repair addendum. It does not claim product implementation, tests, builds, runtime proof, provider proof, training, checkpoint proof, git proof, or mergeability proof.

## Current Readiness Downgrade

The previous `PLAN_READY_FOR_LUBA_REVIEW` overlay is superseded by the newer Fable evidence.

Reason: mental implementation from the active plan still hits plan-level blockers:

1. R-B knowledge libraries are not accepted built artifacts.
2. R-G source addressability is incomplete.
3. Several shared contracts are present in owner files but not propagated to consumers.
4. Several plan-family files still need human-readability and single-owner contract repairs.

At that historical pre-S17-B04 time, until those repairs were fixed and re-reviewed, readiness was:

```text
historical_pre_s17_b04_plan_readiness = PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE
historical_pre_code_first_implementation_authorization = NO until fresh Luba approval after review
```

## Historical Blocking Repairs

### FABLE-BLOCKER-01 - R-B 20-pass llm_mvp docs knowledge library

Current status:
The >=20-pass `llm mvp` docs knowledge library target is now an accepted built target artifact after parent verification. Existing `codex-knowledge-library-*` files remain evidence-only and did not close this requirement; closure came only from the active target artifact's exact 20 focused-pass records.

Required active target artifact:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-knowledge-library-llm-mvp-20-pass.md
```

Current artifact state (2026-07-05): target file is now materialized with the required pass-record schema and source-root queue. Parent-verified accepted count is `accepted_focused_pass_records = 20/20` after sub-agent `019f32a5-d758-7dc1-8394-23432369a6c6` / Aristotle appended Pass 01/20, sub-agent `019f32ac-053d-7d21-a4a1-ec891386546a` / Banach appended Pass 02/20, sub-agent `019f32b0-9bf8-7cd2-8c0a-6f5cd3e45460` / Harvey appended Pass 03/20, sub-agent `019f32b4-eb29-7362-8e03-53b52696a1f5` / Rawls appended Pass 04/20, sub-agent `019f32ba-0b21-7b80-a194-647a73813e1c` / Wegener appended Pass 05/20, sub-agent `019f32c1-7d69-7260-8969-cd8a66a0d0ac` / Fermat appended Pass 06/20, sub-agent `019f32c8-80b0-7611-aee9-67dd0f90a6f2` / Locke appended Pass 07/20, sub-agent `019f32ce-1d1c-7663-be10-cde0e2e7660a` / Halley appended and repaired Pass 08/20, sub-agent `019f32d4-dc5e-75b2-ae74-93906ac9e295` / Planck appended and repaired Pass 09/20, sub-agent `019f32de-b445-7ab0-b177-dbd3d57d35c2` / Bohr appended and repaired Pass 10/20, sub-agent `019f32e6-b77b-7303-8f11-18e35a30c87f` / Heisenberg appended and repaired Pass 11/20, sub-agent `019f32ef-7e39-76a3-bc47-6cd59e38c98d` / Dalton appended Pass 12/20 with parent-reconciled id, sub-agent `019f32f5-d666-7da2-b7c1-f41d50ed2efe` / Kuhn appended Pass 13/20 with parent-reconciled id, sub-agent `019f32fa-8789-75a1-8585-e76d70aed5e1` / Herschel appended Pass 14/20 with parent-reconciled id, sub-agent `019f3300-d6f3-77b3-b934-73774f0b6f23` / Pauli appended Pass 15/20 with parent-reconciled id, sub-agent `019f3306-20a9-7bb2-895f-209632b6b4b6` / Parfit appended Pass 16/20 with parent-reconciled id, sub-agent `019f330c-8d8c-77c2-a996-0f7f74728d3c` / Ptolemy appended Pass 17/20 with parent-reconciled id, sub-agent `019f3318-5a3f-79b2-b7d0-b161de96bc56` / Kant appended Pass 18/20 with parent-reconciled id, sub-agent `019f331f-7b8b-72c3-bd7a-d1967e64ca31` / Hooke appended Pass 19/20 with parent-reconciled id, and sub-agent `019f3329-0b01-7fd0-b684-241d130f0f5f` / Faraday appended Pass 20/20 with parent-reconciled id; parent readback verified one heading per pass, exact pass numbers 01-20, count line, no identity placeholders, no conflict markers, and no configured mojibake markers. This closes the llm mvp 20-pass target artifact only; it does not close the 40-pass llm_mvp_core requirement or overall plan readiness.

Required plan targets:

- `active-codex-execution-02-source-inventory-and-reference-map.md`
- `active-codex-execution-07-execution-gates-open-questions.md`
- `active-codex-execution-plan-14-stage-references.md`
- `active-codex-execution-REQUIREMENTS-COVERAGE-MATRIX.md`

Acceptance rule:

- exactly 20 focused-pass records, not a summary row;
- each pass names root folder, file slice, source line/range evidence, why it matters, accepted/rejected/deferred status, and target plan section;
- every pass has a readback-verifiable section heading;
- no pass count can be reduced unless there is an explicit current Luba quote authorizing the narrower floor.

### FABLE-BLOCKER-02 - R-B 40-pass llm_mvp_core docs+code knowledge library

Problem:
The active plan has seed inventory only; it does not contain an accepted >=40-pass library over `C:\Projects\llm_mvp_core` docs+code.

Required active target artifact:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-knowledge-library-llm-mvp-core-40-pass.md
```

Current artifact state (2026-07-05): target file is now materialized with the required pass-record schema and source-root queue. Parent-verified accepted count is `accepted_focused_pass_records = 40/40` after sub-agent `019f333b-5176-7051-bd05-10c0034c541d` / Sagan appended Pass 01/40 with parent-reconciled id, sub-agent `019f3343-67aa-77c1-b79f-68fd42c9be21` / Nietzsche appended Pass 02/40 with parent-reconciled id, sub-agent `019f334c-58ef-71a1-96c1-777446f9ea4f` / Linnaeus appended Pass 03/40 with parent-reconciled id, sub-agent `019f3354-d8ce-7600-8d19-88be761cb908` / Godel appended Pass 04/40 with parent-reconciled id, sub-agent `019f335c-b028-7ac2-b722-db377d6c7ff1` / Avicenna appended Pass 05/40 with parent-reconciled id, sub-agent `019f3363-f418-7002-8c9b-dbfdc7594aeb` / Mendel appended Pass 06/40 with parent-reconciled id, sub-agent `019f336b-b007-75c1-984a-7f99c43ed496` / Russell appended Pass 07/40 with parent-reconciled id, sub-agent `019f3373-bbca-7c91-beeb-59414a0c7ebf` / Ramanujan appended Pass 08/40 with parent-reconciled id, sub-agent `019f337a-b588-79c3-b0d5-43c05c8e1a0e` / Sartre appended Pass 09/40 with parent-reconciled id, sub-agent `019f3382-4c32-7391-a973-0d4fc5fc8cb3` / Epicurus appended Pass 10/40 with parent-reconciled id, sub-agent `019f3389-2ce3-7160-87f5-adcbc1fc31f4` / Zeno appended Pass 11/40 with parent-reconciled id, sub-agent `019f3392-022d-7573-9a3a-1138b0be4111` / Bernoulli appended Pass 12/40 with parent-reconciled id, sub-agent `019f3398-02d5-7682-bc29-f43cda643f06` / Feynman appended Pass 13/40 with parent-reconciled id, sub-agent `019f339e-ff79-7d73-a833-6ae05dd94a52` / Mill appended Pass 14/40 with parent-reconciled id, sub-agent `019f33a7-494f-7e91-be5d-2f889dc1b3ae` / McClintock appended Pass 15/40 with parent-reconciled id, sub-agent `019f33ae-5614-7b21-89a4-d12dbceffbee` / Curie appended Pass 16/40 with parent-reconciled id, sub-agent `019f33b6-f37e-7bd1-a409-ce8f93145808` / Copernicus appended Pass 17/40 with parent-reconciled id, sub-agent `019f33bc-a560-73e1-b67f-d7461aaec96a` / Kepler appended Pass 18/40 with parent-reconciled id, sub-agent `019f33c7-25ff-7fd1-9193-d1c7416ea2bb` / Schrodinger appended Pass 19/40 with parent-reconciled id, sub-agent `019f33cf-254c-7741-aaab-bc717cc7e171` / Bacon appended Pass 20/40 with parent-reconciled id, sub-agent `019f33dd-4f7d-7592-a758-ead84d0cd273` / Noether appended Pass 21/40 with parent-reconciled id, sub-agent `019f33e9-fbf8-7912-891a-5e5eec4f772b` / Turing appended and repaired Pass 22/40 with parent-reconciled id, sub-agent `019f33f7-d29f-7192-a2d0-671d0f61f4c5` / Franklin appended Pass 23/40 with parent-reconciled id, sub-agent `019f3401-7237-7340-9e13-3efa7feeb64d` / Peirce appended Pass 24/40 with parent-reconciled id, sub-agent `019f3409-f9ce-7123-b71c-2e0e2d03c36c` / Helmholtz appended Pass 25/40 with parent-reconciled id, sub-agent `019f3413-3406-7dd1-b10b-6918f19aa24c` / Lagrange appended Pass 26/40 with parent-reconciled id, sub-agent `019f341b-655b-7862-b0dc-e38242cf7453` / Tesla appended Pass 27/40 with parent-reconciled id, sub-agent `019f3423-4070-7c10-9124-89020607305c` / Meitner appended Pass 28/40 with parent-reconciled id, sub-agent `019f342c-0bac-7d43-9dd2-1c02b4237438` / Kierkegaard appended Pass 29/40 with parent-reconciled id, sub-agent `019f3434-1209-7170-8ee0-1392a2693f8e` / Gibbs appended Pass 30/40 with parent-reconciled id, sub-agent `019f343e-48d1-7882-b053-485f31a64d09` / Anscombe appended Pass 31/40 with parent-reconciled id, sub-agent `019f3446-0f18-78c1-8deb-1010ebe360a2` / Carver appended Pass 32/40 with parent-reconciled id, sub-agent `019f344d-04a5-7da0-8fad-b077e8d113e1` / Hegel appended Pass 33/40 with parent-reconciled id, sub-agent `019f3454-38cf-76a2-b946-023a94c48be7` / Lovelace appended Pass 34/40 with parent-reconciled id, sub-agent `019f3459-f6b6-71f0-a984-e7db71482a3f` / Pasteur appended Pass 35/40 with parent-reconciled id, sub-agent `019f3460-102c-7072-ae0b-29f5f3a36ff9` / Goodall appended and repaired Pass 36/40 with parent-reconciled id, sub-agent `019f3469-2891-7e02-ad38-95308520f79a` / Confucius appended Pass 37/40 with parent-reconciled id, sub-agent `019f3470-e6f3-7e43-b96b-8328f0c1b4bd` / Poincare appended Pass 38/40 with parent-reconciled id, sub-agent `019f347b-49ec-75d1-b900-462d2b8bfd8a` / Beauvoir appended Pass 39/40 with parent-reconciled id, and sub-agent `019f3481-5eda-7630-b6a8-c8ba2204f184` / Lorentz appended Pass 40/40 with parent-reconciled id; parent readback verified count line, exact headings 01-40, no identity placeholders after parent reconciliation, detectable evidence/synthesis tables, no conflict markers, and no configured mojibake markers after Pass22 repair and Pass23-Pass40 acceptance. This closes only the llm_mvp_core 40-pass knowledge-library target; it does not close product readiness, B-22, implementation authorization, tests/builds/runtime/provider/training/checkpoint/git proof, or active plan consumer synchronization.

Required plan targets:

- `active-codex-execution-02-source-inventory-and-reference-map.md`
- `active-codex-execution-07-execution-gates-open-questions.md`
- `active-codex-execution-plan-14-stage-references.md`

Acceptance rule:

- exactly 40 focused-pass records across docs and code;
- each pass names folder/project, files read, evidence lines, code/doc status, decision supported, and target plan consumer;
- implementation claims require implementation or test evidence, not only design docs;
- no ledger-only count can close the requirement.

### FABLE-BLOCKER-03 - R-B floor downgrade without Luba authority

Problem:
The Fable check found Pass43 language that demoted the explicit >=20/>=40 floor to optional/future without a current Luba quote.

Repair:

- restore the >=20/>=40 floor as binding before readiness/implementation; or
- add a clearly named Luba decision row if Luba explicitly approves a narrower v1 scope.

Required plan targets:

- `active-codex-execution-F0-decision-unblocker.md`
- `active-codex-execution-16-full-task-coverage-simulation-gate.md`
- `active-codex-execution-18-pass27-original-task-coverage-reconciliation.md`
- `active-codex-execution-REQUIREMENTS-COVERAGE-MATRIX.md`

Current default:

```text
B-21 status: RESOLVED_FROM_TASK_MINIMUM (per S17-B04 / F0 Q-B21-FLOOR, owner active-codex-execution-F0-decision-unblocker.md:805-857; прежнее LUBA_PRODUCT_DECISION_RESIDUAL_OR_REQUIRED_LIBRARY_WORK = historical/pre-S17-B04). Floor 20/40 = документированный task-минимум, MET at 20/20+40/40, stays binding; открытого «narrow the floor» Luba-решения нет.
```

No downstream worker may treat the 20/40 libraries as optional unless a current unquoted Luba instruction says so.

### FABLE-BLOCKER-04 - R-G source addressability

Problem:
Before the 2026-07-05 repair, named source materials were not all resolvable from active-codex alone. The worst gap was `Planning sessions\session-start-extracted`, which had no active row, no path, no recon target, and no navigation row. Current repair status: exact physical paths are now recorded and existence-checked; navigation/library rows still remain open before R-G can close.

Required active source-addressability table:

| Short name | Required physical path | Status |
|---|---|---|
| `Small model racional` | `C:\Users\lubao\Documents\xiigen\llm mvp\Small model racional` | path row required |
| `language translate learning` | `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning` | path row required |
| `local wiki learning` | `C:\Users\lubao\Documents\xiigen\llm mvp\local wiki learning` | path row required |
| `extensions` | `C:\Users\lubao\Documents\xiigen\llm mvp\extensions` | path row required |
| `documentation learning` | `C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning` | path row required |
| `Planning sessions/session-start-extracted` | `C:\Users\lubao\Documents\xiigen\Planning sessions\session-start-extracted` | exact original-task path row required; previous `llm mvp\Planning sessions` path was wrong |
| `xiigen-platform-dot-net-angular` | `C:\Projects\xiigen-platform-dot-net-angular` | exact host/facade repo path row required |
| `xiigen-flows-dot-net-angular` | `C:\Projects\xiigen-flows-dot-net-angular` | exact FLOW-31/FLOW-38 seam repo path row required |
| `FigmaToCode-design-gen` | `C:\Projects\FigmaToCode-design-gen` | repo path row required |
| `CanvaToCode` | `C:\Projects\CanvaToCode` | client/caller repo row required; not host authority |

Current read-only path check (2026-07-05, Codex parent/direct evidence):

| Short name | Path | Existence |
|---|---|---|
| `Small model racional` | `C:\Users\lubao\Documents\xiigen\llm mvp\Small model racional` | `PATH_EXISTS_VERIFIED` |
| `language translate learning` | `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning` | `PATH_EXISTS_VERIFIED` |
| `local wiki learning` | `C:\Users\lubao\Documents\xiigen\llm mvp\local wiki learning` | `PATH_EXISTS_VERIFIED` |
| `extensions` | `C:\Users\lubao\Documents\xiigen\llm mvp\extensions` | `PATH_EXISTS_VERIFIED` |
| `documentation learning` | `C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning` | `PATH_EXISTS_VERIFIED` |
| `Planning sessions/session-start-extracted` | `C:\Users\lubao\Documents\xiigen\Planning sessions\session-start-extracted` | `PATH_EXISTS_VERIFIED` |
| `wrong historical session path` | `C:\Users\lubao\Documents\xiigen\llm mvp\Planning sessions\session-start-extracted` | `PATH_NOT_FOUND_VERIFIED` |
| `xiigen-platform-dot-net-angular` | `C:\Projects\xiigen-platform-dot-net-angular` | `PATH_EXISTS_VERIFIED` |
| `xiigen-flows-dot-net-angular` | `C:\Projects\xiigen-flows-dot-net-angular` | `PATH_EXISTS_VERIFIED` |
| `wrong historical platform short path` | `C:\Projects\xiigen-platform` | `PATH_NOT_FOUND_VERIFIED` |
| `wrong historical flows short path` | `C:\Projects\xiigen-flows` | `PATH_NOT_FOUND_VERIFIED` |
| `FigmaToCode-design-gen` | `C:\Projects\FigmaToCode-design-gen` | `PATH_EXISTS_VERIFIED` |
| `CanvaToCode` | `C:\Projects\CanvaToCode` | `PATH_EXISTS_VERIFIED` |

Required plan targets:

- `active-codex-execution-02-source-inventory-and-reference-map.md`
- `active-codex-execution-plan-14-stage-references.md`

Acceptance rule:

- every named source folder has exact path, inventory/navigation row, and target plan consumer; or
- a current Luba removal decision is recorded.

## Partial Repairs To Propagate

### FABLE-PARTIAL-01 - Single order taxonomy and GraphOrderDecision canon

Target:
`active-codex-execution-plan-04-dynamicgraph-machine.md`

Repair:
Label the pre-Pass05 order-taxonomy/GraphOrderDecision render as historical, then name exactly one current canon and owner pointer.

### FABLE-PARTIAL-02 - IQueryable GraphNodeCandidate propagation

Targets:

- `active-codex-execution-plan-02-edge.md`
- `active-codex-execution-plan-03-orchestrator.md`
- `active-codex-execution-plan-05-partA-hierarchy.md`

Scope-repair note (2026-07-07, added): `plan-02-edge.md` was previously omitted
from this propagation queue. It is the node's DIRECT consumer (`relation between
units`): the many->many unit-to-unit handoff wires `IDynamicGraphNode.ExecuteAsync`
output (`IQueryable<GraphNodeCandidate>`) into `IDynamicGraphEdge.TraverseAsync`
input, so dropping it leaves the node->edge type seam undefined even after full
execution of this file. It is now a required propagation target.

Repair:
Propagate the owner canon:

```text
unit public output = IQueryable<GraphNodeCandidate>
runner materialization boundary = AsyncCandidateBatch
owner pointers = plan-01 section 5 / PASS10 and plan-06 runner boundary
```

`plan-02-edge.md` §5 specific propagation: add the owner-canon pointer block
above, then either extend `EdgeTraversalInput` with the upstream candidate set
(or its refs) or name an explicit `NodeOutput -> EdgeTraversalInput` adapter
record + projection rule, so the node->edge boundary is typed against the
`GraphNodeCandidate` world instead of the foreign SubPhaseRouter world. This
edit lands inside `plan-02-edge.md` itself (a separate propagation-target file),
not in this repair-queue file.

### FABLE-PARTIAL-03 - DynamicGraphNodeDescriptor domain carrier

Target:
`active-codex-execution-plan-01-node.md`

Repair:
Either add `DomainTag` to the descriptor canon or correct the passport analogy so domain lives only in `LogicalUnitRequest.DomainTag`. The plan must not imply a domain leg exists in the descriptor if it does not.

### FABLE-PARTIAL-04 - Four-role summary parity

Target:
`active-codex-execution-06-ilearningdynamicgraph-architecture-brief.md`

Repair:
Add the missing flow row:

```text
ILearningFlow = ILogicalFlow / DynamicGraphEdgeFlow
role = relation/correction between units
trainability = no direct Train*; trainable authority belongs to EdgeAuthorityUnit or owning machine
```

### FABLE-PARTIAL-05 - Human-readable active plan encoding

Targets:

- `active-codex-execution-plan-00-partA-uml-status.md`
- `active-codex-execution-plan-01-node.md`
- `active-codex-execution-plan-02-edge.md`
- `active-codex-execution-plan-03-orchestrator.md`
- `active-codex-execution-plan-04-dynamicgraph-machine.md`
- `active-codex-execution-plan-05-partA-hierarchy.md`
- `active-codex-execution-06-ilearningdynamicgraph-architecture-brief.md`

Repair:
Run one bounded mechanical UTF-8/mojibake repair over active-codex Part A human-readable Russian sections. Content changes are forbidden during this pass.

Acceptance:

- marker-name scan for the configured UTF-8/Windows-1252 corruption markers and replacement-character marker is zero in changed human-readable current sections;
- sampled Russian readback is clean Cyrillic;
- no plan facts changed except encoding repair.

### FABLE-PARTIAL-06 - Trainable DPO builder update algorithm and lifecycle

Targets:

- `active-codex-execution-plan-07-dpo-builder.md`
- `active-codex-execution-plan-10-training-lifecycle.md`
- `active-codex-execution-plan-04-dynamicgraph-machine.md`

Repair:
Add exact algorithm:

```text
input GraphDpoTrainingRow
-> validate legal/provenance envelope
-> extract unit-level chosen/rejected candidates
-> update GraphDpoBuilderUnitState counters/weights/reward margins
-> write checkpoint delta
-> evaluate held-out lift
-> promote only if lift >= threshold and no legal/provenance regression
```

Also add named lifecycle rows for `TrainableGraphDpoBuilderUnit` and `GraphDpoBuilderUnitState`.

### FABLE-PARTIAL-07 - AllowedTeacherRegistry canonical_license

Targets:

- `active-codex-execution-plan-08-legal-teacher-gate.md`
- `active-codex-execution-04-dpo-legal-model-policy.md`

Repair:
Add `canonical_license` per allowlisted family:

```text
Qwen3 = Apache-2.0
DeepSeek = MIT or explicit per-release permissive license proof
Kimi = modified-MIT / provider-terms check required
GLM = per-release license proof
Mistral = heterogeneous_per_release
```

No family becomes train-ready without provider terms proof and license proof.

### FABLE-PARTIAL-08 - Async multi-run scheduling semantics

Target:
`active-codex-execution-plan-06-async-combinations.md`

Repair:
Name whether N CombinationPlans run concurrently, sequentially, or bounded-concurrently. Bind `AsyncCombinationBudget.MaxParallelBranches` to branch count vs plan count.

### FABLE-PARTIAL-09 - Threshold/routing floors

Target:
`active-codex-execution-plan-12-no-hidden-determinism.md`

Repair:
Every runtime routing floor must be either locked or explicitly per-carrier/tunable. It cannot be implied by adjacent 0.02/0.95/0.025 scorer thresholds.

### FABLE-PARTIAL-10 - Guide-to-plan traceability

Targets:

- `active-codex-execution-plan-10-training-lifecycle.md`
- `active-codex-execution-plan-14-stage-references.md`

Repair:
Add source rows mapping `active-codex-execution-guide-02-how-to-train.md` sections to plan-10 lifecycle phases.

## Governance Repairs Required

The following rules must exist in relevant skill/guide/agent files:

1. A later Fable/Claude/other-model full-task simulation with MISSING/PARTIAL evidence downgrades earlier readiness overlays.
2. A plan-file family must have single-owner consistency across owner and consumer carriers.
3. Numeric pass floors such as >=20/>=40 cannot be lowered by a downstream skill or worker without a current Luba quote.
4. Every named source material needs exact physical path plus inventory/navigation row before readiness.
5. Human-readable architecture artifacts need mojibake admission checks, not only ledger-entry mojibake checks.
6. Per-pass deliverable slices must be defined before claiming N-pass library work.

## Re-Review Gate

After the blocking repairs are complete, run one bounded final plan review with the loaded architecture/planning/plan-review/human-gate stack.

The re-review may upgrade readiness only if:

- the 20-pass and 40-pass library artifacts exist with accepted records, or a current Luba quote narrows the floor;
- all source-addressability rows have exact paths and inventory/navigation targets;
- all Fable partials above are either repaired or explicitly classified nonblocking with source evidence;
- stale current-looking readiness/status lines are historical;
- current authority split is `implementation_authorization: YES_CODE_FIRST_ONLY` for the code-first lane and `teacher_dpo_training_authorization: NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL`; the former blanket `NO until fresh Luba approval after review` is historical_pre_code_first only.
