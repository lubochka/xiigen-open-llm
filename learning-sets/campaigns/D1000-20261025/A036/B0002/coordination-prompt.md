# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR03-EVENT-ORDER/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 840; after it, domain remainder: 150. Finishing this batch does not by itself finish the domain.

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
{"pair_id":"D1000-20261025:A036:P00011","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00011"},
{"pair_id":"D1000-20261025:A036:P00012","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00012"},
{"pair_id":"D1000-20261025:A036:P00013","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00013"},
{"pair_id":"D1000-20261025:A036:P00014","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00014"},
{"pair_id":"D1000-20261025:A036:P00015","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00015"},
{"pair_id":"D1000-20261025:A036:P00016","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00016"},
{"pair_id":"D1000-20261025:A036:P00017","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00017"},
{"pair_id":"D1000-20261025:A036:P00018","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00018"},
{"pair_id":"D1000-20261025:A036:P00019","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00019"},
{"pair_id":"D1000-20261025:A036:P00020","family_id":"D1000-20261025:TR03-EVENT-ORDER:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A036\B0002\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A036\B0002\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'e3bcb36251b74f7c82c821394b03b0ba'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR03-EVENT-ORDER:F00011": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00011",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00011",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Она сначала закрыла форточку, потом легла спать: раньше закрытие форточки, позже отход ко сну.",
    "semantic_facts": [
      "Участник: одна женщина",
      "Событие A: закрыла форточку",
      "Событие B: легла спать",
      "Порядок фактов: A раньше B, связка «до того как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "До того как лечь спать, она закрыла форточку.",
      "en": "Before going to sleep, she closed the vent window.",
      "he": "לפני השכיבה, היא סגרה את החלונק."
    },
    "facts": {
      "participant": "одна женщина",
      "action": "закрытие форточки и отход ко сну",
      "owner": "женщина",
      "time": "прошедшее",
      "number": "одна форточка"
    }
  },
  "D1000-20261025:TR03-EVENT-ORDER:F00012": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00012",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00012",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Столяр сначала отшлифовал доску, потом заказчик пришёл за ней: раньше шлифовка, позже приход заказчика.",
    "semantic_facts": [
      "Участники: столяр и заказчик",
      "Событие A: столяр отшлифовал доску",
      "Событие B: заказчик пришёл за доской",
      "Порядок фактов: A раньше B, связка «до того, как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "Столяр отшлифовал доску до того, как заказчик пришёл за ней.",
      "en": "The carpenter sanded the board before the customer came for it.",
      "he": "הנגר שייף את הקרש לפני שהלקוח בא לקחת אותו."
    },
    "facts": {
      "participant": "столяр и заказчик",
      "action": "шлифовка доски и приход",
      "owner": "столяр",
      "time": "прошедшее",
      "number": "одна доска, один заказчик"
    }
  },
  "D1000-20261025:TR03-EVENT-ORDER:F00013": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00013",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00013",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Музыканты сначала настроили инструменты, потом зрители заняли места: раньше настройка, позже рассадка зрителей.",
    "semantic_facts": [
      "Участники: музыканты и зрители",
      "Событие A: музыканты настроили инструменты",
      "Событие B: зрители заняли места",
      "Порядок фактов: A раньше B, связка «до того как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "До того как зрители заняли места, музыканты настроили инструменты.",
      "en": "Before the audience took their seats, the musicians tuned their instruments.",
      "he": "לפני שהצופים התיישבו, המוזיקאים כיוונו את הכלים."
    },
    "facts": {
      "participant": "музыканты и зрители",
      "action": "настройка инструментов и рассадка",
      "owner": "музыканты",
      "time": "прошедшее",
      "number": "несколько инструментов, несколько зрителей"
    }
  },
  "D1000-20261025:TR03-EVENT-ORDER:F00014": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00014",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00014",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Он сначала вымыл руки, потом сел за стол: раньше мытьё рук, позже посадка за стол.",
    "semantic_facts": [
      "Участник: один мужчина",
      "Событие A: вымыл руки",
      "Событие B: сел за стол",
      "Порядок фактов: A раньше B, связка «до того, как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "Он вымыл руки до того, как сел за стол.",
      "en": "He washed his hands before he sat down at the table.",
      "he": "הוא רחץ את הידיים לפני שהתיישב לשולחן."
    },
    "facts": {
      "participant": "один мужчина",
      "action": "мытьё рук и посадка",
      "owner": "мужчина",
      "time": "прошедшее",
      "number": "один стол"
    }
  },
  "D1000-20261025:TR03-EVENT-ORDER:F00015": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00015",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00015",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Проводник сначала проверил билеты, потом поезд тронулся: раньше проверка, позже отправление поезда.",
    "semantic_facts": [
      "Участники: проводник и поезд",
      "Событие A: проводник проверил билеты",
      "Событие B: поезд тронулся",
      "Порядок фактов: A раньше B, связка «до того как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "До того как поезд тронулся, проводник проверил билеты.",
      "en": "Before the train started moving, the conductor checked the tickets.",
      "he": "לפני שהרכבת זזה, הכרטיסן בדק את הכרטיסים."
    },
    "facts": {
      "participant": "проводник и поезд",
      "action": "проверка билетов и отправление",
      "owner": "проводник",
      "time": "прошедшее",
      "number": "несколько билетов, один поезд"
    }
  },
  "D1000-20261025:TR03-EVENT-ORDER:F00016": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00016",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00016",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Покупатель сначала оплатил товар, потом кассир выдал чек: раньше оплата, позже выдача чека.",
    "semantic_facts": [
      "Участники: кассир и покупатель",
      "Событие A: покупатель оплатил товар",
      "Событие B: кассир выдал чек",
      "Порядок фактов: A раньше B, связка «после того, как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "Кассир выдал чек после того, как покупатель оплатил товар.",
      "en": "The cashier issued the receipt after the buyer paid for the goods.",
      "he": "הקופאית נתנה את הקבלה אחרי שהקונה שילם על הסחורה."
    },
    "facts": {
      "participant": "кассир и покупатель",
      "action": "оплата и выдача чека",
      "owner": "кассир",
      "time": "прошедшее",
      "number": "один товар, один чек"
    }
  },
  "D1000-20261025:TR03-EVENT-ORDER:F00017": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00017",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00017",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Она сначала прочитала письмо, потом спрятала его в ящик: раньше чтение, позже уборка письма.",
    "semantic_facts": [
      "Участник: одна женщина",
      "Событие A: прочитала письмо",
      "Событие B: спрятала письмо в ящик",
      "Порядок фактов: A раньше B, связка «после того как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "После того как она прочитала письмо, она спрятала его в ящик.",
      "en": "After she read the letter, she put it away in the drawer.",
      "he": "אחרי שהיא קראה את המכתב, היא שמה אותו במגרה."
    },
    "facts": {
      "participant": "одна женщина",
      "action": "чтение и уборка письма",
      "owner": "женщина",
      "time": "прошедшее",
      "number": "одно письмо, один ящик"
    }
  },
  "D1000-20261025:TR03-EVENT-ORDER:F00018": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00018",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00018",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Сначала метель стихла, потом бригада убрала снег: раньше стихание метели, позже уборка снега.",
    "semantic_facts": [
      "Участники: бригада",
      "Событие A: метель стихла",
      "Событие B: бригада убрала снег",
      "Порядок фактов: A раньше B, связка «после того, как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "Бригада убрала снег после того, как метель стихла.",
      "en": "The crew cleared the snow after the blizzard subsided.",
      "he": "הצוות פינה את השלג אחרי שהסופה שקטה."
    },
    "facts": {
      "participant": "одна бригада",
      "action": "стихание метели и уборка снега",
      "owner": "бригада",
      "time": "прошедшее",
      "number": "одна метель, одна уборка"
    }
  },
  "D1000-20261025:TR03-EVENT-ORDER:F00019": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00019",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00019",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Он сначала сдал экзамен, потом поехал домой на автобусе: раньше сдача экзамена, позже поездка домой.",
    "semantic_facts": [
      "Участник: один мужчина",
      "Событие A: сдал экзамен",
      "Событие B: поехал домой на автобусе",
      "Порядок фактов: A раньше B, связка «после того как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "После того как он сдал экзамен, он поехал домой на автобусе.",
      "en": "After he passed the exam, he went home by bus.",
      "he": "אחרי שהוא עבר את הבחינה, הוא נסע הביתה באוטובוס."
    },
    "facts": {
      "participant": "один мужчина",
      "action": "сдача экзамена и поездка",
      "owner": "мужчина",
      "time": "прошедшее",
      "number": "один экзамен, одна поездка"
    }
  },
  "D1000-20261025:TR03-EVENT-ORDER:F00020": {
    "family_id": "D1000-20261025:TR03-EVENT-ORDER:F00020",
    "origin_family_id": "D1000-20261025:TR03-EVENT-ORDER:F00020",
    "scenario_group": "TR03-EVENT-ORDER",
    "scenario": "Сначала мэр закончил ответ, потом журналисты ушли: раньше конец ответа мэра, позже уход журналистов.",
    "semantic_facts": [
      "Участники: мэр и журналисты",
      "Событие A: мэр закончил ответ",
      "Событие B: журналисты ушли",
      "Порядок фактов: A раньше B, связка «после того, как»",
      "Оба события завершены, причинный вывод не добавляется"
    ],
    "source_text_by_language": {
      "ru": "Журналисты ушли после того, как мэр закончил ответ.",
      "en": "The journalists left after the mayor finished the answer.",
      "he": "העיתונאים יצאו אחרי שהראש העיר סיים את התשובה."
    },
    "facts": {
      "participant": "мэр и журналисты",
      "action": "конец ответа и уход",
      "owner": "мэр",
      "time": "прошедшее",
      "number": "один ответ, несколько журналистов"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A036\B0002\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A036\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR03, profile_id=he-en, split=train. metadata.learning_objective_id=TR03-EVENT-ORDER; metadata.scenario_group=TR03-EVENT-ORDER; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR03-EVENT-ORDER.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.event.window-call",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.\nИсходный текст: מאיה פתחה את החלון לפני שהתקשרה לדן.",
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
        "source": "he",
        "target": "en"
      },
      "source_raw": "מאיה פתחה את החלון לפני שהתקשרה לדן.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR03-EVENT-ORDER.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.event.lock-light",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.\nИсходный текст: אחרי שאמיר נעל את הדלת, הוא כיבה את האור.",
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
        "source": "he",
        "target": "en"
      },
      "source_raw": "אחרי שאמיר נעל את הדלת, הוא כיבה את האור.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR03-EVENT-ORDER.he-en.backup-before-delete",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR03-EVENT-ORDER",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nИсходный текст: אחרי שהגיבוי הסתיים, היא מחקה את העותק.",
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
      "canonical_source": "אחרי שהגיבוי הסתיים, היא מחקה את העותק.",
      "source_raw": "אחרי שהגיבוי הסתיים, היא מחקה את העותק.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.backup-before-delete",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.backup-before-delete"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A036\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A036\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A036\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A036\B0002\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'e3bcb36251b74f7c82c821394b03b0ba'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
