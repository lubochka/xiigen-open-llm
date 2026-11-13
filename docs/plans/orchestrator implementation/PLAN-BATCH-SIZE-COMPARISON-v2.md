# Plan — Batch Size Comparison Experiment
# Version: 2.0 — 2026-05-07
# Goal: find the largest batch size where CPU, RAM, and disk stay under 80%
#       during a 200-row teaching run on the XIIGen session corpus.

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

---

## Resource thresholds — definitions

The 80% rule applies to three dimensions. Each is measured differently
because the available fields have different semantics:

### CPU — `CpuPercent` field

Direct percentage. Threshold: **80.0%**.

Both current runs sit at 11–16% average, 17% peak. CPU is not currently
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

Before each run starts, record the baseline working set of the process
(available from Task Manager or `dotnet-counters` on Windows).

Gate: if `absolute_ws` at any point exceeds `0.80 × total_RAM_MB`, the run
has exceeded the RAM threshold.

`ManagedAllocationDeltaMb` is GC allocation pressure — how much was
allocated total, not how much is resident. Yesterday's PLAN 100-row batch
allocated over 1 GB per batch. This is high but not directly comparable to
the 80% RAM threshold. Track it as a secondary indicator of GC pressure.

### Disk — `CheckpointBytesTotal` field

`CheckpointBytesTotal` is cumulative bytes written to checkpoint files
since the run started. Compare against total disk space on the drive
where the experiment folder lives.

Gate: if `CheckpointBytesTotal` exceeds `0.80 × available_disk_bytes`
at any point, the run has exceeded the disk threshold.

From current data: 20 rows writes ~6 MB, 200 rows projects to ~60–70 MB.
Disk is extremely unlikely to be the bottleneck for 200 rows, but track
it explicitly for the 1,080-row full corpus projection.

---

## Fixed parameters across all four runs

| Parameter | Value |
|---|---|
| Source corpus | First 200 rows of XIIGen session `rows.jsonl` (same file, all runs) |
| Row count | 200 (same rows for all four runs) |
| Resource review threshold | Same value as today's per-row run |
| Domain order | Natural order in `rows.jsonl` — no shuffling |
| Hardware | Same machine, no other load during runs |
| Codebase | Same commit — all four runs before any code changes |

Using exactly 200 rows ensures batch=200 sees one batch while batch=20 sees
ten batches — the same rows, grouped differently.

---

## Four runs

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

These three numbers (baseline_ws, total_ram, available_disk) are recorded
in a `pre-run-stats.json` file in the experiment folder before each run
starts. The comparison script reads them to compute the absolute thresholds.

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

## Output format per run

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
  "FirstResourceReviewAtStep": null
}
```

---

## Execution order

1. Run A (batch=20) — establishes baseline, fastest to complete
2. Run D (batch=200) — worst-case stress test, run while machine is cold
3. Run B (batch=50)
4. Run C (batch=100)

Wait 60 seconds between runs. Re-record baseline working set before each.

---

## Phase 1 — Prepare the input

Extract exactly 200 rows from the XIIGen session corpus:

```
C:\Users\lubao\Documents\xiigen\llm mvp\
  external-sources-training-sets\
    batch-comparison-experiment\
      rows-200-fixed.jsonl
```

This file is the single source of truth. Created once, not modified.
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

## Phase 3 — Execute Run A (batch=20)

Record `pre-run-stats.json`. Start run. Expected: 10 batches, ~20 minutes.

Monitor: if `CpuPercent` exceeds 80% at any batch, note the batch number
but continue — the run still completes so the full profile is captured.
Same for RAM and disk threshold breaches — continue but record where
each threshold was first crossed.

After completion:
- Record checkpoint folder size: `du -sh checkpoints/`
- Write `comparison-metrics.json`
- Note: was any threshold breached? At which batch?

---

## Phase 4 — Execute Run D (batch=200)

Record `pre-run-stats.json`. This is a single batch of 200 rows.

Monitor actively: if RAM threshold is breached mid-batch, note the
step count and elapsed time but do not abort — completing the run
gives us the full accumulation profile across all 200 rows.

This run is most likely to show both RAM pressure (large GC allocation
from multi-step accumulation) and the highest DPO cost per branch
at late steps.

After completion: record all metrics as Phase 3.

---

## Phase 5 — Execute Run B (batch=50)

Record `pre-run-stats.json`. Expected: 4 batches.
After completion: record all metrics.

---

## Phase 6 — Execute Run C (batch=100)

Record `pre-run-stats.json`. Expected: 2 batches.
After completion: record all metrics.

---

## Phase 7 — Run the comparison script

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

### Output 2 — Performance per row (only for runs within all thresholds)

| Metric | Batch 20 | Batch 50 | Batch 100 | Batch 200 |
|---|---|---|---|---|
| Avg elapsed/row (ms) | | | | |
| Avg DPO/branch (ms) | | | | |
| Avg GC alloc/row (MB) | | | | |
| Checkpoint KB/row | | | | |
| Accept rate | | | | |

### Output 3 — Accumulation profile

For each run, DPO cost per branch by step number — shows where cost
starts rising within a batch.

### Output 4 — Recommendation

```
Viable batch sizes (all thresholds respected throughout):  [list]
Recommended batch size:  [the largest viable size, for best throughput]
First threshold to breach for each non-viable size:  [CPU/RAM/Disk at step N]
```

---

## What the results tell us

**If all four runs stay under all thresholds:** batch=200 is viable and
the accumulation problem is less severe than the current data suggests.
Choose the largest batch size that gives the best time per row.

**If batch=200 breaches RAM:** the optimizer state accumulates more memory
than the machine can safely hold for 200 rows in one session. The
replay buffer cap fix (decision A54) becomes a prerequisite before
running larger batches.

**If batch=100 and batch=200 both breach RAM but batch=50 does not:**
the safe batch size ceiling is 50. Run future teaching sessions at 50.

**If CPU stays under 80% for all runs:** confirms CPU is not the
bottleneck and the accumulation problem is a memory/time problem only.

**Disk** is expected to stay well under 80% for all 200-row runs.
Its value is the projection: `checkpoint_KB_per_row × 1080_rows` gives
the estimated disk cost for a full corpus teaching run.

---

## Success criteria

The experiment is complete when:
- All four `pre-run-stats.json` files are recorded before their runs
- All four `comparison-metrics.json` files are written after their runs
- The comparison script produces all four outputs
- The recommendation names at least one viable batch size

Runs do not need to complete without threshold breaches. The breaches are
the data — they tell us exactly where each batch size becomes unsafe.

---

# Plan version: 2.0
# Phases: 7
# Runs: 4 (batch sizes 20, 50, 100, 200)
# Input: first 200 rows of XIIGen session corpus
# Stop condition per run: none — complete all runs even with breaches,
#                         record breach point as data
# Viability criterion: all three thresholds (CPU 80%, RAM 80%, disk 80%)
#                      respected throughout the entire 200-row run
# Key output: largest viable batch size + full projection to 1080-row corpus
