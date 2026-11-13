# MICRO-N33 — короткий эксперимент: вес и curriculum отдельно от изменения самих пар

**16.10.2026. Статус: PROTOCOL_READY / TOY_JS_PROXY_EXECUTED / NATIVE_DOTNET_NOT_RUN / NO_PRODUCT_QUALITY_CLAIM / INDEPENDENT_REVIEW_NOT_RUN.**

Предметная гипотеза и доказательства: [33-NEW-E44-E45-PAIR-TARGET-CONFOUND](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md). Обязательное правило: это **не** ещё одна кампания «500 пар», не новый CI/Windows gate и не переписывание старых run JSON. На первом шаге **только ProbeNeuralNetwork из E44/E45, выбранные наборы C# predicates и тот же измеритель на всех arms**. Никакого Qwen/teacher, пока он не понадобится для отдельного продукта DPO; этот опыт не создаёт DPO-пары.

## 1. Фальсифицируемые гипотезы и назначение

- **N33.H1 (target-confound):** разница E44v2 baseline/enhanced объясняется не только weighting, но и тем, что смешивание отдельных tokens перед `TrainStep(a,b)` меняет распределение истинных target-переходов. Falsifier: построить source-preserving pairs, сравнить источники/мультисеты и показать, что legacy и исправленные наборы уже идентичны либо что исправление никак не меняет native heldout outcomes.
- **N33.H2 (curriculum-confound):** E45v2 сравнивает «randomized-token synthetic task» с «ordered true-ish task», не clean random-pairs versus ordered-same-pairs. Falsifier: на одном frozen pair multiset, равном количестве шагов и одинаковой инициализации `random pairs` и `easy→hard pairs` дают тот же значимый effect, что наблюдался в старом эксперименте. Отрицательное либо нулевое значение **возможны**.
- **N33.H3 (domain conditional):** порядок source predicate важен для C#/workflow relation tasks, но у bag-based classification, где label задан отдельно, random ordering of independent training examples допустим. **Не переносить эффект на ENRU/Qwen без новой проверки.**
- **N33.H4 (validation-confound):** E44v2 `StratifiedSplit` перемешивает val-предикаты до `Evaluate`, искусственно создавая следующее слово. Falsifier: один и тот же checkpoint получает одинаковое множество valid source transition labels до и после возвращения source-document boundaries. Если разные — источник метрики неэквивалентен.

## 2. Данные и immutable holder

**Минимальный native набор:** 12–24 небольших C# source-файла из разрешённого code corpus; желательно 6–12 независимых source-families (например null-check, throw, return/assignment), по 2–3 файла на family, причём в нём действительно есть ожидаемые **однозначные локальные** predicate transitions. Нельзя выбирать набор по будущему выигрышу. Дополнительный toy набор без C#:

- документ D1 `[1,2,3,1,2,3]`; D2 `[4,5,6,4,5,6]`;
- истинные пары только adjacency **внутри документа**: `(1,2),(2,3),(3,1),(4,5),(5,6),(6,4)` (по числу появлений);
- **отрицательный контроль:** создать перемешанный единый поток предикатов `[1,4,2,5,3,6]` и ошибочно тренировать `(1,4),(4,2),...`; такие ребра никогда не встречались в исходных документах;
- границу T/V и source-family определить **до** extraction; одинаковые семантические семьи не должны течь из train в blind heldout. Сохранить IDs, SHA256 source family и точную версию `LogicExtractor`. При крайне малом числе семей отдельно отметить impossible family-disjoint и не объявлять generalization.

**Алгоритм формирования validPairs:** для каждого `file`: `preds = LogicExtractor.Extract(file.Text)`; `pairs += Zip(preds, preds.Skip(1))` **только внутри одного файла**. Не применять `StratifiedSplit` к individual tokens до построения пар. Domain/general/security metadata — у пары, а не у позиции токена в искусственно смешанном списке. Разрешено shuffle **готовых пар**, не source sequence до получения target. Сохранить количество пар, количество source files, top class frequencies, cross-file edges, inter-arm pair multiset hashes.

## 3. Native E44 четыре arms: same tasks, different sampling weights

Все arms используют одинаковые `validPairs`, один immutable heldout, одинаковый `ProbeNeuralNetwork(22,32,0.02,seed)`, same optimizer steps/epochs, none of heldout rows trained. Отличается **только распределение выбора пар для updates**; не менять model architecture/decoder, данные и число градиентных шагов.

| Arm | Интервенция | Что устанавливает |
| --- | --- | --- |
| **W0** | uniform sampling готовых пар, `N` TrainStep | baseline при истинных labels |
| **W1** | weighted sampling с динамическим `max_weight` (policy D-09), те же `N` TrainStep | есть ли польза политического динамического cap |
| **W2** | security relative weight 10 в sampling при тех же `N` steps, а не дополнительные шаги | изолированный эффект ×10 **только исследовательский**, не production-решение |
| **Wlegacy** | исторический способ E44 baseline/enhanced на старом потоке, отдельно labelled invalid-label comparator | sanity-check происхождения отрицательной исторической метрики; **не** заменяет W0 |

Сбалансировать feasible мини-кейс для понимания механизма и отдельно повторить **реальный дисбаланс** (исторически genPreds=6, secPreds=875) с pinned corpus, не подменяя его идеальной balance. Если нет достаточных независимых general families, пометить результат `DATA_INCONCLUSIVE`, не делать вывод о весах. Обучение по паре с source не менять: одинаковый `(input,target)` при любой доле weight. Все arms минимум 3 deterministic seeds для проверки разброса; **сначала одна seed smoke**.

## 4. Native E45 два arms: order only

Подготовить единый multiset `validPairs`, с зафиксированными числом easy/hard, количеством показов каждого ID и эпох. Использовать те же seed/weights/budget/checkpoint initial state:

- **C0:** shuffle порядка **готовых пар** каждый epoch, TrainStep(pair.Input,pair.Target), без удаления и создания pair.
- **C1:** те же exact pairs и кратности, но easy→hard (в обоих случаях заранее выбранный единый набор).
- **Clegacy:** оригинальные `shuffle individual predicates→new adjacency pairs` только для provenance, не attribution to curriculum.

Требовать checksum `sorted(pairId,source,target,count)` C0 == C1; если не совпадает, эксперимент **INVALID_CONTRAST**, даже при красивом gain. Не выбирать easy/hard простой позицией source files, если возможно измерить task difficulty независимо. `hardVal` должен быть дискретным набором true pairs из непересекающихся source families, не хвостом произвольно сформированного stream.

## 5. Как измерить **качество обучения**, а не только выполнение теста

- **Frozen before/after:** обязательный `B0` baseline, same initialization, exact pair paths, `TrainStep` receipts, separate heldout scores на уровне `(source,target)`. Для ProbeNN можно вызвать `Evaluate(new[]{source,target})` на отдельной паре и агрегировать, но **никогда** на shuffled concatenated validation tokens.
- **Primary:** `paired_heldout_accuracy` по одинаковым genuine source-family-disjoint pairs; вторичные `general_accuracy`, `security_accuracy`, `balanced_accuracy`, `training_transition_coverage`, `regression_old_domain`, `training_steps`, `p50/p95 time`. Отдельно сравнить recall true target edges и fake-edge rate.
- **Controls:** unchanged source, random-label negative control, no-training control, exact pair inventory hashes, 3 seeds, confidence intervals/покомпонентная таблица; для `n<20` честно писать `UNDERPOWERED`. Не принимать чистый loss improvement за learned skill.
- **Decision:** `IMPROVE` только если выигрывает на **тех же истинных heldout labels**, без заранее недопустимого роста critical errors и с повторяемостью по seeds; иначе `NO_GAIN/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE`. Не переписывать original E44/E45 receipts. Если W2 остаётся хуже W0/W1 на valid labels, вред ×10 подтверждается новым качественным опытом. Если C1 хуже C0, curriculum genuinely harms в данной предметной группе; оба вывода domain-scoped.
- **Consumer boundary:** даже улучшение ProbeNN **не доказывает** рост качества RP01 production Student, KNN или Unit. Путь в реальный learned consumer требует следующего адресного опыта: train→state hash→save/cold reopen→ordinary output before/after→blind legal/semantic examiner. В RP01 для DPO/LoRA всё ещё отдельно стоит проверка randomly initialized B0/SFT, mask/gradient/LoRA/decoder.

## 6. Уже выполненный очень маленький synthetic proxy (не native)

2026-10-16 выполнен независимый Node toy-скрипт N33 с Xorshift32; **его PRNG не эквивалентен .NET Random(42)**, используется bigram-count lookup, **не** ProbeNN. На двух подклассах последовательностей 1..6: genuine train transitions = 26; baseline transitions after shuffled individual tokens = 29, enhanced unweighted = 28; overlap distinct legacy baseline / true = 3, enhanced / true = 5. Toy heldout (10 пар): valid x1 и valid x10 = 10/10, legacy baseline 0/10, legacy enhanced x10 = 3/10. Это **демонстрация логической возможности разрушения target**, а не оценка E44v2 и тем более не измеренный прирост продукта. Result/scripts доступны в артефактах сессии; исходный C# authoritative evidence — ссылки в N33.

## 7. Короткий work order исполнителю и критерии завершения

**CODEX (requested astra 6 ultracode), роль: experiment investigator/executor, при отдельном разрешении запуска native:** восстановить E44v2/E45v2 из текущего pinned source, создать bounded native diagnostics **не в protected core-docs**; сначала распечатать исходные пары и counts, доказать `same pair multiset` для контролируемых arms, затем 3 seeds и 2 split strategies. Отдельный human-role CODEX/Astra судья оценивает реальное качество без просмотра arm names, когда будет actual output. В этой документной сессии такие агентские вызовы **НЕ** выполнены.

Перед результатом предъявить: source refs + SHA, frozen parameters, exact pair list hash, source family split, specified control, case-by-case before/after, absolute paired accuracy (не один %), false-edge count, counterexample, decision status, beneficiary domain, training/eval time и reason why no result can be generalized to RP01 unless its consumer path was actually measured.

**Срок модели ограничить работой алгоритма, не CI:** не запускать Windows GitHub Actions, unrelated coverage gates, не переписывать frozen dataset, не запускать external paid teacher. При stop/fail — указать конкретное место, а не бесконечный repair loop.

**Ревью:** авторская self-check выполнена на уровне логики документа; независимое `plan-review` или `code-review` агентом НЕ выполнялось, native результат **NOT_RUN**.

## Усиление причинного сравнения и цены — 12 ноября 2026

Основание — [пересчёт P48/P49](https://github.com/lubochka/llm_mvp_core/blob/5afb9d265fb62db8c9d4ea00f61e5f2ed9e9fc3a/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/codex-experiment-review.md). Результат toy proxy и native NOT_RUN выше остаются историческими. Новое исполнение не назначено.

До smoke выпустить точный ledger для каждой пары seed × split × arm: source-family IDs, exact pair multiset с кратностями, initial-state hash, расписание выбора пар и ожидаемые TrainStep/readout calls. В W0/W1/W2 одинаковая исходная задача и бюджет N сохраняются, но realised sample counts закономерно различаются: записать их отдельно. В C0/C1 совпадают также кратности предъявления каждого pair; отличается порядок. Wlegacy/Clegacy — диагностические искажённые задачи, их score не входит в причинную оценку weights/curriculum.

Шаг numeric oracle тоже является TrainStep, если он меняет настоящий Probe. У нынешнего P48 этот один шаг дал полную цену 4801 вместо 4800; потому ledger N33 включает setup/oracle/controls/arms/readout отдельно. Three seeds × two splits не является достаточной записью бюджета, пока неизвестны N и число реально исполняемых arms. После smoke с неисправной fixture не переходить к остальным seeds автоматически; сохранить ошибку и исходный attempt.

При сравнении результата использовать парные RowId и показать gain/loss, даже если суммарная accuracy одинакова. Отдельно вычислить constant-answer ceiling и input-function ceiling для каждой заявленной панели/домена. Возможна ситуация, когда вход не различает конфликтующие targets: тогда высокая граница по input объясняет потолок задачи, а не победу weighting. Source-family-disjoint остаётся обязательным условием заявления обобщения; перестановка label/ID его не создаёт.

В проверку reporter включить три контрпримера без обучения: разные outputs с равным score; подмена одного RowId дублем при прежнем размере; знакомый input с новым target. Все должны менять соответствующий диагностический вывод, не gold. Внутри rate числитель и знаменатель относятся к одному arm и одному окну. В A/B времени и памяти сравниваются разные arms, но при одинаковых границах caller/setup/warmup/счётчика и с отдельными сырыми величинами; warm preparation не сравнивается с cold main как будто общей цены подготовки не было.

Окончательный verdict зависит от полноты panel, validity guard и terminal внешнего runner. Наличие успешных arm outputs до последующего assertion failure не даёт whole-run PASS. Поправленная сводка старого запуска сохраняется отдельно с input hashes; raw и исходный causal factor остаются неизменными.
