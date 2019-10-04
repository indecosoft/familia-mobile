using System;
using SQLite;

namespace Familia.DataModels
{
    public class ConversationsRecords
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Room { get; set; }
        public string Message { get; set; }
        /// <summary>
        /// 0 - personal message, 1 - friend message
        /// </summary>
        public int MessageType { get; set; }
        public DateTime MessageDateTime { get; set; }
    }
}
