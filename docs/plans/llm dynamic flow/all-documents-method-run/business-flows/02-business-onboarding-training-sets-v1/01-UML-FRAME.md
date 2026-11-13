# 01 - UML Frame

Source diagram:
`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\improve algo.drawio.xml`

## Active Nodes

1. User prompt
2. Node-01 Domain splitter: raw user prompt -> domain labels d1...dm
3. Node-02 Domain translator array: one prompt fragment plus one domain -> domain wording udj
4. Node-03 Scheme planner: all domain wordings -> planned pieces b1...bn and prompts fb1...fbn
5. Node-04 Domain processor array: each planned piece -> processed domain result
6. Node-05 Applier array: each fbi(bi) -> concrete result Rqi
7. Node-06 Result translator array: each Rqi -> natural-language fragment, with all R1...Rn visible for consistency
8. Node-07 Arbiter selector: formal results R1...Rn plus context -> selected arbiters
9. Node-08 Arbiter units: selected arbiter plus result/result group -> pass, concern, or fbx reprocess instruction
10. Node-09 Formatter: approved translated fragments -> one final answer

## Edge Inventory

| Edge | What moves | Target receives | Target must produce | What breaks if missing |
|---|---|---|---|---|
| User prompt -> Node-01 | Human request | Raw prompt | Domain label array | The flow cannot route the request |
| Node-01 -> Node-02 | Domain labels and fragments | One domain fragment per translator unit | Formal wording per domain | The rest of the flow gets vague human language |
| Node-02 -> Node-03 | ud1...udm | All formal domain wordings | b1...bn and fb1...fbn | Parallel onboarding work is not planned |
| Node-03 -> Node-04 | Planned pieces | One planned piece per processor unit | Processed success, failure, and wait-state outcomes | The applier cannot act safely |
| Node-04 -> Node-05 | Processed sub-task result | One processed result plus its prompt | Concrete Rqi | No real outcome exists |
| Node-05 -> Node-06 | Formal results R1...Rn | One formal result per result translator, plus full collection | User-language fragment or consistency flag | Final answer stays internal or contradictory |
| Node-05 -> Node-07 | Formal result collection | R1...Rn plus flow context | Selected arbiter units | Review is missing or too broad |
| Node-07 -> Node-08 | Review responsibilities | Arbiter specialization plus result | Pass or flag with fbx | Risk-specific review is not performed |
| Node-08 -> Node-01 | fbx correction fragment | Reprocessing input | Corrected routing and processing | Failed fragments cannot be repaired |
| Node-06 and Node-08 -> Node-09 | Approved translated fragments | Only approved user-language content | One audience-shaped final answer | Formatter can mix unchecked or rejected content |

This file is shared by all node files in this package.
