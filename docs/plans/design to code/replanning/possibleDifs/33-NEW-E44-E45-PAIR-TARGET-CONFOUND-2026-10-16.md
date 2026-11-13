# N33 — новая source-confirmed гипотеза: E44v2/E45v2 сравнивали разные учебные переходы, а не только weight/curriculum

<!-- XIIGEN-AUDIT-2026-11-11-INSERT -->
<!-- AUDIT-2026-11-11-EV-P48:START -->
## Уточнение статуса P48 — 11 ноября 2026

Исторический target-confound E44/E45 ниже сохраняется. На проверенном core `009bf226174441fa0fbf0336e74ad2f2b7174d92` pre-step backward и вычисляемый finite gate P48 уже исправлены; фразу сверки30.10 «сначала нужен их общий ремонт» нельзя использовать как текущий список открытых работ. Свежий native PASS данным аудитом не получен.

Не объединять последующие попытки: weighted A/B attempt2 дали12/16 при0/16 различий actual; attempt3 измерял equal-weight arm и repeated-row mechanism control, по12/16 при6/16 различий actual. Равный score не удостоверяет равенство ответов, а repeated-row контроль не реализует gradient weight10. При передаче N33 требовать связку claim→attempt→factor→actual rows и источник текущей closure; старый NaN receipt сохраняется. Подробный договор и границы — [адресное дополнение11.11](2026-11-11-P48-P58-EXPERIMENT-MEASUREMENT-ADDENDUM.md). Нового Train это уточнение не назначает.
<!-- AUDIT-2026-11-11-EV-P48:END -->


## Сверка P46/P48/P49 — 30.10.2026

Исторический отрицательный weighted outcome и доказательство его причины остаются разными утверждениями. Target-topology confound, разобранный ниже, нельзя опустить при причинной ссылке на E44/E45. Это не отменяет действующее нормативное правилоD09 и не доказывает безопасность усиления веса.

P48 дал NaN в обоих плечах при2400updates; время первого NaN не сохранено. На3 больше неконечных весов в финальномB не означает «быстрее разрушилась». В текущем ProbeNN численно подтверждены два чтения уже обновлённых весов в backward, для W1 и embedding; сначала нужен их общий ремонт. P49 повторяет все4model-input/target pairs EXAM в TRAIN, поэтому его6/16→11/16 ограничено удержанием известных отображений и одной точкой бюджета. Адресные checks сохранены в карточках и договоре39 core; bridge30.10 связывает версии. Нового Train это уточнение не назначает.

**Дата:** 2026-10-16. **Статус:** `SOURCE_CONFIRMED_MECHANISM / SYNTHETIC_PROXY_EXECUTED / NATIVE_LEARNING_NOT_RUN / PRODUCT_CAUSALITY_NOT_PROVEN / INDEPENDENT_REVIEW_NOT_RUN`. Данный файл — исследовательская дельта к уже существующему [реестру N32](32-INCREMENTAL-NOVELTY-REGISTER-AND-DECISION-ORDER-2026-10-16.md) и [истории Matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), а не ещё один полный аудит DPO. Текущий проверенный code ref: [llm_mvp_core@32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7](https://github.com/lubochka/llm_mvp_core/tree/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7). Planning: `llm_plan3`. **Core и особенно `docs/plans/replanning-possible-diffs` не изменяются.**

## Короткий результат

В исторических E44v2 («security ×10») и E45v2 («curriculum easy→hard») baseline и enhanced **не обучались на одинаковом множестве исходных supervised transitions**. Baseline перемешивает *отдельные предикаты* и только затем создаёт целевые пары `token[i]→token[i+1]`; enhanced создаёт их на иных последовательностях. В E44v2 даже `StratifiedSplit` сначала переставляет отдельные предикаты, а не последовательности/документы. Поэтому их отрицательные результаты действительно зарегистрированы, но гипотеза «их вызвал исключительно ×10 / неправильный curriculum» **не идентифицирована из этих опытов**.

Это НЕ утверждение, что ×10 хорошо, что E44/D-09 нужно отменить, что реальный RP01 DPO/LoRA имеет ту же ошибку или что KNN обязательно выиграет. Правильное утверждение: **старый experiment не отделил воздействие метода от изменения задачи предсказания**.

## Primary source evidence: E44v2 — target topology разошлась до TrainStep

| Звено | Проверенные строки исходника | Наблюдаемый по source эффект |
| --- | --- | --- |
| Входной corpus | [E44v2 L24–36](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs#L24-L36) | `ExtractPreds` собирает predicate stream, затем `StratifiedSplit.Split(genPreds, p => p, ...)` и аналогично security. Группируется **отдельный predicate ID**, не исходный файл/sequence. |
| Разрушение adjacency | [StratifiedSplit L20–35](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/Infrastructure/StratifiedSplit.cs#L20-L35) | `GroupBy(keySelector)`; shuffling внутри групп, затем shuffle общего `train` и `val`. Возвращённые списки уже не исходные токенные предложения/AST-потоки. |
| Baseline training | [E44v2 L38–48](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs#L38-L48) | Новая перетасовка `genTrain.Concat(secTrain)`, затем adjacency по общему `mixed`: появляются cross-domain и pseudo-sequence targets. |
| Enhanced training | [E44v2 L50–65](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs#L50-L65) | Только пары внутри текущих `genTrain` и `secTrain`, другие targets и кратность security ×10; отличие **двух факторов одновременно**, не только weight. |
| Метрика | [ProbeNeuralNetwork L112–120](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/Core/ProbeNeuralNetwork.cs#L112-L120) | `Evaluate(tokens)` измеряет `Predict(tokens[i]) == tokens[i+1]`; E44v2 передаёт уже shuffled `genVal/secVal`, поэтому exam-target также synthetic. |

**Отрицательный исторический receipt сохранился:** [E44v2 2026-05-01 Tier2](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E44V2-DATASET-WEIGHTER/results/run-2026-05-01-001.json) даёт baseline 0.1663 → enhanced 0.0039, −97.67%, `genPreds=6`, `secPreds=875`. Это **отрицательный результат именно описанного runner**, но не строгая оценка weight ×10 на сохранённых истинных source transitions. Уже только imbalance 6:875 требует отдельно оценить классы и допустимые веса; он не снимает target-confound.

## Primary source evidence: E45v2 — effect of curriculum не изолирован

- [E45v2 L27–35](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/Experiments/E45CurriculumOrganizerV2.cs#L27-L35): easy/hard получают из **положения файлов**; поток исходных predicates плоский и также может соединять границы исходных файлов.
- [E45v2 L37–46](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/Experiments/E45CurriculumOrganizerV2.cs#L37-L46): baseline сначала shuffle `easyPreds.Concat(hardTrain)`, затем `TrainStep(mixed[i], mixed[i+1])`.
- [E45v2 L50–62](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/Experiments/E45CurriculumOrganizerV2.cs#L50-L62): enhanced обходит `easyPreds` и `hardTrain` отдельно, сохраняя их внутреннюю adjacency. Получились иная задача, иной порядок и разное количество/состав boundary transitions.
- [E45v2 2026-05-01 Tier2 receipt](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E45V2-CURRICULUM-ORGANIZER/results/run-2026-05-01-001.json): final_hard_accuracy 0.3623 → 0.3478 (−4%). Исторический провал нельзя интерпретировать как доказательство вреда **порядка** без control с одинаковыми pairs. Здесь validation `hardVal` берётся хвостом, **не** через `StratifiedSplit`; не переносить на E45 конкретный E44 validation-shuffle defect.

## Почему это важно для прежних «неудачных» результатов и опыта Matrix→KNN

На уровне источника правильный объект обучения — `(source predicate, next predicate)`, то есть **направленное отношение**, возможно обусловленное AST-context / source file / language. В E44/E45 baseline создаёт часть отношений *после случайной перестановки элементов*. Пример: два исходных документа `[A,B,C]` и `[X,Y,Z]` должны породить `A→B, B→C, X→Y, Y→Z`. Shuffle отдельных элементов может добавить `C→Y`, а внутри документа удалить `A→B`. При переносе weighted/curriculum logic в обучающийся Unit/KNN нужно переносить **признаки и веса для настоящего ребра**, а не synthetic shuffling.

Это новая **адресная** причина не делать поспешный вывод «weight/curriculum в матрице failed → выбросить механизм». Она связана с ранее выявленным E09 cross-file seam, но **другой конкретный механизм**: baseline/enhanced теряют comparability даже если полностью убрать cross-file соединения из input. Как и ранее найденный успешный E09, результат не доказывает переносимость на любой домен.

## Sibling branch readback и происхождение вывода

| Ref | Проверенные code blobs/receipts | Вывод |
| --- | --- | --- |
| `codex/dpo-implementation` | E44v2 `241702f8...`, E45v2 `daefff53...`, StratifiedSplit `fc3a657a...`, ProbeNN `a44f2909...` | Target-confound присутствует в текущем read-only коде. |
| `language_translate` | E44v2 `f3870918...`, E45v2 `e161d5a6...` | В более ранней ветке сохранены аналогичные baseline/enhanced маршруты с mixed-token pair creation. |
| `DesignToCode` | Те же E44v2/E45v2 SHA `f3870918...` / `e161d5a6...` | Это не только новое изменение текущей ветки; механизм есть в sibling source. |
| All three branches | E44v2 historical result blob `9e5a0682...`; E45v2 `22d16c11...` | Те же опубликованные результаты; три ветки **не означают три независимых запуска**. |

## Не исправлять научную историю задним числом: D-09 / D-08

[Skill experiment-design](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/docs/skills/experiment-design-SKILL.md) прямо утверждает D-09 («constant ×10 caused catastrophic forgetting in E44v2») и D-08 («StratifiedSplit for all experiments»). **D-09 сейчас может быть полезной практической политикой, но это обоснование через E44v2 научно не идентифицировано.** D-08 должна стратифицировать **целые независимые sequence/document families либо уже готовые supervised examples**, не individual tokens перед образованием связей. Это не разрешение прямо редактировать protected skill или снимать admission/quality gates; нужен отдельный ownership/review на основе matched experiment.

## Доменные ограничения и статус обнаружения

- **C#/AST, SQL/DSL, workflows / dynamic graph:** особенно важно сохранять typed edges, source file, order, operator/polarity; случайно образованные pseudo-edges могут порождать ложные переходы. В графах проверять и семантику, и version/identity.
- **EN↔RU:** сохранять phrase/lemma/role/negation/word order *в зависимости от задачи*, а не универсально заменять sentence-level обучающие пары next-predicate. E44/E45 работают с code predicates, не являются прямым опытом переводческого LoRA.
- **Независимые bag-of-words классификации:** при исходно независимых строках перемешивание записей допустимо; найденный дефект относится к **созданию target из adjacency после shuffle**, а не к любому shuffle.
- **RP01 Student:** отдельный exact DPO train path; не найдено доказательства, что именно этот E44/E45 helper вызывается из RP01. Гипотеза релевантна **археологии выводов и дизайну будущего учебного pipeline**, не proven root cause C#500 0/3.

**Evidence taxonomy:** SRC_CONFIRMED = unequal pair-generating operators, val shuffling in E44, published receipts. PROXY_ONLY = маленький независимый toy с искусственными последовательностями. HYPOTHESIS = насколько это искажало E44/E45 metrics; может ли корректный метод улучшить обучение современного neural/Unit. NOT_RUN = native .NET, learned product, cold restore, независимый CODEX/Astra и external teacher.

**Следующая проверка:** [N33 однофакторный пилот](33-MICRO-E44-E45-FACTORIAL-QUALITY-PILOT-2026-10-16.md). Запрещено повышать outcome до «исправлено» без парного native-run и независимой оценки качества.