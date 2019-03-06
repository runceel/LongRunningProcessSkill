using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LongRunningProcessSkill.UWP
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private static string GetRaiseEventEndpoint(string instanceId, string eventName) => 
            $"{Secrets.AzureFunctionsUrl}/runtime/webhooks/durabletask/instances/{instanceId}/raiseEvent/{eventName}?code={Secrets.SystemKey}";
        private ObservableCollection<EventLog> EventLogs { get; } = new ObservableCollection<EventLog>();
        private CloudTable _eventLogTable;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private bool SetProperty<T>(ref T field, T value, Action callbackAfterChanged = null, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            callbackAfterChanged?.Invoke();
            return true;
        }
        #endregion

        private EventLog _selectedEventLog;
        public EventLog SelectedEventLog
        {
            get { return _selectedEventLog; }
            set { SetProperty(ref _selectedEventLog, value); }
        }

        private string _sendMessage;
        public string SendMessage
        {
            get { return _sendMessage; }
            set { SetProperty(ref _sendMessage, value); }
        }

        public static bool EvalEnabledOfSendButton(string sendMessage, EventLog selectedEventLog) => 
            !string.IsNullOrWhiteSpace(sendMessage) && selectedEventLog != null;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            StartPinger();
            await RefreshDataAsync();
        }

        private async void StartPinger()
        {
            var client = new HttpClient();
            while (true)
            {
                await client.GetAsync("https://longrunningprocessskill20190303121855.azurewebsites.net/api/Wakeup").ConfigureAwait(false);
                await Task.Delay(5 * 60 * 1000);
            }
        }

        private async Task RefreshDataAsync()
        {
            if (_eventLogTable == null)
            {
                var account = CloudStorageAccount.Parse(Secrets.StorageConnectionString);
                var tableClient = account.CreateCloudTableClient();
                _eventLogTable = tableClient.GetTableReference("EventLog");
                await _eventLogTable.CreateIfNotExistsAsync();
            }

            EventLogs.Clear();
            var buffer = new List<EventLog>();
            TableContinuationToken token = null;
            var query = new TableQuery<EventLog>();
            do
            {
                var result = await _eventLogTable.ExecuteQuerySegmentedAsync(query, token);
                token = result.ContinuationToken;
                buffer.AddRange(result.Results);
            }
            while (token != null);

            foreach (var x in buffer.OrderBy(x => x.Timestamp))
            {
                EventLogs.Add(x);
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(GetRaiseEventEndpoint(SelectedEventLog.RowKey, "done"), 
                    new StringContent(JsonConvert.SerializeObject(SendMessage), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    SendMessage = "";
                    await new MessageDialog("メッセージを送信しました。").ShowAsync();
                }
                else
                {
                    await new MessageDialog("メッセージの送信に失敗しました。").ShowAsync();
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshDataAsync();
        }
    }
}
