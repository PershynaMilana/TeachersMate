using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachersMate
{
    public class Student
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Time { get; set; }
        public string Day { get; set; }

        public Student() {}

        public Student(string name, string phone, string time, string dayOfWeek)
        {
            Name = name;
            PhoneNumber = phone;
            Time = time;
            Day = dayOfWeek;
        }
    }
}