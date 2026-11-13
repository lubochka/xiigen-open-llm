# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR-EN-IF-01/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 870; after it, domain remainder: 120. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Условие действия и направление условной связи — английский",
  "instructions": "## Предметный промпт китайской модели через Kiro\n\nЭтот блок дополняет общий MASTER и request.json. Генератор использует уже выбранную китайскую модель Kiro, помощники наследуют её. Не подменять её CODEX и не подменять оценщика генератором. Если отображается только селектор, записать его как наблюдаемое имя без выдуманной точной версии.\n\nВыполни TR-EN-IF-01, только topic/profile/split/count текущего request.json.\nЦель: Переводить условные правила, сохраняя условие, отрицание внутри него, направление связи и различие «если»/«только если».\n2. Для одного профиля создай требуемое число новых независимых сценариев в пределах квот. Reference-сценарии служат примерами и не становятся EXAM.\n3. Сначала зафиксируй source_raw и семейство. Напиши полноценный перевод и один правдоподобный неправильный перевод. Оба — на целевом языке и в одном формате.\n4. Проверь, что ошибка относится только к смысловому отношению этой карточки. При двух допустимых ответах дай quarantine с причиной вместо ложной пары.\n5. Сохрани prompt, chosen, rejected и все поля xiigen.dpo.v1. Rationale, tags, IDs, смысловая схема и проверочная ситуация остаются только в служебных полях.\n6. Не вставляй [WRONG], рейтинг или пояснение дефекта внутрь rejected. Не режь пару по символам; различающий фрагмент и конец ответа должны позже поместиться в реальный tokenizer/context.\n7. Чистый текст, опечатки, раскладки, перефразирования и все направления одной семьи имеют один split. Не добавляй трудные искажения в эту партию: для них отдельные TR07/TR08.\n8. Сохрани draft.jsonl и русский report.md. Это кандидаты. Отдельная оценка качества сейчас не назначена; после записи и структурного finish перейти к следующему потоку.\n9. При NEXT переходи к указанному ID, не дописывай прошлую задачу. При STOP немедленно прекращай действия. Старые файлы, эксперименты и результаты не удаляй.",
  "answer_contract": "Follow the visible response format in this lesson and its examples; preserve the learner task.",
  "hard_constraints": [],
  "scenario_axes": [],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A038:P00001","family_id":"D1000-20261025:TR-IF-01:F00001"},
{"pair_id":"D1000-20261025:A038:P00002","family_id":"D1000-20261025:TR-IF-01:F00002"},
{"pair_id":"D1000-20261025:A038:P00003","family_id":"D1000-20261025:TR-IF-01:F00003"},
{"pair_id":"D1000-20261025:A038:P00004","family_id":"D1000-20261025:TR-IF-01:F00004"},
{"pair_id":"D1000-20261025:A038:P00005","family_id":"D1000-20261025:TR-IF-01:F00005"},
{"pair_id":"D1000-20261025:A038:P00006","family_id":"D1000-20261025:TR-IF-01:F00006"},
{"pair_id":"D1000-20261025:A038:P00007","family_id":"D1000-20261025:TR-IF-01:F00007"},
{"pair_id":"D1000-20261025:A038:P00008","family_id":"D1000-20261025:TR-IF-01:F00008"},
{"pair_id":"D1000-20261025:A038:P00009","family_id":"D1000-20261025:TR-IF-01:F00009"},
{"pair_id":"D1000-20261025:A038:P00010","family_id":"D1000-20261025:TR-IF-01:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR-IF-01:F00001": {
    "family_id": "D1000-20261025:TR-IF-01:F00001",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00001",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило чайника: если вода горячая, выключить чайник.",
    "semantic_facts": [
      "Условие: вода горячая",
      "Действие: выключить чайник",
      "Связь: горячая вода → выключить",
      "Ситуация проверки: при горячей воде условие выполнено, при холодной нет",
      "Предмет и действие не меняются"
    ],
    "source_text_by_language": {
      "ru": "Если вода горячая, выключи чайник.",
      "en": "If the water is hot, turn off the kettle.",
      "he": "אם המים חמים, כבה את הקומקום."
    },
    "facts": {
      "participant": "один пользователь кухни",
      "action": "выключение чайника по условию",
      "owner": "пользователь",
      "time": "правило",
      "number": "один чайник"
    }
  },
  "D1000-20261025:TR-IF-01:F00002": {
    "family_id": "D1000-20261025:TR-IF-01:F00002",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00002",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило окна: если окно открыто, закрыть задвижку.",
    "semantic_facts": [
      "Условие: окно открыто",
      "Действие: закрыть задвижку",
      "Связь: открытое окно → закрыть задвижку",
      "Ситуация проверки: при открытом окне условие выполнено, при закрытом нет",
      "Предмет и действие не меняются"
    ],
    "source_text_by_language": {
      "ru": "Если окно открыто, закрой задвижку.",
      "en": "If the window is open, close the latch.",
      "he": "אם החלון פתוח, סגור את הבריח."
    },
    "facts": {
      "participant": "один пользователь дома",
      "action": "закрытие задвижки по условию",
      "owner": "пользователь",
      "time": "правило",
      "number": "одно окно, одна задвижка"
    }
  },
  "D1000-20261025:TR-IF-01:F00003": {
    "family_id": "D1000-20261025:TR-IF-01:F00003",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00003",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило счёта: если счёт пуст, пополнить карту.",
    "semantic_facts": [
      "Условие: счёт пуст",
      "Действие: пополнить карту",
      "Связь: пустой счёт → пополнить",
      "Ситуация проверки: при пустом счёте условие выполнено, при полном нет",
      "Предмет и действие не меняются"
    ],
    "source_text_by_language": {
      "ru": "Если счёт пуст, пополни карту.",
      "en": "If the account is empty, top up the card.",
      "he": "אם החשבון ריק, טען את הכרטיס."
    },
    "facts": {
      "participant": "один владелец карты",
      "action": "пополнение карты по условию",
      "owner": "владелец",
      "time": "правило",
      "number": "один счёт, одна карта"
    }
  },
  "D1000-20261025:TR-IF-01:F00004": {
    "family_id": "D1000-20261025:TR-IF-01:F00004",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00004",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило батареи: если батарея села, заменить её сегодня.",
    "semantic_facts": [
      "Условие: батарея села",
      "Действие: заменить батарею сегодня",
      "Связь: севшая батарея → заменить",
      "Ситуация проверки: при севшей батарее условие выполнено, при заряженной нет",
      "Предмет и действие не меняются"
    ],
    "source_text_by_language": {
      "ru": "Если батарея села, замени её сегодня.",
      "en": "If the battery is dead, replace it today.",
      "he": "אם הסוללה ריקה, החלף אותה היום."
    },
    "facts": {
      "participant": "один пользователь устройства",
      "action": "замена батареи по условию",
      "owner": "пользователь",
      "time": "сегодня",
      "number": "одна батарея"
    }
  },
  "D1000-20261025:TR-IF-01:F00005": {
    "family_id": "D1000-20261025:TR-IF-01:F00005",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00005",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило ключа: выдать ключ только если подписан журнал — необходимое условие, не достаточное основание.",
    "semantic_facts": [
      "Действие: выдать ключ",
      "Необходимое условие: журнал подписан",
      "Связь: «только если» — без подписи ключ не выдают; подпись сама не приказывает выдачу",
      "Ситуация проверки: подписанный журнал без выдачи ключа правило не нарушает",
      "Различие необходимое/достаточное сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Выдай ключ, только если подписан журнал.",
      "en": "Hand over the key only if the log is signed.",
      "he": "מסור את המפתח רק אם היומן חתום."
    },
    "facts": {
      "participant": "один хранитель ключа",
      "action": "выдача ключа по необходимому условию",
      "owner": "хранитель",
      "time": "правило",
      "number": "один ключ, один журнал"
    }
  },
  "D1000-20261025:TR-IF-01:F00006": {
    "family_id": "D1000-20261025:TR-IF-01:F00006",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00006",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило лаборатории: входить только если надет халат — необходимое условие, не достаточное основание.",
    "semantic_facts": [
      "Действие: вход в лабораторию",
      "Необходимое условие: халат надет",
      "Связь: «только если» — без халата входить нельзя; халат сам не приказывает входить",
      "Ситуация проверки: надетый халат без входа правило не нарушает",
      "Различие необходимое/достаточное сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Входи в лабораторию, только если надет халат.",
      "en": "Enter the laboratory only if the lab coat is on.",
      "he": "היכנס למעבדה רק אם החלוק לבוש."
    },
    "facts": {
      "participant": "один сотрудник",
      "action": "вход в лабораторию по необходимому условию",
      "owner": "сотрудник",
      "time": "правило",
      "number": "одна лаборатория, один халат"
    }
  },
  "D1000-20261025:TR-IF-01:F00007": {
    "family_id": "D1000-20261025:TR-IF-01:F00007",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00007",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило печати: печатать форму только если выбран верный шаблон — необходимое условие, не достаточное основание.",
    "semantic_facts": [
      "Действие: печать формы",
      "Необходимое условие: выбран верный шаблон",
      "Связь: «только если» — без верного шаблона не печатать; верный шаблон сам не приказывает печать",
      "Ситуация проверки: верный шаблон без печати правило не нарушает",
      "Различие необходимое/достаточное сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Печатай форму, только если выбран верный шаблон.",
      "en": "Print the form only if the correct template is selected.",
      "he": "הדפס את הטופס רק אם נבחרה התבנית הנכונה."
    },
    "facts": {
      "participant": "один оператор",
      "action": "печать формы по необходимому условию",
      "owner": "оператор",
      "time": "правило",
      "number": "одна форма, один шаблон"
    }
  },
  "D1000-20261025:TR-IF-01:F00008": {
    "family_id": "D1000-20261025:TR-IF-01:F00008",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00008",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило светофора: если горит красный, машины стоят — направление условной связи не обращается.",
    "semantic_facts": [
      "Условие: горит красный свет",
      "Следствие: машины стоят",
      "Связь: красный свет → машины стоят; обратное не задано",
      "Ситуация проверки: машины могут стоять и по другой причине; исходник это не исключает",
      "Направление A→B сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Если горит красный, машины стоят.",
      "en": "If the red light is on, the cars stop.",
      "he": "אם האור האדום דולק, המכוניות עומדות."
    },
    "facts": {
      "participant": "водители машин",
      "action": "остановка по красному свету",
      "owner": "водители",
      "time": "правило",
      "number": "один красный свет, несколько машин"
    }
  },
  "D1000-20261025:TR-IF-01:F00009": {
    "family_id": "D1000-20261025:TR-IF-01:F00009",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00009",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило дождя: если идёт дождь, тротуар мокрый — направление условной связи не обращается.",
    "semantic_facts": [
      "Условие: идёт дождь",
      "Следствие: тротуар мокрый",
      "Связь: дождь → мокрый тротуар; обратное не задано",
      "Ситуация проверки: тротуар может быть мокрым и по другой причине; исходник это не исключает",
      "Направление A→B сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Если идёт дождь, тротуар мокрый.",
      "en": "If it is raining, the sidewalk is wet.",
      "he": "אם יורד גשם, המדרכה רטובה."
    },
    "facts": {
      "participant": "прохожие",
      "action": "мокрение тротуара по дождю",
      "owner": "погода",
      "time": "правило",
      "number": "один тротуар"
    }
  },
  "D1000-20261025:TR-IF-01:F00010": {
    "family_id": "D1000-20261025:TR-IF-01:F00010",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00010",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило сигнализации: если срабатывает сигнализация, сторож выходит на пост — направление условной связи не обращается.",
    "semantic_facts": [
      "Условие: срабатывает сигнализация",
      "Следствие: сторож выходит на пост",
      "Связь: сигнализация → выход сторожа; обратное не задано",
      "Ситуация проверки: сторож может выйти на пост и по другой причине; исходник это не исключает",
      "Направление A→B сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Если срабатывает сигнализация, сторож выходит на пост.",
      "en": "If the alarm goes off, the guard goes to the post.",
      "he": "אם האזעקה מופעלת, השומר יוצא לעמדה."
    },
    "facts": {
      "participant": "один сторож",
      "action": "выход на пост по сигнализации",
      "owner": "сторож",
      "time": "правило",
      "number": "одна сигнализация, один пост"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR-EN-IF-01, profile_id=he-en, split=train. metadata.learning_objective_id=TR-EN-IF-01; metadata.scenario_group=TR-IF-01; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-043"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR-EN-IF-01.he-en.REF04.P01",
    "profile_id": "he-en",
    "family_id": "sim:translation:logic-v2:guest-not-arrived-keep-ticket",
    "prompt": "Симуляция перевода. Переведи с языка «иврит» на язык «английский». Ответ — только перевод; не добавляй объяснение, служебную разметку или новый факт.\nИсходный текст: אם האורח לא הגיע, שמור את הכרטיס.",
    "chosen": "If the guest has not arrived, keep the ticket.",
    "rejected": "If the guest has arrived, keep the ticket.",
    "error_tag": "condition_negation_loss",
    "rationale": "Отрицание потеряно внутри условия; команда сохранения билета привязана к противоположной ситуации.",
    "simple_check": "При отсутствии гостя условие chosen выполнено, а условие rejected — нет.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "source_language": "he",
      "target_language": "en",
      "source_raw": "אם האורח לא הגיע, שמור את הכרטיס.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR-EN-IF-01.he-en.REF05.P01",
    "profile_id": "he-en",
    "family_id": "sim:translation:logic-v2:both-windows-closed-heating",
    "prompt": "Симуляция перевода. Переведи с языка «иврит» на язык «английский». Ответ — только перевод; не добавляй объяснение, служебную разметку или новый факт.\nИсходный текст: אם שני החלונות סגורים, הפעל את החימום.",
    "chosen": "If both windows are closed, turn on the heating.",
    "rejected": "If at least one of the two windows is closed, turn on the heating.",
    "error_tag": "conjunction_weakened",
    "rationale": "Требование обоих закрытых окон ослаблено до одного. Это одна ошибка связки внутри условия.",
    "simple_check": "Одно окно закрыто, другое открыто: rejected уже предписывает действие, исходник ещё нет.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "source_language": "he",
      "target_language": "en",
      "source_raw": "אם שני החלונות סגורים, הפעל את החימום.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR-EN-IF-01.he-en.REF06.P01",
    "profile_id": "he-en",
    "family_id": "sim:translation:logic-v2:one-entrance-open-enter",
    "prompt": "Симуляция перевода. Переведи с языка «иврит» на язык «английский». Ответ — только перевод; не добавляй объяснение, служебную разметку или новый факт.\nИсходный текст: אם לפחות אחת משתי הכניסות פתוחה, היכנס פנימה.",
    "chosen": "If at least one of the two entrances is open, go inside.",
    "rejected": "If both entrances are open, go inside.",
    "error_tag": "disjunction_strengthened",
    "rationale": "Достаточность одного открытого входа заменена более сильным условием двух открытых входов.",
    "simple_check": "Открыт только один вход: исходное условие достаточно, rejected его не покрывает.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "source_language": "he",
      "target_language": "en",
      "source_raw": "אם לפחות אחת משתי הכניסות פתוחה, היכנס פנימה.",
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


RAG-043 — rag/sources/RAG-043.md
Источник предлагает категории ошибки, пошаговый промпт построения rejected и проверку причинного различия. Одновременно он требует встроенную [WRONG]-аннотацию и перечисляет только четыре направления.
Applicability: Брать категории контрастов и критерий одного содержательного дефекта. [WRONG] и объяснение последствия хранить в rationale/error_tag, не в learner answer. Для текущего набора предусмотрены все шесть направлений EN/RU/HE. Пример formal register не считать чистым стилевым контрастом, если одновременно потеряно обязательство или число дней. Не переносить старые тексты автоматически как новые принятые строки.
Primary excerpt (partial; full excerpt at the assigned path):
A good rejected side is not random bad text. It is a plausible wrong answer that
teaches one specific boundary:

- wrong direction
- wrong topic route
- wrong adapter
- mistranslation
- omission
- addition
- morphology error
- register shift
- named-entity corruption
- Hebrew RTL/script error

Every rejected side must include:

```text
[WRONG: specific_error - functional consequence]

## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
