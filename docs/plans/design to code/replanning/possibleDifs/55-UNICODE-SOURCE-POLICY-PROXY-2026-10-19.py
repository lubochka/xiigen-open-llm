#!/usr/bin/env python3
"""Source-equivalent microproxy for TranslationMemorySourceLaw regex.
NOT a .NET run. NOT a translation model, training run, or consumer test.
Evidence: current core commit 6d36492714ebc1b9ae5f31a5a1da00673f524d83.
"""
import json
import re

SIMPLE_CLAUSE = re.compile(
    r"\A(?:the|a|an)[ \t]+(?P<subject>box|door|file|room|house|window|container|document|folder|account|connection)"
    r"[ \t]+(?P<copula>is|was)[ \t]+(?P<negative>not[ \t]+)?"
    r"(?P<predicate>empty|full|open|closed|ready|available|valid|invalid|enabled|disabled|safe|unsafe|null)[.!]?\Z",
    re.IGNORECASE | re.ASCII,
)

def classify(query, candidate):
    qm = SIMPLE_CLAUSE.match(query.strip())
    cm = SIMPLE_CLAUSE.match(candidate.strip())
    if qm is None or cm is None:
        return "Unknown"
    for role in ("subject", "copula", "predicate"):
        if qm.group(role).casefold() != cm.group(role).casefold():
            return "Unknown"
    return "Compatible" if bool(qm.group("negative")) == bool(cm.group("negative")) else "Contradiction"

CASES = [
    ("ASCII", "the box is not empty"),
    ("TAB", "the box is\tnot empty"),
    ("NBSP", "the box is\u00a0not empty"),
    ("NNBSP", "the box is\u202fnot empty"),
    ("THINSPACE", "the box is\u2009not empty"),
    ("DOUBLEASCII", "the box is  not empty"),
    ("NONCOPULAR", "do not delete the file"),
]

if __name__ == "__main__":
    candidate = "the box is empty"
    rows = [{"variant": label, "escaped_query": query.encode("unicode_escape").decode("ascii"),
             "policy_result": classify(query, candidate)} for label, query in CASES]
    print(json.dumps({"type": "SOURCE_EQUIVALENT_PROXY_ONLY", "runtime": "Python re", 
                      "model_or_dotnet_executed": False, "candidate_source": candidate,
                      "cases": rows}, indent=2, ensure_ascii=False))
