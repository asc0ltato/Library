﻿using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WCFService.Model
{
    [Table("Listgetbooks")]
    public class Listgetbooks
    {
        [Key]
        public int Id { get; set; }
        public int UsersId { get; set; }
        [Column(TypeName = "smalldatetime")]
        public DateTime DateTaken { get; set; }
        [Column(TypeName = "smalldatetime")]
        public DateTime? DateReturn { get; set; }
        public Users Users { get; set; }
        public int SampleId { get; set; }
        public Sample Sample { get; set; }
    }
}