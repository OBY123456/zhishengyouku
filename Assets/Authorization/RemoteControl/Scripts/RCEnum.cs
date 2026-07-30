using System;

namespace RC
{
    public enum Rotation
    {
        上,
        下,
        左,
        右,
    }

    public enum RCKey
    {
        remote_0_value = 61,
        remote_1_value = 5,
        remote_2_value = 6,
        remote_3_value = 7,
        remote_4_value = 8,
        remote_5_value = 9,
        remote_6_value = 10,
        remote_7_value = 11,
        remote_8_value = 59,
        remote_9_value = 60,

        remote_left_value = 114,
        remote_right_value = 113,
        remote_up_value = 108,
        remote_down_value = 2,

        remote_ok_value = 115,

        remote_power_value = 102,
        remote_mute_value = 232,
        remote_vod_value = 63,
        remote_live_value = 64,
        remote_decVol_value = 116,
        remote_addVol_value = 106,
        remote_play_value = 66,
        remote_pause_value = 65,
        remote_prev_value = 105,
        remote_next_value = 3,

        remote_mouse_value = 103,
        remote_menu_value = 139,
        remote_home_value = 158,
        remote_back_value = 4,
        remote_point_value = 217,
        
        remote_delete_value = 62,
    }

    public static class RCDataControl
    {
        public static event Action<byte> DataEvent;

        public static void RCDataEvent(byte bytes)
        {
            DataEvent?.Invoke(bytes);
        }
    }
}