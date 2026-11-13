# PLAN-DETAILED-TEACHING-REPORT-v1

Date: 2026-05-07
Purpose: compare XIIGen documentation teaching runs by batch size and produce a detailed, inspectable Markdown report for the actual learning process.

This plan is intentionally plain Markdown. No visual widget. No dashboard first. The deliverable is a set of `.json` files for exact data and `.md` files for human inspection.

---

## 1. Question This Experiment Must Answer

Teach the engine from the same XIIGen documentation corpus using four batch sizes:

- 20 rows per batch
- 50 rows per batch
- 100 rows per batch
- 200 rows per batch

Each run must process the same fixed row set so the only variable is batch size.

The comparison must answer:

- Which batch size gives the best actual elapsed time?
- Which batch size stays under machine health limits: CPU, RAM, disk all under 80%?
- Which batch size gives the best learning signal by domain and sub-phase?
- Where exactly is time spent in the teaching process?
- Which phases/sub-phases dominate cost?
- Which domains and flow-tree branches are actually being learned?
- Where do LoRA/DPO, replay, checkpointing, recognition, routing, and beautifier/reporting sit in the cost profile?

---

## 2. Hard Resource Rule

Do not judge the run by an internal multiplier alone.

The run is acceptable if:

- Peak CPU usage during teaching is below 80%.
- Peak absolute RAM usage during teaching is below 80% of total system RAM.
- Total generated teaching/checkpoint/report disk usage stays below 80% of available disk at run start.
- Logic failures are zero, or explicitly explained as expected knowledge edges.

If a threshold is crossed, do not abort automatically. Finish the run if the machine remains responsive, but mark the run as `RESOURCE_REVIEW` and record the first batch/row/stage where the threshold crossed.

---

## 3. Fixed Input Corpus

Build one fixed teaching corpus before running any batch-size experiment.

Recommended first corpus:

- Source: XIIGen documentation and session-derived learning rows.
- Target size: 200 rows.
- Selection rule: oldest rows first, then deterministic order by source path and timestamp.
- Output file: `rows-200-fixed.jsonl`.

Every batch-size run uses the exact same `rows-200-fixed.jsonl`.

Batching:

- Batch 20: 10 batches of 20 rows.
- Batch 50: 4 batches of 50 rows.
- Batch 100: 2 batches of 100 rows.
- Batch 200: 1 batch of 200 rows.

No row may be skipped silently. Rows rejected during preparation must be written to `rejected-rows.jsonl` with reason.

---

## 4. Run Order

Use this order:

1. Batch 20
2. Batch 200
3. Batch 50
4. Batch 100

Reason:

- Batch 20 gives the safest baseline.
- Batch 200 tests the largest memory/disk pressure early.
- Batch 50 and 100 fill the middle after the extremes are known.

Before each run:

- Start from the same input corpus.
- Use a fresh output directory.
- Record git commit hash.
- Record current branch.
- Record machine CPU count, total RAM, available disk.
- Record process working set baseline.
- Record checkpoint directory baseline size.

---

## 5. Output Directory Layout

Each batch size writes to its own run folder:

```text
experiments-v2/XIIGEN-DOCS-BATCH-COMPARISON/
  rows-200-fixed.jsonl
  rejected-rows.jsonl

  batch-20/
    pre-run-stats.json
    progress.jsonl
    batch-summary.jsonl
    row-summary.jsonl
    stage-summary.jsonl
    subphase-summary.json
    flow-tree-summary.json
    lora-audit.json
    disk-profile.json
    detailed-report.json
    detailed-report.md
    checkpoints/

  batch-50/
    ...

  batch-100/
    ...

  batch-200/
    ...

  cross-run-comparison.json
  cross-run-comparison.md
```

---

## 6. Teaching Stage Breakdown

Every row and every batch must be measured across the same stages.

These stage names are fixed. Do not rename them between runs.

| Stage | Name | What It Measures |
|---|---|---|
| S0 | corpus_preparation | Read docs/session data, parse rows, normalize source records, reject invalid rows |
| S1 | batch_preparation | Build the current batch, dedupe, order rows, attach source metadata |
| S2 | semantic_compilation | E10 regex cleanup, SemanticCompiler normalization, prompt splitting, translation/reordering |
| S3 | domain_recognition | Domain classifier result, confidence, rejected domains, context influence |
| S4 | topology_routing | Predicate extraction, graph closure, branch cap, selected sub-phases, rejected sub-phases |
| S5 | flow_tree_placement | Map selected sub-phases into phase/domain/flow tree location |
| S6 | training_set_build | Build chosen/rejected pairs, replay mix, vector features, parent checkpoint selection |
| S7 | lora_dpo_training | LoRA adapter load/create, DPO forward/backward, loss, rewards, accuracy |
| S8 | checkpoint_registry | Save checkpoint, tokenizer, registry snapshot, flow/unit metadata |
| S9 | flow_execution | Execute generated/probationary flow, collect verdict, evidence, warnings |
| S10 | beautifier_translation | Result beautifier/template output/result translation, if present |
| S11 | regression_window | Re-run previous sampled rows to confirm cumulative learning still works |
| S12 | report_write | Write JSON/MD reports and calculate run metrics |

For each stage capture:

- elapsed_ms
- process_cpu_percent_sample
- working_set_mb_before
- working_set_mb_after
- managed_allocation_mb_delta if available
- disk_bytes_before
- disk_bytes_after
- status: PASS / REVIEW / FAIL
- notes

If exact CPU/RAM per stage is not available, capture per-batch CPU/RAM and per-stage elapsed time. The report must explicitly say which metrics are stage-exact and which are batch-level approximations.

---

## 7. Row-Level Report

Each taught row writes one object to `row-summary.jsonl`.

Required fields:

```json
{
  "RunId": "batch-20",
  "BatchSize": 20,
  "BatchIndex": 1,
  "RowIndex": 12,
  "RowId": "XIIGEN-...",
  "SourcePath": "",
  "SourceTimestampUtc": "",
  "Prompt": "",
  "Domain": "BUG",
  "DomainConfidence": 0.0,
  "SelectedSubPhases": ["4.1", "4.4"],
  "RejectedSubPhases": [],
  "BranchCount": 2,
  "VerdictBeforeTeaching": "KNOWLEDGE_EDGE",
  "VerdictAfterTeaching": "PASS",
  "TotalElapsedMs": 0,
  "TeachingElapsedMs": 0,
  "PeakCpuPercent": 0,
  "PeakWorkingSetMb": 0,
  "CheckpointBytesWritten": 0,
  "DiskBytesWritten": 0,
  "StageTimings": {
    "semantic_compilation": 0,
    "domain_recognition": 0,
    "topology_routing": 0,
    "training_set_build": 0,
    "lora_dpo_training": 0,
    "checkpoint_registry": 0,
    "beautifier_translation": 0
  }
}
```

---

## 8. Sub-Phase Performance Report

The main detailed report must include one table per run:

| Sub-phase | Phase | Domain(s) | Task type | Appearances | Avg row index | Avg training step | Avg DPO ms | Avg DPO loss | Avg reward margin | Avg accuracy | Avg checkpoint KB | Verdict pass rate |
|---|---:|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|

Meaning:

- `Appearances`: how many rows trained this sub-phase.
- `Avg row index`: whether this sub-phase appears early or late in the corpus.
- `Avg training step`: how deep into the domain adapter chain it usually appears.
- `Avg DPO ms`: actual LoRA/DPO cost for this sub-phase.
- `Avg DPO loss`: training loss for examples involving this sub-phase.
- `Avg reward margin`: chosen reward minus rejected reward.
- `Avg accuracy`: whether model already preferred the chosen answer before update.
- `Avg checkpoint KB`: disk cost of persisting this sub-phase's learned state.
- `Verdict pass rate`: fraction of rows involving the sub-phase that ended PASS.

The report must sort this table three ways:

1. Most frequent sub-phases.
2. Most expensive sub-phases by DPO ms.
3. Weakest learning signal by reward margin and accuracy.

---

## 9. Domain Report

For every domain, include:

| Domain | Rows | Branches | Sub-phases covered | Avg DPO ms/branch | Max DPO ms/branch | Peak RAM MB | Disk MB | Pass rate | First resource review row |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---|

Also include a step-by-step domain chain:

| Step | Row | Branches | Sub-phases | Replay examples | DPO ms | Loss | Chosen reward | Rejected reward | Margin | Accuracy | Parent checkpoint |
|---:|---|---:|---|---:|---:|---:|---:|---:|---:|---:|---|

This is where we examine whether BUG, PLAN, CODE_Q, ARCH, and REFACTOR learn differently under different batch sizes.

---

## 10. Flow Tree Report

The flow tree report answers: where in the product/lifecycle topology did the learning land?

Render as Markdown grouped by phase:

```text
Phase 1 - Design
  1.1 design-system-and-tokens
      domains: BUG x2, PLAN x1
      avg_dpo_ms: 0
      pass_rate: 0%

Phase 4 - Implementation
  4.1 backend-services
      domains: BUG x18, CODE_Q x7
      avg_dpo_ms: 0
      pass_rate: 0%

  4.4 refactoring-and-code-quality
      domains: BUG x14, REFACTOR x5
      avg_dpo_ms: 0
      pass_rate: 0%
```

Also list:

- sub-phases covered in this run
- sub-phases not covered in this run
- new sub-phases first learned in this run
- sub-phases carried forward from prior checkpoint state

---

## 11. LoRA / DPO Audit

The LoRA audit must confirm whether configuration is uniform:

| Field | Expected | Actual values seen | Uniform |
|---|---|---|---|
| LoRA rank | 2 | | yes/no |
| LoRA alpha | 4 | | yes/no |
| dropout | 0 | | yes/no |
| target modules | q_proj, v_proj | | yes/no |
| vocab size | 512 | | yes/no |
| hidden dim | 16 | | yes/no |
| layers | 1 | | yes/no |
| heads | 4 | | yes/no |

For each domain:

| Domain | Max training step | Parent chain depth | Final checkpoint MB | Adapter checkpoint count | Replay count |
|---|---:|---:|---:|---:|---:|

The report must flag:

- inconsistent LoRA config
- missing parent checkpoint
- parent checkpoint not loaded
- unexpectedly large checkpoint growth
- replay buffer growth without cap
- DPO time growth after replay growth

---

## 12. Recognition and Routing Report

This section checks whether the engine recognized the row correctly before training.

For each row:

| Row | Prompt summary | Domain | Domain confidence | Segments | Predicates | Selected sub-phases | Rejected sub-phases | Branch cap hit |
|---|---|---|---:|---:|---|---|---|---|

Summary metrics:

- domain distribution
- average domain confidence
- rows with domain confidence below 0.40
- rows where branch cap was hit
- average branch count
- rows where topology expansion produced too many branches
- rows where context changed the route

This is the place to inspect recognition, not only training.

---

## 13. Set Preparation Report

This section explains how the teaching set was built before learning.

Required:

| Source type | Files/records scanned | Rows extracted | Rows accepted | Rows rejected | Main rejection reasons |
|---|---:|---:|---:|---:|---|

Also include:

- oldest source timestamp
- newest source timestamp
- deterministic ordering rule
- duplicate prompt count
- duplicate normalized prompt count
- rows per domain before teaching
- rows per domain after recognition

This prevents comparing batch sizes on an unclear or changing input set.

---

## 14. Beautifier / Result Translation Report

If beautifier/result translation units run during teaching or verification, measure them separately.

| Row | Stage | Beautifier used | Template used | Input chars | Output chars | Elapsed ms | Warnings |
|---|---|---|---|---:|---:|---:|---|

If they do not run, the report must explicitly say:

```text
Beautifier/result translation did not run in this teaching path.
```

No silent omission.

---

## 15. Regression Window

After each batch, run a small regression sample:

- 2 rows from the first completed batch
- 2 rows from the immediately previous batch
- 2 rows from the current batch

For batch size 200, use:

- 2 rows from the first 20 rows
- 2 rows from the middle 20 rows
- 2 rows from the last 20 rows

Report:

| Regression row | Original verdict | New verdict | Same domain | Same sub-phases | Selected newer unit | Pass |
|---|---|---|---|---|---|---|

This checks that cumulative learning did not break earlier rows.

---

## 16. Cross-Run Comparison

After all four runs, generate `cross-run-comparison.md`.

Required comparison tables:

### 16.1 Resource Comparison

| Batch size | Total elapsed | Peak CPU | Peak RAM | Disk written | Accepted | Resource review | Logic review |
|---:|---:|---:|---:|---:|---:|---:|---:|

### 16.2 Stage Cost Comparison

| Stage | Batch 20 | Batch 50 | Batch 100 | Batch 200 | Best |
|---|---:|---:|---:|---:|---|

### 16.3 Domain Comparison

| Domain | Batch size | Rows | Avg DPO ms/branch | Max step | Accuracy | Reward margin | Resource status |
|---|---:|---:|---:|---:|---:|---:|---|

### 16.4 Sub-Phase Comparison

| Sub-phase | Batch 20 cost | Batch 50 cost | Batch 100 cost | Batch 200 cost | Best batch | Notes |
|---|---:|---:|---:|---:|---|---|

### 16.5 Recommendation

End with one explicit recommendation:

```text
Recommended batch size: <20|50|100|200>
Reason:
- resource threshold result
- total elapsed result
- learning quality result
- regression result
```

---

## 17. Implementation Steps

### Phase 0 - Implementation gap gate

Before preparing or teaching any rows, validate the current implementation gap inventory.

The report must include:

- A51 infrastructure copy status
- A52 context packet reader status
- A53 E13B held-out validation status
- A54 replay / teaching accumulation status
- prompt-path experiment wiring status
- training-path experiment wiring status

Gate:

- no teaching run starts while an open gap is both relevant to the run and unwaived
- every open gap is marked `closed`, `deferred`, or `blocking`
- if a gap is deferred, the report states why the batch result is still meaningful

### Phase 1 - Prepare fixed corpus

Create:

- `rows-200-fixed.jsonl`
- `rejected-rows.jsonl`
- `set-preparation-report.md`

Gate:

- 200 accepted rows
- deterministic order
- no silent rejected rows

### Phase 2 - Add detailed metrics collection

Instrument the teaching runner to emit:

- `progress.jsonl`
- `batch-summary.jsonl`
- `row-summary.jsonl`
- `stage-summary.jsonl`
- `subphase-summary.json`
- `flow-tree-summary.json`
- `lora-audit.json`
- `disk-profile.json`

Gate:

- one dry-run batch of 2 rows emits every file
- every stage S0-S12 appears in `stage-summary.jsonl`

### Phase 3 - Run batch size 20

Process all 200 rows in batches of 20.

Gate:

- run completes
- detailed report generated
- regression window executed

### Phase 4 - Run batch size 200

Process all 200 rows in one batch.

Gate:

- run completes or records exact failure point
- detailed report generated
- regression window executed if possible

### Phase 5 - Run batch size 50

Process all 200 rows in batches of 50.

Gate:

- same as above

### Phase 6 - Run batch size 100

Process all 200 rows in batches of 100.

Gate:

- same as above

### Phase 7 - Cross-run comparison

Generate:

- `cross-run-comparison.json`
- `cross-run-comparison.md`

Gate:

- all four runs represented
- recommendation section present
- CPU/RAM/disk threshold status shown for all four

### Phase 8 - Commit and push report gate

No result may be presented as final until the implementation and report artifacts are committed and pushed.

Gate:

- run `dotnet test Tests/LlmFromScratch.Tests.csproj`
- stage only scoped source/report/run artifacts for this round
- commit the scoped changes with the live passing test count in the message
- push the commit to the current branch
- record commit hash and push result in `cross-run-comparison.md`

If the commit or push fails, the final report must be labeled `PROVISIONAL_NOT_PUSHED` and the failure reason must be shown before any performance interpretation.

---

## 18. Stop Conditions

Stop and report if:

- corpus preparation cannot produce 200 valid rows
- teaching runner cannot emit stage summaries
- RAM exceeds 90% absolute system usage
- disk free space drops below 10%
- process crashes or corrupts checkpoint registry
- regression window cannot run because prior checkpoint state is missing
- scoped changes cannot be committed or pushed

Do not stop merely because CPU/RAM/disk crosses 80%. Mark `RESOURCE_REVIEW`, finish if safe, and report the first crossing point.

---

## 19. Final Deliverables

The completed experiment must produce:

- `rows-200-fixed.jsonl`
- `set-preparation-report.md`
- `batch-20/detailed-report.md`
- `batch-50/detailed-report.md`
- `batch-100/detailed-report.md`
- `batch-200/detailed-report.md`
- `cross-run-comparison.md`

The final answer to Luba must summarize:

- recommended batch size
- actual time by batch size
- peak CPU/RAM/disk by batch size
- best/worst domains
- best/worst sub-phases
- whether LoRA/DPO is the bottleneck
- whether beautifier/result translation ran
- whether cumulative learning stayed stable
- commit hash for the source/report changes
- push confirmation for the final reported state

If commit hash and push confirmation are missing, the final answer must explicitly say the results are provisional.
