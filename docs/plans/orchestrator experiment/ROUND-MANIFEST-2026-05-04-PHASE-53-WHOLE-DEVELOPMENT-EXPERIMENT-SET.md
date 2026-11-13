# Round Manifest - Phase 53 Whole Development Experiment Set
# Date: 2026-05-04

---

## Purpose

Create the starter experiment set that tests the orchestrator across the whole
development area before broad corpus expansion.

---

## Source Documents Read

- `CORPUS-EXPANSION-PLAN-v3.md`
- `DEVELOPMENT-PHASES-GUIDE-EXTENSION-v1.md`
- `DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md`
- `MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md`
- `HOW-TO-STRUCTURE-A-LEARNING-SET-FROM-AN-EXPERIMENT-v1.md`

`context/interfaces.cs` and `skills/INDEX.md` were requested by the local
AGENTS instructions but are not present under
`C:\Users\lubao\Documents\xiigen\llm mvp`.

---

## Files Produced

- `WHOLE-DEVELOPMENT-AREA-ORCHESTRATION-EXPERIMENT-SET-v1.md`
- `ROUND-MANIFEST-2026-05-04-PHASE-53-WHOLE-DEVELOPMENT-EXPERIMENT-SET.md`

---

## Core Decisions Captured

1. The starter experiment suite comes before full corpus expansion.
2. Whole-development coverage is 31 sub-phases, not 27.
3. WDA-00 topology bootstrap is the first executable experiment.
4. WDA-01 document classification runs only after topology bootstrap passes.
5. WDA-02 through WDA-06 progressively test phase coverage, transition
   continuity, learning, methodology variation, and full lifecycle
   orchestration.
6. Each round must produce a next action item and a zip.

---

## Structural Finding

The corpus expansion plan says "27 sub-phases" and gives 2,700 training
documents, but the phase guide and quota table list 31 sub-phases. The new
experiment set treats 31 as canonical.

Corrected arithmetic:

- 31 sub-phases * 100 training documents = 3,100 training documents
- 31 sub-phases * 20 test documents = 620 held-out test documents

Cross-phase transition documents remain additional.

---

## Verification

Document-only round. No production code changed.

Checks performed:

- Source guidance documents read.
- Experiment set file created.
- Manifest created.
- WDA starter suite contains topology, classification, within-phase,
  transition, learning, methodology variation, and full lifecycle experiments.
- Immediate next action is stated.

Code tests were not run because no C# files changed in this round.

---

## Next Action

Create WDA-00 Round A artifacts:

1. `WDA-00-SOURCE-PACKET.md`
2. `WDA-00-EXPECTED-TRACE.md`
3. `WDA-00-HUMAN-RESULT-CHECKLIST.md`
4. `WDA-00-EXECUTOR-CONTRACT.md`

Then implement WDA-00 Round B as a deterministic topology bootstrap test using
the existing typed topology command surface.
