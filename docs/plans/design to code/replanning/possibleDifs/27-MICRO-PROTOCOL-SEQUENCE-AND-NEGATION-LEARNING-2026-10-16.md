# MICRO-N27 — два коротких проверочных эксперимента вместо новых 500 DPO-пар

<!-- CODEX-POST128-PLANNING:START -->
## Действующее уточнение после P128 — Исполненный order-контроль и совокупная цена

Проверенный core: `f8784df037b59cf878889eef4d0956f5d0c985e7`; planning: `5df24354aa803f7ed5dd3bac48146466e2feb189`. Это адресное усиление приёмки соответствующего класса, а не новая аттестация всех исторических опытов карточки.

P127 уже записан дважды: p1/p2 и h1/h2 — A 1/2→B 2/2, C 0/2. Вторая попытка имеет девять реальных mandatory checks и контроль C→B от проходящей базы; не повторять старое обвинение в константном status. Проверяются полный ordered payload и graph trace, а не число шагов или словарь. На обе попытки с контролями: 10 fit, 100 уроков, 136 чтений, 24 холодных наблюдения, 24 graph/48 node calls и шесть проверок квитанции. Save/load API отсутствовал: восстановление не исполнено. Узкий контроль порядка не доказывает общее усвоение отрицания.

**Передать исполнителю вместе с собственным паспортом:** проследить названный actual consumer, сначала принять полный honest пакет, затем отвергнуть одну адресную порчу тем же checker; результат включить в общий verdict и внешний exit. Указать источник/сборку, фактический расход каждой попытки и предел утверждения. Ремонт expected опирается на независимый договор, не на actual сломанного кода. Сохранённый PRIMARY не повторять; gold, исходные бюджеты и прошлые результаты не менять. Активная попытка сохраняет ранее выданный ей pin карточки. Native source/static counterexample и исполненный тест обозначать отдельно.

[Сводка нового аудита](codex-2026-11-03-POST128-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [действующая core-карточка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/KNN-ORDER-PERMUTATION-01.md#current-executor-task). Историческое содержание ниже сохранено.
<!-- CODEX-POST128-PLANNING:END -->

**2026-10-16 | EXPERIMENT_SPEC + TOY_JS_EXECUTED | NATIVE_DOTNET_NOT_RUN | BLIND_QUALITY_NOT_RUN | INDEPENDENT_REVIEW_PENDING.**
Связанные улики: [27 new findings](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md). Snapshot core [8c7d416](https://github.com/lubochka/llm_mvp_core/tree/8c7d41611aecba98d3d1bbdbf30d69bd784949ae). Не редактировать core `docs/plans/replanning-possible-diffs`, не подгонять RP01 thresholds, не запускать Windows GitHub CI в этом эксперименте. Никаких внешних teacher вызовов здесь **не имитировать**; synthetic fixture не DPO.

## Быстрый локальный прогон, уже исполненный

В этом же каталоге создан [исполняемый JS toy-probe](27-QUICK-SEQUENCE-POLARITY-TOY-PROBE.mjs) и [его реальный receipt](27-QUICK-SEQUENCE-POLARITY-TOY-RECEIPT.json).

```bash
node 27-QUICK-SEQUENCE-POLARITY-TOY-PROBE.mjs
```

**Результаты V8 toy-прогона, не .NET и не Student:** `seed=42`, 4 синтетические train-последовательности (156 истинных соседних переходов), 2 отдельные heldout-последовательности (46 переходов); после токеновой перестановки создано **143/156 переходов, отсутствующих в исходных законах**. У простейшего обучаемого счетчика переходов правильность на heldout 46/46 при сохранении соседства и **5/46** после перестановки. В мини-примере EN-полярности удаление `not` даёт overlap=1 для противоположных источников; исходный порядок двух выученных переводов отвечает 1/2, дополнительный polarity-filter — 2/2.

Это показывает **существование потенциально сильного эффекта потери соседства и полярности в контролируемой игрушечной задаче**. НЕ означает 89% потери качества у реальной XIIGEN, НЕ воспроизводит C# RNG/`StratifiedSplit` побайтно, НЕ проверяет `ProbeNeuralNetwork`, `TranslationKnnStore`, настоящую DPO-пару, LoRA, RU/HE, downstream verifier или вызов ordinary RP01. Toy проводился только для выбора дешёвых реальных тестов; числа не входят в product score.

## Эксперимент A — защищённые переходы vs случайная перестановка

**Объект:** независимые `35.ExperimentsV2` и их `ProbeNeuralNetwork`; **не** `11.DPO/DpoTrainer`. Проверяем, могла ли искусственная `StratifiedSplit` уничтожить предметный supervision до выбора архитектуры модели.

**H_A:** при равных учебных примерах, токенах, количестве `TrainStep`, seed, модели, eval случаях и budget сохранение исходной смежности предикатов лучше восстанавливает исходные переходы, чем посимвольная stratification перед созданием пар.

**Действия для исполнителя (одна focused C# console/test-probe в существующем workspace, никакого изменения production classes):**

1. Закрепить **две доменные семьи** (например, C# nullable validation и command dispatch), по 3–5 исходных крошечных файла и 2 heldout файла каждой. Предметные выходы rubric независимо выписаны до запуска. Все последовательности извлечь тем же `LogicExtractor.Extract`; хранить `source_file_id`, `position`, `predicate_id`, `domain`, `raw_source_sha`. Малую синтетику оставить только mechanism control; дополнительно реальные source-disjoint последовательности из RAG.
2. Split сделать **сначала на уровне полного файла/семейства**, не отдельных токенов. Получить оригинальные соседние пары `(p_i,p_{i+1})` внутри одного файла, **не через границу двух файлов**.
3. Контроль A: пары из оригинальных последовательностей. Treatment B: по точной `StratifiedSplit.Split(allPreds,p=>p,0.8,42)` с теми же train-source предикатами, затем смежности после перемешивания. Для чистого качества использовать одинаковое число `TrainStep` (детерминированно обрезать/добирать число B-пар без новых gold labels). Замерить `synthetic_edge_count`, `train_source_pair_count`, `heldout_source_pair_count`.
4. На обоих плечах `new ProbeNeuralNetwork(vocab=22,hidden=32,learningRate=0.02,seed=42)`; одинаковые epochs, initial parameter hash, training-step count. На каждом реальном heldout-файле `Evaluate(original_pred_sequence)`, а НЕ `Evaluate(shuffled_tokens)`. Получить пофайловую точность и неверные переходы, не только aggregate.
5. Только если A>B по original heldout, добавить отдельный **2×2 experiment**: `pair integrity` × `example weighting`. В E29v2 «chosen first distinct token×2» НЕ является настоящим DPO: для весов нужен либо независимый human/teacher label, либо явно назвать их toy frequency experiment. Количество optimizer updates необходимо контролировать; нельзя менять одновременно градиентную норму и adjacency.
6. Отдельное следствие для E44/E45: повторить сравнение **фиксированных исходных переходов** с одинаковыми train IDs, только порядок mini-batch или вес (изменять по одному). Не присваивать `weighted_f1`, если нет precision/recall; основной score — source-heldout transition accuracy + предметная rubric; secondary — latency и cost.

**Фальсификаторы:** native `StratifiedSplit` сохраняет все границы и все исходные смежности (опровергает механизм); A и B на matched source-disjoint panel одинаковы или B выигрывает (опровергает предположение о вреде для качества в этом corpus); (A) лучше только на train и не на heldout (overfit). При провале НЕ переносить вывод на LoRA/Student.

**Минимальный научный критерий** (для triage, не статистическая достоверность): source-disjoint heldout ≥12 примеров; A−B ≥3 правильных из 12 при нулевом ухудшении критических law cases; signed per-case таблица до подбора гиперпараметров. Если нет явного выигрыша — STOP, не расширять до 500/Windows. Для A/B на целых последовательностях один run seed42 + контрольно seed43; каждую попытку записать, не выбирать лучший seed задним числом.

## Эксперимент B — KNN memory знает обе полярности, но может выбрать противоречащую

**Объект:** `TranslationKnnStore`, `TranslationSemanticTokenizer`, `ExecutorUnit`, `TrainedTranslationAdapter`, `TranslationMachine`. Это НЕ direct RP01 Student free decoder.

**H_B:** source-similarity, исключающая `not/no`, вместе с узким copular source law иногда пропускает более раннюю противоположную по смыслу пару; добавление **контекстно-ограниченного polarity-aware candidate admissibility** при неизменных TRAIN translations улучшит качество на heldout фразах и не уменьшит законный recall.

**Готовый L0 mechanism control (на настоящих публичных C# методах):**

- Одна искусственно совпадающая `fullSignature`, например `"ACTION|APPROVE|REQUEST"`, чтобы **изолировать именно source filter**; один bucket хранит `("approve the request","APPROVE")`, затем `("do not approve the request","DO_NOT_APPROVE")`. `FindNearestCascade(full,prefix,structure,k:5,querySourceText:"do not approve the request")`. Поскольку `TranslationMemorySourceLaw` поддерживает только copular clauses, здесь `Unknown`, **не contradiction**. У обычного полного signature source threshold равен 0; при более широком prefix совпадение `not` исключено из overlap. Проверить фактический returned source/target и сравнить со скриптовым предсказанием.
- **Контроли:** те же кандидаты в обратном порядке (известная N16 FIFO проблематика), `not` и `no`, без отрицания, юридически несовместимые domain/law tags, `unknown` polarity. Отдельный status `SIGNATURE_COLLISION` для случаев, в которых реальный tokenizer НЕ формирует одинаковые сигнатуры; такие строки нельзя тихо перебирать до получения «хорошего» результата.
- Это тест локальной маршрутизации, **не** оценка способности Student изучить отрицание.

**L1 genuinely heldout quality:** создать 12–20 source-disjoint пар коротких директив из ≥3 доменных семейств; в каждой семье противоположные source→target варианты обучены заранее как данные, не hard-coded в parser; 1/2 случаев с `not/no`, 1/2 neutral. Никаких target/gold в query metadata. Независимый эксперт утверждает gold и допустимый abstain **до** прогона. Каждый query вызвать в:
 A. `TranslationKnnStore` current;
 B. тестовом wrapper-кандидат-фильтре, отличающемся **только** проверкой полярности;
 C. **обычном trained adapter** со всеми exact→phrase→word→KNN и verifier (shadow trace; могут выиграть другие механизмы).
Хранить `input_sha`, `signature(full/prefix/structure)`, `memory_id`, `selected_id`, `candidate_source`, `candidate_target`, `similarity`, `law_status`, `polarity_status`, `actual_adapter_id`, `verifier_verdict`, `raw_translation`, `gold_verdict`, `time`.

**L2 real learning and restore:** сравнить **нет обучения / add chosen training / label-permuted chosen training**, но тот же filter и heldout; обучение признаётся реальным только если зависимость предсказаний от учительских labels доказана, а улучшение сохраняется после `ExportCheckpoint→LoadCheckpointAsync` в новом процессе и имеет ordinary caller trace. Переключение постоянного regex из кода без learned memory — **не доказательство обучения**.

**Основные KPI:** lawful correct / total, false acceptance of contradictory candidate, correct abstain rate and coverage, neutral no-regression, baseline→filtered paired win/loss, margin gain after new lessons, cold-restore parity, cost under local PC budget. Если `exact`/word path поглощает все вопросы и KNN never invoked, этот конкретный KNN hypothesis НЕ может объяснять падение quality конкретного product route.

**Заранее определённый исследовательский сигнал:** L1 ≥12 независимых задач, хотя бы 3 matched wrong→correct и ≤1 correct→wrong, false-accept не возрастает, real KNN selected хотя бы для нескольких кейсов; **это повод на больший dataset, не production ACCEPT**. Если фильтр вызывает массовые abstain или худший coverage — не продвигать.

## Как записывать результаты

Не писать в защищённый core planning; артефакты теста `possibleDifs/results/n27-<run-id>/` либо локально в workspace и затем только проверенные receipts:
- `manifest.json`: pinned core/planning SHA, exact probe commit SHA, hypothesis/version, split by whole source file, train/heldout counts, random seeds, tokenizer/hash and actual caller.
- `per_case.jsonl`: anonymized source hash, variant, learned label, prediction, gold/law rubric, nearest/example identity, reject/abstain, latency.
- `summary.json`: exact observed counts, synthetic-vs-native distinction, matched wins/losses, training and restore effect, memory/time/cost. Не подгонять тест под обучение.
- `review.md`: отдельная экспертиза CODEX/architect/quality, ответ human-readable: что подтвердилось, что нет, какие альтернативы и почему.

## Короткое задание исполнителю — готово к копированию

> Режим: диагностический, не реализация продукта. Используй `docs/skills/learning-path-audit-SKILL.md`, `code-examination-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `conversation-intent-and-self-reflection-SKILL.md`. Сначала исследуй *реальные исходники и исторические JSON*, сформулируй predeclared hypothesis, проверь source boundaries и actual caller; не интерпретируй E29V2 как настоящий DPO. Реализуй по одному focused native C# probe для N27-A и N27-B, зафиксируй before/after на одних heldout, train×test leakage=0. В существующих source production classes менять нельзя до результата. Не запускать Windows CI, не создавать 500 DPO-пар, не выдумывать внешний teacher, review или model run. По итогам приложи точные source:line, raw per-case results, hash артефактов, неблагоприятные случаи, простое объяснение человеку и verdict CONFIRMED/REFUTED/INCONCLUSIVE. Работа только по явно указанным scratch и `xiigen-planning/.../possibleDifs`; `llm_mvp_core/docs/plans/replanning-possible-diffs` строго read-only.

**REVIEW / STOP:** нет независимого review, native evidence или ordinary path gain → не объявлять причину RP01 установленной. Любая связь с direct Student требует отдельного RAG graph: `RP01 data→DpoExactPairMaterial→LoRA→TextGenerator` и фактического вызова конкретного Unit.
