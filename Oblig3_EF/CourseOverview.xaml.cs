using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using Oblig3_EF.Models;

namespace Oblig3_EF
{
    /// <summary>
    /// Interaction logic for CourseOverview.xaml
    /// </summary>
    public partial class CourseOverview : Window
    {
        private readonly Dat154Context dx;
        private readonly Course course;

        public CourseOverview(Course course, Dat154Context context)
        {
            InitializeComponent();

            this.course = course;
            dx = context;

            courseInfo.Text = $"Course: {course.Coursename} ({course.Coursecode})";

            // Hent alle karakterer for dette kurset og inkluderer student
            var grades = dx.Grades
                           .Where(g => g.Coursecode == course.Coursecode)
                           .Include(g => g.Student)
                           .OrderBy(g => g.Grade1)
                           .ToList();

            studentGradesList.ItemsSource = grades;
        }
    }
}
