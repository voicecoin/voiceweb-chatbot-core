﻿using Core.Block;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Page
{
    public class DmPage : IDomainModel
    {
        public DmPage()
        {
            Id = Guid.NewGuid().ToString();
            Blocks = new List<DmBlock>();
        }

        public String Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }

        public List<DmBlock> Blocks { get; set; }
    }
}