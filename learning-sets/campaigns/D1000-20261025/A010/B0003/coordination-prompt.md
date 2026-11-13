# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR10-CONDITIONAL/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 3/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 260; after it, domain remainder: 730. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Обучить условному выбору: один конкретный вопрос при неразрешенной неоднозначности и прямой перевод при достаточном контексте.",
  "instructions": "Задача единая: conditional clarification. Не разделяй ambiguous и resolved в одноклассовые учебные наборы: это два класса одной темы, а не разные предметные домены. В каждой партии ровно 50% ambiguous и 50% resolved; generation_batch_size=10. Число строк должно быть четным, иначе подготовка невалидна. Для каждого зарезервированного scenario-family создай ровно две строки: одну с отсутствующим либо недостаточным контекстом и одну с достаточным контекстом. У обеих один family_id и один split, но уникальные pair_id. Тип класса запиши в metadata.clarification_class; это служебная разметка, не подсказка ученику.\n\n1. Выбери source слово с минимум двумя обычными смыслами, которые требуют разных target-переводов. Это одна лексическая семья, несколько sense IDs. Не объединяй разные самостоятельные слова. При одинаковом target-эквиваленте, сохраняющем смысл, обязательного уточнения может не быть; такой пример не входит в ambiguous-квоту.\n2. В ambiguous строке prompt явно разрешает один вопрос на target-языке. Контекста действительно недостаточно. Chosen задает ровно один конкретный вопрос о различии; rejected без основания выбирает один смысл. Валидная неоднозначность не идет в quarantine только из-за отсутствия контекста.\n3. Для resolved строки используй тот же source и добавь видимый факт, исключающий конкурирующий смысл. Chosen дает только правильный перевод, rejected задает уже лишний вопрос. Причинный фактор этой связанной пары — добавленный контекст; не меняй одновременно слово, время, род, раскладку или требование языка.\n4. В generated материале чередуй порядок двух классов и разные смыслы, не строй первые пять строк только из вопросов. Перефразы контекстов не увеличивают число независимых scenario-families. Ambiguous/resolved варианты одного слова и их обратные переводы остаются в одном split во всех профилях.\n5. Quarantine нужен, когда автор не может подтвердить сами значения, целевой перевод, достаточность контекста или предпочтение. Не заменяй сомнительную строку второй строкой другого класса: сохрани квоту и пару сценария. Не добавляй к chosen вопросу перевод, отказ, извинение или выдуманную историю.\n6. При отдельно порученном будущем semantic review потребуется посчитать уместные вопросы, необоснованные конкретные переводы и лишние вопросы при достаточном контексте. Общий процент не должен скрывать провал одного из классов. Примеры в этом файле — публичные reference, не скрытый EXAM.\nSource-specific family: ambiguity belongs to the assigned source language. The two target directions may share the source case; reverse directions are not automatically the same ambiguous lesson. Record at least two sense_translations with en/ru/he equivalents and the exact visible source text. Only the resolved class receives the disambiguating context. A translated unambiguous word is not an ambiguous reverse example.",
  "answer_contract": "При нескольких подтвержденных смыслах и отсутствии достаточного контекста — один конкретный уточняющий вопрос на target-языке. При достаточном контексте — только перевод. Оба класса присутствуют в каждом generation batch в соотношении 50:50; rationale и тип класса не сериализуются в student input.",
  "hard_constraints": [
    {
      "authoring_conditions": {
        "class_field": "metadata.clarification_class",
        "class_quotas": {
          "ambiguous": 0.5,
          "resolved": 0.5
        },
        "require_even_count": true,
        "rows_per_scenario_family": 2,
        "shared_family_across_classes": true
      }
    },
    "FORBIDDEN: Разные предметные темы, source/target профили или учебные objectives в одном shard; ambiguous/resolved разрешены и обязательны как два класса этой одной темы.",
    "FORBIDDEN: Одноклассовая партия только вопросов или только прямых переводов.",
    "FORBIDDEN: Опечатка, раскладка, род или отрицание как второй изменяемый фактор сравнительного сценария.",
    "FORBIDDEN: Скрытый sense_id, clarification_class или правильный перевод в student prompt как готовая метка.",
    "FORBIDDEN: Новый family ID для контекстного варианта исходного сценария; reference/его производные в запечатанном EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "clarification_class",
      "values": [
        "ambiguous",
        "resolved"
      ]
    },
    {
      "axis": "sense",
      "values": [
        "first_common_sense",
        "second_common_sense"
      ]
    },
    {
      "axis": "context_form",
      "values": [
        "absent_or_insufficient",
        "one_sufficient_fact"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A010:P00021","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00011","family_class":"ambiguous"},
{"pair_id":"D1000-20261025:A010:P00022","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00011","family_class":"resolved"},
{"pair_id":"D1000-20261025:A010:P00023","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00012","family_class":"ambiguous"},
{"pair_id":"D1000-20261025:A010:P00024","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00012","family_class":"resolved"},
{"pair_id":"D1000-20261025:A010:P00025","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00013","family_class":"ambiguous"},
{"pair_id":"D1000-20261025:A010:P00026","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00013","family_class":"resolved"},
{"pair_id":"D1000-20261025:A010:P00027","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00014","family_class":"ambiguous"},
{"pair_id":"D1000-20261025:A010:P00028","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00014","family_class":"resolved"},
{"pair_id":"D1000-20261025:A010:P00029","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00015","family_class":"ambiguous"},
{"pair_id":"D1000-20261025:A010:P00030","family_id":"D1000-20261025:TR10-CONDITIONAL-src-he:F00015","family_class":"resolved"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A010\B0003\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A010\B0003\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '03517785a7e540298bb879d1900c174a'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

Set metadata.clarification_class to the exact slot family_class. Produce each assigned class once for that family; do not invent a class mixture.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A010\B0003\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A010\B0003\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

Material kind source_ambiguity: source_text_by_language must contain the assigned source language(s) ["he"]. Also write sense_translations as an object mapping at least two distinct sense IDs to {en:text,ru:text,he:text}. Keep the same ambiguous source input in both classes; the resolved class adds only the visible context. Do not assume the translated words retain the same ambiguity. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

Also save learner_visible_context_by_class as {class:{language:context}} for both assigned classes, covering these source languages: ["he"]. Empty context is allowed when missing context is the assigned condition. Contexts of the two classes must differ. Supply only that class's source-language context to the learner, not the entire author-only facts record. A contrast must be justified in the assigned source language, not inferred from translations.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR10, profile_id=he-en, split=train. metadata.learning_objective_id=TR10-CONDITIONAL; metadata.scenario_group=TR10-CONDITIONAL-src-he; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-045"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR10-CONDITIONAL.he-en.AMBIGUOUS.REF01",
    "profile_id": "he-en",
    "family_id": "v2-ref.translation.ahot-ambiguity",
    "prompt": "Переведи исходное слово с языка «иврит» на язык «английский». Если контекста недостаточно, чтобы выбрать смысл, задай один конкретный уточняющий вопрос на целевом языке. Если контекста достаточно, дай только перевод исходного слова.\nИсходное слово: אחות\nКонтекст: ",
    "chosen": "Do you mean a sister or a nurse?",
    "rejected": "nurse",
    "error_tag": "unsupported_disambiguation",
    "rationale": "Выбранный в rejected смысл возможен, но исходник без контекста не делает его единственным; заданный контракт требует уточнения.",
    "simple_check": "Выбранный в rejected смысл возможен, но исходник без контекста не делает его единственным; заданный контракт требует уточнения.",
    "counterexample": "Если prompt уже сообщает ботанический/профессиональный/зоологический контекст, этот же вопрос становится лишним и не может быть chosen.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "אחות",
      "lexical_family_ids": [
        "lex:he:אחות"
      ],
      "response_contract": "clarify_if_ambiguous",
      "clarification_class": "ambiguous",
      "semantic_context": "",
      "sense_candidates": [
        "female_sibling",
        "nurse_profession"
      ],
      "selected_sense": null
    }
  },
  {
    "example_id": "TR10-CONDITIONAL.he-en.RESOLVED.REF01",
    "profile_id": "he-en",
    "family_id": "v2-ref.translation.ahot-ambiguity",
    "prompt": "Переведи исходное слово с языка «иврит» на язык «английский». Если контекста недостаточно, чтобы выбрать смысл, задай один конкретный уточняющий вопрос на целевом языке. Если контекста достаточно, дай только перевод исходного слова.\nИсходное слово: אחות\nКонтекст: В этой записи указана профессия медицинской работницы больницы, а не родственная связь.",
    "chosen": "nurse",
    "rejected": "Do you mean a sister or a nurse?",
    "error_tag": "unnecessary_clarification",
    "rationale": "Видимый контекст уже исключает второй смысл; вопрос задерживает выполнение вместо правильного перевода.",
    "simple_check": "Видимый контекст уже исключает второй смысл; вопрос задерживает выполнение вместо правильного перевода.",
    "counterexample": "Удаление указанного контекста меняет корректное действие: конкретный перевод больше не доказан, и пара должна быть пересмотрена, а не молча сохранена.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "אחות",
      "lexical_family_ids": [
        "lex:he:אחות"
      ],
      "response_contract": "clarify_if_ambiguous",
      "clarification_class": "resolved",
      "semantic_context": "В этой записи указана профессия медицинской работницы больницы, а не родственная связь.",
      "sense_candidates": [
        "female_sibling",
        "nurse_profession"
      ],
      "selected_sense": "nurse_profession"
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


RAG-045 — rag/sources/RAG-045.md
Источник задаёт исторический TranslationRoutingFlow, полное решение с направлением/регистром/adapter и более узкие Unit-цели. Он запрещает обучать выбор adapter по filenames и подменять учёбу hardcoded switch; старые promotion gates указаны отдельно.
Applicability: Для каждой новой пары явно выбрать получателя: router/flow или translator, и правильную форму его входа/ответа. Не подмешивать route-labels в переводческий урок. Набор адаптируется к шести направлениям; имена исторических интерфейсов не доказывают существование текущего host. Числа confidence из старых примеров не являются калиброванными оценками. [WRONG] исключается из learner output. Старые 99%/95% production promotion gates не подменяют пользовательское правило успеха любого полезного измеренного улучшения; регрессии e2e всё равно показывать. Современная атомарная карточка OR-ROUTE-01 сохраняет свой route|clarify контракт, это иной учебный получатель.
Primary excerpt (partial; full excerpt at the assigned path):
The translation router is a learnable orchestration structure, not a static
classifier. The public training target is:

```csharp
TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>

## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A010\B0003\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A010\B0003\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A010\B0003\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A010\B0003\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '03517785a7e540298bb879d1900c174a'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
