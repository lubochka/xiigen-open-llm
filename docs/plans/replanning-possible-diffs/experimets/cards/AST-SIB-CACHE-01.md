# AST-SIB-CACHE-01 — внутренняя действующая версия

<a id="current-executor-task"></a>
## Текущее задание исполнителю — 3 ноября 2026

**Что проверяется: Кэш структурных признаков.** До отдельного прямого назначения исполнения — анализ сохранённых данных и документации. Уже записанный PRIMARY или alias не повторять; текущее состояние смотреть по ID в индексе. [Индекс опытов](00-execution-index.md) определяет очередь; эта поправка не добавляет новую работу. Исторические промпты ниже не являются альтернативным запускным заданием.

**Хороший пример:** При неизменном AST/cache key сохранены признаки и собственный обученный результат; изменение соответствующего узла инвалидирует его зависимые значения.

**Плохой пример и ожидаемая реакция:** Совпало число соседей, но изменённая структура получила старый вектор. Сверить содержимое/порядок sibling по договору; cold setup и reuse учитывать отдельно.

**Передаваемый текст задания:**

> Работай по полной карточке `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md`, её паспорту данных и этому текущему разделу. Перед заданием заново полностью прочитай local/canonical AGENTS, CLAUDE и стек навыков из [договора120, §1](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md#1-свежее-полное-чтение-перед-новым-аудитом-или-заданием); для приёмки также test-integrity, test-failure-triage и bug-to-tests. Примени [договор44, §10–12](../codex-44-post117-saved-evidence-and-finalizer-contract-2026-11-03.md#10-один-текущий-вход-вместо-нескольких-противоречащих-заданий): независимое ожидание → фактический рабочий путь → общий итог. Используй собственные хорошие/плохие примеры выше, не расширяй панель чужими fixtures. При падении сохрани input/expected/actual и версию; отдели регрессию, неверное ожидание, измеритель и инфраструктуру. Не меняй gold/допуск и не скрывай missing/unknown ради PASS. Показатели до/после, точность и регрессии укажи раздельно на русском. Документное поручение не разрешает код, сборки, тесты, Train/Execute. Будущая разрешённая проверка использует только назначенные в паспорте host/команду/бюджет; отсутствующий режим ремонта верни автору, исторический PRIMARY его не заменяет. При разрешённом запуске дождись исходного handle по договору31 без relaunch. Прямой STOP/NEXT исполни немедленно; не продолжай прежнюю работу ради оформления. Любой измеренный полезный эффект сохраняется по своему показателю и домену, с ценой рядом; допуск переноса оценивается отдельно.

Основание усиления: [аудит138](../../fixes-based-on-experiments/codex-138-post117-transfer-and-experiment-audit-2026-11-03.md) и [разбор контекста исполнителя](../../fixes-based-on-experiments/evidence/2026-11-03-post117-executor-context-review.md). Для родственных карточек это профилактика того же класса ошибки, а не утверждение, что их результаты заново проверены. Исходные результаты, наборы, бюджеты и команды сохранены ниже.


<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница COMPILER:** Сверить назначенный уровень compiler: входные features, target packing и ordinary consumer не взаимозаменяемы. Отрицательный/положительный witness берётся из собственного корпуса этой карточки; copied distances/accuracy не обещать native пути без parity. Если вызывается Compile, учесть его реальные вложенные Grammar/Ngram Train.

**Исходный адрес из паспорта:** `12.SemanticCompiler/Prediction/StructuralKnn.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



<!-- BEGIN mandatory-learning-and-recovery-2026-10-21 -->
## Обязательные правила для карточки и любого её промпта

**Найти tool и дождаться своей операции — обязанность исполнителя.** До выдачи команды прочитать [контракт31](../31-tool-discovery-and-wait.md) и указанные там реальные skills/references; по каталогу текущей среды найти launch и wait/resume/status, прочитать их схему и сохранить provider, handle, start time, H/R и deadline. Не выдумывать Kiro API по Codex-примеру, не просить пользователя выбирать инструмент или запускать команду вместо исполнителя.

**Ожидание не равно бесполезному polling.** Одна выдача entry; затем штатный bounded wait/resume того же session/cell/process ID до terminal exit или исходного experiment deadline. Нормальный wait можно повторять при silent/running. Два быстрых неблокирующих чтения без прогресса требуют перейти в wait; если он неисправен/ответы противоречат — независимое чтение H/R/claim/exit и своего PID+start. Не вводить Start-Sleep в занятый терминал. Предел одного ожидания≤60с — не предел эксперимента; yield/RPC timeout не доказывает exit/kill/нулевой расход. После диагностики живой процесс продолжают ждать. Отсутствие log не доказывает, что запуска не было. Не выдавать entry заново «синхронно на25минут», не подбирать results-NN, не удалять claim/partial, не сокращать TRAIN/steps до двух минут ради tool. При UNKNOWN сохранять неизвестный расход и разбирать существующий запуск. STOP/NEXT Любы немедленны.

**Не завершать ход на ожидании и не придумывать работу.** ISSUED/WAITING → штатный блокирующий/event wait исходного handle → terminal/exit → чтение назначенных raw → отчёт. Финальное «жду уведомления / доложу потом» до результата запрещено; обещание auto-notification не доказывает возобновление. Не создавать фоновый waiter/monitor/watchdog, циклы polling, progress-события каждые N шагов, summary scripts, manifests или другую работу «пока ждём»; не запускать агентов или следующую карточку. Обычное ожидание не требует заполнения паузы работой. Соблюдать provider no-poll/no-sleep. Проверить настоящий wait/continuation в каталоге до launch; при отсутствии доказать конкретный блокер, сохранить handle и INCOMPLETE, без выдуманного API или relaunch. STOP/NEXT Любы немедленны. Полный порядок и исключения — [контракт31](../31-tool-discovery-and-wait.md).

**Без действительного обучения задача не выполнена.** Для каждой новой экспериментальной программы, включая diagnostic/reader/restore, обязателен назначенный реальный учебный участок: original файл/метод, learner→изменённое состояние→обычный consumer, точные TRAIN/EXAM, before/after и конечный бюджет. Сохранённый обученный S0 является стартом, но не заменяет этот участок. Для retention/restore навык проверяется до вмешательства; для acquisition допустим cold start с приобретением поведения. Ноль обновлений допустим у отдельного контрольного плеча или reader после общего Train, но не у всей новой программы. Unit/BPE действительно обучают память/правила; neural updates=0 у них не означает отсутствие native Train. Updates, loss, hash и случайный верный токен сами по себе не доказывают навык; 0→0 не является учебным результатом, частичные токены не объявляются целым ответом. Исторический безучебный receipt сохранить; его повтор получает отдельный назначенный рецепт по оригиналу и новый output, а не считается завершённым новым опытом. Уже выполненный учебный участок этой же попытки не повторять ради счётчика после сбоя отчёта; продолжить его сохранённые stages. Отдельная unit-проверка готового receipt не является новым экспериментом обучения. Полный охват карточек и адресные рецепты — [37](../37-full-catalogue-learning-coverage.md).

**0 → 0, отсутствие обучения или неполное A/B — не завершение эксперимента.** Раздельно записать execution_status, learning_validity и hypothesis_status; при невалидности учебная цель OPEN. Сохранить сырые свидетельства и продолжить разрешённое исправление постановки, а не только написать «невалидно» и отчитаться об успехе. Не искать другой checkpoint/модель/host, не менять seed/фактор, не увеличивать steps до PASS: точный исправленный рецепт назначает автор карточки до новой попытки. Старые опыты не удалять. Поздняя ошибка сохранения не обнуляет состоявшиеся updates; неизвестное число — null, невыполненное плечо — «не измерено».

Любое полезное измеренное улучшение в **валидном учебном сравнении** — успех своей метрики/домена; рост accuracy необязателен, регрессии рядом. Диагностика невалидной постановки не закрывает учебную цель. Default-модель исполнителя без override; точные H/R/entry ниже. Отчёт — русский R/report.md. Текущая правка документов не разрешает runtime. Прямые STOP/NEXT Любы немедленны и при открытой цели.

Полный конечный порядок диагностики и критерии: [контракт30](../30-failure-recovery-and-learning-validity.md). При конфликте со старым текстом ниже действует этот блок.
**Состав данных ограничивает вывод.** До запуска автор задаёт точные TRAIN/EXAM/CONTROL, покрытие классов, роли отрицательных примеров, семьи/пересечения и конкретный контроль постоянного ответа. Одноклассовый TRAIN и выигрыш на его единственной метке не доказывают различение маршрутов; updates/confidence не заменяют этот контроль. Разные опыты не устанавливают причину регрессии состава данных. Измерять любой исход, не предписывать ухудшение; набор и budget исполнитель не меняет сам. Полное обязательное правило — [состав данных и допустимый вывод](../32-training-set-and-inference-contract.md).

**Сохранённый учебный результат используется без повторного выбора.** В уже разрешённой фазе исполнения пригодные сохранённые веса/обученное состояние имеют приоритет: восстановить их и продолжить назначенный опыт, не спрашивая «A: использовать результат или B: обучить заново». Поздняя ошибка проверки, сериализации или отчёта сама по себе не основание повторять Train. Сначала прочитать актуальные receipts и установить, не выполнены ли recovery/плечи уже; состоявшуюся работу не переиздавать. Проверять только необходимую для следующего шага совместимость и учебную квалификацию, используя имеющиеся данные; недостающие показания получать лишь по явно ограниченному recovery-рецепту, без optimizer updates и без притворного восстановления отсутствующих moments/RNG. Если пригодность доказана, выбрать reuse автоматически. Если объективно отсутствует необходимое состояние, назвать конкретный технический пробел; не предлагать повторное обучение «для чистоты», не просить пользователя снять guard и не выдавать размер файла/падение loss за доказательство выученного навыка. Текущая документная фаза запусков не разрешает.

**Реализация не отменяет контракт.** «Код не читает RNG/optimizer/проверку» означает отклонение, а не разрешение его игнорировать. До следующего model call исправить реализацию в уже разрешённом scope либо применить только явно назначенный в карточке совместимый режим восстановления; не придумывать исключение, не просить снять guard и не переносить weights-only DPO-исключение на SFT continuation.

<!-- END mandatory-learning-and-recovery-2026-10-21 -->

**Единственное назначение результата AST-SIB-CACHE-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001; report.md и candidate-fix.md только в R. Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [AST-SIB-CACHE-01](AST-SIB-CACHE-01.md) → [NG01](NG01-null-guard-label.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Только документный дизайн, НЕ запуск.** В конфликте со snapshot planning9d311/53cb действуют назначенные здесь внутренние пути, индекс40, модель сессии по умолчанию без override. Source pins сохраняются, первоначально прочитанный HEAD669477 не подставляется вместо них (поздний read-only snapshot — 6e3e886). Старые метки номеров — только source labels. Report/candidate в R; полезный результат также кратко регистрируется в разрешённом required-changes.md. Внешний запрет править protected относился к другому поручению и не отменяет нынешние правки Markdown. Запрет product/runtime изменений сейчас сохраняется. Claim/legacy attempt нельзя обойти сменой root. Ни host, ни runner не материализованы этой сессией.

# AST-SIB-CACHE-01 — стоимость sibling-признаков внутри обучения Structural KNN

**Статус 20.10.2026:** `DESIGN_READY_NOT_RUN`. Это новая карточка-кандидат; реализация эксперимента и обучение в текущем документном проходе не выполняются. Все численные результаты — `NOT_MEASURED`. Старые эксперименты, raw и карточки сохраняются.

## 1. Задача, приоритет и правила исполнения

Проверить, уменьшает ли однократное вычисление позиций детей одного AST-родителя время и выделения памяти **настоящего `StructuralKnnPredictor.TrainFromSource`**, сохраняя все обученные пары и ответы. Это учебная стоимость C#-структурной модели, а не отдельный benchmark списка/словаря. Эксперимент не заявляет объяснение качества RP01 до собственного результата.

Действующий переход: [AST-SIB-CACHE-01](AST-SIB-CACHE-01.md) → [NG01](NG01-null-guard-label.md). Дальнейшие переходы — [текущий индекс](00-execution-index.md#next-contract), без сокращённой копии очереди. Вход всей программы PARENT-ADOPTION-01. Одиночный ID заканчивается END; документы не разрешают runtime.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. В отчёте рядом показать изменения остальных показателей, включая ухудшения.** Для этого кандидата полезны уменьшение allocated bytes полного обучения и уменьшение времени полного обучения. Изменение только размера JSON, числа строк отчёта или скорости отдельного `ChildNodes` без обучения не является результатом карточки.

**Обязательное правило stop/next.** При явном «следующий эксперимент» немедленно прекратить прежнюю задачу и перейти к явно назначенному следующему ID: не дописывать прежний отчёт/registry, не делать дополнительные review, hash checks, tool calls, запуски, тесты или cleanup предыдущего ID. При явном «закончить немедленно / остановись» немедленно прекратить действия и новые запуски; если собственный процесс ещё работает, допустимо только необходимое прекращение этого процесса и его дочерних процессов, затем никаких новых report/hash/cleanup действий. Кратко сообщить состояние уже выполненного по доступному контексту. Не повторять описанное пользователем поведение многократного продолжения предыдущей задачи после её смены. До запуска процессам задаются конечные таймауты; timeout не разрешает продолжать работу после stop. Если обучение технически не началось, его уже зафиксированный статус — `TRAINING_NOT_STARTED`, не отрицательный результат гипотезы; оставшиеся `status.json`/логи сохраняются.

## 2. Первичные доказательства и новая часть

Source pin: `lubochka/llm_mvp_core@0b76ab950d82534f5904005dda096545505ae307`. Машиночитаемый паспорт — [ast-sib-cache-source-list.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/evidence/ast-sib-cache-source-list.json>); общий порядок — [04-REGISTRY.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/04-REGISTRY.md>).

1. `StructuralVectorExtractor.Extract`, [L54–59](<C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:54>), для каждого узла создаёт `node.Parent.ChildNodes().ToList()`, затем ищет узел через `IndexOf`.
2. Настоящий `TrainFromSource`, [L124–141](<C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:124>), парсит C#, обходит все соседние узлы, вызывает этот extractor и передаёт пары в `Train`. `Train` заменяет `_training` этими парами, [L117–120](<C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:117>).
3. `Predict` затем читает `_training`, считает прежнюю distance, берёт `k` соседей и голосует, [L145–167](<C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:145>). Здесь есть реальная запись и чтение выученной памяти.

Для родителя с `m` детьми, у которых извлекаются признаки, baseline повторяет материализацию списка примерно `m` раз и последовательно ищет позиции. Суммарная работа этого участка растёт как `m²`; это вывод из циклов, **не измеренные ms/bytes**. Область возможного выигрыша — широкие C# блоки, argument/initializer lists, generated code. У узких деревьев словари B могут стоить дороже; именно поэтому есть отдельный узкий профиль.

Ближайшее существующее покрытие — **P26.4** про использование `SiblingCount` в расстоянии, **P27.6** про `%200` codec и прежний [57/argument-gap](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/01-structural-argument-gap.md>). Здесь не меняется ни один признак, расстояние, аргументный ordinal, ограничение 255 или целевой label: меняется цена вычисления тех же данных во время native обучения. Адресный поиск по всем **734 planning Markdown и 330 protected Markdown** не нашёл прежнего опыта такой цены. Receipt и фактические совпадения: [ast-sibling-novelty-search.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/evidence/ast-sibling-novelty-search.json>). Это адресная проверка новизны, а не утверждение, что весь смысл всех документов исчерпывающе доказан поиском строк.

<!-- BEGIN book-math-pass6-2026-10-22:ast-sibling-cache -->
<a id="book-math-ast-sibling-cache"></a>
## Почему одинаковые структурные признаки можно подготовить дешевле

**Пример для студента.** Для каждого ученика нужно записать номер в списке и размер класса. Пересчитывать весь класс отдельно для каждого ученика расточительно. Достаточно один раз пройти список, проставить номера и сохранить общее число. Ответ для каждого ученика останется тем же. В AST роль класса играет родительский узел, а роль учеников — его непосредственные дети; среди признаков нужны позиция ребёнка и число соседей.

**Связь с книгой.** [§2.5, печатные с. 61–64 / PDF с. 97–100](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0093-0100-ch02-encoder-cost-and-matrix-structure.md#section-2-5), отдельно разбирает цену вычислений. Здесь используется этот способ рассуждать о стоимости; формулу ниже мы выводим для конкретного обхода дерева. Книга не доказывает теорему о данном AST-кэше или обучении kNN.

**Наш подсчёт.** Пусть у родителя $p$ есть $k_p$ детей, а признаки запрашиваются для $q_p$ из них. Старый путь каждый раз создаёт список всех детей. Новый путь при первом обращении записывает позиции и число детей, затем читает готовую запись:

$$
W_A=\Theta\!\left(\sum_{p:q_p>0}q_pk_p\right),\qquad
\mathbb E[W_B]=O\!\left(\sum_{p:q_p>0}k_p+\sum_p q_p\right).
$$

При обработке всех детей $q_p=k_p$ это переход от $\Theta(\sum_p k_p^2)$ к ожидаемому $O(\sum_p k_p)$. Слово «ожидаемому» относится к обычной стоимости поиска в хеш-таблице. Общая форма с $q_p$ учитывает и неполный обход, включая исключённый последний AST-узел. Сокращается подготовка этих метаданных; парсинг, остальные признаки и запись учебных пар по-прежнему требуют работы.

**Почему учебный смысл сохраняется.** В [A](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/Native/A.cs), [B](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/Native/B.cs) и [сохранённом diff](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/source-ab.diff) кэш живёт внутри одного Train и относится к тому же неизменному дереву. Сохраняются идентичность узлов, первая найденная позиция и ограничение значения сверху числом 255. Публичный путь извлечения признаков для запроса остаётся прежним. Полученные признаки записываются тем же learner, а ответы читает тот же предиктор.

**Что измерено.** В [host](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/Program.cs) на каждый профиль и плечо приходится один Train. Измерен весь Train, включая парсинг, подготовку признаков, кэш и запись пар:

| Профиль и raw | Максимум детей у родителя | Выделенные байты A → B | Время A → B | Пар после Train; точность A и B |
|---|---:|---:|---:|---|
| Узкий: [nA](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/nA/metrics.json), [nB](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/nB/metrics.json) | 128 | 2990888 → 2593904 (−13.3%) | 64.8875 → 46.8676 мс | 5122; 0/16 → 12/16 |
| Широкий: [wA](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/wA/metrics.json), [wB](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/wB/metrics.json) | 1025 | 35788384 → 2302072 (−93.6%) | 238.6209 → 28.5732 мс | 4106; 0/16 → 7/16 |

SHA упорядоченных учебных пар совпадают между A/B для каждого профиля. Совпадают также все 16 ответов до и после обучения в соответствующих файлах predictions: [nA](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/nA/predictions.json), [nB](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/nB/predictions.json), [wA](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/wA/predictions.json), [wB](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/wB/predictions.json). Точность относится к компактным идентификаторам следующего узла, а не к свободному написанию C#.

На широких списках повторно обходилась большая часть одних и тех же данных, поэтому там механизм особенно полезен. Зафиксирован один холодный процесс на плечо и профиль, с JIT в измерении: устойчивое восьмикратное ускорение всей системы отсюда не следует. Выделенные байты — не пиковая RAM. Сохранённый успех состоит в более дешёвой подготовке тех же учебных признаков и пар, при том же количестве уроков. Этот разбор не назначает новый запуск и не меняет статус карточки.
<!-- END book-math-pass6-2026-10-22:ast-sibling-cache -->

## 3. Замкнутый паспорт: модель, пути, входы, зависимости

| Поле | Фиксированное значение |
|---|---|
| ID | `AST-SIB-CACHE-01` |
| Обучаемый механизм | Исходный `SemanticCompiler.Prediction.StructuralKnnPredictor`, `k=5` — его default. Начальная память пуста. Neural model, LoRA, DPO, токенизатор BPE и внешние teacher/judge здесь не участвуют |
| Модель исполнителя и reviewer | Та же фактически назначенная модель и настройки, что у соответствующего агента по умолчанию. Не переключать модель; в отчёте записать фактическую, не выдумывать имя |
| Язык карточки, промпта и итогового отчёта | Русский; ID, paths, C# и названия метрик сохраняются |
| Применяемые навыки | Точные файлы в `SourceCore/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `learning-path-audit-SKILL.md`, `code-examination-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `plan-self-validation-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `human-communication-GLOSSARY.md`. Применить к одному native пути; правила пользователя выше старых skill gates |
| Source | `12.SemanticCompiler/Prediction/StructuralKnn.cs`, source blob `b3aa5d376be3f93423997674dc015cea7e9fbf58` |
| Native A / B | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/Native/A.cs` и `B.cs` |
| Host | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/Runner.csproj`, `Program.cs`, `prepare.py`, `run.py` |
| Данные | В host: `data/train-narrow.cs`, `data/train-wide.cs`, `data/heldout.cs`; точная генерация ниже, одинаковые bytes обоих arms |
| Результаты | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/`; вложенные `nA`, `nB`, `wA`, `wB` |
| SourceCore / Core / Planning | SourceCore=C:/Projects/llm_mvp_core только для pinned git show; Core=Host=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01; Planning=C:/Users/lubao/Documents/xiigen/llm mvp/design to code session только для чтения справок, не вычисляется по соседнему checkout. Native=Host/Native; Out=Host/results/attempt-0001. |
| Framework / package | `net9.0`, `Microsoft.CodeAnalysis.CSharp` **4.12.0** — как в текущих [12 csproj](<C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompiler.csproj:3>) и [35 csproj](<C:/Projects/llm_mvp_core/35.ExperimentsV2/ExperimentsV2.csproj:3>). `SyntacticRoleMapper` уже в копируемом файле; иных source dependencies нет |
| Бюджет | 4 `TrainFromSource`: narrow A, narrow B, wide B, wide A; 1 обучение в каждом свежем процессе. 0 warm-up, 0 повторных обучений, 0 внешних/LLM генераций. 16 frozen native query до и 16 после каждого обучения: всего 128 `Predict` |

Это полный рецепт подготовки отдельного micro-проекта; `Runner.csproj`, `prepare.py`, `Program.cs` и native копии пока не созданы. Ссылка на весь `ExperimentsV2.csproj` или `SemanticCompiler.csproj` не нужна. Получатель исправления после выбора пользователем — `12.SemanticCompiler/Prediction/StructuralKnn.cs::TrainFromSource/Extract`. Отдельный caller `SemanticCompilerFacade.TrainStructuralKnn` также повторяет extractor; его будущая интеграция потребует передавать аналогичный cache, созданный на один разобранный файл. **Данный опыт измеряет именно `TrainFromSource`; ускорение façade без отдельной интеграции не заявляется.**

Разрешённые экспериментальные исключения заданы заранее: один `Program.cs`, обычные DTO, `void`, `throw` и `try/catch` для запуска/фиксации ошибок, read-only reflection для snapshot вне измеренного Train. Они допустимы в этой изолированной experimental копии и не разрешают перенос таких исключений в production. Product DNA-процедуры, full CI, исправления продуктовых тестов и оформление production-внедрения не являются prerequisite этого опыта.

## 4. Единственное изменение B

`prepare.py` читает source bytes командой `git -C <SourceCore> show 0b76ab950d82534f5904005dda096545505ae307:12.SemanticCompiler/Prediction/StructuralKnn.cs` через `subprocess.check_output` (без shell), сохраняет original SHA/blob provenance и создаёт две копии **в коротком Core**. Команда никогда не ищет Git в `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01`. Только namespace меняется одинаково: `SemanticCompiler.Prediction` → `AstSibCache01.A` / `AstSibCache01.B`. A далее остаётся исходной. В B сделать следующие изменения:

- Добавить `SiblingFeatureCache`, живущий только в одном вызове `TrainFromSource` и одном синтаксическом дереве. Он хранит для каждого родителя его capped child count и для каждого ребёнка capped index; прежняя семантика `Math.Min(...,255)` и fallback index 0 сохраняется.
- Сохранить публичный `Extract(node,fileId=0)` без cache для всех существующих query callers; его исходное поведение остаётся. Внутренний overload принимает cache. В нём изменяется только блок L54–59, остальной расчёт вектора скопирован без изменения.
- В `TrainFromSource` после `nodes = …ToArray()` создать cache и передать его в вызов extractor. `Train(pairs)` выполняется как прежде. По возврату cache больше не хранится в модели или static state.

Однозначная реализация cache, которую использует внутренний overload B:

```csharp
internal sealed class SiblingFeatureCache
{
    private readonly Dictionary<SyntaxNode, byte> _counts = new();
    private readonly Dictionary<SyntaxNode, byte> _positions = new();

    internal (byte Position, byte Count) Get(SyntaxNode node)
    {
        if (node.Parent is not { } parent) return (0, 0);
        if (!_counts.TryGetValue(parent, out var count))
        {
            var children = parent.ChildNodes().ToArray();
            count = (byte)Math.Min(children.Length, 255);
            _counts.Add(parent, count);
            for (var i = 0; i < children.Length; i++)
                _positions.TryAdd(children[i], (byte)Math.Min(i, 255));
        }
        return (_positions.TryGetValue(node, out var index) ? index : (byte)0, count);
    }
}
```

Используется default equality `SyntaxNode`, как в исходном `List.IndexOf`; `TryAdd` сохраняет первое вхождение. Root, byte saturation, fileId, parent chain, depth, role mapping, `%200`, `_training` insertion order, `k`, distance, tie policy, seed и запросы не менять. Не оптимизировать заодно ancestors, parsing или сортировку KNN. Не добавлять global/static cache, pooling и удержание AST между вызовами.

## 5. Точные данные и before/after

Данные генерируются в `prepare.py` до измерений в UTF-8 без BOM, с LF. Для `profile=narrow`: `methodCount=128`, `width=8`; для `wide`: `methodCount=1`, `width=1024`. В обоих профилях ровно 1024 оператора `x += ...;`; число остальных AST-узлов различается и записывается, не считается одинаковым.

```python
def training_source(method_count, width):
    out = ['namespace AstTrain { public sealed class Lessons {']
    for m in range(method_count):
        out.append(f'public int M{m:03d}(int x) {{')
        for i in range(width):
            out.append(f'x += {m * width + i + 1};')
        out.append('return x; }')
    out.append('} }')
    return '\n'.join(out) + '\n'
```

`heldout.cs` имеет ровно следующий текст. Он не поступает в `TrainFromSource` и не используется для выбора размера cache/параметров B:

```csharp
namespace AstExam {
  public sealed class Questions {
    public int H(int x) {
      x += 2001;
      x -= 2002;
      if (x > 2003) { x += 2004; }
      return x;
    }
  }
}
```

До запуска зафиксировать SHA256 всех трёх файлов. Program один раз разбирает `heldout.cs`; из `GetRoot().DescendantNodes().ToArray()` берёт первые 16 пар `(node[i], node[i+1])`. Gold — `NodeTypeId(node[i+1])`, то есть существующий compact ID; сырые SyntaxKind сохранить рядом. Если пар меньше16, это ошибка подготовки и до обучения не запускать ни один arm. В каждом arm `Predict` получает исходный public extractor для query, без cache; это read path настоящей выученной модели.

До обучения сохранить `TrainingSize=0`, snapshot пустой `_training`, 16 predictions и `next_compact_id_accuracy_before`. После **одного** native `TrainFromSource(trainText,fileId:0)` сохранить полный ordered `_training`, 16 predictions и `next_compact_id_accuracy_after`. `_training` читается через reflection только для отчёта, вне timed section; не подставлять/дописывать пары через reflection. Для canonical snapshot записывать по очереди все 10 полей `StructuralVector` и `NextToken`, в фиксированном порядке полей/записей, одинаковом в A/B; SHA namespace-dependent JSON типов не использовать.

Это измеряет обучение структурной памяти и сохранность её вывода. `next_compact_id_accuracy` — корректность текущего compact codec на этой панели; она не равна компилируемости программы или смысловой точности свободного C#-ответа. Для EN/RU/HE/ES, SQL без C# AST и обычного порядка слов этот эксперимент не устанавливает полезности cache или новых структурных законов.

## 6. Измерения и формат результата

Timed section содержит **весь `TrainFromSource`**: Roslyn parse, создание nodes/pairs/cache, извлечение векторов и запись `_training`. Подготовка данных, сборка, процессный startup, сериализация snapshot и evaluation находятся вне секции. В текущем синхронном вызове измерять `Stopwatch.GetTimestamp()` и `GC.GetAllocatedBytesForCurrentThread()` до/после. Никаких ручных `GC.Collect()`; первый JIT, если он попал внутрь вызова, входит в отмеченный **cold training time**. Это один запуск, не оценка распределения latency. Меньшее время — полезное наблюдение этого запуска, без заявления устойчивого causal speedup; allocations и state показывают отдельно. Порядок narrow A→B / wide B→A фиксирован, но не является контролем порядка на одинаковых inputs: профили различаются.

| Показатель | Что записать для каждого профиля |
|---|---|
| `train_allocated_bytes` | A, B, `B-A`; меньше полезно. Это allocation volume, не peak RAM |
| `train_elapsed_ms` | A, B, `B-A`; меньше полезно. Полная секция, одинаковые границы |
| `training_calls` / `TrainingSize` | 1 на arm; before0, after фактический count. Не нулевое обучение не заменять фиктивным счётчиком |
| `trained_pairs_sha256` | Полный ordered state after; совпадение A/B и любые отличия каждого поля |
| `next_compact_id_accuracy` | A0→A1, B0→B1 и разность этих изменений на одной панели |
| `prediction_disagreements` | Число и конкретные query IDs с разными A1/B1 outputs |
| Остальные характеристики | Количество входных bytes, AST nodes, максимальная ширина родителя; runtime/package версии; build/exception status. Peak RSS, CPU time и semantic free-answer accuracy — `NOT_MEASURED`, если собственного измерителя нет |

Меньше allocations в wide при прежней accuracy — успех `train_allocated_bytes / C# wide AST`. Регрессия allocations в narrow или времени в любом профиле показывается рядом и ограничивает область переноса. Рост accuracy не обязателен и не задаёт порог допуска. Если state A/B различается, сохранить полезные измеренные изменения и все регрессии, но не писать «эквивалентная функция доказана»: нужен разбор точного первого расхождения без автоматического нового запуска. Если native обучение не завершилось либо один arm обучен на другом содержимом, сравнение стоимости равной работы не установлено; report содержит ошибки и фактические измерения, гипотеза остаётся без подтверждения.

Каждый из `nA/nB/wA/wB` содержит `metrics.json`, `state-before.json`, `state-after.json`, `predictions.json`, `status.json`, `stdout.txt`, `stderr.txt`. Корень attempt содержит `manifest.json`, `source-ab.diff`, `comparison.json`, `report.md`, `candidate-fix.md`. `report.md` по-русски — **до350 слов плюс таблицы** — начинается: «Меняли …; реально обучалось …; измерили …; остальные показатели …». Затем таблица всех метрик, положительные показатели отдельно по профилям, ухудшения/ограничения, первичные filenames и source pin. `candidate-fix.md` — **до120 слов** — указывает исходный receiver и ровно экспериментальный B diff; итог `CANDIDATE_FOR_USER_DECISION`, без автоматического production переноса. Пока не поступил STOP/NEXT, разрешён один readback сохранённых raw/summary для соответствия отчёту; он не добавляет обучений, предсказаний, тестов или повторов. При STOP/NEXT этот readback тоже прекращается.

## 7. Проект и команды без поиска scaffold

`Runner.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>AstSibCache01</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
    <Compile Include="Program.cs" />
    <Compile Include="Native/A.cs" />
    <Compile Include="Native/B.cs" />
  </ItemGroup>
</Project>
```

Исполнитель создаёт `prepare.py`, `run.py` и `Program.cs` по этому рецепту в коротком Host. `prepare.py` создаёт только перечисленные native copies, проект, данные и manifest; отказывается перезаписывать существующие файлы с другими bytes. `Program.cs` поддерживает `--arm A|B --profile narrow|wide --output <directory>` и read-only режим `--compare <attempt>` для таблицы/русского отчёта. В compare нет `TrainFromSource`, новых inference или запуска процесса. Все raw writes — `FileMode.CreateNew`; существующий частичный attempt не удалять.

`run.py` — контроллер с фиксированной таблицей `phase→argv/timeout`: `build` вызывает только `dotnet build <Host>/Runner.csproj -c Release -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false`, лимит180с; `nA/nB/wB/wA` вызывают DLL `<Host>/bin/Release/net9.0/AstSibCache01.dll` с соответствующими `--arm`, `--profile`, `--output <Out>/<phase>`, лимит60с на каждый; `compare` вызывает ту же DLL `--compare <Out>`, лимит15с. `prepare.py` имеет общий лимит45с; его subprocess version/source reads также имеют finite timeout в остатке бюджета. **Сумма лимитов процессов —480с**; паузы между командами в эту сумму не входят, глобальный wall deadline не заявляется. Это потолки отдельных процессов, не назначенные дополнительные повторы. Python `subprocess` получает список аргументов без shell. Контроллер заранее захватывает stdout/stderr в перечисленные raw, при timeout прекращает только своё дерево процесса (`taskkill /PID <ownedPid> /T /F` на Windows), записывает `process-status.json` и возвращает ненулевой exit; автоматического повторного запуска нет. При explicit stop/next прекращение этого дерева — единственная допустимая техническая завершающая операция. Directory для arm может содержать эти process logs; прежние metrics/state/status не перезаписывать.

PowerShell, из корня уже открытого core checkout, после реализации только этого host:

Literal C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/Run-Assigned.ps1 — единственный владелец подготовки и всех фаз:

```powershell
$ErrorActionPreference = 'Stop'
$SourceCore = 'C:/Projects/llm_mvp_core'
$Core = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01'
$Planning = 'C:/Users/lubao/Documents/xiigen/llm mvp/design to code session' # read-only
$HostDir = $Core
$Out = Join-Path $HostDir 'results/attempt-0001'
if (Test-Path -LiteralPath $Out) { throw 'ATTEMPT_EXISTS_NO_REPLAY' }
$Claim = [System.IO.File]::Open((Join-Path $HostDir 'execution.claim'), [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
$Claim.Dispose()
& 'C:/Python314/python.exe' (Join-Path $HostDir 'prepare.py') --source-core $SourceCore --core $Core --planning $Planning --out $Out
if ($LASTEXITCODE -ne 0) { throw 'AST preparation failed; no training started.' }
function InvokeAstPhase([string]$Phase) {
    & 'C:/Python314/python.exe' (Join-Path $HostDir 'run.py') --host $HostDir --out $Out --phase $Phase
    if ($LASTEXITCODE -ne 0) { throw "AST phase $Phase failed; no automatic retry." }
}
InvokeAstPhase 'build'
InvokeAstPhase 'nA'
InvokeAstPhase 'nB'
InvokeAstPhase 'wB'
InvokeAstPhase 'wA'
InvokeAstPhase 'compare'
```

Единственная внешняя команда:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/Run-Assigned.ps1
```

Исполнять следующую фазу только после успеха предыдущей; при неуспешной сборке обучение не начинать. Preflight `prepare.py` проверяет `SourceCore`/`Planning`, source blob, наличие .NET9/Python, полный путь каждого planned artifact <240 символов и отсутствие существующего attempt **до любого Train**. Package4.12.0 восстанавливается только обычной build-командой с её лимитом; отсутствие package — build failure, не отрицательный результат обучения. Все native/host/build пути уже находятся в коротком `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01`; полный repo path внутрь них не вкладывается. Если отдельный целевой planning output путь всё ещё длиннее240, фиксировать этот конкретный подготовительный blocker до обучения; не менять product/source; registry запись остаётся разрешённой документацией и не запускать опыт без места для его raw. После результата файл не надо переносить из тайной временной папки: raw сразу пишется в назначенный Out.

Конфликт существующего attempt-0001 или legacy 20261020-01: сохранить его целиком, **не создавать автоматически02/03 и не запускать обучение**. Сообщить конкретный existing-attempt status по имеющемуся контексту и перейти к следующему явно назначенному registry ID; если назначен только AST — `END`. Новый attempt может быть указан отдельным последующим заданием, а не выбран в попытке обойти уже завершённый/частичный запуск. Нельзя запускать повторно arm, уже завершивший обучение. Чужое изменение HEAD не означает повтор: source pin остаётся указанным. Если ровно нужный source/package недоступен после конкретной попытки, записать `PREPARATION_BLOCKED` с командой/ошибкой; не чинить production и не превращать это в отрицательный учебный результат. Не требовать нового GitHub разрешения, когда доступ уже авторизован пользователем.

<a id="8-готовый-промпт-исполнителю"></a>
## Исторический текст задания и свидетельства

Действующий передаваемый текст находится в [начале карточки](#current-executor-task). Нижний текст сохранён как история; его императивы не назначают новый запуск или продолжение уже закрытого этапа.

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

> Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Полная карточка: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md. Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. Выполни только AST-SIB-CACHE-01 из этой карточки, если текущая задача уже перешла к реализации/запуску. В документном проходе ничего не запускай. Работай с моделью и настройками по умолчанию; отчёт по-русски. Применяй точные десять skill-файлов из паспорта с приоритетом свежих правил Любы. Source pin0b76, read-only SourceCore, короткий WorkCore C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01, native A/B paths, net9/Roslyn4.12, три Compile файла, две формы учебного C# и фиксированная16-query панель уже заданы: не ищи другой проект, модель, данные или место отчёта. A копирует настоящий StructuralKnn; B добавляет только scoped sibling cache внутри TrainFromSource. Четыре native обучения суммарно, без прогрева/повторов/LLM-generation; одинаковые исходные bytes, k5, roles/parents/ordinal/caps/codec/voting. Соблюдай таймауты prepare45с/build180с/каждыйarm60с/compare15с и прекращай только своё process tree при timeout. Измерь полный TrainFromSource и его настоящую память до/после, не отдельный cache-loop. Любое измеренное полезное улучшение — успех по показателю и домену без требования роста accuracy; все остальные изменения и ухудшения покажи рядом. Single cold time не выдавай за устойчивый speedup, эквивалентность learned state не объявляй при расхождении. Собери только Runner.csproj; без full CI, Main500, production fixes, удаления старых экспериментов или автоматического commit/push. Product/source остаются read-only; полезный кандидат документируется в разрешённом required-changes.md; результат — только C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001. При конфликте используй ровно описанные правила, ничего старого не удаляй. Зафиксируй отчёт и candidate-fix для решения пользователя только пока эта задача активна; не продолжай цепочку внедрения сам. При команде «следующий» немедленно прекращай старую задачу и переходи к явно назначенному следующему ID: без дописывания её отчёта/registry, review, hash checks и новых запусков. При «остановись немедленно» прекращай все действия; допустимо только необходимое прекращение собственного активного процесса и его детей, затем никаких tool calls/cleanup. Завершённый удачный показатель не требует повторного опыта или исправления продуктовых тестов для признания успеха.

**Единая очередь:** [AST-SIB-CACHE-01](AST-SIB-CACHE-01.md) → [NG01](NG01-null-guard-label.md); далее только [действующий индекс](00-execution-index.md#next-contract). При одиночном AST — END. Source12 — историческое согласование.


## Окончательные пути и один владелец

SourceCore=C:/Projects/llm_mvp_core читается только через pinned git show. Host=Core=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01; копии A/B находятся в Host/Native. Prepare генерирует XML с Compile Native/A.cs, Native/B.cs и Program.cs; прежняя вложенность Core/experiments-v2 и относительные ../../../35 не используются. Planning=C:/Users/lubao/Documents/xiigen/llm mvp/design to code session — только справочный источник. Out=Host/results/attempt-0001.

Run-Assigned.ps1 захватывает execution.claim до prepare. Prepare ровно один раз эксклюзивно создаёт Out; run.py фаз не выполняет второй prepare и не отклоняет свой Out. В Program каждого nA/nB/wB/wA до construction и первого Predict создать arm-root/training.claim через CreateNew. При существующем claim, attempt или неизвестном исходе сохранить partial; повторный arm, новый номер и другой root не назначены. Таймауты 45/180/60×4/15 секунд и бюджет 4 Train/128 Predict сохранены.

SKNN-SIBLING-PREP-01 — справочный alias AST, без отдельного запуска и сложения бюджетов. Действующий next_id — [NG01](NG01-null-guard-label.md); дальнейший маршрут — [текущий индекс](00-execution-index.md#next-contract). Для одиночного AST — END. Модель сессии по умолчанию; STOP/NEXT немедленно прекращает прежнюю задачу.
