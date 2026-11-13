# Row12 Cursor token review

Статус: PASS только для узкой проверки токенов row12 Cursor.

## Что проверено

- Checked row12 token packet: `docs/plans/evidence/extension-full-readiness-dual-path/token-savings-cursor-row12-20260628-153837/`.
- Проверены источники из `review-report.json`: `measurement.json`, `execution-report.json`, `pair-validation.json`, `token-traces.json`, `result.json`, host-visible sanitized `result.json`, `blocker-details.json`, а также `row12-cursor-joined-path-a-b-20260628T142044Z/join-report.json`.
- Кандидат `cursor-row12-token-measurement-20260628-153226/` в этой рабочей копии отсутствует.

## Числа

- before 6209.
- after 6209.
- delta 0.
- Наблюдаемая экономия токенов: 0.
- Наблюдаемый процент экономии: 0.
- Сокращение токенов наблюдалось: false.

Вывод: экономия токенов не доказана. При значениях before 6209 и after 6209 допустима только запись delta 0; claim о token savings запрещен.

## Same scenario/pair/hash mapping

- `scenario_id`: `frdp-p01-cursor-token-savings-baseline`.
- `pair_id`: `frdp-p01-row12-cursor-token-baseline-v1`.
- `marker`: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`.
- `prompt_hash`: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`.
- `measurement_same_scenario_id`: true.
- `measurement_same_pair_id`: true.
- `measurement_same_prompt_hash`: true.
- `join_report_same_mapping_found`: true.
- `join_report_used_only_for_mapping`: true.

Та же связка scenario/pair/hash подтверждена только как mapping evidence. Этот отчет не принимает readiness claim из join report.

## Границы утверждений

- `token_savings_claimed`: false.
- `semantic_quality_claimed`: false.
- `quality_claimed`: false.
- `readiness_claimed`: false.
- `external_ai_send`: false.
- `external_ai_used`: false.
- `training_started`: false.
- `checkpoint_mutated`: false.
- `locator_claimed_or_updated_by_review`: false.
- `host_visible_token_savings_proven`: false.

Нет claims for readiness/semantic quality/training/checkpoint/locator. Отчет не доказывает готовность, семантическое качество, обучение, изменение checkpoint или обновление locator.

## Ограничения источников

- `measurement_accounting_type`: `heuristic`.
- `measurement_is_no_send`: true.
- `sanitized_host_visible_packet_status`: `BLOCKED_CURSOR_HOST_VISIBLE_HARNESS_FAILED`.
- `sanitized_packet_previous_measurement_synthetic_no_send_only`: true.
- `token_review_adopts_join_readiness_claim`: false.

Host-visible sanitized packet показывает, что предыдущая measurement была `synthetic_no_send_only`, а host-visible runner был blocker/partial packet. Поэтому текущий token review не доказывает host-visible token savings.

## Следующее bounded action

Не заявлять token savings для row12 Cursor. Следующий worker должен либо распространить delta 0 и `token_savings_claimed=false` в потребляющих handoff/review материалах, либо, если нужен именно savings claim, создать новое bounded paired measurement для той же связки `scenario_id` / `pair_id` / `prompt_hash` на пригодных host-visible trace refs.
