using SuperSimpleTcp;
class ttt_client
{
    static int version = 1;
    public string name;
    public string mode;
    public SimpleTcpClient client;
    public Player player;
    stream_reader reader;
    Game game;
    int state = 0; //0 = not in game, 1 = in game, 2 = your turn
    public ttt_client(SimpleTcpClient created_client, string name, string mode)
    {
        this.mode = mode;
        this.name = name;
        client = created_client;
        client.Events.Connected += Connected;
        client.Events.Disconnected += Disconnected;
        client.Events.DataReceived += DataReceived;
        reader = new stream_reader('|');
        player = new(-1, " ");
        game = new Game(0,0); // Initialize the game field with a non-null value
        client.Connect();
    }

    private void DataReceived(object? sender, DataReceivedEventArgs e)
    {
        reader.AddData(0, System.Text.Encoding.UTF8.GetString(e.Data.Array ?? Array.Empty<byte>(), 0, e.Data.Count));
        string data = reader.GetData(0);
        while (data != string.Empty)
        {
            string[] splited = data.Split(';');
            string command = splited[0];
            string message = splited[1];
            if (command == "id")
            {
                player.set_id(Int32.Parse(message));
            }
            if (command == "loadboard")
            {
                state = 1;
                string[] split = message.Split('_');
                player.mark = split[0][0];
                game = new Game(Int32.Parse(split[1]), Array.IndexOf(game_mode.is_valid_mode, mode)); //create new game instance
                game.init_board();
                string[] board = game.get_printable_board();
                foreach (var str in board)
                {
                    System.Console.WriteLine(str);
                }
            }
            if (command == "setmark")
            {
                string[] split = message.Split('_');
                game.put_mark(Int32.Parse(split[0]), Int32.Parse(split[1]), split[2][0]); //put given mark in your game board instance
                string[] board = game.get_printable_board();
                foreach (var str in board)
                {
                    System.Console.WriteLine(str);
                }
            }
            if (command == "gamewon")
            {
                System.Console.WriteLine("game won by player with mark: " + message[0]);
                System.Console.WriteLine("****************");
                System.Console.WriteLine("press H for help");
                state = 0;
                game = new Game(0,0); //reset game instance
            }
            if (command == "gamedraw")
            {
                System.Console.WriteLine("game draw");
                state = 0;
                game = new Game(0,0); //reset game instance
            }
            if (command == "yourturn")
            {
                if (message == "n")
                {
                    state = 1;
                    System.Console.WriteLine("not your turn anymore");
                }
                else if (message == "y")
                {
                    state = 2;
                    System.Console.WriteLine("its your turn");
                }
            }
            if (command == "player_discon")
            {
                System.Console.WriteLine("player disconnected with mark: " + message[0]);
            }
            if (command == "ping")
            {
                client.Send("pong;1|");
            }
            if (command == "version")
            {
                string action = message.Split('_')[0];
                int server_version = Int32.Parse(message.Split('_')[1]);
                if (action == "stop")
                {
                    System.Console.WriteLine("server version: " + server_version + ", does not match with client version, download newer version please, closing client");
                    Environment.Exit(0);
                }
                else if (action == "cont")
                {
                    System.Console.WriteLine("server version: " + server_version + ", matches with client version");
                }
            }
            if (command == "con_timeout")
            {
                System.Console.WriteLine("connection timeout, client was unreachable for some time, closing client");
                Environment.Exit(0);
            }
            data = reader.GetData(0);
        }
    }

    private void Disconnected(object? sender, ConnectionEventArgs e)
    {
        System.Console.WriteLine("Disconnected");
        reader.RemoveStreamConnection(0);
    }

    private void Connected(object? sender, ConnectionEventArgs e)
    { 
        System.Console.WriteLine("*********Connected************");
        System.Console.WriteLine("press H for help");
        System.Console.WriteLine("*****************************");
        client.Send("name;" + name + "|");
        client.Send("mode;" + mode + "|");
        client.Send("version;" + version + "|");
        reader.AddStreamConnection(0);
    }

    public void client_loop()
    {
        ConsoleKeyInfo key_press = new ConsoleKeyInfo(); // Initialize key_press with a default value
        while (true)
        {
            key_press = new ConsoleKeyInfo(); // Reset key_press
            if (Console.KeyAvailable) // Non-blocking peek
            {
                key_press = Console.ReadKey(true); // Read key
            }
            
            if (key_press.Key == ConsoleKey.Escape) // Exit loop
            {
                break;
            }
            if (key_press.Key == ConsoleKey.T) // Test key
            {
                System.Console.WriteLine("client running");
                System.Console.WriteLine("name: " + name);
                System.Console.WriteLine("mode: " + mode);
                System.Console.WriteLine("player id: " + player.id);
            }
            if (key_press.Key == ConsoleKey.N) // new game
            {
                client.Send("new;n|");
            }
            if (key_press.Key == ConsoleKey.B) // new game
            {
                client.Send("nonew;b|");
            }
            if (key_press.Key == ConsoleKey.M) // set mode
            {
                System.Console.WriteLine("set mode, type mode name and press enter:");
                System.Console.Write("valid modes: ");
                foreach (var mode in game_mode.is_valid_mode)
                {
                    System.Console.Write(mode + " ");
                }
                System.Console.WriteLine();
                System.Console.Write("my new mode: ");
                var tmp = Console.ReadLine();
                if (Array.IndexOf(game_mode.is_valid_mode, tmp) == -1)
                {
                    System.Console.WriteLine("invalid mode, setting to 2_3_3 mode");
                    tmp = "2_3_3";
                    client.Send("mode;" + tmp+"|");
                }
                System.Console.WriteLine("mode set to: " + tmp);
                if (tmp != null)
                {
                    mode = tmp;
                }
                client.Send("mode;" + tmp+ "|");
            }
            if (key_press.Key == ConsoleKey.P && (state != 0)) // put mark if ingame
            {
                if (state != 2)
                {
                    System.Console.WriteLine("not your turn");
                    continue;
                }
                System.Console.WriteLine("put mark, type *x y* and press enter:");
                var tmp = Console.ReadLine();
                tmp = tmp.Replace(" ", "_");
                client.Send("putmark;"+game.id+"_" +tmp+"_" + player.mark+"|");
            }
            if (key_press.Key == ConsoleKey.H) // help
            {
                System.Console.WriteLine("*******************************");
                System.Console.WriteLine("T - test");
                System.Console.WriteLine("N - new game - matchmaking");
                System.Console.WriteLine("B - cancels matchmaking");
                System.Console.WriteLine("M - set mode - type mode name and press enter");
                System.Console.WriteLine("P - put mark - in game only \"1 2\" puts mark in first column ,second row");
                System.Console.WriteLine("H - help");
                System.Console.WriteLine("ESC - exit");
                System.Console.WriteLine("*******************************");
            }

        }
    }
}