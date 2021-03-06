﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using SirenOfShame.Uwp.Ui.Models;
using SirenOfShame.Uwp.Ui.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SirenOfShame.Uwp.Ui.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewUserPage
    {
        public ViewUserPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var person = e.Parameter as PersonDto;
            if (person == null) return;
            DataContext = new ViewUserViewModel(person);
            Title.Text = person.DisplayName;
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
