using System;

namespace ReserveBot.Storage.Tables
{
    public class ReserveEntity  : EntityBase
    {
        public Guid ReservedByAccountId { get; set; }
        public virtual AccountEntity ReservedByAccount { get; set; }
        
        public Guid ResourceId { get; set; }
        public virtual ResourceEntity Resource { get; set; }
        
        public DateTime From { get; set; }
        public DateTime ExpiredIn { get; set; }
        public DateTime? Released { get; set; }
    }
}