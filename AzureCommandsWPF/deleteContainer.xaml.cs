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
    /// Interaction logic for DeleteContainer.xaml
    /// </summary>
    public partial class DeleteContainer : Window
    {
        public string newContainerName = string.Empty;
        public bool cancelled = true;
        public DeleteContainer()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            cancelled = false;
            this.Close();           
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cancelled = true;
            this.Close();
        }
    }
}
