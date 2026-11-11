# 봉우리 배치표와 Unity 변환(위치·스케일) 산출. 실행: python layout.py out
import json, sys, numpy as np
import gen_spires as G, gen_apron as A
INST=[  # kind, 방향, 중심x, 중심z, 확대xz, 확대y
 ('C','x',4.5,82.6,.8,1.0),('D','x',12.5,82.8,1.0,1.0),('B','z',18.6,70.0,.85,1.0),('D','z',18.8,77.3,.9,1.1),
 ('C','x',23.5,66.6,.9,1.05),('D','x',31.5,66.8,.85,.9),('B','x',38.5,66.6,1.0,1.05),('D','x',45.0,66.8,.75,1.0),
 ('C','z',50.6,55.0,.8,1.0),('D','z',50.7,62.0,.8,1.1),
 ('A','x',3.5,89.0,1.0,1.15),('B','x',11.5,88.5,1.0,1.2),('A','z',24.5,70.5,.9,1.05),('A','z',25.0,79.0,1.0,1.15),
 ('A','x',30.5,73.0,.9,1.0),('B','x',38.0,73.5,1.0,1.1),('A','x',46.0,73.0,1.1,1.2),
 ('B','z',56.5,52.5,.9,1.05),('A','z',56.8,60.0,1.0,1.1),('A','z',56.5,67.5,.9,1.15)]

def transforms():
    res = []
    cache = {}
    for n, (k, o, cx, cz, sxz, sy) in enumerate(INST):
        key = k + o
        if key not in cache:
            V, N, C, nq, info = G.build_piece(k, 1 if o == 'z' else 0, seed=ord(k)); cache[key] = info['size']
        NU, NV = cache[key]
        y0 = float(A.base_height(cx, cz)[0]) - 0.1
        res.append(dict(name=f"Spire_{k}{o}_{n:02d}", mesh=f"Spire_{k}{o}", pos=[round(cx - NU / 2 * sxz, 3), round(y0, 3), round(cz - NV / 2 * sxz, 3)], scale=[sxz, sy, sxz], row='front' if n < 10 else 'back'))
    for name, (a, b, c, d) in A.CHUNKS.items():
        res.append(dict(name=name, mesh=name, pos=[a, 0, b], scale=[1, 1, 1], row='apron'))
    return res
if __name__ == '__main__':
    json.dump(transforms(), open((sys.argv[1] if len(sys.argv) > 1 else '.') + '/placement.json', 'w'), indent=1, ensure_ascii=False)
