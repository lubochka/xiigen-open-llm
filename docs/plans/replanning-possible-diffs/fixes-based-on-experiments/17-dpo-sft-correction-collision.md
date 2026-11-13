# FIX-N63C — не возвращать отвергнутый текст в SFT после исправляющего DPO

**Предметные данные для расширения, 4 ноября:** [158](codex-158-learning-sets-transfer-coverage-2026-11-04.md) связывает эту работу с DOC-CS-NULL/M002: утверждённое объяснение не должно вновь заменяться ошибочным Trim после исправляющего DPO. Это будущая отдельная панель после review/qualified start, без изменения исходной дозы 17. CODE-CS-NULL — другой получатель; смешивание специалиста кода с документацией не назначено. Старый успех удержания токенов и экзаменационные ухудшения сохраняются рядом; улучшение на новых данных пока не измерено.

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски LEARN:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

В учебной проверке actual logits/vector/mask от восстановленного состояния должны попадать в назначенный trainer/reader; canned tensor допускается только в отдельно названном component test. Нулевой optimizer step delta и неизменные веса не доказывают отсутствие Backward. Квитанция различает measured/structural/unknown; эту границу P08 consumer уже пришлось исправлять. Предусмотренное контрактом чтение reference cache или выученной памяти разрешено: проверяется фактический hit/miss и привязка к правильному state/key. Запрет касается подмены проверяемого производителя заранее подготовленным oracle, а не самого механизма FIX-LC.

Для FIX-N63C исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Для метрики DPO указывать единицу pair/step/panel, веса и фазу. Контрпример одного шага с margins [+1,-3]: доля положительных пар 0,5, признак положительного среднего 0; совпадение этих счётов на исходном опыте не делает их одной метрикой. Проверка вызывает полный writer с metric_definitions, а не только внутренний builder. Неизмеренное время SFT — null, не0; исторические receipts не переписывать.

Применимость: FIX-N63C. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Проверять по отдельности optimizer step, scheduler step и число реально применённых обновлений. Контрпример P12: импортированные моменты AdamW400 совместимы с новой эпохой scheduler0; P10, напротив, использует fresh DPO0. Не переносить правило fresh/import из соседнего опыта. После отказа сверять веса, моменты, RNG и исполненные Forward/Backward; отчёт о правильных ответах без назначенного обучения не закрывает учебную цель.

Применимость: FIX-N63C. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-N63C

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Уточнение исполнения, 27 октября 2026.** Перед сравнением после stale-SFT запрещён «восстанавливающий» SFT или повторная квалификация donor: они уничтожат измеряемую коллизию. Использовать уже назначенные промежуточные состояния и immutable acquired gold; stale response A остаётся учебным входом, но не становится ожидаемым ответом evaluator. Сравнивать поштучные правильные токены вместе с полным продолжением optimizer/RNG; равные margin или counts не являются доказательством удержания. Лимит 2 DPO + 40 SFT и 42 scoring Forward не увеличивается.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

**Адресная поправка 25 октября 2026:** согласованы donor CDB01, общий с SEP Abs payload, неизменный правильный gold и два профиля чтения. Действующий budget: **2 DPO + 40 SFT, 36 no-LF + 6 LF = 42 scoring Forward, acquisition 0**. Это уточнение существующего кандидата; новое основание связи и ремонт тестов — [§11](#n63c-fixed-gold-readout).

**Актуализация 23 октября 2026:** опыты57–59 добавлены в§8. Это уточнение существующего FIX-N63C, не три новых кандидата. Для будущего native теста§4 используется назначенный ниже donor CDB01 и20SFT updates/плечо при LR.003; прежний100-шаговый рецепт сохранён только как историческое основание и больше не является командой к исполнению. Механизм product projection и запрет подменять admission остаются прежними.

21 октября 2026. **Условный план переноса частичного удержания навыка.** Основание — завершённый `N63C-PROBE`, а не исходная короткая попытка с `0/23`. Кандидат уже описан в [required-changes](../required-changes.md#n63-correction-collision); здесь определены место применения, provenance, неизменяемый набор и проверка производственного потребителя. Реализация, новые вызовы модели и повтор consumed experiment сейчас не назначены. Приоритет и следующий ID — [реестр](01-registry.md).

## 1. Что действительно улучшилось

[Первичный JSON](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json) содержит 600 SFT-шагов приобретения, затем по одному DPO и 100 SFT на каждое плечо: **802 updates, 60 scoring forwards, 0 Generate, 0 externalTeacherCalls**. После приобретения модель воспроизводила 22 из 23 response-токенов T1 при teacher forcing.

| Показатель | Общее обученное «до» | A: после DPO → устаревший SFT | B: после DPO → исправленный SFT |
|---|---:|---:|---:|
| Правильные response-токены T1 | 22/23 | 20/23 | 21/23 |
| Mean-logP margin T1, no-LF | 1,1063861 | после DPO1,1252952 → после SFT−0,119724035 | после DPO1,1252952 → после SFT+1,1151558 |
| Chosen NLL T1 после SFT | исходное 1,1099185 | 1,1158044 | 1,1042708 |
| Полный teacher-forced ответ | false | false | false |
| EXAM Q1/Q2, правильные токены | 0/29; 0/21 | 0/29; 0/21 | 0/29; 0/21 |
| EXAM Q1 chosen NLL после SFT | — | 6,987412 | 7,074539, хуже |
| EXAM Q2 chosen NLL после SFT | — | 7,2149773 | 7,250893, хуже |

Это полезное **частичное удержание T1**, при потерях относительно 22/23 в обоих плечах. B повторяет тот же текст, который был приобретён, поэтому сохранность ожидаема; опыт не доказывает обобщение, правильную свободную генерацию или универсальную пользу исправлений. `EXAM 0→0` не считается учебным результатом по этим строкам. Исходная карточная попытка и диагностическая проба не объединяются в один якобы полный A/B.

Тема — объяснение/исправление C#; `CODE_Q` в design fixture и `code-documentation` в native probe — разные обозначения уровня данных. Их не объявлять разными предметными темами и не смешивать с переводом. Для будущего native теста ниже runtime-domain задан точно: `code-documentation`; исходное поле fixture сохраняется в паспорте без переписывания истории.

## 2. Почему одного исправления кэша недостаточно

На срезе `4f35cb71b3db482bfe7ebe611c652c78d89d5db1`:

- [ContinuousLearningPipeline.RunCycleAsync](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs#L52-L82) добавляет исходные indexed examples в кэш.
- Затем [тот же метод](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs#L175-L244) строит `domainExamples` из результата indexer и передаёт их в `TrainTransactionAsync`/`TrainAsync`. Замена только записи кэша не меняет этот массив.
- [IncrementalTrainer.TrainCoreAsync](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Training/IncrementalTrainer.cs#L180-L266) делает immutable snapshots и проверяет DPO admission. [Дальше](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Training/IncrementalTrainer.cs#L347-L443) строятся source manifest, токены `Prompt + "\n" + Response` и digest точного supervised batch.
- [После DPO](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Training/IncrementalTrainer.cs#L647-L677) именно эти токены получает `ISupervisedTrainingStepExecutor.Execute`.

**Выбранное место фактической замены — внутри `TrainCoreAsync`, после успешного native DPO admission и до построения manifest/tokenSequences/candidate seed.** Это уточняет предложение родительского реестра «до provenance/material binding»: pipeline продолжает передавать реальный исходный массив; тренер строит и использует его исправленную immutable проекцию. Так исправление нельзя обойти прямым вызовом тренера, и ещё не допущенный DPO не получает власть изменять SFT.

Порядок DPO → SFT не меняется. Индексируемые исходные файлы и кэш наблюдённых исходников не переписываются выбранным ответом. Не добавлять fuzzy matching, семантический поиск коллизий, last-write-wins, смену обучения LoRA или новый DPO gate: этого опыт не проверял.

## 3. Точное правило и закрытый список будущих изменений

Новый внутренний файл: `14.ContinuousLearning/Training/DpoSupervisedCorrectionProjection.cs`. Он возвращает immutable `ProjectedSupervisedBatch`: `Examples`, `OriginalBatchSha256`, `Corrections`, `TrainingSources`. Публичный six-field `TrainingExample` не получает фиктивного поля `SourceIds`; [такого поля сейчас нет](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Core/Core.cs#L13-L19).

Новая проекция выбирается только для целевого домена без записанного candidate seed; уже существующий seed задаёт версию и точные bindings по правилу recovery ниже. Наличие mutation в другом домене транзакции не подменяет состояние текущего домена. Для каждого SFT-примера нового кандидата в исходном порядке:

1. Сравнить `Domain`, `Prompt` и `Response == pair.Rejected` через `StringComparer.Ordinal`. Не trim, не lowercase, не AST-подобие.
2. Допустить исправление только если native admission уже принят, chosen непуст и `Chosen != Rejected`. В исходной fixture chosen — буквально `static int Abs(int n)=>n<0?-n:n;`.
3. Если найден ровно один подходящий pair, заменить только Response и derived SourceId. Prompt/Domain/Weight/CapturedAt, позиция и число уроков сохраняются.
4. Нет совпадения — передать пример без изменений. Несколько подходящих pair, даже с одинаковым chosen, — `DPO_SUPERVISED_CORRECTION_AMBIGUOUS` до model mutation. Исполнитель не выбирает по timestamp или порядку случайной коллекции.
5. `DpoPair` не имеет `PairId`/`SourceId`. ID берётся из [`TransactionPairSnapshot(PairId, Pair)`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Feedback/FileDpoPairStore.Transactions.cs#L4792-L4808), сверенного с admitted batch в исходном порядке. Внешний пользовательский ID не подставляется вместо него.

Для исправленного примера создать канонический derived SourceId существующим [`TrainingSourceProvenance.CreateSourceId`](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Core/TrainingProvenance.cs#L72-L83): origin = `dpo-sft-correction-v1:{originalSourceId}:{transactionPairId}:{pairBatchSha256}`, content = точные bytes выбранного Response как UTF-8 string. Prompt/Domain и остальные поля отдельно связаны exact batch digest. Не конкатенировать два `src:...` в одно неканоническое значение.

Для каждой замены сохраняется внутренний `DpoSupervisedCorrectionBinding`: ordinal примера, originalSourceId, correctedSourceId, transactionPairId, pairBatchSha256, SHA-256 исходного и исправленного Response. Обе идентичности источника входят в provenance. Сначала проверить уникальность **исходных строк** штатным `BuildManifest`; затем объединить уже проверенные original и derived IDs как lineage set. Этот union не отменяет действующий отказ на duplicate input SourceId и не создаёт дополнительных учебных строк.

| Файл | Изменение |
|---|---|
| Новый `Training/DpoSupervisedCorrectionProjection.cs` в project14 | Описанная проекция и immutable binding; без доступа к файлам/обучению/кастомному authority |
| `Training/IncrementalTrainer.cs` | После admission получить exact domain transaction pairs; применить проекцию; использовать `Examples` для tokenSequences и обучения, `TrainingSources` для provenance. Не ограничиться локальной переменной, которую последующий код игнорирует |
| Тот же `IncrementalTrainer.cs::BuildTrainingExampleBatchSha256` | Без исправлений сохраняется прежний `xiigen.dpo-supervised-example-batch.v1`. Для исправленного batch новый v2 digest связывает все прежние поля, originalBatchSha256 и ordered correction bindings. Hash вычисляется после проекции |
| `Feedback/FileDpoPairStore.Transactions.cs` | У `CandidateProvenanceSeed` добавить optional typed `SupervisedProjectionIdentity` (version, originalBatchSha256, ordered bindings); согласованно обновить JSON read/write, canonical validation, seed equality и exact recovery. Добавить readonly immutable `CandidateSeed` в `TransactionDomainSnapshot` и возвращать его через `ReadTransactionSnapshotCore`/`TransactionHandle.Read()`. Старые записи без projection-поля сохраняют v1-семантику |
| `Pipeline/ContinuousLearningPipeline.cs` | Проверить actual consumer и существующую recovery-ветвь; не внедрять вторую независимую проекцию в кэш или pipeline. Изменение здесь нужно только если required typed handoff не сохраняет snapshot, а не ради косметического дублирования |

Текущий `TransactionDomainSnapshot` содержит состояние/артефакт, но **не seed**; одного имеющегося `Read()` недостаточно. Будущий readonly handoff выше обязателен: `PrepareCandidateCore` сравнивает seed до `TryRecoverCandidate`, поэтому нельзя сначала безусловно построить v2 и только потом надеяться найти `ExistingArtifact`.

Правило выбора по **конкретному domain snapshot**, включая `CandidatePrepared` до mutation: нет seed — вычислить новую проекцию; есть v1 seed — проверить исходный v1 batch и сохранить v1 без коррекции; есть v2 seed — проверить исходный batch против stored original digest и применить ровно записанные bindings с повторной проверкой exact admitted pair. Не выбирать новые пары заново. `PrepareCandidate` затем атомарно повторяет проверку seed/состояния: изменение между Read и Prepare завершает попытку конфликтом, а не сменой версии. Aggregate `TransactionSnapshot.MutationStarted` не используется вместо состояния домена.

Кандидат с любым уже записанным seed, начавшейся mutation или готовым `ExistingArtifact` **не переводится задним числом** из v1 в v2. Recovery завершает тот же записанный кандидат; готовый артефакт не получает повторного DPO/SFT. Несовпавшие данные дают recovery failure. Новый материал требует нового явно назначенного transaction. Старая модель не переименовывается «исправленной».

Усиленная durable binding нужна потому, что этот же seed управляет продолжением обучения. Это не отдельная инфраструктурная гипотеза и не причина добавлять не относящиеся к consumer проверки.

## 4. Набор, состояние и предел будущего обучающего теста

Источник fixture восстановлен точно: [correction-collision.json в xiigen-planning](https://github.com/lubochka/xiigen-planning/blob/d0722426a90b90486b7b1d446c2478f570190920/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-ingress-and-retention-review/ingress/fixtures/correction-collision.json). Его SHA-256 **`fcf2e07aa283ad40622b7c05eee738d0c490c5c2448f686fde935775ecff1992`** совпал с raw probe. Это `DESIGN_FIXTURE_NOT_ADMITTED_DPO_MATERIAL`, authored simulation; паспорт учителя не предоставлен. Не переименовывать его в production-admitted DPO.

Будущий fixture: `Tests/Fixtures/ExperimentFixes/v1/N63C/`, файлы `correction-collision.json` и `manifest.json`. TRAIN: одна T1 Abs.cs. EXAM: неизменные `same-rule-other-name` и `unrelated-sign-control`; их chosen/rejected используются только для оценки. Никакого нового генератора или смешивания тем.

Текущий donor: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CDB01/results-01/authority-acquired.bin`, основание —60-шаговое приобретение в опыте59 и его raw `n63c-probe.json`. Прежний `N63C-PROBE/results/authority-acquired.bin` сохраняется как historical evidence. Конфигурация из native LoadModels: `config/code-documentation/generation-v1.json`, config SHA `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`, H512/V629/C2048/L4/Q8/KV2/FF1408, q/v LoRA r8/alpha16/dropout0. Штатный B0 reference остаётся неизменным; **после** создания topology/adapters загружаются donor policy weights. Нельзя после load ещё раз вызвать инициализацию адаптеров.

Точные исходные загрузчик, materializer и scoring видны в [N63CollisionProbe.cs](../../../../35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs). Current constants `60 / .003 / 20` относятся к новому поколению, **не аттестуют historical `600 / .0001 / 100` raw**. Старый runner не запускать. Сохранённый CDB01 donor существует, native restore этой документной проверкой не выполнялся; перед использованием сверить полный digest/55 named weights/tokenizer identity и2 независимых начальных readout. Отсутствующее или несовместимое состояние даёт `missing_or_incompatible_fixture`, не новую60/600-шаговую подготовку. Для последующих assertions сохранить принятую identity и использовать тот же donor; исторические acquisition moments не объявлять восстановленными.

Будущая проверка — явно **новая учебная эпоха теста**, не точный исторический replay:

- Восстановить независимые A/B из одного donor. DPO optimizer fresh step0 в обеих руках; одинаковый свежий RNG с исходными seed/stream, dropout0. SFT optimizer также fresh step0 в обеих руках. Исторические SFT moments/RNG не объявляются восстановленными.
- Проверить T1 до воздействия в двух явно названных профилях: no-LF панель — 22/23; SFT/LF retention-gold — 23/23. Ожидание LF связано с тем же payload SHA-256, что измерен в SEP, а не выведено из имени модели. Оба исходных readout A/B должны совпасть; эти чтения входят в бюджет ниже, отдельно их не повторять. При несовпадении проверить actual payload/config/tokenizer/IDs по §11; не обучать до подходящего числа.
- A использует исходную T1, B — **продуктовую** `DpoSupervisedCorrectionProjection`, а не скопированное условие в runner. Runtime-domain в тестовом DTO обеих рук — `code-documentation`.
- В каждом плече один native `DpoTrainer.CreateForProjectLora(...).StepVector` на T1 (LR1e-5), затем20 native `Trainer.TrainSupervisedNextTokenStep`. SFT: LR=min=max.003, batch1, seq2048, accumulation1, warmup0, clip1, AdamW(.9,.95,decay.01,eps1e-8), full-sequence `Prompt + LF + Response`. Это фиксированный рецепт нового20-шагового поколения; неизвестный historical arm LR не выдумывается. B всё время использует product projection исправленного текста; не подменять его чередованием из58 или10+10из59.
- До, после DPO и после SFT прочитать T1/Q1/Q2 chosen/rejected в прежней no-LF панели: 36 scoring Forward. На тех же трёх стадиях каждой руки отдельно прочитать неизменный правильный T1.Chosen в SFT/LF-профиле: ещё 6. **Итого 42 диагностических Forward, 2 DPO + 40 SFT updates, 0 acquisition/Generate/external calls.** Training Forward/Backward учитываются отдельно; 42 — не число всех вызовов модели. Эта явная поправка заменяет прежний бюджет 36; число уроков и стадии воздействия не меняются. Новую dose curve и промежуточные evaluation не добавлять.
- Сравнить все токены, whole exact, состояния и расходы отдельно по профилям: no-LF — correct/NLL/mean-logP margin; SFT/LF — correct и chosen NLL, без LF margin. У SFT/LF обе руки оцениваются против исходного правильного T1.Chosen, даже когда A обучается Rejected. Native DPO loss/reward сохраняются отдельно от диагностического mean margin. Любое полезное улучшение признаётся по своей метрике и теме; ухудшения остаются рядом. Исторические no-LF значения и неизвестные LF-after результаты не превращаются в новые обязательные exact floating-point assertions.

В изолированном тесте разрешены существующие test-only admission fixtures, как в `DpoTrainerOptimizerStateHandoffTests`; они не аттестуют design fixture для продукта. Это исследовательская проверка native learner. Production integration ниже обязана пройти свой настоящий admission и не получает обхода gate.

## 5. Тесты переноса и возможные падения

Новые файлы: `Tests/DpoSftCorrectionProjectionTests.cs`, `Tests/DpoSftCollisionLearningTests.cs`. Второй использует native assemblies и фиксированный рецепт раздела4, не копию Common/ContinuousLearning из эксперимента.

| Проверка | Oracle и допустимое изменение |
|---|---|
| Exact collision / no collision | Меняется только exact stale response; иной prompt/domain/response, empty pairs и уже chosen сохраняются. Никаких «близких» совпадений |
| Ambiguous corrections | До первого optimizer mutation явный error; не выбрать произвольно одну сторону |
| Source lineage / digest | Исходный ID сохранён в manifest, corrected ID каноничен, pair ID взят из transaction. Старый v1 hash неизменен без коррекции; v2 зависит от обеих сторон и порядка |
| `IncrementalTrainer_ConsumesProjectedTokensBeforeBinding` | Через существующий durable transaction fixture передать stale SFT; test spy на `ISupervisedTrainingStepExecutor` фиксирует реальные переданные IDs и сравнивает с независимым `Encode(prompt+LF+chosen)`. Это wiring-test, сам по себе не доказательство обучения |
| `CorrectionCandidate_RecoversWithoutSecondMutation` | После accepted candidate повтор получает ту же identity/bytes и 0 повторных DPO/SFT calls. Отдельно v1/v2: CandidateReady даёт0 повторных calls, CandidatePrepared до mutation выполняет первый проход ровно1раз с сохранённой версией; проверить также другой уже мутировавший domain. v1 не превращается в v2; snapshot/Prepare race отказывает без новой проекции |
| `NativeCollision_NewEpoch_RetainsAcquiredTokens` | Фиксированный двухрукий учебный тест раздела4; отдельные неуспешные EXAM не маскируются улучшением T1 |

В [Tests/IncrementalTrainerDpoWiringTests.cs](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/Tests/IncrementalTrainerDpoWiringTests.cs) сохранить `EmptyPairs_PreserveSupervisedPathWithoutDpoExecution`, `DurableDpoReferenceMutation_FailsClosedBeforePublication`, `Pipeline_RoutesDurablePairToItsDomainExactlyOnce`, `Pipeline_MultiDomainRoutesEveryPairExactlyOnceWithoutDrop` и recovery tests. Ожидания маршрута/числа DPO calls не меняются. Test helper расчёта seed/hash дополняется отдельным v2 case; нельзя глобально переписать старый v1 expected под новый actual.

Тест полной product session должен учитывать [`LoraApplicator.Apply` перед загрузкой ParentAdapterCheckpoint](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/14.ContinuousLearning/Training/IncrementalTrainer.cs#L505-L520). Обычный Apply [переиспользует совместимые установленные adapters](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/10.LoRA/LoraApplicator.cs#L747-L799); существующий код не объявляется переинициализацией. Проверяется фактическая совместимость topology/config и порядок parent load. Здесь тяжёлый quality oracle использует native learner отдельно, а production spy проверяет передачу материала; нельзя объединить их отчёты и заявить measured end-to-end production quality. Публичный IncrementalTrainer в historical probe не вызывался.

Свежих падений сейчас нет в доказательствах: тесты не запускались. Будущий исполнитель сначала получает адресный baseline существующих методов, затем добавляет нижние tests/learning fixture, переносит проекцию, после неё проверяет wiring/recovery. Не понижать gate, не удалять проверки sealed material/teacher/source identity ради зелёного результата.

Будущая команда после реализации тестовых классов, из `C:/Projects/llm_mvp_core`:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~DpoSftCorrectionProjectionTests|FullyQualifiedName~DpoSftCollisionLearningTests|FullyQualifiedName~IncrementalTrainer_ConsumesProjectedTokensBeforeBinding|FullyQualifiedName~CorrectionCandidate_RecoversWithoutSecondMutation|FullyQualifiedName~EmptyPairs_PreserveSupervisedPathWithoutDpoExecution|FullyQualifiedName~DurableDpoReferenceMutation_FailsClosedBeforePublication|FullyQualifiedName~Pipeline_RoutesDurablePairToItsDomainExactlyOnce|FullyQualifiedName~Pipeline_MultiDomainRoutesEveryPairExactlyOnceWithoutDrop" --logger "trx;LogFileName=FIX-N63C.trx" --results-directory artifacts/fbe/FIX-N63C/attempt-0001/tests
```

Raw нового учебного теста: `artifacts/fbe/FIX-N63C/attempt-0001/learning.json`; manifest и русский `report.md` в той же папке. Существующий attempt не удаляется, новый суффикс автоматически не выбирается. Native production authority fixture не получает external teacher calls этим поручением: если допустимого материала нет, фиксируется конкретный недостающий вход; design simulation не перепечатывается как legal teacher.

## 6. Исполнитель, решение и немедленный переход

Исполнитель — модель текущей сессии по умолчанию. Прочитать [общий контракт](03-test-and-execution-contract.md), `docs/skills/architect-behavior-classifier-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-communication-GLOSSARY.md`, `human-readable-architecture-audit-SKILL.md`. Краткие имена после первого пути находятся в той же `docs/skills/`.

Отчёт на русском: факт частичного удержания и потери, source/donor custody, 0→0 EXAM, native learning отдельно от spy/wiring, фактические расходы и результаты восстановления. Любой полезный измеренный выигрыш — успех своей метрики/темы; рост общей accuracy не обязателен. Выпуск и расширение на другие домены требуют отдельного решения, известные регрессии сохраняются.

**STOP/NEXT: немедленно прекратить прежнюю карточку; запрещено сначала дописывать её отчёт, чинить тесты, делать commit или cleanup.** Сообщить только уже подтверждённое состояние и перейти по точному ID. Старые опыты/результаты не удалять и не повторять. Живой назначенный процесс ждать штатным bounded wait того же handle по03; не заканчивать ход вместо ожидания, не опрашивать один tool пустым polling и не relaunch.

<a id="n63c-latest-evidence"></a>

## 8. Новые измеренные ограничения из57–59

Источники: [CDR01 /57raw](../../../../experiments-v2/RP01-MICRO/CDR01/results-01/n63c-probe.json), [отчёт57](../../../../experiments-v2/RP01-MICRO/CDR01/results-01/report.md); [CDI01 /58raw](../../../../experiments-v2/RP01-MICRO/CDI01/results-01/n63c-probe.json), [отчёт58](../../../../experiments-v2/RP01-MICRO/CDI01/results-01/report.md); [CDB01 /59raw](../../../../experiments-v2/RP01-MICRO/CDB01/results-01/n63c-probe.json), [отчёт59](../../../../experiments-v2/RP01-MICRO/CDB01/results-01/report.md). Commits соответственно `d839922f8f`, `2e0ad0bb0d`, `6d68f3f63e`; каждый имеет60acquisition,2DPO,40armSFT,122scoring forwards,0Generate/external calls. Эти прошлые расходы не становятся новым назначением.

Общий обученный before —22/23, после одного исправляющего DPO также22/23, margin+0.8356108. В57уже один устаревший SFT снижает correct до21/23; после второго margin отрицательный. Поэтому нельзя считать «один stale урок допустим» или запретить correction только после отрицательного margin.

| Конец20SFT | A:20устаревших | B57:20исправленных | B58:10+10поочерёдно | B59:10устаревших, затем10исправленных |
|---|---:|---:|---:|---:|
| TRAIN correct/23 |20|22|21|22|
| Chosen−rejected mean-logP margin, no-LF |−0.6125487|+0.87691855|−0.022139013|+0.4630857|
| Chosen NLL |1.1680962|0.62267625|0.5236719|0.500236|

B58 улучшает полезные оси относительно A, но знак предпочтения остаётся неверным. B59 возвращает положительный знак на третьем исправляющем уроке (update13, +0.256711,22/23), к20margin+0.4630857; это восстановление примерно55% положительного after-DPO margin, не полное устранение вреда. У B59 EXAM Q1correct0→1, Q2correct2→1: общего прироста correct на этих двух запросах нет. Whole response не освоен.

«В28раз» в58 — отношение **модулей двух отрицательных конечных margin**, а не28-кратное уменьшение всей потери относительно+0.8356108. A/B58 меняет одновременно долю stale и расписание. Сравнение58с59 поддерживает гипотезу порядка при одинаковом10+10составе, но это межпрогонное сравнение: общего номера fixture мало, нужны совпадения стартовых bytes/optimizer/reference/seed. Поэтому не добавлять в product универсальный scheduler «три урока ремонта» или «чередовать1:1».

Математическая поддержка выбранной проекции: native SFT посылает сигнал уменьшить loss всей поданной next-token последовательности `Prompt + LF + Response`. При `Response=Rejected` этот сигнал обучает отвергнутому тексту и может противодействовать удержанию правильного ответа. Это не гарантия монотонного изменения каждой response probability или no-LF margin после конечного шага AdamW: меняются контекст, общие параметры, моменты; действуют clipping и weight decay. Фактический знак и величина здесь подтверждены конкретными опытами. Обновления разных labels в общем случае не коммутируют, но из этого не следуют универсальный LR, порядок или достаточность трёх шагов. Точные различия loss/mean margin/reward и связь с формулой книги приведены в §11.

Изменение плана: существующий projection выполняется **до tokenization/SFT mutation** и только для точного однозначного совпадения admitted pair. Новая constant-free проверка в том же `NativeCollision_NewEpoch_RetainsAcquiredTokens` требует приобретённого before, ненулевого actual update и отчёта по correct/margin/NLL вместе; не превращать значение13 или22/23after в универсальный oracle всех доменов. Существующие contract tests проверяют замену первого stale ingress и повторное восстановление candidate без второго mutation; готовые dose artifacts используются для объяснения, а не для повторного обучения ради отчёта.

<a id="sep133-collision-boundary"></a>

## 9. Уточнение по SEP01/133: профиль измерения не сокращается автоматически

[Новый FIX-SEP / 62](62-supervised-framing-and-learning-readout.md) имеет отдельную положительную ось: на тех же acquired weights Abs 22/23 и Sign 20/21 в no-LF панели становятся 23/23 и 21/21 при чтении в точном SFT-формате с LF. Этот результат объясняет две ложные ошибки, но не переписывает сырые результаты collision и не подменяет исправление устаревшего Response из этого плана.

У будущего collision report отдельно указывать profile каждого correct/NLL/margin: `sft-joint-lf-v1` для усвоения SFT и `prompt-response-segments-no-separator-v2` для действующего exact DPO. Сравнение A/B допустимо при одинаковых target IDs и одном профиле; перенос numeric margin из одного контекста в другой не доказан. Одинаковое добавление LF в chosen/rejected не гарантирует общей аддитивной константы: attention/hidden state и условные вероятности зависят от контекста и состояния модели. Подробное математическое основание — [62§4](62-supervised-framing-and-learning-readout.md#4-почему-это-математически-правильно).

Историческая фраза отчёта133 о сокращении format gap при сравнении состояний не используется как доказательство устойчивости всех collision margins. Значения сохранённых опытов остаются фактами их no-separator профиля; знак и величина в LF-профиле не измерены. Нового универсального ранжирования доз, LR или результата генерации из SEP не выводить.

Изменение плана ограничено oracle/provenance: SFT acquisition predicate должен читать фактически обученную последовательность; native DPO framing, admission, masks, objective и digest остаются прежними. SEP не доказывает экономию учебных шагов и не назначает повтор всей collision-нити. Число учебных обновлений 17 сохраняется; прежние 36 диагностических Forward дополняются шестью LF-чтениями, итого 42, как явно назначено в §4/§11. Теперь опубликованный [59-collision-repair-transfer](59-collision-repair-transfer.md) прочитан на f0939c5: он назначает отдельное восстановление из повреждённого CDG18 donor, A0/B10SFT и собственный расход. Здесь17 сохраняет CDB01 и projection входящего устаревшего текста; оба назначения не дублируются и их бюджеты не складываются.

<a id="cdg134-schedule-boundary"></a>

## 10. Поздний CDG58/134: локальный выигрыш расписания не задаёт интерполяционный oracle

Перед публикацией ветка продвинулась до `726b33aea95325781083668bdc73687febbd29e4`. Прочитаны [отчёт134](https://github.com/lubochka/llm_mvp_core/blob/726b33aea95325781083668bdc73687febbd29e4/experiments-v2/RP01-MICRO/CDG58/results-01/report.md) и [raw](https://github.com/lubochka/llm_mvp_core/blob/726b33aea95325781083668bdc73687febbd29e4/experiments-v2/RP01-MICRO/CDG58/results-01/n63c-probe.json). Это новая граница существующего collision-плана, а не новый FIX или разрешение повторить нить.

Фактор точен: при полной скорости `0.003` до окна остаётся 110 уроков. A: `85 Negate + 10 Magnitude + 15 Negate`; B: `75 Negate + 10 Magnitude + 25 Negate`; далее у обеих рук 60 уроков окна и 60 ремонта. Итого по 230 SFT updates на руку. Меняется положение близкой практики при фиксированном объёме; нельзя называть это независимым добавлением десяти новых уроков или экспериментом с уменьшением LR.

| Строка | Mean-logP margin A → B | Chosen NLL A → B, меньше лучше | Correct A → B |
|---|---:|---:|---:|
| Abs | 0.34335077 → 0.40394825 | 0.5370036 → 0.5536756 | 22/23 → 22/23 |
| Magnitude | 0.15477633 → 0.14217019 | 3.678684 → 3.9590194 | 13/29 → 7/29 |
| Sign | 0.08171713 → −0.02637899 | 1.0050653 → 0.93640983 | 20/21 → 17/21 |
| Negate | 0.379506 → 0.6797285 | 1.3517736 → 1.5474827 | 15/17 → 14/17 |

B имеет полезные улучшения Sign NLL и Abs/Negate margin. По правилу пользователя это успех именно этих осей в данном профиле. Одновременно ухудшились Sign margin, три token-count результата и NLL трёх остальных строк; универсального лучшего расписания нет. Более низкая NLL при меньшем числе top-1 совпадений математически возможна: NLL оценивает вероятность gold, а correct — его место относительно всех конкурентов.

Ресурсные показания SFT A/B: wall `1 264 276.9843 → 1 153 257.2535 ms`, allocated `405 366 301 112 → 405 366 230 216 B`. DPO wall `15 110.7526 → 8 205.2999 ms`, allocated `936 883 720 → 936 801 368 B`. Это один упорядоченный прогон, причинное ускорение не доказано; CPU отдельной осью не измерен. Всего raw учитывает 60 acquisition + 460 arm SFT updates, 2 DPO updates, 3734 scoring Forward, 0 Generate/external teacher. Эти уже состоявшиеся расходы не превращаются в новый budget плана17.

Конечный Sign margin при gap15 равен `+0.08171713`, при gap25 — `−0.02637899`; они не подтверждают прежний прогноз линейной промежуточной формы, а добавленные точки нарушают предполагаемую монотонность. **Будущий тест не выводит expected, минимальную дозу или выбранный gap линейной интерполяцией по 10/20/30/40.** Он использует только свой назначенный измеренный режим и сохранённое состояние. Нельзя переносить формулировки отчёта «интерполяция опровергнута как метод» или «не функция длины» буквально: ограниченные точки опровергают конкретные линейную/монотонную гипотезы, а не всю математику интерполяции и не существование зависимости при фиксированных прочих условиях.

Все четыре строки участвуют в расписании, что видно в [выборе lessonIds и настоящем SFT-вызове](https://github.com/lubochka/llm_mvp_core/blob/726b33aea95325781083668bdc73687febbd29e4/experiments-v2/RP01-MICRO/CDG58/host/CollisionRepairFullSpeedGapSmoothnessProbe.cs#L936). Унаследованная метка `heldout` не делает эти чтения независимым EXAM. Все числа остаются teacher-forced/no-separator показаниями, с границей профиля из §9. Гипотеза особого поведения длин, кратных пяти/десяти, остаётся гипотезой; новый sweep, переобучение и перенос schedule как универсального алгоритма не назначены. Тема теперь опубликованного59 уже занята, поэтому отдельный дубликат плана не создаётся. STOP/NEXT сохраняются; общий действующий бюджет плана17 уточнён в §4/§11: прежние учебные шаги и42диагностических Forward. Бюджет самого исторического134 не изменяется.

<a id="n63c-fixed-gold-readout"></a>
## 11. Общие веса с SEP, неизменная цель удержания и точные показатели

### Доказательство общего состояния

На срезе `7e79eb156f418dc76d699966ca948d4616b36cdd` [CDB01 donor](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/experiments-v2/RP01-MICRO/CDB01/results-01/authority-acquired.bin) и [SEP Abs donor](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/experiments-v2/RP01-MICRO/SEP01/results-01/producer-abs.bin) имеют один pointer blob `bdac5a6faced90cb8dc35618f737a3e69db955af`, payload SHA-256 `59810def905c4ead2f22985c5e1d525f59feb45590256eca8b492a293206d44f` и размер 47 899 519 bytes. [SEP raw](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/experiments-v2/RP01-MICRO/SEP01/results-01/separator-format.json) связывает этот payload с no-LF 22/23 и LF 23/23, совпадающими response target IDs и сохранённой границей. [CDB01 raw](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/experiments-v2/RP01-MICRO/CDB01/results-01/n63c-probe.json) подтверждает исходные 60 updates и loss 0.00084102585. Числа acquisition — история, не назначение нового producer.

Это доказательство назначенной identity по pointer и raw. В этой документной сессии payload в модель не загружался. Будущий loader сверяет реальные bytes, config SHA-256 `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`, tokenizer identity `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`, topology, 55 named parameters и точные IDs. Совпадения 22/23 без identity недостаточно. Reference остаётся B0; одинаковые policy weights не дают права заменить его policy-state или восстановить отсутствующие optimizer moments.

### Один правильный ответ во всех состояниях

В будущем `Tests/DpoSftCollisionLearningTests.cs` создать один immutable retention probe до воздействий: prompt `// file: Abs.cs`, gold `static int Abs(int n)=>n<0?-n:n;`, профиль `sft-joint-lf-v1`, full length 35, prefix 12, response targets 23. Использовать запланированный `SupervisedResponseReadout` владельца [62](62-supervised-framing-and-learning-readout.md); его готовность — предпосылка учебной части 17. Второй скрытый reader не писать. Отсутствие helper или payload не заменяется новым acquisition.

`sequence_origin=acquired_gold`, `readout_role=retention_gold`: это последовательность приобретённого правильного ответа, а не input последнего stale-урока. Сам `Prepare` по-прежнему проверяет соответствие переданных IDs указанным prompt/gold/profile. Во всех шести LF-чтениях сохранить одни `probe_digest`, gold text digest и target IDs; target-mask равна 11…33 на shifted logits. Перед первой оценкой сравнить их с исходными SEP targets. В A фактически поданные SFT IDs содержат Rejected, в B — исправленный Chosen; они сохраняются отдельно как `submitted_training_material`, не подменяя gold.

**Named check `N63C_RETENTION_GOLD_IS_IMMUTABLE`:** WRONG — подставить `arm.sftResponse` в scorer и назвать выученный Rejected сохранённым навыком. CORRECT — обе руки/все стадии сравнивать с одним исходным T1.Chosen, а доставку A/B проверять отдельным wiring assertion. Проверка digest/IDs выполняется в том же fixture без дополнительного Forward. Уже назначенные 40 SFT-шагов, projection и порядок DPO→SFT остаются прежними.

| Чтение | Назначенные стадии и строки | Диагностических Forward | Что подтверждает |
|---|---|---:|---|
| Исходная no-LF панель | A/B × before/after-DPO/after-SFT × T1/Q1/Q2 × chosen/rejected | 36 | Сопоставимость исторических correct/NLL/mean margin в одном профиле |
| SFT/LF удержание правильного T1 | A/B × before/after-DPO/after-SFT × только неизменный T1.Chosen | 6 | Сохранён ли приобретённый правильный ответ в его учебном формате |
| Сверка receipt, gold, IDs и snapshots | Уже полученные результаты | 0 | Целостность сравнения; не новое обучение |
| **Всего diagnostic** | Один будущий shared fixture | **42** | Не включает training Forward/Backward |

42 — отдельный бюджет N63C, а не удвоение 6-Forward read-only fixture SEP. Не запускать весь SEP Quality автоматически внутри каждого collision test. Исторические результаты других опытов не пересчитываются, Q1/Q2 остаются прежними; перенос на перевод и другие темы не заявлен.

### Mean margin, DPO reward и учебная loss — разные величины

[N63CollisionProbe.Panel/SideScore:413–474](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L413) считает

\[
m_{\mathrm{panel}}=\frac{\log p_\theta(y^+\mid x,s_\mathrm{DPO})}{T_+}-\frac{\log p_\theta(y^-\mid x,s_\mathrm{DPO})}{T_-}.
\]

В receipt no-LF панели это `score_aggregation=mean_response_log_probability` и оба знаменателя `chosen_target_count/rejected_target_count`. NLL панели равна минусу chosen mean. LF-проверка читает только Chosen: её `rejected_target_count` и margin — `null` с причиной `not_measured_by_design`, а не нулевые результаты. Дополнительный rejected Forward для LF не назначается. Native exact DPO использует суммы response logP, β и frozen-reference correction; его реально возвращённые loss/reward записываются в отдельный `native_dpo_step` и не вычисляются из mean margin. Существующий [EvaluateMany_ConsumesResponseSumWhilePublicAverageRemainsLegacyMean](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/Tests/DpoExactPairMaterialTests.cs#L206) сохраняет этот контракт; править expected на average ради согласования отчёта нельзя. Это source reference, не добавленный запуск теста.

[Trainer.TrainSupervisedNextTokenStep:102–145](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/04.TrainingLoop/Trainer.cs#L102) минимизирует full-sequence loss, при `z=Encode(prompt+LF+response)`:

\[
L_{\mathrm{SFT}}=-\frac{w}{N-1}\sum_{i=0}^{N-2}\log p_\theta(z_{i+1}\mid z_{\le i}).
\]

Формула softmax-cross-entropy `δ=z−t` из [Theorem 9.5, печатная стр. 282 / PDF 318](https://github.com/lubochka/xiigen-planning/blob/8ee6745378e9b8213ad9c58a302bd11a6b268277/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#theorem-9-5) объясняет зависимость сигнала от поданной метки. Книга рассматривает MLM-head; это не доказательство универсальной динамики нашего autoregressive AdamW. Собственный локальный вывод: `Δm_panel ≈ ∇m_panel · Δθ`, поэтому без связи этих направлений знак изменения другой метрики не установлен. Устаревшие labels — обоснованный механизм конфликта; величина выигрыша и побочные потери берутся из опытов, не из обещания монотонности.

### Перестройка проверок и результат для пользователя

Если тест после правильного LF-чтения по-прежнему требует 22/23, expected 22/23 переносится в именованный no-LF control; квалификация исходного LF-state ожидает 23/23 при подтверждённой identity. After-LF значения ещё не измерены: их не заменять 23/23 по желанию. Если старый тест оценивает `sftResponse` A, заменить только oracle на immutable gold; не менять training input A, не убирать отрицательный контроль и не понижать качество.

Отчёт остаётся русским `artifacts/fbe/FIX-N63C/attempt-0001/report.md` с `learning.json`: две отдельные таблицы профилей, неизменный gold, historical/new-native status, native DPO step и full-sequence training loss отдельно, 42 diagnostic Forward и фактические training calls. Любое полезное измеренное улучшение учитывается по своей оси; остальные ухудшения видны рядом. Тесты сравнивают приобретённый ненулевой навык, не произвольный format check с 0→0.

Дополнение к промпту §6: выполнить только назначенный FIX-N63C и готовый shared reader 62; восстановить закреплённый CDB01 payload; не повторять acquisition; A/B всегда оценивать против T1.Chosen; сохранить 36 no-LF и ровно 6 LF-чтений. Не считать mean margin native DPO reward. Модель исполнителя — текущая default; выбор и оценка DPO — CODEX с `astra 6 ultracode`, создание пар — внешняя открытая китайская модель; новых таких вызовов здесь нет. STOP/NEXT немедленны; старые результаты сохраняются; живой процесс ждать по 03 в том же ходе, не relaunch. После одного назначенного ID — END; следующий ID программы брать только из реестра.
