# 00 - Index

## Source Document

`C:\Users\lubao\Documents\xiigen\llm mvp\project_skills_v4\all-skills\escalation-orchestrator-SKILL.md`

- Relative path: `llm mvp\project_skills_v4\all-skills\escalation-orchestrator-SKILL.md`
- Manifest category: undefined
- Top folder: llm mvp
- Type: testing and simulation
- Inventory type: not listed in attached MVP inventory
- File size: 2397 bytes
- Line count read: 61
- SHA256: 5E4269FCCE51DF42DFE43C96EF41615BCD637838DFB4FDD8AE348ECD7867396D
- First meaningful line: `escalation-orchestrator-SKILL.md v1.0 — Decision rules when arbiter panel has mixed verdicts`
- Last meaningful line: `Missing evidence for FC-1 count drift check Rule 4: gather first Run then re-evaluate`

## Related Versions And Duplicate Copies

- No duplicate copy with the same content hash was recorded in the manifest.

## Source Meaning Used For Training

escalation-orchestrator-SKILL.md v1.0 — Decision rules when arbiter panel has mixed verdicts

## Domains This Document Teaches

- test objective
- scenario setup
- expected behavior
- failure evidence
- coverage decision
- release gate
- simulation result
- repair signal
- the principle translated from planning--escalation-orchestrator
- the 6 decision rules apply in order — stop at first match
- rule 1: any block → reject
- rule 2: majority pass + all fail are warn → conditional approval
- rule 3: multiple fail in same category → structural problem
- rule 4: context insufficient → gather first

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
