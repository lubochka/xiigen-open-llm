# Новый узкий пробел: KNN теряет номер аргумента за узлом ArgumentSyntax

**19.10.2026. Область:** C# Structural KNN/E09. **Состояние:** формула и путь исходного кода прочитаны; историческая содержательная паритетность проверена; выполнена только арифметическая модель на явно заданных векторах. Native Roslyn, native StructuralKnnPredictor, обычный ответ продукта и влияние на RP01 **не запускались и не измерялись**.

## Что обнаружено

У двух аргументов обычного вызова `Pair(a, b)` имеются разные позиции в списке аргументов. Но StructuralVectorExtractor вычисляет положение выражения только среди детей его **непосредственного** родителя. Родителем выражения `a` является собственный `ArgumentSyntax`, родителем выражения `b` — другой `ArgumentSyntax`. В обычном позиционном аргументе каждый такой родитель содержит одно выражение. Поэтому ожидается `PositionInParent=0` у обоих, а `SiblingCount=1`.

Остальная записываемая цепочка типов тоже одинакова: `IdentifierName → Argument → ArgumentList → InvocationExpression`. Обоим выражениям код даёт роль6. Отдельного номера аргумента, позиции родителя внутри списка или последовательности позиций предков в векторе нет. Если native extraction подтвердит указанную форму, оба входа имеют нулевое расстояние, хотя следующий узел реального обхода может различаться. Это конкретная проверяемая потеря информации **до** KNN-голосования. [Поля и distance][fields], [extract][extract], [роль ArgumentSyntax][role].

Например, в теле `Pair(a, b); return 0;` следующий узел после первого аргументного `IdentifierName a` должен быть узлом следующего `Argument`; после последнего `IdentifierName b` — `ReturnStatement`. Это именно исходная задача StructuralKnn — предсказать следующий AST-kind, а не новая произвольная метка «левый/правый аргумент». Реальную последовательность `DescendantNodes()`, имена kinds и `NodeTypeId` надо сначала записать в native probe. Здесь Roslyn не установлен, и ожидаемая форма не выдаётся за уже наблюдённое AST. [TrainFromSource][train], [E09 train/predict][e09].

**Главное следствие:** если два входа полностью равны по всем используемым старым полям, повторение уроков не научит этот predictor давать им разные ответы. Расширение byte-поля до int тоже не поможет: записаны два действительных нуля, переполнения нет.

## Почему это новое относительно прочитанного реестра

Полнотекстовый поиск проведён по 492 Markdown возможных различий на planning SHA `790bcf3e396411fb8d5a29504cdadc5dd7ef32fa` и 242 Markdown защищённого core-каталога на SHA `eb05219c27663835ce927c7bf97cf1274c07cddb`. Это поиск плюс чтение ближайших владельцев, не 734 независимых рецензии и не чтение всего исходного кода продукта.

| Ближайший прежний владелец | Что уже известно | Чем отличается проверка |
|---|---|---|
| N35 E09 SiblingCount / core P26.4 | Поле записано, но отсутствует в distance | Здесь у обоих выражений один sibling; добавление его distance не различит аргументы |
| N53 NEW-POSITION / mini14 position | Byte-position сливает прямые позиции255/256 | Здесь обычный вызов из двух аргументов, обе прямые позиции0; widening не восстанавливает позицию родителя |
| P16 order/roles / generic KnnLogicalUnit | Частотный BPE не хранит порядок слов | Здесь другой encoder — Roslyn StructuralVector, иной точный owner и конкретный ArgumentSyntax-wrapper |
| Core38 / N26 E09 transfer | Перенос label codec, tie policy и opt-in consumer | Codec/tie/feature flag в нашем A/B одинаковы; добавляется только информация о позиции аргумента |

В прежних файлах не найдено точного поля/условия `ArgumentOrdinal`, `ArgumentIndex`, `ArgumentSyntax`, ancestor-position или контрпримера с двумя одинаковыми direct-child positions у выражений разных аргументов. Общая тема «теряется порядок» давно известна. Наше заявление о новизне относится к **конкретному source seam и отдельному однофакторному опыту**, а не к открытию всей темы порядка. Non-Markdown receipts и более поздние чужие публикации не объявляются исчерпывающе проверенными. [N35][prior35], [N53][prior53], [P26][prior26], [mini14][mini14], [core38][core38].

## Доказательства кода и исторический предел

### 1. Что записывается сейчас

В [StructuralKnn.cs, строки52–59][extract] непосредственная позиция вычисляется так:

```csharp
var siblings = node.Parent.ChildNodes().ToList();
var index = siblings.IndexOf(node);
positionInParent = (byte)Math.Min(index >= 0 ? index : 0, 255);
```

**Сейчас:** сохраняется ordinal выражения внутри ArgumentSyntax. **Почему недостаточно для данного входа:** ordinal самого ArgumentSyntax внутри ArgumentList не переносится. **Кандидат исправления:** внутри того же extractor добавить только `ArgumentOrdinal`, вычисляемый для `node.Parent is ArgumentSyntax argument` с `argument.Parent is ArgumentListSyntax list` через `list.Arguments.IndexOf(argument)`; для остальных узлов значение−1. Непосредственные position/siblings, scope, depth, role и kind codec сохраняются.

В [строке200][role] `ArgumentSyntax => 6`. **Сейчас:** роль одинаковая для всех выражений аргументов. **Почему не надо менять её вместе с опытом:** новая шкала ролей стала бы вторым вмешательством. В A и B остаётся прежняя роль6.

В [distance, строки27–35][fields] нет ordinal предка. **Кандидат B:** одно дополнительное слагаемое `ArgumentOrdinal != other.ArgumentOrdinal ? 0.3 : 0`. Коэффициент0.3 заранее выбран равным весу уже существующей позиции; он не оптимизирован по экзамену. Этот кандидат проверяет полезность одной новой координаты, не утверждает, что0.3 — универсально лучший вес.

### 2. Кто реально обучается и получает результат

[StructuralKnnPredictor.Train, строки117–120][train] заполняет `_training` предоставленными парами. [Predict, строки145–161][predict] ранжирует их по distance и голосует по next-token. Это memory learning внутри существующего predictor; optimizer/LoRA здесь не участвуют.

Реальный API более высокого слоя сохранён: [SemanticCompilerFacade.TrainStructuralKnn, строки206–225][facadetrain] строит пары из C# исходников и обучает predictor при ≥10 парах. [Compile, строки155–172][facadecompile] помещает прогноз в `PredictedNextTokens`; [PredictStructuralNextTokens, строки299–310][facadepredict] требует одновременно CSharp, `UseStructuralKnn=true` и ≥10 обучающих пар. По умолчанию флаг выключен. [Options][options].

Этим доказан путь до поля результата Facade. Не доказан обычный RP01 reader, превращающий это поле в законченный ответ. Ранний core38 находил явные внешние включения только в тестах; этот прошлый ограниченный search не выдаётся за новый exhaustive поиск всех hosts. Наше прямое чтение [Tests/E09E10FeatureFlagTests.cs][tests] также показывает тесты наполнения/непустого выхода и повторяемости; они не измеряют способность различить два аргумента.

**Условия практической достижимости:** C# путь действительно вызывает StructuralKnn; память содержит контрастные next-kind уроки; запрос достигает вычисления выражений внутри ArgumentSyntax; результат predictor влияет на проверяемый выход. Если обычный learner RP01 не вызывает этот компонент, находка не является причиной его нейронного ухудшения.

### 3. Успешная соседняя ветка не потеряла этот код

На зафиксированном `language_translate=f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` прочитан тот же StructuralKnn. [Исторический код][langstruct].

| Объект | Git blob SHA |
|---|---|
| Current StructuralKnn | `b3aa5d376be3f93423997674dc015cea7e9fbf58` |
| language_translate StructuralKnn | `f5132ce4800082797ca7d0197373cbdbd82fdcae` |

Прямой diff показывает единственное отличие: пять строк copyright перед namespace. Формула, extractor и predictor содержательно совпадают. Следовательно, это **унаследованный предел представления**, а не новая доказанная регрессия между этими ветками. Ухудшение возможно при изменении доли вызовов/аргументных контекстов или подключённого маршрута; такая смена условий пока не установлена.

Исторический [E09 receipt от01.05.2026][receipt] сохранён: baseline0.2413 → enhanced0.3507, `passed=true`. Мы не переписываем это положительное наблюдение. Однако прочитанный E09 сравнивает весь baseline validation с первыми500 KNN validation и строит local kindMap на каждый файл. Поэтому45.33% нельзя приписать одному восстановленному признаку. Следует сохранить успешный механизм, воспроизвести совпадающий с ним экзамен и только затем проверять новую координату. [E09 код][e09].

## Маленький причинный эксперимент: 2 урока, 4 новых вопроса

**Цель:** выяснить, помогает ли сохранение номера аргумента существующей обученной памяти предсказывать собственный следующий AST-kind.

**Получатель:** настоящий `StructuralKnnPredictor.Train/Predict`, сначала прямой компонент. Кандидат B живёт в изолированной копии компонента для опыта. Продуктовый код и защищённые документы этой работой не изменяются. KNN остаётся внутренним компонентом; нового самостоятельного уровня trainable-архитектуры не вводится.

### T0. Установить реальную форму входа

Один TRAIN исходник:

```csharp
class Calls
{
    void Pair(int first, int second) { }
    int Run(int a, int b)
    {
        Pair(a, b);
        return 0;
    }
}
```

Разобрать именно native Roslyn. Выбрать два `IdentifierNameSyntax`, являющихся `ArgumentSyntax.Expression` внутри вызова Pair. Для каждого записать Span, RawKind, три родителя, `ChildNodes` непосредственного родителя, old vector, actual ordinal аргумента и **реального следующего** узла в том же массиве `DescendantNodes`.

Условия валидности T0: два вектора A совпадают по всем полям distance, targets различаются после прежнего `NodeTypeId`, оба AST не содержат синтаксических ошибок. Если любое условие не выполнено, исходный контрпример не подтверждён; не подменять native данные удобными искусственными векторами.

### T1. Один фактор и общая учебная память

A — старый vector/distance. B — только дополнительный `ArgumentOrdinal` и его фиксированный вес0.3. Новая координата потребует re-extraction TRAIN и EXAM согласованно. Остальные поля, parser, `NodeTypeId`, список двух target labels, k=1, stable ordering и голосование абсолютно одинаковы.

`k=1` задаётся существующим constructor в обоих плечах, чтобы минимальный набор из двух примеров проверял именно различимость. При двух учебных строках default k=5 возьмёт обе: голоса разных labels будут 1:1, и победит меньший label независимо от расстояния. Поэтому на таком наборе новый ordinal сам по себе не даст контраста при k=5. Здесь проверяется различимость при одинаковом k=1 в обоих плечах; улучшение штатного k=5 потребует отдельной панели настоящих учебных пар. Это не входит в минимальный бюджет.

До обучения вызвать Predict на тех же четырёх EXAM и записать отсутствие памяти/код0. Не считать отсутствие памяти правильным обученным ответом даже при случайном совпадении с ID. Затем выполнить по одному Train A/B с теми же двумя уроками и повторить ровно те же EXAM. Учится только KNN-память: **2 Train calls по2 rows, 16 Predict calls до/после для A/B, 0 neural optimizer updates**. Повторённые измерения не считать дополнительными уроками.

### Четыре EXAM, не использованные для настройки

Взять два новых исходника с теми же объявлениями необходимых методов, изменив **реальную структуру**, не только имена:

1. Весь фрагмент `Pair(x, y); return 1;` находится в дополнительном самостоятельном block внутри Run. Два вопроса: первый и последний аргумент.
2. Внутри Run: `if (x >= 0) { Pair(x, y); return 2; } return 3;`. Два вопроса: первый и последний аргумент внутри ветви.

Gold каждого вопроса берётся из независимо зафиксированного native successor: ожидаемая последовательность классов `Argument, ReturnStatement, Argument, ReturnStatement`. Настройка веса/k после просмотра этих четырёх результатов запрещена. Если actual AST/codec показывает иной successor, исправить предварительный материал до train и запечатать реальные labels; не исправлять gold после сравнения с ответом learner.

До обучения условие T0 проверить также в каждой паре аргументов EXAM: старые поля distance равны друг другу, реальные successor IDs различаются. Это только extraction и фиксация gold, без дополнительных Predict. Если условие не выполнено — NO_CONTRAST. Глубины и scope изменятся относительно TRAIN. В каждом плече надо использовать их **реальные значения**. Асимметричные случайные изменения других полей могут объяснить отличие и должны быть записаны. EXAM нельзя передавать в Train или использовать в автоматическом retraining через Facade.

### Что измерить и как прочитать результат

Основной результат — четыре собственных predicted next-kind ответа до/после, число исправленных и испорченных EXAM. Дополнительно: обе distance до двух учебных записей, выбранный индекс/label, число memory rows, extraction/predict latency, peak RAM. Не заменять ответ расстоянием или `TrainingSize>0`.

Если T0 подтвердится, A обязан давать один ответ двум неразличимым аргументам в каждом EXAM; на сбалансированной паре это не позволяет решить оба. B **может** устранить эту конкретную ошибку; ожидаемый4/4 ещё не измерен. Вывод допустим как локальное улучшение next-kind memory task на этой панели, а не как восстановление генерации C# или успех всех доменов.

Необязательные контроли, выбираемые отдельно от основного бюджета:

- обратный порядок двух TRAIN rows: у A может поменяться победитель tie; B должен сохранить два разных правильных выбора;
- два неаргументных контекста, для которых `ArgumentOrdinal=-1` в обоих: distance должна совпасть со старой;
- контроль неразличимых или неизменяющих target позиций: новый ordinal не должен автоматически интерпретироваться как новый закон;
- cold reproduction: записать raw TRAIN, parser/codec/schema version, vectors и k; новая инстанция восстанавливает ту же memory через Train. Это **rebuild из сохранённых пар**, не существующий native checkpoint export/import. У данного predictor такого публичного persist API в прочитанном файле нет.

Если отдельно выбрана перестановка TRAIN, её бюджет — ещё 2 Train и 8 Predict. Отдельный rebuild двух плеч требует ещё 2 Train и 8 Predict. Неаргументные контроли distance не вызывают Train/Predict. Эти продолжения не считаются частью основного результата на 16 Predict.

**Фальсификаторы:** native старые vectors не равны; targets совпали; B не меняет выбор; прирост есть только на TRAIN; новые depths уничтожают результат; reverse-order контроль ломает B; компонент не участвует в ordinary output. Нулевой прирост — допустимый ответ опыта.

### Только затем — Facade/продукт

Если нужно проверить именно Facade, использовать реальные source snippets, дающие ≥10 **фактических соседних пар**, и включить StructuralKnn в обоих плечах. Не размножать два урока искусственно до10. Raw/E10 входы обоих плеч должны совпадать; код выше без comments/строковых literal позволяет начать без изменения preprocessor. Проверить собственные `PredictedNextTokens` на нужных узлах; затем установить reader результата и отдельно измерить законченную C# подсказку. Нет reader — предел результата остаётся компонентным.

Широкая CI-матрица, Windows runs и изменение LoRA для этого вопроса не нужны. В текущем контейнере `command -v dotnet` не нашёл executable в PATH; native опыт здесь только спроектирован. Это наблюдение не доказывает отсутствия всех возможных runtime вне PATH и не блокирует исследование.

## Domain scope и дополнительная граница ref/out/in

Основной эксперимент относится к **C# AST/next-kind**, включая источник кода для документации и Design→Code только при найденном настоящем caller. SQL/TypeScript могут иметь аналогичные wrappers, но должны иметь собственный parser, ordinal carrier и gold. Перевод EN↔RU/HE требует признаков языкового порядка; Roslyn ArgumentOrdinal ему не назначается.

Отдельная **условная граница, не второй заявленный новый finding:** `ref/out/in` живут в токенах аргумента, а прочитанный StructuralVector не хранит `RefKindKeyword`. Даже добавление ordinal не доказывает различение этих семантик. Старый экзамен next-AST-kind может вообще дать одинаковый target для таких вариантов. Если требуемая задача — объяснить передачу по ссылке, нужен отдельный semantic output exam; нельзя выдать прирост AST-kind за освоение ref/out. Этот вопрос не включён в A/B выше.

## Что реально выполнено

В [argument_ordinal_probe.py](evidence/argument_ordinal_probe.py) воспроизведена только арифметика L27–35 и stable nearest при k=1 на **условных символических векторах**. Результат сохранён в [argument_ordinal_probe_result.json](evidence/argument_ordinal_probe_result.json): расстояние одинаковых старых векторов0, после добавления ordinal0.3; reverse-memory и no-argument controls включены. В JSON явно стоят `native_roslyn_run=false`, `native_knn_run=false`, `native_quality_metrics=null`. Иллюстративные depth values не выдаются за actual AST.

Это служит проверкой, что предложенный single feature способен различить входы **при выполнении условия T0**. Это не native learning result50%→100%, не восстановление модели и не доказательство причины неудачи RP01.

Проверенные непосредственно source SHA/line spans и файл к файлу parity находятся в [knn_source_manifest.json](evidence/knn_source_manifest.json). Роли данных для будущего DPO неизменны: **CODEX на astra 6 ultracode выполняет роль человека при выборе; внешняя открытая китайская модель создаёт выбранную и отклонённую стороны; отдельный CODEX на astra 6 ultracode оценивает точную пару; собственные ответы ученика проверяет отдельный examiner**. Наше текущее исследование не объявляется подтверждённым ultracode-запуском и не создавало модельных DPO-сторон.

### Предел обзора

Проверены формула представления, извлечение позиции/роли, native consumer до Facade output, ветка language_translate, положительный архив и ближайшие карты экспериментального покрытия. Широкие общие темы BPE order drift, E13 ExpressionBody, SiblingCount, duplicate-ID и KnnSkillFilter scope сознательно не переобъявлены новыми после проверки прежних владельцев. Следующее более широкое исследование могло бы обнаружить другие wrapper/token losses и actual consumer state/persistence; нынешний отчёт не утверждает полного закрытия всей обучаемой архитектуры.

[fields]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/12.SemanticCompiler/Prediction/StructuralKnn.cs#L13-L35
[extract]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/12.SemanticCompiler/Prediction/StructuralKnn.cs#L42-L95
[role]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/12.SemanticCompiler/Prediction/StructuralKnn.cs#L193-L209
[train]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/12.SemanticCompiler/Prediction/StructuralKnn.cs#L117-L141
[predict]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/12.SemanticCompiler/Prediction/StructuralKnn.cs#L145-L167
[e09]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L26-L118
[facadetrain]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/12.SemanticCompiler/SemanticCompilerFacade.cs#L206-L225
[facadecompile]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/12.SemanticCompiler/SemanticCompilerFacade.cs#L155-L172
[facadepredict]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/12.SemanticCompiler/SemanticCompilerFacade.cs#L299-L310
[options]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/12.SemanticCompiler/SemanticCompilerOptions.cs#L14-L15
[tests]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/Tests/E09E10FeatureFlagTests.cs
[langstruct]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/StructuralKnn.cs
[receipt]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json
[prior35]: https://github.com/lubochka/xiigen-planning/blob/790bcf3e396411fb8d5a29504cdadc5dd7ef32fa/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md
[prior53]: https://github.com/lubochka/xiigen-planning/blob/790bcf3e396411fb8d5a29504cdadc5dd7ef32fa/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md
[prior26]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/cards/P26-structural-features.md
[mini14]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/14-additions-orchestration-sparse-knn.md
[core38]: https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/38-e09-transfer-labels-and-opt-in-consumer.md
