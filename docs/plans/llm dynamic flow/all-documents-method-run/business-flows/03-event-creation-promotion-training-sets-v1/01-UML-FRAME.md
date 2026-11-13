# 01 - UML Frame

Source diagram:
`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\improve algo.drawio.xml`

## Active Nodes

1. User prompt
2. Node-01 Domain splitter: raw user prompt -> domain labels d1...dm
3. Node-02 Domain translator array: one prompt fragment plus one domain -> formal domain wording
4. Node-03 Scheme planner: all domain wordings -> planned pieces b1...bn and prompts fb1...fbn
5. Node-04 Domain processor array: one planned piece -> processed result
6. Node-05 Applier array: processed piece plus prompt -> Rqi
7. Node-06 Result translator array: one Rqi -> natural-language fragment, with all R1...Rn visible
8. Node-07 Arbiter selector: R1...Rn plus context -> selected arbiters
9. Node-08 Arbiter units: review -> pass or fbx reprocess instruction
10. Node-09 Formatter: approved translated fragments -> one final answer

## Edge Inventory

| Edge | What moves | Target receives | Target must produce | What breaks if missing |
|---|---|---|---|---|
| User prompt -> Node-01 | Organizer/user request | Raw prompt | Domain label array | Event request cannot be routed |
| Node-01 -> Node-02 | Domains and fragments | One domain fragment per unit | Formal domain wording | Later stages lose precise event rules |
| Node-02 -> Node-03 | ud1...udm | All formal wordings | b1...bn and fb1...fbn | Parallel promotion plan is missing |
| Node-03 -> Node-04 | Planned pieces | One piece per processor | Processed outcomes | Applier cannot safely act |
| Node-04 -> Node-05 | Processed result | One processed piece plus prompt | Concrete Rqi | No applied event result exists |
| Node-05 -> Node-06 | R1...Rn | One formal result per translator plus full collection | Natural language or consistency flag | User/stakeholder wording is missing |
| Node-05 -> Node-07 | Formal result collection | R1...Rn plus context | Arbiter list | Risk review is missing |
| Node-07 -> Node-08 | Arbiter responsibilities | One arbiter plus result | Pass or fbx | Unsafe results cannot be caught |
| Node-08 -> Node-01 | fbx correction | Reprocessing fragment | Corrected route | Bad fragment cannot be repaired |
| Node-06 and Node-08 -> Node-09 | Approved translated fragments | Only approved wording | Final audience answer | Formatter may show unchecked content |
