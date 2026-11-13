# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR03-COMPLETION/ru-en; domain tr.en; profile ru-en.

Write exactly 10 rows, batch 3/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 410; after it, domain remainder: 580. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Передавать наличие либо отсутствие завершения к явно указанному моменту, не уравнивая грамматические системы трёх языков.",
  "instructions": "Составь короткие истории с одним действием и явной временной точкой. В одном классе текст прямо говорит «уже закончил», в другом — «ещё делал и не закончил». Chosen сохраняет состояние завершённости; rejected меняет только его. Базовые времена языков не объявляй взаимно однозначными: русский вид, английский aspect и ивритское время не являются одним полем. Запиши отдельно source grammatical form и semantic completion_status. Если завершённость в исходнике не следует ни из формы, ни из контекста, отправь пример на пересмотр либо в TR10-CONDITIONAL, не дописывай скрытый смысл. В каждой партии 10 строк должно быть 5 completed и 5 in_progress_not_finished. Не заменяй этот урок задачей распознавания одного слова «finished»: разнообразь глаголы и контекст, сохраняя доказуемую границу. Обе стороны должны оставаться естественными; различающий глагол/морфема не должен исчезать после реальной токенизации или обрезки.",
  "answer_contract": "Только перевод исходного текста на целевой язык; метаданные и объяснение в ответ не входят.",
  "hard_constraints": [
    "FORBIDDEN: Сравнение вчера/завтра, смена участника или предмета как второй дефект.",
    "FORBIDDEN: Утверждение, что каждое русское совершенное/несовершенное время имеет один обязательный эквивалент в EN/HE.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "completion",
      "values": [
        "completed",
        "in_progress_not_finished"
      ]
    },
    {
      "axis": "activity",
      "values": [
        "write",
        "read",
        "assemble",
        "translate"
      ]
    },
    {
      "axis": "time_anchor",
      "values": [
        "noon",
        "before_meeting",
        "at_arrival"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A015:P00021","family_id":"D1000-20261025:TR03-COMPLETION:F00021"},
{"pair_id":"D1000-20261025:A015:P00022","family_id":"D1000-20261025:TR03-COMPLETION:F00022"},
{"pair_id":"D1000-20261025:A015:P00023","family_id":"D1000-20261025:TR03-COMPLETION:F00023"},
{"pair_id":"D1000-20261025:A015:P00024","family_id":"D1000-20261025:TR03-COMPLETION:F00024"},
{"pair_id":"D1000-20261025:A015:P00025","family_id":"D1000-20261025:TR03-COMPLETION:F00025"},
{"pair_id":"D1000-20261025:A015:P00026","family_id":"D1000-20261025:TR03-COMPLETION:F00026"},
{"pair_id":"D1000-20261025:A015:P00027","family_id":"D1000-20261025:TR03-COMPLETION:F00027"},
{"pair_id":"D1000-20261025:A015:P00028","family_id":"D1000-20261025:TR03-COMPLETION:F00028"},
{"pair_id":"D1000-20261025:A015:P00029","family_id":"D1000-20261025:TR03-COMPLETION:F00029"},
{"pair_id":"D1000-20261025:A015:P00030","family_id":"D1000-20261025:TR03-COMPLETION:F00030"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A015\B0003\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A015\B0003\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR03, profile_id=ru-en, split=train. metadata.learning_objective_id=TR03-COMPLETION; metadata.scenario_group=TR03-COMPLETION; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR03-COMPLETION.ru-en.REF01",
    "profile_id": "ru-en",
    "family_id": "TR03-reference-family-05",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. \nИсходный текст: К полудню она уже закончила писать отчёт.",
    "chosen": "By noon, she had already finished writing the report.",
    "rejected": "By noon, she had not yet finished writing the report.",
    "error_tag": "tr03_completion",
    "rationale": "В исходнике отчёт уже закончен к указанному моменту; rejected отрицает достигнутое завершение к тому же моменту.",
    "simple_check": "В исходнике отчёт уже закончен к указанному моменту; rejected отрицает достигнутое завершение к тому же моменту.",
    "counterexample": "Просто «она писала отчёт» без временного контекста не доказывает, что отчёт остался незавершённым позже; нельзя приписывать эту недостающую историю.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "К полудню она уже закончила писать отчёт.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR03-COMPLETION.ru-en.REF02",
    "profile_id": "ru-en",
    "family_id": "TR03-reference-family-03",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. \nИсходный текст: В полдень он ещё читал письмо и не закончил.",
    "chosen": "At noon, he was still reading the letter and had not finished.",
    "rejected": "At noon, he had already finished reading the letter.",
    "error_tag": "tr03_completion",
    "rationale": "Исходник прямо отрицает завершение чтения в полдень; rejected утверждает обратный результат.",
    "simple_check": "Исходник прямо отрицает завершение чтения в полдень; rejected утверждает обратный результат.",
    "counterexample": "Если оставить только «он читал письмо», выбранный результат требует повторной оценки: контекст о незавершённости удалён.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "В полдень он ещё читал письмо и не закончил.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR03-COMPLETION.ru-en.already-completed-lock",
    "domain_id": "tr.en",
    "profile_id": "ru-en",
    "objective_id": "TR03-COMPLETION",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nИсходный текст: Она уже починила замок.",
    "chosen": "She has already repaired the lock.",
    "rejected": "She is still repairing the lock.",
    "error_tag": "already-completed-lock",
    "rationale": "At the reference time the repair is finished. Rejected leaves it in progress. Changing tense/aspect markers together is one completion-state contrast.",
    "simple_check": "At the reference time the repair is finished. Rejected leaves it in progress. Changing tense/aspect markers together is one completion-state contrast.",
    "wrong_authoring_trap": {
      "bad_authoring": "Accepting the same subject and object while ignoring whether the action has reached its result.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "Она уже починила замок.",
      "source_raw": "Она уже починила замок.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.already-completed-lock",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.already-completed-lock"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A015\B0003\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A015\B0003\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A015\B0003\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A015\B0003\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
