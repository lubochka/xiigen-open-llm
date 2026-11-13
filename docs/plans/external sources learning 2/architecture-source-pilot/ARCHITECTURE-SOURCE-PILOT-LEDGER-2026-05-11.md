# Architecture Source Pilot Ledger — 2026-05-11

## Purpose

This ledger records which external-source instruction/material batches are allowed into orchestration-focused DPO testing and which are blocked or quarantined. The goal is to improve ARCH/PLAN orchestration, skill, and planning behavior without polluting the run with generic code-fix or chat-instruction rows.

## Hard License Gate

| Source class | Status | Reason |
|---|---|---|
| MIT OpenCourseWare | BLOCK | CC BY-NC-SA blocks commercial model-training use. |
| Stack Exchange dump | BLOCK | Download terms are disputed for LLM training; practical legal risk is too high. |
| Wikipedia | QUARANTINE | CC BY-SA share-alike source; only isolated batches with explicit attribution and no merge into clean production sets. |
| arXiv | PER-ITEM CHECK | Only papers whose metadata explicitly says CC-BY are allowed. |
| DOAB/OAPEN | PER-ITEM CHECK | Only books filtered and verified as CC-BY are allowed. |
| S2ORC | PER-ITEM CHECK | Only records with CC-BY license metadata are allowed. |
| IEEE Open | PER-ITEM CHECK | Only CC-BY Open Access papers are allowed. |

## Current Source Status

| Source | License state | Rows built | Status | Notes |
|---|---:|---:|---|---|
| AOSA | CC-BY-3.0 | 200 | CANDIDATE_BATCH20_RUNNING | Full-text architecture prose. 20-row probe: ARCH margin +0.004749, PLAN margin +0.007879. One physical 200-row batch failed early: ARCH margin -0.008999, accuracy 0.044, so the source is being retested as 200 rows split into physical batches of 20. |
| system-design-primer | CC-BY-4.0 | 110 | CANDIDATE_SMALL | Good architecture/system-design source, but strict filter produced only 110 clean rows. |
| mehdihadeli awesome architecture | CC0-1.0 | 76 | REGISTRY_REFERENCE_ONLY | Mostly catalog/link-list material after filtering; not first-choice training text. |
| DovAmir awesome patterns | CC0-1.0 | 13 | REGISTRY_REFERENCE_ONLY | Too sparse for training; usable as source discovery only. |
| simskij awesome architecture | CC0-1.0 | 8 | REGISTRY_REFERENCE_ONLY | Too sparse for training; usable as source discovery only. |
| OASST1/OASST2 generic instruction rows | Dataset-specific | 200 each | REJECTED_FOR_NOW | OASST1 run drifted into CODE_Q and produced negative PLAN margin. Not suitable for orchestration focus without a much stricter extractor. |

## Runner Notes

The comparison runner now supports `--source` and `--trust-row-domain`.

`--source` selects the JSONL row file instead of the old hard-coded source.

`--trust-row-domain` is required for curated external-source ablations where the row builder has already assigned the vetted target domain. Without it, source excerpts containing words such as interface, API, method, error, or failure can be reclassified as CODE_Q or BUG even when the row is an architecture/planning row.

## DPO Quality Notes

For architecture-source ablations, rows are primary-sub-phase only. Earlier rows carried up to five target sub-phases, which made 20 rows expand into 63 branches and turned a source-quality test into a resource-heavy graph stress test.

A single 200-row physical batch can still be too broad for architecture sources: AOSA as one batch grouped 159 ARCH branches into one DPO step and produced a negative reward margin. This is a batch-shape failure, not yet a source rejection. The next accepted test shape is fixed 200 rows with physical batch size 20.

Promotion requires a positive standalone margin, no negative PLAN margin, and a later combo test against the accepted V246 baseline. These source rows are not production-promoted by this ledger.
