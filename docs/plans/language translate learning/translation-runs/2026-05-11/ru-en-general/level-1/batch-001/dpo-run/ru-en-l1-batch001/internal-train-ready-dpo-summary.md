# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch001`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-001\training-pairs-train-ready.jsonl`
- Source SHA256: `DD6AC099810745012F7C9DDB86DA96F4F83ED620BEF53C8AA90A013517D57CE3`
- Pairs:         50
- Batches:       5
- Batch size:    10
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-001\dpo-run\ru-en-l1-batch001`
- Total elapsed: 14197.4 ms
- Avg loss:      0.690297
- Final loss:    0.692118
- Avg accuracy:  0.580
- Final accuracy:0.500
- Avg margin:    +0.005860
- Final margin:  +0.002245

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 10 | 0.685531 | 0.600 | +0.015412 | 3455.6 | TR-RU_EN-L1-T01-0001 | TR-RU_EN-L1-T01-0010 |
| 2 | 10 | 0.691002 | 0.600 | +0.004431 | 2311.2 | TR-RU_EN-L1-T01-0011 | TR-RU_EN-L1-T01-0020 |
| 3 | 10 | 0.687358 | 0.700 | +0.011728 | 2860.8 | TR-RU_EN-L1-T01-0021 | TR-RU_EN-L1-T01-0030 |
| 4 | 10 | 0.695477 | 0.500 | -0.004518 | 2276.9 | TR-RU_EN-L1-T01-0031 | TR-RU_EN-L1-T09-0040 |
| 5 | 10 | 0.692118 | 0.500 | +0.002245 | 3036.3 | TR-RU_EN-L1-CORR-0041 | TR-RU_EN-L1-T01-0050 |
