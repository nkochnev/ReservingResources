using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReserveBot.Storage.Tables;

namespace ReserveBot.Web.Models.Resources
{
    public class EditResourceViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public List<SelectListItem> ResourceTypes { get; set; }
        public ResourceType SelectedResourceType { get; set; }
        public List<SelectListItem> Teams { get; set; }
        public Guid SelectedTeam { get; set; }
    }
}