﻿using System;
using System.Collections.Generic;

namespace BangazonAPI.Models
{
    public class TrainingProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int MaxAttendees { get; set; }

        public List<Employee> EmployeesAttending { get; set; } = new List<Employee>();
        

    }
}