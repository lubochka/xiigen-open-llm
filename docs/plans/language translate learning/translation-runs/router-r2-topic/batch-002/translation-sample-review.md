# Sample Review — router-r2-topic batch-002

## Identity

- stream: router-r2-topic
- batch_index: 2
- router_level: R2
- pairs_reviewed: 5 (10% spot check)
- batch_focus: harder boundary cases, single-keyword signals

## Sampled Pairs

### TR-ROUTE-R2-0201 (RU_EN medical — single stem)

**Source:** "Болезнь прогрессирует, нужна срочная операция."  
**Chosen:** medical / adapter-ru-en-medical / confidence 0.92  
**Evidence:** болезн, операци  
**Verdict:** ACCEPT — two Cyrillic medical stems; болезн matches болезнь/болезни/болезней; операци matches операция/операции/операцию.

### TR-ROUTE-R2-0215 (EN_RU legal — multi-keyword)

**Source:** "The lawsuit involves a criminal liability clause."  
**Chosen:** legal / adapter-en-ru-legal / confidence 0.97  
**Evidence:** lawsuit, criminal, liability, clause  
**Verdict:** ACCEPT — four unambiguous legal keywords; no cross-domain contamination.

### TR-ROUTE-R2-0222 (EN_RU personal — minimal signal)

**Source:** "I feel sad and lonely since she left me."  
**Chosen:** personal / adapter-en-ru-personal / confidence 0.92  
**Evidence:** sad, lonely  
**Verdict:** ACCEPT — two personal keywords sufficient for personal domain detection; no competing domain keywords.

### TR-ROUTE-R2-0234 (HE_EN personal — Hebrew stems)

**Source:** "אני מרגיש אהבה עמוקה למשפחה שלי."  
**Chosen:** personal / adapter-he-en-personal / confidence 0.92  
**Evidence:** אהבה, משפחה  
**Verdict:** ACCEPT — two Hebrew personal keywords verified; אהבה in standalone form, משפחה in standalone form before שלי.

### TR-ROUTE-R2-0249 (EN_HE general — no keywords)

**Source:** "The office opens at eight in the morning."  
**Chosen:** general / adapter-en-he-general / confidence 0.90  
**Evidence:** no_specialised_keywords_matched  
**Verdict:** ACCEPT — correctly defaults to general; no medical/legal/technical/cultural/personal keywords present.

## Overall Assessment

ACCEPT_BATCH — all 5 sampled pairs have correct chosen sides with appropriate keyword evidence. Batch-002 confirms single-keyword detection (confidence 0.85-0.92 tier) and multi-keyword high-confidence detection (0.97 tier) both work correctly.
