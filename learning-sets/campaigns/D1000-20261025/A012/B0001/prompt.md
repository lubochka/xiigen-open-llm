# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR01-NEGATION-SCOPE/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 300; after it, domain remainder: 690. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Сохранять, какое утверждение отрицается: сообщение/обещание целиком либо вложенное действие.",
  "instructions": "Составь новые короткие фразы с ровно одним отрицанием и двумя смысловыми уровнями: «не сказал, что…» против «сказал, что не…», «не обещал сделать…» против «обещал не делать…». Chosen сохраняет исходную область отрицания; rejected переносит тот же оператор на другой предикат. Время, участники, предмет и направление перевода остаются прежними. Не делай rejected случайной противоположной фразой: отмечай в metadata negation_scope_expected и negation_scope_rejected. В learner prompt должен быть весь исходный текст, а не только слово «не». В партии 10 строк используй 5 отрицаний сообщения/обещания и 5 отрицаний вложенного действия, чтобы модель не выучила постоянное положение оператора. Это два класса одного навыка; новые классы не являются новыми доменами. Не включай идиомы с двойным отрицанием, модальные запреты, шутки или иронию в эту микроцель. Не обосновывай предпочтение большим расхождением токенов: изменение места одного отрицания достаточно, если оно меняет утверждение.",
  "answer_contract": "Только перевод исходного текста на целевой язык; метаданные и объяснение в ответ не входят.",
  "hard_constraints": [
    "FORBIDDEN: Вид глагола, численность участников, раскладка и опечатки как дополнительные дефекты.",
    "FORBIDDEN: Двойное отрицание, неоднозначные модальные конструкции и интонация без письменного контекста.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "predicate",
      "values": [
        "say",
        "promise"
      ]
    },
    {
      "axis": "scope",
      "values": [
        "matrix_predicate",
        "embedded_predicate"
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
{"pair_id":"D1000-20261025:A012:P00001","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00001"},
{"pair_id":"D1000-20261025:A012:P00002","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00002"},
{"pair_id":"D1000-20261025:A012:P00003","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00003"},
{"pair_id":"D1000-20261025:A012:P00004","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00004"},
{"pair_id":"D1000-20261025:A012:P00005","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00005"},
{"pair_id":"D1000-20261025:A012:P00006","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00006"},
{"pair_id":"D1000-20261025:A012:P00007","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00007"},
{"pair_id":"D1000-20261025:A012:P00008","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00008"},
{"pair_id":"D1000-20261025:A012:P00009","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00009"},
{"pair_id":"D1000-20261025:A012:P00010","family_id":"D1000-20261025:TR01-NEGATION-SCOPE:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR01-NEGATION-SCOPE:F00001": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00001",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00001",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Мужчина не сообщил об отмене встречи: отрицание относится к глаголу сообщения, а не к отмене.",
    "semantic_facts": [
      "Участник: один мужчина",
      "Действие: сообщить факт",
      "Вложенный факт: встреча отменена, одна встреча",
      "Время: прошедшее",
      "Отрицание относится к матричному предикату «не сказал»"
    ],
    "source_text_by_language": {
      "ru": "Он не сказал, что встреча отменена.",
      "en": "He did not say that the meeting was cancelled.",
      "he": "הוא לא אמר שהפגישה בוטלה."
    },
    "facts": {
      "participant": "один мужчина",
      "action": "сообщение об отмене встречи",
      "owner": "мужчина-сообщающий",
      "time": "прошедшее",
      "number": "одна встреча"
    }
  },
  "D1000-20261025:TR01-NEGATION-SCOPE:F00002": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00002",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00002",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Женщина сообщила о неготовности отчёта: отрицание относится к готовности отчёта, а не к сообщению.",
    "semantic_facts": [
      "Участник: одна женщина",
      "Действие: сообщить факт",
      "Вложенный факт: отчёт не готов, один отчёт",
      "Время: прошедшее",
      "Отрицание относится к вложенному предикату «не готов»"
    ],
    "source_text_by_language": {
      "ru": "Она сказала, что отчёт не готов.",
      "en": "She said that the report was not ready.",
      "he": "היא אמרה שהדוח לא מוכן."
    },
    "facts": {
      "participant": "одна женщина",
      "action": "сообщение о неготовности отчёта",
      "owner": "женщина-сообщающая",
      "time": "прошедшее",
      "number": "один отчёт"
    }
  },
  "D1000-20261025:TR01-NEGATION-SCOPE:F00003": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00003",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00003",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Группа не давала обещания закончить ремонт сегодня: отрицание относится к обещанию, а не к завершению.",
    "semantic_facts": [
      "Участники: группа, мы",
      "Действие: обещание",
      "Вложенный факт: закончить ремонт сегодня, один ремонт",
      "Время обещания: прошедшее; срок: сегодня",
      "Отрицание относится к матричному предикату «не обещали»"
    ],
    "source_text_by_language": {
      "ru": "Мы не обещали закончить ремонт сегодня.",
      "en": "We did not promise to finish the repair today.",
      "he": "לא הבטחנו לסיים את התיקון היום."
    },
    "facts": {
      "participant": "группа, мы",
      "action": "обещание закончить ремонт",
      "owner": "группа-обещающая",
      "time": "прошедшее; срок сегодня",
      "number": "один ремонт"
    }
  },
  "D1000-20261025:TR01-NEGATION-SCOPE:F00004": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00004",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00004",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Говорящий обещал не опаздывать завтра: отрицание относится к опозданию, а не к обещанию.",
    "semantic_facts": [
      "Участник: один говорящий, я",
      "Действие: обещание",
      "Вложенный факт: не опаздывать завтра",
      "Время обещания: прошедшее; срок: завтра",
      "Отрицание относится к вложенному предикату «не опаздывать»"
    ],
    "source_text_by_language": {
      "ru": "Я обещал не опаздывать завтра.",
      "en": "I promised not to be late tomorrow.",
      "he": "הבטחתי לא לאחר מחר."
    },
    "facts": {
      "participant": "один говорящий, я",
      "action": "обещание не опаздывать",
      "owner": "говорящий-обещающий",
      "time": "прошедшее; срок завтра",
      "number": "одно обещание"
    }
  },
  "D1000-20261025:TR01-NEGATION-SCOPE:F00005": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00005",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00005",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Группа не сообщила о готовности договора; добавлено нейтральное второе предложение о деталях.",
    "semantic_facts": [
      "Участники: группа, они",
      "Действие: сообщить факт",
      "Вложенный факт: договор готов, один договор",
      "Время: прошедшее; детали позже",
      "Отрицание относится к матричному предикату «не сообщили»"
    ],
    "source_text_by_language": {
      "ru": "Они не сообщили, что договор готов. Подробности будут позже.",
      "en": "They did not report that the contract was ready. The details will come later.",
      "he": "הם לא הודיעו שהחוזה מוכן. הפרטים יגיעו מאוחר יותר."
    },
    "facts": {
      "participant": "группа, они",
      "action": "сообщение о готовности договора",
      "owner": "группа-сообщающая",
      "time": "прошедшее; детали позже",
      "number": "один договор"
    }
  },
  "D1000-20261025:TR01-NEGATION-SCOPE:F00006": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00006",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00006",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Мужчина сообщил о неприбытии посылки: отрицание относится к прибытию посылки, а не к сообщению.",
    "semantic_facts": [
      "Участник: один мужчина",
      "Действие: сообщить факт",
      "Вложенный факт: посылка не пришла, одна посылка",
      "Время: прошедшее",
      "Отрицание относится к вложенному предикату «не пришла»"
    ],
    "source_text_by_language": {
      "ru": "Он сказал, что посылка не пришла.",
      "en": "He said that the parcel had not arrived.",
      "he": "הוא אמר שהחבילה לא הגיעה."
    },
    "facts": {
      "participant": "один мужчина",
      "action": "сообщение о неприбытии посылки",
      "owner": "мужчина-сообщающий",
      "time": "прошедшее",
      "number": "одна посылка"
    }
  },
  "D1000-20261025:TR01-NEGATION-SCOPE:F00007": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00007",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00007",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Женщина не давала обещания позвонить вечером: отрицание относится к обещанию, а не к звонку.",
    "semantic_facts": [
      "Участник: одна женщина",
      "Действие: обещание",
      "Вложенный факт: позвонить вечером, один звонок",
      "Время обещания: прошедшее; срок: вечер",
      "Отрицание относится к матричному предикату «не обещала»"
    ],
    "source_text_by_language": {
      "ru": "Она не обещала позвонить вечером.",
      "en": "She did not promise to call in the evening.",
      "he": "היא לא הבטיחה להתקשר בערב."
    },
    "facts": {
      "participant": "одна женщина",
      "action": "обещание позвонить",
      "owner": "женщина-обещающая",
      "time": "прошедшее; срок вечером",
      "number": "один звонок"
    }
  },
  "D1000-20261025:TR01-NEGATION-SCOPE:F00008": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00008",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00008",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Группа обещала не трогать ящик: отрицание относится к касанию ящика, а не к обещанию.",
    "semantic_facts": [
      "Участники: группа, мы",
      "Действие: обещание",
      "Вложенный факт: не трогать этот ящик, один ящик",
      "Время обещания: прошедшее",
      "Отрицание относится к вложенному предикату «не трогать»"
    ],
    "source_text_by_language": {
      "ru": "Мы обещали не трогать этот ящик.",
      "en": "We promised not to touch this box.",
      "he": "הבטחנו לא לגעת בקופסה הזאת."
    },
    "facts": {
      "participant": "группа, мы",
      "action": "обещание не трогать ящик",
      "owner": "группа-обещающая",
      "time": "прошедшее",
      "number": "один ящик"
    }
  },
  "D1000-20261025:TR01-NEGATION-SCOPE:F00009": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00009",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00009",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Мужчина не сообщил о подписании документов: отрицание относится к глаголу сообщения, а не к подписанию.",
    "semantic_facts": [
      "Участник: один мужчина",
      "Действие: сообщить факт",
      "Вложенный факт: документы подписаны, несколько документов",
      "Время: прошедшее",
      "Отрицание относится к матричному предикату «не сказал»"
    ],
    "source_text_by_language": {
      "ru": "Он не сказал, что документы подписаны.",
      "en": "He did not say that the documents had been signed.",
      "he": "הוא לא אמר שהמסמכים נחתמו."
    },
    "facts": {
      "participant": "один мужчина",
      "action": "сообщение о подписании документов",
      "owner": "мужчина-сообщающий",
      "time": "прошедшее",
      "number": "несколько документов"
    }
  },
  "D1000-20261025:TR01-NEGATION-SCOPE:F00010": {
    "family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00010",
    "origin_family_id": "D1000-20261025:TR01-NEGATION-SCOPE:F00010",
    "scenario_group": "TR01-NEGATION-SCOPE",
    "scenario": "Группа сообщила о неисправности оборудования; добавлено нейтральное второе предложение о ремонте в пятницу.",
    "semantic_facts": [
      "Участники: группа, они",
      "Действие: сообщить факт",
      "Вложенный факт: оборудование не работает, один комплект оборудования",
      "Время: прошедшее; ремонт в пятницу",
      "Отрицание относится к вложенному предикату «не работает»"
    ],
    "source_text_by_language": {
      "ru": "Они сообщили, что оборудование не работает. Ремонт назначен на пятницу.",
      "en": "They reported that the equipment was not working. The repair is scheduled for Friday.",
      "he": "הם הודיעו שהציוד לא עובד. התיקון נקבע ליום שישי."
    },
    "facts": {
      "participant": "группа, они",
      "action": "сообщение о неисправности оборудования",
      "owner": "группа-сообщающая",
      "time": "прошедшее; ремонт в пятницу",
      "number": "один комплект оборудования"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A012\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A012\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR01, profile_id=he-en, split=train. metadata.learning_objective_id=TR01-NEGATION-SCOPE; metadata.scenario_group=TR01-NEGATION-SCOPE; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR01-NEGATION-SCOPE.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.negation-report",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. \nИсходный текст: היא לא אמרה שהמכתב נשלח.",
    "chosen": "She did not say that the letter had been sent.",
    "rejected": "She said that the letter had not been sent.",
    "error_tag": "tr01_negation_scope",
    "rationale": "Отрицание сообщения не утверждает сообщение об отсутствии отправки. Rejected переносит отрицание со «сказала» на «отправлено».",
    "simple_check": "Отрицание сообщения не утверждает сообщение об отсутствии отправки. Rejected переносит отрицание со «сказала» на «отправлено».",
    "counterexample": "Если исходник действительно означает «сказала, что письмо не отправлено», предпочтение надо обратить. Отсутствие сообщения само по себе не устанавливает факт отправки.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "היא לא אמרה שהמכתב נשלח.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR01-NEGATION-SCOPE.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.negation-promise",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. \nИсходный текст: לא הבטחנו לסיים היום.",
    "chosen": "We did not promise to finish today.",
    "rejected": "We promised not to finish today.",
    "error_tag": "tr01_negation_scope",
    "rationale": "Не давать обещание закончить и дать обещание не заканчивать — разные обязательства; в rejected отрицание относится к завершению.",
    "simple_check": "Не давать обещание закончить и дать обещание не заканчивать — разные обязательства; в rejected отрицание относится к завершению.",
    "counterexample": "Не выводить из отрицания обещания ни невозможность завершения, ни запрет на завершение. В карточке не обучается юридическая интерпретация обязательств.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "לא הבטחנו לסיים היום.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR01-NEGATION-SCOPE.he-en.not-all-is-not-none",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR01-NEGATION-SCOPE",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nИсходный текст: לא כל הדלתות נעולות.",
    "chosen": "Not all doors are locked.",
    "rejected": "No doors are locked.",
    "error_tag": "not-all-is-not-none",
    "rationale": "Use the small counterexample [locked, unlocked]: 'not all locked' is true, while 'no doors locked' is false. The translation must allow this source-compatible situation.",
    "simple_check": "Use the small counterexample [locked, unlocked]: 'not all locked' is true, while 'no doors locked' is false. The translation must allow this source-compatible situation.",
    "wrong_authoring_trap": {
      "bad_authoring": "Equating 'not every' with 'none', or adding an unsupported assertion that at least one door is locked.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "לא כל הדלתות נעולות.",
      "source_raw": "לא כל הדלתות נעולות.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.not-all-is-not-none",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.not-all-is-not-none"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A012\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A012\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A012\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A012\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
