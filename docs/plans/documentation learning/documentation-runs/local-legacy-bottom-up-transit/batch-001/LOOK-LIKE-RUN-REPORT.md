# Look-Like Run Report
# Run: local-legacy-bottom-up-transit / batch-001

## Status

`LOCAL_ACCEPTANCE_GATE_STATIC_PASS__RUNTIME_FLOW_AND_50_PAIR_ADAPTER_IMPLEMENTED__LORA_DPO_CHECKPOINT_PENDING`

This run validates whether the documentation guides can route local legacy
projects correctly and require bottom-up call transit. After the static gate,
`CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>` and
`CodeDocumentationTrainingRunner` were implemented in `llm_mvp_core`. This
report still does not claim LoRA/DPO checkpoint training: the remaining gap is
the heavier checkpoint-producing trainer adapter.

## Coverage

| Project | Expected route | Gate |
|---|---|---|
| wizGameserver | ASP.NET Core 3.1 WebApi / C# 8 / server | PASS |
| GenieDynamicServer | ASP.NET Core 3.1 WebApi + infrastructure libraries / C# 8 / server | PASS |
| geniesoftGameMVPServer | ASP.NET Core 3.1 thin WebApi / C# 8 / server | PASS |
| genietasks | .NET Framework 4.6 WebApi2 + ASMX + AngularJS 1.4.8 / mixed | PASS |

## Static Metrics

| Metric | Value |
|---|---:|
| version_routing_accuracy | 1.00 |
| framework_lifecycle_accuracy | 1.00 |
| bottom_up_order_accuracy | 1.00 |
| callee_summary_transit_accuracy | 1.00 |
| local_vs_delegated_accuracy | 1.00 |
| invented_behavior_count | 0 |
| missing_callee_summary_count | 0 |
| wrong_version_count | 0 |

## CPU / RAM / Training Metrics

Not run for LoRA/DPO checkpoint training. Runtime flow tests and a 50-pair
runner-adapter test now exist for documentation routing, version gates, transit
gates, and CPU/RAM/timing fields.

## Main Finding

The previous documentation-learning guides were structurally good but too broad
on routing. They now require exact platform/version fields and a bottom-up
transit gate. This prevents an executor from documenting `.NET Framework 4.6`
WebApi2/AngularJS code as ASP.NET Core or from writing caller documentation
before inner helper behavior is known.
