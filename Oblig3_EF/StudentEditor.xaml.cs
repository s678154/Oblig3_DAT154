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
using Oblig3_EF.Models;

namespace Oblig3_EF
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {

        public Dat154Context? dx { get; set; }

        public Editor()
        {
            InitializeComponent();
        }

        public Editor(Student s)
        {
            InitializeComponent();

            sid.Text = s.Id.ToString();
            sname.Text = s.Studentname;
            sage.Text = s.Studentage.ToString();
        }

        private void bAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Student s = new()
                {
                    Id = int.Parse(sid.Text),
                    Studentname = sname.Text,
                    Studentage = int.Parse(sage.Text)
                };

                dx.Students.Add(s);
                dx.SaveChanges();

                errorText.Text = sid.Text = sage.Text = sname.Text = "";

            }
            catch (Exception ex)
            {
                errorText.Text = ex.Message + "\n" + ex.InnerException?.Message;
            }

        }

        private void bDel_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Student s = dx.Students.Where(s => s.Id.Equals(int.Parse(sid.Text))).First();

                dx.Students.Remove(s);
                dx.SaveChanges();

                errorText.Text = sid.Text = sage.Text = sname.Text = "";

            }
            catch (Exception ex)
            {
                errorText.Text = ex.Message + "\n" + ex.InnerException?.Message;
            }


        }

        private void bUpdate_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Student s = dx.Students.Where(s => s.Id.Equals(int.Parse(sid.Text))).First();

                if (sage.Text.Length > 0) s.Studentage = int.Parse(sage.Text);
                if (sname.Text.Length > 0) s.Studentname = sname.Text;


                dx.SaveChanges();

                errorText.Text = sid.Text = sage.Text = sname.Text = "";

            }
            catch (Exception ex)
            {
                errorText.Text = ex.Message + "\n" + ex.InnerException?.Message;
            }
        }
    }
}
