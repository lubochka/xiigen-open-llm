# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch025-v5`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-025\training-pairs-train-ready.jsonl`
- Source SHA256: `E147D26B40995E035DFC288033C16272060AAAD54FA1C1CBE00CFC4A1FA64128`
- Pairs:         50
- Batches:       5
- Batch size:    10
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-025\dpo-run\ru-en-l1-batch025-v5`
- Total elapsed: 13809.6 ms
- Avg loss:      0.690398
- Final loss:    0.691131
- Avg accuracy:  0.580
- Final accuracy:0.600
- Avg margin:    +0.005695
- Final margin:  +0.004161

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 10 | 0.689068 | 0.600 | +0.008294 | 3316.0 | ru-en-general-l1-b025-001 | ru-en-general-l1-b025-010 |
| 2 | 10 | 0.692571 | 0.400 | +0.001307 | 2366.3 | ru-en-general-l1-b025-011 | ru-en-general-l1-b025-020 |
| 3 | 10 | 0.692672 | 0.700 | +0.001056 | 2775.2 | ru-en-general-l1-b025-021 | ru-en-general-l1-b025-030 |
| 4 | 10 | 0.686549 | 0.600 | +0.013656 | 2283.3 | ru-en-general-l1-b025-031 | ru-en-general-l1-b025-040 |
| 5 | 10 | 0.691131 | 0.600 | +0.004161 | 2810.0 | ru-en-general-l1-b025-041 | ru-en-general-l1-b025-050 |
