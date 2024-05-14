using SuperSimpleTcp;
class Main_app
{
    static void Main(string[] args)
    {
        bool server_set = false;
        bool client_set = false;
        ttt_server? ttt_s = null;
        ttt_client? ttt_c = null;
        if (find_argument(args, "s"))
        {
            string ip = "";
            if (find_argument(args, "ip"))
            {
                ip = args[find_argument_index(args, "ip") + 1];      
            }
            else
            {
                System.Console.WriteLine("no ip selected - defaulting to LAN *127.0.0.1*");
                ip = "127.0.0.1";
            }
            ttt_s = new ttt_server(new SimpleTcpServer(ip + ":2020"));
            server_set = true;
        }
        if (find_argument(args, "c"))
        {
            int index = 0;
            string name = "";
            string mode = "";
            string ip = "";

            if (find_argument(args, "n"))
            {
                index = find_argument_index(args, "n");
                name = args[index + 1];      
            }
            else
            {
                System.Console.WriteLine("deafulting name to *guest*");
                name = "guest";
            }

            if (find_argument(args, "mode"))
            {
                index = find_argument_index(args, "mode");
                mode = args[index + 1];
            }
            else
            {
                System.Console.WriteLine("deafulting mode to 2_3_3");
                mode = "2_3_3";
            }

            if (find_argument(args, "ip"))
            {
                index = find_argument_index(args, "ip");
                ip = args[index + 1];      
            }
            else
            {
                System.Console.WriteLine("no ip selected - defaulting to LAN *127.0.0.1*");
                ip = "127.0.0.1";
            }

            ttt_c = new ttt_client(new SimpleTcpClient(ip + ":2020"), name, mode);
            client_set = true;       
        }

        if (find_argument(args, "h"))
        {
            System.Console.WriteLine("s - server application");
            System.Console.WriteLine("c - client application");
            System.Console.WriteLine();
            System.Console.WriteLine("arguments for client application");
            System.Console.WriteLine("ip ******** - ip address of server, will default to LAN if not set");
            System.Console.WriteLine("n ********* - name of player, please dont use spaces or words/characters that are arguments");
            System.Console.WriteLine("mode ******* - initial game mode selection for client application, default is 2_3_3 mode");
            System.Console.WriteLine();

            Environment.Exit(0);
        }
        if ((!server_set && !client_set)||( server_set && client_set))
        {
            //server and/nor client set
            Environment.Exit(0);
        }
        
        if (server_set && ttt_s != null)
        {
            ttt_s.server_loop();
        }
        
        if (client_set && ttt_c != null)
        {   
            ttt_c.client_loop();
        }
    }

    static bool find_argument(string[] args, string arg)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == arg)
            {
                return true;
            }
        }
        return false;
    }
    static int find_argument_index(string[] args, string arg)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == arg)
            {
                return i;
            }
        }
        return -1;
    }
}