# DOCUMENT STATE REGISTER - 2026-05-08 - v11

This document supersedes `DOCUMENT-STATE-REGISTER-2026-05-07-v10.md`.

## Current Active Documents

| Area | Active Document | Status |
|---|---|---|
| Session state | `SESSION-STATE-2026-05-08-v19.md` | Current |
| Operational rules | `WHAT-TO-DO-WHAT-NOT-TO-DO-v14.md` | Current |
| Implementation documentation | `IMPLEMENTATION-DOCUMENTATION-v9.md` | Current delta over v8 |
| Document register | `DOCUMENT-STATE-REGISTER-2026-05-08-v11.md` | Current |

## Superseded Documents

| Document | Superseded By | Reason |
|---|---|---|
| `SESSION-STATE-2026-05-07-v18.md` | `SESSION-STATE-2026-05-08-v19.md` | v18 predates training separation and DPO DTO findings |
| `WHAT-TO-DO-WHAT-NOT-TO-DO-v13.md` | `WHAT-TO-DO-WHAT-NOT-TO-DO-v14.md` | v13 predates internal source boundary and DPO pair DTO rules |
| `DOCUMENT-STATE-REGISTER-2026-05-07-v10.md` | `DOCUMENT-STATE-REGISTER-2026-05-08-v11.md` | v10 has stale plan status |
| `IMPLEMENTATION-DOCUMENTATION-v8.md` | `IMPLEMENTATION-DOCUMENTATION-v9.md` | v8 remains historical base, v9 corrects the current architecture and blockers |

## Current Source Guides To Keep Active

These guides are relevant to the next DPO pair builder plan:

- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v2.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\EXAMINATION-METHOD-ORCHESTRATION-v9.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\XIIGEN-DPO-INTERNAL-TEACHING-PLAN-v2.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\XIIGEN-INTERNAL-SOURCE-REGISTRY-v2.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\ORCHESTRATOR-TEACHING-PLAN-v4.md`

## Current Project State Summary

The project has moved beyond the v18/v10 state:

- capability training implementation is now separated into `18.CapabilityTraining`
- shared logical/training contracts are in `00.Core`
- orchestration remains in `21.AgentInfrastructure`
- KNN matrix work, compiled prompt integration, deeper model, back translation, feature flags, SIMD matmul, and experiment integration plans have been implemented or mostly implemented
- last known live suite result from the previous implementation phase was `1421 passed, 0 failed`

The current unresolved issue is not code wiring first. It is training data shape.

## Current Blocker Register

| Id | Blocker | Status |
|---|---|---|
| B-v11-001 | Current internal teaching input is source rows, not DPO pair DTOs | Open |
| B-v11-002 | Generic rejected strings still exist in capability training | Open |
| B-v11-003 | Canonical internal source path is unresolved | Open |
| B-v11-004 | Multi-route rows need segment-aware pair construction | Open |
| B-v11-005 | Full internal teaching should stay paused until pair validation exists | Open |

## Current Run Artifacts

Latest monitored run root before the user stopped monitoring:

`C:\Projects\llm_mvp_core\experiments-v2\XIIGEN-DOCS-BATCH-COMPARISON\run-2026-05-08-093325`

Relevant run copy:

`C:\Projects\llm_mvp_core\experiments-v2\XIIGEN-DOCS-BATCH-COMPARISON\run-2026-05-08-093325\rows-200-fixed.jsonl`

Important note: this file is a run copy, not a canonical master source and not a DPO pair set.

## Next Document To Write

The next plan should be a DPO pair DTO builder and validation plan.

It should answer:

1. Where is the canonical internal source registry?
2. How are source rows segmented?
3. How is the chosen side authored?
4. How is the rejected side authored?
5. How is pair quality measured?
6. How does the runner reject malformed pair records before training starts?
