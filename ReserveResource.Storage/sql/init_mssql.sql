drop table IF EXISTS AccountInTeam
drop table IF EXISTS Reserves
drop table IF EXISTS Resources
drop table IF EXISTS Teams
drop table IF EXISTS Accounts

CREATE TABLE Teams (
    Id uniqueidentifier DEFAULT NEWSEQUENTIALID(),
    Name varchar(1000) not null,
    PRIMARY KEY (id)
);

CREATE TABLE Accounts (
    Id uniqueidentifier DEFAULT NEWSEQUENTIALID(),
    Name varchar(1000) not null,
    TelegramLogin varchar(1000) not null,
    PRIMARY KEY (id)
);

CREATE TABLE AccountInTeam (
     AccountId uniqueidentifier not null FOREIGN KEY REFERENCES Accounts(Id),
     TeamId uniqueidentifier not null FOREIGN KEY REFERENCES Teams(Id),
     PRIMARY KEY (AccountId, TeamId)
);

CREATE TABLE Resources (
     Id uniqueidentifier DEFAULT NEWSEQUENTIALID(),
     Name varchar(1000) not null,
     TeamId uniqueidentifier not null FOREIGN KEY REFERENCES Teams(Id),
     ResourceType int not null,
     DomainName varchar(1000) null,
     Url varchar(1000) null,
     PRIMARY KEY (id)
);

CREATE TABLE Reserves (
     Id uniqueidentifier DEFAULT NEWSEQUENTIALID(),
     AccountId uniqueidentifier not null FOREIGN KEY REFERENCES Accounts(Id),
     ResourceId uniqueidentifier not null FOREIGN KEY REFERENCES Resources(Id),
     FromDate datetime not null,
     ExpiresIn datetime not null,
	 Released datetime null,
     PRIMARY KEY (id)
);

insert into Teams (Name)
values ('ГосОблако')

insert into Accounts (Name, TelegramLogin)
values ('Николай Кочнев','nkochnev')

declare @teamId uniqueidentifier;
set @teamId = (select Id from Teams)

declare @accountId uniqueidentifier;
set @accountId = (select Id from Accounts)

insert into AccountInTeam (AccountId, TeamId)
values (@accountId, @teamId)

-- 1 - VM
-- 2 - Site 
-- 3 - Org

insert into Resources (Name, TeamId, ResourceType, DomainName, Url)
values 
	('GCloud:7777', @teamId, 2, null,'http://GCloud:7777'),
	('GCloud:8888', @teamId, 2, null,'http://GCloud:8888'),
	('GCloud:9999', @teamId, 2, null,'http://GCloud:9999'),
	('vm-fms-04', @teamId, 1, 'vm-fms-04',null),
	('Святые отельеры', @teamId, 3, null, null)