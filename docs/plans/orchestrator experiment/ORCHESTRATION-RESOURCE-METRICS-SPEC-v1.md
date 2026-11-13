# Orchestration Resource Metrics Specification
# Version: 1.0 - 2026-05-05

---

## Purpose

This document defines the resource measurements required for every teaching
cycle and every learned-unit prompt run.

The point is simple: the architecture must stay effective on the real PC, not
only in theory. The Helix Toolkit attempt showed that visual or model ambition
does not matter if the machine becomes unusable.

---

## Measurement Scope

Each record measures one of two events:

```text
teaching_run
prompt_run
```

A teaching run builds or updates a learned unit model.

A prompt run executes a learned or deterministic unit against one user prompt.

---

## Required JSON Shape

```json
{
  "measurement_id": "O1-splitter-v1-train",
  "timestamp_utc": "2026-05-05T00:00:00Z",
  "event_type": "teaching_run",
  "scenario_id": "O1",
  "unit_role": "orchestration-prompt-splitter",
  "model_version": "v1",
  "training_row_count": 7,
  "held_out_row_count": 5,
  "corpus_bytes": 0,
  "model_bytes_before": 0,
  "model_bytes_after": 0,
  "teaching_elapsed_ms": 0,
  "feature_extraction_elapsed_ms": 0,
  "model_build_elapsed_ms": 0,
  "model_load_elapsed_ms": 0,
  "prompt_process_elapsed_ms": 0,
  "cpu_ms_before": 0,
  "cpu_ms_after": 0,
  "cpu_ms_delta": 0,
  "working_set_mb_start": 0,
  "working_set_mb_peak": 0,
  "private_memory_mb_peak": 0,
  "managed_allocated_mb_delta": 0,
  "disk_read_mb_delta": 0,
  "disk_write_mb_delta": 0,
  "process_left_alive": false,
  "notes": ""
}
```

All numeric values must be measured. Unknown values use `null`, not zero.

---

## Windows Measurement Method

Use .NET runtime measurements first:

```text
Stopwatch for elapsed time;
Process.GetCurrentProcess().TotalProcessorTime for CPU time;
Process.WorkingSet64 for current RAM;
Process.PrivateMemorySize64 for private RAM;
GC.GetTotalAllocatedBytes for managed allocation;
FileInfo.Length for model and corpus file size.
```

For disk read/write totals, use process IO counters if available. If process IO
counters are not available in the current runtime, write `null` and add the note
`process_io_counters_unavailable`.

No measurement path may keep a profiler or inspector process running after the
prompt run finishes.

---

## Required Markdown Summary

Every JSON metrics file has a short markdown companion.

The markdown summary contains:

```text
scenario id
unit role
model version
row counts
teaching time
prompt processing time
peak RAM
CPU delta
model size
whether any process was left alive
quality measurements
next machine action
```

The summary is for human review. The JSON is the machine evidence.

---

## Quality Metrics Linked To Resource Metrics

Resource use without quality is not meaningful. Every prompt run includes:

```text
selected_sub_phase_recall
selected_sub_phase_precision
transition_recall
dependency_violation_count
context_loss_count
arbiter_missing_count
knowledge_edge_invention_count
rejected_item_reopened_count
human_result_sections_present
```

This prevents a fast unit from being accepted when it is logically wrong.

---

## Comparison Rows

Each teaching cycle records three comparisons:

```text
deterministic_fixture
learned_v1
learned_v2_after_one_correction
```

The deterministic fixture is not the goal. It is the local oracle used to
identify whether learning is improving, drifting, or overfitting.

---

## Resource Budget Language

This specification does not set a universal CPU or RAM number because corpus
size will grow. It requires trend visibility.

The following must be visible in evidence:

```text
does model size grow linearly with training rows;
does teaching time grow predictably with training rows;
does prompt time stay small enough for interactive use;
does RAM return to baseline after the run;
does any process remain alive after the run.
```

If a run is slow but explains its cost and stays stable, it is a tuning issue.
If a run leaves processes alive or grows memory without explanation, it is an
architecture issue.

---

## Last Updated

2026-05-05.
