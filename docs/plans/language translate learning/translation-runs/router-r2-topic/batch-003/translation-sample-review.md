# Sample Review — router-r2-topic batch-003

## Identity

- stream: router-r2-topic
- batch_index: 3
- router_level: R2
- pairs_reviewed: 5 (10% spot check)
- batch_focus: stress-test edge cases, domain overlap phrases, multi-language coverage

## Sampled Pairs

### TR-ROUTE-R2-0309 (EN_RU legal — multi-keyword)

**Source:** "The contract includes a confidentiality clause and arbitration terms."  
**Chosen:** legal / adapter-en-ru-legal / confidence 0.92  
**Evidence:** contract, clause  
**Verdict:** ACCEPT — two unambiguous legal keywords; "arbitration" not a flow keyword but contract+clause sufficient.

### TR-ROUTE-R2-0325 (EN_RU cultural — ancient temple medieval)

**Source:** "The ancient temple was restored to its medieval glory."  
**Chosen:** cultural / adapter-en-ru-cultural / confidence 0.97  
**Evidence:** ancient, temple, medieval  
**Verdict:** ACCEPT — three cultural keywords; no cross-domain contamination; "glory" not a keyword.

### TR-ROUTE-R2-0334 (RU_EN personal — муж жена)

**Source:** "Мой муж и жена отмечают годовщину свадьбы."  
**Chosen:** personal / adapter-ru-en-personal / confidence 0.92  
**Evidence:** муж, жена  
**Verdict:** ACCEPT — two Cyrillic personal stems; муж standalone, жена standalone. "свадьбы" not a flow keyword.

### TR-ROUTE-R2-0341 (EN_RU general — no keywords)

**Source:** "The store closes at nine o'clock every evening."  
**Chosen:** general / adapter-en-ru-general / confidence 0.90  
**Evidence:** no_specialised_keywords_matched  
**Verdict:** ACCEPT — correctly defaults to general; no medical/legal/technical/cultural/personal keywords present.

### TR-ROUTE-R2-0338 (EN_RU personal — emotion+wedding domain overlap)

**Source:** "She expressed deep emotion at the wedding ceremony."  
**Chosen:** personal / adapter-en-ru-personal / confidence 0.92  
**Evidence:** emotion, wedding  
**Rejected:** cultural (ceremony score=1 vs personal score=2)  
**Verdict:** ACCEPT — emotion+wedding give personal=2, ceremony gives cultural=1; personal correctly wins.

## Overall Assessment

ACCEPT_BATCH — all 5 sampled pairs have correct chosen sides with appropriate keyword evidence. Batch-003 confirms domain overlap resolution (personal beats cultural when emotion keywords dominate), multi-keyword high-confidence detection, and correct general fallback.
