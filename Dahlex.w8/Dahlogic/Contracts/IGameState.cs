using System.Runtime.Serialization;

namespace Dahlex.Logic.Contracts
{
    public interface IGameState
    {
        [DataMember]
        int Level { get; set; }

        [DataMember]
        int MoveCount { get; set; }

        [DataMember]
        int BombCount { get; set; }

        [DataMember]
        int TeleportCount { get; set; }

        [DataMember]
        int GameStatus { get; set; }

        [DataMember]
        int ElapsedTimeInSeconds { get; set; }

        [DataMember]
        string TheBoard { get; set; }

        [DataMember]
        int Mode { get; set; }

        [IgnoreDataMember]
        string Message { get; set; }
    }
}