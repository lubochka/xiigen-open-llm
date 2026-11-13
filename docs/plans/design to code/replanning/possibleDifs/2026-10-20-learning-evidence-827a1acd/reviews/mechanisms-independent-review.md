# Независимый static review общего пакета827: native mechanisms и JSON host

Проверены `JSON-OPTIONS-REUSE-01.md`, `-HOST.md`, `-PROMPT.md`, авторские `work/json-options-01-assets/{prepare.py,Program.cs,compare.py}`, `02-learning-paths-and-gaps.md`, `05-execution-contract.md`, соответствующие9 Translation source и frozen P125 scorer/data. **Build/run/tests не выполнялись; файлы общего пакета не менялись.** Собственная ATTN-карточка не выдаётся за независимо проверенную.

Ниже только существенные замечания и конкретные исправления. Это findings исходного предъявленного варианта; автор JSON получил их для исправления. После получения его финальной версии нужна одна адресная сверка затронутых мест, не повторный аудит/запуск.

## Требующие исправления места

### R1 — проверяемая provenance не связана с фактически исполняемыми A/B binaries

**Места:** `Program.cs:170–222`, `prepare.py:90–114`; `JSON-OPTIONS-REUSE-01.md` раздел4.

Native host сверяет DLL-файлы в `root/deps`, тогда как .NET загружает build-output копии в `A/bin/...`/`B/bin/...`. Список загруженных assemblies лишь записывается, их hashes не сравниваются с manifest. Связь production `27.TranslationDomain.dll`↔PDB проверена, но этот DLL намеренно исключён из Reference; его PDB сам по себе не доказывает source исполнения нового `JsonOptionsA/B.dll`. Кроме того, одинаковый Program принимает аргумент A/B независимо от имени своей сборки: `JsonOptionsA.dll <root> B` проверит source B, хотя исполняется скомпилированное A. Это оставляет возможность ложного A/B без изменения JSON source manifest.

**Правка:** до первого обучения связать arm с фактической assembly (`AssemblyName == JsonOptions{Arm}`); проверить собственный PE/PDB и source checksums девяти compile-файлов плюс Program; сохранить hashes authored/generated assets. Для runtime dependencies сравнить actual load/copy paths с уже подготовленными manifest SHA, а не только `deps`. Не проверять source всей транзитивной системы; честное ограничение покрытия сохранить. Это экспериментальная provenance, не дополнительный учебный запуск или product gate.

**Изоляция Compile:** generated `Probe.csproj` сейчас использует implicit SDK import без отключения parent Directory.Build props/targets. Для утверждения «ровно9+Program, no ProjectReference» явно выключить parent imports **до SDK props** либо передавать соответствующие свойства каждой build-команде; не зависеть от неизвестного родительского `C:/xi/.../Directory.Build.*`.

### R2 — обещанные receipt/log/failure semantics не исполняются приведённым entry

**Места:** карточка раздел6, `prepare.py`, отсутствие общего runner в исходном HOST.

Текст обещает `command-receipts.json`, stdout/stderr каждого шага и «не запускать B после failed A». Приведён последовательный блок bare PowerShell-команд без проверки `$LASTEXITCODE`, redirect или timeout wrapper. Сам `prepare.py` этих receipts не создаёт, Program пишет только native receipts. В обычном PowerShell ненулевой exit native `dotnet` не гарантирует прекращение следующей строки. Исполнитель вынужден сам изобретать отсутствующую управляющую логику.

**Правка:** дать один полный заранее написанный wrapper с фиксированными6 внутренними командами (prepare, buildA, buildB, runA, runB, compare), записью argv/cwd/start/end/exit/log paths и немедленным завершением после nonzero/timeout. Не запускать wrapper одновременно с прежним списком. Существующий root/claim сохранять; stdout подготовки до root писать в однозначный sidecar author-host и затем переносить/копировать без повторного prepare. Бюджет остаётся2build+2native+1comparison,96outputs,16lessons.

### R3 — comparison не выдаёт обещанную четырёхсостояний learning delta и может сохранить неверную attribution

**Места:** `compare.py:55–75`, `05-execution-contract.md:15,27–32`.

`qualityDeltas` формируется только из A-after/B-after. Frozen scorer уже создаёт before/after таблицу, поэтому недостающие A0,A1,B0,B1/ΔA/ΔB/ΔΔ можно вычислить без новых запросов. Кроме того, `beforeOutputsChangedState`/`afterOutputsChangedState` сравнивают только `memorySha256` EN_RU+KNN, хотя полный canonical state четырёх snapshot уже имеется. Влияние обычного readout на tokenizer/verifier/history/version остаётся вне этого verdict. Allocation attribution сейчас всегда `LOCAL_SOURCE_BOUND_OBSERVATION`, даже если initial/trained state mismatch уже записан рядом.

**Правка:** по каждому bucket/quality metric сохранить before/after всех четырёх состояний и три дельты; whole-state readout immutability проверять через существующий `canonicalSha256` initial→before и trained→after. Добавить отдельные `exactOptimizationSupported`/`causal_attribution`, учитывающие initial equality, действительное изменение памяти, frozen state, final equality и output differences; использовать контрактные значения `SUPPORTED_IN_THIS_PAIRED_RUN`, `NOT_ESTABLISHED`, `INVALID_COMPARISON`. **Наблюдаемые полезные deltas не удалять при mismatch** — снять только утверждение об эквивалентности/атрибуции.

### R4 — native host всё-таки читает gold до выполнения ordinary outputs

**Места:** `prepare.py:44–49,96–98`; `Program.cs:178–182`; карточка раздел5.

В manifest dataFiles входят `panel-gold.json` и `score.cjs`. `CheckProvenance` читает bytes **всех** dataFiles, поэтому `File.ReadAllBytes(panel-gold.json)` выполняется до96 outputs. Gold при этом только хешируется и не используется для ответа — **предсказательная утечка не обнаружена**. Но категоричное обещание «native host его не открывает; gold читает только scorer после outputs» неверно.

**Правка:** native preflight читает/сверяет только training-batch и eval-inputs; gold/scorer copies+hash проверяет comparison/scorer после завершённых A/B outputs. Не ослаблять pinned gold identity, а перенести проверку к действительному owner. Обновить metadata границы доступа.

### R5 — общий контракт слишком широко исключает сериализацию из учебной стоимости

**Место:** `05-execution-contract.md:17`.

Фраза «диагностика, сборка, загрузка, сериализация и отчёт учитываются отдельно» распространяется на весь native вызов. Однако в реальном EOPT `StepVector` обязательная checkpoint publication/optimizer state serialization находится внутри учебного транзакционного пути; JSON опыт измеряет и необходимый parsing входного урока. Удаление такой работы ради «чистого» таймера изменило бы саму цепочку, которую пользователь требует измерять.

**Правка текста:** «Диагностические snapshots, подготовка, чтение gold, сборка и отчёт учитываются отдельно. Parsing, guards и publication/serialization, которые входят в назначенный native Train/StepVector, остаются внутри его measured scope и явно называются частью цены; вложенные таймеры не суммировать с охватывающим». Для уже сохранённых EOPT numbers scope исправляется документально без rerun.

### R6 — JSON NEXT противоречит новому основному реестру; incident quote отсутствует локально

**Места:** JSON card148–152, prompt15, HOST introductory paragraph.

`NEXT=END_JSON_OPTIONS_ADDENDUM` не соответствует основной назначенной цепочке JSON36→IDENTITY37. При этом local карточка/промпт имеют строгий запрет продолжения, но обязательная цитата инцидента есть только в общем05, несмотря на его обещание дублировать в каждой новой карточке и промпте.

**Правка:** effective NEXT=`IDENTITY-M1-01`/№37 только при назначенной полной программе; при одиночном JSON ID — END, без самостоятельного запуска следующего. Добавить локально тот же короткий инцидентный quote и исключение только для необходимого прекращения собственного активного процесса. Синхронизировать card/prompt/HOST/output map, не менять protected registry.

## Существенные проверки без замечаний

- Все9 source blob SHA подтверждены текущим Git tree669, а не локальными scratch bytes с возможным добавленным newline. Видимой недостающей Translation source dependency в выбранной композиции не найдено; runtime build при этом не заявляется проверенным.
- B patch ограничен `ExecutorUnit.TrainOnExample` и private holder с явным cctor. Первый доступ к reusable options происходит внутри настоящего TRAIN, без prewarm; конфигурация converter/DTO не меняется.
- `TrainStructuredBatchAsync` на pin синхронно выполняет8 native memory lessons и возвращает completed Task. Сам этот вызов измеряется, не detached JSON benchmark. Observer snapshots/output serialization снаружи.
- 96 outputs согласованы:2arms×2phases×24calls. TRAIN fixture8 не содержит heldout; native consumer получает только input/context и не заменяет ответ gold. Существующий scorer сохраняет assess/rubric; меняются лишь два списка плеч ABC→AB.
- Полный canonical checkpoint сохраняет learned state/history identities; исключаются только export timestamp и три operational history-cost поля. Maps экспортируются в установленном порядке. Структурной причины искусственного A/B state mismatch в этом canonicalization не найдено.
- `02-learning-paths-and-gaps.md` корректно разделяет native DPO/LoRA и structured memory writers, не выдаёт исторические mask/sign дефекты за текущие и не объявляет resource-гипотезы измеренными успехами. Дополнительных material правок в пределах указанного review не найдено.

После исправлений R1–R6 статическая карточка может передаваться как authored-unbuilt experiment. Этот verdict не означает runtime success, готовность product patch или разрешение нового запуска в текущей документарной фазе.
