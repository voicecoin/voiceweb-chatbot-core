﻿using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.DomainModels;
using Core.Bundle;

namespace Core.Account
{
    public class Hooks : IDbInitializer
    {
        public int Priority => 1000;

        public void Load(IHostingEnvironment env, CoreDbContext dc)
        {
            if (dc.Table<BundleEntity>().Any(x => x.EntityName == "User")) return;

            var dm = new DomainModel<BundleEntity>(dc, new BundleEntity { Name = "User Profile", EntityName = "User" });
            dm.Add(dc);
        }
    }
}