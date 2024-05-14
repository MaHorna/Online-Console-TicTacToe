# Online Console Tic Tac Toe 
 Tic Tac Toe via TCP with console interface
 
7year-ish old project finally done.

SuperSimpleTCP for c# library was used

Server can host multiple games, games can be of any size - any number of players, any board size

Basic matchmaking system 




run on linux with:

(server)

dotnet run Project.cs s 

(client)

dotnet run Project.cs c

example of all possible arguments
(client with name GONDOR connecting to server on ip 167.156.21.87, port is set in code to 2020, and mode 2_3_3 - 2 players , 3 board width and height, 3 tiles sequence to win, and show help)

dotnet run Project.cs c n GONDOR ip 167.156.21.87 mode 2_3_3 h 
