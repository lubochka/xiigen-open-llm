# 01 - UML Frame

Source diagram:
`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\improve algo.drawio.xml`

This frame is the mapping contract for the node files in this package.

## Active Nodes

1. User prompt
2. Node-01 Domain splitter: raw user prompt -> domain labels d1...dm
3. Node-02 Domain translator array: each prompt fragment plus one domain -> domain wording udj
4. Node-03 Scheme planner: all domain wordings -> planned pieces b1...bn and prompts fb1...fbn
5. Node-04 Domain processor array: each dbi/bi -> processed domain result
6. Node-05 Applier array: each fbi(bi) -> concrete result Rqi
7. Node-06 Result translator array: each Rbi/Rqi -> user-language result, with all R1...Rn visible for consistency
8. Node-07 Arbiter selector: formal results R1...Rn plus context -> selected arbiters
9. Node-08 Arbiter units: selected arbiter plus result/result group -> pass, concern, or fbx reprocess instruction
10. Node-09 Formatter: approved translated results -> one user-likable final output

## Edge Inventory

| Edge | What moves | Target receives | Target must produce | What breaks if missing |
|---|---|---|---|---|
| User prompt -> Node-01 | The human request | Raw natural-language prompt | Domain label array | The whole flow has no territory map |
| Node-01 -> Node-02 | d1...dm plus prompt fragments | One fragment and one domain per translator unit | Formal domain wording | Domain units get vague user language instead of precise work language |
| Node-02 -> Node-03 | ud1...udm | All formal domain wordings together | b1...bn plus fb1...fbn | Work is not decomposed into runnable pieces |
| Node-03 -> Node-04 | b1...bn/db1...dbn | One planned piece per processor unit | Fully processed sub-task result | The applier lacks success, failure, and wait-state outputs |
| Node-04 -> Node-05 | Processed sub-task result | One processed result plus its transformation prompt | Concrete Rqi | The flow has analysis but no applied outcome |
| Node-05 -> Node-06 | Rb1...Rbn/Rq1...Rqn | One formal result per result-translator unit plus full result collection | Natural-language fragment or consistency flag | The final answer stays too formal or contradictory |
| Node-05 -> Node-07 | R1...Rn plus flow context | Formal result collection | Selected arbiter units | Unsafe or inconsistent results may reach formatter |
| Node-07 -> Node-08 | Selected arbiter responsibilities | One arbiter specialization plus relevant result/result group | Pass, concern, or fbx reprocess instruction | Review is not targeted to actual risk |
| Node-08 -> Node-01 | fbx reprocess fragment when needed | Corrective fragment as new front-of-flow input | Reprocessed domain route | Failed fragments cannot be repaired |
| Node-06 + Node-08 -> Node-09 | Approved translated fragments | Only translated fragments that passed arbitration | One final output for the audience | Formatter may mix raw, unchecked, or rejected content |

## Package Status

This UML frame is shared by all node files in this package. A node pair that cannot be attached to one of these edges does not belong in this package.
