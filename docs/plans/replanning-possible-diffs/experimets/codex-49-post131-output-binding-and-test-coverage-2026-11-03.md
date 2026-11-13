# После P131: связывать сохранённый ответ с native payload и доказывать состав тестового прогона

Дата: 3 ноября 2026. Действующее адресное дополнение после аудита P131.

Основание: core `b27783593745ffac8e74e81f917539717cd33615`, planning `9aef94e0aa02f462141ffb82331eccb96da282a5`; [аудит149](../fixes-based-on-experiments/codex-149-post131-transfer-and-experiment-audit-2026-11-03.md) и [план150](../fixes-based-on-experiments/codex-150-post131-addressed-repair-plan-2026-11-03.md). Это дополнение к [QA46](46-qa-execution-test-repair-and-fresh-results-2026-11-03.md), [executor48](codex-48-post128-executor-verification-2026-11-03.md) и [плану146](../fixes-based-on-experiments/codex-146-post128-addressed-repair-plan-2026-11-03.md). Оно уточняет задания и приёмку. При его подготовке source, tests, паспорта, PRIMARY и исторические результаты не менялись; новый нативный прогон не выполнялся.

## 1. Живой контекст и предел этого наряда

По [живому индексу](cards/00-execution-index.md#experiment-next) записаны результаты **131 из 131** назначенных опытов. Это счётчик записанных результатов, а не 131 доказанный перенос или 131 успешная гипотеза. Q132 не назначен. Не запускать снова P127–P131 из-за исторических формулировок `DESIGNED_NOT_RUN` или устаревшего счётчика в executor48.

В [очереди переносов](../fixes-based-on-experiments/01-registry.md#executor-next) следующим остаётся **Q01 / 135-R01-CONCURRENCY**, затем действующий порядок живой очереди. Исправление фильтра и шести тестовых случаев ниже не закрывает Q01: observed CAS version, поздняя отмена Evaluate, штатное подключение store и восстановление другим процессом имеют собственную приёмку. Сохранить уже выполненную byte-writer починку b4 и R17.

Два разных вида работы не смешивать:

- Назначенный ремонт продукта/тестов: свежая сборка, discovery, требуемые тесты и адресные отрицательные контроли. Штатные вызовы внутри изолированных тестовых fixtures разрешены границами этого ремонта.
- Повторная проверка **сохранённых** P127/P131: только чтение старых данных и вычисление новых квитанций; **Train = Predict = Flow = Load = 0**. Никакой запуск PRIMARY, новое обучение или новый эксперимент для такой проверки не нужны.

## 2. Три независимых результата

Каждой проверке заранее назначить `check_type`. Итог хранит три отдельных поля; общий зелёный цвет не заменяет их.

| Поле / тип | Что означает | Пример допустимого отрицательного результата |
|---|---|---|
| `procedure_integrity` | Нужные данные присутствуют, относятся к назначенному run, не противоречат друг другу; verifier проверил обязательные границы | Повреждение raw при неизменном native payload — INVALID. Честный неправильный перевод при совпадающем raw/native — VALID |
| `scientific_result` | Измеренная точность, различие плеч, перенос, удержание и причинный контроль | Целевой ответ C не получен, B не лучше A, H/X остались 0/4: отрицательный результат или NOT_PROVEN при VALID процедуре |
| `transfer_acceptance` | Назначенный обычный продуктовый путь выдержал свой договор и требуемую регрессию | Опытная копия улучшилась, но тест штатного пути не исполнен: приёмка остаётся открытой |

**WRONG:** требовать победы B или точного ответа C, чтобы verifier вернул exit0; после честного промаха объявлять процедуру испорченной. **CORRECT:** exit0 для полной непротиворечивой процедуры совместим с отрицательным `scientific_result` и открытой `transfer_acceptance`. Научные промахи не переписываются в правильные ответы. Противоречивые поля, пропущенные обязательные данные и неисполненная проверка не превращаются в VALID.

## 3. Фильтр: непустая выборка ещё не покрывает известные падения

В плане146 использован `FullyQualifiedName~ExperimentFixes`. Однако путь файла и namespace различаются: `TranslationPairTailSignatureLearningTests` и `TranslationVerifiedRuleTagTests` лежат в каталоге `Tests/ExperimentFixes`, но их namespace — `LlmFromScratch.Tests`. Поэтому прежний фильтр включает lineage-тест и другие классы этого семейства, одновременно пропуская три оставшихся tail-теста и два положительных tag-контроля. Прогон может содержать тесты и не проверять эти причины падений.

**Сохранить существующее семейство и добавить два класса**; узкая замена только на три класса сокращает ранее назначенный охват:

```text
FullyQualifiedName~ExperimentFixes|FullyQualifiedName~LlmFromScratch.Tests.TranslationPairTailSignatureLearningTests|FullyQualifiedName~LlmFromScratch.Tests.TranslationVerifiedRuleTagTests
```

Машиночитаемый [required-test-cases.json](../../../../experiments-v2/_audits/post131-2026-11-03/required-test-cases.json) опубликован рядом с evidence аудита149; таблица ниже содержит тот же минимальный список. Это четыре именованных исторически оставшихся падения и два положительных контроля, **не утверждение об их исходе на нынешней сборке**.

| ID | Полный `FullyQualifiedName` | Роль |
|---|---|---|
| F01 | `LlmFromScratch.Tests.TranslationPairTailSignatureLearningTests.ForeignProvenanceInTheKey_DoesNotLiftTheOrdinaryEnRuSourceFloor` | Fixture должна довести кандидата до source floor |
| F02 | `LlmFromScratch.Tests.TranslationPairTailSignatureLearningTests.VerifiedRuleTags_SurviveWhileTheProvenanceSegmentIsGone` | Устаревший expected rule tag |
| F03 | `LlmFromScratch.Tests.TranslationPairTailSignatureLearningTests.ImportedCheckpoint_StoresTheLessonUnderACanonicalKeyWithoutProvenance` | Устаревший expected rule tag после import |
| F04 | `LlmFromScratch.Tests.ExperimentFixes.CapabilityMaterialLineageTests.ExamContentAppearingInTrainUnderANewName_IsRejectedBeforeLearning` | Известный дефект разрешения truthful source-row identity |
| C01 | `LlmFromScratch.Tests.TranslationVerifiedRuleTagTests.VerifiedPastTag_IsStoredWithTheLesson_NotThePresentTagOfTheRejectedVariant` | Положительный PAST и отрицательный rejected-side PRESENT |
| C02 | `LlmFromScratch.Tests.TranslationVerifiedRuleTagTests.VerifiedPresentTag_IsStoredWithTheLesson_NotThePastTagOfTheRejectedVariant` | Положительный PRESENT и отрицательный rejected-side PAST |

### Сборка, discovery и результат связываются по именам

Пример команд PowerShell для **назначенного будущего QA** из корня core. Новый каталог должен отсутствовать до начала. Команды не выполнялись при составлении этого документа.

```powershell
$qaOutput = Join-Path (Get-Location) ('qa-results/post131-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $qaOutput | Out-Null
$testFilter = 'FullyQualifiedName~ExperimentFixes|FullyQualifiedName~LlmFromScratch.Tests.TranslationPairTailSignatureLearningTests|FullyQualifiedName~LlmFromScratch.Tests.TranslationVerifiedRuleTagTests'
dotnet build Tests/LlmFromScratch.Tests.csproj --configuration Release 2>&1 | Tee-Object (Join-Path $qaOutput 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'BUILD_FAILED' }
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build --configuration Release --list-tests --filter $testFilter 2>&1 | Tee-Object (Join-Path $qaOutput 'discovery.log')
if ($LASTEXITCODE -ne 0) { throw 'DISCOVERY_FAILED' }
```

После discovery сопоставить **все шесть FQN** с реально обнаруженными именами. При пропуске, неоднозначности или ошибке discovery остановить приёмку и исправить выборку; количество больше нуля не является заменой этой проверки. После совпадения выполнить тот же фильтр на той же сборке:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build --configuration Release --filter $testFilter --logger 'trx;LogFileName=post131-targeted.trx' --results-directory $qaOutput 2>&1 | Tee-Object (Join-Path $qaOutput 'test.log')
$targetedTestExit = $LASTEXITCODE
```

Сохранить exit, terminal и доступный сырой TRX независимо от успеха. В квитанции для каждого обязательного FQN записать обнаруженное имя, `TestDefinitions/UnitTest` и `TestMethod`, соответствующий `testId`, `executionId` и итоговый `UnitTestResult`. Использовать namespace-aware чтение XML. Для этих шести `[Fact]` требуется однозначное соответствие; для соседних параметризованных тестов перечислять реальные строки/IDs, не считать один метод одним исполненным случаем. `Missing`, `Skipped/NotExecuted`, `Aborted` и неоднозначные дубли не равны `Passed`.

Квитанция содержит source commit и dirty manifest, конфигурацию, команду, SDK/runtime, хэши test/product DLL, время, exit и SHA256/местоположение TRX. После изменения source нужна новая соответствующая сборка. Не использовать старую DLL с новым expected. Отдельно сохранять все прочие failures выбранного семейства; не скрывать их за шестью обязательными строками.

**WRONG:** «найдено N > 0, ошибок 0, значит все четыре причины закрыты». **CORRECT:** шесть ожидаемых имён связаны с discovery и TRX; для каждой причины есть положительный путь, различающий отрицательный контроль и объяснённый исход.

## 4. Четыре причины требуют разных правок

### F02/F03: исправить два expected по независимому договору

В pinned tail-файле строки 158 и 593 требуют `RULE:TENSE_PAST`. Основание ожидаемого значения — [план105, §4.4](../fixes-based-on-experiments/105-late-learning-transfer-plan-2026-10-30.md), где указан `RULE:TENSE=PAST`. `TranslationVerifiedRuleTags` нормализует тело в `TENSE=PAST`; структурный builder сохраняет `=`. Положительные C01/C02 независимо требуют канонические PAST/PRESENT и отвергают метку отвергнутого варианта.

**WRONG:** изменить expected на увиденный actual без основания, ослабить проверку до `Contains("RULE:")` или вернуть продукт к underscore ради зелёного теста. **CORRECT:** в двух обучающих/import-проверках привести expected и описание к точному `RULE:TENSE=PAST`, сослаться на §4.4; сохранить запрет противоположного/rejected-side тега, отсутствие `PAIR` и проверку restore. Чистые string-тесты `TranslationSemanticKey.Canonicalize`, сохраняющие произвольные tag-сегменты побайтно, не менять автоматически: их договор — удаление provenance, а не нормализация rule tags.

### F01: сначала доказать достижение кандидата

Pinned fixture кладёт урок под `SVO|UNKNOWN`, а спрашивает `SVO|UNKNOWN|PAIR:P-FOREIGN`. Запасные ключи — `SVO|PREFIX-NOT-STORED` и `SVO|STRUCTURE-NOT-STORED`. Store делает exact lookup по каждому ключу; кандидат не найден ещё до порога. Комментарий «кандидат достижим» не доказывает этого пути.

**WRONG:** убрать положительное утверждение, принять fallback как свидетельство работы 0,60 или изменить product threshold. **CORRECT:** обычный canonical-положительный случай хранит и спрашивает совпадающий ключ и возвращает `learnedAnswer`. Если отдельно проверяется прямой legacy seam с `PAIR`, положить и спросить **тот же raw PAIR key**, явно назвав этот seam. Не выдавать прямое конструирование legacy tokens за штатный parser.

Для урока `alpha beta omega` запрос `alpha beta` даёт source overlap 2/2 = 1,0; запрос `alpha beta gamma delta epsilon` — 2/5 = 0,40. Второй проходит cascade floor 0,35 и должен быть отклонён ordinary EN_RU floor 0,60. В обоих случаях зафиксировать найденного кандидата и достигнутую стадию; неправильная ветвь floor = 0 должна отличаться на втором случае. Ранний miss не засчитывается за отказ source floor.

### F04: R128-03 остаётся ремонтом продукта

В `CapabilityMaterialLineageGuard` из совпавшей семьи выбирается `rows[0]`, затем заявленный `SourceRowId` сравнивается только с ним. Truthful `s5train` и `s5test` с одинаковым содержимым одной семьи поэтому получают ранний `CAPABILITY_MATERIAL_LINEAGE_CONTRADICTS_CONTENT` вместо назначенного `CAPABILITY_MATERIAL_FORBIDDEN_CONTENT_OVERLAP`. Это уже известный R128-03, подтверждённый текущим source, а не вновь измеренная регрессия.

**WRONG:** заменить expected на любой отказ или удалить законный дубль. **CORRECT:** сначала независимо сопоставить digest/содержание, потребовать единственную семью, затем выбрать заявленный ID **среди реально совпавших строк** и взять роль из доверенной выбранной строки. Проверить законные одинаковые Train-строки с разными IDs, перестановку trusted списка, неизвестный ID, чужое содержимое и неоднозначность разных семей. Настоящее запрещённое Train/Test пересечение должно дойти до своего запрета до learning. Раздельные Train/Dev и Train/Test flags имеют отдельную матрицу из плана146; этот один тест её не заменяет.

Исторические `16338 = 15615 + 628 + 95` и целевые `36/36` относятся к разным опубликованным запускам. Последнее подтверждено опубликованной сверкой сохранённых TRX; сами исходные TRX не предъявлены этому аудиту в pinned tree. Нельзя вычитать три исправленных случая из 628 и объявлять нынешние 625 failures. Сохранить сделанную BOM-починку, но получить новые адресные результаты на назначенной сборке.

## 5. P131: raw summary должен быть связан с сохранённым native output

Pinned `Program.cs:333–346` сначала сохраняет `evidence/native-flow/{arm}-{id}.json`, затем производит `RawRow`. `SavedEvidence.cs:131–163` проверяет raw-строки и вычисляет score из них, но не читает эти native-flow файлы. Это **подтверждённый исходниками пробел verifier**. В доступном опубликованном пакете все 128 raw/native строк совпали при независимой повторной обработке; вывод «4/4 на предъявленных отрицательных, без переноса на H/X» от этого не опровергнут. Нативное исполнение усиленного verifier пока **UNRUN**.

**Перед изменением gate — совместимость сохранённого пакета.** Старый `Provenance.Verify` требует HEAD/loaded Program DLL producer; исторический HEAD — `ca2deeedea88fe909935694ff50dec49ab3de04b`, proof включает619 абсолютных input/assembly путей. Новый verifier имеет другие source/DLL. Кроме того, old configuration содержит10 required gates, и старый DATA_BINDING требует SequenceEqual текущему списку. Поэтому простая пересборка с11-м gate отвергнет даже честный baseline. Не ослаблять проверку и не re-seal историю.

Использовать версионированный saved reanalysis по [150, §5.1](../fixes-based-on-experiments/codex-150-post131-addressed-repair-plan-2026-11-03.md#51-сначала-разделить-происхождение-producer-и-нового-verifier): неизменный historical producer proof/source/DLL/PDB/schema и отдельные current verifier source/DLL/schema/gates. Старые абсолютные пути отображаются однозначно на фактический архив и проверяются по исходным hashes; неизвестная схема, пропавшие bytes или неоднозначная mapping не принимаются. Для недоступных producer artifacts сохранить конкретный NOT_PROVEN полной provenance; доступные output binding/score не становятся общим ACCEPTED. Historical10 gates валидируются по old контракту, новый обязательный список задаётся verifier и управляет Terminal/exit. Существующий `VerifyOnly` уже отделяет InputRoot/VerificationOutputRoot и не требует нового HEAD до входа в режим; не менять PRIMARY HEAD guard и не создавать второй runner без причины.

Положительный upgrade-control требует настоящий полный архив старого producer и новую checker сборку; отрицательные — изменённый producer hash, неоднозначная mapping/неизвестная схема и отсутствие нового required gate. Новый hash checker не подставляется вместо old host hash. Исходный configuration/proof/receipts сохраняются.

В следующем назначенном ремонте добавить обязательный `procedure_integrity` gate `NATIVE_OUTPUT_BINDING`, включённый в обязательный список, квитанцию и внешний exit. Для сохранённого `attempt-0001/continuation-0001` ожидаются 128 уникальных `(arm,id)`: cold 8 и шесть полных плеч по 20. Набор выводится из паспорта и данных, а не принимается из произвольного числа raw-строк. Требовать взаимно однозначного совпадения ключей raw-writer/raw-reader и native files; отсутствие, дубль или лишняя строка не замалчиваются.

На каждом ключе сравнить как минимум:

- native `isSuccess`, отсутствие error, полную обязательную value/flowResult форму и `flowResult.summary.summaryId = translation-routing-flow-P131-{arm}-{id}`; `Actual` с `value.finalHumanResult`, verdict с `value.overallVerdict`, selected flow с `value.selectedFlowId`;
- весь словарь `NativeMetrics` с `value.flowResult.metrics` и предупреждения с `value.flowResult.warnings`;
- adapter, confidence, refusal и state version с теми же native полями и теми же правилами преобразования, которыми producer создавал `RawRow`;
- успех и обязательную полноту native результата, принадлежность строки панели и уже назначенную привязку к состоянию. Производные per-row файлы при включении в пакет также не могут противоречить raw/native.

Expected остаётся заранее заданным gold; фактический ответ для scoring извлекается из связанного native payload. Такое сравнение проверяет внутреннюю доказательную связь файлов, а не делает весь пакет неподделываемым: исходный run/source, хэши и provenance по-прежнему обязательны.

**Различающий контроль:** на копии полного честного пакета заменить `Actual` и `NativeMetrics.translated_length` в 16 строках B.after H/X и B.restored H/X на gold и его длину; native files, state, ledger и Train не трогать. Существующие source-derived raw/restore predicates допускают такую согласованную подмену summaries, создающую мнимые H/X 0/4 → 4/4. Новый binding должен отказать на этих 16 ключах. Это результат анализа/повторной обработки сохранённых данных; прохождение мутанта через настоящий C# verifier и внешний entrypoint ещё требуется.

**WRONG:** проверять только длину, выставленный `correct`, итоговую таблицу или равенство двух одинаково исправленных raw copies. **CORRECT:** предъявить полный положительный пакет и его адресно повреждённую копию одному настоящему verifier; сохранить достигнутый gate/reason и внешний ненулевой exit повреждённого пакета. Отдельно предъявить честный raw/native-coherent неправильный ответ: процедура остаётся VALID, научная ошибка учитывается в точности. Не заменять Actual на gold ради прохождения проверки.

### 5.1. Отдельная сборка и тесты проверяющего P131

**`P131-CHECKER-BUILD` — это часть назначенного ремонта, без PRIMARY.** `VerifyOnly` использует `bin/Release/net9.0/NegationExperiment.dll` и сам его не строит. Общий `Tests/LlmFromScratch.Tests.csproj` из §3 не содержит отдельный `P13101/Checks/Checks.csproj`. Его 14 существующих проверок и `Checks/Probe/.../FinalizerProbe.dll` нельзя заменить шестью продуктовыми FQN. Старый PRIMARY требует ровно14: этот исторический лимит не переносить в новый QA и не запускать PRIMARY/ResumePreparation ради сборки.

**До Rebuild:** подтвердить сохранность original producer source/DLL/PDB в отдельном проверенном архиве, включая recorded dirty bytes. Текущая `bin` может содержать единственный экземпляр старой DLL: нельзя сначала затереть его сборкой, затем выяснять provenance. Если нужные original bytes отсутствуют, записать точный NOT_PROVEN; не выдавать новую сборку за producer. Ниже команды только для будущего назначенного ремонта, после реализации совместимости и binding; в этой документной сессии они не выполнялись.

Из `C:/Projects/llm_mvp_core`:

```powershell
$hostP131 = Join-Path (Get-Location) 'experiments-v2/RP01-MICRO/P13101'
$qaP131 = Join-Path (Get-Location) ('qa-results/post131-checker-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $qaP131 -ErrorAction Stop | Out-Null
$checksInputsP131 = Join-Path $qaP131 'checks-build-inputs'
$hostInputsP131 = Join-Path $qaP131 'host-build-inputs'
New-Item -ItemType Directory -Path $checksInputsP131,$hostInputsP131 -ErrorAction Stop | Out-Null
$buildEvidenceP131 = Join-Path $hostP131 'BuildEvidence.targets'
dotnet build "$hostP131/Checks/Checks.csproj" -c Release -t:Rebuild --nologo -v:minimal --disable-build-servers -m:1 -nr:false -p:UseSharedCompilation=false "-p:CustomAfterMicrosoftCommonTargets=$buildEvidenceP131" "-p:P131EvidenceRoot=$checksInputsP131" 2>&1 | Tee-Object "$qaP131/checks-build.log"
if ($LASTEXITCODE -ne 0) { throw 'P131_CHECKS_BUILD_FAILED' }
dotnet build "$hostP131/Experiment.csproj" -c Release -t:Rebuild --nologo -v:minimal --disable-build-servers -m:1 -nr:false -p:UseSharedCompilation=false "-p:CustomAfterMicrosoftCommonTargets=$buildEvidenceP131" "-p:P131EvidenceRoot=$hostInputsP131" 2>&1 | Tee-Object "$qaP131/host-build.log"
if ($LASTEXITCODE -ne 0) { throw 'P131_HOST_BUILD_FAILED' }
dotnet test "$hostP131/Checks/Checks.csproj" -c Release --no-build --no-restore --list-tests 2>&1 | Tee-Object "$qaP131/checks-discovery.log"
if ($LASTEXITCODE -ne 0) { throw 'P131_CHECKS_DISCOVERY_FAILED' }
```

Перед следующим блоком сопоставить discovery с каждым именем ниже и всеми добавленными в этом ремонте случаями. Префикс каждого имени — **`P131Checks.MeasurementChecks.`**. Это список существующих `[Fact]` по исходнику, не выполненный сейчас discovery:

```text
GoldIndependentOfActual
RequiredIdsExact
RefusalIsNotCorrectTranslation
NegationPolarityWholeToken
ConfidenceMustBeFinite
StateAndAssemblyIdentityRequired
MissingReaderReceiptFailsTerminal
SuccessfulBuildDoesNotMeanSuccessfulRun
FullCheckpointHonestFixtureAccepted
TopicOnlyMutationRejected
KnnPayloadWithRehashedIdentityRejected
ProvenanceMultiplicityMutationRejected
MissingExpectedCheckpointRejected
ControlFailureReachesTerminal
```

Не ставить фильтр, исключающий новые классы: здесь исполняется весь небольшой `Checks.csproj`. 14 — сохранённый минимум, не требуемое точное число после ремонта. Missing/Skipped/Aborted/duplicate не PASS. При mismatch исправить discovery/включение исходников до приёмки. Новые mandatory binding/migration проверки требуют собственных различающих случаев; наличие старых14 само по себе их не покрывает.

```powershell
$previousCheckRootP131 = $env:P131_CHECK_OUTPUT_ROOT
$previousDeadlineP131 = $env:P131_DEADLINE_UTC
try {
    $env:P131_CHECK_OUTPUT_ROOT = Join-Path $qaP131 'checks-artifacts'
    $env:P131_DEADLINE_UTC = [DateTimeOffset]::UtcNow.AddMinutes(2).ToString('O')
    dotnet test "$hostP131/Checks/Checks.csproj" -c Release --no-build --no-restore --logger 'trx;LogFileName=p131-checks.trx' --results-directory $qaP131 2>&1 | Tee-Object "$qaP131/checks-test.log"
    $checksExitP131 = $LASTEXITCODE
} finally {
    $env:P131_CHECK_OUTPUT_ROOT = $previousCheckRootP131
    $env:P131_DEADLINE_UTC = $previousDeadlineP131
}
```

Сохранить реальный `$checksExitP131`, полный log и TRX даже при отказе. Две минуты — существующий предел искусственных дочерних проверок finalizer, не новый бюджет обучения. Сверить required FQN с TestDefinitions/UnitTestResult по §3; записать source/dirty/compiler inputs и hashes Checks/Probe/NegationExperiment DLL/PDB. Build или discovery failure не превращаются в тестовый red и не разрешают запуск старой DLL.

### Точные абсолютные пути перед saved-only вызовом

Запустить подготовку из фактического корня checkout. Нативный child меняет WorkingDirectory на hostRoot, поэтому оба аргумента должны быть абсолютными до Invoke-Bounded. Названный output ниже относится только к первой честной проверке и обязан отсутствовать; существующий результат не перезаписывать.

```powershell
$repoRootP131 = (Get-Location).ProviderPath
$runnerP131 = (Resolve-Path -LiteralPath (Join-Path $repoRootP131 'experiments-v2/RP01-MICRO/P13101/Run-Experiment.ps1')).ProviderPath
$inputRootP131 = (Resolve-Path -LiteralPath (Join-Path $repoRootP131 'experiments-v2/RP01-MICRO/P13101/results/attempt-0001/continuation-0001/evidence')).ProviderPath
$verificationParentP131 = [IO.Path]::GetFullPath((Join-Path $repoRootP131 'experiments-v2/_verifications/P131'))
[void][IO.Directory]::CreateDirectory($verificationParentP131)
$verificationOutputRootP131 = Join-Path $verificationParentP131 'postq003-honest-before'
if ((Test-Path -LiteralPath $verificationOutputRootP131) -or (Test-Path -LiteralPath ($verificationOutputRootP131 + '.process'))) { throw 'Verification output or process receipt already exists; preserve it.' }
```

После реализации и проверки этих слоёв использовать существующий `pwsh -NoProfile -File $runnerP131 -Mode VerifyOnly -InputRoot $inputRootP131 -VerificationOutputRoot $verificationOutputRootP131` для honest/mutant/honest из §5, с отдельными output и terminal. Это форма существующего интерфейса после подготовки абсолютных путей выше. InputRoot — именно evidence, не родительский attempt. producer_archive_root назначается отдельно по подлинным original bytes; output не лежит внутри input/archive. Для mutant и второго honest нужны свои абсолютные входы/новые outputs. Parent continuation без /evidence — обязательный layout-negative с nonzero exit без модели. Каждый фактический вход и адресная порча назначены §5; дополнительные аргументы архивного отображения определяются реализованным `VerifySavedProducer`, которого пока нет. Не публиковать команду к несуществующему API как уже исполнимую. Old14 проверяют измеритель/Terminal; полноценный outer процесс дополнительно доказывает новый binding, переход схем и выход. Это разные уровни приёмки.

## 6. P127: согласованность ответа и точность изменённого задания — разные проверки

Pinned `LabelSensitivityHolds` (`Program.cs:1111–1125`) проверяет лишь `B.AnswerRoute != C.AnswerRoute` на p1/p2. `BuildFinalizer` проверяет наличие двух известных шагов и общий размер graph trace, но не связывает каждую метку с её ordered payload. `TrainArm` (`570–578`) задаёт изменённые targets явно. Поэтому слово «ответ изменился» недостаточно для проверки назначенного изменения урока. Это source-derived gap; нативная проверка будущего исправления **UNRUN**.

Проверка **целостности** связывает поля одного ответа: `AnswerRoute` ↔ `AnswerSteps` ↔ рёбра и журнал реально исполненного графа. В этом опыте словарь точен: `review-first` → `[review, implement]`, `build-first` → `[implement, review]`; карточное `implement-first` соответствует DTO `build-first`. Не превращать неизвестную метку в `build-first` по ветке default; сохранить неизвестность/ошибку явно. Для сохранённых A/B графов связать запись по `(Arm, CaseId)` с тем самым ответом, проверить порядок, рёбра, журнал и согласованность записанных производных полей. Графа C в паспорте нет: не приписывать ему A/B trace и не вызывать новый graph ради saved-data проверки.

Проверка **научного причинного контроля** отдельно сравнивает результат C с назначенным target:

| Изменённая строка | Назначенный C target | Требуемый ordered payload для точного совпадения |
|---|---|---|
| p1 | `build-first` | `[implement, review]` |
| p2 | `review-first` | `[review, implement]` |

Этот эталон берётся из назначения C до сравнения с actual. Он не равен исходному `GoldSteps` B; нельзя незаметно переписать исторический `StepsCorrect`, который измерял точность относительно исходного задания. Новое поле, например `assigned_target_match`, отчётливо отделяется от исходной точности. Совпадение C с B или иной **внутренне согласованный** неправильный ответ означает отрицательный/NOT_PROVEN причинный контроль при VALID процедуре, а не INVALID evidence и не обязательный ненулевой exit.

**Конкретная переклассификация старого gate:** в `BuildFinalizer` убрать научный `mandatory["label_sensitive_c_holds"] = LabelSensitivityHolds(...)` из процедурного all-pass и записывать его/точный `assigned_target_match` как scientific observation. Нельзя просто заменить label inequality на exact C-target внутри того же mandatory: это снова сделает честный научный минус INVALID. Integrity содержит непротиворечивость самого DTO и связь с действительно сохранённым producer/trace. Исторический status с девятью checks не переписывать; новая квитанция называет свою схему и новый список процедурных проверок. Контроль с согласованным настоящим C=B обязан сохранить integrity VALID при scientific false; отдельная адресная порча evidence обязана дать ненулевой exit.

**Косвенные зависимости тоже убрать из procedural all-pass.** `CSubstitutedByBControl` вычисляет `control_passed = honestHolds && !substitutedHolds`, а `BuildFinalizer` включает его как `c_substituted_by_b_control_passed`. Поэтому удаления одного `label_sensitive_c_holds` недостаточно: настоящий C=B всё ещё отвергался бы косвенно. `CorruptedReceiptControl` аналогично требует `honestVerdict.OrderCorrect` для настоящего B/e1, то есть победу learner относительно gold, через `corrupted_receipt_control_passed`.

Переработать оба control path вместе с finalizer: положительный контроль integrity принимает любой полный непротиворечивый actual, в том числе научно неверный; отрицательный меняет route/steps либо записанный producer/trace при неизменных остальных предпосылках. Для ответа из двух разных узлов проверять точное множество и порядок по договору DTO; одинаковое имя дважды не является полным payload. Прежний C→B label-sensitivity observation хранить в scientific diagnostics, не использовать его истинность напрямую или через helper как обязательство успеха learner. Если нужен unit-control функции gold scoring, использовать отдельно помеченный синтетический positive/negative fixture самого scorer; его успех не является научным результатом B/C и не подмешивается в historical raw.

В файлах ремонта явно указать также `CSubstitutedByBControl` и `CorruptedReceiptControl`. Проверка после правки должна пройти **весь** новый finalizer, а не только новый предикат: согласованный настоящий C=B и согласованный научно неверный B/e1 сохраняют procedure VALID; разрыв route/steps/producer/trace даёт INVALID/ненулевой outer exit. Новых fit/Predict/graph-node calls для этих saved/validator controls нет; исторические результаты и девять checks не переписываются.

**Различающие контроли:** (1) сохранить `C.AnswerRoute`, но подменить `C.AnswerSteps` на порядок B — отдельные поля имеют допустимый вид, их сочетание невозможно по заданному DTO; integrity отказывает; (2) поменять только журнал сохранённого A/B графа при неизменном ответе — binding должен отказать; (3) предъявить полный явно синтетический согласованный пакет с C=B и неверным B/e1 — validator integrity VALID, scientific false. Не называть изменение исторического raw «честным неверным ответом». Проверить весь finalizer и внешний exit, сохранив stage/reason. Не требовать научной победы B/C для exit0.

**Полнота P127 (`P127-SAVED-KEYS`).** Для каждой выбранной исторической попытки отдельно требовать 68 уникальных `(Arm, CaseId)` в `metrics.reads`: `{A,B,A-perm,B-perm} × {u1…u8,p1,p2,e1…e4,h1,h2}` (64) и `C × {p1,p2,h1,h2}` (4). Не объединять две попытки в один набор. Графы: ровно `{A,B} × {e1…e4,h1,h2}` — 12 ключей. Cold observations и контрольные квитанции учитываются отдельно. Проверить точные множества, кратность и обязательные поля до `All()` и scoring: empty, missing, extra, duplicate и повреждённый payload `A-perm/B-perm` — разные отрицательные случаи. Счётчики 36+32 и 12 не доказывают присутствие строк. `BuildFinalizer` должен принимать также permutation records; прежний `armA.Concat(armB).Concat(armC)` их пропускал.

**Предел первоисточника P127.** `ReadOutcome` формируется из native ответа в `Program.cs:697–724`, затем сохраняется в `metrics.reads` (`459–482`). Отдельного P131-подобного `native-flow` для каждой строки нет. Проверка неизменности опубликованных bytes, DTO и имеющегося A/B graph trace устанавливает внутреннюю согласованность доступных свидетельств; она не создаёт вторую независимую запись ответа и не доказывает полный historical runtime. Отсутствующий native payload обозначить `NOT_RECORDED`; не вызывать модель и не изготовлять «оригинал» из summary. Результат и предел provenance сообщать раздельно.

**Обязательная внешняя authority после Q003 (`P127-HISTORICAL-OUTPUT-AUTHORITY`).** Конкретный набор — [15-file manifest](../../../../experiments-v2/_audits/postq003-2026-11-03/codex-p127-trusted-historical-output-manifest.json), source commit `7ce6b14ca7bb353ed5f57179f0561053496d36ae`. Закреплено: `manifest_commit=f3b64f87159193c2897092d6e571b2b65e3b75c4`, SHA256 manifest `df98d7354296396164b51ce583370f4d10dbc46f2e3527e1b67a5f265b1fca84`; `results_commit=7ce6b14ca7bb353ed5f57179f0561053496d36ae`. Читать manifest из этого commit вне изменяемого input_root; его authority не брать из проверяемой копии. Проверить exact result files выбранной попытки по Git blob/SHA256/bytes; `build-identity` и inventory до/после не доказывают историческое происхождение outputs. Согласованная подмена route+steps+graph+summary+local hashes под прежним attempt должна дать nonzero по внешней authority. Полный synthetic fixture имеет отдельную identity/test mode и не становится историческим запуском. Истинность выводов старого report не следует из authentic bytes; поправки153 сохраняются. Реализация и native outer acceptance остаются UNRUN до исполнения155§5.

**Три разных контроля (`SYNTHETIC-IS-NOT-HISTORICAL`).** (1) Неизменный настоящий пакет проверяется с его фактическими ответами, в том числе неверными по gold. (2) Копия исторического пакета с изменёнными route+steps под прежней identity/proof — повреждённое свидетельство, даже если DTO внутренне согласован; такая копия не становится VALID историческим запуском. (3) Для сценария «настоящий C=B или неверный B/e1» при отсутствии такого сохранённого запуска автор ремонта создаёт явно синтетический **полный** fixture validator/finalizer с собственной synthetic identity, всеми назначенными записями и согласованными A/B traces; C trace не добавляет. Он проходит тот же finalizer и outer exit, но только в явно отделённом режиме тестирования validator, и никогда не попадает в научные метрики или accepted historical runs. Это проверка логики checker без Train/Predict/graph, не новый опыт обучения. Unit-тест одного scorer не заменяет этот контроль; при невозможности получить полный fixture оставить приёмку открытой, не подделывать producer proof.

## 7. Выход повторной проверки и обязательные доказательства

Назвать `input_root` старого пакета и новый, отсутствовавший `verification_output_root`. Исторические raw/native/metrics/status/validation/terminal читаются без записи. Сохранить inventory и SHA256 всех входных файлов до и после; выводить новые отчёты только в новый каталог, со ссылкой на исходный run и source. Не overwrite даже прежнюю квитанцию повторной проверки.

До появления безопасного read-only режима нельзя вызывать PRIMARY runner или команду, которая пишет validation в старый root. Псевдокоманду будущего verifier не выдавать за существующую. Реальная приёмка требует: положительный пакет → настоящий verifier process → внешний потребитель exit; одна адресная порча → тот же путь → нужный отказ. Свод содержит `Train/Predict/Flow/Load = 0`, действительные команды и native execution status. Python/source-derived модель полезна для локализации пробела, но не становится доказательством выполненного C# control flow.

Перед новым назначенным ремонтом применить [основной стек120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md) и навыки из executor48. В `skill_load_status` честно указать canonical agent ID, прочитанные источники/версии и применение: code examination — путь до потребителя; QA/integration — достижение границы; test triage/bug-to-tests — причина и различающий контроль; prompt patch — собственные WRONG/CORRECT; human communication — эффект и предел доказательства на русском. Полный стек не заменяется этой краткой таблицей и не считается прочитанным по хэшу.

## 8. Короткий prompt для назначенного исполнителя

> Возьми один назначенный остаток из живой очереди: сейчас 131/131 результатов записаны, следующий перенос Q01; Q132 и повтор PRIMARY не назначены. Прочитай основной стек120 и executor49, запиши skill_load_status. Сохрани b4/R17 и исторические результаты. Для QA используй OR-фильтр из §3, сохраняя ExperimentFixes; свяжи четыре FQN failures и два положительных контроля из manifest с discovery и сырым TRX той же свежей сборки. Непустой список и Failed=0 без этих имён не приёмка. Раздели F02/F03 wrong expected по плану105, F01 недостижимую fixture и F04 известный lineage defect; не подгоняй expected под actual и не ослабляй отказ. Для saved P131 сначала сохрани original archive, реализуй разделение historical producer identity/schema/10 gates и current verifier identity/gates и новый binding; после последней source-правки выполни§5.1 (Checks/Probe+host,14requiredFQN+новые случаи, discovery/TRX), затем VerifyOnly. Любая дальнейшая source-правка требует соответствующей новой сборки/проверки; сохрани archive bytes/proof и existing VerifyOnly, отсутствие originals оставь NOT_PROVEN. Затем сверь raw с native payload, для P127 потребуй68 reads и12graph keys на попытку, включая permutation arms; свяжи route/steps/имеющийся trace и отдельно посчитай exact assigned C targets. Второго native payload P127 нет. Historical mutant не считать честным отрицательным ответом; полный synthetic fixture всего finalizer имеет отдельную identity и не становится научным результатом. Честный неверный ответ — научные данные, противоречивые записи — integrity failure. Не требуй победы B/C для exit0. Полный baseline и одна адресная порча идут через тот же настоящий verifier и внешний exit. Saved reanalysis: Train/Predict/Flow/Load=0, отдельный новый output, старый input hash до/после одинаков. В отчёте раздельно procedure_integrity, scientific_result, transfer_acceptance, реально выполненные native проверки и UNRUN. Изменения ограничь назначенным ремонтом; родитель проверяет и публикует результат.

