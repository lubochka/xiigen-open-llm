#!/usr/bin/env python3
"""Source-equivalent E26.ContextDistance simulation, NOT native .NET or training.

Compare with language_translate@f6032aff:
35.ExperimentsV2/Infrastructure/ContextualDictionary.cs, lines 66-115.
No model calls, data mutation, or claims of new skill.
"""
from dataclasses import dataclass
import json

@dataclass(frozen=True)
class Entry:
    before: tuple
    after: tuple
    target: str
    priority: int

def legacy_distance(lb, la, rb, ra):
    d = 0.0
    if rb and lb:
        d += 0 if any(x in rb for x in lb) else 1.5
    elif not rb and lb:
        d += 0.5
    if ra and la:
        d += 0 if any(x in ra for x in la) else 1.5
    elif not ra and la:
        d += 0.5
    if not rb and not ra:
        d += 1.0
    return d

def choose(entries, before=(), after=()):
    return min(entries, key=lambda e: (legacy_distance(before, after, e.before, e.after), -e.priority))

def main():
    old = [
        Entry((8,), (), "thread", 10),
        Entry((), (7,), "flow", 9),
        Entry((9,), (), "stream", 8),
        Entry((), (), "flow", 1)
    ]
    order = [
        Entry((7, 9), (), "wrong", 10),
        Entry((9, 7), (), "right", 8)
    ]
    tests = [
        ("missing_both_sides", (), (), old, "thread"),
        ("left_verb", (8,), (), old, "thread"),
        ("right_noun", (), (7,), old, "flow"),
        ("left_adjective", (9,), (), old, "stream"),
        ("order_tie_overridden_by_priority", (9, 7), (), order, "wrong")
    ]
    rows = []
    for case, before, after, entries, expected in tests:
        selected = choose(entries, before, after)
        actual = selected.target
        rows.append({
            "case": case,
            "query_before": list(before),
            "query_after": list(after),
            "actual": actual,
            "expected_from_source_code": expected,
            "distance": legacy_distance(before, after, selected.before, selected.after),
            "match": actual == expected
        })
    result = {
        "source": "language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9",
        "type": "SOURCE_EQUIVALENT_PYTHON_PROXY_NOT_NATIVE_DOTNET",
        "tests_passed": sum(r["match"] for r in rows),
        "tests_total": len(rows),
        "rows": rows,
        "meaning": "These outputs demonstrate two source-logic cases; no learning or product quality measured."
    }
    assert all(r["match"] for r in rows)
    print(json.dumps(result, ensure_ascii=False, indent=2))

if __name__ == "__main__":
    main()
