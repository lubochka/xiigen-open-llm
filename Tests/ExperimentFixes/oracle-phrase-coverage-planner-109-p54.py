#!/usr/bin/env python3
# Oracle for the transfer "greedy phrase composition loses a required word"
# (plan 109, section FIX-P54-PHRASE-COVERAGE; experiment 54).
#
# WHY THIS FILE EXISTS
# The .NET build/test lane is denied by this session's permission settings, so the algorithm is
# proven by an executable port instead. Every ported function below names the exact C# address it
# was ported from, so the port can be checked line by line against the product.
#
# WHAT IT PROVES AND WHAT IT DOES NOT PROVE
# It proves the ALGORITHM of the whole product path: product training, the exact-memory tier, the
# phrase tier (old greedy arm A and new planner arm B), the word tier, the associative-memory
# (K-Nearest Neighbours) tier, the fallback, and the final acceptance gate of the direction adapter.
# It does NOT prove that the C# compiles and it does NOT run the platform tests: that lane is
# closed in this session.
#
# PORT BOUNDARIES THAT ARE DELIBERATELY NARROW
#   * the tokenizer is ported for ENGLISH source only (TranslationSemanticTokenizer.cs:98-151,
#     260-293, 295-324, 351-362, 432-439, 591-611); the Russian branches are not ported because
#     this transfer is admitted for English into Russian only;
#   * the source-polarity law (TranslationMemorySourceLaw.cs:23-77) is ported in full for its two
#     deciding parts (the narrow clause pattern and the negation-sign field); for every row of this
#     panel both sides carry no negation, so it returns Unknown, which the guard admits.

import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace", newline="\n")

# ───────────────────────── ported: normalization of a dictionary address ─────────────────────────
# Port of ExecutorUnit.NormalizeMapKey, TranslationTrainedUnits.cs:1059-1074.
def normalize_map_key(value):
    trimmed = (value or "").strip()
    kept = "".join(ch for ch in trimmed.lower() if ch.isalnum() or ch in ("_", "$") or ch.isspace())
    return " ".join(kept.split())


# Port of NormalizeComposedTranslation, TranslationTrainedUnits.cs:961-966.
def normalize_composed_translation(value):
    joined = " ".join((value or "").split())
    return joined.replace(" !", "!").replace(" .", ".").replace(" ,", ",")


def contains_cyrillic(value):
    # Port of ContainsCyrillic, TranslationTrainedUnits.cs:968-969.
    return any("Ѐ" <= ch <= "ӿ" for ch in (value or ""))


def contains_latin_letter(value):
    # Port of ContainsLatinLetter, TranslationTrainedUnits.cs:1006-1007.
    return any(("A" <= ch <= "Z") or ("a" <= ch <= "z") for ch in (value or ""))


# ───────────────────────── ported: English tokenizer ─────────────────────────
# Port of CleanWord, TranslationSemanticTokenizer.cs:432-439 (non-mojibake branch only).
def clean_word(word):
    value = word or ""
    punctuation = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"
    return value.strip(punctuation)


# Port of IsAsciiCodeIdentifier, TranslationSemanticTokenizer.cs:591-611.
def is_ascii_code_identifier(word):
    if not word or not word.strip() or len(word) <= 2:
        return False
    if not all(ord(ch) < 128 and (ch.isalnum() or ch in ("_", "$")) for ch in word):
        return False
    if "_" in word or "$" in word:
        return True
    if any(ch.isdigit() for ch in word) and any(ch.isalpha() for ch in word):
        return True
    upper = sum(1 for ch in word if ch.isupper())
    lower = sum(1 for ch in word if ch.islower())
    if word[0].islower() and upper > 0:
        return True
    return word[0].isupper() and upper > 1 and lower > 0


# Port of ClassifyPos + ResolveConcept for the English branch,
# TranslationSemanticTokenizer.cs:260-293 and 295-324.
def classify_pos_and_concept_en(word):
    clean = clean_word(word)
    lower = clean.lower()
    if is_ascii_code_identifier(clean):
        return ("CODE_IDENTIFIER", clean, "CODE_IDENTIFIER")
    if lower in ("null", "true", "false"):
        concept = "NULL_LITERAL" if lower == "null" else "UNKNOWN"
        return ("TECH_TERM", lower, concept)
    # The English branch returns NOUN with no concept: ResolveConcept ends at "lang != ru".
    return ("NOUN", clean, "UNKNOWN")


# Port of Tokenize, TranslationSemanticTokenizer.cs:98-151, English source only
# (BuildRuleTags returns an empty list for a non-Russian source: lines 153-157).
def tokenize_en(raw_text):
    words = [w for w in (raw_text or "").split(" ") if w]
    tokens = []
    for word in words:
        pos, lemma, concept = classify_pos_and_concept_en(word)
        tokens.append({"word": word, "lemma": lemma, "pos": pos, "concept": concept})

    pos_seq = [t["pos"] for t in tokens]
    # Port of ClassifyPattern, TranslationSemanticTokenizer.cs:351-362.
    if len(pos_seq) == 0:
        pattern = "UNKNOWN"
    else:
        first_verb = pos_seq.index("VERB") if "VERB" in pos_seq else -1
        has_pron_before = first_verb > 0 and any(p == "PRON" for p in pos_seq[:first_verb])
        if first_verb == 0 and not has_pron_before:
            pattern = "IMPERATIVE_V"
        elif has_pron_before and first_verb > 0 and pos_seq[-1] == "VERB":
            pattern = "SOV"
        else:
            pattern = "SVO"

    rule_tags = []
    sig_concepts = "|".join([t["concept"] for t in tokens if t["concept"] != "UNKNOWN"] + rule_tags)
    signature = pattern if sig_concepts == "" else pattern + "|" + sig_concepts
    prefix_concepts = "|".join(
        [t["concept"] for t in tokens if t["concept"] != "UNKNOWN"][:2] + rule_tags[:3])
    prefix_signature = pattern if prefix_concepts == "" else pattern + "|" + prefix_concepts
    structure_signature = pattern + "|" + "|".join([t["pos"] for t in tokens] + rule_tags)
    return {
        "pattern": pattern,
        "tokens": tokens,
        "signature": signature,
        "prefix_signature": prefix_signature,
        "structure_signature": structure_signature,
        "rule_tags": rule_tags,
    }


# ───────────────────────── ported: source polarity law ─────────────────────────
import re

# Port of SimpleClause, TranslationMemorySourceLaw.cs:23-25.
SIMPLE_CLAUSE = re.compile(
    r"\A(?:the|a|an)[ \t]+(?P<subject>box|door|file|room|house|window|container|document|folder|"
    r"account|connection)[ \t]+(?P<copula>is|was)[ \t]+(?P<negative>not[ \t]+)?"
    r"(?P<predicate>empty|full|open|closed|ready|available|valid|invalid|enabled|disabled|safe|"
    r"unsafe|null)[.!]?\Z",
    re.IGNORECASE)

ALWAYS_NEGATION = {"never", "nothing", "nobody", "nowhere", "none"}
SIMPLE_NEGATION = {"not", "cannot", "cant", "can't", "no", "dont", "don't", "doesnt", "doesn't",
                   "isnt", "isn't", "wont", "won't"}


def classify_english_source_sign(text):
    # Port of EnglishSourcePolarityField.ClassifyEnglishSource, TranslationMemorySourceLaw.cs:160-180.
    words = [w.lower() for w in re.findall(r"[\w'$]+", text or "")]
    always = sum(1 for w in words if w in ALWAYS_NEGATION)
    simple = sum(1 for w in words if w in SIMPLE_NEGATION)
    if always + simple >= 2:
        return "Ambiguous"
    if always == 1:
        return "NegationAlways"
    if simple == 1:
        return "NegationSimple"
    return "Positive"


def compare_legacy(query_source, candidate_source):
    # Port of the two-argument Compare, TranslationMemorySourceLaw.cs:28-45.
    q = SIMPLE_CLAUSE.match((query_source or "").strip())
    c = SIMPLE_CLAUSE.match((candidate_source or "").strip())
    if not q or not c:
        return "Unknown"
    for role in ("subject", "copula", "predicate"):
        if (q.group(role) or "").lower() != (c.group(role) or "").lower():
            return "Unknown"
    return "Compatible" if bool(q.group("negative")) == bool(c.group("negative")) else "Contradiction"


def compare_source_law(query_source, candidate_source, enable_polarity_field):
    # Port of the three-argument Compare, TranslationMemorySourceLaw.cs:49-77.
    if not enable_polarity_field:
        return compare_legacy(query_source, candidate_source)
    q_sign = classify_english_source_sign(query_source)
    c_sign = classify_english_source_sign(candidate_source)
    if q_sign == "Ambiguous" or c_sign == "Ambiguous":
        return compare_legacy(query_source, candidate_source)
    if q_sign != c_sign:
        return "Contradiction"
    return compare_legacy(query_source, candidate_source)


# ───────────────────────── ported: associative memory store ─────────────────────────
SOURCE_AWARE_BROAD = 0.35   # TranslationUnitContracts.cs:186
SOURCE_AWARE_PREFIX = 0.25  # TranslationUnitContracts.cs:189
LOW_INFORMATION_SOURCE_WORDS = {  # TranslationUnitContracts.cs:195-200
    "the", "a", "an", "and", "or", "to", "of", "in", "on", "at", "by", "for", "with", "from",
    "is", "are", "was", "were", "be", "been", "being", "not", "no", "do", "does", "did",
    "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those"}


class KnnStore:
    # Port of TranslationKnnStore, TranslationUnitContracts.cs:183-384.
    def __init__(self):
        self.store = {}

    def add_pair(self, signature, source_text, target_text):
        # Port of AddPair, TranslationUnitContracts.cs:207-217.
        self.store.setdefault(signature, []).append((source_text, target_text))

    @staticmethod
    def _source_tokens(source_text):
        # Port of SourceTokens, TranslationUnitContracts.cs:369-375.
        raw = re.findall(r"[^\W]+|[\w$]+", source_text or "")
        raw = re.findall(r"[\w$]+", source_text or "")
        tokens = {t.strip().lower() for t in raw}
        return {t for t in tokens if len(t) >= 2 and t not in LOW_INFORMATION_SOURCE_WORDS}

    @staticmethod
    def _source_similarity(query_tokens, candidate_tokens):
        # Port of SourceSimilarity, TranslationUnitContracts.cs:377-384.
        if not query_tokens or not candidate_tokens:
            return 0.0
        return len(query_tokens & candidate_tokens) / float(len(query_tokens))

    def _find_exact_with_source_guard(self, signature, k, query_source_text,
                                      minimum_source_similarity, enable_polarity):
        # Port of FindExactWithSourceGuard, TranslationUnitContracts.cs:338-367.
        bucket = self.store.get(signature)
        if bucket is None:
            return []
        admissible = [p for p in bucket
                      if compare_source_law(query_source_text, p[0], enable_polarity) != "Contradiction"]
        if minimum_source_similarity <= 0.0:
            return admissible[:k]
        query_tokens = self._source_tokens(query_source_text)
        if not query_tokens:
            return []
        return [p for p in admissible
                if self._source_similarity(query_tokens, self._source_tokens(p[0]))
                >= minimum_source_similarity][:k]

    def find_nearest_cascade(self, full_signature, prefix_signature, structure_signature, k,
                             query_source_text, enable_polarity, candidate_predicate):
        # Port of FindNearestCascade, TranslationUnitContracts.cs:253-308.
        def apply(tier):
            # Port of ApplyCandidatePredicate, TranslationUnitContracts.cs:318-336.
            if candidate_predicate is None:
                return tier
            return [p for p in tier if candidate_predicate(p[0], p[1])]

        full = apply(self._find_exact_with_source_guard(
            full_signature, k, query_source_text, SOURCE_AWARE_BROAD, enable_polarity))
        if full:
            return ("full", full)
        prefix = apply(self._find_exact_with_source_guard(
            prefix_signature, k, query_source_text, SOURCE_AWARE_PREFIX, enable_polarity))
        if prefix:
            return ("prefix", prefix)
        structure = apply(self._find_exact_with_source_guard(
            structure_signature, k, query_source_text, SOURCE_AWARE_BROAD, enable_polarity))
        if structure:
            return ("structure", structure)
        return ("none", [])


# ───────────────────────── ported: the new phrase-composition planner ─────────────────────────
# Port of TranslationPhraseCompositionPlanner.Plan, TranslationPhraseCompositionPlanner.cs.
MAX_PLANNED_SOURCE_TOKENS = 8
ALGORITHM_IDENTITY = "phrase-coverage-canonical-v1"


def planner_matches_at(source_tokens, start, phrase_tokens):
    if start + len(phrase_tokens) > len(source_tokens):
        return False
    for offset in range(len(phrase_tokens)):
        if source_tokens[start + offset].lower() != phrase_tokens[offset].lower():
            return False
    return True


def compare_source_keys(left, right):
    shared = min(len(left), len(right))
    for index in range(shared):
        if left[index] != right[index]:
            return -1 if left[index] < right[index] else 1
    return (len(left) > len(right)) - (len(left) < len(right))


def is_better(candidate, incumbent):
    if candidate["covered"] != incumbent["covered"]:
        return candidate["covered"] > incumbent["covered"]
    if candidate["spans"] != incumbent["spans"]:
        return candidate["spans"] < incumbent["spans"]
    return compare_source_keys(candidate["keys"], incumbent["keys"]) < 0


PLANNER_COST = {"plan_records_built": 0, "list_copies": 0}


def plan_phrase_composition(normalized_source_tokens, learned_phrases):
    empty = {"covered": 0, "spans": 0, "keys": [], "parts": []}
    if not normalized_source_tokens:
        return empty
    token_count = len(normalized_source_tokens)
    usable = sorted(
        [p for p in learned_phrases if p and p["source_tokens"] and (p["target"] or "").strip()],
        key=lambda p: p["source_key"])
    best = [None] * (token_count + 1)
    best[token_count] = empty
    for position in range(token_count - 1, -1, -1):
        best_here = best[position + 1]
        for phrase in usable:
            if not planner_matches_at(normalized_source_tokens, position, phrase["source_tokens"]):
                continue
            tail = best[position + len(phrase["source_tokens"])]
            candidate = {
                "covered": len(phrase["source_tokens"]) + tail["covered"],
                "spans": 1 + tail["spans"],
                "keys": [phrase["source_key"]] + tail["keys"],
                "parts": [phrase["target"]] + tail["parts"],
            }
            # Count what this algorithm actually allocates, so the memory cost is a number.
            PLANNER_COST["plan_records_built"] += 1
            PLANNER_COST["list_copies"] += 2
            if is_better(candidate, best_here):
                best_here = candidate
        best[position] = best_here
    return best[0]


# ───────────────────────── ported: the executor unit ─────────────────────────
EN_RU_ORDINARY_KNN_MIN_SOURCE_SIMILARITY = 0.60  # TranslationTrainedUnits.cs:234
EN_RU_ORDINARY_KNN_MIN_SHARED_SOURCE_TOKENS = 2  # TranslationTrainedUnits.cs:235

EN_RU_LOW_INFORMATION_TARGETS = {  # TranslationTrainedUnits.cs:614-632
    "не", "от", "на", "в", "к", "и", "он", "она", "они", "это", "том",
    "пожалуйста", "люди", "дверь", "пойду", "спросить"}
EN_RU_ALLOWED_LOW_INFORMATION_PAIRS = {  # TranslationTrainedUnits.cs:634-642
    ("cant", "не"), ("not", "не"), ("no", "не"), ("cannot", "не"), ("tom", "том"), ("door", "дверь")}
EN_RU_STANDALONE_FUNCTION_SOURCE_WORDS = {  # TranslationTrainedUnits.cs:644-769
    "a", "about", "after", "all", "am", "an", "and", "any", "are", "around", "as", "at", "be",
    "because", "been", "before", "being", "between", "both", "but", "by", "can", "cannot", "cant",
    "could", "did", "do", "does", "doing", "done", "dont", "during", "each", "either", "every",
    "few", "for", "from", "had", "has", "have", "having", "he", "her", "here", "hers", "him",
    "his", "how", "i", "if", "in", "into", "is", "it", "its", "least", "less", "many", "may",
    "me", "might", "mine", "more", "most", "much", "must", "my", "neither", "no", "not", "of",
    "on", "only", "onto", "or", "ought", "our", "ours", "over", "own", "same", "shall", "she",
    "should", "some", "such", "than", "that", "the", "their", "theirs", "them", "then", "there",
    "these", "they", "this", "those", "through", "to", "under", "us", "was", "we", "were",
    "what", "when", "where", "which", "while", "who", "whole", "whom", "whose", "why", "will",
    "with", "without", "would", "you", "your", "yours"}

EN_RU_ORDINARY_KNN_LOW_INFORMATION_SOURCE_WORDS = {  # TranslationTrainedUnits.cs:1030-1036
    "the", "a", "an", "and", "or", "to", "of", "in", "on", "at", "by", "for", "with", "from",
    "is", "are", "was", "were", "be", "been", "being", "not", "no", "do", "does", "did",
    "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those",
    "source", "sentence", "fresh", "legacy", "known", "novel", "starter"}


class ExecutorUnit:
    # Port of ExecutorUnit, TranslationTrainedUnits.cs:232-1080.
    # use_planner selects arm B (the transfer) over arm A (the published greedy assembler).
    def __init__(self, direction, knn_store, use_planner):
        self.direction = direction
        self.knn = knn_store
        self.use_planner = use_planner
        self.word_map = {}
        self.surface_word_map = {}
        self.phrase_map = {}
        self.exact_topic_source_map = {}

    # Port of ShouldAcceptStandaloneWordMapping, TranslationTrainedUnits.cs:585-612.
    def should_accept_standalone_word_mapping(self, source_key, target):
        normalized_source = normalize_map_key(source_key)
        normalized_target = normalize_map_key(target)
        if not normalized_source.strip() or not (target or "").strip():
            return False
        if self.direction != "EN_RU":
            return True
        if (normalized_source, normalized_target) in EN_RU_ALLOWED_LOW_INFORMATION_PAIRS:
            return True
        if normalized_source in EN_RU_STANDALONE_FUNCTION_SOURCE_WORDS:
            return False
        if contains_latin_letter(target):
            return False
        if not contains_cyrillic(target) and not is_safe_numeric_or_punctuation_target(target):
            return False
        single = len([t for t in normalized_target.split(" ") if t]) <= 1
        if single and normalized_target in EN_RU_LOW_INFORMATION_TARGETS:
            return False
        return True

    # Port of TrainOnExample, TranslationTrainedUnits.cs:430-485.
    def train_on_example(self, example):
        if example["held_out"]:
            return
        source_text = example["source_text"]
        topic = example["topic_domain"]
        chosen_text = example["chosen_text"]
        # Port of TranslationExactSourceKey.Create, TranslationExactSourceKey.cs:76.
        exact_key = (normalize_topic_domain(topic), normalize_map_key(source_text))
        if exact_key[0] and exact_key[1] and (chosen_text or "").strip():
            self.exact_topic_source_map[exact_key] = chosen_text
        self.knn.add_pair(example["semantic_signature"], source_text, chosen_text)
        source_tokens = tokenize_en(source_text)
        self.knn.add_pair(source_tokens["prefix_signature"], source_text, chosen_text)
        self.knn.add_pair(source_tokens["structure_signature"], source_text, chosen_text)
        for mapping in example["word_mappings"] or []:
            key = normalize_map_key(mapping["source_word"])
            if key.strip() and (mapping["target_word"] or "").strip() and \
                    self.should_accept_standalone_word_mapping(key, mapping["target_word"]):
                self.word_map[(key, mapping["concept"])] = mapping["target_word"]
                self.surface_word_map[key] = mapping["target_word"]
        for phrase in example["phrase_mappings"] or []:
            key = normalize_map_key(phrase["source_phrase"])
            if key.strip() and (phrase["target_phrase"] or "").strip():
                self.phrase_map[key] = phrase["target_phrase"].strip()

    # Port of TryGetAcceptedWordMapping, TranslationTrainedUnits.cs:907-947.
    def try_get_accepted_word_mapping(self, token):
        lemma_key = normalize_map_key(token["lemma"])
        word_key = normalize_map_key(token["word"])
        for key in (lemma_key, word_key):
            if key.strip():
                candidate = self.word_map.get((key, token["concept"]))
                if candidate is not None and self.should_accept_standalone_word_mapping(key, candidate):
                    return candidate
        for key in (lemma_key, word_key):
            if key.strip():
                candidate = self.surface_word_map.get(key)
                if candidate is not None and self.should_accept_standalone_word_mapping(key, candidate):
                    return candidate
        return None

    # Port of LooksLikeCopiedSource, TranslationTrainedUnits.cs:1038-1053.
    @staticmethod
    def looks_like_copied_source(tokens, translation):
        source_words = {normalize_map_key(t["word"]) for t in tokens["tokens"]
                        if t["pos"] != "CODE_IDENTIFIER" and normalize_map_key(t["word"]).strip()}
        target_words = {w for w in normalize_map_key(translation).split(" ") if w}
        if not source_words or not target_words:
            return False
        overlap = len(source_words & target_words)
        return overlap / float(max(len(source_words), len(target_words))) >= 0.80

    # Port of IsTargetCompatibleForDirection, TranslationTrainedUnits.cs:971-987.
    def is_target_compatible(self, tokens, translation):
        if not (translation or "").strip():
            return False
        if self.direction == "RU_EN":
            return not contains_cyrillic(translation) and not self.looks_like_copied_source(tokens, translation)
        if self.direction == "EN_RU":
            return (contains_cyrillic(translation) and not contains_latin_letter(translation)
                    and not self.looks_like_copied_source(tokens, translation))
        return not contains_cyrillic(translation)

    # Port of IsPhraseMapTargetCompatibleForDirection, TranslationTrainedUnits.cs:989-1004.
    def is_phrase_map_target_compatible(self, tokens, translation):
        if not (translation or "").strip():
            return False
        if self.direction == "RU_EN":
            return not contains_cyrillic(translation) and not self.looks_like_copied_source(tokens, translation)
        if self.direction == "EN_RU":
            return contains_cyrillic(translation) and not self.looks_like_copied_source(tokens, translation)
        return not contains_cyrillic(translation)

    # Port of IsPhraseCoveragePlannerScope, TranslationTrainedUnits.cs (added by this transfer).
    def is_phrase_coverage_planner_scope(self, tokens, normalized_source_tokens):
        if self.direction != "EN_RU":
            return False
        if len(normalized_source_tokens) > MAX_PLANNED_SOURCE_TOKENS:
            return False
        for token in tokens["tokens"]:
            if self.try_get_accepted_word_mapping(token) is not None:
                return False
        return True

    # Port of TryComposeFromPhraseMappings, TranslationTrainedUnits.cs:845-... (both arms).
    def try_compose_from_phrase_mappings(self, tokens):
        if not self.phrase_map or not tokens["tokens"]:
            return (False, "", {})
        source_tokens = [normalize_map_key(t["word"]) for t in tokens["tokens"]]
        source_tokens = [t for t in source_tokens if t.strip()]
        if not source_tokens:
            return (False, "", {})

        phrase_entries = [{"source_key": key,
                           "source_tokens": [w for w in key.split(" ") if w],
                           "target": value}
                          for key, value in self.phrase_map.items()]
        phrase_entries = [e for e in phrase_entries
                          if e["source_tokens"] and (e["target"] or "").strip()]
        # Stable sort by descending phrase length, exactly as OrderByDescending does.
        phrase_entries.sort(key=lambda e: -len(e["source_tokens"]))

        parts = []
        matched_tokens = 0
        matched_phrase_spans = 0
        matched_phrase_tokens = 0
        used_planner = False

        if self.use_planner and self.is_phrase_coverage_planner_scope(tokens, source_tokens):
            used_planner = True
            plan = plan_phrase_composition(source_tokens, phrase_entries)
            parts = list(plan["parts"])
            matched_tokens = plan["covered"]
            matched_phrase_tokens = plan["covered"]
            matched_phrase_spans = plan["spans"]
        else:
            i = 0
            while i < len(source_tokens):
                phrase = next((e for e in phrase_entries
                               if planner_matches_at(source_tokens, i, e["source_tokens"])), None)
                if phrase is not None:
                    parts.append(phrase["target"])
                    matched_tokens += len(phrase["source_tokens"])
                    matched_phrase_spans += 1
                    matched_phrase_tokens += len(phrase["source_tokens"])
                    i += len(phrase["source_tokens"])
                    continue
                token = tokens["tokens"][min(i, len(tokens["tokens"]) - 1)]
                mapped = self.try_get_accepted_word_mapping(token)
                if mapped is not None:
                    parts.append(mapped)
                    matched_tokens += 1
                i += 1

        coverage = matched_tokens / float(len(source_tokens))
        repeated = matched_phrase_spans >= 2 and \
            matched_phrase_tokens / float(len(source_tokens)) >= 0.50
        detail = {"used_planner": used_planner, "coverage": coverage,
                  "covered_tokens": matched_tokens, "spans": matched_phrase_spans,
                  "repeated_phrase_coverage": repeated}
        if matched_phrase_spans == 0 or (not repeated and coverage < 0.85) or not parts:
            return (False, "", detail)
        translation = normalize_composed_translation(" ".join(parts))
        if not self.is_phrase_map_target_compatible(tokens, translation):
            return (False, "", detail)
        if not translation.strip():
            return (False, "", detail)
        return (True, translation, detail)

    # Port of TryComposeFromWordMappings, TranslationTrainedUnits.cs:392-427.
    def try_compose_from_word_mappings(self, tokens):
        parts = []
        mapped_tokens = 0
        for token in tokens["tokens"]:
            if token["pos"] == "CODE_IDENTIFIER":
                parts.append(token["word"])
                continue
            mapped = self.try_get_accepted_word_mapping(token)
            if mapped is not None:
                parts.append(mapped)
                mapped_tokens += 1
        translatable = len([t for t in tokens["tokens"] if t["pos"] != "CODE_IDENTIFIER"])
        ratio = 0.0 if translatable == 0 else mapped_tokens / float(translatable)
        if mapped_tokens < 2 or ratio < 0.50:
            return (False, "")
        translation = normalize_composed_translation(" ".join(parts))
        if not self.is_target_compatible(tokens, translation):
            return (False, "")
        return (bool(translation.strip()), translation)

    # Port of SourceContentTokens + SourceTokenSimilarity, TranslationTrainedUnits.cs:1009-1036.
    @staticmethod
    def source_content_tokens(source_text):
        tokens = {t for t in normalize_map_key(source_text).split(" ") if t}
        return {t for t in tokens
                if len(t) >= 2 and t not in EN_RU_ORDINARY_KNN_LOW_INFORMATION_SOURCE_WORDS}

    @classmethod
    def source_token_similarity(cls, query_source, candidate_source):
        query_tokens = cls.source_content_tokens(query_source)
        candidate_tokens = cls.source_content_tokens(candidate_source)
        if not query_tokens or not candidate_tokens:
            return 0.0
        overlap = len(query_tokens & candidate_tokens)
        if overlap < EN_RU_ORDINARY_KNN_MIN_SHARED_SOURCE_TOKENS:
            return 0.0
        return overlap / float(len(query_tokens))

    # Port of TrySelectKnnCandidate, TranslationTrainedUnits.cs:342-390.
    def try_select_knn_candidate(self, tokens, minimum_source_similarity):
        source_text = " ".join(t["word"] for t in tokens["tokens"])
        tier, nearest = self.knn.find_nearest_cascade(
            tokens["signature"], tokens["prefix_signature"], tokens["structure_signature"],
            5, source_text, self.direction == "EN_RU",
            lambda _source, target: bool((target or "").strip())
            and self.is_target_compatible(tokens, target))
        for neighbor_source, neighbor_target in nearest:
            if not (neighbor_target or "").strip():
                continue
            if not self.is_target_compatible(tokens, neighbor_target):
                continue
            if minimum_source_similarity > 0.0 and \
                    self.source_token_similarity(source_text, neighbor_source) < minimum_source_similarity:
                continue
            return (True, {"text": neighbor_target, "confidence": 0.88,
                           "adapter": "executor-en_ru-knn", "tier": tier})
        return (False, None)

    # Port of Execute, TranslationTrainedUnits.cs:268-340.
    def execute(self, tokens, topic_domain):
        exact_key = (normalize_topic_domain(topic_domain),
                     normalize_map_key(" ".join(t["word"] for t in tokens["tokens"])))
        if exact_key in self.exact_topic_source_map:
            return {"text": self.exact_topic_source_map[exact_key], "confidence": 0.93,
                    "adapter": "executor-en_ru-exact-source", "tier": "exact-source", "phrase": {}}

        ok, phrase_translation, phrase_detail = self.try_compose_from_phrase_mappings(tokens)
        if ok:
            return {"text": phrase_translation, "confidence": 0.86,
                    "adapter": "executor-en_ru-phrase-map", "tier": "phrase-map",
                    "phrase": phrase_detail}

        ok, word_translation = self.try_compose_from_word_mappings(tokens)
        if ok:
            return {"text": word_translation, "confidence": 0.50,
                    "adapter": "executor-en_ru-wordmap", "tier": "wordmap", "phrase": phrase_detail}

        has_semantic_concept = any(t["concept"] != "UNKNOWN" for t in tokens["tokens"])
        has_exact_pair_signature = "|PAIR:" in tokens["signature"]
        if has_semantic_concept or has_exact_pair_signature:
            ok, candidate = self.try_select_knn_candidate(tokens, 0.0)
            if ok:
                candidate["phrase"] = phrase_detail
                return candidate
        elif self.direction == "EN_RU":
            ok, candidate = self.try_select_knn_candidate(
                tokens, EN_RU_ORDINARY_KNN_MIN_SOURCE_SIMILARITY)
            if ok:
                candidate["confidence"] = 0.84
                candidate["adapter"] = "executor-en_ru-ordinary-source-knn"
                candidate["tier"] = "ordinary-source-knn"
                candidate["phrase"] = phrase_detail
                return candidate

        parts = []
        used_learned_mapping = False
        for token in tokens["tokens"]:
            if token["pos"] == "CODE_IDENTIFIER":
                parts.append(token["word"])
                continue
            mapped = self.try_get_accepted_word_mapping(token)
            if mapped is not None:
                parts.append(mapped)
                used_learned_mapping = True
                continue
            parts.append(token["word"])
        if used_learned_mapping:
            mapped_translation = normalize_composed_translation(" ".join(parts))
            if self.is_target_compatible(tokens, mapped_translation):
                return {"text": mapped_translation, "confidence": 0.50,
                        "adapter": "executor-en_ru-wordmap", "tier": "wordmap-tail",
                        "phrase": phrase_detail}
        return {"text": " ".join(parts), "confidence": 0.49,
                "adapter": "executor-en_ru-fallback", "tier": "fallback", "phrase": phrase_detail}


def is_safe_numeric_or_punctuation_target(target):
    # Port of IsSafeNumericOrPunctuationTarget, TranslationTrainedUnits.cs:771-774.
    if not (target or "").strip():
        return False
    return any(ch.isdigit() for ch in target) and \
        all(ch.isdigit() or ch.isspace() or not ch.isalnum() for ch in target)


def normalize_topic_domain(topic):
    # Port of NormalizeTopicDomain behaviour used by TranslationExactSourceKey.Create.
    return " ".join((topic or "").strip().lower().split())


# ───────────────────────── ported: final acceptance gate ─────────────────────────
def translate_through_product_path(executor, raw_text, topic_domain):
    # Port of TrainedTranslationAdapter.TranslateAsync, TranslationDirectionAdapters.cs:117-140,
    # with the quality verifier holding no learned rule in this panel, so Verify returns
    # Pass=true and Score=1.0 (TranslationTrainedUnits.cs:1103-1146).
    tokens = tokenize_en(raw_text)
    candidate = executor.execute(tokens, topic_domain)
    verification_pass, verification_score = True, 1.0
    phrase_map_candidate = candidate["adapter"].endswith("-phrase-map")
    if candidate["confidence"] >= 0.5 and (
            verification_pass or (phrase_map_candidate and candidate["confidence"] >= 0.85)):
        reported = min(0.99, max(candidate["confidence"], verification_score))
        return {"text": candidate["text"], "reported_confidence": reported,
                "tier": candidate["tier"], "adapter": candidate["adapter"],
                "phrase": candidate.get("phrase", {}), "bootstrap": False}
    # Port of BootstrapTranslationAdapter, TranslationDirectionAdapters.cs:78-80.
    return {"text": "[EN_RU:BOOTSTRAP] " + raw_text, "reported_confidence": 0.0,
            "tier": candidate["tier"], "adapter": "bootstrap-en_ru",
            "phrase": candidate.get("phrase", {}), "bootstrap": True}


# ───────────────────────── the three learned lessons of experiment 54 ─────────────────────────
# Exact lessons and transfer objects: plan 109 section 4, from P5401/Program.cs:1233-1248.
# Each lesson carries exactly one phrase mapping and no word mappings.
LESSONS = [
    {"id": "R54-t01", "source_text": "close user", "chosen_text": "закрой пользователя"},
    {"id": "R54-t02", "source_text": "user session", "chosen_text": "сеанс пользователя"},
    {"id": "R54-t03", "source_text": "close", "chosen_text": "закрой"},
]


def build_example(lesson):
    return {
        "held_out": False,
        "source_text": lesson["source_text"],
        "topic_domain": "general",
        "chosen_text": lesson["chosen_text"],
        "semantic_signature": tokenize_en(lesson["source_text"])["signature"],
        "word_mappings": None,
        "phrase_mappings": [{"source_phrase": lesson["source_text"],
                             "target_phrase": lesson["chosen_text"]}],
    }


def train_arm(use_planner):
    knn = KnnStore()
    executor = ExecutorUnit("EN_RU", knn, use_planner)
    for lesson in LESSONS:
        executor.train_on_example(build_example(lesson))
    return executor


# ───────────────────────── the panel ─────────────────────────
PANEL = [
    ("r1", "close user session", "закрой сеанс пользователя"),
    ("c1", "close user", "закрой пользователя"),
    ("c2", "user session", "сеанс пользователя"),
    ("c3", "close", "закрой"),
]
EDGE_EIGHT = "close user session now please again today safely"
EDGE_NINE = "close user session now please again today safely tomorrow"

failures = []


def check(label, actual, expected):
    ok = actual == expected
    print("  CHECK %-58s %s" % (label, "OK" if ok else "FAIL expected=%r actual=%r" % (expected, actual)))
    if not ok:
        failures.append(label)
    return ok


print("=" * 100)
print("ORACLE: phrase composition planner, transfer P54 of plan 109 (experiment 54)")
print("algorithm identity: %s ; planner admitted up to %d source tokens"
      % (ALGORITHM_IDENTITY, MAX_PLANNED_SOURCE_TOKENS))
print("=" * 100)

arm_a = train_arm(use_planner=False)
arm_b = train_arm(use_planner=True)

print("\nPANEL 1. Learned memory after product training is identical in both arms")
print("-" * 100)
print("  arm A phrase map       : %s" % sorted(arm_a.phrase_map.items()))
print("  arm B phrase map       : %s" % sorted(arm_b.phrase_map.items()))
check("phrase map A equals B", sorted(arm_a.phrase_map.items()), sorted(arm_b.phrase_map.items()))
check("word map A equals B", sorted(arm_a.word_map.items()), sorted(arm_b.word_map.items()))
check("word map is empty (lessons carry no word mapping)", len(arm_a.word_map), 0)
check("exact memory A equals B",
      sorted(arm_a.exact_topic_source_map.items()), sorted(arm_b.exact_topic_source_map.items()))
check("associative memory A equals B",
      {k: list(v) for k, v in arm_a.knn.store.items()}, {k: list(v) for k, v in arm_b.knn.store.items()})
print("  associative memory buckets: %s"
      % {k: len(v) for k, v in sorted(arm_a.knn.store.items())})

print("\nPANEL 2. Main panel through the whole product path, per row and per answering tier")
print("-" * 100)
arm_results = {}
for arm_name, executor in (("A", arm_a), ("B", arm_b)):
    arm_results[arm_name] = {}
    for row_id, source, gold in PANEL:
        result = translate_through_product_path(executor, source, "general")
        correct = result["text"] == gold
        arm_results[arm_name][row_id] = (result, correct)
        print("  arm %s %-3s source=%-22r tier=%-22s confidence=%.2f correct=%-5s text=%r"
              % (arm_name, row_id, source, result["tier"], result["reported_confidence"],
                 correct, result["text"]))

a_correct = sum(1 for row_id, _, _ in PANEL if arm_results["A"][row_id][1])
b_correct = sum(1 for row_id, _, _ in PANEL if arm_results["B"][row_id][1])
print("\n  arm A correct rows: %d of %d ; arm B correct rows: %d of %d"
      % (a_correct, len(PANEL), b_correct, len(PANEL)))
check("arm A panel score", a_correct, 3)
check("arm B panel score", b_correct, 4)

print("\n  Direct statement about every transition, as required:")
became_correct, stayed_incorrect, regressed, stayed_correct = [], [], [], []
for row_id, source, gold in PANEL:
    was = arm_results["A"][row_id][1]
    now = arm_results["B"][row_id][1]
    if not was and now:
        became_correct.append(row_id)
    elif not was and not now:
        stayed_incorrect.append(row_id)
    elif was and not now:
        regressed.append(row_id)
    else:
        stayed_correct.append(row_id)
print("    rows that became correct   : %s" % (became_correct or "none"))
print("    rows that stayed correct   : %s" % (stayed_correct or "none"))
print("    rows that stayed incorrect : %s" % (stayed_incorrect or "none"))
print("    rows that went from correct to incorrect: %s" % (regressed or "NONE"))
check("no row went from correct to incorrect", regressed, [])
check("exactly the examined row became correct", became_correct, ["r1"])

print("\nPANEL 3. The defect of arm A, named exactly")
print("-" * 100)
a_r1 = arm_results["A"]["r1"][0]
b_r1 = arm_results["B"]["r1"][0]
print("  arm A answered row r1 from tier %r with reported confidence %.2f and text %r"
      % (a_r1["tier"], a_r1["reported_confidence"], a_r1["text"]))
print("  arm A phrase tier detail: %s" % a_r1["phrase"])
print("  arm B answered row r1 from tier %r with reported confidence %.2f and text %r"
      % (b_r1["tier"], b_r1["reported_confidence"], b_r1["text"]))
print("  arm B phrase tier detail: %s" % b_r1["phrase"])
check("arm A lost the required word 'session' in its answer",
      "сеанс" in a_r1["text"], False)
check("arm A still reported a confident answer, not a refusal",
      a_r1["bootstrap"] is False and a_r1["reported_confidence"] >= 0.99, True)
check("arm A phrase tier covered only 2 of 3 source words", a_r1["phrase"]["covered_tokens"], 2)
check("arm B phrase tier covered 3 of 3 source words", b_r1["phrase"]["covered_tokens"], 3)
check("arm B answer came from the phrase tier", b_r1["tier"], "phrase-map")
check("arm B kept the required word 'session'", "сеанс" in b_r1["text"], True)

print("\nPANEL 4. Edges: the agreed refusal and the agreed fallback must not move")
print("-" * 100)
for label, source in (("8 tokens", EDGE_EIGHT), ("9 tokens", EDGE_NINE)):
    res_a = translate_through_product_path(arm_a, source, "general")
    res_b = translate_through_product_path(arm_b, source, "general")
    print("  %-9s arm A tier=%-12s covered=%s text=%r"
          % (label, res_a["tier"], res_a["phrase"].get("covered_tokens"), res_a["text"]))
    print("  %-9s arm B tier=%-12s covered=%s planner=%s text=%r"
          % (label, res_b["tier"], res_b["phrase"].get("covered_tokens"),
             res_b["phrase"].get("used_planner"), res_b["text"]))
    check("%s: answer text is byte-identical in both arms" % label, res_b["text"], res_a["text"])
    check("%s: both arms refuse the phrase tier and fall back" % label,
          (res_a["bootstrap"], res_b["bootstrap"]), (True, True))

res_a8 = translate_through_product_path(arm_a, EDGE_EIGHT, "general")
res_b8 = translate_through_product_path(arm_b, EDGE_EIGHT, "general")
check("8 tokens: planner was inside its admitted area", res_b8["phrase"]["used_planner"], True)
check("8 tokens: coverage improved from 2 to 3 covered words",
      (res_a8["phrase"]["covered_tokens"], res_b8["phrase"]["covered_tokens"]), (2, 3))
res_b9 = translate_through_product_path(arm_b, EDGE_NINE, "general")
check("9 tokens: planner stayed outside its admitted area", res_b9["phrase"]["used_planner"], False)

print("\nPANEL 5. Three pure planner checks, no training")
print("-" * 100)
empty_plan = plan_phrase_composition(["close", "user", "session"], [])
print("  empty learned map        -> covered=%d spans=%d parts=%s"
      % (empty_plan["covered"], empty_plan["spans"], empty_plan["parts"]))
check("empty learned map covers nothing", (empty_plan["covered"], empty_plan["spans"]), (0, 0))

# Dense counterexample for a search that would keep only the first 256 finished variants.
# Eight source words, and both a four-word and several shorter phrases match, so the best answer
# needs 8 covered words in 2 phrases while many worse combinations exist.
dense_tokens = ["a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"]
dense_map = [
    {"source_key": "a1 a2 a3 a4", "source_tokens": ["a1", "a2", "a3", "a4"], "target": "T1234"},
    {"source_key": "a5 a6 a7 a8", "source_tokens": ["a5", "a6", "a7", "a8"], "target": "T5678"},
    {"source_key": "a1 a2", "source_tokens": ["a1", "a2"], "target": "T12"},
    {"source_key": "a3 a4", "source_tokens": ["a3", "a4"], "target": "T34"},
    {"source_key": "a5 a6", "source_tokens": ["a5", "a6"], "target": "T56"},
    {"source_key": "a7 a8", "source_tokens": ["a7", "a8"], "target": "T78"},
    {"source_key": "a1", "source_tokens": ["a1"], "target": "T1"},
    {"source_key": "a2", "source_tokens": ["a2"], "target": "T2"},
    {"source_key": "a3", "source_tokens": ["a3"], "target": "T3"},
    {"source_key": "a4", "source_tokens": ["a4"], "target": "T4"},
    {"source_key": "a5", "source_tokens": ["a5"], "target": "T5"},
    {"source_key": "a6", "source_tokens": ["a6"], "target": "T6"},
    {"source_key": "a7", "source_tokens": ["a7"], "target": "T7"},
    {"source_key": "a8", "source_tokens": ["a8"], "target": "T8"},
]
dense_plan = plan_phrase_composition(dense_tokens, dense_map)
print("  dense map                -> covered=%d spans=%d keys=%s parts=%s"
      % (dense_plan["covered"], dense_plan["spans"], dense_plan["keys"], dense_plan["parts"]))
check("dense map covers all 8 source words", dense_plan["covered"], 8)
check("dense map uses the fewest possible phrases", dense_plan["spans"], 2)
check("dense map chose the two long phrases", dense_plan["parts"], ["T1234", "T5678"])

reversed_plan = plan_phrase_composition(dense_tokens, list(reversed(dense_map)))
print("  dense map reversed order -> covered=%d spans=%d parts=%s"
      % (reversed_plan["covered"], reversed_plan["spans"], reversed_plan["parts"]))
check("reversed learned order gives the same canonical answer", reversed_plan, dense_plan)

print("\nPANEL 6. Sensitivity on separate copies of the final learned state")
print("-" * 100)


def copy_arm_b():
    knn = KnnStore()
    knn.store = {k: list(v) for k, v in arm_b.knn.store.items()}
    clone = ExecutorUnit("EN_RU", knn, True)
    clone.phrase_map = dict(arm_b.phrase_map)
    clone.word_map = dict(arm_b.word_map)
    clone.surface_word_map = dict(arm_b.surface_word_map)
    clone.exact_topic_source_map = dict(arm_b.exact_topic_source_map)
    return clone


removed = copy_arm_b()
del removed.phrase_map["close"]
res_removed = translate_through_product_path(removed, "close user session", "general")
print("  removed learned phrase 'close'        -> tier=%-22s text=%r"
      % (res_removed["tier"], res_removed["text"]))
check("removing 'close' changes the answer of row r1",
      res_removed["text"] != b_r1["text"], True)

replaced = copy_arm_b()
replaced.phrase_map["user session"] = "СЕАНСЗАМЕНА"
res_replaced = translate_through_product_path(replaced, "close user session", "general")
print("  replaced learned target 'user session'-> tier=%-22s text=%r"
      % (res_replaced["tier"], res_replaced["text"]))
check("the answer depends on the changed learned value, not on an expected answer",
      "СЕАНСЗАМЕНА" in res_replaced["text"], True)

cleared = copy_arm_b()
cleared.phrase_map.clear()
res_cleared = translate_through_product_path(cleared, "close user session", "general")
print("  cleared the whole phrase map          -> tier=%-22s text=%r"
      % (res_cleared["tier"], res_cleared["text"]))
check("clearing the phrase map loses the phrase route", res_cleared["tier"] != "phrase-map", True)
check("the live accepted state was not damaged by the copies",
      sorted(arm_b.phrase_map.items()),
      sorted({"close user": "закрой пользователя", "user session": "сеанс пользователя",
              "close": "закрой"}.items()))

print("\nPANEL 7. Mutation sensitivity: the new behaviour must be impossible on the old logic")
print("-" * 100)
old_ok, old_text, old_detail = arm_a.try_compose_from_phrase_mappings(tokenize_en("close user session"))
new_ok, new_text, new_detail = arm_b.try_compose_from_phrase_mappings(tokenize_en("close user session"))
print("  old assembler -> accepted=%s covered=%d text=%r" % (old_ok, old_detail["covered_tokens"], old_text))
print("  new assembler -> accepted=%s covered=%d text=%r" % (new_ok, new_detail["covered_tokens"], new_text))
check("old assembler refuses the row", old_ok, False)
check("new assembler accepts the row with the full translation",
      (new_ok, new_text), (True, "закрой сеанс пользователя"))

print("\nPANEL 8. Existing published rows that must not move")
print("-" * 100)
# Published test ExecutorUnit_EnRuPhraseMapping_WinsOverOrdinaryKnn, Tests/TranslationDomainTests.cs:920.
# No word mappings at all, so this row now runs through the planner and must give the same answer.
wins_knn = KnnStore()
wins_executor_a = ExecutorUnit("EN_RU", wins_knn, False)
wins_executor_b = ExecutorUnit("EN_RU", KnnStore(), True)
for executor in (wins_executor_a, wins_executor_b):
    executor.train_on_example({
        "held_out": False, "source_text": "the blue house is empty", "topic_domain": "general",
        "chosen_text": "синий дом пустой",
        "semantic_signature": "TRAIN_ONLY_SIGNATURE_WITH_NO_RUNTIME_OVERLAP",
        "word_mappings": [],
        "phrase_mappings": [{"source_phrase": "red house", "target_phrase": "красный дом"},
                            {"source_phrase": "is empty", "target_phrase": "пустой"}]})
res_wins_a = translate_through_product_path(wins_executor_a, "red house is empty", "general")
res_wins_b = translate_through_product_path(wins_executor_b, "red house is empty", "general")
print("  phrase-wins-over-memory arm A -> planner=%s text=%r"
      % (res_wins_a["phrase"].get("used_planner"), res_wins_a["text"]))
print("  phrase-wins-over-memory arm B -> planner=%s text=%r"
      % (res_wins_b["phrase"].get("used_planner"), res_wins_b["text"]))
check("published row keeps its exact expected answer", res_wins_b["text"], "красный дом пустой")
check("published row answer is unchanged by the transfer", res_wins_b["text"], res_wins_a["text"])
check("published row does run through the planner", res_wins_b["phrase"]["used_planner"], True)

# Published test ExecutorUnit_EnRuPhraseFirstComposition_DoesNotEmitFunctionWordJunk,
# Tests/TranslationDomainTests.cs:571. Word mappings exist, so the planner must stand aside.
junk_a = ExecutorUnit("EN_RU", KnnStore(), False)
junk_b = ExecutorUnit("EN_RU", KnnStore(), True)
for executor in (junk_a, junk_b):
    executor.train_on_example({
        "held_out": False, "source_text": "phrase training seed", "topic_domain": "general",
        "chosen_text": "Том быстро потушил огонь",
        "semantic_signature": "SVO|EN_RU_PHRASE_FIRST",
        "word_mappings": [
            {"source_word": "tom", "concept": "UNKNOWN", "target_word": "Том"},
            {"source_word": "quickly", "concept": "UNKNOWN", "target_word": "быстро"},
            {"source_word": "put", "concept": "UNKNOWN", "target_word": "положил"},
            {"source_word": "out", "concept": "UNKNOWN", "target_word": "наружу"},
            {"source_word": "the", "concept": "UNKNOWN", "target_word": "моей"},
            {"source_word": "fire", "concept": "UNKNOWN", "target_word": "огонь"}],
        "phrase_mappings": [{"source_phrase": "put out", "target_phrase": "потушил"},
                            {"source_phrase": "the fire", "target_phrase": "огонь"}]})
res_junk_a = translate_through_product_path(junk_a, "Tom quickly put out the fire", "general")
res_junk_b = translate_through_product_path(junk_b, "Tom quickly put out the fire", "general")
print("  function-word-junk arm A -> planner=%s text=%r"
      % (res_junk_a["phrase"].get("used_planner"), res_junk_a["text"]))
print("  function-word-junk arm B -> planner=%s text=%r"
      % (res_junk_b["phrase"].get("used_planner"), res_junk_b["text"]))
check("word mappings keep the planner out of this row", res_junk_b["phrase"]["used_planner"], False)
check("mixed phrase and word row is unchanged", res_junk_b["text"], res_junk_a["text"])
check("mixed row still contains the learned phrase targets",
      ("потушил" in res_junk_b["text"], "огонь" in res_junk_b["text"]), (True, True))
check("mixed row still drops the rejected single-word translations",
      ("моей" in res_junk_b["text"], "положил" in res_junk_b["text"],
       "наружу" in res_junk_b["text"]), (False, False, False))

# Published test ExecutorUnit_EnRuPhraseFirstComposition_ReplacesRepeatedPhraseBeforeUnigrams,
# Tests/TranslationDomainTests.cs:616.
rep_a = ExecutorUnit("EN_RU", KnnStore(), False)
rep_b = ExecutorUnit("EN_RU", KnnStore(), True)
for executor in (rep_a, rep_b):
    executor.train_on_example({
        "held_out": False, "source_text": "repeated phrase seed", "topic_domain": "general",
        "chosen_text": "потушил", "semantic_signature": "SVO|EN_RU_REPEATED_PHRASE_FIRST",
        "word_mappings": [
            {"source_word": "put", "concept": "UNKNOWN", "target_word": "положил"},
            {"source_word": "out", "concept": "UNKNOWN", "target_word": "наружу"},
            {"source_word": "and", "concept": "UNKNOWN", "target_word": "и"}],
        "phrase_mappings": [{"source_phrase": "put out", "target_phrase": "потушил"}]})
res_rep_a = translate_through_product_path(rep_a, "put out and put out", "general")
res_rep_b = translate_through_product_path(rep_b, "put out and put out", "general")
print("  repeated-phrase arm A -> planner=%s text=%r"
      % (res_rep_a["phrase"].get("used_planner"), res_rep_a["text"]))
print("  repeated-phrase arm B -> planner=%s text=%r"
      % (res_rep_b["phrase"].get("used_planner"), res_rep_b["text"]))
check("repeated-phrase row keeps its exact expected answer", res_rep_b["text"], "потушил потушил")
check("repeated-phrase row is unchanged by the transfer", res_rep_b["text"], res_rep_a["text"])

# Direction guard: a Russian-to-English row must never enter the planner.
ru_en = ExecutorUnit("RU_EN", KnnStore(), True)
ru_en.phrase_map = {"добавить запись": "add record"}
ru_tokens = {"pattern": "SVO",
             "tokens": [{"word": "добавить", "lemma": "добавить", "pos": "NOUN", "concept": "UNKNOWN"},
                        {"word": "запись", "lemma": "запись", "pos": "NOUN", "concept": "UNKNOWN"}],
             "signature": "SVO", "prefix_signature": "SVO",
             "structure_signature": "SVO|NOUN|NOUN", "rule_tags": []}
check("Russian to English stays outside the planner",
      ru_en.is_phrase_coverage_planner_scope(ru_tokens, ["добавить", "запись"]), False)

print("\nPANEL 9. The memory cost of the chosen algorithm, as a number")
print("-" * 100)
# A full enumeration would build one finished variant per distinct walk through the graph. The
# planner keeps one best walk per position instead, so these two counts are the honest comparison.


def count_all_walks(normalized_source_tokens, learned_phrases):
    token_count = len(normalized_source_tokens)
    walks = [0] * (token_count + 1)
    walks[token_count] = 1
    for position in range(token_count - 1, -1, -1):
        total = walks[position + 1]
        for phrase in learned_phrases:
            if planner_matches_at(normalized_source_tokens, position, phrase["source_tokens"]):
                total += walks[position + len(phrase["source_tokens"])]
        walks[position] = total
    return walks[0]


LESSON_EDGES = [{"source_key": normalize_map_key(l["source_text"]),
                 "source_tokens": [w for w in normalize_map_key(l["source_text"]).split(" ") if w],
                 "target": l["chosen_text"]} for l in LESSONS]

for label, source_text, edges in (
        ("examined row r1, 3 source words", "close user session", LESSON_EDGES),
        ("edge row, 8 source words", EDGE_EIGHT, LESSON_EDGES),
        ("dense counterexample, 8 source words", " ".join(dense_tokens), dense_map)):
    tokens = [w for w in normalize_map_key(source_text).split(" ") if w]
    PLANNER_COST["plan_records_built"] = 0
    PLANNER_COST["list_copies"] = 0
    plan_phrase_composition(tokens, edges)
    all_walks = count_all_walks(tokens, edges)
    print("  %-38s planner built %2d candidate records and %2d list copies; a full"
          % (label, PLANNER_COST["plan_records_built"], PLANNER_COST["list_copies"]))
    print("  %-38s enumeration of every finished variant would hold %d of them"
          % ("", all_walks))

print("  The planner keeps at most one best walk per source position, so its records grow with the")
print("  number of source words, never with the number of combinations. This is an ALGORITHMIC")
print("  count, not a measurement of allocated .NET bytes: the build and test lane is denied in")
print("  this session, so the product byte cost of this transfer was NOT measured here. The")
print("  +14536 allocated bytes on four rows recorded in experiment 54 belong to the earlier")
print("  prototype that copied the direct search; this transfer deliberately does not copy it.")

print("\n" + "=" * 100)
if failures:
    print("RESULT: %d CHECK(S) FAILED: %s" % (len(failures), failures))
    sys.exit(1)
print("RESULT: every check passed.")
print("PROVEN BY THIS ORACLE: the algorithm of the whole product path for this transfer.")
print("NOT PROVEN HERE: that the C# compiles, and the platform tests - that lane is denied in")
print("this session, so no test run is claimed.")
print("=" * 100)
