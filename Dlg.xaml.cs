using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyPropertyDlg3
{
    /// <summary>
    /// Lógica interna para Dlg.xaml
    /// </summary>
    public partial class Dlg : Window
    {
        public static Options options = new Options();
        public Dlg()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            options.ball_radius = float.Parse(cbBallRadius.Text);
            options.hole_radius = float.Parse(cbHoleRadius.Text);
            options.wall_friction = int.Parse(cbWallFriction.Text);
            options.table_friction = int.Parse(cbTableFriction.Text);
            options.Save();
            DialogResult = true;
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            options.Load();     
            
        }
        // set combos sels

        private void cbBallRadius_Loaded(object sender, RoutedEventArgs e)
        {
            cbBallRadius.Text = options.ball_radius.ToString();
        }

        private void cbHoleRadius_Loaded(object sender, RoutedEventArgs e)
        {
            cbHoleRadius.Text = options.hole_radius.ToString();
        }

        private void cbTableFriction_Loaded(object sender, RoutedEventArgs e)
        {
            cbTableFriction.Text = options.table_friction.ToString();
        }

        private void cbWallFriction_Loaded(object sender, RoutedEventArgs e)
        {
            cbWallFriction.Text = options.wall_friction.ToString();
        }
    }

    public struct Options
    {
        public Options() { }
        public float ball_radius { get; set; } = 10f;
        public float hole_radius { get; set; } = 25f;
        public int table_friction { get; set; } = 5;
        public  int wall_friction { get; set; } = 10;

        public void Save()
        {
            string? path = System.IO.Path.ChangeExtension(Environment.ProcessPath, ".json");
            if (path != null)
            {
                string json;
                File.WriteAllText(path,json = JsonSerializer.Serialize<Options>(this));
            }
        }
        public void Load()
        {
            string? path = System.IO.Path.ChangeExtension(Environment.ProcessPath, ".json");
            if (path != null)
            {
                try
                {
                    string? json = File.ReadAllText(path);
                    if (json != null)
                    {
                        Options options;
                        options = JsonSerializer.Deserialize<Options>(json);
                        ball_radius = options.ball_radius;
                        hole_radius = options.hole_radius;
                        wall_friction = options.wall_friction;
                        table_friction = options.table_friction;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
