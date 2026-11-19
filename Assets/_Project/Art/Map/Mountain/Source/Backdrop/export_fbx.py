# Blender 3.0 브리지/헤드리스: argv = ['x','--', <npz폴더>, <fbx폴더>, <이름...>]
# npz(V,N,C) → 메시(정점 (bx,by,bz)=(-x,-z,y), 면 순서 [0,3,2,1]) → 팔레트 UV(u=(idx+0.5)/256) → FBX(스케일 1, -Z 앞, Y 위)
# 사용자 장면의 다른 오브젝트는 건드리지 않고, 만든 임시 오브젝트만 지운다. 선택한 오브젝트만 내보낸다.
import bpy, numpy as np, sys, os
a = sys.argv[sys.argv.index('--') + 1:]; src, dst, names = a[0], a[1], a[2:]
os.makedirs(dst, exist_ok=True)
for name in names:
    d = np.load(os.path.join(src, name + '.npz')); V = d['V'].copy(); C = d['C'].copy(); d.close()
    nq = len(V) // 4
    me = bpy.data.meshes.new('MtTmp_' + name)
    me.from_pydata([(-float(x), -float(z), float(y)) for x, y, z in V], [], [[4*i, 4*i+3, 4*i+2, 4*i+1] for i in range(nq)]); me.update()
    uv = me.uv_layers.new(name='UVMap')
    u = ((C[0::4].astype(np.float32) + 0.5) / 256.0)
    for p in me.polygons:
        for li in p.loop_indices: uv.data[li].uv = (float(u[p.index]), 0.5)
        p.use_smooth = False
    mat = bpy.data.materials.new('Mountain_Voxel'); me.materials.append(mat)
    ob = bpy.data.objects.new(name, me)
    col = bpy.data.collections.new('MtExport'); bpy.context.scene.collection.children.link(col); col.objects.link(ob)
    prev = bpy.context.view_layer.active_layer_collection
    bpy.context.view_layer.active_layer_collection = bpy.context.view_layer.layer_collection.children['MtExport']
    bpy.context.view_layer.update()
    w = bpy.context.window_manager.windows[0] if bpy.context.window_manager.windows else None
    ov = {'window': w, 'screen': w.screen, 'scene': bpy.context.scene, 'view_layer': bpy.context.view_layer, 'active_object': ob, 'selected_objects': [ob]} if w else {}
    bpy.ops.export_scene.fbx(ov, filepath=os.path.join(dst, name + '.fbx'), use_active_collection=True, object_types={'MESH'},
        apply_scale_options='FBX_SCALE_ALL', global_scale=1.0, bake_space_transform=True, axis_forward='-Z', axis_up='Y',
        mesh_smooth_type='FACE', add_leaf_bones=False)
    bpy.context.view_layer.active_layer_collection = prev
    bpy.data.objects.remove(ob, do_unlink=True); bpy.data.collections.remove(col); bpy.data.meshes.remove(me); bpy.data.materials.remove(mat)
    print('EXPORTED', name, nq, flush=True)
