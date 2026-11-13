# 05 · RP02: промежуточные результаты предметной ревизии

**15.10.2026 · INTERMEDIATE / AUTHOR_SOURCE_REVIEW.** Опубликовано по прямому поручению пользователя «Please publish middle artifacts and then continue». Это наблюдения и конкретные следующие правки, **не заявление, что RP02 уже исправлен или обучен**. Продолжение должно редактировать реальные execution carriers, а не добавлять ещё одну обязательную исследовательскую очередь.

## Зафиксированная база

Planning P: `78ccad8763adc9fd6bf6499f27260c2e9c4c8bab` (`llm_plan3`). Core K, повторно прочитанный ref: `97e4af3eadd3981320f698d104134a01984a2542` (`codex/dpo-implementation`). Старые материалы доступны по этим SHA независимо от дальнейших изменений. Ни один кодовый дефект ниже не объявляется runtime-воспроизведённым.

RP02 = `replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack`; sibling Retrieval — reference-only. R01–R25 не перезапускаются; результаты и владельцы остальных слайсов сохраняются.

## Подтверждено из прежнего атласа

1. **Разные dispatcher-версии.** `replanning/AGENTS.md` начинается с ACTIVE V6A / 17 cards; `RP02/PACKAGE-MANIFEST.json` задаёт V7.2 / 18 ID / tasks-v7. В самом AGENTS ниже сохраняются общие указания original tasks и RP01 V4. Исправление должно устранить действующее противоречие, а не добавить ещё одну шапку. Источники: [AGENTS на P](https://github.com/lubochka/xiigen-planning/blob/78ccad8763adc9fd6bf6499f27260c2e9c4c8bab/llm%20mvp/design%20to%20code%20session/replanning/AGENTS.md), [manifest на P](https://github.com/lubochka/xiigen-planning/blob/78ccad8763adc9fd6bf6499f27260c2e9c4c8bab/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/PACKAGE-MANIFEST.json).
2. **Многослойные карточки.** В просмотренных tasks-v7 одновременно присутствуют V7, V7.1 и V7.2 prompts, дублирующиеся STOP/RETURN и требования к ненужным данной фазе данным. Для ограниченной модели это не один effective prompt. Пример: [02B1 на P](https://github.com/lubochka/xiigen-planning/blob/78ccad8763adc9fd6bf6499f27260c2e9c4c8bab/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.02B1.md). Нужна замена содержимого каждого действующего packet с сохранением уникальных требований, а не V7.3 append поверх трёх старых.
3. **Направление learning-first уже существует.** V7.2 уже запрещает coverage как доказательство обучения, допускает 0 новых product tests и начинает θ-урок без Windows/full-suite/всех доменов. Этот ремонт не должен приписывать себе изобретение этих правил. Сохранить минимальные адресные проверки реально изменённых прав, типов, конечности update, once-journal и cold-load.
4. **kNN действительно не просто top-k count.** Полностью прочитан `KnnLogicalUnit.cs` на K: `ExecuteAsync` получает Search(query,_k), но выбирает `hits[0]`; прочие hits идут в evidence IDs. `Vectorize` суммирует token/predicate buckets, явной структуры порядка/референтов в этой функции нет. Это утверждение об одном consumer, не обо всех kNN в проекте. [Код на K](https://github.com/lubochka/llm_mvp_core/blob/97e4af3eadd3981320f698d104134a01984a2542/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs).

## Дополнения и уточнения к списку ремонтов

### RP02-F17 · Лишняя междоменная зависимость
**DOCUMENT_CONFLICT_VERIFIED.** Board и заголовок 01D требуют `00B2 sources`; S-подпромпт требует одновременно RU и C# passports, тогда как V7.2 разрешает C# **или** RU first pilot с минимальными признаками одного домена. [01D на P](https://github.com/lubochka/xiigen-planning/blob/78ccad8763adc9fd6bf6499f27260c2e9c4c8bab/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.01D.md).
**Правка:** dependency = один выбранный готовый domain passport + конкретный consumer; C#/RU/SQL расширение обязательно для соответствующего multi-domain claim, но не universal first-update gate. Не превращать optional-before-first-lesson в optional-product-functionality.

### RP02-F18 · Слепой reviewer получает раскрытую принадлежность arm
**DOCUMENT_SCHEMA_DEFECT_VERIFIED.** Workbook содержит `blinded_arm: P|C0|T`. Это не blinded label, если поле передано судье. [Workbook на P](https://github.com/lubochka/xiigen-planning/blob/78ccad8763adc9fd6bf6499f27260c2e9c4c8bab/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/RP02-V7.2-FUNCTIONAL-EXAMS-AND-TRACEABILITY.md).
**Правка:** reviewer получает случайные opaque arm IDs и порядок ответов; скрытый mapping P/C0/T хранит curator. Раскрытие только после зафиксированных verdicts. Разделить curator ledger, reviewer payload и публичный aggregate; не публиковать SEALED answers.

### RP02-F19 · Update, adoption и gain смешаны
**DOCUMENT_OVERCONSTRAINT_VERIFIED.** Runbook описывает первый learning point как одновременно update и изменение обычного ответа; 02B2 считает неизменившийся ответ отсутствием полезного эффекта. Между тем θ может обновиться и реально читаться, не изменив argmax; динамический поиск может дать тот же правильный результат дешевле.
**Правка:** отдельные статусы `UPDATE_OBSERVED`, `STATE_CONSUMED`, `OUTPUT_CHANGED`, `QUALITY_VERDICT`, `COST_VERDICT`, `PROMOTED`. Не подгонять update до обязательного flip. Не объявлять gain по hash. Для compactness разрешить доказанную неухудшенную точность при меньших затратах по заранее определённым условиям; отсутствие значимости само по себе не доказывает non-inferiority.

### RP02-F20 · Не определена стоимость наблюдений для dynamic k
**DESIGN_GAP_IN_REVIEWED_PACKET.** 02B2 предлагает выбирать k по margin/density/entropy, но не выделяет предварительный поиск, который эти значения получает. HNSW использует отдельно `topK` и `efSearch`: уменьшение количества возвращённых записей не обязательно уменьшает фактическую работу. [02B2 на P](https://github.com/lubochka/xiigen-planning/blob/78ccad8763adc9fd6bf6499f27260c2e9c4c8bab/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.02B2.md), [HnswIndex на K](https://github.com/lubochka/llm_mvp_core/blob/97e4af3eadd3981320f698d104134a01984a2542/06.VectorSearch/HnswIndex.cs).
**Правка:** различать pre-query features, оплаченный bounded probe, search effort/ef, recall candidate count и consumed k. Учитывать обе фазы и feature extraction в end-to-end cost; не использовать ещё не наблюдённый margin для выбора первого поиска. Learned-k и heuristic-k — разные claims, θk не смешивается с θ-ranker.

### RP02-F21 · Уточнение ошибки нормализации ID, без ложного обвинения Trim
**CURRENT_CODE_VERIFIED.** В Train `trimmedId` используется **и** в dictionary, **и** в `index.Add`; простое добавление/снятие пробела не даёт разных ключей между этими двумя строками. Риск другой: две записи `" A "` и `"A"` создают два index nodes с одним ID, а dictionary сохраняет последний payload. В `HnswIndex.Add` в просмотренном методе дедупликации нет.
**Правка:** находку называть normalized-ID collision / payload alias, не Trim mismatch. Если этот seam меняется — до первой вставки проверить уникальность canonical IDs либо явный согласованный update policy. Не строить новый общий валидатор только ради планового документа. Runtime reproduction NOT_RUN.

### RP02-F22 · SOURCE_UNVERIFIED не означает доказанный missing caller
**DOCUMENT_STATUS_DEFECT_VERIFIED.** 00A одновременно требует source-only и формулирует NOT_WIRED при любом не доказанном edge. Не найденный при ограниченном чтении caller не равен доказанному отсутствию registration.
**Правка:** `SOURCE_TRACED`, `CALLER_UNVERIFIED` и `NOT_WIRED_CONFIRMED` отдельно. 00C разрешать только по конкретному отсутствующему edge с bounded search receipt; не создавать второй bridge из-за неполного поиска. Source-only не должен требовать actual runtime output; runtime observation отдельно.

### RP02-F23 · Negative memory нельзя свести только к пассивной диагностике
**PRIMARY_SEMANTIC_CLARIFICATION.** F/ARBITER-KNN первоисточник различает positive (what to produce), negative (what not to produce), warning (risk/extra arbitration). Runbook фраза diagnostics-only может чрезмерно сузить negative-role learning. [Первичный F документ](https://github.com/lubochka/xiigen-planning/blob/78ccad8763adc9fd6bf6499f27260c2e9c4c8bab/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md).
**Правка:** negative-role record может давать разрешённый contrastive/penalty сигнал, но не становиться accepted positive output. Различать права на данные, роль записи, предметную ошибку и финальный arbiter. Чужой tenant запрещён для serving и не становится доступен через negative channel. Ошибочный Chinese rejected в законной TRAIN DPO-паре допустим как отрицательная сторона; это не разрешение выполнить его.

### RP02-F24 · Requested model и runtime ID не совпадают по определению
**DOCUMENT_CONFLICT_VERIFIED.** Manifest содержит `choice: CODEX gpt-6-astra ultra only where applicable/approved`, тогда как действующее пользовательское требование — **CODEX с astra 6 ultracode** в выборе и отдельной оценке exact pair.
**Правка:** один точный requested label для selector и judge; actual runtime model ID и invocation receipt — отдельные наблюдаемые поля. Не придумывать API alias. Внешняя открытая китайская модель пишет ОБЕ финальные стороны; CODEX не переписывает их под Chinese provenance. Independent code/science reviewer — отдельный реальный вызов.

### RP02-F25 · Similarity confidence не является калиброванной вероятностью
**CURRENT_CODE + DESIGN_RISK.** `KnnLogicalUnit.ExecuteAsync` округляет/clamp best.score в confidence; confidence floor создаёт warning, а не отказ. Это не доказательство calibrated P(correct), и warning не равен hard rejection. Использование такого числа как вероятности correctness/полезности k требует отдельного измерения, не переименования.
**Правка:** сохранить score semantics, проверить фактический caller обработки warning, измерять risk/coverage и calibration на OP_DEV отдельно для доменов; не вводить универсальный magic threshold. При отсутствии наблюдений — UNKNOWN, не fake confidence.

## Что именно будет изменяться дальше

Пакет RP02: действующие task packets, board, start, manifest и научный протокол — согласованно, с одним effective prompt на ID и явными дочерними фазами; затем узкий RP02 dispatcher в родительском AGENTS. Никаких новых 25 раундов, Windows runs или продуктового coverage. История сохраняется по pinned P/Git; уникальные требования к domain mapping, vector/token semantics, learned reranking, dynamic k/radius, next-role/AST, LoRA projection, continual bulks, full-PC и четырём уровням архитектуры не исчезают.

## Граница уже выполненного чтения

Прочитаны текущие Atlas README/01/04 (01 — двумя участками с границами), RP02 manifest/runbook/workbook/README/board/source passport; substantive phase/exam/failure sections 00A,00C,00B1–B3,01A–D,02A,02B1–B2,02C–D. На момент этого промежуточного commit 03A/03B/03C/04A ещё не заявляются прочитанными. В больших карточках чтение выделенных участков не приравнивается FULL_READ.

Прочитаны original task/task prompt/goal; WorkLeft MASTER/usage/MECH-01/RESEARCH-03 активные начала; previous rounds opening/R07 correction; JOIN начальные owner/concurrency sections; V/RAG-06 sections → F/ARBITER-KNN primary §§Decision–Priority Boundary. Полная NDJSON сверка всех RAG НЕ выполнена. В V tree для RAG-06 нет одноимённого NDJSON: нельзя выдумывать файл по шаблону; нужный matching index следует брать из действительного дерева WorkLeft или явно отметить отсутствие.

Шесть requested skills действительно открыты по относящимся начальным секциям: trainable-architecture-decision, phase-planning, plan-review, generated-code-review, prompt-patch-authoring, conversation-intent-and-self-reflection. Исторические 30/50-pass и старые model требования внутри skills не создают новый барьер данному прямому learning-first ремонту.

**Статус этого commit:** `INTERMEDIATE_FINDINGS_PUBLISHED; RP02_REPAIR_IN_PROGRESS; CORE_UNCHANGED; PRODUCT_TESTS_ADDED=0; WINDOWS_NOT_RUN; TRAINING_NOT_RUN; DOMAIN_GAIN_NOT_MEASURED; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED`. Следующий author/source review не является независимой Astra экспертизой.
