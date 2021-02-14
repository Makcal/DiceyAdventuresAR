import bpy
import math
import os

PROJECT = 'D:\\Mine\\UnityProjects\\Dicey Dungeons AR\\Assets\\'
PATH = 'Models\\CubesForPres\\'
FILE = 'Ground.fbx'

obj = bpy.context.object
obj.data.use_auto_smooth = 1
obj.data.auto_smooth_angle = math.radians(25)
bpy.ops.object.shade_smooth()

bpy.ops.export_scene.fbx(
    object_types={'MESH'},
    filepath=PROJECT + PATH + FILE,
)

#for file in os.listdir(PROJECT + PATH):
#    if FILE not in file:
#        os.remove(PROJECT + PATH + file)
