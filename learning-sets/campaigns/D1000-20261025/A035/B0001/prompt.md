# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR03-EVENT-ORDER/ru-en; domain tr.en; profile ru-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 810; after it, domain remainder: 180. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Сохранять временную связь между двумя явно описанными событиями, даже когда порядок частей предложения отличается от порядка событий.",
  "instructions": "Создавай короткие предложения с ровно двумя завершёнными событиями и явным before/after, до/после или לפני/אחרי. Участники и предметы названы; местоимение допускается только при однозначном референте. В learner prompt проси сохранить, какое событие было раньше. Chosen передаёт исходную последовательность. Rejected меняет только временную связь на противоположную: before↔after, до↔после, לפני↔אחרי. Не меняй участников, глаголы, числа, отрицание и завершённость. Согласования, необходимые для естественной грамматики, должны оставаться правильными.\nДля партии из 10 назначенных сценариев используй 5 с before/до/לפני и 5 с after/после/אחרי. Внутри каждого класса варьируй главную/придаточную часть в начале или конце; не делай порядок слов всегда равным хронологии. Это одна тема временной связи, не смешение доменов. Не используй until/пока не/עד ש, условие if, одновременность, намерение или незавершённое действие: у них другие значения. Не создавай причинный вывод из одного временного порядка.\nСамопроверка: отдельно запиши A и B; затем «раньше ___; позже ___» для source, chosen и rejected. Source и chosen должны дать одну пару, rejected — обратную. Перефраз «After A, B»→«B after A» сохраняет A→B и поэтому не годится как rejected. В metadata event_order и rejected_event_order хранят порядок фактов; они не подставляются в learner input. Формулировки оставляй естественными в target-языке. Самопроверка — часть создания, не отдельный review-gate и не измерение обучения.",
  "answer_contract": "Только полный перевод исходной строки на target-языке; объяснения и авторская самопроверка остаются вне learner prompt/chosen/rejected.",
  "hard_constraints": [
    "FORBIDDEN: Условие, отрицание, until, одновременность или незавершённость как второй фактор.",
    "FORBIDDEN: Причинный вывод, не заданный исходником.",
    "FORBIDDEN: Объявление перестановки частей предложения семантической ошибкой, если временная связь прежняя.",
    "FORBIDDEN: Неоднозначные местоимения или изменение участника вместе со связкой.",
    "FORBIDDEN: Несколько направлений, objective или split в одном shard.",
    "FORBIDDEN: Скрытая подсказка в rationale/metadata вместо видимого контекста.",
    "FORBIDDEN: Объявление допустимого синонима или перевода плохим ответом.",
    "FORBIDDEN: Разделение переводов и производных одного исходного сюжета между TRAIN/DEV/EXAM.",
    "FORBIDDEN: Перенос публичных reference и производных в запечатанный DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "connector",
      "values": [
        "before",
        "after"
      ]
    },
    {
      "axis": "clause_order",
      "values": [
        "main_first",
        "subordinate_first"
      ]
    },
    {
      "axis": "agent_count",
      "values": [
        "one_explicit_agent",
        "two_named_agents"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A035:P00001","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00001"},
{"pair_id":"D1000-20261025:A035:P00002","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00002"},
{"pair_id":"D1000-20261025:A035:P00003","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00003"},
{"pair_id":"D1000-20261025:A035:P00004","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00004"},
{"pair_id":"D1000-20261025:A035:P00005","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00005"},
{"pair_id":"D1000-20261025:A035:P00006","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00006"},
{"pair_id":"D1000-20261025:A035:P00007","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00007"},
{"pair_id":"D1000-20261025:A035:P00008","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00008"},
{"pair_id":"D1000-20261025:A035:P00009","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00009"},
{"pair_id":"D1000-20261025:A035:P00010","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A035\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A035\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR03, profile_id=ru-en, split=train. metadata.learning_objective_id=TR03-EVENT-ORDER; metadata.scenario_group=TR03-EVENT-ORDER; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR03-EVENT-ORDER.ru-en.REF01",
    "profile_id": "ru-en",
    "family_id": "v3-ref.translation.event.window-call",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.\nИсходный текст: Майя открыла окно до того, как позвонила Дану.",
    "chosen": "Maya opened the window before she called Dan.",
    "rejected": "Maya opened the window after she called Dan.",
    "error_tag": "reversed_temporal_relation",
    "rationale": "В источнике раньше происходит open_window, затем call_Dan. Rejected меняет только временную связь и обращает эту последовательность.",
    "simple_check": "В источнике раньше происходит open_window, затем call_Dan. Rejected меняет только временную связь и обращает эту последовательность.",
    "counterexample": "Порядок частей предложения не равен порядку событий. «After A, B» и «B after A» могут выражать один порядок A→B; такой перефразированный перевод нельзя назначать rejected.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Майя открыла окно до того, как позвонила Дану.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR03-EVENT-ORDER.ru-en.REF02",
    "profile_id": "ru-en",
    "family_id": "v3-ref.translation.event.lock-light",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.\nИсходный текст: После того как Амир запер дверь, он выключил свет.",
    "chosen": "After Amir locked the door, he switched off the light.",
    "rejected": "Before Amir locked the door, he switched off the light.",
    "error_tag": "reversed_temporal_relation",
    "rationale": "В источнике раньше происходит lock_door, затем switch_off_light. Rejected меняет только временную связь и обращает эту последовательность.",
    "simple_check": "В источнике раньше происходит lock_door, затем switch_off_light. Rejected меняет только временную связь и обращает эту последовательность.",
    "counterexample": "Порядок частей предложения не равен порядку событий. «After A, B» и «B after A» могут выражать один порядок A→B; такой перефразированный перевод нельзя назначать rejected.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "После того как Амир запер дверь, он выключил свет.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR03-EVENT-ORDER.ru-en.backup-before-delete",
    "domain_id": "tr.en",
    "profile_id": "ru-en",
    "objective_id": "TR03-EVENT-ORDER",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nИсходный текст: После завершения резервного копирования она удалила копию.",
    "chosen": "After the backup finished, she deleted the copy.",
    "rejected": "Before the backup finished, she deleted the copy.",
    "error_tag": "backup-before-delete",
    "rationale": "The allowed order is backup completion < deletion. Rejected changes after to before; preserving both event names does not preserve their order.",
    "simple_check": "The allowed order is backup completion < deletion. Rejected changes after to before; preserving both event names does not preserve their order.",
    "wrong_authoring_trap": {
      "bad_authoring": "Treating two sentences containing the same events as equivalent despite reversed time order.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "После завершения резервного копирования она удалила копию.",
      "source_raw": "После завершения резервного копирования она удалила копию.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.backup-before-delete",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.backup-before-delete"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A035\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A035\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A035\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A035\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
