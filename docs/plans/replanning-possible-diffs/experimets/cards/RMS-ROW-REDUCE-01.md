# RMS-ROW-REDUCE-01 — цена вычисления среднего внутри настоящего SFT/LoRA обучения

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница CORE/LEARN:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Исходный адрес и роль:** Фактор — `00.TensorCore/Core/TensorOps.cs::SumAlongAxis`, last-axis keepDims; caller Mean/RmsNorm в настоящем SFT/LoRA пути `04.TrainingLoop/Trainer.cs`. `03.TransformerModel/Randomness/LlmModelRandomState.cs` относится к общему старту, не к редукции. Исторический pin и собственный бюджет ниже сохраняются; роль зависимости не равна factor owner, runtime сейчас не выполнялся.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



<!-- BEGIN shared-start-binding-2026-10-21 -->
## Сохранённый старт: h64-rms-row/v1

**Действующий consumer-контракт:** [общее восстановление33](../33-shared-learned-starts.md), family `h64-rms-row`. Этот раздел заменяет повторную common preparation во всех командах и промптах ниже. Consumer выполняет **0 common updates,0 дополнительных common readouts**; исходные arm updates/readouts и единственный фактор сохраняются.

- Данные этого семейства: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/data.source.jsonl` и `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/data.material.json`. Заданный источник: `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/RMS-ROW-REDUCE-01.md`; рецепт: T+,T- bytes noLF;200cycles;stream=rms-row-reduce-probe-v1.
- Читать только `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/bundle.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/S0.weights.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/S0.optimizer.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/S0.rng.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/preparation.json`; generation/tokenizer/authority/source manifest — там же по33. Missing/partial/unqualified → конкретный SHARED_START_UNAVAILABLE, без самостоятельного обучения или другого checkpoint.
- Единственный producer: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001`, A-семантика [назначенной карточки](RMS-ROW-REDUCE-01.md); его конечный бюджет 400updates/404Forward/400Backward оплачивается один раз **отдельно**. Existing source для экспорта: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/common`.
- Будущий экспорт без model calls: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/Export-Existing.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`.
- Будущий однократный producer, только если он прямо включён в порученную программу: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/Prepare.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`. Это заданный интерфейс, ещё не реализованный этим документным проходом; consumer не вызывает его автоматически.
- Восстановление: S0 weights/RNG и полный SFT optimizer (m/v/step) из bundle. Проверить named weights/keys/shapes, полный optimizer state и native RNG identity; construction/ApplySeeded только **до** load. Только DPO consumer заново создаёт native admission/lease после load, без переноса из producer; SFT-only MASK/MOMENT/H64/SILU/CS01 не создают DPO lease.
- Единственный consumer entry остаётся `C:/Python314/python.exe C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/run.py`; его будущий wrapper вместо common Train читает fixed shared bundle, сохраняет его path/digest в study и восстанавливает A/B. R=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results`; в R/common разрешена неизменная входная копия shared snapshot для прежних внутренних путей. Она не создаёт preparation.claim/новые losses/updates. Reuse cost — чтение/restore, historical producer cost отдельно. Нельзя задним числом переписать claim уже выданного опыта.

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

Один общий W0 строится исходной ветвью A в этом же назначенном host, с V256/H64/L1/Q4/KV2/FF176/Context64, base seed62001, q/v-LoRA4/alpha8/dropout0, seed62002, stream `rms-row-reduce-probe-v1`. Native Trainer.TrainSupervisedNextTokenStep получает UTF-8 bytes **Prompt+Answer без LF/BOS/EOS**: T1=`C# result sign: 9-4 =+`, T2=`C# result sign: 4-9 =-`. Ровно200циклов T1→T2 = **400 updates**, weight1. Native AdamW(.9,.95,decay0,eps1e-8); constantLR.001, Warmup0, TotalSteps404, Clip1, GradAccum1. Native full-sequence shifted objective и frozen base/head не меняются. Переход400подготовительных steps не является фактором A/B.

COMMON W0 и S0 после400: ровно2prompt-only training:false forward на checkpoint, всего4; тот же последний logit предсказывает answer byte, как arm TRAIN readout. Без backward, Generate, дополнительного BPE или EXAM во время подготовки. Сохранить все256logits, exact, NLL, strict-greater gold rank; argmax ties по minID, order-rank с ties отдельным полем. Никакой интерпретации первого логита как полного многотокенного ответа: ответ здесь ровно один byte.

**Конечная квалификация:** common/preparation.json содержит `learning_qualified` (boolean), completed_updates, exact_by_id_W0, exact_by_id_S0, acquired_ids и failure_reason; wrapper требует exit0 **и** learning_qualified=true, иначе arms не выдаёт. минимум одна из фиксированных T1/T2 должна перейти из W0 exact=false в S0 exact=true. Это приобретённый полный **однотокенный ответ на названную строку**, не общий закон знака. Показать оба результата;1/2 не выше constant-label baseline1/2, а2/2 двух разных знаков отличимо от него. Для cost-фактора разрешён узкий вывод по приобретённой строке; общий навык остаётсяOPEN при1/2. Если такого перехода нет, записать PREPARATION_COMPLETED_LEARNING_UNQUALIFIED и не выдавать arms.400updates и4scores сохраняются; report не закрывает цель, повтор/автоNEXT запрещены. Не ждать появления2/2 дополнительными steps.

Только producer после400 создаёт bundle `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/S0.weights.bin`, S0.optimizer.bin, S0.rng.json, preparation.json. Native CreateNew SaveCheckpointToStream; после записи остальных state/receipt — точный typed weights API из binding выше и LoadCheckpoint с новым/rewound stream; named weights+shapes+floatbits совпадают с live S0. Bindings из actual GetCheckpointParameterEntries, отфильтрованных по attached q/v AdapterParameters reference identity, unique ordinal keys. ExportPortableState→ToBytes→FromBytes; A/B new AdamW.TryImportPortableState должны вернуть true; повторный Export сверяет каждый key/shape/m/v bit/step400. Model CaptureRandomState/RestoreRandomState использует именно исходную topology/LoRA identity и stream этой карточки. Это настоящий общий restore, не одинаковый seed вместо снимка. Сначала construction+ApplySeeded, затем Load; после Load никакой инициализации. A0/B0 — S0, optimizer400; после прежних четырёх arm updates обе стороны404. Fresh Trainer counter0 разрешён постояннымLR, optimizer counter не сбрасывается.

Consumer run.py после build читает qualified shared bundle; внутреннего запуска подготовки перед A нет:
Исторический локальный `--prepare-learned` снят с маршрута consumer. Единственная будущая подготовка — отдельный producer из shared-binding в начале карточки и контракта33; LC остаётся export-only. Consumer читает COMPLETE bundle, при его отсутствии отказывает без Train.
Producer — отдельный будущий entry из контракта33 с исходной A semantics и собственным common claim. Consumer wrapper создаёт root claim/study/nonce до build и arm claim до load. Direct mode без живого owner отказывает; existing claim/result не обходить новым aNN. Фактор B применяется только после S0 restore.

**Consumer:8 optimizer updates,8 training Forward/Backward,16 diagnostic Forward; всего24 Forward/8 Backward. Common Train/readout=0.** Отдельный producer:400 updates/404 Forward/400 Backward; сумма однократного producer и одного consumer:408/428/408. Generate/DPO/teacher0. Consumer сохраняет outer cap1800s, build≤180s, каждое arm≤120s, summary≤60s; неиспользованный preparation≤1200s не переносится в новые calls. Producer cap задан в33. Healthy same-handle wait по31; тишина не повод abort/relaunch.

A0/B0 state/readout должны совпасть с S0; в A1/B1 показывать learned answer retention отдельно от EXAM. Ресурсное сравнение допустимо только при завершённых реальных updates, finite state/logit/gradient/optimizer equivalence по прежней tolerance и показанном before; регрессия exact/NLL не скрывается за allocations. Любое полезное измеренное изменение сохраняется без нового gain threshold. Источники: [native updater:67–147](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:67), [portable m/v:188–290](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:188), [constant scheduler:28](C:/Projects/llm_mvp_core/04.TrainingLoop/Schedulers/CosineWithWarmup.cs:28). Эта секция входит в каждый промпт карточки; default модель будущей сессии без override, немедленный STOP/NEXT, no retry. Текущим поручением ни host, ни S0 не созданы.

**Готовность учебной постановки:** DESIGNED_REQUIRES_LEARNING_VALIDATION. Назначенные ниже start/data/schedule не обещают усвоения и не разрешают повторить уже известный нулевой рецепт. При acquisition проверяется приобретённое поведение; retention/restore допускается лишь при показанном навыке до вмешательства. Если сигнал отсутствует, learning objective остаётся OPEN: автор исправляет конкретный start/минимальный набор/бюджет до новой попытки, исполнитель не ищет замену и не добавляет steps до PASS. Один отчёт с INVALID_ZERO_TO_ZERO не завершает задание и не превращает его в успех ресурсов. Прямое NEXT Любы выполняется сразу; иных автоматических повторов нет. [Обязательный контракт30](../30-failure-recovery-and-learning-validity.md).

**Приоритет открытых учебных целей:** [PARENT-ADOPTION-01](PARENT-ADOPTION-01.md) → [N63-INGRESS-LENGTH-LEARNED-01](N63-INGRESS-LENGTH-LEARNED-01.md) → [N63-CORRECTION-COLLISION](N63-correction-collision.md). Это порядок работы, не заявление об их завершении. Состав и статусы — [текущий индекс](00-execution-index.md#next-contract). Старые raw/claims сохраняются; consumed entry N63C не повторять.

**Действующий маршрут:** [RMS-ROW-REDUCE-01](RMS-ROW-REDUCE-01.md) → [ATTN-TRANSPOSE-STRIDES-01](ATTN-TRANSPOSE-STRIDES-01.md). Позиция — [текущий индекс](00-execution-index.md#next-contract). Host=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001; R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results. Модель сессии по умолчанию. Source pin669 сохраняется; текущий HEAD вместо него не подставлять. Текущая фаза READ_ONLY_RESEARCH + документы.


Дата: 20.10.2026. Исходный core: `66947785d1318f7cbbc88118f3156cdc75001e16`. Статус: **гипотеза подтверждена как достижимый участок кода; влияние ещё не измерено; новый host не создан и не собран**. Текущая работа только документирует опыт. Эта карточка не объявляет найденной причину нулевых свободных ответов RP01.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения.** Для этого опыта предполагаемая полезная ось — выделенные байты на настоящий учебный переход; точность, NLL и фактическое состояние ученика остаются отдельными измерениями.

**Стоп и переход имеют немедленный приоритет.** После команды «стоп» / «закончить немедленно» прекратить действия, включая инструменты, запись отчёта, проверку и коммит. Ответить кратко уже известным состоянием. После команды «следующий эксперимент» прекратить дорабатывать прежний опыт и перейти только к названному следующему; сохранение или оформление прежнего результата не создаёт исключения. Старые эксперименты, результаты и незавершённые попытки не удалять. Никаких повторов ради более красивого результата.

## 1. Что проверяется и где это участвует в обучении

Вычисление RMS-нормализации вызывает `Mean(..., axis:-1, keepDims:true)` для каждой строки скрытых признаков. Текущий общий алгоритм суммирования сначала создаёт массив координат строки, затем **копирует этот массив для каждого складываемого элемента**. Для используемой последней оси все элементы строки уже расположены подряд; её начало вычисляется умножением номера строки на ширину. Гипотеза: адресация через последовательный индекс уменьшит выделения памяти и, возможно, время реального обучения, сохранив математическую операцию и полученное состояние.

| Звено реального учебного пути | Доказательство на закреплённом core |
|---|---|
| Данные урока → forward → CE → backward → clipping → AdamW | [`Trainer.TrainSupervisedNextTokenStep`, 102–147][trainer] |
| Forward проходит все блоки и final norm | [`LlmModel.Forward`, 219–242][model] |
| В каждом блоке две RMS-нормализации | [`TransformerBlock.Forward`, 46–64][block] |
| Нормализация действительно использует Mean последней оси | [`RmsNorm.Forward`, 29–40][rms] |
| Mean вызывает SumAlongAxis, затем делит сумму на axisSize; граф backward сохранён | [`TensorOps.Mean`, 530–586][mean] |
| Массив строки и отдельная его копия на каждое слагаемое | [`SumAlongAxis`, 1020–1047][sum] и [`FlatToIndices`, 1008–1018][flat] |

Получатель — настоящий собственный C# Transformer с q/v-LoRA. Тема обучения — язык программирования C#; узкая задача — определить знак целочисленного вычитания. Это не отдельный домен «RMS» и не kernel-only benchmark. Ученик обязан пройти реальное обучение; замер одних индексов, forward без update либо Python-формулы не отвечают на вопрос.

Этот механизм существует также на neural DPO forward, но карточка измеряет только SFT/LoRA: DPO updates — 0. Он не участвует в чистом kNN-пополнении памяти и не доказывает перенос эффекта на kNN, другие размеры модели или другие предметные темы.

## 2. Новизна и близкие прежние работы

Проверены все **799 Markdown possibleDifs** на planning `53cbcabd3926cda714bab428d294eb666301e9e4` и **155 Markdown каталога protected cards**, включая индекс (154 карточки), на core `66947785d1318f7cbbc88118f3156cdc75001e16`. Exact `SumAlongAxis` и `RMS-ROW` отсутствуют; семь широких совпадений отдельно рассмотрены. [Квитанция поиска](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-delta-669477/evidence/rms-row-reduce-novelty.json>).

- Прежний `RMS-COORD-ALLOC-01` изменяет `MultiplyBroadcast` и **явно исключает изменение Mean**. Это родственное семейство затрат адресации, но другой выполняемый метод и отдельный фактор. Не запускать оба фактора одновременно.
- `N-SHAPE-01` описывает ошибку backward при `keepDims:false`; здесь используется штатный `keepDims:true`, backward остаётся исходным. Исправление отсутствующей оси не входит в этот опыт.
- `GRADB-LOCALITY-01` меняет обход градиента правой матрицы MatMul; здесь порядок float-суммирования Mean и весь MatMul сохраняются.
- Подготовка/проверка новизны не является измеренным успехом. Поиск покрывает указанный Markdown-корпус и карточки, а не неизвестные неопубликованные работы и не все исторические ветки.

## 3. Единственный фактор A/B и адрес будущего исправления

**A:** исходный `SumAlongAxis`.

**B:** только при последней оси, сохранённом числе осей и результирующей последней размерности 1 — последовательное чтение той же строки. Остальные формы исполняют исходный код. Суммирование остаётся скалярным слева направо; никаких SIMD, параллельной редукции или перестановки слагаемых.

В экспериментальную копию `00.TensorCore/Core/TensorOps.cs` добавить `internal static bool ExperimentUseContiguousLastAxisSum`. В начале `SumAlongAxis` перед исходным циклом добавить только следующую ветвь:

```csharp
if (ExperimentUseContiguousLastAxisSum &&
    axis == srcShape.Rank - 1 &&
    srcShape.Rank == dstShape.Rank &&
    dstShape.Dimensions[axis] == 1)
{
    int axisSize = srcShape.Dimensions[axis];
    for (int dstFlat = 0; dstFlat < dstShape.TotalElements; dstFlat++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int start = dstFlat * axisSize;
        float sum = 0f;
        for (int k = 0; k < axisSize; k++)
        {
            if ((k & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            sum += src[start + k];
        }
        dst[dstFlat] = sum;
    }
    return;
}
```

Исходный цикл `SumAlongAxis` после этой ветви остаётся полностью. Не менять `Mean`, деление на axisSize, `Pow`, `RSqrt`, `MultiplyBroadcast`, `BroadcastGradAlongAxis`, конструкторы Tensor, `RequiresGrad`, LoRA, optimizer, данные или readout. В обоих плечах перед A0/B0 переключатель false; непосредственно перед TRAIN он получает `arm == "B"` и сохраняется до after-readout. Переключатель — инструмент изолированного опыта, а не предлагаемая конфигурация production.

Обоснование эквивалентности — вывод из адресации: при последней сохранённой оси источник текущего `(dstFlat,k)` равен `dstFlat*axisSize+k`; сохраняется `sum +=` в прежнем порядке и прежнее деление в Mean. Это ещё не native-проверка побитового равенства. Его проверяют **по уже произведённому учебному состоянию**, не дополнительным benchmark.

Адрес возможного переноса после решения Любы — этот же `TensorOps.SumAlongAxis`. Перенос в original source, исправление продуктовых тестов и полная цепочка CI сейчас не разрешены и не являются частью малого опыта.

## 4. Модель, данные и жёсткий бюджет

Исполнитель использует ту же модель и режим, что уже выбраны в назначенной сессии. Внешний teacher/judge не нужен; модель исполнителя и обучаемый собственный Transformer — разные сущности.

| Поле | Фиксированное значение |
|---|---|
| Native config | Vocab256, Context64, Hidden64, один блок, 4 Q heads, 2 KV heads, FFN176, RopeTheta10000, RmsNormEps1e−5 |
| Base initialization | `Initializers.UseDeterministicSeed(62001UL)` отдельно в каждом процессе |
| LoRA | `ApplySeeded`, q_proj/v_proj, rank4, alpha8, dropout0, seed62002UL, stream `rms-row-reduce-probe-v1` |
| Optimizer | `AdamW(.9f,.95f,weightDecay:0f,eps:1e-8f)` |
| TrainingConfig | Batch1, SeqLen64, warmup0, totalSteps404, maxLr=minLr=.001, gradAccum1, clip1, weightDecay0 |
| Токены | UTF-8 byte IDs 0…255 через `Encoding.UTF8`; BPE Train/Encode не вызываются; EOS не добавляется |
| Objective | Текущий full-sequence next-token SFT, weight1; маска ответа не добавляется |
| TRAIN | T1,T2,T1,T2: четыре вызова `TrainSupervisedNextTokenStep` на плечо; всего восемь updates |
| EXAM | Два prompt forward до и два после в каждом плече; всего восемь EXAM forward; argmax фактического последнего логита среди 256 |
| TRAIN diagnostic readout | Те же T1/T2 prompt отдельно до и после каждого плеча, без update: восемь дополнительных forward. Всего 16 readout forward:8 EXAM+8 TRAIN diagnostic |
| Другие вызовы | DPO0, внешние модели 0, warmup0, retries0, дополнительных seed0 |

| Split | ID | Prompt буквально | Answer буквально |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Это две учебные пары и два других входа для чтения результата, а не доказательство освоения арифметики или программирования вообще. Ответ `+`/`-` нельзя вычислять правилом вместо модели.

## 5. Подготовка без поиска места, model и способа запуска

Статус этого раздела: **рецепт реализации host; отсутствующий host не объявлен готовой командой**. Подготовка и native execution выполняются при назначении этой карточки, в этой сессии обучения не запускали.

1. Исходный checkout — `C:/Projects/llm_mvp_core`, read-only core pin указан выше. Единственный короткий корень — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001`. Если каталог уже существует, сохранить его и прочитать имеющийся status; не создавать a002/другую попытку и не перезапускать опыт лишь из-за конфликта каталога. Нужное новое исполнение определяется отдельной прямой командой пользователя, а не автоматической логикой. Старые файлы не удалять. При отсутствии .NET9 SDK записать конкретный технический блокер, не переходить к установке другого toolchain.
2. Создать в новой попытке только `src`, `Program.cs`, `Runner.csproj`, `run.py`, `one-factor.diff`, `source-copy-manifest.json`. `results` создаёт runner эксклюзивно; `bin/obj` создаёт сборка. Из core скопировать **ровно 36 файлов** [source manifest](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-delta-669477/evidence/rms-row-reduce-source-manifest.json>), сохраняя относительные пути внутри `src`. **До копирования и patch проверить actual Git blob SHA каждого из 36 исходных файлов против manifest.** Получать исходные bytes через `git -C C:/Projects/llm_mvp_core show 66947785d1318f7cbbc88118f3156cdc75001e16:<path>` как argv subprocess без shell, stdout в bytes, timeout10s на чтение; не применять CRLF-преобразования рабочего checkout. Точный Python hash: `hashlib.sha1(b"blob " + str(len(source_bytes)).encode("ascii") + bytes([0]) + source_bytes).hexdigest()`. Несовпадение или отсутствующий объект — конкретный PRECONDITION_FAILED до build/Train, без отключения guard и поиска другого source. Prepared SHA256 после patch не заменяет эту original-blob проверку. Manifest закреплён на current core; все 36 blob совпадают с составом существующего проверенного scaffold. Не заменять original Linear на cached variant.
3. Основа `Program.cs` — [точный опубликованный SFT host][host]. Сохранить model config, literal TRAIN/EXAM, updater, timing и readout. Поменять ID на `RMS-ROW-REDUCE-01`, commit на current pin, stream на `rms-row-reduce-probe-v1`. Заменить два переключения `Linear.ExperimentReuseFrozenTranspose` на `TensorOps.ExperimentUseContiguousLastAxisSum`; удалить чтение `ExperimentCacheHits/Misses/CachedFloats` и связанные cache-поля. В `mechanism` записать только `contiguous_last_axis_sum_enabled = arm == "B"`. Включить `subject = "programming-language-csharp"`, `task = "integer-subtraction-sign"`. `training_losses` подписать как online loss перед соответствующим update. Изменить helper на `Observe(LlmModel model, (string Prompt, string Answer)[] rows)`, заменив внутри `Exam.Select` на `rows.Select`. Для каждого row после получения target добавить `gold_rank = 1 + количество last_logits строго больше last_logits[target]`, `teacher_forced_exact = best == target`. До TRAIN вызвать `Observe(model, Exam)` и `Observe(model, Train)`, после TRAIN аналогично; записать `before/after` для EXAM и `training_panel_before/training_panel_after` для TRAIN. Все 16 readout forward находятся вне измеряемого TRAIN; не вычислять before/after разностью online losses разных шагов.
   В наследованном receipt заменить `domain = args[1]` на `profile = args[1]` и `domain = "programming-language-csharp"`; `code-sign` — имя профиля, не предметный домен. Заменить `byte_tokenizer = "...exactly native BPE with zero merges..."` на `"UTF-8 byte IDs 0..255 via Encoding.UTF8; no BPE Train/Encode; no EOS"`. Заменить `unit_count` на `native_learner_instances=1`, добавить `logical_unit_flow_scope="not used"`: wrapper обучает Transformer напрямую и не вызывает LogicalUnit/Flow. Это уточнение фактически выполненного пути, без изменения учебного фактора.
4. Новый плоский `Runner.csproj` содержит OutputType Exe, TargetFramework net9.0, ImplicitUsings enable, Nullable enable, EnableDefaultCompileItems false, AssemblyName `RMS-ROW-REDUCE-01`; Compile только `src/**/*.cs` и `Program.cs`, никаких ProjectReference. До SDK imports выставить `ImportDirectoryBuildProps/Targets=false`; BaseOutputPath/IntermediateOutputPath/MSBuildProjectExtensionsPath привязать к `bin/obj` внутри attempt. Существующий глубокий `ArtifactRoot` не наследовать.
5. `run.py` — новый no-argument runner с `root=Path(__file__).resolve().parent`, `result=root/'results'`, host=`root/'Runner.csproj'`. Один раз проверить prepared SHA256, отсутствие `results/bin/obj`; `result.mkdir(exist_ok=False)`; копировать prepared manifest/diff в results; одна сборка≤180s; затем проверка shared COMPLETE/learning_qualified/identity и restore без common Train, процессыA→B≤120s каждый, общийdeadline1800s. До соответствующего процесса создать `results/A` или `results/B`; конечный JSON открывается FileMode.CreateNew. Не использовать старые `--results-root`, `experiment.json` либо автоматический поиск checkout.
6. После финализации Program/runner/diff записать source-copy-manifest: core commit, path и original blob из соседнего manifest, SHA256 фактических 36 копий, Program, csproj, run.py и diff. Checksum не изменяет охваченные файлы. В обоих плечах одна сборка и один фиксированный A/B-переключатель; никаких изменений чужих проектов.

Единственная внешняя команда после подготовки новой первой попытки:

```powershell
Set-Location 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001'
C:/Python314/python.exe ./run.py
```

Другую попытку этот рецепт автоматически не выбирает. Внутри runner — строго:

```text
dotnet build Runner.csproj -c Release --output bin -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
dotnet bin/RMS-ROW-REDUCE-01.dll A code-sign results/A/code-sign.json
dotnet bin/RMS-ROW-REDUCE-01.dll B code-sign results/B/code-sign.json
```

Build logs: `results/build.stdout`, `build.stderr`; native logs: `results/A/code-sign.stdout`, `.stderr` и B counterparts. `execution.json` фиксирует stage, status, exit code, actual counts из существующего receipt и `no_auto_retry=true`. При build failure обучение не началось — это технический статус, не accuracy0. При timeout до receipt счётчик updates — null/«не измерено»; нельзя запускать ещё раз, чтобы узнать число. Product и прежние raw/results read-only; эта документная сессия правит только разрешённые Markdown/JSON.

Экспериментальные copies, tiny literal fixtures и один переключатель допустимы только здесь; они не разрешают менять production или подменять настоящий learner на surrogate. Нет широких unit tests, solution build, Windows CI, Main500, новых BPE/KNN/restore опытов или исправления продуктовых тестов до доказанного результата.

## 6. Измерение до/после и валидность

Измерить два перехода A0→A1 и B0→B1, а не нулевую цену before против ненулевой цены after. Primary cost scope — четыре настоящих TRAIN вызова; в него входят forward/backward/clipping/AdamW. Сохранить allocated bytes (`GC.GetAllocatedBytesForCurrentThread`), elapsed (`Stopwatch`), process CPU с Refresh, GC0/1/2. Whole-process peak working set — отдельная величина, не retained heap и не сумма allocated bytes. Build, readout, hashes и сериализация снаружи TRAIN.

Для A0/B0 и A1/B1 сохранить каждый EXAM prompt и каждый фиксированный T1/T2 prompt,256 last logits, predicted token, teacher-forced exact, target NLL и gold rank (1 плюс число логитов строго выше gold). TRAIN diagnostic readout выполняется без update и вне TRAIN cost; он показывает before/after на одной и той же панели, тогда как online losses разных шагов её не заменяют. Зафиксировать четыре online losses на плечо; реальные optimizer steps4; trainable hashes до/после, optimizer moments hash после, frozen hashes без изменений. Размер модели/Unit/Flow отчитать прямо: один Transformer block, два LoRA slots, один neural learner, Flow не участвует. `actual_parameter_write` требует изменённого trainable state; счётчик шагов сам по себе его не доказывает.

Валидность: одинаковые начальные states/config/data; только одна source-интервенция; все 4 actual updates на плечо; EXAM не обучает; after снимается после завершения updates. Сравнить ΔA=A1−A0, ΔB=B1−B0 и ΔB−ΔA. Для NLL и cost меньше лучше; для exact больше лучше. Ожидается одинаковое состояние A1/B1 и полезная экономия allocations. Если возникло численное расхождение, сохранить его и ограничить утверждение: нельзя называть замену семантически нейтральной без разбора, но фактическое улучшение отдельной полезной оси не скрывать.

Ни один выигрыш пока не измерен. Один фиксированный порядок A→B не доказывает устойчивое причинное ускорение по wall time; любые фактические timings показываются с этим ограничением. Изменение RAM, CPU, NLL, exact и GC, включая ухудшения, обязательно рядом.

## 7. Отчёт и следующий шаг

Порядок в объединённом реестре: `VT-APOSTROPHE-01` → **`RMS-ROW-REDUCE-01`** → `ATTN-TRANSPOSE-STRIDES-01`. `next_id — [ATTN-TRANSPOSE-STRIDES-01](ATTN-TRANSPOSE-STRIDES-01.md)` после закрытия этой карточки — указатель следующей работы, не разрешение автоматически запускать второй опыт. Прямая команда пользователя о следующем эксперименте имеет приоритет.

Русский Markdown, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/report.md`, максимум 350 слов плюс таблица. Готовые разделы: «Что меняли»; «Тема, задача и ученик»; «A0/A1/B0/B1 и стоимость TRAIN»; «Что улучшилось и ухудшилось»; «Ограничения и кандидат». Неизмеренное — «не измерено», не 0. Links относительно results: `A/code-sign.json`, `B/code-sign.json`, `execution.json`, `one-factor.diff`, `source-copy-manifest.json`, `build.stdout`, `build.stderr` и A/B logs. Не добавлять второй `results/` в ссылки.

При измеренном улучшении полезной оси — `results/candidate.md` до 120 слов: source `TensorOps.SumAlongAxis`, предлагаемый last-axis diff, ось/домен, все существенные ухудшения, `report.md рядом с candidate.md (будущая относительная ссылка)`. Кандидат требует решения Любы для переноса; прежние успехи и эксперименты не удалять. После одного чтения готового отчёта исправить только факт/ссылку/кодировку и закончить. Новые запуски и оформление до бесконечности запрещены.

Скилы для исполнения заданы прямо: `docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`; применяются из уже назначенного core pin. Прямые текущие правила пользователя выше исторических требований mandatory accuracy/порогов и полной production-проверки. Для отчёта повторно искать skills, шаблон, язык или модель не нужно.

[trainer]: <C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:102>
[model]: <C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:219>
[block]: <C:/Projects/llm_mvp_core/03.TransformerModel/TransformerBlock.cs:46>
[rms]: <C:/Projects/llm_mvp_core/03.TransformerModel/Layers/RmsNorm.cs:29>
[mean]: <C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:530>
[sum]: <C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1020>
[flat]: <C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1008>
[host]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs>

## Внутреннее уточнение lifecycle и source closure

Единственный entry: из Host C:/Python314/python.exe ./run.py, без другого wrapper. Будущий run.py до build и до results.mkdir проверяет старые results/bin/obj/execution.claim и атомарно создаёт host/execution.claim (O_CREAT|O_EXCL); сохраняет на любом исходе. Он один резервирует results; arm process получает точный existing arm directory от owner и первым создаёт training.claim до model/readouts/Train, не запускает собственный attempt guard повторно. При nonzero A B не начинать, partial не стирать; unknown outcome читать в той же session, не a002. Не добавлять optimizer calls или readouts ради markers.

Внешние global ImportDirectoryBuildProps/Targets=false обязательны; свой BaseIntermediateOutputPath/MSBuildProjectExtensionsPath выставить до Sdk.props через explicit SDK imports, OutputPath=Host/bin/, AppendTargetFrameworkToOutputPath=false. Компиляция только36 файлов фиксированного source manifest и Program.cs, список явный, не whole-repo glob. Состав manifest не означает, что host собран. GetAllocatedBytesForCurrentThread — allocation именно измеряемого потока, не общий managed/process RAM; стоимость4синхронных Train отдельно от16readouts. Predicted token выбирается по max logit с min-token-ID tie; gold_rank считает только строго большие logits. Delta trainable0 записывается как observed0 и не разрешает повтор/новый урок.

Точные обязательные навыки: C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/phase-planning-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/code-examination-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md. Модель сессии по умолчанию без override; Student отдельно задан карточкой. Отчёт русский, назначенный R/report.md до350 слов плюс факты/raw; candidate до120 слов и при полезном результате краткая запись в [required-changes.md](../../required-changes.md). STOP/NEXT важнее оформления. Никакой сборки/обучения текущий документ не разрешает.

При просьбе «остановить всё» остановись немедленно, без отчёта/проверок/повтора. Одиночное поручение заканчивается END; переход только по назначенному NEXT.

**Закреплённый scaffold без поиска:** [closure36 и borrowed Program](../../2026-10-20-card-reconciliation/d7-rms-source-closure.json). Source36 берётся только из core669. Основа Program читается через git -C C:/Users/lubao/Documents/xiigen show 53cbcabd3926cda714bab428d294eb666301e9e4:<borrowed_program.path из этого manifest>; blob f3601c9ae3785312250d942e3646148a049f2ff5. Файл не берётся из изменяемого HEAD и не копируется как готовый RMS runner: ровно назначенные удаления cache-toggle/receipts и новый SumAlongAxis switch уже перечислены в §5. Операция — будущая bounded подготовка, не запуск модели и не новые входные данные.
