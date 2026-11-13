# CODEX/Kiro: PrepareToGPL gold restoration and independent source/code/plan review

**Статус:** research + written protocol. **NO** C# training, .NET tests, Windows CI or product repairs executed as part of this package. This file is ready-to-use work order, not an imaginary completed subagent execution. **Date:** 2026-10-19.

## Work packet — paste into an executor with actual code access

**CURRENT TASK:** verify and restore gold-trained learning from [PrepareToGPL@31346d8](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2) compared with [codex/dpo-implementation@6d36492](https://github.com/lubochka/llm_mvp_core/tree/6d36492714ebc1b9ae5f31a5a1da00673f524d83) through actual named training recipient, old checkpoint, legal versioned cold import, ordinary consumer and source-disjoint domain performance. Specifically test new narrow N55-01/02 hypothesis without confusing it with known 4→6 restore issue.

**NOT THE TASK:** rewriting the full neural training stack, mass-generating 500 DPO pairs, reproducing many unrelated Windows CI workflows, asserting model quality from synthetic oracle, or modifying protected core docs. The exact protected directory is **llm_mvp_core/docs/plans/replanning-possible-diffs**, strictly READ_ONLY.

**SKILLS (read exact files first):** docs/skills/architect-behavior-classifier-SKILL.md; human-readable-architecture-audit-SKILL.md; phase-planning-SKILL.md; plan-review-SKILL.md; code-examination-SKILL.md; audit-protocol-SKILL.md; learning-path-audit-SKILL.md; prompt-patch-authoring-SKILL.md; human-communication-GLOSSARY.md. For code implementation add generated-code-review-SKILL.md. Distinguish domain (subject), task/direction, Unit/Flow (learner), algorithm and metric.

**SOURCE CONTEXT:** [Gold-first](00-55-PREPARETOGPL-GOLD-FIRST-2026-10-19.md), [gap register](55-CROSSBRANCH-GAP-REGISTER-2026-10-19.md), [new seam](55-NOVEL-DIRECTION-EVAL-AND-METRIC-STATE-2026-10-19.md), [domain microprobes](55-CAUSAL-DOMAIN-MICROEXPERIMENTS-2026-10-19.md), existing [old 4→6 study](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md), existing [old-vs-new LoRA](41-LORA-BRANCH-PARITY-AND-REAL-NEURAL-QUALITY-BOUNDARY-2026-10-18.md), protected [58: score/coverage caveats](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/58-unit-metrics-and-generation-consumer-boundaries.md). Search exact donor commits or PR receipts rather than assuming git branch label means success across every task.

**IMPLEMENTATION EVIDENCE MAP REQUIRED:** For each claimed capability fill requirement | concrete code path and line | actual writer/reader | persistent model/Unit key | ordinary caller | input dataset | direct output | served output | external rubric | negative control | A/B result | branch sha. Absence of one cell means NEEDS_EVIDENCE, not PASS.

### Ordered subtasks

1. **Branch archaeology (READ_ONLY):** enumerate actual historic positive runs across PrepareToGPL and sibling branches; copy raw archived metrics with data/label/eval population caveat. Separate old memory, AST kNN, POS kNN, replay, neural DPO, learned dynamic graph and fallback. Do not add percentages from different owners.
2. **Gold checkpoint preflight (READ_ONLY):** inspect models/translation/translation-v1 tracked bytes, original executor Direction list, corpus/manifest and current ValidateCheckpointStructure. Record old binary loaded/not loaded with stable codes; distinguish missing directions from semantic deterioration.
3. **Minimal restore A/B (native, only when execution authorized):** gold+gold checkpoint vs current+raw vs current+legally migrated vs current+empty on identical source-disjoint actual prompts. Verify all existing exact/phrase/word/KNN/tokenizer/verifier maps and training history survive byte/logical checks and cold load. Avoid admission bypass.
4. **Novel evaluator probe (native, only when authorized):** fake unique answer for each RU_EN/EN_RU/HE_EN/EN_HE/ES_EN/EN_ES adapter; build two cases per direction; trace EvaluateAsync → TranslateForEvaluationAsync → adapter. Re-run with typed per-item language/direction and RU_EN-only negative control. State 12-oracle formula as source-equivalent toy, not translation learning. Check whether any production caller uses this API on mixed datasets.
5. **Metric-memory test (native):** same TranslationMachine instance: train, evaluate mixed domains, then next train; inspect AccuracyBefore source and any promotion gate consumer. Negative control: separate clean machine instances; if no actual gate depends on this state, mark product impact NOT_FOUND rather than CAUSE.
6. **Carrier/function transfer (native):** select exact historical matrix-only failure and subsequent kNN success with **same function definition**; isolated AST C# structural feature, POS ENRU and optional SQL (only with real consumer). Keep splits and weights, toggling one factor per run. No universal domain threshold.
7. **Neural causal split (native, after restoration path):** small legal 8–16 exact DPO pairs B0 alone vs warmed base, optimizer finite gradient, mask, q/v LoRA reachable, checkpoint identity. Measure actual generated heldout, do not reuse 0/3 and 0/4 as newly measured.
8. **Review independently:** separate architect/code reviewer checks real caller, version, code path, finite math, source/proxy/native distinctions, held-out interpretation and compliance with file boundaries. A PASS must name exact reviewed commit SHA and fixture checksum; a merged/head-changed file requires reread.

### Good and bad examples

**GOOD:** Migrated model produces same trained ENRU phrase composition in ordinary Flow after cold load, while fresh empty does not, and the effect survives heldout source groups. A 12-case fake route test shows requested vs actual direction, with RUEN-only control unchanged. AST improves kNN but not matrix on the same code families without changing labels.

**BAD:** Marking a native model PASS from toy 12/12 oracle. Increasing k until examples pass; silently changing expected strings. Copying old schema number without validating provenance. Calling a deterministic fallback a trained neural solution. Declaring 19/24 phrase-map gain a LoRA gain or interpreting an E09 unmatched baseline as causal.

**EDGE CASES:** empty/unknown Unit ID, wrong source/target, unicode/mixed scripts, negative pair polarity, multi-clause role swap, duplicated phrase, HNSW stale vector, saved weight never loaded, unchanged old default, unknown SQL dialect, new Spanish direction empty after lawful migration, Eval called before structured training, Eval result written into next batch metric.

### Required return packet

- **SOURCE_SNAPSHOT:** precise branch SHAs, source refs, source-only discoveries, identical blob negative controls, actual path to final consumer.
- **EXPERIMENTS:** exact command, fixture checksum, training data provenance, native runtime logs, per-direction and per-family human-quality counts, coverage/abstention, train/holdout separation, cold-restore output and resource cost.
- **REVIEWS:** the independent reviewer, exact candidate diff identity, PASS/NEEDS_FIX with enumerated remaining omissions.
- **PUBLICATION:** only permitted planning location; link to changed files and fresh-read verification; protected core location unchanged.
- **HUMAN SUMMARY:** in Russian: which old ability was restored, which new quality result genuinely improved and which remains unproven. Do not use vague «всё исправлено».

**Admissibility:** source-only/JS proxy results are allowed as hypotheses; do not use them as proof of native learning. A meaningful completed run requires raw reproducible C# evidence tied to the same frozen cases. If only source analysis executed, finish with truthful NEEDS_NATIVE_VALIDATION.
