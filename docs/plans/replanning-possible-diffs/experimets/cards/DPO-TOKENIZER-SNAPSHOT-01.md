# DPO-TOKENIZER-SNAPSHOT-01: одна закрытая копия токенизатора на подготовку двух учебных пар

**Передаваемое дополнение SNAPSHOT-IDENTITY — 3 ноября 2026.** Снимок с теми же counts не обязательно тот же tokenizer: проверить фактические token IDs/encoding двух назначенных пар и потребление StepVector по исходному паспорту. Честный saved-пакет проходит, подмена содержимого при сохранении формы достигает именно comparer; ранний отказ разбора — отдельное свойство. При повторном просмотре старые результаты read-only, вывод отдельный. HOLD общего FIX-TS сохраняется; эта профилактика не разрешает перенос или новый Train. Применять [48: навыки, ожидаемая стадия, исправление теста и итог](../codex-48-post128-executor-verification-2026-11-03.md) вместе с собственным паспортом. Это уточнение проверки в назначенном объёме, не новый запуск. STOP/NEXT пользователя немедленны.

<a id="current-executor-task"></a>
## Текущее задание исполнителю — 3 ноября 2026

**Что проверяется: Принадлежность снимка токенизатора.** До отдельного прямого назначения исполнения — анализ сохранённых данных и документации. Уже записанный PRIMARY или alias не повторять; текущее состояние смотреть по ID в индексе. [Индекс опытов](00-execution-index.md) определяет очередь; эта поправка не добавляет новую работу. Исторические промпты ниже не являются альтернативным запускным заданием.

**Хороший пример:** Обе стороны пары подготовлены одной закрытой версией tokenizer; actual vocabulary и данные подготовки принадлежат назначенному TRAIN.

**Плохой пример и ожидаемая реакция:** Сменился vocabulary при прежнем claimed snapshot ID или EXAM участвовал в fit, но сравнение объявлено невиданным. Сохранить измеренную ресурсную ось и отдельно ограничить независимость.

**Передаваемый текст задания:**

> Работай по полной карточке `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/DPO-TOKENIZER-SNAPSHOT-01.md`, её паспорту данных и этому текущему разделу. Перед заданием заново полностью прочитай local/canonical AGENTS, CLAUDE и стек навыков из [договора120, §1](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md#1-свежее-полное-чтение-перед-новым-аудитом-или-заданием); для приёмки также test-integrity, test-failure-triage и bug-to-tests. Примени [договор44, §10–12](../codex-44-post117-saved-evidence-and-finalizer-contract-2026-11-03.md#10-один-текущий-вход-вместо-нескольких-противоречащих-заданий): независимое ожидание → фактический рабочий путь → общий итог. Используй собственные хорошие/плохие примеры выше, не расширяй панель чужими fixtures. При падении сохрани input/expected/actual и версию; отдели регрессию, неверное ожидание, измеритель и инфраструктуру. Не меняй gold/допуск и не скрывай missing/unknown ради PASS. Показатели до/после, точность и регрессии укажи раздельно на русском. Документное поручение не разрешает код, сборки, тесты, Train/Execute. Будущая разрешённая проверка использует только назначенные в паспорте host/команду/бюджет; отсутствующий режим ремонта верни автору, исторический PRIMARY его не заменяет. При разрешённом запуске дождись исходного handle по договору31 без relaunch. Прямой STOP/NEXT исполни немедленно; не продолжай прежнюю работу ради оформления. Любой измеренный полезный эффект сохраняется по своему показателю и домену, с ценой рядом; допуск переноса оценивается отдельно.

Основание усиления: [аудит138](../../fixes-based-on-experiments/codex-138-post117-transfer-and-experiment-audit-2026-11-03.md) и [разбор контекста исполнителя](../../fixes-based-on-experiments/evidence/2026-11-03-post117-executor-context-review.md). Для родственных карточек это профилактика того же класса ошибки, а не утверждение, что их результаты заново проверены. Исходные результаты, наборы, бюджеты и команды сохранены ниже.


<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница STATE/LEARN:** Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Исходный адрес и роль:** Фактор — `11.DPO/Models/DpoExactPairMaterial.cs::CreateBatch`, подготовка через `18.CapabilityTraining/LlmIngredientLogicalUnits.cs::LogicalDpoTrainingUnit.PrepareExecution/BuildBatch` и `LogicalDpoPreparedBatchMaterializer`; consumer — настоящий `11.DPO/DpoTrainer.cs::StepVector`. Исторический N63CollisionProbe не подменяет эту назначенную exact-material цепь. Исторический pin и собственный бюджет ниже сохраняются; роль зависимости не равна factor owner, runtime сейчас не выполнялся.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



<!-- BEGIN shared-start-binding-2026-10-21 -->
## Сохранённый старт: h512-dt-segmented/v1

**Действующий consumer-контракт:** [общее восстановление33](../33-shared-learned-starts.md), family `h512-dt-segmented`. Этот раздел заменяет повторную common preparation во всех командах и промптах ниже. Consumer выполняет **0 common updates,0 дополнительных common readouts**; исходные arm updates/readouts и единственный фактор сохраняются.

- Данные этого семейства: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/data.source.jsonl` и `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/data.material.json`. Заданный источник: `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/fixtures/dpo-tokenizer-snapshot.jsonl`; рецепт: D-T1,D-T2; alternate500 each; segmented no separator; KGB own metadata unchanged.
- Читать только `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/bundle.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/S0.weights.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/S0.optimizer.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/S0.rng.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/preparation.json`; generation/tokenizer/authority/source manifest — там же по33. Missing/partial/unqualified → конкретный SHARED_START_UNAVAILABLE, без самостоятельного обучения или другого checkpoint.
- Единственный producer: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01`, A-семантика [назначенной карточки](DPO-TOKENIZER-SNAPSHOT-01.md); его конечный бюджет 1000updates/1004Forward/1000Backward оплачивается один раз **отдельно**. Existing source для экспорта: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/common`.
- Будущий экспорт без model calls: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/Export-Existing.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`.
- Будущий однократный producer, только если он прямо включён в порученную программу: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/Prepare.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`. Это заданный интерфейс, ещё не реализованный этим документным проходом; consumer не вызывает его автоматически.
- Восстановление: S0 weights/RNG; DPO optimizer заново step0. SFT moments сохраняются как provenance, не выдаются за DPO continuation. Проверить named weights/keys/shapes, полный optimizer state и native RNG identity; construction/ApplySeeded только **до** load. Только DPO consumer заново создаёт native admission/lease после load, без переноса из producer; SFT-only MASK/MOMENT/H64/SILU/CS01 не создают DPO lease.
- Единственный consumer entry остаётся ``powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/Run-With-Learning-Signal.ps1``; его будущий wrapper вместо common Train читает fixed shared bundle, сохраняет его path/digest в study и восстанавливает A/B. R=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001`; в R/common разрешена неизменная входная копия shared snapshot для прежних внутренних путей. Она не создаёт preparation.claim/новые losses/updates. Reuse cost — чтение/restore, historical producer cost отдельно. Нельзя задним числом переписать claim уже выданного опыта.

Старые common-рецепты, методы подготовки и суммарные1000/400+arm budgets ниже теперь описывают **только однократного producer плюс consumer**, а не расход каждого consumer. Старый child `--prepare-learned`/prepare не вызывать из consumer wrapper. Все tools и ожидание — по31; никаких exporters/manifest/scripts «пока ждём». Артефакты и scripts здесь не созданы и не проверены запуском.
**Сохранение оплаченного state:** сначала независимо записать назначенные weights, optimizer, RNG и имеющуюся qualification; затем typed validation/readback; COMPLETE последним по33. Ошибка проверки weights не должна пропускать запись остальных уже доступных состояний. Новых Train/Forward и периодических снимков не добавлять; partial сохранять.

**Точный weights API для этой карточки:** `cfg` — фиксированный ModelConfig этой карточки; LoRA rank8, targets строго `q_proj,v_proj`. Producer: `model.SaveCheckpointToStream(stream)` на новом `FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None)`, изначально Length=0/Position=0; `Flush(flushToDisk:true)`. Затем независимо записать остальные state/receipt по33 и только после этого открыть сохранённый weights для чтения. Установить Position=0; `var checkedArtifact = LlmModel.ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(8,new[]{"q_proj","v_proj"}))`; требовать `checkedArtifact.IsSuccess && checkedArtifact.Value==true`. Если в конкретном рецепте уже назначен maximumArtifactBytes, передать его неизменным (CS01:8388608). Перегрузка без topology недопустима для этого LoRA snapshot. Перед native `freshModel.LoadCheckpoint(stream)` снова Position=0 либо открыть новый read stream; construction/ApplySeeded только до load. Проверить назначенные named keys/shapes/value bits. Равный размер файла, повторное чтение самого файла или снижение loss не доказывают равенство потерянной live memory. В consumed/recovery случае такое равенство помечать UNKNOWN, не придумывать. Эта запись уточняет уже назначенные операции, не добавляет model calls.

<!-- END shared-start-binding-2026-10-21 -->


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
## Исходный рецепт однократного producer S0 — не выполнять в consumer

**Точное связывание S0 с native authority.** Следующая DPO-цепь применяется только если назначенное arm действительно содержит DPO; MASK/MOMENT не создают DpoTrainer/lease и сохраняют0DPO. Для SFT их существующий copied session/executor остаётся владельцем. Это перенос последовательности [N63CollisionProbe:264–335](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:264), без запуска probe: оригинальный LoadModels завершает B0 hash/Step1 проверки **до** policy.LoadCheckpoint(S0.weights.bin); frozen reference остаётся B0. После загрузки вызвать прежний materializer, governance.Validate/ContentAuthority.Project, затем Capture(actual attached adapters), Capture(validatedBatch,recipient.Bindings), LegalTrainingTeacherAdmissionAuthority.Create([],0,same32ByteSalt), ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue с original callbacks, lease по прежнему exact RunId этой карточки и mutation capability. CreateForProjectLora получает **null continuationProof**, потому оба DPO optimizer новые, а не SFT-import. Старую immutable captured recipient/profile из W0 не переиспользовать. B0 manifest/hash остаётся origin W0; S0 bytes/hash/1000SFT записываются в private preparation.json отдельно, не подставляются в B0 manifest. [Profile:134–150](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:134) связан с material/framing/objective и key/element count, не с B0 adapter bytes; [Recipient:198–261](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:198) разрешает изменённые policy adapters, но требует неизменные base bits, frozen reference, topology и прежние Parameter instances. Эти validators не менять. В статической подготовке source-pin/shape/key/serializer/call-chain проверяется до Train; future actual RequireValid выполняется до и после common updates и после arm load, без дополнительного forward. При отказе — сохранить отказ, не ослаблять guards. Это source-compatible design, **не** доказанная ABI или runtime приёмка нового host.

**Точный codec общего RNG.** `S0.rng.json` — частный JSON **полного** TransformerModel.LlmModelRandomStateSnapshot, не native DPO filename. Запись: System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(snapshot), FileStream(FileMode.CreateNew,FileAccess.Write,FileShare.None); чтение: JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(bytes) с null→refusal. Вызвать native policy.RestoreRandomState(snapshot,new LlmModelRandomStateIdentity(topologySha,loraConfigurationSha)); повторный CaptureRandomState(topologySha,loraConfigurationSha,preparationStep) сравнивает FormatVersion/Algorithm/все AdapterKey,StreamDomain,InitializationSeed,DropoutSeed,ConsumedCursor,TopologySha256,LoraConfigurationSha256,RandomStateStep. Типы и guards — [Randomness/LlmModelRandomState.cs:11–39](C:/Projects/llm_mvp_core/03.TransformerModel/Randomness/LlmModelRandomState.cs:11), [LlmModel:286–375](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:286). preparationStep=1000 дляH512,400дляH64. Не вызывать RepositoryOwnedDpoRandomStateFile.WriteCreateNew для имени S0.rng.json. На DPO arm callback первый checkpoint имеет native step1 и **новое** metadata RandomStateStep1 при сохранённых восстановленных stream cursors; writer использует BuildFileName(bundle.Step). Это новый DPO epoch, не импорт SFT optimizer/continuation proof.

Это **AUTHOR_FIXED_PREPARATION_NOT_RUN**, конкретная замена прежнего cold-only старта A/B. H512 B0 остаётся проверяемым источником W0, а A0/B0 получают общий S0 после подготовки ниже. Никакого готового успешного S0 здесь не заявлено. Предел1000 — **DESIGN_CHOICE_NOT_MEASURED**: заранее ограниченная попытка обучения на фиксированных строках; source подтверждает API, но не достаточность числа шагов. Нельзя повышать предел, менять текст/seed/LR или выбирать промежуточный checkpoint по результатам. Host/entry/фактор остаются назначенными этой карточкой; consumed/partial/unknown существующий host запрещено обходить новой папкой.

**Данные и carrier.** Ровно обе строки fixed dpo-tokenizer-snapshot.jsonl с SHA91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95; chosen по500каждой. После исходного Inspect/governance/materializer и CaptureAndValidate брать для каждого выбранного DPO урока только `batch.ChosenIds.Data[0..batch.ExactMaterial.ChosenLength)`, проверенные целочисленные IDs. Не брать padding до SequenceLength, не вставлять LF/BOS/EOS и не перекодировать склеенный текст. COMMON и arm TRAIN scorer используют те же IDs и ChosenResponseMask.

**Оплаченная общая подготовка.** В исходном A-code один раз получить W0 прежним ReadVerified→PrepareStart→LoadModels с исходными Step1/no-parent/55/39/hash guards. В модели q/v-LoRA исходного H512 только16adapter tensors trainable; reference не обучать. Создать native TrainingLoop.Trainer и AdamW(.9,.95,decay0,eps1e-8); Batch1, SeqLen2048, MaxLr=MinLr=.001f, Warmup0, TotalSteps1004, GradAccum1, Clip1, weight1. Выполнить **ровно1000** TrainSupervisedNextTokenStep в назначенном цикле, без early success stop. Это1000forward+1000backward+1000optimizer calls, отдельно от arm timer и ниже указанного фактора. Единственные COMMON readouts — W0 и S0 после1000: 2 rows в каждом, всего4 policy forward, training:false, без backward/Generate. Использовать shifted input IDs[0..L−2], targetID=IDs[i+1], только назначенные response positions; logit row i. Снять per-position predicted/gold IDs, strict-greater gold rank, stable NLL и exact по **всем** response targets. Не сравнивать full-sequence SFT loss с response NLL как одну метрику.

**Qualification и отрицательный исход.** Common preparation.json содержит `learning_qualified` (boolean), completed_updates, exact_by_id_W0, exact_by_id_S0, acquired_ids и failure_reason. Wrapper допускает arms только при exit0 **и** learning_qualified=true; exit0 с false не означает разрешение A/B. Минимальная единица cost-сравнения — хотя бы одна заранее заданная полная TRAIN response, которая была exact=false в W0 и exact=true в S0; сохранить все 2 строк и список приобретённых ID. Это узкая квалификация запоминания полной строки, **не** доказательство общего правила, всех ответов или превосходства над constant-answer baseline. Две разные освоенные строки, если появятся, отражаются отдельно; порог не подбирается по seed. Если предикат не выполнен, записать PREPARATION_COMPLETED_LEARNING_UNQUALIFIED, реальные1000updates и панель; arm A/B не запускать, гипотеза/учебная цель OPEN. Это не автоматический NEXT и не завершение задания одним отчётом. Случай W0 уже exact по всем строкам не доказывает acquisition этой preparation; зафиксировать NO_ACQUISITION_CONTRAST и не искать другой набор. Ошибка до1000 — partial, unknown counts=null, без retry. Прямой STOP/NEXT остаётся выше оформления.

**Снимок и общая граница.** Только назначенный producer из33 в `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1` сохраняет create-new S0.weights.bin, S0.optimizer.bin, S0.rng.json и preparation.json. Native CreateNew SaveCheckpointToStream → независимая запись optimizer/RNG/qualification → точный typed weights API из binding выше → LoadCheckpoint с новым/rewound stream; все55key/shape/value bits сверить с live S0,39base сW0. Native AdamW.ExportPortableState по 16 реальным adapter bindings (GetCheckpointParameterEntries, reference-identity attached adapters, ordinal keys) → ToBytes/FromBytes. CaptureRandomState/RestoreRandomState используют исходный LoraRandomStateIdentity этой модели, не придуманные streams. В A/B после обычного B0 construction/validation загрузить S0; не переинициализировать adapters после Load. После одинаковой загрузки S0 обе arm создают **fresh native DPO optimizer0**; SFT m/v сохраняются в common receipt, но в DPO не импортируются. Reference в обеих arms — независимый исходный B0, frozen. Это осознанный общий weights-only переход SFT→DPO, не full DPO continuation и не production parent adoption. Новый recipient/profile/lease захватывается после S0 load; base39/R0 неизменны. Native mutation, publication, optimizer/RNG callbacks и admission не ослабляются. Private preparation.json честно помечает SFT-derived policy, source pin этой карточки, actual data/carrier identity и1000updates; не создавать поддельный canonical DPO parent manifest и не писать training-artifacts/models.

**Разделение entry.** Consumer wrapper после build проверяет shared bundle и восстанавливает A/B. Производитель запускается отдельно по контракту33; подготовка внутри consumer запрещена:
Исторический локальный `--prepare-learned` снят с маршрута consumer. Единственная будущая подготовка — отдельный producer из shared-binding в начале карточки и контракта33; LC остаётся export-only. Consumer читает COMPLETE bundle, при его отсутствии отказывает без Train.
Реализация будущего producer следует точному `Prepare.ps1`/`--prepare-shared` из контракта33, только в назначенном producer host и с исходной A semantics. Старый локальный PrepareLearnedStartAsync в consumer не реализовывать и не вызывать. Consumer owner создаёт собственные root/arm claims до load, проверяет shared bytes и qualification, затем восстанавливает state. Common claim принадлежит только producer. Ни runtime-реализация, ни новый запуск этим документом не заявлены.

Budget factor: 2 DPO updates, один StepVector(allTwo) на плечо. **Сумма однократного producer и одного consumer:** 1002 optimizer updates; 1016 training forward, 1008 backward; 4 common readout + 8 arm TRAIN readout. Generate0. Materializer common выполняется только отдельным producer на исходном A и учтён как его setup; измеряемые arm materialization и их factor остаются прежними. До первого build зафиксировать общий deadline **14400s**, preparation≤10800s, каждый build≤600s, каждый arm≤900s, summary≤120s; общий deadline ограничивает сумму, прежние более короткие defaults для этой цепочки заменены. Один wait≤60s — только responsiveness, не срок обучения. Ожидать тот же handle; timeout не разрешает новый run.

**Суммарный учёт producer+consumer, включая наблюдение:** 1002 updates /1008 backward;1016 training +4 common +8 arm DIAG_TRAIN =1028 forwards;Generate0. Эти максимумы не разрешают дополнительные вызовы ради отчёта, проверки hashes или retries.

**Текущий consumer после shared restore:** 2 updates/8 Backward;16 training+8 DIAG_TRAIN=24 Forward; Generate0. Common updates/readouts=0; перед arm нет `--prepare-learned`. Сохранённые producer costs указываются по ссылке и не прибавляются к повторно измеренным arm costs. Конечные сроки выше сохраняются как caps, свободный preparation budget не разрешает дополнительные calls.

**Before/after и границы фактора.** A0/B0 обязаны совпасть с квалифицированным S0 на тех же сохранённых TRAIN predictions и state; обе стороны получают одинаковые carrier/reference. Снять исходные arm before/after в их прежнем бюджете; у retention сохранность относится к приобретённым полным строкам,0→0 не считается сохранностью. При after regression записать её рядом с выигрышем ресурсов. Для cost-equivalence сравнить finite losses/logits/gradients/final weights и DPO или SFT optimizer state по прежней tolerance; qualification не ослабляет эту проверку. Материализация/подготовка, measured arm updates и readouts — отдельные временные/allocations интервалы. Любое положительное измеренное изменение оси при валидном сравнении сохраняется без нового minimum gain.

Источники API: [Trainer:67–147](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:67), [AdamW portable:188–290](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:188), [exact carrier:20–78](C:/Projects/llm_mvp_core/11.DPO/Models/DpoExactPairMaterial.cs:20), [DPO import требует собственного continuation proof:533–560](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:533). Подготовка применяется только отдельным producer из контракта33. Consumer, включая встроенный промпт, имеет0common updates/readouts и выполняет назначенное ядро A/B; суммарный бюджет producer+consumer не является командой повторить подготовку. Модель исполнителя — фактическая default своей сессии без override. Никаких операций текущим документным поручением не запущено.

**Готовность учебной постановки:** DESIGNED_REQUIRES_LEARNING_VALIDATION. Назначенные ниже start/data/schedule не обещают усвоения и не разрешают повторить уже известный нулевой рецепт. При acquisition проверяется приобретённое поведение; retention/restore допускается лишь при показанном навыке до вмешательства. Если сигнал отсутствует, learning objective остаётся OPEN: автор исправляет конкретный start/минимальный набор/бюджет до новой попытки, исполнитель не ищет замену и не добавляет steps до PASS. Один отчёт с INVALID_ZERO_TO_ZERO не завершает задание и не превращает его в успех ресурсов. Прямое NEXT Любы выполняется сразу; иных автоматических повторов нет. [Обязательный контракт30](../30-failure-recovery-and-learning-validity.md).

**Единственное назначение результата DPO-TOKENIZER-SNAPSHOT-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [DPO-TOKENIZER-SNAPSHOT-01](DPO-TOKENIZER-SNAPSHOT-01.md) → [BPE-WEIGHTED-CHUNKS-01](BPE-WEIGHTED-CHUNKS-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Новый будущий TRAIN readout — точный рецепт, не выполненный опыт.** Применить [протокол](../24-neural-learning-signal-protocol.md). Панель: chosen двух уже подготовленных exact DpoBatch; A0/A1/B0/B1. DIAG_TRAIN добавляет 8 policy forwards; Arm optimizer updates/backward/Generate/teacher остаются прежними; оплаченная common preparation и её readouts прибавлены отдельным разделом. Единственная внешняя команда будущего подготовленного host: `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/Run-With-Learning-Signal.ps1`. В этом host подготовить ровно один Run-With-Learning-Signal.ps1 по разделу «Место и способ включения» протокола; прежние команды ниже становятся его внутренними шагами, не второй цепочкой запусков. Observer вписать в точный existing bridge/Program/NativeLearner из таблицы протокола; другой проект/файл не искать. Raw.learning_signal содержит TRAIN NLL/rank/token exact и цену панели. Старые нулевые Evaluate/EXAM counts ниже относятся к ресурсному ядру; новые DIAG_TRAIN counts здесь объявлены отдельно. Протокол не обещает PASS и не разрешает увеличивать steps, менять head/objective/LR или подбирать примеры до успеха.

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Arm updates не добавляются; общий preparation budget явно прибавлен в исправленном разделе. Новый TRAIN readout имеет отдельный объявленный выше DIAG_TRAIN бюджет; прежние нули Evaluate не относятся к нему.

**Учебный сигнал и предел вывода.** На точной TRAIN-панели нового протокола показать NLL, полный gold rank, per-token и teacher-forced response exact до/после; EXAM отдельно. Диагностические forward оплачены объявленным выше бюджетом. Свободная генерация/общее владение задачей не выводятся из этих показателей. Неположительный результат не означает автоматической поломки и не назначает повтор; при positive движении показать регрессии и пределы. Само совпадение A/B или одинаковые 0→0 не заменяет учебного сигнала.

## Действующее локальное назначение — 2026-10-20

Это активная карточка, перенесённая из [входящего исследования](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/DPO-TOKENIZER-SNAPSHOT-01.md>). Гипотеза уже существовала в этом источнике; новая находка текущего агента не заявляется. Текущая фаза — **READ_ONLY_RESEARCH и правка Markdown**. Host, код эксперимента и результаты сейчас не создавались; **DESIGNED_NOT_RUN / HOST_NOT_MATERIALIZED**, сборка и обучение не проверены.

По прямому указанию пользователя внешние ссылки заменены локальными корнями. Назначенный будущий host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01`; назначенный результат: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001`. Эти пути заменяют прежние короткие внешние корни источника; состав native файлов, фактор, Student и бюджет не меняются. Это документационный рецепт будущей реализации, не готовый исполняемый host.

Исполнитель и последовательный проверяющий: **модель текущей сессии по умолчанию, без provider/model/reasoning override**. Исследовательская роль Astra ultra не становится исполнителем runtime; отдельное поручение на исполнение выполняется назначенной сессией по её умолчанию. Student/tokenizer/perceptron описан ниже отдельно. Действительное разрешение на конкретный опыт сохраняется при смене инструмента: повторно просить то же разрешение не нужно. Этот документ сам по себе не разрешает запуск.

Конфликт путей: если назначенный host занят чужими/неизвестными файлами или назначенный result/attempt уже существует, **BLOCKED_PATH_EXISTS; не удалять, не переносить, не выбирать другой путь и не повторять запуск**. Новый attempt допускается только в отдельном прямом поручении с конкретным номером. Рецепт использует локальные Git-объекты указанных pin без fetch/checkout; недоступный объект, несовпавший blob или отсутствующая зависимость — PREPARATION_FAILED до обучения, без замены ветки/модели/проекта. Текущее разрешение записи карточек не разрешает менять production, fixtures, source-copy или runtime. При будущем исполнении исходники и сами карточки читаются; результаты пишутся только в назначенный result.

Навыки будущего исполнителя, точные локальные файлы: [architect-behavior-classifier-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md>), [planning-session-startup-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md>), [phase-planning-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/phase-planning-SKILL.md>), [plan-review-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md>), [code-examination-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/code-examination-SKILL.md>), [generated-code-review-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md>), [prompt-patch-authoring-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md>), [output-readability-gate-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md>), [human-readable-architecture-audit-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md>), [human-communication-GLOSSARY.md](<C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md>). Русский отчёт и конечный шаблон указаны ниже; искать другие skills/формат не требуется.

**STOP/NEXT:** при «следующий эксперимент» немедленно оставить прежний опыт и перейти только к названному следующему, без дописывания старого отчёта, повторного hash/build/CI/commit. При «стоп» прекратить действия; допустимо лишь необходимое прекращение собственного уже работающего процесса, чтобы он не продолжал обучение. Старые опыты и failed attempts сохраняются.


**Статус: DESIGNED_NOT_RUN — спроектировано; host ещё требует реализации.** Выполнен анализ исходного механизма и сопоставление с прежними карточками. Обучение, измерение выигрыша и изменение продукта не выполнялись. Эта карточка — добавленный кандидат, не доказанное объяснение неудачи первого PR.

## Механизм и связь с реальным обучением

Тема — документация программ; задача — описать порядок аргументов вычитания в C#. Учится исходный `TransformerModel.LlmModel`, профиль `rp01-csharp-small/v1`, через q/v-LoRA и AdamW. Изменяется подготовка точных текстовых предпочтений, которые **этот же learner затем потребляет в настоящем StepVector**.

Для каждой пары `DpoExactPairMaterial.CreateBatch` сериализует весь BPE tokenizer в MemoryStream, загружает закрытую копию и вычисляет её canonical SHA256. `LogicalDpoTrainingUnit.PrepareExecution` вызывает этот метод внутри цикла по preference rows. При двух парах с одним неизменяемым источником получаются два Save/Load/hash прохода одного tokenizer. Они сохраняют важную гарантию: кодирование сегментов одной пары нельзя менять внешним изменением tokenizer.

Гипотеза: один **закрытый снимок на одну подготовку** может сохранить точный материал всех пар при меньшей цене подготовки. Удалять саму защиту freeze, кэшировать mutable tokenizer глобально или использовать только указатель на внешний объект нельзя.

### Исходные доказательства

Ветка `codex/dpo-implementation`, commit `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`.

| Source | Участие в учебном переходе |
|---|---|
| [DpoExactPairMaterial.cs:64–90](<C:/Projects/llm_mvp_core/11.DPO/Models/DpoExactPairMaterial.cs:64>) | Проверка текстов, Save→Load, три сегментных Encode, hash, response masks и detached tensors для одной пары. |
| [PrepareExecution:1667–1759](<C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1667>) | Per-pair bounds/authorship/vector checks, затем BuildBatch внутри цикла. |
| [BuildBatch:2351–2380](<C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:2351>) | Typed admission и test observer идут до вызова exact CreateBatch. Порядок нельзя бездумно переставлять. |
| [LogicalDpoPreparedBatchMaterializer.cs:14–21](<C:/Projects/llm_mvp_core/18.CapabilityTraining/LogicalDpoPreparedBatchMaterializer.cs:14>) | Реальный public caller явно выбирает exact material version; generic PrepareExecution по умолчанию legacy. |
| [CodeDocumentationSimulationDpoExperiment.cs:106–125](<C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:106>) | Saved B0 load → programmatic typed authors → governance/content projection → **Prepare**. |
| [Тот же owner:175–189](<C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:175>), [DpoTrainer.cs:832–877](<C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:832>), [ApplyOptimizer:1219–1225](<C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1219>) | Prepared batches действительно поступают в policy/reference forward, backward и исходный optimizer update. |

Изолированное измерение сериализации tokenizer без завершённого учебного перехода не закрывает карточку. При отказе подготовки/lease/update это диагностические данные, а не успешное обучение.

## A/B: ровно один фактор

**A:** исходная закрытая копия + hash на каждую exact pair.

**B:** lazy private `FrozenTokenizerSnapshot` живёт только в одном `PrepareExecution`; после typed admission и исходных CheckText первой допустимой exact pair создаётся один clone с hash. Следующие допустимые пары используют этот закрытый clone. Каждая по-прежнему независимо проверяется, получает собственные IDs/masks/tensors/content digest. Оригинальный путь отдельного CreateBatch и legacy version остаются прежними.

Не создавать снимок в начале PrepareExecution: это перенесло бы токенизацию до отрицательного authorship/text guard. Не менять порядок `BatchTokenizationObserver`. Для активного internal observer сохранить исходный per-pair путь, без reuse; в данном обычном учебном caller он отсутствует. В эксперименте tokenizer принадлежит одному arm, не обучается заново и не изменяется параллельно. Эквивалентность для стороннего concurrent mutation не доказана; это отдельный риск будущего переноса, а не повод выдумывать lock/callback обход в опыте.

Точная спецификация меняемых методов, assembly boundary и неизменных authority calls — [dpo-source-copy-design.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/dpo-source-copy-design.md>). Только копии `11.DPO/Models/DpoExactPairMaterial.cs` и `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` получают treatment. Нельзя одновременно менять reference cache, optimizer exports, gradient копии, objective, response sum/mean, masking, padding, LoRA rank или training vector.

## Данные и неизменяемый learner

Точные две JSONL строки — [fixtures/dpo-tokenizer-snapshot.jsonl](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/fixtures/dpo-tokenizer-snapshot.jsonl>), UTF-8 без BOM, с одним конечным LF. SHA256: `91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95`. Первая задача `first-second`, вторая `second-first`; chosen/rejected меняются местами вместе со смыслом. Перед префиксом или после `Code: ` ничего не добавлять. Никакие id/provenance/labels не поступают в модель как текст.

В обоих arms **одно** `Prepare(preferencesOfTwo, tokenizer)`, затем **одно** `StepVector(allTwo, step:1)`. Последовательные два Prepare по одной строке уничтожили бы проверяемую экономию. Четыре предъявления пар и два optimizer updates суммарно A/B; оба arms начинают с одной квалифицированной S0 после common preparation; независимый frozen reference остаётся исходным B0.

| Параметр | Точное значение |
|---|---|
| Канонический источник | `C:/Projects/llm_mvp_core`; `tools/Rp01SimulationDpo/Program.cs:307–332` ReadConfiguration + LoadModels, без вызова большого Run |
| Конфигурация | `config/code-documentation/generation-v1.json`, hash файла `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40` |
| Начальные веса | B0 SHA256 `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911` |
| B0 manifest | `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`, Step1, ParentCheckpoint=null, InitialBaselineNoUpdate |
| Tokenizer | canonical SHA256 `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`; vocab629 |
| Transformer | context2048, hidden512, layers4, heads8, kvHeads2, FFN1408, RoPE10000, RMS epsilon1e-5 |
| LoRA | rank8, alpha16, dropout0, q_proj/v_proj; 55 named parameter tensors: 39 base +16 adapters |
| Seeds | root initialization20261014; `rp01-policy` / `rp01-reference`; реальные RNG cursors сохраняются отдельно |
| Optimizer/objective | исходный CreateForProjectLora, LR1e-5, DPO beta0.1, response-sum-softplus-signal-weighted-v3; фактические AdamW beta/epsilon/decay записать без переопределения |
| Исходный optimizer | новая DPO-сессия над S0, step0 и пустые DPO моменты; не выдавать manifest Step1 за optimizer step1 |

Источники конфигурации: [generation-v1.json](<C:/Projects/llm_mvp_core/config/code-documentation/generation-v1.json>), [constants:14–17](<C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/Program.cs:14>), [saved-root loader:307–332](<C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/Program.cs:307>).

Physical B0 path берётся только из проверенного ParentManifest.RepositoryRelativeWeightsPath через этот loader. Если файл/config/binary boundary отсутствует или identity не совпадает, сохранить ошибку и завершить назначенную попытку. Не искать соседний checkpoint, не уменьшать модель, не обучать random fallback, не заменять B0 C50. Доступность Windows файлов здесь не проверялась.

## Фазы и честные показатели до/после

Перед MeasurePrepare tokenizer и models загружены одинаково; материализация измеряется отдельно от загрузки. В A0/B0 записать named policy/reference hashes, optimizer step0/state bytes, RNG state, tokenizer hash. Native Prepare измерить один раз; его результат напрямую передать единственному StepVector. В конце сохранить A1/B1 и reopen через исходные codecs без нового forward.

| Показатель | Как считать |
|---|---|
| Primary полезная стоимость | Allocated bytes текущего потока, CPU, elapsed **за единственный Prepare**, внутри той же completed learning attempt. A имеет два capture, ожидаемый B — один. Счётчик capture — объяснение работы, не сам полезный результат. |
| Полный учебный переход | Load/materialize, полный StepVector и native checkpoint/readback — отдельные фазы; полный elapsed/CPU/allocations дополнительно. Нельзя складывать вложенную фазу с её родителем. |
| Фактическое обучение | optimizer step0→1; actual keyed LoRA delta, включая возможный ноль; base/reference до/после неизменны; реально completed StepVector, не только issued. Нулевой численный delta записывается как отсутствие изменения весов, а не выдуманный рост качества; он не отменяет реально исполненную работу optimizer. |
| Сохранность точных входов | Два material digests, tokenizer SHA, lengths, actual IDs и masks A/B; bounds/authorship/response-only проверки должны пройти. |
| Восстановление | Записанные private candidate weights/optimizer/RNG прочитаны теми же codecs и равны live состоянию; это readback одного update, не новая continuation-серия. |
| Учебная метрика DPO | Вернуть actual loss/margin из StepVector с подписью **измерено до применения этого update**. Это не оценка A1/B1. |
| Прочее | Peak working set процесса, GC counts, actual runtime/build; accuracy документации, held-out ответы и throughput полного продукта — «не измерено». |

В measured kernel остаются два StepVector. Consumer дополнительно содержит только8 arm TRAIN scores; его common updates/scores=0. Однократный отдельный producer содержит1000 SFT updates и4 common scores; других Generate/Evaluate/teacher/scoring/warmup/retry нет. Не добавлять evaluation, чтобы задним числом назвать pre-update loss послеучебной точностью. Цена самого фактического учебного перехода и изменение learned state — валидные показатели этого cost-опыта.

Любое измеренное улучшение полезного показателя считается успехом по этой метрике и теме; точность не обязана вырасти. Рядом показать все ухудшения и расхождения состояния. При одинаковых входах и completed updates уменьшение materialization allocations — самостоятельный успех, даже если полный wall-time шумный или вырос. При нарушенном начальном равенстве/материале или недошедшем update causal comparison недействительно. Отказ раньше обучения не является ускорением обучения.

## Доменные пределы

Механизм зависит от числа exact pairs и размера tokenizer, а не синтаксического дерева. Прямой опыт сейчас только C# documentation B0. Потенциально полезен exact DPO других тем при общей неизменяемой tokenizer snapshot, но этого результата пока нет. При одной паре ожидаемой экономии нет; при legacy framing этот B вообще не активируется. Общая стоимость обучения может почти не измениться, если доминирует transformer/backward/serialization weights. Не объявлять CPU прогноз измеренным выигрышем.

## Место, будущие команды и конфликты

Будущий короткий пакет — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01`; проект — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/Runner.csproj`; оба arms — два настоящих .NET child процесса и один controller без обучения. Подготовка конкретных файлов и binding указаны в source-copy design. После реализации в отдельно порученной стадии:

```powershell
$ErrorActionPreference = 'Stop'
dotnet build C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/Runner.csproj -c Release -p:Variant=A
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build A; training not started; no retry' }
dotnet build C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/Runner.csproj -c Release -p:Variant=B
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build B; training not started; no retry' }
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/bin/A/Release/net9.0/TokenizerSnapshotRunner.dll run-pair --peer C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/bin/B/Release/net9.0/TokenizerSnapshotRunner.dll --fixture C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/in/train.jsonl --attempt 0001
if ($LASTEXITCODE -ne 0) { throw 'ATTEMPT_FAILED: preserve partial artifacts; no retry' }
```

Host пока отсутствует: это **спецификация будущего интерфейса, не READY_TO_RUN**. Результат — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/report.md`; raw/status/checkpoints A и B рядом. Directory/FileMode.CreateNew; существующий attempt или uncertain update не удалять/не повторять/не обходить новым RunId. Новая попытка допускается только отдельным прямым поручением. При отсутствующих исходниках/dependencies ошибка подготовки сохраняется до обучения; не запускать полный solution для её «исправления». Во время будущего опыта исходные карточки read-only; результаты пишутся только в назначенный result-root.

## Исполнитель, навыки, отчёт, STOP/NEXT

Исполнитель и последовательный reviewer используют **модель текущей сессии по умолчанию**; новый provider/model не искать. Это не обучаемая Student: её B0/profile отдельно зафиксированы выше.

Навыки из `C:/Projects/llm_mvp_core/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Перед новым аудитом или заданием заново и полностью прочитать стек договора120 и навыки применимой роли. Внутри того же задания ожидание и оформление отчёта сами по себе не требуют повторного полного чтения. Исходное разрешение эксперимента действует для изолированной native копии, а не для prod/submitting. Текущий READ_ONLY_RESEARCH ограничивает работу документами; старые full CI/продвижение/правка всех тестов не становятся этапом этой карточки.

Русский report.md, до 350 слов пояснений плюс таблицы raw. Разделы: «Что меняли», «На чём проверяли», «Что получилось», «Выигрыши и ухудшения», «Граница вывода и кандидат исправления». Указать A0/A1/B0/B1, стоимость фаз A/B/B−A с единицами, actual issued/completed и все не измеренные величины. При полезном measured улучшении добавить `candidate.md` до 120 слов с точным source method и ссылкой на raw; только Люба решает перенос. Проверяющий не повторяет обучение.

**Запрет продолжать прежнее поручение после переключения обязателен.** При прямом STOP немедленно прекратить действия, без нового tool call, памяти, оформления или «последней» проверки. Допускается только необходимое прекращение уже работающего собственного процесса, чтобы он не продолжал обучение; не cleanup, readback или новый запуск. При NEXT прекратить прошлый опыт и перейти к назначенному следующему, не чинить прошлые tests/report/registry, не пересчитывать hashes и не повторять learner. Если ID следующего опыта отсутствует — сообщить статус и спросить ID, не возвращаясь к прежней работе. При обычном завершении один readback для фактических ошибок и ссылок; без циклов полировки, full CI, commit/push или автоматического product promotion.



## Встроенный точный рецепт подготовки

Рецепт перенесён из [исходной спецификации](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/dpo-source-copy-design.md>); выше действует локальное назначение путей.

# DPO-TOKENIZER-SNAPSHOT-01: точная граница native source-copy

**DESIGNED_NOT_RUN.** Это документационная спецификация; перечисленные проекты, bridge и runner ещё не реализованы. Текущее поручение их не создаёт и не запускает. Карточка [DPO-TOKENIZER-SNAPSHOT-01.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/DPO-TOKENIZER-SNAPSHOT-01.md>) задаёт единственный фактор, две учебные строки, B0 и измерения.

## Копируемые библиотеки и assembly identity

Источник — `lubochka/llm_mvp_core@ab583032562d6656de9dd5ca6fa0a325fd4b1f30`; канонический checkout только для чтения — `C:/Projects/llm_mvp_core`. Пакет — **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01**. Три **полные** библиотеки скопированы в будущий design потому, что preparation находится в CapabilityTraining, материал — в DPO, а native issuers — в ContinuousLearning:

| Исходный проект | Полный набор C# | Назначенный проект копии |
|---|---:|---|
| `11.DPO/DPO.csproj` | 14 файлов | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/lib/DPO.csproj`, AssemblyName DPO |
| `18.CapabilityTraining/CapabilityTraining.csproj` | 13 файлов | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/lib/CapabilityTraining.csproj`, AssemblyName CapabilityTraining |
| `14.ContinuousLearning/ContinuousLearning.csproj` | 81 файл | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/lib/ContinuousLearning.csproj`, AssemblyName ContinuousLearning |

Полный список 108 source blobs — [source-copy-manifest.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/source-copy-manifest.json>). Все upstream файлы сохраняют имена, namespace и license headers в `common/DPO`, `common/CapabilityTraining`, `common/ContinuousLearning`. Ни одного `Compile Include` прямо в original checkout. Не копировать один DpoTrainer в другой executable и не давать executable имя friend library.

Три library projects используют .NET9, Nullable/ImplicitUsings как исходные проекты. AssemblyName/AssemblyVersion/Culture/PublicKeyToken копий должны совпасть с реально доступными исходными DLL; исходные friend declarations DPO и ContinuousLearning сохраняются, новые friend grants не добавляются. Недоступная strong-name signing identity — конкретная ошибка подготовки; подделка запрещена.

`Variant=A|B` выбирает только свои replacements для `DpoExactPairMaterial.cs` и `LlmIngredientLogicalUnits.cs`; исходные версии этих двух файлов исключаются из common Compile items, чтобы не получить duplicate type. Всё остальное одинаково. Никакой прочий source diff не входит в B. Common bridge — новый `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/bridge/TokenizerSnapshotExperimentBridge.cs`, компилируемый **в ContinuousLearning**. Host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/Runner.csproj` с AssemblyName `TokenizerSnapshotRunner` содержит только `Program.cs` и normal ProjectReference на три копии.

Пути MSBuild абсолютные: у host `OutputPath=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/bin/$(Variant)/Release/net9.0/`; у library projects `OutputPath=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/libbin/$(Variant)/$(MSBuildProjectName)/Release/net9.0/`. Все четыре проекта задают `AppendTargetFrameworkToOutputPath=false` и `AppendRuntimeIdentifierToOutputPath=false`, иначе получится дополнительный net9.0 и конечные команды не найдут DLL. RuntimeIdentifier пустой, Configuration=Release; неподдерживаемые RID/Variant/Configuration — ошибка подготовки. `BaseIntermediateOutputPath` и `MSBuildProjectExtensionsPath` равны `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/obj/$(Variant)/$(MSBuildProjectName)/`, задаются до SDK imports. ImportDirectoryBuildProps/ImportDirectoryBuildTargets отключены. ProjectReference сохраняет Variant и копирует свои библиотеки в назначенный host output; output одного library project не делится с другим.

### Заранее выбранный binary dependency boundary

Ровно `C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/bin/Release/net9.0/`. Пакет копии ссылается обычными Reference/HintPath на следующие неизменные DLL; **TrainingLoop здесь не меняется**, в отличие от старого N62 host.

```text
27.TranslationDomain.dll; 44.DesignConversionDomain.dll;
AgentAlgorithms.dll; AgentInfrastructure.dll; AutonomousLearning.dll;
Core.dll; DatasetPipeline.dll; DynamicDomains.dll; Embeddings.dll;
Inference.dll; LoRA.dll; Microsoft.CodeAnalysis.CSharp.dll; Microsoft.CodeAnalysis.dll;
Microsoft.Data.Sqlite.dll; ModelServer.dll; SemanticCompiler.dll;
SemanticCompiler29.dll; SemanticCompilerAdapter.dll; SQLitePCLRaw.batteries_v2.dll;
SQLitePCLRaw.core.dll; SQLitePCLRaw.provider.e_sqlite3.dll; TensorCore.dll;
Tokenizer.dll; TrainableRouter.dll; TrainingLoop.dll; TransformerModel.dll; VectorSearch.dll
```

Нельзя ссылаться на исходные **DPO.dll, CapabilityTraining.dll, ContinuousLearning.dll** или Rp01SimulationDpo.exe/dll. Первые три замещают полные copy projects. Исходные executable deps/runtimeconfig не копируются как конфигурация нового host. Никаких product ProjectReference, package upgrade, solution rebuild или fallback на другой bin.

При однократной подготовке dependency-manifest фиксирует file paths/SHA/PE assembly identity. После ResolveReferences все `ReferencePath` и `ReferenceCopyLocalPaths` для трёх заменяемых identities должны указывать только на output соответствующего local ProjectReference нужного Variant. Старый transitive DLL — `PREPARATION_FAILED_ASSEMBLY_BOUNDARY`. Не чинить через AssemblyResolve/reflection после старта. Фактическая доступность этих Windows DLL, ABI и custody B0 в текущем документационном проходе не проверялась.

## Однофакторная patch specification

### DPO/Models/DpoExactPairMaterial.cs

В B ввести **internal opaque** `FrozenTokenizerSnapshot` с private readonly BpeTokenizer clone, private readonly canonical hash и vocab size. Ни property возвращающего BpeTokenizer, ни Save/Train mutator у carrier нет. Закрытый snapshot создаёт только существующая exact-material область DPO через исходные Save→Load→ComputeStateSha256. Это новый API **копии**, не уже существующий product API.

Сохранить исходную CreateBatch(BpeTokenizer, prompt, chosen, rejected, admission) для независимых callers: она продолжает capture на один вызов. Добавить internal overload с `ref FrozenTokenizerSnapshot? invocationSnapshot`. Он в исходном порядке выполняет CheckText и chosen≠rejected; только после этих guard делает lazy capture при null. Encoding, lossless checks, encoded inequality, DpoExactPairMaterial construction, detached tensors/masks, Validate выполняются заново для каждой строки. Snapshot не содержит IDs/answers/admission прошлого примера и не становится межсессионным cache.

### CapabilityTraining/LlmIngredientLogicalUnits.cs

В B/PrepareExecution локально определить nullable invocation snapshot; lifetime заканчивается с возвратом этого вызова. Legacy route прежний. Передать его по ref только через private BuildBatch к exact overload.

BuildBatch сначала сохраняет defined authors и `DpoTrainerPairCustoms.CreateAdmitted`, затем **исходный** `BatchTokenizationObserver.Value?.Invoke`. Если observer активен на этой строке, использовать исходный per-pair CreateBatch и **сбросить invocationSnapshot**: следующая строка не должна reuse состояние до потенциальной мутации через observer. Если observer отсутствует, вызвать новый overload. Snapshot создаётся на первом действительно дошедшем до CreateBatch valid exact row, а не до per-pair guard.

Никаких перемещений guard, external callback в DPO encoder, снятия capacity/authorship проверки, глобального Dictionary cache или изменения material version. Два source replacements B содержат только этот reuse; helper refactor общий для A/B не должен нагрузить A дополнительным capture/serialization. Инструментировать Save/Load capture count одинаково, ровно в реальной ветке создания snapshot.

Ожидаемая эквивалентность относится к owner-exclusive неизменяемому tokenizer. При произвольной внешней одновременной мутации старый код мог видеть разные snapshots в разных парах; новый захватывает одну. Это явно ограниченный контракт кандидата, не доказанное поведение всех внешних callers. В будущий prod перенос нельзя молча заявлять глобальную equivalence.

## Тонкий bridge: настоящий learner и исходные authority calls

Назначенный новый тип копии: `ContinuousLearning.TokenizerSnapshotExperimentBridge`, один public Run для immutable request с fixed ID/arm/input/output. Он компилируется в настоящей копии ContinuousLearning и использует существующие internal issuers, без reflection или новых permits. Program только проверяет exact CLI/compiled Variant и вызывает bridge. Не исполнять `CodeDocumentationSimulationDpoExperiment.Run`: его существующие main/calibration guards допускают 500/1 rows, а наш опыт — отдельный finite caller из двух строк.

Порядок bridge уже определён:

1. `CodeDocumentationSimulationDpoExperiment.Inspect(inputPath, fixtureSha, 2, SimulationDpoDomain.CodeDocumentation)`. Native duplicate/tie/text/provenance checks прежние.
2. Перенести в общий bridge **только** ReadConfiguration/LoadModels из `tools/Rp01SimulationDpo/Program.cs:307–332`. Root `C:/Projects/llm_mvp_core`. `ModelServer.CodeDocumentationGenerationConfiguration.ReadVerified` получает функцию env lookup: ENABLED=true, CONFIG_SHA256 из карточки, остальные null; глобальный environment не меняется. Два `TrainableRouter.Training.CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart` дают policy и отдельный reference. Все B0/Step1/no-parent/InitialBaselineNoUpdate/55/39 guards сохраняются. Checkpoint path берётся из manifest; произвольный файл не ищется.
3. Reference параметры RequiresGrad=false. `TrainingDataGovernanceDecision(true,false,["sha256:"+fixtureSha])`; исходные rich DpoPair с domain code-documentation и обеими `DpoPairSideAuthorKind.DeterministicGenerator`. `TrainingDataGovernanceAdmission.Validate` → `DpoPairTrainingContentAuthority.Project`. Никаких external teacher licences/passports не выдумывать.
4. Создать две LogicalDpoPreferenceExample из тех же трёх текстов и typed authors. Отмерить **один** `LogicalDpoPreparedBatchMaterializer.Prepare(preferences, tokenizer)`. Этот результат передать в training, не пересоздавать через вторую materialization. `DpoExactPairMaterial.CaptureAndValidate` для двух результатов выполняет исходную validation; его стоимость отдельная от MeasurePrepare.
5. Получить существующие attached q/v `LoraLinear`; `DpoProjectLoraRecipient.Capture`; `DpoExecutionProfile.Capture`. `LegalTrainingTeacherAdmissionAuthority.Create([],0,RandomNumberGenerator.GetBytes(32))` → `ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue` с исходными Validate/Seal/Verify callbacks → `AcquireCodeDocumentationTrainingLease(content, "DPO-TOKENIZER-SNAPSHOT-01-0001:"+arm)` → `IssueDpoTrainerMutationAdmissionCapability`. Non-null capability передаётся тренеру. `live=false` в finally и Dispose lease остаются.
6. RNG topology/loraIdentity берутся из `LoraRandomStateIdentity` исходного config/seeds. Приватный `<arm>/checkpoints` через SecureLocalFileBoundary. Observer сохраняет ownership receipts и в BeforePolicyFinalMove вызывает исходные RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack / RepositoryOwnedDpoRandomStateFile.WriteCreateNew; null RNG — отказ. Не менять DPO transactional publication/rollback.
7. Создать **один** `DpoTrainer.CreateForProjectLora(policy,reference,checkpointRoot,mutation,recipient,profile,null,observer,captureRng,restoreRng)` на arm. Null — отсутствие DPO continuation proof нового DPO epoch над подготовленным S0; mutation non-null, оба RNG callbacks non-null. Ни нового optimizer, ни подмены softplus.
8. Записать A0/B0: named policy/reference bits, initial optimizer.Export bytes с step0, RNG. Для шага0 не вызывать native optimizer-file writer, который предназначен для положительных checkpoints. Прямо перед mutation записать issued, проверить optimizer.Step==0 и вызвать **ровно один** `trainer.StepVector(batches,1,timing)` с двумя rows. Только returned committed call отмечается completed.
9. Reopen private checkpoint через точный typed weights API из binding выше и LoadCheckpoint с Position=0 в третьем Load из того же B0, optimizer OpenExisting, random OpenAndValidate/RestoreRandomState. Сохранить A1/B1, equality saved/live, unchanged base/reference. Generate0; выполнить только назначенные arm TRAIN readouts,8всего. Нулевой LoRA delta записывать честно как observed no-change; он сам не является основанием признать вызванный optimizer несуществующим. Строгий guard нулевого delta из большого owner:205–207 в bridge не копируется; это одинаковое observation правило обоих arms, не treatment.
10. Из recorded JSON сформировать сравнение и закончить. Другие методы/домены не исполняются только потому, что их DLL присутствуют в boundary.

## Конечный CLI, попытки и отсутствие автоматического продолжения

Будущие файлы: `Runner.csproj`, `Program.cs`, три lib projects, common источники/bridge, A/B replacements, `in/train.jsonl`, source/dependency manifests. Результаты — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/{A,B}/`; общий `report.md`, `comparison.json`, `candidate.md` при useful gain. Максимальная вложенность не включает длинное имя репозитория/эксперимента.

Будущий `run-pair` с exact CLI из карточки сначала создаёт единый `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/intent.json` с factor/source/fixture/B0/config/optimizer identities, A→B, two rows/one update per arm, noRetry=true. После этого запускает A и B child один раз; `run-arm --arm A|B --output <exact arm dir>` — private finite subcommand этого же host. Каждый arm отмечает issued/completed create-new. Controller не обучает и не создаёт третий arm.

Существующий intent/arm/issued marker означает `ATTEMPT_EXISTS_NO_REPLAY`. Нельзя обойти отказ новым RunId, удалить каталог, переписать старый status или повторить «для отчёта». Ошибка before optimizer → training not started; прерывание issued без completed → effect UNKNOWN до анализа существующего журнала, без нового update. Raw/partial artifacts сохраняются. Перезапуск с новой attempt identity требует отдельного прямого поручения; текущая документационная стадия не создаёт никакую попытку.

Если binary/source/config path не существует, отсутствие фиксируется в report подготовки, а не превращается в поиск альтернативного проекта, checkpoint или изменение product. Build касается только четырёх локальных проектов после будущей реализации. Полный CI, test repair, production promotion и изменение исходных карточек во время будущего опыта не входят в опыт.



## Замороженные данные этого опыта

Существующий локальный файл: [fixture](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/fixtures/dpo-tokenizer-snapshot.jsonl>). SHA256 байтов: `91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95`. Исходный файл read-only; будущая копия должна совпасть с этими байтами. Никакая генерация данных не требуется.

```jsonl
{"id":"D-T1","dataset_version":"dpo-tokenizer-snapshot-01-v1","domain":"csharp","pair_kind":"CODE_TO_DOC","provenance":"simulation","prompt":"Explain in one short English sentence what the method returns. Describe argument order. Code: int Delta(int first,int second){return first-second;}","chosen":"Returns the first argument minus the second argument.","rejected":"Returns the second argument minus the first argument."}
{"id":"D-T2","dataset_version":"dpo-tokenizer-snapshot-01-v1","domain":"csharp","pair_kind":"CODE_TO_DOC","provenance":"simulation","prompt":"Explain in one short English sentence what the method returns. Describe argument order. Code: int Delta(int first,int second){return second-first;}","chosen":"Returns the second argument minus the first argument.","rejected":"Returns the first argument minus the second argument."}
```


## Замкнутый список native исходников

Источник manifest: [локальный manifest](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/source-copy-manifest.json>); SHA256 `bff7ccbd4f26bb0fe83511daebccc9055f705e49ece3f842c61689ab938553b6`. В текущем чтении Git-объекты всех 108 строк сверены с pin `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`; несовпадений нет. Это проверка происхождения, не сборка.

| Native путь относительно C:/Projects/llm_mvp_core | Git blob SHA-1 |
|---|---|
| `11.DPO/DpoConvergenceMonitor.cs` | `df774e2954db846ba8694778783ac6b9299e93d4` |
| `11.DPO/DpoDataset.cs` | `008da881258dd18d4975058d2f13ea9a5a35ac66` |
| `11.DPO/DpoMutationAdmissionCapability.cs` | `4d2516952dea3d3a37a23fb096bf8678293af546` |
| `11.DPO/DpoStepVectorTiming.cs` | `69c121c135921d4cbfc6c77709094f3872c48ea8` |
| `11.DPO/DpoTrainer.cs` | `0a7fd4d088fd37824aa213ddf0c6189675fe7dc2` |
| `11.DPO/DpoTrainerStepSnapshot.cs` | `8fd397889326466ca1aba711aaf78e86acfa5373` |
| `11.DPO/DpoTrainingReproducibilityContext.cs` | `24506293990968f61014cd2a6d4f3c03cd7a95e0` |
| `11.DPO/DpoTrainingSeededLoraTopology.cs` | `5e4a891bc01e63a48a23f6457ede29e23de6e477` |
| `11.DPO/Models/DpoBatch.cs` | `d48c9a8da3ce42d8cf8b5f51edca3dc32bd12e02` |
| `11.DPO/Models/DpoDatasetRow.cs` | `f2552ccf269f918f9d16cdd982c344f37465c65c` |
| `11.DPO/Models/DpoExactPairMaterial.cs` | `927faaab8ee086db1aa578f7c29598b291f9d777` |
| `11.DPO/Models/DpoMetrics.cs` | `f21a48f0e998cd7ce48daea282352622e73e11ef` |
| `11.DPO/Models/DpoPair.cs` | `497a9dd14ccba23737792ca5eb49c19b2e03f4bf` |
| `11.DPO/Models/DpoTrainerPairAdmission.cs` | `9da211a523ae7428555c9deae52cdeac90e678f9` |
| `18.CapabilityTraining/CapabilityResultTranslator.cs` | `954b87292d3d128c452a19f6171ac49ba9bada78` |
| `18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs` | `e16657e91d4b0f201aef003f473cd8811bf87893` |
| `18.CapabilityTraining/GeneratedCapabilityExecutableManifestProjector.cs` | `311d1e08813d2fb51f0a6e32664621ee57362dc1` |
| `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` | `2f91fce55c711063aa41780a53ef62b08f337dc8` |
| `18.CapabilityTraining/LogicalCapabilityFileCustody.Posix.cs` | `e81f3f107daaa1bad3d1ba5e015693dc921d4fa9` |
| `18.CapabilityTraining/LogicalCapabilityFileCustody.Windows.cs` | `3c833c68b7aaacb653917ef941ae3e42b5ed3f04` |
| `18.CapabilityTraining/LogicalCapabilityFileCustody.cs` | `2ddd613078dc9ea628da5299618342c10bf58b90` |
| `18.CapabilityTraining/LogicalDpoPreparedBatchMaterializer.cs` | `bd2ebe14d245ad711876d7b2a9f6c987e484401b` |
| `18.CapabilityTraining/LogicalExperimentOptimizations.cs` | `90fa781d31145e5141a22c8a765ebf25ce3737fc` |
| `18.CapabilityTraining/ReplayFusionRanker.cs` | `fd2c1d657a38ea485136fb8e875aceb8f4b16a23` |
| `18.CapabilityTraining/RuntimeProvenance.cs` | `8e01e533dc87dc5f075dceb55eeccef8cf26255e` |
| `18.CapabilityTraining/TeachingRegistryPersistence.cs` | `26155ca4e1960041c5991b72c5a700182c7e0d17` |
| `18.CapabilityTraining/TeachingRegistrySnapshot.cs` | `7833ade1804fe44661976bfed0250a7c5d76a817` |
| `14.ContinuousLearning/Cache/DomainDatasetCache.cs` | `6bc9ab000f830b3175da489f3034725a75258c92` |
| `14.ContinuousLearning/Core/AdapterArtifactReference.cs` | `a845bad602ae1a9d1bc513a6cb12f39f43b2c85d` |
| `14.ContinuousLearning/Core/Core.cs` | `c4c0977721ca0526dd2643596bde184ab74b7627` |
| `14.ContinuousLearning/Core/DpoPairReferencePreferenceMatchingRule.cs` | `41db0ac760a654a0be7efbee9c73645318818342` |
| `14.ContinuousLearning/Core/DpoTrainingGovernance.cs` | `df495b6a8d9915cb9722facd9aba67d7f075409b` |
| `14.ContinuousLearning/Core/FrozenReferenceCorpus.cs` | `a4db1e1cad34280124c67f339324377a157ceb1b` |
| `14.ContinuousLearning/Core/HeldOutScoring.cs` | `72b092d4477a67353794e7381875b43381a08e56` |
| `14.ContinuousLearning/Core/IServerOwnedDpoTrainingLeaseBroker.cs` | `10671d46865687ea033e56962c68987ad6142b30` |
| `14.ContinuousLearning/Core/LegalTrainingTeacherBoundary.cs` | `f0dec88638bcdca6d020e98a91433c6f9080c79f` |
| `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs` | `08fb5ff9d7aacd990f644df2874c9381a94b5668` |
| `14.ContinuousLearning/Core/LogicalCapabilityReplayHmacCodec.cs` | `3aaf46ca4db8512ffe9b3091dc63a188b3acb3ec` |
| `14.ContinuousLearning/Core/MeasurementNoise.cs` | `9225e9a425161452a23ed22af30e2fc126363e31` |
| `14.ContinuousLearning/Core/NativeCapturedReferenceProvenance.cs` | `9ab608a65df4bdd7d2ccd21e72014045bb76d30a` |
| `14.ContinuousLearning/Core/ServerOwnedLegalTrainingTeacherAdmissionAuthority.cs` | `5b1e370b66d5696bd57e7557a413974c17759d0d` |
| `14.ContinuousLearning/Core/ServerOwnedLogicalCapabilityReplayVerificationComposition.cs` | `dbd17b92304aade5b016f8df4eea02536c9e5401` |
| `14.ContinuousLearning/Core/TrainingProvenance.cs` | `0609cf39b4ab13f309c3730dfc0526c564f563d9` |
| `14.ContinuousLearning/Fabric/Interfaces.cs` | `d3a282522de184e0d7220a012f446a7909e1de7d` |
| `14.ContinuousLearning/Feedback/ContinuousLearningDpoPairStores.cs` | `d47c17cec429beb29a70d751549d3e7320959416` |
| `14.ContinuousLearning/Feedback/FeedbackRouter.cs` | `a676058b1ffcaa19dd6385608ec8f1ef242b3822` |
| `14.ContinuousLearning/Feedback/FileDpoPairStore.Transactions.cs` | `a46a003a986ab395b342f8fe33f91156b4f583c5` |
| `14.ContinuousLearning/Feedback/FileDpoPairStore.cs` | `40e1d2c539b446bd1f70ae1aeca06796096a3706` |
| `14.ContinuousLearning/Feedback/SecureLocalFileBoundary.cs` | `f0ddbdd125a1181f50a2c4ed5050759e97c075f6` |
| `14.ContinuousLearning/Indexer/ProjectIndexer.cs` | `1da9a745050206f1acd6a3989e3601984ad9ed13` |
| `14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs` | `24ea6a7a341e49b2756220bf0848113fc9b1597f` |
| `14.ContinuousLearning/Registry/DomainModelRegistry.cs` | `a6e2f41cf414374d3c6ef72014724734e8fec6e2` |
| `14.ContinuousLearning/Training/CanonicalSemanticCompilerCache.cs` | `348bce720558bb3ace44eedeb531e0efd3e82de9` |
| `14.ContinuousLearning/Training/CodeDocumentationCertificationAdmission.cs` | `886eb8b1bbb88150a9d3ba6e696d355f9f1a90bb` |
| `14.ContinuousLearning/Training/CodeDocumentationCertificationSessionStore.cs` | `590b3ef5f0d73e7d933eccdf9e80e61c5f7c83ee` |
| `14.ContinuousLearning/Training/CodeDocumentationMaterialBoundary.cs` | `63cdd0ca9162de3da50576cf1a9abae27dac9ef2` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonCheckpointSelection.cs` | `9634146e8f74e7b6ebbd5139c795ad650743fd5f` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonCorpusSelection.cs` | `2bc17ac16b04fede1c14086f801934e0037a021e` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonEvaluationPlanSelection.cs` | `35a0d1f1398da3ad57e7f43f138501537d0f8f49` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonEvaluatorSelection.cs` | `85a087d7a664e8becb3202c831ee06db5931e0e5` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonGrantCatalog.cs` | `b4a77717cdd04425584dd1939bea3e7b29f1cf41` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonProfileSelection.cs` | `0a613dc594ffd3e84fabfec157727b1356032782` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonSelectionBindingWire.cs` | `d14c85378e68ef9dc0b22b2c56629bcca949b125` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonSourceSelection.cs` | `1a54153cb6ee27f03fd04ebc8ca4b0edc3e18521` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonStartupSelectionDecoder.cs` | `e9f82f9fa50407b34c8ae2e8ec137adcb1eb97e3` |
| `14.ContinuousLearning/Training/CodeDocumentationOneLessonTeachingDisclosureApproval.cs` | `b687e290861092a86cfd7b4cfe40692db3ee6fc4` |
| `14.ContinuousLearning/Training/CodeDocumentationPrivateEvaluationStore.cs` | `06f8d1a5bc86ceb24f9cb70aabf17018565e42e2` |
| `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs` | `60f730102bc106a4ebdf6c7b86104cba0d27462a` |
| `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs` | `a1fe1ea187045d646ab616da27baeef6ee98ebc8` |
| `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs` | `b631458a41b9c88124160fdf971a00cf8dc58f77` |
| `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedPreparedBaselineJournal.cs` | `32b50ef5e0a6bffe6bfe78e8fc47cbda29916db8` |
| `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs` | `1f389cc5d7549d4944e569592693afe629f2722f` |
| `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | `6bac33463f695ab1c1c87517c9c646a8e9454a1d` |
| `14.ContinuousLearning/Training/CodeDocumentationServerOwnedDpoTrainingExecutor.cs` | `7a6f967500cecc630704d6297d7fda337f79d199` |
| `14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs` | `0c12d8d5ff5f4c68fe879d6d7867e3e666e5af1f` |
| `14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs` | `51af64cd2b3345ec53dc6901fa523476542df8e4` |
| `14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs` | `e200ce5b4f113d97c9929c692730e242b9ea50f6` |
| `14.ContinuousLearning/Training/ConfiguredQwenVocabularyLease.cs` | `3593e3cfc0aad5c5e6e6e8f6a6fe2eeef37ccb3f` |
| `14.ContinuousLearning/Training/ContinuousLearningModelServerComposition.cs` | `4a3f7f5d716fde1537ad0d81b8d8601b131786b7` |
| `14.ContinuousLearning/Training/ContinuousLearningProductionComposition.cs` | `0700fa0dd5ce9dba242011f4c14c1bd11dd7c50b` |
| `14.ContinuousLearning/Training/ContinuousLearningTp1EvaluationComposition.cs` | `77c26fd412ecebe4cb7afb75331a930aea2f3ef4` |
| `14.ContinuousLearning/Training/DpoTrainingExecution.cs` | `987abd2caca156f5290b42df6f7202401edb0ad5` |
| `14.ContinuousLearning/Training/IncrementalTrainer.cs` | `3c7e3dcae8cfd17fedc317a2ff3f6d62ac526ad2` |
| `14.ContinuousLearning/Training/IncrementalTrainingSession.cs` | `542d7dd8167af8f631dddae3cea3dc41575d33a2` |
| `14.ContinuousLearning/Training/LocalQwenOllamaTeacherInferenceClient.cs` | `9e5d1ff98afea78b2023fc52eaaba08600b84168` |
| `14.ContinuousLearning/Training/RepositoryOwnedDpoDatasetManifest.cs` | `49e6972716a9cce649518008480e56bebb03de1e` |
| `14.ContinuousLearning/Training/RepositoryOwnedDpoDatasetResolver.cs` | `37787904cefcf8533d11c6f0ea31f14a31f3a5cd` |
| `14.ContinuousLearning/Training/RepositoryOwnedDpoOptimizerStateFile.cs` | `792ba8f82f53d03c64a20fe5793d9dc578c66067` |
| `14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs` | `36e8467291f04703b5f0f486735850d4b01153c0` |
| `14.ContinuousLearning/Training/RepositoryOwnedDpoRawSourceIdentity.cs` | `fb85f907d3e85c7e6b9f0fdd14d97120093e586f` |
| `14.ContinuousLearning/Training/RepositoryOwnedFullLoraCheckpointDescriptor.cs` | `50321342c12c43764499822a6316c1d84f0618a5` |
| `14.ContinuousLearning/Training/RepositoryOwnedIncrementalTrainingCheckpointPublisher.cs` | `7a03a96b8fddfb0ca87d88c26ebbb6412bea98fb` |
| `14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointLoader.cs` | `bc9005137f56e9c87e3a4ee78f846cb57bca6cbd` |
| `14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointManifest.cs` | `a16dba29096c75c2900940b0ee53d8b6aff3be2d` |
| `14.ContinuousLearning/Training/RepositoryOwnedNativeReferenceCorpusIntake.cs` | `55003cc33b3a986be838ce222410b112c633fc8e` |
| `14.ContinuousLearning/Training/RepositoryOwnedReferenceCorpusManifest.cs` | `343ee628210aa0ed9a18dfc689094ed6c84af262` |
| `14.ContinuousLearning/Training/RepositoryOwnedReferenceCorpusNativeCapture.cs` | `20f95fd71209d82c92121ca26f231ecf8e2a6d8a` |
| `14.ContinuousLearning/Training/RepositoryOwnedReferenceMatchingCalibrationManifest.cs` | `8b9f4eba81901745f42b397fed1041ce77a4918a` |
| `14.ContinuousLearning/Training/RepositoryOwnedTrainingArtifactCloneCustodyPreparer.cs` | `893dc6fc100e43c24194b8a70c3f2dad8d057c6d` |
| `14.ContinuousLearning/Training/RepositoryOwnedTrainingArtifactLayout.cs` | `ccb18dd7d987655c9ccd0ad67ee9f1074e481d28` |
| `14.ContinuousLearning/Training/RetainedCheckpointBackedIncrementalTrainingSessionFactory.cs` | `67ad0e8e16754f1d6d18c1a5a5079305d2459225` |
| `14.ContinuousLearning/Training/ServerOwnedContinuousLearningAdapterRootCapability.cs` | `eac120a41109217b21a92df1769ede7e9e2d374f` |
| `14.ContinuousLearning/Training/ServerOwnedContinuousLearningCacheRootCapability.cs` | `854bcce85a77ebf5784ec05e0645b970bb68d671` |
| `14.ContinuousLearning/Training/ServerOwnedContinuousLearningFileAccess.Posix.cs` | `ef60fc4a0d1faafe809581c1872cfdccd1fd3fdb` |
| `14.ContinuousLearning/Training/ServerOwnedContinuousLearningFileAccess.cs` | `9a975a33a0d1e5b999c500a3a0a57871f2051734` |
| `14.ContinuousLearning/Training/ServerOwnedContinuousLearningRegistryRootCapability.cs` | `8d78b2956f3520d8e41f39415a7f1aaed419c70f` |
| `14.ContinuousLearning/Training/SimulationEnRuContinuation.cs` | `cc43125395ccc7e9d2a75d25cd71aacdcb897f61` |
| `14.ContinuousLearning/Training/SupervisedTrainingExecution.cs` | `347ab31e7a7b6e63401c1e3fcf191df888599ec1` |


## Контракт локального result-root

В будущем controller `run-pair` result root задаётся одной неизменяемой абсолютной константой `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01/results/attempt-0001`, а не вычисляется как host/r/0001. CLI `--attempt 0001` только проверяет буквальную разрешённую попытку; другой номер отклоняется. Все arm paths, intent, raw/status/checkpoints и report находятся под этой константой. Build остаётся в host/bin и host/obj. Это согласование путей оболочки одинаково для A/B, не второй фактор обучения.


**Немедленный переход/stop, также часть промпта исполнителю.** Команда пользователя «заканчивай и переходи» немедленно завершает текущий опыт и переводит к названной следующей задаче в пределах разрешённой фазы. После перехода не возвращайся дописывать report, hashes, review, retry или другие остатки предыдущего опыта. Команда «остановить всё» немедленно прекращает всю работу; не завершай ради оформления ни отчёт, ни проверки, ни повтор. Сохрани уже существующие результаты; новая команда не разрешает повтор ранее завершённого или прерванного запуска.
