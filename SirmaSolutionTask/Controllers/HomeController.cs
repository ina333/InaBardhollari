using SirmaSolutionTask.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace SirmaSolutionTask.Controllers
{
    public class HomeController : Controller
    {
        EmployeeData employee = new EmployeeData();
        List<EmployeeData> employeeDataList = new List<EmployeeData>();
       
        public ActionResult Index()
        {
            return View(employee); 
        }
        [HttpPost]
        public ActionResult ReadDataFromCSV( HttpPostedFileBase filebase)
        {
            string FilePath = "";

            if (filebase != null)
            {
                if (ValidateFile(filebase.FileName))
                {
                    FilePath = Server.MapPath("~/UploadedFiles/");
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    filebase.SaveAs(FilePath + filebase.FileName);

                    employee.employeeInfo = new List<Tuple<int, int, int>>();
                    employeeDataList = ReadEmployeeInfo(FilePath + filebase.FileName);
                   
                    employee = FindPairsWithLongestCommonWorkDuration(employeeDataList);

                }
                else
                {
                    TempData["messageInfo"] = "You have not uploaded a CSV file document!";
                   return RedirectToAction("Index");
                }
               
            }
            else
            {
                TempData["messageInfo"] = "You have not uploaded any file document!";
                return RedirectToAction("Index");
            }
            return View("Index", employee);
           
        }
       
        public EmployeeData  FindPairsWithLongestCommonWorkDuration(List<EmployeeData> employeeDataList)
        {
            Dictionary<string, int> pairDaysWorked = new Dictionary<string, int>();

            for (int i = 0; i < employeeDataList.Count - 1; i++)
            {
                for (int j = i + 1; j < employeeDataList.Count; j++)
                {
                    var employee1 = employeeDataList[i];
                    var employee2 = employeeDataList[j];

                    if (employee1.Empld != employee2.Empld && employee1.ProjectId == employee2.ProjectId)
                    {
                        DateTime startDate = employee1.DateFrom > employee2.DateFrom ? employee1.DateFrom : employee2.DateFrom;
                        DateTime endDate = GetEndDateForPair(employee1.DateTo, employee2.DateTo);
                       

                        if (endDate >= startDate)
                        {
                            string pairKey = employee1.Empld < employee2.Empld ? $"{employee1.Empld}-{employee2.Empld}" : $"{employee2.Empld}-{employee1.Empld}";

                            int daysWorked = (endDate - startDate).Days;

                            if (pairDaysWorked.ContainsKey(pairKey))
                            {
                                pairDaysWorked[pairKey] += daysWorked;
                            }
                            else
                            {
                                pairDaysWorked.Add(pairKey, daysWorked);
                            }
                        }
                    }
                }
            }
            // Output the pairs with the longest common work duration
            int maxDaysWorked = pairDaysWorked.Max(x => x.Value);
            var longestPairs = pairDaysWorked.Where(x => x.Value == maxDaysWorked);

            foreach (var pair in longestPairs)
            {
                string[] employees = pair.Key.Split('-');
                int employeeID1 = int.Parse(employees[0]);
                int employeeID2 = int.Parse(employees[1]);
                int daysWorked = pair.Value;

                
                employee.employeeInfo.Add(Tuple.Create(employeeID1, employeeID2, daysWorked));
               
            }
            return employee;
        }
        public static DateTime GetEndDateForPair(DateTime? dateTo1, DateTime? dateTo2) 
        {
            if (dateTo1 == null && dateTo2 == null)
            {
                // If both end dates are null, return today's date
                return DateTime.Today;
            }
            else if (dateTo1 == null)
            {
                // If the first end date is null, return the second end date
                return dateTo2.Value;
            }
            else if (dateTo2 == null)
            {
                // If the second end date is null, return the first end date
                return dateTo1.Value;
            }
            else
            {
                // Return the later end date between the two
                return dateTo1.Value > dateTo2.Value ? dateTo1.Value : dateTo2.Value;
            }
        }
        public List<EmployeeData> ReadEmployeeInfo(string pathCSV)
        {
            List<EmployeeData> listInfo = new List<EmployeeData>();

            if (System.IO.File.Exists(pathCSV)) {
                
                string[] lines = System.IO.File.ReadAllLines(pathCSV);
                foreach (string line in lines.Skip(1)) //skip the header
                {
                    string[] fields = line.Split(',');
                    EmployeeData employeeData = new EmployeeData
                    {
                        Empld = int.Parse(fields[0]),
                        ProjectId = int.Parse(fields[1]),
                        DateFrom = ParseDate(fields[2]),
                        DateTo = fields[3].Equals("NULL") ? DateTime.Today : ParseDate(fields[3])
                    };
                    listInfo.Add(employeeData);
                }
                return listInfo;
            }

            return listInfo;
        }

        static DateTime ParseDate(string dateString) //Ina duhet te marresh parasysh te gjitha formatet e datave per pike +
        {
            string[] formats = { "yyyy-MM-dd", "yyyy/MM/dd", "MM/dd/yyyy" }; 
            DateTime result;

            if (DateTime.TryParseExact(dateString.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            else
            {
                
                throw new Exception("Error parsing date: " + dateString);
            }
        }
        public bool ValidateFile(string file)
        {
            string extension = Path.GetExtension(file);
            if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else
            {
                return true;
            }
        } 

    }
}