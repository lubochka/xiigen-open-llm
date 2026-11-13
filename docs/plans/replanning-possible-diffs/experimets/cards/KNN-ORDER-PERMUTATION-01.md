# KNN-ORDER-PERMUTATION-01 — сохраняет ли обученная память порядок одинаковых слов

<!-- POSTQ011-2026-11-04:START -->
## POSTQ011 — 2026-11-04: короткая дельта к 153/155, без повторного PRIMARY

**Основание:** [аудит POSTQ011](../../fixes-based-on-experiments/codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md) и [адресный план](../../fixes-based-on-experiments/codex-159-postq011-addressed-repair-plan-2026-11-04.md). Срез исходников `45bd4e371f511e29c053f8117d61f43365abb460`. Это уточнение действующего владельца и приёмки; очередь, статусы и исторические результаты сохранены. Текущая нативная приёмка — **UNRUN**; документный аудит не запускал обучение.

**Сохранённый результат проверен по JSON обеих попыток.** TRAIN 9/10→10/10, EXAM 5/6→6/6; приобретённые строки — p2 TRAIN и h2 EXAM. В каждой сохранённой панели 68 reads найдено 0 расхождений route/ordered payload. Это read-only пересчёт сохранённых bytes, не запуск C# finalizer и не доказательство устойчивости его проверок к порче. Ресурсы брать из raw своей попытки по 153; отсутствующие retained/peak/checkpoint bytes не восстанавливать предположением.

**Остаток владельцев Q127 / 105§5 и E131-02/R154-D.** [LabelSensitivityHolds:1111–1125 / BuildFinalizer:1176–1178](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/experiments-v2/RP01-MICRO/P12701/Program.cs#L1111-L1178) допускает различную label C при прежних B steps: inequality label и два известных имени ещё не связывают полный payload. Применить уже назначенный saved-only договор: exact 68 reads / 12 graph keys выбранной попытки, route↔полный ordered steps↔существующие edges/trace и внешняя историческая authority из 155. Genuine scientific-negative actual остаётся VALID; синтетический полный fixture имеет отдельную identity. Future finalizer→process→shell honest 0 / адресная порча nonzero / honest 0 остаётся UNRUN. Native-flow, которого исторически нет, сохраняет NOT_RECORDED; Train/Predict/graph calls для ремонта checker не добавляются.

**Граница переноса.** Измеренный [P127 KnnLogicalUnit<string,OrderedRoute>:552–604](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/experiments-v2/RP01-MICRO/P12701/Program.cs#L552-L604) с input-only positional predicates отличается от [DefaultLogicalCapabilityTrainer:1052–1069](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1052-L1069) и его TransformerEmbeddingKnn. Restore также требует этот тип в [TeachingRegistryPersistence:939–947](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/18.CapabilityTraining/TeachingRegistryPersistence.cs#L939-L947). Продуктовый caller остаётся открытым у 105§5; проверка saved checker не выбирает новый encoder и не доказывает эту интеграцию.
<!-- POSTQ011-2026-11-04:END -->

<!-- POSTQ003-2026-11-03:START -->
## Адресное уточнение после Q003 — P127-EXTERNAL-HISTORICAL-AUTHORITY

**Установлено / область:** Новый153 уточнил p2 TRAIN / h2 EXAM, ресурсы отдельных попыток и продуктового caller. Эти поправки сохраняются; добавлена конкретная внешняя authority для 15 result files на7ce.

**Действие существующего владельца:** Применить155§5/50§4: manifest из закреплённого audit commit вне input copy; точный набор файлов выбранной попытки; 68 read keys и 12 graph keys. Source/DLL proof и inventory до/после не заменяют историческую identity outputs. Synthetic mode имеет собственную identity.

**Различающая приёмка:** Положительный historical контроль: подлинные bytes с реальными, в том числе неверными ответами; synthetic C=B допустим как отдельный validator fixture. Отрицательный: согласованно подменённые route/steps/summary/local hashes под прежним attempt отвергаются внешней authority. Native flow остаётся NOT_RECORDED; новые Train/Predict/graph calls не нужны.

Обязательны [договор50](../codex-50-postq003-binary-and-historical-evidence-contract-2026-11-03.md) и [адресный план155](../../fixes-based-on-experiments/codex-155-postq003-addressed-repair-plan-2026-11-03.md). Это уточняет приёмку и сохранность; собственные factor, данные, gold, исторические budgets и очередь не меняются. Новый PRIMARY/Train для ремонта документа или bytes не назначен. Native Load/технические проверки учитываются отдельно только в собственном назначенном ремонте; их отсутствие обозначается UNRUN.

### Обязательная часть передаваемого поручения

> P127-EXTERNAL-HISTORICAL-AUTHORITY: Применить155§5/50§4: manifest из закреплённого audit commit вне input copy; точный набор файлов выбранной попытки; 68 read keys и 12 graph keys. Source/DLL proof и inventory до/после не заменяют историческую identity outputs. Synthetic mode имеет собственную identity. Положительный historical контроль: подлинные bytes с реальными, в том числе неверными ответами; synthetic C=B допустим как отдельный validator fixture. Отрицательный: согласованно подменённые route/steps/summary/local hashes под прежним attempt отвергаются внешней authority. Native flow остаётся NOT_RECORDED; новые Train/Predict/graph calls не нужны. Передай exact attempt/path/donor authority, фактические commands/exit и предел проверки. Сохрани исторические результаты; повтор обучения ради получения отсутствующего файла не выполняй.
<!-- POSTQ003-2026-11-03:END -->


**Поправка к чтению результатов, 3 ноября — [153§2](../../fixes-based-on-experiments/codex-153-p115-p127-transfer-boundaries-2026-11-03.md).** Обе попытки: TRAIN9/10→10/10, EXAM5/6→6/6; p2 принадлежит TRAIN, h2 — EXAM, поэтому это не «две улучшившиеся экзаменационные строки». §13 отчёта attempt-0002 повторяет ресурсы attempt-0001: брать числа только из raw той же попытки и отдельно показывать reverse-control. Allocations растут на0,60–2,51% в сравненных парах; wall reverse-control +5,21%/−11,83%, устойчивый знак не установлен. Retained/peak по плечам и checkpoint bytes не измерены. Сохранённые raw/исторические отчёты не переписывать, PRIMARY не повторять. Продуктовый caller остаётся открытым у105§5: TransformerEmbedding KNN нельзя объявить тем же механизмом, что измеренный BPE/predicate KNN. Это уточнение обязательно включать в prompt при подготовке переноса; оно не меняет назначенный saved-only ремонт150/152.

## Уточнение текущего задания: состав пакета и вид контроля

**Полнота P127 (`P127-SAVED-KEYS`).** Для каждой выбранной исторической попытки отдельно требовать 68 уникальных `(Arm, CaseId)` в `metrics.reads`: `{A,B,A-perm,B-perm} × {u1…u8,p1,p2,e1…e4,h1,h2}` (64) и `C × {p1,p2,h1,h2}` (4). Не объединять две попытки в один набор. Графы: ровно `{A,B} × {e1…e4,h1,h2}` — 12 ключей. Cold observations и контрольные квитанции учитываются отдельно. Проверить точные множества, кратность и обязательные поля до `All()` и scoring: empty, missing, extra, duplicate и повреждённый payload `A-perm/B-perm` — разные отрицательные случаи. Счётчики 36+32 и 12 не доказывают присутствие строк. `BuildFinalizer` должен принимать также permutation records; прежний `armA.Concat(armB).Concat(armC)` их пропускал.

**Предел первоисточника P127.** `ReadOutcome` формируется из native ответа в `Program.cs:697–724`, затем сохраняется в `metrics.reads` (`459–482`). Отдельного P131-подобного `native-flow` для каждой строки нет. Проверка неизменности опубликованных bytes, DTO и имеющегося A/B graph trace устанавливает внутреннюю согласованность доступных свидетельств; она не создаёт вторую независимую запись ответа и не доказывает полный historical runtime. Отсутствующий native payload обозначить `NOT_RECORDED`; не вызывать модель и не изготовлять «оригинал» из summary. Результат и предел provenance сообщать раздельно.

**Три разных контроля (`SYNTHETIC-IS-NOT-HISTORICAL`).** (1) Неизменный настоящий пакет проверяется с его фактическими ответами, в том числе неверными по gold. (2) Копия исторического пакета с изменёнными route+steps под прежней identity/proof — повреждённое свидетельство, даже если DTO внутренне согласован; такая копия не становится VALID историческим запуском. (3) Для сценария «настоящий C=B или неверный B/e1» при отсутствии такого сохранённого запуска автор ремонта создаёт явно синтетический **полный** fixture validator/finalizer с собственной synthetic identity, всеми назначенными записями и согласованными A/B traces; C trace не добавляет. Он проходит тот же finalizer и outer exit, но только в явно отделённом режиме тестирования validator, и никогда не попадает в научные метрики или accepted historical runs. Это проверка логики checker без Train/Predict/graph, не новый опыт обучения. Unit-тест одного scorer не заменяет этот контроль; при невозможности получить полный fixture оставить приёмку открытой, не подделывать producer proof.

> Обязательная часть копируемого поручения Q127: проверяй exact68 reads и12graph keys **на каждую попытку**, включая A-perm/B-perm; пустота и пропущенный permutation payload не проходят через All(). Второй native record отсутствует — не синтезируй его из reads. Coherent scientific-negative контроль выполняй на настоящем неизменном ответе либо отдельно помеченном полном synthetic validator fixture, а не правкой historical raw под старой identity. Сохрани исходные результаты, новый fit/Predict/graph не назначен.

<!-- CODEX-POST131-CURRENT:START -->
## Действующее уточнение после P131 — 3 ноября 2026

**Проверка `POST131-P127-ORDERED-CONTROL`. WRONG:** любое отличие `AnswerRoute` между B и C означает следование переставленным урокам; два известных имени в `AnswerSteps` подтверждают полный ответ. **CORRECT:** `LabelSensitivityHolds` и `PredictionVersusActual` должны отдельно сообщать точное следование C исходно назначенным целям p1=`[implement,review]`, p2=`[review,implement]`; это научный показатель. Проверяющий целостности отдельно связывает известный `AnswerRoute` с полным упорядоченным `AnswerSteps` и существующим native результатом. `ComparePermutation` сравнивает шаги, маршрут и отдельно соседа/уверенность. **Путь приёмки:** сохранённые `ReadOutcome` → согласованность DTO/trace → пересчитанный научный контроль → общий verdict. Порча C: произвольный route при шагах B должна быть обнаружена; согласованный настоящий C, не усвоивший цель, остаётся допустимым отрицательным научным результатом (`NOT_PROVEN`). Новый Train и дополнительные C-графы не назначаются; исходные 1/2→2/2 и расход двух попыток сохраняются.

**Область:** связь raw с native проверяется только по реально существующим либо явно назначенным записям. Если такого источника нет, записать точный предел подтверждения. Этот блок не назначает новый Train/Flow/Load, persistence API, повтор PRIMARY или новую научную очередь. Integrity refusal и честный отрицательный scientific result различаются.

Основание: [аудит 149](../../fixes-based-on-experiments/codex-149-post131-transfer-and-experiment-audit-2026-11-03.md) · [план 150](../../fixes-based-on-experiments/codex-150-post131-addressed-repair-plan-2026-11-03.md) · [покрытие 151](../../fixes-based-on-experiments/codex-151-post131-card-coverage-2026-11-03.md) · [договор 49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md). Ниже сохранена прежняя постановка; текущий блок уточняет только указанную границу.
**Конкретная переклассификация старого gate:** в `BuildFinalizer` убрать научный `mandatory["label_sensitive_c_holds"] = LabelSensitivityHolds(...)` из процедурного all-pass и записывать его/точный `assigned_target_match` как scientific observation. Нельзя просто заменить label inequality на exact C-target внутри того же mandatory: это снова сделает честный научный минус INVALID. Integrity содержит непротиворечивость самого DTO и связь с действительно сохранённым producer/trace. Исторический status с девятью checks не переписывать; новая квитанция называет свою схему и новый список процедурных проверок. Контроль с согласованным настоящим C=B обязан сохранить integrity VALID при scientific false; отдельная адресная порча evidence обязана дать ненулевой exit.

**Косвенные зависимости тоже убрать из procedural all-pass.** `CSubstitutedByBControl` вычисляет `control_passed = honestHolds && !substitutedHolds`, а `BuildFinalizer` включает его как `c_substituted_by_b_control_passed`. Поэтому удаления одного `label_sensitive_c_holds` недостаточно: настоящий C=B всё ещё отвергался бы косвенно. `CorruptedReceiptControl` аналогично требует `honestVerdict.OrderCorrect` для настоящего B/e1, то есть победу learner относительно gold, через `corrupted_receipt_control_passed`.

Переработать оба control path вместе с finalizer: положительный контроль integrity принимает любой полный непротиворечивый actual, в том числе научно неверный; отрицательный меняет route/steps либо записанный producer/trace при неизменных остальных предпосылках. Для ответа из двух разных узлов проверять точное множество и порядок по договору DTO; одинаковое имя дважды не является полным payload. Прежний C→B label-sensitivity observation хранить в scientific diagnostics, не использовать его истинность напрямую или через helper как обязательство успеха learner. Если нужен unit-control функции gold scoring, использовать отдельно помеченный синтетический positive/negative fixture самого scorer; его успех не является научным результатом B/C и не подмешивается в historical raw.

В файлах ремонта явно указать также `CSubstitutedByBControl` и `CorruptedReceiptControl`. Проверка после правки должна пройти **весь** новый finalizer, а не только новый предикат: согласованный настоящий C=B и согласованный научно неверный B/e1 сохраняют procedure VALID; разрыв route/steps/producer/trace даёт INVALID/ненулевой outer exit. Новых fit/Predict/graph-node calls для этих saved/validator controls нет; исторические результаты и девять checks не переписываются.

<!-- CODEX-POST131-CURRENT:END -->


<a id="current-executor-task"></a>
## Текущее задание после сверки P126–P128 — 3 ноября 2026

<!-- CODEX-POST128-CURRENT:START -->
**Владелец: `KNN-ORDER-PERMUTATION-01 / Q127`. Состояние:** RESULT_RECORDED: P12701/attempt-0001 и attempt-0002. На p1/p2 и h1/h2 A 1/2→B 2/2, C 0/2. Во второй попытке девять вычисляемых обязательных проверок и корректный exit; это выполненное усиление.

**Проверяемый путь:** Настоящий KnnLogicalUnit.Train/Execute → упорядоченные outputs → graph edges/node traces → finalizer в status.json.

**Исторический положительный контроль:** Вторая сохранённая попытка прошла прежний итог; C→B контроль относился к старой классификации gate. Для новой приёмки действуют POST131 и SYNTHETIC-IS-NOT-HISTORICAL выше: scientific false не делает процедуру INVALID. Историческая перестановка candidates: B сохраняет ответы, A меняет5/16.

**Различающий отрицательный контроль:** Длина плана или набор слов без порядка не доказывают различения p1/p2. Не объявлять заново константным уже исправленный finalizer; metrics.json записан до добавления finalizer, приёмный источник сейчас — status.json.

**Итог, предел и расход:** На попытку паспорт: 3 fit, 30 уроков, 36 чтений; контроли отдельно: 2 fit, 20 уроков, 32 чтения. Две попытки вместе: 10 fit, 100 уроков, 136 чтений, 24 холодных наблюдения, 24 graph/48 node calls и шесть проверок квитанции. Save/load API отсутствовал: восстановление не исполнено. PRIMARY не повторять; общий языковой порядок не доказан.

**Текст для исполнителя — передать целиком:**

> Работай по назначенному ID `KNN-ORDER-PERMUTATION-01 / Q127`, сохрани исходный паспорт и его pin. Прочитай действующие AGENTS/CLAUDE и применимые архитектурные, planning/plan-review, code-examination, test-integrity, QA/automation/debug, prompt-patch-authoring и human-communication навыки; запиши фактически прочитанное. Проследи путь: Настоящий KnnLogicalUnit.Train/Execute → упорядоченные outputs → graph edges/node traces → finalizer в status.json. Исторический C→B control не является требованием научной победы нового finalizer. Для текущей приёмки выполни P127-SAVED-KEYS и SYNTHETIC-IS-NOT-HISTORICAL из начала карточки:68 reads/12graphs на попытку, настоящий immutable baseline, адресная evidence corruption и полный synthetic scientific-negative fixture отдельно. При проверенной перестановке candidates B сохраняет ответы, A меняет 5/16. Затем одной адресной порчей: Длина плана или набор слов без порядка не доказывают различения p1/p2. Не объявлять заново константным уже исправленный finalizer; metrics.json записан до добавления finalizer, приёмный источник сейчас — status.json. Обязательное поле должно иметь названного потребителя, предикат отказа и влияние на общий итог/exit. На попытку паспорт: 3 fit, 30 уроков, 36 чтений; контроли отдельно: 2 fit, 20 уроков, 32 чтения. Две попытки вместе: 10 fit, 100 уроков, 136 чтений, 24 холодных наблюдения, 24 graph/48 node calls и шесть проверок квитанции. Save/load API отсутствовал: восстановление не исполнено. PRIMARY не повторять; общий языковой порядок не доказан. SOURCE_IMPLEMENTED и статическая сверка не заменяют нативную приёмку конечного source/DLL. Сборка, discovery, целевые тесты и обоснованная правка неверного теста входят в уже назначенный ремонт по 45/46; при отсутствии runtime сохранить точную ошибку среды, не сочинять PASS. Saved reanalysis не называть новым Train/Flow. STOP/NEXT пользователя имеет приоритет. Обязательная дельта POST131 (`POST131-P127-ORDERED-CONTROL`): Проверяй согласованность известного AnswerRoute с полным AnswerSteps и реально сохранёнными reads/graph records; второго native payload P127 не содержит; для C отдельно вычисли p1=[implement,review], p2=[review,implement] из назначенной перестановки. Произвольный route при шагах B должен выявлять integrity error; согласованный настоящий C без усвоения цели означает NOT_PROVEN, а не невалидную процедуру. В ComparePermutation сравни полный ordered payload. В новом finalizer перенеси старый mandatory label_sensitive_c_holds в scientific observation; также переработай CSubstitutedByBControl/c_substituted_by_b_control_passed и CorruptedReceiptControl/corrupted_receipt_control_passed, чтобы C=B или неверный B/e1 не давали косвенный procedural failure. Integrity baseline принимает полный согласованный actual, а unit-controls scorer отдельно помечены синтетическими; точное следование C-target не ставь условием procedure VALID. Используй сохранённые ответы без нового fit и дополнительных C-графов. Применяй [договор 49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md); связывай raw с native только по реально существующим либо явно назначенным записям. Для historical verification используй `manifest_commit=f3b64f87159193c2897092d6e571b2b65e3b75c4`, SHA256 manifest `df98d7354296396164b51ce583370f4d10dbc46f2e3527e1b67a5f265b1fca84`; `results_commit=7ce6b14ca7bb353ed5f57179f0561053496d36ae`. Путь: `experiments-v2/_audits/postq003-2026-11-03/codex-p127-trusted-historical-output-manifest.json`; manifest читать из закреплённого commit вне input copy. Выбери exactfiles своей попытки из15; согласованная подмена route+steps+trace+summary+местных hashes должна отказать по внешней authority через native finalizer→process→shell. Synthetic mode имеет отдельную identity; он не становится историческим результатом. Train=0 для этой saved-приёмки; сборка и технические тесты входят в назначенный ремонт.

Основание: [аудит 145](../../fixes-based-on-experiments/codex-145-post128-transfer-and-experiment-audit-2026-11-03.md), [адресный ремонт 146](../../fixes-based-on-experiments/codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-07), [нативный договор 45](../45-learning-claim-and-native-acceptance-contract-2026-11-03.md), [QA46](../46-qa-execution-test-repair-and-fresh-results-2026-11-03.md). Исторические блоки ниже сохраняют свои данные, gold, factor, дозы и команды. Новый блок уточняет чтение и приёмку, не запускает PRIMARY и не подтверждает заново все старые результаты.
<!-- CODEX-POST128-CURRENT:END -->

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — порядок входа и содержимое канала

Positive назначенная пара противоположного порядка сохраняет фактический input→positional predicates→native vector→ordered output. Negative в копии сохраняет norm/energy/count, но переставляет finite coordinates или output steps — независимый comparator обязан отвергнуть. Нормировка сама не является доказательством сохранённого порядка.

Существующий фактор positional predicates остаётся единственным; per-half L2P113, другие floors и чужой corpus не вводятся. Действуют 3 fit/30 уроков/36ExecuteAsync. Разница между итогами разных P не является контрольным плечом этого опыта.

Основание: [адресный план 135](../../fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- POSTMERGE-HARDENING-2026-11-02:START -->
## M03/M05 — выученный порядок доходит до назначенного графа

Наблюдать полный OrderedRoute.Steps обычного KnnLogicalUnit k1 и порядок узлов уже назначенного graph adapter. Правильная метка при перепутанном содержимом Steps не закрывает потребителя: в копии receipt переставить шаги при прежнем label, и неизменный checker порядка/рёбер должен отказать. Соседский ID записывается только если его выдаёт штатный результат; иначе NOT_EXPOSED, без новой инструментальной точки и без выдуманной candidate trace. Это plain k1, ему не назначаются cascade, early exact или reached-but-rejected панели. Сохраняются 3 fit, 30 предъявлений, 36 ExecuteAsync, 12 холодных наблюдений, 12 графов и 24 вызова узлов; C с переставленными p1/p2 остаётся существующим различающим контролем.

Основание и выбранный ремонт: [план127](../../fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md). Исходные результаты и учебные дозы сохраняются. Контроли реализации не добавляют скрытые Train/Read к научному паспорту; их стоимость и scope указываются отдельно.
<!-- POSTMERGE-HARDENING-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [120-executor-context-and-test-repair-contract](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница POSITION-TO-ROUTE:** WRONG: RegexSplitter сам является learner или два слова как множество уже доказывают порядок. CORRECT: native KnnLogicalUnit<string,OrderedRoute>.Train/ExecuteAsync при k1/64d получает пустые predicates A или PositionalPredicates B; gold Steps не входят в признаки. Сохранённые e1–e4 положительны, p1/p2 и C со swapped p1/p2 выявляют потерю зависимости от урока. Сверить фактические ordered Steps и назначенные12графов/24node calls, без Retry/Stop и дополнительных сцен.3fit/30lessons/36Execute, включая nested tokenizer fits; отсутствие loader не заменять P50 state-after manifest.

**Исходный адрес и роль:** `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs::Train/ExecuteAsync/Vectorize`; `01.Tokenizer/RegexSplitter.cs` — зависимость исходного представления, не learner; PositionalPredicates/graph consumer назначены экспериментальным host карточки. Достигнутый путь и source parity сверять по назначенному runner; это не заявление о current native acceptance.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
**Действующее согласование:** [40-effective-executor-context](../40-effective-executor-context-2026-11-02.md) связывает роль, scorer и стадию попытки; [120-post102-transfer-and-executor-audit](https://github.com/lubochka/llm_mvp_core/blob/d18b716e4a855f6dd94415104f48999876108a51/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-post102-transfer-and-executor-audit-2026-11-02.md) — отдельный источник, не этот договор120 и не разрешение повторить recorded PRIMARY. Этот адресный абзац заменяет прежний общий пример только в своём факторе; данные/дозы/команды ниже сохранены.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-CODEX-AUDIT-2026-11-02:START -->
## Действующее уточнение POST107 — Граф должен исполнять полный порядок, возвращённый памятью

**Проверка `POST107-Q127`. WRONG:** два Steps любой природы назвать удержанным порядком либо строить graph по expected route при неверном ответе памяти. **CORRECT:** `PositionalPredicates → native KnnLogicalUnit.Train/ExecuteAsync → graph adapter P5001` связывает input positions, actual ordered PredicateIds, выбранный payload и порядок реально выполненных узлов. Raw p1/p2 отличаются порядком; h1/h2 — отдельный перенос формулировки. Нулевое различие B/A при hash collision сохраняется, не лечится новым dimension/k.

**Честный и испорченный контроль.** Назначенный C меняет labels только p1/p2 и действительно проходит native Train; checker видит actual output, не название arm. В копии graph receipt переставить Steps либо заменить обе строки при прежней длине2: неизменная сверка payload→edges→node trace должна отказать. Old u1–u8/e1–e4 проверяются у текущих обученных экземпляров, не после восстановления чужого S0. Native ID соседа писать только если он реально exposed, иначе `NOT_EXPOSED`.

**Предел и бюджет.** Остаются3 fit/30presentations/36Execute,12cold observations и назначенные12graphs/24node calls. Actual счётчики обязаны учитывать отказ/прерванный graph: недовыполненный узел не записывается выполненным, лишний вызов ради круглого числа запрещён. Это фиксация факта при прежнем бюджете, не увеличение программы. Native loader здесь не доказан; выгрузка свидетельств не становится checkpoint restore.

Основание и общий порядок проверки: [аудит POST107](../../fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md) и [инструкция41](../codex-41-post107-executor-checks-2026-11-02.md). Этот верхний блок уточняет только названные доказательства и будущую приёмку; исходный текст/результаты ниже сохранены, очередь и разрешение запуска не меняются.
<!-- POST107-CODEX-AUDIT-2026-11-02:END -->

<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Позиция слова должна дойти до native KNN

**Проверка `POST102-Q127`. WRONG.** Одинаковый набор слов уже кодирует их порядок, либо правильный frozen oracle доказывает выполнение native Train.

**CORRECT.** A/B отличаются только назначенными позиционными predicates; предсказываемый ответ не включается в address. Использовать actual D08 native learner/consumer, отдельно показать обученную state и3fits/30lessons.

**Адресная приёмка.** Сохранить ordered input/predicate IDs на36Execute и обратный p1/p2 control, соответствие12cold/12graph/24node calls паспортному бюджету. Отсутствующий native loader не подменяется фиктивным checkpoint. Критерий collision сравнивает полный target, а не первое совпадение.

Применять [единый контекст40](../40-effective-executor-context-2026-11-02.md) и [адресный план121](../../fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md): актуальные роли навыков, один рабочий оценщик, ordinary consumer и реакция на test/report failure. Это ограничивает вывод и усиливает приёмку; исходные factor, доза, fixed panel и порядок сохраняются. Исправление saved evidence не является новой PRIMARY, а наличие этого блока не разрешает запуск. Исторические указания ниже действуют только в своей исходной области; при противоречии бюджета/фактора требуется адресное решение до исполнения.
<!-- POST102-AUDIT-2026-11-02:END -->


Исторический статус первоначального назначения от30.10.2026: **DESIGNED_NOT_RUN / SHALL_IMPLEMENT**, Q127; сейчас выше действует **RESULT_RECORDED** двух попыток. Исходный P50 и его результаты сохранены. Тема обучения — оркестрация, профиль D08; задача — порядок review/implement внутри большей задачи. Английский здесь язык входа, не отдельная тема обучения переводу. D07 относится к выбору маршрута/намерения и не подменяет D08 этого опыта.

Очередь: [единый индекс](00-execution-index.md), стабильный ID этой карточки. Не заменять текущий NEXT P-серии этой новой строкой. Последовательность — после Q126, если пользователь прямо не назначит данный ID. После terminal-отчёта не расширять старую задачу и не продолжать её вместо явно указанного пользователем следующего ID.

## Основание и вопрос

[P5001 report](../../../../../experiments-v2/RP01-MICRO/P5001/results/attempt-0001/report.md) показал 2/4→4/4 при замене направления графа по именам на маршрут из обученной памяти. S1 на исходной панели дал успех и без позиционных предикатов. Это не доказывает, что BPE bag-of-tokens сохраняет любой порядок. Контрпример ниже выводится из напрямую перечисленных в этой карточке исходников и разобран в [review30.10, §2](../../fixes-based-on-experiments/reviews/2026-10-30-late-positive-evidence-review.md); [аудит120](../../fixes-based-on-experiments/120-post102-transfer-and-executor-audit-2026-11-02.md) задаёт общие границы проверки, а не служит источником этого вывода: `please review code then implement change` и `please implement change then review code` состоят из одинакового мультимножества RegexSplitter chunks. BPE сливает внутри chunk, а Vectorize суммирует токены. Без позиционных предикатов V(x)=V(y), и детерминированный читатель не может дать разные правильные ответы обоим.

**Гипотеза, ещё не результат:** существующие позиционные предикаты устранят хотя бы часть потери обученного порядка на этой паре и её отложенной формулировке. B может не выиграть из-за хеш-коллизий; это не разрешает менять размерность, oracle или бюджет по ходу.

Продуктовые ссылки: [RegexSplitter](../../../../../01.Tokenizer/RegexSplitter.cs):16–21; [BpeTokenizer.Encode](../../../../../01.Tokenizer/BpeTokenizer.cs):125–134; [KnnLogicalUnit.Vectorize](../../../../../21.AgentInfrastructure/Logical/KnnLogicalUnit.cs):438–469 на исходном срезе cbf3b1d834c. Исправление проверяется в эксперименте, продуктовые исходники не менять. Будущий владелец доменного конструктора — [план74](../../fixes-based-on-experiments/74-knn-domain-constructor-transfer.md); общий графовый consumer пока не назначен этим опытом.

## Место, модель и команда будущего запуска

- Рабочий каталог: `C:/Projects/llm_mvp_core`.
- Новый короткий host: `experiments-v2/RP01-MICRO/KO01/`; создать только `Experiment.csproj`, `Program.cs`, `Run-Experiment.ps1` и при необходимости локальную копию graph adapter. Источник структуры проекта и runner — существующие `P5001/Experiment.csproj`, `P5001/Run-Experiment.ps1`; исходные файлы P5001 не менять, папку результатов/веса/obj/bin не копировать. TargetFramework `net9.0`, Release; AssemblyName/RootNamespace `KO01Experiment`; ProjectReference на `21.AgentInfrastructure/AgentInfrastructure.csproj` и `01.Tokenizer/Tokenizer.csproj`, как у P5001. Исходный runner принимает другой параметр, поэтому обработку `-AttemptId` и KO01-путей добавить в новой копии явно; старый runner не вызывать с новым аргументом.
- Модель обучения: **native `KnnLogicalUnit<string, OrderedRoute>`**, dimensions64, k1, confidenceFloor0. Native Train создаёт `new BpeTokenizer()` и вызывает `Train(trainingText, vocabSize:320)`; этот общий набор и размер одинаковы у A/B/C. `KnnAnswerSnapshot.FromCopy<OrderedRoute>` копирует массив Steps. Это дискретная обучаемая память, не LoRA/нейросеть: optimizer/scheduler/RNG нейронного обучения N/A. Не скачивать иной checkpoint или другую модель.
- Функция B: точная `PositionalPredicates` из [P5001/Program.cs](../../../../../experiments-v2/RP01-MICRO/P5001/Program.cs):1068–1074, `W{position}_{word}`. Общие кодирование строк/нормализация/порядок уроков берутся из того же host. До исполнения сохранить в отчёте точную версию исходников и реальные параметры вызова Train; это provenance, не отдельная программа checksum-тестов.
- Исполнитель остаётся на своей модели по умолчанию; новая модель агента/сервис не назначаются. Перед подготовкой перечитать AGENTS/CLAUDE и `docs/skills/architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `human-communication-GLOSSARY.md`. Для создаваемого host применить реально существующие навыки кода/тестирования, указанные AGENTS. Отчёт — русский, по форме ниже.

Будущий runner должен принимать `-AttemptId`; первый вызов после создания host:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KO01/Run-Experiment.ps1 -AttemptId attempt-0001
```

Это назначенная команда **ещё не созданного host**, а не утверждение, что её уже можно исполнить. Подготовка executor включает SDK/package restore по P5001 и сборку локального host до начала измерений; отсутствие зависимости не считать отрицательным результатом обучения. Продуктовые тесты и полную цепочку внедрения здесь не запускать. Папка результата `KO01/results/attempt-0001/`. Если имя занято, выбрать первый свободный `attempt-NNNN`, сохранив старые файлы. Не удалять/переписывать прошлые попытки; не смешивать прогресс двух процессов.

## Учебные данные: один предмет, два класса порядка

| ID | TRAIN | Маршрут / Steps |
|---|---|---|
| u1 | review code then implement change | review-first / review, implement |
| u2 | inspect code then implement change | review-first / review, implement |
| u3 | review module before editing it | review-first / review, implement |
| u4 | inspect module before editing it | review-first / review, implement |
| u5 | implement change then review code | implement-first / implement, review |
| u6 | implement change then inspect code | implement-first / implement, review |
| u7 | edit module before reviewing it | implement-first / implement, review |
| u8 | edit module before inspecting it | implement-first / implement, review |
| p1 | please review code then implement change | review-first / review, implement |
| p2 | please implement change then review code | implement-first / implement, review |

Маршрут обозначает класс, Steps — фактический порядок. Если исходный DTO использует другое строковое имя класса, единожды сопоставить его с двумя показанными Steps, записать соответствие и одинаково применить к A/B/C; не менять тексты/порядок уроков. Нельзя обучать на ID или ожидаемой метке вместо текста.

| ID | EXAM, не передавать в Train | Правильные Steps |
|---|---|---|
| e1 | review code before implementing change | review, implement |
| e2 | inspect code before implementing change | review, implement |
| e3 | implement change before reviewing code | implement, review |
| e4 | implement change before inspecting code | implement, review |
| h1 | kindly review code then implement change | review, implement |
| h2 | kindly implement change then review code | implement, review |

A/B получают ровно те же10 уроков, в указанном порядке. A — пустой PredicateIds; B — PositionalPredicates на входном тексте при Train **и** при ExecuteAsync. Все прочие настройки одинаковы. Нельзя одновременно менять k, расстояние, пороги, parser или graph adapter. GoldRoute/Steps/ID/Kind не доступны функции признаков. Исторические e1–e4 — положительные контроли; p1/p2 проверяют сохранение выученных различий, h1/h2 — отдельный перенос на формулировку.

## Обучение, чтение и полный бюджет

1. Записать по6 холодных `NO_TRAINED_UNIT` наблюдений для A/B; это12 наблюдений отсутствия модели, **не12 вызовов несуществующего learner**. Холодная точность не присваивается выдуманному выходу.
2. Выполнить два настоящих native Train по10 уроков. Общий tokenizer corpus одинаков; не переобучать B на EXAM и не объявлять отдельный tokenizer-witness восстановленным состоянием самой единицы. В текущем native KNN доказанного общего loader нет, поэтому этот рецепт честно обучает обе модели; `P5001/state-after.json` не является допустимым checkpoint для него.
3. На каждом плече выполнить10 retention +6 EXAM =16 обычных ExecuteAsync. Ожидаемый baseline A на одинаковой паре выдаёт один и тот же обученный класс, значит1/2; проверить actual outputs. Если получились0/2 из-за отказа/неизвестного класса, не выдавать score за обучение: зафиксировать непригодную постановку, не объявлять выигрыш. У B измерить результат, не обещая2/2 заранее.
4. Только на EXAM построить и исполнить существующий небольшой graph adapter P5001 из полученных Steps:12 графов, по2 вызова узлов. Порядок графа берётся из ответа памяти, не из gold или имён узлов. Считать пропущенные/лишние рёбра и циклы.
5. Отдельный контроль C: ещё один native Train на тех же10 строках с признаками B, но поменять местами ответы **только p1/p2**. Затем4 ExecuteAsync на p1/p2/h1/h2. Без новых графов. Нужна чувствительность к изменённым урокам в той части панели, где B различает ответы; неизменный результат отмечается как незакрытый контроль, не лечится ещё одним fit.

Вся программа: **3 fit, 30 предъявлений уроков, 36 ExecuteAsync, 12 холодных наблюдений, 12 графов, 24 вызова узлов**. DPO/SFT/внешний teacher=0. Обучение — реальные записи native памяти и fit tokenizer, а не чтение готовых чисел. Длительность: ceiling20 минут на build+run, вычислительная часть ожидается краткой по P50; ждать terminal outcome, не завершать раунд вместо ожидания. Автоматических повторов/поиска гиперпараметров0. Технически исправленная попытка получает новое имя и отдельно отражённую стоимость.

Записать уже подготовленные тексты, PredicateIds и фактические ответы. Равенство V у A — аналитическое следствие кода, а не требование извлечь private вектор через reflection. ID соседа записывать только если его возвращает штатный результат; иначе явно `NOT_EXPOSED`, без нового инструментирования продукта. Не создавать дополнительный tokenizer-witness/fit ради печати признаков: он вышел бы за бюджет3fit и не был бы состоянием native единицы. Коллизии позиционного хеша, если они объясняются доступными признаками, описывать как границу, не выдумывать измерение скрытой памяти. Это объяснение реально выполненного обучения; отдельный surrogate-reader не заменяет native learner.

## Успех, потери, отчёт и остановка

Любое измеренное улучшение полезного показателя считается успехом этого показателя и темы; рост общей точности не обязателен. Здесь основной показатель — различение правильного порядка p1/p2 и отдельно h1/h2; прежние u1–u8/e1–e4 обязательны рядом. Для каждой панели вывести A/B/C, приобретённые и потерянные ID, абсолютную разницу и относительную там, где знаменатель ненулевой. Улучшение p1/p2 не называть обобщением. Улучшение score при0→0 не принимается.

Измерять одинаковые native fit/read границы у A/B: wall, CPU, allocated bytes; retained/peak — только если реально измерены раздельно. Сохранить полную стоимость C и подготовки. Общий пик последовательного процесса не объявлять ценой одного плеча. Порог допуска будущего общего переноса: >5% полезного улучшения без >5% ухудшения других осей, кроме CPU, либо отдельное основание явной логической целостности. Меньший полезный сдвиг сохраняется кандидатом второй очереди. Никакого автоматического изменения продукта по результату этого host.

Выходы в одной attempt-папке: `report.md` (по-русски), `metrics.json` (по строкам/панелям/ресурсам), `learning-events.json` (фактические fit и уроки), `status.json` (terminal status, exit code), `build.stdout`, `run.stdout`. Человеческий отчёт: вопрос → что обучено → до/после → полезность по теме → потери → границы вывода → кандидат или отрицательный результат → следующий ID из индекса. Не выдавать manifest/описание данных за восстановимую модель; новую схему сохранения этот опыт не реализует.

**Запрет повторяемого поведения:** после прямого указания пользователя перейти к следующему эксперименту прекратить расширение этого опыта; после «остановись» не делать новых вызовов, исправлений или журналов ради «завершения». Во время разрешённого запуска использовать один handle и штатное блокирующее ожидание terminal результата с отрезками до60с; не заменять ожидание завершением раунда, `Start-Sleep` без проверки его действия или частым чтением ещё не созданного файла. Если timeout20 минут достигнут, сохранить достоверную причину и фактическую стоимость, не запускать скрыто вторую копию. Сейчас разрешена документация; создание/исполнение host требует отдельной команды пользователя.
