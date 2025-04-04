using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Oblig3_EF.Models;

namespace Oblig3_EF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dat154Context dx = new();
        private readonly ObservableCollection<Student> Students;
        private readonly ObservableCollection<Course> Courses;


        public MainWindow()
        {
            InitializeComponent();

            //OPPGAVE 1:
            Students = dx.Students.Local.ToObservableCollection();
            dx.Students.Load();
            studentList.DataContext = Students.OrderBy(s => s.Studentname);

            studentList.MouseDoubleClick += StudentList_MouseDoubleClick;
            dx.Students.Local.CollectionChanged += Local_CollectionChanged;
            searchField.TextChanged += SearchField_TextChanged;

            //OPPGAVE 2:
            Courses = dx.Courses.Local.ToObservableCollection();
            dx.Courses.Load();
            courseList.DataContext = Courses.OrderBy(s => s.Coursename); //kunne kanskje ordered by kode istedenfor

            courseList.MouseDoubleClick += CourseList_MouseDoubleClick;
        }

 
        //METODER TIL OPPGAVE A: 
        private void SearchField_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            DoSearch_Click(DoSearch, new RoutedEventArgs());
        }

        private void Local_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DoSearch_Click(DoSearch, new RoutedEventArgs());
        }

        private void StudentList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            Student? s = studentList.SelectedItem as Student;

            if (s != null)
            {

                Editor ed = new(s)
                {
                    dx = dx
                };

                ed.Show();
            }

        }

        private void DoSearch_Click(object sender, RoutedEventArgs e)
        {
            //dx.Students.Where(s => s.Studentname.Contains(searchField.Text)).Load();
            studentList.DataContext = Students
                .Where(s => s.Studentname.Contains(searchField.Text, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(s => s.Studentname);
        }

        private void DoEdit_Click(object sender, RoutedEventArgs e)
        {

            Editor ed = new()
            {
                dx = dx
            };

            ed.Show();
        }

        //METODER TIL OPPGAVE B:
        private void CourseList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            Course? c = courseList.SelectedItem as Course;

            if (c != null)
            {
                CourseOverview co = new CourseOverview(c, dx);
                co.Show();
            }
        }

        //METODER TIL OPPGAVE C:
        private void FilterGrades_Click(object sender, RoutedEventArgs e)
        {
            if (gradeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedGrade = selectedItem.Content.ToString();

                //Lager en rangering på karakterene
                Dictionary<string, int> gradeOrder = new Dictionary<string, int>
                {
                    { "A", 1 },
                    { "B", 2 },
                    { "C", 3 },
                    { "D", 4 },
                    { "E", 5 },
                    { "F", 6 }
                 };

                if (gradeOrder.TryGetValue(selectedGrade, out int selectedValue))
                {
                    // 1. henter fra databasen
                    var allGrades = dx.Grades
                        .Include(g => g.Student)
                        .Include(g => g.CoursecodeNavigation)
                        .ToList();

                    // 2. filtrer med Dictionary
                    var filteredGrades = allGrades
                        .Where(g => gradeOrder.ContainsKey(g.Grade1) && gradeOrder[g.Grade1] <= selectedValue)
                        .OrderBy(g => gradeOrder[g.Grade1])
                        .ToList();

                    gradesListView.ItemsSource = filteredGrades;
                }
            }
        }

        //METODER TIL OPPGAVE E: 
        private void ShowFailed_Click(object sender, RoutedEventArgs e)
        {
            // Henter alle som strøk
            var failedGrades = dx.Grades
                .Include(g => g.Student)
                .Include(g => g.CoursecodeNavigation)
                .Where(g => g.Grade1 == "F")
                .OrderBy(g => g.Student.Studentname)  //sorter etter studentnavn. trenger kanskje ikke denne pga overhead men ja
                .ToList();

            failedListView.ItemsSource = failedGrades;
        }


    }
}