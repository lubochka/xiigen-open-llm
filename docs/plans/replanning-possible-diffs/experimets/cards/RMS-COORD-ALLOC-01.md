# RMS-COORD-ALLOC-01 — убрать массив координат на каждый элемент во время настоящего обучения

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница CORE/LEARN:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Исходный адрес и роль:** Фактор — `00.TensorCore/Core/TensorOps.cs::MultiplyBroadcast` и координатная адресация; caller `03.TransformerModel/Layers/RmsNorm.cs::Forward`, learner `04.TrainingLoop/Trainer.cs`. `03.TransformerModel/Randomness/LlmModelRandomState.cs` остаётся state dependency, а не фактором RMS. Исторический pin и собственный бюджет ниже сохраняются; роль зависимости не равна factor owner, runtime сейчас не выполнялся.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



<!-- BEGIN shared-start-binding-2026-10-21 -->
## Сохранённый старт: h64-rms-coordinate/v1

**Действующий consumer-контракт:** [общее восстановление33](../33-shared-learned-starts.md), family `h64-rms-coordinate`. Этот раздел заменяет повторную common preparation во всех командах и промптах ниже. Consumer выполняет **0 common updates,0 дополнительных common readouts**; исходные arm updates/readouts и единственный фактор сохраняются.

- Данные этого семейства: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/data.source.jsonl` и `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/data.material.json`. Заданный источник: `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/RMS-COORD-ALLOC-01.md`; рецепт: T+,T- bytes noLF;200cycles;stream=rms-coordinate-probe-v1.
- Читать только `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/bundle.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/S0.weights.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/S0.optimizer.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/S0.rng.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/preparation.json`; generation/tokenizer/authority/source manifest — там же по33. Missing/partial/unqualified → конкретный SHARED_START_UNAVAILABLE, без самостоятельного обучения или другого checkpoint.
- Единственный producer: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001`, A-семантика [назначенной карточки](RMS-COORD-ALLOC-01.md); его конечный бюджет 400updates/404Forward/400Backward оплачивается один раз **отдельно**. Existing source для экспорта: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results/common`.
- Будущий экспорт без model calls: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/Export-Existing.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`.
- Будущий однократный producer, только если он прямо включён в порученную программу: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/Prepare.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`. Это заданный интерфейс, ещё не реализованный этим документным проходом; consumer не вызывает его автоматически.
- Восстановление: S0 weights/RNG и полный SFT optimizer (m/v/step) из bundle. Проверить named weights/keys/shapes, полный optimizer state и native RNG identity; construction/ApplySeeded только **до** load. Только DPO consumer заново создаёт native admission/lease после load, без переноса из producer; SFT-only MASK/MOMENT/H64/SILU/CS01 не создают DPO lease.
- Единственный consumer entry остаётся ``powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/Run-With-Learning-Signal.ps1``; его будущий wrapper вместо common Train читает fixed shared bundle, сохраняет его path/digest в study и восстанавливает A/B. R=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results`; в R/common разрешена неизменная входная копия shared snapshot для прежних внутренних путей. Она не создаёт preparation.claim/новые losses/updates. Reuse cost — чтение/restore, historical producer cost отдельно. Нельзя задним числом переписать claim уже выданного опыта.

Старые common-рецепты, методы подготовки и суммарные1000/400+arm budgets ниже теперь описывают **только однократного producer плюс consumer**, а не расход каждого consumer. Старый child `--prepare-learned`/prepare не вызывать из consumer wrapper. Все tools и ожидание — по31; никаких exporters/manifest/scripts «пока ждём». Артефакты и scripts здесь не созданы и не проверены запуском.
**Сохранение оплаченного state:** сначала независимо записать назначенные weights, optimizer, RNG и имеющуюся qualification; затем typed validation/readback; COMPLETE последним по33. Ошибка проверки weights не должна пропускать запись остальных уже доступных состояний. Новых Train/Forward и периодических снимков не добавлять; partial сохранять.

**Точный weights API для этой карточки:** `cfg` — фиксированный ModelConfig этой карточки; LoRA rank4, targets строго `q_proj,v_proj`. Producer: `model.SaveCheckpointToStream(stream)` на новом `FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None)`, изначально Length=0/Position=0; `Flush(flushToDisk:true)`. Затем независимо записать остальные state/receipt по33 и только после этого открыть сохранённый weights для чтения. Установить Position=0; `var checkedArtifact = LlmModel.ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(4,new[]{"q_proj","v_proj"}))`; требовать `checkedArtifact.IsSuccess && checkedArtifact.Value==true`. Если в конкретном рецепте уже назначен maximumArtifactBytes, передать его неизменным (CS01:8388608). Перегрузка без topology недопустима для этого LoRA snapshot. Перед native `freshModel.LoadCheckpoint(stream)` снова Position=0 либо открыть новый read stream; construction/ApplySeeded только до load. Проверить назначенные named keys/shapes/value bits. Равный размер файла, повторное чтение самого файла или снижение loss не доказывают равенство потерянной live memory. В consumed/recovery случае такое равенство помечать UNKNOWN, не придумывать. Эта запись уточняет уже назначенные операции, не добавляет model calls.

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

**Точный codec общего RNG.** `S0.rng.json` — частный JSON **полного** TransformerModel.LlmModelRandomStateSnapshot, не native DPO filename. Запись: System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(snapshot), FileStream(FileMode.CreateNew,FileAccess.Write,FileShare.None); чтение: JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(bytes) с null→refusal. Вызвать native policy.RestoreRandomState(snapshot,new LlmModelRandomStateIdentity(topologySha,loraConfigurationSha)); повторный CaptureRandomState(topologySha,loraConfigurationSha,preparationStep) сравнивает FormatVersion/Algorithm/все AdapterKey,StreamDomain,InitializationSeed,DropoutSeed,ConsumedCursor,TopologySha256,LoraConfigurationSha256,RandomStateStep. Типы и guards — [Randomness/LlmModelRandomState.cs:11–39](C:/Projects/llm_mvp_core/03.TransformerModel/Randomness/LlmModelRandomState.cs:11), [LlmModel:286–375](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:286). preparationStep=1000 дляH512,400дляH64. Не вызывать RepositoryOwnedDpoRandomStateFile.WriteCreateNew для имени S0.rng.json. На DPO arm callback первый checkpoint имеет native step1 и **новое** metadata RandomStateStep1 при сохранённых восстановленных stream cursors; writer использует BuildFileName(bundle.Step). Это новый DPO epoch, не импорт SFT optimizer/continuation proof.

**AUTHOR_FIXED_PREPARATION_NOT_RUN.** Вместо сравнения четырёх cold updates назначена одна общая native подготовка до A/B. Она не выполнена и не обещает успеха. Число400 — **DESIGN_CHOICE_NOT_MEASURED** для этой точной модели/stream/двух строк. Исторический LEARN-SANITY400 дал1/2, а не2/2, и не является готовым checkpoint этого опыта. Не переносить H16 PA01, H512 B0 либо чужие streams. Предел фиксирован до результата; не выбирать seed/checkpoint и не продлевать до PASS.

Один общий W0 строится исходной ветвью A в этом же назначенном host, с V256/H64/L1/Q4/KV2/FF176/Context64, base seed62001, q/v-LoRA4/alpha8/dropout0, seed62002, stream `rms-coordinate-probe-v1`. Native Trainer.TrainSupervisedNextTokenStep получает UTF-8 bytes **Prompt+Answer без LF/BOS/EOS**: T1=`C# result sign: 9-4 =+`, T2=`C# result sign: 4-9 =-`. Ровно200циклов T1→T2 = **400 updates**, weight1. Native AdamW(.9,.95,decay0,eps1e-8); constantLR.001, Warmup0, TotalSteps404, Clip1, GradAccum1. Native full-sequence shifted objective и frozen base/head не меняются. Переход400подготовительных steps не является фактором A/B.

COMMON W0 и S0 после400: ровно2prompt-only training:false forward на checkpoint, всего4; тот же последний logit предсказывает answer byte, как arm TRAIN readout. Без backward, Generate, дополнительного BPE или EXAM во время подготовки. Сохранить все256logits, exact, NLL, strict-greater gold rank; argmax ties по minID, order-rank с ties отдельным полем. Никакой интерпретации первого логита как полного многотокенного ответа: ответ здесь ровно один byte.

**Конечная квалификация:** common/preparation.json содержит `learning_qualified` (boolean), completed_updates, exact_by_id_W0, exact_by_id_S0, acquired_ids и failure_reason; wrapper требует exit0 **и** learning_qualified=true, иначе arms не выдаёт. минимум одна из фиксированных T1/T2 должна перейти из W0 exact=false в S0 exact=true. Это приобретённый полный **однотокенный ответ на названную строку**, не общий закон знака. Показать оба результата;1/2 не выше constant-label baseline1/2, а2/2 двух разных знаков отличимо от него. Для cost-фактора разрешён узкий вывод по приобретённой строке; общий навык остаётсяOPEN при1/2. Если такого перехода нет, записать PREPARATION_COMPLETED_LEARNING_UNQUALIFIED и не выдавать arms.400updates и4scores сохраняются; report не закрывает цель, повтор/автоNEXT запрещены. Не ждать появления2/2 дополнительными steps.

Только producer после400 создаёт bundle `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/S0.weights.bin`, S0.optimizer.bin, S0.rng.json, preparation.json. Native CreateNew SaveCheckpointToStream; после записи остальных state/receipt — точный typed weights API из binding выше и LoadCheckpoint с новым/rewound stream; named weights+shapes+floatbits совпадают с live S0. Bindings из actual GetCheckpointParameterEntries, отфильтрованных по attached q/v AdapterParameters reference identity, unique ordinal keys. ExportPortableState→ToBytes→FromBytes; A/B new AdamW.TryImportPortableState должны вернуть true; повторный Export сверяет каждый key/shape/m/v bit/step400. Model CaptureRandomState/RestoreRandomState использует именно исходную topology/LoRA identity и stream этой карточки. Это настоящий общий restore, не одинаковый seed вместо снимка. Сначала construction+ApplySeeded, затем Load; после Load никакой инициализации. A0/B0 — S0, optimizer400; после прежних четырёх arm updates обе стороны404. Fresh Trainer counter0 разрешён постояннымLR, optimizer counter не сбрасывается.

Consumer run.py после build читает qualified shared bundle; внутреннего запуска подготовки перед A нет:
Исторический локальный `--prepare-learned` снят с маршрута consumer. Единственная будущая подготовка — отдельный producer из shared-binding в начале карточки и контракта33; LC остаётся export-only. Consumer читает COMPLETE bundle, при его отсутствии отказывает без Train.
Producer — отдельный будущий entry из контракта33 с исходной A semantics и собственным common claim. Consumer wrapper создаёт root claim/study/nonce до build и arm claim до load. Direct mode без живого owner отказывает; existing claim/result не обходить новым aNN. Фактор B применяется только после S0 restore.

**Consumer:8 optimizer updates,8 training Forward/Backward,16 diagnostic Forward; всего24 Forward/8 Backward. Common Train/readout=0.** Отдельный producer:400 updates/404 Forward/400 Backward; сумма однократного producer и одного consumer:408/428/408. Generate/DPO/teacher0. Consumer сохраняет outer cap1800s, build≤180s, каждое arm≤120s, summary≤60s; неиспользованный preparation≤1200s не переносится в новые calls. Producer cap задан в33. Healthy same-handle wait по31; тишина не повод abort/relaunch.

A0/B0 state/readout должны совпасть с S0; в A1/B1 показывать learned answer retention отдельно от EXAM. Ресурсное сравнение допустимо только при завершённых реальных updates, finite state/logit/gradient/optimizer equivalence по прежней tolerance и показанном before; регрессия exact/NLL не скрывается за allocations. Любое полезное измеренное изменение сохраняется без нового gain threshold. Источники: [native updater:67–147](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:67), [portable m/v:188–290](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:188), [constant scheduler:28](C:/Projects/llm_mvp_core/04.TrainingLoop/Schedulers/CosineWithWarmup.cs:28). Эта секция входит в каждый промпт карточки; default модель будущей сессии без override, немедленный STOP/NEXT, no retry. Текущим поручением ни host, ни S0 не созданы.

**Готовность учебной постановки:** DESIGNED_REQUIRES_LEARNING_VALIDATION. Назначенные ниже start/data/schedule не обещают усвоения и не разрешают повторить уже известный нулевой рецепт. При acquisition проверяется приобретённое поведение; retention/restore допускается лишь при показанном навыке до вмешательства. Если сигнал отсутствует, learning objective остаётся OPEN: автор исправляет конкретный start/минимальный набор/бюджет до новой попытки, исполнитель не ищет замену и не добавляет steps до PASS. Один отчёт с INVALID_ZERO_TO_ZERO не завершает задание и не превращает его в успех ресурсов. Прямое NEXT Любы выполняется сразу; иных автоматических повторов нет. [Обязательный контракт30](../30-failure-recovery-and-learning-validity.md).

**Единственное назначение результата RMS-COORD-ALLOC-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [RMS-COORD-ALLOC-01](RMS-COORD-ALLOC-01.md) → [DPO-TOKENIZER-SNAPSHOT-01](DPO-TOKENIZER-SNAPSHOT-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Новый будущий TRAIN readout — точный рецепт, не выполненный опыт.** Применить [протокол](../24-neural-learning-signal-protocol.md). Панель: T1/T2 sign fixture × A0/A1/B0/B1; TRAIN отдельно от прежних 8 EXAM. DIAG_TRAIN добавляет 8 policy forwards; полный scoring/readout бюджет теперь 16; Arm optimizer updates/backward/Generate/teacher остаются прежними; оплаченная common preparation и её readouts прибавлены отдельным разделом. Единственная внешняя команда будущего подготовленного host: `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/Run-With-Learning-Signal.ps1`. В этом host подготовить ровно один Run-With-Learning-Signal.ps1 по разделу «Место и способ включения» протокола; прежние команды ниже становятся его внутренними шагами, не второй цепочкой запусков. Observer вписать в точный existing bridge/Program/NativeLearner из таблицы протокола; другой проект/файл не искать. Raw.learning_signal содержит TRAIN NLL/rank/token exact и цену панели. Старые нулевые Evaluate/EXAM counts ниже относятся к ресурсному ядру; новые DIAG_TRAIN counts здесь объявлены отдельно. Протокол не обещает PASS и не разрешает увеличивать steps, менять head/objective/LR или подбирать примеры до успеха.

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Arm updates не добавляются; общий preparation budget явно прибавлен в исправленном разделе. Новый TRAIN readout имеет отдельный объявленный выше DIAG_TRAIN бюджет; прежние нули Evaluate не относятся к нему.

**Учебный сигнал и предел вывода.** На точной TRAIN-панели нового протокола показать NLL, полный gold rank, per-token и teacher-forced response exact до/после; EXAM отдельно. Диагностические forward оплачены объявленным выше бюджетом. Свободная генерация/общее владение задачей не выводятся из этих показателей. Неположительный результат не означает автоматической поломки и не назначает повтор; при positive движении показать регрессии и пределы. Само совпадение A/B или одинаковые 0→0 не заменяет учебного сигнала.

## Действующее локальное назначение — 2026-10-20

Это активная карточка, перенесённая из [входящего исследования](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-causal-delta-audit/RMS-COORD-ALLOC-01-CARD.md>). Гипотеза уже существовала в этом источнике; новая находка текущего агента не заявляется. Текущая фаза — **READ_ONLY_RESEARCH и правка Markdown**. Host, код эксперимента и результаты сейчас не создавались; **DESIGNED_NOT_RUN / HOST_NOT_MATERIALIZED**, сборка и обучение не проверены.

По прямому указанию пользователя внешние ссылки заменены локальными корнями. Назначенный будущий host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001`; назначенный результат: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results`. Эти пути заменяют прежние короткие внешние корни источника; состав native файлов, фактор, Student и бюджет не меняются. Это документационный рецепт будущей реализации, не готовый исполняемый host.

Исполнитель и последовательный проверяющий: **модель текущей сессии по умолчанию, без provider/model/reasoning override**. Исследовательская роль Astra ultra не становится исполнителем runtime; отдельное поручение на исполнение выполняется назначенной сессией по её умолчанию. Student/tokenizer/perceptron описан ниже отдельно. Действительное разрешение на конкретный опыт сохраняется при смене инструмента: повторно просить то же разрешение не нужно. Этот документ сам по себе не разрешает запуск.

Конфликт путей: если назначенный host занят чужими/неизвестными файлами или назначенный result/attempt уже существует, **BLOCKED_PATH_EXISTS; не удалять, не переносить, не выбирать другой путь и не повторять запуск**. Новый attempt допускается только в отдельном прямом поручении с конкретным номером. Рецепт использует локальные Git-объекты указанных pin без fetch/checkout; недоступный объект, несовпавший blob или отсутствующая зависимость — PREPARATION_FAILED до обучения, без замены ветки/модели/проекта. Текущее разрешение записи карточек не разрешает менять production, fixtures, source-copy или runtime. При будущем исполнении исходники и сами карточки читаются; результаты пишутся только в назначенный result.

Навыки будущего исполнителя, точные локальные файлы: [architect-behavior-classifier-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md>), [planning-session-startup-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md>), [phase-planning-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/phase-planning-SKILL.md>), [plan-review-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md>), [code-examination-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/code-examination-SKILL.md>), [generated-code-review-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md>), [prompt-patch-authoring-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md>), [output-readability-gate-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md>), [human-readable-architecture-audit-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md>), [human-communication-GLOSSARY.md](<C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md>). Русский отчёт и конечный шаблон указаны ниже; искать другие skills/формат не требуется.

**STOP/NEXT:** при «следующий эксперимент» немедленно оставить прежний опыт и перейти только к названному следующему, без дописывания старого отчёта, повторного hash/build/CI/commit. При «стоп» прекратить действия; допустимо лишь необходимое прекращение собственного уже работающего процесса, чтобы он не продолжал обучение. Старые опыты и failed attempts сохраняются.


## Статус, цель и граница поручения

**Статус: `DESIGN_DOCUMENTED_HOST_NOT_MATERIALIZED_NOT_RUN`.** В этом аудите нет нового запуска, собранного host или измеренного улучшения. Карточка задаёт точный рецепт будущего малого опыта. Команды ниже становятся исполнимыми после указанного этапа подготовки; сейчас готовность к запуску не заявляется.

Цель — проверить стоимость **реального SFT/LoRA update** при замене одного способа вычисления индекса внутри `TensorOps.MultiplyBroadcast`. Основной полезный показатель — allocated bytes за четыре учебных шага; рядом время, CPU, GC, процессная память и качество ученических ответов. Предметная тема — язык программирования C#; задача — определить знак целочисленного вычитания. Приоритет — после разблокирования обучения и разрешения конфликтов действующих карточек. Это не утверждение, что данный механизм объяснил ухудшение ответов RP01.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения.** Числа, рассчитанные по исходникам, не являются измеренными результатами.

**Запрет продолжать прежнюю задачу.** При прямом «стоп», «закончить немедленно» — сразу прекратить текущую работу и кратко сообщить уже известное состояние. При «следующий эксперимент» — прекратить улучшать, перепроверять, коммитить или дописывать предыдущий опыт и перейти только к названному следующему. Нельзя превращать оформление отчёта в новый запуск, поиск формата, стилистический цикл или дополнительную проверку кода. Старые опыты, результаты и незавершённые attempt-каталоги не удалять.

## Доказательства и участие в обучении

Все исходные ссылки относятся к core **`ab583032562d6656de9dd5ca6fa0a325fd4b1f30`**. Более поздний Markdown-коммит не означает изменение алгоритма; при будущем исполнении берутся именно эти исходники.

1. [`Trainer.TrainSupervisedNextTokenStep:102–145`][trainer] строит actual next-token inputs/targets, выполняет `model.Forward(training:true)`, loss, backward, clipping и `optimizer.Step`, затем увеличивает счётчик supervised-step.
2. [`LlmModel.Forward:219–242`][model] проводит этот же forward через блоки и финальный `RmsNorm`. [`TransformerBlock.Forward:46–64`][block] дополнительно вызывает два `RmsNorm` в каждом блоке. Подключение LoRA не убирает эти нормализации: attention читает их выход, LoRA меняет q/v-проекции, а native backward возвращается по сохранённому графу.
3. [`RmsNorm.Forward:29–40`][rms] дважды вызывает `MultiplyBroadcast`: сначала умножает `[B,T,H]` на `[B,T,1]`, затем на gamma `[H]`.
4. В [`MultiplyBroadcast:168–184`][broadcast] для **каждого flat-элемента** создаётся `indices = FlatToIndices(flat, a.Shape)`. [`FlatToIndices:1008–1017`][coords] каждый раз выделяет `new int[shape.Rank]`. Это служебный массив для адресации; он не является весами, словарём, доказательством или обучаемой структурой.
5. [`MultiplyBroadcast:187–210`][backward] использует сохранённый `mappedBIndices` для обратного прохода. Этот общий массив, структура графа и порядок накопления градиентов остаются одинаковыми в A/B.

Цепочка: **одинаковые учебные последовательности → native SFT forward → RMS broadcast → native backward → AdamW → изменённые LoRA параметры → независимый EXAM readout**. Опыт не сводится к benchmark вызовов голой индексации. Если native update не выполнен, результат — технический/частичный, а не нулевая точность.

RMS также достижим в DPO через [`ForwardPolicyLogProbs:1018–1030`][dpo-forward], но этот малый паспорт измеряет SFT/LoRA. Количество DPO steps в отчёте — **0**. Перенос ресурсного эффекта на DPO — отдельное, ещё не измеренное предположение; нельзя добавлять DPO-прогоны к текущему бюджету.

## A/B: один фактор и точный предлагаемый ремонт

**A:** действующий mapping loop `MultiplyBroadcast`, включая `FlatToIndices`.

**B:** тот же `bFlat` вычисляется справа налево целочисленными division/remainder без промежуточного массива координат. Остаются исходные `data`, `mappedBIndices`, проверки форм, cancellation cadence, умножения float, создание Tensor, `Children`, `BackwardFunc`, порядок накопления и вся остальная модель.

Предлагаемый diff **только экспериментальной копии** `00.TensorCore/Core/TensorOps.cs`, внутри исходного метода:

```diff
-            var indices = FlatToIndices(flat, a.Shape);
             int bFlat = 0;
-            for (int dimension = 0; dimension < b.Shape.Rank; dimension++)
+            int remaining = flat;
+            int bStride = 1;
+            for (int dimension = b.Shape.Rank - 1; dimension >= 0; dimension--)
             {
                 int bDimension = b.Shape.Dimensions[dimension];
-                int bIndex = bDimension == 1 ? 0 : indices[rankOffset + dimension];
-                bFlat = bFlat * bDimension + bIndex;
+                int aDimension = a.Shape.Dimensions[rankOffset + dimension];
+                int coordinate = remaining % aDimension;
+                remaining /= aDimension;
+                if (bDimension != 1)
+                    bFlat += coordinate * bStride;
+                bStride *= bDimension;
             }
```

Обоснование — математическое, не результат запуска: trailing dimensions B соответствуют trailing dimensions A; broadcast-ось с размером 1 всегда даёт координату 0. Для `[B,T,1]` выбирается соответствующая строка RMS, для `[H]` — `flat % H`. Данные и порядок float-операций сохраняются. Требуется измерить весь учебный переход и увидеть фактическую численную эквивалентность.

Это отдельный фактор от уже описанных frozen transpose cache, softmax scratch reuse и двойного копирования начального gradient. Здесь **не добавляется кэш**, не меняются `TensorStorage`, `Mean`, mask, objective или trainable set. Уже исправленное прохождение autograd через RMS сохраняется; возврат исторического `new Tensor` без backward запрещён.

## Фиксированная модель, предметная тема, задача, данные и бюджет

Исполнитель — **та же модель и тот же режим, которые уже используются в назначенной сессии**. Другую модель, внешний judge или способ выполнения не искать. Ученик — собственный C# Transformer `XIIGEN-native-1block-SFT-LoRA-64`, не модель исполнителя.

| Поле | Значение |
|---|---|
| Config | Vocab256; Context64; Hidden64; 1 block; 4 Q heads; 2 KV heads; FFN176; RoPE theta10000; RMS eps1e−5 |
| Base | `Initializers.UseDeterministicSeed(62001UL)` при каждом новом процессе |
| LoRA | `LoraApplicator.ApplySeeded`, q_proj/v_proj, rank4, alpha8, dropout0, rootSeed62002, stream `rms-coordinate-probe-v1` одинаков в обоих плечах |
| Native updater | `Trainer.TrainSupervisedNextTokenStep`, weight1; 4 вызова на плечо |
| Optimizer | `AdamW(beta1:.9f, beta2:.95f, weightDecay:0f, eps:1e-8f)` |
| Schedule | warmup0; totalSteps404; maxLr=minLr=.001; clip norm1; gradAccum1 |
| Tokens | UTF-8 byte IDs0…255, ноль BPE merges; EOS не добавляется |
| Objective | Исходный full-sequence shifted next-token SFT; response mask и нормировка не меняются |
| Actual budget | A:4 updates; B:4 updates; TRAIN порядок T1,T2,T1,T2 в каждом плече; всего8 updates |
| EXAM | По2 forward до и после в каждом плече; всего8 EXAM forward; обучение на EXAM запрещено |

| Split | ID | Prompt буквально | Answer буквально |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Это узкая панель разных чисел, не доказательство общей арифметической компетенции. Ответ ученика — argmax фактического последнего логита из 256, gold служит только оценке. Корректирующее правило не генерирует `+`/`−` за модель.

## Подготовка host: заданный рецепт, не поиск

Действующий единственный внешний entry — Run-With-Learning-Signal.ps1 из верхнего TRAIN-binding и протокола24. Он создаёт root claim/study; внутренний run.py получает OwnerNonce и проверяет PID/start, не создаёт второй root claim. run.py владеет build → read shared bundle/qualified check → restore A/B; arm child claims создаются до их model calls; common claim только у отдельного producer. Прямой python run.py без этого owner отказывает. Все нижние python/dotnet команды — только внутренние шаги этой единственной цепи, не второй запуск. Consumer имеет8updates/24modelForward/8Backward, включая16readouts, иdeadline1800s; common400 оплачивается только отдельным producer.

Рабочая машина данного рецепта — Windows с уже используемым **.NET9 SDK**, как задано в прочитанном исходном экспериментальном host. Новый короткий корень **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01`**. Первая новая попытка — `a001`; если она существует, остановиться с BLOCKED_PATH_EXISTS; следующую попытку назначает отдельное прямое поручение. Существующий attempt не очищается и не продолжается автоматически. Внутри текущего поручения нет автоматического retry. Отсутствие SDK фиксируется `BUILD_TOOL_MISSING`; не устанавливать другой toolchain и не искать новый проект.

1. При **отдельно назначенном исполнении** в новом `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001` создать `src`, `Runner.csproj`, `Program.cs`, `run.py`. **Не создавать `results`, `bin` или `obj` на этапе подготовки**: `results` эксклюзивно создаст runner, `bin/obj` — единственная сборка. Ни один из этих файлов не создаётся текущим документарным аудитом.
2. Взять **ровно 36 файлов**, явно перечисленных в [новом соседнем source manifest](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-causal-delta-audit/RMS-COORD-ALLOC-01-SOURCE-MANIFEST.json>), и скопировать их из read-only core на `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`, сохраняя относительные пути внутри `src`. Новый manifest содержит проверенный Git blob SHA-1 каждого исходного файла. [Прежний manifest][source-manifest] привязан к `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`: из него унаследован только состав файлов, он не задаёт версию нового опыта. Все 36 текущих blob совпали с его SHA; это установлено чтением исходников, не запуском. Исходный корень: `C:/Projects/llm_mvp_core`; использовать уже доступную локальную версию указанного файла/коммита. При несовпадении конкретного файла сообщить его и остановить preparation; не искать замену в другом проекте. `Linear.cs` должен быть **оригинальным core**, без экспериментального transpose cache.
3. За основу `Program.cs` взять [точный существующий SFT host][host-program] на planning `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`, сохранить модель, TRAIN, EXAM, updater, таймеры и readouts. Заменить ID/commit/stream на значения карточки. В raw записать `subject = "programming-language-csharp"`, `task = "integer-subtraction-sign"`; `code-sign` оставить техническим именем профиля. Удалить обращения `Linear.ExperimentReuseFrozenTranspose`, `ExperimentCacheHits`, `ExperimentCacheMisses`, `ExperimentCachedFloats`, cache fields и cache limitations. В `mechanism` записывать только `arithmetic_broadcast_index_enabled = arm == "B"`. Указать, что `training_losses` — online loss **перед соответствующим update**, а не оценка после четвёртого update.
4. В экспериментальной копии `TensorOps` добавить `internal static bool ExperimentUseArithmeticBroadcastIndex`. После общих проверок/выделения `data` и `mappedBIndices` — один выбор ветви: A выполняет исходный mapping loop, B — loop с diff выше. Общая конструкция graph/backward остаётся после выбора. Program ставит переключатель `false` на оба A0/B0, затем `arm == "B"` перед TRAIN и сохраняет до A1/B1. Одна сборка включает оба варианта; branch выбирается один раз на вызов метода. Experiment-only switch, literal tiny fixtures и отдельный host из копий исходников разрешены **только в данном опыте**; эти исключения не разрешают менять prod или подменять native Tensor/Trainer/LoRA на mock.
5. Новый плоский `Runner.csproj` основан на [существующем проекте][host-project]: `TargetFramework=net9.0`, `AssemblyName=RMS-COORD-ALLOC-01`, `EnableDefaultCompileItems=false`, Compile только `src/**/*.cs` и `Program.cs`, без ProjectReference. Все `bin`, `obj` и NuGet project extensions — внутри короткого attempt; до SDK imports выключить parent `Directory.Build.props/targets`. Нельзя собирать большой solution или dispatcher.
6. Написать **новый no-argument `run.py`** с фиксированным контрактом ниже. [Прежний runner][host-run] — источник ограниченного build/timeout поведения, а не готовая команда нового опыта. Удалить legacy `argparse/--results-root`, чтение `experiment.json`, old experiment kind и длинные nested paths. Задать буквально `root = Path(__file__).resolve().parent`, `host = root / "Runner.csproj"`, `artifact = root`, `result = Path("C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results")`, `experiment_id = "RMS-COORD-ALLOC-01"`, `jobs = [("A", "code-sign"), ("B", "code-sign")]`; прочитать manifest только из `root / "source-copy-manifest.json"`.
7. После завершения единственного предлагаемого source diff подготовить **новый** `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/source-copy-manifest.json`: `id`, `core_commit`, SHA256 фактических `Program.cs`, `Runner.csproj`, `run.py`, а для каждого из 36 `src` файлов — `path`, исходный `git_blob_sha1` из соседнего manifest и `copied_sha256` после экспериментального изменения. Manifest фиксируется **последним**, после окончательного текста run.py из шага6; вычисление checksum не изменяет ни один охваченный файл. Нельзя брать checksum изменённого Linear/Program прежнего опыта. Одновременно сохранить `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/one-factor.diff` — unified diff исходного `00.TensorCore/Core/TensorOps.cs` против его экспериментальной копии; он включает только переключатель и две ветви mapping внутри этого метода, без других алгоритмических изменений. Его SHA256 также записать в manifest. Подготовка не запускает build, tests или обучение.
8. При одном запуске runner один раз сверяет свои подготовленные checksum, отклоняет существующие `results/bin/obj` и вызывает `result.mkdir(exist_ok=False)`. Только затем копирует уже подготовленный `root/one-factor.diff` в **`results/one-factor.diff`**, пишет source manifest в `results/source-copy-manifest.json` и выполняет одну сборку timeout180s. До соответствующего native процесса эксклюзивно создаёт `results/A` либо `results/B`; Program открывает конечный JSON через `FileMode.CreateNew`. Между build и A выполнить назначенную common preparation≤1200s; при learning_qualified=true оба arm≤120s, общийdeadline1800s. Все эти служебные действия вне measured TRAIN.
9. Build stdout/stderr записать в `results/build.stdout`, `results/build.stderr`; native stdout/stderr — в `results/A/code-sign.stdout`, `results/A/code-sign.stderr` и аналогично B. `results/execution.json` содержит jobs, stage, status, exit code, actual counts из завершённого receipt и `no_auto_retry=true`. Build failure также записывает `results/status.json`. Если внешний timeout оборвал процесс до receipt, `actual_optimizer_steps = null` («не измерено»), не0; число нельзя получать ещё одним запуском. При известном build failure обучение не началось; это технический статус, не accuracy0. Любая ошибка заканчивает текущий run; нет повторов, cleanup и «починки» постороннего проекта.

После подготовки **единственная команда будущего исполнения** из нового attempt:

```powershell
Set-Location 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001'
python ./run.py
```

`a001` — единственная назначенная попытка; при существующем host/result действует BLOCKED_PATH_EXISTS, без выбора следующей попытки. Внутри него команды определены однозначно:

```text
dotnet build Runner.csproj -c Release --output bin -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
dotnet bin/RMS-COORD-ALLOC-01.dll A code-sign "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results/A/code-sign.json"
dotnet bin/RMS-COORD-ALLOC-01.dll B code-sign "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results/B/code-sign.json"
```

Каждое плечо — отдельный процесс с теми же seed/config/input. Не нужны NUnit, GitHub Actions, Windows solution tests, дополнительные seed/warmup сверх common preparation, оценка других доменов или production-публикация; назначенный S0 restore обязателен. Если build/native deadline не выполнен, сохранить достигнутый этап и закончить без нового запуска. Эти подготовка и исполнение возможны лишь после отдельного поручения на этот опыт; текущий аудит ограничен документами.

## Измерение и критерий причинного сравнения

Стоимость измеряется **только вокруг четырёх настоящих TRAIN updates**: `GC.GetAllocatedBytesForCurrentThread`, `Stopwatch`, process CPU с `Refresh` перед вторым чтением, GC0/1/2. Подготовка, чтение файлов, сборка, hashes/readout и запись отчёта находятся вне этого участка и учитываются отдельно. Process peak working set — отдельная строка с честной областью «весь процесс до окончания TRAIN», не retained heap и не выделения.

A0/B0 и A1/B1 содержат по каждому EXAM 256 последних logits, argmax token, exact и target NLL. Сохранить4 online pre-step losses на плечо, actual optimizer step404, отпечатки trainable state до/после и optimizer moments после, frozen-state unchanged. `actual_parameter_write` должен подтверждаться различием записанных trainable hashes; сам счётчик Step не заменяет запись параметров. Структура:1 block,2 LoRA slots,1 learner, Flow отсутствует. Не делать новых hash-сверок ради оформления отчёта; использовать уже записанный raw.

Сравнение: `ΔA=A1−A0`, `ΔB=B1−B0`, `ΔB−ΔA`. Для NLL/стоимости меньше лучше, для exact больше лучше. TRAIN cost — две цены переходов A0→A1 и B0→B1, **не** нулевая стоимость «до» против ненулевой «после».

Условие валидности: одинаковые входные байты/config/начальный state; действительно достигнуты все4 optimizer steps на плечо; B меняет только mapping; EXAM не попал в TRAIN. Если это не выполнено — `INVALID_CAUSAL_COMPARISON`/partial, без вывода об эффекте. Ожидаемая численная эквивалентность — отдельный результат, не предлог отрицать измеренный выигрыш allocations. Любое ухудшение NLL, exact, CPU, wall или process peak показать рядом. Один короткий запуск не определяет устойчивую среднюю производительность; шум времени не объяснять без фактических данных.

## Отчёт и кандидат для решения Любы

Язык — русский; обычный Markdown, **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results/report.md`**, до350 слов плюс фактическая таблица. Скилы из уже прочитанной сессии: `architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`/фактически применённый plan-self-validation, `generated-code-review-SKILL.md`, `output-readability-gate-SKILL.md`; термины — `human-communication-GLOSSARY.md`. Для отчёта не искать другой шаблон/модель/навык и не перечитывать библиотеку.

Разделы: **что меняли → на чём проверяли → таблица A0/A1/B0/B1 и цены TRAIN → выигрыши и ухудшения → границы/кандидат**. При отсутствии значения писать «не измерено». Относительные Markdown links из `results/report.md`: `A/code-sign.json`, `B/code-sign.json`, `build.stdout`, `build.stderr`, `A/code-sign.stdout`, `A/code-sign.stderr`, соответствующие B logs, `execution.json`, `one-factor.diff` и `source-copy-manifest.json`. Не добавлять ещё один префикс `results/`. Отдельный `results/candidate.md` до120 слов — только при измеренном полезном улучшении: исходный метод и предлагаемый mapping diff, полезная ось, ухудшения, имя `report.md` из candidate; его точное будущее расположение — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results/report.md`. Успешный опыт становится кандидатом, основной код автоматически не меняется. Перенос и полная цепочка исправлений — по отдельному решению Любы.

После одного чтения созданного отчёта исправить только фактическую ошибку/битую ссылку/кодировку и закончить. Запрет на продолжение предыдущего опыта после команды перехода действует и на финальное оформление.

[trainer]: <C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:102>
[model]: <C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:219>
[block]: <C:/Projects/llm_mvp_core/03.TransformerModel/TransformerBlock.cs:46>
[rms]: <C:/Projects/llm_mvp_core/03.TransformerModel/Layers/RmsNorm.cs:29>
[broadcast]: <C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:168>
[coords]: <C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1008>
[backward]: <C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:187>
[dpo-forward]: <C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1018>
[source-manifest]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/source-manifest.json>
[host-program]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs>
[host-project]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/NEURAL-FROZEN-TRANSPOSE-01/Runner.csproj>
[host-run]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/run.py>


## Замкнутый список native исходников

Источник manifest: [локальный manifest](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-causal-delta-audit/RMS-COORD-ALLOC-01-SOURCE-MANIFEST.json>); SHA256 `a6c96035d50b79be6adc4297c460f8bc43efdee5663e4fd8122ed37cb1a1ba8e`. В текущем чтении Git-объекты всех 36 строк сверены с pin `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`; несовпадений нет. Это проверка происхождения, не сборка.

| Native путь относительно C:/Projects/llm_mvp_core | Git blob SHA-1 |
|---|---|
| `00.Core/OperationResult.cs` | `2bfde76ca7d99f63eb657e736330da177838c644` |
| `00.Core/SnapshotExceptionGraphClassifier.cs` | `af630e9b64fa2c438f4321d3ba9216841d76d152` |
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
| `02.DatasetPipeline/DataLoader.cs` | `9fb803ef90adbfc3a4ee61eb942d6ab671047996` |
| `02.DatasetPipeline/NpyReader.cs` | `dda1b8922699025413eb495cd1f0fc0aa9625c18` |
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
| `10.LoRA/LoraAdapterRandomSeeds.cs` | `26c7a56ba5dda49671c84e1deb11571b02e73a78` |
| `10.LoRA/LoraApplicator.cs` | `5eeaf38c5fc12fa2f88dc6c247a3fb2fc250716d` |
| `10.LoRA/LoraConfig.cs` | `a0b81255846f37b5e9449d6ccc883d924c3ed5f5` |
| `10.LoRA/LoraLinear.cs` | `1b321e99408c961b896896983d002e3b26425b85` |


## Локальный result-root RMS

Для нового `run.py` фиксировать `result = Path("C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RMS01/a001/results")`; это заменяет прежний root/results. До сборки проверить отсутствие этого result, создать только его родителей и затем result эксклюзивно. Проверка host/bin и host/obj остаётся отдельно. Все упоминания `results/...` в журнале выше означают дочерние пути именно этого result, а не host/results. Единственный no-argument вызов `python ./run.py` остаётся из назначенного host. Все результаты, one-factor.diff и manifest пишутся под result; параметры CLI native процессов содержат абсолютные пути, показанные выше.


**Немедленный переход/stop, также часть промпта исполнителю.** Команда пользователя «заканчивай и переходи» немедленно завершает текущий опыт и переводит к названной следующей задаче в пределах разрешённой фазы. После перехода не возвращайся дописывать report, hashes, review, retry или другие остатки предыдущего опыта. Команда «остановить всё» немедленно прекращает всю работу; не завершай ради оформления ни отчёт, ни проверки, ни повтор. Сохрани уже существующие результаты; новая команда не разрешает повтор ранее завершённого или прерванного запуска.
