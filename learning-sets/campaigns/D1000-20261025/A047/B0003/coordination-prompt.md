# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR07-VALIDWORD/en-ru; domain tr.ru; profile en-ru.

Write exactly 10 rows, batch 3/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 140; after it, domain remainder: 850. Finishing this batch does not by itself finish the domain.

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
  "authoring_checks_en": "Create preferences for translation into Russian. Choose one semantic defect in rejected; a valid synonym is not a defect. Preserve correct source words unless the visible task explicitly declares a recoverable typo."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A047:P00021","family_id":"D1000-20261025:TR07-VALIDWORD:F00021"},
{"pair_id":"D1000-20261025:A047:P00022","family_id":"D1000-20261025:TR07-VALIDWORD:F00022"},
{"pair_id":"D1000-20261025:A047:P00023","family_id":"D1000-20261025:TR07-VALIDWORD:F00023"},
{"pair_id":"D1000-20261025:A047:P00024","family_id":"D1000-20261025:TR07-VALIDWORD:F00024"},
{"pair_id":"D1000-20261025:A047:P00025","family_id":"D1000-20261025:TR07-VALIDWORD:F00025"},
{"pair_id":"D1000-20261025:A047:P00026","family_id":"D1000-20261025:TR07-VALIDWORD:F00026"},
{"pair_id":"D1000-20261025:A047:P00027","family_id":"D1000-20261025:TR07-VALIDWORD:F00027"},
{"pair_id":"D1000-20261025:A047:P00028","family_id":"D1000-20261025:TR07-VALIDWORD:F00028"},
{"pair_id":"D1000-20261025:A047:P00029","family_id":"D1000-20261025:TR07-VALIDWORD:F00029"},
{"pair_id":"D1000-20261025:A047:P00030","family_id":"D1000-20261025:TR07-VALIDWORD:F00030"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent kiro-k3-master-20261028 in pool all. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A047\B0003\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A047\B0003\request.json' '--agent-id' 'kiro-k3-master-20261028' '--claim-token' 'c8869366920c48d7a454b85353a094f5'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR07-VALIDWORD:F00021": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00021",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00021",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Стена покрашена в белый цвет.",
    "semantic_facts": [
      "Стена покрашена",
      "Цвет: белый",
      "Стена — часть комнаты",
      "Сено в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The wall is painted white.",
      "ru": "Стена покрашена в белый цвет.",
      "he": "הקיר צבוע בלבן."
    },
    "participant_facts": [
      "Стена"
    ],
    "action_facts": [
      "Покрашена в белый"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна стена"
    ]
  },
  "D1000-20261025:TR07-VALIDWORD:F00022": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00022",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00022",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Сын играет во дворе.",
    "semantic_facts": [
      "Играет сын",
      "Место: двор",
      "Сын — ребёнок в семье",
      "Сон в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The son plays in the yard.",
      "ru": "Сын играет во дворе.",
      "he": "הבן משחק בחצר."
    },
    "participant_facts": [
      "Сын",
      "Двор"
    ],
    "action_facts": [
      "Играет"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один сын",
      "Один двор"
    ]
  },
  "D1000-20261025:TR07-VALIDWORD:F00023": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00023",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00023",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Утро началось с тумана.",
    "semantic_facts": [
      "Утро началось",
      "Началось с тумана",
      "Утро — ранняя часть дня",
      "Утка в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The morning started with fog.",
      "ru": "Утро началось с тумана.",
      "he": "הבוקר התחיל בערפל."
    },
    "participant_facts": [
      "Утро",
      "Туман"
    ],
    "action_facts": [
      "Началось"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Утро"
    ],
    "number_facts": [
      "Одно утро"
    ]
  },
  "D1000-20261025:TR07-VALIDWORD:F00024": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00024",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00024",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Зонт стоит у двери.",
    "semantic_facts": [
      "У двери зонт",
      "Зонт стоит",
      "Зонт — защита от дождя",
      "Зонд в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The umbrella is by the door.",
      "ru": "Зонт стоит у двери.",
      "he": "המטריה עומדת ליד הדלת."
    },
    "participant_facts": [
      "Зонт",
      "Дверь"
    ],
    "action_facts": [
      "Стоит"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один зонт",
      "Одна дверь"
    ]
  },
  "D1000-20261025:TR07-VALIDWORD:F00025": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00025",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00025",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Чашка стоит на блюдце.",
    "semantic_facts": [
      "На блюдце чашка",
      "Чашка стоит",
      "Чашка — посуда для питья",
      "Чайка в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The cup is on the saucer.",
      "ru": "Чашка стоит на блюдце.",
      "he": "הספל על הצלחת."
    },
    "participant_facts": [
      "Чашка",
      "Блюдце"
    ],
    "action_facts": [
      "Стоит на"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна чашка",
      "Одно блюдце"
    ]
  },
  "D1000-20261025:TR07-VALIDWORD:F00026": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00026",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00026",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Врач слушает сердце.",
    "semantic_facts": [
      "Слушает врач",
      "Объект: сердце",
      "Врач — доктор",
      "Враг в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The doctor listens to the heart.",
      "ru": "Врач слушает сердце.",
      "he": "הרופא מקשיב ללב."
    },
    "participant_facts": [
      "Врач",
      "Сердце"
    ],
    "action_facts": [
      "Слушает"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один врач",
      "Одно сердце"
    ]
  },
  "D1000-20261025:TR07-VALIDWORD:F00027": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00027",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00027",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Ключ лежит на тумбочке.",
    "semantic_facts": [
      "На тумбочке ключ",
      "Ключ лежит",
      "Ключ — предмет для замка",
      "Клуб в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The key is on the nightstand.",
      "ru": "Ключ лежит на тумбочке.",
      "he": "המפתח על שידת הלילה."
    },
    "participant_facts": [
      "Ключ",
      "Тумбочка"
    ],
    "action_facts": [
      "Лежит на"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один ключ",
      "Одна тумбочка"
    ]
  },
  "D1000-20261025:TR07-VALIDWORD:F00028": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00028",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00028",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Том стоит на полке библиотеки.",
    "semantic_facts": [
      "На полке том",
      "Том стоит",
      "Том — большая книга",
      "Дом в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The volume is on the shelf.",
      "ru": "Том стоит на полке.",
      "he": "הכרך על המדף."
    },
    "participant_facts": [
      "Том",
      "Полка"
    ],
    "action_facts": [
      "Стоит на"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один том",
      "Одна полка"
    ]
  },
  "D1000-20261025:TR07-VALIDWORD:F00029": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00029",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00029",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Сок налит в кувшин.",
    "semantic_facts": [
      "В кувшине сок",
      "Сок налит",
      "Сок — фруктовый напиток",
      "Сук в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The juice is poured into the jug.",
      "ru": "Сок налит в кувшин.",
      "he": "המיץ נמזג לכד."
    },
    "participant_facts": [
      "Сок",
      "Кувшин"
    ],
    "action_facts": [
      "Налит в"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один кувшин"
    ]
  },
  "D1000-20261025:TR07-VALIDWORD:F00030": {
    "family_id": "D1000-20261025:TR07-VALIDWORD:F00030",
    "origin_family_id": "D1000-20261025:TR07-VALIDWORD:F00030",
    "scenario_group": "TR07-VALIDWORD",
    "scenario": "Мир большой и разный.",
    "semantic_facts": [
      "Мир большой",
      "Мир разный",
      "Мир — весь земной шар",
      "Миф в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The world is big and diverse.",
      "ru": "Мир большой и разный.",
      "he": "העולם גדול ומגוון."
    },
    "participant_facts": [
      "Мир"
    ],
    "action_facts": [
      "Большой",
      "Разный"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один мир"
    ]
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A047\B0003\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A047\B0003\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.ru, topic_id=TR07, profile_id=en-ru, split=train. metadata.learning_objective_id=TR07-VALIDWORD; metadata.scenario_group=TR07-VALIDWORD; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-024", "RAG-023"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR07-VALIDWORD.en-ru.REF01",
    "profile_id": "en-ru",
    "family_id": "v2-ref.translation.described-desert",
    "prompt": "Переведи с языка «английский» на язык «русский». Выдай только перевод. Речь о географическом ландшафте; опечатка не заявлена.\nИсходный текст: She described the desert.",
    "chosen": "Она описала пустыню.",
    "rejected": "Она описала десерт.",
    "error_tag": "false_typo_correction",
    "rationale": "Raw содержит корректное самостоятельное слово; rejected без основания заменяет его другим похожим словом.",
    "simple_check": "Raw содержит корректное самостоятельное слово; rejected без основания заменяет его другим похожим словом.",
    "counterexample": "Если raw действительно возник из документированной опечатки, это другой сценарий: нужны provenance и видимый разрешающий контекст, а не универсальное do-not-correct правило.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru",
      "language_pair": {
        "source": "en",
        "target": "ru"
      },
      "source_raw": "She described the desert.",
      "canonical_source": "She described the desert.",
      "lexical_family_ids": [
        "lex:en:desert"
      ],
      "sense_id": "arid_landscape",
      "response_contract": "translation_only",
      "do_not_merge": [
        "lex:en:dessert"
      ]
    }
  },
  {
    "example_id": "TR07-VALIDWORD.en-ru.REF02",
    "profile_id": "en-ru",
    "family_id": "reference.translation.boy-drew-cat.v1",
    "prompt": "Переведи с языка «английский» на язык «русский». Выдай только перевод исходного текста, без перевода контекста и без объяснений. Контекст: Речь о рисунке мальчика. Исходное предложение записано правильно, опечатки не заявлены. Сохрани смысл правильного исходного текста. Не исправляй его в другое похожее слово.\nИсходный текст: The boy drew a cat.",
    "chosen": "Мальчик нарисовал кота.",
    "rejected": "Мальчик нарисовал кепку.",
    "error_tag": "valid_word_overcorrected",
    "rationale": "Источник уже содержит правильное слово cat; требование восстановить опечатку отсутствует. Rejected переводит другую похожую лексему и меняет нарисованный предмет.",
    "simple_check": "The raw token is exactly the correct token 'cat'; no learner-input mutation is assigned. 'cap' is a different valid word, so translate 'cat' and do not merge their lexical families.",
    "counterexample": "cat и cap — разные правильные слова. Близость написания не разрешает менять cat на cap; не добавляй строку с заявленной опечаткой в этот preserve-valid-word batch.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru",
      "language_pair": {
        "source": "en",
        "target": "ru"
      },
      "source_raw": "The boy drew a cat.",
      "canonical_source": "The boy drew a cat.",
      "canonical_token": "cat",
      "raw_token": "cat",
      "mutation": {
        "kind": "none_valid_word"
      },
      "lexical_family_ids": [
        "lex:en:cat"
      ],
      "sense_id": "domestic_cat",
      "response_contract": "translation_only",
      "do_not_merge": [
        "lex:en:cap"
      ]
    }
  },
  {
    "example_id": "TQ26.TR07-VALIDWORD.en-ru.goat-valid-word",
    "domain_id": "tr.ru",
    "profile_id": "en-ru",
    "objective_id": "TR07-VALIDWORD",
    "prompt": "Переведи с языка «английский» на язык «русский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Говорится об одной самке животного. Исходная фраза записана правильно; исправление опечаток здесь не назначено. Переведи её как написано.\nИсходный текст: The goat is near the gate.",
    "chosen": "Коза возле ворот.",
    "rejected": "Дерево возле ворот.",
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
      "canonical_source": "The goat is near the gate.",
      "source_raw": "The goat is near the gate.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.goat-valid-word",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.goat-valid-word"
      ],
      "source_language": "en",
      "response_language": "ru",
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A047\B0003\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A047\B0003\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A047\B0003\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A047\B0003\family-material.jsonl' '--model' 'unknown' '--agent-id' 'kiro-k3-master-20261028' '--claim-token' 'c8869366920c48d7a454b85353a094f5'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'kiro-k3-master-20261028' '--pool' 'all'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
