# Oracle for the sixth transfer: the rule mark of a lesson is taken from the verified side.
#
# Address of the repaired mechanism: 27.TranslationDomain/TranslationStructuredBatchBuilder.cs:98
# (before the change: `signature = AppendRuleTags(signature, BuildRuleTags(pair.Rejected));`).
#
# Why this oracle exists: the platform test lane (dotnet build / dotnet test) is closed by the
# session settings, so the pure-logic invariants of this transfer are proven here instead, by
# verbatim ports of the live C# with the exact source addresses cited above each port. This file
# proves algorithm behaviour only. It does not prove that the C# compiles and it does not replace
# the project test run.
#
# What is ported verbatim (the parts this change touches, plus the parts that decide which branch
# answers a request):
#   * TranslationStructuredBatchBuilder.NormalizeRuleTag        (character normalisation of a mark)
#   * TranslationStructuredBatchBuilder.AppendRuleTags          (adding marks to a signature)
#   * TranslationStructuredBatchBuilder.BuildRuleTags(rejected) (the REMOVED old source of marks)
#   * TranslationVerifiedRuleTags.Canonicalize                  (the NEW verified source of marks)
#   * TranslationStructuredBatchBuilder.BuildVerifiedRuleTags   (the new appender input)
#   * TranslationSemanticTokenizer.LooksLikeRussianVerbUtf8     (which word counts as a verb)
#   * TranslationSemanticTokenizer.BuildRuleTags tense branch   (RU:TENSE:PAST for every verb)
#   * SemanticTokenizerUnit.TrainOnExample / Tokenize           (storing and reading the lesson)
#   * ExecutorUnit.NormalizeMapKey                              (address of the exact memory cell)
#   * ExecutorUnit.Execute tier order                           (which branch answers a request)
#
# What is modelled as a labelled constant: the pattern-and-concept prefix of the raw signature.
# That prefix is identical in both arms because this transfer does not touch the tokenizer file at
# all, so it cannot change the prefix. The claim under test is about the appended rule marks.

import re
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace", newline="\n")

FAILURES = []
CHECKS = 0


def check(label, actual, expected):
    """Record one comparison, print it, and remember a mismatch for the exit code."""
    global CHECKS
    CHECKS += 1
    ok = actual == expected
    if not ok:
        FAILURES.append(f"{label}: expected {expected!r}, got {actual!r}")
    print(f"  [{'ok' if ok else 'FAIL'}] {label}: {actual!r}")


# ---------------------------------------------------------------------------
# Verbatim port: TranslationStructuredBatchBuilder.NormalizeRuleTag
#   Regex.Replace(value, @"[^A-Za-z0-9:_=\-]+", "_").Trim('_').ToUpperInvariant()
#   -> empty gives "", otherwise "RULE:" + normalized
# ---------------------------------------------------------------------------
def normalize_rule_tag(value):
    normalized = re.sub(r"[^A-Za-z0-9:_=\-]+", "_", value or "").strip("_").upper()
    return "" if not normalized.strip() else f"RULE:{normalized}"


# ---------------------------------------------------------------------------
# Verbatim port: TranslationStructuredBatchBuilder.AppendRuleTags
# ---------------------------------------------------------------------------
def append_rule_tags(signature, rule_tags):
    if len(rule_tags) == 0:
        return signature
    existing = {part for part in signature.split("|") if part}
    additions = [tag for tag in rule_tags if tag not in existing]
    return signature if not additions else signature + "|" + "|".join(additions)


# ---------------------------------------------------------------------------
# Verbatim port of the REMOVED old source: BuildRuleTags(string rejected)
#   RuleTagRegex = @"\[RULE:\s*(?<rule>.*?)\]" with RegexOptions.Singleline
# ---------------------------------------------------------------------------
OLD_RULE_TAG_REGEX = re.compile(r"\[RULE:\s*(.*?)\]", re.DOTALL)


def old_build_rule_tags_from_rejected(rejected):
    seen = []
    for match in OLD_RULE_TAG_REGEX.finditer(rejected or ""):
        tag = normalize_rule_tag(match.group(1))
        if tag.strip() and tag not in seen:
            seen.append(tag)
    return seen


# ---------------------------------------------------------------------------
# Verbatim port of the NEW source: TranslationVerifiedRuleTags.Canonicalize
# ---------------------------------------------------------------------------
TENSE_PAST_BODY = "TENSE=PAST"
TENSE_PRESENT_BODY = "TENSE=PRESENT"
TENSE_FAMILY = "TENSE"
FAMILY_SEPARATORS = ("_", ":", "=")


def try_canonicalize_tense(trimmed):
    """Return (body_or_None, states_tense) exactly as the C# out-parameter version does."""
    upper = trimmed.upper()
    if not upper.startswith(TENSE_FAMILY):
        return None, False
    if len(upper) == len(TENSE_FAMILY):
        return None, True
    if upper[len(TENSE_FAMILY)] not in FAMILY_SEPARATORS:
        return None, False
    value = upper[len(TENSE_FAMILY) + 1:]
    if value == "PAST":
        return TENSE_PAST_BODY, True
    if value == "PRESENT":
        return TENSE_PRESENT_BODY, True
    return None, True


def canonicalize(verified_rule_tags):
    """Return ("ok", bodies) or ("fail", error_code), mirroring OperationResult."""
    if verified_rule_tags is None or len(verified_rule_tags) == 0:
        return "ok", []
    bodies = []
    seen = set()
    for entry in verified_rule_tags:
        trimmed = (entry or "").strip()
        if len(trimmed) == 0:
            return "fail", "VERIFIED_RULE_TAG_EMPTY"
        tense_body, states_tense = try_canonicalize_tense(trimmed)
        if states_tense and tense_body is None:
            return "fail", "VERIFIED_RULE_TAG_UNSUPPORTED_TENSE"
        body = tense_body if tense_body is not None else trimmed
        if body not in seen:
            seen.add(body)
            bodies.append(body)
    if TENSE_PAST_BODY in seen and TENSE_PRESENT_BODY in seen:
        return "fail", "VERIFIED_RULE_TAG_TENSE_CONFLICT"
    return "ok", bodies


# ---------------------------------------------------------------------------
# Verbatim port: TranslationStructuredBatchBuilder.BuildVerifiedRuleTags
# ---------------------------------------------------------------------------
def build_verified_rule_tags(canonical_bodies):
    out = []
    for body in canonical_bodies:
        tag = normalize_rule_tag(body)
        if tag.strip() and tag not in out:
            out.append(tag)
    return out


# ---------------------------------------------------------------------------
# Verbatim port: TranslationSemanticTokenizer.LooksLikeRussianVerbUtf8
# plus the tense branch of TranslationSemanticTokenizer.BuildRuleTags
# (for a Russian request every token classified as a verb adds RU:TENSE:PAST, unconditionally).
# ---------------------------------------------------------------------------
VERB_SUFFIXES = (
    "ть", "ти", "йте", "ите", "ёт", "ет", "ит", "ут", "ют",
    "лся", "лась", "али", "или", "ала", "яла", "ила",
    "ел", "ал", "ял", "ил",
)


def looks_like_russian_verb(lower):
    if any(lower.endswith(suffix) for suffix in VERB_SUFFIXES):
        return True
    return lower.endswith("л") and len(lower) > 3


def raw_parser_tense_tag(raw_text):
    """The tense mark the raw parser itself puts into the signature for a Russian request."""
    for word in raw_text.split(" "):
        if word and looks_like_russian_verb(word.lower()):
            return "RU:TENSE:PAST"
    return None


# ---------------------------------------------------------------------------
# Verbatim port: ExecutorUnit.NormalizeMapKey (non-mojibake branch) and
# SemanticTokenizerUnit.Normalize (whitespace join of the raw request text).
# ---------------------------------------------------------------------------
def normalize_map_key(value):
    trimmed = (value or "").strip().lower()
    kept = "".join(ch for ch in trimmed if ch.isalnum() or ch in ("_", "$") or ch.isspace())
    return " ".join(kept.split())


def normalize_exact_input(text):
    return " ".join((text or "").strip().split())


# ---------------------------------------------------------------------------
# The two training rows of plan 105, address P52, section 4.3, exactly as written there.
# ---------------------------------------------------------------------------
ROWS = [
    {
        "id": "P52-TAGS-t1",
        "a_side": "Он проверил файл",
        "chosen": "He checked the file",
        "rejected": "He checks the file [WRONG: tense] [RULE: TENSE_PRESENT]",
        "verified": ["TENSE_PAST"],
        "true_tense": "PAST",
    },
    {
        "id": "P52-TAGS-t2",
        "a_side": "Он проверяет файл",
        "chosen": "He checks the file",
        "rejected": "He checked the file [WRONG: tense] [RULE: TENSE_PAST]",
        "verified": ["TENSE_PRESENT"],
        "true_tense": "PRESENT",
    },
]

# The pattern-and-concept prefix of the raw signature. This transfer does not touch the tokenizer
# file, so this prefix is identical in both arms; it is a label here, not a measured value.
RAW_SIGNATURE_PREFIX = "<raw pattern and concepts, identical in both arms>"

# The four panel rows of plan 105, section 4.3: the two training sources, then the same two
# sentences with a full stop added.
PANEL = [
    ("e1", "Он проверил файл", "He checked the file"),
    ("e2", "Он проверяет файл", "He checks the file"),
    ("e3", "Он проверил файл.", "He checked the file"),
    ("e4", "Он проверяет файл.", "He checks the file"),
]

TOPIC = "general"


def build_lesson(row, arm):
    """Build the stored lesson of one row the way the product builder does, per arm."""
    raw_tense = raw_parser_tense_tag(row["a_side"])
    signature = RAW_SIGNATURE_PREFIX + (f"|{raw_tense}" if raw_tense else "")
    if arm == "A":
        # Old source: the marks are read from the "how it is wrong" side of the same pair.
        tags = old_build_rule_tags_from_rejected(row["rejected"])
        refusal = None
    else:
        # New source: the marks come from the verified training side, or the batch is refused.
        status, payload = canonicalize(row["verified"])
        if status == "fail":
            return None, payload, raw_tense
        tags = build_verified_rule_tags(payload)
        refusal = None
    return append_rule_tags(signature, tags), refusal, raw_tense


def stored_tense_mark(signature):
    """The tense the stored verified mark of a lesson states, as the consumer would read it."""
    for part in signature.split("|"):
        if part.startswith("RULE:TENSE"):
            if part.endswith("PAST"):
                return "PAST"
            if part.endswith("PRESENT"):
                return "PRESENT"
            return "UNREADABLE"
    return "NONE"


def run_arm(arm):
    """Train one arm, then answer the whole panel through the product tier order."""
    print(f"\nPANEL {arm}: {'old source (rejected side)' if arm == 'A' else 'new source (verified side)'}")

    # ---- product training: one batch, the semantic lesson of each row is stored by the unit ----
    signature_corrections = {}   # SemanticTokenizerUnit._signatureCorrections
    exact_topic_source_map = {}  # ExecutorUnit._exactTopicSourceMap
    correct_marks = 0
    for row in ROWS:
        signature, refusal, raw_tense = build_lesson(row, arm)
        if signature is None:
            print(f"  {row['id']}: batch refused before any lesson was written ({refusal})")
            continue
        # SemanticTokenizerUnit.TrainOnExample stores the correct output under the request text.
        signature_corrections[normalize_exact_input(row["a_side"])] = signature
        # ExecutorUnit.TrainOnExample stores the exact answer under topic plus normalised source.
        exact_topic_source_map[(TOPIC, normalize_map_key(row["a_side"]))] = row["chosen"]
        mark = stored_tense_mark(signature)
        is_correct = mark == row["true_tense"]
        correct_marks += 1 if is_correct else 0
        print(
            f"  {row['id']}: true tense {row['true_tense']}, stored verified mark {mark}"
            f" -> {'correct' if is_correct else 'WRONG'};"
            f" raw parser mark {raw_tense}"
        )

    # ---- product path: every panel row goes through the tokenizer, then the executor tiers ----
    answers = {}
    for name, request, gold in PANEL:
        # SemanticTokenizerUnit.Tokenize: an exact-input hit returns the stored lesson signature.
        exact_key = normalize_exact_input(request)
        returned_signature = signature_corrections.get(exact_key)
        tokenizer_branch = "trained-exact-correction" if returned_signature else "raw-parser"
        # ExecutorUnit.Execute tier order: exact topic memory first, before anything else.
        memory_key = (TOPIC, normalize_map_key(request))
        if memory_key in exact_topic_source_map:
            text = exact_topic_source_map[memory_key]
            branch, confidence = "exact-source", 0.93
        else:
            # No phrase map, no word map and no neighbour was trained in this panel, so the
            # remaining tiers cannot answer and the direction adapter falls back to bootstrap.
            text = f"[RU_EN:BOOTSTRAP] {request}"
            branch, confidence = "bootstrap-fallback", 0.5
        correct = text == gold
        answers[name] = correct
        print(
            f"  {name}: tokenizer={tokenizer_branch} branch={branch}"
            f" confidence={confidence} correct={correct} text={text!r}"
        )

    return correct_marks, answers


print("ORACLE: the rule mark of a lesson is taken from the verified side (plan 105, address P52)")
print("Repaired mechanism: 27.TranslationDomain/TranslationStructuredBatchBuilder.cs:98")

marks_a, answers_a = run_arm("A")
marks_b, answers_b = run_arm("B")

print("\nPANEL 1: correct verified marks per arm")
check("arm A correct marks out of 2", marks_a, 0)
check("arm B correct marks out of 2", marks_b, 2)

print("\nPANEL 2: transitions of the user-visible answer")
became_correct = sorted(n for n in answers_b if answers_b[n] and not answers_a[n])
stayed_correct = sorted(n for n in answers_b if answers_b[n] and answers_a[n])
regressed = sorted(n for n in answers_b if not answers_b[n] and answers_a[n])
check("rows that became correct", became_correct, [])
check("rows that stayed correct", stayed_correct, ["e1", "e2", "e3", "e4"])
check("rows that went from correct to incorrect", regressed, [])
print("  Plainly: no row went from correct to incorrect, no row became correct;")
print("  all four rows answer from the exact translation memory in both arms, so the text of the")
print("  answer the user sees does not change at all. What changed is the mark saved with the lesson.")

print("\nPANEL 3: the raw parser's own tense mark is NOT repaired by this transfer")
check("raw mark for the past row", raw_parser_tense_tag(ROWS[0]["a_side"]), "RU:TENSE:PAST")
check("raw mark for the present row", raw_parser_tense_tag(ROWS[1]["a_side"]), "RU:TENSE:PAST")
print("  The parser reports the past tense for the present-tense sentence in BOTH arms. That is the")
print("  measured finding of the experiment and it stays visible: this transfer repairs the source")
print("  of the verified mark, not the parser's own guess.")

print("\nPANEL 4: a lesson without a verified annotation gets no mark, and the rejected side is not read")
no_field_row = {
    "id": "no-verified-field",
    "a_side": "Он проверил файл",
    "chosen": "He checked the file",
    "rejected": "He checks the file [WRONG: tense] [RULE: TENSE_PRESENT]",
    "verified": None,
    "true_tense": "PAST",
}
sig_old, _, _ = build_lesson(no_field_row, "A")
sig_new, _, _ = build_lesson(no_field_row, "B")
check("old source still put the rejected mark in", "RULE:TENSE_PRESENT" in sig_old, True)
check("new source adds no rule mark at all", stored_tense_mark(sig_new), "NONE")

print("\nPANEL 5: the same verified marks survive a changed rejected side")
changed = dict(ROWS[0])
changed["rejected"] = "He will check the file [WRONG: tense] [RULE: TENSE_PRESENT] [RULE: CASE=DATIVE]"
sig_plain, _, _ = build_lesson(ROWS[0], "B")
sig_changed, _, _ = build_lesson(changed, "B")
check("verified annotation is unchanged by the rejected text", sig_changed, sig_plain)
sig_old_plain, _, _ = build_lesson(ROWS[0], "A")
sig_old_changed, _, _ = build_lesson(changed, "A")
check("old source did change with the rejected text", sig_old_changed != sig_old_plain, True)

print("\nPANEL 6: refusals happen before the batch is emitted")
check("both tenses at once", canonicalize(["TENSE_PAST", "TENSE_PRESENT"]), ("fail", "VERIFIED_RULE_TAG_TENSE_CONFLICT"))
check("empty mark", canonicalize([""]), ("fail", "VERIFIED_RULE_TAG_EMPTY"))
check("blank mark", canonicalize(["   "]), ("fail", "VERIFIED_RULE_TAG_EMPTY"))
check("unsupported tense value", canonicalize(["TENSE_FUTURE"]), ("fail", "VERIFIED_RULE_TAG_UNSUPPORTED_TENSE"))
check("bare family name", canonicalize(["TENSE"]), ("fail", "VERIFIED_RULE_TAG_UNSUPPORTED_TENSE"))

print("\nPANEL 7: accepted shapes")
check("three spellings of the past tense collapse to one mark",
      canonicalize(["TENSE_PAST", "TENSE:PAST", "TENSE=PAST"]), ("ok", ["TENSE=PAST"]))
check("present tense alias", canonicalize(["tense_present"]), ("ok", ["TENSE=PRESENT"]))
check("a verified mark of another family keeps its own text",
      canonicalize(["CASE=DATIVE"]), ("ok", ["CASE=DATIVE"]))
check("a word that only starts with the family name is not a tense statement",
      canonicalize(["TENSED=X"]), ("ok", ["TENSED=X"]))
check("order of first appearance is kept",
      canonicalize(["TENSE_PAST", "CASE=DATIVE"]), ("ok", ["TENSE=PAST", "CASE=DATIVE"]))
check("final marks for a past row with a case mark",
      build_verified_rule_tags(["TENSE=PAST", "CASE=DATIVE"]),
      ["RULE:TENSE=PAST", "RULE:CASE=DATIVE"])

print("\nPANEL 8: the repointed existing fixture of the project test file")
fixture_verified = ["TENSE=PAST", "CASE=DATIVE"]
fixture_rejected = "[WRONG: agreement_context_lost.] [RULE: tense=present] [AGREEMENT: Она => She] [AGREEMENT: пошла => went]"
status, bodies = canonicalize(fixture_verified)
fixture_tags = build_verified_rule_tags(bodies)
check("fixture accepts its verified marks", status, "ok")
check("fixture marks", fixture_tags, ["RULE:TENSE=PAST", "RULE:CASE=DATIVE"])
check("fixture does not carry the present-tense mark of its rejected side",
      "RULE:TENSE=PRESENT" in fixture_tags, False)
check("the old source would have carried exactly that wrong mark",
      old_build_rule_tags_from_rejected(fixture_rejected), ["RULE:TENSE=PRESENT"])

print(f"\nchecks={CHECKS} failures={len(FAILURES)}")
for failure in FAILURES:
    print(f"  FAIL {failure}")
print("\nBoundary of this oracle: algorithm proven by oracle; C# compilation and the project test")
print("run are unverified, because the dotnet lane is closed by the session setting. The raw")
print("signature prefix is a label, not a measured value: this transfer does not touch the")
print("tokenizer file, so that prefix is identical in both arms.")
sys.exit(1 if FAILURES else 0)
