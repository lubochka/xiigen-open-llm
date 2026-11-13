# Code Documentation Call-Tree Orchestration Plan
# Version: 1.0 - 2026-05-11
# Domain: Bottom-up legacy .NET documentation
# Runtime contract: CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>

---

## Purpose

The documentation model must learn to document legacy C# code bottom-up.

The key behavior is not a single summarizer. It is a compiler-like propagation
flow:

1. extract the smallest method behavior
2. store the method summary as evidence
3. pass callee behavior to callers
4. summarize the caller with both local code and known callee summaries
5. repeat until class, page, controller, module, and workflow documentation can
   be written without inventing behavior

---

## Public Runtime Contract

The routing surface must be a trainable flow or orchestrator:

```csharp
public sealed record CodeDocumentationRequest(
    string SourceId,
    string Repository,
    string CommitSha,
    string FilePath,
    string SymbolId,
    string CodeText,
    IReadOnlyList<CallEdge> Calls,
    IReadOnlyList<CalleeSummary> KnownCallees,
    string AppSide,
    string RuntimeFamily,
    string TargetFramework,
    string CSharpLanguageVersion,
    string LanguageVersionConfidence,
    string ProjectStyle,
    string WebFramework,
    string WebFrameworkVersion,
    string FrontendFramework,
    string FrontendFrameworkVersion,
    string StackLabel,
    string ArtifactKind,
    string DocumentationLevel);

public sealed record CodeDocumentationDecision(
    string DocumentationLevel,
    string StackLabel,
    string ArtifactKind,
    string EvidencePolicy,
    string NextPropagationTarget,
    decimal Confidence,
    IReadOnlyList<string> RequiredEvidence);

public interface ICodeDocumentationRoutingFlow
    : ILogicalFlow<CodeDocumentationRequest>
{
}
```

Do not implement this as a static filename switch.

Do not route by stack label alone. The request must carry server/client side,
runtime family, target framework, inferred C# language version, framework
version, and frontend framework version when present. The required field rules
and local legacy acceptance targets are defined in
`CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md`.

Internal nodes may be `ILogicalUnit`, but the trainable behavior is the full
flow/orchestrator.

The learning sets may be prepared before this flow exists, but no documentation
batch may be accepted or promoted until this flow executes the held-out rows.

---

## Logical Nodes

### Node 01 - Source Inventory Compiler

Inputs:

- repository
- commit
- license proof
- file path
- code text

Outputs:

- source eligibility
- stack label
- artifact kind

Reject if license proof is missing.

### Node 02 - Legacy Stack Classifier

Routes code into one of:

- WebForms page/code-behind
- MVC controller/view model
- Web API controller
- WPF/WinForms class
- HttpModule/HttpHandler
- data access / repository
- infrastructure library
- unknown review-required

Also emits version-aware routing fields:

- app side: server, client, shared, or mixed
- runtime family: .NET Framework, .NET Core, .NET Standard, or unknown
- target framework: for example `net46` or `netcoreapp3.1`
- inferred C# language version and confidence
- project style: SDK-style or legacy non-SDK
- web/client framework and exact package/framework version when available

### Node 03 - Symbol And Call Graph Compiler

Extracts:

- method signature
- parameters
- return type
- direct calls
- external calls
- data access
- side effects
- exception/error handling

### Node 04 - Bottom-Level Method Documenter

Creates a method summary using only local code and direct call names when no
callee summaries exist.

### Node 05 - Callee Summary Propagator

Passes approved method summaries upward to callers.

The caller documentation must distinguish:

- local behavior in the caller
- behavior delegated to a callee
- behavior inferred through a stored callee summary

It must preserve the source version context. A callee summary created from
`.NET Framework 4.6 / WebApi2` code cannot be reused as if it were ASP.NET Core
3.1 behavior.

### Node 06 - Component Documenter

Documents class, controller, page, service, repository, or module behavior with
method summaries as evidence.

### Node 07 - Workflow Documenter

Documents a complete request, page lifecycle, event handler path, controller
action, or business workflow.

### Node 08 - Documentation Arbiter

Checks:

- no invented behavior
- no missing side effects
- no framework mismatch
- no dropped error path
- no over-modernization of legacy code
- all claimed behavior cites code or propagated summary

### Node 09 - Report Formatter

Writes the documentation and the look-like run report.

---

## Documentation Levels

### Level D1 - Method / Function

Document a single method.

Must include:

- purpose
- inputs
- outputs
- direct side effects
- direct dependencies
- error handling
- evidence lines or symbol references

### Level D2 - Class / Component

Document a class or component with method summaries.

Must include:

- responsibility
- public surface
- key collaborators
- state and configuration
- method map
- known limitations

### Level D3 - Caller Chain

Document a caller with known callees.

Must include:

- call path
- local vs delegated behavior
- data/control flow
- propagation of constraints
- failure propagation

### Level D4 - Workflow / Module

Document a user-visible workflow or module.

Must include:

- entry point
- lifecycle or request path
- participating classes
- data stores
- external boundaries
- configuration dependencies
- operational risks

---

## Propagation Rule

No caller-level documentation may claim callee behavior unless one of these is
present:

- the callee code is included in the A-side
- an accepted `CalleeSummary` is included in the A-side
- the behavior is explicitly visible through the caller's own return/error path

This rule is the heart of the curriculum.

## Transit Acceptance Gate

For every D3 or D4 row, the executor must prove:

- all important callees are either included in the A-side or represented by an
  accepted `CalleeSummary`
- the chosen documentation separates local caller behavior from delegated
  callee behavior
- framework/version context is unchanged while behavior propagates upward
- no workflow or module summary is written before the required D1-D3 evidence
  exists

Required metrics:

- `bottom_up_order_accuracy`
- `callee_summary_transit_accuracy`
- `local_vs_delegated_accuracy`
- `missing_callee_summary_count`
- `wrong_version_count`
