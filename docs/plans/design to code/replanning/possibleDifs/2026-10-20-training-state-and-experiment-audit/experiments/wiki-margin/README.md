# WIKI-MARGIN-01 — correct argmax может не создавать обученного состояния

**Факт кода:** native `Train` пропускает строку, когда `predicted == expected`; при zero initialization argmax tie выбирает индекс0. Если малый TRAIN содержит только первый класс `ProjectWikiRequestRouterTraining`, все18эпох проходят без одного обновления. Current native reader требует ненулевой trained signal и минимальный top-two margin, поэтому продолжает возвращать `NeedsSecondPass`. **Гипотеза опыта:** margin-based update в учебной копии разорвёт этот fixed point и улучшит выбранные правильные решения на близких парафразах; одновременно bias может ухудшить OOD. Результат пока не измерен.

Здесь обучается локальный токеновый/биграммный перцептрон. Это не доказательство работы neural DPO и не универсальный рецепт для kNN/LoRA. Дефект границы особенно применим к one-class tiny batch/точечному дообучению; первый нативный `Train` каждый раз начинает с нуля. Мы не утверждаем, что все обычные смешанные50строчные batches попадают в zero-state trap.

## Доказательства и граница истории

| Доказательство | Точный источник и смысл |
|---|---|
| Первый output класс — router training | [Labels L25–42](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L25-L42) |
| Train начинает с нуля и рано пропускает строку | [Train L58–79](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L58-L79): `new double[Labels.Length]`; `if (predicted == expected) continue;` |
| Tie-break выбирает индекс0 | [MaxIndex L301–310](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L301-L310): `var best = 0;` и только строгое `>` |
| Reader отвергает нулевой/слабый learned state | [DecideOrDefer L155–184](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L155-L184): `!HasTrainedSignal()`, all-zero scores, margin below0.02; [HasTrainedSignal L266–268](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L266-L268) читает реальные `_biases`/`_tokenWeights` |
| Это штатный reader в оценке обучения | [TrainingRunner L43–53](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L43-L53) вызывает native `Train` и `RouteForEvaluation` |
| Ранний skip был и в доноре, current gate добавил наблюдаемую несовместимость | [local_wiki_planning Train L38–63](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L38-L63); [его Route L66–89](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L66-L89) не проверял learned signal/margin перед выбором. Старый default-selected маршрут не является доказательством изменения весов |

Source pin current `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`, donor `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`. Доказательство zero fixed point — статическое: нулевой state → равные scores → `best=0` → expected0 → skip → тот же state; индукция по строкам/эпохам. Это не замена runtime замеру и не выдуманные численные результаты. Для более общего «argmax верен, но margin<0.02» есть аналогичная несовместимость train/inference, однако данный первый эксперимент фиксирует простой достижимый случай без создания искусственных готовых весов.

Новизна проверена по planning577 Markdown@`fb9aadf69e0669d36b820d61f76fe3ecc8697a7d` и protected286 Markdown@current: найдено обсуждение confidence/gates/snapshot, но не matching вмешательство **skip-correct при нулевой/недостаточной margin и фактический current reader**. `P93` о confidence, `P118` о content/no-neighbors, recovery49 о restore; они остаются самостоятельными. Абсолютную новизну относительно всех когда-либо существовавших веток не заявляем.

## Native цепочка и состояние

`TrainingRunner` → `Train(TRAIN)` → token/bigram features → per-class score → условие update → `_tokenWeights` и `_biases` → `RouteForEvaluation(EXAM prompt)` → `HasTrainedSignal`/margin → selected/defer. Gold EXAM метки доступны только scorer-у. В DTO native learner использует `Prompt` и `ExpectedRouteKind`, а не `Chosen`/`Rejected`; host не выдаёт проверку разметки DTO за preference learning.

Before — native нулевое состояние; after — состояние native непустого `Train`. Оба оцениваются одинаковым native reader. Поле `DecisionStatus` обязательно, иначе `NotProjectWiki` у deferred OOD можно ошибочно посчитать правильным выполненным маршрутом. Состояние до/после EXAM не меняется; host проверяет это хэшем. Импорт/продолжение/checkpoint gate отдельно не проверяются.

## Единственный фактор A/B

A: current условие `predicted == expected → skip`. B: взять max-score **среди всех классов кроме expected**; если `score(expected)−bestOtherScore < 0.10`, выполнить тот же native update expected `+0.14·feature`, competitor `−0.14·feature`, bias±0.14. Если margin≥0.10, пропустить. Значение0.10 заранее фиксировано как учебный фактор, не подбирается по EXAM.

Когда argmax уже expected, competitor — runner-up, поэтому `+`/`−` не попадают в один класс и не зануляют исправление. Когда argmax неверен, competitor совпадает с неправильным argmax. Tie-break для competing classes сохраняет первый максимальный допустимый индекс. Все15 output labels, их порядок,18эпох, feature extractor, LR, reader/gates, source input и EXAM одинаковы. Candidate не добавляет два класса из taxonomy карточки.

Initial-equivalence здесь полная: labels15, bias0, пустая vocabulary и hash coefficient state A/B совпадают. Никакой фиктивный trained flag, ослабление `HasTrainedSignal`, отключение margin или присоединение успешного gate не разрешены.

## Фиксированные TRAIN и EXAM

| Split / ID | Prompt — точная строка | ExpectedRouteKind | Группа |
|---|---|---|---|
| TRAIN / T1 | `Train project wiki request router on routing examples.` | `ProjectWikiRequestRouterTraining` | `router_training` |
| TRAIN / T2 | `Improve project wiki request router using route labels.` | `ProjectWikiRequestRouterTraining` | `router_training` |
| EXAM / Q1 | `Train project wiki request router using labeled examples.` | `ProjectWikiRequestRouterTraining` | `router_training` |
| EXAM / Q2 | `Improve project wiki request router on routing labels.` | `ProjectWikiRequestRouterTraining` | `router_training` |
| EXAM / Q3 | `Translate a short greeting into Russian.` | `NotProjectWiki` | `ood_translate` |
| EXAM / Q4 | `Plan a weekend trip to the sea.` | `NotProjectWiki` | `ood_travel` |


Это преднамеренно one-class TRAIN. EXAM содержит два близких положительных запроса и два отрицательных домена. Сохранение/рост OOD wrong-selected — обязательная часть вывода: learner с положительным bias может отправлять посторонний запрос в router training. Улучшение `router_training` можно признать успехом именно в этой группе, даже при ухудшении OOD; такое сочетание запрещает универсальный вывод «исправление готово к интеграции». Число updates>0 само по себе успехом не считается.

Для обычного mixed-domain TRAIN это пока гипотеза, а не результат. Дальнейшая balanced-batch проверка, иные first labels и nonzero-small-margin примеры требуют отдельной карточки после этой пары, без автоматического расширения бюджета.

## Куда переносить только после результатов и решения пользователя

Оригинальный target — `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs`, метод `Train` L65–79: criterion и выбор конкурирующего класса. Не менять `DecideOrDefer` ради красивой метрики. До production переноса отдельно подтвердить mixed-class качество, OOD и приемлемую стоимость, затем согласовать downstream gate/consumer обычным процессом. Текущий источник называет модель `averaged-token-perceptron-v1`, но этот опыт не добавляет усреднение весов и не переименовывает известные вопросы модели в новую гипотезу.

## Точный небольшой запуск

**Статус поставленного scaffold: `NOT_BUILT / NOT_RUN`; причина `SDK_UNAVAILABLE`.** В сессии аудита `command -v dotnet` завершилась с кодом 1; стандартные каталоги SDK отсутствуют. Есть готовый исходный host, полный список зависимостей, фиксированные данные, команды и статическая проверка путей/хэшей. Компиляция и результаты обучения не заявлены. Для исполнения нужен .NET 9 SDK с локальными reference packs и Python 3; внешних NuGet-пакетов нет.

Пакет запускается прямо внутри planning: `experiments/wiki-margin/scaffold/`. Под ним сохранены принятые пути `35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/` для копий и `experiments-v2/RP01-MICRO/WIKI-MARGIN-01/` для host. Ни подготовка, ни запуск не записывают файлы в core. При будущей одобренной интеграции это те же относительные пути отдельного experiment checkout, а не изменения `42.ProjectWikiDomain`.

В `scaffold/experiments-v2/RP01-MICRO/WIKI-MARGIN-01/WikiMarginLearning.csproj` ровно четыре `Compile`:

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
task_scaffold="$task_package/experiments/wiki-margin/scaffold"
task_host="$task_scaffold/experiments-v2/RP01-MICRO/WIKI-MARGIN-01"
test -f "$task_host/WikiMarginLearning.csproj"
if test -e "$task_host/bin" || test -e "$task_host/obj"
then
    echo 'EXISTING_BUILD_CONFLICT: preserve existing bin/obj; do not rebuild.' >&2
    exit 1
fi
dotnet --version
(cd "$task_scaffold" && sha256sum -c "35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/checksums.sha256")
dotnet restore "$task_host/WikiMarginLearning.csproj" --configfile "$task_host/NuGet.Config"
dotnet build "$task_host/WikiMarginLearning.csproj" --configuration Release --no-restore
```

Это один самостоятельный csproj с четырьмя C# файлами. Нельзя заменять его сборкой solution, проектом `42.ProjectWikiDomain`, `35.ExperimentsV2.csproj`, обычным `dotnet test` или pipeline всех экспериментов. Preflight останавливается с `EXISTING_BUILD_CONFLICT`, если `bin`/`obj` уже существуют: предыдущие build artifacts сохраняются, их нельзя удалять или автоматически перестраивать. Если нет SDK/reference packs, либо сборка не прошла, статус `NOT_BUILT`; runtime-данные не создавать. Исправление только технической ошибки scaffold допустимо отдельным append-only уточнением с новым checksum receipt; смена A/B алгоритма, fixture или бюджета — новая карточка, а не скрытая правка результата.

### Выполнение двух arms и сравнение

После успешной сборки, в той же shell-сессии:

```bash
dotnet "$task_host/bin/Release/net9.0/WikiMarginLearning.dll" --arm A --output "$task_package/results/WIKI-MARGIN-01/A"
dotnet "$task_host/bin/Release/net9.0/WikiMarginLearning.dll" --arm B --output "$task_package/results/WIKI-MARGIN-01/B"
python3 "$task_scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/compare.py" "$task_package/results/WIKI-MARGIN-01/A/report.json" "$task_package/results/WIKI-MARGIN-01/B/report.json" "$task_package/results/WIKI-MARGIN-01/comparison.json"
```

Выходы строго в новом planning package, вне защищённого `core/docs/plans/replanning-possible-diffs`. Host отказывается перезаписывать существующий каталог arm; comparator отказывается перезаписывать `comparison.json`. При коллизии остановиться с `EXISTING_RESULT_CONFLICT` и ссылкой на существующие результаты; не удалять их, не выбирать другое имя автоматически и не менять условия опыта. Повтор разрешается только отдельной append-only карточкой с собственными literal путями и бюджетом.

**Фиксированный бюджет A+B:** 2 TRAIN и 4 EXAM; две непустые native `Train`, две пустые `Train` для native начального состояния, всего четыре вызова API `Train`; 72 предъявлений TRAIN строк (`18` native эпох на arm); 16 вызовов native evaluation reader. Эпохи — повторные предъявления внутри одного обучения, не отдельные эксперименты. Ручная генерация фикстуры не вызывает teacher/LLM. Полное обучение target unit, neural DPO, publication, cold restore и orchestration не запускаются.

### Что сохранять и как считать

Каждый arm пишет `before-snapshot.json`, `after-snapshot.json`, точную `fixture.json`, `source-provenance.json`, `report.json`. Фикстура SHA-256: `e17e8c84fde0b0aa2108c1778b8df21e6f4fec413bf4d88d48d0e07a8cae5303`. Before получается через native `Train([])`: private constructor не заменяется. After получается через native `Train(TRAIN)` из такого же zero initialization. EXAM никогда не передаётся в train, native reader получает только `Prompt`. Точные TRAIN/EXAM строки различаются; это небольшие лексически близкие парафразы, а не доказательство далёкого обобщения.

Native snapshot включает служебные gate-поля с нулевым `TrainingRowCount`, потому что gate не прикрепляется; это не счётчик реальных предъявлений. `observedTrainingRows`, `rowPresentations` и `modelUpdateCount` измеряет host, а служебное native значение сохраняется отдельно с пояснением. Ни `GatePassed`, ни `PublishAllowed`, ни `SemanticQualityClaimed` не выставляются. Проверка модели штатным `RouteForEvaluation` — тот же reader, который вызывается внутри `TrainingRunner` L43–53; обычный `Route` с production gate не подменяется и не используется для заявления публикации.

Основной полезный показатель — доля **Selected с верными `RouteKind` и `TargetUnitId`**. Отдельно выводятся wrong-selected, defer, строки/группы доменов и все before→after изменения. Сравнение A/B — `(B_after−B_before)−(A_after−A_before)`. Совпадение одного `RouteKind` при `NeedsSecondPass` остаётся диагностикой: native defer возвращает `NotProjectWiki`, поэтому такое совпадение само по себе не является успешным выбором. `Confidence`/top-two margin — диагностика; существующий confidence clamp не чинится этим опытом.

Ресурсы: wall-time, process CPU, allocations текущего потока вокруг непустого `Train`, heap before/after, число параметров, размер snapshot. Comparator пишет отдельный `resourceDeltas` с A/B, B−A и направлением полезного изменения, без общего PASS; `deferredRate` помечен conditional diagnostic и не получает автоматический useful-success. `PeakWorkingSet` честно подписан как пик **всего процесса**, не как пик фазы обучения. Один очень короткий процесс на arm имеет JIT/таймерный шум; результат является наблюдением этой пары, а не доказательством устойчивого ускорения. Счётчик обновлений и изменение хэша сами по себе не полезный результат: нужен эффект на выходах или полезном ресурсе. Без последующей санкционированной карточки автоматические дополнительные повторы запрещены.

**Правило успеха:** любое измеренное улучшение полезного показателя считается успехом **по этому показателю и этому домену**; рост точности не обязателен. Рядом обязательно показать изменения всех остальных показателей, включая ухудшения. Снижение defer считается полезным вместе с качеством выбранных решений; замена безопасных отказов неверными решениями не объявляется улучшением качества. Возможен одновременно успех на одной группе и ухудшение на другой; это не скрывать сводным pass/fail.

### Конфликты и граница переноса

Старые карточки и защищённая папка остаются неизменными. Этот пакет дополняет известные проверки `P33`/`P93`/`P118` и recovery49, не переписывает их критерии. Source-copy сравнение не является исправлением production, результат автоматически ничего не публикует. Решение о дальнейшем переносе пользователь принимает по `comparison.json` и строкам EXAM. До решения нельзя чинить production тесты, менять rollout/gate, расширять все данные или запускать всю цепочку. Порядок: learning blockers → конфликтующие карточки → дополнительные изменения → периферийные проверки.
