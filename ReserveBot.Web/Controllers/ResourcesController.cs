using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReserveBot.Storage;
using ReserveBot.Storage.Tables;
using ReserveBot.Web.Models.Accounts;
using ReserveBot.Web.Models.Resources;
using ReserveBot.Web.Models.Teams;

namespace ReserveBot.Web.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly ILogger<ResourcesController> _logger;
        private readonly ReserveBotContext _reserveBotContext;

        public ResourcesController(ILogger<ResourcesController> logger, ReserveBotContext reserveBotContext)
        {
            _logger = logger;
            _reserveBotContext = reserveBotContext;
        }

        public IActionResult Index()
        {
            var resources = _reserveBotContext.Resources.ToList();

            var model = new ResourcesViewModel {Resources = new List<ResourceViewModel>()};

            foreach (var resourceEntity in resources)
            {
                var resourceViewModel = new ResourceViewModel();
                resourceViewModel.Id = resourceEntity.Id;
                resourceViewModel.Name = resourceEntity.Name;
                resourceViewModel.ResourceType = resourceEntity.ResourceType.ToString();
                resourceViewModel.Team = resourceEntity.Team?.Name;
                model.Resources.Add(resourceViewModel);
            }

            return View(model);
        }

        public IActionResult Edit(Guid? id)
        {
            var allTeams = _reserveBotContext.Teams.ToList();
            var resourceTypes = (ResourceType[]) Enum.GetValues(typeof(ResourceType));

            var model = new EditResourceViewModel
            {
                Teams = allTeams.Select(x => ToSelectListItem(x)).ToList(),
                ResourceTypes = resourceTypes.Select(x => new SelectListItem(x.ToString(), x.ToString())).ToList()
            };

            if (!id.HasValue)
            {
                return View(model);
            }

            var resource = _reserveBotContext.Resources.Single(x => x.Id == id.Value);

            model.Id = resource.Id;
            model.Name = resource.Name;
            model.SelectedTeam = resource.TeamId;
            model.SelectedResourceType = resource.ResourceType;

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EditResourceViewModel model)
        {
            var resource = !model.Id.HasValue
                ? new ResourceEntity()
                : _reserveBotContext.Resources.Single(x => x.Id == model.Id);

            resource.Name = model.Name;
            resource.TeamId = model.SelectedTeam;
            resource.ResourceType = model.SelectedResourceType;

            if (!model.Id.HasValue)
            {
                _reserveBotContext.Resources.Add(resource);
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