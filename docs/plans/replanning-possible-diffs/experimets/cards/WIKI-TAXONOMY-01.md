# WIKI-TAXONOMY-01 — новые маршруты домена превращаются в отрицательный учебный класс

## Уточнение учебного утверждения и проверки — 3 ноября 2026

Отрицательный учебный класс и разные предметные темы — не одно и то же. Проверить состав назначенных четырёх уроков и прежний положительный контроль. Не лечить потерю маршрута изменением gold/списка допустимых входов. Рост внутри группы и регресс отрицательного контроля показать раздельно; недостающую обучаемость проверять отдельным опытом.

**Передаваемое исполнителю правило:** применить [договор45](../45-learning-claim-and-native-acceptance-contract-2026-11-03.md) вместе с основным стеком навыков и experiment-design/test-integrity/test-failure-triage/bug-to-tests. В разрешённой реализации собрать затронутый продукт и host/test project, выполнить назначенные проверки именно собранного состояния, дождаться исходного handle. При failure эскалировать архитектору команду/версию/DLL/log/input/expected/actual; не заменять свежую сборку старым бинарником, не менять gold или продуктовую семантику ради неверного теста. UNRUN не закрывает реализацию. Документный аудит не запускает эти команды. STOP/NEXT пользователя немедленны; записанное исполнение не повторять.

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница ROUTE/STATE:** На названном маршрутизаторе/каталоге проверить writer→обученная память→ordinary route/payload и собственные positive/negative строки; label без выбранного payload не закрывает маршрут. Exact recall, простой baseline и предметная тема раздельно. Дополнительный dynamic same-functions фактор допустим только если назначен паспортом и проверяет скорость native Train. Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Исходный адрес из паспорта:** `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/results/report.md>).
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

## Действующий статус: сохранённая внешняя попытка, повтор запрещён

**CONSUMED_EXTERNAL_RESULT_NO_RERUN,21.10.2026.** Внешние commits до20b30f781d добавили результаты в назначенный WT01. Этот документный проход только прочитал raw; runtime не запускал. A:3 updates,0/6→2/6 correct; B:7 updates,0/6→4/6. Оба native состояния изменены. ВB остаются1wrong и1defer; OOD перевод ухудшился. Это локальная6-строчная панель, не готовность product router. [Readback](../../reconciliation/14308-external-wiki-readback-2026-10-21.json), [A raw](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/results/A/report.json), [B raw](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/results/B/report.json).

Все команды, бюджеты и embedded промпты прежнего рецепта ниже теперь **историческое описание consumed attempt**, не следующий запуск. Нельзя повторять их, пересоздавать host/results или выбирать новый suffix. Первое невыданное поручение остаётся PA01→исправленный08; текущая сессия редактирует документы. Данные и внешние source deviations сохраняются; эта пометка не утверждает независимую проверку binary/единственности запуска/устойчивости эффекта.

## Состав данных и предел сопоставления, 21.10

Домен обучаемой единицы — **оркестрация (D07)**, задача — выбор маршрута project wiki. TRAIN содержит4 уникальные строки с4 ожидаемыми классами по1строке; EXAM —6строк с распределением2/2/1/1. В A две новые метки отсутствуют в пространстве выходов; именно их представимость — фактор этой карточки. Метка NotProjectWiki — отрицательный класс, а не автоматически безопасный отказ. Близкие английские парафразы и проверенные4класса не подтверждают полный router или перенос на новые семейства запросов. Остальные классы не считаются обученными только потому, что перечислены в Labels.

Наличие отрицательного урока не доказывает, что только им объясняется разница с WIKI-MARGIN-01: там другие TRAIN/EXAM и другой кодовый фактор. Нельзя использовать эту пару разных опытов как A/B состава. Для каждого опыта отдельно показывать покрытие, постоянный ответ, ошибочные выборы и отказы по группам по [контракту данных](../32-training-set-and-inference-contract.md). Исторические raw и рецепт не меняются, новый запуск не назначается.

## Исторический рецепт и его ограничения

**Единственное назначение результата WIKI-TAXONOMY-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/results; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [WIKI-TAXONOMY-01](WIKI-TAXONOMY-01.md) → [WIKI-MARGIN-01](WIKI-MARGIN-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Эта вставка не добавляет training/evaluation calls к указанному ниже бюджету.

**Учебный сигнал и предел вывода.** На назначенных TRAIN/EXAM показать правильность native маршрута и TargetUnitId до/после Train по классам и относительно предусмотренных простых контролей. Правильная категория, одинаковая для всех входов, не доказывает различение; статическое изменение labels не подменяет обучение. Общий product router этими локальными результатами не проверяется.

**Разрешение конфликтов из последнего аудита (20.10).** Старый запрет копий в `81-validation-program.md` не применяется: используется только назначенная A/B-копия; product/tests/CI не меняются. Ограничение «protected не менять» в отчёте помощника относилось к его поручению, а не отменяет прямое поручение Любы исправить карточки. `NOT_RUN` означает отсутствие результата данной версии, а не запрет; поздняя карточка не меняет протокол уже сохранённого опыта и не назначает повтор. Любое полезное измеренное улучшение — успех по метрике/домену, без порога5% и требования роста accuracy; несопоставимые затраты не приписываются фактору. Связанные подготовка, restore и чтение выученного состояния допустимы с точным source→state→consumer; посторонний scorer-пример не назначает опыт. Вместо отсутствующей84-ссылки — [реестр источников](../01-full-source-registry.md) и [карта покрытия](../2026-10-19-small-learning-probes/06-findings-map.md); следующая работа задаётся [индексом](00-execution-index.md). Назначенные данные, домен, host и команды нельзя заменять самостоятельным поиском другого опыта.

**Порядок программы:** [WIKI-TAXONOMY-01](WIKI-TAXONOMY-01.md) → [WIKI-MARGIN-01](WIKI-MARGIN-01.md). Для общей карточки использовать только якорь назначенного ID. Позиции — [текущий индекс](00-execution-index.md); условия перехода — [правило NEXT](00-execution-index.md#next-contract). Сохранённые попытки не повторять; одиночное поручение — END.

## Немедленное завершение по указанию Любы

**Обязательно для каждого подслучая и каждого промпта исполнителю этой карточки.** Прямое текущее указание Любы имеет приоритет над нижними требованиями завершить отчёт, review или весь цикл.

- **«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу.
- Если следующее задание уже указано, не выбирать другое и не обходить каталог в поисках более готового. Если сказано «следующий» или назван порядковый номер — взять его из единого индекса `cards/00-execution-index.md`; не искать N-KU-02 по аналогии с N-KU-01 и не задавать вопрос о выборе. После конца назначенной очереди кратко сообщить об этом, не возобновляя прошлый опыт. Поручение редактировать карточки или описывать опыт само по себе не разрешает запуск обучения.
- **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Команду перехода не подменять остановкой всей программы; команду полной остановки не подменять следующим запуском.
- Незавершённые файлы и уже записанные данные оставить как есть. Их наличие не разрешает продолжать вопреки указанию. Не удалять raw/receipts и не устраивать закрывающую проверку; краткий статус — только по уже известным фактам, без новых инструментов.
- Исполнитель опыта не берёт на себя исправление карточек: это отдельное поручение документной сессии. Остановка одного исполнителя не отменяет отдельное поручение другой сессии, если Люба не остановила и его.

**Исторический статус при написании рецепта.** Тогда сборка и обучение этой документной сессией не выполнялись; теперь выше учтён внешний результат и повтор запрещён. Рецепт helper назначал ограниченный запуск; его прежнее «SDK_UNAVAILABLE» относится к его среде, не к этой Windows-машине. Исполнитель и отдельный последовательный reviewer используют модель текущей сессии по умолчанию; фактическое имя пишут в отчёте. Один worker, без вложенных. Вопросов о поиске Astra или выборе другого проекта нет.

**Исходное поручение:** «рассмотри последние обновления помощников и выстрой эксперименты, как можно более простые и маленькие для каждого домейна»; «модель не должна сама искать куда и как ей поместить эксперимент, как его запустить, как решить конфликт». Изменение — только в предоставленной экспериментальной копии. Продуктовый перенос решает Люба по результату.

Источник: [карточка помощника](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-taxonomy/README.md>). Его source pin — `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`; сравнение здесь относится к закреплённым копиям, не к произвольному более новому HEAD. Не выдавать source-derived ожидание за новый native результат.

## Где деталь реально участвует в обучении

Native цепочка: `ProjectWikiRequestRoutingTrainingRunner.TrainAndEvaluateAsync` → `ProjectWikiRequestRoutingTrainableModel.Train(trainingRows)` → `Featurize(row.Prompt)` → `LabelIndex(ExpectedRouteKind)` → запись `_tokenWeights[token][class]`, `_biases[class]` → `RouteForEvaluation` → `Score`/argmax → `Decision`. [Native writer и reader L43–53](<C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs:43> "Исторический ref 62bcc8f2368840e22d2ea98ef16d9bd526ad9a39, L43-L53; локальный файл может отличаться"). Это проверка реально изменяемой модели и native учебного потребителя, не oracle таблица.

Состояние: каждый `Train` создаёт новую модель с нуля; writer меняет коэффициенты весов и bias. `ToSnapshot` сохраняет `Labels`, веса и bias. Reader читает их при выборе цели; host наблюдает его решения до/после на одинаковом EXAM. Cold-load и продолжение существующего обучения в этом первом опыте не участвуют. Текущий `ProjectWikiTrainingOrchestrator` вызывает статический `_router.Route(prompt)` L205, поэтому локальный опыт **не доказывает**, что learned snapshot уже является общим product router. `TargetUnitId` проверяется как native выход, но target training не запускается.

## Единственный фактор A/B

A: точная текущая логика15 выходов. B: согласованный output contract17 выходов: добавить две метки **после** всех старых15, явно возвращать индекс `NotProjectWiki` для неизвестного enum и добавить две decoder-ветви с точными target IDs/required inputs из L61–65 static router. Это один фактор — пространство допустимых классов на стороне writer и reader; одна замена Labels без decoder оставила бы неполный механизм.

Старые индексы0–14 не меняются. B новые координаты15/16 нулевые. Initial-equivalence: одинаковы первые15 names/bias coordinates, оба словаря весов пусты; B имеет ещё две нулевые координаты. Полный hash начального состояния A/B различается законно, потому что разная размерность; comparator проверяет общую часть, а не требует невозможного равенства хэшей. `Enum.GetValues` вместо Labels запрещён: enum ordinal не совпадает с прежним порядком модели.

18эпох, learning-rate0.14, нормировка признаков, token/bigram extractor, порядок TRAIN, условие обновления, inference margin0.02 и gates остаются одинаковыми. Margin-исправление соседней карточки сюда не переносить. Старые snapshot positional load остаются отдельной осью: [FromSnapshot L86–93](<C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:86> "Исторический ref 62bcc8f2368840e22d2ea98ef16d9bd526ad9a39, L86-L93; локальный файл может отличаться") и [NormalizeVector L384–389](<C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:384> "Исторический ref 62bcc8f2368840e22d2ea98ef16d9bd526ad9a39, L384-L389; локальный файл может отличаться"). Этот fresh-state опыт не доказывает cold migration.

## Фиксированные TRAIN и EXAM

| Split / ID | Prompt — точная строка | ExpectedRouteKind | Группа |
|---|---|---|---|
| TRAIN / T1 | `Train system boundary mapper for project evidence.` | `SystemBoundaryMapperTraining` | `system_boundary` |
| TRAIN / T2 | `Learn cross-stack business flow graph from client and server evidence.` | `CrossStackBusinessFlowGraphTraining` | `cross_stack` |
| TRAIN / T3 | `Train code trace builder from method call evidence.` | `CodeTraceTraining` | `legacy_code_trace` |
| TRAIN / T4 | `Translate this short greeting into Russian.` | `NotProjectWiki` | `ood_translate` |
| EXAM / Q1 | `Improve system boundary mapper using project evidence.` | `SystemBoundaryMapperTraining` | `system_boundary` |
| EXAM / Q2 | `Train system boundary mapper on API project evidence.` | `SystemBoundaryMapperTraining` | `system_boundary` |
| EXAM / Q3 | `Improve cross-stack business flow graph using client and server evidence.` | `CrossStackBusinessFlowGraphTraining` | `cross_stack` |
| EXAM / Q4 | `Learn cross-stack business flow graph for repository evidence.` | `CrossStackBusinessFlowGraphTraining` | `cross_stack` |
| EXAM / Q5 | `Improve code trace builder from method call evidence.` | `CodeTraceTraining` | `legacy_code_trace` |
| EXAM / Q6 | `Translate a polite greeting into English.` | `NotProjectWiki` | `ood_translate` |


Для новых меток требуется `Selected` + правильные route/target; `legacy_code_trace` — контроль старого положительного класса, `ood_translate` — контроль отрицательного класса. Точные expected target IDs находятся в `fixture.json` и проверяются рядом с RouteKind. На таком малом срезе языковое обобщение ограничено английскими парафразами команд. Строение AST, русская морфология, перевод и собственно построение cross-stack графа здесь не обучаются.

Если B не улучшает useful metric, но исправляет представимость пространства меток, это механизм с пока не подтверждённой пользой на этом EXAM, а не успешное качество. Если B улучшает новые маршруты и портит старый/OOD, сохранить обе стороны. Исход обучения неизвестен до native запуска.

## Куда переносить только после результатов и решения пользователя

Первая целевая правка — `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs`: `Labels`, `LabelIndex`, decoder switch. До реальной интеграции отдельно проверить именованное сопоставление snapshot labels при загрузке; простое изменение вектора/перестановка enum недопустимы. Второй последующий scope — расширить generator TRAIN/EXAM seeds новыми маршрутами, чтобы обычная оценка замечала покрытие, и проверить действующий consumer подключаемой модели. Ни generator, ни loader, ни production orchestration не являются вторым фактором текущего опыта.

## Одно место и один порядок запуска Windows

Для ещё не начатого назначенного опыта используется короткий каталог **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01`** внутри основного workspace. Готовые файлы помощника копируются туда один раз по исчерпывающему списку ниже; исходный продуктовый код, старые опыты и результаты не меняются. Это небольшой набор файлов, не новый репозиторий или worktree. Исполнитель не выбирает другой host, модель, путь или SDK.

Результаты и русский `report.md`: **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/results`**. Этот адрес находится вне `docs/plans/replanning-possible-diffs`, поэтому существующий guard Wiki не изменяется. Ограничение конкретного output-prefix не запрещает весь core-workspace. Если опыт уже начат в другом месте, не копировать его ради нового запуска; сохранить имеющиеся данные и выполнить текущее поручение Любы.

Сохранить следующий текст как `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/Run.ps1`. Данные, исходники и команды уже выбраны карточкой. Затем выполнить только `powershell -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/Run.ps1`. В документной сессии этот script не создавался и не выполнялся.

```powershell
$ErrorActionPreference = 'Stop'
$taskSource = 'C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-taxonomy/scaffold'
$taskPackage = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01'
$taskOut = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WT01/results'
$taskSourceRel = '35.ExperimentsV2/Experiments/Rp01Micro/WIKI-TAXONOMY-01'
$taskHostRel = 'experiments-v2/RP01-MICRO/WIKI-TAXONOMY-01'
$taskHost = Join-Path $taskPackage $taskHostRel
$taskSources = Join-Path $taskPackage $taskSourceRel
$taskFiles = @(
    "$taskSourceRel/NativeRoutingContracts.cs"
    "$taskSourceRel/BaselineModel.cs"
    "$taskSourceRel/CandidateModel.cs"
    "$taskSourceRel/Program.cs"
    "$taskSourceRel/fixture.json"
    "$taskSourceRel/source-provenance.json"
    "$taskSourceRel/checksums.sha256"
    "$taskSourceRel/compare.py"
    "$taskHostRel/WikiTaxonomyLearning.csproj"
    "$taskHostRel/NuGet.Config"
)
foreach ($taskRelative in $taskFiles) {
    $taskTarget = Join-Path $taskPackage $taskRelative
    if (-not (Test-Path -LiteralPath $taskTarget)) {
        New-Item -ItemType Directory -Path (Split-Path -Parent $taskTarget) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $taskSource $taskRelative) -Destination $taskTarget
    }
}
if (Test-Path -LiteralPath $taskOut) { throw 'EXISTING_RESULT: preserve; no automatic rerun' }
foreach ($taskBuild in @("$taskHost/bin", "$taskHost/obj")) {
    if (Test-Path -LiteralPath $taskBuild) { throw "EXISTING_BUILD: preserve $taskBuild; do not rebuild" }
}
# Однократная автоматическая проверка копий; вручную её не повторять.
foreach ($line in Get-Content -LiteralPath "$taskSources/checksums.sha256") {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    if ($line -notmatch '^([0-9a-fA-F]{64})\s+\*?(.+)$') { throw 'INVALID_CHECKSUM_LINE' }
    $expected = $Matches[1]; $relative = $Matches[2]
    if ((Get-FileHash -LiteralPath (Join-Path $taskPackage $relative) -Algorithm SHA256).Hash -ine $expected) {
        throw "SOURCE_MISMATCH: $relative"
    }
}
[IO.Directory]::CreateDirectory($taskOut) | Out-Null
& 'C:/Program Files/dotnet/dotnet.exe' restore "$taskHost/WikiTaxonomyLearning.csproj" --configfile "$taskHost/NuGet.Config" *> "$taskOut/restore.log"
if ($LASTEXITCODE -ne 0) { throw "RESTORE_FAILED: $LASTEXITCODE; no retry" }
& 'C:/Program Files/dotnet/dotnet.exe' build "$taskHost/WikiTaxonomyLearning.csproj" -c Release --no-restore *> "$taskOut/build.log"
if ($LASTEXITCODE -ne 0) { throw "BUILD_FAILED: $LASTEXITCODE; no retry" }
foreach ($arm in @('A','B')) {
    & 'C:/Program Files/dotnet/dotnet.exe' "$taskHost/bin/Release/net9.0/WikiTaxonomyLearning.dll" --arm $arm --output "$taskOut/$arm" *> "$taskOut/$arm-process.log"
    if ($LASTEXITCODE -ne 0) { throw "ARM_FAILED: $arm / $LASTEXITCODE; no retry" }
}
& 'C:/Python314/python.exe' "$taskSources/compare.py" "$taskOut/A/report.json" "$taskOut/B/report.json" "$taskOut/comparison.json" *> "$taskOut/compare.log"
if ($LASTEXITCODE -ne 0) { throw "COMPARE_FAILED: $LASTEXITCODE; do not rerun arms" }
```

Host компилирует четыре файла `NativeRoutingContracts.cs`, `BaselineModel.cs`, `CandidateModel.cs`, `Program.cs`; BCL net9, без PackageReference/ProjectReference. Родительские Directory.Build отключены. Библиотеки, контракты, teacher или product dispatcher не добавлять. При частичном сбое оставить лог и отметить незавершённый этап; не заменять его нулевой точностью. Ручная перепроверка fingerprints, переустановка SDK, очистка bin/obj и Git не входят в рецепт.

**Фиксированный бюджет A+B:** 4 TRAIN и 6 EXAM; две непустые native `Train`, две пустые `Train` для native начального состояния, всего четыре вызова API `Train`; 144 предъявлений TRAIN строк (`18` native эпох на arm); 24 вызовов native evaluation reader. Эпохи — повторные предъявления внутри одного обучения, не отдельные эксперименты. Ручная генерация фикстуры не вызывает teacher/LLM. Полное обучение target unit, neural DPO, publication, cold restore и orchestration не запускаются.

### Что сохранять и как считать

Каждый arm пишет `before-snapshot.json`, `after-snapshot.json`, точную `fixture.json`, `source-provenance.json`, `report.json`. Фикстура SHA-256: `b1c03bee21330c79c0c8dc8b04f7f7250e313f0ce119f36830feda76d26e0850`. Before получается через native `Train([])`: private constructor не заменяется. After получается через native `Train(TRAIN)` из такого же zero initialization. EXAM никогда не передаётся в train, native reader получает только `Prompt`. Точные TRAIN/EXAM строки различаются; это небольшие лексически близкие парафразы, а не доказательство далёкого обобщения.

Native snapshot включает служебные gate-поля с нулевым `TrainingRowCount`, потому что gate не прикрепляется; это не счётчик реальных предъявлений. `observedTrainingRows`, `rowPresentations` и `modelUpdateCount` измеряет host, а служебное native значение сохраняется отдельно с пояснением. Ни `GatePassed`, ни `PublishAllowed`, ни `SemanticQualityClaimed` не выставляются. Проверка модели штатным `RouteForEvaluation` — тот же reader, который вызывается внутри `TrainingRunner` L43–53; обычный `Route` с production gate не подменяется и не используется для заявления публикации.

Основной полезный показатель — доля **Selected с верными `RouteKind` и `TargetUnitId`**. Отдельно выводятся wrong-selected, defer, строки/группы доменов и все before→after изменения. Сравнение A/B — `(B_after−B_before)−(A_after−A_before)`. Совпадение одного `RouteKind` при `NeedsSecondPass` остаётся диагностикой: native defer возвращает `NotProjectWiki`, поэтому такое совпадение само по себе не является успешным выбором. `Confidence`/top-two margin — диагностика; существующий confidence clamp не чинится этим опытом.

Ресурсы: wall-time, process CPU, allocations текущего потока вокруг непустого `Train`, heap before/after, число параметров, размер snapshot. Comparator пишет отдельный `resourceDeltas` с A/B, B−A и направлением полезного изменения, без общего PASS; `deferredRate` помечен conditional diagnostic и не получает автоматический useful-success. `PeakWorkingSet` честно подписан как пик **всего процесса**, не как пик фазы обучения. Один очень короткий процесс на arm имеет JIT/таймерный шум; результат является наблюдением этой пары, а не доказательством устойчивого ускорения. Счётчик обновлений и изменение хэша сами по себе не полезный результат: нужен эффект на выходах или полезном ресурсе. Без последующей санкционированной карточки автоматические дополнительные повторы запрещены.

**Правило успеха:** любое измеренное улучшение полезного показателя считается успехом **по этому показателю и этому домену**; рост точности не обязателен. Рядом обязательно показать изменения всех остальных показателей, включая ухудшения. Снижение defer считается полезным вместе с качеством выбранных решений; замена безопасных отказов неверными решениями не объявляется улучшением качества. Возможен одновременно успех на одной группе и ухудшение на другой; это не скрывать сводным pass/fail.

## Решения конфликтов и промпт

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-TAXONOMY-01.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

- Поручение на запуск уже определённого опыта не требует нового согласования модели/версии. Отсутствующий обязательный файл — конкретная техническая ошибка этого рецепта; исполнитель не выбирает другой проект или имитацию. Смена Markdown HEAD не меняет сохранённые source/input hashes.
- Существующий build/result означает сохранённый этап: читать его stage/exit code и продолжать только ещё не исполненное в порученном запуске. Не удалять bin/obj, не переименовывать опыт, не повторять A/B. Сбой не записывать как нулевую точность.
- Никаких дополнительных warmup, equivalence, B→A, других datasets или reviewer rerun. Самостоятельная диагностика не расширяет бюджет. Проверяющий читает diff/raw; самопроверка не является независимой проверкой.
- Никаких AGENTS/skills/product edits, работы с worktree, истории коммитов, push, CI, общего dispatcher. Регистрация результата и публикация не нужны для продолжения заданных двух плеч.

> Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Полная карточка: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-TAXONOMY-01.md. Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. **Промпт исполнителю:** Выполни только назначенную тебе фазу WIKI-TAXONOMY-01. Используй модель своей сессии по умолчанию, один worker, указанный готовый scaffold, точные данные и команды этой карточки. Не выбирай другой host и не меняй native learner вне единственного фактора. Сохрани фактические A0/A1/B0/B1, counts, resources и все выигрыши/ухудшения; любое полезное измеренное улучшение — успех по соответствующей оси/домену, accuracy не обязательна. После выполнения заполни report.md на русском по каркасу ниже из уже записанных raw. Никаких повторов ради отчёта. Подготовленный исходник не является доказательством успешной сборки. Основной код не изменяй: точный кандидат переноса уже указан в карточке, решение остаётся у Любы.

## Готовый формат отчёта и навыки — без поиска образца

**Когда оформление закончено.** Достаточно одного русского report.md по этому каркасу: до 350 слов пояснений плюс таблица фактических метрик/ответов и ссылки на raw. При обычном завершении полезного измеренного опыта обязательно добавить или уточнить одну запись до 120 слов в ../../required-changes.md со ссылкой на отчёт; отдельное поручение не требуется. Не выполнять оформление после команды немедленно перейти или остановиться. Один раз прочитать записанное, исправить только фактическую ошибку, сломанную ссылку или кодировку. Не делать циклы стилистического улучшения, Mermaid/SVG/render, исследования Git или новую проверку алгоритма. Не вычислять и не сверять hash ради отчёта. Уже существующие технические receipts оставить как есть; после правки текста достаточно фразы «уточнён отчёт, данные и опыт не менялись». Не удалять старые raw или receipts ради упрощения отчёта. Состояние «независимый review не выполнен» записать честно, оно не требует искать другую модель или удерживать оформление открытым. После этих файлов задача оформления завершена; следующая работа — только уже назначенный следующий опыт, без нового поиска формата.

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


