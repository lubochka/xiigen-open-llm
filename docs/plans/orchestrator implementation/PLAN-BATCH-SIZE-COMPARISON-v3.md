# Plan — Batch Size Comparison Experiment
# Version: 3.0 — 2026-05-07
# Changes from v2: the overall goal and the role of this experiment are now
#   stated explicitly. The 200-row runs are calibration. The output of this
#   plan is a batch size recommendation, not a finished teaching run.
#   The full 1,080-row XIIGen corpus teaching run is a separate subsequent
#   step that uses the recommended batch size from this plan.

---

## Overall goal

The goal is to teach the engine on the complete XIIGen learning base that
has been prepared: approximately 1,080 usable session rows exported from
XIIGen documentation and session history.

Teaching 1,080 rows at the wrong batch size will either be slower than
necessary (accumulation cost dominating) or will breach resource limits
mid-run (RAM or disk exhausted before all rows complete). Neither outcome
is acceptable for a run of this length.

This plan exists to find the right batch size before committing to the
full corpus run. It does this by running four batch sizes against a
200-row calibration sample drawn from the same corpus. The 200-row sample
is large enough to observe accumulation behaviour, resource pressure, and
learning signal quality at each batch size, without spending the full time
cost of a 1,080-row run on a configuration that may prove unsuitable.

The output of this plan is a single recommendation: which batch size to
use for the full corpus run. Once that recommendation is produced, the
full 1,080-row teaching run begins under a separate run identifier.

```
This plan:     calibration — 200 rows × 4 batch sizes → batch size recommendation
Next step:     full corpus teaching — 1,080 rows at the recommended batch size
```

---

## What we are measuring and why

Batch size affects three competing forces:

- **Shared overhead**: larger batches amortise tokeniser init and session
  setup across more rows — cheaper per row at low step counts
- **Accumulation**: larger batches mean more DPO steps per domain before
  the session resets — optimizer state grows, costing more per step
- **Resource pressure**: larger batches hold more state in memory and
  write larger checkpoints to disk

The correct stopping criterion is not an internal DPO metric. It is whether
the machine stays healthy. A run is acceptable as long as CPU, RAM, and disk
stay under 80% throughout. A run that exceeds any threshold is not viable
regardless of how fast it trains per row.

For the full 1,080-row corpus, resource pressure and accumulation matter
far more than they do for a 20-row session. A batch size that looks
borderline at 200 rows will be catastrophic at 1,080 rows. The calibration
sample is intentionally conservative.

---

## Resource thresholds — definitions

### CPU — `CpuPercent` field

Direct percentage. Threshold: **80.0%**.

Both prior runs sit at 11–16% average, 17% peak. CPU is not currently
the bottleneck for either runner mode.

Gate: if any batch/row reports `CpuPercent > 80.0`, the run has exceeded
the CPU threshold.

### RAM — derived from `WorkingSetDeltaMb`

`WorkingSetDeltaMb` is the change in working set during a single batch or
row — not the absolute working set. To apply an 80% threshold against total
RAM, the absolute working set must be tracked:

```
absolute_ws(N) = baseline_ws + sum(WorkingSetDeltaMb for rows 1..N)
```

Before each run starts, record the baseline working set of the process.

Gate: if `absolute_ws` at any point exceeds `0.80 × total_RAM_MB`, the run
has exceeded the RAM threshold.

`ManagedAllocationDeltaMb` tracks GC allocation pressure — not resident
memory. Track it as a secondary indicator of GC stress.

### Disk — `CheckpointBytesTotal` field

`CheckpointBytesTotal` is cumulative bytes written to checkpoint files
since the run started.

Gate: if `CheckpointBytesTotal` exceeds `0.80 × available_disk_bytes`
at any point, the run has exceeded the disk threshold.

Disk is unlikely to be the bottleneck at 200 rows (~60–70 MB expected)
but the projection to 1,080 rows is the critical output. If 200 rows
produces 70 MB of checkpoints, 1,080 rows will produce approximately
378 MB. That projection must appear explicitly in the recommendation.

---

## Fixed parameters across all four calibration runs

| Parameter | Value |
|---|---|
| Source corpus | First 200 rows of XIIGen session `rows.jsonl` (same file, all runs) |
| Row count | 200 — calibration sample only, not the full corpus |
| Resource review threshold | Same value as today's per-row run |
| Domain order | Natural order in `rows.jsonl` — no shuffling |
| Hardware | Same machine, no other load during runs |
| Codebase | Same commit — all four runs before any code changes |

The 200 rows are drawn from the same source file that will be used for the
full corpus run. They are not a separate dataset — they are the first 200
rows of the same sequence the full run will process.

---

## Four calibration runs

| Run | Batch size | Expected batches |
|---|---|---|
| A | 20 | 10 batches |
| B | 50 | 4 batches |
| C | 100 | 2 batches |
| D | 200 | 1 batch |

---

## Pre-run checklist (before each run)

```
□ No other processes consuming significant CPU or RAM
□ Record baseline working set (MB) — from Task Manager before run starts
□ Record total RAM (MB) — from Task Manager System tab
□ Record available disk space (GB) — on the drive holding experiment folder
□ Note start time
```

These three numbers are recorded in `pre-run-stats.json` before each run.

```json
{
  "BatchSize": 20,
  "BaselineWorkingSetMb": 0,
  "TotalRamMb": 0,
  "AvailableDiskMb": 0,
  "CpuThreshold": 80.0,
  "RamThresholdMb": 0,
  "DiskThresholdMb": 0
}
```

`RamThresholdMb = TotalRamMb × 0.80`
`DiskThresholdMb = AvailableDiskMb × 0.80`

---

## Output format per calibration run

```
results/
  run-batch-{SIZE}/
    pre-run-stats.json         — baseline readings before run starts
    progress.txt               — one line per batch
    batch-summary.jsonl        — one record per batch
    comparison-metrics.json    — normalised metrics for comparison script
    checkpoints/               — one folder per batch
```

### `comparison-metrics.json` schema

```json
{
  "BatchSize": 20,
  "TotalRows": 200,
  "TotalBatches": 10,
  "AcceptedRows": 0,
  "ResourceReviewRows": 0,
  "TotalElapsedMs": 0,
  "AvgElapsedMsPerRow": 0,
  "MedianElapsedMsPerRow": 0,
  "MaxElapsedMsPerRow": 0,
  "AvgDpoMsPerBranch": 0,
  "MaxDpoMsPerBranch": 0,
  "AvgCpuPercent": 0,
  "PeakCpuPercent": 0,
  "CpuThresholdBreached": false,
  "AvgWorkingSetDeltaMb": 0,
  "PeakAbsoluteWorkingSetMb": 0,
  "RamThresholdBreached": false,
  "RamThresholdBreachedAtBatch": null,
  "TotalManagedAllocationMb": 0,
  "AvgManagedAllocationMbPerRow": 0,
  "TotalCheckpointMb": 0,
  "CheckpointKbPerRow": 0,
  "DiskThresholdBreached": false,
  "DpoMsAtStep1PerBranch": 0,
  "DpoMsAtMaxStepPerBranch": 0,
  "MaxStepReached": 0,
  "FirstResourceReviewAtRow": null,
  "FirstResourceReviewAtStep": null,
  "ProjectedCheckpointMbAt1080Rows": 0,
  "ProjectedElapsedHoursAt1080Rows": 0
}
```

The two `Projected*` fields are computed as:
- `ProjectedCheckpointMbAt1080Rows = (TotalCheckpointMb / 200) × 1080`
- `ProjectedElapsedHoursAt1080Rows = (AvgElapsedMsPerRow × 1080) / 3_600_000`

These projections appear in every comparison-metrics.json so the
recommendation script can surface them directly.

---

## Execution order

1. Run A (batch=20) — establishes baseline, fastest to complete
2. Run D (batch=200) — worst-case stress test, run while machine is cold
3. Run B (batch=50)
4. Run C (batch=100)

Wait 60 seconds between runs. Re-record baseline working set before each.

---

## Phase 1 — Prepare the calibration input

Extract exactly 200 rows from the XIIGen session corpus:

```
C:\Users\lubao\Documents\xiigen\llm mvp\
  external-sources-training-sets\
    batch-comparison-experiment\
      rows-200-fixed.jsonl
```

These rows are taken in natural order from the beginning of the full
corpus file. The full corpus file is the same one the subsequent 1,080-row
teaching run will use — the calibration rows are rows 1–200 of the same
sequence, not a separate extraction.

Verify: `wc -l rows-200-fixed.jsonl` = 200.

---

## Phase 2 — Configure four experiment folders

```
experiments-v2/
  BATCH-COMPARISON-20/
  BATCH-COMPARISON-50/
  BATCH-COMPARISON-100/
  BATCH-COMPARISON-200/
```

Each folder has its own run configuration pointing at `rows-200-fixed.jsonl`
with the appropriate batch size.

---

## Phases 3–6 — Execute the four calibration runs

Run A (batch=20): 10 batches, ~20 minutes expected.
Run D (batch=200): 1 batch — monitor actively for RAM pressure.
Run B (batch=50): 4 batches.
Run C (batch=100): 2 batches.

For each run: record `pre-run-stats.json` before starting. Do not abort
on threshold breach — finish the run and record the first breach point
as data. After completion write `comparison-metrics.json` including the
1,080-row projections.

---

## Phase 7 — Run the comparison script and produce recommendation

```bash
python3 compare_batch_runs.py \
  BATCH-COMPARISON-20/results/comparison-metrics.json \
  BATCH-COMPARISON-50/results/comparison-metrics.json \
  BATCH-COMPARISON-100/results/comparison-metrics.json \
  BATCH-COMPARISON-200/results/comparison-metrics.json
```

### Output 1 — Resource threshold summary

| Metric | Batch 20 | Batch 50 | Batch 100 | Batch 200 | Threshold |
|---|---|---|---|---|---|
| Peak CPU % | | | | | 80% |
| CPU breached | | | | | — |
| Peak absolute RAM (MB) | | | | | 80% of total |
| RAM breached at batch | | | | | — |
| Total checkpoint (MB) | | | | | 80% of disk |
| Disk breached | | | | | — |

### Output 2 — Performance per row

| Metric | Batch 20 | Batch 50 | Batch 100 | Batch 200 |
|---|---|---|---|---|
| Avg elapsed/row (ms) | | | | |
| Avg DPO/branch (ms) | | | | |
| Avg GC alloc/row (MB) | | | | |
| Checkpoint KB/row | | | | |
| Accept rate | | | | |

### Output 3 — Full corpus projections

| Projection | Batch 20 | Batch 50 | Batch 100 | Batch 200 |
|---|---|---|---|---|
| Estimated total time for 1,080 rows | | | | |
| Estimated total checkpoint size for 1,080 rows | | | | |

### Output 4 — Recommendation

```
Viable batch sizes (all thresholds respected throughout):  [list]
Recommended batch size for full 1,080-row corpus run:      [value]
Reason:
  - resource threshold result at 200 rows
  - projected resource usage at 1,080 rows
  - accumulation onset step
  - estimated total time for full corpus run
```

---

## Phase 8 — Full corpus teaching run

Once the recommendation is produced, the full 1,080-row corpus run begins.

Input:
```
C:\Users\lubao\Documents\xiigen\
  [full XIIGen session corpus — approximately 1,080 usable rows]
```

Run configuration:
- Batch size: as recommended by Phase 7
- Source file: the full corpus file (not `rows-200-fixed.jsonl`)
- Output folder: `experiments-v2/XIIGEN-FULL-CORPUS/`
- Same resource thresholds: CPU 80%, RAM 80%, disk 80%
- Same reporting format as the calibration runs

The full corpus run produces the same `detailed-report.json`,
`detailed-report.md`, and `comparison-metrics.json` as each calibration
run. The cross-run comparison script can be re-run after the full corpus
run completes to compare the full run against the calibration runs at the
same batch size.

This is the run that actually teaches the engine the XIIGen learning base.
The calibration runs in Phases 3–6 exist only to make Phase 8 succeed.

---

## What the calibration results tell us about the full run

**If all four batch sizes stay under all thresholds at 200 rows:**
Batch=200 is viable for the calibration sample. However, check the 1,080-row
projection. If projected RAM at 1,080 rows is above 80% of total RAM, the
safe batch size for the full run may be lower than the 200-row result suggests.
Accumulation grows with session length, not just row count per batch.

**If batch=200 breaches RAM at 200 rows:**
The replay buffer cap fix (A54) is a prerequisite before the full corpus
run at any large batch size. Do not start Phase 8 until A54 is resolved.

**If batch=100 and batch=200 both breach RAM but batch=50 does not:**
Use batch=50 for Phase 8. The 1,080-row run will take approximately
`1080 × avg_elapsed_ms_per_row_at_batch50 / 3_600_000` hours.

**If CPU stays under 80% for all runs:**
CPU is not the bottleneck. The accumulation problem is a memory and time
problem only. The recommended batch size should be chosen on RAM and elapsed
time, not CPU.

**Disk projection:** if 200 rows at the recommended batch size produces
X MB of checkpoints, the full run will produce approximately `X × 5.4` MB.
Confirm that estimate is below 80% of available disk before starting Phase 8.

---

## Success criteria

**Calibration (Phases 1–7) is complete when:**
- All four `pre-run-stats.json` files recorded
- All four `comparison-metrics.json` files written including projections
- Comparison script produces all four outputs
- Recommendation explicitly names a batch size and states expected
  total time and disk usage for the 1,080-row full corpus run

**Full corpus run (Phase 8) is complete when:**
- All 1,080 rows processed (or all viable rows after rejection)
- `detailed-report.md` written for the full run
- No unrecovered resource threshold breach

---

# Plan version: 3.0
# Phases: 8
# Calibration runs: 4 (batch sizes 20, 50, 100, 200) × 200 rows each
# Full corpus run: 1 run at recommended batch size × ~1,080 rows
# Purpose of calibration: find the batch size for the full corpus run
# The full corpus run is the actual goal of this work
