# N56 — родительские RAG/WorkLeft: цепочка авторитета, различие experimental DPO и продуктового обучения

**Дата:** 19.10.2026. **Статус:** `SOURCE_RECONCILIATION / NO_NEW_NATIVE_RUNS`. Основание — прочитанные **активные вводные разделы** родительской библиотеки `llm mvp/design to code session/09.09.26WorkLeft-*`, не утверждение о новом полном пересмотре её сотен тысяч исторических строк. У старых RAG есть собственные временные слои; RAG link ≠ live source ≠ measured learning.

## 1. По какому указателю искать источник, не перепутать исходный смысл

| Родительский source | Положение из его актуальной преамбулы | Что проверять уже сегодня |
|---|---|---|
| [WorkLeft RAG Master Index](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md) | RAG указывает, где искать источник, а не доказывает «готово»; опорное решение и статус в WorkLeft REPORT-01 | Найти exact `claim/owner` → original source → current code → actual consumer |
| [WorkLeft RAG-00 Guide](../../09.09.26WorkLeft-RAG-00-usage-guide.md) | Активные источники и старые исторические цитаты должны различаться; проверка результата одной пары: candidate, restored model, original score, same independent exam | Нельзя взять постулированный «PASS» из старого RAG и назвать текущим качеством |
| [WorkLeft RAG-01 DPO legal](../../09.09.26WorkLeft-RAG-01-dpo-legal-build.md) | Legal training admission, content provenance, sealed dataset, paid reference not training side; production author/judge/student roles разные | Код `DpoTrainer` и proof пары **не** доказывают полезности пары для учебной задачи |
| [WorkLeft RAG-02 Design-to-Code](../../09.09.26WorkLeft-RAG-02-design-to-code.md) | Нужен полный цикл `product error → allowed DPO pair → learned weights → independent user-layout result → accept/revert` | Internal Critic accuracy, category weight, CSS scorer отдельно от готового HTML/CSS |
| [WorkLeft RAG-03 branches/merge](../../09.09.26WorkLeft-RAG-03-merge-and-branches.md) | Срез `bc3762f...` относится к времени RAG, dirty/unpublished branches не доказательство merge | Для сравнений PrepareToGPL, DesignToCode-clean, orchestrator-extensions использовать **актуальные pinned** native sources; исторический branch SHA не считать HEAD сегодня |
| [WorkLeft RESEARCH-03 DPO conformance](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md) | В актуальном R16: final chosen и rejected при продуктовой схеме должны быть authored разрешённой китайской учительской моделью; CODEX выбирает case и затем независимо judges exact pair; honest status неполного review | Отделить `deterministic simulation authoring` от `approved production authoring`, не выдавать simulation за completed Chinese-teacher curriculum |
| [WorkLeft RESEARCH-01 quality/speed](../../09.09.26WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program.md) | Продуктовый quality target **>=95%** — требование, не измеренный факт. Упомянуты Unit/Flow/Orchestrator/DynamicGraph и семь строк исследовательского плана | 2-lesson/4-exam — быстрый **локальный falsifier**, не сертификат глобального порога 95%; домен читать по актуальному [human glossary](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-communication-GLOSSARY.md) |

**Происхождение и иерархия:** WorkLeft RAG/decision = требования и указатель; точные commits/Go-Live code = потенциальный механизм; frozen native receipts = локальная эмпирика; независимая проверка + matched gold/current A/B = причинная приёмка. Более поздние контракты (актуальный `human-communication-GLOSSARY` 18.10) разделяют «домен = предметная тема», профили направлений, обучаемую Unit и механизм; старая строка «DPO как седьмой домен» — проектная классификация своего времени, **не новый предмет обучения**.

## 2. Важное расхождение двух законных режимов DPO

**Source code RP01:** [`CodeDocumentationSimulationDpoExperiment` L100–120](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L100-L124) пишет `provenance=simulation`, вход `DeterministicGenerator` для обеих сторон, принятый internal governance, типизированный content/material, frozen reference; [L159–170](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L159-L170) использует actual DPO recipient and trainer. В [RP01 source-map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md) зафиксированы main500 simulation и independent Student evaluation. Это **реальная экспериментальная тренировка**, но *не автоматически* продуктовая пара с human/teacher provenance.

**Program requirement WorkLeft R16:** [parent RESEARCH-03](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md) говорит, что для финальной DPO-пары продукта chosen и rejected создаёт разрешённая открытая китайская модель, без ложного переименования Student/paid reference в author; evaluator/judge не есть chosen author и не есть сам Student.

Отсюда два разных проверяемых вопроса, **не один дефект**:

- **A. Experimental learning utility (RP01 simulation):** меняется ли собственный ответ Student по независимому code-doc case при фиксированных 2–500 действительно разных, admitted deterministic pairs? Доказательство только изменение recipient+own EXAM; 18-Oct отдельные реальные LoRA изменения без качества показывают, почему это важно.
- **B. Product teaching provenance and effectiveness:** может ли реальный одобренный open teacher представить chosen/rejected на новом case; PairJudge независимо проверить; legal admission подтвердить origin; Student обновиться ровно этой новой парой, не прокрутить старые пары; после — независимый user task; если нет прироста/есть забывание, кандидат не promote.

**Нельзя:** (i) объявлять сам факт `DeterministicGenerator` «нелегальным» для отдельно разрешённого simulation — это неверный вывод; (ii) объявлять `Main500` завершением обучающего продукта с Chinese teacher, judge и real consumer — это другой, недоказанный вывод.

**Предварительная потенциальная причина низкого качества:** synthetic template-family repetition в RP01 (исторический [RP01-CURRENT-CODE-MAP](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md) описывает 25 templates×20 и не доказывает family independence) может дать loss/margin gain без source-conditional навыка. Это **уже документированный prior art**, не уникальная находка N56; точное тестирование предусмотрено protected [P81 input conditioning](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P81-input-conditioning.md) и [P112 family-disjoint](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P112-family-disjoint-evaluation.md).

## 3. Мини-проверка научной идентичности: старый положительный результат не становится новым правилом без адреса

**Один короткий исследовательский пакет**, обучающие updates 0 до установления owner:

1. Из RAG-01 и [RESEARCH-03](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md) выписать exact `chosen/rejected/author/judge/lesson` contract, current executed `DpoPair` и конкретный CLI input; не подменять label `chosen` фактом реального authoring.
2. Из RAG-02 и успешных branch receipts выделить роль `learned Unit → actual ordinary consumer` по каждой теме: C# documentation, EN→RU translation, Figma→HTML/CSS, orchestration. Доказательство из реального flow, не из имени классов.
3. Отметить временной класс каждого source: historical plan, earlier positive native, current implemented code, executed receipt, or future experiment. Единый результат `E26 40→100%` не приписывать новым Unit lessons без вызова `AddEntry`; таблица `BuildDefault` не является обучением.
4. Только после owner/path trace выбрать [X56 bounded A/B](56-CAUSAL-MICROEXPERIMENTS-BY-DOMAIN-2026-10-19.md), один фактор, реальные before/after ответы, explicit safeguards на prompt leak/same-family repeats, true timing/RAM. Protected experiments остаются неизменными.

**Что даёт этот проход:** устраняет semantic mismatch между родительским RAG и current RP01 simulator; **не утверждает новое native ухудшение**, не ретроспективно отбрасывает полезные deterministic эксперименты.

## 4. Crosswalk к защищённым экспериментам и открытые пункты

| Утверждение | Точное покрытие защищённым каталогом | Честный статус |
|---|---|---|
| Pair authorship, contrast validity, contamination and exact material | protected P04/P07/P14/P63/P81/P112 и [P04 preference contrast](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P04-preference-contrast.md) | DESIGN_EXISTS, native Chinese-author teaching utility **не** доказана этим аудитом |
| «Нейронная policy действительно научилась» | [protected small D](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md), [18-Oct actual neural](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/README.md) | Native 8-lesson negative 0/24→0/24; exact RP01 2-lesson runner MISSING; no whole-product certification |
| «kNN функции, proven from failed matrix, полезны по домену» | [X56 C1/K1](56-CAUSAL-MICROEXPERIMENTS-BY-DOMAIN-2026-10-19.md), protected P26/P27/P28/P35/P107/P117/P125 | Existing positive for EN→RU memory; AST syntax and translation order must be separate tests |
| cost/metric/promotion source-specific N56 | [N56 source](56-NEW-METRIC-PROMOTION-SEAMS-2026-10-19.md), [N56 audit](56-PROTECTED-EXPERIMENT-VALIDITY-2026-10-19.md) | Proposed A/B in **allowed planning**, read-only catalog has not been modified/verified to contain full new exact cases |

**Verdict:** `PARENT_RAG_TRAVERSED_AT_ENTRY_AND_DECISION_LEVEL`; `PAIR_PROVENANCE_EXPERIMENT_VS_PRODUCT_RECONCILED`; `HISTORICAL_POSITIVE_PRESERVED`; `NEW_NATIVE_TRAINING_NOT_RUN`; `FULL_SOURCE_RECURSIVE_RECONCILIATION_OPEN`.

[Основной результат аудита](56-INDEPENDENT-LEARNING-GOLD-AND-RP01-AUDIT-2026-10-19.md) · [проверка защищённого каталога](56-PROTECTED-EXPERIMENT-VALIDITY-2026-10-19.md).
