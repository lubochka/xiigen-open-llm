# P5A candidate seed supplement

Эти examples не training-ready. Они нужны только, чтобы parent review видел, какие positive/negative boundaries можно позже превратить в DPO rows без stack/project leakage.

Граница source-state после repair:

- только `AGENTS.md` пришел из analysis-state как `done`;
- остальные пять источников явно названы Luba/current plan, но parent verified их как `todo` в analysis-state;
- эти пять строк являются только `named_reference` evidence и не могут стать training-ready, пока источник не будет analyzed/accepted или parent явно не примет его как named-reference evidence для revised split drafting.

## `P5A-CAND-PTT-AUTHORITY-RULES-001`

Было в источнике: rules-документ с API/service conventions and technical stack details.

Source-state: analysis-state `done`; parent review все еще нужен перед использованием в revised split.

Универсальная граница: из правил нужно извлечь task contract: inputs, result boundary, forbidden actions, allowed verification and evidence scope.

Chosen: сформировать stack-free task contract before execution.

Rejected: копировать service registration, namespace, index names or choose action by `AGENTS.md` title.

Что проверяет ablation: модель не должна проходить по filename, `.NET`, C#, service names or code snippets.

## `P5A-CAND-BOTH-SKILLS-INVENTORY-001`

Было в источнике: анализ skill library as AI extension, with universal core and project adapter sections.

Source-state: analysis-state `todo`; это named reference from Luba/current plan, а не done source. Строка остается candidate-only и not training-ready.

Универсальная граница: reusable session/skill/evidence discipline отдельно от project adapter.

Chosen: выделить universal core, а stack commands and project map оставить adapter-only.

Rejected: объявить все skill names universal or bake project commands into common engine.

Что проверяет ablation: после удаления `.NET`, C#, `OperationResult`, paths and exact skill filenames должно остаться universal-core-vs-adapter reasoning.

## `P5A-CAND-ORCH-DOC07956-001`

Было в источнике: universal orchestrator package with splitter, translators, planner, processors, arbiters and formatter.

Source-state: analysis-state `todo`; это named reference from Luba/current plan, а не done source. Строка остается candidate-only и not training-ready.

Универсальная граница: branches stay separate, merge waits for evidence/review, reviewers run before formatting, failed fragment returns alone.

Chosen: для multi-domain task keep branches separate and repair only failed branch.

Rejected: one happy path, review after formatting, source-document line as runtime input.

Что проверяет ablation: удалить folder/title/node labels; success must rely on branch/review/targeted-repair boundary.

## `P5A-CAND-ORCH-UNIVERSAL-DEPS-001`

Было в источнике: dependency model with root/head/children/relation mechanics.

Source-state: analysis-state `todo`; это named reference from Luba/current plan, а не done source. Строка остается candidate-only и not training-ready.

Универсальная граница: orchestration can reason over root decision, dependent evidence and relation labels.

Chosen: find main decision and dependent gates before sequencing.

Rejected: route by `UniversalDependencies.cs`, `GetRoot`, `GetHead`, parser mechanics or relation token syntax.

Что проверяет ablation: remove C# and CoNLL-U clues; remaining example must still need dependency/relation reasoning.

## `P5A-CAND-ORCH-NINE-NODE-FLOW-001`

Было в источнике: implementation of an ordered station flow.

Source-state: analysis-state `todo`; это named reference from Luba/current plan, а не done source. Строка остается candidate-only и not training-ready.

Универсальная граница: split, translate, plan, process/apply, translate result, select review, review, format; evidence is carried forward.

Chosen: stage-gated orchestration with review before final claim.

Rejected: hardcode exactly nine nodes or use implementation bridge/class names as the answer.

Что проверяет ablation: renamed phases and no C# identifiers must still produce station-chain reasoning.

## `P5A-CAND-ORCH-FLOW-ISOLATION-001`

Было в источнике: tests for shared flow/node instances, concurrent calls, cross-batch isolation and deduplication.

Source-state: analysis-state `todo`; это named reference from Luba/current plan, а не done source. Строка остается candidate-only и not training-ready.

Универсальная граница: reusable orchestration needs isolation, dedup and no cross-run contamination evidence before claim.

Chosen: require no state bleed and duplicate-work control before readiness claim.

Rejected: accept because `Assert` passed, because a singleton exists, or because a test name says universal.

Что проверяет ablation: remove test framework, run ids and temp paths; row must still require isolation evidence.
