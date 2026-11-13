# FILE-STATIC-RECEIVER-01 — ложный null-check от статического поля в учебных метках

**Повтор уже сохранённого результата запрещён.** Сначала прочитать назначенные receipts/outputs; пригодный результат использовать автоматически без вопроса «reuse или retrain». Отсутствие полного снимка не разрешает новый warm-up, перенос другого donor либо восстановление missing native moments/RNG по догадке. Consumed опыт не получает дополнительных model calls.

Статус: **DESIGNED_NOT_IMPLEMENTED_NOT_RUN**. Это новая проверка внутри известной семьи N48/FileAction, не новый корневой класс проблем. Приоритет среди новых опытов: 1. Следующий ID: `RMS-MEAN-BACKWARD-01`, который разрешён только после квалификации назначенного S0. В текущей сессии — только документирование.

## 1. Что именно проверяется

Тема: программирование, C#, обучение выбора действия для файла. Нативный learner — `FileActionKnnPredictor(k:3)`; модель исполнителя остаётся по умолчанию. Neural model/LoRA/внешний teacher здесь не участвуют: изменяется настоящая обучаемая память примеров, которую читает штатный kNN.

В [FileActionKnn.cs:32–38](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/12.SemanticCompiler/Prediction/FileActionKnn.cs#L32-L38) любой member access без найденного if-null даёт `ADD_NULL_CHECK`, в том числе `StaticBox.V`, где `V` — статическое поле. Получатель этого доступа — тип, проверка его на null не имеет смысла. Метка поступает через [TrainFileActionKnn:194–203](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/12.SemanticCompiler/SemanticCompilerFacade.cs#L194-L203) в `Train`, который заменяет память; её читает [PredictFileAction:327–333](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/12.SemanticCompiler/SemanticCompilerFacade.cs#L327-L333). Consumer требует C#, включённый `UseFileActionKnn` и минимум4 урока. [По умолчанию опция выключена](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/12.SemanticCompiler/SemanticCompilerOptions.cs#L14-L21); связь с default RP01 neural path не установлена.

**Единственный фактор B:** при вычислении `hasMemberAccess` исключать доступ, который Roslyn SemanticModel разрешил именно в `IFieldSymbol` с `IsStatic=true`. Не менять `hasNullCondition`, taxonomy, признаки, k, расстояние, tie-break, порядок данных или reader. Если symbol unresolved/не поле — сохранить поведение A. Не угадывать static по имени/заглавной букве. Предложение будущей продуктовой правки: этот предикат в `ActionLabelGenerator.Label`; перенос после решения пользователя, не сейчас.

Это отличается от существующей [NG01](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md): там числовой `is >0` ошибочно считается null guard и подавляет нужную метку; здесь статическое поле создаёт ненужную метку. Старые N-KU/do-while и ограничения bag-of-SyntaxKinds уже известны и новизной не объявляются.

## 2. Точный набор и границы вывода

Вход: [file-static-receiver.json](../fixtures/file-static-receiver.json), SHA256 `bc4a70298103d95728c0aa69211cbf5d93ac0dcc3babd7d36b163c43a27d1c69`. Копировать байты без изменения. Порядок: `C-M1..C-M3`, `C-N1..C-N3`, затем `S1..S3`; только последние3 добавляются в пару. Весь набор — одна тема C# file-action, разные классы внутри неё.

| Группа | Роль | Что задано |
|---|---|---|
| C-M1..3 | Общие старые уроки | Простое `return int`, остаточная метка `ADD_METHOD` |
| C-N1..3 | Общие старые уроки | `b.V`, один параметр экземпляра, метка `ADD_NULL_CHECK` |
| S1..3 | Новые уроки | Static field `StaticBox.V`, метод без параметров; ожидаемая остаточная метка `ADD_METHOD` |
| Q-CS/Q-CI | Только чтение, никогда TRAIN | Статический/обычный доступ с одинаковым параметром метода; ожидается коллизия текущих SyntaxKind-векторов |

Первые9 чтений — fit/удержание обучаемой памяти на трёх семействах. Переименование классов не создаёт независимые структурные примеры и не доказывает generalization. `ADD_METHOD` здесь означает остаточный класс нынешней taxonomy после исключения ложной null-проверки, а не объективно лучший следующий edit. Основная содержательная метрика — ложные `ADD_NULL_CHECK` на статических полях, рядом полная матрица классов.

Признаки [76–107](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/12.SemanticCompiler/Prediction/FileActionKnn.cs#L76-L107) не включают `StaticKeyword`. Поэтому Q-CS/Q-CI намеренно показывают остаточное ограничение представления: одинаковому вектору детерминированный reader не сможет присвоить два разных класса. Не исправлять признаки, не исключать этот контроль, не считать его отрицательный результат опровержением исправления неправильных учебных меток. Это отдельная уже известная family feature-collision, а не второй фактор данного A/B.

## 3. Обученное общее «до» и A/B

1. Один native `Train(common6)` создаёт S0. Его память — упорядоченные шесть пар native feature array/label, без EXAM. Сохранить `common/memory.json`, точные строки и labels, source/fixture SHA и `common/preparation.json`. Нативного checkpoint API у этого класса нет: JSON — явно объявленная оболочка сохранения **настоящих** массивов памяти, не выдуманный native checkpoint.
2. Прочитать11 запросов через native `Predict`; квалификация S0 требует `TrainingSize=6`, C-M=3/3 и C-N=3/3. Одного ненулевого total недостаточно: пустой native reader по исходнику всегда возвращает `ADD_METHOD` и совпал бы с тремя plain gold. Проверить размер памяти0 до common Train и6 после; не выдавать этот исходниковый default за измеренный empty readout. Обученный S0 обязан различить оба класса; иначе `INVALID_LEARNING_BASELINE`, без увеличения набора/бюджета. В отчёт идут весь per-row before и константный `ADD_METHOD` на той же панели.
3. Два независимых экземпляра получают одинаковый S0 вызовом native `Train(saved6)`. Сохранить A0/B0 hashes и11 readouts каждого. Это восстановление памяти, не новые уроки. Если restore расходится — `BLOCKED_STATE_PARITY`, без поиска другого состояния.
4. A получает `Train(common6 + 3new)` с исходными automatic labels. B получает тот же упорядоченный набор с единственным изменением resolved-static-field predicate. `Train` **заменяет** память, поэтому подавать только3 новых урока запрещено. Сохранить фактические label arrays и все неожиданные изменения общих labels.
5. Читатель, feature vectors, k=3 и порядок чтений одинаковы. Сохранить11 A1 и11 B1. Измеряется реально выученная память после новых уроков, не просто точность rule-based labeler.

Всего:5 native Train calls =1 common construction+2 restore+2 update;6 исходных уроков+3 новых/плечо; физически36 переданных пар памяти с учётом восстановления (6+6+6+9+9), из них новые уникальные уроки всего9;55 Predict calls (11 common+22 restored+22 after). Label/feature extraction и сохранение учесть отдельно; измерение pair включает создание новых labels, включая цену SemanticModel в B. Нулевые fake optimizer/DPO counters не нужны: этих механизмов нет.

## 4. Где и как реализовать будущий опыт

Изолированный host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FSR01/`; попытка только `a001`, выход `a001/results/`. Хост ещё не создан. Исполнитель по отдельному заданию создаёт один `FSR01.csproj`, `.NET 9`, `Microsoft.CodeAnalysis.CSharp` версии `4.12.0`, `Program.cs`, `run.py`, `native/A/FileActionKnn.cs`, `native/B/FileActionKnn.cs` и копию fixture. Production ProjectReference не добавлять. В проекте включить implicit usings и `EnableDefaultCompileItems=false`, явно перечислить Program.cs и две native копии; отключить унаследованные Directory.Build.props/targets. До явного Sdk.props задать `OutputPath=a001/bin/`, `BaseIntermediateOutputPath=a001/obj/`, `MSBuildProjectExtensionsPath=a001/obj/`, оба `AppendTargetFrameworkToOutputPath=false` и `AppendRuntimeIdentifierToOutputPath=false`; AssemblyName=FSR01. После свойств и Compile entries импортировать Sdk.targets.

Источник обоих source copies: core commit `43e2337d68dad0e4ab8d7fbb0cea38406b592b61`, файл `12.SemanticCompiler/Prediction/FileActionKnn.cs`, Git blob `ef7a88a3c4c2540569b9ce6f204d8e4750875501`. Одинаково переименовать namespace A/B для изоляции; единственное поведенческое изменение B — описанный предикат со следующей необходимой реализацией binding.

В B `Label(string)` сохранить `tree = CSharpSyntaxTree.ParseText(csharpSource)` и `root = tree.GetRoot()`. Для **одной строки** создать `CSharpCompilation.Create` с единственным этим tree, `MetadataReference.CreateFromFile(typeof(object).Assembly.Location)` и `new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)`, затем `compilation.GetSemanticModel(tree)`. Добавить нужный `using Microsoft.CodeAnalysis`. В существующем Any по member access использовать `semanticModel.GetSymbolInfo(member).Symbol is IFieldSymbol { IsStatic: true }` как единственное основание исключения. Не объединять fixture в одну compilation: Q-CS/Q-CI намеренно повторяют имена типов. Symbol unresolved сохраняет A, diagnostic errors писать в результат. `Emit` и исполнение исходных C# программ не нужны; искусственные labels в Predict запрещены. Подготовка compilation входит в B label-preparation cost. На S1..S3 binding обязан вернуть static IFieldSymbol для каждого из трёх доступов. Если это не выполнено, статус `BLOCKED_SEMANTIC_BINDING`: фактор не применён, отрицательный результат гипотезы объявлять нельзя. Сохранить source diagnostics и actual counts; не исправлять fixture или добавлять ссылки по догадке.

Runner прямо вызывает эти native `Train/FeatureVector/Predict`. Он не называется full facade integration. Если отдельное подключение facade когда-нибудь понадобится, оно требует другой карточки; назначение `UseFileActionKnn=true` здесь объясняет production consumer, а не утверждает, что этот host вызывает `Compile`. Query-readouts не должны попадать в TRAIN или сохранённую память.

Единственный будущий entry после реализации:

```powershell
C:/Python314/python.exe C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FSR01/run.py --attempt a001 --fixture C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FSR01/fixtures/file-static-receiver.json
```

Реализовать exactly эти arguments. Wrapper проверяет существующий `a001`, claim и результат до build/log redirect/Train; затем create-new claim, argv `dotnet build FSR01.csproj -c Release --nologo --verbosity minimal` и один `dotnet a001/bin/FSR01.dll --attempt a001 --fixture <назначенный абсолютный путь> --nonce <созданный nonce>`. Cwd wrapper и child — FSR01 из `Path(__file__).parent`, не текущая директория терминала. Program проверяет переданный nonce и не создаёт вторую независимую попытку. Прямой `dotnet run` без wrapper запрещён, поскольку его build предшествовал бы guard. Занятый слот не перезаписывать и не обходить `a002`. Бюджет: одна сборка source-copy host (до180секунд) и одна описанная пара (runtime после build до120секунд); ноль повторов,0 генераций,0 внешних teacher calls. Native learner budget указан выше; общий deadline360секунд. Timeout сохраняет partial counters, не запускает новый опыт. Отсутствие назначенного Python/.NET даёт `BLOCKED_TOOL_CAPABILITY`, а не установку или поиск другой среды.

## 5. Метрики и допустимый вывод

На A0/A1/B0/B1: per-row output, false null-check count по static/instance/plain/collision отдельно, correct/denominator по группам, whole-panel accuracy, постоянный `ADD_METHOD`, память/число уроков, фактические state hashes; allocation/CPU/wall на label preparation, Train и readout раздельно. Common cost не складывать в A/B twice. Вывод о цене одного короткого прогона ограничить наблюдением, не устойчивым speedup.

Любое измеренное улучшение полезного показателя считается успехом по этому показателю и теме; рост общей точности не обязателен. Показать все ухудшения рядом, особенно instance controls и collision group. `0→0` без освоенного baseline не является учебным результатом. Ни положительный fit, ни исправленная label rule не доказывают способность различать статический/обычный доступ в произвольном C#.

При успехе — кандидат на ограниченную правку `ActionLabelGenerator.Label` с доказательствами и остаточными ограничениями, без production integration. До выполнения все результаты `null`, никаких ожидаемых чисел под видом измеренных.

## 6. Исполнитель, отчёт, STOP/NEXT и ожидание

Использовать модель по умолчанию и точные skills из [контракта](../06-execution-contract.md); отчёт по-русски `a001/results/report.md` (до700слов) и raw `result.json`, common state/receipt, A/B state/readouts. Не искать самому язык, host, donor или следующие ID. Этот документ и [промпт](../prompts/FILE-STATIC-RECEIVER-01.md) имеют приоритет над противоречащими старыми experiment thresholds; protected папка не меняется.

**STOP/NEXT:** немедленно прекратить предыдущую работу по прямому указанию пользователя. Перед переходом запрещены дописывание старого отчёта, исправление скрипта, commit, память, повторная проверка или ещё один запуск. Следовать указанному ID; следующий ID программы как указатель будущего назначения — `RMS-MEAN-BACKWARD-01`, с его donor gate. Это не автоматическое разрешение запуска; при поручении только этой карточки после неё — END. Если он blocked, зафиксировать это, не готовить S0 самостоятельно.

**Ожидание:** найти документированный execute/wait инструмент; сохранить handle и дождаться terminal через его native resume/wait в текущем ходе работы. Не повторять запуск, не заканчивать ответ «жду», не использовать shell polling/sleep/monitor и не считать отсутствие файла отказом. Короткие блокирующие ожидания до60секунд с сообщением существенного прогресса; после exit читать результат. Инструментальная невозможность — точный `BLOCKED_TOOL_CAPABILITY`, без выдуманной причины. В текущей документной сессии команду не выполнять.
