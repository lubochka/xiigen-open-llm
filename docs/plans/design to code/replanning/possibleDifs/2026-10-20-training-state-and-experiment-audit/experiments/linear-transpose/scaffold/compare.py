#!/usr/bin/env python3
"""Read the two bounded native runs; never launch additional work."""
from pathlib import Path
import argparse
import json


def mean(rows, key):
    return sum(float(x[key]) for x in rows)/len(rows)


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--results-root', type=Path, required=True)
    args=parser.parse_args()
    root=Path(__file__).resolve().parent
    meta=json.loads((root/'experiment.json').read_text())
    result=args.results_root.resolve(strict=True)
    domains=['code-sign'] if meta['kind']=='linear-transpose' else ['code-doc','en-ru']
    report={'id':meta['id'],'core_commit':meta['core_commit'],'status':'MEASURED_ONLY_WHERE_RAW_PRESENT','domains':[]}
    for domain in domains:
        a_path,b_path=result/'A'/(domain+'.json'),result/'B'/(domain+'.json')
        if not a_path.exists() or not b_path.exists():
            report['domains'].append({'domain':domain,'status':'INCOMPLETE_NO_EFFECT_CLAIM'})
            continue
        a,b=json.loads(a_path.read_text()),json.loads(b_path.read_text())
        if a['arm']!='A' or b['arm']!='B' or a['core_commit']!=meta['core_commit'] or b['core_commit']!=meta['core_commit']:
            raise SystemExit('RAW_IDENTITY_MISMATCH')
        metrics=[]
        for key in ['allocated_bytes','elapsed_ms','process_cpu_ms']:
            av,bv=a['train_cost'][key],b['train_cost'][key]
            metrics.append({'name':'TRAIN_'+key,'A':av,'B':bv,'B_minus_A':bv-av,
                            'direction':'lower_is_better','local_success':bv<av,'regression':bv>av})
        av,bv=a['process_peak_working_set_bytes'],b['process_peak_working_set_bytes']
        metrics.append({'name':'process_peak_working_set_bytes','A':av,'B':bv,'B_minus_A':bv-av,
                        'direction':'lower_is_better','local_success':bv<av,'regression':bv>av,
                        'scope':'whole native process through TRAIN, includes initial-state readout'})
        if meta['kind']=='linear-transpose':
            same_inputs=a['train']==b['train'] and a['exam']==b['exam'] and a['model_config']==b['model_config'] and a['train_order']==b['train_order']
            identity={'same_initial_trainable':a['trainable_before_sha256']==b['trainable_before_sha256'],
                      'same_initial_frozen':a['frozen_before_sha256']==b['frozen_before_sha256'],
                      'same_final_trainable':a['trainable_after_sha256']==b['trainable_after_sha256'],
                      'same_final_optimizer':a['optimizer_after_sha256']==b['optimizer_after_sha256'],
                      'frozen_unchanged_each':a['frozen_state_unchanged'] and b['frozen_state_unchanged'],
                      'same_training_losses':a['training_losses']==b['training_losses'],
                      'same_before_logits':a['before']==b['before'],'same_after_logits':a['after']==b['after']}
            valid=(a['optimizer_steps']==b['optimizer_steps']==4 and a['actual_parameter_write']
                   and b['actual_parameter_write'] and same_inputs
                   and identity['same_initial_trainable'] and identity['same_initial_frozen']
                   and identity['frozen_unchanged_each'])
            for key,direction in [('target_nll','lower'),('exact','higher')]:
                a0,a1,b0,b1=(mean(x[state],key) for x,state in [(a,'before'),(a,'after'),(b,'before'),(b,'after')])
                did=(b1-b0)-(a1-a0)
                metrics.append({'name':'EXAM_'+key,'A0':a0,'A1':a1,'B0':b0,'B1':b1,
                    'A_change':a1-a0,'B_change':b1-b0,'difference_of_changes':did,
                    'direction':direction+'_is_better','local_success':did<0 if direction=='lower' else did>0,
                    'regression':did>0 if direction=='lower' else did<0})
        else:
            identity={'same_initial_tokenizer':a['state_before_sha256']==b['state_before_sha256'],
                      'same_final_tokenizer':a['state_after_sha256']==b['state_after_sha256'],
                      'same_initial_readout':a['before']==b['before'],
                      'same_final_readout':a['after']==b['after'],
                      'same_vocab_size':a['vocabulary_after']==b['vocabulary_after']}
            same_inputs=(a['train_sha256']==b['train_sha256'] and a['exam_strings']==b['exam_strings']
                         and a['target_vocabulary']==b['target_vocabulary'])
            valid=(a['learning_transitions_measured']==b['learning_transitions_measured']==1
                   and a['learned_merge_count']>0 and b['learned_merge_count']>0
                   and same_inputs and identity['same_initial_tokenizer'])
            for key,direction in [('token_count','lower'),('roundtrip_exact','higher')]:
                a0,a1,b0,b1=(mean(x[state],key) for x,state in [(a,'before'),(a,'after'),(b,'before'),(b,'after')])
                did=(b1-b0)-(a1-a0)
                metrics.append({'name':'EXAM_'+key,'A0':a0,'A1':a1,'B0':b0,'B1':b1,
                    'A_change':a1-a0,'B_change':b1-b0,'difference_of_changes':did,
                    'direction':direction+'_is_better','local_success':did<0 if direction=='lower' else did>0,
                    'regression':did>0 if direction=='lower' else did<0})
        observed_benefits=[m['name'] for m in metrics if m['local_success']]
        if not valid:
            for metric in metrics:
                metric['diagnostic_observed_benefit']=metric['local_success']
                metric['local_success']=False
        report['domains'].append({'domain':domain,
            'comparison_status':'VALID_BOUNDED_LEARNING_COMPARISON' if valid else 'INVALID_CAUSAL_COMPARISON_DIAGNOSTIC_ONLY',
            'learning_and_comparability_admitted':bool(valid),
            'native_state_and_output_identity':identity,'metrics':metrics,
            'successes_by_metric':[m['name'] for m in metrics if m['local_success']] if valid else [],
            'diagnostic_observed_benefits_if_invalid':observed_benefits if not valid else [],
            'regressions_by_metric':[m['name'] for m in metrics if m['regression']],
            'state_or_output_divergence':[key for key,value in identity.items() if not value],
            'timing_limit':'One bounded observation per arm. Any observed benefit is reported locally; no significance or scale claim.',
            'promotion':'No production change is authorized by this report.'})
    output=result/'comparison.json'
    with output.open('x') as stream:
        json.dump(report,stream,ensure_ascii=False,indent=2)
        stream.write('\n')
    print(output)


if __name__=='__main__':
    main()
