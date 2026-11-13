# Вопросы 2 и 6 — боевая ли это дорога и не искажает ли оснастка измерение

## Вопрос 2. Вывод

Путь проверки — производственный. Фасад создаётся так, как его создаёт боевой потребитель (конструктор без аргументов), предсказатель фасад заводит сам, обучение идёт обычным входом `TrainMethodKnn`, ответ читается из результата обычной `Compile`. Обходного пути нет: внедрённого предсказателя нет, прямого вызова `Predict` на боевом ответе нет, обходного публичного метода предсказания нет. Единственное отражение (`_methodKnn`) используется только для диагностики обученности, а не для получения ответа.

## Как фасад получает свой предсказатель

Доказательство: `sed -n 28,50p 12.SemanticCompiler/SemanticCompilerFacade.cs` -> конструктор принимает необязательный `MethodKnnPredictor? methodKnnPredictor = null` (строка 39) и присваивает `_methodKnn = methodKnnPredictor ?? new MethodKnnPredictor(k: 5)` (строка 46). Файл проверок вызывает `new SemanticCompilerFacade()` (строка 140) — все аргументы по умолчанию, значит предсказатель фасадный, пять соседей. Параметры по умолчанию: `_options = options ?? SemanticCompilerOptions.Default` (строка 44); `UseMethodKnn { get; init; } = true` (`SemanticCompilerOptions.cs`, строка 18).

## Какой метод исходника попадает в запрос

Доказательство: `sed -n 365,380p SemanticCompilerFacade.cs` ->
```text
368  private string? PredictNextMethodType(string source, StackLabel stack)
370      if (!_options.UseMethodKnn || stack != StackLabel.CSharp || _methodKnn.TrainingSize < 4) return null;
373      var extracted = MethodExtractor.Extract(source);
374      if (!extracted.IsSuccess || extracted.Value!.Count == 0) return null;
377      return _methodKnn.Predict(extracted.Value[^1]);
```
В запрос попадает ПОСЛЕДНИЙ извлечённый метод. В достижимой форме запроса методов два: `Carrier` (первый) и измеряемый `Q1..Q4` (последний) — проверка утверждает число методов, равное двум (строка 261 файла проверок), и сравнивает признаки именно последнего (`extracted.Value![^1].KindBag`, строка 194).

## Какие условия должны выполниться, чтобы ответ не был пустым (по порядку `Compile`, строки 118–180 фасада)

1. Исходник не пуст (строка 120–122).
2. Определитель стека вернул успех и стек — C# (строки 125–128, затем 370). Определитель в обычном фасаде не обучен и решает по фиксированному списку примет: `Routing/StackDetector.cs`, строка 15 `Markers`, строки 19–20 для C# содержат `"sealed"`; строка 56 `PredictByMarkers`. В панельных исходниках без `sealed` примет C# нет — это вторая находка исполнителя, подтверждена по коду. Доказательство: `grep -n "sealed\|PredictByMarkers" Routing/StackDetector.cs` -> строки 20, 56, 74.
3. Нормализация: при `UseRegexPreprocessor` и стеке C# `source = RegexPreprocessor.Normalize(source)` (строки 132–133) — удаляет XML-документацию, блочные и строчные комментарии, строковые литералы и лишние пустые строки (`Compression/RegexPreprocessor.cs`, строки 16–30). ВАЖНО: дальше по конвейеру идёт уже нормализованный текст, включая `PredictNextMethodType`.
4. `NormalizeSource`, `CollapseToOps` — успех (строки 135–143).
5. Страж признаков `LogicExtractor.Extract(source)` — успех (строки 145–148); отказ `No predicates extracted` стоит в `Compression/LogicExtractor.cs`, строка 85; порог четырёх признаков дерева — `Compression/SemanticTokenizerPipeline.cs`, строка 62 `return result.Count >= 4 ? result : Level2Tokens(csharpSource)`. Это первая находка исполнителя, подтверждена по коду.
6. `_decomposer.Split` — успех (строки 150–153).
7. Внутри `PredictNextMethodType`: признак включён, стек C#, `TrainingSize >= 4`, извлечение методов успешно и непусто.

Доказательство: `sed -n 96,180p SemanticCompilerFacade.cs`, порядок вызовов — как перечислено.

## Обучение идёт обычным входом

`TrainMethodKnn` (строки 183–198 фасада): нумерует файлы по позиции в списке (`for (short fileId = 0; …) MethodExtractor.Extract(csharpSources[fileId], fileId)`), собирает все методы, и обучает `_methodKnn.Train(allMethods)` только если `CountEligiblePairs(allMethods) >= 4`. На панели из четырёх файлов по два метода — ровно четыре смежные пары, порог проходится. Стража признаков и определителя стека обучение не проходит (подтверждено: в теле `TrainMethodKnn` их вызовов нет).

## Отражение — не обход

`FacadeTrainedPairs` (строки 168–181 файла проверок) читает закрытое поле `_methodKnn` и возвращает `TrainingSize`. Это используется только в утверждении `Assert.Equal(4, …)` (строка 249) как диагностика «пустой ответ ≠ необученность». Ответ во всех трёх случаях берётся из `FacadeAnswer` -> `facade.Compile(source)` (строка 154). Приём тот же, что в существующей проверке `Tests/E13E14FeatureFlagTests.cs` (строка 42: `PrivateField<MethodKnnPredictor>(facade, "_methodKnn")`; строка 233: `GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)`). Доказательство: `grep -n "_methodKnn\|GetField" Tests/E13E14FeatureFlagTests.cs`.

## Вопрос 6. Вывод

Измеряемый метод в обеих формах даёт один и тот же набор признаков, и метод-носитель не меняет расстояний до обученных пар. Это следует из устройства извлекателя, а не только из утверждения проверки. Есть одна оговорка о том, НА ЧЁМ считается утверждение равенства (ниже).

## По коду извлекателя (`12.SemanticCompiler/Prediction/MethodKnn.cs`)

- `MethodExtractor.Extract` (строки 41–63) перебирает `root.DescendantNodes().OfType<MethodDeclarationSyntax>()` и для КАЖДОГО объявления метода считает `KindBag: ExtractKindBag(method)` отдельно.
- `ExtractKindBag` (строки 160–187) берёт узлы ТОЛЬКО из тела данного метода: `method.Body.DescendantNodes()` (строка 169) либо `method.ExpressionBody.Expression.DescendantNodesAndSelf()` (строка 172). Узлы соседнего метода `Carrier`, модификатор класса `sealed`, имя класса — в это поддерево не входят.
- Следовательно набор признаков `Q1` в форме `class Q{void Q1(int n){if(n<0)return;}}` и в форме `sealed class Q{void Carrier(string probe){if (probe == null) { return; }} void Q1(int n){if(n<0)return;}}` одинаков: `{If, LessThan, NumericLiteral, Ret}` (вывод по `AbbreviateKind`, строки 190–216: `IfStatement`->`If`, `LessThanExpression`->суффикс `Expression` отсечён -> `LessThan`, `NumericLiteralExpression`->`NumericLiteral`, `ReturnStatement`->`Ret`, `IdentifierName`/`Parameter`-подобные -> `?` и отброшены). Я это вывел по коду; Roslyn-разбор сам не запускал.
- Расстояние `JaccardDistance(query.KindBag, pair.Current.KindBag)` (строка 341) зависит только от набора запроса и набора обученной пары. Обученные пары от формы запроса не зависят: обучение идёт по панели, а не по запросам. Значит добавленный носитель не меняет ни одного расстояния.
- Носитель влияет только на (а) стража признаков (его `== null` даёт признак; это объяснение исполнителя, по коду стража я его не проверял — ограничение) и (б) на то, что методов стало два, а запрос — последний. Оба влияния — на достижимость, не на измерение.

## Оговорка (малая, не дефект чувствительности)

Утверждение равенства наборов (строка 267 файла проверок) считается `MethodExtractor.Extract(exam.Reachable)` на СЫРОМ тексте, а фасад подаёт в извлекатель `RegexPreprocessor.Normalize(source)` (строка 133 фасада). Для данных входов нормализация ничего не меняет: в них нет комментариев, XML-документации и строковых литералов (параметр `string probe` — это тип, не литерал), а наборы признаков нечувствительны к пробелам. Доказательство: регулярные выражения нормализатора — только `///`, `/* */`, `//`, строковые литералы, лишние пустые строки (`RegexPreprocessor.cs`, строки 16–30). Но строго говоря, равенство «то, что видит фасад» и «то, что видит утверждение» доказано рассуждением, а не самим утверждением. Ограничение: если когда-нибудь запрос будет содержать строковый литерал внутри тела измеряемого метода, нормализация изменит его набор признаков, и это утверждение этого не заметит.

## Отпечаток извлекателя (дописываю по обещанию из `00-pre-work-state.md`)

Извлекатель живёт в том же файле `12.SemanticCompiler/Prediction/MethodKnn.cs` (класс `MethodExtractor`), отпечаток `2bb6349423309ad3845e0e3131f120d7724f1e5e16f782c6968067744525a69a` — уже записан в `00-pre-work-state.md`. Отдельного файла извлекателя нет.
