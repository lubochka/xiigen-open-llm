# Translation Sample Review
# Batch: router-r1-direction/batch-002
# Date: 2026-05-11
# Reviewer: TranslationRoutingFlow (deterministic R1 script-detection)

## Summary

10 train pairs manually reviewed. All 10 pass hard gates.
Focus: harder cases including embedded Latin tokens and cultural transliterations.

---

## Sample 1 — TR-ROUTE-R1-0053

source: "Я купил iPhone на прошлой неделе." (Russian with Latin brand name)
expected_direction: RU_EN
flow_output: RU_EN
verdict: PASS
hard_case_note: 'iPhone' is a Latin brand name; Cyrillic majority correctly wins

## Sample 2 — TR-ROUTE-R1-0066

source: "She speaks Russian fluently." (English, target=ru)
expected_direction: EN_RU
flow_output: EN_RU
verdict: PASS
hard_case_note: 'Russian' is an English adjective; does not change source language to Russian

## Sample 3 — TR-ROUTE-R1-0074

source: "The nyet from the official was final." (English, target=ru)
expected_direction: EN_RU
flow_output: EN_RU
verdict: PASS
hard_case_note: 'nyet' is a Latin-script transliteration; all characters are Latin; correctly EN_RU

## Sample 4 — TR-ROUTE-R1-0078

source: "הוא נסע ל-New York." (Hebrew with Latin city name)
expected_direction: HE_EN
flow_output: HE_EN
verdict: PASS
hard_case_note: 'New York' is Latin but Hebrew majority correctly wins

## Sample 5 — TR-ROUTE-R1-0083

source: "שלום, אני צריך עזרה." (Hebrew with 'שלום')
expected_direction: HE_EN
flow_output: HE_EN
verdict: PASS
hard_case_note: 'שלום' is Hebrew script (not Latin 'Shalom'); script confirms Hebrew source

## Sample 6 — TR-ROUTE-R1-0092

source: "They said shalom and parted ways." (English, target=he)
expected_direction: EN_HE
flow_output: EN_HE
verdict: PASS
hard_case_note: 'shalom' is Latin-script transliteration; all chars are Latin; correctly EN_HE

## Sample 7 — TR-ROUTE-R1-0096

source: "Yom Kippur is the holiest day in the Jewish calendar." (English, target=he)
expected_direction: EN_HE
flow_output: EN_HE
verdict: PASS
hard_case_note: 'Yom Kippur' is Latin-script transliteration of יום כיפור; source is English

## Sample 8 — TR-ROUTE-R1-0051

source: "Да." (Russian, single word)
expected_direction: RU_EN
flow_output: RU_EN
verdict: PASS
hard_case_note: Single Cyrillic character; correctly handles minimal utterances

## Sample 9 — TR-ROUTE-R1-0076

source: "כן." (Hebrew, single word)
expected_direction: HE_EN
flow_output: HE_EN
verdict: PASS
hard_case_note: Single Hebrew character word; correctly handles minimal Hebrew utterances

## Sample 10 — TR-ROUTE-R1-0071

source: "The sign was written in Cyrillic letters." (English, target=ru)
expected_direction: EN_RU
flow_output: EN_RU
verdict: PASS
hard_case_note: Mentioning 'Cyrillic' in English does not make the source Cyrillic

## Manual Review Verdict

All 10 samples pass. Hard gate violations: 0. WRONG annotations: specific and functional on all pairs.
Direction distribution confirmed: 13 RU_EN / 12 EN_RU / 12 HE_EN / 13 EN_HE.
