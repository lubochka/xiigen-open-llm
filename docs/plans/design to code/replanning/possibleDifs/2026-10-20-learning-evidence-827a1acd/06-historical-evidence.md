# Историческое обучение и новые доказательные подслучаи

Статус: **документный аудит; новых тренировок, сборок и исправлений продукта не выполнялось**. Свежие находки: узкая гипотеза стоимости `JSON-OPTIONS-REUSE-01` и ранее не названный ложноположительный тест сохранения verifier. Исторические успехи ниже сохранены как наблюдения своих механизмов и доменов.

## 1. Источники и границы проверки

Текущий core: `66947785d1318f7cbbc88118f3156cdc75001e16`; planning: `53cbcabd3926cda714bab428d294eb666301e9e4`. Исторический код и результаты E09/E25/E26/E43 прочитаны на `language_translate=f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`. Дополнительно проверены дерево docs/Translation/training-data `PrepareToGPL=31346d8928be4469b01aaf313780e8f950ff91e2` и его TranslationTrainedUnits/TranslationMachine.

Все 799 Markdown planning/possibleDifs и 339 Markdown protected-каталога были доступны для дедупликации. Поиск конкретных символов дополнен чтением подходящих прежних карточек. Отсутствие текстового совпадения не доказывает абсолютную новизну; заявляется новый конкретный фактор относительно проверенного корпуса. Широкое заявление «все соседние ветки проверены полностью» не делается: external_domains и DesignToCode-clean не были построчно исследованы этим агентом.

Использованы правила архитектурного разделения writer/state/consumer и доменов, дизайна малого опыта, человеческого объяснения; прикладная проверка кода сверяет название с фактическим исполняемым методом. Нормы нового запроса — не менять protected, не запускать обучение в документном аудите, не навязывать полный production lifecycle — имеют приоритет перед историческими общими запретами skill-файлов.

## 2. Что сохранилось в успешных исторических опытах

| Опыт и первичный результат | Фактически обучаемый механизм | Подтверждённый полезный результат | Ограничение интерпретации |
|---|---|---|---|
| [E09 result](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json#L2-L18) | `StructuralKnnPredictor.Train` получает 4492 пары structural-vector→next-kind | В сохранённом pilot top1 .2413→.3507; time 14.5→763.5 ms | Baseline читает весь val, KNN только первые 500 токенов. Успех записан, однако чистый парный прирост 45.33% на одной панели не доказан. |
| [E25 result](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json#L2-L18) | KNN по POS-биграммам, `Train(train)`; 160 synthetic sequences, 32 val | top1 .3438→.875; hard accuracy .8571; time .4→15.4 ms | Проверка предсказания класса порядка слов по POS; не end-to-end перевод и не обучение нейросети. |
| [E26 result](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json#L2-L18) | Retrieval по заранее записанным словарным entries и явно данным POS-contexts | disambiguation .4→1.0 на 15 случаях,6 hard | `BuildDefault` записывает словарь в коде. Это полезность contextual reader, не приобретение новых правил уроками в этом запуске. |
| [E43 result](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json#L2-L18) | Построение счётчиков биграмм из новых данных либо новых + 177 replay pairs | old-val accuracy .3226→.3978, retention 1.1562; time .1→5.8 ms | Каждое плечо вызывает новый `BuildBigram`, без продолжения прежнего model state. Файлы делятся пополам; названия CODE_Q/SECURITY в комментарии сами не доказывают независимые реальные домены. |

### E09: существенная функция KNN и несопоставимые знаменатели

[Строки 21–73](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L21-L73) действительно строят training pairs и обучают KNN. Следовательно, память соседей является допустимым получателем обучения. Но `val.Count-1` и `val.Take(500).Count-1` задают разные панели. При 5617 токенах и 80% train это 1123 baseline predictions против 499 KNN predictions. Эти численные сравнения не заменяют равную source-disjoint панель в новом опыте.

[Строки 97–116](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L97-L116) показывают AST узлы и локальный `kindMap` каждого файла. Это не обычные BPE-токены, несмотря на имя `BpeId`. AST-specific distance применим к исходному коду при совместимой схеме представления. Перенос на предложения или на LoRA требует отдельного learner/input contract. Эти ограничения ранее уже описывались в семье E09; новый номер им не присваивается.

### E25: порядок предложений как обучаемая retrieval-задача

[Реальный training и одна val-панель, строки 17–43](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L17-L43). В отличие от правила по позиции первого глагола, KNN обучается на POS-последовательностях. Релевантен доменам, где порядок POS несёт целевой смысл. Для SQL, AST и иной структуры нужна собственная сохранённая семантика и метрика, а не перенос синтаксиса естественного языка по названию KNN.

### E26: сохранить успех reader, не подменить им learning

[Таблица заранее записанных entries](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L24-L58); [исходные контексты и вызов reader](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L15-L42). Этот опыт не доказывает, что текущий Executor научился выбирать смысл слова из уроков с контекстом. В новом опыте надо назвать writer, показать записи до/после и проверить те же источники через обычного consumer. E26 context-order и missing-context gaps уже покрыты, не объявляются новыми.

### E43: replay в памяти не равен доказанному лечению forgetting нейросети

[BuildBigram и replay, строки 18–50](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L18-L50); [счётчики и evaluator, строки 84–104](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L84-L104). Смешивание старых пар даёт измеренный плюс на старой панели этого count-based model. Однако опыт сравнивает rebuild, не продолжение того же optimizer/LoRA. Поэтому его можно переносить как гипотезу влияния состава памяти, сохранив метод, и отдельно исследовать настоящее последовательное нейросетевое обучение.

**Вывод о matrix→KNN:** данные подтверждают, что семейство retrieve-from-learned-memory имеет успешные варианты и их нельзя отбрасывать только из-за неуспешного матричного воплощения. Приведённые файлы сами по себе не связывают конкретный неудачный matrix-run с функционально тождественным успешным KNN-run. Такая причинная пара остаётся `NOT_ESTABLISHED`; нужен одинаковый target, corpus, split и downstream consumer. Исторические success-status не дают разрешения автоматически перенести один алгоритм во все домены.

## 3. Новая гипотеза стоимости фактического обучения

**JSON-OPTIONS-REUSE-01; family P125; статус PLANNED_NOT_RUN.** [ExecutorUnit.TrainOnExample, строки 404–455](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L455) создаёт новые `JsonSerializerOptions` и `JsonStringEnumConverter` для каждого урока, затем десериализует input/chosen и пишет реальную exact/phrase/KNN память. [Caller, строки 107–145](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationMachine.cs#L107-L145) вызывает этот метод на каждой учебной строке.

Одно вмешательство: повторно использовать фиксированные options только в этом методе. Никаких изменений builder, tokenizer, verifier, evaluation, DTO, сериализуемых полей или данных. Первую инициализацию shared options считать внутри первого реального train-call; не прятать её в setup. Сравнить allocated bytes всего `TrainStructuredBatchAsync`, wall, ordinary output и сохранённую память.

Это гипотеза снижения цены приобретения той же памяти. **Не заявляется объяснением semantic degradation RP01.** Полная карточка и prompt: [карточка](JSON-OPTIONS-REUSE-01.md), [промпт](JSON-OPTIONS-REUSE-01-PROMPT.md). Исполнение в текущем аудите не назначено.

Новизна: `JsonSerializerOptions|serializer…options/cache/reuse` по 799 planning+339 protected найдены лишь в сопутствующих snippets сериализации/manifest. Фактор per-lesson options lifetime в реальном обучении Executor не описан. Это не повтор N62, который устраняет повторное создание training context другого learner.

## 4. Новый доказательный дефект: тест verifier checkpoint не обучает правило

Статус **SOURCE_CONFIRMED_VALIDATION_GAP**, не результат эксперимента и не отдельная строка runtime-очереди. Owners: P25.2 (сохранение выученного state) + P56 (verifier). Три звена доказательства:

1. [Тест TranslationMachine_CheckpointRoundTrip_QualityVerifierConceptRule, строки 3052–3085](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/Tests/TranslationDomainTests.cs#L3052-L3085) утверждает по комментарию, что verifier rules заполнены; реальная проверка после load — только отсутствие `[RU_EN:BOOTSTRAP]`.
2. [BuildVerifierExample, строки 2679–2694](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/Tests/TranslationDomainTests.cs#L2679-L2694) строит `chosen = VerifierOutput(0.96, [], true, null)`.
3. [TrainOnExample verifier, строки 1095–1146](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L1095-L1146) пишет правила только внутри `!chosen.Pass && chosen.Issues is not null`.

Следовательно, этот конкретный тест не приобретает ни concept-rule, ни required-term через указанный урок. Успешный перевод после load может обеспечиваться ранее обученным Executor. Это не доказывает баг importer и не отменяет измеренный P25 EN_RU-memory success.

**Связь с результатом обучения:** [ExportCheckpoint/ImportCheckpoint правил, строки 1173–1214](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L1173-L1214) — реальный state owner; [обычный adapter, строки 117–142](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L142) вызывает learned tokenizer, executor и verifier. Результат verifier влияет на принятие кандидата, но phrase-map confidence>=.85 может обойти FAIL: нужен выбранный обычный non-phrase route, чтобы отсутствие действия правила не маскировалось.

**Метод исправления доказательства, когда пользователь назначит соответствующую существующую family-card:** в isolated harness дать настоящий `chosen.Pass=false` урок с разбираемым issue, показать state rule0→1 и verdict PASS→FAIL для неправильного кандидата, PASS для законного кандидата; сохранить и загрузить те же bytes новым process; сверить оба verdict и rule record. Загрузка не получает новых уроков. Если first lesson не записал ожидаемый ключ, stop с точной parser/fixture причиной, без «лечения» exporter наугад. Parser src/ctx и context-matching — уже отдельные факторы P52.3/P56.1; не исправлять их одновременно ради этого доказательства.

**Конкретный будущий ремонт оригинала после принятого эксперимента:** корректный verifier-failure fixture в `Tests/TranslationDomainTests.cs::TranslationMachine_CheckpointRoundTrip_QualityVerifierConceptRule`, явная проверка learned rule и lawful/unlawful consumer до/после coldload. Изменения production `ExportCheckpoint/ImportCheckpoint` требуются только если реальный эксперимент обнаружит потерю выученного state. Самый факт пустого теста не даёт оснований менять importer.

**Покрытие:** [P25.2](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md#L149-L153) уже описывает live-trained→cold без новых updates. [P56.1](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/P56-verifier-context.md#L142-L145) описывает context. Точный vacuous test/helper не найден по 799+339 MD. Это добавочное доказательство для этих семей, **не новый эксперимент на тестовом фасаде**. Бюджет N60-E1 не меняется: тот ограничен `_requiredTermsBySourceMarker` и не включает concept persistence.

## 5. Не выданы за новые

| Рассмотренный фактор | Уже существующее покрытие / причина не заводить новый ID |
|---|---|
| TryParseIssue теряет роль src/ctx; Verify игнорирует ctx | 28-NEW-VERIFIER-ISSUE-ROLE-PARSER-AND-CONTEXT-LEAK; P52.3/P56.1 |
| Удалённая пунктуация сдвигает phrase-map token ordinal | 33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT |
| E44/E45 перестановка до создания next-token pairs меняет target | 33-NEW-E44-E45-PAIR-TARGET-CONFOUND; 27-NEW-SEQUENCE-SPLIT-CONFOUNDING |
| Probe gradient использует уже изменённые weights | P49.2 |
| Пустой/unknown structured batch сбрасывает readiness | P53 и 27readiness-reset |
| TrainOnExample verifier не принимает positive correction | 62/CANDIDATE-NOT-PROMOTED-VERIFIER-POSITIVE-FEEDBACK; caller admissibility ещё требует доказательства |
| MeasureAccuracy создаёт fresh raw tokenizer | N27/N28, P52/P54/P57 |
| E26 loses POS order/unknown-context eligibility | 44-E26-UNKNOWN-CONTEXT-AND-ORDER |
| Глобальные verifier rules между направлениями; накопление AND | P56.8/P56.9 |
| P125 actual memory и P25 coldload | Уже есть первичные выполненные отчёты; это основания новых bounded follow-ups, не новые измерения |

## 6. Приоритет и запрет продолжать прежний опыт

Сначала закрывать blockers настоящего обучения; затем конфликты карточек; затем новые узкие учебные/стоимостные факторы. `JSON-OPTIONS-REUSE-01` имеет приоритет после активных blockers, не оттесняет P54/N62 и уже назначенные learning runs. Пустой verifier checkpoint test — nonqueue addendum, который используется только в явно назначенной P25/P56 работе.

В каждой новой карточке и prompt явно запрещено продолжать прошлую задачу, дооформлять её report/review/retry/Git после указания «переходи». Сразу перейти к указанному NEXT; после «остановить всё» не вызывать инструменты и не «сохранять ещё одну полезную правку». Partial artifacts сохраняются; отсутствие законченного отчёта не оправдывает нарушение stop. Никогда не удалять старый опыт ради повторного запуска.

Любое измеренное улучшение полезной метрики — успех этой метрики и домена, даже если accuracy не выросла. Все ухудшения показываются рядом. В данном аудите новое улучшение **не измерено**, поэтому новый фактор остаётся кандидатом к проверке, без production promotion.

