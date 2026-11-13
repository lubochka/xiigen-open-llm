# POS-QUERY-SET-ONCE-01 — один набор биграмм запроса на чтение обученной POS-памяти

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница TRANSLATION:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold.

**Исходный адрес из паспорта:** `12.SemanticCompiler/Translation/PatternClassifier.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



**Маршрут общей порученной программы:** POS-QUERY-SET-ONCE-01 → [HIERARCHICAL-LOCAL-HISTORY-01](HIERARCHICAL-LOCAL-HISTORY-01.md); для отдельного назначения одного ID — END. Текущий STOP/NEXT выше маршрута.

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

**Действующая внутренняя карточка POS-QUERY-SET-ONCE-01.** В общей разрешённой программе после [KN-ROUTER-LEARNED-KEY-01](KN-ROUTER-LEARNED-KEY-01.md); NEXT=HIERARCHICAL-LOCAL-HISTORY-01. Позиция и переход программы — [текущий индекс](00-execution-index.md#next-contract). [Источник14308](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-mechanism-audit/cards/POS-QUERY-SET-ONCE-01.md>) прочитан и адаптирован к внутренним правилам. Текущий приоритет — PA01 → исправленный08 → открытая N63C; POS не обходит эти задачи.

**AUTHOR_SETUP_REQUIRED_NOT_RUN / HOST_NOT_MATERIALIZED. Приоритет: после разблокирования обучения, конфликтов и прямых учебных опытов.** Это новая конкретная гипотеза снижения allocations при использовании выученного результата. Она не меняет алгоритм обучения, Jaccard, признаки, k или голосование и не доказывает исправление neural RP01. Текущая фаза — только документация. Этот паспорт задаёт будущую реализацию после поручения исполнить ID; сейчас не создавать host и не запускать команды.

## Исполнитель, домен и ожидаемый результат

Модель исполнителя и проверяющего — модель текущей сессии по умолчанию, без override. Обучаемый механизм — `SemanticCompiler.PatternClassifier` проекта 12, не модель агента и не H512 Student. Домен — перевод и языковые формулировки, узкая задача определения порядка POS; синтетическая панель не доказывает качество естественного перевода. Отчёт — русский Markdown, до 350 слов пояснений плюс таблицы и ссылки на raw; кандидат — до 120 слов с адресом изменения, измеренной осью, ухудшениями и пределом.

Применить адресно skills из `C:/Projects/llm_mvp_core/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Указать реально прочитанные разделы; не писать FULL_LOADED для частичного чтения. Старые автоматические Tier escalation/повторы и model overrides не отменяют пользовательский конечный бюджет и текущую модель.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Изменения остальных показателей, включая ухудшения, показываются рядом.** Успех не разрешает автоматически переносить правку в продукт.

**Запрет продолжать прошлую задачу:** при «следующий эксперимент» немедленно перейти к назначенному NEXT, не дописывая прежний отчёт, hash, review, Git или повторный запуск. При STOP / «остановить всё/закончить немедленно» прекратить работу. Незавершённые артефакты сохраняются; запрещено продолжать под видом завершения. У одиночного назначения NEXT=END; в общей порученной программе следующий ID — HIERARCHICAL-LOCAL-HISTORY-01 (№51), по индексу.

## Причина и реальное участие в обучении

[Train:39–43][native] записывает POS-последовательности и метки в `_train`; [Predict:46–54][native] читает эту память, ранжирует соседей, берёт k3 и голосует. [SequenceDistance:78–86][distance] строит `bigramsA` для **одного и того же запроса заново на каждой обученной строке**, а также `bigramsB` для строки памяти. В E25 TRAIN=128; один Predict повторяет подготовку query128 раз. Это source count, ещё не измеренная цена.

Путь: native GenerateExamples → native Train128 → изменённая `_train` → native Predict → class output → сравнение с frozen label. Фактор участвует непосредственно в чтении выученной таблицы. Поэтому это допустимый опыт зависимости результата от обученной памяти по правилу пользователя 4, но он идёт последним. Не заменять его отдельными вызовами private distance без Train. Текущий production fusion использует другой rule-classifier проекта 29; данный опыт не переключает fusion и не измеряет его качество. [Точный consumer][consumer].

## Единственное различие A/B

A — неизменённый `12.SemanticCompiler/Translation/PatternClassifier.cs` на core `d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19`, Git blob SHA1 `4d7a4c6ba2c9bb84bc915adebf7bd818135ebeba`.

B — копия того же файла. После `if (_train.Count == 0) return PredictByRules(posSequence);` один раз построить `HashSet<(int,int)> queryBigrams` тем же последовательным циклом `i=0..Length-2`. В сортировке передавать этот набор вместо исходного `posSequence`. У private `SequenceDistance` первый аргумент — подготовленный query set; удалить только повторное создание/заполнение bigramsA. Создание bigramsB, intersection/union, `union==0 ? 0`, порядок LINQ, стабильные ties, k3 и vote неизменны. Не кешировать train sets, не добавлять persistent cache, не изменять Train или fallback. Набор query локален одному Predict и не изменяется внутри comparator.

Следствие по исходнику: после Train на 32 запросах число query-set построений 4096→32; число training-set построений 4096→4096. Это контроль реализации, не измеренная экономия памяти и не 32768 искусственных повторов ради timing. Самую первую холодную ветку без `_train` B оставляет без query set.

Адрес будущего переноса, только после решения Любы: тот же `PatternClassifier.cs::Predict/SequenceDistance`. Предположение ownership: в течение синхронного Predict caller не меняет `posSequence` и TRAIN arrays; fixture неизменяема. Расширение на concurrent mutation не проверяется и не обещается.

## Модель, данные, before/after и бюджет

Данные — **native** `PatternClassifier.GenerateExamples(seed:42, perPattern:20)` из того же pinned файла. Получить ровно 160items; первые 128TRAIN, последние 32EXAM. Выполнить генератор один раз в общем prepare, сохранить все ordered POS arrays/labels в `H/in/fixture.json`; A/B читают одни bytes. Сохранить тип/версию .NET и hash fixture. Никаких внешних POS taggers, corpus, перебора seeds либо изменения split. Повторы шаблонов сохраняются и ограничивают вывод синтетической задачей; это не family-disjoint natural-language экзамен.

По этим же bytes до Train записать число каждого label в TRAIN/EXAM, число уникальных POS sequences, exact пересечение splits, повторы и противоречащие labels одинаковой sequence. Отдельно посчитать совпадения множества POS-bigrams — именно его читает расстояние; разные arrays с тем же набором bigrams не являются новыми для этого признака. Отсутствующий в TRAIN класс явно пометить unsupported. Постоянную метку выбрать по TRAIN majority, tie-break — минимальный enum ID, а не лучший результат EXAM; затем посчитать её ответы по32gold и каждому классу без Predict. Состав не перебирать и не «исправлять» новым seed: признаки совпадения ограничивают вывод памятью/шаблонами; невозможность заявленного различения оставляет цельOPEN. Это обработка единственной назначенной fixture, не дополнительный Generate/Train/Predict и не изменение бюджета.

В каждом отдельном процессе A и B:

1. Создать пустой classifier(k3). Снять 32 Predict для EXAM — A0/B0. Это native rule fallback до обучения, а не обученное состояние.
2. Ровно один `Train(TRAIN128)`. Сохранить ordered TRAIN hash, число строк/labels и время/allocated/CPU этого вызова отдельно. Train действительно заполняет память; никаких LoRA/optimizer updates нет.
3. Ровно 32 Predict на тех же EXAM — A1/B1. Сохранить каждый ответ, label, правильность и стоимость всей 32-строчной панели. Hard subset считать по **уже полученным** ответам; полный `E25.Run` запрещён, потому что он повторно вызывает Predict на hardCases.

Итого: **1 GenerateExamples в общей подготовке, 2 Train, 256 предъявлений строк (128 уникальных общих), 128 Predict, 0 нейронных updates, 0 нейронного Generate, 0 warmup, 0 retry, 0 дополнительных hard-panel вызовов**. Исторический E25 показал 11/32→28/32 на этом seed/tier; это основание достаточности набора, не гарантированный будущий результат. Ненулевой полный class output непосредственно измеряется. Если both before/after correct=0 либо фактический Train не меняет состояние — `LEARNING_COMPARISON_INVALID`, без добавления шагов/примеров. Исходные 0/32 не заменять margin; исторические числа в новые raw не переносить.

После обеих назначенных рук оценить учебную валидность: в каждой реально заполнена память128 и есть EXAM-строки wrong-before→correct-after; показать число разных правильных классов и сравнение с заранее выбранной TRAIN-majority меткой по32gold. Одна улучшившаяся строка сама по себе недостаточна, если ответы и заявленное различие объясняет постоянный ответ. Это итоговая проверка после конечного A/B, не отбор fixture/seed по EXAM и не причина пропустить технически исправное B. Если приобретённое различение не показано, отдельно сохранить затратное наблюдение, learning objective=OPEN; не выдавать его за учебный успех. Исторические11/32→28/32 — основание выбора фиксированного набора, не обещание воспроизведения.

Native память читать после constructor и после Train вне timed regions через точный private field `_train` (`BindingFlags.Instance|NonPublic`), без дополнительных Predict/Train: проверить0→128 и ordered arrays/labels, не только hash входного списка. Общий observer одинаков вA/B, immutable fixture не меняется.

A0/B0 и A1/B1 должны совпасть по каждому ответу, ordered memory и labels. Сохранение distance/vote следует проверять по единственному source diff: отдельного runtime dump всех расстояний/голосов этот бюджет не назначает, и совпавшие ответы сами по себе не объявлять таким dump. State хранится в памяти; persistence не исследуется. Не вызывать дополнительный classifier ради проверки parity: исходная source-copy и raw двух назначенных процессов достаточны для данного бюджета.

## Точные будущие host, файлы и команды

H=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01`; R=`H/results/attempt-0001`. Пути намеренно короткие; заранее проверить фактическую длину каждого создаваемого пути <240 символов. Если H уже занят иной работой или имеется `execution.claim`/R — `EXISTS_OR_UNKNOWN`, ничего не удалять, не менять suffix, не искать другое место.

При поручении авторской реализации создать только `H/run.py`, `H/host/Program.cs`, `H/A/Runner.csproj` и `H/B/Runner.csproj`. Native copies и fixture заранее не создавать: их единственный владелец — wrapper после exclusive root claim. Он создаёт `H/A/Native/PatternClassifier.cs` как неизменённую копию, `H/B/Native/PatternClassifier.cs` с единственным указанным B-patch; затем режим prepare создаёт `H/in/fixture.json` ровно один раз. Compile обоих проектов — ровно их own Native/PatternClassifier.cs и общий `../host/Program.cs`. AssemblyName=`PosQueryRunner`, OutputType=Exe, TargetFramework=net9.0, ImplicitUsings/Nullable=enable; default Compile items отключены. ImportDirectoryBuildProps/Targets=false, BaseOutputPath=`$(MSBuildThisFileDirectory)bin/`, BaseIntermediateOutputPath=`$(MSBuildThisFileDirectory)obj/` и MSBuildProjectExtensionsPath=`$(MSBuildThisFileDirectory)obj/` задаются **до** явного `Sdk.props`; `Sdk.targets` импортируется после properties/items. Импорты Sdk используют `Sdk="Microsoft.NET.Sdk"`. Ни ProjectReference на продукт, ни NuGet packages, ни ссылка на весь 35.ExperimentsV2 не нужны. Зависимости — только .NET9 BCL.

Program имеет `prepare-fixture --out <H/in/fixture.json>` и `run --arm A|B --fixture <...> --out <R/A|R/B> --owner <nonce>`. Первый режим выполняет ровно один native GenerateExamples и сериализует fixture; у него 0 Train и 0 Predict, он не создаёт arm claim. Его защищают owner nonce уже созданного root claim и одноразовый `H/prepare.claim`, создаваемый через CreateNew до GenerateExamples. Прямой повтор prepare с тем же nonce также отклоняется. Mode prepare получает `--owner <nonce>`; nonce назначает только root wrapper. Runner не принимает произвольные k/seed/budget/данные. Только режим `run` атомарно создаёт `training.claim` своей руки **до** classifier construction и всех 32 before Predict, Train и 32 after Predict; сравнивается owner nonce общего claim. Ответы до обучения тоже входят в issued budget.

Внешний entry — будущий файл, существование/сборка сейчас не утверждаются:

```powershell
& 'C:/Python314/python.exe' 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/run.py'
```

Wrapper — один владелец: exclusive-create `H/execution.claim` → read-only чтение pinned source и Git-blob SHA check → CreateNew A-copy и B-copy с единственным patch → один buildA, один buildB → один fixture generation режимомA без arm claim → runA со своим arm claim → runB со своим arm claim → compare/report. После B-patch исходник A поверх B не копировать. SHA drift или отсутствующий исходник → `SOURCE_DRIFT`, не выбирать другой commit. Не нужен новый approval каждого hash. Полные build-команды:

```powershell
dotnet build 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/A/Runner.csproj' -c Release --nologo
dotnet build 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/B/Runner.csproj' -c Release --nologo
```

Внутренние команды wrapper после обоих build (их не выдавать вторым внешним запуском; `OWNER_NONCE` — записанный wrapper nonce, не новый выбор исполнителя):

```powershell
dotnet 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/A/bin/Release/net9.0/PosQueryRunner.dll' prepare-fixture --out 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/in/fixture.json' --owner OWNER_NONCE
dotnet 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/A/bin/Release/net9.0/PosQueryRunner.dll' run --arm A --fixture 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/in/fixture.json' --out 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/A' --owner OWNER_NONCE
dotnet 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/B/bin/Release/net9.0/PosQueryRunner.dll' run --arm B --fixture 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/in/fixture.json' --out 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/B' --owner OWNER_NONCE
```

Wrapper запускает только эти дочерние processes без видимых окон, сохраняет свой PID/start/exit и управляет только своим process tree; текущий каталог core root. Fixture JSON состоит из DTO `Rows[{PosSeq:[int],Pattern:int}]` с160ordered rows и split128; не полагаться на сериализацию ValueTuple fields по умолчанию. Parent directories создаёт wrapper, leaf arm folders и training.claim — Program: один владелец на каждый путь. Сборка каждой руки≤120s, prepare≤30s, каждая измерительная рука≤60s, сводка≤30s; повторов нет. На build error сохранитьstdout/stderr/exit; не чинить production или product tests. Для output использовать `CreateNew`, не overwrite. Wrapper создаётRодин раз, children — только своиR/A,R/B; двойное reserve одним путём запрещено.

## Ожидание, метрики и отчёт

Сначала обнаружить реально доступные launch/wait/status и прочитать их схемы. Зафиксировать tool_wait_binding: provider, handle, ongoing/terminal markers, смысл yield/RPC timeout и первоначальный experiment/phase deadline. После выдачи команды ждать тот же handle настоящим bounded wait, не более 60 секунд на один вызов, сообщая прогресс не реже раза в минуту. Два быстрых неблокирующих snapshot без новой информации требуют перейти в настоящий wait. Нормальный silent/running wait продолжать до terminal state или исходного deadline; правила «только два ожидания» нет. Независимая диагностика назначенных файлов и своего PID/session нужна при неисправном wait или противоречии каналов; затем для живого процесса вернуться к wait. Отсутствие stdout не доказывает отказ. Ни диагностика, ни tool yield не продлевают бюджет и не разрешают второй launch. RPC/yield timeout не равен experiment/phase deadline. Только по истечении назначенного experiment/phase deadline сохранить PID/session/start/elapsed/exit/terminal state и завершить принадлежащий wrapper процесс, если он ещё жив. Неизвестное завершение остаётся UNKNOWN, не 0 updates. [Контракт discovery/wait](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md); схему конкретного Kiro/Codex provider проверить в своей среде, не копировать отсутствующий tool по имени.

Снимать allocations (`GC.GetAllocatedBytesForCurrentThread`) вокруг Train и отдельной 32Predict панели, wall monotonic clock, CPU process delta, GC0/1/2 и process peak. Не делать `GC.Collect` или дополнительный прогрев. Сериализация/compare вне измеряемой панели; использовать массивы результатов, не filewrite внутри каждой distance. Общая instrumentation одинакова вA/B; нельзя добавить query-set расходы вA ради удобства refactor. Данные динамическогосчётчика не заменяют native calls.

В отчёте показать A0/A1/B0/B1correct/32, все изменённые ответы, ΔA/ΔB и difference-in-differences; ожидаемая разница качества 0 не является провалом cost-фактора. Снижение allocations trained-readout — полезная метрика; Train costs, output equality, время/CPU/peak и ухудшения рядом. Одна fixed-order A→B пара не доказывает устойчивого ускорения: `observed_metric_success` и `causal_attribution` раздельны. Не выдавать стоимость Predict за цену обучения или общего перевода.

ВR: `report.md`, `candidate.md` (только при measured gain), `arms.json`, `per-row.json`, `costs.json`, `verdict.json`, `source-manifest.json`, `fixture.json`/hash, `one-factor.diff`, build logs/exit receipts, process receipts и фактический бюджет. Формат R/verdict.json: отдельно execution_status, learning_validity, hypothesis_status; actual counts и unknown=null. Обычное завершение с полезным измеренным результатом требует R/candidate.md и одной записи≤120слов в [required-changes.md](../../required-changes.md) с исходным методом, метрикой, регрессиями и raw. STOP/NEXT выше оформления. Другие product/документные поверхности не менять. Исторические эксперименты/raw/bin/objне удалять и не переписывать. Пользователь решает дальнейший перенос; полного CI/solution build нет.

[Полный промпт исполнителю](../prompts/POS-QUERY-SET-ONCE-01-executor.md). [Независимая source/design проверка](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-mechanism-audit/reviews/independent-review.md>). Число новых запусков в текущем аудите —0.

[native]: C:/Projects/llm_mvp_core/12.SemanticCompiler/Translation/PatternClassifier.cs:34
[distance]: C:/Projects/llm_mvp_core/12.SemanticCompiler/Translation/PatternClassifier.cs:78
[consumer]: C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:213
