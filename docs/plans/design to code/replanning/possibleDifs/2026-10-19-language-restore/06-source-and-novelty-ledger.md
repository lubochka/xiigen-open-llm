# Реестр предыдущих исследований, проверка новизны и карта плановых оснований

19 октября 2026. Реестр подготовлен отдельным участником аудита источников и сверки новизны. Исследование выполнено чтением GitHub и локальным поиском; новые тренировки, product tests и Teacher-вызовы не выполнялись. Закрытый для изменений каталог core использован исключительно как источник.

## 1. Что получено и насколько проверено

Создан полный **выбранный текстовый корпус двух заданных директорий на точных снимках**:

| Источник | Ref | Получено файлов |
|---|---|---:|
| `xiigen-planning/.../replanning/possibleDifs` | `9cea3930067f216ccb812ecc5e412aec673f5058` | 417 |
| `llm_mvp_core/docs/plans/replanning-possible-diffs` | `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6` | 861 |
| Всего | 19 759 125 исходных байтов | **1278** |

Включены 563 Markdown, 514 JSON, 12 TXT, 15 PowerShell, 18 C#, 9 Python, 31 MJS, 73 LOG, 9 CSPROJ, 27 CJS, 3 JSONL, 4 PROPS. Первоначальные 1131 документа и текстовых приложения дополнены 147 скриптами/логами/project files. Все 1278 успешно загружены, и **все 1278 локальных файлов побайтно соответствуют Git blob SHA**. Нормализованные транспортом CRLF и добавленный локальным patch конечный LF возвращались к исходному виду только при точном совпадении SHA. Ошибок и отсутствующих выбранных файлов нет.

Directory listings и три ограниченных recursive subtree получены без truncation; полный recursive tree репозитория не использован как доказательство полноты, поскольку такой oversized запрос завершался Transport closed. В известных деревьях есть ещё 208 не включённых blobs: DLL/PDB/EXE/native libraries, архивы, build caches и служебные файлы. Они перечислены в [omitted-source-blobs.json](evidence/omitted-source-blobs.json). Это не 208 «непрочитанных гипотез» и не заявление об отсутствии сведений внутри бинарников.

Состав и проверка:

- [source-manifest.json](evidence/source-manifest.json): 1278 исходных путей с corpus, Git blob SHA и bytes; repository/ref заданы в общей таблице снимков.
- [inventory-validation.json](evidence/inventory-validation.json): итоговые counts и проверка соответствия исходным blob.
- [omitted-source-blobs.json](evidence/omitted-source-blobs.json): 208 явно исключённых blobs.
- [novelty-search-receipts.json](evidence/novelty-search-receipts.json): шаблоны и результаты шести поисков; пути совпадений преобразованы в ссылки на immutable источники.
- [reviewed-source-ledger.json](evidence/reviewed-source-ledger.json): отдельно зафиксированный минимум смыслового чтения.

**Ограничение Git LFS:** четыре выбранных `comparison.json` содержат указатели LFS, а не полные payload: ingress, embedding, bias и DPO mask. Они перечислены в [lfs-pointer-sources.json](evidence/lfs-pointer-sources.json). Для них совпадение Git blob SHA подтверждает указатель; полный LFS payload этим аудитом не получен и не перепроверен. Их содержательные выводы опираются на прочитанные отчёты, таблицы, проверки одинаковости и сохранённые независимые reviews. Остальные 1274 выбранных текстовых blob доступны полностью. Создан также рабочий индекс 5645 заголовков; он не является отдельным новым источником доказательств.

**Fetched и searched не равны полностью осмысленно прочитанному.** Все 1278 участвовали в заключительном поиске. Отдельный [reviewed-source-ledger.json](evidence/reviewed-source-ledger.json) фиксирует подтверждаемый смысловой минимум этого отчёта: 13 целиком прочитанных prior документов, 7 адресно разобранных prior документов, 17 целиком прочитанных плановых источников вне этих двух директорий и 2 целиком прочитанных skills. Дополнительные адресные чтения и поиски не превращены в фиктивные «1278 ручных аудитов». Число файлов также не равно числу независимых гипотез: многие сводки и карточки повторяют один исходный результат.

## 2. Проверка новизны: два ошибочных предварительных NEW и позднее параллельное совпадение

| Кандидат | Финальный статус | Основание и различие |
|---|---|---|
| Масштаб факторов LoRA при неизменном начальном forward: A→cA, B→B/c; свежая B=0 и optimizer0 | **Узкая новая гипотеза в данном корпусе; причинный эффект на качество не проверен** | Exact gauge/reparameterization search: 0 совпадений. Ближайшие 24 строки в 23 файлах описывают Kaiming A/B=0, alpha/r, rank/targets или общий scalar TeachingVector/AdamW. Они не задают same-forward factor conditioning contrast. |
| Разные subphase одного домена получают один Unit и один query, metadata не влияет на выбор | **Уже известный U-N1/PG-06; новую нумерацию механизма не выдавать** | Полная конкретная цепь и различающий опыт уже в [PREPARETOGPL-UNITS-RESTORE, L73–110](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-UNITS-RESTORE-2026-10-18.md#L73-L110) и [PG-06, L93–107](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-EXPERIMENTS-2026-10-18.md#L93-L107). Новым может быть только доказательство той же границы в `language_translate`, если оно закреплено исходником. |
| Encoder до DPO против encoder после DPO при полностью согласованной переиндексации одного corpus | **Уточнение P92.2, не новая гипотеза** | [P92.2, L21–23](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P92-dpo-memory-flow-bridge.md#L21-L23) уже сравнивает S0M1↔S1M1 при одинаковом corpus и согласованных query/document embeddings для каждого Student. |
| SemanticCompilerFacade: RememberCompiledSource до PredictNextKinds, grammar учится на текущем query | **На исходном снимке новый trace P79; на публикационном срезе совпал с LT-N03 пакета47** | Имена RememberCompiledSource/_compiledSources/EnsureGrammarMaskFilter отсутствуют во всех 1278. [P79](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P79-grammar-leakage.md#L5-L27) уже описывает TRAIN+heldout grammar contamination в E05. Первоначальная новизна относилась к caller/lifetime, не общей идее утечки; после concurrent47 самостоятельная новизна C2 снята (см. §9). |

Предварительный положительный verdict для phase-query был отозван после завершения всего корпуса: раньше часть planning файлов ещё не была в поиске. Финальный документ должен использовать только приведённый здесь вердикт. Это пример того, почему перечень «новых» нельзя замораживать после чтения одного каталожного README.

### Что U-N1 уже доказал и спроектировал

Prior U-N1 включает обычный `ExecuteBindingAsync`, latest registry selection по role/domain, canonical query, игнорирование metadata в scoring, последующие subphase annotations и условную инвариантность retrieved output. Он отдельно устанавливает, что current `BuildChosen` даёт одну label на subphase, поэтому перестановка одинаковых labels внутри scope бессмысленна. Scoped eligibility и raw output before translator уже заданы в опыте. Следовательно, это нельзя вновь продавать как обнаруженную здесь потерю декомпозиции.

Сравнение с другим старым branch остаётся полезным: прежний Translate мог вообще не читать knnOutput, тогда как current сохраняет его. Но существующий текст U-N1 уже обсуждает такую разницу для PrepareToGPL. Чтобы добавить вклад для `language_translate`, надо доказать именно состояние этого branch и показать, что оно совпадает или отличается; одна смена названия baseline не создаёт новый алгоритм.

### Минимальный эксперимент для новой LoRA гипотезы

Эксперимент должен отличать факторную параметризацию от обычного LR/rank/alpha sweep. Один frozen base, одна reference, одинаковые q/v targets/rank/alpha, batches, token IDs, masks и stochastic state. A — исходная свежая LoRA; B — те же факторы с заранее фиксированным c при нулевой B и нулевых AdamW moments. Начальные logits обязаны совпасть. Сначала один шаг: градиенты каждого фактора, moments, effective ΔW и logits. Затем только при измеримом различии — короткий одинаковый train budget с собственными C# и EN→RU ответами. Изменение effective ΔW при same-forward старте подтверждает optimizer-conditioning механизм; улучшение качества требует отдельной наблюдаемой разницы ответов. Никакого обещания победы и никакого бездумного перескалирования уже накопленных AdamW states.

## 3. Уже покрытые семьи, которые нельзя повторно называть новыми

| Семья | Ближайшие prior anchors | Что остаётся проверять в конкретном сравнении ветвей |
|---|---|---|
| DPO pair quality, семантические families, chosen/rejected ambiguity, masked objective и sign | core02, core12, core41, P04/P05/P07/P08/P10/P105/P109/P111/P112 | Точные admitted bytes, реальный batch и конкретный functional ответ. Общая просьба «проверить DPO» уже покрыта. |
| LoRA q/v-only, random base, rank/targets, нулевая B, frozen base/reference, continuation | [LoRA planning](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/03-LORA-TOKENIZATION-VECTOR-SPACE.md), P01/P02/P119, core12 | Новый идентифицируемый фактор, а не очередной список гиперпараметров. |
| Forward/backward autograd, Embedding gather, bias broadcast, fractional masks | P11/P86; выполненные receipts ниже | Не объявлять уже исправленную component связь текущим необнаруженным дефектом или причиной exact-v2 RP01 без caller. |
| BPE identity, token/char offset, capacity, fragmentation, position/shape/budget | core12 BPE, core43, core47, P07/P26/P98/P103 | Реальный emitted representation и единая внешняя смысловая оценка; roundtrip не доказывает semantic sufficiency. |
| KNN memory/encoder/index версии, signature alias, top-k, duplicate IDs, stop/continuation | core51/core60/core63/core70, P24/P25/P92 | Точный owner и выбранная запись; stale index и согласованная переиндексация — разные проверки. |
| AST/order/role/domain features, codec и granularity | core38/core44/core73, P26/P27/P51/P79/P82/P83/P84/P91/P120 | Собственный domain parser и ordinary caller; Roslyn-признаки не объявлять универсальными для перевода. |
| DPO→Unit→Flow bridge, cardinality/typed contracts, generated output, restoration | core26, P92/P94/P107; U-R1/U-R2/U-N1/U-N2 | Не подменять memory-learning нейронной генерацией; routing agreement отдельно от выполненной обязанности. |
| Historical FAIL/positive results and transfer | core46/core54/core65/core66/core78, P03/P125 | Прямые raw/consumer/version связи; итоговый FAIL не отменяет полезный подрезультат. |

Это навигационная таблица, не утверждение, что каждый из перечисленных 125 номеров независимо проверен в этой сессии. Точные исходные пути каждой карточки и их снимки доступны в приложенном source manifest; таблица не заменяет прочтение выбранной карточки.

## 4. Сохранённые выполненные опыты уже содержат результаты, которых нет в старых статусах

Ниже — прочитанные **предыдущие результаты**, не новые запуски этого аудита.

| Существующий пакет | Что подтверждено в его отчёте | Предел |
|---|---|---|
| [P125 native memory](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md) | После восьми уроков память Executor дала 0/24→19/24: 8 exact, 3 near, 8 новых комбинаций знакомых action/object. Label control C — 13/24. 144 обычных Flow calls, 0 neural updates. | 22/24 answered; polarity/question ошибки и strict coordination failure. Это смесь exact/phrase/shared-KNN памяти и readiness; независимые language families не доказаны. |
| [P25 cold restore](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md) | Новый процесс, один load, 24 обычных ответа; все output/route/confidence/verdict восстановлены, точность 19/24 сохранена. | 0 новых уроков. Canonical state сравнен с исключением ExportedAtUtc. Не новая генерализация и не neural restore. |
| [Translation ingress](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md) | При одинаковых восьми уроках reader/builder начал передавать 16 phrase mappings: 11/24→19/24, composition0/8→8/8. | Strict failures среди accepted1→3; новые semantic families не проверены. Выполнены реальные reader/builder→Train→ordinaryFlow, но не полный CLI/registry. Уже существующее исправление, не новая наша находка. |
| [P86 component](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P86a-mask-01/report.md) | Fractional legacy mask derivative расходился с собственным weighted mean score; weighted backward прошёл выбранную numerical сетку. | Только logits component, 216 coordinate×h rows, 0 Student/optimizer. Exact-v2 binary masks этим не обвиняются. |
| [P11 component](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P11-embedding-bias-01/report.md) | Gather→EmbeddingWeight и broadcast→Bias связи отсутствовали; изолированный B восстановил native gradients при same forward. | Component effects; в обычном RP01 base frozen и Linear bias выключен, поэтому причинный диагноз его качества не установлен. |
| [Embedding SFT8](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/report.md) | После исправления реальный full-model SFT достигает 55/55 tensors вместо54/55; меняется38400 embedding coordinates. | Предметный результат0/24→0/24. Full-base SFT не равен RP01 frozen-base DPO. |
| [Linear bias SFT8](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/linear-bias-sft8-01/report.md) | Активный компонентный bias guard улучшен; сохранённый C50 biasless. | 0/24→0/24, все тексты/loss/weights равны. Контроль до reused из Embedding after; не дополнительный новый training run. |
| [DPO mask live8](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/report.md) | В экспериментальном legacy fractional режиме по8 native updates; различаются moments16LoRA, guards126/162→162/162. | 0/24→0/24,24текста одинаковы. Не productionCLI500/exact-project; сохранённая память19/24 — другой получатель. |

Практическое следствие: «нет neural gain» и «нет обучения вообще» — разные выводы. Положительный опыт памяти и её cold restore уже существует. Приоритет пользователя восстановить `language_translate` нужно привязать к тому получателю, который тогда действительно давал полезный ответ, а не пытаться объяснить этот ответ исключительно Student LoRA.

[Core65](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md) уже исправляет широкое отрицательное резюме E22: поздний kNN receipt macro-F1 0.4709→0.5646 при360/90 train/validation. При этом менялись corpus и label function, source-only поиск не установил обычного product reader. Этот результат нельзя повторно открывать как новый и нельзя назначать точным «matrix→KNN donor»: обе описанные реализации уже kNN. Положительный подрезультат необходимо сохранить с его собственными ограничениями.

## 5. Что RP01 pack сейчас действительно требует

[README L3–21](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md#L3-L21) переключает пакет на REC01–REC05. Он имеет статус DRAFT_PENDING_INDEPENDENT_ASTRA_REVIEW; прежняя программа BATCH500 не является текущим dispatcher. Прочитаны целиком основной recovery audit и все пять recovery cards.

- REC01 — source→outcome/provenance RCA без тренировок.
- REC02 — 8–32 точные новые пары, tiny gradient/LoRA/train-overfit и actual decode/reload.
- REC03 — matched capability/tokenizer/SFT→DPO versus DPO-only.
- REC04 — независимая ветвь обычного domain consumer, semantic laws и KNN.
- REC05 — дальнейшая новая learning curve только после измеримого раннего сигнала.

Это план восстановления, а не evidence, что все стадии исполнены. [REC02](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC02-DPO-GRADIENT.md#L11-L17) прямо предупреждает: прежний simulation CLI разрешал calibration1 и main500; выдуманная команда `train 8 8` не является исполнимым API. В современных микропланах надо указывать реальный host или точное минимальное предложенное расширение, не фабриковать команду.

Архитектурная граница в [source map](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md) и [recovery audit](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): Student neural update, law-aware retrieval, typed many-to-many Unit/Flow и higher-order graph — разные получатели. RP03/RP04 нужны для полных контрактов, но не превращены в барьер до первого малого neural/hybrid опыта. Детерминированное использование обученной памяти не является отсутствием обучения.

### Сохраняемые противоречия источников

| Противоречие | Точные источники | Что делать в аудите |
|---|---|---|
| Кто автор обеих DPO sides | [RP01 README L29](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md#L29), [EXECUTE-NOW L15](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-EXECUTE-NOW.md#L15) допускают честно помеченную Astra dev simulation; [LEARNING-EXAMINATION L15](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/LEARNING-EXAMINATION.md#L15) задаёт другой simulated-teacher tag. [Replanning AGENTS L29](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/AGENTS.md#L29) и [WorkLeft RESEARCH03 L11–21](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md#L11-L21) требуют actual external Chinese author обеих final sides. | Сравнивать actual provenance конкретного набора и его contemporaneous protocol. Не переименовывать прошлые deterministic/Astra данные в Chinese. Текущее задание — аудит, поэтому вызовы Teacher не требуются. |
| Replay как часть обучения | [DECISIONS-LOCKED D06](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/llm%20dynamic%20flow/DECISIONS-LOCKED.md#L12) сохраняет70/30; [WorkLeft RESEARCH03 L26–30](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md#L26-L30) — новые уникальные пары/apply-once, replay только отдельный определённый режим. | Это разные протоколы/периоды. Нельзя молча включить70/30 как «возвращение алгоритма» или считать исторический replay текущим разрешением. |
| Область публикации в старом research heading | Parent AGENTS содержит старую core-target dispatch, более вложенный replanning допускает authorized planning publication. | Прямой текущий запрос пользователя однозначен: результаты в planning possibleDifs; protected core не трогать. Историческое правило чужой задачи не меняет этой области. |
| Ошибка «kNN заменил арбитра» | [ARBITER-KNN-AUTHORITY-AMENDMENT](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md) сохраняет learned retrieval и отдельную authority проверку. | Candidate score, применимость доменного закона и final acceptance — раздельные функции. Без caller нельзя считать их взаимозаменяемыми. |

Эти противоречия важны как возможный источник разных реализаций и несопоставимых данных, но сами по себе не доказывают ухудшение математики обучения. Устаревшие статусы отмечать историческими, не удалять их из evidence и не переносить в нынешний итог.

## 6. Применённые skills и рабочие правила

Полностью прочитаны `docs/skills/phase-planning-SKILL.md` (Git blob `27ba212c829148158511c7a5a69bc418abfc276c`) и `docs/skills/plan-review-SKILL.md` (`b99f4ce280f48d0242a35f2ba421a2dc1996399f`) на current pinned core. Полные исходники навыков доступны по ссылкам; [phase planning](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/phase-planning-SKILL.md), [plan review](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/plan-review-SKILL.md).

Применимый смысл: исходить из текущего задания, явно ограничивать claims реальной прочитанной областью, прослеживать source→state→consumer, сохранять контраргументы, не считать altered tensor/зелёный тест полезным научением и проектировать конкретный falsifier. План эксперимента должен иметь реального владельца, входные material/state/config, один различающий фактор, доменную оценку, negative/edge controls, stop condition и честное обозначение NOT_RUN.

Исторические exact-count passes, специальные команды другого worker, старые release/Windows/host gates и запрет чужой task на параллельность не применены автоматически к данному документарному поручению. Ни один из навыков не использован для остановки уже разрешённого чтения/подготовки итоговых документов. Другие участники аудита применили архитектурный аудит, проверку кода, prompt engineering и человеческую коммуникацию; чтение двух skills не выдаётся за отдельную проверку всех этих ролей одним участником.

Плановые первоисточники получены по RAG-ссылкам, включая соседние родительские директории. `SOURCE-ROUTES.md` и `SOURCE-AND-ARCHITECTURE.md` прочитаны целиком. Большие `RAG-MASTER-INDEX`, `RECOVERED-DESIGN-CONTRACT`, `HYBRID-ALGORITHMS-AND-RUNS` и WorkLeft использованы адресно. RAG summary является маршрутом к доказательству, не самостоятельным доказательством исполнения.

## 7. Правила интерпретации итогового пакета

1. Приоритетную главу `language_translate` строить по реальному позитивному consumer, его сохранённому состоянию и обычному ответу. Existing ingress/memory/restore не дублировать как новые.
2. Phase-query отнести к уточнениям U-N1/PG-06; добавить только проверенную language lineage. Frozen encoder/reindex отнести к P92.2.
3. LoRA factor conditioning оставить как новую ограниченную гипотезу с same-forward control; не объявлять ошибку factorization или подтверждённую причину RP01.
4. SemanticCompilerFacade query admission объединить с параллельным LT-N03 как конкретный callback/lifetime P79; если caller получает только допустимый prefix и online обучение предусмотрено контрактом, leakage claim для такого вызова опровергнут.
5. Сохранить границу новизны: «точный аналог не найден в1278 файлах указанных pinned директорий и адресно прочитанных основаниях». Это не утверждение, что никто никогда не предлагал механизм, и не гарантия отсутствия синонимичного описания вне проверенной области.

Реестр сохраняет исходное авторство прежних гипотез. Новые документы не заменяют и не изменяют защищённые core-материалы; самостоятельные выводы этого аудита проверяются по указанным первичным источникам и ограниченным спецификациям опытов.

## 8. Дополнительная сверка параллельных работ перед публикацией

После основного снимка planning ветка продвинулась от `9cea393…` до `3215267c0ee2c2beafebec950b45b7fee326daf7`: 18 commits, 15 добавленных файлов и обновлённый родительский README. Новые 12 Markdown, 2 JSON и 1 Python прочитаны и включены в отдельный поиск; proxy-скрипт не исполнялся. Это дополнение к исходным1278 blobs, не подмена даты их снимка. [Точные пути, blob SHA и проверка15/15](evidence/concurrent-planning-delta.json).

Точный same-forward A-scale/gauge контраст и конкретные `SemanticCompilerFacade/RememberCompiledSource` границы в этой дельте не найдены. На промежуточном срезе3215267 сохранялись предварительные статусы: одна новая локальная LoRA-гипотеза и новый consumer существующего P79; C1/P92.2 уже были prior. Окончательная поправка после появления точного LT-N03 приведена в §9: самостоятельная новизна C2 снята.

| Чужая новая работа | Как использовать рядом с нашим пакетом | Статус атрибуции |
|---|---|---|
| [K44-01/02: E26 — отсутствующий контекст и порядок](https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-E26-UNKNOWN-CONTEXT-AND-ORDER-2026-10-19.md) | Проверить поведение metric при неизвестном POS и потерю порядка/кратности через Any overlap. Это самостоятельные вмешательства после точной репликации R2a, их нельзя одновременно смешать с добавлением richer state R2b | Обнаружено и оформлено другими агентами. Сохранённый пятислучайный source-equivalent proxy — их результат, не native .NET и не новый запуск этого аудита |
| [H44: старый R1/R2 routing и новый обязательный входной контракт](https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-NEW-LANGUAGE-ROUTING-CONTRACT-DIFF-2026-10-19.md) | Убедиться, где обычный caller получает направление/тему и компенсирует ли это upstream. Не принимать существование отдельного router за его достижимость | Чужая гипотеза; продуктовый дефект условен относительно caller. Наша C1 проверяет другое условие — подэтап при уже выбранном домене |
| [N45: ProductHeldOut, смысл и supplementary Unicode](https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md) | Отделить внутренний ProductMatch от независимой смысловой оценки перевода. Учитывать, что наличие целевых слов и особенности Unicode-gate могут давать ложный успех | Чужие source-level кандидаты, не наши новые открытия. Наши19/24 опираются на описанную отдельную сохранённую оценку, а не приравниваются к этому gate |
| [46: предметные малые проверки восстановления и оценки](https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/46-MICRO-LANGUAGE-TRANSLATE-RESTORATION-AND-EVAL-2026-10-19.md) | Согласовать планы C# AST, неоднозначного перевода, replay и ordinary Flow; не запускать два экземпляра одной гипотезы с разными именами | Существующий параллельный план, сохранён и связан с этим пакетом |

[Карта25 ранее рассматриваемых механизмов](https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md) также прочитана. Она дополняет навигацию и не отменяет раздельную проверку actual consumer. Наличие там общих rank/alpha/init тем не является точным предшественником сравнения двух одинаковых начальных функций LoRA.

При повторной проверке core по-прежнему указывал на `a392b6be…`, то есть исходники этого аудита не сменились. Существующее содержимое родительского README и все15 параллельных файлов сохраняются при публикации нашего отдельного каталога.

## 9. Последний конкурентный prior: пакет47, e5310a4

Во время публикации ветка получила ещё один чужой commit `e5310a41ab5c79dce2f913563b7557cc27c6f272`: 11 добавленных файлов пакета47 и обновлённый README. Они получены и отдельно проверены на точные совпадения; [список путей и результатов](evidence/concurrent-47-delta.json). Это реальное изменение источников после предыдущего ограниченного прохода, а не повтор всего корпуса.

**C2 совпал с уже опубликованным LT-N03.** [03-NEW-KNN-HYPOTHESES, строки50–62](https://github.com/lubochka/xiigen-planning/blob/e5310a41ab5c79dce2f913563b7557cc27c6f272/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/03-NEW-KNN-HYPOTHESES.md#L50-L62) содержит и добавление текущего source до prediction, и изменение частот при повторном Compile. Поэтому финальный статус C2 — независимое подтверждение и детализация параллельного LT-N03 внутри прежнего P79. Самостоятельная новизна относительно актуального possibleDifs снята. Прежние поисковые receipts с нулём совпадений относятся к своим точным старым снимкам и не скрывают эту поправку.

**Новая LoRA-гипотеза не совпадает с масштабным примером LT-N05.** В [05-ADDITIONAL-MECHANISM-HYPOTHESES, строки11–47](https://github.com/lubochka/xiigen-planning/blob/e5310a41ab5c79dce2f913563b7557cc27c6f272/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/05-ADDITIONAL-MECHANISM-HYPOTHESES.md#L11-L47) сравниваются Append(e,m) и Append(2e,m): изменяется относительный вес морфологической координаты в cosine-геометрии, без optimizer updates. В нашем A₀/2A₀ при B₀=0 весь начальный forward одинаков, а различие возникает при отдельной оптимизации факторов AdamW. Это разные состояния, вмешательства и наблюдения. Полнотекстовый поиск same-forward/A-scale/gauge по11 новым файлам не нашёл точного аналога N-LORA-FACTOR-01.

LT-N01/02/04/06 относятся к другим операторам; они не присваиваются этому пакету. Их [общее оглавление](https://github.com/lubochka/xiigen-planning/blob/e5310a41ab5c79dce2f913563b7557cc27c6f272/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/README.md) и [карта охвата](https://github.com/lubochka/xiigen-planning/blob/e5310a41ab5c79dce2f913563b7557cc27c6f272/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/06-COVERAGE-AND-EXECUTION.md) сохранены в соседнем каталоге. Чужие source-formula probes не запускались.

На промежуточном срезеe5310a4 проверенная область включала исходные1278 Git blobs, дополнительные15 файлов на3215267 и ещё11 файлов наe5310a4, а также актуальные версии родительского README. Это не утверждение глобальной уникальности или полного исследования каждой ветки. Одна узкая новая гипотеза LoRA сохраняется; C1, C2 и P92.2 остаются подтверждениями либо согласованными уточнениями известных работ.




## 10. Финальное дополнение: пакет48, 9733049

Следующий commit `9733049ee39a2906a47e7fc341fe64ae0d5ed323` добавил восемь файлов, включая приоритетную главу и пакет48; [точные пути и Git blob SHA](evidence/concurrent-48-delta.json). Все восемь получены и сверены по хешам; файл новых гипотез прочитан по смыслу основным аудитором и независимо аудитором корпуса. Полнотекстовый поиск exact same-forward/gauge/A-scale по восьми файлам дал0. Это адресный dedup, а не повторная независимая экспертиза всех чужих первичных цепочек.

В [N48-01/02/03](https://github.com/lubochka/xiigen-planning/blob/9733049ee39a2906a47e7fc341fe64ae0d5ed323/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md) рассматриваются соответственно ложная E14 action label от невызванного guard, finite gradient→Inf second moment AdamW и разные recipes TeachingVector current/replay. Это работы других авторов. В частности, N48-02 проверяет численное переполнение, тогда как наша LoRA-карточка сравнивает конечные свежие факторы при одинаковом начальном forward и сохраняет finite-state controls. N48-03 меняет учебный сигнал, не факторную параметризацию. Самостоятельная новизна C2 остаётся снятой; N-LORA-FACTOR-01 по-прежнему одна новая узкая гипотеза в проверенной области.

Пакет48 также выделяет положительный E43 replay retention. Его raw и runner отдельно прочитаны по immutable language_translate refs; добавлен атрибутированный R8 в первую восстановительную очередь и малый протокол в01. Это ранее известный механизм, не новое открытие. Чужой NumPy FP32 proxy не повторялся и не объявлен нашей проверкой.

Конечный source scope: исходные1278 Git blobs (четыре только LFS pointers), затем15+11+8 новых файлов в трёх конкретных обновлениях и версии родительского README. Дополнительные source readbacks E43 перечислены в receipt. Содержимое всех соседних пакетов сохраняется при публикации поверх актуального родителя. Глобальная уникальность и полный построчный аудит всех веток не заявляются.

## 11. Проверка core после основной публикации: 4945b8e

Основной пакет опубликован commit `8046c9bd1aa944d4bc336af304f404fa523c2344`. При контрольном чтении выяснилось, что core продвинулся отa392b6be до `4945b8e85c17b5f150eb5b840d16bbdc4b506242` одним commit **Small sets testing plan**. [Сравнение версий](https://github.com/lubochka/llm_mvp_core/compare/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6...4945b8e85c17b5f150eb5b840d16bbdc4b506242) содержит только девять новых Markdown-планов в защищённом каталоге; продуктовые исходники не изменены. Мы эти файлы только прочитали. Поэтому source/code conclusions остаются привязанными к a392b6be, а свежий план учтён отдельно.

Все девять текстов получены по pinned refs. Exact same-forward/gauge/A-scale поиск дал0 совпадений. Адресно по смыслу прочитаны D-Learn, карта находок и релевантные R06/R21/R24: два обычных DPO-урока, selective m50 reset, alpha/dropout controls и TeachingVector active-count — другие вмешательства. Они не дают точного предшественника A₀→2A₀ при B₀=0, одинаковых alpha и fresh optimizer. Это узкая root-проверка дельты, не новый независимый review всего защищённого корпуса и не запуск чужих опытов.

[Новый каталог малых проверок](https://github.com/lubochka/llm_mvp_core/tree/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes) сохранён в его исходном месте. Его R01–R24 связаны с уже известными семействами; наши R-номера не подменяют эти отдельные идентификаторы.

| Прочитанный файл | Pinned Git blob SHA |
|---|---|
| 01-method-and-report.md | `040a8b9d567d6e73c23d73b49736504aed8cb7c6` |
| 02-programming.md | `387f890fc6f688001e6b9309cc2d2038133ddee6` |
| 03-translation.md | `df243e0ed25e3b32c7c1ef17d4be363f223561f2` |
| 04-documentation-neural.md | `e28068b8963fc3d19adec5733c717381f5ca08da` |
| 05-orchestration.md | `018540f9da73787a52419867dd2e95eecd0c05ed` |
| 06-findings-map.md | `aff2554d25a53bba0c168d84ab60f34a862a1cb1` |
| 07-conditional-probes.md | `ce9cdce4a7238a0cc3f976d85f4527f01e380c76` |
| 08-sources-and-study-log.md | `21f52ce36977887c1e7e3f128ec95745e183c834` |
| README.md | `093175e74d42e934beac2b837e7a6e36e0c6d01b` |

Граница завершённого поиска определена указанными snapshots: исходные1278 blobs,34 новых planning-файла и эти9 новых core-планов. Новые записи после этой границы не включаются задним числом в старые receipts. Ни один core-файл не является частью наших commits.
