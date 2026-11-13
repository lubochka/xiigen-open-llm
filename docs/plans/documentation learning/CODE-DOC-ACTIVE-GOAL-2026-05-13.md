# Code Documentation Active Goal
# Date: 2026-05-13

## Goal

Focus on the documentation-learning task in:

`C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning`

Validate it against local legacy projects:

- `C:\Projects\wizGameserver`
- `C:\Projects\GenieDynamicServer`
- `C:\Projects\geniesoftGameMVPServer`
- `C:\Projects\genietasks`
- `C:\Projects\askapediaweb`

The documentation route must choose more than a broad stack. It must also choose:

- server/client/mixed side
- .NET runtime family
- exact target framework when available
- inferred C# language version and confidence
- web framework and version
- frontend framework and version
- artifact kind
- documentation level D1-D4

## Core Behavior

Documentation must be built like a compiler:

1. start with the smallest internal functions/methods
2. document their local behavior only
3. store accepted method summaries as `CalleeSummary`
4. pass those summaries upward to callers
5. document caller behavior as local behavior plus delegated callee behavior
6. document class/component/workflow only after required lower-level evidence exists

## Self-Check Rule

The executor must check runtime readiness itself:

- whether `CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>` exists
- whether a runner can read 50 train pairs and 50 held-out rows
- whether DPO training actually ran
- whether held-out inference actually ran through the documentation flow
- whether CPU, RAM, timing, DPO metrics, and documentation accuracy are real or null

No batch may be called trained if the flow or runner is missing.

## Current Known Status

The documentation guides and static acceptance gate exist.

General DPO/capability training infrastructure exists in `C:\Projects\llm_mvp_core`.

`CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>` has
now been added to `C:\Projects\llm_mvp_core\26.TrainableRouter\Training`.

The flow currently verifies:

- documentation specialist routing
- .NET runtime family and target framework
- inferred C# language version
- legacy vs SDK project style
- frontend framework/version context
- D1-D4 bottom-up stage planning
- D3/D4 blocking when caller documentation lacks accepted callee summaries

`CodeDocumentationTrainingRunner` has now been added beside the flow.
`CodeDocumentationBatchFileRunner` has also been added so executor-produced
`TRAIN-PAIRS.jsonl` and `HELDOUT-ROWS.jsonl` files can run directly. The runner
requires exactly 50 training examples and exactly 50 held-out examples, trains a
documentation routing profile, runs held-out requests through
`CodeDocumentationRoutingFlow`, and reports:

- routing accuracy
- version gate accuracy
- transit gate accuracy
- bottom-up order accuracy
- callee-summary transit accuracy
- local-vs-delegated accuracy
- CPU milliseconds
- peak working set MB
- elapsed milliseconds
- rejected-side DPO pair candidates with `[WRONG: ...]`

The file runner enforces the hard gate that every rejected side contains
`[WRONG: ...]`.

Remaining gap before full LoRA/DPO documentation training can be accepted:

- connect the code-doc 50-pair adapter to the heavier
  `DefaultLogicalCapabilityTrainer`/checkpoint path when the batch should
  produce LoRA/DPO checkpoint artifacts, not only the trainable routing profile
  and DPO pair candidates.

## Runner Progress 2026-05-13

- dotnet-csharp-runner-adapter batches 001-006 runner verified.
- Total prepared and tested: 300 train rows + 300 held-out rows.
- Latest metrics: routing_accuracy=1.000, version_gate_accuracy=1.000, transit_gate_accuracy=1.000.
- Runner records CPU, peak RAM, and elapsed time per batch in TRAINING-METRICS.json and LOOK-LIKE-RUN-REPORT.md.
- Local legacy projects remain validation-only, not external training authority.


## Runner Progress Update

- dotnet-csharp-runner-adapter batches 001-008 runner verified.
- Total prepared and tested: 400 train rows + 400 held-out rows.
- Added source-root selection gate in code: valid source roots pass, generated/vendor roots fail.
- Latest metrics: routing_accuracy=1.000, source_gate_accuracy=1.000, version_gate_accuracy=1.000, transit_gate_accuracy=1.000.

## Runner Progress Update 2

- dotnet-csharp-runner-adapter batches 001-010 runner verified.
- Total prepared and tested: 500 train rows + 500 held-out rows.
- Batch-010 completed the first 500-row checkpoint.
- Latest batch-010 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=0.800
  - callee_summary_transit_accuracy=0.800
  - local_vs_delegated_accuracy=0.800
  - CPU ms=468.75
  - peak RAM MB=34.910
  - elapsed ms=521.114
- Next focus: harden D4 workflow documentation so caller-level documentation cannot pass unless accepted lower-level summaries are present and carried upward.

## Runner Progress Update 3

- dotnet-csharp-runner-adapter batch 011 runner verified.
- Total prepared and tested: 550 train rows + 550 held-out rows.
- Batch-011 was a corrective D4 transit batch: every local held-out row supplied accepted lower-level `known_callees` and then tested whether workflow documentation carries those meanings upward.
- Latest batch-011 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=437.50
  - peak RAM MB=35.219
  - elapsed ms=386.083
- Improvement vs batch-010: bottom_up_order, callee_summary_transit, and local_vs_delegated rose from 0.800 to 1.000 after changing the held-out design from blocked negative transit to positive accepted-callee propagation.

## Runner Progress Update 4

- dotnet-csharp-runner-adapter batch 012 runner verified.
- Total prepared and tested: 600 train rows + 600 held-out rows.
- Batch-012 mixed controlled validation negatives with positive D4 transit:
  - vendor script roots must route to documentation but fail the source gate
  - generated EF designer roots must route to documentation but fail the source gate
  - net46 code mislabeled as NET_CORE/CSharp8 must fail the version gate
  - valid D4 dynamic-document cases must pass source, version, and transit
- Latest batch-012 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=250.00
  - peak RAM MB=33.996
  - elapsed ms=277.235

## Runner Progress Update 5

- dotnet-csharp-runner-adapter batch 013 runner verified.
- Total prepared and tested: 650 train rows + 650 held-out rows.
- Batch-013 focused on mixed client/server documentation transit:
  - AngularJS controller or client logic is the local source
  - Breeze/WebApi2 route and .NET Framework server behavior are accepted callee evidence
  - documentation must preserve AngularJS 1.4.8, Breeze/WebApi2, System.Web.Http 5.2.3, and net46 distinctions
- Latest batch-013 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=328.125
  - peak RAM MB=34.758
  - elapsed ms=308.485

## Runner Progress Update 6

- dotnet-csharp-runner-adapter batch 014 runner verified.
- Total prepared and tested: 700 train rows + 700 held-out rows.
- Batch-014 focused on ASP.NET Core 3.1 dynamic data-access documentation:
  - controller methods must not be documented without helper/data-access summaries
  - dynamic SQL and Elasticsearch query construction must be carried upward from helpers
  - local projects remain validation-only
- Latest batch-014 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=312.50
  - peak RAM MB=34.844
  - elapsed ms=312.355

## Runner Progress Update 7

- dotnet-csharp-runner-adapter batch 015 runner verified.
- Total prepared and tested: 750 train rows + 750 held-out rows.
- Batch-015 completed the 750-row checkpoint.
- Focus: auth/security/JWT documentation transit across ASP.NET Core 3.1 and legacy net46/WebApi2 code.
- Latest batch-015 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=359.375
  - peak RAM MB=35.152
  - elapsed ms=361.733

## Runner Progress Update 8

- dotnet-csharp-runner-adapter batch 016 runner verified.
- Total prepared and tested: 800 train rows + 800 held-out rows.
- Batch-016 focused on Excel/Elastic processing pipeline documentation:
  - controller/job entry points must carry meanings from ProcessLogic, converters, storage, notification, and Elasticsearch helpers
  - documentation remains version-aware for ASP.NET Core 3.1 and CSharp8
- Latest batch-016 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=437.50
  - peak RAM MB=35.711
  - elapsed ms=439.757

## Runner Progress Update 9

- dotnet-csharp-runner-adapter batch 017 runner verified.
- Total prepared and tested: 850 train rows + 850 held-out rows.
- Batch-017 focused on framework-generation contrast:
  - net46 WebApi2/Breeze controller shapes must not be documented as ASP.NET Core
  - netcoreapp3.1 ASP.NET Core controller shapes must not be documented as System.Web/WebApi2
  - both still require bottom-up route/persistence summaries
- Latest batch-017 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=312.50
  - peak RAM MB=35.289
  - elapsed ms=296.385

## Runner Progress Update 10

- dotnet-csharp-runner-adapter batch 018 runner verified.
- Total prepared and tested: 900 train rows + 900 held-out rows.
- Batch-018 focused on source-root stress:
  - vendor package files fail source gate
  - build output files fail source gate
  - net46 mislabeled as NET_CORE/CSharp8 fails version gate
  - valid ASP.NET Core controller rows still pass source, version, and transit gates
- Runner-001 caught malformed JSONL where empty arrays were serialized as `{}`. The file was fixed to `[]`, then runner-002 passed.
- Latest batch-018 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=296.875
  - peak RAM MB=33.988
  - elapsed ms=290.455

## Runner Progress Update 11

- dotnet-csharp-runner-adapter batch 019 runner verified.
- Total prepared and tested: 950 train rows + 950 held-out rows.
- Batch-019 was a positive recovery batch after source-root stress: valid authored files from all four local validation projects still route and pass gates.
- Latest batch-019 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=406.25
  - peak RAM MB=35.156
  - elapsed ms=391.369

## Runner Progress Update 12

- dotnet-csharp-runner-adapter batch 020 runner verified.
- First documentation stream complete: 1000 train rows + 1000 held-out rows across batches 001-020.
- Batch-020 was a balanced checkpoint:
  - valid dynamic-document workflow positives
  - vendor source-root negatives
  - net46-as-NET_CORE version negatives
  - all rows still route to the documentation unit, while gates distinguish pass/review correctly
- Latest batch-020 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=265.625
  - peak RAM MB=34.086
  - elapsed ms=264.760
- Next recommended stream: build call graph extraction from the local projects, then generate held-out rows from actual extracted caller/callee edges instead of hand-authored validation chains.

## Runner Progress Update 13

- Second stream started: `dotnet-csharp-callgraph-extraction`.
- Actual local call graph extraction produced:
  - 284 methods
  - 277 cleaned direct-name caller/callee edges
  - source projects: wizGameserver, GenieDynamicServer, geniesoftGameMVPServer, genietasks, askapediaweb
- Transit gate was strengthened in code: D3/D4 now require every `calls[].callee_symbol_id` to have a matching `known_callees[].symbol_id`; one known callee is no longer enough.
- CodeDocumentation targeted tests after the gate change: 10/10 passed.
- callgraph-extraction batch 001 runner verified.
- Batch-001 used 50 train rows and 50 held-out rows generated from actual extracted caller/callee edges.
- Latest callgraph batch-001 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=390.625
  - peak RAM MB=36.621
  - elapsed ms=394.434

## Runner Progress Update 14

- callgraph-extraction batch 002 runner verified.
- Total callgraph stream tested: 100 train rows + 100 held-out rows.
- Batch-002 was an exact negative transit batch:
  - held-out rows use actual extracted caller/callee edges
  - each row intentionally omits one distinct extracted callee from `known_callees`
  - expected result is `DOCUMENTATION_ROUTE_REVIEW_REQUIRED`
- Runner-001 exposed two bad negative examples where both call entries pointed to the same callee symbol, so the row was actually fully covered. The held-out set was regenerated to require two distinct callee symbols, then runner-002 passed.
- Latest callgraph batch-002 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=0.000
  - callee_summary_transit_accuracy=0.000
  - local_vs_delegated_accuracy=0.000
  - CPU ms=359.375
  - peak RAM MB=36.305
  - elapsed ms=354.843


## Runner Progress Update 15

- callgraph-extraction batch 003 runner verified.
- Total callgraph stream tested: 150 train rows + 150 held-out rows.
- Batch-003 mixed complete and partial callee coverage in one held-out batch:
  - complete rows require all extracted callees to be present in known_callees
  - partial rows intentionally omit one extracted callee and must route to review
- Latest callgraph batch-003 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=0.500
  - callee_summary_transit_accuracy=0.500
  - local_vs_delegated_accuracy=0.500
  - CPU ms=281.25
  - peak RAM MB=35.668
  - elapsed ms=289.364

## Runner Progress Update 16

- callgraph-extraction batch 004 runner verified.
- Total callgraph stream tested: 200 train rows + 200 held-out rows.
- Batch-004 stressed stack/version/source routing on actual extracted callgraph contexts:
  - valid source/version positives
  - generated/build-output source negatives
  - runtime-family/target-framework mismatch negatives
- Latest callgraph batch-004 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=359.375
  - peak RAM MB=36.348
  - elapsed ms=339.157

## Runner Progress Update 17

- callgraph-extraction batch 005 runner verified.
- Total callgraph stream tested: 250 train rows + 250 held-out rows.
- Batch-005 used only high-degree callers with 3+ distinct extracted callees:
  - complete rows pass
  - missing-one and missing-two callee rows fail transit as expected
  - wrong-version rows fail version gate while preserving route selection
- Latest callgraph batch-005 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=0.480
  - callee_summary_transit_accuracy=0.480
  - local_vs_delegated_accuracy=0.480
  - CPU ms=359.375
  - peak RAM MB=37.906
  - elapsed ms=347.432

## Runner Progress Update 18

- callgraph-extraction batch 006 runner verified.
- Total callgraph stream tested: 300 train rows + 300 held-out rows.
- Batch-006 verified documentation-level transit behavior:
  - D1 leaf/local rows pass without known_callees
  - D3 complete direct-dependency rows pass
  - D4 caller/workflow rows with no known_callees fail transit as expected
- Latest callgraph batch-006 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=0.740
  - callee_summary_transit_accuracy=0.740
  - local_vs_delegated_accuracy=0.740
  - CPU ms=343.75
  - peak RAM MB=36.094
  - elapsed ms=318.65

## Runner Progress Update 19

- callgraph-extraction batch 007 runner verified.
- Total callgraph stream tested: 350 train rows + 350 held-out rows.
- Batch-007 verified cross-project stack/version preservation:
  - wizGameserver: netcoreapp3.1 / CSharp8
  - GenieDynamicServer: netcoreapp3.1 / CSharp8
  - geniesoftGameMVPServer: netcoreapp3.1 / CSharp8 leaf-only validation
  - genietasks: .NET Framework v4.6 / CSharp5 legacy_non_sdk
  - askapediaweb: .NET Framework v3.5/v4.5.2/v4.6.1 / CSharp3-CSharp5 legacy_non_sdk validation
- Latest callgraph batch-007 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=375
  - peak RAM MB=35.746
  - elapsed ms=427.714

## Runner Progress Update 20

- callgraph-extraction batch 008 runner verified.
- Total callgraph stream tested: 400 train rows + 400 held-out rows.
- Batch-008 was the negative counterpart to batch-007:
  - .NET Core 3.1 projects mislabeled as NET_FRAMEWORK fail version gate
  - .NET Framework v4.6 project mislabeled as NET_CORE/CSharp8 fails version gate
  - documentation unit routing still remains correct
- Latest callgraph batch-008 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=343.75
  - peak RAM MB=35.82
  - elapsed ms=374.173

## Runner Progress Update 21

- callgraph-extraction batch 009 runner verified.
- Total callgraph stream tested: 450 train rows + 450 held-out rows.
- Batch-009 verified source-root exclusion across project contexts:
  - obj/bin generated output
  - Designer/generated files
  - packages/vendor paths
  - minified script/vendor paths
- Latest callgraph batch-009 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=312.5
  - peak RAM MB=35.586
  - elapsed ms=292.723

## Runner Progress Update 22

- callgraph-extraction batch 010 runner verified.
- First callgraph stream checkpoint complete: 500 train rows + 500 held-out rows.
- Batch-010 mixed all gate outcomes in one held-out run:
  - pass rows
  - source-fail rows
  - version-fail rows
  - transit-fail rows
  - multi-gate fail rows
- Latest callgraph batch-010 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=0.600
  - callee_summary_transit_accuracy=0.600
  - local_vs_delegated_accuracy=0.600
  - CPU ms=390.625
  - peak RAM MB=36.852
  - elapsed ms=358.012

## Runner Progress Update 23

- callgraph-extraction batch 011 runner verified.
- Total callgraph stream tested: 550 train rows + 550 held-out rows.
- Batch-011 added semantic project-role labels over extracted caller/callee contexts:
  - controller_action
  - startup_configuration
  - service_helper
  - domain_model_or_data_access
  - application_method
- Latest callgraph batch-011 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=390.625
  - peak RAM MB=36.434
  - elapsed ms=709.788

## Runner Progress Update 24

- callgraph-extraction batch 012 runner verified.
- Total callgraph stream tested: 600 train rows + 600 held-out rows.
- Batch-012 focused on DPO rejected quality rather than a new structural gate:
  - chosen side preserves correct semantic project role
  - rejected side is fluent but assigns the wrong role/layer
- Latest callgraph batch-012 metrics:
  - routing_accuracy=1.000
  - source_gate_accuracy=1.000
  - version_gate_accuracy=1.000
  - transit_gate_accuracy=1.000
  - bottom_up_order_accuracy=1.000
  - callee_summary_transit_accuracy=1.000
  - local_vs_delegated_accuracy=1.000
  - CPU ms=343.75
  - peak RAM MB=36.84
  - elapsed ms=325.242

## Runner Progress Update 25

- callgraph-extraction batch 013 runner verified.
- Total callgraph stream tested: 650 train rows + 650 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=437.5; peak RAM MB=37.297; elapsed ms=405.329.


## Runner Progress Update 26

- callgraph-extraction batch 014 runner verified.
- Total callgraph stream tested: 700 train rows + 700 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=359.375; peak RAM MB=37.656; elapsed ms=350.757.


## Runner Progress Update 27

- callgraph-extraction batch 015 runner verified.
- Total callgraph stream tested: 750 train rows + 750 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=359.375; peak RAM MB=36.223; elapsed ms=343.126.


## Runner Progress Update 28

- callgraph-extraction batch 016 runner verified.
- Total callgraph stream tested: 800 train rows + 800 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=390.625; peak RAM MB=37.004; elapsed ms=354.416.


## Runner Progress Update 29

- callgraph-extraction batch 017 runner verified.
- Total callgraph stream tested: 850 train rows + 850 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=359.375; peak RAM MB=37.145; elapsed ms=359.418.


## Runner Progress Update 30

- callgraph-extraction batch 018 runner verified.
- Total callgraph stream tested: 900 train rows + 900 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=359.375; peak RAM MB=37.215; elapsed ms=358.643.


## Runner Progress Update 31

- callgraph-extraction batch 019 runner verified.
- Total callgraph stream tested: 950 train rows + 950 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=36.914; elapsed ms=349.701.


## Runner Progress Update 32

- callgraph-extraction batch 020 runner verified.
- Total callgraph stream tested: 1000 train rows + 1000 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=37.137; elapsed ms=323.587.


## Runner Progress Update 33

- callgraph-extraction batch 021 runner verified.
- Total callgraph stream tested: 1050 train rows + 1050 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=375; peak RAM MB=36.477; elapsed ms=384.364.


## Runner Progress Update 34

- callgraph-extraction batch 022 runner verified.
- Total callgraph stream tested: 1100 train rows + 1100 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=484.375; peak RAM MB=38.844; elapsed ms=461.017.


## Runner Progress Update 35

- callgraph-extraction batch 023 runner verified.
- Total callgraph stream tested: 1150 train rows + 1150 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=36.781; elapsed ms=390.143.


## Runner Progress Update 36

- callgraph-extraction batch 024 runner verified.
- Total callgraph stream tested: 1200 train rows + 1200 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=36.785; elapsed ms=343.543.


## Runner Progress Update 37

- callgraph-extraction batch 025 runner verified.
- Total callgraph stream tested: 1250 train rows + 1250 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=343.75; peak RAM MB=37.129; elapsed ms=318.296.


## Runner Progress Update 38

- callgraph-extraction batch 026 runner verified.
- Total callgraph stream tested: 1300 train rows + 1300 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=37.176; elapsed ms=290.335.


## Runner Progress Update 39

- callgraph-extraction batch 027 runner verified.
- Total callgraph stream tested: 1350 train rows + 1350 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=37.43; elapsed ms=336.882.


## Runner Progress Update 40

- callgraph-extraction batch 028 runner verified.
- Total callgraph stream tested: 1400 train rows + 1400 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=375; peak RAM MB=36.945; elapsed ms=337.301.


## Runner Progress Update 41

- callgraph-extraction batch 029 runner verified.
- Total callgraph stream tested: 1450 train rows + 1450 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=375; peak RAM MB=37.211; elapsed ms=429.858.


## Runner Progress Update 42

- callgraph-extraction batch 030 runner verified.
- Total callgraph stream tested: 1500 train rows + 1500 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=390.625; peak RAM MB=37.348; elapsed ms=363.89.


## Runner Progress Update 43

- callgraph-extraction batch 031 runner verified.
- Total callgraph stream tested: 1550 train rows + 1550 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=359.375; peak RAM MB=37.191; elapsed ms=376.357.


## Runner Progress Update 44

- callgraph-extraction batch 032 runner verified.
- Total callgraph stream tested: 1600 train rows + 1600 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=36.84; elapsed ms=485.21.


## Runner Progress Update 45

- callgraph-extraction batch 033 runner verified.
- Total callgraph stream tested: 1650 train rows + 1650 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=375; peak RAM MB=37.082; elapsed ms=345.176.


## Runner Progress Update 46

- callgraph-extraction batch 034 runner verified.
- Total callgraph stream tested: 1700 train rows + 1700 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=296.875; peak RAM MB=36.559; elapsed ms=287.678.


## Runner Progress Update 47

- callgraph-extraction batch 035 runner verified.
- Total callgraph stream tested: 1750 train rows + 1750 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=390.625; peak RAM MB=35.93; elapsed ms=427.77.


## Runner Progress Update 48

- callgraph-extraction batch 036 runner verified.
- Total callgraph stream tested: 1800 train rows + 1800 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=37.664; elapsed ms=447.296.


## Runner Progress Update 49

- callgraph-extraction batch 037 runner verified.
- Total callgraph stream tested: 1850 train rows + 1850 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=421.875; peak RAM MB=36.961; elapsed ms=385.677.


## Runner Progress Update 50

- callgraph-extraction batch 038 runner verified.
- Total callgraph stream tested: 1900 train rows + 1900 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=390.625; peak RAM MB=37.035; elapsed ms=360.085.


## Runner Progress Update 51

- callgraph-extraction batch 039 runner verified.
- Total callgraph stream tested: 1950 train rows + 1950 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=390.625; peak RAM MB=37.031; elapsed ms=391.194.


## Runner Progress Update 52

- callgraph-extraction batch 040 runner verified.
- Total callgraph stream tested: 2000 train rows + 2000 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=390.625; peak RAM MB=37.023; elapsed ms=379.494.


## Runner Progress Update 53

- callgraph-extraction batch 041 runner verified.
- Total callgraph stream tested: 2050 train rows + 2050 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=421.875; peak RAM MB=37.113; elapsed ms=429.186.


## Runner Progress Update 54

- callgraph-extraction batch 042 runner verified.
- Total callgraph stream tested: 2100 train rows + 2100 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=359.375; peak RAM MB=37.074; elapsed ms=366.678.


## Runner Progress Update 55

- callgraph-extraction batch 043 runner verified.
- Total callgraph stream tested: 2150 train rows + 2150 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=343.75; peak RAM MB=36.996; elapsed ms=365.508.


## Runner Progress Update 56

- callgraph-extraction batch 044 runner verified.
- Total callgraph stream tested: 2200 train rows + 2200 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=37.008; elapsed ms=317.692.


## Runner Progress Update 57

- callgraph-extraction batch 045 runner verified.
- Total callgraph stream tested: 2250 train rows + 2250 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=37.41; elapsed ms=453.644.


## Runner Progress Update 58

- callgraph-extraction batch 046 runner verified.
- Total callgraph stream tested: 2300 train rows + 2300 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=37.461; elapsed ms=321.019.


## Runner Progress Update 59

- callgraph-extraction batch 047 runner verified.
- Total callgraph stream tested: 2350 train rows + 2350 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=421.875; peak RAM MB=37.445; elapsed ms=494.629.


## Runner Progress Update 60

- callgraph-extraction batch 048 runner verified.
- Total callgraph stream tested: 2400 train rows + 2400 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=359.375; peak RAM MB=36.457; elapsed ms=343.982.


## Runner Progress Update 61

- callgraph-extraction batch 049 runner verified.
- Total callgraph stream tested: 2450 train rows + 2450 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=468.75; peak RAM MB=36.777; elapsed ms=604.746.


## Runner Progress Update 62

- callgraph-extraction batch 050 runner verified.
- Total callgraph stream tested: 2500 train rows + 2500 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=36.711; elapsed ms=304.267.


## Runner Progress Update 63

- callgraph-extraction batch 051 runner verified.
- Total callgraph stream tested: 2550 train rows + 2550 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=390.625; peak RAM MB=36.383; elapsed ms=373.851.


## Runner Progress Update 64

- callgraph-extraction batch 052 runner verified.
- Total callgraph stream tested: 2600 train rows + 2600 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=36.699; elapsed ms=612.363.


## Runner Progress Update 65

- callgraph-extraction batch 053 runner verified.
- Total callgraph stream tested: 2650 train rows + 2650 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=375; peak RAM MB=36.605; elapsed ms=337.041.


## Runner Progress Update 66

- callgraph-extraction batch 054 runner verified.
- Total callgraph stream tested: 2700 train rows + 2700 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=421.875; peak RAM MB=37; elapsed ms=506.036.


## Runner Progress Update 67

- callgraph-extraction batch 055 runner verified.
- Total callgraph stream tested: 2750 train rows + 2750 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=359.375; peak RAM MB=37.184; elapsed ms=344.302.


## Runner Progress Update 68

- callgraph-extraction batch 056 runner verified.
- Total callgraph stream tested: 2800 train rows + 2800 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=421.875; peak RAM MB=36.789; elapsed ms=447.862.


## Runner Progress Update 69

- callgraph-extraction batch 057 runner verified.
- Total callgraph stream tested: 2850 train rows + 2850 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=37.043; elapsed ms=301.407.


## Runner Progress Update 70

- callgraph-extraction batch 058 runner verified.
- Total callgraph stream tested: 2900 train rows + 2900 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=453.125; peak RAM MB=37.43; elapsed ms=458.014.


## Runner Progress Update 71

- callgraph-extraction batch 059 runner verified.
- Total callgraph stream tested: 2950 train rows + 2950 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=515.625; peak RAM MB=37.359; elapsed ms=448.346.


## Runner Progress Update 72

- callgraph-extraction batch 060 runner verified.
- Total callgraph stream tested: 3000 train rows + 3000 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=37.66; elapsed ms=447.444.


## Runner Progress Update 73

- callgraph-extraction batch 061 runner verified.
- Total callgraph stream tested: 3050 train rows + 3050 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=36.75; elapsed ms=314.665.


## Runner Progress Update 74

- callgraph-extraction batch 062 runner verified.
- Total callgraph stream tested: 3100 train rows + 3100 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=343.75; peak RAM MB=36.457; elapsed ms=321.116.


## Runner Progress Update 75

- callgraph-extraction batch 063 runner verified.
- Total callgraph stream tested: 3150 train rows + 3150 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=375; peak RAM MB=37.023; elapsed ms=347.611.


## Runner Progress Update 76

- callgraph-extraction batch 064 runner verified.
- Total callgraph stream tested: 3200 train rows + 3200 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=421.875; peak RAM MB=37.125; elapsed ms=476.252.


## Runner Progress Update 77

- callgraph-extraction batch 065 runner verified.
- Total callgraph stream tested: 3250 train rows + 3250 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=36.555; elapsed ms=310.091.


## Runner Progress Update 78

- callgraph-extraction batch 066 runner verified.
- Total callgraph stream tested: 3300 train rows + 3300 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=390.625; peak RAM MB=36.91; elapsed ms=367.747.


## Runner Progress Update 79

- callgraph-extraction batch 067 runner verified.
- Total callgraph stream tested: 3350 train rows + 3350 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=390.625; peak RAM MB=37.086; elapsed ms=415.205.


## Runner Progress Update 80

- callgraph-extraction batch 068 runner verified.
- Total callgraph stream tested: 3400 train rows + 3400 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=37.605; elapsed ms=403.258.


## Runner Progress Update 81

- callgraph-extraction batch 069 runner verified.
- Total callgraph stream tested: 3450 train rows + 3450 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=343.75; peak RAM MB=36.883; elapsed ms=362.224.


## Runner Progress Update 82

- callgraph-extraction batch 070 runner verified.
- Total callgraph stream tested: 3500 train rows + 3500 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=296.875; peak RAM MB=36.547; elapsed ms=286.179.


## Runner Progress Update 83

- callgraph-extraction batch 071 runner verified.
- Total callgraph stream tested: 3550 train rows + 3550 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=343.75; peak RAM MB=36.902; elapsed ms=363.428.


## Runner Progress Update 84

- callgraph-extraction batch 072 runner verified.
- Total callgraph stream tested: 3600 train rows + 3600 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=36.742; elapsed ms=296.108.


## Runner Progress Update 85

- callgraph-extraction batch 073 runner verified.
- Total callgraph stream tested: 3650 train rows + 3650 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=437.5; peak RAM MB=37.488; elapsed ms=485.281.


## Runner Progress Update 86

- callgraph-extraction batch 074 runner verified.
- Total callgraph stream tested: 3700 train rows + 3700 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=296.875; peak RAM MB=36.523; elapsed ms=306.884.


## Runner Progress Update 87

- callgraph-extraction batch 075 runner verified.
- Total callgraph stream tested: 3750 train rows + 3750 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=36.66; elapsed ms=333.097.


## Runner Progress Update 88

- callgraph-extraction batch 076 runner verified.
- Total callgraph stream tested: 3800 train rows + 3800 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=36.832; elapsed ms=331.789.


## Runner Progress Update 89

- callgraph-extraction batch 077 runner verified.
- Total callgraph stream tested: 3850 train rows + 3850 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=421.875; peak RAM MB=37.836; elapsed ms=453.634.


## Runner Progress Update 90

- callgraph-extraction batch 078 runner verified.
- Total callgraph stream tested: 3900 train rows + 3900 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=37.168; elapsed ms=344.586.


## Runner Progress Update 91

- callgraph-extraction batch 079 runner verified.
- Total callgraph stream tested: 3950 train rows + 3950 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=484.375; peak RAM MB=36.832; elapsed ms=494.357.


## Runner Progress Update 92

- callgraph-extraction batch 080 runner verified.
- Total callgraph stream tested: 4000 train rows + 4000 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=36.375; elapsed ms=304.806.


## Runner Progress Update 93

- callgraph-extraction batch 081 runner verified.
- Total callgraph stream tested: 4050 train rows + 4050 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=453.125; peak RAM MB=37.191; elapsed ms=445.981.


## Runner Progress Update 94

- callgraph-extraction batch 082 runner verified.
- Total callgraph stream tested: 4100 train rows + 4100 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=375; peak RAM MB=37.41; elapsed ms=423.141.


## Runner Progress Update 95

- callgraph-extraction batch 083 runner verified.
- Total callgraph stream tested: 4150 train rows + 4150 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=36.453; elapsed ms=430.801.


## Runner Progress Update 96

- callgraph-extraction batch 084 runner verified.
- Total callgraph stream tested: 4200 train rows + 4200 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=437.5; peak RAM MB=36.813; elapsed ms=468.372.


## Runner Progress Update 97

- callgraph-extraction batch 085 runner verified.
- Total callgraph stream tested: 4250 train rows + 4250 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=343.75; peak RAM MB=36.879; elapsed ms=358.45.


## Runner Progress Update 98

- callgraph-extraction batch 086 runner verified.
- Total callgraph stream tested: 4300 train rows + 4300 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=296.875; peak RAM MB=37.375; elapsed ms=304.583.


## Runner Progress Update 99

- callgraph-extraction batch 087 runner verified.
- Total callgraph stream tested: 4350 train rows + 4350 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=375; peak RAM MB=37.277; elapsed ms=396.941.


## Runner Progress Update 100

- callgraph-extraction batch 088 runner verified.
- Total callgraph stream tested: 4400 train rows + 4400 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=296.875; peak RAM MB=36.75; elapsed ms=318.868.


## Runner Progress Update 101

- callgraph-extraction batch 089 runner verified.
- Total callgraph stream tested: 4450 train rows + 4450 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=36.805; elapsed ms=318.873.


## Runner Progress Update 102

- callgraph-extraction batch 090 runner verified.
- Total callgraph stream tested: 4500 train rows + 4500 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=359.375; peak RAM MB=36.637; elapsed ms=348.658.


## Runner Progress Update 103

- callgraph-extraction batch 091 runner verified.
- Total callgraph stream tested: 4550 train rows + 4550 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=421.875; peak RAM MB=36.684; elapsed ms=538.868.


## Runner Progress Update 104

- callgraph-extraction batch 092 runner verified.
- Total callgraph stream tested: 4600 train rows + 4600 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=36.211; elapsed ms=289.805.


## Runner Progress Update 105

- callgraph-extraction batch 093 runner verified.
- Total callgraph stream tested: 4650 train rows + 4650 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=37.297; elapsed ms=320.664.


## Runner Progress Update 106

- callgraph-extraction batch 094 runner verified.
- Total callgraph stream tested: 4700 train rows + 4700 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=36.555; elapsed ms=311.295.


## Runner Progress Update 107

- callgraph-extraction batch 095 runner verified.
- Total callgraph stream tested: 4750 train rows + 4750 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=406.25; peak RAM MB=37.5; elapsed ms=391.282.


## Runner Progress Update 108

- callgraph-extraction batch 096 runner verified.
- Total callgraph stream tested: 4800 train rows + 4800 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=468.75; peak RAM MB=37.594; elapsed ms=767.053.


## Runner Progress Update 109

- callgraph-extraction batch 097 runner verified.
- Total callgraph stream tested: 4850 train rows + 4850 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=312.5; peak RAM MB=36.723; elapsed ms=311.666.


## Runner Progress Update 110

- callgraph-extraction batch 098 runner verified.
- Total callgraph stream tested: 4900 train rows + 4900 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=343.75; peak RAM MB=36.871; elapsed ms=351.897.


## Runner Progress Update 111

- callgraph-extraction batch 099 runner verified.
- Total callgraph stream tested: 4950 train rows + 4950 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=437.5; peak RAM MB=37.605; elapsed ms=440.392.


## Runner Progress Update 112

- callgraph-extraction batch 100 runner verified.
- Total callgraph stream tested: 5000 train rows + 5000 held-out rows.
- routing_accuracy=1.000; source_gate_accuracy=1.000; version_gate_accuracy=1.000; transit_gate_accuracy=1.000.
- CPU ms=328.125; peak RAM MB=36.707; elapsed ms=292.259.

## Runner Progress Update 2026-05-14 - Framework Lifecycle Coverage

- `geniesoftGameMVPServer` was upgraded from leaf-only validation to explicit
  ASP.NET Core lifecycle validation.
- Inventory now includes 444 methods and 430 callgraph/lifecycle edges.
- Added lifecycle support rows for `Program.Main`, `Program.CreateHostBuilder`,
  `Startup.ConfigureServices`, `Startup.Configure`,
  `WeatherForecastController.Get`, controller activation, and
  `WeatherForecast.TemperatureF.get`.
- `CodeDocumentationRoutingFlow` now emits `framework-lifecycle-summary` and
  classifies such rows as `framework_lifecycle_root`.
- Clean lifecycle-support batches 706-720 runner verified:
  - latest accepted batch: 720
  - callgraph stream totals: 36,000 train rows + 36,000 held-out rows
  - routing/source/version/transit accuracy: 1.000
  - latest runner CPU ms: 296.875
  - latest runner peak RAM MB: 35.563
  - latest quality gate: EXCELLENT, score 1.000, hard failures 0

## Runner Progress Update 2026-05-14 - External Source Proof Stream

- Started `external-dotnet-framework-lifecycle-source-proof`.
- Sources are fetched with exact commit SHA and license proof rows:
  - `dotnet/AspNetCore.Docs` at
    `acba8a58d2c2d36bf058ea955dbae9ae04bc2195`, MIT code samples via
    `LICENSE-CODE`;
  - `dotnet-architecture/eShopModernizing` at
    `63bc9ec4414d7e281dc9f9d7bdcf70030950457d`, MIT via `LICENSE`;
  - `aspnet/AspNetWebStack` at
    `af67fe18a182e1013482a7a088bae523a555c25c`, Apache-2.0 via
    `LICENSE.txt`.
- Extracted external source units: 115.
- Source proof rows per batch: 17.
- External batches 001-005 runner verified:
  - latest accepted batch: 005
  - external stream totals: 250 train rows + 250 held-out rows
  - routing/source/version/transit accuracy: 1.000
  - latest bottom-up/callee/local-vs-delegated accuracy: 0.900
  - latest CPU ms: 437.5
  - latest peak RAM MB: 36.625
- First failed run caught a bad partial-transit negative where the row had only
  one callee and therefore could not be incomplete. The generator now downgrades
  such rows to positive instead of fabricating a missing-callee failure.

## Runner Progress Update 2026-05-14 - 500-Row Checkpoint Rule

The active execution rule is now:

- every 500 new train rows, run a project documentation quality checkpoint;
- the checkpoint must include the five local validation projects;
- report CPU, RAM, routing/source/version/transit accuracy, and documentation
  quality.
- after each 10 accepted batches, also run full inventory coverage and depth
  coverage:
  - inventory coverage: methods/edges covered at least once;
  - depth coverage: missing positive-ready methods, missing negative-gate
    methods, missing D4 caller/workflow methods, missing positive-ready edges,
    and missing missing-callee negative edges.
- after each 10 accepted batches, compare the new coverage audit to the
  previous 10-batch checkpoint and report improvements explicitly:
  - inventory improvements: method coverage delta and edge coverage delta;
  - depth improvements: reduced missing positive-ready methods, reduced missing
    negative-gate methods, reduced missing D4 caller/workflow methods, reduced
    missing positive-ready edges, and reduced missing missing-callee negative
    edges;
  - if no coverage improved, report `coverage_improved=false` and
    `depth_improved=false` rather than implying progress.
- if a checkpoint has no coverage/depth improvement, run
  `learning-runs internal sets/analyze_code_doc_depth_gaps.ps1` and attach the
  diagnosis to the checkpoint:
  - identify whether missing positive-ready rows are missing source bodies or
    only missing targeted positive examples;
  - split D4/workflow gaps into real caller gaps vs leaf/callee propagation
    metric gaps;
  - count leaf/callee symbols already propagated through `known_callees` in D4
    caller rows;
  - split missing-callee negative edge gaps into single-callee callers that need
    explicit empty-transit negatives and multi-callee callers that need partial
    transit negatives;
  - name the missing resource/change before generating more batches.
- after each 10 accepted batches, also record normalized cost metrics:
  - runner/training cost per measured row for the external and local streams,
    where measured rows = train rows + held-out rows;
  - CPU ms/row, elapsed ms/row, average peak RAM MB per batch, and normalized
    peak RAM MB/row;
  - documentation quality/generation cost per evaluated row;
  - documentation generation cost per source code line evaluated;
  - documentation generation cost per estimated generated token.
- Treat peak RAM per row/token as a normalized pressure metric only. Peak RAM is
  not cumulative like CPU or elapsed time, so the checkpoint must also preserve
  average peak RAM per batch.
- Use `learning-runs internal sets/run_code_doc_10_batch_checkpoint.ps1` for
  this whole protocol so the 10-batch checkpoint is not forgotten.

Checkpoint A:

- external source-proof stream reached 500 train rows + 500 held-out rows at
  batch 010;
- local callgraph stream reached batch 730 afterward;
- batch 730 documentation quality: EXCELLENT, score 1.000, hard failures 0;
- batch 730 quality CPU/RAM: 359.375 ms / 122.02 MB;
- held-out project coverage: GenieDynamicServer 14, genietasks 5,
  geniesoftGameMVPServer 8, askapediaweb 16, wizGameserver 7.

Checkpoint B:

- external source-proof stream reached 1000 train rows + 1000 held-out rows at
  batch 020;
- local callgraph stream reached batch 740 afterward;
- batch 740 documentation quality: EXCELLENT, score 1.000, hard failures 0;
- batch 740 quality CPU/RAM: 312.5 ms / 126.023 MB;
- held-out project coverage: askapediaweb 16, geniesoftGameMVPServer 10,
  genietasks 6, wizGameserver 8, GenieDynamicServer 10.

Checkpoint C:

- external source-proof stream reached 1500 train rows + 1500 held-out rows at
  batch 030;
- local callgraph stream reached batch 750 afterward;
- batch 750 documentation quality: EXCELLENT, score 1.000, hard failures 0;
- batch 750 runner KPI: routing/source/version/transit accuracy all 1.000,
  CPU 296.875 ms, peak RAM 35.676 MB;
- batch 750 quality KPI: CPU 656.25 ms, peak RAM 121.977 MB;
- held-out project coverage: askapediaweb 12, GenieDynamicServer 15,
  geniesoftGameMVPServer 9, genietasks 7, wizGameserver 7.

Checkpoint D:

- external source-proof stream reached 2000 train rows + 2000 held-out rows at
  batch 040;
- local callgraph stream reached batch 760 afterward;
- batch 760 documentation quality: EXCELLENT, score 1.000, hard failures 0;
- batch 760 runner KPI: routing/source/version/transit accuracy all 1.000,
  CPU 296.875 ms, peak RAM 35.617 MB;
- batch 760 quality KPI: CPU 328.125 ms, peak RAM 122.539 MB;
- held-out project coverage: askapediaweb 17, genietasks 7,
  geniesoftGameMVPServer 8, wizGameserver 9, GenieDynamicServer 9.

