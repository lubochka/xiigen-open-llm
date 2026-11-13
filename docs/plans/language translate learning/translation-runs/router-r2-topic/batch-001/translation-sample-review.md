# Sample Review — router-r2-topic batch-001

## Identity

- stream: router-r2-topic
- batch_index: 1
- router_level: R2
- pairs_reviewed: 5 (10% spot check)

## Sampled Pairs

### TR-ROUTE-R2-0101 (RU_EN medical)

**Source:** "Врач назначил мне антибиотики от инфекции."  
**Chosen:** medical / adapter-ru-en-medical / confidence 0.92  
**Rejected:** general / adapter-ru-en-general  
**Evidence:** врач, антибиотик, инфекц  
**Verdict:** ACCEPT — three clear medical Cyrillic keyword hits; wrong side correctly uses general fallback.

### TR-ROUTE-R2-0116 (EN_RU legal)

**Source:** "The defendant has the right to legal representation in court."  
**Chosen:** legal / adapter-en-ru-legal / confidence 0.97  
**Rejected:** general / adapter-en-ru-general  
**Evidence:** defendant, legal, court  
**Verdict:** ACCEPT — three unambiguous legal keywords; no cross-domain contamination.

### TR-ROUTE-R2-0129 (HE_EN technical)

**Source:** "הקוד מכיל שגיאה במערכת הפיתוח של השרת."  
**Chosen:** technical / adapter-he-en-technical / confidence 0.97  
**Rejected:** general / adapter-he-en-general  
**Evidence:** קוד, מערכת, פיתוח, שרת  
**Verdict:** ACCEPT — four Hebrew technical keywords; software bug report classified correctly.

### TR-ROUTE-R2-0144 (EN_HE personal)

**Source:** "My dear mother, I miss you every day and love you with all my emotion."  
**Chosen:** personal / adapter-en-he-personal / confidence 0.97  
**Rejected:** general / adapter-en-he-general  
**Evidence:** dear, mother, miss, love, emotion  
**Verdict:** ACCEPT — five personal keywords with no medical/legal/technical contamination.

### TR-ROUTE-R2-0111 (RU_EN general)

**Source:** "Магазин закрывается в девять часов вечера."  
**Chosen:** general / adapter-ru-en-general / confidence 0.90  
**Rejected:** technical / adapter-ru-en-technical  
**Evidence:** no_specialised_keywords_matched  
**Verdict:** ACCEPT — correctly defaults to general; rejected technical side has no keyword support.

## Overall Assessment

ACCEPT_BATCH — all 5 sampled pairs have well-grounded chosen sides with keyword evidence. Rejected sides are plausible wrong alternatives. No annotation errors found.
