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

namespace AzureCommandsWPF
{
    /// <summary>
    /// Interaction logic for newContainer.xaml
    /// </summary>
    public partial class newContainer : Window
    {
        public string newContainerName = string.Empty;
        public bool cancelled = false;
        public newContainer()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cancelled = true;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            newContainerName = txtNewContainerName.Text;
            this.Close();
        }
    }
}
