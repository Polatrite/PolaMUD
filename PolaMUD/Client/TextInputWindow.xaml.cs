using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MUDClientEssentials
{
    /// <summary>
    /// Interaction logic for TextInputWindow.xaml
    /// </summary>
    public partial class TextInputDialog : Window
    {
        public TextInputDialog()
        {
            InitializeComponent();
        }

        public static string PromptUser(string title, string instructions)
        {
            TextInputDialog textInputWindow = new TextInputDialog();
            textInputWindow.Title = title;
            textInputWindow.instructionsLabel.Content = instructions;
            textInputWindow.ShowDialog();
            return textInputWindow.inputBox.Text;
        }
        
        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                this.okayButton_Click(null, null);
        }

        private void okayButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
