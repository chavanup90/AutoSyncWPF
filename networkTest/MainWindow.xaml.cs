using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;

namespace networkTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<DataItem> DataCollection { get; set; }
        private const int Port = 11000; // UDP Port

        public MainWindow()
        {
            InitializeComponent();
            DataCollection = new ObservableCollection<DataItem>();
            DataListView.DataContext = this;

            UpdateView();
            // Start listening for broadcasts
            Task.Run(() => ListenForBroadcasts());
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitData();

            // Send a UDP broadcast to notify other clients
            SendBroadcast("UpdateView");
        }

        private void SubmitData()
        {
            // Example: Submit data to a database
            using (SqlConnection conn = new SqlConnection("Data Source=AB-PRACHI;Initial Catalog=Sample;User ID=sa;Password=Ab65622800;"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO MyTable (Name) VALUES ('Sample Data')", conn);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateView()
        {
            Dispatcher.Invoke(() =>
            {
                DataCollection.Clear();

                foreach (var item in GetUpdatedData())
                {
                    DataCollection.Add(item);
                }
            });
        }

        private ObservableCollection<DataItem> GetUpdatedData()
        {
            var dataList = new ObservableCollection<DataItem>();

            using (SqlConnection conn = new SqlConnection("Data Source=AB-PRACHI;Initial Catalog=Sample;User ID=sa;Password=Ab65622800;"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, Name FROM MyTable", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dataList.Add(new DataItem
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
            return dataList;
        }

        private void SendBroadcast(string message)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true;
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, Port);
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                udpClient.Send(bytes, bytes.Length, ip);
            }
        }

        private void ListenForBroadcasts()
        {
            using (UdpClient udpClient = new UdpClient(Port))
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, Port);

                while (true)
                {
                    byte[] bytes = udpClient.Receive(ref ip);
                    string message = Encoding.UTF8.GetString(bytes);

                    if (message == "UpdateView")
                    {
                        UpdateView();
                    }
                }
            }
        }
    }

    public class DataItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}