# 05. Новые контрасты: нормализация морфологии и повторный рост learning rate

Здесь две новые конкретные гипотезы внутри уже известных направлений P28 и P12/P119. **Подтверждены выражения исходного кода и воспроизведены их математические последствия; влияние на качество обучения ещё не измерено.** Обе имеют узкую применимость: LT-N05 — при переносе исторической дополнительной координаты в реальный поиск; LT-N06 — при превышении горизонта supervised scheduler. Они не объявляются установленными причинами нулевого качества обычного RP01 DPO.

Небольшая воспроизводимая проверка: [source-formula-probes.py](evidence/source-formula-probes.py), [сохранённые результаты](evidence/source-formula-results.json). Скрипт выполнен успешно в этой сессии. Он использует Python binary64 и моделирует формулы; **C# runtime, stemmer, KNN consumer, optimizer и обучение не запускались**. В JSON явно записаны `native_csharp_executions=0`, `optimizer_updates=0`, `external_model_calls=0`, `quality_evaluation_performed=false`.

## LT-N05. Неродственное пополнение словаря меняет ранжирование старых соседей

### Механизм и версия

В `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` `MorphFamilyIndex` вычисляет `_maxFamilySize` по всем корням в vocabulary. `GetNormalizedFamilySize(word)` возвращает размер собственного семейства, делённый на этот глобальный максимум. `MorphDimensionAppender.Append` копирует исходный embedding без изменения и добавляет этот скаляр последней координатой. Источники: [построение maximum, строки 49–58](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/01.Tokenizer/MorphFamilyIndex.cs#L49-L58), [нормализованный размер, строки 81–86](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/01.Tokenizer/MorphFamilyIndex.cs#L81-L86), [Append, строки 18–29](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/06.VectorSearch/MorphDimensionAppender.cs#L18-L29).

**До вмешательства:** для неизменной пары query/candidate используется `z(w) = [embedding(w), familySize(w)/M]`. **После пополнения совсем другого семейства:** M увеличивается, и последняя координата уменьшается у всех старых слов, хотя их собственные families, embeddings и набор кандидатов не изменились. Тем самым меняется относительный вес morphology против base similarity. Полный rebuild индекса устраняет смешение версий query/index, но не устраняет это изменение метрики.

Точные файлы отсутствуют в просмотренном current `01.Tokenizer`/`06.VectorSearch`; найденные исторические прямые callers appender — тестовые. Положительный end-to-end learning receipt именно этого appender не найден. Поэтому статус — **новый риск при восстановлении опционального donor механизма**, а не доказанный потерянный источник успеха и не текущий RP01 hot-path defect. Отсутствие файла с таким именем само по себе также не доказывает отсутствие всех аналогичных функций в большом репозитории.

Уже известное замечание «размер семейства не кодирует идентичность корня» здесь не выдаётся за новое: оно относится к [P28 morphology family](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P28-morphology-family.md). Новизна — конкретный контрфактический опыт **unrelated vocabulary growth при фиксированных старых embeddings и candidates**. Поиск этого механизма в двух каталогах не нашёл прежнего описания; границы такого поиска приведены в [06](06-COVERAGE-AND-EXECUTION.md).

### Что уже воспроизведено формулой

Для прозрачной проверки взяты искусственные двумерные unit vectors: query=(1,0); кандидат A под углом 30°, B — под 10°. Размеры собственных семейств неизменны: query=3, A=3, B=1. Сначала M=3, затем третье семейство, которое **не участвует в поиске**, увеличивает M до 6. Обозначение A как того же семейства не делает его автоматически правильным переводом: семантическая разметка в этом расчёте отсутствует.

| Состояние transform | Cosine(query,A) | Cosine(query,B) | Первый сосед |
|---|---:|---:|---|
| Старые query/index, M=3 | 0.933013 | 0.884236 | A |
| Полный rebuild query/index, M=6 | 0.892820 | 0.942376 | B |
| Новый query M=6, старый index M=3 | 0.863950 | 0.977058 | B |

Это доказывает возможность rank flip у данной формулы. Ни stemmer, ни качество перевода, ни ошибочность именно B этим не доказаны. Числа и входы полностью сохранены в [JSON](evidence/source-formula-results.json).

**Отдельный контроль нормы base embedding.** При неизменном vocabulary M=3 удвоение всех исходных vectors сохраняет их обычные cosine distances, но после Append меняет победителя с A на B: значения становятся 0.892820 против 0.942376. Причина та же — незафиксированный относительный вес последней координаты. Это отдельный масштабный контраст внутри LT-N05, а не ещё одна независимая «найденная ошибка». Его практическая применимость зависит от того, нормирует ли конкретный вызывающий код embeddings **до** Append; нормализация целого уже расширенного вектора сама по себе cosine не меняет.

### Быстрый native опыт LT-N05a: vocabulary growth

**Получатель:** KNN similarity/ranking, использующий appender; neural weights не меняются. **Состояние:** точные embeddings, словарь, stems, counts, M, версия transform и индекс. **Домен:** сначала перевод с морфологическими контрастами RU или HE при соответствующем реальном stemmer; EN — отдельный профиль. C# identifiers использовать как отрицательный профиль применимости, а не предполагать полезность русской морфологии для кода.

1. Взять 12–24 размеченных retrieval случая с фиксированными candidates, включая истинных родственников, разные корни одинакового размера и полезного семантического соседа с другой морфологией. До выполнения CODEX фиксирует gold и допустимые ответы.
2. Нативно вызвать donor `MorphFamilyIndex` и `Append` с V0. Затем создать V1, где пополнилось только неродственное семейство. По фактически полученным stems удостовериться, что собственные families test words и base embeddings одинаковы.
3. Плечо A использует нынешнее вычисление M из V1. Плечо B использует прежний зафиксированный scale M0 при тех же V1/candidates; числовой диапазон при новых families задокументировать, clipping в этот опыт не добавлять. Старый V0 нужен как исходный контроль. В обоих основных плечах перестроить весь индекс, чтобы не смешать drift метрики с stale index.
4. Сравнить coordinate values, нормы, top-k IDs, recall/rank gold и неправильные совпадения. Если есть настоящий ordinary consumer, дать ему одни и те же retrieval payloads и сохранить все ответы. Без такого вызова результат остаётся component retrieval, не learning-quality PASS.
5. Отдельно проверить new-query/old-index: несовпадающая версия transform должна быть обнаружена или обработана явной миграцией. Не смешивать этот режим с основным A/B.

**Причинное подтверждение:** только изменение denominator policy объясняет исправление полезного ранга и обычного ответа; выбранные неправильные старые ранги не стали лучше случайно из-за добавленного кандидата или другого embedding. **Опровержение для выбранного пути:** appender не вызывается; M не меняется; ranks/ответы одинаковы; или лучшее совпадение по stem ухудшает смысл. В последнем случае фиксировать отрицательный результат данного профиля, не объявлять morphology бесполезной во всех доменах.

### Быстрый native опыт LT-N05b: норма embeddings

На тех же V0 и candidates сравнить `Append(e,...)` с `Append(2e,...)`; никаких изменений stemmer/vocab. Затем отдельным плечом проверить нормализацию **base** e перед Append с одним явно заданным morphology coefficient. Если upstream уже гарантирует единичную норму, риск этого caller закрыт без расширенного обучения. Если нет — измерить те же ранги и обычные ответы. Фиксацию M и изменение base normalization нельзя одновременно вводить в один A/B и приписывать результат одному из них.

**Предлагаемое исправление для проверки:** версия transform со стабильным scaling, согласованная для записи/query/restore; явно выбранный или обучаемый вес morphology; воспроизводимый rebuild при смене схемы. Не фиксировать произвольное M или coefficient как окончательный универсальный закон до доменного опыта. Отдельное представление root identity — прежняя гипотеза P28 и другая переменная.

## LT-N06. Cosine decay после горизонта снова разгоняет supervised updates

### Конкретная цепочка текущего кода

В `codex/dpo-implementation@a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6` `CosineWithWarmup.GetLr(step)` вычисляет `progress=(step−warmup)/max(1,total−warmup)`, затем `min + 0.5×(1+cos(π×progress))×(max−min)`. Верхней границы progress нет. Следовательно, после minimum при progress=1 LR растёт и при progress=2 возвращается к maximum. Комментарий описывает linear warmup then cosine decay; явного режима warm restarts у класса нет. [CosineWithWarmup, строки 8–35](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/Schedulers/CosineWithWarmup.cs#L8-L35).

Само выражение было и в [старом scheduler](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/04.TrainingLoop/Schedulers/CosineWithWarmup.cs). Новая существенная область анализа — нынешний **вызывающий путь, способный перейти горизонт**:

- `Trainer.TrainSupervisedNextTokenStep` берёт `GetLr(_supervisedStep)`, делает optimizer step и увеличивает `_supervisedStep`; ограничения `step<TotalSteps` в этой функции нет. [Trainer, строки 125–145](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/Trainer.cs#L125-L145).
- Обычный pretraining loop, напротив, ограничен `CompletedSteps < TotalSteps`. Поэтому короткий pretraining не демонстрирует этот эффект. [Trainer, строки 198–207](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/Trainer.cs#L198-L207).
- `IncrementalTrainingSession` создаёт `perCallConfig` с WarmupSteps=0 и новый Trainer/AdamW. [Session, строки 403–407](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L403-L407).
- `IncrementalTrainer` допускает до 100 000 examples и выполняет supervised executor для каждого. Default `TrainingConfig.TotalSteps=10 000`. Это source-level допустимость превышения при соответствующей конфигурации/длине серии, а не доказательство, что такой run уже случился. Связь executor с рассматриваемой функцией проверена: production-конструкторы создают `ProductionSupervisedTrainingStepExecutor`, а его `Execute` вызывает `session.Trainer.TrainSupervisedNextTokenStep(session.Model, session.Optimizer, tokenIds, weight)`. [Production constructor](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L43-L68), [executor](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L19-L31). [Лимит examples](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L23-L28), [цикл](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L645-L671), [defaults](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/TrainingConfig.cs#L13-L21).

**Ключевая граница:** RP01 `DpoTrainer.ApplyOptimizer` использует свой `_learningRate`, не этот scheduler. Для рассмотренного RP01 learning rate=1e-5. LT-N06 нельзя использовать как объяснение исходных 50 DPO updates, если трасса не показывает другой supervised consumer. [DPO ApplyOptimizer](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1219-L1225), [actual RP01 settings](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L160-L170).

Уже известная проблема создания нового AdamW/warmup/counter между вызовами разобрана в [P12 optimizer boundaries](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P12-optimizer-boundaries.md); выбор effective learning dynamics — в [P119](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P119-learning-dynamics.md). LT-N06 отличается: **тот же Trainer и непрерывно растущий step проходят конец расписания, после чего cosine повторяется**. Не требуется reset или потеря moments, чтобы эффект возник.

### Что уже проверено математически

Укороченный горизонт нужен, чтобы native проверка занимала девять updates вместо десятков тысяч. Взяты WarmupSteps=0, TotalSteps=4, MaxLr=0.0003, MinLr=0.00003. Сравнивается текущая формула с единственным изменением: `progress` ограничен интервалом [0,1].

| `_supervisedStep` перед вызовом | Текущая LR | LR с ограничением | Состояние |
|---:|---:|---:|---|
| 0 | 0.00030000 | 0.00030000 | Первый update |
| 2 | 0.00016500 | 0.00016500 | Спад |
| 4 | 0.00003000 | 0.00003000 | Minimum |
| 6 | 0.00016500 | 0.00003000 | Повторный рост |
| 8 | 0.00030000 | 0.00003000 | Снова maximum, в 10 раз выше minimum |

Полные девять строк находятся в [JSON](evidence/source-formula-results.json). Равенство до конца horizon и рост после него проверены assertions скрипта. Native float32 округление может отличаться в младших знаках; показанный эффект большой и не опирается на такие знаки. Полезность постоянного minimum вместо restart этим расчётом не доказана.

### Мини-опыт LT-N06: девять supervised updates на плечо

**Получатель:** настоящий `Trainer.TrainSupervisedNextTokenStep` в supervised/incremental path. **Состояние:** одно исходное сохранённое model/adapter состояние, один tokenizer, одинаковые trainable parameter keys, свежие одинаковые optimizer moments/step, одинаковое начальное RNG/dropout state, последовательность 9 уроков и weights. **Домены:** C# документация либо перевод для данного нейронного recipient; не использовать accuracy дискретной translation memory как метрику scheduler нейронной модели.

1. Сначала записать фактическую конфигурацию и число updates реального целевого caller. Если серия никогда не достигает horizon, пометить `NOT_APPLICABLE_TO_OBSERVED_RUN`; не расширять опыт до дорогого обучения.
2. Для native механической проверки создать две одинаковые копии одного сохранённого состояния с Warmup=0 и TotalSteps=4. Плечо A — текущий scheduler; B — только clamp progress в [0,1]. До первого update восстановить одинаковое RNG/dropout state; стохастические потоки не выбирать независимо. Передать каждой ровно 9 одинаковых supervised последовательностей в одинаковом порядке; модели, optimizer и Trainer между этими вызовами не пересоздавать.
3. После каждого шага записать step до/после, actual LR, loss, grad norm, update norm для каждого trainable tensor, AdamW moments/step, model hash. Контроль равенства A/B до границы нужен, чтобы исключить другое исходное состояние или случайный поток.
4. На 12–24 одинаковых prompt записать обычные ответы before/A/B и отдельно exact, near, contradiction и retention. Для утверждения о forgetting в before должны существовать правильные ответы. Если исходный neural Student даёт 0 правильных, можно измерить новые приобретённые ответы, но нельзя заявлять сохранение ранее полезного навыка.
5. Только если механический эффект и разница качества подтверждены, повторить на настоящем caller с его конфигурацией и остановкой у заранее согласованного градиентного бюджета. Не менять одновременно warmup, обучение base, corpus, beta/weight decay или restoration policy.

**Подтверждение влияния:** после horizon повторный рост actual LR увеличивает update и ухудшает заранее проверенный закон/старый навык; clamp при прочих равных устраняет ухудшение. Если меняются только веса, статус остаётся `MECHANISM_CONFIRMED_QUALITY_UNPROVEN`. Если A отвечает лучше B, restart мог быть полезен выбранному профилю; сохраняется фактический результат, а не предписанный PASS.

**Опровержение/сужение:** caller ограничен horizon; исследуемые updates выполняются только DpoTrainer, без последующей supervised серии; scheduler намеренно имеет документированный restart contract; либо paired quality одинакова. Тогда нельзя рекомендовать clamp как доказанное улучшение обучения этого пути.

**Исправление, которое предлагается проверить:** явная политика после horizon — остановка/обнуление нового этапа по согласованному контракту либо продолжение на MinLr. Для короткого однофакторного опыта используется clamp; окончательный выбор должен соответствовать назначению session. Сохранение `_supervisedStep` между session — другой, ранее известный вопрос и отдельный эксперимент.

## Как повторить только уже выполненную проверку

Из корня этого пакета:

```bash
python3 evidence/source-formula-probes.py
```

Команда печатает JSON в stdout и не обращается к репозиторию, checkpoint, внешней модели или сети. Это **единственная выполненная здесь вычислительная проверка** этих двух гипотез. Будущие C# harness/CLI команды не выдуманы: их необходимо добавить к существующим вызовам в изолированной implementation работе после выбора опыта. Все результаты по качеству в этой главе остаются NOT_RUN.
