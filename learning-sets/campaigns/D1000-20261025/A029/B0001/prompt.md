# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR08-RESTORE-OR-PRESERVE/ru-en; domain tr.en; profile ru-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 690; after it, domain remainder: 300. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "При одинаковом raw token различать исправление случайной раскладки обычного слова и точное сохранение идентификатора по роли фрагмента в видимом условии.",
  "instructions": "Подготовь контрфактические DPO-пары для одного направления перевода: одно и то же значение поля, набранное буквами другой раскладки, в одном видимом контексте является ошибочно набранным обычным словом, а в другом — точным буквальным идентификатором. В обеих строках оставь неизменными raw token, исходную фразу, карту клавиш, формат ответа и все факты кроме роли значения. Контекст роли обязателен прямо в learner prompt. Не раскрывай canonical_source, ответ, family ID или rationale в prompt. Для каждого назначенного family_id сделай две строки: metadata.span_treatment=restore и preserve. Вторая меняет предпочтение: chosen первой становится rejected второй и наоборот, если перевод окружающего текста неизменен. В каждом из пяти новых сценарных семейств текущей партии используй свою лексему и свой самодостаточный контекст; повторный пример с другим ID или именем не считается независимым сценарием. Используй только C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/domains/translation/layout-fixture-v1.json, только поддержанные unshifted code points. В каждый learner prompt вставь полную таблицу соответствия выбранных двух раскладок из fixture, а не только её имя и не только символы текущего ответа. В restore/preserve контрфактуалах карта одинакова. Raw получи механически из canonical, затем проверь обратное преобразование и точные code-point границы raw внутри исходной строки. Для ru-source допустимо en-qwerty→ru-ycuken, для he-source en-qwerty→he-fixture, для en-source ru-ycuken→en-qwerty или he-fixture→en-qwerty. Сохрани ordinary canonical/typo/wrong-layout в одной лексической семье. Буквальный идентификатор не объединяй с лексемой по совпадению символов; контрфактическая пара всё равно имеет один scenario family и split. Каждая партия10: пять restore и пять preserve, связанные попарно; не додумывай дополнительные темы и профили. Другой естественный перевод, синоним или допустимый порядок слов не является rejected. Неоднозначное намерение без заданного типа не размечай силой — отправь в пересмотр/назначенную TR10-тему. Не делай все restore-случаи латиницей, а preserve кириллицей: внутри каждой семьи raw совпадает. Разнообразь формулировки видимого контекста и окружающего текста между семьями; не обучай угадывать метку по одному шаблонному слову. Пиши draft.jsonl в назначенный output_directory; текущая стадия заканчивается структурным finish без независимого предметного review.\nMaterial for this objective stores source_text_by_class_language: ordinary restored text for restore, and the unchanged literal identifier for preserve. The learner-visible raw token is the same within the paired classes; do not force both canonical_source values to be identical.",
  "answer_contract": "Только перевод исходной строки на язык назначения; обычное слово восстановить и перевести, буквальный идентификатор сохранить точно. Не переводить контекст и не добавлять объяснение.",
  "hard_constraints": [
    "FORBIDDEN: Смешивание нескольких направлений перевода или objective в одном request.",
    "FORBIDDEN: Объявление каждого смешанного алфавита ошибкой и автоматическое исправление всех строк.",
    "FORBIDDEN: Объединение буквального ID с лексемой только по совпадающему raw.",
    "FORBIDDEN: Скрытое намерение: роль значения есть только в metadata, но отсутствует в learner prompt.",
    "FORBIDDEN: Одновременная опечатка, другая карта, изменение пола/роли/смысла окружающего предложения.",
    "FORBIDDEN: Разделение restore/preserve, обратных переводов и повторных форм одной семьи между TRAIN/DEV/EXAM.",
    "FORBIDDEN: Перенос публичных reference и их производных в запечатанный экзамен."
  ],
  "scenario_axes": [
    {
      "axis": "span_treatment",
      "values": [
        "restore",
        "preserve"
      ]
    },
    {
      "axis": "carrier",
      "values": [
        "field_value",
        "short_sentence",
        "quoted_value"
      ]
    },
    {
      "axis": "restoration_map",
      "values": [
        "source_ru_en_to_ru",
        "source_he_en_to_he",
        "source_en_ru_to_en",
        "source_en_he_to_en"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A029:P00001","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00001","family_class":"restore"},
{"pair_id":"D1000-20261025:A029:P00002","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00001","family_class":"preserve"},
{"pair_id":"D1000-20261025:A029:P00003","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00002","family_class":"restore"},
{"pair_id":"D1000-20261025:A029:P00004","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00002","family_class":"preserve"},
{"pair_id":"D1000-20261025:A029:P00005","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00003","family_class":"restore"},
{"pair_id":"D1000-20261025:A029:P00006","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00003","family_class":"preserve"},
{"pair_id":"D1000-20261025:A029:P00007","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00004","family_class":"restore"},
{"pair_id":"D1000-20261025:A029:P00008","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00004","family_class":"preserve"},
{"pair_id":"D1000-20261025:A029:P00009","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00005","family_class":"restore"},
{"pair_id":"D1000-20261025:A029:P00010","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00005","family_class":"preserve"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

Set metadata.span_treatment to the exact slot family_class. Produce each assigned class once for that family; do not invent a class mixture.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A029\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A029\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

Material kind layout_counterfactual: write source_text_by_class_language={restore:{en:text,ru:text,he:text},preserve:{en:text,ru:text,he:text}}. Restore contains the decoded ordinary source text; preserve contains the exact RAW identifier source text. These canonical values differ by class. Both learner inputs use the same RAW source within the pair; their visible contexts establish ordinary word versus literal identifier. metadata.canonical_source uses the current class and source language from this material.

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

Also save learner_visible_context_by_class as {class:{language:context}} for both assigned classes, covering these source languages: ["en", "he", "ru"]. Empty context is allowed when missing context is the assigned condition. Contexts of the two classes must differ. Supply only that class's source-language context to the learner, not the entire author-only facts record. A contrast must be justified in the assigned source language, not inferred from translations.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR08, profile_id=ru-en, split=train. metadata.learning_objective_id=TR08-RESTORE-OR-PRESERVE; metadata.scenario_group=TR08-RESTORE-OR-PRESERVE; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-024", "RAG-023"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR08-RESTORE-OR-PRESERVE.ru-en.REF01",
    "profile_id": "ru-en",
    "family_id": "v2-ref.translation.ticket-on-table",
    "prompt": "Переведи с языка «русский» на язык «английский» только исходную строку ниже. Сохрани структуру «название поля: значение»; не добавляй объяснение. Буквальные идентификаторы сохраняются точно; обычное слово со случайной ошибкой раскладки восстанавливается и переводится. Для этого примера доступна карта layout-fixture-v1: фактическая раскладка en-qwerty, предполагаемая ru-ycuken. Наличие карты само по себе не доказывает, что значение ошибочно. Полная карта для этого случая: каждый ключ JSON — символ фактической раскладки, значение — символ предполагаемой; применяется посимвольно, остальные символы остаются на месте. {\"q\": \"й\", \"w\": \"ц\", \"e\": \"у\", \"r\": \"к\", \"t\": \"е\", \"y\": \"н\", \"u\": \"г\", \"i\": \"ш\", \"o\": \"щ\", \"p\": \"з\", \"[\": \"х\", \"]\": \"ъ\", \"a\": \"ф\", \"s\": \"ы\", \"d\": \"в\", \"f\": \"а\", \"g\": \"п\", \"h\": \"р\", \"j\": \"о\", \"k\": \"л\", \"l\": \"д\", \";\": \"ж\", \"'\": \"э\", \"z\": \"я\", \"x\": \"ч\", \"c\": \"с\", \"v\": \"м\", \"b\": \"и\", \"n\": \"т\", \"m\": \"ь\", \",\": \"б\", \".\": \"ю\", \"/\": \".\"} Контекст значения: Значение поля — обычное слово о документе для проезда, который пассажир предъявляет контролёру. Пользователь сообщил, что случайно набрал именно значение в неверной раскладке; это не код и не идентификатор.\nИсходная строка: Значение: ,bktn",
    "chosen": "Value: ticket",
    "rejected": "Value: ,bktn",
    "error_tag": "missed_layout_restoration",
    "rationale": "Видимый контекст объявляет случайную ошибку раскладки в обычном слове. Восстановление по указанной карте даёт слово о проездном документе; его непереведённая последовательность клавиш не передаёт смысл.",
    "simple_check": "Видимый контекст объявляет случайную ошибку раскладки в обычном слове. Восстановление по указанной карте даёт слово о проездном документе; его непереведённая последовательность клавиш не передаёт смысл.",
    "counterexample": "Та же исходная строка с противоположным явно заданным типом значения требует другого ответа. Если тип не указан и оба чтения правдоподобны, это TR10-CONDITIONAL, не угадывание здесь.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Значение: ,bktn",
      "canonical_source": "Значение: билет",
      "raw_token": ",bktn",
      "lexical_family_ids": [
        "lex:ru:билет"
      ],
      "response_contract": "translate_label_then_contextual_span",
      "span_treatment": "restore",
      "layout_from": "en-qwerty",
      "layout_to": "ru-ycuken",
      "map_id": "layout-fixture-v1",
      "source_span_codepoints": [
        10,
        15
      ],
      "decoded_if_transformed": "билет",
      "exact_identifiers": [],
      "do_not_merge": []
    }
  },
  {
    "example_id": "TR08-RESTORE-OR-PRESERVE.ru-en.REF02",
    "profile_id": "ru-en",
    "family_id": "v2-ref.translation.ticket-on-table",
    "prompt": "Переведи с языка «русский» на язык «английский» только исходную строку ниже. Сохрани структуру «название поля: значение»; не добавляй объяснение. Буквальные идентификаторы сохраняются точно; обычное слово со случайной ошибкой раскладки восстанавливается и переводится. Для этого примера доступна карта layout-fixture-v1: фактическая раскладка en-qwerty, предполагаемая ru-ycuken. Наличие карты само по себе не доказывает, что значение ошибочно. Полная карта для этого случая: каждый ключ JSON — символ фактической раскладки, значение — символ предполагаемой; применяется посимвольно, остальные символы остаются на месте. {\"q\": \"й\", \"w\": \"ц\", \"e\": \"у\", \"r\": \"к\", \"t\": \"е\", \"y\": \"н\", \"u\": \"г\", \"i\": \"ш\", \"o\": \"щ\", \"p\": \"з\", \"[\": \"х\", \"]\": \"ъ\", \"a\": \"ф\", \"s\": \"ы\", \"d\": \"в\", \"f\": \"а\", \"g\": \"п\", \"h\": \"р\", \"j\": \"о\", \"k\": \"л\", \"l\": \"д\", \";\": \"ж\", \"'\": \"э\", \"z\": \"я\", \"x\": \"ч\", \"c\": \"с\", \"v\": \"м\", \"b\": \"и\", \"n\": \"т\", \"m\": \"ь\", \",\": \"б\", \".\": \"ю\", \"/\": \".\"} Контекст значения: Значение поля — буквальный идентификатор записи. Опечатки в нём нет; все его символы значимы, алиасов нет. Совпадение с последовательностью клавиш обычного слова не меняет идентификатор.\nИсходная строка: Значение: ,bktn",
    "chosen": "Value: ,bktn",
    "rejected": "Value: ticket",
    "error_tag": "false_layout_restoration_of_identifier",
    "rationale": "Видимый контекст задаёт точный идентификатор без ошибки. Перевод названия поля допустим, но замена буквального значения обычным словом создаёт другой идентификатор.",
    "simple_check": "Видимый контекст задаёт точный идентификатор без ошибки. Перевод названия поля допустим, но замена буквального значения обычным словом создаёт другой идентификатор.",
    "counterexample": "Та же исходная строка с противоположным явно заданным типом значения требует другого ответа. Если тип не указан и оба чтения правдоподобны, это TR10-CONDITIONAL, не угадывание здесь.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Значение: ,bktn",
      "canonical_source": "Значение: ,bktn",
      "raw_token": ",bktn",
      "lexical_family_ids": [],
      "response_contract": "translate_label_then_contextual_span",
      "span_treatment": "preserve",
      "layout_from": "en-qwerty",
      "layout_to": "ru-ycuken",
      "map_id": "layout-fixture-v1",
      "source_span_codepoints": [
        10,
        15
      ],
      "decoded_if_transformed": "билет",
      "exact_identifiers": [
        ",bktn"
      ],
      "do_not_merge": [
        "lex:ru:билет"
      ]
    }
  }
]
```

## Assigned source excerpts

These are bounded, partial source excerpts; they are not a claim to include the entire original or all its requirements. Use the exact linked local file if a detail needed by this lesson is absent.

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


## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A029\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A029\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A029\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A029\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
