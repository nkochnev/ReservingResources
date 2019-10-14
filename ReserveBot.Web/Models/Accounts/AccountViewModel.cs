using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReserveBot.Web.Models.Teams;

namespace ReserveBot.Web.Models.Accounts
{
    public class AccountViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TelegramLogin { get; set; }
        public List<TeamViewModel> Teams { get; set; }
    }

    public class EditAccountViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string TelegramLogin { get; set; }
        public List<SelectListItem> Teams { get; set; }
    }
}