# FIX-MS — явная маска response targets для ограниченного SFT-профиля

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q43](01-registry.md#work-fix-ms). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-MS: Не допускать общий перенос.** Вошёл по одному margin и малой экономии, но существенная потеря других оценок была скрыта. Не повышать статус за математическое предпочтение маски.

- Точность: Учебные ответы 33/33 в обеих руках; экзаменационных предпочтений 3/4 → 3/4.
- Полезный результат: D4 margin −0,198821 → −0,195105 (ближе к правильному направлению на 0,003716, но ответ всё ещё неверен). Выделения 1 163 482 488 → 1 162 231 592 Б (−0,1075%); два урока 6 047,2759 → 5 707,6068 мс (−5,62%).
- Цена и ограничения: D1 margin 0,02803 → 0,00697 (−75,13%), D2 и D3 хуже; NLL первого учебного ответа примерно 0,000156 → 0,000159, второго 0,000160 → 0,000164. Пик 831 016 960 → 848 408 576 Б (+2,09%).
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-ms).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски LEARN:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

От замороженного tokenizer/framing до model/trainer передаются фактически полученные IDs, masks и границы; сохранённый tensor служит oracle, а не заменой выхода проверяемого звена. Partial-path/component PASS не закрывает learned consumer. Evidence-kind и реальные calls фиксируются отдельно от структурных нулей; без расширения назначенного учебного бюджета.

Для FIX-MS исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Разделить точные bytes prompt/response, позиции readout и фактическую функцию loss. Сохранённый навык может диагностироваться тем же reader, но response-position scoring не означает response-only SFT. Контрпример P08/P09: native full-sequence TrainSupervisedNextTokenStep выполнялся при ошибочной фразе exact response masks. Не менять исходную учебную цель ради согласования подписи; сохранять обучение, состояние и отдельный исправляющий receipt.

Применимость: FIX-MS. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Зафиксировать именно функцию учебной потери и назначенного trainable recipient; response-position reader не означает response-only loss. Контрпример P11: full-sequence SFT и единственный embedding recipient; строка «те же q/v keys» была несовместима с PRIMARY. До/после требовать тот же набор уникальных RowId, Kind и gold; равный Count не защищает от потерянной строки.

Применимость: FIX-MS. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-79-followup -->
## Уточнение после последующих исправлений и P03–P05

Перед before/after маски SFT зафиксировать framing, response targets и правило завершения. Donor skill квалифицируется тем же reader; добавление EOS или иной длины вывода — явная новая постановка с обучением этого завершения. Контроль: raw '+x…' не равен полному '+', но не доказывает потерю прежнего1-token навыка. SFT decay/clip не наследуются из DPO default.

Основания — [79](79-followup-transfers-and-learning-audit-2026-10-27.md), адресный план — [80](80-followup-repair-plan-2026-10-27.md). Это изменение требований и статуса по прочитанным источникам; неприменённые проверки не объявляются пройденными. Собственные бюджеты и исторические результаты карточки сохраняются.
<!-- END audit-79-followup -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-MS

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Оракул маски и предел улучшения, 27 октября 2026.** На уже назначенных masked gradient данных обязательна независимая проверка делителя124, а не33: замена нормировки может улучшить отдельную margin и всё же нарушает измеряемый фактор. Common A/B измеряют одинаковые native окна; дополнительное перечисление adapters или SaveCheckpoint только в одном плече нельзя приписывать цене mask. Полный checkpoint, moments и typed RNG сравниваются как state соответствующей руки, не обязаны быть равны A/B при намеренно различном objective; ожидаемая неизменность касается frozen base/reference и корректного continuation contract. Signed negative margin не переводить в произвольный относительный «процент обучения». D4 остаётся неверным, ухудшения TRAIN NLL/D1–D3/peak из §1 сохраняют ограниченный opt-in статус. Это уточнение assertions прежнего бюджета, без дополнительного обучения.

Статус: **план локального кандидата с существенными регрессиями; default не меняется, внедрение не назначено**. Дополнение от 2026-10-21 UTC. Основание — `E-SFT-RESPONSE-MASK` / MS01, `attempt-0001`, source `4e993a33c9396f155a4aaeb42170e0251f34aa77`. Прежний пакет `8ea3e78c039971288b12ecf2657c8e9f6804e98a` и исходные отчёты сохраняются.

Обязательны [реестр](01-registry.md), [fixture/state](02-learning-state-and-fixtures.md), [тесты, skills, ожидание, STOP/NEXT](03-test-and-execution-contract.md). Следующий ID — только по [актуальному реестру01](01-registry.md), если пользователь не назначил иной; номера и старые указатели не задают отдельной очереди. Ни эта очередь, ни положительное частное наблюдение не разрешают включить маску глобально.

## 1. Почему кандидат допущен при отрицательном общем результате

Тема — документация кода; профиль — английское описание порядка вычитания аргументов C#-метода. Получатель — q/v LoRA `rp01-csharp-small/v1`, H512. Реальный SFT train step получает тот же prompt+LF+response; фактор B выключает **прямой loss response-независимых targets prompt/LF**, сохраняя весь prompt как причинный контекст. На обученном старте найдено небольшое улучшение D4 margin и меньше allocations, хотя другие полезные показатели ухудшились.

[Исходный report](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/report.md) и [candidate](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/candidate.md) называют результат отрицательным без выигрыша по осям. Их не переписываем: по текущему пользовательскому правилу любой измеренный полезный gain учитывается отдельно. **Новая интерпретация raw**, а не новое измерение: локальные D4 и allocation gain существуют, причинная чистота ресурсного эффекта не установлена, общего улучшения нет.

Первичные [pair-summary](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/pair-summary.json), [A metrics](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/A/metrics.json), [B metrics](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/B/metrics.json), [A0 EXAM](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/A/evaluation/A0/evaluation-receipt.json), [A1](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/A/evaluation/A1/evaluation-receipt.json), [B1](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/B/evaluation/B1/evaluation-receipt.json).

| Измерение | Общее обученное «до» | A: все shifted targets | B: response mask | Вывод |
|---|---:|---:|---:|---|
| D4 normalizedMargin | −0.2006387710571289 | −0.1988210678100586 | −0.19510507583618164 | B лучше своего старта на0.005533695220947266, A на0.003715991973876953; **знак неверен** |
| D1 margin | 0.0202293395996 | 0.0280327796936 | 0.0069723129272 | **B хуже A и BEFORE** |
| D2 margin | 0.2722663879395 | 0.2751965522766 | 0.2715287208557 | **B хуже A и BEFORE** |
| D3 margin | 0.1547441482544 | 0.1632280349731 | 0.1408348083496 | **B хуже A и BEFORE** |
| EXAM правильные знаки | 3/4 | 3/4 | 3/4 | Рост accuracy не показан и для локального gain не требуется |
| TRAIN D-T1 response NLL | 0.00015603148 | 0.0001555981 | 0.00015854169 | **B хуже A и BEFORE** |
| TRAIN D-T2 response NLL | 0.00016043046 | 0.00015968285 | 0.00016411449 | **B хуже A и BEFORE** |
| TRAIN correct, каждая строка | 33/33 | 33/33 | 33/33 | Whole response exact; исходный навык ненулевой |
| Allocations | — | 1163482488 байт | 1162231592 байт | −1250896, около0.1075%; scope ниже |
| Wall time | — | 6047.2759 мс | 5707.6068 мс | −339.6691 мс; один A/B |
| Peak working set | — | 831016960 байт | 848408576 байт | **+17391616 байт** |

Свободная генерация остаётся бессвязной. `3/4` — знаки preference score, не3правильных свободных ответа. Qualified TRAIN33/33 не является запрещённым0→0; отдельный D4 gain не выдаётся за усвоенный D4 ответ. Объективные train losses A≈.0061 и B≈.000042 считают разные цели, поэтому их разница не доказывает качество.

Стоимость в [MS01ConsumerBridge:244–277](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/35.ExperimentsV2/Experiments/Rp01Micro/MS01/Common/ContinuousLearning/MS01ConsumerBridge.cs#L244-L277) охватывает **два Execute и промежуточный SaveCheckpointToStream+flush после первого урока**. A дополнительно перечисляет adapters в bridge, B вызывает ExecuteMasked другим путём. Поэтому −0.1075% не объявляется причинным memory-выигрышем чистой маски. Один фиксированный A/B не устанавливает стабильное ускорение; source receipt сохраняет эту неопределённость.

## 2. Точный product recipient и выбранный способ переноса

Native код сейчас обучает все shifted targets: [Trainer.TrainSupervisedNextTokenStep:67–145](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/04.TrainingLoop/Trainer.cs#L67-L145). Тот же метод вызывается из [ProductionSupervisedTrainingStepExecutor](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs), а [IncrementalTrainer:385–418](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/14.ContinuousLearning/Training/IncrementalTrainer.cs#L385-L418) строит tokens из prompt+LF+response и [658–663](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/14.ContinuousLearning/Training/IncrementalTrainer.cs#L658-L663) отправляет их learner. Экспериментальный [Trainer masked overload:455–628](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/35.ExperimentsV2/Experiments/Rp01Micro/MS01/B/Trainer.cs#L455-L628) делает настоящие Forward/Backward/AdamW; не просто измеряет маску.

| Точное будущее изменение | Решение |
|---|---|
| `04.TrainingLoop/Trainer.cs` | Добавить masked overload, сохранив старый метод full-target. Общие guards, clipping, finite checks, очистка gradients и session counters остаются |
| Новый `14.ContinuousLearning/Training/SupervisedTargetMaterial.cs` | Typed immutable material: IDs, shifted0/1mask, source byte boundary, tokenizer identity, mode, digest. Builder получает существующие prompt/response, не EXAM/gold |
| `IncrementalTrainer` token preparation385–418 | Для явно назначенного response mode построить/проверить точные spans до BeginMutation; default tokenization/all-target материал остаётся прежним |
| `SupervisedTrainingExecution.cs` | Добавить `ExecuteMasked` с явным mask. Production implementation вызывает native overload; unsupported test/consumer implementation обязана fail, не silently Execute all-target |
| `ContinuousLearningModelServerComposition.cs::CreateRepositoryStartRequest:3199–3237`, `ContinuousLearningDeferredCompositionPlan.ResolveRepositoryStartRequest:2652–2672`; session и Execute658 | Новый typed `SupervisedTargetMode` = `AllTargets` по умолчанию — additive init property существующего `RepositoryOwnedIncrementalTrainingSessionStartRequest` без изменения7/8-argument compatibility. `ResponseTargets` задаётся exact server-owned profile в названном builder, проходит существующий domain-checked resolver и хранится immutable в session. Не HTTP prompt field, не вывод из имени метода/keyword/EXAM |
| `IncrementalTrainer.BuildTrainingExampleBatchSha256` и durable candidate identity | Response mode/material digest связываются с transactional input до update: retry не считает different objective тем же принятым уроком. Старый AllTargets identity сохраняется, response mode получает versioned identity |

Поскольку дефолт остаётся AllTargets, старая семантика всех callers сохраняется. Для внутреннего `ISupervisedTrainingStepExecutor` новый `SupportsTargetMode(SupervisedTargetMode mode)` по умолчанию допускает только AllTargets, production executor явно поддерживает оба режима. `IncrementalTrainer` проверяет capability выбранного session mode **до BeginMutation, adapter attach и любого DPO/SFT Execute**; false даёт typed unsupported-mode error с0updates. `ExecuteMasked` дополнительно имеет default fail implementation с тем же явным error; существующие fakes не получают автоматически маскированный capability. Новые masked tests используют production executor. Не подменять интерфейс двумя внешне одинаковыми вызовами с разным скрытым normalization.

Математика закрыта заранее: при T tokens shifted targets имеют N=T−1; loss = `weight * sum(mask[i] * CE[i]) / N`; прямой gradient строки logits = `weight * mask[i] * (softmax−oneHot) / N`. В measured fixture **N=124, active mask count=33**. Деление на33 вместо124 — другой фактор, этим планом не разрешён. Значения mask только0/1, length=N, хотя бы одна1; invalid/empty/nonfinite mask отвергается до Forward. Общий shape, prompt attention и causal positions не сокращаются.

Prompt/LF target rows имеют нулевой **прямой** loss-gradient. Это не запрет gradients prompt embeddings/attention: prompt остаётся контекстом response и может получать обратный сигнал от response. Нельзя написать тест «все prompt gradients равны0».

Byte boundary определяется как UTF-8 bytes(prompt)+один LF. Token spans восстанавливаются из точных bytes tokenizer и сверяются с полным carrier. Shifted mask[i] относится к token[i+1]. Token целиком после boundary получает1; prompt/LF0. Token, пересекающий boundary, не округляется и не переобозначается: typed `SUPERVISED_RESPONSE_BOUNDARY_UNREPRESENTABLE` до обучения; default AllTargets при этом не меняется. Автоматический переход на иной tokenizer/framing запрещён.

## 3. Состояние, набор и границы применения

Источник `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-joint/v1/`, [bundle](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-joint/v1/bundle.json), [authority](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-joint/v1/authority.json). Это тот же подготовленный donor, что у FIX-MC, но **маска является самостоятельным фактором**. Обе руки MS восстанавливают optimizer1000 и проходят два урока до1002 без reset boundary. Выполнение native masked A/B не зависит от внедрения MC: test loader законно читает existing portable state непосредственно. Production continuation из FIX-MC подключается только если отдельно выбран; эта карточка не даёт скрытого разрешения менять publisher/recovery.

| Файл | Bytes | SHA256 |
|---|---:|---|
| S0.weights.bin | 47899519 | `99325ae2d693e1b73c3bcf653b5f5744117b2bd30e405211b27fe06b6a3c04c1` |
| S0.optimizer.bin | 426788 | `7e3f3fa2b98259325725110bc901ff82f5acb634cbb422b74d72f2cad72cbda2` |
| S0.rng.json | 1573 | `f819248e615f50e41c22db16d5fac5116809857d195d3b9d149c9b6ec580ba9b` |
| data.source.jsonl | 610 | `efedf046d90e69b3eda5f5200c412ae200a28917396147683b94278c89d58573` |
| data.material.json | 9140 | `af2ec74db4323d98033811583fdf1cbd9438f2cb2844f167aec2dc5675097204` |

Tokenizer file SHA256 `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`; source-manifest SHA256 файла `generation.json` — `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`. Внутри файла `runtimeCompatibility.configSha256` — `8cf38d269f806356191c614ccbf3bcafdb3c7e339f02e9d77bdade4e9fcd68ae`; для compatibility используется именно он. Exact source file и runtime identity раздельно описаны в [02](02-learning-state-and-fixtures.md); source-declared file hash не объявляется проверенным по нормализованной локальной копии. Model V629/C2048/H512/L4/Q8/KV2/FF1408, q/v rank8/alpha16/dropout0, seed20261014. Constant LR.001,clip1,AdamW beta1.9,beta2.95,weightDecay0,epsilon1e−8. Это фиксированный research recipe, не набор новых product defaults. Private research authority не является canonical DPO parent/admission/lease.

Будущий passport `Tests/Fixtures/ExperimentFixes/v1/Ms01/manifest.json`; TRAIN D-T1→D-T2 из exact data.source, отдельный EXAM D1–D4 из [bridge Rows/Exam](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/35.ExperimentsV2/Experiments/Rp01Micro/MS01/Common/ContinuousLearning/MS01ConsumerBridge.cs#L60-L81), source bytes и hashes в паспорте. TRAIN125tokens, boundary148UTF-8 bytes,33response targets. Не смешивать темы, не добавлять balanced batch, не подбирать другое S0 ради большего эффекта. Небольшое улучшение D4 margin не оправдывает runtime selector по `Diff`, `right-left` или gold D4.

Missing/incompatible trained bytes, LFS pointer или квалификация не33/33 → `missing_or_incompatible_fixture`; указать путь/поле и не начинать producer1000steps. Соседний h512-pad-segmented используется только FIX-PD. Два набора различны по framing и response; их нельзя взаимозаменять для удобства.

Совместимость с отдельно выбранным FIX-MC: будущий SFT-only v3 bundle связывает target mode/framing. `ResumeSft` не меняет `AllTargets→ResponseTargets` с прежними moments; смена режима требует явного `NewSftEpoch`. MS A/B использует восстановленный research optimizer как фиксированный экспериментальный фактор и не доказывает комбинированный production resume. Реализация MC не является зависимостью этой карточки, нового совместного опыта здесь нет.

## 4. Перестройка тестов без подгонки результата

Новых падений этой сессией не установлено. После назначения сначала existing baseline, затем корректировка только причинно затронутых ожиданий.

| Существующий тест/implementation | Как сохранить или расширить |
|---|---|
| `TrainerTests.SupervisedSteps_DoNotAdvancePretrainingCounterOrScheduler` | Старый AllTargets остаётся; добавить masked аналог с тем же разделением counters, не переписать existing expected |
| `RetainedCheckpointBackedIncrementalTrainingSessionFactoryTests.RetainedParent_LoadsAfterAttachmentBeforeAnyTrainingSideEffect` | Загрузка до любого masked Execute остаётся обязательной |
| `IncrementalTrainerDpoWiringTests.NonAdmittedPairs_FailBeforeSessionExecutorOrFilesystemMutation` | Response mode не открывает обход DPO admission/transaction |
| `MutatingSupervisedStepExecutor` / `NonFiniteSupervisedStepExecutor` в том же тестовом файле | Legacy Execute не менять; отдельно проверить unsupported masked-mode fail до обучения. Не считать fake новым learning evidence |

Новые планируемые файлы: `Tests/ExperimentFixes/SftResponseMaskLearningTests.cs`, `SftResponseMaskContractTests.cs`. Главный `NativeTwoLessonPair_ReportsEveryMeasuredAxis` — **один** native A/B receipt с обоими готовыми S0. A вызывает unchanged all-target, B новый native masked overload. Обоим передать один раз подготовленный одинаковый adapter list; checkpoint после первого урока выполняется у обоих. Записать, что такое выравнивание harness устраняет известную асимметрию enumeration и потому новый allocation delta не является точным повтором исторической стоимости.

Главный тест требует квалифицированное before33/33 по обеим строкам; native4updates на пару; одинаковое начальное состояние и1000→1002; full-target default без изменения; сохранившийся ненулевой TRAIN after; measured response NLL и **каждый** D1–D4 до/после обоих плеч; base frozen, adapter update есть; оценка без обучения. Он не требует общего улучшения или верного D4 знака, которого исходный опыт не дал. Для D4 отдельно сравнить B с BEFORE и A и записать actual delta; численные reference comparisons `abs(a-b)<=1e-6+1e-4*max(abs(a),abs(b))`, рядом actual знак. Если локальный gain не воспроизводится, честно отметить отсутствие воспроизведения, не подбирать seed/steps/набор. Проверка корректной реализации маски и вывод о пользе — разные поля receipt.

Не ставить flaky invariant `allocations_B<=allocations_A` из-за0.1075% historical observation. Записывать allocations/wall/peak с точным scope и все ухудшения. Один test process с независимыми model objects не воспроизводит historical process peak. Генерации16 с теми же settings (max64tokens,temperature0,topK0,topP1,seed17,max160chars) сохраняются как raw без заявления о правильных ответах;32preference score calls не скрывать в TRAIN readout.

Tiny companions: H8/L1/Q2/KV1/FF16/V256/C8,rank2,dropout0,seed17,LR.001,weightDecay0,clip1. Ровно3SFT updates/3Forward/3Backward: два независимых экземпляра на token IDs `[1,2,3,4]` сравнивают AllTargets с all-one mask `[1,1,1]`; третий экземпляр делает один masked update с `[0,0,1]`. Pure tensor fixtures независимо вычисляют gradient logits и denominator для124targets/33ones,0дополнительных model forwards. Invalid mask/zero-mask/token-boundary crossing и unsupported executor проверяются без Train. Boundary contract получает фиксированные byte spans, включая один пересекающий boundary token; не обучать новый tokenizer для генерации контрольного случая. Квалификация пользы берётся только из H512 A/B, не из этих3tinyupdates.

## 5. Команды, пределы и готовый промпт

Все команды будущие, после выбора FIX-MS, из `C:/Projects/llm_mvp_core`, Release/net9.0:

```powershell
Set-Location C:/Projects/llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~TrainerTests.SupervisedSteps_DoNotAdvancePretrainingCounterOrScheduler|FullyQualifiedName~RetainedCheckpointBackedIncrementalTrainingSessionFactoryTests.RetainedParent_LoadsAfterAttachmentBeforeAnyTrainingSideEffect|FullyQualifiedName~IncrementalTrainerDpoWiringTests.NonAdmittedPairs_FailBeforeSessionExecutorOrFilesystemMutation" --logger "trx;LogFileName=baseline.trx" --results-directory artifacts/fbe/FIX-MS/attempt-0001/baseline
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~SftResponseMaskContractTests" --logger "trx;LogFileName=contracts.trx" --results-directory artifacts/fbe/FIX-MS/attempt-0001/tests/contracts
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~SftResponseMaskLearningTests.NativeTwoLessonPair_ReportsEveryMeasuredAxis" --logger "trx;LogFileName=learning.trx" --results-directory artifacts/fbe/FIX-MS/attempt-0001/tests/learning
```

После изменения один раз повторить existing filter в `tests/existing`, logger `existing.trx`. Новые классы сначала реализовать;0discovered/Skip не PASS. Сохранить actual failures и независимые oracles, не ослаблять assertions ради «успеха кандидата».

Главный бюджет:2SFT updates/2training Forward/2Backward/4TRAIN diagnostic Forward/2evaluation states на плечо; пара **4updates,4training Forward,4Backward,8TRAIN diagnostic Forward,32preference score calls,16Generate**,0producer,0external calls. С новыми tiny companions7updates/7training Forward/7Backward; existing baseline/regression расход отдельно по своим маленьким fixtures. Один `artifacts/fbe/FIX-MS/attempt-0001/resources/pair.json`; русский `report.md` по03. Предел главного запуска15минут; нельзя увеличиватьsteps или повторять consumed A/B, чтобы превратить слабый эффект в сильный. Runtime timeout не означает, что запуск не состоялся: сначала terminal evidence исходного handle.

**Промпт исполнителю.** Полный действующий контекст: карточка C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/23-sft-response-mask.md; свежее полное чтение AGENTS/CLAUDE/skills — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1); роли — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2); применимость и разбор падений — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткий список ниже дополнительный. Выполняется только прямо назначенный ID/этап; документальное поручение не разрешает запуск. PRIMARY, собственные примеры, данные, команды и бюджет полной карточки сохраняются. Работай фактической моделью сессии по умолчанию, без override; прочитай AGENTS/CLAUDE, skills03, `planning-session-startup-SKILL.md`, `code-examination-SKILL.md`, `improvement-measurement-SKILL.md`. Только FIX-MS, default AllTargets неизменен. WRONG: объявить «нет ни одного gain», скрыть D1–3/NLL/peak regressions, поделить loss на33, обнулить весь prompt gradient, подбирать domain selector по EXAM, silently implement MC. CORRECT: точная response mask/124, trained donor, all-axis receipt, узкий opt-in и отдельно выраженное решение о внедрении.

Живой запуск жди штатным wait того же handle порциями≤60секунд до terminal outcome; не заканчивай ход ожиданием, не запускай вторую копию и не добавляй polling loop. **Прямой STOP/NEXT немедленно прекращает предыдущую карточку, включая её отчёт, тесты, commit и cleanup.** Цитаты старых остановок не активируют STOP. Переход выполняется по стабильному ID из01, прямое указание пользователя выше очереди.
