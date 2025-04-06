using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Oblig3_EF.Models;

namespace Oblig3_EF
{
    /// <summary>
    /// Hver oppgave har sin egen TabItem i MainWindow.xaml. Oppgave 1: Studenter, Oppg 2: Courses, Oppg 3: Grades, Oppg 4: Failed Students & Oppg 5: Add/Rem Participants
    /// I tillegg har oppgave 1 ett ekstra window - StudentEditor.xaml og tilhørende cs kode.
    /// Oppgave 2 har også et ekstra et vindu - CourseOverview.xaml med tilhørende cs kode. 
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

            // OPPGAVE 5: fylle inn ComboBox for å legge til/fjerne studentdeltakelse på fag
            PopulateManageCourseComboBox();
        }

 
        //METODER TIL OPPGAVE 1: 
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

        //METODER TIL OPPGAVE 2:
        private void CourseList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            Course? c = courseList.SelectedItem as Course;

            if (c != null)
            {
                CourseOverview co = new CourseOverview(c, dx);
                co.Show();
            }
        }

        //METODER TIL OPPGAVE 3:
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

        //METODER TIL OPPGAVE 4: 
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

        //METODER TIL OPPGAVE 5: 
        private void PopulateManageCourseComboBox() // Metode for å fylle opp ComboBox med kurs
        {
            manageCourseComboBox.ItemsSource = Courses.OrderBy(c => c.Coursename);
        }

        private void ManageCourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Kalles når brukeren velger et kurs i ComboBoxen
        {
            LoadParticipants();
        }

        private void LoadParticipants_Click(object sender, RoutedEventArgs e) // Henter deltakere for det valgte kurset
        {
            LoadParticipants();
        }

        private void LoadParticipants()
        {
            if (manageCourseComboBox.SelectedItem is Course selectedCourse)
            {
                var participants = dx.Grades
                    .Include(g => g.Student)
                    .Where(g => g.Coursecode == selectedCourse.Coursecode)
                    .ToList();
                participantsListView.ItemsSource = participants;
            }
        }

        private void AddStudentToCourse_Click(object sender, RoutedEventArgs e)
        {
            if (manageCourseComboBox.SelectedItem is Course selectedCourse)
            {
               
                if (int.TryParse(addStudentIdTextBox.Text, out int studentId))  // hent student-ID fra tekstboksen
                {
                    var student = dx.Students.FirstOrDefault(s => s.Id == studentId); // finn student i databasen
                    if (student != null)
                    {
                        if (addGradeComboBox.SelectedItem is ComboBoxItem gradeItem)  // hent valgt grade fra Combobox
                        {
                            string gradeValue = gradeItem.Content.ToString();
                            var newGrade = new Grade //oppretter ny grade på studenten
                            {
                                Studentid = studentId,
                                Coursecode = selectedCourse.Coursecode,
                                Grade1 = gradeValue
                            };
                            dx.Grades.Add(newGrade);
                            dx.SaveChanges();

                            LoadParticipants();

                            addStudentIdTextBox.Clear(); //rydde i input-datafelt
                            addGradeComboBox.SelectedIndex = -1;
                        }
                        else //grade ikke valgt
                        {
                            MessageBox.Show("Vennligst velg en grade."); 
                        }
                    }
                    else
                    {
                        MessageBox.Show("Studenten ble ikke funnet.");
                    }
                }
                else
                {
                    MessageBox.Show("Vennligst skriv inn et gyldig student-ID.");
                }
            }
            else
            {
                MessageBox.Show("Vennligst velg et kurs først.");
            }
        }

        private void RemoveStudentFromCourse_Click(object sender, RoutedEventArgs e)
        {
            if (participantsListView.SelectedItem is Grade selectedGrade)
            {
                dx.Grades.Remove(selectedGrade);
                dx.SaveChanges();
                LoadParticipants();
            }
            else
            {
                MessageBox.Show("Vennligst velg en deltaker å fjerne.");
            }
        }

    }
}