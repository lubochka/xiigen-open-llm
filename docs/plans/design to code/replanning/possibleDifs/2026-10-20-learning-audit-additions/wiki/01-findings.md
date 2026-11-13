# Wiki learner: два новых кандидата и сохранённые результаты kNN

Дата: 2026-10-20. Режим: **документирование и проектирование, без исполнения**. Core: `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`; planning: `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`. Защищённая папка `llm_mvp_core/docs/plans/replanning-possible-diffs` прочитана, не изменена. Новые карточки добавляются в `possibleDifs` как кандидаты для последующего решения владельца.

## 1. Что добавлено

| Кандидат | Подтверждено чтением исходника | Что ещё предстоит измерить | Приоритет |
|---|---|---|---|
| [WIKI-FEATURES-ONCE-01](03-features-once-experiment.md) | Один и тот же чистый `Featurize(row.Prompt)` вызывается внутри каждой из 18 эпох; признаки затем прямо участвуют в обновлении весов | Изменение allocations, CPU, времени настоящего `Train`; цена дополнительной живой памяти во время эпох; сохранение полученного обученного состояния | Первый из этих двух: прямые затраты обучения, малая область изменения |
| [WIKI-FEATURE-KEYS-01](02-feature-keys-experiment.md) | Unigram и bigram используют общее строковое пространство ключей; `_` одновременно разрешён внутри токена и служит разделителем биграммы | Меняет ли раздельное кодирование признаков результат обучения на содержательных wiki-запросах с идентификаторами; допустим `NO_GAIN` | Затем: условная гипотеза качества, зависящая от домена |

**Ни один новый кандидат ещё не имеет измеренного улучшения.** Для обоих `NOT_RUN`; runner в этой добавке не реализован и не собран. Приложены буквальные TRAIN/EXAM fixtures и [точное задание подготовки короткого source-copy host](04-source-copy-work-order.md). Это законченный дизайн малого опыта, не утверждение о готовой исполнимой программе. Не выдавать его за исправление production или установленную причину неудачи RP01.

## 2. Почему оба механизма действительно относятся к обучению

Текущий [учебный runner L43–53][runner] вызывает `ProjectWikiRequestRoutingTrainableModel.Train(trainingRows)`, затем `RouteForEvaluation(row.Prompt)`. [Learner L58–83][train] преобразует `Prompt` в признаки, вычисляет предсказание и обновляет `_tokenWeights` и `_biases` при ошибке. [Score L238–264][score] читает именно эти веса. [Учебный reader L95–99, L155–220][reader] заново строит признаки запроса и возвращает решение с route, target, status и confidence.

Это обучаемый лексический классификатор, получающий DTO с именем `ProjectWikiRequestRoutingDpoPair`. Его `Train` фактически использует `Prompt` и `ExpectedRouteKind`, а не likelihood разности chosen/rejected. Измерение данного learner не следует называть нейронным DPO, LoRA или обучением генерации CodeDocumentation. Наличие такого DTO не устанавливает участие learner в неудачном RP01; связь с конкретным failed run остаётся `UNPROVED`.

Для малого опыта копируется ровно этот learner и его учебный reader. Production runner требует [ровно 50 TRAIN и 50 held-out L27–32][quota]; он не запускается. Прямой вызов того же native `Train` с 4/8 строками — явно ограниченный лабораторный интерфейс, допустимый для проверки учебной гипотезы, а не ослабление production guard. Входные строки проходят настоящее обновление весов. EXAM получает только prompt; gold остаётся в host.

## 3. Исходное различие FEATURES-ONCE

В [Train L60–81][train] `Featurize(row.Prompt)` находится внутри цикла `epoch < 18`. [Featurize и RegexLikeWords L341–378][features] читают только переданную строку; в них нет доступа к весам, random, времени или меняющемуся состоянию. Следовательно, из исходного кода можно вывести повторное построение одинакового набора признаков для одной строки. Это ещё не замер скорости.

В кандидате B допускается один фактор: построить словарь признаков каждой строки один раз перед эпохами и читать его внутри них. `LabelIndex`, `Score`, порядок строк, порог ошибки, запись весов, learning rate и число эпох остаются на исходных местах. Перенос `Score` был бы ошибкой: score зависит от обновляемых весов. Не добавляются margin-training, усреднение perceptron или исправление taxonomy.

Цена изменения: все словари TRAIN одновременно живы во время обучения, вместо одного текущего словаря. Поэтому уменьшение выделенных байтов может сопровождаться ростом пиковой живой памяти. После `Train` локальный cache не должен сохраняться в модели; увеличение persisted snapshot не является необходимым следствием этого изменения. Короткий process peak и heap-after не измеряют точный live-memory peak внутри `Train`, и отчёт должен сохранить этот предел.

## 4. Исходное различие FEATURE-KEYS и граница его силы

[RegexLikeWords L359–378][features] сохраняет `_` внутри токенов. [Featurize L341–356][features] записывает unigram под самим текстом токена, а bigram под `left + "_" + right`.

| Разные признаки | Текущий общий ключ | Предлагаемое раздельное представление |
|---|---|---|
| Unigram `load_customer`; bigram `load`, `customer` | `load_customer` | `u:13:load_customer`; `b:4:load:8:customer` |
| Bigram `a_b`, `c`; bigram `a`, `b_c` | `a_b_c` | `b:3:a_b:1:c`; `b:1:a:3:b_c` |

Существует и полная коллизия вектора: `trace trace_trace` и `trace_trace trace` имеют одинаковое отображение `{trace: 1/√3, trace_trace: 1/√3, trace_trace_trace: 1/√3}`. Это математический вывод из source, **не запуск learner**. Порядок перечисления словаря может различаться и влиять на округление, но не восстанавливает отсутствующую идентичность признаков.

Этим двум искусственным строкам не присвоены разные routing-метки: у них нет двух установленных различных wiki-намерений. Их различение само по себе не было бы доказательством улучшения настоящей учебной задачи. Основной fixture использует явные запросы обучить code trace либо ответить по project wiki. `load_customer` и `save_order` выступают аргументами, а обычные `load customer` и `save order` встречаются в запросах ответить. Остальные слова могут полностью разрешить задачу и до изменения — поэтому возможны одинаковые результаты A/B.

Область релевантности: wiki/code identifiers и domain lexicons с подчёркиваниями, где unigram и соседняя пара несут различный полезный сигнал. Для запросов без таких пересечений введение namespace часто является лишь переименованием координат; обещать улучшение общего качества нельзя. Это не AST-функция kNN, не hash/modulo collision и не alias имени flow при загрузке checkpoint.

## 5. Первичные переходы в родительскую библиотеку

Прочитан маршрут `SOURCE-ROUTES.md` → две следующие родительские папки; использованы конкретные тела документов, а не только их названия в индексе.

| Первичный документ planning на зафиксированном commit | Содержательная связь | Уровень доказательства |
|---|---|---|
| [local wiki learning / ORCHESTRATION-PROTOCOL-REFERENCE-v1.md L35–47, L70–80][parent-wiki] | Требует runtime A→B примеров на нужном узле, отделяет source ledger от обучающих данных. Поэтому fixtures здесь содержат реальный вход route learner и ожидаемый native route; таблица коллизий не выдаётся за учебный результат | Исторический проектный reference, прочитан полностью. В шапке явно указано read-for-context; это не журнал исполненного обучения |
| [language translate learning / TRANSLATION-SMALL-MODEL-RATIONALE-v1.md L63–98, L189–204][parent-translation] | Выделяет направления и доменные потоки, измерения после урока, CPU/RAM/time рядом с содержательным качеством. Это поддерживает отдельный wiki-domain опыт и сохранение resource-gain даже без роста accuracy | Проектное обоснование, прочитано полностью. Заявление «small model sufficient» требует результатов и не принимается за установленный факт |

Исторические правила этих документов не заменяют текущее поручение: сейчас только документируем; старая квота 50 не навязывается малому source-copy опыту; старые gates не отменяют правило пользователя «любое полезное измеренное улучшение — успех по показателю и домену».

## 6. Положительные kNN результаты сохранены с их действительной областью

Следующие значения прочитаны из первичных сохранённых JSON. Это **исторические receipt, не повторные исполнения 20 октября**. Метка `passed` в них не заменяет анализ того, что сравнивалось. Исходник на текущем pin помогает определить механизм, но сам по себе не аутентифицирует бинарник, создавший старый receipt.

| Receipt | Сохранённое измерение | Что действительно следует и какие ограничения остаются |
|---|---|---|
| [E09 STRUCTURAL-KNN, 2026-05-01-001][r-e09] | top1 `0.2413 → 0.3507`, +10.94 п.п.; время `14.5 → 763.5 ms` | Положительное наблюдение в структурной задаче сохранено. [E09 L45–78][e09] использует для A весь validation, для B первые 500 элементов; это мешает считать delta чистым причинным эффектом на одинаковой панели. Известное ограничение нельзя скрывать, как и положительный receipt |
| [E13 METHOD-KNN, 2026-05-01-001][r-e13] | top1 `0.3333 → 0.3333`; время `2 → 6.8 ms`; 47 methods, 30 pairs, 24 TRAIN | Нулевой эффект этой панели не означает неработоспособность всех структурных признаков или всех доменов kNN |
| [E14 CONTEXT-ACTION-KNN, 2026-05-01-001][r-e14] | top1 `0.75 → 1.0`, +25 п.п.; время `3.3 → 7.3 ms`; 16 TRAIN / 4 validation | [E14 L36–70][e14] действительно обучает `FileActionKnnPredictor` и предсказывает. Метки порождены `ActionLabelGenerator`; это согласие с synthetic policy действий на C#, не измерение качества итоговой программы и не LoRA instruction tuning, несмотря на формулировку source header |
| [E25 PATTERN-CLASSIFIER, 2026-05-01-001][r-e25] | top1 `0.3438 → 0.875`, +53.12 п.п.; hard accuracy `0.8571`; время `0.4 → 15.4 ms`; 160 examples / 32 validation | [E25 L24–47][e25] использует `SentencePatternClassifier.Train/Predict`. [Классификатор L39–55, L77–91][pos] хранит обучающие примеры и считает Jaccard по **tuple POS bigrams**. Это положительное обучение выбора pattern на synthetic POS templates, не BLEU/chrF или свободный перевод raw text |

E25 показывает, почему знание порядка может быть полезно для POS/синтаксической задачи. Оно не обосновывает требование учитывать порядок во всех наборах JSON-полей, мешках независимых фактов или графовых узлах. Реализация POS bigrams использует пары целых, поэтому текущую wiki string-key коллизию нельзя приписать ей по сходству слова bigram.

Для проверки соседних веток просмотрены корневые деревья `language_translate`, `DesignToCode`, `external_domains`, `PrepareToGPL`, `main` и текущая ветка. У первых трёх совпадает subtree `12.SemanticCompiler` (`66eb13…`), у последних двух и current — `cae2d9…`. Полные тексты `StructuralKnn.cs`, `MethodKnn.cs`, `FileActionKnn.cs` и `E13MethodLevelKnn.cs` сравнены между [language_translate@f6032a…][old-tree] и current: различается только пятистрочная license-шапка. **Потеря алгоритма при переносе этих четырёх файлов не подтверждена.** Это ограниченная сверка, не заявление об отсутствии иных различий во всех 15 ветках.

## 7. Дедупликация и protected coverage

Сверено полное локально полученное содержимое **678 Markdown possibleDifs + 301 Markdown защищённого корпуса** на указанных pins. Поиск `WIKI.FEATURES.ONCE`, `WIKI.FEATURE.KEYS`, `Featurize`, сочетаний unigram/bigram с collision/namespace/underscore, perceptron с cache/epoch и `features…once/epoch` не обнаружил карточки двух точных новых факторов. Найденные близкие темы прочитаны и отделены ниже. Это подтверждение границ текстового и адресного смыслового поиска, не обещание, что никакой агент никогда не обсуждал такой фактор вне этих файлов.

| Уже существующая тема | Почему новый фактор другой | Состояние coverage |
|---|---|---|
| [WIKI-TAXONOMY-01][old-taxonomy] | Пропущенные два route labels; у нас только уже поддерживаемые CodeTraceTraining и WikiQuestionAnswer, Labels15 неизменен | Известная карточка сохранена; не дублируем |
| [WIKI-MARGIN-01][old-margin] | Пропуск обновления при первом правильном argmax; у нас условие `predicted == expected` неизменно | Известная карточка сохранена; её CandidateModel не копировать |
| [KN-ROUTER-LEARNED-KEY-CANONICALIZATION][old-canon] | В другом learner trainer нормализует C#/.NET иначе, чем reader. Здесь оба конца вызывают одну Featurize, а коллизия находится между типами признаков внутри неё | Другая исходная функция и другой A/B фактор |
| [G-W05 в additions-language-restore-49][old-restore] | Snapshot label-order/version restore; новый опыт начинает с двух свежих состояний, старые checkpoints не мигрирует | Известная тема не «открыта» заново |
| [Исторические E25 POS seams][old-pos] | Short POS seq / multiplicity / Jaccard set; string namespaces wiki не затрагиваются | Положительные старые результаты и domain scope сохранены |

Для новых двух факторов покрытие защищённых экспериментов: **MISSING_EXACT_EXPERIMENT** на `ab583032…`. Предложенные карточки здесь — добавочный кандидат на будущее, а не самовольная правка protected. Старые эксперименты, результаты, журналы и упавшие attempts сохраняются.

### Поздняя дельта, проверенная перед завершением

Core `ab583032… → 65d03a73a76810a0425a41c33d3c7eeaf4cdbc48` меняет 156 файлов только в `docs/`; native source pin двух новых факторов остаётся действительным. Поздний planning `a868f46ee3695728406173f8cd945e278ddb0def` добавил пакет `2026-10-20-learning-candidates-and-causal-gaps`; дополнительно проверены его 17 Markdown на эти feature/epoch механизмы. [KH-COST-01](https://github.com/lubochka/xiigen-planning/blob/a868f46ee3695728406173f8cd945e278ddb0def/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps/knn-cost-card.md) переиспользует один Roslyn AST между `FeatureVector` и `Label` в `TrainFileActionKnn`; это другой learner, данные и место вычислений. KH-EXT-01 касается сохранности MethodKnn памяти после sparse batch; новая gradB карточка — нейронного градиента. Ни одна не заменяет lexical Featurize-per-epoch или unigram/bigram namespace. Поздние короткие WT01/WM01 пути относятся к сохранённым taxonomy/margin опытам; новые `C:/xg/wc1` и `C:/xg/wk1` не пересекаются с ними и не берут их treatment. Защищённые поздние документы не изменены этим аудитом.

[runner]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L43-L53
[train]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L58-L83
[score]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L238-L264
[reader]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L95-L220
[quota]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L27-L32
[features]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L341-L378
[parent-wiki]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/local%20wiki%20learning/ORCHESTRATION-PROTOCOL-REFERENCE-v1.md#L35-L80
[parent-translation]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/language%20translate%20learning/TRANSLATION-SMALL-MODEL-RATIONALE-v1.md#L63-L204
[r-e09]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json
[r-e13]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/35.ExperimentsV2/results/E13-METHOD-KNN/results/run-2026-05-01-001.json
[r-e14]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-001.json
[r-e25]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json
[e09]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L45-L78
[e14]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/35.ExperimentsV2/Experiments/E14ContextActionKnn.cs#L36-L70
[e25]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L24-L47
[pos]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/12.SemanticCompiler/Translation/PatternClassifier.cs#L39-L91
[old-tree]: https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction
[old-taxonomy]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-TAXONOMY-01.md
[old-margin]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-MARGIN-01.md
[old-canon]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/routing/KN-ROUTER-LEARNED-KEY-CANONICALIZATION-2026-10-20.md
[old-restore]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/10-additions-language-restore-49.md#L159
[old-pos]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/50-EXTERNAL-DOMAINS-E25-NOVEL-POS-SEAMS-2026-10-19.md
