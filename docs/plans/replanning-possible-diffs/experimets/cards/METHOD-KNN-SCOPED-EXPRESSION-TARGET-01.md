# METHOD-KNN-SCOPED-EXPRESSION-TARGET-01 — выражение-тело в обучаемой целевой проекции

<!-- CODEX-POST131-CURRENT:START -->
## Действующее уточнение после P131 — 3 ноября 2026

**Проверка `POST131-Q130-RECORDED-DELTA`. WRONG:** верхнее «Будущий назначенный опыт» и старый прогноз 2/4→4/4 разрешают повтор ST01. **CORRECT:** живой Q130 уже записал cold 1/4→A 2/4/B 4/4 для k1/k5, E2/E4 2/2, 4 Train/32 пары/32 Predict; превышение ресурса>5% сохраняет HOLD переноса. Результат: [отчёт ST01](../../../../../experiments-v2/RP01-MICRO/ST01/results/attempt-0001/report.md) и [метрики](../../../../../experiments-v2/RP01-MICRO/ST01/results/attempt-0001/raw/metrics.json). Текущий копируемый prompt учитывает этот исполненный пакет; первоначальная постановка остаётся историей. **Путь приёмки:** source/symbol/span→actual `_pairs.Next.KindBag`→записанный native Predict→пересчитанный scope/score. Не принимать совпадение ID/длины/summary вместо actual expression payload. Порчу сохранённой копии проверять без новых methods, Train или Predict; отсутствие persistence API не обходить новым сериализатором. Ни этот блок, ни старый NEXT не назначают новую научную очередь; скопированный prompt должен явно сохранять `RESULT_RECORDED`, конечный scope проверки и HOLD. Четыре per-arm JSON согласованы с aggregate; все 32 фактические пары Current/Next совпали с material. Повторные Current и три копии четырёх семей при k5 не являются 12 независимыми семьями. Исправление experimental extractor и native predictor само по себе не закрывает ordinary product consumer и обновление прежних ошибочных targets.

**Область:** связь raw с native проверяется только по реально существующим либо явно назначенным записям. Если такого источника нет, записать точный предел подтверждения. Этот блок не назначает новый Train/Flow/Load, persistence API, повтор PRIMARY или новую научную очередь. Integrity refusal и честный отрицательный scientific result различаются.

Основание: [аудит 149](../../fixes-based-on-experiments/codex-149-post131-transfer-and-experiment-audit-2026-11-03.md) · [план 150](../../fixes-based-on-experiments/codex-150-post131-addressed-repair-plan-2026-11-03.md) · [покрытие 151](../../fixes-based-on-experiments/codex-151-post131-card-coverage-2026-11-03.md) · [договор 49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md). Ниже сохранена прежняя постановка; текущий блок уточняет только указанную границу.
<!-- CODEX-POST131-CURRENT:END -->


<a id="current-executor-task"></a>
## Текущее задание после сверки P126–P128 — 3 ноября 2026

<!-- CODEX-POST128-CURRENT:START -->
**Владелец: `METHOD-KNN-SCOPED-EXPRESSION-TARGET-01 / Q130`. Состояние:** `RESULT_RECORDED`: ST01/attempt-0001 уже выполнен; текущий объём — проверка сохранённых свидетельств и адресная приёмка переноса без повторного PRIMARY. P128 показывает: равные keys/counts не подтверждают фактические targets; P112 — одинаковый текст не стирает SourceRowId.

**Проверяемый путь:** ExtractWithTargetScope → TrainWithTargetScope → actual _pairs.Next.KindBag → native Predict k1/k5 → общий finalizer.

**Положительный контроль:** Собственные expression-body цели сохраняют payload, symbol/span, owner и исходные IDs. Разрешённые копии k5 — кратность одной семьи, не пять независимых семей. Честный полный набор проходит.

**Различающий отрицательный контроль:** При прежних ID/N заменить KindBag/expression payload либо внести чужую nested lambda/local function; независимый scope checker отвергает. Не выдумывать недоступный neighbor trace. Успешный output без верной цели не закрывает семантический результат.

**Итог, предел и расход:** Сохраняются 4 Train, 32 пары и 32 Predict. Отсутствующий checkpoint API не заменять сериализатором журнала, не добавлять Compile/facade/restore. Равные правильные A/B означают нулевую добавочную дельту, не отсутствие обучения. Поле, предикат и влияние на итог назвать до проверки сохранённых данных.

**Текст для исполнителя — передать целиком:**

> Работай по уже записанному ID `METHOD-KNN-SCOPED-EXPRESSION-TARGET-01 / Q130`, сохрани исходный паспорт и его pin. Прочитай действующие AGENTS/CLAUDE и применимые архитектурные, planning/plan-review, code-examination, test-integrity, QA/automation/debug, prompt-patch-authoring и human-communication навыки; запиши фактически прочитанное. Проследи путь: ExtractWithTargetScope → TrainWithTargetScope → actual _pairs.Next.KindBag → native Predict k1/k5 → общий finalizer. Сначала на существующих сохранённых свидетельствах проверь: Собственные expression-body цели сохраняют payload, symbol/span, owner и исходные IDs. Разрешённые копии k5 — кратность одной семьи, не пять независимых семей. Честный полный набор проходит. Затем на отдельной копии сохранённых свидетельств одной адресной порчей: При прежних ID/N заменить KindBag/expression payload либо внести чужую nested lambda/local function; независимый scope checker отвергает. Не выдумывать недоступный neighbor trace. Успешный output без верной цели не закрывает семантический результат. Обязательное поле должно иметь названного потребителя, предикат отказа и влияние на общий итог/exit. Сохраняются 4 Train, 32 пары и 32 Predict. Отсутствующий checkpoint API не заменять сериализатором журнала, не добавлять Compile/facade/restore. Равные правильные A/B означают нулевую добавочную дельту, не отсутствие обучения. Поле, предикат и влияние на итог назвать до saved-only проверки. SOURCE_IMPLEMENTED и статическая сверка не заменяют нативную приёмку конечного source/DLL. Сборка, discovery, целевые тесты и обоснованная правка неверного теста входят в уже назначенный ремонт по 45/46; при отсутствии runtime сохранить точную ошибку среды, не сочинять PASS. Saved reanalysis не называть новым Train/Flow. STOP/NEXT пользователя имеет приоритет. Обязательная дельта POST131 (`POST131-Q130-RECORDED-DELTA`): Статус Q130 — RESULT_RECORDED: ST01/attempt-0001 уже выполнил 4 Train/32 пары/32 Predict; cold 1/4→A 2/4/B 4/4 иE2/E4=2/2, transfer HOLD из-за затрат>5%. Проверяй существующие Current/Next/material и 32 native pairs; четыре per-arm JSON совпадают с aggregate. Повторные Current и три копии четырёх семей при k5 не являются 12 независимыми семьями. Experimental extractor→native predictor не закрывает ordinary product consumer. Эта задача проверяет сохранённые свидетельства; новый PRIMARY, новые методы, Train/Predict/Compile/loader и новая очередь не назначаются. Применяй [договор 49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md); связывай raw с native только по реально существующим либо явно назначенным записям.

Основание: [аудит 145](../../fixes-based-on-experiments/codex-145-post128-transfer-and-experiment-audit-2026-11-03.md), [адресный ремонт 146](../../fixes-based-on-experiments/codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-03), [нативный договор 45](../45-learning-claim-and-native-acceptance-contract-2026-11-03.md), [QA46](../46-qa-execution-test-repair-and-fresh-results-2026-11-03.md). Исторические блоки ниже сохраняют свои данные, gold, factor, дозы и команды. Новый блок уточняет чтение и приёмку, не запускает PRIMARY и не подтверждает заново все старые результаты.
<!-- CODEX-POST128-CURRENT:END -->

## Исторические уточнения и исходный паспорт

Датированные блоки ниже сохраняются как история постановки. Их future/DESIGNED_NOT_RUN и команды не отменяют записанный ST01 и не разрешают повтор; действующий статус и задание находятся выше.

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — реальное целевое поддерево и native Predict

Связать фактические bytes исходного файла, target symbol/span, extracted expression/body и output pair, который действительно принял TrainWithTargetScope. Positive expression-body цели сохраняет собственный смысл; negative добавляет чужую nested lambda/local-function expression либо меняет extracted payload при прежнем content hash — независимый scope comparator отвергает до Train.

После обучения обычный native Predict читает назначенную target memory; checkpoint metadata и direct helper не заменяют этот вызов. Прогноз 2/4→4/4 не выдаётся за результат. Сохраняются 4 Train/32 предъявления/32 Predict и k1/k5; новые методы/fixtures или преобразование соседнего ProjectWiki механизма не назначаются.

Основание: [адресный план 135](../../fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- POSTMERGE-HARDENING-2026-11-02:START -->
## M03/M05 — extracted target доходит до прямого native Predict

Q130 сравнивает только outer expression-body target projection. Связать ExtractWithTargetScope → native TrainWithTargetScope → фактический _pairs.Next.KindBag → direct Predict на существующих k1/k5 панелях. Correct PackedString либо ID при чужом KindBag недостаточен: предусмотренная копия extracted target с подменённым KindBag при прежних Methods/counts должна быть отвергнута тем же структурным checker без дополнительного Predict. S1/S2 сохраняют границы lambda/local-function, E2/E4 — блочные цели. Сохраняются4 fits/32 pair presentations/32 Predict; k5 multiplicity, vote и порядок общие. Cascade, rank2-only панель, ordinary Compile/facade и новый loader не назначаются; невыданные native result поля не заменяются придуманной trace.

Основание и выбранный ремонт: [план127](../../fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md). Исходные результаты и учебные дозы сохраняются. Контроли реализации не добавляют скрытые Train/Read к научному паспорту; их стоимость и scope указываются отдельно.
<!-- POSTMERGE-HARDENING-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [120-executor-context-and-test-repair-contract](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница SCOPED-TARGET:** WRONG: обычный expression extractor, Compile либо fresh-load surrogate закрыли PackMethod/outer target. CORRECT: copy input extractor общий; native TrainWithTargetScope/Predict различает block/expression target и исключает вложенные lambda/local-function тела. Положительный block control сохраняется, expression target не превращается в guessed Ret. Текущий distance tiebreak общий; k5 majority не новый tie factor.4fits/32pairs/32Predict остаются. Native loader отсутствует: не добавлять restore, external mutation, Grammar/Ngram/Compile; unexpected2/4→4/4 сохранять как неподтверждённое ожидание, не менять gold.

**Исходный адрес из паспорта:** `12.SemanticCompiler/Prediction/MethodKnn.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
**Действующее согласование:** [40-effective-executor-context](../40-effective-executor-context-2026-11-02.md) связывает роль, scorer и стадию попытки; [120-post102-transfer-and-executor-audit](https://github.com/lubochka/llm_mvp_core/blob/d18b716e4a855f6dd94415104f48999876108a51/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-post102-transfer-and-executor-audit-2026-11-02.md) — отдельный источник, не этот договор120 и не разрешение повторить recorded PRIMARY. Этот адресный абзац заменяет прежний общий пример только в своём факторе; данные/дозы/команды ниже сохранены.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-CODEX-AUDIT-2026-11-02:START -->
## Действующее уточнение POST107 — Scope цели должен изменить native обученную пару, а не красивую упаковку

**Проверка `POST107-Q130`. WRONG:** исправленный PackedString или guessed `Ret` считать изменённой памятью, хотя predictor читает `Next.KindBag`. **CORRECT:** проследить `ExtractOuterBodyKindBag → ExtractWithTargetScope → MethodKnnPredictor.TrainWithTargetScope → _pairs.Next.KindBag → Predict`. Copied seam ограничена extractor; native predictor, vote/distance policy и порядок общие. До Train A совпадает с native на назначенных T1–T4/S1/S2; отсутствие Root Await у стрелочного N — проверяемая причина, не повод сменить gold.

**Честный и испорченный контроль.** S1 не выпускает внутреннюю lambda, S2 — Try/Catch local function; E2/E4 подтверждают сохранённые блочные цели. В копии extracted target поменять KindBag одной строки при прежних Methods/counts: тот же checker границ/ключей должен отвергнуть несогласованность. Actual acquired outputs оцениваются по всем четырём EXAM без знания имени мутации. K5 три копии каждой цели — прямо назначенная multiplicity panel; её не выдавать за три независимых source families и не менять dedupe/vote как дополнительный фактор.

**Предел и бюджет.** Остаются4 fits/32 pair presentations/32 native Predict. Ожидаемые2/4→4/4 не измерены; совпадение Current features с EXAM ограничивает вывод усвоением пар. Отсутствующий native checkpoint не заменяется surrogate serializer; фасад/Compile и новый loader не добавляются в бюджет. Запуск не разрешён этим усилением.

Основание и общий порядок проверки: [аудит POST107](../../fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md) и [инструкция41](../codex-41-post107-executor-checks-2026-11-02.md). Этот верхний блок уточняет только названные доказательства и будущую приёмку; исходный текст/результаты ниже сохранены, очередь и разрешение запуска не меняются.
<!-- POST107-CODEX-AUDIT-2026-11-02:END -->

<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Scoped target при общем новом tiebreak

**Проверка `POST102-Q130`. WRONG.** Обычный P83 или новый distance tiebreak уже починили ExtractOuterBodyKindBag; guessed Ret делает expected answer законным.

**CORRECT.** Проверяется только outer target projection expression body через native TrainWithTargetScope/Predict. Текущий distance tiebreak общий для arms; k5 panel с тремя копиями каждой цели проверяет большинство, не новый tie factor.

**Адресная приёмка.** Сохранить4fits/32pair presentations/32Predict, k1/k5 и S1/S2 lambda/local-function exclusions. Ожидаемые2/4→4/4 пока не измерены; acquired pair recall не является unseen generalization. Native checkpoint API отсутствует: сериализованный surrogate не выдавать за восстановление этого predictor.

Применять [единый контекст40](../40-effective-executor-context-2026-11-02.md) и [адресный план121](../../fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md): актуальные роли навыков, один рабочий оценщик, ordinary consumer и реакция на test/report failure. Это ограничивает вывод и усиливает приёмку; исходные factor, доза, fixed panel и порядок сохраняются. Исправление saved evidence не является новой PRIMARY, а наличие этого блока не разрешает запуск. Исторические указания ниже действуют только в своей исходной области; при противоречии бюджета/фактора требуется адресное решение до исполнения.
<!-- POST102-AUDIT-2026-11-02:END -->


**Q130 · DESIGNED_NOT_RUN · 01.11.2026 · тема: C#, профиль D02.** Это новый исследовательский рецепт, не результат и не разрешение на запуск. Нынешняя фаза — READ_ONLY_RESEARCH + Markdown. В fixes-based-on-experiments эта гипотеза не получает допуск, пока не измерена польза настоящего обучения. Старые P83 и METHOD-KNN-TARGET-SCOPE-01 сохраняются и не переоткрываются.

## Что именно ещё не проверено

В [MethodKnn.cs](../../../../../12.SemanticCompiler/Prediction/MethodKnn.cs):102–120 `ExtractOuterBodyKindBag` возвращает пустое множество при `method.Body is null`. Между тем обычный `ExtractKindBag`:160–186 уже читает `ExpressionBody.Expression.DescendantNodesAndSelf()`. Это два разных производителя состояния. `ExtractWithTargetScope`:67–93 вызывает первый для целевых проекций, а `TrainWithTargetScope`:280–285 заменяет ими **Next.KindBag** в реально обучаемой `_pairs`. `Predict`:301–307 затем голосует по `CanonicalType(Next.KindBag)`. Следовательно, стрелочный целевой метод может учиться как Simple даже после исправления обычного извлечения P83.

Дедуп: [старый scope-опыт](METHOD-KNN-TARGET-SCOPE-01.md), пункт2 рецепта, прямо задавал Body-null→empty и запрещал менять ExpressionBody. В [P8301/Program.cs](../../../../../experiments-v2/RP01-MICRO/P8301/Program.cs):70–79 все четыре **целевых N** блочные; его стрелочные методы стоят в роли Current. В текущем срезе документов найденные P83/scope/114 не проверяли эту комбинацию. Это ограниченная проверка новизны по конкретному writer/state/reader, не заявление об отсутствии любого похожего текста.

`PackMethod` тоже пропускает expression body, но `PackedString` не читается предсказателем. Опыт только на упаковке не назначен. Здесь проверяется именно путь `ExtractWithTargetScope → native TrainWithTargetScope → _pairs.Next.KindBag → native Predict`, поэтому изменение участвует в обучении и чтении усвоенного результата.

## Единственный фактор и точные файлы будущего host

Будущий host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/ST01/`. На момент подготовки каталога нет. Будущие файлы: `Experiment.csproj`, `Program.cs`, `Run-Experiment.ps1`, `src/ScopedExpressionSeam.cs`. ProjectReference — `../../../12.SemanticCompiler/SemanticCompiler.csproj`; net9.0, как в существующем P8301. Текущий документ не создаёт эти файлы.

Скопировать **только MethodExtractor** с его приватными помощниками в experimental namespace, возвращая прежние native `PackedMethod`/`MethodScopeMaterial`. Predictor не копировать: оба плеча используют настоящий `MethodKnnPredictor.TrainWithTargetScope` и `Predict`. A — точная текущая логика. B меняет только `ExtractOuterBodyKindBag`: для Body оставить нынешний обход; иначе для ExpressionBody использовать обход **Expression.DescendantNodesAndSelf** с тем же запретом спуска в `LocalFunctionStatementSyntax` и `AnonymousFunctionExpressionSyntax` и исключением самих boundary nodes. При отсутствии обоих тел — прежнее пустое множество. Корневой Await должен попасть в проекцию; корневая lambda и её дети не должны попасть. AbbreviateKind, ordinary features, Current/query bags, k, vote, порядок, pair adjacency и CanonicalType неизменны. Не добавлять Ret искусственно к выражению.

До обучения сравнить copied A с native `ExtractWithTargetScope` на всех четырёх TRAIN и двух структурных контролях: Methods/TargetScopes должны совпасть по ключам и множествам. При отличии зафиксировать PREPARATION_INVALID, без Train. Не подгонять B, oracle или данные. Наблюдение snapshot `_pairs` допустимо только read-only; задавать память через reflection запрещено. В обоих плечах память создаёт только native Train.

## Автором назначенные данные

Каждый TRAIN — отдельный C# файл с одной соседней парой M→N. Это parser fixtures, их самостоятельная компилируемость приложением не заявлена; компилируется только host. Исходники являются входами задачи классификации методов, не уроками разных предметных тем. Gold буквальный, он никогда не подаётся extractor/predictor.

| ID/fileId | TRAIN source | Gold вида N |
|---|---|---|
| T1/0 | `class A{void M(){for(int i=0;i<2;i++){}}async Task<int> N()=>await R();}` | Async |
| T2/1 | `class B{void M(int n){if(n<0)return;}async Task<int> N(){return await R();}}` | Async |
| T3/2 | `class C{void M(){try{}catch{}}int N()=>Calc();}` | Mixed |
| T4/3 | `class D{void M(){while(false){}}void N(){}}` | Simple |

EXAM содержит только запросы без N, не обучается: E1=`class QA{void Query(){for(int i=0;i<2;i++){}}}`; E2=`class QB{void Query(int n){if(n<0)return;}}`; E3=`class QC{void Query(){try{}catch{}}}`; E4=`class QD{void Query(){while(false){}}}`. Gold: Async, Async, Mixed, Simple. E2/E4 — положительные контроли сохранности уже обучаемых блочных целей. Имена класса/метода отличны, но признаки запросов совпадают с Current: это **усвоение назначенных пар, не семантическое обобщение**. Constant ceiling=2/4 (Async); холодный fallback Mixed случайно даёт1/4 и не считается приобретённым навыком.

Два чисто структурных контроля, не дополнительные уроки/предсказания: S1=`class S{System.Func<Task> N()=>async()=>await R();}` — проекция пустая в обоих плечах, вложенная lambda не должна стать Async; S2=`class S{int N(){int Local(){try{return 1;}catch{return 0;}}return 8;}}` — A/B проекции равны, Try/Catch вложенного local function исключены. Эти проверки не заменяют обучение ниже.

## Обучение, бюджет и ожидаемое различие

Сохранённого native MethodKnn checkpoint для этих пар не найдено; проверенного save/load API у этого learner нет. Здесь acquisition из пустого экземпляра. Повторный Train не называть восстановлением, serializer не придумывать. Четыре явно назначенных fit — разные A/B и k-панели, не скрытое повторение обучения.

| Панель | Экземпляры | Train calls | Предъявления пар | Predict до | Predict после |
|---|---:|---:|---:|---:|---:|
| K1: один сосед, T1–T4 по одному | A/B, каждый fresh | 2 | 2×4=8 | 2×4=8 | 2×4=8 |
| K5: пять соседей, три копии каждой T1–T4 | A/B, каждый fresh | 2 | 2×12=24 | 2×4=8 | 2×4=8 |
| **Всего** | **4** | **4** | **32** | **16** | **16** |

В K5 порядок T1×3,T2×3,T3×3,T4×3; каждая копия — отдельный файл, fileId0–11, ровно одна пара. Межфайловые пары запрещены. После Train TrainingPairs обязан быть4 либо12 соответственно. Все32 Predict — native вызовы; дополнительные вызовы для отчёта не нужны. Векторная/нейронная модель отсутствует, это обучение KNN-памяти, а не0-step нейронный опыт.

**Аналитическое ожидание, пока не результат:** A целевые метки [Simple,Async,Simple,Simple], B [Async,Async,Mixed,Simple]. У запросов четыре различных Current bags; в K1 свой сосед имеет расстояние0, у остальных>0. В K5 три точных соседа дают три одинаковых голоса против максимум двух остальных. Поэтому ожидается cold1/4 → A2/4 и B4/4 на каждой панели, E2/E4 сохранены2/2. Улучшение2/4→4/4 считается полезным локальным результатом при фактическом подтверждении. Одна корректная строка до и после не объявляется generalization; по каждому ID показать приобретение/потерю и происхождение ответа.

Отдельно измерить extraction, native Train wall/CPU/allocations и Predict wall, retained state/peak по возможности. Любое полезное улучшение именованной метрики — успех этой метрики/домена, accuracy не единственный критерий; рядом все ухудшения, ресурсы ниже разрешения — UNKNOWN. Для переноса приоритет >5% полезного изменения без >5% регрессии прочих назначенных метрик (CPU-исключение отдельно); эксперимент не даёт автоматического PRODUCT_DONE. k5 здесь остаётся прямым predictor, не end-to-end Compile/маршрутизацией промпта. Продолжение через facade и сохранение требуют отдельного доказанного контракта, их расход в этот опыт не спрятан.

## Исполнитель, команды, результаты, ожидание

Исполнитель — текущая модель сессии по умолчанию, без повышения модели/effort; исследователь Astra не становится runtime-исполнителем. Перед разрешённым запуском **полностью и заново** прочитать AGENTS.md, CLAUDE.md и `docs/skills/architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Отчёт на русском. Проверить наличие SDK9 и ProjectReference до Train; одна restore/build host (до300s), runtime до120s, общий deadline600s, одна попытка, без прогрева/ретрая/добавочных fits. Подготовка dependencies входит в эту же попытку, не меняет набор. Если prerequisites действительно недоступны — точный PREPARATION_BLOCKED,0Train, а не провал учебной гипотезы.

Будущая команда (runner ещё не написан, здесь не запускалась), cwd=`C:/Projects/llm_mvp_core`:

```powershell
pwsh -NoProfile -File experiments-v2/RP01-MICRO/ST01/Run-Experiment.ps1 -AttemptId attempt-0001
```

Runner должен создать `results/attempt-0001` только create-new. Если уже существует — читать его status/handle, не удалять, не перезаписывать, не подбирать новый номер автоматически. Записать raw `metrics.json` (построчные before/after, actual4Train/32pairs/32Predict, source provenance, ресурсы), `status.json`, stdout/stderr и `report.md`. Числа ожидания выше не копировать в metrics. Условие nonzero learned control проверять по E2/E4 после реального Train; 0→0, partial A/B или срыв positive control оставляют учебную цель OPEN. Старые отчёты не менять ради новой постановки.

До launch найти реальные launch/wait/resume инструменты и их схемы по [контракту31](../31-tool-discovery-and-wait.md). Одна выдача команды, затем bounded wait≤60s исходного session/process handle до terminal/deadline; yield и отсутствие stdout не означают отказ. Не делать busy polling, не создавать watchdog/summary-скрипты и не завершать ход словами «жду». При timeout сохранить фактический расход, остановить только свой подтверждённый процесс; скрытый повтор запрещён. Прямая команда Любы STOP/NEXT немедленно прекращает текущую работу: не продолжать прошлый опыт, отчёт, checksums или исправления под видом перехода. Дальнейшее назначение берётся только из [единого индекса](00-execution-index.md#next-contract).

Это также полный текст задания исполнителю: отдельный промпт не может менять корпус, host, фактор, бюджет, язык, модель или границу разрешения. Исследование записало0запусков; после будущего результата возможный адрес исправления — только `ExtractOuterBodyKindBag` внутри существующего MethodExtractor, с проверкой потребителя35.
