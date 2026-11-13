# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR07-REPEAT/ru-en; domain tr.en; profile ru-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 740; after it, domain remainder: 250. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Переводить однозначное слово после одного случайного повторения буквы; связывать чистую, ошибочную и layout-формы одной лексемы, сохраняя исходную строку.",
  "instructions": "Сначала составь грамматически правильную исходную фразу и выбери ровно одно обычное слово в той форме, которая реально стоит в предложении. Выбери индекс i одного Unicode code point и повтори этот же символ сразу после него: raw = canonical[:i+1] + canonical[i] + canonical[i+1:]. Длина raw на один code point больше canonical. При удалении вставленного символа по i+1 должно получиться исходное слово побуквенно, включая регистр. Запиши canonical_token, raw_token, mutation.kind=repeat_one_codepoint, mutation.index=i, inserted_index=i+1 и character. Индексы нулевые, не UTF-8 bytes и не model tokens. Строка source_raw и learner prompt содержат raw, а canonical_source хранится только в metadata. Не «упрощай» законные двойные буквы: book→boook — одна вставка; book→bok — уже удаление.\nChosen переводит восстановленное слово; rejected подменяет только этот предмет другой естественной лексемой, сохраняя действие, место, участников и остальные факты. Не используй бессмысленную строку как rejected и не выдавай правильный синоним за ошибку. Для Hebrew возьми простые слова без niqqud и без преобразования final forms; вставленный code point буквальный. Не добавляй опечатку в другое слово. В каждом request 10 новых семей назначенной темы: пять случаев повторения внутри слова, три у начала и два у конца, если назначенный материал позволяет; это распределение позиции, а не дополнительные темы. Каждый пример получает ровно одну фактическую вставку. Уже назначенное содержательное распределение request имеет приоритет перед этими примерами позиционного разнообразия.\nПростая самопроверка до записи: выпиши длины n→n+1, убери только inserted_index, сравни весь результат с canonical_token; затем назови один предмет, который неверно переведён в rejected. Эти действия помогают авторингу, отдельный review/отчёт об этом не создаётся. Лексическая семья canonical/raw/layout остаётся той же при подтверждённом смысле; молоко и сок в одну lexical family не сливаются. Если вывод о семейном происхождении не следует из назначенного общего сюжета, не выдумывай его.",
  "answer_contract": "Только полный перевод исходной строки на target-языке; объяснения и авторская самопроверка остаются вне learner prompt/chosen/rejected.",
  "hard_constraints": [
    "FORBIDDEN: Удаление, замена, перестановка или два повторения одновременно.",
    "FORBIDDEN: Удаление законной двойной буквы как будто она всегда опечатка.",
    "FORBIDDEN: Смена падежа/рода/окончания вместе с повторением.",
    "FORBIDDEN: Нормализация Hebrew final forms или niqqud вместо буквальной вставки.",
    "FORBIDDEN: Несколько направлений, objective или split в одном shard.",
    "FORBIDDEN: Скрытая подсказка в rationale/metadata вместо видимого контекста.",
    "FORBIDDEN: Объявление допустимого синонима или перевода плохим ответом.",
    "FORBIDDEN: Разделение переводов и производных одного исходного сюжета между TRAIN/DEV/EXAM.",
    "FORBIDDEN: Перенос публичных reference и производных в запечатанный DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "repeated_position",
      "values": [
        "near_start",
        "interior",
        "last"
      ]
    },
    {
      "axis": "legitimate_double_letter",
      "values": [
        "none",
        "one_already_present"
      ]
    },
    {
      "axis": "context_length",
      "values": [
        "one_sentence",
        "two_short_sentences"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A031:P00011","family_id":"D1000-20261025:TR07-REPEAT:F00011"},
{"pair_id":"D1000-20261025:A031:P00012","family_id":"D1000-20261025:TR07-REPEAT:F00012"},
{"pair_id":"D1000-20261025:A031:P00013","family_id":"D1000-20261025:TR07-REPEAT:F00013"},
{"pair_id":"D1000-20261025:A031:P00014","family_id":"D1000-20261025:TR07-REPEAT:F00014"},
{"pair_id":"D1000-20261025:A031:P00015","family_id":"D1000-20261025:TR07-REPEAT:F00015"},
{"pair_id":"D1000-20261025:A031:P00016","family_id":"D1000-20261025:TR07-REPEAT:F00016"},
{"pair_id":"D1000-20261025:A031:P00017","family_id":"D1000-20261025:TR07-REPEAT:F00017"},
{"pair_id":"D1000-20261025:A031:P00018","family_id":"D1000-20261025:TR07-REPEAT:F00018"},
{"pair_id":"D1000-20261025:A031:P00019","family_id":"D1000-20261025:TR07-REPEAT:F00019"},
{"pair_id":"D1000-20261025:A031:P00020","family_id":"D1000-20261025:TR07-REPEAT:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A031\B0002\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A031\B0002\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '1bf8f1a110074547a4321185f94cb487'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A031\B0002\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A031\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR07, profile_id=ru-en, split=train. metadata.learning_objective_id=TR07-REPEAT; metadata.scenario_group=TR07-REPEAT; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR07-REPEAT.ru-en.REF01",
    "profile_id": "ru-en",
    "family_id": "v3-ref.translation.repeat.milk-table",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. В одном обычном слове случайно повторена ровно одна соседняя буква. Восстанови это слово и переведи предложение, сохранив остальные факты.\nИсходный текст: Моолоко на столе.",
    "chosen": "The milk is on the table.",
    "rejected": "The juice is on the table.",
    "error_tag": "single_repeated_character_wrong_lexeme",
    "rationale": "Удаление ровно добавленной повторной буквы возвращает исходную лексему. Rejected подменяет предмет другим; место и остальные факты не меняются.",
    "simple_check": "Удаление ровно добавленной повторной буквы возвращает исходную лексему. Rejected подменяет предмет другим; место и остальные факты не меняются.",
    "counterexample": "Нельзя удалять букву из правильного слова только из-за двойной буквы: book имеет законное oo. В boook лишняя только одна o. Допустимый синоним перевода не становится rejected.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Моолоко на столе.",
      "canonical_source": "Молоко на столе.",
      "canonical_token": "Молоко",
      "raw_token": "Моолоко",
      "mutation": {
        "kind": "repeat_one_codepoint",
        "index": 1,
        "inserted_index": 2,
        "character": "о"
      },
      "lexical_family_ids": [
        "lex:ru:молоко"
      ],
      "sense_id": "milk_beverage",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR07-REPEAT.ru-en.REF02",
    "profile_id": "ru-en",
    "family_id": "v3-ref.translation.repeat.book-bag",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. В одном обычном слове случайно повторена ровно одна соседняя буква. Восстанови это слово и переведи предложение, сохранив остальные факты.\nИсходный текст: Книгаа в сумке.",
    "chosen": "The book is in the bag.",
    "rejected": "The notebook is in the bag.",
    "error_tag": "single_repeated_character_wrong_lexeme",
    "rationale": "Удаление ровно добавленной повторной буквы возвращает исходную лексему. Rejected подменяет предмет другим; место и остальные факты не меняются.",
    "simple_check": "Удаление ровно добавленной повторной буквы возвращает исходную лексему. Rejected подменяет предмет другим; место и остальные факты не меняются.",
    "counterexample": "Нельзя удалять букву из правильного слова только из-за двойной буквы: book имеет законное oo. В boook лишняя только одна o. Допустимый синоним перевода не становится rejected.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Книгаа в сумке.",
      "canonical_source": "Книга в сумке.",
      "canonical_token": "Книга",
      "raw_token": "Книгаа",
      "mutation": {
        "kind": "repeat_one_codepoint",
        "index": 4,
        "inserted_index": 5,
        "character": "а"
      },
      "lexical_family_ids": [
        "lex:ru:книга"
      ],
      "sense_id": "printed_book",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR07-REPEAT.ru-en.bottle-one-repetition",
    "domain_id": "tr.en",
    "profile_id": "ru-en",
    "objective_id": "TR07-REPEAT",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Речь о пустой стеклянной ёмкости. В одном слове ровно одна буква случайно повторена; удали только лишнюю копию.\nИсходный текст: Буутылка пустая.",
    "chosen": "The bottle is empty.",
    "rejected": "The glass is empty.",
    "error_tag": "bottle-one-repetition",
    "rationale": "The object stays a bottle and its state stays empty. Changing grammatical agreement to match the wrong noun does not create another semantic defect.",
    "simple_check": "Canonical token 'Бутылка' has 7 code points; raw 'Буутылка' has 8. Removing the inserted code point at raw position 2 restores the exact canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The object stays a bottle and its state stays empty. Changing grammatical agreement to match the wrong noun does not create another semantic defect.",
    "wrong_authoring_trap": {
      "bad_authoring": "Calling repeated adjacent letters a transposition or requiring a unique insertion index when identical letters permit more than one witness.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "Бутылка пустая.",
      "source_raw": "Буутылка пустая.",
      "canonical_token": "Бутылка",
      "raw_token": "Буутылка",
      "mutation": {
        "kind": "insert_one_codepoint",
        "index": 2,
        "inserted_codepoint": "у"
      },
      "canonical_token_span": {
        "start": 0,
        "end": 7,
        "unit": "unicode_codepoints"
      },
      "raw_token_span": {
        "start": 0,
        "end": 8,
        "unit": "unicode_codepoints"
      },
      "mechanical_construction_check": {
        "status": "CHECKED_REFERENCE_CONSTRUCTION_ONLY",
        "canonical_length": 7,
        "raw_length": 8,
        "whole_sentence_restored_exactly": true
      },
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.bottle-one-repetition",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.bottle-one-repetition"
      ],
      "source_language": "ru",
      "response_language": "en",
      "generator_model": "reference_authoring_not_a_Chinese_model_run",
      "review_status": "NOT_REQUESTED",
      "training_status": "NOT_RUN",
      "semantic_status": "illustrative_authoring_not_independent_attestation",
      "learner_visible_fields": [
        "prompt"
      ],
      "response_contract": "translation_only",
      "lexical_family_ids": [
        "lex:ru:бутылка:container"
      ],
      "lemma": "бутылка",
      "sense_id": "container",
      "surface_form": "Бутылка"
    },
    "supplement_path": "campaign/translation-quality-lessons-2026-10-26.json"
  }
]
```

## Assigned source excerpts

These are bounded, partial source excerpts; they are not a claim to include the entire original or all its requirements. Use the exact linked local file if a detail needed by this lesson is absent.

RAG-023 — rag/sources/RAG-023.md
Различает сырое переводимое предложение, выбранный перевод и тип ошибки; описывает морфологию RU/HE и четыре старых направления.
Applicability: Актуально шесть направлений EN/RU/HE и раздельные темы. Не наследовать overlap>0.60, [WRONG]-аннотации и тезис «высокое сходство значит нет сигнала»: минимальная смысловая пара может быть полезной.
Primary excerpt (partial; full excerpt at the assigned path):
## Why This Document Exists

The orchestration DPO guidance covers ARCH, PLAN, SKILL, and CODE sub-phases.
Translation is a separate capability domain with different:

- A-side source (raw sentence, not compiled orchestration prompt)
- Chosen authority (verified parallel corpus, not XIIGen internal contract)
- Rejected error taxonomy (linguistic errors, not routing/governance errors)
- Reward margin mechanics (token-level linguistic divergence, not predicate match)

Read this document instead of DPO-PAIR-AUTHORING-GUIDANCE-v2.md when writing
translation pairs. Do not mix translation pairs into orchestration batches.

---

## Reward Margin Mechanics for Translation

DPO computes for every pair:


RAG-024 — rag/sources/RAG-024.md
Рассматривает Unicode-скрипты, Hebrew final forms/nikud и смешанные технические слова внутри предложения.
Applicability: Измерять покрытие на закреплённом локальном наборе, сохранять raw текст и привязку к семье. Не выполнять внешние загрузки из старого руководства; не объявлять его пороги и нормализацию доказанно оптимальными.
Primary excerpt (partial; full excerpt at the assigned path):
## Hebrew-Specific Tokenizer Notes

### Nikud (Vowel Points)

Hebrew vowel diacritics (U+05B0–U+05BD, U+05C1–U+05C2, U+05C7) combine with
base characters. The tokenizer must treat base + nikud as a single combining
character sequence, not split them.

Recommendation: normalise Hebrew text to NFD (Unicode Canonical Decomposition)
before tokenisation. This ensures base letters and combining marks are consistently
separated at the unicode level, then the BPE merge rules can recombine them.

### Final Letter Forms

Hebrew has five final letters (ף ך ם ן ץ) that replace their regular forms
at word end. These are separate Unicode code points (not combining characters).
The tokenizer must treat them as distinct from their non-final counterparts:
- פ (pe) ≠ ף (final pe)
- כ (kaf) ≠ ך (final kaf)
- מ (mem) ≠ ם (final mem)
- נ (nun) ≠ ן (final nun)
- צ (tsadi) ≠ ץ (final tsadi)

Verify that the vocabulary contains both forms for each pair.

### Gershayim and Geresh

- Geresh (׳ U+05F3): marks abbreviations and foreign-origin letters
- Gershayim (״ U+05F4): marks acronyms and compound abbreviations

These must be in the vocabulary as single tokens, not split as punctuation.

## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A031\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A031\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A031\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A031\B0002\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '1bf8f1a110074547a4321185f94cb487'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
