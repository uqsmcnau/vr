using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Monitor : MonoBehaviour
{
    public String run_id;

    public Camera cam;
    public SteamVR_Behaviour_Pose leftcontrollerPose;
    public SteamVR_Behaviour_Pose rightcontrollerPose;
    public WordEmbeddingModel wem;

    public SteamVR_Input_Sources LefthandType;
    public SteamVR_Input_Sources RighthandType;
    public SteamVR_Action_Boolean grabAction;
    public SteamVR_Action_Boolean zoomInAction;
    public SteamVR_Action_Boolean zoomOutAction;
    public SteamVR_Action_Boolean teleportAction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DateTime aDate = DateTime.Now;
        WordEmbedding Target = wem.getTarget();
        GameObject[] options = wem.getOptions();

        String outputString = "";

        Debug.Log(wem.word + "-" + wem.number_of_neighbours);
        if (options != null)
        {
            for (int i = 0; i < options.Length; i++)
            {
                GameObject option = options[i];
                outputString += "(" + option.GetComponent<Option>().getWord() + "-" + option.transform.position + ")";
            }
            Debug.Log(outputString);

        }
        //InsertRecord(run_id, aDate.ToString("yyyy/MM/ddTHH:mm:ss.fffffff"), cam.transform.position.ToString(), cam.transform.rotation.ToString(),
        //    leftcontrollerPose.transform.position.ToString(), leftcontrollerPose.transform.rotation.ToString(),
        //    rightcontrollerPose.transform.position.ToString(), rightcontrollerPose.transform.rotation.ToString(),
        //    teleportAction.GetState(LefthandType), teleportAction.GetState(RighthandType),
        //    grabAction.GetState(LefthandType), grabAction.GetState(RighthandType),
        //    zoomInAction.GetState(LefthandType), zoomInAction.GetState(RighthandType),
        //    zoomOutAction.GetState(LefthandType), zoomOutAction.GetState(RighthandType),
        //    wem.word, wem.number_of_neighbours, outputString, wem.getZoom());
    }

    private static void InsertRecord(String run_id, String timeString, String cam_pos, String cam_rot, String left_pos, String left_rot,
                                        String right_pos, String right_rot, bool left_teleport, bool left_grab, bool left_zoom_in, bool left_zoom_out,
                                        bool right_teleport, bool right_grab, bool right_zoom_in, bool right_zoom_out, String word, int k, String options, double zoom)
    {
        MySqlConnection conn = new MySqlConnection("Server=localhost; database=wordembeddings; UID=root; password=password");
        conn.Open();

        MySqlCommand comm = conn.CreateCommand();
        comm.CommandText = "INSERT INTO session_monitor(session_string, timestring, camera_position, camera_rotation, left_controller_pos, " +
            "left_controller_rot, right_controller_pos, right_controller_rot, left_teleport, left_grab, left_zoom_in, left_zoom_out, right_teleport, " +
            "right_grab, right_zoom_in, right_zoom_out, word, k, options, zoom) VALUES(@session_string, @timestring, @camera_position, @camera_rotation, " +
            "@left_controller_pos, @left_controller_rot, @right_controller_pos, @right_controller_rot, @left_teleport, @left_grab, @left_zoom_in, " +
            "@left_zoom_out, @right_teleport, @right_grab, @right_zoom_in, @right_zoom_out, @word, @k, @options, @zoom)";
        comm.Parameters.AddWithValue("@session_string", run_id);
        comm.Parameters.AddWithValue("@timestring", timeString);
        comm.Parameters.AddWithValue("@camera_position", cam_pos);
        comm.Parameters.AddWithValue("@camera_rotation", cam_rot);
        comm.Parameters.AddWithValue("@left_controller_pos", left_pos);
        comm.Parameters.AddWithValue("@left_controller_rot", left_rot);
        comm.Parameters.AddWithValue("@right_controller_pos", right_pos);
        comm.Parameters.AddWithValue("@right_controller_rot", right_rot);
        comm.Parameters.AddWithValue("@left_teleport", left_teleport);
        comm.Parameters.AddWithValue("@left_grab", left_grab);
        comm.Parameters.AddWithValue("@left_zoom_in", left_zoom_in);
        comm.Parameters.AddWithValue("@left_zoom_out", left_zoom_out);
        comm.Parameters.AddWithValue("@right_teleport", right_teleport);
        comm.Parameters.AddWithValue("@right_grab", right_grab);
        comm.Parameters.AddWithValue("@right_zoom_in", right_zoom_in);
        comm.Parameters.AddWithValue("@right_zoom_out", right_zoom_out);
        comm.Parameters.AddWithValue("@word", word);
        comm.Parameters.AddWithValue("@k", k);
        comm.Parameters.AddWithValue("@options", options);
        comm.Parameters.AddWithValue("@zoom", zoom);
        comm.ExecuteNonQuery();

        conn.Close();
    }
}
