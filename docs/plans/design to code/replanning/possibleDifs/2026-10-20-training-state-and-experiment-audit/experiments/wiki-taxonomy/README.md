# WIKI-TAXONOMY-01 — новые маршруты домена превращаются в отрицательный учебный класс

**Факт кода:** enum содержит 17 допустимых маршрутов, а native обучаемый классификатор — 15. Два новых вида обучения, `SystemBoundaryMapperTraining` и `CrossStackBusinessFlowGraphTraining`, отсутствуют в `Labels`. При передаче законной TRAIN-строки с одной из этих меток `LabelIndex` возвращает последний старый индекс, то есть `NotProjectWiki`. Это не просто невозможный вывод: такой пример обучает веса **другого** маршрута. **Гипотеза опыта:** согласование пространства выходов и обучения улучшит learned selection двух новых маршрутов; эффект и регрессии пока не измерены.

Это учебный блокер покрытия локального project-wiki router. Никакого утверждения, что он объясняет всё падение neural RP01, нет. Здесь обучаются веса токенов/биграмм перцептрона; поля DTO `Chosen`/`Rejected` не участвуют в этом learner, хотя DTO называется DPO. Действующее правило-based routing может успешно обслуживать те же запросы независимо от дефекта learned classifier.

## Доказательства и отличие от уже известных тем

| Доказательство | Точный источник и вывод |
|---|---|
| Текущий контракт включает обе новые метки | [ProjectWikiDomainModels.cs L473–492](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiDomainModels.cs#L473-L492): строки479–480; всего17 enum значений |
| Модель хранит старые15 выходов | [TrainableModel.cs L25–42](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L25-L42); последний — `NotProjectWiki` |
| Неверный класс попадает прямо в update | [Train L58–79](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L58-L79): `var expected = LabelIndex(row.ExpectedRouteKind);`; затем `weights[expected] += LearningRate * value;` |
| Неизвестная метка молча переименовывается | [LabelIndex L295–299](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L295-L299): `return index < 0 ? Labels.Length - 1 : index;` |
| Вывод двух новых маршрутов тоже отсутствует | [reader switch L203–221](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L203-L221); решения строятся только из `Labels[predicted]` |
| Это реальные текущие target units | [static router L61–65](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRouterUnit.cs#L61-L65) задаёт exact IDs/inputs; [orchestrator L205–226](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs#L205-L226) dispatch-ит оба target runner |
| В донорской ветке совпадали15/15 | [local_wiki_planning enum L381–398](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiDomainModels.cs#L381-L398) и [его Labels L10–27](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L10-L27). Это конкретное расхождение при развитии контракта, а не предположение о непроверенной древней ветке |

Source pin: current `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`, donor `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`. Полный current `ProjectWikiRequestRoutingDpoGenerator.cs` просмотрен: имён двух новых enum значений в нём нет; DTO получает `seed.ExpectedRouteKind` ([generator L27–42](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingDpoGenerator.cs#L27-L42)). Поэтому мы не утверждаем, что стандартный старый batch50/50 уже включал такие примеры или что исторический успешный отчёт опровергнут. Его матрица могла не проверять новые классы. Опыт добавляет конкретный adversarial учебный срез ровно по легальному текущему контракту.

Novelty inventory ограничен просмотренными корпусами: planning577 Markdown@`fb9aadf69e0669d36b820d61f76fe3ecc8697a7d`, protected286 Markdown@current. В них аналогичной карточки про **две пропущенные output labels и их relabel в отрицательный класс** не обнаружено. Последний `KN-ROUTER` относится к `RoutingProfileTrainer → BaselineKeywordCandidateGenerator` и `C#/.NET` canonicalization; recovery49 — к snapshot/gate; `P33`, `P93`, `P118` — к другим вмешательствам. Это не заявление абсолютной исторической новизны.

## Где деталь реально участвует в обучении

Native цепочка: `ProjectWikiRequestRoutingTrainingRunner.TrainAndEvaluateAsync` → `ProjectWikiRequestRoutingTrainableModel.Train(trainingRows)` → `Featurize(row.Prompt)` → `LabelIndex(ExpectedRouteKind)` → запись `_tokenWeights[token][class]`, `_biases[class]` → `RouteForEvaluation` → `Score`/argmax → `Decision`. [Native writer и reader L43–53](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L43-L53). Это проверка реально изменяемой модели и native учебного потребителя, не oracle таблица.

Состояние: каждый `Train` создаёт новую модель с нуля; writer меняет коэффициенты весов и bias. `ToSnapshot` сохраняет `Labels`, веса и bias. Reader читает их при выборе цели; host наблюдает его решения до/после на одинаковом EXAM. Cold-load и продолжение существующего обучения в этом первом опыте не участвуют. Текущий `ProjectWikiTrainingOrchestrator` вызывает статический `_router.Route(prompt)` L205, поэтому локальный опыт **не доказывает**, что learned snapshot уже является общим product router. `TargetUnitId` проверяется как native выход, но target training не запускается.

## Единственный фактор A/B

A: точная текущая логика15 выходов. B: согласованный output contract17 выходов: добавить две метки **после** всех старых15, явно возвращать индекс `NotProjectWiki` для неизвестного enum и добавить две decoder-ветви с точными target IDs/required inputs из L61–65 static router. Это один фактор — пространство допустимых классов на стороне writer и reader; одна замена Labels без decoder оставила бы неполный механизм.

Старые индексы0–14 не меняются. B новые координаты15/16 нулевые. Initial-equivalence: одинаковы первые15 names/bias coordinates, оба словаря весов пусты; B имеет ещё две нулевые координаты. Полный hash начального состояния A/B различается законно, потому что разная размерность; comparator проверяет общую часть, а не требует невозможного равенства хэшей. `Enum.GetValues` вместо Labels запрещён: enum ordinal не совпадает с прежним порядком модели.

18эпох, learning-rate0.14, нормировка признаков, token/bigram extractor, порядок TRAIN, условие обновления, inference margin0.02 и gates остаются одинаковыми. Margin-исправление соседней карточки сюда не переносить. Старые snapshot positional load остаются отдельной осью: [FromSnapshot L86–93](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L86-L93) и [NormalizeVector L384–389](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L384-L389). Этот fresh-state опыт не доказывает cold migration.

## Фиксированные TRAIN и EXAM

| Split / ID | Prompt — точная строка | ExpectedRouteKind | Группа |
|---|---|---|---|
| TRAIN / T1 | `Train system boundary mapper for project evidence.` | `SystemBoundaryMapperTraining` | `system_boundary` |
| TRAIN / T2 | `Learn cross-stack business flow graph from client and server evidence.` | `CrossStackBusinessFlowGraphTraining` | `cross_stack` |
| TRAIN / T3 | `Train code trace builder from method call evidence.` | `CodeTraceTraining` | `legacy_code_trace` |
| TRAIN / T4 | `Translate this short greeting into Russian.` | `NotProjectWiki` | `ood_translate` |
| EXAM / Q1 | `Improve system boundary mapper using project evidence.` | `SystemBoundaryMapperTraining` | `system_boundary` |
| EXAM / Q2 | `Train system boundary mapper on API project evidence.` | `SystemBoundaryMapperTraining` | `system_boundary` |
| EXAM / Q3 | `Improve cross-stack business flow graph using client and server evidence.` | `CrossStackBusinessFlowGraphTraining` | `cross_stack` |
| EXAM / Q4 | `Learn cross-stack business flow graph for repository evidence.` | `CrossStackBusinessFlowGraphTraining` | `cross_stack` |
| EXAM / Q5 | `Improve code trace builder from method call evidence.` | `CodeTraceTraining` | `legacy_code_trace` |
| EXAM / Q6 | `Translate a polite greeting into English.` | `NotProjectWiki` | `ood_translate` |


Для новых меток требуется `Selected` + правильные route/target; `legacy_code_trace` — контроль старого положительного класса, `ood_translate` — контроль отрицательного класса. Точные expected target IDs находятся в `fixture.json` и проверяются рядом с RouteKind. На таком малом срезе языковое обобщение ограничено английскими парафразами команд. Строение AST, русская морфология, перевод и собственно построение cross-stack графа здесь не обучаются.

Если B не улучшает useful metric, но исправляет представимость пространства меток, это механизм с пока не подтверждённой пользой на этом EXAM, а не успешное качество. Если B улучшает новые маршруты и портит старый/OOD, сохранить обе стороны. Исход обучения неизвестен до native запуска.

## Куда переносить только после результатов и решения пользователя

Первая целевая правка — `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs`: `Labels`, `LabelIndex`, decoder switch. До реальной интеграции отдельно проверить именованное сопоставление snapshot labels при загрузке; простое изменение вектора/перестановка enum недопустимы. Второй последующий scope — расширить generator TRAIN/EXAM seeds новыми маршрутами, чтобы обычная оценка замечала покрытие, и проверить действующий consumer подключаемой модели. Ни generator, ни loader, ни production orchestration не являются вторым фактором текущего опыта.

## Точный небольшой запуск

**Статус поставленного scaffold: `NOT_BUILT / NOT_RUN`; причина `SDK_UNAVAILABLE`.** В сессии аудита `command -v dotnet` завершилась с кодом 1; стандартные каталоги SDK отсутствуют. Есть готовый исходный host, полный список зависимостей, фиксированные данные, команды и статическая проверка путей/хэшей. Компиляция и результаты обучения не заявлены. Для исполнения нужен .NET 9 SDK с локальными reference packs и Python 3; внешних NuGet-пакетов нет.

Пакет запускается прямо внутри planning: `experiments/wiki-taxonomy/scaffold/`. Под ним сохранены принятые пути `35.ExperimentsV2/Experiments/Rp01Micro/WIKI-TAXONOMY-01/` для копий и `experiments-v2/RP01-MICRO/WIKI-TAXONOMY-01/` для host. Ни подготовка, ни запуск не записывают файлы в core. При будущей одобренной интеграции это те же относительные пути отдельного experiment checkout, а не изменения `42.ProjectWikiDomain`.

В `scaffold/experiments-v2/RP01-MICRO/WIKI-TAXONOMY-01/WikiTaxonomyLearning.csproj` ровно четыре `Compile`:

| Файл | Содержание и происхождение |
|---|---|
| `NativeRoutingContracts.cs` | Шесть native типов: enum маршрутов L473–492, enum статусов L494–499, gate L505–520, decision L522–534, DPO DTO L536–544, snapshot L575–615 из `ProjectWikiDomainModels.cs` |
| `BaselineModel.cs` | Native learner L1–390, отдельный namespace; соседний `ILogicalUnit` wrapper не включён |
| `CandidateModel.cs` | Тот же learner с единственным фактором этой карточки |
| `Program.cs` | Измерение реальных вызовов A/B, native `Train`, native `RouteForEvaluation`, состояния и метрик |

Полный список source receipts находится в `source-provenance.json`: core commit, Git blob SHA, диапазоны извлечения и 15 точных `UnitId`/`FlowId` констант. Константы строк механически подставлены из pinned исходников; исполнитель не реализует заменители runner-ов. Неиспользуемые `using AgentInfrastructure` удалены, `RouteForEvaluation` открыт только в копии. Для одинакового наблюдения A/B добавлены два счётчика: число предъявлений строк и число обновлений. Сигнатуры/поля native contracts сохранены. Все остальные зависимости — стандартная библиотека .NET 9 (`System.Text`, JSON, collections, diagnostics, cryptography). `ProjectReference`/`PackageReference` отсутствуют; импорт родительских `Directory.Build.*` отключён; `NuGet.Config` очищает внешние package sources.

### Сборка без обучения

Выполнить **из корня checkout `xiigen-planning`**. Команды не содержат placeholder ID и не вызывают общий experiment dispatcher.

```bash
set -euo pipefail
task_package="$(pwd)/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit"
task_scaffold="$task_package/experiments/wiki-taxonomy/scaffold"
task_host="$task_scaffold/experiments-v2/RP01-MICRO/WIKI-TAXONOMY-01"
test -f "$task_host/WikiTaxonomyLearning.csproj"
if test -e "$task_host/bin" || test -e "$task_host/obj"
then
    echo 'EXISTING_BUILD_CONFLICT: preserve existing bin/obj; do not rebuild.' >&2
    exit 1
fi
dotnet --version
(cd "$task_scaffold" && sha256sum -c "35.ExperimentsV2/Experiments/Rp01Micro/WIKI-TAXONOMY-01/checksums.sha256")
dotnet restore "$task_host/WikiTaxonomyLearning.csproj" --configfile "$task_host/NuGet.Config"
dotnet build "$task_host/WikiTaxonomyLearning.csproj" --configuration Release --no-restore
```

Это один самостоятельный csproj с четырьмя C# файлами. Нельзя заменять его сборкой solution, проектом `42.ProjectWikiDomain`, `35.ExperimentsV2.csproj`, обычным `dotnet test` или pipeline всех экспериментов. Preflight останавливается с `EXISTING_BUILD_CONFLICT`, если `bin`/`obj` уже существуют: предыдущие build artifacts сохраняются, их нельзя удалять или автоматически перестраивать. Если нет SDK/reference packs, либо сборка не прошла, статус `NOT_BUILT`; runtime-данные не создавать. Исправление только технической ошибки scaffold допустимо отдельным append-only уточнением с новым checksum receipt; смена A/B алгоритма, fixture или бюджета — новая карточка, а не скрытая правка результата.

### Выполнение двух arms и сравнение

После успешной сборки, в той же shell-сессии:

```bash
dotnet "$task_host/bin/Release/net9.0/WikiTaxonomyLearning.dll" --arm A --output "$task_package/results/WIKI-TAXONOMY-01/A"
dotnet "$task_host/bin/Release/net9.0/WikiTaxonomyLearning.dll" --arm B --output "$task_package/results/WIKI-TAXONOMY-01/B"
python3 "$task_scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-TAXONOMY-01/compare.py" "$task_package/results/WIKI-TAXONOMY-01/A/report.json" "$task_package/results/WIKI-TAXONOMY-01/B/report.json" "$task_package/results/WIKI-TAXONOMY-01/comparison.json"
```

Выходы строго в новом planning package, вне защищённого `core/docs/plans/replanning-possible-diffs`. Host отказывается перезаписывать существующий каталог arm; comparator отказывается перезаписывать `comparison.json`. При коллизии остановиться с `EXISTING_RESULT_CONFLICT` и ссылкой на существующие результаты; не удалять их, не выбирать другое имя автоматически и не менять условия опыта. Повтор разрешается только отдельной append-only карточкой с собственными literal путями и бюджетом.

**Фиксированный бюджет A+B:** 4 TRAIN и 6 EXAM; две непустые native `Train`, две пустые `Train` для native начального состояния, всего четыре вызова API `Train`; 144 предъявлений TRAIN строк (`18` native эпох на arm); 24 вызовов native evaluation reader. Эпохи — повторные предъявления внутри одного обучения, не отдельные эксперименты. Ручная генерация фикстуры не вызывает teacher/LLM. Полное обучение target unit, neural DPO, publication, cold restore и orchestration не запускаются.

### Что сохранять и как считать

Каждый arm пишет `before-snapshot.json`, `after-snapshot.json`, точную `fixture.json`, `source-provenance.json`, `report.json`. Фикстура SHA-256: `b1c03bee21330c79c0c8dc8b04f7f7250e313f0ce119f36830feda76d26e0850`. Before получается через native `Train([])`: private constructor не заменяется. After получается через native `Train(TRAIN)` из такого же zero initialization. EXAM никогда не передаётся в train, native reader получает только `Prompt`. Точные TRAIN/EXAM строки различаются; это небольшие лексически близкие парафразы, а не доказательство далёкого обобщения.

Native snapshot включает служебные gate-поля с нулевым `TrainingRowCount`, потому что gate не прикрепляется; это не счётчик реальных предъявлений. `observedTrainingRows`, `rowPresentations` и `modelUpdateCount` измеряет host, а служебное native значение сохраняется отдельно с пояснением. Ни `GatePassed`, ни `PublishAllowed`, ни `SemanticQualityClaimed` не выставляются. Проверка модели штатным `RouteForEvaluation` — тот же reader, который вызывается внутри `TrainingRunner` L43–53; обычный `Route` с production gate не подменяется и не используется для заявления публикации.

Основной полезный показатель — доля **Selected с верными `RouteKind` и `TargetUnitId`**. Отдельно выводятся wrong-selected, defer, строки/группы доменов и все before→after изменения. Сравнение A/B — `(B_after−B_before)−(A_after−A_before)`. Совпадение одного `RouteKind` при `NeedsSecondPass` остаётся диагностикой: native defer возвращает `NotProjectWiki`, поэтому такое совпадение само по себе не является успешным выбором. `Confidence`/top-two margin — диагностика; существующий confidence clamp не чинится этим опытом.

Ресурсы: wall-time, process CPU, allocations текущего потока вокруг непустого `Train`, heap before/after, число параметров, размер snapshot. Comparator пишет отдельный `resourceDeltas` с A/B, B−A и направлением полезного изменения, без общего PASS; `deferredRate` помечен conditional diagnostic и не получает автоматический useful-success. `PeakWorkingSet` честно подписан как пик **всего процесса**, не как пик фазы обучения. Один очень короткий процесс на arm имеет JIT/таймерный шум; результат является наблюдением этой пары, а не доказательством устойчивого ускорения. Счётчик обновлений и изменение хэша сами по себе не полезный результат: нужен эффект на выходах или полезном ресурсе. Без последующей санкционированной карточки автоматические дополнительные повторы запрещены.

**Правило успеха:** любое измеренное улучшение полезного показателя считается успехом **по этому показателю и этому домену**; рост точности не обязателен. Рядом обязательно показать изменения всех остальных показателей, включая ухудшения. Снижение defer считается полезным вместе с качеством выбранных решений; замена безопасных отказов неверными решениями не объявляется улучшением качества. Возможен одновременно успех на одной группе и ухудшение на другой; это не скрывать сводным pass/fail.

### Конфликты и граница переноса

Старые карточки и защищённая папка остаются неизменными. Этот пакет дополняет известные проверки `P33`/`P93`/`P118` и recovery49, не переписывает их критерии. Source-copy сравнение не является исправлением production, результат автоматически ничего не публикует. Решение о дальнейшем переносе пользователь принимает по `comparison.json` и строкам EXAM. До решения нельзя чинить production тесты, менять rollout/gate, расширять все данные или запускать всю цепочку. Порядок: learning blockers → конфликтующие карточки → дополнительные изменения → периферийные проверки.
