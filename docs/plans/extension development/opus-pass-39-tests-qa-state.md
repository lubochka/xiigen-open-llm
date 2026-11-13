# Pass P39 — Tests/QA реальное состояние (Tests-проект, e2e, handler)

Дата: 2026-07-08. Ассессор: Opus 4.8 (read-only, bounded reads реального `C:\Projects\llm_mvp_core\Tests`).

## Что проверял (slice P39)

Не харнесс-плумбинг (это P35), не host-репо node-QA (P12/P16/P20), не dashboard route-tied QA (P29),
не «сессия гоняла ли dotnet test» (P07). Здесь — **содержание и целостность самого .NET-набора
тестов `Tests/LlmFromScratch.Tests` для extension-путей**: что тесты РЕАЛЬНО ассертят
(корректность vs честность-негатив vs форма артефакта), есть ли e2e/host-слой, есть ли handler-слой,
и зафиксирован ли зелёный baseline-прогон.

Источники (bounded):
- инвентарь тест-файлов через `Get-ChildItem` (233 `*.cs` в `Tests/`); extension-релевантных десятки:
  `Extension*`, `SmartSkillsSemanticQuality*` (P10..P25), `PromptToTask*`, `OrchestrationReasoning*`,
  `SkillSelection*`, `Phase08CommonUserTwoLegLifecycleTests`, `XiigenExtensionApiContractsTests`.
- грепы: `SemanticQualityClaimed` (237 хитов / 53 файла); `Assert.False(...SemanticQualityClaimed)`
  (повсеместно) vs `Assert.True(...SemanticQualityClaimed)` (**0 совпадений**);
  `Trait|Category|e2e|EndToEnd` (extension — 0 реальных host-e2e; единственный `*E2e*` файл — иной домен);
  `ProcessStartInfo|Process.Start|new Process(` (6 файлов, extension = только локальный PS-scorer).
- целиком прочитаны: `ExtensionExecutionStatePatchWriter.cs` (59 стр),
  `ExtensionRuntimeTokenQualityArtifactPaths.cs` (50 стр); слайсы:
  `ExtensionToken04QualityScoringArtifactsTests.cs:1-70`,
  `SmartSkillsSemanticQualityP25RuntimeHoldoutComparisonTests.cs:1-60`,
  `XiigenExtensionApiContractsTests.cs:1-40 (заголовки 26 методов) + 323-392`.
- `Tests/TestResults`: 5 `.trx` (max `total=5`; `pass144-exact.trx total=1`, DesignConversion; answer-composer
  2..5, май) + ~70 GUID-deployment-папок 07-03/07-04 без полного `.trx`; свежайшая GUID-папка (07-08) пуста.

## Честная картина

Extension-QA в ядре **существует и обширна** (десятки файлов, 236 ассертов в одном
`XiigenExtensionApiContractsTests.cs`). Но по содержанию это **набор про ЧЕСТНОСТЬ, а не про
СПОСОБНОСТЬ**: он доказывает, что расширение НЕ заявляет семантическое качество / экономию токенов /
обучаемость и НЕ утекает host-логику/секреты, — и НИ ОДИН тест не ассертит, что ответ на известный
вход ВЕРЕН. Даже полностью зелёный прогон доказывает «no overclaim», а не «работает».

Ключевые факты:
1. `Assert.True(...SemanticQualityClaimed)` = **0** во всём наборе; `Assert.False(...)` — десятки
   (SkillSelectionMachineTests один даёт 15+; ExtensionToken04, все машины, все P10..P25).
2. Тест с самым «положительным» именем —
   `XiigenExtensionApiService_SemanticClaimRequiresPositiveEvidenceFromEveryComponent`
   (`XiigenExtensionApiContractsTests.cs:323-386`) — подаёт синтетический положительный токен
   ТОЛЬКО в skill-ноге (`EvidenceFeatures:["SEMANTIC_QUALITY_CLAIMED_TRUE"]`, `Confidence:1.0`,
   `DecisionText:"Future positive skill evidence fixture."`), тогда как prompt-to-task и orchestration
   жёстко несут `SEMANTIC_QUALITY_CLAIMED_FALSE`/`BASELINE_NOT_SEMANTIC_QUALITY`, и ассерт всё равно
   `Assert.False(shouldClaim)`. Положительная (true) ветка гейта семантического качества **не имеет
   ни одного проходящего теста** — she тестируется только на возврат false.
3. Самый большой extension-QA файл `ExtensionToken04QualityScoringArtifactsTests.cs` (**222 КБ**) — это
   **проверка формы + негатива по УЖЕ ЗАКОММИЧЕННЫМ JSONL**: читает
   `artifacts/extension-runtime-token-quality-learnability/token-04-*.jsonl`, ассертит счётчики (108
   row / 54 pair; Claude=36/Codex=36/Cursor=36; 3 метода×12 сценариев) и что ВСЕ флаги
   `quality_result_claimed/semantic_quality_claimed/learnability_claimed/token_savings_claimed`=false,
   `token_savings_claim_allowed_pairs=0`, `adaptive_effect_proven_pairs=0`. **Никакого скоринга во время
   теста нет** — доказывается нулевой результат из статического файла.
4. Коллекция token-quality жёстко зависит от закоммиченных артефактов: `FindRepoRoot`
   (`ExtensionRuntimeTokenQualityArtifactPaths.cs:41-43`) ТРЕБУЕТ существования
   `artifacts/extension-runtime-token-quality-learnability`, иначе бросает `DirectoryNotFoundException`
   — на чистом клоне без артефактов вся коллекция падает на инициализации fixture (среда, не продукт).
5. В тест-сборке лежит **не-тестовый governance-writer**: `ExtensionExecutionStatePatchWriter`
   пишет `execution-state-patches/*-state-patch.json` (schema `xiigen.extension_execution_state_patch.v1`,
   поля `giant_state_mutated:false`, `state_file_read_strategy:"metadata_only_no_json_parse"`), а
   `ExtensionRuntimeTokenQualityArtifactPaths.StatePath` указывает на
   `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`. Тесты выступают
   ещё и производителями/мутаторами governance-состояния в `docs/plans` и `artifacts/` — это
   QA-integrity/архитектурный запах, а не тест-логика.
6. **e2e/host-слой для трёх расширений в ядре отсутствует**: единственный `*E2e*`-файл —
   `DesignConversionLocalFullLoopE2eTests` (домен design-to-code, не extensions); слова `e2e` в
   extension-тестах встречаются только как строки-ссылки в `ExtensionQuality03*`. Единственный
   process-spawn в extension-тестах — локальный `powershell` на `tools/token-quality-full-host-scorer.ps1`
   (`RunFullHostScorer`, уже зафиксирован P35), а не запуск реального Claude/Cursor/Codex-хоста. Весь
   extension-набор — in-process boundary/contract/artifact-тесты; вершина пирамиды (e2e/host) пуста.
7. **Зелёного полного baseline-прогона на диске нет**: все `.trx` — узкие filtered-прогоны
   (`total` максимум 5; `pass144-exact.trx total=1` — DesignConversion, 07-03; answer-composer 2..5, май).
   ~70 GUID-deployment-папок 07-03/07-04 без полного `.trx`, свежая (07-08) пуста. CLAUDE.md-baseline
   «≥1488 passed, 0 failed» не подтверждён ни одним воспроизводящим артефактом; зелёный/Rule-4-gate
   статус самого extension-набора не доказан ни одним записанным прогоном; CI (`.github`) нет (P35).

## НОВЫЕ дыры (5) — только не дублирующие P07/P12/P16/P20/P29/P35

| Component | Real-status | Evidence file:line | Gap | Closure steps |
|---|---|---|---|---|
| Оракул extension-QA в ядре (`Tests/`) | HONESTY-ONLY / no correctness oracle; gate-true-path untested | грепы: `SemanticQualityClaimed` 237/53 файла, `Assert.False(...SemanticQualityClaimed)` десятки (`SkillSelectionMachineTests.cs:289/367/441/458-462/542/557-564/668-672`, `ExtensionToken04...:2095`, `OrchestrationPlanTests.cs:397/457`, `PromptToTaskP7EEvalAblationRerunTests.cs:39-40` и др.); `Assert.True(...SemanticQualityClaimed)`=**0**; `XiigenExtensionApiContractsTests.cs:323-386` (positive-evidence-тест кладёт синтетический 1/3 токен и всё равно `Assert.False`) | Весь .NET extension/машинный QA доказывает, что расширение НИКОГДА не заявляет семантическое качество (честность), и НИ ОДИН тест не доказывает, что ответ ВЕРЕН (способность). Положительная ветка гейта semantic-quality не имеет ни одного проходящего теста. Даже полностью зелёный прогон = «no overclaim», не «работает». Отдельно от P10 (гейт в коде сервиса), P12/P16/P20 (host-репо node-QA), P38 (аудит-артефакт) — здесь суммарный характер самого core-набора + 0 положительных ассертов. | Ввести golden correctness-oracle (известный вход → ожидаемый task/plan/selectedSkills) поверх РЕАЛЬНОГО обученного inference; добавить хотя бы один проходящий тест true-ветки гейта на реальной положительной evidence (не «Future … fixture»); не засчитывать negative-only набор как QA способности. |
| Флагманский `ExtensionToken04QualityScoringArtifactsTests.cs` (222 КБ) | ARTIFACT-SHAPE + NULL-RESULT ASSERTION / no live scoring; hard repo-artifact dependency | `ExtensionToken04QualityScoringArtifactsTests.cs:14-70` (читает committed `token-04-*.jsonl`, ассертит counts 108/54 и `quality/semantic/learnability/token_savings *_claimed=false`, `token_savings_claim_allowed_pairs=0`, `adaptive_effect_proven_pairs=0`); `ExtensionRuntimeTokenQualityArtifactPaths.cs:10-13,41-43` (`ArtifactDir=<repo>/artifacts/extension-runtime-token-quality-learnability`, `FindRepoRoot` бросает если папки нет) | Самый крупный «quality scoring» extension-тест не запускает скоринг — он проверяет ФОРМУ уже закоммиченного JSONL и утверждает НУЛЕВОЙ результат (0 экономии/0 adaptive-effect/0 claims) по 108 строкам всех 3 хостов; плюс жёсткая привязка к committed-артефактам (на чистом клоне коллекция падает на fixture-init). Отдельно от P27 (реальность экономии токенов — данные) и P35 gap-5 (scorer OS/artifact-coupling) — здесь именно тест = статический null-result assertion. | Сделать тест исполняющим: прогнать скоринг через реальный сервис/машину на held-out и ассертить измеренные числа; фикстуры генерировать/восстанавливать в самом тесте; committed-JSONL держать как снапшот, не как единственный источник. |
| Governance-writer и path-const-класс в тест-сборке; тесты пишут `docs/plans`+`artifacts` execution-state | QA-INTEGRITY DEFECT / non-test governance code in test assembly | `ExtensionExecutionStatePatchWriter.cs:4-49` (пишет `execution-state-patches/<phase>-state-patch.json`, schema `xiigen.extension_execution_state_patch.v1`, `WritePatch`/`ReadPatchFields`); `ExtensionRuntimeTokenQualityArtifactPaths.cs:6,15-19` (`StatePath=docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`) | В `Tests/` компилируется не-тестовый код, производящий/мутирующий governance/plan-state артефакты; тесты выступают производителями execution-state, а не только верификаторами. Тест-прогон имеет побочный эффект записи governance-состояния в `docs/plans`/`artifacts`. Ново (никакой прошлый проход не отметил writer-in-test-assembly). | Вынести writer/paths в отдельный prod/tooling-проект; тесты — только читают/ассертят, не пишут governance-state; запретить тестам мутировать `docs/plans` execution-state; grep 0 не-тестовых writer-классов в `Tests/`. |
| Записанный зелёный baseline-прогон extension-набора | UNRECORDED / no full-suite .trx; baseline number unbacked | `Tests/TestResults`: `total` максимум 5 (`pass144-exact.trx:24 total=1` DesignConversion; answer-composer `total=2..5`, май); ~70 GUID-deployment-папок 07-03/07-04 без полного `.trx`; свежая GUID (07-08) пуста; `.github` отсутствует (P35) | Ни один сохранённый `.trx` не фиксирует полный прогон (~1488) — все узкие filtered-запуски; CLAUDE.md-baseline «≥1488 passed, 0 failed» не подтверждён воспроизводящим артефактом, а зелёный/Rule-4-gate статус самого extension-набора не доказан ни одним записанным прогоном. Отдельно от P35 gap-6 (собираемость под текущее дерево at-risk) — здесь именно отсутствие записанного full-green `.trx`. | Прогнать полный `dotnet test` (или трейт-фильтр extension) под текущее дерево, сохранить `.trx` с `total/passed/failed` и привязкой к commit-hash; включить в CI как воспроизводимый gate; extension-набор гейтить отдельным трейтом. |
| e2e/host-слой extension-пирамиды в ядре | MISSING TOP LAYER / sole *E2e* file is other domain | инвентарь: единственный `*E2e*`-файл = `DesignConversionLocalFullLoopE2eTests` (домен design-to-code); грепы `e2e/EndToEnd` в extension-тестах = только строки в `ExtensionQuality03ProofContractRunnerTests`/`...NonInterRaterProofProducerTests`; `ProcessStart/FileName` в extension-тестах = только локальный `powershell` scorer (`XiigenExtensionApiContractsTests.cs`, `ExtensionTokenQualityRunnerHarnessTests.cs` — RunFullHostScorer, P35) | В core `Tests/` нет ни одного e2e/host-теста для Claude/Cursor/Codex; вершина тест-пирамиды (запуск сквозь реальный хост) для расширений отсутствует, весь набор — in-process boundary/contract/artifact. Единственный `*E2e*` принадлежит другому домену. Отдельно от P12/P16/P20 (Path B в host-репо) — здесь именно отсутствие e2e-слоя в .NET-наборе ядра. | Ввести отдельный (изолированный) e2e/host-слой для extension-путей — запуск через собранную host-команду с prompt→visible answer и correctness-ассертом; пометить трейтом; не выдавать boundary/contract-тесты за e2e. |

## Вывод P39

Extension-QA в ядре **обширна по объёму и добросовестна по честности** (десятки файлов, строгие
no-overclaim/no-leak-ассерты), но по СПОСОБНОСТИ пуста: 0 положительных semantic-quality-ассертов,
0 correctness-oracle, флагманский 222-КБ-тест = статический null-result по committed-JSONL, e2e/host-слой
для трёх расширений отсутствует, зелёный full-baseline на диске не зафиксирован, а в тест-сборке живёт
governance-writer. То есть даже полностью «зелёный» extension-QA-прогон доказывает только, что
расширения ничего не переобещают — не что они работают.
