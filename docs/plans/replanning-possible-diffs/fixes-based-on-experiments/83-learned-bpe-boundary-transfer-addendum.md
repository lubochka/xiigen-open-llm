# 83. Учебная граница BPE → DPO: дополнение FIX-PD, FIX-TS и FIX-SEP

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски BPE/LEARN:** У BpeTokenizer.Train отдельно requested/achieved/merges/reason и настоящий читатель outcome; законный roundtrip сохраняется, shortfall не скрывается. Все вложенные и twin BPE fits входят в полный бюджет; больший request не доказывает больший достигнутый vocab. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Граница оцениваемого ответа и фактические tokens

**Проверка `POST102-FIX-PD/TS/SEP`. WRONG.** Decode каждого BPE token или повторное Encode текста восстанавливает actual sampled sequence и response boundary.

**CORRECT.** Byte-span oracle остаётся независимым от per-token Decode; true sampled IDs, reencoded IDs и source bytes — разные представления. Совместимость обученного tokenizer, assessment mask и downstream reader должна быть явной.

**Адресная приёмка.** Удержать learned boundary merge witness и проверить actual assessed rows. Для генерации дополнительно проверить canonical cap и UTF-8 prefix/terminal policy в настоящем caller; не вводить ASCII whitelist. Новое обучение для исправления отчёта не требуется, исходный переносу назначенный fit сохраняется.

Применять [единый контекст40](../experimets/40-effective-executor-context-2026-11-02.md) и [адресный план121](121-post102-addressed-repair-plan-2026-11-02.md): актуальные роли навыков, один рабочий оценщик, ordinary consumer и реакция на test/report failure. Это ограничивает вывод и усиливает приёмку; исходные factor, доза, fixed panel и порядок сохраняются. Исправление saved evidence не является новой PRIMARY, а наличие этого блока не разрешает запуск. Исторические указания ниже действуют только в своей исходной области; при противоречии бюджета/фактора требуется адресное решение до исполнения.
<!-- POST102-AUDIT-2026-11-02:END -->


Дата: 27 октября 2026. **PLANNED / NOT_RUN.** Основание — положительный P07-PRIMARY-01/P0701, commit `797dfb5702c`, с уточнениями [аудита82](82-positive-transfer-delta-audit-2026-10-27.md). Это адресное усиление существующего переноса и тестов, не новый FIX и не разрешение начать выполнение. Исторические hosts/results не редактируются.

## 1. Что именно переносится

P07 измерил после одного настоящего BPE Train сохранение 1 оцениваемой позиции у раздельного продуктового представления против 0 у совместного контрольного представления. Поведение продукта уже верно. **Добавляется пример с выученным слиянием на границе, независимый байтовый эталон и связанная проверка фактической целевой позиции в продуктовых тестах.** Это первая очередь защиты логики измерения и подготовки учебного входа; прирост качества LLM, ресурсы и улучшение маршрутизатора по P07 не заявляются.

| Файл | Будущее действие | Что уже существует |
|---|---|---|
| `Tests/DpoExactPairMaterialTests.cs` | Усилить `Segmentation_DoesNotMergeAcrossAssessedBoundaryOrAddSeparator` единым обученным набором ниже; добавить проверку target row и восстановленного tokenizer | Настоящий BPE Train и проверка отсутствия boundary merge уже есть,49–58 |
| `Tests/ExperimentFixes/TokenByteSpanOracle.cs` **планируемый** | Только test helper: source UTF-8 gold + длины byte[] из canonical Vocab, без per-token Decode | Не объявляется существующим API |
| `Tests/ExperimentFixes/SupervisedResponseReadout.cs` **планируемый владельцем62** | Использовать тот же helper, когда нужна диагностика границы; проверять actual IDs и профиль до метрик/stop | Сохраняются все обязанности62, включая unresolved boundary |
| `11.DPO/Models/DpoExactPairMaterial.cs` | Сохранить private constructor, snapshot и separate Encode, exact Match масок | Новая runtime-проверка только суммы маски не назначена: обход существующих guards P07 не показал |
| `01.Tokenizer/BpeTokenizer.cs` | Использовать текущие Train/Save/Load; не вводить новый формат состояния или публичный доступ к mutable Vocab | Canonical Save уже содержит Base64 байтов токенов |

Все пути от корня `C:/Projects/llm_mvp_core`. Владелец реализации теста — [22/FIX-PD](22-dpo-valid-length.md). [28/FIX-TS](28-dpo-tokenizer-snapshot.md) и [62/FIX-SEP](62-supervised-framing-and-learning-readout.md) ссылаются на **тот же receipt**, а не запускают ещё одну копию программы.

## 2. Точный учебный набор, состояния и домены

Реально обучаемая единица — native BPE merge table. Новый tokenizer до Train; записать initial state/hash и IDs для`ab`,`a`,`b` (3Encode). Не вызывать exact materialization на неинициализированном before: его Decode ещё пуст, это не годный donor.

Один `BpeTokenizer.Train("ab ab ab ab",257)`. Проверить единственную merge(97,98)→256, vocab257, ненулевое приобретение merge0→1. Если это не выполнено — `INVALID_LEARNING_FIXTURE`, без подбора другого корпуса/дозы. После обучения сохранить один frozen snapshot и переиспользовать его для всех строк A/B. Нейронная модель/LoRA/optimizer/RNG не создаются: тест касается обученной подготовки данных для настоящего DPO consumer, а не качества нейронных ответов.

| Строка | Prompt | Chosen | Rejected | Независимый gold после Train |
|---|---|---|---|---|
| Основная P07 | `a` | `b` | `c` | Segmented chosen `[97,98]`, rejected `[97,99]`; response mask `[0,1]`; joint chosen `[256]` не имеет отдельного next-token ответа |
| ASCII-контроль | `x` | `y` | `z` | Segmented `[120,121]`/`[120,122]`,mask`[0,1]`; joint не пересекает границу |
| RU byte-контроль | `а` | `б` | `в` | UTF-8 prompt`[208,176]`,chosen`[208,177]`,rejected`[208,178]`; response mask`[0,0,1,1]` |
| HE byte-контроль | `א` | `ב` | `ג` | UTF-8 prompt`[215,144]`,chosen`[215,145]`,rejected`[215,146]`; response mask`[0,0,1,1]` |

RU/HE строки — **новые защитные случаи**, ещё не выполненные и не доказательство качества перевода/грамматики. Общая тема — граница учебного материала; эти строки не учат предметным темам разных доменов. Единственное обучение BPE остаётся исходным ASCII TRAIN, чтобы отдельно выявить ошибку per-token UTF-8 decoding на ещё не слитых byte IDs.

Данные берутся из literal таблицы и сохранённых [gold](../../../../experiments-v2/RP01-MICRO/P0701/results/attempt-0001/gold-oracle.json), [trained state](../../../../experiments-v2/RP01-MICRO/P0701/results/attempt-0001/tokenizer-trained-state.json), [untrained state](../../../../experiments-v2/RP01-MICRO/P0701/results/attempt-0001/tokenizer-untrained-state.json). Не менять файлы опыта. Будущий output: `artifacts/fbe/FIX-PD/P07-boundary/attempt-0001/`, в нём `tokenizer-before.json`, `tokenizer-trained-state.json`, `manifest.json`, `rows.json`, `report.md`. Существующий attempt не перезаписывать: выбрать первый свободный `attempt-NNNN`, записать путь один раз, не удалять прежний.

Manifest фиксирует TRAIN literal/hash, tokenizer hash, Vocab/merge, профили`prompt-response-segments-no-separator-v2` и диагностический joint, expected byte spans, git HEAD, команду, фактические счётчики. Для следующих владельцев восстановить этот snapshot и receipt; отсутствие/несовместимость — явный отказ потребителя, а не скрытый повтор обучения.

## 3. Проверки одного receipt и причинная связь с обучением

1. До/после Train подтвердить изменившуюся table и появившуюся merge. Состояние не создаётся вручную из списка готовых merges. A/B читают один learned snapshot.
2. Для каждой строки B получить через существующий test helper `Batch` настоящий `DpoExactPairMaterial.CreateBatch`; A — только диагностические `Encode(prompt+chosen)`/`Encode(prompt+rejected)`. Не отправлять A в обучение, не подделывать admission и не ослаблять production вход.
3. Gold — исходные UTF-8 bytes/диапазон ответа из literal таблицы. Byte lengths токенов брать из Base64 Vocab snapshot. Сумма длин обязана равняться исходной длине, восстановленные token bytes — исходному тексту. Отдельный `Decode([id])` не используется для длины: неполный UTF-8 byte может стать U+FFFD. Совпадение Decode всей строки оставить дополнительным, недостаточным условием.
4. Для native B проверить **точные** IDs,PromptLength,ChosenLength,RejectedLength,response masks, source spans и target count. Для token position j,где responseMask[j]=1, обучающий target — IDs[j], строка logits — j−1; j≥1. Для ASCII пары единственный target `(row0,token98)`; для RU chosen — `(row1,token208),(row2,token177)`. Получить indices из независимых gold bytes, а не из возвращённых масок.
5. Связать существующий native score helper `Score(...,sum:true)` класса теста с детерминированным tensor logitsV257: все0, у ожидаемого target на ожидаемой row значение1. Независимая сумма для ntargets равна `n*(1−log(exp(1)+256))`. Проверить с tolerance1e−5; нет Forward/Generate и нет выдуманной accuracy. Существующий finite-difference test остаётся отдельной проверкой derivative, не переписывается под новый код.
6. Проверить, что joint chosen`[256]` в основном случае даёт0 чистых response targets и потому **непригоден для response-score/stop**. Нельзя записать его как правильный пустой ответ или удалить проблемную строку из знаменателя. Native B имеет1target,контроли1/2/2; это квалификация learned input, а не требование случайному LLM ответить правильно.
7. Один native Save/Load этого learned tokenizer. Повторно материализовать четыре B-пары; IDs, masks,spans,hashes совпадают. Повторного Train нет. Восстановленный результат используется28/62, не три разные fixtures.

Математическое ожидание не подменяет результат: до запуска все новые asserts — `NOT_RUN`. Старый ASCII P07 подтверждён только в своём scope. Неподтверждённые RU/HE числа не заносятся в таблицу измеренного улучшения.

## 4. Как обращаться с падающими тестами

В этой сессии падения **не запускались и не установлены**. Сначала один baseline существующего selector, после правки — один rerun. Сохранить имена/expected исходных `Masks_IncludeFirstAndLastResponseExcludePromptAndRightPadding`, `ResponseSumAndBackward_MatchIndependentFiniteDifferencesWithPromptAndPaddingExcluded` и `Commitment_IsStableAndBindsRawSegmentsAdmissionAndTokenizer`.

Если изменился expected ID/маска — сверить frozen tokenizer и framing, не переписать gold по actual. Если старый тест продолжает сравнивать только Decode, добавить byte/target assertions вместо удаления старого условия. Если новый oracle не сходится на RU/HE, ремонтировать test helper, не менять native tokenizer или считать строку «неподдерживаемой». Изменение продукта допустимо только по отдельно доказанному дефекту и назначению; этот план такого дефекта не установил.

## 5. E2E prompt→domain→model и сохранение границ

В существующих учебных E2E [20](20-prompt-domain-e2e.md) / [74](74-knn-domain-constructor-transfer.md) из уже выбранного domain profile передавать связанную identity: model/checkpoint,tokenizer hash,framing version и functions catalogue version. Новая диагностика получает **реальный tokenizer выбранной модели**, а не глобальный default или артефакт соседнего домена. Проверить на уже полученных материалах, что после restore identity и response target map прежние; это дополнительные assertions существующего прогона, не новый бюджет вызовов.

У KNN-only профиля, не потребляющего BPE/DPO carrier, эта проверка неприменима: не добавлять ему чужой tokenizer ради галочки. Неизвестный/неоднозначный route сохраняет отказ без создания другой модели; full configured restore KNN остаётся открытым у74/80. P07 **не** доказывает обученность маршрутизатора, качество доменных функций или выбор RU/HE маршрутов. Их допускают только собственные KDR evidence и native учебные E2E.

## 6. Команда, бюджет и промпт исполнителю

После отдельного поручения на реализацию из `C:/Projects/llm_mvp_core`:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~DpoExactPairMaterialTests.Segmentation_DoesNotMergeAcrossAssessedBoundaryOrAddSeparator|FullyQualifiedName~DpoExactPairMaterialTests.Masks_IncludeFirstAndLastResponseExcludePromptAndRightPadding|FullyQualifiedName~DpoExactPairMaterialTests.ResponseSumAndBackward_MatchIndependentFiniteDifferencesWithPromptAndPaddingExcluded|FullyQualifiedName~DpoExactPairMaterialTests.Commitment_IsStableAndBindsRawSegmentsAdmissionAndTokenizer" --logger "trx;LogFileName=boundary.trx" --results-directory artifacts/fbe/FIX-PD/P07-boundary/attempt-0001/tests
```

Для baseline использовать отдельную подпапку`baseline`/имя`baseline.trx`; для rerun —`tests`/`boundary.trx`, с выбранным свободным attempt.0discovered не PASS. Команда подготовлена, сейчас не выполнялась.

**Новый связанный набор за один запуск:** 1 Train; 8 exact materializations (4 после Train + 4 после Load); максимум 35 Encode (3 before + 12 native + 8 joint + 12 native после Load); 0 neural updates, 0 model Forward, 0 Generate, 0 teacher calls. Внутренние Encode считать отдельно по исходнику, если не инструментированы. Save/Load внутренних snapshot также учитывать отдельно от внешнего одного restore; не выдавать их за 0. Score по искусственным logits — алгебраическая проверка, не model Forward. Один before/after output, один русский отчёт. Старые companion tests и baseline имеют собственные фактические Train/Encode и не прячутся в бюджет нового набора. Предел одной сборки — 600s, нового набора — 120s; timeout означает terminal partial с тем же handle, без повышения дозы или скрытого retry.

**Промпт:** полный свежий вход — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1), роли — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2), самопроверка — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Полная карточка — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/83-learned-bpe-boundary-transfer-addendum.md; нижний список навыков её дополняет. Документальное поручение не разрешает исполнение нижней команды; модель текущей сессии по умолчанию; обязательны AGENTS/CLAUDE и skills `architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, а также `human-communication-GLOSSARY.md` из`C:/Projects/llm_mvp_core/docs/skills`. Прочитать82 и83 полностью, затем existing tests/source по таблице. Работа ограничена назначенными будущими test/helper изменениями, без старых hosts/results. Отчёт по-русски: что обучалось;точные до/после;A/B по каждой строке;все ухудшения/неизмеренное;счётчики;restore;diff;незакрытые проверки.

WRONG: «Decode совпал, обучение исправлено»;«P07 доказал отсутствие guards»;«у обоих0target, значит parityPASS»;«норма решит знаковую проблему P06»;«все домены стали лучше». CORRECT:1BPETrain,сохранённая merge,байтовый gold,ненулевые native targets,точные masks/shift,один общий snapshot;у RU/HE до запускаNOT_RUN. Любое измеренное полезное улучшение признаётся по своему показателю/домену, рядом показываются все ухудшения; точность LLM не обязательна для выигрыша подготовки входа.

Ожидать исходную живую операцию штатным wait того же handle порциями≤60s; не завершать ход вместо ожидания, не запускать polling loop или вторую копию. **Прямой STOP/NEXT немедленно прекращает прежнюю задачу, включая отчёт/commit/cleanup.** После штатного terminal-результата продолжение определяет живой индекс и прямое назначение пользователя; эта карточка не разрешает застрять на старом опыте или расширить программу.
