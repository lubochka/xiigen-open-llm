# Generated Code Review

## Exact Final SHA Publication Integrity Gate

For S04 and any slice adopting the publication-integrity contract, code/evidence review must reject an `ACCEPT` claim unless:
- the required independent review is PASS for the relevant candidate;
- pack/static/focused/impacted checks required by the task are bound to the exact final SHA;
- any merge/rebase/reconciliation result was revalidated at that exact HEAD;
- no committed textual conflict markers remain;
- no accidental generated caches such as `__pycache__` / `*.pyc` are tracked in the slice pack;
- the publication receipt says remote SHA equals the published commit SHA.

A scratch composite, parent commit, pre-merge test run, or clean Git index is insufficient evidence for a later merge HEAD.

`MERGE_RESULT_MUST_BE_VALIDATED_AT_EXACT_HEAD`
`TRACKED_PYTHON_CACHE_IS_HARD_FAIL`
`SCRATCH_OR_PARENT_PASS_DOES_NOT_VALIDATE_MERGE_HEAD`


## Учебный тест и реальная приёмка — правило Любы от 3 ноября 2026

Для аудита/ремонта учебного теста и переноса обязателен [разбор причины и нативная приёмка](../plans/replanning-possible-diffs/experimets/45-learning-claim-and-native-acceptance-contract-2026-11-03.md). Архитектор не исправляет отсутствие нужных уроков ручным семантическим исключением в продукте и не объявляет опасный наблюдённый ответ несуществующим. Он отделяет дефект постановки от дефекта контракта, назначает отдельный опыт с TRAIN/EXAM и сохраняет исходное наблюдение.

При назначенном переносе, аудите кода/QA, приёмке или ремонте тестов исполнитель обязан **действительно собрать затронутые проекты и тестовый проект, запустить проверки на финальном исходнике и проверить discovery**. Обоснованная адаптация тестов к уже утверждённому договору входит в это поручение; отдельное одобрение каждой правки assertion не требуется. Нельзя подгонять expected под дефект продукта. UNRUN не закрывает приёмку; при отказе передать архитектору команду, версию, журнал, expected/actual и следующий шаг. Слово «аудит» не включает READ_ONLY_RESEARCH автоматически. Прямое ограничение «только поиск/документы, не запускать» действует только в своей задаче. [Навыки по этапам, свежие результаты и примеры46](../plans/replanning-possible-diffs/experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md) обязательны вместе с45; не использовать старую DLL/TRX и не завершать поручение вместо ожидания.

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Fresh Execution / Method / Humility Code-Review Hook

Generated-code review must reject stale-method and overfast evidence:
- `REPEATED_CURRENT_COMMAND_IS_FRESH_EXECUTION_GATE`: repeated current command is a fresh execution trigger; old artifacts are baseline evidence, not completion.
- `TOO_FAST_HIGH_PASS_SUSPICION_GATE`: too-fast 50-pass completion is suspicious until per-pass sub-agent evidence is accepted.
- `LARGE_VOLUME_MODEL_HUMILITY_AND_FOCUS_GATE`: large-volume model humility requires bounded slices and focus ledger before completion.
- `EXPLICIT_METHOD_OVERRIDE_GATE`: Luba's method/model/reasoning/count/folders/target/sub-agent route is binding; old code/package evidence cannot replace the requested route.
- `FINAL_ANSWER_FRESHNESS_GATE`: review/final text must answer the latest command and state freshly executed this turn vs only verified.
- `CURRENT_REPEATED_PACKAGE_COMMAND_GOVERNANCE_ADDENDUM`: generated-code review must reject repeated extension-development package closure by old artifact verification instead of a fresh execution chain; too-fast N-pass/status-package claims without ordered packets/records; missing large-volume model humility focus ledger with exact current task, not-the-task, files, pass/review counts, active worker, remaining actions, and latest-command freshness; parallel workers when Luba flags parallel/too-fast or requires exact-count sub-agent work unless her current unquoted message authorizes parallelism; interrupted or zero-work spawned-worker packets using `NONE` instead of `subagent_id: UNKNOWN_IN_SUBAGENT` when platform id is hidden; parent local sidecar work during waiting; missing human-readable phase counts before execution continuation; and final/status without latest-command, fresh-vs-old execution, actual packets/records, no parent self-review counted, and no product-readiness overclaim.
- `COMMAND_SCOPE_EXACTNESS_GATE`: generated-code review must reject current-task command violations. Current `allowed_commands` and `forbidden_commands` are binding; read-only convenience commands, alternate git diff modes, external AI/web, `codex.exe`, nested sub-agents, branch/git mutations, or shell variants outside the current allowed list are violations unless Luba's current unquoted instruction explicitly authorizes them. Evidence from an out-of-scope command is quarantine evidence only and cannot support completion.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## Pass172 Generated-Code Review Firewall

Generated-code review must not accept code, packaging, branch, or ProductReady
claims unless the required governance and QA/code-review stack is loaded and the
claim stays inside executed evidence.

## Code-First Claim Review Gate

Apply this at every agent-work stage that touches code: work-order admission,
worker return packet review, generated-code review, phase review, claim review,
and final packaging. Do not wait until before-completion.

For any generated-code, runtime, model, extension, DPO, trainable unit, or
product-delivery claim, review the code path first. Reject any claim whose
evidence is only a plan, README, status package, ledger, JSON manifest,
screenshot, generated report, or self-report.

Require the implementing agent to provide:

```text
Implementation Evidence Map:
  user_task_requirement:
  claimed_capability:
  implementation_location: <repo/project/file/class/method/entrypoint>
  exact_file_line_refs:
  call_chain: <host/API/runner -> service/machine -> logic/state/model>
  changed_code_files:
  tests_or_runtime_commands_that_exercise_call_chain:
  expected_observable_behavior:
  actual_observed_behavior:
  positive_case_and_negative_or_retention_case:
  no_code_algorithm_and_domain_purpose:
  validation_storage_vs_learning_vs_measured_training_boundary:
  architecture_flow_diagram_or_simulation_when_changed:
  smallest_remaining_work_before_first_training_run:
  measurement_and_reuse_readiness_remaining_work:
  published_state_vs_pending_state:
  stub_not_ready_scan_terms:

Independent Verification Work Order:
  verifier_role: separate reviewer/sub-agent
  files_to_read:
  commands_or_tests_to_run:
  expected_evidence:
  falsification_conditions:
```

Use these verdicts when applicable: `CODE_IMPLEMENTED`, `ARTIFACT_ONLY`,
`SCAFFOLD_ONLY`, `NOT_WIRED`, `TEST_ONLY`, `STATIC_REPORT_ONLY`,
`UNKNOWN_NO_CODE_TRACE`.

Architect/reviewer authority is read-only for this gate: inspect code, compare
the claim to the real path, report violations to Luba, and issue the next
bounded fix/verification work order. Do not silently repair product code while
reviewing the claim.

Reject or downgrade when:
- `skill_load_status` is missing for AGENTS.md, startup/session, plan-review,
  claim-verification, QA session type, generated-code-review, and
  capability-design-review;
- NEEDS_FIX, timeout, tool error, usage limit, partial packet, audit downgrade,
  or stale status row is treated as a stop instead of a repair queue;
- quoted transcript/attachment/state/old plan/sub-agent text creates code scope,
  git scope, cleanup scope, branch authority, or implementation authority without
  exact current unquoted reactivation;
- parent directly edits product/governed files while sub-agent-only or
  orchestrator-only mode is active, except allowed orchestration, returned-packet
  verification, and exact stale cleanup;
- a plan-only or governance-only patch is claimed as product execution,
  project completion, or code readiness;
- `governance repair is not product continuation evidence`: a governance/skill/
  guide/agent-file repair, verification, or no-change result does not verify the
  next product/code/QA execution slice.
- `rule presence is not enforcement`: code review must reject a final/status
  packet that says a governance rule already exists, reports `NO_PATCH`, or
  reads back unchanged rule text as if that were code/QA execution evidence.
- `NO_PATCH from a governance worker is not product progress`: if product/code/QA
  work remains, review must require the next product/code/QA worker or narrowed
  recovery worker plus human status fields `next_product_code_slice`,
  `current_worker_or_recovery_worker_id`,
  `current_worker_or_recovery_worker_status`, and
  `accepted_product_packet_count`. Otherwise classify
  `false_stop_rule_presence_not_enforcement`.
- `pre_final_continuation_enforcement_required`: generated-code review must
  require pre-final fields before any status, review, no-safe-work, completion,
  hold, PASS, or final packet: active task open, next bounded product/review/
  governance action, pending packet verification, required passes remaining,
  next worker started/waited this turn, and true external blocker certificate.
- `parent_must_run_pre_final_check_before_status_final`: after governance repair,
  rule readback, or `NO_PATCH`, review must reject status/final without the
  pre-final continuation check.
- `final_packet_forbidden_when_next_bounded_action_exists`: final/status/
  completion/hold/no-safe-work is forbidden while next bounded work, pending
  verification, or pass deficit remains and no true external blocker is proven.
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`:
  governance repair can be accepted only with same-chain proof that the next
  product/review worker was started, waited for, or reviewed, or with true
  external blocker proof.
- `after_governance_repair_spawn_or_wait_next_product_review_worker`: continue
  to the next product/review slice named by the current unquoted work order;
  stopping after governance repair is `premature_final_after_governance_repair`.
- `NO_PATCH_requires_product_continuation_check`: `NO_PATCH`, PASS, NEEDS_FIX,
  timeout, usage limit, partial packet, worker silence, and stale status route to
  continuation/recovery, not stop permission.
- `rule_text_present_but_not_enforced_is_failure`: rule text is not enforcement
  unless final/status acceptance is blocked when next bounded action exists.
- `PRODUCT_CODE_CONTINUATION_ONLY`: after governance/skill/rule/agent-file
  repair for a live product/code/QA task, generated-code review must reject
  final/status unless the same continuation chain starts, waits for, or reviews
  the product/code/QA execution worker or narrowed recovery worker for the
  concrete `audit_union_row_id` or `next_product_code_slice`. Another
  governance worker, read-only scan, status package, `NO_PATCH`, rule readback,
  or packet-formatting repair is not product/code continuation.
- `PRODUCT_WORKER_NO_PACKET_ZERO_ACCEPTED_REQUIRES_RECOVERY`: `shutdown`,
  `not_found`, interrupted, no packet, or 0 accepted product evidence from the
  product/code worker is zero accepted product work and requires a narrowed
  product/code recovery worker before final/status.
- `subagent_wait_heartbeat_required_before_wait`: before each wait/poll, require
  a human-readable heartbeat naming the worker, unaccepted result, and next
  wait/recovery step.
- `one_active_agent_does_not_mean_wait_silently`: one-active-agent sequencing
  requires bounded heartbeat waits; silent waiting fails review.
- bounded local/safe-worktree evidence is restated as ProductReady, cloud/deploy,
  provider execution, public endpoint, production browser farm, CI, PR, clean
  package restore, or merge readiness without exact executed evidence;
- stale NOT_READY/NEEDS_FIX/G1/G2/current-status labels remain unmarked as
  historical/pre-repair/evidence-only after superseding evidence;
- mergeability/latest-authority checks mutate a source branch or publish git
  changes without current authorization. Use no-write mergeability into
  original/base and main for branch-readiness review.

## When to Apply

Apply this skill to all code produced by:
- `SemanticCompilerPipeline` → `PipelineOutput.Predicates[]`
- `TemplateBeautifier` → formatted template strings
- Future: `17.DynamicDomains` → C# type definitions
- Any trainable `ILogicalUnit` / `ILearningMachine` deliverable (e.g., `SkillSelectionMachine`, `PromptToTaskMachine`, `OrchestrationReasoningMachine`, `ResultGapMachine`) — apply ALL layers including Layer 5 below
- Any deliverable a plan calls "trainable", "learned", "model", or "DPO" — Layer 5 is mandatory

Generated code cannot be verified by inspection alone (see self-verification-SKILL.md Rule 5).
This skill defines the 5-layer review that replaces visual inspection.

For trainable-unit deliverables, contract/build/DNA evidence is NOT sufficient — Layer 5 (numeric metrics) is required. This is the V20 anti-stub jurisdiction fix.

---

## Layer 1 — Semantic Review

Check that names and comments reflect intent, not implementation accidents.

```csharp
// Review questions for each generated class/method:
// 1. Does the class name describe what it represents (not what generated it)?
// 2. Does every method name describe the action, not the algorithm?
// 3. Are there any placeholder names? ("Item1", "Temp", "Result1")
// 4. Are comments present on every line? (Rule 3)
// 5. Are all abbreviations expanded? (Rule 9)
```

**Gate B for Layer 1:**
```bash
# Check for placeholder names
grep -n "Item[0-9]\|Temp[0-9]\|Result[0-9]\|TODO\|FIXME\|PLACEHOLDER" [generated-file.cs]
→ 0 matches
```

---

## Layer 2 — DNA Compliance

Run the 5 DNA grep checks against the generated file:

```bash
# Rule 1: No throw for business logic
grep -n "throw new" [generated-file.cs]
→ 0 matches (or only in truly exceptional paths with comment explaining why)

# Rule 2: No direct fabric instantiation
grep -n "new.*Reader\|new.*Writer\|new.*Storage\|new.*Index" [generated-file.cs]
→ 0 matches in generated business logic

# Rule 3: Every non-blank line has a comment
# Manual check: open file, verify no uncommented production lines

# Rule 1 continued: OperationResult<T> return types
grep -n "OperationResult" [generated-file.cs]
→ Count: every public method should have at least one match

# Rule 10: No ML framework imports
grep -n "using.*TorchSharp\|using.*ML\.NET\|using.*TensorFlow" [generated-file.cs]
→ 0 matches
```

**Gate B for Layer 2:** All 5 grep checks return 0 (except Rule 3 which is manual).

---

## Layer 3 — Silent Failure (MACHINE Constant Test)

Generated code can produce output that looks correct but silently fails at runtime.
The security-break test pattern catches this.

For generated predicate code:

```csharp
// MACHINE constant test — verify the predicate logic, not just that it compiles
[Fact]
public void GeneratedPredicate_WithKnownInput_ProducesExpectedOutput()
{
    // Arrange: use a known input where the correct answer is pre-calculated
    var predicate = GeneratedPredicateFactory.Create("fieldName = 'value'");

    // Act: apply the predicate to data where the answer is known
    var matchData = new Record { FieldName = "value" };
    var noMatchData = new Record { FieldName = "other" };

    // Assert: MACHINE constants — the expected outputs are fixed, not derived from the generator
    Assert.True(predicate.Evaluate(matchData));    // Must match
    Assert.False(predicate.Evaluate(noMatchData)); // Must not match
}

// "Security break" variant — verify that the predicate FAILS when it should fail
[Fact]
public void GeneratedPredicate_WithNonMatchingInput_ReturnsFalse()
{
    var predicate = GeneratedPredicateFactory.Create("fieldName = 'value'");

    // This must return false — if it returns true, the predicate has a logic error
    var result = predicate.Evaluate(new Record { FieldName = "completely_different" });

    Assert.False(result); // If this passes, the generator produced broken logic
}
```

**Gate B for Layer 3:** Both the match test AND the non-match test pass.

---

## Layer 4 — Training Data Impact

Ask: does this generated pattern generalize, or does it only work for this specific case?

```
Review questions:
1. If this generator runs on 100 different inputs, will it produce 100 correct outputs?
   Or is this working "by accident" for this input?

2. Does the generated code rely on a specific value being hardcoded?
   grep -n "[\"'][a-zA-Z0-9_]{10,}[\"']" [generated-file.cs]
   → Hardcoded strings in business logic are a red flag

3. Does the generation algorithm handle edge cases?
   - Empty input
   - Single-element input
   - Maximum-length input
   - Input with special characters

4. Would this generated code be valid training data for future model improvement?
   (If not, the generator needs fixing before more examples are generated)
```

**Gate B for Layer 4:** At least 3 different inputs tested, all producing correct output.

---

## Layer 5 — Trainable vs Stub (MACHINE numeric-metric test)

A trainable unit can compile, pass its contract tests, and satisfy DNA while being a pure stub that never learns or infers. Layer 5 catches that.

Keep three claims separate in every review: validation/storage/reopen logic,
learned-state update logic, and measured training-run quality. A checkpoint,
manifest, retention test, nearest-neighbor example match, or constant green
case may validate persistence or retrieval, but it is not a measured training
run and must not be reported as training progress unless the unit changed
learned state and metrics were measured.

FAIL the review when the plan requires a trained algorithm but the delivered code shows any of:
- a status string such as `not_ready_*`, `*_no_inference_implemented`, `not_trained_*`, `loaded_state_ready_no_inference_implemented`, or an equivalent placeholder;
- a method that returns a fixed JSON/descriptor object instead of computing a decision from learned state;
- an empty or no-op training loop (TrainBatch that does not change any learned state);
- `NotImplementedException`, `throw` for the main path, `TODO`, `FIXME`, or `PLACEHOLDER` on the inference/training path;
- inference that is exact-source/exact-prompt lookup presented as generalization.

Required numeric evidence (reproducible from the runner; UNKNOWN is not allowed for a done claim):
- accuracy / exact / loss / reward-margin on a held-out set (numbers, not UNKNOWN);
- `cpu_ms`, `ram_mb`, row counts;
- train vs test identity sets of the same type (no replay leakage).

Gate B for Layer 5: numeric metrics present AND reproducible AND the unit changes its decision after training on new pairs. If metrics are UNKNOWN / validation_only / static / contract-only, Layer 5 = FAIL.

Cross-reference: verification-before-completion-SKILL.md "Runtime Evidence Firewall"; AGENTS.md OP-53; plan-review-SKILL.md FC-25; architect-behavior-classifier-SKILL.md FAIL-32; injected as part of SK-VERIFY.

### Layer 5A - Allowed Trainable Architecture Pattern

For any learned, semantic, trainable, adaptive, DPO, selector, router,
classifier, scorer, retrieval, KNN, dictionary, counter, or pattern-matching
claim or mechanism, classify the actual runtime mechanism before judging
quality.

Allowed architecture levels only:

- `ILearningMachine<TDomain>`;
- `ILogicalOrchestrator`;
- `ILogicalFlow<TInput>`;
- `ILogicalUnit<TInput,TOutput>`;
- `ILearningMachinePair<TPersonal,TGlobal>`.

Scorers, selectors, classifiers, routers, retrievers, detectors,
KNN/Jaccard/BM25 stores, dictionary/counter state, regex/keyword matching,
pattern matching, and private methods such as `TryDecide*`,
`ChooseBestPattern`, `SelectBestPattern`, and `CountMatchingSignals` are not
architecture patterns. They can pass review only as internal implementation
details inside one of the allowed levels and only when the report labels their
scope honestly.

Required output row:

```text
Allowed hierarchy: <one of the five allowed levels>
Non-level classification: <internal detail only / none>
Actual mechanism: <short human explanation + code quote>
Runtime authority: yes/no
Semantic claim: full/baseline/scaffold/eval-only/too strong
Verdict: OK_ALLOWED_LEVEL / OK_INTERNAL_DETAIL / BASELINE_MUST_BE_LABELLED /
NEEDS_ILOGICALUNIT_WRAPPER / FORBIDDEN_RUNTIME_SHORTCUT /
REPORT_CLAIM_TOO_STRONG
Concrete repair:
  current -> target:
  replacement allowed level:
  replacement class/unit/flow:
  replacement algorithm:
  input/output contracts:
  typed trained state:
  KNN/nearest methods and role:
  DPO/source roots and license gate:
  pair-building rule:
  storage route from current contract:
  backup/export/import/share:
  continue-training from checkpoint:
  affected sets/assets:
  affected tests:
```

Layer 5A fails if `Concrete repair` is generic. For a candidate-position
counter, the review must state whether final authority is removed or whether a
guard `ILogicalUnit` such as `CandidateOrderLeakageGuardUnit` will detect
candidate-order sensitivity by permutation and block position features.

Layer 5A also fails when a learned dictionary/counter/map is replaced only by a
class name. The review must show the typed state record and learning lifecycle.
For example, `_promptContextSignals` cannot be accepted as "fixed" by naming
`PromptContextSkillSelectionUnit` alone; the review must show how that unit
stores prompt/context weights, how KNN/nearest examples are used as support
evidence, how DPO pairs are built, and how the unit loads existing user/common
state before continue-training.

### Layer 5B - Non-Scalar Output Reward-Scalarization Code Check

When the reviewed trainable unit's chosen/rejected is NOT an atomic scalar
answer but a sequence, ranking, order, path, plan, or structured output (for
example a graph order, a candidate list, or an html+css string), the DPO
training-update primitive `margin = chosenReward - rejectedReward` cannot be
accepted until the code shows a concrete reward-scalarization function that maps
the multi-element output into a single reward scalar for each side.

FAIL Layer 5B when any is true:
- the code quotes/uses a DPO margin/reward primitive but has no named function
  that computes `reward(side)` from the actual chosen/rejected type (text ->
  reward, sequence -> reward, structure -> reward);
- `reward(side)` is only prose ("worse becomes rejected", "best by quality")
  with no field-by-field extraction in code;
- the reward extractor is missing a defined scale/calibration or a documented
  tie-disposition rule for equal-reward sides.

Required output row:

```text
output_arity = scalar | sequence | ranking | structure | free_text
reward_scalarization_function = <class.method file:line | MISSING>
reward_scale_or_calibration = <defined | MISSING>
tie_disposition_rule = <defined | MISSING>
verdict = OK | FAIL_MISSING_REWARD_SCALARIZATION
```

This is the code-review sibling of the Part A "reward-scalarization rule"
requirement: a non-scalar trainable output without an instantiated reward
extractor is admission-blocking, not a deferrable row.

### Layer 5C - Winner-vs-Losers Cardinality Code Check

When the reviewed unit produces N candidates (N > 1) but the canonical DPO pair
record carries a single `rejected` field (1:1), the code review must confirm the
implemented projection rule from N candidates to pair count. Verify the code,
not the simulation.

FAIL Layer 5C when:
- the producer emits N > 1 candidates but no code path reduces N losers to the
  pair shape (winner-vs-each-loser | winner-vs-single-worst | other named rule);
- the projection rule exists only as a 1-vs-1 illustrative test/comment while
  the locked candidate count is N > 2;
- the acceptance signal that selects the winner (chosen) is not named, or is the
  trained unit's own self-reported confidence (self-labeling loop).

Required output row:

```text
candidate_count_N = <int>
pair_cardinality_rule = winner_vs_each_loser | winner_vs_single_worst | other:<name> | MISSING
loser_reduction_code = <class.method file:line | MISSING>
chosen_acceptance_signal_source = <named source>
acceptance_signal_independent_of_trained_unit_self_confidence = true/false
verdict = OK | FAIL_CARDINALITY | FAIL_SELF_LABELING_LOOP
```

### Layer 5D - Determinism-Primitive Naming Code Check

Any code path whose claimed guarantee is reproducibility, determinism,
source-disjoint split, or cross-process stability must name the concrete
primitive in code. An identifier that merely contains `stable` or
`deterministic` (for example `stable_hash(...)`, `deterministic_split`) is NOT
instantiation.

FAIL Layer 5D when:
- a split/sample/hash guaranteeing determinism uses `String.GetHashCode` or any
  per-process randomized hash;
- the hash/PRNG/sort primitive is unnamed (no FNV-1a, SHA-256-truncated, seeded
  PRNG, or a sort with an explicit tie-break);
- the first cold-start iteration is not line-by-line computable (init rule for
  from-zero trained state unnamed, or the first before/after delta cannot be
  computed because the untrained baseline is not the honest not_ready scaffold).

Required output row:

```text
determinism_claim_present = true/false
named_primitive = FNV-1a | SHA-256-trunc | seeded-PRNG | sort+tiebreak | OTHER:<name> | MISSING
forbidden_platform_randomized_hash_used = true/false
cold_start_first_iteration_computable = true/false
verdict = OK | FAIL_UNNAMED_PRIMITIVE | FAIL_PLATFORM_RANDOM_HASH | FAIL_COLD_START_NONCOMPUTABLE
```

### Layer 5E - Seam-Sourced Provenance/Signal Extractor Code Check

When a provenance field or a training signal (for example
`GroundingWasUsed`, a judge `ScoreA/ScoreB`, or a teacher metadata flag) is
sourced from an external provider response/parser seam rather than from static
config, the code review must confirm the extractor itself is instantiated, not
only the return-field contract.

FAIL Layer 5E when:
- a seam-sourced provenance/signal field is consumed but no parser names the
  exact per-provider keys/JSON paths, normalization, and unknown-format branch;
- a per-side judge score feeds DPO but the RawOutput -> ScoreA/ScoreB extraction
  seam is missing;
- a grounding/anti-lie check reads only self-declared metadata and there is no
  compensating control or explicit residual note for silent grounding
  (retrieval happened but the response carried no metadata).

Required output row:

```text
seam_sourced_fields = [names]
per_field_extractor = <class.method file:line | MISSING per field>
unknown_provider_format_branch = <present | MISSING>
silent_grounding_residual_control_or_note = <present | MISSING | N/A>
verdict = OK | FAIL_MISSING_EXTRACTOR
```

### Layer 5F - Cross-File Contract Single-Owner Consistency Code Check

Before a trainable/DPO code slice can pass, when a consumer file names a
type/unit and attributes it to an owner block/file (NEW, owner block N), the
review must grep the named owner and confirm the owner file actually contains
THAT type with the SAME name, the SAME architectural classification
(`ILogicalUnit` vs helper class), and the SAME input type.

FAIL Layer 5F when:
- the named owner file does not contain the referenced type/unit name;
- the owner has a differently named or differently typed builder/unit
  (for example consumer references `IGraphDpoPairBuilderUnit(GraphDpoBuildInput)`
  but the owner contains only `GraphDpoBuilder(GraphDpoSourceRow)`);
- a cross-block/cross-file owner pointer for a delegate field
  (`_critic`, `_repair`, "canon in block 01") does not resolve to an actual
  instantiated contract + algorithm + status at the named owner;
- a locked numeric canon (for example a provenance field count) differs between
  the owner and any consumer view.

Required output row:

```text
named_type_or_unit = <name>
claimed_owner = <file/block>
owner_contains_same_name = true/false
owner_same_architectural_level = true/false
owner_same_input_output_type = true/false
locked_numeric_canon_matches_all_carriers = true/false | N/A
verdict = OK | FAIL_DANGLING_OWNER | FAIL_CONTRACT_MISMATCH | FAIL_LOCKED_CANON_DRIFT
```

The single-owner sweep must cover ALL carriers of a shared type or locked value:
the owner declaration, every consumer reference, every `note`/diagram render, and
every metric denominator. A locked value (for example TeacherProvenance = 20
fields, RR6) proven only in the owner but stale in a consumer carrier
(18/18 vs 20/20) is a Layer 5F FAIL because it silently flips a measurable gate
result such as `provenance_field_presence_rate`.

### Deterministic-Boundary Code Review Gate

Every code slice review must explicitly search for hidden deterministic
mechanisms posing as trainable behavior before it can pass. The review must
inspect the affected code path for:

- scorers, routers, selectors, classifiers, retrievers, or rankers that are
  final authority while the claim says "trainable";
- dictionaries, counters, maps, exact-prompt/source lookup, candidate-order
  weights, source/path/provenance shortcuts, or fixture replay;
- regex/BM25/Jaccard/KNN used as unlabelled final semantic authority;
- static thresholds or "first candidate wins" fallbacks hidden behind model
  wording;
- reports or tests that prove only a deterministic helper while claiming a
  trained unit.

Required output for each code slice:

```text
deterministic_boundary_review = PASS | FAIL
searched_mechanisms = scorer/router/lookup/counter/exact-replay/static-fixture
hidden_deterministic_mechanism_found = true/false
actual_runtime_authority:
allowed_level = ILogicalUnit | ILogicalFlow | ILogicalOrchestrator | ILearningMachine | ILearningMachinePair | internal detail only
claim_matches_mechanism = true/false
project_coding_standards_checked = true/false
allowed_tests_or_gate_b_checked = true/false
llm_mvp_core_core_logic_boundary_checked = true/false
host_adapters_thin_checked = true/false
deterministic_helpers_labelled_eval_only_helper_internal_or_baseline = true/false
deterministic_helpers_not_runtime_trainable_authority = true/false
docs_qa_dashboard_impact_checked_when_applicable = true/false
required_fix:
```

If a hidden deterministic mechanism is found, the review fails until the claim
is downgraded or the runtime authority is moved into an allowed architecture
level with lifecycle evidence: training data, checkpoint/manifest, export/
import, backup, fresh-load, continue-training, DPO quality, and no-shortcut or
ablation evidence.

The code review also fails if project coding standards, allowed tests, core
ownership in `llm_mvp_core`, thin host adapters, deterministic-helper labels, or
docs/QA/dashboard impact were not explicitly checked. A passing test result or
machine label alone is not a complete code review.

### Human-readable code review report gate

Before implementation or generated-code review can be accepted, the controlling
plan/report must pass the Human-Readable Architecture Artifact Gate. Reject code
review when the report is table-only, shortcode-heavy, English-only when Russian
is required, schema-only, or prompt/spec instead of an implementable blueprint.
For Luba-facing acceptance of a code slice, also apply the Product-Level Human
Readability Acceptance Gate from `output-readability-gate-SKILL.md`.

For trainable or architecture findings, the review must include:
- Russian human summary;
- what user-visible or product behavior exists now, with one example input/
  output or scenario result;
- PlantUML or Mermaid UML importable to draw.io when architecture/data flow is
  being explained;
- no-code algorithm explanation and domain purpose;
- expected behavior versus actual observed test/runtime result, including a
  positive case and a negative/error/retention case when relevant;
- explicit validation/storage versus learning-update versus measured-training
  distinction;
- smallest remaining work before first training run, and separate remaining
  work before measurement/reuse/readiness claims;
- current published state versus pending local/unpublished state;
- exact file:line quote plus `сейчас`, `почему плохо/ок`, `должно стать`;
- adjacent Concrete Repair Blueprint Gate fields for suspicious
  Dictionary/counter/KNN/scorer/selector/private method quotes.

Generic fixes such as "wrap in ILogicalUnit", "add KNN", "add tests", or
"chosen/rejected/quarantine" не является решением unless exact
classes/records/methods/tests/data rows, sets/checkpoints/manifests, and
lifecycle paths are named.

### Audit sufficiency and delta-severity code-review gate

Before accepting a code review that depends on an architecture/trainable audit,
verify that the audit did not overclaim its scope.

Reject the code review when:
- "v1 scope completed" does not name the exact bounded scope;
- "Architecture audit sufficient/complete", "достаточно", "готово", or
  "закрыто" is used without a green breadth challenge;
- the breadth challenge did not check confidence claims, export accuracy
  claims, target-label leakage, runtime routing confidence floors,
  source/path/provenance leakage, and fallback-to-first-entry/default-candidate
  shortcuts;
- parent review did not ask: "What would a broader audit likely find that this
  artifact missed?";
- a later same-domain audit found discoverable BLOCKING/HIGH issues and the
  review has no Delta-Severity Feedback Gate record explaining why the previous
  review missed them.

When this gate fails, the next action is governance/review repair, not code
approval or implementation polish.

### Independent audit union code-review gate

Generated-code and trainable-code review must reject any PASS/approval when a
later independent audit map for the same active product found implementable
holes that are not represented as code/test/runtime rows. Audit disagreement is
blind-spot evidence, not permission to keep only common findings.

Required review rows:

```text
INDEPENDENT_AUDIT_UNION_TO_PRODUCT_BOARD_GATE:
  audit_union_board_ref:
  reviewed_row_source_tag: BOTH | FABLE_ONLY | OPUS_ONLY | CLAUDE_ONLY | BENCHMARK_ONLY
AUDIT_STATS_ARE_NOT_PRODUCT_PROGRESS_GATE:
  counts_used_only_as_coverage_evidence: true
EVERY_UNIQUE_AUDIT_HOLE_BECOMES_PRODUCT_SLICE_GATE:
  code_or_test_or_runtime_slice_for_reviewed_hole:
SINGLE_MODEL_BLIND_SPOT_GATE:
  single_model_only_findings_preserved: true
SECURITY_OPS_LIFECYCLE_ARE_PRODUCT_GAPS_GATE:
  security_ops_lifecycle_findings_not_polish: true
REAL_TRAINER_ROUTE_B_GATE:
  DpoTrainingHandoff_to_11_DPO_DpoTrainer_checkpoint_lifecycle_checked: true
POST_GOVERNANCE_RETURN_TO_PRODUCT_GATE:
  next_product_slice_named_after_governance_repair: true
```

The review must treat auth/tenant/idempotency/rotation/rollback, PNG/OOM/SSRF,
loop semantics, tenant/runtime/scaffold marker mismatches, and S2.1/S3.1
contradictions as product gaps. For trained-feature claims, local aggregation
such as `_weightedSum`/`ObservationCount`, false/constant `PassesGate`, score
nudges capped below thresholds, and synthetic pairs are not enough; the real
route through `DpoTrainingHandoff -> 11.DPO/DpoTrainer`, checkpoint lifecycle,
held-out evaluation, fresh-load, continue-training, and threshold influence
must be implemented/proven or explicitly open.

---

## Review Output Record

After completing all 5 layers plus Layer 5A when applicable, record the result:

```
GENERATED CODE REVIEW — [date]
File: [generated-file.cs]
Generator: SemanticCompilerPipeline v[N] / TemplateBeautifier / DynamicDomains

Layer 1 (Semantic):  PASS / FAIL — [notes]
Layer 2 (DNA):       PASS / FAIL — [grep results]
Layer 3 (Silent):    PASS / FAIL — [test results]
Layer 4 (Training):  PASS / FAIL — [inputs tested]
Layer 5 (Trainable vs Stub): PASS / FAIL — [numeric metrics or stub-evidence]

Layer 5A (Allowed Trainable Architecture): PASS / FAIL - [allowed level, actual mechanism, verdict]
Layer 5B (Reward-Scalarization for non-scalar output): PASS / FAIL / N/A - [output arity, reward extractor file:line]
Layer 5C (Winner-vs-Losers cardinality): PASS / FAIL / N/A - [N, pair rule, acceptance-signal independence]
Layer 5D (Determinism-primitive naming): PASS / FAIL / N/A - [named primitive, cold-start computability]
Layer 5E (Seam-sourced extractor): PASS / FAIL / N/A - [per-field extractor file:line, unknown-format branch]
Layer 5F (Cross-file single-owner consistency): PASS / FAIL / N/A - [named type, owner match, locked-canon parity]
Code review completeness: PASS / FAIL - [standards/tests/core boundary/thin host adapters/deterministic labels/docs-QA-dashboard impact]
Audit Sufficiency Boundary: PASS / FAIL / N/A - [bounded scope, breadth challenge, broader-audit question, delta-severity record]

Overall: PASS / FAIL
Action: [commit as-is / regenerate with fixed generator / escalate]
```

## Multi-cycle code review floor

When Luba or the active plan requires at least four code-review/fix cycles
before commit/push, one Generated Code Review record is only one cycle. Do not
compress the floor into a single broad review.

Required before commit/push under that floor:

```text
generated_code_review_cycle_1 = PASS after fixes/recheck
generated_code_review_cycle_2 = PASS after fixes/recheck
generated_code_review_cycle_3 = PASS after fixes/recheck
generated_code_review_cycle_4 = PASS after fixes/recheck
cycle_focuses_are_distinct = true
unfixed_findings_open = false
```

If any cycle finds a defect, apply the scoped fix and rerun the next cycle after
the fix. Missing cycle evidence keeps commit/push closed.

## Bounded sub-agent evidence boundary

When generated-code or trainable-code evidence comes from a bounded sub-agent,
review must first verify the work-order boundary before judging code quality:

```text
work_order_scope:
exact_allowed_files:
actual_files_changed:
commit_push_authorized:
allowed_git_commands:
forbidden_commands:
commit_push_performed:
parent_will_perform_commit_push_after_review:
commit_only_work_order:
may_derive_next_phase:
must_stop_after_report:
parent_review_readback_done:
subagent_notification_checked_for_commit_push_claims:
parent_local_sidecar_work_while_waiting:
parent_read_governed_files_before_packet_return:
read_only_verification_after_packet_return_only:
```

Reject the code-review packet when the sub-agent created later-phase artifacts,
committed, pushed, or derived the next phase from `STATE.json next_action`
without explicit work-order authorization. A sub-agent's self-review or
`parent-review` file is evidence only and cannot replace parent code review.
Also reject when `commit_push_authorized=false` but the work order did not list
git write/ref commands as forbidden, when the sub-agent ran a git write/ref
command anyway, or when the final packet emits git directives without exact
commit/push authority.

For artifact/code/content work orders, review must also reject a packet that
performed commit/push in the same work order. Commit/push authority belongs only
to a separate commit-only work order with no artifact/code/content edits, exact
file list, exact human-approved commit message, and no next-phase derivation.
That commit message must describe product purpose, behavior before/after, the
algorithm/rule changed, actual verification, evidence boundary, relationship to
training, and published-vs-pending state; a three-line body or green test count
alone is insufficient. Parent review must inspect the sub-agent notification for
commit, push, staging, branch/ref, and directive claims before judging the
packet clean.

When Luba required sub-agent-only execution or repair, generated-code/code
review must also reject parent local sidecar work. The parent cannot read
governed files, skill indexes, plans, code, reports, or state to independently
investigate or prepare fixes while the assigned sub-agent is still running.
Read-only review is valid only after the returned packet exists and only for
verification/integration of that packet.

Parent Architect Subagent-Only Firewall compatibility: read-only verification
is allowed only after a sub-agent packet returns. The no local non-overlapping
work escape is disabled: parent may not read code, reports, plans, skills, or
state for sidecar code-review analysis while waiting for the assigned sub-agent.
Sub-agent waiting is not a stop.

---

## Clean-Mergeable-Into-Original-And-Main Check (before DONE / before commit)

Development/code-review level of the `clean-mergeable-into-original-and-main
invariant` (AGENTS.md OP-67; CLAUDE.md "Clean-mergeable-into-original-and-main
invariant pattern"; plan-review-SKILL.md FC-43). Before any DONE claim or any
commit on a working/design branch (for example `DesignToCode-clean`), review
must confirm the slice still merges cleanly into BOTH the original/base branch
(for example `codex/chat-gpt-5.5`) AND `main`. Literal:
`every commit must merge cleanly into BOTH original and main at any time`.

```text
mergeability_probe_run = true            # git merge-tree (or no-write merge) into <original> AND main
mergeability_into_original = clean       # 0 conflicts, else FAIL
mergeability_into_main = clean           # 0 conflicts, else FAIL
no_add_add_generated_evidence_shared_paths = true   # generated evidence on branch-unique/gitignored paths
shared_file_edits_coordinated = true     # docs/skills, CLAUDE.md, AGENTS.md, shared contracts: additive/minimal
mergeability_evidence_line_present = true
```

Required evidence line in the packet:
`mergeability: clean into <original> AND main (git merge-tree, 0 conflicts)`
with the concrete original/base branch name substituted. Literals:
`mergeability check: clean into <original> AND main`;
`no add/add on generated evidence at shared paths`.

Reject the packet (commit/push stays closed) when the probe was not run, when it
reports nonzero conflicts into original OR `main`, when generated evidence
collides at shared paths (add/add), or when a shared file was edited divergently
without coordination.

---

## Anti-Patterns

1. **Visual inspection as verification** — "looks right to me" is not Layer 3
2. **One-input testing** — one passing case does not verify the pattern generalizes
3. **Skipping Layer 2 because "the generator would never do that"** — verify, don't assume
4. **Committing generated files without review record** — the record proves review happened
5. **Reviewing a trainable unit by contract/build test only** — a stub returning `not_ready` passes contract tests, builds clean, and satisfies DNA. Layer 5 numeric metrics are mandatory for trainable deliverables (the V20 failure).
