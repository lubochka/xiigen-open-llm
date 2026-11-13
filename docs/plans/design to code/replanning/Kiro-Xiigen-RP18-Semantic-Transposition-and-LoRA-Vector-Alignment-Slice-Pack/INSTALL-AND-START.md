# RP18 — install/start a single bounded task

This package is **documentation in P**, not code in K. Planning repository = lubochka/xiigen-planning@llm_plan3; future separately granted product implementation = lubochka/llm_mvp_core@codex/dpo-implementation. Do not install blindly into a live Kiro station or trigger agents merely by reading a Markdown file.

1. Pin P and K branch commit SHA, record exact task ID (e.g. `RP18.00C`), single write owner, CURRENT TASK / NOT TASK, permissions and budget. Record sources' HEAD/blob:lines and source depth.
2. Read this README, SOURCE-AND-OWNERSHIP, ARCHITECTURE-AND-CONTRACTS, DOMAIN-LAWS-AND-CURRICULUM, EXPERIMENT-AND-PC, EXECUTION-BOARD, GOVERNANCE-AND-REVIEW and the **entire selected task**. Bring *relevant contents* of original P/G/F/V, WorkLeft RAG→primary, current K code and named skill texts; a bare link is not skill/context loaded.
3. For an agent cold start use [START-PROMPT](START-PROMPT.md) and paste the exact selected task content, its owner-specific skill excerpts, code/source excerpts, frozen protocol and any applicable approval. Do not give only “do RP18.”
4. Read full target file, actual caller, output consumer, representation manifests, index/pointer readers, and focused existing checks before deciding on code. Existing correct consumer = `REUSE_NO_CHANGE`, with actual evidence.
5. Only after concrete authorization implement the minimum missing seam in the proven owner's repo. Refuse unauthorized model calls, token/weights mutation, private corpus export, Windows CI changes and cold pointer writes. GitHub document write rights are **not** implied GPU/model/training/data grants.
6. Before a measurement, freeze source-family-split exam, domain-law rubric, metric thresholds, matched arms, stop budget, actual PC profile and identifiers. Then run one permitted narrow exam; if unavailable return `NOT_MEASURED`, not green.
7. Save a task receipt as defined in HANDOFF-AND-RECEIPTS; independently review the exact candidate SHA via an **actual separate** CODEX GPT-6 Astra UltraCode invocation where authorized, never by writer self-label.

Allowed statuses: `SOURCE_REQUIRED`, `AUTHORITY_REQUIRED`, `REUSE_NO_CHANGE`, `DESIGN_BINDING_REQUIRED`, `IMPLEMENTED_NOT_MEASURED`, `IMPROVE`, `REGRESS`, `INCONCLUSIVE`, `INVALID_EVIDENCE`, `NO_TRAINING_NEEDED`. All are more honest than fake DONE.

**Concurrency:** 00A/00B/00C read in parallel from pinned sources; 01A/01B/01C independently plan and reconcile source schema; one mutable K owner / one adapter Student state writer at a time. R01–R25 independent starts per PARALLEL25 do not make Rxx DONE. RP18 must not mutate their five result carriers.
