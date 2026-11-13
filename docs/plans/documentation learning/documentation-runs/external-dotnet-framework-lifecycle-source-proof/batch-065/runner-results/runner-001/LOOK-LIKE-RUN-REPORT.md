# Code Documentation Batch Run

Status: PASS
Metric mode: runner_verified

| Metric | Value |
|---|---:|
| train rows | 50 |
| held-out rows | 50 |
| routing accuracy | 1.000 |
| source gate accuracy | 1.000 |
| version gate accuracy | 1.000 |
| transit gate accuracy | 1.000 |
| bottom-up order accuracy | 0.940 |
| callee-summary transit accuracy | 0.940 |
| local-vs-delegated accuracy | 0.940 |
| CPU ms | 593.75 |
| peak RAM MB | 36.273 |
| elapsed ms | 559.624 |

This run verifies the trainable documentation router path. It does not claim a full LoRA checkpoint unless a separate DPO model runner is attached.