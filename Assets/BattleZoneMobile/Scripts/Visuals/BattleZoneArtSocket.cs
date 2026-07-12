using UnityEngine;

namespace BattleZoneMobile
{
    public sealed class BattleZoneArtSocket : MonoBehaviour
    {
        [SerializeField] private string socketId;
        [SerializeField] private string notes;

        public string SocketId => socketId;
        public string Notes => notes;

        public void ConfigureRuntime(string id, string socketNotes = "")
        {
            socketId = id;
            notes = socketNotes;
        }
    }
}
