# NEURAL-FROZEN-TRANSPOSE-01 — стоимость повторного transpose замороженных матриц при SFT/LoRA

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница CORE/LEARN:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Исходный адрес и роль:** `04.TrainingLoop/Trainer.cs::TrainSupervisedNextTokenStep` — настоящий learner; фактор — experimental `00.TensorCore/Layers/Linear.cs::Forward`, reuse transpose только frozen Weight. `10.LoRA/LoraLinear.cs` и trainable adapters сохраняют свой путь. Это SFT/LoRA evidence, не измеренный DPO gain. Исторический pin и собственный бюджет ниже сохраняются; роль зависимости не равна factor owner, runtime сейчас не выполнялся.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md>), [отчёт 2](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/report.md>).
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

**Маршрут по ID:** [NEURAL-FROZEN-TRANSPOSE-01](NEURAL-FROZEN-TRANSPOSE-01.md) → [BPE-NO-MATCH-COPY-01](BPE-NO-MATCH-COPY-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Разрешение конфликтов из последнего аудита (20.10).** Старый запрет копий в `81-validation-program.md` не применяется: используется только назначенная A/B-копия; product/tests/CI не меняются. Ограничение «protected не менять» в отчёте помощника относилось к его поручению, а не отменяет прямое поручение Любы исправить карточки. `NOT_RUN` означает отсутствие результата данной версии, а не запрет; поздняя карточка не меняет протокол уже сохранённого опыта и не назначает повтор. Любое полезное измеренное улучшение — успех по метрике/домену, без порога5% и требования роста accuracy; несопоставимые затраты не приписываются фактору. Связанные подготовка, restore и чтение выученного состояния допустимы с точным source→state→consumer; посторонний scorer-пример не назначает опыт. Вместо отсутствующей84-ссылки — [реестр источников](../01-full-source-registry.md) и [карта покрытия](../2026-10-19-small-learning-probes/06-findings-map.md); следующая работа задаётся [индексом](00-execution-index.md). Назначенные данные, домен, host и команды нельзя заменять самостоятельным поиском другого опыта.

**Порядок программы:** [NEURAL-FROZEN-TRANSPOSE-01](NEURAL-FROZEN-TRANSPOSE-01.md) → [BPE-NO-MATCH-COPY-01](BPE-NO-MATCH-COPY-01.md). Для общей карточки использовать только якорь назначенного ID. Позиции — [текущий индекс](00-execution-index.md); условия перехода — [правило NEXT](00-execution-index.md#next-contract). Сохранённые попытки не повторять; одиночное поручение — END.

## Немедленное завершение по указанию Любы

**Обязательно для каждого подслучая и каждого промпта исполнителю этой карточки.** Прямое текущее указание Любы имеет приоритет над нижними требованиями завершить отчёт, review или весь цикл.

- **«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу.
- Если следующее задание уже указано, не выбирать другое и не обходить каталог в поисках более готового. Если сказано «следующий» или назван порядковый номер — взять его из единого индекса `cards/00-execution-index.md`; не искать N-KU-02 по аналогии с N-KU-01 и не задавать вопрос о выборе. После конца назначенной очереди кратко сообщить об этом, не возобновляя прошлый опыт. Поручение редактировать карточки или описывать опыт само по себе не разрешает запуск обучения.
- **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Команду перехода не подменять остановкой всей программы; команду полной остановки не подменять следующим запуском.
- Незавершённые файлы и уже записанные данные оставить как есть. Их наличие не разрешает продолжать вопреки указанию. Не удалять raw/receipts и не устраивать закрывающую проверку; краткий статус — только по уже известным фактам, без новых инструментов.
- Исполнитель опыта не берёт на себя исправление карточек: это отдельное поручение документной сессии. Остановка одного исполнителя не отменяет отдельное поручение другой сессии, если Люба не остановила и его.

**Актуально на 20.10.2026: сохранённый A/B уже завершён; текущее поручение — документная сверка.** Raw, execution и отчёт находятся в указанном ниже canonical results-root. Здесь прочитаны существующие результаты; нового build/обучения/повтора не было. Старые helper `NOT_RUN`/`SDK_UNAVAILABLE` описывают его прежнюю среду. Нижние команды сохраняют рецепт выполненного опыта, а не новое задание на запуск; занятые results/bin/obj не очищать. Исполнитель и последовательный reviewer используют модель своей текущей сессии по умолчанию, без override; фактическое имя записывается, если известно. Один worker, без вложенных. Отсутствующее имя модели прошлого Kiro не выдумывать и не делать поводом повторить опыт.

**Исходное поручение:** «рассмотри последние обновления помощников и выстрой эксперименты, как можно более простые и маленькие для каждого домейна»; «модель не должна сама искать куда и как ей поместить эксперимент, как его запустить, как решить конфликт». Изменение — только в предоставленной экспериментальной копии. Продуктовый перенос решает Люба по результату.

Источник: [карточка помощника](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/README.md>). Его source pin — `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`; сравнение здесь относится к закреплённым копиям, не к произвольному более новому HEAD. Не выдавать source-derived ожидание за новый native результат.

## Сохранённый результат и положительный учебный контроль

[Raw A](../results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/A/code-sign.json), [raw B](../results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/B/code-sign.json), [execution](../results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/execution.json), [отчёт](../results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md): два exit 0, по 4 optimizer steps, реальные изменения trainable, frozen неизменны. Конечные trainable и optimizer hashes совпадают, все 4 loss и все before/after logits совпадают. Allocations13 647 840→12 140 448 байт; B8 miss / 24 hit, удержанный cache payload 249 856 байт. Это локальный выигрыш по выделениям при той же четырёхшаговой траектории.

EXAM target NLL 8.01533412366764→7.974559659894938 в обоих плечах — положительный вероятностный сдвиг. Exact0/2→0/2 не доказывает освоение знака и не доказывает поломку learner. Время 84.3133→84.57 мс, CPU 62.5→78.125 мс, process peak 37 105 664→37 031 936 байт **измерены**; причинный эффект кэша по этим трём осям не установлен одним фиксированным A→B. Их нельзя заменять «не измерено», скрывать наблюдённое ухудшение или автоматически делать причиной JIT/шума. Whole-process peak включает инициализацию/первый EXAM.

Отдельная уже сохранённая [LEARN-SANITY-PROBE](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json>) содержит один завершённый результат с 400 native SFT updates, checkpoints 0/4/20/100/400 и 20 readouts: 2 TRAIN + 2 EXAM на каждом. Это бюджет записанной траектории; совокупный расход всех попыток запуска не аттестован (см. уточнение ниже). [Program.cs:47](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs:47>) задаёт те же config/seeds/qv-LoRA/AdamW/LR. [Program.cs:61](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs:61>) выключает кэш; [обновления:74](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs:74>) вызывает native TrainSupervisedNextTokenStep. [Runner.csproj:6](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Runner.csproj:6>) подключает LFT01/src; [Run-Probe.ps1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Run-Probe.ps1>) и [build.log](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/build.log>) описывают прежнее исполнение. Здесь их только прочитали.

Сохранённый положительный контроль **частичный**: TRAIN exact 0/2→1/2, EXAM 0/2→1/2; TRAIN NLL 7.960885496452843→1.8879445998230966, rank 206.5→1.5; EXAM NLL 8.01533412366764→2.7988595276200154, rank 215→2.5. Полная посадка2/2 не достигнута (`learner_fits_own_training_examples=false`). Порог «есть хотя бы один gained exact и снижение NLL/rank» описывает сохранённый частичный сигнал, не выдаётся за его предварительную регистрацию. Первые 4 losses и EXAM NLL совпали с A/B, но проба не сохраняет initial/final weights/moments hashes, optimizer-step receipt и loaded assembly identity: exact состояние на шаге4 этим совпадением не доказано. Она не содержит B-cache 400; не доказывает его health на 400 шагах или арифметическую генерализацию. Это связанный read-only диагностический результат, а не расширение бюджета A/B и не новая строка очереди. Новых запусков, адаптации числа шагов до успеха и гарантированного future PASS не назначено.

Подробное происхождение, несовпадения интерпретаций и ограниченные gaps: [additional-conflicts-evidence](../../2026-10-20-additional-conflicts-evidence.md).

### Уточнение по новому журналу обучаемости — без нового запуска

[Журнал 9b5077e0, строки 78–108](<C:/Users/lubao/.codex/attachments/9b5077e0-c496-44ad-991a-a0000bb5fb39/Pasted text.txt:78>) описывает старт terminal session37, её остановку, затем session38 и остановку после обнаружения результата; это tool IDs, не подтверждённые OS/native PID. [Program.cs:74–85](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs:74>) выполняет обучение и readouts **до** [FileMode.CreateNew:135](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs:135>). [Run-Probe.ps1:4–9](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Run-Probe.ps1:4>) перед build/Train не проверяет существующий output и пишет оба запуска в одинаковые журналы. CreateNew защищает прежний результат от перезаписи, но не предотвращает повторное обучение до отказа записи. Пустой run.log не доказывает exit0 или отсутствие второй учебной попытки. Подтверждены одна полная записанная траектория 400/20 и два сообщения start/terminate; неизвестно, достигла ли вторая попытка native exe, сколько updates/forwards она выполнила и какая попытка записала результат. Нельзя подставлять ни 0, ни 400 дополнительных updates или подтверждённый aggregate 800. Уточнение относится к sanity, не отменяет отдельные два exit0 четырёхшагового A/B.

На шаге 400 TRAIN выдал `+,+`, EXAM — `-,-` при gold `+,-` на каждой панели. Результат 1/2 равен числу совпадений постоянного знака на такой сбалансированной паре; это прочтение сохранённых ответов, без нового inference. Положительный сдвиг exact/NLL/rank против начальных `=,=` сохраняется. Условное правило выбора знака, полный fit 2/2 и обобщение этим не подтверждены. `any_positive_movement` в [Program.cs:124–125](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs:124>) означает только снижение среднего TRAIN NLL; `learner_fits_own_training_examples` отдельно требует 2/2 и равен false. Это не сертификат всей исправности learner или кэша B.

Фразы журнала «четыре шага физически не могут», «нет ни одного положительного признака», «обучение ещё не началось», «нужны сотни» и «причина видна в коде» сильнее доказательств. При 4 уже менялись веса и NLL; при 400 frozen-head профиль сменил argmax. [Native Trainer:102–125](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/src/04.TrainingLoop/Trainer.cs:102>) обучает 21 shifted target для 22-byte prompt+answer, включая ровно 1 answer target. Полнострочная цель и q/v-LoRA с frozen head — подтверждённая конфигурация; их причинная ответственность за 0/2 не выделена. Включать response-only loss, размораживать lm_head, менять число шагов или требовать гарантированный PASS эта карточка не назначает.

**Контракт наблюдения.** В сохранённой sanity [Observe:142–169](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs:142>) принимает `LlmModel` и массив `(string Prompt,string Answer)`; один `Forward(training:false)` на строку читает последний логит полного словаря. Argmax при равных значениях сохраняет первый ID; gold rank = 1 + число logits строго выше gold. NLL считается через stable log-sum-exp. Используется `Bytes(answer)[0]`, поэтому observer пригоден именно для одно-byte `+`/`-`; это не оценка всего многотокенного ответа. У исходного [Observe:108–127](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs:108>) панель только EXAM, дополнительно сохраняются все last_logits. Добавленный TRAIN readout имеет стоимость: 5 точек × (2 TRAIN + 2 EXAM) = 20 forwards в записанном sanity; 8 EXAM forwards всего в исходном A/B. Новые callbacks/проверки и дополнительные вызовы ради отчёта не добавляются. Для иных Student автор отдельного рецепта заранее фиксирует подходящий target, панель, критерий и бюджет; single-byte observer не навязывается чужому tokenizer/профилю.

**Запуск и кандидаты.** Оба ресурсных кандидата уже записаны в [required-changes.md](../../required-changes.md); просьба «внести оба» не означает автоматический product перенос или регистрацию sanity как доказанного ремонта. Исправление контракта наблюдения — работа документации. При любом отдельно порученном будущем исполнении подготовка script должна завершиться до одного запуска; существующий результат проверяется до build/Train. Неизвестный исход наблюдают у того же процесса; он не разрешает повторный старт. По этой завершённой карточке не вызываются ни script, ни новый host. Немедленные «переходи»/«остановить всё» сохраняют приоритет над оформлением и регистрацией. Новое [evidence](../../2026-10-20-learning-signal-conflicts-evidence.md) содержит полный разбор 237-строчного журнала и пределы учёта попыток.

## Связь с фактическим обучением и один фактор

Native путь: [`Trainer.TrainSupervisedNextTokenStep:62–150`](<C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:62> "Исторический ref 62bcc8f2368840e22d2ea98ef16d9bd526ad9a39, L62-L150; локальный файл может отличаться") → `LlmModel.Forward(training:true)` → [`Linear.Forward:45–50`](<C:/Projects/llm_mvp_core/00.TensorCore/Layers/Linear.cs:45> "Исторический ref 62bcc8f2368840e22d2ea98ef16d9bd526ad9a39, L45-L50; локальный файл может отличаться") / [`LoraLinear.Forward:112–125`](<C:/Projects/llm_mvp_core/10.LoRA/LoraLinear.cs:112> "Исторический ref 62bcc8f2368840e22d2ea98ef16d9bd526ad9a39, L112-L125; локальный файл может отличаться") → native CE/backward/clipping → native `AdamW.Step`. В A и B действительно меняются обучаемые параметры.

A каждый раз транспонирует Weight исходным кодом. B при `!Weight.Value.RequiresGrad` лениво создаёт transpose один раз на экземпляр Linear и использует его повторно. Меняется только экспериментальная копия `00.TensorCore/Layers/Linear.cs`; `one-factor.diff` сохранён рядом. Общая ветвь переключения и счётчики являются инструментированием host. Body A остаётся исходным. LoRA A/B обучаемые и продолжают пересчитываться каждый раз.

Это опыт **SFT/LoRA**. DPO steps = 0; ни exact-pair admission, ни DPO runner не обходятся. Применимость того же механизма к DPO следует из общего Linear, но эта карточка не измеряет DPO gain и не доказывает улучшение RP01 end-to-end.

<!-- BEGIN book-math-pass6-2026-10-22:frozen-transpose -->
<a id="book-math-frozen-transpose"></a>
## Почему постоянную матрицу можно переставить один раз

**Пример для студента.** У преподавателя есть неизменная таблица перевода одних величин в другие. Для удобства вычислений он переписывает строки этой таблицы в столбцы. Переписывать её перед каждой новой задачей незачем; достаточно сохранить переставленную таблицу. Но саму новую задачу всё равно нужно посчитать, а полученную ошибку — передать назад ученику. Кэш убирает повторное изготовление справочника, а не работу над новым уроком.

**Что объясняет книга.** [Theorem 9.5, печатная с. 282 / PDF с. 318](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#theorem-9-5), различает градиент по входному представлению $\delta_mW_D^\top$ и градиент по параметрам $A_m^\top\delta_m$. В [§9.6, печатная с. 284 / PDF с. 320](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#display-9-feedforward-pullback), обратный сигнал проходит через матрицы слоёв. Поэтому фиксированные веса всё ещё участвуют в доставке ошибки к обучаемым параметрам, расположенным раньше в графе.

**Как это записано именно в нашем коде.** В книге линейный слой можно записать как $Y=XW$. В [сохранённом Linear.cs](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/src/00.TensorCore/Layers/Linear.cs#L22-L76) веса хранятся в обратном порядке осей: $F=\texttt{Weight[out,in]}$, поэтому:

$$
Y=XF^\top+b,\qquad W=F^\top,\qquad
\nabla_X L=G_YW^\top=G_YF.
$$

**Кэшируется $F^\top$ в прямом проходе `Forward`.** Это не создание транспонированной матрицы в backward. Обратное умножение $G_YF$ остаётся необходимым. Входы, градиенты и обучаемые LoRA-параметры меняются, поэтому вычисления с ними выполняются заново. Замороженная база позволяет сохранить только её постоянное представление.

**Наш расчёт экономии.** Для матрицы с $ab$ элементами и $r$ обращений повторное транспонирование копирует $O(rab)$ элементов. Однократное — $O(ab)$ плюс обращения к кэшу. Эта оценка относится к копированию, не ко всему обучению. В опыте восемь матриц используются четыре раза: вместо 32 построений зарегистрированы **8 промахов и 24 попадания** кэша. Условие корректности — тот же экземпляр слоя и неизменная база, без восстановления другого состояния. Автоматического сброса при изменении весов в экспериментальной реализации нет; такое изменение потребовало бы заново создать transpose.

**Что подтверждено.** [Raw A](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/A/code-sign.json) и [raw B](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/B/code-sign.json) содержат по четыре настоящих шага оптимизатора. Обучаемые параметры меняются, база остаётся прежней. Между плечами совпадают четыре loss, записанные logits до/после, итоговый SHA обучаемых весов и SHA состояния оптимизатора.

| Измерение | A → B | Смысл результата |
|---|---:|---|
| Выделенные байты | 13647840 → 12140448 | Снижение примерно на 11.0% |
| Удерживаемые кэшем float-данные | 0 → 249856 байт | Плата за повторное использование, без накладных объектов |
| Время обучения | 84.3133 → 84.57 мс | Выигрыш времени не наблюдается |

За эти четыре шага точные ответы остаются 0/2 в обоих плечах, хотя NLL снижается примерно с 8.015334 до 7.97456. Это фактические обновления с частичным прогрессом, а не освоенное правило знака. Отдельный более длинный учебный контроль и его ограничения уже описаны в разделе сохранённого результата выше. Опыт подтверждает экономию выделений при одинаковом зафиксированном ходе обучения; превращать её в измеренное ускорение или полное решение задачи оснований нет. Статус, бюджет и запрет повтора сохраняются.
<!-- END book-math-pass6-2026-10-22:frozen-transpose -->

## Модель, данные и состояние — выбирать ничего не нужно

| Поле | Фиксированное значение |
|---|---|
| Архитектура | Vocab256; Context64; Hidden64; 1 Transformer block; 4 Q heads; 2 KV heads; FFN176; RopeTheta10000; RmsNormEps1e-5 |
| Base initialization | `Initializers.UseDeterministicSeed(62001UL)` в каждом новом процессе |
| LoRA | `ApplySeeded`; rootSeed62002; stream `frozen-transpose-probe-v1`; q_proj/v_proj; rank4; alpha8; dropout0 |
| TRAIN | Native `TrainSupervisedNextTokenStep`, weight1, ровно 4 updates; порядок T1,T2,T1,T2 |
| Optimizer | Native AdamW beta1=.9, beta2=.95, eps=1e-8, weightDecay0 |
| Schedule | warmup0, totalSteps4, maxLr=minLr=.001, gradClipNorm1 |
| Токены | UTF-8 byte IDs 0..255; это представление BPE с нулём merges; EOS не добавляется |
| Формат SFT | Полная последовательность prompt+answer, обычный shifted next-token loss. Prompt masking не является вторым фактором |
| Исходное состояние | Независимая воспроизводимая инициализация A/B; hash frozen и trainable state до TRAIN |
| Глубина | 1 Transformer block, 2 LoRA adapter slots, 1 native learner; Flow отсутствует |

| Split | ID | Prompt — буквально | Answer — буквально |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Новые числа проверяют тот же маленький закон, но не доказывают широкую арифметическую генерализацию. Предсказанный token ID берётся из фактического argmax последнего логита, gold не подставляется вместо выхода. Ни C# compiler, ни ручное вычисление знака не выдают ответ ученика.

В A0/B0 выполняется ровно один readout каждого EXAM. Кэш тогда выключен **в обоих** плечах, поэтому B не получает бесплатное заполнение. Затем четыре TRAIN updates. В A1/B1 выполняется такой же EXAM. Получение hashes и readouts находится вне измеряемого участка TRAIN. В нём нет restore, создания новой модели между шагами, настройки по EXAM или изменения frozen base. Создание кэша оплачивается первым TRAIN шагом B.

## Что измеряем до/после

| Группа | Реальные данные |
|---|---|
| A0,A1,B0,B1 | Для каждого EXAM: все 256 последних logits, argmax token, exact, NLL правильного следующего байта |
| Фактическое обучение | 4 native loss значения; AdamW step=4; hash trainable before/after; hash optimizer moments after; признак actual parameter write |
| Основная стоимость TRAIN | `GC.GetAllocatedBytesForCurrentThread`, `Stopwatch`, process CPU с Refresh перед вторым чтением; GC0/1/2 |
| Цена памяти | Process peak working set до конца TRAIN отдельно от TRAIN allocations; удерживаемый float payload кэша отдельно от всего процесса |
| Механизм | Cache misses/hits за TRAIN; сохранение frozen state; идентичность конечных trainable/optimizer state и readouts A/B |

Сравнение качества: `ΔA=A1−A0`, `ΔB=B1−B0`, затем `ΔB−ΔA`; для NLL меньше лучше, для exact больше лучше. Стоимость относится к переходу A0→A1 и B0→B1, поэтому сравниваются **два реальных TRAIN участка**, не нулевая стоимость «до обучения».

Ожидается точное равенство native logits/весов/moments A и B при экономии allocations; это гипотеза, не заранее заполненный результат. Для заданной архитектуры исходники предполагают восемь frozen Linears и 32 их вызова за четыре шага; при такой фактической топологии B должен иметь 8 misses и 24 hits. Эта арифметика — ожидаемый механизм, а не измеренная метрика. Если расходится, сначала в отчёте указать actual counts; не добавлять повторный TRAIN.

**Причинное сравнение эквивалентной оптимизации:** одинаковые input/config/initial state, ровно 4 фактических updates, finite loss/logits, frozen unchanged **и** одинаковые конечные trainable/optimizer hashes, 4 loss и before/after logits. У сохранённого `compare.py:47–50` final equality не входит в `valid`; его `VALID_BOUNDED_LEARNING_COMPARISON` сам по себе недостаточен. Для сохранённого A/B эти поля прочитаны и совпали; код compare не менялся. Если условие нарушено, ресурсы остаются описательными наблюдениями, но не доказательством цены той же учебной траектории. Любое полезное подтверждённое улучшение по допустимой оси считается локальным успехом, без процента или обязательного exact gain; все регрессии рядом. Равенство 0→0 само по себе не положительный health-контроль.

## Назначенное место и сохранённый рецепт Windows — уже выполнен

В выполненном опыте использован короткий каталог **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LFT01`** внутри основного workspace. Готовые файлы помощника были скопированы туда по исчерпывающему списку ниже; исходный продуктовый код, старые опыты и результаты не меняются. Это небольшой набор файлов, не новый репозиторий или worktree. Исполнитель не выбирает другой host, модель, путь или SDK.

Результаты и русский `report.md`: **`C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01`**. Ограничения Wiki сюда не относятся. Если опыт уже начат в другом месте, не копировать его ради нового запуска; сохранить имеющиеся данные и выполнить текущее поручение Любы.

Назначенный helper — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LFT01/Run.ps1`; данные, исходники и команды уже выбраны. При readback core 6dcd42cbbb файл уже существовал: 29 строк, копирование только отсутствующих файлов, run.py и compare.py с одним results-root и отказом без retry. Parent этой документной сессии его не создавал и не выполнял. Сохранённая команда исполненного рецепта — `powershell -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LFT01/Run.ps1`; не перезаписывать его ради оформления. Блок ниже — происхождение уже выполненной подготовки. Теперь успех этого A/B подтверждён raw, а не наличием script. Повтор не назначен. Первое Copy-Item столкнулось с длинным source path; сохранённые local-decisions.md и Prepare-ShortPath.ps1/prepare-short-path.log показывают временную подстановку Q: и копирование 35 + 8 = 43 файла. Эта историческая техническая попытка закончилась до Train; воспроизводить её ради отчёта не нужно.

```powershell
$ErrorActionPreference = 'Stop'
$taskSource = 'C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold'
$taskPackage = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LFT01'
$taskOut = 'C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01'
$taskArtifact = 'experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01'
$taskManifest = Get-Content -LiteralPath (Join-Path $taskSource "$taskArtifact/source-copy-manifest.json") -Raw | ConvertFrom-Json
$taskFiles = @(
    'run.py'
    'compare.py'
    'experiment.json'
    '35.ExperimentsV2/Experiments/Rp01Micro/NEURAL-FROZEN-TRANSPOSE-01/Runner.csproj'
    "$taskArtifact/Program.cs"
    "$taskArtifact/source-copy-manifest.json"
    "$taskArtifact/one-factor.diff"
)
$taskFiles += @($taskManifest.files | ForEach-Object { "$taskArtifact/src/$($_.path)" })
foreach ($taskRelative in $taskFiles) {
    $taskTarget = Join-Path $taskPackage $taskRelative
    if (-not (Test-Path -LiteralPath $taskTarget)) {
        New-Item -ItemType Directory -Path (Split-Path -Parent $taskTarget) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $taskSource $taskRelative) -Destination $taskTarget
    }
}
$env:PATH = 'C:/Program Files/dotnet;' + $env:PATH
& 'C:/Python314/python.exe' (Join-Path $taskPackage 'run.py') --results-root $taskOut
if ($LASTEXITCODE -ne 0) { throw "EXPERIMENT_INCOMPLETE: $LASTEXITCODE; preserve logs; no retry" }
& 'C:/Python314/python.exe' (Join-Path $taskPackage 'compare.py') --results-root $taskOut
if ($LASTEXITCODE -ne 0) { throw "COMPARISON_FAILED: $LASTEXITCODE; do not rerun training" }
```

В назначенном manifest ровно 36 native source files; с семью служебными файлами копируется 43 файла. Host и относительная структура сохраняются; product solution и дополнительные зависимости не подключаются. `run.py` проверяет готовые копии автоматически — не повторять это вручную и не вызывать prepare.py. Если result/bin/obj уже существуют, не удалять их и не придумывать команду resume: её нет. Отметить фактический сохранённый этап; повтор A/B эта карточка не разрешает.

**Бюджет выполненного A/B (не новое разрешение):** два процесса A/B, 4 SFT updates на плечо (всего 8), 2 EXAM до и 2 после на плечо (всего 8 forwards). Один build ≤120с, весь run ≤120с, один процесс ≤60с. Кэш B впервые наполняется внутри измеряемого TRAIN, а не бесплатным прогревом.

Raw: `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/A/code-sign.json`, соответствующий B и общие execution/comparison. Readouts/counters фиксируются отдельно от стоимости TRAIN.

## Решения конфликтов и промпт

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/NEURAL-FROZEN-TRANSPOSE-01.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

**Приоритет источников.** Текущее прямое поручение и эта актуализированная canonical карточка определяют фазу, host и results-root; README/EXECUTOR-PROMPT помощника сохраняются как происхождение scaffold. Их planning-only/protected запрет и `python3` не переопределяют назначенные Windows-пути. Source pin `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39` сохраняется: более новый Markdown HEAD не требует нового согласования или замены исходников. `C:/Python314/python.exe` теперь подтверждён чтением наличия файла и согласован с сохранёнными успешными receipts; интерпретатор в документной сверке не запускался.

**Минимальный host — явное исключение для экспериментальной копии.** Допускаются один Program, локальные helper-функции, `throw`/`try-catch`/`void`, простые DTO и JSON-выход; не требуется заменять их на product OperationResult, добавлять DI/слои/комментарий к каждой строке. Это не отменяет native проверки конечности, формы входов, отмену и границы один фактор/одинаковый ученик. Product/tests/CI не меняются. Ручные checksums, новый baseline, дополнительный equivalence-run и Git-аудит ради оформления не нужны; уже сохранённые автоматические receipts читаются, но не пересоздаются.

- Поручение на запуск уже определённого опыта не требует нового согласования модели/версии. Отсутствующий обязательный файл — конкретная техническая ошибка этого рецепта; исполнитель не выбирает другой проект или имитацию. Смена Markdown HEAD не меняет сохранённые source/input hashes.
- Здесь build/result уже содержат завершённые A/B. Нет оставшегося TRAIN или команды resume; повтор не назначен. Не удалять bin/obj и не переименовывать опыт. Сбой подготовки не записывать как нулевую точность.
- Никаких дополнительных warmup, equivalence, B→A, других datasets или reviewer rerun. Самостоятельная диагностика не расширяет бюджет. Проверяющий читает diff/raw; самопроверка не является независимой проверкой.
- Никаких AGENTS/skills/product edits, работы с worktree, истории коммитов, push, CI, общего dispatcher. Регистрация кандидата не блокирует два плеча; при обычном завершении она обязательна по правилу ниже. Публикация не входит в опыт.

> Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Полная карточка: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/NEURAL-FROZEN-TRANSPOSE-01.md. Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. **Промпт исполнителю:** Сохранённый NEURAL-FROZEN-TRANSPOSE-01 уже выполнен. В текущей документной фазе читай его raw и уточняй только порученные документы; не вызывай нижние команды и не повторяй A/B. Следующий опыт — только по текущему поручению и единому индексу. При отдельном поручении иной фазы следуй её точной границе. Используй модель своей сессии по умолчанию, один worker, указанный готовый scaffold, точные данные и команды этой карточки. Не выбирай другой host и не меняй native learner вне единственного фактора. Сохрани фактические A0/A1/B0/B1, counts, resources и все выигрыши/ухудшения; любое полезное измеренное улучшение — успех по соответствующей оси/домену, accuracy не обязательна. После выполнения заполни report.md на русском по каркасу ниже из уже записанных raw. Никаких повторов ради отчёта. Подготовленный исходник не является доказательством успешной сборки. Основной код не изменяй: точный кандидат переноса уже указан в карточке, решение остаётся у Любы.

## Готовый формат отчёта и навыки — без поиска образца

**Когда оформление закончено.** Достаточно одного русского report.md по этому каркасу: до 350 слов пояснений плюс таблица фактических метрик/ответов и ссылки на raw. При обычном завершении полезный кандидат обязательно регистрируется одной записью до 120 слов в [required-changes.md](../../required-changes.md), со ссылкой на отчёт и ухудшениями; существующую запись уточнить без дубликата. Это не gate до обучения. Немедленное указание перехода/остановки выше отменяет дописывание регистрации и отчёта. Один раз прочитать записанное, исправить только фактическую ошибку, сломанную ссылку или кодировку. Не делать циклы стилистического улучшения, Mermaid/SVG/render, исследования Git или новую проверку алгоритма. Не вычислять и не сверять hash ради отчёта. Уже существующие технические receipts оставить как есть; после правки текста достаточно фразы «уточнён отчёт, данные и опыт не менялись». Не удалять старые raw или receipts ради упрощения отчёта. Состояние «независимый review не выполнен» записать честно, оно не требует искать другую модель или удерживать оформление открытым. После этих файлов задача оформления завершена; следующая работа — только уже назначенный следующий опыт, без нового поиска формата.

**Язык — русский. Формат — обычный Markdown.** Итоговый файл — `report.md` в точном каталоге результатов выбранного подслучая, указанном в этой карточке; рядом остаются исходные JSON/логи. Не искать другие отчёты, новый реестр или шаблон. Перед каждым новым аудитом/заданием свежо и полностью прочитать governance и весь основной/ролевой стек договора120 из входного блока этой карточки; старые пять навыков не являются полным перечнем. Внутри того же задания отчёт/readback/wait не требуют повторять уже выполненное полное чтение. Исторические команды навыков не расширяют текущую фазу, модель, данные или бюджет.

Скопировать следующий каркас и заполнить только фактическими данными. Пустые измерения писать «не измерено», сбой — «этап не завершён», а не 0.

```markdown
# <ID>: <что изменилось в обучении, простыми словами>
Домен: <предметная тема>. Исполнитель: <фактическая модель и режим>.
Проверяющий: <фактическая модель или «не выполнялась»>.
execution_status: <NOT_STARTED / PARTIAL / COMPLETED / FAILED / OUTCOME_UNKNOWN>.
learning_validity: <VALID_FOR_NAMED_UNIT / INVALID_NO_TRAINING / INVALID_ZERO_TO_ZERO / INVALID_UNQUALIFIED_BEFORE / INVALID_INCOMPLETE_COMPARISON / NOT_EVALUATED / UNKNOWN>; единица и raw before/after: <точно>.
hypothesis_status: <OPEN / SUPPORTED_FOR_NAMED_METRIC / NOT_SUPPORTED_IN_VALID_COMPARISON>; invalid всегда оставляет OPEN.
Источники: <паспорт, raw, команды>; завершённые updates отдельно от ошибки сохранения, неизвестные counts=null, невыполненное плечо — не измерено.

## Что меняли
<Один фактор, исходный файл/метод, куда урок записывает навык и кто его читает.
A — исходный код, B — конкретная правка только экспериментальной копии.>
## На чём проверяли
<Точные TRAIN/EXAM или ссылка на сохранённые данные; начальное состояние; реальные вызовы/предъявления.
Запланированный и фактический бюджет отдельно; лишние операции перечислить.>
## Что получилось
| Показатель, единица и область | A до | A после | B до | B после | Изменение и смысл |
|---|---:|---:|---:|---:|---|
| <точность/ошибки/состояние> | | | | | |
<Стоимость реального перехода обучения: A = …, B = …, B−A = ….
Время, CPU, allocated bytes, retained heap, RSS/peak — отдельные строки.
Слои модели, уровни Unit/Flow и усилия подготовки — значения или не измерено.
Для короткой панели привести каждый фактический ответ и gold.>
## Выигрыши и ухудшения
<Любое полезное измеренное улучшение — успех по этой оси и домену;
рост accuracy не обязателен. Все ухудшения рядом. Ожидание не результат.>
## Граница вывода и кандидат изменения
<Что измерение доказывает и чего не измеряло. Не объяснять шум/JIT без контроля.
Точный исходный метод и предлагаемый diff для решения Любы; основной код
автоматически не меняется. Никаких новых запусков ради составления отчёта.>
```

**Хорошо:** «За тот же учебный переход выделено A=12 МБ, B=9 МБ; ответы одинаковы. Выигрыш по выделениям — 3 МБ. Пик процесса вырос на 1 МБ. Один запуск; причина разницы wall-time не установлена». Числа здесь — только образец формы, не результат.
**Плохо:** «Все тесты зелёные, обучение исправлено»; «accuracy не выросла, поэтому экономия памяти не считается»; «память стала 9 МБ» без указания, что измеряли.

**Термины и предел редактирования.** Сначала простое русское название, затем точный ID/API в скобках; статус словами, машинный код — рядом в backticks. Не строить Mermaid, симуляцию ветвей или повторный расчёт алгоритма ради отчёта. Использовать уже записанные ответы и counters. «Не предложил лишний цикл» не означает «весь совет правильный»; «расходы не измерены/шумны» не означает «без дополнительных затрат». Проверка push, обход истории и перезапуск модели не являются частью написания отчёта.


