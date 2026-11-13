# PLAN-DETAILED-TEACHING-REPORT-v2

Date: 2026-05-07
Changes from v1: the overall goal is now stated explicitly. Section 3
  (Fixed Input Corpus) is rewritten to distinguish the 200-row calibration
  sample from the full 1,080-row corpus run. All downstream sections now
  state which run they apply to. The final deliverables section includes
  the full corpus run reports.

This plan is intentionally plain Markdown. No visual widget. No dashboard.
The deliverable is a set of `.json` files for exact data and `.md` files
for human inspection.

---

## Overall goal

The goal of this work is to teach the engine on the complete XIIGen
learning base: approximately 1,080 usable session rows exported from
XIIGen documentation and session history.

The detailed report format defined in this plan applies to both:

1. The calibration runs — four 200-row runs at different batch sizes,
   used to select the right batch size before committing to the full run.
2. The full corpus run — one 1,080-row run at the recommended batch size,
   which is the actual teaching run that matters.

Every section in this plan produces output for both types of run. The
calibration runs produce four `detailed-report.md` files that answer
"which batch size is viable and best". The full corpus run produces one
`detailed-report.md` that answers "what did the engine actually learn
from the XIIGen corpus".

The cross-run comparison (Section 16) runs twice: once to compare the
four calibration runs against each other, and once to compare the full
corpus run against the calibration run at the same batch size to confirm
that the engine's behaviour at 1,080 rows is consistent with what the
calibration predicted at 200 rows.

---

## 1. Questions This Work Must Answer

### Calibration questions (answered after the four 200-row runs)

- Which batch size gives the best elapsed time per row?
- Which batch size stays under 80% CPU, RAM, and disk throughout?
- What is the projected total time for 1,080 rows at each batch size?
- What is the projected total disk usage for 1,080 rows at each batch size?
- Which batch size shows the earliest accumulation onset?
- Which batch size gives the best learning signal by domain and sub-phase?

### Full corpus questions (answered after the 1,080-row run)

- Which sub-phases of the XIIGen learning base were successfully learned?
- Which domains dominate the corpus and were they all taught effectively?
- Where in the flow tree did the learning land?
- Did cumulative learning stay stable across the full 1,080 rows?
- Did any domain show accumulation onset before the full corpus completed?
- What is the final flow count and unit count after teaching the full corpus?
- Were there rows the engine could not learn from, and why?

---

## 2. Hard Resource Rule

The same rule applies to both calibration runs and the full corpus run.

The run is acceptable if:

- Peak CPU usage during teaching is below 80%.
- Peak absolute RAM usage during teaching is below 80% of total system RAM.
- Total generated teaching/checkpoint/report disk usage stays below 80%
  of available disk at run start.
- Logic failures are zero, or explicitly explained as expected knowledge edges.

If a threshold is crossed, do not abort automatically. Finish the run if
the machine remains responsive. Mark the run `RESOURCE_REVIEW` and record
the first batch/row/stage where the threshold crossed.

For the full corpus run, an unresolved RAM breach at row 200 means A54
(replay buffer cap) must be fixed before the run can continue. The run
may be paused and resumed after the fix rather than restarted from row 1,
provided checkpoint state is intact.

---

## 3. Input Corpus

### Calibration corpus (200 rows)

Build one fixed 200-row corpus before running any calibration experiment.

- Source: the full XIIGen session corpus file.
- Selection rule: first 200 rows in natural order from the corpus file.
  These are the same rows the full corpus run will process first — they
  are not a separate extraction.
- Output file: `rows-200-fixed.jsonl`.

Every calibration run uses the exact same `rows-200-fixed.jsonl`.
No row may be skipped silently. Rejected rows go to `rejected-rows.jsonl`
with reason.

Batching for calibration runs:

- Batch 20: 10 batches of 20 rows.
- Batch 50: 4 batches of 50 rows.
- Batch 100: 2 batches of 100 rows.
- Batch 200: 1 batch of 200 rows.

### Full corpus (approximately 1,080 rows)

After the calibration recommendation is produced, the full corpus run
uses the complete XIIGen session corpus:

```
C:\Users\lubao\Documents\xiigen\
  [all usable session rows — approximately 1,080 rows]
```

The batch size used for the full corpus run is the one recommended by
the calibration comparison. The full corpus is not subdivided into
artificial fixed sets — it is taught in a single continuous run, batched
at the recommended size, until all usable rows are processed.

---

## 4. Run Order

### Calibration run order

1. Batch 20 — safest baseline, fastest to complete.
2. Batch 200 — largest memory/disk pressure, tested early.
3. Batch 50
4. Batch 100

### Full corpus run

After calibration recommendation is confirmed. Separate run identifier.
Separate output folder.

Before each run record: git commit hash, branch, machine CPU count, total
RAM, available disk, process working set baseline, checkpoint directory
baseline size.

---

## 5. Output Directory Layout

### Calibration runs

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

  batch-50/ ...
  batch-100/ ...
  batch-200/ ...

  cross-run-comparison.json
  cross-run-comparison.md
  recommendation.md
```

### Full corpus run

```text
experiments-v2/XIIGEN-FULL-CORPUS/
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
```

The `detailed-report.md` for the full corpus run is the primary record of
what the engine learned from the XIIGen learning base.

---

## 6. Teaching Stage Breakdown

Every row and every batch is measured across the same stages. These stage
names are fixed and identical between calibration runs and the full corpus
run.

| Stage | Name | What It Measures |
|---|---|---|
| S0 | corpus_preparation | Read docs/session data, parse rows, normalize, reject invalid rows |
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

For each stage capture: elapsed_ms, process_cpu_percent_sample,
working_set_mb_before, working_set_mb_after, managed_allocation_mb_delta
if available, disk_bytes_before, disk_bytes_after, status (PASS/REVIEW/FAIL),
notes.

If exact CPU/RAM per stage is not available, capture per-batch CPU/RAM
and per-stage elapsed time. The report must state which metrics are
stage-exact and which are batch-level approximations.

---

## 7. Row-Level Report

Each taught row writes one object to `row-summary.jsonl`.

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

This format is identical for calibration runs and the full corpus run.

---

## 8. Sub-Phase Performance Report

One table per run, sorted three ways (most frequent, most expensive by
DPO ms, weakest signal by reward margin and accuracy).

| Sub-phase | Phase | Domain(s) | Task type | Appearances | Avg row index | Avg training step | Avg DPO ms | Avg DPO loss | Avg reward margin | Avg accuracy | Avg checkpoint KB | Verdict pass rate |
|---|---:|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|

For the full corpus run this table covers all 1,080 rows and gives the
complete picture of which sub-phases of the XIIGen learning base were
taught most often and most effectively.

---

## 9. Domain Report

For every domain:

| Domain | Rows | Branches | Sub-phases covered | Avg DPO ms/branch | Max DPO ms/branch | Peak RAM MB | Disk MB | Pass rate | First resource review row |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---|

And the per-step domain chain:

| Step | Row | Branches | Sub-phases | Replay examples | DPO ms | Loss | Chosen reward | Rejected reward | Margin | Accuracy | Parent checkpoint |
|---:|---|---:|---|---:|---:|---:|---:|---:|---:|---:|---|

For the full corpus run, the domain chain for BUG, PLAN, CODE_Q, ARCH,
and REFACTOR will extend to many more steps than the calibration runs.
The accumulation profile across 1,080 rows is the key output — it shows
whether the engine maintained stable learning quality throughout the
entire XIIGen corpus or degraded after a certain number of rows per domain.

---

## 10. Flow Tree Report

Grouped by phase. Shows where in the product lifecycle topology the
XIIGen learning base mapped.

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
```

Also list:

- sub-phases covered in this run
- sub-phases not covered in this run
- new sub-phases first learned in this run
- sub-phases carried forward from prior checkpoint state

For the full corpus run, the "not covered" list is particularly important:
it shows which areas of the development lifecycle the XIIGen corpus does
not address, meaning the engine has no learned capability for those areas
after the full corpus teaching run.

---

## 11. LoRA / DPO Audit

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

Per-domain audit:

| Domain | Max training step | Parent chain depth | Final checkpoint MB | Adapter checkpoint count | Replay count |
|---|---:|---:|---:|---:|---:|

The LoRA audit must flag: inconsistent config, missing parent checkpoint,
unexpectedly large checkpoint growth, replay buffer growth without cap
(A54), DPO time growth after replay growth.

For the full corpus run, the parent chain depth is the most important
column — it shows how many generations of continued learning each domain
adapter accumulated across all 1,080 rows.

---

## 12. Recognition and Routing Report

| Row | Prompt summary | Domain | Domain confidence | Segments | Predicates | Selected sub-phases | Rejected sub-phases | Branch cap hit |
|---|---|---|---:|---:|---|---|---|---|

Summary metrics: domain distribution, average domain confidence, rows
with confidence below 0.40, rows where branch cap was hit, average branch
count, rows where context changed the route.

---

## 13. Set Preparation Report

| Source type | Files/records scanned | Rows extracted | Rows accepted | Rows rejected | Main rejection reasons |
|---|---:|---:|---:|---:|---|

Also: oldest/newest source timestamp, deterministic ordering rule,
duplicate prompt count, rows per domain before and after recognition.

For the full corpus run, the set preparation report covers all source
files in the XIIGen export directory and explains exactly which rows were
included in the 1,080-row teaching set and which were rejected and why.

---

## 14. Beautifier / Result Translation Report

If beautifier/result translation units run during teaching or verification:

| Row | Stage | Beautifier used | Template used | Input chars | Output chars | Elapsed ms | Warnings |
|---|---|---|---|---:|---:|---:|---|

If they do not run:

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

For batch size 200 and the full corpus run, use:

- 2 rows from the first 20 rows
- 2 rows from the middle rows
- 2 rows from the last completed batch

| Regression row | Original verdict | New verdict | Same domain | Same sub-phases | Selected newer unit | Pass |
|---|---|---|---|---|---|---|

For the full corpus run, this check is critical. With 1,080 rows across
multiple domains, the risk of catastrophic forgetting is real. Any
regression failure in the full corpus run must be investigated before
proceeding to the next batch.

---

## 16. Cross-Run Comparison

### 16.1 Calibration cross-run comparison

After all four 200-row calibration runs, generate
`cross-run-comparison.md` comparing all four batch sizes.

Required tables:

**Resource comparison:**

| Batch size | Total elapsed | Peak CPU | Peak RAM | Disk written | Accepted | Resource review | Logic review |
|---:|---:|---:|---:|---:|---:|---:|---:|

**Stage cost comparison:**

| Stage | Batch 20 | Batch 50 | Batch 100 | Batch 200 | Best |
|---|---:|---:|---:|---:|---|

**Domain comparison:**

| Domain | Batch size | Rows | Avg DPO ms/branch | Max step | Accuracy | Reward margin | Resource status |
|---|---:|---:|---:|---:|---:|---:|---|

**Full corpus projections:**

| Projection | Batch 20 | Batch 50 | Batch 100 | Batch 200 |
|---|---:|---:|---:|---:|
| Estimated time for 1,080 rows (hours) | | | | |
| Estimated checkpoint size for 1,080 rows (MB) | | | | |

**Recommendation:**

```text
Recommended batch size for full 1,080-row XIIGen corpus run: <20|50|100|200>
Reason:
  - resource threshold result at 200 rows
  - projected resource usage at 1,080 rows
  - accumulation onset step
  - estimated total time for full corpus run
```

### 16.2 Full corpus vs calibration comparison

After the full corpus run, run the comparison script a second time
comparing the full corpus run against the calibration run at the same
batch size:

```bash
python3 compare_detailed_reports.py \
  XIIGEN-DOCS-BATCH-COMPARISON/batch-{N}/detailed-report.json \
  XIIGEN-FULL-CORPUS/detailed-report.json
```

This comparison answers: did the engine's behaviour at 1,080 rows match
what the calibration predicted at 200 rows? If accumulation onset was
predicted at step 7 for BUG at batch size 50, did the full corpus run
stay within that limit across all 1,080 rows?

---

## 17. Implementation Steps

### Phase 1 — Prepare calibration corpus

- `rows-200-fixed.jsonl` (first 200 rows of full XIIGen corpus, in order)
- `rejected-rows.jsonl`
- `set-preparation-report.md`

Gate: 200 accepted rows, deterministic order, no silent rejected rows.

### Phase 2 — Instrument teaching runner

Instrument to emit all stage summary files listed in Section 5.

Gate: dry-run of 2 rows emits every file; every stage S0–S12 present.

### Phases 3–6 — Run the four calibration experiments

Batch 20, Batch 200, Batch 50, Batch 100 in that order.

Gate per run: completes, detailed report generated, regression window run.

### Phase 7 — Calibration cross-run comparison

Generate `cross-run-comparison.md` and `recommendation.md`.

Gate: all four runs represented, recommendation present with projected
time and disk usage for 1,080 rows.

### Phase 8 — Full corpus teaching run

Using the recommended batch size, teach the engine on the complete
XIIGen corpus of approximately 1,080 usable rows.

Gate: all rows processed, `detailed-report.md` written, no unrecovered
resource breach.

### Phase 9 — Full corpus vs calibration comparison

Run the comparison script to confirm the full run matched predictions.

---

## 18. Stop Conditions

Stop and report if:

- Corpus preparation cannot produce 200 valid calibration rows, or cannot
  identify the full 1,080-row corpus.
- Teaching runner cannot emit stage summaries.
- RAM exceeds 90% absolute system usage.
- Disk free space drops below 10%.
- Process crashes or corrupts checkpoint registry.
- Regression window cannot run because prior checkpoint state is missing.

Do not stop merely because CPU/RAM/disk crosses 80%. Mark `RESOURCE_REVIEW`,
finish if safe, report the first crossing point.

For the full corpus run: if a resource breach occurs that makes the
machine unresponsive, pause the run, record the last completed batch and
row, fix A54 if the breach is RAM/accumulation-related, then resume from
the next unprocessed row.

---

## 19. Final Deliverables

The completed work must produce:

**From calibration:**

- `rows-200-fixed.jsonl`
- `set-preparation-report.md`
- `batch-20/detailed-report.md`
- `batch-50/detailed-report.md`
- `batch-100/detailed-report.md`
- `batch-200/detailed-report.md`
- `cross-run-comparison.md`
- `recommendation.md`

**From full corpus run:**

- `XIIGEN-FULL-CORPUS/detailed-report.md` — the primary record of what
  the engine learned from the XIIGen learning base
- `XIIGEN-FULL-CORPUS/subphase-summary.json` — machine-readable learning
  coverage for all sub-phases
- `XIIGEN-FULL-CORPUS/flow-tree-summary.json` — which areas of the
  development lifecycle are now covered by learned units

**Summary to Luba after Phase 8:**

- Recommended batch size and why
- Total time for the full corpus run
- Peak CPU/RAM/disk for the full run
- Sub-phases learned and not learned from the XIIGen corpus
- Best and worst performing domains
- Whether LoRA/DPO was the bottleneck
- Whether cumulative learning stayed stable across all 1,080 rows
- Final flow count and unit count

---

# Plan version: 2.0
# Sections: 19
# Calibration: 4 runs × 200 rows — find the right batch size
# Full corpus: 1 run × ~1,080 rows — teach the engine the XIIGen learning base
# The full corpus run is the actual goal of this work
