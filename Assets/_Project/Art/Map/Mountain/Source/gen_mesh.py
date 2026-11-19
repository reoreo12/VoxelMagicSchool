# 뒷산 바닥 청크 메시 생성기: hf.py 높이장 → 청크별 greedy 메시(npz) + MagicaVoxel .vox + 팔레트 PNG
# 윗면 + 단차면 + 바깥 둘레 1.5 m 단면. 아랫면·안쪽은 만들지 않는다(보이지 않음). 좌표는 청크 왼쪽 아래 모서리 기준 미터(Y 위).
import numpy as np, struct, sys, os, json
import hf
from PIL import Image

S = hf.S
D = 22                      # 단면 두께 22복셀 = 1.47 m
OUT = sys.argv[1] if len(sys.argv) > 1 else 'out'
os.makedirs(OUT, exist_ok=True)

# ---- 팔레트 ----
PAL_ORDER = ['path', 'path_dark', 'path_light', 'outline', 'soil', 'soil_dark', 'moss', 'moss_dark', 'moss_light', 'skirt1', 'skirt2']
PIDX = {n: i + 1 for i, n in enumerate(PAL_ORDER)}
RGB2IDX = {hf.PAL[n]: PIDX[n] for n in PAL_ORDER}
TOPI = np.zeros(hf.TOP.shape[:2], np.uint8)
flat = hf.TOP.reshape(-1, 3)
for rgb, idx in RGB2IDX.items():
    TOPI.reshape(-1)[(flat == np.array(rgb, np.uint8)).all(1)] = idx
assert (TOPI > 0).all(), 'unmapped color'

def depth_color(top_idx, d):
    # d = 맨 위 복셀부터의 깊이(0이 윗면). 0~1은 윗면색(단차면에서 보임), 2~5 짙은 흙, 그 아래 더 어두운 흙
    return np.where(d <= 1, top_idx, np.where(d <= 5, PIDX['skirt1'], PIDX['skirt2'])).astype(np.uint8)

H, M = hf.Hv, hf.MASK
NX, NZ = H.shape

def greedy(g):
    rects = []
    if not g.any(): return rects
    used = np.zeros(g.shape, bool)
    nu, nv = g.shape
    for u, v in zip(*np.nonzero(g)):
        if used[u, v]: continue
        c = g[u, v]
        v2 = v + 1
        while v2 < nv and g[u, v2] == c and not used[u, v2]: v2 += 1
        u2 = u + 1
        while u2 < nu and (g[u2, v:v2] == c).all() and not used[u2, v:v2].any(): u2 += 1
        used[u:u2, v:v2] = True
        rects.append((u, v, u2, v2, int(c)))
    return rects

def build_chunk(name, box):
    x0, z0, x1, z1 = box
    i0 = int(round((x0 - hf.X0) / S)); i1 = int(round((x1 - hf.X0) / S))
    j0 = int(round((z0 - hf.Z0) / S)); j1 = int(round((z1 - hf.Z0) / S))
    open_north = (name == 'B')          # 포획 구역이 붙을 B 북쪽 변은 단면을 만들지 않고 열어 둠
    quads = []   # (verts[4][3], normal(3), color)
    area_top = 0

    def nb(i, j, di, dj):
        ii, jj = i + di, j + dj
        if ii < 0 or jj < 0 or ii >= NX or jj >= NZ or not M[ii, jj]: return None
        return int(H[ii, jj])

    hs = H[i0:i1, j0:j1]
    kmin = int(hs.min()) - D; kmax = int(hs.max())
    # ---- 윗면: 높이 층마다 greedy ----
    for lvl in np.unique(hs):
        g = np.where(hs == lvl, TOPI[i0:i1, j0:j1], 0).astype(np.uint8)
        for u, v, u2, v2, c in greedy(g):
            y = lvl * S; xa, xb = u * S, u2 * S; za, zb = v * S, v2 * S
            quads.append(((xa, y, za), (xa, y, zb), (xb, y, zb), (xb, y, za), 0, 1, 0, c))   # 윗면, 유니티 기준 시계 방향 = 바깥
            area_top += (u2 - u) * (v2 - v)
    # ---- 옆면 ----
    # dirs: (di,dj, 평면축)  -x:(−1,0) +x:(+1,0) -z:(0,−1) +z:(0,+1)
    for di, dj in ((-1, 0), (1, 0), (0, -1), (0, 1)):
        planes = {}
        for i in range(i0, i1):
            for j in range(j0, j1):
                pass
        # 벡터화: 각 열의 노출 구간 [lo, hi) (k 기준: lo+1..hi 복셀)
        ii = np.arange(i0, i1)[:, None]; jj = np.arange(j0, j1)[None, :]
        h = hs
        ni = ii + di; nj = jj + dj
        inside = (ni >= 0) & (ni < NX) & (nj >= 0) & (nj < NZ)
        nm = np.zeros(h.shape, bool); nh = np.zeros(h.shape, np.int32)
        sel = inside & np.ones(h.shape, bool)
        nii = np.clip(ni, 0, NX - 1); njj = np.clip(nj, 0, NZ - 1)
        nm = M[nii, njj] & inside
        nh = np.where(nm, H[nii, njj], 0)
        lo = np.where(nm, nh, h - D)
        expo = h > lo
        if open_north and (di, dj) == (0, 1):
            edge = (jj + 1 >= j1) & np.ones(h.shape, bool)
            expo = expo & ~(edge & ~nm)
        pi_, pj_ = np.nonzero(expo)
        # 평면 키: 법선이 x축이면 i(+ di>0이면 i+1), z축이면 j
        for a, b in zip(pi_, pj_):
            gi = i0 + a; gj = j0 + b
            ks = np.arange(lo[a, b] + 1, h[a, b] + 1)     # 노출 복셀 k
            cols = depth_color(TOPI[gi, gj], h[a, b] - ks)
            key = (gi + (1 if di > 0 else 0)) if di != 0 else (gj + (1 if dj > 0 else 0))
            along = (gj - j0) if di != 0 else (gi - i0)
            planes.setdefault(key, []).append((along, ks, cols))
        for key, items in planes.items():
            n_al = (j1 - j0) if di != 0 else (i1 - i0)
            g = np.zeros((n_al, kmax - kmin + 2), np.uint8)
            for along, ks, cols in items:
                g[along, ks - kmin] = cols
            for u, v, u2, v2, c in greedy(g):
                ya, yb = (v + kmin - 1) * S, (v2 + kmin - 1) * S      # k번째 복셀 = [(k-1)S, kS]
                if di != 0:
                    x = (key - i0) * S; za, zb = u * S, u2 * S
                    if di < 0:   # 법선 -x : 바깥에서 볼 때 시계 방향
                        quads.append(((x, ya, za), (x, yb, za), (x, yb, zb), (x, ya, zb), -1, 0, 0, c))
                    else:
                        quads.append(((x, ya, za), (x, ya, zb), (x, yb, zb), (x, yb, za), 1, 0, 0, c))
                else:
                    z = (key - j0) * S; xa, xb = u * S, u2 * S
                    if dj < 0:   # 법선 -z
                        quads.append(((xa, ya, z), (xb, ya, z), (xb, yb, z), (xa, yb, z), 0, 0, -1, c))
                    else:
                        quads.append(((xa, ya, z), (xa, yb, z), (xb, yb, z), (xb, ya, z), 0, 0, 1, c))
    # ---- 배열화 ----
    Vq = np.array([q[:4] for q in quads], np.float32)
    Nq = np.array([q[4:7] for q in quads], np.float32)
    cr = np.cross(Vq[:, 1] - Vq[:, 0], Vq[:, 2] - Vq[:, 0])
    bad = (cr * Nq).sum(1) < 0
    Vq[bad] = Vq[bad][:, [0, 3, 2, 1]]          # 유니티 시계 방향 = 바깥(법선) 이 되도록 순서 보정
    V = Vq.reshape(-1, 3)
    N = np.repeat(np.array([q[4:7] for q in quads], np.float32), 4, axis=0)
    C = np.repeat(np.array([q[7] for q in quads], np.int32), 4)
    np.savez_compressed(f'{OUT}/{name}.npz', V=V, N=N, C=C, palette_n=len(PAL_ORDER))
    # ---- .vox ----
    write_vox(name, i0, i1, j0, j1, hs)
    return dict(name=name, quads=len(quads), tris=len(quads) * 2, area_top_vox=int(area_top), expected=(i1 - i0) * (j1 - j0),
                ymin=float(V[:, 1].min()), ymax=float(V[:, 1].max()), size=[x1 - x0, z1 - z0])

def write_vox(name, i0, i1, j0, j1, hs):
    # MagicaVoxel은 Z 위: (x, y=월드 z, z=높이). k 범위를 0부터로 평행이동
    kmin = int(hs.min()) - D + 1; kmax = int(hs.max())
    sx, sy, sz = i1 - i0, j1 - j0, kmax - kmin + 1
    assert max(sx, sy, sz) <= 256, (name, sx, sy, sz)
    ii, jj = np.meshgrid(np.arange(sx), np.arange(sy), indexing='ij')
    chunks_xyzi = []
    topi = TOPI[i0:i1, j0:j1]
    for d in range(D):
        k = hs - d
        col = depth_color(topi, np.full(hs.shape, d))
        xyz = np.stack([ii.ravel(), jj.ravel(), (k - kmin).ravel(), col.ravel()], 1).astype(np.uint8)
        chunks_xyzi.append(xyz)
    xyzi = np.concatenate(chunks_xyzi)
    pal = np.zeros((256, 4), np.uint8); pal[:, 3] = 255
    for n, i in PIDX.items(): pal[i - 1, :3] = hf.PAL[n]
    def chunk(tag, content, children=b''):
        return tag + struct.pack('<ii', len(content), len(children)) + content + children
    size = chunk(b'SIZE', struct.pack('<iii', sx, sy, sz))
    xy = chunk(b'XYZI', struct.pack('<i', len(xyzi)) + xyzi.tobytes())
    rgba = chunk(b'RGBA', pal.tobytes())
    main = chunk(b'MAIN', b'', size + xy + rgba)
    with open(f'{OUT}/{name}.vox', 'wb') as f: f.write(b'VOX ' + struct.pack('<i', 150) + main)

if __name__ == '__main__':
    stats = []
    for name, box in hf.CHUNKS.items():
        st = build_chunk(name, box); stats.append(st); print(st, flush=True)
    # 팔레트 PNG: 256x1, 칸 번호 n → 색 PAL_ORDER[n-1]
    pal = np.zeros((1, 256, 3), np.uint8)
    for n, i in PIDX.items(): pal[0, i, :] = hf.PAL[n]        # 칸 i 의 중심 u = (i+0.5)/256
    Image.fromarray(pal).save(f'{OUT}/Mountain_palette.png')
    json.dump(dict(stats=stats, palette=PAL_ORDER, voxel=S, D=D, chunks=hf.CHUNKS), open(f'{OUT}/stats.json', 'w'), indent=1)
