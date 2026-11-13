# IDENTITY-TRAIN-INDEX-01 — индекс identities внутри настоящего обучения

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница TRANSLATION:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/IDX01/a001/results/report.md>).
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

**DESIGNED_NOT_RUN.** Новый фактор поступления dd150aa, после срочного ремонта parent/№08 и остальных прямых учебных опытов, перед IDENTITY-M1-01 и KN-ROUTER-LEARNED-KEY-01. Модель исполнителя/проверяющего — default текущей сессии без override. Learner — native TranslationMachine, обучаемая EN_RU memory/kNN, не нейронный Student. Текущая фаза READ_ONLY_RESEARCH: host, сборка, обучение и Flow не запускались.

## Один фактор и точный результат

В B заменить только линейный поиск пары `(ExampleId,UnitId)` в журнале `_structuredTrainingIdentities` на вспомогательный `HashSet<(string ExampleId,string UnitId)>`. Три изменения в source copy TranslationMachine.cs: добавить readonly set; заменить условие Any после `trainer.TrainOnExample` на `set.Add((example.ExampleId,example.UnitId))`; при ImportCheckpoint очищать/восстанавливать set вместе с прежним ordered list. List, его сортировка/экспорт и формат checkpoint остаются прежними; tuple default string equality ordinal. Добавление identity происходит **после реального TrainOnExample**, не превращается в admission/dedup, не пропускает повторное обучение. Если точных трёх sites нет — SOURCE_DRIFT, без другого patch.

## Место и внешний entry

H=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/IDX01`; будущие authored файлы H/host/{run.py,prepare.py,Program.cs,compare.py}; work=H/a001; R=H/a001/results. Work содержит A/B проекты и source copies, inputs и deps. Не использовать C:/xgi или C:/Rp01. Единственная внешняя команда после прямого разрешения Любы на этот ID/программу:

```powershell
& 'C:/Python314/python.exe' 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/IDX01/host/run.py'
```

Это спецификация будущего entry, не уже готовая CLI. run.py первым exclusive-create H/execution.claim, до prepare/build/native. При existing claim/work/results — ALREADY_ATTEMPTED, не удалить и не выбрать a002. Prepare один раз создаёт work/R; процессы W,A,B проверяют root owner nonce и **до первого native constructor/load/Train** создают FileMode.CreateNew R/<W|A|B>/phase.claim. Directory.CreateDirectory не считается exclusive claim; existing phase claim прекращает процесс без native calls. Последовательно prepare30s → buildA120s → buildB120s → W120s → A60s → B60s → compare30s. Одна попытка,0retry, timeout останавливает только собственный child process и сохраняет частичные данные. Не запускать solution build/tests, старый P125, SDK/package install.

## Закрытый состав и pin

Pin=`dd150aa5216e42b8a8bba14955337398ec5f8397`; source root `C:/Projects/llm_mvp_core/27.TranslationDomain`. Девять файлов и Git blobs:

| Файл | blob |
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

Read pinned git blob и сравнить с рабочими bytes; несовпадение → SOURCE_DRIFT, не обновлять pin исполнителем. A получает9неизменённых копий work/A/src, B — те же work/B/src плюс ровно указанные3замены в TranslationMachine.cs. Future one-factor.diff должен содержать только эти sites. Бинарные зависимости: закрытый на prepare inventory `C:/Projects/llm_mvp_core/27.TranslationDomain/bin/Release/net9.0/*.dll`, исключить27.TranslationDomain.dll (его owner — source copy); записать полный список/hash, копировать в work/deps. Нет DLL → DEPENDENCY_MISSING; не пересобирать продукт. Эта binary boundary не была загружена данной сессией.

A/B/Runner.csproj: net9.0, OutputTypeExe, Nullable/ImplicitUsings enable, EnableDefaultCompileItems=false, явные9Compile плюс общий work/Program.cs, явныеRefs по frozen deps. ImportDirectoryBuildProps/Targets=false до явных Sdk.props/Sdk.targets imports. Встроенные команды wrapper: `dotnet build work/A/Runner.csproj -c Release --nologo`, затем B; `dotnet work/A/bin/Release/net9.0/Runner.dll --mode W --out R/W`, затем Aс--modeA/R/A, затем Bс--modeB/R/B. Подставить только назначенные абсолютные work/R. Program.cs имеет эти3режима; не читать произвольные пути/новый корпус.

## Данные, warm и настоящее обновление

Источник frozen данных: `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/`.

| Файл | blob |
|---|---|
| training-batch-B.json | 0c2a3d9a0da31b7277c0d215fb79ddebda49aa6c |
| eval-inputs.json | 585bf30cad84b57b2e1f3fe8c00f171f2348c313 |
| panel-gold.json | af73d89cfd5b42f954380437bc5596b46dae9498 |
| score.cjs | e8b20d471667da397e8d5c59ac63b4e7f947540b |

Восемь raw rows tr01..08: validate file/delete cache/copy report/move config/upload image/download package/save draft/archive log. Deserialize frozen `TranslationLearningBatch` напрямую; сохранить InputJson/ChosenOutputJson/RejectedOutputJson, UnitIdexecutor-en_ru, SourceIdsimulation, SemanticSignatureSVO,HeldOutfalse. Не брать старую функцию P125 Train, которая генерирует свой пакет: это другой input. Eval24rows и gold learner не изменяет; gold используется только после завершения rawoutputs.

W: новый native TranslationMachine,128пакетов по4первых строки tr01..04. g=0..127,j=0..3,r=4*g+j; ExampleId=`IDX01-W{r:D4}::{originalExampleId}`, BatchId=`IDENTITY-TRAIN-INDEX-01-warm-{g:D3}`. Все остальные поля без изменений. Это512предъявлений/512identity, но4независимых содержания. Каждый пакет через настоящий TrainStructuredBatchAsync, не вставлять identities в checkpoint вручную. Ожидаются128history,512identities,4exact entries,8phrase entries,1536KNN entries. Реальное несовпадение сохранить как INVALID_FIXTURE; не подделывать счётчики. SaveCheckpointAsync в R/W/warm.checkpoint.json. W не вызывает Flow.

A/B: новый процесс и native LoadCheckpointAsync одного R/W/warm.checkpoint.json;24before Flow; один TrainStructuredBatchAsync только tr05..08 с исходными ExampleId и BatchId=`IDENTITY-TRAIN-INDEX-01-new04`;24after Flow. Ожидаемые after:129history,516identities,8exact,16phrase,1548KNN. Это ожидаемые числа, не результаты. Проверить PromotedUnitIds ровно executor-en_ru и изменение native memory: RowsTrained=4 само по себе недостаточно (оно может считать непроученную строку).

Общий потолок130Train calls,520row updates,96Flow,2restore,1warm save,0DPO/neuralTrain/Generate/teacher/retries. Warm counted separately; не скрывать512updates в «подготовке». Порядок W→A→B фиксирован, потому resource deltas описываются как наблюдения, не доказанный causal speedup.

## Program и scorer без недостающих решений

Program использует тот же ordinary consumer, что `P125-enru-memory-01/harness/Program.cs:66–88`: GetOrchestratorAsync, LogicalOrchestratorRequest по frozen source/source_language/target_language/topic_domain, RunAsync. RequestId=`IDX01-{mode}-{phase}-{id}`, остальные поля/flags из donor Evaluate. Только Evaluate/Snapshot patterns; Train-function donor не копировать. Raw каждой строки: arm,phase,id,bucket,source,success,error,errorCode,actual,verdict,selectedFlow,adapterId,metrics,warnings,elapsedMs.

Canonical checkpoint сравнивается до/после eval и A/B: исключить только ExportedAtUtc и TrainingHistory[].CpuMs/PeakRamMb/ElapsedMs; EN_RU memory+sharedKnnStore должны меняться от Train и сохраняться от eval; frozen-other digest (SemanticTokenizer, QualityVerifier, прочие executors) неизменен. Restore/Train wall,processCPU,GC,allocations раздельно; thread allocation только с same-managed-thread proof, peak отдельно без сложения пиков.

compare.py после successful A/B с valid receipts собирает ровно96строк в R/raw-outputs.json; копирует R/eval-inputs.json, R/panel-gold.json. Только тогда создаёт completion.json, который требует frozen scorer. Scorer — top-level script с __dirname=R, **не импортируемая функция**: сохранить R/score.cjs с двумя и только двумя replacements `['A','B','C']`→`['A','B']`; assess/gold не менять. Вызвать `C:/Program Files/nodejs/node.exe R/score.cjs` отдельными argv с timeout30s внутри compare. Выходы R/scores.json и R/score-summary.json. Если native process не завершён, completion не создавать и scorer не запускать.

Показать all/exact/near/composition/edge отдельно и raw/gold пары. В before должны быть4правильных старых exact x01–x04; иначе INVALID_LEARNING_FIXTURE. After x05–x08 проверяют приобретение нового знания. A/B качество и canonical state должны совпадать; выигрыш allocated/time при эквивалентном обучении — допустимый полезный результат.0→0 по новой задаче не выдавать за освоение. Более быстрый журнал не доказывает качество других языков.

## Выход, STOP и NEXT

R содержит manifest.json, one-factor.diff, W/A/Breceipts, raw-outputs.json, scores.json, score-summary.json, comparison.json, report.md. Русский report≤350слов+таблицы; полезный measured результат одной записью≤120слов в C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/required-changes.md. Source pin/byte comparison — inputguard, не повод бесконечно переоформлять hashes.

Одиночное поручение: NEXT=END. Порученная программа: next_id — [SFT-EXP-REUSE-01](SFT-EXP-REUSE-01.md). «Следующий»/«остановить всё» немедленно останавливают отчёт/review/hash/Git/retry. Статус DESIGNED_NOT_RUN не запрещает отдельно разрешённую реализацию experimentalhost, но эта карточка не разрешает её сама. Продукт и старые raw не менять. Навыки: C:/Projects/llm_mvp_core/docs/skills/experiment-design-SKILL.md, generated-code-review-SKILL.md, human-readable-architecture-audit-SKILL.md, human-communication-GLOSSARY.md. [Промпт](../prompts/IDENTITY-TRAIN-INDEX-01-executor.md).
