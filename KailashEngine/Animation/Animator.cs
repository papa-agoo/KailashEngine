﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.Animation
{
    class Animator
    {

        public struct KeyFrame
        {
            // The time this key frame triggers
            public float time;

            // Offset for when a keyframe animates longer than the rest
            public float time_offset;

            // Frame data
            public float data;

            // Bezier interpolation values
            public Vector4 bezier_values;


            public KeyFrame(float time, float data, Vector4 bezier_values)
            {
                this.time = time;
                this.time_offset = 0.0f;

                this.data = data;
                this.bezier_values = bezier_values;               
            }
        };

        //------------------------------------------------------
        // Data
        //------------------------------------------------------

        private string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }

        private float _global_last_frame_time;


        //------------------------------------------------------
        // Key Frame dictionaries
        //------------------------------------------------------

        private Dictionary<float, KeyFrame> _key_frames_location_x;
        private Dictionary<float, KeyFrame> _key_frames_location_y;
        private Dictionary<float, KeyFrame> _key_frames_location_z;

        private Dictionary<float, KeyFrame> _key_frames_rotation_x;
        private Dictionary<float, KeyFrame> _key_frames_rotation_y;
        private Dictionary<float, KeyFrame> _key_frames_rotation_z;

        private Dictionary<float, KeyFrame> _key_frames_scale_x;
        private Dictionary<float, KeyFrame> _key_frames_scale_y;
        private Dictionary<float, KeyFrame> _key_frames_scale_z;

        private List<List<float>> _key_frame_times;


        //------------------------------------------------------
        // Constructor
        //------------------------------------------------------

        public Animator(string id)
        {
            _id = id;
            _global_last_frame_time = 0.0f;
           
            _key_frames_location_x = new Dictionary<float, KeyFrame>();
            _key_frames_location_y = new Dictionary<float, KeyFrame>();
            _key_frames_location_z = new Dictionary<float, KeyFrame>();

            _key_frames_rotation_x = new Dictionary<float, KeyFrame>();
            _key_frames_rotation_y = new Dictionary<float, KeyFrame>();
            _key_frames_rotation_z = new Dictionary<float, KeyFrame>();

            _key_frames_scale_x = new Dictionary<float, KeyFrame>();
            _key_frames_scale_y = new Dictionary<float, KeyFrame>();
            _key_frames_scale_z = new Dictionary<float, KeyFrame>();
        }


        //------------------------------------------------------
        // Build Key Frame Dictionaries
        //------------------------------------------------------

        // Add key frames into dictionaries
        public void addKeyFrame(float time, string action, string channel, float data, Vector4 data_bezier)
        {
            // Decide which Key Frame dictionary to add action to
            Dictionary<float, KeyFrame> temp_dictionary = null;
            switch (action)
            {
                case AnimationHelper.translate:
                    switch (channel)
                    {
                        case "X":
                            temp_dictionary = _key_frames_location_x;
                            break;
                        case "Y":
                            temp_dictionary = _key_frames_location_y;
                            break;
                        case "Z":
                            temp_dictionary = _key_frames_location_z;
                            break;
                    }
                    break;
                case AnimationHelper.rotation_euler:
                    switch (channel)
                    {
                        case "X":
                            temp_dictionary = _key_frames_rotation_x;
                            break;
                        case "Y":
                            temp_dictionary = _key_frames_rotation_y;
                            break;
                        case "Z":
                            temp_dictionary = _key_frames_rotation_z;
                            break;
                    }
                    break;
                case AnimationHelper.scale:
                    switch (channel)
                    {
                        case "X":
                            temp_dictionary = _key_frames_scale_x;
                            break;
                        case "Y":
                            temp_dictionary = _key_frames_scale_y;
                            break;
                        case "Z":
                            temp_dictionary = _key_frames_scale_z;
                            break;
                    }
                    break;
            }

            // Finally, add key frame to dictionary
            KeyFrame temp_key_frame = new KeyFrame(time, data, data_bezier);
            temp_dictionary.Add(time, temp_key_frame);
        }


        // Calculate the last frame for this animator
        public void calcLastFrame()
        {
            _key_frame_times = new List<List<float>>
            {
                _key_frames_location_x.Keys.ToList(),
                _key_frames_location_y.Keys.ToList(),
                _key_frames_location_z.Keys.ToList(),
                _key_frames_rotation_x.Keys.ToList(),
                _key_frames_rotation_y.Keys.ToList(),
                _key_frames_rotation_z.Keys.ToList(),
                _key_frames_scale_x.Keys.ToList(),
                _key_frames_scale_y.Keys.ToList(),
                _key_frames_scale_z.Keys.ToList()
            };

            float last_frame_time = 0;

            foreach (List<float> frames in _key_frame_times)
            {
                last_frame_time = Math.Max(frames.Last(), last_frame_time);
            }

            _global_last_frame_time = last_frame_time;
        }


        //------------------------------------------------------
        // Get Animation Data
        //------------------------------------------------------

        private float bezierInterpolation(KeyFrame previous_frame, KeyFrame next_frame, float current_time)
        {
            BezierCurveCubic temp_bezier = new BezierCurveCubic();
            temp_bezier.StartAnchor = new Vector2(previous_frame.time, previous_frame.data);
            temp_bezier.FirstControlPoint = previous_frame.bezier_values.Zw;
            temp_bezier.SecondControlPoint = next_frame.bezier_values.Xy;
            temp_bezier.EndAnchor = new Vector2(next_frame.time, next_frame.data);

            return temp_bezier.CalculatePoint(current_time).Y;
        }


        private float getData(Dictionary<float, KeyFrame> key_frame_dictionary, float current_time)
        {
            List<float> key_frame_times = key_frame_dictionary.Keys.ToList();
            //key_frame_times.Sort();

            float num_repeats = 1;

            float last_frame_time = key_frame_times.Last();
            last_frame_time = _global_last_frame_time;
            float repeat_multiplier = (num_repeats == -1) ? (float)Math.Floor(current_time / last_frame_time) : Math.Min((float)Math.Floor(current_time / last_frame_time), num_repeats - 1);
            float repeat_frame = repeat_multiplier * last_frame_time;
            float loop_time = current_time - repeat_frame;


            // Get prevous and next frame with interpolation between them
            Vector3 PrevNextInterp = getNearestFrame(key_frame_times.ToArray(), loop_time);

            float output = 0;


            output = key_frame_dictionary[PrevNextInterp.X].data;

            if (PrevNextInterp.Z != -1)
            {
                KeyFrame previous_frame = key_frame_dictionary[PrevNextInterp.X];
                KeyFrame next_frame = key_frame_dictionary[PrevNextInterp.Y];
                float interpolation = PrevNextInterp.Z;

                output = bezierInterpolation(previous_frame, next_frame, interpolation);
            }


            return output;
        }


        public Matrix4 getKeyFrame(float time, int num_repeats)
        {
            Matrix4 temp_matrix = Matrix4.Identity;


            // Set animation actions
            Vector3 translation = new Vector3(
                getData(_key_frames_location_x, time),
                getData(_key_frames_location_y, time),
                getData(_key_frames_location_z, time)
            );

            Vector3 rotation_euler = new Vector3(
                getData(_key_frames_rotation_x, time),
                getData(_key_frames_rotation_y, time),
                getData(_key_frames_rotation_z, time)
            );

            Vector3 scale = new Vector3(
                getData(_key_frames_scale_x, time),
                getData(_key_frames_scale_y, time),
                getData(_key_frames_scale_z, time)
            );


            return EngineHelper.blender2Kailash(translation, rotation_euler, scale);
        }


        // Get frame before and after the submitted time
        private Vector3 getNearestFrame(float[] frame_times, float time)
        {
            float start_frame = frame_times[0];
            float end_frame;

            // If animation hasn't started yet, just hold
            if (time < start_frame)
            {
                return new Vector3(start_frame, start_frame, -1);
            }
            for(int i = 0; i < frame_times.Length; i++)
            {
                if(time == frame_times[i])
                {
                    return new Vector3(time, time, -1.0f);
                }
                else if (time > frame_times[i])
                {
                    start_frame = frame_times[i];
                }
                else if (time < frame_times[i])
                {
                    end_frame = frame_times[i];

                    float interpolation = (time - start_frame) / (end_frame - start_frame);

                    return new Vector3(start_frame, end_frame, interpolation);
                }
            }

            return new Vector3(start_frame, start_frame, -1);
        }

    }
}