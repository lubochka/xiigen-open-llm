#!/usr/bin/env python3
"""
Читает предложения из stdin (одно предложение на строку),
возвращает POS-теги в JSON (массив строк) в stdout.
Используется из C# через Process.
"""
import sys
import json
import spacy

nlp = spacy.load("en_core_web_sm")

for line in sys.stdin:
    line = line.strip()
    if not line:
        print("[]")
        sys.stdout.flush()
        continue
    doc = nlp(line)
    tags = [token.pos_ for token in doc if not token.is_space]
    print(json.dumps(tags))
    sys.stdout.flush()
