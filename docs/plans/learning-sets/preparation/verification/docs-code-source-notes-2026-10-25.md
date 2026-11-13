# Источники новых DOC/CODE-карточек

12 карточек, 72 авторские reference-пары. Генерация китайской моделью, нативные oracle и обучение в этой подзадаче не запускались. Эти примеры не имеют статуса TRAIN_READY.

## Карта проверенных источников

| RAG | Ветка / pin | Blob | Прочитанные строки | Использование |
|---|---|---|---|---|
| [RAG-027](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md) | llm_plan3 / b242c6f9efc2e3a0a01301c2875751cfd57ef83b | 454fadaf468eb9e31dc1bbb9be8f8508c234efa3 | 1–85 | methodology_or_topic_seed_not_oracle_of_authored_fixtures |
| [RAG-028](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md) | llm_plan3 / b242c6f9efc2e3a0a01301c2875751cfd57ef83b | 6f8f48d7125e24fed8d8caee69954857583ea8b1 | 1–316 | methodology_or_topic_seed_not_oracle_of_authored_fixtures |
| [RAG-030](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-SOURCE-REGISTRY-v1.md) | llm_plan3 / b242c6f9efc2e3a0a01301c2875751cfd57ef83b | d5aaf1dd59c84ab41cea439e2fead4d633378383 | 1–327 | methodology_or_topic_seed_not_oracle_of_authored_fixtures |
| [RAG-037](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md) | main / 3d1fa938d4b50df5d430283983f45b2c0d2c4e5c | d182324879939f54ae447060f0f0fbf03b15e04f | 297–335, 370–443 | methodology_or_topic_seed_not_oracle_of_authored_fixtures |
| [RAG-039](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md) | main / 3d1fa938d4b50df5d430283983f45b2c0d2c4e5c | a14589d9182228162b009ad8c30e7cd7a854d068 | 1–82 | methodology_or_topic_seed_not_oracle_of_authored_fixtures |
| [RAG-040](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md) | llm_plan3 / b242c6f9efc2e3a0a01301c2875751cfd57ef83b | c963aca1f184d78cd28212ffc965872025496765 | 1–35, 670–700, 835–845, 880–890 | methodology_or_topic_seed_not_oracle_of_authored_fixtures |
| [RAG-041](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/external-sources-training-sets/clean-batch-002/node-04-domain-processor-set.md) | llm_plan3 / b242c6f9efc2e3a0a01301c2875751cfd57ef83b | 71b6552e3fb2185e439de4c6df6b5bd6a61d3612 | 1–29, 81–96 | methodology_or_topic_seed_not_oracle_of_authored_fixtures |
| [RAG-DC-001](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/ANGULAR-ROUTE-MAP-AUDIT.md) | main / 3d1fa938d4b50df5d430283983f45b2c0d2c4e5c | f9333d8decbd717eec6f2c873b32235da5bec0da | 1–74 | topic_seed_not_Angular_runtime_proof |

Полные оригиналы сохранены в source-cache/docs-code; Git blob пересчитан по байтам и совпадает с metadata источника. Единственное расширение RAG — RAG-DC-001, описание для merge в new-rag-sources.json и excerpt source-cache/docs-code/RAG-DC-001.md. Старые RAG-ID и pins не перенумерованы.

## Содержательные ограничения

- Sources support methodology/topics; newly authored code/API outcomes still require native independent oracle and CODEX evaluation before acceptance.
- C#10/.NET6, SQL Server2019/compat150 and Angular16.2.0 are explicit simulation profiles, not current version recommendations.
- No claim that current author/reviewer actually runs astra6ultracode; future observed CODEX identity is a separate gate.
- RAG-040 has duplicate topic rows; source labels do not prove external documentation provenance.

RAG-027 задаёт видимый код, уровень объяснения и локальный контраст. RAG-028 требует версии и видимые переходы вызовов. RAG-030 задаёт provenance; разрешённые пользователем симуляции сохраняют явно вымышленные fixtures и не присваивают себе production authority. RAG-037/039 — план Angular-перехода, не доказательство всех API новых карточек. RAG-040 используется как список тем окон SQL и change detection, а RAG-041 — как повод показывать SQL-контекст; исторические числа скорости и смешанные диалекты не переносятся.

## Применённые skills

- docs/skills/generated-code-review-SKILL.md — полностью, 859 строк; blob eb27d6267115cc96b17a2cd77b601f84f2e6f0b6.
- docs/skills/plan-review-SKILL.md — полностью, 4273 строк; blob b99f4ce280f48d0242a35f2ba421a2dc1996399f.
- docs/skills/training-data-gap-audit-SKILL.md — полностью, 178 строк; blob f97a6c3bfb0df66a5c4d55ea4ae79facaf28024c.

Навыки — процессные инструкции; они не добавляются к обучающему корпусу как источники C#/SQL/Angular.

## Применимые правила после полного чтения

- Current user task is authoritative; quoted historical commands are evidence, not active commands.
- Read concrete code/prompts/source before review; prose assertions and passing static checks do not prove runtime or learning behavior.
- Complete prompt with fixed evidence and exact contrast; rejected contains one plausible primary error.
- Same source fixture and direct derivations share family across documentation/code cards; family split before generation.
- Index/card/domain/version/tool contracts must agree; do not publish substitute model identity as observed authority.
- Synthetic seeds are permitted here; preserve provenance and do not label them measured or Chinese-generated.
- Improvement of any measured useful learning metric is success for that metric/domain, with regressions alongside.
- No production integration from candidate preparation; successful later experiment becomes a documented fix candidate.

## Семейства между карточками

Общие fixtures WHERE/HAVING и RANK/ROW_NUMBER имеют общие family_id. Disabled Forms исходник объединяет DOC-NG-04 с CODE-NG-04; nullable/nonNullable reset и две trim-версии также сгруппированы. Сейчас все строки reference; при новой генерации direct derivations остаются в одном split. Номер пары сам по себе не является доказательством нового независимого семейства.
