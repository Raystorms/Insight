using Mirror;
using System.Collections.Generic;

namespace Insight
{
    public enum MsgId : short
    {
        Error = -1,
        Empty,
        Status,

        Login,
        LoginResponse,

        //Chat
        Chat,
        JoinChatChannel,
        LeaveChatChannel,

        //GameManager
        RegisterSpawner,
        RegisterGame,
        GameList,
        JoinGame,
        LeaveGame,

        //ProcessSpawner
        RequestSpawn,
        KillSpawn,

        //MatchMaking
        StartMatchMaking,
        StopMatchMaking,
        
        ChangeServers,
        
        //Health Management Msgs
        SpawnerStatus,
    }

    public class ErrorMsg : MessageBase
    {
        public string Text;
        public bool CauseDisconnect;
    }

    public class EmptyMsg : MessageBase { }

    public class StatusMsg : MessageBase
    {
        public string Text;
    }

    public class PropertiesMsg : MessageBase
    {
        public string SceneName;
        public string GameType;
        public int MaxPlayers;
        public bool IsPublic;
        public string GamePassword;
    }

    public class LoginMsg : MessageBase
    {
        public string AccountName;
        public string AccountPassword;
    }

    public class LoginResponseMsg : MessageBase
    {
        public bool Authenticated;
        public string UniqueID;
    }

    public class ChatMsg : MessageBase
    {
        public short Channel; //0 for global
        public string Origin; //This could be controlled by the server.
        public string Target; //Used for private chat
        public string Data;
    }

    public class JoinChatChannelMsg : MessageBase
    {
        public short Channel;
        public string ChannelPassword;
    }

    public class LeaveChatChannelMsg : MessageBase
    {
        public short Channel;
    }

    public class RegisterSpawnerMsg : MessageBase
    {
        public string UniqueID; //Guid
        public int MaxThreads;
    }

    public class RegisterGameMsg : MessageBase
    {
        public string UniqueID; //Guid
        public string NetworkAddress;
        public ushort NetworkPort;
        public string SceneName;
        public int MaxPlayers;
        public int CurrentPlayers;
    }

    public class RequestSpawnMsg : MessageBase
    {
        public string ProcessAlias;
        public string SceneName;

        //Only valid in the reply from the spawner. So it should be moved to another message
        public string UniqueID; //Guid
        public string NetworkAddress; 
    }

    public class KillSpawnMsg : MessageBase
    {
        public string UniqueID; //Guid
    }

    public class StartMatchMakingMsg : MessageBase
    {
        public string PlayListName;
    }

    public class StopMatchMakingMsg : MessageBase
    {

    }

    public class GameListMsg : MessageBase
    {
        public GameContainer[] gamesArray;

        public void Load(List<GameContainer> gamesList)
        {
            gamesArray = gamesList.ToArray();
        }
    }

    public class JoinGamMsg : MessageBase
    {
        public string UniqueID;
    }

    //Used to tell a player to connect to a new game server
    public class ChangeServerMsg : MessageBase
    {
        public string NetworkAddress;
        public ushort NetworkPort;
        public string SceneName;
    }

    //Updates the MasterSpawner with current status
    public class SpawnerStatusMsg : MessageBase
    {
        public int CurrentThreads;
    }
}
