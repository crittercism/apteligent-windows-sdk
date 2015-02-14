using HubApp.Data;
using HubApp.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CrittercismSDK;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace HubApp
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ItemPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var item = await SampleDataSource.GetItemAsync((string)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
            Debug.WriteLine("UniqueId == "+item.UniqueId);
            Debug.WriteLine("Title == "+item.Title);
            Debug.WriteLine("Subtitle == "+item.Subtitle);
            Crittercism.LeaveBreadcrumb("UniqueId == "+item.UniqueId);
            Crittercism.LeaveBreadcrumb("Title == "+item.Title);
            Crittercism.LeaveBreadcrumb("Subtitle == "+item.Subtitle);
            if (item.UniqueId.Equals("Group-4-Item-1")) {
                Crittercism.LeaveBreadcrumb("Test Windows Store LogHandledException");
                {
                    int i=0;
                    int j=5;
                    try {
                        int k=j/i;
                    } catch (Exception ex) {
                        Crittercism.LogHandledException(ex);
                    }
                }
            } else if (item.UniqueId.Equals("Group-4-Item-2")) {
                Crittercism.LeaveBreadcrumb("Test Windows Store App Current_UnhandledException");
                int x=0;
                int y=1/x;
            } else if (item.UniqueId.Equals("Group-4-Item-3")) {
                Crittercism.LeaveBreadcrumb("Test Windows Store App LogCrash");
                {
                    try {
                        int x=0;
                        int y=1/x;
                    } catch (Exception ex) {
                        Crittercism.LogCrash(ex);
                    }
                }
            } else if (item.UniqueId.Equals("Group-4-Item-4")) {
                Crittercism.LeaveBreadcrumb("Q: Do you love Crittercism? A: YES!");
            } else if (item.UniqueId.Equals("Group-4-Item-5")) {
                Random random = new Random();
                string[] names= { "Blue Jay","Chinchilla","Chipmunk","Gerbil","Hamster","Parrot","Robin","Squirrel","Turtle" };
                string name=names[random.Next(0,names.Length)];
                Crittercism.SetUsername("Critter "+name);
            }
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}