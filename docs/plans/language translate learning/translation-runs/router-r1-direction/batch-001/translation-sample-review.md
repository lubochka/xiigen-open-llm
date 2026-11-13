# Translation Router R1 Sample Review
# Batch: router-r1-direction/batch-001
# Date: 2026-05-11
# Review type: Routing decision pair quality review (look-like run, not model inference)

---

## Purpose

This review manually inspects 10 held-out routing examples to verify that the
chosen and rejected routing decisions in the training set are correct and that
the [WRONG:...] annotations teach a genuine boundary.

---

## Sample 1 — TR-ROUTE-R1-HO-0004

**Source:** "Он живёт в Москве."
**Expected direction:** RU_EN
**Script class:** cyrillic
**Named entity in source:** Москве (Moscow — Cyrillic)

**Training pair quality check:**
The corresponding train pair (TR-ROUTE-R1-0007 "Где ты живёшь?") teaches that Cyrillic
question forms route to RU_EN. This held-out example contains a Cyrillic proper noun in
its source script. The routing decision is unambiguous.

**Verdict:** pass
**Error category:** none

---

## Sample 2 — TR-ROUTE-R1-HO-0017

**Source:** "The weather forecast says it will snow tomorrow."
**Target language:** ru
**Expected direction:** EN_RU
**Script class:** latin

**Training pair quality check:**
Seven Latin English words. Target is Russian. The training pairs for EN_RU (0014-0025)
consistently teach that Latin English source with Russian target routes to EN_RU.
This held-out example is a clear case with no script ambiguity.

**Verdict:** pass
**Error category:** none

---

## Sample 3 — TR-ROUTE-R1-HO-0029

**Source:** "הוא גר בתל אביב."
**Expected direction:** HE_EN
**Script class:** hebrew
**Named entity in source:** תל אביב (Tel Aviv — Hebrew script)

**Training pair quality check:**
TR-ROUTE-R1-0048 teaches that 'Tel Aviv' in Latin script (inside an English sentence)
is Latin, not Hebrew, and routes EN_HE not HE_EN. This held-out sentence has 'תל אביב'
in Hebrew script — the opposite case. Routing should be HE_EN. The named entity is in
Hebrew script confirming Hebrew source. Routing decision: correct.

**Verdict:** pass
**Error category:** none

---

## Sample 4 — TR-ROUTE-R1-HO-0039

**Source:** "She is reading a very interesting book."
**Target language:** he
**Expected direction:** EN_HE
**Script class:** latin

**Training pair quality check:**
Seven Latin English tokens. Target is Hebrew. Training pairs for EN_HE (0038-0050)
consistently teach that Latin English source with Hebrew target routes to EN_HE.
No script ambiguity. No Hebrew characters in source.

**Verdict:** pass
**Error category:** none

---

## Sample 5 — TR-ROUTE-R1-HO-0026

**Source:** "בוקר טוב, מה שלומך?"
**Expected direction:** HE_EN
**Script class:** hebrew
**Note:** This is a Hebrew greeting; "מה שלומך" contains alef-bet characters.

**Training pair quality check:**
TR-ROUTE-R1-0026 trains the directly analogous pair "שלום, מה שלומך?" for HE_EN.
The held-out version "בוקר טוב, מה שלומך?" differs only in the opening greeting
token. All tokens are Hebrew script. Routing to HE_EN is the only correct choice.

**Verdict:** pass
**Error category:** none

---

## Sample 6 — TR-ROUTE-R1-HO-0014

**Source:** "The library closes at nine in the evening."
**Target language:** ru
**Expected direction:** EN_RU
**Script class:** latin

**Training pair quality check:**
Ten Latin English tokens with definite article 'the' confirming English source.
Target is Russian. No Cyrillic or Hebrew characters. EN_RU is the only correct route.
Training pair TR-ROUTE-R1-0023 covers a similar English schedule sentence.

**Verdict:** pass
**Error category:** none

---

## Sample 7 — TR-ROUTE-R1-HO-0033

**Source:** "כמה עולה הכרטיס הזה?"
**Expected direction:** HE_EN
**Script class:** hebrew
**Note:** All tokens in Hebrew script; הכרטיס uses definite article prefix ה.

**Training pair quality check:**
TR-ROUTE-R1-0032 trains "איפה אתה גר?" which is a similar Hebrew question form.
TR-ROUTE-R1-0033 in train trains "הרכבת יוצאת בשעה חמש" with the ה definite prefix.
The held-out "כמה עולה הכרטיס הזה?" is unambiguously Hebrew. HE_EN is correct.

**Verdict:** pass
**Error category:** none

---

## Sample 8 — TR-ROUTE-R1-HO-0001

**Source:** "Добрый день, чем могу помочь?"
**Expected direction:** RU_EN
**Script class:** cyrillic
**Note:** Formal greeting in Russian. More tokens than train average (5 words).

**Training pair quality check:**
TR-ROUTE-R1-0001 trains the simpler "Привет, как дела?" greeting. The held-out
greeting is more formal but all tokens are Cyrillic. Length does not change routing.
Script detection is sufficient for RU_EN.

**Verdict:** pass
**Error category:** none

---

## Sample 9 — TR-ROUTE-R1-HO-0050

**Source:** "Good evening, welcome to our restaurant."
**Target language:** he
**Expected direction:** EN_HE
**Script class:** latin

**Training pair quality check:**
TR-ROUTE-R1-0038 trains "Good morning, how are you today?" for EN_HE.
The held-out "Good evening, welcome to our restaurant" is the same register and
sentence type — English social greeting with Hebrew target. All Latin tokens.
EN_HE is correct.

**Verdict:** pass
**Error category:** none

---

## Sample 10 — REJECTED SIDE QUALITY SPOT CHECK

Reviewing TR-ROUTE-R1-0041:

**A-side:** "She studies Hebrew at the university."
**Chosen direction:** EN_HE
**Rejected direction:** HE_EN with reason "Routes HE_EN because 'Hebrew' appears in the sentence"

**Rejected side quality assessment:**
- Is it plausible? YES — a naive keyword-based router could make this mistake.
- Is it specifically wrong? YES — routing on the English word 'Hebrew' as if it were Hebrew script is a genuine failure mode.
- [WRONG:...] annotation present? YES — "routing on keyword presence instead of script evidence will systematically misroute this class of English sentence every time the target language name appears as an English word in the source."
- Is it a functional consequence? YES — it names the class of sentences that would fail.
- Is rejected different from chosen? YES — different direction, different adapter_id, different decision_reason.

**Verdict:** pass
**Annotation quality:** STRONG — teaches a reusable boundary about keyword-vs-script routing.

---

## Summary

| Sample | ID | Verdict | Error |
|---|---|---|---|
| 1 | HO-0004 | pass | none |
| 2 | HO-0017 | pass | none |
| 3 | HO-0029 | pass | none |
| 4 | HO-0039 | pass | none |
| 5 | HO-0026 | pass | none |
| 6 | HO-0014 | pass | none |
| 7 | HO-0033 | pass | none |
| 8 | HO-0001 | pass | none |
| 9 | HO-0050 | pass | none |
| 10 | R1-0041 (rejected side) | pass | none |

Manual review result: 10/10 pass.
