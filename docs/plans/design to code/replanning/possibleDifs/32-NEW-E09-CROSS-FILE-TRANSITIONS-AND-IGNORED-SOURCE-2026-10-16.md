# N32-B — E09 Structural KNN: ложная соседность на стыке файлов и фактический владелец входного кода

<!-- POST117-CURRENT-2026-11-03:START -->
## После P116/P117: общее правило границы усилено, E09 не объявлен исполненным

[текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) подтверждает удаление 3 ложных переходов в P116, копии счётного E43. Это не native E09/Roslyn: ни его kindMap, ни caller с csharpSource, ни его quality panel опытомP116 не проверены. Историческое L0 ниже остаётся source-aligned proxy.

Переносимый урок относится к проверке происхождения обоих концов. Нужны trusted project+file, позиция, actual token и hash исходного stream; одинаковое количество и множество значений не удостоверяют соседство. **Плохой случай:** project одного конца изменён, file labels/count/tokens сохранены, проверяющий принимает. **Хороший контроль:** такой журнал отвергается, честные внутренние пары принимаются, JSON неправильного вида/duplicate file ID/короткий hash дают typed отказ. P116 проверяет file lineage, но не trusted project lineage.

Любая диагностическая память, сравнение до replay и упавший физический запуск учитываются как фактическая работа. E09 consumer должен читать те же files, для которых доказаны пары; неизменность отдельного csharpSource проверяется только в отдельно назначенном native срезе. Ниже исходная редакция 16.10, не команда повторить PRIMARY.
<!-- POST117-CURRENT-2026-11-03:END -->


**Дата:** 2026-10-16. **Статус:** `CODE_CONFIRMED_MECHANISM / SYNTHETIC_PROXY_EXECUTED / NATIVE_E09_NOT_RUN / HISTORICAL_CAUSALITY_NOT_PROVEN`. Исходный код `llm_mvp_core/codex/dpo-implementation@79082340f01b744cdf08185616f2bd818dbd06b8`, проверены соответствующие тела в `DesignToCode` и `language_translate`. Никаких правок `llm_mvp_core`.

## Относительно положительного исторического результата

[N12 Matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) уже подтвердил, что E07 «неудачно обучал AST» с неверным BPE→char alignment/игнорированием заявленных измерений, а E09 впервые строит Roslyn structural features напрямую в KNN. [K09 provenance](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md) сохраняет ранний малый **положительный**, но не прошедший threshold результат, и следующий сохранённый **PASSED** при большем корпусе. Но E09 baseline vs KNN там сравнивались на разных validation subsets; локальный per-file `kindMap` тоже уже описан. **Не повторяем эти два findings как новые.**

### N32-B1 — extra hypothesis: `train.Zip(train.Skip(1))` обучает несуществующие межфайловые переходы

1. [`E09StructuralKnn.BuildTokenVectors` L102–125](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L102-L125): для каждого файла `CSharpSyntaxTree.ParseText(text)` и `DescendantNodes()`; затем `result.Add` в **единый** список `TokenWithVector`, без `FileId` на самом record и без sentinel на границе.
2. [`Run` L25–36, L62–76](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L25-L76): отрезает `train = tokenVectors.Take(80%)`; `trainingPairs = train.Zip(train.Skip(1))` и метка — `p.Second.BpeId`, даже если `p.First` был последним узлом файла A, а `p.Second` первым узлом файла B. Аналогично для bigram baseline и validation transition target.
3. **Смысл:** модель учит формальный переход между двумя отдельными C# файлами по позиции в перечне `files`; этот переход не является структурным отношением внутри AST. Сама AST-форма могла быть полезной, но тренировочная цель содержит межфайловый «шум», и прежние FAILED/PASSED могли зависеть от порядка файлов.

**Исполненный только синтетический L0:** три файла по четыре записи: A1..A4, B1..B4, C1..C4. Точный `floor(.8*12)=9`, старый zip создаёт 8 пар, из них **2 ложные межфайловые: A4→B1 и B4→C1 (25%)**; file-aware zip оставляет 6. Это **арифметика source-aligned proxy, НЕ исполнение C# E09, НЕ оценка top1 accuracy**. Наличие столь же большой доли в историческом dataset НЕ проверено. Запуск и JSON: [MICRO-N32 protocol](32-MICRO-ONE-FACTOR-LEARNING-CONTRAST-AND-E09-2026-10-16.md).

### N32-B2 — independent source-ownership seam: `csharpSource` не участвует в расчёте E09

[`E09StructuralKnn.Run` L18–31](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L18-L31) проверяет только `!IsNullOrWhiteSpace(csharpSource)`, затем вызывает `BuildTokenVectors(files)`. [`BuildTokenVectors` L102–125](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L102-L125) читает только `files[fileId].Text`; отдельный `csharpSource` в остальных строках `Run` не используется. **Факт:** при постоянном `files` изменение *любого непустого* `csharpSource` не меняет обучающие structural vectors, labels и метрики E09 (но способно изменить интерпретацию того, на чём якобы учились). Это **может быть корректным API дизайном**, если `files` всегда canonical, но требует source-to-consumer evidence.

**Граница причинности:** оба свойства относятся к **историческому E09 AST-KNN**, не к текущему RP01 exact-v2 Student (там другой model/feature/consumer). Нельзя утверждать, что нашли источник его 0/3/0/4. Нельзя объявлять исторический положительный результат ложным: вмешательство ещё не исполнялось на том же data panel. Прикреплённый old archive остаётся неизменным.

### Быстрый целевой A/B

- **Механика E09:** фиксировать те же `files`, `kindMap`, k, AST tokenizer, порядок, 80% split; A = существующий train zip; B = тот же набор кандидатов минус пары, пересекающие `fileId`. В обеих ветках **evaluation только на одном и том же** subset внутрифайловых val transitions; метрики paired top1, per-file coverage. НЕ менять одновременно per-file local kind IDs (это отдельный известный gap) и file split.
- **Отдельный source-invariance фальсификатор:** два непустых `csharpSource`, одни и те же `files`: ожидаются одинаковые labels, predictions, accuracy (не wall time). Если отличаются — пересмотреть actual native caller/path и вывод.
- **Новый consumer:** лишь после позитивного E09 механического и paired результата тестировать selective addition AST boundary-safe features в реально вызываемый C# KNN/Flow на независимом source-family split; error law correctness, rejected wrong answers и latency. Ни E09 top1, ни математика synthetic сами по себе не доказывают полезного обучения всей платформы.

**Проверка sibling branches:** `language_translate` и `DesignToCode` содержат те же существенные `Run` L18–76 (после сдвига строк лицензией); E26 contextual POS и E13B memory также присутствуют в этих ветках. Это подтверждает исторические алгоритмы, но **не** независимый семантический PASSED будущей архитектуры. Old→new donor commit конкретной matrix→KNN функции по этому наблюдению не найден, **не выдумывается**.
