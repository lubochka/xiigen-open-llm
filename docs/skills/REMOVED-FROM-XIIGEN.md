# CURRENT STACK REFERENCE — LlmFromScratch for Xiigen Developers

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.
## Every xiigen concept and its live equivalent in this project

Nothing was removed. Everything was translated and implemented.
This document tells you where to find each capability.

---

## Stack map

| Xiigen (NestJS/TypeScript/React) | This project (.NET 9/C#/Windows Forms) | Location |
|----------------------------------|----------------------------------------|----------|
| Elasticsearch document store | BruteForceIndex / HnswIndex (vector), BM25Scorer + InvertedIndex (text) | 06.VectorSearch/, 12.SearchEngine/ |
| Elasticsearch hybrid search | HybridRetriever | 08.GraphRag/ |
| Redis Streams event bus | IAgentMessageBus | 22.AgentAlgorithms/AgentOrchestrator/ |
| Redis key-value cache | ISessionStorage (SessionStateStore) | 21.AgentInfrastructure/Session/ |
| NestJS DI container | .NET DI — constructor injection via interfaces (Rule 2) | every .cs service file |
| APP_GUARD (global auth guard) | PermissionGate | 21.AgentInfrastructure/Tools/PermissionGate.cs |
| APP_INTERCEPTOR | ToolRegistry — wraps every tool call | 21.AgentInfrastructure/Tools/ToolRegistry.cs |
| APP_PIPE (input validation) | OperationResult\<T\> guard chain — every public method validates at entry | Rule 1 |
| APP_FILTER (exception handling) | OperationResult.Fail() — no exceptions propagate as business logic | Rule 1 |
| DataProcessResult\<T\> | OperationResult\<T\> | AgentInfrastructure.OperationResult\<T\> |
| MicroserviceBase | BaseService(ISessionStorage) | 21.AgentInfrastructure/BaseService.cs |
| DynamicController | OrchestratorFacade + McpToolDispatcher | 15.OrchestratorFacade/, 23.OssEquivalents/Mcp/ |
| MCP server (tool dispatch) | McpTransport + McpToolDispatcher | 23.OssEquivalents/Mcp/ |
| Flow DAG orchestration | OrchestratorFacade → DependencyGraph → AgentLauncher | 15.OrchestratorFacade/, 22.AgentAlgorithms/ |
| Arbiter quality panel | QualityOrchestrator (16-check pattern) | 16.QualityOrchestrator/ |
| Skill file loader (.yaml) | SkillFileLoader | 23.OssEquivalents/Frontmatter/SkillFileLoader.cs |
| Multi-tenant scopeId | sessionId (ISessionStorage) + rootPath (ProjectIndexer) — passed explicitly |  |
| Docker Compose test containers | In-process — all real .NET implementations available without containers | see integration-testing-SKILL.md |
| React / React Native UI | Windows Forms (net9.0-windows, UseWindowsForms=true) | 21/22/23.Desktop/ |
| TypeScript .ts file generation | SemanticCompilerPipeline → PipelineOutput.Predicates[] + TemplateBeautifier | 29.SemanticCompiler/ |
| jest + @testing-library | xUnit [Fact]/[Theory] for library code; manual via .Desktop stage explorer for UI | Tests/ |
| npm run build | dotnet build LlmFromScratch.sln | — |
| npx jest --verbose | dotnet test Tests/LlmFromScratch.Tests.csproj | — |

---

## Event bus — IAgentMessageBus

`IAgentMessageBus` is this project's event bus. It is not a port of something external — it is the project's own fabric interface, implemented today by `MockAgentMessageBus` in the demos and tested live in 22.AgentAlgorithms.

```csharp
// Interface (22.AgentAlgorithms)
public interface IAgentMessageBus
{
    Task<OperationResult<string>> AssignTaskAsync(string subAgentName, string task);
}
```

Live usage: `AgentOrchestrator` injects `IAgentMessageBus` and dispatches sub-agent tasks through it. The `22.AgentAlgorithms.Desktop` Stage Explorer demo exercises this on every run.

---

## Session cache — ISessionStorage

`ISessionStorage` is this project's persistent session cache. It is not a port of something external — it is the project's own fabric interface, implemented today by `SessionStateStore` (file-backed) and `InMemorySessionStorage` (tests/demos).

```csharp
// Interface (21.AgentInfrastructure)
public interface ISessionStorage
{
    Task<OperationResult<bool>>                    SaveAsync(string sessionId, AgentState state);
    Task<OperationResult<AgentState>>              LoadAsync(string sessionId);
    Task<OperationResult<IReadOnlyList<string>>>   ListSessionsAsync();
}
```

Live usage: Stage 07 in the `21.AgentInfrastructure.Desktop` Stage Explorer saves and loads an `AgentState` via `ISessionStorage` on every demo run.

---

## Permission gate — PermissionGate + ToolRegistry

`PermissionGate` is the equivalent of NestJS `APP_GUARD`. It runs before every tool execution, checks that the requested path is within the project root, and returns `OperationResult<PermissionDecision>`.

`ToolRegistry` is the equivalent of `APP_INTERCEPTOR`. It holds all registered tool definitions and calls `PermissionGate.Check()` before invoking any tool handler.

Live usage: Stage 05 in the `21.AgentInfrastructure.Desktop` Stage Explorer runs three probe decisions (ReadOnly, WriteWithApproval, Denied) using the real `PermissionGate`.

---

## Search fabric — BM25 + VectorSearch + GraphRag

| Concept | Implementation | Tests |
|---------|---------------|-------|
| Full-text BM25 ranking | BM25Scorer + InvertedIndex | 12.SearchEngine tests |
| Dense vector search (HNSW) | HnswIndex, IVectorIndex | 06.VectorSearch tests |
| Exact-match vector search | BruteForceIndex | 06.VectorSearch tests |
| Hybrid search (text + vector) | HybridRetriever | 08.GraphRag tests |
| Unified search facade | SearchEngineFacade | 12.SearchEngine tests |

All implementations are in-process .NET — no external server required. See `integration-testing-SKILL.md` for the fabric coverage matrix.

---

## Windows Forms UI

Three Desktop stage explorers exist today, each a `WinExe` project (`net9.0-windows`, `UseWindowsForms=true`):

| Project | What it shows |
|---------|--------------|
| 21.AgentInfrastructure.Desktop | 7 stages: config loader, codebase indexer, context assembler, agent loop, tool system, file-change applier, session store |
| 22.AgentAlgorithms.Desktop | 8 algorithms: pipeline, repo-map, ReAct loop, context compaction, orchestrator, permission model, rules parser, task decomposer |
| 23.OssEquivalents.Desktop | Stage explorer for MCP, skill-file loader, and OSS-equivalent tools |

Form layout conventions used across all three:
- `SplitContainer` (vertical, left panel fixed at 380)
- `ListBox` (left) + `TextBox` multiline readonly (right, Consolas 9.5f)
- `Button` at bottom of left panel, `StatusStrip` at bottom of form
- `DockStyle.Fill` for all panels

UI is tested manually via the stage explorers. Class library logic is tested via xUnit. See `ui-testing-SKILL.md` for the full protocol.

---

## Generated code audit

SemanticCompilerPipeline (29.SemanticCompiler) already generates output today:
- `PipelineOutput.Predicates[]` — structured predicate list derived from input text
- `TemplateBeautifier` formatted output — formatted code template strings

Future subproject 17 (DynamicDomains) will generate C# type definitions.

All generated output must pass the audit in `dna-compliance-guard-SKILL.md §Generated output audit` before commit.

---

## Skill translation status

| Xiigen skill | This project equivalent | Status |
|---|---|---|
| dev-safety-SKILL | dev-safety-SKILL.md v2.0 | ✅ Translated |
| test-integrity-SKILL | test-integrity-SKILL.md v1.0 | ✅ Translated |
| mental-debug-SKILL (14 rules) | mental-debug-SKILL.md v3.0 (8 rules — 6 relevant, 2 deferred*) | ✅ Translated |
| dna-compliance-guard-SKILL | dna-compliance-guard-SKILL.md v1.0 | ✅ Translated |
| three-level-verification-SKILL | three-level-verification-SKILL.md v1.0 | ✅ Translated |
| plan-review-SKILL (FC-1..12) | plan-review-SKILL.md v3.0 | ✅ Translated |
| docker-local-testing-SKILL | integration-testing-SKILL.md v1.0 | ✅ Translated |
| impeccable-SKILL + design-for-ai | ui-testing-SKILL.md v1.0 | ✅ Translated |
| generated-service-audit (mental-debug rule 12) | dna-compliance-guard-SKILL.md §Generated output audit | ✅ Translated |
| flow-lifecycle-SKILL | docs/ARCHITECTURE.md §Agent Pipeline + §SemanticCompiler | ✅ Documented |
| portability tier certification | docs/plans/LLM-MVP-CORE-LANGUAGE-PORTABILITY-PLAN-v1_0.md | ✅ Covered |
| session-output skills | Round Contract + memory system | ✅ Already implemented |

*Mental-debug rules deferred (2):
- Rule 11 (AsyncLocalStorage propagation): Node.js only — C# passes context via explicit parameters
- Rule 14 (tenant-scope-leak in Promise.all): C# uses CancellationToken, no concurrent AsyncLocalStorage
