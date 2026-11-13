# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR03-COMPLETION/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 2/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 430; after it, domain remainder: 560. Finishing this batch does not by itself finish the domain.

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
{"pair_id":"D1000-20261025:A016:P00011","family_id":"D1000-20261025:TR03-COMPLETION:F00011"},
{"pair_id":"D1000-20261025:A016:P00012","family_id":"D1000-20261025:TR03-COMPLETION:F00012"},
{"pair_id":"D1000-20261025:A016:P00013","family_id":"D1000-20261025:TR03-COMPLETION:F00013"},
{"pair_id":"D1000-20261025:A016:P00014","family_id":"D1000-20261025:TR03-COMPLETION:F00014"},
{"pair_id":"D1000-20261025:A016:P00015","family_id":"D1000-20261025:TR03-COMPLETION:F00015"},
{"pair_id":"D1000-20261025:A016:P00016","family_id":"D1000-20261025:TR03-COMPLETION:F00016"},
{"pair_id":"D1000-20261025:A016:P00017","family_id":"D1000-20261025:TR03-COMPLETION:F00017"},
{"pair_id":"D1000-20261025:A016:P00018","family_id":"D1000-20261025:TR03-COMPLETION:F00018"},
{"pair_id":"D1000-20261025:A016:P00019","family_id":"D1000-20261025:TR03-COMPLETION:F00019"},
{"pair_id":"D1000-20261025:A016:P00020","family_id":"D1000-20261025:TR03-COMPLETION:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR03-COMPLETION:F00011": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00011",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00011",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Монтажники уже собрали лестницу к полудню: завершение достигнуто к явной временной точке.",
    "semantic_facts": [
      "Участники: монтажники",
      "Действие: сборка лестницы",
      "Временная точка: полдень",
      "Статус завершения: завершено к указанному моменту",
      "Русская форма: прошедшее совершенный вид «собрали»"
    ],
    "source_text_by_language": {
      "ru": "К полудню монтажники уже собрали лестницу.",
      "en": "By noon, the installers had already assembled the staircase.",
      "he": "עד הצהרים המתקינים כבר הרכיבו את גרם המדרגות."
    },
    "facts": {
      "participant": "монтажники",
      "action": "сборка лестницы",
      "owner": "монтажники-сборщики",
      "time": "к полудню",
      "number": "одна лестница"
    }
  },
  "D1000-20261025:TR03-COMPLETION:F00012": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00012",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00012",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Женщина уже перевела письмо к приходу клиента: завершение достигнуто к явной временной точке.",
    "semantic_facts": [
      "Участник: одна женщина",
      "Действие: перевод письма",
      "Временная точка: приход клиента",
      "Статус завершения: завершено к указанному моменту",
      "Русская форма: прошедшее совершенный вид «перевела»"
    ],
    "source_text_by_language": {
      "ru": "К приходу клиента она уже перевела письмо.",
      "en": "By the time the client arrived, she had already translated the letter.",
      "he": "כשהלקוח הגיע, היא כבר תרגמה את המכתב."
    },
    "facts": {
      "participant": "одна женщина и клиент",
      "action": "перевод письма",
      "owner": "женщина-переводчик",
      "time": "к приходу клиента",
      "number": "одно письмо"
    }
  },
  "D1000-20261025:TR03-COMPLETION:F00013": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00013",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00013",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Мужчина уже написал заявление к полудню: завершение достигнуто к явной временной точке.",
    "semantic_facts": [
      "Участник: один мужчина",
      "Действие: написание заявления",
      "Временная точка: полдень",
      "Статус завершения: завершено к указанному моменту",
      "Русская форма: прошедшее совершенный вид «написал»"
    ],
    "source_text_by_language": {
      "ru": "К полудню он уже написал заявление.",
      "en": "By noon, he had already written the application.",
      "he": "עד הצהרים הוא כבר כתב את הבקשה."
    },
    "facts": {
      "participant": "один мужчина",
      "action": "написание заявления",
      "owner": "мужчина-писатель",
      "time": "к полудню",
      "number": "одно заявление"
    }
  },
  "D1000-20261025:TR03-COMPLETION:F00014": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00014",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00014",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Врач уже прочитал карту пациента до обхода: завершение достигнуто к явной временной точке.",
    "semantic_facts": [
      "Участник: один врач",
      "Действие: чтение карты пациента",
      "Временная точка: до обхода",
      "Статус завершения: завершено к указанному моменту",
      "Русская форма: прошедшее совершенный вид «прочитал»"
    ],
    "source_text_by_language": {
      "ru": "До обхода врач уже прочитал карту пациента.",
      "en": "Before the rounds, the doctor had already read the patient's chart.",
      "he": "לפני הסיור הרופא כבר קרא את תיק המטופל."
    },
    "facts": {
      "participant": "один врач",
      "action": "чтение карты пациента",
      "owner": "врач-читатель",
      "time": "до обхода",
      "number": "одна карта"
    }
  },
  "D1000-20261025:TR03-COMPLETION:F00015": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00015",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00015",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Рабочие уже собрали каркас до осмотра: завершение достигнуто к явной временной точке.",
    "semantic_facts": [
      "Участники: рабочие",
      "Действие: сборка каркаса",
      "Временная точка: до осмотра",
      "Статус завершения: завершено к указанному моменту",
      "Русская форма: прошедшее совершенный вид «собрали»"
    ],
    "source_text_by_language": {
      "ru": "До осмотра рабочие уже собрали каркас.",
      "en": "Before the inspection, the workers had already assembled the frame.",
      "he": "לפני הבדיקה הפועלים כבר הרכיבו את השלד."
    },
    "facts": {
      "participant": "рабочие",
      "action": "сборка каркаса",
      "owner": "рабочие-сборщики",
      "time": "до осмотра",
      "number": "один каркас"
    }
  },
  "D1000-20261025:TR03-COMPLETION:F00016": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00016",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00016",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Женщина ещё писала объявление в полдень и не закончила: незавершённость прямо утверждена.",
    "semantic_facts": [
      "Участник: одна женщина",
      "Действие: написание объявления",
      "Временная точка: полдень",
      "Статус завершения: не завершено, действие продолжалось",
      "Русская форма: прошедшее несовершенный вид «писала» с отрицанием завершения"
    ],
    "source_text_by_language": {
      "ru": "В полдень она ещё писала объявление и не закончила.",
      "en": "At noon, she was still writing the announcement and had not finished.",
      "he": "בצהרים היא עדיין כתבה את המודעה ולא סיימה."
    },
    "facts": {
      "participant": "одна женщина",
      "action": "написание объявления",
      "owner": "женщина-писатель",
      "time": "в полдень",
      "number": "одно объявление"
    }
  },
  "D1000-20261025:TR03-COMPLETION:F00017": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00017",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00017",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Переводчик ещё переводил решение и не закончил, когда вошёл судья: незавершённость прямо утверждена.",
    "semantic_facts": [
      "Участник: один переводчик",
      "Действие: перевод решения",
      "Временная точка: вход судьи",
      "Статус завершения: не завершено, действие продолжалось",
      "Русская форма: прошедшее несовершенный вид «переводил» с отрицанием завершения"
    ],
    "source_text_by_language": {
      "ru": "Когда вошёл судья, переводчик ещё переводил решение и не закончил.",
      "en": "When the judge entered, the translator was still translating the decision and had not finished.",
      "he": "כשהשופט נכנס, המתרגם עדיין תרגם את ההחלטה ולא סיים."
    },
    "facts": {
      "participant": "один переводчик и судья",
      "action": "перевод решения",
      "owner": "переводчик-исполнитель",
      "time": "ко входу судьи",
      "number": "одно решение"
    }
  },
  "D1000-20261025:TR03-COMPLETION:F00018": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00018",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00018",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Мужчина ещё читал замечания до созвона и не закончил: незавершённость прямо утверждена.",
    "semantic_facts": [
      "Участник: один мужчина",
      "Действие: чтение замечаний",
      "Временная точка: до созвона",
      "Статус завершения: не завершено, действие продолжалось",
      "Русская форма: прошедшее несовершенный вид «читал» с отрицанием завершения"
    ],
    "source_text_by_language": {
      "ru": "До созвона он ещё читал замечания и не закончил.",
      "en": "Before the call, he was still reading the comments and had not finished.",
      "he": "לפני השיחה הוא עדיין קרא את ההערות ולא סיים."
    },
    "facts": {
      "participant": "один мужчина",
      "action": "чтение замечаний",
      "owner": "мужчина-читатель",
      "time": "до созвона",
      "number": "несколько замечаний"
    }
  },
  "D1000-20261025:TR03-COMPLETION:F00019": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00019",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00019",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Рабочие ещё собирали станок и не закончили, когда подошла смена: незавершённость прямо утверждена.",
    "semantic_facts": [
      "Участники: рабочие",
      "Действие: сборка станка",
      "Временная точка: приход смены",
      "Статус завершения: не завершено, действие продолжалось",
      "Русская форма: прошедшее несовершенный вид «собирали» с отрицанием завершения"
    ],
    "source_text_by_language": {
      "ru": "Когда подошла смена, они ещё собирали станок и не закончили.",
      "en": "When the shift arrived, they were still assembling the machine and had not finished.",
      "he": "כשהמשמרת הגיעה, הם עדיין הרכיבו את המכונה ולא סיימו."
    },
    "facts": {
      "participant": "рабочие и новая смена",
      "action": "сборка станка",
      "owner": "рабочие-сборщики",
      "time": "к приходу смены",
      "number": "один станок"
    }
  },
  "D1000-20261025:TR03-COMPLETION:F00020": {
    "family_id": "D1000-20261025:TR03-COMPLETION:F00020",
    "origin_family_id": "D1000-20261025:TR03-COMPLETION:F00020",
    "scenario_group": "TR03-COMPLETION",
    "scenario": "Комиссия ещё писала заключение до заседания и не закончила: незавершённость прямо утверждена.",
    "semantic_facts": [
      "Участники: комиссия",
      "Действие: написание заключения",
      "Временная точка: до заседания",
      "Статус завершения: не завершено, действие продолжалось",
      "Русская форма: прошедшее несовершенный вид «писала» с отрицанием завершения"
    ],
    "source_text_by_language": {
      "ru": "До заседания комиссия ещё писала заключение и не закончила.",
      "en": "Before the session, the commission was still writing the conclusion and had not finished.",
      "he": "לפני הישיבה הוועדה עדיין כתבה את חוות הדעת ולא סיימה."
    },
    "facts": {
      "participant": "комиссия",
      "action": "написание заключения",
      "owner": "комиссия-писатель",
      "time": "до заседания",
      "number": "одно заключение"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A016\B0002\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A016\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR03, profile_id=he-en, split=train. metadata.learning_objective_id=TR03-COMPLETION; metadata.scenario_group=TR03-COMPLETION; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR03-COMPLETION.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "TR03-reference-family-05",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. \nИсходный текст: עד הצהריים היא כבר סיימה לכתוב את הדוח.",
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
        "source": "he",
        "target": "en"
      },
      "source_raw": "עד הצהריים היא כבר סיימה לכתוב את הדוח.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR03-COMPLETION.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "TR03-reference-family-03",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. \nИсходный текст: בצהריים הוא עדיין קרא את המכתב ולא סיים.",
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
        "source": "he",
        "target": "en"
      },
      "source_raw": "בצהריים הוא עדיין קרא את המכתב ולא סיים.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR03-COMPLETION.he-en.already-completed-lock",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR03-COMPLETION",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nИсходный текст: היא כבר תיקנה את המנעול.",
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
      "canonical_source": "היא כבר תיקנה את המנעול.",
      "source_raw": "היא כבר תיקנה את המנעול.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.already-completed-lock",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.already-completed-lock"
      ],
      "source_language": "he",
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A016\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A016\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A016\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A016\B0002\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
