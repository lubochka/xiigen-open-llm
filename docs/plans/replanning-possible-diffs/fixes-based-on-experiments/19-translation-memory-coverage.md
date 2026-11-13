# 19. Закрепить обучение EN→RU памяти и её восстановление в новом процессе

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q49](01-registry.md#work-test-tmem). Это отдельное задание на проверку обучения. Продуктовый исполнитель берёт его только по назначению на эту строку. В P125 обычный поток перевода улучшился с 0/24 до 19/24. Для отдельного покрытия памяти используйте его корпус. Прочитайте известную пару и предусмотренный пример переноса до обучения, после уроков и через вновь открытый переводчик. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — атомарная партия сделана, согласование писателей осталось

a28bc355 реализовал staged whole-graph publication; сохранены 6/6 atomic checks и 114/114 neighbours, naive runtime-only swap даёт 4/6. Не назначать этот holder повторно. R01 закрывает два отдельных source-level interleaving: два writer от одной базы теряют update/номер; Evaluate(S0) может записать метрику в S1. Positive старый/new orchestrator и один lawful writer сохраняются; negative контролируемые барьеры выявляют lost update и чужую accuracy.

M03a/b остаются: canonical semantic key для всех readers, PairId отдельно, RULE после PAIR сохранён, collision отказ до swap. Исправленный M03c уже читает импортированный target, но green whole-product из его журнала не следует. Late held-out и отмена имеют собственный gate, Q129 дозу не расширять.

Основание: [адресный план 135](codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.


**Уточнение перед публикацией, 1aaa3bc:** канонический основной builder, PairId, ряд readers/imports, collision preflight и удаление PAIR-обхода floor уже реализованы в исходнике; native gate UNRUN. Открыты version/default, достижение кандидата N16, RULE/restore и новая N18 (non-object JSON в collision preflight). Действующий остаток — [135/R09](codex-135-post113-addressed-repair-plan-2026-11-02.md), подробная сверка — [отдельный документ](../../../../experiments-v2/_audits/2026-11-02-post113-audit/publication-reconciliation.md). Повторять реализованные части M03 не требуется.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- POSTMERGE-HARDENING-2026-11-02:START -->
## После merge: найденная память и восстановленный потребитель

Факт наличия записи в re-export не подтверждает использование памяти. После fresh import работает именно target без Train; неверный payload при прежнем ID/размере должен изменить полный ответ. Trace разделяет поиск/допуск/выбор/final source. При staged publication согласован весь граф runtime/Flow/Units, включая ранее выданный orchestrator.

Основание и выбранный ремонт: [127](codex-127-postmerge-addressed-repair-plan-2026-11-02.md). Исходные результаты и учебные дозы сохраняются. Контроли реализации не добавляют скрытые Train/Read к научному паспорту; их стоимость и scope указываются отдельно.
<!-- POSTMERGE-HARDENING-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски TRANSLATION/STATE:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold. Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Покрытие памяти включает согласованность ключей и атомарную публикацию.** В набор состояния входят semantic corrections, executor targets, verifier requirements, lesson identity/index, version/history/readiness. Новый PairId хранится как provenance; semantic key с сохранёнными RULE одинаков у записи и чтения. Миграция старых ключей выполняется целиком и обнаруживает коллизии до публикации.

**Контрпримеры:** одинаковая семантика с другим PairId не меняет source-floor решение; удаление требуемого слова отвергается verifier; malformed heldout и поздний persist failure не оставляют новый урок в live памяти. Проверять содержимое прежних ответов и все изменяемые компоненты, не только размер dictionary.

**Обычная цепочка:** TrainStructuredBatchAsync→state commit→TranslationMachine→fresh import→та же панель. Проверить early-return route: exact lookup может скрыть недостигнутый KNN. P1062/2 по аннотации не закрывает token roles или пунктуацию; Q129 закрывает свой синтаксический boundary, не весь rollback. Научные результаты и дозы этих опытов сохраняются.

Основание: [аудит 123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md), [план 124](codex-124-post107-addressed-repair-plan-2026-11-02.md), [контекст 41](../experimets/codex-41-post107-executor-checks-2026-11-02.md). Это уточнение приёмки будущего переноса; прежние результаты и назначенная доза сохраняются.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Восстановление согласованной памяти после партии

**Проверка `POST102-TEST-TMEM`. WRONG.** Урок виден live, значит rejected batch ничего не записал и fresh child получит согласованное состояние.

**CORRECT.** Coverage включает native batch→все memory/identity/version/history→один accepted snapshot→ordinary reader в fresh process. Syntax preflight не эквивалентен atomic rollback поздней evaluation.

**Адресная приёмка.** R06: legal new lesson плюс malformed heldout и late evaluation failure оставляют весь accepted S0, не только один dictionary. После законного приёма live/cold ответы совпадают; не повторять Train внутри restore helper. Existing exact retention и original budget сохраняются.

Применять [единый контекст40](../experimets/40-effective-executor-context-2026-11-02.md) и [адресный план121](121-post102-addressed-repair-plan-2026-11-02.md): актуальные роли навыков, один рабочий оценщик, ordinary consumer и реакция на test/report failure. Это ограничивает вывод и усиливает приёмку; исходные factor, доза, fixed panel и порядок сохраняются. Исправление saved evidence не является новой PRIMARY, а наличие этого блока не разрешает запуск. Исторические указания ниже действуют только в своей исходной области; при противоречии бюджета/фактора требуется адресное решение до исполнения.
<!-- POST102-AUDIT-2026-11-02:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Проверять чтение именно памяти, изменённой назначенным Train, с реальными ключами и payload до потребителя и после restore. Законный kNN retrieval сохранён; запрещена подстановка gold/архивного oracle вместо текущего поиска в обученной памяти. Отдельный PASS структуры или счётчик-константа не доказывает этот consumer либо новые updates.

Для TEST-TMEM исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Сохранять построчные исходы и независимый gold выбранного домена вместе с итоговой долей. Одинаковая общая точность может скрыть потерю прежнего урока и случайный выигрыш другой строки; изменение scorer/support само по себе не доказывает обучение. Для данной карточки подтверждать назначенный Train и чтение его результата реальным потребителем; существующую проверку усиливать сохранённым learned state вместо дублирования искусственного теста.

Применимость: TEST-TMEM. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Панель должна сохранять точное множество уникальных RowId, Kind и gold. Контрпример P05: [T1,T2]→[T1,T1] имеет тот же размер, но прячет потерянный урок. Контрпример P11: 2/4→2/4 скрывает E1 gain и E2 loss; constant-answer baseline считать отдельно. 1→0 после настоящего обучения остаётся валидной регрессией. Сдвиг весов сам по себе не квалифицирует результат, PARTIAL и нулевой назначенный Train не закрывают учебную цель.

Применимость: TEST-TMEM. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: TEST-TMEM

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** TRANSLATION/STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**K — владение допущенным состоянием и восстановление.** Показать, какой объект хранит допущенные параметры, примеры, конфигурацию или ключ кэша; интерфейс IReadOnlyList сам по себе не доказывает неизменность. Контроль после создания/допуска: изменение исходного caller-owned массива или внешнего представления не меняет захваченную запись незаметно. Законное обновление/смена model identity вызывает предусмотренное обновление или инвалидирование кэша, а не запрет обучения. Если reader сохраняется/восстанавливается, сравнить поведение обычного consumer по сохранённым данным и настройкам: reconstruction с default не является эквивалентным restore opt-in режима. Ошибка из KDP: catalogue был проверен, затем изменён через возвращённый массив; schema не содержала catalogue. Для этой карточки проверять её собственный carrier, не добавлять чужой KNN-механизм ради теста. Если изменяемого carrier/сохранения нет, отметить это явно в evidence и не выдумывать лишний runtime.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Независимость cold reader от fixture, 27 октября 2026.** Сохранённые translation entries/phrase mappings и их Output читаются из назначенного исторического checkpoint; fixture ID служит сверке provenance, но не ключом для подстановки пропавшего ответа. В comparer уже captured source/loaded state проверять содержимое entries и actual24 outputs, а не только число записей/route/confidence. Копия состояния с удалённой learned phrase при прежних IDs/count не проходит полную semantic comparison; expected берётся из отдельного frozen scorer, не из заново обученного B.

Два native memory Train/16lessons уже обеспечивают учебную часть новой программы; cold leg0Train проверяет именно сохранённый donor и не становится «нулевым обучающим экспериментом».168Flow и один cold load остаются пределом; новый cold process/fit ради каждого assertion не добавляется. Отсутствие AdamW/RNG здесь обосновано symbolic translation memory, а не пропуском нейронного состояния.

ID: **TEST-TMEM**. Статус: **план недостающего тестового покрытия**, без нового исправления продукта. Основания — завершённые P125-enru-memory-01 и P25-enru-cold-01. Порядок и следующий ID — [реестр](01-registry.md); состояния — [контракт02](02-learning-state-and-fixtures.md); исполнение — [контракт03](03-test-and-execution-contract.md). Этот план добавлен после проверки уже опубликованного пакета на `00f627f1371414f92f43ec2be3c4af56e24d68b2`.

## 1. Основание и новизна

Доставка `phrase_mappings` уже исправлена. Повторно реализовывать её этот план не назначает. Новое содержание — постоянный native regression test на замороженной панели, причинный контроль с заменой учебных target и отдельный process cold-load без обучения. В проверенных существующих планах описаны исправленный ingress и результаты P125/P25; отдельная связка этих трёх проверок в `Tests` не назначена. Существующие маленькие проверки `TranslationDomainTests` сохраняются.

Первичные основания: [отчёт P125](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md), [raw P125](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/raw-outputs.json), [отчёт P25](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md), [сравнение P25](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/comparison-summary.json).

| Наблюдение | Измеренный результат | Граница |
|---|---|---|
| P125, обучение памяти B одним batch из восьми уроков | 0/24 → 19/24 верных ответов обычного Flow; exact 8/8, near 3/3, composition 8/8 | Это память `executor-en_ru`, не LoRA и не обобщение на независимые семейства |
| P125, диагностический C с обменом действий validate/delete | 13/24 вместо B19/24; изменились шесть ранее правильных ответов | Контроль зависимости ответа от учебного материала, не кандидат улучшения C |
| P25, готовый B checkpoint в новом процессе | Совпали 24/24 наблюдаемых ответа; сохранено 19/24 правильных при 0 новых уроков | Cold означает новый process/owner, не сброс дискового cache |
| Отрицательные края | cc01, cc03, cc04 ошибочно приняты; cc02/cc05 — отказы | Потеря отрицания, связи «и» и вопроса не превращается в правильный перевод |

Внутренние `accuracyBefore/accuracyAfter=0` в training receipt не подменяют оценку 24 ответов отдельным scorer. Любое улучшение полезной метрики — успех именно этой метрики и профиля; рядом сохраняются остальные сдвиги. После обучения 0→0 не выдаётся за успешный учебный тест.

## 2. Что реально пишет и читает состояние

Native запись: `TranslationMachine.TrainStructuredBatchAsync → ExecutorUnit.TrainOnExample → TranslationKnnStore`. Для исходного P125 берутся сохранённые **восемь executor-only примеров**, с explicit `PhraseMappings`, из двух native batch JSON ниже. `BuildGeneralBatch` не заменяет их: генерация semantic/verifier примеров изменила бы обучаемые units и фактор исходного опыта.

Обычный consumer: `TranslationMachine.GetOrchestratorAsync().RunAsync → InlineTranslationOrchestrator → TranslationRoutingFlow.ExecuteAsync → TrainedTranslationAdapter → tokenizer/executor/verifier`. Направление EN→RU и topic=`technical` задаются исходным request metadata; в request нет gold, ожидаемого ответа или выбранного соседа. Явный `TranslationRoutingFlow.ScenarioId` здесь проверяет доменного потребителя и **не выдаётся за обученный выбор домена**.

Persistence: `SaveCheckpointAsync/ExportCheckpoint → новый OS process → TranslationMachine.LoadCheckpointAsync → тот же ordinary Flow`. `ExportAsync/ImportAsync` с placeholder metadata не сохраняют выученные units. Importer восстанавливает сохранённые KNN entries через AddPair; это реконструкция состояния, не новый `TrainOnExample`.

Места native реализации: [TranslationMachine](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/27.TranslationDomain/TranslationMachine.cs), [TranslationTrainedUnits](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/27.TranslationDomain/TranslationTrainedUnits.cs), [TranslationUnitContracts](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/27.TranslationDomain/TranslationUnitContracts.cs), [TranslationRoutingFlow](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/27.TranslationDomain/TranslationRoutingFlow.cs), [TranslationDirectionAdapters](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/27.TranslationDomain/TranslationDirectionAdapters.cs).

## 3. Точные источники, будущие файлы и reuse

Источник всех файлов — закреплённый commit `2297566a5c07b564031337c2b5d4187ca10ca729`, каталог `docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/`. Будущий fixture root — **`Tests/Fixtures/ExperimentFixes/v1/TranslationMemory/`**. Файлы переносятся копиями с паспортом исходных Git blobs и SHA256; старые данные не редактируются.

| Будущий файл | Существующий источник | Использование |
|---|---|---|
| `train.json` | [P125/train.json](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/train.json) | Восемь tr01–tr08, только EN→RU technical |
| `training-batch-B.json` | [P125/training-batch-B.json](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/training-batch-B.json) | Exact native batch; 8 примеров для `executor-en_ru`, 1 вызов Train |
| `training-batch-C.json` | [P125/training-batch-C.json](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/training-batch-C.json) | Уже заданный обмен validate/delete в полном chosen и phrase mappings; 1 вызов Train |
| `eval-inputs.json` | [P125/eval-inputs.json](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/eval-inputs.json) | 24 case IDs и source/language/topic, без gold |
| `panel-gold.json` | [P125/panel-gold.json](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/panel-gold.json) | Frozen scorer: exact8, near3, composition8, edges5; learner этот файл не читает |
| `input-checkpoint.json` | [P25/input-checkpoint.json](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/input-checkpoint.json) | Готовое обученное B-состояние P25 для restore, без acquisition |
| `retained-outputs.json` | [P125/raw-outputs.json](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/raw-outputs.json) | Исторические P125 observations; comparator выбирает ровно B_after, 24 строки |
| `provenance.json` | Новый паспорт этих копий | Исходные paths/commit, hashes, learner, направление, observed limitations |

Исторический checkpoint — 8 930 bytes, SHA256 `125c15ddb400dc00cfddb2367e1ea811917a87fb641c4e58ade22da2bea2e56c`; Git blob `578436c02ca77da9be3f40776d65b89d42298ff8`. Размер и SHA256 относятся к исходным bytes, включая их кодировку/переводы строк; не вычислять их по повторной сериализации JSON. Canonical state P25: `8b03a483d5d32385fb22efdd164f633efc2ae92ca88813ac1f50a88f0c573305`, исключён только `ExportedAtUtc`. Источник этих значений — P25 `input-provenance.json` и `state-receipts.json`; при будущем переносе проверить реальные bytes. При несовместимости — конкретный `missing_or_incompatible_fixture`, без обучения нового донора «взамен».

Acquisition проверяет сам writer один раз на двух независимых свежих B/C; готовый checkpoint не подменяет этот тест. Все остальные assertions читают одну общую неизменяемую квитанцию acquisition, не инициируют новый fit. Restore проверяет существующий исторический donor независимо от нового acquisition: уже готовое состояние не заменяется заново обученным B. Публичная схема checkpoint не получает выдуманной tokenizer/corpus hash validation; внешний контроль fixture не называется имеющейся возможностью importer.


Scorer назначен отдельно от learner: новый `Tests/ExperimentFixes/TranslationMemoryPanelScorer.cs` переносит ровно `normalize/assess` из [P125 score.cjs](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/score.cjs), Git blob `e8b20d471667da397e8d5c59ac63b4e7f947540b`. Правило не выбирает другую LLM: lowercase, ё→е, обрезка только перечисленной в исходнике краевой пунктуации/пробелов и сведение whitespace; затем exact accepted phrase list, action/object/polarity/quantity/second-object/question predicates. `answered` требует native success, verdict TRANSLATED и отсутствие bootstrap/fallback adapter. Отказ не считается верным переводом. Сравнение cold полей берётся из [P25 compare.cjs](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/compare.cjs), Git blob `2eabc5b5d6eedc346abafdb0129bacf9b8c3f100`. Сам scorer предварительно сверяется по уже сохранённым144outputs/scores, без новых Flow/Train calls; он не записывает файлы старого эксперимента. Неперечисленный ответ не принимается по усмотрению исполнителя.

## 4. Точная последовательность и бюджет

Новый файл — `Tests/ExperimentFixes/TranslationMemoryLifecycleTests.cs`, класс `TranslationMemoryLifecycleTests`, одна collection fixture на весь класс. API/DTO и формирование request берутся из исходного P125 harness, но ссылка идёт на текущие native assemblies, не старую DLL и не копию learner.

1. Создать три `new TranslationMachine()` A/B/C с одинаковой штатной конфигурацией. Сравнить полное исходное состояние, нормализовав только `ExportedAtUtc`. Дать каждой машине 24 before запроса через ordinary Flow: 72 вызова.
2. A не обучать; B один раз получает `training-batch-B.json`, C один раз получает `training-batch-C.json`. SourceId=`simulation`, DomainTag=`TranslationDomain`, Unit=`executor-en_ru`; все восемь строк имеют HeldOut=false. При native отказе его сохранить, admission не обходить.
3. По 24 after запроса A/B/C: ещё 72 вызова. Только после сохранения outputs scorer читает gold. Сохранить до/после native state и все ошибочные accepted/отказы.
4. Один child process загружает исторический `input-checkpoint.json` и делает 24 ordinary вызова, не читая TRAIN или gold. Сравнить outputs с историческим B_after, loaded и post-inference state — с исходным checkpoint. Никакой повторной подготовки.

**Общий максимум: 2 memory Train вызова, 16 предъявленных уроков, 144 acquisition/readout + 24 cold readout = 168 ordinary Flow calls; 1 cold load; 0 neural updates, 0 external model calls.** Учебные прогоны shared, а не по одному на каждый test method. Два Train вызова измеряют приобретение; cold leg содержит ноль новых Train. Wall deadline всего runtime набора — 5 минут; build отдельно. Число повторов/шагов не увеличивается для получения PASS.

Новый cold helper задаётся точно: **`Tests.Support/TranslationMemoryColdHost/TranslationMemoryColdHost.csproj`**, `Program.cs`; `OutputType=Exe`, `TargetFramework=net9.0`, `ProjectReference` на `../../27.TranslationDomain/27.TranslationDomain.csproj`. Он находится вне SDK glob основного Tests проекта. Аргументы: `--checkpoint <fixture/input-checkpoint.json> --eval <fixture/eval-inputs.json> --output <result/cold>`; каждый путь формирует parent fixture относительно checkout. Аргументов TRAIN/gold и вызовов Train в helper нет. Он сохраняет PID/start/exit, load result, 24 outputs и до/после state; читатель gold остаётся в parent после выхода child. Native исключение и nonzero exit передаются в результат. Это новые запланированные файлы, сейчас они не создаются и не запускаются.

## 5. Assertions и существующие тесты

| Новый тест | Проверка без дополнительного обучения |
|---|---|
| `P125_EightLessonsChangeOrdinaryFlow` | Общая квитанция: A остаётся без усвоения, B приобретает ненулевой полезный результат; исторический ориентир 19/24 с поштучными x/n/ev/cc. Изменение текущего ответа требует объяснения по неизменному gold |
| `P125_TargetPermutationChangesLearnedActions` | Та же квитанция C: ответ действительно зависит от заменённых учебных действий; исторически x01,x02,n01,n02,ev01,ev02 теряют правильность. Это абляция, не полезный product fix |
| `P125_OrdinaryFlowUsesLearnedEnRuMemory` | По той же квитанции route/adapter и state показывают native memory consumer; confidence или количество entries отдельно не доказывают правильный перевод |
| `P25_FreshProcessRestoresObservedAbilityWithoutTraining` | Ровно один cold child; те же 24 outputs/route/confidence/verdict и canonical state, 0 Train. Из output equality исключаются только arm/phase, elapsed/allocated counters и Summary.summaryId нового RunId, как у P25 |

Исторические ошибочные ответы не становятся желаемым semantic gold. Воспроизведение frozen checkpoint и сохранность состояния — отдельный контракт; если последующее целевое исправление семантики намеренно меняет output, его надо предъявить как изменение контракта с новым доказательством, не переписать ожидаемую фразу молча. Все пять edge cases остаются в отчёте.

В `Tests/TranslationDomainTests.cs` сохранить существующие checkpoint machine/schema checks, quality verifier state, continued-training old/new coverage и отрицательные source-copy/unrelated-KNN проверки. Не заменять их общим `19/24`. `TranslationSeedCorpusTests` и разделение TRAIN/held-out сохраняются. Свежих падений в этой документной работе нет; адресный baseline после назначения переноса определит, какие проверки действительно затронуты.

## 6. Связь с общим E2E

TEST-TMEM даёт проверенный по эксперименту **EN→RU technical** receiver и cold lifecycle. В [FIX-E2E](20-prompt-domain-e2e.md) уже назначен отдельный **RU→EN general** receiver из двух product-b001 строк; данные, направление и бюджеты не смешиваются и checkpoint не подменяется. Новый EN→RU topic/prompt-routing набор здесь не придумывается. Неподтверждённый внешний selector или RU→EN fixture не блокирует независимое покрытие уже измеренного EN→RU пути. Прямой domain scenario TEST-TMEM не засчитывается как успех learned outer routing.

## 7. Будущие команды, отчёт и управление

Из `C:/Projects/llm_mvp_core`, только после создания запланированных тестов/helper:

```powershell
dotnet build Tests.Support/TranslationMemoryColdHost/TranslationMemoryColdHost.csproj -c Release --nologo
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~TranslationMemoryLifecycleTests" --logger "trx;LogFileName=TEST-TMEM.trx" --results-directory artifacts/fbe/TEST-TMEM/attempt-0001/tests
```

Fixture запускает ровно один `dotnet Tests.Support/TranslationMemoryColdHost/bin/Release/net9.0/TranslationMemoryColdHost.dll` с аргументами из раздела4 и ждёт его terminal exit в том же run. Пользователь не ищет host, проект или модель. Result root — `artifacts/fbe/TEST-TMEM/attempt-0001/`; `acquisition.json`, `cold/`, `metrics.json`, русский `report.md`. Если result root уже занят, существующее исполнение выясняется по общему контракту; папка не удаляется, новый suffix автоматически не выбирается.

Модель исполнителя — текущая default без override; обучаемый получатель — native TranslationMachine memory, нейронная конфигурация неприменима. Скилы из `docs/skills`: `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, `human-communication-GLOSSARY.md`; если меняется prompt исполнителя — `prompt-patch-authoring-SKILL.md`. Отчёт на русском по03: обучение и state → все ответы до/после → полезные оси и ухудшения → cold parity → расходы и ограничения. Новые DPO этой карточке не требуются; исторические synthetic fixtures не объявляются новым внешним teacher review.

**STOP/NEXT:** немедленно прекратить прежнюю карточку, не дописывать её отчёт, тесты, commit или cleanup. Сообщить только уже подтверждённое состояние; NEXT — перейти к указанному ID, STOP — никаких новых действий. Живой разрешённый процесс ждать штатным bounded wait того же handle в том же ходе до exit/deadline; не relaunch, не заменять ожидание polling или финальным «жду». Старые эксперименты и результаты не удалять и не перезапускать.
