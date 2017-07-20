﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Core.Bundle;
using Core.Interfaces;

namespace Core.Taxonomy
{
    [Table("Taxonomies")]
    public class TaxonomyEntity : DbRecord, IDbRecord4SqlServer
    {
        [Required]
        public string BundleId { get; set; }

        [MaxLength(512, ErrorMessage = "Description cannot be longer than 512 characters.")]
        public String Description { get; set; }
    }

    [Table("TaxonomyTerms")]
    public class TaxonomyTermEntity : DbRecord, IDbRecord4SqlServer
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Entity Name cannot be longer than 50 characters.")]
        public String Name { get; set; }
        [Required]
        public string TaxonomyId { get; set; }
        public TaxonomyEntity Taxonomy { get; set; }

        /// <summary>
        /// Parent Term, In order to create taxonomy with hierarchy
        /// </summary>
        public string ParentId { get; set; }
    }
}
