using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SirmaSolutionTask.Models
{
    public class EmployeeData
    {
        public int Empld { get; set; }
        public int ProjectId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string file { get; set; }
        public HttpPostedFile fileBase { get; set; }
        public List<Tuple<int, int,int, int>> employeeInfo { get; set; } = null;
        public EmployeeData()
        {
            file = "";
            employeeInfo = new List<Tuple<int, int,int, int>>();
        }
    }
}