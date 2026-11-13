# P131. Учим четыре отрицательные фразы и проверяем запоминание и перенос

<!-- POSTQ011-2026-11-04:START -->
## POSTQ011 — 2026-11-04: короткая дельта к 149/155, исходный результат сохранён

**Основание:** [аудит POSTQ011](../../fixes-based-on-experiments/codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md) и [адресный план](../../fixes-based-on-experiments/codex-159-postq011-addressed-repair-plan-2026-11-04.md). Срез исходников `45bd4e371f511e29c053f8117d61f43365abb460`. Это уточнение действующего владельца и приёмки; очередь, статусы и исторические результаты сохранены. Текущая нативная приёмка — **UNRUN**; документный аудит не запускал обучение.

**Повторная read-only сверка фактических bytes.** Все 128 уникальных raw `(arm,id)` совпали 128/128 с сохранёнными native-flow по actual/verdict/flow/metrics/warnings/adapter/confidence/state version. N 0/4→4/4, P 8/8, H 0/4 и X 0/4; после reader эти выводы сохранены. Новых Train/Flow/Load не было. Это независимый JSON пересчёт; он не исполнял native checker и не закрывает его приёмку.

**Остаток E131-01 / R131-04 / R154-C.** [SavedEvidence.cs](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/experiments-v2/RP01-MICRO/P13101/SavedEvidence.cs) читает raw без native-flow; [Scoring:39–66](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/experiments-v2/RP01-MICRO/P13101/Scoring.cs#L39-L66) связывает actual с производной length и writer/reader parity. Согласованная подмена actual+length в B.after/B.restored потому требует назначенного NATIVE_OUTPUT_BINDING, а не ещё одной проверки производной копии. Existing exact gate IDs и Terminal writer сохраняются: проблема в составе semantic gates, не в полном отсутствии finalizer.

**Приоритет ремонта остаётся прежним.** Сначала отделить immutable historical producer identity/schema от новой verifier DLL/gates: [Provenance:79–85](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/experiments-v2/RP01-MICRO/P13101/Provenance.cs#L79-L85) и `SavedEvidence:49–56` иначе отвергнут честный архив после rebuild. Затем обязательный native binding и honest 0 / coherent actual+length mutation nonzero / honest 0 через настоящий outer exit. Scientific-negative, согласованный с настоящим native output, должен оставаться VALID. Исторические proofs не переподписывать новой DLL; missing archive оставляет NOT_PROVEN.

В существующем VerifyOnly использовать абсолютный `InputRoot`, заканчивающийся `attempt-0001/continuation-0001/evidence`, и отдельный новый абсолютный VerificationOutputRoot по 155. [Runner:155–168](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/experiments-v2/RP01-MICRO/P13101/Run-Experiment.ps1#L155-L168) вычисляет absolute paths для проверки overlap, но передаёт child исходные arguments при cwd=hostRoot. Неверный parent должен дать nonzero outer exit. Относительный корректный путь нужно нормализовать перед передачей child либо явно отвергнуть по документированному API; случайная смена смысла пути при cwd=hostRoot недопустима. Назначенные Checks+Probe+host build/discovery после последней source-правки сохранены. Current native acceptance — UNRUN; повтор PRIMARY или новая модель не требуются.
<!-- POSTQ011-2026-11-04:END -->

<!-- POSTQ003-2026-11-03:START -->
## Адресное уточнение после Q003 — P131-EXACT-EVIDENCE-ROOT

**Установлено / область:** В150 InputRoot был задан родительским continuation, но SavedEvidence читает data/source-proof/raw непосредственно под указанным корнем.

**Действие существующего владельца:** Назначить input_root=experiments-v2/RP01-MICRO/P13101/results/attempt-0001/continuation-0001/evidence. Разделить attempt root, producer archive и новый verification output по155§4. Сохранить49: архив originals→сборка Checks/Probe и host→discovery14 и новых tests→native outer binding. При фактическом вызове оба пути InputRoot/VerificationOutputRoot заранее преобразовать в абсолютные: дочерний процесс меняет WorkingDirectory на hostRoot и получает исходные аргументы.

**Различающая приёмка:** Положительный контроль: полный подлинный evidence/archive достигает необходимых 128 bindings. Отрицательный: parent continuation без /evidence отвергается по layout с nonzero exit без Train/Flow/Load. Отсутствующие historical originals не изготавливаются новой сборкой.

Обязательны [договор50](../codex-50-postq003-binary-and-historical-evidence-contract-2026-11-03.md) и [адресный план155](../../fixes-based-on-experiments/codex-155-postq003-addressed-repair-plan-2026-11-03.md). Это уточняет приёмку и сохранность; собственные factor, данные, gold, исторические budgets и очередь не меняются. Новый PRIMARY/Train для ремонта документа или bytes не назначен. Native Load/технические проверки учитываются отдельно только в собственном назначенном ремонте; их отсутствие обозначается UNRUN.

### Обязательная часть передаваемого поручения

> P131-EXACT-EVIDENCE-ROOT: Назначить input_root=experiments-v2/RP01-MICRO/P13101/results/attempt-0001/continuation-0001/evidence. Разделить attempt root, producer archive и новый verification output по155§4. Сохранить49: архив originals→сборка Checks/Probe и host→discovery14 и новых tests→native outer binding. При фактическом вызове оба пути InputRoot/VerificationOutputRoot заранее преобразовать в абсолютные: дочерний процесс меняет WorkingDirectory на hostRoot и получает исходные аргументы. Положительный контроль: полный подлинный evidence/archive достигает необходимых 128 bindings. Отрицательный: parent continuation без /evidence отвергается по layout с nonzero exit без Train/Flow/Load. Отсутствующие historical originals не изготавливаются новой сборкой. Передай exact attempt/path/donor authority, фактические commands/exit и предел проверки. Сохрани исторические результаты; повтор обучения ради получения отсутствующего файла не выполняй.
<!-- POSTQ003-2026-11-03:END -->


**Обязательное дополнение к текущему наряду — `P131-CHECKER-BUILD`.** Сначала сохранить и проверить original producer source/DLL/PDB до любой пересборки. Затем реализовать совместимость producer/verifier и NATIVE_OUTPUT_BINDING. После последней source-правки по [49 §5.1](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md#51-отдельная-сборка-и-тесты-проверяющего-p131) собрать отдельные `Checks/Checks.csproj` (с Probe) и `Experiment.csproj`, провести discovery и проверки всех14 существующих FQN плюс новых случаев. Общий Tests проект их не содержит, а VerifyOnly не строит host. После любой дальнейшей source-правки нужна соответствующая новая сборка и проверка. Только затем новый verifier вызывается через существующий VerifyOnly с новым output. PRIMARY/ResumePreparation для сборки не вызывать; отсутствие producer originals не лечится новой DLL. Приёмка остаётся NATIVE_UNRUN до фактических проверок.

<a id="current-executor-task"></a>
## Текущее задание после P131 — проверка сохранённых ответов

<!-- CODEX-POST131-CURRENT:START -->
**P131-NEGATION-LEARNING-01 / Q131 · RESULT_RECORDED; перенос на H/X не подтверждён.** На срезе `b27783593745ffac8e74e81f917539717cd33615` записан [результат единственной попытки](../../../../../experiments-v2/RP01-MICRO/P13101/results/attempt-0001/continuation-0001/report.md). Начальный pre-model отказ и continuation относятся к одному `attempt-0001`. Повтор PRIMARY, bootstrap, Train, Flow или Load для ремонта измерителя не назначен. Нижний паспорт сохраняет исходную постановку; его будущие формулировки и команды не являются текущим запускным заданием.

**Подтверждённый частный результат.** N: 0/4 → 4/4; P: 8/8 сохранены; H и X: 0/4 → 0/4, в том числе после загрузки B другим процессом. Все восемь H/X после B дают положительный приказ с confidence 0,99 через `executor-en_ru-phrase-map`; эти ошибки остаются в отчёте. H содержит глаголы, знакомые из положительного S0, но не из отрицательных уроков. X проверяет новые сочетания знакомых глаголов и объектов. C точно воспроизвёл назначенные циклически переставленные targets на 4/4 N. Это усвоение четырёх предъявленных фраз, а не общее владение отрицанием.

**Доказательство, которое уже есть.** Независимая сверка всех 128 строк `raw-writer.json`/`raw-reader.json` с сохранёнными `evidence/native-flow/{arm}-{id}.json` нашла 0 расхождений: native `isSuccess=true`, finalHumanResult, overallVerdict, selectedFlowId, полные metrics и warnings соответствуют raw. Full live-state и reader-state совпадают с оговорёнными исключениями; expected-B проверен независимо. Это подтверждает опубликованные числа, но не заменяет отсутствующую обязательную связь в рабочем saved-verifier.

**Именованная проверка `NATIVE_OUTPUT_BINDING`.** Неверная приёмка: согласованно заменить actual и `nativeMetrics.translated_length` в B.after и B.restored, оставить настоящие native outputs и принять совпавшие производные копии. Правильная приёмка: из неизменяемого рецепта вывести точный набор arm/id — здесь все128; прочитать соответствующие native payload и проверить success, actual, verdict, flow, adapter/confidence/refusal, native version, все metrics/warnings и согласованность aggregate/per-row копий. Missing, duplicate, лишний ключ или mismatch обязательной связи должны доходить до общего status и ненулевого outer exit. Если какое-либо поле не наблюдается в исходном payload, обозначить эту границу, не изготовлять его из gold или state.

**Законный отрицательный результат.** Полный настоящий native ответ, согласованный со своими raw-копиями, может быть неверным относительно gold. Он остаётся VALID измерением с неуспешной метрикой обучения/переноса; совпадение с gold не является условием технической валидности. Отказ законной C до обучения также сохраняется как NOT_PROVEN причинного контроля, без обхода guard. При исполненном C отдельно посчитать точное совпадение с назначенным target каждого id; одного Any(B != C) недостаточно. Внутренне согласованный настоящий промах C оставляет процедуру VALID, а причинный результат отрицательным/NOT_PROVEN.

**Совместимость старого producer и нового verifier.** Исторические producer HEAD `ca2deeedea88fe909935694ff50dec49ab3de04b`, source/DLL proof и10 required gates не меняются. Новый checker имеет собственные identity/schema/gates; простая пересборка ломает старые SOURCE_BINDING/DATA_BINDING до целевого контроля. Назначен versioned saved reanalysis по150/49: archived producer bytes и однозначная mapping619 путей проверяются отдельно от текущей verifier DLL, old schema отдельно от нового mandatory NATIVE_OUTPUT_BINDING/Terminal. Недоступные original artifacts дают NOT_PROVEN полной provenance, не вымышленный PASS. Существующий VerifyOnly/input-output separation сохранить, PRIMARY HEAD guard не отключать. Положительный upgrade-control на настоящем полном архиве обязателен; порча producer identity/неизвестная schema/missing current gate должна отвергаться. Native record дополнительно связывается с `(arm,id)` по `flowResult.summary.summaryId`, а не одному имени файла.

**Приёмка ремонта без модели.** Read-only `input_root` и новый `verification_output_root`; честный пакет → новая копия с согласованной подменой actual+length у 16 H/X строк B/reader → неизменённый оригинал. Настоящий saved-verifier/outer entrypoint должен дать 0 → nonzero → 0, с причиной `NATIVE_OUTPUT_BINDING`; отдельно missing/duplicate native record и законный полный неверный ответ. Исходный пакет не менять. Python-проекция предикатов уже показывает различающий случай, но не является нативным C# PASS или exit. До реального запуска исправленного verifier статус его приёмки NATIVE_UNRUN. Хеш, заново вычисленный из проверяемой копии, не доказывает независимое происхождение; имеющиеся source/process receipts сверять, отсутствующие задним числом не создавать.

**Расход и цена.** Историческая доза сохранена: 3 Train / 16 предъявлений, 128 обычных Flow, 2 внешних Save / 4 Load и 17 явных ExportCheckpoint. Внутренние export/import при staging учитываются отдельно; 17 не являются новым бюджетом. S0 — 9642 байта, B — 13651 байт: +4009, или +41,58%. Ремонт saved-verifier добавляет 0 Train/Flow/Load. Усвоение N, отсутствие переноса H/X и рост сохранённого состояния — отдельные выводы.

**Текст для исполнителя — передать целиком:**

> Работай только над назначенной saved-only приёмкой P131 по [договору49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md) и [аудиту149](../../fixes-based-on-experiments/codex-149-post131-transfer-and-experiment-audit-2026-11-03.md). Прочитай действующие AGENTS/CLAUDE и применимые experiment-design, code-examination, test-integrity, QA/automation/debug, prompt-patch-authoring и human-communication навыки; назови реально прочитанное. Сохрани RESULT_RECORDED, единственный attempt, данные/gold, механизм и дозу; не повторяй PRIMARY/bootstrap/Train/Flow/Load. Published native/raw совпали128/128; новая находка относится к отсутствующей автоматической связи, а не к подлогу результата. Сначала сохрани original producer bytes до изменения/пересборки; раздели immutable historical producer identity/schema/10 gates и current verifier identity/gates по150/49; архивные bytes не заменяй новой DLL, недоступность originals оставь NOT_PROVEN, existing VerifyOnly сохранить. Затем добавь mandatory NATIVE_OUTPUT_BINDING: точный набор всех128 arm/id, оригинальные native payload → actual/verdict/flow/adapter/confidence/refusal/version/metrics/warnings → aggregate/per-row. Ненаблюдаемое поле не подставляй из gold. После последней source-правки выполни P131-CHECKER-BUILD по49§5.1: отдельные Checks+Probe и host,14requiredFQN+новые случаи, discovery/TRX; VerifyOnly сам не строит DLL. Дальнейшая source-правка требует новой соответствующей сборки/проверки. На новых копиях без модели проверь честный0 → согласованная подмена actual+length у16 H/X строк B.after/B.restored nonzero → исходный честный0, а также missing/duplicate native record. Полный согласованный native ответ, неверный по gold, должен оставаться VALID отрицательным измерением. C проверяется по точным назначенным targets, законный отказ C сохраняет NOT_PROVEN. Раздели read-only input_root и новый verification_output_root; исходные native outputs, raw, checkpoints и receipts не изменяй. Реальный outer exit, source/DLL, команду и результаты запиши; Python-проекцию не называй C# исполнением, при отсутствии runtime укажи NATIVE_UNRUN. В отчёте сохранить N0/4→4/4, P8/8, H/X0/4 с confidence0,99 и phrase-map, 3Train/16предъявлений/128Flow/2Save/4Load/17явныхExport, checkpoint9642→13651 (+41,58%). Исторический паспорт ниже не разрешает новый запуск. STOP/NEXT пользователя имеет приоритет. Передай абсолютный `InputRoot=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P13101/results/attempt-0001/continuation-0001/evidence`; child cwd=hostRoot. Абсолютный новый VerificationOutputRoot и доказанный producer_archive_root назначаются отдельно по155§4; отсутствующий архив не заменяй вымышленным путём. Родительский continuation-0001 вместо evidence должен отказать по layout до model calls. PRIMARY/Train не повторять; сборки, discovery и нативные технические тесты разрешены в назначенном ремонте.

Основание и следующая приёмка: [аудит149](../../fixes-based-on-experiments/codex-149-post131-transfer-and-experiment-audit-2026-11-03.md), [договор49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md). Нативная приёмка нового checker остаётся отдельным фактом от выполненного read-only аудита.
<!-- CODEX-POST131-CURRENT:END -->

## Исходное задание и граница — исторический паспорт

Следующий текст сохраняет постановку до запуска. Его DESIGNED_NOT_RUN и слова «будущие файлы» относятся к моменту проектирования; текущий статус и допустимое действие заданы выше. Учебный источник, PRIMARY, gold и доза не изменены.

**P131-NEGATION-LEARNING-01 · эксперимент/Q131 · DESIGNED_NOT_RUN.** Карточка подготовлена по поручению Любы 3 ноября 2026. Новый опыт не запускался; результаты не предполагаются заранее. Будущие файлы измерителя/данных ниже назначены, но пока не созданы. Нынешняя документная сессия не разрешает их реализацию или запуск.

**Тема обучения — перевод; профиль — EN→RU, технические команды.** Обучается существующая типизированная память TranslationMachine, единица executor-en_ru, TranslationRoutingFlow.FlowId, topic=technical. Это обучение памяти примерам, а не LoRA/нейросеть. Исполнитель использует свою default-модель без переключения провайдера.

P125 проверил восстановление памяти восьми утвердительных уроков на закреплённой старой Debug DLL. Ответ «do not delete the file → удали файл» с уверенностью0,99 остаётся наблюдённой потерей смысла. Но отрицанию там не учили, поэтому вывод о неудачном **обучении отрицанию** некорректен. Вопросы и наклонение в этот новый опыт не включены.

**Гипотеза:** четыре согласованных отрицательных урока изменяют ответы памяти и уменьшают потери отрицания, сохраняя прежние утвердительные ответы. Запоминание предъявленных фраз, новые сочетания знакомых слов и перенос на другие глаголы измеряются раздельно.

Основание: [аудит](../../fixes-based-on-experiments/evidence/2026-11-03-p125-negation-and-test-triage-audit.md), [договор45](../45-learning-claim-and-native-acceptance-contract-2026-11-03.md), [P125](P125-executor-memory-learning.md), [P25](P25-cold-restore.md). Старые опыты, raw, gold, manifest и checkpoints не изменять.

## Один обученный старт, три независимых плеча

S0 уже обучен восьми положительным урокам. A/B/C загружаются из одних байтов S0 в независимые экземпляры; before-ответы и identity памяти должны совпадать.

| Плечо | После before | Назначение |
|---|---|---|
| A | Без дополнительного обучения | Результат восьми положительных уроков |
| B | Один batch из четырёх отрицательных уроков N1–N4 | Эффект нового материала |
| C | Те же четыре входа; chosen циклически переставлены N2,N3,N4,N1 | Проверка зависимости от обучающих ответов |

В C переставить согласованно chosen и whole-phrase mapping; отрицательная полярность сохранена, действие/объект неверны. **Gold панели не меняется.** C изолирована и не публикуется. Если продукт отвергает её до обучения, сохранить отказ и отметить причинный контроль неисполненным; guard не обходить.

A/B не имеют одинаковой добавленной дозы: добавление уроков — исследуемый фактор. Нельзя объявить сравнение нулевого обучения A с четырьмя уроками B ускорением. Стоимость B/C показывается с дозой рядом.

Алгоритм: совместимый S0 → Load A/B/C → before20 каждого → один Train B и один Train C → after20 B/C → Save B → новый процесс → Load B →20 ответов без Train. Ответы идут через обычный переводческий flow, не через прямую выдачу эталона.

## Полностью назначенные данные

Источник положительных строк — [train.json P125](../results/2026-10-18/P125-enru-memory-01/train.json), SHA256 **cf765bcc7cf2147f32e05d0e0e2af60eb99eb75319b50209c9092f966973a880**. Исходный файл не редактировать. Его отдельные mappings глагола/объекта сохранить.

| ID | Положительный вход S0 | Chosen |
|---|---|---|
| P1 | validate the file | проверь файл |
| P2 | delete the cache | удали кэш |
| P3 | copy the report | скопируй отчёт |
| P4 | move the config | перемести конфигурацию |
| P5 | upload the image | загрузи изображение |
| P6 | download the package | скачай пакет |
| P7 | save the draft | сохрани черновик |
| P8 | archive the log | архивируй журнал |

| ID | Новый учебный вход B | Chosen B |
|---|---|---|
| N1 | do not delete the cache | не удаляй кэш |
| N2 | do not upload the image | не загружай изображение |
| N3 | do not copy the report | не копируй отчёт |
| N4 | do not move the config | не перемещай конфигурацию |

У каждого N ровно **один phraseMapping: целая входная фраза → весь chosen**. Не добавлять общую ручную пару «do not→не», не перезаписывать положительный «delete→удали» отрицательным значением. Не менять алгоритм/пороги/уверенность продукта. Родные представления строятся штатным кодом.

Формирование TranslationLearningExample/ExecutorInput/ExecutorOutput/TranslationLearningBatch — по [P125 Program.cs](../../../../../experiments-v2/RP01-MICRO/P12501/Program.cs). BatchId: P131-S0-v1 / P131-B-v1 / P131-C-v1; ExampleId уникальны по плечу. UnitId=executor-en_ru; источник=simulation; HeldOut=false; chosen confidence=0.95; rejected="[WRONG: missing action and object - not a translation]", confidence=0.1; tag=synthetic_missing_translation. Signature вычисляется родным TranslationSemanticTokenizer из входа. Между B/C меняется только назначенный target.

**Панель —20 строк в порядке P1–P8, N1–N4, H1–H4, X1–X4.** P — удержание; N — запоминание (не независимое обобщение). Дополнительные строки:

| ID | Вход | Независимый gold | Проверяемая граница |
|---|---|---|---|
| H1 | do not save the draft | не сохраняй черновик | Глаголу save отрицанием не учили |
| H2 | do not archive the log | не архивируй журнал | Глаголу archive отрицанием не учили |
| H3 | do not validate the file | не проверяй файл | Глаголу validate отрицанием не учили |
| H4 | do not download the package | не скачивай пакет | Глаголу download отрицанием не учили |
| X1 | do not delete the file | не удаляй файл | Новое сочетание; сохраняет контрпример P125 |
| X2 | do not upload the package | не загружай пакет | Выученный отрицательный глагол, другой знакомый объект |
| X3 | do not copy the draft | не копируй черновик | То же |
| X4 | do not move the image | не перемещай изображение | То же |

Все слова объектов уже встречались в S0. H/X не включать в TRAIN, phraseMappings, настройку порога или выбор алгоритма. Одной темы достаточно; разные классы внутри неё не являются смешением доменов.

## Host, сборка и переиспользование S0

Корень будущего host: **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P13101/**.
Назначенные будущие файлы: Experiment.csproj; Program.cs; Run-Experiment.ps1; data/positive.json; data/negative-B.json; data/negative-C.json; data/panel.json; data/gold.json; Checks/Checks.csproj. Их создаёт исполнитель только при разрешённой реализации опыта.

ProjectReference ведёт на **../../../27.TranslationDomain/27.TranslationDomain.csproj**. Не копировать из P12501 ссылки на старые bin/Debug DLL. До первого Train реально собрать продукт и host в Release; сохранить HEAD+локальную дельту, closure исходников/props/targets и identity загруженных DLL/PDB. Compiler failure передать архитектору; старой сборкой его не обходить.

В host исключить Checks/**/*.cs из его Compile items: вложенный тестовый проект не должен попадать в executable через SDK glob. До первого Train заморозить SHA256 всех пяти файлов данных (positive, negative-B, negative-C, panel, gold), scorer/config, host и продуктового source/reference closure. Каждая стадия сверяет именно эти байты; новая версия набора/измерителя не перезаписывает прежнюю попытку.

Назначенный reusable S0: **P13101/starts/enru-positive-v1/checkpoint.json**, рядом manifest.json. Manifest: хеши checkpoint и positive.json, продуктового closure и DLL, serializer/model/schema, origin attempt, состав восьми уроков, подтверждение квалификации. Пока этого старта нет; snapshot P125 от другой DLL автоматически не подходит.

- При наличии совместимого квалифицированного S0 именно этой сборки/набора использовать Load, не обучать S0 повторно.
- Иначе внутри разрешённого бюджета сделать **единственный bootstrap**: холодный экземпляр отвечает на P1–P8; один TrainStructuredBatchAsync из8 положительных уроков; Save S0.
- После Load A/B/C выполнить before20. На P1–P8 требуется8/8 и одинаковая identity. Если старт этого не даёт — сохранить результат, признать старт негодным и эскалировать. Дозу/эталоны самостоятельно не расширять.
- B/C добавляют по4 урока одним родным batch к копии загруженного состояния. Проверить RowsTrained и действительное изменение памяти, не только TrainCalls.
- B сохранить в results/attempt-NNNN/states/B/checkpoint.json. Свежий reader-процесс создаёт новый экземпляр, Load и20 flow-вопросов, Train=0. S0 остаётся неизменяемым.

Приёмка восстановления обязательна: все20 B.after/B.restored outputs, routes/adapters, verdicts и конечные confidence совпадают; проверить полный payload/состав памяти и canonical identity, не один count. Независимый ожидаемый состав получен из S0 и ровно четырёх уроков B; фактический экспорт B сохранён до выгрузки, память reader не строится из ожидания. Исключения из canonical hash (например, время выгрузки) перечислить до запуска; обученные данные, версии схемы и принадлежность не исключать ради совпадения. Missing/false обязательный технический контроль даёт INVALID_MEASUREMENT и ненулевой exit. Отсутствие полезного научного улучшения при исполненном корректном измерении не подменять технической ошибкой.

## Доза и команда

| Режим | Train calls / предъявления уроков | Обычные flow calls | Внешние Save / Load |
|---|---:|---:|---:|
| Совместимый S0 уже есть |2 /8: B4+C4|120|1 /4|
| Единственный bootstrap S0 |3 /16: S0:8+B4+C4|128|2 /4|

Save/Load здесь — внешние вызовы хозяина. Внутреннее копирование состояния в родном Train (StageRuntimeCopy с export/import) тоже учитывать в physical ledger и стоимости; не выдавать внешние4 Load за полный счёт внутренних операций.

120=before A/B/C:60 + after B/C:40 + fresh B:20. Bootstrap добавляет8 холодных flow,1 Train/8 уроков и1 Save. A после B/C не опрашивать повторно: фиксировать unchanged identity без нового flow. Нейронные Forward/Backward=0, но это не обнуляет стоимость120/128 обычных вызовов. Все физические попытки/диагностические вызовы включаются в общий ledger. После сборки предел15 минут; сборка отдельно до20 минут. Превышение — отказ по бюджету, не сокращение панели.

Общий предел попытки35 минут: первые20 включают product/host/checks build и исполнение проверок измерителя; следующие15 включают bootstrap, writer, reader, scoring и finalization. Deadline общий для всех дочерних процессов, а не новый для каждой роли. Его превышение фиксируется с сохранением logs/состояния процесса; новую попытку без разбора не запускать.

Команда после реализации host и прямого разрешения исполнения:

~~~powershell
Set-Location -LiteralPath 'C:/Projects/llm_mvp_core'
dotnet build '27.TranslationDomain/27.TranslationDomain.csproj' -c Release
dotnet build 'experiments-v2/RP01-MICRO/P13101/Experiment.csproj' -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File 'experiments-v2/RP01-MICRO/P13101/Run-Experiment.ps1' -Attempt next
~~~

После каждой команды проверять exit; при неуспехе не переходить к следующей. Runner атомарно выбирает первый свободный attempt-NNNN; не перезаписывает существующий и не запускает вторую копию активной попытки. Дождаться исходного task/session handle ограниченными блокирующими ожиданиями по [договору31](../31-tool-discovery-and-wait.md). Команды описывают будущий host; они не объявлены исполненными.

## Проверки измерителя, которые предшествуют обучению

Checks/Checks.csproj — изолированные проверки искусственных сохранённых документов, без Train/flow callbacks. При реализации измерителя реально собрать проект, проверить discovery и выполнить проверки по [договору46](../46-qa-execution-test-repair-and-fresh-results-2026-11-03.md). Каждому вызову — новая папка результатов:

~~~powershell
$p131QaDir = Join-Path 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P13101/qa-results' ((Get-Date -Format 'yyyyMMdd-HHmmssfffffff') + '-' + [guid]::NewGuid().ToString('N'))
dotnet test 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P13101/Checks/Checks.csproj' -c Release --results-directory $p131QaDir --logger 'trx;LogFileName=p131-checks.trx'
$p131QaExit = $LASTEXITCODE
if ($p131QaExit -ne 0) { throw "P131 checks failed: exit=$p131QaExit; results=$p131QaDir" }
~~~

Сохранить команду, stdout/stderr, terminal exit, source/DLL и содержимое TRX: TestRun ID, Times, имена и counters. Ссылка на этот конкретный TRX входит в manifest попытки. Новый mtime, одинаковое имя файла или собственное "passed":true не доказывают свежего исполнения. Проверки входят в первые20 минут общего бюджета; они не открывают дополнительную попытку обучения.

Именованные проверки: GoldIndependentOfActual; RequiredIdsExact; RefusalIsNotCorrectTranslation; NegationPolarityWholeToken; ConfidenceMustBeFinite; StateAndAssemblyIdentityRequired; MissingReaderReceiptFailsTerminal; SuccessfulBuildDoesNotMeanSuccessfulRun. Законный документ проходит; минимальная порча по каждой обязанности отвергается. Это проверки измерителя с0 Train/flow, не научный результат и не замена20 реальных запросов.

**Собственный контроль полного восстановления.** У P12501 рабочий IndependentExpectation читает Topic, но сравнивает только Source/Target; его ComparatorControls вызывает другую функцию ContainsLesson, которая ищет target где угодно в тексте ([Program.cs:429](../../../../../experiments-v2/RP01-MICRO/P12501/Program.cs:429), [Program.cs:456](../../../../../experiments-v2/RP01-MICRO/P12501/Program.cs:456)). Поэтому P131 не копирует эти три controls как доказательство чувствительности своего сравнивателя. В Checks назначены ещё шесть проверок:

| Имя | Честный вход и одна проверяемая граница |
|---|---|
| FullCheckpointHonestFixtureAccepted | Полный искусственный S0 с восьмью P и законный B с четырьмя N проходят рабочий comparer. Ожидаемый состав заморожен отдельно до порчи; live и reader — два проверяемых входа. Нулевой научный выигрыш при исправных документах не делает это измерение негодным. |
| TopicOnlyMutationRejected | При прежних Source/Target, количестве записей и пересчитанном SHA изменить только topic одной exact-записи. Тот же comparer отвергает несоответствие назначенному technical; поиск target по всему JSON не участвует в решении. |
| KnnPayloadWithRehashedIdentityRejected | Подменить target одной реальной KNN-записи, сохранив count и пересчитав её заявленные hashes. Подменить её согласованно в live и reader: их равенства недостаточно, независимый ожидаемый состав обязан обнаружить порчу. |
| ProvenanceMultiplicityMutationRejected | В полной fixture заменить одну запись происхождения копией другой при прежнем общем count. Проверить назначенные BatchId/ExampleId, кратность и порядок там, где его сохраняет действующая схема; не превращать журнал в множество. Если нужного поля нет в продуктовой схеме, явно отметить границу наблюдаемости до запуска, не изготовлять его как actual. |
| MissingExpectedCheckpointRejected | Отсутствующее независимое ожидание — ошибка измерителя. Нельзя получить expected из проверяемого live/reader и принять совпавшие копии. |
| ControlFailureReachesTerminal | У честной полной receipt заменить один обязательный результат на false; отдельно проверить missing, duplicate и exception. Тот же рабочий finalizer меняет status.json, exit.json, код процесса и признак допустимости научного вывода согласованно. Исторические ответы и расходы при этом сохраняются. |

Итого назначены14 имён проверок измерителя; это план, не число выполненных тестов. Реальный discovery отдельно показывает методы и параметризованные случаи. Честный вход и порчи проходят один comparer/finalizer, который действительно вызывает host P131; отдельная демонстрационная функция не закрывает контроль. Все эти проверки используют только искусственные документы и добавляют0 Train/flow. Научные дозы2/8 либо3/16 и120/128 запросов остаются прежними. Полное равенство live→reader и независимая проверка учебного состава — разные обязательные утверждения; одно не заменяет другое.

## Метрики и вывод

Для A.before/B.before/C.before/B.after/C.after/B.restored сохранить полный output, refusal, confidence, actual adapter/route, наличие точного учебного соответствия и identity памяти. Обычный вход — TranslationMachine.GetOrchestratorAsync и flow-вызов из P125 Evaluate. Expected нельзя подставлять в продуктовый ответ.

По P/N/H/X отдельно показать:
- полный правильный ответ k/n и изменение в процентных пунктах;
- сохранение полярности k/n; отказы/пустые ответы;
- опасное снятие отрицания: число на всех отрицательных запросах и среди непустых ответов;
- верные/неверные ответы при confidence≥0.90, высокоуверенные ошибки; среднюю confidence верных и неверных (если группы нет — не измерено);
- exact/phrase-map/near/композиция по реальному trace, без объяснения phrase-map как exact-memory;
- wall/CPU обучения, RAM/выделения, размер снимка, время записи/загрузки, с единицами и фактической дозой.

LearningBatchResult.AccuracyBefore/After может обозначать coverage при отсутствии HeldOut; он не заменяет точность независимой панели.

**Точный oracle:** вход/выход Unicode NFC, затем ToLowerInvariant, trim и свёртка Unicode-пробельных последовательностей в один пробел. Удалять только один конечный знак «.»; остальные знаки, «ё/е», порядок слов и токены сохраняются. Допустимый gold ровно один, указанный в таблице; новые варианты после просмотра actual не добавлять. Refusal, пустой, missing или нестроковый output не являются правильным переводом. NaN/Infinity или отсутствующая confidence — неполное техническое измерение.

Для этой ограниченной панели полярность считается сохранённой, когда первые два целых токена — «не» и один из «удаляй, загружай, копируй, перемещай, сохраняй, архивируй, проверяй, скачивай». Это отдельная синтаксическая метрика: «не копируй отчёт» вместо «не удаляй файл» остаётся неверным полным переводом. Опасный переворот фиксировать при первом токене «удали, загрузи, скопируй, перемести, сохрани, архивируй, проверь, скачай» на отрицательном входе. Другие непустые неверные формы учитывать отдельно как неклассифицированные, не объявлять безопасными. Проверка полярности не использует substring и не возвращает ожидаемый текст вместо actual.

Это правило полярности применяется только к N/H/X. Для положительной P полярность отдельно N/A; её сохранение измеряется строгой правильностью8 ответов. Нельзя поставить P0/8 по правилу, которое требует отрицание.

Порог постоянного ответа вычислить без модели: максимальная частота одного gold в каждой группе / n. Превышение этого порога само по себе не доказывает обучение. Сильное утверждение о переносе на H/X требует соответствующего улучшения относительно A/before и анализа C; это всё ещё ограниченная панель, не универсальная грамматика.

**Любой измеренный полезный выигрыш — успех по показателю и профилю; рост общей точности не обязателен.** Все ухудшения рядом. N:0/4→4/4 при H:0/4→0/4 означает приобретение предъявленных фраз без доказанного переноса. Само0→0 в целевой группе не успех и не основание продуктового исправления. Если C не показывает зависимости от target, причинное обучение — NOT_PROVEN; дополнительным fit это не компенсировать.

Меньше опасных переворотов за счёт отказов — полезный эффект этого показателя, но покрытие ухудшилось; отказ не становится правильным переводом. Неполученный/негодный старт — не отрицательный научный вывод. Допуск переноса оценивается отдельно по правилу пользователя: значимый >5% либо малый выигрыш без значимой регрессии других метрик, исключение CPU. До результата этой карточки новый продуктовый семантический механизм не разрешён.

## Отчёт, навыки, следующая работа

Отчёт по-русски: **P13101/results/attempt-NNNN/report.md**. Рядом raw.json, metrics.json, training-receipts.json, state-receipts.json, build.log, test.trx, ledger.json, status.json, exit.json, manifest. Показать задачу, уроки, before/after, отдельные P/N/H/X, confidence/опасные ответы, стоимость, C, restore, версию сборки и ограничения. Нет исполнения — DESIGNED_NOT_RUN, не ожидаемые числа как факт.

Перед новым заданием заново полностью читать local/canonical AGENTS, CLAUDE, архитектуру, planning-session-startup, plan-review, code-execution, code-examination, generated-code-review, prompt-patch-authoring, output-readability-gate, human-readable-architecture-audit, human-communication-GLOSSARY. Пути и полный основной список — [120, §1](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md#1-свежее-полное-чтение-перед-новым-аудитом-или-заданием). Дополнительно experiment-design, test-integrity, test-failure-triage, bug-to-tests; [договор45](../45-learning-claim-and-native-acceptance-contract-2026-11-03.md), [договор44](../codex-44-post117-saved-evidence-and-finalizer-contract-2026-11-03.md) и [договор46 с QA/debug-навыками по стадиям](../46-qa-execution-test-repair-and-fresh-results-2026-11-03.md).

В назначенной реализации/приёмке исполнитель применяет QA для build/discovery/test, test-integrity — к рабочему comparer и полноте проверки, test-failure-triage и bug-to-tests — при compiler/setup/assert failure; нужный debug-навык выбирается по46. Неизвестный baseline остаётся UNKNOWN. Неверный тип, fixture или expected исправляется по независимому утверждённому договору внутри поручения, без повторного разрешения; actual сам по себе эталоном не становится. Спорное изменение требования передать архитектору с input/expected/actual, source/DLL и журналом. После правки снова собрать и выполнить назначенные проверки финального исходника; UNRUN не закрывает реализацию. Прямое поручение только на исследование/документы не запускает эти команды; оно не становится запретом операционному исполнителю следующего назначенного этапа.

Хорошо: свежая сборка, один S0, отдельные B/C, независимый gold, запоминание не названо обобщением. Плохо: ручное do not→не в продукте, обучение на H/X, удаление отрицательного контроля, старая DLL вместо current build, "принято" без запуска проверок.

После исполнения записать результат Q131; кандидата исправления оформлять только по измеренному выигрышу и цене. P125 не повторять; вопросы/наклонение не запускать хвостом этого опыта. Прямой STOP немедленен, NEXT переводит к названному ID без продолжения старой работы ради отчёта. При разрешённом запуске не завершать ответ вместо ожидания. После среза выполнить git add --all по правилу пользователя.

