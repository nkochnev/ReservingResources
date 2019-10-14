using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReserveBot.Storage;
using ReserveBot.Storage.Tables;
using ReserveBot.Web.Models;
using ReserveBot.Web.Models.Teams;

namespace ReserveBot.Web.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ILogger<TeamsController> _logger;
        private readonly ReserveBotContext _reserveBotContext;

        public TeamsController(ILogger<TeamsController> logger, ReserveBotContext reserveBotContext)
        {
            _logger = logger;
            _reserveBotContext = reserveBotContext;
        }

        public IActionResult Index()
        {
            var teams = _reserveBotContext.Teams.ToList();
            var model = new TeamsViewModel();
            model.Teams = teams.Select(x => new TeamViewModel() {Id = x.Id, Name = x.Name}).ToList();
            return View(model);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(TeamViewModel model)
        {
            var team = new TeamEntity() {Name = model.Name};
            _reserveBotContext.Teams.Add(team);
            _reserveBotContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(Guid id)
        {
            var team = _reserveBotContext.Teams.Single(x => x.Id == id);
            _reserveBotContext.Teams.Remove(team);
            _reserveBotContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}