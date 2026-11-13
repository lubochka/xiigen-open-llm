# WHAT TO DO / WHAT NOT TO DO - v14

Date: 2026-05-08

This document supersedes `WHAT-TO-DO-WHAT-NOT-TO-DO-v13.md`.

Purpose: preserve the operational rules that must not be forgotten between sessions.

## Current Prime Directive

Do not restart full XIIGen internal teaching until the training input is a proper DPO pair DTO set, not just source registry rows.

## What To Do

### Documentation

- Keep this file current. When a new hard rule is discovered, add it here before starting a long run.
- Create new versioned documents instead of overwriting old session state.
- Keep a clear separation between confirmed facts, inferred facts, and open questions.
- Mark stale documents as superseded in the document register.

### Source Registry

- Confirm the canonical internal XIIGen source location before running internal teaching.
- Treat `rows.jsonl` source registry rows as raw material only.
- Preserve source provenance in every generated DTO.
- Keep internal XIIGen knowledge separate from external source training sets unless the user explicitly approves mixing them.

### DPO Pair Authoring

- Convert source rows into explicit DPO pair DTOs before training.
- Include chosen and rejected answers in each final pair.
- Include `contract_signature`, `pair_kind`, `rejected_type`, `overlap_ratio`, `annotation_quality`, and compiled prompt metadata.
- Use realistic wrong-but-plausible rejected answers.
- Use explicit corruption markers such as `[WRONG: ...]` when the guide calls for them.
- Preserve segment/domain/subphase provenance for multi-route prompts.
- Run pair quality gates before invoking the trainer.

### Compiled Prompt Usage

- Use DevelopmentPromptCompiler output as the canonical prompt representation for training and inference where the domain is developer natural language.
- Keep expression or math-like units on their own deterministic normalization pipeline when DPC would corrupt the domain.
- Do not let raw user prompt text become the main DPO prompt side for developer prompt teaching.

### Training Architecture

- Keep training implementation in `18.CapabilityTraining`.
- Keep shared contracts in `00.Core`.
- Keep `21.AgentInfrastructure` as orchestration only.
- Call `ILogicalCapabilityTrainer` through the interface from orchestration code.
- Keep DPO step counts real. Do not report hardcoded step counts.
- Keep RAM release boundaries between physical batches.

### Monitoring

- Report the full training picture when monitoring is requested:
  - current batch
  - current domain
  - completed domains
  - completed physical batches
  - latest completed domain timing
  - DPO timing
  - CPU percent
  - process or process-tree RAM MB and RAM percent
  - run disk size
  - checkpoint file count and size
  - whether CPU, RAM, or disk gate reached 80 percent
- Do not show C: used percent unless the user asks for it.
- Do not treat DPO elapsed time or `RESOURCE_REVIEW` as a stopper by itself.

### Testing

- Run `dotnet test` live before claiming a current floor.
- Do not reuse remembered test counts as proof.
- Add tests for every new permanent contract.
- Fix production code rather than weakening test assertions.

## What Not To Do

### Teaching Runs

- Do not restart internal full-set teaching from source rows that are not DPO pair DTOs.
- Do not use `rows-200-fixed.jsonl` as if it were a canonical master training set.
- Do not silently use an `external-sources-training-sets` path for internal XIIGen learning.
- Do not use `external-sources-training-sets/wave-2` for internal XIIGen teaching. It is external.

### DPO Pairs

- Do not use `Do not generate a capability for sub-phase ...` as the rejected side.
- Do not create easy negative examples that only teach string avoidance.
- Do not use a chosen-only record as a DPO pair.
- Do not explode multi-route prompts into independent pairs without segment-aware provenance.
- Do not collapse multi-domain prompts into one global domain label.

### Architecture

- Do not add new DPO, replay, convergence, checkpoint, or trainer cache logic to `21.AgentInfrastructure`.
- Do not create duplicate deterministic classifier paths when an `ILogicalUnit` path exists.
- Do not bypass registry-resolved logical units with direct `new` construction unless the object is a local value helper and is explicitly documented as such.

### Experiments

- Do not treat experiment success as production wiring.
- Do not leave stale experiment state JSON claiming a gap remains after the code has been wired.
- Do not mix external experiment corpora with internal XIIGen teaching sources.

### Monitoring And Reports

- Do not hide the current domain or batch.
- Do not report only resource usage when training phase timing is requested.
- Do not continue monitor noise after the user says stop.

## Current Hard Blockers

1. Proper DPO pair DTO builder or authored set is missing.
2. Forbidden generic rejected strings still exist in capability training code.
3. Canonical internal source path is unresolved.
4. Current source rows need segment-aware conversion before full teaching.

## Safe Next Step

**Track A (Orchestration/project 26):** Continue from `PLAN-A2155`; build next clear `3.1`/`3.3`/`3.4` routing rows; run standalone then combo gate before promoting past V309.

**Track B (Capability/project 18):** Write the DPO pair DTO builder and validation gate plan, then implement it before any new full internal teaching run.

**Track C (Translation/project 27):** Continue the `27.TranslationDomain` 100+100 base loop. Current focus: run P0 pilot (build approved EN/RU pilot-100 word lists, construct the first 100 training sets and 100 never-seen test sets, run the runner). Do not substitute unrelated data for the selected pair-set curriculum. Do not start professional domain fine-tuning until `runtimeProductAccuracy ≥ 0.95` on the general base language. Phase 25 code repairs (UTF-8 rule tags, 95% gate, mojibake test data) are pending and are the code model's responsibility. See `docs/plans/STATE-PARALLEL-DOMAIN-TRAINING.json` and `docs/plans/TRANSLATION-CORPUS-SOURCING-PLAN-v1.md §13` for full loop definition.
