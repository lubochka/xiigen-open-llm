# Code Documentation Four-Level Training Plan
# Version: 1.0 - 2026-05-11
# Domain: Legacy .NET / C# documentation
# Source registry: CODE-DOC-SOURCE-REGISTRY-v1.md
# Routing plan: CODE-DOC-CALL-TREE-ORCHESTRATION-PLAN-v1.md
# Executor prompt: EXECUTOR-PROMPT-CODE-DOC-50-PAIR-LEARNING-v1.md

---

## Purpose

This plan trains a small local model to write accurate documentation for
legacy .NET code by climbing the call tree.

The model learns in four levels:

- D1 - method/function documentation
- D2 - class/component documentation
- D3 - caller-chain documentation
- D4 - workflow/module documentation

Fifty pairs is the batch size, not the total curriculum size. Each stream may
take many accepted 50-pair batches.

---

## Stream Shape

One stream is:

```text
app_side x runtime_family x target_framework x csharp_language_version x stack_label x artifact_kind x documentation_level
```

Examples:

- `server x NET_FRAMEWORK x net46 x CSharp5_or_CSharp6_review_required x NETFX_WEBAPI x controller_action x D3`
- `server x NET_CORE x netcoreapp3.1 x CSharp8 x NETCORE_WEBAPI x controller_action x D3`
- `mixed x NET_FRAMEWORK x net46 x CSharp5_or_CSharp6_review_required x NETFX_WEBAPI x auth_workflow x D4`
- `server x NET_CORE x netcoreapp3.1 x CSharp8 x LIBRARY_INFRASTRUCTURE x public_method x D1`

Each batch contains:

- 50 training pairs
- 50 held-out evaluation rows
- one look-like report
- one stream-state update
- one next-batch decision

Prepared pairsets and accepted batches are different things.

A batch is accepted only after actual DPO training and actual held-out inference
through `CodeDocumentationRoutingFlow`. If the flow is missing, the correct
status is `PAIRSET_READY_TRAINING_PENDING`, not accepted.

---

## Level D1 - Method / Function Documentation

Goal:
Teach local behavior without hallucinating caller context.

Pair mix per 50:

- 20 simple method purpose pairs
- 10 parameter and return documentation pairs
- 8 side-effect or data-access pairs
- 6 error-handling pairs
- 6 rejected pairs for invented behavior, wrong framework lifecycle, or missed
  side effects

Promotion gate:

- Documentation factual accuracy >= 0.80 on held-out rows
- No more than 2 invented-behavior findings per 50 held-out rows
- Side-effect recall >= 0.75
- DPO reward margin > 0

---

## Level D2 - Class / Component Documentation

Goal:
Teach component responsibility using method summaries.

Pair mix per 50:

- 15 class responsibility pairs
- 10 public API surface pairs
- 10 collaborator/dependency pairs
- 8 configuration/state pairs
- 7 rejected pairs for over-broad class purpose or dropped method evidence

Promotion gate:

- Public member coverage >= 0.85
- Collaborator precision >= 0.80
- No systematic confusion between controller, service, repository, and model
- DPO reward margin > 0

---

## Level D3 - Caller Chain Documentation

Goal:
Teach propagation from callee summaries into callers.

Pair mix per 50:

- 15 direct caller/callee propagation pairs
- 10 multi-step call-path pairs
- 10 local-vs-delegated behavior pairs
- 8 failure propagation pairs
- 7 rejected pairs for using stale/missing callee summaries

Promotion gate:

- Call-edge preservation >= 0.85
- Local-vs-delegated distinction >= 0.80
- Failure propagation recall >= 0.75
- DPO reward margin > 0

---

## Level D4 - Workflow / Module Documentation

Goal:
Teach workflow documentation from accumulated call-tree evidence.

Pair mix per 50:

- 12 entry point and lifecycle pairs
- 10 data-flow pairs
- 8 configuration and deployment context pairs
- 8 security/session/state boundary pairs
- 6 operational-risk pairs
- 6 rejected pairs for modernizing legacy behavior or inventing architecture

Promotion gate:

- Workflow factual accuracy >= 0.78
- No high-severity invented subsystem
- Configuration/lifecycle accuracy >= 0.75
- DPO reward margin > 0

---

## Stack Order

Recommended first streams:

1. `server x NET_CORE x netcoreapp3.1 x CSharp8 x LIBRARY_INFRASTRUCTURE x public_method x D1`
2. `server x NET_FRAMEWORK x net46 x CSharp5_or_CSharp6_review_required x NETFX_WEBAPI x helper_method x D1`
3. `server x NET_CORE x netcoreapp3.1 x CSharp8 x NETCORE_WEBAPI x controller_action x D3`
4. `mixed x NET_FRAMEWORK x net46 x CSharp5_or_CSharp6_review_required x NETFX_WEBAPI x auth_workflow x D3`
5. Promote successful streams to D2.

Do not start D4 for a stack until D1-D3 have accepted batches for that stack.

Version fields are part of the stream identity. Do not merge .NET Framework
4.6, ASP.NET Core 3.1, AngularJS 1.x, and modern Angular/.NET into one stream.
