#!/usr/bin/env python3
"""Read-only source-contract preflight; never imports or executes C#.

Print JSON to stdout. Input files are not modified. Direction names are
extracted only from the uniquely located TranslationRuntime CreateRuntime()
body, then cross-checked against that body's executorsByDirection map.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from pathlib import Path


def strip_cs_noncode(text: str) -> str:
    # Preserve character offsets and newlines while hiding comments/literals.
    # This bounded lexer covers ordinary/verbatim strings and chars in these
    # observed files. Interpolated content is ignored as a complete literal.
    token = re.compile(r'//[^\n]*|/\*[\s\S]*?\*/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'')
    return token.sub(lambda m: ''.join('\n' if c == '\n' else ' ' for c in m.group()), text)


def brace_body(masked: str, start: int) -> tuple[str, int, int]:
    opening = masked.find('{', start)
    if opening < 0:
        raise ValueError('Missing opening brace')
    depth = 0
    for i in range(opening, len(masked)):
        if masked[i] == '{':
            depth += 1
        elif masked[i] == '}':
            depth -= 1
            if depth == 0:
                return masked[opening + 1:i], opening + 1, i
    raise ValueError('Unbalanced braces')


def identity(path: Path, api_blob_sha: str | None) -> dict:
    raw = path.read_bytes()
    def git_blob(data: bytes) -> str:
        return hashlib.sha1(b'blob ' + str(len(data)).encode('ascii') + b'\0' + data).hexdigest()
    actual = git_blob(raw)
    # apply_patch may append one LF to an already newline-terminated tool file.
    without_added_lf = git_blob(raw[:-1]) if raw.endswith(b'\n') else None
    return {
        'path': str(path.resolve()), 'byte_length': len(raw),
        'sha256_local': hashlib.sha256(raw).hexdigest(),
        'git_blob_sha_local': actual, 'api_blob_sha': api_blob_sha,
        'api_blob_match_exact': actual == api_blob_sha if api_blob_sha else None,
        'api_blob_match_without_one_final_lf': without_added_lf == api_blob_sha if api_blob_sha else None,
    }


def runtime(path: Path, api_blob_sha: str | None) -> dict:
    text = path.read_text(encoding='utf-8-sig')
    masked = strip_cs_noncode(text)
    matches = list(re.finditer(r'private\s+static\s+TranslationRuntime\s+CreateRuntime\s*\(\s*\)', masked))
    if len(matches) != 1:
        raise ValueError(f'{path}: expected one CreateRuntime, got {len(matches)}')
    body, start, end = brace_body(masked, matches[0].end())
    created = re.findall(r'new\s+ExecutorUnit\s*\(\s*TranslationDirection\.([A-Z_]+)\s*,', body)
    map_matches = list(re.finditer(r'var\s+executorsByDirection\s*=\s*new\s+Dictionary\s*<\s*TranslationDirection\s*,\s*ExecutorUnit\s*>', body))
    if len(map_matches) != 1:
        raise ValueError(f'{path}: expected one executorsByDirection')
    map_body, _, _ = brace_body(body, map_matches[0].end())
    mapped = re.findall(r'\[\s*TranslationDirection\.([A-Z_]+)\s*\]\s*=', map_body)
    if not created or len(created) != len(set(created)) or set(created) != set(mapped):
        raise ValueError(f'{path}: constructor/map inconsistency')
    exported = re.findall(r'\bSchemaVersion\s*:\s*(\d+)', masked)
    accepted = re.findall(r'checkpoint\.SchemaVersion\s*!=\s*(\d+)', masked)
    return {
        'source': identity(path, api_blob_sha),
        'create_runtime_lines': [text.count('\n', 0, matches[0].start()) + 1, text.count('\n', 0, end) + 1],
        'created_directions': created, 'mapped_directions': mapped,
        'export_schema_literals': sorted(set(map(int, exported))),
        'accepted_schema_literals': sorted(set(map(int, accepted))),
        'complete_direction_guard_found_in_file': bool(re.search(r'_runtime\.Executors\.Keys\s*\.Except\s*\(\s*checkpoint\.Executors\.Select', masked)),
        'guard_refusal_literal_found_in_file': '"EXECUTOR_DIRECTIONS_INCOMPLETE"' in text,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--old-source', required=True, type=Path)
    parser.add_argument('--new-source', required=True, type=Path)
    parser.add_argument('--checkpoint', required=True, type=Path)
    parser.add_argument('--old-api-blob-sha')
    parser.add_argument('--new-api-blob-sha')
    parser.add_argument('--checkpoint-api-blob-sha')
    args = parser.parse_args()
    old = runtime(args.old_source, args.old_api_blob_sha)
    new = runtime(args.new_source, args.new_api_blob_sha)
    ckpt = json.loads(args.checkpoint.read_text(encoding='utf-8-sig'))
    directions = [row['direction'] for row in ckpt['executors']]
    old_missing = sorted(set(old['mapped_directions']) - set(directions))
    new_missing = sorted(set(new['mapped_directions']) - set(directions))
    result = {
        'classification': 'SOURCE_CONTRACT_REPRODUCTION_ONLY',
        'native_csharp_load_executed': False,
        'training_updates': 0,
        'quality_measured': False,
        'limits': ['No C# compilation/execution', 'Direction-set/schema preflight only; not every loader guard is simulated', 'Source parsing is bounded to observed C# syntax, not a general C# parser', 'Stored accuracy is metadata and not rescored'],
        'old_runtime': old, 'new_runtime': new,
        'checkpoint': {
            'source': identity(args.checkpoint, args.checkpoint_api_blob_sha),
            'schema_version': ckpt['schema_version'], 'directions': directions,
            'duplicate_directions': len(directions) != len(set(directions)),
            'has_structured_training': ckpt['has_structured_training'],
            'knn_triples': len(ckpt['knn_store']['entries']),
        },
        'old_missing_directions': old_missing,
        'new_missing_directions': new_missing,
        'removed_runtime_directions': sorted(set(old['mapped_directions']) - set(new['mapped_directions'])),
        'added_runtime_directions': sorted(set(new['mapped_directions']) - set(old['mapped_directions'])),
        'schema_same': old['export_schema_literals'] == new['export_schema_literals'] == [ckpt['schema_version']],
        'prediction': 'Current complete-direction guard would reject this checkpoint with EXECUTOR_DIRECTIONS_INCOMPLETE' if new_missing and new['complete_direction_guard_found_in_file'] and new['guard_refusal_literal_found_in_file'] else 'No direction-set rejection predicted by this bounded probe',
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
