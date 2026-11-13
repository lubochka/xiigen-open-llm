# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR04-EXPLICIT-GENDER/ru-en; domain tr.en; profile ru-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 460; after it, domain remainder: 530. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Сохранять явно выраженный род участника и согласование, не угадывая его по профессии или имени.",
  "instructions": "Создай 10 новых коротких ситуаций с одним участником: 5 с явно женским и 5 с явно мужским референтом. Источник должен содержать местоимение либо однозначное согласование, а при необходимости — видимый контекст о том, к кому относится местоимение. Chosen сохраняет эту информацию; rejected заменяет её на другой род, оставляя число, время, событие и профессию прежними. Различай grammatical_gender и явно сообщённую информацию о референте: грамматический род неодушевлённого существительного не означает пол человека. Не делай вывод «программист — мужчина» или «медсестра — женщина» из стереотипа. Для английского source без заданного рода не изобретай женщину/мужчину: такой случай относится к карточке условного уточнения либо требует нейтрального перевода, если он естественно возможен. Не включай одновременное изменение singular/plural: число — отдельная микроцель. Сохраняй лемму, словоформу, род, число и роль в companion metadata; не сливай разные словоформы в один текст только потому, что у них одна лексическая семья.",
  "answer_contract": "Только перевод исходного текста на целевой язык; метаданные и объяснение в ответ не входят.",
  "hard_constraints": [
    "FORBIDDEN: Определение пола по профессии, имени, статистике корпуса или гендерному стереотипу.",
    "FORBIDDEN: Изменение числа, отрицания, действия или отношения времени вместе с родом.",
    "FORBIDDEN: Скрытая gender-аннотация как единственное доказательство ответа.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "explicit_reference",
      "values": [
        "female",
        "male"
      ]
    },
    {
      "axis": "role",
      "values": [
        "engineer",
        "client",
        "speaker",
        "student"
      ]
    },
    {
      "axis": "evidence",
      "values": [
        "pronoun",
        "overt_agreement",
        "visible_context"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A017:P00011","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00011"},
{"pair_id":"D1000-20261025:A017:P00012","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00012"},
{"pair_id":"D1000-20261025:A017:P00013","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00013"},
{"pair_id":"D1000-20261025:A017:P00014","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00014"},
{"pair_id":"D1000-20261025:A017:P00015","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00015"},
{"pair_id":"D1000-20261025:A017:P00016","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00016"},
{"pair_id":"D1000-20261025:A017:P00017","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00017"},
{"pair_id":"D1000-20261025:A017:P00018","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00018"},
{"pair_id":"D1000-20261025:A017:P00019","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00019"},
{"pair_id":"D1000-20261025:A017:P00020","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A017\B0002\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A017\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR04, profile_id=ru-en, split=train. metadata.learning_objective_id=TR04-EXPLICIT-GENDER; metadata.scenario_group=TR04-EXPLICIT-GENDER; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR04-EXPLICIT-GENDER.ru-en.REF01",
    "profile_id": "ru-en",
    "family_id": "v3-ref.translation.gender-engineer",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Контекст: в сообщении описывается состояние самого говорящего; местоимение в придаточной части относится к этому же человеку.\nИсходный текст: Инженер сказала, что она готова.",
    "chosen": "The engineer said that she was ready.",
    "rejected": "The engineer said that he was ready.",
    "error_tag": "tr04_explicit_gender",
    "rationale": "Женский референт задан местоимением и/или согласованием в самом тексте. Rejected меняет его, а не просто выбирает другой стиль.",
    "simple_check": "Женский референт задан местоимением и/или согласованием в самом тексте. Rejected меняет его, а не просто выбирает другой стиль.",
    "counterexample": "Исходник «The engineer is ready» не сообщает пол. Нельзя автоматически переносить на него женский chosen этого примера.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Инженер сказала, что она готова.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR04-EXPLICIT-GENDER.ru-en.REF02",
    "profile_id": "ru-en",
    "family_id": "v3-ref.translation.gender-client",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Контекст: в сообщении описывается состояние самого говорящего; местоимение в придаточной части относится к этому же человеку.\nИсходный текст: Клиент сказал, что он устал.",
    "chosen": "The client said that he was tired.",
    "rejected": "The client said that she was tired.",
    "error_tag": "tr04_explicit_gender",
    "rationale": "Мужской референт указан в исходнике; rejected меняет его при сохранении состояния усталости.",
    "simple_check": "Мужской референт указан в исходнике; rejected меняет его при сохранении состояния усталости.",
    "counterexample": "Грамматически мужское название профессии без дополнительных свидетельств не всегда задаёт пол референта. Здесь доказательство — полный текст, включая местоимение.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "ru",
        "target": "en"
      },
      "source_raw": "Клиент сказал, что он устал.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR04-EXPLICIT-GENDER.ru-en.female-ready",
    "domain_id": "tr.en",
    "profile_id": "ru-en",
    "objective_id": "TR04-EXPLICIT-GENDER",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nИсходный текст: Она готова.",
    "chosen": "She is ready.",
    "rejected": "He is ready.",
    "error_tag": "female-ready",
    "rationale": "The subject is explicitly feminine. In Hebrew היא and מוכנה agree; in Russian она and готова agree. Rejected changes the referent's gender.",
    "simple_check": "The subject is explicitly feminine. In Hebrew היא and מוכנה agree; in Russian она and готова agree. Rejected changes the referent's gender.",
    "wrong_authoring_trap": {
      "bad_authoring": "Inferring a person's gender from a job stereotype when the source leaves it unspecified.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "Она готова.",
      "source_raw": "Она готова.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.female-ready",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.female-ready"
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
  },
  {
    "example_id": "TQ26.TR04-EXPLICIT-GENDER.ru-en.her-umbrella-owner",
    "domain_id": "tr.en",
    "profile_id": "ru-en",
    "objective_id": "TR04-EXPLICIT-GENDER",
    "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Требуется сохранить владельца предмета; место и сам предмет не меняются.\nИсходный текст: Её зонт здесь.",
    "chosen": "Her umbrella is here.",
    "rejected": "His umbrella is here.",
    "error_tag": "her-umbrella-owner",
    "rationale": "Ask whose umbrella it is. שלה / её / her identifies a female owner, independent of the grammatical gender of umbrella. Rejected changes only ownership.",
    "simple_check": "Ask whose umbrella it is. שלה / её / her identifies a female owner, independent of the grammatical gender of umbrella. Rejected changes only ownership.",
    "wrong_authoring_trap": {
      "bad_authoring": "Choosing the possessor's gender from the grammatical gender of the possessed noun.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "Её зонт здесь.",
      "source_raw": "Её зонт здесь.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.her-umbrella-owner",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.her-umbrella-owner"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A017\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A017\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A017\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A017\B0002\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
