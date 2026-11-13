# GRADB-LOCALITY-01 — порядок обхода правого градиента в настоящем DPO

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница CORE/LEARN:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Исходный адрес и роль:** Фактор — `00.TensorCore/Core/TensorOps.cs::MatMul`, правый adjoint в BackwardFunc; `10.LoRA/LoraLinear.cs::Forward` → `11.DPO/DpoTrainer.cs::BackwardLogProbs` → AccumulateGrad/AdamW связывает его с обучением. N63CollisionProbe не подменяет этот owner. Исторический pin и собственный бюджет ниже сохраняются; роль зависимости не равна factor owner, runtime сейчас не выполнялся.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/results/report.md>).
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
- Единственный consumer entry остаётся ``powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/Run-With-Learning-Signal.ps1``; его будущий wrapper вместо common Train читает fixed shared bundle, сохраняет его path/digest в study и восстанавливает A/B. R=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/results`; в R/common разрешена неизменная входная копия shared snapshot для прежних внутренних путей. Она не создаёт preparation.claim/новые losses/updates. Reuse cost — чтение/restore, historical producer cost отдельно. Нельзя задним числом переписать claim уже выданного опыта.

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

**Данные и carrier.** Две зафиксированные D-T1/D-T2 subtraction пары; выбранные chosen в исходном порядке, по500каждой. После исходного Inspect/governance/materializer и CaptureAndValidate брать для каждого выбранного DPO урока только `batch.ChosenIds.Data[0..batch.ExactMaterial.ChosenLength)`, проверенные целочисленные IDs. Не брать padding до SequenceLength, не вставлять LF/BOS/EOS и не перекодировать склеенный текст. COMMON и arm TRAIN scorer используют те же IDs и ChosenResponseMask. 

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

**Единственное назначение результата GRADB-LOCALITY-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/results; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [GRADB-LOCALITY-01](GRADB-LOCALITY-01.md) → [RMS-COORD-ALLOC-01](RMS-COORD-ALLOC-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Новый будущий TRAIN readout — точный рецепт, не выполненный опыт.** Применить [протокол](../24-neural-learning-signal-protocol.md). Панель: chosen D-T1/D-T2; A0/A1/B0/B1. DIAG_TRAIN добавляет 8 policy forwards; Arm optimizer updates/backward/Generate/teacher остаются прежними; оплаченная common preparation и её readouts прибавлены отдельным разделом. Единственная внешняя команда будущего подготовленного host: `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/Run-With-Learning-Signal.ps1`. В этом host подготовить ровно один Run-With-Learning-Signal.ps1 по разделу «Место и способ включения» протокола; прежние команды ниже становятся его внутренними шагами, не второй цепочкой запусков. Observer вписать в точный existing bridge/Program/NativeLearner из таблицы протокола; другой проект/файл не искать. Raw.learning_signal содержит TRAIN NLL/rank/token exact и цену панели. Старые нулевые Evaluate/EXAM counts ниже относятся к ресурсному ядру; новые DIAG_TRAIN counts здесь объявлены отдельно. Протокол не обещает PASS и не разрешает увеличивать steps, менять head/objective/LR или подбирать примеры до успеха.

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Arm updates не добавляются; общий preparation budget явно прибавлен в исправленном разделе. Новый TRAIN readout имеет отдельный объявленный выше DIAG_TRAIN бюджет; прежние нули Evaluate не относятся к нему.

**Учебный сигнал и предел вывода.** На точной TRAIN-панели нового протокола показать NLL, полный gold rank, per-token и teacher-forced response exact до/после; EXAM отдельно. Диагностические forward оплачены объявленным выше бюджетом. Свободная генерация/общее владение задачей не выводятся из этих показателей. Неположительный результат не означает автоматической поломки и не назначает повтор; при positive движении показать регрессии и пределы. Само совпадение A/B или одинаковые 0→0 не заменяет учебного сигнала.



Меняем только скалярный порядок обхода `gradB` в MatMul; учатся q/v-LoRA Student; измеряем стоимость двух настоящих DPO-шагов каждого плеча. Домен — документация методов C#. Это меньший ресурсный профиль, чем предложенные helper восемь EN→RU-пар и 48 ответов. Экономия в переводе и точность новых ответов здесь не измеряются.

Рецепт библиотек/host назначен ниже; они **не создавались и не собирались этим документным проходом**. `NOT_RUN` не запрет. Не выбирать другой проект или полный production pipeline. Номер и продолжение — [единый индекс](00-execution-index.md).

## Поручение, модель, навыки и конфликты

Исходное поручение: «рассмотри последние обновления помощников и выстрой эксперименты, как можно более простые и маленькие для каждого домейна». Исполнитель и отдельный последовательный reviewer используют модель текущей сессии по умолчанию; записывают фактическое имя. Один worker, без вложенных; self-review не независимое review.

| Навык относительно `C:/Projects/llm_mvp_core` | Применяемое правило |
|---|---|
| `docs/skills/architect-behavior-classifier-SKILL.md` | Назвать native writer, state, читателя и scope |
| `docs/skills/planning-session-startup-SKILL.md` | Фаза и точные разрешённые файлы |
| `docs/skills/plan-review-SKILL.md` | Один фактор, одинаковые inputs и реальные обновления |
| `docs/skills/generated-code-review-SKILL.md` | Не подделывать capability или результат; source-copy boundary |
| `docs/skills/output-readability-gate-SKILL.md` | Короткий русский отчёт, наблюдение отдельно от ожидания |
| `docs/skills/human-communication-GLOSSARY.md` | Документация C# — домен; LoRA/gradB — механизм |

Точная экспериментальная копия разрешена. Старый `81` copy-ban и helper protected-dir запрет не применяются к этому поручению. Оригинал остаётся read-only; full product build/tests/CI, S00–S04, commit/push не нужны. Оболочке разрешены `throw/try-catch/void/DTO`; настоящие mutation/recipient/RNG/rollback guards не заменять. Перенос решает Люба. Нет порога 5%, новой личной подписи ID/SHA или blanket PENDING.

Параллельная сессия правит только документы. Прочитанное тело карточки фиксируется один раз; документный HEAD не повод повторять чтение/опыт. Ручные hashes паспорта/отчёта не вычисляются. Автоматические identity-проверки native loader/admission сохраняются и не дублируются вручную. Существующие raw/checkpoints/bin/obj/markers не удалять.

**Стоп и переход:** прямое «переходи к следующему» прекращает все дополнительные действия прошлого опыта, включая отчёт, review, hashes и правки; следующий берётся из индекса в разрешённой фазе. «Остановить всё» останавливает работу полностью, кроме необходимой остановки собственного процесса. Незавершённое сохранить. Карточка сама не запускает runtime; исполнитель не берёт документную работу.

## Единственное изменение и native путь

[TensorOps.MatMul:679–698](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:679) вычисляет правый adjoint `AᵀG` порядком `batch → inner → column → row`. [LoraLinear.Forward:118–125](C:/Projects/llm_mvp_core/10.LoRA/LoraLinear.cs:118) ставит обучаемые транспонированные факторы справа в двух MatMul. Путь `StepVector → BackwardLogProbs → logits.Backward → MatMul.BackwardFunc → AccumulateGrad → AdamW` действительно относится к обновлению этих весов.

A — исходный скалярный цикл. B заменяет только вложенные циклы **внутри прежнего batch-loop**, сохраняя offsets/modulo, массив результата и `AccumulateGrad`:

```csharp
for (int row = 0; row < aRows; row++)
for (int inner = 0; inner < aCols; inner++)
{
    float aScalar = a.Data[aOffset + row * aCols + inner];
    for (int column = 0; column < bCols; column++)
        gradB[bOffset + inner * bCols + column] +=
            aScalar * result.Grad!.Data[resultOffset + row * bCols + column];
}
```

У каждой координаты порядок прибавлений по batch и row остаётся прежним. Не добавлять SIMD/FMA/FP64, pooling, transpose cache, thread, иной reduction tree, другой forward/gradA, маску, R1/R2/E-OPT/LF-03. Это не прежний N-ADJOINT-01 (gradA scalar/SIMD) и не общий MatVec benchmark P110.

Одинаковая telemetry обеих копий окружает фактический `if(b.RequiresGrad)` body вместе с `AccumulateGrad`. В `TensorCore.Core` добавить `public static class GradBExperimentTelemetry` с `Reset()` и `Snapshot()`; snapshot — `public sealed record GradBSnapshot(long Calls,long ElapsedTicks,long MultiplyAddCount,long StopwatchFrequency)`. `MultiplyAddCount += (long)batchSize*aRows*aCols*bCols`; timestamp — Stopwatch. Счётчики сбрасываются перед каждым StepVector и читаются после него; standalone MatMul не вызывается. Это общая измерительная обвязка, не второй A/B-фактор.

<!-- BEGIN book-math-pass6-2026-10-22:gradb-locality -->
<a id="book-math-gradb-locality"></a>
## Почему тот же градиент можно вычислить быстрее

**Пример для студента.** Представьте таблицу оценок: строки — ученики, столбцы — задания. Преподавателю нужны несколько взвешенных итогов. Он может для каждого итога снова прыгать по строкам таблицы, а может прочитать строку подряд и сразу добавить её вклад во все итоги. Числа, множители и результаты те же; меняется удобство чтения данных. Именно такую экономию работы с памятью проверяет сохранённый KGB.

**Что даёт книга.** [Proposition 9.22](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#proposition-9-22), печатная с. 298 / PDF с. 334, выводит для $u=xW+b$ вклад $\partial L/\partial W=x^\top\delta$. [Theorem 9.23](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#theorem-9-23), печатные с. 298–299 / PDF с. 334–335, складывает такие вклады в $A^\top\Delta$. **Наше применение** к правому аргументу произведения $C=AB$, где $A\in\mathbb R^{m\times k}$, $B\in\mathbb R^{k\times n}$ и $G=\partial L/\partial C$:

$$
G_B=A^\top G,\qquad (G_B)_{rj}=\sum_{i=1}^{m}A_{ir}G_{ij}.
$$

Один коэффициент $A_{ir}$ нужен сразу всем столбцам $j$. В сохранённом коде порядок [A: batch → inner → column → row](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/Experiments/Rp01Micro/KGB/A/Core/TensorOps.cs#L679-L700) заменён на [B: batch → row → inner → column](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/Experiments/Rp01Micro/KGB/B/Core/TensorOps.cs#L679-L700). B берёт коэффициент один раз и проходит подряд строку $G$ и строку результата. Для каждой итоговой ячейки порядок накопления по batch и строкам остаётся прежним. Это важно и для округления чисел с плавающей точкой.

**Откуда ожидается выигрыш.** Последовательные обращения позволяют лучше использовать уже загруженные участки памяти. Это инженерное объяснение устройства циклов: промахи процессорного кэша здесь не измерены. В исходнике обычные скалярные операции; ускорение через SIMD ему не приписывается. Арифметическая сложность остаётся $O(mkn)$: на каждом шаге в обоих плечах зарегистрированы те же 46 вызовов и 123 420 672 операций «умножить и прибавить». Приближение матрицы малым рангом не используется.

**Что получилось в сохранённом опыте.** [Raw A](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/KGB/results/A/metrics.json) и [raw B](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/experiments-v2/RP01-MICRO/KGB/results/B/metrics.json) содержат по два настоящих DPO-обновления от одного квалифицированного обученного старта. Веса меняются на каждом шаге; проверенные TRAIN-ответы сохраняются на уровне 2/2 при teacher forcing. Совпадают записанные loss, margins, величины изменения весов и SHA итоговых весов. Полных массивов градиентов и SHA состояния оптимизатора в этих raw нет.

| Измерение | A → B | Что можно заключить |
|---|---:|---|
| Ядро правого градиента, шаг 1 | 558.9294 → 209.0994 мс | Наблюдаемое снижение на 62.6% |
| То же, шаг 2 | 570.4808 → 224.3966 мс | Наблюдаемое снижение на 60.7% |
| Всё обучение в плечах | 26999.8673 → 18495.4858 мс | Весь разрыв нельзя приписать этому ядру |
| Выделенные байты | 3765261568 → 3765294024 | Рост на 32 456 байт |

Суммарная разница двух измерений ядра — около **695.9 мс**, всего обучения — **8504.4 мс**. Остальная разница этим объяснением не установлена. Был один процесс на плечо; два последовательных шага не являются независимыми повторениями опыта. Подготовка общего старта учитывается отдельно. Здесь показан более дешёвый участок тех же обновлений, без сокращения числа уроков и без нового вывода о качестве на незнакомых задачах. Разбор объясняет сохранённые свидетельства; исполнительский статус и бюджет карточки остаются прежними.
<!-- END book-math-pass6-2026-10-22:gradb-locality -->

## Два точных урока и измеряемая работа

Prefix добавляется ровно один раз, с завершающим пробелом:

```text
Explain in one short English sentence what the method returns. Describe argument order. Code: 
```

| ID | Код после prefix | Chosen | Rejected |
|---|---|---|---|
| D-T1 | `int Delta(int first,int second){return first-second;}` | `Returns the first argument minus the second argument.` | `Returns the second argument minus the first argument.` |
| D-T2 | `int Delta(int first,int second){return second-first;}` | `Returns the second argument minus the first argument.` | `Returns the first argument minus the second argument.` |

Тексты simulation из уже назначенного [малого native договора N62](N62-neural-update-costs.md#общие-точные-уроки-и-начальное-состояние), а не новый внешний автор. Host создаёт две JSONL-строки UTF-8 без BOM в таком порядке: `id`, `dataset_version="n62-csharp-subtraction-v1"`, `domain="csharp"`, `pair_kind="CODE_TO_DOC"`, `provenance="simulation"`, `prompt`, `chosen`, `rejected`. Native Inspect/admission вычисляют требуемые identities автоматически.

Семантический gold — направление вычитания. Здесь chosen/rejected участвуют в настоящем objective; **EXAM/Generate = 0**; отдельно назначена TRAIN-панель из 8 DIAG_TRAIN policy forwards. Свободная/heldout EXAM accuracy не измеряется; TRAIN token/teacher-forced exact и rank измеряются новым протоколом. Отсутствующую EXAM оценку не заменять нулём или «не изменилась».

| Начальное состояние обеих ветвей | Назначение |
|---|---|
| Student | `TransformerModel.LlmModel`, `rp01-csharp-small/v1`, [generation-v1.json](C:/Projects/llm_mvp_core/config/code-documentation/generation-v1.json) |
| Топология | vocab629/context2048/hidden512, четыре блока, 8 heads/2 KV heads, FFN1408 |
| Учится | q/v-LoRA каждого блока, rank8/alpha16/dropout0: 16 factor tensors; 39 base frozen |
| Root B0 | Saved Step1/no parent/`InitialBaselineNoUpdate`, тот же canonical root loader [Program:307–332](C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/Program.cs:307) |
| Путь weights | `ReadVerified → PrepareStart → ParentManifest.RepositoryRelativeWeightsPath`, разрешённый относительно K; это конкретный resolver, не выбор другой модели |
| Reference | Независимая замороженная копия того же B0 |
| Tokenizer | [config/code-documentation/tokenizer.json](C:/Projects/llm_mvp_core/config/code-documentation/tokenizer.json), native identity из config |
| Optimizer | Свежий native AdamW m/v/step0, `CreateForProjectLora`, LR1e−5, betaDPO0.1, прежние native настройки |
| RNG | Config root seed20261014 и native policy/reference streams; actual snapshot/cursors, non-null capture/restore |

Native loader сохраняет свои текущие weight/config/manifest checks; ручные новые hash-ведомости не нужны. Физическая доступность B0/custody и ABI не проверялись этим проходом. Нет random/C50 fallback или reset между уроками.

## Короткие пути и полный сборочный состав

`K = C:/Projects/llm_mvp_core`; source-root `35.ExperimentsV2/Experiments/Rp01Micro/KGB/`; host-root `experiments-v2/RP01-MICRO/KGB/`; единственный результат `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/results/` с `common/`, `A/`, `B/`. Старый документный results-root не является вторым runtime выходом.

Используется уже разобранная [четырёхбиблиотечная граница N62/LF-03](N62-neural-update-costs.md#1-точный-состав-сборки), но **без правки LF-03**. Здесь она полностью назначена:

1. В source-root `Common/TrainingLoop`, `Common/DPO`, `Common/ContinuousLearning`, `Common/TensorCore` скопировать все `.cs` из соответствующих `04.TrainingLoop`, `11.DPO`, `14.ContinuousLearning`, `00.TensorCore`, исключая bin/obj; сохранить namespaces/copyright/relative paths. Список фиксируется один раз явными `Compile Include`. В прочитанном составе N62 это 6+14+81+13 файлов; изменившийся список не повод расширить scope.
2. Исключить `Common/TensorCore/Core/TensorOps.cs` из компиляции; вместо него `A/Core/TensorOps.cs` или `B/Core/TensorOps.cs` из того же original. A — исходный код плюс common telemetry; B — то же плюс единственная перестановка. Добавить общий `Common/TensorCore/Core/GradBExperimentTelemetry.cs`. Остальные TensorCore файлы, включая Tensor.Backward, исходные.
3. В host-root: `Runner.csproj`, `Program.cs`, `Directory.Build.props`, `References.props`, четыре `Sources.<Library>.props`, `Libs/TensorCore/TensorCore.csproj`, `Libs/TrainingLoop/TrainingLoop.csproj`, `Libs/DPO/DPO.csproj`, `Libs/ContinuousLearning/ContinuousLearning.csproj`.

| AssemblyName | Compile | Только local ProjectReference |
|---|---|---|
| `TensorCore` | Common/TensorCore кроме TensorOps + selected A/B TensorOps + common telemetry | Нет |
| `TrainingLoop` | Common/TrainingLoop | TensorCore |
| `DPO` | Common/DPO | TrainingLoop, TensorCore |
| `ContinuousLearning` | Common/ContinuousLearning + `GradBExperimentBridge.cs` | DPO, TrainingLoop, TensorCore |
| `GradBRunner` | Только Program.cs | Все четыре выше |

Все проекты net9.0, implicit usings/nullable, `EnableDefaultCompileItems=false`, Variant строго A/B. Локальный Directory.Build.props задаёт `BaseIntermediateOutputPath=<host>/obj/<Variant>/<MSBuildProjectName>/`, outputs Runner `<host>/bin/<Variant>/Release/net9.0/`, libraries `<host>/libbin/<Variant>/<MSBuildProjectName>/Release/net9.0/`, `AppendTargetFrameworkToOutputPath=false`; compile constant `KGB_ARM_A`/`KGB_ARM_B`. Без inherited product Directory.Build targets; свойства/импорты host задать явно. Все Compile Include/Reference пути разрешаются относительно фиксированного K.

Неизменный binary pack — **только** `K/tools/Rp01SimulationDpo/bin/Release/net9.0/`. `References.props` содержит явные Reference/HintPath/Private=true на следующие 26 DLL:

```text
27.TranslationDomain.dll; 44.DesignConversionDomain.dll;
AgentAlgorithms.dll; AgentInfrastructure.dll; AutonomousLearning.dll;
CapabilityTraining.dll; Core.dll; DatasetPipeline.dll; DynamicDomains.dll;
Embeddings.dll; Inference.dll; LoRA.dll;
Microsoft.CodeAnalysis.CSharp.dll; Microsoft.CodeAnalysis.dll; Microsoft.Data.Sqlite.dll;
ModelServer.dll; SemanticCompiler.dll; SemanticCompiler29.dll; SemanticCompilerAdapter.dll;
SQLitePCLRaw.batteries_v2.dll; SQLitePCLRaw.core.dll; SQLitePCLRaw.provider.e_sqlite3.dll;
Tokenizer.dll; TrainableRouter.dll; TransformerModel.dll; VectorSearch.dll
```

Ни оригинальные TensorCore/TrainingLoop/DPO/ContinuousLearning DLL, ни Rp01SimulationDpo executable не включать. Assembly identity/version и исходные friend declarations библиотек сохраняются из их csproj/source; Runner не получает новый friend. После ResolveReferences заменённые имена должны разрешаться в local outputs текущего Variant; transitive original DLL — конкретная ошибка сборки, не fallback. Не копировать весь A bin в B, не применять AssemblyResolve/reflection или product rebuild.

## Bridge, настоящая authority и конечный lifecycle

Bridge находится в **экспериментальной ContinuousLearning**, namespace `ContinuousLearning`, класс `public static GradBExperimentBridge`, метод `Run(GradBRunRequest request,CancellationToken token)`. `GradBRunRequest` — immutable DTO fixed ID/arm/input/output/root; native proof values получает реализация, не пользователь. Program CLI строго `--arm A|B <absolute-arm-output>`, compiled arm должен совпасть. Он не вызывает `CodeDocumentationSimulationDpoExperiment.Run` или main500.

Назначенная последовательность, одинаковая A/B:

1. `CodeDocumentationSimulationDpoExperiment.Inspect(inputPath,inputSha,2,SimulationDpoDomain.CodeDocumentation)` — сохраняет text/duplicate/tie rules. Автоматическое вычисление inputSha необходимо этому API; отдельного ручного хеширования нет.
2. `ModelServer.CodeDocumentationGenerationConfiguration.ReadVerified` с root K и конфигурацией canonical loader; затем `TrainableRouter.Training.CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart`. Узко воспроизвести `LoadModels` из указанного Program307–332: Step1/no-parent/InitialBaselineNoUpdate/full55/base39, независимые policy/reference, actual saved-weights checks. Reference RequiresGrad=false, adapters только прежние q/v; dropout0.
3. Native `TrainingDataGovernanceDecision(true,false,["sha256:"+inputSha])`, обе authorship `DeterministicGenerator`, `TrainingDataGovernanceAdmission.Validate`; `DpoPairTrainingContentAuthority.Project`; `AgentInfrastructure.Logical.LogicalDpoPreparedBatchMaterializer.Prepare`; `DpoExactPairMaterial.CaptureAndValidate` с actual tokenizer/vocab/context. Не менять exact mask/material/digest.
4. `DpoProjectLoraRecipient.Capture(policy,reference,adapters,loraConfig)` и `DpoExecutionProfile.Capture(batches[0].Batch,recipient.Bindings)`. Затем настоящая authority/lease, как [simulation-owner:132–139](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:132):

```csharp
var legal=Require(LegalTrainingTeacherAdmissionAuthority.Create([],0,authoritySalt));
var live=true;
var authority=ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
    legal.ValidateBatch,()=>live,null,
    legal.SealLogicalCapabilityReplay,legal.VerifyLogicalCapabilityReplay,
    legal.SealLogicalCapabilityExecutableManifest,
    legal.VerifyLogicalCapabilityExecutableManifest,legal.ValidateBatch);
using var lease=Require(((IServerOwnedDpoTrainingLeaseBroker)authority)
    .AcquireCodeDocumentationTrainingLease(content,trainingRunId));
var mutation=Require(lease.IssueDpoTrainerMutationAdmissionCapability());
```

Общий 32-byte salt study создаётся однократно, не заменяет Student RNG. `Require` сохраняет исходный error code. Нельзя создать fake mutation, permissive issuer или выдать Runner другое assembly name.

5. `LoraRandomStateIdentity` вычисляет topology/config identity. Native `SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(<arm>/checkpoints)`. `DpoTrainerPublicationObserver`: ArtifactOwnershipProven пишет receipt; BeforePolicyFinalMove вызывает `RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack` и `RepositoryOwnedDpoRandomStateFile.WriteCreateNew`, как [145–158](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:145). RandomState=null — ошибка. PublicationFailed сохраняет факт, не повторяет шаг.
6. Один `DpoTrainer.CreateForProjectLora(policy,reference,checkpointRoot,mutation,recipient,profile,null,observer,step=>policy.CaptureRandomState(topology,loraIdentity,step),state=>policy.RestoreRandomState(state,new(topology,loraIdentity)))`. Null здесь root continuation proof, не permit. Записать A0/B0 weights/optimizer step0/RNG один раз.
7. Цикл ровно D-T1→D-T2: create-new issued receipt, reset common telemetry, phase Stopwatch/CPU/allocated bytes, один `StepVector` с одним batch, завершить interval, сохранить returned metrics/telemetry/completed receipt. Нативная transaction сохраняет RequireValid, optimizer, RNG, ownership и rollback. Никаких probes/extra forwards/Train для прогрева.
8. После двух committed шагов A1/B1: сохранить final LoRA/m/v/RNG, base/reference invariants; открыть final full checkpoint в третьем canonical Load и native optimizer/RNG sidecars, как [194–205](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:194). Readback не генерирует ответы.

Подготовка создаёт единственную study `GRADB-LOCALITY-01`, фиксирует texts/order/состав/variant и native run IDs `GRADB-LOCALITY-01:A`/`:B`. Двоеточие только в run ID, не имени Windows-каталога. `A.started.json`/`B.started.json` создаются один раз; существующая попытка не даёт право сменить ID/повторить. Partial issued/applied/committed counts сохраняются. В finally `live=false`, lease Dispose; ошибки ненулевой exit, без retry/продолжения следующего шага.

Это source-backed состав, не проверенная бинарная сборка. Конкретные остающиеся engineering-проверки при подготовке — compile/ABI четырёх copied libraries с фиксированным pack и доступ canonical B0/custody. Здесь нет поручения искать новый host или ослаблять контракт при отказе.

## Точные команды будущего ограниченного запуска

```powershell
$gbHost='C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB'
$gbOut='C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/results'
dotnet build "$gbHost/Runner.csproj" -c Release -p:Variant=A
if($LASTEXITCODE -ne 0){throw 'A build failed; no run'}
dotnet build "$gbHost/Runner.csproj" -c Release -p:Variant=B
if($LASTEXITCODE -ne 0){throw 'B build failed; no run'}
dotnet "$gbHost/bin/A/Release/net9.0/GradBRunner.dll" --arm A "$gbOut/A"
if($LASTEXITCODE -ne 0){throw 'A failed; preserve partial evidence; no B/retry'}
dotnet "$gbHost/bin/B/Release/net9.0/GradBRunner.dll" --arm B "$gbOut/B"
if($LASTEXITCODE -ne 0){throw 'B failed; preserve partial evidence; no retry'}
```

Обычная инженерная реализация назначенных файлов и починка собственного build до runtime допустимы в порученной программе; эта Markdown-задача ничего из команд не выполняет. Нет SDK-install/full35 fallback, reverse order, reviewer rerun.

## Бюджет, метрики и вердикт

Два fresh процесса A→B. Каждый: один trainer, два разных урока, два sequential StepVector/optimizer. **В ядре A/B4updates,8policyForward+8referenceForward,8policyBackward; common1000SFT учтены отдельно**; внутренние gradB contractions считать telemetry, не заранее придуманным числом. Native load — policy/reference/final readback по три на плечо, всего шесть. `Generate/teacher/warmup/retry=0`; отдельно 8 DIAG_TRAIN policy forwards. Подготовка/загрузка/сохранение вне step-cost и показаны отдельно.

Одинаковая работа: token IDs, masks, order, initial states, settings, native calls. Не ставить условие «LoRA delta обязательно ненулевой»: честный нулевой эффект допустимого реального update — результат. `gradB Calls=0` означает недостижение выбранного участка, а не успешную экономию. Expected coordinate-order parity проверяется по сохранённым final states; различие нельзя прятать или объявлять эквивалентностью.

Основные показатели: actual StepVector wall/CPU/allocated bytes и вложенный gradB elapsed/calls/multiply-adds. Вложенное время **не складывать** с общим Step time. CPU `Process.TotalProcessorTime` delta; managed allocations `GC.GetTotalAllocatedBytes(true)` delta, не peak RAM. Whole-process peak working set отдельно, не training-only. Native returned loss — pre-update online по каждому уроку, не heldout/final loss. Сохранить реальные исходные/итоговые weights, m/v, RNG; comparisons читают эти данные без нового model call.

**Любое измеренное полезное улучшение — успех по показателю и C#-профилю, без обязательного роста accuracy.** Все ухудшения рядом; единственная пара запусков даёт локальную наблюдаемую разницу, не устойчивый speedup и не доказанную причину JIT/noise. Unit/Flow не создаются; Student четыре блока, q/v-LoRA16 tensors, frozen base39, reference целиком frozen; не называть tensors слоями.

`report.md` ≤350 слов объяснений плюс таблицы/raw-ссылки:

| Что сравнили | A0 → A1 | B0 → B1 | B−A / предел |
|---|---|---|---|
| Фактические updates/gradB calls/work | не измерено | не измерено | — |
| Step wall/CPU/allocated bytes | не измерено | не измерено | — |
| Вложенный gradB ms | не измерено | не измерено | Не складывать со Step |
| Online loss каждого урока; final state parity | не измерено | не измерено | Не heldout |
| Process peak; load/save; state bytes | не измерено | не измерено | Полный scope подписан |
| Новые ответы/accuracy | не измерено | не измерено | Генераций нет |

Порядок русского отчёта: что меняли → на чём проверяли → writer/state/reader → факты и расходы → выигрыши/ухудшения → границы/кандидат. GOOD-образец: «В четырёх настоящих updates gradB достигнут; B использовал меньше времени, allocations выросли; состояние совпало. Это локальный выигрыш времени». BAD: «Переставили циклы — восстановили перевод» или «Не генерировали ответы, значит ресурсный результат неуспех».

Один readback фактов/ссылок/кодировки, без Mermaid/render/manual hashes/Git/нового run. Кандидат ≤120 слов со ссылкой на TensorOps.MatMul gradB; перенос original решает Люба. Редакционная правка: «уточнён отчёт, данные и опыт не менялись».

> **Промпт исполнителю.** Реализуй только назначенные KGB-копии и GradBRunner по этому телу. Реальный canonical B0, exact два C# simulation-урока, прежний native issuer/recipient/RNG/publication/rollback. Меняется только scalar loop order gradB; общая telemetry одинакова. Два StepVector на плечо, всего четыре; 8 DIAG_TRAIN policy forwards; Generate и дополнительные проверочные updates=0. Сохрани actual cost и состояние, признай любой полезный измеренный выигрыш успехом его оси с остальными изменениями рядом. Модель по умолчанию, простой русский отчёт, без ручных hashes; product/CI не трогать. Прямой переход/стоп немедленно завершает дополнительные действия.

## Доказательства и границы чтения

[Helper gradB-карточка](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps/new-gradb-locality-card.md>) и [DPO/LoRA dossier](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps/dpo-lora.md>) рассмотрены как отдельный EN→RU C50/live8 профиль. [Первичный read-only пакет20.10](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/2026-10-20-primary-evidence.md:117) закрывает **только presence/hash gap архивных inputs**: [C50 checkpoint](C:/Projects/llm_mvp_core/training-artifacts/experiments/code-documentation-simulation/simulation-csharp-main500-20261015-01/checkpoints/dpo_policy_step_000050.bin) —47,899,519bytes, SHA256 `6f4d0284ea823ba99932b0f28576855c14205703115b48c983c7a9f03749179b`; [after-runtime-manifest.json](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after-runtime-manifest.json) —SHA256 `fb3be95387e62976e9305cf695d3e66be9ea36af0bd59a113c31149d6abe46f9`,73/73actualfiles hashes совпали; [native-provenance.json](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/native-provenance.json) —SHA256 `cbf4beb7eff482cd1c8324f8bd3bbf522551bd9fabd841d2a7067c7b9195f1bd`,8loadedDLL owners/17source checks совпали с current bytes и9622b575a6aabb79939b12cc9f5d96313f1bf370. Файл checkpoint только читался/hash, loader не вызывался. Совместимость ABI нового host, PDB metadata и исполнение не проверялись; архив использует **legacy fractional carrier**, поэтому это не proof current exact-carrier/default-profile equivalence. ENRU не активируется и не передаётся исполнителю как задача поиска. Здесь остаются C# профиль, бюджет4updates/0Generate и очередь24→25. Current TensorOps679–698/LoraLinear113–125/BackwardLogProbs1072–1120 и lawful N62 source path остаются основанием этого C# рецепта; никакой ENRU результат ему не присвоен.
