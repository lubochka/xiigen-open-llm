# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR-EN-IF-01/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 880; after it, domain remainder: 110. Finishing this batch does not by itself finish the domain.

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
{"pair_id":"D1000-20261025:A038:P00011","family_id":"D1000-20261025:TR-IF-01:F00011"},
{"pair_id":"D1000-20261025:A038:P00012","family_id":"D1000-20261025:TR-IF-01:F00012"},
{"pair_id":"D1000-20261025:A038:P00013","family_id":"D1000-20261025:TR-IF-01:F00013"},
{"pair_id":"D1000-20261025:A038:P00014","family_id":"D1000-20261025:TR-IF-01:F00014"},
{"pair_id":"D1000-20261025:A038:P00015","family_id":"D1000-20261025:TR-IF-01:F00015"},
{"pair_id":"D1000-20261025:A038:P00016","family_id":"D1000-20261025:TR-IF-01:F00016"},
{"pair_id":"D1000-20261025:A038:P00017","family_id":"D1000-20261025:TR-IF-01:F00017"},
{"pair_id":"D1000-20261025:A038:P00018","family_id":"D1000-20261025:TR-IF-01:F00018"},
{"pair_id":"D1000-20261025:A038:P00019","family_id":"D1000-20261025:TR-IF-01:F00019"},
{"pair_id":"D1000-20261025:A038:P00020","family_id":"D1000-20261025:TR-IF-01:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR-IF-01:F00011": {
    "family_id": "D1000-20261025:TR-IF-01:F00011",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00011",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило молока: если молоко холодное, налить стакан.",
    "semantic_facts": [
      "Условие: молоко холодное",
      "Действие: налить стакан",
      "Связь: холодное молоко → налить",
      "Ситуация проверки: при холодном молоке условие выполнено, при тёплом нет",
      "Предмет и действие не меняются"
    ],
    "source_text_by_language": {
      "ru": "Если молоко холодное, наливай стакан.",
      "en": "If the milk is cold, pour the glass.",
      "he": "אם החלב קר, מזג את הכוס."
    },
    "facts": {
      "participant": "один пользователь кухни",
      "action": "наливание стакана по условию",
      "owner": "пользователь",
      "time": "правило",
      "number": "один стакан"
    }
  },
  "D1000-20261025:TR-IF-01:F00012": {
    "family_id": "D1000-20261025:TR-IF-01:F00012",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00012",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило дороги: если дорога мокрая, снизить скорость.",
    "semantic_facts": [
      "Условие: дорога мокрая",
      "Действие: снизить скорость",
      "Связь: мокрая дорога → снизить",
      "Ситуация проверки: при мокрой дороге условие выполнено, при сухой нет",
      "Предмет и действие не меняются"
    ],
    "source_text_by_language": {
      "ru": "Если дорога мокрая, снизь скорость.",
      "en": "If the road is wet, reduce the speed.",
      "he": "אם הכביש רטוב, הפחת את המהירות."
    },
    "facts": {
      "participant": "один водитель",
      "action": "снижение скорости по условию",
      "owner": "водитель",
      "time": "правило",
      "number": "одна дорога"
    }
  },
  "D1000-20261025:TR-IF-01:F00013": {
    "family_id": "D1000-20261025:TR-IF-01:F00013",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00013",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило номера: если номер занят, оставить записку дежурному.",
    "semantic_facts": [
      "Условие: номер занят",
      "Действие: оставить записку дежурному",
      "Связь: занятый номер → оставить записку",
      "Ситуация проверки: при занятом номере условие выполнено, при свободном нет",
      "Предмет и действие не меняются"
    ],
    "source_text_by_language": {
      "ru": "Если номер занят, оставь записку дежурному.",
      "en": "If the room is occupied, leave a note for the attendant.",
      "he": "אם החדר תפוס, השאר פתק לפקיד."
    },
    "facts": {
      "participant": "один посетитель",
      "action": "оставление записки по условию",
      "owner": "посетитель",
      "time": "правило",
      "number": "один номер, одна записка"
    }
  },
  "D1000-20261025:TR-IF-01:F00014": {
    "family_id": "D1000-20261025:TR-IF-01:F00014",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00014",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило льда: если лёд тонкий, не выходить на озеро.",
    "semantic_facts": [
      "Условие: лёд тонкий",
      "Действие: не выходить на озеро",
      "Связь: тонкий лёд → не выходить",
      "Ситуация проверки: при тонком льде условие выполнено, при толстом нет",
      "Предмет и действие не меняются"
    ],
    "source_text_by_language": {
      "ru": "Если лёд тонкий, не выходи на озеро.",
      "en": "If the ice is thin, do not go out on the lake.",
      "he": "אם הקרח דק, אל תצא לאגם."
    },
    "facts": {
      "participant": "один посетитель озера",
      "action": "невыход на озеро по условию",
      "owner": "посетитель",
      "time": "правило",
      "number": "одно озеро"
    }
  },
  "D1000-20261025:TR-IF-01:F00015": {
    "family_id": "D1000-20261025:TR-IF-01:F00015",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00015",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило склада: открывать склад только если предъявлен пропуск — необходимое условие, не достаточное основание.",
    "semantic_facts": [
      "Действие: открыть склад",
      "Необходимое условие: предъявлен пропуск",
      "Связь: «только если» — без пропуска склад не открывают; пропуск сам не приказывает открытие",
      "Ситуация проверки: предъявленный пропуск без открытия правило не нарушает",
      "Различие необходимое/достаточное сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Открывай склад, только если предъявлен пропуск.",
      "en": "Open the warehouse only if the pass is shown.",
      "he": "פתח את המחסן רק אם מוצגת הכרטיס."
    },
    "facts": {
      "participant": "один кладовщик",
      "action": "открытие склада по необходимому условию",
      "owner": "кладовщик",
      "time": "правило",
      "number": "один склад, один пропуск"
    }
  },
  "D1000-20261025:TR-IF-01:F00016": {
    "family_id": "D1000-20261025:TR-IF-01:F00016",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00016",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило акта: подписывать акт только если проверены все пункты — необходимое условие, не достаточное основание.",
    "semantic_facts": [
      "Действие: подпись акта",
      "Необходимое условие: проверены все пункты",
      "Связь: «только если» — без проверки пунктов акт не подписывают; проверка сама не приказывает подпись",
      "Ситуация проверки: проверенные пункты без подписи правило не нарушает",
      "Различие необходимое/достаточное сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Подписывай акт, только если проверены все пункты.",
      "en": "Sign the act only if all the items have been checked.",
      "he": "חתום על הפרוטוקול רק אם נבדקו כל הסעיפים."
    },
    "facts": {
      "participant": "один проверяющий",
      "action": "подпись акта по необходимому условию",
      "owner": "проверяющий",
      "time": "правило",
      "number": "один акт, несколько пунктов"
    }
  },
  "D1000-20261025:TR-IF-01:F00017": {
    "family_id": "D1000-20261025:TR-IF-01:F00017",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00017",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило насоса: запускать насос только если закрыт клапан — необходимое условие, не достаточное основание.",
    "semantic_facts": [
      "Действие: запуск насоса",
      "Необходимое условие: клапан закрыт",
      "Связь: «только если» — при открытом клапане насос не запускают; закрытый клапан сам не приказывает запуск",
      "Ситуация проверки: закрытый клапан без запуска правило не нарушает",
      "Различие необходимое/достаточное сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Запускай насос, только если закрыт клапан.",
      "en": "Start the pump only if the valve is closed.",
      "he": "הפעל את המשאבה רק אם השסתום סגור."
    },
    "facts": {
      "participant": "один оператор",
      "action": "запуск насоса по необходимому условию",
      "owner": "оператор",
      "time": "правило",
      "number": "один насос, один клапан"
    }
  },
  "D1000-20261025:TR-IF-01:F00018": {
    "family_id": "D1000-20261025:TR-IF-01:F00018",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00018",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило будильника: если звонит будильник, сосед просыпается — направление условной связи не обращается.",
    "semantic_facts": [
      "Условие: звонит будильник",
      "Следствие: сосед просыпается",
      "Связь: звонок будильника → пробуждение соседа; обратное не задано",
      "Ситуация проверки: сосед может проснуться и по другой причине; исходник это не исключает",
      "Направление A→B сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Если звонит будильник, сосед просыпается.",
      "en": "If the alarm clock rings, the neighbor wakes up.",
      "he": "אם השעון המעורר מצלצל, השכן מתעורר."
    },
    "facts": {
      "participant": "один сосед",
      "action": "пробуждение по будильнику",
      "owner": "сосед",
      "time": "правило",
      "number": "один будильник, один сосед"
    }
  },
  "D1000-20261025:TR-IF-01:F00019": {
    "family_id": "D1000-20261025:TR-IF-01:F00019",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00019",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило плиты: если включают плиту, чайник шумит — направление условной связи не обращается.",
    "semantic_facts": [
      "Условие: включают плиту",
      "Следствие: чайник шумит",
      "Связь: включение плиты → шум чайника; обратное не задано",
      "Ситуация проверки: чайник может шуметь и по другой причине; исходник это не исключает",
      "Направление A→B сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Если включают плиту, чайник шумит.",
      "en": "If the stove is turned on, the kettle hums.",
      "he": "אם מדליקים את הכיריים, הקומקום שואג."
    },
    "facts": {
      "participant": "один пользователь кухни",
      "action": "шум чайника по включению плиты",
      "owner": "пользователь",
      "time": "правило",
      "number": "одна плита, один чайник"
    }
  },
  "D1000-20261025:TR-IF-01:F00020": {
    "family_id": "D1000-20261025:TR-IF-01:F00020",
    "origin_family_id": "D1000-20261025:TR-IF-01:F00020",
    "scenario_group": "TR-IF-01",
    "scenario": "Правило лампы: если мигает лампа, мастер идёт в подвал — направление условной связи не обращается.",
    "semantic_facts": [
      "Условие: мигает лампа",
      "Следствие: мастер идёт в подвал",
      "Связь: мигание лампы → поход мастера в подвал; обратное не задано",
      "Ситуация проверки: мастер может идти в подвал и по другой причине; исходник это не исключает",
      "Направление A→B сохраняется"
    ],
    "source_text_by_language": {
      "ru": "Если мигает лампа, мастер идёт в подвал.",
      "en": "If the lamp blinks, the repairman goes to the basement.",
      "he": "אם המנורה מהבהבת, הטכנאי יורד למרתף."
    },
    "facts": {
      "participant": "один мастер",
      "action": "поход в подвал по миганию лампы",
      "owner": "мастер",
      "time": "правило",
      "number": "одна лампа, один подвал"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0002\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A038\B0002\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
