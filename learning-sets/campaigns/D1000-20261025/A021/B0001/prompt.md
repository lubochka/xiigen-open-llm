# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR06-PLACEHOLDER/ru-en; domain tr.en; profile ru-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 530; after it, domain remainder: 460. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Переводить обычный текст, сохраняя фигурные скобки, регистр и полное имя каждого заполнителя.",
  "instructions": "Составляй короткие сообщения интерфейса с 1–3 заполнителями в фигурных скобках. Эти имена — точный контракт симуляции, а не текст для перевода. Chosen переводит окружающую фразу и сохраняет список заполнителей побуквенно; rejected меняет ровно одно имя заполнителя либо его регистр, сохраняя остальной текст и остальные заполнители. Запиши protected_tokens_expected и changed_token в metadata. Количество заполнителей и наличие имени проверяются по raw-тексту; не исправляй данные автоматически при проверке. Не вводи одновременно неправильное число, валюту, порядок аргументов, время или смысл фразы. В каждой партии разнообразь позиции и длину имён. Если имя повторяется в источнике, требуется та же кратность, когда контракт перевода это предполагает. Для HE храни логический Unicode-порядок; не переворачивай буквы вручную ради отображения RTL. Статическая проверка имён — допуск данных, а учебное улучшение существует только после фактического обучения переводчика на этих парах.",
  "answer_contract": "Только перевод исходного текста на целевой язык; метаданные и объяснение в ответ не входят.",
  "hard_constraints": [
    "FORBIDDEN: Изменение чисел, единиц, смыслового глагола или направления перевода вместе с placeholder.",
    "FORBIDDEN: Автоматическая починка выбранной стороны во время оценки student, скрывающая ошибку модели.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "placeholder_count",
      "values": [
        1,
        2,
        3
      ]
    },
    {
      "axis": "mutation",
      "values": [
        "rename_one",
        "case_change_one"
      ]
    },
    {
      "axis": "position",
      "values": [
        "start",
        "middle",
        "end"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A021:P00001","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00001"},
{"pair_id":"D1000-20261025:A021:P00002","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00002"},
{"pair_id":"D1000-20261025:A021:P00003","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00003"},
{"pair_id":"D1000-20261025:A021:P00004","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00004"},
{"pair_id":"D1000-20261025:A021:P00005","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00005"},
{"pair_id":"D1000-20261025:A021:P00006","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00006"},
{"pair_id":"D1000-20261025:A021:P00007","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00007"},
{"pair_id":"D1000-20261025:A021:P00008","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00008"},
{"pair_id":"D1000-20261025:A021:P00009","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00009"},
{"pair_id":"D1000-20261025:A021:P00010","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A021\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A021\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR06, profile_id=ru-en, split=train. metadata.learning_objective_id=TR06-PLACEHOLDER; metadata.scenario_group=TR06-PLACEHOLDER; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR06-PLACEHOLDER.ru-en.REF01",
    "profile_id": "ru-en",
    "family_id": "v3-ref.translation.placeholder-code-email",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Симуляция сообщения группе пользователей. Сохрани имена в фигурных скобках без изменений; их написание — точный контракт шаблона.\nИсходный текст: Отправьте код {code} на адрес {email}.",
    "chosen": "Send code {code} to address {email}.",
    "rejected": "Send code {code} to address {mail}.",
    "error_tag": "tr06_placeholder",
    "rationale": "Имя {email} является ключом подстановки. Замена на {mail} нарушает объявленный контракт, хотя фраза остаётся понятной человеку.",
    "simple_check": "Имя {email} является ключом подстановки. Замена на {mail} нарушает объявленный контракт, хотя фраза остаётся понятной человеку.",
    "counterexample": "В обычном тексте слова mail/email могут быть допустимыми вариантами; здесь ошибку делает именно изменение точного имени в скобках.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Отправьте код {code} на адрес {email}.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR06-PLACEHOLDER.ru-en.REF02",
    "profile_id": "ru-en",
    "family_id": "v3-ref.translation.placeholder-file-rows",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Симуляция. Имена заполнителей чувствительны к регистру и не имеют алиасов; не меняй символы внутри фигурных скобок.\nИсходный текст: Файл {fileName} содержит {rowCount} строк.",
    "chosen": "File {fileName} contains {rowCount} rows.",
    "rejected": "File {fileName} contains {rowsCount} rows.",
    "error_tag": "tr06_placeholder",
    "rationale": "Лишняя s в {rowsCount} создаёт другое имя параметра; изменение нельзя оправдать более естественным английским названием.",
    "simple_check": "Лишняя s в {rowsCount} создаёт другое имя параметра; изменение нельзя оправдать более естественным английским названием.",
    "counterexample": "Если контракт приложения заранее допускает алиас rowsCount, этот контраст перестаёт доказывать ошибку. В симуляции алиасы запрещены явно.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Файл {fileName} содержит {rowCount} строк.",
      "response_contract": "translation_only"
    }
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A021\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A021\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A021\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A021\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
