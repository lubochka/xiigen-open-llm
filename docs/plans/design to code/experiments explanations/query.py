#!/usr/bin/env python3
"""Offline retrieval: five additional examples, P02 reserve, and preserved history.

python query.py RD01
python query.py KDP01 --selection main --json
python query.py --selection main
python query.py P02 --json
No embeddings, model execution or network access are used.
"""
import argparse
import importlib.util
import json
import sys
from pathlib import Path

sys.dont_write_bytecode=True
_spec=importlib.util.spec_from_file_location('more_query_legacy',Path(__file__).with_name('query_legacy.py'))
_base=importlib.util.module_from_spec(_spec);_spec.loader.exec_module(_base)
for _name in ['KINDS','KIND_LABELS','STAGE_LABELS','ALIAS_GROUPS','STOPWORDS','normalize','tokens','flatten','first','list_text','collect_refs','read_rows','prepare','query_terms','variants','term_match','stage_matches','shorten']:
 globals()[_name]=getattr(_base,_name)
FIELD_WEIGHTS=dict(_base.FIELD_WEIGHTS)
FIELD_WEIGHTS.update(next_status=25,next_current_status=23,next_review=15,next_category=17,next_uml=3)
_base.FIELD_WEIGHTS=FIELD_WEIGHTS
SELECTION_LABELS={
 'all':'Все: новые примеры и история',
 'main':'Пять новых подробных примеров',
 'reserve':'Краткий резерв P02',
 'reviewed':'Все шесть записей нового прохода',
 'historical':'Карточки без нового разбора',
 'previous_main':'Основные кандидаты предыдущего отбора',
}

def normalize_record(row,kind):
 out=_base.normalize_record(row,kind)
 v=row.get('next_review',{})
 out.update(next_review=v,next_status=v.get('status_ru',''),next_current_status=v.get('current_status_ru',''),
  next_category=v.get('classification',''),next_sources=collect_refs(v,kind) if v else [],
  next_uml=v.get('uml',{}),main_method_improvement=bool(v.get('include_in_main',False)),
  latest_status=v.get('current_status_ru') or out.get('current_status',''))
 if v.get('title_ru'):out['title']=v['title_ru']
 return out

def load_records(directory,kinds=KINDS):
 p=Path(directory);result=[]
 for kind,name,key in [('experiment','mechanisms-more.jsonl',None),('book','book-map.json','records'),('catalog','catalog.jsonl',None),('source','sources.json','sources')]:
  if kind in kinds:result.extend(normalize_record(r,kind) for r in read_rows(p/name,key))
 if 'book' in kinds:
  for q in read_rows(p/'book-quotes.json','records'):
   r=dict(q,id='QUOTE-'+q['id'],title_ru='Книжная опора: '+q['topic_ru'],
    aliases=[q['id'],*q.get('case_ids',[])],plain_explanation_ru=q.get('bridge_ru',''),limitations_ru=q.get('limits_ru',''),
    formula=dict(book=q.get('book_formula_latex',''),project=q.get('project_formula_latex',''),provenance=q.get('formula_provenance','')),
    book_quotes=[q],evidence_notice_ru='Один из пяти новых коротких фрагментов; суммарно11слов. Формула книги и вывод для кода отделены.')
   result.append(normalize_record(r,'book'))
 return result

def selection_matches(row,requested):
 if requested=='all':return True
 if row['kind']!='experiment':return False
 v=row.get('next_review',{})
 if requested=='main':return v.get('include_in_main') is True
 if requested=='reserve':return v.get('classification')=='reserve_training_dose_observation'
 if requested=='reviewed':return bool(v)
 if requested=='historical':return not v
 if requested=='previous_main':return row.get('selection_review',{}).get('include_in_main') is True
 return False

_base.selection_matches=selection_matches

def rank(records,query='',kinds=('experiment','book'),stages=(),limit=10,selection='all'):
 # The previous engine's exact status filter must use the latest selected review.
 if normalize(query)=='не внедрено':
  selected=[dict(r,current_status=r.get('latest_status',r.get('current_status',''))) for r in records]
  return _base.rank(selected,query,kinds,stages,limit,selection)
 return _base.rank(records,query,kinds,stages,limit,selection)

def main(argv=None):
 p=argparse.ArgumentParser(description=__doc__,formatter_class=argparse.RawDescriptionHelpFormatter)
 p.add_argument('query',nargs='*');p.add_argument('--stage',action='append',default=[])
 p.add_argument('--kind',action='append',default=[]);p.add_argument('--catalog',action='store_true');p.add_argument('--sources',action='store_true')
 p.add_argument('--selection',choices=SELECTION_LABELS,default='all')
 p.add_argument('--limit',type=int,default=10);p.add_argument('--json',action='store_true')
 p.add_argument('--data-dir',type=Path,default=Path(__file__).resolve().parent)
 args=p.parse_args(argv)
 if not 1<=args.limit<=500:p.error('--limit: от1до500')
 kinds={v.strip() for x in args.kind for v in x.split(',') if v.strip()} or {'experiment','book'}
 if args.catalog:kinds.add('catalog')
 if args.sources:kinds.add('source')
 if not kinds.issubset(KINDS):p.error('Неизвестный kind')
 stages=[v.strip() for x in args.stage for v in x.split(',') if v.strip()]
 query=' '.join(args.query);records=prepare(load_records(args.data_dir,kinds))
 found,total=rank(records,query,kinds,stages,args.limit,args.selection)
 output=dict(query=query,engine='offline weighted lexical retrieval; no embeddings or generation',scope='additional_examples_round2',selection=args.selection,matched_records=total,returned_records=len(found),results=found)
 if args.json:print(json.dumps(output,ensure_ascii=False,indent=2));return 0
 print('Локальный поиск: '+(query or 'все')+f'. Найдено{total}, показано{len(found)}.\n')
 for i,r in enumerate(found,1):
  print(f"{i}. [{KIND_LABELS[r['kind']]}] {r['id']} — {r['title']}")
  v=r.get('next_review',{})
  if v:
   print('   Новый разбор: '+r['next_status'])
   print('   Текущий статус: '+r['next_current_status'])
   print('   Вывод: '+shorten(v.get('summary_ru',''),900))
   for key in ['current_integration_ru','cost_assessment_ru','historical_aggregate_notice_ru']:
    if v.get(key):print('   '+shorten(v[key],900))
   if r['next_sources']:print('   Источник: '+r['next_sources'][0]['url'])
  elif r.get('selection_review'):
   print('   Предыдущий отбор: '+r['selection_status'])
   print('   '+shorten(r['selection_review'].get('summary_ru',''),650))
  else:print('   '+shorten(r.get('summary',''),650))
  if r.get('result'):print('   Исторические данные: '+shorten(r['result'],450))
  print()
 return 0

if __name__=='__main__':raise SystemExit(main())
