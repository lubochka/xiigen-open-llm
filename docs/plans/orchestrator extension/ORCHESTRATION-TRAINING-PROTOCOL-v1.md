> **STATUS: IMPLEMENTED — Phase 37 (2026-06)**
> The training protocol described here was implemented in Phase 37 of ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.
> For current gate commands, acceptance metrics, and the trained model checkpoint, see:
> `C:\Projects\llm_mvp_core\docs\plans\ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md`
> Current checkpoint: `C:\Projects\llm_mvp_core\models\orchestration\orchestration-v1\`

---

# Orchestration Training Protocol v1

Date: 2026-05-12

Purpose: document the real procedure used to improve the internal orchestration/planning DPO model, so another executor can continue the work without guessing. This is not a generic training note. It describes the current XIIGen-style cycle: build small safe increments, train them, measure them, reject harmful rows, and promote only when the full combined baseline improves.

Current baseline at time of writing:

- Baseline: `RUN-479/V309`
- Run id: `train-ready-v309-v307-plus-v308-plan-head16-full-20260512-004000`
- Source: `C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\train-ready-v2\dpo-pairs-domain-v309-v307-plus-v308-plan-head16-trainable.jsonl`
- Rows trained: 1,404
- Batches: 88
- Domain counts: orchestration 501, plans 829, skills 74
- Weighted accuracy: 0.838068
- Final accuracy: 1.000000
- Weighted reward margin: 0.028928
- Final reward margin: 0.051783
- Weighted loss: 0.678916
- No `sub_phase=3.6` rows are currently allowed in the promoted baseline.

## 1. What We Are Training

The target is not project-local code generation. The target is the orchestration intelligence around complex work:

- route a request into the correct orchestration/planning/skill domain;
- choose the correct phase or sub-phase before implementation;
- preserve dependency order;
- distinguish planning from implementation;
- detect stale, unsafe, or unregistered authority;
- produce strong negative contrast through DPO rejected sides;
- avoid training on rows that the current system cannot route.

The model should learn to ask: "What kind of work is this, what authority controls it, what comes before what, and what is the wrong move?"

The model should not learn project-specific source code details unless those details are part of orchestration/planning authority.

## 2. Roles of Model Judgment and Deterministic Code

This work cannot be done with deterministic code alone. Deterministic scripts are useful only for mechanical parts:

- file inventory;
- SHA256 checks;
- row counting;
- JSONL validation;
- hard gates such as `chosen != rejected`;
- mojibake detection;
- `[WRONG: ...]` presence;
- training runner execution;
- CPU/RAM/time metric collection.

Model judgment is required for the parts that are not syntactic:

- deciding whether a document is canonical, stale, or only evidence;
- deciding whether a "new file type" is actually new or a new version of an older type;
- deciding whether a rejected answer is plausibly wrong enough to teach the model;
- deciding whether a source is root prompt authority, plan authority, state authority, or only audit evidence;
- diagnosing why a batch regressed;
- deciding which rows to ablate next.

Rule: code may inventory and verify; the model must adjudicate meaning.

## 3. Canonical Working Locations

Internal learning root:

`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2`

Current run root:

`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1`

Current status file:

`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\RUN-STATUS.json`

Train-ready rows:

`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\train-ready-v2`

Evidence reports:

`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\evidence`

Training runner project:

`C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\InternalTrainReadyDpoRunner.csproj`

Training results:

`C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\results`

## 4. Exact Read Order Before Any New Run

Every executor must read these in order before building a new batch:

1. `RUN-STATUS.json`

   Purpose: restore the current accepted baseline, accepted additions, rejected runs, known debt, and current input SHA.

2. Latest evidence report in `evidence\`

   Purpose: learn what the last successful run accepted and which row shapes were rejected.

3. Current baseline JSONL from `RUN-STATUS.json.input.source`

   Purpose: inspect tail rows and avoid duplicate raw ids.

4. Candidate source file or source directory

   Purpose: identify the next not-yet-trained rows. For recent PLAN work this was:

   `C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\new alternative sets\PLAN-Rows.md`

5. Relevant guide documents only after the state is restored

   Purpose: guides explain pair rules, but the current baseline tells which guide decisions have already been proven or disproven.

6. `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md`

   Purpose: restore the unit solution library, the proven pattern for the unit being trained, the current language coverage state, and the required protocol update after promotion or rejection.

7. `EXECUTOR-DISCIPLINE-RULES-v1.md`

   Purpose: enforce executor behavior. Do not stop between passed gates, do not ask whether to run the next required step, do not stop because of elapsed time or session length, and do not claim done until the real training and real-system gates pass.

8. `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md`

   Purpose: define raw-number reporting for accuracy, reward margin, CPU, RAM, time, per-row cost, below-0.75 behavior, and the repeated 50-row improvement loop.
   Also read Section 22.0 before any user-facing status: it decides whether a
   runner report is allowed at all. Do not use a test result, generated
   manifest, or implementation summary as a runner report.
   Also read Section 22.0A before reporting metrics: CPU, RAM, accuracy,
   loss, or resource-cost values must be persisted to a JSON report artifact
   before they are shown to the operator.

9. `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`

   Purpose: restore the required prompt-derived validation tasks for project-local wiki training, especially developer reasoning, entity impact, flow trace, missing evidence, and project-boundary questions.

10. `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`

   Purpose: restore the trainable flow that turns developer questions into multi-step wiki evidence-query plans before retrieval and answer composition.

11. `EXECUTOR-STAGE-PROMPT-v2.md`

   Purpose: remind the executor of the concrete goal of the current stage, where to read for each sub-stage, and which decision dilemmas must not be improvised.

Never start from a generic guide alone. The guide can be stale relative to the latest run.

## 5. Data Unit: What One DPO Row Must Contain

Each train-ready row must be a JSONL object with at least:

- `id`: unique local row id for the generated train-ready set;
- `origin`: run/build provenance;
- `normalized_family`: source family or document-type bucket;
- `bucket`: curriculum bucket;
- `tier`: curriculum tier;
- `pair_kind`: why this pair exists;
- `source_status`: license or authority state;
- `source`: exact source file path;
- `source_span`: raw id and source span;
- `source_license_state`: license/authority state;
- `attribution_required`: true/false;
- `a_side`: prompt/request/situation;
- `chosen`: correct behavior;
- `rejected`: wrong behavior, with a specific `[WRONG: ...]` explanation;
- `target_behavior`: what the model must learn;
- `domain_focus`: `orchestration`, `plans`, or `skills`;
- `notes`: filtering decisions or risks;
- `raw_pair_id`: original compact-row id where available.

Hard gates:

- `chosen` must not equal `rejected`;
- `rejected` must contain `[WRONG: ...]`;
- no mojibake in `a_side`, `chosen`, or `rejected`;
- no unregistered `sub_phase=3.6` rows;
- no rows from source ledger/status/inventory files as if they were domain authority;
- no rows whose only contrast is a trivial negation of the chosen side.

## 6. How Candidate Data Is Built

The normal process:

1. Find the next raw rows after the last accepted raw id.

   Example: V309 ends at `PLAN-A2155`, so the next candidate scan starts after that raw id.

2. Filter rows by current known debt.

   Current filters:

   - exclude `sub_phase=3.6`, because production routing has no registered 3.6 sub-phase;
   - exclude late 3.5/3.6 telemetry until the model has more stable routing support;
   - exclude 21-module schema-shaped `sub_phase=3.2` rows, because ablations isolated schema rows as harmful near V304/V306/V308;
   - avoid broad prose-shaped architecture rows unless they have already been compacted and ablated.

3. Prefer rows that teach one clear contrast.

   Good example:

   - A-side asks for a plan.
   - Chosen routes to `PLAN/SchemePlannerUnit` with the correct sub-phase and ordering.
   - Rejected jumps to implementation or reverses dependency order.

4. Lift compact rows into full DPO rows.

   The lifted chosen side must include:

   - unit/domain name;
   - correct output;
   - sub-phase;
   - dependency order;
   - gate expectation;
   - authority/license note.

   The rejected side must include:

   - plausible wrong domain or wrong sub-phase;
   - specific `[WRONG: ...]`;
   - functional consequence.

## 7. 50-Row Batches vs 16-Row Diagnostic Probes

The full curriculum concept is batch-by-batch learning, commonly 50 rows at a time.

But during late-stage orchestration/planning improvement the current baseline became sensitive to small bad row shapes. At that point, using a full 50-row addition is dangerous because one harmful row can lower final accuracy or margin and make it harder to diagnose the cause.

Therefore two batch sizes are used for different purposes:

- 50-row batch: normal curriculum unit when the source family is stable and the row shape is already proven.
- 16-row probe: diagnostic safety unit used when adding a new or risky tail segment to an already strong baseline.

The 16-row probe is not a replacement for the 50-row training concept. It is the safety mechanism used before promotion. Once a shape is stable, it can be expanded again.

## 8. Training Cycle

Every accepted addition must pass two gates.

### Gate A: Standalone Candidate Gate

Train only the candidate addition.

Purpose:

- detect whether the candidate rows are internally learnable;
- identify harmful rows before they touch the baseline;
- measure candidate accuracy, loss, reward margin, CPU, RAM, and elapsed time.

Acceptable candidate pattern:

- accuracy should be high, preferably 1.0 for a 16-row probe;
- reward margin must be positive;
- loss should be stable or improving;
- no final batch collapse.

If the candidate fails:

- do not promote;
- split the candidate into head/tail;
- if needed, run single-row ablations;
- record isolated bad raw ids in `RUN-STATUS.json.rejected_runs`;
- rebuild the candidate without those rows.

### Gate B: Full Combo Gate

Append the accepted candidate to the current baseline and train the full combined dataset.

Purpose:

- verify the addition improves the real model context;
- catch interference with older orchestration/planning/skills rows;
- ensure the final batch remains healthy.

Promotion requires:

- weighted accuracy improves or does not materially regress;
- weighted reward margin improves or remains clearly positive;
- weighted loss improves or does not materially regress;
- final accuracy stays healthy, ideally 1.0;
- final reward margin remains positive;
- no known blocked row type re-enters the dataset.

Only the combo run can become the new baseline.

## 9. How I Know What To Improve Next

The next improvement is selected from the evidence, not from a generic backlog.

Signals:

1. Weighted accuracy drops.

   Interpretation: too many rows are ambiguous, too broad, or the rejected sides are not strong enough.

   Action: stop adding broad rows, inspect failure batch, compact the row shape, or ablate.

2. Reward margin is low or negative.

   Interpretation: the model is not reliably preferring chosen over rejected.

   Action: strengthen rejected consequence; remove trivial negation; use sharper wrong-domain or wrong-order contrast.

3. Final accuracy drops while average improves.

   Interpretation: tail rows are harmful even if earlier batches look good.

   Action: split tail rows and run head/tail or single-row ablations.

4. Loss improves but accuracy drops.

   Interpretation: the model is becoming more confident but on the wrong distinction.

   Action: inspect pair semantics, not only metrics.

5. A row type repeatedly fails single-row ablation.

   Interpretation: the current model cannot safely learn that shape yet.

   Action: block the shape temporarily and record it as known debt.

Current examples:

- `sub_phase=3.6` is blocked because the production topology has no registered 3.6.
- 21-module schema-shaped `sub_phase=3.2` PLAN rows are blocked because `PLAN-A2090` and `PLAN-A2120` failed ablations.
- broad prose architecture-source probes were rejected because they produced low or negative standalone metrics.

## 10. Ablation Method

When a candidate fails:

1. Run the full candidate standalone.
2. If it fails, split into head and tail.
3. Train head only.
4. Train tail only.
5. If one half fails, split that half again.
6. If needed, train one row at a time.
7. Mark bad raw ids explicitly.
8. Rebuild the candidate without bad rows.
9. Re-run standalone.
10. Only then run combo.

A bad row is not "bad forever". It is bad for the current model state and current routing topology. It may be reintroduced later after the missing structure exists.

## 11. Metric Interpretation

Training metrics are real only when produced by the DPO runner.

Structural gate pass rate is not training accuracy.

Actual run metrics:

- accuracy: percentage of DPO comparisons where chosen beats rejected;
- loss: DPO loss;
- reward margin: chosen reward minus rejected reward;
- elapsed time: runner wall-clock time;
- CPU: measured per batch by the runner;
- working-set delta: memory movement per batch;
- checkpoint bytes: model/checkpoint artifact size.

For reporting:

- report standalone candidate metrics;
- report full combo metrics;
- compare combo against previous baseline;
- include CPU/RAM only when the runner recorded them;
- never invent CPU/RAM placeholders;
- write the report artifact before the user-facing report:
  training runs under `runner-reports/`, phase/test validation under
  `orchestrator extension/phase-metrics/<run-id>/PHASE-RESOURCE-REPORT.json`;
- include `report_artifact_path` in any user-facing metric report.

Example from V309:

- Average CPU: 9.077790%
- Max CPU: 12.543612%
- Average working-set delta: -0.172541 MB
- Max working-set delta: 17.566406 MB
- Elapsed: 860,816.106 ms

## 12. Promotion Decision Format

An evidence report must include:

- run ids;
- input file paths;
- row counts;
- SHA256 of promoted input;
- accepted raw ids;
- rejected or held-out raw ids;
- standalone metrics;
- combo metrics;
- comparison to previous baseline;
- final verdict;
- reason for acceptance or rejection.

Accepted additions must also be written into:

`RUN-STATUS.json.accepted_additions`

Rejected probes must be written into:

`RUN-STATUS.json.rejected_runs`

Superseded baselines must be moved or noted under:

`RUN-STATUS.json.candidates`

Known structural debt must be written under:

`RUN-STATUS.json.known_debt`

## 13. Common Failure Modes

### Failure Mode 1: Beautiful but weak rejected side

Symptom:

- accuracy drops;
- reward margin is small;
- rejected side sounds like a vague alternative.

Fix:

- state the exact weakness in `[WRONG: ...]`;
- name the consequence;
- make rejected plausible but clearly harmful.

### Failure Mode 2: Broad prose rows

Symptom:

- standalone accuracy around 0.2 to 0.6;
- margin close to zero or negative.

Fix:

- compact the row into a concrete routing/planning decision;
- preserve source authority but remove essay-like wording.

### Failure Mode 3: Unregistered routing target

Symptom:

- row teaches a sub-phase or predicate that the current topology cannot route.

Fix:

- block from training until the topology exists;
- do not try to "teach around" missing structure.

### Failure Mode 4: Schema-shaped rows too early

Symptom:

- one schema row causes a full 16-row probe to fail.

Fix:

- isolate with single-row ablation;
- postpone schema rows until the model has stronger plan-vs-schema boundaries.

### Failure Mode 5: Confusing actual training accuracy with validation formatting

Symptom:

- a batch is called "accurate" because JSONL shape passed.

Fix:

- call it `structural_gate_pass_rate`;
- reserve `accuracy` for DPO runner output only.

## 14. Practical Command Pattern

Run from:

`C:\Projects\llm_mvp_core`

Typical standalone candidate run:

```powershell
dotnet run --project "C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\InternalTrainReadyDpoRunner.csproj" -- --source "<candidate-jsonl>" --run-id "<standalone-run-id>" --context 96 --batch-size 16 --save-every 25
```

Typical combo run:

```powershell
dotnet run --project "C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\InternalTrainReadyDpoRunner.csproj" -- --source "<baseline-plus-candidate-jsonl>" --run-id "<combo-run-id>" --context 96 --batch-size 16 --save-every 25
```

After the run, inspect:

- `internal-train-ready-dpo-summary.json`
- `batch-summary.jsonl`
- `progress.jsonl`
- generated checkpoint files.

## 15. Current Continuation Rule

From V309, the next safe move is:

1. scan after `PLAN-A2155`;
2. exclude late `3.5` / `3.6`;
3. exclude 21-module schema-shaped `3.2` rows;
4. build the next PLAN-only candidate from clear `3.1`, `3.3`, and `3.4` routing rows;
5. run standalone;
6. if standalone passes, run combo;
7. promote only if combo improves or safely preserves V309 metrics;
8. write evidence before continuing.

Do not resume by adding arbitrary new external-source rows. The current model has a proven path: small PLAN routing increments with aggressive ablation.

## 16. Why This Process Works

The small model is not trying to memorize all architecture documents. It is learning a compact decision surface:

- which domain a request belongs to;
- which phase owns it;
- which dependency must precede it;
- which answer shape is dangerous;
- which authority source wins;
- which rows are not trainable yet.

That is why a small model can improve meaningfully: the curriculum reduces the huge documentation universe into repeated, high-contrast decisions. The model does not need to contain the whole project. It needs to route, plan, reject unsafe shortcuts, and ask for the right authority surface.

## 17. Executor Checklist

Before building:

- [ ] Read `RUN-STATUS.json`.
- [ ] Read latest evidence report.
- [ ] Read `EXECUTOR-DISCIPLINE-RULES-v1.md` and confirm no passed-gate stop is allowed.
- [ ] Read `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md` Section 18 and confirm the current unit's proven pattern or current missing pattern.
- [ ] Read `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` and confirm which raw metrics the runner must emit.
- [ ] If wiki/project-local rows are being built, read `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md` and select validation prompts before selecting rows.
- [ ] If wiki/project-local question rows are being built, read `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md` and include expected query plans.
- [ ] Read `EXECUTOR-STAGE-PROMPT-v2.md` and confirm the current stage goal and dilemmas.
- [ ] Inspect baseline tail rows.
- [ ] Identify next raw ids.
- [ ] Apply known-debt filters.

Before training:

- [ ] Validate JSONL shape.
- [ ] Validate `[WRONG: ...]`.
- [ ] Check `chosen != rejected`.
- [ ] Check no mojibake.
- [ ] Check no `sub_phase=3.6`.
- [ ] Check no duplicate raw ids.

Before promotion:

- [ ] Standalone run passed.
- [ ] Combo run passed.
- [ ] Combo compared to previous baseline.
- [ ] CPU/RAM/time reported from runner output.
- [ ] Evidence report written.
- [ ] `RUN-STATUS.json` updated.

Never promote on hope. Promote only on evidence.
