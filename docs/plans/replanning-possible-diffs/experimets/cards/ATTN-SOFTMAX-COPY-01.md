# ATTN-SOFTMAX-COPY-01 — лишний проход копирования в attention softmax при обучении

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница CORE:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения.

**Исходный адрес и роль:** `04.TrainingLoop/Trainer.cs` — настоящий learner, а изменяемый фактор — `00.TensorCore/Core/TensorOps.cs::Softmax`, last-axis forward в attention. Current source parity проверяется по назначенной копии/срезу; новый общий production cancellation contract этим адресом не доказан. Исторический pin и собственный бюджет ниже сохраняются; роль зависимости не равна factor owner, runtime сейчас не выполнялся.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AC01/a001/results/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



**Маршрут:** №56, ID `ATTN-SOFTMAX-COPY-01`; NEXT END. [Действующий индекс](00-execution-index.md). Старые source labels не номера очереди. Приоритетные учебные блокеры идут раньше этих добавлений.

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


**Повтор уже сохранённого результата запрещён.** Сначала прочитать назначенные receipts/outputs; пригодный результат использовать автоматически без вопроса «reuse или retrain». Отсутствие полного снимка не разрешает новый warm-up, перенос другого donor либо восстановление missing native moments/RNG по догадке. Consumed опыт не получает дополнительных model calls.

Дата:21.10.2026. Core source43e2337d68dad0e4ab8d7fbb0cea38406b592b61. **Низкий приоритет; новая source-backed гипотеза, не измеренное улучшение. Host/runner ниже — будущий рецепт, не существующая проверенная команда. Native runs=0.**

| Поле реестра | Значение |
|---|---|
| ID | ATTN-SOFTMAX-COPY-01 |
| Историческое имя draft | SOFTMAX-INPUT-COPY-01, тот же фактор; не отдельный опыт |
| Статус | LOW_PRIORITY_DESIGNED / NOT_RUN / DEPENDS_ON_QUALIFIED_S0 |
| Тема | C# арифметика; знак целочисленной разности |
| Learning recipient | Native q/v-LoRA + AdamW |
| Перед началом | Учебные блокеры и карточные конфликты программы разобраны; точный RD01-S0 квалифицирован |
| Предыдущий ID | RMS-MEAN-BACKWARD-01 |
| Следующий ID | END |
| Protected tree | docs/plans/replanning-possible-diffs — read-only; исключение required-changes ниже |

[Общий контракт исполнения](../26-output-and-entry-contract.md) обязателен. ID согласован с действующим реестром; старые позиции1–53 сохранены. Предыдущий ID задаёт порядок, не starting state. **Каждое A/B здесь заново восстанавливает исходный RD01-S0, никогда RMS-A1/B1.** Никаких объединённых оптимизаций.

## Вопрос и прямая связь с обучением

Уменьшится ли CPU/elapsed цена одинаковых native SFT/LoRA updates, если в attention Softmax убрать исходное копирование x в resultData и читать x непосредственно при поиске max/exp, оставив арифметику и learned outcome прежними?

Source evidence, все ссылки закреплены на проверенном snapshot:

| Звено | Источник | Доказательство |
|---|---|---|
| Native SFT update | [Trainer102–145](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:102) | Forward→Backward→clipping→AdamW |
| Attention caller | [CausalSelfAttention263–291](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:263) | Scores[B,heads,T,T] после causal mask идут в TensorOps.Softmax dim−1; затем weights@V возвращается в обучающий graph |
| Лишний copy-pass | [TensorOps793–837](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:793) | L797–802 читает и записывает каждую ячейку до того, как L824/836 используют resultData |
| Сохраняемый backward | [TensorOps852–882](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:852) | Native Jacobian-vector product остаётся тем же |

Данные — float32 row-major. Для assigned B1/Q4/T21/L1 один attention score tensor содержит4×21×21=1764cells; столько дополнительных read+write операций делает исходный copy-loop на одном training forward. Это **аналитический подсчёт прохода, не измерение времени/байтов памяти**. Массив resultData и Tensor defensive copy не удаляются.

## Единственный фактор в экспериментальной копии

Добавить `internal static bool ExperimentSkipSoftmaxInputCopy = false` в static class TensorOps. В TensorOps.Softmax вычислить `skipInputCopy = ExperimentSkipSoftmaxInputCopy && normalizedDim == x.Shape.Rank - 1`. A всегдаfalse; Btrue только в TRAIN и after-readout. В general-axis branch оставить исходный copy и арифметику.

1. При !skipInputCopy исходный copy-loop L797–802 выполняется дословно. При skipInputCopy его нет.
2. В max-loop источник — `skipInputCopy ? x.Data[idx] : resultData[idx]`; сравнение `>` и порядок d прежние.
3. В exp-loop источник — тот же; запись `resultData[idx] = MathF.Exp(source - maxVal)`; expSum прежний порядок и float32.
4. Деление на expSum, output Tensor и его backward без изменений. Результат не alias к x; x никогда не мутируется.

Не добавлять pooling, scratch reuse N62, fusion с attention mask, другой softmax algorithm/dtype, head reduction, RMS changes, gradients caching, teacher или иной learner. Сохранить existing cancellation/progress checks в max/exp/normalize; удаление copy-loop меняет место возможного раннего cancellation. Для этого синхронного конечного опыта без concurrent x mutation такой runtime contract не участвует в значении learning result; общая production cancellation-equivalence ещё не доказана и должна быть указана в кандидате, а не замолчана. Не писать отдельный synthetic softmax benchmark.

**Новизна ограничена корпусом:** N62-R2/LF-02 меняют `new float[v]` в `DpoTrainer.BackwardLogProbs`, один scratch на response backward; здесь меняется `TensorOps.Softmax` forward в attention, без сокращения arrays. RMS-ROW-REDUCE / RMS-COORD / ATTN-TRANSPOSE меняют другие методы. Точный перечень поиска и ограничения — [dedup receipt](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-audit-43e2337/evidence/learning-code-dedup.json>). Проверенное исходное поведение не доказывает native gain. Ресурсная разница может оказаться нулевой или отрицательной; такого результата достаточно для честного отчёта, без подбора более выгодного fixture.

## Самостоятельное A/B из общего S0


### Реестр и зависимость

Статус: **LOW_PRIORITY_DESIGNED / NOT_RUN / DEPENDS_ON_QUALIFIED_S0**. Назначать после решения приоритетных учебных блокеров и после получения квалифицированного общего состояния RD01. Source-only находка не получает `SUCCESS` и не добавляется как измеренное улучшение.

Реестр должен различать `depends_on=h64-rms-row/v1:COMPLETE_QUALIFIED_SFT_STEP400_FULL_STATE` и `next_id=END`: зависимость не разрешает исполнителю запускать RD01 или продолжать уже остановленный опыт. Прямое указание Любы NEXT/STOP всегда немедленно прекращает прежнюю работу. Нельзя четырежды продолжать старую карточку, дописывать её необязательные файлы или выполнять новый train после NEXT.

### Общий обученный before: один назначенный donor

Единственный источник:
`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1`.

Полный donor contract обновлён по [core0cf3634/33](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/33-shared-learned-starts.md): `S0.weights.bin`, `S0.optimizer.bin`, `S0.rng.json`, `preparation.json`, `generation.json`, `tokenizer.json`, `data.source.jsonl`, `data.material.json`, `authority.json`, `source-manifest.json`, `bundle.json`. Только COMPLETE/qualified bundle этой family допустим. Статус — `FUTURE_NOT_MATERIALIZED`; scripts — `DESIGNED_NOT_IMPLEMENTED_NOT_RUN`. Прежний `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/common` — только original source назначенного экспорта, не запасной donor. Источник фактора остаётся pinned43e2337; все36 native files совместимы с0cf3634 по [проверке](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-audit-43e2337/evidence/late-shared-start-delta.json>).

**В этой новой карточке common TRAIN=0.** Не обучать ещё400 шагов, не брать другой checkpoint, не заменять восстановление одинаковым seed, не брать `RESTORE-MINIMAL-SET` только потому, что там есть файлы. Если named donor ещё не создан/не квалифицирован, эксперимент остаётся ожидающим конкретную зависимость; это не включает поручение consumer на Prepare/Export; общая подготовка остаётся отдельным назначением по контракту33. Никаких автоматических retry/a002/alternate donor.

До запуска arms автор проверяет receipt: `learning_qualified=true`, actual400 updates, точная config/data/stream и хотя бы один приобретённый **полный однотокенный ответ**: W0 false→S0 true на T1 или T2. Состояние с1/2 показывает приобретение named answer, но не превосходит constant-label1/2 и не доказывает общий закон знака. Оба learned IDs и ограничения обязаны быть в отчёте. Если нужен вывод о различении двух знаков, нужен2/2; исполнитель не увеличивает budget до этого результата.

Donor original source был закреплён на66947785d1318f7cbbc88118f3156cdc75001e16. **Все36 файлов его native source closure имеют те же Git blob SHA на текущем43e2337d.** Это проверено по Git tree metadata, перечень — [source closure36](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-audit-43e2337/evidence/learning-code-source-closure.json>). Проверка совместимости исходников не доказывает наличие donor bytes.

Private identity для обеих arms: `topologySha = lowercase SHA256(UTF8("RD01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj"))`; `loraConfigurationSha = lowercase SHA256(UTF8("RD01|alpha8|dropout0|seed62002|rms-row-reduce-probe-v1"))`. Без BOM и конечного LF. Не заменять префикс RD01 именем MB01/AC01 и не перемаркировать несовпадающий старый snapshot.

### Место, модель и запуск

- Единственный будущий host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AC01/a001`.
- Results: тот же host + `/results`; A/B raw: `results/A/code-sign.json`, `results/B/code-sign.json`; русский отчёт: `results/report.md`.
- Модель исполнителя: модель назначенной сессии по умолчанию, без override. Внешний teacher/judge не нужен.
- Student: V256/Context64/H64/L1/Q4/KV2/FF176/Rope10000/RmsEps1e-5; q/v-LoRA rank4/alpha8/dropout0; base seed62001, adapter seed62002, stream `rms-row-reduce-probe-v1`, **тот же stream donor**, не новая строка по ID этого опыта.
- Native source: ровно36 файлов собственного [source closure36](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-audit-43e2337/evidence/learning-code-source-closure.json>), current pin43e2337d. Получать bytes `git show <pin>:<path>` через argv без shell; проверить Git blob SHA до изменения копии. Только копия TensorOps получает фактор. Product files и старые результаты read-only; docs read-only с единственным исключением обычной записи required-changes ниже.
- `Runner.csproj`: net9.0 Exe, AssemblyName `ATTN-SOFTMAX-COPY-01`, compile36 явных src files+`Program.cs`; external Directory.Build.props/targets выключены. Bin/obj только под AC01/a001, пути задаются до SDK imports. Без solution build/широких tests/установки иного SDK.
- Будущие файлы host: `Program.cs`, `Runner.csproj`, `run.py`, `src/`, `source-copy-manifest.json`, `one-factor.diff`. Этот аудит их не создаёт и не объявляет CLI проверенной.

Единственная будущая внешняя команда **после подготовки именно этого host и квалификации donor**:

```powershell
Set-Location 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AC01/a001'
C:/Python314/python.exe ./run.py
```

Точный build-контракт `Runner.csproj`: свойства назначаются **до явного импорта Sdk.props**: `ImportDirectoryBuildProps=false`, `ImportDirectoryBuildTargets=false`, `EnableDefaultCompileItems=false`, `BaseIntermediateOutputPath=obj/`, `MSBuildProjectExtensionsPath=obj/`, `OutputPath=bin/`, `AppendTargetFrameworkToOutputPath=false`, `AppendRuntimeIdentifierToOutputPath=false`. После него объявить `TargetFramework=net9.0`, `OutputType=Exe`, `ImplicitUsings=enable`, AssemblyName из этой карточки, явные36 Compile+Program.cs и импорт Sdk.targets. Wrapper выполняет argv `dotnet build Runner.csproj -c Release --nologo --verbosity minimal` с cwd именно host; поэтому DLL лежит ровно в `bin/<AssemblyName>.dll`, как в командах ниже. Сборка не включает посторонние cs-файлы и production targets.

Runner без аргументов: фиксированный root от `__file__`; сначала root `execution.claim` create-new, затем results/create-new и одна Release build. После проверки donor и build выполнить последовательно:

```text
dotnet bin/ATTN-SOFTMAX-COPY-01.dll A code-sign results/A/code-sign.json --owner-nonce <nonce>
dotnet bin/ATTN-SOFTMAX-COPY-01.dll B code-sign results/B/code-sign.json --owner-nonce <nonce>
```

Ненулевой exit code A не разрешает начинать B; существующие claim/results/bin/obj не удаляются и не обходятся подбором другого aNN. Сохранить build.stdout/stderr и отдельные arm stdout/stderr. `execution.json`: стадия, issued/terminal, actual exit, complete/unknown counts; отсутствующее измерение=null, не0.

### Что точно делает Program

1. Проверить `bundle.json`: family=h64-rms-row/version1/status=COMPLETE, qualification и SHA256/length остальных10 файлов; сохранить read-only receipt. Сверить source/config/данные и фактические native веса/m/v/RNG. Ничего не писать обратно в bundle; дополнительных Forward/Train для этой проверки нет.
2. Создать policy через native constructor+ApplySeeded с указанной topology, затем загрузить `S0.weights.bin`. После Load повторно инициализировать adapter запрещено.
3. Сформировать `AdamWParameterBinding` из native `GetCheckpointParameterEntries()` и фактических attached q/v Parameter references, ordinal keys. Не придумывать имена по числу параметров. `AdamW(.9,.95,0,1e-8)`; `AdamWPortableState.FromBytes` и `TryImportPortableState` должны успешно восстановить step400 и все key/shape/m/v bits. Повторный Export сравнить с прочитанным portable snapshot.
4. `S0.rng.json` читать `JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>`. Это private JSON полного native snapshot, не файл native DPO writer. `RestoreRandomState(snapshot,new LlmModelRandomStateIdentity(topologySha,loraConfigurationSha))`; Capture сверяет все stream/cursor/seed/step fields. Dropout0 не разрешает скрытую замену topology.
5. Сверить все загруженные named weight bits/shapes, frozen hash, optimizer и RNG. A0/B0 должны точно совпадать. Создать `Trainer` с Warmup0, MaxLr=MinLr.001, TotalSteps404, Batch1/Seq64/GradAccum1/Clip1. Новый счётчик Trainer0 допустим здесь только из-за constantLR; native AdamW остаётся400.
6. Перед TRAIN снять одним и тем же Observer панель T1,T2,E1,E2, training:false, prompt-only last logits. Затем **четыре** native `TrainSupervisedNextTokenStep`: T1,T2,T1,T2, weight1. После них снять ту же панель. A0/B0 выполняются исходным forward; B-factor включается перед B TRAIN и остаётся включённым для B1 readout. A всегда исходный. Число readouts не увеличивается. RMS-MEAN-BACKWARD и все другие факторы выключены.
7. Из четырёх real updates сохранить online loss, все post-update named weights, optimizer step404, все m/v bits и статистику затрат. Сохранить zero/finiteness/changed-state flags; числовые прочтения после TRAIN не добавляют forward/backward.
8. Observer: UTF-8 bytes Prompt (без Answer), `model.Forward(training:false)`, последние256 logits; predicted ID = максимальный logit, ties=min token ID; gold rank=1+число logits строго выше gold. NLL logsumexp считать одинаковым методом из одних logits A/B. `exact = predicted == byte(Answer)`. Full answer здесь1 byte, не первый токен длинного текста.

### Данные, scope и бюджет

| Split | ID | Prompt буквально | Answer |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Тема — C# арифметика, задача — знак разности. Это не уроки разных предметных доменов; два знака — два класса одной задачи. TRAIN целиком `Prompt+Answer`, без newline/BOS/EOS; UTF-8 byte IDs0…255, BPE Train/Encode не вызываются. Native objective остаётся full-sequence SFT. Тождественная/переименованная схема EXAM ограничивает перенос: не доказывать общий арифметический навык.

Полный новый runtime budget после готового donor: **8 optimizer updates / 8 training forward / 8 backward / 16 diagnostic forward**, всего24model forward. На каждом arm4 updates,8 readouts. Common retraining0, Generate0, DPO0, teacher0, finite-difference0, warmup0, retries0, дополнительные seeds0. Зафиксированный общий deadline600s: build≤180s, каждое arm≤120s, чтение/summary≤60s, оставшееся — подготовка. Это operational budget, не гарантия успешной сборки/обучения в любой среде.

### Метрики, успех и регрессии

Измеряется стоимость четырёх **native TRAIN calls**, включая forward/backward/clipping/AdamW. Build, restore, readout, сериализация и hashes вне cost timer. Primary resources — CPU time настоящих четырёх updates (Process.Refresh + TotalProcessorTime) и elapsed Stopwatch. Дополнительно `GC.GetAllocatedBytesForCurrentThread` (managed текущего потока, не общий RAM), GC0/1/2 и peak working set процесса. Уменьшение allocated arrays этим фактором не ожидается: удаляется только copy-pass, массив результата остаётся. Из-за малой модели wall/CPU может не показать выигрыша; это допустимый измеренный исход, не разрешение расширить бюджет.

Сохранить A0→A1, B0→B1 и difference-in-differences на **одинаковых** readout rows; не сравнивать online loss разных уроков вместо before/after. Отдельно TRAIN exact/NLL/rank, EXAM exact/NLL/rank; learned answer retention. Целевой контроль эквивалентности — побитовые A/B конечные weights, m/v, step и panel logits, finite values; если есть расхождение, сохранить значения, не округлять его доPASS. Материальная регрессия одной оси не скрывает измеренное улучшение другой, но ограничивает статус семантической нейтральности.

**Любое измеренное улучшение полезной метрики в валидном учебном сравнении считается успехом именно этой метрики/темы; роста accuracy не требуется.** Никакого обязательного5%/10% порога. Если A/B incomplete или learned before не доказан — обучение невалидно, cost cannot close learning objective. Wall time одного фиксированного A→B чувствителен к порядку/JIT/load; не выдавать его за устойчивое ускорение всех моделей. Additional rerun не следует автоматически из шума.

### Ожидание, отчёт и остановка

До launch обнаружить реальный tool launch и wait/resume в текущем каталоге и прочитать схему. Одна выдача entry, затем same-handle bounded blocking/event wait до terminal или исходного deadline. Yield/RPC timeout, отсутствие output и пустой файл не доказывают остановку. Не заканчивать ход со словами «жду, сообщу потом», не перезапускать entry, не заводить background monitor/watchdog/summary script и не использовать быстрый polling вместо ожидания. При противоречии tool responses проверить именно исходный handle/PID+start и известные raw; неизвестные updates оставитьnull. Руководство: [docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md). В среде с `exec` running cell используется wait того же cell; имя чужого Kiro tool не выдумывать.

STOP/NEXT Любы — немедленно: не продолжать прежний эксперимент ради отчёта, косметики, проверки или публикации. После обычного завершения один русский `results/report.md`: «Что изменили»; «Обученный before»; «A0/A1/B0/B1»; «Цена TRAIN»; «Улучшения и ухудшения»; «Границы и кандидат». При полезном измеренном эффекте создать `results/candidate.md` с оригинальным методом, единственным diff, метрикой, темой, ограничениями и ссылками raw. Решение о переносе в product остаётся за Любой; ни production patch, ни исправление product tests/CI не входит в этот опыт.

Skills будущего исполнителя закреплены на core snapshot: `docs/skills/architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `capability-measurement-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `output-readability-gate-SKILL.md`, `human-communication-GLOSSARY.md`. Прямые пользовательские правила о малом опыте, любой полезной метрике и отсутствии prod changes имеют приоритет над историческими thresholds/full production pipeline.


Итоговый `results/result.json` объединяет уже сохранённые A/B числовые arrays, hashes и actual counters; для него запрещены новые model calls. Отчёт `results/report.md` — до350слов пояснений плюс таблица, язык русский; отдельный `results/candidate.md` только при полезном измеренном эффекте.

Сравнение0→0 правильных учебных ответов не является валидным учебным результатом: сдвиг margin/loss его не исправляет. При отсутствии назначенного qualified before — остановка до TRAIN; никакого самостоятельного добора уроков, нового warm-up или другого donor.

## Обязательное уточнение восстановления и сохранения

**Точный weights API для этой карточки:** `cfg` — фиксированный ModelConfig этой карточки; LoRA rank4, targets строго `q_proj,v_proj`. Producer: `model.SaveCheckpointToStream(stream)` на новом `FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None)`, изначально Length=0/Position=0; `Flush(flushToDisk:true)`. Затем независимо записать остальные state/receipt по33 и только после этого открыть сохранённый weights для чтения. Установить Position=0; `var checkedArtifact = LlmModel.ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(4,new[]{"q_proj","v_proj"}))`; требовать `checkedArtifact.IsSuccess && checkedArtifact.Value==true`. Если в конкретном рецепте уже назначен maximumArtifactBytes, передать его неизменным (здесь native default). Перегрузка без topology недопустима для этого LoRA snapshot. Перед native `freshModel.LoadCheckpoint(stream)` снова Position=0 либо открыть новый read stream; construction/ApplySeeded только до load. Проверить назначенные named keys/shapes/value bits. Равный размер файла, повторное чтение самого файла или снижение loss не доказывают равенство потерянной live memory. В consumed/recovery случае такое равенство помечать UNKNOWN, не придумывать. Эта запись уточняет уже назначенные операции, не добавляет model calls.

**Сохранение конечного состояния:** захватить уже вычисленные weights, optimizer/m/v/step404, полный RNG и readouts/counters; записать каждый артефакт create-new до потенциально падающей валидации другого. Ошибки записывать отдельно; остальные захваченные артефакты сохранять лишь при действительных owner/path guards. После записей — typed validation/readback; COMPLETE последним только при всех обязательных успешных проверках. Дополнительные Train/Forward, периодические checkpoints и повтор опыта для восстановления отсутствующих данных запрещены.

**Совместимость исходников:** [read-only сверка36 Git blobs](../../2026-10-21-d072-integration/source-compatibility.json) с текущим core4f35cb71b3db482bfe7ebe611c652c78d89d5db1. Это не свидетельство существования или квалификации donor. Source pin43e2337d остаётся исходником фактора; donor identity RD01 не меняется.

**Узкое исключение при STOP/NEXT:** если собственный уже выданный процесс ещё работает, прекратить только его через известный handle/PID и подтвердить прекращение тем же handle. Не оставлять обучение работающим из-за запрета оформительских вызовов; не искать новые процессы, не трогать чужие, не запускать relaunch/отчёт/проверки. Если отмена не подтверждена, сообщить известный handle и этот факт. После прямого NEXT перейти к указанному ID в текущей разрешённой фазе.

**Точные post-arm пути:** для каждого arm `A`/`B` внутри назначенного host: `results/<arm>/S1.weights.bin`, `S1.optimizer.bin`, `S1.rng.json`, `code-sign.json` (уже вычисленные before/after logits, per-row ответы, counters и costs), `artifact-write.json` (отдельный исход каждой записи/проверки). Это четыре уже назначенных update и сохранение полученного состояния, без новых model calls. Итоговый `results/result.json` строится только из этих raw; status COMPLETE требует обязательные artifacts/валидацию обоих arms, иначе INCOMPLETE с actual/null counts. К исходному shared S0 ничего не дописывать.

## Одноразовое владение запуском и регистрация

**Точный child guard MB01/AC01:** root `execution.claim` wrapper пишет CreateNew до build: ownerNonce, PID/start time wrapper, canonical host/results, source pin, donor identity и expected arms. Под тем же host wrapper создаёт только каталог `claims/`; каждый Program получает ровно пять аргументов: `<A|B> code-sign results/<A|B>/code-sign.json --owner-nonce <nonce>`. До bundle read/load/первого Forward/Train сверить ownerNonce с root claim, exact arm/mode/output/host и живой wrapper PID с тем же start time; затем создать `claims/<A|B>.claim` через FileMode.CreateNew и записать nonce, own PID/start и bound output. Existing arm claim отказывает даже при прежнем nonce; его не удалять и не менять arm/result suffix. Запуск B допустим лишь после подтверждённого успешного terminal A; ошибку A не обходить. Bare dotnet без owner binding отказывает до model calls. Стадии/actual counts фиксировать в том же owner, существующий partial сохранять. Эти guards не добавляют model calls.

**Обычное завершение и разрешённая запись:** русский `results/report.md` — до350слов пояснений плюс таблицы и raw-ссылки. При полезном измеренном результате добавить/уточнить одну запись до120слов в `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/required-changes.md` по правилу индекса, без отдельного вопроса и повторного запуска. Это единственное исключение из read-only режима карточек/реестров для будущего исполнителя; product code не менять. При отсутствии полезного эффекта достаточно отчёта с фактическим исходом и ограничениями. Прямой STOP/NEXT отменяет оформление старой задачи.

**S1 RNG:** полный native snapshot сохраняется с исходными RD01 topology/lora identity, фактическими stream cursors и metadata RandomStateStep=404; не подменять его seed или DPO writer-файлом.
