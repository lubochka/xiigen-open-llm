#!/usr/bin/env python3
"""Source-equivalent lexical-admission proxy. NOT a .NET run or quality experiment."""
import json

CASES = [
    ("ru_en_polarity", "Этот важный файл уже доступен", "Этот важный файл уже не доступен", "different_meaning"),
    ("ru_en_order", "Сначала проверь доступ затем опубликуй событие", "Сначала опубликуй событие затем проверь доступ", "different_order"),
    ("ru_en_quantity", "Клиент получил три важных письма вчера вечером", "Клиент получил четыре важных письма вчера вечером", "different_quantity"),
    ("duplicate_punctuation", "Этот важный файл уже доступен", "этот важный файл уже доступен!", "same_meaning"),
    ("different_topic", "Этот важный файл уже доступен", "Завтра студент напишет программу самостоятельно", "different_topic"),
]

def token_set(s):
    return set(''.join(c for c in part if c.isalnum()) for part in s.lower().split()) - {''}

def jaccard(a, b):
    a, b = token_set(a), token_set(b)
    if not (a or b):
        return 1.0
    return len(a & b) / len(a | b)

def main():
    cases = []
    for key, history, candidate, relation in CASES:
        overlap = jaccard(history, candidate)
        removed_from_new_train = overlap >= 0.70
        rejected_as_holdout = overlap > 0.70
        cases.append({
            "case_id": key, "relation": relation,
            "jaccard": round(overlap, 6),
            "current_admission": "DROP" if removed_from_new_train else "KEEP",
            "current_heldout": "REJECT" if rejected_as_holdout else "ALLOW",
        })
    # Source-equivalent dictionary last-writer result, not a runtime model call.
    same_input_map = {}
    for provenance, translation in [("batch", "OUT_BATCH"), ("supplemental", "OUT_SUPPLEMENTAL")]:
        same_input_map["source"] = {"value": translation, "source": provenance}

    assert cases[0]["current_admission"] == "DROP"
    assert cases[1]["current_admission"] == "DROP"
    assert cases[2]["current_admission"] == "DROP"
    assert cases[3]["current_admission"] == "DROP"
    assert cases[4]["current_admission"] == "KEEP"
    assert same_input_map["source"]["source"] == "supplemental"
    print(json.dumps({
        "scope": "SOURCE_EQUIVALENT_PYTHON_PROXY_ONLY",
        "native_dotnet": "NOT_RUN",
        "student_training": "NOT_RUN",
        "student_inference": "NOT_RUN",
        "independent_quality_judge": "NOT_RUN",
        "threshold_train_gte": 0.70,
        "threshold_holdout_gt": 0.70,
        "cases": cases,
        "toy_last_writer": same_input_map["source"],
    }, ensure_ascii=False, indent=2))

if __name__ == "__main__":
    main()
