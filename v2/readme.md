1. put ProceduralSkybox2017_TimeOfDay.shader into Assets/Shader
2. put SkyboxTimeOfDayController.cs into Assets/
3. in hierachy create an empty and name it Skybox
4. add component Skybox Time Of Day Controller
5. Create a new material and name it timeofday_material
6. Set the shader of that material to custom/ProceduralSkybox2017_TimeOfDay
7. Define your low and high clouds to your liking
8. Select your Skybox object in hierarchy and drag your timeofday_material into skybox material
9. drag your sun light object into sun light
10. drag your moon light object into moon light
11. for clouds, create an empty in hierachy and name it CloudShadows
12. add a component projector
13. create a new material and name it cloudshadow_material
14. find a grayscale image of a cloud texture and import to unity and drag to image texture of cloudshadow_material
15. set the shader to legacy shaders/transparent/diffuse
16. drag that cloudshadow_material onto your CloudShadows object
17. position that object over your terrain

18. Right now the cloud shadows is not working as intended.
19. 
