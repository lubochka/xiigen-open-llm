# 00 - Index

## Source Document

`C:\Users\lubao\Documents\xiigen\llm mvp\project_skills_v4\all-skills\cross-component-conflict-SKILL.md`

- Relative path: `llm mvp\project_skills_v4\all-skills\cross-component-conflict-SKILL.md`
- Manifest category: undefined
- Top folder: llm mvp
- Type: testing and simulation
- Inventory type: not listed in attached MVP inventory
- File size: 2412 bytes
- Line count read: 63
- SHA256: 8451D295D2986A32B7CB97FDAAF154B2CE3D1E7494CB14293853415F8EF08EF8
- First meaningful line: `cross-component-conflict-SKILL.md v1.0 — Detect interface contract collisions between subprojects`
- Last meaningful line: `29.SemanticCompiler/Core/Core.cs vs 21.AgentInfrastructure/Core/Core.cs Both define OperationResult<T and BaseService Use project reference to resolve; never mix namespaces`

## Related Versions And Duplicate Copies

- No duplicate copy with the same content hash was recorded in the manifest.

## Source Meaning Used For Training

cross-component-conflict-SKILL.md v1.0 — Detect interface contract collisions between subprojects

## Domains This Document Teaches

- test objective
- scenario setup
- expected behavior
- failure evidence
- coverage decision
- release gate
- simulation result
- repair signal
- translated from: xiigen bfa behavior/function/artifact conflict detection
- the principle
- when to invoke
- conflict detection protocol
- step 1: find all implementations of the changed interface
- step 2: find all callers of the changed algorithm

## Package Files

- `01-UML-FRAME.md`
- `node-01-domain-splitter-set.md`
- `node-02-domain-translator-set.md`
- `node-03-scheme-planner-set.md`
- `node-04-domain-processor-set.md`
- `node-05-applier-set.md`
- `node-06-result-translator-set.md`
- `node-07-arbiter-selector-set.md`
- `node-08-arbiter-units-set.md`
- `node-09-formatter-set.md`
- `10-SIMULATION-CHECK.md`
