using System.Collections.Generic;

namespace ReserveBot.Storage.Tables
{
    public class AccountEntity  : EntityBase
    {
        public string Name { get; set; }
        public string TelegramLogin { get; set; }
        
        public virtual ICollection<ReserveEntity> Reserves { get; set; }
    }
}