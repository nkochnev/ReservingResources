using System;
using System.Collections.Generic;

namespace ReserveBot.Storage.Tables
{
    public class ResourceEntity  : EntityBase
    {
        public string Name { get; set; }
        
        public string DomainName { get; set; }
        public string Url { get; set; }
        
        public Guid TeamId { get; set; }
        public virtual TeamEntity Team{get; set; }
        
        public ResourceType ResourceType { get; set; }
        
        public virtual ICollection<ReserveEntity> Reserves { get; set; }
    }
}