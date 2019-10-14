using System;
using System.Collections.Generic;
using ReserveBot.Storage.Tables;
using ReserveBot.Web.Models.Teams;

namespace ReserveBot.Web.Models.Resources
{
    public class ResourceViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ResourceType { get; set; }
        public string Team { get; set; }
    }
}