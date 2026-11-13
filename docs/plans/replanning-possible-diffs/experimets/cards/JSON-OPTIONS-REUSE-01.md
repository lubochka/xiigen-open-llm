# JSON-OPTIONS-REUSE-01 — стоимость десериализации в настоящем обучении Executor

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница TRANSLATION:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold.

**Исходный адрес из паспорта:** `27.TranslationDomain/TranslationTrainedUnits.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/result/report.md>).
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

**Статус:** `PLANNED_NOT_RUN / SOURCE_COPY_HOST_AUTHORED_UNBUILT`. Это новая узкая cost-гипотеза семейства P125. В этом документном аудите обучение и сборка не выполняются. Полный source-copy host задан в [приложении](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/28-json-options-host-contract.md>); он написан, но ещё не собран и не проверен native запуском. Домен: **Перевод EN→RU, technical, синтетический существующий fixture P125**.

## 1. Вопрос и факт в коде

Можно ли уменьшить выделение managed memory на **весь реальный batch8 обучения**, повторно используя неизменные `JsonSerializerOptions` только в `ExecutorUnit.TrainOnExample`, при том же выученном состоянии и тех же обычных ответах?

В [TranslationTrainedUnits.cs:404–455](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:404>) строка408 создаёт новый options и converter на каждый урок. Затем метод десериализует `ExecutorInput/ExecutorOutput` и пишет exact-source, phrase/word maps и общий KNN. [TrainStructuredBatchAsync:107–145](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:107>) вызывает метод для каждого обучающего примера.

Полезный эффект пока **гипотеза**. Не измерены ни экономия памяти, ни ускорение. Этот фактор не объявлен доказанной причиной снижения semantic quality RP01.

**Новизна относительно корпуса:** в799planningMD+339protectedMD на зафиксированных снимках найдено использование JsonSerializerOptions в сопутствующих snippets, но не сравнение его lifetime внутри Executor training. Это не N62: тот меняет training context другого learner. На более позднем core `827a1acdbef95d220886e54f9d36bce0b405c8cd` появились новые docs; эта карточка сохраняет воспроизводимый code/data pin669 и не присваивает новые результаты его старому снимку.

## 2. Получатель обучения и проверяемая цепочка

`existing batch8 → TranslationMachine.TrainStructuredBatchAsync → ExecutorUnit.TrainOnExample → exact/phrase/shared-KNN state → GetOrchestratorAsync().RunAsync → TranslationRoutingFlow → TrainedTranslationAdapter → learned tokenizer / executor / QualityVerifier.Verify → обычный output/verdict`.

[Обычный consumer, TranslationDirectionAdapters.cs:117–142](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationDirectionAdapters.cs:117>). Измеряется обучение настоящей памяти и её применение; микробенчмарк одного JsonSerializer отдельно от обучения сюда не допускается. Neural optimizer отсутствует: neural updates0, neural layers N/A, external model calls0. По исходнику одна машина содержит8Units и1Flow; обучается только `executor-en_ru`.

## 3. Единственное различие A/B

| Поле | A | B |
|---|---|---|
| Код | Девять закреплённых исходников без изменений | Те же девять исходников; один метод использует повторные options |
| Train data / IDs / order | Исходный `training-batch-B.json`,8уроков | Тот же файл побайтно |
| Изначальная память | Fresh TranslationMachine, не checkpoint | То же |
| Обучение | Один TrainStructuredBatchAsync(batch8) | Один такой же вызов |
| Evaluation | Одна фиксированная24-case панель до и после | Та же |
| Изменённый участок | `var opts = new JsonSerializerOptions ...` | `var opts = TrainingJsonPolicy.Options;` |
| Остальная логика | Без изменений | Без изменений |

В B добавляется **только внутри ExecutorUnit** приватный holder:

```csharp
private static class TrainingJsonPolicy
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
    static TrainingJsonPolicy() { }
}
```

Явный static constructor исключает `beforefieldinit`: создание options приходится на первое обращение **внутри первого измеряемого TrainOnExample**, а не переносится в setup/warmup. Конфигурация, JSON DTO, converters, данные и порядок уроков одинаковы. Ни tokenizer/verifier TrainOnExample, ни MeasureAccuracy, ни builder не меняются. После первого использования options не модифицируются. Никакого cache текстов, десериализованных уроков или ответов модели нет.

**Конкретный будущий ремонт оригинала:** вынести lifetime options в этот приватный holder и заменить только строку408 `ExecutorUnit.TrainOnExample`; production изменение рассматривается пользователем после результата. Полный pipeline, тесты продукта и CI сейчас не исправляются.

## 4. Точное место, источники и границы изоляции

Авторские файлы из приложения сохраняются исполнителем без изменений:

`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/prepare.py`  
`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/Program.cs`  
`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/compare.py`

`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/run.py` — готовая авторская обвязка всех шести шагов; ещё не исполнялась.

**Короткий execution root:** `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001`. Компиляция A/B, bin/obj, dependency copies, data и results находятся только там. Не использовать длинный protected result-path в имени компилируемого проекта. При существующем `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001` → `BLOCKED_EXISTING_OUTPUT`: сохранить содержимое и передать точный путь; не удалять старый опыт и не подбирать самостоятельно другой ID.

**Read-only inputs:** core git objects669 и уже существующий `C:/Projects/llm_mvp_core/27.TranslationDomain/bin/Release/net9.0`. Из protected читаются pinned inputs; при обычном завершении полезного опыта разрешена назначенная запись в required-changes.md. Checkout, production source, tests, csproj проекта35 и его dispatcher не изменяются.

### Девять фактически компилируемых исходников

Все пути относительны `27.TranslationDomain/`, commit669. `prepare.py` читает их через `git show`, сверяет Git blob SHA и фиксирует SHA256:

| Файл | Git blob SHA |
|---|---|
| TranslationMachine.cs | fd0e736de3739236b4dde2d6b9d000f5ebae8416 |
| TranslationTrainedUnits.cs | 1742abfc9fb617dfc66e4d618e0c169da01a6ccc |
| TranslationUnitContracts.cs | 0f88d0d9cc3a62d2b2f8b0b9321142a4a4c1b0d8 |
| TranslationContracts.cs | 9ccff62783661fab24131e206b42280fe4cdc063 |
| TranslationCheckpointContracts.cs | 3879805b0504df7718adb2760e86aa6b4c3b13ab |
| TranslationDirectionAdapters.cs | ce139d39cfab36b7e919c31398cacde225b952d5 |
| TranslationRoutingFlow.cs | ae9da139a68b9e6d6cfd655425b0327053e06f0d |
| TranslationSemanticTokenizer.cs | ee6d81c7933fa33a9c57a2111990019ec55e5dab |
| TranslationMemorySourceLaw.cs | ba841f4f1839d504444d214feb73c80a4cc24f32 |

Compile items: ровно эти9копий + один общий Program.cs. Reference: копии уже существующих dependency DLL из указанного Release-каталога, **исключая production27.TranslationDomain.dll**, поскольку domain source компилируется в экспериментальный exe. ProjectReference на продукт, проект35 или12 отсутствует. `ImportDirectoryBuildProps/Targets=false` выставлены до SDK imports. A/B имеют одинаковые build settings и отдельные процессы.

Manifest сохраняет sourceCommit, sourceblob/SHA256, data blobs/SHA256, native original paths/SHA256, candidate source SHA256, все четыре host-файла, два project-файла, scorer и one-factor.diff. Перед первым обучением native host проверяет DLL/PDB identity исходной27assembly и PDB checksums всех9baseline source files; отдельно проверяет собственный `JsonOptionsA/B` assembly, его PDB и checksums9исходников+Program. SHA фактических bin dependencies и уже загруженных assemblies сравниваются с manifest до TRAIN, после TRAIN и после evaluation, вне измеренного окна. Production27assembly загружаться не должна. Полная source provenance всех transitive dependencies не заявляется.

Если существующий baseline DLL/PDB отсутствует, не соответствует9исходникам или dependency copy изменена — `BLOCKED_ENVIRONMENT`, **zero additional training**. Не собирать весь продукт и не чинить тесты для обхода блокировки. Сохранить preflight evidence и перейти по NEXT. Этот статус не означает неудачу механизма обучения.

## 5. Данные и независимое оценивание

Источник всех входов:

`docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/` на commit669:

- [training-batch-B.json](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/training-batch-B.json>) — ровно8готовых native lessons, `sourceId=simulation`, все `executor-en_ru`, не heldout.
- [eval-inputs.json](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/eval-inputs.json>) — вся прежняя панель24без gold:8exact,3near,8composition,5edge.
- [panel-gold.json](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/panel-gold.json>) — frozen accepted phrases/rubric.
- [score.cjs](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/score.cjs>) — frozen assess().

Единственное изменение экспериментальной копии scorer: два списка плеч `['A','B','C']` становятся `['A','B']`, поскольку здесь оба плеча обучаются и третьего нет. Rubric, normalize, assess() и24cases не меняются. Gold читается только scorer после сохранения всех96native outputs; native host его не открывает. Никаких новых DPO вызовов: исходные уроки честно названы синтетическими, а не приписаны внешней модели.

Исторический P125 показал19/24 после8уроков, включая3ошибочно принятых edge-ответа; [первичный отчёт](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md:1>). Это baseline evidence, а не ожидаемый обязательный результат нового запуска. При новых числах записать именно новые outputs.

## 6. Бюджет и команда запуска

**Только после назначения этой строки реестра на выполнение.** Текущее задание — документация, не запуск. Модель исполнителя — текущая модель его сессии по умолчанию, без override; не путать её с памятью Student. Минимальный host, простые DTO, exceptions и try/catch допустимы для эксперимента.

Максимум:2малые сборки isolated host;2native process starts, A затем B;2batch8=16memory lessons;96ordinary outputs=2arms×2phases×24;0neural steps;0external model calls;0cold-load;0warmup/reverse order/repeat. Один scorer и один comparison. Не запускать B после неуспешного A; не считать incomplete run полноценным A/B.

Авторский `run.py` выполняет подготовку, build A, build B, A, B, comparison строго последовательно. **Единственная команда:**

```powershell
C:/Python314/python.exe C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/run.py
```

Используется уже известный `C:/Python314/python.exe`; нет поиска другого Python. Если этот executable/Windows, git, dotnet9 или Node недоступны — `BLOCKED_ENVIRONMENT`, без установки и обхода. Wrapper не допускает существующий execution root или `host/attempt-jo1`. Последний — точно назначенный staging-каталог только для логов подготовки, не для компиляции.

Обвязка записывает stdout/stderr и exit каждого из6шагов в `host/attempt-jo1/`, затем копирует logs и `command-receipts.json` в `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/result/`, когда этот root уже создан. Даже preparation failure сохраняет собственные stdout/stderr/receipt в staging. Поля receipt: argv, cwd, startUtc, finishUtc, exit, wallMs, timeoutSeconds, stdoutPath, stderrPath, status/error. Любой nonzero/timeout заканчивает попытку: B не запускается после failed A. Нет удаления, автоматического retry или перехода на другой host.

Внутренние команды `run.py`, отдельное ручное повторение не требуется:

```text
C:/Python314/python.exe <assigned-host>/prepare.py --repo C:/Projects/llm_mvp_core --out C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001
dotnet build C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/A/Probe.csproj -c Release --nologo
dotnet build C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/B/Probe.csproj -c Release --nologo
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/A/bin/Release/net9.0/JsonOptionsA.dll C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001 A
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/B/bin/Release/net9.0/JsonOptionsB.dll C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001 B
C:/Python314/python.exe C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/compare.py --root C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001
```

`<assigned-host>` здесь означает единственный полный host-path из единственной команды; в authored run.py placeholder отсутствует. Таймауты: prepare30s, каждый build120s, каждый native process60s, compare30s. Wrapper завершает только собственное дочернее дерево при timeout/interrupt. После прямого пользовательского stop не продолжает receipt/report; существующие artifacts остаются. При native failure число завершённых updates берётся из сохранившегося native receipt либо остаётся неизвестным, не придумывается0.

`compare.py` после96native outputs проверяет frozen data/host hashes и копии фактически читаемых gold/eval, затем сам вызывает ровно один `node C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/result/score.cjs`. Native Program открывает только training-batch и eval-inputs, **не gold**. Не запускать scorer вторично. Все сборки изолированы; source factor и бюджет не меняются.

## 7. Что именно измеряется

**Первичная ось:** `trainingThreadAllocatedBytes` — allocation между непосредственным входом/выходом одного реального `TrainStructuredBatchAsync`. Pinned implementation синхронный и возвращает completed Task; host проверяет completed + sameManagedThread. Это managed allocation, не RAM peak.

Рядом: `trainingProcessAllocatedBytes`, training wall и process CPU deltas, GC0/1/2, общий lifetime allocated/process peak, source/checkpoint bytes, правильные/принятые/ошибочно принятые outputs и все rubric errors. Цена подготовки и build отдельно от цены обучения. VRAM и neural layers N/A; стоимость модели-исполнителя, whole-PC/group memory не измеряется и не выдаётся за0.

Для A/B сохранить initial/before/trained/after checkpoints. Сравнить:

- A_initial=B_initial; A_trained=B_trained по каноническому state hash.
- before→trained меняется memory state в обоих плечах; RowsTrained сам по себе не доказательство.
- `exact/phrase/word/surface/KNN` counts и contents; frozen tokenizer/verifier/other directions.
- Ordinary queries не меняют полный canonical state; memory hash дополнительно сохранён отдельно. After output/route/verdict/confidence/metrics/warnings A/B совпадают либо различия выписаны.
- В обоих плечах тот же Verify и Translate consumer реально вызван ordinary Flow; не заменять ответы gold.

Из canonical state equality исключены **только** `ExportedAtUtc` и operational fields `TrainingHistory[].CpuMs/PeakRamMb/ElapsedMs`. Все исходные checkpoints остаются без вырезания этих полей; runtime costs сравниваются отдельно. Batch IDs, learned entries, version, readiness, history learning fields и identities сохраняются.

Comparator пишет A0,A1,B0,B1,ΔA,ΔB,ΔB−ΔA по каждому bucket/quality metric; неопределённые значения остаются null. `exactOptimizationSupported` требует initial/trained equality, реальной записи памяти, неизменных остальных owners, полного state без query mutations и output parity. При нарушении causal attribution=INVALID_COMPARISON, но наблюдаемые полезные deltas сохраняются. При валидном контрасте allocation attribution ограничен этой парой; wall/CPU/GC attribution=NOT_ESTABLISHED.

В wall/CPU одна холодная точка на плечо; наблюдаемый выигрыш честно записать как локальный полезный delta, **causal attribution=NOT_ESTABLISHED**. Не объяснять заранее JIT/порядком/шумом и не назначать повторы для признания локального результата.

## 8. Успех, ухудшения и кандидат в исправления

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и этому домену. Рост accuracy не обязателен; нет порога5%, требования статистической значимости или дополнительного прогона.** Все изменения остальных метрик, включая ухудшения, показываются рядом. Если состояние/outputs различаются, не скрывать выигрыш cost, но отдельно отметить semantic regression/confounding и не называть factor semantics-preserving.

Не подменять физический выигрыш изменением счётчика. При нулевом измеренном выигрыше `NO_MEASURED_GAIN`; при environment/failure `PARTIAL/BLOCKED`, не «обучение хуже».

Один русский `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/result/report.md`: вопрос; original method/patch; реальные команды и код SHA; данные/домен; actual learning state before/after;24ответа с gold для каждого плеча/фазы (таблица или linkedraw + ключевые differences); все cost/quality deltas и gains/losses; ограничения; точный original fix; решение `CANDIDATE_ONLY_USER_DECISION`. Если есть полезный измеренный delta, добавить `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001/result/CANDIDATE.md` до120слов с метрикой, domain, ухудшениями и ссылкой на report. **В required-changes.md добавить одну запись кандидата со ссылкой на report; production fix остаётся решением Любы.** До выполнения эти файлы не заполнять вымышленными результатами.

Использовать skills из `docs/skills/`: intent-to-plan; plan-self-validation; generated-code-review; trainable-architecture-decision; experiment-design; prompt-patch-authoring; human-readable-architecture-audit и human-communication-GLOSSARY. Роль этих skills — точный scope, source→state→consumer, понятный русский отчёт. Старые product gates не требуют полный lifecycle для экспериментальной копии.

## 9. Порядок и немедленное переключение

Место в основном реестре: после blockers настоящего обучения и разрешения конфликтов в уже назначенных карточках; до unrelated инфраструктурных проверок. Эта строка не запускается при текущем документном аудите. В полной назначенной программе [этого реестра](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md>) next_id для JSON-OPTIONS-REUSE-01 — [DPO-SHIFTED-CONTEXT-01](DPO-SHIFTED-CONTEXT-01.md). Исполнитель не выбирает следующий опыт. При поручении только JSON-OPTIONS-REUSE-01 завершить один этот ID: `NEXT=END`, не запускать следующий самостоятельно.

Исторический инцидент зафиксирован словами пользователя: «четыре раза вместо переходя к следущему эксперименту продолжаешь прошлую задачу!!!» и «Закончить немедленно!!!!!!». Это свидетельство прошлого нарушения, не текущий stop документного аудита.

**Запрет обязателен и для этой карточки, и для каждого её prompt.** После «заканчивай и переходи к следующему» немедленно прекратить прошлый опыт: не дописывать report/review, не повторять запуск, не чинить тесты, не коммитить, не «улучшать» прежнюю задачу. Сразу выполнять прямо назначенный NEXT. После «остановить всё» немедленно прекратить любую работу и tool calls. Сохранённые raw/partial не удалять; их неполнота не оправдывает продолжение. Если native start упал до Train, честно trainingCalls=0; имеющиеся output files не удалять ради retry.

**Приоритет конфликтов:** текущая прямая команда пользователя → назначенная строка реестра/данная карточка → исторические общие правила. Нельзя из документного поручения вывести разрешение на runtime, а из успеха метрики — разрешение на production promotion.



**Применение в этой сессии:** только Markdown/JSON; приведённые source blocks остаются будущим заданием, реальные host/source/data/weights сейчас не создаются. Исполнитель — модель его сессии по умолчанию без override. Student указан отдельно. При «остановить всё» остановиться немедленно; при «следующий» немедленно перейти по назначенному NEXT, без report/hash/review/Git/retry прежнего опыта. При отдельном поручении одного ID — END.

При обычном завершении измеренного полезного опыта: русский report по карточке и одна запись до120слов в [required-changes.md](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/required-changes.md) со ссылкой на report, метрикой, доменом и ухудшениями. Отдельный candidate рядом с raw служит содержимым этой записи; дополнительных запусков нет. STOP/NEXT выше оформления. Все экспериментальные послабления (минимальный host, throw/try-catch/void) действуют; полный product lifecycle не требуется.

**Защита старой попытки:** C:/xi/e/jo1 — только проверка legacy существования. При наличии этого пути либо JO1/a001 либо host/attempt-jo1 сохранить всё и закончить BLOCKED_EXISTING_OUTPUT. Материализатор по четырём блокам не запускает их; единственный runtime owner — host/run.py. Он владеет staging/execution.claim до prepare/build, prepare создаёт JO1/a001/result, Program резервирует arm.claim до модели/Train, compare резервирует comparison.claim до scorer. Ручные внутренние команды запрещены: это повтор бюджета. Baseline DLL/PDB на этой машине существуют, но их runtime/PDB совпадение ещё не проверено; несовпадение даёт BLOCKED_ENVIRONMENT, без product rebuild.

**Точные навыки:** [trainable-architecture-decision-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md), [intent-to-plan-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/intent-to-plan-SKILL.md), [plan-self-validation-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/plan-self-validation-SKILL.md), [generated-code-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md), [experiment-design-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/experiment-design-SKILL.md), [prompt-patch-authoring-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md), [human-readable-architecture-audit-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md), [human-communication-GLOSSARY.md](C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md). Нового стека/модели не выбирать.
