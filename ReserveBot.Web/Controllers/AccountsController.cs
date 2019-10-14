using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReserveBot.Storage;
using ReserveBot.Storage.Tables;
using ReserveBot.Web.Models;
using ReserveBot.Web.Models.Accounts;
using ReserveBot.Web.Models.Teams;

namespace ReserveBot.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly ReserveBotContext _reserveBotContext;

        public AccountsController(ILogger<AccountsController> logger, ReserveBotContext reserveBotContext)
        {
            _logger = logger;
            _reserveBotContext = reserveBotContext;
        }

        public IActionResult Index()
        {
            var accounts = _reserveBotContext.Accounts.ToList();

            var accountInTeams = _reserveBotContext
                .AccountInTeams
                .Include(x => x.Account)
                .Include(x => x.Team)
                .ToList()
                .ToLookup(x => x.AccountId);

            var model = new AccountsViewModel();
            model.Accounts = new List<AccountViewModel>();

            foreach (var account in accounts)
            {
                var accountViewModel = new AccountViewModel();
                accountViewModel.Id = account.Id;
                accountViewModel.Name = account.Name;
                accountViewModel.TelegramLogin = account.TelegramLogin;

                accountViewModel.Teams = new List<TeamViewModel>();
                var teams = accountInTeams.Contains(account.Id)
                    ? accountInTeams[account.Id]
                    : new List<AccountInTeamEntity>();
                foreach (var teamEntity in teams)
                {
                    accountViewModel.Teams.Add(new TeamViewModel()
                        {Id = teamEntity.Team.Id, Name = teamEntity.Team.Name});
                }

                model.Accounts.Add(accountViewModel);
            }

            return View(model);
        }

        public IActionResult Edit(Guid? id)
        {
            var allTeams = _reserveBotContext.Teams.ToList();

            var model = new EditAccountViewModel();
            if (!id.HasValue)
            {
                model.Teams = allTeams.Select(x => ToSelectListItem(x)).ToList();
                return View(model);
            }

            var account = _reserveBotContext.Accounts.Single(x => x.Id == id.Value);
            var teamIds = _reserveBotContext
                .AccountInTeams
                .Where(x => x.AccountId == id)
                .Select(x => x.TeamId)
                .ToList();

            model.Id = account.Id;
            model.Name = account.Name;
            model.TelegramLogin = account.TelegramLogin;
            model.Teams = new List<SelectListItem>();
            foreach (var teamEntity in allTeams)
            {
                model.Teams.Add(ToSelectListItem(teamEntity, teamIds.Contains(teamEntity.Id)));
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EditAccountViewModel model)
        {
            AccountEntity account;
            if (!model.Id.HasValue)
            {
                account = new AccountEntity();
                _reserveBotContext.Accounts.Add(account);
                _reserveBotContext.SaveChanges();
            }
            else
            {
                account = _reserveBotContext.Accounts.Single(x => x.Id == model.Id);
            }

            account.Name = model.Name;
            account.TelegramLogin = model.TelegramLogin;

            var inTeams = new List<AccountInTeamEntity>();
            _reserveBotContext.AccountInTeams.RemoveRange(
                _reserveBotContext.AccountInTeams.Where(x => x.AccountId == account.Id));
            _reserveBotContext.SaveChanges();

            if (model.Teams != null)
                foreach (var selectListItem in model.Teams.Where(selectListItem => selectListItem.Selected))
                {
                    inTeams.Add(new AccountInTeamEntity()
                        {AccountId = account.Id, TeamId = Guid.Parse(selectListItem.Value)});
                    _reserveBotContext.AccountInTeams.AddRange(inTeams);
                }

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

        private SelectListItem ToSelectListItem(TeamEntity teamEntity, bool selected = false)
        {
            return new SelectListItem() {Text = teamEntity.Name, Value = teamEntity.Id.ToString(), Selected = selected};
        }
    }
}