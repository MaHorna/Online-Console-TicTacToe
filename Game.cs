class Game
{
    public int id;
    public int mode;
    char[][]? board;
    public List<Player> players = new List<Player>();
    public Game(int id, int mode)
    {
        this.id = id;
        this.mode = mode;
    }
    public void add_player(Player p)
    {
        players.Add(p);
    }
    public void init_board()
    {
        board = new char[game_mode.board_size[mode]][];
        for (int i = 0; i < game_mode.board_size[mode]; i++)
        {
            board[i] = new char[game_mode.board_size[mode]];
            for (int j = 0; j < game_mode.board_size[mode]; j++)
            {
                board[i][j] = ' ';
            }
        }
    }
    public string[] get_printable_board()
    {
        string[] board_str = new string[(game_mode.board_size[mode]*2)-1];
        for (int i = 0; i < game_mode.board_size[mode]; i++)
        {
            for (int j = 0; j < game_mode.board_size[mode]; j++)
            {
                board_str[i*2] += board[i][j];                
                if (j != game_mode.board_size[mode]-1)
                {
                    board_str[i*2] += "|";
                }
            }
            if (i != game_mode.board_size[mode]-1)
            {
                for (int j = 0; j < game_mode.board_size[mode]; j++)
                {
                    board_str[(i*2)+1] += "─";
                    if (j != game_mode.board_size[mode]-1)
                    {
                        board_str[(i*2)+1] += "┼";
                    }
                }
            }
        }
        return board_str;
    }
    public bool put_mark(int x, int y, char mark)
    {
        if (board != null && board[x][y] == ' ')
        {
            board[x][y] = mark;
            return true;
        }
        return false;
    }
    public char check_win()
    {
        for (int i = 0; i < game_mode.board_size[mode]; i++)
        {
            for (int j = 0; j < game_mode.board_size[mode]; j++)
            {
                if (board == null)
                {
                    break;
                }
                char mark = board[i][j];
                if (mark == ' ') //to stop checking if empty tile won :D
                {
                    mark='?'; //set it to sopmething that wont ever happen
                }
                int max_x = Math.Min(j + game_mode.length_to_win[mode], game_mode.board_size[mode]);
                int max_y = Math.Min(i + game_mode.length_to_win[mode], game_mode.board_size[mode]);
                //horizontal
                int count = 0;
                for (int k = j; k < max_x; k++)
                {
                    if (board[i][k] != mark)
                    {
                        break;
                    }
                    count++;
                    if (count == game_mode.length_to_win[mode])
                    {
                        return mark;
                    }
                }
                count = 0;
                //vertical
                for (int k = i; k < max_y; k++)
                {
                    if (board[k][j] != mark)
                    {
                        break;
                    }
                    count++;
                    if (count == game_mode.length_to_win[mode])
                    {
                        return mark;
                    }
                }
                count = 0;
                //diagonal
                for (int k = 0; k < game_mode.length_to_win[mode]; k++)
                {
                    if (i+k >= game_mode.board_size[mode] || j+k >= game_mode.board_size[mode])
                    {
                        break;
                    }
                    
                    if (board[i+k][j+k] != mark)
                    {
                        break;
                    }
                    count++;
                    if (count == game_mode.length_to_win[mode])
                    {
                        return mark;
                    }
                }
            }
        }
        for (int i = 0; i < game_mode.board_size[mode]; i++)
        {
            for (int j = 0; j < game_mode.board_size[mode]; j++)
            {
                if(board[i][j] == ' ') //if not yet all tiles filled
                {
                    return ' ';
                }
            }
        }
        return '!'; //draw
    }
}