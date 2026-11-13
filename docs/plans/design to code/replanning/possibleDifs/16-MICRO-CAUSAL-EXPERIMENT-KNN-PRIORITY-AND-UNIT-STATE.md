# MICRO-16 · Минимальный причинный эксперимент: видна ли новая KNN-коррекция после настоящего урока?

**Статус 2026-10-16:** SPECIFIED / NOT_EXECUTED. **Один приоритетный эксперимент**, два дешёвых проверочных контроля. Не менять существующие RP01 results, dataset, checkpoint, admission и Windows GitHub CI. Источник кода: [current core exact SHA](https://github.com/lubochka/llm_mvp_core/tree/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5). Найденные причины и честные ограничения: [16 new differences](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md).

## Вопрос и предзарегистрированная гипотеза

**H16-KNN:** При одинаковых legal constraints, KNN index/state и одинаковом наборе записей, изменение только порядка admissible corrections меняет реально выбранный перевод; `Take(k)` до ранжирования по правоте/recency блокирует позднюю качественную коррекцию. Следовательно, **восстановление работающего пути обновления KNN** может улучшить unseen translation на новых перефразах — это **отдельно** от качества Student DPO/LoRA.

Три уровня проверки, не смешивать:
- **L0 / mechanism:** KNN старой версии не возвращает новую запись, несмотря на её присутствие в сохранённой памяти.
- **L1 / end-user effect:** на frozen 12 source-disjoint перефразах у ordinary translation consumer исправленный выбор даёт больше корректных законных переводов при той же памяти.
- **L2 / genuine learning effect:** после разрешённых новых lessons появляется прирост L1 на ранее не обученных *семействах* текстов, который исчезает в контрольной ветке без новых уроков; корректная память переживает cold restore.

**Нулевая гипотеза:** актуальный обычный consumer получает нужную коррекцию другим способом / она не legal или не подходит для другого источника / повторное ранжирование не влияет на корректность. Тогда не объявлять KNN причиной падения RP01.

## Phase A (десятки строк C#, 0 Student/Teacher calls): прямой воспроизводимый контрпример

Создать **изолированный** минимальный C# diagnostic в существующем test/probe проекте, не редактировать `TranslationKnnStore` до первого прогона. Конструктор и методы публичные, поэтому не нужен Reflection/DI. Этот probe проверяет **порядок памяти**, не перевод и не машинное обучение:

```csharp
using TranslationDomain;

const string signature = "SVO|VERB|NOUN";
const string source = "They submit the request.";
var memory = new TranslationKnnStore();

// Synthetic fixtures, NOT external model output / NOT approved DPO pairs.
for (var i = 1; i <= 5; i++)
    memory.AddPair(signature, source, $"OLD_{i}");

memory.AddPair(signature, source, "ACCEPTED_CORRECTION");

var hits = memory.FindNearestCascade(
    fullSignature: signature,
    prefixSignature: "PFX",
    structureSignature: "STRUCT",
    k: 5,
    querySourceText: source);

Console.WriteLine(string.Join("|", hits.Select(h => h.Target)));
Console.WriteLine($"memoryCount={memory.Count}; hitCount={hits.Count}");
```

**Prediction from source**, not actual observed run: `OLD_1|OLD_2|OLD_3|OLD_4|OLD_5` and `memoryCount=6; hitCount=5`. Why: `AddPair` appends; `SourceLaw.Compare` for this ordinary non-copular source is `Unknown`, not `Contradiction`; `FindExactWithSourceGuard` takes first `k`; `FindNearestCascade` returns first nonempty stage. Nothing invents scorer inputs. [AddPair/Take](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L202-L292); [SourceLaw](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMemorySourceLaw.cs#L23-L44).

**Controls (не трогая другие переменные):**

| Arm | Frozen six (signature,source,target) records | Только изменение | L0 prediction |
| --- | --- | --- | --- |
| A0 current | `OLD_1..5` затем `ACCEPTED_CORRECTION` | baseline | chosen last unseen |
| A1 order reversal | `ACCEPTED_CORRECTION`, затем `OLD_1..5` | insertion order | new first visible |
| A2 lawful latest-priority candidate | вход как A0 | сначала rank permissible candidates by explicitly registered approved correction version, then k | new visible |
| A3 no-new-lesson negative | только `OLD_1..5` | убрать новый пример | никакого нового knowledge нет |

A1 показывает зависимость от порядка, **не доказывает превосходство новой коррекции**. Для A2 нужна заранее зафиксированная законность/приоритет correction, а не правило «новее значит правильнее». Не внедрять permanent «last wins» без protection от плохих новых записей. Варианты: replace old entry только по authenticated `(language direction, domain, signature, canonical source)` при CODEX-approved correction; либо хранить reason/version/eligibility и score/rerank до `Take(k)`. Любой вариант допускает abstain и law veto.

**Cold restore subcheck:** после вызова internal `ExportCheckpoint` и `ImportCheckpoint` в test assembly сравнить records/count и порядок возврата; [store checkpoint](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L328-L351). Если корректирующая запись пропала из durable state — это **другая** причинная проблема, не смешивать с rank.

## Phase B (один настоящий недорогой quality A/B, при L0 confirm)

**Цель:** выяснить, влияет ли разница на *полезность*, а не на вывод debug list.

Подготовить один **заранее замороженный** panel:
- TRAIN: 6 lawful baseline source→target пар и 6 одобряемых **исправлений** на отличающихся вариантах/независимых источниках; не повторять closed heldout.
- HELDOUT: 12 запросов из других source/template families, с 4 near-miss/negation/changed actor cases, ещё 4 новых перефраза и 4 cross-target-language cases. Источник каждого независим от конкретной train-фразы; explicit `source_family_id` + `template_family_id` + checksum.
- Для каждой задачи до опыта CODEX/Astra UltraCode фиксирует правильную языковую направленность, lawful required meaning, допустимые альтернативы, critical error, legal abstain. Blind Student evaluation проводится **независимым** оценщиком post-update. Negative controls: бессмысленная новая коррекция (quarantined, не trained), тот же pool без последней коррекции, query с opposite polarity.

Ветка A: unchanged store/cascade, current 5-neighbour cap. Ветка B: **тот же точный train state и budget**, единственное изменение — по CODEX-заверенным lawful correction IDs сделать правильную eligibility/priority ДО cap; остальные rules, tokenization, flow, confidence, verifier, checkpoint, target language, model/adapters остаются одинаковыми. На каждом случае логировать `candidate_list_before_cap`, `candidate_list_after_cap`, `chosen_candidate_id`, `law_veto`, `final_output`, `correct/invalid/abstain`, `source_family_id`.

**Primary metric:** paired correct lawful end-user answers on 12 unseen tasks (n01/n10/n00/n11) плюс critical regressions. Дополнительные: coverage/abstain (не считать отказ правильным ответом), wrong-target-language, precision conditional on answering, latency and bytes index+checkpoint. No pre/post training accuracy surrogate. Если A и B различаются **только в ID**, но end-user quality не улучшилась или вырос critical error → `QUALITY_GAIN_NOT_ESTABLISHED`. 12 задач — диагностическая выборка, не статистическая публикация; для promotion расширить sealed family-disjoint pool до утверждённого sample и доверительных интервалов **без изменения гипотезы**.

**Отдельный cross-direction control:** 5 `EN→RU` записей и шестая `EN→ES` с пересекающимися сигнатурами; при общем store проверить, какой кандидат доступен **до** late direction check. Затем сделать такой же поиск с target-partition **до** top-k, сохранив исходные source-law и verifier. Ordinary flow проверять на перефразах, где exact/phrase bypass не сработал. Это не обучает новый Flow и не измеряет качество LoRA.

## Phase C (два микро-контроля ложного подтверждения обучения)

### C1. `TrainStructuredBatchAsync` может вернуть успех при отсутствии изменения Unit

В отдельной `TranslationMachine` записать hash только `ExportCheckpoint().Value.SemanticTokenizer` до/после (не весь checkpoint: history/scalars действительно изменятся). Вызвать один batch с записью:
- `UnitId="semantic-tokenizer"`, `HeldOut=false`, `InputJson="null"`, `ChosenOutputJson="null"`, остальные record-поля непустые, `DomainTag="TranslationDomain"`.
- `JsonSerializer.Deserialize<TokenizerOutput>("null")` возвращает null, [TrainOnExample](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L123-L140) выходит без mutation; [TrainStructuredBatchAsync](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L103-L145) **по коду** возвращает `RowsTrained=1`, `PromotedUnitIds` содержит Unit, `AccuracyAfter=1/3` (при отсутствии heldout).
- **Negative:** такой же row с `UnitId="missing-unit"`, `RowsTrained=1` при `PromotedUnitIds=[]`, no mutation.
- **Positive:** настоящий корректный JSON через `JsonSerializer.Serialize(new TokenizerOutput(SentencePattern.SVO, "some|signature"))` и `new TokenizerInput("They submit the request.", "en")`; корректный `SemanticSignature`. В этом случае новый `PatternCorrection` должен появиться после state compare.

**Решение:** в реальном продукте `rows_seen`, `rows_routed`, `rows_validated`, `actual_state_mutations`, `promoted_units` и `heldout_accuracy` должны различаться. Перекраска counter PASS не решает learning.

### C2. Метрика может сообщить «обучение пропало» после batch без зарегистрированного Unit

В той же машине выполнить valid known unit batch, сохранить `ExportCheckpoint.SemanticTokenizer` и флаг `HasStructuredTraining`. Далее выполнить batch с единственной записью `UnitId="missing-unit"`, сохранить снова. Код [присваивает флаг по текущему batch](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L124-L130), а [EvaluateAsync](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L162-L200) при false пропускает фактический экзамен. Отличать metadata-effect от actual persisted memory. В отдельной панели проверить что `EvaluateAsync` hardcodes RU→EN; если intended EN→RU, сравнить с ordinary consumer с явными `source_language="en"`/`target_language="ru"` ([code](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L686-L705)).

## Инструкция CODEX Astra 6 UltraCode (исполнение, оценка и review)

**CODEX GPT-6 Astra 6 UltraCode** выполняет **человеческий выбор** разрешённого урока, его scope и заранее фиксируемого gold. Если нужен новый **DPO**-урок — **разрешённая внешняя открытая китайская модель авторствует BOTH окончательные `chosen` и `rejected`**, exact bytes сохраняются. Затем **CODEX GPT-6 Astra 6 UltraCode отдельно оценивает качество обеих сторон/предпочтения**, включая `TIE`, `BOTH_BAD`, `UNJUDGEABLE`, provenance/licence. CODEX не выдаёт свои ответы за Teacher и не делегирует выбор человеку. **Independent post-training Student evaluator** проверяет новые ответы после cold restart; его роль не заменяет CODEX pair-quality review. Только реально допущенные пары могут идти в DPO optimizer; простые KNN fixtures выше — не DPO, не «внешний teacher».

Порядок для executor:
1. Читать pin-snapshot файла и названных методов, использовать skills `code-examination`, `human-readable-architecture-audit`, `experiment-design`, `phase-planning`, `plan-review`, `generated-code-review`, `prompt-patch-authoring`, `conversation-intent-and-self-reflection` из [canonical docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills). Зафиксировать actual runtime HEAD, branch и путь.
2. Запустить Phase A с **неизменным** current store, сохранить actual stdout в новом result файле. Один малый diagnostic project, не 20 Windows CI runs.
3. До Phase B заморозить все train/heldout IDs, source families, output gold, budgets, seed, law policy; разрешённые teacher calls/rights контролировать отдельно. Изменить ровно один ranking/filter seam, провести paired blind human-facing quality exam.
4. C1/C2 запустить адресно, при необходимости отдельно — не блокировать быстрый Phase A долгим DPO.
5. Architect делает независимый model-of-calls review; reviewer проверяет **точный final candidate SHA**. Ни этот документ, ни публикация его в GitHub **не являются independent Astra review**.
6. Итоговый `MICRO-16-RESULT.md` содержит: HEAD, code links, command and exit code, train/heldout hashes, exact A/B outputs, n01/n10/n00/n11, failure cases, saved/restored state hashes, size/time, logic-level verdict (L0/L1/L2) и ограничения. **Пока результатов нет**.

**Stopping rules:** если L0 не воспроизведён — `SOURCE_HYPOTHESIS_FALSIFIED` и не чинить; если L0 подтверждён, L1 нет — `MECHANISM_BUG_WITHOUT_MEASURED_GAIN`; если L1 вырос, но L2 нет — `RETRIEVAL_EFFECT_NOT_GENERALIZED_LEARNING`; только если все L0–L2 и controls проходят, говорить о полезном восстановлении фрагмента learning pipeline, **не** обо всём RP01.

## Практическая ценность по сравнению с полной кампанией

Phase A и C — дешёвые локальные reproducer-опыты **без** новой нейросетевой тренировки и внешней модели; Phase B — одна настоящая целевая оценка на 12 отложенных задачах с narrow patch. Доказанная KNN memory regression полезна для восстановления Unit, но **не исключает** независимую корневую гипотезу RP01: случайный B0 + q/v-only LoRA + короткое DPO обучение не обеспечили генеративной компетенции. Этот вопрос проверять отдельно через [RP01 tiny SFT-vs-DPO pilot](12-RP01-MINI01-COLD-START-CAUSAL-EXPERIMENT.md), не смешивая механизмы.
