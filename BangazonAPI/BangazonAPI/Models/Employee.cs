﻿using System;

namespace BangazonAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int DepartmentId { get; set; }
        public bool IsSupervisor { get; set; }

        public Computer CurrentComputer { get; set; } = new Computer();

    }
}