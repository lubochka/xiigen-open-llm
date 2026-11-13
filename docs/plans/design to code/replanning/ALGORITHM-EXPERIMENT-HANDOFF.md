# Из находок R04–R06 — в ближайший алгоритм и различающий опыт

**13.10.2026 · INTEGRATED6 · редакторский checkpoint 1.** Это применённая передача в план, не выполнение R07–R25 и не отчёт об обучении. Результаты R01–R06 не изменяются. Документ задаёт адресные обязательства для действующих model packets; не создаёт новую очередь или обязательный R26.

## 1. Что действительно изменяет следующую работу

Прочитаны опубликованные FINDINGS R03 CP08, R04 предметный результат, R05 CP1 и R06 checkpoint B. Это **приём исследовательских результатов**, не повторное независимое чтение всех перечисленных ими C# файлов. Для зависимого решения исполнитель проверяет соответствующий primary span по SOURCES своего источника. Read/checkpoint и эмпирическое доказательство различаются.

| Переданный источник | Сохраняемый смысл и ограничение | Конкретный consumer в плане |
|---|---|---|
| [R04-F01–F03](results/R04/FINDINGS.md) | Raw text, canonical text и TeachingVector — разные каналы. Нормализованный канал может терять отрицание, числа и нелатинские символы; raw может сохраняться другим путём. Локальный root vocabulary 60-мерного TeachingVector не равен BPE-пространству kNN. Конечная ошибка ещё не измерена. | R07 проверяет только реально используемый канал; R12 выбирает одну representation delta; R13/E1 измеряет смысл на обычном consumer; R16 переносит требование; R18 переиспользует имеющийся sharedRootVocab overload, только если его применение обосновано. |
| [R04-F04 / R06-F07](results/R06/FINDINGS.md) | KnnLogicalUnit сохраняет обученный BPE и typed examples, но возвращает Output первого hit; остальные дают evidence IDs. Это не агрегатор k выходов. ANN может менять top-1 при изменении поиска — отдельный эффект. | R06 продолжает только остаток; R07 показывает потребление соседей; R12 выбирает, нужен ли learned reranker/aggregator; R13/E2 отделяет retrieval recall от использования соседей; R18 не начинает с переписывания HNSW. |
| [R05-F01–F04](results/R05/FINDINGS.md) | DpoTrainer уже имеет policy/reference, backward и AdamW. Training Accuracy — pre-step margin, не внешнее качество. TeachingVector в StepVector масштабирует сигнал, не автоматически целевой вектор. LoraLinear — low-rank transform, не tokenizer. | R12 выбирает фактический recipient/objective; R13/E3 отделяет память от parameter update; R18 чинит missing seam существующего learner→ordinary consumer; R22 проверяет projection отдельно от pair-signal weighting. |
| [R06-F08–F11](results/R06/FINDINGS.md) | Есть trusted edge-authority resolver, реальный вызов L2 и отдельный L3 executor; DynamicGraphMachine lifecycle имеет NOT_ACTIVATED в прочитанном пути. Exact index существует как in-memory reference, не полный persistence bundle. | R07 ограничен trained state→preload и L2 decision→L3 association; R11 не заказывает весь runtime заново; R18 сохраняет reusable parts; R24 проверяет реальный host отдельно. |
| [R06-F13–F14](results/R06/FINDINGS.md) / [R03-F02–F04](results/R03/FINDINGS.md) | Нулевая DPO-margin не означает нулевой gradient. Есть предметный конфликт historical helper-ban с разрешённым hybrid. One-pair/no-repeat не заменяется replay70/30. | R12 принимает адресное решение по двум точным нормам; R16 применяет его в разрешённых документах, а вне области оставляет exact patch+owner. Не объявлять runtime defect по одному тексту старого плана. |

## 2. Четыре сохраняемые роли — четыре разных предмета оценки

**Unit.** Принимает domain, строение входных векторов и запрошенного выхода; возвращает связанные typed many→many results. Наблюдение: сохраняются смысл, input/output association, lawful alternatives и one→zero. Сам по себе список похожих строк не доказывает контракт.

**Flow.** Определяет связи Units и коррекцию входа в выход. Наблюдение: те же корректные Unit outputs при изменённой relation дают более правильный downstream result. Замена Unit одновременно с Flow не позволяет приписать выигрыш связи.

**Orchestrator.** Асинхронно выбирает и соединяет допустимые пути. Наблюдение: качество результата и вся цена, включая проигравшие, отменённые и просроченные ветви. Первое завершение и короткий путь не равны лучшему решению.

**DynamicGraph.** Выражает отношения паттернов разных порядков. Наблюдение: перенос на новые композиции уже известных primitives, с контролем запоминания whole-path. Graph остаётся L5 specialization; обучение принадлежит ILearningMachine. Не создавать четыре trainer и шестой уровень. Unit-level recipient допускает обучение edge-authority Unit, но это надо показать реальной адресацией.

## 3. Один паспорт алгоритма, а не слово «hybrid»

R07/R12 используют следующую запись внутри существующего FINDINGS/DECISIONS, не отдельный ledger на каждый узел:

`expectation/source → input domain+intent+stack+project → deterministic constraints/helper → learned decision → state owner → signal/objective → parameter or memory mutation → ordinary consumer → intervention → semantic outcome + cost → falsifier`.

Для связи дополнительно: producer/consumer methods, input/output type и значение, schema/tokenizer/codebook/projection versions, кратность, semantic order, fan-in association, timeout/cancellation, state visibility, rollback boundary. Совпадение shapes не совместимость значений. Входы, выходы и provenance не смешивать с финальным quality score.

Domain pool определяется допустимостью: права, project/tenant, schema, language/stack/dialect при предметной необходимости. Затем изучаются features, scoring, k/radius, memory, reranking/aggregation и final arbiter. Learned membership не ослабляет права. Unknown/mixed domain имеет abstention или обоснованный multi-domain route.

Предлагаемое семейство, **не выбранный optimum**: `s(d,q,n)=Σ w_j(d,q)·φ_j(q,n)`, с предметным смыслом/масштабом каждого φ, ограничениями w, learning signal и отдельным k policy. Query-dependent score может не соответствовать metric используемого ANN. Для малого допустимого пула сначала сравнить с существующим exact reference; production threshold и budget выбираются в R12/R13, не угадываются здесь.

## 4. Четыре готовых поручения для опытов

Это детальные задания **на проектирование и затем отдельное разрешённое исполнение**. R13 выбирает один первый опыт; R22 расширяет по обнаруженной причине. Не требовать все четыре перед первым уроком.

### E1 — сохраняет ли representation предметный смысл?

**Контекст/владелец:** R04-F01–F04 → R07 actual consumer → R12 representation choice → R13 recipe. Skills: code-examination, trainable-architecture-decision, experiment-design, improvement-measurement.

**Промпт.** Возьми одну разрешённую семью запросов и её lawful near misses. Проследи raw/canonical/vector отдельно до того consumer, который действительно принимает решение. Сравни исходное и ровно одно изменённое представление при одинаковых solver, памяти, candidate pool и admissible laws. Старые stored vectors должны соответствовать своему encoder; при изменении пространства явно учти re-encoding и reindex cost. Не называй несовместимыми все векторы по одному TeachingVector.

**Подзадачи:** A — зафиксировать channel/consumer и какие различия он видит; B — заполнить pair/contrast rubric и paired comparison. Реальное делегирование A/B требует полного этого контекста и собственной выдачи; дополнительные calls учитываются.

**Хороший пример:** «не удалять 10 строк» против «удалить 100 строк»; оценить action, negation и quantity, а не длину текста. **Плохой:** одинаковый размер vector объявлен смысловой эквивалентностью. **Край:** RU/HE, SQL NULL, одинаковый термин разных стеков, unknown schema.

**Выход/приёмка:** exact channel map, frozen factors, semantic metric, source-family split, cost, competing explanation и falsifier. Будущее наблюдение должно различать смысловую потерю, ошибку routing и downstream solver; одно изменение hash не принимается.

### E2 — помогают ли несколько соседей решению?

**Контекст/владелец:** R04-F04/R06-F07/F11 → R07 neighbor consumer → R12 retrieval/aggregation choice → R13 или R22. Skills: architecture, code examination, measurement.

**Промпт.** Раздели два вопроса. Сначала сравни exact/ANN на одном pool и vector space: это качество нахождения соседей. Затем при фиксированном наборе кандидатов сравни текущий first-hit consumer с выбранным reranker/aggregator: это использование соседей. Только после этого исследуй adaptive k/radius как отдельный фактор. Не заявляй, что повышение k само по себе учит объединять outputs. Если top-1 одинаков и текущий consumer копирует его, объясни, почему изменение только остальных evidence IDs не доказывает улучшение ответа.

**Подзадачи:** A — outcome influence каждого hit; B — один controlled contrast с полным search/rerank cost.

**Хороший пример:** SQL-кандидат похож лексически, но использует другой dialect/NULL semantics; допустимый альтернативный neighbor помогает корректному выбору. **Плохой:** больше neighbors/evidence IDs = лучше learning. **Край:** k>|pool|, empty pool, конфликтующие positive/negative/warning, required skill вне top-k, stale index, zero norm.

**Выход/приёмка:** separate retrieval relevance/recall, consumer semantic accuracy и latency; proposed dynamic k имеет owner/signal/budget и baseline. Exact retrieval — не semantic oracle. Не делать полный Cartesian sweep.

### E3 — чему действительно учит DPO/LoRA?

**Контекст/владелец:** R05-F01–F08, его дальнейший remainder → R12 owner/objective → R13 → R18. Skills: trainable architecture, code examination, experiment design, prompt/context design.

**Промпт.** Зафиксируй один context, exact chosen/rejected bytes и их независимую assessment, recipient и actual policy/reference. Chinese author создаёт обе final sides; CODEX Astra выбирает урок и оценивает точную пару. TIE/BOTH_BAD/UNJUDGEABLE не превращай в выдуманное предпочтение. Проследи pair→admission→existing trainer→конкретный изменяемый state→publication/load→обычный ответ. Для LoRA назови точную матрицу, freeze/masks, representation consumer и что именно оптимизируется. TeachingVector weighting не выдавай за vector-output training. Для probability-based DPO сохраняй objective; для иной pairwise/vector цели явно укажи отличие.

`m=β[(logπθ(y+|x)−logπref(y+|x))−(logπθ(y−|x)−logπref(y−|x))]; L=−logσ(m)`.

При policy=reference m=0; dL/dm=−1/2. Это не гарантия ненулевого parameter gradient или полезности пары, но опровергает универсальное требование «m=0 ⇒ learning невозможен». Не требовать изменения каждой LoRA matrix на каждом шаге.

**Подзадачи:** A — pair/recipient/state/consumer trace; B — no-update control versus one authorised new lesson при одинаковой памяти и контексте. Первое поручение implementation чинит минимальный seam, не создаёт второй trainer.

**Хороший пример:** пара про выбор solver обучает selector owner, не winning solver по совпадению ID. **Плохой:** pre-step Accuracy, loss или adapter hash объявлены качеством домена. **Край:** identical sides, all-token masking, zero Jacobian, stale reference, interrupted publication, loaded old adapter.

**Выход/приёмка:** exact first-missing-seam packet, no-update/memory controls, independent semantic outcome, retention and full cost. Без опыта — NOT_MEASURED.

### E4 — чему учатся связи, асинхронность и higher-order patterns?

**Контекст/владелец:** R03-F01/F17, R06-F08–F10 → R07 → R12/R13/R22. Skills: architecture, code examination, measurement, readable audit.

**Промпт.** На одной задаче сравни frozen Units/changed Flow и fixed relations/changed Unit. Для Graph используй новые композиции знакомых primitives и отрицательный пример неправильного порядка зависимостей. Для Orchestrator отдели presentation order от semantic order, качество выбора от scheduler timing, учитывай все branches. Начни с двух известных неразрешённых стыков R06 — trained state→trusted preload и L2 decision→L3 association; не заказывай весь runtime заново. Joint change разрешай только после явной interaction hypothesis.

**Подзадачи:** A — semantic port/recipient map; B — controlled relation/route/composition contrast.

**Хороший пример:** Units возвращают правильные количества, Flow связывает их с неправильными сущностями; исправление association повышает качество без смены Units. **Плохой:** короче path или больше graph nodes = лучше reasoning. **Край:** cycle, timeout, несколько равноценных lawful paths, missing skill, partial fan-in, отменённая ветвь с побочным эффектом.

**Выход/приёмка:** state/credit recipient, lawful partial order, independently described expected result, ordinary consumer, quality and total resource cost. Нельзя объявить все четыре уровня обученными по одному successful route.

## 5. Общая измерительная дисциплина

Bulk — последовательность **новых** разрешённых уроков при действующей one-pair/no-repeat policy, а не повторная оптимизация старых пар. Для каждого bulk оцениваются incumbent и challenger на одной task panel/rubric; новая трудность следующего bulk не смешивается с изменением модели. Family, а не строка/paraphrase, — единица независимости.

Разделить: dataset/exact-pair quality; optimizer eligibility; факт update; ordinary-consumer adoption; semantic gain; retention; PC efficiency. Планировать blinded/randomized presentation оценщику, не менять semantic step order. Operational panel может переиспользоваться, но больше не называется untouched certification; closed families/answers не входят в teaching context. Feedback пользователя сначала проверяется на права/смысл/конфликт/новизну, не становится truth автоматически.

R13 заранее задаёт primary metric, practical effect, uncertainty/stopping, tolerated regression и budgets. Нет утверждённого promotion threshold — exploratory conclusion, не promotion. Verdict: IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE. Порог не подгоняется после результата; неизвестное не считается PASS.

Полная цена: base/runtime/encoder/codebook/adapters/optimizer/index/graph/cache/snapshots; peak process RAM/VRAM, disk, обучение, поиск, reindex и все branches. GC current-thread allocated bytes не peak process RAM. 95%, 20× и 2MB остаются goals/historical claims, не текущими измерениями.

R15/R25 требуют реального отдельного Astra Ultra reviewer exact candidate, actual invocation identity и предметных контрпримеров. Этот редакторский checkpoint не такая экспертиза. Не запускать product tests/coverage/Windows/Actions или обучение для приёмки этих документов; существующие проверки не отключаются.

## 6. Методические первоисточники

Используются только как основания формулы/метода, не доказательство эффективности XIIGEN: Rafailov et al., DPO, https://arxiv.org/abs/2305.18290 ; Hu et al., LoRA, https://arxiv.org/abs/2106.09685 ; Faiss, MetricType and distances, https://github.com/facebookresearch/faiss/wiki/MetricType-and-distances . Проверены 13.10.2026. Ни одна библиотека этим документом не назначается новым dependency.
