﻿using System;
using Dapper;
using MicroOrm.Dapper.Repositories.Tests.Classes;
using MicroOrm.Dapper.Repositories.Tests.DbContexts;

namespace MicroOrm.Dapper.Repositories.Tests.DatabaseFixture
{
    public class MsSqlDatabaseFixture : IDisposable
    {
        private const string DbName = "test_micro_orm";

        public MsSqlDatabaseFixture()
        {
            var connString = "Server=(local);Initial Catalog=master;Integrated Security=True";

            if (Environments.IsAppVeyor)
                connString = "Server=(local)\\SQL2016;Database=master;User ID=sa;Password=Password12!";

            Db = new MSSqlDbContext(connString);

            InitDb();
        }

        public MSSqlDbContext Db { get; }

        public void Dispose()
        {
            Db.Connection.Execute($"USE master; DROP DATABASE {DbName}");
            Db.Dispose();
        }

        private void InitDb()
        {
            Db.Connection.Execute($"IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = '{DbName}') CREATE DATABASE [{DbName}];");
            Db.Connection.Execute($"USE [{DbName}]");
            Action<string> dropTable = name => Db.Connection.Execute($@"IF OBJECT_ID('{name}', 'U') IS NOT NULL DROP TABLE [{name}]; ");
            dropTable("Users");
            dropTable("Cars");
            dropTable("Addresses");
            dropTable("Cities");
            dropTable("Reports");

            Db.Connection.Execute(@"CREATE TABLE Users (Id int IDENTITY(1,1) not null, Name varchar(256) not null, AddressId int not null, Deleted bit not null, UpdatedAt datetime2,  PRIMARY KEY (Id))");
            Db.Connection.Execute(@"CREATE TABLE Cars (Id int IDENTITY(1,1) not null, Name varchar(256) not null, UserId int not null, Status int not null, Data binary(16) null, PRIMARY KEY (Id))");

            Db.Connection.Execute(@"CREATE TABLE Addresses (Id int IDENTITY(1,1) not null, Street varchar(256) not null, CityId varchar(256) not null,  PRIMARY KEY (Id))");
            Db.Connection.Execute(@"CREATE TABLE Cities (Identifier varchar(256) not null, Name varchar(256) not null)");
            Db.Connection.Execute(@"CREATE TABLE Reports (Id int not null, AnotherId int not null, UserId int not null,  PRIMARY KEY (Id, AnotherId))");

            Db.Address.Insert(new Address { Street = "Street0", CityId = "MSK" });
            Db.Cities.Insert(new City { Identifier = "MSK", Name = "Moscow" });

            for (var i = 0; i < 10; i++)
                Db.Users.Insert(new User
                {
                    Name = $"TestName{i}",
                    AddressId = 1,
                });

            Db.Users.Insert(new User { Name = "TestName0" });
            Db.Cars.Insert(new Car { Name = "TestCar0", UserId = 1 });
        }
    }
}