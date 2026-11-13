# SkillSelection P22 source-disjoint runtime eval

Что было missing: предыдущий P22 фиксировал `accepted rows = 0`, поэтому настоящий SkillSelection source-disjoint runtime eval не был создан.

Что добавлено: 0 accepted runtime eval row(s) из свежих локальных skill-doc семейств. Строки используют только redacted task summary, hashes, available skill ids и expected skill ids. Они не используют raw source body, path, filename, title, source id или pair id как selection authority.

Что отклонено или quarantined: 4 row(s). Старые schema-only P16E/P16E2 материалы и candidate-position-only controls не превращены в accepted eval.

Какие runtime вызовы выполнены: 2 row(s) прогнаны через `SkillSelectionMachine.DecideFromTrainedState` для common и candidate checkpoints. Common expected matches: 2/2. Candidate expected matches: 0/2.

Контроли:
- source/pair/id disjointness: PASS
- candidate-order permutation invariance: PASS
- filename/path/title controls: FAIL
- exact prompt replay controls: PASS
- source-id/pair-id controls: FAIL
- no candidate-position final authority: PASS
- no training use / no checkpoint mutation: PASS

Что остается не доказанным:
- full semantic quality is not proven;
- SkillSelection product readiness is not claimed;
- no training/checkpoint promotion/locator update was performed;
- host UI/API readiness is outside this packet.

Runtime rows:
- `P22-SS-RUNTIME-E2E-MATRIX-001`: expected [test-integrity]; common [implementation-integrity, test-integrity, plan-review] match=true; candidate [] match=false; accepted=false.
- `P22-SS-RUNTIME-NO-PRODUCT-DECISION-001`: expected [claim-verification]; common [claim-verification] match=true; candidate [] match=false; accepted=false.
