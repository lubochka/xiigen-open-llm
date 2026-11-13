# Layer 0-F - Orchestration Routing Flow Pairs

Status: `PAIRSET_READY_ROUTING_FLOW_PENDING`

This file defines the required flow-level pair shape for promoting adaptive
routing. It intentionally contains zero train-ready pairs until a model-led
authoring pass builds actual 50-row batches and held-out rows.

Station files already present in this batch:

- `Layer-0-A-PromptCompiler.md`
- `Layer-0-B-MultiSubPhaseRouter.md`
- `Layer-0-C-KnowledgeEdge.md`
- `Layer-0-D-FlowFactory.md`

Those files are useful diagnostic material, but routing is accepted only when
the full flow is trained and measured.

## Contract

```csharp
OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>
```

Equivalent accepted surface:

```csharp
AdaptiveLogicalOrchestrator
```

## A-Side

The A-side is the complete runtime request:

```text
LogicalOrchestratorRequest
```

It must preserve:

- original user prompt
- request metadata
- source/canonicality context when available
- prior logical context when available

## Chosen B-Side

The chosen answer must include the complete flow-level routing result:

```text
CompiledDocument
RoutingDecisions
KnowledgeEdgeState
FlowResolutionState
SelectedFlowIds
Warnings
OverallRoutingVerdict
```

Minimum accepted verdicts:

- `route`
- `route_with_warning`
- `knowledge_edge`
- `reject`

## Rejected Patterns

Use adjacent, plausible wrong answers:

- compiler-only output presented as final routing
- router-only output presented as final routing
- single route when multiple branches are required
- missing knowledge-edge branch
- false executable-flow claim when only a factory or missing flow exists
- static lookup that skips evidence, confidence, or topology checks

Every rejected side must contain a specific `[WRONG: ...]` explanation naming
the failure and the consequence for orchestration.

## Batch Gate

Do not mark routing as accepted until both are true:

1. A 50-pair flow-level training batch exists for this file.
2. A disjoint held-out set executes through the full flow and records measured
   routing accuracy, wrong-route rate, knowledge-edge false-pass rate, CPU, RAM,
   and elapsed time.

If only station rows exist, keep:

```text
PAIRSET_READY_ROUTING_FLOW_PENDING
```
