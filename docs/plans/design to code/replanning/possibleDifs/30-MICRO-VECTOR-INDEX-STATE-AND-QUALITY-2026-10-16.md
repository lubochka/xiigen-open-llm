# N30-MICRO — за один маленький опыт отделить порчу индекса от ухудшения обучения

**16.10.2026 · FUTURE EXECUTOR WORK ORDER / native NOT_RUN.** Источники: [N30-A failed Add](30-NEW-HNSW-FAILED-ADD-PARTIAL-MEMORY-2026-10-16.md), [N30-B/C alias + nonfinite](30-NEW-VECTOR-ALIAS-AND-NONFINITE-KNN-INGRESS-2026-10-16.md). Сначала три независимых Tier-1 probes, потом только при реальном consumer — одно парное domain-quality сравнение. Ничего в core не менялось.

## Цель для CODEX-исполнителя

**CURRENT TASK:** на одном pinned core SHA подтвердить/опровергнуть три конкретные механические гипотезы и проверить, могут ли они *при достижимом обычном KNN-потребителе* менять правильность ответа.

**NOT THE TASK:** не повторять DPO500/LoRA обучение, не объявлять косинусный пример обучением Student, не менять frozen C#500/ENRU500, не переписывать HNSW на новую библиотеку, не править защищённые core research docs, не запускать Windows CI ради этой проверки.

**SOURCE snapshot:** core codex/dpo-implementation@79082340f01b744cdf08185616f2bd818dbd06b8; перед запуском записать реальный HEAD и snapshot hashes. Использовать существующие IVectorIndex, HnswIndex, BruteForceIndex; новые C# test/helper файлы — только после отдельного разрешения CODEX на product-code changes. Ниже код — точное задание и **неисполненная** заготовка.

**SKILLS:** core/docs/skills: human-readable-architecture-audit-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, prompt-patch-authoring-SKILL.md, conversation-intent-and-self-reflection-SKILL.md, experiment-design-SKILL.md, claim-verification-SKILL.md. Эксперимент даёт только локальную диагностику до независимого source-disjoint examination.

## Предварительный Gate 0 — reachability

Найти реального caller HnswIndex/BruteForceIndex.Add/Search в текущем релизном пути, отметив, какой из них вызывается ordinary user request и откуда берутся float[] и их lifetime. Прямой RP01 CodeDocumentationSimulationDpoExperiment→DpoTrainer→TextGenerator не равен KNN path: при отсутствии вызова конкретного индекса писать **NOT_APPLICABLE_TO_RP01_NEURAL**, хотя API-risk может остаться для других функций. Заморозить исходные positive/negative/warning memories, domain, feature spec, encoder SHA, tokenizer SHA, index params, checkpoint, gold, evaluator и порядок lessons.

Фактически прочитанные callers [generic KNN](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L155), [transformer KNN](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L114-L207). В обоих fresh arrays; это negative evidence для B, пока не найден consumer с shared buffer.

## Gate 1: три независимые проверки без тяжёлого ML

### Arm A — failed Add / incomplete graph (N30-A)

**Baseline A:** оригинальный HNSW, после успешного вектора [1,0] попытаться добавить [1,0,0]. Поскольку в [Add](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/HnswIndex.cs#L49-L96) append происходит до cosine, прогноз — IndexOutOfRangeException + Count 1→2.

**Intervention B:** отказать в Add из-за несовпадающей длины ДО первой мутации (без изменения HNSW search/score или corpus). Прогноз — exception + Count 1→1. Сохранить HNSW с одинаковыми legacy параметрами до/после; осмотреть persisted Vectors/Ids/Graph, cold Load и возможность последующего валидного Add. Нельзя приписывать изменение ответов различию random HNSW graph; сравнение item-count механическое, качество потребует одинакового pre-saved index state или deterministic exact oracle.

**Metric:** count_delta_after_failed_add, persisted_failed_id_count, reachable_valid_ids / accepted_ids, плюс отдельно domain correct@1. **Falsifier:** native код откатывает append или реальный pipeline запрещает malformed vector до Add и не сохраняет ошибочный индекс. Если defect есть только при искусственном call, статус SOURCE-AND-NATIVE-CONFIRMED_LOCAL, но не observed RP01 regression.

### Arm B — external mutable float[] changes learned memory (N30-B)

**Baseline A:** BruteForceIndex.Add(value-owned mutable good=[1,0]→GOOD; bad=[0,1]→BAD); query [1,0]. После Add поменять good in-place на [-1,0] без Train. Прогноз GOOD→BAD.

**Intervention B:** до Add скопировать good в новый массив. С той же query, gold, bad и без изменения индекса/encoder прогноз GOOD→GOOD. Для HNSW отдельно log original graph hash, vector bytes before/after external mutation: граф неизменен, геометрия меняется. На одном и том же frozen HNSW graph сравнить query scores; **не** реконструировать граф с неконтролируемой RNG для механического контраста.

**Metric:** selected_id_before_after, score_before_after, index_vector_sha_before_after_without_train. **Falsifier:** index делает защитную копию или никакой actual caller никогда не изменяет переданный массив; даже если API эффект реален, нет consumer causal effect.

### Arm C — nonfinite similarity ingress (N30-C)

**Baseline A:** BruteForceIndex.Add([NaN,0]); Search([1,0], topK1). Прогноз nonfinite score.

**Intervention B:** перед Add/Search reject любой vector nonfinite с typed error, **не** превращать NaN в ноль молча. В HNSW проверить, что failed NaN Add не оставляет индекс в частичном состоянии (Arm A пересекается только общим invariant, причинный анализ раздельный).

**Metric:** nonfinite_score_count, rejected_nonfinite_count, index_mutation_after_rejection. **Falsifier:** найденный upstream всегда гарантирует finite и есть действующий guard раньше IVectorIndex, который исключает такую пару в обычном пути. Нет доказанной реальной upstream NaN инцидентности.

## Native C# console sketch: точные предположения, не runtime receipt

Создать во временном *разрешённом* экспериментальном инструменте маленький проект, ссылающийся на 06.VectorSearch.csproj. Код ниже разделяет A/B/C, не подменяет unit-level learning. Наличие dotnet и настоящая сборка в настоящем окружении исполнителя должны быть записаны отдельно.

~~~csharp
using VectorSearch;
using System;
using System.Linq;

static string Top(IVectorIndex index, float[] q)
    => index.Search(q, 1).Single().id;

// A: Add throws *after* _vectors/_ids/_graph were appended.
var h = new HnswIndex(m: 8, efConstruction: 64, efSearch: 32);
h.Add(new float[] { 1f, 0f }, "good");
int aBefore = h.Count;
bool aThrew = false;
try { h.Add(new float[] { 1f, 0f, 0f }, "bad_dim"); }
catch (IndexOutOfRangeException) { aThrew = true; }
Console.WriteLine($"A threw={aThrew} count={aBefore}->{h.Count}");

// A-control: deny invalid shape before entering Add.
var hc = new HnswIndex(m: 8, efConstruction: 64, efSearch: 32);
hc.Add(new float[] { 1f, 0f }, "good");
bool controlDenied = false;
try {
    var vector = new float[] { 1f, 0f, 0f };
    if (vector.Length != 2) throw new ArgumentException("VECTOR_DIM");
    hc.Add(vector, "bad_dim");
} catch (ArgumentException) { controlDenied = true; }
Console.WriteLine($"A control_denied={controlDenied} count={hc.Count}");

// B: keep reference vs clone, exact brute-force oracle.
var good = new float[] { 1f, 0f };
var exposed = new BruteForceIndex();
exposed.Add(good, "GOOD");
exposed.Add(new float[] { 0f, 1f }, "BAD");
var protectedIndex = new BruteForceIndex();
protectedIndex.Add((float[])good.Clone(), "GOOD");
protectedIndex.Add(new float[] { 0f, 1f }, "BAD");
var query = new float[] { 1f, 0f };
string before = Top(exposed, query);
good[0] = -1f;
string after = Top(exposed, query);
string control = Top(protectedIndex, query);
Console.WriteLine($"B {before}->{after}; copied={control}");

// C: same dimensionality, invalid finite value.
var nonfinite = new BruteForceIndex();
nonfinite.Add(new float[] { float.NaN, 0f }, "NAN");
float score = nonfinite.Search(query, 1).Single().score;
Console.WriteLine($"C isFinite={float.IsFinite(score)}");

// No assertion of product quality/Student learning from these outputs.
~~~

Expected from source, **NOT observed**: A thrown=true/count 1→2 versus guard count1; B GOOD→BAD versus copied GOOD; C isFinite=false. CODEX executor must record real actual stdout and exceptions; discrepancies override predictions. Do not call this a PASS before executing.

## Gate 2 — если Gate 0 доказал живой learned KNN consumer

Один небольшой слепой набор 8–12 **source-family-disjoint** задач в *одном* домене, с конкретным legitimate output и критической ошибкой, плюс 3–4 retention из старого домена (если применимо). Сначала CODEX на Astra 6 UltraCode выбирает уроки и проверяет допустимость доменных примеров. **Внешняя открытая китайская модель создаёт обе стороны chosen/rejected DPO**, а **CODEX на Astra 6 UltraCode независимо оценивает качество exact DPO pair**; для чисто KNN corpus допускается не-DPO labeled-memory путь с явным provenance, нельзя выдавать его за китайскую DPO генерацию. Blind quality final ordinary answers проверяет отдельный CODEX/Astra 6 UltraCode evaluator, не generator или optimizer self-metric.

Пары сравнения: (i) неизменный source / frozen lessons / active caller; (ii) *ровно один* исправленный индексный invariant, остальные memory/encoder/candidate pool/index RNG/checkpoint/config/decoder unchanged. Если индекс повреждён, сохранять same graph baseline snapshot; для доменного сравнения при rebuid graph записать variation between seeds и не выдавать одну удачную топологию за качество learning. Измерить per-case correct/partial/critical/abstain, top-k coverage, corrected learned lesson reachability, p95 query latency, peak-own-process memory; отдельно отмечать NaN rejection coverage. Predeclare success as **zero new critical errors + paired correct@1 improvement on reachable affected cases** без снижения lawful coverage; если 0 affected cases, verdict NOT_APPLICABLE_FOR_THIS_CORPUS, а не успех.

## Review + logging schema

Сохранить 1 компактный JSON/Markdown receipt:

- git_core_sha, git_planning_sha, source_file_line, experiment_id (N30-A/B/C), tier, status (SOURCE_CONFIRMED | NATIVE_OBSERVED | CONSUMER_OBSERVED | FALSIFIED | NOT_APPLICABLE), deterministic_input_sha;
- index_type, dimensions, vector_finite, learned_example_id, domain/source_family_id, expected_vs_actual_count, exception_type, score_ids, index sha before/after, save/load sha;
- actual_caller_path, actual_consumer_path, learning_event_id, model/encoder/tokenizer fingerprint, selected_prediction, blind_grade, negative-control outcome;
- wall-time, peak own-process RSS, independent evaluator model real runtime id, exact differences and unchanged factors;
- explicit **NOT_RUN/UNKNOWN** for steps not performed.

**Self-review before publication:** no zero-effect mixed results called a success; no toy ANN result sold as broad RP01 neural rescue; no producer role confusion; no HNSW seed/rebuild confounding; no retrospective frozen gold changes; no instruction to edit protected core docs. Implementation must begin only after direct user authorization for code changes.

**Next decision:** если core original caller не reachable from user request — сохранить finding как API hardening backlog и вернуться к P0 B0 warm-up vs DPO-only / genuine historical KNN consumer parity из [N29 register](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md). Не запускать новый 500-pair batch для закрытия этого index-specific finding.
