# 121. Адресный план после P102: проверяющий, исполнение и продуктовые границы

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

Это **исторический срез после P102**. Последующее состояние описано в [аудите 123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md) и [плане 124](codex-124-post107-addressed-repair-plan-2026-11-02.md): R09 terminal metrics уже реализован; сборка Tests исправлена; targeted P85/facade остаётся открытым. Теперь 107 записанных/23 будущих, nextQ108. Остальные требования ниже сохраняются в незакрытой части; старые даты, бюджеты и результаты не переписаны.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


Основание: [аудит120](120-post102-transfer-and-executor-audit-2026-11-02.md), core `15745b357e055055ee9bc58b52cc374d90580961`. Статус: **PLANNED_PRODUCT_WORK / NOT_EXECUTED**. Документная интерпретация исправлена сейчас; перечисленные ниже изменения runtime не сделаны. Ни один пункт не запускается наличием этого плана. Исходные дозы/Q сохраняются; исправление saved evidence не даёт разрешения повторить обучение.

## Приоритет и порядок

Сначала сделать достоверной приёмку и безопасным восстановление попытки (R01–R04). Затем закрывать продуктовые потери данных и ошибки допуска (R06–R09), параллельно по разрешённым независимым назначениям. R05 — ограниченная native приёмка уже реализованного P85. R10 — отдельная scoped-expression граница. R11 удерживает эти правила в задании исполнителю. Старые118/RF остаются источниками подробных fixtures; этот план уточняет их, не создаёт повторные FIX/PRIMARY.

## R01. Один проверяющий текста и полный decoder receipt — приоритет P0

**Владелец:** экспериментальная оснастка `experiments-v2/RP01-MICRO/P10201/src/P102TrainingStepBridgeCopy.cs` и будущие host/evaluator helpers. Product `09.Inference/TextGenerator.cs` — только для отдельного согласованного добавления actual-token receipt, без скрытой смены генерации.

1. Выделить именованную чистую функцию `(prompt, actualText, goldFacts, stopInfo) → verdict`; обе процедуры, калибровка и приёмка, вызывают её. Exact text отделить от фактов stdout/postfix, contradiction и completeness. Для ограниченной рубрики задать поддерживаемые формулировки; вне неё UNJUDGEABLE сохраняется в знаменателе.
2. Положительный canonical и законный пересказ, отрицательные10/120/new-value/противоречащий хвост/empty/truncated подать через реальный callable. Порча текста обязана менять его verdict. Не использовать bool-vs-bool вместо проверки входа.
3. Добавить исправляющий receipt для четырёх сохранённых текстов, со ссылкой на неизменённый historical attempt. Ожидаемые факты TRAIN correct, EXAM numeric wrong проверяются по независимой программе, не по model output.
4. Перед будущим запуском проверить cap против canonical IDs; собрать actual chosen IDs, decoded text, reencoded IDs и stop reason раздельно. Исторический cap46 не менять. Полный повтор чтений от S0/S1 требует отдельного bounded continuation с дополнительными0Train; без него поля actual trace остаются UNKNOWN.

**Красная проверка:** gold с противоположным хвостом на старом StartsWith ошибочно true; новый evaluator false. **Удержание:** верный пересказ проходит смысл, canonical остаётся точным. **Готово:** calibration и финальная сборка результата используют одну функцию, mutation обнаружена, все существующие ответы повторно оценены без обучения. Это исправляет оценку, а не результат EXAM модели.

## R02. Одна попытка, подтверждённая стадия и восстановление — P0

**Владелец:** `P10201/Run-Experiment.ps1`, admission в bridge; общий run-binding контракт `experiments-v2/_runners/run-binding/` после проверки реально используемых entry scripts. Не придумывать отсутствующий Invoke-BoundRun.ps1.

Заменить «если нет успешного exit, можно начать» на terminal ledger exact ID: process identity, stage, issued actions, saved state, exit/timeout. Атомарная claim не получает новый суффикс для обхода занятого ID. Живой/неизвестный процесс требует ожидания исходного handle. Ненулевой exit допускает повтор только при доказанном PRE_MODEL и разрешённом host repair. После UPDATE_ISSUED/STATE_SAVED восстановление читает тот же S1 без нового Train. Таймер ограничивает конкретный child, receipt подтверждает остановку; комментарий600s не считается контролем.

**Дешёвая приёмка:** dummy child без модели увеличивает persistent counter вместо update. Проверить active claim, pre-model failure, post-counter/report failure, stale/unknown PID, timeout и повтор запроса. Во всех поздних случаях counter остаётся1; ранний ремонт не стирает цену первой попытки. Тест должен вызывать настоящий admission runner, не отдельную копию условия. **Готово:** повтор не вызывает оплачиваемый callback, terminal receipt достаточен для восстановления, all-attempt cost ledger сохранён.

## R03. Реальные входы сборки и потребления — P0, продолжение RF02

**Владелец:** `experiments-v2/_runners/run-binding/New-RunBinding.ps1`, reader соответствующего runner; P102 `Run-Experiment.ps1` и `src/ContinuousLearning.csproj` как конкретный failure witness.

Собирать evaluated project instance `(path, effective properties)` с действительными Compile, imports, props/targets и reference closure. Подписать именно snapshot, который получает compiler/child. Manifest-only rehash не доказывает отсутствие изменения между проверкой и открытием. Стратегию immutable snapshot или отказа reader при изменении реализовать явно; внешние зависимости и toolchain versions именовать отдельно.

**Красная проверка:** исключить прямо подключённый `CodeDocumentationGenerationConfiguration.cs`; изменить обязательный Compile Include после preparation; один csproj оценить с двумя различными properties. Должны получиться недопуск/разные instances, а не общий PASS по одной подписи пути. Удержать уже исправленные extra/optional drift случаи. Test child действительно читает fixture bytes и сообщает digest; тест проверяет receipt этого reader. **Готово:** ни один фактически читаемый обязательный input не отсутствует, неизвестный input не становится0/ignored. Новое модельное обучение для проверки не требуется.

## R04. Исправить смысл отчёта и цену попыток — P0, без Train

**Владелец:** отдельный correcting evidence record для P102 и summary/registry, будущий reporter. Исторические report/raw остаются исходным документом.

Раздельно именовать TF position accuracy, free prefix length, independent semantic verdict, NLL, actual/reencoded IDs. Исправить changed-weights→nonzero-gradients и «только два Backward→остальные logits без сигнала». Inventory указывать вместе с конкретным массивом: argmax4/6 и best-unnamed1/9 не смешивать. Optimizer moments после step1 обозначить NOT_MEASURED, пока payload не прочитан и не привязан к checkpoint.

All-attempt ledger включает6build/4runtime; успешные109.939367s/32 188 097 352allocated bytes не переименовывать во всю сессию. Неизвестные общие CPU/peak/allocations не восстанавливать выдуманным нулём; cumulative allocations не равны peak. Отдельно сохранить source-derived192 calls и доказанную PRE_MODEL границу трёх отказов. **Приёмка:** пересчёт из saved JSON и логов, точные определения/знаменатели, raw unchanged. Документная часть этого пункта выполнена аудитом120; runtime reporter остаётся будущим ремонтом.

## R05. Принять уже внесённый P85 через настоящий проект — P1

**Владелец:** `12.SemanticCompiler/Prediction/MethodKnn.cs`, `12.SemanticCompiler/SemanticCompilerFacade.cs`, существующий `Tests/ExperimentFixes/MethodKnnTieIsResolvedByDistanceTests.cs`.

Не переписывать уже добавленный distance tiebreak. Выполнить существующие targeted tests обычным test project/toolchain; добавить один ordinary facade witness с UseMethodKnn=true, CSharp, достаточным native training size и вызовом реального метода-потребителя. Тест должен достигать predictor, а не keyword ClassifyDomain. Сохранить current-label last-in-alphabet, k1/k5 и24 untied retention cases из oracle.

**Красная проверка:** удалённый distance ThenBy возвращает прежний неверный ответ; farthest mutation тоже падает. Большинство голосов остаётся большинством; при полном равенстве допустим объявленный ordinal fallback. Другая граница равных расстояний на cutoff и sibling StructuralKnn/FileActionKnn не чинится автоматически. **Готово:** точные native commands/exit и ordinary consumer evidence на конечных bytes. Сейчас35/35 — сохранённое компонентное свидетельство, native_acceptance=NOT_EXECUTED. Повтор исходного обучения P85 не нужен.

## R06. Атомарная партия Translation — P1, RF01

**Владелец:** `27.TranslationDomain/TranslationMachine.cs` (`TrainStructuredBatchAsync`, preflight, heldout evaluation), состояние trained units/history/version/readiness.

Уже есть syntax preflight учебных строк. Остаток: проверить все используемые DTO, включая heldout, и обеспечить общую транзакцию state→metrics→history→version. Один preflight не защищает от позднего исключения Evaluate/save. Нужен staged state/clone или эквивалентный rollback всего набора, затем единственная публикация snapshot; runtime readers должны видеть согласованную версию. Сначала описать ownership/serialization и только затем выбрать механизм.

**Красные проверки:** новый законный урок + malformed heldout; JSON42 вместо ожидаемого DTO; поздний отказ evaluation; поздний отказ persistence. После каждого rejected batch hash памяти/identity/version/history/readiness и ответы равны S0. После законной партии новый урок виден ровно один раз и в live/fresh child state. Q129 исследует только syntax-heldout boundary, не доказывает всю транзакцию. Дозу его4batch/4lessons не расширять. **Готово:** test использует public batch entry и ordinary Execute, а не одну preflight функцию.

## R07. Допуск кандидата до cap и выбора tier — P1, RF06

**Владелец:** `TranslationUnitContracts.cs` / `TranslationKnnStore.FindNearestCascade`, `TranslationTrainedUnits.cs` / `ExecutorUnit.TrySelectKnnCandidate` и его caller.

Собрать общий admission по направлению, target law и SourceTokenSimilarity до top limit и до решения «tier найден». Не прятать minimum-source threshold после окончательного выбора. Logical lesson identity и повторные signature/prefix/structure записи считать отдельно; dedup не заменяет admission.

**Красные проверки:** первые5 кандидатов незаконны, шестой законен; полный tier имеет source0.50, проходит store0.35, но не caller0.60, а prefix кандидат1.0 законен. Должен быть найден законный кандидат. Панель обходит exact/word/phrase early returns с явным path receipt; иначе NOT_REACHED. Все-invalid сохраняет честный отказ. «Фиолетовый фонарь сейчас светится» с отсутствующим now в сохранённых targets не превращать в полный semantic PASS от одной доступности KNN; gold и уроки не подстраивать. **Готово:** ordinary translation output соблюдает все условия, существующие exact positives удержаны.

## R08. Typed decode и многотокенный UTF-8 — P1

**Владелец:** `01.Tokenizer/BpeTokenizer.cs` (Decode), `09.Inference/TextGenerator.cs` и фактический caller строгого результата.

Явно разделить legacy tolerant decode и strict result с неизвестными IDs, остатком UTF-8 и статусом completeness. Выбрать call-site политику, довести её до greedy и stochastic ветвей и итогового выдаваемого ответа. Не изобретать EOS, не заменять весь vocabulary ASCII диапазоном; русские буквы и многобайтные символы могут собираться из нескольких tokens.

**Красные проверки:** unknown ID между корректными tokens не должен исчезать без статуса даже если итоговый текст совпал с gold; валидная русская многотокенная последовательность проходит; незавершённый suffix при LENGTH_LIMIT явно обозначен; invalid candidate не выигрывает обходным greedy argmax. **Готово:** настоящий пользователь strict consumer различает правильный перевод и потерю токена. P99 не объявляется нейронной проверкой: его recorded scope другой. Отдельные exact tests могут использовать сохранённый обученный tokenizer без повторного BPETrain; новая PRIMARY следует своей дозе.

## R09. Конечные Wiki метрики отдельно от средних по обучению — P1

**Владелец:** `42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs`, `CrossStackBusinessFlowGraphTrainingPipeline.cs`, `CrossStackBusinessFlowGraphManifestPublisher.cs`.

Сохранить корректное pair-weighted averaging как TrajectoryAverage*. Для Final* либо измерить конечное состояние на заданной frozen panel, либо оставить unknown; переименование mean в final недопустимо. Publication gate должен потреблять нужную terminal metric с происхождением и panel ID, совместно с уже обязательным heldout. Compatibility/serializer change планировать явно, старые отчёты не переписывать как новые измерения.

**Красная проверка:** trajectories с одинаковым weighted average, но разным final checkpoint должны давать разные terminal verdict при пересечении порога. Отсутствующая final observation блокирует её сравнение с0.75, а не превращается в0. Final truth и test expectation не вычислять тем же averaging helper. **Готово:** trace от endpoint reader до gate, nullable/typed missing, сохранённые trajectory значения без потери истории.

## R10. Expression body scoped-target и общий tie policy — P2

**Владелец:** `MethodKnn.ExtractOuterBodyKindBag`, `TrainWithTargetScope`; действующий Q130. Current/input extraction и Next/target extraction разные роли. Действующий ordinary expression fix P83 не закрывает Body=null в scoped target.

Единственный фактор — projection внешнего тела целевого метода. Expression body читается без включения вложенных lambda/local function; не добавлять искусственный Ret ради нужной метки. Новое distance tie правило P85 общее в A/B; majority panel Q130 не изменять в скрытый tie experiment. **Приёмка:** назначенные4pairs,4fits/32pair presentations/32Predict, k1/k5, ожидаемые2/4→4/4 пока прогноз; вложенные callable controls остаются. Компонентная acquired-pair точность не означает unseen generalization или restore, которого API не предоставляет.

## R11. Применять навыки через проверяемое задание — P0, документация готова

**Владелец:** [40](../experimets/40-effective-executor-context-2026-11-02.md), start prompt,34/39, transfer02/03/04/90, адресные карточки. У каждой будущей работы до входа есть owner/caller, один factor, named state, полный бюджет, shared scorer, WRONG/CORRECT и реакция на failure. Загруженные роли skills связаны с конкретными решениями; процент строк не объявляется качеством review.

**Проверка:** следующий исполнитель по своему exact ID объясняет один ожидаемый положительный пример и один ложный PASS, называет actual consumer и recovery after update. Если этого нет, пакет не готов, даже при всех заполненных полях. Исправление источника/теста не означает ослабление gold; отрицательный научный результат сохраняется. Эта проверка не создаёт новую очередь и не разрешает автоматический запуск Q104.

## R12. Согласовать дополнительную область P103 — P0 для отчётности, без Train

**Источник:** concurrent c8f3908f и отдельная [входная проверка](../../../../experiments-v2/_audits/2026-11-02-post102-executor-audit/p103-concurrent-intake.md). **Владелец:** supplemental evidence/summary P103; будущий host admission и cost ledger. Основное A/B0→3получателя и0/3→3/3named сохраняется.

Не называть весь host исполненным ровно вPRIMARY: показаны29observations,22offered/19admitted lesson rows,15непустыхUnit.Train,1rejected empty call и1standaloneBPEfit. Вложенные15BPEfits и18source-derivedExecute учитываются отдельными осями. Перед будущим оплачиваемым вызовом проверяется остаток общего budget; дополнительные «диагностические» fits не становятся бесплатными. R02 отдельно реализует фактический deadline, не только его печать.

Исправить причинное утверждение о forgetting: два разных factory results с размерами3/1 не являются before/after одного обученного состояния. Для будущей проверки continuation сначала назвать реальный API/owner/state identity; при отсутствии такого API зафиксировать CONTRACT_ABSENT, не создавать второй singleton как якобы восстановленный первый. Текущий отчёт уточняется по source без дополнительных fits; новая continuation программа этим пунктом не назначена. Проверка готовности исправления — identity lineage и полный счётчик сохранённых действий, а не желательный ответ после нового обучения.
