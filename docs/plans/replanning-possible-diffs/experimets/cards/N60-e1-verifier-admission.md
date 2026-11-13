# N60-E1 — какой отрицательный урок допускается к обучению Verifier

<a id="current-executor-task"></a>
## Текущее задание исполнителю — 3 ноября 2026

**Что проверяется: Отрицательный урок проверяющего.** До отдельного прямого назначения исполнения — анализ сохранённых данных и документации. Уже записанный PRIMARY или alias не повторять; текущее состояние смотреть по ID в индексе. [Индекс опытов](00-execution-index.md) определяет очередь; эта поправка не добавляет новую работу. Исторические промпты ниже не являются альтернативным запускным заданием.

**Хороший пример:** Исправляющий отрицательный урок проходит назначенный admission и реально достигает обучения; честный правильный ответ остаётся принят.

**Плохой пример и ожидаемая реакция:** Ранний отказ не допустил урок до learner, но назван улучшением Verifier; false accepts уменьшились ценой скрытых false rejects. Показать обе таблицы ошибок и фактическую границу допуска.

**Передаваемый текст задания:**

> Работай по полной карточке `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/N60-e1-verifier-admission.md`, её паспорту данных и этому текущему разделу. Перед заданием заново полностью прочитай local/canonical AGENTS, CLAUDE и стек навыков из [договора120, §1](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md#1-свежее-полное-чтение-перед-новым-аудитом-или-заданием); для приёмки также test-integrity, test-failure-triage и bug-to-tests. Примени [договор44, §10–12](../codex-44-post117-saved-evidence-and-finalizer-contract-2026-11-03.md#10-один-текущий-вход-вместо-нескольких-противоречащих-заданий): независимое ожидание → фактический рабочий путь → общий итог. Используй собственные хорошие/плохие примеры выше, не расширяй панель чужими fixtures. При падении сохрани input/expected/actual и версию; отдели регрессию, неверное ожидание, измеритель и инфраструктуру. Не меняй gold/допуск и не скрывай missing/unknown ради PASS. Показатели до/после, точность и регрессии укажи раздельно на русском. Документное поручение не разрешает код, сборки, тесты, Train/Execute. Будущая разрешённая проверка использует только назначенные в паспорте host/команду/бюджет; отсутствующий режим ремонта верни автору, исторический PRIMARY его не заменяет. При разрешённом запуске дождись исходного handle по договору31 без relaunch. Прямой STOP/NEXT исполни немедленно; не продолжай прежнюю работу ради оформления. Любой измеренный полезный эффект сохраняется по своему показателю и домену, с ценой рядом; допуск переноса оценивается отдельно.

Основание усиления: [аудит138](../../fixes-based-on-experiments/codex-138-post117-transfer-and-experiment-audit-2026-11-03.md) и [разбор контекста исполнителя](../../fixes-based-on-experiments/evidence/2026-11-03-post117-executor-context-review.md). Для родственных карточек это профилактика того же класса ошибки, а не утверждение, что их результаты заново проверены. Исходные результаты, наборы, бюджеты и команды сохранены ниже.


<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница TRANSLATION:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold.

**Исходный адрес из паспорта:** `27.TranslationDomain/TranslationStructuredBatchBuilder.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/report.md>).
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

## Сохранённая внешняя попытка: старый entry больше не выдавать

**CONSUMED_EXTERNAL_OUTPUT_NO_RERUN,21.10.2026.** При заключительной сверке найдены A/B raw в C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results. Эта документная сессия их не создавала. По counters реально6/5 Unit lessons; панель2/4→3/4 в обоих плечах, B уменьшает ложный отказ и добавляет ложный приём. `heldOut:false` передан в BuildGeneralBatch учебных pairs; это флаг учебного batch, а не доказательство включения EXAM в Train. Отдельная проверочная панель содержит новые рекомбинации тех же2marker-семейств и не является family-independent. Внешний pair-summary сообщает ошибку штатного summary и отдельное построение сводки; это не отменяет сохранённых arm updates.

**Контроль состава,21.10.** У EXAM4строки:2ожидаемых Pass и2Fail. Постоянный Pass или Fail даёт2/4. Общие3/4 у A и B скрывают обмен ошибок: ложный отказ1→0, ложный приём0→1; это не общее улучшение проверяющего. Считать отдельно обе ошибки и обе marker-family, не объявлять удаление неправильного урока приобретением полного отрицательного правила. Исторический TRAIN и raw не исправляются задним числом. Прежний вывод об EXAM только из поля heldOut был необоснован; принадлежность split определяется фактическим путём данных, а не названием одного флага.

[Ограниченный readback трёх JSON](../../reconciliation/14308-intake-2026-10-21.json). Полный source/binary/runtime аудит здесь не заявлен. Все commands и embedded prompts ниже — исторический consumed рецепт; не повторять запуск, не удалять raw, не выбирать другой suffix. Этот статус приоритетен над прежними «host не запущен/будущий entry». Первое невыданное поручение в общей очереди остаётся PA01; текущая работа только документы.

## Исторический рецепт

**Единственное назначение результата N60-E1:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [N60-E1](N60-e1-verifier-admission.md) → [LC-REF-CACHE](N61-dpo-reference-reuse.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Эта вставка не добавляет training/evaluation calls к указанному ниже бюджету.

**Учебный сигнал и предел вывода.** На назначенных Q1–Q4 сравнить фактический Verify до/после V1→V2: полезные ответы по спокойно/calmly и громко/loudly, ложные вердикты и отказы. Admission counts или экспортированные строки без правильного Verify не доказывают полезное обучение. Translate/Flow не добавлять: они вне этого бюджета.

**Разрешение конфликтов из последнего аудита (20.10).** Старый запрет копий в `81-validation-program.md` не применяется: используется только назначенная A/B-копия; product/tests/CI не меняются. Ограничение «protected не менять» в отчёте помощника относилось к его поручению, а не отменяет прямое поручение Любы исправить карточки. `NOT_RUN` означает отсутствие результата данной версии, а не запрет; поздняя карточка не меняет протокол уже сохранённого опыта и не назначает повтор. Любое полезное измеренное улучшение — успех по метрике/домену, без порога5% и требования роста accuracy; несопоставимые затраты не приписываются фактору. Связанные подготовка, restore и чтение выученного состояния допустимы с точным source→state→consumer; посторонний scorer-пример не назначает опыт. Вместо отсутствующей84-ссылки — [реестр источников](../01-full-source-registry.md) и [карта покрытия](../2026-10-19-small-learning-probes/06-findings-map.md); следующая работа задаётся [индексом](00-execution-index.md). Назначенные данные, домен, host и команды нельзя заменять самостоятельным поиском другого опыта.

**Порядок программы:** [N60-E1](N60-e1-verifier-admission.md) → [LC-REF-CACHE](N61-dpo-reference-reuse.md). Для общей карточки использовать только якорь назначенного ID. Позиции — [текущий индекс](00-execution-index.md); условия перехода — [правило NEXT](00-execution-index.md#next-contract). Сохранённые попытки не повторять; одиночное поручение — END.

## Немедленное завершение по указанию Любы

**Обязательно для каждого подслучая и каждого промпта исполнителю этой карточки.** Прямое текущее указание Любы имеет приоритет над нижними требованиями завершить отчёт, review или весь цикл.

- **«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу.
- Если следующее задание уже указано, не выбирать другое и не обходить каталог в поисках более готового. Если сказано «следующий» или назван порядковый номер — взять его из единого индекса `cards/00-execution-index.md`; не искать N-KU-02 по аналогии с N-KU-01 и не задавать вопрос о выборе. После конца назначенной очереди кратко сообщить об этом, не возобновляя прошлый опыт. Поручение редактировать карточки или описывать опыт само по себе не разрешает запуск обучения.
- **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Команду перехода не подменять остановкой всей программы; команду полной остановки не подменять следующим запуском.
- Незавершённые файлы и уже записанные данные оставить как есть. Их наличие не разрешает продолжать вопреки указанию. Не удалять raw/receipts и не устраивать закрывающую проверку; краткий статус — только по уже известным фактам, без новых инструментов.
- Исполнитель опыта не берёт на себя исправление карточек: это отдельное поручение документной сессии. Остановка одного исполнителя не отменяет отдельное поручение другой сессии, если Люба не остановила и его.

**Меняем только допуск совпавшего отрицательного ответа; учатся настоящие Units перевода; проверяем собственные решения QualityVerifierUnit.** Выбран один вариант: «спокойно/calmly» и «громко/loudly», извлечённый native `TrainStructuredBatchAsync`, **2 batch-вызова, внутри 11 Unit.TrainOnExample, из них 3 Verifier; 16 Verify**. Это обучение дискретной памяти, не DPO/LoRA и не генерация полного перевода.

Сейчас изменяется только карточка. Host ниже назначен полностью, но этим документным проходом не создан, не собран и не запущен. Новое личное подтверждение каждого ID/SHA не требуется: исполнитель действует в пределах прямого поручения на ограниченную программу. Документный запрос сам по себе её не запускает.

## Исходное поручение и правила исполнения

> «Ок, рассмотри пожалуйста последние обновления помошников и выстрои из них экперименты, как можно более простые и маленькие для каждого домейна. Задача ускорить процесс проверки и сузить обучающийся механизм до очень простых маленьких примеров, что бы проверить улучшаются показатели довольно быстро. Отчетность должна быть человеческим языком но точно показывающая какую часть механизма обучения меняем и как будем тестировать.»

> «Разве эксперименты не были построены по формуле: Что  было - пробег по маленькому сету, изменение в качестве эксперимента, измеренеи как стало стем же сетом?»

**Успех:** любое фактически измеренное полезное улучшение — успех по этому показателю и домену, без обязательного роста accuracy. Все другие измерения и ухудшения показываются рядом. Здесь A и B начинают с одинаковых пустых обучаемых таблиц, получают одни и те же две исходные пары и четыре проверочных входа; единственное изменение может закономерно сократить число допущенных внутренних уроков. Нулевой эффект реально достигнутого обучения допустим.

**Модель исполнителя и последовательного reviewer — текущая модель соответствующей сессии по умолчанию**, без принудительной Astra/ultra или поиска замены. Записать фактическое имя. Student — отдельно: `SemanticTokenizerUnit`, шесть `ExecutorUnit` с общим `TranslationKnnStore`, `QualityVerifierUnit`. Нейронной модели, B0, весов, LoRA, optimizer и RNG здесь нет. Самопроверка не называется независимым worker review.

| Применяемый навык: точный путь | Правило этого задания |
|---|---|
| [architect-behavior-classifier-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md) | Разделить урок, writer, обучаемую память и настоящий reader; не выдавать Verify за Translate |
| [planning-session-startup-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md) | Сохранить фазу и назначенные пути; подготовка/исполнение требуют соответствующего прямого поручения |
| [plan-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md) | Один фактор, одинаковые исходные пары и EXAM, честный вложенный счёт |
| [generated-code-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md) | Проверить точность извлечения и единственный treatment diff; не начинать другой опыт |
| [output-readability-gate-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md) | Русский отчёт по шаблону ниже; actual отдельно от ожидания |
| [human-communication-GLOSSARY.md](C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md) | «Перевод» — предметная тема; CPU, elapsed, allocations, heap и RSS — разные показатели |

При конфликте старых инструкций применяется прямое разрешение Любы на точную экспериментальную копию. Original source — read-only и будущий адрес переноса, не edit target. Малый host может использовать throw/try-catch/void/обычные DTO; он не обязан проходить S00–S04, product OperationResult/DNA/comment-rule/full CI. **Исходный OperationResult метода сохраняется**, поскольку это часть копируемого метода, а не требование переделать всю оболочку. Product tests, общий 27/35 build, SDK/install, commit/push и перенос в продукт не входят в этот опыт. Решение о переносе принимает Люба после результата.

Параллельная сессия читает рассуждения и меняет только карточки; она не меняет inputs/state/code и не запускает опыт. Исполнитель работает по один раз прочитанному паспорту и назначенным inputs. Ручные SHA/hash для оформления паспорта, отчёта или редакционной записи не требуются; автоматические проверки готового runner вручную не дублируются. Движение документного HEAD не требует бесконечного reread AGENTS/карточки или перезапуска. Старые raw/receipts, результаты, исходники, bin/obj и markers сохраняются. Самовольные B→A/order-check, warmup, повторы и reviewer rerun запрещены; observed разницу нельзя объявлять JIT/noise без измерения.

## Причинная связь и одно изменение

Путь:

`BuildGeneralBatch → quality-verifier TranslationLearningExample → TrainStructuredBatchAsync → QualityVerifierUnit.TrainOnExample → _requiredTermsBySourceMarker → QualityVerifierUnit.Verify`.

Original/proposed target: [TranslationStructuredBatchBuilder.cs:128–141](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationStructuredBatchBuilder.cs:128). В A остаётся исходный `examples.Add(...:verifier...)`. В B **только этот Add** обёрнут:

```csharp
if (!string.Equals(rejectedCandidate, pair.Chosen, StringComparison.Ordinal))
{
    // Полный исходный examples.Add(BuildExample(...:verifier...)) без иных правок.
}
```

Причину исключения `NO_DISTINCT_NEGATIVE` фиксирует host при сравнении projected IDs с исходными двумя парами; не вводить другое правило выбора, другую нормализацию или дополнительное обучение. Остальные пять B-строк должны совпадать с соответствующими A-строками, включая их JSON/сигнатуры/SourceId.

[BuildRejectedCandidate:269–288](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationStructuredBatchBuilder.cs:269) для `[WRONG:tense]` без explicit negative возвращает chosen, но исходный builder всё равно создаёт fail-урок. [Writer:1095–1144](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:1095) извлекает требование из issue и source marker; [reader:1052–1091](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:1052) затем требует один из записанных target terms.

Новые входы выбраны потому, что существующий tokenizer распознаёт ADV по окончаниям «но»/«ко». Tokenizer, N57 mojibake-helper, английский regex, concept/signature rules, остальные Unit writers/readers одинаковы в A/B. Исправлять их одновременно нельзя.

## Точные замороженные данные

Материал — существующие simulation-файлы из:

`C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections/`.

| Файл | SHA256 прочитанных bytes |
|---|---|
| [N60-E1-TRAIN.jsonl](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections/N60-E1-TRAIN.jsonl>) | `565cbd862c1ccc015dbdd986891bd71436295df0f92cfb0a7c19747225125b5d` |
| [N60-E1-EXAM.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections/N60-E1-EXAM.json>) | `85f9dda66ff27c4c0bdba95fe82c2d39618a5cf714d63950123ae90fec97f2bb` |
| [N60-E1-source-manifest.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections/N60-E1-source-manifest.json>) | `382e95a26da6a9723b04a84f34e378b1198c9d9027ab35e558ccfb117d07247a` |

При подготовке скопировать bytes без пересериализации в назначенные fixtures ниже. Не сочинять автора модели: поля `source=simulation:n60-e1:V1/V2` сохраняются.

| TRAIN | a_side = prompt | chosen | rejected |
|---|---|---|---|
| V1 | `Он спокойно открыл дверь.` | `He calmly opened the door.` | `[WRONG:tense]` |
| V2 | `Она громко закрыла окно.` | `She loudly closed the window.` | `She closed the window.` |

Обе записи: `direction="RU_EN"`, `pair_family="manner-admission"`, `validation_outcome="train_ready"`, `validation_errors=[]`. Порядок V1→V2. BatchId у обоих плеч одинаковый: `N60-E1-VERIFIER-ADMISSION-01`. `heldOut=false`, `signatureMode=Structural` указать **явно**: native default равен ExactPair и здесь не подходит.

| EXAM | Source | CandidateText | Заранее заданный gold |
|---|---|---|---|
| Q1 | `Он спокойно закрыл окно.` | `He closed the window in a composed manner.` | Pass: смысл «спокойно» сохранён парафразой |
| Q2 | `Он спокойно закрыл окно.` | `He closed the window.` | Fail: способ действия пропущен |
| Q3 | `Она громко открыла дверь.` | `She loudly opened the door.` | Pass |
| Q4 | `Она громко открыла дверь.` | `She opened the door.` | Fail: способ действия пропущен |

Это четыре наблюдения двух marker-семейств на новых рекомбинациях, не четыре новых закона. CandidateText — фиксированный вход именно проверяющему, не якобы полученный ответ Executor. `gold_pass` читает только reporting слой после native Verify; он не попадает в builder/Unit/reader.

## Назначенный host: 11 исходных файлов, без product ProjectReference

Фиксированные пути:

- Source-root: `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N60E1VerifierAdmission/`.
- Host-root: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/`.
- **Результат:** `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/`, отдельные A/ и B/.

Helper-путь результатов в possibleDifs **не применяется**. Его фраза о «protected docs» не отменяет прямое поручение записать карточку и результаты в эти адреса.

Manifest pin — `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`. При текущем чтении все 11 файлов совпали с manifest по SHA256. В таблице ниже номера строк относятся к этим проверенным bytes. Сохранить оригиналы в `reference/<repo-relative-path>` с исходными headers, manifest в `reference/source-manifest.json`, TRAIN/EXAM в `fixtures/` Source-root.

| Исходник относительно core | Точная компилируемая часть |
|---|---|
| `27.TranslationDomain/TranslationContracts.cs` | Весь файл |
| `27.TranslationDomain/TranslationUnitContracts.cs` | Весь файл, включая настоящий KNN store и ITranslationUnitTrainer |
| `27.TranslationDomain/TranslationSemanticTokenizer.cs` | Весь файл |
| `27.TranslationDomain/TranslationStructuredBatchBuilder.cs` | Весь файл; только в B treatment выше |
| `27.TranslationDomain/TranslationTrainedUnits.cs` | Весь файл, все настоящие Units и internal ExportCheckpoint |
| `27.TranslationDomain/TranslationMemorySourceLaw.cs` | Весь файл, неизменная зависимость KNN store |
| `27.TranslationDomain/TranslationCheckpointContracts.cs` | Весь файл, Unit DTOs; load/save опыт из этого не следует |
| `27.TranslationDomain/TranslationMachine.cs` | Константы 14–16; поля 19–21,27–29; **полный метод 89–146**; helpers 662–684,707–712; конструктор из объектов 716–724 и dictionary 760–770, как ниже |
| `27.TranslationDomain/TranslationRoutingFlow.cs` | Только `public const string FlowId = "translation-routing-flow";` в metadata-классе TranslationRoutingFlow |
| `00.Core/LearningMachineContracts.cs` | Только record LearningBatchResult 48–67, namespace AgentInfrastructure.Logical |
| `00.Core/OperationResult.cs` | Весь файл, namespace AgentInfrastructure |

Семь полных TranslationDomain-файлов и два извлечённых файла дать в `A/` и `B/`. Единственная одинаковая инфраструктурная замена полного source — `namespace TranslationDomain;` на `namespace N60E1.A;` / `namespace N60E1.B;`. В `shared/` два Core-фрагмента с исходными namespace. **Не** переписывать строки DomainTag/FlowId/UnitId и JSON: namespace не должен менять входной материал.

Извлечённый `public sealed class TranslationMachine` не реализует ILearningMachine и не содержит `_flow`, `_runtime`, RunAsync, legacy TrainBatchAsync, checkpoint import/promotion. Его метод TrainStructuredBatchAsync и три helper-метода source-identical. Добавляется одинаковый адаптер конструктора/наблюдения:

1. Создать `knnStore = new TranslationKnnStore()`, `semanticTokenizer = new SemanticTokenizerUnit()`, шесть `new ExecutorUnit(direction, knnStore)` для RU_EN, EN_RU, HE_EN, EN_HE, ES_EN, EN_ES, `verifier = new QualityVerifierUnit()` — ровно native construction 716–724.
2. Словарь `Dictionary<string,ITranslationUnitTrainer>(StringComparer.Ordinal)` содержит эти восемь объектов по их UnitId, как 760–770. Для счётчиков **в этом же извлечённом файле** определить `CountingTrainer : ITranslationUnitTrainer`: `UnitId => inner.UnitId`; TrainOnExample записывает attempted, делегирует **один** настоящий вызов, затем completed; MeasureAccuracy только делегирует. `_unitTrainers` получает такой wrapper для каждого существующего объекта. Поведение wrapper одинаковое A/B; gold/условий допуска в нём нет.
3. Сохранить internal typed properties `SemanticTokenizer`, `Verifier`, `KnnStore` и `IReadOnlyList<ExecutorUnit> Executors` на исходные объекты, не wrappers. `ExportObservedState()` возвращает обычный DTO/anonymous object: native ExportCheckpoint этих объектов, все executor states, `_structuredTrainingIdentities`, `_trainingHistory`, `_currentVersion`, `_accuracyAfterLastBatch`, `_hasStructuredTraining` и counters. Все copies/host в одной сборке, поэтому существующие internal ExportCheckpoint доступны **без reflection или нового friend grant**.
4. В AArm.cs/BArm.cs использовать полные aliases `using Native = N60E1.A;` / `using Native = N60E1.B;`. Control flow одинаковый; общий host DTO не является substitute Unit. Новых SourceLaw/tokenizer/Verifier алгоритмов нет.

**Назначенный состав сборки:** 9 A-файлов + 9 B-файлов + 2 shared-файла + Program.cs/AArm.cs/BArm.cs = 23 explicit Compile items. Никаких внешних Packages/ProjectReference, нейронных assemblies или full 27/35 build. Namespace/type/extraction выбор закрыт; фактическая компиляция ещё не выполнена.

`Runner.csproj` имеет следующий полный состав. Это рецепт, а не материализованный файл:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <AssemblyName>N60E1VerifierAdmission</AssemblyName>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <E1SourceRoot>C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N60E1VerifierAdmission</E1SourceRoot>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="AArm.cs" />
    <Compile Include="BArm.cs" />
    <Compile Include="$(E1SourceRoot)/shared/OperationResult.cs" />
    <Compile Include="$(E1SourceRoot)/shared/LearningBatchResult.cs" />
    <Compile Include="$(E1SourceRoot)/A/TranslationContracts.cs" />
    <Compile Include="$(E1SourceRoot)/A/TranslationUnitContracts.cs" />
    <Compile Include="$(E1SourceRoot)/A/TranslationSemanticTokenizer.cs" />
    <Compile Include="$(E1SourceRoot)/A/TranslationStructuredBatchBuilder.cs" />
    <Compile Include="$(E1SourceRoot)/A/TranslationTrainedUnits.cs" />
    <Compile Include="$(E1SourceRoot)/A/TranslationMemorySourceLaw.cs" />
    <Compile Include="$(E1SourceRoot)/A/TranslationCheckpointContracts.cs" />
    <Compile Include="$(E1SourceRoot)/A/TranslationMachine.cs" />
    <Compile Include="$(E1SourceRoot)/A/TranslationRoutingFlow.cs" />
    <Compile Include="$(E1SourceRoot)/B/TranslationContracts.cs" />
    <Compile Include="$(E1SourceRoot)/B/TranslationUnitContracts.cs" />
    <Compile Include="$(E1SourceRoot)/B/TranslationSemanticTokenizer.cs" />
    <Compile Include="$(E1SourceRoot)/B/TranslationStructuredBatchBuilder.cs" />
    <Compile Include="$(E1SourceRoot)/B/TranslationTrainedUnits.cs" />
    <Compile Include="$(E1SourceRoot)/B/TranslationMemorySourceLaw.cs" />
    <Compile Include="$(E1SourceRoot)/B/TranslationCheckpointContracts.cs" />
    <Compile Include="$(E1SourceRoot)/B/TranslationMachine.cs" />
    <Compile Include="$(E1SourceRoot)/B/TranslationRoutingFlow.cs" />
  </ItemGroup>
</Project>
```

### Конечный Program/arm-контракт

Program принимает ровно четыре аргумента `--arm A|B --output <absolute-arm-directory>`. Output допускается только `<назначенный result-root>/A` или `/B`; другой ID/путь не выбирается. Создаётся один экземпляр только выбранного arm. Input paths внутри host зафиксированы на Source-root/fixtures; данные читаются один раз с указанными SHA.

1. Native `TranslationStructuredBatchBuilder.ParseJsonl(trainText)` один раз; проверить V1/V2 и фиксированные поля, не менять их. Создать `new Native.TranslationMachine()` с пустыми обучаемыми таблицами. Его неизменные bootstrap rules не называются изученными уроками.
2. Сохранить A0/B0 native state. Для Q1→Q4 каждый раз вызвать **того же** `machine.SemanticTokenizer.Tokenize(q.Source,"ru")`, затем `machine.Verifier.Verify(q.Candidate,tokens)`. Сохранить actual tokens/Signature/RuleTags и VerificationResult. Само tokenization не учит; mappings из EXAM не записываются.
3. Один native `BuildGeneralBatch("N60-E1-VERIFIER-ADMISSION-01", pairs, heldOut:false, signatureMode:Native.TranslationSignatureMode.Structural)`. Внутри builder остаётся его собственный `new TranslationSemanticTokenizer()` — не заменять на обученный Unit. Сохранить все projected rows, включая JSON chosen/rejected. A ожидает V1:semantic/executor/verifier и V2:semantic/executor/verifier, B — тот же список без V1:verifier. B-исключение фиксировать host как отсутствующий соответствующий ID, не как новый lesson.
4. Один `await machine.TrainStructuredBatchAsync(batch, CancellationToken.None)`. IsSuccess=false/Value=null — сохранить native ErrorCode/ErrorMessage, failure и ненулевой exit; не выполнять следующий этап как успешный.
5. Сохранить A1/B1 state и повторить те же Q1→Q4/Tokenize/Verify без дополнительного Train. Snapshot после панели может фиксировать отсутствие неожиданных read-mutations; это не новая панель.
6. Записать actual counters, projected rows, два state snapshots, восемь Verify records и ресурсные интервалы. Gold join выполняется только reporting-кодом. B после своей панели читает сохранённые A receipts и создаёт pair-summary/report, без повторного вызова A.

Никакого `MeasureAccuracy`/held-out вызова: heldOut=false. Никакого обычного Flow/Translate, прямого unit-only обучения вместо batch, cold reload или extra tokenizer correction. Внутренние tokenization/JSON/маркерные операции native Train сохраняются, но не считаются дополнительными уроками. Обработанный допустимый урок может дать нулевой state delta — это не invalid.

### Подготовка и команды

Прочитанный helper [N60-E1-prepare-sources.ps1](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections/N60-E1-prepare-sources.ps1>) **не запускать неизменным**: в нём чужой output target и только копирование reference bytes, без создания host. Его bounded алгоритм проверки 11 SHA/копирования bytes использовать при реализации назначенной подготовки с output из этой карточки. Helper source не редактировать и не исполнять его продолжения.

При подготовке использовать существующий source manifest и expected counters. Автоматическая проверка исходных bytes остаётся частью подготовки runner; отдельный ручной реестр hashes карточки/host/отчёта не создавать. Использовать существующий SDK в разрешённой фазе; не устанавливать SDK/пакеты автоматически. Если соответствующий source/host/result уже занят, сохранить существующее; незавершённую известную подготовку продолжать по её frozen паспорту, не перезаписывая raw/results и не создавая другой RunId. Issued arm без completed не повторять.

Будущие фиксированные команды после подготовки, **не доказательство готовой CLI**:

```powershell
$E1Host = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION'
$E1Out = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results'
dotnet build "$E1Host/Runner.csproj" -c Release -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if ($LASTEXITCODE -ne 0) { throw 'E1 build failed; preserve evidence, no runtime.' }
dotnet "$E1Host/bin/Release/net9.0/N60E1VerifierAdmission.dll" --arm A --output "$E1Out/A"
if ($LASTEXITCODE -ne 0) { throw 'A failed; no B or retry.' }
dotnet "$E1Host/bin/Release/net9.0/N60E1VerifierAdmission.dll" --arm B --output "$E1Out/B"
if ($LASTEXITCODE -ne 0) { throw 'B failed; preserve partial evidence.' }
```

Один свежий OS process на arm; A→B без обратного порядка/repetitions. Create-new issued/completed и raw files; исключения сохраняются с фазой и фактическими счётчиками. Старые bin/obj/results не удаляются. Если source bytes отличаются, это конкретная проверка новой версии, а не повод постоянно reread из-за документного HEAD или молча сменить материал.

## Честный бюджет и измерители

| Вызов / работа | A | B | Всего |
|---|---:|---:|---:|
| Исходные пары / предъявления builder | 2 / 2 | 2 / 2 | 2 общие пары / 4 предъявления |
| ParseJsonl / BuildGeneralBatch | 1 / 1 | 1 / 1 | 2 / 2 |
| TrainStructuredBatchAsync | 1 | 1 | 2 |
| Внутренние Unit.TrainOnExample | 6 | 5 | 11 |
| **Подмножество** предыдущей строки: Verifier | 2 | 1 | 3 |
| Verify A0/B0 | 4 | 4 | 8 |
| Verify A1/B1 | 4 | 4 | 8 |
| Все Verify | 8 | 8 | 16 |
| Ordinary Flow/Translate, neural updates, external models | 0 | 0 | 0 |
| Warmup / повтор / reviewer rerun | 0 | 0 | 0 |

Не складывать 11 и 3 как 14 уроков. Различие принятых rows 6→5 — следствие одного admission-фактора; не компенсировать его копиями/повторением. Сравнить общую стоимость обработки тех же двух исходных пар и отдельно расходы самой Train, при actual units 6/5.

По одинаковым границам отдельно: preparation/build-batch; `await TrainStructuredBatchAsync` (без записи receipt внутри измерительного окна); каждая четырёхвходовая Verify-панель; сохранение. Wall — Stopwatch, CPU — delta Process.TotalProcessorTime, allocations — delta GC.GetTotalAllocatedBytes, GC Gen0/1/2, managed live bytes — GC.GetTotalMemory(false), whole-process peak RSS — Process.PeakWorkingSet64. Не называть allocations RAM, heap snapshot пиком или TickCount CPU. Peak включает загрузку процесса и обе панели, его не приписывать только Train. Без forced GC и произвольного таймаута.

Native result сохраняется без переименования полей, но с пояснением: `CpuMs` здесь elapsed TickCount, `PeakRamMb` — целочисленный managed heap snapshot, `AccuracyAfter` — coverage затронутых UnitId (при этих входах ожидается 3/3), `RowsTrained` — projected rows, `PromotedUnitIds` — touched IDs, **не публикация**. Собственная точность и false reject/accept берутся только из actual Verify + заранее заданного gold.

State bytes — размер одинаково сериализованного native learned-state DTO, отдельно от receipts/history/counters. Neural layers/RNG/weights=N/A. Unit/Flow уровни: source pair → structured Unit lessons → реальные learned tables → Verify; Flow 0. Два процессных одиночных замера не доказывают устойчивое ускорение. Наблюдаемый выигрыш всё равно показывается как локальный успех по выбранной метрике, с масштабом/ценой и без автоматических дополнительных прогонов.

## Ожидания и граница отрицательного результата

Ожидание по исходнику, **не результат**: A0/B0 без learned rules принимают все четыре candidates. A1 записывает marker «спокойно» с `calmly` и «громко» с `loudly`: Q1 отвергается вместе с omission Q2; Q3 принимается, Q4 отвергается. B1 не записывает ложный V1:verifier: Q1 принимается, но Q2 тоже; V2 сохраняет Q3/Q4. Возможны одновременно снижение false reject на Q1 и рост false accept на Q2; общий correct count может остаться 3/4 против 3/4. Фактический отчёт обязан показывать все четыре строки и native Issues, а не подставлять ожидание.

Проверить по actual state, что relevant marker найден, terms записаны intended writer и прочитаны Verify. Если замысленный writer/reader не достигнут, заявленная причинная проверка не исполнена; не чинить одновременно tokenizer/N57 и не переходить на «бесшумно/нежно». Если путь достигнут и сравнение не даёт пользы, это честный нулевой/отрицательный результат. Любая полезная ресурсная или предметная разница — успех по этой оси, с показом остальных потерь. Вывод о полном переводе, LoRA или универсальной синонимии не делается.

## Формат report.md — заполнить, не искать другой образец

**Предел оформления:** не более 350 слов русского пояснения плюс таблицы/raw; кандидат переноса — не более 120 слов плюс точная ссылка. Один readback только фактов, ссылок и кодировки. Без Mermaid/render/Git/new run, повторных style/read/hash циклов и пересчёта сохранённых результатов. Ручной hash отчёта/паспорта не вычислять. При редакционной правке достаточно записи «уточнён отчёт, данные и опыт не менялись»; старые raw/receipts сохранить, автоматические проверки runner не дублировать. После этого оформление завершено.

Следующий текст — **пустой шаблон**, не результаты:

```markdown
# N60-E1 — допуск отрицательного урока Verifier

## Что меняли
В B пропускали только V1:verifier, если вычисленный candidate совпал с chosen.
Writer → state → reader:
BuildGeneralBatch → TrainStructuredBatchAsync → QualityVerifierUnit.TrainOnExample
→ _requiredTermsBySourceMarker → QualityVerifierUnit.Verify.
Модель исполнителя: ___. Reviewer: ___ / самостоятельная проверка.
Пути исходника, карточки, inputs, host; команды и ссылки на существующие receipts: ___.

## На чём проверяли
Две фиксированные RU→EN пары; A0/B0 пустые learned tables, одинаковые bootstrap rules.
План: A6/B5 Unit-вызовов внутри двух batch; 16 Verify. Факт: ___.
Это извлечённый batch consumer с настоящими Units; полного Flow/Translate не было.

## Что получилось
| Задание | Gold | A0 | A1 | B0 | B1 | Issues / Score |
|---|---|---|---|---|---|---|
| Q1 парафраза спокойно | Pass | — | — | — | — | — |
| Q2 пропуск спокойно | Fail | — | — | — | — | — |
| Q3 громко сохранено | Pass | — | — | — | — | — |
| Q4 пропуск громко | Fail | — | — | — | — | — |

| Показатель | A до → после | B до → после | Разница B−A |
|---|---|---|---|
| False reject / False accept / correct | — | — | — |
| Marker/terms и native state bytes | — | — | — |
| Train wall / CPU / allocated bytes | — | — | — |
| Build-batch wall / CPU / allocations | — | — | — |
| Verify panels wall / CPU / allocations | — | — | — |
| GC / managed heap / whole-process peak RSS | — | — | — |
| Attempted/completed Unit calls; verifier subset | — | — | — |
| Model layers / Unit/Flow уровни | N/A / — | N/A / — | — |

## Выигрыши и ухудшения
Успех по ___: фактически измерено ___; accuracy growth не требуется.
Одновременно ухудшилось ___; не измерялось ___.
Ожидание по исходнику было ___; actual оказалось ___.
Native AccuracyAfter — Unit coverage; CpuMs — elapsed; PeakRamMb — heap snapshot.

## Граница вывода и кандидат переноса
Один A/B, без прогрева/повторов/перестановки порядка.
Observed разницу сохраняем; устойчивый speedup или общий перевод не доказаны.
Предлагаемый product target: BuildGeneralBatch verifier Add128–141.
Оригинал не изменялся; перенос решает Люба.
```

GOOD, только иллюстрация формы: «Verifier стал принимать допустимую парафразу, но стал пропускать omission; Train дешевле на ___ при 5 вместо 6 внутренних уроков. Это успех по ___ с показанной потерей». BAD: «Отбросили строку, значит перевод и LoRA восстановлены». Незамкнутый запуск/сбой не записывать как нулевую accuracy или экономию успешного обучения.

<a id="полный-промпт-исполнителю"></a>
## Исторический текст задания и свидетельства

Действующий передаваемый текст находится в [начале карточки](#current-executor-task). Нижний текст сохранён как история; его императивы не назначают новый запуск или продолжение уже закрытого этапа.

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/N60-e1-verifier-admission.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

> Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Полная карточка: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/N60-e1-verifier-admission.md. Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. Подготовь и в порученной стадии выполни только N60-E1 из этой карточки. Используй модель текущей сессии по умолчанию; последовательный reviewer также использует свой default, фактические имена запиши. Сохрани frozen паспорт/inputs/source bytes один раз. Подготовь ровно 11-source extracted batch host в указанных 35/RP01-MICRO путях, 23 Compile items, без PackageReference/ProjectReference/full product build. Реальные TrainStructuredBatchAsync, tokenizer, Units и ExportCheckpoint сохраняются; constructor/counters/report — прозрачный одинаковый адаптер.
>
> A исходный; B только admission candidate==chosen вокруг verifier Add. Набор только спокойно/calmly и громко/loudly, V1→V2, Structural/heldOut=false, четыре Q1–Q4. Два batch вызова, внутри 11 Unit calls, из них 3 verifier; 16 Verify A0/A1/B0/B1, Flow/Translate/optimizer/teacher/repeat/warmup 0. Gold не входит в обучение. Нельзя заменить путь direct-Verifier вариантом, чинить N57/tokenizer или дополнять mappings из экзамена.
>
> Результат направь только в назначенный E/results/2026-10-20/N60-E1-VERIFIER-ADMISSION-01, raw не перезаписывай. Сохрани native state/rows/decisions и счётчики; сравни время/CPU/allocations/heap/whole-process RSS с правильными единицами. Заполни русский report.md выше, включая false reject, false accept и все ухудшения. Любое фактически измеренное полезное улучшение — успех по показателю и RU→EN без обязательного роста accuracy. Нулевой достигнутый эффект допустим; недостижение writer не подменяй выдуманным результатом.
>
> Простые throw/try-catch/void/DTO разрешены; ошибки сохраняй с ненулевым exit. Не правь оригинал, skills/AGENTS/steering, product tests/CI, не устанавливай SDK, не commit/push. Не запускай B→A, reviewer rerun или повтор из-за документной параллельной сессии. Русское пояснение report ограничь 350 словами плюс таблицы/raw, кандидат переноса — 120 словами плюс ссылка. Сделай один readback фактов/ссылок/кодировки, без Mermaid/render и повторных style/read/hash циклов. Не вычисляй ручные hashes паспорта/отчёта, не дублируй автоматические проверки runner; при правке текста достаточно «уточнён отчёт, данные и опыт не менялись», старые raw/receipts сохраняются. После результата передай Любе кандидат переноса. Текущий запрос на запись этой карточки не выполняет будущую программу.

## Источники, исторический вариант и предел проверки

- [02-N60-E1-fixture-correction.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections/02-N60-E1-fixture-correction.md>) и [N60-E1-native-host-preparation.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections/N60-E1-native-host-preparation.md>): уточнённый материал и извлечение. Result target/запрет записи protected helper не перенесены.
- Source manifest/prepare-sources.ps1/TRAIN/EXAM прочитаны как текст/данные, скрипт не запускался. 11 source SHA совпали; это целостность, не 11 runtime-проверок.
- Первично прочитаны Machine 14–29/89–146/662–684/707–724/760–770; Builder 12–50/53–145/269–288; Unit contracts 129–178 и KNN export; настоящий Verifier writer/read/export/marker и tokenizer ADV-ветка; Core DTO/OperationResult. Выбранные namespaces/type boundaries проверены чтением. Фактическое создание host, compile/Tokenize/Train/Verify не выполнялось.
- [Историческая первоначальная E1-карточка с быстро/тихо](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md:213> "Исторический ref 62bcc8f2368840e22d2ea98ef16d9bd526ad9a39, L213-L243; локальный файл может отличаться") сохранена как источник старого дизайна, **не результат запуска**. Там не достигалась нужная marker-запись; этот паспорт заменяет её выбранный материал и host-контракт. Родственный LF-04 — атрибуция той же проблемы, не второй обязательный опыт.
- 20.10.2026 current core read: `67abfabe6def8a002984be80b999824601ad273c`; byte match с указанным source pin установлен отдельно. Осталась реализация назначенного host и её реальная сборочная проверка в разрешённой фазе; неизвестный результат compile/запуска не маскируется обещанием готовой CLI. Новых архитектурных выборов/входов для Любы здесь не оставлено.
