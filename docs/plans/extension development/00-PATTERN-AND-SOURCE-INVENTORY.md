# Pattern and source inventory - 2026-07-08

## Why these pattern folders were used

The package follows two local product-folder patterns, but does not inherit their readiness claims.

| Pattern folder | What was copied as a pattern | How this package uses it | Boundary |
|---|---|---|---|
| C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build | Library index, execution board, readiness/evidence separation, manifest-style source accounting. | Current package separates evidence/status from implementation authorization and keeps old files baseline-only. | DPO legal build readiness or pass counts are not extension project readiness. |
| C:\Users\lubao\Documents\xiigen\llm mvp\design to code session | Human-readable startup/status narrative, plan target explanation, requirements-first framing. | Current package opens with Russian human-readable status and gives a matrix from Luba requirements to package evidence. | Design-to-code session does not authorize extension execution. |

## Source roots used for this refresh

| Source root | Use | Write authority |
|---|---|---|
| C:\Projects\llm_mvp_core\docs\plans | Primary plan/state source for copied-plans and selected evidence. | Read/copy only. |
| C:\Projects\llm_mvp_core\docs | Supporting HOWTO, project docs, status template, skills. | Read/copy only. |
| C:\Projects\llm_mvp_core\AGENTS.md and CLAUDE.md | Project rules copied into supporting files as reference. | Read/copy only. |
| C:\Users\lubao\Documents\xiigen\llm mvp\extension development | Target package folder. | Only allowed write scope in this slice. |

## Pattern decisions applied

1. Russian-first, human-readable report before machine manifest.
2. Phase counts before internal labels.
3. Source path -> destination path -> purpose -> status inventory.
4. Explicit no-product-readiness boundary.
5. dual-path same-scenario coverage wording for extension/runtime scenario evidence.
6. 50-pass review ledger prepared as pending, not backfilled with old or invented pass records.

## Freshness boundary

Existing package files were baseline evidence only. The current manifest records copied_overwritten_this_turn for refreshed file records. This file is a package-source inventory, not a product implementation proof.
