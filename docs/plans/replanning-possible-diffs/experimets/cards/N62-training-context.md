# T-TRAIN-CONTEXT-01. Что теряется, когда урок не сохраняет контекст слова

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница TRANSLATION:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold.

**Исходный адрес из паспорта:** `12.SemanticCompiler/Translation/ContextualDictionary.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



**Текущий статус21.10: CONSUMED_EXTERNAL / VALID_FOR_NAMED_UNIT.** [Проверены сохранённые A/B и exit receipts](../../2026-10-21-d072-integration/t-train-late-result.json) из внешнего core2297566a; эта документная сессия ничего не запускала. A:0/4→2/4, B:0/4→4/4; на3уникальных входах A1/3,B3/3. По2 AddEntry и8 TranslateWithContext на плечо, total4/16; entry consumed, повтор/подготовка/добавочные calls запрещены. При чтении этой карточки использовать существующие `results/A/raw.json`, `results/B/raw.json`, `results/report.md`, `results/stages.txt`. Это acquisition конкретного ContextualDictionary, не доказанный дефект продуктового caller и не полный переводчик. Чтение после обучения:3304→4328 allocated bytes; по одному замеру нельзя установить надёжную границу шума или устойчивый speedup.

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

**Единственное назначение результата T-TRAIN-CONTEXT-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [T-TRAIN-CONTEXT-01](N62-training-context.md) → [E-SFT-RESPONSE-MASK](E-SFT-RESPONSE-MASK.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Эта вставка не добавляет training/evaluation calls к указанному ниже бюджету.

**Учебный сигнал и предел вывода.** На назначенной четырёхстрочной EXAM панели показать переводы в A0/A1/B0/B1 и correct/4; старые контексты и новые значения слова не смешивать. Положительный сигнал — после реального урока появился правильный контекстный перевод. Само сохранение Before/After в DTO этого не доказывает.

**Разрешение конфликтов из последнего аудита (20.10).** Старый запрет копий в `81-validation-program.md` не применяется: используется только назначенная A/B-копия; product/tests/CI не меняются. Ограничение «protected не менять» в отчёте помощника относилось к его поручению, а не отменяет прямое поручение Любы исправить карточки. `NOT_RUN` означает отсутствие результата данной версии, а не запрет; поздняя карточка не меняет протокол уже сохранённого опыта и не назначает повтор. Любое полезное измеренное улучшение — успех по метрике/домену, без порога5% и требования роста accuracy; несопоставимые затраты не приписываются фактору. Связанные подготовка, restore и чтение выученного состояния допустимы с точным source→state→consumer; посторонний scorer-пример не назначает опыт. Вместо отсутствующей84-ссылки — [реестр источников](../01-full-source-registry.md) и [карта покрытия](../2026-10-19-small-learning-probes/06-findings-map.md); следующая работа задаётся [индексом](00-execution-index.md). Назначенные данные, домен, host и команды нельзя заменять самостоятельным поиском другого опыта.

**Порядок программы:** [T-TRAIN-CONTEXT-01](N62-training-context.md) → [E-SFT-RESPONSE-MASK](E-SFT-RESPONSE-MASK.md). Для общей карточки использовать только якорь назначенного ID. Позиции — [текущий индекс](00-execution-index.md); условия перехода — [правило NEXT](00-execution-index.md#next-contract). Сохранённые попытки не повторять; одиночное поручение — END.

## Немедленное завершение по указанию Любы

**Обязательно для каждого подслучая и каждого промпта исполнителю этой карточки.** Прямое текущее указание Любы имеет приоритет над нижними требованиями завершить отчёт, review или весь цикл.

- **«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу.
- Если следующее задание уже указано, не выбирать другое и не обходить каталог в поисках более готового. Если сказано «следующий» или назван порядковый номер — взять его из единого индекса `cards/00-execution-index.md`; не искать N-KU-02 по аналогии с N-KU-01 и не задавать вопрос о выборе. После конца назначенной очереди кратко сообщить об этом, не возобновляя прошлый опыт. Поручение редактировать карточки или описывать опыт само по себе не разрешает запуск обучения.
- **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Команду перехода не подменять остановкой всей программы; команду полной остановки не подменять следующим запуском.
- Незавершённые файлы и уже записанные данные оставить как есть. Их наличие не разрешает продолжать вопреки указанию. Не удалять raw/receipts и не устраивать закрывающую проверку; краткий статус — только по уже известным фактам, без новых инструментов.
- Исполнитель опыта не берёт на себя исправление карточек: это отдельное поручение документной сессии. Остановка одного исполнителя не отменяет отдельное поручение другой сессии, если Люба не остановила и его.

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


20.10.2026 — исходный рецепт; ниже он сохранён как история выполненной внешней попытки, не поручение повторить её. Тема — перевод EN→RU, два значения `book`. Учится настоящий `SemanticCompiler.ContextualDictionary`: `AddEntry` записывает примеры, `TranslateWithContext` использует их. Нейронная модель, LoRA и перевод целых предложений здесь не проверяются.

Исходное поручение Любы:

> «Задача ускорить процесс проверки и сузить обучающийся механизм до очень простых маленьких примеров, что бы проверить улучшаются показатели довольно быстро. Отчетность должна быть человеческим языком но точно показывающая какую часть механизма обучения меняем и как будем тестировать».

Единственный фактор — сохранение POS-массивов **при записи двух уроков**. A передаёт пустые массивы; B сохраняет заданные в уроках. Сам learner, reader, порядок и приоритет примеров неизменны. Оба плеча реально получают уроки. Это проверка ценности информации в обучаемой памяти; она **не доказывает**, что нынешний продуктовый writer действительно теряет POS. Такой caller ещё не установлен. Поэтому для переноса в продукт нельзя автоматически предложить правку найденного `AddEntry`: он уже сохраняет переданную запись.

**Бюджет всего сравнения: 4 AddEntry, 16 TranslateWithContext, 0 BuildDefault, 0 Generate, 0 optimizer updates, 0 Flow, 0 повторов и прогревов.** Два свежих процесса A/B; в каждом две записи и четыре вопроса до/после.

Любое измеренное улучшение полезного показателя — успех по показателю и теме, даже без роста точности. Рядом показываются ухудшения. Снижение allocations, времени или усилий не требует дополнительной генерационной панели. Один замер не устанавливает устойчивое ускорение или причину разницы; дополнительные B→A и reviewer rerun не входят в бюджет.

## Навыки, модель и конфликтные указания

Исполнитель и последовательный независимый проверяющий используют модель своей текущей сессии по умолчанию, без model/reasoning override и поиска Astra. Это прямое уточнение Любы от 20.10.2026. Фактическое имя модели записать; если платформа его не сообщает — «не раскрыто платформой», без догадки. Не более одного активного worker, без вложенных агентов. Модель исполнителя не является обучаемым Student/Unit. Модель не отвечает вместо проверяемого обучаемого словаря.

| Точный путь навыка | Требование для этой карточки |
|---|---|
| [architect-behavior-classifier-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md) | Назвать writer, память, reader и предел вывода |
| [planning-session-startup-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md) | Соблюдать текущее поручение и малый бюджет |
| [plan-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md) | Один фактор, одинаковые начальные состояния и EXAM |
| [generated-code-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md) | Проверить точную копию и отсутствие подмены learner |
| [output-readability-gate-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md) | Отделять фактические ответы от ожиданий |
| [human-readable-architecture-audit-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md) и [словарь](C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md) | Перевод — домен; POS-память — механизм; примеры не называются нейронными слоями |
| [prompt-patch-authoring-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md) | Сохранять узкий промпт и точные данные |

Это указание применяемых навыков, не заявление нового полного reload.

- Текущее поручение этой сессии — **только карточки**. Кодовые блоки ниже не создаются и не запускаются при её редактировании.
- Назначенный исполнитель ограниченного опыта сохраняет эти блоки по указанным адресам. Он не выбирает другой проект, namespace, dispatcher или способ измерения.
- Оригинальные C#, AGENTS, CLAUDE, навыки, `.kiro`, продуктовые тесты и CI не меняются. Product code-slice, S00–S04, публикация и commit/push не являются подготовкой этого опыта.
- В экспериментальном host разрешены `throw`, `try/catch`, `void`, DTO. Не переделывать исходный API в `OperationResult`.
- Старые эксперименты, результаты и `bin/obj` не удалять, не переносить и не перезаписывать. Совпадение пути означает сохранение прежней подготовки, а не разрешение на новый RunId.
- Общего PENDING и личного согласования каждого ID/SHA нет. В рамках порученной программы подготовка этих конкретных файлов входит в работу исполнителя.
- Другая сессия улучшает только карточки; её Markdown/HEAD не меняет уже зафиксированные входы. Снимок прочитанного паспорта сохраняется один раз. После compaction продолжать известный stage по журналу, не повторять завершённые вызовы.
- Reviewer читает source, diff и raw; собственный повтор не назначен. Если назначенная модель проверки недоступна, сохранить результат и отметить отсутствие этой проверки, без замены модели или дополнительного запуска.

## Источник и данные

Исходник целиком: [ContextualDictionary.cs](C:/Projects/llm_mvp_core/12.SemanticCompiler/Translation/ContextualDictionary.cs). SHA256 прочитанного файла:
`9926db1ed06111c7f64fd80f69cf6b81e28af110df77a4c4a452cf46d8313525`.

Нужен ровно этот один C#-файл и стандартная библиотека .NET. Существующий namespace `SemanticCompiler` сохраняется; другой тип с таким именем не подключается. `ContextualDictionary.AddEntry` добавляет запись в `_entries`; `TranslateWithContext` ранжирует через `ContextDistance`, затем Priority. При равенстве сохраняется порядок примеров. Поле `_k` не используется reader этого файла; опыт не меняет его и не заявляет проверку kNN-voting.

Материал — simulation, POS заданы человеком, не вычисляются другой моделью.

| TRAIN | Слово | Before | After | Перевод | Priority |
|---|---|---|---|---|---|
| T1, `the book arrived` | book | `[0]` | `[8]` | книга | 1 |
| T2, `we book rooms` | book | `[3]` | `[7]` | бронировать | 1 |

A записывает те же записи с `Before=[]; After=[]`. B записывает таблицу дословно. DET=0, PRON=3, AUX=4, NOUN=7, VERB=8, ADJ=9.

| EXAM | Слово | Before | After | Gold |
|---|---|---|---|---|
| Q1, `a red book fell` | book | `[0,9]` | `[8]` | книга |
| Q2, `they will book flights` | book | `[3,4]` | `[7]` | бронировать |
| Q3, `this old book vanished` | book | `[0,9]` | `[8]` | книга |
| Q4, `you can book cheap tickets` | book | `[3,4]` | `[9,7]` | бронировать |

Reader получает слово и массивы, не полные фразы. Q1=Q3 на уровне его входа: четыре наблюдения, **три разных tuple**, не четыре независимых свидетельства.

**Ожидание по коду, не измерение:** A0/B0 вернут `book`; A1 при равных контекстах выберет первую запись (2/4, 1/3 уникальных tuple), B1 может различить два значения (4/4, 3/3). В отчёте заменить ожидания actual строками, не подставлять эти числа при ошибке.

## Назначенные файлы

Корень всегда `C:/Projects/llm_mvp_core`.

| Назначение | Точный путь |
|---|---|
| Одна неизменная копия механизма для A/B | `35.ExperimentsV2/Experiments/Rp01Micro/TTrainContext/ContextualDictionary.cs` |
| Проект и host | `experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/Runner.csproj`, соседний `Program.cs` |
| Подготовка и запуск | там же `Prepare.ps1`, `Run.ps1` |
| Снимок входов | там же `source-manifest.json`, `card-snapshot.md` |
| Новый результат | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/A/` и `B/` |

Сначала сохранить четыре следующих блока файловым инструментом, без эмуляции ввода длинных команд в терминал. Проверить, что назначенные файлы ещё не существуют; не заменять ранее сохранённые файлы. Никаких ProjectReference, NuGet-пакетов, product assembly, StackDetector или global usings проекта 35.

### Runner.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <AssemblyName>TTrainContextRunner</AssemblyName>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/TTrainContext/ContextualDictionary.cs" />
  </ItemGroup>
</Project>
```

### Program.cs

```csharp
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using SemanticCompiler;

string? createdDirectory = null;
string stage = "arguments";
int addIssued = 0, addCompleted = 0, queryIssued = 0, queryCompleted = 0;
try
{
    if (args.Length != 3 || args[0] != "--arm" || (args[1] != "A" && args[1] != "B"))
        throw new ArgumentException("Expected --arm A|B absolute-output-directory");
    var arm = args[1];
    var expected = Path.GetFullPath(
        "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/" + arm);
    var output = Path.GetFullPath(args[2]);
    if (!string.Equals(output, expected, StringComparison.OrdinalIgnoreCase))
        throw new ArgumentException("Output differs from the card");
    if (Directory.Exists(output) || File.Exists(output))
        throw new IOException("Existing result is preserved; no retry or overwrite");
    Directory.CreateDirectory(output);
    createdDirectory = output;

    var train = new[]
    {
        new Lesson("T1", "book", new[]{0}, new[]{8}, "книга", 1),
        new Lesson("T2", "book", new[]{3}, new[]{7}, "бронировать", 1)
    };
    var exam = new[]
    {
        new Query("Q1", "book", new[]{0,9}, new[]{8}),
        new Query("Q2", "book", new[]{3,4}, new[]{7}),
        new Query("Q3", "book", new[]{0,9}, new[]{8}),
        new Query("Q4", "book", new[]{3,4}, new[]{9,7})
    };
    var submitted = train.Select(x => arm == "A"
        ? x with { Before = Array.Empty<int>(), After = Array.Empty<int>() }
        : x).ToArray();
    var entries = submitted.Select(x => new ContextualDictionary.DictionaryEntry(
        x.Word, x.Before, x.After, x.Translation, x.Priority)).ToArray();
    var learner = new ContextualDictionary();
    var answers = new List<Answer>();
    var observations = new List<Cost>();
    void ReadExam(string phaseName)
    {
        foreach (var q in exam)
        {
            queryIssued++;
            var actual = learner.TranslateWithContext(q.Word, q.Before, q.After);
            queryCompleted++;
            answers.Add(new Answer(phaseName, q.Id, actual));
        }
    }
    stage = "before-query";
    observations.Add(Measure(stage, () => ReadExam("before")));
    stage = "AddEntry-two-lessons";
    observations.Add(Measure(stage, () =>
    {
        foreach (var entry in entries)
        {
            addIssued++;
            learner.AddEntry(entry);
            addCompleted++;
        }
    }));
    stage = "after-query";
    observations.Add(Measure(stage, () => ReadExam("after")));

    stage = "write-report";
    SaveNew(Path.Combine(output, "raw.json"), new
    {
        arm,
        binding = typeof(ContextualDictionary).AssemblyQualifiedName,
        train, submitted, exam, answers, observations,
        counts = new { addIssued, addCompleted, queryIssued, queryCompleted },
        suppliedPosInts = submitted.Sum(x => x.Before.Length + x.After.Length),
        observationPayloadBytes = JsonSerializer.SerializeToUtf8Bytes(submitted).Length,
        stateEvidence = "submitted AddEntry arguments and successful calls; not a private-state export or checkpoint",
        neuralModelLayers = (int?)null,
        unitFlowLevels = (int?)null
    });
    return 0;
}
catch (Exception error)
{
    if (createdDirectory is not null)
        SaveNew(Path.Combine(createdDirectory, "failure.json"),
            new { error = error.ToString(), stage, status = "INCOMPLETE",
                addIssued, addCompleted, queryIssued, queryCompleted, accuracy = (double?)null });
    Console.Error.WriteLine(error);
    return 1;
}

static Cost Measure(string stage, Action action)
{
    using var process = Process.GetCurrentProcess();
    var gc0 = GC.CollectionCount(0);
    var gc1 = GC.CollectionCount(1);
    var gc2 = GC.CollectionCount(2);
    var allocated0 = GC.GetTotalAllocatedBytes(true);
    var cpu0 = process.TotalProcessorTime.TotalMilliseconds;
    var timer = Stopwatch.StartNew();
    action();
    timer.Stop();
    var cpu = process.TotalProcessorTime.TotalMilliseconds - cpu0;
    var allocated = GC.GetTotalAllocatedBytes(true) - allocated0;
    process.Refresh();
    return new Cost(stage, timer.Elapsed.TotalMilliseconds, cpu, allocated,
        GC.CollectionCount(0)-gc0, GC.CollectionCount(1)-gc1, GC.CollectionCount(2)-gc2,
        GC.GetTotalMemory(false), process.WorkingSet64, process.PeakWorkingSet64,
        Stopwatch.Frequency);
}
static void SaveNew(string path, object value)
{
    using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
    JsonSerializer.Serialize(stream, value, new JsonSerializerOptions { WriteIndented = true });
}
record Lesson(string Id, string Word, int[] Before, int[] After, string Translation, int Priority);
record Query(string Id, string Word, int[] Before, int[] After);
record Answer(string Stage, string Id, string Actual);
record Cost(string Stage, double WallMs, double CpuMs, long AllocatedBytes,
    int Gen0, int Gen1, int Gen2, long ManagedHeapBytes, long WorkingSetBytes,
    long WholeProcessPeakBytes, long StopwatchFrequency);
```

Gold отсутствует во входе learner и runner: сравнение четырёх сохранённых строк с таблицей выполняется после завершения процессов. Повторно вызывать reader для отчёта нельзя. `observationPayloadBytes` — размер JSON переданных записей, **не** native checkpoint и не точный retained heap. Общий peak процесса включает запуск, а query cost — создание строк наблюдений. Такая одинаковая оболочка не даёт чистую наносекундную цену reader.

### Prepare.ps1

```powershell
$ErrorActionPreference = 'Stop'
$tcRoot = 'C:/Projects/llm_mvp_core'
$tcHost = "$tcRoot/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01"
$tcSource = "$tcRoot/12.SemanticCompiler/Translation/ContextualDictionary.cs"
$tcCopy = "$tcRoot/35.ExperimentsV2/Experiments/Rp01Micro/TTrainContext/ContextualDictionary.cs"
$tcCard = "$tcRoot/docs/plans/replanning-possible-diffs/experimets/cards/N62-training-context.md"
$tcManifest = "$tcHost/source-manifest.json"
$tcSnapshot = "$tcHost/card-snapshot.md"
foreach ($tcPath in @($tcCopy,$tcManifest,$tcSnapshot)) {
    if (Test-Path -LiteralPath $tcPath) { throw "Preserve existing input: $tcPath" }
}
$tcHash = (Get-FileHash -LiteralPath $tcSource -Algorithm SHA256).Hash.ToLowerInvariant()
if ($tcHash -ne '9926db1ed06111c7f64fd80f69cf6b81e28af110df77a4c4a452cf46d8313525') {
    throw 'Source differs from the inspected card; do not substitute another implementation'
}
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($tcCopy)) | Out-Null
[IO.File]::Copy($tcSource,$tcCopy,$false)
[IO.File]::Copy($tcCard,$tcSnapshot,$false)
$tcFiles = @($tcCopy,"$tcHost/Runner.csproj","$tcHost/Program.cs","$tcHost/Prepare.ps1","$tcHost/Run.ps1",$tcSnapshot)
$tcRecords = foreach ($tcPath in $tcFiles) {
    [ordered]@{path=$tcPath;sha256=(Get-FileHash -LiteralPath $tcPath -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$tcBody = [ordered]@{source=$tcSource;sourceSha256=$tcHash;files=@($tcRecords)}
$tcStream = [IO.File]::Open($tcManifest,[IO.FileMode]::CreateNew)
try {
    $tcBytes = [Text.Encoding]::UTF8.GetBytes(($tcBody | ConvertTo-Json -Depth 5))
    $tcStream.Write($tcBytes,0,$tcBytes.Length)
} finally { $tcStream.Dispose() }
```

### Run.ps1

```powershell
$ErrorActionPreference = 'Stop'
$tcRoot = 'C:/Projects/llm_mvp_core'
$tcHost = "$tcRoot/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01"
$tcResults = "$tcRoot/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results"
$tcManifest = Get-Content -LiteralPath "$tcHost/source-manifest.json" -Raw | ConvertFrom-Json
foreach ($tcFile in $tcManifest.files) {
    if ((Get-FileHash -LiteralPath $tcFile.path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $tcFile.sha256) {
        throw "Changed prepared input: $($tcFile.path)"
    }
}
if (Test-Path -LiteralPath $tcResults) { throw 'Preserve existing run; do not replay this script' }
[IO.Directory]::CreateDirectory($tcResults) | Out-Null
'BUILD_ISSUED' | Set-Content -LiteralPath "$tcResults/stages.txt"
& dotnet build "$tcHost/Runner.csproj" -c Release -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false *> "$tcResults/build.log"
$tcBuildExit = $LASTEXITCODE
"BUILD_EXIT=$tcBuildExit" | Add-Content -LiteralPath "$tcResults/stages.txt"
if ($tcBuildExit -ne 0) { throw "Build failed: $tcBuildExit" }
$tcDll = "$tcHost/bin/Release/net9.0/TTrainContextRunner.dll"
foreach ($tcArm in @('A','B')) {
    "ARM_${tcArm}_ISSUED" | Add-Content -LiteralPath "$tcResults/stages.txt"
    & dotnet $tcDll --arm $tcArm "$tcResults/$tcArm" *> "$tcResults/console-$tcArm.log"
    $tcExit = $LASTEXITCODE
    "ARM_${tcArm}_EXIT=$tcExit" | Add-Content -LiteralPath "$tcResults/stages.txt"
    if ($tcExit -ne 0) { throw "Arm $tcArm failed: $tcExit" }
}
```

Исторические команды выполненной внешней попытки — не выдавать повторно:

```powershell
powershell.exe -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/Prepare.ps1"
```

```powershell
powershell.exe -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/Run.ps1"
```

Истечение времени ожидания инструмента не означает завершения процесса: наблюдать тот же выданный process/session до exit, не запускать вторую копию скрипта. Restore-only log или старый DLL не доказывает успешную сборку. При неизвестном исходе сохранить журнал; повтор из начала не поручен. Freeze/manifest не переписывать ради форматирования. Эти действия назначены будущему исполнителю; документная сессия не проверяет его процессы.

## Отчёт и проверка результата

В той же папке результата сохранить `comparison.md`, читая только два raw.json. Показать каждую Q в A0/A1/B0/B1, correct/4 и correct/3 unique, отдельно noun и verb. Для каждого показателя показать исходные значения и изменение; для затрат обучения сравнить интервалы двух настоящих AddEntry, а не нулевую длительность до уроков.

| Показатель | A до → после | B до → после | Что означает |
|---|---|---|---|
| Правильных /4; /3 разных входов | — | — | Перевод одного выделенного слова |
| Выбор noun / verb | — | — | Потеря одного значения не скрывается средним |
| AddEntry issued/completed | — | — | Ожидается 2/2 на плечо |
| TranslateWithContext issued/completed | — | — | Ожидается 8/8 на плечо |
| Train wall/CPU/allocated bytes | — | — | Отдельные показатели, малый одиночный замер |
| Heap/RSS/whole-process peak | — | — | Не стоимость только двух записей |
| Переданные POS ints; JSON наблюдения bytes | — | — | Цена представления, не метрика усвоения сама по себе |
| Слои модели / Unit/Flow | N/A | N/A | Один словарь; нейронных слоёв и Flow нет |

**Хороший отчёт — только пример формы:** «Сохраняли POS при двух записях в словарь. На неизменных входах выбор глагольного значения изменился; точные raw ответы ниже. Представление стало больше на …, время неразличимо. Это эффект записи контекста в данном learner; потерю POS в продуктовой цепи не установили».

**Плохой отчёт:** «Переводчик восстановлен, LoRA обучилась, RAM выросла — значит весь опыт провален».

Промпт текущему читателю результата:

> T-TRAIN-CONTEXT-01 уже consumed. Прочитай только назначенные A/B raw, stages.txt и report.md. Не создавать и не выполнять Prepare/Run/Program, не пересчитывать ответы, не повторять AddEntry/TranslateWithContext. Сверь A0/A1/B0/B1 и фактические4 AddEntry/16чтений; сохрани различие4наблюдений/3уникальных tuple и ограничение на неизвестный продуктовый caller. По обычному поручению чтения сообщи уже сохранённый результат; прямой STOP/NEXT имеет приоритет.

Источник нового учебного контраста: [05-T-TRAIN-CONTEXT-SMALL-EXPERIMENT.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/05-T-TRAIN-CONTEXT-SMALL-EXPERIMENT.md>). Его альтернативы проекта 34/35, запрет писать результаты в этот каталог и требование самостоятельно назначить пути не перенесены: прямое задание Любы этой сессии задаёт каталог и требует точного рецепта. Это самостоятельная карточка записи признака, не повтор старого query-only T и не новый исторический результат E26.

Проверка этой редакции — чтение исходного класса, его hash и текста рецепта. C#/csproj/scripts из Markdown не создавались, не компилировались и не исполнялись. Новая сверка читает внешний сохранённый результат и exit receipts; команды выше остаются историей consumed attempt. Эта документная сессия не исполняла их.
