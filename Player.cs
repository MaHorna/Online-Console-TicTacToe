class Player
{
    public int id;
    public int mode = 0;
    
    public string ip = "";
    public string name = "";
    public char mark = ' ';
    public Player(int id, string ip)
    {
        this.id = id;
        this.ip = ip;
    }
    public void set_id(int id)
    {
        this.id = id;
    }
    public void set_ip(string ip)
    {
        this.ip = ip;
    }
}