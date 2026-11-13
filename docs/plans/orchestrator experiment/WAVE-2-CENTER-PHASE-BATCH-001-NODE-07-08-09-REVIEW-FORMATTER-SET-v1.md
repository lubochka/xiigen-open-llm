# Wave 2 Center Phase Batch 001 - Node 07/08/09 Review And Formatter Set v1
# Date: 2026-05-05

---

## Purpose

Add arbiter-selector, arbiter-unit, and formatter coverage for Wave 2 Center
Phase Batch 001.

This closes the nine-node pair path for the batch:

```text
node-07 selects review authorities
node-08 produces pass, concern, reprocess, or rejection verdicts
node-09 composes the final human result
```

---

## Node 07 - Arbiter Selector Pairs

| pair_id | A.result_fragment_ids | A.visible_statuses | B.selected_arbiters | B.selection_reason | B.confidence |
|---|---|---|---|---|---:|
| W2C-N07-001 | W2C-N06-001 | can_proceed, rejected | topology_gate_arbiter, corpus_promotion_gate | The result contains accepted topology work plus a rejected corpus promotion gate, so topology authority and promotion authority must review. | 0.90 |
| W2C-N07-002 | W2C-N06-002 | can_proceed, rejected | api_contract_arbiter, pair_shape_arbiter | The result contains accepted API contract boundaries and a rejected descriptive B output, so contract authority and pair-shape authority must review. | 0.91 |
| W2C-N07-003 | W2C-N06-003 | reprocess, concern, rejected | security_requirement_arbiter, token_safety_arbiter, security_concern_arbiter | The result contains hard security reprocess paths, compound token risk, and concern-level gaps, so all three security review families must fire. | 0.93 |
| W2C-N07-004 | W2C-N06-004 | can_proceed, rejected | implementation_discipline_arbiter, verification_gate_arbiter | The result contains accepted backend implementation discipline and rejected focused-only acceptance, so implementation and verification authorities must review. | 0.90 |
| W2C-N07-005 | W2C-N06-005 | can_proceed, concern, rejected | provider_authority_arbiter, internal_first_arbiter | The result contains provider-authority concerns and an external-first rejection, so connector authority and internal-first authority must review. | 0.91 |

---

## Node 08 - Arbiter Unit Pairs

| pair_id | A.selected_arbiters | A.reviewed_fragment | B.verdict | B.verdict_text | B.confidence |
|---|---|---|---|---|---:|
| W2C-N08-001 | topology_gate_arbiter, corpus_promotion_gate | Architecture topology fragment | pass_with_rejection_preserved | Pass: topology work can proceed. Rejection preserved: corpus promotion remains blocked until WDA gates pass. | 0.90 |
| W2C-N08-002 | api_contract_arbiter, pair_shape_arbiter | API contract fragment | pass_with_rejection_preserved | Pass: typed contract work can proceed. Rejection preserved: descriptive B output remains malformed and cannot train. | 0.91 |
| W2C-N08-003 | security_requirement_arbiter, token_safety_arbiter, security_concern_arbiter | Security authorization fragment | reprocess_with_concerns | Reprocess: missing PKCE and compound token risk must be corrected. Concern: IP-only throttling and missing logout hint remain visible without false escalation. Rejection preserved: future-release exception is not accepted. | 0.93 |
| W2C-N08-004 | implementation_discipline_arbiter, verification_gate_arbiter | Backend service fragment | pass_with_rejection_preserved | Pass: one-unit implementation and envelope execution can proceed. Rejection preserved: focused-only acceptance is not enough without full suite and source-anchor verification. | 0.90 |
| W2C-N08-005 | provider_authority_arbiter, internal_first_arbiter | Connector fragment | pass_with_concern_and_rejection | Pass: adapter-first connector work can proceed. Concern: provider-authority selection must stay visible. Rejection preserved: external-first fallback is not accepted. | 0.91 |

---

## Node 09 - Formatter Pairs

| pair_id | A.arbiter_verdict_ids | A.audience | B.final_human_result | B.sections_present | B.confidence |
|---|---|---|---|---|---:|
| W2C-N09-001 | W2C-N08-001 | engineer | Can proceed: phase context, registry topology, and recursive trace modeling. Rejected: corpus promotion before WDA gates. Next action: finish topology proof before promoting broad corpus rows. | can_proceed, rejected, next_action | 0.90 |
| W2C-N09-002 | W2C-N08-002 | engineer | Can proceed: typed logical-unit contract, runtime flags, and router audit fields. Rejected: descriptive B output. Next action: keep exact A/B object shape in training pairs. | can_proceed, rejected, next_action | 0.91 |
| W2C-N09-003 | W2C-N08-003 | engineer | Reprocess: missing PKCE and compound token risk. Concern: IP-only throttling and missing logout id_token_hint. Rejected: future-release exception for current security requirements. Next action: correct hard security failures before formatter approval. | reprocess, concern, rejected, next_action | 0.93 |
| W2C-N09-004 | W2C-N08-004 | engineer | Can proceed: one-unit backend implementation, registered flow routing, and operation-envelope metrics. Rejected: focused-only acceptance. Next action: require focused tests, full suite, and source-anchor verification. | can_proceed, rejected, next_action | 0.90 |
| W2C-N09-005 | W2C-N08-005 | engineer | Can proceed: adapter-first connector wrapping and preserved multi-provider evidence. Concern: provider-authority selection. Rejected: external-first fallback. Next action: complete local connector proof before allowing fallback. | can_proceed, concern, rejected, next_action | 0.91 |

---

## Phrasing B Companions

| pair_id | phrasing_b_input |
|---|---|
| W2C-N07-001 | Select review authorities for an architecture fragment that mixes accepted topology work with rejected corpus promotion. |
| W2C-N07-002 | Select review authorities for an API contract fragment that accepts typed boundaries and rejects descriptive training output. |
| W2C-N07-003 | Select security review families for a fragment with hard security failures, concerns, and preserved rejection. |
| W2C-N07-004 | Select implementation review authorities for backend work with one accepted path and one verification rejection. |
| W2C-N07-005 | Select connector review authorities for adapter work, provider concern, and external-first rejection. |
| W2C-N08-001 | Review the architecture fragment and keep the promotion rejection intact. |
| W2C-N08-002 | Review the API fragment and keep the malformed pair rejection intact. |
| W2C-N08-003 | Review the security fragment and preserve reprocess, concern, and rejection separately. |
| W2C-N08-004 | Review the backend fragment and keep full verification as a required acceptance gate. |
| W2C-N08-005 | Review the connector fragment and keep internal-first authority ahead of fallback. |
| W2C-N09-001 | Format the architecture verdict for an engineer without hiding the rejected corpus-promotion gate. |
| W2C-N09-002 | Format the API verdict for an engineer without softening malformed B output. |
| W2C-N09-003 | Format the security verdict for an engineer with reprocess, concern, and rejection still separate. |
| W2C-N09-004 | Format the backend verdict for an engineer with full verification named. |
| W2C-N09-005 | Format the connector verdict for an engineer with internal-first fallback rejection named. |

Each Phrasing B companion preserves the same B output as the original row.

---

## Gate Result

```text
5 node-07 original pairs: PASS
5 node-08 original pairs: PASS
5 node-09 original pairs: PASS
15 Phrasing B companions: PASS
Arbiter selections grounded in node-06 fragments: PASS
Arbiter verdicts preserve status classes: PASS
Formatter output keeps rejected and concern items visible: PASS
```
