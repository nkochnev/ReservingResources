module ReserveResource.Storage.Context

open FSharp.Data.Sql

type sql = SqlDataProvider<Common.DatabaseProviderTypes.MSSQLSERVER,
                                 """Server=localhost\sqlexpress;Database=reserve_bot;User Id=reserve_bot;Password=reserve_bot""",
                                 "",
                                 "",
                                 1000, 
                                 true
                                 >