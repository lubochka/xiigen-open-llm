# R11 RUN LOG

## Assignment

Execute **R11 — choose process changes by causal dependencies** from `LEARNING-WORK-ORDERS-11-15.md`.

The run is analysis-only. It does not implement product code, launch training, dispatch Windows/GitHub Actions, change tests, datasets or weights, or perform R15 Astra independent review.

## Start snapshot

- Planning branch: `llm_plan3`.
- Observed branch head before R11 publication: `f012545e7feed17a4a69836fd4a5ca1ef9525a93`.
- Observed results directories: R01 through R09.
- `results/R10`: **RESULT_NOT_OBSERVED** at start.
- R11 contract permits bounded process analysis before joined algorithm evidence is complete.

## Skills actually consulted

From `lubochka/llm_mvp_core@codex/dpo-implementation/docs/skills`, applicable sections were read from:

- `planning-session-startup-SKILL.md` — current instruction/authority, complete work-order and delivery-first boundaries.
- `phase-planning-SKILL.md` — bounded outcome and dependency-oriented planning.
- `plan-review-SKILL.md` — requirement preservation and plan quality.
- `trainable-architecture-decision-SKILL.md` — deterministic helper versus learned authority/state.
- `code-examination-SKILL.md` — producer/caller/consumer reasoning and hypothesis-before-tests principle.
- `generated-code-review-SKILL.md` — code/evidence review boundaries.
- `prompt-patch-authoring-SKILL.md` — concrete wrong/correct behavior in downstream work orders.
- `human-readable-architecture-audit-SKILL.md` — owner-facing architecture explanation.
- `experiment-design-SKILL.md` — controlled baseline/challenger/falsifier thinking.
- `improvement-measurement-SKILL.md` — behavior/time/resource measurement rather than test counts.
- `output-readability-gate-SKILL.md` — human-readable result before identifiers.

This log does not claim the whole skills library was read.

## Evidence consumed

Directly read current R11 contract, previous-round history, R09 findings/decisions, and bounded portions of R02/R04–R08 findings/decisions. Worker results are treated as attributable handoffs; R11 does not claim to have repeated every primary source/code trace performed by those workers.

Key inherited evidence:

1. R05 traced an existing DPO path and identified loss of exact pair meaning through preparation plus an objective seam.
2. R04 found multiple distinct representations and a reusable typed kNN path; first DPO lesson does not have to wait for a representation rewrite.
3. R06/R08 require domain-specific dynamic retrieval to have actual learned signal/state/owner and distinguish domain mapping, vectorization, kNN and LoRA.
4. R07 preserves Unit/Flow/Orchestrator/DynamicGraph as distinct capabilities and keeps ordinary adoption/credit routing explicit.
5. R09 bounded Actions evidence supports moving unrelated full Windows qualification out of the first-learning critical path while retaining native verification at the proper milestone.
6. Current DPO authority is locked: CODEX with Astra 6 UltraCode chooses the lesson and judges the exact final pair; the external open Chinese model writes both final chosen/rejected; Student learns; independent Student evaluation is separate.

## Causal analysis performed

The run compared candidate explanations for slow progress:

- missing trainer framework;
- lossy/incorrect material-to-objective seam;
- no ordinary consumer adoption;
- domain/representation/kNN uncertainty;
- Windows/native verification cost;
- evaluation proxy mismatch;
- repeated discovery/planning duplication.

The highest-priority explanation supported by current evidence is **not** “trainer is missing.” It is that an existing training path cannot yet be interpreted as a valid learning experiment until exact pair fidelity, objective/recipient and ordinary adoption are closed.

The user’s hypothesis that tests are being fitted remains globally **INCONCLUSIVE**; no unsupported accusation was promoted to fact. The process can nevertheless be changed because green coverage/native runs are independently insufficient learning evidence.

## Published R11 artifacts

1. `SOURCES.csv` — exact source receipts and R10 absence.
2. `FINDINGS.md` — causal DAG, minimal cut-set, role boundaries and evidence limits.
3. `DECISIONS.md` — ranked process decisions, falsifiers, owners and handoff.
4. `CHANGE-MAP.csv` — downstream owners R12/R13/R14/R17/R18/R22/R23.
5. This `RUN-LOG.md`.

## Publication commits

- `57d11316db85bee057745f90d9e9a9f6236ab088` — SOURCES.
- `62d6372b14e6142c4e30ce080408fa63e5d2239b` — FINDINGS.
- `632426d571a5fdcc7e4f3a5869a8c0ac51e87766` — DECISIONS.
- `b646b7e1a5e6ecd8c400c285e7bb062f9a1495c9` — CHANGE-MAP.
- RUN-LOG commit recorded after this write by remote readback.

## Result boundary

**Measured learning gain:** NOT_MEASURED.  
**PC-fit:** NOT_MEASURED.  
**R15 independent Astra review:** NOT_PERFORMED.  
**New CODEX/Astra lesson-selection or pair-judging model call:** NOT_PERFORMED in R11.  
**Chinese-model DPO generation:** NOT_PERFORMED in R11.  
**Training:** NOT_RUN.  
**Tests/coverage:** NOT_RUN.  
**Windows/Actions:** NOT_RUN.  
**Product code:** NOT_CHANGED.

R11 is a causal planning decision packet. It moves the next work toward the first valid learning loop instead of adding another verification campaign.
