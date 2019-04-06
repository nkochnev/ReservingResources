module ReserveResource.HardCode

open System
open ReserveResource.Types

let now = DateTime.Now;

let gCloudTeam = {Id = Guid.NewGuid();Name = "GCloud";}
let hotelTeam = {Id = Guid.NewGuid();Name = "Hotel";}

let teamLeadAccount = {Id = Guid.NewGuid(); Name = "TeamLead";TelegramLogin="nkochnev";InTeams = [gCloudTeam; hotelTeam] } 
let middleAccount = {Id = Guid.NewGuid(); Name = "Middle";TelegramLogin="middle";InTeams = [gCloudTeam] }
let juniorAccount = {Id = Guid.NewGuid(); Name = "Junior";TelegramLogin="junior";InTeams = [hotelTeam] }

let accounts() = [teamLeadAccount;middleAccount;juniorAccount]

let gCloud7777 = Site {Id = Guid.NewGuid();Name = "GCloud:7777";Url="http://GCloud:7777"; Team = gCloudTeam}
let gCloud8888 = Site {Id = Guid.NewGuid();Name = "GCloud:8888";Url="http://GCloud:8888"; Team = gCloudTeam}
let gCloud9999 = Site {Id = Guid.NewGuid();Name = "GCloud:9999";Url="http://GCloud:9999"; Team = gCloudTeam}
let gCloudVm = VM {Id = Guid.NewGuid();Name = "GCloud-vm";DomainName = "GCloud-vm"; Team = gCloudTeam}

let hotelVm = VM {Id = Guid.NewGuid();Name = "vm-fms-04";DomainName = "vm-fms-04"; Team = hotelTeam}
let holyHotelersOrganization = Organization {Id = Guid.NewGuid();Name = "Святые отельеры";Team = hotelTeam}

let resources() =  [gCloud7777; gCloud8888; gCloud9999; gCloudVm; hotelVm; holyHotelersOrganization]

let gCloud7777ExpiredReserve = {
    Account = teamLeadAccount;
    Resource = gCloud7777;
    From = now.AddDays(float -10);
    ExpiredIn = now.AddDays(float -1);
    Status = ReservingStatus.Expired
}

let gCloud7777ActiveReserve = {
    Account = teamLeadAccount;
    Resource = gCloud7777;
    From = now.AddHours(float -10);
    ExpiredIn = now.AddDays(float 1);
    Status = ReservingStatus.Active
}

let gCloud9999ActiveReserve = {
    Account = middleAccount;
    Resource = gCloud9999;
    From = now.AddMinutes(float -1);
    ExpiredIn = now.AddDays(float 7);
    Status = ReservingStatus.Active
}
