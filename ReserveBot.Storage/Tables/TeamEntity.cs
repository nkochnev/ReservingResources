using System.Collections.Generic;

namespace ReserveBot.Storage.Tables
{
    public class TeamEntity  : EntityBase
    {
        private ICollection<ResourceEntity> _resources;
        
        public string Name { get; set; }
        public virtual ICollection<ResourceEntity> Resources => _resources ?? (_resources = new List<ResourceEntity>());
    }
}