"""Source-level mirror of E10 normalization. This is not Roslyn/.NET/KNN execution."""
import json
import re
from pathlib import Path

def normalize(source):
    if not source.strip():
        return source
    value = re.sub(r"///[^\n]*", "", source)
    value = re.sub(r"/\*.*?\*/", " ", value, flags=re.S)
    value = re.sub(r"//[^\n]*", "", value)
    value = re.sub(r'@"(?:[^"]|"")*"|"(?:[^"\\]|\\.)*"', '""', value)
    return re.sub(r"\n(\s*\n){2,}", "\n\n", value)

cases = [
    {
        "id": "url_removes_executable_tail",
        "source": 'class C { string F(string user) { var endpoint = "https://example.test"; return user.Length.ToString(); } }',
        "expected_normalized": 'class C { string F(string user) { var endpoint = "https:',
        "mechanism": "line-comment regex matches a delimiter inside a string before strings are recognized"
    },
    {
        "id": "interpolation_removes_member_access",
        "source": 'class C { string F(string user) { return $"{user.Length}"; } }',
        "expected_normalized": 'class C { string F(string user) { return $""; } }',
        "mechanism": "string regex removes a real interpolation expression"
    },
    {
        "id": "control_plain_literal_preserves_code",
        "source": 'class C { int F(string user) { var message = "plain"; return user.Length; } }',
        "expected_normalized": 'class C { int F(string user) { var message = ""; return user.Length; } }',
        "mechanism": "negative control: ordinary literal content is removed, executable member access remains"
    },
    {
        "id": "control_real_comment_preserves_code",
        "source": 'class C { int F(string user) { /* explanation */ return user.Length; } }',
        "expected_normalized": 'class C { int F(string user) {   return user.Length; } }',
        "mechanism": "negative control: a real block comment is removed without erasing executable text"
    },
]

rows = []
for case in cases:
    actual = normalize(case["source"])
    rows.append({**case, "normalized": actual, "matches_expected": actual == case["expected_normalized"]})
if not all(row["matches_expected"] for row in rows):
    raise SystemExit("A regex mirror case disagreed with the declared expectation.")

result = {
    "scope": "Python mirror of four simple ASCII cases for the exact source regex sequence",
    "native_dotnet_roslyn": "NOT_RUN",
    "model_training": "NOT_RUN",
    "dpo_generation_or_judging": "NOT_RUN",
    "semantic_quality_gain": "NOT_MEASURED",
    "cases": rows,
}
Path(__file__).with_name("regex_proxy_result.json").write_text(
    json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(result, ensure_ascii=False, indent=2))
