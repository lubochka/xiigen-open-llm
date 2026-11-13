# Перенос новых результатов в native tests

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->


Дата: 23.10.2026. Это план, не разрешение на исполнение. База чтения: `ff6ed114e49f275da50891b8dd13e9d9288a8692`. Новые tests/helpers ниже ещё не созданы. Исторические результаты не являются результатами текущего продукта.

## Общая часть уже существует

Использовать [прежний контракт, §1–9](../fixes-based-on-experiments/03-test-and-execution-contract.md) и его предпосылку [02§10](../fixes-based-on-experiments/02-learning-state-and-fixtures.md#native-build-prerequisite). Здесь добавляются только новые PlanId и их матрицы. Не переписывать старый пакет и не запускать его очередь. Два последовательных этапа в `C:/Projects/llm_mvp_core`: A до product diff, B после него; один checkout и ветка. `ARM` — provenance, не выключатель нового алгоритма. Копии алгоритма из эксперимента — источник сравнения, а не реализация продукта в тесте.

При будущем разрешении реализации: сначала common harness/fixtures и capture A, затем ровно factor выбранного ID, затем capture B и сравнение. Actual source SHA, hashes fixtures и загруженных native DLL входят в receipts. Completed A используется повторно; ради сломанного форматтера, перегрузки сохранения или чтения receipt не повторять обучение. Неподтверждённый source/binary mismatch — конкретный setup defect, не отрицательное обучение.

## Точные адреса новых родительских планов

Namespace новых классов: `LlmFromScratch.Tests.ExperimentFixes`. Файл каждого класса — `Tests/ExperimentFixes/<Class>.cs`. Fixture root — `Tests/Fixtures/ExperimentFixes/v1/<Fixture>/`. Сначала перенести указанные замороженные входы побайтно, записать их SHA256 и source path в `manifest.json`; не генерировать иной набор под желаемый ответ.

| PlanId | Class | Fixture |
|---|---|---|
| FIX-KR | RoutingLearnedKeyTransferTests | RoutingLearnedKey01 |
| FIX-FSR | FileStaticReceiverTransferTests | FileStaticReceiver01 |
| FIX-PQ | PosQuerySetTransferTests | PosQuerySetOnce01 |
| FIX-JO | TranslationJsonOptionsTransferTests | JsonOptionsReuse01 |
| FIX-IDX | TranslationIdentityIndexTransferTests | IdentityTrainIndex01 |
| FIX-QVA | QualityTermApostropheTransferTests | QualityTermApostrophe01 |
| FIX-HNQ | HnswQueryNormTransferTests | HnswQueryNorm01 |
| FIX-HCL | HierarchicalLocalHistoryTransferTests | HierarchicalLocalHistory01 |
| FIX-SVD | SvdDirectedTargetTransferTests | SvdDirectedTarget01 |
| FIX-IL | SftIngressLengthTransferTests | SftIngressLength01 |

У каждого класса два основных Fact: `CaptureLearning` (ровно назначенная учебная матрица) и `CompareCompletedReceipts` (B-only, 0 Train/Forward/Generate). У FIX-IDX дополнительно один B-only `RepeatedIdentityStillLearns` с отдельным бюджетом из 52. Небольшие проверки равенств/контрактов из соответствующего плана выполняются на уже захваченных состояниях внутри capture, а assertions сравнения читают raw. Новые самостоятельные fit ради каждой assertion запрещены. Классы нейронного [плана 41](41-neural-learning-resource-deltas.md) имеют собственную матрицу и бюджет.

Исключения перечислены явно: у [FIX-IL, 55](55-supervised-ingress-length.md) три дополнительных B-only Fact `AdmissionContracts`, `AllDeferredDoesNotPublish`, `NativeAdmissionWiring`, без обучения. Native capture использует frozen test fixture и настоящий IncrementalTrainer: A ожидаемо отказывает без updates, B делает200. Продуктовый SFT admission и его wiring требуют prerequisite FIX-MC; исследовательский donor не превращается в canonical parent. Для [дополнения FIX-N63C, 54](54-correction-collision-measured-followup.md) действует его собственная матрица старого ID, без второго назначения прежнего17. Новые evidence receipts не дают права автоматически реализовать prerequisites или запускать их измерения.

## Команды будущего исполнения

Выбранный ID задаёт один output: `C:/Projects/llm_mvp_core/artifacts/fbe/<PlanId>/attempt-0001/`, внутри `learning/A`, `learning/B`, `contracts/B`, `report.ru.md`. Планируемый существующим пакетом `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` расширить указанными ID, а не создавать отдельный launcher на каждый опыт. Он присваивает test-only `XIIGEN_FBE_SOURCE_ROOT`, `XIIGEN_FBE_OUTPUT`, `XIIGEN_FBE_ARM`, `XIIGEN_FBE_PAIR_ROOT`; явно связывает baseline SHA и candidate SHA, пишет immutable intent/stage/binding по старому §9.

В каждой стадии — один Rebuild выбранного Tests проекта, затем discovery и один capture выбранного класса. Пример **FIX-KR** (для другого ID подстановка однозначно из таблицы, а не поиск исполнителем):

~~~powershell
Set-Location -LiteralPath 'C:/Projects/llm_mvp_core'
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -t:Rebuild
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --list-tests --filter "FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.RoutingLearnedKeyTransferTests.CaptureLearning"
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.RoutingLearnedKeyTransferTests.CaptureLearning" --logger "trx;LogFileName=capture.trx" --results-directory artifacts/fbe/FIX-KR/attempt-0001/learning/A
~~~

После product diff/Rebuild B команда capture та же, output `learning/B`; затем ровно один `CompareCompletedReceipts`, output `contracts/B`. Discovery должен найти 1 назначенный Fact, ноль — ошибка. Это команды для будущего harness-контекста с назначенными переменными, не готовая команда запуска ещё не созданного теста. Rebuild ≤15 минут; capture ≤5 минут (если план 41 задаёт иной предел, действует его предел); сравнение ≤1 минуты. Deadline — повод проверить состояние своего handle, а не объявить процесс убитым или немедленно перезапустить.

## Как чинить падающие tests

Свежих падений не измеряли: builds/tests в этом исследовании не запускались. Перечисленные в планах существующие методы — адреса анализа последствий, не выдуманный список красных тестов.

1. Зафиксировать failing assertion, actual output и native assembly. Отделить compile/API break, неправильный прежний expected, разницу обученного состояния и реальную регрессию. Архивный host не заменяет продуктовую сборку.
2. Стабильные контракты (authority, tie order, instance null guard, checkpoint schema, held-out) сохраняются. При их падении исправлять factor, не снижать порог.
3. Expected, буквально закрепляющий исправляемую ошибку, менять только на независимый gold из указанной fixture, сохраняя старый пример и рядом контроль. Не брать expected из фактического B.
4. Если helper потерял setup обучения, восстановить выбранный совместимый snapshot. Для in-memory learner без Save/Load выполнить единственный назначенный fit; JSON массива не называть поддержанным restore. Не добавлять «1000 шагов на всякий случай».
5. Известные регрессии QVA/FSR/IDX/HNQ показывать рядом с выигрышем; не ослаблять старые checks, чтобы скрыть их. Узкая полезность даёт кандидат, не общий production admission.
6. Существующие guards, перечисленные в плане, запускать отдельно от ресурсного capture после B, точным методом. Их собственные Train/Execute счётчики выделяются в `guards-budget.json`; не включать их в A/B стоимость. Если существующий guard делает обучение, переиспользовать его однократно и считать расход, а не добавлять такую же новую проверку.

Назначенные дополнительные проверки после B (методы названы в50–53; ни одна строка не запускает весь test class):

| ID | Дополнительный расход вне основной пары |
|---|---|
| FIX-KR | 3 существующих guards: 0 Train, 3 ScoreAsync |
| FIX-FSR | 3 существующих guards: 0 Train, 3 Label и 1 FeatureVector |
| FIX-PQ | 3 существующих rule guards: 0 Train, 3 Predict пустой памяти |
| FIX-JO | 1 checkpoint guard: 1 batch/1 lesson, 1 translation flow; save/load без повторного fit |
| FIX-IDX | Continuation guard: 2 batch/2 lesson/5 flow; новый RepeatedIdentityStillLearns: 1 restore/1 batch/1 lesson/1 flow |
| FIX-QVA | 1 scope guard: 1 TrainOnExample, 1 Verify |
| FIX-HNQ | 2 edge guards: 0 learner Train, 1 Add, 2 Search; save/load сравнивает capture bytes без fit |
| FIX-HCL | TooFewTokens: 0 fit; SmallVocab: 1 flat Bigram Train на240tokens и59Predict до отказа vocabulary guard. Три полноразмерных успешных E05 Run здесь не назначены |
| FIX-SVD | Существующего directed guard не найдено; новые matrix assertions читают уже захваченные factors, 0 дополнительных fit |

Runner собирает FQN filter только из этих методов в одном отдельном guards-процессе после B, output `artifacts/fbe/<ID>/attempt-0001/guards/existing`. Новый IDX guard имеет отдельный output из52. Если текущий исходник guard изменил число обучающих вызовов относительно этой таблицы, сначала сверить конкретный diff и обновить bounded work order; не скрывать дополнительные вызовы в setup. Build/discovery и обычный разбор JSON не выдаются за обучение. У41 собственная таблица contract/learning вызовов.

## Обязательный промпт исполнителю каждого ID

Применяется ко всем разделам пакета, включая 41. Реализация начинается только после прямого разрешения пользователя выбранного PlanId. Модель исполнителя — текущая модель сессии по умолчанию; не выбирать другую. Читать конкретный план и этот контракт, а также `C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `output-readability-gate-SKILL.md` и `human-communication-GLOSSARY.md` из той же папки. Точные paths/fixtures/tests уже выбраны; неожиданный конфликт разбирать по этим правилам, не создавать альтернативный эксперимент.

До продолжения своего запуска один раз проверить его handle/terminal state. Если работает — штатное блокирующее ожидание того же handle; искать доступный wait/status tool по документации среды. Для Codex `exec_command` с session_id продолжать через `write_stdin`, для yielded `functions.exec` — `functions.wait` с cell_id. Не смешивать ID разных инструментов. Если обычный инструмент не блокирует, использовать реально доступный bounded wait, сверив elapsed/статус, а не повторять пустой вызов. Никаких собственных фоновых loop-monitor, polling одного файла, дублирующего waiter или выдуманной работы во время ожидания. Не заканчивать задачу фразой «жду уведомления». Completion проверяется по exit/receipt того же процесса. При новом ходе сначала проверить, не завершился ли он.

STOP немедленно прекращает прежнюю задачу; NEXT переключает на явно названный ID, не на старый номер строки. Не дописывать прежние reports/skills/manifest после прямого переключения. Исторические outputs не удалять. Сохранённое обученное состояние после formatter/API-save ошибки использовать автоматически: исправить только запись/чтение и закончить отчёт без выбора «обучать заново или сохранить».

Отчёт на русском: тема и получатель → A до/после и B до/после → полезные оси → все ухудшения → фактический Train/readout budget → source/state/raw links → кандидат и границы. 0→0 не измерение успешного обучения; частичные правильные токены явно назвать частичными. Любой измеренный полезный сдвиг считается успехом своей оси, но единичное время не доказывает устойчивой причинности. Отчёт/commit не заменяет завершение разрешённого выбранного переноса.
