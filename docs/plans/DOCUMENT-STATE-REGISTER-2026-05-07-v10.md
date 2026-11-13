# Document State Register — Dynamic Flow Project
# Version: 10.0 — 2026-05-07
# Replaces v9. Updated: DPC integration implemented (A55), back-translation implemented (A56),
# model depth 1L→4L locked (A58), inference path fixed (A59), prior manifests obsolete (A60),
# floor 1251→1265, two new plans written (RAM release, singleton cache), SESSION-STATE v18,
# IMPLEMENTATION-DOCUMENTATION v8, DPO-DEEP-DIVE v2 produced.

---

## How to use this register

Read at the start of any session touching the dynamic flow document set. The
register tells you which document is current, which is superseded, and what each
open thread requires next.

---

## Resolved since v5

Full experiment gap audit completed. All 27 positive experiments confirmed disconnected
from project 21's execution path. Root cause: project 21 references only projects
00, 01, 03, 05, 06, 10, 11. Projects 12, 15, 16, 22, 29 are invisible to runtime.

Three-path architecture documented: Path A (prompt processing), Path B (training),
Path C (result translation). All three have proven components waiting to be wired.

Source duplication confirmed: DomainLabel enum exists three times with different value
names; RouteClassifier exists in three projects with diverged implementations; 13
pipeline classes duplicated between projects 12 and 29.

E21 (Decomposer) reclassified from IMPLEMENTED to PARTIAL — class exists in project 12
but is never called from project 21. E24 locked as DO NOT IMPLEMENT until E21 is
verified in production.

Plan v4 produced: 8 phases (plus Phase 0 consolidation), 33 new tests, raises floor
to 1257. Requires two new projects: 17.DynamicDomains, 23.SemanticCompilerAdapter.

Decisions A43–A54 added. Four plans implemented (2026-05-07): PLAN-DOMAIN-CLASSIFIER,
PLAN-E10, PLAN-CONTEXT-ENRICHED-ROUTING, PLAN-PHASE-TRANSITION-SIGNAL. Floor 1224 → 1251.
Project 23 SemanticCompilerAdapter created. E13B experiment run (corpus retrieval proven).
Replay buffer unbounded growth identified (A54). New plan PLAN-E13B produced.
Seven teaching documents updated: examination method v8, external sources pipeline v7,
XIIGen export addendum v3, self-teaching guidance v2, orchestrator teaching plan v3,
teaching measurement guidance v4, executor-prompt-register v2.

---

## Current open threads

### Thread 1 — RAM release and singleton caching (HIGHEST PRIORITY before next run)

PLAN-RUNNER-RAM-RELEASE-v1 written — reconstruct registries and trainer per physical
batch, add ClearCache() to trainer. Not yet implemented. Floor: 1265 → ~1267.

PLAN-SINGLETON-CACHE-MANAGEMENT-v1 written — static shared node units, Project15Bridge
reflection cache, TemplateBeautifier static instance, inline replay shuffle, flow
cache in seeder. Not yet implemented. Floor: ~1267 → ~1270.

Both must be implemented before the next calibration run to keep RAM pressure low.

### Thread 2 — Next calibration run (BLOCKED on Thread 1)

Prior batch-20 run (317 manifests, run-2026-05-07-133809) is obsolete — wrong model
shape, wrong vocabulary, wrong UserRequest. Start fresh in a new checkpoint directory.
Next run: batch-100 on a clean machine. Expected 4–6 hours due to 4-layer model.

### Thread 3 — E13B held-out validation (HIGH PRIORITY)

PLAN-E13B-EXPRESSION-COMPLETION-UNIT-v1 ready to execute. Phase 1 is a conditional
stop at held-out accuracy < 60%.

### Thread 4 — Infrastructure copy deletion (MEDIUM)

Step 2D of PLAN-DOMAIN-CLASSIFIER-SINGLE-SOURCE-v1 still incomplete. Eight
production-class copies in `35.ExperimentsV2/Infrastructure/` still present.

### Thread 5 — Experiment wiring (after threads 1–3)

Execute PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md phases 0–8.

### Thread 6 — Orchestration scenarios (BLOCKED pending teaching + wiring)

O1 through O5 blocked. Teaching is the gate, followed by experiment wiring.

---

## Document status table

| Document | Status | Notes |
|---|---|---|
| DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v3-5.md | Current authority | Unchanged from v9. |
| WHAT-TO-DO-WHAT-NOT-TO-DO-v13.md | Current | Unchanged from v9. |
| SESSION-STATE-2026-05-07-v18.md | **Current** | Supersedes v17/v16. A55–A60 locked. Floor 1265. |
| SESSION-STATE-2026-05-06-v16.md | Superseded | Use v18. |
| SESSION-START-PROMPT-v10.md | Current | Unchanged from v9. |
| IMPLEMENTATION-DOCUMENTATION-v8.md | **Current** | Supersedes v7/v6. DPC wiring, CapabilityResultTranslator, 4L model, Path C closed. |
| IMPLEMENTATION-DOCUMENTATION-v7.md | Superseded | Use v8. |
| IMPLEMENTATION-DOCUMENTATION-v6.md | Superseded | Use v8. |
| DPO-DEEP-DIVE-v2.md | **Current — new** | Full DPO pipeline with DPC integration, 4-layer model, run metrics diagnosis. |
| PROJECT-HISTORY-v7.md | Current | Unchanged from v9. |
| DOCUMENT-STATE-REGISTER-2026-05-07-v10.md | Current (this file) | Supersedes v9. |
| DOCUMENT-STATE-REGISTER-2026-05-06-v9.md | Superseded | Use v10. |
| EXAMINATION-METHOD-ORCHESTRATION-v8.md | Current | Unchanged from v9. |
| PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md | Current | Unchanged. Execution deferred until threads 1–3 complete. |
| PLAN-DOMAIN-CLASSIFIER-SINGLE-SOURCE-v1.md | Current (partially implemented) | Step 2D (Infrastructure deletion) still incomplete. |
| PLAN-CONTEXT-ENRICHED-ROUTING-v1.md | Current (implemented) | Unchanged from v9. |
| PLAN-PHASE-TRANSITION-SIGNAL-v1.md | Current (implemented) | Unchanged from v9. |
| PLAN-E10-REGEX-PREPROCESSOR-v1.md | Current (implemented) | Unchanged from v9. |
| PLAN-E13B-EXPRESSION-COMPLETION-UNIT-v1.md | Current (not yet executed) | 5 phases, 10 tests. Phase 1 conditional stop. |
| PLAN-DPC-INTEGRATION-v1.md | **Current (implemented)** | DevelopmentPromptCompiler into teaching pipeline. A55 fix. 6 tests. |
| PLAN-BACK-TRANSLATION-DEEPER-MODEL-v1.md | **Current (implemented)** | CapabilityResultTranslator + 4-layer model. A56/A58 fix. 8 tests. |
| PLAN-RUNNER-RAM-RELEASE-v1.md | **Current (written, not implemented)** | Registry+trainer reconstruction per batch. Floor: ~+2 tests. |
| PLAN-SINGLETON-CACHE-MANAGEMENT-v1.md | **Current (written, not implemented)** | Static nodes, reflection cache, flow cache, 12 isolation tests. Floor: ~+5 tests. |
| EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v7.md | Current | Unchanged from v9. |
| XIIGEN-EXPORT-TEACHING-ADDENDUM-v3.md | Current | Unchanged from v9. |
| XIIGEN-DRIVEN-SELF-TEACHING-GUIDANCE-v2.md | Current | Unchanged from v9. |
| ORCHESTRATOR-TEACHING-PLAN-v3.md | Current | Unchanged from v9. |
| TEACHING-MEASUREMENT-GUIDANCE-v5.md | Current | Unchanged from v9. |
| EXECUTOR-PROMPT-REGISTER-ALL-31-SUB-PHASES-v2.md | Current | Unchanged from v9. |
| DECISIONS-LOCKED.md | Current — D-01 through D-15, A1–A60 | A55–A60 added this session. |
| SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v7.md | Current | Unchanged from v9. |

---

## How testing works

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Baseline: **1265 passing, 0 failing.** Floor for all sessions.

---

## Key numbers

| Item | Value |
|---|---|
| Current test count | **1265/1265** |
| Baseline for all sessions | **1265** |
| Latest commit | post back-translation — 8/8 new tests passing |
| Model config (locked A58) | 4 layers, hidden 32 — 69,920 params, 273 KB/checkpoint |
| LoRA-trainable parameters | 2,048 |
| Batch-20 calibration run | 200 rows, 23 sub-phases, 3,559s — obsolete (A60) |
| XIIGen export | 1,080 usable rows, not yet taught |
| CB4 pairs (wave-1, sub-phase 3.4) | 2,436 instances, node-08 complete |
| Domain adapters active | all 5 |
| Branch cap | 8 |
| DPO cost share | 97.7% of elapsed (from batch-20 run) |
| Decisions locked | D-01 through D-15, A1 through A60 |

---

## File locations

### Repository: `C:\Projects\llm_mvp_core` — branch `main`. Floor: 1265.

```
21.AgentInfrastructure/Logical/
  AdaptiveLogicalOrchestrator.cs
  DefaultLogicalCapabilityTrainer.cs      ← DPC integrated, compiled training text
  CapabilityResultTranslator.cs           ← NEW — back-translation
  GeneratedCapabilityFlow.cs              ← compiles prompt before KNN; uses translator
  PromptDomainClassifier.cs
  MultiSubPhaseRouterUnit.cs
  DevelopmentLifecycleTopologySeeder.cs
  UniversalNineNodeDevelopmentFlow.cs
  LogicalCapabilityTrainingContracts.cs   ← CompiledPrompt field added
  GeneratedCapabilityCheckpointManifest   ← OriginalPrompt field added

experiments-v2/XIIGEN-DOCS-BATCH-COMPARISON/
  run-2026-05-07-133809/                  ← OBSOLETE (A60) — wrong model + vocabulary
```

### Teaching ledger

```
C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\
  claude-export\batch-0000\legacy-docs-skills-from-old-code\
    teaching-ledger-active.jsonl
    teaching-registry-snapshot-active.json
    checkpoints-cumulative\active\        ← START FRESH for next run (A60)
```

### External sources

```
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\
  wave-1\CB4-stage-3-pairs\              ← 2,436 instances, sub-phase 3.4
  wave-2\                                ← 291 usable rows, 16 sub-phases

C:\Users\lubao\Documents\xiigen\         ← XIIGen export (1,080 usable rows)
```

### Plans

```
C:\Users\lubao\Documents\xiigen\llm mvp\
  DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v3-5.md   ← READ FIRST EVERY SESSION
  WHAT-TO-DO-WHAT-NOT-TO-DO-v13.md
  SESSION-STATE-2026-05-07-v18.md
  IMPLEMENTATION-DOCUMENTATION-v8.md
  DPO-DEEP-DIVE-v2.md
  PLAN-RUNNER-RAM-RELEASE-v1.md                   ← implement before next run
  PLAN-SINGLETON-CACHE-MANAGEMENT-v1.md           ← implement before next run
  PLAN-DPC-INTEGRATION-v1.md                      ← implemented
  PLAN-BACK-TRANSLATION-DEEPER-MODEL-v1.md        ← implemented
  PLAN-E13B-EXPRESSION-COMPLETION-UNIT-v1.md      ← ready to execute
```

---

# Last updated: 2026-05-07
# Test baseline: 1265/1265
# New decisions: A55, A56, A57 corrected → A58, A59, A60
# New documents this session: SESSION-STATE v18, IMPLEMENTATION-DOCUMENTATION v8,
#   DPO-DEEP-DIVE v2, PLAN-DPC-INTEGRATION-v1, PLAN-BACK-TRANSLATION-DEEPER-MODEL-v1,
#   PLAN-RUNNER-RAM-RELEASE-v1, PLAN-SINGLETON-CACHE-MANAGEMENT-v1
# Key events: DevelopmentPromptCompiler wired into teaching pipeline (A55 — three stacked
#   failures fixed); CapabilityResultTranslator added — back-translation of KNN output (A56);
#   model depth 1L→4L locked (A58); inference path now compiles prompt before KNN (A59);
#   prior batch-20 manifests declared obsolete (A60); batch-20 calibration run completed
#   (200 rows, 23 sub-phases, 0.99h) but results are diagnostic only — wrong pipeline
