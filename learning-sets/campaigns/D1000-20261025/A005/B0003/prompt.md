# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR07-VALIDWORD/ru-en; domain tr.en; profile ru-en.

Write exactly 10 rows, batch 3/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 110; after it, domain remainder: 880. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Не заменять самостоятельную лексему соседней по написанию лексемой без свидетельства опечатки.",
  "instructions": "Это отрицательный контроль ложной коррекции, а не новый набор орфографических ошибок. Выбери два похожих реальных слова source-языка с разным значением. Видимый контекст употребляет первое грамматически и семантически корректно; prompt прямо не сообщает о наличии опечатки. Chosen сохраняет первое значение; rejected переводит второе, будто оно было скрытым намерением. Оба target-ответа должны быть естественными в отрыве от исходника. Запиши distinct lexical_family_ids для обеих лексем, но только действительно изучаемая source-лексема является corrected/source family; do_not_merge содержит конкурента. Не сливай разные слова через общую форму, корень, расстояние или общий перевод. Не добавляй транслитерацию, клавиатуру, омографы с неразрешенным смыслом или правку технических идентификаторов: эти задачи отдельно. Варьируй валидную лексему в контекстах, исключающих чтение ее как ошибки.\nExact class composition: every assigned row preserves an already valid source word. Do not insert typo-restoration rows or invent a 5+5 mixture from a counterexample.",
  "answer_contract": "Только перевод на target-языке; в objectives уточнения — одна разрешенная конкретная просьба уточнить смысл, когда контекста недостаточно. Конкретный контракт каждого prompt имеет приоритет. Объяснения только в metadata/rationale.",
  "hard_constraints": [
    "FORBIDDEN: Неоднозначный ввод без контекста — это TR10-CONDITIONAL (класс ambiguous), не отрицательный контроль.",
    "FORBIDDEN: Синонимы, переводы, разные грамматические формы одного слова в качестве заведомо разных предметов.",
    "FORBIDDEN: Другие цели обучения, соседние objective_id и несколько профилей в одном shard.",
    "FORBIDDEN: Непроверенные карты раскладки, гадание о скрытом намерении, ошибочно связанные самостоятельные слова.",
    "FORBIDDEN: REFERENCE примеры, их переводы и производные в запечатанном EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "lexical_relation",
      "values": [
        "one_character_difference",
        "one_repeated_character_difference"
      ]
    },
    {
      "axis": "context",
      "values": [
        "object_in_photo",
        "reported_description",
        "observed_person_or_object"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A005:P00021","family_id":"D1000-20261025:TR07-VALIDWORD:F00021"},
{"pair_id":"D1000-20261025:A005:P00022","family_id":"D1000-20261025:TR07-VALIDWORD:F00022"},
{"pair_id":"D1000-20261025:A005:P00023","family_id":"D1000-20261025:TR07-VALIDWORD:F00023"},
{"pair_id":"D1000-20261025:A005:P00024","family_id":"D1000-20261025:TR07-VALIDWORD:F00024"},
{"pair_id":"D1000-20261025:A005:P00025","family_id":"D1000-20261025:TR07-VALIDWORD:F00025"},
{"pair_id":"D1000-20261025:A005:P00026","family_id":"D1000-20261025:TR07-VALIDWORD:F00026"},
{"pair_id":"D1000-20261025:A005:P00027","family_id":"D1000-20261025:TR07-VALIDWORD:F00027"},
{"pair_id":"D1000-20261025:A005:P00028","family_id":"D1000-20261025:TR07-VALIDWORD:F00028"},
{"pair_id":"D1000-20261025:A005:P00029","family_id":"D1000-20261025:TR07-VALIDWORD:F00029"},
{"pair_id":"D1000-20261025:A005:P00030","family_id":"D1000-20261025:TR07-VALIDWORD:F00030"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A005\B0003\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A005\B0003\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR07, profile_id=ru-en, split=train. metadata.learning_objective_id=TR07-VALIDWORD; metadata.scenario_group=TR07-VALIDWORD; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-024", "RAG-023"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR07-VALIDWORD.ru-en.REF01",
    "profile_id": "ru-en",
    "family_id": "v2-ref.translation.photo-hatch",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Речь о техническом отверстии доступа в полу; опечатка не заявлена.\nИсходный текст: На фотографии виден люк.",
    "chosen": "The photo shows an access hatch.",
    "rejected": "The photo shows an onion.",
    "error_tag": "false_typo_correction",
    "rationale": "Raw содержит корректное самостоятельное слово; rejected без основания заменяет его другим похожим словом.",
    "simple_check": "Raw содержит корректное самостоятельное слово; rejected без основания заменяет его другим похожим словом.",
    "counterexample": "Если raw действительно возник из документированной опечатки, это другой сценарий: нужны provenance и видимый разрешающий контекст, а не универсальное do-not-correct правило.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "На фотографии виден люк.",
      "canonical_source": "На фотографии виден люк.",
      "lexical_family_ids": [
        "lex:ru:люк"
      ],
      "sense_id": "access_hatch",
      "response_contract": "translation_only",
      "do_not_merge": [
        "lex:ru:лук"
      ]
    }
  },
  {
    "example_id": "TR07-VALIDWORD.ru-en.REF02",
    "profile_id": "ru-en",
    "family_id": "reference.translation.boy-drew-cat.v1",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод исходного текста, без перевода контекста и без объяснений. Контекст: Речь о рисунке мальчика. Исходное предложение записано правильно, опечатки не заявлены. Сохрани смысл правильного исходного текста. Не исправляй его в другое похожее слово.\nИсходный текст: Мальчик нарисовал кота.",
    "chosen": "The boy drew a cat.",
    "rejected": "The boy drew a whale.",
    "error_tag": "valid_word_overcorrected",
    "rationale": "Источник уже содержит правильное слово кота; требование восстановить опечатку отсутствует. Rejected переводит другую похожую лексему и меняет нарисованный предмет.",
    "simple_check": "The raw token is exactly the correct token 'кота'; no learner-input mutation is assigned. 'кита' is a different valid word, so translate 'кота' and do not merge their lexical families.",
    "counterexample": "кота и кита — формы разных существительных. Нельзя называть их опечатками одной лексической семьи только потому, что отличается одна буква.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Мальчик нарисовал кота.",
      "canonical_source": "Мальчик нарисовал кота.",
      "canonical_token": "кота",
      "raw_token": "кота",
      "mutation": {
        "kind": "none_valid_word"
      },
      "lexical_family_ids": [
        "lex:ru:кот"
      ],
      "sense_id": "domestic_cat",
      "response_contract": "translation_only",
      "do_not_merge": [
        "lex:ru:кит"
      ]
    }
  },
  {
    "example_id": "TQ26.TR07-VALIDWORD.ru-en.goat-valid-word",
    "domain_id": "tr.en",
    "profile_id": "ru-en",
    "objective_id": "TR07-VALIDWORD",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Говорится об одной самке животного. Исходная фраза записана правильно; исправление опечаток здесь не назначено. Переведи её как написано.\nИсходный текст: Коза возле ворот.",
    "chosen": "The goat is near the gate.",
    "rejected": "The tree is near the gate.",
    "error_tag": "goat-valid-word",
    "rationale": "The visible input names a goat. Hebrew עז and עץ are different correctly spelled words; proximity of spelling is not permission to change the animal into a tree.",
    "simple_check": "The visible input names a goat. Hebrew עז and עץ are different correctly spelled words; proximity of spelling is not permission to change the animal into a tree.",
    "wrong_authoring_trap": {
      "bad_authoring": "Adding a restoration lesson to half the batch merely because the guidance includes a counterexample.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "Коза возле ворот.",
      "source_raw": "Коза возле ворот.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.goat-valid-word",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.goat-valid-word"
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
      "response_contract": "translation_only"
    },
    "supplement_path": "campaign/translation-quality-lessons-2026-10-26.json"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A005\B0003\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A005\B0003\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A005\B0003\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A005\B0003\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
