# ATTN-TRANSPOSE-STRIDES-01 — координатные массивы при перестановке голов attention

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница CORE:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения.

**Исходный адрес и роль:** Фактор — `00.TensorCore/Core/TensorOps.cs::Transpose` rank4/axes1,2; caller `03.TransformerModel/Layers/CausalSelfAttention.cs::Reshape4D/Reshape3D`, learner `04.TrainingLoop/Trainer.cs`. `03.TransformerModel/Randomness/LlmModelRandomState.cs` — зависимость восстановления RNG, не изменяемый transpose. Исторический pin и собственный бюджет ниже сохраняются; роль зависимости не равна factor owner, runtime сейчас не выполнялся.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001/results/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



<!-- BEGIN shared-start-binding-2026-10-21 -->
## Сохранённый старт: h64-attn-transpose/v1

**Действующий consumer-контракт:** [общее восстановление33](../33-shared-learned-starts.md), family `h64-attn-transpose`. Этот раздел заменяет повторную common preparation во всех командах и промптах ниже. Consumer выполняет **0 common updates,0 дополнительных common readouts**; исходные arm updates/readouts и единственный фактор сохраняются.

- Данные этого семейства: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/data.source.jsonl` и `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/data.material.json`. Заданный источник: `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/ATTN-TRANSPOSE-STRIDES-01.md`; рецепт: T+,T- bytes noLF;200cycles;stream=attention-transpose-probe-v1.
- Читать только `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/bundle.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/S0.weights.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/S0.optimizer.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/S0.rng.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/preparation.json`; generation/tokenizer/authority/source manifest — там же по33. Missing/partial/unqualified → конкретный SHARED_START_UNAVAILABLE, без самостоятельного обучения или другого checkpoint.
- Единственный producer: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001`, A-семантика [назначенной карточки](ATTN-TRANSPOSE-STRIDES-01.md); его конечный бюджет 400updates/404Forward/400Backward оплачивается один раз **отдельно**. Existing source для экспорта: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001/results/common`.
- Будущий экспорт без model calls: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/Export-Existing.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`.
- Будущий однократный producer, только если он прямо включён в порученную программу: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/Prepare.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`. Это заданный интерфейс, ещё не реализованный этим документным проходом; consumer не вызывает его автоматически.
- Восстановление: S0 weights/RNG и полный SFT optimizer (m/v/step) из bundle. Проверить named weights/keys/shapes, полный optimizer state и native RNG identity; construction/ApplySeeded только **до** load. Только DPO consumer заново создаёт native admission/lease после load, без переноса из producer; SFT-only MASK/MOMENT/H64/SILU/CS01 не создают DPO lease.
- Единственный consumer entry остаётся `C:/Python314/python.exe C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001/run.py`; его будущий wrapper вместо common Train читает fixed shared bundle, сохраняет его path/digest в study и восстанавливает A/B. R=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001/results`; в R/common разрешена неизменная входная копия shared snapshot для прежних внутренних путей. Она не создаёт preparation.claim/новые losses/updates. Reuse cost — чтение/restore, historical producer cost отдельно. Нельзя задним числом переписать claim уже выданного опыта.

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

Один общий W0 строится исходной ветвью A в этом же назначенном host, с V256/H64/L1/Q4/KV2/FF176/Context64, base seed62001, q/v-LoRA4/alpha8/dropout0, seed62002, stream `attention-transpose-probe-v1`. Native Trainer.TrainSupervisedNextTokenStep получает UTF-8 bytes **Prompt+Answer без LF/BOS/EOS**: T1=`C# result sign: 9-4 =+`, T2=`C# result sign: 4-9 =-`. Ровно200циклов T1→T2 = **400 updates**, weight1. Native AdamW(.9,.95,decay0,eps1e-8); constantLR.001, Warmup0, TotalSteps404, Clip1, GradAccum1. Native full-sequence shifted objective и frozen base/head не меняются. Переход400подготовительных steps не является фактором A/B.

COMMON W0 и S0 после400: ровно2prompt-only training:false forward на checkpoint, всего4; тот же последний logit предсказывает answer byte, как arm TRAIN readout. Без backward, Generate, дополнительного BPE или EXAM во время подготовки. Сохранить все256logits, exact, NLL, strict-greater gold rank; argmax ties по minID, order-rank с ties отдельным полем. Никакой интерпретации первого логита как полного многотокенного ответа: ответ здесь ровно один byte.

**Конечная квалификация:** common/preparation.json содержит `learning_qualified` (boolean), completed_updates, exact_by_id_W0, exact_by_id_S0, acquired_ids и failure_reason; wrapper требует exit0 **и** learning_qualified=true, иначе arms не выдаёт. минимум одна из фиксированных T1/T2 должна перейти из W0 exact=false в S0 exact=true. Это приобретённый полный **однотокенный ответ на названную строку**, не общий закон знака. Показать оба результата;1/2 не выше constant-label baseline1/2, а2/2 двух разных знаков отличимо от него. Для cost-фактора разрешён узкий вывод по приобретённой строке; общий навык остаётсяOPEN при1/2. Если такого перехода нет, записать PREPARATION_COMPLETED_LEARNING_UNQUALIFIED и не выдавать arms.400updates и4scores сохраняются; report не закрывает цель, повтор/автоNEXT запрещены. Не ждать появления2/2 дополнительными steps.

Только producer после400 создаёт bundle `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1/S0.weights.bin`, S0.optimizer.bin, S0.rng.json, preparation.json. Native CreateNew SaveCheckpointToStream; после записи остальных state/receipt — точный typed weights API из binding выше и LoadCheckpoint с новым/rewound stream; named weights+shapes+floatbits совпадают с live S0. Bindings из actual GetCheckpointParameterEntries, отфильтрованных по attached q/v AdapterParameters reference identity, unique ordinal keys. ExportPortableState→ToBytes→FromBytes; A/B new AdamW.TryImportPortableState должны вернуть true; повторный Export сверяет каждый key/shape/m/v bit/step400. Model CaptureRandomState/RestoreRandomState использует именно исходную topology/LoRA identity и stream этой карточки. Это настоящий общий restore, не одинаковый seed вместо снимка. Сначала construction+ApplySeeded, затем Load; после Load никакой инициализации. A0/B0 — S0, optimizer400; после прежних четырёх arm updates обе стороны404. Fresh Trainer counter0 разрешён постояннымLR, optimizer counter не сбрасывается.

Consumer run.py после build читает qualified shared bundle; внутреннего запуска подготовки перед A нет:
Исторический локальный `--prepare-learned` снят с маршрута consumer. Единственная будущая подготовка — отдельный producer из shared-binding в начале карточки и контракта33; LC остаётся export-only. Consumer читает COMPLETE bundle, при его отсутствии отказывает без Train.
Producer — отдельный будущий entry из контракта33 с исходной A semantics и собственным common claim. Consumer wrapper создаёт root claim/study/nonce до build и arm claim до load. Direct mode без живого owner отказывает; existing claim/result не обходить новым aNN. Фактор B применяется только после S0 restore.

**Consumer:8 optimizer updates,8 training Forward/Backward,16 diagnostic Forward; всего24 Forward/8 Backward. Common Train/readout=0.** Отдельный producer:400 updates/404 Forward/400 Backward; сумма однократного producer и одного consumer:408/428/408. Generate/DPO/teacher0. Consumer сохраняет outer cap1800s, build≤180s, каждое arm≤120s, summary≤60s; неиспользованный preparation≤1200s не переносится в новые calls. Producer cap задан в33. Healthy same-handle wait по31; тишина не повод abort/relaunch.

A0/B0 state/readout должны совпасть с S0; в A1/B1 показывать learned answer retention отдельно от EXAM. Ресурсное сравнение допустимо только при завершённых реальных updates, finite state/logit/gradient/optimizer equivalence по прежней tolerance и показанном before; регрессия exact/NLL не скрывается за allocations. Любое полезное измеренное изменение сохраняется без нового gain threshold. Источники: [native updater:67–147](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:67), [portable m/v:188–290](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:188), [constant scheduler:28](C:/Projects/llm_mvp_core/04.TrainingLoop/Schedulers/CosineWithWarmup.cs:28). Эта секция входит в каждый промпт карточки; default модель будущей сессии без override, немедленный STOP/NEXT, no retry. Текущим поручением ни host, ни S0 не созданы.

**Готовность учебной постановки:** DESIGNED_REQUIRES_LEARNING_VALIDATION. Назначенные ниже start/data/schedule не обещают усвоения и не разрешают повторить уже известный нулевой рецепт. При acquisition проверяется приобретённое поведение; retention/restore допускается лишь при показанном навыке до вмешательства. Если сигнал отсутствует, learning objective остаётся OPEN: автор исправляет конкретный start/минимальный набор/бюджет до новой попытки, исполнитель не ищет замену и не добавляет steps до PASS. Один отчёт с INVALID_ZERO_TO_ZERO не завершает задание и не превращает его в успех ресурсов. Прямое NEXT Любы выполняется сразу; иных автоматических повторов нет. [Обязательный контракт30](../30-failure-recovery-and-learning-validity.md).


**Действующий next_id — [JSON-OPTIONS-REUSE-01](JSON-OPTIONS-REUSE-01.md); числовая позиция — [индекс](00-execution-index.md).** Если назначен только один ID, после него завершить работу. Если уже назначена программа или дан переход «следующий», следовать этому literal NEXT из [реестра](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md>), без поиска другого опыта.

## Статус и приоритет

**`DESIGN_DOCUMENTED / TODO_IMPLEMENT_EXPERIMENT_HOST / NOT_RUN`.** Исходники прочитаны, один новый фактор отделён от соседних опытов, состав source-copy host зафиксирован. Новый `Program.cs`/runner ещё не реализован и не собран. Ни обучение, ни benchmark не запускались. Измеренного улучшения или доказанной причины плохих ответов сейчас нет. Это кандидат для малого исследования, не готовое исправление продукта.

Приоритет — **реальная стоимость обучения**, после разблокирования действующей учебной цепочки и устранения конфликтов активных карточек. Запускать только после назначения её ID в актуальном реестре; прежний `E-OPT-STEP` после позднего обновления уже выполнен и не назначается повторно. Старые эксперименты/результаты остаются. Исторический запрет аудитору менять protected не применяется к текущей документной правке.

**Поздняя reconciliation:** во время аудита core перешёл на `827a1acdbef95d220886e54f9d36bce0b405c8cd`. [Проверены одинаковые Git tree SHA всех шести source поддеревьев host](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-827a1acd/ATTN-TRANSPOSE-STRIDES-01-LATE-SOURCE-IDENTITY.json>), поэтому pin669 остаётся воспроизводимой версией тех же36 исходников. Наличие нового commit не означает, что этот опыт уже исполнен. Свежие protected AST/ROPE/BPE карточки проверены отдельно в [дополнении к novelty receipt](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-827a1acd/ATTN-TRANSPOSE-STRIDES-01-NOVELTY-799.json>); перенос очереди/результатов учитывает827, source pin не меняется молча.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения.** Для одиночной пары A/B различать `observed_metric_success` и `causal_attribution`: уменьшение измеренного времени остаётся наблюдаемым успехом времени, даже если устойчивое ускорение/его причина ещё не установлены. Успех не означает перенос в продукт.

## Запрет продолжать старый опыт

Обязательный протокол инцидента: пользователь сообщила, что агент **«четыре раза вместо переходя к следущему эксперименту продолжаешь прошлую задачу!!!»**, и потребовала **«Закончить немедленно!!!!!!»**. Недопустимо воспроизводить это поведение.

При новом прямом «стоп»/«закончить немедленно» немедленно прекратить действия, включая tools, отчёт, hash-проверки, review и git, и кратко сообщить уже известное состояние. При «следующий эксперимент» прекратить улучшать/проверять/дописывать предыдущий и перейти только к названному следующему. Не оправдывать продолжение незавершённым оформлением. Нельзя повторять запуск, стилистически полировать отчёт, удалять attempt, чинить прежний опыт или продолжать его после команды перехода. Цитата выше фиксирует прошлый инцидент; она не является текущей командой остановить документарный аудит.

## Доказательство участия в настоящем обучении

Все core-ссылки закреплены на **`66947785d1318f7cbbc88118f3156cdc75001e16`**.

| Звено | Что делает конкретный код и почему это относится к обучению |
|---|---|
| [Trainer:102–145][trainer] | Из учебных token IDs строит next-token targets, вызывает `Forward(training:true)`, loss, native backward, clipping, `AdamW.Step` и очищает градиенты. |
| [LlmModel:219–242][model] | Тот же forward проходит Transformer blocks, final norm и vocabulary head. |
| [CausalSelfAttention:129–170][attention] | Выходы Q/K/V проекций направлены в `Reshape4D`; attention context — в `Reshape3D`. При q/v LoRA меняются проекции, но перестановки остаются на действующем пути. |
| [Reshape4D/Reshape3D:323–352][reshape] | Переводят `[B,T,heads,D] ↔ [B,heads,T,D]` через `TensorOps.Transpose(x,1,2)`. |
| [TensorOps.Transpose:710–783][transpose] | Оптимизация последних двух осей не подходит rank4/axes1,2. Общая ветвь на **каждом scalar** вызывает `FlatToIndices`, переставляет два индекса и копирует float в новый буфер. |
| [FlatToIndices:1008–1017][coords] | Каждый вызов создаёт `new int[shape.Rank]`; для этих attention-тензоров это `int[4]`. |
| [Transpose backward:775–779][transpose-backward] | Включает тот же transpose для градиента в `BackwardFunc`, затем `AccumulateGrad`; служебная адресация повторяется и в достижимых обратных ветвях. |

Следовательно, это не отдельный тест голой перестановки: **учебные строки → native SFT/LoRA → Q/K/V и context transpose → backward → AdamW → реальные изменённые LoRA веса → readout до/после**. Наличие лишних временных массивов установлено статически; фактическая цена и влияние на доступный учебный бюджет ещё не измерены. Не все проекции обязаны иметь trainable backward в каждом профиле: число вызовов измеряется, а не предполагается одинаковым для frozen K и trainable Q/V.

## Почему это новая проверка в прочитанном корпусе

[Полный receipt поиска по 799 Markdown possibleDifs и 339 Markdown защищённого корпуса](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-827a1acd/ATTN-TRANSPOSE-STRIDES-01-NOVELTY-799.json>) сохраняет запросы, количество файлов и совпадения. Для `ATTN-TRANSPOSE-STRIDES|Reshape4D|Reshape3D` совпадений нет; более широкий поиск проверен по смыслу. Это ограниченная проверка прочитанного snapshot, а не заявление об отсутствии идеи во всей истории Git.

В [защищённом обзоре 47][protected47] уже разобраны корректность перестановок и перенос градиента. **Новая гипотеза — ресурсная цена coordinate arrays внутри этих корректных active-activation перестановок.** `NEURAL-FROZEN-TRANSPOSE-01` кеширует транспонированные **замороженные матрицы весов** в `Linear`; `RMS-COORD-ALLOC-01` меняет indexing **broadcast умножения**; `GRADB-LOCALITY-01` меняет порядок доступа в **MatMul gradient**. Это другие владельцы, lifetime и вмешательства. Ни один из них не включается в плечо B здесь.

## Один фактор A/B и предлагаемый ремонт

**A:** исходный generic mapping `Transpose` на rank4 axes1↔2. **B:** для этого же случая вычислять адрес непосредственно целочисленными операциями, не выделяя `int[4]` на каждый scalar. Остальные shape/axes идут по исходному коду.

В **экспериментальную копию** `TensorOps.cs` добавить `internal static bool ExperimentAttentionTransposeOffsets`. После исходных ветвей identity и last-two-axes, перед generic `else`, вставить ветвь:

```csharp
else if (ExperimentAttentionTransposeOffsets && rank == 4 &&
         ((d1 == 1 && d2 == 2) || (d1 == 2 && d2 == 1)))
{
    int aSize = x.Shape.Dimensions[1];
    int cSize = x.Shape.Dimensions[2];
    int width = x.Shape.Dimensions[3];
    for (int flat = 0; flat < x.Shape.TotalElements; flat++)
    {
        if (flat > 0 && (flat & 4095) == 0)
        {
            TensorKernelProgress.Report("TensorOps.Transpose", flat);
            cancellationToken.ThrowIfCancellationRequested();
        }
        int d = flat % width;
        int remaining = flat / width;
        int c = remaining % cSize;
        remaining /= cSize;
        int a = remaining % aSize;
        int b = remaining / aSize;
        int destination = ((b * cSize + c) * aSize + a) * width + d;
        newData[destination] = x.Data[flat];
    }
}
```

У `[B,A,C,D]` source адрес — `((b*A+a)*C+c)*D+d`, target адрес после swap — `((b*C+c)*A+a)*D+d`. Та же формула применима на обратном transpose с обращёнными размерами. Это обоснование адресации, **не результат исполнения**.

Сохранить исходные normalization отрицательных осей, shape validation, `newData`, Tensor construction, `Children`, `BackwardFunc`, `AccumulateGrad`, float copies и порядок прохода `flat`. Cancellation/progress cadence остаётся на прежних flat offsets; существующие safeguards не удалять. Не вводить SIMD, pooled buffers, views, cache, новый objective, mask, RoPE или optimizer. `Linear.cs` берётся оригинальным из core, без frozen-transpose cache. В общую часть rank4 axes1↔2 можно добавить одинаковые для A/B два scalar счётчика `ExperimentEligibleCalls` и `ExperimentEligibleElements`; сброс вне измерений, на вызов один increment и прибавление `TotalElements`, без объектов/логирования в scalar loop. Они подтверждают фактическую достижимость и не подменяют bytes/time измерения.

## Фиксированная модель, данные и пределы домена

Исполнитель и последовательный проверяющий используют **модель своей сессии по умолчанию**, без override и поиска другой модели. Известное фактическое имя сохранить в metadata; если платформа не раскрывает — так и написать. Обучаемый Student — собственный native Transformer ниже. Проектная роль CODEX / Astra 6 ultracode для выбора кода/спецификации и оценки качества отделена от Student и от default исполнителя: в этом готовом tiny fixture нет вызовов внешнего judge или учителя. Генерация новых DPO-пар китайской open-моделью здесь не нужна; external calls0, DPO updates0. Не заменять Student внешним LLM.

| Поле | Зафиксированное значение |
|---|---|
| Student | `XIIGEN-native-1block-SFT-LoRA-64` |
| Config | Vocab256, Context64, Hidden64, 1 block, 4 Q heads, 2 KV heads, FFN176, RoPE10000, RMS eps1e−5 |
| Base initialization | `Initializers.UseDeterministicSeed(62001UL)` |
| LoRA | `ApplySeeded`, q_proj/v_proj, rank4, alpha8, dropout0, rootSeed62002, stream `attention-transpose-probe-v1` одинаков для A/B |
| Learning | `Trainer.TrainSupervisedNextTokenStep`, full-sequence shifted next-token SFT, weight1 |
| Optimizer | Native AdamW beta1=.9, beta2=.95, eps1e−8, weightDecay0 |
| Schedule | Warmup0, TotalSteps404, MaxLr=MinLr=.001, clip norm1, gradAccum1 |
| Tokens | UTF-8 bytes IDs0…255, zero BPE merges, no EOS, без смены tokenizer |
| TRAIN | 4 update/arm, порядок T1,T2,T1,T2; всего **8 optimizer updates** |
| Readout | В каждом A0/A1/B0/B1 ровно 2 TRAIN и 2 EXAM forward, **16 diagnostic forward** суммарно; backward на readout запрещён |
| Структура | 1 learner/block, 2 LoRA slots; Flow/units router не участвуют |

| Split | ID | Prompt буквально | Answer буквально |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Предметная область — программирование на C#, задача — знак целочисленного вычитания. Это базовые короткие случаи существующего host, не проверка общей компетенции или построения AST. Материалы документации, перевод и другие темы могут использовать тот же native learner и такие же tensor shapes, поэтому возможен ресурсный перенос; **они не измерены этой панелью**. Не объявлять улучшение docs/перевода без отдельного назначенного доменного опыта. Эффект зависит прежде всего от последовательности/heads/достижимости backward, а не от наличия синтаксического дерева.

## Точное место и состав будущего host

Рабочая платформа рецепта — Windows, уже используемый .NET9 SDK и Python. Единственный корень первой попытки — **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001`**. Если он существует, сохранить его и завершить подготовку статусом `ATTEMPT_EXISTS`; автоматического удаления, продолжения и выбора новой папки нет. Новая попытка требует нового назначения. При отсутствии SDK/commit/исходника — назвать конкретную недостающую вещь и завершить этот опыт, не искать проект/версию/модель.

1. В новом attempt создать `src`, `Program.cs`, плоский `Runner.csproj`, `run.py`, `compare.py`. **Не создавать `results/bin/obj`** при preparation. Сейчас это `TODO_IMPLEMENT_EXPERIMENT_HOST`, не файлы, которые уже доступны для запуска.
2. [Соседний source manifest](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/2026-10-20-card-reconciliation/0ea-attn-source-closure.json>) содержит **ровно36** core paths и Git blob SHA-1 текущего pin. Из `C:/Projects/llm_mvp_core` экспортировать эти пути через `git show 66947785d1318f7cbbc88118f3156cdc75001e16:<path>` в `src/<path>` с сохранением stdout bytes; рабочий checkout core не переключать и не редактировать. Все36 blobs совпали с inventory прежнего host. Этот факт касается исходников, не успешности сборки. Особенно `Linear.cs` — оригинал core, не patched copy старого опыта.
3. Использовать ровно [существующий Program][host-program] и [Runner.csproj][host-project] frozen-transpose host на planning **`53cbcabd3926cda714bab428d294eb666301e9e4`** как шаблоны; [старый source manifest][host-manifest] задаёт происхождение состава. Если источник уже есть в локальном planning checkout, экспортировать pin через `git show`, не брать незакоммиченный изменённый вариант. Никакого поиска иного учебного проекта.
4. В новом Program заменить ID/commit/LoRA stream значениями карточки. Удалить все обращения `Linear.ExperimentReuseFrozenTranspose`, `ExperimentCacheHits`, `ExperimentCacheMisses`, `ExperimentCachedFloats` и cache-readouts. Добавить только switch/same counters из раздела A/B. Для обоих A0/B0 switch=false; непосредственно перед TRAIN switch=`arm == "B"`, сохранить это значение до A1/B1. Ровно одна сборка содержит оба плеча; каждый arm — отдельный процесс.
5. Вместо прежнего `Observe(Exam)` сделать `Observe(Train)` и `Observe(Exam)` до/после. Старый `Observe` включает NLL и `ToArray`, поэтому разделить его явно: внутри readout cost-window выполнить ровно четыре native forward и удержать возвращённые Tensor; после закрытия окна получить из этих же Tensor argmax, target NLL, rank, `ToArray`/float bits и report DTO. Дополнительных inference для вычисления метрик нет. Собрать actual `optimizer.CaptureState()` вне TRAIN, начальные/конечные trainable и frozen arrays в стабильном порядке `model.Parameters()`. Не переносить `CaptureState` внутрь step loop.
6. Плоский project: `TargetFramework=net9.0`, `AssemblyName=ATTN-TRANSPOSE-STRIDES-01`, `EnableDefaultCompileItems=false`, Compile только `src/**/*.cs` и `Program.cs`, без ProjectReference. `ImportDirectoryBuildProps=false` и `ImportDirectoryBuildTargets=false` выставить **до SDK imports**; все outputs/intermediates/extensions внутри `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001/bin` и `/obj`. Не собирать solution/dispatcher, не запускать NUnit, CI или production tests.
7. Новый no-argument `run.py` основан на [существующем runner][host-run], но не читает legacy `experiment.json`/`--results-root`: literal `root=Path(__file__).resolve().parent`, `host=root/'Runner.csproj'`, `result=root/'results'`, `jobs=[('A','code-sign'),('B','code-sign')]`. Создать manifest **последним**, после правок: SHA256 `Program.cs`, `Runner.csproj`, `run.py`, `compare.py`, `one-factor.diff`, плюс 36 copied source SHA256 и исходные blob из соседнего manifest. `one-factor.diff` — только experiment switch/counters и rank4 branch в TensorOps. Никаких изменений других core методов. Подготовка не запускает build/training.
8. Runner один раз проверяет готовые checksums, отклоняет существующие `results/bin/obj`, затем эксклюзивно создаёт `results` и копирует туда prepared manifest/diff. Ровно один build≤180s; затем common preparation≤1200s, после learning_qualified=true A и B≤120s каждый; общийdeadline1800s. До создания модели Program эксклюзивно открывает output JSON `FileMode.CreateNew`; частичный файл после сбоя не удалять. Нельзя строить модель/обучать, а лишь затем проверять конфликт output. Стандартные stdout/stderr paths заданы ниже.
9. `compare.py` **только читает готовые raw JSON**, проверяет identity/float bit equality и считает deltas; ничего не запускает, не восстанавливает и не переобучает. Сохраняет новый `results/comparison.json` эксклюзивно. При неполном raw выдаёт PARTIAL и null для неизвестного, не accuracy0. После comparison ровно один русский отчёт из этих артефактов.

### Команды после отдельного назначения и завершённого preparation

```powershell
Set-Location 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001'
C:/Python314/python.exe ./run.py
C:/Python314/python.exe ./compare.py
```

Внутри `run.py` команды фиксированы, cwd — этот attempt:

```text
dotnet build Runner.csproj -c Release --output bin -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
dotnet bin/ATTN-TRANSPOSE-STRIDES-01.dll A code-sign results/A/code-sign.json
dotnet bin/ATTN-TRANSPOSE-STRIDES-01.dll B code-sign results/B/code-sign.json
```

Build logs — `results/build.stdout`, `results/build.stderr`; native logs — `results/A/code-sign.stdout`, `.stderr` и аналогично B. `results/execution.json`: status/stage/exit code/jobs/deadline/actual count из завершённого receipt/`no_auto_retry=true`. Сбой build — `results/status.json` с `BUILD_FAILED`, `BUILD_TIMEOUT` или `BUILD_TOOL_MISSING`; обучение не началось. Timeout native без полного receipt — actual steps `null`, не0. Ошибка заканчивает attempt: без cleanup, retry, смены SDK или ремонта production. Стоп/переход имеет приоритет и над записью этих файлов.

## Что измеряется и как сравнивается

**TRAIN cost — цена четырёх update A0→A1 и B0→B1**, а не фиктивный нулевой расход «до». Измерить `GC.GetAllocatedBytesForCurrentThread`, elapsed Stopwatch, process CPU с `Refresh` перед чтениями, GC0/1/2 строго вокруг четырёх native updates. Сборка, загрузка, диагностика, snapshots, serialization и отчёт — снаружи. Student/optimizer не пересоздавать между шагами. Не делать скрытых warmup updates, дополнительных seeds или повторов.

В каждой из четырёх диагностических точек A0/A1/B0/B1 измерить отдельно **readout allocation bytes** вокруг имеющихся TRAIN+EXAM forward, затем вне этого окна вычислить/записать метрики из их выходов. Не добавлять их к TRAIN cost. Их before/after deltas относятся к readout обученной модели, а не выдаются за стоимость update. `process_peak_working_set_bytes` — отдельно, область «процесс до конца TRAIN», не retained heap и не сумма allocations. Снять peak до after-readout и serialization.

| Данные в raw | Правило |
|---|---|
| TRAIN/EXAM по каждой строке в A0/A1/B0/B1 | Сохранить prompt, gold, predicted byte, exact, target NLL, target rank, 256 last logits. NLL — `logsumexp(last_logits)−target_logit`; rank — 1 + число logits больше target + число равных target logits с меньшим token ID. Argmax ties тоже по меньшему token ID. |
| Численная эквивалентность | Для того же forward сохранить shape и **float32 bit patterns всех logits**, а не только argmax. Для начальных/конечных trainable/frozen параметров — ordinal, shape, bits. Для optimizer — step, ordinal соответствующего parameter, bits обоих moments; пустой initial optimizer тоже записать. Эти массивы сохраняются за пределами TRAIN/readout cost окон. |
| Учебный переход | Ровно4 online pre-step losses, `actual_optimizer_steps=4`, `actual_parameter_write` из реального изменения trainable bits, frozen unchanged, counters eligible calls/elements отдельно для TRAIN. Счётчик Step один не доказывает обучение. |
| Парное сравнение | A0=B0 по model/config/data/state; A1=B1 по full outputs, trainable/frozen state и optimizer moments; все4 online loss bits равны. Comparator сравнивает записанные bits без новых model calls. |

Для каждого качества отдельно: `ΔA=A1−A0`, `ΔB=B1−B0`, разность `ΔB−ΔA`. TRAIN и EXAM, NLL, rank, exact не смешивать. Низкие NLL/rank/bytes/time лучше; exact выше лучше. При ответе в один byte argmax действительно задаёт весь ответ этого fixture; нельзя переносить это равенство на длинную генерацию.

Валидность причинного сравнения требует одинаковых inputs/seeds/model/init, 4 updates в каждом плече, actual parameter write, unchanged frozen state и единственного фактора. **Bit mismatch не скрывать**: это не подтверждённая точная оптимизация. Сохранить измеренные улучшения и ухудшения, установить `exact_optimization_supported=false`, объяснить изменение поведения; не продолжать «чинить» и не переименовывать mismatch в успех эквивалентности.

У результата две независимые оси: `observed_metric_success` (для каждой полезной метрики по её фактической дельте) и `causal_attribution` (`SUPPORTED_IN_THIS_PAIRED_RUN` / `NOT_ESTABLISHED` / `INVALID_COMPARISON`). Одиночный fixed-order A→B запуск не устанавливает устойчивое среднее время/CPU/peak; его полезная измеренная дельта всё равно фиксируется. Missing/failed values — «не измерено». Статически ожидаемое уменьшение числа массивов не заполняет измеренный allocated bytes.

## Отчёт, скилы и кандидат

Результат будущего исполнения — **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001/results/report.md`**, русский Markdown, **до350 слов плюс таблица**. Разделы: что меняли; на чём проверяли; A0/A1/B0/B1; цена TRAIN; выигрыши и ухудшения; causal attribution/ограничения/кандидат. Таблица качества содержит по split/метрике A0,A1,B0,B1,ΔA,ΔB,ΔB−ΔA; отдельная таблица стоимости — A TRAIN,B TRAIN,B−A,observed success,attribution. В обеих нет придуманных чисел.

Использовать уже заданные project skills: `trainable-architecture-decision-SKILL.md`, `intent-to-plan-SKILL.md`, `plan-self-validation-SKILL.md`, `generated-code-review-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md` из `docs/skills` pinned core. Пользовательский малый бюджет и запреты выше общих skill-требований к production tests/promotion. Не искать ещё один формат, язык, модель или стек скилов.

Links из report: `A/code-sign.json`, `B/code-sign.json`, `comparison.json`, `execution.json`, `one-factor.diff`, `source-copy-manifest.json` и существующие logs без лишнего префикса `results/`. При любом измеренном полезном улучшении создать **`results/fix-candidate.md`**, до120 слов: исходный `TensorOps.Transpose`, proposed rank4 branch, метрика/домен/дельта, все известные ухудшения, attribution и `report.md (относительная ссылка внутри R)`. Статус «кандидат для решения Любы», не prod-ready. Нельзя добавлять кандидат с выдуманным успехом, удалять прежние результаты, автоматически переносить diff в original core или запускать полную цепочку тестов.

После одного чтения отчёта исправить только фактическую ошибку, битую ссылку или кодировку и закончить. Если пользователь уже велела следующий опыт — не выполнять и это чтение. Дальнейшие исправления original `00.TensorCore/Core/TensorOps.cs` и полная проверка возможны после её отдельного решения.

[trainer]: <C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:102>
[model]: <C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:219>
[attention]: <C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:129>
[reshape]: <C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:323>
[transpose]: <C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:710>
[coords]: <C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1008>
[transpose-backward]: <C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:775>
[protected47]: <C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/47-current-attention-backprop-and-position-contract.md:30>
[host-program]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs>
[host-project]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/NEURAL-FROZEN-TRANSPOSE-01/Runner.csproj>
[host-manifest]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/source-manifest.json>
[host-run]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/run.py>


**Применение в этой сессии:** только Markdown/JSON; приведённые source blocks остаются будущим заданием, реальные host/source/data/weights сейчас не создаются. Исполнитель — модель его сессии по умолчанию без override. Student указан отдельно. При «остановить всё» остановиться немедленно; при «следующий» немедленно перейти по назначенному NEXT, без report/hash/review/Git/retry прежнего опыта. При отдельном поручении одного ID — END.

При обычном завершении измеренного полезного опыта: русский report по карточке и одна запись до120слов в [required-changes.md](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/required-changes.md) со ссылкой на report, метрикой, доменом и ухудшениями. Отдельный candidate рядом с raw служит содержимым этой записи; дополнительных запусков нет. STOP/NEXT выше оформления. Все экспериментальные послабления (минимальный host, throw/try-catch/void) действуют; полный product lifecycle не требуется.

**Защита старой попытки:** перед подготовкой читать наличие C:/x/at01/a001; любой существующий legacy root/claim/raw означает ATTEMPT_EXISTS. Перенос пути не разрешает повтор. Единственный новый root — C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AT01/a001. Runner эксклюзивно создаёт root/execution.claim до results/build. Создатель results — только run.py; output JSON arm открывается CreateNew до model/Observe/Train. Наружный run.py выполняется один раз, затем один compare.py; их внутренние команды отдельно не повторять.

**Точные навыки:** [trainable-architecture-decision-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md), [intent-to-plan-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/intent-to-plan-SKILL.md), [plan-self-validation-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/plan-self-validation-SKILL.md), [generated-code-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md), [experiment-design-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/experiment-design-SKILL.md), [prompt-patch-authoring-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md), [human-readable-architecture-audit-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md), [human-communication-GLOSSARY.md](C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md). Нового стека/модели не выбирать.
