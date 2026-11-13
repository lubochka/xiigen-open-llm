# Адресный ремонт после P128: получатель, проверка, итог

**Статус 4 ноября:** R128-01/Q01 принят по [сохранённой приёмке 29/29](../../../../qa-results/q01-concurrency-2026-11-04-20261104091631764/codex-report.md); не выдавать старый список его трёх остатков как новый ремонт. R128-02/03 и прочие владельцы имеют свои отдельные статусы. Применять [161](codex-161-postq011-executor-context-repair-2026-11-04.md): настоящая вызывающая сторона, независимый expected, сохранённый первый отказ, полный required test set и явная фаза. Текущий общий переход — Q02 по реестру.

<!-- CODEX-POST131-AUDIT:START -->
## Адресная дельта после P131

[149](codex-149-post131-transfer-and-experiment-audit-2026-11-03.md) · [150](codex-150-post131-addressed-repair-plan-2026-11-03.md) · [49](../experimets/codex-49-post131-output-binding-and-test-coverage-2026-11-03.md) уточняют именно существующие R128-03/04/06 и связанные карточки. Изменён неполный фильтр в R128-06, названы причины трёх tail failures и обязательная привязка required FQN к discovery/TRX. Для P131 добавлена проверка raw/native, для P127 — route/steps и отдельный scientific C-target control. Q130 уже RESULT_RECORDED;131/131 опытов записаны. Нет нового PRIMARY или новой очереди. Исторический source145/146 сохранён; исправления кода и нативная приёмка этим документным обновлением не объявляются выполненными.
<!-- CODEX-POST131-AUDIT:END -->

**Действующее согласование после abf83ee797b:** применять [48](../experimets/codex-48-post128-executor-verification-2026-11-03.md) вместе с этим планом. Q129 уже RESULT_RECORDED и его Say-only исправлен; четыре helper-пары приняты по сохранённому результату, новая PRIMARY не назначена. Открыты отрицательный verifier→внешний entrypoint и раздельные input/output для безопасной повторной проверки. Старые данные145/147 сохраняют свой source pin. Опубликованная сверка целевых TRX для b4 находится в [сверке передачи](evidence/2026-11-03-handover-test-reconciliation.md); это не приёмка оставшихся границ Q01. Навыки по этапам, собственные WRONG/CORRECT и типы результата заданы в48.

Дата: 3 ноября 2026. Основание — [аудит 145](codex-145-post128-transfer-and-experiment-audit-2026-11-03.md), source `f8784df037b59cf878889eef4d0956f5d0c985e7`, planning `5df24354aa803f7ed5dd3bac48146466e2feb189`. Это уточнение существующих работ, не новая очередь запусков. Продукт и первичные результаты в данном этапе не меняются.

## Порядок

Назначенный пользователем ID имеет приоритет. Если назначена вся очередь переносов без другого ID: **Q01/135-R01-CONCURRENCY (+R17) → Q02 → Q03 → Q06** по [реестру](01-registry.md#executor-next). Q04 приостановлен, Q05 ждёт его. Обозначения R128 ниже — разделы этого плана; они не подменяют стабильные ID реестра.

| Раздел | Существующий владелец | Что должно стать наблюдаемо | Зависимость |
|---|---|---|---|
| R128-01 | Q01/135-R01-CONCURRENCY +R17 | Настоящий candidate file подтверждён, конфликт и отмена не меняют победителя | Первый остаток очереди |
| R128-02 | T121-OWNER/BINDING/SHARING | Разрешение относится к фактическому получателю на всех конструкторах | Привязка проекта; отдельный T121-PERMIT не присваивать себе |
| R128-03 | P112 / перенос проверки материала | Две законные одинаковые строки не теряют IDs; selective policy соблюдается | Сначала row resolver, затем policy matrix |
| R128-04 | P128 / PHRASE-COVERAGE-CAP-01 | Байты→state→ответ→finalizer→exit образуют проверяемую цепь | Сохранённый пакет; без нового PRIMARY |
| R128-05 | P126 / SAVED-STATE-PANEL-LEARNED-02; Q03 по общему finalizer | Квалификационный отказ/отсутствующий restore не превращаются в DONE | Сохранённые 16 плеч; без ещё 14 updates |
| R128-06 | QA переноса / договор 46 | Свежие Release discovery/build/tests связаны с исходником и не меняют исторические artifacts | После изменения соответствующего source, не после одного отчёта |
| R128-07 | Q02/происхождение пары; прежние R125-04…12 | Остальные уже найденные частные проверки доведены по существующим карточкам | Прежний 143 остаётся действующим в неизменённой части |

## Общий принцип приёмки

Для каждого обязательного свойства назвать **producer → фактический объект/байты → потребитель → проверяющий предикат → общий итог → внешний exit**. Одной записи поля недостаточно. Полный честный набор сначала проходит; ровно одна порча должна пройти остальные предпосылки, достигнуть того же проверяющего и получить отказ по нужной причине. Пустое множество не может проходить через `All()` вместо проверки полноты IDs/кратности.

Три независимых вывода: `execution_validity` (процедура соблюдена), `hypothesis_result` (какую дельту показал фактор), `transfer_acceptance` (проверен рабочий продукт). Честное отсутствие выигрыша B допускает первый вывод и отрицательный второй; не допускает выдуманного продуктового улучшения. Обязательный процедурный false/missing/unknown не может сопровождаться общей готовностью.

<a id="r128-01"></a>
## R128-01. Сохранение кандидата, CAS и поздняя отмена

**Адрес:** `27.TranslationDomain/TranslationLostCandidateArtifact.cs` (`HashOfText`, оба `WriteAtomically`, `Save`), `TranslationMachine.cs` (`TryPublishVersion`, `PersistLostCandidate`, `EvaluateAtVersionAsync`), `TatoebaPilotLoopRunner.cs` и действительная composition full runner. Существующий staging/store сохраняется.

1. **Уже выполнено в [b4af9eef](https://github.com/lubochka/llm_mvp_core/commit/b4af9eef70f52020e3b10558fe86287dfb054023):** единый `Utf8BytesOf` без BOM используется HashOfText и обоими писателями; readback сохранён. Эту правку не реализовывать повторно. При последующем ремонте сохранить общий договор sync/async и фактическую проверку записанных bytes; не заменять прочитанный hash ожидаемым.
2. Сохранить в результате CAS наблюдённую им версию вместе с успехом/отказом. Позднее чтение CurrentVersion не источник победителя конкретного конфликта.
3. Между `BeforeMeasuredAccuracyPublishBarrier` и публикацией перечитать cancellation token. Отказ не меняет version, quality/history и возвращает CANCELLED согласно договору. Не удалять уже существующую late-cancel training проверку.
4. Подключить durable candidate store в реально назначенную ordinary composition для нового и восстановленного экземпляра. Явное отсутствие store назвать отдельным состоянием, не обещать locator, которого нет. Не сохранять через обычный Save победившее состояние вместо своего staged candidate.

**Приёмка оставшегося объёма:** b4 сообщает 36/36 целевых Release и закрытие прежней тройки отказов без изменения assertions; rawTRX этого сообщения аудитору недоступен. Проверки CAS/lateEvaluate/ordinarycomposition/другого процесса ещё добавляются адресно. После их реализации существующие `TranslationLostCandidateArtifactTests`, `TranslationWriterConflictAcceptanceTests`, `TranslationStateVersionLostUpdateTests` сначала обнаруживаются и исполняются в Release. Настоящий store пишет файл; hash прочитанных bytes равен locator; другой процесс восстанавливает именно проигравший урок без Train. Два писателя и последующий третий различают CAS-winner и текущую version. Late-cancel Evaluate проверяется после барьера. Обычная composition тоже достигается: прямой unit test store её не заменяет. Sync/async контролируются отдельно, включая русский текст и отсутствие усечения long version. Файловые ошибки имеют собственный отказ и не замалчиваются.

**Завершение:** исходная тройка `CANDIDATE_PERSISTENCE_FAILED` уже устранена в b4 по опубликованной целевой расписке; сохранить expected STATE_VERSION_CHANGED/CANCELLED. Q01 закрывается после оставшихся CAS/lateEvaluate/ordinary-store/fresh-process проверок, а не только по 36/36. Изменение assertion допустимо только по независимому изменению договора; это не такой случай. Сохранённые файлы кандидатов не удалять ради зелёного статуса.

<a id="r128-02"></a>
## R128-02. Один владелец — конкретный набор изменяемых объектов

**Адрес:** `11.DPO/DpoTrainer.cs`, `DpoMutationAdmissionCapability.cs`, `DpoMutationOwnerLedger.cs`, `14.ContinuousLearning/Core/ServerOwnedLegalTrainingTeacherAdmissionAuthority.cs`; оба публичных/внутренних входа тренера и реальный issuer. Существующие reference-separation/binding проверки сохранить.

Доверенная сторона разрешает project→recipient/поколение. Содержательный checkpoint hash хранится отдельно от личности mutable Parameter/Data. Область ledger соответствует области владения; новая capability не создаёт пустую независимую историю для уже занятого recipient. Project owner не может исчезнуть в обычном Issue или при public admitted constructor recipient=null. Если законный legacy/shared путь предусмотрен, он именован и проверяется в своём договоре, а не случайно проходит через null.

**Минимальная различающая матрица:**

| Вход | Ожидание |
|---|---|
| A со своим реальным recipientA, два последовательных шага | Оба разрешены |
| Два recipient из одинаковых checkpoint bytes/keys; capabilityA подставлена к B | Отказ до weights/optimizer/RNG/counters/files |
| Новая capability владельца B над уже принадлежащим A recipientA | Отказ; над своим B — успех |
| Реальный issuer не знает owner в проектной операции | Именованный отказ до mutation |
| Capability с ownerA передана через public admitted constructor без project recipient | Проектная операция не получает молчаливую свободу выбирать B |
| Законный shared/legacy вход | Положительный контроль по явно названному договору, не blanket запрет |

Исполнять настоящий optimizer step на изолированных небольших fixtures, сравнивать фактические ссылки/состояния и результаты. Придуманные разные строки `ref-alpha/ref-beta` и один вручную созданный ledger не воспроизводят найденный обход. Научный PRIMARY P121 с 2 updates не повторять. Решение о внешнем допуске по подписи/имени сборки T121-PERMIT остаётся отдельным.

<a id="r128-03"></a>
## R128-03. Сохранить SourceRowId и выполнить именно указанную policy

**Адрес:** `18.CapabilityTraining/CapabilityMaterialLineageGuard.cs`, `CapabilityEvaluationMaterial.cs`, `Tests/ExperimentFixes/CapabilityMaterialLineageTests.cs`. Это product defect ложного отказа, не доказанная утечка.

**Resolver:** независимый exact digest → при отсутствии exact разрешённый normalized fallback → единственная trusted family → claimed ID внутри найденного множества → trusted role/family. Не возвращать `rows[0]`, не сортировать список ради того же первого, не запрещать законные повторяющиеся Train bytes задним числом. Предъявленный ID не отменяет проверку actual content и межсемейную неоднозначность.

**Положительные контроли:** два разных Train IDs одной семьи с одинаковым Content и независимый Test проходят с Checked3/Train2/Test1; `SelectTrainingRows` сохраняет оба ID и порядок actual. Подача одного второго actual не зависит от порядка SourceRows. Аналогичный разрешённый normalized случай проверяет fallback.

**Отрицательные:** существующий `ExamContentAppearingInTrainUnderANewName_IsRejectedBeforeLearning` достигает FORBIDDEN_CONTENT_OVERLAP; неизвестный ID, ID строки с другим текстом и несколько семей на один digest по-прежнему отвергаются. Сначала исправить причину преждевременного отказа, затем запускать intended overlap, не ослабляя expected.

**Policy:** Dev и Test учитывать раздельно. Четыре комбинации `(ForbidTrainTest, ForbidTrainDev)=(0,0),(1,0),(0,1),(1,1)` отдельно для family и content; exact и normalized obey те же content-флаги. Разные тексты Train/Dev одной семьи позволяют проверить family независимо от resolver. All-true остаётся строгим. `RefuseHeldOutContentInConsumedMaterial` внутри Admit имеет отдельную гарантию; не ослаблять её этой правкой без отдельного изменения договора.

**Завершение:** native тесты доказывают разрешённый положительный случай и именованный отрицательный; saved Python projection служит воспроизводимым объяснением, не заменой C# приёмки.

<a id="r128-04"></a>
## R128-04. Восстановление P128 и решения из его результатов

**Адрес:** `experiments-v2/RP01-MICRO/P12801/Program.cs` (`RunChild`, Snapshot/StableState, sensitivity, общий Problems/finalizer), `Run-Experiment.ps1`. Вход — существующий `results-01`, независимый bundle и snapshot производителя. Историческую попытку не перезаписывать; новая verification-папка с указателем на её hash.

1. До Load сверить обязательные files/bytes/size/SHA по immutable bundle, включая phrase-order. После Load сравнить full stable state с независимым producer snapshot и согласованно значимые order/namespace/direction/lineage. Если timestamp исключён, исключение названо до проверки.
2. Compare по ключам и кратности всех требуемых row IDs, exact/phrase/KNN contents, затем ordinary outputs и copied composer. Воспроизводимый отрицательный тест сначала достигает этого же comparer через настоящий Load; parse failure до него не закрывает проверку state. Отдельный предзагрузочный hash mutant должен отказать именно до Load.
3. В общий итог включить `fresh_load_state_matches`, `fresh_load_consumer_matches`, `fresh_load_composer_matches` и результаты трёх чувствительных вмешательств. Каждый полный honest пакет проходит; замена одного child answer, исчезнувшая строка, неизменный ответ после удаления/замены памяти заставляют тот же finalizer отказать. Новый PRIMARY/Train не требуется; где используется уже сохранённый output, маркировать reanalysis, а не fresh model call.
4. Producer/child status согласовать с внешним exit. Малые вспомогательные процессы с кодами 0/9 закрывают четыре сочетания; только 0/0 допустим. Отсутствующий child/checkpoint — отказ, не success с предупреждением. Не запускать научную программу для проверки оболочки.

**Соседний адрес, история и действующий остаток:** в b4 P12901/Run-Experiment.ps1 повторял Say-only; в abf83ee797b это исправлено через Get-CombinedExit и exit. Сохранённые четыре пары относятся к helper producer/child; verifier имеет отдельный третий exit. В будущем назначенном QA проверить 0/0/9 и настоящий повреждённый пакет через verifier-процесс и тот же внешний потребитель, без PRIMARY. До повторной saved-проверки отделить immutable input от нового output: нынешний verifier пишет validation.json в Evidence.Root. Сохранить исторические результаты и измерения Q129.

**Граница переноса:** копия cap256/2048 показывает 1/2→2/2, обычный действующий DP-планировщик уже 2/2. Этот ремонт совершенствует проверку доказательства, а не назначает увеличение cap в другом алгоритме. Приёмка нового продуктового алгоритма возможна только при отдельно доказанном дефекте текущего consumer. Научный бюджет 1 batch/29 уроков/64 Flow/17 composer сохранён;2 live reference calls учтены отдельно.

<a id="r128-05"></a>
## R128-05. P126: завершение по исполнению, не по назначению

**Адрес:** `P12601/Pnl0407L2Bridge.cs`, `Program.cs`, `Run-Experiment.ps1`; общее применение к P119/P123 и Q03. Return моста содержит terminal/фактические updates/checkpoints; Main возвращает его outcome, оболочка сверяет все назначенные arms и restores. `continue` при отсутствии обязательного checkpoint не закрывает опыт.

**Контроли без обучения:** честный сохранённый полный пакет; квалификационный отказ; arm-result с отказом при process exit0; один отсутствующий checkpoint; неуспех reader; отсутствующий обязательный arm; внешний ненулевой exit. Положительная и отрицательные копии идут через тот же finalizer. DONE считает observed arms/updates, не `arms_planned`/`assigned_updates_total`.

Сохранить 16 выполненных arm-result и 28 updates двух попыток. Не менять исходное научное заключение:66/66 и 46/46 уже были до обучения, margin вырос, конечные веса и RNG в A/B равны. Проверка текущих ProjectReference/DLL при будущем продуктовом переносе обязательна; октябрьский HintPath не заменяет её. Новое обучение для ремонта отчёта или выходного кода не назначено.

<a id="r128-06"></a>
## R128-06. Свежая нативная QA и устойчивые результаты

Среда этого аудита не содержит dotnet/pwsh. Следующие команды — **задание рабочему .NET9/PowerShell7 host**, не расписка выполненного здесь прогона. Договор[46](../experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md) разрешает сборку, диагностику и обоснованный ремонт тестов в рамках назначенной работы. Новый общий запрос разрешения на тесты не нужен.

1. Получить полный checkout конечного source SHA, dependency graph и предусмотренные LFS artifacts. Зафиксировать source tree плюс dirty content manifest, SDK/runtime, configuration и actual assembly SHA. Partial source download или старый DLL не превращать в full build.
2. На каждый запуск свой каталог `scratchpad/qa/<unique-run-id>`; перед созданием доказать, что каталога не было. Тестовые artifacts направлять туда через реальные поддерживаемые paths/injection. Если test code пишет в tracked `artifacts/`, исправить этот seam или запускать в отдельном disposable checkout, сохраняя diff. Обычное удаление лога не чинит источник загрязнения. Исторические baseline-файлы — только для чтения; обновление expected — явная отдельная reviewed дельта.
3. Выполнить build, discovery, целевые тесты и назначенную достаточную регрессию в **Release**. Фильтр ниже сохраняет существующее семейство и явно добавляет два класса с другим namespace. До запуска сверить все шесть required FQN из49 с discovery; после запуска — с фактическими TRX TestDefinitions/UnitTestResult. Ненулевой total и Failed=0 без этих имён недостаточны; missing/skipped/aborted/ambiguous required case не закрывает приёмку.

```powershell
dotnet build Tests/LlmFromScratch.Tests.csproj --configuration Release --nologo -v minimal
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build --configuration Release --list-tests
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build --configuration Release --filter "FullyQualifiedName~ExperimentFixes|FullyQualifiedName~LlmFromScratch.Tests.TranslationPairTailSignatureLearningTests|FullyQualifiedName~LlmFromScratch.Tests.TranslationVerifiedRuleTagTests" --logger "trx;LogFileName=targeted.trx" --results-directory $qaRunDir
```

`$qaRunDir` задаётся новым проверенным абсолютным каталогом; это placeholder задания, не уже созданный путь. После ремонта и достаточной целевой приёмки выполнить назначенный full suite с другим fresh directory, если он является воротами данной работы. Запускать всё повторно без новой причины не требуется. .NET/Pwsh commands и terminal process handle записываются буквально.

4. Связать команду/handle→время→run ID внутри TRX→исходный набор test names→counters→exit→DLL/source. Сохранить raw TRX/лог по доступному проверяющему адресу; если repository limit мешает, durable artifact locator + hash/size, а не единственный временный путь. Не путать build timestamp и runtime timezone; сохранить интерпретацию времени.
5. Каждый failure классифицировать отдельно: compiler/discovery/setup/fixture/contract/product/infrastructure/unknown. Assert означает достигнутое утверждение, но не доказывает правильность setup. Для lineage применить R128-03; для candidate — R128-01; оставшиеся rule-tag/provenance failures не лечить одной заменой expected. 621 прочее падение без прежнего baseline остаётся неатрибутированным; 487 похожих failure messages — кандидат общей причины, не доказанные 487 независимых дефектов.

**Артефакт QA:** source SHA+dirty manifest, actual DLL/PDB hashes, discovery list, fresh TRX/log, exit, разбор изменённых artifacts, минимальная матрица влияния, статус каждого обязательного условия. SOURCE_IMPLEMENTED, STATIC_COUNTEREXAMPLE, NATIVE_EXECUTED и ACCEPTED не взаимозаменяемы.

<a id="r128-07"></a>
## R128-07. Остальные адресные проверки и карточки

Ремонт E43 использует tuple/однозначное кодирование project/file вместо display-конкатенации, сохраняет легальные внутренние пары и отвергает подмену provenance. Прежние 143/R125-04…12 остаются назначениями: P119 согласованный verdict; P117 настоящий consumer; P118 смысл полного плана; P120 сравнимая метрика и nested cost; P121 полный fingerprint; P122 реально достигнутый cap; P123 рабочий oracle/provenance; P124 passing-base label control; Q125 полный restored state. Не дублировать их новыми очередями и не объявлять выполненными из-за появления этого плана.

Адресные блоки 18 core-карточек и 10 planning-документов уточняют собственный seam, honest/bad example, действующего потребителя и предел. Q127/Q128 отмечены исполненными, Q129/Q130 сохраняют исходные дозы. Если активной попытке уже передан старый pin карточки, он остаётся её паспортом; новый текст не применяется задним числом и не требует перезапуска. История, gold, trained state и raw остаются неизменными.

## Передаваемый текст задания

> Дельта POST131: применить150/49 вместе с146. Для R128-06 сохранить ExperimentFixes и добавить tail/tag классы OR; сопоставить шесть FQN с discovery и TRX. Два canonical-tag expected исправлять по105; floor fixture должна достигать кандидата; lineage requires product fix. Для P127/P131 связывать actual с native payload и упорядоченным содержимым; научный минус не равен invalid procedure.


> Возьми назначенный стабильный ID из действующей очереди и соответствующий раздел 146; назови source SHA, исходный договор и уже принятую часть. Прочитай действующие AGENTS/CLAUDE, применимые архитектурные, planning, plan-review, code-review, test-integrity, QA/automation/debug, prompt и communication навыки, честно зафиксируй прочитанное. Проследи реального producer, изменяемое состояние, обычного consumer и предикат итогового отказа. Сначала сохрани исходный failure и полный проходящий положительный пример; затем докажи одну адресную порчу тем же проверяющим. Исправляй причину, не подгоняй expected под actual. Новое научное обучение не нужно для анализа saved outputs/finalizer/exit; обычные изолированные fixtures уже назначенной нативной приёмки допустимы. Собери и проверь конечный source в Release на доступном host, свяжи raw result с DLL и командой, не перезаписывай историю. Если runtime недоступен, запиши буквальный отказ среды и не называй source/static просмотр native PASS. Отчёт по-русски: что уже работает, что изменено, какой контрпример исчез, фактический расход, ограничения и ровно следующий ID реестра. STOP/NEXT пользователя имеет приоритет. Не возвращай уже исполненные PRIMARY в очередь из-за устаревшего текста карточки.
