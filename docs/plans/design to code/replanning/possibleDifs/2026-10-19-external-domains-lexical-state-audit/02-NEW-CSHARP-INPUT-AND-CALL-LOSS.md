# Три новых разрыва перед обучаемой памятью C#: границы строк, интерполяция и условный вызов

Дата: 19 октября 2026. Область: C# → AST → признаки и наблюдения → E09/E13/E14 KNN либо E02 completion memory.

## Что найдено

До выбора соседей исходный C# уже может потерять учебное содержание. Здесь три разных механизма:

1. Препроцессор E10 принимает // внутри строкового литерала за начало комментария и удаляет оставшуюся часть строки вместе с настоящим кодом.
2. Даже когда C# остаётся синтаксически правильным, E10 превращает интерполяцию с выражением user.Length в пустую интерполированную строку. KNN учился на исходном AST, а обычный Compile передаёт ему AST без этого выражения.
3. Память вызовов E02 пропускает cache?.Add(...), хотя учитывает cache.Add(...). Безопасный условный вызов не становится наблюдением, и новые такие примеры не меняют рейтинг.

Это исходниково подтверждённые пробелы конкретного получателя признаков. Они **не доказывают**, что именно они вызвали плохой результат RP01. Между external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639 и codex/dpo-implementation@44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76 тела шести проверенных production-файлов одинаковы: добавлены только пять строк copyright. Сравнение записано в [source_parity.json](evidence/source_parity.json).

Blob SHA в этом JSON — metadata GitHub tree API. Равенство тел проверено по декодированному тексту источников после удаления заголовка; тождество SHA локальных копий с Git blob не утверждается.

Реальная новая регрессия при одинаковом коде могла бы появиться вследствие другого корпуса, клиентского пути, флагов или восстановленного состояния. Таких парных runtime receipts здесь нет.

## Сначала восстановление external_domains: контроль R0

В первую очередь повторить фактический успешный путь старой ветки с прежним набором источников и флагами. Для этой семьи кода внешний публичный путь проверен до конкретных методов:

TrainFileActionKnn(raw sources) строит признаки до E10. Compile(raw query) по умолчанию включает E10, затем вызывает предсказатели на изменённой строке. Аналогично TrainMethodKnn и TrainStructuralKnn читают raw sources. E09 и E14 требуют явного включения; UseStructuralKnn=false и UseFileActionKnn=false — дефолты обеих веток. Наличие сохранённого predictor само по себе ещё не показывает, что он участвовал в успешном ответе. [Current train path](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerFacade.cs#L178-L225), [current inference path](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerFacade.cs#L122-L172), [flags](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerOptions.cs#L8-L32).

**R0 — восстановление поведения:** две чистые копии checkout, одинаковые raw TRAIN и запросы, одинаковые flags, тот же extractor и способ восстановления. Сравнить промежуточные normalized source, AST kinds, vectors, prediction и конечный CompilationResult. Если именно эти одинаковые компоненты дают одинаковый результат, искать различие в вызывающем пути и восстановленной памяти. Изменение E10 ниже — отдельное улучшение унаследованного механизма; оно не является откатом к отличавшемуся старому коду.

## Новое KNN-RX1: строка с URL удаляет последующий код до AST

### Сейчас

В RegexPreprocessor.Normalize сначала работают XML/block/line-comment regex, и только затем строковый regex. Ключевые строки самого метода:

~~~csharp
s = LineComments.Replace(s, "");
s = StringLiterals.Replace(s, "\"\"");
~~~

Шаблон комментария не знает, находится ли // внутри C# литерала:

~~~csharp
class C {
  string F(string user) {
    var endpoint = "https://example.test"; return user.Length.ToString();
  }
}
~~~

Часть после https: исчезает до разбора Roslyn. Если весь пример записан одной строкой, исчезают и закрывающие скобки. Прямой источник: [current RegexPreprocessor L15–38](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Compression/RegexPreprocessor.cs#L15-L38); прежний код: [external_domains L10–33](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/Compression/RegexPreprocessor.cs#L10-L33).

### Почему это касается обучения

Compile подменяет переменную source результатом E10 **до** LogicExtractor, запоминания C# и E09/E13/E14. Историческая скорость разбора не обеспечивает сохранность условного запроса к обученной памяти. Строка с URL особенно уместна для C# HTTP-клиентов, сетевой конфигурации, HTML/JSON-содержимого в литералах. Для natural language и SQL этот C# preprocessing нельзя включать как универсальную меру.

Наблюдаемое здесь: Python-зеркало точной последовательности regex на ASCII fixture удалило весь хвост. Это не Roslyn-запуск, не обученный Unit и не измерение качества.

### Что менять в коротком опыте

Сохранить raw source как вход AST/KNN. Единственная переменная первой проверки — UseRegexPreprocessor=true/false при неизменных TRAIN, флагах предсказателя, k и порядке примеров. Отдельно проверить вариант разбора Roslyn один раз с удалением только comment trivia; границы строк и interpolation expression должны оставаться собственностью C# parser.

Конкретная экспериментальная точка: SemanticCompilerFacade.Compile, отдельно RegexPreprocessor.Normalize. Для будущего исправления предложен типизированный результат CSharpPreprocessingResult(RawSource, SourceForAst, Diagnostics, PreprocessorVersion) и передача одного и того же SourceForAst в training/query encoder. Эти поля — предложение ремонта, не существующий production API.

Проверка: URL-literal, настоящий line comment, обычный literal без //, block comment. Falsifier: actual .NET нормализация сохраняет хвост и соответствующие AST nodes; либо обнаружен обычный caller, который не применяет E10 и потому не несёт этого риска.

## Новое KNN-RX2: корректный синтаксис скрывает потерю interpolation AST

### Сейчас

Два разных исходника:

~~~csharp
return $"{user.Length}";
return $"";
~~~

становятся одной строкой после StringLiterals.Replace. У первого варианта исчезает реальное вычисление user.Length. Синтаксис второго C# допустим, поэтому одна проверка отсутствия syntax errors этого не заметит. [E10 replacement](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Compression/RegexPreprocessor.cs#L25-L38).

Это другой механизм, чем RX1: в RX1 неверна граница комментария и возникает повреждённый текст; в RX2 валидное преобразование формы стирает исполняемое выражение.

### До/после в конкретном KNN

TrainFileActionKnn вычисляет ActionLabelGenerator.FeatureVector(source) из raw source. Compile вызывает тот же featurizer после E10. В FeatureVector есть координата SimpleMemberAccessExpression; после очистки интерполяции её значение меняется. При исходном примере без иных обращений к членам меняется и deterministic E14 label function: raw содержит member access, normalized уже не содержит. Это доказывает потерю model-visible сигнала, а правильность самой gold label требует независимой оценки. [E14 source L21–38, L75–108](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Prediction/FileActionKnn.cs#L21-L108), [ordinary consumer](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerFacade.cs#L323-L332).

### Почему старый PASS этого не отвергает

Сохранённый E10 от 30 апреля: 3 файла, 0.7372→0.5194 ms, одинаковые 968 AST nodes, syntaxErrors=0. От 1 мая: 20 файлов, 10.5218→7.7883 ms, **5617→5590 AST nodes**, syntaxErrors=0. E10 считал время и уменьшение текста; код не сравнивал наборы semantic features и решения trained KNN. Уменьшение 27 nodes нельзя без разбора тех файлов приписать именно interpolation. Оно показывает лишь, что отсутствие syntax errors и сохранность всех узлов уже не были эквивалентны. [Receipt 30 апреля](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E10-REGEX-PREP/results/run-2026-04-30-001.json), [receipt 1 мая](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E10-REGEX-PREP/results/run-2026-05-01-001.json), [definition E10](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E10RegexPrep.cs#L19-L80).

### Маленький опыт

В native probe зафиксированы шесть raw TRAIN: три C# функции с user.Length внутри interpolation, три с обычным константным ответом. Два свежих facade получают тот же TRAIN и один новый query с другим именем аргумента. Меняется только E10 on/off; FileActionKnn включён в обоих. Отдельный constant-string query — отрицательный контроль. Записать AST nodes, cosine raw/normalized, PredictedFileAction.

Это проверяет достижимость изученного различия; для качества отдельно нужны C# задачи с independently approved target: nullability/flow-анализ, code trace, обработка URL. Нельзя использовать собственную ActionLabelGenerator.Label как независимого judge. Ремонт должен сохранять выражения интерполяции в AST, а обучение и serving должны использовать согласованную версию признаков. Индекс/память при смене признаков перестраиваются из сохранённых raw sources, вместе с новым PreprocessorVersion; прежняя память остаётся rollback-контролем.

## Новое KNN-OBS1: безопасный условный вызов не попадает в учебные наблюдения

### Сейчас

MethodFrequencyTable.ExtractMethodCalls берёт только invocation с MemberAccessExpressionSyntax; прочие invocation пропускает:

~~~csharp
if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
    continue;
~~~

У C# cache?.Add(1) имя вызова представлено MemberBindingExpressionSyntax внутри conditional access. Такой наблюдаемый вызов не записывается; cache.Add(1) записывается. [Current extraction L61–81](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Completion/MethodFrequencyTable.cs#L61-L81), [external extraction L56–76](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/Completion/MethodFrequencyTable.cs#L56-L76).

Путь до потребителя конкретен: Compile → RememberCompiledSource, затем публичный RankCompletions → EnsureMethodFrequencyTable → Train → ExtractMethodCalls → RankMethods. Если receiver отсутствует в памяти, RankMethods возвращает исходный порядок candidates. Поэтому корпус из корректных условных вызовов может расти без изменения completion ranking. Это learned counter baseline, не LoRA, не доказанный новый semantic Unit и не KNN расстояние. [Facade ranking](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerFacade.cs#L100-L111), [lazy training](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerFacade.cs#L240-L256), [fallback/order](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Completion/MethodFrequencyTable.cs#L45-L58).

### Новый риск интерпретации E02

Эксперимент E02 использует тот же extractor при построении списка оцениваемых allCalls; пропущенные conditional calls исчезают и из training observations, и из знаменателя top1. Поэтому старый E02 результат не измеряет эту часть C# синтаксиса. Это узкая потеря покрытия, отдельная от уже описанного переучёта одинакового source при повторном Compile. [E02 extraction and denominator](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E02FrequencyRanking.cs#L11-L63), [delegation to production](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E02FrequencyRanking.cs#L76-L84).

### Маленький опыт и конкретный ремонт

Два корпуса по два вызова: cache.Add и cache?.Add. Одинаковые candidates [Other, Add], одинаковая частотная модель, один новый query receiver cache. Записать extracted calls, TotalPairs и ranking; повторить после перестановки candidates. В исправленном extractor добавить явный разбор MemberBindingExpressionSyntax с receiver от соответствующего ConditionalAccessExpressionSyntax; хранить InvocationObservation(ReceiverKey, MethodName, IsConditional, SourceId, SpanStart). Условность сохранять отдельным полем, чтобы не смешивать различие намерения с идентичностью метода.

Положительный контроль: обычный member call сохраняет прежнее наблюдение. Отрицательный: a?.Add не должен зачисляться receiver b. Отдельный контроль — вложенные a?.B?.Run() с проверкой точного receiver, а не ближайшего попавшегося ancestor.

Ремонт выполняется в C# observation helper существующей completion memory; изменение не даёт права объявить её новым semantic learner. При переносе в архитектуру ILogicalUnit владельцем может быть CSharpCompletionObservationUnit, а MethodFrequencyTable остаётся его baseline state. Для текущего API восстановление — повтор Train из прежних raw C# sources; скрытого сохранённого файла этого table в исследованном коде нет.

## Дополнительные сигналы без увеличения числа основных находок

В E02 также видны два конкретных ограничения идентичности: case-insensitive ключи склеивают различные C# методы Read/read, а ExtractLeafIdentifier сворачивает db.Users.Add и db.Log.Write к общему receiver db. Native probe печатает их отдельно. Выполненная ASCII арифметическая проверка Read×5/read×1 показала merged count=6 и зависимость выбора от порядка candidates. Это [proxy](evidence/e02_identity_proxy.py), не результат C# parser/модели. Переносить правило сохранения регистра на любой SQL-диалект или natural language нельзя: идентичность определяется доменом.

## Новизна и границы

Audit_inventory сверил оба README, P01–P125 и весь Markdown в двух исследовательских каталогах: 661 уникальный документ, 389 planning и 272 core, без ошибок загрузки. Для RegexPreprocessor, XmlDocs, LineComments, StringLiterals, ExtractMethodCalls, ExtractLeafIdentifier, MemberBindingExpression, conditional access точных прежних находок не обнаружено; прежний MethodFrequencyTable описан в LT-N03 лишь для повторного Compile. Это полнотекстовая проверка точных механизмов и ближайших тематических совпадений; полный семантический аудит каждой строки и ZIP этим не объявляется.

Снятые дубликаты: E13 expression-body collapse; E09 SiblingCount; E14 Array/List/Task feature alias; E14 guard scope/receiver/dominance; E05 самообучение на текущем Compile; generic hash overflow; prefix64 и fusion scale. Они не считаются новыми результатами этого пакета.

Проверено шесть production-файлов на body parity, соседние KNN и экспериментальные consumers, четыре строки regex-зеркала и один отдельный counter fixture. Независимый исследователь /root/dpo_lora прочёл production sources, options, FileActionKnn, E02/E10 и diagnostic runner; три механизма подтвердил по source, потребовал явную проверку CSharp route и ожидаемых outputs. Эти диагностические поля добавлены в native JSON. Это code review исследовательского пакета, не независимый DPO quality judge.

Независимых новых механизмов заявлено **три**, RX1/RX2 не считаются двумя независимыми historical receipts. Не выполнены native .NET, Roslyn diagnostics, настоящая тренировка Student, китайская модель, CODEX judge, cold semantic quality test. В этой среде dotnet отсутствует.

**Роли DPO:** CODEX с Astra 6 UltraCode выполняет делегированный человеческий выбор и независимую оценку обеих финальных сторон пары. Внешняя открытая китайская модель создаёт chosen и rejected. Ни один synthetic fixture этого пакета не выдаётся за такую DPO-пару или за независимую оценку.
