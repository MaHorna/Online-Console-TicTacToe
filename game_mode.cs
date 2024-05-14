using System.Diagnostics.Contracts;

class game_mode
{
    public static int[] player_count = { 2, 3, 10, 2, 2 };

    public static int[] board_size = { 3, 4, 10, 5, 7 };
    public static int[] length_to_win = { 3, 3, 4, 4, 5 };
    public static string[] is_valid_mode = { "2_3_3", "3_4_3", "10_10_4", "2_5_4", "2_7_5" };
    public static char[] valid_marks = { 'X', 'O', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
}