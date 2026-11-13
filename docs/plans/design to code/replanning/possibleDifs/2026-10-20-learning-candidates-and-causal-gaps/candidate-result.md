# N-KU-01: положительный результат обучения памяти и кандидат исправления

**Обновление статуса перед публикацией:** N-KU-01 уже зарегистрирован как кандидат в [core `required-changes.md` на `ab583032`](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/required-changes.md#L21-L57). Регистрация пришла внешним коммитом, не выполнена этим аудитом; product code и raw-измерения не изменены. Ниже сохранён подробный разбор исходных наблюдений на `9622b575`. [Финальное дополнение](05-final-publication-delta.md) уточняет оставшуюся причинную оговорку: при соотношении меток 3:1 и k=3 близость признаков не выделена как причина регрессии KQ4.

**Статус: кандидат на изменение продукта, результат успешен по измеренному показателю; внедрение не выполнено.** В сохранённом эксперименте добавление `DoStatement` в генератор учебных меток снизило число ложных советов `ADD_LOOP` с **3 до 0**. После обучения выполнение заданного требования к совету выросло с **1/4 до 3/4**, то есть с **25% до 75%, +50 процентных пунктов**. В контрольном примере без цикла возникла регрессия: правильный совет `ADD_LOOP` сменился на `ADD_METHOD`. Регрессия и ограничения не отменяют измеренный успех на своём показателе и C#-панели.

Этот документ пересматривает сохранённые исходники и результаты; новых запусков, обучения, исправлений продукта, его тестов или защищённой папки нет. Источники закреплены на core `9622b575a6aabb79939b12cc9f5d96313f1bf370`. Текущее поручение пользователя — только документировать и представить кандидата.

## Что именно является успехом

В исходном gold три запроса требуют **любого непустого совета, кроме `ADD_LOOP`**, а четвёртый — именно `ADD_LOOP`. Поэтому 3/4 нельзя без уточнения назвать точностью выбора оптимального действия: пригодность `ADD_METHOD` для первых трёх задач не установлена. Корректное название метрики — «выполнение заданного требования к совету». Нулевой совет не засчитывался. [Точный gold и правило](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/gold.json#L2-L8).

| Показатель / случай | A0: до обучения | A1: исходное обучение | B0: до обучения | B1: исправленные метки | Вывод |
|---|---:|---:|---:|---:|---|
| Выполнено требование к совету | 0/4 | 1/4 | 0/4 | 3/4 | Успех +2/4, +50 п.п. после обучения |
| Ложный `ADD_LOOP` на KQ1–KQ3 | 0/3 при отсутствии советов | 3/3 | 0/3 при отсутствии советов | 0/3 | Успех снижения ложных советов; нулевые A0/B0 — не правильный ответ |
| KQ1: `do-while` уже перебирает массив | null | ADD_LOOP | null | ADD_METHOD | Исправлен ложный совет добавить цикл |
| KQ2: второй `do-while`-пример | null | ADD_LOOP | null | ADD_METHOD | Исправлен ложный совет добавить цикл |
| KQ3: известная форма `while` | null | ADD_LOOP | null | ADD_METHOD | Исправлен ложный совет добавить цикл |
| KQ4: для задачи нужен отсутствующий цикл | null | ADD_LOOP | null | ADD_METHOD | Регрессия: 1/1 → 0/1 |
| Обычные обращения к обученной памяти после Train | 0 | 4 | 0 | 4 | Один и тот же reader |

Первичные наблюдения: [A0](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json#L38-L58), [A1](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json#L214-L240), [B0](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json#L43-L63), [B1](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json#L219-L245). Доли пересчитаны по этим сохранённым ответам и сохранённому gold; это аудит документов, а не новый эксперимент. В имеющемся обратном порядке запуска ответы совпали: [существующий отчёт](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md#L31-L45).

Любое измеренное улучшение полезного показателя считается успехом именно на этом показателе и домене. Порог ≥5%, рост общей accuracy, статистическая значимость, 95%-й интервал или повторный запуск не являются условиями признания данного локального успеха. Малый размер панели, возможный шум ресурсов и регрессии фиксируются отдельно. [Действующее правило](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md#L29-L35).

## Почему эксперимент связан с настоящим обучением

Это обучение таблицы kNN, а не обновление LoRA или DPO и не одна лишь проверка teacher labels. Изолированный entry в каждой ветви строит четыре `(Features, Label)` и вызывает настоящий `_train(examples)`. После этого обычный портированный путь совета вызывает `_predict(_featureVector(source))` и возвращает метку из обученной таблицы. [Writer и reader](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/Experiments/Rp01Micro/NKu01/NKu01FileActionEntry.cs#L135-L168).

Цепь: четыре C#-исходника → `ActionLabelGenerator.Label` и `ActionLabelGenerator.FeatureVector` → `FileActionKnnPredictor.Train` → четыре строки памяти → `PredictFileAction` → четыре фактических совета. В каждой основной ветви зарегистрированы один Train и четыре строки, четыре обращения к предиктору после обучения, ноль обновлений нейронных весов; исходная пустая память возвращала null из guard. [A: операции и состояние](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json#L207-L254); [B: операции и состояние](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json#L212-L259).

При этом весь продуктовый Compile/Flow не вызывался. Эксперимент проверяет узкую память file-action и портированный обычный путь совета. Это достаточная учебная связь для данного вывода, но не доказательство изменения качества компиляции проекта или всего RP01.

## Единственное изменение A/B и текущий продукт

В исходном `Label` коллекция без `ForEachStatement`, `ForStatement` и `WhileStatement` получает `ADD_LOOP`; `DoStatement` отсутствует. В B к этому перечислению добавлен только `DoStatement`. Product и VariantA совпадают за исключением namespace; A/B отличаются namespace и этим элементом. Учебные исходники, извлечение 30 признаков, `k=3`, число строк и обычный reader сохранены. [Текущий продукт](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/Prediction/FileActionKnn.cs#L41-L55); [Дельта копии в raw B](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json#L22-L40).

**Кандидат оригинального исправления:** файл `12.SemanticCompiler/Prediction/FileActionKnn.cs`, метод `ActionLabelGenerator.Label`, выражение `hasLoop`:

```diff
                 node.Kind() == SyntaxKind.ForEachStatement ||
                 node.Kind() == SyntaxKind.ForStatement ||
-                node.Kind() == SyntaxKind.WhileStatement);
+                node.Kind() == SyntaxKind.WhileStatement ||
+                node.Kind() == SyntaxKind.DoStatement);
```

Именно эта правка является кандидатом переноса; изменять extractor, `k`, общий policy или корпус этим результатом не поручено. Сохранённая [предложенная дельта](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/product-change-proposal.diff#L1-L16) не означает, что её уже внесли: в закреплённом продукте `DoStatement` по-прежнему отсутствует. При включённом file-action механизме learner вызывается из [SemanticCompilerFacade.TrainFileActionKnn](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerFacade.cs#L194-L203), а совет читается из [PredictFileAction](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerFacade.cs#L327-L335). Расширение применимости на обычный продукт зависит от соответствующей опции и вызова этого пути.

## Почему это ещё не доказательство распознавания структуры цикла

У A метки четырёх строк: три `ADD_LOOP` и одна `ADD_METHOD`; у B — три `ADD_METHOD` и одна `ADD_LOOP`. При `k=3` среди любых трёх выбранных строк большинство всегда составляет класс, представленный тремя строками во всей памяти. Поэтому в этой панели ответ математически постоянен для каждого плеча вне зависимости от расстояний: A всегда советует `ADD_LOOP`, B всегда `ADD_METHOD`. Это объясняет и выигрыш KQ1–KQ3, и потерю KQ4. [Метки, признаки и ограничение большинства](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md#L47-L80); [Сам отчёт явно оговаривает предел](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md#L139-L156).

Вывод остаётся положительным: исправленный генератор изменил настоящую учебную память и снизил число ошибочных советов на измеренной панели. Не доказано, что память научилась различать наличие цикла, что `ADD_METHOD` — лучший совет, что качество стало лучше на иных C#-задачах или иных языках. Изменение представления `DoStatement` в extractor и обработка tuple-`foreach` уже названы в существующем отчёте; их нельзя выдавать за новые находки этого аудита или включать в данный однофакторный результат.

## Измеренные затраты: сохранить все изменения, не смешивать разные объёмы работы

| Сохранённый замер | A | B | Что разрешено заключить |
|---|---:|---:|---|
| Время подготовки 4 строк + одного Train, A→B | 195,8347 мс | 123,8038 мс | В этой паре B меньше на 72,0309 мс; стабильный causal speedup не установлен |
| То же, существующий порядок B→A | 98,7049 мс | 133,1402 мс | Направление эффекта обратное; первый процесс в обоих порядках медленнее |
| CPU обучения, A→B | 109,375 мс | 93,75 мс | Локальный измеренный результат этой пары |
| CPU обучения, B→A | 78,125 мс | 93,75 мс | Обратное направление; не усреднять до выдуманной гарантии |
| Выделено байт в фазе обучения, A→B | 1 179 032 | 1 169 344 | Сопоставимый фазовый замер, локальное уменьшение 9 688 байт |
| Выделено байт в фазе обучения, B→A | 1 166 696 | 1 169 344 | B больше на 2 648 байт |
| Время четырёх советов после Train, A→B | 22,4050 мс | 26,0613 мс | Регрессия времени ответа в этой паре |
| Выделено байт на четыре совета, оба порядка | 45 168 | 45 152 | Наблюдаемое локальное уменьшение 16 байт; эффект мал |
| Payload четырёх строк, оценка без object overhead | 496 байт | 496 байт | Размер представления не изменился; это не measured heap/RAM |
| Process peak working set, A→B | 45 776 896 байт | 45 408 256 байт | Объём работы процессов различался, причинный выигрыш RAM обучения не доказан |

Фазовые данные взяты из [raw A](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json#L207-L262), [raw B](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json#L212-L267) и сохранённых [raw A обратного порядка](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/order-check-BA/arm-A.json#L207-L262), [raw B обратного порядка](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/order-check-BA/arm-B.json#L212-L267). Весь A-процесс дополнительно выполнял product-equivalence; у B такого же объёма работы нет. Поэтому сравнение process CPU, GC-total, wall и peak working set не изолирует стоимость нужного изменения. Предположение о JIT/прогреве не следует выдавать за измеренную причину. [Описанные дополнительные операции](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md#L93-L124).

Отрицательные/нестабильные затраты не отменяют уже показанное улучшение ложных советов. Отсутствующее улучшение accuracy отдельной общей панели также не требовалось. Новых замеров для принятия документа в кандидаты не нужно.

## Происхождение и ограничения аудита

- Использованы сохранённые `arm-A.json`, `arm-B.json`, reverse-order arms, `gold.json`, report и исходники обоих вариантов. Новая runtime-проверка не выполнялась; существующий reverse-order прогон не является повтором, запущенным этим аудитом.
- `comparison.json` в закреплённом Git blob — **Git LFS pointer** (`oid sha256:5927b9af1d2a02ea0f8787ff6ee0fb599e4161b9dc723741f6efb69b8c24b143`, size 8532), а не доступный JSON измерений. Он не использован как будто был распарсен: [первичный pointer](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/comparison.json#L1-L3). Основной вывод воспроизводится из доступных arms и gold.
- В исходном отчёте независимый review помечен как ещё не выполненный. Настоящий документ — проверка сохранённого материала, **не аттестация runtime и не подмена отдельного независимого повторного эксперимента**. [Статус исходного отчёта](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md#L1-L14).
- Старый preregistration и более поздняя расширенная карточка различаются; менять задним числом условия фактически выполненного опыта или перезапускать его только ради соответствия новому тексту нельзя. [Сохранённое различие версий](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md#L160-L172).

**Решение, предлагаемое пользователю:** оставить N-KU-01 как положительный кандидат исправления `DoStatement` в генераторе учебных меток. При дальнейшем отдельном решении о внедрении перенести именно указанную дельту и отдельно оценить приоритет регрессии/представления признаков; в текущем поручении продукт и защищённые документы остаются неизменными.
