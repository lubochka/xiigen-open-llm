# FIX-IL — длинный SFT-урок не должен блокировать допустимые уроки

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q05](../fixes-based-on-experiments/01-registry.md#work-fix-il). Сначала завершите указанную в строке зависимость. Потом проверьте тот же короткий урок через нужного получателя. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](../fixes-based-on-experiments/03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](../fixes-based-on-experiments/01-registry.md#executor-next).

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски LEARN:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

От замороженного tokenizer/framing до model/trainer передаются фактически полученные IDs, masks и границы; сохранённый tensor служит oracle, а не заменой выхода проверяемого звена. Partial-path/component PASS не закрывает learned consumer. Evidence-kind и реальные calls фиксируются отдельно от структурных нулей; без расширения назначенного учебного бюджета.

Для FIX-IL исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](../fixes-based-on-experiments/92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Разделить точные bytes prompt/response, позиции readout и фактическую функцию loss. Сохранённый навык может диагностироваться тем же reader, но response-position scoring не означает response-only SFT. Контрпример P08/P09: native full-sequence TrainSupervisedNextTokenStep выполнялся при ошибочной фразе exact response masks. Не менять исходную учебную цель ради согласования подписи; сохранять обучение, состояние и отдельный исправляющий receipt.

Применимость: FIX-IL. Основание и адресные проверки — [план85](../fixes-based-on-experiments/85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](../fixes-based-on-experiments/03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Зафиксировать именно функцию учебной потери и назначенного trainable recipient; response-position reader не означает response-only loss. Контрпример P11: full-sequence SFT и единственный embedding recipient; строка «те же q/v keys» была несовместима с PRIMARY. До/после требовать тот же набор уникальных RowId, Kind и gold; равный Count не защищает от потерянной строки.

Применимость: FIX-IL. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](../fixes-based-on-experiments/88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-IL

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](../fixes-based-on-experiments/76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](../fixes-based-on-experiments/77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Адресный контроль полного каталога, 27 октября 2026.** Admission receipt обязан разделять 300 исходных предъявлений, 200 допущенных, 100 отложенных и число **завершённых** SFT updates. Успешный partition ещё не означает исполнение 200 уроков. В назначенном контроллере учёта проверять отказ исполнителя до первого update и после первого успешного: completed=0/1, оставшееся not_reached, terminal failure; отсутствие after не заменять нулевой точностью. Контракт счётчика проверяется через тот же управляющий helper с подставным step-callback, а не четырьмя вручную заполненными receipts. Это проверка учёта без model calls; основной native SFT бюджет §3–4 не увеличивается. Accepted parent с ResetAtAdoptionDropoutZero не переименовывается в восстановленный исторический RNG шага1000.

23.10.2026. Новый кандидат по завершённому **N63-INGRESS-LENGTH-LEARNED-01**, `N63IL/results-after-fix`, опубликованному в `c58d08bcc2`. Это план переноса, без исполнения. Старое исключение N63-INGRESS-LENGTH в прежнем реестре относилось к результату без обучения; оно не описывает новый замер. Не смешивать этот фактор с DPO FIX-SC: здесь меняется допуск **SFT**, а DPO-пар нет.

Обязательны [реестр](01-registry.md), [общий контракт](02-native-transfer-and-test-repair.md) и уже спланированный [FIX-MC, §2–3](../fixes-based-on-experiments/21-sft-optimizer-continuity.md). Последний задаёт native SupervisedOnly admission, продолжение optimizer и публикацию. FIX-IL дополняет его правилом длины; не создаёт второй механизм SFT и не разрешает автоматически реализовать весь FIX-MC. Следующий ID определяется только актуальным реестром.

## 1. Измеренное основание и границы

Первичные источники: [отчёт](../../../../experiments-v2/RP01-MICRO/N63IL/results-after-fix/report.md), [вердикт](../../../../experiments-v2/RP01-MICRO/N63IL/results-after-fix/verdict.json), [все строки](../../../../experiments-v2/RP01-MICRO/N63IL/results-after-fix/per-row.json), [счётчики](../../../../experiments-v2/RP01-MICRO/N63IL/results-after-fix/counters.json), [рука A](../../../../experiments-v2/RP01-MICRO/N63IL/results-after-fix/A/arm-result.json), [рука B](../../../../experiments-v2/RP01-MICRO/N63IL/results-after-fix/B/arm-result.json), [паспорт исходников и DLL](../../../../experiments-v2/RP01-MICRO/N63IL/results-after-fix/study.json). Первоначальный [отказ обеих рук](../../../../experiments-v2/RP01-MICRO/N63IL/results/verdict.json) сохранён отдельно и не считается учебным результатом.

Профиль — восстановление короткого тела C# по подсказке `// file: <имя>`, техническая метка `CODE_Q`. Отчёт называет тему документацией кода, однако [закреплённые ответы](../../../../experiments-v2/RP01-MICRO/N63IL/fixtures/ingress-length.json) содержат сам C#-код, а не естественно-языковую документацию. Вывод относится к этому профилю языка программирования, не доказывает качество написания документации. Получатель — Transformer H16 с q/v LoRA; оркестратор и kNN здесь не обучаются.

| Показатель | A: весь список | B: отложить слишком длинное |
|---|---:|---:|
| Запланировано / допущено / отложено | 300 / 300 / 0 | 300 / 200 / 100 |
| Обновления optimizer | 0; шаг 1000 → 1000 | 200; шаг 1000 → 1200 |
| Итог | `TRAINING_EXAMPLE_CONTEXT_EXCEEDED` | Успешный native SFT |
| TRAIN правильные токены до → после | 3/42 → 3/42 | **3/42 → 4/42** |
| TRAIN средний response NLL | 4.800813544 → столько же | **4.800813544 → 4.727502428** |
| Полные TRAIN-ответы | 0/2 → 0/2 | 0/2 → 0/2 |
| Контроль Pos.cs, NLL | 5.8156 → столько же | **5.8156 → 5.8640, хуже** |
| Время вызова / выделенная память | 193.2979 мс / 655144 байт | 856.334 мс / 313372816 байт |

B приобретает один правильный токен One.cs; Two.cs остаётся 1/21. Обе экзаменационные строки остаются с нулём правильных токенов, хотя NLL снижается. Длинный Long.cs не обучен и не оценён: его статус `not_scored`, не ошибка с нулевым значением. Время отказа и время 200 обновлений несопоставимы как скорость одной работы. Это измеренное частичное улучшение, достаточное для кандидата по правилу Любы, но не освоенный полный ответ и не доказательство обобщения.

## 2. Математика и точное место вмешательства

Для next-token шага последовательность из T токенов образует T−1 входных позиций и T−1 целей. Допустимо **2 ≤ T ≤ C+1**, где C — окно именно выбранной модели. При C=64 One.cs и Two.cs имеют T=36, Long.cs — T=221. В [host/Program.cs:167–216](../../../../experiments-v2/RP01-MICRO/N63IL/host/Program.cs) B сохраняет порядок допустимых строк и исключает только Long.cs. Нельзя переносить literal `65` в продукт: использовать C из связанной с admission модели и tokenizer того же parent.

Текущий [IncrementalTrainer.cs:385–419](../../../../14.ContinuousLearning/Training/IncrementalTrainer.cs) кодирует `Prompt + "\n" + Response` и отклоняет весь вызов, если хоть одна строка длиннее C+1. Это корректный строгий контракт. Обновления начинаются позже, в том же файле:645–671, через [ProductionSupervisedTrainingStepExecutor](../../../../14.ContinuousLearning/Training/SupervisedTrainingExecution.cs). Исправление размещается **перед строгим обучателем**, в планируемом SFT admission FIX-MC, а не заменой отказа внутри TrainAsync на молчаливый `continue`.

| Точное место будущего изменения | Назначенное изменение |
|---|---|
| Новый `14.ContinuousLearning/Training/SupervisedContextAdmission.cs` | Один internal алгоритм разбиения неизменяемого списка. Для каждого исходного индекса сохранить SourceId, encoded length, статус и причину. Возвращать упорядоченные admitted rows + token sequences и deferred rows; не менять тексты, веса и порядок |
| Планируемый FIX-MC `RepositoryOwnedSupervisedTrainingAdmission.cs` | После проверки исходного запроса и provenance применить разбиение на том же captured parent/tokenizer. Связать план со всеми исходными строками, parent/manifest/tokenizer SHA, C и framing. Затем разрешить только admitted subset |
| Планируемый FIX-MC `RepositoryOwnedSupervisedDatasetManifest.cs` | Сохранить исходный ordered batch digest и решение по каждой строке. Исполняемый SFT dataset и его digest содержат только admitted rows. Deferred источники не записывать как обученные |
| Планируемый FIX-MC `IncrementalTrainer.TrainSupervisedTransactionAsync` | Перед мутацией повторно проверить binding parent/tokenizer и допуск. Использовать уже проверенные token sequences и обычный native SFT step. Не выбирать модель повторно и не токенизировать другим словарём. Existing TrainAsync/TrainTransactionAsync остаются строгими |
| Планируемый FIX-MC `25.ModelServer/ContinuousLearning/CodeDocumentationSupervisedContinuationCoordinator.cs` | В конкретном server-owned SFT-профиле выбрать `DeferOverContext`; вернуть typed admission report вместе с результатом. Исходные 300, admitted 200, deferred 100 и actual updates — разные поля. При отсутствии admitted rows вернуть `DEFERRED_ONLY`, без нового checkpoint/adapter и без обучения |

Режим задаёт выбранный SFT-профиль владельца, не произвольный флаг клиентского промпта. Прямой строгий API, DPO transaction и парные chosen/rejected не получают автоматического отсечения. Слишком короткий материал T<2, malformed SourceId, несовместимый domain, невалидный вес или parent остаются ошибками запроса, а не причиной скрыть строку в deferred.

Сначала проверить provenance **всего** входного списка, включая будущие deferred: нельзя обходить валидацию удалением плохой строки. Затем построить training manifest только admitted источников и соединить его с parent. Состояние deferred сохраняется в SFT admission receipt и исходном dataset; отдельного фонового повторного обучения или автоматического увеличения окна нет. Повтор обращения с тем же accepted transaction возвращает его результат по FIX-MC, а не обучает ещё раз.

Математический вывод ограничен: в B появляется допустимая последовательность шагов вместо нулевого шага A. Он не гарантирует рост качества любой смеси данных. Удаление длинных примеров меняет распределение учебного материала; регрессия Pos.cs — конкретное предупреждение из измерения. Truncation, разбиение длинного текста на части, смена модели и балансировка набора здесь не назначены: их выигрыш этим опытом не проверен.

## 3. Обученный старт, набор и исправление происхождения строк

Использовать [PA01 accepted-parent-handoff](../../../../experiments-v2/RP01-MICRO/PA01/results/accepted-parent-handoff.json) и [N63IL parent-selection](../../../../experiments-v2/RP01-MICRO/N63IL/results-after-fix/parent-selection.json). Полный источник: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/repos/B/training-artifacts/models/code-documentation/sft/rp01-ingress-h16/v1/checkpoints/000002/`. Ordinal2, optimizer step1000, localUpdates0. Это частный экспериментальный donor, не новый canonical production parent.

| Идентичность | Закреплённое значение |
|---|---|
| manifest SHA256 | `e5d93e0c362b8e8fe9348cc68b836430c141bd1605af2642cc78152589d15beb` |
| weights SHA256 | `ba8ec8c20250bc431da4bf9993ff22c508422cd7224b5c57cec33ce7b98c6e0e` |
| tokenizer SHA256 | `3bba3bc0d53fb07a5de4d28839f14c2b296b8034de37e5d561b123a9dcf95bf4` |
| optimizer SHA256 / bytes | `603cf13423f7c6ae5af073a4ea824f49009175b145ce551f6c1b5cf59b66842a` / 1112 |
| rng.json SHA256 / bytes | `7f75acabdd1297985a227a6e154950ee1ac7a10bf6bb4252bca212e82e8cc5ad` / 765 |
| loaded model-state digest | `db44f1a437653299b345dc83c85523aad1ba1c55d3285865fe92501e5e78e386` |
| ingress-length.json SHA256 | `640eee3d89c1ce3907489bdd27ffffaf5a4651315722369293b7d859231084d6` |

Не повторять 1000 шагов подготовки PA01 и его демонстрационное продолжение. В будущие `Tests/Fixtures/ExperimentFixes/v1/SftIngressLength01/` перенести frozen fixture, handoff и необходимые donor bytes без изменения; `ExperimentFixtureLoader` и тестовая фабрика из §4 проверяют исходные hashes, stable parameter keys, формы и каждый бит AdamW moments. A/B получают независимые экземпляры одного состояния; пропавший donor означает `missing_or_incompatible_fixture`, не разрешение bootstrap. Исторический [SftAcceptedParent.cs](../../../../experiments-v2/RP01-MICRO/N63IL/host/SftAcceptedParent.cs) — свидетельство восстановления, а не разрешение подключить экспериментальную копию в native test.

Набор строго фиксирован: 100 повторов тройки `[One.cs, Two.cs, Long.cs]`; после допуска порядок B — 100 повторов `[One.cs, Two.cs]`. Ни экзамен Three/Four, ни контроль Pos не включать в TRAIN. В Program.cs веса назначены равными1, timestamp `2026-10-20T00:00:00Z`; native optimizer продолжает шаг1000 с LR0.0003, weightDecay0, AdamW(.9,.95,eps1e−8), LoRA rank2/alpha4/dropout0. Модель V256/C64/H16/L1/Q2/KV1/FF32. **Источник действительной TrainingConfig — SftAcceptedParent.CreateTrainingConfig:211–215**, а не неиспользованное поле fixture: TotalSteps2000, batch1, seq64, accumulation1, warmup0, clip1, minLR=maxLR0.0003. В fixture записано TotalSteps2, но host не берёт из неё эти настройки; переносить2 было бы ошибкой воспроизведения. Бюджет нового воздействия по-прежнему200 updates, не2000. Не заменять модель обычной H512 RP01; модель **исполнителя** остаётся сессионной по умолчанию.

В [manifest принятого donor](../../../../experiments-v2/RP01-MICRO/PA01/repos/B/training-artifacts/models/code-documentation/sft/rp01-ingress-h16/v1/checkpoints/000002/manifest.json) явно указаны `sourceTrainingParentUnknown=true` и `rngContinuity=ResetAtAdoptionDropoutZero`. Импортируются реальные weights/moments/step1000 и сохранённый RNG принятия; это не доказательство восстановления исторического RNG тысячного шага. Оба плеча используют один и тот же снимок принятия при dropout0. Экспериментальный reviewed import не подменяет product authority и не снимает native FIX-MC admission.

Первый запуск отказал из-за повторяющихся SourceId. [TrainingSourceProvenance.BuildManifest:97–136](../../../../14.ContinuousLearning/Core/TrainingProvenance.cs) требует уникальности. Исправленная постановка использует origin `fixture/One.cs#0` … `#99`, аналогично для остальных файлов, и неизменное content. Это номера реально назначенных предъявлений, а не новый текст и не 300 независимых примеров. Один и тот же рецепт применяется к A и B. Продукт не должен сам переписывать duplicate SourceId ради прохождения guard: производитель назначенного расписания формирует occurrence identity, а неожиданный дубль остаётся ошибкой.

Scoring framing: prompt mask — токены `// file: <имя>` **без LF**; LF входит в ответ. Поэтому 42 цели, а не donor-панель40. Сохранить одновременно hash состояния и этот mask contract. Изменение знаменателя не трактовать как приобретение двух токенов. SFT objective не менять на response-only в этом факторе.

## 4. Перенос опыта в native tests и ремонт assertions

Prerequisite **продуктового внедрения** — уже выбранный и реализованный SFT-only путь FIX-MC с настоящим admission и восстановлением. Если его нет, продуктовая интеграция FIX-IL остаётся зависимым планом; нельзя чинить это фиктивной DPO-парой или копией IncrementalTrainer из опыта. Исследовательский donor не является canonical parent этого пути. Поэтому native учебная проверка и проверка подключения к product admission разделены явно, как в FIX-MC: первая использует настоящий алгоритм на frozen fixture, вторая — wire-контракт без дополнительного обучения. Они вместе не объявляются измеренным production E2E.

Новый файл `Tests/ExperimentFixes/SftIngressLengthTransferTests.cs`, namespace `LlmFromScratch.Tests.ExperimentFixes`. Основной Fact `CaptureLearning` выполняется один раз в каждой native стадии [02](02-native-transfer-and-test-repair.md): A до factor, B после. Общий запланированный `Tests/ExperimentFixes/ExperimentFixtureLoader.cs` проверяет bytes donor и создаёт независимые native model/tokenizer/AdamW instances; маленький test-only `IIncrementalTrainingSessionFactory` в новом классе связывает их с `IncrementalTrainingSession`, как граница `AcceptedParentFactory` в host/Program.cs:425–454. Это адаптация загрузки fixture, не копия learner и не production authority. При чтении private PA manifest не передавать его в строгий production manifest loader и не присваивать обученным весам роль InitialBaselineNoUpdate.

В обоих плечах вызывается настоящий `IncrementalTrainer.TrainAsync` из product assembly с тем же parent и empty DPO list. A подаёт весь исходный список300, B сначала вызывает **продуктовый** `SupervisedContextAdmission`, затем передаёт его admitted200. A получает ожидаемый чистый отказ на длинной строке; это контроль, не причина чинить A до начала B. В B выполняются200 updates. Новый helper является фактором, его правила в test runner не переписывать. Model/optimizer/tokenization/SFT step берутся из собранных проектов, не из `N63IL/src/CL` или `host`.

Снять до/после одну и ту же панель из пяти помещающихся строк, по одному teacher-forced Forward на строку; Long.cs отмечается `not_scored`. Никакой генерации. До A/B проверить общий loaded state и optimizer step1000, TRAIN3/42. После A — точное равенство weights, moments, step, RNG и панели; после B — step1200, изменённые adapters, прежняя frozen base, новый полезный токенный/NLL сдвиг и явная таблица регрессий. Полный ответ0/2 и нулевой EXAM не объявлять успешным навыком. Gold берётся из fixture, не из B.

Другой Fact `CompareCompletedReceipts` только читает уже завершённые A/B: 0 Train/Forward/Backward. Он сравнивает фактические token/NLL/rank, all/deferred/admitted counts и provenance. Исторические числа являются ориентиром сопоставимости, не разрешением ослабить ошибочный результат продукта. Если новый native результат не повторяет полезную ось, оставить его отрицательным и локализовать отличие без нового обучения по умолчанию.

| Существующий/новый тест | Что сохранить или добавить |
|---|---|
| [ContinuousLearningTests.IncrementalTrainer_ContextOverflow_FailsBeforeLoraMutation:3480–3507](../../../../Tests/ContinuousLearningTests.cs) | Оставить строгий отказ `TRAINING_EXAMPLE_CONTEXT_EXCEEDED`, отсутствие LoRA/registry artifacts. Не заменить expected на success: новый допуск находится выше этого API |
| `IncrementalTrainer_InvalidExamples_FailBeforeSessionCreation` в том же файле:3448–3477 | Сохранить domain/prompt/response/weight validation и CreateCount0 |
| Новый Fact `AdmissionContracts` | Проверить на native helper пять длин T=0,1,2,65,66 при C64; первый/последний допустимый токен не теряются. Отдельно проверить порядок и partition тройки, и неизменность отказа duplicate SourceId: всего7 конечных случаев, 0 обучения |
| Новый Fact `AllDeferredDoesNotPublish` | Один материал T66 через SFT admission, отдельный canonical test parent из следующего абзаца: 0 updates/Forward/Backward, явный `DEFERRED_ONLY`, нет нового adapter/checkpoint сверх входного parent, ни одного нового источника в learned manifest |
| Новый Fact `NativeAdmissionWiring` | После native FIX-MC интеграции spy на границе SFT step проверяет, что coordinator/admission передают именно ordered admitted200 и соответствующий digest; отчёт содержит deferred100. Lessons/tokenizer/context те же, но authority и parent descriptor — от отдельной MC test fixture ниже. Model updates0. При отсутствии prerequisite тест не выдаёт PASS и не подменяет проверку копией admission |
| Existing MC restore/publication guards | Использовать принятые результаты prerequisite. Не запускать весь прежний FIX-MC заново ради IL |

Уже падающие тесты в этой сессии не устанавливались. Если после будущего diff тест списка источников/числа уроков ломается, изменить ожидание **только у нового SFT admission пути**: admitted200/deferred100; старые strict API и DPO transaction expectations не ослаблять. Ошибку сохранения отчёта чинить чтением completed receipts, без повторных 200 шагов.

Для двух product-contract Facts в том же новом тестовом классе назначить `CreateCanonicalAdmissionParent`: использовать настоящий test owner и SFT grant/sealer путь из FIX-MC `ServerOwnedSftRoute_PublishesAndColdResumesWithoutRetraining`, но **только создание начального parent**, без его учебной части. Конфигурация здесь V256/C64/H16/L1/Q2/KV1/FF32, q/v rank2/alpha4/dropout0, seed17; tokenizer — те же закреплённые bytes из SftIngressLength01, без BPE.Train. Веса этого отдельного parent создаются нативной инициализацией, optimizer step0; PA weights/moments/manifest не импортируются. Его native descriptor/grant создаёт test owner, не JSON опыта. Root `artifacts/fbe/FIX-IL/attempt-0001/contracts/B/canonical-parent`; общая fixture этих двух Facts создаёт его один раз. Для T66 заданы prompt `a`, response из64 букв `a` и LF между ними, SourceId от origin `fixture/context66` и этого response. Этот материал нужен только для проверки допуска, не для учебной пары. В wiring остаются исходные300 lessons, а spy не обновляет модель; это проверка проводки и учёта, не второй результат обучения.

Бюджет основной пары: 2 вызова native TrainAsync, 200 реальных supervised steps/optimizer updates, 20 диагностических Forward, 0 подготовки, DPO, Generate, teacher calls и обучения словаря. Обновления A=0, B=200. Instrumentation считает training Forward/Backward внутри этих200 шагов отдельно; их нельзя назвать нулевыми, а20diagnostic Forward — общим расходом. Named guard ContextOverflow и InvalidExamples не обучают; новые contracts/all-deferred/wiring также 0 updates. Длинная строка не попадает в model.Forward. Никаких дополнительных прогревов, seeds или повторов для улучшения чисел.

## 5. Команды, E2E и инструкция исполнителю

Это будущие команды после отдельного выбора FIX-IL и готового prerequisite, не инструкция запустить их в документной сессии. Рабочая папка `C:/Projects/llm_mvp_core`; добавить ID к тому же планируемому `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1`. Output: `artifacts/fbe/FIX-IL/attempt-0001/learning/A`, затем `learning/B`, `contracts/B` и `report.ru.md`. A/B binding, fixture hashes и build stage — по02. В каждой стадии один Rebuild Tests проекта, discovery ровно одного выбранного Fact, затем один capture:

```powershell
Set-Location -LiteralPath 'C:/Projects/llm_mvp_core'
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -t:Rebuild
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --list-tests --filter "FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.SftIngressLengthTransferTests.CaptureLearning"
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.SftIngressLengthTransferTests.CaptureLearning" --logger "trx;LogFileName=capture.trx" --results-directory artifacts/fbe/FIX-IL/attempt-0001/learning/A
```

После factor тот же capture с binding/output B. На B по одному запуску точных FQN `LlmFromScratch.Tests.ExperimentFixes.SftIngressLengthTransferTests.AdmissionContracts`, `.AllDeferredDoesNotPublish`, `.NativeAdmissionWiring`, `.CompareCompletedReceipts`, затем два названных existing guards; все сокращённые имена относятся к тому же полному namespace/class. Не использовать фильтр всего класса, который повторит CaptureLearning. Новые классы ещё не созданы, discovery0 не PASS. Продуктовый перенос не завершён без NativeAdmissionWiring и prerequisite; повтор capture ради завершения wiring не нужен.

E2E расширяет [существующий FIX-E2E](../fixes-based-on-experiments/20-prompt-domain-e2e.md): learned route выбирает действительного получателя, его server-owned SFT coordinator связывает parent/tokenizer/context, admission допускает только помещающиеся lessons, native SFT публикует admitted subset, а отчёт сохраняет deferred. Чтение результата адресуется тому же recipient/checkpoint; CODE_Q из fixture не является самостоятельным доказательством learned routing. Outer routing не получает чужие C#-тексты как свои уроки. В IL main capture уже измерен выбранный recipient; дополнительную E2E цепочку обучения автоматически не запускать, использовать принятую границу и сохранённый результат при отдельном назначении FIX-E2E.

Исполнитель использует модель текущей сессии без override; читает AGENTS/CLAUDE и `docs/skills/{planning-session-startup,architect-behavior-classifier,plan-review,generated-code-review,prompt-patch-authoring,output-readability-gate}-SKILL.md`, `human-communication-GLOSSARY.md`. Отчёт по-русски: источник → изменение допуска → TRAIN/EXAM/контроль до/после → цена → регрессии → конкретный кандидат и его пределы.

**Проверка IL-ADMISSION.** Неверно: удалить длинный пример без учёта, заменить строгий отказ успехом, обучить новый donor, подставить65 в любую модель, сообщить ускорение отказа относительно обучения. Верно: тот же сохранённый parent, связанная токенизация, ordered admitted/deferred receipt, неизменный strict API,200 шагов только B и частичный учебный вывод.

Completed A повторно используется для B; живой handle ожидается штатным wait той же операции порциями ≤60 секунд после проверки статуса. Отсутствие файла не доказывает остановку процесса. Не завершать ход словами «жду», не создавать дополнительный monitor, не сочинять работу во время ожидания и не запускать новый attempt вместо неизвестного старого. Прямой STOP/NEXT немедленно прекращает прежнюю работу; исторические цитаты пользователя не считаются новой командой STOP.
