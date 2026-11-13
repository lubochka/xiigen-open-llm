# KN-ROUTER-LEARNED-KEY-01 — одинаковое написание выученных ключей при выборе Unit

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): свежое полное чтение governance и 12 навыков, дополнительные experiment-design/test-integrity по роли; текущая default-модель без override. Этот блок не разрешает запуск и не меняет TRAIN/EXAM, бюджет, H/R или очередь.

**Адресная граница ROUTE/STATE:** проверить названный здесь writer learned key → состояние → настоящий routing consumer; верный label без выбранного payload не закрывает маршрут. Положительный свидетель — прежний правильный learned route после записи/чтения; отрицательный — потеря ключа/чужой payload при том же label. Не называть exact recall обобщением. Ресурс создания view/inference не подменяет native Train.

**При падении:** до любого restore сохранить input/expected/actual/stack и source/assembly SHA; отделить fixture/контракт/регрессию/инфраструктуру. Сохранить independent gold, negative control и old-fails/new-passes; не лечить Skip/threshold. Недостающее именованное API/filter/данные вернуть автору карточки. Команда и расход только из паспорта; ждать terminal того же handle, STOP/NEXT немедленны.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01/results/report.md>).
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

**Единственное назначение результата KN-ROUTER-LEARNED-KEY-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01/results; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [KN-ROUTER-LEARNED-KEY-01](KN-ROUTER-LEARNED-KEY-01.md) → [POS-QUERY-SET-ONCE-01](POS-QUERY-SET-ONCE-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Эта вставка не добавляет training/evaluation calls к указанному ниже бюджету.

**Учебный сигнал и предел вывода.** Показать уже назначенные Route ответы Q1–Q4 до/после двух настоящих обучений: освоенные code/documentation ключи и корректный отрицательный случай. Исправление consumer считается использованием выученного состояния; не приписывать ему обновление neural weights или новое обучение.

**Разрешение конфликтов из последнего аудита (20.10).** Старый запрет копий в `81-validation-program.md` не применяется: используется только назначенная A/B-копия; product/tests/CI не меняются. Ограничение «protected не менять» в отчёте помощника относилось к его поручению, а не отменяет прямое поручение Любы исправить карточки. `NOT_RUN` означает отсутствие результата данной версии, а не запрет; поздняя карточка не меняет протокол уже сохранённого опыта и не назначает повтор. Любое полезное измеренное улучшение — успех по метрике/домену, без порога5% и требования роста accuracy; несопоставимые затраты не приписываются фактору. Связанные подготовка, restore и чтение выученного состояния допустимы с точным source→state→consumer; посторонний scorer-пример не назначает опыт. Вместо отсутствующей84-ссылки — [реестр источников](../01-full-source-registry.md) и [карта покрытия](../2026-10-19-small-learning-probes/06-findings-map.md); следующая работа задаётся [индексом](00-execution-index.md). Назначенные данные, домен, host и команды нельзя заменять самостоятельным поиском другого опыта.

**Оформление:** один русский report.md, до350 слов пояснений плюс таблицы и ссылки на raw; запись кандидата, если поручена, до120 слов. Один readback фактов, ссылок и кодировки. Без новых hashes, Git, Mermaid, поиска reviewer-модели или повторного обучения ради отчёта. При прямой команде перехода старый отчёт не дописывать.

**Порядок программы:** [KN-ROUTER-LEARNED-KEY-01](KN-ROUTER-LEARNED-KEY-01.md) → [POS-QUERY-SET-ONCE-01](POS-QUERY-SET-ONCE-01.md). Для общей карточки использовать только якорь назначенного ID. Позиции — [текущий индекс](00-execution-index.md); условия перехода — [правило NEXT](00-execution-index.md#next-contract). Сохранённые попытки не повторять; одиночное поручение — END.

## Немедленное завершение по указанию Любы

**Обязательно для каждого подслучая и каждого промпта исполнителю этой карточки.** Прямое текущее указание Любы имеет приоритет над нижними требованиями завершить отчёт, review или весь цикл.

- **«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу.
- Если следующее задание уже указано, не выбирать другое и не обходить каталог в поисках более готового. Если сказано «следующий» или назван порядковый номер — взять его из единого индекса `cards/00-execution-index.md`; не искать N-KU-02 по аналогии с N-KU-01 и не задавать вопрос о выборе. После конца назначенной очереди кратко сообщить об этом, не возобновляя прошлый опыт. Поручение редактировать карточки или описывать опыт само по себе не разрешает запуск обучения.
- **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Команду перехода не подменять остановкой всей программы; команду полной остановки не подменять следующим запуском.
- Незавершённые файлы и уже записанные данные оставить как есть. Их наличие не разрешает продолжать вопреки указанию. Не удалять raw/receipts и не устраивать закрывающую проверку; краткий статус — только по уже известным фактам, без новых инструментов.
- Исполнитель опыта не берёт на себя исправление карточек: это отдельное поручение документной сессии. Остановка одного исполнителя не отменяет отдельное поручение другой сессии, если Люба не остановила и его.


## Задание, модель и граница

Малый опыт использует два настоящих урока `RoutingProfileTrainer.Train` и читает полученные profiles через `TrainableRoutingFlow.RouteAsync`. Учебная тема — Оркестрация, D07; C#/.NET и документация — контексты двух маршрутов `code`/`docs`, а не два предметных навыка этого learner. Изменяется только нормализация запроса читателем. Это проверка использования результата обучения; весов нейросети здесь нет. Модель исполнителя и последовательного reviewer — модель их сессии по умолчанию; максимум один активный worker. Эта карточка — документация, сборка и обучение при её написании не выполнялись.

Исходный запрос: учесть все присланные детали и задать точные данные, место, механизм, команды и измерения. Источник известной находки: [четыре inherited factors](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/coverage/03-inherited-uncovered-factors.md>), строка `KN-ROUTER-LEARNED-KEY-CANONICALIZATION`. Это отдельный механизм, не новый авторский claim и не переименование KU-02.

## Одна причина и конкретный код

- Writer: [RoutingProfileTrainer.cs](C:/Projects/llm_mvp_core/26.TrainableRouter/Training/RoutingProfileTrainer.cs), `Train → ExtractKeywords`: `C# → csharp`, `.NET → dotnet`, затем lowercase. Два урока создают два `UnitScoringProfile`.
- Reader: [BaselineKeywordCandidateGenerator.cs](C:/Projects/llm_mvp_core/26.TrainableRouter/Training/BaselineKeywordCandidateGenerator.cs), `ScoreAsync`: сейчас только lowercase; часть выученных ключей не находится.
- Обычный потребитель: [TrainableRoutingFlow.cs](C:/Projects/llm_mvp_core/26.TrainableRouter/Training/TrainableRoutingFlow.cs), `RouteAsync → ScoreAsync → TryChooseRoutableCandidate`.
- B добавляет перед lowercase **ровно те же две Replace**, в том же порядке. Пороги, substring matching, профили и данные не меняются. A — нынешний reader.

## Точные данные и четыре состояния

Registry — два явно заданных Unit: `code` и `docs`; role `executor`, техническое поле domain соответственно `programming`/`documentation`, allowed input `text`, confidence floor `0.20`, base score `0`, metadata ref `KN-ROUTER-LEARNED-KEY-01`. Эти значения registry сохраняются; они обозначают контексты маршрутов внутри учебной темы D07. Использовать native default thresholds: confidence `0.40`, score gap `0.05`.

TRAIN, по одному предъявлению:
1. Prompt `C# .NET` → chosen `code`, rejected `docs`.
2. Prompt `manual guide` → chosen `docs`, rejected `code`.

TaskSlice пустой; RequiredDirection и DomainHint — null у TRAIN и EXAM. Не подставлять domain hint: его boost скрыл бы проверяемую потерю.

| EXAM | Prompt | Gold |
|---|---|---|
| Q1 | C# .NET | code |
| Q2 | .NET C# | code |
| Q3 | csharp dotnet | code |
| Q4 | manual guide | docs |

**Роли строк и контроль состава.** TRAIN содержит1code+1docs. В панели Q1 точно совпадает с первым уроком, Q4 — со вторым; это2recall-строки из4, а не независимый held-out. Q2 переставляет те же слова, Q3 даёт их каноническое написание; ни одна строка не представляет новое смысловое семейство. Gold панели3code:1docs; постоянный `code` даёт3/4 (TRAIN tie1:1 разрешается заранее в пользу первого code в registry), постоянный `docs` —1/4. Считать тот же полный route/status, а не только слово. Показать отдельно Q1/Q2 восстановления ключа и Q3/Q4 контроль сохранности. Успех этого опыта — исправление чтения уже записанного ключа; его нельзя назвать новым обучением или общим умением выбирать маршруты. Новые строки/перестановки и вызовы не добавляются. [Контракт данных](../32-training-set-and-inference-contract.md).

В каждом отдельном процессе: A0/B0 — reader над пустым словарём профилей; затем один Train на двух уроках; A1/B1 — тот же EXAM над результатом Train. Не вызывать Train с пустым списком: native API это отклоняет. Начальное состояние пустое задано явно. Всего **2 Train, 4 предъявления учебных строк, 16 RouteAsync**. `RecordDecisionAsync`, `ExecuteAsync`, DPO и дополнительные повторения не вызываются.

Предположение по коду: A0/B0 — knowledge edge на четырёх запросах; A1 — 2/4, B1 — 4/4 по указанной панели. Это ожидание, не измерение. Q3 проверяет уже каноническое написание, Q4 — сохранность другого маршрута `docs` той же темы Оркестрация; первые две строки не являются доказательством широкого обобщения.

## Назначенная экспериментальная копия

Корень: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01`.
Плечи: `A/Runner.csproj`, `B/Runner.csproj`, в каждом `Program.cs` и `src/`.
Результат: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01/results`; raw `A.json`, `B.json`, русский `report.md`.

В каждое плечо копируются **ровно девять** исходных файлов:
```text
00.Core/OperationResult.cs
00.Core/RoutingAuthorityVocabulary.cs
26.TrainableRouter/Contracts/IUnitScorer.cs
26.TrainableRouter/Contracts/IRoutingDecisionLog.cs
26.TrainableRouter/Models/RoutingModels.cs
26.TrainableRouter/Training/RoutingTrainingModels.cs
26.TrainableRouter/Training/RoutingProfileTrainer.cs
26.TrainableRouter/Training/BaselineKeywordCandidateGenerator.cs
26.TrainableRouter/Training/TrainableRoutingFlow.cs
```

Одинаковые технические изменения в копиях A/B для замкнутого host:
1. В `RoutingProfileTrainer.cs` удалить строки using ContinuousLearning/System.Globalization и участок от `internal OperationResult<CodeDocumentationProfileMetadataV2[]> DescribeMetadata` до **не включая** `private static IEnumerable<string> ExtractKeywords`. Это две не вызываемые здесь функции описания метаданных; Train/ExtractKeywords/BuildProfile сохраняются.
2. В конструкторе заменить только fallback `OrchestrationRoutingCurriculum.CreateDefaultUnitMetadataRegistry()` на `throw new ArgumentNullException(nameof(metadataRegistry))`. Program всегда передаёт явный registry; ветвь fallback в опыте не исполняется.
3. В `TrainableRoutingFlow.cs` убрать ` : ITrainableRoutingFlow` и весь метод `ExecuteAsync` от его объявления до **не включая** `public static bool TryChooseRoutableCandidate`. Сохранить RouteAsync, ScoreAsync, constructor, RecordDecisionAsync и все helpers буквально. Это извлечение обычного RouteAsync без неиспользуемого логического envelope, а не симуляция routing.
4. Только в B заменить единственную строку построения `text` в `BaselineKeywordCandidateGenerator.ScoreAsync`:
```csharp
var text = $"{request.OriginalPrompt} {request.TaskSlice} {request.RequiredDirection} {request.DomainHint}"
    .Replace("C#", "csharp", StringComparison.OrdinalIgnoreCase)
    .Replace(".NET", "dotnet", StringComparison.OrdinalIgnoreCase)
    .ToLowerInvariant();
```

Если указанный anchor отсутствует, записать конкретное несовпадение source; не искать другой проект и не менять продукт. Подготовка копий — обязанность будущего исполнителя по этому исчерпывающему рецепту, без выбора метода/фикстуры. Главные типы принадлежат `AgentInfrastructure`, `TrainableRouter` и `TrainableRouter.Training`; чужой `SemanticCompiler.OperationResult` не подключать.

В обоих `Runner.csproj`:
```xml
<Project>
  <PropertyGroup>
    <ImportDirectoryBuildProps>false</ImportDirectoryBuildProps>
    <ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets>
  </PropertyGroup>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk"/>
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>KeyReader</AssemblyName>
  </PropertyGroup>
  <ItemGroup><Compile Include="src/**/*.cs"/><Compile Include="Program.cs"/></ItemGroup>
  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk"/>
</Project>
```

Один и тот же `Program.cs` в обоих плечах:
```csharp
using System.Diagnostics;
using System.Text.Json;
using AgentInfrastructure;
using TrainableRouter;
using TrainableRouter.Contracts;
using TrainableRouter.Training;

if (args.Length != 2) throw new ArgumentException("arm output.json");
var output = Path.GetFullPath(args[1]);
if (File.Exists(output)) throw new IOException("Preserve existing result");
using var armClaim = new FileStream(output + ".training.claim", FileMode.CreateNew, FileAccess.Write, FileShare.None);
var readCosts = new List<object>();
string[] candidates = ["code", "docs"];
var registry = new RoutingUnitMetadataRegistry([
    new("code","executor","programming",["text"],0.20,"KN-ROUTER-LEARNED-KEY-01"),
    new("docs","executor","documentation",["text"],0.20,"KN-ROUTER-LEARNED-KEY-01")
]);
RoutingTrainingExample[] train = [
    new("T1","C# .NET","",null,null,candidates,"code","docs","code lesson","wrong unit"),
    new("T2","manual guide","",null,null,candidates,"docs","code","docs lesson","wrong unit")
];
(string Text,string Gold)[] exam = [
    ("C# .NET","code"),(".NET C#","code"),
    ("csharp dotnet","code"),("manual guide","docs")
];
async Task<object[]> Read(string stage, IReadOnlyDictionary<string,UnitScoringProfile> profiles) {
    var readStart = Stopwatch.GetTimestamp();
    var readAllocated0 = GC.GetTotalAllocatedBytes(false);
    using var readProcess = Process.GetCurrentProcess();
    readProcess.Refresh(); var readCpu0 = readProcess.TotalProcessorTime;
    var flow = new TrainableRoutingFlow(new BaselineKeywordCandidateGenerator(profiles),new EmptyLog());
    var rows = new List<object>();
    foreach (var (query,gold) in exam) {
        var r = await flow.RouteAsync(new("Q"+rows.Count,query,"",null,null,candidates,new Dictionary<string,string>()));
        if (!r.IsSuccess) throw new InvalidOperationException(r.ErrorCode+": "+r.ErrorMessage);
        var d = r.Value!;
        rows.Add(new {query,gold,actual=d.ChosenUnitId,d.KnowledgeEdge,d.Confidence,
            correct=!d.KnowledgeEdge && d.ChosenUnitId==gold,d.RankedCandidates});
    }
    readProcess.Refresh();
    readCosts.Add(new { stage, routeCalls=rows.Count,
        elapsedMs=Stopwatch.GetElapsedTime(readStart).TotalMilliseconds,
        processAllocatedBytes=GC.GetTotalAllocatedBytes(false)-readAllocated0,
        processCpuMs=(readProcess.TotalProcessorTime-readCpu0).TotalMilliseconds });
    return rows.ToArray();
}
var before = await Read("before", new Dictionary<string,UnitScoringProfile>());
var trainer = new RoutingProfileTrainer(registry);
using var process = Process.GetCurrentProcess();
process.Refresh(); var cpu0=process.TotalProcessorTime;
var heap0=GC.GetTotalMemory(false);
var alloc0=GC.GetAllocatedBytesForCurrentThread();
var watch=Stopwatch.StartNew();
var trained=trainer.Train(train,registry);
watch.Stop();
var allocated=GC.GetAllocatedBytesForCurrentThread()-alloc0;
process.Refresh(); var cpuMs=(process.TotalProcessorTime-cpu0).TotalMilliseconds;
var heap1=GC.GetTotalMemory(false); var peak=process.PeakWorkingSet64;
if (!trained.IsSuccess) throw new InvalidOperationException(trained.ErrorMessage);
var profiles=trained.Value!;
var after=await Read("after",profiles);
var raw=new {arm=args[0],before,after,profiles,readerCosts=readCosts.ToArray(),trainCalls=1,trainRows=2,routeCalls=8,
    trainMs=watch.Elapsed.TotalMilliseconds,cpuMs,allocated,heap0,heap1,processPeak=peak,
    profileUtf8Bytes=JsonSerializer.SerializeToUtf8Bytes(profiles).Length,
    modelLayers=0,unitLevels=1,flowLevels=1};
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
File.WriteAllText(output,JsonSerializer.Serialize(raw,new JsonSerializerOptions{WriteIndented=true}));

sealed class EmptyLog : IRoutingDecisionLog {
    public Task<OperationResult<bool>> AppendAsync(RoutingDecisionRecord d,CancellationToken ct=default)
        => throw new InvalidOperationException("RecordDecision is outside this experiment");
    public Task<OperationResult<IReadOnlyList<RoutingDecisionRecord>>> ListAsync(CancellationToken ct=default)
        => throw new InvalidOperationException("List is outside this experiment");
}
```

## Команды и предел

Сохранить как `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01/Run.ps1`; запуск — `powershell -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01/Run.ps1`.
```powershell
$ErrorActionPreference='Stop'
$krRoot='C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01'
$krOut='C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01/results'
if(Test-Path -LiteralPath $krOut){throw 'Preserve existing result; no automatic rerun'}
$krClaim=[System.IO.File]::Open((Join-Path $krRoot 'execution.claim'),[System.IO.FileMode]::CreateNew,[System.IO.FileAccess]::Write,[System.IO.FileShare]::None)
$krClaim.Dispose() # retained after any outcome
New-Item -ItemType Directory -Path $krOut | Out-Null
foreach($krArm in @('A','B')){
    if((Test-Path -LiteralPath "$krRoot/$krArm/bin") -or (Test-Path -LiteralPath "$krRoot/$krArm/obj")){throw 'Preserve existing build'}
    & 'C:/Program Files/dotnet/dotnet.exe' build "$krRoot/$krArm/Runner.csproj" -c Release *> "$krOut/$krArm-build.log"
    if($LASTEXITCODE -ne 0){throw "Build failed: $krArm; no retry"}
    & 'C:/Program Files/dotnet/dotnet.exe' "$krRoot/$krArm/bin/Release/net9.0/KeyReader.dll" $krArm "$krOut/$krArm.json" *> "$krOut/$krArm-run.log"
    if($LASTEXITCODE -ne 0){throw "Run failed: $krArm; preserve partial result"}
}
```



## Чтение измеряется на тех же вызовах

Read(before) и Read(after) каждой ветви теперь сохраняют readerCosts вокруг уже назначенного полного reader: создание TrainableRoutingFlow и четыре RouteAsync. Всего остаются2 Train и16 Route, без новых evaluation/inference. Измеряются elapsed, CPU после Refresh и process-wide allocated bytes; CurrentThread allocation нельзя вычитать через await. Поля readerCosts и прежние синхронные TRAIN costs имеют разные области, не смешиваются. Выигрыш reader на действительно trained profiles допустим как learning-dependent result; одинаковый неизменённый Train timer сам такой выигрыш не проверяет. Cold fixed-order timings — наблюдения с неизвестной причинностью. Claims резервируют один запуск до build/native lifecycle; partial не удалять и не повторять.
Два малых build, два процесса; никаких дополнительных warmup/review reruns, повторов обучения или product tests. Сравнить четыре сохранённых rows A0/A1/B0/B1, correct и knowledge-edge раздельно по целевому маршруту `code`/`docs` внутри темы D07; ресурсы TRAIN A/B и их разницу. Unit/Flow levels здесь описывают один локальный scorer/flow, не всю оркестрацию. Полная source closure и сборка ещё не проверены компилятором.

Любое полезное измеренное улучшение — успех по этой оси/домену, без обязательного роста accuracy; ухудшения рядом. Не обещать устойчивое ускорение по одному короткому Train. Точный будущий product target — только две Replace в `BaselineKeywordCandidateGenerator.ScoreAsync`; перенос решает Люба.

> **Промпт исполнителю:** Выполни назначенную фазу KN-ROUTER-LEARNED-KEY-01 по данным, девяти источникам, общему извлечению и командам этой карточки. Модель — текущая по умолчанию. Сравни пустые и обученные profiles на тех же четырёх запросах, меняя только нормализацию reader. Не ищи другой host, teacher или workflow. Сохрани успехи любых полезных показателей и ухудшения, коротко по-русски. При команде перехода немедленно прекрати оформление прошлого опыта.

## Готовый формат отчёта и навыки — без поиска образца

**Язык — русский. Формат — обычный Markdown.** Итоговый файл — `report.md` в точном каталоге результатов выбранного подслучая, указанном в этой карточке; рядом остаются исходные JSON/логи. Не искать другие отчёты, новый реестр или шаблон. Перед каждым новым аудитом/заданием свежо и полностью прочитать governance и весь основной/ролевой стек договора120 из входного блока этой карточки; старые пять навыков не являются полным перечнем. Внутри того же задания отчёт/readback/wait не требуют повторять уже выполненное полное чтение. Исторические команды навыков не расширяют текущую фазу, модель, данные или бюджет.

Скопировать следующий каркас и заполнить только фактическими данными. Пустые измерения писать «не измерено», сбой — «этап не завершён», а не 0.

```markdown
# <ID>: <что изменилось в обучении, простыми словами>
Учебная тема: Оркестрация, D07; контексты маршрутов: code/docs. Исполнитель: <фактическая модель и режим>.
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


