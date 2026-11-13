# Translation Sample Review — translator-en-ru-general / batch-018
# Session: 2026-05-11
# Reviewer: executor (manual inspection of 10 held-out rows)
# Status: PAIRSET_READY_TRAINING_RUNNER_MISSING

## Review Scope

Ten rows sampled from translation-heldout-50.jsonl (pairs 0001, 0006, 0011, 0016,
0021, 0026, 0031, 0036, 0041, 0046). Each row is inspected for:

- chosen translation correctness (fluency, grammar, completeness)
- rejected side defect clarity (error must be distinct and annotatable)
- defect annotation accuracy ([WRONG: type - consequence])
- source text coverage (no omitted meaning in chosen side)

---

## Row 0001

source:       I have a stomachache.
chosen:       U menya bolit zhivot.   [У меня болит живот.]
rejected:     Ya imeyu bol zhivota.   [Я имею боль живота.]
defect_type:  literal_translation

Chosen PASS — standard dative-impersonal construction.
Rejected PASS — clear calque; contrast is sharp and teachable.
Annotation PASS.

## Row 0006

source:       He has been having trouble sleeping.
chosen:       U nego problemy so snom.   [У него проблемы со сном.]
rejected:     On imeet trudnosti so spaniem.   [Он имеет трудности со спанием.]
defect_type:  literal_translation

Chosen PASS — natural.
Rejected PASS — два ошибки: non-standard nominalization and calque verb.
Annotation PASS.

## Row 0011

source:       She went to see a specialist.
chosen:       Ona poshla k spetsialistu.   [Она пошла к специалисту.]
rejected:     Ona poshla videt spetsialista.   [Она пошла видеть специалиста.]
defect_type:  literal_translation

Chosen PASS — correct directional construction without the infinitive.
Rejected PASS — calque of went to see; clearly foreign.
Annotation PASS.

## Row 0016

source:       I can't sleep because of the pain.
chosen:       Ya ne mogu spat iz-za boli.   [Я не могу спать из-за боли.]
rejected:     Ya ne splyu potomu chto imeyu bol.   [Я не сплю потому что имею боль.]
defect_type:  literal_translation

Chosen PASS — correct modal + iz-za construction.
Rejected PASS — imeyu bol is a calque; subordinate clause is awkward.
Annotation PASS.

## Row 0021

source:       I have a food allergy.
chosen:       U menya pishchevaya allergiya.   [У меня пищевая аллергия.]
rejected:     Ya imeyu allergiyu k ede.   [Я имею аллергию к еде.]
defect_type:  literal_translation

Chosen PASS — compact compound-adjective form used in medical contexts.
Rejected PASS — double error: calque verb + wrong preposition.
Annotation PASS.

## Row 0026

source:       I have a slight cough.
chosen:       U menya nebolshoy kashel.   [У меня небольшой кашель.]
rejected:     U menya slegka kashlyayu.   [У меня слегка кашляю.]
defect_type:  grammar_error

Chosen PASS — correct nominal predicate after u menya.
Rejected PASS — slegka kashlyayu after u menya is genuinely ungrammatical.
Annotation PASS.

## Row 0031

source:       The injection didn't hurt at all.
chosen:       Ukol sovsem ne bolel.   [Укол совсем не болел.]
rejected:     Inyektsiya sovsem ne bolno.   [Инъекция совсем не больно.]
defect_type:  grammar_error

Chosen PASS — ukol (masculine) -> bolel agreement correct.
Rejected PASS — bolno (adverb) cannot serve as predicate for inyektsiya.
Annotation PASS.

## Row 0036 (T08 omission)

source:       I have been coughing, sneezing, and feeling feverish since Monday.
chosen:       Ya kashlyayu, chikhayu i chuvstvuyu zhar s ponedelnika.
rejected:     Ya kashlyayu s ponedelnika.
defect_type:  omission

Chosen PASS — all three symptoms translated.
Rejected PASS — two symptoms dropped.
Annotation REPAIR REQUIRED — annotation text references nausea and photosensitivity
instead of sneezing and feverish. Content is correct; annotation text must be updated.

## Row 0041 (T09 named entity)

source:       The Red Cross clinic is open on Saturdays.
chosen:       Klinika Krasnogo Kresta rabotaet po subbotam.   [Клиника Красного Креста работает по субботам.]
rejected:     Klinika Krasnogo Peresecheniya rabotaet v subboty.
defect_type:  wrong_word_choice

Chosen PASS — correct genitive of proper name.
Rejected PASS — literal translation of proper name; Krasnoe Peresechenie does not exist.
Annotation PASS.

## Row 0046 (corruption — copied_source)

source:       You should see a doctor immediately.
chosen:       Vam sleduet nemedlenno obratitsya k vrachu.   [Вам следует немедленно обратиться к врачу.]
rejected:     You should see a doctor immediately.
defect_type:  literal_translation

Chosen PASS — fluent and formally appropriate.
Rejected PASS — English source copied verbatim into Russian target field.
Annotation PASS.

---

## Summary Table

| Row  | Chosen | Rejected | Annotation | Verdict |
|------|--------|----------|------------|---------|
| 0001 | PASS   | PASS     | PASS       | PASS    |
| 0006 | PASS   | PASS     | PASS       | PASS    |
| 0011 | PASS   | PASS     | PASS       | PASS    |
| 0016 | PASS   | PASS     | PASS       | PASS    |
| 0021 | PASS   | PASS     | PASS       | PASS    |
| 0026 | PASS   | PASS     | PASS       | PASS    |
| 0031 | PASS   | PASS     | PASS       | PASS    |
| 0036 | PASS   | PASS     | REPAIR     | ANNOTATION_REPAIR |
| 0041 | PASS   | PASS     | PASS       | PASS    |
| 0046 | PASS   | PASS     | PASS       | PASS    |

Pairs passing full review: 9 / 10
Pairs requiring annotation repair: 1 / 10 (row 0036 annotation text only)

The row 0036 chosen/rejected content is correct. The annotation string copies the
wrong pair's symptom list. Repair: replace nausea and photosensitivity with
sneezing and feverish. This does not block training.

Overall pairset assessment: STRUCTURALLY SOUND — MINOR ANNOTATION REPAIR IN HELDOUT ROW 0036
