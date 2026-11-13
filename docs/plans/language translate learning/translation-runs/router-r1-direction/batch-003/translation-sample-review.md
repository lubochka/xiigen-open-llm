# Translation Sample Review
# Batch: router-r1-direction/batch-003
# Date: 2026-05-11
# Reviewer: synthetic-oracle (TranslationRoutingFlow R1)

## Reviewed 10 of 50 training pairs

---

### 1. TR-ROUTE-R1-0101 — RU_EN — code-switched Cyrillic+Latin (Cyrillic wins)

**Source:** "Это хорошо, I think."
**Expected:** RU_EN
**Chosen:** RU_EN (confidence 0.92)
**Verdict:** PASS — Cyrillic (9) > Latin (6); correct Cyrillic-majority detection.

---

### 2. TR-ROUTE-R1-0105 — RU_EN — Cyrillic lookalike 'Ок'

**Source:** "Ок, сделаю это завтра."
**Expected:** RU_EN
**Chosen:** RU_EN (confidence 0.98)
**Verdict:** PASS — 'Ок' uses Cyrillic О (U+041E) and к (U+043A), not Latin O and k; entire sentence is Cyrillic.

---

### 3. TR-ROUTE-R1-0107 — RU_EN — Latin brand name McDonalds embedded in Cyrillic

**Source:** "Давай встретимся у McDonalds!"
**Expected:** RU_EN
**Chosen:** RU_EN (confidence 0.95)
**Verdict:** PASS — Cyrillic (16) > Latin (9); brand name does not override Cyrillic majority.

---

### 4. TR-ROUTE-R1-0112 — RU_EN — exact Cyrillic/Latin tie, Cyrillic wins

**Source:** "Пора спать, goodnight!"
**Expected:** RU_EN
**Chosen:** RU_EN (confidence 0.95)
**Verdict:** PASS — Cyrillic (9) == Latin (9); router applies >= condition giving Cyrillic precedence on tie.

---

### 5. TR-ROUTE-R1-0117 — EN_RU — Russian transliterations 'da' and 'nyet' in Latin script

**Source:** "He said da but meant nyet."
**Expected:** EN_RU
**Chosen:** EN_RU (confidence 0.95)
**Verdict:** PASS — 'da' and 'nyet' are Latin-script transliterations; no Cyrillic chars present; routing by script evidence, not semantics.

---

### 6. TR-ROUTE-R1-0120 — EN_RU — Russian cultural loanword 'blini' in Latin source

**Source:** "We ate blini with sour cream."
**Expected:** EN_RU
**Chosen:** EN_RU (confidence 0.95)
**Verdict:** PASS — 'blini' is a Latin-script English loanword; source is entirely Latin; cultural origin does not override script evidence.

---

### 7. TR-ROUTE-R1-0130 — HE_EN — code-switched Hebrew + Latin tech term Wi-Fi

**Source:** "יש כאן Wi-Fi?"
**Expected:** HE_EN
**Chosen:** HE_EN (confidence 0.97)
**Verdict:** PASS — Hebrew (5) > Latin (4); 'Wi-Fi' is a Latin abbreviation in a Hebrew sentence; Hebrew majority confirmed.

---

### 8. TR-ROUTE-R1-0135 — HE_EN — Latin transliteration 'Ivrit' in Hebrew sentence

**Source:** "היא מדברת קצת Ivrit."
**Expected:** HE_EN
**Chosen:** HE_EN (confidence 0.97)
**Verdict:** PASS — Hebrew majority; 'Ivrit' is a Latin-script transliteration of עברית; semantic reference to Hebrew does not override script majority.

---

### 9. TR-ROUTE-R1-0141 — EN_HE — Hebrew cultural loanwords 'challah' and 'Shabbat' in Latin source

**Source:** "She made challah for Shabbat dinner."
**Expected:** EN_HE
**Chosen:** EN_HE (confidence 0.95)
**Verdict:** PASS — 'challah' and 'Shabbat' are Latin-script transliterations in an English sentence; no Hebrew chars present; routing by script.

---

### 10. TR-ROUTE-R1-0144 — EN_HE — Hebrew name + word 'Hebrew' as English adjective

**Source:** "Eliezer Ben-Yehuda revived the Hebrew language."
**Expected:** EN_HE
**Chosen:** EN_HE (confidence 0.95)
**Verdict:** PASS — 'Eliezer Ben-Yehuda' and 'Hebrew' are all Latin script; 'Hebrew' functions as an English adjective; no Hebrew chars present; routing to EN_HE correct.

---

## Summary

All 10 reviewed pairs pass the oracle gate. Edge cases confirmed:
- Cyrillic lookalike letters (Ок) correctly detected as Cyrillic
- Exact Cyrillic/Latin ties resolve to Cyrillic as designed
- Latin brand names, URLs, and tech terms in Cyrillic/Hebrew sentences do not override script majority
- Latin-script transliterations and cultural loanwords correctly route by script, not cultural origin
