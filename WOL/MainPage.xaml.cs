using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace WOL;

public partial class MainPage : ContentPage
{
    private System.Timers.Timer statusCheckTimer;
    public MainPageViewModel Vm { get; set; } = new();
    public MainPage()
	{
		InitializeComponent();
        BindingContext = Vm;

        // Initialize the timer
        statusCheckTimer = new System.Timers.Timer
        {
            Interval = TimeSpan.FromSeconds(5).TotalMilliseconds,
            AutoReset = true
        };
        statusCheckTimer.Elapsed += OnStatusCheckTimerElapsed;
    }

    private void OnStatusCheckTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(new Uri("http://192.168.0.110:4001"));

                if (response.Result.IsSuccessStatusCode)
                {
                    statusCheckTimer.Stop();
                    Vm.IsLoaderOn = false;
                    Vm.IsDeviceOn = true;
                }
                else
                {
                    statusLabel.Text = "...";
                }

            }
        }
        catch (Exception)
        {
            statusLabel.Text = "Error...";
        }
    }

    private void OnBtnClicked(object sender, EventArgs e)
	{
        Application.Current.Quit();
	}

    private void OnPowerButtonClicked(object sender, EventArgs e)
	{
        if (powerButton.Source.ToString() == "File: power_on.png")
        {
            powerButton.IsVisible = false;
            Vm.IsLoaderOn = true;
            SendMagicBytes();
        }
    }

	private void SendMagicBytes()
	{
        statusLabel.Text = "Magic Byte Initiated...";
        // Convert the MAC address to bytes
        var macBytes = PhysicalAddress.Parse(userMAC.Text).GetAddressBytes();

        // Create a magic packet (6 bytes of 0xFF followed by 16 repetitions of the MAC address)
        var magicPacket = new byte[102];

        for (var i = 0; i < 6; i++)
        {
            magicPacket[i] = 0xFF;
        }

        for (var i = 6; i < 102; i += 6)
        {
            for (var j = 0; j < 6; j++)
            {
                magicPacket[i + j] = macBytes[j];
            }
        }

        // Send the magic packet to the target IP address and port (usually 9)

        try
        {
            using (var client = new UdpClient())
            {
                statusLabel.Text = "Sending...";
                client.Send(magicPacket, magicPacket.Length, IPAddress.Parse(userHost.Text).ToString(), 9);
            }
            statusLabel.Text = "Packet Sent Successful";

            statusCheckTimer.Start();
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Failed to send packet...";
            Vm.IsLoaderOn = false;
            powerButton.IsVisible = true;
        }
    }
}
