# 07 — Порядок исследования различий без ещё одной дорогой программы CI
**Режим:** переиспользование данных. Не изменять frozen RP01 scientific grades, labels, checkpoint или опубликованные source receipts.

| Приоритет/гипотеза | Действие | Отрицательный результат означает | Достаточный результат |
|---|---|---|---|
| P0 Cold B0 | 8–32 chosen-only SFT train-only overfit; compare DPO-only, warm→DPO | Дальнейшие 500 пар пока бессмысленны: выяснять train mechanism/capacity | measurable train logits/token correct; holdout separate |
| P0 Gradient/loss | tiny finite-diff grad against actual loss; prompt-only masking | STOP repair before next batch | signed finite-diff agreement and reference immutable |
| P0 Data/teacher | audit law counterfactuals in 25 C#/20 ENRU families, 500 row structure | quarantine bad future lessons, preserve old | semantic law per pair, true authorship, independent CODEX judge |
| P0 Representation | exact raw→canonical→semantic IR→vector→consumer route for 3 domains | find missing transform/version/collision | preserved law distinctions, explicit loss if compress |
| P0 kNN | old/top1 vs domain-law+adaptive rerank, same index | no claim of adaptive gain | paired law quality plus PC cost |
| P1 Unit/Flow | trace train-state-consumer; simple many→many comparison | static pipeline not claimed learned | changed mapped path with fresh input |
| P1 Restore | heldout prompt pre-save/post-save/fresh ordinary path | isolate stale pointer, tokenizer/cache | same exact loaded behavior |
| P1 Graph/Orchestrator | same units + static vs adaptive async graph paths | graph still design-only | actual unseen composition gain |
| P1 PC | measure stage wall time and memory of process tree | not say faster | equal-domain quality and reduced full cost |

## Detailed executor prompt: task template (applies to each row)
You are **CODEX Astra 6 UltraCode** acting as investigator and independent scientific planner. Read exact original RAG/WorkLeft docs and cross-references before asserting historical algorithm parity. Read actual current code; bind path:line/blob SHA to every CODE finding; distinguish observed outcome, contract difference, hypothesis and still-unread surface. Apply `architect-behavior-classifier`, `phase-planning`, `plan-review`, `code-examination`, `root-cause-tracing`, `data-flow-integrity`, `prompt-patch-authoring`, `human-readable-architecture-audit` from core `docs/skills`. Do not accept prior findings without primary-source verification. Produce only one concise evidence packet per cause and a change proposal, not a rewrite of all RP01/RP02/RP03. No code mutation in this auditing pass.

**Context:** core `codex/dpo-implementation`, planning `llm_plan3`, [original task](../../original%20task.md), [RAG usage](../../09.09.26WorkLeft-RAG-00-usage-guide.md), [previous round thinking](../previous%20rounds%20thinking%20results.md), [recovered contract](../RECOVERED-DESIGN-CONTRACT.md), RP01 live artifacts, RP02 interface and RP03/RP04. Re-pin branches and enumerate all WorkLeft* and inbound references; inspect actual files not just S04. Budget initially read-only; reuse C#500 and ENRU500 without new 500-row generation.

**Procedure:** (1) trace current consumer; (2) quote minimal exact source proving CODE claim; (3) find original design law and historical experiment; (4) state whether same inputs/metrics; (5) define *one* falsifying experiment on heldout families, upfront before observing outputs; (6) estimate calls/cost with early-stop; (7) name owner and handoff slice; (8) independent Astra review of evidence only (review has not happened yet).

**Good:** `KnnLogicalUnit` uses best=hits[0] yet RP01 GenerateCore may never call it → confirmed implementation gap, causal link still UNKNOWN. **Bad:** declare kNN broke DPO without proving invocation. **Good:** warmed Student improves chosen token log-prob in 16 train examples, followed by validated unseen law gains. **Bad:** weights changed, therefore Student learned the law.

**Edge cases:** duplicated semantic families, answer token-length bias, shared prefix, Unicode/RU/HE normalization, referent ambiguity, C# version and nullable, SQL NULL three-valued logic, negative preferences based only on tone, wrong provenance, optimizer-RNG step misbinding, KV cache, nondeterministic sampling, kNN score treated as calibrated probability, fixed-size hash collision, multiple admissible outputs, absence of graph consumer.

**Output schema:** `DIF-ID | status | original source | current exact source | change in behavior | impact scope | experiment | falsifier | test cost | repair owner | scientific result`. No new test coverage quota; only a minimal deterministic invariant check if actual code edited. Independent domain examination and wall/resource comparisons are primary.

## Parallel ownership
RP01: Student prerequisites/DPO/LoRA/restore. RP02: domain semantic tokens, kNN, actual law consumers. RP03: Unit many→many and Flow learnable correction. RP04: async Orchestrator, graph higher-order learning. Run source investigations concurrently; do not gate RP02 on DPO improvement. Do not interpret this document as authorization for expensive execution.