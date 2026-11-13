#!/usr/bin/env python3
"""Source-equivalent BPE budget probe; no .NET, model, optimizer or quality judge.

Mirrors BpeTokenizer.Train/Encode and RegexSplitter for the finite fixture
alphabet. Python unicode category membership is used only for the ASCII and
Cyrillic characters present here. Ties use first insertion, as in the inspected
CountPairs/MaxBy iteration. Results require native confirmation before a
production claim. The script does not read or write any repository model.
"""
import hashlib
import json
import re
import unicodedata
from collections import Counter
from pathlib import Path

PIN = "4945b8e85c17b5f150eb5b840d16bbdc4b506242"
OLD = "219d7ba89dc3ef43e03e17f1461ad96154d3dd7a"
PREFIX = "SECURITY AUTHORIZATION API_CONTRACT ARCHITECTURE IMPLEMENT TEST QA UI DOCUMENT LEARNING capability generated learned flow selected rejected"
UNIQUE = " ".join(f"semantic_seed_{i:03d}" for i in range(512))
REPEATED = " ".join("semantic_seed_000" for _ in range(512))

FIXTURES = [
    {
        "id": "documentation_order",
        "domain": "documentation",
        "text": "Document the behavior of the payment request handler. The handler validates the account, checks authorization and starts a database transaction. It writes the payment, commits the transaction and then publishes an audit event. If validation fails, it returns an error without writing anything. If the database write fails, it rolls back and does not publish the audit event. Explain the observable effects and the order of these operations. Avoid confusing a rollback with a retry. The payment key must remain unchanged across retries.",
        "query": "Explain how the payment request handler validates the account, checks authorization, starts a database transaction, writes the payment and commits the transaction before publishing the audit event. Describe the failure rule: rollback prevents publication.",
        "critical": "rollback",
    },
    {
        "id": "orchestration_order",
        "domain": "orchestration",
        "text": "Plan a recovery workflow. First create and verify a backup, then apply the database migration, verify the migrated records and activate the new service. Keep the old service available until verification succeeds. If migration fails, restore the backup. If verification fails, route traffic back to the old service. Never remove the backup before the new service has processed an independent check. Record the completed steps and continue from the last completed step after a restart.",
        "query": "Plan a recovery workflow that creates and verifies a backup, applies the database migration, verifies migrated records and activates the new service while keeping the old service available until verification succeeds. The final constraint is rollback.",
        "critical": "rollback",
    },
    {
        "id": "translation_ru_negation",
        "domain": "translation",
        "text": "Переведи описание операции, сохранив условие и отрицание. Сначала проверь документ и подпись, затем отправь запрос. Если подпись неверна, запрос отправлять нельзя. После подтверждения сохрани ответ и уведомление. При ошибке повтори проверку, но не создавай второй платёж. Перевод должен сохранять порядок действий, обязательность проверки, запрет повторной оплаты и различие между ошибкой отправки и отказом получателя.",
        "query": "Переведи описание операции: сначала проверь документ и подпись, затем отправь запрос, после подтверждения сохрани ответ и уведомление. Если подпись неверна, запрос отправлять нельзя.",
        "critical": "нельзя",
    },
    {
        "id": "programming_numeric_control",
        "domain": "programming",
        "text": "Implement parsing of numeric identifiers and HTTP response codes. Accept identifiers 001 002 003 010 020 100 101 200 201 202 400 401 403 404 409 500 501 502 503. Preserve leading zeros in record identifiers. Treat 200 as success, 401 as missing credentials, 403 as denied permission, 409 as conflict and 503 as a temporary unavailable service. Retry only the temporary failure. Do not retry a denied permission and do not reinterpret the record identifier as a response code.",
        "query": "Classify identifiers 001 002 003 010 020 100 101 200 201 202 400 401 403 404 409 500 501 502 503 without removing leading zeros. The final response code is 503.",
        "critical": "503",
    },
]


def make_splitter(all_text):
    letters = "".join(sorted(c for c in set(all_text) if unicodedata.category(c).startswith("L")))
    numbers = "".join(sorted(c for c in set(all_text) if unicodedata.category(c).startswith("N")))
    ls, ns = re.escape(letters), re.escape(numbers)
    # Exact membership for this finite fixture alphabet, not a general .NET regex port.
    return re.compile(r"'s|'t|'re|'ve|'m|'ll|'d| ?[" + ls + r"]+| ?[" + ns + r"]+| ?[^\s" + ls + ns + r"]+|\s+(?!\S)|\s+")


class Bpe:
    def __init__(self, splitter):
        self.splitter = splitter
        self.vocab = {i: bytes([i]) for i in range(256)}
        self.merges = []

    @staticmethod
    def merge(ids, pair, new_id):
        out, i = [], 0
        while i < len(ids):
            if i + 1 < len(ids) and (ids[i], ids[i + 1]) == pair:
                out.append(new_id)
                i += 2
            else:
                out.append(ids[i])
                i += 1
        return out

    def train(self, text, size=512):
        chunks = [list(s.encode("utf-8")) for s in self.splitter.findall(text)]
        assert "".join(self.splitter.findall(text)) == text
        while len(self.vocab) < size:
            counts = Counter()
            for chunk in chunks:
                counts.update(zip(chunk, chunk[1:]))
            if not counts:
                break
            pair = max(counts, key=counts.get)
            new_id = len(self.vocab)
            self.vocab[new_id] = self.vocab[pair[0]] + self.vocab[pair[1]]
            self.merges.append((pair, new_id))
            chunks = [self.merge(c, pair, new_id) for c in chunks]
        return self

    def encode(self, text):
        result = []
        for s in self.splitter.findall(text):
            ids = list(s.encode("utf-8"))
            for pair, new_id in self.merges:
                ids = self.merge(ids, pair, new_id)
            result.extend(ids)
        return result

    def decode_bytes(self, ids):
        return b"".join(self.vocab[i] for i in ids)

    def state_hash(self):
        data = [[a, b, token] for (a, b), token in self.merges]
        return hashlib.sha256(json.dumps(data, separators=(",", ":")).encode()).hexdigest()


def main():
    universe = PREFIX + UNIQUE + REPEATED + "".join(f["text"] + f["query"] for f in FIXTURES)
    splitter = make_splitter(universe)
    output = {
        "status": "MECHANISM_PROXY_ONLY_NOT_NATIVE_LEARNING",
        "current_code_sha": PIN,
        "donor_code_sha": OLD,
        "unicode_version": unicodedata.unidata_version,
        "byte_vocabulary": 256,
        "vocabulary_budget": 512,
        "native_model_or_training_executed": False,
        "seed_bytes_equal_control": len(UNIQUE.encode()) == len(REPEATED.encode()),
        "seed_utf8_bytes": len(UNIQUE.encode()),
        "seed_text": PREFIX,
        "fixtures": [],
    }
    for fixture in FIXTURES:
        record = dict(fixture)
        record["arms"] = {}
        for arm, filler in [("A_current_unique_numeric_seed", UNIQUE), ("B_same_length_repeated_seed_control", REPEATED), ("C_no_filler_diagnostic", "")]:
            train_input = f"{PREFIX} {fixture['text']} {filler}"
            bpe = Bpe(splitter).train(train_input)
            ids = bpe.encode(fixture["query"])
            assert bpe.decode_bytes(ids) == fixture["query"].encode("utf-8")
            prefix_bytes = bpe.decode_bytes(ids[:64])
            prefix = prefix_bytes.decode("utf-8", errors="replace")
            all_merged = [(token, data) for token, data in bpe.vocab.items() if token >= 256]
            numeric = [token for token, data in all_merged if data.strip().isdigit()]
            filler_usage = set(bpe.encode(UNIQUE))
            lesson_usage = set(bpe.encode(PREFIX + " " + fixture["text"]))
            exclusively_filler = [token for token, _ in all_merged if token in filler_usage and token not in lesson_usage]
            record["arms"][arm] = {
                "train_input_sha256": hashlib.sha256(train_input.encode()).hexdigest(),
                "merge_recipe_sha256": bpe.state_hash(),
                "vocab_size": len(bpe.vocab),
                "merge_count": len(bpe.merges),
                "numeric_merge_count": len(numeric),
                "final_tokens_used_in_unique_filler_but_not_lesson_count": len(exclusively_filler),
                "query_token_count": len(ids),
                "query_ids": ids,
                "prefix64_utf8_byte_count": len(prefix_bytes),
                "prefix64_text": prefix,
                "critical_last_occurrence_byte_end": fixture["query"].encode("utf-8").rfind(fixture["critical"].encode("utf-8")) + len(fixture["critical"].encode("utf-8")),
                "critical_in_prefix64": len(prefix_bytes) >= fixture["query"].encode("utf-8").rfind(fixture["critical"].encode("utf-8")) + len(fixture["critical"].encode("utf-8")),
                "roundtrip_ok": True,
            }
        output["fixtures"].append(record)
    result_path = Path(__file__).with_name("seed_probe_results.json")
    result_path.write_text(json.dumps(output, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    for f in output["fixtures"]:
        print(f["id"], {a: {k: v[k] for k in ["vocab_size", "numeric_merge_count", "query_token_count", "prefix64_utf8_byte_count", "critical_in_prefix64"]} for a, v in f["arms"].items()})


if __name__ == "__main__":
    main()
