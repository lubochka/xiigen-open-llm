# FIX-OSR — читать проверенный шаг AdamW без копирования моментов

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q57](01-registry.md#work-fix-osr). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-OSR: Приостановить общий перенос.** Малый ресурсный выигрыш достаточен для кандидата второй очереди, но показанный пик >5% не позволяет автоматически перенести.

- Точность: 66/66 учебных токенов сохранены; экзамен и генерация не измерены.
- Полезный результат: Медиана выделений на обновление 1 879 191 334,4 → 1 878 804 284,8 Б (−0,02060%).
- Цена и ограничения: Медиана пика 1 280 729 088 → 1 362 112 512 Б (+6,35%). CPU +1,65%; диапазоны времени пересекаются.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-osr).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски LEARN/STATE:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно. Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Дешёвый step-read не является полным fingerprint

**Проверка `POST102-FIX-OSR`. WRONG.** Из ReadStep получены moments, либо отсутствие экземпляра optimizer разрешает выдуманный measured0.

**CORRECT.** Metadata reader сохраняет проверку формата/границ и честно отдаёт только step. Отсутствие по sentinel contract, неизвестность и настоящий step0 различаются. Исправление обязательного sentinel0 сломало бы reader и не назначается.

**Адресная приёмка.** Существующие valid/corrupt/sentinel fixtures проходят реальный metadata reader без копирования moments. После step1 caller не пишет «не применяется до первого шага» для нечитанных moments. Отдельный payload/restore gate не снимается от дешёвого успешного step-read.

Применять [единый контекст40](../experimets/40-effective-executor-context-2026-11-02.md) и [адресный план121](121-post102-addressed-repair-plan-2026-11-02.md): актуальные роли навыков, один рабочий оценщик, ordinary consumer и реакция на test/report failure. Это ограничивает вывод и усиливает приёмку; исходные factor, доза, fixed panel и порядок сохраняются. Исправление saved evidence не является новой PRIMARY, а наличие этого блока не разрешает запуск. Исторические указания ниже действуют только в своей исходной области; при противоречии бюджета/фактора требуется адресное решение до исполнения.
<!-- POST102-AUDIT-2026-11-02:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-OSR исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-OSR. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-OSR. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Допуск на27 октября — HOLD для общего переноса.** Повторное чтение шести `OPT01/results-01/verdict.json::runs` даёт median process peak A`1 280 729 088`→B`1 362 112 512`bytes, **+6,35446%**. Малый выигрыш allocations около0,0205966% сохраняется как успех своей оси, но не удовлетворяет правилу отсутствия более5% не-CPU ухудшения. Перекрытие диапазонов не доказывает безопасность и не превращает observed peak в причинную оценку фактора. По [69](69-transfer-admission-and-priority-2026-10-27.md) разрешённый безопасный профиль пока не установлен; не исполнять ниже как безусловно допущенный перенос и не начинать повторные прогоны этим дополнением. Новые runtime измерения не проводились; сверка — [82](82-positive-transfer-delta-audit-2026-10-27.md).

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-OSR

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN/STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

Дата: 23 октября 2026. Статус: **документный план; native код, тесты и новые прогоны не выполнялись**. Основание — завершённый опыт №64 / OPT01, прочитанный на `40242e7c6aa5ea1ca1ac70a74b03affac51d5c1a`. Исполнитель будущего переноса — модель текущей сессии по умолчанию. Общие условия: [реестр](01-registry.md), [состояние и fixtures](02-learning-state-and-fixtures.md), [исполнение и отчёт](03-test-and-execution-contract.md), [prompt→domain](20-prompt-domain-e2e.md).

**Решение:** в одном месте `DpoTrainer.ExecuteWithPublication` получать номер шага для callback сохранения RNG из проверенных метаданных AdamW. Массивы первого и второго моментов здесь не нужны. Полный экспорт для публикации, продолжения и rollback остаётся прежним. Перенос уменьшает выделения памяти в реальном DPO для документации C#; он не обучает новый домен и не меняет objective.

## 1. Что подтверждено OPT01 и чего его отчёт не доказывает

Прочитаны [report](../../../../experiments-v2/RP01-MICRO/OPT01/results-01/report.md), [verdict](../../../../experiments-v2/RP01-MICRO/OPT01/results-01/verdict.json), [run-01-A](../../../../experiments-v2/RP01-MICRO/OPT01/results-01/run-01-A/arm-result.json), [Opt01Bridge.cs](../../../../experiments-v2/RP01-MICRO/OPT01/Opt01Bridge.cs), [Sources.DPO.props](../../../../experiments-v2/RP01-MICRO/OPT01/Sources.DPO.props), [Sources.TrainingLoop.props](../../../../experiments-v2/RP01-MICRO/OPT01/Sources.TrainingLoop.props). Они связывают измерения с прежним source-copy `35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/{A,B}`, а не с изменённым продуктовым assembly. Точный переносимый diff виден в [B/AdamW.cs](../../../../35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/Optimizers/AdamW.cs), [B/AdamWPortableState.cs](../../../../35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/Optimizers/AdamWPortableState.cs) и [B/DpoTrainer.cs](../../../../35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs); baseline — соответствующие файлы [A](../../../../35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/A/DpoTrainer.cs).

В шести завершённых прогонах порядок `A B B A B A`, по 10 DPO updates в каждом. Во всех raw записях verdict совпали начальный и конечный keyed parameter hash, десять losses и before/after panels. Обе TRAIN строки D-T1/D-T2 дают по 33/33 правильных response tokens до и после, whole exact — 2/2; оптимизатор проходит 0→10, параметры действительно меняются. Это положительная квалификация обученного старта, которой не было у раннего опыта E-OPT-STEP. Сопоставление этих полей не является независимым сравнением всех бинарных optimizer/RNG snapshots.

| Показатель | Наблюдение A → B | Граница вывода |
|---|---|---|
| Медиана allocated bytes / update | `1 879 191 334,4 → 1 878 804 284,8`; экономия `387 049,6`, около `0,0205966%` | Три точки на руку; диапазоны не пересекаются, зазор `382 884` bytes. Поддержана полезная ось allocations в этой серии |
| Медиана wall / update | `15 334,97326 → 14 863,37801 ms`, около `−3,0753%` | Диапазоны перекрываются; причинный процент ускорения не установлен |
| Медиана CPU / update | `12 237,5 → 12 439,0625 ms`, около `+1,6471%` | Наблюдаемое ухудшение сохраняется в отчёте; диапазоны перекрываются |
| Median process peak | `1 280 729 088 → 1 362 112 512`bytes, `+6,35446%`; диапазоны перекрываются | Выигрыш peak не доказан; общий допуск HOLD, причинное происхождение ухудшения не установлено |

`Opt01Bridge.cs:201–224` ставит счётчики вокруг всего цикла `StepVector`, включая штатную публикацию и одинаковые записи progress/loss. Поле `measured_path` на строке 258 — описание фактора. Формулировка report «измеряется один callback» слишком сильна: прямого изолированного измерения callback нет. Старые комментарии bridge о frozen-base factor, опыте 60 и пяти повторах не заменяют исполняемый код и шесть raw результатов.

**Исправление внутренней сверки report.** Размер portable-файла `426 788` bytes нельзя приравнять к managed allocation. Native [LoraLinear.cs:54–60,95–101](../../../../10.LoRA/LoraLinear.cs) задаёт `A[r,in]` и `B[out,r]`; [CausalSelfAttention.cs:62–64](../../../../03.TransformerModel/Layers/CausalSelfAttention.cs) задаёт q width `8×64=512`, v width `2×64=128`. При четырёх слоях, H512 и rank 8 число adapter floats равно

`N = 4 × [(8×512 + 512×8) + (8×512 + 128×8)] = 53 248`.

Полезная нагрузка двух float32 moments — `2×4×N = 425 984` bytes. [AdamWPortableState.cs:130–180](../../../../04.TrainingLoop/Optimizers/AdamWPortableState.cs) отдельно сериализует header, ключи, shapes и lengths; разница с указанным файлом составляет `804` bytes, а не `39 738`. Это арифметика формата/топологии, не повторное измерение allocation или разбор binary payload в этой работе.

Свежий AdamW имеет пустую память перед первым update. В десяти pre-step чтениях полные моменты присутствуют только перед updates 2–10: средний устранённый объём самих float payload равен `9/10 × 425 984 = 383 385,6` bytes / update. Полный экспорт также создаёт объекты entries, копии shapes, массивы и read-only wrappers; metadata reader сохраняет часть объектов и проверок. Из разницы с наблюдаемыми `387 049,6` нельзя восстановить точную разбивку managed allocations. Фраза «39 738 — metadata» не переносится в план; такого allocation breakdown нет. Числа опыта не становятся точным CI порогом для другого runtime или профиля.

После 10 updates chosen NLL D-T1 изменился `0,0001682954173 → 0,0001711616317`, D-T2 — `0,0001697295635 → 0,0001762844567`: небольшое ухудшение одинаково A/B. Сохранённые правильные ответы проверены с teacher forcing; свободная генерация и EXAM не выполнялись. Общие изменения margin не приписываются FIX-OSR.

## 2. Единственная native дельта и неизменные проверки

| Product-файл и исходная точка | Будущее изменение |
|---|---|
| [04.TrainingLoop/Optimizers/AdamW.cs:188–227](../../../../04.TrainingLoop/Optimizers/AdamW.cs) | Добавить `ReadPortableMetadata(IReadOnlyList<AdamWParameterBinding>)` по проверенному B: версия, `_step`, ключи, owned shapes и lengths; не читать/копировать значения `m`/`v` |
| [04.TrainingLoop/Optimizers/AdamWPortableState.cs](../../../../04.TrainingLoop/Optimizers/AdamWPortableState.cs) | Добавить `AdamWPortableParameterMetadata` и `AdamWPortableMetadata` с owned read-only metadata. Для общих пределов раскрыть существующие `MaximumEntryCount` и `MaximumKeyByteLength` лишь как `internal`, как в B; версия portable codec остаётся 2 |
| [11.DPO/DpoTrainer.cs:264–274,507–519,1228–1244](../../../../11.DPO/DpoTrainer.cs) | Добавить `DpoProjectLoraRecipient.RequireAllowedMetadata` и private `ReadOptimizerStepForRandomState`. Для project recipient — metadata + прежняя recipient validation; для остальных trainers — прежний `ExportOptimizerState().Step`. Заменить только аргумент callback в pre-step snapshot |

Точная форма вызова остаётся `_captureRandomState?.Invoke(ReadOptimizerStepForRandomState())`: при `null` callback аргумент **не вычисляется**. Вынос metadata read перед `?.` добавил бы проверки и allocations в прежний null-путь и не является этим переносом. Новый reader не выдаёт permit, proof, lease или право публикации. Не менять public trainer constructors, mutation admission, test-scoped issuer, pair authorship, profile/continuation checks или checkpoint address rules.

Reader обязан повторить **порядок и смысл** `ExportPortableState`: null list/binding/parameter; непустой key; уникальные ordinal keys и parameter identities; затем пропуск binding без накопленных moments; затем `ValidateStateEntry`, проверка shape и создание metadata. Пропуск lazy entry остаётся до shape-проверки: нельзя фабриковать нулевые моменты или добавлять новое требование ко всем неинициализированным bindings.

Сохранить `ADAMW_PORTABLE_BINDING_KEY_EMPTY`, `…KEY_DUPLICATE`, `…PARAMETER_DUPLICATE`, `ADAMW_SNAPSHOT_SHAPE_MISMATCH`, `ADAMW_PORTABLE_SHAPE_INVALID` и соответствующие типы исключений. Metadata constructors сохраняют проверки версии, неотрицательного step, количества entries, UTF-8 key length, повторяющихся keys, равных moment lengths и безопасного произведения dimensions. Общие bounds — те же константы, а не новые числа. Ограничение сериализованного frame проверяется в прежнем `ToBytes`; reader не сериализует frame и не обязан искусственно запускать этот codec. Старый Export не проверял finite каждого moment value; новый reader не вводит и не убирает такую проверку.

`RequireAllowedMetadata` сохраняет default `validateRecipient=true`, вызов полного `RequireValid` и проверку каждого entry по реальным bindings: key, exact dimensions, first length = parameter data length, second length = first length. Ошибка остаётся `DPO_PROJECT_LORA_RECIPIENT_INVALID`. Сохраняются reference hash, frozen-base bits, отсутствие alias, topology/rank/alpha/dropout/RequiresGrad, вся allowlist и частота существующих проверок. Нельзя заменить это простым публичным `_step` getter или ранним `return`.

Полный `ExportOptimizerState()` в `BuildPublicationBundle` на строках 1273–1282 **остаётся**. Полные copies в [DpoTrainerStepSnapshot.Capture:41–68](../../../../11.DPO/DpoTrainerStepSnapshot.cs) нужны для rollback и тоже остаются. Не менять `TryImportPortableState`, portable codec, loss, AdamW update, RNG capture/restore или publication transaction. Из уже применённого FIX-FBC либо другой общей базы ничего не откатывать: зафиксировать эту базу одинаково A/B. Перенос FIX-FBC не является prerequisite FIX-OSR.

### Почему преобразование эквивалентно

Для полного состояния `S=(version, step, {(key, shape, m, v)})` определить проекцию `π(S)=(version, step, {(key, shape, |m|, |v|)})`. В данном callback используется только `step`; все проверки на этом пути зависят от полей `π(S)`, bindings и текущего recipient. Поэтому для допустимого состояния `ReadPortableMetadata(bindings) = π(ExportPortableState(bindings))`; недопустимые входы должны отвергаться теми же проверками до callback. Утверждение не распространяется на публикацию/restore, которым нужны сами `m` и `v`.

При `N` moment elements, `P` entries и суммарном размере ключей/shapes `K` полный экспорт требует `Θ(N+K+P)` работы/новой памяти; metadata reader убирает `Θ(N)` копирование и оставляет `O(K+P)`. Это **не** сложность всего callback: `RequireValid` по-прежнему хеширует reference, проверяет базу и делает поиски bindings. Из формулы не следует ускорение всего обучения или пропорциональная экономия wall/peak.

## 3. Fixture: полностью повторно использовать квалифицированный профиль FBQ

`Opt01Bridge.LoadQualified` загружает сохранённый S0 в **обе независимые** policy/reference, как [54 §4](54-frozen-base-check-scope.md). `run-01-A` подтверждает те же пути, hashes и длины. Применить запланированный там `Tests/ExperimentFixes/FbqLearningFixture.cs` и `Tests/Fixtures/ExperimentFixes/v1/Fbq/manifest.json`; создать loader, если его ещё нет, но не запускать тесты FIX-FBC и не менять его factor ради FIX-OSR.

| Вход/настройка | Точная привязка |
|---|---|
| Donor | `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/S0.weights.bin`, `47 899 519` bytes, SHA-256 `a3cad45680c0953ffd03ca52a32d80f0afa5547015e493907af6811cf85cd9c5` |
| Generation / tokenizer | Generation SHA `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`; tokenizer SHA `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`; остальные hash/length/B0 gates — ровно 54 §4 |
| Lessons | [OPT01/train.jsonl](../../../../experiments-v2/RP01-MICRO/OPT01/train.jsonl), `841` bytes, SHA `abb353b31fd5c973fe2e84efbb91e465bf9027a90e870903f36ca66718c78a41`; bytes совпадают с FBQ01/train.jsonl |
| Model / LoRA | V629, context 2048, H512, 4 layers, 8 heads, 2 KV heads, FFN1408; q/v rank 8, alpha 16, dropout 0; 39 base и 16 adapter tensors |
| RNG / restart | Root/base seed `20261014`, streams `rp01-policy`/`rp01-reference`. Fresh DPO optimizer с шагом 0, continuation proof=null; moments/RNG SFT producer на шаге 1000 не импортировать. Это weights-only restart, не точное продолжение producer |
| DPO | LR `1e−5`, AdamW β₁ `0.9`, β₂ `0.95`, decay `0.01`, ε `1e−8`, DPO β `0.1`; D-T1→D-T2 пять раз |

`LogicalDpoPreparedBatchMaterializer.Prepare` сохраняет exact masks/vectors и обе стороны `DeterministicGenerator`; `CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart`, `ValidateFullCheckpointArtifact/LoadCheckpoint` и actual source assemblies используются штатно. Reference полностью frozen, arrays не alias. Before panel обязан дать 33/33 и whole exact по обоим урокам после реальной native загрузки. Hash без panel не доказывает квалификацию; отсутствующий/LFS-pointer/несовместимый payload — `missing_or_incompatible_fixture`, **0 producer**, без нового acquisition, подмены donor, tokenizer или referenceB0. Эта документная работа не заявляет нового чтения полного binary bundle или выполненной native загрузки.

## 4. Узкие contracts и ремонт existing tests

Все FQN ниже в **global namespace**. Существующий namespace нельзя дополнять `LlmFromScratch.Tests` только из-за имени проекта. До A создать общий test harness; production metadata API появится только между A и B. Узкий test-only lazy reflection adapter в A проецирует полный native Export, в B требует именно новый reader/constructor/recipient method; проверяет разрешённый source SHA и разворачивает `TargetInvocationException` для сравнения настоящих ошибок. Отсутствие нового API в B — failure, а не fallback. Он не выбирает learner и не копирует алгоритм AdamW/DPO. В A не добавлять product stub или metadata API для компиляции тестов.

| Точный метод | Смысл и ограниченный расход на стадию |
|---|---|
| `AdamWOptimizerStateRecoveryTests.PortableState_SnapshotCollectionsDoNotAlias` — existing Fact | Сохранить полный portable-state ownership и неизменность snapshot; 0 optimizer steps |
| `AdamWOptimizerStateRecoveryTests.PortableState_SparseAndKeyContractsRemainStable` — existing Fact | Сохранить sparse import/export, duplicate/shape отказ и unchanged bytes; 1 прямой tiny AdamW step, без model Forward/Backward |
| `DpoTrainerOptimizerStateHandoffTests.ExactRecipient_UpdatesOnlyInstalledAdaptersAndExportsActualProfile` — existing Fact | Сохранить allowlist/profile, первый успешный Step и второй Step с publication failure/rollback. RNG callback здесь null: это полезная проверка прежнего пути, не доказательство нового metadata callback |
| `DpoTrainerOptimizerStateHandoffTests.ExactRecipient_MomentAllowlistCheckedBeforeFirstRandomRestore` — existing Theory, обе строки | Сохранить sparse valid state и foreign-key отказ до RNG restore; 0 updates |
| `AdamWOptimizerStateRecoveryTests.PortableMetadata_MatchesFullExportAndPreservesFailures` — новый Fact в том же файле | 23 фиксированных synthetic случая ниже, без Train/Forward/optimizer update; A oracle — полный Export + явный expected projection, B — новый reader |
| `DpoTrainerOptimizerStateHandoffTests.ExactRecipient_MetadataGuardPreservesRecipientValidation` — новый Fact в том же файле | 8 фиксированных случаев ниже, один существующий `ExactRecipientFixture`, без Step/Forward/optimizer update |

Для нового portable Fact: два положительных случая — fresh step 0 / empty и импортированный sparse step 7 с одним `[2,3]` entry длины 6 и вторым lazy binding. Сравнить version/step/key/shape/count, неизменность optimizer до/после read и owned read-only metadata после изменения исходного shape/массивов в отдельной проверке. Во втором случае lazy binding не должен порождать entry. Десять отрицательных reader inputs: null list, null binding, null parameter, empty key, duplicate key, duplicate parameter, длина m не равна parameter data, длина v не равна data, неверное произведение live dimensions, UTF-8 key из 1025 ASCII bytes при существующем state. Повреждение live `_state` допускается только reflection в test helper; после проверки его восстановить, product import не ослаблять.

Ещё шесть header cases сравнивают constructor contracts полного состояния и metadata: неподдерживаемая версия, отрицательный step, null entries, null entry, duplicate entry key, count `1 000 001`. Для count использовать `IReadOnlyList<T>` с заведомо большим Count и запрещённым enumerator: отказ должен произойти до enumeration, без миллиона объектов. Последние пять parameter-constructor cases: разные длины moments, empty dimensions, нулевая dimension, отрицательная dimension, `[65536,65536]` при длине 1. Не выделять гигантские payload ради overflow. Ожидания — одинаковый тип исключения и error code, а не совпадение локализованного полного сообщения. Это 23 внутренних случая одного Fact, не 23 запуска обучения.

Для нового recipient Fact: empty, sparse valid, foreign key, другая shape с тем же произведением, другая valid shape с иным length, active base, изменение bit frozen policy base, изменение bit reference после Capture. A вызывает существующий `RequireAllowedState`, B — `RequireAllowedMetadata` с default `validateRecipient=true`; metadata для отрицательных allowlist cases валидны сами по себе. Первые два случая принимаются, остальные дают прежний `DPO_PROJECT_LORA_RECIPIENT_INVALID`. Использовать один tiny fixture, восстанавливая намеренные мутации между случаями; не пересоздавать и не обучать его восемь раз. Отказ не обязан откатывать внешнюю мутацию, сделанную тестом до вызова guard.

Existing rollback asserts не удалять и не заменять сравнением только step/hash. Сравнение frozen floats, если оно ещё числовое, исправить на независимый scalar integer-bit oracle в общем harness до A. Не назначать широкую suite, дополнительные finite-difference fits или прежние экспериментальные hosts. Existing `PublicationFailureRestoresRngAndLeavesAcceptedParentUntouched` использует legacy trainer: его unchanged fallback проверить адресным source review; этот план не запускает его дополнительно и не приписывает ему coverage project metadata path.

## 5. Один native A/B: команды, бюджет и сравнение

Будущий новый Fact — `OptimizerStepMetadataTransferTests.NativeQualifiedLearning_RecordsArm`, файл `Tests/ExperimentFixes/OptimizerStepMetadataTransferTests.cs`, global namespace. Он использует общий FBQ loader, реальные native `AdamW`, `DpoTrainer`, `LlmModel`, собственный immutable output FIX-OSR и ненулевые штатные RNG capture/restore callbacks. Assembly path/SHA/MVID сверить до model calls; сборки/source-link из E-OPT-STEP/OPT01 не допускаются. Test callback регистрирует реально переданные pre-step 0…9 и publication 1…10, не вызывает дополнительный Export или Forward в measured window.

Общий будущий runner [03](03-test-and-execution-contract.md) ещё должен получить entry `FIX-OSR`; команды ниже — точный интерфейс после его реализации, не заявление о существующей Stage API. `$baselineCommit` и `$candidateCommit` — полные подтверждённые SHA в единственном checkout `C:/Projects/llm_mvp_core`, ветка `codex/dpo-implementation`. Сначала common harness при прежнем product, завершённая A; затем только product diff §2 и B. Не создавать worktree/вторую копию/ветку, не выполнять reset и не повторять A.

```powershell
Set-Location C:/Projects/llm_mvp_core
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-OSR -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
# После terminal A и native diff, в том же checkout:
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-OSR -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
```

В каждой стадии — один Rebuild `Tests/LlmFromScratch.Tests.csproj`, Release/net9.0, одна discovery, один contract process и один learning process. Перед первым обращением к проекту выполнить только общую build-предпосылку 02 §10. Contract filter — OR из шести **точных** `FullyQualifiedName=` методов таблицы §4; Theory раскрывается в две строки. Learning filter — `FullyQualifiedName=OptimizerStepMetadataTransferTests.NativeQualifiedLearning_RecordsArm`. Discovery обязана перечислить **7 contract cases + 1 learning case** на стадию. Итого 2 Rebuild, 2 discovery, 4 test processes, 16 test-case executions; compare/formatter читают уже готовые receipts и ничего не обучают.

Основное окно каждой руки — `StepVector(new[]{batches[index%2]}, index+1)` ровно 10 раз. Before/after panel — по четыре diagnostic forwards, итого восемь на руку. **Главный A+B бюджет: 20 DPO updates, 80 training forwards, 40 backward, 20 optimizer applications, 16 diagnostic forwards; 0 producer/SFT/Generate/EXAM/BPE Train и 0 external teacher calls.** Полная publication каждого шага входит в окно. Дополнительный contract расход за A+B: четыре tiny Step attempts, из них два сохраняются, два откатываются, 16 tiny model forwards и 8 backward; два прямых AdamW steps без модели. Четыре `ExactRecipientFixture` на стадию дают всего 8 маленьких `BpeTokenizer.Train("fixture",256)` с 0 merges; это existing test setup вне main window, не скрытый corpus fit. Новые synthetic contracts не добавляют neural updates. Исторические 60 updates и 48 diagnostics OPT01 в этот будущий бюджет не входят и не повторяются шестью runs.

До model calls создать `artifacts/fbe/FIX-OSR/attempt-0001/intent.json` через CreateNew с input/source/runtime hashes и budget; результаты — `{contracts,learning}/{A,B}`. Candidate-binding ссылается на digest A intent. При наличии completed/in-progress результата читать его, не перезаписывать, не чистить output и не выбирать автоматом attempt-0002. Terminal receipt из finally содержит actual attempted/completed/rolled-back counts; ошибка до ownership видна в TRX/stdout. Отсутствующий fixture или отсутствие обнаруженных тестов не PASS.

Measured window как в OPT01 охватывает все 10 StepVector, штатную публикацию и одинаковый bounded logging/observer. Restore, panels, финальное readback и форматирование находятся вне окна. Записать total и per-update wall/CPU/allocated, GC0/1/2, process peak и порядок A→B. Копирование moments устранено по source/contract proof; измеренный delta всего окна не назвать собственной стоимостью callback. Новый необязательный microbenchmark ради точной атрибуции не назначается.

Main acceptance: правильный learned-before, 33/33 и whole exact по обеим строкам после, реальные изменения adapters, optimizer 0→10, неизменные frozen base/reference, равные A/B учебные поля десяти step records, final keyed parameter bits, полные optimizer bytes и typed RNG identity/cursor/state. Сравнить индексы, порядок уроков, optimizer step, loss/rewards/margins и token counts; timestamp, wall/CPU/GC/allocated/peak и абсолютные output paths сравниваются как provenance/ресурсы, а не требуют равенства. Произвольное исключение других полей не допускается. Final weights уже опубликованы native trainer как `dpo_policy_step_000010.bin`; полный optimizer/RNG сохранить штатными `RepositoryOwnedDpoOptimizerStateFile` / `RepositoryOwnedDpoRandomStateFile` вне окна, прочитать обратно и сравнить. Загрузить final weights в тот же policy и сравнить keyed bits без нового Forward/fit. Это private test output, не accepted model publication и не continuation authority.

Ресурсная цель — меньшие allocated bytes при сохранении учебных и state contracts. Не требовать ровно 387 050 bytes или исторических losses на другом runtime. Если direction в этом единственном native A/B не подтвердился, записать `resource_gain_not_confirmed`; не приравнивать это к доказанному улучшению и не добирать повторы до нужного знака. Если A/B state/updates расходятся, это correctness failure независимо от времени/памяти. Все NLL/CPU/peak ухудшения показываются рядом с выгодой.

## 6. Consumer, завершение и управление исполнением

Native consumer — [CodeDocumentationSimulationDpoExperiment.cs](../../../../14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs): `Capture → CreateForProjectLora → StepVector`. Исправление действует в его обычном trainer при ненулевом RNG callback, не требует нового domain handler. По [20 §16](20-prompt-domain-e2e.md) existing E2E может прочитать завершённую квитанцию FIX-OSR с точными input/assembly identities; отсутствие соответствующего consumer обозначается `not_applicable`, не общей победой маршрутизатора. **0 дополнительных E2E requests, H512 loads или fits.** Main retention не доказывает новый learned route, generalization, free generation или Wiki gating.

Будущий исполнитель читает актуальные AGENTS/CLAUDE и навыки из 03 §4: architecture, planning, plan-review, generated-code-review/test-integrity, prompt-patch-authoring и русский glossary/report. Не менять модель/effort самостоятельно. Здесь использованы адресные source и planning/review фрагменты; полный reload навыков и выполненная реализация не заявляются. Русский `report.ru.md` по 03 §8 называет source diff, квалификацию состояния, расход, allocations и все ухудшения, а также ограничения исторического отчёта из §1.

После запуска ждать **тот же** реальный handle блокирующим штатным wait, порциями не дольше 60 секунд, до terminal state. `exec_command` session ожидается `write_stdin` с пустым input; `functions.wait` — только после `Script running with cell ID`. Не заканчивать ход словом «жду», пока процесс жив, не заводить второй waiter/повторный launch. Timeout одного вызова wait оставляет процесс running и требует следующего штатного ожидания того же handle. Отдельные deadlines процесса: Rebuild 15 минут, contracts 5 минут, learning 15 минут. По deadline проверить owned handle и при необходимости штатно остановить именно его; terminal failure записать только после подтверждённого exit/завершённой остановки, сохранив частичный расход первоначальной попытки. Если остановка ещё не подтверждена, состояние остаётся running/stop-requested. Отсутствие log или истечение одного ожидания не означает завершение процесса.

**STOP/NEXT:** немедленно прекратить прежнюю карточку; не дописывать её отчёт, не править тесты, не коммитить, не чистить output и не делать «последний» запуск. При NEXT перейти к точному ID реестра, назвав состояние живого handle, если он есть. Этот текст сейчас разрешает только документный план: ни одна из будущих команд, загрузок, проверок или публикаций модели в этой работе не выполнялась.
