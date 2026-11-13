# N-HEAD-01 — проецировать только оцениваемые строки при полном контексте DPO

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница LEARN:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Исходный адрес и роль:** Фактор — согласованные будущие replacements в `00.TensorCore/Core/TensorOps.cs` (row-select/scatter), `03.TransformerModel/LlmModel.cs` (selected head rows), `11.DPO/DpoTrainer.cs` (row map/scoring/backward). Новые API существуют только по спецификации experimental copy; N63CollisionProbe не является owner scored-head. Исторический pin и собственный бюджет ниже сохраняются; роль зависимости не равна factor owner, runtime сейчас не выполнялся.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01/results/attempt-0001/report.md>).
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
- Единственный consumer entry остаётся ``powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01/Run-With-Learning-Signal.ps1``; его будущий wrapper вместо common Train читает fixed shared bundle, сохраняет его path/digest в study и восстанавливает A/B. R=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01/results/attempt-0001`; в R/common разрешена неизменная входная копия shared snapshot для прежних внутренних путей. Она не создаёт preparation.claim/новые losses/updates. Reuse cost — чтение/restore, historical producer cost отдельно. Нельзя задним числом переписать claim уже выданного опыта.

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

**Данные и carrier.** Обе строки fixed dpo-tokenizer-snapshot.jsonl с SHA91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95; chosen по500каждой. После исходного Inspect/governance/materializer и CaptureAndValidate брать для каждого выбранного DPO урока только `batch.ChosenIds.Data[0..batch.ExactMaterial.ChosenLength)`, проверенные целочисленные IDs. Не брать padding до SequenceLength, не вставлять LF/BOS/EOS и не перекодировать склеенный текст. COMMON и arm TRAIN scorer используют те же IDs и ChosenResponseMask. 

**Оплаченная общая подготовка.** В исходном A-code один раз получить W0 прежним ReadVerified→PrepareStart→LoadModels с исходными Step1/no-parent/55/39/hash guards. В модели q/v-LoRA исходного H512 только16adapter tensors trainable; reference не обучать. Создать native TrainingLoop.Trainer и AdamW(.9,.95,decay0,eps1e-8); Batch1, SeqLen2048, MaxLr=MinLr=.001f, Warmup0, TotalSteps1004, GradAccum1, Clip1, weight1. Выполнить **ровно1000** TrainSupervisedNextTokenStep в назначенном цикле, без early success stop. Это1000forward+1000backward+1000optimizer calls, отдельно от arm timer и ниже указанного фактора. Единственные COMMON readouts — W0 и S0 после1000: 2 rows в каждом, всего4 policy forward, training:false, без backward/Generate. Использовать shifted input IDs[0..L−2], targetID=IDs[i+1], только назначенные response positions; logit row i. Снять per-position predicted/gold IDs, strict-greater gold rank, stable NLL и exact по **всем** response targets. Не сравнивать full-sequence SFT loss с response NLL как одну метрику.

**Qualification и отрицательный исход.** Common preparation.json содержит `learning_qualified` (boolean), completed_updates, exact_by_id_W0, exact_by_id_S0, acquired_ids и failure_reason. Wrapper допускает arms только при exit0 **и** learning_qualified=true; exit0 с false не означает разрешение A/B. Минимальная единица cost-сравнения — хотя бы одна заранее заданная полная TRAIN response, которая была exact=false в W0 и exact=true в S0; сохранить все 2 строк и список приобретённых ID. Это узкая квалификация запоминания полной строки, **не** доказательство общего правила, всех ответов или превосходства над constant-answer baseline. Две разные освоенные строки, если появятся, отражаются отдельно; порог не подбирается по seed. Если предикат не выполнен, записать PREPARATION_COMPLETED_LEARNING_UNQUALIFIED, реальные1000updates и панель; arm A/B не запускать, гипотеза/учебная цель OPEN. Это не автоматический NEXT и не завершение задания одним отчётом. Случай W0 уже exact по всем строкам не доказывает acquisition этой preparation; зафиксировать NO_ACQUISITION_CONTRAST и не искать другой набор. Ошибка до1000 — partial, unknown counts=null, без retry. Прямой STOP/NEXT остаётся выше оформления.

**Снимок и общая граница.** Только назначенный producer из33 в `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1` сохраняет create-new S0.weights.bin, S0.optimizer.bin, S0.rng.json и preparation.json. Native CreateNew SaveCheckpointToStream → независимая запись optimizer/RNG/qualification → точный typed weights API из binding выше → LoadCheckpoint с новым/rewound stream; все55key/shape/value bits сверить с live S0,39base сW0. Native AdamW.ExportPortableState по 16 реальным adapter bindings (GetCheckpointParameterEntries, reference-identity attached adapters, ordinal keys) → ToBytes/FromBytes. CaptureRandomState/RestoreRandomState используют исходный LoraRandomStateIdentity этой модели, не придуманные streams. В A/B после обычного B0 construction/validation загрузить S0; не переинициализировать adapters после Load. После одинаковой загрузки S0 обе arm создают **fresh native DPO optimizer0**; SFT m/v сохраняются в common receipt, но в DPO не импортируются. Reference в обеих arms — независимый исходный B0, frozen. Это осознанный общий weights-only переход SFT→DPO, не full DPO continuation и не production parent adoption. Новый recipient/profile/lease захватывается после S0 load; base39/R0 неизменны. Native mutation, publication, optimizer/RNG callbacks и admission не ослабляются. Private preparation.json честно помечает SFT-derived policy, source pin этой карточки, actual data/carrier identity и1000updates; не создавать поддельный canonical DPO parent manifest и не писать training-artifacts/models.

**Разделение entry.** Consumer wrapper после build проверяет shared bundle и восстанавливает A/B. Производитель запускается отдельно по контракту33; подготовка внутри consumer запрещена:
Исторический локальный `--prepare-learned` снят с маршрута consumer. Единственная будущая подготовка — отдельный producer из shared-binding в начале карточки и контракта33; LC остаётся export-only. Consumer читает COMPLETE bundle, при его отсутствии отказывает без Train.
Реализация будущего producer следует точному `Prepare.ps1`/`--prepare-shared` из контракта33, только в назначенном producer host и с исходной A semantics. Старый локальный PrepareLearnedStartAsync в consumer не реализовывать и не вызывать. Consumer owner создаёт собственные root/arm claims до load, проверяет shared bytes и qualification, затем восстанавливает state. Common claim принадлежит только producer. Ни runtime-реализация, ни новый запуск этим документом не заявлены.

Budget factor: 4 DPO updates, два одноэлементных StepVector на плечо. **Сумма однократного producer и одного consumer:** 1004 optimizer updates; 1016 training forward, 1008 backward; 4 common readout + прежние 8 DIAG_TRAIN. Generate0. Materializer common выполняется только отдельным producer на исходном A и учтён как его setup; измеряемые arm materialization и их factor остаются прежними. До первого build зафиксировать общий deadline **14400s**, preparation≤10800s, каждый build≤600s, каждый arm≤900s, summary≤120s; общий deadline ограничивает сумму, прежние более короткие defaults для этой цепочки заменены. Один wait≤60s — только responsiveness, не срок обучения. Ожидать тот же handle; timeout не разрешает новый run.

**Суммарный учёт producer+consumer, включая наблюдение:** 1004 updates /1008 backward;1016 training +4 common +8 arm DIAG_TRAIN =1028 forwards;Generate0. Эти максимумы не разрешают дополнительные вызовы ради отчёта, проверки hashes или retries.

**Текущий consumer после shared restore:** 4 updates/8 Backward;16 training+8 DIAG_TRAIN=24 Forward; Generate0. Common updates/readouts=0; перед arm нет `--prepare-learned`. Сохранённые producer costs указываются по ссылке и не прибавляются к повторно измеренным arm costs. Конечные сроки выше сохраняются как caps, свободный preparation budget не разрешает дополнительные calls.

**Before/after и границы фактора.** A0/B0 обязаны совпасть с квалифицированным S0 на тех же сохранённых TRAIN predictions и state; обе стороны получают одинаковые carrier/reference. Снять исходные arm before/after в их прежнем бюджете; у retention сохранность относится к приобретённым полным строкам,0→0 не считается сохранностью. При after regression записать её рядом с выигрышем ресурсов. Для cost-equivalence сравнить finite losses/logits/gradients/final weights и DPO или SFT optimizer state по прежней tolerance; qualification не ослабляет эту проверку. Материализация/подготовка, measured arm updates и readouts — отдельные временные/allocations интервалы. Любое положительное измеренное изменение оси при валидном сравнении сохраняется без нового minimum gain.

Источники API: [Trainer:67–147](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:67), [AdamW portable:188–290](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:188), [exact carrier:20–78](C:/Projects/llm_mvp_core/11.DPO/Models/DpoExactPairMaterial.cs:20), [DPO import требует собственного continuation proof:533–560](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:533). Подготовка применяется только отдельным producer из контракта33. Consumer, включая встроенный промпт, имеет0common updates/readouts и выполняет назначенное ядро A/B; суммарный бюджет producer+consumer не является командой повторить подготовку. Модель исполнителя — фактическая default своей сессии без override. Никаких операций текущим документным поручением не запущено.

**Готовность учебной постановки:** DESIGNED_REQUIRES_LEARNING_VALIDATION. Назначенные ниже start/data/schedule не обещают усвоения и не разрешают повторить уже известный нулевой рецепт. При acquisition проверяется приобретённое поведение; retention/restore допускается лишь при показанном навыке до вмешательства. Если сигнал отсутствует, learning objective остаётся OPEN: автор исправляет конкретный start/минимальный набор/бюджет до новой попытки, исполнитель не ищет замену и не добавляет steps до PASS. Один отчёт с INVALID_ZERO_TO_ZERO не завершает задание и не превращает его в успех ресурсов. Прямое NEXT Любы выполняется сразу; иных автоматических повторов нет. [Обязательный контракт30](../30-failure-recovery-and-learning-validity.md).

**Единственное назначение результата N-HEAD-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01/results/attempt-0001; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [N-HEAD-01](N-HEAD-01.md) → [METHOD-KNN-TARGET-SCOPE-01](METHOD-KNN-TARGET-SCOPE-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Новый будущий TRAIN readout — точный рецепт, не выполненный опыт.** Применить [протокол](../24-neural-learning-signal-protocol.md). Панель: chosen D-T1/D-T2; A0/A1/B0/B1; diagnostic через полный обычный Forward. DIAG_TRAIN добавляет 8 policy forwards; Arm optimizer updates/backward/Generate/teacher остаются прежними; оплаченная common preparation и её readouts прибавлены отдельным разделом. Единственная внешняя команда будущего подготовленного host: `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01/Run-With-Learning-Signal.ps1`. В этом host подготовить ровно один Run-With-Learning-Signal.ps1 по разделу «Место и способ включения» протокола; прежние команды ниже становятся его внутренними шагами, не второй цепочкой запусков. Observer вписать в точный existing bridge/Program/NativeLearner из таблицы протокола; другой проект/файл не искать. Raw.learning_signal содержит TRAIN NLL/rank/token exact и цену панели. Старые нулевые Evaluate/EXAM counts ниже относятся к ресурсному ядру; новые DIAG_TRAIN counts здесь объявлены отдельно. Протокол не обещает PASS и не разрешает увеличивать steps, менять head/objective/LR или подбирать примеры до успеха.

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Arm updates не добавляются; общий preparation budget явно прибавлен в исправленном разделе. Новый TRAIN readout имеет отдельный объявленный выше DIAG_TRAIN бюджет; прежние нули Evaluate не относятся к нему.

**Учебный сигнал и предел вывода.** На точной TRAIN-панели нового протокола показать NLL, полный gold rank, per-token и teacher-forced response exact до/после; EXAM отдельно. Диагностические forward оплачены объявленным выше бюджетом. Свободная генерация/общее владение задачей не выводятся из этих показателей. Неположительный результат не означает автоматической поломки и не назначает повтор; при positive движении показать регрессии и пределы. Само совпадение A/B или одинаковые 0→0 не заменяет учебного сигнала.

## Назначение и текущий статус

Дата: 2026-10-20. **DESIGNED_NOT_RUN / HOST_NOT_MATERIALIZED**. Это документное завершение [входящего N-HEAD-01](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/02-NEURAL-CURRENT-STATE-AND-NOVEL-HEAD.md:30>), а не новая гипотеза этого агента. Product, экспериментальный код, fixtures, веса и runtime в этом проходе не менялись; build/обучение/ABI не проверены. Карточка сама по себе не разрешает исполнение.

Предмет — документация C#; обучается настоящий saved Student `rp01-csharp-small/v1` через q/v LoRA. Проверяется стоимость полного DPO update, если вычислять LM-head только для hidden rows, чьи next-token targets оценивает exact response mask. Полный prefix, attention и backward сохраняются. P01 про обучение весов lm_head — другой фактор; здесь lm_head остаётся frozen.

Исполнитель и последовательный проверяющий используют **модель текущей сессии по умолчанию без override**. Astra ultra в текущем исследовании не выполняет runtime. По отдельно полученному прямому поручению на этот опыт исполнитель готовит один назначенный рецепт; действительное разрешение не спрашивается заново из-за смены инструмента. Host/model/data/command не заменяются самостоятельным поиском.

Назначенный host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01`.
Назначенный result: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01/results/attempt-0001`.
Чужой/неизвестный host или любой existing result/issued marker означает `BLOCKED_PATH_EXISTS`: не удалять, не переносить, не выбирать следующий attempt. Новый номер — только отдельное прямое поручение. Никаких fetch/checkout, product tests/full CI или promotion.

## Первичные доказательства и граница вывода

- [LlmModel.Forward](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:219) вычисляет `_lmHead` для всех T rows после полного Transformer. [ForwardHidden](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:244) уже предоставляет embedding→blocks→finalNorm без head.
- [ComputeTargetLogProbs](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1036) пропускает `mask[i+1]==0` после полного forward. [BackwardLogProbs](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1072) вносит gradient в выбранные target rows, затем вызывает `logits.Backward`.
- [StepVectorCore](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:832) вызывает reference/policy forward, два backwards на пару и [ApplyOptimizer](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1219). Материал поступает из [настоящего simulation owner](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:175).
- [Tensor.Children/BackwardFunc](C:/Projects/llm_mvp_core/00.TensorCore/Core/Tensor.cs:24), [Backward(initialGradient)](C:/Projects/llm_mvp_core/00.TensorCore/Core/Tensor.cs:94) и [AccumulateGrad](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:984) позволяют определить differentiable row-select внутри копии TensorCore. Существующий Gather без gradient edge не используется.

Pin: `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`, локально доступный. Computational regions прочитаны; качество и ресурсный выигрыш не измерены. Снижение объёма head с T×H×V до R×H×V — расчёт, не measured speedup. Scatter/copies могут уменьшить или убрать выигрыш.

## A/B: один фактор и точные изменения копий

A — исходный full head. B — тот же full Transformer, затем differentiable row-select и тот же frozen `_lmHead` только для scored rows. Response objective, fractional masks, denominator/sum, IDs, padding, reference, q/v adapters, optimizer, RNG и publication прежние. Не добавлять PAD-trimming, reference cache, N62 scratch/transpose/optimizer optimizations или trainable head.

Новые API ниже — **спецификация будущей экспериментальной копии**, не существующие API продукта.

1. В B TensorOps добавить public `SelectTimeRowsForHead(Tensor x, IReadOnlyList<int> sourceRows, CancellationToken ct)`. Узкий контракт: x=[1,T,H]; rows непусты, строго возрастают, 0≤row<T; сохранить defensive int[] копию. Возврат y=[1,R,H] копирует x[row*H+h]; `y.RequiresGrad=x.RequiresGrad`. При true: `y.Children.Add(x)`; `y.BackwardFunc` читает y.Grad, создаёт нулевой gradX исходной shape x, в порядке j→h делает `gradX[rows[j]*H+h] += y.Grad[j*H+h]`, затем `TensorOps.AccumulateGrad(x,new Tensor(gradX,x.Shape))`. Не заменять прежний x.Grad, не вызывать дополнительный x.Backward. Проверять ct до/после копии и каждые4096элементов; rows неизменяемы до конца backward. Старые операции остаются прежними.
2. В B LlmModel добавить public `ForwardSelectedOutputRows(Tensor tokenIds, IReadOnlyList<int> rows, bool training, CancellationToken ct)`: `x=ForwardHidden(tokenIds,training,ct)`; selected=новый row-select; return `_lmHead.Forward(selected,ct)`. Prefix/finalNorm считаются один раз. Старые Forward/ForwardWithCache/parameters/checkpoint не меняются. Новых параметров нет.
3. В B DpoTrainer добавить bounded rowMap builder: по исходному T=ids.Shape.Dimensions[1] выбрать i из0…T−2, когда `mask.Data[i+1]>0`. Только exact material после native Capture/Validate; legacy остаётся original. Reference `BatchLogProbs` и policy `ForwardPolicyLogProbs` вызывают selected forward с прежними training:false/true.
4. В private `PolicyLogProbGraph` добавить optional `int[]? OutputRowIndices = null`. A/legacy оставляют null; B exact хранит immutable map. Selected ComputeTargetLogProbs проходит **j=0…R−1**, original i=map[j], targetId=ids[i+1], maskVal=mask[i+1], logit offset=j*V. Формулы max/logsumexp/logP, maskVal, sum/cnt и порядок targets прежние. Не использовать R−1: это удалит последний target. При prompt length p и side length L карта p−1…L−2; первый ответ предсказывает последняя hidden row prompt.
5. Selected BackwardLogProbs считает cnt по map, scale exact=upstream, `targetScale=scale*mask[i+1]`, записывает `(oneHot−softmax)*targetScale` в logits.Grad[j*V+k] и вызывает исходный `logits.Backward(logits.Grad)` один раз. При null map — исходное тело. Reference frozen; policy/reference graph не смешиваются.
6. В обеих копиях DPO добавить одинаковую наблюдательную instrumentation: original-row-index + active logits после каждого reference/policy forward; после обоих backwards и непосредственно перед ApplyOptimizer в StepVectorCore — named LoRA gradient arrays. A observer копирует только scored rows full logits, B — те же R rows. Observer не вызывает forward/backward/optimizer и не меняет tensors. Одинаковый clone/capture расход входит в TRAIN counters; disk writes после timed update. Новые DTO/callback объявить в copied DPO, callback реализовать в copied ContinuousLearning; без нового friend grant/reflection.
7. B row-select сохраняет diagnostic counters forward/requiresGrad/backward/rowCount. Ожидается4 actual policy scatter-backward на B (2steps×chosen/rejected); записать фактическое число. Wiring Children/BackwardFunc и сохранение gradients обязательны: tolerance не разрешает оборванный edge. Это наблюдение текущих updates, не отдельные operator tests.

Алгоритмические replacements только `00.TensorCore/Core/TensorOps.cs`, `03.TransformerModel/LlmModel.cs`, `11.DPO/DpoTrainer.cs`. Общая instrumentation DPO/bridge не является B-factor. Это точные original target methods будущего кандидата; продукт сейчас не исправляется.

## Назначенная сборочная цепь

Основа — [конкретная композиция N62](N62-neural-update-costs.md:146). К трём библиотекам N62 добавлены TensorCore и TransformerModel: изменение private head и internal autograd должно собираться в собственных native assemblies.

| AssemblyName | Исходные .cs на pin | Local ProjectReference |
|---|---:|---|
| TensorCore | 13 | Нет |
| TransformerModel | 10 | TensorCore |
| TrainingLoop | 6 | TensorCore, TransformerModel |
| DPO | 14 | TensorCore, TransformerModel, TrainingLoop |
| ContinuousLearning | 81 + общий NHeadExperimentBridge.cs | Все4 выше |
| NHeadRunner | Новый Program.cs | Все5 выше |

Замкнутый manifest124 native файлов приведён ниже. Источники копируются по pin в `NH01/Common/<AssemblyName>/<relative native path>`, сохраняя подпапки/headers. Во всех .props явные Compile items из manifest, без glob на product/bin/obj. Из common Compile исключить три replacements; вместо них `NH01/A/TensorOps.cs`, `A/LlmModel.cs`, `A/DpoTrainer.cs` либо соответствующие B. Другие N62 factors не переносить.

Будущие файлы: Runner.csproj, Program.cs, Directory.Build.props, References.props, Sources.<AssemblyName>.props, Libs/<AssemblyName>/<AssemblyName>.csproj для5libraries; bridge в Common/ContinuousLearning/NHeadExperimentBridge.cs; study.json, train.jsonl, source-manifest.json, dependency-manifest.json, one-factor.diff. Ни один не создан текущим документным ходом.

Все проекты net9.0, Nullable/ImplicitUsings, EnableDefaultCompileItems=false; Variant=A|B обязателен. Абсолютные outputs под назначенным NH01: Runner `bin/<Variant>/Release/net9.0/`, libraries `libbin/<Variant>/<MSBuildProjectName>/Release/net9.0/`, obj/extensions `obj/<Variant>/<MSBuildProjectName>/` до SDK import. AppendTargetFrameworkToOutputPath=false, AppendRuntimeIdentifierToOutputPath=false; только Release/пустой RID. Родительские Directory.Build.props/targets отключены; локальные props подключены явно. AssemblyName=NHeadRunner, constant=NHEAD_ARM_A/B. Native library identities и исходные friends сохранены; новые friends запрещены.

Binary boundary — ровно `C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/bin/Release/net9.0/`. Явные Reference/HintPath/Private=true к25 неизменным DLL:

```text
27.TranslationDomain.dll;44.DesignConversionDomain.dll;
AgentAlgorithms.dll;AgentInfrastructure.dll;AutonomousLearning.dll;
CapabilityTraining.dll;Core.dll;DatasetPipeline.dll;DynamicDomains.dll;
Embeddings.dll;Inference.dll;LoRA.dll;
Microsoft.CodeAnalysis.CSharp.dll;Microsoft.CodeAnalysis.dll;Microsoft.Data.Sqlite.dll;
ModelServer.dll;SemanticCompiler.dll;SemanticCompiler29.dll;SemanticCompilerAdapter.dll;
SQLitePCLRaw.batteries_v2.dll;SQLitePCLRaw.core.dll;SQLitePCLRaw.provider.e_sqlite3.dll;
Tokenizer.dll;TrainableRouter.dll;VectorSearch.dll
```

Не ссылаться на native TensorCore/TransformerModel/TrainingLoop/DPO/ContinuousLearning DLL и executable Rp01SimulationDpo: их identities предоставляет copied graph. После ResolveReferences любой ReferencePath/CopyLocalPaths для replaced identities вне правильного local Variant output — PREPARATION_FAILED_ASSEMBLY_BOUNDARY. Product source напрямую не компилируется. Metadata SHA/name/version/culture/token фиксируются один раз при будущей подготовке, без исполнения DLL. Signing/ABI/B0 loading не проверены; отказ не обходить reflection/fake capability/fallback bin.

## Данные, Student и конечный lifecycle

Данные — те же две строки [существующего local fixture](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/fixtures/dpo-tokenizer-snapshot.jsonl>). SHA256 `91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95`. В будущей подготовке копировать байт-в-байт в NH01/train.jsonl. Без нового prefix/перевода/иной пары; IDs/gold не поступают Student как текст.

| Поле | Значение |
|---|---|
| Profile/config | rp01-csharp-small/v1; C:/Projects/llm_mvp_core/config/code-documentation/generation-v1.json |
| Config SHA256 | e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40 |
| B0 weights SHA256 | a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911 |
| Manifest SHA256 | c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d; Step1/no-parent/InitialBaselineNoUpdate |
| Tokenizer SHA256/vocab | e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066 /629 |
| Topology | context2048/hidden512/layers4/heads8/KV2/FFN1408/RoPE10000/RMSeps1e-5 |
| LoRA | q_proj/v_proj,rank8,alpha16,dropout0;39base+16adapter tensors |
| Optimizer/RNG | Fresh native AdamW state step0,LR1e-5,DPO beta0.1; config root seed20261014; actual policy/reference cursor snapshots retained |
| Sequence | D-T1 then D-T2,batch1,1epoch; один trainer на2урока каждого arm |
| Budget | 2StepVector/arm,4total;16policy/reference forward total;8backward total;0Generate/8DIAG_TRAIN/0Teacher/0 дополнительного warmup сверх common preparation/0retry |

Bridge `ContinuousLearning.NHeadExperimentBridge.Run(NHeadRunRequest,CancellationToken)` находится в copied ContinuousLearning. DTO immutable, fixed ID N-HEAD-01/arm/input/result. Program CLI ровно `--arm A|B <absolute-arm-output>`,3 аргумента; compiled Variant и exact разрешённый output проверяются. Не вызывать старый owner.Run с guards1/500.

Native chain — [N62:189–230](N62-neural-update-costs.md:189), **без его оптимизаций**: Inspect2→ReadVerified config→два PrepareStart/LoadModels policy/reference с original hash/55/39/Step1 guards→disjoint+dropout0→simulation authors DeterministicGenerator→governance Validate+Project→LogicalDpoPreparedBatchMaterializer.Prepare→CaptureAndValidate→DpoProjectLoraRecipient.Capture/DpoExecutionProfile.Capture→реальный LegalTrainingTeacherAdmissionAuthority/ServerOwned...Issue→AcquireCodeDocumentationTrainingLease→non-null mutation capability. Authority salt один32-byte на study, приватный и одинаков A/B; Student RNG отдельно. Permissive issuer запрещён.

Тот же publication observer сохраняет optimizer/RNG native codecs до final policy move, ownership receipts и rollback. CreateForProjectLora с non-null mutation и RNG callbacks; continuation proof null для нового DPO epoch над S0; SFT moments не импортируются. A0/B0 snapshot optimizer.Export с step0, без writer положительного checkpoint. Два sequential StepVector с original step-budget/issued/completed; actual loss подписан pre-update. Затем один native saved/live readback через третий Load, optimizer OpenExisting, RNG OpenAndValidate/Restore. Base/reference unchanged. Zero LoRA delta — реальный исход, не выдуманная accuracy. При любом исходе finally live=false и Dispose lease.

Study/result резервируются create-new до первого arm; study фиксирует source/factor/input/config/B0/arm-order/budget/noRetry. Каждый native step имеет issued до вызова и completed после returned committed update. Failure прекращает arm и весь A/B: original exception/ErrorCode, partial/unknown counts и пути сохраняются; no retry/cleanup. Подготовка/build только под NH01; результаты только под assigned attempt. Existing study/arm/markers не обходить новым RunId.

## Метрики и заранее выбранная численная граница

Primary useful metrics — полный StepVector elapsed/CPU/allocated bytes каждого шага и суммы2steps; рядом process peak working set, persisted bytes, GC. Вложенное head time не складывать с полным StepVector. Build/load/readback/report вне TRAIN, отдельно. Actual4updates/rowCounts/graph telemetry доказывают участие, но не являются сами полезным выигрышем.

A0/B0 и A1/B1: named weights/optimizer moments/RNG; active target logits по original rows, logprobs/online loss, LoRA gradient arrays **до ApplyOptimizer**, costs с units. Observer одинаков A/B, no new model call. Accuracy/послеучебный held-out loss **не измерены**; returned DPO loss не является A1/B1 quality.

Заранее заданное coordinate comparison logits/logprobs/loss/gradients/adapter weights/moments: все значения finite; `abs(a-b) <= 1e-6 + 1e-4*max(abs(a),abs(b))`. Дополнительно report maxAbs/maxRel и exact-zero↔nonzero counts по tensor. Frozen base/reference и initial states — побитово; tokenizer/material/masks/order — точно. Tolerance является выбранным условием будущего опыта, не measured error и не доказательством graph reachability. Wiring/actual scatter-backward counters обязательны; missing edge/gradient capture — INVALID_MECHANISM, не parity PASS. При divergence сохранить raw и NUMERICAL_DIVERGENCE; не менять tolerance после результата и не повторять для согласования.

Любое **измеренное улучшение полезного показателя** — успех этой метрики и домена; accuracy↑ не требуется. Все ухудшения рядом. Matched valid training с меньшими allocations может быть успехом при выросшем wall-time. При numerical divergence нельзя назвать candidate эквивалентной оптимизацией, но costs сохраняются. Один A/B не доказывает устойчивую среднюю скорость; NO_GAIN/INCONCLUSIVE допустимы.

## Точные будущие команды после реализации recipe

Ниже спецификация CLI **ещё не созданного host**, не работающая сейчас команда. Автор назначил весь путь; исполнитель реализует указанные методы/файлы и обычную оболочку. При отказе не устанавливать иной SDK/модель и не собирать product solution.

```powershell
$nhHost = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01'
$nhResult = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NH01/results/attempt-0001'
dotnet build "$nhHost/Runner.csproj" -c Release -p:Variant=A
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED A; training not started; no retry' }
dotnet build "$nhHost/Runner.csproj" -c Release -p:Variant=B
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED B; training not started; no retry' }
dotnet "$nhHost/bin/A/Release/net9.0/NHeadRunner.dll" --arm A "$nhResult/A"
if ($LASTEXITCODE -ne 0) { throw 'A_FAILED; preserve partial; no B or retry' }
dotnet "$nhHost/bin/B/Release/net9.0/NHeadRunner.dll" --arm B "$nhResult/B"
if ($LASTEXITCODE -ne 0) { throw 'B_FAILED; preserve partial; no retry' }
```

Будущая preparation фиксирует manifest124+Program/bridge/projects+one-factor.diff и B0/material identities; build stdout/stderr и exit codes сохраняются вне TRAIN. Отсутствующий .NET9 SDK/reference pack → BUILD_TOOL_MISSING; binary pack/identity mismatch → PREPARATION_FAILED_DEPENDENCY. Это технические входы, не дополнительный approval gate. ABI/build success сейчас не заявляется.

## Отчёт, навыки, STOP/NEXT

Русский report.md в assigned attempt: «Что меняли → На чём проверяли → A0/A1/B0/B1 и цена TRAIN → Выигрыши/ухудшения → Граница и кандидат». До350слов плюс raw table; пропуск «не измерено», не0. При useful gain candidate.md до120слов с original methods/report. Product adoption решает Люба; full product chain не входит в опыт.

Навыки, точный локальный корень `C:/Projects/llm_mvp_core/docs/skills/`: architect-behavior-classifier-SKILL.md, planning-session-startup-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, generated-code-review-SKILL.md, prompt-patch-authoring-SKILL.md, output-readability-gate-SKILL.md, human-readable-architecture-audit-SKILL.md, human-communication-GLOSSARY.md. Не искать другой формат/модель/skill.

**STOP/NEXT:** прямой переход — сразу следующий назначенный ID из execution-index, без прежнего report/hash/build/CI/commit/review. Прямой стоп прекращает действия; допускается только необходимая остановка собственного ещё работающего процесса. Старые experiments/results/partial сохраняются. Один readback отчёта для factual/link/encoding ошибки, без полировки. Документный HEAD другой сессии не меняет fixed runtime study.

**Промпт исполнителю:** Полный действующий контекст: карточка C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/N-HEAD-01.md; свежее полное чтение AGENTS/CLAUDE/skills — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1); роли — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2); применимость и разбор падений — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткий список ниже дополнительный. Выполняется только прямо назначенный ID/этап; документальное поручение не разрешает запуск. PRIMARY, собственные примеры, данные, команды и бюджет полной карточки сохраняются. в отдельно порученной программе реализуй только NH01 по этому рецепту: five native copies, exact2C#DPO lessons, selected head при полном prefix, настоящий authority/recipient/publication/RNG,4StepVector0Generate. Не искать host/model/command. Любой useful measured gain успешен по его оси; regressions рядом; никакой фиктивной accuracy. Failure сохраняет partial и завершает attempt без повтора. STOP/NEXT прекращает прежнюю задачу немедленно. Модель сессии по умолчанию; русский report; prod/CI не трогать.

## Manifest124 native файлов на pin

Список далее получен чтением локального Git tree `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. Это происхождение исходников, не сборка. Source/copy hashes фиксируются при будущей подготовке однократно; ручной пересчёт ради оформления отчёта не требуется.

| Source relative path | Git blob SHA-1 |
|---|---|
| `00.TensorCore/AssemblyInfo.cs` | `7fc5a8b171b5da9ea1ab92dcb0e7cfd8ffd4776e` |
| `00.TensorCore/Core/SimdOps.cs` | `cf2a93a4486e141a595b658aa8346133721c842f` |
| `00.TensorCore/Core/Tensor.cs` | `a2569eefe378fb0a841cbb2137c6a8ac001bdb3a` |
| `00.TensorCore/Core/TensorOps.cs` | `e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b` |
| `00.TensorCore/Core/TensorShape.cs` | `0879e0702f210e2dd45358af09bddff2a4ef9874` |
| `00.TensorCore/Core/TensorStorage.cs` | `22aec3ba03b756530001ae786acbdce28af90104` |
| `00.TensorCore/Init/Initializers.cs` | `30ff381b091c3ee7aaf56e9cdbab98be2652db1e` |
| `00.TensorCore/Layers/Dropout.cs` | `02cf86dc84854b163cb536cab5be40dd1b7f9d63` |
| `00.TensorCore/Layers/Embedding.cs` | `26fc1a0b46fcbc6df305421bfc48c15cc70d11a4` |
| `00.TensorCore/Layers/Linear.cs` | `00533dca7b331dcedb532a76c0b36b84cbfe9c0e` |
| `00.TensorCore/Layers/Module.cs` | `dc396e8fa5a23ce6d487ff30e1326cddc7df8efd` |
| `00.TensorCore/Layers/Parameter.cs` | `82f5ea5aebc01b70f9c3f651283a1481c1df9a26` |
| `00.TensorCore/Randomness/SplitMix64Random.cs` | `11c6d483ed94829ece961af2d77f2cff91ce411a` |
| `03.TransformerModel/Caching/ITransformerKvCache.cs` | `4d12f26e03f50b519bff987b455015d4cc2fefdd` |
| `03.TransformerModel/Config/ModelConfig.cs` | `818877df9a8dda4b40dd67bb441489b8512200b1` |
| `03.TransformerModel/FullCheckpointLoraTopology.cs` | `25f24c1a260b1421ec5ded03e6dc3a0a121ff984` |
| `03.TransformerModel/Layers/CausalSelfAttention.cs` | `8106a98c5f2dba90a975abe2056e21b5550166c6` |
| `03.TransformerModel/Layers/RmsNorm.cs` | `246bbc4781d4c06d3b71f617d51aec8860ac39f2` |
| `03.TransformerModel/Layers/RotaryEmbedding.cs` | `48597599b46f884e74bf911c66189f77eb649e37` |
| `03.TransformerModel/Layers/SwiGLUFeedForward.cs` | `5daa1c56bb54cac66bd95b8658df9cc0eb2ee56e` |
| `03.TransformerModel/LlmModel.cs` | `bef968513391f3941e40f20393a4e5af30567ff8` |
| `03.TransformerModel/Randomness/LlmModelRandomState.cs` | `96c79e56e176234e3cd8e777b2ae55fc70ae9572` |
| `03.TransformerModel/TransformerBlock.cs` | `28eeaba3abf22ccca4bd748bf8c81470c327520c` |
| `04.TrainingLoop/Optimizers/AdamW.cs` | `05877706c68c8a76b63fb62842d91a2596070faf` |
| `04.TrainingLoop/Optimizers/AdamWPortableState.cs` | `fef6af5366e412e2508a716118bd3f76759abc73` |
| `04.TrainingLoop/Optimizers/AdamWStateSnapshot.cs` | `009983981b3ff213ff68d28c955027649ba83684` |
| `04.TrainingLoop/Schedulers/CosineWithWarmup.cs` | `6020cbaa994468cbd09c12d5a9cce82a993051a1` |
| `04.TrainingLoop/Trainer.cs` | `b63d550ba43e24a2b51be2c836519aab537d1312` |
| `04.TrainingLoop/TrainingConfig.cs` | `31dd056a15888e0f0a287bf9af56297b647c8b7d` |
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


**Немедленный переход/stop, также часть промпта исполнителю.** Команда пользователя «заканчивай и переходи» немедленно завершает текущий опыт и переводит к названной следующей задаче в пределах разрешённой фазы. После перехода не возвращайся дописывать report, hashes, review, retry или другие остатки предыдущего опыта. Команда «остановить всё» немедленно прекращает всю работу; не завершай ради оформления ни отчёт, ни проверки, ни повтор. Сохрани уже существующие результаты; новая команда не разрешает повтор ранее завершённого или прерванного запуска.
