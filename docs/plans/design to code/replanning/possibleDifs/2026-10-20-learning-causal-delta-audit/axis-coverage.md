# Карта различий: источник → обучение → обычный потребитель

Проверяемый core: **65d03a73a76810a0425a41c33d3c7eeaf4cdbc48**. Для уже полученных source links используется также **ab583032562d6656de9dd5ca6fa0a325fd4b1f30**: между этими коммитами нет изменения алгоритмов, моделей или данных. Это подтверждено полным compare из 156 изменённых документов. [Дельта источников](latest-source-delta.md).

Здесь перечислены направления, проверенные или сопоставленные в этом аудите. **«Покрытие» означает наличие соответствующей темы в защищённом каталоге, а не автоматически правильный и готовый эксперимент.** Точные пути всех P-карточек, их подслучаи и доказательства правил находятся в [experiment_audit.json](experiment_audit.json). Один source difference ещё не доказывает причинное ухудшение RP01; сохранённый domain-specific успех не обязан распространяться на другой learner.

## Матрица направлений

| Ось | Причинная связь с обучением и возможное отличие | Вывод и адрес дальнейшей работы | Уже существующее покрытие |
|---|---|---|---|
| Учебный получатель DPO | Pair → native log-prob → backward → optimizer → LoRA weights → обычный forward. Изменение счётчика без изменения этого состояния недостаточно | Native DPO update существует. Full-model SFT и frozen-base q/v-LoRA различаются получателями; EMB результат не доказывает отсутствующий gradient замороженного embedding. [Разбор](core_learning.md) | P01, P02, P10, P11 |
| Содержательность chosen/rejected | Если ToDpoMetricPair заменяет запрос и ответы маркерами класса ошибки, learner получает другой материал | Два ProjectWiki преобразователя отличаются от donor. После отдельного опыта возможный fix — передавать исходное содержимое пары в этих методах. Ниже приведены current/donor строки | P04, P108, P118 |
| DPO objective, response mask, длина | Mean по prompt+response, sum по response и fractional weighting задают разные функции. Их loss нельзя смешивать | Exact RP01 response sum и native signs разобраны; fractional derivative уже исправлена. FP32 totals/margin precision известны как PG-09/DLT-2, не новая гипотеза. [Разбор](core_learning.md) | P05, P07, P08, P86, P87; PG-09/DLT-2 |
| LoRA target/инициализация/заморозка | A/B, rank/alpha, q/k/v/o selection, allowlist и frozen base определяют, что получает update | Нулевой gradient A при fresh B=0 на первом шаге сам по себе корректен. Сравнивать real allowed tensors, deltas и обычный learner, не ярлык «LoRA сломана». [Разбор](core_learning.md) | P01, P06, P09–P12, P105 |
| Tokenization и response boundary | Raw pair → vocabulary/IDs → truncation/response mask. Потерянный ответ или meaningful token меняет учебную цель | Исторические BPE settings нельзя переносить без совпадения vocab/tokenizer/материализации. Проверка самого string helper без вызова из train не заменяет учебный опыт. [Карточечный аудит](experiment_audit.md) | P07, P08, P17, P71, P76, P98, P99; BPE-NO-MATCH-COPY-01 |
| Длина входа до начала обучения | Whole-batch validation может остановить допустимые уроки из-за одной слишком длинной строки, до первого update | Уже описано другим агентом и добавлено в N63-ingress-length. Не объявляется новым. Сравнение A0 updates/B2 updates измеряет восстановление проходимости, не equal-compute качество | N63-ingress-length; прежний INGRESS-LENGTH-01 |
| Координатные allocations RMS | Trainer → RmsNorm → два MultiplyBroadcast → FlatToIndices на элемент → backward/update | **NEW: RMS-COORD-ALLOC-01.** Один возможный fix: арифметика broadcast index вместо отдельного int[] на элемент, с сохранением math/backward. Пока NOT_RUN. [Полная карточка](RMS-COORD-ALLOC-01-CARD.md) | Точного опыта в protected нет; добавление подготовлено здесь |
| Другие затраты настоящего update | Gradient clones, softmax scratch, frozen transpose и gradB access order участвуют в native forward/backward учебного шага | Уже известны LF-02/LF-03, NEURAL-FROZEN-TRANSPOSE, OCT20-R2 и опубликованный GRADB-LOCALITY-ENRU-01. Не переименовываются в наши новые находки | N62, NEURAL-FROZEN-TRANSPOSE; [поздний planning пакет][late-planning] |
| Векторная геометрия/AST | Extractor → learned examples → distance/vote. Частоты/длина не эквивалентны ролям, глубине и положению узла | E09 реально читает structural fields, но не все заполняемые поля используются в DistanceTo. Roslyn признаки относятся к коду; для текста нужна отдельная геометрия. [Источники](historical_knn.md) | P16, P26–P28, P61, P103, P113, P122 |
| Порядок и POS | Learner запоминает примеры, SequenceDistance сравнивает множества POS bigrams | Положительный E25 относится к частичному порядку, не полной последовательности. Множество bigrams теряет повторяемость и часть глобального порядка; это известное направление. [E25](historical_knn.md) | P28, P51, P55, P122 |
| Учитель KNN | ActionLabelGenerator.Label → пары features/label → Train → Predict | **N-KU измерен и остаётся кандидатом**: добавить DoStatement в проверку наличия цикла. Показать ложные и пропущенные ADD_LOOP раздельно. [Реестр](candidate-register.md) | N-KU-01 |
| k/votes/target codec | Class balance, duplicates, tie-breaks, k и кодировка labels меняют reader обученной памяти | В N-KU k3 на четырёх строках ограничивает обобщение. В E09 per-file ordinal и reusable SyntaxKind%200 различаются; baseline/enhanced оцениваются на разных панелях. [История](historical_knn.md) | P20–P23, P27.4, P30, P35, P47, P60, P64, P77, P90, P100, P107 |
| «Матрица неудачна → KNN успешен» | Признак может не читаться старой функцией, а затем войти в learned neighbor selection | E07 tier меняет note, composite key остаётся тем же; BPE ordinal/char alignment ошибочен. E09 положительные receipts сохранены отдельно. Буквальное происхождение одного и того же перенесённого алгоритма не установлено. [Разбор по полям](historical_knn.md) | P27, E-MATRIX-KNN, прежние audit12/history |
| Повторный Train и потеря памяти | T1 создаёт пары; следующий sparse T2 проходит ранний gate, очищает память и не восстанавливает пары | KH-EXT-01 — опубликованное уточнение LT-N02; обычный RP01 caller пока не доказан. Не считать отсутствующий caller разрешением на случайный isolated helper benchmark | P36, P74, P91; [имеющийся точный рецепт][method-memory] |
| Стоимость подготовки KNN | ParseText для features + отдельный ParseText для label непосредственно перед Train | KH-COST-01 уже опубликован другим агентом. Возможная экономия общего immutable AST root, NOT_RUN. Не новая гипотеза этого пакета | [Имеющаяся карточка][knn-cost] |
| DPO → subunits и typed memory | Loader → ExecutorOutput.PhraseMappings → TrainOnExample → word/phrase/exact/KNN state → ordinary Flow | Равная pair count не доказывает равный материал Unit. Доставка phrase_mappings дала композиционный gain и уже исправлена в Tools. [Точные методы](historical_knn.md) | P52, P58, P88, P92, P106, P118, P125 |
| Unit → Flow и проверяющий | Нужен actual writer, его state и обычный reader. Request-only checker или receipt не доказывает изменение ответа | P125 содержит настоящий Executor writer и Flow consumer. N-KU проверяет native predictor, не весь Compile/IDE. Не объединять уровни результата | P38–P42, P53–P58, P88, P92–P95, P123, P124 |
| Детерминированная подмена | Fixed rules могут обходить learned component; «passed» может проверять только формат или шаблон | Название KNN/Unit и отчётный PASS не доказывают learning. Исторический E25 имеет Train; соседний context-only E26 не подтверждает приобретение навыка. Детерминированное вычисление внутри реального learner само по себе не дефект | P28, P39, P43–P45, P79, P80, P107, P122 |
| Replay/retention | Новые уроки меняют old memory/weights; порядок/capacity влияет на доступность старых примеров | E43 положителен для frequency memory. Глобальный replay cap уже вынесен в N63-replay-capacity. В N63 дополнительные B updates — цена политики; old retention имеет смысл после доказанного W0→WA acquisition | P15, P24, P48, P59, P67, P116, P119; N63-replay-capacity |
| Restore/identity | Записанный learned state → loader/schema/tokenizer/unit memories → выбранный instance → обычный output | P25 сохраняет конкретную память. Четыре vs шесть translation directions и Orchestration schema1 rejection остаются другими seams. Не ослаблять provenance как «исправление». Ниже exact source | P18, P22, P25, P53, P59, P117 |
| Orchestrator/DynamicGraph | Учебные отношения должны менять используемое состояние, а не только фиксированный порядок ветвей или графовую структуру отчёта | В этом проходе нет нового runtime доказательства приобретения graph/orchestrator навыка. Требуются producer→state→consumer и конкретный домен; ручной route не заменяет этот путь | P32–P42, P50, P69, P70, P94–P97 |
| Измерение полезного результата | До/после одного состояния, равные exam cases, корректные знаменатели и равные timer regions | Память, ответы, train loss, preference margin, производная, время и allocations — разные полезные метрики. Любой измеренный gain признаётся по своей оси; побочные потери сохраняются | P03, P57, P66, P82, P91, P102, P109, P112 |

## DPO: что именно доходит до учебного получателя

Подтверждено повторным чтением текущего **65d03a73** и donor **219d7ba89dc3ef43e03e17f1461ad96154d3dd7a**.

В donor CodeTraceBuilderTrainingPipeline.ToDpoMetricPair передаёт PairId, pair.A, pair.Chosen, pair.Rejected. Current передаёт PairId и три значения, построенные только из RejectedType. Prompt содержит тип дефекта, chosen/rejected — повторённые PREFERRED/REJECTED tokens, CHOSEN_IS_CORRECT и FINAL_PASS/FINAL_FAIL. [Donor builder][wiki-old-builder], [current builder][wiki-current-builder].

Donor ProjectWikiRequestRoutingTrainingPipeline передаёт настоящий Executor prompt и оба содержательных ответа. Current ToDpoMetricPair также строит их из rejected-type и признаков исправления/ошибки. [Donor router][wiki-old-router], [current router][wiki-current-router].

**Причинная гипотеза:** DPO способен улучшать различение искусственной метки без обучения исходному содержательному выбору. Это не доказательство того, что все DPO всех доменов построены так, и не чистый runtime A/B против donor.

**Точный адрес возможного исправления:** названные два ToDpoMetricPair. Experimental B должен сохранять действительные запрос/chosen/rejected, а не просто удалить слово WRONG и оставить остальные маркеры. Остальные параметры и train/exam families должны совпадать. Это существующее направление, а не новая самостоятельно запускаемая карточка данного пакета. Подготовку host и конкретных данных нельзя переложить на будущего исполнителя.

Наличие преобразования показано в [primary-source-excerpts.json](primary-source-excerpts.json) вместе с source SHA и диапазоном строк; этот файл содержит прочитанные выдержки исходников, а не экспериментальный результат.

## Восстановление: зависимость от результата обучения

### Translation

Tracked translation-v1-checkpoint.json имеет schema_version1 и ровно четыре executors: RU_EN, EN_RU, HE_EN, EN_HE. Это повторно прочитанный JSON текущего **65d03a73**. Current TranslationMachine создаёт также ES_EN и EN_ES и проверяет наличие каждого ожидаемого направления при preflight. [Checkpoint][translation-checkpoint], [runtime и preflight][translation-machine].

Следствие относится к совместимости конкретного старого обученного файла с новым reader. Недопустимо записать новые пустые executors и заявить, что сохранены или восстановлены ранее выученные испанские навыки. Возможный будущий fix должен явно определить миграцию старых направлений, не изобретать обученное состояние и проверить обычный loaded consumer.

P25 не опровергает этот seam: он проверяет другое конкретное новое состояние и даёт 24/24 сохранённых наблюдений с 19 правильными. Для зависимого restore-опыта повторное обучение ради формальности не требуется, если происхождение и поведение изученного checkpoint доказаны. [Cold evidence](historical_knn.md).

### OrchestrationReasoning

Текущий tracked manifest имеет schemaVersion1 и ссылается на orchestration-reasoning-v1-checkpoint.json. Reader явно отклоняет common schema1 с требованием provenance rebuild. Обычный XiigenExtensionApiService.LoadCommonAssets пытается загрузить manifest target; при неуспехе оставляет первоначально созданный instance и записывает asset_loaded=false, error code и warning. [Manifest][or-manifest], [reader][or-reader], [ordinary consumer][or-consumer].

Следовательно, обучение исторического asset и фактическая доступность его навыков в current API — разные утверждения. Ошибка не полностью молчаливая; код не позволяет утверждать, что старый asset успешно загружен. Предлагаемое направление исправления — подготовить совместимый asset с проверяемым learned-state/provenance и согласованным manifest, затем проверить ordinary consumer. **Отключение проверки происхождения не предлагается.**

Эти наблюдения не являются общей рекомендацией немедленно менять restore во всех доменах. Это существующие learning-dependent candidates после первичных блокеров обучения, с указанными оригинальными loader/consumer.

## Доменные границы

- **C# и структурный код:** Roslyn AST, типы узлов, parents, roles, положение в методе и учебные labels цикла. Перенос на другой язык программирования требует соответствующего parser/schema.
- **Перевод и языковая структура:** POS/order, отрицание, направление, morphology, phrase/word/exact maps и композиция. POS classification не равна качеству всего переведённого текста.
- **ProjectWiki:** содержательные запросы, маршруты и code traces; marker discrimination не заменяет качество ответа по исходному запросу.
- **Общая нейронная стоимость:** RMS/MatMul/gradient allocations могут повторяться в нескольких доменах, но размер эффекта зависит от shapes, длины и learner. Первый измеренный домен фиксируется; перенос за него не предполагается.
- **Orchestrator/Graph:** графовая корректность вне связи с обучаемым состоянием остаётся отдельной архитектурной работой последнего приоритета.

[late-planning]: ../2026-10-20-learning-candidates-and-causal-gaps/README.md
[knn-cost]: ../2026-10-20-learning-candidates-and-causal-gaps/knn-cost-card.md
[method-memory]: ../2026-10-20-learning-candidates-and-causal-gaps/method-memory-card.md
[wiki-old-builder]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L119-L120
[wiki-current-builder]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L124-L208
[wiki-old-router]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L110-L116
[wiki-current-router]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L131-L167
[translation-checkpoint]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/models/translation/translation-v1/translation-v1-checkpoint.json
[translation-machine]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/27.TranslationDomain/TranslationMachine.cs#L390-L431
[or-manifest]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json
[or-reader]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L971-L1079
[or-consumer]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1403-L1445

