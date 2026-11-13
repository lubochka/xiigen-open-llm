# Phase 12 cross-host token and quality comparison

Вердикт: PHASE_12_NEEDS_FIX.

Что улучшилось: В harness-оценке adaptive/core trace использует меньше эвристических токенов, чем прозрачный synthetic baseline manual packet, на 9 из 9 пар.
Что не улучшилось: Adaptive quality не закрыла все rubric criteria: orchestration-plan и skill-selection остаются partial/not proven, поэтому token success не принят.
Что не доказано: Не доказаны live chat UI token savings, semantic quality improvement, learnability, training success, checkpoint promotion, full Codex/Claude/Cursor UI readiness или all-host final readiness.

Регресс качества где-либо: False.
Token success accepted: False.
Token success blocked by quality: True.

Не-claims:
- No live Claude/Cursor/Codex chat UI token savings are claimed.
- No semantic quality improvement is claimed.
- No learnability, DPO, training success, checkpoint promotion, or checkpoint mutation is claimed.
- No full chat UI or all-host final readiness is claimed.
- No status board update, git stage, commit, or push was performed by the runner.

Evidence paths:
- run_dir: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z
- scenario_pair_matrix_json: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z\scenario-pair-matrix.json
- baseline_token_traces_json: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z\baseline-token-traces.json
- adaptive_token_traces_json: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z\adaptive-token-traces.json
- token_accounting_variance_report_json: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z\token-accounting-variance-report.json
- quality_rubric_json: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z\quality-rubric.json
- quality_scoring_report_json: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z\quality-scoring-report.json
- heldout_ablation_no_leakage_report_json: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z\heldout-ablation-no-leakage-report.json
- independent_review_packet_json: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z\independent-token-quality-review-packet.json
- phase12_report_json: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison\phase12-cross-host-token-quality-20260623T1610377734755Z\phase12-cross-host-token-quality-report.json
