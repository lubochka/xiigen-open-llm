# Plan: E13 Method KNN и E14 Context-Action KNN — Feature Flags
# Version: 1
# Date: 2026-05-07
# Floor before: 1265
# Floor after:  1277 (+12 tests)

---

## Суть

**E13 Method KNN** — tier 2 пройден (+150%, 28.6%→71.4%). Предсказывает
канонический тип следующего метода (Guard / Loop / Simple / Mixed) по KindBag
текущего. Полезен при анализе кода в `SemanticCompilerFacade.Compile` — когда
стек CSharp, можно добавить `PredictedNextMethodType` в `CompilationResult`.
`MethodKnnPredictor` и `MethodExtractor` сейчас только в `35.ExperimentsV2`.
Флаг по умолчанию **true** — tier 2 пройден, данных достаточно.

**E14 Context-Action KNN** — tier 2 пройден (+33.3%, 75%→100%), tier 3/4
провалился из-за дисбаланса классов (balance=0.01, ADD_NULL_CHECK занимает 374
из 464 примеров). Предсказывает что нужно сделать с файлом (ADD_NULL_CHECK /
ADD_LOOP / REFACTOR_EXTRACT / ADD_METHOD) по AST feature vector. Полезен как
сигнал в `DefaultLogicalCapabilityTrainer` при выборе DPO примеров. Флаг по
умолчанию **false** — пока не пройден tier 3, включать осторожно.

Оба флага добавляются в `SemanticCompilerOptions` из предыдущего плана.

---

## Phase 1 — Перенести в production: MethodExtractor, PackedMethod,
              MethodKnnPredictor, ActionLabelGenerator, FileActionKnnPredictor

**New file:** `12.SemanticCompiler/Prediction/MethodKnn.cs`

Переносит из `35.ExperimentsV2`:
- `PackedMethod` (record struct) из `MethodExtractor.cs`
- `MethodExtractor` (static class) из `Infrastructure/MethodExtractor.cs`
- `MethodKnnPredictor` из `Core/MethodKnnPredictor.cs`

Namespace меняется на `SemanticCompiler.Prediction`. Логика без изменений.

**New file:** `12.SemanticCompiler/Prediction/FileActionKnn.cs`

Переносит из `35.ExperimentsV2`:
- `ActionLabelGenerator` (static class) из `Infrastructure/ActionLabelGenerator.cs`
- `FileActionKnnPredictor` из `Core/FileActionKnnPredictor.cs`

Namespace меняется на `SemanticCompiler.Prediction`. Логика без изменений.

Эксперименты продолжают работать через project reference — ссылаются на
production классы, не держат копии.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 2 — Добавить флаги в SemanticCompilerOptions

**Modified:** `12.SemanticCompiler/SemanticCompilerOptions.cs`

Добавить два флага к уже существующим `UseRegexPreprocessor` и `UseStructuralKnn`:

```csharp
// E13: после AST-разбора C# файла предсказывает канонический тип следующего
// метода (Guard / Loop / Simple / Mixed) по KindBag текущего.
// Результат: CompilationResult.PredictedNextMethodType.
// Tier 2: +150% (28.6% → 71.4%). Флаг true по умолчанию — tier 2 пройден.
// Требует обучения: TrainMethodKnn(sources) до первого Compile.
// Пока предиктор не обучен (< 4 пар методов) — шаг пропускается молча.
public bool UseMethodKnn { get; init; } = true;

// E14: предсказывает какое действие нужно файлу по AST feature vector.
// Метки: ADD_NULL_CHECK / ADD_LOOP / REFACTOR_EXTRACT / ADD_METHOD.
// Результат: CompilationResult.PredictedFileAction.
// Tier 2: +33.3% (75% → 100%). Tier 3/4: 0% прироста — label_balance=0.01.
// Флаг false по умолчанию — пока не пройден tier 3 на сбалансированных данных.
public bool UseFileActionKnn { get; init; } = false;
```

Обновить `BaselineOnly`:

```csharp
public static SemanticCompilerOptions BaselineOnly => new()
{
    UseRegexPreprocessor = false,
    UseStructuralKnn     = false,
    UseMethodKnn         = false,
    UseFileActionKnn     = false
};
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 3 — Wire в SemanticCompilerFacade и CompilationResult

**Modified:** `12.SemanticCompiler/Core/Core.cs`

Добавить два опциональных поля в `CompilationResult`:

```csharp
public sealed record CompilationResult(
    StackLabel             Stack,
    SemanticCompiler29.Domain Domain,
    IReadOnlyList<int>     Predicates,
    IReadOnlyList<string>  Ops,
    IReadOnlyList<string>  NormalizedWords,
    IReadOnlyList<PromptSegment> Segments,
    // E13: canonical type of next method predicted by Method KNN.
    // Null when UseMethodKnn = false or predictor not yet trained.
    string?                PredictedNextMethodType = null,
    // E14: predicted action label for this source file.
    // Null when UseFileActionKnn = false.
    string?                PredictedFileAction = null,
    // E09: predicted next AST token ids (from previous plan).
    IReadOnlyList<int>?    PredictedNextTokens = null
);
```

**Modified:** `12.SemanticCompiler/SemanticCompilerFacade.cs`

Добавить поля и публичный метод обучения:

```csharp
private readonly MethodKnnPredictor     _methodKnn     = new(k: 5);
private readonly FileActionKnnPredictor _fileActionKnn = new(k: 3);

// Train Method KNN on a set of C# source files.
// Call once after loading project files, before Compile.
// Safe to call multiple times — retrains from scratch each time.
public void TrainMethodKnn(IReadOnlyList<string> csharpSources)
{
    var allMethods = new List<PackedMethod>();
    for (short fid = 0; fid < (short)Math.Min(csharpSources.Count, short.MaxValue); fid++)
    {
        var result = MethodExtractor.Extract(csharpSources[fid], fid);
        if (result.IsSuccess) allMethods.AddRange(result.Value!);
    }
    if (allMethods.Count >= 4)
        _methodKnn.Train(allMethods);
}

// Train File-Action KNN on a set of C# source files.
// Call once after loading project files, before Compile.
public void TrainFileActionKnn(IReadOnlyList<string> csharpSources)
{
    var examples = csharpSources
        .Select(src => (
            Features: ActionLabelGenerator.FeatureVector(src),
            Label:    ActionLabelGenerator.Label(src)))
        .ToList();
    if (examples.Count >= 4)
        _fileActionKnn.Train(examples);
}
```

Внутри `Compile` после основного pipeline:

```csharp
// E13: Method KNN — предсказание типа следующего метода.
string? predictedNextMethodType = null;
if (_options.UseMethodKnn && _methodKnn.TrainingSize >= 4
    && stackResult.Value == StackLabel.CSharp)
{
    var extracted = MethodExtractor.Extract(source);
    if (extracted.IsSuccess && extracted.Value!.Count > 0)
    {
        var lastMethod = extracted.Value[^1];
        predictedNextMethodType = _methodKnn.Predict(lastMethod);
    }
}

// E14: File-Action KNN — предсказание действия для файла.
string? predictedFileAction = null;
if (_options.UseFileActionKnn && _fileActionKnn.TrainingSize >= 4
    && stackResult.Value == StackLabel.CSharp)
{
    var features = ActionLabelGenerator.FeatureVector(source);
    int labelIdx = _fileActionKnn.Predict(features);
    predictedFileAction = ActionLabelGenerator.Labels[labelIdx];
}

var result = new CompilationResult(
    Stack:                   stackResult.Value,
    Domain:                  domain,
    Predicates:              predicatesResult.Value!,
    Ops:                     opsResult.Value!,
    NormalizedWords:         normalizedResult.Value!,
    Segments:                segmentsResult.Value!,
    PredictedNextMethodType: predictedNextMethodType,
    PredictedFileAction:     predictedFileAction);
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 4 — Tests

**New file:** `Tests/E13E14FeatureFlagTests.cs`

### E13 — Method KNN flag

**Test 1 — UseMethodKnn=false: PredictedNextMethodType всегда null**
```
Given: SemanticCompilerFacade с options { UseMethodKnn = false }
       C# source с двумя методами
When:  Compile(source)
Then:  result.Value.PredictedNextMethodType == null
```

**Test 2 — UseMethodKnn=true, предиктор не обучен: шаг пропускается молча**
```
Given: новый SemanticCompilerFacade с options { UseMethodKnn = true }
       TrainMethodKnn НЕ вызывался (TrainingSize < 4)
       C# source
When:  Compile(source)
Then:  result успешен (нет исключений)
       result.Value.PredictedNextMethodType == null
```

**Test 3 — TrainMethodKnn обучает предиктор**
```
Given: список C# source файлов, каждый с 3+ методами (итого 10+ методов)
When:  facade.TrainMethodKnn(sources)
Then:  _methodKnn.TrainingSize >= 4
```

**Test 4 — UseMethodKnn=true с обученным предиктором: результат не null**
```
Given: facade с UseMethodKnn=true
       TrainMethodKnn вызван на 5 файлах с методами
       C# source "class Foo { bool Validate(string s) { if (s == null) return false; return true; } }"
When:  Compile(source)
Then:  result.Value.PredictedNextMethodType != null
       result.Value.PredictedNextMethodType is one of: "Guard", "Loop", "Simple", "Mixed"
```

**Test 5 — MethodExtractor.Extract возвращает правильный KindBag**
```
Given: source = "class C { bool Check(string s) { if (s == null) return false; return true; } }"
When:  MethodExtractor.Extract(source)
Then:  result.IsSuccess == true
       result.Value[0].KindBag.Contains("If")
       result.Value[0].KindBag.Contains("Ret")
       MethodKnnPredictor.CanonicalType(result.Value[0].KindBag) == "Guard"
```

**Test 6 — MethodKnnPredictor.Predict детерминирован**
```
Given: predictor обучен на 10 парах методов
       один и тот же query метод
When:  Predict(query) вызван дважды
Then:  оба вызова возвращают одно и то же значение
```

### E14 — File-Action KNN flag

**Test 7 — UseFileActionKnn=false: PredictedFileAction всегда null**
```
Given: SemanticCompilerFacade с options { UseFileActionKnn = false }
       C# source
When:  Compile(source)
Then:  result.Value.PredictedFileAction == null
```

**Test 8 — UseFileActionKnn=true, предиктор не обучен: шаг пропускается молча**
```
Given: новый facade с options { UseFileActionKnn = true }
       TrainFileActionKnn НЕ вызывался
When:  Compile(source)
Then:  result успешен
       result.Value.PredictedFileAction == null
```

**Test 9 — ActionLabelGenerator.Label возвращает ADD_NULL_CHECK для метода без null guard**
```
Given: source = "class C { void Process(string s) { var len = s.Length; } }"
       (есть MemberAccess s.Length, нет null check)
When:  ActionLabelGenerator.Label(source)
Then:  result == ActionLabelGenerator.ADD_NULL_CHECK
```

**Test 10 — ActionLabelGenerator.Label возвращает ADD_LOOP для метода с IEnumerable без цикла**
```
Given: source = "class C { void Process(IEnumerable<int> items) { var x = 1; } }"
       (принимает коллекцию, нет foreach/for/while)
When:  ActionLabelGenerator.Label(source)
Then:  result == ActionLabelGenerator.ADD_LOOP
```

**Test 11 — TrainFileActionKnn обучает предиктор**
```
Given: 5 C# source файлов с разными структурами методов
When:  facade.TrainFileActionKnn(sources)
Then:  _fileActionKnn.TrainingSize >= 4
```

**Test 12 — UseFileActionKnn=true с обученным предиктором: результат валидный label**
```
Given: facade с UseFileActionKnn=true
       TrainFileActionKnn вызван на 5 файлах
       C# source с методом без null guard
When:  Compile(source)
Then:  result.Value.PredictedFileAction != null
       result.Value.PredictedFileAction is one of:
         "ADD_NULL_CHECK", "ADD_LOOP", "REFACTOR_EXTRACT", "ADD_METHOD"
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1277, zero failures.
(+12 новых тестов)

---

## Таблица флагов — итоговая (все четыре плана вместе)

| Флаг | Default | Tier пройден | Что добавляет в CompilationResult |
|---|---|---|---|
| `UseRegexPreprocessor` | true | Tier 1 ✅ Tier 2 ✅ | Очищает source до парсинга |
| `UseStructuralKnn` | false | Tier 1 ⚠ (мало данных) | `PredictedNextTokens` |
| `UseMethodKnn` | true | Tier 2 ✅ | `PredictedNextMethodType` |
| `UseFileActionKnn` | false | Tier 2 ✅ Tier 3 ❌ (дисбаланс) | `PredictedFileAction` |

---

## Execution order

Phase 1 → Phase 2 → Phase 3 → Phase 4.
Каждая фаза: `dotnet test`, floor 1265, zero failures.
Phase 4 поднимает floor до 1277.

---

## Floor summary

| After | Floor |
|---|---:|
| Start | 1265 |
| Phases 1–3 | 1265 |
| Phase 4 | 1277 |
