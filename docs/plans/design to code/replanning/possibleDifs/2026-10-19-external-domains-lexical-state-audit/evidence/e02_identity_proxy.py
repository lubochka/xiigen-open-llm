"""ASCII arithmetic mirror of MethodFrequencyTable's case-insensitive keys.

No C# parser or learned model is invoked. The observations are explicit fixtures.
"""
from collections import Counter
from pathlib import Path
import json

observations = ["Read"] * 5 + ["read"]
current = Counter(method.lower() for method in observations)
separate = Counter(observations)
candidates = ["read", "Read"]
current_rank = sorted(candidates, key=lambda method: -current[method.lower()])
separate_rank = sorted(candidates, key=lambda method: -separate[method])
result = {
    "scope": "ASCII counter/ranking formula only; fixture observations, not Roslyn extraction",
    "native_dotnet": "NOT_RUN",
    "model_quality": "NOT_MEASURED",
    "observations": observations,
    "candidates": candidates,
    "current_key_counts": dict(current),
    "ordinal_key_counts": dict(separate),
    "current_rank": current_rank,
    "ordinal_rank": separate_rank,
    "candidate_permutation_current": sorted(reversed(candidates), key=lambda method: -current[method.lower()]),
    "candidate_permutation_ordinal": sorted(reversed(candidates), key=lambda method: -separate[method]),
}
Path(__file__).with_name("e02_identity_proxy_result.json").write_text(
    json.dumps(result, indent=2) + "\n", encoding="utf-8")
print(json.dumps(result, indent=2))
