#!/usr/bin/env python3
"""Поиск по проверенным карточкам и указателю источников. Python 3, без зависимостей."""
import argparse
import json
import math
import re
from collections import Counter
from pathlib import Path
from urllib.parse import quote

ROOT = Path(__file__).resolve().parent
STOP = set('как почему что это этого именно в на и с по из для при или ли же не без чтобы чем the a an of to in and why how'.split())
SYNONYMS = {
    'лора': ['lora'], 'лоpa': ['lora'], 'кнн': ['knn'],
    'токенизация': ['tokenizer', 'bpe'], 'токенизатор': ['tokenizer', 'bpe'],
    'бпе': ['bpe'], 'внимание': ['attention'], 'оптимизатор': ['optimizer', 'adamw'],
    'перевод': ['translation', 'enru'], 'контекст': ['context'],
    'забывание': ['retention', 'optimizer'], 'вероятности': ['softmax'],
    'градиент': ['gradient', 'backprop'], 'память': ['memory'],
    'разметка': ['labeling'], 'маршрут': ['routing'], 'род': ['gender'],
}


def tokens(value):
    full = re.findall(r'[\w]+(?:-[\w]+)*', str(value).lower().replace('ё', 'е'))
    parts = [part for token in full if '-' in token for part in token.split('-')]
    return [t for t in full + parts if t not in STOP and len(t) > 1]


def expanded(query):
    result = tokens(query)
    for t in list(result):
        result.extend(SYNONYMS.get(t, []))
    return list(dict.fromkeys(result))


def flatten(value):
    if isinstance(value, dict):
        return ' '.join(flatten(v) for v in value.values())
    if isinstance(value, list):
        return ' '.join(flatten(v) for v in value)
    return str(value or '')


def source_url(ref):
    url = 'https://github.com/{}/blob/{}/{}'.format(ref['repo'], ref['ref'], quote(ref['path'], safe='/'))
    if ref.get('anchor'):
        url += '#' + ref['anchor']
    elif ref.get('lines'):
        lines = ref['lines']
        if isinstance(lines, list):
            url += '#L{}-L{}'.format(lines[0], lines[-1])
        elif isinstance(lines, str):
            line_range = re.search(r'(\d+)(?:\D+(\d+))?', lines)
            if line_range:
                url += '#L' + line_range[1] + ('-L' + line_range[2] if line_range[2] else '')
    return url


def rank(rows, query, mode):
    terms = expanded(query)
    weighted = []
    for row in rows:
        if mode == 'cards':
            fields = [(row.get('id'), 8), (row.get('title_ru'), 5),
                      (row.get('aliases'), 5), (row.get('stage'), 4),
                      (row.get('question_ru'), 3), (row.get('summary_ru'), 2),
                      (row.get('limits_ru'), 1), (row.get('numeric_result_ru'), 1)]
        else:
            fields = [(row.get('path'), 5), (row.get('title'), 4),
                      (row.get('headings'), 2), (row.get('linked_cards'), 4)]
        counts = Counter()
        for value, weight in fields:
            for token in tokens(flatten(value)):
                counts[token] += weight
        weighted.append(counts)
    document_frequency = Counter(t for counts in weighted for t in counts)
    results = []
    for row, counts in zip(rows, weighted):
        score = 0.0
        matched = []
        for term in terms:
            tf = counts.get(term, 0)
            # Небольшая поддержка русских окончаний; это лексический поиск, не embeddings.
            if tf == 0 and len(term) >= 6:
                variants = [v for k, v in counts.items() if k.startswith(term[:6])]
                tf = max(variants, default=0) * 0.45
            if tf:
                idf = math.log(1 + len(rows) / (1 + document_frequency.get(term, 0)))
                score += idf * math.log(1 + tf)
                matched.append(term)
        if score or not terms:
            result = dict(row)
            result['_score'] = round(score * (1 + len(matched) / max(1, len(terms))), 3)
            result['_matched'] = matched
            results.append(result)
    return sorted(results, key=lambda r: (-r['_score'], r.get('id', r.get('path', ''))))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('query', nargs='?', default='', help='Вопрос, название опыта или часть пути')
    parser.add_argument('--stage', help='Например: tokenizer, knn, lora, optimizer, attention')
    parser.add_argument('--kind', choices=['experiment', 'book', 'all'], default='all')
    parser.add_argument('--outcome', help='Например: quality_gain, resource_gain, tradeoff')
    parser.add_argument('--sources', action='store_true', help='Искать в полном указателе источников')
    parser.add_argument('--limit', type=int, default=5)
    parser.add_argument('--json', action='store_true', help='JSON для следующего агента или RAG')
    args = parser.parse_args()
    if args.limit < 1:
        parser.error('--limit должен быть положительным')
    if args.sources:
        if args.stage or args.outcome or args.kind != 'all':
            parser.error('--sources не совмещается с фильтрами карточек; используйте слова в запросе')
        rows = json.loads((ROOT / 'sources.json').read_text(encoding='utf-8'))['sources']
    else:
        rows = [json.loads(line) for line in (ROOT / 'chunks.jsonl').read_text(encoding='utf-8').splitlines() if line]
        rows = [r for r in rows if (args.kind == 'all' or r['kind'] == args.kind)
                and (not args.stage or args.stage in r.get('stage', []))
                and (not args.outcome or args.outcome == r.get('outcome_kind'))]
    result = rank(rows, args.query, 'sources' if args.sources else 'cards')[:args.limit]
    if args.json:
        print(json.dumps(result, ensure_ascii=False, indent=2))
        return
    if not result:
        print('Совпадений нет. Уберите фильтр, попробуйте E-номер или --sources. Отсутствие карточки не означает отсутствие опыта.')
        return
    for row in result:
        if args.sources:
            print('\n{}\n{}\n{}\nДоступ: {}'.format(row.get('title') or row['path'], row['path'], row['url'], row['content_access']))
            continue
        print('\n[{}] {} | {}'.format(row['id'], row['title_ru'], ', '.join(row['stage'])))
        print(row['summary_ru'])
        if row.get('numeric_result_ru'):
            print('Результат:', row['numeric_result_ru'])
        print('Граница:', row['limits_ru'])
        print('Основание:', row['evidence_level'])
        for ref in row['source_refs'][:3]:
            print('Источник:', source_url(ref))
        if len(row['source_refs']) > 3:
            print('Остальные источники: --json')


if __name__ == '__main__':
    main()
