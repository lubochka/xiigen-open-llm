# Свежая адресная QA 12 ноября 2026

Исполнитель `/root/fresh_native_qa` получил свежую Release-сборку, discovery и один завершённый прогон: **73/73 Passed**, пропусков и прерываний нет. Это результат отдельной DLL и сохранённого post-test source snapshot. Более поздние изменения двух исходников этой сборкой не проверены. Весь F03/R01 и полный набор тестов не приняты.

Наши изменения исходников тестов ограничены двумя недостающими импортами namespace: `TranslationPhraseAdmissionAlignmentTests.cs:50` и `TranslationPhraseCoverageLearningTests.cs:45`. Первая сборка показала CS0246 в этих файлах; источник типа — public `ExperimentAttemptScope` в соседнем namespace. Каждая наша правка добавила ровно одну using-строку; точные pre-edit bytes и проверка минимальности сохранены. Вся остальная C#-дельта принадлежит параллельной незавершённой линии. Assertions/gold/научные факторы мы не меняли.

## Запуски и границы

| Шаг | Реальный исход | Доказательство |
|---|---|---|
| Исходная сборка общих bin/obj | exit 1; CS0246 в двух файлах; тесты не запускались | `codex-build-process.json`, `logs/build.stdout.log` |
| Изолированная свежая сборка | exit 0; 251 warnings, 0 errors; 7 мин 46.38 с | `codex-build-stable-process.json`, `logs/build-stable.stdout.log` |
| Discovery | exit 0; 73 строки / 68 методов; missing/unknown/duplicates = 0 | `codex-discovery-reconciliation.json` |
| Один выбранный прогон | exit 0; 73/73 Passed; skipped/aborted/notExecuted = 0 | `results/selected/selected.trx`, `codex-test-process.json` |
| Строгая проверка PDB против последних файлов | exit 1: последние два source checksum отличаются | `codex-pdb-reader-terminal.json`, `codex-pdb-source-proof.json` |

Ниже — история фактически выполненного запуска, не повторное поручение. Эта попытка и сохранённые скрипты с жёстко закреплённым root заморожены как доказательства: не запускать их повторно поверх этой попытки. Новая универсальная оснастка не создавалась.

Основные команды из `C:/Projects/llm_mvp_core`:
```text
dotnet --info
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release --artifacts-path <this-root>/native-build -p:CustomBeforeDirectoryBuildProps=<this-root>/codex-artifacts-layout.props
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --list-tests --filter <codex-filter.txt> --artifacts-path <this-root>/native-build -p:CustomBeforeDirectoryBuildProps=<this-root>/codex-artifacts-layout.props
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter <codex-filter.txt> --results-directory <this-root>/results/selected --logger 'trx;LogFileName=selected.trx' --artifacts-path <this-root>/native-build -p:CustomBeforeDirectoryBuildProps=<this-root>/codex-artifacts-layout.props
```

Точные массивы аргументов, cwd, PID, UTC начала/конца, exit и хеши раздельных stdout/stderr находятся в `codex-*-process.json`; `<this-root>` — каталог этого отчёта. SDK 10.0.302, runtime net9.0 / 9.0.18. stdout/stderr основных четырёх шагов сохранены целиком. Отдельный PDB reader имел merged tool output и структурированный JSON; отдельный stderr этого диагностического reader не записывался.

Локальный props меняет только компоновку QA-выходов: 53 проекта получили 53 уникальных имени по каталогам. Это нужно для двух `SemanticCompiler.csproj` с одинаковым именем файла. Для Tests, 12.SemanticCompiler и 29.SemanticCompiler реальные evaluated BaseOutputPath/BaseIntermediateOutputPath проверены и различаются. Product csproj и глобальные props не изменялись. Native bin/obj не являются частью source deliverable.

## Что именно выполнено

| Класс | Случаев |
|---|---|
| `LlmFromScratch.Tests.ExperimentFixes.ExperimentAttemptOwnershipTests` | 15 |
| `LlmFromScratch.Tests.ExperimentFixes.ExperimentEvidenceSinkTests` | 4 |
| `LlmFromScratch.Tests.ExperimentFixes.ProbeNeuralNetworkGradientTests` | 8 |
| `LlmFromScratch.Tests.TranslationBatchJsonLearningTests` | 3 |
| `LlmFromScratch.Tests.TranslationBatchTypeMismatchRollbackControlTests` | 3 |
| `LlmFromScratch.Tests.TranslationBatchTypeMismatchRollbackTests` | 3 |
| `LlmFromScratch.Tests.TranslationExactTopicMemoryTests` | 8 |
| `LlmFromScratch.Tests.TranslationLateCorrectionAnnotationConsistencyTests` | 13 |
| `LlmFromScratch.Tests.TranslationRequiredWholeTermLearningTests` | 3 |
| `LlmFromScratch.Tests.TranslationSourcePolarityLearningTests` | 3 |
| `LlmFromScratch.Tests.TranslationWordRoleConsistencyTests` | 10 |

Это 4 helper + 15 ownership + прежние 51 случая восьми классов + 3 отдельного контрольного класса. Пять новых ownership Fact добавлены чужой линией до сборки; поэтому прежняя цель 68 стала 73. Source-derived manifest, InlineData и discovery сверены с TestDefinitions, TestEntries, testId/executionId и UnitTestResult; 73 уникальных executionId, 68 методов. ResultSummary не считался отдельным тестом. CLI/Counters/timestamps и фактический codeBase той же DLL совпали.

TestRun: `83e5cf7d-3f2b-4f3c-9be5-0458275c762d`. TRX UTC start `2026-11-12T11:41:27.5825483Z`, finish `2026-11-12T11:44:35.8855440Z`. Полный TRX исходного runner сохранён, включая его BOM/CRLF; raw логи также не нормализовались.

## Привязка source → PDB → DLL

HEAD до и после: `6b52307dc26f982dde161ee1d6f470eec1c84de4`. Исходное дерево было чистым; затем параллельная линия меняла код и запускала свои проверки. Общие bin/obj не пересобирались во время наблюдаемых чужих testhost/build; процессы не завершались нами. Их результаты не засчитаны как наши.

Из 2928 известных source/config inputs 2926 совпали между pre-build и post-test; два отличия сохранены, а не перезаписаны. Инвентарь охватывает 53 ProjectReference проекта, известные compile dirs, immediate untracked C# и явные Compile/Import; новые самостоятельные source subdirs вне этих известных dirs рекурсивно не перечислялись. Условия MSBuild items целиком не вычислялись; generated bin/obj и runtime TestResults исключены.

| Файл | Pre-build SHA256 | Post-test / compiler SHA256 | Последний SHA256 во время PDB-проверки |
|---|---|---|---|
| `Tests/ExperimentFixes/ExperimentAttemptOwnershipTests.cs` | `7e899c0c2d75e3e12820e3f20adff018fc094a7c747ae99788cb6792afac75ed` | `3254a8c5ffa907dd9cec96490d2f47b2002218df1b81d82d2669b9f369dfdfa3` | `02276f51b1a1c2b295d55a6c45837de38282ef95d33897547e48dab313acbee9` |
| `Tests/ExperimentFixes/ExperimentAttemptScope.cs` | `92fca38d743354ae93513bd56f526dc4e62ecaaa7be216d7a0371eadc7817a77` | `6eb87048ac17ef7d19ae20329bba9a460022cda1a7c44ada5043ade5c576a513` | `7e3d56747b9a55f667d61700dd32551f18de4cd6eae29d7fa20a9ee9105c3e45` |

Для обоих документов PDB SHA256 algorithm GUID `8829d00f-11b8-4213-878b-770e8597ac16`; compiler checksum совпал с сохранённым post-test snapshot. Он не совпал с pre-build и с ещё более поздними текущими bytes. Это доказывает, что компилятор взял поздние восстановленные bytes; строгий стабильный before/after source pin не заявляется.

CodeView GUID `88e174ab-9a1d-470f-8bc4-2d8fef2b1803`, age 1, stamp `2947232323` совпали с Portable PDB header id `ab74e1881d9a0f478bc42d8fef2b18034332abaf`. Это именно PDB нашей DLL.

Tests.dll: `d48e3f37e5177f73d7ff00f70a3e225197830c924b79e57978e211dab50f21d0`; Tests.pdb: `2540870f8018dfa57874b356d23fea2c751c62ad0b7d9d07eecdb7fba3ed4000`. Все 53 DLL и 53 PDB сохранили байты после тестов; local props SHA256 `71e0fe204977f9869c61116f5fb4e61719cca5fae8542b54ed984a3e45e15e1b` также не менялся.

Полные байты именно скомпилированного состояния двух изменённых файлов нашей попыткой отдельно не сохранялись. Сохранены их checksum, привязанный PDB, общие снимки хешей и рабочие diffs; точное соответствие этих diffs полным compiled bytes не подтверждалось. Untracked Scope не входит в обычный git diff. Поэтому пакет не называется воспроизводимым source checkout.

157 проверенных опубликованных файлов вне C# остались побайтово прежними. Десять изменённых C# в первоначальном 270-file published inventory отделены как изменения source, а не выдаются за перезапись raw. Их список находится в `codex-result-reconciliation.json`.

## Чтение, ошибки подготовки и документы

Работник полностью прочитал qa-session-type, test-integrity, test-failure-triage, systematic-debugging, verification-before-completion, debug-session, human-communication glossary; договоры 45/46 и CTX12; sink/helper/ownership tests, добавленные пять Facts и новый Scope. Caller fixtures/Lazy/doses восьми файлов прочитаны адресно. Это не полный личный reload работником двух мегабайт canonical AGENTS/CLAUDE/архитектурного стека: полный личный reload выполнил родитель, его работу себе не приписываю.

Два избыточных inventory subprocess с 25/55-second limits завершились timeout/status null; один direct-walk inventory отменён только в собственном session 40376. Это ошибки подготовки измерения, не продуктовые отказы. Их нельзя смешивать с настоящим первым build exit 1. Успешный ограниченный snapshot заменил чрезмерный обход. Промежуточный context-mode peek отменён как необязательное чтение; тестовый процесс не останавливался.

Узкая независимая review верхних 42 строк context/01-registry/README подтвердила, что общая F03 и R01 остаются открытыми, historical51+3 не превращены в полный suite, и существующий владелец R01 сохранён. Одно замечание передано родителю: context line7 «Восемь дополнительных callers уже назначены ...» следует явно пометить историей ce245 и связать с завершением6b; новый верх уже говорит, что выборка выполнена. Родитель сообщил, что исправил это замечание в context:7. Сам работник эти индексы не редактировал.

Полный suite, свежий прогон остальных 12 ordinary callers, two-process acceptance, полный owner stdout/stderr/TRX, запрет bypass writers после seal, ACL/mkdir failure и весь F03/R01 этим пакетом не доказаны. Новых PRIMARY, программ обучения, infer/train campaigns, git add/commit/push, worktree или nested AI не было. Существующие учебные fixtures выбранных тестов реально исполнились как часть разрешённого regression; из этого не выводится качество обучения на новых данных.

Следующая приёмка последних меняющихся R01-файлов остаётся их владельцу после прекращения правок на согласованное окно build/tests. Этот QA-срез завершён как адресный результат сохранённой DLL; latest source и весь проект не объявляются принятыми.

Поля честности: claim_scope=73 selected results on isolated DLL/post-test source; evidence_scope=commands/TRX/PDB/hashes; count_claimed=73 cases in one run; independent_evidence_count=73 UnitTestResult, not 73 review cycles; subagent_id_or_NONE=/root/fresh_native_qa; downgrade_needed=latest source not verified.
