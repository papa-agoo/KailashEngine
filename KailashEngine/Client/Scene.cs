﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KailashEngine.Render;
using KailashEngine.World;
using KailashEngine.World.Model;

namespace KailashEngine.Client
{
    class Scene
    {

        private string _path_mesh;
        private string _path_physics;
        private string _path_lights;

        private MatrixStack _MS;



        // Scene Objects
        private UniqueMesh _sLight;
        private UniqueMesh _pLight;
        private WorldLoader _test_scene;
        


        public Scene(string path_mesh, string path_physics, string path_lights)
        {
            _path_mesh = path_mesh;
            _path_physics = path_physics;
            _path_lights = path_lights;
            _MS = new MatrixStack();
        }

        private WorldLoader loadHelper(string filename)
        {
            try
            {
                return new WorldLoader(filename, _path_mesh, _path_physics, _path_lights, _sLight, _pLight);
            }
            catch(Exception e)
            {
                Debug.DebugHelper.logError("[ ERROR ] World File: " + filename, e.Message);
                return null;
            }
            
        }

        public void load()
        {
            // Load standard light objects
            _sLight = DAE_Loader.load(_path_mesh + "sLight.dae").ElementAt(0).Value;
            _pLight = DAE_Loader.load(_path_mesh + "pLight.dae").ElementAt(0).Value;
            

            // Load Scenes
            _test_scene = loadHelper("test_scene");
        }


        public void render(Program program)
        {
            _test_scene.draw(_MS, program);
        }

    }
}
