# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# SPDX-License-Identifier: AGPL-3.0-only
"""Shadow analysis ONLY: legacy .NET seeded Random sequence, NOT a native .NET
or E35/E36 experiment. Predict validation CLASS DISTRIBUTION under identical
seed 42 in Generate... and OrderBy(_ => rng.Next()).
Run with Python 3: python 23-E35-E36-COUPLED-SEED-SPLIT-PROXY.py
"""
import json
from collections import Counter

class DotNetSeededRandom:
    BIG = 2147483647
    def __init__(self, seed):
        arr = [0] * 56
        mj = 161803398 - abs(seed)
        if mj < 0:
            mj += self.BIG
        arr[55], mk = mj, 1
        for i in range(1, 55):
            ii = (21 * i) % 55
            arr[ii] = mk
            mk = mj - mk
            if mk < 0:
                mk += self.BIG
            mj = arr[ii]
        for _ in range(4):
            for i in range(1, 56):
                arr[i] -= arr[1 + (i + 30) % 55]
                if arr[i] < 0:
                    arr[i] += self.BIG
        self.arr, self.pos, self.peer = arr, 0, 21

    def sample(self):
        a, b = self.pos + 1, self.peer + 1
        if a >= 56: a = 1
        if b >= 56: b = 1
        r = self.arr[a] - self.arr[b]
        if r == self.BIG: r -= 1
        if r < 0: r += self.BIG
        self.arr[a], self.pos, self.peer = r, a, b
        return r

    def next(self, exclusive_max=None):
        r = self.sample()
        return r if exclusive_max is None else int((r / self.BIG) * exclusive_max)

assert DotNetSeededRandom(1).next() == 534011718, "legacy seeded sequence guard"
cases = [
    ("E35-Tier1", 50, [0] * 8 + [1] * 6, {1: 10}),
    ("E35-Tier2", 100, [0] * 8 + [1] * 6, {1: 20}),
    ("E36-Tier1", 40, [32] * 4 + [33] * 3 + [34] * 3 + [35] * 3 + [37] * 3, {35: 4, 37: 4}),
    ("E36-Tier2", 80, [32] * 4 + [33] * 3 + [34] * 3 + [35] * 3 + [37] * 3, {35: 11, 37: 5}),
]
out = []
for name, n, labels_by_template, expected in cases:
    gen, sort_rng = DotNetSeededRandom(42), DotNetSeededRandom(42)
    template_indices = [gen.next(len(labels_by_template)) for _ in range(n)]
    sort_keys = [sort_rng.next() for _ in range(n)]
    coupled = all(t == int(k / 2147483647 * len(labels_by_template))
                  for t, k in zip(template_indices, sort_keys))
    assert coupled, "RNG streams were not aligned"
    order = sorted(range(n), key=lambda i: sort_keys[i])
    heldout = order[int(n * .8):]
    by_class = dict(Counter(labels_by_template[template_indices[i]] for i in heldout))
    assert by_class == expected, (name, by_class, expected)
    out.append(dict(experiment=name, validation_count=len(heldout),
                    validation_classes=by_class, source_template_indices=[template_indices[i] for i in heldout],
                    random_streams_correlated=coupled))
print(json.dumps({"status": "PASS_PROXY_ONLY", "native_dotnet_replay": False,
                  "student_training_performed": False, "cases": out}, ensure_ascii=False, indent=2))
