# LLM-MVP-CORE: Language Portability Plan v1.0
## General Framework for Moving Any Component to Any Language

---

## The core principle (from XIIGen DNA)

> "You are not building N language versions. You are building 1 algorithmic
> specification and N language expressions of it."

Every algorithm in this codebase has two layers:

```
Layer 1 — Algorithmic (language-neutral):  the WHAT and WHY
  E17 logic extractor, ReAct loop, context compaction,
  false-CONJ guard, 70/30 replay buffer — these are the same
  in C#, Python, TypeScript, Rust, or Java.

Layer 2 — Language expression (replaceable): the HOW
  Roslyn → tree-sitter / ast-grep / javaparser
  SQLite via Microsoft.Data.Sqlite → sqlite3 / better-sqlite3 / rusqlite
  OperationResult<T> → Result / Either / TypedDict / tagged union
  YamlDotNet → PyYAML / js-yaml / serde_yaml
```

A port succeeds when Layer 1 is fully preserved and Layer 2 is fully replaced.
A port fails when Layer 1 is lost (algorithm simplified) or Layer 2 is kept
(the C# library called through FFI instead of replaced).

---

## Part 1 — General portability framework

### Step 1: Classify each component

Before writing a single line of the target language, classify every
source file into one of four categories.

| Category | Definition | Example | Port action |
|---|---|---|---|
| Pure algorithm | No I/O, no platform API, only math/logic | LogicExtractor, FamilyCollapser, EpisodeDecomposer | Translate line by line — no library replacements needed |
| Algorithm + serialization | Logic + read/write structured data | DomainDatasetCache, DomainModelRegistry | Translate algorithm, replace serialization library |
| Algorithm + platform | Logic + OS/runtime feature | IdleDetector, AutonomousTrainer | Translate algorithm, replace platform calls |
| Platform adapter | Thin wrapper around external library | Roslyn code analysis, SQLite session storage | Replace entirely with target-language equivalent — do not translate |

### Step 2: Map language-specific dependencies

For each dependency in the source, find the target-language equivalent
before starting the port. Do not start translating until this table is complete.

| Source library | What it does | Python | TypeScript/Node | Java | Rust |
|---|---|---|---|---|---|
| `Microsoft.CodeAnalysis.CSharp` (Roslyn) | Parse C# AST: symbols, methods, classes | `tree-sitter-c-sharp` | `@tree-sitter/tree-sitter` + `tree-sitter-c-sharp` | `JavaParser` (for Java AST) | `tree-sitter` crate |
| `Microsoft.Data.Sqlite` | SQLite persistence | `sqlite3` (stdlib) | `better-sqlite3` | `sqlite-jdbc` | `rusqlite` |
| `YamlDotNet` | Parse YAML skill files | `PyYAML` | `js-yaml` | `SnakeYAML` | `serde_yaml` |
| xUnit (test runner) | Unit tests | `pytest` | `jest` or `vitest` | JUnit 5 | `#[test]` built-in |

### Step 3: Translate the DNA patterns

The five DNA patterns from `CLAUDE.md` must appear in every target language.
They are not optional and must not be simplified.

**DNA-1 — OperationResult: never throw for business logic**

```python
# Python
from dataclasses import dataclass
from typing import TypeVar, Generic, Optional

T = TypeVar('T')

@dataclass
class OperationResult(Generic[T]):
    is_success: bool
    value: Optional[T]
    error_message: str = ""
    error_code: str = ""

    @classmethod
    def ok(cls, value: T) -> 'OperationResult[T]':
        return cls(True, value)

    @classmethod
    def fail(cls, message: str, code: str = "ERROR") -> 'OperationResult[T]':
        return cls(False, None, message, code)
```

```typescript
// TypeScript
interface OperationResult<T> {
    isSuccess: boolean;
    value?: T;
    errorMessage?: string;
    errorCode?: string;
}
function ok<T>(value: T): OperationResult<T> {
    return { isSuccess: true, value };
}
function fail<T>(message: string, code = "ERROR"): OperationResult<T> {
    return { isSuccess: false, errorMessage: message, errorCode: code };
}
```

```java
// Java
public record OperationResult<T>(boolean isSuccess, T value,
    String errorMessage, String errorCode) {
    public static <T> OperationResult<T> ok(T value) {
        return new OperationResult<>(true, value, "", "");
    }
    public static <T> OperationResult<T> fail(String message, String code) {
        return new OperationResult<>(false, null, message, code);
    }
}
```

```rust
// Rust — use the built-in Result<T, E> type directly
// DNA-1 maps to: Ok(value) / Err(AppError { message, code })
#[derive(Debug)]
pub struct AppError { pub message: String, pub code: String }
type OpResult<T> = Result<T, AppError>;
```

**DNA-2 — BaseService: all services inherit a common base**

```python
class BaseService:
    def __init__(self, storage):  # storage = session persistence
        self._storage = storage
```

```typescript
abstract class BaseService {
    constructor(protected storage: ISessionStorage) {}
}
```

**DNA-3 — Fabric First: external dependencies through interfaces**

```python
from abc import ABC, abstractmethod

class ICodeReader(ABC):
    @abstractmethod
    async def read_file(self, path: str) -> OperationResult[str]: ...

class ISemanticCompiler(ABC):
    @abstractmethod
    def compile(self, input: PipelineInput) -> OperationResult[PipelineOutput]: ...
```

**DNA-4 — Every public method returns OperationResult (or async equivalent)**

```python
# ✅ correct
def decompose(self, task: str, domain: Domain) -> OperationResult[list[SubTask]]:
    ...

# ❌ wrong
def decompose(self, task: str) -> list[SubTask]:  # can raise
    ...
```

**DNA-5 — Dynamic max_weight (from DECISIONS-LOCKED D-09)**

```python
def compute_max_weight(total_examples: int, num_classes: int,
                       rare_examples: int) -> float:
    if rare_examples <= 0:
        return 1.0
    return total_examples / (num_classes * rare_examples)
# Example: 1000 examples, 5 domains, 50 security → max_weight = 4.0
# NEVER hardcode ×10
```

### Step 4: Translate test by test

For every test in `Tests/*.cs`, write the equivalent test in the target language
before marking the component as ported. The port is not complete until:

```
C# tests that PASS  →  Target-language tests that PASS
C# tests that FAIL  →  Target-language tests that FAIL (same failure reason)
```

Test count must be equal or greater. Do not skip tests.

### Step 5: Run the portability verification checklist

After translating each component, run this checklist. All five must pass.

```
□ DNA-1: every public method returns OperationResult (or Result/Either/TypedDict)
□ DNA-2: every service class inherits BaseService equivalent
□ DNA-3: every external dependency injected through interface/ABC/trait
□ DNA-4: no exception thrown for business logic (only for programmer errors)
□ DNA-5: max_weight formula uses dynamic calculation, not a constant

□ Layer 1 preserved: algorithm behaviour matches C# baseline on same inputs
□ Layer 2 replaced: no C# library called via FFI or wrapper
□ Tests pass: equivalent test count, all pass
□ Dependency table complete: every source library has a named replacement
```

---

## Part 2 — Component portability map

Which components are easy, which are hard, and why.

### Group A — Pure algorithms (easiest, port in any order)

These have no platform dependencies. Translate line by line.

| Component | Subproject | Why easy |
|---|---|---|
| `LogicExtractor` | 29.SemanticCompiler | Pure predicate extraction — arrays and string matching |
| `FamilyCollapser` | 29.SemanticCompiler | Dictionary lookup + string manipulation |
| `SemanticNormalizer` | 29.SemanticCompiler | Stop-word list + stemming rules |
| `RouteClassifier` | 29.SemanticCompiler | K-NN on float arrays |
| `EpisodeDecomposer` | 15.OrchestratorModel | String splitting + domain lookup |
| `StopSignalDetector` | 15.OrchestratorModel | Pattern matching on strings |
| `DependencyGraph` | 22.AgentAlgorithms | BFS topological sort — pure graph algorithm |
| `ComplexTaskDecomposer` | 22.AgentAlgorithms | String splitting + false-CONJ guard |
| `FeedbackRouter` | 14.ContinuousLearning | Signal routing + weight formula |
| `ReplayBuffer` (E43) | 16.AutonomousLearning | List management + 70/30 split |
| `IdleDetector` (E41) | 16.AutonomousLearning | OS metric polling — replace 4 calls |

### Group B — Algorithm + serialization (medium, replace one library)

| Component | Source library to replace | Replacement |
|---|---|---|
| `DomainDatasetCache` | `System.Text.Json` | `json` stdlib (Python) / `JSON` built-in (JS) |
| `DomainModelRegistry` | `System.Text.Json` | same |
| `InMemoryEpisodeMemory` | none (in-memory only) | trivial port |
| `ContextWindowManager` | none | trivial port |
| `KnnSkillFilter` | none | trivial port |
| `AstTokenBudgetCalculator` | none | trivial port |

### Group C — Algorithm + platform (medium, replace platform calls)

| Component | Platform dependency | Python replacement | Node replacement |
|---|---|---|---|
| `AutonomousTrainer` | `LoraApplicator` from 10.LoRA | `peft` library | `onnxruntime` + custom LoRA |
| `ProjectIndexer` | `ICodeReader` (file system) | `pathlib` + `open()` | `fs.readFile` |
| `SessionStorage` (SQLite) | `Microsoft.Data.Sqlite` | `sqlite3` stdlib | `better-sqlite3` |
| `McpTransport` (23) | Named Pipe, .NET runtime | Unix domain socket | Node `net.createServer` |

### Group D — Platform adapters (hardest, replace entirely — do not translate)

| Component | What it does in C# | Target-language equivalent |
|---|---|---|
| Roslyn analysis (21, 23) | Parse C# AST: symbols, methods | `tree-sitter-c-sharp` (any target language) |
| `RoslynCodeAnalyzer` | Extract class/method names | `ast-grep` CLI or `tree-sitter` bindings |
| `YamlDotNet` parsing | Read skill `.yaml` files | `PyYAML` / `js-yaml` / `serde_yaml` |
| `LoraApplicator` (10.LoRA) | Apply LoRA layers in-process | `peft` (Python only — no Node/Java equivalent yet) |
| `DpoTrainer` (11.DPO) | DPO training loop | `trl` library (Python only) |

**Important note on LoRA and DPO:** subprojects 10 and 11 depend on
`TorchSharp` or similar ML runtime. For Python this maps directly to
`torch` + `peft` + `trl`. For Node/Java/Rust there is no mature equivalent —
these subprojects are Python-only until the ecosystem catches up.
The recommendation: keep 10.LoRA and 11.DPO as Python services behind an HTTP
or gRPC interface. All other subprojects call them as external services.

---

## Part 3 — Language-specific session plans

### TypeScript/Node.js port

**Recommended stack:** TypeScript 5 + Node 22 + Vitest (tests) + better-sqlite3

**What changes:**
- `OperationResult<T>` → interface + `ok<T>()` / `fail<T>()` functions
- `BaseService` → abstract class with constructor injection
- `ISemanticCompiler` → TypeScript interface (same name, same contract)
- Roslyn → `tree-sitter` + `tree-sitter-c-sharp` binding
- SQLite → `better-sqlite3` (synchronous, matches the C# usage pattern)
- LoRA/DPO → HTTP calls to Python service (Group D — do not port)

**What stays identical:** all algorithm logic (E01-E50 results are
language-neutral), all DNA patterns, all test cases (translated to Vitest).

**Order to port:** Group A first (pure algorithms — 11 components) →
Group B second (serialization — 6 components) → Group C (platform — 4 components)
→ Group D last (platform adapters — call Python service, do not rewrite).

### Python port

**Recommended stack:** Python 3.12 + pytest + dataclasses (no Pydantic required)

**What changes:**
- `OperationResult<T>` → generic dataclass (see DNA-1 above)
- `BaseService` → base class with `__init__(self, storage)`
- Async: C# `async/await` → Python `asyncio` + `async def`
- Roslyn → `tree-sitter` Python bindings or `ast` stdlib (for Python files)
- LoRA/DPO → **keep in Python** — this is the native home of `peft` and `trl`

**Unique advantage:** subprojects 10.LoRA, 11.DPO, 14.ContinuousLearning,
and 16.AutonomousLearning are most naturally expressed in Python because
`torch`, `peft`, `trl`, and `transformers` are Python-native.
The recommendation for a Python port is to start with these four subprojects
and keep the rest as optional ports.

**Order to port:** 10.LoRA → 11.DPO → 14.ContinuousLearning →
16.AutonomousLearning (all natural Python) → then Group A/B/C if needed.

### Java port

**Recommended stack:** Java 21 + Spring Boot 3 + JUnit 5 + JavaParser

**What changes:**
- `OperationResult<T>` → generic record (see DNA-1 above)
- `BaseService` → abstract class with constructor injection
- Roslyn → JavaParser (for Java AST) or tree-sitter Java binding (for C# AST)
- SQLite → `sqlite-jdbc`
- LoRA/DPO → HTTP calls to Python service

**Strongest use case:** enterprise environments where Java is mandated.
The agent runtime (21, 22) and semantic compiler (29) port cleanly to Java.

---

## Part 4 — The portability skill file

Save this as `docs/skills/language-portability-SKILL.md` in the repository.
Load it at the start of any session that ports a component to a new language.

```markdown
# language-portability-SKILL.md

## When to load
Any session that translates a component from C# to another language.

## The four-step protocol

### Step 1 — Classify (before writing any code)
Determine the category: Pure Algorithm / Algorithm+Serialization /
Algorithm+Platform / Platform Adapter.
Platform Adapters are replaced, not translated.

### Step 2 — Map dependencies
Complete the dependency table for the component being ported.
Every source library must have a named target-language replacement.
Do not start translating until the table is complete.

### Step 3 — Translate DNA patterns first
Before translating the algorithm, write the DNA patterns for the target
language (OperationResult, BaseService, Fabric First interfaces).
The algorithm translation goes faster once the scaffolding exists.

### Step 4 — Test parity gate
Port is complete only when:
  target test count >= source test count
  all ported tests pass
  DNA verification checklist passes (5 checks)

## DNA verification checklist (run after every component)
□ every public method returns OperationResult equivalent
□ every service inherits BaseService equivalent
□ every external dependency injected through interface/ABC/trait
□ no exception thrown for business logic
□ max_weight formula is dynamic (not constant ×10)
□ algorithm behaviour matches C# baseline on same inputs
□ Layer 2 (platform) fully replaced, no FFI to C# libraries
```

---

## Part 5 — What to do first

The fastest path to a working non-C# version:

**Step 1 (1 week):** Port all Group A components to Python.
These are 11 pure algorithms — no dependencies, translate line by line.
This gives a working semantic compiler pipeline in Python.

**Step 2 (1 week):** Add LoRA and DPO in Python (they are already Python-native).
Connect them to the ported Group A components.
Result: a complete `14.ContinuousLearning` in Python.

**Step 3 (1 week):** Port Group B (serialization) to Python.
Result: `DomainDatasetCache` and `DomainModelRegistry` in Python.
The full continuous learning loop works in Python.

**Step 4 (ongoing):** Port the agent runtime (21, 22) to TypeScript
for VS Code / JetBrains integration via `mcp.json`.
This is where the IDE-native experience comes from.

**What to NOT port:** 10.LoRA and 11.DPO in any language except Python.
Keep them as Python services. Every other language calls them over HTTP.
