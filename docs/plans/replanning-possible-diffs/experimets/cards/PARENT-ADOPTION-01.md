# PARENT-ADOPTION-01 — принять обученный SFT-кандидат и выбрать его родителем следующего START

<!-- POSTQ003-2026-11-03:START -->
## Адресное уточнение после Q003 — ADOPTION-USES-THE-SAVED-BYTES

**Установлено / область:** Проверка adoption должна различать исправный donor, реально принятый component holder и обычного consumer. Повторный export сам по себе не доказывает исправность reader.

**Действие существующего владельца:** Проверять source payload→imported owner→ordinary caller и фактический holder, используемый Flow после import. До reuse донора применять50; список применимых частей101 сохраняется.

**Различающая приёмка:** Положительный контроль: назначенный bundle принят новым owner без Train. Отрицательный: несовместимый компонент или конфликт внутри materialized bytes не публикуется. Заново обученный третий объект не считается принятым исходным состоянием.

Обязательны [договор50](../codex-50-postq003-binary-and-historical-evidence-contract-2026-11-03.md) и [адресный план155](../../fixes-based-on-experiments/codex-155-postq003-addressed-repair-plan-2026-11-03.md). Это уточняет приёмку и сохранность; собственные factor, данные, gold, исторические budgets и очередь не меняются. Новый PRIMARY/Train для ремонта документа или bytes не назначен. Native Load/технические проверки учитываются отдельно только в собственном назначенном ремонте; их отсутствие обозначается UNRUN.

### Обязательная часть передаваемого поручения

> ADOPTION-USES-THE-SAVED-BYTES: Проверять source payload→imported owner→ordinary caller и фактический holder, используемый Flow после import. До reuse донора применять50; список применимых частей101 сохраняется. Положительный контроль: назначенный bundle принят новым owner без Train. Отрицательный: несовместимый компонент или конфликт внутри materialized bytes не публикуется. Заново обученный третий объект не считается принятым исходным состоянием. Передай exact attempt/path/donor authority, фактические commands/exit и предел проверки. Сохрани исторические результаты; повтор обучения ради получения отсутствующего файла не выполняй.
<!-- POSTQ003-2026-11-03:END -->


<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница STATE:** Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Исходный адрес из паспорта:** `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/PARENT-ADOPTION-01/report.md>), [отчёт 2](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/results/report.md>).
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

20 октября 2026. **DESIGNED_NOT_MATERIALIZED / NOT_RUN.** Сейчас разрешены READ_ONLY_RESEARCH и документы, не реализация, build, загрузка checkpoint или обучение. Этот авторский рецепт назначает будущий ремонт и проверку, но не подтверждает, что новые API уже существуют или собираются. Сохранённые опыты не повторяются. Учебная цель старого №08 остаётся открытой.

**Действующий переход: [PARENT-ADOPTION-01](PARENT-ADOPTION-01.md) → [N63-INGRESS-LENGTH-LEARNED-01](N63-INGRESS-LENGTH-LEARNED-01.md).** При одиночном поручении только этого ID — END; при прямо порученной программе — указанный NEXT. Числа остальных позиций принадлежат [текущему индексу](00-execution-index.md). Исполнитель и проверяющий используют модель текущей сессии по умолчанию, без override. Student — назначенная ниже native H16, а не модель агента. Astra выполняет только исследование и документирование.

При «остановить всё» прекратить действия немедленно; при прямом «следующий» перейти немедленно в разрешённой фазе. Не дописывать report, hashes, review, cleanup, Git или retry предыдущего ID. Ни новый root, ни suffix не обходят уже выданную попытку.

## 1. Конкретный ремонт и граница утверждения

Старый [N63I/Program.cs:48](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I/Program.cs:48) создаёт новую базу; [FrozenFactory:363](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I/Program.cs:363) загружает её, после Load устанавливает новые LoRA и создаёт новый AdamW. Выбора принятого обученного SFT-parent здесь нет. Исправляемая цепь: **проверенный SFT candidate → accepted full bundle → обнаружение latest compatible → восстановление weights + keyed m/v/step → следующий native SFT update**.

Native DPO публикация уже есть: [Publish:108](C:/Projects/llm_mvp_core/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.cs:108), [LoadLatestCompatible:96](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointLoader.cs:96). Их DPO manifest требует `DpoDatasetManifest` [request:61](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointManifest.cs:61). Поэтому не выдавать raw SFT за DPO training run и не использовать `InitialBaselineNoUpdate` для обученных весов. Наличие отдельных step1/parent=null не доказывает, что все будущие опыты обязаны давать0.

**Единственный A/B фактор — принять один и тот же проверенный candidate перед следующим START.** В A он остаётся pending; discovery выбирает accepted baseline000001. В B owner публикует candidate как accepted000002; тот же discovery выбирает000002. У обоих одинаковые executable, schema, model/profile, tokenizer, данные, evaluator, update count и source closure. Разные веса/m/v/step на старте являются именно следствием принятия полного кандидата, а не вторым независимо выбранным фактором. Не добавлять вариант weights-only/reset-moments.

Это будущий ремонт изолированной SFT integration; не уже существующая native SFT CLI и не продуктовый rollout. DPO guards/roles не ослабляются. Подтверждение может быть механическим и частичным учебным; полное освоение и генерализация требуют отдельного фактического показателя.

## 2. Единственные пути, entry и lifecycle

| Назначение | Полный путь |
|---|---|
| H | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01 |
| R | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/results |
| Private A/B repository roots | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/repos/A и C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/repos/B |
| Shared future module | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/host/SftAcceptedParent.cs |
| Future entry | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/Run-PA01.ps1 |
| Consumer handoff | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/results/accepted-parent-handoff.json |
| Documentary result mirror | C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/PARENT-ADOPTION-01/report.md |

Файлы H сейчас не создаются. После отдельного прямого поручения этого опыта исполнитель реализует один точный внешний entry:

```powershell
& 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/Run-PA01.ps1'
```

Внутренняя последовательность ровно одна: early claim → Prepare-PA01.ps1 → одна build → один Runner --study → documentary summary. `Run-PA01.ps1` атомарно создаёт H/execution.claim через FileMode.CreateNew **до** prepare/copy/build/model load. До claim проверить отсутствие H/results и private roots; найденные outputs/claim означают ALREADY_ATTEMPTED, не удаление/новый suffix. Wrapper — единственный владелец создания R. В root claim он записывает ownerNonce=lowercase hex32случайных bytes из RandomNumberGenerator.GetBytes(32), свой PID, exact H/R/entry и UTC; тот же nonce передаёт prepare/study.json/Runner, без ручного ввода. Prepare проверяет root claim/nonce и только затем пишет finite files. Внутренний Runner **до первого common model constructor/load/Forward** проверяет exact study/root/ownerNonce и атомарно создаёт R/study.claim FileMode.CreateNew; существующий study.claim либо mismatch прекращает весь Runner до common stage. Только затем Runner создаёт R/common, R/A, R/B и repo-shaped private trees. Перед model construction/train каждой руки дополнительно создать R/A/training.claim либо R/B/training.claim с тем же ownerNonce. Прямой повтор Runner теперь отказывается до common panel5, не лишь на позднем arm claim; поздний CreateNew raw не защита от повторного обучения.

Будущие внутренние команды, не самостоятельные альтернативные entry:

```powershell
& 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/Prepare-PA01.ps1' -OwnerNonce $ownerNonce
dotnet build 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/host/Runner.csproj' -c Release --nologo
dotnet 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/host/bin/Release/net9.0/PA01Runner.dll' --study 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/results/study.json' --owner-nonce $ownerNonce
```

Prepare30s, build120s, Runner120s, summary30s; один запуск каждого, без retry. Каждый дочерний процесс запускать через Start-Process -WindowStyle Hidden с раздельными stdout/stderr и WaitForExit(timeout); timeout завершает только принадлежащий wrapper PID tree, сохраняет partial и прекращает цепочку. Exit unknown → наблюдать тот же PID/receipt, не запускать заново. До внешнего Run scripts, Program и новый module написаны целиком; перенос фиксированных native sources/deps выполняется ровно один раз внутренним Prepare после claim. Нет solution build, tests, SDK/NuGet установки, старых N63I/RESTORE launchers или дочернего AI. Ошибка не разрешает обход native guards.

Минимальные разрешённые при будущем execution coding exceptions: только перечисленные H scripts/projects/source copies, SftAcceptedParent.cs, Program.cs, literal metadata и частные runtime artifacts; required-changes.md и documentary report в разрешённом docs-root. Canonical product source, training-artifacts/models, старые raw/donor/weights только читаются. Эти exceptions не разрешают реализацию в текущем READ_ONLY_RESEARCH.

## 3. Зафиксированные исходники, Student и donor

[pa01-source-contract.json](../../reconciliation/pa01-source-contract.json) фиксирует81 native CL source copy +29 dependency metadata entries. Source pin для существующих копий — `fb9175cc752d41e1cb525e0609b7c96140940217`; fallback на будущий HEAD запрещён. Список берётся целиком из [N63I source manifest](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63I/source-manifest.json), bytes — из его81 файлов `35.ExperimentsV2/Experiments/Rp01Micro/N63I/CL`. Dependency manifest — [N63I/dependency-manifest.json](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I/dependency-manifest.json). В настоящем исследовании DLL/checkpoints не загружались и не хэшировались; binary hashes — требования существующих receipts, которые будущий prepare проверит автоматически. Несовпадение → SOURCE_OR_DEPENDENCY_DRIFT, без поиска замены.

Два source-copy проекта сохраняют ABI:

- H/src/CL/ContinuousLearning.csproj: net9.0, AssemblyName/RootNamespace=ContinuousLearning,81 явных Compile + Link на H/host/SftAcceptedParent.cs; исходные IVT `42.ProjectWikiDomain`, `27.TranslationDomain`, `TrainableRouter`, `LlmFromScratch.Tests`. `_FindDependencies=false`; никаких wildcard imports.
- H/host/Runner.csproj: net9.0, OutputTypeExe, AssemblyName=PA01Runner, RootNamespace=PA01Runner, только Program.cs, ProjectReference на H/src/CL/ContinuousLearning.csproj и явные29References. Не компилировать CL прямо в executable.

Оба проекта: Nullable/ImplicitUsings enable, EnableDefaultCompileItems=false; ImportDirectoryBuildProps=false/ImportDirectoryBuildTargets=false до явных Sdk.props/Sdk.targets; разные obj/bin. Dependency paths — H/deps/<file> из фиксированного manifest, Private=true. Native boundary не содержит исходную ContinuousLearning.dll/Rp01SimulationDpo.dll; ResolveReferences проверяет CL только из H/src/CL/bin. Основание — [исходный Runner:21](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I/Runner.csproj:21) и [Native/ContinuousLearning.csproj](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I/Native/ContinuousLearning.csproj). Новые модуль и CLI ещё не проверены сборкой.

Student точно [RESTORE Program:37](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs:37): V256/Context64/H16/L1/Heads2/KV1/FF32, RopeTheta10000, RmsNormEps1e-5; q_proj/v_proj LoRA rank2/alpha4/dropout0; frozen base. Seed20261020UL, stream `restore-minimal-set`. Baseline000001 создаётся один раз `using Initializers.UseDeterministicSeed(20261020UL)` → new LlmModel → ApplySeeded(...,20261020UL,"restore-minimal-set"); её bytes копируются в оба private roots. AdamW beta1=.9,beta2=.95,eps1e-8,weightDecay0; baseline step0/empty moment entries.

Обученный donor назначен, исполнитель его не ищет:

| Файл | Ожидаемый receipt |
|---|---|
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/trained-weights.bin | 43500bytes; SHA256 ba8ec8c20250bc431da4bf9993ff22c508422cd7224b5c57cec33ce7b98c6e0e |
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/trained-optimizer.bin | 1112bytes; SHA256 603cf13423f7c6ae5af073a4ea824f49009175b145ce551f6c1b5cf59b66842a |
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json | historical receipt; snapshot step1000; TRAIN One2/20 + Two1/20, exact0/2 |

Ожидаемые binary SHA/length подтверждены чтением текстовых Git LFS pointers при source pin fb9175cc752d41e1cb525e0609b7c96140940217; actual payload не хэшировался в исследовании. Native Save/Load и portable export/import — [Program:84–113](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs:84). Это реально обученный исторический SFT snapshot, **не** доказанный полный fit. В raw после дополнительных200 было4/40, exact0/2; эти200/1000 не повторяются. Нельзя обещать будущие4/40, выдавать0/2 за2/2 или приписывать четырём новым updates результат прежних1000. Исторические raw сохраняются.

Tokenizer: ноль merges, Vocab256 byte IDs, source [BpeTokenizer:42–58](C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:42). Для нового bundle без Train tokenizer записать canonical JSON через BpeTokenizer.Load/Save(stream): `Vocab` = mapping строковых ID0..255 на Base64 однобайтовых значений, `Merges` = пустой массив. Имена JSON properties/формат точно native SavedModel/SerializeCanonicalState; не придумывать другой serializer. Reopen и Encode five fixtures должны равняться их UTF8 bytes. Ни одного нового tokenizer training call. Этот артефакт восстановлен из доказанного byte-only source contract; не притворяться, что donor сохранил tokenizer.json.

## 4. Честный SFT schema, writer, selector и restore

**Новый future module** H/host/SftAcceptedParent.cs, namespace ContinuousLearning, без top-level/Main. Он компилируется внутри copied ContinuousLearning и использует native `RepositoryOwnedTrainingArtifactLayout`, `SecureLocalFileBoundary`, `LlmModel`, `BpeTokenizer`, `AdamW`. Не меняет DPO manifest/parser, не имитирует DPO lease/dataset. Общий SFT owner проверяется в этом опыте; его существование не заявляется заранее.

Фиксированный identity: domain=`code-documentation`, design=`sft`, model=`rp01-ingress-h16`, version=`v1`. Address создаёт native `CreateModelCheckpointAddress(root,...,ordinal)` [layout:425](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedTrainingArtifactLayout.cs:425). Private roots содержат copied canonical LlmFromScratch.sln bytes как sentinel и пустые marker directories14.ContinuousLearning/11.DPO; это не clone/worktree и solution не собирается. Native root validation [1328–1379](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedTrainingArtifactLayout.cs:1328) сохраняется; все resolved paths строго внутри PA01/repos/A|B, no-reparse. Canonical repository root как write target отвергается.

Каждый accepted bundle — `training-artifacts/models/code-documentation/sft/rp01-ingress-h16/v1/checkpoints/000001|000002/{manifest.json,weights.bin,optimizer.bin,tokenizer.json,rng.json,source-attempt.json}`. Ordinal — **порядок принятия**, не optimizer step. Pending candidate располагается вне checkpoints: `<privateRoot>/training-artifacts/models/code-documentation/sft/rp01-ingress-h16/v1/candidates/import-restore-1000/` с теми же payload files и candidate.json; selector этот каталог не перечисляет.

Публичный будущий контракт модуля исчерпывающий; DTO ниже реализовать в том же файле:

```csharp
namespace ContinuousLearning;
// Все OperationResult — native AgentInfrastructure.OperationResult<T>.
public static class SftAcceptedParent
{
    // Фиксированные inputs/schema из этой карточки, без caller-assigned provenance roles.
    public static OperationResult<SftAcceptedReceipt> PublishInitialBaseline(
        string repositoryRoot, SftBaselineSnapshot baseline, CancellationToken token);
    public static OperationResult<VerifiedSftCandidate> ImportReviewedCandidate(
        string repositoryRoot, string donorDirectory, CancellationToken token);
    public static OperationResult<SftAcceptedReceipt> PublishAccepted(
        string repositoryRoot, VerifiedSftCandidate candidate, CancellationToken token);
    public static OperationResult<SftStartedState> PrepareStartLatest(
        string repositoryRoot, string? expectedHandoffPath, CancellationToken token);
}
```

`VerifiedSftCandidate` — sealed, private constructor, создаётся только ImportReviewedCandidate после проверки named donor/source receipts, complete payload, model-state hash и import roundtrip; переданные caller значения `step/role/parent` не принимаются на веру. `SftBaselineSnapshot` — sealed owner-created snapshot из фиксированной baseline-фабрики того же модуля, public static `SftBaselineSnapshot.CreateBaseline()` с указанным seed/config, без обучения; ctor internal/private. `SftAcceptedReceipt`: ordinal, manifest absolute path/hash, weights/optimizer/tokenizer hashes, modelStateSha256, optimizerStep. `SftStartedState`: Model, Tokenizer, Optimizer, TrainingConfig, ParentManifestPath, ParentManifestSha256, AcceptedOrdinal, OptimizerStep, WeightsSha256, TokenizerSha256, ModelStateSha256. Возвращаются новые graph objects; на том же model не устанавливаются адаптеры повторно.

Новый JSON schema=`rp01-reviewed-sft-accepted-bundle-v1`; strict UTF8 без BOM, SHA256 lowercase; duplicate/unknown required-contract fields и non-finite numbers отвергать. Manifest содержит:

1. schema, role, domain/design/model/version, acceptedOrdinal, optimizerStep, historicalUpdates, localUpdates;
2. role000001=`FreshSftBaseline`, step0/historical0/local0; role000002=`ImportedReviewedSftSnapshot`, step1000/historical1000/local0;
3. `previousAccepted` = null либо {relativeManifestPath,manifestSha256,acceptedOrdinal}; у000002 это000001 **только adoption order**; `sourceTrainingParentUnknown=true`, `sourceTrainingParent=null` для imported snapshot;
4. полный ModelConfig, LoraConfig, TrainingConfig, optimizer hyperparameters, stable checkpoint parameter key/dimension inventory; SHA256 каждого canonical config JSON с явно заданным порядком fields и UTF8;
5. payload `{name,byteLength,sha256}` для weights/optimizer/tokenizer/rng/source-attempt; имя только из шести фиксированных basename, никаких абсолютных payload paths/`..`;
6. donor Program git blob/SHA256, donor raw SHA256 и ожидаемые weights/optimizer receipts; actual runtime source/dependency receipt SHA256; `provenanceKind=ReviewedHistoricalSftImport`, `sourceRoot` точный RESTORE root; не создавать fictitious native training attempt;
7. `rngContinuity=ResetAtAdoptionDropoutZero`: у donor нет сохранённого RNG receipt; новая явно обозначенная epoch при adoption, не обещание historical cursor continuity. Dropout обязан быть0.

`source-attempt.json` фиксирует именно прочитанное: original learner TrainSupervisedNextTokenStep,1000historical updates, step1000, snapshot weights/moments, original receipt/source pins, no native DPO manifest/lease, unknown historical parent/RNG. `rng.json` сохраняет actual native CaptureRandomState(topologySha,loraSha,optimizerStep) нового graph после attach-before-load; topology/lora hashes вычисляют native LoraRandomStateIdentity.ComputeTopologySha256(modelConfig,loraConfig,authority) и ComputeLoraConfigurationSha256(loraConfig,authority), где authority=new LoraPairRandomSeedAuthority(20261020UL,"restore-minimal-set","restore-minimal-set"). Сохранить все snapshot fields/entries (AdapterKey,StreamDomain,InitializationSeed,DropoutSeed,ConsumedCursor,format/algorithm/topology/lora/step); восстановить native RestoreRandomState(snapshot,new LlmModelRandomStateIdentity(topologySha,loraSha)). После START Capture должен совпасть field-for-field. No historical RNG invented; dropout0 проверяется до adoption.

Writer: new immutable staging directory под тем же private root; native owner-only CreateNew/WriteThrough/Flush(true)/handle-path/no-reparse checks; exact source copies, автоматические hashes; `weights.bin`/`optimizer.bin` не пересериализовывать для imported candidate. Reopen всех payload и проверить bytes/hash/length, checkpoint keys/config, portable state. Только затем atomic File.Move без overwrite payload в новый ordinal directory и **manifest последним**. Manifest никогда не является доказательством собственной корректности: читатель перепроверяет actual files. Из готового accepted directory ничего не удаляется/перезаписывается. Частично созданный bundle прекращает попытку, не запускает repair/retry. Guard patterns: [optimizer file:80–120](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedDpoOptimizerStateFile.cs:80), [manifest sealer:454](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointManifest.cs:454). Эти native file-boundary helpers переиспользуются, DPO-specific serializers/identity не вызываются для SFT.

**Точный H16 codec в общем SftAcceptedParent writer/selector:** `cfg` — закреплённый H16 ModelConfig §3; на повторно открытом weights stream установить Position=0, вызвать `var checkedArtifact = LlmModel.ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(2,new[]{"q_proj","v_proj"}),maximumArtifactBytes:1048576)` и требовать `checkedArtifact.IsSuccess && checkedArtifact.Value==true`. Перегрузка без topology не подходит. Перед последующим существующим attach-before-load restore снова Position=0 либо новый read stream; imported bytes не пересериализовывать. Это byte/shape/schema проверка без Train/Forward, не новый learner. N63IL и SFT-EXP наследуют именно этот исправленный PA01-модуль; отдельные loader/режимы для них не создавать.

Selector: перечислить только six-digit checkpoint directories указанного profile; parse/reopen все manifest/payload bounded (manifest/source/rng≤1MiB каждый, tokenizer≤64KiB, optimizer≤1MiB, weights≤1MiB для этого H16); проверить canonical root/relative paths/no-reparse, source role, полный compatibility tuple и monotonic previousAccepted chain. Corrupt/unsafe/sealed mismatch → typed failure **без fallback**. Правильно запечатанный иной profile не становится compatible; для текущего фиксированного profile отсутствие compatible → SFT_PARENT_NOT_FOUND. Выбрать максимальный acceptedOrdinal, а не max optimizerStep и не candidate pending. Не ограничивать выбор заранее000002. `expectedHandoffPath` проверяет выбранный result после discovery; не превращается в hardcoded LoadCheckpoint-path.

Restore selected: new LlmModel cfg + ApplySeeded(...,"restore-minimal-set") **до** LoadCheckpoint; BpeTokenizer.Load validated stream; GetCheckpointParameterEntries → AdamWParameterBinding(key,parameter); AdamWPortableState.FromBytes → TryImportPortableState. Затем повторный ExportPortableState: точное совпадение format, step, sorted keys, dimensions и каждого float32-bit m/v. Native import действительно копирует m/v и step [AdamW:234–291](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:234), но bool alone не является нашим доказательством. Model StateHash — native sorted keys/data bits algorithm [donor:264](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs:264); imported expected `db44f1a437653299b345dc83c85523aad1ba1c55d3285865fe92501e5e78e386`. No ApplySeeded/reset/reinit после Load. Mismatch → SFT_PARENT_STATE_MISMATCH до любого Train.

Для corrected08 эта же public PrepareStartLatest становится factory START. Её точный session constructor:

```csharp
new IncrementalTrainingSession(start.Model, start.Tokenizer,
    new Trainer(start.TrainingConfig), start.Optimizer,
    start.WeightsSha256, start.TokenizerSha256,
    TrainingSourceProvenance.Identify(start.Model.Config))
```

Нативное поле ResumeParentCheckpoint имеет DPO-typed schema и не подделывается; новая SFT identity остаётся обязательным SftStartedState/parent-selection receipt, связывающим session и accepted manifest. Future product integration этих metadata отдельно от изолированного опыта; карточка не объявляет native DPO metadata заполненными.

## 5. Точный порядок, данные и полный бюджет

Prepare только materializes finite source/metadata/scripts/deps, без model calls; $ownerNonce в командах — именно значение, созданное Run при exclusive root claim. Runner сначала выполняет root ownership validation и exclusive R/study.claim; common: создать baseline один раз, owner-publish identical baseline payload в A/B; в оба roots ImportReviewedCandidate exact donor; вычислить candidate panel **один раз**, затем A и B по очереди. Одинаковый новый модуль применяется к обеим рукам. A пропускает PublishAccepted (единственный фактор), B вызывает его один раз. Далее обе руки вызывают PrepareStartLatest(root,null,token), сохраняют actual selected identities и full restore comparisons.

Перед B continuation записать immutable R/accepted-parent-handoff.json (CreateNew): selected accepted000002 manifest path/hash, weights/optimizer/tokenizer/rng hashes, model/config/source-contract hashes, step1000/localUpdates0, future module file hash, `consumer=N63-INGRESS-LENGTH-LEARNED-01`, `snapshotStatus=FROZEN_BEFORE_PARENT_CONTINUATION`. Результат parent-repair подтверждается отдельным R/status.json **после** завершения проверки; consumer требует оба. Handoff не ссылается на post1002 state. После двух B updates **не** публиковать000003 и не менять000002. Post states хранятся только R/A|B/after.*.

TrainingConfig donor exact: Batch1, Seq64, MaxLr=MinLr.0003, Warmup0, TotalSteps2000, GradAccum1, Clip1, WeightDecay0. Native AdamW .9/.95/eps1e-8/decay0; scheduler constant. В каждой руке ровно два `Trainer.TrainSupervisedNextTokenStep(model,optimizer,ids,weight:1f)`: One, затемTwo. A step0→2; B1000→1002. Не назначать200updates для parent mechanics; они принадлежат отдельному corrected08,1000historical не повторять.

Все тексты UTF8 без BOM; prefix=`// file: `+FileName+LF, target весь Source. Никаких добавочных EOS/BOS/pad/holdout Train:

| role/id | FileName | Source |
|---|---|---|
| TRAIN/train-one | One.cs | `static int One()=>1;` |
| TRAIN/train-two | Two.cs | `static int Two()=>2;` |
| EXAM/heldout-three | Three.cs | `static int Three()=>3;` |
| EXAM/heldout-four | Four.cs | `static int Four()=>4;` |
| CONTROL/control-branch | Pos.cs | `static bool Pos(int n)=>n>0;` |

Factory не вызывает повторный ApplySeeded/reset. Native IncrementalTrainer ordinary Apply разрешён: LoraApplicator.cs:787–799 возвращает прежний совместимый adapter, а не переинициализирует его. Native SFT objective — полная последовательность ids[0..L−2]→ids[1..L−1], а readout отдельно response-only; не менять objective ради улучшения. Readout exactly donor [Score:228–261](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs:228): один forward на строку, input IDs без последнего token, target row i+1, response mask target position≥prefix length; argmax ties lowestID, goldRank=1+count(strictGreater), NLL в double из уже полученных logits, сохранить native training loss отдельно.

Панели: common candidate5 + A0/A1/B0/B1 по5 = **25scoring Forward**. Training4Forward/4Backward/4optimizer updates. **Потолок29Forward,4Backward,4updates,0Generate,0DPO,0teacher,0tokenizer Train,0дополнительных calls ради gold/hash/report/review**. Negative controls Three/Four/Pos входят в эти25, не добавляются. Base/random graph creation/restore/file IO не бесплатны по времени, но не Forward; их costs записываются отдельно. Буферизация logits/weights/moments comparisons не вызывает model повторно. Любой technical failure прекращает остаток; actual/ceiling раздельно. Положительный результат не увеличивает бюджет, отрицательный не запускает поиск seed/data/checkpoint.

## 6. Что считать результатом и что передать дальше

Сохранить R/study.json, source/dependency/input receipts, common donor verification/panel, A/B parent-selection.json, before/after panels, online losses, parameter+optimizer bit comparisons, call counters, status.json, report.md. Один study.json владелец R; не создавать второй study в host и не принимать path из текущего cwd. Во всех outputs actual source/hash/IDs, сравниваемые baselines и причины пропущенных стадий.

Независимые verdict:

- `ADOPTION_REPAIR_VERIFIED`: pending candidate обеих рук identical; A selects000001/step0, B selects000002/step1000; selected bytes/m/v/step/config/RNG epoch точно восстановлены; B0 panel совпадает common candidate (token/rank exact, NLL/logits abs≤1e-6); B step1002 и A step2 после ровно2updates; accepted000002/handoff остались1000. Все технические guards обязательны.
- `TRAINED_BEFORE_PARTIAL`: actual B0 TRAIN One2/20+Two1/20, exact0/2, statehash native совпал historical snapshot. Это частичные токены обученного состояния, не полное решение. При несовпадении historical panel указать measured discrepancy; не дописывать обучением до нужных чисел. `TRAINED_START_SIGNAL` отдельно сравнивает B0 с A0: correct tokens/exact/NLL/rank и delta; NLL decrease≥1e-6 полезен, но слабее accuracy. Считать и вывести лучшую constant-token accuracy на том же gold panel без model calls;3/40 не объявлять превосходством над baseline без расчёта.
- `CONTINUATION_OBSERVED`: actual losses/finite updated parameters и m/v, правильные steps. Два новых updates проверяют продолжение, **не обязаны** давать learning gain. Отсутствие accuracy gain не отменяет доказанный selection/restore;0→0 не закрывает учебную цель08. Corrected08 получает отдельный200update budget по своей карточке.
- EXAM/CONTROL регрессии всегда рядом с TRAIN улучшением. Costs import/selection/train/readout раздельно; фиксированный порядок A→B и разные starting state не дают causal speedup. Статистических повторов нет.

R/status.json содержит `technicalStatus=COMPLETED` только после нормального завершения; ошибки дают FAILED либо PARTIAL. Остальные поля: `adoptionRepairVerified`, `trainedBeforePartialObserved`, `continuationObserved`, `handoffReady`, `actualCounters`, `limitations`. HandoffReady только при intact accepted000002/step1000, ADOPTION_REPAIR_VERIFIED и ненулевой воспроизведённой TRAIN panel; он не означает full task fit. Никакой blanket gate на иные acquisition/Unit/kNN/BPE/coldconsumer опыты. Source/ABI/runtime failure — честный partial, без materialized-ready claim.

При обычном завершении подтвердившийся полезный кандидат обязательно записать в [required-changes.md](../../required-changes.md) одной записью≤120слов: конкретная точка ремонта, measured before/after, scope и регрессии. Ручной checksum review не назначается: автором предусмотрены автоматические receipts, читается их результат. Русский report≤350слов плюс таблица. STOP/NEXT выше любых отчётных обязанностей. Следующий зависимый опыт использует **byte-copy этого же SftAcceptedParent.cs** в N63IL/host и accepted1000 через selector, а не напрямую donor files.

Навыки исполнителя: C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md; planning-session-startup-SKILL.md; plan-review-SKILL.md; generated-code-review-SKILL.md; experiment-design-SKILL.md; output-readability-gate-SKILL.md; human-readable-architecture-audit-SKILL.md; prompt-patch-authoring-SKILL.md; human-communication-GLOSSARY.md. Приоритет этого concrete recipe над старым запретом искать SFT schema: schema repair теперь явно назначен автором; unknown API/compile remains unverified, но не задача исполнителя выбирать модель/данные/host/budget.

[Standalone executor prompt](../prompts/PARENT-ADOPTION-01-executor.md). [Primary repair evidence](../../2026-10-20-parent-adoption-repair-evidence.md).
