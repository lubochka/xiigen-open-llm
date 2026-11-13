# Plan: E09 Structural KNN и E10 Regex Prep — Feature Flags
# Version: 1
# Date: 2026-05-07
# Floor before: 1265
# Floor after:  1277 (+12 tests)

---

## Суть

**E10 Regex Prep** уже реализован в `RegexPreprocessor.Normalize` и вызывается
безусловно для CSharp стека внутри `SemanticCompilerFacade.Compile`. Флага нет.
Это значит невозможно написать тест, который проверяет поведение с препроцессором
и без него в одном тест-прогоне — нужно либо создавать два экземпляра фасада с
разной логикой, либо мокать статический класс. Оба варианта хрупкие.

Нужен явный флаг `bool useRegexPreprocessor` в `SemanticCompilerFacade`, чтобы
тест мог сравнить оба пути через один API.

**E09 Structural KNN** вообще не подключён к production. `StructuralKnnPredictor`
и `StructuralVectorExtractor` живут только в `35.ExperimentsV2`. Флаг `bool
useStructuralKnn` включает его как дополнительный шаг после AST токенизации —
добавляет `PredictedNextTokens` в `CompilationResult`. Когда флаг false — шаг
пропускается полностью, нулевых аллокаций нет.

Оба флага живут в одном объекте `SemanticCompilerOptions` — единственная точка
конфигурации. Default: оба true (поведение совпадает с текущим production).

---

## Phase 1 — SemanticCompilerOptions

**New file:** `12.SemanticCompiler/SemanticCompilerOptions.cs`

```csharp
namespace SemanticCompiler;

// Feature flags for SemanticCompilerFacade.
// All flags default to true so existing production behaviour is unchanged.
// Set to false in tests to isolate a specific feature.
public sealed class SemanticCompilerOptions
{
    // E10: strip XML docs, block/line comments, string literals, and extra blank
    // lines before AST parsing. Applied only when stack = CSharp.
    // Tier 1: +29.5% parse time improvement, 21% char reduction, zero syntax errors.
    // Tier 2: +2.4% on larger corpus — still positive, no regression.
    public bool UseRegexPreprocessor { get; init; } = true;

    // E09: after AST tokenisation, run StructuralKnnPredictor to populate
    // CompilationResult.PredictedNextTokens. Requires training data — when the
    // predictor has seen fewer than 10 token pairs, this step is skipped silently.
    // Tier 1: +3.1% top1 accuracy at 968 tokens; +39.3% at 5806 tokens.
    // Not yet ready for unconditional production use — flag allows A/B testing.
    public bool UseStructuralKnn { get; init; } = false; // off by default until Tier 2 passes

    // Convenience: both flags off — useful for baseline tests.
    public static SemanticCompilerOptions BaselineOnly => new()
    {
        UseRegexPreprocessor = false,
        UseStructuralKnn     = false
    };

    // Convenience: production defaults.
    public static SemanticCompilerOptions Default => new();
}
```

Note: `UseStructuralKnn` defaults to **false** because E09 is TUNE_OR_ABANDON at
tier 1 on small data. The flag exists so that when more data is available and the
experiment is re-run with larger corpus (tier 2+), the feature can be turned on
and measured without any code change.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 2 — Move StructuralVector and StructuralKnnPredictor to production

**New file:** `12.SemanticCompiler/Prediction/StructuralKnn.cs`

Move `StructuralVector`, `StructuralVectorExtractor`, and `StructuralKnnPredictor`
from `35.ExperimentsV2/Infrastructure/StructuralVectorExtractor.cs` and
`35.ExperimentsV2/Core/StructuralKnnPredictor.cs` into the production namespace
`SemanticCompiler.Prediction`. Namespace changes, all logic stays identical.

The experiment files update their references to use the production namespace via
the project reference that already exists. The experiment classes themselves stay
in `35.ExperimentsV2` — they measure the production implementation.

`StructuralKnnPredictor` needs no change: `Train`, `Predict`, `AverageNearestNeighborDistance`
are already the right API.

`StructuralVectorExtractor.Extract(SyntaxNode, short)` is already the right API.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 3 — Wire flags into SemanticCompilerFacade

**Modified:** `12.SemanticCompiler/SemanticCompilerFacade.cs`

Add `SemanticCompilerOptions` to the constructor. Wire both flags into `Compile`.

```csharp
public sealed class SemanticCompilerFacade : ISemanticCompiler
{
    private readonly StackDetector           _stackDetector;
    private readonly RouteClassifier         _routeClassifier;
    private readonly Decomposer              _decomposer;
    private readonly SemanticCompilerOptions _options;
    private readonly StructuralKnnPredictor  _structuralKnn;

    public SemanticCompilerFacade(
        StackDetector?           stackDetector   = null,
        RouteClassifier?         routeClassifier = null,
        Decomposer?              decomposer      = null,
        SemanticCompilerOptions? options         = null)
    {
        _stackDetector   = stackDetector   ?? new StackDetector();
        _routeClassifier = routeClassifier ?? new RouteClassifier();
        _decomposer      = decomposer      ?? new Decomposer();
        _options         = options         ?? SemanticCompilerOptions.Default;
        _structuralKnn   = new StructuralKnnPredictor(k: 5);
    }
```

Inside `Compile`:

```csharp
// E10: strip comments and string literals before AST parsing.
// Skipped when UseRegexPreprocessor = false (baseline tests) or non-CSharp stack.
if (_options.UseRegexPreprocessor && stackResult.Value == StackLabel.CSharp)
    source = RegexPreprocessor.Normalize(source);

// ... existing pipeline: NormalizeSource, CollapseToOps, ExtractPredicates, Split ...

// E09: structural KNN next-token prediction.
// Skipped when UseStructuralKnn = false (default) or predictor not yet trained.
IReadOnlyList<int> predictedNextTokens = [];
if (_options.UseStructuralKnn && _structuralKnn.TrainingSize >= 10)
{
    var nodes = CSharpSyntaxTree.ParseText(source)
        .GetRoot()
        .DescendantNodes()
        .ToArray();

    predictedNextTokens = nodes
        .Select(node => _structuralKnn.PredictFromNode(node, fileId: 0))
        .ToList();
}
```

Note: the full `StructuralKnnPredictor` integration requires AST node objects,
not just token ids — the predictor needs the `SyntaxNode` to build the vector.
This means `E09` integrates best at the point where `Compile` has direct access
to Roslyn nodes, not after the fact on a flat token list. The Compile step uses
`CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes()` and passes each
`SyntaxNode` to `PredictFromNode`.

```csharp
// In StructuralKnnPredictor (production version):
// Train from source text directly — builds token+vector pairs in one Roslyn pass.
public void TrainFromSource(string csharpSource, short fileId = 0) { ... }

// Predict next token given the current AST node.
public int PredictFromNode(SyntaxNode node, short fileId = 0)
    => Predict(StructuralVectorExtractor.Extract(node, fileId));
```

`CompilationResult` gets one new optional field:

```csharp
// Populated when UseStructuralKnn = true and predictor is trained.
// Each index i contains the predicted next AST node type for position i.
public IReadOnlyList<int> PredictedNextTokens { get; init; } = [];
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 4 — Tests

**New file:** `Tests/E09E10FeatureFlagTests.cs`

### E10 — Regex Preprocessor flag

**Test 1 — UseRegexPreprocessor=true strips XML doc comments**
```
Given: source = "/// <summary>Remove me</summary>\nclass Foo {}"
       SemanticCompilerFacade with options { UseRegexPreprocessor = true }
When:  Compile(source)
Then:  result succeeds
       (internally the XML comment was stripped before parsing — no parse error)
```

**Test 2 — UseRegexPreprocessor=false leaves source unchanged**
```
Given: source with block comment "/* keep me */" and code "class Foo {}"
       SemanticCompilerFacade with options { UseRegexPreprocessor = false }
       SemanticCompilerFacade with options { UseRegexPreprocessor = true }
When:  both compiled
Then:  both succeed
       with_flag.Value.NormalizedWords.Count <= without_flag.Value.NormalizedWords.Count
       (regex strips noise tokens; fewer normalized words when enabled)
```

**Test 3 — UseRegexPreprocessor not applied to non-CSharp stack**
```
Given: source = "// TypeScript comment\nconst x = 1;"
       (detected as TypeScript, not CSharp)
       options { UseRegexPreprocessor = true }
When:  Compile(source)
Then:  result succeeds
       the comment is NOT stripped (StackLabel != CSharp → flag bypassed)
```

**Test 4 — Char reduction matches E10 experiment findings**
```
Given: C# source with at least 3 XML doc comment lines and 1 block comment
       options { UseRegexPreprocessor = true }
When:  measure len(source) before and after Normalize
Then:  len(normalized) < len(source)
       reduction >= 10%  (E10: observed 19.5–21.1%)
```

**Test 5 — Zero syntax errors after preprocessing**
```
Given: 5 real C# source snippets each containing XML docs and line comments
       options { UseRegexPreprocessor = true }
When:  Compile called on each
Then:  all succeed with no parse errors in CompilationResult
```

**Test 6 — SemanticCompilerOptions.BaselineOnly disables both flags**
```
Given: SemanticCompilerOptions.BaselineOnly
When:  options.UseRegexPreprocessor and options.UseStructuralKnn read
Then:  both == false
```

### E09 — Structural KNN flag

**Test 7 — UseStructuralKnn=false produces empty PredictedNextTokens**
```
Given: SemanticCompilerFacade with options { UseStructuralKnn = false }
       valid C# source
When:  Compile(source)
Then:  result.Value.PredictedNextTokens.Count == 0
       (flag off → no KNN step, no allocation)
```

**Test 8 — UseStructuralKnn=true with untrained predictor still succeeds**
```
Given: SemanticCompilerFacade with options { UseStructuralKnn = true }
       predictor not yet trained (TrainingSize < 10)
       valid C# source
When:  Compile(source)
Then:  result succeeds (no exception)
       result.Value.PredictedNextTokens.Count == 0
       (guard: TrainingSize < 10 → skip silently)
```

**Test 9 — TrainFromSource populates the predictor**
```
Given: StructuralKnnPredictor, C# source with at least 20 AST nodes
When:  TrainFromSource(source) called
Then:  TrainingSize >= 10
```

**Test 10 — UseStructuralKnn=true with trained predictor populates predictions**
```
Given: SemanticCompilerFacade with options { UseStructuralKnn = true }
       predictor trained on same source (TrainingSize >= 10)
       valid C# source "class Foo { void Bar() { int x = 1; } }"
When:  Compile(source)
Then:  result.Value.PredictedNextTokens.Count > 0
       each value in PredictedNextTokens is a valid non-negative token id
```

**Test 11 — StructuralVectorExtractor.Extract returns non-default vector**
```
Given: C# source "class Foo { void Bar() { } }"
       parsed with Roslyn, first MethodDeclaration node extracted
When:  StructuralVectorExtractor.Extract(methodNode, fileId: 0)
Then:  vector.AstNodeType != 0  (real node type, not default)
       vector.Parent1Type != 0  (has parent — ClassDeclaration)
```

**Test 12 — StructuralKnnPredictor.Predict returns consistent result**
```
Given: predictor trained on 20 (vector, nextToken) pairs
       query vector identical to one of the training vectors
When:  Predict(that vector) called twice
Then:  both calls return the same token id
       (deterministic — same training, same query)
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1277, zero failures.
(+12 new tests)

---

## Поведение флагов — сводная таблица

| Флаг | Default | Что происходит когда true | Что происходит когда false |
|---|---|---|---|
| `UseRegexPreprocessor` | true | XML docs, block/line comments, строковые литералы убираются до AST парсинга (только для CSharp) | Source идёт в парсер как есть |
| `UseStructuralKnn` | false | После AST токенизации запускается `StructuralKnnPredictor`, результат в `PredictedNextTokens` | `PredictedNextTokens = []`, KNN шаг пропускается |

`UseStructuralKnn` по умолчанию false потому что E09 прошёл tier 1 только при
достаточном количестве данных (5806 токенов, +39.3%). При малых корпусах
(968 токенов, +3.1%) — ниже порога. Когда tier 2 будет пройден — переключить
default на true в одну строку.

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
