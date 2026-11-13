# 13 — MICRO-KNN-01: быстрый причинный эксперимент «обученные соседи есть, но не участвуют в решении»

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Причинный вывод требует различающего состояния и исправного измерителя.** P103 показывает 0/3 → 3/3 на конкретных уроках, но известные экзаменационные строки совпадают с TRAIN; забывание не измерено. P104 сохраняет 2/5 → 4/5, однако четыре прямые подстановки false не являются отрицательными контролями. В P107 холодная память не отвечала, после обучения достигнуты 8/8 и 4/4; вклад большинства неразличим при нуле разногласий с первым соседом.

Назвать личности S0/S1, один фактор, источник выбранного содержимого и полный ответ. Для проверки удержания заменить шаги при прежней длине; неизменный оценщик обязан заметить ошибку. Для first-hit/majority нужен случай разных решений; синтетический контроль подписывается своим ограниченным уровнем.

Дополнительные fits и queries входят в расход, даже если названы диагностическими: P107 остаётся 51/50, дополнительные вызовы P103 не исчезают. Исправление описания по сохранённым данным не назначает новый Train.

Основание: [POST107-аудит](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и [проверка реального кода](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md). Исторические результаты и бюджеты сохранены; это условия будущего переноса, не новый запуск.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


**Дата 16.10.2026. Статус: `EXPERIMENT_SPEC_ONLY / NOT_EXECUTED`.** Эта карточка — готовая постановка небольшого локального опыта, не утверждение об уже измеренном улучшении. Область: `llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`, план `xiigen-planning/llm_plan3`. Запрещены изменения существующих весов, published datasets RP01 (C#500/ENRU500), релиз, Windows CI, 500-row training. Первой проверяется одна функциональная потеря, а не сразу DPO, LoRA и граф.

## 0. Гипотеза и какое расхождение доказываем

**H-MICRO-KNN-01.** При идентичных обучающих парах, BPE-векторизаторе и HNSW-индексе нынешний `KnnLogicalUnit` чаще выдаёт ошибочный ответ на новые law-sensitive C# случаи, поскольку получает `k≥3` соседей, но фактически использует только `hits[0]` как итоговый output. Другой, разрешённый по законам домена кандидат уже может находиться в остальных `hits`; использование всей выборки кандидатов при неизменном обучаемом состоянии улучшит качество. Если это так, потеря алгоритмической детали KNN действительно влияет на **качество работы обученного Unit**, а не только на число evidence IDs. **Это не проверка качества градиента RP01 DPO/LoRA.**

**Проверенный кодовый seam:** [KnnLogicalUnit.cs:103–155](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L103-L155) обучает BPE 320 и строит HNSW; [строки 202–258](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L202-L258): `var hits = _index.Search(query,_k); var best=hits[0]; ... bestExample.Output`, а все ranked IDs размещает в `EvidenceIds`. **Наличие `k=3` не означает применение трёх ответов**. Первичный архитектурный источник: [original task](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/original%20task.md#L12-L23): domain/vector/output, typed many→many; исторический arbiter/KNN amendment требует сохранить законность и типы кандидатов, не заменять learned decision first-hit.

### Где разместить минимальную реализацию

**В `llm_mvp_core` только временная изолированная экспериментальная ветка/patch, не менять production-класс.** Новый один файл `Tests/KnnNeighborUseMicroExperimentTests.cs` (существующие `Tests/LlmFromScratch.Tests.csproj`, xUnit, net9.0; по стилю [KnnLogicalUnitTests.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/Tests/KnnLogicalUnitTests.cs#L8-L45)). Новый небольшой fixture рядом `Tests/TestData/KnnNeighborUseMicro-v1.json` с разделами train/dev/heldout. Возможен `Skip`/manual research runner вместо обязательного assertions о росте качества: **отрицательный научный результат не должен краснить продуктовый CI**. Вывод — отдельный `results/MICRO-KNN-01/result.json` в выделенной экспериментальной папке, а не переписывание existing historical JSON. Все права и ограничения из root/slice AGENTS остаются действующими.

## 1. Сценарии и разделение без утечки

**Домен:** C# analysis → выбор маленького структурированного `ActionLabel`, не свободная генерация текста и не EN→RU. Три класса ошибок: (a) `NULL_DEREF` / `GUARD_PRESENT` (доступ к nullable после отсутствующего/имеющегося guard); (b) `CANCEL_NOT_FORWARDED` / `CANCEL_FORWARDED` (распространение CancellationToken в async call); (c) `ORDER_VIOLATED` / `ORDER_OK` (порядок валидирующих/публикующих вызовов). Выходы — заранее объявленные action labels, а не строки, сочинённые judge после ответа. **Do not invent schema:** названия — только экспериментальный fixture, не новый общий production enum.

Собрать **12 разрешённых учебных примеров** (по 2 для каждого из шести состояний), **6 dev примеров** и **12 закрытых heldout** (по 2 на состояние) из других функций/имен/текстов, с отдельно отмеченной lineage family. Не использовать один и тот же AST/парафраз/переименование переменной в train и heldout. Взять C# фрагменты из собственных синтетических fixture без private customer data; lawful action label проверяется по коду и короткой заранее записанной рубрике. `not`/`!`, `return`, отмена, nullability, порядок и номера — adversarial случаи. Не выбирать subset после просмотра ответов.

Примеры **структуры**, не обещанные outputs реального запуска:

```text
TRAIN:  void A(User? x) { Console.WriteLine(x.Id); }
       law=NULL_DEREF; chosen action=ADD_NULL_GUARD
TRAIN:  void B(User? x) { if (x is null) return; Console.WriteLine(x.Id); }
       law=GUARD_PRESENT; chosen action=NO_EXTRA_GUARD
HELDOUT: void C(Account? a) { Audit(a.Name); }
       law=NULL_DEREF; expected=ADD_NULL_GUARD
HELDOUT: void D(Account? a) { if (a == null) throw new ArgumentNullException(); Audit(a.Name); }
       law=GUARD_PRESENT; expected=NO_EXTRA_GUARD
EDGE:   void E(Account? a) { if (a != null) Audit(a.Name); }
       law=GUARD_PRESENT; expected=NO_EXTRA_GUARD
```

Каждый fixture row: `id, lineage, input_csharp, input_structural_predicates, expected_action_label, domain, trap_tags`. `input_structural_predicates` извлекаются одной **фиксированной** простой AST-функцией из входного текста, без чтения ожидаемого label и без внешнего LLM. **Для C# передавать те же predicates в train Example.PredicateIds и в request.Metadata["predicate_ids"]**. Для выбранных ловушек — hard-law constraints вычисляются независимо от ground-truth, не вручную по результату каждого запроса; сообщить и результат `gate-only` без learned candidates.

## 2. Один и тот же обученный Unit, одно изменение: использование соседей

Создать **ровно один** `KnnLogicalUnit<string,string>.Train(...)` на train12, `k=3`, `vectorDimensions=64`, `confidenceFloor=0.70`, `textSelector=x=>x`. В исходном [KnnLogicalUnitTests.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/Tests/KnnLogicalUnitTests.cs#L10-L46) показаны реальный `Train`, `LogicalUnitRequest<string>`, `ExecuteAsync` и `EvidenceIds`. Никаких новых trainable layers, LoRA, новых encoder/Tokenizer и переобучений по arms. Последовательно подать **одни и те же 12 heldout запросов** и записать ordered hits, confidence, chosen output, latency и expected label.

**Контроль A / CURRENT FIRST-HIT:** прямо `ExecuteAsync(request)`, брать `result.Value.Output`. Так устроен текущий метод. **Интервенция B / SAME-HITS TOP-K LAW FILTER:** в экспериментальном sidecar после `ExecuteAsync` взять первые `k` идентификаторов из `result.Value.EvidenceIds` (но сперва убедиться, что они действительно ranked HNSW hits, а не дополнительные source-evidence; для демонстрации в train задать `EvidenceIds=[]`), разрешить каждый ID по map исходных *train* примеров, исключить кандидатов с несовместимым с запросом структурным классом/проверенным hard-law constraint, затем взять первого оставшегося в прежнем порядке. Если пригодных нет — `ABSTAIN` (или сохранить A как отдельно отмеченный fallback, не подменять veto). Никакого доступа к `expected_action_label` в selecting code.

**Контроль C / SHUFFLED-OUTPUTS:** те же индексы/вектора/ids/predicates и тот же B selector, но *только* output labels учебных примеров заменены фиксированной заранее permutation так, чтобы часть меток не соответствовала признакам. Если C не хуже B, вероятна зависимость от невыученного детерминированного правила/утечка. **Контроль D / RULE-ONLY:** применить одинаковый fixed constraint без чтения train map и явно записать, смог ли он сам выдать правильный action label. Если D полностью решает опыт, нельзя приписывать B улучшение обучению kNN.

**Практическая граница:** B не является новым trainable reranker и может лишь показать ценность дополнительного использования *обученной памяти*. Если raw ranked list текущего API недостаточен, **не менять** production API ради эксперимента: вернуть экспериментально полученные first k через изолированную instrumented-copy/internal test seam с теми же построенным index+tokenizer; сравнить совпадение top-1 и checksum. Нельзя считать один `EvidenceId` гарантированным raw score, потому что score для остальных не экспонируется.

### Минимальный контур реализации (псевдокод, не готовый C# проект)

```csharp
var learned = KnnLogicalUnit<string,string>.Train(
    "micro-knn", "micro-classifier", "csharp", trainExamples, s => s,
    vectorDimensions: 64, k: 3, confidenceFloor: 0.70);
Require(learned.IsSuccess);
foreach (var item in sealedHeldout)
{
    // Serialize ONE request + metadata once; reuse for every arm.
    var actual = await learned.Value!.ExecuteAsync(BuildRequest(item));
    var rankedIds = ReadFirstKRankedIds(actual, 3);  // check evidence contract
    var A = actual.Value!.Output;
    var B = LawAwareSelect(rankedIds, TRAIN_MAP, ExtractPredicates(item.Input));
    var C = LawAwareSelect(rankedIds, SHUFFLED_TRAIN_MAP, ExtractPredicates(item.Input));
    var D = RuleOnlySelect(ExtractPredicates(item.Input));
    RecordBeforeGrading(item.Id, rankedIds, A, B, C, D);
}
// Grade only after frozen raw predictions are written.
```

## 3. Меры влияния **на качество**, а не на число тестов

Primary `paired_heldout_semantic_accuracy`: доля 12 задач, где action label удовлетворяет заранее определённому содержательному классу. Отдельно: `wins_B_over_A`, `losses_B_over_A`, `net_wins`, `ABSTAIN`, критические нарушения закона (неверно проигнорирован null/отмена/порядок), unsafe false approvals, candidate-recall@3 (содержится ли подходящий обученный аналог вообще), `first_hit_invalid_but_later_hit_valid`, determinism/gate-only control, latency и managed allocation. Exact new-line/word match — не семантическая метрика.

**Ключевой признак причинного действия именно top-k consumer:** минимум один trial с `A wrong` → `B correct` и с доказательством, что корректный training candidate был в исходном ranked k, но **не использован** из-за `hits[0]`; same input/index/hits; B не использовал gold. **Исследовательский GO-кандидат**, заранее до запуска: хотя бы `3` net правильных на 12 и **0** новых критических ошибок B против A, B лучше C и D; маленький pilot **НЕ** достаточен для production promotion или статистически сильного вывода. `REGRESS`, `NO_GAIN`, `INVALID` и `INCONCLUSIVE` сохранять как результаты.

**Если B=A на всех 12:** гипотеза для панели не подтверждена, даже если она имеет смысл архитектурно; проверить `correct candidate in top3`, а не просто увеличить k. **Если retrieval@3 плохой:** проблема могла быть в feature-vector/tokenizer/index, а не decision. **Если B>C, но B=D:** эффективен fixed law extractor, не доказан learning gain. **Если C≈B:** возможна leakage/неправильный evaluator или результат не использует learned labels. **Если B лучше A, но ответы не доходят до обычного consumer:** подтверждён только unit-level effect, а не исправление RP01 end-to-end.

## 4. Runbook: минимальное исполнение и результаты

```bash
# Из отдельной disposable ветки репозитория llm_mvp_core:
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~KnnNeighborUseMicroExperiment" --no-restore
```

Если текущий SDK или восстановленные зависимости не готовы — запуск с обычным restore, без Windows matrix; не утверждать `PASS` до реального exit-code+JSON. **Не запускать** полный `dotnet test`, GitHub Actions или большой 500-pair DPO цикл в качестве проверки этого research question. Один исследовательский runner/test + одна train fixture + отчёт достаточны для первоначальной диагностики.

Вывести `result.json`: `source_ref_sha`, `test/code sha`, `dataset_sha`, `per-item train family / holdout family`, `vector_dim=64`, `k=3`, `BPE vocab320`, `HNSW m/ef`, `same_trained_index=true`, `predictions A/B/C/D`, `topk_ids`, `constraint_reasons`, `gold_after_freeze`, `per-arm semantic grade`, `wins/losses/abstains/critical`, `wall_ms`, `process_peak_rss_if_measured`, `status` и `next_falsifier`. Не замещать наблюдаемую память индекса суммой managed allocations. Сохранить честное `NOT_MEASURED` для недоступных метрик.

## 5. Что делать по исходу, без большого перепланирования

| Результат MICRO-KNN-01 | Адресный следующий шаг |
|---|---|
| B улучшает unseen A, превосходит C/D, траектория доказана | Реализовать минимальный learned-candidate arbiter внутри существующего Unit/Flow owner, сохранить typed outputs и реальные provenance; затем повторить на другом домене и RP01 ordinary consumer. Не переписывать DPO. |
| A и B одинаковы, но top3 содержит правильный кандидат | Диагностировать scorer/eligibility/topK order: достаточно ли структурных features, почему выбрана неправильная первая строка. |
| Нет правильных кандидатов в top3 | Сравнить structural Roslyn vector и hashed BPE, exact index vs HNSW и доменное scoping как отдельный **следующий** one-factor experiment (см. документ 12). |
| D (=fixed rules only) не хуже B | Нет доказательства обучения. Искать реально обучаемый scorer/metric/credit recipient, не продвигать deterministic rule под видом ML. |
| B полезен внутри Unit, но RP01 ordinary остаётся 0/3 или 0/4 | Проверить конкретное producer→KNN Unit→Flow→ordinary consumer соединение в RP01/RP02/RP03; это другая гипотеза и отдельный 2-arm replay. |

**Проверка навыками:** [experiment-design-SKILL](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/docs/skills/experiment-design-SKILL.md), [code-examination](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/docs/skills/code-examination-SKILL.md), [trainable-architecture-decision](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/docs/skills/trainable-architecture-decision-SKILL.md), [plan-review](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/docs/skills/plan-review-SKILL.md), [human-readable-architecture-audit](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/docs/skills/human-readable-architecture-audit-SKILL.md). Здесь **нет** независимого исполнения CODEX/Astra, actual train, тестов или модели.

## 6. Связь с соседними документами

- [12 — сравнение KNN Tier1/Tier2 и matrix→KNN lineage](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md).
- [11 — реальные DPO/translation runs из соседних веток](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md).
- [05 — векторы и текущий first-hit](05-SEMANTIC-VECTOR-KNN.md).
- [09 — историческая оценка без matched-panel](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md).

**Честный предел:** опыт проверяет **одну** утерянную деталь применения KNN; он не удостоверяет глобальное ухудшение RP01, не восстанавливает автоматически LoRA/DPO/Unit трансформацию и не устанавливает генеалогию конкретного old matrix function. Для утверждения о качестве RP01 необходим второй same-Student/same-holdout ordinary consumer experiment после положительного unit-level сигнала.

## 7. Обязательное дополнение: доказать эффект **урока**, а не только качество reranking

Первые A/B/C/D на одном уже обученном индексе доказывают **качество использования памяти**, но сами по себе не доказывают, что **новая обучающая пара** улучшает навык. Поэтому, если цель — именно `LEARNING_QUALITY_CAUSAL`, добавить ещё один малый шаг, оставаясь в тех же 12 train / 6 dev / 12 holdout:

1. Разделить 12 train на первые `T0=6` и дополнительные `T1=6` **до** запуска, с источниками/lineage и фиксированным curriculum. Все 12 holdout остаются закрытыми.
2. Построить состояние `S0=KnnLogicalUnit.Train(T0,k=3,vectorDimensions=64)`; один раз оценить те же A0=first-hit и B0=top-k lawful на закрытой панели. Зафиксировать `datasetSha / orderedIds / logits=N/A / predicates / outputs` и запретить пересмотр трепов после просмотра grade.
3. Построить `S1=KnnLogicalUnit.Train(T0+T1, точно те же параметры)`. Повторить **идентичные** вопросы → A1/B1; не менять экзамен и законный фильтр. Внутри каждой версии A/B используют **один** обученный index/encoder: только selection differs.
4. Записать изменения `A1−A0`, `B1−B0` и разность изменений `[(B1−B0)−(A1−A0)]`. Проверить случаи, где `T1` добавила подходящего соседа, A всё ещё выбирает неверного top-1, а B выбрал новый законный пример. Для каждого такого случая сохранить `source example id`, ranked hits обоих состояний, входные predicate и полный before/after answer. C=shuffled output и D=rule-only повторить на `S1`.

**Важный методический предел:** `Train(...)` заново обучает BPE320 и строит HNSW. Поэтому переход S0→S1 меняет **и примеры, и tokenizer/index**; это корректный быстрый тест качества всего learn-then-retrieve pipeline, но **не изолирует влияние только нового примера при frozen vectors**. Для строгой изоляции дополнительного урока потребуется заморозить BPE/state и добавить один вектор в index в isolated harness — отдельный probe, **не обещать**, что текущий public `KnnLogicalUnit` умеет `TrainOnExample` incremental. Нельзя приписывать весь S0→S1 gain одному source row.

**Решение для именно этой гипотезы:** положительный научный сигнал — одновременные (i) `B1 > B0` на новых source families, (ii) `B1 > A1`, (iii) measured benefit над shuffled/rule-only, (iv) хотя бы один точный T1→neighbor→B1 output provenance. При `B1 > A1` без `B1 > B0` обнаружена лишь проблема **выбора** уже существующих соседей, а не улучшение **обучения от нового урока**. При `B1 > B0`, но `B1≈A1` урок полезен и без multi-neighbor алгоритма; специфическая KNN-деталь не признана причиной.

Не считать изменение единственного результата статистическим доказательством generalization; сохранять per-item трассу и малый доверительный статус `EXPLORATORY`, затем при positive запускать ту же карточку на независимом домене. Эта секция уточняет назначение MICRO-KNN-01 без дополнительных 500-парных тренировок.
