# DOCUMENT STATE REGISTER — 2026-05-14 — v12

This document supersedes `DOCUMENT-STATE-REGISTER-2026-05-08-v11.md`.

---

## Current Active Documents

| Area | Active Document | Status |
|---|---|---|
| Session state | `SESSION-STATE-2026-05-14-v20.md` | Current |
| Operational rules | `WHAT-TO-DO-WHAT-NOT-TO-DO-v14.md` | Current — no new rules added this session, but two planning documents require saving to the repository before the next session |
| Implementation documentation | `IMPLEMENTATION-DOCUMENTATION-v9.md` | Current base — no new delta written this session; corrections captured in session state v20 instead |
| Document register | `DOCUMENT-STATE-REGISTER-2026-05-14-v12.md` | Current |

---

## Superseded Documents

| Document | Superseded By | Reason |
|---|---|---|
| `SESSION-STATE-2026-05-08-v19.md` | `SESSION-STATE-2026-05-14-v20.md` | v19 did not reflect projects 25 and 26 being in-progress on disk; stale test count in `build-state.json` was not flagged; two new planning documents were not registered |
| `DOCUMENT-STATE-REGISTER-2026-05-08-v11.md` | `DOCUMENT-STATE-REGISTER-2026-05-14-v12.md` | v11 did not register the two new planning documents |

---

## New Documents Registered This Session

These two documents were uploaded on 2026-05-14 and have not yet been saved to the repository `docs/plans/` folder. They must be copied there before the next executor session.

| Document | Intended repository path | Status |
|---|---|---|
| `DISTRIBUTED-MODEL-ARCHITECTURE-PLAN-v1.md` | `docs/plans/DISTRIBUTED-MODEL-ARCHITECTURE-PLAN-v1.md` | Not yet committed to repository |
| `ORCHESTRATION-TRAINING-PROTOCOL-v1.md` | `docs/plans/ORCHESTRATION-TRAINING-PROTOCOL-v1.md` | Not yet committed to repository |

---

## Documents Added After v12 (2026-05-29)

The following documents were committed to the repository after this register was written.
They are recorded here for completeness.

| Document | Repository path | Committed | Status |
|---|---|---|---|
| `GRAMMAR-RULE-DPO-REGISTRY-GUIDE-v1.md` | `docs/plans/GRAMMAR-RULE-DPO-REGISTRY-GUIDE-v1.md` | 2026-05-29 (phase 24, commit `a5e74154`) | In repository |
| `LANGUAGE-RULE-SOURCE-REGISTRY-v1.md` | `docs/plans/LANGUAGE-RULE-SOURCE-REGISTRY-v1.md` | 2026-05-29 (phase 24, commit `a5e74154`) | In repository |
| `TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` | `docs/plans/TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` | 2026-05-29 (phase 17, commit before `a5e74154`) | In repository |
| `TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md` | `docs/plans/TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md` | 2026-05-29 (direction 1 scaffolding) | In repository |
| `TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md` | `docs/plans/TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md` | 2026-05-29 (direction 1 scaffolding) | In repository |
| `STATE-PARALLEL-DOMAIN-TRAINING.json` | `docs/plans/STATE-PARALLEL-DOMAIN-TRAINING.json` | 2026-05-29 (ongoing state file) | In repository — live |
| `PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md` | `docs/plans/PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md` | 2026-05-29 | In repository |

---

## Current Source Guides — Internal Learning Domain

These guides remain relevant to the orchestration DPO continuation work (Track A):

- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v2.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\EXAMINATION-METHOD-ORCHESTRATION-v9.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\XIIGEN-DPO-INTERNAL-TEACHING-PLAN-v2.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\XIIGEN-INTERNAL-SOURCE-REGISTRY-v2.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\ORCHESTRATOR-TEACHING-PLAN-v4.md`

The `ORCHESTRATION-TRAINING-PROTOCOL-v1.md` uploaded this session now serves as the canonical procedure document for the orchestration DPO training cycle and supersedes any informal notes on that topic.

---

## Current Project State Summary

Training separation is implemented and confirmed on disk:

- `00.Core` holds shared logical and training contracts.
- `18.CapabilityTraining` holds capability trainer implementation with confirmed forbidden pattern locations.
- `21.AgentInfrastructure` holds orchestration and calling interfaces.
- `25.ModelServer` holds model server contracts and implementations — confirmed in-progress on disk as of this review.
- `26.TrainableRouter` holds trainable routing flow, DPO pair builder, and training runner — confirmed in-progress on disk as of this review.

The current unresolved issue remains training data shape for the capability teaching track (project 18). The orchestration training track (project 26) has a working baseline at V309 and a clear documented continuation path.

---

## Current Blocker Register (brief reference — full detail in session state v20)

| Id | Blocker | Status |
|---|---|---|
| B-v20-001 | Project 18 teaching input is source-row registry format, not DPO pair DTOs | Open |
| B-v20-002 | Forbidden generic rejected strings in two project 18 files at confirmed line numbers | Open |
| B-v20-003 | Canonical internal source path for capability teaching is unresolved | Open |
| B-v20-004 | Multi-route source rows need segment-aware pair construction | Open |
| B-v20-005 | Full internal capability teaching must stay paused until pair validation gate exists | Open |
| B-v20-006 | `SUBPROJECT-MAP.md` does not list projects 25 and 26 | **Resolved** — 2026-05-29: SUBPROJECT-MAP.md now lists 25, 26, and 27.TranslationDomain (Active, 44 tests) |
| B-v20-007 | `build-state.json` test count is stale (shows 499 from 2026-04-30) | Open |

---

## Stale Files Flagged (action required in next executor session)

| File | What is stale | Required update |
|---|---|---|
| `docs/SUBPROJECT-MAP.md` | ~~Does not list projects 25 or 26~~ | **Resolved 2026-05-29** — projects 25, 26, and 27.TranslationDomain added with Active status |
| `build-state.json` | Test count shows 499 from 2026-04-30; projects 25 and 26 are absent from projects array | Run live `dotnet test`, record actual count, add projects 25 and 26 |
| `CLAUDE.md` | ~~Test baseline says 916 passed as of 2026-05-01~~ | **Resolved 2026-05-29** — baseline updated to ≥ 1488 passed (2026-05-28 gate) |
