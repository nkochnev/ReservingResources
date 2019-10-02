using System;

namespace ReserveBot.Storage.Tables
{
    public class AccountInTeamEntity : EntityBase
    {
        public Guid AccountId { get; set; }
        public virtual AccountEntity Account { get; set; }
        
        public Guid TeamId { get; set; }
        public virtual TeamEntity Team { get; set; }
    }
}