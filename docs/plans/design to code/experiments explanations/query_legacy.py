#!/usr/bin/env python3
"""Offline retrieval for the experiment/book navigation packet (Python 3, stdlib).

Examples:
  python query.py "Вчера Олег"
  python query.py --selection main
  python query.py "stronger-baseline=28/32" --kind experiment
  python query.py SVD --kind experiment
  python query.py "токенизация" --stage tokenizer --json
  python query.py CDG68 --catalog
  python query.py "Principles.cs" --sources --kind source

This is a lexical retrieval layer for RAG, not a vector database or a generator.
The default corpus is reviewed mechanism cards + book explanations. Catalog and
source metadata are explicitly opt-in and never imply a successful experiment.
"""

import argparse
import json
import re
import sys
import unicodedata
from pathlib import Path
from urllib.parse import quote

KINDS = ("experiment", "book", "catalog", "source")
KIND_LABELS = {
    "experiment": "Разобранный механизм",
    "book": "Математика из книги",
    "catalog": "Каталог — только навигация",
    "source": "Источник — метаданные",
}
STAGE_LABELS = {
    "tokenizer": "Токенизация", "features": "Признаки и контекст",
    "knn": "KNN", "memory": "Память", "routing": "Маршрутизация",
    "optimizer": "Оптимизатор", "lora": "LoRA", "dpo": "DPO",
    "attention": "Attention", "tensor": "Тензоры и градиенты",
    "evaluation": "Оценка результата", "normalization": "Нормализация",
    "training": "Обучение", "matrix": "Матрица / факторизация",
    "backprop": "Обратное распространение", "cache": "Кэширование",
    "composition": "Составление частей", "embedding": "Векторное представление",
    "format": "Формат ответа", "history": "История событий",
    "ingress": "Обработка входа", "linear_classifier": "Линейный классификатор",
    "neural_weights": "Веса нейросети", "padding": "Выравнивание длины",
    "pretraining": "Предварительное обучение", "readout": "Чтение результата",
    "reranking": "Ранжирование кандидатов", "segmentation": "Разделение запроса",
    "sft": "Обучение по правильным ответам", "softmax": "Вероятности softmax",
    "training_data": "Учебные данные", "trust": "Доверие к источнику",
    "vector_index": "Векторный индекс",
}
ALIAS_GROUPS = [
    ["knn", "кнн", "nearest", "neighbors", "neighbours", "сосед", "соседи", "ближайшие"],
    ["bpe", "бпе", "tokenizer", "tokenization", "токенизация", "токенизатор", "токенизировать"],
    ["lora", "лора", "low-rank", "низкоранговый", "низкоранговая"],
    ["svd", "singular", "сингулярное", "сингулярный", "факторизация", "factorization"],
    ["dpo", "дпо", "preference", "предпочтение", "предпочтения"],
    ["context", "contextual", "контекст", "контекстный", "контекстные"],
    ["pos", "части-речи", "часть-речи", "части", "речи"],
    ["routing", "router", "маршрут", "маршрутизация", "маршрутизатор"],
    ["memory", "память", "памяти", "запоминание"],
    ["attention", "внимание", "внимания"],
    ["gradient", "gradients", "градиент", "градиенты", "градиента", "backward"],
    ["optimizer", "оптимизатор", "оптимизатора", "adamw", "адам"],
    ["normalization", "normalisation", "нормализация", "нормализации", "rmsnorm"],
    ["cache", "caching", "кэш", "кеш", "кэширование", "кеширование"],
    ["allocation", "allocations", "выделения", "аллокации", "выделений"],
    ["retention", "удержание", "сохранение", "забывание"],
    ["directed", "direction", "направленный", "направление", "направленные"],
    ["gender", "род", "рода", "грамматический"],
    ["tense", "time-frame", "прошедшее", "временной"],
    ["yesterday", "вчера"],
    ["stage", "этап", "этапы", "этапов"],
    ["probability", "probabilities", "вероятность", "вероятности", "softmax"],
    ["matrix", "матрица", "матрицы", "матрицу"],
    ["embedding", "embeddings", "эмбеддинг", "эмбеддинги", "представление"],
    ["training", "learning", "обучение", "обучения", "обучать"],
    ["evaluation", "оценка", "метрика", "измерение", "readout"],
]
STOPWORDS = set("а и или в во на с со к по для из у о об от до это этой этот как почему что чем работает работают было стало the a an and or in on of for to how why is are was with".split())
FIELD_WEIGHTS = {"id": 34, "aliases": 23, "title": 17, "stage": 11,
                 "stage_tags": 9, "domain": 6, "summary": 7, "baseline": 3,
                 "change": 7, "learned_state": 3, "reason": 5, "formula": 5,
                 "result": 3, "limitations": 2, "extra": 1,
                 "current_status": 17, "current_review": 6, "book_quotes": 5, "uml": 2,
                 "selection_status": 22, "selection_review": 12, "selection_category": 16}
SELECTION_LABELS = {
    "all": "Все карточки: новый отбор и история",
    "main": "Основные кандидаты дополнительной проверки",
    "tradeoff": "Измеренный компромисс: CDG68 и SVD",
    "manual": "Ручные правила чтения: KDP",
    "alternative": "Краткие альтернативы: CDG03 и CDG09",
    "bug": "Исправление ошибки: HCL",
    "reviewed": "Все карточки дополнительной проверки",
}


def normalize(value):
    text = unicodedata.normalize("NFKC", str(value)).lower().replace("ё", "е")
    text = re.sub(r"[‐‑‒–—−]", "-", text)
    return re.sub(r"\s+", " ", text).strip()


def tokens(value):
    text = normalize(value)
    ordinary = re.findall(r"[a-zа-я0-9]+", text)
    compounds = re.findall(r"[a-zа-я0-9]+(?:[-_.][a-zа-я0-9]+)+", text)
    return set(ordinary + compounds)


def flatten(value):
    if value is None:
        return ""
    if isinstance(value, str):
        return value
    if isinstance(value, dict):
        return " · ".join(str(k) + ": " + flatten(v) for k, v in value.items())
    if isinstance(value, (list, tuple)):
        return " · ".join(flatten(v) for v in value)
    return str(value)


def first(row, *keys, default=""):
    for key in keys:
        if key in row and row[key] not in (None, "", []):
            return row[key]
    return default


def list_text(value):
    if value is None or value == "":
        return []
    if isinstance(value, list):
        return [flatten(x) for x in value if x is not None]
    return [flatten(value)]


def collect_refs(row, kind):
    refs = []
    for key in ("sources", "source_refs", "evidence_sources", "report_refs", "primary_refs", "additional_sources"):
        value = row.get(key, [])
        if isinstance(value, dict):
            value = [value]
        refs.extend(x for x in value if isinstance(x, dict))
    for key in ("card_ref", "source_ref"):
        if isinstance(row.get(key), dict):
            refs.append(row[key])
    if isinstance(row.get("coverage_review"), dict):
        refs.extend(collect_refs(row["coverage_review"], "catalog"))
    if row.get("url") and kind in ("book", "source"):
        refs.insert(0, row)
    unique = []
    seen = set()
    for ref in refs:
        address = ref.get("url", "")
        if not address and ref.get("path") and ref.get("commit"):
            repo = ref.get("repository", ref.get("repo", "lubochka/llm_mvp_core"))
            address = "https://github.com/" + repo + "/blob/" + ref["commit"] + "/" + quote(ref["path"], safe="/")
            if ref.get("line_start"):
                address += "#L" + str(ref["line_start"])
                if ref.get("line_end"):
                    address += "-L" + str(ref["line_end"])
        if not address.startswith(("https://", "http://")) or address in seen:
            continue
        seen.add(address)
        unique.append({"url": address, "path": ref.get("path", ""),
                       "label": first(ref, "label", "role", "supports_ru", "section", "path", default="Источник"),
                       "commit": ref.get("commit", ref.get("ref", "")),
                       "line_start": ref.get("line_start"), "line_end": ref.get("line_end")})
    return unique


def infer_book_stages(row):
    text = normalize(" ".join(flatten(row.get(k, "")) for k in
                              ("title_ru", "section", "plain_explanation_ru", "applicable_experiments")))
    mapping = {
        "tokenizer": ("токен получает", "bpe", "tokeniz"),
        "features": ("контекст", "context", "позици", "перестанов"),
        "knn": ("knn", "k-nn", "ближайш"),
        "attention": ("attention", "внимани"),
        "lora": ("lora", "лора", "низкоранг"),
        "dpo": ("dpo", "предпочтен"),
        "optimizer": ("adam", "оптимизатор", "момент"),
        "matrix": ("svd", "матриц", "симметри", "факториза"),
        "tensor": ("градиент", "backward", "якоби", "производн"),
        "normalization": ("rmsnorm", "нормализа"),
        "memory": ("памят", "memory"),
        "evaluation": ("оценк", "вероятност", "softmax"),
    }
    return [tag for tag, values in mapping.items() if any(v in text for v in values)]


def normalize_record(row, kind):
    identity = str(first(row, "id", "path", default=kind))
    title = flatten(first(row, "title_ru", "title", "task_ru", "topic_ru", "path", default=identity))
    aliases = list_text(row.get("aliases", []))
    current_review = row.get("current_review", {})
    selection_review = row.get("selection_review", {})
    aliases.extend(list_text(current_review.get("aliases", [])))
    aliases.extend(list_text(row.get("host")))
    # Short stable IDs are often present as hosts or path components, e.g. PQ01.
    aliases.extend(re.findall(r"\b[A-Z][A-Z0-9]*\d+[A-Z0-9]*\b", identity))
    refs = collect_refs(row, kind)
    for ref in refs:
        aliases.extend(re.findall(r"/RP01-MICRO/([^/#?]+)", ref.get("path", "") or ref.get("url", "")))
    aliases = list(dict.fromkeys(x for x in aliases if x))
    stage = flatten(first(row, "stage", "stage_ru", "section"))
    stage_tags = row.get("stage_tags", [])
    if isinstance(stage_tags, str):
        stage_tags = [stage_tags]
    if not stage_tags and kind == "book":
        stage_tags = infer_book_stages(row)
    change = flatten(first(row, "change_ru", "treatment_ru", "candidate_ru", "after_ru"))
    reason = flatten(first(row, "why_ru", "plain_explanation_ru", "mathematical_explanation_ru", "explanation_ru"))
    summary = change or reason or flatten(first(row, "topic_ru", "recorded_status", "design_status", "headings"))
    limitations = list_text(first(row, "limitations", "limitations_ru", "limits_ru", "limits", default=[]))
    if kind == "catalog":
        limitations.insert(0, "Только навигационная запись каталога. Наличие отчёта или RESULT_RECORDED не означает доказанный успех.")
    if kind == "source":
        limitations.insert(0, "Метаданные источника: эта запись сама по себе не подтверждает результат опыта и не содержит полный текст.")
    notice = flatten(first(row, "evidence_notice_ru", "verification_scope", "relation_to_project", "review_status"))
    result = first(row, "measured_result", "metrics", "result", "results", default={})
    formula = first(row, "formula", "formulas", "formula_latex", "mathematical_explanation_ru", default="")
    extra_keys = ("classification", "outcome_kind", "book_relation", "book_mapping_hint",
                  "applicable_experiments", "coverage_review", "recorded_status", "design_status",
                  "example", "example_ru", "examples", "headings", "path", "category", "content_access")
    result_row = {
        "id": identity, "kind": kind, "title": title, "aliases": aliases,
        "stage": stage, "stage_tags": stage_tags,
        "domain": flatten(first(row, "domain", "learning_topic", "topic_ru")),
        "summary": summary, "baseline": flatten(first(row, "usual_ru", "baseline_ru", "before_ru")),
        "change": change, "learned_state": flatten(first(row, "learned_state_ru", "learner_ru")),
        "reason": reason, "formula": formula, "result": result,
        "limitations": limitations, "sources": refs, "notice": notice,
        "extra": " · ".join(flatten(row[k]) for k in extra_keys if k in row),
        "source_snapshot": row.get("source_snapshot", ""),
        "current_status": current_review.get("status_ru", ""),
        "current_review": current_review,
        "current_sources": collect_refs(current_review, kind) if current_review else [],
        "selection_status": selection_review.get("status_ru", ""),
        "selection_category": selection_review.get("classification", ""),
        "selection_review": selection_review,
        "main_method_improvement": bool(selection_review.get("include_in_main", False)),
        "selection_sources": collect_refs(selection_review, kind) if selection_review else [],
        "book_quotes": row.get("book_quotes", []),
        "uml": row.get("uml", {}),
    }
    if kind == "book":
        result_row["pages"] = row.get("pdf_pages", [])
        result_row["section"] = row.get("section", "")
    if kind == "catalog":
        result_row["status"] = flatten(first(row, "recorded_status", "design_status"))
    if kind == "source":
        result_row["access"] = row.get("content_access", "metadata_only")
        result_row["commit"] = row.get("commit", "")
    return result_row


def read_rows(path, key=None):
    if not path.is_file():
        raise FileNotFoundError("Нет файла данных: " + str(path))
    if path.suffix == ".jsonl":
        return [json.loads(line) for line in path.read_text(encoding="utf-8-sig").splitlines() if line.strip()]
    data = json.loads(path.read_text(encoding="utf-8-sig"))
    if isinstance(data, list):
        return data
    return data.get(key, [])


def load_records(directory, kinds=KINDS):
    files = {"experiment": ("mechanisms-reviewed.jsonl", None), "book": ("book-map.json", "records"),
             "catalog": ("catalog.jsonl", None), "source": ("sources.json", "sources")}
    records = []
    for kind in KINDS:
        if kind in kinds:
            filename, key = files[kind]
            records.extend(normalize_record(row, kind) for row in read_rows(Path(directory) / filename, key))
    quote_file = Path(directory) / "book-quotes.json"
    if "book" in kinds and quote_file.is_file():
        for quote in read_rows(quote_file, "records"):
            row = dict(quote)
            row.update(id="QUOTE-" + quote["id"],
                       title_ru="Цитата " + quote["id"] + ": " + quote["topic_ru"],
                       aliases=[quote["id"], *quote.get("case_ids", [])],
                       plain_explanation_ru=quote.get("bridge_ru", ""),
                       limitations_ru=quote.get("limits_ru", ""),
                       formula={"book": quote.get("book_formula_latex", ""),
                                "project": quote.get("project_formula_latex", ""),
                                "provenance": quote.get("formula_provenance", "")},
                       book_quotes=[quote],
                       evidence_notice_ru="Короткий точный фрагмент книги; область применимости и наш вывод указаны отдельно.")
            records.append(normalize_record(row, "book"))
    return records


def prepare(records):
    for row in records:
        row["_fields"] = {name: normalize(flatten(row.get(name, ""))) for name in FIELD_WEIGHTS}
        row["_tokens"] = {name: tokens(value) for name, value in row["_fields"].items()}
    return records


def query_terms(query):
    values = tokens(query)
    # A compound ID is useful on its own; its component words must not dilute it.
    if re.fullmatch(r"[a-z0-9]+(?:[-_.][a-z0-9]+)+", normalize(query)):
        return [normalize(query)]
    return sorted(t for t in values if t not in STOPWORDS and (len(t) > 1 or t.isdigit()))


def variants(term):
    result = {term}
    for group in ALIAS_GROUPS:
        normal = [normalize(x) for x in group]
        if term in normal:
            result.update(normal)
    return result


def term_match(term, options, words, text):
    if term in words:
        return 1.0
    if any(option in words for option in options if option != term):
        return 0.68
    # Conservative ending tolerance, useful for Russian inflection, not stemming.
    if len(term) >= 5 and re.fullmatch(r"[а-я]+", term):
        stem = term[:-2] if len(term) >= 7 else term[:-1]
        if any(word.startswith(stem) for word in words):
            return 0.42
    if len(term) >= 3 and re.fullmatch(r"[a-z]+\d*", term):
        if any(word.startswith(term) for word in words):
            return 0.38
    if "-" in term and term in text:
        return 0.65
    return 0.0


def stage_matches(row, requested):
    if not requested:
        return True
    tags = {normalize(tag) for tag in row.get("stage_tags", [])}
    for value in requested:
        value = normalize(value)
        for tag, label in STAGE_LABELS.items():
            if value == normalize(label) or value in variants(tag):
                value = tag
                break
        if value in tags:
            return True
        if value == normalize(row.get("stage", "")):
            return True
    return False


def selection_matches(row, requested):
    if requested == "all":
        return True
    category = row.get("selection_category", "")
    if requested == "main":
        return row["kind"] == "experiment" and row.get("main_method_improvement") is True
    if requested == "reviewed":
        return row["kind"] == "experiment" and category not in ("", "historical_not_reselected")
    return category == {"tradeoff": "measured_tradeoff", "manual": "fixed_rule_readout",
                        "alternative": "training_schedule_alternative", "bug": "implementation_bug_fix"}.get(requested)


def rank(records, query="", kinds=("experiment", "book"), stages=(), limit=10, selection="all"):
    phrase = normalize(query)
    terms = query_terms(query)
    expanded = {term: variants(term) for term in terms}
    hits = []
    for row in records:
        if row["kind"] not in kinds or not stage_matches(row, stages) or not selection_matches(row, selection):
            continue
        if phrase == "не внедрено" and "не внедрено" not in normalize(row.get("current_status", "")):
            continue
        if "_fields" not in row:
            prepare([row])
        fields, wordsets = row["_fields"], row["_tokens"]
        score = 0.0
        matched = []
        identities = [normalize(row["id"])] + [normalize(x) for x in row["aliases"]]
        if phrase and phrase in identities:
            score += 1000.0
        elif phrase and any(phrase in value for value in identities):
            score += 180.0
        if phrase and len(phrase) >= 4:
            for field, weight in FIELD_WEIGHTS.items():
                if phrase in fields[field]:
                    score += weight * 1.4
        for term in terms:
            contributions = [weight * term_match(term, expanded[term], wordsets[field], fields[field])
                             for field, weight in FIELD_WEIGHTS.items()]
            strongest = max(contributions, default=0)
            if strongest:
                matched.append(term)
                score += strongest + 0.12 * (sum(contributions) - strongest)
        if terms:
            score *= 0.35 + 0.65 * len(matched) / len(terms)
        if phrase and score <= 0:
            continue
        # Empty query presents reviewed mechanisms before book explanations.
        clean = {key: value for key, value in row.items() if not key.startswith("_")}
        clean.update(score=round(score, 4), matched_terms=matched)
        hits.append(clean)
    hits.sort(key=lambda row: (-row["score"], 0 if not phrase and row.get("main_method_improvement") else 1,
                              KINDS.index(row["kind"]), normalize(row["id"])))
    return hits[:limit], len(hits)


def shorten(text, length=550):
    text = re.sub(r"\s+", " ", flatten(text)).strip()
    return text if len(text) <= length else text[:length - 1].rsplit(" ", 1)[0] + "…"


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("query", nargs="*", help="слова, фраза или точный ID")
    parser.add_argument("--stage", action="append", default=[], help="точный тег этапа; повторение/запятая = ИЛИ")
    parser.add_argument("--kind", action="append", default=[], help="experiment, book, catalog, source; можно повторять")
    parser.add_argument("--catalog", action="store_true", help="добавить навигационный каталог")
    parser.add_argument("--sources", action="store_true", help="добавить метаданные источников")
    parser.add_argument("--selection", choices=SELECTION_LABELS, default="all", help="фильтр нового отбора; main исключает HCL, CDG68 и SVD")
    parser.add_argument("--limit", type=int, default=10, help="число результатов, 1–500 (по умолчанию 10)")
    parser.add_argument("--json", action="store_true", help="машиночитаемый JSON с доказательствами и ограничениями")
    parser.add_argument("--data-dir", type=Path, default=Path(__file__).resolve().parent, help="папка файлов индекса")
    args = parser.parse_args(argv)
    if not 1 <= args.limit <= 500:
        parser.error("--limit должен быть в диапазоне 1–500")
    kinds = {part.strip() for item in args.kind for part in item.split(",") if part.strip()} or {"experiment", "book"}
    if not kinds.issubset(KINDS):
        parser.error("--kind: допустимы experiment, book, catalog, source")
    if args.catalog:
        kinds.add("catalog")
    if args.sources:
        kinds.add("source")
    stages = [part.strip() for item in args.stage for part in item.split(",") if part.strip()]
    query = " ".join(args.query)
    try:
        records = prepare(load_records(args.data_dir, kinds))
        results, total = rank(records, query, kinds, stages, args.limit, args.selection)
    except (OSError, ValueError) as exc:
        parser.error(str(exc))
    output = {"query": query, "engine": "offline weighted lexical retrieval; no embeddings or generation",
              "kinds": [k for k in KINDS if k in kinds], "stages": stages, "selection": args.selection,
              "matched_records": total, "returned_records": len(results), "results": results}
    if args.json:
        print(json.dumps(output, ensure_ascii=False, indent=2))
        return 0
    print("Локальный поиск по карточкам: %s. Найдено %d, показано %d.\n" % (query or "все", total, len(results)))
    for index, row in enumerate(results, 1):
        print("%d. [%s] %s — %s" % (index, KIND_LABELS[row["kind"]], row["id"], row["title"]))
        print("   Этап: " + (row["stage"] or ", ".join(row["stage_tags"]) or "не указан"))
        if row["selection_review"]:
            print("   Новый отбор: " + row["selection_status"])
            print("   Вывод: " + shorten(row["selection_review"].get("summary_ru", ""), 950))
            for key in ("losses_ru", "cost_assessment_ru", "stronger_baseline_ru"):
                if row["selection_review"].get(key):
                    print("   Уточнение: " + shorten(row["selection_review"][key], 700))
            if row["selection_sources"]:
                print("   Основание нового отбора: " + row["selection_sources"][0]["url"])
        if row["current_review"]:
            print("   Текущая проверка (" + row["current_review"].get("checked_commit", "")[:12] + "): " + row["current_status"])
            print("   Сейчас: " + shorten(row["current_review"].get("current_ru", ""), 600))
            if row["current_review"].get("current_repair_metrics"):
                print("   Свежие измерения: " + shorten(row["current_review"]["current_repair_metrics"], 500))
        if row["summary"]:
            print("   " + shorten(row["summary"]))
        if row["result"]:
            print("   Измерение исходного опыта: " + shorten(row["result"], 450))
        if row["limitations"]:
            print("   Граница: " + shorten(" ".join(row["limitations"]), 650))
        for ref in row["sources"][:3]:
            print("   Источник: " + ref["url"])
        if len(row["sources"]) > 3:
            print("   Ещё источников: %d; полный результат доступен с --json." % (len(row["sources"]) - 3))
        if row["current_sources"]:
            print("   Источник текущей проверки: " + row["current_sources"][0]["url"])
        if row["book_quotes"]:
            q = row["book_quotes"][0]
            print("   Книга: «" + q["quote"] + "» — " + q.get("translation_ru", "") + "; " + q.get("url", ""))
        if row["uml"]:
            print("   Исторические UML: до / после / причина; новый selection_review имеет приоритет. Полный исходник в --json.")
        print()
    if not results:
        print("Совпадений нет. Попробуйте ID, более короткую фразу или добавьте --catalog / --sources.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
