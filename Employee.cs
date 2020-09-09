using System;
using System.Collections.Generic;
using System.Text;

namespace readrecordtestapp
{
    public class Employee
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }

        public int DepartmentId { get; set; }
        public decimal Salary { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
