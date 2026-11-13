# Round Manifest - Phase 81 WDA-01 Real Router
# Date: 2026-05-05

---

## Purpose

Replace the WDA-01 deterministic document-classification fixture with a real
`ILogicalUnit` implementation for the 31 Wave 1 seed rows.

---

## Files Changed

- `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\SubPhaseRouterUnit.cs`
- `C:\Projects\llm_mvp_core\Tests\WdaOrchestrationExperimentTests.cs`

---

## Files Produced

- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\WDA-01-REAL-SOURCE-LEDGER-v1.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\WDA-01-SUB-PHASE-ROUTER-UNIT-CONTRACT-v1.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\WDA-01-ROUTER-REPLACEMENT-EXPECTED-TRACE-v1.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\WDA-01-ROUTER-REPLACEMENT-EVIDENCE-2026-05-05.json`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-COMPLETION-LEDGER-v2.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ROUND-MANIFEST-2026-05-05-PHASE-81-WDA-01-REAL-ROUTER.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\round-phase-81-wda-01-real-router-2026-05-05.zip`

---

## What Changed

Added `SubPhaseRouterUnit`, a real logical unit with this boundary:

```text
ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
```

The unit:

- reads active sub-phases from `ILogicalTopologyRegistry`;
- scores candidates using compiled predicate overlap and source-text evidence;
- returns `OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>`;
- preserves the 0.70 confidence floor and 0.50 wrong-domain ceiling;
- creates a knowledge-edge output instead of inventing a classification when
  confidence is too low.

Updated WDA-01 test:

- creates a topology registry with the 31 canonical sub-phases;
- builds 31 ready source documents from the source ledger;
- calls `SubPhaseRouterUnit.ExecuteAsync`;
- asserts 31/31 phase and sub-phase classifications;
- asserts zero source knowledge edges;
- asserts zero router knowledge edges;
- asserts the old deterministic runtime gap is absent.

---

## Verification

Focused WDA-01:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA01"
```

Result:

```text
Passed: 1, Failed: 0, Total: 1
```

Focused WDA:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA" --no-build
```

Result:

```text
Passed: 7, Failed: 0, Total: 7
```

Full suite:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build
```

Result:

```text
Passed: 1171, Failed: 0, Total: 1171
```

---

## Result

Phase 81 passes.

WDA-01 now has a real router-unit replacement for the Wave 1 seed set. The
whole v4 corpus is still not complete because quota and held-out evaluation
remain open.

---

## Next Action

Start Phase 82:

```text
Wave 1 quota ledger and held-out split plan
```
