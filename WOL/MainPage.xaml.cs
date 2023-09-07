using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace WOL;

public partial class MainPage : ContentPage
{
    private System.Timers.Timer statusCheckTimer;
    private bool isStatusCheckRunning = false;
    public MainPageViewModel Vm { get; set; } = new();

    public MainPage()
	{
		InitializeComponent();
        BindingContext = Vm;

        // Initialize the timer
        statusCheckTimer = new System.Timers.Timer
        {
            Interval = TimeSpan.FromSeconds(2).TotalMilliseconds,
            AutoReset = true
        };
        statusCheckTimer.Elapsed += OnStatusCheckTimerElapsed;
    }

    private async void OnStatusCheckTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (isStatusCheckRunning)
        {
            return;
        }
        try
        {
            isStatusCheckRunning = true;

            Vm.CurrentStatus = "Checking Device Status...";
            await Task.Delay(400);

            using (var client = new HttpClient())
            {
                var url = $"http://{userHost.Text}:4001";
                var response = await client.GetAsync(new Uri(url));

                if (response.IsSuccessStatusCode)
                {
                    statusCheckTimer.Stop();
                    Vm.IsDeviceOn = true;
                    Vm.IsLoaderOn = false;
                    Vm.IsVisibleStatusLabel = false;
                }
                else
                {
                    Vm.CurrentStatus = "Offline";
                    await Task.Delay(300);
                }

            }
        }
        catch (Exception)
        {
            statusCheckTimer.Stop();
            Vm.CurrentStatus = "Failed to check Device Status";
            powerButton.IsVisible = true;
            Vm.IsLoaderOn = false;
            await Task.Delay(200);
        }
    }

    public async Task<bool> IsServerReachable()
    {
        try
        {
            Vm.CurrentStatus = "Pinging to server...";
            await Task.Delay(400);

            using (Ping ping = new Ping())
            {
                PingReply reply = ping.Send(IPAddress.Parse(userHost.Text));

                if (reply != null && reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        catch (PingException)
        {
            return false;
        }
    }

    private void OnBtnClicked(object sender, EventArgs e)
	{
        Application.Current.Quit();
	}

    private async void OnPowerButtonClicked(object sender, EventArgs e)
	{
        if (powerButton.Source.ToString() == "File: power_on.png")
        {
            var isServerReachable = await IsServerReachable();

            if (isServerReachable)
            {
                powerButton.IsVisible = false;
                Vm.IsLoaderOn = true;
                SendMagicBytes();
            }
            else
            {
                Vm.CurrentStatus = "Server is not reachable.";
            }

        }
    }

	private async void SendMagicBytes()
	{
        Vm.CurrentStatus = "Magic Byte Initiated...";
        await Task.Delay(400);

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
            Vm.CurrentStatus = "Sending Packet...";
            await Task.Delay(400);
            for (int i = 0; i < 5; i++)
            {
                using (var client = new UdpClient())
                {
                    client.Send(magicPacket, magicPacket.Length, IPAddress.Parse(userHost.Text).ToString(), 9);
                }
                Vm.CurrentStatus = "Packet Sent Successful";
                await Task.Delay(400);
            }

            statusCheckTimer.Start();
        }
        catch (Exception)
        {
            Vm.CurrentStatus = "Failed to send packet...";
            Vm.IsLoaderOn = false;
            powerButton.IsVisible = true;
        }
    }
}
