import bpy, json, os, math
ROOT = r'C:\Unity_Project\NCAI\VoxelMagicSchool'
OUT = r'C:\Users\reo36\Downloads\NCAI_수업자료\999999_프로젝트\Voxel_Magical_Game\Assets\Object\Gather_Items\3D\Voxel_Gather_Items\fbx'
DATA = ROOT + r'\Claude outputs\gather_top_quads.json'
PALF = ROOT + r'\Claude outputs\gather_palette.json'
os.makedirs(OUT, exist_ok=True)
data = json.load(open(DATA))
pal = json.load(open(PALF))           # {"1":"#RRGGBB",...}
N = 64                                # 팔레트 고정 64칸 (미사용은 마젠타)
# --- 팔레트 PNG (N x 1 블록, 블록 8x8px) ---
import tempfile; png = os.path.join(tempfile.gettempdir(), 'gather_items_palette.png')
if False and os.path.exists(png):  # 공용 팔레트는 제자리 갱신(슬롯 번호 불변, 빈 슬롯에만 추가)
    print('PNG exists, reuse:', png)
    img = bpy.data.images.load(png, check_existing=True)
else:
    img = bpy.data.images.get('gather_items_palette') or bpy.data.images.new('gather_items_palette', N*8, 8, alpha=True)
    px = []
    cols = []
    for n in range(1, N+1):
        h = pal.get(str(n), '#FF00FF')
        cols.append(tuple(int(h[i:i+2],16)/255.0 for i in (1,3,5)) + (1.0,))
    row = []
    for n in range(N):
        row += list(cols[n]) * 8
    img.pixels = row * 8
    img.filepath_raw = png
    img.file_format = 'PNG'
    img.save()
    img = bpy.data.images.load(png, check_existing=True)
img.colorspace_settings.name = 'sRGB'

def make_mat(name):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    for nd in list(nt.nodes): nt.nodes.remove(nd)
    out = nt.nodes.new('ShaderNodeOutputMaterial')
    bsdf = nt.nodes.new('ShaderNodeBsdfPrincipled')
    bsdf.inputs['Roughness'].default_value = 1.0
    bsdf.inputs['Specular'].default_value = 0.0
    tex = nt.nodes.new('ShaderNodeTexImage')
    tex.image = img
    tex.interpolation = 'Closest'
    nt.links.new(tex.outputs['Color'], bsdf.inputs['Base Color'])
    nt.links.new(bsdf.outputs['BSDF'], out.inputs['Surface'])
    return m

def build(name, d, matname):
    vs = d['vs']; bx, by, bz = d['bbox']
    ox, oy, oz = d.get('origin', [bx/2.0, by/2.0, 0.0])
    verts, faces, uvs = [], [], []
    for ax, s, plane, u, w, du, dw, c in d['quads']:
        def P(a, b):
            if ax == 0: p = (plane, a, b)
            elif ax == 1: p = (a, plane, b)
            else: p = (a, b, plane)
            return ((p[0]-ox)*vs, (p[1]-oy)*vs, (p[2]-oz)*vs)
        pts = [P(u,w), P(u+du,w), P(u+du,w+dw), P(u,w+dw)]
        e1 = [pts[1][i]-pts[0][i] for i in range(3)]
        e2 = [pts[3][i]-pts[0][i] for i in range(3)]
        nrm = [e1[1]*e2[2]-e1[2]*e2[1], e1[2]*e2[0]-e1[0]*e2[2], e1[0]*e2[1]-e1[1]*e2[0]]
        if nrm[ax]*s < 0: pts = [pts[0], pts[3], pts[2], pts[1]]
        i0 = len(verts); verts += pts; faces.append((i0, i0+1, i0+2, i0+3))
        uvs.append((c-0.5)/N)
    me = bpy.data.meshes.new(name); me.from_pydata(verts, [], faces); me.update()
    uv = me.uv_layers.new(name='UVMap')
    for pi, poly in enumerate(me.polygons):
        for li in poly.loop_indices: uv.data[li].uv = (uvs[pi], 0.5)
        poly.use_smooth = False
    me.materials.append(make_mat(matname))
    ob = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(ob)
    return ob

report = []
made = []
for name, d in data.items():
    fname = name
    path = OUT + '\\' + fname + '.fbx'
    if os.path.exists(path):
        k = 2
        while os.path.exists(OUT + '\\%s_v%d.fbx' % (name, k)): k += 1
        fname = '%s_v%d' % (name, k); path = OUT + '\\' + fname + '.fbx'
    matname = 'M_Gather_Items_Glass' if name.startswith(('Moon','Star')) else 'M_Gather_Items'
    ob = build(fname, d, matname)
    made.append(ob)
    for o in bpy.context.view_layer.objects: o.select_set(False)
    ob.select_set(True); bpy.context.view_layer.objects.active = ob
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'MESH'},
        add_leaf_bones=False, path_mode='COPY', embed_textures=True,
        apply_scale_options='FBX_SCALE_UNITS', axis_forward='-Z', axis_up='Y', mesh_smooth_type='OFF')
    dims = ob.dimensions
    bb = [v[:] for v in ob.bound_box]
    report.append('%s: %s tris=%d size=%.3f x %.3f x %.3f m zmin=%.3f' % (name, path, len(ob.data.polygons)*2, dims.x, dims.y, dims.z, min(b[2] for b in bb)))
for ob in made:
    bpy.data.objects.remove(ob, do_unlink=True)
print('\n'.join(report))
