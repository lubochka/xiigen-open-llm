# Instruction-Tuning Source Assessment for Orchestration Sets
# Version: 1.0 - 2026-05-11

Purpose: decide whether broad instruction/chat datasets can support XIIGen
orchestration training without diluting code-backed ARCH/PLAN/SKILL accuracy.

This document is not a training set. It is a source-gating guide for producing
orchestration-focused DPO rows from instruction-tuning corpora.

2026-05-11 routing-flow correction: instruction-tuning corpora may provide
A-side prompt surfaces for compiler/router station pairs, but accepted routing
training must target the full flow/orchestrator behavior:

```csharp
OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>
```

or equivalent `AdaptiveLogicalOrchestrator`. If an instruction-tuning batch
contains only `PromptRoutingInputCompilerUnit` or `MultiSubPhaseRouterUnit`
pairs, its routing status is `PAIRSET_READY_ROUTING_FLOW_PENDING`.

---

## Core Rule

Instruction-tuning datasets are prompt-surface sources, not XIIGen authority.

They may supply:

- A-side user intent phrasing
- multi-turn clarification patterns
- ambiguous or underspecified request shapes
- negative examples where the assistant jumps to implementation too early
- paraphrases for routing, decomposition, and orchestration gates

They must not supply:

- B-side architecture decisions
- source-of-truth sub-phase names
- skill activation rules
- registry numbering
- implementation contracts
- code-level authority for the current project

The chosen side for orchestration pairs must still come from XIIGen internal
contracts, source registries, current code validation, or approved external
technical standards. The broad chat dataset only contributes the shape of the
request.

---

## Source Decisions

| Source | Current license state | Decision | Use in XIIGen |
|---|---:|---|---|
| OpenAssistant OASST1 | Apache-2.0 | ACCEPT with filtering | High-value multi-turn prompt/clarification source. |
| OpenAssistant OASST2 | Apache-2.0 | ACCEPT with filtering | Same role as OASST1; keep separate source ids. |
| Databricks Dolly 15k | CC-BY-SA-3.0 | QUARANTINE | Do not mix into clean MIT/Apache production batches unless ShareAlike handling is explicitly accepted. |
| FLAN Collection | Repo Apache-2.0; Google Research datasets generally CC-BY-4.0; underlying tasks require per-task review | METHOD-ONLY until audited | Use task/form templates and transformation method; do not ingest raw examples until per-task license is recorded. |
| Stanford Alpaca | CC-BY-NC-4.0 / research-only | BLOCK | Do not use in production training sets. Can be cited only as excluded source. |
| ShareGPT variants | fragmented / often unclear / model-output provenance risk | BLOCK by default | No direct ingestion without exact dataset card, license, and provenance review. |
| WizardLM / Evol-Instruct | often derived from ShareGPT or model outputs; dataset notes mention data-usage constraints | BLOCK by default | Use paper idea as method inspiration only, not direct rows. |
| UltraChat | synthetic; common cards lack complete license/provenance clarity | REVIEW REQUIRED | No direct ingestion until the exact release has a license file and provenance accepted. |

---

## Allowed Pair Families

### IT-ORCH-00: Root Intent to Orchestration Routing Flow

A-side: accepted user prompt surface from an instruction/chat source.

Chosen: complete XIIGen routing-flow result: compiled document, routing
decisions, knowledge-edge state, flow/factory resolution, warnings, and final
routing verdict.

Rejected: compiler-only or router-only answer presented as complete routing.

This is the only instruction-tuning routing family that can support routing
promotion. IT-ORCH-01 and IT-ORCH-02 are station-level ingredients.

### IT-ORCH-01: Root Intent to Routing Compiler

A-side: user prompt from accepted instruction/chat source, rewritten only to
remove irrelevant domain details and preserve the request shape.

Chosen: XIIGen `PromptRoutingInputCompilerUnit` output, including code-backed
predicate ids and confidence rules.

Rejected: wrong predicate assignment, missing LEARNING/AUTOMATION predicate, or
false no-predicate knowledge edge.

Do not copy the source assistant answer as chosen.

### IT-ORCH-02: Ambiguous Request to Multi-Sub-Phase Routing

A-side: accepted user request that contains multiple task intents, e.g. plan +
test + documentation, or implementation + learning feedback.

Chosen: all matching sub-phases are routed; no valid sub-phase is suppressed.

Rejected: single-route answer that hides one of the required branches.

### IT-ORCH-03: Clarification Before Execution

A-side: multi-turn request where constraints are missing or conflict.

Chosen: compile missing constraints, record knowledge-edge metrics, and ask for
the minimum clarifying information before execution.

Rejected: direct implementation without constraint capture.

### IT-ORCH-04: Complex Task Decomposition

A-side: broad instruction like "build a system", "prepare a plan", "fix this
workflow", or "make an agent".

Chosen: decomposition into goal, sub-phases, plan gates, skills, evidence, and
testing path using XIIGen orchestration rules.

Rejected: flat checklist, one-step code answer, or generic chat response.

### IT-ORCH-05: Skill Activation and Load Order

A-side: request shape that implies planning, architecture, validation, or
handoff.

Chosen: correct skill load order and "must-load-first" constraints.

Rejected: loading a late implementation skill before governance or planning
skills.

---

## Extraction Rules

1. Use only the user side, prompt tree, or dialog structure as source material.
2. Strip assistant answers unless they are only used to create a rejected
   "generic chat answer" contrast.
3. Require `external_role: "a_side_style_only"` on every row from this family.
4. Require `internal_contract_reference` for every chosen side.
5. Require `source_license_state` and exact source id.
6. Keep one dataset per batch until standalone ablation passes.
7. Do not mix CC-BY-SA or unclear-license rows with Apache/MIT/CC-BY clean rows.
8. Hold rows from blocked or review-required sources in `source_review_required`.

---

## Negative DPO Targets

Good rejected examples for this source family:

- generic assistant answer instead of orchestration decomposition
- direct coding answer before routing/gates
- missing one branch in a multi-intent request
- treating chat/source answer as authority
- omitting `[WRONG:]` consequence
- routing a LEARNING prompt without the LEARNING predicate
- routing Phase 7 automation text without AUTOMATION
- assigning nonexistent sub-phase `3.6`

Bad rejected examples:

- chosen text with one negated sentence
- same answer with different wording
- moralizing refusal unrelated to orchestration
- source assistant answer copied as both chosen and rejected
- external dataset answer used as XIIGen architecture authority

---

## Batch Gates

Every instruction-tuning augmentation batch must pass:

- source decision is ACCEPT or explicitly quarantined
- no blocked source rows in train-ready
- all chosen sides cite internal contract/code-backed guide
- all rejected sides contain specific `[WRONG: ...]`
- no mojibake in A/chosen/rejected
- no `chosen == rejected`
- standalone ablation before mixing with V246 baseline
- combo gate against V246 before production merge

---

## Recommended First Run

Start with OASST1/OASST2 only.

Target: 64 rows.

Distribution:

- 20 IT-ORCH-01 routing compiler pairs
- 16 IT-ORCH-02 multi-sub-phase routing pairs
- 12 IT-ORCH-03 clarification/knowledge-edge pairs
- 10 IT-ORCH-04 decomposition pairs
- 6 IT-ORCH-05 skill activation/load-order pairs

Do not use Dolly, Alpaca, ShareGPT, WizardLM, or UltraChat in the first run.
FLAN can be used only as method inspiration until a per-task license ledger is
prepared.
