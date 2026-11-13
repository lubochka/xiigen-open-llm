# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch027-v1`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-027\training-pairs-train-ready.jsonl`
- Source SHA256: `5B2B688801DA54E4762D890C689BA761B9C658F724A829A4FCEB337D124F5C54`
- Pairs:         50
- Batches:       5
- Batch size:    10
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-027\dpo-run\ru-en-l1-batch027-v1`
- Total elapsed: 13413.4 ms
- Avg loss:      0.677519
- Final loss:    0.677067
- Avg accuracy:  0.940
- Final accuracy:0.900
- Avg margin:    +0.031650
- Final margin:  +0.032614

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 10 | 0.676082 | 1.000 | +0.034603 | 2843.8 | ru-en-general-l1-b027-001 | ru-en-general-l1-b027-010 |
| 2 | 10 | 0.675954 | 1.000 | +0.034794 | 2816.5 | ru-en-general-l1-b027-011 | ru-en-general-l1-b027-020 |
| 3 | 10 | 0.680141 | 0.900 | +0.026346 | 2228.4 | ru-en-general-l1-b027-021 | ru-en-general-l1-b027-030 |
| 4 | 10 | 0.678352 | 0.900 | +0.029896 | 2710.7 | ru-en-general-l1-b027-031 | ru-en-general-l1-b027-040 |
| 5 | 10 | 0.677067 | 0.900 | +0.032614 | 2538.0 | ru-en-general-l1-b027-041 | ru-en-general-l1-b027-050 |
