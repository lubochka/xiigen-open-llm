# R06 — домены, dynamic kNN и обучаемые пути графа

**13.10.2026 · checkpoint C · R06-REPAIR-1.** Проверены и исправлены вывод о ближайшем учебном переходе и действующие передачи. Исследование checkpoint B сохранено с его source pins/границами; это не повторный полный аудит R06. Документный ремонт не означает product acceptance, обучение или независимую Astra-экспертизу. Эффективность предложенных изменений — **NOT_MEASURED**.

Начальный checkpoint A опубликован commit `8765d98c882df7da184b67f06905e2d2ad02f8a1`; F01–F14 checkpoint B сохранены ниже. Новая дельта — F15–F18 и §8. Способы сравнения и промпты передачи — [DECISIONS.md](DECISIONS.md); первоисточники прежнего исследования — [SOURCES.csv](SOURCES.csv); fresh review locators — [RUN-LOG.md](RUN-LOG.md), раздел R06-REPAIR-1; адресные изменения — [CHANGE-MAP.csv](CHANGE-MAP.csv). Действующий самостоятельный вход — [R06-PARALLEL6](../../run-prompts/R06-PARALLEL6.md).

## 1. Вывод для следующей реализации

**Нельзя заново строить весь graph runtime или DPO trainer.** Уже имеются точный и приблизительный vector search, память примеров, корпусный BM25, resolver обучаемой edge-authority, вызов L2-юнита и отдельный реальный L3 flow executor. Одновременно эти части не доказывают, что конкретный разрешённый урок изменяет нужное состояние, а обычный запрос использует именно его.

**Поправка C к прежнему приоритету:** поздний receipt R06-L02 учитывал только R05 CP1. R05 CP2 выявил потерю смысла между оценённой парой и optimizer input, а также отличие текущего scorer от response-conditional objective. Эти узкие участки повторно прочитаны при ремонте. Поэтому остаток до первого DPO-опыта нельзя описывать только как «checkpoint → L2 → L3». Для выбранного существующего learner сначала нужно сохранить смысл exact pair и согласовать objective, затем доказать получение обновлённого состояния обычным consumer. Это адресный R05/R18 seam, не заказ повторить исследование trainer в R06.

**Два пути не смешиваются.** Для первого CodeDocumentation урока достаточно соответствующего действительного learner→обычный document consumer; неиспользуемый DynamicGraph/L3 не становится обязательным prerequisite. Для опыта domain/path/graph policy нужно дополнительно показать, что обновляется именно selector/отношение, а не выигравший document solver, и что обновлённое состояние реально используется L2/L3. Произвольные LlmModel weights нельзя объявить состоянием memory-based KnnLogicalUnit или совместимой edge-authority по одному ID.

R07 получает только оставшиеся конкретные recipient/ordinary-consumer соединения выбранного пути. R12 выбирает владельца/архитектурную дельту по R04/R05/R06; R13 определяет первый измеримый опыт; R18 переиспользует уже локализованный R05 token/objective packet. Полное изучение domain metrics, encoder experiments и Windows release не ставится перед первым допустимым уроком. Независимое memory/retrieval исследование не ждёт исправления DPO или DONE R05.

Для малого допустимого пула сначала использовать уже имеющийся точный поиск как диагностическую опору. Оптимизация HNSW и dynamic k не должна предшествовать пониманию, **как найденные соседи изменяют решение**. В нынешнем KnnLogicalUnit ответ берётся из примера первого hit, остальные hits добавляют evidence IDs. Это подтверждено свежим чтением класса, но выигрыш нового consumer ещё не измерен.

## 2. Снимки и область

- **P0 исследования B:** `lubochka/xiigen-planning@0d7ccfac70f189ceb618a6da5c499bd2351beed4` — предметные документы, R03 checkpoint, WorkLeft и S07–S10 карта.
- **K исследования B:** `lubochka/llm_mvp_core@dcdded6bb6039c03e80a2786312435350894a4ee` — прежний код и skills.
- **P1:** `7bddddf7d75aeb0bf59bbbd482b103f70011e6d9` — адресная управляющая дельта PARALLEL6; R01–R06 не ждут друг друга, пишут свои shards.
- Перед финальной публикацией B наблюдался HEAD `66b98d1f375f0de9b5bbb708c44770ebda09e467`; собственный A FINDINGS blob оставался `35c505cc0d4337df8fb11a9dbb15bce1eb2afb6b`.
- **P ремонта C:** `64a5e745eecce129d2a52a0a1ac307da22e248da`; R06 B FINDINGS blob `82a57d40a5fd980b05c621effb6b72aa14391667`, R05 CP2 FINDINGS blob `e1357f5845197dfd1f58835b75ad9267896ce721`.
- **K ремонта C:** `3c7f5f8c7809936e25579603c96ea6df8598fc40`. Проверены actual KnnLogicalUnit и адресные BuildBatch/Compact/StepVector/scorer spans. Их blobs совпадают с прежним K; это не утверждение, что весь core не изменился. Compare показывает и другие изменения, не принадлежащие этому ремонту.

Это адресное чтение первичных решений и кодовых тел, а не полный аудит каждого S07–S10 task/scenario или всех внешних репозиториев. R03 CP08 принят только в перечисленном ниже смысле; его неполное исследование не объявлено завершённым. R05 CP2 остаётся OPEN. Результаты соседей не переписываются и не получают DONE от R06.

## 3. Что обнаружено в коде: сохранённые F01–F14

Источники K01–K25 означают `SRC-R06-Kxx` в SOURCES.csv. Их чтение относится к checkpoint B, если ниже не назван fresh locator ремонта. Имена классов и flags не используются как доказательство обученности.

| ID | Источник и наблюдение | Классификация и следствие |
|---|---|---|
| **R06-F01** | K01 `06.VectorSearch/HnswIndex.cs`: Add/Search/GreedySearch/SearchLayer/SelectNeighbours используют cosine similarity; topK и efSearch — отдельные параметры; metadata не фильтрует поиск внутри этого класса. | Детерминированное вычисление близости поверх случайно построенной ANN-структуры; не learned metric. Domain scoping может быть выше и должен быть показан отдельным producer→consumer. Меняющая координаты проекция требует совместимости индекса, не только одинаковой размерности. |
| **R06-F02** | K02/K03 `08.GraphRag/HybridRetriever.cs`, `LocalSearch.cs`: local/global fusion с весами 0.6/0.4; embedding→vector search→graph nodes; similarity плюс PageRank. | Здесь слово Hybrid **не означает** BM25+kNN или обучаемое смешивание. Калибровка шкал и выигрыш не измерены. Графовый поиск контекста не равен обучению graph policy. |
| **R06-F03** | K04 `17.DynamicDomains/DynamicDomainGraph.cs`: closure достижимости и BFS-поиск bridge domains. | Переиспользуемая topology support. Это не вся ILearningDynamicGraph и не доказанная higher-order policy. Сортировка множества мостов не разрешает сортировать смысловую последовательность плана. |
| **R06-F04** | K05/K06 `TrainableRoutingFlow`, `BaselineKeywordCandidateGenerator`: внешние CandidateUnitIds, baseline authority, keyword score, DomainHint boost; flow выбирает подходящий score/confidence либо KnowledgeEdge. | Этот route — явно baseline. DomainHint не является фильтром допустимости; ExecuteAsync здесь возвращает routing result, а не доказанный solver call. Fixed confidence formula не является измеренной вероятностью правильности. |
| **R06-F05** | K07 `RoutingProfileTrainer`: профили keywords агрегируются из примеров по ChosenUnitId. | Реально data-derived baseline, а не неизменная константа. Но DPO/LoRA/обучаемая distance/k policy в этом классе не прослежены. Не обобщать отсутствие на весь репозиторий. |
| **R06-F06** | K08/K09 `DynamicGraphMachine`, `DynamicGraphMachineCompositionFactory`: L5 `ILearningMachine<DynamicGraphDomain>`, `honest_untrained`; TrainBatch/Evaluate/Export/Import отвечают NOT_ACTIVATED; фабрика принимает honest-untrained composition. | Конкретный lifecycle не активирован. Нельзя назвать весь graph код отсутствующим, переключить честный flag и назвать это обучением либо создать четыре новых trainers. |
| **R06-F07** | K10 `KnnLogicalUnit`: Train обучает BPE по корпусу, строит HNSW; vector имеет token-count и predicate-count половины. Execute берёт `bestExample.Output` через первый hit, остальные hits добавляют evidence IDs. k задаётся при Train. | Есть обучение словаря и память примеров; нет показанного neural-DPO обновления. Изменение k без изменения consumer может менять стоимость/evidence, но не способ агрегации ответа. ANN search itself также может менять первый hit — это отдельный эффект retrieval recall. Hash collisions и потеря структурных различий — гипотезы для предметного опыта, не измеренные дефекты. |
| **R06-F08** | K11–K14: runtime принимает trusted edge-authority preload; resolver проверяет exact identity/domain/types/IsLearned/trust; pipeline вызывает `await unit.ExecuteAsync`; L3 executor разрешает flow и вызывает `flow.ExecuteAsync`. | **Существуют реальные reusable seams**, а не только имена контрактов. Resolver сам не должен исполнять solver. Не прослежены конкретный trained checkpoint→preload и весь ordinary-host handoff от L2 decision к L3 association. Это graph-specific вопросы R07/R05, **не полный перечень остатка до DPO**: см. F15–F17. |
| **R06-F09** | K15/K16: typed LogicalUnit envelopes; fan-in содержит PathOrderRef, rejected alternatives, conflicts, provenance и MergeOrderIsAuthority. | Типизированная база уже существует. Generic TOutput сам по себе не доказывает сохранение many→many associations. Наличие fan-in carrier не доказывает фактическую async execution/обучаемую коррекцию связей. |
| **R06-F10** | K17 `OrchestrationReasoningScenarioUnit`: загружает admitted user/common state, делает baseline nearest-signal matching; runtime требует exact signal contract, transferable structure overlap оставляет eval-only. K18 `SkillSelectionOrchestrator` явно CONTRACT_ONLY. | Есть state/receipts, но не доказана переносимая semantic reasoning policy. Последний вывод относится только к названному adapter, не ко всему SmartSkillsDomain. Планы не должны повышать статус по слову implemented в старой шапке. |
| **R06-F11** | K19 `BM25Scorer`: реальная корпусная статистика, fixed K1=1.2/B=0.75. K20 `BruteForceIndex`: полный cosine scan/sort/topK, Save/Load не реализованы. | BM25 и точный контроль существуют; второй retrieval engine не нужен. End-to-end BM25+kNN consumer в выбранном пути пока не установлен. Точный индекс здесь in-memory reference, не готовый publication bundle и не эталон предметной правильности. |
| **R06-F12** | K21 `DomainClassifierUnit`: отдельный Train передаёт predicate examples внутреннему classifier; Classify выбирает trained predicate route либо direct-question/runtime-failure/predicate-affinity/keyword fallback. При UsedKnn confidence поднимается минимум до 0.90. | Hybrid domain/intent classifier уже есть. Эта confidence — правило, не эмпирическая calibration. CODE_Q/BUG/PLAN/ARCH/REFACTOR нельзя смешивать с семью учебными предметами или tenant/project scope. Внутренний classifier и его обычный training owner здесь не заявлены полностью прослеженными. |
| **R06-F13** | P15 WorkLeft P4-1 §6 требует «нулевой margin — нет изменения весов», а §5 сохраняет policy/reference reward difference. | **Конкретное спорное требование**, не доказательство подгонки CI tests. Для `L(m)=-log(sigmoid(m))`, `dL/dm=sigmoid(m)-1`; при m=0 производная = -1/2. Нулевая маржа не влечёт нулевой parameter gradient. Не превращать старую формулировку в общий запрет cold-start update. |
| **R06-F14** | P06 DECISIONS-LOCKED D10 разрешает BM25+kNN; P16 Skill Selection §0 запрещает даже semantic candidate helpers. PARALLEL6 сохраняет hybrid helpers и trained final authority. | Конфликт области/версии требований, не повод удалить детерминированную пользу. R12 выпускает адресную migration decision для skill-selection path: допустимый helper, окончательный learned owner и consumer, заменяющий старый запрет. Исторический источник не переписывается. |

**Предел F13:** производная по margin не гарантирует ненулевое изменение каждого веса. Возможны нулевой Jacobian, одинаковые представления сторон, замороженные параметры и другие причины. Не вводить обратный gate «веса обязаны измениться/качество обязано вырасти». B читал только fields текущего DpoTrainer K22; C дополнительно прочитал StepVector/scorer 530–730. Это не полный gradient audit. Первичная публикация DPO: https://arxiv.org/abs/2305.18290; математическое рассуждение не является результатом запуска.

## 4. Восстановленная цепь с разделением ответственности

| Решение | Что сохранить детерминированным | Что должно зависеть от данных/обучения | Где наблюдать эффект |
|---|---|---|---|
| Domain/intent/stack/project | Права, exact project/tenant identity, schema/version compatibility; StackDetector before normalization по исходному D07 | Определение смысла, mixed-domain membership, uncertainty/knowledge edge | Ошибки по каждой оси, unknown/mixed cases; router-vs-oracle-domain diagnostic |
| Представление | Канонические IDs, размеры/единицы, типы, структурные связи и допустимость преобразований | Нужная предметная проекция/веса признаков, при наличии конкретного обучаемого owner | Различение near misses при неизменном candidate universe; сохранение отношений, не только similarity |
| Кандидаты/соседи | Разрешённый пул, exact reference для малой памяти, индексирование/ANN | Предметная/query-conditioned relevance, обоснованная k/radius policy, actual neighbor consumer | Pool recall, retrieval recall, lawful answer utility, затраты — отдельно |
| Flow/route | Валидность портов, реальные dependencies, no forbidden effects, ограничения ресурсов | Содержательный выбор отношений, recipients и порядка допустимых шагов | Фактически исполненный маршрут и выход при frozen Units; semantic order сохраняется |
| Async orchestration/Graph | Cancellation, immutable version association, bounded scheduling, provenance | Полезная комбинация/слияние и перенос higher-order паттерна | Качество ответа, novel compositions, все branch costs, не только latency победителя |
| Принятие/память | Точное связывание verdict с содержимым; запрет обхода arbiter | Positive/negative/warning experience как контекст learned решения | Contradiction avoidance против memory-only/no-update controls |
| Следующий урок | No-repeat journal, full state, exact pair/provenance/admission и сохранность обучаемого смысла | Machine-owned update именно выбранной policy/state group под явным objective | Новый ordinary inference с принятым состоянием либо точный rollback; не только trial evaluator |

Это смысловая схема, а не утверждение, что все стрелки уже подключены. Она не добавляет интерфейсы/модели ради названий. Similarity score не становится conditional policy probability или semantic reward без определённого механизма.

## 5. Четыре роли не потеряны

| Роль владельца | Способность, которую надо наблюдать | Различающий опыт из DECISIONS |
|---|---|---|
| ILearningUnit | domain + input/vector schema + output schema; many→many с сохранением того, какой выход отвечает какому входу | E1/E3: несколько входов и допустимых выходов, law-sensitive near misses, не сведение всего к одной строке |
| ILearningFlow | Коррекция отношений между выходами одних Units и входами других; recipients и fan-out не равны окончательному winner | E5-A: Units заморожены, меняются только отношения/политика Flow |
| ILearningOrchestrator | Асинхронные комбинации для одного ввода, корректная обработка частичных результатов/отмены | E5-B: фиксированные знания, изменённая комбинация; полная стоимость всех ветвей |
| ILearningDynamicGraph | Соотношения паттернов разных порядков и перенос композиции на новые семейства | E5-C: известные primitives, неизвестная composition; не запоминание route ID |

Train остаётся в ILearningMachine; Graph — L5 specialization. Названия ILogical* связываются с ролями по поведению, не переименованием. LoRA не является автоматически tokenizer или transpose operator: нужны исходный/целевой coordinate space и реальное место adapter в forward/update. TeachingVector как pair-weighting input не является целевым semantic output token.

## 6. Receipt первичных решений и соседних исследований

**R03 CP08 F02/F03 → F01/F02/F07 и E2/E3/E4.** Перенесены domain-scoped pool, memory vs authority и положительная роль корректного deterministic helper. В B прочитаны WorkLeft MASTER→NDJSON RAG-04 row24→Continue RAG-06→dynamic-flow ARBITER и DECISIONS-LOCKED. Это не копирование RAG-списка вместо чтения решения; C не заявляет повторного полного traversal.

**R03 F17 → F03/F09 и E5.** Meaningful sequence не перемешивается под видом presentation permutation. Branch recipients и winner различены; rejected alternatives и conflicts сохраняются.

**R03 F20 → F10/F14 и R12 packet.** Более широкий skill-helper ban не замолчан; есть обе первичные стороны и конкретный владелец разрешения. Существующие state adapters не названы полноценной neural reasoning системой.

**R03 F22 → DPO contract в DECISIONS.** Авторство final chosen/rejected решено, не отправляется заново на согласование. CODEX **astra 6 ultracode** выбирает учебную задачу; внешняя открытая китайская модель пишет **обе стороны**; CODEX **astra 6 ultracode** оценивает именно эту пару. Эти записи не являются фактическими вызовами операторов.

**WorkLeft P4-1 §5 → F08/F13.** Промпт требует встраиваться в имеющуюся edge-authority обвязку; код уточняет reusable seams. Старое «пять отсутствующих типов» не переносится как современная inventory. Исследована конкретная формулировка нулевой маржи; consumer — R05/R12/R17, не новая test campaign.

**Original task + task prompt + human slice map → четыре роли и три delivery линии.** S07.04A/B/C, .05B/C, .07A, .08D, .10C, .11A остаются отдельными результатами. Сохранены S08 design→HTML/CSS→real browser→visual judge→history/UI→authorized lesson→next consumer; S09 private semantic index/editor journeys; S10 whole-program acceptance. Внешние клиентские репозитории и полный task catalog этим R06 не обследованы. Перенос обязательства должен иметь primary section, owner и trigger, а не исчезнуть из программы.

**R04 representation receipt → F07 и E1/E2.** Raw, canonical и TeachingVector различаются; saved kNN BPE нельзя объявлять несовместимым из-за локального root vocabulary другого канала. Consumer нужного канала устанавливается по вызову. Прочитанный R04 источник — `55baaca73b2d09779f519ed3fe5ec28c7235bcd0`, 1–65 в C; это не присвоение полного исследования R04.

**R05 CP2 F09–F14 → F15–F17, E6 и H07/H12/H13.** Приняты actual training/publish chain, token/objective mismatch, TeachingVector weight-only смысл и отсутствие доказанного ordinary consumer. Нельзя заново создавать coordinator/trainer, нельзя считать старые два graph links единственным остатком. R05 сохраняет OPEN и свою ответственность; конкретные новый prompt и receipts записаны в CHANGE-MAP, fresh locators — RUN-LOG.

## 7. Что этот результат не утверждает

Нет measured domain accuracy, learning gain, optimal k, latency, RAM/VRAM, устойчивости к забыванию, полного source-to-gradient trace, complete S07–S10 scenario audit или independent Astra verdict. Исторические 95%, 0.50, 20×, 2 MB и +8% не превращены в сегодняшние измерения. Подозрение о подгонке текущих тестов не доказано: F13 — проблемный плановый критерий, F15/F16 — адресные code observations, не аудит всей test/code history.

Остаток распределён: R04 — конкретные координаты/семантика представления; R05/R18 — existing exact-pair→token/objective seam; R07 — правильный получатель и ordinary consumer, graph L2/L3 только при выборе этого пути; R08 — версии предметных законов/семейства; R12 — один owner/challenger и helper scope; R13 — минимальное заранее определённое сравнение. R22 получает остальные ablations, не превращая их в prerequisites. E1–E6 — варианты различения гипотез, **не шесть дополнительных обязательных запусков сверх 25**.

## 8. Новые выводы ремонта C — предметный смысл, не новый статусный аудит

### R06-F15 — исходная оценка пары не защищает от потери различия перед optimizer

**Источник:** R05 CP2-F10, затем собственное чтение K ремонта / `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, PrepareExecution 1510–1695 и BuildBatch/Compact 2260–EOF, blob `cb8744fcbbcff99821852e789dc92163976c0ad7`.

CreateAdmitted связывает raw Prompt/Chosen/Rejected, а Encode получает отдельно Compact каждой части. Compact сворачивает whitespace и оставляет первые 480 символов. Следовательно, исходная assessed pair и фактически обучаемое представление могут различаться по смыслу. Это code-traced risk/механизм потери, не измеренная частота ошибок.

**Различающий пример, не real Teacher pair:** стороны совпадают в первых 480 нормализованных символах, а существенное отрицание/условие различает их только дальше. После этого transform стороны могут совпасть. Разные исходные fingerprints не устраняют совпадения encoded input. Исправление — lossless в рамках поддерживаемого бюджета либо явное отклонение/новая разрешённая и повторно оценённая учебная задача, не молчаливое усечение.

**Принято:** active R06 A1/B2, E6 и first-seam handoff R18. Такой опыт не должен заключать «domain metric/LoRA не учится», пока target distinction не доходит до objective. Условие касается зависимого DPO-опыта, не блокирует всё исследование R06.

### R06-F16 — существующий scorer требует явной response-objective границы

**Источник:** R05 CP2-F11 и собственное чтение K / `11.DPO/DpoTrainer.cs` 530–730, blob `9c358978075db8c22fe0125eaea9e89e831d5635`; BuildBatch/ToMask в F15.

StepVector действительно вызывает policy/reference forward, backward и AdamW. ComputeLogProbs возвращает sum/cnt по непустым next-token позициям; текущие маски покрывают prompt+answer. Это не выделенная likelihood ответа при заданном prompt. Разные длины меняют нормировку и вклад prompt. Объявленный response boundary/length policy должен совпадать в forward/backward и reference, а не только в названии DPO.

**Граница:** mean score не запрещён для любого возможного objective; он должен быть явно обоснован как выбранный objective, а не подменять заявленный conditional DPO. Публичный EvaluateAverageNextTokenLogProbability использует тот же helper: R18 не должен незаметно менять смысл общего inference scorer. Полное доказательство gradient correctness здесь не выполнено.

**Принято:** E6/H12/H13 и уже локализованный R05→R18 packet, без нового trainer и повторного общего аудита. Независимая Student quality отделена от pre-update training accuracy.

### R06-F17 — получатель обновления важнее общего имени «learning»

**Источник:** R05 CP2-F03/F05/F09/F12 и F07/F08 R06; свежий KnnLogicalUnit в F18. R05 сообщает actual PolicyModel, а LogicalDpoTrainingUnit.Id — capability, не автоматически semantic selector. TeachingVector масштабирует pair contribution, а не становится output vector/metric training.

**Архитектурный вывод:** требуется одно связанное соответствие lesson intent → semantic recipient → state/parameters → objective → typed decision/output → ordinary consumer. LlmModel checkpoint не становится saved BPE/example map kNN без конкретной семантически определённой связи. Winning solver, learned selector и runtime arbiter не взаимозаменяемы.

**Принято:** H07 теперь сначала выбирает фактический product path и получателя, затем проверяет его оставшийся consumer. Два L2/L3 links остаются полезным graph-specific остатком F08, но не универсальным первым gate для CodeDocumentation. Сами недостающие bridges этим ремонтом не реализованы.

### R06-F18 — kNN baseline проверен свежим кодом; новая политика остаётся гипотезой

**Источник:** K ремонта / `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`, 1–EOF, blob `2957816a88aa9cdcd3020bb81be56b5dfff165e0`. Подтверждены fixed-at-Train k, saved BPE, cosine HNSW и `bestExample.Output`. Остальные neighbors дают evidence IDs. Low confidence создаёт warning; фактическое воздержание нужно проверить у потребителя, а не предполагать.

Vectorize считает BPE-token/predicate slots и нормализует их; если два входа дают одинаковые counts/predicates, такой этап не различит порядок. Это условный структурный контрпример, **не утверждение**, что любая перестановка текста сохраняет BPE tokens или что все vectors системы теряют порядок.

**Принято:** B1/E2 обязаны назвать один настоящий neighbor consumer и один изменяемый фактор. Предметные признаки, learned score/selection, k/radius и ANN effort разделены. Saved vocabulary/example learning сохраняется как реальный механизм; neural DPO и улучшение на новых compositions не приписываются ему без evidence.

**Итог ремонта:** устранены устаревшая передача CP1-only и ложное сужение остатка до двух связей; сохранены полезные алгоритмические выводы B и параллельная работа. Следующий результат — конкретная реализация/экзаменация ближайшего seam по R12/R13/R18, а не повторное перепланирование всего проекта в R06.
