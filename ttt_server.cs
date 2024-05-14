using System.Data.Common;
using System.Formats.Asn1;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml;
using SuperSimpleTcp; 
using System.Timers;
class ttt_server
{
    static int version = 1;
    private System.Timers.Timer disconnect_timer;
    public SimpleTcpServer server;
    List<Game> games;
    int last_game_index;
    List<Player> players;
    int last_player_index;
    stream_reader reader;
    List<Player> waiting_players;

    List<string> ping_sent_to_ips = new List<string>();
    bool check_ips = false;
    private void CheckDisconnected(Object? source, ElapsedEventArgs e)
    {
        if (check_ips == false)
        {
            for (int i = 0; i < players.Count; i++)
            {
                server.Send(players[i].ip, "ping;|");
                ping_sent_to_ips.Add(players[i].ip);
            }
            check_ips = true; //check ips in next checkdisconnect call 
        }
        else 
        {
            for (int i = 0; i < ping_sent_to_ips.Count; i++)
            {
                for (int j = 0; j < players.Count; j++)
                {
                    if (ping_sent_to_ips[i] == players[j].ip)
                    {
                        System.Console.WriteLine("killing ghost connection: " + ping_sent_to_ips[i]);
                        ManageDisconnect(ping_sent_to_ips[i]);
                        break;
                    }
                }
            }
            ping_sent_to_ips.Clear();
            check_ips = false;
        }
    }
    public ttt_server(SimpleTcpServer created_server)
    {
        server = created_server;
        games = new List<Game>(); 
        players = new List<Player>();
        waiting_players = new List<Player>(); //list of id and mode of users waiting to be connected
        last_game_index = 0;
        last_player_index = 0;
        server.Events.ClientConnected += ClientConnected;
        server.Events.ClientDisconnected += ClientDisconnected;
        server.Events.DataReceived += DataReceived;
        reader = new stream_reader('|');
        server.Start();
        disconnect_timer = new System.Timers.Timer(5000);
        disconnect_timer.Elapsed += CheckDisconnected;
        disconnect_timer.AutoReset = true;
        disconnect_timer.Enabled = true;
        
    }

    private int get_id_from_ip(string ip)
    {
        foreach (var player in players)
        {
            if (player.ip == ip)
            {
                return player.id;
            }
        }
        return -1;
    }
    private int get_waiting_index_from_id(int id)
    {
        for (int i = 0; i < waiting_players.Count; i++)
        {
            if (waiting_players[i].id == id)
            {
                return i;
            }
        }
        return -1;
    }
    private int get_player_index_from_id(List<Player> array, int id)
    {
        for (int i = 0; i < array.Count; i++)
        {
            if (array[i].id == id)
            {
                return i;
            }
        }
        return -1;
    }
    private void DataReceived(object? sender, DataReceivedEventArgs e)
    {
        int id = get_id_from_ip(e.IpPort);
        reader.AddData(id, System.Text.Encoding.UTF8.GetString(e.Data.Array ?? Array.Empty<byte>(), 0, e.Data.Count));
        string data = reader.GetData(id);
        while (data != string.Empty)
        {
            
            string[] splited = data.Split(';');
            string command = splited[0];
            string message = splited[1];
            if (command != "pong")
            {
                System.Console.WriteLine("SRRD: " + data);
            }
            
            if (command == "name")
            {
                players[id].name = message;
            }
            if (command == "mode")
            {
                players[id].mode = Array.IndexOf(game_mode.is_valid_mode, message);
            }
            if (command == "new")
            {
                if (get_player_index_from_id(waiting_players, id) != -1) //check if is in waiting list
                {
                    System.Console.WriteLine("player already in waiting list");
                }
                else 
                {   
                    waiting_players.Add(players[get_player_index_from_id(players, id)]);  //add player to waiting list
                    
                    int[] mode_wait_count = new int[game_mode.is_valid_mode.Length];
                    for (int i = 0; i < waiting_players.Count; i++)
                    {
                        mode_wait_count[waiting_players[i].mode]++;
                    }
                    for (int i = 0; i < game_mode.is_valid_mode.Length; i++) //cycle all game counts
                    {   
                        if (mode_wait_count[i] >= game_mode.player_count[i]) //game mode has enough players
                        {
                            System.Console.WriteLine("starting game mode: "+game_mode.is_valid_mode[i]);
                            Game g = new Game(last_game_index, i);
                            last_game_index++;
                            g.init_board();
                            games.Add(g);
                            int player_count = 0;
                            int player_max = game_mode.player_count[i];
                            foreach (var player in players) //cycle all players
                            {
                                if (player.mode == i) //player is waiting for that game mode to start
                                {
                                    char assigned_mark = game_mode.valid_marks[player_count];
                                    player.mark = assigned_mark;
                                    g.add_player(player);
                                    server.Send(player.ip, "loadboard;"+assigned_mark+"_"+g.id+"|");
                                    if (player_count == 0)
                                    {
                                        server.Send(player.ip, "yourturn;y|");
                                    }
                                    
                                    player_count++;
                                    waiting_players.Remove(player);
                                }
                                if (player_count >= player_max) //no need to search for more players
                                {
                                    break;
                                }
                            }
                            break; //no need to cycle more game modes
                        }
                    }
                }
            }
            if (command == "nonew")
            {
                int tmp_index = get_waiting_index_from_id(id);
                if (tmp_index != -1)
                {
                    waiting_players.RemoveAt(tmp_index);
                }
            }
            if (command == "putmark")
            {
                string[] splited_message = message.Split('_');
                int game_id = Int32.Parse(splited_message[0]);
                int x = Int32.Parse(splited_message[1]);
                int y = Int32.Parse(splited_message[2]);
                char mark = splited_message[3][0];
                if(games[game_id].put_mark(x, y, mark) == true) //if you can put mark
                {
                    games[game_id].put_mark(x, y, mark);
                    int player_array_index = 0;
                    foreach (var player in games[game_id].players) //send message to clients to put marks
                    {
                        server.Send(player.ip, "setmark;" + x + "_" + y + "_" + mark + "|");
                        if (id == player.id) //this player is the one who made the move
                        {
                            server.Send(player.ip, "yourturn;n|");
                            if (player_array_index == games[game_id].players.Count-1)
                            {
                                server.Send(games[game_id].players[0].ip, "yourturn;y|");
                            }
                            else
                            {
                                server.Send(games[game_id].players[player_array_index+1].ip, "yourturn;y|");
                            }
                        }
                        player_array_index++;
                    }
                    char winning_mark = games[game_id].check_win();
                    if (winning_mark == '!') //draw
                    {
                        foreach (var player in games[game_id].players) 
                        {
                            server.Send(player.ip, "gamedraw;|");
                        }
                    }
                    else if (winning_mark != ' ')
                    {
                        foreach (var player in games[game_id].players) 
                        {
                            server.Send(player.ip, "gamewon;"+winning_mark+"|");
                        }
                    }
                }
            }
            if (command == "pong")
            {
                for (int i = 0; i < ping_sent_to_ips.Count; i++)
                {
                    if (ping_sent_to_ips[i] == e.IpPort)
                    {
                        ping_sent_to_ips.RemoveAt(i); //remove ip from ping list, remaining pinged ips are ghost connections
                        break;
                    }
                }
            }
            if (command == "version")
            {
                if (Int32.Parse(message) == version) 
                {
                    server.Send(e.IpPort, "version;cont_" + version + "|");
                }
                else
                {
                    server.Send(e.IpPort, "version;stop_"+version+"|");
                }
            }
            data = reader.GetData(id);
        }
    }
    void remove_player_from_game(int id)
    {
        for (int i = 0; i < games.Count; i++)
        {
            for (int j = 0; j < games[i].players.Count; j++)
            {
                if (games[i].players[j].id == id)
                {
                    if (j == games[i].players.Count-1)
                    {
                        server.Send(games[i].players[0].ip, "yourturn;y|");
                    }
                    else
                    {
                        server.Send(games[i].players[j+1].ip, "yourturn;y|");
                    }
                    char disconnected_mark = games[i].players[j].mark;
                    games[i].players.RemoveAt(j);
                    foreach (var player in games[i].players) 
                    {
                        server.Send(player.ip, "player_discon;"+disconnected_mark+"|");
                    }
                    if (games[i].players.Count == 1)
                    {
                        foreach (var player in games[i].players) 
                        {
                            server.Send(player.ip, "gamewon;"+games[i].players[0].mark+"|");
                        }
                        games.RemoveAt(i);
                    }
                }
            }
        }
    }
    private void ClientDisconnected(object? sender, ConnectionEventArgs e)
    {
        ManageDisconnect(e.IpPort);
    }
    private void ManageDisconnect(string ipport)
    {
        server.Send(ipport, "con_timeout;|");
        int tmp_id = get_id_from_ip(ipport);
        System.Console.WriteLine("player disconnected with id: " + tmp_id + " and port: " + ipport);
        
        remove_player_from_game(tmp_id);
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].id == tmp_id)
            {
                players.RemoveAt(i);
            }
        }
        reader.RemoveStreamConnection(tmp_id);
        int tmp_index = get_waiting_index_from_id(tmp_id);
        if (tmp_index != -1)
        {
            waiting_players.RemoveAt(tmp_index);
        }
    }
    private void ClientConnected(object? sender, ConnectionEventArgs e)
    {
        Player p = new Player(last_player_index, e.IpPort);
        server.Send(p.ip, "id;" + p.id+ "|");
        players.Add(p);
        reader.AddStreamConnection(last_player_index);    
        last_player_index++;
        System.Console.WriteLine("player connected with id: " + p.id + " and ip: " + e.IpPort);
    }


    public void server_loop()
    {
        ConsoleKeyInfo key_press = new ConsoleKeyInfo(); // Initialize key_press with a default value
        while (true)
        {            
            if (Console.KeyAvailable) // Non-blocking peek
            {
                key_press = Console.ReadKey(true); //read key press
            }
            else
            {
                key_press = new ConsoleKeyInfo(); // clean key press
            }
            if (key_press.Key == ConsoleKey.Escape) //exit server
            {
                break;
            }
            if (key_press.Key == ConsoleKey.T) //test server
            {
                System.Console.WriteLine("server running");
                System.Console.WriteLine("players: " + players.Count);
                System.Console.WriteLine("games: " + games.Count);
                System.Console.WriteLine("waiting players: " + waiting_players.Count);

            }
            if (key_press.Key == ConsoleKey.V) //verbose test server
            {
                System.Console.WriteLine("****************************");
                System.Console.WriteLine("server running");
                System.Console.WriteLine("players: " + players.Count);
                foreach (var player in players)
                {
                    System.Console.WriteLine("player id: " + player.id + " ip: " + player.ip + " name: " + player.name + " mode: " + player.mode);
                }

                System.Console.WriteLine("games: " + games.Count);
                foreach (var game in games)
                {
                    System.Console.WriteLine("game id: " + game.id + " mode: " + game.mode);
                    System.Console.WriteLine("players: ");
                    foreach (var player in game.players)
                    {
                        System.Console.WriteLine("player id: " + player.id + " ip: " + player.ip + " name: " + player.name + " mode: " + player.mode);
                    }
                }
                System.Console.WriteLine("waiting players: " + waiting_players.Count);
                foreach (var player in waiting_players)
                {
                    System.Console.WriteLine("player id: " + player.id + " ip: " + player.ip + " name: " + player.name + " mode: " + player.mode);
                }
                System.Console.WriteLine("****************************");
            }
        }
    }
}