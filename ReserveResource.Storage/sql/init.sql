--CREATE USER reserve_bot with password 'reserve_bot';
--create database reserve_bot;
--grant all privileges on database reserve_bot to reserve_bot;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS public.Teams (
    Id uuid DEFAULT gen_random_uuid(),
    Name varchar(1000) not null,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS public.Accounts (
    Id uuid DEFAULT gen_random_uuid(),
    Name varchar(1000) not null,
    TelegramLogin varchar(1000) not null,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS public.AccountInTeam (
     AccountId uuid not null,
     TeamId uuid not null,
     PRIMARY KEY (AccountId, TeamId)
);

CREATE TABLE IF NOT EXISTS public.Resources (
     Id uuid DEFAULT gen_random_uuid(),
     Name varchar(1000) not null,
     TeamId uuid not null,
     Type int not null,
     DomainName varchar(1000) null,
     Url varchar(1000) null,
     PRIMARY KEY (id)
);