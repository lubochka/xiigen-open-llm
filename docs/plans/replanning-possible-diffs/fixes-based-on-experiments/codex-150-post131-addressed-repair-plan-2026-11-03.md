# Адресный план после P131: исправить причину и доказать прохождение нужного пути

**Применение после приёмки Q01, 4 ноября:** [161](codex-161-postq011-executor-context-repair-2026-11-04.md) уточняет Q02 RULE и scope каждого expected. Два builder/import ожидания меняются на `TENSE=PAST`; отдельные legacy PAIR-removal случаи сохраняют `TENSE_PAST`. Это не глобальная замена строк. Сохранить достижимый floor и положительные RULE-контроли в общем фильтре. Ссылка ниже на прежний первый Q01 историческая: он принят по 29/29 в своём охвате; следующий общий перенос Q02, другие остатки этим не закрываются.

Дата: 3 ноября 2026. Основание: [аудит149](codex-149-post131-transfer-and-experiment-audit-2026-11-03.md), core `b27783593745ffac8e74e81f917539717cd33615`, planning `9aef94e0aa02f462141ffb82331eccb96da282a5`. Этот план уточняет [146](codex-146-post128-addressed-repair-plan-2026-11-03.md), а не создаёт новую очередь. [Executor49](../experimets/codex-49-post131-output-binding-and-test-coverage-2026-11-03.md) содержит команды и полные проверяемые контракты. [151](codex-151-post131-card-coverage-2026-11-03.md) перечисляет уже внесённые документные дельты.

## 1. Порядок и исполнители

При прямом назначении конкретного ID выполнять его. Для общей очереди переноса сохраняется **Q01/135-R01-CONCURRENCY (+R17) → Q02 → Q03 → Q06**; Q04 HOLD, Q05 зависит от Q04. Перечисленные ниже R131 — разделы ремонта, не новые экспериментальные Q. Результаты всех131 опытов уже записаны; данный план не назначает Q132 или повтор PRIMARY.

| Раздел | Владелец существующей работы | Следующий ограниченный результат | Граница закрытия |
|---|---|---|---|
| R131-01 | R128-06 / QA46 | Исправленный filter + manifest/discovery/TRX coverage | Все обязательные имена реально исполнены на нужной сборке; остальные failures выбранного набора видны |
| R131-02 | R128-06, tail-signature tests | Два exact expected и достижимая floor fixture | Различающие проверки passed нативно, прежний дефект/неверная ветка rejected |
| R131-03 | R128-03 / material lineage | SourceRowId разрешён внутри independently matched content/family | Законные дубли приняты, пересечение отклонено по назначенному коду до Train |
| R131-04 | P131 / saved verifier repair | Native output binding всех128 сохранённых строк | Honest → порча16 raw-строк → honest через реальный verifier и внешний exit |
| R131-05 | P127 / finalizer repair | Route/ordered payload/существующий graph trace согласованы | Несогласованный payload отклонён; согласованный неверный C сохранён как научный минус |
| R131-06 | Владельцы карточек / parent | Q130 RESULT_RECORDED, собственные ограничения соседних карточек | Действующий вход и копируемый prompt согласованы; старые паспорта и Q-строки сохранены |
| R131-07 | Q01, T121/R128-02 и владельцы прежних остатков | Продолжить незакрытые product boundaries из146 | Только отдельные настоящие consumer-тесты могут закрыть соответствующую строку |

В текущем аудите R131-06 и документные части остальных строк выполнены. Product/test/verifier repair и native acceptance — **не выполнены**: .NET/PowerShell в среде отсутствуют. Назначенный исполнитель на штатной машине начинает с указанного исходника и свежей сборки; нет оснований повторять исследовательское обучение ради исправления отчёта.

## 2. R131-01: сначала доказать состав запуска

**Изменяемые файлы:** назначения в03-test-and-execution-contract, QA46,146 и конкретном тестовом наряде; при реализации automation — один явно названный проверяющий discovery/TRX, без изменения product semantics. Машиночитаемый [required-test-cases.json](../../../../experiments-v2/_audits/post131-2026-11-03/required-test-cases.json) задаёт четыре исторических failures и два положительных контроля.

Фильтр сохраняет `FullyQualifiedName~ExperimentFixes` и добавляет через OR полные имена классов `LlmFromScratch.Tests.TranslationPairTailSignatureLearningTests` и `LlmFromScratch.Tests.TranslationVerifiedRuleTagTests`. Нельзя заменить весь набор только шестью методами и объявить Q01 принятым.

**Положительный путь:** финальный source/dirty manifest → fresh Release build → test DLL hash → discovery шести FQN → тот же filter/сборка → сырой TRX → сопоставление TestDefinitions/TestMethod/testId/executionId/UnitTestResult. Для соседних Theory учитывать реальные data rows. Полный командный пример находится в49.

**Отрицательный контроль:** пропустить один required FQN в копии списка discovery/TRX, сохранив ненулевой total и Failed=0. Coverage verifier должен отказать по missing case. Аналогично required Skipped/Aborted/неоднозначный ID не закрывает контракт. Это QA automation, не новый тест вымышленного поведения продукта.

**Доказательство:** команда/exit, SDK, source и DLL hashes, четыре failures и два controls с фактическим исходом, полный результат выбранного семейства, сырой TRX в доступном месте с SHA256. Если raw отсутствует, писать «по опубликованной сверке», а не «лично пересчитан». Zero-tests guard остаётся, но не заменяет coverage.

## 3. R131-02: три tail failures — два вида ремонта

**Файл:** `Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs`; независимые controls — `TranslationVerifiedRuleTagTests.cs`; продуктовые consumers для сверки — `TranslationVerifiedRuleTags`, `TranslationStructuredBatchBuilder`, `ExecutorUnit`, `TranslationKnnStore.FindExactWithSourceGuard` (`TranslationUnitContracts.cs`).

### 3.1. Два exact expected

В `VerifiedRuleTags_SurviveWhileTheProvenanceSegmentIsGone` и `ImportedCheckpoint_StoresTheLessonUnderACanonicalKeyWithoutProvenance` заменить обучающее/import ожидание `RULE:TENSE_PAST` на утверждённое `RULE:TENSE=PAST`, исправить относящиеся комментарии. Основание — план105 §4.4, не случайно наблюдённый actual. Сохранить no-PAIR, PAST/PRESENT и запрет метки rejected, import/restore, положительные C01/C02.

**Различающий контроль:** chosen PAST/rejected PRESENT и обратная пара должны сохранять ровно chosen canonical tag. Неверная метка rejected или сохранённый PAIR должны по-прежнему проваливать проверку. Чистые provenance-канонизаторные тесты с legacy underscore не переписываются автоматически.

### 3.2. Достижимый source floor

В `ForeignProvenanceInTheKey_DoesNotLiftTheOrdinaryEnRuSourceFloor` восстановить достижимость одного кандидата. Ordinary positive использует одинаковый canonical key в store/query. При сохранении проверки legacy PAIR seam записать и спросить одинаковый raw key; явно отличить этот direct seam от текущего parser output. Prefix/structure miss не должны выдавать себя за проверку floor.

Урок `alpha beta omega`: positive `alpha beta` достигает кандидата, overlap1,0, возвращает learnedAnswer; negative `alpha beta gamma delta epsilon` достигает того же кандидата, overlap0,40, проходит0,35 и отклоняется0,60. Зафиксировать stage/tier/candidate. Старую неверную ветку ordinary floor0 проверить как различающий контроль: она принимает negative. Никаких новых product heuristics и изменения0,60.

**Приёмка:** свежая сборка и точные F01/F02/F03/C01/C02 из manifest плюс прежнее семейство. Исправление fixture не доказывает закрытие полной задачи FIX-P52-TAGS: согласованность Tokens/RuleTags/full/prefix/structure остаётся отдельным существующим договором105.

## 4. R131-03: lineage должен разрешать честный ID, а не первую строку

**Файлы:** `18.CapabilityTraining/CapabilityMaterialLineageGuard.cs`, существующие `CapabilityMaterialLineageTests`; получатель — обычный admission до любого learning. Не менять обучающее содержимое fixture, чтобы скрыть resolver defect.

**Алгоритм:** независимо найти trusted rows по canonical content/digest → потребовать единственную family → найти declared SourceRowId среди действительно совпавших rows → взять role из выбранной trusted row → применить отдельно заданные Train/Dev/Test политики. Не разрешать произвольному claimed ID выбирать чужое содержимое и не использовать `rows[0]` как authority.

**Положительные случаи:** два одинаковых lawful Train-content с разными честными IDs; тот же набор trusted rows в другом порядке; lawful policy combination из146. **Отрицательные:** неизвестный ID, ID чужого content, несколько семей, запрещённое Train/Test пересечение. Последнее обязано достичь `CAPABILITY_MATERIAL_FORBIDDEN_CONTENT_OVERLAP` до Train; ранний `CAPABILITY_MATERIAL_LINEAGE_CONTRADICTS_CONTENT` не засчитывается как нужная защита.

Селективные Dev/Test flags проверить раздельной матрицей146; существующий OR и общие heldOut sets означают известный ложный отказ, не доказанную утечку в обучение. Приёмка source-row выбора не закрывает остальные DPO owner/recipient границы.

## 5. R131-04: P131 — настоящий ответ является источником actual

**Файлы:** `experiments-v2/RP01-MICRO/P13101/SavedEvidence.cs`, `Scoring.cs`, `Provenance.cs`, `Contract.cs`, `Terminal.cs`. Существующий `Run-Experiment.ps1 -Mode VerifyOnly -InputRoot ... -VerificationOutputRoot ...` уже отделяет input/output, не строит модель и передаёт реальный exit. Сохранить этот путь; runner менять только для явно необходимых аргументов версии/архива. PRIMARY HEAD guard не отключать. Исторические producer исходники, proof, configuration и raw не редактировать.

### 5.1. Сначала разделить происхождение producer и нового verifier

**Зависимость исполнимости, найденная независимым review плана.** `Provenance.Verify` сейчас требует, чтобы текущий HEAD и загруженная Program DLL совпали с producer, и проверяет619 абсолютных input/assembly paths. Исторический producer: `ca2deeedea88fe909935694ff50dec49ab3de04b` плюс записанная dirty delta. Новый checker изменит DLL и source bytes. `SavedEvidence` также требует равенства `configuration.requiredGates` текущему `Contract.RequiredGates`: добавление11-го gate сделает старый честный список10 несовместимым. Обычная пересборка и запуск прежнего verifier поэтому откажут раньше целевого binding.

**Выбранный ремонт:** версионированный saved reanalysis через существующий VerifyOnly. Передать неизменный producer bundle и отдельную verifier identity. Исторический `SourceProof` проверять против архивных producer source/DLL/PDB bytes и записанной local delta; новый verifier записывает собственные source/dirty/schema/assembly hashes и команду. Старый producer hash не сравнивать с новым исполняемым verifier DLL и не подменять новым hash. Живой producer `Provenance.Verify` сохраняет строгие правила; для архива нужен отдельный явно названный путь `VerifySavedProducer` с равносильной проверкой исходных bytes.

Старые абсолютные619 путей разрешать через явную однозначную mapping-таблицу на архив, сохраняя исходный ключ и hash. Не искать произвольно по basename и не допускать выход из объявленного archive root. Если original source/DLL/PDB недоступны, перечислить missing entries: полная producer provenance остаётся NOT_PROVEN, общий ACCEPTED запрещён. Доступные raw/native score и binding можно сообщить отдельными частичными результатами; это не снимает проверку происхождения и не разрешает новое обучение.

Historical producer schema/data pins/порядок10 required gates проверяются по закреплённому контракту этой попытки. Current verifier schema/required checks определяются отдельно и обязаны включать `NATIVE_OUTPUT_BINDING`; `Terminal`/общий exit использует **текущий** обязательный список. Не добавлять новый gate задним числом в старый configuration.json и не принимать self-declared список отчёта как authority. Явная совместимость old schema→current verifier поддерживает именно этот неизменный пакет; неизвестная схема отказывает.

**Контроли перехода версии:** полный доступный архив старого producer + новая verifier сборка принимаются без изменения исторических bytes; порча архивного producer source/DLL, неоднозначная mapping, неизвестная historical schema и отсутствие нового mandatory gate выявляются отдельными причинами. Если исходных архивных bytes нет, нельзя конструировать фиктивный honest PASS. Сначала отметить точный NOT_PROVEN; нативный положительный upgrade-control требует настоящий полный архив. Затем выполнить binding-контроли ниже.

### 5.2. Подготовить именно checker, затем принять внешний результат

**Отдельный обязательный наряд сборки:** выполнить [договор49 §5.1](../experimets/codex-49-post131-output-binding-and-test-coverage-2026-11-03.md#51-отдельная-сборка-и-тесты-проверяющего-p131). До Rebuild сохранить и проверить original producer bytes. Собрать `P13101/Checks/Checks.csproj` (включает Probe) и `P13101/Experiment.csproj`, выполнить discovery и все проверки первого проекта на той же сборке. В49 назначены точные14 прежних FQN; новые binding/migration случаи добавляются к ним, число14 не является верхним пределом. Шесть FQN общего Tests проекта не покрывают checker. `VerifyOnly` никогда не строит DLL; PRIMARY/ResumePreparation для этой подготовки запрещены. Артефакты сборки, TRX и source/DLL identity checker сохраняются отдельно от producer proof. Затем выполнить saved-only outer проверки ниже.

**Вход:** сохранённый `experiments-v2/RP01-MICRO/P13101/results/attempt-0001/continuation-0001/evidence`, pinned source и gold. Здесь задан адрес от корня repo; в фактический вызов передать абсолютные InputRoot/VerificationOutputRoot по49: child WorkingDirectory=hostRoot, исходные относительные аргументы не нормализуются перед передачей. `attempt_root` — родительский continuation, не InputRoot; `producer_archive_root` — отдельные подлинные source/DLL/PDB с mapping619. Negative: родительский continuation без `/evidence` должен дать layout failure/nonzero, без model calls. **Выход:** новый `verification_output_root`, которого не было; input inventory/SHA256 до и после одинаков. `Train/Predict/Flow/Load = 0`. P131 уже имеет `Contract.Separate` и создание нового output; сохранять эту защиту. Известный writable-root остаток относится к P129 и не назначается P131 повторно.

**Алгоритм:** вывести ожидаемые128 `(arm,id)` из паспорта/данных → проверить точное множество и кратность raw/native → проверить native `isSuccess`, отсутствие error и обязательную value/flowResult форму; связать `flowResult.summary.summaryId = translation-routing-flow-P131-{arm}-{id}` с ключом; сверить actual, verdict, selected flow, полный metrics/warnings и производные adapter/confidence/state поля → получить actual для exact scoring из native payload → отдельно вычислить procedure integrity, научный score и transfer status. Gold берётся из назначения до actual. Manifest фиксирует source/run и byte hashes; self-resealed inventory сам по себе не заменяет происхождение данных.

**Приёмка:** полный честный пакет проходит; изменение actual+translated_length в16 H/X строках B.after/B.restored при неизменном native отвергается `NATIVE_OUTPUT_BINDING` через настоящий verifier process и внешний ненулевой exit; восстановленный честный пакет снова проходит. Согласованные настоящие H/X0/4 не должны делать процедуру INVALID. Проверить отсутствие/дубль ключа как отдельную completeness-порчу без лишних Train.

**Сохраняемые результаты:** P8/8, N4/4, H/X0/4, C exact assigned4/4; confidence0,99 у восьми ошибочных H/X; reader parity; checkpoint9642→13651;17Export. Не подменять научный минус требованием обязательного отказа модели или вручную исправленным ответом. Нативная приёмка нового gate остаётся UNRUN до её выполнения.

## 6. R131-05: P127 — целостность и причинный контроль раздельны

**Файлы:** `P12701/Program.cs` (`LabelSensitivityHolds`, `BuildFinalizer`, `ComparePermutation`) либо выделенный явно назначенный saved verifier с безопасным output. Исходные lessons, результаты обеих попыток и оплаченное обучение сохраняются.

**Контракт целостности:** `review-first` соответствует `[review,implement]`, `build-first` — `[implement,review]`. Неизвестное route не превращать default-веткой в известное. Для A/B найти graph trace именно по `(arm,caseId)` и связать steps, edges и журнал; C trace не существует и не добавляется. Перестановочный comparator сравнивает полный ordered payload, а route/neighbors/confidence сообщает отдельно.

**Научная метрика:** отдельно от прежнего `StepsCorrect` вычислить `assigned_target_match` для C: p1 `[implement,review]`, p2 `[review,implement]`. Неверное следование назначенным targets означает отрицательный/NOT_PROVEN причинный результат, если настоящий ответ записан согласованно. Оно не должно искусственно требовать exit failure для честного опыта.

**Конкретная переклассификация старого gate:** в `BuildFinalizer` убрать научный `mandatory["label_sensitive_c_holds"] = LabelSensitivityHolds(...)` из процедурного all-pass и записывать его/точный `assigned_target_match` как scientific observation. Нельзя просто заменить label inequality на exact C-target внутри того же mandatory: это снова сделает честный научный минус INVALID. Integrity содержит непротиворечивость самого DTO и связь с действительно сохранённым producer/trace. Исторический status с девятью checks не переписывать; новая квитанция называет свою схему и новый список процедурных проверок. Контроль с согласованным настоящим C=B обязан сохранить integrity VALID при scientific false; отдельная адресная порча evidence обязана дать ненулевой exit.

**Косвенные зависимости тоже убрать из procedural all-pass.** `CSubstitutedByBControl` вычисляет `control_passed = honestHolds && !substitutedHolds`, а `BuildFinalizer` включает его как `c_substituted_by_b_control_passed`. Поэтому удаления одного `label_sensitive_c_holds` недостаточно: настоящий C=B всё ещё отвергался бы косвенно. `CorruptedReceiptControl` аналогично требует `honestVerdict.OrderCorrect` для настоящего B/e1, то есть победу learner относительно gold, через `corrupted_receipt_control_passed`.

Переработать оба control path вместе с finalizer: положительный контроль integrity принимает любой полный непротиворечивый actual, в том числе научно неверный; отрицательный меняет route/steps либо записанный producer/trace при неизменных остальных предпосылках. Для ответа из двух разных узлов проверять точное множество и порядок по договору DTO; одинаковое имя дважды не является полным payload. Прежний C→B label-sensitivity observation хранить в scientific diagnostics, не использовать его истинность напрямую или через helper как обязательство успеха learner. Если нужен unit-control функции gold scoring, использовать отдельно помеченный синтетический positive/negative fixture самого scorer; его успех не является научным результатом B/C и не подмешивается в historical raw.

В файлах ремонта явно указать также `CSubstitutedByBControl` и `CorruptedReceiptControl`. Проверка после правки должна пройти **весь** новый finalizer, а не только новый предикат: согласованный настоящий C=B и согласованный научно неверный B/e1 сохраняют procedure VALID; разрыв route/steps/producer/trace даёт INVALID/ненулевой outer exit. Новых fit/Predict/graph-node calls для этих saved/validator controls нет; исторические результаты и девять checks не переписываются.

**Различающие контроли:** (1) C route при B steps — integrity failure; (2) другой журнал сохранённого A/B графа — binding failure; (3) полный синтетический согласованный fixture C=B/неверного B проходит validator с scientific false, но не становится historical accepted run; (4) изменённые steps при прежнем route не исчезают из permutation comparison. Baseline, адресная порча и явно synthetic fixture проходят полный finalizer/внешний exit без fit/Predict/graph execution. Первые Python-контрпримеры из149 — локализация, не native acceptance.

**Полнота P127 (`P127-SAVED-KEYS`).** Для каждой выбранной исторической попытки отдельно требовать 68 уникальных `(Arm, CaseId)` в `metrics.reads`: `{A,B,A-perm,B-perm} × {u1…u8,p1,p2,e1…e4,h1,h2}` (64) и `C × {p1,p2,h1,h2}` (4). Не объединять две попытки в один набор. Графы: ровно `{A,B} × {e1…e4,h1,h2}` — 12 ключей. Cold observations и контрольные квитанции учитываются отдельно. Проверить точные множества, кратность и обязательные поля до `All()` и scoring: empty, missing, extra, duplicate и повреждённый payload `A-perm/B-perm` — разные отрицательные случаи. Счётчики 36+32 и 12 не доказывают присутствие строк. `BuildFinalizer` должен принимать также permutation records; прежний `armA.Concat(armB).Concat(armC)` их пропускал.

**Предел первоисточника P127.** `ReadOutcome` формируется из native ответа в `Program.cs:697–724`, затем сохраняется в `metrics.reads` (`459–482`). Отдельного P131-подобного `native-flow` для каждой строки нет. Проверка неизменности опубликованных bytes, DTO и имеющегося A/B graph trace устанавливает внутреннюю согласованность доступных свидетельств; она не создаёт вторую независимую запись ответа и не доказывает полный historical runtime. Отсутствующий native payload обозначить `NOT_RECORDED`; не вызывать модель и не изготовлять «оригинал» из summary. Результат и предел provenance сообщать раздельно.

**Обязательная внешняя authority после Q003 (`P127-HISTORICAL-OUTPUT-AUTHORITY`).** Конкретный набор — [15-file manifest](../../../../experiments-v2/_audits/postq003-2026-11-03/codex-p127-trusted-historical-output-manifest.json), source commit `7ce6b14ca7bb353ed5f57179f0561053496d36ae`. Закреплено: `manifest_commit=f3b64f87159193c2897092d6e571b2b65e3b75c4`, SHA256 manifest `df98d7354296396164b51ce583370f4d10dbc46f2e3527e1b67a5f265b1fca84`; `results_commit=7ce6b14ca7bb353ed5f57179f0561053496d36ae`. Читать manifest из этого commit вне изменяемого input_root; его authority не брать из проверяемой копии. Проверить exact result files выбранной попытки по Git blob/SHA256/bytes; `build-identity` и inventory до/после не доказывают историческое происхождение outputs. Согласованная подмена route+steps+graph+summary+local hashes под прежним attempt должна дать nonzero по внешней authority. Полный synthetic fixture имеет отдельную identity/test mode и не становится историческим запуском. Истинность выводов старого report не следует из authentic bytes; поправки153 сохраняются. Реализация и native outer acceptance остаются UNRUN до исполнения155§5.

**Три разных контроля (`SYNTHETIC-IS-NOT-HISTORICAL`).** (1) Неизменный настоящий пакет проверяется с его фактическими ответами, в том числе неверными по gold. (2) Копия исторического пакета с изменёнными route+steps под прежней identity/proof — повреждённое свидетельство, даже если DTO внутренне согласован; такая копия не становится VALID историческим запуском. (3) Для сценария «настоящий C=B или неверный B/e1» при отсутствии такого сохранённого запуска автор ремонта создаёт явно синтетический **полный** fixture validator/finalizer с собственной synthetic identity, всеми назначенными записями и согласованными A/B traces; C trace не добавляет. Он проходит тот же finalizer и outer exit, но только в явно отделённом режиме тестирования validator, и никогда не попадает в научные метрики или accepted historical runs. Это проверка логики checker без Train/Predict/graph, не новый опыт обучения. Unit-тест одного scorer не заменяет этот контроль; при невозможности получить полный fixture оставить приёмку открытой, не подделывать producer proof.

## 7. R131-06 и профилактика на соседних карточках

В действующем Q130 заменить future-задачу на RESULT_RECORDED/ST01 и текущую проверку сохранённых свидетельств; копируемый prompt получает те же ограничения. Сохранить частный gain2/4→4/4,32native pairs и resource HOLD; не добавлять новую серию методов или persistence API ради формального gate.

Для соседних карточек переносится **конкретная причина ошибки**: actual↔native для reader/restore; route↔steps для порядка и graph; teacher-forced metric↔observations для P126; source/target KindBag↔native memory для METHOD; candidate reachability и точные required FQN для test contract. В каждой вставке есть WRONG/CORRECT и путь до consumer. Полный перечень в151 отличает найденные дефекты от профилактических дополнений. Нижние исторические паспорта не служат текущим запуском.

## 8. R131-07: прежние продуктовые остатки не исчезли после новых опытов

Следующий общий перенос — Q01. Сохранить BOM/hash fix и R17. Для CAS вернуть единый результат попытки публикации, содержащий success и **объект/версию, непосредственно возвращённые CompareExchange**; не читать CurrentVersion позже для поля observed. Различающий сценарий: A проигрывает B, C публикуется до записи artifact A; artifact обязан назвать B как observed при CAS, а текущим победителем может уже быть C.

После `BeforeMeasuredAccuracyPublishBarrier` проверить cancellation до публикации measured accuracy. При отмене сохранены прежние state/holder/publication/accuracy и candidate lifecycle по договору; соседний положительный случай без отмены публикуется. Это отдельный путь Evaluate, не повтор уже исправленного Train cancellation.

Доказать ordinary store wiring и consumer acceptance другим процессом на настоящих bytes/hash; не закрывать это одним fake store или сериализацией DTO в том же процессе. R17/readback/hash остаются. Конкретные файлы и tests Q01 заданы146 и135; шесть FQN этого плана их не заменяют.

Для R128-02 проверить recipient identity (capability + model instance + dataset/replay identity по существующему договору), обязательное owner admission и доступность публичного обходного constructor. Положительный legitimate transfer и отрицательный cross-recipient/replayed input должны достигать реального потребителя; не закрывать safety одним registry helper. Полный caller audit в этом срезе не выполнен, поэтому существующая карта работ сохраняется.

P126 finalizer/qualification, P128 full state/outer exit, P129 typed payload/finite numeric branch/реальный третий verifier exit/input-output продолжают существующий ремонт145/146/48. P129 Say-only уже исправлен, четыре helper-пары уже приняты: повторно не назначать. Строка NaN, отвергнутая при JSON/type parsing, не доказывает достижение finite-value predicate. Восстановление writer из checkpoint перед failure snapshot запрещено прежним договором: оно могло бы скрыть неатомарную мутацию.

## 9. Когда можно изменить статус

Для document repair достаточны просмотр родителем, согласованность карточки/prompt/очереди, проверка ссылок, diff и сохранность исторического содержимого. Для code/test/verifier repair нужны свежая нативная сборка, покрытие требуемых случаев, actual consumer/exit и сохранённые доказательства. UNRUN оставляет эту приёмку открытой.

В итоговой квитанции отдельными строками: что изменено; почему это исправление контракта; source/DLL/input identity; что реально исполнено; expected/actual положительного и отрицательного примеров; процедура; научный результат; продуктовая приёмка; оставшийся следующий срез. Документ и скрипт аудита сами по себе не превращаются в рабочий product transfer.
