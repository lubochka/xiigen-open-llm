# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch002`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-002\training-pairs-combined-100.jsonl`
- Source SHA256: `BB413722C0C83F1B7B858547248B10F5E73737F14A83CE8E6FB263C9C6E9F9EF`
- Pairs:         100
- Batches:       2
- Batch size:    50
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-002\dpo-run\ru-en-l1-batch002`
- Total elapsed: 26277.3 ms
- Avg loss:      0.693990
- Final loss:    0.697888
- Avg accuracy:  0.480
- Final accuracy:0.280
- Avg margin:    -0.001577
- Final margin:  -0.009364

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 50 | 0.690092 | 0.680 | +0.006210 | 13822.9 | TR-RU_EN-L1-T01-0001 | TR-RU_EN-L1-T01-0050 |
| 2 | 50 | 0.697888 | 0.280 | -0.009364 | 12321.2 | ru-en-g-l1-b002-train-0001 | ru-en-g-l1-b002-train-0050 |
