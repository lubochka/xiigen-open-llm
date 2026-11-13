# SILU-SIGMOID-REUSE-01 — повторное использование знаменателя SiLU в настоящем обучении

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница CORE:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения.

**Исходный адрес из паспорта:** `00.TensorCore/Core/TensorOps.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



<!-- BEGIN shared-start-binding-2026-10-21 -->
## Сохранённый старт: h64-silu/v1

**Действующий consumer-контракт:** [общее восстановление33](../33-shared-learned-starts.md), family `h64-silu`. Этот раздел заменяет повторную common preparation во всех командах и промптах ниже. Consumer выполняет **0 common updates,0 дополнительных common readouts**; исходные arm updates/readouts и единственный фактор сохраняются.

- Данные этого семейства: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/data.source.jsonl` и `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/data.material.json`. Заданный источник: `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/SILU-SIGMOID-REUSE-01.md`; рецепт: T+,T- bytes noLF;200cycles;stream=frozen-transpose-probe-v1;TotalSteps400;8commonreadouts inclEXAM.
- Читать только `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/bundle.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/S0.weights.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/S0.optimizer.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/S0.rng.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/preparation.json`; generation/tokenizer/authority/source manifest — там же по33. Missing/partial/unqualified → конкретный SHARED_START_UNAVAILABLE, без самостоятельного обучения или другого checkpoint.
- Единственный producer: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01`, A-семантика [назначенной карточки](SILU-SIGMOID-REUSE-01.md); его конечный бюджет 400updates/408Forward/400Backward оплачивается один раз **отдельно**. Existing source для экспорта: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/s0/SILU01-R1`.
- Будущий экспорт без model calls: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/Export-Existing.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`.
- Будущий однократный producer, только если он прямо включён в порученную программу: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1/Prepare.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`. Это заданный интерфейс, ещё не реализованный этим документным проходом; consumer не вызывает его автоматически.
- Восстановление: S0 weights/RNG и полный SFT optimizer (m/v/step) из bundle. Проверить named weights/keys/shapes, полный optimizer state и native RNG identity; construction/ApplySeeded только **до** load. Только DPO consumer заново создаёт native admission/lease после load, без переноса из producer; SFT-only MASK/MOMENT/H64/SILU/CS01 не создают DPO lease.
- Единственный consumer entry остаётся `pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/Run.ps1`; его будущий wrapper вместо common Train читает fixed shared bundle, сохраняет его path/digest в study и восстанавливает A/B. R=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1`; в R/common разрешена неизменная входная копия shared snapshot для прежних внутренних путей. Она не создаёт preparation.claim/новые losses/updates. Reuse cost — чтение/restore, historical producer cost отдельно. Нельзя задним числом переписать claim уже выданного опыта.

Старые common-рецепты, методы подготовки и суммарные1000/400+arm budgets ниже теперь описывают **только однократного producer плюс consumer**, а не расход каждого consumer. Старый child `--prepare-learned`/prepare не вызывать из consumer wrapper. Все tools и ожидание — по31; никаких exporters/manifest/scripts «пока ждём». Артефакты и scripts здесь не созданы и не проверены запуском.
**Сохранение оплаченного state:** сначала независимо записать назначенные weights, optimizer, RNG и имеющуюся qualification; затем typed validation/readback; COMPLETE последним по33. Ошибка проверки weights не должна пропускать запись остальных уже доступных состояний. Новых Train/Forward и периодических снимков не добавлять; partial сохранять.

**Точный weights API для этой карточки:** `cfg` — фиксированный ModelConfig этой карточки; LoRA rank4, targets строго `q_proj,v_proj`. Producer: `model.SaveCheckpointToStream(stream)` на новом `FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None)`, изначально Length=0/Position=0; `Flush(flushToDisk:true)`. Затем независимо записать остальные state/receipt по33 и только после этого открыть сохранённый weights для чтения. Установить Position=0; `var checkedArtifact = LlmModel.ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(4,new[]{"q_proj","v_proj"}))`; требовать `checkedArtifact.IsSuccess && checkedArtifact.Value==true`. Если в конкретном рецепте уже назначен maximumArtifactBytes, передать его неизменным (CS01:8388608). Перегрузка без topology недопустима для этого LoRA snapshot. Перед native `freshModel.LoadCheckpoint(stream)` снова Position=0 либо открыть новый read stream; construction/ApplySeeded только до load. Проверить назначенные named keys/shapes/value bits. Равный размер файла, повторное чтение самого файла или снижение loss не доказывают равенство потерянной live memory. В consumed/recovery случае такое равенство помечать UNKNOWN, не придумывать. Эта запись уточняет уже назначенные операции, не добавляет model calls.

<!-- END shared-start-binding-2026-10-21 -->


**Внутренняя редакция21.10, incoming8346dfb697:** после [SFT-EXP-REUSE-01](SFT-EXP-REUSE-01.md); next_id — [HNSW-QUERY-NORM-REUSE-01](HNSW-QUERY-NORM-REUSE-01.md). Остальные переходы и позиции — [текущий индекс](00-execution-index.md#next-contract). Активный вход [PARENT-ADOPTION-01](PARENT-ADOPTION-01.md) сохраняется. Source pin ниже обязателен; локальные пути — места чтения, а не разрешение незаметно взять изменённый HEAD. Один правильный TRAIN ответ означает ограниченный приобретённый ответ; 1/2 равно constant baseline и не доказывает правило знака. Гипотеза касается стоимости настоящего Train из этого ограниченного состояния. Цель общего навыка остаётся OPEN.

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


## Статус и место в очереди

**AUTHOR_SETUP_REQUIRED · новая гипотеза · программирование · измерений A/B ещё нет.** Этот документ добавляет кандидата в исправления. Он не разрешает менять production, запускать обучение, повторять прошлые эксперименты или исправлять их тесты. Текущая работа — документация. Изменения этого поручения разрешены только в C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs; старые результаты и каталоги опытов сохраняются.

В реестре эта строка должна иметь priority_group=P2_NATIVE_TRAIN_COST, run_ready=false. Сначала идут действующие блокеры обучения и конфликты карточек. Активный вход [PARENT-ADOPTION-01](PARENT-ADOPTION-01.md) сохраняется: новая кандидатура его не подменяет. Эта карточка непосредственно связана с native training и поэтому располагается перед проверками, вообще не участвующими в обучении.

Модель исполнителя — default/inherited текущей сессии, без смены модели. Для данного численного SFT-эксперимента новые DPO не нужны: teacher calls=0, quality-model calls=0, generated DPO pairs=0. Отчёт и объяснения — по-русски; идентификаторы, API и имена файлов сохраняются.

**Обязательный запрет поведения:** при STOP/«закончить немедленно» немедленно прекратить действия. При NEXT/«следующий эксперимент» немедленно оставить прежнюю задачу и перейти только к указанному следующему эксперименту в разрешённой фазе. Нельзя продолжать прежнюю задачу ради дополнительного отчёта, проверки, хеша, коммита, исправления тестов, warm-up, повторного запуска или «завершения полного цикла». Доступный статус сообщить без новых действий над прежней задачей. Нельзя выдавать неначавшееся обучение за результат; build failure и оставшиеся файлы описываются как таковые.

## 1. Что предлагается проверить

Гипотеза: сохранение уже вычисленного знаменателя SiLU между forward и backward может уменьшить CPU-время всего native SFT update при сохранении тех же численных обновлений LoRA. Цена — дополнительный массив активаций и его время жизни в графе. Ускорение, снижение затрат или улучшение точности пока не измерены.

В текущем [TensorOps.Silu, строки 1129–1162][silu] выполняются две одинаковые экспоненты для каждого элемента, который участвует в backward:

    forward:  data[i] = x.Data[i] / (1f + MathF.Exp(-x.Data[i]))
    backward: sigmoid = 1f / (1f + MathF.Exp(-x.Data[i]))

Вариант B сохраняет именно знаменатель d = 1f + MathF.Exp(-x.Data[i]) во время forward и читает его в backward. Это конкретная гипотеза о затратах обучения, а не доказательство, что историческое снижение качества вызвано SiLU.

### Участие в обучении и место возможного будущего исправления

| Звено | Проверенный путь |
|---|---|
| Настоящий update | [Trainer.TrainSupervisedNextTokenStep][trainer]: model.Forward(training:true) → CE → BackwardLoss → ClipGradNorm → AdamW.Step → ClearGradients |
| Вход в Transformer | [LlmModel.Forward, строки 219–240][model]: embedding → все blocks → final norm → LM head |
| Путь к FFN | [TransformerBlock.Forward, строки 45–65][block]: attention с LoRA → residual → norm2 → FFN |
| Нужный вычислительный узел | [SwiGLUFeedForward.Forward, строки 40–51][ffn]: gate projection → TensorOps.Silu → multiply с up projection → down projection |
| Обучаемые параметры | [LoraLinear.Forward, строки 113–131][lora]: q/v LoRA участвует в forward; градиент через замороженный FFN нужен для обучения этих адаптеров |
| Файл возможного production-кандидата после решения пользователя | 00.TensorCore/Core/TensorOps.cs, метод Silu(Tensor, CancellationToken), только сохранение знаменателя для backward |

Основное плечо использует native LlmModel и четыре обучаемых массива q/v LoRA. Это не обучение полного RP01 Unit/Flow и не проверка интеграции Unit lifecycle. Общая достижимость SiLU через DPO forward не заменяет отдельного DPO-эксперимента.

### Новизна

Следующее описание поиска — сохранённое свидетельство входящего [аудита8346](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-causal-audit-fb9175cc/07-neural-source-audit.md>), а не новый просмотр929документов при внутреннем применении21.10. Здесь заново сверены36manifest entries по pinned Git tree и прочитан нужный native SiLU/caller/исторический recipe; весь corpus повторно содержательно не аттестован.

Выполнен полный адресный поиск по 929 Markdown в possibleDifs на planning SHA fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c и по 372 Markdown защищённого core-корпуса на core SHA fb9175cc752d41e1cb525e0609b7c96140940217. Проверялись Silu, SwiGLU, сочетания sigmoid с cache/reuse, русские сочетания повторных экспонент и кеширования активаций. Точные соседние материалы прочитаны. Сплошное содержательное чтение каждого документа не заявляется.

| Существующий материал | Почему это другой фактор |
|---|---|
| SFT-EXP-REUSE-01 | Повторные экспоненты softmax в Trainer.BackwardLoss; здесь SiLU между forward и backward |
| NEURAL-FROZEN-TRANSPOSE-01 | Повторные транспозиции замороженных Linear; здесь дополнительный массив знаменателя активации |
| RMS-COORD-ALLOC-01 | Координаты broadcast RMSNorm, другой узел и другой расход |
| Материалы об исправленном SiLU autograd | Подтверждают наличие backward; не предлагают повторно использовать знаменатель |

Предшествующая карточка именно этого фактора не найдена. Счётчик совпадений не является доказательством универсальной новизны вне проверенного корпуса.

## 2. Домены и минимальные данные

Основной домен — **программирование**, очень узкая задача: выдать один знак результата простого C#-совместимого арифметического выражения. Доказательство не переносится автоматически на длинную генерацию кода, перевод, документацию или orchestration. Для других доменов понадобится отдельное ненулевое обученное S0 и тот же native caller; SiLU, LoRA и KNN сами по себе доменами не называются.

Набор фиксирован и совпадает с прочитанным [LEARN-SANITY-PROBE/Program.cs][sanity-source].

| ID | Роль | Prompt — точные символы, без пробела после = | Единственный токен ответа |
|---|---|---|---|
| T+ | TRAIN | C# result sign: 9-4 = | + |
| T− | TRAIN | C# result sign: 4-9 = | - |
| E+ | EXAM | C# result sign: 8-3 = | + |
| E− | EXAM | C# result sign: 3-8 = | - |

Кодирование — UTF-8 bytes, int ID каждого байта, словарь 256, без BPE merges, без BOS/EOS и без дополнительного разделителя. Каждый prompt имеет 21 байт, prompt+answer — 22; один SFT update обучается на всех 21 shifted next-token targets, включая один токен ответа. IDs ответов: +=43, -=45. Нельзя сообщать, что loss маскирован только на ответ: исходный native Trainer учится на всей последовательности.

EXAM не входит в обучение, checkpoint selection, допуск A/B, LR tuning или подбор seed. Положительный EXAM readout — лишь две близкие строки. Статистическое обобщение на программирование не доказано.

## 3. Откуда взять обученное S0

**Готового пригодного H64 checkpoint с весами и optimizer state в проверенном LEARN-SANITY-PROBE не найдено.** В его дереве есть исходник, проект, журналы, JSON и отчёт. Program сохраняет JSON readouts; он не сохраняет checkpoint. Его положительный результат даёт рецепт подготовки, а не готовые восстановимые веса.

Проверен существующий [raw probe-result.json][sanity-raw]:

| Измерение в исторической пробе | W0, 0 updates | W400, 400 updates |
|---|---:|---:|
| TRAIN, правильных однотокенных ответов | 0/2 | 1/2 |
| TRAIN, средний NLL правильного токена | 7.960885496452843 | 1.8879445998230966 |
| TRAIN, средний gold rank | 206.5 | 1.5 |
| EXAM, правильных однотокенных ответов | 0/2 | 1/2 |
| EXAM, средний NLL правильного токена | 8.01533412366764 | 2.7988595276200154 |
| EXAM, средний gold rank | 215 | 2.5 |

Это положительные измерения полезных показателей именно обучения; они не требуют роста до 2/2. Результат 1/2 на каждом сбалансированном наборе равен constant-label baseline, поэтому не доказывает усвоения условного правила знака. При этом модель не освоила обе строки: после 400 updates TRAIN отвечает + на оба примера, EXAM отвечает - на оба. До 100 updates включительно полных правильных ответов на обоих наборах не было. Поэтому бюджет подготовки нельзя произвольно ужать до четырёх updates.

Запись RESTORE-MINIMAL-SET использует другую H16 модель и имеет 0/2 полных TRAIN ответов после восстановления; её нельзя подставлять как готовый донор этого H64 опыта. Наличие LFS pointer не доказывает наличие скачанного payload.

### Рецепт отдельного producer; consumer только восстанавливает

Отдельно назначенный producer h64-silu использует A-код SILU01 и сохраняет S0 в shared root выше; consumer SILU01 не вызывает этот bounded recipe. Старый LEARN-SANITY-PROBE не запускать заново и не редактировать.

| Настройка | Значение |
|---|---|
| LlmModel | VocabSize=256; ContextLen=64; HiddenDim=64; NumLayers=1; NumHeads=4; NumKvHeads=2; FfnHiddenDim=176 |
| Численные параметры | RopeTheta=10000f; RmsNormEps=1e-5f |
| База | Initializers.UseDeterministicSeed(62001UL) |
| LoRA | TargetModules=["q_proj","v_proj"]; Rank=4; Alpha=8f; Dropout=0f |
| LoRA seed и stream | ApplySeeded(...,62002UL,"frozen-transpose-probe-v1") |
| Trainable/frozen | Ровно 4 trainable parameter arrays и 12 frozen arrays, включая frozen lm_head |
| Trainer | BatchSize=1; SeqLen=64; WarmupSteps=0; TotalSteps=400; MaxLr=MinLr=0.001f; GradClipNorm=1f; GradAccumSteps=1; WeightDecay=0 |
| AdamW | beta1=0.9f; beta2=0.95f; eps=1e-8f; weightDecay=0 |
| Порядок | T+, T−, T+, T−, …, ровно 400 calls TrainSupervisedNextTokenStep, weight=1f |
| Прочие факторы | Linear.ExperimentReuseFrozenTranspose=false; SiLU reuse=false; нет иных оптимизаций из соседних карточек |

W0 и W400 измеряются на четырёх строках. S0 принимается для данного A/B только если минимум одна и та же TRAIN строка перешла W0exact=false→W400exact=true, W400 имеет минимум один правильный TRAIN ответ, изменившиеся trainable веса, конечные losses и optimizer step=400. Уже случайно правильная W0 строка сама по себе не означает приобретения. Это использует те же назначенные readouts без новых calls. EXAM полностью показывается при любом результате и не участвует в допуске S0/A/B; один фиксированный W400 не выбирается по EXAM. Это проверка пригодности уже фиксированного рецепта по TRAIN и состоянию: рассматривается только W400, EXAM остаётся неотфильтрованным наблюдением. Если критерий не выполнен, статус BASELINE_NOT_LEARNED_AT_FIXED_BOUND; сохранить результат и прекратить. Не увеличивать updates, learning rate, размер модели или число seeds. Не запускать A/B с 0/0 под видом обучения.

Producer независимо сохраняет веса S0 через LlmModel.SaveCheckpointToStream/CreateNew и portable optimizer через ExportPortableState(...).ToBytes()/CreateNew, RNG и имеющуюся qualification. Только после записи доступных state — точный typed weights API rank4/q_proj,v_proj из binding выше и порядок33. Не использовать overwriting SaveCheckpoint. Для bindings использовать GetCheckpointParameterEntries().Select(e => new AdamWParameterBinding(e.Key,e.Parameter)); не сопоставлять массивы по случайному порядку. API импорта/экспорта определены в [AdamW][adamw] и уже используются в [RESTORE-MINIMAL-SET/Program.cs][restore-source].

Для каждого плеча создать новый LlmModel той же конфигурации; до LoadCheckpoint применить LoraApplicator.ApplySeeded с теми же q/v targets, rank4/alpha8/dropout0, seed62002UL и stream "frozen-transpose-probe-v1". Затем выполнить LlmModel.LoadCheckpoint из S0 и проверить состав:4 trainable массива,12 frozen, те же named parameter keys/hashes. LoadCheckpoint читает существующую topology и сам не добавляет LoRA. После этого создать новый AdamW, построить bindings по GetCheckpointParameterEntries и выполнить TryImportPortableState из одних сохранённых optimizer bytes. Оба плеча получают те же веса и моменты; initial seed не заменяет сравнение загруженного S0. TryImportPortableState должен вернуть true; точные hashes всех named parameters и portable optimizer bytes до A/B должны совпасть с S0. Для каждого плеча ожидается optimizer step 400→404. Новый Trainer допустим при явно постоянном MaxLr=MinLr: его локальный scheduler counter не создаёт различие LR. Dropout=0; дополнительные случайные преобразования запрещены.

## 4. Полная спецификация нового host и замыкания исходников

**SILU01, Runner.csproj, Program.cs и Run.ps1 сейчас не существуют как проверенный runnable experiment.** Ниже — точное задание автору host. Runtime-исполнитель не должен искать альтернативный проект, угадывать флаги или запускать вымышленный CLI. Пока автор не добавил файлы, manifest, точную команду и статическую проверку dependency closure, статус остаётся AUTHOR_SETUP_REQUIRED.

Место автора заранее определено: C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01. В нём должны находиться src/, Program.cs, Runner.csproj, Run.ps1 и короткие каталоги obj/, bin/, s0/, results/. Не воспроизводить вложение experiments-v2/RP01-MICRO дважды. Перед созданием вычислить длины всех путей; предел для каждого планируемого пути — 220 символов. Если корень отличается, требуется авторское обновление карточки с точным абсолютным корнем до runtime; исполнитель не ищет его сам.

Проверенное исходное замыкание — 36 .cs из:

    experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/src

в core commit fb9175cc752d41e1cb525e0609b7c96140940217. Git tree SHA src: e448640bd1f2fe2c65adcc0b5ffcb02e7f5539e6. [Существующий Runner.csproj][lft-project] показывает компиляцию этих исходников с EnableDefaultCompileItems=false и target net9.0. Автор копирует **весь** src и сохраняет относительные пути, а не один TensorOps.cs. Новый Runner.csproj компилирует только src/**/*.cs и новый Program.cs; OutputType=Exe, AssemblyName=SILU01, TargetFramework=net9.0, EnableDefaultCompileItems=false; без ProjectReference на runtime, тесты, solution или dispatcher; ImportDirectoryBuildProps=false, ImportDirectoryBuildTargets=false, локальные bin/obj.

TensorOps.cs этого замыкания идентичен текущему core target по Git blob e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b. Проверенные Tensor/Transformer/Trainer/AdamW/LoRA файлы совпадают с текущими tree entries, кроме Linear.cs: в копии есть экспериментальный frozen-transpose switch, который здесь всегда false. Это ограничение происхождения фиксируется в manifest, а не скрывается как полное равенство всего репозитория.

### Manifest обязательных исходников

Все следующие записи находятся относительно src/ указанного pinned subtree. Git blob SHA проверяется до авторского изменения единственного численного фактора; отдельный B diff сохраняется рядом.

| Source относительно src/ | Git blob SHA |
|---|---|
| 00.Core/OperationResult.cs | 2bfde76ca7d99f63eb657e736330da177838c644 |
| 00.Core/SnapshotExceptionGraphClassifier.cs | af630e9b64fa2c438f4321d3ba9216841d76d152 |
| 00.TensorCore/Core/SimdOps.cs | cf2a93a4486e141a595b658aa8346133721c842f |
| 00.TensorCore/Core/Tensor.cs | a2569eefe378fb0a841cbb2137c6a8ac001bdb3a |
| 00.TensorCore/Core/TensorOps.cs | e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b |
| 00.TensorCore/Core/TensorShape.cs | 0879e0702f210e2dd45358af09bddff2a4ef9874 |
| 00.TensorCore/Core/TensorStorage.cs | 22aec3ba03b756530001ae786acbdce28af90104 |
| 00.TensorCore/Init/Initializers.cs | 30ff381b091c3ee7aaf56e9cdbab98be2652db1e |
| 00.TensorCore/Layers/Dropout.cs | 02cf86dc84854b163cb536cab5be40dd1b7f9d63 |
| 00.TensorCore/Layers/Embedding.cs | 26fc1a0b46fcbc6df305421bfc48c15cc70d11a4 |
| 00.TensorCore/Layers/Linear.cs | d8120e0bdcfc2673aa17862e5225d77420827db6 |
| 00.TensorCore/Layers/Module.cs | dc396e8fa5a23ce6d487ff30e1326cddc7df8efd |
| 00.TensorCore/Layers/Parameter.cs | 82f5ea5aebc01b70f9c3f651283a1481c1df9a26 |
| 00.TensorCore/Randomness/SplitMix64Random.cs | 11c6d483ed94829ece961af2d77f2cff91ce411a |
| 02.DatasetPipeline/DataLoader.cs | 9fb803ef90adbfc3a4ee61eb942d6ab671047996 |
| 02.DatasetPipeline/NpyReader.cs | dda1b8922699025413eb495cd1f0fc0aa9625c18 |
| 03.TransformerModel/Caching/ITransformerKvCache.cs | 4d12f26e03f50b519bff987b455015d4cc2fefdd |
| 03.TransformerModel/Config/ModelConfig.cs | 818877df9a8dda4b40dd67bb441489b8512200b1 |
| 03.TransformerModel/FullCheckpointLoraTopology.cs | 25f24c1a260b1421ec5ded03e6dc3a0a121ff984 |
| 03.TransformerModel/Layers/CausalSelfAttention.cs | 8106a98c5f2dba90a975abe2056e21b5550166c6 |
| 03.TransformerModel/Layers/RmsNorm.cs | 246bbc4781d4c06d3b71f617d51aec8860ac39f2 |
| 03.TransformerModel/Layers/RotaryEmbedding.cs | 48597599b46f884e74bf911c66189f77eb649e37 |
| 03.TransformerModel/Layers/SwiGLUFeedForward.cs | 5daa1c56bb54cac66bd95b8658df9cc0eb2ee56e |
| 03.TransformerModel/LlmModel.cs | bef968513391f3941e40f20393a4e5af30567ff8 |
| 03.TransformerModel/Randomness/LlmModelRandomState.cs | 96c79e56e176234e3cd8e777b2ae55fc70ae9572 |
| 03.TransformerModel/TransformerBlock.cs | 28eeaba3abf22ccca4bd748bf8c81470c327520c |
| 04.TrainingLoop/Optimizers/AdamW.cs | 05877706c68c8a76b63fb62842d91a2596070faf |
| 04.TrainingLoop/Optimizers/AdamWPortableState.cs | fef6af5366e412e2508a716118bd3f76759abc73 |
| 04.TrainingLoop/Optimizers/AdamWStateSnapshot.cs | 009983981b3ff213ff68d28c955027649ba83684 |
| 04.TrainingLoop/Schedulers/CosineWithWarmup.cs | 6020cbaa994468cbd09c12d5a9cce82a993051a1 |
| 04.TrainingLoop/Trainer.cs | b63d550ba43e24a2b51be2c836519aab537d1312 |
| 04.TrainingLoop/TrainingConfig.cs | 31dd056a15888e0f0a287bf9af56297b647c8b7d |
| 10.LoRA/LoraAdapterRandomSeeds.cs | 26c7a56ba5dda49671c84e1deb11571b02e73a78 |
| 10.LoRA/LoraApplicator.cs | 5eeaf38c5fc12fa2f88dc6c247a3fb2fc250716d |
| 10.LoRA/LoraConfig.cs | a0b81255846f37b5e9449d6ccc883d924c3ed5f5 |
| 10.LoRA/LoraLinear.cs | 1b321e99408c961b896896983d002e3b26425b85 |

### Контракт Program и запуска после авторской готовности

Consumer Program выполняет ровно последовательность shared COMPLETE/qualified S0 → restore A → A0 → 4 updates A → A1 → restore B → B0 → 4 updates B → B1 → report. A и B получают один S0 и один порядок T+,T−,T+,T−. Состояние A не становится началом B.

В экспериментальной копии TensorOps используется один явный switch для варианта; все общие измерители одинаковы. На подготовке, A и readout forwards switch=false; только во время четырёх B updates он true. Это область эксперимента о training cost. Продуктовая политика кеширования при inference здесь не проверяется.

Уточнение14308: `RequiresGrad` не означает `training:true`. В [LoraLinear.Forward](C:/Projects/llm_mvp_core/10.LoRA/LoraLinear.cs:107) флаг training управляет dropout, но LoRA MatMul/Transpose остаются в графе при scoring. Поэтому нельзя заменить назначенный switch одной проверкой RequiresGrad и обещать отсутствие расходов inference. Деноминатор выделяется только при `ExperimentReuseSiluDenominator && x.RequiresGrad`; switch обязательно возвращается в false перед каждым readout. Безусловный production cache и его расходы не входят в этот опыт. Название SILU-DENOM-REUSE-01 из нового planning-пакета — alias этого ID; отдельные SD01, бюджет и запуск запрещены.

Run.ps1 — заранее назначенная единственная точка запуска будущего host, без поискового dispatcher. Автор обязан вписать в карточку **проверенную команду этого созданного файла**, SDK версию, точные out/obj пути и один run ID перед переводом в RUN_READY. Не добавлять в карточку будто существующие флаги native runtime. Run.ps1 должен однократно собирать только этот проект, создавать результаты без overwrite и выполнять этот fixed plan; до ready никаких runtime команд не разрешено.

Контракт создания Run.ps1 уже назначен: без входных параметров, repo root строго C:/Projects/llm_mvp_core, единственный run ID SILU01-R1, results/SILU01-R1 и s0/SILU01-R1. Автор реализует именно этот интерфейс. После создания и готовности **будущая** команда должна иметь следующий вид; сейчас это спецификация отсутствующего host, запуск запрещён:

    pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/Run.ps1

Внутри будущего скрипта назначены ровно две последовательные команды из H: dotnet build H/Runner.csproj -c Release --nologo; затем dotnet H/bin/Release/net9.0/SILU01.dll без аргументов. H — полный корень SILU01 выше, рабочий каталог H. В Runner.csproj до Import Sdk.props задать ImportDirectoryBuildProps=false, ImportDirectoryBuildTargets=false, BaseOutputPath=H/bin/, BaseIntermediateOutputPath=H/obj/, MSBuildProjectExtensionsPath=H/obj/; после него OutputType=Exe, AssemblyName=SILU01, TargetFramework=net9.0, ImplicitUsings=enable, Nullable=enable, EnableDefaultCompileItems=false, Compile только src/**/*.cs и Program.cs. Никаких ProjectReference, production solution, другого output-root или dispatcher. В разрешённой будущей фазе записать фактический dotnet --version в H/run-contract.md; не угадывать SDK и не менять target ради ошибки. Точный установленный SDK автор фиксирует в run-contract.md до RUN_READY; net9.0 обозначает target framework, а не проверенную версию SDK. Runtime executor не выбирает flags, paths, fixture, модель или runner. Если автор не смог создать именно этот контракт, карточка остаётся AUTHOR_SETUP_REQUIRED с одной конкретной причиной; альтернативного запуска нет.

Если out уже существует, исходники не совпали, compile завершился ошибкой, portable restore не прошёл или обнаружен неизвестный runtime конфликт — остановить этот опыт с конкретным статусом. Существующие файлы не удалять, run ID не менять автоматически. Допустимые экспериментальные исключения — короткий host, source copy, common instrumentation и локальный switch; они не становятся разрешением менять production или отправлять код.

Ранняя защита бюджета назначена точно: **до build, запуска SILU01.dll и любого model load/W0 forward/400-step preparation**, Run.ps1 атомарно создаёт один файл C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/execution-SILU01-R1.claim через FileMode.CreateNew. До этого проверяет, что S и R указанного run не содержат старых результатов. Claim содержит run ID, pin, пути, случайный owner nonce, PID launcher и его start time. Скрипт передаёт nonce только своему дочернему Program через SILU01_OWNER_NONCE. Program проверяет nonce и launcher identity, затем до создания/загрузки модели атомарно создаёт H/study-SILU01-R1.claim через FileMode.CreateNew, записывая свой PID/start time. Это одноразовый native-entry guard: прямой второй запуск Program не использует прежний execution claim как пропуск. Program не пересоздаёт execution claim. Launcher не создаёт leaf S/R до Program; Program создаёт их только после успешного study claim, поэтому собственные каталоги запуска не конфликтуют с ранней проверкой. Existing S/R даже без claim, любой execution/study claim или partial → EXISTS_OR_UNKNOWN до модели; существующие данные сохранять. Claim и partial остаются при ошибке/STOP/timeout; следующий запуск с этим claim завершается EXISTS_OR_UNKNOWN до модели/обучения, без удаления и скрытого run02. Нельзя заменить ранний claim поздним CreateNew(report/raw) после 408 updates. После build failure execution claim сохраняется; эта попытка не возобновляется автоматически. Build не выдаётся за обучение. Все stdout/stderr/progress и exit относятся к сохранённому owner/handle; читать до terminal по контракту31.

При отдельном поручении только SILU-SIGMOID-REUSE-01 его NEXT=END. В явно разрешённой программе next_id — [HNSW-QUERY-NORM-REUSE-01](HNSW-QUERY-NORM-REUSE-01.md); A4/A5 остаются историческими source labels; разрешение фазы не расширяется автоматически.

## 5. Единственный фактор A/B

A сохраняет исходную арифметику SiLU.

B только в ветке x.RequiresGrad:

1. Создать float[] denominator длиной x.Shape.TotalElements, принадлежащий этому forward.
2. В forward вычислить float d = 1f + MathF.Exp(-x.Data[i]); затем data[i] = x.Data[i] / d; затем denominator[i] = d.
3. В backward вычислить sigmoid = 1f / denominator[i].
4. Оставить формулу grad, порядок умножений, Children, AccumulateGrad, CancellationToken checks и формы неизменными.
5. Когда x.RequiresGrad=false, не выделять denominator и сохранять исходный forward.

Нельзя заменить x/d на x*(1/d), восстанавливать sigmoid через result/x, кешировать между шагами или моделями, менять точность, менять входные x после forward, вводить approximate Exp, mask, новый optimizer, LR, LoRA target или tokenizer. Нельзя одновременно включать SFT-EXP-REUSE или frozen-transpose cache.

Проверка float parity проводится на фактически обученных A/B: loss каждого из четырёх updates, named weights после updates, portable optimizer bytes и все сохранённые before/after logits. Эти сравнения не требуют новых forward или optimizer calls. Если обнаружены отличия, report содержит max abs/relative error и количество разных float bit patterns; численный характер варианта становится отдельным риском, а «тот же update за меньшее время» не утверждается.

## 6. Измерения до и после

Основное измерение — **суммарное elapsed_ms четырёх полных вызовов native TrainSupervisedNextTokenStep** каждого плеча. Внутри интервала остаются forward, loss, backward, clipping, optimizer и очистка gradients. Вычислять длительность только MathF.Exp или Silu и выдавать её за выигрыш обучения запрещено.

| Группа | Что сохранять и как сравнивать |
|---|---|
| Обучение | 4 returned loss каждого плеча; model/optimizer hashes и steps; изменение LoRA, неизменность frozen weights |
| Ответы | На T+/T−/E+/E− отдельно A0,A1,B0,B1: predicted token, exact, gold token NLL, gold rank; raw 256 final-position logits |
| Скорость | total native Train elapsed_ms; process CPU delta; per-update elapsed; targets/sec при фактическом знаменателе 84 targets/плечо |
| Память | GC.GetTotalAllocatedBytes(precise:true) delta для native Train; managed heap до/после; общий peak working set с честной областью измерения |
| Цена кеша | Число реально созданных denominator arrays; их payload bytes в Train; максимум payload одного forward-графа |
| Достижимость | Число native updates, backward calls и SiLU graph elements; instrumentation читает тот же путь, отдельный helper-probe запрещён |
| Учёт затрат | Создание модели, 400 preparation updates, checkpoint/restore, scoring и report отдельно от сравниваемого native Train |

При 21 позиции, одном слое и FF176 ожидаемый payload одного denominator array равен 21×176×4=14784 байта; четыре B updates дают 59136 байт payload, если узел достигнут один раз на update. Это расчёт ожидаемого дополнительного массива, **не измеренное сокращение памяти**. Заголовки массива, GC и время жизни графа тоже имеют цену. Перестать использовать граф после backward не означает, что GC уже освободил массив.

Process.PeakWorkingSet64 накопителен: после A→B его нельзя выдавать за два независимых пика плеч. Показать общий процессный peak и честные snapshots. При фиксированном A→B и одном коротком замере timing имеет порядок и шум; не объявлять причинное ускорение на всех доменах. Дополнительные повторы для получения удобного знака результата запрещены.

Readout точно повторяет смысл существующего Observe: model.Forward(prompt,training:false), logits последней позиции prompt, argmax по 256 IDs с первым максимальным ID, NLL=logsumexp(logits)-gold_logit, rank=1+число logits строго больше gold. Однотокенный ответ полностью измеряется этой генерацией; это не teacher-forced многотокенный exact. Нельзя считать совпадение logits между A и B доказательством ненулевого обучения: отдельно нужны пригодное S0, actual updates и ответы.

**Правило успеха:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и данному домену; рост точности для этого не обязателен. Рядом обязательно показать все остальные изменения, включая ухудшения. Например, native training elapsed снизился, accuracy прежняя, allocations выросли — это успех по elapsed с ценой памяти, а не общий безусловный успех. Число изменённых строк исходника и арифметически предсказанное уменьшение Exp сами по себе не доказывают выигрыш скорости или качества.

<!-- BEGIN book-math-pass9-2026-10-23:silu-reuse -->
<a id="book-math-pass9-silu-reuse"></a>
## Математическое объяснение результата: сохранить уже вычисленный знаменатель

**Дополнение 23.10.2026.** Здесь разобран опубликованный `SILU01-R1`; исходная постановка карточки сохранена. Источники: [raw с затратами и проверкой равенства](https://github.com/lubochka/llm_mvp_core/blob/ff6ed114e49f275da50891b8dd13e9d9288a8692/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/raw.json), [отчёт](https://github.com/lubochka/llm_mvp_core/blob/ff6ed114e49f275da50891b8dd13e9d9288a8692/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/report.md), [точное изменение кода](https://github.com/lubochka/llm_mvp_core/blob/ff6ed114e49f275da50891b8dd13e9d9288a8692/experiments-v2/RP01-MICRO/SILU01/factor.patch).

**Пример для студента.** При решении задачи вы уже посчитали сложное промежуточное число и через минуту используете его в проверке. Запись этого числа позволяет не считать его повторно. В SiLU таким числом стал знаменатель $d=1+e^{-x}$: он нужен и для значения функции, и для производной, по которой исправляют веса.

**Что взято из книги.** [§9.6, печатные с. 284–285 / PDF с. 320–321](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#display-9-feedforward-pullback) показывает перенос градиента через FFN по цепному правилу: входящий сигнал последовательно умножается на производные слоёв. В книге выписан случай ReLU с диагональной маской $D_m$; [Proposition 9.6](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#proposition-9-6) также относится к ReLU. **Следующий вывод для SiLU — наш**, с той же логикой цепного правила:

$$
s=\sigma(x)=\frac1{1+e^{-x}},\qquad f(x)=xs,\qquad s'=s(1-s),
$$

$$
f'(x)=s+x\,s(1-s)=s\,[1+x(1-s)],\qquad
\frac{\partial L}{\partial x}=\frac{\partial L}{\partial f}\,f'(x).
$$

**Что сохраняет настоящий код.** A вычисляет $x/(1+e^{-x})$ вперёд и повторяет экспоненту для $s$ назад. B сохраняет **знаменатель $d$**, вычисляет вперёд то же $x/d$, а назад получает $s=1/d$. Дальнейший порядок умножений в производной остаётся прежним. Массив принадлежит конкретному узлу прямого прохода и создаётся только при включённом факторе и `RequiresGrad`; он используется для того же входа в соответствующем обратном проходе. Это не кеш значений между разными обновлениями. Фактор выключен при подготовке, в A и во всех диагностических чтениях.

Равенство производных над вещественными числами само по себе не доказывает побитовое равенство в `float`. Здесь сохранены dtype и порядок арифметики, а границу фактического равенства показывают raw этого опыта: совпали четыре loss, 896 обучаемых значений весов, все 7 384 байта переносимого состояния оптимизатора, 1 024 логита до и 1 024 после.

**Измеренный выигрыш и его цена:**

| Показатель четырёх обновлений | A | B |
|---|---:|---:|
| Повторные экспоненты в обратном SiLU | 14 784 | 0 |
| Дополнительные массивы знаменателя | 0 | 4 |
| Полезные данные этих массивов | 0 B | 59 136 B |
| Все выделенные байты в учебном окне | 13 639 760 B | 13 698 992 B |
| Время четырёх обновлений | 22.1668 ms | 19.9589 ms |
| Шаг оптимизатора до → после | 400 → 404 | 400 → 404 |

Количество устранённых экспонент следует из 14 784 элементов четырёх SiLU-узлов и изменения обратной формулы. Дополнительные выделения составили 59 232 B: 59 136 B данных и 96 B накладных расходов четырёх массивов. Это сумма выделений за окно, не измерение пикового потребления памяти. Счётчики диагностических чтений подтверждают отсутствие массивов знаменателя вне учебного окна.

**Почему время может улучшаться.** Повторный расчёт экспоненты заменён чтением сохранённого числа. Новый [§14.5, печатные с. 478–480 / PDF с. 514–516](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0512-0515-ch14-sa-channels-and-cost-accounting.md#section-14-5) помогает разделить цену вычислений и хранения; конкретный обмен памяти на повторный расчёт здесь следует из кода. В одной короткой паре A затем B наблюдалось уменьшение времени на 2.2079 ms, или 9.96%, ценой дополнительных выделений. Порядок плеч фиксирован, повторных замеров нет, поэтому устойчивый процент ускорения не установлен.

Общие 400 подготовительных обновлений учтены отдельно. Учебная панель сохранила 2/4 правильных ответов до/после в обоих плечах. Успех этого фактора — меньше повторных вычислений при том же проверенном обновлении; освоение правила на меньшем числе примеров или рост качества этим опытом не показаны.
<!-- END book-math-pass9-2026-10-23:silu-reuse -->

## 7. Полный бюджет и терминальные состояния

| Фаза | Native updates | Полные encoded sequence bytes | Shifted SFT targets | Дополнительные scoring forwards |
|---|---:|---:|---:|---:|
| Только producer: W0 и W400, common preparation | 400 | 8800 | 8400 | 8 |
| A0 → A1 | 4 | 88 | 84 | 8 |
| B0 → B1 | 4 | 88 | 84 | 8 |
| Producer+consumer | 408 | 8976 | 8568 | 24 |
| Только consumer | 8 | 176 | 168 | 16 |

Каждый scoring forward имеет 21 prompt position. 408 updates и24 readout относятся к однократному producer плюс одному consumer. Каждый consumer:8 updates/8 Backward/24 Forward (8training+16readout), common0; дополнительных warm-up, pilot, LR/seed grid, «почти готовой модели» другого размера и backfill-обучения нет. Стоимость отдельного producer сохраняется по ссылке; её не исполняют заново при restore. 400 updates — фиксированная верхняя и целевая граница подготовки, не обучение до достижения произвольного критерия.

Автор задаёт safety wall cap 300 секунд отдельно на native phase producer и на consumer A/B; consumer не тратит этот cap на preparation; это аварийная граница, не обоснование урезать минимальный set. При достижении cap статус BUDGET_EXHAUSTED с фактически выполненными calls и metrics; автоматически не расширять budget. Compile ограничивается отдельно 120 секундами. В текущей фазе документации исполнять ни один из этих этапов нельзя.

В R/report.md и R/status.json отдельно записать execution_status (ход/exit), learning_validity (пригодность S0 и сравнения), hypothesis_status (OPEN при invalid/incomplete); unknown counts=null. Терминальные статусы: AUTHOR_SETUP_REQUIRED; AUTHOR_READY/RUN_READY после полного host contract; BASELINE_NOT_LEARNED_AT_FIXED_BOUND; SOURCE_MISMATCH; BUILD_FAILED; RESTORE_FAILED; BUDGET_EXHAUSTED; STOPPED_BY_USER; MEASURED_SUCCESS_BY_METRIC; MEASURED_NO_IMPROVEMENT; NUMERICAL_DIVERGENCE_REQUIRES_USER_DECISION. Ни один из статусов не разрешает автоматический production fix. После одного разрешённого запуска — один report и кандидат пользователю; не проходить полную цепочку исправлений до решения пользователя.

## 8. Файлы отчёта и точный формат

Авторский набор нового host: source-manifest.tsv, factor.patch, run-contract.md. Единые абсолютные корни: S=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1; R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1. Во время будущего разрешённого запуска: чтение S/S0.weights.bin, S/S0.optimizer.bin, S/S0.rng.json и S/bundle.json; R/status.json, R/raw.json, R/per-row.csv, R/costs.csv и R/report.md; R/candidate.md только при полезном измеренном результате, с метрикой, регрессиями и границей вывода, без переноса в production. Здесь S и R — именно эти полностью назначенные пути, без выбора дополнительного run ID. Все новые, без overwrite старых. Документное согласование — только C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs. Consumer пишет только R; S публикуется отдельно producer по33 и читается без изменения. Planning source read-only.

report.md по-русски содержит ровно нужные разделы: «Статус и решение», «Гипотеза и домен», «Какое S0 действительно восстановлено», «Один фактор и реальный обучающий путь», «До/после по каждому набору и метрике», «Стоимость подготовки и native updates», «Улучшения рядом с ухудшениями», «Ограничения и кандидат исправления», «Фактические counts, pins и raw artifacts». Unknown значения писать NOT_MEASURED, не 0. При обычном полезном завершении добавить≤120слов в [required-changes.md](../../required-changes.md) с адресом исходного метода, before/after, регрессиями и raw/report; STOP/NEXT выше записи. Отдельно фиксировать stop/next, если они поступили.

### Встроенный промпт исполнителю

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/SILU-SIGMOID-REUSE-01.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательное исполнение ожидания и валидности: прочитай контракты [30](../30-failure-recovery-and-learning-validity.md) и [31](../31-tool-discovery-and-wait.md); найди фактические launch/wait/status по их текущей схеме. Выдай entry один раз, затем жди тот же handle до terminal exit/deadline; нормальный silent/running wait продолжать. Два быстрых пустых snapshot означают переход к wait. Неисправный wait проверять по своим H/R/claim/exit/PID+start; неизвестный исход не даёт права на relaunch/results-NN, урезание steps или перекладывание запуска на Любу. Раздели execution_status, learning_validity, hypothesis_status; 0→0 и неполный A/B не закрывают цель. STOP/NEXT немедленны.

> Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Полная карточка: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/SILU-SIGMOID-REUSE-01.md. Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. Ты работаешь с SILU-SIGMOID-REUSE-01 на default/inherited модели текущей сессии. Отвечай и документируй по-русски. Сейчас разрешена только документация; статус AUTHOR_SETUP_REQUIRED. Не запускай обучение, не меняй production или старые эксперименты; документные правки разрешены только в C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs. Активный вход [PARENT-ADOPTION-01](PARENT-ADOPTION-01.md) не заменять этой кандидатурой.
>
> Сначала используй перечисленные конкретные skills: C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md, C:/Projects/llm_mvp_core/docs/skills/phase-planning-SKILL.md, C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md, C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md, C:/Projects/llm_mvp_core/docs/skills/experiment-design-SKILL.md, C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md, C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md и C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md. Правила пользователя об экспериментальных исключениях, документации и STOP/NEXT имеют приоритет. Модель агента — default текущей сессии без override. Учебная H64 ниже — отдельная native Student; teacher/quality/DPO calls=0.
>
> Единственный новый фактор: в копии TensorOps.Silu сохранить forward denominator 1f+MathF.Exp(-x) для backward, сохранив исходный порядок x/d и производную. Native learner: Trainer.TrainSupervisedNextTokenStep → LlmModel → TransformerBlock → SwiGLU → SiLU → native backward/AdamW; никакой helper-only benchmark. Core pin fb9175cc752d41e1cb525e0609b7c96140940217. Source closure — все 36 файлов таблицы из LFT01/src, дерево e448640bd1f2fe2c65adcc0b5ffcb02e7f5539e6. Место нового host строго C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01.
>
> При последующем отдельном разрешении авторской реализации создай только этот host по полному контракту карточки. Нельзя объявлять Run.ps1 или какой-либо runtime CLI существующим, пока файл и exact command не добавлены. До готовности runtime ничего не ищет и не запускает. Не выбирай другой scaffold. Конфликт путей/версии/существующего output фиксируй; автоматического удаления, нового ID и retry нет.
>
> Единственный допустимый рецепт S0: H64/V256/L1/Q4/KV2/FF176, context64, q/v LoRA rank4 alpha8 dropout0, seeds62001/62002, stream frozen-transpose-probe-v1, LR0.001, AdamW0.9/0.95/eps1e-8, warmup0, clip1, weightDecay0. T+: "C# result sign: 9-4 ="→"+"; T−: "C# result sign: 4-9 ="→"-"; EXAM 8-3 и 3-8. UTF-8 byte IDs, без разделителя и EOS. Подготовить ровно 400 SFT updates в новом host, затем сохранить модель и моменты шага 400. Исторический raw даёт рецепт с 1/2 TRAIN и 1/2 EXAM, но готового H64 checkpoint не найдено. H16 restore с 0/2 не заменяет S0. Если после фиксированных 400 нет хотя бы одной TRAIN строки с переходом W0exact=false→W400exact=true — сохранить BASELINE_NOT_LEARNED_AT_FIXED_BOUND и не выполнять A/B. EXAM измерить и показать без отбора; он не gate допуска и не выбор checkpoint.
>
> Run.ps1 создаёт execution-SILU01-R1.claim через CreateNew до build/модели, с nonce/PID/start; Program сверяет владельца и одноразово создаёт H/study-SILU01-R1.claim до W0. Existing S/R, claim или partial запрещают повтор даже через прямой Program. Существующий claim/raw не удалять и не обходить новым ID. При отдельном поручении только этого ID NEXT=END; общий следующий [HNSW-QUERY-NORM-REUSE-01](HNSW-QUERY-NORM-REUSE-01.md) действует лишь в разрешённой очереди по [next-contract](00-execution-index.md#next-contract); A5 — историческая метка.
>
> Уточнение14308: SILU-DENOM-REUSE-01 — alias этого ID, не новый SD01/host/бюджет. RequiresGrad не равен training:true: LoRA строит differentiable граф и при scoring. Switch reuse=false на подготовке, A и всех readouts; true только в четырёх B updates. Внутри B выделяй denominator только при switch && x.RequiresGrad. Расходы безусловного production inference cache этим опытом не установлены.
>
> При последующем разрешении исполнения оба плеча восстанови из одних bytes: new LlmModel→ApplySeeded q/v LoRA→LoadCheckpoint→4/12 named-state check→TryImportPortableState; optimizer400→404. A и B: ровно 4 native updates T+,T−,T+,T−, один фактор. Consumer:8 updates/168 targets/16 scoring forwards; common0. Отдельный producer:400 updates/8400 targets/8 scoring forwards; сумма408/8568/24, максимум 300 секунд native phase; нет grid/warm-up/автоповтора. Главная метрика — полные native Train elapsed_ms; рядом CPU, allocations, payload дополнительных arrays, losses, logits/weights/optimizer parity, exact/NLL/rank TRAIN и EXAM до и после. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; роста accuracy не требуется. Все ухудшения показывай рядом. Не называй расчёт Exp или helper-probe улучшением native training.
>
> Один report.md по шаблону раздела 8, raw.json/per-row.csv/costs.csv/status.json только в R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1; shared checkpoint INPUT (read-only) в S=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-silu/v1; unknown=NOT_MEASURED. При обычном завершении с полезным измеренным результатом запиши R/candidate.md и краткий кандидат≤120слов в C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/required-changes.md: исходный метод, before/after, регрессии, граница и raw/report. Это не product fix; STOP/NEXT выше оформления. Не исправляй production или тесты и не проходи полный цикл интеграции.
>
> Обязательный запрет: STOP/«закончить немедленно» — немедленно остановить действия. NEXT/«следующий эксперимент» — немедленно оставить предыдущую задачу и перейти только к явно заданной следующей в разрешённой фазе. Никакого продолжения предыдущего report/review/hash/git/warm-up/повтора ради «завершения». При явном STOP сообщи лишь доступный статус без новых действий. Не превращай build failure, 0 выполненных updates или 0/0 ответов в успешное обучение.

[silu]: C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1129
[trainer]: C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:117
[model]: C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:219
[block]: C:/Projects/llm_mvp_core/03.TransformerModel/TransformerBlock.cs:45
[ffn]: C:/Projects/llm_mvp_core/03.TransformerModel/Layers/SwiGLUFeedForward.cs:40
[lora]: C:/Projects/llm_mvp_core/10.LoRA/LoraLinear.cs:113
[adamw]: C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:188
[sanity-source]: C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs
[sanity-raw]: C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json
[restore-source]: C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs
[lft-project]: C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LFT01/35.ExperimentsV2/Experiments/Rp01Micro/NEURAL-FROZEN-TRANSPOSE-01/Runner.csproj

