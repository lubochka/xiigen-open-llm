# N33 MICRO — быстро отделить наученную способность от HNSW/Unicode-эффекта

**16.10.2026 | READY-TO-RUN WORK ORDER / NATIVE .NET NOT RUN / INDEPENDENT CODEX-ASTRA REVIEW NOT RUN**

Читать [master](33-MASTER-RP01-QUALITY-DIVERGENCE-AND-NOVELTY-2026-10-16.md), [HNSW RNG](33-NEW-HNSW-RNG-CONTINUATION-AND-COUPLING-2026-10-16.md), [UTF8](33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md). Требование: самый короткий фальсифицируемый эксперимент, НЕ новая многоэтапная тестовая бюрократия. Никогда не модифицировать core/docs/plans/replanning-possible-diffs.

## Исполнитель и обязательные навыки

CODEX source investigator → CODEX experiment executor → **независимый CODEX/Astra 6 UltraCode reviewer**, если действительно назначен. При старте прочитать реальные текущие docs/skills: human-readable-architecture-audit-SKILL.md, conversation-intent-and-self-reflection-SKILL.md (human communication), architect-behavior-classifier-SKILL.md, trainable-architecture-decision-SKILL.md, reference-inventory-and-source-map-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, generated-code-review-SKILL.md, prompt-patch-authoring-SKILL.md, claim-verification-SKILL.md, root-cause-tracing-SKILL.md, investigation-handoff-SKILL.md. Проверить pinned SHA на дату запуска; отчёт bound к реально запущенному code/commit, не к этому описанию.

**Не меняемые роли DPO:** CODEX/Astra 6 UltraCode выполняет human case selection. Внешняя открытая китайская модель создаёт ОБЕ финальные стороны chosen/rejected. Другой CODEX/Astra 6 UltraCode независимо проверяет pair-law quality и blind free outputs. Отсутствие доступа к настоящей внешней модели = НЕ выполнять DPO authoring либо обозначить synthetic PROXY, не подменять происхождение.

## Experiment A: N33-HNSW-RNG — один .NET микроран

**H0:** разные random-level histories не меняют достигнутую learned-answer correctness. **H1:** при идентичных preserved lessons, encoder, labels и query дополнительный unrelated HNSW insertion perturbation (через static _rng) меняет ANN graph/hit и хотя бы один final law answer.

Минимальные данные: 128 deterministic, finite, same-dimensional vectors; первые 64 — frozen base; next64 — одинаковый continuation; 16 source-disjoint query vectors, с независимой заранее принятой correct-label oracle. Сохранить original indexed IDs, title of domain, expected IDs/law and topK. Для выявления условного механизма m=2 или 4, efConstruction=8, efSearch=2; после обнаружения — повторить с production m=16/ef defaults.

A: fresh process loads base HNSW; insert continuation; Search16; save. B: fresh process loads **same exact base bytes**; before continuation performs 64 unrelated Add in *another* HNSW object; insert same continuation; Search16; save. C: exact BruteForceIndex control with all same-dimension stored vectors; if exact oracle itself changes, STOP experiment. Source expected: HNSW static RNG is shared; save/load omit RNG; changing unrelated Add consumes random draws. HNSW A/B serialized graph MAY differ; only if hit/final answer change do we infer local quality impact.

**Pass (mechanism):** same base/content/encoder and different new graph. **Pass (quality):** A/B exact same heldout case yields different final learned correctness under verified actual consumer. **Fail/inconclusive:** graphs differ but final correctness identical; or no actual HNSW consumer in RP01. Evidence: base SHA, generated insert list SHA, graph edges/entry points, raw correct/incorrect hits, latency, original/code branch commit, negative control.

## Experiment B: N33-UTF8 — один byte-unit probe плюс inference opt-in

Cheap native .NET source-level probe (in Tests scratch or standalone referenced project, not product code):

~~~csharp
var bpe = new BpeTokenizer();
bpe.Train("п", 256); // base vocab 0..255, no merge
var first = bpe.Decode(new List<int> { 0xD0 });
var full = bpe.Decode(new List<int> { 0xD0, 0xBF });
// Source prediction: first == "�"; full == "п".
// Then verify ordinary generator can terminate on such incomplete boundary.
~~~

An [executed standalone Python shadow](33-MICRO-UTF8-PROXY-RESULT-2026-10-16.json) already demonstrates the byte replacement behaviour, but NOT C# runtime. For frozen RP01 ENRU Student: 8–16 source-disjoint prompts, capture raw generated IDs and vocab bytes (must inspect actual Source/Model/Generated receipts; existing detailed output only includes decoded text unless instrumented). Compare max-length stop A vs boundary-only completion B, same checkpoint/query/seed; count UTF8-valid and independent law correctness. ASCII-only C# is negative control. Do not change DPO pair, teacher, LoRA, or existing gold labels to make a result positive.

**Gate:** if UTF-8 strict decode always valid and no stop on partial bytes, mark N33-UTF8 = NOT_APPLICABLE_TO_RP01, do not ship a repair.

## Experiment C: the one essential hybrid-vs-Student control

Take 4–8 lawful **new** lessons (chosen/rejected approved by real external open Chinese author; CODEX/Astra selects, independent CODEX/Astra judges), 8–12 different heldout source-family cases from C# or ENRU. Freeze law rubric and split before learning. Compare four recipients:

- S0: frozen current Student/LoRA before DPO, no KNN;
- S1: same Student with 8–32 exact DPO updates, no KNN;
- K1: same frozen Student + trained Unit/KNN memory, without domain-specific features;
- K2: same frozen Student + learned Unit/KNN memory plus ONE selected POS or AST/sequence feature appropriate to the domain.

Run **ordinary consumer** in all four, not internal IsTrained flags. Hold all teacher data and query IDs stable. Log per-case before/after exact generated text, actual selected learned ID, chosen law, critical wrong count, abstain, unknown-law generalization, cold restore. This separates *effective discrete learned retrieval* from *neural learning quality*. Different recipients must never be conflated or promoted by combined aggregate.

Priority: first A or B **only if real caller evidence exists**; first C for the fundamental learning problem otherwise. If case source/provenance from a real Chinese model unavailable, C=BLOCKED_NO_REAL_PAIR; execute **read-only** A/B without inventing DPO authorship.

## Return packet / review gate

Write no more than one compact result card per A/B/C: pinned candidate SHA, data SHA, exact source path/line, hypothesis, one changed factor, elapsed time, input, metric denominator, old→new outputs, false positives, stops, negative controls, causal verdict, unsupported generalization, next action, files changed, actual worker/model invocation receipts or NONE. Claimed outcomes require evidence: **SOURCE_CONFIRMED**, **PROXY_EXECUTED**, **NATIVE_MECHANISM_PASS**, **LOCAL_QUALITY_DELTA**, **NO_EFFECT**, **INCONCLUSIVE**, **NOT_RUN**, not one generic PASS.

Review: ensure the right human-selector/Chinese-pair-author/independent CODEX-judge roles; no edits to protected core docs; no Windows CI gate or 500-lesson repeat imposed before a differentiating causal result; no accidental teaching-label leakage into KNN keys; no claims of cross-domain winner without per-domain scores; no claims of independent Astra review without genuine execution.
