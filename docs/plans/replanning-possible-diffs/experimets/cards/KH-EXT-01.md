# KH-EXT-01 — недостаточный следующий набор не стирает обученную память методов

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница COMPILER/LEARN:** Сверить назначенный уровень compiler: входные features, target packing и ordinary consumer не взаимозаменяемы. Отрицательный/положительный witness берётся из собственного корпуса этой карточки; copied distances/accuracy не обещать native пути без parity. Если вызывается Compile, учесть его реальные вложенные Grammar/Ngram Train. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Исходный адрес из паспорта:** `12.SemanticCompiler/SemanticCompilerFacade.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE/results/report.md>).
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

**Единственное назначение результата KH-EXT-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE/results; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [KH-EXT-01](KH-EXT-01.md) → [GRADB-LOCALITY-01](GRADB-LOCALITY-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Эта вставка не добавляет training/evaluation calls к указанному ниже бюджету.

**Учебный сигнал и предел вывода.** Положительный контроль — native ответы после T1 на назначенной Q-панели, затем их удержание после T2/отказа. Показать non-null и Loop/correct counts, потерю нового материала при отказе B. Одни арифметические числа eligible pairs не заменяют обученный ответ.

**Обязательное правило карточки и её промпта.** По указанию «заканчивай и переходи» немедленно перейти к «Дальше» в индексе: без дописывания прежнего отчёта, hashes, Git, review, повторного запуска и закрывающей проверки. «Остановить всё» прекращает всю работу. Старые результаты не удалять. По одиночному поручению не начинать следующий опыт.



Меняем допуск следующего набора **до** разрушительного `Train`; учится настоящая `MethodKnnPredictor._pairs`; проверяем сохранность её собственных ответов. Это уточнение известного LT-N02 (методы против пар), а не новая причина RP01 или изменение нейронных весов.

Рецепт записан; host, копии и результаты этим проходом не создавались, build/runtime не проверены. `NOT_RUN` не запрет. Номер и следующая работа — [единый индекс](00-execution-index.md).

## Поручение и правила исполнения

Исходное поручение: «рассмотри последние обновления помощников и выстрой эксперименты, как можно более простые и маленькие для каждого домейна». Модель исполнителя и отдельного последовательного reviewer — модель текущей сессии по умолчанию; фактическое имя в отчёте. Один worker, без вложенных; self-review не независимый reviewer.

Разрешена точная экспериментальная копия, старый `81` copy-ban исторический. Host может применять `throw`, `try/catch`, `void`, простые DTO; native алгоритм/ошибки не подменять. Product, 35.csproj/dispatcher, тесты/CI, S00–S04, commit/push не входят. Перенос решает Люба. Нет нового личного допуска ID/SHA, порога 5% или blanket PENDING.

Карточка читается один раз; параллельная сессия меняет только документы и не требует нового чтения/перезапуска по HEAD. Ручные hashes отчёта/паспорта не нужны. Старые результаты, bin/obj и partial сохраняются.

**Немедленный переход/стоп:** указание Любы перейти прекращает все дополнительные действия прежнего опыта, включая отчёт, review, hashes и правки карточки; следующий берётся из индекса в разрешённой фазе. Полная остановка прекращает всё, кроме необходимой остановки собственного живого процесса. Незавершённое не дописывать вопреки указанию. Документный заказ сам не запускает runtime; исполнитель не берёт работу документной сессии.

| Навык относительно K | Что применяется |
|---|---|
| `docs/skills/architect-behavior-classifier-SKILL.md` | Writer → память → reader и точная граница |
| `docs/skills/planning-session-startup-SKILL.md` | Только назначенная фаза и файлы |
| `docs/skills/plan-review-SKILL.md` | Один фактор, retention не обобщение |
| `docs/skills/generated-code-review-SKILL.md` | Сверить копию/admission и счёт действий |
| `docs/skills/output-readability-gate-SKILL.md` | Русские наблюдения и цена решения |
| `docs/skills/human-communication-GLOSSARY.md` | Домен C#, механизм kNN, не LoRA |

## Код, вмешательство и граница применения

[TrainMethodKnn:179–190](C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerFacade.cs:179) допускает `allMethods.Count>=4`. [MethodKnn.Train:120–130](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/MethodKnn.cs:120) сначала очищает `_pairs`, затем оставляет соседей одного `FileId` с последовательными `IndexInFile`. [Reader:314–323](C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerFacade.cs:314) требует `TrainingSize>=4`, где size — число пар.

**A:** исходный допуск по числу методов. **B:** только допуск `eligiblePairs>=4` до native Train. При недостатке не вызывать разрушительную замену. Не менять Clear на append, extractor, canonical labels, k5, reader minimum4, метрики расстояния и голоса. Получатель сохраняется между двумя наборами одного плеча.

`source → MethodExtractor.Extract → admission → MethodKnnPredictor.Train → _pairs → порт PredictNextMethodType → native Predict`. Полный Compile не нужен и не вызывается: он трогает другие memories. Public caller/reader подтверждены исходником; отдельная production-сессия, реально подающая такие T1/T2 на тот же экземпляр, этим чтением не установлена. Это проверка явного контракта «недостаточный replacement сохраняет прежнюю компетенцию». Если продукт намеренно очищает память таким набором, это отдельное решение о политике.

## Точные данные и стадии

T1 — один source с пятью методами; четыре next-label равны Loop.

```csharp
class Seed { int Start(){return 1;} void Stage1(){for(int i=0;i<2;i++) {}} void Stage2(){for(int j=0;j<3;j++) {}} void Stage3(){for(int k=0;k<4;k++) {}} void Stage4(){for(int n=0;n<5;n++) {}} }
```

T2 — четыре **разных файла**, по одному source:

```csharp
class NewA { int OnlyA(){return 10;} }
class NewB { int OnlyB(){return 20;} }
class NewC { int OnlyC(){return 30;} }
class NewD { int OnlyD(){return 40;} }
```

EXAM — четыре source, не передаваемые в Train:

```csharp
class QueryA { int BeforeStage(){return 7;} }
class QueryB { void BeforeStage(){for(int x=0;x<7;x++) {}} }
class QueryC { void BeforeStage(){for(int x=0;x<8;x++) {int y=x+1;}} }
class QueryD { void BeforeStage(){foreach(int x in new int[]{1,2}) {}} }
```

Для этого условного учебного конвейера заранее требуется следующая стадия `Loop` на всех четырёх запросах. TRAIN T1 содержит4пары одного класса; EXAM4/4 того же класса. Постоянный `Loop` тоже даёт4/4: правильность на этой панели не доказывает различение классов или широкую компетенцию C#. Узкий предмет — сохранение ранее записанной памяти и её ответа после непригодного replacement. Показать сохранённые rows/counts и ответы до/после, отдельно от заявления о новом навыке. Новые классы и generalization этим набором не проверяются; B, отвергнувшее T2, не объявлять обучившимся на T2. [Контракт данных](../32-training-set-and-inference-contract.md).

На каждом плече: **Q0 → T1 → Q1 → T2 → Q2**, четыре запроса на каждом Q. Основное до/после retention — A1/B1 после T1 и A2/B2 после T2; A0/B0 дополнительно показывают пустой старт. У B отклонение T2 — реальный исход admission, а не притворное усвоение нового материала.

| Плановая операция | A | B | Всего |
|---|---:|---:|---:|
| Учебных входов T1/T2 | 2 | 2 | 4 |
| Предъявленных source-файлов | 5 | 5 | 10 |
| Извлечённых методов | 9 | 9 | 18 |
| Native Train, ожидаемо | 2 | 1 | 3 |
| Logical queries | 12 | 12 | 24 |
| Native Predict, ожидаемо | 4 | 8 | 12 |
| Закрытых guard запросов | 8 | 4 | 12 |
| Optimizer / Generate / Compile / warmup / retries | 0 | 0 | 0 |

Не добавлять уроки, если фактический extractor дал другие counts; сохранить расхождение. Полезный нулевой/идемпотентный результат реального учебного входа валиден.

## Назначенный состав и алгоритм host

`K = C:/Projects/llm_mvp_core`. Source-root строго `35.ExperimentsV2/Experiments/Rp01Micro/KHE/Source/`. Скопировать неизменными три файла, сохранив namespaces/copyright:

| Original | Имя копии | Зачем |
|---|---|---|
| `12.SemanticCompiler/Prediction/MethodKnn.cs` | `MethodKnn.cs` | Настоящие Extract/Train/Predict |
| `12.SemanticCompiler/Core/Core.cs` | `Core12.cs` | Родные OperationResult/StackLabel и compile types |
| `29.SemanticCompiler/Core/Core.cs` | `Core29.cs` | Родные типы зависимости Core12 |

Host `experiments-v2/RP01-MICRO/KHE/Runner.csproj` и `Program.cs`; результат `docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/KH-EXT-01/`. Алгоритм learner в единственной общей копии одинаков A/B; фактор находится в тонком `KhExtEntry` ниже. Не менять original facade.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>KhExtRunner</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
    <Compile Include="Program.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KHE/Source/MethodKnn.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KHE/Source/Core12.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KHE/Source/Core29.cs" />
  </ItemGroup>
</Project>
```

Без ProjectReference. Program использует `SemanticCompiler`, `SemanticCompiler.Prediction`, `System.Diagnostics`, `System.Text.Json`; global `internal sealed class KhExtEntry(bool validatePairs)` содержит один `readonly MethodKnnPredictor memory = new(k:5)`.

Точная операция учебного входа, рядом с измерителем сохранить prepared methods/eligible count без повторного Extract:

```csharp
var methods = new List<PackedMethod>();
for (short fileId=0; fileId<Math.Min(sources.Count,short.MaxValue); fileId++)
{
    var extracted=MethodExtractor.Extract(sources[fileId],fileId);
    if(extracted.IsSuccess) methods.AddRange(extracted.Value!);
    // Ошибка не превращается в урок: записать исходные ErrorCode/ErrorMessage.
}
int eligible=0;
for(int i=0;i<methods.Count-1;i++)
    if(methods[i].FileId==methods[i+1].FileId
       && methods[i+1].IndexInFile==methods[i].IndexInFile+1) eligible++;
bool accepted=validatePairs ? eligible>=4 : methods.Count>=4;
if(accepted) memory.Train(methods);
```

Этот диагностический подсчёт одинаков в A/B; только B использует его в admission. Цена замера включает его в обоих плечах, поэтому это не чистое измерение расходов production A без диагностики.

Reader точно:

```csharp
string? Query(string source)
{
    logicalQueries++;
    if(memory.TrainingSize<4) { guardNulls++; return null; }
    var extracted=MethodExtractor.Extract(source);
    if(!extracted.IsSuccess || extracted.Value!.Count==0) return null;
    nativePredicts++;
    return memory.Predict(extracted.Value[^1]);
}
```

Flag `UseMethodKnn=true`, stack CSharp фиксированы обоим. CLI ровно `--arm A|B <absolute-result-directory>`; ожидаемый result-root фиксирован. Обычные JSON DTO: `sources`, extracted `PackedString/KindBag/FileId/IndexInFile`, eligible/accepted, size before/after, Train attempts/native counts, Q0/Q1/Q2 raw answers, nonnull/correct/retained, Train phase wall/CPU/allocated bytes и whole-process peak. `FileMode.CreateNew` для `A.started.json`/`B.started.json`; существующие markers/raw не перезаписывать. Ошибка — partial receipt и ненулевой exit, не retry.

## Команды

```powershell
$khHost='C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE'
$khOut='C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHE/results'
dotnet build "$khHost/Runner.csproj" -c Release --nologo -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if($LASTEXITCODE -ne 0){throw 'Host build failed; no learning started'}
dotnet "$khHost/bin/Release/net9.0/KhExtRunner.dll" --arm A $khOut
if($LASTEXITCODE -ne 0){throw 'A failed; preserve partial data; no retry'}
dotnet "$khHost/bin/Release/net9.0/KhExtRunner.dll" --arm B $khOut
if($LASTEXITCODE -ne 0){throw 'B failed; preserve partial data; no retry'}
```

Два свежих процесса, порядок A→B. Исполнитель реализует именно описанную оболочку, не ищет другой host. Ошибки её сборки исправляются локально до runtime с прежними данными/фактором. Никаких BA/reviewer rerun, facade crosscheck или общей сборки.

## Ожидание, метрики и русский отчёт

По исходнику ожидаются sizes A:0→4→0, B:0→4→4; Q1 Loop в обоих, Q2 A null/B Loop. Это **не измеренные результаты**. Цена B — T2 не усвоен, память сохраняет старые сведения; время admission может вырасти. Нет требования превысить Q1 accuracy или обязательно получить ненулевой delta.

Любое измеренное полезное улучшение — успех своей метрики/домена, без обязательного роста accuracy; регрессии рядом. Retention denominator — число собственных правильных Q1, non-null denominator — четыре; не смешивать их. Stopwatch/CPU/allocated bytes снимаются вокруг Extract+admission+Train каждого T отдельно; запись JSON и вопросы вне интервала. Allocations не peak RAM; process peak включает всё плечо. Нейронных слоёв/optimizer нет, одна обучаемая таблица, полный Unit/Flow не запускался.

`report.md` ≤350 слов плюс таблицы/raw: что меняли; writer→state→reader; T1 acquisition и T2 retention; actual вызовы и затраты; все потери; предел product-caller. Обязательная таблица:

| Стадия | A0 | A1 | A2 | B0 | B1 | B2 |
|---|---|---|---|---|---|---|
| Размер пар / 4 raw ответа | не измерено | не измерено | не измерено | не измерено | не измерено | не измерено |
| Non-null /4; Loop /4; retention относительно Q1 | — | — | — | — | — | — |

Отдельно T1/T2 A/B: accepted, methods/pairs, actual Train, wall ms, CPU ms, allocated bytes, whole-process peak и дельты. GOOD-образец: «B сохранил 4 ответа после недостаточного набора, но новый набор отклонил; расходы показаны». BAD: «B выучил новые четыре метода», когда Train не вызывался, или «kNN восстановил LoRA».

После записи — один readback фактов/ссылок/кодировки; без Mermaid/render/ручных hashes/Git/нового run. Кандидат ≤120 слов: admission перед `TrainMethodKnn` с тем же pair predicate; продукт пока не менять. При правке текста: «уточнён отчёт, данные и опыт не менялись».

> **Промпт исполнителю.** Выполни только назначенный индексом KH-EXT-01: копии и KHE host выше, одинаковые T1/T2/Q и k5. Единственное изменение B — eligible-pairs admission перед Clear. Настоящее T1, исходный T2 update A и реальный отказ B, Q0/Q1/Q2; всего четыре входа, ожидаемо три native Train и 24 query. Не заменяй опыт арифметикой пар. Покажи сохранность, отказ нового материала и затраты; любой полезный измеренный эффект успешен по своей оси, accuracy рост не обязателен. Модель по умолчанию; product/CI не трогать; короткий русский отчёт без hashes. Прямой переход/стоп немедленно прекращает дополнительные действия.

Источник: [полная helper-карточка](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps/method-memory-card.md>), prior art LT-N02 указан в ней. 20.10.2026 прочитаны helper целиком, `MethodKnn.cs` целиком и названные facade/dependency участки. Компиляция и native состояние не проверялись.
